# GitHub Copilot Instructions - CRM Solution

> **Last Updated:** February 8, 2026  
> **Load this file at the start of every agent session**

---

## � Feature Specification Framework

### IMPORTANT: Service Implementation Guide

**Before implementing ANY feature, consult the feature specification:**

📁 **[docs/specifications/INDEX.md](../docs/specifications/INDEX.md)** - Master index of all specifications

### Specification Structure

Each feature has a specification file (`SPEC-{MODULE}-{SEQ}-{Name}.md`) with full traceability:

| Section | Contents |
|---------|----------|
| **1. Business Context** | Sub-features, functionalities, use cases |
| **2. Frontend** | Pages, components, services, validations (with ❌ Not Implemented markers) |
| **3. Backend** | Entities, DTOs, interfaces, services, controllers, endpoints, validations |
| **4. Database** | Tables, columns, data types, constraints, relationships, indexes |
| **5. Tests** | Unit tests, integration tests, E2E tests |
| **6. Issues** | Naming inconsistencies, validation gaps |
| **7. TODOs** | Extracted to [MASTER_TODO_LIST.md](../docs/MASTER_TODO_LIST.md) |

### Before Writing Code

1. **Find the spec:** `docs/specifications/SPEC-{MODULE}-{SEQ}-{FeatureName}.md`
2. **Check implementation status:** Look for ✅ Implemented, ⚠️ Partial, ❌ Not Found markers
3. **Follow validations:** Use exact rules from spec (both frontend AND backend)
4. **Match data types:** Follow entity property types exactly
5. **Update spec:** Add any new TODO items, mark items as implemented

### Creating New Features

1. **Create spec first:** Copy `SPEC-TEMPLATE.md` → `SPEC-{MODULE}-{SEQ}-{Name}.md`
2. **Document before coding:** Fill in business context, planned implementation
3. **Mark ❌ Not Implemented:** For all items pending implementation
4. **Extract TODOs:** Add all TODO-{SPEC}-{SEQ} items to master list
5. **Implement:** Follow the spec exactly
6. **Update spec:** Mark items as ✅ Implemented when complete

---

## �🚨 Active Remediation Plan

**IMPORTANT:** An active remediation plan exists to address solution gaps. Before starting new development work, review:

📋 **[SOLUTION_GAPS_REMEDIATION_PLAN.md](../docs/SOLUTION_GAPS_REMEDIATION_PLAN.md)**

| Phase | Priority | Description |
|-------|----------|-------------|
| Phase 1 | 🔴 High | ITSM Module Completion (40% → 100%) |
| Phase 2 | 🔴 High | Missing Services Implementation |
| Phase 3 | 🟡 Medium | API Controllers Completion |
| Phase 4 | 🟡 Medium | Frontend Components |
| Phase 5 | 🟡 Medium | Test Coverage Expansion |
| Phase 6 | 🟢 Low | Integration & Webhook Enhancements |
| Phase 7 | 🟢 Low | AI/Analytics Enhancements |
| Phase 8 | 🟢 Low | Documentation & Polish |

**Current Progress:** 0% (Plan Created February 8, 2026)

---

## 1. Solution Overview

### What is this?

A full-stack enterprise CRM (Customer Relationship Management) solution with:

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core 8.0 + Entity Framework Core 8.0 |
| **Frontend** | React 18 + TypeScript + Material-UI 5 |
| **Database** | MariaDB (primary), SQL Server, PostgreSQL supported |
| **Caching** | Redis |
| **Real-time** | SignalR WebSocket |
| **AI/LLM** | Multi-provider (Ollama, OpenAI, Azure, Anthropic, Bedrock) |
| **Architecture** | Hexagonal (Ports & Adapters) with Pluggable Providers |

### Repository Structure

```
crm-solution/
├── CRM.Backend/                    # .NET Backend
│   ├── src/
│   │   ├── CRM.Api/                # REST API (Controllers, Middleware)
│   │   ├── CRM.Core/               # Domain (Entities, DTOs, Ports)
│   │   ├── CRM.Infrastructure/     # Data Access, Services, Providers
│   │   └── Services/               # Microservices (Gateway, Identity, etc.)
│   └── tests/                      # Unit & Integration Tests
├── CRM.Frontend/                   # React SPA
│   └── src/
│       ├── components/             # Reusable UI components
│       ├── pages/                  # Route-level components
│       ├── services/               # API service layer (Axios)
│       └── store/                  # Redux state management
├── CRM.Infrastructure/             # Deployment Tools
│   └── deployment-tool/            # Configuration wizard
├── database/                       # SQL Schema & Seeds
├── docker/                         # Docker configurations
├── kubernetes/                     # K8s manifests
└── docs/                           # Documentation
```

---

## 2. Naming Conventions

### 2.1 Code Naming

| Element | Convention | Example |
|---------|------------|---------|
| **Classes** | PascalCase | `AccountService`, `OpportunityDto` |
| **Interfaces** | I + PascalCase | `IAccountService`, `ISearchPort` |
| **Methods** | PascalCase | `GetAllAsync`, `CreateAsync` |
| **Properties** | PascalCase | `FirstName`, `CreatedAt` |
| **Private fields** | _camelCase | `_logger`, `_dbContext` |
| **Constants** | PascalCase | `MaxRetryCount`, `DefaultTimeout` |
| **Enums** | PascalCase | `UserRole`, `OpportunityStage` |
| **Enum values** | PascalCase | `UserRole.Admin`, `Stage.Negotiation` |

### 2.2 File Naming

| Type | Pattern | Example |
|------|---------|---------|
| **Entity** | `{Name}.cs` | `Account.cs`, `Opportunity.cs` |
| **DTO** | `{Name}Dto.cs` | `AccountDto.cs`, `CreateAccountDto.cs` |
| **Service** | `{Name}Service.cs` | `AccountService.cs` |
| **Controller** | `{Name}Controller.cs` | `AccountsController.cs` (plural) |
| **Interface** | `I{Name}.cs` | `IAccountService.cs` |
| **Provider** | `{Provider}{Category}Provider.cs` | `MeilisearchSearchProvider.cs` |
| **Factory** | `{Category}ProviderFactory.cs` | `SearchProviderFactory.cs` |
| **Test** | `{Name}Tests.cs` | `AccountServiceTests.cs` |

### 2.3 Database Naming

| Element | Convention | Example |
|---------|------------|---------|
| **Tables** | PascalCase plural | `Accounts`, `Opportunities` |
| **Columns** | PascalCase | `FirstName`, `CreatedAt` |
| **Primary Key** | `Id` | `Id` (int, auto-increment) |
| **Foreign Key** | `{Entity}Id` | `AccountId`, `UserId` |
| **Junction Table** | `{Entity1}{Entity2}` | `AccountContacts` |
| **Index** | `IX_{Table}_{Column}` | `IX_Accounts_Email` |

### 2.4 API Naming

| Element | Convention | Example |
|---------|------------|---------|
| **Endpoints** | lowercase plural | `/api/accounts`, `/api/opportunities` |
| **Actions** | REST verbs | GET, POST, PUT, PATCH, DELETE |
| **Query params** | camelCase | `?pageSize=20&sortBy=name` |
| **Route params** | `{id}` | `/api/accounts/{id}` |

---

## 3. Infrastructure & Network

### 3.1 Development Server (192.168.0.9)

| Container | Port | Network Alias | Purpose |
|-----------|------|---------------|---------|
| `crm-api` | 5000 | crm-api | .NET Web API (Monolith) |
| `crm-frontend` | 80 | crm-frontend | React app (Nginx) |
| `crm-mariadb` | 3306 | crm-mariadb | MariaDB database |
| `crm-redis` | 6379 | crm-redis | Redis cache |
| `crm-meilisearch` | 7700 | crm-meilisearch | Search engine |
| `crm-ollama` | 11434 | crm-ollama | Local LLM |

**Docker Network:** `docker_crm-network` (bridge)

### 3.2 Microservices Architecture

| Service | Port | Image | Domain |
|---------|------|-------|--------|
| `crm-gateway` | 5000 | crm-gateway | YARP API Gateway |
| `crm-identity` | 5001 | crm-identity | Auth, Users, Groups |
| `crm-customer` | 5002 | crm-customer | Accounts, Contacts |
| `crm-sales` | 5003 | crm-sales | Opportunities, Quotes |
| `crm-marketing` | 5004 | crm-marketing | Campaigns, Leads |
| `crm-servicedesk` | 5005 | crm-servicedesk | Tickets, Workflows |
| `crm-core` | 5006 | crm-core | Settings, Monitoring |

### 3.3 Azure Resources Naming

| Resource | Naming Pattern | Example |
|----------|----------------|---------|
| **Resource Group** | `rg-crm-{env}` | `rg-crm-dev`, `rg-crm-prod` |
| **Container Registry** | `crm{env}acr` | `crmdevacr`, `crmprodacr` |
| **App Service Plan** | `asp-crm-{env}` | `asp-crm-dev` |
| **App Service (API)** | `app-crm-api-{env}` | `app-crm-api-prod` |
| **App Service (Web)** | `app-crm-web-{env}` | `app-crm-web-prod` |
| **MySQL Server** | `mysql-crm-{env}` | `mysql-crm-prod` |
| **Key Vault** | `kv-crm-{env}` | `kv-crm-prod` |
| **App Insights** | `ai-crm-{env}` | `ai-crm-prod` |
| **Log Analytics** | `log-crm-{env}` | `log-crm-prod` |
| **Storage Account** | `stcrm{env}` | `stcrmdev`, `stcrmprod` |
| **AKS Cluster** | `aks-crm-{env}` | `aks-crm-prod` |
| **Redis Cache** | `redis-crm-{env}` | `redis-crm-prod` |

### 3.4 AWS Resources Naming

| Resource | Naming Pattern | Example |
|----------|----------------|---------|
| **VPC** | `crm-{env}-vpc` | `crm-prod-vpc` |
| **Subnet** | `crm-{env}-{tier}-{az}` | `crm-prod-public-1a` |
| **ECS Cluster** | `crm-{env}-cluster` | `crm-prod-cluster` |
| **ECS Service** | `crm-{env}-{service}` | `crm-prod-api` |
| **RDS Instance** | `crm-{env}-db` | `crm-prod-db` |
| **S3 Bucket** | `crm-{env}-{purpose}` | `crm-prod-uploads` |
| **ECR Repository** | `crm/{service}` | `crm/api`, `crm/frontend` |
| **ALB** | `alb-crm-{env}` | `alb-crm-prod` |
| **Security Group** | `sg-crm-{env}-{purpose}` | `sg-crm-prod-api` |
| **IAM Role** | `role-crm-{env}-{purpose}` | `role-crm-prod-ecs-task` |

### 3.5 GCP Resources Naming

| Resource | Naming Pattern | Example |
|----------|----------------|---------|
| **Project** | `crm-{env}` | `crm-prod` |
| **VPC Network** | `vpc-crm-{env}` | `vpc-crm-prod` |
| **Subnet** | `subnet-crm-{env}-{region}` | `subnet-crm-prod-us-central1` |
| **GKE Cluster** | `gke-crm-{env}` | `gke-crm-prod` |
| **Cloud SQL** | `sql-crm-{env}` | `sql-crm-prod` |
| **Cloud Storage** | `gs-crm-{env}-{purpose}` | `gs-crm-prod-uploads` |
| **Cloud Run** | `run-crm-{env}-{service}` | `run-crm-prod-api` |

### 3.6 Kubernetes Resources Naming

| Resource | Naming Pattern | Example |
|----------|----------------|---------|
| **Namespace** | `crm-{env}` | `crm-prod`, `crm-staging` |
| **Deployment** | `{service}-deployment` | `api-deployment` |
| **Service** | `{service}-svc` | `api-svc` |
| **ConfigMap** | `{service}-config` | `api-config` |
| **Secret** | `{service}-secrets` | `api-secrets` |
| **Ingress** | `{service}-ingress` | `api-ingress` |
| **PVC** | `{service}-pvc` | `db-pvc` |
| **ServiceAccount** | `sa-{service}` | `sa-api` |
| **HPA** | `{service}-hpa` | `api-hpa` |

---

## 4. Database Schema

### 4.1 Core Tables (~95 tables total)

```
CORE ENTITIES:
├── Users                    # User accounts
├── UserGroups               # Role-based groups
├── UserGroupMembers         # Junction: Users ↔ Groups
├── Departments              # Organizational units
└── SystemSettings           # Application configuration

CRM ENTITIES:
├── Customers                # Accounts (organizations) - Note: Table named Customers
├── Contacts                 # Individual people
├── AccountContacts          # Junction: Accounts ↔ Contacts
├── Leads                    # Sales leads
├── Opportunities            # Sales pipeline
├── OpportunityProducts      # Junction: Opportunities ↔ Products
├── Products                 # Product catalog
└── Interactions             # Activity tracking

CONTACT INFO (Polymorphic):
├── Addresses                # Physical addresses
├── PhoneNumbers             # Phone records
├── EmailAddresses           # Email records
├── SocialMediaAccounts      # Social profiles
├── EntityAddressLinks       # Any entity ↔ Address
├── EntityPhoneLinks         # Any entity ↔ Phone
├── EntityEmailLinks         # Any entity ↔ Email
└── EntitySocialMediaLinks   # Any entity ↔ Social

SALES:
├── Quotes                   # Sales quotes
├── QuoteLineItems           # Quote details
├── Orders                   # Sales orders
├── Invoices                 # Invoices
├── Payments                 # Payment records
├── Contracts                # Customer contracts
└── Subscriptions            # Recurring revenue

MARKETING:
├── MarketingCampaigns       # Campaign definitions
├── CampaignRecipients       # Campaign targets
├── CampaignMetrics          # Performance data
├── EmailTemplates           # Email templates
├── EmailSequences           # Drip campaigns
└── CampaignConversions      # Conversion tracking

SERVICE DESK (ITSM):
├── ServiceRequests          # Support tickets
├── ServiceRequestCategories # Ticket categories
├── KnowledgeArticles        # KB articles
├── SLAPolicies              # SLA definitions
├── EscalationRules          # Escalation rules
└── WorkflowDefinitions      # Workflow engine
```

### 4.2 Database Credentials (Development)

```bash
# MariaDB
DB_HOST=crm-mariadb
DB_PORT=3306
DB_NAME=crm_db
DB_USER=crm_user
DB_PASSWORD=CrmPass@Dev2024
DB_ROOT_PASSWORD=RootPass@Dev2024

# Connection String
Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;
```

### 4.3 Entity Conventions

```csharp
// All entities inherit from BaseEntity
public class BaseEntity
{
    public int Id { get; set; }              // Primary key
    public DateTime CreatedAt { get; set; }  // Created timestamp
    public DateTime UpdatedAt { get; set; }  // Modified timestamp
    public bool IsDeleted { get; set; }      // Soft delete flag
    public byte[] RowVersion { get; set; }   // Optimistic concurrency
}
```

---

## 5. Pluggable Architecture

### 5.1 Overview

The CRM implements **Hexagonal Architecture** (Ports & Adapters) with feature-flag-driven provider selection.

```
┌─────────────────────────────────────────────────────────────────┐
│                    APPLICATION CORE                              │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  CRM.Core/Ports/Output/Providers/                         │  │
│  │  ├── ISearchPort.cs          (Search abstraction)         │  │
│  │  ├── IChatPort.cs            (Chat abstraction)           │  │
│  │  ├── INotificationPort.cs    (Notification abstraction)   │  │
│  │  ├── IAnalyticsPort.cs       (Analytics abstraction)      │  │
│  │  ├── ISignaturePort.cs       (E-signature abstraction)    │  │
│  │  ├── IAIPort.cs              (AI/LLM abstraction)         │  │
│  │  └── IIntegrationPort.cs     (Integration abstraction)    │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    PROVIDER FACTORIES                            │
│  CRM.Infrastructure/Factories/                                   │
│  ├── SearchProviderFactory.cs                                   │
│  ├── ChatProviderFactory.cs                                     │
│  ├── NotificationProviderFactory.cs                             │
│  ├── AnalyticsProviderFactory.cs                                │
│  ├── SignatureProviderFactory.cs                                │
│  ├── AIProviderFactory.cs                                       │
│  └── IntegrationProviderFactory.cs                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    PROVIDER IMPLEMENTATIONS                      │
│  CRM.Infrastructure/Providers/                                   │
│  ├── BuiltIn/           (Default implementations)               │
│  ├── Meilisearch/       (Search)                                │
│  ├── Algolia/           (Search - SaaS)                         │
│  ├── Chatwoot/          (Chat - OSS)                            │
│  ├── Intercom/          (Chat - SaaS)                           │
│  ├── Novu/              (Notifications - OSS)                   │
│  ├── Twilio/            (Notifications - SaaS)                  │
│  ├── SendGrid/          (Email - SaaS)                          │
│  ├── DocuSeal/          (Signatures - OSS)                      │
│  ├── DocuSign/          (Signatures - SaaS)                     │
│  ├── Superset/          (Analytics - OSS)                       │
│  ├── PowerBI/           (Analytics - SaaS)                      │
│  └── AI/ (Ollama, OpenAI, Azure, Anthropic, Bedrock, OpenRouter)│
└─────────────────────────────────────────────────────────────────┘
```

### 5.2 Feature Flags

Location: `CRM.Core/Features/FeatureFlags.cs`

```csharp
public static class FeatureFlags
{
    // Provider Selection (when true, uses external provider)
    public const string UseExternalChat = "UseExternalChat";
    public const string UseExternalSearch = "UseExternalSearch";
    public const string UseExternalNotifications = "UseExternalNotifications";
    public const string UseExternalAnalytics = "UseExternalAnalytics";
    public const string UseExternalSignatures = "UseExternalSignatures";
    public const string UseExternalAI = "UseExternalAI";
    public const string UseExternalIntegrations = "UseExternalIntegrations";
    
    // Module Enablement
    public const string EnableITSM = "EnableITSM";
    public const string EnableMarketing = "EnableMarketing";
    public const string EnableCustomerPortal = "EnableCustomerPortal";
    public const string EnablePartnerPortal = "EnablePartnerPortal";
    public const string EnableKnowledgeBase = "EnableKnowledgeBase";
}
```

### 5.3 Provider Types

Location: `CRM.Core/Features/ProviderTypes.cs`

| Category | Providers |
|----------|-----------|
| **Search** | BuiltIn, Meilisearch, Algolia, Typesense, Elasticsearch, AzureCognitiveSearch |
| **Chat** | BuiltIn, Chatwoot, Intercom, Zendesk, Freshchat, RocketChat |
| **Notifications** | BuiltIn, Novu, Twilio, SendGrid, OneSignal, Courier, AWSSES |
| **Analytics** | BuiltIn, Superset, Metabase, PowerBI, Looker, QuickSight |
| **Signatures** | BuiltIn, DocuSeal, DocuSign, AdobeSign, HelloSign |
| **AI** | Ollama, OpenAI, AzureOpenAI, Anthropic, Bedrock, OpenRouter, Gemini |
| **Integrations** | BuiltIn, N8n, Zapier, Make, Workato |

### 5.4 Configuration Schema

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
    "Search": { 
      "Type": "Meilisearch", 
      "Meilisearch": { 
        "Url": "http://crm-meilisearch:7700",
        "ApiKey": "masterKey"
      } 
    },
    "Chat": { 
      "Type": "Chatwoot", 
      "Chatwoot": { 
        "BaseUrl": "https://chat.example.com",
        "ApiKey": "...",
        "AccountId": "1"
      } 
    },
    "Notifications": { 
      "Type": "Novu", 
      "Novu": { 
        "ApiKey": "...",
        "ApplicationId": "..."
      } 
    },
    "Analytics": { 
      "Type": "Superset", 
      "Superset": { 
        "Url": "https://bi.example.com",
        "Username": "admin",
        "Password": "..."
      } 
    },
    "Signatures": { 
      "Type": "DocuSeal", 
      "DocuSeal": { 
        "Url": "https://sign.example.com",
        "ApiKey": "..."
      } 
    },
    "AI": { 
      "Type": "OpenAI", 
      "OpenAI": { 
        "ApiKey": "sk-...", 
        "Model": "gpt-4o" 
      },
      "Ollama": {
        "Url": "http://crm-ollama:11434",
        "Model": "llama3"
      },
      "AzureOpenAI": {
        "Endpoint": "https://xxx.openai.azure.com/",
        "ApiKey": "...",
        "DeploymentName": "gpt-4o"
      }
    },
    "Integrations": { 
      "Type": "N8n", 
      "N8n": { 
        "BaseUrl": "https://n8n.example.com",
        "ApiKey": "..."
      } 
    }
  }
}
```

---

## 6. Authentication & Security

### 6.1 JWT Configuration

```csharp
// Token settings
JWT_SECRET    = <minimum 32 characters>
JWT_ISSUER    = "CRM.Api"
JWT_AUDIENCE  = "CRM.Client"
EXPIRATION    = 60 minutes (access token)
REFRESH_EXPIRY = 7 days
ALGORITHM     = HmacSha256
```

### 6.2 Password Hashing

```csharp
// BCrypt.Net - NOT ASP.NET Identity
BCrypt.Net.BCrypt.HashPassword(password)
BCrypt.Net.BCrypt.Verify(password, hash)
```

### 6.3 Default Admin User

```bash
ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@crm.local
ADMIN_PASSWORD=Admin@123
ADMIN_ROLE=Admin
```

### 6.4 Health Endpoints (No Auth Required)

```bash
GET /health       # Liveness probe
GET /health/ready # Readiness probe  
GET /health/live  # Kubernetes liveness
```

---

## 7. API Endpoints

### 7.1 Core CRUD Pattern

All entities follow this pattern:

```
GET    /api/{entity}              # List all (paginated)
GET    /api/{entity}/{id}         # Get by ID
POST   /api/{entity}              # Create new
PUT    /api/{entity}/{id}         # Full update
PATCH  /api/{entity}/{id}         # Partial update
DELETE /api/{entity}/{id}         # Soft delete
```

### 7.2 Key Endpoints

| Endpoint | Purpose |
|----------|---------|
| `POST /api/auth/login` | Authenticate user |
| `POST /api/auth/refresh` | Refresh access token |
| `GET /api/accounts` | List accounts |
| `GET /api/contacts` | List contacts |
| `GET /api/opportunities` | List opportunities |
| `GET /api/leads` | List leads |
| `GET /api/products` | List products |
| `GET /api/campaigns` | List campaigns |
| `GET /api/servicerequests` | List tickets |
| `GET /api/users` | List users |
| `GET /api/settings` | System settings |
| `GET /api/dashboard` | Dashboard data |
| `GET /api/admin/features` | Feature flags status |
| `GET /api/health/providers` | Provider health status |

### 7.3 Pagination

```json
// Request
GET /api/accounts?page=1&pageSize=20&sortBy=name&sortOrder=asc

// Response
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

---

## 8. Frontend Architecture

### 8.1 Key Technologies

| Library | Version | Purpose |
|---------|---------|---------|
| React | 18.x | UI Framework |
| TypeScript | 5.x | Type safety |
| Material-UI | 5.x | Component library |
| React Router | 6.x | Client routing |
| Redux Toolkit | 2.x | State management |
| Axios | 1.x | HTTP client |
| Formik + Yup | - | Form handling |
| SignalR | - | Real-time updates |

### 8.2 Directory Structure

```
src/
├── components/
│   ├── common/           # Shared components (DataGrid, Form, etc.)
│   ├── accounts/         # Account components
│   ├── contacts/         # Contact components
│   └── ...
├── pages/
│   ├── AccountsPage.tsx  # Route-level components
│   ├── DashboardPage.tsx
│   └── ...
├── services/
│   ├── api.ts            # Axios instance
│   ├── accountService.ts # API calls
│   └── ...
├── store/
│   ├── index.ts          # Redux store
│   ├── authSlice.ts      # Auth state
│   └── ...
├── contexts/
│   ├── AuthContext.tsx   # Auth provider
│   ├── SignalRContext.tsx # Real-time
│   └── ...
└── hooks/
    ├── useAuth.ts
    ├── usePagination.ts
    └── ...
```

---

## 9. Build & Deploy Commands

### 9.1 Backend

```bash
# Build
cd CRM.Backend && dotnet build

# Run tests
cd CRM.Backend && dotnet test

# Run locally
cd CRM.Backend/src/CRM.Api && dotnet run

# Build Docker image (cross-platform for Linux server)
docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend .

# Build microservices
cd CRM.Backend && dotnet build CRM.Microservices.sln
```

### 9.2 Frontend

```bash
# Install dependencies
cd CRM.Frontend && npm install

# Development server
cd CRM.Frontend && npm start

# Production build
cd CRM.Frontend && npm run build

# Run tests
cd CRM.Frontend && npm test
```

### 9.3 Docker Compose

```bash
# Start all services (monolith)
docker-compose -f docker/docker-compose.yml up -d

# Start microservices
docker-compose -f docker/docker-compose.microservices.unified.yml up -d

# Start with providers (Meilisearch, Novu, etc.)
docker-compose -f docker/docker-compose.providers.yml up -d

# View logs
docker-compose -f docker/docker-compose.yml logs -f crm-api
```

### 9.4 Database

```bash
# Setup database
./database/setup-database.sh --provider mariadb --host localhost

# Apply migrations
cd CRM.Backend && dotnet ef database update

# Generate migration
cd CRM.Backend && dotnet ef migrations add MigrationName
```

### 9.5 Deployment Tool GUI

```bash
# Start GUI wizard for deployment configuration
cd CRM.Infrastructure/deployment-tool && ./start-gui.sh
# Open http://localhost:5050/wizard
```

---

## 10. Testing Standards

### 10.1 Test Organization

```
tests/
├── CRM.Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   │   ├── AccountServiceTests.cs
│   │   │   └── ...
│   │   └── Providers/
│   │       ├── BuiltInSearchProviderTests.cs
│   │       └── ...
│   └── Integration/
│       ├── AccountsControllerTests.cs
│       └── ...
└── e2e-tests/
    └── tests/
        ├── auth.spec.ts
        └── accounts.spec.ts
```

### 10.2 Test Naming Convention

```csharp
// Pattern: {Method}_Should{ExpectedBehavior}_When{Condition}
[Fact]
public async Task GetById_ShouldReturnAccount_WhenAccountExists()

[Fact]
public async Task Create_ShouldThrowValidationException_WhenNameIsEmpty()
```

### 10.3 Running Tests

```bash
# All tests
cd CRM.Backend && dotnet test

# Specific project
cd CRM.Backend && dotnet test tests/CRM.Tests

# With coverage
cd CRM.Backend && dotnet test --collect:"XPlat Code Coverage"

# E2E tests
cd e2e-tests && npx playwright test
```

---

## 11. Documentation References

| Document | Purpose |
|----------|---------|
| [SOLUTION_CONTEXT.md](../SOLUTION_CONTEXT.md) | Complete technical reference |
| [ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md) | System architecture |
| [MICROSERVICES_ARCHITECTURE.md](../MICROSERVICES_ARCHITECTURE.md) | Microservices guide |
| [docs/architecture/ADR-001-*.md](../docs/architecture/) | Architecture decisions |
| [docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md](../docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md) | Implementation progress |
| [docs/PHASE4_SERVICE_SPECIFICATIONS.md](../docs/PHASE4_SERVICE_SPECIFICATIONS.md) | **Phase 4 Service Interface Specifications** |
| [database/DATABASE_SCHEMA.md](../database/DATABASE_SCHEMA.md) | Database schema |
| [azure/AZURE_DEPLOYMENT.md](../azure/AZURE_DEPLOYMENT.md) | Azure deployment guide |

---

## 11.1 Service Implementation Guidelines

**CRITICAL:** When implementing services, ALWAYS refer to [docs/PHASE4_SERVICE_SPECIFICATIONS.md](../docs/PHASE4_SERVICE_SPECIFICATIONS.md) for:

1. **Exact method signatures** - Parameter names, types, default values, return types
2. **Supporting types** - DTOs, enums, statistics classes defined in interfaces
3. **Common patterns** - CancellationToken usage, soft delete, timestamps

### Service Implementation Checklist

- [ ] Method signature matches interface EXACTLY (parameter names, types, defaults)
- [ ] All CancellationTokens passed to async database operations
- [ ] Use `IsDeleted = true` for soft deletes, never hard delete
- [ ] Set `CreatedAt` on create, `UpdatedAt` on update
- [ ] Inject `ICrmDbContext` and `ILogger<T>`
- [ ] Return types match interface (nullable where specified)
- [ ] Supporting types defined in interface file, not duplicated

---

## 12. Important Notes

### 12.1 Customer → Account Migration

The solution underwent a major refactoring where `Customer` was renamed to `Account`:
- **Entity**: `Account.cs` (but database table still named `Customers`)
- **API**: `/api/accounts` (not `/api/customers`)
- **Frontend**: `AccountsPage.tsx`

### 12.2 Code Preservation Rule

**NEVER DELETE EXISTING CODE** - Always refactor to BuiltIn providers:
- Existing search → `BuiltInSearchProvider`
- Existing email → `BuiltInNotificationProvider`
- Existing reports → `BuiltInAnalyticsProvider`

### 12.3 MariaDB Row Size Limit

MariaDB has a 65535 byte row limit. The solution includes fixes in `CrmDbContext.OnModelCreating()` to handle this.

### 12.4 Feature Flag Names

Microsoft.FeatureManagement does **NOT** allow colons (`:`) in feature names. Use flat names like `UseExternalSearch` instead of `Providers:Search:External`.

### 12.5 Cross-Platform Builds

Development on Mac (arm64) deploying to Linux server (amd64):
```bash
docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend .
```

---

## 13. Quick Reference

### Environment Variables

```bash
# Runtime
ASPNETCORE_ENVIRONMENT=Development|Production

# Database
DatabaseProvider=mariadb|sqlserver|postgresql
ConnectionStrings__DefaultConnection=<connection-string>

# JWT
Jwt__Secret=<min-32-chars>
Jwt__Issuer=CRM.Api
Jwt__Audience=CRM.Client

# Redis
Redis__ConnectionString=crm-redis:6379

# Admin seeding
ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@crm.local
ADMIN_PASSWORD=<password>

# AI Providers
AI__Provider=Ollama|OpenAI|Azure|Anthropic
AI__OpenAI__ApiKey=<key>
AI__AzureOpenAI__Endpoint=<endpoint>

# Feature Flags
FeatureManagement__UseExternalSearch=false
FeatureManagement__UseExternalAI=true
```

### Common Issues

| Issue | Solution |
|-------|----------|
| Port 5000 in use | `lsof -i :5000 && kill -9 <PID>` |
| DB connection failed | Check `crm-mariadb` container is running |
| JWT invalid | Ensure `JWT_SECRET` is at least 32 characters |
| CORS error | Check `AllowedOrigins` in appsettings.json |
| EF Core tracking error | Use `.AsNoTracking()` or detach entity |
| Provider not found | Check feature flag and provider type in config |

### Useful Commands

```bash
# Check container status
docker ps -a | grep crm

# View API logs
docker logs -f crm-api

# Enter database
docker exec -it crm-mariadb mysql -u crm_user -p crm_db

# Check provider health
curl http://localhost:5000/api/health/providers

# Test auth
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@crm.local","password":"Admin@123"}'
```

---

**END OF COPILOT INSTRUCTIONS**
