#!/usr/bin/env python3
"""
tests/test_vault.py — Unit tests for core.vault.VaultManager.
"""

from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import pytest
import tempfile

from core.vault import VaultManager, VaultLockedError, VaultCorruptError


def make_vault(tmp_dir: str, profile_name: str = "test") -> VaultManager:
    """Create a VaultManager backed by a temporary directory."""
    return VaultManager(profile_name, vault_dir=Path(tmp_dir))


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------


def test_vault_unlock_new_vault():
    """Unlocking a brand-new (non-existent) vault succeeds and reports unlocked."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        result = vault.unlock("my-master-password")
        assert result is True
        assert vault.is_locked() is False


def test_vault_lock_unlock():
    """Lock/unlock cycle works correctly."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        vault.unlock("secret-password")
        assert vault.is_locked() is False

        vault.lock()
        assert vault.is_locked() is True

        # Should be able to unlock again with correct password
        result = vault.unlock("secret-password")
        assert result is True
        assert vault.is_locked() is False


def test_vault_set_get():
    """Set a key and retrieve the same value back."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        vault.unlock("password123")
        vault.set("DB_PASSWORD", "supersecret")
        assert vault.get("DB_PASSWORD") == "supersecret"


def test_vault_locked_get_raises():
    """Accessing a key on a locked vault raises VaultLockedError."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        vault.unlock("password")
        vault.set("KEY", "value")
        vault.lock()

        with pytest.raises(VaultLockedError):
            vault.get("KEY")


def test_vault_delete():
    """Deleting a key removes it from list_keys()."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        vault.unlock("password")
        vault.set("MY_KEY", "my-value")
        assert "MY_KEY" in vault.list_keys()

        vault.delete("MY_KEY")
        assert "MY_KEY" not in vault.list_keys()


def test_vault_rotate():
    """Rotating a key produces a different stored value."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        vault.unlock("password")
        vault.set("JWT_SECRET", "old-value-abcdef")
        old_value = vault.get("JWT_SECRET")

        new_value = vault.rotate("JWT_SECRET")
        assert new_value != old_value
        assert vault.get("JWT_SECRET") == new_value
        assert len(new_value) == 24


def test_vault_export_import_bundle():
    """Export vault bundle, import into a fresh vault, and verify secrets are accessible."""
    with tempfile.TemporaryDirectory() as tmp_a, tempfile.TemporaryDirectory() as tmp_b:
        # Source vault
        vault_a = make_vault(tmp_a, "profile-a")
        vault_a.unlock("master-a")
        vault_a.set("SECRET_KEY", "hello-world-secret")
        vault_a.set("API_TOKEN", "tok_abc123")

        bundle = vault_a.export_bundle("bundle-password")
        assert bundle.startswith(b"CRMVAULT1")

        # Target vault (different profile, different dir)
        vault_b = make_vault(tmp_b, "profile-b")
        vault_b.unlock("master-b")
        vault_b.import_bundle(bundle, "bundle-password")

        assert vault_b.get("SECRET_KEY") == "hello-world-secret"
        assert vault_b.get("API_TOKEN") == "tok_abc123"


def test_vault_wrong_master_password():
    """Providing the wrong master password returns False, not an exception."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        # First unlock to create the vault with a known password and some data
        vault.unlock("correct-password")
        vault.set("SOMETHING", "value")
        vault.lock()

        # Try incorrect password
        result = vault.unlock("wrong-password")
        assert result is False
        assert vault.is_locked() is True


def test_generate_password_length():
    """generate_password(16) returns exactly 16 characters."""
    with tempfile.TemporaryDirectory() as tmp:
        vault = make_vault(tmp)
        vault.unlock("pw")
        pwd = vault.generate_password(16)
        assert isinstance(pwd, str)
        assert len(pwd) == 16
