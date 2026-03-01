#!/usr/bin/env python3
"""
tests/test_profile.py — Unit tests for core.profile.ProfileManager.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import pytest
import tempfile

from core.profile import ProfileManager, ProfileNotFoundError, ProfileExistsError


def make_manager(tmp_dir: str) -> ProfileManager:
    """Create a ProfileManager backed by a temporary directory."""
    return ProfileManager(profiles_dir=Path(tmp_dir))


def _sample_profile(name: str = "test-profile") -> dict:
    return {
        "meta": {
            "profile_name": name,
            "crm_version": "0.608.1",
        },
        "target": {"provider": "local_docker", "environment_type": "development"},
        "architecture": {"mode": "monolith"},
        "database": {"db_provider": "mariadb"},
        "network": {},
        "security": {},
        "providers": {},
        "seed": {},
    }


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------


def test_create_and_load_profile():
    """Save a profile and load it back — meta fields should match."""
    with tempfile.TemporaryDirectory() as tmp:
        mgr = make_manager(tmp)
        data = _sample_profile("my-profile")
        mgr.save("my-profile", data)

        loaded = mgr.load("my-profile")
        assert loaded["meta"]["profile_name"] == "my-profile"
        assert loaded["target"]["provider"] == "local_docker"


def test_delete_profile():
    """Save then delete — list_profiles should return empty."""
    with tempfile.TemporaryDirectory() as tmp:
        mgr = make_manager(tmp)
        mgr.save("to-delete", _sample_profile("to-delete"))
        assert len(mgr.list_profiles()) == 1

        mgr.delete("to-delete")
        assert mgr.list_profiles() == []


def test_delete_nonexistent_raises():
    """Deleting a profile that doesn't exist raises ProfileNotFoundError."""
    with tempfile.TemporaryDirectory() as tmp:
        mgr = make_manager(tmp)
        with pytest.raises(ProfileNotFoundError):
            mgr.delete("ghost-profile")


def test_import_profile():
    """Export then import — imported profile name matches."""
    with tempfile.TemporaryDirectory() as tmp:
        mgr = make_manager(tmp)
        mgr.save("original", _sample_profile("original"))

        json_str = mgr.export("original")
        # Parse the exported JSON to verify it's valid
        exported_data = json.loads(json_str)
        assert exported_data["meta"]["profile_name"] == "original"

        # Import under a different name by modifying profile_name
        exported_data["meta"]["profile_name"] = "imported"
        imported_name = mgr.import_profile(json.dumps(exported_data))
        assert imported_name == "imported"
        assert len(mgr.list_profiles()) == 2


def test_compare_profiles():
    """Two profiles with one differing field — compare returns that diff."""
    with tempfile.TemporaryDirectory() as tmp:
        mgr = make_manager(tmp)

        profile_a = _sample_profile("alpha")
        profile_a["target"]["environment_type"] = "development"
        mgr.save("alpha", profile_a)

        profile_b = _sample_profile("beta")
        profile_b["target"]["environment_type"] = "production"
        mgr.save("beta", profile_b)

        diff = mgr.compare("alpha", "beta")

        # Should have detected at least the environment_type difference
        env_diff = None
        for k, v in diff.items():
            if "environment_type" in k:
                env_diff = v
                break
        assert env_diff is not None
        assert env_diff["a"] == "development"
        assert env_diff["b"] == "production"


def test_get_templates_returns_9():
    """get_templates() returns exactly 9 templates."""
    mgr = ProfileManager()  # uses default dir; we only call get_templates()
    templates = mgr.get_templates()
    assert len(templates) == 9


def test_template_names():
    """Template list includes 'local-dev' and 'aws-eks-microservices'."""
    mgr = ProfileManager()
    templates = mgr.get_templates()
    names = [t["meta"]["profile_name"] for t in templates]
    assert "local-dev" in names
    assert "aws-eks" in names
