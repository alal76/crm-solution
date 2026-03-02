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