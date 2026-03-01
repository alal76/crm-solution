#!/usr/bin/env python3
"""Deployment management routes."""
import io
import sys
import queue
import threading
import json
import time
import zipfile
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
from deployers.docker_compose import DockerComposeDeployer, classify_container
from deployers.kubernetes import KubernetesDeployer

deploy_bp = Blueprint("deploy", __name__)
_active_deploys: dict = {}


@deploy_bp.route("/api/deploy/preflight", methods=["POST"])
def deploy_preflight():
    """Check for existing CRM containers before deployment.

    Returns a list of existing containers so the UI can ask the user
    whether to *reuse* or *recreate* them.
    """
    import subprocess  # noqa: PLC0415

    try:
        result = subprocess.run(
            ["docker", "ps", "-a", "--filter", "name=crm-", "--format", "{{json .}}"],
            capture_output=True, text=True, timeout=15,
        )
    except Exception as exc:
        return jsonify({"error": f"Docker not available: {exc}"}), 500

    containers = []
    if result.returncode == 0 and result.stdout.strip():
        for line in result.stdout.strip().splitlines():
            try:
                obj = json.loads(line)
                name = obj.get("Names", "")
                containers.append({
                    "name": name,
                    "image": obj.get("Image", ""),
                    "status": obj.get("Status", ""),
                    "state": obj.get("State", ""),
                    "ports": obj.get("Ports", ""),
                    "group": classify_container(name),
                })
            except json.JSONDecodeError:
                continue

    return jsonify({"containers": containers})


@deploy_bp.route("/api/target/arch", methods=["POST"])
def detect_target_arch():
    """Detect the CPU architecture of the deployment target.

    For local targets, uses platform.machine().
    For remote (SSH) targets, runs ``uname -m`` over SSH.

    Body (JSON):
      { "host": "<hostname>", "ssh_user": "root", "ssh_port": 22 }

    Returns:
      { "machine_arch": "x86_64"|"aarch64"|..., "docker_platform": "linux/amd64"|"linux/arm64"|... }
    """
    import platform as _platform  # noqa: PLC0415
    body = request.get_json(silent=True) or {}
    host = body.get("host", "localhost")
    ssh_user = body.get("ssh_user", "root")
    ssh_port = int(body.get("ssh_port", 22))

    _arch_map = {
        "arm64": "linux/arm64", "aarch64": "linux/arm64",
        "x86_64": "linux/amd64", "amd64": "linux/amd64",
    }

    # Local deployment — use build machine arch
    if host in ("localhost", "127.0.0.1", ""):
        raw = _platform.machine()
        return jsonify({
            "machine_arch": raw,
            "docker_platform": _arch_map.get(raw.lower(), f"linux/{raw}"),
            "source": "local",
        })

    # Remote deployment — SSH uname -m
    try:
        import paramiko  # noqa: PLC0415
    except ImportError:
        return jsonify({
            "machine_arch": "unknown",
            "docker_platform": "linux/amd64",
            "source": "default (paramiko not installed)",
        })

    try:
        client = paramiko.SSHClient()
        client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        client.connect(hostname=host, username=ssh_user, port=ssh_port, timeout=10)
        _, stdout, _ = client.exec_command("uname -m", timeout=10)
        raw = stdout.read().decode().strip()
        client.close()
        return jsonify({
            "machine_arch": raw,
            "docker_platform": _arch_map.get(raw.lower(), f"linux/{raw}"),
            "source": "ssh",
        })
    except Exception as exc:  # noqa: BLE001
        return jsonify({
            "machine_arch": "unknown",
            "docker_platform": "linux/amd64",
            "source": f"default (SSH error: {exc})",
            "error": str(exc),
        })


@deploy_bp.route("/api/deploy", methods=["POST"])
def start_deploy():
    data = request.json or {}
    profile = data.get("profile", {})
    session_id = data.get("session_id", f"dep_{int(time.time())}")
    dry_run = data.get("dry_run", False)
    container_action = data.get("container_action", "recreate")  # reuse | recreate
    containers_to_remove = data.get("containers_to_remove", [])  # explicit list

    gen = ConfigGenerator()
    try:
        result = gen.generate(profile)
    except Exception as exc:  # noqa: BLE001
        return jsonify({"error": f"Configuration error: {exc}"}), 400

    log_q: queue.Queue = queue.Queue()
    arch = profile.get("architecture", {})
    runtime = (
        arch.get("container_runtime", "docker_compose")
        if isinstance(arch, dict)
        else (arch or "docker_compose")
    )
    if runtime == "kubernetes":
        deployer = KubernetesDeployer(
            result.output_dir or Path("/tmp"), profile, log_q, dry_run=dry_run
        )
    else:
        deployer = DockerComposeDeployer(
            result.output_dir or Path("/tmp"), profile, log_q, dry_run=dry_run,
            container_action=container_action,
            containers_to_remove=containers_to_remove,
        )

    # Register session BEFORE starting background task to avoid race condition
    _active_deploys[session_id] = {
        "deployer": deployer,
        "thread": None,  # will be set below
        "log_queue": log_q,
        "started": time.time(),
        "output_dir": str(result.output_dir or ""),
        "success": None,
    }

    def run():
        success = False
        emit_log(session_id, "INFO", "Deployment started")
        emit_progress(session_id, 5, "initializing")
        try:
            success = bool(deployer.deploy())
            level = "SUCCESS" if success else "ERROR"
            msg = "Deployment finished" if success else "Deployment failed"
            emit_log(session_id, level, msg)
            emit_done(session_id, success=success, result={"session_id": session_id})
        except Exception as exc:  # noqa: BLE001
            emit_log(session_id, "ERROR", f"Deployment error: {exc}")
            emit_done(session_id, success=False, result={"error": str(exc)})
        finally:
            _active_deploys[session_id]["success"] = success
            log_q.put(None)  # sentinel

    t = start_background_task(run)
    _active_deploys[session_id]["thread"] = t
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
                    success = dep.get("success", False)
                    yield f"data: {json.dumps({'done': True, 'success': success})}\n\n"
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
    alive = t.is_alive() if hasattr(t, "is_alive") else True
    return jsonify(
        {
            "session_id": session_id,
            "running": alive,
            "elapsed_seconds": round(elapsed, 1),
            "success": dep.get("success"),
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


@deploy_bp.route("/api/download", methods=["GET"])
def download_generated_files():
    """Zip the output_dir for a completed deployment and stream it to the browser."""
    from flask import send_file  # noqa: PLC0415
    session_id = request.args.get("session_id", "")
    entry = _active_deploys.get(session_id)
    if not entry:
        return jsonify({"error": "Session not found"}), 404
    output_dir = entry.get("output_dir")
    if not output_dir:
        return jsonify({"error": "No output directory recorded for this session"}), 400
    p = Path(output_dir)
    if not p.exists() or not p.is_dir():
        return jsonify({"error": "Output directory no longer exists"}), 410
    buf = io.BytesIO()
    with zipfile.ZipFile(buf, "w", zipfile.ZIP_DEFLATED) as zf:
        for f in p.rglob("*"):
            if f.is_file():
                zf.write(f, f.relative_to(p))
    buf.seek(0)
    return send_file(
        buf,
        mimetype="application/zip",
        as_attachment=True,
        download_name=f"crm-deploy-{session_id}.zip",
    )


@deploy_bp.route("/api/config/preview", methods=["POST"])
def config_preview():
    profile = (request.json or {}).get("profile", {})
    gen = ConfigGenerator()
    try:
        files = gen.generate_preview(profile)
        return jsonify({"files": files})
    except Exception as e:
        return jsonify({"error": str(e)}), 500
