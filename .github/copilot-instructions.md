# GitHub Copilot Instructions - CRM Solution

> **Last Updated:** March 1, 2026  
> **Current Version:** 0.614.11  
> **Load this file at the start of every agent session**

Copilot usage
- Use Claude Opus 4.6 subagents for all tasks in particular for research/analysis and test authoring when tasks are complex or multi-step
- Update documentation as you proceed (specs, remediation plan, and related docs).
- **Enum changes:** whenever you introduce a new public enum or modify existing ones, update the relevant feature spec and the centralized enum reference (`docs/11-specifications/SPEC-GEN-001-EnumReference.md`), and ensure corresponding unit tests (enum value counts/values) are added or updated. Record enum field gaps in FIELD_GAP_REMEDIATION_PLAN.md.
- **Test authoring — MANDATORY rule:** Write unit/integration tests ONLY after first reading and verifying the actual code being tested. Before writing any test, confirm: (1) the exact class name, namespace, and constructor signature of the service/class under test, (2) the exact method signatures and parameter names, (3) which interfaces are injected and their concrete implementations, (4) what DTOs/entities are used including nullable fields. Never write tests based on assumed or inferred method signatures — always grep/read the real code first. Failing to do this causes compilation errors and wasted effort.
- Write and validate unit tests at the end of the task to ensure code quality and correctness.
- Clean up terminals when done to maintain an organized workspace..
- Where possible, use the provided documentation and 11-specifications to guide your implementation and ensure consistency with the project's standards and requirements.
- where possible reuse terminals for related tasks to maintain context and reduce setup time, but feel free to open new terminals for unrelated tasks or when it helps keep things organized.
- for deployments check the target architecture every time , for docker builds remove the old images before building the new ones to avoid confusion and ensure the latest code is being used. For Kubernetes deployments, ensure you are in the correct context and namespace before applying manifests or helm charts.
- Create a problems and solutions tracking file where common problems and solutions are tracked - for common problems write helper scripts to automate the solution and add it to the repository for future use. Name this file common_development_issues.md in the /docs folder and keep it updated with any new issues encountered and their solutions, this will help future developers who might encounter the same issues and also help in tracking recurring problems that might indicate underlying issues in the codebase or development process.update link to that file here once it's created.

- Set current version of the solution to ver 0.560.1 and update it with every new feature or significant change on commit, this will help in tracking the evolution of the solution and also in communicating the current state of the project to all stakeholders. update version .json on every commit . Depending on size of change evaluate if the version should be minor or patch and update accordingly, for example if it's a small bug fix that doesn't add any new features it would be a patch update (0.560.2) but if it's a new feature or a significant change it would be a minor update (0.561.0).
- after first version update change this copilot version to match what is updated into the version.json file to ensure consistency and clarity in communication about the current version of the solution. This will help in tracking which version of the codebase is currently being worked on and also in communicating with other team members about the state of the project.

- Do not delete any code without a very good reason, if you think some code is not needed or can be improved, first check if it's being used anywhere in the codebase, if it's not being used and you are sure it can be removed then mark it as dead code for deletion, but if it's being used or you are not sure about its usage then it's better to keep it and maybe mark it as deprecated or add comments for future reference. This will help in maintaining the integrity of the codebase and also in avoiding any unintended consequences of deleting code that might still be needed.
- Clean up stylecop and sonarqube warnings where possible, but if there are warnings that require significant refactoring or changes to the codebase, it's better to create a separate task for them and prioritize them accordingly rather than trying to fix them in the middle of implementing a new feature or change. This will help in maintaining focus on the current task and also in ensuring that any refactoring or changes are done in a controlled and deliberate manner.



## � Feature Specification Framework

## 1.1 Field Gap Audit & Architecture Alignment (MANDATORY)

**Field Gap Remediation and Audit Policy**

To ensure ongoing data model and contract integrity, the following architecture and development rules are MANDATORY for all contributors:

- **Field Gap Audit:**
  - All new features, entities, DTOs, and frontend types must be checked against the latest FIELD_GAP_REMEDIATION_PLAN.md.
  - Any new or changed fields must be reflected in the plan before merging.
  - All layers (DB, backend entity, DTO, frontend type, UI) must be aligned for every field.

- **DTO Layer Enforcement:**
  - All API endpoints must use DTOs as the contract layer, never raw entities.
  - DTOs must be a superset of the fields returned by the API.

- **Enum Alignment:**
  - All enums must use numeric values in API/DTO contracts.
  - Frontend must map these to TypeScript enums or string unions.

- **Date Serialization:**
  - All date fields must use ISO 8601 strings in API responses.
  - Frontend must handle `string | null` for all date fields.

- **Field Naming Conventions:**
  - Avoid mismatches (e.g., subject/title, estimatedMinutes/estimatedHours).
  - Follow the DTO naming standard as defined in FIELD_GAP_REMEDIATION_PLAN.md.
  - Implement defensive coding practices to handle potential nullability and type mismatches gracefully.

- **UI Accordion Strategy:**
  - All forms must use a two-section layout: core fields in the main form, optional/secondary fields in a collapsible "Additional Information" accordion at the bottom.

- **Automated Alignment Checks:**
  - Add or maintain build-time tests that verify DTO public properties are a superset of the fields returned by the API (contract testing).

- **Documentation:**
  - Maintain FIELD_GAP_REMEDIATION_PLAN.md as a living specification. Update it with every field addition, removal, or change.

**Failure to comply with these rules will result in code review rejection.**

### IMPORTANT: Service Implementation Guide

**Before implementing ANY feature, consult the feature specification:**

**Before implenenting any test case or test file ensure the test matches the signature , data fields ,namespace and validation rules specified in the relevant feature specification file in the docs/11-specifications folder. If the specification is missing or incomplete, create or update the spec before proceeding with implementation. This ensures consistency, traceability, and alignment with business requirements across the entire solution.**

**After implimenting or updating any test case or test file, verify signature , field names , namespace ,field values and validations - update the relevant feature specification to mark the test as implemented and ensure all details are accurately reflected in the spec. This maintains the integrity of the documentation and provides a clear reference for future development and testing efforts.**

**Do not use heredoc - the output is usually garbled , use scripts (python) as a workaround**

📁 **[docs/11-specifications/INDEX.md](../docs/11-specifications/INDEX.md)** - Master index of all 11-specifications

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


### Database Schema Management (EF Core is Source of Truth)

**IMPORTANT:**
- The Entity Framework (EF) Core model and migrations are the **only** authoritative source for the database schema.
- All tables, columns, constraints, and relationships must be defined in the C# model (`DbSet` in `CrmDbContext` and `OnModelCreating` configuration).
- **Do not** make direct changes to the database schema (via SQL, admin tools, or raw DDL files). All schema changes must be made in code and applied via EF Core migrations.
- **Never** use `EnsureCreated()` for production or shared databases. Only use `MigrateAsync()` and the migration workflow. If `EnsureCreated()` was used previously, drop and recreate the database using migrations to avoid drift.
- All schema changes must be documented in the relevant feature specification (`docs/11-specifications/SPEC-*.md`) before implementation. Update the spec and mark as implemented when complete.
- If schema drift is detected (DB does not match EF model), drop and recreate the database from migrations, or manually align the schema, then reapply migrations. See `docs/architecture/ADR-002-EF-Core-Schema-Management.md` for recovery steps.

#### EF Core Migration Workflow (MANDATORY)
1. Update or add entities in code (C# model, `DbSet`, `OnModelCreating`).
2. Update the feature spec with the planned schema change.
3. Run:
  ```bash
  dotnet ef migrations add <MigrationName> --project src/CRM.Infrastructure --startup-project src/CRM.Api
  dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
  ```
4. Validate the migration on all supported DBs (MariaDB, SQL Server, PostgreSQL).
5. Commit migration files and update documentation/specs.

**References:**
- [docs/architecture/ADR-002-EF-Core-Schema-Management.md](../docs/architecture/ADR-002-EF-Core-Schema-Management.md)
- [docs/development/DATABASE_EF_CORE_GAP_ANALYSIS.md](../docs/development/DATABASE_EF_CORE_GAP_ANALYSIS.md)

### Single Database Policy (MANDATORY)

**CRITICAL:** The CRM solution operates with a **single database only** (`crm_db`).

| Rule | Description |
|------|-------------|
| **No Demo Database** | The legacy "demo database" (`crm_demodb`) feature has been deprecated and removed. All DemoDatabase configuration has been commented out. |
| **Single Connection String** | Only `ConnectionStrings__DefaultConnection` should be configured. No secondary database connections. |
| **Sample Data in Production DB** | If sample/demo data is needed, seed it directly into the production database using `SampleDataSeederService`, not a separate database. |
| **Deprecated Properties** | `SystemSettings.ShowDemoData`, `SampleDataSeeded`, and `SampleDataLastSeeded` are marked `[Obsolete]` and should not be used in new code. |

**What was deprecated:**
- `DemoDatabase__AutoSeed` environment variable
- `DemoDatabase__DatabaseName` environment variable  
- `crm_demodb` database name references
- `DemoDbContextFactory`, `DemoDataController`, `DemoDataSeederService` (removed from codebase)
- `IDemoDbContextFactory`, `IDemoModeState` interfaces (removed)

**If you encounter demo database references:**
1. Do NOT re-enable them
2. Use the single production database (`crm_db`) for all operations
3. Seed sample data using `SampleDataSeederService` if needed
4. Report any active demo database code to be deprecated

---
### Before Writing Code

1. **Find the spec:** `docs/11-specifications/SPEC-{MODULE}-{SEQ}-{FeatureName}.md` in not found create this file with understanding of the implimented code and the instructions . Add in addition put in additional details as you see fit to ensure the spec is comprehensive and clear for implementation. Use the `SPEC-TEMPLATE.md` as a starting point to maintain consistency across 11-specifications.
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
7. **Follow the patterns and conventions:** Refer to the documentation for naming, architecture, and coding standards
8. **Write tests:** Add unit/integration tests as per spec, mark as implemented when done
9. **Review and update documentation:** Ensure all relevant documentation is updated with any new features or changes
10. **Communicate with the team:** If you encounter any ambiguities or need clarifications, reach out to the team before proceeding with implementation
11. **Continuous Improvement:** As you implement features, if you identify any improvements or optimizations, document them and discuss with the team for potential inclusion in future iterations
12. **Maintain traceability:** Ensure that all code changes can be traced back to the original specification for accountability and future reference
13. **Ensure CI/CD builds pass:** After implementation, make sure all tests pass and the CI/CD pipeline is successful before merging changes
14. **Post-implementation review:** Conduct a review to ensure all features are working as expected and gather feedback for future improvements
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

**Current Progress:** 99% (Updated February 17, 2026 — see pending test coverage items)
Keep this plan updated and write back to this file the current status of the remediation efforts as you work through the phases. This will help maintain visibility and ensure we are on track to address all gaps in a timely manner.

---

## 1. Solution Overview

Update this and subsequent sections as needed to reflect the current state of the solution, especially as new features are added or architectural changes are made. This will serve as a quick reference for anyone new joining the project or needing an overview of the system.
### What is this?

A full-stack enterprise CRM (Customer Relationship Management) solution with:

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core 10.0 + Entity Framework Core 10.0 |
| **Frontend** | React 18 + TypeScript + Material-UI 5 |
| **Database** | MariaDB (primary), SQL Server, PostgreSQL supported |
| **Caching** | Redis |
| **Real-time** | SignalR WebSocket |
| **AI/LLM** | Multi-provider (Ollama, OpenAI, Azure, Anthropic, Bedrock, OpenRouter, Gemini) |
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
│       └── contexts/               # React Context state management
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

### 3.1 Logical Network Stacks

The CRM solution uses **three logical Docker network stacks** for separation of concerns:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           CRM INFRASTRUCTURE                                     │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌──────────────────────────┐ │
│  │     crm-core        │  │      crm-db         │  │     crm-components       │ │
│  │  (Application)      │  │    (Databases)      │  │   (Pluggable Providers)  │ │
│  ├─────────────────────┤  ├─────────────────────┤  ├──────────────────────────┤ │
│  │ • crm-api (5000)    │  │ • crm-mariadb(3306) │  │ • crm-meilisearch (7700) │ │
│  │ • crm-frontend (80) │  │ • crm-redis (6379)  │  │ • crm-ollama (11434)     │ │
│  │ • crm-gateway (5000)│  │ • crm-postgres(5432)│  │ • crm-chatwoot (3000)    │ │
│  └─────────────────────┘  │ • crm-sqlserver     │  │ • crm-novu (3000)        │ │
│                           │   (1433)            │  │ • crm-superset (8088)    │ │
│                           └─────────────────────┘  │ • crm-docuseal (3000)    │ │
│                                                    │ • crm-n8n (5678)         │ │
│                                                    └──────────────────────────┘ │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

| Network Stack | Purpose | Containers | External Access |
|---------------|---------|------------|------------------|
| **crm-core** | Core application services | crm-api, crm-frontend, crm-gateway | Yes (ports 80, 5000) |
| **crm-db** | Database services | crm-mariadb, crm-redis, crm-postgres, crm-sqlserver | Internal only |
| **crm-components** | Pluggable provider services | meilisearch, ollama, chatwoot, novu, superset, n8n, docuseal | Internal only |

**Network Communication:**
- All three networks are bridged allowing inter-stack communication
- `crm-core` → `crm-db`: Database connections
- `crm-core` → `crm-components`: Provider API calls
- External → `crm-core` only: Public-facing services

### 3.2 Development Server (192.168.0.9)
use root as the username to login to the server and the ssh key is stored in the local ssh client. This server is used for development and testing purposes, and it hosts the Docker containers for the CRM solution. Ensure that you have the necessary permissions and access rights to connect to this server before attempting to log in.
#### crm-core Stack (Core Application)

| Container | Port | Network Alias | Purpose |
|-----------|------|---------------|---------|
| `crm-api` | 5000 | crm-api | .NET Web API (Monolith) |
| `crm-frontend` | 80 | crm-frontend | React app (Nginx) |

#### crm-db Stack (Databases)

| Container | Port | Network Alias | Purpose |
|-----------|------|---------------|---------|
| `crm-mariadb` | 3306 | crm-mariadb | MariaDB database (primary) |
| `crm-redis` | 6379 | crm-redis | Redis cache & sessions |
| `crm-postgres` | 5432 | crm-postgres | PostgreSQL (optional) |
| `crm-sqlserver` | 1433 | crm-sqlserver | SQL Server (optional) |

#### crm-components Stack (Pluggable Providers)

| Container | Port | Network Alias | Category | Purpose |
|-----------|------|---------------|----------|---------|
| `crm-meilisearch` | 7700 | crm-meilisearch | Search | Full-text search engine |
| `crm-ollama` | 11434 | crm-ollama | AI | Local LLM inference |
| `crm-chatwoot` | 3000 | crm-chatwoot | Chat | Customer chat support |
| `crm-novu` | 3000 | crm-novu | Notifications | Multi-channel notifications |
| `crm-superset` | 8088 | crm-superset | Analytics | BI & data visualization |
| `crm-docuseal` | 3000 | crm-docuseal | Signatures | E-signature workflows |
| `crm-n8n` | 5678 | crm-n8n | Integrations | Workflow automation |

**Docker Networks:** 
- `crm_crm-network` (bridge) - Main unified network
- `crm-core-network` (bridge) - Core stack isolation
- `crm-db-network` (bridge) - Database stack isolation
- `crm-components-network` (bridge) - Components stack isolation

### 3.4 Microservices Architecture

| Service | Port | Image | Domain |
|---------|------|-------|--------|
| `crm-gateway` | 5000 | crm-gateway | YARP API Gateway |
| `crm-identity` | 5001 | crm-identity | Auth, Users, Groups |
| `crm-customer` | 5002 | crm-customer | Accounts, Contacts |
| `crm-sales` | 5003 | crm-sales | Opportunities, Quotes |
| `crm-marketing` | 5004 | crm-marketing | Campaigns, Leads |
| `crm-servicedesk` | 5005 | crm-servicedesk | Tickets, Workflows |
| `crm-core` | 5006 | crm-core | Settings, Monitoring |

### 3.5 Azure Resources Naming

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

### 3.6 AWS Resources Naming

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

### 3.7 GCP Resources Naming

| Resource | Naming Pattern | Example |
|----------|----------------|---------|
| **Project** | `crm-{env}` | `crm-prod` |
| **VPC Network** | `vpc-crm-{env}` | `vpc-crm-prod` |
| **Subnet** | `subnet-crm-{env}-{region}` | `subnet-crm-prod-us-central1` |
| **GKE Cluster** | `gke-crm-{env}` | `gke-crm-prod` |
| **Cloud SQL** | `sql-crm-{env}` | `sql-crm-prod` |
| **Cloud Storage** | `gs-crm-{env}-{purpose}` | `gs-crm-prod-uploads` |
| **Cloud Run** | `run-crm-{env}-{service}` | `run-crm-prod-api` |

### 3.8 Kubernetes Resources Naming

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

### 5.5 Pluggable Component Details

Each pluggable component can be deployed as a Docker container in the `crm-components` stack:

#### 5.5.1 Search - Meilisearch

| Property | Value |
|----------|-------|
| **Container** | `crm-meilisearch` |
| **Image** | `getmeili/meilisearch:v1.6` |
| **Port** | 7700 |
| **Feature Flag** | `UseExternalSearch` |
| **Provider Type** | `Meilisearch` |
| **Data Volume** | `/meili_data` |

```yaml
# docker-compose.providers.yml
crm-meilisearch:
  image: getmeili/meilisearch:v1.6
  container_name: crm-meilisearch
  ports:
    - "7700:7700"
  environment:
    - MEILI_MASTER_KEY=masterKey
    - MEILI_ENV=development
  volumes:
    - meilisearch_data:/meili_data
  networks:
    - crm-components-network
```

**Configuration:**
```json
{
  "Providers": {
    "Search": {
      "Type": "Meilisearch",
      "Meilisearch": {
        "Url": "http://crm-meilisearch:7700",
        "ApiKey": "masterKey",
        "IndexPrefix": "crm_"
      }
    }
  }
}
```

#### 5.5.2 AI - Ollama (Local LLM)

| Property | Value |
|----------|-------|
| **Container** | `crm-ollama` |
| **Image** | `ollama/ollama:latest` |
| **Port** | 11434 |
| **Feature Flag** | `UseExternalAI` |
| **Provider Type** | `Ollama` |
| **GPU Support** | Optional (NVIDIA) |

```yaml
crm-ollama:
  image: ollama/ollama:latest
  container_name: crm-ollama
  ports:
    - "11434:11434"
  volumes:
    - ollama_data:/root/.ollama
  networks:
    - crm-components-network
  deploy:
    resources:
      reservations:
        devices:
          - driver: nvidia
            count: all
            capabilities: [gpu]
```

**Configuration:**
```json
{
  "Providers": {
    "AI": {
      "Type": "Ollama",
      "Ollama": {
        "Url": "http://crm-ollama:11434",
        "Model": "llama3.1:8b",
        "EmbeddingModel": "nomic-embed-text"
      }
    }
  }
}
```

**Available Models:**
- `llama3.1:8b` - General purpose (default)
- `llama3.1:70b` - Large model
- `mistral:7b` - Fast inference
- `codellama:13b` - Code generation
- `nomic-embed-text` - Embeddings

#### 5.5.3 Chat - Chatwoot

| Property | Value |
|----------|-------|
| **Container** | `crm-chatwoot` |
| **Image** | `chatwoot/chatwoot:latest` |
| **Port** | 3000 |
| **Feature Flag** | `UseExternalChat` |
| **Provider Type** | `Chatwoot` |
| **Dependencies** | PostgreSQL, Redis |

```yaml
crm-chatwoot:
  image: chatwoot/chatwoot:latest
  container_name: crm-chatwoot
  depends_on:
    - crm-postgres
    - crm-redis
  ports:
    - "3000:3000"
  environment:
    - SECRET_KEY_BASE=<secret>
    - FRONTEND_URL=http://localhost:3000
    - RAILS_ENV=production
    - DATABASE_URL=postgres://crm_user:CrmPass@Dev2024@crm-postgres:5432/chatwoot
    - REDIS_URL=redis://crm-redis:6379
  networks:
    - crm-components-network
```

**Configuration:**
```json
{
  "Providers": {
    "Chat": {
      "Type": "Chatwoot",
      "Chatwoot": {
        "BaseUrl": "http://crm-chatwoot:3000",
        "ApiKey": "<api-access-token>",
        "AccountId": "1",
        "InboxId": "1"
      }
    }
  }
}
```

#### 5.5.4 Notifications - Novu

| Property | Value |
|----------|-------|
| **Container** | `crm-novu` |
| **Image** | `ghcr.io/novuhq/novu/api:latest` |
| **Port** | 3000 |
| **Feature Flag** | `UseExternalNotifications` |
| **Provider Type** | `Novu` |
| **Dependencies** | MongoDB, Redis |

```yaml
crm-novu:
  image: ghcr.io/novuhq/novu/api:latest
  container_name: crm-novu
  ports:
    - "3000:3000"
  environment:
    - NODE_ENV=production
    - MONGO_URL=mongodb://crm-mongo:27017/novu
    - REDIS_HOST=crm-redis
    - JWT_SECRET=<secret>
  networks:
    - crm-components-network
```

**Configuration:**
```json
{
  "Providers": {
    "Notifications": {
      "Type": "Novu",
      "Novu": {
        "ApiKey": "api-key-from-novu-dashboard",
        "ApplicationId": "app-id",
        "BaseUrl": "http://crm-novu:3000"
      }
    }
  }
}
```

**Notification Channels:**
- Email (SendGrid, Mailgun, SES)
- SMS (Twilio, Nexmo)
- Push (FCM, APNS)
- In-App notifications
- Slack, Discord webhooks

#### 5.5.5 Analytics - Apache Superset

| Property | Value |
|----------|-------|
| **Container** | `crm-superset` |
| **Image** | `apache/superset:latest` |
| **Port** | 8088 |
| **Feature Flag** | `UseExternalAnalytics` |
| **Provider Type** | `Superset` |
| **Dependencies** | PostgreSQL/MySQL |

```yaml
crm-superset:
  image: apache/superset:latest
  container_name: crm-superset
  ports:
    - "8088:8088"
  environment:
    - SUPERSET_SECRET_KEY=<secret>
    - SUPERSET_SQLALCHEMY_DATABASE_URI=mysql://crm_user:CrmPass@Dev2024@crm-mariadb:3306/superset
  volumes:
    - superset_data:/app/superset_home
  networks:
    - crm-components-network
```

**Configuration:**
```json
{
  "Providers": {
    "Analytics": {
      "Type": "Superset",
      "Superset": {
        "Url": "http://crm-superset:8088",
        "Username": "admin",
        "Password": "<password>",
        "DatabaseId": 1
      }
    }
  }
}
```

**Features:**
- SQL Lab for ad-hoc queries
- Dashboard builder
- Chart visualizations
- Scheduled reports
- Role-based access control

#### 5.5.6 E-Signatures - DocuSeal

| Property | Value |
|----------|-------|
| **Container** | `crm-docuseal` |
| **Image** | `docuseal/docuseal:latest` |
| **Port** | 3000 |
| **Feature Flag** | `UseExternalSignatures` |
| **Provider Type** | `DocuSeal` |
| **Data Volume** | `/data` |

```yaml
crm-docuseal:
  image: docuseal/docuseal:latest
  container_name: crm-docuseal
  ports:
    - "3000:3000"
  environment:
    - DATABASE_URL=postgres://crm_user:CrmPass@Dev2024@crm-postgres:5432/docuseal
    - SECRET_KEY_BASE=<secret>
  volumes:
    - docuseal_data:/data
  networks:
    - crm-components-network
```

**Configuration:**
```json
{
  "Providers": {
    "Signatures": {
      "Type": "DocuSeal",
      "DocuSeal": {
        "Url": "http://crm-docuseal:3000",
        "ApiKey": "<api-key>",
        "WebhookSecret": "<webhook-secret>"
      }
    }
  }
}
```

**Features:**
- PDF document signing
- Template management
- Multi-signer workflows
- Audit trails
- Webhook notifications

#### 5.5.7 Integrations - n8n

| Property | Value |
|----------|-------|
| **Container** | `crm-n8n` |
| **Image** | `n8nio/n8n:latest` |
| **Port** | 5678 |
| **Feature Flag** | `UseExternalIntegrations` |
| **Provider Type** | `N8n` |
| **Data Volume** | `/home/node/.n8n` |

```yaml
crm-n8n:
  image: n8nio/n8n:latest
  container_name: crm-n8n
  ports:
    - "5678:5678"
  environment:
    - N8N_BASIC_AUTH_ACTIVE=true
    - N8N_BASIC_AUTH_USER=admin
    - N8N_BASIC_AUTH_PASSWORD=<password>
    - WEBHOOK_URL=http://crm-n8n:5678/
    - DB_TYPE=postgresdb
    - DB_POSTGRESDB_HOST=crm-postgres
    - DB_POSTGRESDB_DATABASE=n8n
  volumes:
    - n8n_data:/home/node/.n8n
  networks:
    - crm-components-network
```

**Configuration:**
```json
{
  "Providers": {
    "Integrations": {
      "Type": "N8n",
      "N8n": {
        "BaseUrl": "http://crm-n8n:5678",
        "ApiKey": "<api-key>",
        "WebhookBaseUrl": "http://crm-n8n:5678/webhook"
      }
    }
  }
}
```

**Available Integrations (400+):**
- CRM: Salesforce, HubSpot, Pipedrive
- Communication: Slack, Teams, Discord
- Email: Gmail, Outlook, SendGrid
- Cloud: AWS, GCP, Azure
- Data: PostgreSQL, MongoDB, Airtable

### 5.6 Semantic Kernel AI Integration

The CRM includes a **Semantic Kernel v1.34.0** integration providing AI-powered agents:

**Architecture:**
```
CRM.Infrastructure/AI/SK/
├── Agents/          # 12 specialized agents + Orchestrator
├── Attributes/      # RequiresApprovalAttribute
├── Configuration/   # SemanticKernelOptions, MemoryCollections
├── Connectors/      # CrmKernelFactory, Chat/Embedding connectors
├── Filters/         # Audit, Approval, Cost tracking
├── Plugins/         # 12 CRM plugins (Account, Lead, etc.)
└── Services/        # AgentExecutionService
```

**Key Components:**
| Component | Count | Purpose |
|-----------|-------|--------|
| Plugins | 12 | CRM data access via `[KernelFunction]` methods |
| Agents | 12 | Specialized AI assistants (Lead Scoring, Support Triage, etc.) |
| Filters | 3 | Audit logging, human approval gates, cost tracking |
| API Endpoints | 20 | `/api/agents/*` for chat, admin, analytics |

**Feature Flags:** 16 flags under `FeatureManagement` control each agent individually.

**Plugin Convention:** Plugins named `{Name}Plugin` are resolved by `CrmKernelFactory` via DI.

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

### 6.5 Rate Limiting

The API uses ASP.NET Core's built-in `AddRateLimiter` with a configuration-driven toggle in `Program.cs`. The custom `RateLimitingMiddleware` class exists but is **not** registered in the pipeline.

**Configuration** (`appsettings.json`):

```json
"RateLimiting": {
    "EnableEndpointRateLimiting": true,
    "HttpStatusCode": 429,
    "QuotaExceededMessage": "API calls quota exceeded!",
    "GeneralRules": [
      { "Endpoint": "*", "Period": "1m", "Limit": 1000 }
    ],
    "EndpointRules": {
      "/api/auth/login":           { "Period": "1m", "Limit": 5 },
      "/api/auth/register":        { "Period": "1h", "Limit": 3 },
      "/api/auth/verify-2fa":      { "Period": "1m", "Limit": 10 },
      "/api/auth/forgot-password": { "Period": "1h", "Limit": 5 },
      "/api/auth/refresh-token":   { "Period": "1m", "Limit": 10 },
      "/api/auth/logout":          { "Period": "1m", "Limit": 30 },
      "/api/customers":            { "Period": "1m", "Limit": 500 },
      "/api/contacts":             { "Period": "1m", "Limit": 500 },
      "/api/opportunities":        { "Period": "1m", "Limit": 500 },
      "/api/products":             { "Period": "1m", "Limit": 500 },
      "/api/activities":           { "Period": "1m", "Limit": 300 },
      "/api/workflowengine":       { "Period": "1m", "Limit": 200 },
      "/api/reports":              { "Period": "1m", "Limit": 100 },
      "/api/dashboard":            { "Period": "1m", "Limit": 200 },
      "/api/llm":                  { "Period": "1m", "Limit": 60 }
    }
}
```

**Environment-based defaults:**
- `appsettings.Development.json`: `EnableEndpointRateLimiting: false` (disabled)
- `appsettings.Testing.json`: `EnableEndpointRateLimiting: false` (disabled)
- `appsettings.json` (Production): `EnableEndpointRateLimiting: true` (enabled)
- Code default: `!isDevelopment` (off in Development, on otherwise)

**Docker override** (via `docker-compose.yml`):

```yaml
- RateLimiting__EnableEndpointRateLimiting=${RATE_LIMITING_ENABLED:-true}
```

**To disable rate limiting for bulk data loading or testing:**

| Method | How |
|--------|-----|
| **Docker env var** | Set `RATE_LIMITING_ENABLED=false` in `.env` or pass `-e RateLimiting__EnableEndpointRateLimiting=false` to `docker run` |
| **ASPNETCORE_ENVIRONMENT** | Set to `Development` (loads `appsettings.Development.json` which disables it) |
| **appsettings override** | Add `"RateLimiting": { "EnableEndpointRateLimiting": false }` to the active appsettings file |

**Note:** The dev server deploy script (`deploy-to-dev-server.sh`) sets `ASPNETCORE_ENVIRONMENT=Production` in the generated `.env`, which enables rate limiting. The `RATE_LIMITING_ENABLED=false` variable is included in the deploy script to disable it for development. When deploying to production, change this to `true`.

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
| `POST /api/agents/{agentId}/chat` | Chat with AI agent |
| `GET /api/agents` | List AI agents |
| `GET /api/agents/analytics/usage` | Agent usage stats |

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
| React Context | 18.x | State management |
| Axios | 1.x | HTTP client |
| Formik + Yup | - | Form handling |
| SignalR | - | Real-time updates |

### 8.2 Directory Structure

```
src/
├── components/
│   ├── common/           # Shared components (DataGrid, Form, etc.)
│   ├── sales/            # Sales components (Quotes, Orders, etc.)
│   ├── itsm/             # ITSM module components
│   └── ...
├── pages/
│   ├── CustomersPage.tsx # Route-level components (Accounts list)
│   ├── DashboardPage.tsx
│   └── ...
├── services/
│   ├── apiClient.ts      # Axios instance
│   ├── accountService.ts # API calls
│   └── ...
├── contexts/
│   ├── AuthContext.tsx    # Auth provider
│   ├── ThemeContext.tsx   # Theme provider
│   ├── SignalRContext.tsx  # Real-time
│   └── ...
└── hooks/
    ├── useSignalR.ts
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
update this section as needed to reflect the current testing structure and any new test categories or patterns that emerge as the solution evolves.
```
CRM.Backend/tests/
├── Services/                     # Service unit tests
│   ├── AccountServiceTests.cs
│   └── ...
├── Controllers/                  # Controller tests
│   ├── AccountsControllerTests.cs
│   └── ...
├── Providers/                    # Provider unit tests
├── Integration/                  # Integration tests
│   ├── BuiltInSearchProviderIntegrationTests.cs
│   └── ...
├── CRM.Tests/                    # Additional test structure
│   ├── Services/
│   ├── Integration/
│   └── Helpers/
└── ...
e2e-tests/
└── tests/
    ├── bvt/
    │   └── api-bvt.spec.ts
    ├── auth/
    │   └── authentication.spec.ts
    └── customers/
        └── customers.spec.ts
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
On every first build and deploy after merging code to mail - run the github cicd script and select the option to run all tests. This will help ensure that all tests are passing and that the new code is properly integrated with the existing codebase. Fix issues as needed and repeat until all tests pass successfully. Github CICD should be kept clean on the main branch to ensure stability.
---

## 11. Documentation References

| Document | Purpose |
|----------|---------|
| [SOLUTION_CONTEXT.md](../docs/development/SOLUTION_CONTEXT.md) | Complete technical reference |
| [ARCHITECTURE_OVERVIEW.md](../docs/development/ARCHITECTURE_OVERVIEW.md) | System architecture |
| [MICROSERVICES_ARCHITECTURE.md](../docs/development/MICROSERVICES_ARCHITECTURE.md) | Microservices guide |
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
- **Frontend**: `CustomersPage.tsx` (route: `/accounts`)

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
| HTTP 429 Too Many Requests | Rate limiting is on. Disable with `-e RateLimiting__EnableEndpointRateLimiting=false` or set `RATE_LIMITING_ENABLED=false` in `.env` |
| Test data loader 429 errors | Ensure `--base-url http://192.168.0.9:5000` points to the dev server (default is localhost) and rate limiting is disabled |

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

### List of all endpoints and their purposes###
Add and update this section as new endpoints are implemented, ensuring it remains a comprehensive reference for developers and testers.

---

## 14. Documentation Structure & Maintenance (CRITICAL)

### 14.1 Root Directory Policy

**ONLY these files should exist in the solution root:**

| File | Purpose | Updatable |
|------|---------|-----------|
| **README.md** | Project overview & quick start | Yes |
| **LICENSE** | License information | No |
| **version.json** | Current version tracking | Yes (every commit) |
| **build.sh** | Primary build script | Yes |
| **deploy-to-dev-server.sh** | Main development deployment | Yes |
| **start-dev.sh** | Development environment startup | Yes |
| **deploy.sh** | Production deployment script | Yes |
| **.env** | Local environment variables (git-ignored) | No |
| **.env.example** | Environment template | Yes |
| **.gitignore** | Git exclusion rules | Yes |
| **Dockerfile.*** | Docker configurations (if any) | Yes |

**ALL OTHER FILES** must be moved to appropriate subdirectories per this policy. Do NOT add new .md files to root.

### 14.2 Documentation Directory Structure

```
docs/
├── README.md                          # Docs index
├──
├── 01-architecture/                  # ADRs & architectural decisions
│   ├── ADR-001-*.md
│   ├── ADR-002-*.md
│   └── ARCHITECTURE_OVERVIEW.md
│
├── 02-design/                        # Design documents
│   ├── SYSTEM_DESIGN.md
│   ├── DATABASE_DESIGN.md
│   └── UI_UX_GUIDELINES.md
│
├── 03-backend/                       # Backend documentation
│   ├── API_REFERENCE.md
│   ├── SERVICE_STRUCTURE.md
│   └── CODE_PATTERNS.md
│
├── 04-api/                           # API specifications
│   ├── ENDPOINTS_REFERENCE.md
│   ├── API_AUTHENTICATION.md
│   └── RATE_LIMITING.md
│
├── 05-frontend/                      # Frontend documentation
│   ├── COMPONENT_LIBRARY.md
│   ├── STATE_MANAGEMENT.md
│   └── STYLING_GUIDE.md
│
├── 06-standards/                     # Code standards
│   ├── NAMING_CONVENTIONS.md
│   ├── CODE_REVIEW_CHECKLIST.md
│   └── GIT_WORKFLOW.md
│
├── 07-testing/                       # Testing guides
│   ├── TEST_STRATEGY.md
│   ├── UNIT_TEST_GUIDE.md
│   ├── E2E_TEST_GUIDE.md
│   └── TEST_DATA_SETUP.md
│
├── 08-deployment/                    # Deployment guides
│   ├── DOCKER_DEPLOYMENT.md
│   ├── KUBERNETES_DEPLOYMENT.md
│   ├── AWS_DEPLOYMENT.md
│   ├── AZURE_DEPLOYMENT.md
│   └── GCP_DEPLOYMENT.md
│
├── 09-operations/                    # Operational runbooks
│   ├── MONITORING_SETUP.md
│   ├── TROUBLESHOOTING_RUNBOOK.md
│   ├── BACKUP_RECOVERY.md
│   └── SCALING_GUIDE.md
│
├── 10-traceability/                  # Traceability & mapping
│   ├── REQUIREMENTS_TRACEABILITY.md
│   └── TEST_COVERAGE_MATRIX.md
│
├── 11-specifications/                # Feature specifications (MAIN)
│   ├── INDEX.md
│   ├── SPEC-TEMPLATE.md
│   ├── SPEC-*.md
│   ├── SPEC-GEN-001-EnumReference.md
│   ├── FIELD_GAP_REMEDIATION_PLAN.md
│   ├── MASTER_TODO_LIST.md
│   └── SOLUTION_GAPS_REMEDIATION_PLAN.md
│
├── 12-enhancements/                  # Future enhancements
│   ├── ROADMAP.md
│   └── PLANNED_FEATURES.md
│
├── guides/                           # How-to guides
│   ├── LOCAL_DEVELOPMENT_SETUP.md
│   ├── FIRST_RUN_GUIDE.md
│   ├── ONBOARDING.md
│   ├── DATABASE_SETUP.md
│   └── BACKEND_IMPLEMENTATION_GUIDE.md
│
├── decisions/                        # Architecture decision records
│   ├── ADR-*.md (if not in 01-architecture/)
│   ├── POSTGRES_CHOICE.md
│   └── HEXAGONAL_ARCHITECTURE.md
│
├── references/                       # Technical references
│   ├── TECHNOLOGY_STACK.md
│   ├── GLOSSARY.md
│   ├── QUICK_REFERENCE.md
│   ├── SOLUTION_CONTEXT.md
│   ├── UNIFIED_CONFIG_SYSTEM_SUMMARY.md
│   └── PROVIDER_CONFIGURATION_REFERENCE.md
│
├── investigations/                   # Investigation reports
│   ├── 2026-02-*.md
│   ├── BACKEND_BUILD_FAILURE_ANALYSIS.md
│   ├── TEST_DATA_LOADER_FAILURE_ANALYSIS.md
│   └── PERFORMANCE_ANALYSIS.md
│
├── fixes-and-patches/                # Implementation fixes
│   ├── ADMIN_CONFIG_IMPLEMENTATION_STATUS.md
│   ├── CAMPAIGN_EXECUTION_FIX_IMPLEMENTATION.md
│   ├── COMMISSIONS_FIXES_IMPLEMENTATION.md
│   ├── DEPLOYMENT_SUCCESS_20260217.md
│   ├── SESSION_FIXES_SUMMARY.md
│   ├── stylecop_fixes_summary.md
│   ├── TEST_DATA_LOADER_FIXES_SUMMARY.md
│   └── CI_CD_BUILD_FIX_SUMMARY.md
│
├── changelog/                        # Version history
│   ├── CHANGELOG.md
│   ├── v0.560.md
│   └── v0.561.md
│
├── development/                      # Development processes
│   ├── DEVELOPMENT.md
│   ├── DATABASE_EF_CORE_GAP_ANALYSIS.md
│   ├── SOLUTION_CONTEXT.md
│   ├── ARCHITECTURE_OVERVIEW.md
│   └── MICROSERVICES_ARCHITECTURE.md
│
├── tools/                            # Utility scripts & analysis
│   ├── analyze_failures.py
│   ├── detailed_failure_analysis.py
│   ├── fix_bulk_crm_seed_json.py
│   └── README.md (with tool descriptions)
│
├── archives/                         # Historical/temporary files
│   ├── generated_tests.txt
│   ├── regenerated_tests_*.txt
│   └── old_implementations/
│
├── common_development_issues.md      # Living document: problems & solutions
└── INDEX.md                          # Complete documentation index
```

### 14.3 Documentation Categories & Rules

| Category | Location | File Pattern | When Created | Updates | Audience |
|----------|----------|--------------|-------------|---------|----------|
| **Feature Specifications** | `docs/11-specifications/` | `SPEC-{MODULE}-{SEQ}-{Name}.md` | Before implementation | Every iteration | Tech leads, developers |
| **Architecture Decisions** | `docs/01-architecture/` or `docs/decisions/` | `ADR-{Number}-{Title}.md` | Design phase | On reversal | All engineers |
| **API Documentation** | `docs/04-api/` | `ENDPOINTS_REFERENCE.md` | Before release | Per release | Backend, frontend devs |
| **How-To Guides** | `docs/guides/` | Descriptive names | Ongoing | When process changes | New developers |
| **Implementation Fixes** | `docs/fixes-and-patches/` | `{FEATURE}_FIX_*.md` | During bug fix | After resolution | Team |
| **Investigation Reports** | `docs/investigations/` | `{DATE}-{ISSUE}_*.md` | During problem-solving | Never (immutable) | All engineers |
| **Troubleshooting** | `docs/09-operations/` | `TROUBLESHOOTING_RUNBOOK.md` | On first issue | When issue recurs | Ops, developers |
| **Problem Tracker** | `docs/common_development_issues.md` | Living document | On first occurrence | After each new issue | All |
| **Remediations** | `docs/11-specifications/` | `*_REMEDIATION_PLAN.md` | During gap analysis | After each completed item | Tech leads |
| **Code Standards** | `docs/06-standards/` | `*.md` | Project start | Per policy change | All developers |

### 14.4 When to Create New Documentation

**MANDATORY DOCUMENTATION CREATION:**

1. **New Feature** → Create `SPEC-*.md` in `docs/11-specifications/`
2. **Breaking Change** → Create `ADR-*.md` in `docs/01-architecture/`
3. **New Service/Module** → Create guide in `docs/guides/` + API docs in `docs/04-api/`
4. **Investigation/Bug Fix** → Create report in `docs/investigations/` or `docs/fixes-and-patches/`
5. **New Recurring Problem** → Add to `docs/common_development_issues.md`
6. **Deployment Issue** → Update `docs/09-operations/TROUBLESHOOTING_RUNBOOK.md`

**DO NOT CREATE:**
- Documentation in project root (use `/docs`)
- Random .md files without category
- Temporary notes (use docs/archives/)
- Duplicate documentation (update existing instead)

### 14.5 Documentation Maintenance Checklist

Before committing code with documentation changes:

- [ ] File location matches `14.2` structure
- [ ] File naming follows patterns in `14.3`
- [ ] Update `docs/INDEX.md` with new files
- [ ] Link related documents together
- [ ] Mark implementation status (`✅`, `⚠️`, `❌`)
- [ ] Update version.json
- [ ] No new files in project root (except approved scripts)
- [ ] All links are relative paths (`docs/11-specifications/SPEC-*.md`)
- [ ] Verify no PHI/sensitive data in documentation
- [ ] For fixes: mark as completed and move to archives if superseded

### 14.6 Documentation Entry Points

| Role | Read These First |
|------|------------------|
| **New Contributor** | README.md → docs/guides/ONBOARDING.md |
| **Backend Developer** | docs/03-backend/SERVICE_STRUCTURE.md → docs/11-specifications/ |
| **Frontend Developer** | docs/05-frontend/COMPONENT_LIBRARY.md → docs/guides/ |
| **DevOps/Ops** | docs/08-deployment/ → docs/09-operations/ |
| **QA Engineer** | docs/07-testing/ → docs/11-specifications/ |
| **Tech Lead** | docs/01-architecture/ → docs/11-specifications/ |
| **Project Manager** | docs/INDEX.md → docs/11-specifications/SOLUTION_GAPS_REMEDIATION_PLAN.md |

### 14.7 Living Documents (Continuously Updated)

These files should be updated regularly and never archived:

- `docs/11-specifications/FIELD_GAP_REMEDIATION_PLAN.md` - Updated every field change
- `docs/11-specifications/SOLUTION_GAPS_REMEDIATION_PLAN.md` - Updated per session
- `docs/11-specifications/MASTER_TODO_LIST.md` - Updated per TODO completion
- `docs/common_development_issues.md` - Updated on each new problem
- `docs/09-operations/TROUBLESHOOTING_RUNBOOK.md` - Updated on new issue
- `CHANGELOG.md` / `version.json` - Updated before every commit
- `README.md` - Updated when dependencies or setup changes

---

**END OF COPILOT INSTRUCTIONS**
