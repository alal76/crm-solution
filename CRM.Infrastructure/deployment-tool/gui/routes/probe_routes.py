#!/usr/bin/env python3
"""
CRM CDT - Environment probe and component detection Flask routes.

NEW routes registered via this Blueprint:
    POST  /api/probe                  — run full environment probe
    POST  /api/detect                 — detect running CRM components
    GET   /api/probe/port-check       — quick port availability check
    POST  /api/probe/streaming        — streaming (NDJSON) probe results

These coexist with the existing /api/discovery/* routes in gui/app.py.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

# Ensure the tool root is on sys.path when the blueprint is imported
_TOOL_ROOT = Path(__file__).parent.parent.parent
if str(_TOOL_ROOT) not in sys.path:
    sys.path.insert(0, str(_TOOL_ROOT))

from flask import Blueprint, Response, jsonify, request, stream_with_context  # noqa: E402
from core.probe import EnvironmentProbe, ProbeTarget, CheckStatus  # noqa: E402
from core.detector import ComponentDetector  # noqa: E402

probe_bp = Blueprint("probe", __name__)


# ---------------------------------------------------------------------------
# POST /api/probe — run full environment probe for a target
# ---------------------------------------------------------------------------

@probe_bp.route("/api/probe", methods=["POST"])
def run_probe():
    """
    Body (JSON):
      {
        "connection_type": "local" | "ssh" | "cloud_aws" | "cloud_azure" | "cloud_gcp" | "kubernetes",
        "host":            "<hostname or IP>",        // optional
        "ssh_user":        "<username>",              // optional, default "root"
        "ssh_key_path":    "<path to private key>",   // optional
        "ssh_password":    "<password>",              // optional
        "cloud_credentials": { ... },                // optional
        "kubeconfig_path": "<path>",                  // optional
      }

    Returns ProbeResult serialized to JSON.
    """
    try:
        body = request.get_json(silent=True) or {}
        target = ProbeTarget(
            connection_type=body.get("connection_type", "local"),
            host=body.get("host", "localhost"),
            ssh_user=body.get("ssh_user", "root"),
            ssh_key_path=body.get("ssh_key_path", ""),
            ssh_password=body.get("ssh_password", ""),
            cloud_credentials=body.get("cloud_credentials", {}),
            kubeconfig_path=body.get("kubeconfig_path", ""),
        )
        result = EnvironmentProbe().run_all(target)
        return jsonify(result.to_dict())
    except Exception as exc:  # noqa: BLE001
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# POST /api/detect — detect running CRM components on a host
# ---------------------------------------------------------------------------

@probe_bp.route("/api/detect", methods=["POST"])
def detect_components():
    """
    Body (JSON):
      { "host": "<hostname or IP>" }   // optional, default "localhost"

    Returns a JSON array of ComponentStatus dicts.
    """
    try:
        body = request.get_json(silent=True) or {}
        host = body.get("host", "localhost")
        components = ComponentDetector().detect_all(host)
        return jsonify([c.to_dict() for c in components])
    except Exception as exc:  # noqa: BLE001
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# GET /api/probe/port-check?ports=80,443,5000 — quick port availability
# ---------------------------------------------------------------------------

@probe_bp.route("/api/probe/port-check", methods=["GET"])
def port_check():
    """
    Query params:
      ports   — comma-separated list of integers, e.g. ?ports=80,443,5000

    Returns a JSON object mapping each port to a bool (True = available).
    """
    ports_param = request.args.get("ports", "")
    if not ports_param:
        return jsonify({"error": "Missing 'ports' query parameter"}), 400

    results: dict = {}
    for raw in ports_param.split(","):
        raw = raw.strip()
        if not raw:
            continue
        try:
            port = int(raw)
        except ValueError:
            results[raw] = False
            continue

        # detect_tcp returns True when the port is *in use*; the UI
        # asks "is this port *available*?" so we invert the result.
        in_use = ComponentDetector.detect_tcp("localhost", port, timeout=1.0)
        results[port] = not in_use  # True = available / bindable

    return jsonify(results)


# ---------------------------------------------------------------------------
# POST /api/probe/streaming — same as /api/probe but streams NDJSON
# ---------------------------------------------------------------------------

@probe_bp.route("/api/probe/streaming", methods=["POST"])
def run_probe_streaming():
    """
    Same request body as POST /api/probe.
    Emits one JSON object per line (NDJSON) as each check completes.
    Final line is the aggregated ProbeResult with overall status.

    Example output:
      {"name":"Local Docker","status":"pass","detail":"...","fix_hint":""}
      {"name":"Disk Space","status":"pass","detail":"50.0 GB free","fix_hint":""}
      {"overall":"pass","passed":2,"warned":0,"failed":0,"checks":[...]}
    """
    body = request.get_json(silent=True) or {}
    target = ProbeTarget(
        connection_type=body.get("connection_type", "local"),
        host=body.get("host", "localhost"),
        ssh_user=body.get("ssh_user", "root"),
        ssh_key_path=body.get("ssh_key_path", ""),
        ssh_password=body.get("ssh_password", ""),
        cloud_credentials=body.get("cloud_credentials", {}),
        kubeconfig_path=body.get("kubeconfig_path", ""),
    )

    probe = EnvironmentProbe()

    def _generate():
        """Yield each CheckResult as it completes, then the final summary."""
        from core.probe import CheckResult, ProbeResult  # local import to avoid circular

        # We delegate to run_all to get all checks, then stream them.
        # For true streaming we could hook per-check callbacks, but here
        # we build the list eagerly then flush line-by-line — still useful
        # for chunked encoding.
        try:
            result = probe.run_all(target)
            for check in result.checks:
                yield json.dumps(check.to_dict()) + "\n"
            # Final summary line
            yield json.dumps(result.to_dict()) + "\n"
        except Exception as exc:  # noqa: BLE001
            yield json.dumps({"error": str(exc)}) + "\n"

    return Response(
        stream_with_context(_generate()),
        mimetype="application/x-ndjson",
    )
