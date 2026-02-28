#!/usr/bin/env python3
"""CRM CDT Day-2 - Secret Rotation."""
from __future__ import annotations
from dataclasses import dataclass, field
from pathlib import Path
import subprocess, secrets, string


@dataclass
class RotationResult:
    success: bool
    rotated_secrets: list = field(default_factory=list)
    skipped: list = field(default_factory=list)
    errors: list = field(default_factory=list)

    def to_dict(self) -> dict:
        return {"success": self.success, "rotated_secrets": self.rotated_secrets, "skipped": self.skipped, "errors": self.errors}


class SecretRotator:
    def __init__(self, work_dir: Path, profile: dict, vault=None, dry_run: bool = False):
        self.work_dir = Path(work_dir)
        self.profile = profile
        self.vault = vault
        self.dry_run = dry_run

    def generate_secret(self, length: int = 32, special: bool = True) -> str:
        alphabet = string.ascii_letters + string.digits + ("!@#$%^&*" if special else "")
        return "".join(secrets.choice(alphabet) for _ in range(length))

    def _update_env_file(self, key: str, value: str) -> bool:
        env_file = self.work_dir / ".env"
        if not env_file.exists():
            return False
        content = env_file.read_text()
        lines = content.splitlines()
        updated = False
        new_lines = []
        for line in lines:
            if line.startswith(f"{key}="):
                new_lines.append(f"{key}={value}")
                updated = True
            else:
                new_lines.append(line)
        if not updated:
            new_lines.append(f"{key}={value}")
        env_file.write_text("\n".join(new_lines) + "\n")
        return True

    def rotate_jwt_secret(self) -> RotationResult:
        result = RotationResult(success=False)
        new_secret = self.generate_secret(64, special=True)
        if self.dry_run:
            result.rotated_secrets.append("JWT_SECRET")
            result.success = True
            result.errors.append("DRY-RUN: Would update .env and restart crm-api")
            return result
        self._update_env_file("JWT_SECRET", new_secret)
        subprocess.call(["docker", "compose", "up", "-d", "--no-deps", "crm-api"], cwd=str(self.work_dir))
        if self.vault:
            try:
                self.vault.set("jwt_secret", new_secret)
            except Exception:
                pass
        result.rotated_secrets.append("JWT_SECRET")
        result.success = True
        return result

    def rotate_db_password(self) -> RotationResult:
        result = RotationResult(success=False)
        new_pass = self.generate_secret(24, special=False)
        db = self.profile.get("database", {})
        if self.dry_run:
            result.rotated_secrets.append("DB_PASSWORD")
            result.success = True
            result.errors.append("DRY-RUN: Would alter DB user and restart crm-api")
            return result
        try:
            cmd = ["docker", "exec", "crm-mariadb", "mysql", f"-u{db.get('db_user','crm_user')}", f"-p{db.get('db_password','')}", "-e", f"ALTER USER '{db.get('db_user','crm_user')}'@'%' IDENTIFIED BY '{new_pass}'; FLUSH PRIVILEGES;"]
            rc = subprocess.call(cmd)
            if rc == 0:
                self._update_env_file("DB_PASSWORD", new_pass)
                subprocess.call(["docker", "compose", "up", "-d", "--no-deps", "crm-api"], cwd=str(self.work_dir))
                result.rotated_secrets.append("DB_PASSWORD")
                result.success = True
        except Exception as e:
            result.errors.append(str(e))
        return result

    def rotate_provider_api_key(self, provider: str) -> RotationResult:
        result = RotationResult(success=False)
        if provider == "meilisearch":
            new_key = self.generate_secret(32, special=False)
            if self.dry_run:
                result.rotated_secrets.append(f"{provider}_key")
                result.success = True
            else:
                self._update_env_file("MEILI_MASTER_KEY", new_key)
                result.rotated_secrets.append(f"{provider}_key")
                result.success = True
        else:
            result.skipped.append(provider)
            result.success = True
        return result

    def rotate_all(self) -> RotationResult:
        combined = RotationResult(success=True)
        for fn in [self.rotate_jwt_secret, self.rotate_db_password]:
            r = fn()
            combined.rotated_secrets.extend(r.rotated_secrets)
            combined.skipped.extend(r.skipped)
            combined.errors.extend(r.errors)
            if not r.success:
                combined.success = False
        return combined
