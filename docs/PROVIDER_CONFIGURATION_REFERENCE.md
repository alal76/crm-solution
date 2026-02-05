# Provider Configuration Reference

## Complete Reference for All Pluggable Providers

This document provides detailed configuration options for all providers in the CRM solution.

---

## Table of Contents

1. [Feature Flags](#feature-flags)
2. [Search Providers](#search-providers)
3. [Notification Providers](#notification-providers)
4. [Chat Providers](#chat-providers)
5. [E-Signature Providers](#e-signature-providers)
6. [Analytics Providers](#analytics-providers)
7. [Integration Providers](#integration-providers)
8. [AI/LLM Providers](#aillm-providers)

---

## Feature Flags

### Provider Selection Flags

| Flag | Default | Description |
|------|---------|-------------|
| `UseExternalSearch` | `false` | Use external search provider |
| `UseExternalChat` | `false` | Use external chat provider |
| `UseExternalNotifications` | `false` | Use external notification provider |
| `UseExternalAnalytics` | `false` | Use external analytics provider |
| `UseExternalSignatures` | `false` | Use external e-signature provider |
| `UseExternalIntegrations` | `false` | Use external integration platform |
| `UseExternalAI` | `false` | Use external AI/LLM provider |

### Module Flags

| Flag | Default | Description |
|------|---------|-------------|
| `EnableITSM` | `true` | Enable ITSM module |
| `EnableMarketing` | `true` | Enable Marketing module |
| `EnableCustomerPortal` | `false` | Enable Customer Portal |
| `EnablePartnerPortal` | `false` | Enable Partner Portal |

### Configuration

```json
{
  "FeatureManagement": {
    "UseExternalSearch": true,
    "UseExternalChat": false,
    "UseExternalNotifications": true,
    "UseExternalAnalytics": false,
    "UseExternalSignatures": true,
    "UseExternalIntegrations": false,
    "UseExternalAI": true,
    "EnableITSM": true,
    "EnableMarketing": true,
    "EnableCustomerPortal": false,
    "EnablePartnerPortal": false
  }
}
```

---

## Search Providers

### BuiltIn Search

**Type:** `BuiltIn`

Uses SQL LIKE queries for search functionality. No additional configuration required.

```json
{
  "Providers": {
    "Search": {
      "Type": "BuiltIn"
    }
  }
}
```

### Meilisearch

**Type:** `Meilisearch`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Url` | string | Yes | - | Meilisearch server URL |
| `ApiKey` | string | Yes | - | Master API key |
| `IndexPrefix` | string | No | `crm_` | Prefix for index names |
| `TimeoutSeconds` | int | No | `30` | Request timeout |

```json
{
  "Providers": {
    "Search": {
      "Type": "Meilisearch",
      "Meilisearch": {
        "Url": "http://meilisearch:7700",
        "ApiKey": "${MEILISEARCH_API_KEY}",
        "IndexPrefix": "crm_",
        "TimeoutSeconds": 30
      }
    }
  }
}
```

### Algolia

**Type:** `Algolia`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `ApplicationId` | string | Yes | - | Algolia application ID |
| `ApiKey` | string | Yes | - | Admin API key |
| `SearchOnlyKey` | string | No | - | Search-only key for client |
| `IndexPrefix` | string | No | `crm_` | Prefix for index names |

```json
{
  "Providers": {
    "Search": {
      "Type": "Algolia",
      "Algolia": {
        "ApplicationId": "${ALGOLIA_APP_ID}",
        "ApiKey": "${ALGOLIA_ADMIN_KEY}",
        "SearchOnlyKey": "${ALGOLIA_SEARCH_KEY}",
        "IndexPrefix": "crm_prod_"
      }
    }
  }
}
```

---

## Notification Providers

### BuiltIn (SMTP)

**Type:** `BuiltIn`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Host` | string | Yes | - | SMTP server hostname |
| `Port` | int | Yes | `587` | SMTP port |
| `UseSsl` | bool | No | `true` | Use TLS/SSL |
| `Username` | string | Yes | - | SMTP username |
| `Password` | string | Yes | - | SMTP password |
| `FromAddress` | string | Yes | - | Default from address |
| `FromName` | string | No | - | Default from name |

```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}",
    "FromAddress": "crm@example.com",
    "FromName": "CRM System"
  }
}
```

### Novu

**Type:** `Novu`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `ApiKey` | string | Yes | - | Novu API key |
| `AppId` | string | Yes | - | Novu application ID |
| `ApiUrl` | string | No | `https://api.novu.co` | API endpoint |
| `EmailWorkflowId` | string | No | - | Email workflow ID |
| `SmsWorkflowId` | string | No | - | SMS workflow ID |
| `PushWorkflowId` | string | No | - | Push workflow ID |
| `InAppWorkflowId` | string | No | - | In-app workflow ID |

```json
{
  "Providers": {
    "Notifications": {
      "Type": "Novu",
      "Novu": {
        "ApiKey": "${NOVU_API_KEY}",
        "AppId": "${NOVU_APP_ID}",
        "ApiUrl": "http://novu:3000",
        "EmailWorkflowId": "email-general",
        "SmsWorkflowId": "sms-alert"
      }
    }
  }
}
```

### Twilio

**Type:** `Twilio`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `AccountSid` | string | Yes | - | Twilio Account SID |
| `AuthToken` | string | Yes | - | Twilio Auth Token |
| `FromPhone` | string | Yes | - | Default SMS from number |
| `MessagingServiceSid` | string | No | - | Messaging service SID |
| `StatusCallbackUrl` | string | No | - | Webhook for status updates |

```json
{
  "Providers": {
    "Notifications": {
      "Type": "Twilio",
      "Twilio": {
        "AccountSid": "${TWILIO_ACCOUNT_SID}",
        "AuthToken": "${TWILIO_AUTH_TOKEN}",
        "FromPhone": "+15551234567",
        "StatusCallbackUrl": "https://crm.example.com/api/webhooks/twilio"
      }
    }
  }
}
```

### SendGrid

**Type:** `SendGrid`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `ApiKey` | string | Yes | - | SendGrid API key |
| `FromEmail` | string | Yes | - | Default from email |
| `FromName` | string | No | - | Default from name |
| `TemplateId` | string | No | - | Default template ID |
| `EnableTracking` | bool | No | `true` | Enable open/click tracking |
| `SandboxMode` | bool | No | `false` | Enable sandbox mode |

```json
{
  "Providers": {
    "Notifications": {
      "Type": "SendGrid",
      "SendGrid": {
        "ApiKey": "${SENDGRID_API_KEY}",
        "FromEmail": "crm@example.com",
        "FromName": "CRM System",
        "EnableTracking": true
      }
    }
  }
}
```

---

## Chat Providers

### BuiltIn Chat

**Type:** `BuiltIn`

Basic in-memory chat functionality. Suitable for development/testing only.

### Chatwoot

**Type:** `Chatwoot`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `BaseUrl` | string | Yes | - | Chatwoot API URL |
| `ApiKey` | string | Yes | - | Agent API access token |
| `AccountId` | int | Yes | - | Chatwoot account ID |
| `InboxId` | int | No | - | Default inbox ID |
| `WebhookSecret` | string | No | - | Webhook signature secret |

```json
{
  "Providers": {
    "Chat": {
      "Type": "Chatwoot",
      "Chatwoot": {
        "BaseUrl": "http://chatwoot:3000",
        "ApiKey": "${CHATWOOT_API_KEY}",
        "AccountId": 1,
        "InboxId": 1,
        "WebhookSecret": "${CHATWOOT_WEBHOOK_SECRET}"
      }
    }
  }
}
```

### Intercom

**Type:** `Intercom`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `AppId` | string | Yes | - | Intercom App ID |
| `AccessToken` | string | Yes | - | Access token |
| `AdminId` | string | No | - | Default admin for assignments |
| `WebhookSecret` | string | No | - | Webhook signature secret |
| `ApiVersion` | string | No | `2.10` | API version |

```json
{
  "Providers": {
    "Chat": {
      "Type": "Intercom",
      "Intercom": {
        "AppId": "${INTERCOM_APP_ID}",
        "AccessToken": "${INTERCOM_ACCESS_TOKEN}",
        "WebhookSecret": "${INTERCOM_WEBHOOK_SECRET}",
        "ApiVersion": "2.10"
      }
    }
  }
}
```

---

## E-Signature Providers

### BuiltIn Signatures

**Type:** `BuiltIn`

Manual signature workflow - documents are marked as "manually signed" with audit trail.

### DocuSeal

**Type:** `DocuSeal`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `ApiUrl` | string | Yes | - | DocuSeal API URL |
| `ApiKey` | string | Yes | - | API key |
| `WebhookSecret` | string | No | - | Webhook signature secret |
| `DefaultTemplateId` | int | No | - | Default template ID |

```json
{
  "Providers": {
    "Signatures": {
      "Type": "DocuSeal",
      "DocuSeal": {
        "ApiUrl": "http://docuseal:3000",
        "ApiKey": "${DOCUSEAL_API_KEY}",
        "WebhookSecret": "${DOCUSEAL_WEBHOOK_SECRET}"
      }
    }
  }
}
```

### DocuSign

**Type:** `DocuSign`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `IntegrationKey` | string | Yes | - | Integration key (client ID) |
| `UserId` | string | Yes | - | User ID (GUID) |
| `AccountId` | string | Yes | - | Account ID |
| `BaseUri` | string | No | `https://na4.docusign.net` | API base URI |
| `PrivateKeyPath` | string | Yes | - | Path to RSA private key |
| `OAuthBasePath` | string | No | `account.docusign.com` | OAuth server |
| `AuthorizationEndpoint` | string | No | - | Custom auth endpoint |

```json
{
  "Providers": {
    "Signatures": {
      "Type": "DocuSign",
      "DocuSign": {
        "IntegrationKey": "${DOCUSIGN_INTEGRATION_KEY}",
        "UserId": "${DOCUSIGN_USER_ID}",
        "AccountId": "${DOCUSIGN_ACCOUNT_ID}",
        "BaseUri": "https://na4.docusign.net",
        "PrivateKeyPath": "/secrets/docusign-private.pem",
        "OAuthBasePath": "account.docusign.com"
      }
    }
  }
}
```

---

## Analytics Providers

### BuiltIn Analytics

**Type:** `BuiltIn`

Basic SQL-based reports and dashboards. Limited charting capabilities.

### Superset

**Type:** `Superset`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Url` | string | Yes | - | Superset URL |
| `Username` | string | Yes | - | Admin username |
| `Password` | string | Yes | - | Admin password |
| `GuestTokenEnabled` | bool | No | `true` | Enable guest tokens |
| `DashboardIds` | array | No | - | Dashboard IDs to expose |

```json
{
  "Providers": {
    "Analytics": {
      "Type": "Superset",
      "Superset": {
        "Url": "http://superset:8088",
        "Username": "${SUPERSET_USERNAME}",
        "Password": "${SUPERSET_PASSWORD}",
        "GuestTokenEnabled": true,
        "DashboardIds": ["sales-pipeline", "account-overview"]
      }
    }
  }
}
```

### Power BI

**Type:** `PowerBI`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `TenantId` | string | Yes | - | Azure AD tenant ID |
| `ClientId` | string | Yes | - | Application (client) ID |
| `ClientSecret` | string | Yes | - | Client secret |
| `WorkspaceId` | string | Yes | - | Power BI workspace ID |
| `AuthorityUrl` | string | No | - | Azure AD authority |
| `Scope` | string | No | - | API scope |

```json
{
  "Providers": {
    "Analytics": {
      "Type": "PowerBI",
      "PowerBI": {
        "TenantId": "${AZURE_TENANT_ID}",
        "ClientId": "${POWERBI_CLIENT_ID}",
        "ClientSecret": "${POWERBI_CLIENT_SECRET}",
        "WorkspaceId": "${POWERBI_WORKSPACE_ID}"
      }
    }
  }
}
```

---

## Integration Providers

### BuiltIn Integrations

**Type:** `BuiltIn`

Webhook-based integrations with manual configuration.

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `WebhookBaseUrl` | string | No | - | Base URL for webhooks |
| `SigningSecret` | string | No | - | HMAC signing secret |
| `RetryAttempts` | int | No | `3` | Retry attempts on failure |

### n8n

**Type:** `N8n`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `BaseUrl` | string | Yes | - | n8n API URL |
| `ApiKey` | string | No | - | API key (if enabled) |
| `WebhookUrl` | string | No | - | Webhook base URL |
| `BasicAuthUser` | string | No | - | Basic auth username |
| `BasicAuthPassword` | string | No | - | Basic auth password |

```json
{
  "Providers": {
    "Integrations": {
      "Type": "N8n",
      "N8n": {
        "BaseUrl": "http://n8n:5678",
        "ApiKey": "${N8N_API_KEY}",
        "WebhookUrl": "http://n8n:5678/webhook"
      }
    }
  }
}
```

### Zapier

**Type:** `Zapier`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `WebhookBaseUrl` | string | Yes | - | Zapier webhook URL |
| `ApiKey` | string | No | - | Zapier API key |

```json
{
  "Providers": {
    "Integrations": {
      "Type": "Zapier",
      "Zapier": {
        "WebhookBaseUrl": "https://hooks.zapier.com/hooks/catch/123456"
      }
    }
  }
}
```

---

## AI/LLM Providers

### Ollama

**Type:** `Ollama`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Url` | string | Yes | - | Ollama API URL |
| `Model` | string | Yes | - | Default model name |
| `TimeoutSeconds` | int | No | `120` | Request timeout |
| `EmbeddingModel` | string | No | - | Model for embeddings |

```json
{
  "Providers": {
    "AI": {
      "Type": "Ollama",
      "Ollama": {
        "Url": "http://ollama:11434",
        "Model": "llama3",
        "EmbeddingModel": "nomic-embed-text",
        "TimeoutSeconds": 120
      }
    }
  }
}
```

### Azure OpenAI

**Type:** `AzureOpenAI`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Endpoint` | string | Yes | - | Azure OpenAI endpoint |
| `ApiKey` | string | Yes | - | API key |
| `DeploymentName` | string | Yes | - | Model deployment name |
| `ApiVersion` | string | No | `2024-02-15-preview` | API version |
| `EmbeddingDeployment` | string | No | - | Embedding deployment |

```json
{
  "Providers": {
    "AI": {
      "Type": "AzureOpenAI",
      "AzureOpenAI": {
        "Endpoint": "https://your-resource.openai.azure.com/",
        "ApiKey": "${AZURE_OPENAI_KEY}",
        "DeploymentName": "gpt-4",
        "ApiVersion": "2024-02-15-preview",
        "EmbeddingDeployment": "text-embedding-ada-002"
      }
    }
  }
}
```

### AWS Bedrock

**Type:** `Bedrock`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Region` | string | Yes | - | AWS region |
| `ModelId` | string | Yes | - | Model identifier |
| `AccessKeyId` | string | No | - | AWS access key (or use IAM) |
| `SecretAccessKey` | string | No | - | AWS secret key |
| `MaxTokens` | int | No | `4096` | Max output tokens |

```json
{
  "Providers": {
    "AI": {
      "Type": "Bedrock",
      "Bedrock": {
        "Region": "us-east-1",
        "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0",
        "MaxTokens": 4096
      }
    }
  }
}
```

---

## Environment Variable Mapping

All configuration values can be set via environment variables using the `__` separator:

```bash
# Examples
Providers__Search__Type=Meilisearch
Providers__Search__Meilisearch__Url=http://meilisearch:7700
Providers__Chat__Chatwoot__AccountId=1
FeatureManagement__UseExternalSearch=true
```

---

**Last Updated:** 2024-02-05  
**Version:** 1.0.0
