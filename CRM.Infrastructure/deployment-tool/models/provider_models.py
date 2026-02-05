#!/usr/bin/env python3
"""
CRM Solution - Provider Configuration Models
Detailed configuration for BuiltIn, OpenSource, and Cloud SaaS providers.

Author: Abhishek Lal
License: AGPL-3.0
"""

from dataclasses import dataclass, field
from typing import Dict, List, Optional, Any
from enum import Enum


# ============================================================================
# PROVIDER DEFINITIONS
# ============================================================================

@dataclass
class ProviderInfo:
    """Information about a provider."""
    name: str
    display_name: str
    category: str
    strategy: str  # builtin, opensource, cloud_saas
    description: str
    documentation_url: str
    license: str
    requires_credentials: List[str]
    requires_containers: List[str]
    estimated_monthly_cost: str  # "Free", "$X/mo", "Usage-based"
    setup_complexity: str  # "Low", "Medium", "High"


# ============================================================================
# SEARCH PROVIDERS
# ============================================================================

SEARCH_PROVIDERS = {
    "builtin": ProviderInfo(
        name="builtin",
        display_name="Built-In SQL Search",
        category="search",
        strategy="builtin",
        description="Basic SQL LIKE-based search using Entity Framework Core. Good for small datasets.",
        documentation_url="",
        license="Included",
        requires_credentials=[],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "meilisearch": ProviderInfo(
        name="meilisearch",
        display_name="Meilisearch",
        category="search",
        strategy="opensource",
        description="Lightning-fast, typo-tolerant search engine. Open-source, self-hosted.",
        documentation_url="https://docs.meilisearch.com/",
        license="MIT",
        requires_credentials=["meilisearch_api_key"],
        requires_containers=["meilisearch"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "algolia": ProviderInfo(
        name="algolia",
        display_name="Algolia",
        category="search",
        strategy="cloud_saas",
        description="Powerful hosted search API with instant results and typo tolerance.",
        documentation_url="https://www.algolia.com/doc/",
        license="SaaS",
        requires_credentials=["algolia_app_id", "algolia_api_key"],
        requires_containers=[],
        estimated_monthly_cost="$1/1K searches",
        setup_complexity="Low"
    ),
    "elasticsearch": ProviderInfo(
        name="elasticsearch",
        display_name="Elasticsearch",
        category="search",
        strategy="opensource",
        description="Distributed search and analytics engine. Powerful but resource-intensive.",
        documentation_url="https://www.elastic.co/guide/",
        license="Elastic License 2.0",
        requires_credentials=["elasticsearch_username", "elasticsearch_password"],
        requires_containers=["elasticsearch"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="High"
    ),
    "azure_cognitive_search": ProviderInfo(
        name="azure_cognitive_search",
        display_name="Azure Cognitive Search",
        category="search",
        strategy="cloud_saas",
        description="Enterprise search with AI enrichment capabilities.",
        documentation_url="https://docs.microsoft.com/azure/search/",
        license="SaaS",
        requires_credentials=["azure_search_admin_key", "azure_search_query_key"],
        requires_containers=[],
        estimated_monthly_cost="$73+/unit/mo",
        setup_complexity="Medium"
    )
}


# ============================================================================
# CHAT PROVIDERS
# ============================================================================

CHAT_PROVIDERS = {
    "builtin": ProviderInfo(
        name="builtin",
        display_name="Built-In Chat (Stub)",
        category="chat",
        strategy="builtin",
        description="Basic in-memory chat implementation for development.",
        documentation_url="",
        license="Included",
        requires_credentials=[],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "chatwoot": ProviderInfo(
        name="chatwoot",
        display_name="Chatwoot",
        category="chat",
        strategy="opensource",
        description="Open-source customer engagement platform with multi-channel support.",
        documentation_url="https://www.chatwoot.com/docs/",
        license="MIT",
        requires_credentials=["chatwoot_api_key", "chatwoot_account_id"],
        requires_containers=["chatwoot", "chatwoot-sidekiq", "chatwoot-postgres", "chatwoot-redis"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Medium"
    ),
    "intercom": ProviderInfo(
        name="intercom",
        display_name="Intercom",
        category="chat",
        strategy="cloud_saas",
        description="Customer messaging platform with powerful automation.",
        documentation_url="https://developers.intercom.com/",
        license="SaaS",
        requires_credentials=["intercom_app_id", "intercom_api_key"],
        requires_containers=[],
        estimated_monthly_cost="$74+/seat/mo",
        setup_complexity="Low"
    ),
    "zendesk": ProviderInfo(
        name="zendesk",
        display_name="Zendesk Chat",
        category="chat",
        strategy="cloud_saas",
        description="Enterprise customer service platform.",
        documentation_url="https://developer.zendesk.com/",
        license="SaaS",
        requires_credentials=["zendesk_subdomain", "zendesk_api_token"],
        requires_containers=[],
        estimated_monthly_cost="$19+/agent/mo",
        setup_complexity="Low"
    ),
    "freshchat": ProviderInfo(
        name="freshchat",
        display_name="Freshchat",
        category="chat",
        strategy="cloud_saas",
        description="Modern messaging software for customer engagement.",
        documentation_url="https://developers.freshchat.com/",
        license="SaaS",
        requires_credentials=["freshchat_api_key", "freshchat_app_id"],
        requires_containers=[],
        estimated_monthly_cost="$19+/agent/mo",
        setup_complexity="Low"
    )
}


# ============================================================================
# NOTIFICATION PROVIDERS
# ============================================================================

NOTIFICATION_PROVIDERS = {
    "builtin": ProviderInfo(
        name="builtin",
        display_name="Built-In SMTP",
        category="notification",
        strategy="builtin",
        description="Basic SMTP email notifications using System.Net.Mail.",
        documentation_url="",
        license="Included",
        requires_credentials=["smtp_host", "smtp_port", "smtp_username", "smtp_password"],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "novu": ProviderInfo(
        name="novu",
        display_name="Novu",
        category="notification",
        strategy="opensource",
        description="Open-source notification infrastructure for developers.",
        documentation_url="https://docs.novu.co/",
        license="MIT",
        requires_credentials=["novu_api_key"],
        requires_containers=["novu-api", "novu-worker", "novu-web", "novu-mongodb", "novu-redis"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Medium"
    ),
    "twilio": ProviderInfo(
        name="twilio",
        display_name="Twilio",
        category="notification",
        strategy="cloud_saas",
        description="Cloud communications platform for SMS, voice, and more.",
        documentation_url="https://www.twilio.com/docs",
        license="SaaS",
        requires_credentials=["twilio_account_sid", "twilio_auth_token"],
        requires_containers=[],
        estimated_monthly_cost="$0.0079/SMS",
        setup_complexity="Low"
    ),
    "sendgrid": ProviderInfo(
        name="sendgrid",
        display_name="SendGrid",
        category="notification",
        strategy="cloud_saas",
        description="Email delivery service by Twilio.",
        documentation_url="https://docs.sendgrid.com/",
        license="SaaS",
        requires_credentials=["sendgrid_api_key"],
        requires_containers=[],
        estimated_monthly_cost="$20+/mo",
        setup_complexity="Low"
    ),
    "onesignal": ProviderInfo(
        name="onesignal",
        display_name="OneSignal",
        category="notification",
        strategy="cloud_saas",
        description="Push notification service for mobile and web.",
        documentation_url="https://documentation.onesignal.com/",
        license="SaaS",
        requires_credentials=["onesignal_app_id", "onesignal_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Free-$99+/mo",
        setup_complexity="Low"
    )
}


# ============================================================================
# ANALYTICS PROVIDERS
# ============================================================================

ANALYTICS_PROVIDERS = {
    "builtin": ProviderInfo(
        name="builtin",
        display_name="Built-In Analytics",
        category="analytics",
        strategy="builtin",
        description="Basic pre-built dashboards and reports using CRM data.",
        documentation_url="",
        license="Included",
        requires_credentials=[],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "superset": ProviderInfo(
        name="superset",
        display_name="Apache Superset",
        category="analytics",
        strategy="opensource",
        description="Modern data exploration and visualization platform.",
        documentation_url="https://superset.apache.org/docs/",
        license="Apache 2.0",
        requires_credentials=["superset_username", "superset_password"],
        requires_containers=["superset", "superset-worker", "superset-beat", "superset-postgres", "superset-redis"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="High"
    ),
    "metabase": ProviderInfo(
        name="metabase",
        display_name="Metabase",
        category="analytics",
        strategy="opensource",
        description="Easy-to-use business intelligence tool.",
        documentation_url="https://www.metabase.com/docs/",
        license="AGPL",
        requires_credentials=["metabase_username", "metabase_password"],
        requires_containers=["metabase"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "powerbi": ProviderInfo(
        name="powerbi",
        display_name="Power BI Embedded",
        category="analytics",
        strategy="cloud_saas",
        description="Microsoft's enterprise analytics platform.",
        documentation_url="https://docs.microsoft.com/power-bi/",
        license="SaaS",
        requires_credentials=["powerbi_client_id", "powerbi_client_secret", "powerbi_tenant_id", "powerbi_workspace_id"],
        requires_containers=[],
        estimated_monthly_cost="$5K+/mo (capacity)",
        setup_complexity="Medium"
    ),
    "looker": ProviderInfo(
        name="looker",
        display_name="Looker",
        category="analytics",
        strategy="cloud_saas",
        description="Google Cloud's enterprise analytics platform.",
        documentation_url="https://docs.looker.com/",
        license="SaaS",
        requires_credentials=["looker_client_id", "looker_client_secret"],
        requires_containers=[],
        estimated_monthly_cost="$60+/user/mo",
        setup_complexity="High"
    ),
    "quicksight": ProviderInfo(
        name="quicksight",
        display_name="Amazon QuickSight",
        category="analytics",
        strategy="cloud_saas",
        description="AWS's serverless BI service.",
        documentation_url="https://docs.aws.amazon.com/quicksight/",
        license="SaaS",
        requires_credentials=["aws_access_key", "aws_secret_key"],
        requires_containers=[],
        estimated_monthly_cost="$0.30/session",
        setup_complexity="Medium"
    )
}


# ============================================================================
# E-SIGNATURE PROVIDERS
# ============================================================================

SIGNATURE_PROVIDERS = {
    "builtin": ProviderInfo(
        name="builtin",
        display_name="Built-In Manual Workflow",
        category="signature",
        strategy="builtin",
        description="Manual signature tracking without digital signatures.",
        documentation_url="",
        license="Included",
        requires_credentials=[],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "docuseal": ProviderInfo(
        name="docuseal",
        display_name="DocuSeal",
        category="signature",
        strategy="opensource",
        description="Open-source digital document signing platform.",
        documentation_url="https://www.docuseal.co/docs",
        license="AGPL",
        requires_credentials=["docuseal_api_key"],
        requires_containers=["docuseal", "docuseal-postgres"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "docusign": ProviderInfo(
        name="docusign",
        display_name="DocuSign",
        category="signature",
        strategy="cloud_saas",
        description="Industry-leading e-signature platform.",
        documentation_url="https://developers.docusign.com/",
        license="SaaS",
        requires_credentials=["docusign_integration_key", "docusign_user_id", "docusign_account_id", "docusign_rsa_key_path"],
        requires_containers=[],
        estimated_monthly_cost="$10+/envelope",
        setup_complexity="Medium"
    ),
    "adobe_sign": ProviderInfo(
        name="adobe_sign",
        display_name="Adobe Sign",
        category="signature",
        strategy="cloud_saas",
        description="Adobe's e-signature solution.",
        documentation_url="https://developer.adobe.com/document-services/",
        license="SaaS",
        requires_credentials=["adobe_sign_client_id", "adobe_sign_client_secret"],
        requires_containers=[],
        estimated_monthly_cost="$15+/user/mo",
        setup_complexity="Medium"
    ),
    "hellosign": ProviderInfo(
        name="hellosign",
        display_name="HelloSign",
        category="signature",
        strategy="cloud_saas",
        description="Dropbox's e-signature solution.",
        documentation_url="https://developers.hellosign.com/",
        license="SaaS",
        requires_credentials=["hellosign_api_key"],
        requires_containers=[],
        estimated_monthly_cost="$15+/user/mo",
        setup_complexity="Low"
    )
}


# ============================================================================
# INTEGRATION PROVIDERS
# ============================================================================

INTEGRATION_PROVIDERS = {
    "builtin": ProviderInfo(
        name="builtin",
        display_name="Built-In Webhooks",
        category="integration",
        strategy="builtin",
        description="Basic webhook-based integrations.",
        documentation_url="",
        license="Included",
        requires_credentials=[],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "n8n": ProviderInfo(
        name="n8n",
        display_name="n8n",
        category="integration",
        strategy="opensource",
        description="Fair-code workflow automation platform with 400+ integrations.",
        documentation_url="https://docs.n8n.io/",
        license="Sustainable Use License",
        requires_credentials=["n8n_api_key"],
        requires_containers=["n8n"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "zapier": ProviderInfo(
        name="zapier",
        display_name="Zapier",
        category="integration",
        strategy="cloud_saas",
        description="Automation platform with 6000+ app integrations.",
        documentation_url="https://platform.zapier.com/",
        license="SaaS",
        requires_credentials=["zapier_api_key"],
        requires_containers=[],
        estimated_monthly_cost="$20+/mo",
        setup_complexity="Low"
    ),
    "make": ProviderInfo(
        name="make",
        display_name="Make (Integromat)",
        category="integration",
        strategy="cloud_saas",
        description="Visual automation platform for complex workflows.",
        documentation_url="https://www.make.com/en/api-documentation",
        license="SaaS",
        requires_credentials=["make_api_token"],
        requires_containers=[],
        estimated_monthly_cost="$9+/mo",
        setup_complexity="Low"
    ),
    "workato": ProviderInfo(
        name="workato",
        display_name="Workato",
        category="integration",
        strategy="cloud_saas",
        description="Enterprise automation platform.",
        documentation_url="https://docs.workato.com/",
        license="SaaS",
        requires_credentials=["workato_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Custom pricing",
        setup_complexity="Medium"
    )
}


# ============================================================================
# AI/LLM PROVIDERS
# ============================================================================

AI_PROVIDERS = {
    "ollama": ProviderInfo(
        name="ollama",
        display_name="Ollama (Local)",
        category="ai",
        strategy="opensource",
        description="Run large language models locally. Privacy-focused.",
        documentation_url="https://ollama.ai/",
        license="MIT",
        requires_credentials=[],
        requires_containers=["ollama"],
        estimated_monthly_cost="Free (local compute)",
        setup_complexity="Low"
    ),
    "openai": ProviderInfo(
        name="openai",
        display_name="OpenAI",
        category="ai",
        strategy="cloud_saas",
        description="GPT-4 and other frontier models.",
        documentation_url="https://platform.openai.com/docs/",
        license="SaaS",
        requires_credentials=["openai_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    ),
    "azure_openai": ProviderInfo(
        name="azure_openai",
        display_name="Azure OpenAI",
        category="ai",
        strategy="cloud_saas",
        description="OpenAI models hosted on Azure with enterprise features.",
        documentation_url="https://learn.microsoft.com/azure/ai-services/openai/",
        license="SaaS",
        requires_credentials=["azure_openai_endpoint", "azure_openai_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Medium"
    ),
    "anthropic": ProviderInfo(
        name="anthropic",
        display_name="Anthropic Claude",
        category="ai",
        strategy="cloud_saas",
        description="Claude models with strong reasoning capabilities.",
        documentation_url="https://docs.anthropic.com/",
        license="SaaS",
        requires_credentials=["anthropic_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    ),
    "openrouter": ProviderInfo(
        name="openrouter",
        display_name="OpenRouter",
        category="ai",
        strategy="cloud_saas",
        description="Multi-model gateway with access to 100+ models.",
        documentation_url="https://openrouter.ai/docs",
        license="SaaS",
        requires_credentials=["openrouter_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    ),
    "bedrock": ProviderInfo(
        name="bedrock",
        display_name="AWS Bedrock",
        category="ai",
        strategy="cloud_saas",
        description="Managed foundation models on AWS.",
        documentation_url="https://docs.aws.amazon.com/bedrock/",
        license="SaaS",
        requires_credentials=["aws_access_key", "aws_secret_key", "aws_region"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Medium"
    ),
    "gemini": ProviderInfo(
        name="gemini",
        display_name="Google Gemini",
        category="ai",
        strategy="cloud_saas",
        description="Google's multimodal AI models.",
        documentation_url="https://ai.google.dev/docs",
        license="SaaS",
        requires_credentials=["google_api_key"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    )
}


# ============================================================================
# ALL PROVIDERS
# ============================================================================

ALL_PROVIDERS = {
    "search": SEARCH_PROVIDERS,
    "chat": CHAT_PROVIDERS,
    "notification": NOTIFICATION_PROVIDERS,
    "analytics": ANALYTICS_PROVIDERS,
    "signature": SIGNATURE_PROVIDERS,
    "integration": INTEGRATION_PROVIDERS,
    "ai": AI_PROVIDERS
}


def get_provider_info(category: str, provider_name: str) -> Optional[ProviderInfo]:
    """Get provider information by category and name."""
    category_providers = ALL_PROVIDERS.get(category, {})
    return category_providers.get(provider_name)


def get_providers_by_strategy(category: str, strategy: str) -> Dict[str, ProviderInfo]:
    """Get all providers in a category matching a strategy."""
    category_providers = ALL_PROVIDERS.get(category, {})
    return {
        name: info for name, info in category_providers.items()
        if info.strategy == strategy
    }


def get_required_containers(providers: Dict[str, str]) -> List[str]:
    """Get all required containers for selected providers."""
    containers = []
    for category, provider_name in providers.items():
        info = get_provider_info(category, provider_name)
        if info:
            containers.extend(info.requires_containers)
    return list(set(containers))


def get_required_credentials(providers: Dict[str, str]) -> List[str]:
    """Get all required credentials for selected providers."""
    credentials = []
    for category, provider_name in providers.items():
        info = get_provider_info(category, provider_name)
        if info:
            credentials.extend(info.requires_credentials)
    return list(set(credentials))


# ============================================================================
# CONTAINER CONFIGURATIONS
# ============================================================================

@dataclass
class ContainerConfig:
    """Container configuration for a service."""
    name: str
    image: str
    tag: str = "latest"
    ports: List[Dict[str, int]] = field(default_factory=list)
    environment: Dict[str, str] = field(default_factory=dict)
    volumes: List[str] = field(default_factory=list)
    depends_on: List[str] = field(default_factory=list)
    cpu: str = "0.5"
    memory: str = "512Mi"
    replicas: int = 1
    health_check: Optional[Dict[str, Any]] = None


CONTAINER_CONFIGS = {
    "meilisearch": ContainerConfig(
        name="meilisearch",
        image="getmeili/meilisearch",
        tag="v1.6",
        ports=[{"container": 7700, "host": 7700}],
        environment={"MEILI_MASTER_KEY": "${MEILISEARCH_API_KEY}"},
        volumes=["meilisearch_data:/meili_data"],
        cpu="0.5",
        memory="1Gi",
        health_check={"endpoint": "/health", "port": 7700}
    ),
    "chatwoot": ContainerConfig(
        name="chatwoot",
        image="chatwoot/chatwoot",
        tag="v3.5.0",
        ports=[{"container": 3000, "host": 3100}],
        environment={
            "RAILS_ENV": "production",
            "SECRET_KEY_BASE": "${CHATWOOT_SECRET}",
            "POSTGRES_HOST": "chatwoot-postgres",
            "REDIS_URL": "redis://chatwoot-redis:6379"
        },
        volumes=["chatwoot_storage:/app/storage"],
        depends_on=["chatwoot-postgres", "chatwoot-redis"],
        cpu="1",
        memory="2Gi"
    ),
    "novu-api": ContainerConfig(
        name="novu-api",
        image="ghcr.io/novuhq/novu/api",
        tag="0.24.0",
        ports=[{"container": 3000, "host": 3200}],
        environment={
            "NODE_ENV": "production",
            "MONGO_URL": "mongodb://novu-mongodb:27017/novu",
            "REDIS_HOST": "novu-redis"
        },
        depends_on=["novu-mongodb", "novu-redis"],
        cpu="0.5",
        memory="1Gi"
    ),
    "superset": ContainerConfig(
        name="superset",
        image="apache/superset",
        tag="3.1.0",
        ports=[{"container": 8088, "host": 8088}],
        environment={
            "SUPERSET_SECRET_KEY": "${SUPERSET_SECRET}"
        },
        volumes=["superset_home:/app/superset_home"],
        depends_on=["superset-postgres", "superset-redis"],
        cpu="1",
        memory="2Gi"
    ),
    "docuseal": ContainerConfig(
        name="docuseal",
        image="docuseal/docuseal",
        tag="1.5.0",
        ports=[{"container": 3000, "host": 3300}],
        environment={
            "DATABASE_URL": "postgresql://docuseal:${DOCUSEAL_DB_PASSWORD}@docuseal-postgres/docuseal"
        },
        volumes=["docuseal_data:/data"],
        depends_on=["docuseal-postgres"],
        cpu="0.5",
        memory="1Gi"
    ),
    "n8n": ContainerConfig(
        name="n8n",
        image="n8nio/n8n",
        tag="1.25.0",
        ports=[{"container": 5678, "host": 5678}],
        environment={
            "N8N_BASIC_AUTH_ACTIVE": "true",
            "N8N_BASIC_AUTH_USER": "admin"
        },
        volumes=["n8n_data:/home/node/.n8n"],
        cpu="0.5",
        memory="1Gi"
    ),
    "ollama": ContainerConfig(
        name="ollama",
        image="ollama/ollama",
        tag="latest",
        ports=[{"container": 11434, "host": 11434}],
        volumes=["ollama_models:/root/.ollama"],
        cpu="2",
        memory="8Gi"  # LLMs need more memory
    ),
    "metabase": ContainerConfig(
        name="metabase",
        image="metabase/metabase",
        tag="latest",
        ports=[{"container": 3000, "host": 3400}],
        environment={
            "MB_DB_TYPE": "postgres",
            "MB_DB_HOST": "metabase-postgres"
        },
        depends_on=["metabase-postgres"],
        cpu="0.5",
        memory="1Gi"
    )
}


def get_container_config(container_name: str) -> Optional[ContainerConfig]:
    """Get container configuration by name."""
    return CONTAINER_CONFIGS.get(container_name)
