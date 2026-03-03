"""
tests/conftest.py — Shared fixtures and configuration for CDT test suite.

Responsibilities:
  1. Add tool root to sys.path so every test module can import CDT packages
     without its own sys.path.insert hack.
  2. Register custom pytest markers (unit, integration, ui).
  3. Inject CDT version and test metadata into the HTML report header.
  4. Add a "Module" column to the HTML results table for quick filtering.
"""

from __future__ import annotations

import json
import sys
from datetime import datetime, timezone
from pathlib import Path

import pytest

# ---------------------------------------------------------------------------
# 1.  sys.path — make CDT packages importable from any test file
# ---------------------------------------------------------------------------
_TOOL_ROOT = Path(__file__).resolve().parent.parent
if str(_TOOL_ROOT) not in sys.path:
    sys.path.insert(0, str(_TOOL_ROOT))

# ---------------------------------------------------------------------------
# 2.  Constants
# ---------------------------------------------------------------------------
_VERSION_FILE = _TOOL_ROOT / "cdt_versions.json"

# Map test file stems → friendly module names
_MODULE_MAP = {
    "test_flask_routes": "Flask Routes & API",
    "test_generator": "Config Generator",
    "test_probe": "Environment Probe",
    "test_profile": "Profile Manager",
    "test_vault": "Vault Manager",
    "test_wizard_html": "Wizard HTML/UI",
}

# Map test file stems → test category
_CATEGORY_MAP = {
    "test_flask_routes": "integration",
    "test_generator": "unit",
    "test_probe": "unit",
    "test_profile": "unit",
    "test_vault": "unit",
    "test_wizard_html": "ui",
}


def _load_cdt_version() -> str:
    """Return the current CDT version string, or 'unknown'."""
    try:
        data = json.loads(_VERSION_FILE.read_text())
        return data.get("current", "unknown")
    except Exception:
        return "unknown"


# ---------------------------------------------------------------------------
# 3.  Pytest hooks
# ---------------------------------------------------------------------------

def pytest_configure(config):
    """Register custom markers and set report title metadata."""
    config.addinivalue_line("markers", "unit: Pure unit tests (mocked dependencies)")
    config.addinivalue_line("markers", "integration: Integration tests (Flask client, temp files)")
    config.addinivalue_line("markers", "ui: UI/template validation tests")


def pytest_collection_modifyitems(items):
    """Auto-tag tests with markers based on their source file."""
    for item in items:
        stem = Path(item.fspath).stem
        cat = _CATEGORY_MAP.get(stem)
        if cat and not any(m.name == cat for m in item.iter_markers()):
            item.add_marker(getattr(pytest.mark, cat))


def pytest_html_report_title(report):
    """Set the HTML report page title."""
    report.title = "CDT — Test Report"


def pytest_metadata(metadata):
    """Add CDT-specific metadata to the HTML report Environment table."""
    metadata["CDT Version"] = _load_cdt_version()
    metadata["Tool Root"] = str(_TOOL_ROOT)
    metadata["Report Generated"] = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S UTC")
    # Remove noisy default keys that aren't useful
    for key in ("JAVA_HOME", "Plugins"):
        metadata.pop(key, None)


def pytest_html_results_table_header(cells):
    """Add Module and Category columns to the HTML results table."""
    cells.insert(1, '<th class="sortable" data-column-type="module">Module</th>')
    cells.insert(2, '<th class="sortable" data-column-type="category">Category</th>')


def pytest_html_results_table_row(report, cells):
    """Populate Module and Category columns for each test row."""
    stem = Path(report.fspath).stem if hasattr(report, "fspath") else ""
    module = _MODULE_MAP.get(stem, stem)
    category = _CATEGORY_MAP.get(stem, "other")

    cat_colours = {
        "unit": ("#d1fae5", "#065f46"),
        "integration": ("#e0e7ff", "#3730a3"),
        "ui": ("#fef3c7", "#92400e"),
    }
    bg, fg = cat_colours.get(category, ("#f3f4f6", "#374151"))

    cells.insert(1, f'<td>{module}</td>')
    cells.insert(
        2,
        f'<td><span style="background:{bg};color:{fg};padding:2px 8px;'
        f'border-radius:4px;font-size:0.82em;font-weight:600;text-transform:uppercase">'
        f'{category}</span></td>',
    )
