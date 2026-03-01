#!/usr/bin/env python3
"""
CRM CDT - Component Detector
Detects running CRM components and their status/version.

Provides a structured view of what is already deployed so the wizard
can offer "reuse existing" options instead of always redeploying.
"""

from __future__ import annotations

import json
import socket
import subprocess
import urllib.request
import urllib.error
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass, field
from enum import Enum
from typing import Any, Dict, List, Optional

# The probe user has no real password; connection is expected to fail with
# an auth error, which still confirms the DB socket is accepting connections.
_probe_password: str = ""  # noqa: S105 (intentionally empty probe credential)


# ---------------------------------------------------------------------------
# Enums
# ---------------------------------------------------------------------------

class ComponentAction(Enum):
    REUSE = "reuse"
    REPLACE = "replace"
    UPGRADE = "upgrade"
    DEPLOY_NEW = "deploy_new"
    SKIP = "skip"


# ---------------------------------------------------------------------------
# ComponentStatus
# ---------------------------------------------------------------------------

@dataclass
class ComponentStatus:
    name: str                                                # display name, e.g. "MariaDB"
    key: str                                                 # internal key,  e.g. "mariadb"
    detected: bool = False
    running: bool = False
    version: str = ""
    port: int = 0
    image: str = ""
    latest_version: str = ""                                 # for upgrade detection
    upgrade_available: bool = False
    action: ComponentAction = ComponentAction.DEPLOY_NEW
    reuse_credentials: Dict[str, Any] = field(default_factory=dict)

    def to_dict(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "key": self.key,
            "detected": self.detected,
            "running": self.running,
            "version": self.version,
            "port": self.port,
            "image": self.image,
            "latest_version": self.latest_version,
            "upgrade_available": self.upgrade_available,
            "action": self.action.value,
            "reuse_credentials": self.reuse_credentials,
        }


# ---------------------------------------------------------------------------
# Helper utilities
# ---------------------------------------------------------------------------

def _http_get(url: str, timeout: float = 3.0) -> Optional[urllib.request.http.client.HTTPResponse]:  # noqa: F821
    """Perform a simple HTTP GET.  Returns the response or None on error."""
    try:
        return urllib.request.urlopen(url, timeout=timeout)  # noqa: S310
    except Exception:
        return None


def _http_get_json(url: str, timeout: float = 3.0) -> Optional[Dict[str, Any]]:
    """HTTP GET → parse JSON body.  Returns None on any error."""
    try:
        with urllib.request.urlopen(url, timeout=timeout) as resp:  # noqa: S310
            raw = resp.read()
            return json.loads(raw.decode())
    except Exception:
        return None


# ---------------------------------------------------------------------------
# Detector
# ---------------------------------------------------------------------------

class ComponentDetector:
    """Detect running CRM components by probing well-known ports and endpoints."""

    # ------------------------------------------------------------------
    # Base TCP helper
    # ------------------------------------------------------------------

    @staticmethod
    def detect_tcp(host: str, port: int, timeout: float = 2.0) -> bool:
        """Return True if a TCP connection to host:port succeeds within timeout."""
        try:
            with socket.create_connection((host, port), timeout=timeout):
                return True
        except OSError:
            return False

    # ------------------------------------------------------------------
    # Individual detectors
    # ------------------------------------------------------------------

    def detect_mariadb(self, host: str = "localhost", port: int = 3306) -> ComponentStatus:
        status = ComponentStatus(name="MariaDB", key="mariadb", port=port)
        tcp_open = self.detect_tcp(host, port)
        status.detected = tcp_open
        status.running = tcp_open

        if tcp_open:
            # Try to determine version via anonymous connect (usually denied; TCP open is the real check)
            try:
                import pymysql  # type: ignore
                conn = pymysql.connect(host=host, port=port, user="crm_probe", password=_probe_password,
                                       connect_timeout=2)
                with conn.cursor() as cur:
                    cur.execute("SELECT @@version")
                    row = cur.fetchone()
                    if row:
                        status.version = str(row[0])
                conn.close()
            except Exception:
                pass  # Expected — anon connect usually denied; TCP open is enough

        return self.set_action(status)

    def detect_redis(self, host: str = "localhost", port: int = 6379) -> ComponentStatus:
        status = ComponentStatus(name="Redis", key="redis", port=port)
        tcp_open = self.detect_tcp(host, port)
        status.detected = tcp_open
        status.running = tcp_open

        if tcp_open:
            try:
                import redis  # type: ignore
                r = redis.Redis(host=host, port=port, socket_connect_timeout=2)
                r.ping()
                info = r.info("server")
                status.version = info.get("redis_version", "")
            except Exception:
                pass

        return self.set_action(status)

    def detect_meilisearch(self, host: str = "localhost", port: int = 7700) -> ComponentStatus:
        status = ComponentStatus(name="Meilisearch", key="meilisearch", port=port)
        url = f"http://{host}:{port}/health"
        try:
            with urllib.request.urlopen(url, timeout=3) as resp:  # noqa: S310
                body = json.loads(resp.read().decode())
                if body.get("status") == "available":
                    status.detected = True
                    status.running = True
                # Try to extract version from headers
                version = resp.headers.get("X-Meilisearch-Version", "")
                if version:
                    status.version = version
                # Or from body (newer releases)
                if not status.version and "pkgVersion" in body:
                    status.version = body["pkgVersion"]
        except Exception:
            pass

        return self.set_action(status)

    def detect_ollama(self, host: str = "localhost", port: int = 11434) -> ComponentStatus:
        status = ComponentStatus(name="Ollama", key="ollama", port=port)
        data = _http_get_json(f"http://{host}:{port}/api/tags", timeout=3)
        if data is not None:
            status.detected = True
            status.running = True
            models = data.get("models", [])
            status.version = f"{len(models)} model(s) loaded"
        return self.set_action(status)

    def detect_crm_api(self, host: str = "localhost", port: int = 5000) -> ComponentStatus:
        status = ComponentStatus(name="CRM API", key="crm_api", port=port)
        data = _http_get_json(f"http://{host}:{port}/health", timeout=4)
        if data is not None:
            status.detected = True
            status.running = True
            status.version = data.get("version", "")
        return self.set_action(status)

    def detect_crm_frontend(self, host: str = "localhost", port: int = 80) -> ComponentStatus:
        status = ComponentStatus(name="CRM Frontend", key="crm_frontend", port=port)
        resp = _http_get(f"http://{host}:{port}/", timeout=3)
        if resp is not None:
            status.detected = True
            status.running = True
        return self.set_action(status)

    def detect_n8n(self, host: str = "localhost", port: int = 5678) -> ComponentStatus:
        status = ComponentStatus(name="n8n", key="n8n", port=port)
        resp = _http_get(f"http://{host}:{port}/healthz", timeout=3)
        if resp is not None:
            status.detected = True
            status.running = True
        return self.set_action(status)

    def detect_chatwoot(self, host: str = "localhost", port: int = 3000) -> ComponentStatus:
        status = ComponentStatus(name="Chatwoot", key="chatwoot", port=port)
        tcp_open = self.detect_tcp(host, port)
        status.detected = tcp_open
        status.running = tcp_open
        return self.set_action(status)

    def detect_superset(self, host: str = "localhost", port: int = 8088) -> ComponentStatus:
        status = ComponentStatus(name="Superset", key="superset", port=port)
        resp = _http_get(f"http://{host}:{port}/health", timeout=3)
        if resp is not None:
            status.detected = True
            status.running = True
        return self.set_action(status)

    # ------------------------------------------------------------------
    # Docker container detection
    # ------------------------------------------------------------------

    def detect_docker_containers(self, _host: str = "localhost") -> List[ComponentStatus]:
        """Parse `docker ps` output to find running crm-* containers."""
        results: List[ComponentStatus] = []
        try:
            fmt = '{"name":"{{.Names}}","status":"{{.Status}}","image":"{{.Image}}"}'
            cmd = ["docker", "ps", "--format", fmt]
            proc = subprocess.run(cmd, capture_output=True, text=True, timeout=10)
            if proc.returncode != 0:
                return results

            # Map well-known container names → component keys
            name_map: Dict[str, str] = {
                "crm-api": "crm_api",
                "crm-frontend": "crm_frontend",
                "crm-mariadb": "mariadb",
                "crm-redis": "redis",
                "crm-meilisearch": "meilisearch",
                "crm-ollama": "ollama",
                "crm-n8n": "n8n",
                "crm-chatwoot": "chatwoot",
                "crm-superset": "superset",
                "crm-docuseal": "docuseal",
            }

            for line in proc.stdout.splitlines():
                line = line.strip()
                if not line:
                    continue
                try:
                    rec = json.loads(line)
                except json.JSONDecodeError:
                    continue

                container_name: str = rec.get("name", "")
                if not container_name.startswith("crm-"):
                    continue

                key = name_map.get(container_name, container_name.lstrip("crm-").replace("-", "_"))
                display = container_name.removeprefix("crm-").title()
                stat = rec.get("status", "")
                running = "Up" in stat

                cs = ComponentStatus(
                    name=display,
                    key=key,
                    detected=True,
                    running=running,
                    image=rec.get("image", ""),
                )
                results.append(self.set_action(cs))

        except (FileNotFoundError, subprocess.TimeoutExpired):
            pass  # Docker not installed or not running
        except Exception:
            pass

        return results

    # ------------------------------------------------------------------
    # detect_all
    # ------------------------------------------------------------------

    def detect_all(self, host: str = "localhost") -> List[ComponentStatus]:
        """Run all detect_* methods in parallel and return consolidated results."""
        detectors = [
            lambda h=host: self.detect_mariadb(h),
            lambda h=host: self.detect_redis(h),
            lambda h=host: self.detect_meilisearch(h),
            lambda h=host: self.detect_ollama(h),
            lambda h=host: self.detect_crm_api(h),
            lambda h=host: self.detect_crm_frontend(h),
            lambda h=host: self.detect_n8n(h),
            lambda h=host: self.detect_chatwoot(h),
            lambda h=host: self.detect_superset(h),
        ]

        results: List[ComponentStatus] = []
        with ThreadPoolExecutor(max_workers=8) as executor:
            futures = [executor.submit(fn) for fn in detectors]
            for future in as_completed(futures):
                try:
                    results.append(future.result())
                except Exception:
                    pass  # Skip failed detectors — component still gets added as DEPLOY_NEW

        # Also merge Docker container info
        try:
            docker_results = self.detect_docker_containers(host)
            # Enrich existing results with Docker image info where key matches
            keyed = {cs.key: cs for cs in results}
            for dc in docker_results:
                if dc.key in keyed:
                    keyed[dc.key].image = dc.image or keyed[dc.key].image
                else:
                    results.append(dc)
        except Exception:
            pass

        # Sort: detected (running) first, then detected (stopped), then not detected; by name within group
        def _sort_key(cs: ComponentStatus) -> tuple:
            if cs.detected and cs.running:
                return (0, cs.name)
            if cs.detected:
                return (1, cs.name)
            return (2, cs.name)

        results.sort(key=_sort_key)
        return results

    # ------------------------------------------------------------------
    # Action resolver
    # ------------------------------------------------------------------

    def set_action(self, component: ComponentStatus) -> ComponentStatus:
        """Derive and set the recommended action for a component."""
        if not component.detected:
            component.action = ComponentAction.DEPLOY_NEW
        elif component.upgrade_available:
            component.action = ComponentAction.UPGRADE
        elif component.running:
            component.action = ComponentAction.REUSE
        else:
            component.action = ComponentAction.REPLACE
        return component
