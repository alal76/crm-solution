#!/usr/bin/env python3
"""
CRM CDT - Environment Probe
Tests connectivity and resource availability on target systems.

Provides structured, actionable results so the wizard can surface
clear pass/warn/fail signals with fix hints before the user commits
to a deployment action.
"""

from __future__ import annotations

import shutil
import socket
import subprocess
import urllib.request
import urllib.error
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass, field
from enum import Enum
from pathlib import Path
from typing import List, Dict, Any


# ---------------------------------------------------------------------------
# Enums & result primitives
# ---------------------------------------------------------------------------

class CheckStatus(Enum):
    PASS = "pass"
    WARN = "warn"
    FAIL = "fail"


@dataclass
class CheckResult:
    name: str
    status: CheckStatus
    detail: str = ""
    fix_hint: str = ""

    def to_dict(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "status": self.status.value,
            "detail": self.detail,
            "fix_hint": self.fix_hint,
        }


@dataclass
class ProbeResult:
    overall: CheckStatus
    checks: List[CheckResult] = field(default_factory=list)

    # computed properties -------------------------------------------------

    @property
    def passed_count(self) -> int:
        return sum(1 for c in self.checks if c.status == CheckStatus.PASS)

    @property
    def warned_count(self) -> int:
        return sum(1 for c in self.checks if c.status == CheckStatus.WARN)

    @property
    def failed_count(self) -> int:
        return sum(1 for c in self.checks if c.status == CheckStatus.FAIL)

    # kept for backwards-compat with code referencing .passed / .warned / .failed
    @property
    def passed(self) -> int:
        return self.passed_count

    @property
    def warned(self) -> int:
        return self.warned_count

    @property
    def failed(self) -> int:
        return self.failed_count

    def to_dict(self) -> Dict[str, Any]:
        return {
            "overall": self.overall.value,
            "passed": self.passed_count,
            "warned": self.warned_count,
            "failed": self.failed_count,
            "checks": [c.to_dict() for c in self.checks],
        }

    @classmethod
    def from_checks(cls, checks: List[CheckResult]) -> "ProbeResult":
        """Build a ProbeResult and derive the overall status from the checks."""
        if any(c.status == CheckStatus.FAIL for c in checks):
            overall = CheckStatus.FAIL
        elif any(c.status == CheckStatus.WARN for c in checks):
            overall = CheckStatus.WARN
        else:
            overall = CheckStatus.PASS
        return cls(overall=overall, checks=checks)


# ---------------------------------------------------------------------------
# Target descriptor
# ---------------------------------------------------------------------------

@dataclass
class ProbeTarget:
    """Describes the environment to probe."""
    connection_type: str = "local"   # local | ssh | cloud_aws | cloud_azure | cloud_gcp | kubernetes
    host: str = "localhost"
    ssh_user: str = "root"
    ssh_key_path: str = ""
    ssh_password: str = ""
    cloud_credentials: Dict[str, Any] = field(default_factory=dict)
    kubeconfig_path: str = ""


# ---------------------------------------------------------------------------
# Probe implementation
# ---------------------------------------------------------------------------

_GB = 1024 ** 3

# Check name constants - used in CheckResult.name fields
CHECK_LOCAL_DOCKER  = "Local Docker"
CHECK_SSH           = "SSH Connectivity"
CHECK_DISK_SPACE    = "Disk Space"
CHECK_AVAILABLE_RAM = "Available RAM"
CHECK_K8S_ACCESS    = "Kubernetes Access"
CHECK_AWS_AUTH      = "AWS Auth"
CHECK_AZURE_AUTH    = "Azure Auth"
CHECK_GCP_AUTH      = "GCP Auth"


class EnvironmentProbe:
    """Collection of environment checks that can be run individually or in bulk."""

    # ------------------------------------------------------------------
    # Individual checks
    # ------------------------------------------------------------------

    def check_local_docker(self) -> CheckResult:
        """Verify Docker daemon is running and accessible on the local machine."""
        try:
            result = subprocess.run(
                ["docker", "info"],
                capture_output=True,
                timeout=15,
            )
            if result.returncode == 0:
                return CheckResult(
                    name=CHECK_LOCAL_DOCKER,
                    status=CheckStatus.PASS,
                    detail="Docker daemon is running and accessible.",
                )
            stderr = result.stderr.decode(errors="replace").strip()
            return CheckResult(
                name=CHECK_LOCAL_DOCKER,
                status=CheckStatus.FAIL,
                detail=stderr[:300] or "docker info returned non-zero exit code.",
                fix_hint="Install Docker Desktop or Docker Engine and ensure the daemon is started.",
            )
        except FileNotFoundError:
            return CheckResult(
                name=CHECK_LOCAL_DOCKER,
                status=CheckStatus.FAIL,
                detail="docker command not found.",
                fix_hint="Install Docker Desktop or Docker Engine.",
            )
        except subprocess.TimeoutExpired:
            return CheckResult(
                name=CHECK_LOCAL_DOCKER,
                status=CheckStatus.FAIL,
                detail="docker info timed out after 15 s.",
                fix_hint="Check that the Docker daemon is not hanging.",
            )

    def check_ssh_connectivity(self, target: ProbeTarget) -> CheckResult:
        """Test SSH connectivity to the target host using paramiko."""
        try:
            import paramiko  # type: ignore
        except ImportError:
            return CheckResult(
                name=CHECK_SSH,
                status=CheckStatus.WARN,
                detail="paramiko not installed — cannot test SSH connectivity.",
                fix_hint="pip install paramiko",
            )

        client = paramiko.SSHClient()
        client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        try:
            connect_kwargs: Dict[str, Any] = {
                "hostname": target.host,
                "username": target.ssh_user,
                "timeout": 10,
            }
            if target.ssh_key_path:
                connect_kwargs["key_filename"] = target.ssh_key_path
            if target.ssh_password:
                connect_kwargs["password"] = target.ssh_password

            client.connect(**connect_kwargs)
            client.close()
            return CheckResult(
                name=CHECK_SSH,
                status=CheckStatus.PASS,
                detail=f"Successfully connected to {target.host} as {target.ssh_user}.",
            )
        except paramiko.AuthenticationException:
            return CheckResult(
                name=CHECK_SSH,
                status=CheckStatus.FAIL,
                detail=f"Authentication failed for {target.ssh_user}@{target.host}.",
                fix_hint="Verify SSH key path or password.",
            )
        except (paramiko.ssh_exception.NoValidConnectionsError, OSError, socket.error) as exc:
            return CheckResult(
                name=CHECK_SSH,
                status=CheckStatus.FAIL,
                detail=f"Could not connect to {target.host}: {exc}",
                fix_hint="Check that the host is reachable and SSH is enabled.",
            )
        finally:
            try:
                client.close()
            except Exception:
                pass

    def check_disk_space(self, min_gb: int = 20) -> CheckResult:
        """Check available disk space on the local machine."""
        try:
            path = Path.home()
            usage = shutil.disk_usage(str(path))
            free_gb = usage.free / _GB
            detail = f"{free_gb:.1f} GB free on {path}"
        except Exception:
            try:
                usage = shutil.disk_usage("/")
                free_gb = usage.free / _GB
                detail = f"{free_gb:.1f} GB free on /"
            except Exception as exc:
                return CheckResult(
                    name=CHECK_DISK_SPACE,
                    status=CheckStatus.WARN,
                    detail=f"Could not determine disk usage: {exc}",
                )

        if free_gb >= min_gb:
            return CheckResult(
                name=CHECK_DISK_SPACE,
                status=CheckStatus.PASS,
                detail=detail,
            )
        if free_gb >= min_gb * 0.5:
            return CheckResult(
                name=CHECK_DISK_SPACE,
                status=CheckStatus.WARN,
                detail=f"{detail} (recommended: ≥ {min_gb} GB)",
                fix_hint=f"Free up additional disk space. Minimum required: {min_gb} GB.",
            )
        return CheckResult(
            name=CHECK_DISK_SPACE,
            status=CheckStatus.FAIL,
            detail=f"{detail} (required: ≥ {min_gb} GB)",
            fix_hint=f"Free up disk space. At least {min_gb} GB is required for a full deployment.",
        )

    def check_available_ram(self, min_gb: int = 4) -> CheckResult:
        """Check available system RAM.  Uses psutil if available, else /proc/meminfo."""

        def _ram_result(available_bytes: int) -> CheckResult:
            avail_gb = available_bytes / _GB
            detail = f"{avail_gb:.1f} GB available"
            if avail_gb >= min_gb:
                return CheckResult(name=CHECK_AVAILABLE_RAM, status=CheckStatus.PASS, detail=detail)
            if avail_gb >= min_gb * 0.5:
                return CheckResult(
                    name=CHECK_AVAILABLE_RAM,
                    status=CheckStatus.WARN,
                    detail=f"{detail} (recommended: ≥ {min_gb} GB)",
                    fix_hint="Close other applications to free RAM.",
                )
            return CheckResult(
                name=CHECK_AVAILABLE_RAM,
                status=CheckStatus.FAIL,
                detail=f"{detail} (required: ≥ {min_gb} GB)",
                fix_hint=f"At least {min_gb} GB of RAM is needed. Add more memory or reduce component count.",
            )

        # Try psutil first
        try:
            import psutil  # type: ignore
            return _ram_result(psutil.virtual_memory().available)
        except ImportError:
            pass

        # Fallback: read /proc/meminfo (Linux)
        try:
            proc_meminfo = Path("/proc/meminfo")
            if proc_meminfo.exists():
                with proc_meminfo.open() as fh:
                    for line in fh:
                        if line.startswith("MemAvailable:"):
                            kb = int(line.split()[1])
                            return _ram_result(kb * 1024)
        except Exception:
            pass

        # Fallback: sysctl on macOS / BSD
        try:
            result = subprocess.run(
                ["sysctl", "-n", "hw.memsize"],
                capture_output=True, text=True, timeout=5,
            )
            if result.returncode == 0:
                total_bytes = int(result.stdout.strip())
                # sysctl gives total; approximate available as 50 % (conservative)
                return _ram_result(total_bytes // 2)
        except Exception:
            pass

        return CheckResult(
            name=CHECK_AVAILABLE_RAM,
            status=CheckStatus.WARN,
            detail="psutil not installed and /proc/meminfo unavailable — cannot check RAM.",
            fix_hint="pip install psutil",
        )

    def check_port_available(self, port: int) -> CheckResult:
        """Check whether a TCP port is free to bind on localhost."""
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
                sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 0)
                sock.bind(("", port))
            return CheckResult(
                name=f"Port {port}",
                status=CheckStatus.PASS,
                detail=f"Port {port} is available.",
            )
        except OSError:
            return CheckResult(
                name=f"Port {port}",
                status=CheckStatus.WARN,
                detail=f"Port {port} is already in use.",
                fix_hint=f"Stop the process using port {port} or reconfigure to use a different port.",
            )

    def check_ports_available(self, ports: List[int]) -> List[CheckResult]:
        """Run check_port_available for each port in the list."""
        return [self.check_port_available(p) for p in ports]

    def check_dns_resolution(self, domain: str) -> CheckResult:
        """Verify that the domain resolves via the system DNS."""
        try:
            addr = socket.gethostbyname(domain)
            return CheckResult(
                name=f"DNS: {domain}",
                status=CheckStatus.PASS,
                detail=f"Resolved to {addr}.",
            )
        except socket.gaierror as exc:
            return CheckResult(
                name=f"DNS: {domain}",
                status=CheckStatus.FAIL,
                detail=f"Could not resolve '{domain}': {exc}",
                fix_hint="Check DNS configuration or ensure the domain is correct.",
            )

    def check_internet_access(self) -> CheckResult:
        """Verify that outbound HTTPS is reachable (pypi.org as a probe URL)."""
        try:
            urllib.request.urlopen("https://pypi.org", timeout=5)  # noqa: S310
            return CheckResult(
                name="Internet Access",
                status=CheckStatus.PASS,
                detail="Outbound HTTPS is reachable (pypi.org).",
            )
        except urllib.error.URLError:
            return CheckResult(
                name="Internet Access",
                status=CheckStatus.WARN,
                detail="Could not reach https://pypi.org.",
                fix_hint="No internet access — image pulls will fail. Use air-gapped mode or configure a pull-through cache.",
            )

    def check_kubectl_access(self, kubeconfig: str = "") -> CheckResult:
        """Test kubectl connectivity to a cluster."""
        cmd = ["kubectl", "cluster-info"]
        if kubeconfig:
            cmd += ["--kubeconfig", kubeconfig]
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                timeout=15,
            )
            if result.returncode == 0:
                return CheckResult(
                    name=CHECK_K8S_ACCESS,
                    status=CheckStatus.PASS,
                    detail="kubectl cluster-info succeeded.",
                )
            stderr = result.stderr.decode(errors="replace").strip()
            return CheckResult(
                name=CHECK_K8S_ACCESS,
                status=CheckStatus.FAIL,
                detail=stderr[:300] or "kubectl cluster-info failed.",
                fix_hint="Check that KUBECONFIG is set correctly and the cluster is reachable.",
            )
        except FileNotFoundError:
            return CheckResult(
                name=CHECK_K8S_ACCESS,
                status=CheckStatus.WARN,
                detail="kubectl not found.",
                fix_hint="Install kubectl: https://kubernetes.io/docs/tasks/tools/",
            )
        except subprocess.TimeoutExpired:
            return CheckResult(
                name=CHECK_K8S_ACCESS,
                status=CheckStatus.FAIL,
                detail="kubectl cluster-info timed out.",
                fix_hint="Check cluster API server connectivity.",
            )

    def check_cloud_auth_aws(self, credentials: Dict[str, Any]) -> CheckResult:
        """Test AWS credentials by calling STS GetCallerIdentity."""
        try:
            import boto3  # type: ignore
        except ImportError:
            return CheckResult(
                name=CHECK_AWS_AUTH,
                status=CheckStatus.WARN,
                detail="boto3 not installed — cannot validate AWS credentials.",
                fix_hint="pip install boto3",
            )
        try:
            sts_kwargs: Dict[str, Any] = {}
            if credentials.get("aws_access_key_id"):
                sts_kwargs["aws_access_key_id"] = credentials["aws_access_key_id"]
            if credentials.get("aws_secret_access_key"):
                sts_kwargs["aws_secret_access_key"] = credentials["aws_secret_access_key"]
            if credentials.get("region_name"):
                sts_kwargs["region_name"] = credentials["region_name"]
            sts = boto3.client("sts", **sts_kwargs)
            identity = sts.get_caller_identity()
            return CheckResult(
                name=CHECK_AWS_AUTH,
                status=CheckStatus.PASS,
                detail=f"Authenticated as {identity.get('Arn', 'unknown ARN')}.",
            )
        except Exception as exc:
            return CheckResult(
                name=CHECK_AWS_AUTH,
                status=CheckStatus.FAIL,
                detail=f"AWS authentication failed: {exc}",
                fix_hint="Check AWS credentials (access key, secret key, IAM permissions).",
            )

    def check_cloud_auth_azure(self, credentials: Dict[str, Any]) -> CheckResult:
        """Test Azure credentials by requesting an ARM management token."""
        try:
            from azure.identity import ClientSecretCredential  # type: ignore
        except ImportError:
            return CheckResult(
                name=CHECK_AZURE_AUTH,
                status=CheckStatus.WARN,
                detail="azure-identity not installed — cannot validate Azure credentials.",
                fix_hint="pip install azure-identity",
            )
        try:
            cred = ClientSecretCredential(
                tenant_id=credentials.get("tenant_id", ""),
                client_id=credentials.get("client_id", ""),
                client_secret=credentials.get("client_secret", ""),
            )
            token = cred.get_token("https://management.azure.com/.default")
            token_preview = token.token[:12] + "..." if token.token else "<empty>"
            return CheckResult(
                name=CHECK_AZURE_AUTH,
                status=CheckStatus.PASS,
                detail=f"Azure token acquired (preview: {token_preview}).",
            )
        except Exception as exc:
            return CheckResult(
                name=CHECK_AZURE_AUTH,
                status=CheckStatus.FAIL,
                detail=f"Azure authentication failed: {exc}",
                fix_hint="Check tenant_id, client_id, and client_secret in your Azure app registration.",
            )

    def check_cloud_auth_gcp(self, credentials: Dict[str, Any]) -> CheckResult:
        """Test GCP credentials via google-auth Application Default Credentials."""
        try:
            import google.auth  # type: ignore
            import google.auth.transport.requests  # type: ignore
        except ImportError:
            return CheckResult(
                name=CHECK_GCP_AUTH,
                status=CheckStatus.WARN,
                detail="google-auth not installed — cannot validate GCP credentials.",
                fix_hint="pip install google-auth",
            )
        try:
            key_file = credentials.get("service_account_key_file", "")
            if key_file:
                import google.oauth2.service_account as sa  # type: ignore
                creds = sa.Credentials.from_service_account_file(
                    key_file,
                    scopes=["https://www.googleapis.com/auth/cloud-platform"],
                )
            else:
                creds, _project = google.auth.default(
                    scopes=["https://www.googleapis.com/auth/cloud-platform"]
                )
            auth_req = google.auth.transport.requests.Request()
            creds.refresh(auth_req)
            return CheckResult(
                name=CHECK_GCP_AUTH,
                status=CheckStatus.PASS,
                detail="GCP credentials are valid.",
            )
        except Exception as exc:
            return CheckResult(
                name=CHECK_GCP_AUTH,
                status=CheckStatus.FAIL,
                detail=f"GCP authentication failed: {exc}",
                fix_hint="Run 'gcloud auth application-default login' or provide a service account key file.",
            )

    # ------------------------------------------------------------------
    # Bulk execution helpers
    # ------------------------------------------------------------------

    def _key_port_checks(self) -> List[CheckResult]:
        """Standard CRM port checks."""
        return self.check_ports_available([80, 443, 5000, 3306, 6379])

    def run_all(self, target: ProbeTarget) -> ProbeResult:
        """Run checks appropriate for the target connection type sequentially."""
        checks: List[CheckResult] = []

        ct = target.connection_type.lower()

        if ct == "local":
            checks.append(self.check_local_docker())
            checks.append(self.check_disk_space())
            checks.append(self.check_available_ram())
            checks.extend(self._key_port_checks())
            checks.append(self.check_internet_access())

        elif ct == "ssh":
            checks.append(self.check_ssh_connectivity(target))
            checks.append(self.check_disk_space())
            checks.append(self.check_available_ram())
            checks.extend(self._key_port_checks())

        elif ct == "cloud_aws":
            checks.append(self.check_cloud_auth_aws(target.cloud_credentials))
            checks.append(self.check_internet_access())
            if target.kubeconfig_path:
                checks.append(self.check_kubectl_access(target.kubeconfig_path))

        elif ct == "cloud_azure":
            checks.append(self.check_cloud_auth_azure(target.cloud_credentials))
            checks.append(self.check_internet_access())
            if target.kubeconfig_path:
                checks.append(self.check_kubectl_access(target.kubeconfig_path))

        elif ct == "cloud_gcp":
            checks.append(self.check_cloud_auth_gcp(target.cloud_credentials))
            checks.append(self.check_internet_access())
            if target.kubeconfig_path:
                checks.append(self.check_kubectl_access(target.kubeconfig_path))

        elif ct == "kubernetes":
            checks.append(self.check_kubectl_access(target.kubeconfig_path))
            checks.append(self.check_internet_access())

        else:
            # Unknown type: run safe local checks
            checks.append(self.check_disk_space())
            checks.append(self.check_internet_access())

        return ProbeResult.from_checks(checks)

    def run_parallel(self, target: ProbeTarget) -> ProbeResult:
        """Same check suite as run_all but executed concurrently via ThreadPoolExecutor."""
        ct = target.connection_type.lower()

        # Build callables based on connection type
        tasks = []

        if ct == "local":
            tasks = [
                self.check_local_docker,
                self.check_disk_space,
                self.check_available_ram,
                self.check_internet_access,
            ] + [lambda _p=port: self.check_port_available(_p) for port in [80, 443, 5000, 3306, 6379]]

        elif ct == "ssh":
            tasks = [
                lambda: self.check_ssh_connectivity(target),
                self.check_disk_space,
                self.check_available_ram,
            ] + [lambda _p=port: self.check_port_available(_p) for port in [80, 443, 5000, 3306, 6379]]

        elif ct == "cloud_aws":
            tasks = [
                lambda: self.check_cloud_auth_aws(target.cloud_credentials),
                self.check_internet_access,
            ]
            if target.kubeconfig_path:
                tasks.append(lambda: self.check_kubectl_access(target.kubeconfig_path))

        elif ct == "cloud_azure":
            tasks = [
                lambda: self.check_cloud_auth_azure(target.cloud_credentials),
                self.check_internet_access,
            ]
            if target.kubeconfig_path:
                tasks.append(lambda: self.check_kubectl_access(target.kubeconfig_path))

        elif ct == "cloud_gcp":
            tasks = [
                lambda: self.check_cloud_auth_gcp(target.cloud_credentials),
                self.check_internet_access,
            ]
            if target.kubeconfig_path:
                tasks.append(lambda: self.check_kubectl_access(target.kubeconfig_path))

        elif ct == "kubernetes":
            tasks = [
                lambda: self.check_kubectl_access(target.kubeconfig_path),
                self.check_internet_access,
            ]

        else:
            tasks = [self.check_disk_space, self.check_internet_access]

        checks: List[CheckResult] = []
        with ThreadPoolExecutor(max_workers=5) as executor:
            futures = {executor.submit(fn): fn for fn in tasks}
            for future in as_completed(futures):
                try:
                    result = future.result()
                    if isinstance(result, list):
                        checks.extend(result)
                    else:
                        checks.append(result)
                except Exception as exc:
                    checks.append(
                        CheckResult(
                            name="Unknown Check",
                            status=CheckStatus.WARN,
                            detail=f"Check raised an unexpected error: {exc}",
                        )
                    )

        # Stable sort: FAIL → WARN → PASS, then by name
        _order = {CheckStatus.FAIL: 0, CheckStatus.WARN: 1, CheckStatus.PASS: 2}
        checks.sort(key=lambda c: (_order[c.status], c.name))
        return ProbeResult.from_checks(checks)
