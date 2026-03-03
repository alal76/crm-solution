"""
tests/conftest.py — Shared fixtures and configuration for CDT test suite.

Responsibilities:
  1. Add tool root to sys.path so every test module can import CDT packages
     without its own sys.path.insert hack.
  2. Register custom pytest markers (unit, integration, ui).
  3. Inject CDT version and test metadata into the HTML report header.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

# ---------------------------------------------------------------------------
# 1.  sys.path — make CDT packages importable from any test file
# ---------------------------------------------------------------------------
_TOOL_ROOT = Path(__file__).resolve().parent.parent
if str(_TOOL_ROOT) not in sys.path:
    sys.path.insert(0, str(_TOOL_ROOT))

# ---------------------------------------------------------------------------
# 2.  HTML report metadata (pytest-html / pytest-metadata)
# ---------------------------------------------------------------------------
_VERSION_FILE = _TOOL_ROOT / "cdt_versions.json"


def _load_cdt_version() -> str:
    """Return the current CDT version string, or 'unknown'."""
    try:
        data = json.loads(_VERSION_FILE.read_text())
        return data.get("current", "unknown")
    except Exception:
        return "unknown"


def pytest_configure(config):
    """Register custom markers and set report title metadata."""
    config.addinivalue_line("markers", "unit: Pure unit tests (mocked dependencies)")
    config.addinivalue_line("markers", "integration: Integration tests (Flask client, temp files)")
    config.addinivalue_line("markers", "ui: UI/template validation tests")


def pytest_html_report_title(report):
    """Set the HTML report page title."""
    report.title = "CDT — Test Report"


def pytest_metadata(metadata):
    """Add CDT-specific metadata to the HTML report Environment table."""
    metadata["CDT Version"] = _load_cdt_version()
    metadata["Tool Root"] = str(_TOOL_ROOT)
    # Remove noisy default keys that aren't useful
    for key in ("JAVA_HOME", "Plugins"):
        metadata.pop(key, None)
