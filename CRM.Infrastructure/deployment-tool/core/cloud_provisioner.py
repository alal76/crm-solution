#!/usr/bin/env python3
"""
CRM CDT — Cloud Provisioning Orchestrator

For cloud deployments, each pluggable component may be provisioned as a separate
managed service (e.g. AWS OpenSearch, Azure Database for MariaDB, Novu SaaS, etc.).
This module provisions components one-by-one, captures their external endpoints,
and feeds those endpoints back to the CDT generator so the final API/frontend
containers are configured with the correct URLs.

Deployment order:
  1. Data stores   (MariaDB/RDS, Redis/ElastiCache)
  2. Providers     (Meilisearch, Novu, Superset, Chatwoot, DocuSeal, n8n, Ollama)
  3. Core app      (CRM API, CRM Frontend)

Each step can return endpoint overrides that are accumulated and injected into the
final configuration generation pass.

Usage (programmatic)::

    from core.cloud_provisioner import CloudProvisioner
    prov = CloudProvisioner(profile, platform="aws")
    result = prov.provision_all()
    # result.endpoint_overrides → {"meilisearch_url": "https://...", ...}

Usage (CLI)::

    python -m core.cloud_provisioner --profile generated/deployment-config.json \\
                                     --platform aws --dry-run
"""
from __future__ import annotations

import json
import logging
import subprocess
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

logger = logging.getLogger("cdt.cloud_provisioner")


# ---------------------------------------------------------------------------
# Data types
# ---------------------------------------------------------------------------

@dataclass
class ProvisionResult:
    """Result of provisioning a single component."""
    component: str
    success: bool
    endpoint: str = ""
    message: str = ""
    details: dict = field(default_factory=dict)


@dataclass
class CloudProvisioningResult:
    """Aggregate result of provisioning all components."""
    success: bool
    endpoint_overrides: dict[str, str] = field(default_factory=dict)
    component_results: list[ProvisionResult] = field(default_factory=list)
    errors: list[str] = field(default_factory=list)


# ---------------------------------------------------------------------------
# Provider component definitions
# ---------------------------------------------------------------------------

# Each entry: (context_provider_key, provider_value, endpoint_override_key,
#              default_port, description)
PROVIDER_COMPONENTS = [
    ("search_provider",       "meilisearch", "meilisearch_url", 7700,  "Meilisearch search engine"),
    ("chat_provider",         "chatwoot",    "chatwoot_url",    3003,  "Chatwoot live chat"),
    ("notification_provider", "novu",        "novu_url",        3000,  "Novu notification service"),
    ("analytics_provider",    "superset",    "superset_url",    8088,  "Apache Superset analytics"),
    ("signature_provider",    "docuseal",    "docuseal_url",    3004,  "DocuSeal e-signatures"),
    ("integration_provider",  "n8n",         "n8n_url",         5678,  "n8n workflow automation"),
    ("ai_provider",           "ollama",      "ollama_url",      11434, "Ollama local LLM"),
]


# ---------------------------------------------------------------------------
# Cloud provisioner
# ---------------------------------------------------------------------------

class CloudProvisioner:
    """Provisions cloud infrastructure for each CRM component and captures endpoints.

    This is the orchestration layer between the CDT wizard profile and the
    actual cloud CLI commands (az, aws, gcloud) or managed-service APIs.
    """

    SUPPORTED_PLATFORMS = ("aws", "azure", "gcp", "docker_compose")

    def __init__(
        self,
        profile: dict,
        platform: str = "docker_compose",
        dry_run: bool = False,
        region: str = "",
    ):
        self.profile = profile
        self.platform = platform.lower()
        self.dry_run = dry_run
        self.region = region
        self._endpoint_overrides: dict[str, str] = {}

        if self.platform not in self.SUPPORTED_PLATFORMS:
            raise ValueError(
                f"Unsupported platform '{platform}'. "
                f"Supported: {', '.join(self.SUPPORTED_PLATFORMS)}"
            )

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def provision_all(self) -> CloudProvisioningResult:
        """Provision all selected components in dependency order.

        Returns a :class:`CloudProvisioningResult` containing endpoint overrides
        that should be passed to ``ConfigGenerator.generate(endpoint_overrides=...)``.
        """
        result = CloudProvisioningResult(success=True)

        # Phase 1: Data stores
        logger.info("Phase 1: Provisioning data stores...")
        for component in ("mariadb", "redis"):
            res = self._provision_datastore(component)
            result.component_results.append(res)
            if not res.success:
                result.errors.append(f"Data store '{component}' failed: {res.message}")
                result.success = False
                return result  # data stores are critical — abort on failure
            if res.endpoint:
                self._endpoint_overrides[f"{component}_url"] = res.endpoint

        # Phase 2: Provider services (order doesn't matter, but sequential
        # for now; future: parallelize with asyncio)
        logger.info("Phase 2: Provisioning provider services...")
        providers = self.profile.get("providers", {})
        # Also check top-level provider keys (the wizard orphan-key pattern)
        for ctx_key, expected_val, endpoint_key, default_port, desc in PROVIDER_COMPONENTS:
            selected = providers.get(ctx_key) or self.profile.get(ctx_key, "")
            if selected and selected.lower() != "builtin":
                logger.info(f"  Provisioning {desc} ({selected})...")
                res = self._provision_provider(
                    ctx_key, selected, endpoint_key, default_port
                )
                result.component_results.append(res)
                if res.success and res.endpoint:
                    self._endpoint_overrides[endpoint_key] = res.endpoint
                elif not res.success:
                    result.errors.append(f"Provider '{desc}' failed: {res.message}")
                    # Non-critical: continue so the remaining providers can be
                    # provisioned.  The failed one will use a fallback or be
                    # skipped.

        # Phase 3: Core application (API + Frontend)
        # These are provisioned last because they depend on the endpoints
        # captured from phases 1 & 2.
        logger.info("Phase 3: Core application provisioning deferred to deployer.")

        result.endpoint_overrides = dict(self._endpoint_overrides)
        return result

    def get_endpoint_overrides(self) -> dict[str, str]:
        """Return captured endpoint overrides (valid after :meth:`provision_all`)."""
        return dict(self._endpoint_overrides)

    # ------------------------------------------------------------------
    # Data store provisioning
    # ------------------------------------------------------------------

    def _provision_datastore(self, component: str) -> ProvisionResult:
        """Provision a data store (MariaDB or Redis)."""
        if self.platform == "docker_compose":
            return self._docker_compose_noop(component)

        dispatch = {
            ("aws", "mariadb"):   self._aws_provision_rds,
            ("aws", "redis"):     self._aws_provision_elasticache,
            ("azure", "mariadb"): self._azure_provision_mysql,
            ("azure", "redis"):   self._azure_provision_redis,
            ("gcp", "mariadb"):   self._gcp_provision_cloudsql,
            ("gcp", "redis"):     self._gcp_provision_memorystore,
        }
        fn = dispatch.get((self.platform, component))
        if fn is None:
            return ProvisionResult(
                component=component, success=True,
                message=f"No cloud provisioner for {self.platform}/{component} — using default"
            )
        return fn()

    # ------------------------------------------------------------------
    # Provider provisioning
    # ------------------------------------------------------------------

    def _provision_provider(
        self, ctx_key: str, selected: str, endpoint_key: str, default_port: int
    ) -> ProvisionResult:
        """Provision a pluggable provider service."""
        if self.platform == "docker_compose":
            return self._docker_compose_noop(selected)

        # For cloud: check if user supplied an explicit external endpoint
        # in the profile (e.g. a managed SaaS URL). If so, skip provisioning
        # and just return it.
        explicit_url = (
            self.profile.get(endpoint_key)
            or self.profile.get("providers", {}).get(endpoint_key)
        )
        if explicit_url:
            return ProvisionResult(
                component=selected, success=True,
                endpoint=explicit_url,
                message=f"Using explicit endpoint from profile: {explicit_url}"
            )

        # Platform-specific provisioning
        dispatch = {
            "aws":   self._aws_provision_container,
            "azure": self._azure_provision_container,
            "gcp":   self._gcp_provision_cloudrun,
        }
        fn = dispatch.get(self.platform)
        if fn is None:
            return ProvisionResult(
                component=selected, success=True,
                message=f"No cloud provisioner for {self.platform}/{selected}"
            )
        return fn(selected, endpoint_key, default_port)

    # ------------------------------------------------------------------
    # Docker Compose (no-op for local deployments)
    # ------------------------------------------------------------------

    @staticmethod
    def _docker_compose_noop(component: str) -> ProvisionResult:
        """For docker_compose platform, components are defined in the compose file.
        No external provisioning needed — return success with no endpoint override."""
        return ProvisionResult(
            component=component, success=True,
            message="Docker Compose: managed by docker-compose.yml"
        )

    # ------------------------------------------------------------------
    # AWS provisioning stubs
    # ------------------------------------------------------------------

    def _aws_provision_rds(self) -> ProvisionResult:
        """Provision an AWS RDS MariaDB instance."""
        db_name = self.profile.get("db_name", "crm_db")
        db_user = self.profile.get("db_user", "crm_user")
        region = self.region or "us-east-1"
        instance_id = f"crm-{self.profile.get('meta', {}).get('profile_name', 'prod')}-db"

        cmd = [
            "aws", "rds", "create-db-instance",
            "--db-instance-identifier", instance_id,
            "--db-instance-class", "db.t3.medium",
            "--engine", "mariadb",
            "--engine-version", "10.11",
            "--allocated-storage", "20",
            "--db-name", db_name,
            "--master-username", db_user,
            "--master-user-password", self.profile.get("db_password", ""),
            "--region", region,
            "--no-publicly-accessible",
            "--output", "json",
        ]
        return self._run_cloud_cmd("mariadb", cmd, "Endpoint.Address",
                                   port_key="Endpoint.Port")

    def _aws_provision_elasticache(self) -> ProvisionResult:
        """Provision an AWS ElastiCache Redis cluster."""
        region = self.region or "us-east-1"
        cluster_id = f"crm-{self.profile.get('meta', {}).get('profile_name', 'prod')}-redis"
        cmd = [
            "aws", "elasticache", "create-cache-cluster",
            "--cache-cluster-id", cluster_id,
            "--engine", "redis",
            "--cache-node-type", "cache.t3.micro",
            "--num-cache-nodes", "1",
            "--region", region,
            "--output", "json",
        ]
        return self._run_cloud_cmd("redis", cmd)

    def _aws_provision_container(self, component: str, endpoint_key: str,
                                 default_port: int) -> ProvisionResult:
        """Provision a provider as an ECS Fargate service (stub)."""
        if self.dry_run:
            return ProvisionResult(
                component=component, success=True,
                message=f"[DRY RUN] Would provision ECS service for {component}",
                endpoint=f"http://{component}.internal:{default_port}",
            )
        # TODO: Implement full ECS Fargate provisioning
        return ProvisionResult(
            component=component, success=True,
            message=f"AWS ECS provisioning for {component} — not yet implemented, using Docker container",
        )

    # ------------------------------------------------------------------
    # Azure provisioning stubs
    # ------------------------------------------------------------------

    def _azure_provision_mysql(self) -> ProvisionResult:
        """Provision Azure Database for MySQL Flexible Server."""
        rg = self.profile.get("azure_resource_group", "rg-crm-dev")
        server = f"mysql-crm-{self.profile.get('meta', {}).get('profile_name', 'prod')}"
        cmd = [
            "az", "mysql", "flexible-server", "create",
            "--resource-group", rg,
            "--name", server,
            "--admin-user", self.profile.get("db_user", "crm_user"),
            "--admin-password", self.profile.get("db_password", ""),
            "--sku-name", "Standard_B1ms",
            "--version", "8.0.21",
            "--output", "json",
        ]
        return self._run_cloud_cmd("mariadb", cmd, "fullyQualifiedDomainName")

    def _azure_provision_redis(self) -> ProvisionResult:
        """Provision Azure Cache for Redis."""
        rg = self.profile.get("azure_resource_group", "rg-crm-dev")
        name = f"redis-crm-{self.profile.get('meta', {}).get('profile_name', 'prod')}"
        cmd = [
            "az", "redis", "create",
            "--resource-group", rg,
            "--name", name,
            "--sku", "Basic",
            "--vm-size", "c0",
            "--output", "json",
        ]
        return self._run_cloud_cmd("redis", cmd, "hostName")

    def _azure_provision_container(self, component: str, endpoint_key: str,
                                   default_port: int) -> ProvisionResult:
        """Provision a provider as an Azure Container Instance (stub)."""
        if self.dry_run:
            return ProvisionResult(
                component=component, success=True,
                message=f"[DRY RUN] Would provision ACI for {component}",
                endpoint=f"http://{component}.azurecontainer.io:{default_port}",
            )
        # TODO: Implement full ACI or ACA provisioning
        return ProvisionResult(
            component=component, success=True,
            message=f"Azure container provisioning for {component} — not yet implemented",
        )

    # ------------------------------------------------------------------
    # GCP provisioning stubs
    # ------------------------------------------------------------------

    def _gcp_provision_cloudsql(self) -> ProvisionResult:
        """Provision a Cloud SQL for MySQL instance."""
        project = self.profile.get("gcp_project_id", "")
        instance_name = f"crm-{self.profile.get('meta', {}).get('profile_name', 'prod')}-db"
        cmd = [
            "gcloud", "sql", "instances", "create", instance_name,
            "--database-version=MYSQL_8_0",
            "--tier=db-f1-micro",
            "--region", self.region or "us-central1",
            "--project", project,
            "--format=json",
        ]
        return self._run_cloud_cmd("mariadb", cmd, "ipAddresses[0].ipAddress")

    def _gcp_provision_memorystore(self) -> ProvisionResult:
        """Provision a Memorystore for Redis instance."""
        project = self.profile.get("gcp_project_id", "")
        name = f"crm-{self.profile.get('meta', {}).get('profile_name', 'prod')}-redis"
        cmd = [
            "gcloud", "redis", "instances", "create", name,
            "--size=1",
            "--region", self.region or "us-central1",
            "--project", project,
            "--format=json",
        ]
        return self._run_cloud_cmd("redis", cmd, "host")

    def _gcp_provision_cloudrun(self, component: str, endpoint_key: str,
                                default_port: int) -> ProvisionResult:
        """Provision a provider as a Cloud Run service (stub)."""
        if self.dry_run:
            return ProvisionResult(
                component=component, success=True,
                message=f"[DRY RUN] Would provision Cloud Run for {component}",
                endpoint=f"https://{component}-xyz.run.app",
            )
        # TODO: Implement full Cloud Run provisioning
        return ProvisionResult(
            component=component, success=True,
            message=f"GCP Cloud Run provisioning for {component} — not yet implemented",
        )

    # ------------------------------------------------------------------
    # Helpers
    # ------------------------------------------------------------------

    def _run_cloud_cmd(self, component: str, cmd: list[str],
                       endpoint_json_path: str = "",
                       port_key: str = "") -> ProvisionResult:
        """Execute a cloud CLI command and optionally extract an endpoint from JSON output."""
        if self.dry_run:
            return ProvisionResult(
                component=component, success=True,
                message=f"[DRY RUN] Would execute: {' '.join(cmd)}",
            )

        try:
            proc = subprocess.run(
                cmd, capture_output=True, text=True, timeout=600
            )
        except FileNotFoundError:
            return ProvisionResult(
                component=component, success=False,
                message=f"CLI tool '{cmd[0]}' not found. Install it first."
            )
        except subprocess.TimeoutExpired:
            return ProvisionResult(
                component=component, success=False,
                message=f"Command timed out after 600s: {' '.join(cmd[:3])}..."
            )

        if proc.returncode != 0:
            return ProvisionResult(
                component=component, success=False,
                message=f"Command failed (exit {proc.returncode}): {proc.stderr[:500]}",
            )

        endpoint = ""
        if endpoint_json_path and proc.stdout.strip():
            try:
                data = json.loads(proc.stdout)
                # Navigate dotted path like "Endpoint.Address"
                for part in endpoint_json_path.split("."):
                    if part.endswith("]"):
                        # Handle array index like "ipAddresses[0]"
                        key, idx = part.rstrip("]").split("[")
                        data = data[key][int(idx)]
                    else:
                        data = data[part]
                endpoint = str(data)
            except (json.JSONDecodeError, KeyError, IndexError, TypeError) as exc:
                logger.warning(f"Could not extract endpoint from {component} output: {exc}")

        return ProvisionResult(
            component=component, success=True,
            endpoint=endpoint,
            message=f"Provisioned successfully",
            details={"stdout": proc.stdout[:2000], "stderr": proc.stderr[:500]},
        )


# ---------------------------------------------------------------------------
# CLI entry point
# ---------------------------------------------------------------------------

def main() -> None:
    """CLI entry point for standalone provisioning."""
    import argparse
    parser = argparse.ArgumentParser(
        description="CRM CDT Cloud Provisioner — provision components and capture endpoints"
    )
    parser.add_argument("--profile", required=True, help="Path to deployment-config.json")
    parser.add_argument("--platform", default="docker_compose",
                        choices=CloudProvisioner.SUPPORTED_PLATFORMS)
    parser.add_argument("--region", default="", help="Cloud region")
    parser.add_argument("--dry-run", action="store_true", help="Simulate without executing")
    parser.add_argument("--output", default="", help="Write endpoint overrides JSON to file")
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(message)s")

    with open(args.profile) as f:
        profile = json.load(f)

    provisioner = CloudProvisioner(
        profile=profile,
        platform=args.platform,
        dry_run=args.dry_run,
        region=args.region,
    )

    result = provisioner.provision_all()

    print(f"\nProvisioning {'succeeded' if result.success else 'FAILED'}")
    print(f"Components: {len(result.component_results)}")
    for cr in result.component_results:
        status = "OK" if cr.success else "FAIL"
        ep = f" → {cr.endpoint}" if cr.endpoint else ""
        print(f"  [{status}] {cr.component}: {cr.message}{ep}")

    if result.endpoint_overrides:
        print(f"\nEndpoint overrides ({len(result.endpoint_overrides)}):")
        for k, v in result.endpoint_overrides.items():
            print(f"  {k}: {v}")

    if args.output:
        Path(args.output).write_text(
            json.dumps(result.endpoint_overrides, indent=2), encoding="utf-8"
        )
        print(f"\nEndpoint overrides written to: {args.output}")

    if result.errors:
        print(f"\nErrors ({len(result.errors)}):")
        for err in result.errors:
            print(f"  - {err}")


if __name__ == "__main__":
    main()
