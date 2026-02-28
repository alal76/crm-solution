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
    return (_check_available("azure.identity")
            and _check_available("azure.mgmt.compute")
            and _check_available("azure.mgmt.containerinstance"))

def _aws_available() -> bool:
    return _check_available("boto3")

def _gcp_available() -> bool:
    return (_check_available("google.cloud.compute_v1")
            and _check_available("google.cloud.container_v1"))


logger = logging.getLogger(__name__)


@dataclass
class DeploymentComponent:
    """Represents a deployed component."""
    name: str
    type: str  # container, vm, service, etc.
    status: str  # running, stopped, etc.
    version: Optional[str] = None
    image: Optional[str] = None
    ports: List[int] = None
    environment: Dict[str, str] = None
    health_url: Optional[str] = None
    last_updated: Optional[datetime] = None
    metadata: Dict[str, Any] = None

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
    metadata: Dict[str, Any] = None

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
        """Establish SSH connection."""
        try:
            paramiko = self._get_paramiko()
            self.ssh_client = paramiko.SSHClient()
            self.ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())

            if key_path:
                key = paramiko.RSAKey.from_private_key_file(key_path)
                self.ssh_client.connect(hostname, port=port, username=username, pkey=key)
            else:
                self.ssh_client.connect(hostname, port=port, username=username, password=password)

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
            stdin, stdout, stderr = self.ssh_client.exec_command(command)
            output = stdout.read().decode('utf-8')
            error = stderr.read().decode('utf-8')
            return output, error, stdout.channel.recv_exit_status()
        except Exception as e:
            raise SSHConnectionError(f"Command execution failed: {str(e)}")

    def discover_docker_containers(self, remote_host: str = 'localhost', container_prefix: str = 'crm-') -> List[DeploymentComponent]:
        """Discover Docker containers matching the given prefix (default: crm-)."""
        components = []

        # Get running containers
        output, error, code = self.run_command("docker ps --format json")
        if code != 0:
            logger.warning(f"Docker command failed: {error}")
            return components

        try:
            containers = [json.loads(line) for line in output.strip().split('\n') if line.strip()]
        except json.JSONDecodeError:
            # Fallback to table format
            output, error, code = self.run_command("docker ps --format 'table {{.Names}}\\t{{.Image}}\\t{{.Ports}}\\t{{.Status}}'")
            if code != 0:
                return components

            lines = output.strip().split('\n')[1:]  # Skip header
            containers = []
            for line in lines:
                parts = line.split('\t')
                if len(parts) >= 4:
                    containers.append({
                        'Names': parts[0],
                        'Image': parts[1],
                        'Ports': parts[2],
                        'Status': parts[3]
                    })

        for container in containers:
            name = container.get('Names', '')
            if not name.startswith(container_prefix):
                continue

            # Get container details
            details_output, _, _ = self.run_command(f"docker inspect {name}")
            details = {}
            try:
                details = json.loads(details_output)[0] if details_output else {}
            except:
                pass

            # Extract ports
            ports = []
            port_bindings = details.get('HostConfig', {}).get('PortBindings', {})
            for container_port, bindings in port_bindings.items():
                if bindings:
                    for binding in bindings:
                        ports.append(int(binding.get('HostPort', 0)))

            # Extract environment variables
            env_vars = {}
            env_list = details.get('Config', {}).get('Env', [])
            for env in env_list:
                if '=' in env:
                    key, value = env.split('=', 1)
                    env_vars[key] = value

            # Determine component type and health URL
            component_type = "unknown"
            health_url = None

            if 'api' in name:
                component_type = "api"
                health_url = f"http://{remote_host}:{ports[0]}/health" if ports else None
            elif 'frontend' in name:
                component_type = "frontend"
                health_url = f"http://{remote_host}:{ports[0]}" if ports else None
            elif 'mariadb' in name or 'mysql' in name:
                component_type = "database"
            elif 'redis' in name:
                component_type = "cache"
            elif 'meilisearch' in name:
                component_type = "search"
            elif 'chatwoot' in name:
                component_type = "chat"
            elif 'novu' in name:
                component_type = "notifications"
            elif 'superset' in name:
                component_type = "analytics"
            elif 'docuseal' in name:
                component_type = "signatures"
            elif 'ollama' in name:
                component_type = "ai"
            elif 'n8n' in name:
                component_type = "integrations"

            # Extract version from image
            image = container.get('Image', '')
            version = None
            if ':' in image:
                version = image.split(':')[-1]

            component = DeploymentComponent(
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
            components.append(component)

        return components

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover on-premises deployment."""
        # Accept 'host' (frontend key) or 'hostname' (legacy key)
        hostname = config.get('host') or config.get('hostname', '')
        if not hostname:
            raise DeploymentDiscoveryError("Host/IP address is required for on-premises discovery")
        username = config.get('username', 'root') or 'root'
        password = config.get('password')
        key_path = config.get('key_path')
        port = int(config.get('port', 22) or 22)

        self.connect(hostname, username, password, key_path, port)

        try:
            container_prefix = config.get('container_prefix', 'crm-')
            components = self.discover_docker_containers(remote_host=hostname, container_prefix=container_prefix)

            # Determine architecture
            service_names = {comp.name for comp in components}
            architecture = "microservices" if len(service_names) > 3 else "monolithic"

            # Try to determine version from API component's health endpoint
            version = None
            _requests = _lazy_import("requests")
            api_component = next((c for c in components if c.type == "api"), None)
            if api_component and api_component.health_url:
                for version_path in ["/version", "", "/api/version"]:
                    try:
                        resp = _requests.get(f"{api_component.health_url}{version_path}", timeout=5)
                        if resp.status_code == 200:
                            data = resp.json()
                            version = data.get('version') or data.get('Version') or data.get('appVersion')
                            if version:
                                break
                    except Exception:
                        pass

            # Compute overall health from components that have health URLs
            health_checks = []
            for comp in components:
                if comp.health_url:
                    try:
                        resp = _requests.get(comp.health_url, timeout=5)
                        health_checks.append(resp.status_code == 200)
                    except Exception:
                        health_checks.append(False)
            if health_checks:
                passed = sum(health_checks)
                if passed == len(health_checks):
                    overall_health = "healthy"
                elif passed > 0:
                    overall_health = "degraded"
                else:
                    overall_health = "unhealthy"
            else:
                # No HTTP health URLs available — infer from container running state
                overall_health = "healthy" if components else "unknown"

            # Determine environment
            environment = "production"  # Default assumption
            for comp in components:
                if comp.environment.get('ASPNETCORE_ENVIRONMENT') == 'Development':
                    environment = "development"
                    break

            return DeploymentInfo(
                platform="on_premises",
                architecture=architecture,
                components=components,
                version=version,
                environment=environment,
                health_status=overall_health,
                last_checked=datetime.now()
            )

        finally:
            self.disconnect()


class AzureDiscoveryClient(BaseDiscoveryClient):
    """Azure-based deployment discovery."""

    def __init__(self):
        if not _azure_available():
            # Attempt on-demand install
            try:
                from prerequisites import ensure_group_installed
                if not ensure_group_installed("azure"):
                    raise ImportError("Azure SDK not available. Install with: pip install azure-identity azure-mgmt-compute azure-mgmt-containerinstance")
            except ImportError as orig:
                raise ImportError("Azure SDK not available. Install with: pip install azure-identity azure-mgmt-compute azure-mgmt-containerinstance") from orig
        azure_identity = _lazy_import("azure.identity")
        self.credential = azure_identity.DefaultAzureCredential()

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover Azure deployment."""
        subscription_id = config.get('subscription_id')
        resource_group = config.get('resource_group')
        location = config.get('location')

        if not subscription_id or not resource_group:
            raise CloudAPIError("Azure subscription_id and resource_group required")

        components = []

        # Discover VMs
        azure_compute = _lazy_import("azure.mgmt.compute")
        compute_client = azure_compute.ComputeManagementClient(self.credential, subscription_id)
        vms = compute_client.virtual_machines.list(resource_group)

        for vm in vms:
            if 'crm' in vm.name.lower():
                component = DeploymentComponent(
                    name=vm.name,
                    type="vm",
                    status="running",  # Assume running if found
                    metadata={
                        "vm_size": vm.hardware_profile.vm_size,
                        "location": vm.location,
                        "os_type": vm.storage_profile.os_disk.os_type
                    }
                )
                components.append(component)

        # Discover Container Instances
        azure_container = _lazy_import("azure.mgmt.containerinstance")
        container_client = azure_container.ContainerInstanceManagementClient(self.credential, subscription_id)
        containers = container_client.container_groups.list_by_resource_group(resource_group)

        for container_group in containers:
            if 'crm' in container_group.name.lower():
                for container in container_group.containers:
                    component = DeploymentComponent(
                        name=f"{container_group.name}/{container.name}",
                        type="container",
                        status="running",
                        image=container.image,
                        ports=[port.port for port in container.ports] if container.ports else []
                    )
                    components.append(component)

        # Determine architecture
        architecture = "microservices" if len(components) > 3 else "monolithic"

        return DeploymentInfo(
            platform="azure",
            architecture=architecture,
            components=components,
            last_checked=datetime.now()
        )


class AWSDiscoveryClient(BaseDiscoveryClient):
    """AWS-based deployment discovery."""

    def __init__(self):
        if not _aws_available():
            # Attempt on-demand install
            try:
                from prerequisites import ensure_group_installed
                if not ensure_group_installed("aws"):
                    raise ImportError("AWS SDK not available. Install with: pip install boto3")
            except ImportError as orig:
                raise ImportError("AWS SDK not available. Install with: pip install boto3") from orig
        _boto3 = _lazy_import("boto3")
        self.ec2_client = _boto3.client('ec2')
        self.ecs_client = _boto3.client('ecs')
        self.rds_client = _boto3.client('rds')

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover AWS deployment."""
        region = config.get('region', 'us-east-1')

        # Update clients with region
        _boto3 = _lazy_import("boto3")
        self.ec2_client = _boto3.client('ec2', region_name=region)
        self.ecs_client = _boto3.client('ecs', region_name=region)
        self.rds_client = _boto3.client('rds', region_name=region)

        components = []

        # Discover EC2 instances
        response = self.ec2_client.describe_instances(
            Filters=[{'Name': 'tag:Name', 'Values': ['*crm*']}]
        )

        for reservation in response['Reservations']:
            for instance in reservation['Instances']:
                if instance['State']['Name'] == 'running':
                    component = DeploymentComponent(
                        name=instance.get('Tags', [{}])[0].get('Value', instance['InstanceId']),
                        type="vm",
                        status="running",
                        metadata={
                            "instance_type": instance['InstanceType'],
                            "instance_id": instance['InstanceId'],
                            "availability_zone": instance['Placement']['AvailabilityZone']
                        }
                    )
                    components.append(component)

        # Discover ECS services
        clusters = self.ecs_client.list_clusters()['clusterArns']
        for cluster_arn in clusters:
            if 'crm' in cluster_arn:
                services = self.ecs_client.list_services(cluster=cluster_arn)['serviceArns']
                for service_arn in services:
                    service = self.ecs_client.describe_services(
                        cluster=cluster_arn,
                        services=[service_arn]
                    )['services'][0]

                    component = DeploymentComponent(
                        name=service['serviceName'],
                        type="service",
                        status=service['status'].lower(),
                        metadata={
                            "cluster": cluster_arn,
                            "task_definition": service['taskDefinition'],
                            "desired_count": service['desiredCount'],
                            "running_count": service['runningCount']
                        }
                    )
                    components.append(component)

        # Discover RDS instances
        response = self.rds_client.describe_db_instances()
        for db_instance in response['DBInstances']:
            if 'crm' in db_instance['DBInstanceIdentifier']:
                component = DeploymentComponent(
                    name=db_instance['DBInstanceIdentifier'],
                    type="database",
                    status=db_instance['DBInstanceStatus'],
                    metadata={
                        "engine": db_instance['Engine'],
                        "engine_version": db_instance['EngineVersion'],
                        "instance_class": db_instance['DBInstanceClass']
                    }
                )
                components.append(component)

        # Determine architecture
        architecture = "microservices" if len(components) > 3 else "monolithic"

        return DeploymentInfo(
            platform="aws",
            architecture=architecture,
            components=components,
            last_checked=datetime.now()
        )


class GCPDiscoveryClient(BaseDiscoveryClient):
    """GCP-based deployment discovery."""

    def __init__(self):
        if not _gcp_available():
            # Attempt on-demand install
            try:
                from prerequisites import ensure_group_installed
                if not ensure_group_installed("gcp"):
                    raise ImportError("GCP SDK not available. Install with: pip install google-cloud-compute google-cloud-container")
            except ImportError as orig:
                raise ImportError("GCP SDK not available. Install with: pip install google-cloud-compute google-cloud-container") from orig
        # GCP uses default credentials from environment

    def discover_deployment(self, config: Dict[str, Any]) -> DeploymentInfo:
        """Discover GCP deployment."""
        project_id = config.get('project_id')
        zone = config.get('zone', 'us-central1-a')

        if not project_id:
            raise CloudAPIError("GCP project_id required")

        components = []

        # Discover Compute Engine instances
        _compute_v1 = _lazy_import("google.cloud.compute_v1")
        compute_client = _compute_v1.InstancesClient()
        request = _compute_v1.ListInstancesRequest(
            project=project_id,
            zone=zone
        )

        for instance in compute_client.list(request):
            if 'crm' in instance.name.lower():
                component = DeploymentComponent(
                    name=instance.name,
                    type="vm",
                    status="running",
                    metadata={
                        "machine_type": instance.machine_type.split('/')[-1],
                        "zone": zone,
                        "status": instance.status
                    }
                )
                components.append(component)

        # Discover GKE clusters and workloads
        _container_v1 = _lazy_import("google.cloud.container_v1")
        container_client = _container_v1.ClusterManagerClient()
        clusters = container_client.list_clusters(project_id=project_id, zone=zone)

        for cluster in clusters.clusters:
            if 'crm' in cluster.name.lower():
                component = DeploymentComponent(
                    name=cluster.name,
                    type="kubernetes",
                    status="running",
                    metadata={
                        "location": cluster.location,
                        "node_count": cluster.current_node_count,
                        "version": cluster.current_master_version
                    }
                )
                components.append(component)

        # Determine architecture
        architecture = "microservices" if len(components) > 3 else "monolithic"

        return DeploymentInfo(
            platform="gcp",
            architecture=architecture,
            components=components,
            last_checked=datetime.now()
        )


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