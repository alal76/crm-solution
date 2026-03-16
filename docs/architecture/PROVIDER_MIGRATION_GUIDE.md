# Provider Migration Guide

This guide helps operators migrate existing CRM deployments to use the new pluggable provider architecture.

## Overview

The CRM solution now supports a pluggable architecture where external services (search, chat, notifications, etc.) can be swapped at deployment time without code changes. This guide covers:

1. [Understanding the Changes](#understanding-the-changes)
2. [Configuration Migration](#configuration-migration)
3. [Deployment Updates](#deployment-updates)
4. [Enabling External Providers](#enabling-external-providers)
5. [Health Monitoring](#health-monitoring)
6. [Rollback Procedures](#rollback-procedures)

---

## Understanding the Changes

### What Changed

| Component | Before | After |
|-----------|--------|-------|
| Search | Hardcoded SQL LIKE | Pluggable via `ISearchPort` |
| Notifications | Direct SMTP | Pluggable via `INotificationPort` |
| Analytics | Built-in reports | Pluggable via `IAnalyticsPort` |
| Chat | N/A | New pluggable `IChatPort` |
| E-Signatures | N/A | New pluggable `ISignaturePort` |
| AI/LLM | Multi-provider | Unified via `IAIPort` |

### Backward Compatibility

**The default configuration is fully backward compatible.** Without any configuration changes:

- All providers default to "BuiltIn" implementations
- Existing functionality works exactly as before
- No database schema changes required
- No frontend changes required

---

## Configuration Migration

### Step 1: Add Feature Management Section

Add the following to your `appsettings.json`:

```json
{
  "FeatureManagement": {
    "UseExternalSearch": false,
    "UseExternalChat": false,
    "UseExternalNotifications": false,
    "UseExternalAnalytics": false,
    "UseExternalSignatures": false,
    "UseExternalAI": false,
    "UseExternalIntegrations": false,
    "EnableITSM": true,
    "EnableMarketing": true,
    "EnableCustomerPortal": false,
    "EnablePartnerPortal": false
  }
}
```

### Step 2: Add Providers Section (Optional)

If you plan to enable external providers, add their configuration:

```json
{
  "Providers": {
    "Search": {
      "Type": "BuiltIn",
      "Meilisearch": {
        "Url": "http://meilisearch:7700",
        "ApiKey": "${MEILISEARCH_API_KEY}"
      }
    },
    "Chat": {
      "Type": "BuiltIn",
      "Chatwoot": {
        "BaseUrl": "http://chatwoot:3000",
        "ApiKey": "${CHATWOOT_API_KEY}",
        "AccountId": "1"
      }
    },
    "Notifications": {
      "Type": "BuiltIn",
      "Novu": {
        "ApiKey": "${NOVU_API_KEY}",
        "ApplicationId": "${NOVU_APP_ID}"
      }
    },
    "Analytics": {
      "Type": "BuiltIn",
      "Superset": {
        "Url": "http://superset:8088",
        "Username": "admin",
        "Password": "${SUPERSET_PASSWORD}"
      }
    },
    "AI": {
      "Type": "Ollama",
      "Ollama": {
        "Url": "http://ollama:11434",
        "Model": "llama3"
      }
    }
  }
}
```

---

## Deployment Updates

### Docker Compose

Add environment variables for feature flags:

```yaml
services:
  crm-api:
    environment:
      # Feature Flags (all disabled by default)
      - FeatureManagement__UseExternalSearch=false
      - FeatureManagement__UseExternalChat=false
      - FeatureManagement__UseExternalNotifications=false
      - FeatureManagement__UseExternalAnalytics=false
      
      # Provider Types (used when external is enabled)
      - Providers__Search__Type=Meilisearch
      - Providers__Chat__Type=Chatwoot
      - Providers__Notifications__Type=Novu
```

### Kubernetes

Update your ConfigMap:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: crm-config
data:
  FeatureManagement__UseExternalSearch: "false"
  FeatureManagement__UseExternalChat: "false"
  FeatureManagement__UseExternalNotifications: "false"
  FeatureManagement__UseExternalAnalytics: "false"
  Providers__Search__Type: "BuiltIn"
  Providers__Chat__Type: "BuiltIn"
```

Add Secrets for provider credentials:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: crm-provider-secrets
type: Opaque
stringData:
  MEILISEARCH_API_KEY: "your-api-key"
  CHATWOOT_API_KEY: "your-api-key"
  NOVU_API_KEY: "your-api-key"
```

---

## Enabling External Providers

### Step 1: Deploy the External Service

Example for Meilisearch:

```yaml
# Add to docker-compose.yml
meilisearch:
  image: getmeili/meilisearch:v1.6
  ports:
    - "7700:7700"
  environment:
    - MEILI_MASTER_KEY=${MEILISEARCH_API_KEY}
  volumes:
    - meilisearch_data:/meili_data
```

### Step 2: Configure Provider Connection

```yaml
crm-api:
  environment:
    - Providers__Search__Meilisearch__Url=http://meilisearch:7700
    - Providers__Search__Meilisearch__ApiKey=${MEILISEARCH_API_KEY}
```

### Step 3: Enable the External Provider

```yaml
crm-api:
  environment:
    - FeatureManagement__UseExternalSearch=true
    - Providers__Search__Type=Meilisearch
```

### Step 4: Restart the CRM API

```bash
docker-compose up -d crm-api
```

### Step 5: Verify Health

```bash
curl http://localhost:5000/api/health/providers
```

Expected response:
```json
{
  "timestamp": "2026-02-04T12:00:00Z",
  "overallHealthy": true,
  "providers": {
    "Search": {
      "activeProvider": "Meilisearch",
      "isHealthy": true
    }
  }
}
```

---

## Health Monitoring

### Health Check Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /api/health/providers` | Overall provider health |
| `GET /api/health/providers/{category}` | Category-specific health |
| `GET /api/health/providers/registry` | Detailed registry (Admin only) |

### Monitoring Integration

Add health checks to your monitoring system:

```bash
# Prometheus scrape config
- job_name: 'crm-providers'
  metrics_path: '/api/health/providers'
  static_configs:
    - targets: ['crm-api:5000']
```

### Alerting

Set up alerts for provider failures:

```yaml
# Example Alertmanager rule
- alert: CRMProviderUnhealthy
  expr: crm_provider_health == 0
  for: 5m
  labels:
    severity: critical
  annotations:
    summary: "CRM Provider {{ $labels.provider }} is unhealthy"
```

---

## Rollback Procedures

### Quick Rollback

To immediately disable an external provider and fall back to BuiltIn:

1. **Update configuration:**
   ```bash
   # Docker
   docker-compose exec crm-api \
     env FeatureManagement__UseExternalSearch=false
   
   # Or update docker-compose.yml and restart
   ```

2. **Restart the service:**
   ```bash
   docker-compose restart crm-api
   ```

### Graceful Rollback

For production environments:

1. **Update ConfigMap/Environment:**
   ```yaml
   FeatureManagement__UseExternalSearch: "false"
   ```

2. **Rolling restart (Kubernetes):**
   ```bash
   kubectl rollout restart deployment/crm-api
   ```

3. **Verify health:**
   ```bash
   kubectl exec -it deployment/crm-api -- curl localhost:5000/api/health/providers
   ```

### Data Considerations

| Provider | Rollback Data Impact |
|----------|---------------------|
| Search | Indexes need rebuilding if switching back to Meilisearch |
| Chat | Conversation history remains in Chatwoot |
| Notifications | Delivery history remains in Novu/Twilio |
| Analytics | Dashboards remain in Superset |
| Signatures | Signed documents remain in DocuSeal |

---

## Troubleshooting

### Provider Not Resolving

**Symptom:** `InvalidOperationException: Unable to resolve service for type 'ISearchPort'`

**Cause:** Provider factory can't find the configured provider.

**Solution:**
1. Verify the provider type is spelled correctly
2. Check that the provider's configuration section exists
3. Ensure all required environment variables are set

### External Provider Unhealthy

**Symptom:** Health check shows provider as unhealthy

**Diagnosis:**
```bash
curl http://localhost:5000/api/health/providers/Search
```

**Common Causes:**
1. External service is not running
2. Network connectivity issues
3. Invalid API key
4. Service URL misconfigured

### Feature Flag Not Working

**Symptom:** Provider doesn't switch when flag is changed

**Cause:** Configuration is cached.

**Solution:** Restart the CRM API service.

---

## Migration Checklist

- [ ] Review current deployment configuration
- [ ] Add FeatureManagement section to appsettings.json
- [ ] Add Providers section (if using external providers)
- [ ] Update docker-compose.yml or Kubernetes manifests
- [ ] Test in staging environment
- [ ] Deploy external services (if applicable)
- [ ] Enable feature flags for external providers
- [ ] Verify health endpoints
- [ ] Set up monitoring and alerting
- [ ] Document rollback procedures
- [ ] Train operations team

---

## Support

For issues with the pluggable architecture:

1. Check the [ADR-001 Architecture Decision Record](ADR-001-Pluggable-Architecture-Strategy.md)
2. Review the [Implementation Tracker](PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md)
3. Check logs: `docker-compose logs crm-api | grep -i provider`

---

**Last Updated:** 2026-03-16
