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
# MONITORING PROVIDERS
# ============================================================================

MONITORING_PROVIDERS = {
    "prometheus_grafana": ProviderInfo(
        name="prometheus_grafana",
        display_name="Prometheus + Grafana + Loki",
        category="monitoring",
        strategy="opensource",
        description="Self-hosted observability stack. Prometheus for metrics, Grafana for dashboards, Loki for logs.",
        documentation_url="https://grafana.com/docs/",
        license="Apache 2.0",
        requires_credentials=[],
        requires_containers=["prometheus", "grafana", "loki"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Medium"
    ),
    "uptime_kuma": ProviderInfo(
        name="uptime_kuma",
        display_name="Uptime Kuma",
        category="monitoring",
        strategy="opensource",
        description="Self-hosted uptime monitoring with a beautiful UI and alerting.",
        documentation_url="https://github.com/louislam/uptime-kuma",
        license="MIT",
        requires_credentials=[],
        requires_containers=["uptime-kuma"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "datadog": ProviderInfo(
        name="datadog",
        display_name="Datadog",
        category="monitoring",
        strategy="cloud_saas",
        description="Full-stack observability platform — APM, metrics, logs, synthetics.",
        documentation_url="https://docs.datadoghq.com/",
        license="SaaS",
        requires_credentials=["datadog_api_key", "datadog_app_key"],
        requires_containers=[],
        estimated_monthly_cost="~$15/host/mo",
        setup_complexity="Low"
    ),
    "azure_monitor": ProviderInfo(
        name="azure_monitor",
        display_name="Azure Monitor + Application Insights",
        category="monitoring",
        strategy="cloud_saas",
        description="Native Azure observability — logs, metrics, alerts, and Application Insights APM.",
        documentation_url="https://learn.microsoft.com/azure/azure-monitor/",
        license="SaaS",
        requires_credentials=["azure_appinsights_connection_string"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    ),
    "cloudwatch": ProviderInfo(
        name="cloudwatch",
        display_name="AWS CloudWatch",
        category="monitoring",
        strategy="cloud_saas",
        description="AWS-native monitoring and observability service — metrics, logs, alarms.",
        documentation_url="https://docs.aws.amazon.com/cloudwatch/",
        license="SaaS",
        requires_credentials=["aws_access_key", "aws_secret_key", "aws_region"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    ),
    "cloud_monitoring": ProviderInfo(
        name="cloud_monitoring",
        display_name="Google Cloud Monitoring (Stackdriver)",
        category="monitoring",
        strategy="cloud_saas",
        description="GCP-native monitoring, logging, and alerting via Cloud Monitoring and Logging APIs.",
        documentation_url="https://cloud.google.com/monitoring/docs",
        license="SaaS",
        requires_credentials=["gcp_project_id", "google_credentials_json"],
        requires_containers=[],
        estimated_monthly_cost="Usage-based",
        setup_complexity="Low"
    ),
    "newrelic": ProviderInfo(
        name="newrelic",
        display_name="New Relic",
        category="monitoring",
        strategy="cloud_saas",
        description="Full-stack observability platform with APM, infrastructure, browser, and synthetics.",
        documentation_url="https://docs.newrelic.com/",
        license="SaaS",
        requires_credentials=["newrelic_license_key"],
        requires_containers=[],
        estimated_monthly_cost="Free tier + usage",
        setup_complexity="Low"
    ),
}


# ============================================================================
# PORTAINER / CONTAINER MANAGEMENT PROVIDERS
# ============================================================================

PORTAINER_PROVIDERS = {
    "portainer_ce": ProviderInfo(
        name="portainer_ce",
        display_name="Portainer Community Edition",
        category="portainer",
        strategy="opensource",
        description="Self-hosted Docker / Kubernetes management UI. Visualise, manage, and troubleshoot containers.",
        documentation_url="https://docs.portainer.io/",
        license="zlib",
        requires_credentials=[],
        requires_containers=["portainer"],
        estimated_monthly_cost="Free",
        setup_complexity="Low"
    ),
    "portainer_be": ProviderInfo(
        name="portainer_be",
        display_name="Portainer Business Edition",
        category="portainer",
        strategy="cloud_saas",
        description="Commercial Portainer with RBAC, LDAP/AD, and multi-cluster features.",
        documentation_url="https://docs.portainer.io/",
        license="Commercial",
        requires_credentials=["portainer_license_key"],
        requires_containers=["portainer"],
        estimated_monthly_cost="Contact vendor",
        setup_complexity="Low"
    ),
    "kubernetes_dashboard": ProviderInfo(
        name="kubernetes_dashboard",
        display_name="Kubernetes Dashboard",
        category="portainer",
        strategy="opensource",
        description="Official Kubernetes web UI for managing cluster resources.",
        documentation_url="https://kubernetes.io/docs/tasks/access-application-cluster/web-ui-dashboard/",
        license="Apache 2.0",
        requires_credentials=[],
        requires_containers=[],
        estimated_monthly_cost="Free",
        setup_complexity="Medium"
    ),
}


# ============================================================================
# STORAGE PROVIDERS
# ============================================================================

STORAGE_PROVIDERS = {
    "minio": ProviderInfo(
        name="minio",
        display_name="MinIO",
        category="storage",
        strategy="opensource",
        description="High-performance, S3-compatible object storage. Self-hosted.",
        documentation_url="https://min.io/docs/minio/",
        license="AGPL-3.0",
        requires_credentials=["minio_access_key", "minio_secret_key"],
        requires_containers=["minio"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "azure_blob": ProviderInfo(
        name="azure_blob",
        display_name="Azure Blob Storage",
        category="storage",
        strategy="cloud_saas",
        description="Massively scalable object storage for unstructured data on Azure.",
        documentation_url="https://learn.microsoft.com/azure/storage/blobs/",
        license="SaaS",
        requires_credentials=["azure_storage_account", "azure_storage_key"],
        requires_containers=[],
        estimated_monthly_cost="~$0.018/GB/mo",
        setup_complexity="Low"
    ),
    "aws_s3": ProviderInfo(
        name="aws_s3",
        display_name="Amazon S3",
        category="storage",
        strategy="cloud_saas",
        description="Scalable object storage on AWS. Industry standard S3 API.",
        documentation_url="https://docs.aws.amazon.com/s3/",
        license="SaaS",
        requires_credentials=["aws_access_key", "aws_secret_key", "aws_region", "s3_bucket_name"],
        requires_containers=[],
        estimated_monthly_cost="~$0.023/GB/mo",
        setup_complexity="Low"
    ),
    "gcs": ProviderInfo(
        name="gcs",
        display_name="Google Cloud Storage",
        category="storage",
        strategy="cloud_saas",
        description="Unified object storage on GCP for any amount of data.",
        documentation_url="https://cloud.google.com/storage/docs",
        license="SaaS",
        requires_credentials=["gcp_project_id", "google_credentials_json", "gcs_bucket_name"],
        requires_containers=[],
        estimated_monthly_cost="~$0.020/GB/mo",
        setup_complexity="Low"
    ),
}


# ============================================================================
# REVERSE PROXY PROVIDERS
# ============================================================================

REVERSE_PROXY_PROVIDERS = {
    "traefik": ProviderInfo(
        name="traefik",
        display_name="Traefik",
        category="reverse_proxy",
        strategy="opensource",
        description="Cloud-native reverse proxy and load balancer with automatic SSL via Let's Encrypt.",
        documentation_url="https://doc.traefik.io/traefik/",
        license="MIT",
        requires_credentials=[],
        requires_containers=["traefik"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "nginx": ProviderInfo(
        name="nginx",
        display_name="Nginx",
        category="reverse_proxy",
        strategy="opensource",
        description="High-performance web server, reverse proxy, and load balancer.",
        documentation_url="https://nginx.org/en/docs/",
        license="BSD-like",
        requires_credentials=[],
        requires_containers=["nginx"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
    "caddy": ProviderInfo(
        name="caddy",
        display_name="Caddy",
        category="reverse_proxy",
        strategy="opensource",
        description="Modern web server with automatic HTTPS and easy Caddyfile configuration.",
        documentation_url="https://caddyserver.com/docs/",
        license="Apache 2.0",
        requires_credentials=[],
        requires_containers=["caddy"],
        estimated_monthly_cost="Free (self-hosted)",
        setup_complexity="Low"
    ),
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
    "ai": AI_PROVIDERS,
    "monitoring": MONITORING_PROVIDERS,
    "portainer": PORTAINER_PROVIDERS,
    "storage": STORAGE_PROVIDERS,
    "reverse_proxy": REVERSE_PROXY_PROVIDERS,
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
    ),
    # ── Infrastructure / Monitoring containers ──────────────────
    "portainer": ContainerConfig(
        name="portainer",
        image="portainer/portainer-ce",
        tag="latest",
        ports=[{"container": 9443, "host": 9443}, {"container": 9000, "host": 9000}],
        volumes=["portainer_data:/data", "/var/run/docker.sock:/var/run/docker.sock"],
        cpu="0.25",
        memory="256Mi",
        health_check={"endpoint": "/api/status", "port": 9000}
    ),
    "prometheus": ContainerConfig(
        name="prometheus",
        image="prom/prometheus",
        tag="latest",
        ports=[{"container": 9090, "host": 9090}],
        volumes=["prometheus_data:/prometheus", "./prometheus.yml:/etc/prometheus/prometheus.yml:ro"],
        cpu="0.5",
        memory="512Mi",
        health_check={"endpoint": "/-/healthy", "port": 9090}
    ),
    "grafana": ContainerConfig(
        name="grafana",
        image="grafana/grafana",
        tag="latest",
        ports=[{"container": 3000, "host": 3010}],
        environment={
            "GF_SECURITY_ADMIN_PASSWORD": "${GRAFANA_ADMIN_PASSWORD}",
            "GF_USERS_ALLOW_SIGN_UP": "false"
        },
        volumes=["grafana_data:/var/lib/grafana"],
        depends_on=["prometheus"],
        cpu="0.5",
        memory="512Mi",
        health_check={"endpoint": "/api/health", "port": 3010}
    ),
    "loki": ContainerConfig(
        name="loki",
        image="grafana/loki",
        tag="latest",
        ports=[{"container": 3100, "host": 3100}],
        volumes=["loki_data:/loki"],
        cpu="0.5",
        memory="512Mi"
    ),
    "uptime-kuma": ContainerConfig(
        name="uptime-kuma",
        image="louislam/uptime-kuma",
        tag="1",
        ports=[{"container": 3001, "host": 3001}],
        volumes=["uptime-kuma_data:/app/data"],
        cpu="0.25",
        memory="256Mi",
        health_check={"endpoint": "/", "port": 3001}
    ),
    "traefik": ContainerConfig(
        name="traefik",
        image="traefik",
        tag="v3.0",
        ports=[
            {"container": 80, "host": 80},
            {"container": 443, "host": 443},
            {"container": 8080, "host": 8080}
        ],
        volumes=[
            "/var/run/docker.sock:/var/run/docker.sock:ro",
            "traefik_certs:/letsencrypt"
        ],
        cpu="0.25",
        memory="128Mi",
        health_check={"endpoint": "/ping", "port": 8080}
    ),
    "minio": ContainerConfig(
        name="minio",
        image="minio/minio",
        tag="latest",
        ports=[
            {"container": 9002, "host": 9002},
            {"container": 9003, "host": 9003}
        ],
        environment={
            "MINIO_ROOT_USER": "${MINIO_ACCESS_KEY}",
            "MINIO_ROOT_PASSWORD": "${MINIO_SECRET_KEY}"
        },
        volumes=["minio_data:/data"],
        cpu="0.5",
        memory="512Mi",
        health_check={"endpoint": "/minio/health/live", "port": 9002}
    ),
}


def get_container_config(container_name: str) -> Optional[ContainerConfig]:
    """Get container configuration by name."""
    return CONTAINER_CONFIGS.get(container_name)


# ============================================================================
# PLATFORM-AWARE PROVIDER DEFAULTS
# ============================================================================

def get_default_providers(platform: str) -> dict:
    """Return recommended provider selection based on deployment platform.

    On-premises / Docker Compose → all open-source self-hosted providers.
    Azure → Azure-native cloud services where available.
    AWS   → AWS-native services where available.
    GCP   → GCP-native services where available.

    Args:
        platform: One of "on_premises", "hybrid", "azure", "aws", "gcp".

    Returns:
        Dict mapping category name → provider short-key.
    """
    _ONPREM = {
        "search": "meilisearch",
        "chat": "chatwoot",
        "notification": "novu",
        "analytics": "metabase",
        "signature": "docuseal",
        "integration": "n8n",
        "ai": "ollama",
        "monitoring": "prometheus_grafana",
        "portainer": "portainer_ce",
        "storage": "minio",
        "reverse_proxy": "traefik",
    }

    _AZURE = {
        "search": "azure_cognitive_search",
        "chat": "chatwoot",             # no dominant Azure-native chat SaaS
        "notification": "sendgrid",
        "analytics": "powerbi",
        "signature": "docusign",
        "integration": "n8n",           # Azure Logic Apps alternative via n8n
        "ai": "azure_openai",
        "monitoring": "azure_monitor",
        "portainer": "portainer_ce",    # still useful for container visibility
        "storage": "azure_blob",
        "reverse_proxy": "traefik",
    }

    _AWS = {
        "search": "elasticsearch",
        "chat": "chatwoot",
        "notification": "sendgrid",
        "analytics": "metabase",        # QuickSight not in provider catalogue yet
        "signature": "docusign",
        "integration": "n8n",
        "ai": "bedrock",
        "monitoring": "cloudwatch",
        "portainer": "portainer_ce",
        "storage": "aws_s3",
        "reverse_proxy": "traefik",
    }

    _GCP = {
        "search": "elasticsearch",
        "chat": "chatwoot",
        "notification": "sendgrid",
        "analytics": "metabase",
        "signature": "docusign",
        "integration": "n8n",
        "ai": "gemini",
        "monitoring": "cloud_monitoring",
        "portainer": "portainer_ce",
        "storage": "gcs",
        "reverse_proxy": "traefik",
    }

    _MAP = {
        "on_premises": _ONPREM,
        "hybrid": _ONPREM,
        "azure": _AZURE,
        "aws": _AWS,
        "gcp": _GCP,
    }

    return dict(_MAP.get(platform, _ONPREM))
