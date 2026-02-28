#!/usr/bin/env python3
"""Deployment management routes."""
import sys
import queue
import threading
import json
import time
from pathlib import Path

try:
    from core.socket_helpers import emit_log, emit_step, emit_progress, emit_done, start_background_task
    _SOCKET_HELPERS = True
except ImportError:
    _SOCKET_HELPERS = False

    def emit_log(run_id, level, message, extra=None):  # noqa: E303
        pass

    def emit_step(run_id, step_index, state, label=""):
        pass  # no-op fallback when socket_helpers unavailable

    def emit_progress(run_id, pct, status=""):
        pass  # no-op fallback when socket_helpers unavailable

    def emit_done(run_id, success, result=None):
        pass  # no-op fallback when socket_helpers unavailable

    def start_background_task(fn, *args, **kwargs):
        import threading as _threading
        t = _threading.Thread(target=fn, args=args, kwargs=kwargs, daemon=True)
        t.start()
        return t

sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from flask import Blueprint, request, jsonify, Response, stream_with_context
from core.generator import ConfigGenerator
from deployers.docker_compose import DockerComposeDeployer
from deployers.kubernetes import KubernetesDeployer

deploy_bp = Blueprint("deploy", __name__)
_active_deploys: dict = {}


@deploy_bp.route("/api/deploy", methods=["POST"])
def start_deploy():
    data = request.json or {}
    profile = data.get("profile", {})
    session_id = data.get("session_id", f"dep_{int(time.time())}")
    dry_run = data.get("dry_run", False)

    gen = ConfigGenerator()
    result = gen.generate(profile)

    log_q: queue.Queue = queue.Queue()
    runtime = profile.get("architecture", {}).get("container_runtime", "docker_compose")
    if runtime == "kubernetes":
        deployer = KubernetesDeployer(
            result.output_dir or Path("/tmp"), profile, log_q, dry_run=dry_run
        )
    else:
        deployer = DockerComposeDeployer(
            result.output_dir or Path("/tmp"), profile, log_q, dry_run=dry_run
        )

    def run():
        emit_log(session_id, "INFO", "Deployment started")
        emit_progress(session_id, 5, "initializing")
        try:
            deployer.deploy()
            emit_log(session_id, "SUCCESS", "Deployment finished")
            emit_done(session_id, success=True, result={"session_id": session_id})
        except Exception as exc:  # noqa: BLE001
            emit_log(session_id, "ERROR", f"Deployment error: {exc}")
            emit_done(session_id, success=False, result={"error": str(exc)})
        finally:
            log_q.put(None)  # sentinel

    t = start_background_task(run)
    _active_deploys[session_id] = {
        "deployer": deployer,
        "thread": t,
        "log_queue": log_q,
        "started": time.time(),
    }
    return jsonify(
        {
            "session_id": session_id,
            "status": "started",
            "output_dir": str(result.output_dir or ""),
            "gen_errors": result.errors,
        }
    )


@deploy_bp.route("/api/deploy/<session_id>/stream", methods=["GET"])
def stream_deploy(session_id: str):
    if session_id not in _active_deploys:
        return jsonify({"error": "session not found"}), 404
    dep = _active_deploys[session_id]

    def generate():
        log_q = dep["log_queue"]
        while True:
            try:
                event = log_q.get(timeout=30)
                if event is None:
                    yield f"data: {json.dumps({'done': True})}\n\n"
                    break
                yield f"data: {json.dumps(event.to_dict())}\n\n"
            except queue.Empty:
                yield f"data: {json.dumps({'heartbeat': True})}\n\n"

    return Response(
        stream_with_context(generate()),
        content_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )


@deploy_bp.route("/api/deploy/<session_id>/status", methods=["GET"])
def deploy_status(session_id: str):
    if session_id not in _active_deploys:
        return jsonify({"error": "not found"}), 404
    dep = _active_deploys[session_id]
    t = dep["thread"]
    elapsed = time.time() - dep["started"]
    alive = t.is_alive()
    return jsonify(
        {
            "session_id": session_id,
            "running": alive,
            "elapsed_seconds": round(elapsed, 1),
        }
    )


@deploy_bp.route("/api/deploy/<session_id>/stop", methods=["POST"])
def stop_deploy(session_id: str):
    if session_id not in _active_deploys:
        return jsonify({"error": "not found"}), 404
    _active_deploys[session_id]["deployer"].abort()
    return jsonify({"message": "Abort signal sent"})


@deploy_bp.route("/api/deploy/history", methods=["GET"])
def deploy_history():
    history_file = Path.home() / ".crm-cdt" / "deploy_history.json"
    if history_file.exists():
        try:
            return jsonify(json.loads(history_file.read_text()))
        except Exception:
            pass
    return jsonify([])


@deploy_bp.route("/api/config/preview", methods=["POST"])
def config_preview():
    profile = (request.json or {}).get("profile", {})
    gen = ConfigGenerator()
    try:
        files = gen.generate_preview(profile)
        return jsonify({"files": files})
    except Exception as e:
        return jsonify({"error": str(e)}), 500
