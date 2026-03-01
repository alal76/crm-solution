#!/usr/bin/env python3
"""
CRM CDT - Configuration Generator
Generates deployment configuration files from Jinja2 templates.
"""
from __future__ import annotations
import os
import re
import shutil
import tempfile
import secrets
import string
from dataclasses import dataclass, field
from pathlib import Path
from datetime import datetime, timezone
from typing import Any, Optional

try:
    from jinja2 import Environment, FileSystemLoader, StrictUndefined, TemplateNotFound
    _JINJA2_AVAILABLE = True
except ImportError:
    _JINJA2_AVAILABLE = False


@dataclass
class GeneratedFile:
    """Represents a single generated configuration file."""
    filename: str
    content: str
    path: Path
    executable: bool = False


@dataclass
class GenerationResult:
    """Result of a configuration generation run."""
    success: bool
    files: list[GeneratedFile] = field(default_factory=list)
    output_dir: Optional[Path] = None
    errors: list[str] = field(default_factory=list)
    warnings: list[str] = field(default_factory=list)

    def to_dict(self) -> dict:
        """Convert to serialisable dict."""
        return {
            "success": self.success,
            "output_dir": str(self.output_dir) if self.output_dir else None,
            "files": [
                {
                    "filename": f.filename,
                    "path": str(f.path),
                    "executable": f.executable,
                }
                for f in self.files
            ],
            "errors": self.errors,
            "warnings": self.warnings,
        }


class ConfigGenerator:
    """Generates deployment configuration files from Jinja2 templates."""

    # Maps (container_runtime) → [(template_name, output_filename)]
    _TEMPLATE_MAP: dict[str, list[tuple[str, str]]] = {
        "docker_compose": [
            ("docker-compose.j2", "docker-compose.yml"),
            ("appsettings.j2", "appsettings.Production.json"),
            ("nginx.conf.j2", "nginx.conf"),
            (".env.j2", ".env"),
        ],
        "kubernetes": [
            ("k8s-deployment.j2", "crm-deployment.yaml"),
            ("helm-values.j2", "values.yaml"),
            ("appsettings.j2", "appsettings.Production.json"),
        ],
    }

    def __init__(self, templates_dir: str = None):
        base = Path(__file__).parent.parent
        self.templates_dir = (
            Path(templates_dir) if templates_dir else base / "config-templates"
        )
        self.templates_dir.mkdir(exist_ok=True)

        if _JINJA2_AVAILABLE:
            self._env = Environment(
                loader=FileSystemLoader(str(self.templates_dir)),
                undefined=StrictUndefined,
                trim_blocks=True,
                lstrip_blocks=True,
            )
            self._env.globals.update(self._template_globals())
            # Also register docker_escape as a Jinja2 filter for | pipe usage
            self._env.filters["docker_escape"] = self._docker_escape
        else:
            self._env = None

    # ------------------------------------------------------------------
    # Helpers exposed as template globals
    # ------------------------------------------------------------------

    @staticmethod
    def _docker_escape(value: str) -> str:
        """Escape ``$`` as ``$$`` for Docker Compose variable interpolation."""
        if not isinstance(value, str):
            return str(value)
        return value.replace("$", "$$")

    def _template_globals(self) -> dict:
        return {
            "now": datetime.now(timezone.utc).isoformat(),
            "generate_password": self.generate_password,
            "generate_token": self.generate_token,
            "docker_escape": self._docker_escape,
        }

    @staticmethod
    def generate_password(length: int = 16, special: bool = True) -> str:
        """Return a cryptographically secure random password.

        Excludes ``$``, backtick, single-quote, double-quote and backslash
        so that generated values are safe inside Docker Compose env vars,
        shell scripts, and .env files without additional escaping.
        """
        alphabet = string.ascii_letters + string.digits
        if special:
            # Omit $, `, ', ", \ which break Docker Compose / shell interpolation
            alphabet += "!@#%^&*()-_=+[]{}|;:,.<>?"
        return "".join(secrets.choice(alphabet) for _ in range(length))

    @staticmethod
    def generate_token(length: int = 32) -> str:
        """Return a cryptographically secure hex token."""
        return secrets.token_hex(length)

    # ------------------------------------------------------------------
    # Context building
    # ------------------------------------------------------------------

    def _flatten_profile_sections(self, profile: dict, ctx: dict) -> None:
        """Flatten all wizard profile sections into the template context dict."""
        # Top-level sections — guard against scalar values sent by the wizard
        for section in ("target", "database", "network", "security", "architecture"):
            val = profile.get(section, {})
            if isinstance(val, dict):
                ctx.update(val)
            elif val is not None and val != "":
                ctx[section] = val

        # Simple dict sections merged directly
        for key in ("secrets", "ssl"):
            val = profile.get(key, {})
            if isinstance(val, dict):
                ctx.update(val)

        # Image registry (may be dict or plain string)
        registry = profile.get("image_registry", {})
        if isinstance(registry, dict):
            ctx.update(registry)
        elif registry and isinstance(registry, str):
            ctx["image_registry"] = registry

        # Service accounts
        service_accounts = profile.get("service_accounts", {})
        if isinstance(service_accounts, dict):
            ctx["service_accounts"] = service_accounts
            ctx.update(service_accounts)

        # Providers kept under their own key AND flattened for convenience
        providers = profile.get("providers", {})
        ctx["providers"] = providers
        if isinstance(providers, dict):
            ctx.update(providers)

        # Seed fields exposed with admin_ prefix (backward compat: also raw keys)
        seed = profile.get("seed", {})
        if isinstance(seed, dict):
            ctx.update({f"admin_{k}": v for k, v in seed.items()})
            ctx.update(seed)

        # Meta
        meta = profile.get("meta", {})
        ctx["profile_name"] = meta.get("profile_name", "crm")
        ctx["crm_version"] = meta.get("crm_version", "latest")

    def _apply_optional_defaults(self, ctx: dict) -> None:
        """Apply optional template variable defaults so StrictUndefined never raises."""
        defaults = {
            "redis_password": "",
            "meilisearch_master_key": "masterKey",
            "chatwoot_api_key": "",
            "chatwoot_secret_key": "",
            "chatwoot_account_id": "1",
            "novu_api_key": "",
            "novu_jwt_secret": self.generate_token(32),
            "superset_secret_key": "",
            "superset_admin_password": "",
            "docuseal_api_key": "",
            "docuseal_secret_key": "",
            "n8n_api_key": "",
            "n8n_username": "admin",
            "n8n_password": "",
            "openai_api_key": "",
            "openai_model": "gpt-4o",
            "anthropic_api_key": "",
            "azure_openai_endpoint": "",
            "azure_openai_api_key": "",
            "azure_openai_deployment": "gpt-4o",
            "ollama_model": "llama3.1:8b",
            "rate_limiting_enabled": "true",
            "is_development": False,
            "admin_email": "",
            "admin_password": "",
            "admin_username": "admin",
            "admin_admin_email": "admin@crm.local",
            "admin_admin_password": "Admin@123",  # NOSONAR - template placeholder default, rotated on first deploy
            "admin_admin_username": "admin",
            "api_port": "5000",
            "frontend_port": "80",
            "db_host": "crm-mariadb",
            "db_port": 3306,
            "db_name": "crm_db",
            "db_user": "crm_user",
            "db_version": "10.11",
            "domain_name": "localhost",
        }
        for key, default in defaults.items():
            ctx.setdefault(key, default)

    def _build_context(self, profile: dict) -> dict:
        """Flatten a wizard profile dict into a Jinja2 template context."""
        ctx: dict = {}
        self._flatten_profile_sections(profile, ctx)

        # Auto-fill missing required secrets
        if not ctx.get("db_password"):
            ctx["db_password"] = self.generate_password(20)
        if not ctx.get("jwt_secret"):
            ctx["jwt_secret"] = self.generate_token(32)
        if not ctx.get("db_root_password"):
            ctx["db_root_password"] = self.generate_password(24)

        # Default image registry — empty means local images (no registry prefix)
        ctx.setdefault("image_registry", "")
        ctx.setdefault("image_org", "")

        # SSL defaults
        ctx.setdefault("ssl_enabled", False)

        self._apply_optional_defaults(ctx)
        return ctx

    # ------------------------------------------------------------------
    # Rendering
    # ------------------------------------------------------------------

    def render_template(self, template_name: str, context: dict) -> str:
        """Render a Jinja2 template by name with the given context."""
        if not _JINJA2_AVAILABLE:
            raise RuntimeError(
                "Jinja2 is not installed. Run: pip install jinja2"
            )
        try:
            tmpl = self._env.get_template(template_name)
            return tmpl.render(**context)
        except TemplateNotFound:
            raise FileNotFoundError(
                f"Template '{template_name}' not found in {self.templates_dir}"
            )

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def generate(self, profile: dict, output_dir: Optional[Path] = None) -> GenerationResult:
        """Generate all configuration files for the given wizard profile."""

        # Default output goes to the persistent generated/ directory next to the CDT root
        if output_dir is None:
            base = Path(__file__).parent.parent / "generated"
            base.mkdir(parents=True, exist_ok=True)
            output_dir = base

        output_dir.mkdir(parents=True, exist_ok=True)

        result = GenerationResult(success=True, output_dir=output_dir)
        context = self._build_context(profile)

        # architecture may be a flat string (e.g. "docker_compose") or a nested dict
        arch = profile.get("architecture", {})
        if isinstance(arch, dict):
            container_runtime = arch.get("container_runtime", "docker_compose")
        else:
            container_runtime = arch if arch else "docker_compose"
        template_pairs = self._TEMPLATE_MAP.get(
            container_runtime, self._TEMPLATE_MAP["docker_compose"]
        )

        for template_name, output_name in template_pairs:
            try:
                content = self.render_template(template_name, context)
            except FileNotFoundError as exc:
                result.warnings.append(
                    f"Template '{template_name}' not found — skipped: {exc}"
                )
                continue
            except RuntimeError as exc:
                result.errors.append(str(exc))
                result.success = False
                continue
            except Exception as exc:  # noqa: BLE001
                result.errors.append(
                    f"Error rendering '{template_name}': {exc}"
                )
                result.success = False
                continue

            out_path = output_dir / output_name
            out_path.write_text(content, encoding="utf-8")

            executable = output_name.endswith(".sh")
            if executable:
                out_path.chmod(out_path.stat().st_mode | 0o555)

            result.files.append(
                GeneratedFile(
                    filename=output_name,
                    content=content,
                    path=out_path,
                    executable=executable,
                )
            )

        return result

    def generate_preview(self, profile: dict) -> dict[str, str]:
        """
        Same as generate() but returns {filename: content} without writing to disk.
        Useful for the wizard review step.
        """
        context = self._build_context(profile)
        container_runtime = (
            profile.get("architecture", {}).get("container_runtime", "docker_compose")
        )
        template_pairs = self._TEMPLATE_MAP.get(
            container_runtime, self._TEMPLATE_MAP["docker_compose"]
        )

        previews: dict[str, str] = {}
        for template_name, output_name in template_pairs:
            try:
                content = self.render_template(template_name, context)
                previews[output_name] = content
            except FileNotFoundError:
                previews[output_name] = f"# Template '{template_name}' not found"
            except RuntimeError as exc:
                previews[output_name] = f"# ERROR: {exc}"
            except Exception as exc:  # noqa: BLE001
                previews[output_name] = f"# Render error: {exc}"

        return previews
