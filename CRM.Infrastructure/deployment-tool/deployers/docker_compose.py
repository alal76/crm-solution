#!/usr/bin/env python3
"""CRM CDT - Docker Compose Deployer."""
from __future__ import annotations
import subprocess
import queue
import threading
import time
import json
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

DOCKER_COMPOSE_FILE = "docker-compose.yml"


@dataclass
class DeployEvent:
    timestamp: float
    level: str
    message: str
    step: int = 0
    total_steps: int = 12
    percent: int = 0

    def to_dict(self) -> dict:
        return {
            "timestamp": self.timestamp,
            "level": self.level,
            "message": self.message,
            "step": self.step,
            "total_steps": self.total_steps,
            "percent": self.percent,
        }


class DockerComposeDeployer:
    def __init__(
        self,
        work_dir: Path,
        profile: dict,
        log_queue: Optional[queue.Queue] = None,
        dry_run: bool = False,
    ):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.log_queue = log_queue if log_queue is not None else queue.Queue()
        self.dry_run = dry_run
        self._abort = threading.Event()
        self.total_steps = 12

    def _emit(self, message: str, level: str = "info", step: int = 0) -> None:
        pct = int((step / self.total_steps) * 100) if self.total_steps else 0
        event = DeployEvent(time.time(), level, message, step, self.total_steps, pct)
        self.log_queue.put(event)
        print(f"[{level.upper()}] {message}")

    def abort(self) -> None:
        self._abort.set()

    def _run(self, cmd: list, cwd: Path = None, timeout: int = 300) -> tuple:
        if self.dry_run:
            self._emit(f"[DRY-RUN] Would run: {' '.join(str(c) for c in cmd)}")
            return (0, "", "")
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                cwd=str(cwd or self.work_dir),
                timeout=timeout,
            )
            return (result.returncode, result.stdout, result.stderr)
        except subprocess.TimeoutExpired:
            return (1, "", f"Command timed out after {timeout}s")
        except Exception as e:
            return (1, "", str(e))

    def deploy(self) -> bool:
        steps = [
            (1,  "Validating prerequisites", self._step_validate_prerequisites),
            (2,  "Pulling images",           self._step_pull_images),
            (3,  "Creating networks",        self._step_create_networks),
            (4,  "Starting databases",       self._step_start_databases),
            (5,  "Waiting for databases",    self._step_wait_databases),
            (6,  "Running migrations",       self._step_run_migrations),
            (7,  "Starting providers",       self._step_start_providers),
            (8,  "Starting API",             self._step_start_api),
            (9,  "Health checking API",      self._step_health_check_api),
            (10, "Starting frontend",        self._step_start_frontend),
            (11, "Seeding data",             self._step_configure_seed),
            (12, "Finishing",                self._step_finish),
        ]
        for step_num, step_name, step_fn in steps:
            if self._abort.is_set():
                self._emit("Deployment aborted by user", "warn", step_num)
                return False
            self._emit(f"Step {step_num}/{self.total_steps}: {step_name}", "info", step_num)
            try:
                ok = step_fn()
                if not ok:
                    self._emit(f"Step {step_num} failed: {step_name}", "error", step_num)
                    return False
            except Exception as e:
                self._emit(f"Step {step_num} exception: {e}", "error", step_num)
                return False
        return True

    def _step_validate_prerequisites(self) -> bool:
        rc, _, _ = self._run(["docker", "info"])
        if rc != 0:
            self._emit(
                "Docker not available. Install Docker Desktop or Docker Engine.", "error"
            )
            return False
        self._emit("Docker is available", "info")
        return True

    def _step_pull_images(self) -> bool:
        rc, _, err = self._run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "pull"], timeout=600
        )
        if rc != 0 and not self.dry_run:
            self._emit(f"Pull failed (non-fatal): {err}", "warn")
        return True

    def _step_create_networks(self) -> bool:
        self._run(
            ["docker", "network", "create", "crm-network", "--driver", "bridge"]
        )
        return True

    def _step_start_databases(self) -> bool:
        rc, _, _ = self._run(
            [
                "docker", "compose", "-f", DOCKER_COMPOSE_FILE,
                "up", "-d", "crm-mariadb", "crm-redis",
            ],
            timeout=120,
        )
        return rc == 0 or self.dry_run

    def _step_wait_databases(self) -> bool:
        if self.dry_run:
            return True
        for _ in range(24):
            rc, _, _ = self._run(
                ["docker", "exec", "crm-mariadb", "mysqladmin", "ping", "-h", "localhost", "--silent"]
            )
            if rc == 0:
                self._emit("Database ready", "info")
                return True
            time.sleep(5)
        self._emit("Database health check timed out", "warn")
        return False

    def _step_run_migrations(self) -> bool:
        if self.dry_run:
            return True
        rc, _, err = self._run(
            ["docker", "compose", "run", "--rm", "crm-api", "dotnet", "ef", "database", "update"],
            timeout=120,
        )
        if rc != 0:
            self._emit(f"Migration warning: {err}", "warn")
            return False
        return True

    def _step_start_providers(self) -> bool:
        providers = self.profile.get("providers", {})
        extras = []
        if providers.get("search_provider") == "meilisearch":
            extras.append("crm-meilisearch")
        if providers.get("ai_provider") == "ollama":
            extras.append("crm-ollama")
        if extras:
            self._run(
                ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d"] + extras,
                timeout=120,
            )
        return True

    def _step_start_api(self) -> bool:
        rc, _, _ = self._run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d", "crm-api"],
            timeout=120,
        )
        return rc == 0 or self.dry_run

    def _step_health_check_api(self) -> bool:
        if self.dry_run:
            return True
        import urllib.request

        for _ in range(12):
            try:
                urllib.request.urlopen("http://localhost:5000/health", timeout=5)  # noqa: S310
                self._emit("API is healthy", "info")
                return True
            except Exception:
                time.sleep(5)
        self._emit("API health check timed out — continuing anyway", "warn")
        return False

    def _step_start_frontend(self) -> bool:
        rc, _, _ = self._run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d", "crm-frontend"],
            timeout=120,
        )
        return rc == 0 or self.dry_run

    def _step_configure_seed(self) -> bool:
        seed = self.profile.get("seed", {})
        if seed.get("seed_master_data") and not self.dry_run:
            import urllib.request

            try:
                req = urllib.request.Request(
                    "http://localhost:5000/api/admin/seed/master-data", method="POST"
                )
                urllib.request.urlopen(req, timeout=30)
                self._emit("Master data seeded", "success")
            except Exception as e:
                self._emit(f"Seed warning: {e}", "warn")
        else:
            self._emit("Seed skipped (dry-run or not requested)", "info")
        return True

    def _step_finish(self) -> bool:
        domain = self.profile.get("target", {}).get("domain_name", "localhost")
        self._emit(
            f"Deployment complete! CRM is available at http://{domain}",
            "success",
            self.total_steps,
        )
        return True

    def rollback(self) -> bool:
        self._emit("Rolling back — stopping all containers", "warn")
        rc, _, _ = self._run(["docker", "compose", "down"])
        self._emit("Rollback complete", "info")
        return rc == 0 or self.dry_run

    def status(self) -> dict:
        rc, out, _ = self._run(["docker", "compose", "ps", "--format", "json"])
        containers = []
        if rc == 0 and out.strip():
            try:
                data = json.loads(out)
                if isinstance(data, list):
                    containers = data
                else:
                    containers = [data]
            except json.JSONDecodeError:
                pass
        running = sum(
            1 for c in containers if "running" in str(c.get("State", "")).lower()
        )
        return {
            "containers": containers,
            "running": running,
            "stopped": len(containers) - running,
        }
