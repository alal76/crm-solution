# Operator Deployment Guide

## CRM Solution with Pluggable Architecture

This guide provides comprehensive instructions for deploying the CRM solution with its pluggable provider architecture.

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Deployment Options](#deployment-options)
4. [Configuration](#configuration)
5. [Provider Setup](#provider-setup)
6. [Monitoring & Health Checks](#monitoring--health-checks)
7. [Scaling](#scaling)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The CRM solution uses a **pluggable architecture** that allows operators to choose between:

- **BuiltIn Providers**: Default implementations included in the application
- **External OSS Providers**: Self-hosted open-source alternatives (Meilisearch, Chatwoot, etc.)
- **Cloud SaaS Providers**: Managed cloud services (Algolia, Intercom, DocuSign, etc.)

### Provider Categories

| Category | BuiltIn | OSS Options | SaaS Options |
|----------|---------|-------------|--------------|
| **Search** | SQL-based | Meilisearch | Algolia |
| **Notifications** | SMTP Email | Novu | Twilio, SendGrid |
| **Chat** | Basic | Chatwoot | Intercom |
| **E-Signature** | Manual | DocuSeal | DocuSign |
| **Analytics** | Basic SQL | Superset | Power BI |
| **Integration** | Webhooks | n8n | Zapier |
| **AI/LLM** | - | Ollama | Azure OpenAI, Bedrock |

---

## Prerequisites

### System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **CPU** | 2 cores | 4+ cores |
| **RAM** | 4 GB | 8+ GB |
| **Storage** | 20 GB | 50+ GB SSD |
| **.NET Runtime** | 8.0 | 8.0 |
| **Database** | MariaDB 10.6 | MariaDB 11.x |

### Software Requirements

```bash
# Required
dotnet --version  # >= 8.0
docker --version  # >= 24.0
docker-compose --version  # >= 2.20

# Optional (for Kubernetes)
kubectl version
helm version
```

---

## Deployment Options

### Option 1: Docker Compose (Recommended for Development/Small Deployments)

```bash
# Clone repository
git clone <repository-url>
cd crm-solution

# Start with BuiltIn providers only
docker-compose -f docker/docker-compose.yml up -d

# Or with OSS providers
docker-compose -f docker/docker-compose.yml \
               -f docker/docker-compose.providers.yml up -d
```

### Option 2: Kubernetes (Production)

```bash
# Create namespace
kubectl create namespace crm

# Deploy with Helm
helm install crm ./kubernetes/helm/crm \
  --namespace crm \
  --values ./kubernetes/helm/crm/values-production.yaml
```

### Option 3: Azure Container Apps

See [Azure Deployment Guide](azure/AZURE_DEPLOYMENT.md)

---

## Configuration

### Feature Flags

Provider selection is controlled via feature flags in `appsettings.json`:

```json
{
  "FeatureManagement": {
    "UseExternalSearch": false,
    "UseExternalChat": false,
    "UseExternalNotifications": false,
    "UseExternalAnalytics": false,
    "UseExternalSignatures": false,
    "UseExternalAI": true
  }
}
```

### Environment Variables

All settings can be overridden via environment variables:

```bash
# Feature flags
FeatureManagement__UseExternalSearch=true
FeatureManagement__UseExternalChat=true

# Provider configuration
Providers__Search__Type=Meilisearch
Providers__Search__Meilisearch__Url=http://meilisearch:7700
Providers__Search__Meilisearch__ApiKey=your-api-key

Providers__Chat__Type=Chatwoot
Providers__Chat__Chatwoot__BaseUrl=http://chatwoot:3000
Providers__Chat__Chatwoot__ApiKey=your-api-key
```

### Configuration Hierarchy

1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables (highest priority)
4. User secrets (development only)

---

## Provider Setup

### Search Providers

#### BuiltIn (Default)
No additional setup required. Uses SQL LIKE queries.

#### Meilisearch
```bash
# Docker
docker run -d --name meilisearch \
  -p 7700:7700 \
  -e MEILI_MASTER_KEY=your-master-key \
  getmeili/meilisearch:v1.6

# Configure CRM
Providers__Search__Type=Meilisearch
Providers__Search__Meilisearch__Url=http://localhost:7700
Providers__Search__Meilisearch__ApiKey=your-master-key
```

#### Algolia
```bash
# No self-hosting - Cloud SaaS only
Providers__Search__Type=Algolia
Providers__Search__Algolia__ApplicationId=your-app-id
Providers__Search__Algolia__ApiKey=your-admin-api-key
Providers__Search__Algolia__IndexPrefix=crm_prod_
```

### Notification Providers

#### BuiltIn (SMTP)
```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "crm@example.com",
    "Password": "your-password",
    "FromAddress": "crm@example.com",
    "FromName": "CRM System"
  }
}
```

#### Novu (Self-Hosted)
```bash
# Deploy Novu
docker-compose -f docker/docker-compose.providers.yml up -d novu

# Configure CRM
Providers__Notifications__Type=Novu
Providers__Notifications__Novu__ApiKey=your-novu-api-key
Providers__Notifications__Novu__AppId=your-app-id
```

#### Twilio/SendGrid (Cloud SaaS)
```bash
Providers__Notifications__Type=Twilio
Providers__Notifications__Twilio__AccountSid=ACxxxxxxxxxx
Providers__Notifications__Twilio__AuthToken=your-auth-token
Providers__Notifications__Twilio__FromPhone=+1234567890
```

### Chat Providers

#### Chatwoot (Self-Hosted)
```bash
# Deploy Chatwoot
docker-compose -f docker/docker-compose.providers.yml up -d chatwoot

# Configure CRM
Providers__Chat__Type=Chatwoot
Providers__Chat__Chatwoot__BaseUrl=http://chatwoot:3000
Providers__Chat__Chatwoot__ApiKey=your-api-key
Providers__Chat__Chatwoot__AccountId=1
```

#### Intercom (Cloud SaaS)
```bash
Providers__Chat__Type=Intercom
Providers__Chat__Intercom__AppId=your-app-id
Providers__Chat__Intercom__AccessToken=your-access-token
```

### E-Signature Providers

#### DocuSeal (Self-Hosted)
```bash
# Deploy DocuSeal
docker-compose -f docker/docker-compose.providers.yml up -d docuseal

# Configure CRM
Providers__Signatures__Type=DocuSeal
Providers__Signatures__DocuSeal__ApiUrl=http://docuseal:3000
Providers__Signatures__DocuSeal__ApiKey=your-api-key
```

#### DocuSign (Cloud SaaS)
```bash
Providers__Signatures__Type=DocuSign
Providers__Signatures__DocuSign__IntegrationKey=your-integration-key
Providers__Signatures__DocuSign__UserId=your-user-guid
Providers__Signatures__DocuSign__AccountId=your-account-id
# For JWT auth, provide RSA private key path
Providers__Signatures__DocuSign__PrivateKeyPath=/secrets/docusign-private.pem
```

### Analytics Providers

#### Superset (Self-Hosted)
```bash
# Deploy Superset
docker-compose -f docker/docker-compose.providers.yml up -d superset

# Configure CRM
Providers__Analytics__Type=Superset
Providers__Analytics__Superset__Url=http://superset:8088
Providers__Analytics__Superset__Username=admin
Providers__Analytics__Superset__Password=your-password
```

#### Power BI (Cloud SaaS)
```bash
Providers__Analytics__Type=PowerBI
Providers__Analytics__PowerBI__TenantId=your-tenant-id
Providers__Analytics__PowerBI__ClientId=your-client-id
Providers__Analytics__PowerBI__ClientSecret=your-client-secret
Providers__Analytics__PowerBI__WorkspaceId=your-workspace-id
```

### AI/LLM Providers

#### Ollama (Self-Hosted)
```bash
# Deploy Ollama
docker-compose -f docker/docker-compose.ollama.yml up -d

# Configure CRM
Providers__AI__Type=Ollama
Providers__AI__Ollama__Url=http://ollama:11434
Providers__AI__Ollama__Model=llama3
```

#### Azure OpenAI (Cloud SaaS)
```bash
Providers__AI__Type=AzureOpenAI
Providers__AI__AzureOpenAI__Endpoint=https://your-resource.openai.azure.com/
Providers__AI__AzureOpenAI__ApiKey=your-api-key
Providers__AI__AzureOpenAI__DeploymentName=gpt-4
```

#### AWS Bedrock
```bash
Providers__AI__Type=Bedrock
Providers__AI__Bedrock__Region=us-east-1
Providers__AI__Bedrock__ModelId=anthropic.claude-3-sonnet-20240229-v1:0
# Uses AWS credentials from environment or IAM role
```

---

## Monitoring & Health Checks

### Health Check Endpoints

```bash
# Overall health
GET /api/health

# Provider-specific health
GET /api/health/providers

# Feature flag status
GET /api/admin/features
```

### Example Health Response

```json
{
  "status": "Healthy",
  "providers": {
    "Search": {
      "provider": "Meilisearch",
      "status": "Healthy",
      "responseTime": "12ms"
    },
    "Chat": {
      "provider": "Chatwoot",
      "status": "Healthy",
      "responseTime": "45ms"
    },
    "Notifications": {
      "provider": "BuiltIn",
      "status": "Healthy"
    }
  }
}
```

### Metrics

The CRM exposes Prometheus-compatible metrics:

```bash
# Metrics endpoint
GET /metrics

# Key metrics
crm_provider_requests_total{provider="Meilisearch",operation="search"}
crm_provider_errors_total{provider="Chatwoot",error_type="timeout"}
crm_provider_latency_seconds{provider="DocuSign",quantile="0.99"}
```

---

## Scaling

### Horizontal Scaling

The CRM API is stateless and can be scaled horizontally:

```yaml
# Kubernetes
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-api
spec:
  replicas: 3  # Scale as needed
  ...
```

### Provider Scaling Considerations

| Provider | Scaling Notes |
|----------|---------------|
| **Meilisearch** | Single-node; use Meilisearch Cloud for HA |
| **Chatwoot** | Redis required for multi-instance |
| **Novu** | Requires MongoDB replica set for HA |
| **Superset** | Celery workers for background tasks |

---

## Troubleshooting

### Common Issues

#### Provider Connection Failures

```bash
# Check provider health
curl http://localhost:5000/api/health/providers

# Check network connectivity
docker exec crm-api ping meilisearch

# View logs
docker logs crm-api --tail 100 -f
```

#### Feature Flag Not Taking Effect

```bash
# Verify configuration
docker exec crm-api env | grep FeatureManagement

# Check active providers via API
curl http://localhost:5000/api/admin/features
```

#### Search Index Out of Sync

```bash
# Trigger reindex
curl -X POST http://localhost:5000/api/admin/search/reindex \
  -H "Authorization: Bearer $TOKEN"
```

### Logs

```bash
# Docker
docker logs crm-api --tail 100 -f

# Kubernetes
kubectl logs -f deployment/crm-api -n crm

# Filter by level
docker logs crm-api 2>&1 | grep -E "(Error|Warning)"
```

### Debug Mode

Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CRM.Infrastructure.Providers": "Debug"
    }
  }
}
```

---

## Security Considerations

### API Keys

- Store all API keys in environment variables or secret management
- Never commit API keys to source control
- Rotate keys regularly

### Network Security

- Place providers behind internal network
- Use TLS for all external communications
- Configure firewalls to restrict access

### Webhook Security

- Validate webhook signatures (Chatwoot, DocuSign, etc.)
- Use HTTPS for webhook endpoints
- Implement rate limiting

---

## Support

For issues:
1. Check the [Troubleshooting Runbook](TROUBLESHOOTING_RUNBOOK.md)
2. Review [Provider Configuration Reference](PROVIDER_CONFIGURATION_REFERENCE.md)
3. Open an issue in the repository

---

**Last Updated:** 2024-02-05  
**Version:** 1.0.0
