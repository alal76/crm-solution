#!/usr/bin/env python3
"""CRM CDT Day-2 - Scale Operation."""
from __future__ import annotations
from dataclasses import dataclass, field
from pathlib import Path
import subprocess


@dataclass
class ScaleResult:
    success: bool
    service: str = ""
    replicas: int = 0
    messages: list = field(default_factory=list)

    def to_dict(self) -> dict:
        return {"success": self.success, "service": self.service, "replicas": self.replicas, "messages": self.messages}


class ScaleManager:
    def __init__(self, work_dir: Path, profile: dict, dry_run: bool = False):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.dry_run = dry_run
        self.namespace = profile.get("architecture", {}).get("namespace", "crm-prod")

    def get_current_replicas(self, service: str) -> int:
        runtime = self.profile.get("architecture", {}).get("container_runtime", "docker_compose")
        try:
            if runtime == "kubernetes":
                result = subprocess.run(["kubectl", "get", "deployment", service, "-o", "jsonpath={.spec.replicas}", f"-n={self.namespace}"], capture_output=True, text=True, timeout=10)
                return int(result.stdout.strip()) if result.returncode == 0 and result.stdout.strip() else 1
            else:
                result = subprocess.run(["docker", "compose", "ps", "-q", service], capture_output=True, text=True, timeout=10, cwd=str(self.work_dir))
                return len([l for l in result.stdout.strip().splitlines() if l.strip()])
        except Exception:
            return 1

    def scale(self, service: str, replicas: int) -> ScaleResult:
        result = ScaleResult(success=False, service=service, replicas=replicas)
        runtime = self.profile.get("architecture", {}).get("container_runtime", "docker_compose")
        if self.dry_run:
            result.messages.append(f"[DRY-RUN] Would scale {service} to {replicas} replica(s)")
            result.success = True
            return result
        try:
            if runtime == "kubernetes":
                rc = subprocess.call(["kubectl", "scale", f"deployment/{service}", f"--replicas={replicas}", f"-n={self.namespace}"])
            else:
                rc = subprocess.call(["docker", "compose", "up", "-d", "--scale", f"{service}={replicas}", service], cwd=str(self.work_dir))
            result.success = rc == 0
            result.messages.append(f"Scaled {service} to {replicas} replica(s)")
        except Exception as e:
            result.errors = [str(e)] if hasattr(result, "errors") else []
            result.messages.append(f"Scale failed: {e}")
        return result

    def scale_all(self, scale_map: dict) -> list:
        return [self.scale(svc, count) for svc, count in scale_map.items()]
