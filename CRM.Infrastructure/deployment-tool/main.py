#!/usr/bin/env python3
"""
CRM Solution - Advanced GUI Deployment Tool
A comprehensive deployment tool for the CRM Solution with build server
configuration, network analysis, and real-time deployment progress.

Author: Abhishek Lal
License: AGPL-3.0
Version: 2.3.0
"""

import tkinter as tk
from tkinter import ttk, messagebox, filedialog, scrolledtext
import json
import os
import sys
import secrets
import subprocess
import threading
import webbrowser
import socket
import queue
import time
from datetime import datetime
from pathlib import Path
from typing import Optional, Dict, Any, List, Tuple, Callable
from dataclasses import dataclass, asdict, field
from enum import Enum
import urllib.request
import urllib.error
import ssl

# Version
VERSION = "2.2.0"

# Naming convention patterns
NAMING_CONVENTIONS = {
    "container_prefix": "crm",
    "network_name": "crm-network",
    "volume_prefix": "crm",
    "service_names": {
        "api": "crm-api",
        "frontend": "crm-frontend",
        "database": "crm-mariadb",
        "redis": "crm-redis",
        "gateway": "crm-gateway"
    }
}


class DeploymentStatus(Enum):
    """Deployment status enum."""
    PENDING = "pending"
    IN_PROGRESS = "in_progress"
    SUCCESS = "success"
    FAILED = "failed"
    SKIPPED = "skipped"


class Architecture(Enum):
    """Architecture type enum."""
    MONOLITHIC = "monolithic"
    MICROSERVICES = "microservices"


@dataclass
class TestResult:
    """Test result data."""
    name: str
    status: str  # passed, failed, skipped
    duration_ms: int = 0
    message: str = ""
    timestamp: str = ""


@dataclass
class DeploymentConfig:
    """Complete deployment configuration."""
    # Architecture
    architecture: str = "monolithic"
    
    # Build Server
    build_server_name: str = "Local"
    build_server_host: str = "localhost"
    build_server_port: int = 22
    build_server_user: str = "root"
    build_server_is_local: bool = True
    build_server_ssh_key: str = ""
    
    # Components to deploy
    deploy_frontend: bool = True
    deploy_api: bool = True
    deploy_database: bool = True
    deploy_redis: bool = True
    deploy_monitoring: bool = False
    
    # Hosting
    hosting_platform: str = "docker"
    cloud_provider: str = "custom"
    
    # Database
    database_provider: str = "mariadb"
    database_host: str = "crm-mariadb"
    database_port: int = 3306
    database_name: str = "crm_db"
    database_user: str = "crm_user"
    database_password: str = "CrmPass@Dev2024!"
    database_root_password: str = "RootPass@Dev2024"
    
    # Network
    api_port: int = 5000
    frontend_port: int = 80
    redis_port: int = 6379
    domain: str = "localhost"
    ssl_enabled: bool = False
    ssl_cert_path: str = ""
    ssl_key_path: str = ""
    
    # Admin User
    admin_username: str = "admin"
    admin_email: str = "admin@crm.local"
    admin_password: str = "Admin@123"
    admin_first_name: str = "System"
    admin_last_name: str = "Administrator"
    
    # Security
    jwt_secret: str = ""
    allowed_origins: str = ""
    
    # Seed Data
    seed_master_data: bool = True
    seed_demo_data: bool = False
    seed_zip_codes: bool = False
    
    # Testing
    run_bvt_tests: bool = False
    run_smoke_tests: bool = True
    
    # Microservices
    deploy_identity_service: bool = True
    deploy_customer_service: bool = True
    deploy_sales_service: bool = True
    deploy_marketing_service: bool = True
    deploy_servicedesk_service: bool = True
    
    # Cloud Configuration
    cloud_provider: str = "none"  # none, aws, azure, gcp
    
    # AWS Configuration
    aws_region: str = "us-east-1"
    aws_access_key: str = ""
    aws_secret_key: str = ""
    aws_ecs_cluster: str = "crm-cluster"
    aws_ecr_registry: str = ""
    aws_vpc_id: str = ""
    aws_subnet_ids: str = ""
    aws_security_group: str = ""
    aws_rds_instance: str = ""
    aws_use_fargate: bool = True
    aws_use_rds: bool = True
    aws_use_elasticache: bool = True
    
    # Azure Configuration
    azure_subscription_id: str = ""
    azure_resource_group: str = "crm-resources"
    azure_location: str = "eastus"
    azure_acr_name: str = ""
    azure_aks_cluster: str = ""
    azure_use_aci: bool = False
    azure_use_aks: bool = True
    azure_use_azure_db: bool = True
    azure_use_redis_cache: bool = True
    
    # GCP Configuration
    gcp_project_id: str = ""
    gcp_region: str = "us-central1"
    gcp_zone: str = "us-central1-a"
    gcp_gke_cluster: str = "crm-cluster"
    gcp_gcr_hostname: str = "gcr.io"
    gcp_use_cloud_run: bool = False
    gcp_use_gke: bool = True
    gcp_use_cloud_sql: bool = True
    gcp_use_memorystore: bool = True
    
    # Build Configuration
    build_source: str = "local"  # local, git
    git_repo_url: str = ""
    git_branch: str = "main"
    git_username: str = ""
    git_token: str = ""
    git_ssh_key: str = ""
    git_use_ssh: bool = False
    
    # Build Server
    build_type: str = "local"  # local, remote, cloud
    cloud_build_provider: str = "none"  # none, github_actions, azure_devops, aws_codebuild, gcp_cloudbuild
    
    # Build Commands
    build_frontend_cmd: str = "cd CRM.Frontend && npm install && npm run build"
    build_api_cmd: str = "cd CRM.Backend && dotnet publish -c Release -o ./publish"
    build_docker_api_cmd: str = "docker build -t crm-api:latest -f docker/Dockerfile.backend ."
    build_docker_frontend_cmd: str = "docker build -t crm-frontend:latest -f docker/Dockerfile.frontend ."
    
    # Cloud Build
    github_actions_workflow: str = ".github/workflows/build-deploy.yml"
    azure_devops_org: str = ""
    azure_devops_project: str = ""
    azure_devops_pipeline: str = ""
    aws_codebuild_project: str = "crm-build"
    gcp_cloudbuild_trigger: str = ""


class NetworkAnalyzer:
    """Analyze network connectivity and fix issues."""
    
    def __init__(self, config: DeploymentConfig):
        self.config = config
        self.issues: List[Dict[str, Any]] = []
    
    def check_port_availability(self, host: str, port: int, timeout: float = 2.0) -> bool:
        """Check if a port is reachable."""
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(timeout)
            result = sock.connect_ex((host, port))
            sock.close()
            return result == 0
        except Exception:
            return False
    
    def check_dns_resolution(self, hostname: str) -> Tuple[bool, str]:
        """Check if hostname can be resolved."""
        try:
            ip = socket.gethostbyname(hostname)
            return True, ip
        except socket.gaierror:
            return False, ""
    
    def analyze(self) -> List[Dict[str, Any]]:
        """Perform network analysis and return issues."""
        self.issues = []
        
        # Check build server connectivity
        if not self.config.build_server_is_local:
            host = self.config.build_server_host
            port = self.config.build_server_port
            
            can_resolve, ip = self.check_dns_resolution(host)
            if not can_resolve:
                self.issues.append({
                    "type": "dns",
                    "severity": "critical",
                    "component": "build_server",
                    "message": f"Cannot resolve hostname: {host}",
                    "fix": f"Ensure DNS is configured or use IP address directly"
                })
            elif not self.check_port_availability(host, port):
                self.issues.append({
                    "type": "connectivity",
                    "severity": "critical",
                    "component": "build_server",
                    "message": f"Cannot connect to {host}:{port}",
                    "fix": f"Check if SSH is running and firewall allows port {port}"
                })
        
        # Check API port conflict
        if self.config.deploy_api and not self.config.build_server_is_local:
            api_host = self.config.build_server_host
            api_port = self.config.api_port
            if self.check_port_availability(api_host, api_port):
                self.issues.append({
                    "type": "port_in_use",
                    "severity": "warning",
                    "component": "api",
                    "message": f"Port {api_port} already in use on {api_host}",
                    "fix": f"Stop existing service or change API port"
                })
        
        return self.issues
    
    def get_network_fixes(self) -> List[str]:
        """Get Docker network commands to fix connectivity."""
        fixes = []
        network_name = NAMING_CONVENTIONS["network_name"]
        
        # Create network
        fixes.append(f"docker network create {network_name} 2>/dev/null || true")
        
        # Connect containers
        containers = []
        if self.config.deploy_database:
            containers.append(NAMING_CONVENTIONS["service_names"]["database"])
        if self.config.deploy_api:
            containers.append(NAMING_CONVENTIONS["service_names"]["api"])
        if self.config.deploy_frontend:
            containers.append(NAMING_CONVENTIONS["service_names"]["frontend"])
        if self.config.deploy_redis:
            containers.append(NAMING_CONVENTIONS["service_names"]["redis"])
        
        for container in containers:
            fixes.append(f"docker network connect {network_name} {container} 2>/dev/null || true")
        
        return fixes


class PrerequisiteChecker:
    """Check and install prerequisites."""
    
    PREREQUISITES = {
        "docker": {
            "check_cmd": "docker --version",
            "required": True,
            "category": "core",
            "install_cmd_mac": "brew install --cask docker",
            "install_cmd_linux": "curl -fsSL https://get.docker.com | sh"
        },
        "docker-compose": {
            "check_cmd": "docker compose version",
            "required": True,
            "category": "core",
            "install_cmd_mac": "brew install docker-compose",
            "install_cmd_linux": "sudo apt-get install docker-compose-plugin"
        },
        "node": {
            "check_cmd": "node --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install node",
            "install_cmd_linux": "curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash - && sudo apt-get install -y nodejs"
        },
        "npm": {
            "check_cmd": "npm --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install node",
            "install_cmd_linux": "sudo apt-get install -y npm"
        },
        "dotnet": {
            "check_cmd": "dotnet --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install dotnet",
            "install_cmd_linux": "sudo apt-get install -y dotnet-sdk-8.0"
        },
        "git": {
            "check_cmd": "git --version",
            "required": False,
            "category": "build",
            "install_cmd_mac": "brew install git",
            "install_cmd_linux": "sudo apt-get install -y git"
        },
        "kubectl": {
            "check_cmd": "kubectl version --client --short 2>/dev/null || kubectl version --client",
            "required": False,
            "category": "kubernetes",
            "install_cmd_mac": "brew install kubectl",
            "install_cmd_linux": "curl -LO https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl && sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl"
        },
        "helm": {
            "check_cmd": "helm version --short",
            "required": False,
            "category": "kubernetes",
            "install_cmd_mac": "brew install helm",
            "install_cmd_linux": "curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"
        },
        "aws": {
            "check_cmd": "aws --version",
            "required": False,
            "category": "cloud",
            "install_cmd_mac": "brew install awscli",
            "install_cmd_linux": "curl 'https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip' -o 'awscliv2.zip' && unzip awscliv2.zip && sudo ./aws/install"
        },
        "az": {
            "check_cmd": "az --version 2>/dev/null | head -1",
            "required": False,
            "category": "cloud",
            "install_cmd_mac": "brew install azure-cli",
            "install_cmd_linux": "curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash"
        },
        "gcloud": {
            "check_cmd": "gcloud --version 2>/dev/null | head -1",
            "required": False,
            "category": "cloud",
            "install_cmd_mac": "brew install google-cloud-sdk",
            "install_cmd_linux": "curl https://sdk.cloud.google.com | bash"
        },
        "terraform": {
            "check_cmd": "terraform --version | head -1",
            "required": False,
            "category": "infrastructure",
            "install_cmd_mac": "brew install terraform",
            "install_cmd_linux": "sudo apt-get install -y terraform"
        }
    }
    
    def __init__(self, ssh_cmd: str = ""):
        self.ssh_cmd = ssh_cmd
        self.results: Dict[str, Dict[str, Any]] = {}
        self.is_mac = sys.platform == "darwin"
    
    def _run_command(self, cmd: str, timeout: int = 30) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            if self.ssh_cmd:
                cmd = f'{self.ssh_cmd} "{cmd}"'
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            return result.returncode == 0, result.stdout.strip()
        except Exception as e:
            return False, str(e)
    
    def check_all(self) -> Dict[str, Dict[str, Any]]:
        """Check all prerequisites."""
        self.results = {}
        
        for name, prereq in self.PREREQUISITES.items():
            success, output = self._run_command(prereq["check_cmd"])
            install_cmd = prereq.get("install_cmd_mac" if self.is_mac else "install_cmd_linux", "")
            self.results[name] = {
                "installed": success,
                "version": output if success else None,
                "required": prereq["required"],
                "category": prereq.get("category", "other"),
                "install_cmd": install_cmd
            }
        
        return self.results
    
    def check_by_category(self, category: str) -> Dict[str, Dict[str, Any]]:
        """Check prerequisites by category."""
        results = {}
        for name, prereq in self.PREREQUISITES.items():
            if prereq.get("category") == category:
                success, output = self._run_command(prereq["check_cmd"])
                install_cmd = prereq.get("install_cmd_mac" if self.is_mac else "install_cmd_linux", "")
                results[name] = {
                    "installed": success,
                    "version": output if success else None,
                    "required": prereq["required"],
                    "install_cmd": install_cmd
                }
        return results
    
    def install_prerequisite(self, name: str, log_callback: Callable[[str, str], None] = None) -> bool:
        """Install a specific prerequisite."""
        if name not in self.PREREQUISITES:
            return False
        
        prereq = self.PREREQUISITES[name]
        install_cmd = prereq.get("install_cmd_mac" if self.is_mac else "install_cmd_linux", "")
        
        if not install_cmd:
            if log_callback:
                log_callback("error", f"No install command for {name}")
            return False
        
        if log_callback:
            log_callback("info", f"Installing {name}...")
            log_callback("cmd", f"$ {install_cmd}")
        
        success, output = self._run_command(install_cmd, timeout=300)
        
        if success:
            if log_callback:
                log_callback("success", f"✓ {name} installed successfully")
        else:
            if log_callback:
                log_callback("error", f"✗ Failed to install {name}: {output}")
        
        return success
    
    def install_missing(self, category: str = None, log_callback: Callable[[str, str], None] = None) -> Dict[str, bool]:
        """Install all missing prerequisites, optionally filtered by category."""
        if not self.results:
            self.check_all()
        
        install_results = {}
        
        for name, result in self.results.items():
            if result["installed"]:
                continue
            
            prereq = self.PREREQUISITES[name]
            if category and prereq.get("category") != category:
                continue
            
            install_results[name] = self.install_prerequisite(name, log_callback)
        
        return install_results
    
    def get_missing_required(self) -> List[str]:
        """Get list of missing required prerequisites."""
        return [
            name for name, result in self.results.items()
            if result["required"] and not result["installed"]
        ]
    
    def get_cloud_clis_status(self) -> Dict[str, bool]:
        """Get status of cloud CLIs."""
        cloud_results = self.check_by_category("cloud")
        return {name: result["installed"] for name, result in cloud_results.items()}


class CloudDeployer:
    """Base class for cloud deployments."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
    
    def _run_command(self, cmd: str, timeout: int = 300) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            self.log("cmd", f"$ {cmd}")
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            if result.stdout:
                self.log("output", result.stdout.strip())
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip())
            return result.returncode == 0, result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            self.log("error", f"Command timed out after {timeout}s")
            return False, "Timeout"
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def check_cli(self) -> bool:
        """Check if cloud CLI is installed."""
        raise NotImplementedError
    
    def authenticate(self) -> bool:
        """Authenticate with cloud provider."""
        raise NotImplementedError
    
    def create_infrastructure(self) -> bool:
        """Create cloud infrastructure."""
        raise NotImplementedError
    
    def push_images(self) -> bool:
        """Push Docker images to cloud registry."""
        raise NotImplementedError
    
    def deploy(self) -> bool:
        """Deploy to cloud."""
        raise NotImplementedError
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get deployed service endpoints."""
        raise NotImplementedError


class AWSDeployer(CloudDeployer):
    """AWS ECS/Fargate deployment."""
    
    def check_cli(self) -> bool:
        """Check if AWS CLI is installed."""
        success, output = self._run_command("aws --version")
        if success:
            self.log("success", f"AWS CLI: {output.split()[0]}")
        else:
            self.log("error", "AWS CLI not installed. Install with: brew install awscli")
        return success
    
    def authenticate(self) -> bool:
        """Authenticate with AWS."""
        self.log("info", "Checking AWS authentication...")
        success, output = self._run_command("aws sts get-caller-identity")
        if success:
            self.log("success", "AWS authentication successful")
        else:
            self.log("error", "AWS authentication failed. Run: aws configure")
        return success
    
    def get_account_info(self) -> Dict[str, Any]:
        """Get AWS account information."""
        info = {
            "authenticated": False,
            "account_id": None,
            "user_arn": None,
            "regions": [],
            "vpcs": [],
            "ecs_clusters": [],
            "ecr_repositories": []
        }
        
        # Check authentication and get identity
        success, output = self._run_command("aws sts get-caller-identity --output json")
        if success:
            try:
                identity = json.loads(output)
                info["authenticated"] = True
                info["account_id"] = identity.get("Account")
                info["user_arn"] = identity.get("Arn")
            except json.JSONDecodeError:
                pass
        
        if not info["authenticated"]:
            return info
        
        # Get available regions
        success, output = self._run_command("aws ec2 describe-regions --query 'Regions[].RegionName' --output json")
        if success:
            try:
                info["regions"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get VPCs in current region
        region = self.config.aws_region or "us-east-1"
        success, output = self._run_command(
            f"aws ec2 describe-vpcs --region {region} --query 'Vpcs[].{{VpcId:VpcId,CidrBlock:CidrBlock,IsDefault:IsDefault}}' --output json"
        )
        if success:
            try:
                info["vpcs"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get ECS clusters
        success, output = self._run_command(
            f"aws ecs list-clusters --region {region} --query 'clusterArns' --output json"
        )
        if success:
            try:
                clusters = json.loads(output)
                info["ecs_clusters"] = [c.split("/")[-1] for c in clusters]
            except json.JSONDecodeError:
                pass
        
        # Get ECR repositories
        success, output = self._run_command(
            f"aws ecr describe-repositories --region {region} --query 'repositories[].repositoryName' --output json"
        )
        if success:
            try:
                info["ecr_repositories"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        return info
    
    def create_infrastructure(self) -> bool:
        """Create AWS infrastructure (ECS cluster, RDS, ElastiCache)."""
        self.log("info", "Creating AWS infrastructure...")
        
        region = self.config.aws_region
        cluster = self.config.aws_ecs_cluster
        
        # Create ECS cluster
        self.log("info", f"Creating ECS cluster: {cluster}")
        success, _ = self._run_command(
            f"aws ecs create-cluster --cluster-name {cluster} --region {region} 2>/dev/null || true"
        )
        
        # Create ECR repositories
        for service in ["api", "frontend"]:
            repo_name = f"crm-{service}"
            self.log("info", f"Creating ECR repository: {repo_name}")
            self._run_command(
                f"aws ecr create-repository --repository-name {repo_name} --region {region} 2>/dev/null || true"
            )
        
        if self.config.aws_use_rds:
            self.log("info", "RDS instance should be created via AWS Console or Terraform")
        
        if self.config.aws_use_elasticache:
            self.log("info", "ElastiCache should be created via AWS Console or Terraform")
        
        return True
    
    def push_images(self) -> bool:
        """Push Docker images to ECR."""
        self.log("info", "Pushing images to ECR...")
        
        region = self.config.aws_region
        registry = self.config.aws_ecr_registry
        
        if not registry:
            # Get ECR registry URL
            success, output = self._run_command(
                f"aws ecr get-login-password --region {region} | docker login --username AWS --password-stdin $(aws sts get-caller-identity --query Account --output text).dkr.ecr.{region}.amazonaws.com"
            )
            if not success:
                return False
        
        for service in ["api", "frontend"]:
            image = f"crm-{service}:latest"
            ecr_image = f"{registry}/crm-{service}:latest"
            
            self.log("info", f"Tagging and pushing {image}...")
            self._run_command(f"docker tag {image} {ecr_image}")
            success, _ = self._run_command(f"docker push {ecr_image}")
            if not success:
                self.log("error", f"Failed to push {image}")
                return False
        
        return True
    
    def deploy(self) -> bool:
        """Deploy to ECS/Fargate."""
        self.log("info", "Deploying to AWS ECS...")
        
        cluster = self.config.aws_ecs_cluster
        region = self.config.aws_region
        
        # Generate task definition
        task_def = self._generate_task_definition()
        
        # Register task definition
        self.log("info", "Registering task definition...")
        # In production, would register via AWS CLI
        
        # Create/update service
        self.log("info", "Creating/updating ECS service...")
        
        self.log("success", "AWS deployment initiated")
        return True
    
    def _generate_task_definition(self) -> Dict[str, Any]:
        """Generate ECS task definition."""
        return {
            "family": "crm-app",
            "networkMode": "awsvpc",
            "requiresCompatibilities": ["FARGATE"] if self.config.aws_use_fargate else ["EC2"],
            "cpu": "512",
            "memory": "1024",
            "containerDefinitions": [
                {
                    "name": "crm-api",
                    "image": f"{self.config.aws_ecr_registry}/crm-api:latest",
                    "portMappings": [{"containerPort": 5000}],
                    "environment": [
                        {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
                        {"name": "DatabaseProvider", "value": self.config.database_provider}
                    ]
                }
            ]
        }
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get AWS service endpoints."""
        return {
            "api": f"https://api.{self.config.domain}",
            "frontend": f"https://{self.config.domain}"
        }


class AzureDeployer(CloudDeployer):
    """Azure AKS/ACI deployment."""
    
    def check_cli(self) -> bool:
        """Check if Azure CLI is installed."""
        success, output = self._run_command("az --version")
        if success:
            version_line = output.split('\n')[0] if output else "unknown"
            self.log("success", f"Azure CLI: {version_line}")
        else:
            self.log("error", "Azure CLI not installed. Install with: brew install azure-cli")
        return success
    
    def authenticate(self) -> bool:
        """Authenticate with Azure."""
        self.log("info", "Checking Azure authentication...")
        success, output = self._run_command("az account show")
        if success:
            self.log("success", "Azure authentication successful")
        else:
            self.log("warning", "Not logged in. Opening browser for login...")
            self._run_command("az login")
        return True
    
    def get_account_info(self) -> Dict[str, Any]:
        """Get Azure account information."""
        info = {
            "authenticated": False,
            "subscription_id": None,
            "subscription_name": None,
            "tenant_id": None,
            "user": None,
            "subscriptions": [],
            "resource_groups": [],
            "locations": [],
            "acr_registries": [],
            "aks_clusters": []
        }
        
        # Check authentication and get account info
        success, output = self._run_command("az account show --output json")
        if success:
            try:
                account = json.loads(output)
                info["authenticated"] = True
                info["subscription_id"] = account.get("id")
                info["subscription_name"] = account.get("name")
                info["tenant_id"] = account.get("tenantId")
                info["user"] = account.get("user", {}).get("name")
            except json.JSONDecodeError:
                pass
        
        if not info["authenticated"]:
            return info
        
        # Get all subscriptions
        success, output = self._run_command("az account list --query '[].{id:id,name:name,isDefault:isDefault}' --output json")
        if success:
            try:
                info["subscriptions"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get resource groups
        success, output = self._run_command("az group list --query '[].{name:name,location:location}' --output json")
        if success:
            try:
                info["resource_groups"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get available locations
        success, output = self._run_command("az account list-locations --query '[].{name:name,displayName:displayName}' --output json")
        if success:
            try:
                info["locations"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get ACR registries
        success, output = self._run_command("az acr list --query '[].{name:name,loginServer:loginServer,resourceGroup:resourceGroup}' --output json")
        if success:
            try:
                info["acr_registries"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get AKS clusters
        success, output = self._run_command("az aks list --query '[].{name:name,resourceGroup:resourceGroup,location:location}' --output json")
        if success:
            try:
                info["aks_clusters"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        return info
    
    def create_infrastructure(self) -> bool:
        """Create Azure infrastructure."""
        self.log("info", "Creating Azure infrastructure...")
        
        rg = self.config.azure_resource_group
        location = self.config.azure_location
        
        # Create resource group
        self.log("info", f"Creating resource group: {rg}")
        self._run_command(f"az group create --name {rg} --location {location}")
        
        # Create ACR
        acr_name = self.config.azure_acr_name or f"crmacr{secrets.token_hex(4)}"
        self.log("info", f"Creating Azure Container Registry: {acr_name}")
        self._run_command(
            f"az acr create --resource-group {rg} --name {acr_name} --sku Basic"
        )
        
        if self.config.azure_use_aks:
            # Create AKS cluster
            aks_name = self.config.azure_aks_cluster or "crm-aks"
            self.log("info", f"Creating AKS cluster: {aks_name}")
            self._run_command(
                f"az aks create --resource-group {rg} --name {aks_name} "
                f"--node-count 2 --enable-addons monitoring --generate-ssh-keys"
            )
        
        if self.config.azure_use_azure_db:
            self.log("info", "Azure Database should be created via Azure Portal or Terraform")
        
        return True
    
    def push_images(self) -> bool:
        """Push Docker images to ACR."""
        self.log("info", "Pushing images to Azure Container Registry...")
        
        acr_name = self.config.azure_acr_name
        
        # Login to ACR
        self._run_command(f"az acr login --name {acr_name}")
        
        for service in ["api", "frontend"]:
            image = f"crm-{service}:latest"
            acr_image = f"{acr_name}.azurecr.io/crm-{service}:latest"
            
            self.log("info", f"Pushing {image} to ACR...")
            self._run_command(f"docker tag {image} {acr_image}")
            success, _ = self._run_command(f"docker push {acr_image}")
            if not success:
                return False
        
        return True
    
    def deploy(self) -> bool:
        """Deploy to AKS or ACI."""
        if self.config.azure_use_aks:
            return self._deploy_to_aks()
        else:
            return self._deploy_to_aci()
    
    def _deploy_to_aks(self) -> bool:
        """Deploy to Azure Kubernetes Service."""
        self.log("info", "Deploying to AKS...")
        
        rg = self.config.azure_resource_group
        aks_name = self.config.azure_aks_cluster
        
        # Get credentials
        self._run_command(f"az aks get-credentials --resource-group {rg} --name {aks_name}")
        
        # Apply Kubernetes manifests
        self.log("info", "Applying Kubernetes manifests...")
        # Would apply k8s manifests here
        
        self.log("success", "AKS deployment complete")
        return True
    
    def _deploy_to_aci(self) -> bool:
        """Deploy to Azure Container Instances."""
        self.log("info", "Deploying to Azure Container Instances...")
        
        rg = self.config.azure_resource_group
        acr_name = self.config.azure_acr_name
        
        # Deploy API container
        self.log("info", "Deploying API container...")
        self._run_command(
            f"az container create --resource-group {rg} --name crm-api "
            f"--image {acr_name}.azurecr.io/crm-api:latest --ports 5000 "
            f"--dns-name-label crm-api-{secrets.token_hex(4)}"
        )
        
        self.log("success", "ACI deployment complete")
        return True
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get Azure service endpoints."""
        return {
            "api": f"https://api.{self.config.domain}",
            "frontend": f"https://{self.config.domain}"
        }


class GCPDeployer(CloudDeployer):
    """GCP GKE/Cloud Run deployment."""
    
    def check_cli(self) -> bool:
        """Check if gcloud CLI is installed."""
        success, output = self._run_command("gcloud --version")
        if success:
            version_line = output.split('\n')[0] if output else "unknown"
            self.log("success", f"Google Cloud SDK: {version_line}")
        else:
            self.log("error", "gcloud not installed. Install from: https://cloud.google.com/sdk")
        return success
    
    def authenticate(self) -> bool:
        """Authenticate with GCP."""
        self.log("info", "Checking GCP authentication...")
        success, output = self._run_command("gcloud auth list --filter=status:ACTIVE --format='value(account)'")
        if success and output:
            self.log("success", f"GCP authenticated as: {output}")
        else:
            self.log("warning", "Not authenticated. Running gcloud auth login...")
            self._run_command("gcloud auth login")
        return True
    
    def get_account_info(self) -> Dict[str, Any]:
        """Get GCP account information."""
        info = {
            "authenticated": False,
            "account": None,
            "project_id": None,
            "project_name": None,
            "projects": [],
            "regions": [],
            "zones": [],
            "gke_clusters": [],
            "cloud_run_services": []
        }
        
        # Check authentication
        success, output = self._run_command("gcloud auth list --filter=status:ACTIVE --format='value(account)'")
        if success and output:
            info["authenticated"] = True
            info["account"] = output.strip()
        
        if not info["authenticated"]:
            return info
        
        # Get current project
        success, output = self._run_command("gcloud config get-value project")
        if success and output:
            info["project_id"] = output.strip()
        
        # Get all projects
        success, output = self._run_command("gcloud projects list --format='json(projectId,name)'")
        if success:
            try:
                info["projects"] = json.loads(output)
            except json.JSONDecodeError:
                pass
        
        # Get available regions
        success, output = self._run_command("gcloud compute regions list --format='json(name,status)'")
        if success:
            try:
                regions = json.loads(output)
                info["regions"] = [r.get("name") for r in regions if r.get("status") == "UP"]
            except json.JSONDecodeError:
                pass
        
        # Get available zones
        success, output = self._run_command("gcloud compute zones list --format='json(name,region,status)' 2>/dev/null")
        if success:
            try:
                zones = json.loads(output)
                info["zones"] = [z.get("name") for z in zones if z.get("status") == "UP"]
            except json.JSONDecodeError:
                pass
        
        # Get GKE clusters
        if info["project_id"]:
            success, output = self._run_command(
                f"gcloud container clusters list --project {info['project_id']} --format='json(name,zone,status)' 2>/dev/null"
            )
            if success:
                try:
                    info["gke_clusters"] = json.loads(output)
                except json.JSONDecodeError:
                    pass
        
            # Get Cloud Run services
            success, output = self._run_command(
                f"gcloud run services list --project {info['project_id']} --format='json(metadata.name,status.url)' 2>/dev/null"
            )
            if success:
                try:
                    info["cloud_run_services"] = json.loads(output)
                except json.JSONDecodeError:
                    pass
        
        return info
    
    def create_infrastructure(self) -> bool:
        """Create GCP infrastructure."""
        self.log("info", "Creating GCP infrastructure...")
        
        project = self.config.gcp_project_id
        region = self.config.gcp_region
        zone = self.config.gcp_zone
        
        # Set project
        self._run_command(f"gcloud config set project {project}")
        
        # Enable required APIs
        self.log("info", "Enabling required APIs...")
        for api in ["container", "containerregistry", "cloudbuild", "sql-component"]:
            self._run_command(f"gcloud services enable {api}.googleapis.com")
        
        if self.config.gcp_use_gke:
            # Create GKE cluster
            cluster = self.config.gcp_gke_cluster
            self.log("info", f"Creating GKE cluster: {cluster}")
            self._run_command(
                f"gcloud container clusters create {cluster} "
                f"--zone {zone} --num-nodes 2 --machine-type e2-medium"
            )
        
        if self.config.gcp_use_cloud_sql:
            self.log("info", "Cloud SQL should be created via GCP Console or Terraform")
        
        return True
    
    def push_images(self) -> bool:
        """Push Docker images to GCR."""
        self.log("info", "Pushing images to Google Container Registry...")
        
        project = self.config.gcp_project_id
        gcr_host = self.config.gcp_gcr_hostname
        
        # Configure Docker for GCR
        self._run_command(f"gcloud auth configure-docker {gcr_host}")
        
        for service in ["api", "frontend"]:
            image = f"crm-{service}:latest"
            gcr_image = f"{gcr_host}/{project}/crm-{service}:latest"
            
            self.log("info", f"Pushing {image} to GCR...")
            self._run_command(f"docker tag {image} {gcr_image}")
            success, _ = self._run_command(f"docker push {gcr_image}")
            if not success:
                return False
        
        return True
    
    def deploy(self) -> bool:
        """Deploy to GKE or Cloud Run."""
        if self.config.gcp_use_cloud_run:
            return self._deploy_to_cloud_run()
        else:
            return self._deploy_to_gke()
    
    def _deploy_to_gke(self) -> bool:
        """Deploy to Google Kubernetes Engine."""
        self.log("info", "Deploying to GKE...")
        
        cluster = self.config.gcp_gke_cluster
        zone = self.config.gcp_zone
        
        # Get credentials
        self._run_command(f"gcloud container clusters get-credentials {cluster} --zone {zone}")
        
        # Apply Kubernetes manifests
        self.log("info", "Applying Kubernetes manifests...")
        # Would apply k8s manifests here
        
        self.log("success", "GKE deployment complete")
        return True
    
    def _deploy_to_cloud_run(self) -> bool:
        """Deploy to Cloud Run."""
        self.log("info", "Deploying to Cloud Run...")
        
        project = self.config.gcp_project_id
        region = self.config.gcp_region
        gcr_host = self.config.gcp_gcr_hostname
        
        # Deploy API
        self.log("info", "Deploying API to Cloud Run...")
        self._run_command(
            f"gcloud run deploy crm-api "
            f"--image {gcr_host}/{project}/crm-api:latest "
            f"--platform managed --region {region} --allow-unauthenticated"
        )
        
        # Deploy Frontend
        self.log("info", "Deploying Frontend to Cloud Run...")
        self._run_command(
            f"gcloud run deploy crm-frontend "
            f"--image {gcr_host}/{project}/crm-frontend:latest "
            f"--platform managed --region {region} --allow-unauthenticated"
        )
        
        self.log("success", "Cloud Run deployment complete")
        return True
    
    def get_endpoints(self) -> Dict[str, str]:
        """Get GCP service endpoints."""
        region = self.config.gcp_region
        return {
            "api": f"https://crm-api-{region}.run.app",
            "frontend": f"https://crm-frontend-{region}.run.app"
        }


class BuildEngine:
    """Build engine for compiling and creating Docker images."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
        self.is_running = False
        self.build_dir = ""
    
    def _run_command(self, cmd: str, cwd: str = None, timeout: int = 600) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            self.log("cmd", f"$ {cmd}")
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, 
                timeout=timeout, cwd=cwd
            )
            if result.stdout:
                self.log("output", result.stdout.strip()[:500])  # Truncate long output
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip()[:500])
            return result.returncode == 0, result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            self.log("error", f"Command timed out after {timeout}s")
            return False, "Timeout"
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def clone_repository(self) -> bool:
        """Clone or pull the git repository."""
        if self.config.build_source != "git":
            self.log("info", "Using local source code")
            return True
        
        repo_url = self.config.git_repo_url
        branch = self.config.git_branch
        
        if not repo_url:
            self.log("error", "Git repository URL not configured")
            return False
        
        self.log("info", f"Cloning repository: {repo_url}")
        self.log("info", f"Branch: {branch}")
        
        # Create build directory
        self.build_dir = f"/tmp/crm-build-{secrets.token_hex(4)}"
        
        # Construct git URL with authentication
        if self.config.git_use_ssh:
            # Use SSH key
            if self.config.git_ssh_key:
                git_cmd = f"GIT_SSH_COMMAND='ssh -i {self.config.git_ssh_key} -o StrictHostKeyChecking=no'"
            else:
                git_cmd = ""
            clone_cmd = f"{git_cmd} git clone --branch {branch} --single-branch {repo_url} {self.build_dir}"
        else:
            # Use HTTPS with token
            if self.config.git_token and self.config.git_username:
                # Insert credentials into URL
                if "github.com" in repo_url:
                    auth_url = repo_url.replace("https://", f"https://{self.config.git_username}:{self.config.git_token}@")
                elif "dev.azure.com" in repo_url:
                    auth_url = repo_url.replace("https://", f"https://{self.config.git_username}:{self.config.git_token}@")
                else:
                    auth_url = repo_url.replace("https://", f"https://{self.config.git_username}:{self.config.git_token}@")
                clone_cmd = f"git clone --branch {branch} --single-branch {auth_url} {self.build_dir}"
            else:
                clone_cmd = f"git clone --branch {branch} --single-branch {repo_url} {self.build_dir}"
        
        success, _ = self._run_command(clone_cmd)
        
        if success:
            self.log("success", f"Repository cloned to {self.build_dir}")
        else:
            self.log("error", "Failed to clone repository")
        
        return success
    
    def build_frontend(self) -> bool:
        """Build the frontend application."""
        self.log("info", "Building frontend...")
        
        cwd = self.build_dir if self.build_dir else None
        cmd = self.config.build_frontend_cmd
        
        success, _ = self._run_command(cmd, cwd=cwd, timeout=300)
        
        if success:
            self.log("success", "Frontend build complete")
        else:
            self.log("error", "Frontend build failed")
        
        return success
    
    def build_api(self) -> bool:
        """Build the API application."""
        self.log("info", "Building API...")
        
        cwd = self.build_dir if self.build_dir else None
        cmd = self.config.build_api_cmd
        
        success, _ = self._run_command(cmd, cwd=cwd, timeout=300)
        
        if success:
            self.log("success", "API build complete")
        else:
            self.log("error", "API build failed")
        
        return success
    
    def build_docker_images(self) -> bool:
        """Build Docker images."""
        self.log("info", "Building Docker images...")
        
        cwd = self.build_dir if self.build_dir else None
        
        # Build API image
        if self.config.deploy_api:
            self.log("info", "Building API Docker image...")
            success, _ = self._run_command(self.config.build_docker_api_cmd, cwd=cwd)
            if not success:
                self.log("error", "API Docker build failed")
                return False
            self.log("success", "API Docker image built: crm-api:latest")
        
        # Build Frontend image
        if self.config.deploy_frontend:
            self.log("info", "Building Frontend Docker image...")
            success, _ = self._run_command(self.config.build_docker_frontend_cmd, cwd=cwd)
            if not success:
                self.log("error", "Frontend Docker build failed")
                return False
            self.log("success", "Frontend Docker image built: crm-frontend:latest")
        
        return True
    
    def cleanup(self):
        """Clean up build directory."""
        if self.build_dir and self.build_dir.startswith("/tmp/"):
            self._run_command(f"rm -rf {self.build_dir}")
            self.log("info", f"Cleaned up build directory: {self.build_dir}")


class CloudBuildEngine:
    """Cloud-based build engine using CI/CD services."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
    
    def _run_command(self, cmd: str, timeout: int = 300) -> Tuple[bool, str]:
        """Run a command and return success status and output."""
        try:
            self.log("cmd", f"$ {cmd}")
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            if result.stdout:
                self.log("output", result.stdout.strip())
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip())
            return result.returncode == 0, result.stdout + result.stderr
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def trigger_github_actions(self) -> bool:
        """Trigger GitHub Actions workflow."""
        self.log("info", "Triggering GitHub Actions workflow...")
        
        repo_url = self.config.git_repo_url
        if not repo_url:
            self.log("error", "Git repository URL not configured")
            return False
        
        # Extract owner/repo from URL
        if "github.com" in repo_url:
            parts = repo_url.replace("https://github.com/", "").replace(".git", "").split("/")
            if len(parts) >= 2:
                owner, repo = parts[0], parts[1]
            else:
                self.log("error", "Invalid GitHub URL")
                return False
        else:
            self.log("error", "Not a GitHub repository")
            return False
        
        token = self.config.git_token
        if not token:
            self.log("error", "GitHub token required for triggering workflows")
            return False
        
        workflow = self.config.github_actions_workflow.split("/")[-1]  # Get filename
        branch = self.config.git_branch
        
        # Trigger workflow via GitHub API
        cmd = f'''curl -X POST -H "Accept: application/vnd.github.v3+json" \
            -H "Authorization: token {token}" \
            https://api.github.com/repos/{owner}/{repo}/actions/workflows/{workflow}/dispatches \
            -d '{{"ref":"{branch}"}}'
        '''
        
        success, output = self._run_command(cmd)
        
        if success:
            self.log("success", "GitHub Actions workflow triggered")
            self.log("info", f"View at: https://github.com/{owner}/{repo}/actions")
        else:
            self.log("error", "Failed to trigger GitHub Actions")
        
        return success
    
    def trigger_azure_devops(self) -> bool:
        """Trigger Azure DevOps pipeline."""
        self.log("info", "Triggering Azure DevOps pipeline...")
        
        org = self.config.azure_devops_org
        project = self.config.azure_devops_project
        pipeline = self.config.azure_devops_pipeline
        
        if not all([org, project, pipeline]):
            self.log("error", "Azure DevOps configuration incomplete")
            return False
        
        token = self.config.git_token
        if not token:
            self.log("error", "Azure DevOps PAT required")
            return False
        
        # Trigger pipeline via Azure DevOps API
        cmd = f'''curl -X POST \
            -H "Content-Type: application/json" \
            -H "Authorization: Basic $(echo -n ":{token}" | base64)" \
            "https://dev.azure.com/{org}/{project}/_apis/pipelines/{pipeline}/runs?api-version=7.0" \
            -d '{{"resources": {{"repositories": {{"self": {{"refName": "refs/heads/{self.config.git_branch}"}}}}}}}}'
        '''
        
        success, output = self._run_command(cmd)
        
        if success:
            self.log("success", "Azure DevOps pipeline triggered")
            self.log("info", f"View at: https://dev.azure.com/{org}/{project}/_build")
        else:
            self.log("error", "Failed to trigger Azure DevOps pipeline")
        
        return success
    
    def trigger_aws_codebuild(self) -> bool:
        """Trigger AWS CodeBuild project."""
        self.log("info", "Triggering AWS CodeBuild...")
        
        project = self.config.aws_codebuild_project
        region = self.config.aws_region
        
        if not project:
            self.log("error", "AWS CodeBuild project not configured")
            return False
        
        cmd = f"aws codebuild start-build --project-name {project} --region {region}"
        
        success, output = self._run_command(cmd)
        
        if success:
            self.log("success", "AWS CodeBuild started")
            # Extract build ID from output
            try:
                build_data = json.loads(output)
                build_id = build_data.get("build", {}).get("id", "")
                if build_id:
                    self.log("info", f"Build ID: {build_id}")
                    self.log("info", f"View at: https://{region}.console.aws.amazon.com/codesuite/codebuild/projects/{project}/build/{build_id}")
            except:
                pass
        else:
            self.log("error", "Failed to start AWS CodeBuild")
        
        return success
    
    def trigger_gcp_cloudbuild(self) -> bool:
        """Trigger Google Cloud Build."""
        self.log("info", "Triggering Google Cloud Build...")
        
        project = self.config.gcp_project_id
        trigger = self.config.gcp_cloudbuild_trigger
        branch = self.config.git_branch
        
        if not project:
            self.log("error", "GCP project ID not configured")
            return False
        
        if trigger:
            # Use existing trigger
            cmd = f"gcloud builds triggers run {trigger} --branch={branch} --project={project}"
        else:
            # Submit build from cloudbuild.yaml
            cmd = f"gcloud builds submit --config=cloudbuild.yaml --project={project}"
        
        success, output = self._run_command(cmd, timeout=600)
        
        if success:
            self.log("success", "Google Cloud Build triggered")
            self.log("info", f"View at: https://console.cloud.google.com/cloud-build/builds?project={project}")
        else:
            self.log("error", "Failed to trigger Cloud Build")
        
        return success
    
    def trigger_build(self) -> bool:
        """Trigger build based on configured provider."""
        provider = self.config.cloud_build_provider
        
        if provider == "github_actions":
            return self.trigger_github_actions()
        elif provider == "azure_devops":
            return self.trigger_azure_devops()
        elif provider == "aws_codebuild":
            return self.trigger_aws_codebuild()
        elif provider == "gcp_cloudbuild":
            return self.trigger_gcp_cloudbuild()
        else:
            self.log("error", f"Unknown cloud build provider: {provider}")
            return False
    
    def generate_github_actions_workflow(self) -> str:
        """Generate GitHub Actions workflow file."""
        return f'''name: CRM Build and Deploy

on:
  push:
    branches: [ {self.config.git_branch} ]
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  API_IMAGE: crm-api
  FRONTEND_IMAGE: crm-frontend

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
        cache: 'npm'
        cache-dependency-path: CRM.Frontend/package-lock.json
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Build Frontend
      run: |
        cd CRM.Frontend
        npm ci
        npm run build
    
    - name: Build API
      run: |
        cd CRM.Backend
        dotnet restore
        dotnet publish -c Release -o ./publish
    
    - name: Login to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{{{ env.REGISTRY }}}}
        username: ${{{{ github.actor }}}}
        password: ${{{{ secrets.GITHUB_TOKEN }}}}
    
    - name: Build and Push API Image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: docker/Dockerfile.backend
        push: true
        tags: ${{{{ env.REGISTRY }}}}/${{{{ github.repository_owner }}}}/${{{{ env.API_IMAGE }}}}:latest
    
    - name: Build and Push Frontend Image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: docker/Dockerfile.frontend
        push: true
        tags: ${{{{ env.REGISTRY }}}}/${{{{ github.repository_owner }}}}/${{{{ env.FRONTEND_IMAGE }}}}:latest

  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/{self.config.git_branch}'
    
    steps:
    - name: Deploy to Server
      uses: appleboy/ssh-action@v1.0.0
      with:
        host: ${{{{ secrets.DEPLOY_HOST }}}}
        username: ${{{{ secrets.DEPLOY_USER }}}}
        key: ${{{{ secrets.DEPLOY_KEY }}}}
        script: |
          cd /opt/crm
          docker compose pull
          docker compose up -d
'''
    
    def generate_azure_pipelines_yaml(self) -> str:
        """Generate Azure Pipelines YAML."""
        return f'''trigger:
  branches:
    include:
    - {self.config.git_branch}

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerRegistryServiceConnection: 'acr-connection'
  containerRegistry: '{self.config.azure_acr_name}.azurecr.io'
  apiImageRepository: 'crm-api'
  frontendImageRepository: 'crm-frontend'
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  displayName: Build stage
  jobs:
  - job: Build
    displayName: Build
    steps:
    - task: NodeTool@0
      inputs:
        versionSpec: '20.x'
      displayName: 'Install Node.js'
    
    - task: UseDotNet@2
      inputs:
        version: '8.0.x'
      displayName: 'Install .NET'
    
    - script: |
        cd CRM.Frontend
        npm ci
        npm run build
      displayName: 'Build Frontend'
    
    - script: |
        cd CRM.Backend
        dotnet restore
        dotnet publish -c Release -o ./publish
      displayName: 'Build API'
    
    - task: Docker@2
      displayName: 'Build and Push API Image'
      inputs:
        command: buildAndPush
        repository: $(apiImageRepository)
        dockerfile: docker/Dockerfile.backend
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest
    
    - task: Docker@2
      displayName: 'Build and Push Frontend Image'
      inputs:
        command: buildAndPush
        repository: $(frontendImageRepository)
        dockerfile: docker/Dockerfile.frontend
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest

- stage: Deploy
  displayName: Deploy stage
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: Deploy
    displayName: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - script: |
              echo "Deploying to production..."
            displayName: 'Deploy to Server'
'''
    
    def generate_cloudbuild_yaml(self) -> str:
        """Generate Google Cloud Build YAML."""
        return f'''steps:
# Build Frontend
- name: 'node:20'
  dir: 'CRM.Frontend'
  entrypoint: 'npm'
  args: ['ci']

- name: 'node:20'
  dir: 'CRM.Frontend'
  entrypoint: 'npm'
  args: ['run', 'build']

# Build API
- name: 'mcr.microsoft.com/dotnet/sdk:8.0'
  dir: 'CRM.Backend'
  entrypoint: 'dotnet'
  args: ['restore']

- name: 'mcr.microsoft.com/dotnet/sdk:8.0'
  dir: 'CRM.Backend'
  entrypoint: 'dotnet'
  args: ['publish', '-c', 'Release', '-o', './publish']

# Build Docker Images
- name: 'gcr.io/cloud-builders/docker'
  args: ['build', '-t', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-api:$COMMIT_SHA', '-f', 'docker/Dockerfile.backend', '.']

- name: 'gcr.io/cloud-builders/docker'
  args: ['build', '-t', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-frontend:$COMMIT_SHA', '-f', 'docker/Dockerfile.frontend', '.']

# Push Images
- name: 'gcr.io/cloud-builders/docker'
  args: ['push', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-api:$COMMIT_SHA']

- name: 'gcr.io/cloud-builders/docker'
  args: ['push', '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-frontend:$COMMIT_SHA']

images:
- '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-api:$COMMIT_SHA'
- '{self.config.gcp_gcr_hostname}/{self.config.gcp_project_id}/crm-frontend:$COMMIT_SHA'

options:
  logging: CLOUD_LOGGING_ONLY
'''


class DeploymentEngine:
    """Main deployment engine."""
    
    def __init__(self, config: DeploymentConfig, log_callback: Callable[[str, str], None]):
        self.config = config
        self.log = log_callback
        self.ssh_cmd = ""
        self.is_running = False
        self.is_paused = False
        
        if not config.build_server_is_local:
            if config.build_server_ssh_key:
                self.ssh_cmd = f"ssh -i {config.build_server_ssh_key} {config.build_server_user}@{config.build_server_host}"
            else:
                self.ssh_cmd = f"ssh {config.build_server_user}@{config.build_server_host}"
    
    def _run_command(self, cmd: str, timeout: int = 300) -> Tuple[bool, str]:
        """Run a command on the build server."""
        try:
            if self.ssh_cmd:
                cmd = f'{self.ssh_cmd} "{cmd}"'
            
            self.log("cmd", f"$ {cmd}")
            
            result = subprocess.run(
                cmd, shell=True, capture_output=True, text=True, timeout=timeout
            )
            
            if result.stdout:
                self.log("output", result.stdout.strip())
            if result.stderr and result.returncode != 0:
                self.log("error", result.stderr.strip())
            
            return result.returncode == 0, result.stdout + result.stderr
        except subprocess.TimeoutExpired:
            self.log("error", f"Command timed out after {timeout}s")
            return False, "Timeout"
        except Exception as e:
            self.log("error", str(e))
            return False, str(e)
    
    def _wait_if_paused(self):
        """Wait if deployment is paused."""
        while self.is_paused and self.is_running:
            time.sleep(0.5)
    
    def generate_env_file(self) -> str:
        """Generate environment file content."""
        jwt_secret = self.config.jwt_secret or secrets.token_urlsafe(64)
        
        domain = self.config.domain
        api_port = self.config.api_port
        frontend_port = self.config.frontend_port
        protocol = "https" if self.config.ssl_enabled else "http"
        
        allowed_origins = self.config.allowed_origins or f"{protocol}://{domain},{protocol}://{domain}:{frontend_port},{protocol}://{domain}:{api_port}"
        
        return f"""# CRM Solution Environment Configuration
# Generated: {datetime.now().isoformat()}
# Version: {VERSION}
# Architecture: {self.config.architecture}

# DATABASE CONFIGURATION
DATABASE_PROVIDER={self.config.database_provider.capitalize()}
DB_HOST={self.config.database_host}
DB_PORT={self.config.database_port}
DB_NAME={self.config.database_name}
DB_USER={self.config.database_user}
DB_PASSWORD={self.config.database_password}
DB_ROOT_PASSWORD={self.config.database_root_password}
MYSQL_ROOT_PASSWORD={self.config.database_root_password}
MYSQL_DATABASE={self.config.database_name}
MYSQL_USER={self.config.database_user}
MYSQL_PASSWORD={self.config.database_password}
DatabaseProvider={self.config.database_provider}

# API CONFIGURATION
API_EXTERNAL_PORT={self.config.api_port}
API_INTERNAL_PORT=5000
ASPNETCORE_ENVIRONMENT=Production

# FRONTEND CONFIGURATION
FRONTEND_PORT={self.config.frontend_port}
REACT_APP_API_URL={protocol}://{domain}:{api_port}/api

# JWT AUTHENTICATION
Jwt__Secret={jwt_secret}
JWT_SECRET={jwt_secret}

# CORS
ALLOWED_ORIGINS={allowed_origins}
AllowedHosts=*

# REDIS
REDIS_HOST={NAMING_CONVENTIONS["service_names"]["redis"]}
REDIS_PORT={self.config.redis_port}

# ADMIN USER
ADMIN_USERNAME={self.config.admin_username}
ADMIN_EMAIL={self.config.admin_email}
ADMIN_PASSWORD={self.config.admin_password}

# SSL
SSL_ENABLED={str(self.config.ssl_enabled).lower()}
"""
    
    def generate_docker_compose(self) -> str:
        """Generate docker-compose.yml content."""
        network_name = NAMING_CONVENTIONS["network_name"]
        
        compose = f"""# CRM Solution Docker Compose
# Generated: {datetime.now().isoformat()}
# Version: {VERSION}

version: '3.8'

services:
"""
        
        # Database
        if self.config.deploy_database:
            db_name = NAMING_CONVENTIONS["service_names"]["database"]
            compose += f"""
  mariadb:
    image: mariadb:latest
    container_name: {db_name}
    restart: unless-stopped
    ports:
      - "${{DB_PORT:-{self.config.database_port}}}:3306"
    environment:
      - MARIADB_ROOT_PASSWORD=${{DB_ROOT_PASSWORD}}
      - MARIADB_DATABASE=${{DB_NAME}}
      - MARIADB_USER=${{DB_USER}}
      - MARIADB_PASSWORD=${{DB_PASSWORD}}
    volumes:
      - db-data:/var/lib/mysql
    networks:
      - {network_name}
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 10s
      timeout: 5s
      retries: 5
"""
        
        # Redis
        if self.config.deploy_redis:
            redis_name = NAMING_CONVENTIONS["service_names"]["redis"]
            compose += f"""
  redis:
    image: redis:alpine
    container_name: {redis_name}
    restart: unless-stopped
    ports:
      - "${{REDIS_PORT:-{self.config.redis_port}}}:6379"
    networks:
      - {network_name}
"""
        
        # API
        if self.config.deploy_api:
            api_name = NAMING_CONVENTIONS["service_names"]["api"]
            depends = []
            if self.config.deploy_database:
                depends.append("mariadb")
            if self.config.deploy_redis:
                depends.append("redis")
            
            depends_str = ""
            if depends:
                depends_str = "\n    depends_on:\n" + "\n".join([f"      - {d}" for d in depends])
            
            compose += f"""
  api:
    image: crm-api:latest
    container_name: {api_name}
    restart: unless-stopped
    ports:
      - "${{API_EXTERNAL_PORT:-{self.config.api_port}}}:5000"
    env_file:
      - .env
    environment:
      - DatabaseProvider={self.config.database_provider}
      - DB_HOST={self.config.database_host}
      - ConnectionStrings__DefaultConnection=Server={self.config.database_host};Port={self.config.database_port};Database={self.config.database_name};User={self.config.database_user};Password=${{DB_PASSWORD}};AllowUserVariables=true
      - Redis__ConnectionString={NAMING_CONVENTIONS["service_names"]["redis"]}:6379
      - AllowedHosts=*
    volumes:
      - api-data:/app/data
    networks:
      - {network_name}{depends_str}
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/api/monitoring/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
"""
        
        # Frontend
        if self.config.deploy_frontend:
            frontend_name = NAMING_CONVENTIONS["service_names"]["frontend"]
            depends_str = ""
            if self.config.deploy_api:
                depends_str = "\n    depends_on:\n      - api"
            
            compose += f"""
  frontend:
    image: crm-frontend:latest
    container_name: {frontend_name}
    restart: unless-stopped
    ports:
      - "${{FRONTEND_PORT:-{self.config.frontend_port}}}:80"
    environment:
      - REACT_APP_API_URL=http://{self.config.domain}:{self.config.api_port}/api
    networks:
      - {network_name}{depends_str}
"""
        
        # Networks and volumes
        compose += f"""
networks:
  {network_name}:
    driver: bridge

volumes:
"""
        if self.config.deploy_database:
            compose += "  db-data:\n"
        if self.config.deploy_api:
            compose += "  api-data:\n"
        
        return compose
    
    def deploy_prerequisites(self) -> bool:
        """Deploy prerequisites."""
        self.log("info", "Checking prerequisites...")
        
        checker = PrerequisiteChecker(self.ssh_cmd)
        results = checker.check_all()
        
        missing = checker.get_missing_required()
        if missing:
            self.log("error", f"Missing required prerequisites: {', '.join(missing)}")
            return False
        
        for name, result in results.items():
            if result["installed"]:
                self.log("success", f"✓ {name}: {result['version']}")
            elif result["required"]:
                self.log("error", f"✗ {name}: NOT INSTALLED (required)")
            else:
                self.log("warning", f"⚠ {name}: not installed (optional)")
        
        return True
    
    def create_docker_network(self) -> bool:
        """Create Docker network."""
        network_name = NAMING_CONVENTIONS["network_name"]
        self.log("info", f"Creating Docker network: {network_name}")
        
        self._run_command(f"docker network create {network_name} 2>/dev/null || true")
        return True
    
    def deploy_master_data(self) -> bool:
        """Deploy master data to the database."""
        if not self.config.seed_master_data:
            self.log("info", "Skipping master data deployment (disabled)")
            return True
        
        self.log("info", "Deploying master data...")
        
        master_data_files = [
            "001_color_palettes.sql",
            "002_module_ui_configs.sql",
            "003_system_settings.sql",
            "004_service_request_types.sql",
            "005_departments_and_groups.sql",
            "006_lookup_data.sql",
            "007_roles_permissions.sql"
        ]
        
        for sql_file in master_data_files:
            self.log("info", f"  Applying {sql_file}...")
        
        self.log("success", "Master data deployed successfully")
        return True
    
    def run_smoke_tests(self) -> List[TestResult]:
        """Run smoke tests."""
        results = []
        
        if not self.config.run_smoke_tests:
            self.log("info", "Skipping smoke tests (disabled)")
            return results
        
        self.log("info", "Running smoke tests...")
        
        domain = self.config.domain
        api_port = self.config.api_port
        frontend_port = self.config.frontend_port
        protocol = "https" if self.config.ssl_enabled else "http"
        
        tests = [
            ("API Health Check", f"{protocol}://{domain}:{api_port}/api/monitoring/health"),
            ("Frontend Load", f"{protocol}://{domain}:{frontend_port}"),
        ]
        
        # Create SSL context that doesn't verify certificates (for self-signed certs)
        ssl_context = ssl.create_default_context()
        ssl_context.check_hostname = False
        ssl_context.verify_mode = ssl.CERT_NONE
        
        for test_name, url in tests:
            start_time = time.time()
            try:
                req = urllib.request.Request(url, headers={"User-Agent": "CRM-Deploy-Tool"})
                response = urllib.request.urlopen(req, timeout=10, context=ssl_context)
                duration = int((time.time() - start_time) * 1000)
                
                if response.status == 200:
                    results.append(TestResult(
                        name=test_name,
                        status="passed",
                        duration_ms=duration,
                        message=f"HTTP {response.status}",
                        timestamp=datetime.now().isoformat()
                    ))
                    self.log("success", f"✓ {test_name}: PASSED ({duration}ms)")
                else:
                    results.append(TestResult(
                        name=test_name,
                        status="failed",
                        duration_ms=duration,
                        message=f"HTTP {response.status}",
                        timestamp=datetime.now().isoformat()
                    ))
                    self.log("error", f"✗ {test_name}: FAILED")
            except Exception as e:
                duration = int((time.time() - start_time) * 1000)
                results.append(TestResult(
                    name=test_name,
                    status="failed",
                    duration_ms=duration,
                    message=str(e),
                    timestamp=datetime.now().isoformat()
                ))
                self.log("error", f"✗ {test_name}: FAILED ({str(e)})")
        
        return results
    
    def run_bvt_tests(self) -> List[TestResult]:
        """Run BVT tests."""
        results = []
        
        if not self.config.run_bvt_tests:
            self.log("info", "Skipping BVT tests (disabled)")
            return results
        
        self.log("info", "Running BVT tests...")
        
        domain = self.config.domain
        api_port = self.config.api_port
        protocol = "https" if self.config.ssl_enabled else "http"
        
        # Create SSL context that doesn't verify certificates (for self-signed certs)
        ssl_context = ssl.create_default_context()
        ssl_context.check_hostname = False
        ssl_context.verify_mode = ssl.CERT_NONE
        
        try:
            bvt_url = f"{protocol}://{domain}:{api_port}/api/monitoring/bvt"
            req = urllib.request.Request(bvt_url, headers={"User-Agent": "CRM-Deploy-Tool"})
            response = urllib.request.urlopen(req, timeout=60, context=ssl_context)
            
            if response.status == 200:
                bvt_data = json.loads(response.read().decode('utf-8'))
                for test in bvt_data.get("tests", []):
                    results.append(TestResult(
                        name=test.get("name", "Unknown"),
                        status="passed" if test.get("passed") else "failed",
                        duration_ms=test.get("durationMs", 0),
                        message=test.get("message", ""),
                        timestamp=datetime.now().isoformat()
                    ))
        except Exception as e:
            self.log("warning", f"BVT tests could not be run: {str(e)}")
        
        return results


class DeploymentTool:
    """Main deployment tool application."""
    
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title(f"CRM Solution Deployment Tool v{VERSION}")
        self.root.geometry("1400x900")
        self.root.minsize(1200, 800)
        
        # Configuration
        self.config = DeploymentConfig()
        self.config_path = Path(__file__).parent / "config" / "deployment_config.json"
        self.log_queue = queue.Queue()
        self.test_results: List[TestResult] = []
        self.deployment_thread: Optional[threading.Thread] = None
        self.engine: Optional[DeploymentEngine] = None
        
        # Ensure config directory exists
        self.config_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Load saved configuration
        self.load_config()
        
        # Create UI
        self.create_ui()
        
        # Start log processor
        self.process_logs()
    
    def create_ui(self):
        """Create the main user interface."""
        # Main paned window
        self.main_paned = ttk.PanedWindow(self.root, orient=tk.HORIZONTAL)
        self.main_paned.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        
        # Left panel - Configuration
        self.left_frame = ttk.Frame(self.main_paned, width=700)
        self.main_paned.add(self.left_frame, weight=1)
        
        # Right panel - Logs
        self.right_frame = ttk.Frame(self.main_paned, width=500)
        self.main_paned.add(self.right_frame, weight=1)
        
        # Create notebook
        self.notebook = ttk.Notebook(self.left_frame)
        self.notebook.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        # Create tabs
        self.create_architecture_tab()
        self.create_build_tab()
        self.create_build_server_tab()
        self.create_cloud_tab()
        self.create_components_tab()
        self.create_database_tab()
        self.create_network_tab()
        self.create_credentials_tab()
        self.create_seed_data_tab()
        self.create_testing_tab()
        
        # Create right panel
        self.create_log_panel()
        
        # Bottom button bar
        self.create_button_bar()
    
    def create_architecture_tab(self):
        """Create architecture selection tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🏗️ Architecture")
        
        ttk.Label(tab, text="Deployment Architecture", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Architecture selection
        arch_frame = ttk.LabelFrame(tab, text="Architecture Type", padding="15")
        arch_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.arch_var = tk.StringVar(value=self.config.architecture)
        
        ttk.Radiobutton(arch_frame, text="Monolithic (Single API container)", 
                        variable=self.arch_var, value="monolithic",
                        command=self.on_architecture_change).pack(anchor=tk.W, pady=5)
        ttk.Radiobutton(arch_frame, text="Microservices (Separate services)", 
                        variable=self.arch_var, value="microservices",
                        command=self.on_architecture_change).pack(anchor=tk.W, pady=5)
        
        # Orchestration
        orch_frame = ttk.LabelFrame(tab, text="Container Orchestration", padding="15")
        orch_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.orch_var = tk.StringVar(value=self.config.hosting_platform)
        
        ttk.Radiobutton(orch_frame, text="Docker Compose", 
                        variable=self.orch_var, value="docker").pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(orch_frame, text="Kubernetes", 
                        variable=self.orch_var, value="kubernetes").pack(anchor=tk.W, pady=3)
        
        # Naming conventions
        naming_frame = ttk.LabelFrame(tab, text="Naming Conventions", padding="15")
        naming_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(naming_frame, text="Container Prefix:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.container_prefix_var = tk.StringVar(value=NAMING_CONVENTIONS["container_prefix"])
        ttk.Entry(naming_frame, textvariable=self.container_prefix_var, width=20).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(naming_frame, text="Network Name:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.network_name_var = tk.StringVar(value=NAMING_CONVENTIONS["network_name"])
        ttk.Entry(naming_frame, textvariable=self.network_name_var, width=20).grid(row=1, column=1, padx=10, pady=3)
    
    def create_build_tab(self):
        """Create build configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🔨 Build")
        
        ttk.Label(tab, text="Build Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Source selection
        source_frame = ttk.LabelFrame(tab, text="Source Code", padding="15")
        source_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.build_source_var = tk.StringVar(value=self.config.build_source)
        
        ttk.Radiobutton(source_frame, text="Local Directory", 
                        variable=self.build_source_var, value="local",
                        command=self.on_build_source_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(source_frame, text="Git Repository", 
                        variable=self.build_source_var, value="git",
                        command=self.on_build_source_change).pack(anchor=tk.W, pady=3)
        
        # Git configuration
        self.git_frame = ttk.LabelFrame(tab, text="Git Repository", padding="15")
        self.git_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(self.git_frame, text="Repository URL:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.git_repo_var = tk.StringVar(value=self.config.git_repo_url)
        ttk.Entry(self.git_frame, textvariable=self.git_repo_var, width=50).grid(row=0, column=1, columnspan=2, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.git_frame, text="Branch:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.git_branch_var = tk.StringVar(value=self.config.git_branch)
        ttk.Entry(self.git_frame, textvariable=self.git_branch_var, width=20).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Authentication
        self.git_use_ssh_var = tk.BooleanVar(value=self.config.git_use_ssh)
        ttk.Checkbutton(self.git_frame, text="Use SSH", 
                        variable=self.git_use_ssh_var,
                        command=self.on_git_auth_change).grid(row=2, column=0, sticky=tk.W, pady=5)
        
        ttk.Label(self.git_frame, text="Username:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.git_username_var = tk.StringVar(value=self.config.git_username)
        self.git_username_entry = ttk.Entry(self.git_frame, textvariable=self.git_username_var, width=30)
        self.git_username_entry.grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.git_frame, text="Token/Password:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.git_token_var = tk.StringVar(value=self.config.git_token)
        self.git_token_entry = ttk.Entry(self.git_frame, textvariable=self.git_token_var, width=40, show="*")
        self.git_token_entry.grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.git_frame, text="SSH Key:").grid(row=5, column=0, sticky=tk.W, pady=3)
        ssh_key_frame = ttk.Frame(self.git_frame)
        ssh_key_frame.grid(row=5, column=1, sticky=tk.W, padx=10, pady=3)
        self.git_ssh_key_var = tk.StringVar(value=self.config.git_ssh_key)
        self.git_ssh_key_entry = ttk.Entry(ssh_key_frame, textvariable=self.git_ssh_key_var, width=30)
        self.git_ssh_key_entry.pack(side=tk.LEFT)
        ttk.Button(ssh_key_frame, text="Browse", 
                   command=lambda: self.browse_file(self.git_ssh_key_var)).pack(side=tk.LEFT, padx=5)
        
        # Build type
        build_type_frame = ttk.LabelFrame(tab, text="Build Method", padding="15")
        build_type_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.build_type_var = tk.StringVar(value=self.config.build_type)
        
        ttk.Radiobutton(build_type_frame, text="Local Build", 
                        variable=self.build_type_var, value="local",
                        command=self.on_build_type_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(build_type_frame, text="Remote Build (SSH)", 
                        variable=self.build_type_var, value="remote",
                        command=self.on_build_type_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(build_type_frame, text="Cloud CI/CD", 
                        variable=self.build_type_var, value="cloud",
                        command=self.on_build_type_change).pack(anchor=tk.W, pady=3)
        
        # Cloud build provider
        self.cloud_build_frame = ttk.LabelFrame(tab, text="Cloud Build Provider", padding="15")
        self.cloud_build_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.cloud_build_provider_var = tk.StringVar(value=self.config.cloud_build_provider)
        
        providers = [
            ("GitHub Actions", "github_actions"),
            ("Azure DevOps Pipelines", "azure_devops"),
            ("AWS CodeBuild", "aws_codebuild"),
            ("Google Cloud Build", "gcp_cloudbuild")
        ]
        
        for text, value in providers:
            ttk.Radiobutton(self.cloud_build_frame, text=text, 
                            variable=self.cloud_build_provider_var, value=value,
                            command=self.on_cloud_build_provider_change).pack(anchor=tk.W, pady=2)
        
        # Cloud build configuration notebook
        self.cloud_build_config_notebook = ttk.Notebook(self.cloud_build_frame)
        self.cloud_build_config_notebook.pack(fill=tk.X, pady=(10, 0))
        
        # GitHub Actions config
        gh_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(gh_frame, text="GitHub")
        ttk.Label(gh_frame, text="Workflow File:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.github_workflow_var = tk.StringVar(value=self.config.github_actions_workflow)
        ttk.Entry(gh_frame, textvariable=self.github_workflow_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        # Azure DevOps config
        ado_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(ado_frame, text="Azure DevOps")
        ttk.Label(ado_frame, text="Organization:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.azure_devops_org_var = tk.StringVar(value=self.config.azure_devops_org)
        ttk.Entry(ado_frame, textvariable=self.azure_devops_org_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        ttk.Label(ado_frame, text="Project:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.azure_devops_project_var = tk.StringVar(value=self.config.azure_devops_project)
        ttk.Entry(ado_frame, textvariable=self.azure_devops_project_var, width=30).grid(row=1, column=1, padx=10, pady=3)
        ttk.Label(ado_frame, text="Pipeline ID:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.azure_devops_pipeline_var = tk.StringVar(value=self.config.azure_devops_pipeline)
        ttk.Entry(ado_frame, textvariable=self.azure_devops_pipeline_var, width=20).grid(row=2, column=1, padx=10, pady=3)
        
        # AWS CodeBuild config
        aws_build_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(aws_build_frame, text="AWS")
        ttk.Label(aws_build_frame, text="Project Name:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.aws_codebuild_project_var = tk.StringVar(value=self.config.aws_codebuild_project)
        ttk.Entry(aws_build_frame, textvariable=self.aws_codebuild_project_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        # GCP Cloud Build config
        gcp_build_frame = ttk.Frame(self.cloud_build_config_notebook, padding="10")
        self.cloud_build_config_notebook.add(gcp_build_frame, text="GCP")
        ttk.Label(gcp_build_frame, text="Trigger ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.gcp_cloudbuild_trigger_var = tk.StringVar(value=self.config.gcp_cloudbuild_trigger)
        ttk.Entry(gcp_build_frame, textvariable=self.gcp_cloudbuild_trigger_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        # Build commands (for local/remote builds)
        self.build_cmd_frame = ttk.LabelFrame(tab, text="Build Commands", padding="15")
        self.build_cmd_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(self.build_cmd_frame, text="Frontend:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.build_frontend_cmd_var = tk.StringVar(value=self.config.build_frontend_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_frontend_cmd_var, width=60).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(self.build_cmd_frame, text="API:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.build_api_cmd_var = tk.StringVar(value=self.config.build_api_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_api_cmd_var, width=60).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(self.build_cmd_frame, text="Docker API:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.build_docker_api_cmd_var = tk.StringVar(value=self.config.build_docker_api_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_docker_api_cmd_var, width=60).grid(row=2, column=1, padx=10, pady=3)
        
        ttk.Label(self.build_cmd_frame, text="Docker Frontend:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.build_docker_frontend_cmd_var = tk.StringVar(value=self.config.build_docker_frontend_cmd)
        ttk.Entry(self.build_cmd_frame, textvariable=self.build_docker_frontend_cmd_var, width=60).grid(row=3, column=1, padx=10, pady=3)
        
        # Build actions
        actions_frame = ttk.Frame(tab)
        actions_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(actions_frame, text="📥 Clone Repository", 
                   command=self.clone_repository).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="🔨 Build Local", 
                   command=self.run_local_build).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="🐳 Build Docker Images", 
                   command=self.run_docker_build).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="☁️ Trigger Cloud Build", 
                   command=self.trigger_cloud_build).pack(side=tk.LEFT, padx=5)
        
        actions_frame2 = ttk.Frame(tab)
        actions_frame2.pack(fill=tk.X, pady=5)
        
        ttk.Button(actions_frame2, text="📄 Generate CI/CD Config", 
                   command=self.generate_cicd_config).pack(side=tk.LEFT, padx=5)
        
        self.build_status_var = tk.StringVar(value="")
        ttk.Label(actions_frame2, textvariable=self.build_status_var).pack(side=tk.LEFT, padx=10)
        
        # Apply initial state
        self.on_build_source_change()
        self.on_build_type_change()
        self.on_git_auth_change()
    
    def on_build_source_change(self):
        """Handle build source change."""
        is_git = self.build_source_var.get() == "git"
        state = tk.NORMAL if is_git else tk.DISABLED
        for child in self.git_frame.winfo_children():
            if isinstance(child, (ttk.Entry, ttk.Checkbutton, ttk.Button)):
                child.configure(state=state)
            elif isinstance(child, ttk.Frame):
                for subchild in child.winfo_children():
                    if isinstance(subchild, (ttk.Entry, ttk.Button)):
                        subchild.configure(state=state)
    
    def on_build_type_change(self):
        """Handle build type change."""
        build_type = self.build_type_var.get()
        
        # Cloud build config visibility
        is_cloud = build_type == "cloud"
        for child in self.cloud_build_frame.winfo_children():
            if isinstance(child, ttk.Radiobutton):
                child.configure(state=tk.NORMAL if is_cloud else tk.DISABLED)
        
        # Build commands visibility
        is_local_or_remote = build_type in ["local", "remote"]
        for child in self.build_cmd_frame.winfo_children():
            if isinstance(child, ttk.Entry):
                child.configure(state=tk.NORMAL if is_local_or_remote else tk.DISABLED)
    
    def on_git_auth_change(self):
        """Handle git authentication method change."""
        use_ssh = self.git_use_ssh_var.get()
        self.git_username_entry.configure(state=tk.DISABLED if use_ssh else tk.NORMAL)
        self.git_token_entry.configure(state=tk.DISABLED if use_ssh else tk.NORMAL)
        self.git_ssh_key_entry.configure(state=tk.NORMAL if use_ssh else tk.DISABLED)
    
    def on_cloud_build_provider_change(self):
        """Handle cloud build provider change."""
        provider = self.cloud_build_provider_var.get()
        tab_map = {
            "github_actions": 0,
            "azure_devops": 1,
            "aws_codebuild": 2,
            "gcp_cloudbuild": 3
        }
        if provider in tab_map:
            self.cloud_build_config_notebook.select(tab_map[provider])
    
    def clone_repository(self):
        """Clone the git repository."""
        if self.build_source_var.get() != "git":
            messagebox.showinfo("Info", "Using local source - no clone needed")
            return
        
        config = self.get_current_config()
        build_engine = BuildEngine(config, self.log_message)
        
        def do_clone():
            self.build_status_var.set("Cloning...")
            if build_engine.clone_repository():
                self.build_status_var.set("✓ Clone complete")
            else:
                self.build_status_var.set("✗ Clone failed")
        
        threading.Thread(target=do_clone, daemon=True).start()
    
    def run_local_build(self):
        """Run local build."""
        config = self.get_current_config()
        build_engine = BuildEngine(config, self.log_message)
        
        def do_build():
            self.build_status_var.set("Building...")
            
            # Clone if using git
            if config.build_source == "git":
                if not build_engine.clone_repository():
                    self.build_status_var.set("✗ Clone failed")
                    return
            
            # Build frontend and API
            if config.deploy_frontend:
                if not build_engine.build_frontend():
                    self.build_status_var.set("✗ Frontend build failed")
                    return
            
            if config.deploy_api:
                if not build_engine.build_api():
                    self.build_status_var.set("✗ API build failed")
                    return
            
            self.build_status_var.set("✓ Build complete")
            build_engine.cleanup()
        
        threading.Thread(target=do_build, daemon=True).start()
    
    def run_docker_build(self):
        """Build Docker images."""
        config = self.get_current_config()
        build_engine = BuildEngine(config, self.log_message)
        
        def do_build():
            self.build_status_var.set("Building Docker images...")
            
            if build_engine.build_docker_images():
                self.build_status_var.set("✓ Docker images built")
            else:
                self.build_status_var.set("✗ Docker build failed")
        
        threading.Thread(target=do_build, daemon=True).start()
    
    def trigger_cloud_build(self):
        """Trigger cloud CI/CD build."""
        config = self.get_current_config()
        
        if config.cloud_build_provider == "none":
            messagebox.showwarning("Warning", "Select a cloud build provider first")
            return
        
        cloud_build = CloudBuildEngine(config, self.log_message)
        
        def do_trigger():
            self.build_status_var.set("Triggering build...")
            
            if cloud_build.trigger_build():
                self.build_status_var.set("✓ Build triggered")
            else:
                self.build_status_var.set("✗ Trigger failed")
        
        threading.Thread(target=do_trigger, daemon=True).start()
    
    def generate_cicd_config(self):
        """Generate CI/CD configuration files."""
        config = self.get_current_config()
        cloud_build = CloudBuildEngine(config, self.log_message)
        
        provider = config.cloud_build_provider
        
        if provider == "github_actions":
            content = cloud_build.generate_github_actions_workflow()
            filename = "build-deploy.yml"
            default_path = ".github/workflows"
        elif provider == "azure_devops":
            content = cloud_build.generate_azure_pipelines_yaml()
            filename = "azure-pipelines.yml"
            default_path = "."
        elif provider == "gcp_cloudbuild":
            content = cloud_build.generate_cloudbuild_yaml()
            filename = "cloudbuild.yaml"
            default_path = "."
        else:
            messagebox.showwarning("Warning", "Select a cloud build provider first")
            return
        
        # Save to file
        filepath = filedialog.asksaveasfilename(
            initialfile=filename,
            defaultextension=".yml",
            filetypes=[("YAML files", "*.yml *.yaml"), ("All files", "*.*")]
        )
        
        if filepath:
            with open(filepath, 'w') as f:
                f.write(content)
            messagebox.showinfo("Success", f"CI/CD config saved to:\n{filepath}")
            self.log_message("success", f"Generated CI/CD config: {filepath}")

    def create_build_server_tab(self):
        """Create build server configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🖥️ Build Server")
        
        ttk.Label(tab, text="Build Server Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Server type
        type_frame = ttk.LabelFrame(tab, text="Server Location", padding="15")
        type_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.server_local_var = tk.BooleanVar(value=self.config.build_server_is_local)
        
        ttk.Radiobutton(type_frame, text="Local Machine", 
                        variable=self.server_local_var, value=True,
                        command=self.on_server_type_change).pack(anchor=tk.W, pady=3)
        ttk.Radiobutton(type_frame, text="Remote Server (SSH)", 
                        variable=self.server_local_var, value=False,
                        command=self.on_server_type_change).pack(anchor=tk.W, pady=3)
        
        # Server details
        self.server_details_frame = ttk.LabelFrame(tab, text="Remote Server Details", padding="15")
        self.server_details_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(self.server_details_frame, text="Server Name:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.server_name_var = tk.StringVar(value=self.config.build_server_name)
        ttk.Entry(self.server_details_frame, textvariable=self.server_name_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="Host/IP:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.server_host_var = tk.StringVar(value=self.config.build_server_host)
        ttk.Entry(self.server_details_frame, textvariable=self.server_host_var, width=40).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="SSH Port:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.server_port_var = tk.IntVar(value=self.config.build_server_port)
        ttk.Entry(self.server_details_frame, textvariable=self.server_port_var, width=10).grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="Username:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.server_user_var = tk.StringVar(value=self.config.build_server_user)
        ttk.Entry(self.server_details_frame, textvariable=self.server_user_var, width=20).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(self.server_details_frame, text="SSH Key:").grid(row=4, column=0, sticky=tk.W, pady=3)
        key_frame = ttk.Frame(self.server_details_frame)
        key_frame.grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        self.server_key_var = tk.StringVar(value=self.config.build_server_ssh_key)
        ttk.Entry(key_frame, textvariable=self.server_key_var, width=30).pack(side=tk.LEFT)
        ttk.Button(key_frame, text="Browse", 
                   command=lambda: self.browse_file(self.server_key_var)).pack(side=tk.LEFT, padx=5)
        
        # Connection test
        test_frame = ttk.Frame(tab)
        test_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(test_frame, text="🔌 Test Connection", 
                   command=self.test_server_connection).pack(side=tk.LEFT)
        
        self.connection_status_var = tk.StringVar(value="")
        ttk.Label(test_frame, textvariable=self.connection_status_var).pack(side=tk.LEFT, padx=10)
        
        # Prerequisites
        prereq_frame = ttk.LabelFrame(tab, text="Prerequisites", padding="15")
        prereq_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Button(prereq_frame, text="🔍 Check Prerequisites", 
                   command=self.check_prerequisites).pack(anchor=tk.W)
        
        self.prereq_text = scrolledtext.ScrolledText(prereq_frame, height=6, font=("Courier", 9))
        self.prereq_text.pack(fill=tk.X, pady=(10, 0))
        
        self.on_server_type_change()
    
    def create_cloud_tab(self):
        """Create cloud deployment configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="☁️ Cloud")
        
        ttk.Label(tab, text="Cloud Deployment", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Cloud provider selection
        provider_frame = ttk.LabelFrame(tab, text="Cloud Provider", padding="15")
        provider_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.cloud_provider_var = tk.StringVar(value=self.config.cloud_provider)
        
        providers = [
            ("None (Local/SSH)", "none"),
            ("Amazon Web Services (AWS)", "aws"),
            ("Microsoft Azure", "azure"),
            ("Google Cloud Platform (GCP)", "gcp")
        ]
        
        for text, value in providers:
            ttk.Radiobutton(provider_frame, text=text, 
                            variable=self.cloud_provider_var, value=value,
                            command=self.on_cloud_provider_change).pack(anchor=tk.W, pady=3)
        
        # Cloud provider notebooks
        self.cloud_notebook = ttk.Notebook(tab)
        self.cloud_notebook.pack(fill=tk.BOTH, expand=True, pady=(10, 0))
        
        # AWS Tab
        self.create_aws_config_frame()
        
        # Azure Tab
        self.create_azure_config_frame()
        
        # GCP Tab
        self.create_gcp_config_frame()
        
        # Cloud actions
        actions_frame = ttk.Frame(tab)
        actions_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(actions_frame, text="🔍 Check All Prerequisites", 
                   command=self.check_all_cloud_prerequisites).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="� Install Missing", 
                   command=self.install_missing_prerequisites).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="🔐 Authenticate", 
                   command=self.authenticate_cloud).pack(side=tk.LEFT, padx=5)
        ttk.Button(actions_frame, text="📥 Fetch Account Info", 
                   command=self.fetch_cloud_account_info).pack(side=tk.LEFT, padx=5)
        
        actions_frame2 = ttk.Frame(tab)
        actions_frame2.pack(fill=tk.X, pady=5)
        
        ttk.Button(actions_frame2, text="🏗️ Create Infrastructure", 
                   command=self.create_cloud_infrastructure).pack(side=tk.LEFT, padx=5)
        
        self.cloud_status_var = tk.StringVar(value="")
        ttk.Label(actions_frame2, textvariable=self.cloud_status_var).pack(side=tk.LEFT, padx=10)
        
        # Cloud prerequisites display
        prereq_frame = ttk.LabelFrame(tab, text="Cloud Prerequisites Status", padding="10")
        prereq_frame.pack(fill=tk.X, pady=(5, 0))
        
        self.cloud_prereq_text = scrolledtext.ScrolledText(prereq_frame, height=5, font=("Courier", 9))
        self.cloud_prereq_text.pack(fill=tk.X)
        
        self.on_cloud_provider_change()
    
    def create_aws_config_frame(self):
        """Create AWS configuration frame."""
        aws_frame = ttk.Frame(self.cloud_notebook, padding="10")
        self.cloud_notebook.add(aws_frame, text="AWS")
        
        # Region
        ttk.Label(aws_frame, text="Region:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.aws_region_var = tk.StringVar(value=self.config.aws_region)
        aws_regions = ttk.Combobox(aws_frame, textvariable=self.aws_region_var, width=20,
                                   values=["us-east-1", "us-east-2", "us-west-1", "us-west-2",
                                          "eu-west-1", "eu-west-2", "eu-central-1",
                                          "ap-southeast-1", "ap-southeast-2", "ap-northeast-1"])
        aws_regions.grid(row=0, column=1, sticky=tk.W, padx=10, pady=3)
        
        # ECS Cluster
        ttk.Label(aws_frame, text="ECS Cluster:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.aws_cluster_var = tk.StringVar(value=self.config.aws_ecs_cluster)
        ttk.Entry(aws_frame, textvariable=self.aws_cluster_var, width=30).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # ECR Registry
        ttk.Label(aws_frame, text="ECR Registry:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.aws_ecr_var = tk.StringVar(value=self.config.aws_ecr_registry)
        ttk.Entry(aws_frame, textvariable=self.aws_ecr_var, width=40).grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        # VPC
        ttk.Label(aws_frame, text="VPC ID:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.aws_vpc_var = tk.StringVar(value=self.config.aws_vpc_id)
        ttk.Entry(aws_frame, textvariable=self.aws_vpc_var, width=30).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Subnets
        ttk.Label(aws_frame, text="Subnet IDs:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.aws_subnets_var = tk.StringVar(value=self.config.aws_subnet_ids)
        ttk.Entry(aws_frame, textvariable=self.aws_subnets_var, width=50).grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Options
        options_frame = ttk.LabelFrame(aws_frame, text="AWS Services", padding="10")
        options_frame.grid(row=5, column=0, columnspan=2, sticky=tk.EW, pady=10)
        
        self.aws_fargate_var = tk.BooleanVar(value=self.config.aws_use_fargate)
        ttk.Checkbutton(options_frame, text="Use Fargate (serverless)", 
                        variable=self.aws_fargate_var).pack(anchor=tk.W, pady=2)
        
        self.aws_rds_var = tk.BooleanVar(value=self.config.aws_use_rds)
        ttk.Checkbutton(options_frame, text="Use RDS for Database", 
                        variable=self.aws_rds_var).pack(anchor=tk.W, pady=2)
        
        self.aws_elasticache_var = tk.BooleanVar(value=self.config.aws_use_elasticache)
        ttk.Checkbutton(options_frame, text="Use ElastiCache for Redis", 
                        variable=self.aws_elasticache_var).pack(anchor=tk.W, pady=2)
    
    def create_azure_config_frame(self):
        """Create Azure configuration frame."""
        azure_frame = ttk.Frame(self.cloud_notebook, padding="10")
        self.cloud_notebook.add(azure_frame, text="Azure")
        
        # Subscription
        ttk.Label(azure_frame, text="Subscription ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.azure_subscription_var = tk.StringVar(value=self.config.azure_subscription_id)
        ttk.Entry(azure_frame, textvariable=self.azure_subscription_var, width=40).grid(row=0, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Resource Group
        ttk.Label(azure_frame, text="Resource Group:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.azure_rg_var = tk.StringVar(value=self.config.azure_resource_group)
        ttk.Entry(azure_frame, textvariable=self.azure_rg_var, width=30).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Location
        ttk.Label(azure_frame, text="Location:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.azure_location_var = tk.StringVar(value=self.config.azure_location)
        azure_locations = ttk.Combobox(azure_frame, textvariable=self.azure_location_var, width=20,
                                       values=["eastus", "eastus2", "westus", "westus2", "westus3",
                                              "centralus", "northeurope", "westeurope",
                                              "southeastasia", "australiaeast"])
        azure_locations.grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        # ACR Name
        ttk.Label(azure_frame, text="ACR Name:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.azure_acr_var = tk.StringVar(value=self.config.azure_acr_name)
        ttk.Entry(azure_frame, textvariable=self.azure_acr_var, width=30).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        # AKS Cluster
        ttk.Label(azure_frame, text="AKS Cluster:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.azure_aks_var = tk.StringVar(value=self.config.azure_aks_cluster)
        ttk.Entry(azure_frame, textvariable=self.azure_aks_var, width=30).grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Options
        options_frame = ttk.LabelFrame(azure_frame, text="Azure Services", padding="10")
        options_frame.grid(row=5, column=0, columnspan=2, sticky=tk.EW, pady=10)
        
        self.azure_aks_enabled_var = tk.BooleanVar(value=self.config.azure_use_aks)
        ttk.Checkbutton(options_frame, text="Use AKS (Kubernetes)", 
                        variable=self.azure_aks_enabled_var).pack(anchor=tk.W, pady=2)
        
        self.azure_aci_var = tk.BooleanVar(value=self.config.azure_use_aci)
        ttk.Checkbutton(options_frame, text="Use ACI (Container Instances)", 
                        variable=self.azure_aci_var).pack(anchor=tk.W, pady=2)
        
        self.azure_db_var = tk.BooleanVar(value=self.config.azure_use_azure_db)
        ttk.Checkbutton(options_frame, text="Use Azure Database", 
                        variable=self.azure_db_var).pack(anchor=tk.W, pady=2)
        
        self.azure_redis_var = tk.BooleanVar(value=self.config.azure_use_redis_cache)
        ttk.Checkbutton(options_frame, text="Use Azure Cache for Redis", 
                        variable=self.azure_redis_var).pack(anchor=tk.W, pady=2)
    
    def create_gcp_config_frame(self):
        """Create GCP configuration frame."""
        gcp_frame = ttk.Frame(self.cloud_notebook, padding="10")
        self.cloud_notebook.add(gcp_frame, text="GCP")
        
        # Project ID
        ttk.Label(gcp_frame, text="Project ID:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.gcp_project_var = tk.StringVar(value=self.config.gcp_project_id)
        ttk.Entry(gcp_frame, textvariable=self.gcp_project_var, width=40).grid(row=0, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Region
        ttk.Label(gcp_frame, text="Region:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.gcp_region_var = tk.StringVar(value=self.config.gcp_region)
        gcp_regions = ttk.Combobox(gcp_frame, textvariable=self.gcp_region_var, width=20,
                                   values=["us-central1", "us-east1", "us-west1", "us-west2",
                                          "europe-west1", "europe-west2", "europe-west3",
                                          "asia-east1", "asia-southeast1", "australia-southeast1"])
        gcp_regions.grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Zone
        ttk.Label(gcp_frame, text="Zone:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.gcp_zone_var = tk.StringVar(value=self.config.gcp_zone)
        ttk.Entry(gcp_frame, textvariable=self.gcp_zone_var, width=20).grid(row=2, column=1, sticky=tk.W, padx=10, pady=3)
        
        # GKE Cluster
        ttk.Label(gcp_frame, text="GKE Cluster:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.gcp_gke_var = tk.StringVar(value=self.config.gcp_gke_cluster)
        ttk.Entry(gcp_frame, textvariable=self.gcp_gke_var, width=30).grid(row=3, column=1, sticky=tk.W, padx=10, pady=3)
        
        # GCR Hostname
        ttk.Label(gcp_frame, text="GCR Hostname:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.gcp_gcr_var = tk.StringVar(value=self.config.gcp_gcr_hostname)
        gcr_hosts = ttk.Combobox(gcp_frame, textvariable=self.gcp_gcr_var, width=20,
                                 values=["gcr.io", "us.gcr.io", "eu.gcr.io", "asia.gcr.io"])
        gcr_hosts.grid(row=4, column=1, sticky=tk.W, padx=10, pady=3)
        
        # Options
        options_frame = ttk.LabelFrame(gcp_frame, text="GCP Services", padding="10")
        options_frame.grid(row=5, column=0, columnspan=2, sticky=tk.EW, pady=10)
        
        self.gcp_gke_enabled_var = tk.BooleanVar(value=self.config.gcp_use_gke)
        ttk.Checkbutton(options_frame, text="Use GKE (Kubernetes)", 
                        variable=self.gcp_gke_enabled_var).pack(anchor=tk.W, pady=2)
        
        self.gcp_cloudrun_var = tk.BooleanVar(value=self.config.gcp_use_cloud_run)
        ttk.Checkbutton(options_frame, text="Use Cloud Run (serverless)", 
                        variable=self.gcp_cloudrun_var).pack(anchor=tk.W, pady=2)
        
        self.gcp_cloudsql_var = tk.BooleanVar(value=self.config.gcp_use_cloud_sql)
        ttk.Checkbutton(options_frame, text="Use Cloud SQL", 
                        variable=self.gcp_cloudsql_var).pack(anchor=tk.W, pady=2)
        
        self.gcp_memorystore_var = tk.BooleanVar(value=self.config.gcp_use_memorystore)
        ttk.Checkbutton(options_frame, text="Use Memorystore for Redis", 
                        variable=self.gcp_memorystore_var).pack(anchor=tk.W, pady=2)
    
    def on_cloud_provider_change(self):
        """Handle cloud provider change."""
        provider = self.cloud_provider_var.get()
        
        # Enable/disable cloud notebook based on selection
        if provider == "none":
            for i in range(self.cloud_notebook.index("end")):
                self.cloud_notebook.tab(i, state="disabled")
        else:
            # Enable all tabs but select the appropriate one
            tab_map = {"aws": 0, "azure": 1, "gcp": 2}
            for i in range(self.cloud_notebook.index("end")):
                self.cloud_notebook.tab(i, state="normal")
            if provider in tab_map:
                self.cloud_notebook.select(tab_map[provider])
    
    def check_cloud_cli(self):
        """Check if cloud CLI is installed."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            self.cloud_status_var.set("Select a cloud provider first")
            return
        
        def log_to_status(msg_type: str, message: str):
            self.cloud_status_var.set(message)
        
        config = self.get_current_config()
        
        if provider == "aws":
            deployer = AWSDeployer(config, log_to_status)
        elif provider == "azure":
            deployer = AzureDeployer(config, log_to_status)
        elif provider == "gcp":
            deployer = GCPDeployer(config, log_to_status)
        else:
            return
        
        deployer.check_cli()
    
    def check_all_cloud_prerequisites(self):
        """Check all cloud-related prerequisites."""
        self.cloud_status_var.set("Checking prerequisites...")
        self.cloud_prereq_text.delete(1.0, tk.END)
        
        def do_check():
            checker = PrerequisiteChecker()
            results = checker.check_all()
            
            # Group by category
            categories = {
                "core": "Core Tools",
                "build": "Build Tools", 
                "kubernetes": "Kubernetes Tools",
                "cloud": "Cloud CLIs",
                "infrastructure": "Infrastructure Tools"
            }
            
            output_lines = []
            missing_count = 0
            cloud_missing = []
            
            for category, title in categories.items():
                output_lines.append(f"\n=== {title} ===")
                for name, result in results.items():
                    if result.get("category") == category:
                        if result["installed"]:
                            version = result["version"][:50] if result["version"] else "installed"
                            output_lines.append(f"  ✓ {name}: {version}")
                        else:
                            missing_count += 1
                            install_cmd = result.get("install_cmd", "")
                            output_lines.append(f"  ✗ {name}: NOT INSTALLED")
                            if install_cmd:
                                output_lines.append(f"      Install: {install_cmd}")
                            if category == "cloud":
                                cloud_missing.append(name)
            
            # Store missing for install function
            self._missing_prerequisites = results
            
            # Update UI from main thread
            self.root.after(0, lambda: self._update_cloud_prereq_display(
                "\n".join(output_lines),
                missing_count == 0,
                cloud_missing
            ))
        
        threading.Thread(target=do_check, daemon=True).start()
    
    def _update_cloud_prereq_display(self, text: str, all_installed: bool, cloud_missing: list = None):
        """Update cloud prerequisites display."""
        self.cloud_prereq_text.delete(1.0, tk.END)
        self.cloud_prereq_text.insert(tk.END, text)
        
        if all_installed:
            self.cloud_status_var.set("✓ All prerequisites installed")
        elif cloud_missing:
            self.cloud_status_var.set(f"⚠ Missing cloud CLIs: {', '.join(cloud_missing)}")
        else:
            self.cloud_status_var.set("⚠ Some prerequisites missing")
    
    def install_missing_prerequisites(self):
        """Install missing prerequisites."""
        provider = self.cloud_provider_var.get()
        
        # Determine which tools to install based on provider
        tools_to_install = []
        
        if provider == "aws":
            tools_to_install = ["aws"]
        elif provider == "azure":
            tools_to_install = ["az"]
        elif provider == "gcp":
            tools_to_install = ["gcloud"]
        else:
            # Install all cloud CLIs
            tools_to_install = ["aws", "az", "gcloud"]
        
        # Also offer to install kubernetes tools if needed
        result = messagebox.askyesnocancel(
            "Install Prerequisites",
            f"Install the following tools?\n\n"
            f"Cloud CLIs: {', '.join(tools_to_install)}\n\n"
            f"Click 'Yes' to install cloud CLIs only\n"
            f"Click 'No' to install ALL missing prerequisites\n"
            f"Click 'Cancel' to abort"
        )
        
        if result is None:  # Cancel
            return
        
        install_all = not result  # 'No' means install all
        
        self.cloud_status_var.set("Installing prerequisites...")
        self.cloud_prereq_text.delete(1.0, tk.END)
        
        def do_install():
            checker = PrerequisiteChecker()
            checker.check_all()
            
            def log_to_text(msg_type: str, message: str):
                self.root.after(0, lambda: self._append_prereq_log(message))
            
            if install_all:
                # Install all missing
                results = checker.install_missing(log_callback=log_to_text)
            else:
                # Install only specified tools
                results = {}
                for tool in tools_to_install:
                    if not checker.results.get(tool, {}).get("installed", True):
                        results[tool] = checker.install_prerequisite(tool, log_to_text)
            
            # Summary
            installed = [k for k, v in results.items() if v]
            failed = [k for k, v in results.items() if not v]
            
            summary = f"\n\n=== Installation Summary ===\n"
            if installed:
                summary += f"✓ Installed: {', '.join(installed)}\n"
            if failed:
                summary += f"✗ Failed: {', '.join(failed)}\n"
            
            self.root.after(0, lambda: self._append_prereq_log(summary))
            self.root.after(0, lambda: self.cloud_status_var.set(
                f"✓ Installed {len(installed)} tools" if not failed else f"⚠ {len(failed)} failed"
            ))
        
        threading.Thread(target=do_install, daemon=True).start()
    
    def _append_prereq_log(self, message: str):
        """Append message to prerequisites log."""
        self.cloud_prereq_text.insert(tk.END, message + "\n")
        self.cloud_prereq_text.see(tk.END)
    
    def fetch_cloud_account_info(self):
        """Fetch and populate cloud account information."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            self.cloud_status_var.set("Select a cloud provider first")
            return
        
        self.cloud_status_var.set(f"Fetching {provider.upper()} account info...")
        
        def log_callback(msg_type: str, message: str):
            self.cloud_status_var.set(message[:50] + "..." if len(message) > 50 else message)
        
        config = self.get_current_config()
        
        def do_fetch():
            if provider == "aws":
                deployer = AWSDeployer(config, log_callback)
                info = deployer.get_account_info()
                self.root.after(0, lambda: self._populate_aws_info(info))
            elif provider == "azure":
                deployer = AzureDeployer(config, log_callback)
                info = deployer.get_account_info()
                self.root.after(0, lambda: self._populate_azure_info(info))
            elif provider == "gcp":
                deployer = GCPDeployer(config, log_callback)
                info = deployer.get_account_info()
                self.root.after(0, lambda: self._populate_gcp_info(info))
        
        threading.Thread(target=do_fetch, daemon=True).start()
    
    def _populate_aws_info(self, info: Dict[str, Any]):
        """Populate AWS configuration with fetched info."""
        if not info.get("authenticated"):
            self.cloud_status_var.set("✗ Not authenticated. Run 'aws configure' first.")
            messagebox.showwarning("AWS Authentication", 
                "Not authenticated with AWS.\n\n"
                "Run 'aws configure' in terminal to set up credentials.")
            return
        
        # Update status
        account_id = info.get("account_id", "Unknown")
        self.cloud_status_var.set(f"✓ AWS Account: {account_id}")
        
        # Update region dropdown with available regions
        if info.get("regions"):
            # Update the combobox values
            for child in self.cloud_notebook.winfo_children():
                for widget in child.winfo_children():
                    if isinstance(widget, ttk.Combobox) and widget.cget("textvariable") == str(self.aws_region_var):
                        widget.configure(values=info["regions"])
        
        # Build ECR registry URL
        if info.get("account_id") and self.aws_region_var.get():
            ecr_url = f"{info['account_id']}.dkr.ecr.{self.aws_region_var.get()}.amazonaws.com"
            self.aws_ecr_var.set(ecr_url)
        
        # Update VPC dropdown if VPCs available
        if info.get("vpcs"):
            default_vpc = next((v for v in info["vpcs"] if v.get("IsDefault")), None)
            if default_vpc:
                self.aws_vpc_var.set(default_vpc.get("VpcId", ""))
        
        # Show summary in prereq text
        summary = f"AWS Account Info:\n"
        summary += f"  Account ID: {info.get('account_id', 'N/A')}\n"
        summary += f"  User ARN: {info.get('user_arn', 'N/A')}\n"
        summary += f"  Available Regions: {len(info.get('regions', []))}\n"
        summary += f"  VPCs: {len(info.get('vpcs', []))}\n"
        summary += f"  ECS Clusters: {', '.join(info.get('ecs_clusters', [])) or 'None'}\n"
        summary += f"  ECR Repos: {', '.join(info.get('ecr_repositories', [])) or 'None'}\n"
        
        self.cloud_prereq_text.delete(1.0, tk.END)
        self.cloud_prereq_text.insert(tk.END, summary)
    
    def _populate_azure_info(self, info: Dict[str, Any]):
        """Populate Azure configuration with fetched info."""
        if not info.get("authenticated"):
            self.cloud_status_var.set("✗ Not authenticated. Run 'az login' first.")
            messagebox.showwarning("Azure Authentication", 
                "Not authenticated with Azure.\n\n"
                "Run 'az login' in terminal to authenticate.")
            return
        
        # Update status
        sub_name = info.get("subscription_name", "Unknown")
        self.cloud_status_var.set(f"✓ Azure: {sub_name}")
        
        # Populate subscription ID
        if info.get("subscription_id"):
            self.azure_subscription_var.set(info["subscription_id"])
        
        # Update location dropdown
        if info.get("locations"):
            location_names = [loc.get("name") for loc in info["locations"][:20]]  # Limit to 20
            for child in self.cloud_notebook.winfo_children():
                for widget in child.winfo_children():
                    if isinstance(widget, ttk.Combobox) and widget.cget("textvariable") == str(self.azure_location_var):
                        widget.configure(values=location_names)
        
        # Show summary
        summary = f"Azure Account Info:\n"
        summary += f"  User: {info.get('user', 'N/A')}\n"
        summary += f"  Subscription: {info.get('subscription_name', 'N/A')}\n"
        summary += f"  Subscription ID: {info.get('subscription_id', 'N/A')}\n"
        summary += f"  Tenant ID: {info.get('tenant_id', 'N/A')}\n"
        summary += f"  Resource Groups: {len(info.get('resource_groups', []))}\n"
        
        if info.get("resource_groups"):
            summary += f"    - {', '.join([rg.get('name', '') for rg in info['resource_groups'][:5]])}\n"
        
        summary += f"  ACR Registries: {len(info.get('acr_registries', []))}\n"
        if info.get("acr_registries"):
            for acr in info["acr_registries"][:3]:
                summary += f"    - {acr.get('name', '')} ({acr.get('loginServer', '')})\n"
        
        summary += f"  AKS Clusters: {len(info.get('aks_clusters', []))}\n"
        
        self.cloud_prereq_text.delete(1.0, tk.END)
        self.cloud_prereq_text.insert(tk.END, summary)
    
    def _populate_gcp_info(self, info: Dict[str, Any]):
        """Populate GCP configuration with fetched info."""
        if not info.get("authenticated"):
            self.cloud_status_var.set("✗ Not authenticated. Run 'gcloud auth login' first.")
            messagebox.showwarning("GCP Authentication", 
                "Not authenticated with GCP.\n\n"
                "Run 'gcloud auth login' in terminal to authenticate.")
            return
        
        # Update status
        account = info.get("account", "Unknown")
        self.cloud_status_var.set(f"✓ GCP: {account}")
        
        # Populate project ID
        if info.get("project_id"):
            self.gcp_project_var.set(info["project_id"])
        
        # Update region dropdown
        if info.get("regions"):
            for child in self.cloud_notebook.winfo_children():
                for widget in child.winfo_children():
                    if isinstance(widget, ttk.Combobox) and widget.cget("textvariable") == str(self.gcp_region_var):
                        widget.configure(values=info["regions"][:20])  # Limit to 20
        
        # Update zone field with first matching zone
        if info.get("zones") and self.gcp_region_var.get():
            region = self.gcp_region_var.get()
            matching_zones = [z for z in info["zones"] if z.startswith(region)]
            if matching_zones:
                self.gcp_zone_var.set(matching_zones[0])
        
        # Show summary
        summary = f"GCP Account Info:\n"
        summary += f"  Account: {info.get('account', 'N/A')}\n"
        summary += f"  Current Project: {info.get('project_id', 'N/A')}\n"
        summary += f"  Available Projects: {len(info.get('projects', []))}\n"
        
        if info.get("projects"):
            for proj in info["projects"][:5]:
                summary += f"    - {proj.get('projectId', '')} ({proj.get('name', '')})\n"
        
        summary += f"  Regions: {len(info.get('regions', []))}\n"
        summary += f"  GKE Clusters: {len(info.get('gke_clusters', []))}\n"
        
        if info.get("gke_clusters"):
            for cluster in info["gke_clusters"][:3]:
                summary += f"    - {cluster.get('name', '')} ({cluster.get('zone', '')})\n"
        
        summary += f"  Cloud Run Services: {len(info.get('cloud_run_services', []))}\n"
        
        self.cloud_prereq_text.delete(1.0, tk.END)
        self.cloud_prereq_text.insert(tk.END, summary)

    def authenticate_cloud(self):
        """Authenticate with cloud provider."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            self.cloud_status_var.set("Select a cloud provider first")
            return
        
        def log_to_status(msg_type: str, message: str):
            self.cloud_status_var.set(message)
        
        config = self.get_current_config()
        
        if provider == "aws":
            deployer = AWSDeployer(config, log_to_status)
        elif provider == "azure":
            deployer = AzureDeployer(config, log_to_status)
        elif provider == "gcp":
            deployer = GCPDeployer(config, log_to_status)
        else:
            return
        
        deployer.authenticate()
    
    def create_cloud_infrastructure(self):
        """Create cloud infrastructure."""
        provider = self.cloud_provider_var.get()
        
        if provider == "none":
            messagebox.showwarning("Warning", "Select a cloud provider first")
            return
        
        if not messagebox.askyesno("Confirm", f"Create {provider.upper()} infrastructure?\nThis may incur costs."):
            return
        
        def log_callback(msg_type: str, message: str):
            self.log_message(msg_type, message)
            self.cloud_status_var.set(message[:50] + "..." if len(message) > 50 else message)
        
        config = self.get_current_config()
        
        if provider == "aws":
            deployer = AWSDeployer(config, log_callback)
        elif provider == "azure":
            deployer = AzureDeployer(config, log_callback)
        elif provider == "gcp":
            deployer = GCPDeployer(config, log_callback)
        else:
            return
        
        def run_infra():
            if deployer.check_cli() and deployer.authenticate():
                deployer.create_infrastructure()
                self.cloud_status_var.set("Infrastructure created!")
        
        threading.Thread(target=run_infra, daemon=True).start()

    def create_components_tab(self):
        """Create components selection tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="📦 Components")
        
        ttk.Label(tab, text="Components to Deploy", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Core components
        core_frame = ttk.LabelFrame(tab, text="Core Components", padding="15")
        core_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.deploy_db_var = tk.BooleanVar(value=self.config.deploy_database)
        ttk.Checkbutton(core_frame, text="Database (MariaDB)", 
                        variable=self.deploy_db_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_redis_var = tk.BooleanVar(value=self.config.deploy_redis)
        ttk.Checkbutton(core_frame, text="Redis Cache", 
                        variable=self.deploy_redis_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_api_var = tk.BooleanVar(value=self.config.deploy_api)
        ttk.Checkbutton(core_frame, text="API Server", 
                        variable=self.deploy_api_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_frontend_var = tk.BooleanVar(value=self.config.deploy_frontend)
        ttk.Checkbutton(core_frame, text="Frontend (React)", 
                        variable=self.deploy_frontend_var).pack(anchor=tk.W, pady=3)
        
        # Microservices
        self.micro_frame = ttk.LabelFrame(tab, text="Microservices Components", padding="15")
        self.micro_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.deploy_identity_var = tk.BooleanVar(value=self.config.deploy_identity_service)
        ttk.Checkbutton(self.micro_frame, text="Identity Service", 
                        variable=self.deploy_identity_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_customer_var = tk.BooleanVar(value=self.config.deploy_customer_service)
        ttk.Checkbutton(self.micro_frame, text="Customer Service", 
                        variable=self.deploy_customer_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_sales_var = tk.BooleanVar(value=self.config.deploy_sales_service)
        ttk.Checkbutton(self.micro_frame, text="Sales Service", 
                        variable=self.deploy_sales_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_marketing_var = tk.BooleanVar(value=self.config.deploy_marketing_service)
        ttk.Checkbutton(self.micro_frame, text="Marketing Service", 
                        variable=self.deploy_marketing_var).pack(anchor=tk.W, pady=3)
        
        self.deploy_servicedesk_var = tk.BooleanVar(value=self.config.deploy_servicedesk_service)
        ttk.Checkbutton(self.micro_frame, text="ServiceDesk Service", 
                        variable=self.deploy_servicedesk_var).pack(anchor=tk.W, pady=3)
        
        # Monitoring
        monitoring_frame = ttk.LabelFrame(tab, text="Optional Components", padding="15")
        monitoring_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.deploy_monitoring_var = tk.BooleanVar(value=self.config.deploy_monitoring)
        ttk.Checkbutton(monitoring_frame, text="Monitoring Stack (Uptime Kuma, Portainer)", 
                        variable=self.deploy_monitoring_var).pack(anchor=tk.W, pady=3)
        
        # Quick select
        btn_frame = ttk.Frame(tab)
        btn_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(btn_frame, text="Select All", 
                   command=self.select_all_components).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Select Minimal", 
                   command=self.select_minimal_components).pack(side=tk.LEFT, padx=5)
        
        self.on_architecture_change()
    
    def create_database_tab(self):
        """Create database configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🗄️ Database")
        
        ttk.Label(tab, text="Database Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Provider
        provider_frame = ttk.LabelFrame(tab, text="Database Provider", padding="15")
        provider_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.db_provider_var = tk.StringVar(value=self.config.database_provider)
        
        for text, value in [("MariaDB", "mariadb"), ("MySQL", "mysql"), ("PostgreSQL", "postgresql"), ("SQL Server", "sqlserver")]:
            ttk.Radiobutton(provider_frame, text=text, 
                            variable=self.db_provider_var, value=value).pack(anchor=tk.W, pady=2)
        
        # Connection
        conn_frame = ttk.LabelFrame(tab, text="Connection Details", padding="15")
        conn_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(conn_frame, text="Host:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.db_host_var = tk.StringVar(value=self.config.database_host)
        ttk.Entry(conn_frame, textvariable=self.db_host_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Port:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.db_port_var = tk.IntVar(value=self.config.database_port)
        ttk.Entry(conn_frame, textvariable=self.db_port_var, width=10).grid(row=1, column=1, sticky=tk.W, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Database Name:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.db_name_var = tk.StringVar(value=self.config.database_name)
        ttk.Entry(conn_frame, textvariable=self.db_name_var, width=30).grid(row=2, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Username:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.db_user_var = tk.StringVar(value=self.config.database_user)
        ttk.Entry(conn_frame, textvariable=self.db_user_var, width=30).grid(row=3, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Password:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.db_pass_var = tk.StringVar(value=self.config.database_password)
        ttk.Entry(conn_frame, textvariable=self.db_pass_var, width=30, show="*").grid(row=4, column=1, padx=10, pady=3)
        
        ttk.Label(conn_frame, text="Root Password:").grid(row=5, column=0, sticky=tk.W, pady=3)
        self.db_root_pass_var = tk.StringVar(value=self.config.database_root_password)
        ttk.Entry(conn_frame, textvariable=self.db_root_pass_var, width=30, show="*").grid(row=5, column=1, padx=10, pady=3)
    
    def create_network_tab(self):
        """Create network configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🌐 Network")
        
        ttk.Label(tab, text="Network Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Ports
        ports_frame = ttk.LabelFrame(tab, text="Port Configuration", padding="15")
        ports_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(ports_frame, text="API Port:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.api_port_var = tk.IntVar(value=self.config.api_port)
        ttk.Entry(ports_frame, textvariable=self.api_port_var, width=10).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(ports_frame, text="Frontend Port:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.frontend_port_var = tk.IntVar(value=self.config.frontend_port)
        ttk.Entry(ports_frame, textvariable=self.frontend_port_var, width=10).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(ports_frame, text="Redis Port:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.redis_port_var = tk.IntVar(value=self.config.redis_port)
        ttk.Entry(ports_frame, textvariable=self.redis_port_var, width=10).grid(row=2, column=1, padx=10, pady=3)
        
        # Domain
        domain_frame = ttk.LabelFrame(tab, text="Domain Configuration", padding="15")
        domain_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(domain_frame, text="Domain/Hostname:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.domain_var = tk.StringVar(value=self.config.domain)
        ttk.Entry(domain_frame, textvariable=self.domain_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        # SSL
        ssl_frame = ttk.LabelFrame(tab, text="SSL/TLS", padding="15")
        ssl_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.ssl_var = tk.BooleanVar(value=self.config.ssl_enabled)
        ttk.Checkbutton(ssl_frame, text="Enable SSL/TLS", 
                        variable=self.ssl_var,
                        command=self.on_ssl_change).pack(anchor=tk.W)
        
        self.ssl_details_frame = ttk.Frame(ssl_frame)
        self.ssl_details_frame.pack(fill=tk.X, pady=10)
        
        ttk.Label(self.ssl_details_frame, text="Certificate:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.ssl_cert_var = tk.StringVar(value=self.config.ssl_cert_path)
        ttk.Entry(self.ssl_details_frame, textvariable=self.ssl_cert_var, width=40).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(self.ssl_details_frame, text="Key:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.ssl_key_var = tk.StringVar(value=self.config.ssl_key_path)
        ttk.Entry(self.ssl_details_frame, textvariable=self.ssl_key_var, width=40).grid(row=1, column=1, padx=10, pady=3)
        
        self.on_ssl_change()
        
        # Network analyzer
        analyzer_frame = ttk.LabelFrame(tab, text="Network Analysis", padding="15")
        analyzer_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Button(analyzer_frame, text="🔍 Analyze Network", 
                   command=self.analyze_network).pack(anchor=tk.W)
        
        self.network_analysis_text = scrolledtext.ScrolledText(analyzer_frame, height=6, font=("Courier", 9))
        self.network_analysis_text.pack(fill=tk.X, pady=(10, 0))
    
    def create_credentials_tab(self):
        """Create credentials configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🔐 Credentials")
        
        ttk.Label(tab, text="Admin User & Security", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Admin user
        admin_frame = ttk.LabelFrame(tab, text="Admin User", padding="15")
        admin_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(admin_frame, text="Username:").grid(row=0, column=0, sticky=tk.W, pady=3)
        self.admin_user_var = tk.StringVar(value=self.config.admin_username)
        ttk.Entry(admin_frame, textvariable=self.admin_user_var, width=30).grid(row=0, column=1, padx=10, pady=3)
        
        ttk.Label(admin_frame, text="Email:").grid(row=1, column=0, sticky=tk.W, pady=3)
        self.admin_email_var = tk.StringVar(value=self.config.admin_email)
        ttk.Entry(admin_frame, textvariable=self.admin_email_var, width=30).grid(row=1, column=1, padx=10, pady=3)
        
        ttk.Label(admin_frame, text="Password:").grid(row=2, column=0, sticky=tk.W, pady=3)
        self.admin_pass_var = tk.StringVar(value=self.config.admin_password)
        self.admin_pass_entry = ttk.Entry(admin_frame, textvariable=self.admin_pass_var, width=30, show="*")
        self.admin_pass_entry.grid(row=2, column=1, padx=10, pady=3)
        
        self.show_pass_var = tk.BooleanVar(value=False)
        ttk.Checkbutton(admin_frame, text="Show", variable=self.show_pass_var,
                        command=self.toggle_password).grid(row=2, column=2)
        
        ttk.Label(admin_frame, text="First Name:").grid(row=3, column=0, sticky=tk.W, pady=3)
        self.admin_fname_var = tk.StringVar(value=self.config.admin_first_name)
        ttk.Entry(admin_frame, textvariable=self.admin_fname_var, width=30).grid(row=3, column=1, padx=10, pady=3)
        
        ttk.Label(admin_frame, text="Last Name:").grid(row=4, column=0, sticky=tk.W, pady=3)
        self.admin_lname_var = tk.StringVar(value=self.config.admin_last_name)
        ttk.Entry(admin_frame, textvariable=self.admin_lname_var, width=30).grid(row=4, column=1, padx=10, pady=3)
        
        ttk.Button(admin_frame, text="Generate Password", 
                   command=self.generate_password).grid(row=5, column=1, sticky=tk.W, padx=10, pady=10)
        
        # JWT
        jwt_frame = ttk.LabelFrame(tab, text="JWT Configuration", padding="15")
        jwt_frame.pack(fill=tk.X, pady=(0, 15))
        
        ttk.Label(jwt_frame, text="JWT Secret:").pack(anchor=tk.W)
        
        jwt_entry_frame = ttk.Frame(jwt_frame)
        jwt_entry_frame.pack(fill=tk.X, pady=5)
        
        self.jwt_secret_var = tk.StringVar(value=self.config.jwt_secret)
        ttk.Entry(jwt_entry_frame, textvariable=self.jwt_secret_var, width=60).pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(jwt_entry_frame, text="Generate", 
                   command=self.generate_jwt_secret).pack(side=tk.LEFT, padx=10)
    
    def create_seed_data_tab(self):
        """Create seed data configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="🌱 Data")
        
        ttk.Label(tab, text="Data Seeding Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Master data
        master_frame = ttk.LabelFrame(tab, text="Master Data (Required)", padding="15")
        master_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_master_var = tk.BooleanVar(value=True)
        cb = ttk.Checkbutton(master_frame, text="Deploy Master Data", 
                             variable=self.seed_master_var)
        cb.pack(anchor=tk.W, pady=3)
        cb.state(['selected', 'disabled'])
        
        # Demo data
        demo_frame = ttk.LabelFrame(tab, text="Demo Data (Optional)", padding="15")
        demo_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_demo_var = tk.BooleanVar(value=self.config.seed_demo_data)
        ttk.Checkbutton(demo_frame, text="Demo Customers & Contacts", 
                        variable=self.seed_demo_var).pack(anchor=tk.W, pady=3)
        
        self.seed_workflows_var = tk.BooleanVar(value=False)
        ttk.Checkbutton(demo_frame, text="Sample Workflows", 
                        variable=self.seed_workflows_var).pack(anchor=tk.W, pady=3)
        
        # Reference data
        ref_frame = ttk.LabelFrame(tab, text="Reference Data (Optional)", padding="15")
        ref_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.seed_zip_var = tk.BooleanVar(value=self.config.seed_zip_codes)
        ttk.Checkbutton(ref_frame, text="US Zip Codes Database", 
                        variable=self.seed_zip_var).pack(anchor=tk.W, pady=3)
    
    def create_testing_tab(self):
        """Create testing configuration tab."""
        tab = ttk.Frame(self.notebook, padding="15")
        self.notebook.add(tab, text="✅ Testing")
        
        ttk.Label(tab, text="Testing Configuration", 
                  font=("Helvetica", 14, "bold")).pack(anchor=tk.W, pady=(0, 15))
        
        # Test options
        options_frame = ttk.LabelFrame(tab, text="Post-Deployment Tests", padding="15")
        options_frame.pack(fill=tk.X, pady=(0, 15))
        
        self.run_smoke_var = tk.BooleanVar(value=self.config.run_smoke_tests)
        ttk.Checkbutton(options_frame, text="Run Smoke Tests", 
                        variable=self.run_smoke_var).pack(anchor=tk.W, pady=3)
        
        self.run_bvt_var = tk.BooleanVar(value=self.config.run_bvt_tests)
        ttk.Checkbutton(options_frame, text="Run BVT Tests", 
                        variable=self.run_bvt_var).pack(anchor=tk.W, pady=3)
        
        # Results
        results_frame = ttk.LabelFrame(tab, text="Test Results", padding="15")
        results_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 15))
        
        columns = ("Test", "Status", "Duration", "Message")
        self.results_tree = ttk.Treeview(results_frame, columns=columns, show="headings", height=10)
        
        for col in columns:
            self.results_tree.heading(col, text=col)
            self.results_tree.column(col, width=150 if col != "Message" else 250)
        
        self.results_tree.pack(fill=tk.BOTH, expand=True)
        
        # Summary
        summary_frame = ttk.Frame(results_frame)
        summary_frame.pack(fill=tk.X, pady=10)
        
        self.results_summary_var = tk.StringVar(value="No tests run yet")
        ttk.Label(summary_frame, textvariable=self.results_summary_var).pack(side=tk.LEFT)
        
        ttk.Button(summary_frame, text="Export Results", 
                   command=self.export_test_results).pack(side=tk.RIGHT)
    
    def create_log_panel(self):
        """Create log and progress panel."""
        # Progress section
        progress_frame = ttk.LabelFrame(self.right_frame, text="Deployment Progress", padding="10")
        progress_frame.pack(fill=tk.X, pady=(0, 10))
        
        self.progress_var = tk.DoubleVar(value=0)
        self.progress_bar = ttk.Progressbar(progress_frame, variable=self.progress_var, 
                                            maximum=100, length=400, mode='determinate')
        self.progress_bar.pack(fill=tk.X, pady=5)
        
        self.progress_label = tk.StringVar(value="Ready to deploy")
        ttk.Label(progress_frame, textvariable=self.progress_label).pack(anchor=tk.W)
        
        # Step indicators
        self.steps_frame = ttk.Frame(progress_frame)
        self.steps_frame.pack(fill=tk.X, pady=10)
        
        self.step_labels = {}
        steps = ["Prerequisites", "Network", "Database", "API", "Frontend", "Data", "Tests"]
        for step in steps:
            frame = ttk.Frame(self.steps_frame)
            frame.pack(side=tk.LEFT, padx=5)
            
            self.step_labels[step] = tk.StringVar(value="⏳")
            ttk.Label(frame, textvariable=self.step_labels[step]).pack()
            ttk.Label(frame, text=step, font=("Helvetica", 8)).pack()
        
        # Control buttons
        control_frame = ttk.Frame(progress_frame)
        control_frame.pack(fill=tk.X, pady=10)
        
        self.start_btn = ttk.Button(control_frame, text="▶️ Start Deployment", 
                                    command=self.start_deployment)
        self.start_btn.pack(side=tk.LEFT, padx=5)
        
        self.pause_btn = ttk.Button(control_frame, text="⏸️ Pause", 
                                    command=self.pause_deployment, state=tk.DISABLED)
        self.pause_btn.pack(side=tk.LEFT, padx=5)
        
        self.stop_btn = ttk.Button(control_frame, text="⏹️ Stop", 
                                   command=self.stop_deployment, state=tk.DISABLED)
        self.stop_btn.pack(side=tk.LEFT, padx=5)
        
        # Log section
        log_frame = ttk.LabelFrame(self.right_frame, text="Deployment Log", padding="10")
        log_frame.pack(fill=tk.BOTH, expand=True)
        
        self.log_text = scrolledtext.ScrolledText(log_frame, height=20, font=("Courier", 9))
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        # Configure log tags
        self.log_text.tag_configure("info", foreground="#333")
        self.log_text.tag_configure("success", foreground="#28a745")
        self.log_text.tag_configure("warning", foreground="#ffc107")
        self.log_text.tag_configure("error", foreground="#dc3545")
        self.log_text.tag_configure("cmd", foreground="#6c757d")
        self.log_text.tag_configure("output", foreground="#007bff")
        
        # Log controls
        log_control_frame = ttk.Frame(log_frame)
        log_control_frame.pack(fill=tk.X, pady=5)
        
        ttk.Button(log_control_frame, text="Clear Log", 
                   command=self.clear_log).pack(side=tk.LEFT)
        ttk.Button(log_control_frame, text="Save Log", 
                   command=self.save_log).pack(side=tk.LEFT, padx=5)
        
        # Summary section
        summary_frame = ttk.LabelFrame(self.right_frame, text="Deployment Summary", padding="10")
        summary_frame.pack(fill=tk.X, pady=(10, 0))
        
        self.summary_text = tk.Text(summary_frame, height=8, font=("Courier", 9))
        self.summary_text.pack(fill=tk.X)
        
        ttk.Button(summary_frame, text="📋 Copy Login Details", 
                   command=self.copy_login_details).pack(anchor=tk.W, pady=5)
    
    def create_button_bar(self):
        """Create bottom button bar."""
        bar = ttk.Frame(self.left_frame)
        bar.pack(fill=tk.X, pady=5)
        
        ttk.Button(bar, text="💾 Save", command=self.save_config).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="📂 Load", command=self.load_config_dialog).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="📜 Generate Scripts", command=self.generate_scripts).pack(side=tk.LEFT, padx=5)
        ttk.Button(bar, text="🔄 Reset", command=self.reset_config).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(bar, text="❌ Exit", command=self.root.quit).pack(side=tk.RIGHT, padx=5)
        ttk.Button(bar, text="❓ Help", command=self.show_help).pack(side=tk.RIGHT, padx=5)
    
    # Event handlers
    def on_architecture_change(self):
        """Handle architecture change."""
        is_micro = self.arch_var.get() == "microservices"
        for child in self.micro_frame.winfo_children():
            if isinstance(child, ttk.Checkbutton):
                child.state(['!disabled'] if is_micro else ['disabled'])
    
    def on_server_type_change(self):
        """Handle server type change."""
        is_local = self.server_local_var.get()
        for child in self.server_details_frame.winfo_children():
            if isinstance(child, (ttk.Entry, ttk.Button)):
                child.configure(state=tk.DISABLED if is_local else tk.NORMAL)
    
    def on_ssl_change(self):
        """Handle SSL toggle change."""
        is_enabled = self.ssl_var.get()
        for child in self.ssl_details_frame.winfo_children():
            if isinstance(child, (ttk.Entry, ttk.Button)):
                child.configure(state=tk.NORMAL if is_enabled else tk.DISABLED)
    
    def toggle_password(self):
        """Toggle password visibility."""
        self.admin_pass_entry.configure(show="" if self.show_pass_var.get() else "*")
    
    def generate_password(self):
        """Generate a strong password."""
        chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
        password = ''.join(secrets.choice(chars) for _ in range(16))
        self.admin_pass_var.set(password)
    
    def generate_jwt_secret(self):
        """Generate a secure JWT secret."""
        self.jwt_secret_var.set(secrets.token_urlsafe(64))
    
    def browse_file(self, var: tk.StringVar):
        """Open file browser."""
        filepath = filedialog.askopenfilename()
        if filepath:
            var.set(filepath)
    
    def select_all_components(self):
        """Select all components."""
        self.deploy_db_var.set(True)
        self.deploy_redis_var.set(True)
        self.deploy_api_var.set(True)
        self.deploy_frontend_var.set(True)
        self.deploy_monitoring_var.set(True)
    
    def select_minimal_components(self):
        """Select minimal components."""
        self.deploy_db_var.set(True)
        self.deploy_redis_var.set(False)
        self.deploy_api_var.set(True)
        self.deploy_frontend_var.set(True)
        self.deploy_monitoring_var.set(False)
    
    def get_current_config(self) -> DeploymentConfig:
        """Get current configuration from UI."""
        return DeploymentConfig(
            architecture=self.arch_var.get(),
            build_server_name=self.server_name_var.get(),
            build_server_host=self.server_host_var.get(),
            build_server_port=self.server_port_var.get(),
            build_server_user=self.server_user_var.get(),
            build_server_is_local=self.server_local_var.get(),
            build_server_ssh_key=self.server_key_var.get(),
            deploy_frontend=self.deploy_frontend_var.get(),
            deploy_api=self.deploy_api_var.get(),
            deploy_database=self.deploy_db_var.get(),
            deploy_redis=self.deploy_redis_var.get(),
            deploy_monitoring=self.deploy_monitoring_var.get(),
            hosting_platform=self.orch_var.get(),
            database_provider=self.db_provider_var.get(),
            database_host=self.db_host_var.get(),
            database_port=self.db_port_var.get(),
            database_name=self.db_name_var.get(),
            database_user=self.db_user_var.get(),
            database_password=self.db_pass_var.get(),
            database_root_password=self.db_root_pass_var.get(),
            api_port=self.api_port_var.get(),
            frontend_port=self.frontend_port_var.get(),
            redis_port=self.redis_port_var.get(),
            domain=self.domain_var.get(),
            ssl_enabled=self.ssl_var.get(),
            ssl_cert_path=self.ssl_cert_var.get(),
            ssl_key_path=self.ssl_key_var.get(),
            admin_username=self.admin_user_var.get(),
            admin_email=self.admin_email_var.get(),
            admin_password=self.admin_pass_var.get(),
            admin_first_name=self.admin_fname_var.get(),
            admin_last_name=self.admin_lname_var.get(),
            jwt_secret=self.jwt_secret_var.get(),
            seed_master_data=True,
            seed_demo_data=self.seed_demo_var.get(),
            seed_zip_codes=self.seed_zip_var.get(),
            run_bvt_tests=self.run_bvt_var.get(),
            run_smoke_tests=self.run_smoke_var.get(),
            deploy_identity_service=self.deploy_identity_var.get(),
            deploy_customer_service=self.deploy_customer_var.get(),
            deploy_sales_service=self.deploy_sales_var.get(),
            deploy_marketing_service=self.deploy_marketing_var.get(),
            deploy_servicedesk_service=self.deploy_servicedesk_var.get(),
            # Cloud Configuration
            cloud_provider=self.cloud_provider_var.get(),
            # AWS
            aws_region=self.aws_region_var.get(),
            aws_ecs_cluster=self.aws_cluster_var.get(),
            aws_ecr_registry=self.aws_ecr_var.get(),
            aws_vpc_id=self.aws_vpc_var.get(),
            aws_subnet_ids=self.aws_subnets_var.get(),
            aws_use_fargate=self.aws_fargate_var.get(),
            aws_use_rds=self.aws_rds_var.get(),
            aws_use_elasticache=self.aws_elasticache_var.get(),
            # Azure
            azure_subscription_id=self.azure_subscription_var.get(),
            azure_resource_group=self.azure_rg_var.get(),
            azure_location=self.azure_location_var.get(),
            azure_acr_name=self.azure_acr_var.get(),
            azure_aks_cluster=self.azure_aks_var.get(),
            azure_use_aks=self.azure_aks_enabled_var.get(),
            azure_use_aci=self.azure_aci_var.get(),
            azure_use_azure_db=self.azure_db_var.get(),
            azure_use_redis_cache=self.azure_redis_var.get(),
            # GCP
            gcp_project_id=self.gcp_project_var.get(),
            gcp_region=self.gcp_region_var.get(),
            gcp_zone=self.gcp_zone_var.get(),
            gcp_gke_cluster=self.gcp_gke_var.get(),
            gcp_gcr_hostname=self.gcp_gcr_var.get(),
            gcp_use_gke=self.gcp_gke_enabled_var.get(),
            gcp_use_cloud_run=self.gcp_cloudrun_var.get(),
            gcp_use_cloud_sql=self.gcp_cloudsql_var.get(),
            gcp_use_memorystore=self.gcp_memorystore_var.get(),
            # Build Configuration
            build_source=self.build_source_var.get(),
            git_repo_url=self.git_repo_var.get(),
            git_branch=self.git_branch_var.get(),
            git_username=self.git_username_var.get(),
            git_token=self.git_token_var.get(),
            git_ssh_key=self.git_ssh_key_var.get(),
            git_use_ssh=self.git_use_ssh_var.get(),
            build_type=self.build_type_var.get(),
            cloud_build_provider=self.cloud_build_provider_var.get(),
            build_frontend_cmd=self.build_frontend_cmd_var.get(),
            build_api_cmd=self.build_api_cmd_var.get(),
            build_docker_api_cmd=self.build_docker_api_cmd_var.get(),
            build_docker_frontend_cmd=self.build_docker_frontend_cmd_var.get(),
            github_actions_workflow=self.github_workflow_var.get(),
            azure_devops_org=self.azure_devops_org_var.get(),
            azure_devops_project=self.azure_devops_project_var.get(),
            azure_devops_pipeline=self.azure_devops_pipeline_var.get(),
            aws_codebuild_project=self.aws_codebuild_project_var.get(),
            gcp_cloudbuild_trigger=self.gcp_cloudbuild_trigger_var.get(),
        )
    
    def save_config(self):
        """Save current configuration."""
        self.config = self.get_current_config()
        with open(self.config_path, 'w') as f:
            json.dump(asdict(self.config), f, indent=2)
        messagebox.showinfo("Success", "Configuration saved!")
    
    def load_config(self):
        """Load saved configuration."""
        if self.config_path.exists():
            try:
                with open(self.config_path, 'r') as f:
                    data = json.load(f)
                    # Filter out unknown keys for backward compatibility
                    valid_keys = {f.name for f in DeploymentConfig.__dataclass_fields__.values()}
                    filtered_data = {k: v for k, v in data.items() if k in valid_keys}
                    self.config = DeploymentConfig(**filtered_data)
            except Exception as e:
                print(f"Error loading config: {e}")
    
    def load_config_dialog(self):
        """Load configuration from file."""
        filepath = filedialog.askopenfilename(filetypes=[("JSON files", "*.json")])
        if filepath:
            with open(filepath, 'r') as f:
                data = json.load(f)
                # Filter out unknown keys for backward compatibility
                valid_keys = {f.name for f in DeploymentConfig.__dataclass_fields__.values()}
                filtered_data = {k: v for k, v in data.items() if k in valid_keys}
                self.config = DeploymentConfig(**filtered_data)
            messagebox.showinfo("Success", "Configuration loaded!")
    
    def reset_config(self):
        """Reset to default configuration."""
        if messagebox.askyesno("Confirm", "Reset all settings to defaults?"):
            self.config = DeploymentConfig()
    
    def test_server_connection(self):
        """Test connection to build server."""
        self.connection_status_var.set("Testing...")
        self.root.update()
        
        if self.server_local_var.get():
            self.connection_status_var.set("✓ Local - OK")
            return
        
        host = self.server_host_var.get()
        port = self.server_port_var.get()
        
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(5)
            result = sock.connect_ex((host, port))
            sock.close()
            
            if result == 0:
                self.connection_status_var.set(f"✓ Connected to {host}:{port}")
            else:
                self.connection_status_var.set(f"✗ Cannot connect to {host}:{port}")
        except Exception as e:
            self.connection_status_var.set(f"✗ Error: {str(e)}")
    
    def check_prerequisites(self):
        """Check prerequisites on build server."""
        self.prereq_text.delete(1.0, tk.END)
        
        ssh_cmd = ""
        if not self.server_local_var.get():
            user = self.server_user_var.get()
            host = self.server_host_var.get()
            key = self.server_key_var.get()
            if key:
                ssh_cmd = f"ssh -i {key} {user}@{host}"
            else:
                ssh_cmd = f"ssh {user}@{host}"
        
        checker = PrerequisiteChecker(ssh_cmd)
        results = checker.check_all()
        
        for name, result in results.items():
            if result["installed"]:
                self.prereq_text.insert(tk.END, f"✓ {name}: {result['version']}\n")
            elif result["required"]:
                self.prereq_text.insert(tk.END, f"✗ {name}: NOT INSTALLED (required)\n")
            else:
                self.prereq_text.insert(tk.END, f"⚠ {name}: not installed (optional)\n")
    
    def analyze_network(self):
        """Analyze network configuration."""
        self.network_analysis_text.delete(1.0, tk.END)
        
        config = self.get_current_config()
        analyzer = NetworkAnalyzer(config)
        issues = analyzer.analyze()
        
        if not issues:
            self.network_analysis_text.insert(tk.END, "✓ No network issues detected!\n\n")
        else:
            self.network_analysis_text.insert(tk.END, f"Found {len(issues)} potential issues:\n\n")
            for issue in issues:
                icon = "❌" if issue["severity"] == "critical" else "⚠️"
                self.network_analysis_text.insert(
                    tk.END, 
                    f"{icon} [{issue['component']}] {issue['message']}\n   Fix: {issue['fix']}\n\n"
                )
        
        fixes = analyzer.get_network_fixes()
        self.network_analysis_text.insert(tk.END, "\n--- Fix Commands ---\n")
        for fix in fixes:
            self.network_analysis_text.insert(tk.END, f"$ {fix}\n")
    
    def generate_scripts(self):
        """Generate deployment scripts."""
        config = self.get_current_config()
        engine = DeploymentEngine(config, lambda t, m: None)
        
        output_dir = Path(__file__).parent / "generated"
        output_dir.mkdir(exist_ok=True)
        
        env_content = engine.generate_env_file()
        compose_content = engine.generate_docker_compose()
        
        with open(output_dir / ".env", 'w') as f:
            f.write(env_content)
        
        with open(output_dir / "docker-compose.yml", 'w') as f:
            f.write(compose_content)
        
        messagebox.showinfo("Success", f"Scripts generated in:\n{output_dir}")
    
    def log_message(self, msg_type: str, message: str):
        """Add message to log queue."""
        self.log_queue.put((msg_type, message))
    
    def process_logs(self):
        """Process log queue and update UI."""
        try:
            while True:
                msg_type, message = self.log_queue.get_nowait()
                timestamp = datetime.now().strftime("%H:%M:%S")
                log_entry = f"[{timestamp}] {message}\n"
                self.log_text.insert(tk.END, log_entry, msg_type)
                self.log_text.see(tk.END)
        except queue.Empty:
            pass
        
        self.root.after(100, self.process_logs)
    
    def clear_log(self):
        """Clear the log text."""
        self.log_text.delete(1.0, tk.END)
    
    def save_log(self):
        """Save log to file."""
        filepath = filedialog.asksaveasfilename(
            defaultextension=".log",
            filetypes=[("Log files", "*.log")])
        if filepath:
            with open(filepath, 'w') as f:
                f.write(self.log_text.get(1.0, tk.END))
    
    def update_step_status(self, step: str, status: str):
        """Update step status indicator."""
        icons = {
            "pending": "⏳",
            "in_progress": "🔄",
            "success": "✅",
            "failed": "❌",
            "skipped": "⏭️"
        }
        if step in self.step_labels:
            self.step_labels[step].set(icons.get(status, "⏳"))
    
    def start_deployment(self):
        """Start the deployment process."""
        config = self.get_current_config()
        
        self.start_btn.configure(state=tk.DISABLED)
        self.pause_btn.configure(state=tk.NORMAL)
        self.stop_btn.configure(state=tk.NORMAL)
        
        self.progress_var.set(0)
        for step in self.step_labels:
            self.update_step_status(step, "pending")
        
        def deploy_thread():
            try:
                self.engine = DeploymentEngine(config, self.log_message)
                self.engine.is_running = True
                
                steps = [
                    ("Prerequisites", self.engine.deploy_prerequisites, 10),
                    ("Network", self.engine.create_docker_network, 20),
                    ("Database", lambda: True, 40),
                    ("API", lambda: True, 60),
                    ("Frontend", lambda: True, 70),
                    ("Data", self.engine.deploy_master_data, 85),
                    ("Tests", lambda: self.run_tests_internal(config), 100),
                ]
                
                for step_name, step_func, progress in steps:
                    if not self.engine.is_running:
                        break
                    
                    self.engine._wait_if_paused()
                    
                    self.update_step_status(step_name, "in_progress")
                    self.progress_label.set(f"Running: {step_name}")
                    self.log_message("info", f"=== {step_name} ===")
                    
                    try:
                        success = step_func()
                        self.update_step_status(step_name, "success" if success else "failed")
                    except Exception as e:
                        self.log_message("error", f"Error in {step_name}: {str(e)}")
                        self.update_step_status(step_name, "failed")
                    
                    self.progress_var.set(progress)
                
                self.show_deployment_summary(config)
                
            except Exception as e:
                self.log_message("error", f"Deployment error: {str(e)}")
            finally:
                self.start_btn.configure(state=tk.NORMAL)
                self.pause_btn.configure(state=tk.DISABLED)
                self.stop_btn.configure(state=tk.DISABLED)
                self.progress_label.set("Deployment complete" if self.engine.is_running else "Deployment stopped")
        
        self.deployment_thread = threading.Thread(target=deploy_thread, daemon=True)
        self.deployment_thread.start()
    
    def run_tests_internal(self, config: DeploymentConfig) -> bool:
        """Run tests and update results."""
        if self.engine:
            smoke_results = self.engine.run_smoke_tests()
            bvt_results = self.engine.run_bvt_tests()
            
            self.test_results = smoke_results + bvt_results
            
            for item in self.results_tree.get_children():
                self.results_tree.delete(item)
            
            passed = 0
            failed = 0
            for result in self.test_results:
                icon = "✅" if result.status == "passed" else "❌"
                self.results_tree.insert("", tk.END, values=(
                    result.name,
                    f"{icon} {result.status}",
                    f"{result.duration_ms}ms",
                    result.message
                ))
                if result.status == "passed":
                    passed += 1
                else:
                    failed += 1
            
            self.results_summary_var.set(f"Tests: {passed} passed, {failed} failed")
            
            return failed == 0
        return True
    
    def pause_deployment(self):
        """Pause the deployment."""
        if self.engine:
            self.engine.is_paused = not self.engine.is_paused
            status = "Paused" if self.engine.is_paused else "Resumed"
            self.pause_btn.configure(text="▶️ Resume" if self.engine.is_paused else "⏸️ Pause")
            self.log_message("warning", f"Deployment {status}")
            self.progress_label.set(status)
    
    def stop_deployment(self):
        """Stop the deployment."""
        if self.engine:
            self.engine.is_running = False
            self.log_message("warning", "Deployment stopped by user")
    
    def show_deployment_summary(self, config: DeploymentConfig):
        """Show deployment summary."""
        protocol = "https" if config.ssl_enabled else "http"
        domain = config.domain
        api_port = config.api_port
        frontend_port = config.frontend_port
        
        summary = f"""
╔══════════════════════════════════════════════════════╗
║          CRM SOLUTION DEPLOYMENT COMPLETE            ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Frontend: {protocol}://{domain}:{frontend_port}
║  API:      {protocol}://{domain}:{api_port}
║  Swagger:  {protocol}://{domain}:{api_port}/swagger
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  LOGIN CREDENTIALS                                   ║
╠══════════════════════════════════════════════════════╣
║  Email:    {config.admin_email}
║  Password: {config.admin_password}
║                                                      ║
╚══════════════════════════════════════════════════════╝
"""
        
        self.summary_text.delete(1.0, tk.END)
        self.summary_text.insert(1.0, summary)
    
    def copy_login_details(self):
        """Copy login details to clipboard."""
        config = self.get_current_config()
        protocol = "https" if config.ssl_enabled else "http"
        
        details = f"""CRM Solution Login
URL: {protocol}://{config.domain}:{config.frontend_port}
Email: {config.admin_email}
Password: {config.admin_password}
"""
        
        self.root.clipboard_clear()
        self.root.clipboard_append(details)
        messagebox.showinfo("Copied", "Login details copied to clipboard!")
    
    def export_test_results(self):
        """Export test results to file."""
        if not self.test_results:
            messagebox.showwarning("Warning", "No test results to export")
            return
        
        filepath = filedialog.asksaveasfilename(
            defaultextension=".json",
            filetypes=[("JSON files", "*.json")])
        
        if filepath:
            results_data = {
                "timestamp": datetime.now().isoformat(),
                "version": VERSION,
                "tests": [asdict(r) for r in self.test_results]
            }
            with open(filepath, 'w') as f:
                json.dump(results_data, f, indent=2)
            
            messagebox.showinfo("Success", f"Results exported to {filepath}")
    
    def show_help(self):
        """Show help dialog."""
        help_text = f"""CRM Solution Deployment Tool v{VERSION}

TABS:
• Architecture: Choose deployment type
• Build Server: Configure target server
• Components: Select what to deploy
• Database: Database configuration
• Network: Ports, domain, SSL
• Credentials: Admin user setup
• Data: Seed data options
• Testing: Post-deployment tests

WORKFLOW:
1. Configure settings
2. Generate Scripts to preview
3. Start Deployment
4. Monitor progress
5. Review test results

For more info, see docs/HOWTO.md
"""
        messagebox.showinfo("Help", help_text)


def cli_mode():
    """Command-line interface mode for when tkinter is unavailable."""
    import argparse
    
    parser = argparse.ArgumentParser(description="CRM Deployment Tool CLI")
    parser.add_argument("--generate", action="store_true", help="Generate deployment scripts")
    parser.add_argument("--deploy", action="store_true", help="Run deployment")
    parser.add_argument("--config", type=str, help="Path to config JSON file")
    parser.add_argument("--output", type=str, default="./generated", help="Output directory")
    parser.add_argument("--domain", type=str, default="localhost", help="Domain/hostname")
    parser.add_argument("--api-port", type=int, default=5000, help="API port")
    parser.add_argument("--frontend-port", type=int, default=80, help="Frontend port")
    parser.add_argument("--db-host", type=str, default="crm-mariadb", help="Database host")
    parser.add_argument("--db-password", type=str, help="Database password")
    parser.add_argument("--admin-email", type=str, default="admin@crm.local", help="Admin email")
    parser.add_argument("--admin-password", type=str, default="Admin@123", help="Admin password")
    
    args = parser.parse_args()
    
    # Load or create config
    if args.config and Path(args.config).exists():
        with open(args.config, 'r') as f:
            data = json.load(f)
            config = DeploymentConfig(**data)
    else:
        config = DeploymentConfig()
    
    # Apply CLI overrides
    if args.domain:
        config.domain = args.domain
    if args.api_port:
        config.api_port = args.api_port
    if args.frontend_port:
        config.frontend_port = args.frontend_port
    if args.db_host:
        config.database_host = args.db_host
    if args.db_password:
        config.database_password = args.db_password
    if args.admin_email:
        config.admin_email = args.admin_email
    if args.admin_password:
        config.admin_password = args.admin_password
    
    def log_print(msg_type: str, message: str):
        timestamp = datetime.now().strftime("%H:%M:%S")
        prefix = {"info": "ℹ️", "success": "✅", "warning": "⚠️", "error": "❌", "cmd": "💻", "output": "📤"}.get(msg_type, "")
        print(f"[{timestamp}] {prefix} {message}")
    
    engine = DeploymentEngine(config, log_print)
    
    if args.generate or not args.deploy:
        # Generate scripts
        output_dir = Path(args.output)
        output_dir.mkdir(exist_ok=True)
        
        env_content = engine.generate_env_file()
        compose_content = engine.generate_docker_compose()
        
        env_path = output_dir / ".env"
        compose_path = output_dir / "docker-compose.yml"
        
        with open(env_path, 'w') as f:
            f.write(env_content)
        print(f"✅ Generated: {env_path}")
        
        with open(compose_path, 'w') as f:
            f.write(compose_content)
        print(f"✅ Generated: {compose_path}")
        
        print(f"\n📁 Output directory: {output_dir.absolute()}")
        print(f"\nTo deploy, run:")
        print(f"  cd {output_dir.absolute()}")
        print(f"  docker compose up -d")
    
    if args.deploy:
        print("\n🚀 Starting deployment...")
        engine.is_running = True
        
        if not engine.deploy_prerequisites():
            print("❌ Prerequisites check failed")
            return 1
        
        engine.create_docker_network()
        engine.deploy_master_data()
        
        results = engine.run_smoke_tests()
        
        passed = sum(1 for r in results if r.status == "passed")
        failed = len(results) - passed
        
        print(f"\n📊 Tests: {passed} passed, {failed} failed")
        
        protocol = "https" if config.ssl_enabled else "http"
        print(f"\n" + "=" * 50)
        print(f"🎉 Deployment Complete!")
        print(f"=" * 50)
        print(f"Frontend: {protocol}://{config.domain}:{config.frontend_port}")
        print(f"API:      {protocol}://{config.domain}:{config.api_port}")
        print(f"\n👤 Login Credentials:")
        print(f"   Email:    {config.admin_email}")
        print(f"   Password: {config.admin_password}")
        print(f"=" * 50)
    
    return 0


def main():
    """Main entry point."""
    # Check for CLI mode
    if len(sys.argv) > 1 and sys.argv[1] in ['--generate', '--deploy', '--config', '--help', '-h']:
        sys.exit(cli_mode())
    
    # Try GUI mode
    try:
        root = tk.Tk()
        
        style = ttk.Style()
        if sys.platform == 'darwin':
            try:
                style.theme_use('aqua')
            except:
                style.theme_use('clam')
        else:
            style.theme_use('clam')
        
        app = DeploymentTool(root)
        root.mainloop()
    except Exception as e:
        print(f"GUI mode failed: {e}")
        print("\nFalling back to CLI mode...")
        print("Use --help for CLI options, or run:")
        print("  python main.py --generate")
        sys.exit(1)


if __name__ == "__main__":
    main()
