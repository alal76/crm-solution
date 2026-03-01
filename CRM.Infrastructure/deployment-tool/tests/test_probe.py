#!/usr/bin/env python3
"""
Tests for EnvironmentProbe and ComponentDetector.
Uses unittest.mock to avoid real network/subprocess calls.
"""

from __future__ import annotations

import sys
from pathlib import Path

# Ensure tool root is on sys.path
sys.path.insert(0, str(Path(__file__).parent.parent))

import pytest
from unittest.mock import MagicMock, patch

from core.probe import (
    CheckResult,
    CheckStatus,
    EnvironmentProbe,
    ProbeResult,
    ProbeTarget,
)
from core.detector import ComponentAction, ComponentDetector, ComponentStatus


# ===========================================================================
# EnvironmentProbe — docker check
# ===========================================================================

class TestCheckLocalDocker:

    def test_check_local_docker_pass(self):
        """docker info returns rc=0 → PASS."""
        mock_result = MagicMock()
        mock_result.returncode = 0
        with patch("subprocess.run", return_value=mock_result):
            result = EnvironmentProbe().check_local_docker()
        assert result.status == CheckStatus.PASS, f"Expected PASS, got {result.status}"

    def test_check_local_docker_fail(self):
        """docker info returns rc=1 → FAIL."""
        mock_result = MagicMock()
        mock_result.returncode = 1
        mock_result.stderr = b"Cannot connect to Docker daemon"
        with patch("subprocess.run", return_value=mock_result):
            result = EnvironmentProbe().check_local_docker()
        assert result.status == CheckStatus.FAIL, f"Expected FAIL, got {result.status}"


# ===========================================================================
# EnvironmentProbe — disk space
# ===========================================================================

class TestCheckDiskSpace:

    def _usage(self, free_bytes: int):
        """Return a shutil.disk_usage-like namedtuple mock."""
        m = MagicMock()
        m.free = free_bytes
        return m

    @patch("shutil.disk_usage")
    def test_check_disk_space_pass(self, mock_du):
        """50 GB free → PASS."""
        mock_du.return_value = self._usage(50 * (1024 ** 3))
        result = EnvironmentProbe().check_disk_space(min_gb=20)
        assert result.status == CheckStatus.PASS
        assert "50.0 GB" in result.detail

    @patch("shutil.disk_usage")
    def test_check_disk_space_warn(self, mock_du):
        """12 GB free (< 20 but >= min_gb*0.5=10) → WARN."""
        mock_du.return_value = self._usage(12 * (1024 ** 3))
        result = EnvironmentProbe().check_disk_space(min_gb=20)
        assert result.status == CheckStatus.WARN
        assert "12.0 GB" in result.detail

    @patch("shutil.disk_usage")
    def test_check_disk_space_fail(self, mock_du):
        """2 GB free → FAIL."""
        mock_du.return_value = self._usage(2 * (1024 ** 3))
        result = EnvironmentProbe().check_disk_space(min_gb=20)
        assert result.status == CheckStatus.FAIL
        assert "2.0 GB" in result.detail


# ===========================================================================
# EnvironmentProbe — port availability
# ===========================================================================

class TestCheckPortAvailable:

    def test_check_port_occupied_returns_warn(self):
        """Binding fails (port in use) → WARN."""
        import socket as _socket

        # Simulate OSError raised when binding a port already in use
        with patch("socket.socket") as mock_sock_cls:
            instance = MagicMock()
            instance.__enter__ = MagicMock(return_value=instance)
            instance.__exit__ = MagicMock(return_value=False)
            instance.bind.side_effect = OSError("address already in use")
            mock_sock_cls.return_value = instance

            result = EnvironmentProbe().check_port_available(9999)

        assert result.status == CheckStatus.WARN
        assert "9999" in result.detail


# ===========================================================================
# ProbeResult — overall status derivation
# ===========================================================================

class TestProbeResultOverall:

    def test_probe_result_overall_fail(self):
        """Any FAIL check → overall = FAIL."""
        checks = [
            CheckResult("A", CheckStatus.PASS),
            CheckResult("B", CheckStatus.FAIL, "some error"),
            CheckResult("C", CheckStatus.WARN),
        ]
        result = ProbeResult.from_checks(checks)
        assert result.overall == CheckStatus.FAIL
        assert result.failed_count == 1
        assert result.passed_count == 1
        assert result.warned_count == 1

    def test_probe_result_overall_warn(self):
        """Only WARN checks (no FAIL) → overall = WARN."""
        checks = [
            CheckResult("X", CheckStatus.WARN, "minor issue"),
            CheckResult("Y", CheckStatus.WARN, "another warning"),
        ]
        result = ProbeResult.from_checks(checks)
        assert result.overall == CheckStatus.WARN
        assert result.failed_count == 0
        assert result.warned_count == 2

    def test_probe_result_overall_pass(self):
        """All PASS → overall = PASS."""
        checks = [
            CheckResult("A", CheckStatus.PASS),
            CheckResult("B", CheckStatus.PASS),
        ]
        result = ProbeResult.from_checks(checks)
        assert result.overall == CheckStatus.PASS

    def test_probe_result_to_dict(self):
        """to_dict returns serialisable structure."""
        checks = [CheckResult("A", CheckStatus.PASS, "detail", "hint")]
        result = ProbeResult.from_checks(checks)
        d = result.to_dict()
        assert d["overall"] == "pass"
        assert d["passed"] == 1
        assert len(d["checks"]) == 1
        assert d["checks"][0]["name"] == "A"
        assert d["checks"][0]["status"] == "pass"


# ===========================================================================
# ComponentDetector — Meilisearch
# ===========================================================================

class TestDetectMeilisearch:

    def test_detect_meilisearch_found(self):
        """HTTP 200 + {"status":"available"} → detected=True, running=True."""
        import io
        import http.client

        fake_body = b'{"status":"available"}'

        mock_resp = MagicMock()
        mock_resp.read.return_value = fake_body
        mock_resp.headers = {"X-Meilisearch-Version": "1.6.0"}
        mock_resp.__enter__ = MagicMock(return_value=mock_resp)
        mock_resp.__exit__ = MagicMock(return_value=False)

        with patch("urllib.request.urlopen", return_value=mock_resp):
            cs = ComponentDetector().detect_meilisearch()

        assert cs.detected is True
        assert cs.running is True

    def test_detect_meilisearch_not_found(self):
        """Connection refused → detected=False."""
        with patch("urllib.request.urlopen", side_effect=ConnectionRefusedError()):
            cs = ComponentDetector().detect_meilisearch()
        assert cs.detected is False
        assert cs.running is False


# ===========================================================================
# ComponentDetector — detect_all
# ===========================================================================

class TestDetectAll:

    def test_detect_all_returns_all_components(self):
        """detect_all should return at least 8 ComponentStatus objects."""
        # Patch all network operations to avoid real calls
        with (
            patch.object(ComponentDetector, "detect_tcp", return_value=False),
            patch("urllib.request.urlopen", side_effect=OSError()),
            patch("subprocess.run", side_effect=FileNotFoundError()),
        ):
            results = ComponentDetector().detect_all("localhost")

        assert isinstance(results, list)
        assert len(results) >= 8, f"Expected ≥8 components, got {len(results)}"


# ===========================================================================
# ComponentDetector — set_action
# ===========================================================================

class TestSetAction:

    def test_set_action_deploy_new(self):
        """Not detected → DEPLOY_NEW."""
        cs = ComponentStatus(name="Test", key="test", detected=False, running=False)
        result = ComponentDetector().set_action(cs)
        assert result.action == ComponentAction.DEPLOY_NEW

    def test_set_action_reuse(self):
        """Detected + running, no upgrade → REUSE."""
        cs = ComponentStatus(
            name="Test", key="test",
            detected=True, running=True, upgrade_available=False,
        )
        result = ComponentDetector().set_action(cs)
        assert result.action == ComponentAction.REUSE

    def test_set_action_replace_not_running(self):
        """Detected but not running → REPLACE."""
        cs = ComponentStatus(
            name="Test", key="test",
            detected=True, running=False, upgrade_available=False,
        )
        result = ComponentDetector().set_action(cs)
        assert result.action == ComponentAction.REPLACE

    def test_set_action_upgrade(self):
        """Detected + upgrade_available → UPGRADE."""
        cs = ComponentStatus(
            name="Test", key="test",
            detected=True, running=True, upgrade_available=True,
        )
        result = ComponentDetector().set_action(cs)
        assert result.action == ComponentAction.UPGRADE


# ===========================================================================
# ComponentStatus serialization
# ===========================================================================

class TestComponentStatusSerialization:

    def test_to_dict_is_serializable(self):
        """to_dict should return only JSON-compatible types."""
        import json
        cs = ComponentStatus(
            name="Redis", key="redis", detected=True, running=True,
            version="7.0.1", port=6379,
        )
        cs.action = ComponentAction.REUSE
        d = cs.to_dict()
        # Must not raise
        serialized = json.dumps(d)
        assert '"redis"' in serialized
        assert '"reuse"' in serialized
