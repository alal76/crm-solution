#!/usr/bin/env python3
"""
core/profile.py — Profile CRUD, quick-start templates, and run history for
the CRM Deployment Tool.

Profiles are stored as JSON files under ``~/.crm-cdt/profiles/<name>.json``.
Run history is persisted in SQLite at ``~/.crm-cdt/history.db``.
"""

from __future__ import annotations

import json
import os
import sqlite3
from dataclasses import dataclass, field, asdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Optional


# ---------------------------------------------------------------------------
# Custom exceptions
# ---------------------------------------------------------------------------


class ProfileNotFoundError(Exception):
    """Raised when a profile with the given name does not exist."""


class ProfileExistsError(Exception):
    """Raised when trying to create a profile that already exists (without overwrite)."""


# ---------------------------------------------------------------------------
# Run history
# ---------------------------------------------------------------------------


@dataclass
class RunHistoryRecord:
    """A single completed (or failed) deployment run."""

    run_id: str
    profile_name: str
    timestamp: datetime
    crm_version: str
    action: str
    result: str
    duration_seconds: float
    log_path: str
    snapshot: dict = field(default_factory=dict)


class RunHistoryManager:
    """SQLite-backed store for deployment run history.

    Database location: ``~/.crm-cdt/history.db``

    The ``runs`` table schema::

        run_id TEXT PRIMARY KEY
        profile_name TEXT NOT NULL
        timestamp TEXT NOT NULL        -- ISO-8601
        crm_version TEXT
        action TEXT
        result TEXT
        duration_seconds REAL
        log_path TEXT
        snapshot TEXT                  -- JSON blob
    """

    def __init__(self, db_path: Optional[Path] = None) -> None:
        if db_path is None:
            db_dir = Path.home() / ".crm-cdt"
            db_dir.mkdir(parents=True, exist_ok=True)
            self._db_path = db_dir / "history.db"
        else:
            self._db_path = Path(db_path)
            self._db_path.parent.mkdir(parents=True, exist_ok=True)
        self._init_db()

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def record_run(self, record: RunHistoryRecord) -> None:
        """Insert a run history record into the database."""
        with self._connect() as conn:
            conn.execute(
                """
                INSERT OR REPLACE INTO runs
                    (run_id, profile_name, timestamp, crm_version, action,
                     result, duration_seconds, log_path, snapshot)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    record.run_id,
                    record.profile_name,
                    record.timestamp.isoformat(),
                    record.crm_version,
                    record.action,
                    record.result,
                    record.duration_seconds,
                    record.log_path,
                    json.dumps(record.snapshot),
                ),
            )

    def list_runs(self, profile_name: Optional[str] = None) -> list[RunHistoryRecord]:
        """Return all run records, optionally filtered by *profile_name*."""
        with self._connect() as conn:
            if profile_name:
                rows = conn.execute(
                    "SELECT * FROM runs WHERE profile_name = ? ORDER BY timestamp DESC",
                    (profile_name,),
                ).fetchall()
            else:
                rows = conn.execute(
                    "SELECT * FROM runs ORDER BY timestamp DESC"
                ).fetchall()
        return [self._row_to_record(r) for r in rows]

    def get_snapshot(self, run_id: str) -> dict:
        """Return the snapshot dict for a specific run."""
        with self._connect() as conn:
            row = conn.execute(
                "SELECT snapshot FROM runs WHERE run_id = ?", (run_id,)
            ).fetchone()
        if row is None:
            raise KeyError(f"Run '{run_id}' not found.")
        return json.loads(row[0]) if row[0] else {}

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _init_db(self) -> None:
        with self._connect() as conn:
            conn.execute(
                """
                CREATE TABLE IF NOT EXISTS runs (
                    run_id           TEXT PRIMARY KEY,
                    profile_name     TEXT NOT NULL,
                    timestamp        TEXT NOT NULL,
                    crm_version      TEXT,
                    action           TEXT,
                    result           TEXT,
                    duration_seconds REAL,
                    log_path         TEXT,
                    snapshot         TEXT
                )
                """
            )

    def _connect(self) -> sqlite3.Connection:
        conn = sqlite3.connect(str(self._db_path))
        conn.row_factory = sqlite3.Row
        return conn

    @staticmethod
    def _row_to_record(row: sqlite3.Row) -> RunHistoryRecord:
        return RunHistoryRecord(
            run_id=row["run_id"],
            profile_name=row["profile_name"],
            timestamp=datetime.fromisoformat(row["timestamp"]),
            crm_version=row["crm_version"] or "",
            action=row["action"] or "",
            result=row["result"] or "",
            duration_seconds=row["duration_seconds"] or 0.0,
            log_path=row["log_path"] or "",
            snapshot=json.loads(row["snapshot"]) if row["snapshot"] else {},
        )


# ---------------------------------------------------------------------------
# ProfileManager
# ---------------------------------------------------------------------------


class ProfileManager:
    """CRUD manager for deployment profiles stored as JSON files.

    Profile directory: ``~/.crm-cdt/profiles/``

    Each profile is a ``<name>.json`` file containing sections:
    ``meta``, ``target``, ``architecture``, ``database``, ``network``,
    ``security``, ``providers``, ``seed``.
    """

    def __init__(self, profiles_dir: Optional[Path] = None) -> None:
        if profiles_dir is None:
            self._profiles_dir = Path.home() / ".crm-cdt" / "profiles"
        else:
            self._profiles_dir = Path(profiles_dir)
        self._profiles_dir.mkdir(parents=True, exist_ok=True)

    # ------------------------------------------------------------------
    # CRUD
    # ------------------------------------------------------------------

    def list_profiles(self) -> list[dict]:
        """Return a summary list of all profiles.

        Each item contains: ``name``, ``created_at``, ``updated_at``,
        ``crm_version``, ``environment_type``.
        """
        result = []
        for path in sorted(self._profiles_dir.glob("*.json")):
            try:
                data = json.loads(path.read_text(encoding="utf-8"))
                meta = data.get("meta", {})
                result.append(
                    {
                        "name": path.stem,
                        "created_at": meta.get("created_at", ""),
                        "updated_at": meta.get("updated_at", ""),
                        "crm_version": meta.get("crm_version", ""),
                        "environment_type": data.get("target", {}).get("environment_type", ""),
                    }
                )
            except (OSError, json.JSONDecodeError):
                # Skip corrupt files silently
                pass
        return result

    def load(self, name: str) -> dict:
        """Load and return the profile dict for *name*.

        Raises
        ------
        ProfileNotFoundError
            If no profile with *name* exists.
        """
        path = self._profile_path(name)
        if not path.exists():
            raise ProfileNotFoundError(f"Profile '{name}' not found.")
        return json.loads(path.read_text(encoding="utf-8"))

    def save(self, name: str, data: dict) -> None:
        """Write *data* as the profile JSON for *name*.

        The ``meta.updated_at`` field is automatically set to the current UTC
        time.  ``meta.profile_name`` is set to *name*.
        """
        data.setdefault("meta", {})
        data["meta"]["profile_name"] = name
        data["meta"]["updated_at"] = datetime.now(timezone.utc).isoformat()
        if "created_at" not in data["meta"]:
            data["meta"]["created_at"] = data["meta"]["updated_at"]
        path = self._profile_path(name)
        tmp = path.with_suffix(".json.tmp")
        tmp.write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")
        tmp.replace(path)

    def delete(self, name: str) -> None:
        """Delete the profile *name*.

        Raises
        ------
        ProfileNotFoundError
            If no profile with *name* exists.
        """
        path = self._profile_path(name)
        if not path.exists():
            raise ProfileNotFoundError(f"Profile '{name}' not found.")
        path.unlink()

    # ------------------------------------------------------------------
    # Import / export
    # ------------------------------------------------------------------

    def export(self, name: str) -> str:
        """Return the profile as a JSON string with credential values replaced
        by ``"__vault::<key>"`` placeholder references.

        Raises
        ------
        ProfileNotFoundError
            If no profile with *name* exists.
        """
        data = self.load(name)
        sanitized = self._strip_secrets(data)
        return json.dumps(sanitized, indent=2, ensure_ascii=False)

    def import_profile(self, json_str: str, overwrite: bool = False) -> str:
        """Parse *json_str* and persist as a profile.

        The JSON must contain ``meta.profile_name``.

        Parameters
        ----------
        json_str:
            JSON string representing the profile.
        overwrite:
            If ``False`` and a profile with that name already exists,
            raise :class:`ProfileExistsError`.

        Returns
        -------
        str
            The profile name that was imported.

        Raises
        ------
        ValueError
            If ``meta.profile_name`` is missing.
        ProfileExistsError
            If the profile exists and *overwrite* is ``False``.
        """
        data = json.loads(json_str)
        name = data.get("meta", {}).get("profile_name")
        if not name:
            raise ValueError("Imported profile JSON must contain 'meta.profile_name'.")
        if not overwrite and self._profile_path(name).exists():
            raise ProfileExistsError(
                f"Profile '{name}' already exists. Pass overwrite=True to replace it."
            )
        self.save(name, data)
        return name

    # ------------------------------------------------------------------
    # Comparison
    # ------------------------------------------------------------------

    def compare(self, name_a: str, name_b: str) -> dict:
        """Return a deep diff of two profiles.

        Returns
        -------
        dict
            Keys are dot-separated field paths to all leaf values that differ.
            Each value is ``{"a": val_a, "b": val_b}``.

        Raises
        ------
        ProfileNotFoundError
            If either profile does not exist.
        """
        data_a = self.load(name_a)
        data_b = self.load(name_b)
        return self._deep_diff(data_a, data_b, "")

    # ------------------------------------------------------------------
    # Quick-start templates
    # ------------------------------------------------------------------

    def get_templates(self) -> list[dict]:
        """Return 7 quick-start template profile dicts."""
        _now = datetime.now(timezone.utc).isoformat()

        def _base(name: str) -> dict:
            return {
                "meta": {
                    "profile_name": name,
                    "created_at": _now,
                    "updated_at": _now,
                    "crm_version": "0.608.1",
                },
                "target": {},
                "architecture": {},
                "database": {},
                "network": {},
                "security": {},
                "providers": {},
                "seed": {},
            }

        def _merge(base: dict, overrides: dict) -> dict:
            """Shallow-merge top-level sections."""
            for section, values in overrides.items():
                base.setdefault(section, {}).update(values)
            return base

        templates = [
            _merge(
                _base("local-dev"),
                {
                    "target": {"provider": "local_docker", "environment_type": "development", "use_ssl": False},
                    "architecture": {"mode": "monolith", "container_runtime": "docker_compose"},
                    "database": {"db_provider": "mariadb"},
                },
            ),
            _merge(
                _base("aws-ecs-monolith"),
                {
                    "target": {"provider": "aws", "environment_type": "production", "use_ssl": True},
                    "architecture": {"mode": "monolith", "container_runtime": "ecs_fargate"},
                    "database": {"db_provider": "rds_mysql"},
                },
            ),
            _merge(
                _base("aws-eks-microservices"),
                {
                    "target": {"provider": "aws", "environment_type": "production", "use_ssl": True},
                    "architecture": {"mode": "microservices", "container_runtime": "kubernetes"},
                    "database": {"db_provider": "rds_mysql"},
                },
            ),
            _merge(
                _base("azure-aks-microservices"),
                {
                    "target": {"provider": "azure", "environment_type": "production", "use_ssl": True},
                    "architecture": {"mode": "microservices", "container_runtime": "kubernetes"},
                    "database": {"db_provider": "azure_mysql"},
                },
            ),
            _merge(
                _base("gcp-gke-microservices"),
                {
                    "target": {"provider": "gcp", "environment_type": "production", "use_ssl": True},
                    "architecture": {"mode": "microservices", "container_runtime": "kubernetes"},
                    "database": {"db_provider": "cloud_sql"},
                },
            ),
            _merge(
                _base("on-prem-k8s"),
                {
                    "target": {"provider": "on_prem_bare", "environment_type": "production", "use_ssl": True},
                    "architecture": {"mode": "microservices", "container_runtime": "kubernetes"},
                    "database": {"db_provider": "mariadb"},
                },
            ),
            _merge(
                _base("on-prem-docker"),
                {
                    "target": {"provider": "on_prem_vm", "environment_type": "production", "use_ssl": False},
                    "architecture": {"mode": "monolith", "container_runtime": "docker_compose"},
                    "database": {"db_provider": "mariadb"},
                },
            ),
        ]
        return templates

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _profile_path(self, name: str) -> Path:
        return self._profiles_dir / f"{name}.json"

    @staticmethod
    def _strip_secrets(data: Any, path: str = "") -> Any:
        """Recursively replace password-like leaf values with vault key refs."""
        _SECRET_KEYS = {"password", "secret", "api_key", "token", "key", "credential"}

        if isinstance(data, dict):
            out = {}
            for k, v in data.items():
                if any(s in k.lower() for s in _SECRET_KEYS) and isinstance(v, str) and v:
                    vault_key = f"{path}.{k}".lstrip(".")
                    out[k] = f"__vault::{vault_key}"
                else:
                    out[k] = ProfileManager._strip_secrets(v, f"{path}.{k}".lstrip("."))
            return out
        if isinstance(data, list):
            return [ProfileManager._strip_secrets(item, path) for item in data]
        return data

    @staticmethod
    def _deep_diff(a: Any, b: Any, path: str) -> dict:
        """Recursively compute the diff of two values."""
        diff: dict = {}
        if isinstance(a, dict) and isinstance(b, dict):
            all_keys = set(a.keys()) | set(b.keys())
            for k in all_keys:
                child_path = f"{path}.{k}".lstrip(".")
                child_diff = ProfileManager._deep_diff(a.get(k), b.get(k), child_path)
                diff.update(child_diff)
        elif a != b:
            diff[path] = {"a": a, "b": b}
        return diff
