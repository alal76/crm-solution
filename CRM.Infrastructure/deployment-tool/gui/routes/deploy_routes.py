#!/usr/bin/env python3
"""Deployment management routes."""
import io
import logging
import sys
import queue
import threading
import json
import time
import zipfile
from pathlib import Path

logger = logging.getLogger("cdt.deploy")

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
from core.constants import (
    K8S_COMPUTES,
    SERVERLESS_COMPUTES,
    CLOUD_COMPUTES,
    RUNTIME_DOCKER_COMPOSE,
    RUNTIME_KUBERNETES,
    RUNTIME_SERVERLESS,
    detect_runtime,
    get_kubeconfig,
)
from deployers.docker_compose import DockerComposeDeployer, classify_container
from deployers.kubernetes import KubernetesDeployer

deploy_bp = Blueprint("deploy", __name__)
_active_deploys: dict = {}


@deploy_bp.route("/api/deploy/preflight", methods=["POST"])
def deploy_preflight():
    """Check for existing CRM infrastructure before deployment.

    Platform-aware: checks local/remote Docker containers for docker_compose
    deployments, Kubernetes pods/deployments for K8s targets, and returns
    structured info so the UI can show a reuse/recreate choice regardless
    of platform.

    Body (JSON — all optional):
      { "platform": "on_premises|azure|aws|gcp",
        "runtime":  "docker_compose|kubernetes",
        "target":   { "host": "...", "ssh_user": "root", "ssh_port": 22 },
        "cloud_services": { "<platform>": { "compute": "aks|eks|..." } },
        "namespace": "crm-prod" }

    Returns:
      { "containers": [...],          # Docker containers found
        "resources":  [...],          # K8s resources found
        "runtime":    "docker_compose|kubernetes",
        "has_existing": true|false }  # convenience flag
    """
    import subprocess  # noqa: PLC0415

    body = request.get_json(silent=True) or {}
    target = body.get("target", {})
    deploy_host = (
        target.get("host")
        or target.get("domain_name")
        or body.get("host")
        or body.get("deployment_host")
        or ""
    )

    # Determine runtime using centralized logic
    runtime = detect_runtime(body)

    # ── Kubernetes preflight ─────────────────────────────────────────
    if runtime in (RUNTIME_KUBERNETES, RUNTIME_SERVERLESS):
        ns = body.get("namespace", "crm-prod")
        resources = []
        try:
            kubeconfig = get_kubeconfig(body)
            cmd = ["kubectl"]
            if kubeconfig:
                cmd += ["--kubeconfig", kubeconfig]
            cmd += ["get", "all", f"-n={ns}", "-o", "json"]
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=15)
            if result.returncode == 0 and result.stdout.strip():
                data = json.loads(result.stdout)
                for item in data.get("items", []):
                    kind = item.get("kind", "")
                    meta = item.get("metadata", {})
                    name = meta.get("name", "")
                    status_info = ""
                    if kind == "Pod":
                        status_info = item.get("status", {}).get("phase", "")
                    elif kind in ("Deployment", "StatefulSet"):
                        spec_r = item.get("spec", {}).get("replicas", 0)
                        ready_r = item.get("status", {}).get("readyReplicas", 0)
                        status_info = f"{ready_r}/{spec_r} ready"
                    elif kind == "Service":
                        svc_type = item.get("spec", {}).get("type", "")
                        status_info = svc_type
                    resources.append({
                        "kind": kind,
                        "name": name,
                        "status": status_info,
                        "group": _classify_k8s_resource(name),
                    })
        except Exception as exc:  # noqa: BLE001
            logger.warning("K8s preflight: kubectl unavailable or errored: %s", exc)

        return jsonify({
            "containers": [],
            "resources": resources,
            "runtime": runtime,
            "has_existing": len(resources) > 0,
            "namespace": ns,
        })

    # ── Docker Compose preflight ─────────────────────────────────────
    containers = []

    # Remote target: check via SSH
    if deploy_host and deploy_host not in ("localhost", "127.0.0.1", "", "NO_HOST_CONFIGURED"):
        ssh_user = target.get("ssh_user", "root")
        ssh_port = int(target.get("ssh_port", 22))
        try:
            import paramiko  # noqa: PLC0415
            client = paramiko.SSHClient()
            client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            client.connect(hostname=deploy_host, username=ssh_user, port=ssh_port, timeout=10)
            _, stdout, _ = client.exec_command(
                'docker ps -a --filter name=crm- --format \'{"Names":"{{.Names}}","Image":"{{.Image}}","Status":"{{.Status}}","State":"{{.State}}","Ports":"{{.Ports}}"}\'',
                timeout=15,
            )
            raw = stdout.read().decode().strip()
            client.close()
            if raw:
                for line in raw.splitlines():
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
        except Exception as exc:  # noqa: BLE001
            logger.warning("SSH preflight to %s:%s failed: %s", deploy_host, ssh_port, exc)
    else:
        # Local Docker check
        try:
            result = subprocess.run(
                ["docker", "ps", "-a", "--filter", "name=crm-", "--format", "{{json .}}"],
                capture_output=True, text=True, timeout=15,
            )
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
        except Exception as exc:  # noqa: BLE001
            logger.warning("Local Docker preflight failed: %s", exc)

    return jsonify({
        "containers": containers,
        "resources": [],
        "runtime": "docker_compose",
        "has_existing": len(containers) > 0,
    })


def _classify_k8s_resource(name: str) -> str:
    """Classify a Kubernetes resource by name into a UI group."""
    name_lower = name.lower()
    if any(k in name_lower for k in ("mariadb", "mysql", "postgres", "redis", "mongo")):
        return "database"
    if any(k in name_lower for k in ("api", "frontend", "gateway")):
        return "app"
    if any(k in name_lower for k in ("meilisearch", "ollama", "chatwoot", "novu", "superset", "docuseal", "n8n")):
        return "provider"
    return "other"


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
    # "fetch_existing" (default) reads secrets from the remote .env so
    # redeployments keep the same passwords.  "entered" means the user
    # has manually entered all required passwords in the wizard.
    password_strategy = data.get("password_strategy", "fetch_existing")

    # ── Recover existing secrets from remote deployment ──────────────
    # When redeploying to a server that already has containers running,
    # read passwords/secrets from the existing .env so we don't generate
    # new random values that mismatch the already-initialized databases.
    if password_strategy == "fetch_existing":
        target = profile.get("target", {})
        deploy_host = (
            target.get("host")
            or target.get("domain_name")
            or profile.get("host")
            or profile.get("deployment_host")
            or ""
        )
        remote_dir = target.get(
            "remote_deploy_dir",
            profile.get("remote_deploy_dir", "/opt/crm-deployment"),
        )
        if deploy_host and deploy_host not in ("NO_HOST_CONFIGURED",):
            ssh_user = target.get("ssh_user", "root")
            ssh_port = int(target.get("ssh_port", 22))
            ssh_key  = target.get("ssh_key") or None
            ssh_password = target.get("ssh_password") or None
            try:
                existing_secrets = DockerComposeDeployer.read_remote_env_secrets(
                    host=deploy_host,
                    remote_deploy_dir=remote_dir,
                    ssh_user=ssh_user,
                    ssh_port=ssh_port,
                    ssh_key=ssh_key,
                    ssh_password=ssh_password,
                )
                if existing_secrets:
                    logger.info("Recovered %d existing secrets from %s", len(existing_secrets), deploy_host)
                    DockerComposeDeployer.inject_secrets_into_profile(
                        profile, existing_secrets
                    )
                else:
                    # Check if a MariaDB volume exists — if so, generating
                    # fresh passwords will cause ACCESS DENIED because
                    # MariaDB ignores MYSQL_PASSWORD after first init.
                    volume_exists = DockerComposeDeployer.check_remote_db_volume_exists(
                        host=deploy_host,
                        ssh_user=ssh_user,
                        ssh_port=ssh_port,
                        ssh_key=ssh_key,
                        ssh_password=ssh_password,
                    )
                    if volume_exists:
                        logger.error(
                            "No .env secrets recovered from %s but MariaDB "
                            "data volume exists — fresh passwords WILL cause "
                            "credential mismatch. Aborting deploy.",
                            deploy_host,
                        )
                        return jsonify({
                            "error": (
                                f"Cannot recover existing DB passwords from "
                                f"{deploy_host}:{remote_dir}/.env, but MariaDB "
                                f"data volume already exists.  Please enter the "
                                f"correct database passwords in the Secrets & "
                                f"Authentication step, or remove the MariaDB "
                                f"volume on the target for a fresh start."
                            ),
                        }), 400
                    logger.warning(
                        "No existing secrets recovered from %s:%s — new random "
                        "passwords will be generated (first-time deploy).",
                        deploy_host, remote_dir,
                    )
            except Exception as exc:  # noqa: BLE001
                # Recovery failed — check if this is a dangerous re-deploy
                volume_exists = DockerComposeDeployer.check_remote_db_volume_exists(
                    host=deploy_host,
                    ssh_user=ssh_user,
                    ssh_port=ssh_port,
                )
                if volume_exists:
                    logger.error(
                        "Secret recovery from %s failed (%s) AND MariaDB volume "
                        "exists — aborting to prevent credential mismatch.",
                        deploy_host, exc,
                    )
                    return jsonify({
                        "error": (
                            f"Secret recovery from {deploy_host} failed: {exc}. "
                            f"MariaDB data volume exists — please enter the "
                            f"correct database passwords in the Secrets & "
                            f"Authentication step, or remove the MariaDB "
                            f"volume on the target for a fresh start."
                        ),
                    }), 400
                logger.warning(
                    "Secret recovery from %s failed: %s — generating new "
                    "passwords (no existing DB volume detected, safe to proceed).",
                    deploy_host, exc,
                )

    gen = ConfigGenerator()
    try:
        result = gen.generate(profile)
    except Exception as exc:  # noqa: BLE001
        return jsonify({"error": f"Configuration error: {exc}"}), 400

    log_q: queue.Queue = queue.Queue()

    # ── Determine deployer runtime (centralized) ────────────────────
    runtime = detect_runtime(profile)

    try:
        if runtime == RUNTIME_KUBERNETES:
            deployer = KubernetesDeployer(
                result.output_dir or Path("/tmp"), profile, log_q, dry_run=dry_run,
                infrastructure_action=container_action,
            )
        else:
            # docker_compose AND serverless both use DockerComposeDeployer
            deployer = DockerComposeDeployer(
                result.output_dir or Path("/tmp"), profile, log_q, dry_run=dry_run,
                container_action=container_action,
                containers_to_remove=containers_to_remove,
            )
    except (ValueError, TypeError) as exc:
        return jsonify({"error": str(exc)}), 400

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
            # ── Save generated artifacts with the profile ──
            try:
                from core.profile import ProfileManager
                pm = ProfileManager()
                profile_name = (
                    profile.get("meta", {}).get("profile_name")
                    or profile.get("name")
                    or session_id
                )
                artifact_files: dict[str, str] = {}
                if result and result.files:
                    for gf in result.files:
                        if gf.filename and gf.content:
                            artifact_files[gf.filename] = gf.content
                if artifact_files:
                    pm.save_artifacts(profile_name, artifact_files)
                    emit_log(
                        session_id, "INFO",
                        f"Saved {len(artifact_files)} deployment artifact(s) "
                        f"with profile '{profile_name}'",
                    )
            except Exception as ae:  # noqa: BLE001
                logger.warning("Failed to save deployment artifacts: %s", ae)

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
        except Exception as exc:
            logger.warning("Failed to read deploy history from %s: %s", history_file, exc)
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
