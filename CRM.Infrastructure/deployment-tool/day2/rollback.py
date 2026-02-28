#!/usr/bin/env python3
"""CRM CDT Day-2 - Rollback Operation."""
from __future__ import annotations
from dataclasses import dataclass, field
from pathlib import Path
import json, subprocess


@dataclass
class RollbackResult:
    success: bool
    snapshot_id: str = ""
    messages: list = field(default_factory=list)
    errors: list = field(default_factory=list)

    def to_dict(self) -> dict:
        return {"success": self.success, "snapshot_id": self.snapshot_id, "messages": self.messages, "errors": self.errors}


class RollbackManager:
    def __init__(self, work_dir: Path, profile: dict, dry_run: bool = False):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.dry_run = dry_run
        self.snapshot_dir = Path.home() / ".crm-cdt" / "snapshots"
        self.snapshot_dir.mkdir(parents=True, exist_ok=True)

    def list_snapshots(self) -> list:
        snaps = []
        for f in sorted(self.snapshot_dir.glob("snap_*.json"), reverse=True):
            try:
                data = json.loads(f.read_text())
                snaps.append(data)
            except Exception:
                pass
        return snaps

    def restore_snapshot(self, snapshot_id: str) -> RollbackResult:
        result = RollbackResult(success=False, snapshot_id=snapshot_id)
        snap_file = self.snapshot_dir / f"{snapshot_id}.json"
        if not snap_file.exists():
            result.errors.append(f"Snapshot not found: {snapshot_id}")
            return result

        snap = json.loads(snap_file.read_text())
        version = snap.get("version", "latest")
        result.messages.append(f"Restoring snapshot {snapshot_id} (version {version})")

        if self.dry_run:
            result.messages.append(f"[DRY-RUN] Would restore version {version}")
            result.success = True
            return result

        subprocess.call(["docker", "compose", "pull", "crm-api", "crm-frontend"], cwd=str(self.work_dir))
        subprocess.call(["docker", "compose", "up", "-d", "crm-api", "crm-frontend"], cwd=str(self.work_dir))
        result.messages.append("Rollback complete")
        result.success = True
        return result

    def delete_snapshot(self, snapshot_id: str) -> bool:
        snap_file = self.snapshot_dir / f"{snapshot_id}.json"
        if snap_file.exists():
            snap_file.unlink()
            return True
        return False
