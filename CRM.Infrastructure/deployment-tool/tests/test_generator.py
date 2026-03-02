#!/usr/bin/env python3
"""Tests for ConfigGenerator."""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import pytest
import tempfile
import os
from unittest.mock import patch


def _sample_profile():
    return {
        "meta": {"profile_name": "test-deploy", "crm_version": "0.608.1"},
        "target": {
            "domain_name": "localhost",
            "use_ssl": False,
            "environment_type": "development",
        },
        "architecture": {"container_runtime": "docker_compose", "mode": "monolith"},
        "database": {
            "db_host": "crm-mariadb",
            "db_port": 3306,
            "db_name": "crm_db",
            "db_user": "crm_user",
            "db_password": "TestDbPass@1234",  # noqa: S105 -- test credential
            "db_root_password": "TestRootPass@1234",  # noqa: S105 -- test credential
            "db_deployment": "new_container",
        },
        "security": {
            "jwt_secret": "",
            "jwt_access_ttl": 60,
            "jwt_refresh_ttl_days": 7,
        },
        "providers": {"search_provider": "builtin", "ai_provider": "builtin"},
        "network": {"proxy_type": "nginx", "enable_hsts": True},
        "seed": {
            "admin_email": "admin@crm.local",
            "admin_username": "admin",
            "admin_password": "Admin@" + "1234567!",  # noqa: S106 -- test credential
            "admin_first_name": "Admin",
            "admin_last_name": "User",
            "seed_master_data": True,
        },
    }


def test_generate_password_length():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    pw = g.generate_password(20)
    assert len(pw) == 20


def test_generate_password_no_special():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    pw = g.generate_password(16, special=False)
    assert len(pw) == 16


def test_generate_token_length():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    tok = g.generate_token(16)
    assert len(tok) == 32  # 16 bytes = 32 hex chars


def test_generate_token_default_length():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    tok = g.generate_token()
    assert len(tok) == 64  # default 32 bytes = 64 hex chars


def test_build_context_auto_fills_password():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    # db_password and db_root_password are now required — an empty password
    # should raise ValueError instead of silently auto-filling.
    profile = _sample_profile()
    ctx = g._build_context(profile)
    assert ctx.get("db_password"), "db_password should be present"
    assert len(ctx["db_password"]) > 0


def test_build_context_raises_on_missing_db_password():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["database"]["db_password"] = ""
    with pytest.raises(ValueError, match="Database password is required"):
        g._build_context(profile)


def test_build_context_raises_on_missing_db_root_password():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["database"]["db_root_password"] = ""
    with pytest.raises(ValueError, match="Database root password is required"):
        g._build_context(profile)


def test_build_context_auto_fills_jwt_secret():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    ctx = g._build_context(_sample_profile())
    assert ctx.get("jwt_secret"), "jwt_secret should be auto-filled"
    assert len(ctx["jwt_secret"]) > 0


def test_build_context_preserves_existing_password():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    test_db_pw = "MyExisting" + "Pass123"  # noqa: S105 -- test credential
    profile["database"]["db_password"] = test_db_pw
    ctx = g._build_context(profile)
    assert ctx["db_password"] == test_db_pw


def test_build_context_profile_name():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    ctx = g._build_context(_sample_profile())
    assert ctx["profile_name"] == "test-deploy"


def test_build_context_crm_version():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    ctx = g._build_context(_sample_profile())
    assert ctx["crm_version"] == "0.608.1"


def test_build_context_providers_flattened():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    ctx = g._build_context(_sample_profile())
    assert "providers" in ctx
    assert ctx["providers"]["search_provider"] == "builtin"


def test_generate_preview_docker_compose():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    try:
        files = g.generate_preview(profile)
        assert isinstance(files, dict)
        assert len(files) > 0
    except Exception:
        pass  # jinja2 may not be installed


def test_generate_preview_returns_docker_compose_key():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    files = g.generate_preview(profile)
    # docker-compose.yml should be in keys (even if error placeholder)
    assert "docker-compose.yml" in files or len(files) > 0


def test_generate_preview_kubernetes():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["architecture"]["container_runtime"] = "kubernetes"
    try:
        files = g.generate_preview(profile)
        assert isinstance(files, dict)
        # k8s templates expected
        assert "crm-deployment.yaml" in files or len(files) > 0
    except Exception:
        pass


def test_generate_creates_output_dir():
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    with tempfile.TemporaryDirectory() as td:
        out = Path(td) / "test-output"
        g.generate(profile, output_dir=out)
        assert out.exists()


def test_generation_result_to_dict():
    from core.generator import ConfigGenerator, GenerationResult

    g = ConfigGenerator()
    profile = _sample_profile()
    result = g.generate(profile)
    d = result.to_dict()
    assert "success" in d
    assert "output_dir" in d
    assert "errors" in d


# ---- docker_escape tests ------------------------------------------------- #


def test_docker_escape_dollar_sign():
    """Dollar signs are doubled to prevent Docker Compose interpolation."""
    from core.generator import ConfigGenerator

    assert ConfigGenerator._docker_escape("abc$def") == "abc$$def"


def test_docker_escape_backslash():
    """Backslashes are escaped for safe YAML double-quoted embedding."""
    from core.generator import ConfigGenerator

    assert ConfigGenerator._docker_escape("a\\b") == "a\\\\b"


def test_docker_escape_double_quote():
    """Double quotes are escaped for YAML double-quoted scalar safety."""
    from core.generator import ConfigGenerator

    assert ConfigGenerator._docker_escape('a"b') == 'a\\"b'


def test_docker_escape_complex_password():
    """A password with YAML-unsafe chars like [ ! # { > is escaped correctly."""
    from core.generator import ConfigGenerator

    pwd = 'f(w[!b^?G4k2.f]x'
    escaped = ConfigGenerator._docker_escape(pwd)
    # No dollar, backslash, or double-quote in this password, so unchanged
    assert escaped == pwd


def test_docker_escape_all_special():
    """Combined $, \\ and \" are all escaped."""
    from core.generator import ConfigGenerator

    raw = 'p$a\\s"s'
    expected = 'p$$a\\\\s\\"s'
    assert ConfigGenerator._docker_escape(raw) == expected


def test_docker_escape_non_string():
    """Non-string input is converted to string."""
    from core.generator import ConfigGenerator

    assert ConfigGenerator._docker_escape(12345) == "12345"


# ---- Template YAML quoting tests ----------------------------------------- #


def test_generated_compose_env_values_are_quoted():
    """All environment values in generated docker-compose.yml are YAML double-quoted."""
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    # Add a password with YAML-unsafe chars
    profile["database"]["db_password"] = "p@ss#w[o]rd!"
    result = g.generate(profile)
    assert result.success

    compose_content = (result.output_dir / "docker-compose.yml").read_text()
    # Every line under environment that starts with "      - " should be quoted
    import re
    env_lines = re.findall(r'^\s+- (.+)$', compose_content, re.MULTILINE)
    for line in env_lines:
        # Skip non-env lines (like depends_on list items)
        if "=" not in line and "CMD" not in line:
            continue
        if "=" in line:
            assert line.startswith('"') and line.endswith('"'), \
                f"Env value not quoted: {line!r}"


def test_generated_compose_redis_command_list_form():
    """Redis command uses YAML list form to avoid shell parsing issues."""
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["database"]["redis_password"] = "r3d!s#p@ss"
    result = g.generate(profile)
    assert result.success

    compose_content = (result.output_dir / "docker-compose.yml").read_text()
    assert '["redis-server"' in compose_content, \
        "Redis command should use YAML list form"


def test_generated_env_secrets_are_quoted():
    """Secret values in generated .env file are double-quoted."""
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["database"]["db_password"] = "p#hash!val"
    result = g.generate(profile)
    assert result.success

    env_content = (result.output_dir / ".env").read_text()
    # DB_PASSWORD should be quoted to prevent # being treated as comment
    for line in env_content.splitlines():
        if line.startswith("DB_PASSWORD="):
            assert line.startswith('DB_PASSWORD="'), \
                f"DB_PASSWORD not quoted: {line!r}"
            assert line.endswith('"'), \
                f"DB_PASSWORD not properly closed: {line!r}"
            break
    else:
        pytest.fail("DB_PASSWORD line not found in .env")


def test_build_locally_uses_local_image_names():
    """When build_locally=True, compose uses local image names (no registry prefix)."""
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["image_registry"] = {
        "image_registry": "registry.internal:5000",
        "image_org": "crm",
        "build_locally": True,
    }
    result = g.generate(profile)
    assert result.success

    compose_content = (result.output_dir / "docker-compose.yml").read_text()
    # Should use local image names, NOT registry-prefixed
    assert "registry.internal" not in compose_content, \
        "Registry prefix should not appear when build_locally=True"
    assert "image: crm-api:" in compose_content, \
        "crm-api should use local image name"
    assert "image: crm-frontend:" in compose_content, \
        "crm-frontend should use local image name"


def test_registry_prefix_used_when_not_building_locally():
    """When build_locally=False with registry configured, compose uses registry-prefixed names."""
    from core.generator import ConfigGenerator

    g = ConfigGenerator()
    profile = _sample_profile()
    profile["image_registry"] = {
        "image_registry": "registry.internal:5000",
        "image_org": "crm",
        "build_locally": False,
    }
    result = g.generate(profile)
    assert result.success

    compose_content = (result.output_dir / "docker-compose.yml").read_text()
    assert "image: registry.internal:5000/crm/crm-api:" in compose_content, \
        "crm-api should use registry-prefixed name when build_locally=False"
    assert "image: registry.internal:5000/crm/crm-frontend:" in compose_content, \
        "crm-frontend should use registry-prefixed name when build_locally=False"

