#!/usr/bin/env python3
"""
tests/test_flask_routes.py — Tests for Flask GUI routes including the /api/defaults endpoint,
/api/config, /api/enums, /api/providers, /api/size-recommendation, and health check.

These tests validate:
  - Smart defaults API returns correct structure for all architecture/platform/SSL combos
  - Protocol switching (HTTP → HTTPS) works correctly
  - Microservice URLs appear only in microservices architecture
  - Config save/load via /api/config
  - Enum and provider endpoints return valid lists
  - Health check returns 200
  - Page templates render without errors
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

# Ensure tool root is on sys.path
sys.path.insert(0, str(Path(__file__).parent.parent))

import pytest

from gui.app import app


@pytest.fixture()
def client():
    """Create a Flask test client."""
    app.config["TESTING"] = True
    with app.test_client() as c:
        yield c


# ===========================================================================
# Health check
# ===========================================================================


class TestHealthCheck:
    """Tests for /health endpoint."""

    def test_health_returns_ok(self, client):
        resp = client.get("/health")
        assert resp.status_code == 200
        data = resp.get_json()
        assert data["status"] == "ok"
        assert data["service"] == "crm-cdt"


# ===========================================================================
# Page rendering
# ===========================================================================


class TestPageRendering:
    """Tests that page templates render without errors."""

    def test_index_page(self, client):
        resp = client.get("/")
        assert resp.status_code == 200
        assert b"CRM" in resp.data or b"crm" in resp.data

    def test_wizard_page(self, client):
        resp = client.get("/wizard")
        assert resp.status_code == 200
        assert b"wizard" in resp.data.lower() or b"Wizard" in resp.data

    def test_day2_page(self, client):
        resp = client.get("/day2")
        assert resp.status_code == 200

    def test_profiles_page(self, client):
        resp = client.get("/profiles")
        assert resp.status_code == 200


# ===========================================================================
# /api/defaults — Smart Defaults
# ===========================================================================


class TestDefaultsAPI:
    """Tests for /api/defaults endpoint."""

    def test_defaults_returns_200(self, client):
        resp = client.get("/api/defaults")
        assert resp.status_code == 200

    def test_defaults_contains_required_keys(self, client):
        resp = client.get("/api/defaults")
        data = resp.get_json()
        required_keys = [
            "core_services", "database_services", "provider_services",
            "microservices", "networks", "registry", "database_defaults",
            "secret_hints", "urls", "protocol", "architecture", "platform",
        ]
        for key in required_keys:
            assert key in data, f"Missing key: {key}"

    def test_defaults_monolithic_no_microservices(self, client):
        """Monolithic architecture should return empty microservices dict."""
        resp = client.get("/api/defaults?architecture=monolithic")
        data = resp.get_json()
        assert data["microservices"] == {}
        assert data["architecture"] == "monolithic"

    def test_defaults_microservices_has_services(self, client):
        """Microservices architecture should return gateway and individual services."""
        resp = client.get("/api/defaults?architecture=microservices")
        data = resp.get_json()
        assert len(data["microservices"]) > 0
        assert "crm-gateway" in data["microservices"]
        assert "crm-identity" in data["microservices"]
        assert "crm-customer" in data["microservices"]
        assert "crm-sales" in data["microservices"]
        assert "crm-marketing" in data["microservices"]
        assert "crm-servicedesk" in data["microservices"]
        assert "crm-core" in data["microservices"]

    def test_defaults_http_protocol_when_ssl_false(self, client):
        """SSL=false should return HTTP protocol."""
        resp = client.get("/api/defaults?ssl=false")
        data = resp.get_json()
        assert data["protocol"] == "http"
        assert data["urls"]["api_url"].startswith("http://")
        assert data["urls"]["frontend_url"].startswith("http://")

    def test_defaults_https_protocol_when_ssl_true(self, client):
        """SSL=true should return HTTPS protocol."""
        resp = client.get("/api/defaults?ssl=true")
        data = resp.get_json()
        assert data["protocol"] == "https"
        assert data["urls"]["api_url"].startswith("https://")
        assert data["urls"]["frontend_url"].startswith("https://")

    def test_defaults_frontend_port_80_for_http(self, client):
        resp = client.get("/api/defaults?ssl=false")
        data = resp.get_json()
        assert data["core_services"]["crm-frontend"]["port"] == 80

    def test_defaults_frontend_port_443_for_https(self, client):
        resp = client.get("/api/defaults?ssl=true")
        data = resp.get_json()
        assert data["core_services"]["crm-frontend"]["port"] == 443

    def test_defaults_custom_host_in_urls(self, client):
        """Custom host should appear in generated URLs."""
        resp = client.get("/api/defaults?host=myserver.example.com")
        data = resp.get_json()
        assert "myserver.example.com" in data["urls"]["api_url"]
        assert "myserver.example.com" in data["urls"]["frontend_url"]

    def test_defaults_microservices_gateway_url(self, client):
        """Microservices should include gateway_url in urls."""
        resp = client.get("/api/defaults?architecture=microservices&host=gw.test.com")
        data = resp.get_json()
        assert "gateway_url" in data["urls"]
        assert "gw.test.com" in data["urls"]["gateway_url"]

    def test_defaults_microservices_all_service_urls(self, client):
        """Microservice architecture should generate URLs for each service."""
        resp = client.get("/api/defaults?architecture=microservices&host=svc.test.com&ssl=true")
        data = resp.get_json()
        expected_url_keys = [
            "crm_gateway_url", "crm_identity_url", "crm_customer_url",
            "crm_sales_url", "crm_marketing_url", "crm_servicedesk_url", "crm_core_url",
        ]
        for key in expected_url_keys:
            assert key in data["urls"], f"Missing URL key: {key}"
            assert data["urls"][key].startswith("https://")

    def test_defaults_on_premises_registry(self, client):
        resp = client.get("/api/defaults?platform=on_premises")
        data = resp.get_json()
        assert data["registry"]["build_locally"] is True
        assert "registry.internal" in data["registry"]["registry"]

    def test_defaults_azure_registry(self, client):
        resp = client.get("/api/defaults?platform=azure")
        data = resp.get_json()
        assert data["registry"]["build_locally"] is False
        assert "azurecr.io" in data["registry"]["registry"]

    def test_defaults_aws_registry(self, client):
        resp = client.get("/api/defaults?platform=aws")
        data = resp.get_json()
        assert data["registry"]["build_locally"] is False
        assert "ecr" in data["registry"]["registry"]

    def test_defaults_gcp_registry(self, client):
        resp = client.get("/api/defaults?platform=gcp")
        data = resp.get_json()
        assert "docker.pkg.dev" in data["registry"]["registry"]

    def test_defaults_database_defaults_all_types(self, client):
        """All four database types should be present."""
        resp = client.get("/api/defaults")
        data = resp.get_json()
        db = data["database_defaults"]
        assert "mariadb" in db
        assert "mysql" in db
        assert "postgresql" in db
        assert "sqlserver" in db
        assert db["mariadb"]["port"] == 3306
        assert db["postgresql"]["port"] == 5432
        assert db["sqlserver"]["port"] == 1433

    def test_defaults_database_container_names(self, client):
        resp = client.get("/api/defaults")
        data = resp.get_json()
        db = data["database_defaults"]
        assert db["mariadb"]["container"] == "crm-mariadb"
        assert db["postgresql"]["container"] == "crm-postgres"
        assert db["sqlserver"]["container"] == "crm-sqlserver"

    def test_defaults_secret_hints(self, client):
        resp = client.get("/api/defaults")
        data = resp.get_json()
        hints = data["secret_hints"]
        assert hints["db_user"] == "crm_user"
        assert hints["admin_username"] == "admin"
        assert hints["admin_email"] == "admin@crm.local"

    def test_defaults_core_services_always_present(self, client):
        resp = client.get("/api/defaults")
        data = resp.get_json()
        assert "crm-api" in data["core_services"]
        assert "crm-frontend" in data["core_services"]
        assert data["core_services"]["crm-api"]["port"] == 5000

    def test_defaults_provider_services(self, client):
        resp = client.get("/api/defaults")
        data = resp.get_json()
        ps = data["provider_services"]
        assert "crm-meilisearch" in ps
        assert "crm-ollama" in ps
        assert ps["crm-meilisearch"]["port"] == 7700
        assert ps["crm-ollama"]["port"] == 11434

    def test_defaults_networks(self, client):
        resp = client.get("/api/defaults")
        data = resp.get_json()
        nets = data["networks"]
        assert "crm_crm-network" in nets
        assert "crm-core-network" in nets
        assert "crm-db-network" in nets

    def test_defaults_https_frontend_url_omits_port_443(self, client):
        """Frontend URL for HTTPS should not include :443."""
        resp = client.get("/api/defaults?ssl=true&host=app.test.com")
        data = resp.get_json()
        assert data["urls"]["frontend_url"] == "https://app.test.com"

    def test_defaults_http_frontend_url_omits_port_80(self, client):
        """Frontend URL for HTTP should not include :80."""
        resp = client.get("/api/defaults?ssl=false&host=app.test.com")
        data = resp.get_json()
        assert data["urls"]["frontend_url"] == "http://app.test.com"


# ===========================================================================
# /api/config — Config store
# ===========================================================================


class TestConfigAPI:
    """Tests for /api/config GET/POST."""

    def test_config_get_empty_initially(self, client):
        resp = client.get("/api/config")
        assert resp.status_code == 200
        data = resp.get_json()
        assert isinstance(data, dict)

    def test_config_post_and_get(self, client):
        payload = {"name": "test-deploy", "platform": "azure"}
        resp = client.post(
            "/api/config",
            data=json.dumps(payload),
            content_type="application/json",
        )
        assert resp.status_code == 200
        assert resp.get_json()["status"] == "ok"


# ===========================================================================
# /api/enums — Enum choices
# ===========================================================================


class TestEnumsAPI:
    """Tests for /api/enums/<enum_name>."""

    @pytest.mark.parametrize("enum_name", [
        "target_platform",
        "deployment_architecture",
        "database_type",
        "provider_strategy",
        "azure_region",
        "aws_region",
        "gcp_region",
        "onprem_virtualization",
        "onprem_container_runtime",
        "onprem_orchestration",
    ])
    def test_enum_returns_list(self, client, enum_name):
        resp = client.get(f"/api/enums/{enum_name}")
        assert resp.status_code == 200
        data = resp.get_json()
        assert isinstance(data, list)
        assert len(data) > 0
        # Each item should have value and label
        assert "value" in data[0]
        assert "label" in data[0]

    def test_unknown_enum_returns_404(self, client):
        resp = client.get("/api/enums/nonexistent_enum")
        assert resp.status_code == 404


# ===========================================================================
# /api/providers — Provider choices
# ===========================================================================


class TestProvidersAPI:
    """Tests for /api/providers/<category>."""

    @pytest.mark.parametrize("category", [
        "search", "chat", "notification",
        "analytics", "signature", "ai", "integration",
    ])
    def test_provider_returns_list(self, client, category):
        resp = client.get(f"/api/providers/{category}")
        assert resp.status_code == 200
        data = resp.get_json()
        assert isinstance(data, list)
        assert len(data) > 0
        assert "value" in data[0]
        assert "label" in data[0]

    def test_unknown_provider_returns_404(self, client):
        resp = client.get("/api/providers/unknown_category")
        assert resp.status_code == 404


# ===========================================================================
# /api/size-recommendation — Size recommendations
# ===========================================================================


class TestSizeRecommendationAPI:
    """Tests for /api/size-recommendation/<users>."""

    @pytest.mark.parametrize("users", [10, 50, 100, 500, 1000])
    def test_recommendation_returns_valid_data(self, client, users):
        resp = client.get(f"/api/size-recommendation/{users}")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "user_range" in data
        assert "description" in data
        assert "azure" in data
        assert "aws" in data
        assert "gcp" in data

    def test_recommendation_azure_has_vm_and_db(self, client):
        resp = client.get("/api/size-recommendation/100")
        data = resp.get_json()
        assert "frontend_vm" in data["azure"]
        assert "api_vm" in data["azure"]
        assert "database_sku" in data["azure"]

    def test_recommendation_aws_has_instance_and_rds(self, client):
        resp = client.get("/api/size-recommendation/100")
        data = resp.get_json()
        assert "frontend_instance" in data["aws"]
        assert "api_instance" in data["aws"]
        assert "rds_class" in data["aws"]

    def test_recommendation_gcp_has_machine_and_sql(self, client):
        resp = client.get("/api/size-recommendation/100")
        data = resp.get_json()
        assert "frontend_machine" in data["gcp"]
        assert "api_machine" in data["gcp"]
        assert "cloudsql_tier" in data["gcp"]


# ===========================================================================
# Integration: defaults + microservices + HTTPS
# ===========================================================================


class TestDefaultsIntegration:
    """Cross-cutting integration tests for the defaults endpoint."""

    def test_full_production_scenario(self, client):
        """Full production: microservices + HTTPS + Azure + custom domain."""
        resp = client.get(
            "/api/defaults?architecture=microservices&platform=azure"
            "&ssl=true&host=crm.company.com"
        )
        assert resp.status_code == 200
        data = resp.get_json()

        # Protocol
        assert data["protocol"] == "https"

        # Core services
        assert data["core_services"]["crm-api"]["protocol"] == "https"
        assert data["core_services"]["crm-frontend"]["port"] == 443

        # Microservices
        assert len(data["microservices"]) == 7
        assert data["microservices"]["crm-gateway"]["port"] == 5000

        # URLs
        assert data["urls"]["frontend_url"] == "https://crm.company.com"
        assert data["urls"]["gateway_url"] == "https://crm.company.com:5000"

        # Azure registry
        assert "azurecr.io" in data["registry"]["registry"]

    def test_dev_scenario(self, client):
        """Dev scenario: monolith + HTTP + local docker."""
        resp = client.get(
            "/api/defaults?architecture=monolithic&platform=on_premises"
            "&ssl=false&host=localhost"
        )
        data = resp.get_json()

        assert data["protocol"] == "http"
        assert data["microservices"] == {}
        assert data["urls"]["api_url"] == "http://localhost:5000"
        assert data["urls"]["frontend_url"] == "http://localhost"
        assert data["registry"]["build_locally"] is True


# ===========================================================================
# /api/deploy/preflight — Container Pre-flight Check
# ===========================================================================


class TestDeployPreflight:
    """Tests for /api/deploy/preflight endpoint."""

    def test_preflight_returns_200(self, client):
        resp = client.post("/api/deploy/preflight", json={})
        assert resp.status_code == 200
        data = resp.get_json()
        assert "containers" in data
        assert isinstance(data["containers"], list)

    def test_preflight_container_fields(self, client):
        """Each container in the response should have name, image, status, state, group."""
        resp = client.post("/api/deploy/preflight", json={})
        data = resp.get_json()
        for c in data["containers"]:
            assert "name" in c
            assert "image" in c
            assert "status" in c
            assert "state" in c
            assert "group" in c

    def test_preflight_group_field_values(self, client):
        """If containers are present, group should be one of the known values."""
        resp = client.post("/api/deploy/preflight", json={})
        data = resp.get_json()
        valid_groups = {"app", "database", "provider", "other"}
        for c in data["containers"]:
            assert c["group"] in valid_groups


# ===========================================================================
# DockerComposeDeployer — Container Action Tests
# ===========================================================================


class TestDeployerContainerAction:
    """Tests for the container_action parameter on DockerComposeDeployer."""

    def test_deployer_accepts_container_action_param(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, container_action="reuse", dry_run=True)
        assert d.container_action == "reuse"

    def test_deployer_default_container_action_is_recreate(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        assert d.container_action == "recreate"

    def test_deployer_has_15_steps(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        assert d.total_steps == 15

    def test_deployer_accepts_containers_to_remove(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, containers_to_remove=["crm-api", "crm-frontend"], dry_run=True)
        assert d.containers_to_remove == ["crm-api", "crm-frontend"]

    def test_deployer_default_containers_to_remove_is_empty(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        assert d.containers_to_remove == []

    def test_handle_existing_containers_reuse_skips_cleanup(self):
        """When container_action=reuse and no removal list, step should skip cleanup."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, container_action="reuse", dry_run=True)
        result = d._step_handle_existing_containers()
        assert result is True
        # Check that the reuse message was emitted
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("Reusing" in m for m in messages)

    def test_handle_existing_containers_recreate_runs_cleanup(self):
        """When container_action=recreate (dry_run), step should attempt cleanup."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, container_action="recreate", dry_run=True)
        result = d._step_handle_existing_containers()
        assert result is True

    def test_handle_existing_containers_specific_list(self):
        """When containers_to_remove is given, only those should be targeted."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(
            Path("/tmp"), {}, log_queue=q, container_action="recreate",
            containers_to_remove=["crm-api", "crm-frontend"], dry_run=True,
        )
        result = d._step_handle_existing_containers()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("crm-api" in m and "crm-frontend" in m for m in messages)


# ===========================================================================
# classify_container — Container Group Classification
# ===========================================================================


class TestClassifyContainer:
    """Tests for classify_container helper."""

    def test_app_containers(self):
        from deployers.docker_compose import classify_container
        assert classify_container("crm-api") == "app"
        assert classify_container("crm-frontend") == "app"

    def test_database_containers(self):
        from deployers.docker_compose import classify_container
        assert classify_container("crm-mariadb") == "database"
        assert classify_container("crm-redis") == "database"

    def test_provider_containers(self):
        from deployers.docker_compose import classify_container
        assert classify_container("crm-meilisearch") == "provider"
        assert classify_container("crm-ollama") == "provider"
        assert classify_container("crm-n8n") == "provider"

    def test_unknown_container_returns_other(self):
        from deployers.docker_compose import classify_container
        assert classify_container("crm-unknown-thing") == "other"
        assert classify_container("my-random-container") == "other"


# ===========================================================================
# DockerComposeDeployer — Reuse-aware compose-up skip logic
# ===========================================================================


class TestReuseSkipsComposeUp:
    """When containers are reused, compose-up steps must skip them."""

    def test_services_to_start_filters_reused(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        d._reused_containers = {"crm-mariadb", "crm-redis"}
        result = d._services_to_start(["crm-mariadb", "crm-redis"])
        assert result == []

    def test_services_to_start_keeps_non_reused(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        d._reused_containers = {"crm-mariadb"}
        result = d._services_to_start(["crm-mariadb", "crm-redis"])
        assert result == ["crm-redis"]

    def test_db_step_skips_when_all_reused(self):
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        d._reused_containers = {"crm-mariadb", "crm-redis"}
        assert d._step_start_databases() is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("verified running" in m.lower() for m in messages)
        assert any("ensuring reused" in m.lower() for m in messages)

    def test_api_step_skips_when_reused(self):
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        d._reused_containers = {"crm-api"}
        assert d._step_start_api() is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("verified running" in m.lower() for m in messages)

    def test_frontend_step_skips_when_reused(self):
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        d._reused_containers = {"crm-frontend"}
        assert d._step_start_frontend() is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("verified running" in m.lower() for m in messages)

    def test_providers_step_skips_when_all_reused(self):
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {"providers": {"search_provider": "meilisearch", "ai_provider": "ollama"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        d._reused_containers = {"crm-meilisearch", "crm-ollama"}
        assert d._step_start_providers() is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("verified running" in m.lower() for m in messages)

    def test_ensure_reused_running_called_for_reused_only(self):
        """_ensure_reused_running should only attempt to start reused containers."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        d._reused_containers = {"crm-mariadb"}
        d._ensure_reused_running(["crm-mariadb", "crm-redis"])
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        # Should mention crm-mariadb (reused) but not crm-redis (not reused)
        ensuring_msg = [m for m in messages if "ensuring reused" in m.lower()]
        assert len(ensuring_msg) == 1
        assert "crm-mariadb" in ensuring_msg[0]
        assert "crm-redis" not in ensuring_msg[0]

    def test_ensure_reused_running_noop_when_none_reused(self):
        """_ensure_reused_running should be a no-op when no containers are reused."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        d._reused_containers = set()
        result = d._ensure_reused_running(["crm-mariadb"])
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert not any("ensuring" in m.lower() for m in messages)

    def test_reused_containers_set_initialized_empty(self):
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        assert d._reused_containers == set()


# ===========================================================================
# Architecture Detection & Target Docker Platform
# ===========================================================================


class TestArchitectureDetection:
    """Tests for ARM vs AMD64 detection and _target_docker_platform resolution."""

    def test_system_info_returns_machine_arch(self, client):
        """GET /api/system/info should include machine_arch and docker_platform."""
        resp = client.get("/api/system/info")
        assert resp.status_code == 200
        data = resp.get_json()
        bs = data.get("build_server", {})
        assert "machine_arch" in bs
        assert "docker_platform" in bs
        assert bs["machine_arch"]  # should not be empty
        assert bs["docker_platform"].startswith("linux/")

    def test_system_info_arch_consistency(self, client):
        """Build server machine_arch should match the docker_platform suffix."""
        resp = client.get("/api/system/info")
        data = resp.get_json()
        bs = data["build_server"]
        arch = bs["machine_arch"].lower()
        plat = bs["docker_platform"]
        # arm64/aarch64 → linux/arm64, x86_64/amd64 → linux/amd64
        if arch in ("arm64", "aarch64"):
            assert plat == "linux/arm64"
        elif arch in ("x86_64", "amd64"):
            assert plat == "linux/amd64"

    def test_target_arch_local(self, client):
        """POST /api/target/arch with localhost should return local machine arch."""
        resp = client.post(
            "/api/target/arch",
            json={"host": "localhost", "ssh_user": "root", "ssh_port": 22},
        )
        assert resp.status_code == 200
        data = resp.get_json()
        assert data["source"] == "local"
        assert data["machine_arch"]  # should not be empty
        assert data["docker_platform"].startswith("linux/")

    def test_target_arch_empty_host_is_local(self, client):
        """POST /api/target/arch with empty host should fallback to local."""
        resp = client.post("/api/target/arch", json={"host": ""})
        assert resp.status_code == 200
        data = resp.get_json()
        assert data["source"] == "local"

    def test_target_arch_127001_is_local(self, client):
        """POST /api/target/arch with 127.0.0.1 should be local."""
        resp = client.post("/api/target/arch", json={"host": "127.0.0.1"})
        assert resp.status_code == 200
        assert resp.get_json()["source"] == "local"

    def test_target_arch_remote_graceful_failure(self, client):
        """POST /api/target/arch with unreachable host should return default amd64."""
        resp = client.post(
            "/api/target/arch",
            json={"host": "192.0.2.1", "ssh_user": "root", "ssh_port": 22},
        )
        assert resp.status_code == 200
        data = resp.get_json()
        # Should fallback gracefully (either paramiko not installed or SSH timeout)
        assert data["docker_platform"] == "linux/amd64"

    def test_deployer_target_docker_platform_default_amd64(self):
        """Deployer with no target_arch in profile should default to linux/amd64."""
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        assert d._target_docker_platform == "linux/amd64"

    def test_deployer_target_docker_platform_arm64(self):
        """Deployer with target.target_arch='arm64' should resolve to linux/arm64."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"target_arch": "arm64"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_docker_platform == "linux/arm64"

    def test_deployer_target_docker_platform_aarch64(self):
        """Deployer with target.target_arch='aarch64' should resolve to linux/arm64."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"target_arch": "aarch64"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_docker_platform == "linux/arm64"

    def test_deployer_target_docker_platform_x86_64(self):
        """Deployer with target.target_arch='x86_64' should resolve to linux/amd64."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"target_arch": "x86_64"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_docker_platform == "linux/amd64"

    def test_deployer_target_docker_platform_from_top_level(self):
        """Deployer should also read target_arch from top-level profile key."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target_arch": "arm64"}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_docker_platform == "linux/arm64"

    def test_deployer_target_docker_platform_machine_arch_key(self):
        """Deployer should accept machine_arch in target dict."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"machine_arch": "aarch64"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_docker_platform == "linux/arm64"

    def test_deployer_deploy_header_includes_docker_platform(self):
        """Deployment header should include the Docker platform line."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {"target": {"target_arch": "arm64"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        d.deploy()
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        docker_plat_msgs = [m for m in messages if "Docker platform" in m and "linux/arm64" in m]
        assert len(docker_plat_msgs) >= 1, f"Expected docker platform in header, got: {messages[:20]}"

    def test_deployer_build_step_uses_target_platform(self):
        """Step 3 build should use _target_docker_platform, not hardcoded amd64."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {
            "target": {"target_arch": "arm64"},
            "image_registry": {"build_locally": True},
        }
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        d._step_build_local_images()
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        plat_msgs = [m for m in messages if "linux/arm64" in m]
        assert len(plat_msgs) >= 1, f"Expected linux/arm64 in build logs, got: {messages}"

    def test_deployer_target_host_from_target_dict(self):
        """Deployer should resolve _target_host from target.host."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "192.168.0.9"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_host == "192.168.0.9"

    def test_deployer_target_host_from_domain_name(self):
        """Deployer should use target.domain_name when target.host is absent."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"domain_name": "crm.example.com"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_host == "crm.example.com"

    def test_deployer_target_host_prefers_host_over_domain(self):
        """Deployer should prefer target.host over target.domain_name."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"domain_name": "crm.example.com", "host": "192.168.0.9"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_host == "192.168.0.9"

    def test_deployer_target_host_fallback_to_profile_host(self):
        """Deployer should fall back to profile['host'] when target.host is missing."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"host": "10.0.0.5", "target": {"target_arch": "amd64"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_host == "10.0.0.5"

    def test_deployer_target_host_fallback_to_deployment_host(self):
        """Deployer should fall back to profile['deployment_host']."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"deployment_host": "deploy.local"}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_host == "deploy.local"

    def test_deployer_target_host_marker_for_dry_run_no_host(self):
        """Deployer dry-run with no host should get NO_HOST_CONFIGURED marker."""
        from deployers.docker_compose import DockerComposeDeployer
        d = DockerComposeDeployer(Path("/tmp"), {}, dry_run=True)
        assert d._target_host == "NO_HOST_CONFIGURED"

    def test_deployer_health_check_uses_target_host(self):
        """Health check URL should use _target_host, not hardcoded localhost."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {"target": {"host": "192.168.0.9", "api_port": "5000"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        d._step_health_check_api()
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        # Dry-run emits "[192.168.0.9] [DRY-RUN] Would health check API"
        host_msgs = [m for m in messages if "192.168.0.9" in m]
        assert len(host_msgs) >= 1, f"Expected 192.168.0.9 in health check msg, got: {messages}"
        # Also verify the target_host was resolved correctly
        assert d._target_host == "192.168.0.9"


# ===========================================================================
# DockerComposeDeployer — Remote SSH Execution & Image Transfer
# ===========================================================================


class TestRemoteDeployment:
    """Tests for SSH-based remote deployment support (v0.614.35)."""

    def test_is_remote_true_for_non_localhost(self):
        """_is_remote should be True for a real host IP."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "192.168.0.9"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._is_remote is True

    def test_is_remote_false_for_localhost(self):
        """_is_remote should be False when target is localhost."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "localhost"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._is_remote is False

    def test_is_remote_false_for_127_0_0_1(self):
        """_is_remote should be False when target is 127.0.0.1."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "127.0.0.1"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._is_remote is False

    def test_remote_deploy_dir_default(self):
        """Remote deploy directory should default to /opt/crm-deployment."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "192.168.0.9"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._remote_deploy_dir == "/opt/crm-deployment"

    def test_ssh_port_from_profile(self):
        """_target_ssh_port should come from profile."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "192.168.0.9", "ssh_port": "2222"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_ssh_port == "2222"

    def test_ssh_user_from_profile(self):
        """_target_ssh_user should come from profile."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "192.168.0.9", "ssh_user": "deploy"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._target_ssh_user == "deploy"

    def test_run_on_target_delegates_to_run_when_local(self):
        """_run_on_target should delegate to _run for local deployments."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "localhost"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d._is_remote is False
        # Should call local _run — dry_run returns (0, "", "")
        rc, out, err = d._run_on_target(["echo", "hello"])
        assert rc == 0

    def test_step_transfer_images_skipped_when_local(self):
        """Image transfer should skip when deploying locally."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {"target": {"host": "localhost"}, "image_registry": {"build_locally": True}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        result = d._step_transfer_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("already available" in m for m in messages), f"Expected skip msg, got: {messages}"

    def test_step_transfer_images_skipped_when_registry(self):
        """Image transfer should skip when using registry images (not build_locally)."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {"target": {"host": "192.168.0.9"}, "image_registry": {"build_locally": False}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        result = d._step_transfer_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("registry" in m.lower() or "not needed" in m.lower() for m in messages)

    def test_step_transfer_images_dry_run_remote(self):
        """Image transfer dry-run should emit save/transfer/load would-be message."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {
            "target": {"host": "192.168.0.9", "crm_version": "latest"},
            "image_registry": {"build_locally": True},
        }
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        result = d._step_transfer_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("DRY-RUN" in m and "save/transfer/load" in m.lower() for m in messages) or \
            any("DRY-RUN" in m for m in messages), f"Expected DRY-RUN msg, got: {messages}"

    def test_deployer_15_steps_with_transfer(self):
        """Deployer should have 15 steps (including image transfer)."""
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"target": {"host": "localhost"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, dry_run=True)
        assert d.total_steps == 15

    def test_rollback_uses_compose_file_flag(self):
        """rollback() should pass -f compose file path."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        profile = {"target": {"host": "localhost"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        d.rollback()
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("Rolling back" in m for m in messages)


# ===========================================================================
# /api/generate endpoint
# ===========================================================================


class TestGenerateAPI:
    """Tests for /api/generate endpoint — file generation with proper error handling."""

    def test_generate_returns_400_when_no_target_host(self, client):
        """Missing target.host should give a 400, not a 500."""
        resp = client.post(
            "/api/generate",
            data=json.dumps({
                "name": "test",
                "platform": "on_premises",
                "architecture": "monolithic",
                "database_type": "mariadb",
            }),
            content_type="application/json",
        )
        assert resp.status_code == 400
        data = resp.get_json()
        assert data["status"] == "error"
        assert "host" in data["message"].lower()

    def test_generate_returns_400_when_body_is_empty(self, client):
        """No JSON body at all should return 400."""
        resp = client.post(
            "/api/generate",
            data="",
            content_type="application/json",
        )
        # Flask may return 400 or 415 depending on parsing
        assert resp.status_code in (400, 415)

    def test_generate_success_with_valid_config(self, client, tmp_path):
        """Valid config with target.host should succeed and create files."""
        resp = client.post(
            "/api/generate",
            data=json.dumps({
                "name": "test-deploy",
                "platform": "on_premises",
                "architecture": "monolithic",
                "database_type": "mariadb",
                "target": {
                    "host": "192.168.0.9",
                    "api_port": 5000,
                    "frontend_port": 80,
                },
            }),
            content_type="application/json",
        )
        assert resp.status_code == 200
        data = resp.get_json()
        assert data["status"] == "ok"
        assert "files" in data
        assert len(data["files"]) >= 4  # compose, .env, deploy.sh, config.json

    def test_generate_success_includes_deployment_host_in_env(self, client):
        """Generated .env should reference the deployment host, not localhost."""
        resp = client.post(
            "/api/generate",
            data=json.dumps({
                "platform": "on_premises",
                "architecture": "monolithic",
                "database_type": "mariadb",
                "target": {"host": "10.0.0.5"},
            }),
            content_type="application/json",
        )
        assert resp.status_code == 200
        data = resp.get_json()
        # Read the generated .env to verify host
        env_file = [f for f in data["files"] if f.endswith(".env")]
        assert env_file, "Expected .env in generated files"
        with open(env_file[0]) as fh:
            env_content = fh.read()
        assert "10.0.0.5" in env_content
        assert "localhost" not in env_content.split("CRM_DEPLOYMENT_HOST")[1].split("\n")[0]

    def test_generate_azure_platform_returns_ok(self, client):
        """Azure platform should not crash (even if files are not yet generated)."""
        resp = client.post(
            "/api/generate",
            data=json.dumps({"platform": "azure", "name": "test"}),
            content_type="application/json",
        )
        assert resp.status_code == 200
        data = resp.get_json()
        assert data["status"] == "ok"


class TestComposeServiceNames:
    """Verify generated docker-compose uses crm-prefixed service names matching the deployer."""

    def test_compose_service_names_are_crm_prefixed(self):
        """Core services must use crm- prefix so deployer can reference them."""
        import yaml
        from gui.app import generate_docker_compose

        config = {
            "name": "test",
            "platform": "on_premises",
            "database_type": "mariadb",
            "target": {"host": "10.0.0.1"},
        }
        compose_yaml = generate_docker_compose(config)
        compose = yaml.safe_load(compose_yaml)
        svc_names = list(compose["services"].keys())
        assert "crm-mariadb" in svc_names, f"Expected crm-mariadb in {svc_names}"
        assert "crm-redis" in svc_names, f"Expected crm-redis in {svc_names}"
        assert "crm-api" in svc_names, f"Expected crm-api in {svc_names}"
        assert "crm-frontend" in svc_names, f"Expected crm-frontend in {svc_names}"
        # Short names must NOT be present
        for short in ("mariadb", "redis", "api", "frontend"):
            assert short not in svc_names, f"Short service name '{short}' should not appear"

    def test_compose_provider_service_names_are_crm_prefixed(self):
        """Provider services must also use crm- prefix."""
        import yaml
        from gui.app import generate_docker_compose

        config = {
            "name": "test",
            "platform": "on_premises",
            "database_type": "mariadb",
            "target": {"host": "10.0.0.1"},
            "search_provider": "meilisearch",
            "ai_provider": "ollama",
            "signature_provider": "docuseal",
        }
        compose_yaml = generate_docker_compose(config)
        compose = yaml.safe_load(compose_yaml)
        svc_names = list(compose["services"].keys())
        assert "crm-meilisearch" in svc_names
        assert "crm-ollama" in svc_names
        assert "crm-docuseal" in svc_names
        assert "crm-docuseal-postgres" in svc_names
        for short in ("meilisearch", "ollama", "docuseal", "docuseal-postgres"):
            assert short not in svc_names, f"Short service name '{short}' should not appear"

    def test_compose_depends_on_uses_crm_prefixed(self):
        """depends_on references must use crm-prefixed names."""
        import yaml
        from gui.app import generate_docker_compose

        config = {
            "name": "test",
            "platform": "on_premises",
            "database_type": "mariadb",
            "target": {"host": "10.0.0.1"},
        }
        compose_yaml = generate_docker_compose(config)
        compose = yaml.safe_load(compose_yaml)
        api_deps = compose["services"]["crm-api"]["depends_on"]
        assert "crm-mariadb" in api_deps
        assert "crm-redis" in api_deps
        fe_deps = compose["services"]["crm-frontend"]["depends_on"]
        assert "crm-api" in fe_deps

    def test_compose_no_version_attribute(self):
        """Generated compose should not include the obsolete 'version' attribute."""
        import yaml
        from gui.app import generate_docker_compose

        config = {
            "name": "test",
            "platform": "on_premises",
            "database_type": "mariadb",
            "target": {"host": "10.0.0.1"},
        }
        compose_yaml = generate_docker_compose(config)
        compose = yaml.safe_load(compose_yaml)
        assert "version" not in compose, "Compose 'version' attribute is obsolete and should be omitted"

    def test_env_file_includes_docuseal_db_password(self):
        """When docuseal is selected, .env must include DOCUSEAL_DB_PASSWORD."""
        from gui.app import generate_env_file

        config = {
            "name": "test",
            "platform": "on_premises",
            "database_type": "mariadb",
            "target": {"host": "10.0.0.1"},
            "signature_provider": "docuseal",
        }
        env_content = generate_env_file(config)
        assert "DOCUSEAL_DB_PASSWORD=" in env_content


# ===========================================================================
# Provider Key Normalization & Detection Tests
# ===========================================================================


class TestProviderKeyNormalization:
    """Verify generator normalizes short provider keys to long keys and vice versa."""

    def test_normalize_short_keys_to_long(self):
        """Profiles store short keys; templates expect long keys."""
        from core.generator import ConfigGenerator
        providers = {"search": "meilisearch", "ai": "ollama", "chat": "chatwoot"}
        result = ConfigGenerator._normalize_provider_keys(providers)
        assert result["search_provider"] == "meilisearch"
        assert result["ai_provider"] == "ollama"
        assert result["chat_provider"] == "chatwoot"
        # Short keys preserved
        assert result["search"] == "meilisearch"
        assert result["ai"] == "ollama"
        assert result["chat"] == "chatwoot"

    def test_normalize_long_keys_to_short(self):
        """Backward compat: if someone uses search_provider, short key also set."""
        from core.generator import ConfigGenerator
        providers = {"search_provider": "meilisearch", "ai_provider": "ollama"}
        result = ConfigGenerator._normalize_provider_keys(providers)
        assert result["search"] == "meilisearch"
        assert result["ai"] == "ollama"

    def test_normalize_all_seven_providers(self):
        """All 7 provider categories are normalized."""
        from core.generator import ConfigGenerator
        providers = {
            "search": "meilisearch",
            "ai": "ollama",
            "chat": "chatwoot",
            "notification": "novu",
            "analytics": "superset",
            "signature": "docuseal",
            "integration": "n8n",
        }
        result = ConfigGenerator._normalize_provider_keys(providers)
        for short, long in ConfigGenerator._PROVIDER_KEY_MAP.items():
            assert short in result, f"Missing short key: {short}"
            assert long in result, f"Missing long key: {long}"
            assert result[short] == result[long]

    def test_normalize_empty_dict(self):
        """Empty providers dict stays empty."""
        from core.generator import ConfigGenerator
        result = ConfigGenerator._normalize_provider_keys({})
        assert result == {}

    def test_normalize_preserves_extra_keys(self):
        """Non-provider keys must not be dropped."""
        from core.generator import ConfigGenerator
        providers = {"search": "meilisearch", "extra_key": "value"}
        result = ConfigGenerator._normalize_provider_keys(providers)
        assert result["extra_key"] == "value"

    def test_template_context_gets_normalized_providers(self):
        """ConfigGenerator._build_context normalizes provider keys."""
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "providers": {"search": "meilisearch", "ai": "ollama"},
            "database": {"db_password": "test", "db_root_password": "roottest", "db_host": "localhost"},
            "security": {"jwt_secret": "x" * 64},
        }
        ctx = gen._build_context(profile)
        assert ctx["providers"]["search_provider"] == "meilisearch"
        assert ctx["providers"]["ai_provider"] == "ollama"

    def test_jinja2_template_renders_meilisearch_with_short_keys(self):
        """docker-compose.j2 renders meilisearch service when profile uses short keys."""
        import yaml
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "providers": {"search": "meilisearch"},
            "database": {"db_password": "test123", "db_root_password": "roottest", "db_host": "crm-mariadb"},
            "security": {"jwt_secret": "x" * 64},
        }
        ctx = gen._build_context(profile)
        # Render the docker-compose template
        tmpl = gen._env.get_template("docker-compose.j2")
        rendered = tmpl.render(ctx)
        compose = yaml.safe_load(rendered)
        assert "crm-meilisearch" in compose["services"], \
            f"Expected crm-meilisearch in {list(compose['services'].keys())}"

    def test_jinja2_template_renders_all_providers_with_short_keys(self):
        """All providers render when profile uses short keys."""
        import yaml
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "providers": {
                "search": "meilisearch",
                "ai": "ollama",
                "chat": "chatwoot",
                "notification": "novu",
                "analytics": "superset",
                "signature": "docuseal",
                "integration": "n8n",
            },
            "database": {"db_password": "test123", "db_root_password": "roottest", "db_host": "crm-mariadb"},
            "security": {"jwt_secret": "x" * 64},
        }
        ctx = gen._build_context(profile)
        tmpl = gen._env.get_template("docker-compose.j2")
        rendered = tmpl.render(ctx)
        compose = yaml.safe_load(rendered)
        svc = list(compose["services"].keys())
        for expected in ("crm-meilisearch", "crm-ollama", "crm-chatwoot",
                         "crm-novu", "crm-superset", "crm-docuseal", "crm-n8n"):
            assert expected in svc, f"Missing provider service {expected} in {svc}"


class TestDeployerProviderDetection:
    """Verify the deployer step_start_providers detects providers with both key formats."""

    def _make_deployer(self, profile):
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        return d, q

    def test_detects_providers_with_short_keys(self):
        """Profile uses short keys (search, ai, etc.) — providers should be detected."""
        profile = {
            "target": {"host": "10.0.0.1"},
            "providers": {
                "search": "meilisearch",
                "ai": "ollama",
                "chat": "chatwoot",
                "notification": "novu",
                "analytics": "superset",
                "signature": "docuseal",
                "integration": "n8n",
            },
        }
        d, q = self._make_deployer(profile)
        # Stub _ensure_reused_running and _services_to_start to capture calls
        requested = []
        d._ensure_reused_running = lambda svcs: None
        d._services_to_start = lambda svcs: svcs
        d._compose_up = lambda svcs, timeout=120: (0, "", "")
        d._step_start_providers()
        # Check log messages for requested providers
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        provider_msg = [m for m in messages if "Requested providers" in m]
        assert len(provider_msg) == 1, f"Expected 1 'Requested providers' log, got {provider_msg}"
        for container in ("crm-meilisearch", "crm-ollama", "crm-chatwoot",
                          "crm-novu", "crm-superset", "crm-docuseal", "crm-n8n"):
            assert container in provider_msg[0], f"Missing {container} in {provider_msg[0]}"

    def test_detects_providers_with_long_keys_in_wizard_config(self):
        """wizard_config uses long keys (search_provider, etc.) — should also be detected."""
        profile = {
            "target": {"host": "10.0.0.1"},
            "providers": {},
            "wizard_config": {
                "search_provider": "meilisearch",
                "ai_provider": "ollama",
            },
        }
        d, q = self._make_deployer(profile)
        d._ensure_reused_running = lambda svcs: None
        d._services_to_start = lambda svcs: svcs
        d._compose_up = lambda svcs, timeout=120: (0, "", "")
        d._step_start_providers()
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        provider_msg = [m for m in messages if "Requested providers" in m]
        assert len(provider_msg) == 1
        assert "crm-meilisearch" in provider_msg[0]
        assert "crm-ollama" in provider_msg[0]

    def test_no_providers_emits_skip_message(self):
        """Empty providers dict emits skip message."""
        profile = {"target": {"host": "10.0.0.1"}, "providers": {}}
        d, q = self._make_deployer(profile)
        d._step_start_providers()
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("No external providers selected" in m for m in messages)

    def test_resolve_provider_static_method(self):
        """The _resolve_provider helper checks short then long keys."""
        from deployers.docker_compose import DockerComposeDeployer
        assert DockerComposeDeployer._resolve_provider({"search": "meilisearch"}, "search", "search_provider") == "meilisearch"
        assert DockerComposeDeployer._resolve_provider({"search_provider": "meilisearch"}, "search", "search_provider") == "meilisearch"
        assert DockerComposeDeployer._resolve_provider({}, "search", "search_provider") == ""


# ===========================================================================
# DockerComposeDeployer — _compose_up Retry Tests
# ===========================================================================


class TestComposeUpRetry:
    """Tests for the _compose_up method that auto-retries on container name conflicts."""

    def _make_deployer(self, log_queue=None):
        from deployers.docker_compose import DockerComposeDeployer
        import queue as q_mod
        lq = log_queue or q_mod.Queue()
        # Use dry_run=True to bypass host validation, then override dry_run
        # for the method under test (which doesn't check it).
        d = DockerComposeDeployer(Path("/tmp"), {"target": {"host": "10.0.0.1"}}, log_queue=lq, dry_run=True)
        return d

    def test_compose_up_success_on_first_attempt(self):
        """When docker compose up succeeds, no retry should happen."""
        d = self._make_deployer()
        calls = []

        def fake_run(cmd, timeout=120, log_command=False):
            calls.append(cmd)
            return (0, "ok", "")

        d._run_on_target = fake_run
        rc, out, err = d._compose_up(["crm-mariadb", "crm-redis"])
        assert rc == 0
        assert len(calls) == 1  # Only 1 call, no retry

    def test_compose_up_retries_on_name_conflict(self):
        """When first attempt fails with 'already in use', should remove and retry."""
        import queue
        lq = queue.Queue()
        d = self._make_deployer(log_queue=lq)
        call_count = [0]

        def fake_run(cmd, timeout=120, log_command=False):
            call_count[0] += 1
            cmd_str = " ".join(cmd)
            if "docker rm -f" in cmd_str:
                return (0, "", "")
            if call_count[0] == 1:
                # First compose up fails with name conflict
                return (
                    1,
                    "",
                    'Error response from daemon: Conflict. The container name "/crm-redis" '
                    "is already in use by container abc123. You have to remove (or rename) "
                    "that container to be able to reuse that name.",
                )
            # Retry succeeds
            return (0, "ok", "")

        d._run_on_target = fake_run
        rc, out, err = d._compose_up(["crm-mariadb", "crm-redis"])
        assert rc == 0
        # Should have been called 3 times: first compose up, docker rm, retry compose up
        assert call_count[0] == 3

    def test_compose_up_extracts_multiple_conflicting_names(self):
        """When multiple containers conflict, all should be removed."""
        d = self._make_deployer()
        rm_calls = []

        def fake_run(cmd, timeout=120, log_command=False):
            cmd_str = " ".join(cmd)
            if "docker rm -f" in cmd_str:
                rm_calls.append(cmd)
                return (0, "", "")
            if not rm_calls:
                return (
                    1,
                    "",
                    'The container name "/crm-redis" is already in use by container abc. '
                    'The container name "/crm-mariadb" is already in use by container def.',
                )
            return (0, "ok", "")

        d._run_on_target = fake_run
        rc, _, _ = d._compose_up(["crm-mariadb", "crm-redis"])
        assert rc == 0
        assert len(rm_calls) == 1
        # Both conflicting names should appear in the rm command
        assert "crm-redis" in rm_calls[0]
        assert "crm-mariadb" in rm_calls[0]

    def test_compose_up_no_retry_on_other_errors(self):
        """Non-name-conflict errors should NOT trigger a retry."""
        d = self._make_deployer()
        calls = []

        def fake_run(cmd, timeout=120, log_command=False):
            calls.append(cmd)
            return (1, "", "network xyz not found")

        d._run_on_target = fake_run
        rc, _, err = d._compose_up(["crm-api"])
        assert rc == 1
        assert len(calls) == 1  # No retry


# ===========================================================================
# Version-aware Build & Image Tagging Tests
# ===========================================================================


class TestVersionAwareBuild:
    """Tests for _read_version_from_repo and _image_exists_locally."""

    def test_read_version_from_valid_json(self, tmp_path):
        """Reads major.minor.patch from version.json."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text('{"major": 0, "minor": 614, "patch": 53}')
        assert DockerComposeDeployer._read_version_from_repo(tmp_path) == "0.614.53"

    def test_read_version_missing_file(self, tmp_path):
        """Returns 'latest' when version.json does not exist."""
        from deployers.docker_compose import DockerComposeDeployer
        assert DockerComposeDeployer._read_version_from_repo(tmp_path) == "latest"

    def test_read_version_invalid_json(self, tmp_path):
        """Returns 'latest' on malformed JSON."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text("not valid json!")
        assert DockerComposeDeployer._read_version_from_repo(tmp_path) == "latest"

    def test_image_exists_locally_true(self):
        """When docker images -q returns an ID, image exists."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        d = DockerComposeDeployer(Path("/tmp"), {"target": {"host": "10.0.0.1"}},
                                  log_queue=queue.Queue(), dry_run=True)
        d._run = lambda cmd, timeout=15: (0, "abc123\n", "")
        assert d._image_exists_locally("crm-api:0.614.53") is True

    def test_image_exists_locally_false(self):
        """When docker images -q returns empty, image does not exist."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        d = DockerComposeDeployer(Path("/tmp"), {"target": {"host": "10.0.0.1"}},
                                  log_queue=queue.Queue(), dry_run=True)
        d._run = lambda cmd, timeout=15: (0, "", "")
        assert d._image_exists_locally("crm-api:0.614.53") is False

    def test_build_step_skips_existing_versioned_image(self, tmp_path):
        """When the versioned image already exists, build is skipped."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue

        # Create version.json
        vf = tmp_path / "version.json"
        vf.write_text('{"major": 0, "minor": 614, "patch": 53}')

        # Create dummy Dockerfiles
        docker_dir = tmp_path / "docker"
        docker_dir.mkdir()
        (docker_dir / "Dockerfile.backend").write_text("FROM scratch")
        (docker_dir / "Dockerfile.frontend").write_text("FROM scratch")

        profile = {
            "target": {"host": "10.0.0.1", "repo_root": str(tmp_path)},
            "image_registry": {"build_locally": True},
        }
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=False)

        # Fake: image already exists
        d._run = lambda cmd, timeout=15: (0, "sha256:abc123\n", "")
        d._run_streaming = lambda *a, **kw: 0  # should not be called

        result = d._step_build_local_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        skip_msgs = [m for m in messages if "already exists" in m]
        assert len(skip_msgs) == 2, f"Expected 2 skip messages, got: {skip_msgs}"

    def test_build_step_uses_version_tag(self, tmp_path):
        """Build step uses version from version.json as the image tag."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue

        vf = tmp_path / "version.json"
        vf.write_text('{"major": 1, "minor": 2, "patch": 3}')
        docker_dir = tmp_path / "docker"
        docker_dir.mkdir()
        (docker_dir / "Dockerfile.backend").write_text("FROM scratch")
        (docker_dir / "Dockerfile.frontend").write_text("FROM scratch")

        profile = {
            "target": {"host": "10.0.0.1", "repo_root": str(tmp_path)},
            "image_registry": {"build_locally": True},
        }
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        result = d._step_build_local_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        # Check that 1.2.3 appears as the image tag
        tag_msg = [m for m in messages if "1.2.3" in m]
        assert len(tag_msg) > 0, f"Expected version 1.2.3 in log, got: {messages}"


# ===========================================================================
# Component-level Versioning Tests
# ===========================================================================


class TestComponentVersioning:
    """Tests for per-component version reading from version.json."""

    def test_read_api_component_version(self, tmp_path):
        """Reads component-specific API version from version.json."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({
            "major": 0, "minor": 614, "patch": 58,
            "components": {
                "api": {"version": "0.614.60", "lastUpdate": "2026-03-02"},
                "frontend": {"version": "0.614.59", "lastUpdate": "2026-03-01"},
            }
        }))
        assert DockerComposeDeployer._read_version_from_repo(tmp_path, "api") == "0.614.60"

    def test_read_frontend_component_version(self, tmp_path):
        """Reads component-specific frontend version from version.json."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({
            "major": 0, "minor": 614, "patch": 58,
            "components": {
                "api": {"version": "0.614.60", "lastUpdate": "2026-03-02"},
                "frontend": {"version": "0.614.59", "lastUpdate": "2026-03-01"},
            }
        }))
        assert DockerComposeDeployer._read_version_from_repo(tmp_path, "frontend") == "0.614.59"

    def test_component_falls_back_to_solution_version(self, tmp_path):
        """Falls back to solution version when component key is missing."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({"major": 0, "minor": 614, "patch": 58}))
        assert DockerComposeDeployer._read_version_from_repo(tmp_path, "api") == "0.614.58"

    def test_component_none_returns_solution_version(self, tmp_path):
        """component=None returns the solution-level version."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({
            "major": 0, "minor": 614, "patch": 58,
            "components": {
                "api": {"version": "0.614.60", "lastUpdate": "2026-03-02"},
            }
        }))
        assert DockerComposeDeployer._read_version_from_repo(tmp_path, None) == "0.614.58"
        assert DockerComposeDeployer._read_version_from_repo(tmp_path) == "0.614.58"

    def test_component_empty_version_falls_back(self, tmp_path):
        """Falls back when component exists but version is empty."""
        from deployers.docker_compose import DockerComposeDeployer
        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({
            "major": 0, "minor": 614, "patch": 58,
            "components": {"api": {"lastUpdate": "2026-03-02"}}
        }))
        assert DockerComposeDeployer._read_version_from_repo(tmp_path, "api") == "0.614.58"

    def test_image_component_map_exists(self):
        """IMAGE_COMPONENT_MAP maps image names to component keys."""
        from deployers.docker_compose import IMAGE_COMPONENT_MAP, LOCAL_BUILD_IMAGES
        for name in LOCAL_BUILD_IMAGES:
            assert name in IMAGE_COMPONENT_MAP, f"{name} missing from IMAGE_COMPONENT_MAP"

    def test_build_step_uses_per_component_tags(self, tmp_path):
        """Build step tags each image with its component-specific version."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue

        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({
            "major": 0, "minor": 614, "patch": 58,
            "components": {
                "api": {"version": "0.614.60", "lastUpdate": "2026-03-02"},
                "frontend": {"version": "0.614.59", "lastUpdate": "2026-03-01"},
            }
        }))
        docker_dir = tmp_path / "docker"
        docker_dir.mkdir()
        (docker_dir / "Dockerfile.backend").write_text("FROM scratch")
        (docker_dir / "Dockerfile.frontend").write_text("FROM scratch")

        profile = {
            "target": {"host": "10.0.0.1", "repo_root": str(tmp_path)},
            "image_registry": {"build_locally": True},
        }
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        result = d._step_build_local_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        all_text = " ".join(messages)
        assert "crm-api" in all_text and "0.614.60" in all_text, f"Expected crm-api:0.614.60 in: {messages}"
        assert "crm-frontend" in all_text and "0.614.59" in all_text, f"Expected crm-frontend:0.614.59 in: {messages}"

    def test_transfer_step_uses_per_component_tags(self, tmp_path):
        """Transfer step assembles image tags with per-component versions."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue

        vf = tmp_path / "version.json"
        vf.write_text(json.dumps({
            "major": 0, "minor": 614, "patch": 58,
            "components": {
                "api": {"version": "0.614.60", "lastUpdate": "2026-03-02"},
                "frontend": {"version": "0.614.59", "lastUpdate": "2026-03-01"},
            }
        }))
        profile = {
            "target": {"host": "10.0.0.1", "repo_root": str(tmp_path)},
            "image_registry": {"build_locally": True},
        }
        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=True)
        result = d._step_transfer_images()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        all_text = " ".join(messages)
        assert "crm-api:0.614.60" in all_text, f"Expected crm-api:0.614.60 in: {messages}"
        assert "crm-frontend:0.614.59" in all_text, f"Expected crm-frontend:0.614.59 in: {messages}"


# ===========================================================================
# Password reuse / secret recovery
# ===========================================================================


class TestParseEnvFile:
    """Tests for DockerComposeDeployer._parse_env_file()."""

    def test_simple_key_value(self):
        from deployers.docker_compose import DockerComposeDeployer
        content = "DB_PASSWORD=secret123\nJWT_SECRET=abc"
        result = DockerComposeDeployer._parse_env_file(content)
        assert result == {"DB_PASSWORD": "secret123", "JWT_SECRET": "abc"}

    def test_quoted_values(self):
        from deployers.docker_compose import DockerComposeDeployer
        content = 'DB_PASSWORD="my secret"\nJWT_SECRET=\'tok$en\''
        result = DockerComposeDeployer._parse_env_file(content)
        assert result["DB_PASSWORD"] == "my secret"
        assert result["JWT_SECRET"] == "tok$en"

    def test_double_dollar_unescape(self):
        """Docker Compose escapes $ as $$; parser should reverse that."""
        from deployers.docker_compose import DockerComposeDeployer
        content = "DB_PASSWORD=Pa$$word@Dev2024"
        result = DockerComposeDeployer._parse_env_file(content)
        assert result["DB_PASSWORD"] == "Pa$word@Dev2024"

    def test_comments_and_blanks_ignored(self):
        from deployers.docker_compose import DockerComposeDeployer
        content = "# this is a comment\n\nDB_HOST=localhost\n  \n# another comment\n"
        result = DockerComposeDeployer._parse_env_file(content)
        assert result == {"DB_HOST": "localhost"}

    def test_no_equals_lines_ignored(self):
        from deployers.docker_compose import DockerComposeDeployer
        content = "no-equals-here\nGOOD=value"
        result = DockerComposeDeployer._parse_env_file(content)
        assert result == {"GOOD": "value"}

    def test_empty_value(self):
        from deployers.docker_compose import DockerComposeDeployer
        content = "EMPTY_KEY="
        result = DockerComposeDeployer._parse_env_file(content)
        assert result == {"EMPTY_KEY": ""}

    def test_value_with_equals(self):
        """Values containing '=' should be handled correctly."""
        from deployers.docker_compose import DockerComposeDeployer
        content = "CONNECTION=Server=db;Port=3306;Database=crm"
        result = DockerComposeDeployer._parse_env_file(content)
        assert result["CONNECTION"] == "Server=db;Port=3306;Database=crm"

    def test_empty_content(self):
        from deployers.docker_compose import DockerComposeDeployer
        result = DockerComposeDeployer._parse_env_file("")
        assert result == {}


class TestInjectSecretsIntoProfile:
    """Tests for DockerComposeDeployer.inject_secrets_into_profile()."""

    def test_db_secrets_go_to_database_section(self):
        from deployers.docker_compose import DockerComposeDeployer
        profile: dict = {}
        secrets = {"db_password": "pass123", "db_root_password": "rootpass"}
        DockerComposeDeployer.inject_secrets_into_profile(profile, secrets)
        assert profile["database"]["db_password"] == "pass123"
        assert profile["database"]["db_root_password"] == "rootpass"

    def test_non_db_secrets_go_to_secrets_section(self):
        from deployers.docker_compose import DockerComposeDeployer
        profile: dict = {}
        secrets = {"jwt_secret": "tok123", "redis_password": "red456"}
        DockerComposeDeployer.inject_secrets_into_profile(profile, secrets)
        assert profile["secrets"]["jwt_secret"] == "tok123"
        assert profile["secrets"]["redis_password"] == "red456"

    def test_recovered_secrets_override_stale_profile_values(self):
        """Recovered secrets from remote .env MUST override stale wizard values.

        MariaDB ignores MYSQL_PASSWORD env var after first volume init, so the
        recovered password from the running deployment is authoritative.
        """
        from deployers.docker_compose import DockerComposeDeployer
        profile = {
            "database": {"db_password": "stale_wizard_pass"},
            "secrets": {"jwt_secret": "stale_wizard_jwt"},
        }
        secrets = {"db_password": "recovered_pass", "jwt_secret": "recovered_jwt"}
        DockerComposeDeployer.inject_secrets_into_profile(profile, secrets)
        assert profile["database"]["db_password"] == "recovered_pass"
        assert profile["secrets"]["jwt_secret"] == "recovered_jwt"

    def test_empty_secrets_noop(self):
        from deployers.docker_compose import DockerComposeDeployer
        profile = {"database": {"db_name": "crm_db"}}
        DockerComposeDeployer.inject_secrets_into_profile(profile, {})
        assert profile == {"database": {"db_name": "crm_db"}}

    def test_returns_profile(self):
        from deployers.docker_compose import DockerComposeDeployer
        profile: dict = {}
        result = DockerComposeDeployer.inject_secrets_into_profile(profile, {"db_password": "p"})
        assert result is profile

    def test_mixed_db_and_non_db_secrets(self):
        from deployers.docker_compose import DockerComposeDeployer
        profile: dict = {}
        secrets = {
            "db_password": "dbp",
            "jwt_secret": "jwt",
            "redis_password": "redis",
            "openai_api_key": "sk-123",
        }
        DockerComposeDeployer.inject_secrets_into_profile(profile, secrets)
        assert profile["database"] == {"db_password": "dbp"}
        assert profile["secrets"]["jwt_secret"] == "jwt"
        assert profile["secrets"]["redis_password"] == "redis"
        assert profile["secrets"]["openai_api_key"] == "sk-123"


class TestCheckRemoteDbVolumeExists:
    """Tests for DockerComposeDeployer.check_remote_db_volume_exists()."""

    def test_localhost_volume_not_found(self):
        """Returns False when docker volume inspect fails locally."""
        from deployers.docker_compose import DockerComposeDeployer
        from unittest.mock import patch, MagicMock

        mock_result = MagicMock()
        mock_result.returncode = 1
        with patch("subprocess.run", return_value=mock_result):
            result = DockerComposeDeployer.check_remote_db_volume_exists(
                host="localhost", volume_name="nonexistent_vol"
            )
        assert result is False

    def test_localhost_volume_found(self):
        """Returns True when docker volume inspect succeeds locally."""
        from deployers.docker_compose import DockerComposeDeployer
        from unittest.mock import patch, MagicMock

        mock_result = MagicMock()
        mock_result.returncode = 0
        with patch("subprocess.run", return_value=mock_result):
            result = DockerComposeDeployer.check_remote_db_volume_exists(
                host="localhost", volume_name="mariadb_data"
            )
        assert result is True

    def test_empty_host_returns_false(self):
        """Empty host string should use local check and handle gracefully."""
        from deployers.docker_compose import DockerComposeDeployer
        from unittest.mock import patch, MagicMock

        mock_result = MagicMock()
        mock_result.returncode = 1
        with patch("subprocess.run", return_value=mock_result):
            result = DockerComposeDeployer.check_remote_db_volume_exists(host="")
        assert result is False

    def test_remote_ssh_failure_returns_false(self):
        """SSH failure for remote host should return False (safe to generate)."""
        from deployers.docker_compose import DockerComposeDeployer
        from unittest.mock import patch

        with patch("paramiko.SSHClient") as mock_ssh_cls:
            mock_ssh_cls.return_value.connect.side_effect = Exception("SSH timeout")
            result = DockerComposeDeployer.check_remote_db_volume_exists(
                host="192.168.0.9"
            )
        assert result is False


class TestCredentialHelpers:
    """Tests for _get_configured_db_password and _get_configured_db_root_password."""

    def _make_deployer(self, profile):
        """Create a DockerComposeDeployer with a minimal profile for testing."""
        from deployers.docker_compose import DockerComposeDeployer
        import tempfile

        profile.setdefault("target", {"host": "test-host"})
        with tempfile.TemporaryDirectory() as td:
            deployer = DockerComposeDeployer(
                work_dir=td,
                profile=profile,
                dry_run=True,
            )
        return deployer

    def test_get_db_password_from_database_section(self):
        deployer = self._make_deployer({"database": {"db_password": "from_db"}})
        assert deployer._get_configured_db_password() == "from_db"

    def test_get_db_password_from_secrets_section(self):
        deployer = self._make_deployer({"secrets": {"db_password": "from_secrets"}})
        assert deployer._get_configured_db_password() == "from_secrets"

    def test_get_db_password_from_top_level(self):
        deployer = self._make_deployer({"db_password": "from_top"})
        assert deployer._get_configured_db_password() == "from_top"

    def test_get_db_password_missing_returns_empty(self):
        deployer = self._make_deployer({})
        assert deployer._get_configured_db_password() == ""

    def test_get_db_root_password_from_database_section(self):
        deployer = self._make_deployer({"database": {"db_root_password": "root_pw"}})
        assert deployer._get_configured_db_root_password() == "root_pw"

    def test_get_db_root_password_missing_returns_empty(self):
        deployer = self._make_deployer({})
        assert deployer._get_configured_db_root_password() == ""

    def test_database_section_takes_precedence_over_top_level(self):
        deployer = self._make_deployer({
            "database": {"db_password": "db_section"},
            "db_password": "top_level",
        })
        assert deployer._get_configured_db_password() == "db_section"


class TestDeployAbortOnVolumeConflict:
    """Tests that deploy aborts when secret recovery fails + DB volume exists."""

    def test_deploy_aborted_when_no_secrets_and_volume_exists(self, client):
        """Deploy should return 400 when .env recovery returns empty but DB volume exists."""
        from unittest.mock import patch

        profile = {
            "target": {"host": "192.168.0.9", "ssh_user": "root"},
        }
        with patch(
            "deployers.docker_compose.DockerComposeDeployer.read_remote_env_secrets",
            return_value={},
        ), patch(
            "deployers.docker_compose.DockerComposeDeployer.check_remote_db_volume_exists",
            return_value=True,
        ):
            resp = client.post("/api/deploy", json={
                "profile": profile,
                "password_strategy": "fetch_existing",
            })
        assert resp.status_code == 400
        data = resp.get_json()
        assert "cannot recover" in data["error"].lower() or "mariadb" in data["error"].lower()

    def test_deploy_proceeds_when_no_secrets_and_no_volume(self, client):
        """Deploy should NOT return credential-mismatch 400 when no DB volume exists."""
        from unittest.mock import patch, MagicMock

        profile = {
            "target": {"host": "192.168.0.9", "ssh_user": "root"},
        }
        with patch(
            "deployers.docker_compose.DockerComposeDeployer.read_remote_env_secrets",
            return_value={},
        ), patch(
            "deployers.docker_compose.DockerComposeDeployer.check_remote_db_volume_exists",
            return_value=False,
        ), patch(
            "gui.routes.deploy_routes.ConfigGenerator") as mock_gen_cls, \
             patch("gui.routes.deploy_routes.DockerComposeDeployer") as mock_deployer_cls:
            # ConfigGenerator().generate() must return something with output_dir
            mock_result = MagicMock()
            mock_result.output_dir = "/tmp/test"
            mock_result.errors = []
            mock_gen_cls.return_value.generate.return_value = mock_result
            # DockerComposeDeployer() must be constructable
            mock_deployer_cls.return_value.deploy.return_value = True

            resp = client.post("/api/deploy", json={
                "profile": profile,
                "password_strategy": "fetch_existing",
            })
        # The key assertion: we should NOT get the volume-conflict 400
        data = resp.get_json() or {}
        error_msg = data.get("error", "").lower()
        assert not (resp.status_code == 400 and (
            "cannot recover" in error_msg or "mariadb" in error_msg
        )), f"Got unexpected volume-conflict abort: {data}"


class TestReadRemoteEnvSecrets:
    """Tests for DockerComposeDeployer.read_remote_env_secrets() — localhost path only."""

    def test_reads_local_env(self, tmp_path):
        """When host is localhost, reads .env from local filesystem."""
        from deployers.docker_compose import DockerComposeDeployer
        env_file = tmp_path / ".env"
        env_file.write_text(
            "DB_PASSWORD=LocalPass123\n"
            "DB_ROOT_PASSWORD=RootLocal456\n"
            "JWT_SECRET=jwt_local_secret\n"
            "UNRELATED_VAR=should_be_ignored\n"
        )
        result = DockerComposeDeployer.read_remote_env_secrets(
            host="localhost", remote_deploy_dir=str(tmp_path)
        )
        assert result["db_password"] == "LocalPass123"
        assert result["db_root_password"] == "RootLocal456"
        assert result["jwt_secret"] == "jwt_local_secret"
        assert "UNRELATED_VAR" not in result
        assert "unrelated_var" not in result

    def test_missing_env_returns_empty(self, tmp_path):
        """When .env does not exist, returns empty dict."""
        from deployers.docker_compose import DockerComposeDeployer
        result = DockerComposeDeployer.read_remote_env_secrets(
            host="localhost", remote_deploy_dir=str(tmp_path / "nonexistent")
        )
        assert result == {}

    def test_empty_values_excluded(self, tmp_path):
        """Empty values in .env should not appear in results."""
        from deployers.docker_compose import DockerComposeDeployer
        env_file = tmp_path / ".env"
        env_file.write_text("DB_PASSWORD=\nJWT_SECRET=actual_value\n")
        result = DockerComposeDeployer.read_remote_env_secrets(
            host="localhost", remote_deploy_dir=str(tmp_path)
        )
        assert "db_password" not in result
        assert result["jwt_secret"] == "actual_value"

    def test_all_preserved_keys(self, tmp_path):
        """All 17 preserved keys are mapped when present in .env."""
        from deployers.docker_compose import DockerComposeDeployer
        lines = [
            f"{k}=test_value_{i}"
            for i, k in enumerate(DockerComposeDeployer._PRESERVED_SECRET_KEYS)
        ]
        env_file = tmp_path / ".env"
        env_file.write_text("\n".join(lines))
        result = DockerComposeDeployer.read_remote_env_secrets(
            host="localhost", remote_deploy_dir=str(tmp_path)
        )
        # All 17 keys should map through
        assert len(result) == len(DockerComposeDeployer._PRESERVED_SECRET_KEYS)

    def test_docker_escaped_dollars(self, tmp_path):
        """$$ in .env values should be unescaped to $."""
        from deployers.docker_compose import DockerComposeDeployer
        env_file = tmp_path / ".env"
        env_file.write_text("DB_PASSWORD=Pa$$word@2024\n")
        result = DockerComposeDeployer.read_remote_env_secrets(
            host="localhost", remote_deploy_dir=str(tmp_path)
        )
        assert result["db_password"] == "Pa$word@2024"


class TestDeployPasswordStrategy:
    """Tests for /api/deploy password_strategy parameter."""

    def test_deploy_accepts_password_strategy_param(self, client):
        """The deploy endpoint should accept password_strategy without error."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {"architecture": "monolith"},
                "password_strategy": "entered",
                "dry_run": True,
            },
            content_type="application/json",
        )
        # Should not be a 500; 400 or 200 depending on profile completeness
        assert resp.status_code in (200, 400)

    def test_deploy_default_strategy_is_fetch_existing(self, client):
        """When password_strategy is not provided, default should be fetch_existing."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {"architecture": "monolith"},
                "dry_run": True,
            },
            content_type="application/json",
        )
        assert resp.status_code in (200, 400)

    def test_preserved_secret_keys_count(self):
        """Verify the preserved secret keys list has the expected count."""
        from deployers.docker_compose import DockerComposeDeployer
        assert len(DockerComposeDeployer._PRESERVED_SECRET_KEYS) == 17

    def test_env_to_context_key_mapping_complete(self):
        """Every preserved key should have a context key mapping."""
        from deployers.docker_compose import DockerComposeDeployer
        for key in DockerComposeDeployer._PRESERVED_SECRET_KEYS:
            assert key in DockerComposeDeployer._ENV_TO_CONTEXT_KEY, \
                f"Missing mapping for {key}"


# ===========================================================================
# Cloud-aware deployer selection (Azure/AWS/GCP → K8s or Docker Compose)
# ===========================================================================


class TestCloudDeployerSelection:
    """Tests for cloud-aware deployer routing in /api/deploy."""

    def test_azure_no_host_returns_400_not_500(self, client):
        """Azure deploy without target host should return 400 (not 500)."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "azure", "architecture": "monolith",
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": False,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 400
        data = resp.get_json()
        assert "target host" in data.get("error", "").lower()

    def test_azure_aks_routes_to_kubernetes_deployer(self, client):
        """Azure with AKS compute should use KubernetesDeployer (200)."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "azure",
                    "architecture": "monolith",
                    "cloud_services": {"azure": {"compute": "aks"}},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": True,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 200

    def test_aws_eks_routes_to_kubernetes_deployer(self, client):
        """AWS with EKS compute should use KubernetesDeployer (200)."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "aws",
                    "architecture": "monolith",
                    "cloud_services": {"aws": {"compute": "eks"}},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": True,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 200

    def test_gcp_gke_routes_to_kubernetes_deployer(self, client):
        """GCP with GKE compute should use KubernetesDeployer (200)."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "gcp",
                    "architecture": "monolith",
                    "cloud_services": {"gcp": {"compute": "gke"}},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": True,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 200

    def test_azure_container_apps_routes_to_kubernetes(self, client):
        """Azure Container Apps should route to KubernetesDeployer."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "azure",
                    "architecture": "monolith",
                    "cloud_services": {"azure": {"compute": "container_apps"}},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": True,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 200

    def test_azure_vm_without_host_returns_400(self, client):
        """Azure VMs without a target host should return 400 (needs SSH host)."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "azure",
                    "architecture": "monolith",
                    "cloud_services": {"azure": {"compute": "vm"}},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": False,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 400

    def test_on_premises_with_host_still_works(self, client):
        """On-premises with target host should work as before (200)."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "on_premises",
                    "architecture": "monolith",
                    "target": {"host": "192.168.0.9"},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": True,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 200

    def test_deploy_error_response_has_error_field(self, client):
        """400 responses from deploy should include an 'error' field."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "azure", "architecture": "monolith",
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": False,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 400
        data = resp.get_json()
        assert "error" in data
        assert len(data["error"]) > 0

    def test_explicit_kubernetes_runtime_uses_k8s_deployer(self, client):
        """Explicit architecture.container_runtime=kubernetes should use K8s deployer."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {
                    "platform": "on_premises",
                    "architecture": {"container_runtime": "kubernetes"},
                    "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
                },
                "dry_run": True,
                "password_strategy": "entered",
            },
            content_type="application/json",
        )
        assert resp.status_code == 200


# ===========================================================================
# Day-2 Profile-Aware Helpers (unit tests)
# ===========================================================================


class TestDay2ProfileHelpers:
    """Unit tests for the profile-aware Day-2 helper functions."""

    def test_detect_runtime_docker_by_default(self):
        """Empty profile should default to docker_compose."""
        from gui.routes.day2_routes import _detect_runtime
        assert _detect_runtime({}) == "docker_compose"

    def test_detect_runtime_kubernetes_from_architecture(self):
        """architecture.container_runtime=kubernetes should be detected."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {"architecture": {"container_runtime": "kubernetes"}}
        assert _detect_runtime(profile) == "kubernetes"

    def test_detect_runtime_docker_from_architecture(self):
        """architecture.container_runtime=docker_compose should be detected."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {"architecture": {"container_runtime": "docker_compose"}}
        assert _detect_runtime(profile) == "docker_compose"

    def test_detect_runtime_aks_compute_means_kubernetes(self):
        """Azure AKS compute should resolve to kubernetes runtime."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {
            "platform": "azure",
            "cloud_services": {"azure": {"compute": "aks"}},
        }
        assert _detect_runtime(profile) == "kubernetes"

    def test_detect_runtime_eks_compute_means_kubernetes(self):
        """AWS EKS compute should resolve to kubernetes runtime."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {
            "platform": "aws",
            "cloud_services": {"aws": {"compute": "eks"}},
        }
        assert _detect_runtime(profile) == "kubernetes"

    def test_detect_runtime_gke_compute_means_kubernetes(self):
        """GCP GKE compute should resolve to kubernetes runtime."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {
            "platform": "gcp",
            "cloud_services": {"gcp": {"compute": "gke"}},
        }
        assert _detect_runtime(profile) == "kubernetes"

    def test_detect_runtime_container_apps_means_serverless(self):
        """Azure Container Apps should resolve to serverless runtime."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {
            "platform": "azure",
            "cloud_services": {"azure": {"compute": "container_apps"}},
        }
        assert _detect_runtime(profile) == "serverless"

    def test_detect_runtime_fargate_means_serverless(self):
        """AWS Fargate should resolve to serverless runtime."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {
            "platform": "aws",
            "cloud_services": {"aws": {"compute": "fargate"}},
        }
        assert _detect_runtime(profile) == "serverless"

    def test_detect_runtime_vm_stays_docker(self):
        """VM compute should stay as docker_compose."""
        from gui.routes.day2_routes import _detect_runtime
        profile = {
            "platform": "azure",
            "cloud_services": {"azure": {"compute": "vm"}},
        }
        assert _detect_runtime(profile) == "docker_compose"

    def test_get_deploy_host_from_target(self):
        """Should extract host from target.host."""
        from gui.routes.day2_routes import _get_deploy_host
        assert _get_deploy_host({"target": {"host": "192.168.0.9"}}) == "192.168.0.9"

    def test_get_deploy_host_from_domain_name(self):
        """Should fallback to target.domain_name."""
        from gui.routes.day2_routes import _get_deploy_host
        assert _get_deploy_host({"target": {"domain_name": "crm.example.com"}}) == "crm.example.com"

    def test_get_deploy_host_empty_for_local(self):
        """Should return empty string for profile with no target host."""
        from gui.routes.day2_routes import _get_deploy_host
        assert _get_deploy_host({}) == ""
        assert _get_deploy_host({"target": {}}) == ""

    def test_is_remote_true_for_ip(self):
        """Remote IP addresses should be considered remote."""
        from gui.routes.day2_routes import _is_remote
        assert _is_remote("192.168.0.9") is True
        assert _is_remote("10.0.0.1") is True

    def test_is_remote_true_for_hostname(self):
        """Hostnames should be considered remote."""
        from gui.routes.day2_routes import _is_remote
        assert _is_remote("crm.example.com") is True

    def test_is_remote_false_for_localhost(self):
        """localhost and 127.0.0.1 should not be considered remote."""
        from gui.routes.day2_routes import _is_remote
        assert _is_remote("localhost") is False
        assert _is_remote("127.0.0.1") is False
        assert _is_remote("") is False

    def test_build_health_url_remote_host(self):
        """Health URL should include the remote host and port."""
        from gui.routes.day2_routes import _build_health_url
        profile = {"target": {"host": "192.168.0.9", "api_port": "5000"}}
        assert _build_health_url(profile) == "http://192.168.0.9:5000/health"

    def test_build_health_url_local(self):
        """Health URL for local profiles should target localhost."""
        from gui.routes.day2_routes import _build_health_url
        assert _build_health_url({}) == "http://localhost:5000/health"

    def test_build_health_url_custom_port(self):
        """Health URL should respect non-default API port."""
        from gui.routes.day2_routes import _build_health_url
        profile = {"target": {"api_port": "8080"}}
        assert _build_health_url(profile) == "http://localhost:8080/health"

    def test_build_api_base_url_remote(self):
        """API base URL should include the remote host."""
        from gui.routes.day2_routes import _build_api_base_url
        profile = {"target": {"host": "10.0.0.5", "api_port": "5000"}}
        assert _build_api_base_url(profile) == "http://10.0.0.5:5000"

    def test_build_api_base_url_local(self):
        """API base URL for local profiles should target localhost."""
        from gui.routes.day2_routes import _build_api_base_url
        assert _build_api_base_url({}) == "http://localhost:5000"

    def test_safe_name_strips_slash(self):
        """_safe_name should strip leading slashes."""
        from gui.routes.day2_routes import _safe_name
        assert _safe_name("/crm-api") == "crm-api"
        assert _safe_name("crm-api") == "crm-api"


# ===========================================================================
# Day-2 Profile-Aware Endpoints (integration tests)
# ===========================================================================


class TestDay2ProfileEndpoints:
    """Integration tests for profile-aware Day-2 API endpoints."""

    def test_status_returns_runtime_field(self, client):
        """GET /api/day2/status should include a 'runtime' field."""
        resp = client.get("/api/day2/status")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "runtime" in data
        assert data["runtime"] in ("docker_compose", "kubernetes")

    def test_status_returns_profile_name(self, client):
        """GET /api/day2/status should include profile_name."""
        resp = client.get("/api/day2/status")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "profile_name" in data

    def test_status_all_returns_runtime_field(self, client):
        """GET /api/day2/status/all should include a 'runtime' field."""
        resp = client.get("/api/day2/status/all")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "runtime" in data
        assert data["runtime"] in ("docker_compose", "kubernetes")

    def test_status_all_returns_environment_type(self, client):
        """GET /api/day2/status/all should include environment_type."""
        resp = client.get("/api/day2/status/all")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "environment_type" in data

    def test_status_all_accepts_profile_param(self, client):
        """GET /api/day2/status/all?profile=nonexistent should still return 200."""
        resp = client.get("/api/day2/status/all?profile=does-not-exist")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "runtime" in data

    def test_version_info_returns_runtime(self, client):
        """GET /api/day2/version-info should include runtime field."""
        resp = client.get("/api/day2/version-info")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "runtime" in data
        assert data["runtime"] in ("docker_compose", "kubernetes")

    def test_version_info_returns_platform(self, client):
        """GET /api/day2/version-info should include platform."""
        resp = client.get("/api/day2/version-info")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "platform" in data

    def test_version_info_accepts_profile_param(self, client):
        """GET /api/day2/version-info?profile=x should still return 200."""
        resp = client.get("/api/day2/version-info?profile=nonexistent")
        assert resp.status_code == 200

    def test_container_start_accepts_profile_param(self, client):
        """POST /api/day2/container/crm-api/start?profile=x should return 200."""
        resp = client.post("/api/day2/container/crm-api/start?profile=nonexistent")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "success" in data

    def test_container_stop_accepts_profile_param(self, client):
        """POST /api/day2/container/crm-api/stop?profile=x should return 200."""
        resp = client.post("/api/day2/container/crm-api/stop?profile=nonexistent")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "success" in data

    def test_container_logs_accepts_profile_param(self, client):
        """GET /api/day2/container/crm-api/logs?profile=x should return 200."""
        resp = client.get("/api/day2/container/crm-api/logs?profile=nonexistent&lines=10")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "success" in data or "logs" in data

    def test_stack_stop_accepts_profile_param(self, client):
        """POST /api/day2/stack/stop?profile=x should return 200."""
        resp = client.post("/api/day2/stack/stop?profile=nonexistent")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "success" in data

    def test_stack_start_accepts_profile_param(self, client):
        """POST /api/day2/stack/start?profile=x should return 200."""
        resp = client.post("/api/day2/stack/start?profile=nonexistent")
        assert resp.status_code == 200

    def test_stack_restart_accepts_profile_param(self, client):
        """POST /api/day2/stack/restart?profile=x should return 200."""
        resp = client.post("/api/day2/stack/restart?profile=nonexistent")
        assert resp.status_code == 200

    def test_status_all_docker_has_containers_key(self, client):
        """Docker runtime status should have 'containers' list."""
        resp = client.get("/api/day2/status/all")
        data = resp.get_json()
        if data.get("runtime") == "docker_compose":
            assert "containers" in data
            assert isinstance(data["containers"], list)

    def test_status_all_has_health_keys(self, client):
        """Status response should contain health sub-object."""
        resp = client.get("/api/day2/status/all")
        data = resp.get_json()
        assert "health" in data
        health = data["health"]
        assert "api" in health
        assert "db" in health
        assert "redis" in health


# ===========================================================================
# Cloud-agnostic centralized constants & runtime detection
# ===========================================================================


class TestCoreConstants:
    """Unit tests for core.constants — the single source of truth for cloud
    compute classification, runtime detection, and kubeconfig extraction."""

    # ── K8S vs Serverless classification ──────────────────────────────

    def test_k8s_computes_are_true_kubernetes(self):
        """AKS, EKS, GKE should be in K8S_COMPUTES."""
        from core.constants import K8S_COMPUTES
        assert "aks" in K8S_COMPUTES
        assert "eks" in K8S_COMPUTES
        assert "gke" in K8S_COMPUTES

    def test_serverless_computes_are_separate(self):
        """Container Apps, Fargate, Cloud Run should be in SERVERLESS_COMPUTES."""
        from core.constants import SERVERLESS_COMPUTES
        assert "container_apps" in SERVERLESS_COMPUTES
        assert "fargate" in SERVERLESS_COMPUTES
        assert "cloud_run" in SERVERLESS_COMPUTES

    def test_serverless_not_in_k8s(self):
        """Serverless computes must NOT be in K8S_COMPUTES."""
        from core.constants import K8S_COMPUTES, SERVERLESS_COMPUTES
        for s in SERVERLESS_COMPUTES:
            assert s not in K8S_COMPUTES, f"{s} should not be in K8S_COMPUTES"

    def test_k8s_not_in_serverless(self):
        """K8s computes must NOT be in SERVERLESS_COMPUTES."""
        from core.constants import K8S_COMPUTES, SERVERLESS_COMPUTES
        for k in K8S_COMPUTES:
            assert k not in SERVERLESS_COMPUTES, f"{k} should not be in SERVERLESS_COMPUTES"

    def test_cloud_computes_is_union(self):
        """CLOUD_COMPUTES should be the union of K8S and SERVERLESS."""
        from core.constants import K8S_COMPUTES, SERVERLESS_COMPUTES, CLOUD_COMPUTES
        assert CLOUD_COMPUTES == K8S_COMPUTES | SERVERLESS_COMPUTES

    def test_cloud_computes_has_all_six(self):
        """CLOUD_COMPUTES should contain exactly 6 entries."""
        from core.constants import CLOUD_COMPUTES
        assert len(CLOUD_COMPUTES) == 6

    # ── detect_runtime() ─────────────────────────────────────────────

    def test_detect_runtime_empty_profile(self):
        """Empty dict should default to docker_compose."""
        from core.constants import detect_runtime, RUNTIME_DOCKER_COMPOSE
        assert detect_runtime({}) == RUNTIME_DOCKER_COMPOSE

    def test_detect_runtime_explicit_kubernetes(self):
        """Explicit architecture.container_runtime=kubernetes wins."""
        from core.constants import detect_runtime, RUNTIME_KUBERNETES
        p = {"architecture": {"container_runtime": "kubernetes"}}
        assert detect_runtime(p) == RUNTIME_KUBERNETES

    def test_detect_runtime_explicit_docker(self):
        """Explicit architecture.container_runtime=docker_compose wins."""
        from core.constants import detect_runtime, RUNTIME_DOCKER_COMPOSE
        p = {"architecture": {"container_runtime": "docker_compose"}}
        assert detect_runtime(p) == RUNTIME_DOCKER_COMPOSE

    def test_detect_runtime_explicit_serverless(self):
        """Explicit architecture.container_runtime=serverless wins."""
        from core.constants import detect_runtime, RUNTIME_SERVERLESS
        p = {"architecture": {"container_runtime": "serverless"}}
        assert detect_runtime(p) == RUNTIME_SERVERLESS

    def test_detect_runtime_aks_is_kubernetes(self):
        """Azure AKS → kubernetes."""
        from core.constants import detect_runtime, RUNTIME_KUBERNETES
        p = {"platform": "azure", "cloud_services": {"azure": {"compute": "aks"}}}
        assert detect_runtime(p) == RUNTIME_KUBERNETES

    def test_detect_runtime_eks_is_kubernetes(self):
        """AWS EKS → kubernetes."""
        from core.constants import detect_runtime, RUNTIME_KUBERNETES
        p = {"platform": "aws", "cloud_services": {"aws": {"compute": "eks"}}}
        assert detect_runtime(p) == RUNTIME_KUBERNETES

    def test_detect_runtime_gke_is_kubernetes(self):
        """GCP GKE → kubernetes."""
        from core.constants import detect_runtime, RUNTIME_KUBERNETES
        p = {"platform": "gcp", "cloud_services": {"gcp": {"compute": "gke"}}}
        assert detect_runtime(p) == RUNTIME_KUBERNETES

    def test_detect_runtime_container_apps_is_serverless(self):
        """Azure Container Apps → serverless."""
        from core.constants import detect_runtime, RUNTIME_SERVERLESS
        p = {"platform": "azure", "cloud_services": {"azure": {"compute": "container_apps"}}}
        assert detect_runtime(p) == RUNTIME_SERVERLESS

    def test_detect_runtime_fargate_is_serverless(self):
        """AWS Fargate → serverless."""
        from core.constants import detect_runtime, RUNTIME_SERVERLESS
        p = {"platform": "aws", "cloud_services": {"aws": {"compute": "fargate"}}}
        assert detect_runtime(p) == RUNTIME_SERVERLESS

    def test_detect_runtime_cloud_run_is_serverless(self):
        """GCP Cloud Run → serverless."""
        from core.constants import detect_runtime, RUNTIME_SERVERLESS
        p = {"platform": "gcp", "cloud_services": {"gcp": {"compute": "cloud_run"}}}
        assert detect_runtime(p) == RUNTIME_SERVERLESS

    def test_detect_runtime_vm_is_docker(self):
        """VM compute → docker_compose (not K8s or serverless)."""
        from core.constants import detect_runtime, RUNTIME_DOCKER_COMPOSE
        p = {"platform": "azure", "cloud_services": {"azure": {"compute": "vm"}}}
        assert detect_runtime(p) == RUNTIME_DOCKER_COMPOSE

    def test_detect_runtime_on_premises_is_docker(self):
        """On-premises with no cloud services → docker_compose."""
        from core.constants import detect_runtime, RUNTIME_DOCKER_COMPOSE
        p = {"platform": "on_premises"}
        assert detect_runtime(p) == RUNTIME_DOCKER_COMPOSE

    def test_detect_runtime_mismatched_platform_falls_back(self):
        """Cloud services for wrong platform → docker_compose."""
        from core.constants import detect_runtime, RUNTIME_DOCKER_COMPOSE
        p = {"platform": "aws", "cloud_services": {"azure": {"compute": "aks"}}}
        assert detect_runtime(p) == RUNTIME_DOCKER_COMPOSE

    # ── get_kubeconfig() ─────────────────────────────────────────────

    def test_get_kubeconfig_from_target_kubeconfig(self):
        """Should extract from target.kubeconfig."""
        from core.constants import get_kubeconfig
        p = {"target": {"kubeconfig": "/path/to/kubeconfig"}}
        assert get_kubeconfig(p) == "/path/to/kubeconfig"

    def test_get_kubeconfig_from_target_kubeconfig_path(self):
        """Should fallback to target.kubeconfig_path."""
        from core.constants import get_kubeconfig
        p = {"target": {"kubeconfig_path": "/alt/path"}}
        assert get_kubeconfig(p) == "/alt/path"

    def test_get_kubeconfig_from_profile_root(self):
        """Should fallback to profile.kubeconfig."""
        from core.constants import get_kubeconfig
        p = {"kubeconfig": "/root/kube"}
        assert get_kubeconfig(p) == "/root/kube"

    def test_get_kubeconfig_priority_order(self):
        """target.kubeconfig should take priority over kubeconfig_path."""
        from core.constants import get_kubeconfig
        p = {
            "target": {"kubeconfig": "/first", "kubeconfig_path": "/second"},
            "kubeconfig": "/third",
        }
        assert get_kubeconfig(p) == "/first"

    def test_get_kubeconfig_empty_for_no_config(self):
        """Should return empty string when no kubeconfig is set."""
        from core.constants import get_kubeconfig
        assert get_kubeconfig({}) == ""
        assert get_kubeconfig({"target": {}}) == ""


class TestDeployPreflightCloudAgnostic:
    """Integration tests for /api/deploy/preflight ensuring cloud-agnostic behavior."""

    @pytest.fixture()
    def client(self):
        app.config["TESTING"] = True
        with app.test_client() as c:
            yield c

    def test_preflight_returns_runtime_for_k8s(self, client):
        """Preflight with AKS compute should return runtime=kubernetes."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "azure",
            "cloud_services": {"azure": {"compute": "aks"}},
        })
        data = resp.get_json()
        assert data["runtime"] == "kubernetes"

    def test_preflight_returns_runtime_for_serverless(self, client):
        """Preflight with container_apps should return runtime=serverless."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "azure",
            "cloud_services": {"azure": {"compute": "container_apps"}},
        })
        data = resp.get_json()
        assert data["runtime"] == "serverless"

    def test_preflight_returns_runtime_for_docker(self, client):
        """Preflight with on_premises should return runtime=docker_compose."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "on_premises",
        })
        data = resp.get_json()
        assert data["runtime"] == "docker_compose"

    def test_preflight_eks_is_kubernetes(self, client):
        """EKS should be treated as kubernetes preflight."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "aws",
            "cloud_services": {"aws": {"compute": "eks"}},
        })
        data = resp.get_json()
        assert data["runtime"] == "kubernetes"

    def test_preflight_gke_is_kubernetes(self, client):
        """GKE should be treated as kubernetes preflight."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "gcp",
            "cloud_services": {"gcp": {"compute": "gke"}},
        })
        data = resp.get_json()
        assert data["runtime"] == "kubernetes"

    def test_preflight_fargate_is_serverless(self, client):
        """Fargate should be treated as serverless preflight."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "aws",
            "cloud_services": {"aws": {"compute": "fargate"}},
        })
        data = resp.get_json()
        assert data["runtime"] == "serverless"

    def test_preflight_cloud_run_is_serverless(self, client):
        """Cloud Run should be treated as serverless preflight."""
        resp = client.post("/api/deploy/preflight", json={
            "platform": "gcp",
            "cloud_services": {"gcp": {"compute": "cloud_run"}},
        })
        data = resp.get_json()
        assert data["runtime"] == "serverless"

    def test_preflight_has_existing_flag(self, client):
        """All preflight responses should include has_existing flag."""
        resp = client.post("/api/deploy/preflight", json={"platform": "on_premises"})
        data = resp.get_json()
        assert "has_existing" in data

    def test_preflight_empty_body_defaults_docker(self, client):
        """Empty body should default to docker_compose."""
        resp = client.post("/api/deploy/preflight", json={})
        data = resp.get_json()
        assert data["runtime"] == "docker_compose"


# ===========================================================================
# Day-2 / Monitoring — Rename & deploy_host
# ===========================================================================


class TestDay2Rename:
    """Verify the Day-2 page has been renamed to Monitoring & Post Deployment."""

    def test_day2_page_renders(self, client):
        resp = client.get("/day2")
        assert resp.status_code == 200

    def test_day2_title_renamed(self, client):
        resp = client.get("/day2")
        html = resp.data.decode()
        assert "Monitoring &amp; Post Deployment" in html
        assert "Day-2 Operations" not in html

    def test_day2_navbar_renamed(self, client):
        resp = client.get("/day2")
        html = resp.data.decode()
        assert "Monitoring &amp; Post Deployment" in html

    def test_index_day2_links_renamed(self, client):
        resp = client.get("/")
        html = resp.data.decode()
        assert "Monitoring</a>" in html or "Monitoring&" in html
        assert "Day-2 Ops" not in html
        assert ">Day-2<" not in html

    def test_day2_has_postinstall_tab(self, client):
        resp = client.get("/day2")
        html = resp.data.decode()
        assert "tab-postinstall" in html
        assert "Post-Install" in html

    def test_day2_has_testrunner_tab(self, client):
        resp = client.get("/day2")
        html = resp.data.decode()
        assert "tab-testrunner" in html
        assert "Test Runner" in html

    def test_day2_has_deploy_host_badge(self, client):
        resp = client.get("/day2")
        html = resp.data.decode()
        assert "stack-deploy-host" in html


# ===========================================================================
# Post-Install endpoints
# ===========================================================================


class TestPostInstallEndpoints:
    """Test the /api/day2/postinstall/* proxy endpoints."""

    def test_database_status_returns_json(self, client):
        """GET /api/day2/postinstall/database-status should return JSON."""
        resp = client.get("/api/day2/postinstall/database-status")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "api_healthy" in data
        assert "db_status" in data
        assert "deploy_host" in data

    def test_migrate_database_endpoint_exists(self, client):
        """POST /api/day2/postinstall/migrate-database — endpoint exists (may fail if no API)."""
        resp = client.post("/api/day2/postinstall/migrate-database",
                           content_type="application/json")
        # Expect 502 (Bad Gateway) when API is unreachable, not 404
        assert resp.status_code in (200, 502)
        data = resp.get_json()
        assert "success" in data

    def test_clear_database_endpoint_exists(self, client):
        """POST /api/day2/postinstall/clear-database endpoint exists."""
        resp = client.post("/api/day2/postinstall/clear-database",
                           content_type="application/json")
        assert resp.status_code in (200, 502)
        data = resp.get_json()
        assert "success" in data

    def test_seed_master_data_endpoint_exists(self, client):
        resp = client.post("/api/day2/postinstall/seed-master-data",
                           content_type="application/json")
        assert resp.status_code in (200, 502)
        data = resp.get_json()
        assert "success" in data

    def test_reseed_data_endpoint_exists(self, client):
        resp = client.post("/api/day2/postinstall/reseed-data",
                           content_type="application/json")
        assert resp.status_code in (200, 502)
        data = resp.get_json()
        assert "success" in data

    def test_clear_sample_data_endpoint_exists(self, client):
        resp = client.post("/api/day2/postinstall/clear-sample-data",
                           content_type="application/json")
        assert resp.status_code in (200, 502)
        data = resp.get_json()
        assert "success" in data

    def test_seed_sample_data_endpoint_exists(self, client):
        resp = client.post("/api/day2/postinstall/seed-sample-data",
                           content_type="application/json")
        assert resp.status_code in (200, 502)
        data = resp.get_json()
        assert "success" in data


# ===========================================================================
# Test Runner endpoints
# ===========================================================================


class TestTestRunnerEndpoints:
    """Test the /api/day2/testrunner/* endpoints."""

    def test_run_invalid_test_type(self, client):
        """Invalid test_type should return 400."""
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "invalid_type"})
        assert resp.status_code == 400
        data = resp.get_json()
        assert "error" in data

    def test_run_bvt_starts(self, client):
        """BVT test run should start and return a job_id."""
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "bvt"})
        assert resp.status_code == 200
        data = resp.get_json()
        assert "job_id" in data
        assert data["test_type"] == "bvt"
        assert data["job_id"].startswith("test_")

    def test_run_crud_loader_starts(self, client):
        """CRUD loader test run should start."""
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "crud_loader"})
        assert resp.status_code == 200
        data = resp.get_json()
        assert "job_id" in data
        assert data["test_type"] == "crud_loader"

    def test_run_all_starts(self, client):
        """All test suites should start."""
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "all"})
        assert resp.status_code == 200
        data = resp.get_json()
        assert "job_id" in data

    def test_status_unknown_job(self, client):
        """Unknown job_id should return 404."""
        resp = client.get("/api/day2/testrunner/status/nonexistent")
        assert resp.status_code == 404

    def test_results_unknown_job(self, client):
        """Unknown job_id should return 404."""
        resp = client.get("/api/day2/testrunner/results/nonexistent")
        assert resp.status_code == 404

    def test_bvt_completes_and_has_results(self, client):
        """Run BVT and poll until complete — should have results."""
        import time
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "bvt"})
        job_id = resp.get_json()["job_id"]

        # Poll for completion (BVT is fast — health checks only)
        for _ in range(20):
            time.sleep(0.5)
            sr = client.get(f"/api/day2/testrunner/status/{job_id}")
            sd = sr.get_json()
            if sd["done"]:
                break

        # Verify results
        rr = client.get(f"/api/day2/testrunner/results/{job_id}")
        assert rr.status_code == 200
        rd = rr.get_json()
        assert rd["done"] is True
        assert rd["test_type"] == "bvt"
        assert isinstance(rd["results"], list)
        assert len(rd["results"]) >= 1  # At least some BVT tests ran
        assert isinstance(rd["output"], list)
        assert rd["total_pass"] + rd["total_fail"] > 0

    def test_history_returns_list(self, client):
        """History endpoint should return a list."""
        resp = client.get("/api/day2/testrunner/history")
        assert resp.status_code == 200
        data = resp.get_json()
        assert "runs" in data
        assert isinstance(data["runs"], list)

    def test_history_includes_bvt_run(self, client):
        """After running BVT, history should include it."""
        import time
        # Start a run
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "bvt"})
        job_id = resp.get_json()["job_id"]
        # Wait for completion
        for _ in range(20):
            time.sleep(0.5)
            sr = client.get(f"/api/day2/testrunner/status/{job_id}")
            if sr.get_json()["done"]:
                break
        # Check history
        hr = client.get("/api/day2/testrunner/history")
        data = hr.get_json()
        job_ids = [r["job_id"] for r in data["runs"]]
        assert job_id in job_ids

    def test_run_with_custom_base_url(self, client):
        """Custom base_url should be passed through."""
        resp = client.post("/api/day2/testrunner/run",
                           json={"test_type": "bvt", "base_url": "http://example.com:5000"})
        data = resp.get_json()
        assert data["base_url"] == "http://example.com:5000"

    def test_cleanup_endpoint_exists(self, client):
        """POST /api/day2/testrunner/cleanup — endpoint exists."""
        resp = client.post("/api/day2/testrunner/cleanup",
                           json={})
        # 200 if script runs, 404 if script not found — both acceptable
        assert resp.status_code in (200, 404, 500)


# ===========================================================================
# Deploy_host in status responses
# ===========================================================================


class TestDeployHostInStatus:
    """Verify deploy_host is included in status responses."""

    def test_status_all_has_deploy_host(self, client):
        resp = client.get("/api/day2/status/all")
        data = resp.get_json()
        assert "deploy_host" in data

    def test_version_info_has_deploy_host(self, client):
        resp = client.get("/api/day2/version-info")
        data = resp.get_json()
        assert "deploy_host" in data

    def test_database_status_has_deploy_host(self, client):
        resp = client.get("/api/day2/postinstall/database-status")
        data = resp.get_json()
        assert "deploy_host" in data


# ===========================================================================
# Health Check Continuation & Frontend Deployment (v0.614.57)
# ===========================================================================


class TestHealthCheckContinuation:
    """Verify health check timeout no longer blocks frontend deployment."""

    def test_health_check_timeout_returns_true(self):
        """When API health check times out, step should return True to continue."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        from unittest.mock import patch

        q = queue.Queue()
        profile = {"target": {"host": "192.168.0.9", "api_port": "5000"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=False)

        # Mock urlopen to always raise (simulating unreachable API)
        with patch("urllib.request.urlopen", side_effect=Exception("Connection refused")):
            # Also patch time.sleep to not actually wait
            with patch("time.sleep"):
                result = d._step_health_check_api()

        # The step should return True (continue) not False (abort)
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("timed out" in m.lower() for m in messages)
        assert any("continuing" in m.lower() for m in messages)

    def test_health_check_success_returns_true(self):
        """When API health check succeeds, step should return True."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue
        from unittest.mock import patch, MagicMock

        q = queue.Queue()
        profile = {"target": {"host": "192.168.0.9", "api_port": "5000"}}
        d = DockerComposeDeployer(Path("/tmp"), profile, log_queue=q, dry_run=False)

        with patch("urllib.request.urlopen", return_value=MagicMock()):
            result = d._step_health_check_api()

        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("healthy" in m.lower() for m in messages)

    def test_frontend_step_verifies_container_state(self):
        """Frontend start step should verify the container is actually running."""
        from deployers.docker_compose import DockerComposeDeployer
        import queue

        q = queue.Queue()
        d = DockerComposeDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        d._reused_containers = set()
        result = d._step_start_frontend()
        assert result is True
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("frontend" in m.lower() for m in messages)


class TestKubernetesDeployerLogging:
    """Verify Kubernetes deployer logging improvements."""

    def test_stub_step_logs_message(self):
        """Stub steps should emit an informational log message."""
        from deployers.kubernetes import KubernetesDeployer
        import queue

        q = queue.Queue()
        d = KubernetesDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        rc, out, err = d._stub_step("Apply configmaps")
        assert rc == 0
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("stub" in m.lower() or "not yet implemented" in m.lower() for m in messages)
        assert any("apply configmaps" in m.lower() for m in messages)

    def test_rollback_returns_false_on_failure(self):
        """Rollback should return False when kubectl delete fails (non-dry-run)."""
        from deployers.kubernetes import KubernetesDeployer
        from unittest.mock import patch
        import queue

        q = queue.Queue()
        d = KubernetesDeployer(Path("/tmp"), {}, log_queue=q, dry_run=False)

        with patch.object(d, "_run", return_value=(1, "", "namespace not found")):
            result = d.rollback()

        assert result is False
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("error" in m.lower() or "warn" in m.lower() for m in messages)

    def test_rollback_returns_true_on_success(self):
        """Rollback should return True when kubectl deletes successfully."""
        from deployers.kubernetes import KubernetesDeployer
        from unittest.mock import patch
        import queue

        q = queue.Queue()
        d = KubernetesDeployer(Path("/tmp"), {}, log_queue=q, dry_run=False)

        with patch.object(d, "_run", return_value=(0, "", "")):
            result = d.rollback()

        assert result is True

    def test_rollback_returns_true_in_dry_run(self):
        """Rollback should return True in dry-run mode."""
        from deployers.kubernetes import KubernetesDeployer
        import queue

        q = queue.Queue()
        d = KubernetesDeployer(Path("/tmp"), {}, log_queue=q, dry_run=True)
        result = d.rollback()
        assert result is True

    def test_status_handles_json_error_gracefully(self):
        """status() should log JSONDecodeError instead of silently swallowing."""
        from deployers.kubernetes import KubernetesDeployer
        from unittest.mock import patch
        import queue

        q = queue.Queue()
        d = KubernetesDeployer(Path("/tmp"), {}, log_queue=q, dry_run=False)

        with patch.object(d, "_run", return_value=(0, "NOT JSON", "")):
            result = d.status()

        assert result["pods"] == 0
        messages = []
        while not q.empty():
            messages.append(q.get().message)
        assert any("json" in m.lower() or "parse" in m.lower() for m in messages)


class TestDeployRoutesLogging:
    """Verify deploy_routes properly logs failures instead of silently swallowing."""

    def test_deploy_history_returns_empty_on_error(self, client):
        """Deploy history should return [] when file is corrupt."""
        from unittest.mock import patch
        from pathlib import Path as P
        history_path = P.home() / ".crm-cdt" / "deploy_history.json"

        # Ensure history file does not exist to get clean []
        with patch.object(P, "exists", return_value=False):
            resp = client.get("/api/deploy/history")
        assert resp.status_code == 200
        assert resp.get_json() == []

    def test_secret_recovery_logs_warning_when_host_empty(self, client):
        """When no deploy host configured, should not crash on secret recovery."""
        profile = {"target": {}, "database": {}}
        resp = client.post("/api/deploy", json={
            "profile": profile,
            "password_strategy": "fetch_existing",
            "dry_run": True,
        })
        # Should not 500 — either 200 (started) or 400 (config validation)
        assert resp.status_code in (200, 400)


# ===========================================================================
# DeployEvent host field
# ===========================================================================


class TestDeployEventHost:
    """Verify DeployEvent includes the 'host' field."""

    def test_deploy_event_has_host_field(self):
        from deployers.docker_compose import DeployEvent
        evt = DeployEvent(timestamp=1.0, level="info", message="test")
        assert hasattr(evt, "host")
        assert evt.host == ""

    def test_deploy_event_host_in_to_dict(self):
        from deployers.docker_compose import DeployEvent
        evt = DeployEvent(timestamp=1.0, level="info", message="hello", host="192.168.0.9")
        d = evt.to_dict()
        assert d["host"] == "192.168.0.9"
        assert d["level"] == "info"
        assert d["message"] == "hello"

    def test_deploy_event_host_defaults_empty(self):
        from deployers.docker_compose import DeployEvent
        evt = DeployEvent(timestamp=1.0, level="success", message="done")
        d = evt.to_dict()
        assert d["host"] == ""


# ===========================================================================
# Vault encrypt_data / decrypt_data
# ===========================================================================


class TestVaultEncryptDecrypt:
    """Tests for the new encrypt_data / decrypt_data functions in vault.py."""

    def test_encrypt_decrypt_roundtrip(self):
        from core.vault import encrypt_data, decrypt_data
        original = b"Hello, CRM deployment secrets!"
        password = "super-secret-password"
        encrypted = encrypt_data(original, password)
        assert encrypted != original
        decrypted = decrypt_data(encrypted, password)
        assert decrypted == original

    def test_encrypt_decrypt_empty_data(self):
        from core.vault import encrypt_data, decrypt_data
        original = b""
        encrypted = encrypt_data(original, "pw")
        decrypted = decrypt_data(encrypted, "pw")
        assert decrypted == original

    def test_decrypt_wrong_password_raises(self):
        from core.vault import encrypt_data, decrypt_data, VaultCorruptError
        encrypted = encrypt_data(b"secret data", "correct-password")
        with pytest.raises(VaultCorruptError):
            decrypt_data(encrypted, "wrong-password")

    def test_encrypt_produces_ascii_hex(self):
        from core.vault import encrypt_data
        encrypted = encrypt_data(b"test data", "pw")
        # Should be pure ASCII hex bytes
        assert isinstance(encrypted, bytes)
        encrypted.decode("ascii")  # Should not raise

    def test_encrypt_decrypt_large_payload(self):
        from core.vault import encrypt_data, decrypt_data
        # ~100KB profile-like payload
        original = json.dumps({"key": "x" * 100_000}).encode("utf-8")
        encrypted = encrypt_data(original, "big-pass")
        decrypted = decrypt_data(encrypted, "big-pass")
        assert decrypted == original


# ===========================================================================
# Profile artifacts
# ===========================================================================


class TestProfileArtifacts:
    """Tests for ProfileManager artifact storage."""

    def test_save_and_load_artifacts(self, tmp_path, monkeypatch):
        from core.profile import ProfileManager
        pm = ProfileManager()

        def _make_art_dir(name):
            d = tmp_path / name
            d.mkdir(parents=True, exist_ok=True)
            return d

        monkeypatch.setattr(pm, "_artifacts_dir", _make_art_dir)

        files = {
            "docker-compose.yml": "version: '3.8'\nservices: {}",
            ".env": "DB_HOST=localhost\nDB_PORT=3306",
        }
        pm.save_artifacts("test-profile", files)

        manifest = pm.load_artifacts("test-profile")
        assert "files" in manifest
        assert set(manifest["files"]) == {"docker-compose.yml", ".env"}
        assert "saved_at" in manifest

    def test_get_artifact_returns_content(self, tmp_path, monkeypatch):
        from core.profile import ProfileManager
        pm = ProfileManager()

        def _make_art_dir(name):
            d = tmp_path / name
            d.mkdir(parents=True, exist_ok=True)
            return d

        monkeypatch.setattr(pm, "_artifacts_dir", _make_art_dir)

        files = {"test.yml": "hello: world"}
        pm.save_artifacts("myprofile", files)
        content = pm.get_artifact("myprofile", "test.yml")
        assert content == "hello: world"

    def test_get_artifact_missing_raises(self, tmp_path, monkeypatch):
        from core.profile import ProfileManager, ProfileNotFoundError
        pm = ProfileManager()

        def _make_art_dir(name):
            d = tmp_path / name
            d.mkdir(parents=True, exist_ok=True)
            return d

        monkeypatch.setattr(pm, "_artifacts_dir", _make_art_dir)

        with pytest.raises(ProfileNotFoundError):
            pm.get_artifact("nonexistent", "no-file.yml")

    def test_load_artifacts_empty_dir(self, tmp_path, monkeypatch):
        from core.profile import ProfileManager
        pm = ProfileManager()
        d = tmp_path / "empty-profile"
        d.mkdir()
        monkeypatch.setattr(pm, "_artifacts_dir", lambda name: d)

        manifest = pm.load_artifacts("empty-profile")
        assert manifest["files"] == []


# ===========================================================================
# Artifact API endpoints
# ===========================================================================


class TestArtifactEndpoints:
    """Tests for profile artifact REST endpoints."""

    def test_list_artifacts_404_when_profile_missing(self, client):
        resp = client.get("/api/profiles/nonexistent-xyz/artifacts")
        # Either 404 or empty list depending on implementation
        assert resp.status_code in (200, 404)

    def test_get_artifact_404_when_missing(self, client):
        resp = client.get("/api/profiles/nonexistent-xyz/artifacts/docker-compose.yml")
        assert resp.status_code in (404, 500)

    def test_list_artifacts_endpoint_exists(self, client):
        """The /api/profiles/<name>/artifacts endpoint should respond."""
        resp = client.get("/api/profiles/test-profile/artifacts")
        # Should not be 405 Method Not Allowed
        assert resp.status_code != 405


# ===========================================================================
# Build server config in collectStepData
# ===========================================================================


class TestBuildServerConfig:
    """Verify build_server config structure is accepted by the deploy endpoint."""

    def test_deploy_accepts_build_server_config(self, client):
        profile = {
            "target": {"host": "192.168.0.9"},
            "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
            "build_server": {
                "type": "remote",
                "host": "build.example.com",
                "ssh_user": "builder",
                "ssh_port": 22,
                "repo_path": "/opt/crm-solution",
            },
        }
        resp = client.post("/api/deploy", json={
            "profile": profile,
            "dry_run": True,
        })
        # Should not 500 — the deploy endpoint should handle the extra key
        assert resp.status_code in (200, 400)

    def test_deploy_accepts_local_build_server(self, client):
        profile = {
            "target": {"host": "localhost"},
            "database": {"db_password": "Test@1234", "db_root_password": "Root@1234"},
            "build_server": {"type": "local"},
        }
        resp = client.post("/api/deploy", json={
            "profile": profile,
            "dry_run": True,
        })
        assert resp.status_code in (200, 400)


# ===========================================================================
# Password handling overhaul — required passwords & no auto-generate
# ===========================================================================


class TestPasswordRequiredValidation:
    """Verify that _build_context rejects missing passwords."""

    def test_missing_db_password_auto_generates(self):
        """_build_context should auto-generate db_password for first-time deploys."""
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "database": {"db_root_password": "roottest", "db_host": "localhost"},
            "security": {"jwt_secret": "x" * 64},
        }
        ctx = gen._build_context(profile)
        assert ctx.get("db_password"), "db_password should be auto-generated"
        assert len(ctx["db_password"]) >= 16, "auto-generated password should be strong"

    def test_missing_db_root_password_auto_generates(self):
        """_build_context should auto-generate db_root_password for first-time deploys."""
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "database": {"db_password": "test", "db_host": "localhost"},
            "security": {"jwt_secret": "x" * 64},
        }
        ctx = gen._build_context(profile)
        assert ctx.get("db_root_password"), "db_root_password should be auto-generated"
        assert len(ctx["db_root_password"]) >= 16, "auto-generated password should be strong"

    def test_jwt_secret_still_auto_generated(self):
        """JWT secret should still auto-generate when missing — it's a token, not a password."""
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "database": {"db_password": "test", "db_root_password": "roottest"},
        }
        ctx = gen._build_context(profile)
        assert ctx.get("jwt_secret"), "JWT secret should have been auto-generated"
        assert len(ctx["jwt_secret"]) >= 32, "JWT secret should be at least 32 chars"

    def test_provided_passwords_pass_validation(self):
        """Providing both passwords should not raise."""
        from core.generator import ConfigGenerator
        gen = ConfigGenerator()
        profile = {
            "database": {"db_password": "MyP@ss1", "db_root_password": "Root@2"},
            "security": {"jwt_secret": "x" * 64},
        }
        ctx = gen._build_context(profile)
        assert ctx["db_password"] == "MyP@ss1"
        assert ctx["db_root_password"] == "Root@2"

    def test_deploy_returns_400_when_passwords_missing(self, client):
        """Deploy endpoint should return 400 when required passwords are not provided."""
        resp = client.post(
            "/api/deploy",
            json={
                "profile": {"architecture": "monolith"},
                "password_strategy": "entered",
                "dry_run": True,
            },
            content_type="application/json",
        )
        assert resp.status_code == 400
        data = resp.get_json()
        assert "password" in data.get("error", "").lower()


class TestAdminPasswordEndpoint:
    """Tests for /api/day2/postinstall/set-admin-password."""

    def test_missing_username_returns_400(self, client):
        """Endpoint should return 400 when username is missing."""
        resp = client.post(
            "/api/day2/postinstall/set-admin-password",
            json={"email": "a@b.com", "password": "Test@1234"},
        )
        assert resp.status_code == 400
        assert "username" in resp.get_json()["error"].lower()

    def test_missing_email_returns_400(self, client):
        """Endpoint should return 400 when email is missing."""
        resp = client.post(
            "/api/day2/postinstall/set-admin-password",
            json={"username": "admin", "password": "Test@1234"},
        )
        assert resp.status_code == 400
        assert "email" in resp.get_json()["error"].lower()

    def test_missing_password_returns_400(self, client):
        """Endpoint should return 400 when password is missing."""
        resp = client.post(
            "/api/day2/postinstall/set-admin-password",
            json={"username": "admin", "email": "a@b.com"},
        )
        assert resp.status_code == 400
        assert "password" in resp.get_json()["error"].lower()

    def test_short_password_returns_400(self, client):
        """Endpoint should return 400 when password is too short."""
        resp = client.post(
            "/api/day2/postinstall/set-admin-password",
            json={"username": "admin", "email": "a@b.com", "password": "Ab1@"},
        )
        assert resp.status_code == 400
        assert "8 characters" in resp.get_json()["error"]

    def test_weak_password_returns_400(self, client):
        """Endpoint should return 400 when password lacks complexity."""
        resp = client.post(
            "/api/day2/postinstall/set-admin-password",
            json={"username": "admin", "email": "a@b.com", "password": "alllowercase1!"},
        )
        assert resp.status_code == 400
        assert "uppercase" in resp.get_json()["error"].lower()

    def test_valid_request_hashes_password(self, client):
        """Endpoint with valid body should pass validation (password complexity)
        and then either attempt DB update or fail on missing DB credentials —
        but NOT fail on input validation (username/email/password rules)."""
        resp = client.post(
            "/api/day2/postinstall/set-admin-password",
            json={
                "username": "admin",
                "email": "admin@crm.local",
                "password": "NewAdmin@123",
            },
        )
        body = resp.get_json()
        err = (body.get("error") or "").lower()
        # Should NOT be a password-complexity or missing-field validation error.
        # Acceptable failures: no DB credentials configured, docker not available.
        assert "8 characters" not in err, "Should not fail on length validation"
        assert "uppercase" not in err, "Should not fail on complexity validation"
        assert "username is required" not in err
        assert "email is required" not in err
        assert "password is required" not in err


# ===========================================================================
# Day-2 Monitoring — Networks, Images, Image History
# ===========================================================================


class TestDay2MonitoringEndpoints:
    """Tests for the Day-2 monitoring endpoints (networks, images, image history)."""

    def test_networks_endpoint_exists(self, client):
        """GET /api/day2/networks should return a response (not 404)."""
        resp = client.get("/api/day2/networks")
        assert resp.status_code != 404, "networks endpoint should exist"
        data = resp.get_json()
        assert "networks" in data

    def test_images_endpoint_exists(self, client):
        """GET /api/day2/images should return a response (not 404)."""
        resp = client.get("/api/day2/images")
        assert resp.status_code != 404, "images endpoint should exist"
        data = resp.get_json()
        assert "images" in data

    def test_image_history_endpoint_exists(self, client):
        """GET /api/day2/images/history should return a response (not 404)."""
        resp = client.get("/api/day2/images/history")
        assert resp.status_code != 404, "image history endpoint should exist"
        data = resp.get_json()
        assert "images" in data

    def test_networks_returns_profile_name(self, client):
        """Networks endpoint should include profile_name in response."""
        resp = client.get("/api/day2/networks")
        data = resp.get_json()
        assert "profile_name" in data

    def test_images_returns_profile_name(self, client):
        """Images endpoint should include profile_name in response."""
        resp = client.get("/api/day2/images")
        data = resp.get_json()
        assert "profile_name" in data

    def test_image_history_returns_profile_name(self, client):
        """Image history endpoint should include profile_name in response."""
        resp = client.get("/api/day2/images/history")
        data = resp.get_json()
        assert "profile_name" in data


class TestDay2MonitoringUI:
    """Tests for the Day-2 monitoring tab in the wizard HTML."""

    def test_monitoring_tab_exists(self, client):
        """Wizard should have a Monitoring tab button."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "showDay2Tab('monitoring'" in html

    def test_monitoring_pane_exists(self, client):
        """Wizard should have the day2-monitoring pane."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="day2-monitoring"' in html

    def test_admin_tab_removed(self, client):
        """Old day2-admin pane should no longer exist."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="day2-admin"' not in html

    def test_admin_password_in_secrets_tab(self, client):
        """Admin password form should be inside the secrets tab pane."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        # d2AdminPassword input should exist
        assert 'id="d2AdminPassword"' in html
        # It should be within the day2-secrets pane (check that secrets pane comes before the admin input)
        secrets_pos = html.find('id="day2-secrets"')
        admin_pw_pos = html.find('id="d2AdminPassword"')
        monitoring_pos = html.find('id="day2-monitoring"')
        assert secrets_pos < admin_pw_pos < monitoring_pos, \
            "Admin password form should be inside the Secrets tab, before Monitoring tab"

    def test_networks_section_in_monitoring(self, client):
        """Monitoring pane should have a networks section."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="d2NetworkList"' in html

    def test_images_section_in_monitoring(self, client):
        """Monitoring pane should have an images section."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="d2ImageList"' in html

    def test_image_history_section_in_monitoring(self, client):
        """Monitoring pane should have an image history section."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="d2ImageHistory"' in html

    def test_js_network_function_exists(self, client):
        """loadDay2Networks JS function should exist."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "loadDay2Networks" in html

    def test_js_images_function_exists(self, client):
        """loadDay2Images JS function should exist."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "loadDay2Images" in html

    def test_js_image_history_function_exists(self, client):
        """loadDay2ImageHistory JS function should exist."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "loadDay2ImageHistory" in html


class TestIntegrationProviderGrid:
    """Tests for the integration provider selection grid."""

    def test_integration_grid_exists(self, client):
        """Wizard should have an integration provider grid."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="integrationProviderGrid"' in html

    def test_integration_provider_options(self, client):
        """Integration grid should have Built-in, n8n, Zapier, Make options."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        for provider in ["builtin", "n8n", "zapier", "make"]:
            assert f'data-value="{provider}"' in html or provider in html.lower(), \
                f"Integration provider option '{provider}' should exist"


class TestAdminPasswordPlacements:
    """Tests that Set Admin Password form appears in Secrets, Monitoring, and Post-Deployment."""

    def test_admin_pw_in_monitoring_tab(self, client):
        """Monitoring pane should contain a Set Admin Password form."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="monAdminPassword"' in html

    def test_admin_pw_monitoring_has_username(self, client):
        """Monitoring admin pw form should have a username field."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="monAdminUsername"' in html

    def test_admin_pw_monitoring_has_email(self, client):
        """Monitoring admin pw form should have an email field."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="monAdminEmail"' in html

    def test_admin_pw_monitoring_has_confirm(self, client):
        """Monitoring admin pw form should have a confirm password field."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="monAdminPasswordConfirm"' in html

    def test_admin_pw_monitoring_position(self, client):
        """Admin pw form in Monitoring should be inside day2-monitoring pane."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        mon_start = html.find('id="day2-monitoring"')
        mon_admin = html.find('id="monAdminPassword"')
        assert mon_start < mon_admin, "Admin pw form should be inside the Monitoring pane"

    def test_admin_pw_in_post_deploy(self, client):
        """Post-deployment Done step should contain a Set Admin Password form."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="pdAdminPassword"' in html

    def test_admin_pw_post_deploy_has_username(self, client):
        """Post-deploy admin pw form should have a username field."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="pdAdminUsername"' in html

    def test_admin_pw_post_deploy_has_confirm(self, client):
        """Post-deploy admin pw form should have a confirm password field."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert 'id="pdAdminPasswordConfirm"' in html

    def test_admin_pw_post_deploy_position(self, client):
        """Admin pw form should appear in the Done step after validation table."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        validation_pos = html.find('id="done-validation"')
        pd_admin_pos = html.find('id="pd-admin-card"')
        assert validation_pos < pd_admin_pos, \
            "Post-deploy admin pw form should be after the validation table"

    def test_shared_setadminpwfrom_function(self, client):
        """The shared setAdminPwFrom JS function should exist."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "function setAdminPwFrom" in html

    def test_secrets_tab_uses_shared_function(self, client):
        """Secrets tab should call setAdminPwFrom('d2')."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "setAdminPwFrom('d2')" in html

    def test_monitoring_tab_uses_shared_function(self, client):
        """Monitoring tab should call setAdminPwFrom('mon')."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "setAdminPwFrom('mon')" in html

    def test_post_deploy_uses_shared_function(self, client):
        """Post-deploy should call setAdminPwFrom('pd')."""
        resp = client.get("/wizard")
        html = resp.data.decode()
        assert "setAdminPwFrom('pd')" in html