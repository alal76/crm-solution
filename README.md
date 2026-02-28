# CRM Solution

<div align="center">

![Version](https://img.shields.io/badge/version-0.603.0-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![React](https://img.shields.io/badge/React-18-61DAFB)
![License](https://img.shields.io/badge/license-Source%20Available-orange)
![Build](https://img.shields.io/badge/build-passing-brightgreen)

**Enterprise-Grade Customer Relationship Management System**

[Features](#-features) • [ITSM Module](#-itsm-module) • [Quick Start](#-quick-start) • [Architecture](#-architecture) • [Documentation](#-documentation) • [API Reference](#-api-reference)

</div>

---

## 📋 Overview

CRM Solution is a comprehensive, full-stack Customer Relationship Management platform built with modern technologies. It supports both **monolithic** and **microservices** deployment architectures, offering flexibility for organizations of any size.

This is still very much a work in progress - totally unested and an experiment to see how far vibe coding can be used to build an enterprise grade solution . 

Comments feedback and enhacement requests along with your how it worked in your situation are welcome. 
I plan to use this  learning to do a writeup regarding this , and will incorporate the feedback into that writeup. 

This is a hobby side project not related to my day job - and built on weekends - no commitment on any time or effort will be put into this going forward - this is an experiment . Feel free to clone , extend enhance or change this as you feel free to.



### Key Highlights

- 🏢 **Multi-tenant Ready** - Support for multiple organizations and user groups
- 🔄 **Dual Architecture** - Deploy as monolith or microservices
- 📱 **Responsive Design** - Works seamlessly on desktop, tablet, and mobile
- 🔐 **Enterprise Security** - JWT authentication, role-based access control
- 🌐 **Multi-Database** - Supports MariaDB, MySQL, PostgreSQL, SQL Server
- 🚀 **Cloud Native** - Docker and Kubernetes ready
- 🤖 **AI Integration** - LLM provider support for intelligent features
- ⚡ **Real-time Updates** - SignalR for live notifications and concurrent editing

---

## 🎯 Features

### Core CRM Modules

| Module | Description |
|--------|-------------|
| **Customer Management** | Complete customer profiles, lifecycle tracking, organization/individual types |
| **Contact Management** | Multi-channel contact info (email, phone, social), relationship mapping |
| **Opportunity Management** | Sales pipeline, stage tracking, probability forecasting, win/loss analysis |
| **Account Management** | Business accounts, territory management, health scoring |
| **Lead Management** | Lead capture, scoring (fit/engagement), qualification workflow |
| **Quote Management** | Quote generation, line items, pricing, approval workflow |
| **Product Catalog** | Products, categories, pricing, SKU tracking |
| **CPQ Bundle Wizard** | 5-step guided bundle configuration UI for complex product configurations |
| **Dynamic Pricing Rules** | Volume/promotional/customer-tier pricing rules engine (`PricingRulesController`) |

### Marketing & Campaigns

| Feature | Description |
|---------|-------------|
| **Campaign Management** | Multi-channel campaigns (Email, Social, Event, Webinar) |
| **Campaign Execution** | A/B testing, recipient management, conversion tracking |
| **Marketing Analytics** | ROI, engagement metrics, conversion funnels |
| **Lead Scoring** | Automatic lead qualification based on engagement |

### Service & Support

| Feature | Description |
|---------|-------------|
| **Service Requests** | Ticketing system with SLA tracking |
| **Case Categories** | Hierarchical categorization and routing |
| **Priority Management** | Escalation levels, VIP customer handling |
| **Resolution Tracking** | Resolution codes, root cause analysis |
| **ITSM Module** | Incident, Problem, Change Management, CMDB, Service Catalog, SLA Dashboard ([details](#-itsm-module)) |
| **ITSM Notifications** | Slack (Block Kit) & Microsoft Teams (Adaptive Cards) notification channels with fan-out dispatcher |

### Automation & Workflow

| Feature | Description |
|---------|-------------|
| **Workflow Engine** | Visual workflow designer with conditional logic |
| **Task Automation** | Automated task creation and assignment |
| **Approval Workflows** | Multi-level approval processes |
| **Notifications** | Real-time alerts via SignalR |

### Data Quality & Deduplication

| Feature | Description |
|---------|-------------|
| **Duplicate Detection** | Real-time detection on create/edit with configurable rules |
| **Fuzzy Matching** | Levenshtein, Soundex, and email domain matching algorithms |
| **Merge Wizard** | Field-by-field comparison with override selection |
| **Merge Preview** | Preview changes before executing merge |
| **Unmerge Capability** | Restore merged records with full audit trail |
| **Batch Scanning** | Scan entire entity tables for duplicates |
| **Match Scoring** | Configurable thresholds and field weights |

### Relationships & Mapping

| Feature | Description |
|---------|-------------|
| **Relationship Types** | Parent/child, partner, competitor relationships |
| **Account Relationships** | Complex B2B relationship mapping |
| **Territory Management** | Geographic and account-based territories |
| **Interaction Tracking** | Communication history across relationships |

### Analytics & Reporting

| Feature | Description |
|---------|-------------|
| **Dashboards** | Customizable widgets and KPIs with SignalR live updates |
| **Sales Analytics** | Pipeline analysis, forecasting, win/loss, cohort analysis |
| **Marketing Reports** | Campaign performance, ROI analysis, lead score distribution widget |
| **Report Sharing** | Share reports with team members via POST/GET/DELETE `/api/reports/{id}/shares` |
| **Cohort Analysis** | Customer cohort and segment analytics (`/api/reports/cohort-analysis`, `/customer-segments`) |
| **Activity Tracking** | User activity logs and audit trails |

### Customer & Partner Portals

| Feature | Description |
|---------|-------------|
| **Customer Portal** | Self-service portal for customers — view accounts, submit tickets, track status, file attachments |
| **Partner Portal** | Deal registration, shared opportunity pipeline, partner resource library |
| **Portal Authentication** | Separate auth flow, rate-limited login/register/forgot-password |
| **CSAT / NPS / CES** | Satisfaction surveys triggered on ticket resolution, NPS scoring, token-based anonymous response form |

### Collaboration & Engagement

| Feature | Description |
|---------|-------------|
| **Record Comments** | Threaded comments with nested replies on Accounts, Leads, Opportunities, Service Requests |
| **@Mentions** | User @mention autocomplete in comment composer with avatar hsl colours |
| **Satisfaction Surveys** | CSAT (star 1-5), NPS (pill 0-10), CES surveys; SatisfactionDashboardPage with aggregate metrics |

### Scripting Engine

| Feature | Description |
|---------|-------------|
| **TypeScript Script Engine** | Node.js 20 sidecar (`crm-script-runner`) with `isolated-vm`, SWC AST scanner, `@engine/stdlib` |
| **Roslyn C# Engine** | Server-side C# scripting via Roslyn; script registry with lifecycle (draft/active/archived) |
| **Script Contracts** | `@engine/contracts` package defining shared types between host and sandbox |
| **Vitest Harness** | Unit-test harness for scripts running inside the isolated VM |

### System Administration

| Feature | Description |
|---------|-------------|
| **User Management** | Users, groups, roles, permissions with RBAC audit logging |
| **Configurable Enums** | Database-driven enums — admin UI to add/edit/deactivate enum values at runtime |
| **Field Configuration** | Custom fields per module |
| **System Settings** | Global configuration options with audit trail |
| **LLM Integration** | AI provider configuration (OpenAI, Anthropic, Groq, Ollama, Azure, Bedrock, OpenRouter, Gemini) |
| **Monitoring** | Health checks, performance metrics |
| **Theme Customization** | Light/dark modes with localStorage persistence, colour palettes |
| **Audit Log Export** | Export audit logs to CSV/JSON (10k cap), scheduled cleanup job |

---

## � ITSM Module

The ITSM (IT Service Management) module provides ITIL-aligned processes for managing IT services. It was implemented as part of the Phase 1 remediation plan and is fully operational.

> 📖 **Full user guide:** [ITSM User Guide](docs/guides/ITSM_USER_GUIDE.md)

### ITSM Capabilities

| Capability | Description |
|------------|-------------|
| **Incident Management** | Incident lifecycle — create, assign, escalate, resolve, close, reopen. Comment threads for collaboration. |
| **Problem Management** | Root cause analysis, known error tracking, linked incidents. |
| **Change Management** | Change requests with approval workflow, scheduling, conflict detection, blackout periods, impacted CIs. |
| **CMDB** | Configuration Item management, CI relationships and dependency mapping, impact analysis. |
| **Knowledge Base** | Knowledge articles with publish/retire lifecycle, feedback/ratings, suggested & popular articles. |
| **Service Catalog** | Browseable service catalog with categories, request submission, request-for-others, cancellation. |
| **SLA Management** | SLA policies & instances, breach detection, pause/resume, at-risk items, SLA dashboard & metrics. |
| **ITSM Dashboard** | Analytics — incident trends, problem analysis, change metrics, SLA compliance, team performance. |

### ITSM API Endpoints

| Base Route | Controller | Key Operations |
|-----------|------------|----------------|
| `/api/itsm/incidents` | IncidentsController | CRUD, assign, escalate, resolve, close, reopen, comments |
| `/api/itsm/problems` | ProblemsController | CRUD, link-incident, mark-known-error, related-incidents, root-cause |
| `/api/itsm/cmdb` | CMDBController | CRUD, search, relationships, related, impact-analysis |
| `/api/itsm/changes` | ChangesController | CRUD, submit-approval, approve, reject, schedule, conflicts, impacted-cis, blackout-periods |
| `/api/itsm/knowledge` | KnowledgeController | CRUD, publish, retire, feedback, suggested, popular, recent, categories |
| `/api/itsm/catalog` | CatalogController | Items, requests, search, categories, request-for-others, cancel |
| `/api/itsm/sla` | SLAController | Policies, instances, start/pause/resume/complete, breached, dashboard, at-risk, metrics |
| `/api/itsm/dashboard` | ITSMDashboardController | Incident trends, problem analysis, change metrics, SLA compliance, team performance, service health |

### ITSM Frontend Routes

| Route | Page |
|-------|------|
| `/itsm/incidents` | Incident list |
| `/itsm/incidents/:id` | Incident detail |
| `/itsm/problems` | Problem list |
| `/itsm/problems/:id` | Problem detail |
| `/itsm/changes` | Change list |
| `/itsm/changes/:id` | Change detail |
| `/itsm/cmdb` | CMDB list |
| `/itsm/cmdb/:id` | CI detail |
| `/itsm/knowledge` | Knowledge Base |
| `/itsm/knowledge/:id` | Article detail |
| `/itsm/catalog` | Service Catalog |
| `/itsm/catalog/request/:id` | Submit request |
| `/itsm/sla` | SLA Dashboard |

### ITSM Test Coverage

- **7,722** backend unit tests passing (across 3 test projects)
- **118 / 118** BVT (Build Verification Tests) passing — **100%**
- Dedicated `ITSMDashboardServiceTests` covering all dashboard analytics

---

## �🛠 Tech Stack

### Backend (.NET 10.0)

| Component | Technology |
|-----------|------------|
| **Framework** | ASP.NET Core 10.0 |
| **ORM** | Entity Framework Core 10.0 |
| **Real-time** | SignalR |
| **Logging** | Serilog (structured logging) |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **API Docs** | Swagger / OpenAPI 3.0 |
| **Caching** | In-Memory / Redis |

### Frontend (React 18)

| Component | Technology |
|-----------|------------|
| **Framework** | React 18 + TypeScript |
| **UI Library** | Material-UI (MUI) v5 |
| **Routing** | React Router v6 |
| **HTTP Client** | Axios |
| **Charts** | Recharts |
| **Forms** | Formik + Yup |
| **Real-time** | @microsoft/signalr |
| **State** | React Context + Hooks |

### Infrastructure

| Component | Technology |
|-----------|------------|
| **Containerization** | Docker 24+ |
| **Orchestration** | Docker Compose / Kubernetes |
| **Reverse Proxy** | Nginx |
| **Databases** | MariaDB, PostgreSQL, SQL Server |
| **Caching** | Redis (optional) |

---

## 🚀 Quick Start

### Prerequisites

- **Docker** 24.0+ and Docker Compose 2.0+
- **Node.js** 20+ (for local frontend development)
- **.NET SDK** 10.0+ (for local backend development)

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/alal76/crm-solution.git
cd crm-solution

# Start all services
docker compose -f docker/docker-compose.yml up -d

# Wait for services to be healthy (about 30 seconds)
docker compose -f docker/docker-compose.yml ps

# Access the application
# Frontend: http://localhost
# API:      http://localhost:5000
# Swagger:  http://localhost:5000/swagger
```

### Option 2: Local Development

```bash
# Terminal 1: Start Database
docker compose -f docker/docker-compose.databases.yml up -d

# Terminal 2: Start Backend
cd CRM.Backend/src/CRM.Api
dotnet run

# Terminal 3: Start Frontend
cd CRM.Frontend
npm install
npm start
```

### Option 3: Microservices Mode

```bash
# Start microservices architecture
docker compose -f docker/docker-compose.microservices.unified.yml up -d

# Services will be available:
# Gateway:    http://localhost:5000
# Identity:   http://localhost:5001
# Customer:   http://localhost:5002
# Sales:      http://localhost:5003
# Marketing:  http://localhost:5004
# ServiceDesk: http://localhost:5005
# Core:       http://localhost:5006
```

### Default Login Credentials

| Email | Password | Role |
|-------|----------|------|
| admin@crm.local | Admin@123 | Administrator |

---

## 🏗 Architecture

### Monolithic Architecture (Default)

```
┌─────────────────────────────────────────────────────────┐
│                     Frontend (React)                     │
│                   http://localhost:80                    │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                      CRM.Api                             │
│                http://localhost:5000                     │
│  ┌─────────────────────────────────────────────────────┐│
│  │  Controllers │ Services │ SignalR Hub │ Middleware  ││
│  └─────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────┐│
│  │              Entity Framework Core                   ││
│  └─────────────────────────────────────────────────────┘│
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                   MariaDB Database                       │
│                   (89 Tables)                            │
└─────────────────────────────────────────────────────────┘
```

### Microservices Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Frontend (React)                     │
│                       Port: 80                           │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                    API Gateway                           │
│                (CRM.Gateway:5000)                        │
└──────┬──────┬──────┬──────┬──────┬──────┬───────────────┘
       │      │      │      │      │      │
       ▼      ▼      ▼      ▼      ▼      ▼
┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐
│Ident.│ │Custom│ │Sales │ │Market│ │Serv. │ │Core  │
│:5001 │ │:5002 │ │:5003 │ │:5004 │ │:5005 │ │:5006 │
└──────┘ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘
       │      │      │      │      │      │
       └──────┴──────┴──────┴──────┴──────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                   Shared Database                        │
│                     MariaDB                              │
└─────────────────────────────────────────────────────────┘
```

### Database Schema (89 Tables)

| Domain | Count | Key Tables |
|--------|-------|------------|
| **Customer/Contact** | 12 | Customers, Contacts, CustomerContacts, Addresses |
| **Sales** | 10 | Opportunities, Quotes, QuoteLineItems, Leads, Products |
| **Marketing** | 12 | MarketingCampaigns, CampaignRecipients, CampaignABTests |
| **Service Desk** | 8 | ServiceRequests, Categories, Subcategories |
| **Relationships** | 6 | AccountRelationships, RelationshipMaps, RelationshipTypes |
| **Workflow** | 8 | WorkflowDefinitions, WorkflowInstances, WorkflowTasks |
| **Contact Info** | 8 | EmailAddresses, PhoneNumbers, SocialMediaLinks |
| **System** | 15 | Users, UserGroups, SystemSettings, ModuleConfigs |
| **Other** | 10 | Notes, Tags, Dashboards, LLMProviderSettings |

---

## � Pluggable Architecture

CRM Solution features a **pluggable provider architecture** that allows operators to choose between built-in implementations and external services for key capabilities. This enables flexibility from simple deployments to enterprise-grade integrations.

### Provider Categories

| Category | Built-In | Self-Hosted OSS | Cloud SaaS |
|----------|----------|-----------------|------------|
| **Search** | SQL LIKE queries | Meilisearch, Typesense | Algolia, Azure Search |
| **Notifications** | SMTP Email | Novu | Twilio, SendGrid |
| **Chat** | In-memory | Chatwoot | Intercom, Zendesk |
| **E-Signatures** | Manual workflow | DocuSeal | DocuSign, Adobe Sign |
| **Analytics** | Basic dashboards | Apache Superset | Power BI, Looker |
| **Integrations** | Webhooks | n8n | Zapier, Make |
| **AI/LLM** | Ollama (local) | - | Azure OpenAI, AWS Bedrock |

### Configuration-Driven Selection

Providers are selected via feature flags in `appsettings.json`:

```json
{
  "FeatureManagement": {
    "UseExternalSearch": false,
    "UseExternalNotifications": false,
    "UseExternalChat": false,
    "UseExternalSignatures": false,
    "UseExternalAnalytics": false
  },
  "Providers": {
    "Search": {
      "Type": "BuiltIn",
      "Meilisearch": {
        "Url": "http://meilisearch:7700",
        "ApiKey": "${MEILISEARCH_API_KEY}"
      }
    }
  }
}
```

### Provider Health Endpoint

Check provider status at runtime:

```bash
GET /api/health/providers

{
  "timestamp": "2024-02-05T10:30:00Z",
  "overallHealthy": true,
  "providers": {
    "Search": { "activeProvider": "Meilisearch", "isHealthy": true },
    "Chat": { "activeProvider": "BuiltIn", "isHealthy": true },
    "Notifications": { "activeProvider": "Novu", "isHealthy": true }
  }
}
```

### Documentation

| Document | Description |
|----------|-------------|
| [Operator Deployment Guide](docs/OPERATOR_DEPLOYMENT_GUIDE.md) | Complete deployment instructions |
| [Provider Configuration Reference](docs/PROVIDER_CONFIGURATION_REFERENCE.md) | All provider settings |
| [Troubleshooting Runbook](docs/TROUBLESHOOTING_RUNBOOK.md) | Provider issue resolution |
| [ADR-001: Architecture Strategy](docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md) | Architecture decision record || [Third-Party Licenses](docs/THIRD_PARTY_LICENSES.md) | Complete dependency licensing inventory |
---

## �📁 Project Structure

```
crm-solution/
├── CRM.Backend/                    # .NET Backend
│   ├── src/
│   │   ├── CRM.Api/               # Main API (Monolith)
│   │   │   ├── Controllers/       # REST API Controllers
│   │   │   ├── Hubs/              # SignalR Hubs
│   │   │   ├── Middleware/        # Custom Middleware
│   │   │   └── Helpers/           # Utility Classes
│   │   ├── CRM.Core/              # Domain Layer
│   │   │   ├── Entities/          # Domain Entities
│   │   │   ├── Dtos/              # Data Transfer Objects
│   │   │   └── Interfaces/        # Service Interfaces
│   │   ├── CRM.Infrastructure/    # Data Access Layer
│   │   │   ├── Data/              # DbContext & Configurations
│   │   │   └── Services/          # Service Implementations
│   │   └── Services/              # Microservices
│   │       ├── CRM.Gateway/       # API Gateway (Ocelot)
│   │       ├── CRM.Identity/      # Auth Service
│   │       ├── CRM.CustomerService/
│   │       ├── CRM.SalesService/
│   │       ├── CRM.MarketingService/
│   │       ├── CRM.ServiceDeskService/
│   │       └── CRM.CoreService/
│   ├── tests/                     # Test Projects
│   └── migrations/                # SQL Migration Scripts
│
├── CRM.Frontend/                   # React Frontend
│   ├── src/
│   │   ├── components/            # Reusable Components
│   │   │   ├── common/            # Shared UI Components
│   │   │   ├── ContactInfo/       # Contact Info Components
│   │   │   └── settings/          # Settings Components
│   │   ├── pages/                 # Page Components
│   │   ├── services/              # API Service Clients
│   │   ├── contexts/              # React Context Providers
│   │   ├── hooks/                 # Custom Hooks
│   │   ├── config/                # Configuration
│   │   └── theme/                 # MUI Theme Config
│   └── public/                    # Static Assets
│
├── docker/                         # Docker Configuration
│   ├── docker-compose.yml         # Main (Monolith)
│   ├── docker-compose.databases.yml
│   ├── docker-compose.microservices.unified.yml
│   ├── docker-compose.unified.yml
│   └── Dockerfile.*               # Service Dockerfiles
│
├── kubernetes/                     # K8s Manifests
│   ├── 00-namespace-config.yaml
│   ├── 01-database-tier.yaml
│   ├── 02-application-tier.yaml
│   ├── 03-presentation-tier.yaml
│   ├── 04-ingress-network.yaml
│   ├── local/                     # Local K8s configs
│   ├── microservices/             # Microservices K8s
│   └── production/                # Production K8s
│
├── e2e-tests/                      # E2E Tests (Playwright)
│   ├── playwright.config.ts
│   └── tests/
│       ├── auth.setup.ts
│       ├── customers/
│       ├── contacts/
│       ├── data/
│       ├── bvt/
│       └── functional/
│
├── scripts/                        # Automation Scripts
│   ├── deploy.sh                  # Main deploy script
│   ├── deploy-192.168.0.9.sh      # Production deploy
│   ├── build-and-deploy.sh        # Build + deploy
│   └── seed-test-data.sh          # Data seeding
│
├── docs/                           # Documentation
│   ├── architecture/
│   ├── deployment/
│   ├── features/
│   ├── guides/
│   └── testing/
│
├── config/                         # Configuration Files
├── ARCHITECTURE_OVERVIEW.md
├── MICROSERVICES_ARCHITECTURE.md
├── TESTING_SUMMARY.md
├── CHANGELOG.md
└── version.json
```

---

## 📚 Documentation

### Standards & Best Practices

| Document | Description |
|----------|-------------|
| [Coding Standards](docs/development/CODING_STANDARDS.md) | Code style guidelines |
| [Security Best Practices](docs/development/SECURITY_BEST_PRACTICES.md) | Security guidelines |
| [Architecture Decisions](docs/architecture/decisions/README.md) | ADR framework |

### Architecture

| Document | Description |
|----------|-------------|
| [Architecture Overview](docs/development/ARCHITECTURE_OVERVIEW.md) | High-level system design |
| [Microservices Architecture](docs/development/MICROSERVICES_ARCHITECTURE.md) | Service decomposition |
| [Database Configuration](docs/architecture/DATABASE_CONFIGURATION.md) | Multi-database support |

### Deployment

| Document | Description |
|----------|-------------|
| [Docker Setup](docs/deployment/DOCKER_SETUP.md) | Docker Compose guide |
| [Kubernetes Guide](docs/deployment/KUBERNETES_DEPLOYMENT_GUIDE.md) | K8s deployment |
| [Production Deploy](docs/development/DEPLOYMENT_GUIDE_192.168.0.9.md) | Server deployment |
| [Infrastructure Guide](docs/INFRASTRUCTURE_GUIDE.md) | Infrastructure overview |

### Features

| Document | Description |
|----------|-------------|
| [User Management](docs/features/USER_MANAGEMENT_README.md) | Users & permissions |
| [Multi-User Capability](docs/features/MULTI_USER_CAPABILITY.md) | Concurrent editing |
| [Workflow Engine](docs/WORKFLOW_EXAMPLES.md) | Automation examples |
| [Contact Info System](docs/features/CONSOLIDATED_CONTACT_INFO.md) | Contact management |
| [Navigation Config](docs/guides/NAVIGATION_CONFIGURATION.md) | Menu customization |
| [ITSM User Guide](docs/ITSM_USER_GUIDE.md) | ITSM module workflows and usage |

### Development

| Document | Description |
|----------|-------------|
| [Development Guide](docs/DEVELOPMENT.md) | Local setup |
| [How-To Guide](docs/HOWTO.md) | Common tasks |
| [Testing Summary](docs/test/TESTING_SUMMARY.md) | Test strategies |

---

## 🔌 API Reference

### Base URLs

| Environment | URL |
|-------------|-----|
| Development | http://localhost:5000/api |
| Production | http://192.168.0.9:5000/api |
| Swagger UI | http://localhost:5000/swagger |

### Authentication

```bash
# Login
POST /api/auth/login
Content-Type: application/json
{
  "email": "admin@crm.local",
  "password": "Admin@123"
}

# Response
{
  "accessToken": "eyJhbG...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

### Using the Token

```bash
curl -H "Authorization: Bearer <token>" \
     http://localhost:5000/api/customers
```

### Core Endpoints

| Resource | GET | POST | PUT | DELETE |
|----------|-----|------|-----|--------|
| `/api/customers` | ✅ | ✅ | ✅ | ✅ |
| `/api/contacts` | ✅ | ✅ | ✅ | ✅ |
| `/api/opportunities` | ✅ | ✅ | ✅ | ✅ |
| `/api/products` | ✅ | ✅ | ✅ | ✅ |
| `/api/leads` | ✅ | ✅ | ✅ | ✅ |
| `/api/quotes` | ✅ | ✅ | ✅ | ✅ |
| `/api/campaigns` | ✅ | ✅ | ✅ | ✅ |
| `/api/servicerequests` | ✅ | ✅ | ✅ | ✅ |
| `/api/comments` | ✅ | ✅ | ✅ | ✅ |
| `/api/satisfaction` | ✅ | ✅ | ✅ | ✅ |
| `/api/reports` | ✅ | ✅ | — | — |
| `/api/reports/{id}/shares` | ✅ | ✅ | — | ✅ |
| `/api/pricing-rules` | ✅ | ✅ | ✅ | ✅ |
| `/api/portal/crm` | ✅ | ✅ | ✅ | — |
| `/api/partner-portal` | ✅ | ✅ | — | — |
| `/api/agents` | ✅ | ✅ | — | — |
| `/api/admin/enums` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/incidents` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/problems` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/changes` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/cmdb` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/knowledge` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/catalog` | ✅ | ✅ | — | — |
| `/api/itsm/sla` | ✅ | ✅ | ✅ | ✅ |
| `/api/itsm/dashboard` | ✅ | — | — | — |

### Pagination

```
GET /api/customers?page=1&pageSize=25&sortBy=company&sortOrder=asc
```

### Response Format

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 25,
  "totalPages": 4
}
```

---

## 🧪 Testing

### E2E Tests (Playwright)

```bash
cd e2e-tests
npm install
npx playwright install

# Run all tests
BASE_URL=http://localhost npx playwright test

# Run specific suite
BASE_URL=http://localhost npx playwright test tests/customers

# Run with UI
BASE_URL=http://localhost npx playwright test --ui

# Run against production
BASE_URL=http://192.168.0.9 npx playwright test
```

### Backend Tests

```bash
cd CRM.Backend/tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

| Category | Location | Description |
|----------|----------|-------------|
| **BVT** | `e2e-tests/tests/bvt/` | Build Verification Tests |
| **Functional** | `e2e-tests/tests/functional/` | UI Functional Tests |
| **Data** | `e2e-tests/tests/data/` | Data Creation Tests |
| **Unit** | `CRM.Backend/tests/` | Backend Unit Tests |

---

## 🚢 Production Deployment

### Deploy Script

```bash
# Deploy to 192.168.0.9
./scripts/deploy-192.168.0.9.sh

# Build with version bump and deploy
./scripts/build-and-deploy.sh patch
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Database connection |
| `Jwt__Secret` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | JWT issuer name |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |
| `AllowedOrigins` | CORS allowed origins |

### Docker Images

| Service | Image | Port |
|---------|-------|------|
| Frontend | crm-frontend | 80 |
| API | crm-api | 5000 |
| Gateway | crm-gateway | 5000 |
| Identity | crm-identity | 5001 |
| Customer | crm-customer | 5002 |
| Sales | crm-sales | 5003 |
| Marketing | crm-marketing | 5004 |
| ServiceDesk | crm-servicedesk | 5005 |
| Core | crm-core | 5006 |

---

## 📈 Current Statistics

| Metric | Value |
|--------|-------|
| **Version** | 0.603.0 |
| **Database Tables** | 100+ |
| **API Controllers** | 45+ |
| **React Components** | 100+ |
| **Microservices** | 8 |
| **Backend Unit Tests** | 8,000+ passing |
| **BVT Tests** | 118 / 118 (100%) |
| **E2E Test Files** | 30+ |
| **Lines of Code** | 100,000+ |
| **ADRs** | 10 (ADR-001 → ADR-010) |

---

## 🔄 Recent Updates (v0.603.0 — February 2026)

### 🆕 New Modules & Features
- ✅ **TypeScript Script Engine Sidecar** (`crm-script-runner`) — Node.js 20 + `isolated-vm`, SWC AST scanner, `@engine/stdlib` / `@engine/contracts`, Vitest harness, Docker Compose sidecar (SARCH-030→038)
- ✅ **Roslyn C# Script Engine** — Script registry with full lifecycle (draft/active/archived), server-side C# execution (SARCH-001→029)
- ✅ **Customer Portal** (`/api/portal/crm`) — Self-service ticket submission, file attachments (10 MB), profile management, CSAT trigger hooks (PORTAL-014→043)
- ✅ **Partner Portal** (`/api/partner-portal`) — Deal registration, shared opportunity pipeline, resource library
- ✅ **CSAT / NPS / CES Surveys** — `SurveyResponseForm`, token-based anonymous responses, `SatisfactionDashboardPage`, 15 unit tests
- ✅ **Record Comments & @Mentions** — Threaded comments, nested replies, @mention autocomplete on Accounts / Leads / Opportunities / Service Requests
- ✅ **Configurable Enums** — Database-driven enum management; admin CRUD UI to add/edit/deactivate values at runtime without code changes
- ✅ **Dynamic Pricing Rules Engine** — Volume, promotional, and customer-tier pricing (`PricingRulesController`)
- ✅ **CPQ Bundle Wizard** — 5-step guided product-bundle configuration UI
- ✅ **ZIP / GeoNames Import** — Admin Master Data panel: GeoNames (all / per-country), CSV upload with progress bar and polling
- ✅ **Report Sharing** — `POST/GET/DELETE /api/reports/{id}/shares`, `ReportShareDialog.tsx`, EF migration
- ✅ **SignalR Live Dashboard Hub** — `DashboardHub` at `/hubs/dashboard`, `useDashboardRealtime.ts` hook
- ✅ **Cohort Analysis** — `POST /api/reports/cohort-analysis` + `/customer-segments`, `ReportCohortType` / `CohortMetricType` / `SegmentBy` enums

### 🤖 AI & Agents
- ✅ **AI Lead Scoring** — `LeadScoringController` + Semantic Kernel agent; supports Groq as default provider via `ILLMService`
- ✅ **Groq Provider** — `CrmChatCompletionConnector` resolves model from `LLMProviders.DefaultProvider`
- ✅ **12 Semantic Kernel Agents** — CRM AI agents (Lead Scoring, Support Triage, etc.) with Feature Flag governance

### 🔔 Notifications & Integrations
- ✅ **Slack ITSM Notifications** — Block Kit formatting, `SlackItsmNotificationService`
- ✅ **Teams ITSM Notifications** — Adaptive Cards, `TeamsItsmNotificationService`
- ✅ **Fan-out Dispatcher** — `ItsmNotificationDispatcher` fans to `IEnumerable<IItsmNotificationChannel>` via `Task.WhenAll` with per-channel error isolation
- ✅ **Slack & Teams Notification Providers** — `SlackNotificationProvider` (Block Kit), `TeamsNotificationProvider` (Adaptive Cards)

### 🏗 Architecture & Quality
- ✅ **SonarCloud Remediation** — 78 bugs + 64 security vulnerabilities resolved; hotspot suppress rules added
- ✅ **10 ADRs** (ADR-001 → ADR-010) covering architecture decisions and scripting engine design
- ✅ **RBAC Navigation** — `NavigationConfigService` conditionally adds ITSM/Marketing nav items via Feature Manager
- ✅ **Audit Log Export** — `AuditLogExportService` (CSV/JSON, 10 k cap), `AuditLogCleanupJob`, `IX_AuditLogs_CreatedAt` index
- ✅ **Usage Record Batch Buffer** — `UsageRecordBatchBuffer` (ConcurrentQueue, Singleton) + 30 s flush `UsageRecordBatchHostedService`
- ✅ **BillingTimezoneService** — DST-aware UTC conversion for billing operations
- ✅ **Dunning Scheduler** — `DunningSchedulerService` (IHostedService, 4 h interval), grace period, escalation emails
- ✅ **KnowledgeBase Search Index** — `KnowledgeBaseSearchIndexService` with Meilisearch schema (6 searchable, 9 filterable attrs)
- ✅ **Dark Mode Toggle** with `localStorage` persistence; sidebar collapse persisted; Recent Items dropdown in AppBar
- ✅ **WCAG Accessibility** — ARIA labels and keyboard navigation improvements across core pages

### Previously Completed (included for reference)
- ✅ **ITSM Module** — Incident, Problem, Change Management, CMDB, Service Catalog, SLA Dashboard
- ✅ **8 ITSM API Controllers**, 13 ITSM frontend pages, 7 ITSM backend services
- ✅ Pluggable Architecture — 7 provider categories (Search, Chat, Notifications, E-Signatures, Analytics, AI, Integrations)
- ✅ Microservices architecture with 8 services and YARP API gateway
- ✅ Campaign execution with A/B testing, conversion tracking
- ✅ Relationship management, territory management, interaction tracking
- ✅ LLM provider integration (Ollama, OpenAI, Azure, Anthropic, Bedrock, OpenRouter, Gemini, Groq)
- ✅ SignalR real-time notifications and live dashboard updates
- ✅ Multi-user concurrent editing with optimistic concurrency (`RowVersion`)

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is **source-available** — free for non-commercial use, commercial use requires a license. See [LICENSE](LICENSE) for the full terms.

**Copyright © 2024–2026 Abhishek Lal. All rights reserved.**

### Third-Party Dependency Licensing Matrix

All third-party components used in this solution are open source with permissive or copyleft licenses. For the complete dependency inventory with version numbers and license details, see [docs/THIRD_PARTY_LICENSES.md](docs/THIRD_PARTY_LICENSES.md).

#### Backend (.NET 10.0) — Key Packages

| Package | License | Notes |
|---------|---------|-------|
| ASP.NET Core / EF Core | MIT | Microsoft .NET ecosystem |
| Pomelo.EntityFrameworkCore.MySql | MIT | MariaDB/MySQL provider |
| Npgsql.EntityFrameworkCore.PostgreSQL | PostgreSQL License | Permissive |
| Oracle.EntityFrameworkCore | Oracle Free Use (non-OSI) | ⚠️ Optional — operator responsibility |
| BCrypt.Net-Next | MIT | Password hashing |
| Serilog | Apache-2.0 | Structured logging |
| FluentValidation | Apache-2.0 | Input validation |
| Meilisearch SDK | MIT | Search provider |
| Algolia.Search | MIT | Search provider |
| Twilio / SendGrid | MIT | Notification providers |
| DocuSign.eSign | MIT | E-signature provider |

#### Frontend (React 18) — Key Packages

| Package | License | Notes |
|---------|---------|-------|
| React / ReactDOM | MIT | UI framework |
| Material-UI (MUI) v5 | MIT | Component library |
| TypeScript | Apache-2.0 | Type system |
| Axios | MIT | HTTP client |
| React Router v6 | MIT | Client routing |
| Formik + Yup | Apache-2.0 / MIT | Form handling |
| Recharts | MIT | Charts & visualization |
| SignalR Client | MIT | Real-time updates |

#### Infrastructure & Docker Images

| Component | License | Role |
|-----------|---------|------|
| MariaDB 11.2 | GPL-2.0 | Primary database |
| Redis 7 | BSD-3-Clause | Cache |
| Meilisearch v1.6 | MIT | Search engine |
| Ollama | MIT | Local LLM runtime |
| Novu (self-hosted) | MIT (app) / SSPL-1.0 (MongoDB dep) | ⚠️ Notifications — see notes |
| Chatwoot | MIT | Chat provider |
| DocuSeal | AGPL-3.0 | E-signatures |
| Apache Superset | Apache-2.0 | Analytics |

#### External Service Integrations (API-only)

| Service | Integration Method | License Impact |
|---------|-------------------|----------------|
| n8n | REST API (HttpClient) | ✅ None — no code bundled |
| Zapier / Make | Webhook URLs | ✅ None — SaaS only |
| OpenAI / Azure OpenAI / Anthropic | REST API | ✅ None — SaaS only |
| AWS Bedrock / OpenRouter | REST API | ✅ None — SaaS only |
| DocuSign / Adobe Sign | REST API + SDK | ✅ SDK is MIT licensed |
| Algolia / Elasticsearch | REST API + SDK | ✅ SDK is MIT licensed |
| Intercom / Zendesk | REST API | ✅ None — SaaS only |
| Power BI / Looker | REST API | ✅ None — SaaS only |
| Twilio / SendGrid | REST API + SDK | ✅ SDK is MIT licensed |

> **⚠️ Special Notes:**
> - **n8n** uses a [Sustainable Use License](https://github.com/n8n-io/n8n/blob/master/LICENSE.md) — this CRM integrates via REST API only (no code bundling), so no license restrictions apply.
> - **Oracle.EntityFrameworkCore** uses a non-OSI license — only relevant if operators choose Oracle as their database provider.
> - **MongoDB SSPL-1.0** is a transitive dependency of self-hosted Novu — relevant only when self-hosting Novu with MongoDB.
> - All **SaaS integrations** (OpenAI, Zapier, etc.) are API-only and carry no license obligations on the CRM codebase.

---

## 👥 Authors

- **Abhi Lal** - *Lead Developer* - [@alal76](https://github.com/alal76)

---

<div align="center">

**Built with ❤️ using .NET 10 and React 18**

[Report Bug](https://github.com/alal76/crm-solution/issues) • [Request Feature](https://github.com/alal76/crm-solution/issues)

</div>
