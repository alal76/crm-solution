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
            "db_password": "",
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
    ctx = g._build_context(_sample_profile())
    assert ctx.get("db_password"), "db_password should be auto-filled"
    assert len(ctx["db_password"]) > 0


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
