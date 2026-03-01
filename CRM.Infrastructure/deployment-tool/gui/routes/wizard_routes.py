#!/usr/bin/env python3
"""Wizard step management routes."""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

import uuid
import os
from flask import Blueprint, request, jsonify
from core.validator import WizardValidator
from core.session import WizardSession, SessionStore
from core.step_manifest import StepManifestLoader

wizard_bp = Blueprint("wizard", __name__)
_store = SessionStore()
_validator = WizardValidator()

# Load step manifest
_manifest_path = Path(__file__).parent.parent.parent / "steps.yaml"
_manifest = StepManifestLoader()
try:
    _steps = _manifest.load(str(_manifest_path))
except Exception:
    _steps = []

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

_REQUIRED_STEPS = ["profile", "target", "database", "security", "seed"]
_PREFLIGHT_PORTS = [80, 443, 5000, 3306, 6379, 7700]


def _session_or_404(session_id: str):
    """Return the session or a 404 JSON response tuple."""
    session = _store.get(session_id)
    if session is None:
        return None, (jsonify({"error": f"Session '{session_id}' not found."}), 404)
    return session, None


def _percent_complete(session: WizardSession) -> int:
    total = len(_steps) if _steps else len(_REQUIRED_STEPS)
    if total == 0:
        return 0
    done = len(session.completed_steps)
    return min(100, int((done / total) * 100))


# ---------------------------------------------------------------------------
# POST /api/wizard/session — Create a new wizard session
# ---------------------------------------------------------------------------

@wizard_bp.route("/api/wizard/session", methods=["POST"])
def create_session():
    session = WizardSession(session_id=str(uuid.uuid4()))
    _store.save(session)
    return jsonify({
        "session_id": session.session_id,
        "current_step": session.current_step,
        "percent_complete": _percent_complete(session),
    }), 201


# ---------------------------------------------------------------------------
# GET /api/wizard/session/<session_id>
# ---------------------------------------------------------------------------

@wizard_bp.route("/api/wizard/session/<session_id>", methods=["GET"])
def get_session(session_id: str):
    session, err = _session_or_404(session_id)
    if err:
        return err
    return jsonify({
        "session_id": session.session_id,
        "current_step": session.current_step,
        "completed_steps": session.completed_steps,
        "data": session.data,
        "percent_complete": _percent_complete(session),
    })


# ---------------------------------------------------------------------------
# POST /api/wizard/session/<session_id>/step/<step_id>
# ---------------------------------------------------------------------------

@wizard_bp.route("/api/wizard/session/<session_id>/step/<step_id>", methods=["POST"])
def submit_step(session_id: str, step_id: str):
    session, err = _session_or_404(session_id)
    if err:
        return err

    body = request.get_json(silent=True) or {}
    data = body if isinstance(body, dict) else {}

    validation = _validator.validate_step(step_id, data)
    if not validation.valid:
        return jsonify({
            "valid": False,
            "errors": [
                {"field_id": e.field_id, "message": e.message, "fix_hint": e.fix_hint}
                for e in validation.errors
            ],
            "warnings": validation.warnings,
        }), 422

    # Persist step data in session
    session.data[step_id] = data
    if step_id not in session.completed_steps:
        session.completed_steps.append(step_id)
    session.current_step = step_id
    _store.save(session)

    return jsonify({
        "valid": True,
        "session_id": session.session_id,
        "current_step": step_id,
        "percent_complete": _percent_complete(session),
        "warnings": validation.warnings,
    })


# ---------------------------------------------------------------------------
# GET /api/wizard/session/<session_id>/review — Pre-deploy preflight
# ---------------------------------------------------------------------------

@wizard_bp.route("/api/wizard/session/<session_id>/review", methods=["GET"])
def review_session(session_id: str):
    session, err = _session_or_404(session_id)
    if err:
        return err

    issues: list[str] = []
    warnings: list[str] = []

    # 1. Check all required steps have data
    for step in _REQUIRED_STEPS:
        if step not in session.data:
            issues.append(f"Step '{step}' has not been completed.")

    # 2. Re-validate each completed step
    for step in _REQUIRED_STEPS:
        step_data = session.data.get(step)
        if step_data is None:
            continue
        result = _validator.validate_step(step, step_data)
        if not result.valid:
            for e in result.errors:
                issues.append(f"[{step}] {e.message}")
        warnings.extend(result.warnings)

    # 3. Port conflict check
    port_result = _validator.validate_port_conflict(_PREFLIGHT_PORTS, "ports")
    for e in port_result.errors:
        issues.append(e.message)
    warnings.extend(port_result.warnings)

    ready = len(issues) == 0
    return jsonify({
        "ready": ready,
        "issues": issues,
        "warnings": warnings,
        "session_data": session.data,
    })


# ---------------------------------------------------------------------------
# DELETE /api/wizard/session/<session_id>
# ---------------------------------------------------------------------------

@wizard_bp.route("/api/wizard/session/<session_id>", methods=["DELETE"])
def delete_session(session_id: str):
    _store.delete(session_id)
    return jsonify({"message": "session removed"})


# ---------------------------------------------------------------------------
# POST /api/wizard/validate-field — Inline single-field validation
# ---------------------------------------------------------------------------

@wizard_bp.route("/api/wizard/validate-field", methods=["POST"])
def validate_field():
    body = request.get_json(silent=True) or {}
    field_id = body.get("field_id", "")
    value = body.get("value", "")
    field_type = body.get("field_type", "")
    # step_id available in body if needed for cross-field validation

    # Dispatch based on field_type or field_id
    result = _dispatch_field_validation(field_id, field_type, value)

    if result.valid:
        resp = {"valid": True}
        if result.warnings:
            resp["warning"] = result.warnings[0]
        return jsonify(resp)

    first_error = result.errors[0] if result.errors else None
    return jsonify({
        "valid": False,
        "error": first_error.message if first_error else "Validation failed.",
        "hint": first_error.fix_hint if first_error else "",
    })


def _dispatch_field_validation(field_id: str, field_type: str, value: str):
    """Route to the appropriate validator method based on field type or id."""
    if field_type == "email" or field_id in ("email",) or field_id.endswith("_email"):
        return _validator.validate_email(value, field_id)
    if field_type == "username" or field_id.endswith("_username"):
        return _validator.validate_username(value, field_id)
    if field_type == "password" or field_id.endswith("_password"):
        return _validator.validate_password_strength(value, field_id)
    if field_type == "domain" or field_id.endswith("_domain") or field_id == "cors_origins":
        return _validator.validate_domain(value, field_id)
    if field_type == "port" or field_id.endswith("_port"):
        return _validator.validate_port(value, field_id)
    if field_type == "cidr" or field_id.endswith("_cidr"):
        return _validator.validate_cidr(value, field_id)
    return _validator.validate_required(value, field_id)
