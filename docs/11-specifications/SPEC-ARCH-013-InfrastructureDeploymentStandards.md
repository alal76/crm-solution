# SPEC-ARCH-013: Infrastructure & Deployment Standards

> **Spec ID:** SPEC-ARCH-013  
> **Feature:** Infrastructure & Deployment Standards  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** 2026-02-16  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description
Define the canonical infrastructure, container naming, networking, and deployment standards for all supported modes (on-prem, data center, Docker monolith, Kubernetes, and microservices). This spec standardizes how pluggable providers, databases, and AI services are wired together so every environment behaves consistently.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| INF-001 | Deployment Modes | Monolith, microservices, hybrid | ❌ |
| INF-002 | Container & Image Naming | Standard names, tags, and registries | ❌ |
| INF-003 | Network & DNS | Docker network standards, aliases | ❌ |
| INF-004 | Configuration & Secrets | Env var schema, config files, secret handling | ❌ |
| INF-005 | Pluggable Providers | Provider mappings, flags, URLs | ❌ |
| INF-006 | Storage & Backups | Volumes, backups, retention | ❌ |
| INF-007 | Orchestration | Docker Compose, Kubernetes manifests | ❌ |
| INF-008 | Cloud Options | AWS/Azure/GCP resource naming | ❌ |
| INF-009 | Observability | Health checks, logs, metrics | ❌ |
| INF-010 | Security Hardening | TLS, ports, auth boundaries | ❌ |
| INF-011 | Helm Packaging | Helm charts for all deployment modes | ✅ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Deploy monolith on-prem | DevOps | Docker host + DB ready | CRM running with API + frontend | ❌ |
| UC-002 | Deploy microservices in data center | DevOps | K8s or Compose ready | Services reachable via gateway | ❌ |
| UC-003 | Enable external providers | Admin | Feature flags + keys set | Provider health reports healthy | ❌ |
| UC-004 | Migrate to cloud | Architect | Registry + network configured | Services deployed in cloud | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| WorkerOperationsPage | `CRM.Frontend/src/pages/admin/WorkerOperationsPage.tsx` | ✅ | Ops view for worker control |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| N/A | N/A | ❌ | No additional UI required |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| workerAdminService | `CRM.Frontend/src/services/workerAdminService.ts` | getHealth, getStats, control | ✅ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| MaxWorkers | >= 1 | Frontend | ⚠️ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| SystemSettings | `CRM.Core/Entities/SystemSettings.cs` | ⚠️ | Worker control settings added |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| WorkerControlStatusDto | `CRM.Core/Dtos/WorkerControlDtos.cs` | ✅ | Control state + max workers |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ISystemSettingsService | `CRM.Core/Interfaces/ISystemSettingsService.cs` | existing | ✅ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| SystemSettingsService | `CRM.Infrastructure/Services/SystemSettingsService.cs` | updated | ⚠️ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| WorkerControlController | `CRM.Api/Controllers/WorkerControlController.cs` | 4 | ✅ |
| WorkerHealthController | `CRM.Api/Controllers/WorkerHealthController.cs` | 2 | ✅ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/workers/health` | GetHealth | No | ✅ |
| GET | `/api/workers/stats` | GetStats | Yes | ✅ |
| GET | `/api/workers/control` | GetControlStatus | Yes | ✅ |
| PUT | `/api/workers/control/max-workers` | UpdateMaxWorkers | Yes | ✅ |
| POST | `/api/workers/control/start` | StartWorkers | Yes | ✅ |
| POST | `/api/workers/control/stop` | StopWorkers | Yes | ✅ |
| POST | `/api/workers/control/restart` | RestartWorkers | Yes | ✅ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| WorkerMaxInstances | >= 1 | Service | ✅ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|----------|--------|-------|
| SystemSettings | `database/migrations/20260216_add_worker_control_settings.sql` | ✅ | Adds worker control columns |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| WorkerControlState | VARCHAR(50) | No | Running | - | WorkerControlState | ✅ |
| WorkerMaxInstances | INT | No | 1 | - | WorkerMaxInstances | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| N/A | N/A | N/A | N/A | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| N/A | N/A | N/A | N/A | ✅ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WorkerControlControllerTests | `CRM.Backend/tests/Controllers/WorkerControlControllerTests.cs` | 2 | ⚠️ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| N/A | N/A | 0 | ❌ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WorkerOpsAdminUI | `e2e-tests/tests/ops/worker-ops.spec.ts` | 0 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| N/A | N/A | N/A | N/A |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Secrets manager guidance | `docs/deployment/` | Not documented | TODO-ARCH-013-003 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| WorkerControlState | No enum validation | TODO-ARCH-013-004 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-ARCH-013-003 | Add secrets manager guidance (Vault/AWS/Azure/GCP) | P1 | Security |
| TODO-ARCH-013-004 | Validate WorkerControlState values in API | P2 | Backend |

---

## 8. Infrastructure & Deployment Standards

### 8.1 Deployment Modes
| Mode | Description | Recommended For | Notes |
|------|-------------|----------------|------|
| Monolith (Docker) | Single API + frontend container | Small/medium deployments | Simpler ops, single DB |
| Microservices | Multiple services + gateway | High scale/HA | Requires service discovery |
| Hybrid | Monolith API + external providers | Transitional deployments | Use feature flags |

### 8.2 Standard Docker Containers (Monolith)
| Container | Image | Port(s) | Network Alias | Purpose |
|-----------|-------|---------|---------------|---------|
| crm-api | crm-api:latest | 5000 | crm-api | API monolith |
| crm-frontend | crm-frontend:latest | 80 (or 3000 dev) | crm-frontend | React SPA |
| crm-mariadb | mariadb | 3306 | crm-mariadb | Primary DB |
| crm-redis | redis | 6379 | crm-redis | Cache |
| crm-meilisearch | meilisearch | 7700 | crm-meilisearch | Search |
| crm-ollama | ollama | 11434 | crm-ollama | Local LLM |

**Legacy/alternate naming:** Some deployments use `crm-db` for MariaDB and `crm_crm-network` for the Docker network. Preferred standard is `crm-mariadb` and `docker_crm-network` (see 8.4).

### 8.3 Standard Docker Containers (Microservices)
| Service | Port | Image | Purpose |
|--------|------|-------|---------|
| crm-gateway | 5000 | crm-gateway | YARP API Gateway |
| crm-identity | 5001 | crm-identity | Auth, Users, Groups |
| crm-customer | 5002 | crm-customer | Accounts, Contacts |
| crm-sales | 5003 | crm-sales | Opportunities, Quotes |
| crm-marketing | 5004 | crm-marketing | Campaigns, Leads |
| crm-servicedesk | 5005 | crm-servicedesk | Tickets, Workflows |
| crm-core | 5006 | crm-core | Settings, Monitoring |

### 8.4 Docker Network Standards
| Standard | Value | Notes |
|----------|-------|------|
| Network name (preferred) | docker_crm-network | Primary docs standard |
| Network name (legacy) | crm_crm-network | Exists in remote deploy scripts |
| Aliases | crm-api, crm-frontend, crm-mariadb, crm-redis | Resolve inside network |

### 8.5 Configuration & Environment Variables
**Core:**
- `ASPNETCORE_ENVIRONMENT=Development|Production`
- `ConnectionStrings__DefaultConnection=<connection-string>`
- `Jwt__Secret=<min-32-char>`
- `Jwt__Issuer=CRM.Api`
- `Jwt__Audience=CRM.Client`
- `Redis__ConnectionString=crm-redis:6379`

**Worker control:**
- `WorkerMaxInstances` stored in `SystemSettings`
- `WorkerControlState` stored in `SystemSettings`

### 8.6 Pluggable Provider Configuration
**Feature flags (Microsoft.FeatureManagement):**
- `UseExternalSearch`, `UseExternalChat`, `UseExternalNotifications`, `UseExternalAnalytics`, `UseExternalSignatures`, `UseExternalAI`, `UseExternalIntegrations`

**Provider types:**
- Search: BuiltIn, Meilisearch, Algolia, Typesense, Elasticsearch, AzureCognitiveSearch
- Chat: BuiltIn, Chatwoot, Intercom, Zendesk, Freshchat, RocketChat
- Notifications: BuiltIn, Novu, Twilio, SendGrid, OneSignal, Courier, AWSSES
- Analytics: BuiltIn, Superset, Metabase, PowerBI, Looker, QuickSight
- Signatures: BuiltIn, DocuSeal, DocuSign, AdobeSign, HelloSign
- AI: Ollama, OpenAI, AzureOpenAI, Anthropic, Bedrock, OpenRouter, Gemini
- Integrations: BuiltIn, N8n, Zapier, Make, Workato

**Configuration schema (example):**
```json
{
  "FeatureManagement": {
    "UseExternalSearch": false,
    "UseExternalChat": false,
    "UseExternalNotifications": false,
    "UseExternalAnalytics": false,
    "UseExternalSignatures": false,
    "UseExternalAI": true,
    "UseExternalIntegrations": false
  },
  "Providers": {
    "Search": { "Type": "Meilisearch", "Meilisearch": { "Url": "http://crm-meilisearch:7700", "ApiKey": "masterKey" } },
    "Chat": { "Type": "Chatwoot", "Chatwoot": { "BaseUrl": "https://chat.example.com", "ApiKey": "...", "AccountId": "1" } },
    "Notifications": { "Type": "Novu", "Novu": { "ApiKey": "...", "ApplicationId": "..." } },
    "Analytics": { "Type": "Superset", "Superset": { "Url": "https://bi.example.com", "Username": "admin", "Password": "..." } },
    "Signatures": { "Type": "DocuSeal", "DocuSeal": { "Url": "https://sign.example.com", "ApiKey": "..." } },
    "AI": { "Type": "OpenAI", "OpenAI": { "ApiKey": "sk-...", "Model": "gpt-4o" } },
    "Integrations": { "Type": "N8n", "N8n": { "BaseUrl": "https://n8n.example.com", "ApiKey": "..." } }
  }
}
```

### 8.7 On-Prem / Data Center Deployment
**Docker (monolith):**
- Use `docker-compose.yml` on the target host.
- Images from GHCR or local registry: `ghcr.io/alal76/crm-api:<tag>`, `ghcr.io/alal76/crm-frontend:<tag>`.
- Volumes: `db-data` for MariaDB, optional `redis-data`.

**Microservices:**
- Deploy with Docker Compose or Kubernetes.
- Use gateway at port 5000 for northbound traffic.
- Ensure DNS routing for service names `crm-*`.

### 8.8 Kubernetes Standards
| Resource | Naming Pattern | Example |
|----------|----------------|---------|
| Namespace (monolith) | `crm-{env}` | crm-dev, crm-prod |
| Namespace (microservices) | `crm-{env}-ms` | crm-dev-ms |
| Deployment | `{service}-deployment` | api-deployment |
| Service | `{service}-svc` | api-svc |
| Ingress | `{service}-ingress` | api-ingress |

**Legacy namespaces:** `crm-app`, `crm-microservices`, and `crm-production` exist in older manifests; update to `crm-{env}` and `crm-{env}-ms`.

### 8.9 Cloud Provider Standards
**Azure (AKS / App Service):**
- Resource group: `rg-crm-{env}`
- Container registry: `crm{env}acr`
- AKS cluster: `aks-crm-{env}`
- App services: `app-crm-api-{env}`, `app-crm-web-{env}`
- MySQL: `mysql-crm-{env}`
- Key Vault: `kv-crm-{env}`

**AWS (EKS / ECS):**
- VPC: `crm-{env}-vpc`
- EKS cluster: `crm-{env}-cluster`
- ECS service: `crm-{env}-{service}`
- RDS: `crm-{env}-db`
- ECR: `crm/{service}`
- Secrets Manager: `crm-{env}-secrets`

**GCP (GKE / Cloud Run):**
- Project: `crm-{env}`
- GKE cluster: `gke-crm-{env}`
- Cloud Run: `run-crm-{env}-{service}`
- Cloud SQL: `sql-crm-{env}`
- Artifact Registry: `crm-{env}`
- Secret Manager: `crm-{env}-secrets`

### 8.10 Cloud Deployment Options (Azure/AWS/GCP)
| Cloud | Monolith (Containers) | Kubernetes | Database | Load Balancer | Registry |
|-------|------------------------|------------|----------|---------------|----------|
| Azure | App Service for Containers or VM + Docker | AKS | Azure Database for MySQL | Azure Application Gateway | ACR |
| AWS | ECS/Fargate or EC2 + Docker | EKS | RDS MySQL/MariaDB | ALB/NLB | ECR |
| GCP | Cloud Run or GCE + Docker | GKE | Cloud SQL (MySQL) | Cloud Load Balancing | Artifact Registry |

**Notes:**
- Use managed MySQL for production; self-hosted MariaDB for on-prem.
- Use managed Redis (Azure Cache/ElastiCache/Memorystore) when external caching is required.

### 8.11 Docker Artifacts (Current Repository)
**Dockerfiles (docker/):**
- `docker/Dockerfile.backend`
- `docker/Dockerfile.frontend`
- `docker/Dockerfile.frontend.prebuilt`
- `docker/Dockerfile.gateway`
- `docker/Dockerfile.identity`
- `docker/Dockerfile.customer`
- `docker/Dockerfile.sales`
- `docker/Dockerfile.marketing`
- `docker/Dockerfile.servicedesk`
- `docker/Dockerfile.core`

**Compose files (docker/):**
- `docker/docker-compose.yml`
- `docker/docker-compose.production.yml`
- `docker/docker-compose.unified.yml`
- `docker/docker-compose.microservices.unified.yml`
- `docker/docker-compose.providers.yml`
- `docker/docker-compose.databases.yml`
- `docker/docker-compose.sqlserver.yml`
- `docker/docker-compose.ollama.yml`
- `docker/docker-compose.app.yml`

### 8.12 Kubernetes Manifests (Current Repository)
**Monolith (kubernetes/local/):**
- `kubernetes/local/00-namespace.yaml`
- `kubernetes/local/01-database.yaml`
- `kubernetes/local/02-api.yaml`
- `kubernetes/local/03-frontend.yaml`

**Microservices (kubernetes/microservices/):**
- `kubernetes/microservices/00-namespace.yaml`
- `kubernetes/microservices/01-database.yaml`
- `kubernetes/microservices/02-gateway.yaml`
- `kubernetes/microservices/03-identity.yaml`
- `kubernetes/microservices/04-customer.yaml`
- `kubernetes/microservices/05-sales.yaml`
- `kubernetes/microservices/06-marketing.yaml`
- `kubernetes/microservices/07-servicedesk.yaml`
- `kubernetes/microservices/08-core.yaml`
- `kubernetes/microservices/09-frontend.yaml`

**Production (kubernetes/production/):**
- `kubernetes/production/00-namespace-secrets.yaml`
- `kubernetes/production/01-api.yaml`
- `kubernetes/production/02-frontend.yaml`

### 8.13 Helm Charts (Current Repository)
**Monolith chart:**
- `kubernetes/helm/monolith/Chart.yaml`
- `kubernetes/helm/monolith/values.yaml`
- `kubernetes/helm/monolith/templates/_helpers.tpl`
- `kubernetes/helm/monolith/templates/configmap.yaml`
- `kubernetes/helm/monolith/templates/secret.yaml`
- `kubernetes/helm/monolith/templates/pvc.yaml`
- `kubernetes/helm/monolith/templates/db-deployment.yaml`
- `kubernetes/helm/monolith/templates/db-service.yaml`
- `kubernetes/helm/monolith/templates/api-deployment.yaml`
- `kubernetes/helm/monolith/templates/api-service.yaml`
- `kubernetes/helm/monolith/templates/api-hpa.yaml`
- `kubernetes/helm/monolith/templates/frontend-deployment.yaml`
- `kubernetes/helm/monolith/templates/frontend-service.yaml`
- `kubernetes/helm/monolith/templates/frontend-hpa.yaml`
- `kubernetes/helm/monolith/templates/ingress.yaml`

**Microservices chart:**
- `kubernetes/helm/microservices/Chart.yaml`
- `kubernetes/helm/microservices/values.yaml`
- `kubernetes/helm/microservices/templates/_helpers.tpl`
- `kubernetes/helm/microservices/templates/configmap.yaml`
- `kubernetes/helm/microservices/templates/secret.yaml`
- `kubernetes/helm/microservices/templates/db-deployment.yaml`
- `kubernetes/helm/microservices/templates/db-service.yaml`
- `kubernetes/helm/microservices/templates/gateway-deployment.yaml`
- `kubernetes/helm/microservices/templates/gateway-service.yaml`
- `kubernetes/helm/microservices/templates/gateway-hpa.yaml`
- `kubernetes/helm/microservices/templates/identity-deployment.yaml`
- `kubernetes/helm/microservices/templates/identity-service.yaml`
- `kubernetes/helm/microservices/templates/identity-hpa.yaml`
- `kubernetes/helm/microservices/templates/customer-deployment.yaml`
- `kubernetes/helm/microservices/templates/customer-service.yaml`
- `kubernetes/helm/microservices/templates/customer-hpa.yaml`
- `kubernetes/helm/microservices/templates/sales-deployment.yaml`
- `kubernetes/helm/microservices/templates/sales-service.yaml`
- `kubernetes/helm/microservices/templates/sales-hpa.yaml`
- `kubernetes/helm/microservices/templates/marketing-deployment.yaml`
- `kubernetes/helm/microservices/templates/marketing-service.yaml`
- `kubernetes/helm/microservices/templates/marketing-hpa.yaml`
- `kubernetes/helm/microservices/templates/servicedesk-deployment.yaml`
- `kubernetes/helm/microservices/templates/servicedesk-service.yaml`
- `kubernetes/helm/microservices/templates/servicedesk-hpa.yaml`
- `kubernetes/helm/microservices/templates/core-deployment.yaml`
- `kubernetes/helm/microservices/templates/core-service.yaml`
- `kubernetes/helm/microservices/templates/core-hpa.yaml`
- `kubernetes/helm/microservices/templates/frontend-deployment.yaml`
- `kubernetes/helm/microservices/templates/frontend-service.yaml`
- `kubernetes/helm/microservices/templates/frontend-hpa.yaml`
- `kubernetes/helm/microservices/templates/ingress.yaml`

### 8.14 Helm Install/Upgrade Guide
**Monolith (dev):**
```bash
helm upgrade --install crm-monolith kubernetes/helm/monolith \
  --namespace crm-dev --create-namespace \
  --set images.api.repository=ghcr.io/<owner>/crm-api \
  --set images.api.tag=latest \
  --set images.frontend.repository=ghcr.io/<owner>/crm-frontend \
  --set images.frontend.tag=latest
```

**Monolith (prod, managed DB):**
```bash
helm upgrade --install crm-monolith kubernetes/helm/monolith \
  --namespace crm-prod --create-namespace \
  --set db.enabled=false \
  --set secrets.DB_CONNECTION_STRING="<managed-db-connection-string>" \
  --set images.api.repository=ghcr.io/<owner>/crm-api \
  --set images.api.tag=<tag> \
  --set images.frontend.repository=ghcr.io/<owner>/crm-frontend \
  --set images.frontend.tag=<tag>
```

**Microservices (dev):**
```bash
helm upgrade --install crm-microservices kubernetes/helm/microservices \
  --namespace crm-dev-ms --create-namespace \
  --set images.gateway.repository=ghcr.io/<owner>/crm-gateway \
  --set images.gateway.tag=latest \
  --set images.frontend.repository=ghcr.io/<owner>/crm-frontend \
  --set images.frontend.tag=latest
```

**Microservices (prod, managed DB):**
```bash
helm upgrade --install crm-microservices kubernetes/helm/microservices \
  --namespace crm-prod-ms --create-namespace \
  --set db.enabled=false \
  --set config.DB_HOST="<managed-db-host>" \
  --set secrets.DB_USER="<db-user>" \
  --set secrets.DB_PASSWORD="<db-password>" \
  --set images.gateway.repository=ghcr.io/<owner>/crm-gateway \
  --set images.gateway.tag=<tag>
```

**Resource/HPA tuning:**
- Configure resource presets per component via the `resources` section in `values.yaml`.
- Enable autoscaling per component via the `hpa` section in `values.yaml`.

### 8.15 Health & Observability
- Health endpoints: `/health`, `/health/ready`, `/health/live`, `/api/health/providers`.
- Worker endpoints: `/api/workers/health`, `/api/workers/stats`, `/api/workers/control/*`.
- Logs: structured logging with provider category tags.

### 8.16 Security
- JWT secret must be at least 32 chars.
- Do not expose DB/Redis ports publicly; use internal network.
- TLS termination at ingress or reverse proxy.

---

## 9. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-16 | Abhishek Lal | Initial specification |
