#!/usr/bin/env python3
"""
CRM Solution - Platform-Specific Configuration Models
Defines platform-specific options for Azure, AWS, GCP, and On-Premises.

Author: Abhishek Lal
License: AGPL-3.0
"""

from dataclasses import dataclass, field
from typing import Dict, List, Optional, Any
from enum import Enum


# ============================================================================
# AZURE PLATFORM OPTIONS
# ============================================================================

class AzureVMSize(Enum):
    """Azure VM sizes for different workloads."""
    # Burstable (Dev/Test)
    B1S = "Standard_B1s"      # 1 vCPU, 1 GB RAM
    B1MS = "Standard_B1ms"    # 1 vCPU, 2 GB RAM
    B2S = "Standard_B2s"      # 2 vCPU, 4 GB RAM
    B2MS = "Standard_B2ms"    # 2 vCPU, 8 GB RAM
    B4MS = "Standard_B4ms"    # 4 vCPU, 16 GB RAM
    
    # General Purpose (Production)
    D2S_V5 = "Standard_D2s_v5"    # 2 vCPU, 8 GB RAM
    D4S_V5 = "Standard_D4s_v5"    # 4 vCPU, 16 GB RAM
    D8S_V5 = "Standard_D8s_v5"    # 8 vCPU, 32 GB RAM
    D16S_V5 = "Standard_D16s_v5"  # 16 vCPU, 64 GB RAM
    D32S_V5 = "Standard_D32s_v5"  # 32 vCPU, 128 GB RAM
    
    # Memory Optimized (Database)
    E2S_V5 = "Standard_E2s_v5"    # 2 vCPU, 16 GB RAM
    E4S_V5 = "Standard_E4s_v5"    # 4 vCPU, 32 GB RAM
    E8S_V5 = "Standard_E8s_v5"    # 8 vCPU, 64 GB RAM
    E16S_V5 = "Standard_E16s_v5"  # 16 vCPU, 128 GB RAM
    
    # Compute Optimized (API Heavy)
    F2S_V2 = "Standard_F2s_v2"    # 2 vCPU, 4 GB RAM
    F4S_V2 = "Standard_F4s_v2"    # 4 vCPU, 8 GB RAM
    F8S_V2 = "Standard_F8s_v2"    # 8 vCPU, 16 GB RAM


class AzureContainerSize(Enum):
    """Azure Container Apps/ACI sizes."""
    SMALL = {"cpu": "0.25", "memory": "0.5Gi"}
    MEDIUM = {"cpu": "0.5", "memory": "1Gi"}
    LARGE = {"cpu": "1", "memory": "2Gi"}
    XLARGE = {"cpu": "2", "memory": "4Gi"}
    XXLARGE = {"cpu": "4", "memory": "8Gi"}


class AzureDatabaseSKU(Enum):
    """Azure Database for MySQL/MariaDB SKUs."""
    # Burstable
    B1S = "Standard_B1s"       # 1 vCore, 1 GB
    B1MS = "Standard_B1ms"     # 1 vCore, 2 GB
    B2S = "Standard_B2s"       # 2 vCores, 4 GB
    B2MS = "Standard_B2ms"     # 2 vCores, 8 GB
    
    # General Purpose
    D2DS_V4 = "Standard_D2ds_v4"   # 2 vCores, 8 GB
    D4DS_V4 = "Standard_D4ds_v4"   # 4 vCores, 16 GB
    D8DS_V4 = "Standard_D8ds_v4"   # 8 vCores, 32 GB
    D16DS_V4 = "Standard_D16ds_v4" # 16 vCores, 64 GB
    
    # Memory Optimized
    E2DS_V4 = "Standard_E2ds_v4"   # 2 vCores, 16 GB
    E4DS_V4 = "Standard_E4ds_v4"   # 4 vCores, 32 GB
    E8DS_V4 = "Standard_E8ds_v4"   # 8 vCores, 64 GB


class AzureRedisSKU(Enum):
    """Azure Cache for Redis SKUs."""
    BASIC_C0 = "Basic_C0"     # 250 MB
    BASIC_C1 = "Basic_C1"     # 1 GB
    STANDARD_C0 = "Standard_C0"  # 250 MB, HA
    STANDARD_C1 = "Standard_C1"  # 1 GB, HA
    STANDARD_C2 = "Standard_C2"  # 2.5 GB, HA
    PREMIUM_P1 = "Premium_P1"    # 6 GB, HA, Clustering
    PREMIUM_P2 = "Premium_P2"    # 13 GB, HA, Clustering


class AzureRegion(Enum):
    """Azure regions."""
    EAST_US = "eastus"
    EAST_US_2 = "eastus2"
    WEST_US = "westus"
    WEST_US_2 = "westus2"
    WEST_US_3 = "westus3"
    CENTRAL_US = "centralus"
    NORTH_CENTRAL_US = "northcentralus"
    SOUTH_CENTRAL_US = "southcentralus"
    WEST_EUROPE = "westeurope"
    NORTH_EUROPE = "northeurope"
    UK_SOUTH = "uksouth"
    UK_WEST = "ukwest"
    GERMANY_WEST_CENTRAL = "germanywestcentral"
    FRANCE_CENTRAL = "francecentral"
    AUSTRALIA_EAST = "australiaeast"
    SOUTHEAST_ASIA = "southeastasia"
    JAPAN_EAST = "japaneast"
    KOREA_CENTRAL = "koreacentral"
    CANADA_CENTRAL = "canadacentral"
    BRAZIL_SOUTH = "brazilsouth"
    INDIA_CENTRAL = "centralindia"


@dataclass
class AzurePlatformConfig:
    """Azure-specific configuration."""
    subscription_id: str = ""
    tenant_id: str = ""
    resource_group: str = "crm-solution-rg"
    location: AzureRegion = AzureRegion.EAST_US
    
    # VM Sizes
    frontend_vm_size: AzureVMSize = AzureVMSize.B2S
    api_vm_size: AzureVMSize = AzureVMSize.D2S_V5
    database_vm_size: AzureVMSize = AzureVMSize.E4S_V5
    
    # Container Sizes
    frontend_container_size: AzureContainerSize = AzureContainerSize.MEDIUM
    api_container_size: AzureContainerSize = AzureContainerSize.LARGE
    
    # Database
    database_sku: AzureDatabaseSKU = AzureDatabaseSKU.D2DS_V4
    database_storage_gb: int = 100
    
    # Redis
    redis_sku: AzureRedisSKU = AzureRedisSKU.STANDARD_C1
    
    # Networking
    vnet_name: str = "crm-vnet"
    subnet_name: str = "crm-subnet"
    nsg_name: str = "crm-nsg"
    
    # App Service / Container Apps
    use_app_service: bool = False
    use_container_apps: bool = True
    container_registry: str = ""
    
    # AKS (Kubernetes)
    use_aks: bool = False
    aks_cluster_name: str = "crm-aks"
    aks_node_count: int = 3
    aks_node_size: AzureVMSize = AzureVMSize.D2S_V5
    
    # Storage
    storage_account_name: str = ""
    storage_sku: str = "Standard_LRS"
    
    # Key Vault
    key_vault_name: str = ""
    
    # Tags
    tags: Dict[str, str] = field(default_factory=lambda: {
        "project": "crm-solution",
        "environment": "development",
        "managed-by": "deployment-tool"
    })


# ============================================================================
# AWS PLATFORM OPTIONS
# ============================================================================

class AWSInstanceType(Enum):
    """AWS EC2 instance types."""
    # Burstable (Dev/Test)
    T3_MICRO = "t3.micro"     # 2 vCPU, 1 GB
    T3_SMALL = "t3.small"     # 2 vCPU, 2 GB
    T3_MEDIUM = "t3.medium"   # 2 vCPU, 4 GB
    T3_LARGE = "t3.large"     # 2 vCPU, 8 GB
    T3_XLARGE = "t3.xlarge"   # 4 vCPU, 16 GB
    
    # General Purpose (Production)
    M6I_LARGE = "m6i.large"       # 2 vCPU, 8 GB
    M6I_XLARGE = "m6i.xlarge"     # 4 vCPU, 16 GB
    M6I_2XLARGE = "m6i.2xlarge"   # 8 vCPU, 32 GB
    M6I_4XLARGE = "m6i.4xlarge"   # 16 vCPU, 64 GB
    
    # Memory Optimized (Database)
    R6I_LARGE = "r6i.large"       # 2 vCPU, 16 GB
    R6I_XLARGE = "r6i.xlarge"     # 4 vCPU, 32 GB
    R6I_2XLARGE = "r6i.2xlarge"   # 8 vCPU, 64 GB
    R6I_4XLARGE = "r6i.4xlarge"   # 16 vCPU, 128 GB
    
    # Compute Optimized
    C6I_LARGE = "c6i.large"       # 2 vCPU, 4 GB
    C6I_XLARGE = "c6i.xlarge"     # 4 vCPU, 8 GB
    C6I_2XLARGE = "c6i.2xlarge"   # 8 vCPU, 16 GB


class AWSRDSInstanceClass(Enum):
    """AWS RDS instance classes."""
    # Burstable
    DB_T3_MICRO = "db.t3.micro"     # 2 vCPU, 1 GB
    DB_T3_SMALL = "db.t3.small"     # 2 vCPU, 2 GB
    DB_T3_MEDIUM = "db.t3.medium"   # 2 vCPU, 4 GB
    DB_T3_LARGE = "db.t3.large"     # 2 vCPU, 8 GB
    
    # General Purpose
    DB_M6I_LARGE = "db.m6i.large"       # 2 vCPU, 8 GB
    DB_M6I_XLARGE = "db.m6i.xlarge"     # 4 vCPU, 16 GB
    DB_M6I_2XLARGE = "db.m6i.2xlarge"   # 8 vCPU, 32 GB
    
    # Memory Optimized
    DB_R6I_LARGE = "db.r6i.large"       # 2 vCPU, 16 GB
    DB_R6I_XLARGE = "db.r6i.xlarge"     # 4 vCPU, 32 GB
    DB_R6I_2XLARGE = "db.r6i.2xlarge"   # 8 vCPU, 64 GB


class AWSElastiCacheNodeType(Enum):
    """AWS ElastiCache node types."""
    CACHE_T3_MICRO = "cache.t3.micro"     # 0.5 GB
    CACHE_T3_SMALL = "cache.t3.small"     # 1.37 GB
    CACHE_T3_MEDIUM = "cache.t3.medium"   # 3.09 GB
    CACHE_M6G_LARGE = "cache.m6g.large"   # 6.38 GB
    CACHE_R6G_LARGE = "cache.r6g.large"   # 13.07 GB


class AWSRegion(Enum):
    """AWS regions."""
    US_EAST_1 = "us-east-1"         # N. Virginia
    US_EAST_2 = "us-east-2"         # Ohio
    US_WEST_1 = "us-west-1"         # N. California
    US_WEST_2 = "us-west-2"         # Oregon
    EU_WEST_1 = "eu-west-1"         # Ireland
    EU_WEST_2 = "eu-west-2"         # London
    EU_WEST_3 = "eu-west-3"         # Paris
    EU_CENTRAL_1 = "eu-central-1"   # Frankfurt
    EU_NORTH_1 = "eu-north-1"       # Stockholm
    AP_SOUTHEAST_1 = "ap-southeast-1"  # Singapore
    AP_SOUTHEAST_2 = "ap-southeast-2"  # Sydney
    AP_NORTHEAST_1 = "ap-northeast-1"  # Tokyo
    AP_NORTHEAST_2 = "ap-northeast-2"  # Seoul
    AP_SOUTH_1 = "ap-south-1"       # Mumbai
    SA_EAST_1 = "sa-east-1"         # São Paulo
    CA_CENTRAL_1 = "ca-central-1"   # Canada


@dataclass
class AWSPlatformConfig:
    """AWS-specific configuration."""
    region: AWSRegion = AWSRegion.US_EAST_1
    account_id: str = ""
    
    # EC2 Instance Types
    frontend_instance_type: AWSInstanceType = AWSInstanceType.T3_SMALL
    api_instance_type: AWSInstanceType = AWSInstanceType.M6I_LARGE
    database_instance_type: AWSInstanceType = AWSInstanceType.R6I_LARGE
    
    # ECS/Fargate
    use_ecs: bool = True
    use_fargate: bool = True
    ecs_cluster_name: str = "crm-cluster"
    frontend_cpu: int = 256    # 0.25 vCPU
    frontend_memory: int = 512  # MB
    api_cpu: int = 512         # 0.5 vCPU
    api_memory: int = 1024     # MB
    
    # EKS (Kubernetes)
    use_eks: bool = False
    eks_cluster_name: str = "crm-eks"
    eks_node_count: int = 3
    eks_node_type: AWSInstanceType = AWSInstanceType.M6I_LARGE
    
    # RDS
    rds_instance_class: AWSRDSInstanceClass = AWSRDSInstanceClass.DB_M6I_LARGE
    rds_engine: str = "mariadb"  # mariadb, mysql, postgres
    rds_engine_version: str = "10.11"
    rds_storage_gb: int = 100
    rds_multi_az: bool = True
    rds_storage_type: str = "gp3"
    
    # ElastiCache
    elasticache_node_type: AWSElastiCacheNodeType = AWSElastiCacheNodeType.CACHE_T3_MEDIUM
    elasticache_num_nodes: int = 1
    
    # VPC
    vpc_cidr: str = "10.0.0.0/16"
    public_subnet_cidrs: List[str] = field(default_factory=lambda: ["10.0.1.0/24", "10.0.2.0/24"])
    private_subnet_cidrs: List[str] = field(default_factory=lambda: ["10.0.3.0/24", "10.0.4.0/24"])
    
    # ECR
    ecr_repository_prefix: str = "crm-solution"
    
    # S3
    s3_bucket_prefix: str = "crm-solution"
    
    # ALB
    use_alb: bool = True
    alb_name: str = "crm-alb"
    
    # CloudFront
    use_cloudfront: bool = False
    
    # Tags
    tags: Dict[str, str] = field(default_factory=lambda: {
        "Project": "crm-solution",
        "Environment": "development",
        "ManagedBy": "deployment-tool"
    })


# ============================================================================
# GCP PLATFORM OPTIONS
# ============================================================================

class GCPMachineType(Enum):
    """GCP Compute Engine machine types."""
    # Shared-core (Dev/Test)
    E2_MICRO = "e2-micro"         # 0.25-2 vCPU, 1 GB
    E2_SMALL = "e2-small"         # 0.5-2 vCPU, 2 GB
    E2_MEDIUM = "e2-medium"       # 1-2 vCPU, 4 GB
    
    # General Purpose
    E2_STANDARD_2 = "e2-standard-2"   # 2 vCPU, 8 GB
    E2_STANDARD_4 = "e2-standard-4"   # 4 vCPU, 16 GB
    E2_STANDARD_8 = "e2-standard-8"   # 8 vCPU, 32 GB
    E2_STANDARD_16 = "e2-standard-16" # 16 vCPU, 64 GB
    
    N2_STANDARD_2 = "n2-standard-2"   # 2 vCPU, 8 GB
    N2_STANDARD_4 = "n2-standard-4"   # 4 vCPU, 16 GB
    N2_STANDARD_8 = "n2-standard-8"   # 8 vCPU, 32 GB
    
    # High Memory
    N2_HIGHMEM_2 = "n2-highmem-2"     # 2 vCPU, 16 GB
    N2_HIGHMEM_4 = "n2-highmem-4"     # 4 vCPU, 32 GB
    N2_HIGHMEM_8 = "n2-highmem-8"     # 8 vCPU, 64 GB
    
    # Compute Optimized
    C2_STANDARD_4 = "c2-standard-4"   # 4 vCPU, 16 GB
    C2_STANDARD_8 = "c2-standard-8"   # 8 vCPU, 32 GB


class GCPCloudSQLTier(Enum):
    """GCP Cloud SQL instance tiers."""
    DB_F1_MICRO = "db-f1-micro"       # Shared, 0.6 GB
    DB_G1_SMALL = "db-g1-small"       # Shared, 1.7 GB
    DB_N1_STANDARD_1 = "db-n1-standard-1"   # 1 vCPU, 3.75 GB
    DB_N1_STANDARD_2 = "db-n1-standard-2"   # 2 vCPU, 7.5 GB
    DB_N1_STANDARD_4 = "db-n1-standard-4"   # 4 vCPU, 15 GB
    DB_N1_STANDARD_8 = "db-n1-standard-8"   # 8 vCPU, 30 GB
    DB_N1_HIGHMEM_2 = "db-n1-highmem-2"     # 2 vCPU, 13 GB
    DB_N1_HIGHMEM_4 = "db-n1-highmem-4"     # 4 vCPU, 26 GB
    DB_N1_HIGHMEM_8 = "db-n1-highmem-8"     # 8 vCPU, 52 GB


class GCPMemorystoreTier(Enum):
    """GCP Memorystore (Redis) tiers."""
    BASIC = "BASIC"
    STANDARD_HA = "STANDARD_HA"


class GCPRegion(Enum):
    """GCP regions."""
    US_CENTRAL1 = "us-central1"       # Iowa
    US_EAST1 = "us-east1"             # South Carolina
    US_EAST4 = "us-east4"             # N. Virginia
    US_WEST1 = "us-west1"             # Oregon
    US_WEST2 = "us-west2"             # Los Angeles
    US_WEST3 = "us-west3"             # Salt Lake City
    US_WEST4 = "us-west4"             # Las Vegas
    EUROPE_WEST1 = "europe-west1"     # Belgium
    EUROPE_WEST2 = "europe-west2"     # London
    EUROPE_WEST3 = "europe-west3"     # Frankfurt
    EUROPE_WEST4 = "europe-west4"     # Netherlands
    EUROPE_NORTH1 = "europe-north1"   # Finland
    ASIA_EAST1 = "asia-east1"         # Taiwan
    ASIA_EAST2 = "asia-east2"         # Hong Kong
    ASIA_SOUTHEAST1 = "asia-southeast1"  # Singapore
    ASIA_SOUTHEAST2 = "asia-southeast2"  # Jakarta
    ASIA_NORTHEAST1 = "asia-northeast1"  # Tokyo
    ASIA_NORTHEAST2 = "asia-northeast2"  # Osaka
    ASIA_SOUTH1 = "asia-south1"       # Mumbai
    AUSTRALIA_SOUTHEAST1 = "australia-southeast1"  # Sydney
    SOUTHAMERICA_EAST1 = "southamerica-east1"      # São Paulo


@dataclass
class GCPPlatformConfig:
    """GCP-specific configuration."""
    project_id: str = ""
    region: GCPRegion = GCPRegion.US_CENTRAL1
    zone: str = "us-central1-a"
    
    # Compute Engine
    frontend_machine_type: GCPMachineType = GCPMachineType.E2_SMALL
    api_machine_type: GCPMachineType = GCPMachineType.E2_STANDARD_2
    database_machine_type: GCPMachineType = GCPMachineType.N2_HIGHMEM_2
    
    # Cloud Run
    use_cloud_run: bool = True
    frontend_cpu: str = "1"
    frontend_memory: str = "512Mi"
    frontend_max_instances: int = 10
    api_cpu: str = "2"
    api_memory: str = "2Gi"
    api_max_instances: int = 20
    
    # GKE (Kubernetes)
    use_gke: bool = False
    gke_cluster_name: str = "crm-gke"
    gke_node_count: int = 3
    gke_machine_type: GCPMachineType = GCPMachineType.E2_STANDARD_2
    gke_autopilot: bool = True
    
    # Cloud SQL
    cloudsql_tier: GCPCloudSQLTier = GCPCloudSQLTier.DB_N1_STANDARD_2
    cloudsql_database_version: str = "MYSQL_8_0"  # MYSQL_8_0, POSTGRES_15
    cloudsql_storage_gb: int = 100
    cloudsql_high_availability: bool = True
    
    # Memorystore
    memorystore_tier: GCPMemorystoreTier = GCPMemorystoreTier.BASIC
    memorystore_memory_gb: int = 1
    
    # VPC
    vpc_name: str = "crm-vpc"
    subnet_cidr: str = "10.0.0.0/24"
    
    # Artifact Registry
    artifact_registry_repo: str = "crm-solution"
    
    # Cloud Storage
    storage_bucket_prefix: str = "crm-solution"
    
    # Cloud Load Balancing
    use_load_balancer: bool = True
    
    # Cloud CDN
    use_cdn: bool = False
    
    # Labels
    labels: Dict[str, str] = field(default_factory=lambda: {
        "project": "crm-solution",
        "environment": "development",
        "managed-by": "deployment-tool"
    })


# ============================================================================
# ON-PREMISES PLATFORM OPTIONS
# ============================================================================

class OnPremVirtualization(Enum):
    """On-premises virtualization platforms."""
    VMWARE = "vmware"
    HYPERV = "hyperv"
    KVM = "kvm"
    PROXMOX = "proxmox"
    BARE_METAL = "bare_metal"


class OnPremContainerRuntime(Enum):
    """Container runtimes for on-premises."""
    DOCKER = "docker"
    PODMAN = "podman"
    CONTAINERD = "containerd"


class OnPremOrchestration(Enum):
    """Container orchestration for on-premises."""
    DOCKER_COMPOSE = "docker_compose"
    DOCKER_SWARM = "docker_swarm"
    KUBERNETES = "kubernetes"
    K3S = "k3s"
    RANCHER = "rancher"
    OPENSHIFT = "openshift"


@dataclass
class OnPremServer:
    """On-premises server specification."""
    hostname: str = ""
    ip_address: str = ""
    ssh_user: str = "deploy"
    ssh_key_path: str = ""
    ssh_port: int = 22
    role: str = "worker"  # master, worker, database, all-in-one
    cpu_cores: int = 4
    memory_gb: int = 16
    storage_gb: int = 100


@dataclass
class OnPremPlatformConfig:
    """On-premises specific configuration."""
    virtualization: OnPremVirtualization = OnPremVirtualization.BARE_METAL
    container_runtime: OnPremContainerRuntime = OnPremContainerRuntime.DOCKER
    orchestration: OnPremOrchestration = OnPremOrchestration.DOCKER_COMPOSE
    
    # Servers
    servers: List[OnPremServer] = field(default_factory=list)
    
    # Kubernetes specifics
    k8s_control_plane_endpoint: str = ""
    k8s_pod_cidr: str = "10.244.0.0/16"
    k8s_service_cidr: str = "10.96.0.0/12"
    k8s_version: str = "1.29"
    
    # Load Balancer
    use_haproxy: bool = False
    use_nginx: bool = True
    load_balancer_ip: str = ""
    
    # Storage
    storage_class: str = "local-path"
    nfs_server: str = ""
    nfs_path: str = ""
    
    # Database
    database_host: str = ""
    database_external: bool = False  # Use external database
    
    # Registry
    private_registry: str = ""
    registry_username: str = ""
    registry_password: str = ""
    
    # DNS
    dns_server: str = ""
    domain: str = "crm.local"
    
    # Proxy
    http_proxy: str = ""
    https_proxy: str = ""
    no_proxy: str = "localhost,127.0.0.1"


# ============================================================================
# UNIFIED PLATFORM OPTIONS
# ============================================================================

@dataclass 
class PlatformOptions:
    """Unified platform options container."""
    azure: AzurePlatformConfig = field(default_factory=AzurePlatformConfig)
    aws: AWSPlatformConfig = field(default_factory=AWSPlatformConfig)
    gcp: GCPPlatformConfig = field(default_factory=GCPPlatformConfig)
    on_premises: OnPremPlatformConfig = field(default_factory=OnPremPlatformConfig)


# ============================================================================
# SIZE RECOMMENDATIONS
# ============================================================================

@dataclass
class SizeRecommendation:
    """Size recommendations based on user count."""
    user_count_min: int
    user_count_max: int
    description: str
    
    # Azure
    azure_frontend_vm: AzureVMSize
    azure_api_vm: AzureVMSize
    azure_database_sku: AzureDatabaseSKU
    azure_redis_sku: AzureRedisSKU
    
    # AWS
    aws_frontend_instance: AWSInstanceType
    aws_api_instance: AWSInstanceType
    aws_rds_class: AWSRDSInstanceClass
    aws_elasticache_type: AWSElastiCacheNodeType
    
    # GCP
    gcp_frontend_machine: GCPMachineType
    gcp_api_machine: GCPMachineType
    gcp_cloudsql_tier: GCPCloudSQLTier


SIZE_RECOMMENDATIONS = [
    SizeRecommendation(
        user_count_min=1,
        user_count_max=50,
        description="Small (Development/POC)",
        azure_frontend_vm=AzureVMSize.B2S,
        azure_api_vm=AzureVMSize.B2MS,
        azure_database_sku=AzureDatabaseSKU.B2S,
        azure_redis_sku=AzureRedisSKU.BASIC_C0,
        aws_frontend_instance=AWSInstanceType.T3_SMALL,
        aws_api_instance=AWSInstanceType.T3_MEDIUM,
        aws_rds_class=AWSRDSInstanceClass.DB_T3_SMALL,
        aws_elasticache_type=AWSElastiCacheNodeType.CACHE_T3_MICRO,
        gcp_frontend_machine=GCPMachineType.E2_SMALL,
        gcp_api_machine=GCPMachineType.E2_MEDIUM,
        gcp_cloudsql_tier=GCPCloudSQLTier.DB_G1_SMALL
    ),
    SizeRecommendation(
        user_count_min=51,
        user_count_max=200,
        description="Medium (Small Business)",
        azure_frontend_vm=AzureVMSize.D2S_V5,
        azure_api_vm=AzureVMSize.D4S_V5,
        azure_database_sku=AzureDatabaseSKU.D2DS_V4,
        azure_redis_sku=AzureRedisSKU.STANDARD_C1,
        aws_frontend_instance=AWSInstanceType.T3_MEDIUM,
        aws_api_instance=AWSInstanceType.M6I_LARGE,
        aws_rds_class=AWSRDSInstanceClass.DB_M6I_LARGE,
        aws_elasticache_type=AWSElastiCacheNodeType.CACHE_T3_SMALL,
        gcp_frontend_machine=GCPMachineType.E2_MEDIUM,
        gcp_api_machine=GCPMachineType.E2_STANDARD_2,
        gcp_cloudsql_tier=GCPCloudSQLTier.DB_N1_STANDARD_1
    ),
    SizeRecommendation(
        user_count_min=201,
        user_count_max=500,
        description="Large (Medium Business)",
        azure_frontend_vm=AzureVMSize.D4S_V5,
        azure_api_vm=AzureVMSize.D8S_V5,
        azure_database_sku=AzureDatabaseSKU.D4DS_V4,
        azure_redis_sku=AzureRedisSKU.STANDARD_C2,
        aws_frontend_instance=AWSInstanceType.M6I_LARGE,
        aws_api_instance=AWSInstanceType.M6I_XLARGE,
        aws_rds_class=AWSRDSInstanceClass.DB_M6I_XLARGE,
        aws_elasticache_type=AWSElastiCacheNodeType.CACHE_T3_MEDIUM,
        gcp_frontend_machine=GCPMachineType.E2_STANDARD_2,
        gcp_api_machine=GCPMachineType.E2_STANDARD_4,
        gcp_cloudsql_tier=GCPCloudSQLTier.DB_N1_STANDARD_2
    ),
    SizeRecommendation(
        user_count_min=501,
        user_count_max=2000,
        description="Enterprise (Large Business)",
        azure_frontend_vm=AzureVMSize.D8S_V5,
        azure_api_vm=AzureVMSize.D16S_V5,
        azure_database_sku=AzureDatabaseSKU.D8DS_V4,
        azure_redis_sku=AzureRedisSKU.PREMIUM_P1,
        aws_frontend_instance=AWSInstanceType.M6I_XLARGE,
        aws_api_instance=AWSInstanceType.M6I_2XLARGE,
        aws_rds_class=AWSRDSInstanceClass.DB_M6I_2XLARGE,
        aws_elasticache_type=AWSElastiCacheNodeType.CACHE_M6G_LARGE,
        gcp_frontend_machine=GCPMachineType.E2_STANDARD_4,
        gcp_api_machine=GCPMachineType.E2_STANDARD_8,
        gcp_cloudsql_tier=GCPCloudSQLTier.DB_N1_STANDARD_4
    ),
    SizeRecommendation(
        user_count_min=2001,
        user_count_max=10000,
        description="Large Enterprise",
        azure_frontend_vm=AzureVMSize.D16S_V5,
        azure_api_vm=AzureVMSize.D32S_V5,
        azure_database_sku=AzureDatabaseSKU.D16DS_V4,
        azure_redis_sku=AzureRedisSKU.PREMIUM_P2,
        aws_frontend_instance=AWSInstanceType.M6I_2XLARGE,
        aws_api_instance=AWSInstanceType.M6I_4XLARGE,
        aws_rds_class=AWSRDSInstanceClass.DB_R6I_2XLARGE,
        aws_elasticache_type=AWSElastiCacheNodeType.CACHE_R6G_LARGE,
        gcp_frontend_machine=GCPMachineType.E2_STANDARD_8,
        gcp_api_machine=GCPMachineType.E2_STANDARD_16,
        gcp_cloudsql_tier=GCPCloudSQLTier.DB_N1_STANDARD_8
    )
]


def get_size_recommendation(user_count: int) -> SizeRecommendation:
    """Get size recommendation based on expected user count."""
    for rec in SIZE_RECOMMENDATIONS:
        if rec.user_count_min <= user_count <= rec.user_count_max:
            return rec
    # Return largest for very large deployments
    return SIZE_RECOMMENDATIONS[-1]
