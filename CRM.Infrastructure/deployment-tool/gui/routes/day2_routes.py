#!/usr/bin/env python3
"""Day-2 operations routes."""
import sys
import json
import time
import threading
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from flask import Blueprint, request, jsonify
from day2.upgrade import UpgradeManager
from day2.rollback import RollbackManager
from day2.scale import ScaleManager
from day2.rotate_secrets import SecretRotator

day2_bp = Blueprint("day2", __name__)
_upgrade_jobs: dict = {}


def _get_profile():
    profile_file = Path.home() / ".crm-cdt" / "last_profile.json"
    if profile_file.exists():
        return json.loads(profile_file.read_text())
    return {}


@day2_bp.route("/api/day2/status", methods=["GET"])
def day2_status():
    import subprocess, urllib.request
    containers = []
    try:
        result = subprocess.run(["docker", "ps", "--filter", "name=crm", "--format", "{{json .}}"], capture_output=True, text=True, timeout=10)
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
        urllib.request.urlopen("http://localhost:5000/health", timeout=3)
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
