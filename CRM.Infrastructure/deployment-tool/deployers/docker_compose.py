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

# Images that may be built locally (name → Dockerfile relative to repo root)
LOCAL_BUILD_IMAGES = {
    "crm-api": "docker/Dockerfile.backend",
    "crm-frontend": "docker/Dockerfile.frontend",
}


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


# Container group classification
CONTAINER_GROUPS = {
    "app": ["crm-api", "crm-frontend"],
    "database": ["crm-mariadb", "crm-redis"],
    "provider": [
        "crm-meilisearch", "crm-ollama", "crm-chatwoot",
        "crm-novu", "crm-superset", "crm-docuseal", "crm-n8n",
    ],
}


def classify_container(name: str) -> str:
    """Return the group name for a given container name."""
    for group, members in CONTAINER_GROUPS.items():
        if name in members:
            return group
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
        self.total_steps = 14
        self._step_start_time: float = 0.0

        # Resolve target server label for logging context
        target = profile.get("target", {})
        self._target_host = target.get("domain_name") or target.get("host") or "localhost"
        self._target_platform = profile.get("platform", profile.get("architecture", "docker_compose"))
        if isinstance(self._target_platform, dict):
            self._target_platform = self._target_platform.get("container_runtime", "docker_compose")
        self._target_ssh_user = target.get("ssh_user", "")
        self._target_ssh_port = target.get("ssh_port", 22)

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

    def _ensure_reused_running(self, containers: list[str]) -> bool:
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
            rc_insp, state_out, _ = self._run(
                ["docker", "inspect", "-f", "{{.State.Status}}", name], timeout=10
            )
            state = state_out.strip() if rc_insp == 0 else "unknown"
            if state == "running":
                self._emit(f"  {name}: already running — no action needed", "info")
                continue
            self._emit(f"  {name}: state={state} — issuing docker start", "info")
            rc, _, err = self._run(["docker", "start", name], timeout=30)
            if rc != 0 and not self.dry_run:
                self._emit(f"  Could not start {name}: {err.strip()}", "warn")
            else:
                self._emit(f"  {name}: started successfully", "info")
        return True

    def _emit(self, message: str, level: str = "info", step: int = 0) -> None:
        pct = int((step / self.total_steps) * 100) if self.total_steps else 0
        event = DeployEvent(time.time(), level, message, step, self.total_steps, pct)
        self.log_queue.put(event)
        print(f"[{level.upper()}] {message}")

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
                if stripped and time.time() - last_emit > 0.5:
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

    def deploy(self) -> bool:
        steps = [
            (1,  "Validating prerequisites",  self._step_validate_prerequisites),
            (2,  "Checking existing containers", self._step_handle_existing_containers),
            (3,  "Building local images",      self._step_build_local_images),
            (4,  "Pulling images",             self._step_pull_images),
            (5,  "Creating networks",          self._step_create_networks),
            (6,  "Starting databases",         self._step_start_databases),
            (7,  "Waiting for databases",      self._step_wait_databases),
            (8,  "Running migrations",         self._step_run_migrations),
            (9,  "Starting providers",         self._step_start_providers),
            (10, "Starting API",               self._step_start_api),
            (11, "Health checking API",        self._step_health_check_api),
            (12, "Starting frontend",          self._step_start_frontend),
            (13, "Seeding data",               self._step_configure_seed),
            (14, "Finishing",                  self._step_finish),
        ]

        # ── Deployment header ──
        self._emit("═" * 60, "info")
        self._emit(f"CRM Deployment starting on {self._target_host}", "info")
        self._emit(f"  Platform:         {self._target_platform}", "info")
        self._emit(f"  Docker platform:  {self._target_docker_platform}", "info")
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
        rc, out, err = self._run(["docker", "info"], log_command=True)
        if rc != 0:
            self._emit(
                f"[{self._target_host}] Docker not available. "
                "Install Docker Desktop or Docker Engine.", "error"
            )
            if err:
                self._emit(f"  Error: {err.strip()[:300]}", "error")
            return False

        # Extract Docker version for the log
        rc_v, ver_out, _ = self._run(["docker", "version", "--format", "{{.Server.Version}}"], timeout=10)
        docker_ver = ver_out.strip() if rc_v == 0 else "unknown"
        self._emit(f"[{self._target_host}] Docker is available (version {docker_ver})", "info")

        # Log Docker Compose version too
        rc_c, comp_out, _ = self._run(["docker", "compose", "version", "--short"], timeout=10)
        compose_ver = comp_out.strip() if rc_c == 0 else "unknown"
        self._emit(f"[{self._target_host}] Docker Compose version: {compose_ver}", "info")

        # Check disk space on the target
        rc_d, df_out, _ = self._run(["df", "-h", str(self.work_dir)], timeout=10)
        if rc_d == 0 and df_out.strip():
            lines = df_out.strip().splitlines()
            if len(lines) >= 2:
                self._emit(f"[{self._target_host}] Disk space: {lines[-1].strip()}", "info")

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
        rc, out, _ = self._run(
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
            rc, _, err = self._run(["docker", "rm", "-f", name], timeout=15)
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
        return True

    # ------------------------------------------------------------------ #
    #  Step 3 — Build Local Images (skipped when using a remote registry) #
    # ------------------------------------------------------------------ #
    def _step_build_local_images(self) -> bool:
        """Build CRM Docker images from source when build_locally is enabled."""
        img_reg = self.profile.get("image_registry", {})
        build_locally = img_reg.get("build_locally", False)

        if not build_locally:
            registry = img_reg.get("image_registry", "Docker Hub / default")
            self._emit(
                f"[{self._target_host}] Skipping local build — using registry images from {registry}",
                "info",
            )
            return True

        # Resolve repo root: profile may override, else walk up from this file
        repo_root = self.profile.get("target", {}).get("repo_root")
        if not repo_root:
            repo_root = str(Path(__file__).resolve().parent.parent.parent.parent)
        repo_root = Path(repo_root)

        if not repo_root.is_dir():
            self._emit(f"[{self._target_host}] Repo root not found: {repo_root}", "error")
            return False

        # Determine image tag — use crm_version from profile or 'latest'
        tag = self.profile.get("target", {}).get("crm_version", "latest")

        self._emit(f"[{self._target_host}] Build configuration:", "info")
        self._emit(f"  Repo root:   {repo_root}", "info")
        self._emit(f"  Image tag:   {tag}", "info")
        self._emit(f"  Platform:    {self._target_docker_platform}", "info")
        self._emit(f"  Images to build: {', '.join(LOCAL_BUILD_IMAGES.keys())}", "info")

        built, failed = 0, 0
        for name, dockerfile in LOCAL_BUILD_IMAGES.items():
            df_path = repo_root / dockerfile
            if not df_path.is_file():
                self._emit(f"[{self._target_host}] Dockerfile not found: {df_path} — skipping {name}", "warn")
                continue

            image_tag = f"{name}:{tag}"
            self._emit(f"[{self._target_host}] ── Building {image_tag} ──", "info")
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
            build_elapsed = time.time() - build_start
            if rc != 0:
                self._emit(
                    f"[{self._target_host}] Build FAILED for {name} (exit code {rc}, took {build_elapsed:.1f}s)",
                    "error",
                )
                failed += 1
            else:
                self._emit(
                    f"[{self._target_host}] Built {image_tag} successfully in {build_elapsed:.1f}s",
                    "success",
                )
                built += 1

        if failed > 0:
            self._emit(f"[{self._target_host}] {failed} image(s) failed to build", "error")
            return False
        self._emit(f"[{self._target_host}] {built} image(s) built successfully", "success")
        return True

    def _step_pull_images(self) -> bool:
        build_locally = self.profile.get("image_registry", {}).get("build_locally", False)
        if build_locally:
            # Only pull non-locally-built services (databases, providers, etc.)
            self._emit(f"[{self._target_host}] build_locally=True — pulling only non-local images", "info")
            pull_cmd = [
                "docker", "compose", "-f", DOCKER_COMPOSE_FILE,
                "pull", "--ignore-buildable",
            ]
            # Enumerate services that are NOT locally built
            rc_cfg, stdout_cfg, _ = self._run(
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
        rc, out, err = self._run(pull_cmd, timeout=600, log_command=True)
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
        rc, _, err = self._run(
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

    def _step_start_databases(self) -> bool:
        all_db = ["crm-mariadb", "crm-redis"]
        self._ensure_reused_running(all_db)
        services = self._services_to_start(all_db)
        if not services:
            self._emit(f"[{self._target_host}] All database containers are reused — verified running", "info")
            return True
        self._emit(f"[{self._target_host}] Starting database services: {', '.join(services)}", "info")
        rc, out, err = self._run(
            [
                "docker", "compose", "-f", DOCKER_COMPOSE_FILE,
                "up", "-d",
            ] + services,
            timeout=120,
            log_command=True,
        )
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
        rc, out, _ = self._run(
            ["docker", "inspect", "-f", "{{.State.Status}}", "crm-mariadb"], timeout=10
        )
        container_state = out.strip() if rc == 0 else "unknown"
        self._emit(f"[{self._target_host}] crm-mariadb container state: {container_state}", "info")

        if container_state not in ("running",):
            self._emit(
                f"[{self._target_host}] crm-mariadb is '{container_state}' — attempting docker start",
                "warn",
            )
            self._run(["docker", "start", "crm-mariadb"], timeout=15, log_command=True)
            # Re-check state after start
            rc2, out2, _ = self._run(
                ["docker", "inspect", "-f", "{{.State.Status}}", "crm-mariadb"], timeout=10
            )
            new_state = out2.strip() if rc2 == 0 else "unknown"
            self._emit(f"[{self._target_host}] crm-mariadb state after start: {new_state}", "info")

        # Also check Redis
        rc_r, out_r, _ = self._run(
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

            rc, out, err = self._run(
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

    def _step_run_migrations(self) -> bool:
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
        mig_start = time.time()
        rc, out, err = self._run(
            [
                "docker", "exec", "crm-mariadb",
                "mariadb", "-u", "crm_user", "-pCrmPass@Dev2024",
                "-e", "SELECT 1 AS db_ready;",
                "crm_db",
            ],
            timeout=30,
            log_command=True,
        )
        mig_elapsed = time.time() - mig_start

        if rc != 0:
            # Fallback: try mysql client name (older images)
            rc2, out2, err2 = self._run(
                [
                    "docker", "exec", "crm-mariadb",
                    "mysql", "-u", "crm_user", "-pCrmPass@Dev2024",
                    "-e", "SELECT 1 AS db_ready;",
                    "crm_db",
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

    def _step_start_providers(self) -> bool:
        providers = self.profile.get("providers", {})
        extras = []
        if providers.get("search_provider") == "meilisearch":
            extras.append("crm-meilisearch")
        if providers.get("ai_provider") == "ollama":
            extras.append("crm-ollama")
        if providers.get("chat_provider") == "chatwoot":
            extras.append("crm-chatwoot")
        if providers.get("notification_provider") == "novu":
            extras.append("crm-novu")
        if providers.get("analytics_provider") == "superset":
            extras.append("crm-superset")
        if providers.get("signature_provider") == "docuseal":
            extras.append("crm-docuseal")
        if providers.get("integration_provider") == "n8n":
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
        rc, out, err = self._run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d"] + extras,
            timeout=120,
            log_command=True,
        )
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
        rc, out, err = self._run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d", "crm-api"],
            timeout=120,
            log_command=True,
        )
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

        # Determine the health URL based on target
        api_port = self.profile.get("target", {}).get("api_port", "5000")
        health_url = f"http://{self._target_host}:{api_port}/health"
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
            f"[{self._target_host}] API health check timed out after {max_attempts * 5}s — continuing anyway",
            "warn",
        )
        return False

    def _step_start_frontend(self) -> bool:
        if "crm-frontend" in self._reused_containers:
            self._ensure_reused_running(["crm-frontend"])
            self._emit(f"[{self._target_host}] crm-frontend container is reused — verified running", "info")
            return True
        self._emit(f"[{self._target_host}] Starting crm-frontend container…", "info")
        rc, out, err = self._run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d", "crm-frontend"],
            timeout=120,
            log_command=True,
        )
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
        return rc == 0 or self.dry_run

    def _step_configure_seed(self) -> bool:
        seed = self.profile.get("seed", {})
        if seed.get("seed_master_data") and not self.dry_run:
            import urllib.request

            api_port = self.profile.get("target", {}).get("api_port", "5000")
            seed_url = f"http://{self._target_host}:{api_port}/api/admin/seed/master-data"
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
        domain = self.profile.get("target", {}).get("domain_name", "localhost")
        # Show final container status for the log
        rc, ps_out, _ = self._run(
            ["docker", "ps", "--filter", "name=crm-", "--format", "table {{.Names}}\t{{.Status}}\t{{.Ports}}"],
            timeout=10,
        )
        if rc == 0 and ps_out.strip():
            self._emit(f"[{self._target_host}] Final container status:", "info")
            for line in ps_out.strip().splitlines():
                self._emit(f"  {line}", "info")

        ssl_enabled = self.profile.get("ssl", {}).get("ssl_enabled", False)
        protocol = "https" if ssl_enabled else "http"
        self._emit(
            f"[{self._target_host}] Deployment complete! CRM is available at {protocol}://{domain}",
            "success",
            self.total_steps,
        )
        return True

    def rollback(self) -> bool:
        self._emit(f"[{self._target_host}] Rolling back — stopping all containers", "warn")
        rc, _, _ = self._run(["docker", "compose", "down"], log_command=True)
        self._emit(f"[{self._target_host}] Rollback complete", "info")
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
