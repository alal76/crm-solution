#!/usr/bin/env python3
"""
core/vault.py — AES-256-GCM encrypted secrets vault for the CRM Deployment Tool.

Secrets are stored per-profile in ~/.crm-cdt/secrets/<profile_name>.vault as
a JSON file.  The master password is never persisted on disk; it is used to
derive a 32-byte key via PBKDF2-HMAC-SHA256.  Each individual secret value is
independently encrypted with its own random 96-bit nonce so that compromise of
one ciphertext does not help an attacker with the others.

Vault JSON schema (version 1):
    {
        "version": 1,
        "salt": "<16-byte hex>",
        "entries": {
            "<key>": {
                "nonce": "<12-byte hex>",
                "ciphertext": "<hex>"        # encrypted value; TAG appended by AESGCM
            },
            ...
        }
    }

Bundle format (export_bundle / import_bundle):
    b"CRMVAULT1" + uint32_be(len(payload_json)) + payload_json_bytes
    where payload_json = {"salt": hex, "nonce": hex, "ciphertext": hex}
    and the ciphertext is the full vault entries JSON encrypted with AES-GCM
    using a key derived from the bundle_password + salt.
"""

from __future__ import annotations

import json
import os
import secrets
import string
from pathlib import Path
from typing import Optional

from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

# ---------------------------------------------------------------------------
# Custom exceptions
# ---------------------------------------------------------------------------


class VaultLockedError(Exception):
    """Raised when a vault operation is attempted while the vault is locked."""


class VaultCorruptError(Exception):
    """Raised when the vault file cannot be decrypted (wrong password or corruption)."""


# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

_VAULT_VERSION = 1
_KDF_ITERATIONS = 100_000
_SALT_BYTES = 16
_NONCE_BYTES = 12
_KEY_BYTES = 32
_BUNDLE_MAGIC = b"CRMVAULT1"


# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------


def _derive_key(password: str, salt_hex: str) -> bytes:
    """Derive a 32-byte AES key from *password* and *salt_hex* via PBKDF2-HMAC-SHA256."""
    salt = bytes.fromhex(salt_hex)
    kdf = PBKDF2HMAC(
        algorithm=hashes.SHA256(),
        length=_KEY_BYTES,
        salt=salt,
        iterations=_KDF_ITERATIONS,
    )
    return kdf.derive(password.encode("utf-8"))


def _encrypt(key: bytes, plaintext: str) -> dict:
    """Encrypt *plaintext* with *key* using AES-256-GCM.

    Returns a dict with ``nonce`` and ``ciphertext`` as hex strings.
    The AESGCM ciphertext already includes the 16-byte authentication tag
    appended at the end (cryptography library convention).
    """
    nonce = os.urandom(_NONCE_BYTES)
    aesgcm = AESGCM(key)
    ciphertext = aesgcm.encrypt(nonce, plaintext.encode("utf-8"), None)
    return {
        "nonce": nonce.hex(),
        "ciphertext": ciphertext.hex(),
    }


def _decrypt(key: bytes, entry: dict) -> str:
    """Decrypt an entry dict produced by :func:`_encrypt`.

    Raises :class:`VaultCorruptError` on authentication failure.
    """
    try:
        nonce = bytes.fromhex(entry["nonce"])
        ciphertext = bytes.fromhex(entry["ciphertext"])
        aesgcm = AESGCM(key)
        plaintext_bytes = aesgcm.decrypt(nonce, ciphertext, None)
        return plaintext_bytes.decode("utf-8")
    except Exception as exc:
        raise VaultCorruptError("Failed to decrypt entry — wrong password or corrupted vault.") from exc


def encrypt_data(data: bytes, password: str) -> bytes:
    """Encrypt arbitrary *data* bytes with *password* using AES-256-GCM + PBKDF2.

    Returns a self-contained blob:
        salt (32 hex chars, 16 bytes) | nonce (24 hex chars, 12 bytes) | ciphertext (hex)

    The blob is pure ASCII hex so it can be stored as text if needed.
    """
    salt_hex = secrets.token_hex(_SALT_BYTES)
    key = _derive_key(password, salt_hex)
    nonce = os.urandom(_NONCE_BYTES)
    aesgcm = AESGCM(key)
    ct = aesgcm.encrypt(nonce, data, None)
    return (salt_hex + nonce.hex() + ct.hex()).encode("ascii")


def decrypt_data(blob: bytes, password: str) -> bytes:
    """Decrypt a blob produced by :func:`encrypt_data`.

    Raises :class:`VaultCorruptError` on failure.
    """
    try:
        text = blob.decode("ascii")
        salt_hex = text[: _SALT_BYTES * 2]
        nonce_hex = text[_SALT_BYTES * 2 : _SALT_BYTES * 2 + _NONCE_BYTES * 2]
        ct_hex = text[_SALT_BYTES * 2 + _NONCE_BYTES * 2 :]
        key = _derive_key(password, salt_hex)
        aesgcm = AESGCM(key)
        return aesgcm.decrypt(bytes.fromhex(nonce_hex), bytes.fromhex(ct_hex), None)
    except VaultCorruptError:
        raise
    except Exception as exc:
        raise VaultCorruptError("Failed to decrypt data — wrong password or corrupted blob.") from exc


# ---------------------------------------------------------------------------
# VaultManager
# ---------------------------------------------------------------------------


class VaultManager:
    """AES-256-GCM encrypted secret store for a single deployment profile.

    Usage::

        vault = VaultManager("my-profile")
        vault.unlock("my-master-password")
        vault.set("DB_PASSWORD", "supersecret")
        print(vault.get("DB_PASSWORD"))  # "supersecret"
        vault.lock()

    Parameters
    ----------
    profile_name:
        The deployment profile name.  Determines the vault file path:
        ``~/.crm-cdt/secrets/<profile_name>.vault``.
    vault_dir:
        Override the directory that contains vault files.  Used in tests.
    """

    def __init__(self, profile_name: str, vault_dir: Optional[Path] = None) -> None:
        self._profile_name = profile_name
        if vault_dir is not None:
            self._vault_path = Path(vault_dir) / f"{profile_name}.vault"
        else:
            self._vault_path = Path.home() / ".crm-cdt" / "secrets" / f"{profile_name}.vault"
        self._vault_path.parent.mkdir(parents=True, exist_ok=True)

        self._key: Optional[bytes] = None          # None ⟹ locked
        self._data: dict = {}                      # in-memory vault state

    # ------------------------------------------------------------------
    # Lock / unlock
    # ------------------------------------------------------------------

    def unlock(self, master_password: str) -> bool:
        """Unlock the vault with *master_password*.

        On a **new** vault (file does not yet exist) the vault is initialised
        with a fresh salt and empty entries, then persisted.

        On an **existing** vault the password is verified by attempting to
        decrypt the first entry (if any exist) or by doing a round-trip
        encrypt/decrypt with a sentinel value.  A wrong password causes this
        method to return ``False``; a structurally corrupt file raises
        :class:`VaultCorruptError`.

        Returns
        -------
        bool
            ``True`` on success, ``False`` if the password is incorrect.
        """
        if self._vault_path.exists():
            try:
                raw = self._vault_path.read_text(encoding="utf-8")
                vault_json = json.loads(raw)
            except (OSError, json.JSONDecodeError) as exc:
                raise VaultCorruptError(f"Vault file is unreadable or corrupt: {exc}") from exc

            salt_hex = vault_json.get("salt")
            if not salt_hex:
                raise VaultCorruptError("Vault file missing 'salt' field.")

            key = _derive_key(master_password, salt_hex)
            entries = vault_json.get("entries", {})

            if entries:
                # Verify password by decrypting the first entry
                first_entry = next(iter(entries.values()))
                try:
                    _decrypt(key, first_entry)
                except VaultCorruptError:
                    return False
            # else: empty vault — trust the salt; will be verifiable on first set()

            self._key = key
            self._data = vault_json
            return True
        else:
            # New vault — initialise
            salt_hex = os.urandom(_SALT_BYTES).hex()
            self._key = _derive_key(master_password, salt_hex)
            self._data = {
                "version": _VAULT_VERSION,
                "salt": salt_hex,
                "entries": {},
            }
            self._save()
            return True

    def is_locked(self) -> bool:
        """Return ``True`` if the vault is currently locked (no key in memory)."""
        return self._key is None

    def lock(self) -> None:
        """Clear the in-memory key, locking the vault."""
        self._key = None
        self._data = {}

    # ------------------------------------------------------------------
    # Secret CRUD
    # ------------------------------------------------------------------

    def set(self, key: str, value: str, ephemeral: bool = False) -> None:
        """Store *value* under *key*.

        Parameters
        ----------
        key:
            Logical name (e.g. ``"DB_PASSWORD"``).
        value:
            Plaintext secret value.
        ephemeral:
            If ``True`` the secret is **not** written to disk (in-memory only
            for the lifetime of this session).
        """
        self._require_unlocked()
        entry = _encrypt(self._key, value)
        self._data.setdefault("entries", {})[key] = entry
        if not ephemeral:
            self._save()

    def get(self, key: str) -> str:
        """Retrieve the plaintext value for *key*.

        Raises
        ------
        VaultLockedError
            If the vault is locked.
        KeyError
            If *key* does not exist.
        """
        self._require_unlocked()
        entries = self._data.get("entries", {})
        if key not in entries:
            raise KeyError(f"Secret key '{key}' not found in vault.")
        return _decrypt(self._key, entries[key])

    def delete(self, key: str) -> None:
        """Remove *key* from the vault.

        Raises
        ------
        VaultLockedError
            If the vault is locked.
        KeyError
            If *key* does not exist.
        """
        self._require_unlocked()
        entries = self._data.get("entries", {})
        if key not in entries:
            raise KeyError(f"Secret key '{key}' not found in vault.")
        del entries[key]
        self._save()

    def list_keys(self) -> list[str]:
        """Return the list of stored key names.

        Raises
        ------
        VaultLockedError
            If the vault is locked.
        """
        self._require_unlocked()
        return list(self._data.get("entries", {}).keys())

    def rotate(self, key: str) -> str:
        """Generate a new 24-character secure token for *key*, store it, and return it.

        Raises
        ------
        VaultLockedError
            If the vault is locked.
        KeyError
            If *key* does not exist.
        """
        self._require_unlocked()
        if key not in self._data.get("entries", {}):
            raise KeyError(f"Secret key '{key}' not found in vault.")
        new_value = self.generate_password(24)
        self.set(key, new_value)
        return new_value

    # ------------------------------------------------------------------
    # Bundle export / import
    # ------------------------------------------------------------------

    def export_bundle(self, bundle_password: str) -> bytes:
        """Serialise and re-encrypt the entire vault as a portable bundle.

        The bundle can be imported into a different vault instance via
        :meth:`import_bundle`.

        Format::

            b"CRMVAULT1" + uint32_be(len(payload_json_bytes)) + payload_json_bytes

        where *payload_json_bytes* is the UTF-8 encoding of::

            {"salt": "<hex>", "nonce": "<hex>", "ciphertext": "<hex>"}

        and the *ciphertext* is the AES-GCM encryption of the plaintext entries
        JSON using a key derived from *bundle_password* + *salt*.

        Raises
        ------
        VaultLockedError
            If the vault is locked.
        """
        self._require_unlocked()

        # Decrypt all entries to plaintext first
        entries = self._data.get("entries", {})
        plaintext_entries: dict[str, str] = {}
        for k, enc in entries.items():
            plaintext_entries[k] = _decrypt(self._key, enc)

        plaintext_json = json.dumps(plaintext_entries, ensure_ascii=False).encode("utf-8")

        # Encrypt with bundle password
        salt_hex = os.urandom(_SALT_BYTES).hex()
        bundle_key = _derive_key(bundle_password, salt_hex)
        enc = _encrypt(bundle_key, plaintext_json.decode("utf-8"))

        payload = {
            "salt": salt_hex,
            "nonce": enc["nonce"],
            "ciphertext": enc["ciphertext"],
        }
        payload_bytes = json.dumps(payload).encode("utf-8")
        return _BUNDLE_MAGIC + len(payload_bytes).to_bytes(4, "big") + payload_bytes

    def import_bundle(self, data: bytes, bundle_password: str) -> None:
        """Merge entries from a bundle created by :meth:`export_bundle`.

        Existing keys in this vault are overwritten by bundle values if they
        have the same name.

        Raises
        ------
        VaultLockedError
            If the vault is locked.
        VaultCorruptError
            If *data* is malformed or *bundle_password* is wrong.
        """
        self._require_unlocked()

        if not data.startswith(_BUNDLE_MAGIC):
            raise VaultCorruptError("Bundle magic bytes missing — not a valid CRM vault bundle.")

        offset = len(_BUNDLE_MAGIC)
        try:
            payload_len = int.from_bytes(data[offset: offset + 4], "big")
            offset += 4
            payload_bytes = data[offset: offset + payload_len]
            payload = json.loads(payload_bytes.decode("utf-8"))
        except Exception as exc:
            raise VaultCorruptError(f"Bundle header corrupt: {exc}") from exc

        bundle_key = _derive_key(bundle_password, payload["salt"])
        try:
            plaintext_json = _decrypt(bundle_key, {
                "nonce": payload["nonce"],
                "ciphertext": payload["ciphertext"],
            })
        except VaultCorruptError as exc:
            raise VaultCorruptError("Wrong bundle password or corrupted bundle data.") from exc

        try:
            entries: dict[str, str] = json.loads(plaintext_json)
        except json.JSONDecodeError as exc:
            raise VaultCorruptError(f"Bundle entries JSON corrupt: {exc}") from exc

        for k, v in entries.items():
            self.set(k, v)

    # ------------------------------------------------------------------
    # Utility
    # ------------------------------------------------------------------

    def generate_password(self, length: int = 16) -> str:
        """Generate a cryptographically secure random password.

        The generated password contains uppercase, lowercase, digits, and
        punctuation characters (excluding ambiguous chars ``'"\\``).

        Parameters
        ----------
        length:
            Desired password length (default 16).

        Returns
        -------
        str
            A random password of *length* characters.
        """
        alphabet = (
            string.ascii_uppercase
            + string.ascii_lowercase
            + string.digits
            + "!@#$%^&*()-_=+[]{}|;:,.<>/?"
        )
        return "".join(secrets.choice(alphabet) for _ in range(length))

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _require_unlocked(self) -> None:
        if self._key is None:
            raise VaultLockedError("Vault is locked. Call unlock() first.")

    def _save(self) -> None:
        """Persist the current vault state to disk."""
        tmp_path = self._vault_path.with_suffix(".vault.tmp")
        tmp_path.write_text(json.dumps(self._data, indent=2), encoding="utf-8")
        tmp_path.replace(self._vault_path)
