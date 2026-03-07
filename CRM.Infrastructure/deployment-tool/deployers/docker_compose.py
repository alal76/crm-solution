#!/usr/bin/env python3
"""CRM CDT - Docker Compose Deployer."""
from __future__ import annotations
import logging
import re
import subprocess
import queue
import threading
import time
import json
import tempfile
import os
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

logger = logging.getLogger("cdt.deployer.docker")

DOCKER_COMPOSE_FILE = "docker-compose.yml"

# Images that may be built locally (name → Dockerfile relative to repo root)
LOCAL_BUILD_IMAGES = {
    "crm-api": "docker/Dockerfile.backend",
    "crm-frontend": "docker/Dockerfile.frontend",
}

# Map image name → component key in version.json components section
IMAGE_COMPONENT_MAP = {
    "crm-api": "api",
    "crm-frontend": "frontend",
}


@dataclass
class DeployEvent:
    timestamp: float
    level: str
    message: str
    step: int = 0
    total_steps: int = 12
    percent: int = 0
    host: str = ""

    def to_dict(self) -> dict:
        return {
            "timestamp": self.timestamp,
            "level": self.level,
            "message": self.message,
            "step": self.step,
            "total_steps": self.total_steps,
            "percent": self.percent,
            "host": self.host,
        }


# Container group classification
CONTAINER_GROUPS = {
    # Deployed first — reverse proxy, storage, management plane
    "infrastructure": [
        "crm-traefik",
        "crm-minio",
        "crm-portainer",
    ],
    # Deployed second — observability stack
    "monitoring": [
        "crm-prometheus",
        "crm-grafana",
        "crm-loki",
        "crm-uptime-kuma",
    ],
    # Deployed third — databases
    "database": ["crm-mariadb", "crm-redis"],
    # Deployed fourth — pluggable provider services
    "provider": [
        "crm-meilisearch", "crm-ollama", "crm-chatwoot",
        "crm-novu", "crm-superset", "crm-docuseal", "crm-n8n",
    ],
    # Deployed last — core application
    "app": ["crm-api", "crm-frontend"],
}

# Provider profile keys → container names (for selective provider start)
PROVIDER_CONTAINER_MAP = {
    "search":       {"meilisearch": "crm-meilisearch"},
    "ai":           {"ollama": "crm-ollama"},
    "chat":         {"chatwoot": "crm-chatwoot"},
    "notification": {"novu": "crm-novu"},
    "analytics":    {"superset": "crm-superset"},
    "signature":    {"docuseal": "crm-docuseal"},
    "integration":  {"n8n": "crm-n8n"},
}

# Infrastructure container → health check command (run via docker exec)
INFRA_HEALTH_CHECKS = {
    "crm-traefik": {
        "http_path": "/ping",
        "port": 8080,
        "description": "Traefik API/health port",
    },
    "crm-minio": {
        "http_path": "/minio/health/live",
        "port": 9002,
        "description": "MinIO liveness probe",
    },
    "crm-portainer": {
        "http_path": "/api/system/status",
        "port": 9000,
        "description": "Portainer status",
    },
    "crm-prometheus": {
        "http_path": "/-/ready",
        "port": 9090,
        "description": "Prometheus readiness",
    },
    "crm-grafana": {
        "http_path": "/api/health",
        "port": 3010,
        "description": "Grafana health",
    },
    "crm-uptime-kuma": {
        "http_path": "/",
        "port": 3001,
        "description": "Uptime Kuma UI",
    },
}

# Provider container → health endpoint for readiness checks
PROVIDER_HEALTH_CHECKS = {
    "crm-meilisearch": {"http_path": "/health",    "port": 7700},
    "crm-ollama":      {"http_path": "/api/tags",  "port": 11434},
    "crm-n8n":         {"http_path": "/healthz",   "port": 5678},
    "crm-superset":    {"http_path": "/health",    "port": 8088},
}


def classify_container(name: str) -> str:
    """Return the group name for a given container name."""
    for group, members in CONTAINER_GROUPS.items():
        if name in members:
            return group
    # Heuristic fallbacks for containers not explicitly listed
    if name.startswith("crm-traefik") or name.startswith("crm-minio") or name.startswith("crm-portainer"):
        return "infrastructure"
    if name in ("crm-prometheus", "crm-grafana", "crm-loki", "crm-uptime-kuma"):
        return "monitoring"
    return "other"


class DockerComposeDeployer:
    def __init__(
        self,
        work_dir: Path,
        profile: dict,
        log_queue: Optional[queue.Queue] = None,
        dry_run: bool = False,
        container_action: str = "recreate",
        containers_to_remove: Optional[list] = None,
    ):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.log_queue = log_queue if log_queue is not None else queue.Queue()
        self.dry_run = dry_run
        self.container_action = container_action  # "reuse" | "recreate"
        self.containers_to_remove = containers_to_remove or []
        self._reused_containers: set = set()  # populated in Step 2
        self._abort = threading.Event()
        self.total_steps = 18
        self._step_start_time: float = 0.0

        # Resolve target server from profile — NO localhost fallback.
        # The wizard MUST populate config.target.host before deploy.
        target = profile.get("target", {})
        self._target_host = (
            target.get("host")
            or target.get("domain_name")
            or profile.get("host")
            or profile.get("deployment_host")
        )
        if not self._target_host:
            # Hard error for non-dry-run; allow dry_run to proceed with a marker
            if dry_run:
                self._target_host = "NO_HOST_CONFIGURED"
            else:
                raise ValueError(
                    "Deployment target host is not configured. "
                    "Set config.target.host in the wizard before deploying."
                )

        # Store port configuration from the profile — no assumptions
        self._api_port = str(target.get("api_port", profile.get("api_port", "5000")))
        self._frontend_port = str(target.get("frontend_port", profile.get("frontend_port", "80")))
        self._ssl_enabled = profile.get("ssl", {}).get("ssl_enabled", False)

        self._target_platform = profile.get("platform", profile.get("architecture", "docker_compose"))
        if isinstance(self._target_platform, dict):
            self._target_platform = self._target_platform.get("container_runtime", "docker_compose")
        self._target_ssh_user = target.get("ssh_user", "")
        self._target_ssh_port = target.get("ssh_port", 22)
        # SSH private-key path — used by paramiko for key-based auth to remote targets.
        # Populated from the wizard's "SSH Key Path" field (config.target.ssh_key).
        # When blank, paramiko's look_for_keys=True will discover keys in ~/.ssh automatically.
        self._target_ssh_key = target.get("ssh_key", None) or None
        # SSH password — used when the wizard selects password-based auth.
        # When blank (key-based auth), paramiko falls back to key discovery.
        self._target_ssh_password = target.get("ssh_password", None) or None

        # Determine if the target is a remote host (not localhost/127.x)
        self._is_remote = self._target_host not in (
            "localhost", "127.0.0.1", "0.0.0.0", "", "NO_HOST_CONFIGURED"
        )
        self._is_local = not self._is_remote
        # Remote working directory on the target server
        self._remote_deploy_dir = target.get(
            "remote_deploy_dir",
            profile.get("remote_deploy_dir", "/opt/crm-deployment"),
        )

        # Resolve Docker build platform from profile (arm64 vs amd64)
        raw_arch = (
            target.get("target_arch")
            or target.get("machine_arch")
            or profile.get("target_arch")
            or ""
        ).lower()
        _arch_map = {"arm64": "linux/arm64", "aarch64": "linux/arm64",
                     "x86_64": "linux/amd64", "amd64": "linux/amd64"}
        self._target_docker_platform = _arch_map.get(raw_arch, "linux/amd64")

    def _services_to_start(self, requested: list[str]) -> list[str]:
        """Filter out services whose containers are being reused."""
        return [s for s in requested if s not in self._reused_containers]

    def _ensure_reused_running(self, containers: list[str]) -> bool:  # NOSONAR - best-effort; container start failures are non-fatal
        """Ensure reused containers are running via ``docker start`` (idempotent).

        ``docker start`` is a no-op for already-running containers and starts
        stopped/exited ones — unlike ``docker compose up`` which tries to
        recreate them under a new compose project and hits name conflicts.
        """
        reused = [c for c in containers if c in self._reused_containers]
        if not reused:
            return True
        self._emit(f"[{self._target_host}] Ensuring reused container(s) are running: {', '.join(reused)}", "info")
        for name in reused:
            # Check current state first
            rc_insp, state_out, _ = self._run_on_target(
                ["docker", "inspect", "-f", "{{.State.Status}}", name], timeout=10
            )
            state = state_out.strip() if rc_insp == 0 else "unknown"
            if state == "running":
                self._emit(f"  {name}: already running — no action needed", "info")
                continue
            self._emit(f"  {name}: state={state} — issuing docker start", "info")
            rc, _, err = self._run_on_target(["docker", "start", name], timeout=30)
            if rc != 0 and not self.dry_run:
                self._emit(f"  Could not start {name}: {err.strip()}", "warn")
            else:
                self._emit(f"  {name}: started successfully", "info")
        return True

    def _compose_up(self, services: list[str], timeout: int = 120) -> tuple[int, str, str]:
        """Run ``docker compose up -d`` with auto-retry on container name conflicts.

        If the first attempt fails because existing containers with the same
        ``container_name`` already exist (common when a prior compose project
        left orphans), the conflicting containers are forcibly removed and the
        command is retried once.
        """
        cmd = ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d"] + services
        rc, out, err = self._run_on_target(cmd, timeout=timeout, log_command=True)
        if rc != 0 and "already in use" in (err or ""):
            conflicts = re.findall(
                r'container name "/?([^"]+)" is already in use', err
            )
            if conflicts:
                self._emit(
                    f"[{self._target_host}] Removing {len(conflicts)} conflicting orphan container(s): {', '.join(conflicts)}",
                    "warn",
                )
                self._run_on_target(
                    ["docker", "rm", "-f"] + conflicts, timeout=30
                )
                rc, out, err = self._run_on_target(
                    cmd, timeout=timeout, log_command=True
                )
        return rc, out, err

    _LOG_LEVEL_MAP = {"info": logging.INFO, "warn": logging.WARNING, "error": logging.ERROR, "success": logging.INFO}

    def _emit(self, message: str, level: str = "info", step: int = 0) -> None:
        pct = int((step / self.total_steps) * 100) if self.total_steps else 0
        host = getattr(self, "_target_host", "") or ""
        event = DeployEvent(time.time(), level, message, step, self.total_steps, pct, host)
        self.log_queue.put(event)
        logger.log(self._LOG_LEVEL_MAP.get(level, logging.INFO), message)

    def abort(self) -> None:
        self._abort.set()

    def _run(self, cmd: list, cwd: Path = None, timeout: int = 300, log_command: bool = False) -> tuple:
        cmd_str = " ".join(str(c) for c in cmd)
        if log_command:
            self._emit(f"[{self._target_host}] Running: {cmd_str}", "info")
        if self.dry_run:
            self._emit(f"[DRY-RUN] [{self._target_host}] Would run: {cmd_str}")
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

    def _run_streaming(
        self, cmd: list, cwd: Path = None, timeout: int = 600, prefix: str = ""
    ) -> int:
        """Run a command and stream stdout/stderr lines to the log queue.

        Returns the exit code (0 = success).
        """
        if self.dry_run:
            self._emit(f"[DRY-RUN] Would run: {' '.join(str(c) for c in cmd)}")
            return 0
        try:
            proc = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True,
                cwd=str(cwd or self.work_dir),
            )
            deadline = time.time() + timeout
            last_emit = time.time()
            for line in proc.stdout:
                stripped = line.rstrip()
                # Emit meaningful lines (skip blank / overly repetitive)
                if stripped and time.time() - last_emit > 0.1:
                    tag = f"[{prefix}] " if prefix else ""
                    self._emit(f"{tag}{stripped}", "info")
                    last_emit = time.time()
                if time.time() > deadline:
                    proc.kill()
                    self._emit(f"Command timed out after {timeout}s", "error")
                    return 1
            proc.wait(timeout=30)
            return proc.returncode
        except Exception as e:
            self._emit(f"Streaming exec error: {e}", "error")
            return 1

    # ------------------------------------------------------------------ #
    #  SSH remote execution (for deploying to a remote target server)     #
    # ------------------------------------------------------------------ #
    def _run_remote_ssh(self, cmd_str: str, timeout: int = 300) -> tuple:
        """Execute a shell command on the remote target via SSH (paramiko).

        Returns (returncode, stdout, stderr).
        """
        if self.dry_run:
            self._emit(f"[DRY-RUN] [{self._target_host}] Would SSH: {cmd_str}", "info")
            return (0, "", "")
        try:
            import paramiko  # noqa: delayed import
            ssh = paramiko.SSHClient()
            ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            _auto = not bool(self._target_ssh_key) and not bool(self._target_ssh_password)
            ssh.connect(
                self._target_host,
                port=int(self._target_ssh_port),
                username=self._target_ssh_user or "root",
                password=self._target_ssh_password,  # passphrase when key set; auth-password otherwise
                key_filename=self._target_ssh_key,
                look_for_keys=_auto,
                allow_agent=_auto,
                timeout=15,
            )
            _, stdout_ch, stderr_ch = ssh.exec_command(cmd_str, timeout=timeout)
            stdout_text = stdout_ch.read().decode("utf-8", errors="replace")
            stderr_text = stderr_ch.read().decode("utf-8", errors="replace")
            rc = stdout_ch.channel.recv_exit_status()
            ssh.close()
            return (rc, stdout_text, stderr_text)
        except Exception as e:
            logger.error("SSH exec on %s failed: %s", self._target_host, e)
            return (1, "", str(e))

    def _run_on_target(
        self, cmd: list, timeout: int = 300, log_command: bool = False, cwd: str = ""
    ) -> tuple:
        """Run a command on the deployment target.

        If the target is remote (not localhost), executes via SSH.
        If the target is local, delegates to ``_run()``.
        """
        cmd_str = " ".join(str(c) for c in cmd)
        if log_command:
            self._emit(f"[{self._target_host}] Running: {cmd_str}", "info")
        if not self._is_remote:
            return self._run(cmd, cwd=Path(cwd) if cwd else None, timeout=timeout)
        # Remote: wrap in cd + command
        if cwd:
            remote_cmd = f"cd {cwd} && {cmd_str}"
        else:
            remote_cmd = f"cd {self._remote_deploy_dir} && {cmd_str}"
        return self._run_remote_ssh(remote_cmd, timeout=timeout)

    def _scp_to_target(self, local_path: str, remote_path: str) -> bool:
        """SCP a file from local machine to the remote target."""
        if self.dry_run:
            self._emit(
                f"[DRY-RUN] Would SCP {local_path} → "
                f"{self._target_host}:{remote_path}",
                "info",
            )
            return True
        try:
            import paramiko
            ssh = paramiko.SSHClient()
            ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            _auto = not bool(self._target_ssh_key) and not bool(self._target_ssh_password)
            ssh.connect(
                self._target_host,
                port=int(self._target_ssh_port),
                username=self._target_ssh_user or "root",
                password=self._target_ssh_password,  # passphrase when key set; auth-password otherwise
                key_filename=self._target_ssh_key,
                look_for_keys=_auto,
                allow_agent=_auto,
                timeout=15,
            )
            sftp = ssh.open_sftp()
            sftp.put(local_path, remote_path)
            sftp.close()
            ssh.close()
            return True
        except Exception as e:
            self._emit(f"SCP failed: {e}", "error")
            return False

    def deploy(self) -> bool:
        steps = [
            # ── Phase 1: Preparation ──────────────────────────────────────────────
            (1,  "Validating prerequisites",          self._step_validate_prerequisites),
            (2,  "Checking existing containers",       self._step_handle_existing_containers),
            (3,  "Building local images",              self._step_build_local_images),
            (4,  "Transferring images to target",       self._step_transfer_images),
            (5,  "Pulling images",                     self._step_pull_images),
            (6,  "Creating networks",                  self._step_create_networks),
            # ── Phase 2: Infrastructure layer (reverse-proxy, storage, mgmt) ─────
            (7,  "Starting infrastructure services",   self._step_start_infrastructure),
            (8,  "Waiting for infrastructure ready",   self._step_wait_infrastructure),
            # ── Phase 3: Data layer ───────────────────────────────────────────────
            (9,  "Starting databases",                 self._step_start_databases),
            (10, "Waiting for databases",              self._step_wait_databases),
            (11, "Running migrations",                 self._step_run_migrations),
            # ── Phase 4: Pluggable providers ─────────────────────────────────────
            (12, "Starting provider services",         self._step_start_providers),
            (13, "Waiting for providers ready",        self._step_wait_providers),
            # ── Phase 5: Core application ─────────────────────────────────────────
            (14, "Starting API",                       self._step_start_api),
            (15, "Health checking API",                self._step_health_check_api),
            (16, "Starting frontend",                  self._step_start_frontend),
            # ── Phase 6: Post-deployment ─────────────────────────────────────────
            (17, "Seeding data",                       self._step_configure_seed),
            (18, "Finishing",                          self._step_finish),
        ]

        # ── Deployment header ──
        self._emit("═" * 60, "info")
        self._emit(f"CRM Deployment starting on {self._target_host}", "info")
        self._emit(f"  Platform:         {self._target_platform}", "info")
        self._emit(f"  Docker platform:  {self._target_docker_platform}", "info")
        self._emit(f"  Remote target:    {self._is_remote}", "info")
        if self._is_remote:
            self._emit(f"  Remote dir:       {self._remote_deploy_dir}", "info")
        self._emit(f"  Working directory: {self.work_dir}", "info")
        self._emit(f"  Compose file:     {DOCKER_COMPOSE_FILE}", "info")
        self._emit(f"  Container action: {self.container_action}", "info")
        if self.containers_to_remove:
            self._emit(f"  Containers to remove: {', '.join(self.containers_to_remove)}", "info")
        if self._target_ssh_user:
            self._emit(f"  SSH user:         {self._target_ssh_user}", "info")
            self._emit(f"  SSH port:         {self._target_ssh_port}", "info")
        self._emit(f"  Dry run:          {self.dry_run}", "info")
        self._emit("═" * 60, "info")

        deploy_start = time.time()
        for step_num, step_name, step_fn in steps:
            if self._abort.is_set():
                self._emit("Deployment aborted by user", "warn", step_num)
                return False
            self._step_start_time = time.time()
            self._emit(
                f"Step {step_num}/{self.total_steps}: {step_name} "
                f"[{self._target_host}]",
                "info", step_num,
            )
            try:
                ok = step_fn()
                elapsed = time.time() - self._step_start_time
                if not ok:
                    self._emit(
                        f"Step {step_num} FAILED: {step_name} "
                        f"(took {elapsed:.1f}s) [{self._target_host}]",
                        "error", step_num,
                    )
                    return False
                self._emit(
                    f"Step {step_num} completed in {elapsed:.1f}s",
                    "info", step_num,
                )
            except Exception as e:
                elapsed = time.time() - self._step_start_time
                self._emit(
                    f"Step {step_num} exception after {elapsed:.1f}s: {e} [{self._target_host}]",
                    "error", step_num,
                )
                return False

        total_elapsed = time.time() - deploy_start
        self._emit(
            f"All {self.total_steps} steps completed in {total_elapsed:.1f}s on {self._target_host}",
            "success",
        )
        return True

    def _step_validate_prerequisites(self) -> bool:
        self._emit(f"[{self._target_host}] Checking Docker availability…", "info")
        rc, out, err = self._run_on_target(["docker", "info"], log_command=True)
        if rc != 0:
            self._emit(
                f"[{self._target_host}] Docker not available. "
                "Install Docker Desktop or Docker Engine.", "error"
            )
            if err:
                self._emit(f"  Error: {err.strip()[:300]}", "error")
            return False

        # Extract Docker version for the log
        rc_v, ver_out, _ = self._run_on_target(["docker", "version", "--format", "{{.Server.Version}}"], timeout=10)
        docker_ver = ver_out.strip() if rc_v == 0 else "unknown"
        self._emit(f"[{self._target_host}] Docker is available (version {docker_ver})", "info")

        # Log Docker Compose version too
        rc_c, comp_out, _ = self._run_on_target(["docker", "compose", "version", "--short"], timeout=10)
        compose_ver = comp_out.strip() if rc_c == 0 else "unknown"
        self._emit(f"[{self._target_host}] Docker Compose version: {compose_ver}", "info")

        # Check disk space on the target
        disk_path = self._remote_deploy_dir if self._is_remote else str(self.work_dir)
        rc_d, df_out, _ = self._run_on_target(["df", "-h", disk_path], timeout=10)
        if rc_d == 0 and df_out.strip():
            lines = df_out.strip().splitlines()
            if len(lines) >= 2:
                self._emit(f"[{self._target_host}] Disk space: {lines[-1].strip()}", "info")

        # For remote targets, verify SSH connectivity
        if self._is_remote and not self.dry_run:
            self._emit(f"[{self._target_host}] SSH connectivity to remote target verified", "info")

        return True

    # ------------------------------------------------------------------ #
    #  Step 2 — Handle Existing Containers                                #
    # ------------------------------------------------------------------ #
    def _step_handle_existing_containers(self) -> bool:
        """Stop and remove existing CRM containers selected for recreation.

        Uses ``docker rm -f`` directly by container name so it works
        regardless of which compose project originally created them.

        When container_action is ``reuse`` and no explicit removal list was
        supplied, all existing containers are kept as-is.
        """
        # Discover all existing CRM containers once
        self._emit(f"[{self._target_host}] Scanning for existing CRM containers…", "info")
        rc, out, _ = self._run_on_target(
            ["docker", "ps", "-a", "--filter", "name=crm-", "--format", "{{.Names}}\t{{.Status}}\t{{.Image}}"],
            log_command=True,
        )
        existing = set()
        if rc == 0 and out.strip():
            for line in out.strip().splitlines():
                parts = line.split("\t")
                name = parts[0].strip() if parts else ""
                status = parts[1].strip() if len(parts) > 1 else "unknown"
                image = parts[2].strip() if len(parts) > 2 else "unknown"
                if name:
                    existing.add(name)
                    self._emit(f"  Found: {name}  image={image}  status={status}", "info")
        self._emit(f"[{self._target_host}] Discovered {len(existing)} existing CRM container(s)", "info")

        if self.container_action == "reuse" and not self.containers_to_remove:
            self._reused_containers = existing
            self._emit(f"[{self._target_host}] Reusing all {len(existing)} existing container(s) (no cleanup)", "info")
            return True

        # If a specific list was provided, use it; otherwise recreate all
        to_remove = list(self.containers_to_remove) if self.containers_to_remove else []
        if not to_remove and self.container_action == "recreate":
            to_remove = list(existing)

        # Everything that exists but is NOT being removed → reuse it
        self._reused_containers = existing - set(to_remove)
        if self._reused_containers:
            self._emit(
                f"[{self._target_host}] Reusing {len(self._reused_containers)} container(s): "
                f"{', '.join(sorted(self._reused_containers))}",
                "info",
            )

        if not to_remove:
            self._emit(f"[{self._target_host}] No containers to remove", "info")
            return True

        self._emit(
            f"[{self._target_host}] Removing {len(to_remove)} container(s): {', '.join(to_remove)}",
            "info",
        )

        # Force-remove each container by name (works across compose projects)
        removed, failed = 0, 0
        for name in to_remove:
            self._emit(f"  [{self._target_host}] docker rm -f {name}", "info")
            rc, _, err = self._run_on_target(["docker", "rm", "-f", name], timeout=15)
            if rc == 0 or self.dry_run:
                removed += 1
                self._emit(f"  {name}: removed", "info")
            else:
                failed += 1
                self._emit(f"  {name}: FAILED — {err.strip()}", "warn")

        self._emit(
            f"[{self._target_host}] Removed {removed} container(s)"
            + (f", {failed} failed" if failed else ""),
            "info",
        )
        return failed == 0  # return False when container removal failed

    # ------------------------------------------------------------------ #
    #  Existing deployment secrets recovery                               #
    # ------------------------------------------------------------------ #
    # Keys in the remote .env that should be preserved across deploys
    _PRESERVED_SECRET_KEYS: list[str] = [
        "DB_PASSWORD", "DB_ROOT_PASSWORD", "JWT_SECRET",
        "REDIS_PASSWORD", "MEILI_MASTER_KEY",
        "ADMIN_PASSWORD",
        "CHATWOOT_API_KEY", "CHATWOOT_SECRET_KEY",
        "NOVU_API_KEY", "NOVU_JWT_SECRET",
        "SUPERSET_SECRET_KEY", "SUPERSET_ADMIN_PASSWORD",
        "DOCUSEAL_API_KEY", "DOCUSEAL_SECRET_KEY",
        "N8N_API_KEY",
        "OPENAI_API_KEY", "ANTHROPIC_API_KEY",
        "AZURE_OPENAI_API_KEY",
    ]

    # Map from .env key → profile context key used by the Jinja2 generator
    _ENV_TO_CONTEXT_KEY: dict[str, str] = {
        "DB_PASSWORD": "db_password",
        "DB_ROOT_PASSWORD": "db_root_password",
        "JWT_SECRET": "jwt_secret",
        "REDIS_PASSWORD": "redis_password",
        "MEILI_MASTER_KEY": "meilisearch_master_key",
        "ADMIN_PASSWORD": "admin_password",
        "CHATWOOT_API_KEY": "chatwoot_api_key",
        "CHATWOOT_SECRET_KEY": "chatwoot_secret_key",
        "NOVU_API_KEY": "novu_api_key",
        "NOVU_JWT_SECRET": "novu_jwt_secret",
        "SUPERSET_SECRET_KEY": "superset_secret_key",
        "SUPERSET_ADMIN_PASSWORD": "superset_admin_password",
        "DOCUSEAL_API_KEY": "docuseal_api_key",
        "DOCUSEAL_SECRET_KEY": "docuseal_secret_key",
        "N8N_API_KEY": "n8n_api_key",
        "OPENAI_API_KEY": "openai_api_key",
        "ANTHROPIC_API_KEY": "anthropic_api_key",
        "AZURE_OPENAI_API_KEY": "azure_openai_api_key",
    }

    @staticmethod
    def _parse_env_file(content: str) -> dict[str, str]:
        """Parse a .env file content and return a {KEY: VALUE} dict.

        Handles:
        - ``KEY=VALUE``
        - ``KEY="VALUE"`` (strips quotes)
        - ``KEY='VALUE'`` (strips quotes)
        - Comments (``#``) and blank lines are skipped
        - Docker-escaped ``$$`` is unescaped back to ``$``
        """
        result: dict[str, str] = {}
        for line in content.splitlines():
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            if "=" not in line:
                continue
            key, _, value = line.partition("=")
            key = key.strip()
            value = value.strip()
            # Strip surrounding quotes
            if len(value) >= 2 and value[0] == value[-1] and value[0] in ('"', "'"):
                value = value[1:-1]
            # Unescape Docker Compose $$ → $
            value = value.replace("$$", "$")
            result[key] = value
        return result

    @staticmethod
    def read_remote_env_secrets(
        host: str,
        remote_deploy_dir: str = "/opt/crm-deployment",
        ssh_user: str = "root",
        ssh_port: int = 22,
        ssh_key: Optional[str] = None,
        ssh_password: Optional[str] = None,
    ) -> dict[str, str]:
        """SSH to *host* and read existing secrets from the deployed ``.env`` file.

        Returns a dict mapping **context keys** (e.g. ``db_password``) to their
        existing values.  Only non-empty secrets listed in
        ``_PRESERVED_SECRET_KEYS`` are returned.

        If the remote ``.env`` does not exist or SSH fails, returns ``{}``.
        This is intentionally best-effort so that first-time deployments
        (where no ``.env`` exists yet) silently fall through to generation.
        """
        if host in ("localhost", "127.0.0.1", "0.0.0.0", ""):
            # Local deployment — try reading .env directly
            env_path = Path(remote_deploy_dir) / ".env"
            if env_path.is_file():
                content = env_path.read_text(encoding="utf-8", errors="replace")
            else:
                return {}
        else:
            try:
                import paramiko  # noqa: delayed import
                ssh = paramiko.SSHClient()
                ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
                _auto = not bool(ssh_key) and not bool(ssh_password)
                ssh.connect(
                    host, port=ssh_port, username=ssh_user,
                    password=ssh_password, key_filename=ssh_key,
                    look_for_keys=_auto, allow_agent=_auto, timeout=15,
                )
                _, stdout_ch, _ = ssh.exec_command(
                    f"cat {remote_deploy_dir}/.env 2>/dev/null", timeout=15
                )
                content = stdout_ch.read().decode("utf-8", errors="replace")
                rc = stdout_ch.channel.recv_exit_status()
                ssh.close()
                if rc != 0 or not content.strip():
                    return {}
            except Exception as exc:  # noqa: BLE001
                logger.warning("read_remote_env_secrets SSH to %s failed: %s", host, exc)
                return {}

        parsed = DockerComposeDeployer._parse_env_file(content)
        secrets: dict[str, str] = {}
        for env_key in DockerComposeDeployer._PRESERVED_SECRET_KEYS:
            ctx_key = DockerComposeDeployer._ENV_TO_CONTEXT_KEY.get(env_key)
            if not ctx_key:
                continue
            value = parsed.get(env_key, "")
            if value:
                secrets[ctx_key] = value
        return secrets

    @staticmethod
    def inject_secrets_into_profile(profile: dict, secrets: dict[str, str]) -> dict:
        """Merge recovered *secrets* into *profile* so the config generator
        reuses them instead of auto-generating new values.

        Secrets are placed under ``profile['database']`` for DB creds and
        ``profile['secrets']`` for everything else, matching how the
        generator's ``_flatten_profile_sections`` reads them.

        Returns the mutated *profile* for convenience.
        """
        if not secrets:
            return profile

        db_keys = {"db_password", "db_root_password"}
        db_section = profile.setdefault("database", {})
        if not isinstance(db_section, dict):
            db_section = {}
            profile["database"] = db_section

        secrets_section = profile.setdefault("secrets", {})
        if not isinstance(secrets_section, dict):
            secrets_section = {}
            profile["secrets"] = secrets_section

        for key, value in secrets.items():
            if key in db_keys:
                # Force-override: recovered secrets from the running deployment
                # MUST take precedence over stale wizard/profile values,
                # because MariaDB ignores MYSQL_PASSWORD after first volume init.
                db_section[key] = value
            else:
                secrets_section[key] = value

        return profile

    @staticmethod
    def check_remote_db_volume_exists(
        host: str,
        ssh_user: str = "root",
        ssh_port: int = 22,
        volume_name: str = "mariadb_data",
        ssh_key: Optional[str] = None,
        ssh_password: Optional[str] = None,
    ) -> bool:
        """Check if a MariaDB data volume exists on the remote *host*.

        Returns ``True`` if the volume exists (meaning MariaDB has been
        previously initialized and MYSQL_PASSWORD env vars will be
        ignored on container recreation).
        """
        if host in ("localhost", "127.0.0.1", "0.0.0.0", ""):
            import subprocess
            try:
                rc = subprocess.run(
                    ["docker", "volume", "inspect", volume_name],
                    capture_output=True, timeout=10,
                ).returncode
                return rc == 0
            except Exception:  # noqa: BLE001
                return False
        else:
            try:
                import paramiko  # noqa: delayed import
                ssh = paramiko.SSHClient()
                ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
                _auto = not bool(ssh_key) and not bool(ssh_password)
                ssh.connect(
                    host, port=ssh_port, username=ssh_user,
                    password=ssh_password, key_filename=ssh_key,
                    look_for_keys=_auto, allow_agent=_auto, timeout=15,
                )
                # Check multiple possible volume name patterns
                # (compose project prefixes the volume name)
                cmd = (
                    f"docker volume ls --format '{{{{.Name}}}}' "
                    f"| grep -q '{volume_name}'"
                )
                _, stdout_ch, _ = ssh.exec_command(cmd, timeout=15)
                rc = stdout_ch.channel.recv_exit_status()
                ssh.close()
                return rc == 0
            except Exception as exc:  # noqa: BLE001
                logger.warning(
                    "check_remote_db_volume_exists SSH to %s failed: %s",
                    host, exc,
                )
                return False  # Assume no volume → safe to generate new passwords

    # ------------------------------------------------------------------ #
    #  Version-aware image helpers                                        #
    # ------------------------------------------------------------------ #
    @staticmethod
    def _read_version_from_repo(repo_root: Path, component: str | None = None) -> str:
        """Read version string from ``version.json`` at *repo_root*.

        When *component* is ``None`` returns the solution-level version
        (e.g. ``0.614.58``).  When *component* is ``"api"`` or
        ``"frontend"`` the component-specific version from the
        ``components`` section is returned, falling back to the
        solution-level version when the component key is missing.

        Returns ``"latest"`` on any read/parse failure.
        """
        version_file = repo_root / "version.json"
        if not version_file.is_file():
            return "latest"
        try:
            with open(version_file) as fh:
                data = json.load(fh)

            # Component-specific version
            if component:
                comp = data.get("components", {}).get(component, {})
                comp_ver = comp.get("version")
                if comp_ver:
                    return comp_ver

            # Fallback: solution-level version
            major = data.get("major", 0)
            minor = data.get("minor", 0)
            patch = data.get("patch", 0)
            return f"{major}.{minor}.{patch}"
        except (json.JSONDecodeError, KeyError, OSError) as exc:
            logger.warning("_read_version_from_repo failed (%s): %s", version_file, exc)
            return "latest"

    def _image_exists_locally(self, image_tag: str) -> bool:
        """Return True when *image_tag* already exists in the local Docker daemon."""
        rc, out, _ = self._run(
            ["docker", "images", "-q", image_tag], timeout=15
        )
        return rc == 0 and bool(out.strip())

    # ------------------------------------------------------------------ #
    #  Remote build server helpers                                        #
    # ------------------------------------------------------------------ #
    def _run_build_server_ssh(
        self, build_server: dict, cmd_str: str, timeout: int = 300
    ) -> tuple:
        """Execute a shell command on the remote build server via SSH.

        Returns (returncode, stdout, stderr).
        """
        host = build_server.get("host", "")
        ssh_user = build_server.get("ssh_user", "root")
        ssh_port = int(build_server.get("ssh_port", 22))

        if self.dry_run:
            self._emit(
                f"[DRY-RUN] [{host}] Would SSH: {cmd_str}", "info"
            )
            return (0, "", "")
        try:
            import paramiko
            ssh = paramiko.SSHClient()
            ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            ssh.connect(host, port=ssh_port, username=ssh_user, timeout=15)
            _, stdout, stderr = ssh.exec_command(cmd_str, timeout=timeout)
            rc = stdout.channel.recv_exit_status()
            out = stdout.read().decode("utf-8", errors="replace")
            err = stderr.read().decode("utf-8", errors="replace")
            ssh.close()
            return (rc, out, err)
        except Exception as e:
            self._emit(f"[{host}] Build server SSH error: {e}", "error")
            return (1, "", str(e))

    def _read_remote_version(self, build_server: dict) -> str:
        """Read version from version.json on the remote build server."""
        repo_path = build_server.get("repo_path", "/opt/crm-solution")
        rc, out, _ = self._run_build_server_ssh(
            build_server,
            f"cat {repo_path}/version.json",
            timeout=10,
        )
        if rc != 0 or not out.strip():
            return "latest"
        try:
            data = json.loads(out)
            major = data.get("major", 0)
            minor = data.get("minor", 0)
            patch = data.get("patch", 0)
            return f"{major}.{minor}.{patch}"
        except (json.JSONDecodeError, KeyError):
            return "latest"

    # ------------------------------------------------------------------ #
    #  Step 3 — Build Local Images (skipped when using a remote registry) #
    # ------------------------------------------------------------------ #
    def _step_build_local_images(self) -> bool:
        """Build CRM Docker images from source when build_locally is enabled.

        Each image is tagged with its **component-specific** version from
        ``version.json`` (e.g. ``crm-api:0.614.58``,
        ``crm-frontend:0.614.58``) **and** ``:latest``.
        If a versioned image already exists locally the build is skipped
        for that image, saving significant time on repeated deployments.

        Supports building on a **remote build server** when
        ``profile.build_server.type == "remote"`` — the build commands
        are executed via SSH on that server instead of locally.
        """
        img_reg = self.profile.get("image_registry", {})
        build_locally = img_reg.get("build_locally", False)

        if not build_locally:
            registry = img_reg.get("image_registry", "Docker Hub / default")
            self._emit(
                f"[{self._target_host}] Skipping local build — using registry images from {registry}",
                "info",
            )
            return True

        # ── Check for remote build server ──
        build_server = self.profile.get("build_server", {})
        use_remote_build = build_server.get("type") == "remote" and build_server.get("host")
        build_host_label = build_server.get("host", "localhost") if use_remote_build else "localhost"

        # Resolve repo root: profile may override, else walk up from this file
        if use_remote_build:
            repo_root_str = build_server.get("repo_path", "/opt/crm-solution")
            repo_root = Path(repo_root_str)
            self._emit(f"[{build_host_label}] Using remote build server: {build_server.get('host')}", "info")
            self._emit(f"[{build_host_label}] Remote repo path: {repo_root_str}", "info")
        else:
            repo_root_str = self.profile.get("target", {}).get("repo_root")
            if not repo_root_str:
                repo_root_str = str(Path(__file__).resolve().parent.parent.parent.parent)
            repo_root = Path(repo_root_str)

        if not use_remote_build and not repo_root.is_dir():
            self._emit(f"[{build_host_label}] Repo root not found: {repo_root}", "error")
            return False

        # Read solution-level version and per-component versions
        if not use_remote_build:
            code_version = self._read_version_from_repo(repo_root)
        else:
            # For remote builds, try to read version via SSH
            code_version = self._read_remote_version(build_server) or "latest"
        profile_version = self.profile.get("target", {}).get("crm_version")

        # Build a map of image name → version tag
        image_tags: dict[str, str] = {}
        for name in LOCAL_BUILD_IMAGES:
            component = IMAGE_COMPONENT_MAP.get(name)
            if not use_remote_build:
                comp_ver = self._read_version_from_repo(repo_root, component) if component else None
            else:
                comp_ver = None  # remote builds use solution-level version
            image_tags[name] = profile_version or comp_ver or code_version or "latest"

        self._emit(f"[{build_host_label}] Build configuration:", "info")
        self._emit(f"  Build server:    {build_host_label}", "info")
        self._emit(f"  Repo root:       {repo_root}", "info")
        self._emit(f"  Solution ver:    {code_version}", "info")
        for name, tag in image_tags.items():
            self._emit(f"  {name} tag:  {tag}", "info")
        self._emit(f"  Platform:        {self._target_docker_platform}", "info")
        self._emit(f"  Images to build: {', '.join(LOCAL_BUILD_IMAGES.keys())}", "info")

        built, skipped, failed = 0, 0, 0
        for name, dockerfile in LOCAL_BUILD_IMAGES.items():
            tag = image_tags[name]
            image_tag = f"{name}:{tag}"

            if not use_remote_build:
                df_path = repo_root / dockerfile
                if not df_path.is_file():
                    self._emit(f"[{build_host_label}] Dockerfile not found: {df_path} — skipping {name}", "warn")
                    continue

                # --- Skip build when the versioned image already exists -----
                if not self.dry_run and tag != "latest" and self._image_exists_locally(image_tag):
                    self._emit(
                        f"[{build_host_label}] Image {image_tag} already exists — skipping build",
                        "info",
                    )
                    skipped += 1
                    continue

                self._emit(f"[{build_host_label}] ── Building {image_tag} ──", "info")
                self._emit(f"  Dockerfile: {df_path}", "info")
                self._emit(f"  Context:    {repo_root}", "info")
                build_start = time.time()
                rc = self._run_streaming(
                    [
                        "docker", "build",
                        "--platform", self._target_docker_platform,
                        "-t", image_tag,
                        "-f", str(df_path),
                        ".",
                    ],
                    cwd=repo_root,
                    timeout=600,
                    prefix=name,
                )
            else:
                # Remote build: execute docker build via SSH on the build server
                remote_df = f"{repo_root_str}/{dockerfile}"
                self._emit(f"[{build_host_label}] ── Building {image_tag} (remote) ──", "info")
                self._emit(f"  Dockerfile: {remote_df}", "info")
                self._emit(f"  Context:    {repo_root_str}", "info")
                build_start = time.time()
                build_cmd = (
                    f"cd {repo_root_str} && docker build"
                    f" --platform {self._target_docker_platform}"
                    f" -t {image_tag}"
                    f" -f {remote_df}"
                    f" ."
                )
                rc_t, out, err = self._run_build_server_ssh(build_server, build_cmd, timeout=600)
                rc = rc_t
                if out and out.strip():
                    for line in out.strip().splitlines()[-20:]:
                        self._emit(f"  [{build_host_label}] {line}", "info")

            build_elapsed = time.time() - build_start
            if rc != 0:
                self._emit(
                    f"[{build_host_label}] Build FAILED for {name} (exit code {rc}, took {build_elapsed:.1f}s)",
                    "error",
                )
                failed += 1
            else:
                self._emit(
                    f"[{build_host_label}] Built {image_tag} successfully in {build_elapsed:.1f}s",
                    "success",
                )
                # Also tag as :latest for backwards-compatibility
                if use_remote_build:
                    self._run_build_server_ssh(build_server, f"docker tag {image_tag} {name}:latest", timeout=15)
                else:
                    self._run(["docker", "tag", image_tag, f"{name}:latest"], timeout=15)
                built += 1

        if failed > 0:
            self._emit(f"[{self._target_host}] {failed} image(s) failed to build", "error")
            return False
        summary_parts = []
        if built:
            summary_parts.append(f"{built} built")
        if skipped:
            summary_parts.append(f"{skipped} skipped (already up-to-date)")
        self._emit(
            f"[{self._target_host}] Image build complete: {', '.join(summary_parts) or 'nothing to do'}",
            "success",
        )
        return True

    # ------------------------------------------------------------------ #
    #  Step 4 — Transfer Images to Remote Target                          #
    # ------------------------------------------------------------------ #
    def _step_transfer_images(self) -> bool:
        """Save locally-built images, transfer to remote target, and load them.

        This step is only needed for remote deployments with build_locally=True.
        For local deployments the images are already in the local Docker daemon.
        """
        build_locally = self.profile.get("image_registry", {}).get("build_locally", False)
        if not build_locally:
            self._emit(f"[{self._target_host}] Using registry images — transfer not needed", "info")
            return True
        if not self._is_remote:
            self._emit(f"[{self._target_host}] Local deployment — images already available", "info")
            return True

        # Resolve version tags — consistent with _step_build_local_images
        repo_root = self.profile.get("target", {}).get("repo_root")
        if not repo_root:
            repo_root = str(Path(__file__).resolve().parent.parent.parent.parent)
        repo_path = Path(repo_root)
        profile_version = self.profile.get("target", {}).get("crm_version")

        images_to_transfer = []
        for name in LOCAL_BUILD_IMAGES:
            component = IMAGE_COMPONENT_MAP.get(name)
            comp_ver = self._read_version_from_repo(repo_path, component) if component else None
            tag = profile_version or comp_ver or self._read_version_from_repo(repo_path) or "latest"
            images_to_transfer.append(f"{name}:{tag}")
        self._emit(
            f"[{self._target_host}] Transferring {len(images_to_transfer)} image(s) "
            f"to remote target: {', '.join(images_to_transfer)}",
            "info",
        )

        if self.dry_run:
            self._emit(
                f"[DRY-RUN] Would save/transfer/load images to {self._target_host}",
                "info",
            )
            return True

        # Ensure remote deploy directory exists
        self._run_remote_ssh(
            f"mkdir -p {self._remote_deploy_dir}", timeout=15
        )

        tmp_dir = tempfile.mkdtemp(prefix="crm-deploy-")
        try:
            for image_tag in images_to_transfer:
                safe_name = image_tag.replace(":", "_").replace("/", "_")
                tar_path = os.path.join(tmp_dir, f"{safe_name}.tar")
                remote_tar = f"{self._remote_deploy_dir}/{safe_name}.tar"

                # 1. docker save locally
                self._emit(f"  Saving {image_tag} to {tar_path}…", "info")
                save_start = time.time()
                rc, _, err = self._run(
                    ["docker", "save", image_tag, "-o", tar_path], timeout=300
                )
                save_elapsed = time.time() - save_start
                if rc != 0:
                    self._emit(f"  docker save failed for {image_tag}: {err.strip()[:200]}", "error")
                    return False
                tar_size_mb = os.path.getsize(tar_path) / (1024 * 1024)
                self._emit(
                    f"  Saved {image_tag} ({tar_size_mb:.0f} MB) in {save_elapsed:.1f}s",
                    "info",
                )

                # 2. SCP to remote
                self._emit(
                    f"  Transferring {safe_name}.tar to {self._target_host}:{remote_tar}…",
                    "info",
                )
                xfer_start = time.time()
                if not self._scp_to_target(tar_path, remote_tar):
                    self._emit(f"  Transfer failed for {image_tag}", "error")
                    return False
                xfer_elapsed = time.time() - xfer_start
                xfer_speed = tar_size_mb / xfer_elapsed if xfer_elapsed > 0 else 0
                self._emit(
                    f"  Transferred in {xfer_elapsed:.1f}s ({xfer_speed:.1f} MB/s)",
                    "info",
                )

                # 3. docker load on remote
                self._emit(f"  Loading {image_tag} on {self._target_host}…", "info")
                load_start = time.time()
                rc_load, out_load, err_load = self._run_remote_ssh(
                    f"docker load -i {remote_tar}", timeout=300
                )
                load_elapsed = time.time() - load_start
                if rc_load != 0:
                    self._emit(
                        f"  docker load failed on remote: {(err_load or out_load).strip()[:200]}",
                        "error",
                    )
                    return False
                self._emit(f"  Loaded {image_tag} on remote in {load_elapsed:.1f}s", "info")

                # 4. Clean up remote tar to save disk space
                self._run_remote_ssh(f"rm -f {remote_tar}", timeout=15)

        finally:
            # Clean up local temp files
            import shutil
            shutil.rmtree(tmp_dir, ignore_errors=True)

        self._emit(
            f"[{self._target_host}] All {len(images_to_transfer)} image(s) transferred successfully",
            "success",
        )

        # Transfer docker-compose.yml and .env to remote
        compose_src = self.work_dir / DOCKER_COMPOSE_FILE
        if not compose_src.is_file():
            # Try repo root docker/ dir
            compose_src = Path(self.profile.get("target", {}).get(
                "repo_root",
                str(Path(__file__).resolve().parent.parent.parent.parent),
            )) / "docker" / DOCKER_COMPOSE_FILE
        if compose_src.is_file():
            remote_compose = f"{self._remote_deploy_dir}/{DOCKER_COMPOSE_FILE}"
            self._emit(f"  Transferring {DOCKER_COMPOSE_FILE} to remote…", "info")
            self._scp_to_target(str(compose_src), remote_compose)
        else:
            self._emit(f"  Warning: {DOCKER_COMPOSE_FILE} not found — remote must already have it", "warn")

        # Transfer .env if it exists in the work directory
        env_src = self.work_dir / ".env"
        if env_src.is_file():
            remote_env = f"{self._remote_deploy_dir}/.env"
            self._emit(f"  Transferring .env to remote…", "info")
            self._scp_to_target(str(env_src), remote_env)

        return True

    def _step_pull_images(self) -> bool:  # NOSONAR - pull failures are non-fatal by design
        build_locally = self.profile.get("image_registry", {}).get("build_locally", False)
        if build_locally:
            # Only pull non-locally-built services (databases, providers, etc.)
            self._emit(f"[{self._target_host}] build_locally=True — pulling only non-local images", "info")
            pull_cmd = [
                "docker", "compose", "-f", DOCKER_COMPOSE_FILE,
                "pull", "--ignore-buildable",
            ]
            # Enumerate services that are NOT locally built
            rc_cfg, stdout_cfg, _ = self._run_on_target(
                ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "config", "--services"]
            )
            if rc_cfg == 0 and stdout_cfg.strip():
                all_services = stdout_cfg.strip().splitlines()
                local_names = set(LOCAL_BUILD_IMAGES.keys())
                remote_services = [s for s in all_services if s not in local_names]
                if remote_services:
                    self._emit(
                        f"[{self._target_host}] Pulling {len(remote_services)} remote image(s): "
                        f"{', '.join(remote_services)}",
                        "info",
                    )
                    pull_cmd = [
                        "docker", "compose", "-f", DOCKER_COMPOSE_FILE,
                        "pull",
                    ] + remote_services
                else:
                    self._emit(f"[{self._target_host}] All images built locally — skipping pull", "info")
                    return True
        else:
            self._emit(f"[{self._target_host}] Pulling all images from registry", "info")
            pull_cmd = ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "pull"]

        pull_start = time.time()
        rc, out, err = self._run_on_target(pull_cmd, timeout=600, log_command=True)
        pull_elapsed = time.time() - pull_start
        if rc != 0 and not self.dry_run:
            self._emit(
                f"[{self._target_host}] Pull failed (non-fatal, took {pull_elapsed:.1f}s): {err.strip()[:300]}",
                "warn",
            )
        else:
            self._emit(f"[{self._target_host}] Image pull completed in {pull_elapsed:.1f}s", "info")
        return True

    def _step_create_networks(self) -> bool:
        self._emit(f"[{self._target_host}] Creating Docker network 'crm-network' (bridge driver)", "info")
        rc, _, err = self._run_on_target(
            ["docker", "network", "create", "crm-network", "--driver", "bridge"],
            log_command=True,
        )
        if rc != 0:
            if "already exists" in (err or ""):
                self._emit(f"[{self._target_host}] Network crm-network already exists — reusing", "info")
            else:
                self._emit(f"[{self._target_host}] Network creation note: {err.strip()}", "warn")
        else:
            self._emit(f"[{self._target_host}] Network crm-network created", "info")
        return True

    # ------------------------------------------------------------------ #
    #  Database credential helpers                                        #
    # ------------------------------------------------------------------ #
    def _get_configured_db_password(self) -> str:
        """Extract the DB password that was configured for this deployment."""
        # Check profile sections in order of specificity
        pw = (
            self.profile.get("database", {}).get("db_password")
            or self.profile.get("secrets", {}).get("db_password")
            or self.profile.get("db_password")
            or ""
        )
        return pw

    def _get_configured_db_root_password(self) -> str:
        """Extract the DB root password that was configured for this deployment."""
        return (
            self.profile.get("database", {}).get("db_root_password")
            or self.profile.get("secrets", {}).get("db_root_password")
            or self.profile.get("db_root_password")
            or ""
        )

    def _validate_db_credentials(
        self, db_user: str, db_password: str, db_name: str
    ) -> bool:
        """Test that *db_user* can connect to MariaDB with *db_password*.

        Tries both ``mariadb`` and ``mysql`` client binaries.
        """
        for client in ("mariadb", "mysql"):
            rc, _, _ = self._run_on_target(
                [
                    "docker", "exec", "crm-mariadb",
                    client, "-u", db_user, f"-p{db_password}",
                    "-e", "SELECT 1 AS auth_ok;", db_name,
                ],
                timeout=15,
            )
            if rc == 0:
                self._emit(
                    f"[{self._target_host}] DB credential validation OK (via {client})",
                    "success",
                )
                return True
        return False

    def _repair_db_credentials(
        self, db_user: str, db_password: str
    ) -> bool:
        """Attempt to ALTER the DB user's password via root to match config.

        MariaDB ignores MYSQL_PASSWORD on existing volumes, so on redeploy
        the only way to sync is via ALTER USER using root credentials.
        """
        root_pw = self._get_configured_db_root_password()
        if not root_pw:
            self._emit(
                f"[{self._target_host}] No root password available — cannot repair credentials",
                "warn",
            )
            return False

        # Escape single quotes in passwords for shell
        escaped_user_pw = db_password.replace("'", "'\\''")

        for client in ("mariadb", "mysql"):
            rc, _, err = self._run_on_target(
                [
                    "docker", "exec", "crm-mariadb",
                    client, "-u", "root", f"-p{root_pw}",
                    "-e",
                    f"ALTER USER '{db_user}'@'%' IDENTIFIED BY '{escaped_user_pw}'; FLUSH PRIVILEGES;",
                ],
                timeout=15,
            )
            if rc == 0:
                self._emit(
                    f"[{self._target_host}] Password for '{db_user}' reset via {client} root access",
                    "info",
                )
                return True

        self._emit(
            f"[{self._target_host}] Root access also failed — "
            f"stderr: {(err or '').strip()[:200]}",
            "error",
        )
        return False

    def _step_start_databases(self) -> bool:
        all_db = ["crm-mariadb", "crm-redis"]
        self._ensure_reused_running(all_db)
        services = self._services_to_start(all_db)
        if not services:
            self._emit(f"[{self._target_host}] All database containers are reused — verified running", "info")
            return True
        self._emit(f"[{self._target_host}] Starting database services: {', '.join(services)}", "info")
        rc, out, err = self._compose_up(services, timeout=120)
        if rc != 0 and not self.dry_run:
            self._emit(f"[{self._target_host}] Database start failed (rc={rc})", "error")
            if err:
                for line in err.strip().splitlines()[-10:]:
                    self._emit(f"  stderr: {line}", "error")
            if out:
                for line in out.strip().splitlines()[-5:]:
                    self._emit(f"  stdout: {line}", "error")
        else:
            self._emit(f"[{self._target_host}] Database services started", "info")
        return rc == 0 or self.dry_run

    def _step_wait_databases(self) -> bool:
        if self.dry_run:
            return True

        self._emit(f"[{self._target_host}] Checking database container health…", "info")

        # Quick check: is crm-mariadb actually running?
        rc, out, _ = self._run_on_target(
            ["docker", "inspect", "-f", "{{.State.Status}}", "crm-mariadb"], timeout=10
        )
        container_state = out.strip() if rc == 0 else "unknown"
        self._emit(f"[{self._target_host}] crm-mariadb container state: {container_state}", "info")

        if container_state not in ("running",):
            self._emit(
                f"[{self._target_host}] crm-mariadb is '{container_state}' — attempting docker start",
                "warn",
            )
            self._run_on_target(["docker", "start", "crm-mariadb"], timeout=15, log_command=True)
            # Re-check state after start
            rc2, out2, _ = self._run_on_target(
                ["docker", "inspect", "-f", "{{.State.Status}}", "crm-mariadb"], timeout=10
            )
            new_state = out2.strip() if rc2 == 0 else "unknown"
            self._emit(f"[{self._target_host}] crm-mariadb state after start: {new_state}", "info")

        # Also check Redis
        rc_r, out_r, _ = self._run_on_target(
            ["docker", "inspect", "-f", "{{.State.Status}}", "crm-redis"], timeout=10
        )
        redis_state = out_r.strip() if rc_r == 0 else "unknown"
        self._emit(f"[{self._target_host}] crm-redis container state: {redis_state}", "info")

        max_attempts = 24
        self._emit(
            f"[{self._target_host}] Pinging MariaDB (max {max_attempts} attempts, 5s interval)…",
            "info",
        )
        for attempt in range(1, max_attempts + 1):
            if self._abort.is_set():
                self._emit(f"[{self._target_host}] Database wait aborted by user", "warn")
                return False

            rc, out, err = self._run_on_target(
                [
                    "docker", "exec", "crm-mariadb",
                    "mysqladmin", "ping", "-h", "localhost", "--silent",
                ],
                timeout=10,
            )
            if rc == 0:
                self._emit(
                    f"[{self._target_host}] MariaDB ready after {attempt} attempt(s)",
                    "success",
                )
                # ── Validate database credentials ──────────────────────
                # MariaDB ignores MYSQL_PASSWORD env var after first volume
                # initialization.  Verify the configured password actually
                # works before letting the API container start (and fail
                # with ACCESS DENIED).
                db_password = self._get_configured_db_password()
                db_user = (
                    self.profile.get("database", {}).get("db_user")
                    or self.profile.get("db_user")
                    or "crm_user"
                )
                db_name = (
                    self.profile.get("database", {}).get("db_name")
                    or self.profile.get("db_name")
                    or "crm_db"
                )
                if db_password:
                    self._emit(
                        f"[{self._target_host}] Validating DB credentials for user '{db_user}'…",
                        "info",
                    )
                    auth_ok = self._validate_db_credentials(
                        db_user, db_password, db_name
                    )
                    if not auth_ok:
                        self._emit(
                            f"[{self._target_host}] DB credential validation FAILED — "
                            "attempting password repair via root…",
                            "warn",
                        )
                        repaired = self._repair_db_credentials(
                            db_user, db_password
                        )
                        if repaired:
                            self._emit(
                                f"[{self._target_host}] DB credentials repaired successfully",
                                "success",
                            )
                        else:
                            self._emit(
                                f"[{self._target_host}] DB credential repair FAILED — "
                                "API may fail to connect. Check DB password manually.",
                                "error",
                            )
                            # Non-fatal: let API attempt anyway — it has retry logic
                else:
                    self._emit(
                        f"[{self._target_host}] No DB password in profile — skipping credential check",
                        "warn",
                    )
                return True

            detail = (err or out or "").strip()[:120]
            self._emit(
                f"[{self._target_host}] Waiting for MariaDB… attempt {attempt}/{max_attempts}"
                + (f" ({detail})" if detail else ""),
                "info",
            )
            time.sleep(5)

        self._emit(
            f"[{self._target_host}] MariaDB health check timed out after {max_attempts * 5}s",
            "error",
        )
        return False

    def _step_run_migrations(self) -> bool:  # NOSONAR - migration step is advisory; API handles schema on startup
        if self.dry_run:
            self._emit(f"[{self._target_host}] [DRY-RUN] Would run EF Core migrations", "info")
            return True

        # The published crm-api container does NOT contain project/source files,
        # so `dotnet ef database update` cannot work (it requires .csproj).
        # Instead, the CRM API automatically runs MigrateAsync() on startup,
        # which applies all pending EF Core migrations.
        #
        # We verify that the database is accepting connections as a pre-check
        # before the API starts in the next steps.

        self._emit(
            f"[{self._target_host}] Verifying database readiness before API startup migration…",
            "info",
        )
        self._emit(
            f"[{self._target_host}] Note: EF Core migrations will be applied automatically "
            "when crm-api starts (MigrateAsync on startup)",
            "info",
        )

        # Quick connectivity check — exec into the mariadb container
        # Use the configured password from the profile — never fall back to hardcoded values
        db_password = self._get_configured_db_password()
        if not db_password:
            self._emit(
                f"[{self._target_host}] ⚠️ No database password configured in profile — "
                "skipping pre-migration DB connectivity check. "
                "To fix: open Day-2 Operations → Secrets tab → 'Configure DB Password'.",
                "warning",
            )
            return
        db_user = (
            self.profile.get("database", {}).get("db_user")
            or self.profile.get("db_user")
            or "crm_user"
        )
        db_name = (
            self.profile.get("database", {}).get("db_name")
            or self.profile.get("db_name")
            or "crm_db"
        )
        mig_start = time.time()
        rc, _, err = self._run_on_target(
            [
                "docker", "exec", "crm-mariadb",
                "mariadb", "-u", db_user, f"-p{db_password}",
                "-e", "SELECT 1 AS db_ready;",
                db_name,
            ],
            timeout=30,
            log_command=True,
        )
        mig_elapsed = time.time() - mig_start

        if rc != 0:
            # Fallback: try mysql client name (older images)
            rc2, _, err2 = self._run_on_target(
                [
                    "docker", "exec", "crm-mariadb",
                    "mysql", "-u", db_user, f"-p{db_password}",
                    "-e", "SELECT 1 AS db_ready;",
                    db_name,
                ],
                timeout=30,
            )
            if rc2 != 0:
                self._emit(
                    f"[{self._target_host}] Database connectivity check failed (took {mig_elapsed:.1f}s)",
                    "warn",
                )
                self._emit(
                    f"  stderr: {(err or err2 or '').strip()[:300]}",
                    "warn",
                )
                self._emit(
                    f"[{self._target_host}] Continuing — API will retry database connection on startup",
                    "warn",
                )
                return True  # Non-fatal: API has its own startup retry logic

        self._emit(
            f"[{self._target_host}] Database ready — migrations will apply during API startup ({mig_elapsed:.1f}s)",
            "success",
        )
        return True

    @staticmethod
    def _resolve_provider(providers: dict, short_key: str, long_key: str) -> str:
        """Return the provider value from *providers* checking both key formats."""
        return providers.get(short_key) or providers.get(long_key) or ""

    # ── Phase 2 helpers ──────────────────────────────────────────────────────

    def _selected_infra_containers(self) -> list[str]:
        """Return the list of infrastructure/monitoring containers selected in the profile."""
        providers = self.profile.get("providers", {})
        wiz = self.profile.get("wizard_config", {})
        merged: dict = {**wiz, **providers} if isinstance(wiz, dict) else dict(providers)

        containers: list[str] = []

        # Reverse proxy
        rp = merged.get("reverse_proxy") or merged.get("reverse_proxy_provider") or ""
        if rp == "traefik":
            containers.append("crm-traefik")
        elif rp in ("nginx", "caddy"):
            # built-in nginx — no separate container needed
            pass

        # Object storage
        store = merged.get("storage") or merged.get("storage_provider") or ""
        if store == "minio":
            containers.append("crm-minio")

        # Portainer (management UI)
        portainer = merged.get("portainer") or merged.get("portainer_provider") or ""
        if portainer in ("portainer_ce", "portainer_be"):
            containers.append("crm-portainer")

        # Monitoring stack
        monitoring = merged.get("monitoring") or merged.get("monitoring_provider") or ""
        if monitoring == "prometheus_grafana":
            containers.extend(["crm-prometheus", "crm-grafana"])
            if merged.get("loki_enabled") or merged.get("monitoring_loki_enabled"):
                containers.append("crm-loki")
            if merged.get("uptime_kuma_enabled") or merged.get("monitoring_uptime_kuma_enabled"):
                containers.append("crm-uptime-kuma")
        elif monitoring == "uptime_kuma":
            containers.append("crm-uptime-kuma")

        return containers

    def _step_start_infrastructure(self) -> bool:
        """Phase 2 — Start reverse proxy, storage, management, and monitoring containers.

        These are deployed *before* databases so that the data layer can
        be accessed through the reverse proxy right from the start and so
        that monitoring can observe database startup from the very beginning.
        Failures here are non-fatal: the core CRM can run without them.
        """
        infra_containers = self._selected_infra_containers()
        if not infra_containers:
            self._emit(
                f"[{self._target_host}] No infrastructure/monitoring containers selected — skipping",
                "info",
            )
            return True

        self._emit(
            f"[{self._target_host}] Infrastructure containers requested: {', '.join(infra_containers)}",
            "info",
        )
        self._ensure_reused_running(infra_containers)
        to_start = self._services_to_start(infra_containers)
        if not to_start:
            self._emit(
                f"[{self._target_host}] All infrastructure containers reused — verified running",
                "info",
            )
            return True

        self._emit(
            f"[{self._target_host}] Starting infrastructure services: {', '.join(to_start)}",
            "info",
        )
        rc, _out, err = self._compose_up(to_start, timeout=90)
        if rc != 0 and not self.dry_run:
            self._emit(
                f"[{self._target_host}] ⚠️  Infrastructure services start warning (rc={rc}) — "
                "continuing (non-fatal)",
                "warn",
            )
            if err:
                for line in err.strip().splitlines()[-5:]:
                    self._emit(f"  stderr: {line}", "warn")
        else:
            self._emit(
                f"[{self._target_host}] Infrastructure services started",
                "info",
            )
        return True  # non-fatal

    def _step_wait_infrastructure(self) -> bool:
        """Phase 2 — Poll infrastructure containers until they respond or timeout.

        Uses HTTP probing where a health endpoint is known; falls back to checking
        the Docker container state via ``docker inspect``.  Non-fatal: if a container
        does not become healthy within the timeout we log a warning and continue.
        """
        if self.dry_run:
            self._emit(
                f"[{self._target_host}] [DRY-RUN] Would wait for infrastructure readiness",
                "info",
            )
            return True

        infra_containers = self._selected_infra_containers()
        if not infra_containers:
            return True

        import urllib.request

        max_wait = 60   # seconds per container
        poll_interval = 5

        for container in infra_containers:
            if self._abort.is_set():
                return False

            check = INFRA_HEALTH_CHECKS.get(container)
            if check:
                url = f"http://{self._target_host}:{check['port']}{check['http_path']}"
                self._emit(
                    f"[{self._target_host}] Waiting for {container} at {url} …",
                    "info",
                )
                deadline = time.time() + max_wait
                ready = False
                while time.time() < deadline:
                    if self._abort.is_set():
                        return False
                    try:
                        urllib.request.urlopen(url, timeout=4)  # noqa: S310
                        ready = True
                        break
                    except Exception:
                        time.sleep(poll_interval)

                if ready:
                    self._emit(
                        f"[{self._target_host}] ✅ {container} is ready",
                        "info",
                    )
                else:
                    self._emit(
                        f"[{self._target_host}] ⚠️  {container} did not respond within {max_wait}s "
                        f"({check['description']}) — continuing",
                        "warn",
                    )
            else:
                # No HTTP probe — just verify container is running
                rc, state_out, _ = self._run_on_target(
                    ["docker", "inspect", "-f", "{{.State.Status}}", container],
                    timeout=10,
                )
                state = state_out.strip() if rc == 0 else "unknown"
                if state == "running":
                    self._emit(
                        f"[{self._target_host}] ✅ {container} is running",
                        "info",
                    )
                else:
                    self._emit(
                        f"[{self._target_host}] ⚠️  {container} state is '{state}' — continuing",
                        "warn",
                    )

        return True  # non-fatal

    # ── Phase 4 helpers ──────────────────────────────────────────────────────

    def _step_wait_providers(self) -> bool:
        """Phase 4 — Wait for provider services to become ready before starting the API.

        Providers whose health endpoint is known in ``PROVIDER_HEALTH_CHECKS`` are
        probed via HTTP.  All others are checked by container state only.
        This step is non-fatal: providers starting slowly will not block the API
        (the API has its own retry/fallback behaviour), but we log a clear warning
        so operators know which providers are lagging.
        """
        if self.dry_run:
            self._emit(
                f"[{self._target_host}] [DRY-RUN] Would wait for provider readiness",
                "info",
            )
            return True

        providers = self.profile.get("providers", {})
        wiz = self.profile.get("wizard_config", {})
        merged: dict = {**wiz, **providers} if isinstance(wiz, dict) else dict(providers)

        # Build the list of provider containers actually selected
        active_providers: list[str] = []
        for category, mapping in PROVIDER_CONTAINER_MAP.items():
            selected = merged.get(category) or merged.get(f"{category}_provider") or ""
            container = mapping.get(selected)
            if container:
                active_providers.append(container)

        if not active_providers:
            self._emit(
                f"[{self._target_host}] No external providers selected — skipping provider readiness check",
                "info",
            )
            return True

        import urllib.request

        max_wait = 90   # seconds — providers (Ollama, Chatwoot) can take a while
        poll_interval = 6

        self._emit(
            f"[{self._target_host}] Waiting for provider readiness: {', '.join(active_providers)}",
            "info",
        )

        for container in active_providers:
            if self._abort.is_set():
                return False

            check = PROVIDER_HEALTH_CHECKS.get(container)
            if check:
                url = f"http://{self._target_host}:{check['port']}{check['http_path']}"
                self._emit(
                    f"[{self._target_host}] Probing {container} at {url} …",
                    "info",
                )
                deadline = time.time() + max_wait
                ready = False
                while time.time() < deadline:
                    if self._abort.is_set():
                        return False
                    try:
                        urllib.request.urlopen(url, timeout=5)  # noqa: S310
                        ready = True
                        break
                    except Exception:
                        time.sleep(poll_interval)

                if ready:
                    self._emit(
                        f"[{self._target_host}] ✅ {container} is ready",
                        "info",
                    )
                else:
                    self._emit(
                        f"[{self._target_host}] ⚠️  {container} not ready after {max_wait}s — "
                        "API will connect when it becomes available",
                        "warn",
                    )
            else:
                # No HTTP probe — quick container state check
                rc, state_out, _ = self._run_on_target(
                    ["docker", "inspect", "-f", "{{.State.Status}}", container],
                    timeout=10,
                )
                state = state_out.strip() if rc == 0 else "unknown"
                level = "info" if state == "running" else "warn"
                self._emit(
                    f"[{self._target_host}] {container} state: {state}",
                    level,
                )

        return True  # non-fatal

    def _step_start_providers(self) -> bool:  # NOSONAR - provider failures are non-fatal; core CRM runs without them
        providers = self.profile.get("providers", {})
        # Also check wizard_config for flat-key format (search_provider, ai_provider, etc.)
        wiz = self.profile.get("wizard_config", {})
        merged = {**wiz, **providers} if isinstance(wiz, dict) else dict(providers)
        extras = []
        if self._resolve_provider(merged, "search", "search_provider") == "meilisearch":
            extras.append("crm-meilisearch")
        if self._resolve_provider(merged, "ai", "ai_provider") == "ollama":
            extras.append("crm-ollama")
        if self._resolve_provider(merged, "chat", "chat_provider") == "chatwoot":
            extras.append("crm-chatwoot")
        if self._resolve_provider(merged, "notification", "notification_provider") == "novu":
            extras.append("crm-novu")
        if self._resolve_provider(merged, "analytics", "analytics_provider") == "superset":
            extras.append("crm-superset")
        if self._resolve_provider(merged, "signature", "signature_provider") == "docuseal":
            extras.append("crm-docuseal")
        if self._resolve_provider(merged, "integration", "integration_provider") == "n8n":
            extras.append("crm-n8n")
        if not extras:
            self._emit(f"[{self._target_host}] No external providers selected — skipping", "info")
            return True
        all_requested = list(extras)
        self._emit(
            f"[{self._target_host}] Requested providers: {', '.join(all_requested)}",
            "info",
        )
        self._ensure_reused_running(all_requested)
        extras = self._services_to_start(all_requested)
        if not extras:
            self._emit(f"[{self._target_host}] All provider containers are reused — verified running", "info")
            return True
        self._emit(f"[{self._target_host}] Starting provider services: {', '.join(extras)}", "info")
        rc, _out, err = self._compose_up(extras, timeout=120)
        if rc != 0 and not self.dry_run:
            self._emit(f"[{self._target_host}] Provider start warning (rc={rc})", "warn")
            if err:
                for line in err.strip().splitlines()[-5:]:
                    self._emit(f"  stderr: {line}", "warn")
        else:
            self._emit(f"[{self._target_host}] Provider services started", "info")
        return True

    def _step_start_api(self) -> bool:
        if "crm-api" in self._reused_containers:
            self._ensure_reused_running(["crm-api"])
            self._emit(f"[{self._target_host}] crm-api container is reused — verified running", "info")
            return True
        self._emit(f"[{self._target_host}] Starting crm-api container…", "info")
        rc, out, err = self._compose_up(["crm-api"], timeout=120)
        if rc != 0 and not self.dry_run:
            self._emit(f"[{self._target_host}] API start failed (rc={rc})", "error")
            if err:
                for line in err.strip().splitlines()[-10:]:
                    self._emit(f"  stderr: {line}", "error")
            if out:
                for line in out.strip().splitlines()[-5:]:
                    self._emit(f"  stdout: {line}", "error")
        else:
            self._emit(f"[{self._target_host}] crm-api container started", "info")
        return rc == 0 or self.dry_run

    def _step_health_check_api(self) -> bool:
        if self.dry_run:
            self._emit(f"[{self._target_host}] [DRY-RUN] Would health check API", "info")
            return True
        import urllib.request

        # Use api_port stored in constructor from profile
        health_url = f"http://{self._target_host}:{self._api_port}/health"
        self._emit(
            f"[{self._target_host}] Health checking API at {health_url} (max 12 attempts, 5s interval)",
            "info",
        )

        max_attempts = 12
        for attempt in range(1, max_attempts + 1):
            if self._abort.is_set():
                self._emit(f"[{self._target_host}] Health check aborted by user", "warn")
                return False
            try:
                urllib.request.urlopen(health_url, timeout=5)  # noqa: S310
                self._emit(
                    f"[{self._target_host}] API is healthy after {attempt} attempt(s)",
                    "success",
                )
                return True
            except Exception as exc:
                detail = str(exc)[:120]
                self._emit(
                    f"[{self._target_host}] Health check attempt {attempt}/{max_attempts}: {detail}",
                    "info",
                )
                time.sleep(5)
        self._emit(
            f"[{self._target_host}] API health check timed out after {max_attempts * 5}s "
            f"— continuing with remaining steps (frontend, seed)",
            "warn",
        )
        # Return True so the deploy continues to start the frontend and finish.
        # The API may still be booting; the frontend (static nginx) does not
        # depend on a healthy API to start.  A hard failure here would silently
        # prevent the frontend from being deployed at all.
        return True

    def _step_start_frontend(self) -> bool:
        if "crm-frontend" in self._reused_containers:
            self._ensure_reused_running(["crm-frontend"])
            self._emit(f"[{self._target_host}] crm-frontend container is reused — verified running", "info")
            return True
        self._emit(f"[{self._target_host}] Starting crm-frontend container…", "info")
        rc, out, err = self._compose_up(["crm-frontend"], timeout=120)
        if rc != 0 and not self.dry_run:
            self._emit(f"[{self._target_host}] Frontend start failed (rc={rc})", "error")
            if err:
                for line in err.strip().splitlines()[-10:]:
                    self._emit(f"  stderr: {line}", "error")
            if out:
                for line in out.strip().splitlines()[-5:]:
                    self._emit(f"  stdout: {line}", "error")
        else:
            self._emit(f"[{self._target_host}] crm-frontend container started", "info")

        # Verify the container is actually running after compose up
        if not self.dry_run:
            time.sleep(2)  # brief settle time
            rc_chk, state_out, _ = self._run_on_target(
                ["docker", "inspect", "-f", "{{.State.Status}}", "crm-frontend"], timeout=10
            )
            state = state_out.strip() if rc_chk == 0 else "unknown"
            if state != "running":
                self._emit(
                    f"[{self._target_host}] WARNING: crm-frontend container state is '{state}' after start attempt",
                    "warn",
                )
                # Check container logs for clues
                rc_log, log_out, _ = self._run_on_target(
                    ["docker", "logs", "--tail", "20", "crm-frontend"], timeout=10
                )
                if rc_log == 0 and log_out.strip():
                    self._emit(f"[{self._target_host}] crm-frontend container logs:", "warn")
                    for line in log_out.strip().splitlines()[-10:]:
                        self._emit(f"  {line}", "warn")
                return False
            self._emit(f"[{self._target_host}] crm-frontend container verified running", "info")

        return rc == 0 or self.dry_run

    def _step_configure_seed(self) -> bool:
        seed = self.profile.get("seed", {})
        if seed.get("seed_master_data") and not self.dry_run:
            import urllib.request

            seed_url = f"http://{self._target_host}:{self._api_port}/api/admin/seed/master-data"
            self._emit(f"[{self._target_host}] Seeding master data via POST {seed_url}", "info")
            try:
                req = urllib.request.Request(seed_url, method="POST")
                urllib.request.urlopen(req, timeout=30)  # noqa: S310
                self._emit(f"[{self._target_host}] Master data seeded successfully", "success")
            except Exception as e:
                self._emit(f"[{self._target_host}] Seed warning: {e}", "warn")
        else:
            reason = "dry-run" if self.dry_run else "not requested"
            self._emit(f"[{self._target_host}] Seed skipped ({reason})", "info")
        return True

    def _step_finish(self) -> bool:
        # Show final container status for the log
        rc, ps_out, _ = self._run_on_target(
            ["docker", "ps", "--filter", "name=crm-", "--format", "table {{.Names}}\t{{.Status}}\t{{.Ports}}"],
            timeout=10,
        )
        if rc == 0 and ps_out.strip():
            self._emit(f"[{self._target_host}] Final container status:", "info")
            for line in ps_out.strip().splitlines():
                self._emit(f"  {line}", "info")

        protocol = "https" if self._ssl_enabled else "http"
        self._emit(
            f"[{self._target_host}] Deployment complete!",
            "success",
            self.total_steps,
        )
        self._emit(
            f"  Frontend: {protocol}://{self._target_host}:{self._frontend_port}/",
            "success",
        )
        self._emit(
            f"  API:      {protocol}://{self._target_host}:{self._api_port}/health",
            "success",
        )

        # ── Push encrypted profile bundle to deployment target ──
        try:
            from core.profile import ProfileManager
            from core.vault import encrypt_data

            pm = ProfileManager()
            profile_name = (
                self.profile.get("meta", {}).get("profile_name")
                or self.profile.get("name")
                or "default"
            )

            # Collect profile JSON + artifacts into a single bundle dict
            bundle: dict = {"profile": self.profile}
            try:
                artifacts = pm.load_artifacts(profile_name)
                if artifacts:
                    bundle["artifacts"] = artifacts
            except Exception:
                pass  # No artifacts yet — that's fine

            bundle_json = json.dumps(bundle, indent=2, default=str).encode("utf-8")
            # Encrypt with a deterministic password derived from profile name + host
            enc_password = f"crm-cdt-{profile_name}-{self._target_host}"
            encrypted = encrypt_data(bundle_json, enc_password)

            remote_path = f"{self._remote_deploy_dir}/.crm-profile.enc"

            if self._is_local:
                import pathlib
                pathlib.Path(remote_path).parent.mkdir(parents=True, exist_ok=True)
                pathlib.Path(remote_path).write_bytes(encrypted)
                self._emit(f"Encrypted profile saved to {remote_path}", "info")
            else:
                # Write temp file, SCP to target
                import tempfile
                with tempfile.NamedTemporaryFile(delete=False, suffix=".enc") as tmp:
                    tmp.write(encrypted)
                    tmp_path = tmp.name
                try:
                    scp_cmd = [
                        "scp", "-o", "StrictHostKeyChecking=no",
                        "-P", str(self._ssh_port),
                        tmp_path,
                        f"{self._ssh_user}@{self._target_host}:{remote_path}",
                    ]
                    subprocess.run(scp_cmd, timeout=30, capture_output=True, check=True)
                    self._emit(f"Encrypted profile pushed to {self._target_host}:{remote_path}", "info")
                finally:
                    os.unlink(tmp_path)
        except Exception as enc_exc:
            self._emit(f"Warning: Could not save encrypted profile: {enc_exc}", "warn")

        return True

    def rollback(self) -> bool:
        self._emit(f"[{self._target_host}] Rolling back — stopping all containers", "warn")
        rc, _, _ = self._run_on_target(["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "down"], log_command=True)
        self._emit(f"[{self._target_host}] Rollback complete", "info")
        return rc == 0 or self.dry_run

    def status(self) -> dict:
        rc, out, _ = self._run_on_target(["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "ps", "--format", "json"])
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


# ════════════════════════════════════════════════════════════════════════ #
#  Registry helper utilities (used by registry_routes blueprint)          #
# ════════════════════════════════════════════════════════════════════════ #

CRM_IMAGE_PREFIX = "crm-"


def _run_cmd(cmd: list, timeout: int = 30) -> tuple:
    """Run a shell command and return (rc, stdout, stderr)."""
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
        return (result.returncode, result.stdout, result.stderr)
    except Exception as exc:
        return (1, "", str(exc))


def list_local_images(filter_crm: bool = True) -> list:
    """Return a list of dicts describing local Docker images.

    Each dict: {repository, tag, image_id, created, size}.
    When *filter_crm* is True only images whose repo starts with ``crm-`` are
    returned.
    """
    rc, out, _ = _run_cmd(
        ["docker", "images", "--format", "{{json .}}"]
    )
    if rc != 0 or not out.strip():
        return []
    images = []
    for line in out.strip().splitlines():
        try:
            obj = json.loads(line)
        except json.JSONDecodeError:
            continue
        repo = obj.get("Repository", "")
        if filter_crm and not repo.startswith(CRM_IMAGE_PREFIX):
            continue
        images.append({
            "repository": repo,
            "tag": obj.get("Tag", ""),
            "image_id": obj.get("ID", ""),
            "created": obj.get("CreatedSince", ""),
            "size": obj.get("Size", ""),
        })
    return images


def get_deployed_image_versions() -> list:
    """Return image info for running CRM containers.

    Each dict: {container, image, image_id, status, needs_update: bool}.
    ``needs_update`` is True when a newer local image exists that differs from
    the running container's image ID.
    """
    rc, out, _ = _run_cmd(
        ["docker", "ps", "--filter", "name=crm-", "--format", "{{json .}}"]
    )
    if rc != 0 or not out.strip():
        return []

    # Build a lookup: repo -> newest local image id
    local = {}
    for img in list_local_images(filter_crm=True):
        key = f"{img['repository']}:{img['tag']}"
        local[key] = img["image_id"]

    result = []
    for line in out.strip().splitlines():
        try:
            obj = json.loads(line)
        except json.JSONDecodeError:
            continue
        container_image = obj.get("Image", "")
        container_id = obj.get("ID", "")
        # Get the image ID the container was started with
        irc, iout, _ = _run_cmd(
            ["docker", "inspect", "--format", "{{.Image}}", container_id]
        )
        running_image_id = iout.strip()[:12] if irc == 0 else ""
        newest_id = local.get(container_image, "")
        result.append({
            "container": obj.get("Names", ""),
            "image": container_image,
            "image_id": running_image_id,
            "status": obj.get("Status", ""),
            "needs_update": bool(newest_id and newest_id != running_image_id),
        })
    return result


def purge_images(image_ids: list = None, dangling_only: bool = False) -> dict:
    """Remove specified images or dangling images.

    Returns dict with 'removed' (list) and 'errors' (list).
    """
    removed, errors = [], []
    if dangling_only:
        rc, out, err = _run_cmd(["docker", "image", "prune", "-f"])
        if rc == 0:
            removed.append(out.strip())
        else:
            errors.append(err.strip())
    elif image_ids:
        for iid in image_ids:
            rc, _out, err = _run_cmd(["docker", "rmi", str(iid)])
            if rc == 0:
                removed.append(iid)
            else:
                errors.append(f"{iid}: {err.strip()}")
    return {"removed": removed, "errors": errors}


# Recommended registry config per platform
PLATFORM_REGISTRY_DEFAULTS = {
    "local_docker": {
        "registry_type": "local",
        "image_registry": "",
        "image_org": "",
        "build_locally": True,
        "hint": "Images built from source on this machine.",
    },
    "on_premises": {
        "registry_type": "local",
        "image_registry": "",
        "image_org": "",
        "build_locally": True,
        "hint": "Build locally or push to a private registry (e.g. registry.internal:5000).",
    },
    "azure": {
        "registry_type": "acr",
        "image_registry": "<your-acr-name>.azurecr.io",
        "image_org": "crm",
        "build_locally": False,
        "hint": "Use Azure Container Registry (ACR). Create with: az acr create -n <name> -g <rg> --sku Basic",
    },
    "aws": {
        "registry_type": "ecr",
        "image_registry": "<account-id>.dkr.ecr.<region>.amazonaws.com",
        "image_org": "crm",
        "build_locally": False,
        "hint": "Use Amazon ECR. Create repos with: aws ecr create-repository --repository-name crm/<svc>",
    },
    "gcp": {
        "registry_type": "gar",
        "image_registry": "<region>-docker.pkg.dev/<project-id>",
        "image_org": "crm",
        "build_locally": False,
        "hint": "Use Google Artifact Registry. Create with: gcloud artifacts repositories create crm --repository-format=docker",
    },
}


def recommend_registry(platform: str) -> dict:
    """Return recommended registry configuration for the given platform."""
    return PLATFORM_REGISTRY_DEFAULTS.get(
        platform,
        PLATFORM_REGISTRY_DEFAULTS["local_docker"],
    )
