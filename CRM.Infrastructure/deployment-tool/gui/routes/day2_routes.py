#!/usr/bin/env python3
"""Day-2 operations routes."""
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
# Module-level constants (avoid duplicating literals)
# ---------------------------------------------------------------------------
CDT_DIR = ".crm-cdt"
DOCKER_FILTER_CRM = "name=crm"
API_HEALTH_URL = "http://localhost:5000/health"

def _get_profile():
    profile_file = Path.home() / CDT_DIR / "last_profile.json"
    if profile_file.exists():
        return json.loads(profile_file.read_text())
    return {}


@day2_bp.route("/api/day2/status", methods=["GET"])
def day2_status():
    import subprocess, urllib.request
    containers = []
    try:
        result = subprocess.run(["docker", "ps", "--filter", DOCKER_FILTER_CRM, "--format", "{{json .}}"], capture_output=True, text=True, timeout=10)
        for line in result.stdout.strip().splitlines():
            if line.strip():
                try:
                    containers.append(json.loads(line))
                except Exception:
                    pass
    except Exception:
        pass
    api_healthy = False
    try:
        urllib.request.urlopen(API_HEALTH_URL, timeout=3)
        api_healthy = True
    except Exception:
        pass
    profile = _get_profile()
    return jsonify({
        "crm_version": profile.get("meta", {}).get("crm_version", "unknown"),
        "containers": containers,
        "container_count": len(containers),
        "health": {"api": api_healthy, "db": any("mariadb" in str(c) for c in containers), "redis": any("redis" in str(c) for c in containers)},
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
# Container-level controls
# ---------------------------------------------------------------------------

def _docker_cmd(*args, timeout: int = 30):
    """Run a docker command and return (ok, stdout, stderr)."""
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
    ok, out, err = _docker_cmd("start", _safe_name(name))
    return jsonify({"success": ok, "output": out or err})


@day2_bp.route("/api/day2/container/<name>/stop", methods=["POST"])
def container_stop(name: str):
    ok, out, err = _docker_cmd("stop", "--time", "10", _safe_name(name))
    return jsonify({"success": ok, "output": out or err})


@day2_bp.route("/api/day2/container/<name>/restart", methods=["POST"])
def container_restart(name: str):
    ok, out, err = _docker_cmd("restart", "--time", "10", _safe_name(name))
    return jsonify({"success": ok, "output": out or err})


@day2_bp.route("/api/day2/container/<name>/logs", methods=["GET"])
def container_logs_tail(name: str):
    lines = request.args.get("lines", "150")
    ok, out, err = _docker_cmd(
        "logs", "--tail", str(lines), "--timestamps", _safe_name(name)
    )
    return jsonify({"success": ok, "logs": out if ok else err})


@day2_bp.route("/api/day2/container/<name>/inspect", methods=["GET"])
def container_inspect(name: str):
    ok, out, _ = _docker_cmd("inspect", _safe_name(name))
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
    """Stop all running crm-* containers gracefully."""
    try:
        r = subprocess.run(
            ["docker", "ps", "--filter", DOCKER_FILTER_CRM, "-q"],
            capture_output=True, text=True, timeout=10
        )
        ids = [x.strip() for x in r.stdout.strip().splitlines() if x.strip()]
        if not ids:
            return jsonify({"success": True, "stopped": [], "message": "No running CRM containers."})
        stopped, errors = [], []
        for cid in ids:
            ok, _, _ = _docker_cmd("stop", "--time", "15", cid)
            (stopped if ok else errors).append(cid)
        return jsonify({"success": not errors, "stopped": stopped, "errors": errors})
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


@day2_bp.route("/api/day2/stack/start", methods=["POST"])
def stack_start():
    """Start all stopped crm-* containers."""
    try:
        r = subprocess.run(
            ["docker", "ps", "-a", "--filter", DOCKER_FILTER_CRM,
             "--filter", "status=exited", "-q"],
            capture_output=True, text=True, timeout=10
        )
        ids = [x.strip() for x in r.stdout.strip().splitlines() if x.strip()]
        if not ids:
            return jsonify({"success": True, "started": [], "message": "No stopped CRM containers found."})
        started, errors = [], []
        for cid in ids:
            ok, _, _ = _docker_cmd("start", cid)
            (started if ok else errors).append(cid)
        return jsonify({"success": not errors, "started": started, "errors": errors})
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


@day2_bp.route("/api/day2/stack/restart", methods=["POST"])
def stack_restart():
    """Restart all running crm-* containers."""
    try:
        r = subprocess.run(
            ["docker", "ps", "--filter", DOCKER_FILTER_CRM, "-q"],
            capture_output=True, text=True, timeout=10
        )
        ids = [x.strip() for x in r.stdout.strip().splitlines() if x.strip()]
        if not ids:
            return jsonify({"success": True, "restarted": [], "message": "No running CRM containers."})
        restarted, errors = [], []
        for cid in ids:
            ok, _, _ = _docker_cmd("restart", "--time", "10", cid)
            (restarted if ok else errors).append(cid)
        return jsonify({"success": not errors, "restarted": restarted, "errors": errors})
    except Exception as exc:
        return jsonify({"success": False, "error": str(exc)}), 500


# ---------------------------------------------------------------------------
# Enhanced status with all (stopped + running) containers
# ---------------------------------------------------------------------------

@day2_bp.route("/api/day2/status/all", methods=["GET"])
def day2_status_all():
    """Return all CRM containers (running and stopped) with rich metadata."""
    import urllib.request
    containers = []
    try:
        fmt = "{{json .}}"
        r = subprocess.run(
            ["docker", "ps", "-a", "--filter", DOCKER_FILTER_CRM, "--format", fmt],
            capture_output=True, text=True, timeout=15
        )
        for line in r.stdout.strip().splitlines():
            if line.strip():
                try:
                    c = json.loads(line)
                    containers.append(c)
                except Exception:
                    pass
    except Exception:
        pass

    # API health check
    api_healthy = False
    try:
        urllib.request.urlopen(API_HEALTH_URL, timeout=3)
        api_healthy = True
    except Exception:
        pass

    profile = _get_profile()
    meta = profile.get("meta", {})

    # Fetch active profile name
    active_name_file = Path.home() / CDT_DIR / "active_profile_name.txt"
    active_name = active_name_file.read_text().strip() if active_name_file.exists() else None

    return jsonify({
        "crm_version": meta.get("crm_version", "unknown"),
        "profile_name": active_name or meta.get("profile_name", "—"),
        "environment_type": profile.get("target", {}).get("environment_type",
                            profile.get("environment_type", "—")),
        "containers": containers,
        "container_count": sum(1 for c in containers if "Up" in str(c.get("Status", ""))),
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
    """Return profile metadata + running image tags for all CRM containers."""
    import urllib.request
    profile = _get_profile()
    meta = profile.get("meta", {})

    # Running container images
    images: dict = {}
    try:
        r = subprocess.run(
            ["docker", "ps", "--filter", DOCKER_FILTER_CRM, "--format",
             "{{.Names}}\t{{.Image}}\t{{.CreatedAt}}\t{{.Status}}"],
            capture_output=True, text=True, timeout=10
        )
        for line in r.stdout.strip().splitlines():
            parts = line.split("\t")
            if len(parts) >= 2:
                cname = parts[0].lstrip("/")
                images[cname] = {
                    "image": parts[1],
                    "created": parts[2] if len(parts) > 2 else "",
                    "status": parts[3] if len(parts) > 3 else "",
                }
    except Exception:
        pass

    # Live API version
    api_version: str | None = None
    try:
        with urllib.request.urlopen("http://localhost:5000/api/version", timeout=3) as resp:
            vdata = json.loads(resp.read().decode())
            api_version = vdata.get("version") or vdata.get("Version")
    except Exception:
        try:
            with urllib.request.urlopen(API_HEALTH_URL, timeout=3) as resp:
                hdata = json.loads(resp.read().decode())
                api_version = hdata.get("version")
        except Exception:
            pass

    active_name_file = Path.home() / CDT_DIR / "active_profile_name.txt"
    active_name = active_name_file.read_text().strip() if active_name_file.exists() else None

    return jsonify({
        "profile_name": active_name or meta.get("profile_name", "—"),
        "crm_version": meta.get("crm_version", api_version or "unknown"),
        "api_version": api_version,
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
        rows = hist.list_runs(limit=limit)
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
