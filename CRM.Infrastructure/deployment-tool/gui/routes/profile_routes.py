#!/usr/bin/env python3
"""Profile management routes for CDT wizard."""

import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from flask import Blueprint, request, jsonify
from core.profile import ProfileManager, RunHistoryManager, ProfileNotFoundError, ProfileExistsError
from core.vault import VaultManager, VaultLockedError
import json
import copy
import secrets
import string

profile_bp = Blueprint("profile", __name__)
profile_manager = ProfileManager()
history_manager = RunHistoryManager()

# Shared vault instance (keyed by profile name)
_vault_cache: dict = {}


def get_vault(profile_name: str) -> VaultManager:
    if profile_name not in _vault_cache:
        _vault_cache[profile_name] = VaultManager(profile_name)
    return _vault_cache[profile_name]


# ---------------------------------------------------------------------------
# Profile routes
# ---------------------------------------------------------------------------


@profile_bp.route("/api/profiles", methods=["GET"])
def list_profiles():
    """Return a list of all saved profiles."""
    try:
        profiles = profile_manager.list_profiles()
        return jsonify({"profiles": profiles})
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/templates", methods=["GET"])
def get_templates():
    """Return the list of quick-start template profiles."""
    try:
        templates = profile_manager.get_templates()
        return jsonify({"templates": templates})
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/<name>", methods=["GET"])
def load_profile(name: str):
    """Load a single profile by name."""
    try:
        data = profile_manager.load(name)
        return jsonify(data)
    except ProfileNotFoundError:
        return jsonify({"error": f"Profile '{name}' not found."}), 404
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles", methods=["POST"])
def create_profile():
    """Create or overwrite a profile.  Body: ``{name, data}``."""
    body = request.json or {}
    name = body.get("name")
    data = body.get("data", {})
    if not name:
        return jsonify({"error": "'name' is required."}), 400
    try:
        profile_manager.save(name, data)
        return jsonify({"saved": name}), 201
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/<name>", methods=["PUT"])
def update_profile(name: str):
    """Replace a profile's data wholesale."""
    try:
        profile_manager.save(name, request.json or {})
        return jsonify({"saved": name})
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/<name>", methods=["DELETE"])
def delete_profile(name: str):
    """Delete a profile by name."""
    try:
        profile_manager.delete(name)
        return jsonify({"deleted": name})
    except ProfileNotFoundError:
        return jsonify({"error": f"Profile '{name}' not found."}), 404
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/import", methods=["POST"])
def import_profile():
    """Import a profile from a JSON string.  Body: ``{json_str, overwrite}``."""
    body = request.json or {}
    json_str = body.get("json_str", "")
    overwrite = bool(body.get("overwrite", False))
    if not json_str:
        return jsonify({"error": "'json_str' is required."}), 400
    try:
        name = profile_manager.import_profile(json_str, overwrite=overwrite)
        return jsonify({"imported": name}), 201
    except ProfileExistsError as exc:
        return jsonify({"error": str(exc)}), 409
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/compare", methods=["GET"])
def compare_profiles():
    """Compare two profiles.  Query params: ``?a=<name>&b=<name>``."""
    name_a = request.args.get("a", "")
    name_b = request.args.get("b", "")
    if not name_a or not name_b:
        return jsonify({"error": "Query params 'a' and 'b' are required."}), 400
    try:
        diff = profile_manager.compare(name_a, name_b)
        return jsonify({"diff": diff})
    except ProfileNotFoundError as exc:
        return jsonify({"error": str(exc)}), 404
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# Vault routes
# ---------------------------------------------------------------------------


@profile_bp.route("/api/vault/status", methods=["GET"])
def vault_status():
    """Return vault lock status.  Query param: ``?profile=<name>``."""
    profile_name = request.args.get("profile", "")
    if not profile_name:
        return jsonify({"error": "'profile' query param is required."}), 400
    vault = get_vault(profile_name)
    locked = vault.is_locked()
    result: dict = {"locked": locked}
    if not locked:
        result["keys"] = vault.list_keys()
    return jsonify(result)


@profile_bp.route("/api/vault/unlock", methods=["POST"])
def vault_unlock():
    """Unlock a vault.  Body: ``{profile_name, master_password}``."""
    body = request.json or {}
    profile_name = body.get("profile_name", "")
    master_password = body.get("master_password", "")
    if not profile_name or not master_password:
        return jsonify({"error": "'profile_name' and 'master_password' are required."}), 400
    try:
        vault = get_vault(profile_name)
        success = vault.unlock(master_password)
        return jsonify({"unlocked": success})
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/vault/secret", methods=["POST"])
def vault_set_secret():
    """Store a secret.  Body: ``{profile_name, key, value}``."""
    body = request.json or {}
    profile_name = body.get("profile_name", "")
    key = body.get("key", "")
    value = body.get("value", "")
    if not profile_name or not key:
        return jsonify({"error": "'profile_name' and 'key' are required."}), 400
    try:
        vault = get_vault(profile_name)
        vault.set(key, value)
        return jsonify({"stored": key})
    except VaultLockedError:
        return jsonify({"error": "Vault is locked. Unlock it first."}), 403
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/vault/rotate/<key>", methods=["GET"])
def vault_rotate(key: str):
    """Rotate a secret.  Query param: ``?profile=<name>``."""
    profile_name = request.args.get("profile", "")
    if not profile_name:
        return jsonify({"error": "'profile' query param is required."}), 400
    try:
        vault = get_vault(profile_name)
        new_value = vault.rotate(key)
        hint = new_value[:4] + "****"
        return jsonify({"new_value_hint": hint})
    except VaultLockedError:
        return jsonify({"error": "Vault is locked. Unlock it first."}), 403
    except KeyError:
        return jsonify({"error": f"Key '{key}' not found in vault."}), 404
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/generate-password", methods=["GET"])
def generate_password():
    """Return a random 16-character password."""
    try:
        # Use vault utility if available, else fallback
        alphabet = (
            string.ascii_uppercase
            + string.ascii_lowercase
            + string.digits
            + "!@#$%^&*()-_=+[]{}|;:,.<>/?"
        )
        password = "".join(secrets.choice(alphabet) for _ in range(16))
        return jsonify({"password": password})
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# Active-profile routes
# ---------------------------------------------------------------------------

_ACTIVE_PROFILE_FILE = Path.home() / ".crm-cdt" / "last_profile.json"
_ACTIVE_META_FILE = Path.home() / ".crm-cdt" / "active_profile_name.txt"

# Keys whose values should be blanked on export
_CREDENTIAL_KEYS = {
    "db_password", "admin_password", "redis_password", "jwt_secret",
    "api_key", "secret_key", "master_password", "private_key",
    "client_secret", "access_key", "secret", "token", "password",
}


def _strip_credentials(data: dict) -> dict:
    """Deep-copy *data* blanking any value whose key contains a credential keyword."""
    cleaned = copy.deepcopy(data)

    def _clean(obj):
        if isinstance(obj, dict):
            for k in obj.keys():
                if any(ck in k.lower() for ck in _CREDENTIAL_KEYS):
                    obj[k] = ""
                else:
                    _clean(obj[k])
        elif isinstance(obj, list):
            for item in obj:
                _clean(item)

    _clean(cleaned)
    return cleaned


@profile_bp.route("/api/profiles/active", methods=["GET"])
def get_active_profile():
    """Return the name of the currently-active deployment profile."""
    name = None
    if _ACTIVE_META_FILE.exists():
        try:
            name = _ACTIVE_META_FILE.read_text(encoding="utf-8").strip() or None
        except OSError:
            name = None
    return jsonify({"active_profile": name})


@profile_bp.route("/api/profiles/<name>/activate", methods=["POST"])
def activate_profile(name: str):
    """Set *name* as the active deployment profile."""
    try:
        data = profile_manager.load(name)
    except ProfileNotFoundError:
        return jsonify({"error": f"Profile '{name}' not found."}), 404
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500
    try:
        _ACTIVE_PROFILE_FILE.parent.mkdir(parents=True, exist_ok=True)
        _ACTIVE_PROFILE_FILE.write_text(json.dumps(data, indent=2), encoding="utf-8")
        _ACTIVE_META_FILE.write_text(name, encoding="utf-8")
        return jsonify({"activated": name})
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@profile_bp.route("/api/profiles/<name>/export", methods=["GET"])
def export_profile_sanitized(name: str):
    """Return a credential-free snapshot of profile *name* for sharing."""
    try:
        data = profile_manager.load(name)
        return jsonify(_strip_credentials(data))
    except ProfileNotFoundError:
        return jsonify({"error": f"Profile '{name}' not found."}), 404
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500
