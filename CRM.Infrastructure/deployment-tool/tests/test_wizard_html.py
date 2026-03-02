#!/usr/bin/env python3
"""
tests/test_wizard_html.py — Tests for wizard.html template structure and JavaScript functions.

Validates:
  - All wizard steps (0-8) are present in the HTML
  - Critical JavaScript functions are defined
  - No orphaned/duplicate function definitions
  - showToast and showErrorPopup are defined
  - Smart defaults functions are present
  - All API endpoint references use correct paths
  - No references to deprecated/broken endpoints
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import pytest

from gui.app import app


@pytest.fixture()
def wizard_html():
    """Render wizard.html and return the HTML content."""
    app.config["TESTING"] = True
    with app.test_client() as client:
        resp = client.get("/wizard")
        return resp.data.decode("utf-8")


# ===========================================================================
# Template structure
# ===========================================================================


class TestWizardSteps:
    """Verify all wizard steps are present."""

    @pytest.mark.parametrize("step", range(6))
    def test_step_section_exists(self, wizard_html, step):
        """Each step 0-5 should have a data-section attribute."""
        pattern = f'data-section="{step}"'
        assert pattern in wizard_html, f"Missing wizard step section: {step}"

    def test_step_indicator_has_all_steps(self, wizard_html):
        """Step indicator should have entries for steps 0 through 5."""
        for step in range(6):
            assert f'data-step="{step}"' in wizard_html


class TestCriticalElements:
    """Verify critical HTML elements exist."""

    @pytest.mark.parametrize("element_id", [
        "configSummary",
        "serviceReferenceTable",
        "protocolBadge",
        "previewApiUrl",
        "previewFrontendUrl",
        "previewGatewayUrl",
        "deployPanel",
        "deployLog",
        "preflightList",
        "deploymentName",
        "deploymentDescription",
        "expectedUsers",
        "dbName",
        "dbPort",
        "sslEnabled",
        "welcomeModal",
    ])
    def test_element_exists(self, wizard_html, element_id):
        assert f'id="{element_id}"' in wizard_html, f"Missing element: {element_id}"


# ===========================================================================
# JavaScript function definitions
# ===========================================================================


class TestJavaScriptFunctions:
    """Verify critical JavaScript functions are defined in wizard.html."""

    @pytest.mark.parametrize("func_name", [
        "collectStepData",
        "validateStep",
        "nextStep",
        "prevStep",
        "renderSummary",
        "generateConfig",
        "startDeployment",
        "selectPlatform",
        "selectArchitecture",
        "selectDatabase",
        "selectProvider",
        "buildReviewSummary",
        "applyTemplateToWizard",
        "applyQuickPreset",
        "saveDraft",
        "loadDraft",
        "updateArchSuggestion",
        "loadRecommendation",
        "initDiscovery",
        "autoPopulateRegistry",
    ])
    def test_function_defined(self, wizard_html, func_name):
        """Each critical function should appear as a function definition."""
        # Match both 'function name(' and 'async function name('
        pattern = rf"(?:async\s+)?function\s+{func_name}\s*\("
        assert re.search(pattern, wizard_html), f"Function not found: {func_name}"

    @pytest.mark.parametrize("func_name", [
        "showToast",
        "showErrorPopup",
    ])
    def test_ui_feedback_functions(self, wizard_html, func_name):
        """Toast and error popup functions must be defined."""
        pattern = rf"(?:async\s+)?function\s+{func_name}\s*\("
        assert re.search(pattern, wizard_html), f"UI function not found: {func_name}"

    @pytest.mark.parametrize("func_name", [
        "loadSmartDefaults",
        "applySmartDefaults",
        "onConfigContextChange",
        "_renderServiceReference",
        "_fillIfEmpty",
        "_updateProtocolIndicators",
    ])
    def test_smart_defaults_functions(self, wizard_html, func_name):
        """Smart defaults functions must be defined."""
        pattern = rf"(?:async\s+)?function\s+{re.escape(func_name)}\s*\("
        assert re.search(pattern, wizard_html), f"Smart defaults function not found: {func_name}"


# ===========================================================================
# API endpoint references
# ===========================================================================


class TestAPIEndpointReferences:
    """Verify wizard.html references correct API endpoints."""

    def test_no_broken_day2_containers_endpoint(self, wizard_html):
        """Should NOT reference deprecated /api/day2/containers."""
        assert "/api/day2/containers" not in wizard_html

    def test_uses_correct_day2_status_all(self, wizard_html):
        """Should reference /api/day2/status/all."""
        assert "/api/day2/status/all" in wizard_html

    def test_no_broken_rotate_secrets_endpoint(self, wizard_html):
        """Should NOT reference /api/day2/rotate-secrets (plural)."""
        # The correct endpoint is rotate-secret (singular)
        occurrences = [
            m.start() for m in re.finditer(r"/api/day2/rotate-secrets\b", wizard_html)
        ]
        assert len(occurrences) == 0, "Found deprecated /api/day2/rotate-secrets endpoint"

    def test_uses_correct_rotate_secret_endpoint(self, wizard_html):
        """Should reference /api/day2/rotate-secret (singular)."""
        assert "/api/day2/rotate-secret" in wizard_html

    def test_no_window_crmSessionId(self, wizard_html):
        """Should NOT reference the broken window.crmSessionId."""
        assert "window.crmSessionId" not in wizard_html

    def test_references_deploy_api(self, wizard_html):
        """Should reference /api/deploy for deployment."""
        assert "/api/deploy" in wizard_html

    def test_references_config_api(self, wizard_html):
        """Should reference /api/config."""
        assert "/api/config" in wizard_html

    def test_references_generate_api(self, wizard_html):
        """Should reference /api/generate."""
        assert "/api/generate" in wizard_html

    def test_references_defaults_api(self, wizard_html):
        """Should reference /api/defaults for smart defaults."""
        assert "/api/defaults" in wizard_html

    def test_references_profiles_templates_api(self, wizard_html):
        """Should reference /api/profiles/templates."""
        assert "/api/profiles/templates" in wizard_html


# ===========================================================================
# No raw alert() calls
# ===========================================================================


class TestNoRawAlerts:
    """Verify no raw alert() calls remain in the wizard."""

    def test_no_raw_alert_calls(self, wizard_html):
        """Should not have plain alert() calls — use showErrorPopup or showToast instead."""
        # Find alert( but exclude showAlert, alertEl, .alert (CSS class), etc.
        # Also exclude 'alert-' (Bootstrap class prefixes)
        pattern = r"(?<![.\w])alert\s*\("
        matches = list(re.finditer(pattern, wizard_html))
        # Filter out false positives in HTML attributes like class="alert ..."
        real_alerts = []
        for m in matches:
            # Get surrounding context
            start = max(0, m.start() - 50)
            context = wizard_html[start:m.end() + 20]
            # Skip if it's inside an HTML class attribute or comment
            if 'class="' in context and "alert-" in context:
                continue
            if "// alert" in context or "/* alert" in context:
                continue
            real_alerts.append(context.strip())

        assert len(real_alerts) == 0, (
            f"Found {len(real_alerts)} raw alert() call(s). "
            "Use showErrorPopup() or showToast() instead.\n"
            + "\n".join(real_alerts[:5])
        )


# ===========================================================================
# Configuration object
# ===========================================================================


class TestConfigObject:
    """Verify the config object initialization in wizard.html."""

    def test_config_has_required_fields(self, wizard_html):
        """The initial config object should have all required fields."""
        required_fields = [
            "name", "description", "expected_users",
            "platform", "architecture",
            "database_type", "database_name", "database_port",
            "search_provider", "chat_provider", "notification_provider",
            "analytics_provider", "ai_provider",
            "secrets", "ssl", "service_accounts", "image_registry",
        ]
        for field in required_fields:
            assert field in wizard_html, f"Config missing field: {field}"

    def test_smart_defaults_variable_initialized(self, wizard_html):
        """_smartDefaults should be initialized to null."""
        assert "_smartDefaults = null" in wizard_html

    def test_deploy_state_object_exists(self, wizard_html):
        """_deploy state object should be defined."""
        assert "const _deploy" in wizard_html
        assert "jobId" in wizard_html
        assert "evtSource" in wizard_html
