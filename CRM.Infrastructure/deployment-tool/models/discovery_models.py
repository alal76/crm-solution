#!/usr/bin/env python3
"""
CRM Solution - Deployment Discovery Module
Discover and inspect existing CRM deployments.

Author: Abhishek Lal
License: AGPL-3.0
"""

import json
import subprocess
from typing import Dict, List, Optional, Any, Tuple
from dataclasses import dataclass, asdict
from datetime import datetime
import logging
import os
import re

# ---------------------------------------------------------------------------
# Lazy / optional imports
#
# Heavy SDKs (paramiko, requests, azure-*, boto3, google-cloud-*) are NOT
# imported at module load time.  They are resolved on demand when the user
# actually invokes a feature that needs them.  The prerequisite checker
# (prerequisites.py) will offer to install missing packages at that point.
# ---------------------------------------------------------------------------

def _check_available(import_name: str) -> bool:
    """Return True if a module can be imported."""
    try:
        __import__(import_name)
        return True
    except ImportError:
        return False


def _lazy_import(module_name: str):
    """Import and return a module, raising ImportError if missing."""
    import importlib
    return importlib.import_module(module_name)


# Availability flags – evaluated lazily via properties
def _ssh_available() -> bool:
    return _check_available("paramiko")

def _requests_available() -> bool:
    return _check_available("requests")

def _azure_available() -> bool:
    # Core Azure SDK packages are sufficient — azure.mgmt.containerinstance is optional
    return (_check_available("azure.identity")
            and _check_available("azure.mgmt.compute"))

def _azure_selectable() -> bool:  # noqa: D103 — always selectable for manual config
    return True

def _aws_available() -> bool:
    return _check_available("boto3")

def _gcp_available() -> bool:
    return (_check_available(_GOOGLE_COMPUTE_MODULE)
            and _check_available(_GOOGLE_CONTAINER_MODULE))


logger = logging.getLogger(__name__)

# Module name constants (used multiple times to avoid string literal duplication)
_GOOGLE_COMPUTE_MODULE = "google.cloud.compute_v1"
_GOOGLE_CONTAINER_MODULE = "google.cloud.container_v1"


@dataclass
class DeploymentComponent:
    """Represents a deployed component."""
    name: str
    type: str  # container, vm, service, etc.
    status: str  # running, stopped, etc.
    version: Optional[str] = None
    image: Optional[str] = None
    ports: Optional[List[int]] = None
    environment: Optional[Dict[str, str]] = None
    health_url: Optional[str] = None
    last_updated: Optional[datetime] = None
    metadata: Optional[Dict[str, Any]] = None

    def __post_init__(self):
        if self.ports is None:
            self.ports = []
        if self.environment is None:
            self.environment = {}
        if self.metadata is None:
            self.metadata = {}


@dataclass
class DeploymentInfo:
    """Information about a CRM deployment."""
    platform: str  # azure, aws, gcp, on_premises
    architecture: str  # monolithic, microservices
    components: List[DeploymentComponent]
    version: Optional[str] = None
    deployment_date: Optional[datetime] = None
    environment: str = "unknown"  # development, staging, production
    health_status: str = "unknown"
    last_checked: Optional[datetime] = None
    metadata: Optional[Dict[str, Any]] = None

    def __post_init__(self):
        if self.metadata is None:
            self.metadata = {}


class DeploymentDiscoveryError(Exception):
    """Base exception for deployment discovery errors."""
    pass


class SSHConnectionError(DeploymentDiscoveryError):
    """SSH connection failed."""
    pass


class CloudAPIError(DeploymentDiscoveryError):
    """Cloud API call failed."""
    pass


class BaseDiscoveryClient:
    """Base class for deployment discovery clients."""

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover deployment components."""
        raise NotImplementedError

    def get_component_health(self, component: DeploymentComponent) -> str:
        """Get health status of a component."""
        if component.health_url:
            try:
                requests = _lazy_import("requests")
                response = requests.get(component.health_url, timeout=10)
                if response.status_code == 200:
                    return "healthy"
                else:
                    return f"unhealthy ({response.status_code})"
            except Exception as e:
                return f"error: {str(e)}"
        return "unknown"


class SSHDiscoveryClient(BaseDiscoveryClient):
    """SSH-based discovery for on-premises deployments."""

    def __init__(self):
        self.ssh_client = None
        self._paramiko = None

    def _get_paramiko(self):
        """Lazy-load paramiko, offering installation if missing."""
        if self._paramiko is None:
            try:
                self._paramiko = _lazy_import("paramiko")
            except ImportError:
                # Attempt on-demand install via prerequisites module
                try:
                    from prerequisites import ensure_group_installed
                    if ensure_group_installed("ssh"):
                        self._paramiko = _lazy_import("paramiko")
                    else:
                        raise ImportError(
                            "paramiko is required for SSH discovery. "
                            "Install with: pip install paramiko"
                        )
                except ImportError:
                    raise ImportError(
                        "paramiko is required for SSH discovery. "
                        "Install with: pip install paramiko"
                    )
        return self._paramiko

    def connect(self, hostname: str, username: str, password: str = None,
                key_path: str = None, port: int = 22) -> None:
        """Establish SSH connection.

        When *key_path* is provided the key file is used for authentication.
        If *password* is also set it is treated as the key passphrase.
        When only *password* is provided, password authentication is used.
        When neither is provided, paramiko auto-discovers keys in ~/.ssh.
        """
        try:
            paramiko = self._get_paramiko()
            self.ssh_client = paramiko.SSHClient()
            self.ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())

            _auto = not key_path and not password
            self.ssh_client.connect(
                hostname,
                port=port,
                username=username,
                # When key_path set: password is the key passphrase (may be None/empty).
                # When key_path not set: password is the auth password (may be None).
                password=password or None,
                key_filename=key_path or None,
                look_for_keys=_auto,
                allow_agent=_auto,
            )

            logger.info(f"SSH connection established to {hostname}")
        except Exception as e:
            raise SSHConnectionError(f"Failed to connect to {hostname}: {str(e)}")

    def disconnect(self):
        """Close SSH connection."""
        if self.ssh_client:
            self.ssh_client.close()
            self.ssh_client = None

    def run_command(self, command: str) -> Tuple[str, str, int]:
        """Run command via SSH."""
        if not self.ssh_client:
            raise SSHConnectionError("No SSH connection established")

        try:
            _, stdout, stderr = self.ssh_client.exec_command(command)
            output = stdout.read().decode('utf-8')
            error = stderr.read().decode('utf-8')
            return output, error, stdout.channel.recv_exit_status()
        except Exception as e:
            raise SSHConnectionError(f"Command execution failed: {str(e)}")

    def discover_docker_containers(self, remote_host: str = 'localhost', container_prefix: str = 'crm-') -> List[DeploymentComponent]:
        """Discover Docker containers matching the given prefix (default: crm-)."""
        containers = self._list_containers()
        return [
            self._build_component(c, remote_host)
            for c in containers
            if c.get('Names', '').startswith(container_prefix)
        ]

    def _list_containers(self) -> List[Dict[str, Any]]:
        """Return a list of running container dicts from `docker ps`."""
        output, error, code = self.run_command("docker ps --format json")
        if code != 0:
            logger.warning(f"Docker command failed: {error}")
            return []
        try:
            return [json.loads(line) for line in output.strip().split('\n') if line.strip()]
        except json.JSONDecodeError:
            return self._list_containers_table()

    def _list_containers_table(self) -> List[Dict[str, Any]]:
        """Fallback: parse containers from `docker ps` table output."""
        output, _, code = self.run_command(
            "docker ps --format 'table {{.Names}}\\t{{.Image}}\\t{{.Ports}}\\t{{.Status}}'"
        )
        if code != 0:
            return []
        containers = []
        for line in output.strip().split('\n')[1:]:  # Skip header
            parts = line.split('\t')
            if len(parts) >= 4:
                containers.append({'Names': parts[0], 'Image': parts[1],
                                   'Ports': parts[2], 'Status': parts[3]})
        return containers

    def _inspect_container(self, name: str) -> Dict[str, Any]:
        """Return `docker inspect` output for a single container."""
        details_output, _, _ = self.run_command(f"docker inspect {name}")
        try:
            return json.loads(details_output)[0] if details_output else {}
        except Exception:
            return {}

    @staticmethod
    def _extract_ports(details: Dict[str, Any]) -> List[int]:
        """Extract host-bound port numbers from inspect details."""
        ports: List[int] = []
        for bindings in details.get('HostConfig', {}).get('PortBindings', {}).values():
            if bindings:
                for binding in bindings:
                    ports.append(int(binding.get('HostPort', 0)))
        return ports

    @staticmethod
    def _extract_env(details: Dict[str, Any]) -> Dict[str, str]:
        """Extract environment variables from inspect details."""
        env_vars: Dict[str, str] = {}
        for env in details.get('Config', {}).get('Env', []):
            if '=' in env:
                key, value = env.split('=', 1)
                env_vars[key] = value
        return env_vars

    @staticmethod
    def _classify_container(name: str, ports: List[int], remote_host: str) -> tuple:
        """Return (component_type, health_url) for a container by name."""
        if 'api' in name:
            return "api", (f"http://{remote_host}:{ports[0]}/health" if ports else None)
        if 'frontend' in name:
            return "frontend", (f"http://{remote_host}:{ports[0]}" if ports else None)
        type_map = {
            'mariadb': "database", 'mysql': "database",
            'redis': "cache", 'meilisearch': "search",
            'chatwoot': "chat", 'novu': "notifications",
            'superset': "analytics", 'docuseal': "signatures",
            'ollama': "ai", 'n8n': "integrations",
        }
        for keyword, ctype in type_map.items():
            if keyword in name:
                return ctype, None
        return "unknown", None

    def _build_component(self, container: Dict[str, Any], remote_host: str) -> DeploymentComponent:
        """Build a DeploymentComponent from a container dict."""
        name = container.get('Names', '')
        details = self._inspect_container(name)
        ports = self._extract_ports(details)
        env_vars = self._extract_env(details)
        component_type, health_url = self._classify_container(name, ports, remote_host)
        image = container.get('Image', '')
        version = image.split(':')[-1] if ':' in image else None
        return DeploymentComponent(
            name=name,
            type=component_type,
            status="running",
            version=version,
            image=image,
            ports=ports,
            environment=env_vars,
            health_url=health_url,
            metadata={
                "container_id": details.get('Id', ''),
                "created": details.get('Created', ''),
                "labels": details.get('Config', {}).get('Labels', {})
            }
        )

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover on-premises deployment."""
        hostname = config.get('host') or config.get('hostname', '')
        if not hostname:
            raise DeploymentDiscoveryError("Host/IP address is required for on-premises discovery")
        username = config.get('username', 'root') or 'root'
        password = config.get('password')
        key_path = config.get('key_path')
        port = int(config.get('port', 22) or 22)
        self.connect(hostname, username, password, key_path, port)
        try:
            return self._build_deployment_info(config, hostname)
        finally:
            self.disconnect()

    def _build_deployment_info(self, config: Dict[str, Any], hostname: str) -> DeploymentInfo:
        """Build DeploymentInfo after SSH connection is established."""
        container_prefix = config.get('container_prefix', 'crm-')
        components = self.discover_docker_containers(remote_host=hostname, container_prefix=container_prefix)
        architecture = "microservices" if len({c.name for c in components}) > 3 else "monolithic"
        _requests = _lazy_import("requests")
        version = self._detect_version(components, _requests)
        overall_health = self._compute_health(components, _requests)
        environment = self._detect_environment(components)
        return DeploymentInfo(
            platform="on_premises",
            architecture=architecture,
            components=components,
            version=version,
            environment=environment,
            health_status=overall_health,
            last_checked=datetime.now()
        )

    @staticmethod
    def _detect_version(components: List[DeploymentComponent], requests_module) -> Optional[str]:
        """Try to read the app version from the API health endpoint."""
        api_component = next((c for c in components if c.type == "api"), None)
        if not api_component or not api_component.health_url:
            return None
        for version_path in ["/version", "", "/api/version"]:
            try:
                resp = requests_module.get(f"{api_component.health_url}{version_path}", timeout=5)
                if resp.status_code == 200:
                    data = resp.json()
                    version = data.get('version') or data.get('Version') or data.get('appVersion')
                    if version:
                        return version
            except Exception:
                pass
        return None

    @staticmethod
    def _compute_health(components: List[DeploymentComponent], requests_module) -> str:
        """Determine overall deployment health from HTTP health-check URLs."""
        health_checks = []
        for comp in components:
            if comp.health_url:
                try:
                    resp = requests_module.get(comp.health_url, timeout=5)
                    health_checks.append(resp.status_code == 200)
                except Exception:
                    health_checks.append(False)
        if not health_checks:
            return "healthy" if components else "unknown"
        passed = sum(health_checks)
        if passed == len(health_checks):
            return "healthy"
        return "degraded" if passed > 0 else "unhealthy"

    @staticmethod
    def _detect_environment(components: List[DeploymentComponent]) -> str:
        """Determine deployment environment from container env vars."""
        for comp in components:
            if comp.environment and comp.environment.get('ASPNETCORE_ENVIRONMENT') == 'Development':
                return "development"
        return "production"


class AzureDiscoveryClient(BaseDiscoveryClient):
    """Azure-based deployment discovery."""

    def __init__(self):
        if not _azure_available():
            try:
                from prerequisites import ensure_group_installed
                if not ensure_group_installed("azure"):
                    raise ImportError("Azure SDK not available. Install with: pip install azure-identity azure-mgmt-compute")
            except ImportError as orig:
                raise ImportError("Azure SDK not available. Install with: pip install azure-identity azure-mgmt-compute") from orig

    def _build_credential(self, config: Dict[str, Any]):
        """Build Azure credential from explicit config or fall back to DefaultAzureCredential."""
        azure_identity = _lazy_import("azure.identity")
        use_cli     = config.get('use_cli_auth', False)
        tenant_id   = config.get('tenant_id', '').strip()
        client_id   = config.get('client_id', '').strip()
        client_secret = config.get('client_secret', '').strip()

        if use_cli:
            return azure_identity.AzureCliCredential()
        if client_id and client_secret and tenant_id:
            return azure_identity.ClientSecretCredential(
                tenant_id=tenant_id,
                client_id=client_id,
                client_secret=client_secret
            )
        # Fall back to DefaultAzureCredential (env vars, MSI, CLI, etc.)
        return azure_identity.DefaultAzureCredential()

    def test_connection(self, config: Dict[str, Any]) -> Dict[str, Any]:
        """Test Azure connection by requesting a management token."""
        subscription_id = config.get('subscription_id', '').strip()
        use_cli = config.get('use_cli_auth', False)

        # When using CLI auth and no subscription provided, auto-detect from az account show
        if not subscription_id and use_cli:
            try:
                import subprocess as _sp
                _j = __import__('json')
                r = _sp.run(["az", "account", "show", "--output", "json"],
                            capture_output=True, text=True, timeout=10)
                if r.returncode == 0:
                    subscription_id = _j.loads(r.stdout).get('id', '').strip()
            except Exception:
                pass

        if not subscription_id:
            return {"status": "error", "message": "Azure Subscription ID is required — select one from the dropdown or enter it manually."}
        try:
            credential = self._build_credential(config)
            # Validate by requesting a token — fails immediately if creds are wrong
            token = credential.get_token("https://management.azure.com/.default")
            if not token or not token.token:
                return {"status": "error", "message": "Failed to acquire Azure auth token"}
            return {"status": "success", "message": f"Azure connection successful (subscription: {subscription_id})"}
        except Exception as exc:
            return {"status": "error", "message": str(exc)}

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover Azure deployment, auto-detecting subscription/resource group when omitted."""
        subscription_id = self._resolve_subscription_id(config)
        credential = self._build_credential(config)
        resource_groups = self._resolve_resource_groups(
            config, credential, subscription_id
        )
        if not resource_groups:
            raise CloudAPIError(
                "Could not determine any resource groups in this subscription. "
                "Please specify a Resource Group in the form or ensure the account has access."
            )
        components: List[DeploymentComponent] = []
        azure_compute = _lazy_import("azure.mgmt.compute")
        compute_client = azure_compute.ComputeManagementClient(credential, subscription_id)
        # azure.mgmt.containerinstance is optional — ACI scanning skipped if not installed
        container_client = None
        try:
            azure_container = _lazy_import("azure.mgmt.containerinstance")
            container_client = azure_container.ContainerInstanceManagementClient(credential, subscription_id)
        except ImportError:
            logger.warning(
                "azure.mgmt.containerinstance not installed — Azure Container Instance "
                "discovery skipped. Install it with: pip install azure-mgmt-containerinstance"
            )
        for rg in resource_groups:
            components += self._scan_resource_group(compute_client, container_client, rg)
        architecture = "microservices" if len(components) > 3 else "monolithic"
        return DeploymentInfo(
            platform="azure", architecture=architecture,
            components=components, last_checked=datetime.now()
        )

    @staticmethod
    def _resolve_subscription_id(config: Dict[str, Any]) -> str:
        """Return subscription_id from config or auto-detect via `az account show`."""
        import subprocess as _sp
        import json as _json
        subscription_id = config.get('subscription_id', '').strip()
        if not subscription_id:
            try:
                r = _sp.run(["az", "account", "show", "--output", "json"],
                            capture_output=True, text=True, timeout=10)
                if r.returncode == 0:
                    subscription_id = _json.loads(r.stdout).get('id', '').strip()
            except Exception:
                pass
        if not subscription_id:
            raise CloudAPIError(
                "Azure Subscription ID is required — select one from the dropdown or run `az login` first."
            )
        return subscription_id

    @staticmethod
    def _resolve_resource_groups(config: Dict[str, Any], credential,
                                  subscription_id: str) -> List[str]:
        """Return the list of resource groups to scan, auto-detecting when not specified."""
        import subprocess as _sp
        import json as _json
        resource_group = config.get('resource_group', '').strip()
        if resource_group:
            return [resource_group]
        # Try Azure CLI first (fast, no extra SDK)
        try:
            r = _sp.run(
                ["az", "group", "list", "--subscription", subscription_id,
                 "--output", "json", "--query", "[].name"],
                capture_output=True, text=True, timeout=20
            )
            if r.returncode == 0:
                groups = _json.loads(r.stdout) or []
                if groups:
                    return groups
        except Exception:
            pass
        # Fallback to SDK
        try:
            azure_resources = _lazy_import("azure.mgmt.resource")
            rm_client = azure_resources.ResourceManagementClient(credential, subscription_id)
            return [rg.name for rg in rm_client.resource_groups.list()]
        except Exception:
            return []

    @staticmethod
    def _scan_vms_in_rg(compute_client, rg: str) -> List[DeploymentComponent]:
        """Discover CRM virtual machines in a single resource group."""
        out: List[DeploymentComponent] = []
        try:
            for vm in compute_client.virtual_machines.list(rg):
                if 'crm' in vm.name.lower():
                    out.append(DeploymentComponent(
                        name=vm.name, type="vm", status="running",
                        metadata={"vm_size": vm.hardware_profile.vm_size,
                                  "location": vm.location,
                                  "os_type": vm.storage_profile.os_disk.os_type,
                                  "resource_group": rg}
                    ))
        except Exception:
            pass
        return out

    @staticmethod
    def _scan_containers_in_rg(container_client, rg: str) -> List[DeploymentComponent]:
        """Discover CRM container instances in a single resource group."""
        out: List[DeploymentComponent] = []
        if container_client is None:
            return out
        try:
            for cg in container_client.container_groups.list_by_resource_group(rg):
                if 'crm' in cg.name.lower():
                    for c in cg.containers:
                        out.append(DeploymentComponent(
                            name=f"{cg.name}/{c.name}", type="container", status="running",
                            image=c.image,
                            ports=[p.port for p in c.ports] if c.ports else [],
                            metadata={"resource_group": rg}
                        ))
        except Exception:
            pass
        return out

    @staticmethod
    def _scan_resource_group(compute_client, container_client,
                              rg: str) -> List[DeploymentComponent]:
        """Discover CRM VMs and container instances in a single resource group."""
        return (
            AzureDiscoveryClient._scan_vms_in_rg(compute_client, rg)
            + AzureDiscoveryClient._scan_containers_in_rg(container_client, rg)
        )


class AWSDiscoveryClient(BaseDiscoveryClient):
    """AWS-based deployment discovery."""

    def __init__(self):
        if not _aws_available():
            try:
                from prerequisites import ensure_group_installed
                if not ensure_group_installed("aws"):
                    raise ImportError("AWS SDK not available. Install with: pip install boto3")
            except ImportError as orig:
                raise ImportError("AWS SDK not available. Install with: pip install boto3") from orig

    def _build_session(self, config: Dict[str, Any]):
        """Build boto3 session from explicit credentials or CLI profile."""
        boto3 = _lazy_import("boto3")
        region      = config.get('region', 'us-east-1')
        use_profile = config.get('use_profile', False)
        access_key  = config.get('access_key_id', '').strip()
        secret_key  = config.get('secret_access_key', '').strip()
        session_tok = config.get('session_token', '').strip() or None

        if use_profile or not access_key:
            return boto3.Session(region_name=region)
        return boto3.Session(
            aws_access_key_id=access_key,
            aws_secret_access_key=secret_key,
            aws_session_token=session_tok,
            region_name=region
        )

    def test_connection(self, config: Dict[str, Any]) -> Dict[str, Any]:
        """Test AWS connection using STS get-caller-identity."""
        try:
            session = self._build_session(config)
            sts = session.client('sts')
            identity = sts.get_caller_identity()
            return {"status": "success", "message": f"AWS connection successful — Account: {identity.get('Account', 'unknown')}"}
        except Exception as exc:
            return {"status": "error", "message": str(exc)}

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover AWS deployment."""
        region  = config.get('region', 'us-east-1')
        session = self._build_session(config)
        components: List[DeploymentComponent] = []
        components += self._discover_ec2(session, region)
        components += self._discover_ecs(session, region)
        components += self._discover_rds(session, region)
        architecture = "microservices" if len(components) > 3 else "monolithic"
        return DeploymentInfo(
            platform="aws", architecture=architecture,
            components=components, last_checked=datetime.now()
        )

    @staticmethod
    def _discover_ec2(session, region: str) -> List[DeploymentComponent]:
        ec2 = session.client('ec2', region_name=region)
        response = ec2.describe_instances(Filters=[{'Name': 'tag:Name', 'Values': ['*crm*']}])
        out = []
        for reservation in response['Reservations']:
            for instance in reservation['Instances']:
                if instance['State']['Name'] == 'running':
                    out.append(DeploymentComponent(
                        name=instance.get('Tags', [{}])[0].get('Value', instance['InstanceId']),
                        type="vm", status="running",
                        metadata={"instance_type": instance['InstanceType'],
                                  "instance_id": instance['InstanceId'],
                                  "availability_zone": instance['Placement']['AvailabilityZone']}
                    ))
        return out

    @staticmethod
    def _discover_ecs(session, region: str) -> List[DeploymentComponent]:
        ecs = session.client('ecs', region_name=region)
        out = []
        for cluster_arn in ecs.list_clusters()['clusterArns']:
            if 'crm' not in cluster_arn:
                continue
            for service_arn in ecs.list_services(cluster=cluster_arn)['serviceArns']:
                svc = ecs.describe_services(cluster=cluster_arn, services=[service_arn])['services'][0]
                out.append(DeploymentComponent(
                    name=svc['serviceName'], type="service", status=svc['status'].lower(),
                    metadata={"cluster": cluster_arn, "task_definition": svc['taskDefinition'],
                              "desired_count": svc['desiredCount'], "running_count": svc['runningCount']}
                ))
        return out

    @staticmethod
    def _discover_rds(session, region: str) -> List[DeploymentComponent]:
        rds = session.client('rds', region_name=region)
        out = []
        for db in rds.describe_db_instances()['DBInstances']:
            if 'crm' in db['DBInstanceIdentifier']:
                out.append(DeploymentComponent(
                    name=db['DBInstanceIdentifier'], type="database", status=db['DBInstanceStatus'],
                    metadata={"engine": db['Engine'], "engine_version": db['EngineVersion'],
                              "instance_class": db['DBInstanceClass']}
                ))
        return out


class GCPDiscoveryClient(BaseDiscoveryClient):
    """GCP-based deployment discovery."""

    def __init__(self):
        if not _gcp_available():
            try:
                from prerequisites import ensure_group_installed
                if not ensure_group_installed("gcp"):
                    raise ImportError("GCP SDK not available. Install with: pip install google-cloud-compute google-cloud-container")
            except ImportError as orig:
                raise ImportError("GCP SDK not available. Install with: pip install google-cloud-compute google-cloud-container") from orig

    def _build_credentials(self, config: Dict[str, Any]):
        """Build GCP credentials from service-account JSON or fall back to ADC."""
        use_adc = config.get('use_adc', False)
        sa_json = config.get('service_account_json', '').strip()
        if use_adc or not sa_json:
            return None  # google-auth will pick up ADC automatically
        import json as _json
        google_sa = _lazy_import("google.oauth2.service_account")
        sa_info = _json.loads(sa_json)
        return google_sa.Credentials.from_service_account_info(
            sa_info,
            scopes=["https://www.googleapis.com/auth/cloud-platform"]
        )

    def test_connection(self, config: Dict[str, Any]) -> Dict[str, Any]:
        """Test GCP connection by listing compute zones."""
        project_id = config.get('project_id', '').strip()
        if not project_id:
            return {"status": "error", "message": "GCP Project ID is required"}
        try:
            credentials = self._build_credentials(config)
            _compute_v1 = _lazy_import(_GOOGLE_COMPUTE_MODULE)
            zones_client = _compute_v1.ZonesClient(credentials=credentials)
            next(iter(zones_client.list(project=project_id)), None)
            return {"status": "success", "message": f"GCP connection successful — Project: {project_id}"}
        except Exception as exc:
            return {"status": "error", "message": str(exc)}

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover GCP deployment."""
        project_id  = config.get('project_id', '').strip()
        zone        = config.get('region', 'us-central1') + '-a'
        if not project_id:
            raise CloudAPIError("GCP project_id required")
        credentials = self._build_credentials(config)
        components: List[DeploymentComponent] = []
        components += self._discover_gce(credentials, project_id, zone)
        components += self._discover_gke(credentials, project_id, zone)
        architecture = "microservices" if len(components) > 3 else "monolithic"
        return DeploymentInfo(
            platform="gcp", architecture=architecture,
            components=components, last_checked=datetime.now()
        )

    @staticmethod
    def _discover_gce(credentials, project_id: str, zone: str) -> List[DeploymentComponent]:
        _compute_v1 = _lazy_import(_GOOGLE_COMPUTE_MODULE)
        compute_client = _compute_v1.InstancesClient(credentials=credentials)
        request = _compute_v1.ListInstancesRequest(project=project_id, zone=zone)
        out = []
        for instance in compute_client.list(request):
            if 'crm' in instance.name.lower():
                out.append(DeploymentComponent(
                    name=instance.name, type="vm", status="running",
                    metadata={"machine_type": instance.machine_type.split('/')[-1],
                              "zone": zone, "status": instance.status}
                ))
        return out

    @staticmethod
    def _discover_gke(credentials, project_id: str, zone: str) -> List[DeploymentComponent]:
        _container_v1 = _lazy_import(_GOOGLE_CONTAINER_MODULE)
        container_client = _container_v1.ClusterManagerClient(credentials=credentials)
        clusters = container_client.list_clusters(project_id=project_id, zone=zone)
        out = []
        for cluster in clusters.clusters:
            if 'crm' in cluster.name.lower():
                out.append(DeploymentComponent(
                    name=cluster.name, type="kubernetes", status="running",
                    metadata={"location": cluster.location,
                              "node_count": cluster.current_node_count,
                              "version": cluster.current_master_version}
                ))
        return out


class DeploymentDiscoveryManager:
    """Main manager for deployment discovery across platforms."""

    def __init__(self):
        self.clients = {
            'on_premises': SSHDiscoveryClient,
            'azure': AzureDiscoveryClient,
            'aws': AWSDiscoveryClient,
            'gcp': GCPDiscoveryClient
        }

    def discover_deployment(self, platform: str, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover deployment on specified platform."""
        if platform not in self.clients:
            raise ValueError(f"Unsupported platform: {platform}")

        client_class = self.clients[platform]
        client = client_class()

        try:
            return client.discover_deployment(config)
        except Exception as e:
            logger.error(f"Discovery failed for {platform}: {str(e)}")
            raise DeploymentDiscoveryError(f"Discovery failed: {str(e)}")

    def get_available_platforms(self) -> List[str]:
        """Get list of available platforms."""
        return list(self.clients.keys())

    def check_platform_availability(self, platform: str) -> bool:
        """Check if platform dependencies are available."""
        if platform == 'on_premises':
            return _ssh_available()
        elif platform == 'azure':
            return _azure_available()
        elif platform == 'aws':
            return _aws_available()
        elif platform == 'gcp':
            return _gcp_available()
        return False


# Global discovery manager instance
discovery_manager = DeploymentDiscoveryManager()