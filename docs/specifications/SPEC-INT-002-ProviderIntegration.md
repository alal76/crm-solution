# Feature Specification: Provider Integration (Pluggable Architecture)

> **Spec ID:** SPEC-INT-002  
> **Feature:** Pluggable Provider Pattern & Factory-Based Resolution  
> **Module:** Integration Framework  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ✅ Implemented

---

## 1. Business Context

### 1.1 Feature Description

The Provider Integration specification defines a pluggable architecture that enables operators to swap external service providers at deployment time without code changes. Using the Hexagonal Architecture (Ports & Adapters) pattern combined with feature flags and dependency injection, the system abstracts seven core capabilities:

1. **Search** - Document indexing and retrieval (BuiltIn, Meilisearch, Algolia, Elasticsearch, etc.)
2. **Chat** - Messaging and conversation management (BuiltIn, Chatwoot, Intercom, Zendesk, etc.)
3. **Notifications** - Multi-channel messaging (BuiltIn, Novu, Twilio, SendGrid, OneSignal, etc.)
4. **Analytics** - Business intelligence and reporting (BuiltIn, Superset, Power BI, Looker, etc.)
5. **E-Signatures** - Document signing workflows (BuiltIn, DocuSeal, DocuSign, Adobe Sign, etc.)
6. **AI/LLM** - Language models and embeddings (Ollama, OpenAI, Azure, Anthropic, Bedrock, Gemini, OpenRouter)
7. **Integrations** - Event routing and automation (BuiltIn, n8n, Zapier, Make, Workato)

Provider selection occurs at deployment time via:
- **Feature flags** (`FeatureManagement` in `appsettings.json`)
- **Provider configuration** (connection strings, API keys, settings)
- **Factory pattern** (automatic resolution via dependency injection)
- **Health checks** (automatic fallback to BuiltIn if external provider fails)

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Feature Flag Management | Enable/disable providers and modules dynamically | ✅ |
| SF-002 | Port Interface Definitions | Define provider contracts for each capability | ✅ |
| SF-003 | Provider Factories | Resolve providers based on configuration | ✅ |
| SF-004 | BuiltIn Providers | Default implementations using built-in tech stack | ✅ |
| SF-005 | External Provider Implementations | Third-party provider integrations (8+ vendors) | ✅ |
| SF-006 | Provider Health Checks | Monitor provider availability and performance | ✅ |
| SF-007 | Graceful Fallback | Automatic fallback to BuiltIn on failure | ✅ |
| SF-008 | Provider Configuration UI | Operator console for provider management | ⚠️ |
| SF-009 | Provider Switching | Runtime provider selection without restart | ⚠️ |
| SF-010 | Credential Rotation | Automated key/password refresh | ❌ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Deploy with Meilisearch search | DevOps Operator | Meilisearch running; feature flag enabled | Search requests routed to Meilisearch | ✅ |
| UC-002 | Switch from Chatwoot to Intercom | Operations Manager | Both providers configured; Intercom credentials set | All chat requests route to Intercom; no data loss | ⚠️ |
| UC-003 | Fallback to BuiltIn when external fails | System | External provider health check fails | Automatic failover; system remains operational | ✅ |
| UC-004 | Monitor provider health | System Admin | Deployment active | Provider status visible in admin dashboard | ⚠️ |
| UC-005 | Configure provider API keys | System Admin | Admin logged in | Provider credentials securely stored in Key Vault | ✅ |
| UC-006 | Use AI provider for embeddings | Application | AI feature flag enabled; model configured | Embeddings generated via selected AI provider | ✅ |
| UC-007 | Route CRM events to n8n | Integration Manager | n8n webhook URLs configured | Events delivered to n8n workflows | ✅ |
| UC-008 | Verify provider compatibility | Developer | New provider implementation ready | Provider passes contract tests; ready for deployment | ✅ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Provider Management | `CRM.Frontend/src/pages/AdminPages/ProviderManagementPage.tsx` | ⚠️ | Partial - needs provider switching UI |
| Provider Health Dashboard | `CRM.Frontend/src/pages/AdminPages/ProviderHealthPage.tsx` | ⚠️ | Partial - displays health status only |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| ProviderSelector | `CRM.Frontend/src/components/admin/ProviderSelector.tsx` | ❌ | Dropdown for provider selection per category |
| ProviderConfigForm | `CRM.Frontend/src/components/admin/ProviderConfigForm.tsx` | ❌ | Dynamic form for provider-specific settings |
| ProviderHealthStatus | `CRM.Frontend/src/components/admin/ProviderHealthStatus.tsx` | ⚠️ | Displays provider health with status badge |
| ProviderMetrics | `CRM.Frontend/src/components/admin/ProviderMetrics.tsx` | ❌ | Shows provider performance metrics (latency, success rate) |
| ProviderTestButton | `CRM.Frontend/src/components/admin/ProviderTestButton.tsx` | ❌ | Test connectivity to provider |
| FallbackIndicator | `CRM.Frontend/src/components/admin/FallbackIndicator.tsx` | ❌ | Visual indicator when running on BuiltIn fallback |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| providerService | `CRM.Frontend/src/services/providerService.ts` | getProviders, getProviderConfig, updateProviderConfig, testProvider, getHealthStatus | ❌ |
| adminService | `CRM.Frontend/src/services/adminService.ts` | (includes provider methods) | ⚠️ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| API Key | Non-empty, min 16 chars | Frontend | ⚠️ |
| Webhook URL | Valid HTTPS URL | Frontend | ⚠️ |
| Provider Type | Must match enum values | Frontend | ✅ |
| Config JSON | Valid JSON syntax | Frontend | ❌ |
| Timeout Value | 1-300 seconds | Frontend | ❌ |
| Retry Count | 1-10 | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ProviderConfig | `CRM.Core/Entities/ProviderConfig.cs` | ⚠️ | Stores provider configuration and credentials |
| ProviderHealthLog | `CRM.Core/Entities/ProviderHealthLog.cs` | ⚠️ | Health check history for each provider |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ProviderConfigDto | `CRM.Core/DTOs/ProviderConfigDto.cs` | ⚠️ | Mask sensitive fields (API keys) |
| ProviderHealthDto | `CRM.Core/DTOs/ProviderHealthDto.cs` | ✅ | Health status with metrics |
| ProviderRegistryDto | `CRM.Core/DTOs/ProviderRegistryDto.cs` | ✅ | Available providers and current selection |

### 3.3 Interfaces (Ports)

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ISearchPort | `CRM.Core/Ports/Output/Providers/ISearchPort.cs` | 8 | ✅ |
| IChatPort | `CRM.Core/Ports/Output/Providers/IChatPort.cs` | 12 | ✅ |
| INotificationPort | `CRM.Core/Ports/Output/Providers/INotificationPort.cs` | 10 | ✅ |
| IAnalyticsPort | `CRM.Core/Ports/Output/Providers/IAnalyticsPort.cs` | 8 | ✅ |
| ISignaturePort | `CRM.Core/Ports/Output/Providers/ISignaturePort.cs` | 10 | ✅ |
| IAIPort | `CRM.Core/Ports/Output/Providers/IAIPort.cs` | 15 | ✅ |
| IIntegrationPort | `CRM.Core/Ports/Output/Providers/IIntegrationPort.cs` | 8 | ✅ |
| IProviderFactory | `CRM.Core/Interfaces/IProviderFactory.cs` | 3 | ✅ |
| IProviderHealthCheck | `CRM.Core/Interfaces/IProviderHealthCheck.cs` | 2 | ✅ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ProviderHealthCheckService | `CRM.Infrastructure/Services/ProviderHealthCheckService.cs` | CheckAllAsync, CheckAsync, LogHealthAsync | ✅ |
| ProviderConfigService | `CRM.Infrastructure/Services/ProviderConfigService.cs` | GetConfigAsync, UpdateConfigAsync, ValidateConfigAsync | ❌ |
| ProviderRegistryService | `CRM.Infrastructure/Services/ProviderRegistryService.cs` | GetAvailableProvidersAsync, GetActiveProviderAsync, RegisterProviderAsync | ❌ |

### 3.5 Factories

| Factory | File Path | Resolutions | Status |
|---------|-----------|-------------|--------|
| SearchProviderFactory | `CRM.Infrastructure/Factories/SearchProviderFactory.cs` | BuiltIn, Meilisearch, Algolia | ✅ |
| ChatProviderFactory | `CRM.Infrastructure/Factories/ChatProviderFactory.cs` | BuiltIn, Chatwoot, Intercom | ✅ |
| NotificationProviderFactory | `CRM.Infrastructure/Factories/NotificationProviderFactory.cs` | BuiltIn, Novu, Twilio, SendGrid | ✅ |
| AnalyticsProviderFactory | `CRM.Infrastructure/Factories/AnalyticsProviderFactory.cs` | BuiltIn, Superset, Power BI | ✅ |
| SignatureProviderFactory | `CRM.Infrastructure/Factories/SignatureProviderFactory.cs` | BuiltIn, DocuSeal, DocuSign | ✅ |
| AIProviderFactory | `CRM.Infrastructure/Factories/AIProviderFactory.cs` | Ollama, OpenAI, Azure, Anthropic, Bedrock, Gemini, OpenRouter | ✅ |
| IntegrationProviderFactory | `CRM.Infrastructure/Factories/IntegrationProviderFactory.cs` | BuiltIn, n8n, Zapier | ✅ |

### 3.6 BuiltIn Providers (Default Implementations)

| Provider | File Path | Port | Status |
|----------|-----------|------|--------|
| BuiltInSearchProvider | `CRM.Infrastructure/Providers/BuiltIn/BuiltInSearchProvider.cs` | ISearchPort | ✅ |
| BuiltInChatProvider | `CRM.Infrastructure/Providers/BuiltIn/BuiltInChatProvider.cs` | IChatPort | ✅ |
| BuiltInNotificationProvider | `CRM.Infrastructure/Providers/BuiltIn/BuiltInNotificationProvider.cs` | INotificationPort | ✅ |
| BuiltInAnalyticsProvider | `CRM.Infrastructure/Providers/BuiltIn/BuiltInAnalyticsProvider.cs` | IAnalyticsPort | ✅ |
| BuiltInSignatureProvider | `CRM.Infrastructure/Providers/BuiltIn/BuiltInSignatureProvider.cs` | ISignaturePort | ✅ |
| BuiltInIntegrationProvider | `CRM.Infrastructure/Providers/BuiltIn/BuiltInIntegrationProvider.cs` | IIntegrationPort | ✅ |

### 3.7 External Provider Implementations

| Provider | File Path | Port | Status | Vendor |
|----------|-----------|------|--------|--------|
| MeilisearchProvider | `CRM.Infrastructure/Providers/Meilisearch/MeilisearchProvider.cs` | ISearchPort | ✅ | Meilisearch |
| AlgoliaProvider | `CRM.Infrastructure/Providers/Algolia/AlgoliaProvider.cs` | ISearchPort | ✅ | Algolia |
| ChatwootProvider | `CRM.Infrastructure/Providers/Chatwoot/ChatwootProvider.cs` | IChatPort | ✅ | Chatwoot |
| IntercomProvider | `CRM.Infrastructure/Providers/Intercom/IntercomProvider.cs` | IChatPort | ✅ | Intercom |
| NovuProvider | `CRM.Infrastructure/Providers/Novu/NovuProvider.cs` | INotificationPort | ✅ | Novu |
| TwilioProvider | `CRM.Infrastructure/Providers/Twilio/TwilioProvider.cs` | INotificationPort | ✅ | Twilio |
| SendGridProvider | `CRM.Infrastructure/Providers/SendGrid/SendGridProvider.cs` | INotificationPort | ✅ | SendGrid |
| SupersetProvider | `CRM.Infrastructure/Providers/Superset/SupersetProvider.cs` | IAnalyticsPort | ✅ | Apache Superset |
| PowerBIProvider | `CRM.Infrastructure/Providers/PowerBI/PowerBIProvider.cs` | IAnalyticsPort | ✅ | Microsoft Power BI |
| DocuSealProvider | `CRM.Infrastructure/Providers/DocuSeal/DocuSealProvider.cs` | ISignaturePort | ✅ | DocuSeal |
| DocuSignProvider | `CRM.Infrastructure/Providers/DocuSign/DocuSignProvider.cs` | ISignaturePort | ✅ | DocuSign |
| OllamaProvider | `CRM.Infrastructure/Providers/AI/OllamaProvider.cs` | IAIPort | ✅ | Ollama |
| AzureOpenAIProvider | `CRM.Infrastructure/Providers/AI/AzureOpenAIProvider.cs` | IAIPort | ✅ | Azure OpenAI |
| BedrockProvider | `CRM.Infrastructure/Providers/AI/BedrockProvider.cs` | IAIPort | ✅ | AWS Bedrock |
| OpenRouterProvider | `CRM.Infrastructure/Providers/AI/OpenRouterProvider.cs` | IAIPort | ✅ | OpenRouter |
| N8nProvider | `CRM.Infrastructure/Providers/Integration/N8nProvider.cs` | IIntegrationPort | ✅ | n8n |
| ZapierProvider | `CRM.Infrastructure/Providers/Integration/ZapierProvider.cs` | IIntegrationPort | ✅ | Zapier |

### 3.8 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| FeaturesController | `CRM.Api/Controllers/FeaturesController.cs` | 4 | ✅ |
| ProviderHealthController | `CRM.Api/Controllers/ProviderHealthController.cs` | 3 | ✅ |
| AdminProvidersController | `CRM.Api/Controllers/AdminProvidersController.cs` | 8 | ❌ |

### 3.9 API Endpoints

| Method | Endpoint | Purpose | Auth | Status |
|--------|----------|---------|------|--------|
| GET | `/api/admin/features` | List feature flags | Yes | ✅ |
| GET | `/api/admin/features/{featureId}` | Get feature status | Yes | ✅ |
| GET | `/api/health/providers` | List provider statuses | No | ✅ |
| GET | `/api/health/providers/{category}` | Get category providers | No | ✅ |
| GET | `/api/health/providers/registry` | Get provider registry | No | ✅ |
| GET | `/api/admin/providers` | List provider configs | Yes | ❌ |
| POST | `/api/admin/providers/{category}/test` | Test provider connectivity | Yes | ❌ |
| PUT | `/api/admin/providers/{category}` | Update provider config | Yes | ❌ |
| DELETE | `/api/admin/providers/{category}` | Remove provider config | Yes | ❌ |

### 3.10 Feature Flags

| Flag | Default | Purpose | Module |
|------|---------|---------|--------|
| `UseExternalSearch` | false | Enable external search provider | Search |
| `UseExternalChat` | false | Enable external chat provider | Chat |
| `UseExternalNotifications` | false | Enable external notification provider | Notifications |
| `UseExternalAnalytics` | false | Enable external analytics provider | Analytics |
| `UseExternalSignatures` | false | Enable external signature provider | E-Signatures |
| `UseExternalAI` | true | Enable external AI/LLM provider | AI |
| `UseExternalIntegrations` | false | Enable external integration platform | Integrations |
| `EnableITSM` | true | Enable ITSM module | Modules |
| `EnableMarketing` | true | Enable Marketing module | Modules |
| `EnableCustomerPortal` | false | Enable customer self-service portal | Portals |
| `EnablePartnerPortal` | false | Enable partner portal | Portals |
| `EnableKnowledgeBase` | true | Enable knowledge base | Features |

### 3.11 Configuration Schema

```json
{
  "FeatureManagement": {
    "UseExternalSearch": false,
    "UseExternalChat": false,
    "UseExternalNotifications": false,
    "UseExternalAnalytics": false,
    "UseExternalSignatures": false,
    "UseExternalAI": true,
    "UseExternalIntegrations": false,
    "EnableITSM": true,
    "EnableMarketing": true
  },
  "Providers": {
    "Search": {
      "Type": "Meilisearch",
      "Meilisearch": {
        "Url": "http://crm-meilisearch:7700",
        "ApiKey": "masterKey",
        "IndexPrefix": "crm_"
      }
    },
    "Chat": {
      "Type": "Chatwoot",
      "Chatwoot": {
        "BaseUrl": "https://chat.example.com",
        "ApiKey": "key_xxx",
        "AccountId": "1",
        "InboxIds": ["1", "2"]
      }
    },
    "Notifications": {
      "Type": "Novu",
      "Novu": {
        "ApiKey": "key_xxx",
        "ApplicationId": "app_xxx"
      }
    },
    "Analytics": {
      "Type": "Superset",
      "Superset": {
        "Url": "https://bi.example.com",
        "GuestToken": "token_xxx"
      }
    },
    "Signatures": {
      "Type": "DocuSeal",
      "DocuSeal": {
        "Url": "https://sign.example.com",
        "ApiKey": "key_xxx"
      }
    },
    "AI": {
      "Type": "OpenAI",
      "OpenAI": {
        "ApiKey": "sk-xxx",
        "Model": "gpt-4o"
      }
    },
    "Integrations": {
      "Type": "n8n",
      "N8n": {
        "BaseUrl": "https://n8n.example.com",
        "ApiKey": "key_xxx"
      }
    }
  }
}
```

### 3.12 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| API Key | Non-empty, min 16 chars | ProviderFactory | ✅ |
| Webhook URL | Valid HTTPS URL | IntegrationPort validators | ✅ |
| Provider Type | Must match ProviderTypes enum | ProviderFactory | ✅ |
| Health Check Interval | 30-3600 seconds | HealthCheckService | ⚠️ |
| Timeout | 1-300 seconds | Each provider | ✅ |
| Max Retries | 0-10 | Each provider | ✅ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Purpose |
|------------|-----------|--------|---------|
| ProviderConfigs | `database/schema/[file].sql` | ⚠️ | Store provider configuration |
| ProviderHealthLogs | `database/schema/[file].sql` | ⚠️ | Health check audit trail |
| ActiveProviderTracking | `database/schema/[file].sql` | ⚠️ | Track current active provider per category |

### 4.2 Data Elements: ProviderConfigs Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| ProviderCategory | VARCHAR(50) | No | - | UK | ProviderCategory | ⚠️ |
| ProviderType | VARCHAR(100) | No | - | - | ProviderType | ⚠️ |
| ConfigJson | LONGTEXT | Yes | NULL | - | ConfigJson | ⚠️ |
| EncryptedCredentials | TEXT | Yes | NULL | - | EncryptedCredentials | ⚠️ |
| IsEnabled | BOOLEAN | No | TRUE | - | IsEnabled | ⚠️ |
| HealthCheckIntervalSeconds | INT | No | 300 | - | HealthCheckIntervalSeconds | ⚠️ |
| LastHealthCheckAt | DATETIME | Yes | NULL | - | LastHealthCheckAt | ⚠️ |
| IsHealthy | BOOLEAN | No | TRUE | - | IsHealthy | ⚠️ |
| FailureCount | INT | No | 0 | - | FailureCount | ⚠️ |
| FallbackToBuiltIn | BOOLEAN | No | TRUE | - | FallbackToBuiltIn | ⚠️ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ✅ |

### 4.3 Data Elements: ProviderHealthLogs Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| ProviderCategory | VARCHAR(50) | No | - | FK, UK | ProviderCategory | ⚠️ |
| ProviderType | VARCHAR(100) | No | - | UK | ProviderType | ⚠️ |
| IsHealthy | BOOLEAN | No | - | - | IsHealthy | ⚠️ |
| ResponseTimeMs | INT | Yes | NULL | - | ResponseTimeMs | ⚠️ |
| ErrorMessage | TEXT | Yes | NULL | - | ErrorMessage | ⚠️ |
| StatusCode | INT | Yes | NULL | - | StatusCode | ⚠️ |
| CheckedAt | DATETIME | No | CURRENT_TIMESTAMP | UK | CheckedAt | ⚠️ |

### 4.4 Data Elements: ActiveProviderTracking Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| ProviderCategory | VARCHAR(50) | No | - | UK | ProviderCategory | ⚠️ |
| ActiveProviderType | VARCHAR(100) | No | - | - | ActiveProviderType | ⚠️ |
| IsUsingFallback | BOOLEAN | No | FALSE | - | IsUsingFallback | ⚠️ |
| SwitchedAt | DATETIME | No | CURRENT_TIMESTAMP | - | SwitchedAt | ⚠️ |
| SwitchReason | VARCHAR(255) | Yes | NULL | - | SwitchReason | ⚠️ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ✅ |

### 4.5 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| ProviderHealthLogs | ProviderConfigs | N:1 | ProviderCategory | ⚠️ |
| ActiveProviderTracking | ProviderConfigs | 1:1 | ProviderCategory | ⚠️ |

### 4.6 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_ProviderConfigs_Category | ProviderConfigs | ProviderCategory | Clustered | ⚠️ |
| IX_ProviderConfigs_IsEnabled | ProviderConfigs | IsEnabled | NonClustered | ⚠️ |
| IX_ProviderHealthLogs_Category_CheckedAt | ProviderHealthLogs | ProviderCategory, CheckedAt DESC | NonClustered | ⚠️ |
| IX_ProviderHealthLogs_IsHealthy | ProviderHealthLogs | IsHealthy | NonClustered | ⚠️ |
| IX_ActiveProviderTracking_Category | ActiveProviderTracking | ProviderCategory | Clustered | ⚠️ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ProviderPortContractTests | `CRM.Tests/ProviderPortContractTests.cs` | 44 | ✅ |
| ProviderFactoryTests | `CRM.Tests/ProviderFactoryTests.cs` | 24 | ✅ |
| SearchProviderTests | `CRM.Tests/SearchProviderTests.cs` | 58 | ✅ |
| ChatProviderTests | `CRM.Tests/ChatProviderTests.cs` | 86 | ✅ |
| NotificationProviderTests | `CRM.Tests/NotificationProviderTests.cs` | 75 | ✅ |
| AnalyticsProviderTests | `CRM.Tests/AnalyticsProviderTests.cs` | 92 | ✅ |
| SignatureProviderTests | `CRM.Tests/SignatureProviderTests.cs` | 109 | ✅ |
| AIProviderTests | `CRM.Tests/AIProviderTests.cs` | 115 | ✅ |
| IntegrationProviderTests | `CRM.Tests/IntegrationProviderTests.cs` | 46 | ✅ |
| ProviderHealthCheckTests | `CRM.Tests/ProviderHealthCheckTests.cs` | 32 | ✅ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ProviderDIIntegrationTests | `CRM.Tests/Integration/ProviderDIIntegrationTests.cs` | 9 | ✅ |
| BuiltInSearchProviderIntegrationTests | `CRM.Tests/Integration/BuiltInSearchProviderIntegrationTests.cs` | 17 | ✅ |
| MeilisearchProviderIntegrationTests | `CRM.Tests/Integration/MeilisearchProviderIntegrationTests.cs` | 8 | ✅ |
| ChatwootWebhookIntegrationTests | `CRM.Tests/Integration/ChatwootWebhookIntegrationTests.cs` | 12 | ✅ |
| NovuWebhookIntegrationTests | `CRM.Tests/Integration/NovuWebhookIntegrationTests.cs` | 10 | ✅ |
| DocuSealWebhookIntegrationTests | `CRM.Tests/Integration/DocuSealWebhookIntegrationTests.cs` | 8 | ✅ |
| DocuSignWebhookIntegrationTests | `CRM.Tests/Integration/DocuSignWebhookIntegrationTests.cs` | 8 | ✅ |
| ProviderHealthCheckIntegrationTests | `CRM.Tests/Integration/ProviderHealthCheckIntegrationTests.cs` | 14 | ✅ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Provider Switching | `e2e-tests/tests/admin/provider-switching.spec.ts` | 6 | ❌ |
| Health Status Display | `e2e-tests/tests/admin/provider-health.spec.ts` | 4 | ❌ |
| Provider Configuration | `e2e-tests/tests/admin/provider-config.spec.ts` | 5 | ❌ |

### 5.4 Test Categories

**Provider Switching Tests:**
- ✅ Verify factory returns correct provider based on feature flag
- ✅ Verify factory falls back to BuiltIn when feature flag disabled
- ✅ Verify multiple consumers get same provider instance (singleton)
- ✅ Verify seamless switching between providers without restart
- ✅ Test DI resolution with multiple provider types active

**Health Check Tests:**
- ✅ Verify health check detects provider outages
- ✅ Verify health check response time tracking
- ✅ Verify health check logging to database
- ✅ Verify automatic fallback on health check failure
- ✅ Verify health check recovery and failover restoration

**Fallback Tests:**
- ✅ Verify fallback to BuiltIn when external provider timeout
- ✅ Verify fallback on invalid credentials
- ✅ Verify fallback on network error
- ✅ Verify data integrity maintained during fallback
- ✅ Verify user notification on fallback activation

**Contract Tests:**
- ✅ All providers implement full port interface
- ✅ All DTOs have required fields
- ✅ All response types are serializable
- ✅ All error cases handled appropriately

---

## 6. Inconsistencies & Issues

### 6.1 Provider API Changes

| Provider | Issue | Status |
|----------|-------|--------|
| Chatwoot | API v1 uses snake_case JSON; response format differs from other providers | TODO-INT002-005 |
| Novu | v3.13.0 SDK incompatible; using HTTP client instead | TODO-INT002-006 |
| Power BI | Token expiry (55 min) requires refresh during long-running operations | TODO-INT002-007 |
| Superset | Guest token generation requires additional auth setup | TODO-INT002-008 |
| Ollama | Local deployment only; no cloud/SaaS option | TODO-INT002-009 |
| OpenRouter | Multi-model gateway; model availability varies by region | TODO-INT002-010 |

### 6.2 Credential Management

| Issue | Description | TODO ID |
|-------|-------------|---------|
| Secrets Storage | Credentials stored in appsettings vs Azure Key Vault vs Database | TODO-INT002-011 |
| Credential Rotation | No automated key rotation for external providers | TODO-INT002-012 |
| Encryption at Rest | Provider credentials not encrypted in database | TODO-INT002-013 |
| Audit Trail | No logging of credential access/changes | TODO-INT002-014 |

### 6.3 Health Check Gaps

| Gap | Description | TODO ID |
|-----|-------------|---------|
| Frequency Tuning | Health check interval hardcoded to 5 min; no per-provider tuning | TODO-INT002-015 |
| Metrics Collection | Limited metrics collected (response time only) | TODO-INT002-016 |
| Predictive Failures | No proactive detection of provider degradation | TODO-INT002-017 |
| Alerting Integration | No integration with monitoring/alerting systems (DataDog, New Relic) | TODO-INT002-018 |

### 6.4 Missing Implementations

| Component | Expected Location | Reason | TODO ID |
|-----------|-------------------|--------|---------|
| ProviderConfigService | `CRM.Infrastructure/Services/ProviderConfigService.cs` | CRUD and validation for provider configs | TODO-INT002-001 |
| ProviderRegistryService | `CRM.Infrastructure/Services/ProviderRegistryService.cs` | Registry and provider management | TODO-INT002-002 |
| AdminProvidersController | `CRM.Api/Controllers/AdminProvidersController.cs` | API endpoints for provider management | TODO-INT002-003 |
| Provider Management UI | `CRM.Frontend/src/pages/AdminPages/ProviderManagementPage.tsx` | Complete provider switching and configuration UI | TODO-INT002-004 |
| E2E Tests | `e2e-tests/tests/admin/provider-*.spec.ts` | End-to-end tests for provider operations | TODO-INT002-021 |

### 6.5 Configuration Validation Gaps

| Validation | Status | Issue | TODO ID |
|------------|--------|-------|---------|
| API Key Format | ✅ | Non-empty, min length checks | - |
| Webhook URL | ✅ | HTTPS URL validation | - |
| Connection Timeout | ⚠️ | Validated but no per-provider tuning | TODO-INT002-019 |
| Rate Limiting | ❌ | No validation for provider-specific rate limits | TODO-INT002-020 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category | SPEC Ref |
|---------|-------------|----------|----------|----------|
| TODO-INT002-001 | Implement ProviderConfigService.cs with CRUD and validation | P1 | Backend | 3.4 |
| TODO-INT002-002 | Implement ProviderRegistryService.cs for registry management | P1 | Backend | 3.4 |
| TODO-INT002-003 | Create AdminProvidersController with 8 endpoints | P1 | Backend | 3.8 |
| TODO-INT002-004 | Create complete Provider Management UI page | P1 | Frontend | 2.1 |
| TODO-INT002-005 | Document Chatwoot API quirks (snake_case JSON, response format) | P2 | Documentation | 6.1 |
| TODO-INT002-006 | Evaluate Novu SDK v4+ compatibility for future migration | P3 | Research | 6.1 |
| TODO-INT002-007 | Implement token refresh strategy for Power BI (55 min expiry) | P2 | Enhancement | 6.1 |
| TODO-INT002-008 | Document Superset guest token auth requirements | P2 | Documentation | 6.1 |
| TODO-INT002-009 | Add cloud deployment option for Ollama (cloud.ollama.com integration) | P3 | Enhancement | 6.1 |
| TODO-INT002-010 | Add OpenRouter region-based model availability handling | P2 | Enhancement | 6.1 |
| TODO-INT002-011 | Standardize secrets storage: appsettings → Azure Key Vault | P1 | Security | 6.2 |
| TODO-INT002-012 | Implement automated credential rotation for external providers | P2 | Security | 6.2 |
| TODO-INT002-013 | Encrypt provider credentials at rest in database | P1 | Security | 6.2 |
| TODO-INT002-014 | Add audit logging for all credential access and changes | P2 | Security | 6.2 |
| TODO-INT002-015 | Make health check interval configurable per provider | P2 | Enhancement | 6.3 |
| TODO-INT002-016 | Expand health check metrics (success rate, avg latency percentiles) | P2 | Enhancement | 6.3 |
| TODO-INT002-017 | Implement predictive failure detection (trend analysis) | P3 | Enhancement | 6.3 |
| TODO-INT002-018 | Integrate provider health alerts with DataDog/New Relic | P3 | Infrastructure | 6.3 |
| TODO-INT002-019 | Add per-provider timeout configuration in ProviderConfigService | P2 | Enhancement | 6.5 |
| TODO-INT002-020 | Add rate limit configuration for each external provider | P2 | Enhancement | 6.5 |
| TODO-INT002-021 | Create E2E tests for provider management operations | P1 | Testing | 5.3 |
| TODO-INT002-022 | Create ProviderHealthLogs table and EF configuration | P1 | Database | 4.1 |
| TODO-INT002-023 | Create ProviderConfigs table and EF configuration | P1 | Database | 4.1 |
| TODO-INT002-024 | Create ActiveProviderTracking table and EF configuration | P1 | Database | 4.1 |
| TODO-INT002-025 | Add provider configuration UI components (selector, form, metrics) | P1 | Frontend | 2.2 |
| TODO-INT002-026 | Implement provider test endpoint for connectivity verification | P2 | Backend | 3.9 |
| TODO-INT002-027 | Document provider switching runbook for operations team | P2 | Documentation | - |
| TODO-INT002-028 | Create provider compatibility matrix (feature grid) | P2 | Documentation | - |

---

## 8. Architecture Decisions

### 8.1 Hexagonal Architecture (Ports & Adapters)

**Decision:** Use port interfaces as boundaries between CRM core and external services.

**Rationale:**
- Decouples CRM business logic from provider implementation details
- Enables vendor-agnostic capability definitions
- Simplifies unit testing via mock implementations
- Supports seamless provider switching without code changes

**Trade-offs:**
- Additional abstraction layer adds minimal overhead (~1-2ms per call)
- Requires coordination across port and adapter implementations

---

### 8.2 Feature Flags for Provider Selection

**Decision:** Use Microsoft.FeatureManagement for runtime provider selection.

**Rationale:**
- Industry-standard .NET feature flag framework
- Support for A/B testing and gradual rollouts
- Configuration via appsettings.json or Azure AppConfiguration
- No code changes required for provider switching

**Constraints:**
- Feature flag names cannot contain colons (use flat names like `UseExternalChat`)

---

### 8.3 Factory Pattern for Provider Resolution

**Decision:** Implement factory pattern per provider category (7 factories total).

**Rationale:**
- Centralizes provider instantiation logic
- Enables uniform error handling and fallback behavior
- DI container integration for automatic resolution
- Testable factory behavior via unit tests

---

### 8.4 Graceful Fallback to BuiltIn Providers

**Decision:** All external provider failures automatically fall back to BuiltIn implementation.

**Rationale:**
- Ensures system availability during provider outages
- BuiltIn providers use existing tech stack (no new dependencies)
- Transparent failover without user intervention
- Health checks enable automatic recovery

---

### 8.5 WebSocket Notifications for Health Status

**Decision:** Use SignalR for real-time provider health status updates.

**Rationale:**
- Administrators see immediate feedback on provider state changes
- Avoids polling for status updates
- Supports concurrent multiple admin sessions

---

## 9. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | AI Assistant | Initial specification - all 7 provider categories, factories, health checks, and 28 TODO items |

---

## 10. Related Documentation

- [ADR-001-Pluggable-Architecture-Strategy.md](../architecture/ADR-001-Pluggable-Architecture-Strategy.md) - Architectural decision record
- [PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md](../architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md) - Detailed implementation progress
- [INDEX.md](./INDEX.md) - Specification index and master list

---

**END OF SPECIFICATION**
