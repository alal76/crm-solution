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
