#!/usr/bin/env python3
"""
CRM Solution - Deployment Configuration Models
Comprehensive data models for deployment configuration.

Author: Abhishek Lal
License: AGPL-3.0
"""

from dataclasses import dataclass, field, asdict
from typing import Optional, Dict, List, Any
from enum import Enum
import json
from datetime import datetime, timezone


class TargetPlatform(Enum):
    """Target deployment platform."""
    AZURE = "azure"
    AWS = "aws"
    GCP = "gcp"
    ON_PREMISES = "on_premises"
    HYBRID = "hybrid"


class DeploymentType(Enum):
    """Deployment type (production vs non-production)."""
    DEVELOPMENT = "development"
    STAGING = "staging"
    PRODUCTION = "production"


class HostingType(Enum):
    """Hosting type for components."""
    VM = "vm"
    CONTAINER = "container"
    SERVERLESS = "serverless"
    KUBERNETES = "kubernetes"
    PAAS = "paas"


class DatabaseType(Enum):
    """Database type."""
    MARIADB = "mariadb"
    MYSQL = "mysql"
    POSTGRESQL = "postgresql"
    SQL_SERVER = "sql_server"


class DatabaseHosting(Enum):
    """Database hosting option."""
    VM = "vm"
    PAAS = "paas"
    CLUSTER = "cluster"
    MANAGED = "managed"


class DeploymentArchitecture(Enum):
    """Deployment architecture."""
    MONOLITHIC = "monolithic"
    MICROSERVICES = "microservices"


class ProviderStrategy(Enum):
    """Provider implementation strategy."""
    BUILTIN = "builtin"
    OPENSOURCE = "opensource"
    CLOUD_SAAS = "cloud_saas"


@dataclass
class SSLConfiguration:
    """SSL/HTTPS configuration."""
    enabled: bool = False
    certificate_source: str = "self_signed"  # self_signed, upload, lets_encrypt
    certificate_path: Optional[str] = None
    private_key_path: Optional[str] = None
    ca_bundle_path: Optional[str] = None
    domain: str = "localhost"
    letsencrypt_email: Optional[str] = None
    force_https: bool = True
    hsts_enabled: bool = True
    min_tls_version: str = "1.2"
    auto_generate: bool = False  # Generate self-signed certificate


@dataclass
class HostConfiguration:
    """Host configuration for a service."""
    hostname: str = "localhost"
    port: int = 80
    protocol: str = "http"  # http, https
    external_url: Optional[str] = None  # For external access
    internal_only: bool = False  # Only accessible internally


@dataclass
class ServiceHosts:
    """Host configurations for all services."""
    # Core services
    frontend: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=80))
    api: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=5000))
    database: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=3306))
    redis: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=6379))

    # Provider services
    meilisearch: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=7700))
    chatwoot: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=3000))
    novu: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=3001))
    superset: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=8088))
    docuseal: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=3002))
    ollama: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=11434))
    n8n: HostConfiguration = field(default_factory=lambda: HostConfiguration(hostname="localhost", port=5678))


@dataclass
class NetworkConfiguration:
    """Network configuration."""
    deployment_type: DeploymentType = DeploymentType.DEVELOPMENT
    frontend_port: int = 80
    api_port: int = 5000
    database_port: int = 3306
    redis_port: int = 6379
    vnet_cidr: str = "10.0.0.0/16"
    subnet_cidr: str = "10.0.1.0/24"
    public_ip: bool = True
    load_balancer: bool = True
    cdn_enabled: bool = False
    waf_enabled: bool = False
    # New host configurations
    hosts: ServiceHosts = field(default_factory=ServiceHosts)


@dataclass
class FrontendConfig:
    """Frontend deployment configuration."""
    hosting_type: HostingType = HostingType.CONTAINER
    instance_count: int = 2
    auto_scale: bool = True
    min_instances: int = 1
    max_instances: int = 10
    cdn_enabled: bool = False
    # VM-specific
    vm_size: Optional[str] = None
    # Container-specific
    container_image: str = "crm-frontend:latest"
    container_cpu: str = "0.5"
    container_memory: str = "512Mi"
    # Serverless-specific
    function_timeout: int = 30


@dataclass
class APIConfig:
    """API deployment configuration."""
    hosting_type: HostingType = HostingType.CONTAINER
    architecture: DeploymentArchitecture = DeploymentArchitecture.MONOLITHIC
    instance_count: int = 2
    auto_scale: bool = True
    min_instances: int = 1
    max_instances: int = 20
    # VM-specific
    vm_size: Optional[str] = None
    # Container-specific
    container_image: str = "crm-api:latest"
    container_cpu: str = "1"
    container_memory: str = "2Gi"
    # Microservices
    microservices: List[str] = field(default_factory=lambda: [
        "identity", "customer", "sales", "marketing", "servicedesk", "core", "gateway"
    ])


@dataclass
class DatabaseConfig:
    """Database deployment configuration."""
    type: DatabaseType = DatabaseType.MARIADB
    hosting: DatabaseHosting = DatabaseHosting.PAAS
    version: str = "10.11"
    # VM-specific
    vm_size: Optional[str] = None
    storage_gb: int = 100
    # Cluster-specific
    cluster_size: int = 3
    high_availability: bool = True
    # Performance
    max_connections: int = 200
    backup_enabled: bool = True
    backup_retention_days: int = 30
    geo_redundant_backup: bool = False
    # Connection
    host: str = "localhost"
    port: int = 3306
    database_name: str = "crm_db"
    admin_username: str = "crmadmin"
    admin_password: Optional[str] = None  # Will be generated if not provided


@dataclass
class CacheConfig:
    """Cache (Redis) configuration."""
    enabled: bool = True
    hosting: DatabaseHosting = DatabaseHosting.PAAS
    version: str = "7.0"
    cluster_enabled: bool = False
    memory_mb: int = 256
    host: str = "localhost"
    port: int = 6379
    password: Optional[str] = None


@dataclass
class ModuleSelection:
    """CRM module enablement."""
    core_crm: bool = True
    sales: bool = True
    marketing: bool = True
    service_desk: bool = True
    itsm: bool = True
    knowledge_base: bool = True
    customer_portal: bool = False
    partner_portal: bool = False
    ai_features: bool = True


@dataclass
class ProviderSelection:
    """Provider selection for each capability.

    **Default deployment model:**
    - Non-cloud (on_premises / hybrid): all open-source self-hosted providers.
    - Cloud deployments: use cloud-native recommended providers (set via
      ``get_default_providers(platform)`` from ``models.provider_models``).
    """
    # Search Provider — default: Meilisearch (OSS)
    search_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    search_provider: str = "meilisearch"  # builtin, meilisearch, algolia, elasticsearch, azure_cognitive_search

    # Chat Provider — default: Chatwoot (OSS)
    chat_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    chat_provider: str = "chatwoot"  # builtin, chatwoot, intercom, zendesk, rocketchat

    # Notification Provider — default: Novu (OSS)
    notification_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    notification_provider: str = "novu"  # builtin, novu, twilio, sendgrid

    # Analytics Provider — default: Metabase (OSS)
    analytics_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    analytics_provider: str = "metabase"  # builtin, superset, metabase, powerbi

    # E-Signature Provider — default: DocuSeal (OSS)
    signature_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    signature_provider: str = "docuseal"  # builtin, docuseal, docusign, adobe_sign

    # Integration Provider — default: n8n (OSS)
    integration_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    integration_provider: str = "n8n"  # builtin, n8n, zapier, make, workato

    # AI/LLM Provider — default: Ollama (OSS local LLM)
    ai_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    ai_provider: str = "ollama"  # ollama, openai, azure_openai, anthropic, openrouter, bedrock, gemini

    # Monitoring — default: Prometheus + Grafana + Loki (OSS)
    monitoring_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    monitoring_provider: str = "prometheus_grafana"  # prometheus_grafana, uptime_kuma, datadog, azure_monitor, cloudwatch, cloud_monitoring, newrelic

    # Container Management — default: Portainer CE (OSS)
    portainer_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    portainer_provider: str = "portainer_ce"  # portainer_ce, portainer_be, kubernetes_dashboard

    # Object Storage — default: MinIO (OSS)
    storage_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    storage_provider: str = "minio"  # minio, azure_blob, aws_s3, gcs

    # Reverse Proxy — default: Traefik (OSS)
    reverse_proxy_strategy: ProviderStrategy = ProviderStrategy.OPENSOURCE
    reverse_proxy_provider: str = "traefik"  # traefik, nginx, caddy


@dataclass
class ProviderCredentials:
    """Credentials for external providers."""
    # Search
    meilisearch_api_key: Optional[str] = None
    algolia_app_id: Optional[str] = None
    algolia_api_key: Optional[str] = None
    
    # Chat
    chatwoot_api_key: Optional[str] = None
    chatwoot_account_id: Optional[str] = None
    intercom_app_id: Optional[str] = None
    intercom_api_key: Optional[str] = None
    
    # Notifications
    novu_api_key: Optional[str] = None
    twilio_account_sid: Optional[str] = None
    twilio_auth_token: Optional[str] = None
    sendgrid_api_key: Optional[str] = None
    smtp_host: Optional[str] = None
    smtp_port: int = 587
    smtp_username: Optional[str] = None
    smtp_password: Optional[str] = None
    
    # Analytics
    superset_username: Optional[str] = None
    superset_password: Optional[str] = None
    powerbi_client_id: Optional[str] = None
    powerbi_client_secret: Optional[str] = None
    powerbi_tenant_id: Optional[str] = None
    
    # E-Signature
    docuseal_api_key: Optional[str] = None
    docusign_integration_key: Optional[str] = None
    docusign_user_id: Optional[str] = None
    docusign_account_id: Optional[str] = None
    docusign_rsa_key_path: Optional[str] = None
    
    # Integration
    n8n_api_key: Optional[str] = None
    zapier_api_key: Optional[str] = None
    
    # AI
    openai_api_key: Optional[str] = None
    azure_openai_endpoint: Optional[str] = None
    azure_openai_api_key: Optional[str] = None
    anthropic_api_key: Optional[str] = None
    openrouter_api_key: Optional[str] = None
    aws_access_key: Optional[str] = None
    aws_secret_key: Optional[str] = None
    aws_region: str = "us-east-1"


@dataclass
class CloudCredentials:
    """Cloud platform credentials."""
    # Azure
    azure_subscription_id: Optional[str] = None
    azure_tenant_id: Optional[str] = None
    azure_client_id: Optional[str] = None
    azure_client_secret: Optional[str] = None
    azure_resource_group: str = "crm-solution-rg"
    azure_location: str = "eastus"
    
    # AWS
    aws_access_key_id: Optional[str] = None
    aws_secret_access_key: Optional[str] = None
    aws_region: str = "us-east-1"
    aws_account_id: Optional[str] = None
    
    # GCP
    gcp_project_id: Optional[str] = None
    gcp_service_account_key_path: Optional[str] = None
    gcp_region: str = "us-central1"
    gcp_zone: str = "us-central1-a"


@dataclass
class GitConfiguration:
    """Git repository configuration."""
    repository_url: str = "https://github.com/alal76/crm-solution.git"
    branch: str = "main"
    ssh_key_path: Optional[str] = None
    use_ssh: bool = False
    personal_access_token: Optional[str] = None


@dataclass
class MonitoringConfig:
    """Monitoring and logging configuration.

    ``provider`` mirrors ``ProviderSelection.monitoring_provider`` and drives
    which monitoring stack is deployed.  The boolean flags below are retained
    for backwards compatibility and are auto-derived from the provider when not
    explicitly set.
    """
    # Provider key — matches ALL_PROVIDERS["monitoring"] keys.
    # Default: "prometheus_grafana" (OSS self-hosted stack).
    provider: str = "prometheus_grafana"

    # Convenience flags (kept for backward-compat)
    prometheus_enabled: bool = True
    grafana_enabled: bool = True
    loki_enabled: bool = False
    uptime_kuma_enabled: bool = False

    # Cloud-native / SaaS monitoring
    application_insights_enabled: bool = False
    application_insights_key: Optional[str] = None
    log_analytics_enabled: bool = False
    log_analytics_workspace_id: Optional[str] = None
    datadog_api_key: Optional[str] = None
    cloudwatch_region: Optional[str] = None
    cloud_monitoring_project: Optional[str] = None
    newrelic_license_key: Optional[str] = None

    # Common settings
    log_retention_days: int = 30
    alert_email: Optional[str] = None


@dataclass
class DeploymentOptions:
    """Deployment behavior options."""
    simulation_mode: bool = True  # Dry run by default
    stop_on_error: bool = True
    rollback_on_failure: bool = True
    verbose_logging: bool = True
    parallel_deployment: bool = True
    health_check_timeout_seconds: int = 300
    deployment_timeout_minutes: int = 60
    skip_validation: bool = False
    skip_tests: bool = False
    cleanup_on_success: bool = False  # Clean up temp resources


@dataclass
class DeploymentConfig:
    """Master deployment configuration."""
    # Metadata
    config_version: str = "2.0.0"
    config_name: str = "crm-deployment"
    created_at: str = field(default_factory=lambda: datetime.now(timezone.utc).isoformat())
    created_by: str = ""
    
    # Target Platform
    platform: TargetPlatform = TargetPlatform.AZURE
    deployment_type: DeploymentType = DeploymentType.DEVELOPMENT  # New field
    environment: str = "development"  # development, staging, production
    
    # Architecture
    frontend: FrontendConfig = field(default_factory=FrontendConfig)
    api: APIConfig = field(default_factory=APIConfig)
    database: DatabaseConfig = field(default_factory=DatabaseConfig)
    cache: CacheConfig = field(default_factory=CacheConfig)
    
    # Network & Security
    network: NetworkConfiguration = field(default_factory=NetworkConfiguration)
    ssl: SSLConfiguration = field(default_factory=SSLConfiguration)
    
    # Modules & Providers
    modules: ModuleSelection = field(default_factory=ModuleSelection)
    providers: ProviderSelection = field(default_factory=ProviderSelection)
    
    # Credentials
    provider_credentials: ProviderCredentials = field(default_factory=ProviderCredentials)
    cloud_credentials: CloudCredentials = field(default_factory=CloudCredentials)
    
    # Git & Source
    git: GitConfiguration = field(default_factory=GitConfiguration)
    
    # Monitoring
    monitoring: MonitoringConfig = field(default_factory=MonitoringConfig)
    
    # Deployment Options
    options: DeploymentOptions = field(default_factory=DeploymentOptions)
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary with enum handling."""
        def convert(obj):
            if isinstance(obj, Enum):
                return obj.value
            elif hasattr(obj, '__dataclass_fields__'):
                return {k: convert(v) for k, v in asdict(obj).items()}
            elif isinstance(obj, dict):
                return {k: convert(v) for k, v in obj.items()}
            elif isinstance(obj, list):
                return [convert(item) for item in obj]
            return obj
        return convert(asdict(self))
    
    def to_json(self, indent: int = 2) -> str:
        """Convert to JSON string."""
        return json.dumps(self.to_dict(), indent=indent)
    
    def save(self, filepath: str) -> None:
        """Save configuration to file."""
        with open(filepath, 'w') as f:
            f.write(self.to_json())
    
    @classmethod
    def load(cls, filepath: str) -> 'DeploymentConfig':
        """Load configuration from file."""
        with open(filepath, 'r') as f:
            data = json.load(f)
        return cls.from_dict(data)
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'DeploymentConfig':
        """Create from dictionary."""
        # Convert string enums back to Enum types
        if 'platform' in data and isinstance(data['platform'], str):
            data['platform'] = TargetPlatform(data['platform'])
        if 'deployment_type' in data and isinstance(data['deployment_type'], str):
            data['deployment_type'] = DeploymentType(data['deployment_type'])
        
        # Convert nested dataclasses
        if 'frontend' in data and isinstance(data['frontend'], dict):
            if 'hosting_type' in data['frontend']:
                data['frontend']['hosting_type'] = HostingType(data['frontend']['hosting_type'])
            data['frontend'] = FrontendConfig(**data['frontend'])
        
        if 'api' in data and isinstance(data['api'], dict):
            if 'hosting_type' in data['api']:
                data['api']['hosting_type'] = HostingType(data['api']['hosting_type'])
            if 'architecture' in data['api']:
                data['api']['architecture'] = DeploymentArchitecture(data['api']['architecture'])
            data['api'] = APIConfig(**data['api'])
        
        if 'database' in data and isinstance(data['database'], dict):
            if 'type' in data['database']:
                data['database']['type'] = DatabaseType(data['database']['type'])
            if 'hosting' in data['database']:
                data['database']['hosting'] = DatabaseHosting(data['database']['hosting'])
            data['database'] = DatabaseConfig(**data['database'])
        
        if 'cache' in data and isinstance(data['cache'], dict):
            if 'hosting' in data['cache']:
                data['cache']['hosting'] = DatabaseHosting(data['cache']['hosting'])
            data['cache'] = CacheConfig(**data['cache'])
        
        if 'network' in data and isinstance(data['network'], dict):
            data['network'] = NetworkConfiguration(**data['network'])
        
        if 'ssl' in data and isinstance(data['ssl'], dict):
            data['ssl'] = SSLConfiguration(**data['ssl'])
        
        if 'modules' in data and isinstance(data['modules'], dict):
            data['modules'] = ModuleSelection(**data['modules'])
        
        if 'providers' in data and isinstance(data['providers'], dict):
            for key in [
                'search_strategy', 'chat_strategy', 'notification_strategy',
                'analytics_strategy', 'signature_strategy', 'integration_strategy',
                'ai_strategy', 'monitoring_strategy', 'portainer_strategy',
                'storage_strategy', 'reverse_proxy_strategy',
            ]:
                if key in data['providers']:
                    data['providers'][key] = ProviderStrategy(data['providers'][key])
            data['providers'] = ProviderSelection(**data['providers'])
        
        if 'provider_credentials' in data and isinstance(data['provider_credentials'], dict):
            data['provider_credentials'] = ProviderCredentials(**data['provider_credentials'])
        
        if 'cloud_credentials' in data and isinstance(data['cloud_credentials'], dict):
            data['cloud_credentials'] = CloudCredentials(**data['cloud_credentials'])
        
        if 'git' in data and isinstance(data['git'], dict):
            data['git'] = GitConfiguration(**data['git'])
        
        if 'monitoring' in data and isinstance(data['monitoring'], dict):
            data['monitoring'] = MonitoringConfig(**data['monitoring'])
        
        if 'options' in data and isinstance(data['options'], dict):
            data['options'] = DeploymentOptions(**data['options'])
        
        return cls(**data)


@dataclass
class DeploymentState:
    """Tracks the state of a deployment."""
    deployment_id: str
    config: DeploymentConfig
    status: str = "pending"  # pending, running, validating, deploying, completed, failed, rolled_back
    phase: str = "initialization"
    started_at: Optional[str] = None
    completed_at: Optional[str] = None
    error_message: Optional[str] = None
    deployed_resources: List[Dict[str, Any]] = field(default_factory=list)
    rollback_stack: List[Dict[str, Any]] = field(default_factory=list)
    validation_results: List[Dict[str, Any]] = field(default_factory=list)
    log_file: Optional[str] = None


# ============================================================================
# PLATFORM-AWARE PROVIDER SELECTION BUILDER
# ============================================================================

def build_provider_selection_for_platform(platform: str) -> "ProviderSelection":
    """Return a ``ProviderSelection`` pre-populated with the recommended providers
    for the given deployment platform.

    On-premises / hybrid → all OSS self-hosted providers.
    Azure / AWS / GCP    → cloud-native services where available.

    Args:
        platform: One of "on_premises", "hybrid", "azure", "aws", "gcp".

    Returns:
        A fully populated ``ProviderSelection`` instance.
    """
    try:
        from models.provider_models import get_default_providers  # noqa: PLC0415
    except ImportError:
        try:
            from provider_models import get_default_providers  # noqa: PLC0415
        except ImportError:
            return ProviderSelection()  # fallback to OSS defaults already set

    defaults = get_default_providers(platform)

    # Determine strategy based on whether the platform is a cloud target
    _CLOUD_PLATFORMS = {"azure", "aws", "gcp"}
    cloud = platform in _CLOUD_PLATFORMS

    def _strategy(provider_key: str) -> ProviderStrategy:
        """Infer strategy from provider key."""
        if provider_key in ("builtin", ""):
            return ProviderStrategy.BUILTIN
        if cloud and provider_key not in (
            "meilisearch", "chatwoot", "novu", "metabase", "docuseal",
            "n8n", "ollama", "portainer_ce", "portainer_be", "minio",
            "traefik", "nginx", "caddy", "prometheus_grafana", "uptime_kuma",
            "elasticsearch",
        ):
            return ProviderStrategy.CLOUD_SAAS
        return ProviderStrategy.OPENSOURCE

    return ProviderSelection(
        search_provider=defaults.get("search", "meilisearch"),
        search_strategy=_strategy(defaults.get("search", "meilisearch")),
        chat_provider=defaults.get("chat", "chatwoot"),
        chat_strategy=_strategy(defaults.get("chat", "chatwoot")),
        notification_provider=defaults.get("notification", "novu"),
        notification_strategy=_strategy(defaults.get("notification", "novu")),
        analytics_provider=defaults.get("analytics", "metabase"),
        analytics_strategy=_strategy(defaults.get("analytics", "metabase")),
        signature_provider=defaults.get("signature", "docuseal"),
        signature_strategy=_strategy(defaults.get("signature", "docuseal")),
        integration_provider=defaults.get("integration", "n8n"),
        integration_strategy=_strategy(defaults.get("integration", "n8n")),
        ai_provider=defaults.get("ai", "ollama"),
        ai_strategy=_strategy(defaults.get("ai", "ollama")),
        monitoring_provider=defaults.get("monitoring", "prometheus_grafana"),
        monitoring_strategy=_strategy(defaults.get("monitoring", "prometheus_grafana")),
        portainer_provider=defaults.get("portainer", "portainer_ce"),
        portainer_strategy=_strategy(defaults.get("portainer", "portainer_ce")),
        storage_provider=defaults.get("storage", "minio"),
        storage_strategy=_strategy(defaults.get("storage", "minio")),
        reverse_proxy_provider=defaults.get("reverse_proxy", "traefik"),
        reverse_proxy_strategy=_strategy(defaults.get("reverse_proxy", "traefik")),
    )
