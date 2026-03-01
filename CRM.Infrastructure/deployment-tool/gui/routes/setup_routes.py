"""
Setup Routes — Prerequisites & Tool Installation API
=====================================================
Provides endpoints for checking and installing system tools, Python SDKs,
and streaming installation progress via Server-Sent Events (SSE).
"""

from __future__ import annotations

import json
import os
import queue
import threading
from typing import Generator

from flask import Blueprint, Response, jsonify, request, stream_with_context

setup_bp = Blueprint("setup", __name__)

# Lazy import so Flask starts even if tool_installer deps are partly missing
def _installer():
    import importlib, sys
    sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..'))
    from core.tool_installer import (
        full_status_report,
        install_tool_streaming,
        install_sdk_streaming,
        install_all_pip_streaming,
        check_all_tools,
        check_all_sdks,
    )
    return {
        "full_status_report": full_status_report,
        "install_tool_streaming": install_tool_streaming,
        "install_sdk_streaming": install_sdk_streaming,
        "install_all_pip_streaming": install_all_pip_streaming,
        "check_all_tools": check_all_tools,
        "check_all_sdks": check_all_sdks,
    }


# ---------------------------------------------------------------------------
# GET /api/setup/status
# ---------------------------------------------------------------------------

@setup_bp.route("/api/setup/status", methods=["GET"])
def get_status():
    """Return full tool + SDK installation status."""
    try:
        report = _installer()["full_status_report"]()
        return jsonify(report)
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# GET /api/setup/tools
# ---------------------------------------------------------------------------

@setup_bp.route("/api/setup/tools", methods=["GET"])
def get_tools():
    """Return tool check results only."""
    try:
        tools = _installer()["check_all_tools"]()
        return jsonify([t.__dict__ if hasattr(t, "__dict__") else t for t in tools])
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# GET /api/setup/sdks
# ---------------------------------------------------------------------------

@setup_bp.route("/api/setup/sdks", methods=["GET"])
def get_sdks():
    """Return Python SDK group check results."""
    try:
        sdks = _installer()["check_all_sdks"]()
        return jsonify(sdks)
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# POST /api/setup/install — streaming SSE
# ---------------------------------------------------------------------------

@setup_bp.route("/api/setup/install", methods=["POST"])
def install_tool():
    """
    Install a system tool or Python SDK group.
    Body: { "type": "tool"|"sdk"|"pip", "key": "<tool_or_sdk_key>" }
    Returns: SSE stream of installation output lines.
    """
    data = request.get_json(force=True, silent=True) or {}
    install_type = data.get("type", "tool")
    key = data.get("key", "")

    if not key:
        return jsonify({"error": "key is required"}), 400

    # Queue to bridge worker thread → SSE generator
    q: queue.Queue = queue.Queue()
    SENTINEL = object()

    def emit(line: str):
        q.put(line)

    def worker():
        try:
            funcs = _installer()
            if install_type == "tool":
                funcs["install_tool_streaming"](key, emit)
            elif install_type == "sdk":
                funcs["install_sdk_streaming"](key, emit)
            elif install_type == "pip":
                req_file = data.get("requirements_file")
                funcs["install_all_pip_streaming"](emit, req_file)
            else:
                emit(f"Unknown install type: {install_type}")
        except Exception as exc:
            emit(f"ERROR: {exc}")
        finally:
            q.put(SENTINEL)

    threading.Thread(target=worker, daemon=True).start()

    @stream_with_context
    def generate() -> Generator[str, None, None]:
        while True:
            item = q.get()
            if item is SENTINEL:
                yield "data: [DONE]\n\n"
                break
            yield f"data: {json.dumps({'line': item})}\n\n"

    return Response(
        generate(),
        mimetype="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "X-Accel-Buffering": "no",
        },
    )


# ---------------------------------------------------------------------------
# POST /api/setup/install-all-required
# ---------------------------------------------------------------------------

@setup_bp.route("/api/setup/install-all-required", methods=["POST"])
def install_all_required():
    """
    Install all missing required + recommended tools sequentially.
    Returns SSE stream.
    """
    q: queue.Queue = queue.Queue()
    SENTINEL = object()

    def emit(line: str):
        q.put(line)

    def worker():
        try:
            funcs = _installer()
            tools = funcs["check_all_tools"]()
            missing = [t for t in tools if not t.installed and t.category in ("required", "recommended")]
            if not missing:
                emit("✅ All required tools are already installed.")
            else:
                for tool_status in missing:
                    emit(f"\n── Installing {tool_status.name} ──")
                    funcs["install_tool_streaming"](tool_status.key, emit)

            # Also install base pip packages
            emit("\n── Installing Python dependencies ──")
            req_file = os.path.join(
                os.path.dirname(__file__), '..', '..', 'requirements.txt'
            )
            funcs["install_all_pip_streaming"](emit, req_file)
            emit("\n✅ Setup complete.")
        except Exception as exc:
            emit(f"ERROR: {exc}")
        finally:
            q.put(SENTINEL)

    threading.Thread(target=worker, daemon=True).start()

    @stream_with_context
    def generate() -> Generator[str, None, None]:
        while True:
            item = q.get()
            if item is SENTINEL:
                yield "data: [DONE]\n\n"
                break
            yield f"data: {json.dumps({'line': item})}\n\n"

    return Response(
        generate(),
        mimetype="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )
