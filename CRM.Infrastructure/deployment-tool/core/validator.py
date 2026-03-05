#!/usr/bin/env python3
"""CRM CDT - Wizard Validator"""
from __future__ import annotations
import re
import socket
import ipaddress
from dataclasses import dataclass, field
from typing import Any

try:
    import zxcvbn
    _ZXCVBN_AVAILABLE = True
except ImportError:
    _ZXCVBN_AVAILABLE = False


@dataclass
class ValidationError:
    field_id: str
    message: str
    fix_hint: str = ""


@dataclass
class ValidationResult:
    valid: bool
    errors: list = field(default_factory=list)
    warnings: list = field(default_factory=list)

    def add_error(self, field_id: str, message: str, fix_hint: str = "") -> None:
        self.errors.append(ValidationError(field_id, message, fix_hint))
        self.valid = False

    def add_warning(self, message: str) -> None:
        self.warnings.append(message)

    def to_dict(self) -> dict:
        return {
            "valid": self.valid,
            "errors": [
                {"field_id": e.field_id, "message": e.message, "fix_hint": e.fix_hint}
                for e in self.errors
            ],
            "warnings": self.warnings,
        }


class WizardValidator:
    """Stateless validator for wizard step data."""

    # ------------------------------------------------------------------
    # Primitive validators
    # ------------------------------------------------------------------

    def validate_required(self, value: Any, field_id: str, label: str = "") -> ValidationResult:
        result = ValidationResult(valid=True)
        display = label or field_id
        if value is None or (isinstance(value, str) and not value.strip()):
            result.add_error(field_id, f"{display} is required.", f"Please provide a value for {display}.")
        return result

    def validate_min_length(self, value: str, field_id: str, min_len: int, label: str = "") -> ValidationResult:
        result = ValidationResult(valid=True)
        display = label or field_id
        if value and len(value) < min_len:
            result.add_error(
                field_id,
                f"{display} must be at least {min_len} characters.",
                f"Enter at least {min_len} characters.",
            )
        return result

    def validate_max_length(self, value: str, field_id: str, max_len: int, label: str = "") -> ValidationResult:
        result = ValidationResult(valid=True)
        display = label or field_id
        if value and len(value) > max_len:
            result.add_error(
                field_id,
                f"{display} must be at most {max_len} characters.",
                f"Shorten the value to {max_len} characters or fewer.",
            )
        return result

    def validate_email(self, value: str, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        pattern = r"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$"
        if not value or not re.match(pattern, value.strip()):
            result.add_error(field_id, "Invalid email address.", "Enter a valid email like user@example.com.")
        return result

    def validate_domain(self, value: str, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        if not value or not value.strip():
            result.add_error(field_id, "Domain cannot be empty.")
            return result
        val = value.strip()

        # Allow localhost with a warning
        if val.lower() in ("localhost", "localhost:80", "localhost:443") or val.startswith("localhost:"):
            result.add_warning(f"'{val}' is localhost — not suitable for production.")
            return result

        # Check if it's a valid IP address
        try:
            ipaddress.ip_address(val.split(":")[0])
            return result  # valid IP
        except ValueError:
            pass

        # Validate as FQDN
        fqdn_pattern = r"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$"
        hostname = val.split(":")[0]  # strip optional port
        if not re.match(fqdn_pattern, hostname):
            result.add_error(
                field_id,
                f"'{val}' is not a valid domain, hostname, or IP address.",
                "Enter a valid FQDN (e.g. app.example.com) or IP address.",
            )
        return result

    def validate_cidr(self, value: str, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        try:
            ipaddress.ip_network(value.strip(), strict=False)
        except (ValueError, AttributeError):
            result.add_error(
                field_id,
                f"'{value}' is not a valid CIDR notation.",
                "Enter a valid CIDR e.g. 10.0.0.0/16.",
            )
        return result

    def validate_password_strength(self, value: str, field_id: str, min_score: int = 2) -> ValidationResult:
        result = ValidationResult(valid=True)
        if not value:
            result.add_error(field_id, "Password cannot be empty.")
            return result
        if _ZXCVBN_AVAILABLE:
            return self._validate_password_zxcvbn(value, field_id, min_score)
        return self._validate_password_manual(value, field_id)

    def _validate_password_zxcvbn(self, value: str, field_id: str, min_score: int) -> ValidationResult:
        result = ValidationResult(valid=True)
        analysis = zxcvbn.zxcvbn(value)
        score = analysis.get("score", 0)
        if score < min_score:
            suggestions = analysis.get("feedback", {}).get("suggestions", [])
            hint = " ".join(suggestions) if suggestions else f"Use a stronger password (score {score}/{4})."
            result.add_error(field_id, "Password is too weak.", hint)
        return result

    def _validate_password_manual(self, value: str, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        issues = []
        if len(value) < 12:
            issues.append("at least 12 characters")
        if not re.search(r"[A-Z]", value):
            issues.append("an uppercase letter")
        if not re.search(r"[a-z]", value):
            issues.append("a lowercase letter")
        if not re.search(r"\d", value):
            issues.append("a digit")
        if not re.search(r"[^a-zA-Z0-9]", value):
            issues.append("a special character")
        if issues:
            result.add_error(
                field_id,
                "Password is too weak.",
                "Password must contain: " + ", ".join(issues) + ".",
            )
        return result

    def validate_port(self, value: Any, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        try:
            port = int(value)
        except (TypeError, ValueError):
            result.add_error(field_id, "Port must be a number.", "Enter a number between 1 and 65535.")
            return result
        if port < 1 or port > 65535:
            result.add_error(field_id, f"Port {port} is out of range (1–65535).")
        elif port < 1024:
            result.add_warning(f"Port {port} is a privileged port (< 1024). Running as root may be required.")
        return result

    def validate_port_conflict(self, ports: list, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        for port in ports:
            try:
                port_int = int(port)
            except (TypeError, ValueError):
                continue
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
                try:
                    s.bind(("0.0.0.0", port_int))
                except OSError:
                    result.add_error(
                        field_id,
                        f"Port {port_int} is already in use.",
                        f"Free port {port_int} before deploying, or change the assigned port.",
                    )
        return result

    def validate_username(self, value: str, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        pattern = r"^[a-zA-Z][a-zA-Z0-9_\-]{2,49}$"
        if not value or not re.match(pattern, value.strip()):
            result.add_error(
                field_id,
                "Username must start with a letter, be 3–50 characters, and contain only letters, digits, _ or -.",
                "Example: admin, crm_user, deploy-user",
            )
        return result

    def validate_passwords_match(self, pw1: str, pw2: str, field_id: str) -> ValidationResult:
        result = ValidationResult(valid=True)
        if pw1 != pw2:
            result.add_error(field_id, "Passwords do not match.", "Ensure both password fields contain the same value.")
        return result

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _merge(self, target: ValidationResult, source: ValidationResult) -> None:
        """Merge errors + warnings from *source* into *target*."""
        target.errors.extend(source.errors)
        target.warnings.extend(source.warnings)
        if not source.valid:
            target.valid = False

    # ------------------------------------------------------------------
    # Step-level validator
    # ------------------------------------------------------------------

    def validate_step(self, step_id: str, data: dict) -> ValidationResult:
        """Dispatch to per-step validator, return merged ValidationResult."""
        dispatch = {
            "profile": self._validate_profile,
            "target": self._validate_target,
            "database": self._validate_database,
            "security": self._validate_security,
            "seed": self._validate_seed,
            "network": self._validate_network,
        }
        handler = dispatch.get(step_id)
        if handler:
            return handler(data)
        return ValidationResult(valid=True)

    # ------------------------------------------------------------------
    # Per-step validators
    # ------------------------------------------------------------------

    def _validate_profile(self, data: dict) -> ValidationResult:
        result = ValidationResult(valid=True)
        name = data.get("profile_name", "")
        self._merge(result, self.validate_required(name, "profile_name", "Profile name"))
        if name:
            self._merge(result, self.validate_min_length(name, "profile_name", 1))
            self._merge(result, self.validate_max_length(name, "profile_name", 50))
            if not re.match(r"^[a-zA-Z0-9][a-zA-Z0-9_\-]*$", name):
                result.add_error(
                    "profile_name",
                    "Profile name must start with a letter or digit and contain only letters, digits, _ or -.",
                    "Example: my-deploy, prod_v2, crm2026",
                )
        return result

    def _validate_target(self, data: dict) -> ValidationResult:
        result = ValidationResult(valid=True)
        self._merge(result, self.validate_required(data.get("provider"), "provider", "Deployment provider"))
        return result

    def _validate_db_password_required(self, data: dict, auth_method: str) -> ValidationResult:
        """Validate password only when auth method requires a static credential."""
        result = ValidationResult(valid=True)
        if auth_method not in ("password", "docker_secret"):
            return result
        db_password = data.get("db_password", "")
        if not db_password or not db_password.strip():
            result.add_error(
                "db_password",
                f"Database password is required when auth method is '{auth_method}'.",
                "Enter a password or switch to a password-free auth method "
                "(iam_aws / iam_azure / iam_gcp / ssl_cert / vault_dynamic).",
            )
        return result

    def _validate_db_auth_method_fields(self, data: dict, auth_method: str) -> ValidationResult:
        """Validate auth-method-specific required fields per SPEC-INF-001."""
        result = ValidationResult(valid=True)
        if auth_method == "iam_aws":
            arn = data.get("db_iam_role_arn", "")
            if not arn or not arn.strip():
                result.add_warning(
                    "db_iam_role_arn is empty — ensure the EKS/ECS task role has "
                    "rds-db:connect IAM permission for the RDS resource ARN."
                )
        elif auth_method == "iam_gcp":
            instance = data.get("db_cloud_sql_instance", "")
            if not instance or not instance.strip():
                result.add_error(
                    "db_cloud_sql_instance",
                    "Cloud SQL instance connection name is required for GCP Workload Identity auth.",
                    "Format: PROJECT:REGION:INSTANCE — e.g. my-project:us-central1:crm-db",
                )
        elif auth_method == "ssl_cert":
            self._merge(result, self._validate_ssl_cert_fields(data))
        elif auth_method == "vault_dynamic":
            vault_addr = data.get("vault_address", "")
            if not vault_addr or not vault_addr.strip():
                result.add_error(
                    "vault_address",
                    "Vault address is required for dynamic credential auth.",
                    "Enter the Vault server URL accessible from within the cluster.",
                )
        return result

    def _validate_ssl_cert_fields(self, data: dict) -> ValidationResult:
        """Validate the three SSL cert paths required for mTLS auth."""
        result = ValidationResult(valid=True)
        for field_id, label in (
            ("db_ssl_cert_path", "Client certificate path"),
            ("db_ssl_key_path", "Client key path"),
            ("db_ssl_ca_path", "CA certificate path"),
        ):
            val = data.get(field_id, "")
            if not val or not val.strip():
                result.add_error(
                    field_id,
                    f"{label} is required for mTLS client certificate auth.",
                    "Provide the container path to the mounted certificate file.",
                )
        return result

    def _validate_database(self, data: dict) -> ValidationResult:
        result = ValidationResult(valid=True)
        auth_method = data.get("db_auth_method", "password")

        self._merge(result, self.validate_required(data.get("db_name", ""), "db_name", "Database name"))
        if data.get("db_name"):
            self._merge(result, self.validate_min_length(data["db_name"], "db_name", 3, "Database name"))

        self._merge(result, self.validate_required(data.get("db_user", ""), "db_user", "Database user"))
        if data.get("db_user"):
            self._merge(result, self.validate_min_length(data["db_user"], "db_user", 3, "Database user"))

        self._merge(result, self.validate_required(data.get("db_host", ""), "db_host", "Database host"))

        if data.get("db_port") is not None:
            self._merge(result, self.validate_port(data["db_port"], "db_port"))

        self._merge(result, self._validate_db_password_required(data, auth_method))
        self._merge(result, self._validate_db_auth_method_fields(data, auth_method))
        return result

    def _validate_security(self, data: dict) -> ValidationResult:
        result = ValidationResult(valid=True)
        jwt_secret = data.get("jwt_secret", "")
        access_ttl = data.get("jwt_access_ttl")
        refresh_ttl = data.get("jwt_refresh_ttl_days")

        if jwt_secret:
            self._merge(result, self.validate_min_length(jwt_secret, "jwt_secret", 32, "JWT secret"))
            self._merge(result, self.validate_password_strength(jwt_secret, "jwt_secret", min_score=3))

        if access_ttl is not None:
            try:
                ttl = int(access_ttl)
                if ttl < 5 or ttl > 1440:
                    result.add_error(
                        "jwt_access_ttl", "Access token TTL must be between 5 and 1440 minutes.",
                        "Enter a value between 5 (5 min) and 1440 (24 h).",
                    )
            except (TypeError, ValueError):
                result.add_error("jwt_access_ttl", "Access token TTL must be a number.")

        if refresh_ttl is not None:
            try:
                days = int(refresh_ttl)
                if days < 1 or days > 90:
                    result.add_error(
                        "jwt_refresh_ttl_days", "Refresh token TTL must be between 1 and 90 days.",
                        "Enter a value between 1 and 90.",
                    )
            except (TypeError, ValueError):
                result.add_error("jwt_refresh_ttl_days", "Refresh token TTL must be a number.")

        return result

    def _validate_seed(self, data: dict) -> ValidationResult:
        result = ValidationResult(valid=True)

        email = data.get("admin_email", "")
        username = data.get("admin_username", "")
        first_name = data.get("admin_first_name", "")
        last_name = data.get("admin_last_name", "")
        password = data.get("admin_password", "")
        password_confirm = data.get("admin_password_confirm", "")

        self._merge(result, self.validate_required(email, "admin_email", "Admin email"))
        if email:
            self._merge(result, self.validate_email(email, "admin_email"))

        self._merge(result, self.validate_required(username, "admin_username", "Admin username"))
        if username:
            self._merge(result, self.validate_username(username, "admin_username"))

        self._merge(result, self.validate_required(first_name, "admin_first_name", "First name"))
        self._merge(result, self.validate_required(last_name, "admin_last_name", "Last name"))

        self._merge(result, self.validate_required(password, "admin_password", "Admin password"))
        if password:
            self._merge(result, self.validate_password_strength(password, "admin_password"))

        self._merge(result, self.validate_passwords_match(password, password_confirm, "admin_password_confirm"))

        return result

    def _validate_network(self, data: dict) -> ValidationResult:
        result = ValidationResult(valid=True)
        cors_origins_raw = data.get("cors_origins", "")
        if cors_origins_raw:
            lines = [line.strip() for line in cors_origins_raw.splitlines() if line.strip()]
            for origin in lines:
                # Strip scheme for domain validation
                domain_part = re.sub(r"^https?://", "", origin)
                self._merge(result, self.validate_domain(domain_part, "cors_origins"))
        return result
