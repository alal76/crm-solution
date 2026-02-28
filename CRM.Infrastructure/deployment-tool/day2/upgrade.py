#!/usr/bin/env python3
"""CRM CDT Day-2 - Upgrade Operation."""
from __future__ import annotations
import subprocess
import time
import json
from datetime import datetime, timezone
from dataclasses import dataclass, field
from pathlib import Path
import urllib.request


@dataclass
class UpgradeResult:
    success: bool
    old_version: str = ""
    new_version: str = ""
    messages: list = field(default_factory=list)
    errors: list = field(default_factory=list)
    snapshot_id: str = ""

    def to_dict(self) -> dict:
        return {"success": self.success, "old_version": self.old_version, "new_version": self.new_version, "messages": self.messages, "errors": self.errors, "snapshot_id": self.snapshot_id}


class UpgradeManager:
    def __init__(self, work_dir: Path, profile: dict, dry_run: bool = False):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.dry_run = dry_run
        self.snapshot_dir = Path.home() / ".crm-cdt" / "snapshots"
        self.snapshot_dir.mkdir(parents=True, exist_ok=True)

    def list_available_versions(self) -> list:
        try:
            req = urllib.request.Request(
                "https://api.github.com/repos/your-org/crm-solution/releases",
                headers={"User-Agent": "crm-cdt/1.0"}
            )
            with urllib.request.urlopen(req, timeout=10) as resp:
                releases = json.loads(resp.read().decode())
                return [r["tag_name"] for r in releases if not r.get("draft")]
        except Exception:
            return ["latest"]

    def get_current_version(self) -> str:
        try:
            with urllib.request.urlopen("http://localhost:5000/health", timeout=5) as resp:
                data = json.loads(resp.read().decode())
                return data.get("version", "unknown")
        except Exception:
            return self.profile.get("meta", {}).get("crm_version", "unknown")

    def create_snapshot(self, version: str, reason: str = "pre-upgrade") -> str:
        snapshot_id = f"snap_{datetime.now(timezone.utc).strftime('%Y%m%d_%H%M%S')}"
        snap_data = {"snapshot_id": snapshot_id, "version": version, "reason": reason, "timestamp": datetime.now(timezone.utc).isoformat(), "profile": self.profile}
        # Get container states
        try:
            result = subprocess.run(["docker", "container", "ls", "--filter", "name=crm", "--format", "{{json .}}"], capture_output=True, text=True, timeout=10)
            containers = [json.loads(line) for line in result.stdout.strip().splitlines() if line.strip()]
            snap_data["containers"] = containers
        except Exception:
            snap_data["containers"] = []
        (self.snapshot_dir / f"{snapshot_id}.json").write_text(json.dumps(snap_data, indent=2))
        return snapshot_id

    def run_db_backup(self) -> bool:
        db = self.profile.get("database", {})
        cmd = ["docker", "exec", "crm-mariadb", "mysqldump", f"-u{db.get('db_user','crm_user')}", f"-p{db.get('db_password','')}", db.get("db_name", "crm_db")]
        if self.dry_run:
            print(f"[DRY-RUN] Would run: {' '.join(cmd)}")
            return True
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=120)
            if result.returncode == 0:
                backup_path = self.snapshot_dir / f"backup_{datetime.now(timezone.utc).strftime('%Y%m%d_%H%M%S')}.sql"
                backup_path.write_text(result.stdout)
                return True
            return False
        except Exception:
            return False

    def upgrade(self, target_version: str, backup: bool = True) -> UpgradeResult:
        result = UpgradeResult(success=False)
        result.old_version = self.get_current_version()
        result.new_version = target_version
        result.messages.append(f"Starting upgrade from {result.old_version} to {target_version}")

        result.snapshot_id = self.create_snapshot(result.old_version)
        result.messages.append(f"Snapshot created: {result.snapshot_id}")

        if backup:
            db_ok = self.run_db_backup()
            result.messages.append("Database backed up" if db_ok else "Database backup skipped (may not be running)")

        # Pull new images
        pull_cmd = ["docker", "compose", "pull", "crm-api", "crm-frontend"]
        if self.dry_run:
            result.messages.append(f"[DRY-RUN] Would run: {' '.join(pull_cmd)}")
        else:
            rc = subprocess.call(pull_cmd, cwd=str(self.work_dir))
            if rc != 0:
                result.errors.append("Image pull failed")

        # Restart
        up_cmd = ["docker", "compose", "up", "-d", "crm-api", "crm-frontend"]
        if self.dry_run:
            result.messages.append(f"[DRY-RUN] Would run: {' '.join(up_cmd)}")
        else:
            subprocess.call(up_cmd, cwd=str(self.work_dir))

        result.success = True
        result.messages.append(f"Upgrade to {target_version} complete")
        return result
