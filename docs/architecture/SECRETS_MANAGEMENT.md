# Secrets Management Guide

> **CRM Solution - Secrets Management Architecture**  
> **Last Updated:** February 24, 2026  
> **Status:** Active

## Overview

This document describes secrets management patterns for the CRM solution across different deployment environments and secret store providers.

## Supported Secret Stores

### 1. HashiCorp Vault

**Recommended for:** Self-hosted production deployments

#### Configuration

```json
{
  "SecretsManagement": {
    "Provider": "Vault",
    "Vault": {
      "Address": "https://vault.example.com:8200",
      "Token": "s.xxxxxxxxxxxxx",
      "SecretPath": "crm/production",
      "AuthMethod": "token"
    }
  }
}
```

#### Authentication Methods

| Method | Description | Use Case |
|--------|-------------|----------|
| **Token** | Static token authentication | Development, testing |
| **AppRole** | Role-based authentication with role_id/secret_id | Production services |
| **Kubernetes** | Service account authentication | Kubernetes deployments |
| **AWS IAM** | IAM role-based authentication | AWS deployments |

#### AppRole Configuration

```json
{
  "Vault": {
    "AuthMethod": "approle",
    "AppRole": {
      "RoleId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "SecretIdFile": "/run/secrets/vault-secret-id"
    }
  }
}
```

#### Secret Structure

```
crm/production/
├── database/
│   ├── connection-string
│   ├── migration-user
│   └── migration-password
├── jwt/
│   ├── signing-key
│   └── encryption-key
├── redis/
│   └── connection-string
├── providers/
│   ├── openai-api-key
│   ├── sendgrid-api-key
│   └── meilisearch-master-key
└── integrations/
    ├── oauth-client-secrets
    └── webhook-signing-keys
```

---

### 2. AWS Secrets Manager

**Recommended for:** AWS deployments (ECS, EKS, Lambda)

#### Configuration

```json
{
  "SecretsManagement": {
    "Provider": "AWSSecretsManager",
    "AWS": {
      "Region": "us-east-1",
      "SecretPrefix": "crm/production/",
      "UseIAMRole": true
    }
  }
}
```

#### IAM Policy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "secretsmanager:GetSecretValue",
        "secretsmanager:DescribeSecret"
      ],
      "Resource": "arn:aws:secretsmanager:us-east-1:123456789012:secret:crm/production/*"
    }
  ]
}
```

#### Secret Naming Convention

| Secret Name | Contents |
|-------------|----------|
| `crm/production/database` | `{"connectionString": "..."}` |
| `crm/production/jwt` | `{"signingKey": "...", "encryptionKey": "..."}` |
| `crm/production/redis` | `{"connectionString": "..."}` |
| `crm/production/providers/openai` | `{"apiKey": "..."}` |

#### ECS Task Definition Integration

```json
{
  "containerDefinitions": [
    {
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:crm/production/database:connectionString::"
        },
        {
          "name": "Jwt__Secret",
          "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:crm/production/jwt:signingKey::"
        }
      ]
    }
  ]
}
```

---

### 3. Azure Key Vault

**Recommended for:** Azure deployments (AKS, App Service, Azure Functions)

#### Configuration

```json
{
  "SecretsManagement": {
    "Provider": "AzureKeyVault",
    "AzureKeyVault": {
      "VaultUri": "https://crm-prod-kv.vault.azure.net/",
      "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "ClientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "UseManagedIdentity": true
    }
  }
}
```

#### Secret Naming Convention

Azure Key Vault doesn't support nested paths, so use double-dash (`--`) as separator:

| Secret Name | Contents |
|-------------|----------|
| `crm-database--connectionstring` | Connection string value |
| `crm-jwt--signingkey` | JWT signing key |
| `crm-redis--connectionstring` | Redis connection |
| `crm-providers--openai--apikey` | OpenAI API key |

#### Azure App Configuration Integration

```bicep
resource keyVaultReference 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  name: '${configStoreName}/ConnectionStrings:DefaultConnection'
  properties: {
    value: '@Microsoft.KeyVault(SecretUri=https://crm-prod-kv.vault.azure.net/secrets/crm-database--connectionstring)'
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
  }
}
```

#### Managed Identity Setup

```bash
# Enable managed identity on AKS
az aks update -g crm-rg -n crm-aks --enable-managed-identity

# Grant Key Vault access
az keyvault set-policy --name crm-prod-kv \
  --object-id $(az aks show -g crm-rg -n crm-aks --query identityProfile.kubeletidentity.objectId -o tsv) \
  --secret-permissions get list
```

---

## Implementation Patterns

### 1. Configuration Provider Pattern

```csharp
// Program.cs
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

var secretsProvider = builder.Configuration["SecretsManagement:Provider"];

switch (secretsProvider?.ToLowerInvariant())
{
    case "vault":
        builder.Configuration.AddVaultSecrets(builder.Configuration);
        break;
    case "awssecretsmanager":
        builder.Configuration.AddSecretsManager(region: RegionEndpoint.USEast1);
        break;
    case "azurekeyvault":
        var vaultUri = builder.Configuration["SecretsManagement:AzureKeyVault:VaultUri"];
        builder.Configuration.AddAzureKeyVault(new Uri(vaultUri!), new DefaultAzureCredential());
        break;
    default:
        // Use environment variables or user secrets
        break;
}
```

### 2. Secret Rotation

All providers support automatic rotation. Implement refresh logic:

```csharp
public class SecretRefreshService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IConfigurationRoot _configRoot;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            
            if (_configRoot is IConfigurationRoot root)
            {
                root.Reload();
            }
        }
    }
}
```

### 3. Secret Categories

| Category | Examples | Rotation Frequency |
|----------|----------|-------------------|
| **Database** | Connection strings, credentials | 90 days |
| **JWT** | Signing keys, encryption keys | 30 days |
| **API Keys** | Provider API keys | Per vendor policy |
| **OAuth** | Client secrets | 365 days |
| **Webhooks** | Signing secrets | 90 days |

---

## Security Best Practices

### 1. Principle of Least Privilege

- Grant minimum permissions needed
- Use separate identities per environment
- Audit access regularly

### 2. Secret Segregation

```
Environment Structure:
├── development/
│   └── (developer access)
├── staging/
│   └── (CI/CD pipeline access)
└── production/
    └── (production service accounts only)
```

### 3. Never in Source Control

- ❌ Never commit secrets to git
- ❌ Never log secret values
- ❌ Never expose in error messages
- ✅ Use secret references
- ✅ Use environment-specific stores

### 4. Encryption

| At Rest | In Transit |
|---------|------------|
| AES-256 encryption in all providers | TLS 1.3 for all connections |
| Customer-managed keys (CMK) option | Certificate pinning for critical services |

---

## Environment-Specific Configuration

### Development

```bash
# Use .NET User Secrets for local development
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..."
dotnet user-secrets set "Jwt:Secret" "dev-secret-key-min-32-characters!"
```

### Docker Compose (Development)

```yaml
services:
  crm-api:
    environment:
      - ConnectionStrings__DefaultConnection=Server=crm-mariadb;...
      - Jwt__Secret=${JWT_SECRET}  # From .env file
    secrets:
      - db_password
      - jwt_key

secrets:
  db_password:
    file: ./secrets/db_password.txt
  jwt_key:
    file: ./secrets/jwt_key.txt
```

### Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
type: Opaque
data:
  db-connection-string: <base64-encoded>
  jwt-signing-key: <base64-encoded>
---
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
        - name: crm-api
          envFrom:
            - secretRef:
                name: crm-secrets
```

#### External Secrets Operator (Recommended)

```yaml
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: vault-backend
spec:
  provider:
    vault:
      server: "https://vault.example.com:8200"
      path: "crm"
      auth:
        kubernetes:
          mountPath: "kubernetes"
          role: "crm-api"
---
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: crm-secrets
spec:
  secretStoreRef:
    name: vault-backend
    kind: SecretStore
  target:
    name: crm-secrets
  data:
    - secretKey: db-connection-string
      remoteRef:
        key: production/database
        property: connectionString
```

---

## Monitoring & Auditing

### Vault Audit Logs

```hcl
audit "file" {
  file_path = "/var/log/vault/audit.log"
  log_raw   = false
}
```

### AWS CloudTrail

```json
{
  "eventSource": "secretsmanager.amazonaws.com",
  "eventName": "GetSecretValue",
  "userIdentity": {
    "type": "AssumedRole",
    "arn": "arn:aws:sts::123456789012:assumed-role/crm-api-role/..."
  }
}
```

### Azure Key Vault Diagnostics

```bash
az monitor diagnostic-settings create \
  --resource $(az keyvault show --name crm-prod-kv --query id -o tsv) \
  --name kv-diagnostics \
  --logs '[{"category": "AuditEvent", "enabled": true}]'
```

---

## References

- [HashiCorp Vault Documentation](https://www.vaultproject.io/docs)
- [AWS Secrets Manager Best Practices](https://docs.aws.amazon.com/secretsmanager/latest/userguide/best-practices.html)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [Kubernetes Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [External Secrets Operator](https://external-secrets.io/)

---

**TODO-ARCH-013-003** ✅ Implemented
