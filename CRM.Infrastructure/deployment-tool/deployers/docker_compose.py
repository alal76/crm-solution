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
        self.total_steps = 13

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
            (1,  "Validating prerequisites",  self._step_validate_prerequisites),
            (2,  "Building local images",      self._step_build_local_images),
            (3,  "Pulling images",             self._step_pull_images),
            (4,  "Creating networks",          self._step_create_networks),
            (5,  "Starting databases",         self._step_start_databases),
            (6,  "Waiting for databases",      self._step_wait_databases),
            (7,  "Running migrations",         self._step_run_migrations),
            (8,  "Starting providers",         self._step_start_providers),
            (9,  "Starting API",               self._step_start_api),
            (10, "Health checking API",        self._step_health_check_api),
            (11, "Starting frontend",          self._step_start_frontend),
            (12, "Seeding data",               self._step_configure_seed),
            (13, "Finishing",                  self._step_finish),
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

    # ------------------------------------------------------------------ #
    #  Step 2 — Build Local Images (skipped when using a remote registry) #
    # ------------------------------------------------------------------ #
    def _step_build_local_images(self) -> bool:
        """Build CRM Docker images from source when build_locally is enabled."""
        img_reg = self.profile.get("image_registry", {})
        build_locally = img_reg.get("build_locally", False)

        if not build_locally:
            self._emit("Skipping local build — using registry images", "info")
            return True

        # Resolve repo root: profile may override, else walk up from this file
        repo_root = self.profile.get("target", {}).get("repo_root")
        if not repo_root:
            repo_root = str(Path(__file__).resolve().parent.parent.parent.parent)
        repo_root = Path(repo_root)

        if not repo_root.is_dir():
            self._emit(f"Repo root not found: {repo_root}", "error")
            return False

        # Determine image tag — use crm_version from profile or 'latest'
        tag = self.profile.get("target", {}).get("crm_version", "latest")

        built, failed = 0, 0
        for name, dockerfile in LOCAL_BUILD_IMAGES.items():
            df_path = repo_root / dockerfile
            if not df_path.is_file():
                self._emit(f"Dockerfile not found: {df_path} — skipping {name}", "warn")
                continue

            image_tag = f"{name}:{tag}"
            self._emit(f"Building {image_tag} from {dockerfile} …")
            rc, _out, err = self._run(
                [
                    "docker", "build",
                    "--platform", "linux/amd64",
                    "-t", image_tag,
                    "-f", str(df_path),
                    ".",
                ],
                cwd=repo_root,
                timeout=600,
            )
            if rc != 0:
                self._emit(f"Build failed for {name}: {err[:300]}", "error")
                failed += 1
            else:
                self._emit(f"Built {image_tag} successfully", "success")
                built += 1

        if failed > 0:
            self._emit(f"{failed} image(s) failed to build", "error")
            return False
        self._emit(f"{built} image(s) built successfully", "success")
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
        if extras:
            self._emit(f"Starting provider services: {', '.join(extras)}", "info")
            self._run(
                ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "up", "-d"] + extras,
                timeout=120,
            )
        else:
            self._emit("No external providers selected — skipping", "info")
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
