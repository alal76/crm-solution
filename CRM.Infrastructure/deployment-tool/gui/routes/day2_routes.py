#!/usr/bin/env python3
"""Day-2 operations routes.

All monitoring and management endpoints are **profile-aware**.  Pass
``?profile=<name>`` to target a specific deployment profile.  When the
query-param is omitted the active profile (``~/.crm-cdt/active_profile_name.txt``)
is used as a fallback – ensuring the Day-2 page always monitors the right
environment instead of blindly checking localhost.
"""
import sys
import json
import time
import threading
import subprocess
import shutil
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from flask import Blueprint, request, jsonify
from day2.upgrade import UpgradeManager
from day2.rollback import RollbackManager
from day2.scale import ScaleManager
from day2.rotate_secrets import SecretRotator

day2_bp = Blueprint("day2", __name__)
_upgrade_jobs: dict = {}

# ---------------------------------------------------------------------------
# Shared constants (single source of truth in core.constants)
# ---------------------------------------------------------------------------
from core.constants import (  # noqa: E402
    CDT_DIR,
    DOCKER_FILTER_CRM,
    ACTIVE_PROFILE_NAME_FILE as _ACTIVE_PROFILE_NAME_FILE,
    K8S_COMPUTES,
    SERVERLESS_COMPUTES,
    RUNTIME_DOCKER_COMPOSE,
    RUNTIME_KUBERNETES,
    RUNTIME_SERVERLESS,
    detect_runtime as _detect_runtime_shared,
    get_kubeconfig as _get_kubeconfig,
)


# ---------------------------------------------------------------------------
# Profile-aware helpers
# ---------------------------------------------------------------------------

def _get_profile():
    """Return the active profile dict from ~/<CDT_DIR>/last_profile.json."""
    profile_file = Path.home() / CDT_DIR / "last_profile.json"
    if profile_file.exists():
        return json.loads(profile_file.read_text())
    return {}


def _resolve_profile(profile_name: str | None = None) -> tuple[dict, str]:
    """Resolve a profile dict and its display name.

    Priority:
      1. ``profile_name`` query-param → load from ProfileManager
      2. Active profile name from ``active_profile_name.txt`` → load
      3. Fallback to ``last_profile.json`` (legacy)

    Returns ``(profile_dict, name_str)``.
    """
    # 1. Explicit name passed
    if profile_name:
        try:
            from core.profile import ProfileManager  # noqa: PLC0415
            pm = ProfileManager()
            data = pm.load(profile_name)
            return data, profile_name
        except Exception:  # noqa: BLE001
            pass  # fall-through

    # 2. Active profile name file
    meta_file = Path.home() / CDT_DIR / _ACTIVE_PROFILE_NAME_FILE
    if meta_file.exists():
        try:
            name = meta_file.read_text(encoding="utf-8").strip()
            if name:
                from core.profile import ProfileManager  # noqa: PLC0415
                pm = ProfileManager()
                data = pm.load(name)
                return data, name
        except Exception:  # noqa: BLE001
            pass

    # 3. Legacy fallback
    return _get_profile(), _get_profile().get("meta", {}).get("profile_name", "—")


def _detect_runtime(profile: dict) -> str:
    """Return ``'kubernetes'``, ``'serverless'``, or ``'docker_compose'``.

    Delegates to the centralised ``core.constants.detect_runtime`` so that
    all modules use the same compute→runtime mapping.
    """
    return _detect_runtime_shared(profile)


def _get_deploy_host(profile: dict) -> str:
    """Extract the deployment host from a profile (empty = localhost)."""
    target = profile.get("target", {})
    host = (
        target.get("host")
        or target.get("domain_name")
        or ""
    )
    return host


def _is_remote(host: str) -> bool:
    """Return True when *host* is a real remote address (not empty/localhost)."""
    return bool(host) and host not in ("localhost", "127.0.0.1", "")


def _build_health_url(profile: dict) -> str:
    """Build the API health-check URL from profile target settings."""
    target = profile.get("target", {})
    host = _get_deploy_host(profile)
    port = target.get("api_port", "5000")
    if _is_remote(host):
        return f"http://{host}:{port}/health"
    return f"http://localhost:{port}/health"


def _build_api_base_url(profile: dict) -> str:
    """Build base API URL (without path) for the profiled environment."""
    target = profile.get("target", {})
    host = _get_deploy_host(profile)
    port = target.get("api_port", "5000")
    if _is_remote(host):
        return f"http://{host}:{port}"
    return f"http://localhost:{port}"


def _ssh_connect(profile: dict):
    """Open a paramiko SSH connection to the profile's target host.

    Returns the connected client or *None* on failure.
    """
    host = _get_deploy_host(profile)
    if not _is_remote(host):
        return None
    target = profile.get("target", {})
    try:
        import paramiko  # noqa: PLC0415
        client = paramiko.SSHClient()
        client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        client.connect(
            hostname=host,
            username=target.get("ssh_user", "root"),
            port=int(target.get("ssh_port", 22)),
            timeout=10,
        )
        return client
    except Exception:  # noqa: BLE001
        return None


def _docker_cmd_profile(profile: dict, *args, timeout: int = 30):
    """Run a docker command locally or remotely depending on the profile.

    Returns ``(ok, stdout, stderr)``.
    """
    import shlex  # noqa: PLC0415
    host = _get_deploy_host(profile)

    # Remote execution via SSH
    if _is_remote(host):
        client = _ssh_connect(profile)
        if not client:
            return False, "", f"SSH connection to {host} failed"
        try:
            full_cmd = "docker " + " ".join(shlex.quote(str(a)) for a in args)
            _, stdout, stderr = client.exec_command(full_cmd, timeout=timeout)
            out = stdout.read().decode().strip()
            err = stderr.read().decode().strip()
            rc = stdout.channel.recv_exit_status()
            return rc == 0, out, err
        except Exception as exc:
            return False, "", str(exc)
        finally:
            client.close()

    # Local docker
    if not shutil.which("docker"):
        return False, "", "docker not found in PATH"
    try:
        result = subprocess.run(
            ["docker", *args], capture_output=True, text=True, timeout=timeout
        )
        return result.returncode == 0, result.stdout.strip(), result.stderr.strip()
    except subprocess.TimeoutExpired:
        return False, "", "Command timed out"
    except Exception as exc:
        return False, "", str(exc)


def _k8s_list_resources(profile: dict) -> list[dict]:
    """List K8s resources for the profile's namespace.

    Uses the profile's ``kubeconfig`` and ``namespace`` settings so that
    monitoring works for any cloud (AKS, EKS, GKE) without relying on the
    operator's default kubectl context.
    """
    ns = profile.get("target", {}).get("namespace", "crm-prod")
    kubeconfig = _get_kubeconfig(profile)
    resources = []
    try:
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
                image = ""
                if kind == "Pod":
                    status_info = item.get("status", {}).get("phase", "")
                    # Extract first container image for version-info
                    containers = item.get("spec", {}).get("containers", [])
                    if containers:
                        image = containers[0].get("image", "")
                elif kind in ("Deployment", "StatefulSet", "ReplicaSet"):
                    spec_r = item.get("spec", {}).get("replicas", 0)
                    ready_r = item.get("status", {}).get("readyReplicas", 0)
                    status_info = f"{ready_r}/{spec_r} ready"
                elif kind == "Service":
                    status_info = item.get("spec", {}).get("type", "")
                resources.append({
                    "kind": kind,
                    "name": name,
                    "namespace": ns,
                    "status": status_info,
                    "image": image,
                })
    except Exception:  # noqa: BLE001
        pass
    return resources


def _check_health(profile: dict) -> bool:
    """Check the API health endpoint for the profiled environment."""
    import urllib.request  # noqa: PLC0415
    url = _build_health_url(profile)
    try:
        urllib.request.urlopen(url, timeout=5)
        return True
    except Exception:  # noqa: BLE001
        return False


def _profile_from_request() -> tuple[dict, str]:
    """Convenience: resolve profile from ``?profile=`` query-param."""
    return _resolve_profile(request.args.get("profile"))


def _kubectl_cmd_profile(profile: dict, *args, timeout: int = 30):
    """Run a kubectl command with the profile's kubeconfig/namespace.

    Returns ``(ok, stdout, stderr)`` — same contract as ``_docker_cmd_profile``.
    """
    kubeconfig = _get_kubeconfig(profile)
    ns = profile.get("target", {}).get("namespace", "crm-prod")
    cmd = ["kubectl"]
    if kubeconfig:
        cmd += ["--kubeconfig", kubeconfig]
    cmd += [f"-n={ns}"] + [str(a) for a in args]
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
        return result.returncode == 0, result.stdout.strip(), result.stderr.strip()
    except subprocess.TimeoutExpired:
        return False, "", "Command timed out"
    except Exception as exc:
        return False, "", str(exc)


def _list_docker_containers(profile: dict, running_only: bool = False) -> list[dict]:
    """List CRM Docker containers for the profiled environment.

    Runs ``docker ps`` locally or over SSH depending on the profile's target host.
    """
    flag = "--filter"
    fmt_arg = "{{json .}}"
    ps_args = ["ps"]
    if not running_only:
        ps_args.append("-a")
    ps_args += [flag, DOCKER_FILTER_CRM, "--format", fmt_arg]

    ok, out, _ = _docker_cmd_profile(profile, *ps_args, timeout=15)
    containers: list[dict] = []
    if ok and out:
        for line in out.splitlines():
            if line.strip():
                try:
                    containers.append(json.loads(line))
                except json.JSONDecodeError:
                    continue
    return containers


@day2_bp.route("/api/day2/status", methods=["GET"])
def day2_status():
    """Quick status — profile-aware."""
    profile, pname = _profile_from_request()
    runtime = _detect_runtime(profile)
    api_healthy = _check_health(profile)
    meta = profile.get("meta", {})

    if runtime == "kubernetes":
        resources = _k8s_list_resources(profile)
        running_count = sum(1 for r in resources if r["kind"] == "Pod" and r["status"] == "Running")
        return jsonify({
            "crm_version": meta.get("crm_version", "unknown"),
            "profile_name": pname,
            "runtime": "kubernetes",
            "resources": resources,
            "resource_count": len(resources),
            "running_pods": running_count,
            "containers": [],
            "container_count": running_count,
            "health": {"api": api_healthy, "db": False, "redis": False},
        })

    # Docker Compose
    containers = _list_docker_containers(profile)
    return jsonify({
        "crm_version": meta.get("crm_version", "unknown"),
        "profile_name": pname,
        "runtime": "docker_compose",
        "containers": containers,
        "container_count": len(containers),
        "health": {
            "api": api_healthy,
            "db": any("mariadb" in str(c).lower() or "mysql" in str(c).lower() for c in containers),
            "redis": any("redis" in str(c).lower() for c in containers),
        },
    })


@day2_bp.route("/api/day2/versions", methods=["GET"])
def day2_versions():
    profile = _get_profile()
    mgr = UpgradeManager(Path.cwd(), profile)
    return jsonify(mgr.list_available_versions())


@day2_bp.route("/api/day2/upgrade", methods=["POST"])
def day2_upgrade():
    data = request.json or {}
    target_version = data.get("target_version", "latest")
    backup = data.get("backup", True)
    dry_run = data.get("dry_run", False)
    profile = _get_profile()
    job_id = f"upg_{int(time.time())}"

    def run():
        mgr = UpgradeManager(Path.cwd(), profile, dry_run=dry_run)
        result = mgr.upgrade(target_version, backup=backup)
        _upgrade_jobs[job_id]["result"] = result.to_dict()
        _upgrade_jobs[job_id]["done"] = True

    _upgrade_jobs[job_id] = {"done": False, "result": None, "started": time.time()}
    threading.Thread(target=run, daemon=True).start()
    return jsonify({"message": "Upgrade started", "job_id": job_id})


@day2_bp.route("/api/day2/upgrade/<job_id>/status", methods=["GET"])
def day2_upgrade_status(job_id: str):
    if job_id not in _upgrade_jobs:
        return jsonify({"error": "job not found"}), 404
    job = _upgrade_jobs[job_id]
    return jsonify({"done": job["done"], "result": job.get("result"), "elapsed": round(time.time() - job["started"], 1)})


@day2_bp.route("/api/day2/snapshots", methods=["GET"])
def day2_snapshots():
    profile = _get_profile()
    mgr = RollbackManager(Path.cwd(), profile)
    return jsonify(mgr.list_snapshots())


@day2_bp.route("/api/day2/rollback", methods=["POST"])
def day2_rollback():
    snapshot_id = (request.json or {}).get("snapshot_id", "")
    profile = _get_profile()
    mgr = RollbackManager(Path.cwd(), profile)
    result = mgr.restore_snapshot(snapshot_id)
    return jsonify(result.to_dict())


@day2_bp.route("/api/day2/snapshots/<snapshot_id>", methods=["DELETE"])
def day2_delete_snapshot(snapshot_id: str):
    profile = _get_profile()
    mgr = RollbackManager(Path.cwd(), profile)
    ok = mgr.delete_snapshot(snapshot_id)
    return jsonify({"deleted": ok})


@day2_bp.route("/api/day2/scale", methods=["POST"])
def day2_scale():
    data = request.json or {}
    service = data.get("service", "crm-api")
    replicas = int(data.get("replicas", 1))
    profile = _get_profile()
    mgr = ScaleManager(Path.cwd(), profile)
    result = mgr.scale(service, replicas)
    return jsonify(result.to_dict())


@day2_bp.route("/api/day2/rotate-secret", methods=["POST"])
def day2_rotate_secret():
    secret_type = (request.json or {}).get("secret_type", "jwt")
    profile = _get_profile()
    rotator = SecretRotator(Path.cwd(), profile)
    if secret_type == "all":
        result = rotator.rotate_all()
    elif secret_type == "db_password":
        result = rotator.rotate_db_password()
    else:
        result = rotator.rotate_jwt_secret()
    return jsonify(result.to_dict())


# ---------------------------------------------------------------------------
# Container-level controls (profile-aware)
# ---------------------------------------------------------------------------

def _docker_cmd(*args, timeout: int = 30):
    """Run a LOCAL docker command and return (ok, stdout, stderr).

    Kept as a fallback for callers that don't have a profile context.
    """
    if not shutil.which("docker"):
        return False, "", "docker not found in PATH"
    try:
        result = subprocess.run(
            ["docker", *args], capture_output=True, text=True, timeout=timeout
        )
        return result.returncode == 0, result.stdout.strip(), result.stderr.strip()
    except subprocess.TimeoutExpired:
        return False, "", "Command timed out"
    except Exception as exc:
        return False, "", str(exc)


def _safe_name(name: str) -> str:
    """Strip leading slashes docker sometimes prepends to Names."""
    return name.lstrip("/").split(",")[0].strip()


@day2_bp.route("/api/day2/container/<name>/start", methods=["POST"])
def container_start(name: str):
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    safe = _safe_name(name)
    if runtime == "kubernetes":
        # K8s: no direct "start" — scale the owning deployment to 1
        ok, out, err = _kubectl_cmd_profile(profile, "scale", f"deployment/{safe}", "--replicas=1")
    else:
        ok, out, err = _docker_cmd_profile(profile, "start", safe)
    return jsonify({"success": ok, "output": out or err})


@day2_bp.route("/api/day2/container/<name>/stop", methods=["POST"])
def container_stop(name: str):
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    safe = _safe_name(name)
    if runtime == "kubernetes":
        # K8s: scale the deployment to 0
        ok, out, err = _kubectl_cmd_profile(profile, "scale", f"deployment/{safe}", "--replicas=0")
    else:
        ok, out, err = _docker_cmd_profile(profile, "stop", "--time", "10", safe)
    return jsonify({"success": ok, "output": out or err})


@day2_bp.route("/api/day2/container/<name>/restart", methods=["POST"])
def container_restart(name: str):
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    safe = _safe_name(name)
    if runtime == "kubernetes":
        # K8s: rollout restart for a deployment, or delete pod to restart
        ok, out, err = _kubectl_cmd_profile(profile, "rollout", "restart", f"deployment/{safe}")
        if not ok:
            # Might be a pod name, try deleting it to let the ReplicaSet recreate
            ok, out, err = _kubectl_cmd_profile(profile, "delete", "pod", safe)
    else:
        ok, out, err = _docker_cmd_profile(profile, "restart", "--time", "10", safe)
    return jsonify({"success": ok, "output": out or err})


@day2_bp.route("/api/day2/container/<name>/logs", methods=["GET"])
def container_logs_tail(name: str):
    lines = request.args.get("lines", "150")
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    safe = _safe_name(name)
    if runtime == "kubernetes":
        ok, out, err = _kubectl_cmd_profile(
            profile, "logs", "--tail", str(lines), "--timestamps", safe
        )
    else:
        ok, out, err = _docker_cmd_profile(
            profile, "logs", "--tail", str(lines), "--timestamps", safe
        )
    return jsonify({"success": ok, "logs": out if ok else err})


@day2_bp.route("/api/day2/container/<name>/inspect", methods=["GET"])
def container_inspect(name: str):
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    safe = _safe_name(name)
    if runtime == "kubernetes":
        ok, out, _ = _kubectl_cmd_profile(profile, "get", "pod", safe, "-o", "json")
        if not ok:
            return jsonify({"error": "pod not found"}), 404
        try:
            data = json.loads(out)
            return jsonify({"container": data})
        except Exception as exc:
            return jsonify({"error": str(exc)}), 500
    else:
        ok, out, _ = _docker_cmd_profile(profile, "inspect", safe)
        if not ok:
            return jsonify({"error": "container not found"}), 404
        try:
            data = json.loads(out)
            return jsonify({"container": data[0] if data else {}})
        except Exception as exc:
            return jsonify({"error": str(exc)}), 500


# ---------------------------------------------------------------------------
# Stack-level controls
# ---------------------------------------------------------------------------

@day2_bp.route("/api/day2/stack/stop", methods=["POST"])
def stack_stop():
    """Stop all running crm-* containers/pods — profile- and runtime-aware."""
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    try:
        if runtime == "kubernetes":
            # Scale all deployments to 0
            ok, out, _ = _kubectl_cmd_profile(profile, "get", "deployments", "-o", "jsonpath={.items[*].metadata.name}")
            names = out.split() if ok and out else []
            if not names:
                return jsonify({"success": True, "stopped": [], "message": "No CRM deployments found."})
            stopped, errors = [], []
            for dep in names:
                ok2, _, _ = _kubectl_cmd_profile(profile, "scale", f"deployment/{dep}", "--replicas=0")
                (stopped if ok2 else errors).append(dep)
            return jsonify({"success": not errors, "stopped": stopped, "errors": errors})
        else:
            ok, out, _ = _docker_cmd_profile(profile, "ps", "--filter", DOCKER_FILTER_CRM, "-q")
            ids = [x.strip() for x in (out or "").splitlines() if x.strip()] if ok else []
            if not ids:
                return jsonify({"success": True, "stopped": [], "message": "No running CRM containers."})
            stopped, errors = [], []
            for cid in ids:
                ok2, _, _ = _docker_cmd_profile(profile, "stop", "--time", "15", cid)
                (stopped if ok2 else errors).append(cid)
            return jsonify({"success": not errors, "stopped": stopped, "errors": errors})
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


@day2_bp.route("/api/day2/stack/start", methods=["POST"])
def stack_start():
    """Start all stopped crm-* containers or scale up K8s pods — profile-aware."""
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    try:
        if runtime == "kubernetes":
            # Scale all deployments to 1 (restart from 0)
            ok, out, _ = _kubectl_cmd_profile(profile, "get", "deployments", "-o", "jsonpath={.items[*].metadata.name}")
            names = out.split() if ok and out else []
            if not names:
                return jsonify({"success": True, "started": [], "message": "No CRM deployments found."})
            started, errors = [], []
            for dep in names:
                ok2, _, _ = _kubectl_cmd_profile(profile, "scale", f"deployment/{dep}", "--replicas=1")
                (started if ok2 else errors).append(dep)
            return jsonify({"success": not errors, "started": started, "errors": errors})
        else:
            ok, out, _ = _docker_cmd_profile(
                profile, "ps", "-a", "--filter", DOCKER_FILTER_CRM,
                "--filter", "status=exited", "-q",
            )
            ids = [x.strip() for x in (out or "").splitlines() if x.strip()] if ok else []
            if not ids:
                return jsonify({"success": True, "started": [], "message": "No stopped CRM containers found."})
            started, errors = [], []
            for cid in ids:
                ok2, _, _ = _docker_cmd_profile(profile, "start", cid)
                (started if ok2 else errors).append(cid)
            return jsonify({"success": not errors, "started": started, "errors": errors})
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


@day2_bp.route("/api/day2/stack/restart", methods=["POST"])
def stack_restart():
    """Restart all running crm-* containers/pods — profile-aware."""
    profile, _ = _profile_from_request()
    runtime = _detect_runtime(profile)
    try:
        if runtime == "kubernetes":
            # Rollout restart all deployments
            ok, out, _ = _kubectl_cmd_profile(profile, "get", "deployments", "-o", "jsonpath={.items[*].metadata.name}")
            names = out.split() if ok and out else []
            if not names:
                return jsonify({"success": True, "restarted": [], "message": "No CRM deployments found."})
            restarted, errors = [], []
            for dep in names:
                ok2, _, _ = _kubectl_cmd_profile(profile, "rollout", "restart", f"deployment/{dep}")
                (restarted if ok2 else errors).append(dep)
            return jsonify({"success": not errors, "restarted": restarted, "errors": errors})
        else:
            ok, out, _ = _docker_cmd_profile(profile, "ps", "--filter", DOCKER_FILTER_CRM, "-q")
            ids = [x.strip() for x in (out or "").splitlines() if x.strip()] if ok else []
            if not ids:
                return jsonify({"success": True, "restarted": [], "message": "No running CRM containers."})
            restarted, errors = [], []
            for cid in ids:
                ok2, _, _ = _docker_cmd_profile(profile, "restart", "--time", "10", cid)
                (restarted if ok2 else errors).append(cid)
            return jsonify({"success": not errors, "restarted": restarted, "errors": errors})
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


# ---------------------------------------------------------------------------
# Enhanced status with all (stopped + running) containers
# ---------------------------------------------------------------------------

@day2_bp.route("/api/day2/status/all", methods=["GET"])
def day2_status_all():
    """Return all CRM containers/resources (running and stopped) with rich metadata.

    Profile-aware: checks the profiled environment's host (local/remote Docker
    or Kubernetes) via ``?profile=<name>``.
    """
    profile, pname = _profile_from_request()
    runtime = _detect_runtime(profile)
    meta = profile.get("meta", {})
    api_healthy = _check_health(profile)

    if runtime == "kubernetes":
        resources = _k8s_list_resources(profile)
        running_pods = sum(1 for r in resources if r["kind"] == "Pod" and r["status"] == "Running")
        return jsonify({
            "crm_version": meta.get("crm_version", "unknown"),
            "profile_name": pname,
            "runtime": "kubernetes",
            "deploy_host": _get_deploy_host(profile),
            "environment_type": profile.get("target", {}).get("environment_type",
                                profile.get("environment_type", "—")),
            "resources": resources,
            "containers": [],
            "container_count": running_pods,
            "total_containers": len(resources),
            "health": {"api": api_healthy, "db": False, "redis": False},
        })

    # Docker Compose
    containers = _list_docker_containers(profile)
    return jsonify({
        "crm_version": meta.get("crm_version", "unknown"),
        "profile_name": pname,
        "runtime": "docker_compose",
        "deploy_host": _get_deploy_host(profile),
        "environment_type": profile.get("target", {}).get("environment_type",
                            profile.get("environment_type", "—")),
        "containers": containers,
        "container_count": sum(1 for c in containers if "Up" in str(c.get("Status", c.get("status", "")))),
        "total_containers": len(containers),
        "health": {
            "api": api_healthy,
            "db": any("mariadb" in str(c).lower() or "mysql" in str(c).lower() for c in containers),
            "redis": any("redis" in str(c).lower() for c in containers),
        },
    })


# ---------------------------------------------------------------------------
# Detailed version info
# ---------------------------------------------------------------------------

@day2_bp.route("/api/day2/version-info", methods=["GET"])
def day2_version_info():  # NOSONAR - complexity acceptable for version aggregation endpoint
    """Return profile metadata + running image tags — profile-aware."""
    import urllib.request  # noqa: PLC0415
    profile, pname = _profile_from_request()
    meta = profile.get("meta", {})
    runtime = _detect_runtime(profile)
    base_url = _build_api_base_url(profile)

    # Running container/resource images
    images: dict = {}
    if runtime == "kubernetes":
        for res in _k8s_list_resources(profile):
            if res["kind"] == "Pod":
                images[res["name"]] = {"image": res.get("image", "—"), "status": res["status"]}
    else:
        ok, out, _ = _docker_cmd_profile(
            profile, "ps", "--filter", DOCKER_FILTER_CRM, "--format",
            "{{.Names}}\t{{.Image}}\t{{.CreatedAt}}\t{{.Status}}",
        )
        if ok and out:
            for line in out.splitlines():
                parts = line.split("\t")
                if len(parts) >= 2:
                    cname = parts[0].lstrip("/")
                    images[cname] = {
                        "image": parts[1],
                        "created": parts[2] if len(parts) > 2 else "",
                        "status": parts[3] if len(parts) > 3 else "",
                    }

    # Live API version
    api_version: str | None = None
    try:
        with urllib.request.urlopen(f"{base_url}/api/version", timeout=3) as resp:
            vdata = json.loads(resp.read().decode())
            api_version = vdata.get("version") or vdata.get("Version")
    except Exception:  # noqa: BLE001
        try:
            health_url = _build_health_url(profile)
            with urllib.request.urlopen(health_url, timeout=3) as resp:
                hdata = json.loads(resp.read().decode())
                api_version = hdata.get("version")
        except Exception:  # noqa: BLE001
            pass

    return jsonify({
        "profile_name": pname,
        "crm_version": meta.get("crm_version", api_version or "unknown"),
        "api_version": api_version,
        "runtime": runtime,
        "deploy_host": _get_deploy_host(profile),
        "environment_type": profile.get("target", {}).get("environment_type", "—"),
        "deployed_at": meta.get("updated_at", meta.get("created_at", "—")),
        "platform": profile.get("platform", profile.get("target", {}).get("platform", "—")),
        "images": images,
    })


# ---------------------------------------------------------------------------
# Deployment run history
# ---------------------------------------------------------------------------

@day2_bp.route("/api/day2/run-history", methods=["GET"])
def day2_run_history():
    """Return last N deployment run history entries."""
    limit = int(request.args.get("limit", 25))
    # Try RunHistoryManager (SQLite)
    try:
        from core.profile import RunHistoryManager  # noqa: PLC0415
        hist = RunHistoryManager()
        rows = hist.list_runs()[:limit]
        serial = []
        for r in rows:
            if hasattr(r, "_asdict"):
                serial.append(r._asdict())
            elif hasattr(r, "__dict__"):
                serial.append(vars(r))
            else:
                serial.append(dict(r))
        return jsonify({"runs": serial})
    except Exception:
        pass
    # Fallback: deploy_history.json
    history_file = Path.home() / CDT_DIR / "deploy_history.json"
    if history_file.exists():
        try:
            entries = json.loads(history_file.read_text())
            return jsonify({"runs": entries[-limit:]})
        except Exception:
            pass
    return jsonify({"runs": []})


# ---------------------------------------------------------------------------
# Post-Install Database Operations (proxy to CRM API)
# ---------------------------------------------------------------------------

def _proxy_crm_api(profile: dict, method: str, path: str, body: dict | None = None, timeout: int = 60):
    """Forward a request to the CRM API and return (status_code, response_dict)."""
    import urllib.request  # noqa: PLC0415
    import urllib.error  # noqa: PLC0415
    base_url = _build_api_base_url(profile)
    url = f"{base_url}{path}"
    payload = json.dumps(body).encode("utf-8") if body else None
    req = urllib.request.Request(url, data=payload, method=method)
    req.add_header("Content-Type", "application/json")
    req.add_header("Accept", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            raw = resp.read().decode("utf-8")
            try:
                data = json.loads(raw)
            except json.JSONDecodeError:
                data = {"raw": raw}
            return resp.status, data
    except urllib.error.HTTPError as exc:
        raw = exc.read().decode("utf-8") if exc.fp else ""
        try:
            data = json.loads(raw)
        except Exception:
            data = {"raw": raw}
        return exc.code, data
    except Exception as exc:
        return 0, {"error": str(exc)}


@day2_bp.route("/api/day2/postinstall/clear-database", methods=["POST"])
def day2_postinstall_clear_database():
    """Clear all data in the CRM database (requires confirmation code)."""
    profile, _ = _profile_from_request()
    code, data = _proxy_crm_api(profile, "POST", "/api/database/clear-data",
                                body={"ConfirmationCode": "DELETE_ALL_DATA"}, timeout=120)
    return jsonify({"success": 200 <= code < 300, "status_code": code, "data": data}), (200 if 200 <= code < 300 else 502)


@day2_bp.route("/api/day2/postinstall/migrate-database", methods=["POST"])
def day2_postinstall_migrate_database():
    """Run EF Core migrations on the CRM database."""
    profile, _ = _profile_from_request()
    code, data = _proxy_crm_api(profile, "POST", "/api/database/migrate", timeout=120)
    return jsonify({"success": 200 <= code < 300, "status_code": code, "data": data}), (200 if 200 <= code < 300 else 502)


@day2_bp.route("/api/day2/postinstall/reseed-data", methods=["POST"])
def day2_postinstall_reseed_data():
    """Reseed core data in the CRM database."""
    profile, _ = _profile_from_request()
    code, data = _proxy_crm_api(profile, "POST", "/api/database/reseed", timeout=120)
    return jsonify({"success": 200 <= code < 300, "status_code": code, "data": data}), (200 if 200 <= code < 300 else 502)


@day2_bp.route("/api/day2/postinstall/seed-master-data", methods=["POST"])
def day2_postinstall_seed_master_data():
    """Seed all master data in order via the CRM API."""
    profile, _ = _profile_from_request()
    code, data = _proxy_crm_api(profile, "POST", "/api/admin/seed/core", timeout=180)
    return jsonify({"success": 200 <= code < 300, "status_code": code, "data": data}), (200 if 200 <= code < 300 else 502)


@day2_bp.route("/api/day2/postinstall/clear-sample-data", methods=["POST"])
def day2_postinstall_clear_sample_data():
    """Clear sample data but keep master data."""
    profile, _ = _profile_from_request()
    code, data = _proxy_crm_api(profile, "DELETE", "/api/sampledata/clear", timeout=120)
    return jsonify({"success": 200 <= code < 300, "status_code": code, "data": data}), (200 if 200 <= code < 300 else 502)


@day2_bp.route("/api/day2/postinstall/seed-sample-data", methods=["POST"])
def day2_postinstall_seed_sample_data():
    """Seed all sample data via the CRM API."""
    profile, _ = _profile_from_request()
    code, data = _proxy_crm_api(profile, "POST", "/api/sampledata/seed", timeout=300)
    return jsonify({"success": 200 <= code < 300, "status_code": code, "data": data}), (200 if 200 <= code < 300 else 502)


@day2_bp.route("/api/day2/postinstall/database-status", methods=["GET"])
def day2_postinstall_database_status():
    """Return CRM database health via the API health endpoint."""
    profile, pname = _profile_from_request()
    api_healthy = _check_health(profile)
    base_url = _build_api_base_url(profile)

    # Try to get detailed DB status from /health/ready
    db_status = "unknown"
    try:
        import urllib.request  # noqa: PLC0415
        with urllib.request.urlopen(f"{base_url}/health/ready", timeout=5) as resp:
            _ = resp.read()  # consume body
            db_status = "healthy" if resp.status == 200 else "degraded"
    except Exception:
        db_status = "unreachable" if not api_healthy else "unknown"

    return jsonify({
        "profile_name": pname,
        "deploy_host": _get_deploy_host(profile),
        "api_healthy": api_healthy,
        "db_status": db_status,
        "base_url": base_url,
    })


# ---------------------------------------------------------------------------
# Test Runner
# ---------------------------------------------------------------------------

_test_jobs: dict = {}

_WORKSPACE_ROOT = str(Path(__file__).resolve().parent.parent.parent.parent.parent)


def _test_runner_thread(job_id: str, test_type: str, base_url: str, cleanup: bool):
    """Run tests in background thread, updating ``_test_jobs[job_id]``."""
    job = _test_jobs[job_id]
    job["status"] = "running"
    job["started_at"] = time.time()
    results: list[dict] = []
    total_pass = 0
    total_fail = 0
    output_lines: list[str] = []

    def _log(line: str):
        output_lines.append(line)
        job["output"] = output_lines

    try:
        # ----- BVT -----
        if test_type in ("bvt", "all"):
            _log("[BVT] Starting health-check tests …")
            bvt_endpoints = [
                ("GET", "/health", 200),
                ("GET", "/health/ready", 200),
                ("GET", "/health/live", 200),
                ("GET", "/api/version", 200),
            ]
            for method, path, expected in bvt_endpoints:
                try:
                    import urllib.request, urllib.error  # noqa: PLC0415,E401
                    req = urllib.request.Request(f"{base_url}{path}", method=method)
                    with urllib.request.urlopen(req, timeout=10) as resp:
                        status = resp.status
                except urllib.error.HTTPError as exc:
                    status = exc.code
                except Exception as exc:
                    status = 0
                    _log(f"  [BVT] {method} {path} -> ERROR: {exc}")
                passed = status == expected
                total_pass += int(passed)
                total_fail += int(not passed)
                results.append({"suite": "bvt", "test": f"{method} {path}", "expected": expected, "actual": status, "passed": passed})
                _log(f"  [BVT] {method} {path} -> {status} {'✓' if passed else '✗'}")
            _log(f"[BVT] Done. {total_pass} passed, {total_fail} failed.")

        # ----- CRUD Loader -----
        if test_type in ("crud_loader", "all"):
            _log("[CRUD_LOADER] Starting test_data_loader.py …")
            loader_script = Path(_WORKSPACE_ROOT) / "scripts" / "test_data_loader.py"
            if not loader_script.exists():
                _log(f"  [CRUD_LOADER] Script not found: {loader_script}")
                total_fail += 1
                results.append({"suite": "crud_loader", "test": "script_exists", "passed": False, "error": "script not found"})
            else:
                cmd = [
                    sys.executable, str(loader_script),
                    "--base-url", base_url,
                ]
                if cleanup:
                    cmd.append("--cleanup")
                try:
                    proc = subprocess.Popen(
                        cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                        text=True, cwd=_WORKSPACE_ROOT,
                    )
                    for line in iter(proc.stdout.readline, ""):
                        _log(f"  [CRUD_LOADER] {line.rstrip()}")
                    proc.wait(timeout=600)
                    passed = proc.returncode == 0
                    total_pass += int(passed)
                    total_fail += int(not passed)
                    results.append({"suite": "crud_loader", "test": "test_data_loader", "passed": passed, "returncode": proc.returncode})
                    _log(f"[CRUD_LOADER] Done. Exit code: {proc.returncode}")
                except subprocess.TimeoutExpired:
                    proc.kill()
                    total_fail += 1
                    results.append({"suite": "crud_loader", "test": "test_data_loader", "passed": False, "error": "timeout"})
                    _log("[CRUD_LOADER] Timed out after 600s.")
                except Exception as exc:
                    total_fail += 1
                    results.append({"suite": "crud_loader", "test": "test_data_loader", "passed": False, "error": str(exc)})
                    _log(f"[CRUD_LOADER] Error: {exc}")

        # ----- E2E (Playwright) -----
        if test_type in ("e2e", "all"):
            _log("[E2E] Starting Playwright tests …")
            e2e_dir = Path(_WORKSPACE_ROOT) / "e2e-tests"
            if not e2e_dir.exists():
                _log(f"  [E2E] e2e-tests directory not found: {e2e_dir}")
                total_fail += 1
                results.append({"suite": "e2e", "test": "e2e_dir_exists", "passed": False, "error": "directory not found"})
            else:
                npx = shutil.which("npx")
                if not npx:
                    _log("  [E2E] npx not found — skipping Playwright tests.")
                    results.append({"suite": "e2e", "test": "npx_available", "passed": False, "error": "npx not found"})
                    total_fail += 1
                else:
                    try:
                        env = {**__import__("os").environ, "BASE_URL": base_url}
                        proc = subprocess.Popen(
                            [npx, "playwright", "test", "--reporter=json"],
                            stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                            text=True, cwd=str(e2e_dir), env=env,
                        )
                        for line in iter(proc.stdout.readline, ""):
                            _log(f"  [E2E] {line.rstrip()}")
                        proc.wait(timeout=600)
                        passed = proc.returncode == 0
                        total_pass += int(passed)
                        total_fail += int(not passed)
                        results.append({"suite": "e2e", "test": "playwright", "passed": passed, "returncode": proc.returncode})
                        _log(f"[E2E] Done. Exit code: {proc.returncode}")
                    except subprocess.TimeoutExpired:
                        proc.kill()
                        total_fail += 1
                        results.append({"suite": "e2e", "test": "playwright", "passed": False, "error": "timeout"})
                        _log("[E2E] Timed out after 600s.")
                    except Exception as exc:
                        total_fail += 1
                        results.append({"suite": "e2e", "test": "playwright", "passed": False, "error": str(exc)})
                        _log(f"[E2E] Error: {exc}")

        job["status"] = "completed"

    except Exception as exc:
        _log(f"[ERROR] Unhandled: {exc}")
        job["status"] = "failed"
        job["error"] = str(exc)

    job["finished_at"] = time.time()
    job["elapsed"] = round(job["finished_at"] - job["started_at"], 2)
    job["results"] = results
    job["total_pass"] = total_pass
    job["total_fail"] = total_fail
    job["done"] = True


@day2_bp.route("/api/day2/testrunner/run", methods=["POST"])
def day2_testrunner_run():
    """Start an async test run against the deployed CRM."""
    data = request.json or {}
    test_type = data.get("test_type", "bvt")
    if test_type not in ("bvt", "e2e", "crud_loader", "all"):
        return jsonify({"error": f"Invalid test_type '{test_type}'. Use bvt|e2e|crud_loader|all"}), 400
    cleanup = bool(data.get("cleanup", False))

    profile, pname = _profile_from_request()
    base_url = data.get("base_url") or _build_api_base_url(profile)
    job_id = f"test_{int(time.time())}_{test_type}"

    _test_jobs[job_id] = {
        "job_id": job_id,
        "test_type": test_type,
        "base_url": base_url,
        "profile_name": pname,
        "done": False,
        "status": "pending",
        "output": [],
        "results": [],
        "total_pass": 0,
        "total_fail": 0,
        "started_at": None,
        "finished_at": None,
        "elapsed": 0,
        "error": None,
    }

    threading.Thread(target=_test_runner_thread, args=(job_id, test_type, base_url, cleanup), daemon=True).start()
    return jsonify({"message": "Test run started", "job_id": job_id, "test_type": test_type, "base_url": base_url})


@day2_bp.route("/api/day2/testrunner/status/<job_id>", methods=["GET"])
def day2_testrunner_status(job_id: str):
    """Return status / progress for a test run."""
    if job_id not in _test_jobs:
        return jsonify({"error": "Job not found"}), 404
    job = _test_jobs[job_id]
    return jsonify({
        "job_id": job_id,
        "status": job["status"],
        "done": job["done"],
        "total_pass": job["total_pass"],
        "total_fail": job["total_fail"],
        "elapsed": round(time.time() - job["started_at"], 1) if job["started_at"] and not job["done"] else job.get("elapsed", 0),
        "output_tail": job["output"][-30:] if job["output"] else [],
    })


@day2_bp.route("/api/day2/testrunner/results/<job_id>", methods=["GET"])
def day2_testrunner_results(job_id: str):
    """Return full test results for a completed run."""
    if job_id not in _test_jobs:
        return jsonify({"error": "Job not found"}), 404
    job = _test_jobs[job_id]
    return jsonify({
        "job_id": job_id,
        "test_type": job["test_type"],
        "base_url": job["base_url"],
        "profile_name": job["profile_name"],
        "status": job["status"],
        "done": job["done"],
        "total_pass": job["total_pass"],
        "total_fail": job["total_fail"],
        "elapsed": job.get("elapsed", 0),
        "results": job["results"],
        "output": job["output"],
        "error": job.get("error"),
    })


@day2_bp.route("/api/day2/testrunner/cleanup", methods=["POST"])
def day2_testrunner_cleanup():
    """Run cleanup of test data against the deployed CRM."""
    profile, _ = _profile_from_request()
    base_url = (request.json or {}).get("base_url") or _build_api_base_url(profile)
    loader_script = Path(_WORKSPACE_ROOT) / "scripts" / "test_data_loader.py"
    if not loader_script.exists():
        return jsonify({"success": False, "error": f"Script not found: {loader_script}"}), 404
    try:
        result = subprocess.run(
            [sys.executable, str(loader_script), "--base-url", base_url, "--cleanup"],
            capture_output=True, text=True, timeout=300, cwd=_WORKSPACE_ROOT,
        )
        return jsonify({
            "success": result.returncode == 0,
            "returncode": result.returncode,
            "stdout": result.stdout[-5000:] if result.stdout else "",
            "stderr": result.stderr[-2000:] if result.stderr else "",
        })
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


@day2_bp.route("/api/day2/testrunner/history", methods=["GET"])
def day2_testrunner_history():
    """List past test runs (from in-memory store)."""
    limit = int(request.args.get("limit", 25))
    runs = sorted(_test_jobs.values(), key=lambda j: j.get("started_at") or 0, reverse=True)[:limit]
    summary = []
    for j in runs:
        summary.append({
            "job_id": j["job_id"],
            "test_type": j["test_type"],
            "base_url": j["base_url"],
            "profile_name": j["profile_name"],
            "status": j["status"],
            "done": j["done"],
            "total_pass": j["total_pass"],
            "total_fail": j["total_fail"],
            "elapsed": j.get("elapsed", 0),
            "started_at": j.get("started_at"),
        })
    return jsonify({"runs": summary})
