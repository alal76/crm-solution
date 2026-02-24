# CRM Solution — Functional & Technical Specifications

**Version:** 0.577.0  
**Date:** February 24, 2026  
**Architecture:** Monolith (microservices path available)  
**License:** Source-Available — Commercial Use Requires License

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Functional Specifications](#2-functional-specifications)
   - 2.1 [Core CRM](#21-core-crm)
   - 2.2 [Sales Module](#22-sales-module)
   - 2.3 [Marketing Module](#23-marketing-module)
   - 2.4 [Service Desk / ITSM Module](#24-service-desk--itsm-module)
   - 2.5 [AI & Agents Module](#25-ai--agents-module)
   - 2.6 [Administration & System](#26-administration--system)
   - 2.7 [Integrations & Webhooks](#27-integrations--webhooks)
3. [Technical Specifications](#3-technical-specifications)
   - 3.1 [Technology Stack](#31-technology-stack)
   - 3.2 [Architecture](#32-architecture)
   - 3.3 [Database Schema](#33-database-schema)
   - 3.4 [API Design](#34-api-design)
   - 3.5 [Authentication & Security](#35-authentication--security)
   - 3.6 [Frontend Architecture](#36-frontend-architecture)
   - 3.7 [Pluggable Provider Architecture](#37-pluggable-provider-architecture)
   - 3.8 [Infrastructure & Deployment](#38-infrastructure--deployment)
   - 3.9 [Non-Functional Requirements](#39-non-functional-requirements)
4. [Module Inventory](#4-module-inventory)
5. [Data Model Summary](#5-data-model-summary)
6. [API Endpoint Reference](#6-api-endpoint-reference)

---

## 1. Executive Summary

The CRM Solution is a full-stack, enterprise-grade Customer Relationship Management platform built on ASP.NET Core 10 and React 18. It covers the full customer lifecycle — from lead capture through opportunity management, quoting, ordering, invoicing, service desk ticketing, and AI-assisted workflows.

The platform is designed with a **hexagonal (ports & adapters) architecture** that allows pluggable, feature-flag-driven substitution of external providers for search, chat, notifications, analytics, e-signatures, AI/LLM inference, and workflow automation. All providers have built-in fallback implementations, making the system self-contained by default while allowing cloud or SaaS augmentation at any layer.

Key characteristics:

| Attribute | Value |
|-----------|-------|
| **Backend** | ASP.NET Core 10 + Entity Framework Core 10 |
| **Frontend** | React 18 + TypeScript + Material-UI v5 |
| **Primary Database** | MariaDB (SQL Server & PostgreSQL also supported) |
| **Caching** | Redis |
| **Real-time** | SignalR WebSocket |
| **AI/LLM** | Multi-provider: Ollama, OpenAI, Azure OpenAI, Anthropic, Amazon Bedrock, OpenRouter, Google Gemini |
| **Deployment** | Docker Compose, Kubernetes (Helm), Azure, AWS, GCP |
| **Auth** | JWT + Refresh Tokens, OAuth 2.0 (Google, GitHub), 2FA (TOTP), WebAuthn/Passkeys |
| **Test Coverage** | xUnit (backend), Jest/React Testing Library (frontend), Playwright (E2E) |
| **Controllers** | 120+ REST API controllers |
| **Entities** | 130+ domain entities |
| **Frontend Pages** | 75+ route-level pages |

---

## 2. Functional Specifications

### 2.1 Core CRM

#### 2.1.1 Account Management (`SPEC-CRM-001`)

Accounts represent organizations or companies. Each account can hold multiple contacts, opportunities, activities, notes, addresses, phone numbers, emails, and social media profiles.

**Key capabilities:**
- Full CRUD with soft delete
- Polymorphic contact info (addresses, phones, emails, social media) linked via entity-link tables
- Account relationships (parent/subsidiary, partner, competitor, reseller)
- Account health snapshots for churn scoring
- Territory assignment
- Account team membership
- Duplicate detection and merge
- Custom fields
- Activity timeline

**Business rules:**
- Account name is required and must be unique within a tenant
- Primary contact must be a linked `Contact` record
- Soft delete sets `IsDeleted = true`; hard delete is not permitted

---

#### 2.1.2 Contact Management (`SPEC-CRM-004`)

Contacts represent individual people. A contact may be linked to one or more accounts.

**Key capabilities:**
- Full CRUD with polymorphic contact info
- Many-to-many link to accounts (`AccountContacts`)
- Interaction and activity history
- Relationship map visualization
- Duplicate detection

---

#### 2.1.3 Lead Management (`SPEC-CRM-002`)

Leads are unqualified prospects entering the sales funnel.

**Key capabilities:**
- Capture leads from web forms, email-to-lead, manual entry, and import
- Lead scoring with configurable scoring rules (`LeadScoreRules`)
- Lead routing rules (`LeadRoutingRules`) — round-robin, weighted, territory-based
- Convert lead to Account + Contact + Opportunity in a single atomic transaction
- Qualification workflow with stage tracking
- Duplicate checking before conversion

---

#### 2.1.4 Opportunity Management (`SPEC-CRM-003`)

Opportunities track active sales pursuits through a configurable pipeline.

**Key capabilities:**
- Configurable pipeline stages per pipeline definition
- Win/loss tracking with reason codes
- Product line items on opportunity
- Weighted and quota-adjusted forecast value
- Close date tracking and overdue detection
- Activity and interaction history
- AI-assisted lead scoring and next-best-action recommendations

---

#### 2.1.5 Activity Management (`SPEC-CRM-005`)

Activities log all interactions — calls, meetings, emails, notes — against any CRM entity.

**Key capabilities:**
- Polymorphic entity linkage (account, contact, lead, opportunity, service request)
- Activity types: Call, Meeting, Email, Task, Note, Demo, Other
- Schedule and reminder support
- Integration with calendar providers (Google Calendar, Outlook)

---

#### 2.1.6 Task Management (`SPEC-CRM-007`)

Tasks are work items assigned to users, linked to CRM records.

**Key capabilities:**
- Due date, priority, status, and assignee tracking
- Link to accounts, contacts, leads, opportunities
- Workflow-triggered task creation
- Overdue notifications

---

#### 2.1.7 Pipeline Management (`SPEC-CRM-006`)

Pipelines define the stages through which opportunities progress.

**Key capabilities:**
- Multiple configurable pipelines per organization
- Drag-and-drop kanban board on frontend
- Stage probability weighting
- Win/loss stage designation

---

#### 2.1.8 Relationships & Interactions

- **Relationship Map:** Visual graph of account and contact relationships with relationship type labels
- **Interaction Log:** Timestamped log of all touchpoints with a contact or account
- **Notes:** Rich-text notes linked to any entity

---

### 2.2 Sales Module

#### 2.2.1 Quote Management (`SPEC-SALES-001`)

- Create quotes from opportunities with product line items
- Multi-currency support
- Discount rules and approval matrices
- PDF generation and e-signature request via DocuSeal/DocuSign
- Quote versioning and comparison
- Convert quote to order

#### 2.2.2 Order Management (`SPEC-SALES-002`)

- Orders created from accepted quotes or standalone
- Order line items with product catalog lookup
- Fulfillment status tracking
- Integration with invoicing

#### 2.2.3 Invoice Management (`SPEC-SALES-003`)

- Generate invoices from orders or standalone
- Invoice line items, tax, discounts
- Payment tracking against invoice
- Credit memo issuance
- Dunning record tracking for overdue invoices
- PDF generation

#### 2.2.4 Payment Management (`SPEC-SALES-004`)

- Record payments against invoices
- Payment methods: Credit Card, Bank Transfer, Check, Wire, ACH, Cash, Manual
- Payment status lifecycle: Pending → Completed / Failed / Refunded
- Reconciliation against invoices

#### 2.2.5 Contract Management (`SPEC-SALES-005`)

- Customer contracts with start/end dates
- Contract value and renewal tracking
- E-signature integration
- Contract status: Draft, Active, Expired, Terminated

#### 2.2.6 Subscription Management (`SPEC-SALES-006`)

- Recurring revenue subscriptions linked to products
- Billing period configuration (monthly, quarterly, annually)
- Renewal automation and dunning
- Subscription upgrades, downgrades, cancellations
- Usage-based billing hooks (controller stubs)

#### 2.2.7 Commission Management (`SPEC-SALES-007`)

- Commission plans with configurable tiers and rules
- Commission calculations per sales rep per period
- Commission payouts tracking
- Approval workflow for payout processing

#### 2.2.8 Price Books & Product Catalog

- Product master with SKU, pricing, category, and bundle support
- Price books for customer segment or territory-specific pricing
- Product bundles for grouped offerings
- Discount rules engine with approval matrix

#### 2.2.9 Sales Quotas & Forecasts

- Quota assignment per user, team, and territory per period
- Forecast roll-up with weighted pipeline contributions
- Forecast vs. actual reporting

---

### 2.3 Marketing Module

#### 2.3.1 Campaign Management (`SPEC-MKT-001`)

- Multi-channel marketing campaigns (email, SMS, social)
- Campaign lifecycle: Draft → Scheduled → Running → Completed → Cancelled
- Recipient list management with segment targeting
- A/B test support (`CampaignABTest`)
- Campaign attribution tracking across the funnel
- Link click tracking (`CampaignLinkClick`)
- Conversion tracking (`CampaignConversion`)
- Campaign metrics (open rate, click rate, conversion rate, revenue attributed)
- Campaign workflow automation (`CampaignWorkflow`)

#### 2.3.2 Email Templates (`SPEC-MKT-002`)

- Rich-text HTML email templates with versioning
- Merge tag variable substitution
- Template categories and tagging
- Preview and test send

#### 2.3.3 Email Sequences (`SPEC-MKT-003`)

- Multi-step drip email sequences
- Delay scheduling between steps
- Enrollment and unenrollment rules
- Per-step open/click tracking

#### 2.3.4 Web Form Builder (`SPEC-MKT-004`)

- Drag-and-drop web form builder
- Form fields: text, email, phone, dropdown, checkbox, file upload
- Submit-to-lead or submit-to-contact capture
- Embed code generation
- Spam protection (honeypot, reCAPTCHA-ready)

#### 2.3.5 Landing Pages (`SPEC-MKT-005`)

- Landing page builder linked to campaigns
- Form embedding on landing pages
- Published URL management

#### 2.3.6 Web Tracking

- Web visitor identification (`WebVisitor`)
- Page visit and event tracking (`AnalyticsEvent`)
- Attribution of web sessions to leads and contacts

---

### 2.4 Service Desk / ITSM Module

#### 2.4.1 Service Request Management (`SPEC-SD-001`)

- Full ticket lifecycle: Open → In Progress → Pending → Resolved → Closed
- Priority levels: Critical, High, Medium, Low
- Category and sub-category classification
- Assignment to agents or queues
- Email-to-ticket ingestion
- SLA tracking with countdown timer (SignalR real-time update)
- Internal notes vs. customer-visible comments
- File attachment support
- Customer self-service portal integration stubs

#### 2.4.2 Knowledge Base (`SPEC-SD-002`)

- Article authoring with rich text
- Categories and tags
- Article versioning
- Public and internal visibility levels
- Search integration (Meilisearch or built-in)
- Article rating and feedback

#### 2.4.3 SLA Management (`SPEC-SD-003`)

- SLA policy definitions with response and resolution time targets
- Business hours configuration
- SLA breach detection and alerting
- SLA pausing rules (e.g., waiting on customer)
- Real-time countdown displayed on ticket detail

#### 2.4.4 Workflow Engine (`SPEC-SD-004`)

- Visual configurable trigger-condition-action workflow definitions
- Triggers: ticket created, field changed, SLA breach, time elapsed
- Actions: assign ticket, send email, update field, create task, trigger webhook
- Workflow instances and execution logs
- Workflow tasks assigned to agents

#### 2.4.5 Escalation Management (`SPEC-SD-005`)

- Escalation rules based on SLA breach or priority
- Escalation policies with multiple tiers
- Auto-escalation to supervisors or queues
- Escalation notification routing

#### 2.4.6 ITSM Module (IT Service Management)

Supporting ITIL-aligned processes:

| Process | Feature |
|---------|---------|
| **Incident Management** (`SPEC-ITSM-001`) | P1–P4 incidents, major incident handling, resolution workflow |
| **Problem Management** (`SPEC-ITSM-002`) | Root cause analysis, known error database |
| **Change Management** (`SPEC-ITSM-003`) | Change Advisory Board (CAB) approval, change types, risk assessment |
| **CMDB** (`SPEC-ITSM-004`) | Configuration items (CIs), CI types, relationship mapping |
| **Service Queue** | Queue definitions for agent workload distribution |

---

### 2.5 AI & Agents Module

#### 2.5.1 AI Agent Framework

Built on **Microsoft Semantic Kernel v1.34.0** with 12 specialized AI agents orchestrated by a central orchestrator agent:

| Agent | Purpose |
|-------|---------|
| Lead Scoring Agent | Score and rank leads using ML signals |
| Customer Support Triage Agent | Auto-classify and route service requests |
| Sales Coach Agent | Next-best-action and deal guidance |
| Email Intelligence Agent | Draft, summarize, and classify inbound emails |
| Churn Prediction Agent | Identify at-risk accounts |
| Campaign Intelligence Agent | Recommend campaign content and timing |
| Contract Analyzer Agent | Extract and review contract terms |
| Knowledge Curator Agent | Suggest and organize KB articles |
| Forecast Agent | Generate revenue forecast narratives |
| Data Quality Agent | Identify and suggest duplicate/data fixes |
| Relationship Intelligence Agent | Map stakeholder influence |
| Orchestrator Agent | Route tasks to specialist agents |

#### 2.5.2 AI Plugins (Kernel Functions)

12 CRM domain plugins expose native CRM data to the Semantic Kernel planner:
Account, Contact, Lead, Opportunity, Quote, Order, Invoice, ServiceRequest, Campaign, KnowledgeBase, User, Analytics plugins.

#### 2.5.3 AI Capabilities

- **Multi-provider LLM:** Ollama (local), OpenAI GPT-4o, Azure OpenAI, Anthropic Claude, Amazon Bedrock, OpenRouter, Google Gemini — switchable via feature flags
- **Embeddings:** Semantic search and similarity via embedding models
- **Human Approval Gates:** Configurable approval filters on high-risk agent actions
- **Audit Logging:** All agent actions logged with user attribution
- **Cost Tracking:** Token usage and cost tracking per agent session
- **Chat Interface:** Streaming chat UI per agent

#### 2.5.4 AI Email Intelligence

- Smart email composition with context
- Email classification (inquiry, complaint, renewal, churn risk)
- Suggested responses
- Email-to-ticket creation with auto-population

#### 2.5.5 Chatbot

- Self-service chatbot for customer portal
- Intent classification and knowledge base lookup
- Fallback to live agent handoff

---

### 2.6 Administration & System

#### 2.6.1 User Management (`SPEC-SYS-001`)

- User CRUD with role assignment
- Soft delete with account deactivation
- User profile with avatar, timezone, language preference
- Password reset flow with email verification
- Bulk import of users

#### 2.6.2 Authentication (`SPEC-SYS-002`)

- Local username/password with BCrypt hashing
- OAuth 2.0 social login: Google, GitHub
- JWT access tokens (60-minute expiry) + Refresh tokens (7-day rolling)
- Two-factor authentication (TOTP/app-based)
- WebAuthn passkey registration and authentication
- CSRF protection on OAuth flows
- Session management and forced logout

#### 2.6.3 Role-Based Access Control (`SPEC-SYS-012`)

- Roles: Admin, Manager, Sales Rep, Support Agent, Marketing, Read-Only, and custom
- Permissions model: resource + action (Create, Read, Update, Delete, Execute)
- Group-based permission assignment (`UserGroups`, `UserGroupMembers`)
- Per-endpoint authorization via `[Authorize]` policies
- UI permission-based menu and action hiding

#### 2.6.4 Group Management (`SPEC-SYS-003`)

- User groups for team and role organization
- Group-based feature access
- Group membership management

#### 2.6.5 Department Management

- Department hierarchy
- Users assigned to departments
- Department-scoped reporting

#### 2.6.6 Feature Flag Management (`SPEC-SYS-004`)

- Runtime-configurable feature flags via UI or environment variables
- Feature flag audit log
- Granular flags for each AI agent, provider, and optional module

#### 2.6.7 System Settings (`SPEC-SYS-005`)

- Configuration for branding (logo, colors, company name)
- Email SMTP settings
- Default timezone and locale
- Session and token TTL configuration
- Rate limiting configuration

#### 2.6.8 Audit Logging (`SPEC-SYS-006`)

- Immutable audit log for all create/update/delete operations
- Who, what, when, old value, new value
- GDPR access log (`GdprAccessLog`)
- Field-level change log (`FieldChangeLog`)
- Searchable and exportable audit trail

#### 2.6.9 Dashboard & Reporting

- Configurable dashboard with drag-and-drop widgets
- User-level and admin-level dashboard customization
- Built-in report types: pipeline summary, sales performance, campaign performance, ticket trends, SLA compliance
- Report export (CSV, PDF)
- Custom analytics events for trend analysis
- Performance monitoring dashboard

#### 2.6.10 Navigation Management (`SPEC-SYS-007`)

- Navigation item configuration per role
- Module enable/disable via feature flags
- Dynamic sidebar/menu generation

#### 2.6.11 Data Management

- **Import/Export:** CSV/Excel import and export with column mapping, validation, and error reporting
- **Duplicate Detection:** Configurable duplicate rules per entity with merge workflow
- **Data Normalization:** Address and phone number normalization
- **Lookups & Master Data:** Configurable lookup categories and items for dropdown fields
- **Custom Fields:** Dynamic custom fields per entity type
- **Tags:** Entity tagging with autocomplete

#### 2.6.12 Branding & UI Customization

- Logo, favicon, and color palette configuration
- Light/dark theme support
- Per-user UI preferences
- Module-level field and UI rendering configuration (`ModuleFieldConfiguration`, `ModuleUIConfig`)
- Color palette management

#### 2.6.13 Communication Management

- Communication channel definitions (email, SMS, chat, push)
- Communication message log
- Channel-level routing rules

#### 2.6.14 Calendar Integration

- Google Calendar and Outlook integration (`CalendarIntegration`)
- Activity-to-calendar sync
- OAuth token management for calendar providers

---

### 2.7 Integrations & Webhooks

#### 2.7.1 Webhook Management (`SPEC-INT-001`)

- Register outbound webhook endpoints with event filters
- Event types: entity created, updated, deleted; SLA breached; stage changed; payment received
- Delivery log with retry and failure tracking (`WebhookDeliveryGeneral`)
- HMAC signature verification on outbound payloads
- Inbound webhook receivers: DocuSeal, ITSM external systems

#### 2.7.2 Provider Integrations (`SPEC-INT-002`)

Pluggable integration providers (see §3.7 for architecture detail):

| Category | Available Providers |
|----------|-------------------|
| Search | Built-in, Meilisearch, Algolia, Typesense, Elasticsearch, Azure Cognitive Search |
| Chat | Built-in, Chatwoot, Intercom, Zendesk, Freshchat, RocketChat |
| Notifications | Built-in, Novu, Twilio, SendGrid, OneSignal, Courier, AWS SES |
| Analytics | Built-in, Apache Superset, Metabase, Power BI, Looker, QuickSight |
| E-Signatures | Built-in, DocuSeal, DocuSign, Adobe Sign, HelloSign |
| AI/LLM | Ollama, OpenAI, Azure OpenAI, Anthropic, Amazon Bedrock, OpenRouter, Google Gemini |
| Workflow Automation | Built-in, n8n, Zapier, Make, Workato |

#### 2.7.3 Import & Export (`SPEC-INT-003`)

- Guided import wizard: file upload → column mapping → validation → preview → import
- Export wizard: entity selection → field selection → filter → download
- Import job queue with background processing
- Import error reporting per row
- Supported formats: CSV, XLSX

#### 2.7.4 CI/CD & Cloud Deployment Integration

- CI/CD integration monitoring controller
- Cloud deployment record tracking (`CloudDeployment`)
- Monitoring integration for external APM tools

#### 2.7.5 Social Media & News

- Social media account linking (`SocialMediaAccount`, `SocialMediaFollow`)
- News feed integration stubs for account intelligence

---

## 3. Technical Specifications

### 3.1 Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| **Backend Runtime** | ASP.NET Core | 10.0 |
| **ORM** | Entity Framework Core | 10.0 |
| **Primary Database** | MariaDB | 10.11+ |
| **Alt Databases** | SQL Server 2022, PostgreSQL 16 | — |
| **Caching** | Redis | 7.x |
| **Real-time** | ASP.NET Core SignalR | 10.0 |
| **AI Framework** | Microsoft Semantic Kernel | 1.34.0 |
| **Frontend Framework** | React | 18.x |
| **UI Component Library** | Material-UI (MUI) | 5.x |
| **Language (Frontend)** | TypeScript | 5.x |
| **HTTP Client** | Axios | 1.x |
| **Forms** | Formik + Yup | — |
| **Routing** | React Router | 6.x |
| **Build Tool** | craco (Create React App) | — |
| **Backend Tests** | xUnit + Moq + FluentAssertions | — |
| **Frontend Tests** | Jest + React Testing Library | — |
| **E2E Tests** | Playwright | — |
| **Containerization** | Docker + Docker Compose | — |
| **Orchestration** | Kubernetes (Helm charts) | — |
| **CI/CD** | Azure DevOps Pipelines | — |

---

### 3.2 Architecture

#### 3.2.1 Hexagonal Architecture (Ports & Adapters)

```
┌───────────────────────────────────────────────┐
│             Application Core (CRM.Core)        │
│  Domain Entities · Business Logic · Ports      │
│  ISearchPort · IChatPort · INotificationPort   │
│  IAIPort · IAnalyticsPort · ISignaturePort     │
│  IIntegrationPort                              │
└───────────────────────┬───────────────────────┘
                        │ DI / Factory
┌───────────────────────▼───────────────────────┐
│          Infrastructure (CRM.Infrastructure)   │
│  EF Core DbContext · Provider Implementations  │
│  Meilisearch · Ollama · Chatwoot · Novu        │
│  DocuSeal · Superset · n8n                     │
└───────────────────────┬───────────────────────┘
                        │ HTTP/gRPC
┌───────────────────────▼───────────────────────┐
│               API Layer (CRM.Api)              │
│  120+ REST Controllers · SignalR Hubs          │
│  JWT / OAuth2 / 2FA / WebAuthn Middleware      │
└───────────────────────┬───────────────────────┘
                        │ HTTPS
┌───────────────────────▼───────────────────────┐
│           React SPA (CRM.Frontend)             │
│  75+ Pages · Material-UI · Axios · SignalR     │
└───────────────────────────────────────────────┘
```

#### 3.2.2 Project Structure

```
crm-solution/
├── CRM.Backend/
│   └── src/
│       ├── CRM.Api/            # Controllers, Middleware, Program.cs
│       ├── CRM.Core/           # Entities, DTOs, Ports, Enums
│       └── CRM.Infrastructure/ # DbContext, Services, Providers, Migrations
│   └── tests/                  # xUnit test projects
├── CRM.Frontend/
│   └── src/
│       ├── pages/              # 75+ route-level page components
│       ├── components/         # Reusable UI components
│       ├── services/           # Axios API service layer
│       ├── contexts/           # React Context (Auth, Theme, SignalR)
│       └── hooks/              # Custom hooks
├── docker/                     # Dockerfiles and Compose files
├── kubernetes/                 # K8s manifests and Helm charts
├── database/                   # SQL schema, seeds, migrations
├── docs/                       # Full documentation tree
└── scripts/                    # Utility scripts
```

#### 3.2.3 Microservices Path

A microservices decomposition is available and deployable via `docker/docker-compose.microservices.unified.yml`. The decomposed services split by domain:

| Service | Port | Domain |
|---------|------|--------|
| crm-gateway | 5000 | YARP API Gateway |
| crm-identity | 5001 | Auth, Users, Groups |
| crm-customer | 5002 | Accounts, Contacts |
| crm-sales | 5003 | Opportunities, Quotes |
| crm-marketing | 5004 | Campaigns, Leads |
| crm-servicedesk | 5005 | Tickets, Workflows |
| crm-core | 5006 | Settings, Monitoring |

The monolith (`CRM.Api`) remains the primary production deployment.

---

### 3.3 Database Schema

#### 3.3.1 Schema Management

- Entity Framework Core is the **single source of truth** for the schema
- All changes must go through EF Core migrations — no direct DDL
- Migration apply command:

```bash
dotnet ef migrations add <Name> --project src/CRM.Infrastructure --startup-project src/CRM.Api
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

#### 3.3.2 Base Entity Pattern

All entities inherit `BaseEntity`:

```csharp
public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }     // soft delete
    public byte[] RowVersion { get; set; }  // optimistic concurrency
}
```

#### 3.3.3 Core Table Groups (~95+ tables)

| Group | Tables |
|-------|--------|
| **Identity** | Users, UserGroups, UserGroupMembers, Roles, Permissions, UserSessions, RefreshTokens, OAuthTokens, WebAuthnCredentials |
| **CRM Core** | Customers (Accounts), Contacts, AccountContacts, Leads, Opportunities, OpportunityProducts, Interactions, Activities |
| **Contact Info (Polymorphic)** | Addresses, PhoneNumbers, EmailAddresses, SocialMediaAccounts, EntityAddressLinks, EntityPhoneLinks, EntityEmailLinks, EntitySocialMediaLinks |
| **Sales** | Quotes, QuoteLineItems, Orders, Invoices, Payments, Contracts, Subscriptions, SubscriptionRenewals, CreditMemos, DunningRecords, Commissions, CommissionRules, CommissionPlans, CommissionPayouts |
| **Products** | Products, ProductBundles, PriceBooks, PricingRules, DiscountRules, DiscountApprovalMatrices |
| **Pipeline** | Pipelines, Stages |
| **Territories & Sales Ops** | Territories, AccountTerritories, Teams, SalesQuotas, SalesForecasts, SalesConfiguration |
| **Marketing** | MarketingCampaigns, CampaignRecipients, CampaignMetrics, CampaignConversions, CampaignLinkClicks, CampaignABTests, CampaignAttributions, CampaignWorkflows |
| **Email** | EmailTemplates, EmailTemplateVersions, EmailSequences, EmailSequenceSteps, EmailIntegrations |
| **Web** | FormDefinitions, LandingPages, WebVisitors, AnalyticsEvents |
| **Service Desk** | ServiceRequests, ServiceRequestCategories, KnowledgeBase articles, SLAPolicies, EscalationRules, EscalationPolicies, ServiceQueues |
| **ITSM** | Incidents, Problems, Changes, ChangeTypes, CatalogItems, CatalogCategories, CIs (CMDB), CITypes |
| **Workflow** | WorkflowDefinitions, WorkflowInstances, WorkflowTasks, WorkflowTriggers |
| **AI** | AIAgentUsage, LLMProviderSettings |
| **Integration** | WebhookEndpoints, WebhookEvents, WebhookDeliveries, ProviderConfigurations |
| **Import/Export** | ImportJobs, ImportMappings, ImportErrors, ExportJobs |
| **System** | SystemSettings, FeatureFlags, FeatureFlagAuditLogs, AuditLogs, FieldChangeLogs, GdprAccessLogs |
| **UI/Config** | DashboardWidgets, DashboardCustomizations, ModuleFieldConfigurations, ModuleUIConfigs, UIPreferences, UICustomizations, ColorPalettes, BrandingConfigs |
| **Misc** | Notes, Tags, EntityTags, CustomFields, LookupCategories, LookupItems, Departments, ZipCodes, Localities |

#### 3.3.4 Database Credentials (Development)

```
Host:     crm-mariadb
Port:     3306
Database: crm_db
User:     crm_user
Password: CrmPass@Dev2024
```

---

### 3.4 API Design

#### 3.4.1 REST Conventions

All entities implement the standard CRUD pattern:

```
GET    /api/{entity}        List (paginated)
GET    /api/{entity}/{id}   Get by ID
POST   /api/{entity}        Create
PUT    /api/{entity}/{id}   Full update
PATCH  /api/{entity}/{id}   Partial update
DELETE /api/{entity}/{id}   Soft delete
```

#### 3.4.2 Pagination

```json
// Request
GET /api/accounts?page=1&pageSize=20&sortBy=name&sortOrder=asc

// Response envelope
{
  "items": [ ... ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

#### 3.4.3 DTO Layer

- All API contracts use DTOs, never raw entities
- DTOs are a superset of API response fields
- Naming standard: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`
- Dates serialized as ISO 8601 strings
- Enums serialized as numeric values; frontend maps to TypeScript enums

#### 3.4.4 Rate Limiting

| Endpoint | Limit |
|----------|-------|
| `POST /api/auth/login` | 5/min |
| `POST /api/auth/register` | 3/hour |
| `GET /api/customers` | 500/min |
| `GET /api/contacts` | 500/min |
| `GET /api/opportunities` | 500/min |
| `POST /api/agents/*/chat` | 60/min |
| Default (all endpoints) | 1000/min |

Rate limiting is disabled in `Development` environment and enabled in `Production`.

#### 3.4.5 Health Endpoints (No Auth Required)

```
GET /health        Liveness
GET /health/ready  Readiness
GET /health/live   Kubernetes liveness
GET /api/health/providers  Provider connectivity status
```

---

### 3.5 Authentication & Security

#### 3.5.1 Authentication Flows

| Flow | Mechanism |
|------|-----------|
| Local login | Email + password (BCrypt) → JWT access + refresh tokens |
| Social login | OAuth 2.0 (Google, GitHub) with CSRF state token |
| Two-factor | TOTP (authenticator app), enforced per-user or globally |
| Passkey | WebAuthn FIDO2 credential registration and assertion |
| Token refresh | Sliding refresh token (7-day, HTTP-only cookie option) |
| Forced logout | Session revocation via `UserSession` table |

#### 3.5.2 JWT Configuration

```
Algorithm:      HmacSha256
Issuer:         CRM.Api
Audience:       CRM.Client
Access expiry:  60 minutes
Refresh expiry: 7 days (rolling)
Secret:         Minimum 32 characters
```

#### 3.5.3 Password Policy

- Minimum 8 characters
- Requires uppercase, lowercase, digit, and symbol
- BCrypt hashing (rounds configurable)
- Password reset via email-verification link

#### 3.5.4 Data Protection

- All sensitive configuration values via environment variables / Kubernetes Secrets
- TLS enforced in all production deployments
- CORS configured to allowed origins only
- Optimistic concurrency via `RowVersion` on all entities

---

### 3.6 Frontend Architecture

#### 3.6.1 State Management

| Concern | Mechanism |
|---------|-----------|
| Auth state | `AuthContext` (React Context) |
| Theme | `ThemeContext` |
| Real-time | `SignalRContext` + custom `useSignalR` hook |
| Server state | Axios services with component-local state |
| Forms | Formik + Yup validation schemas |

#### 3.6.2 Routing Structure (selected pages)

| Route | Page |
|-------|------|
| `/dashboard` | `DashboardPage` |
| `/accounts` | `AccountsPage` |
| `/accounts/:id` | `AccountPage`, `AccountOverviewPage` |
| `/contacts` | `ContactsPage` |
| `/leads` | `LeadsPage` |
| `/opportunities` | `OpportunitiesPage` |
| `/quotes` | `QuotesPage` |
| `/orders` | `OrdersPage` |
| `/invoices` | `InvoicesPage` |
| `/payments` | `PaymentsPage` |
| `/contracts` | `ContractsPage` |
| `/subscriptions` | `SubscriptionsPage` |
| `/commissions` | `CommissionsPage` |
| `/campaigns` | `CampaignsPage` |
| `/email-templates` | `EmailTemplatesPage` |
| `/service-requests` | `ServiceRequestsPage` |
| `/service-requests/:id` | `ServiceRequestDetailPage` |
| `/knowledge-base` | `KnowledgeBasePage` |
| `/itsm/*` | ITSM sub-module pages |
| `/agents/*` | AI Agent pages |
| `/reports` | `ReportsPage` |
| `/analytics` | `AnalyticsPage` |
| `/admin/*` | Admin sub-pages |
| `/settings` | `SettingsPage` |
| `/users` | `UserManagementPage` |
| `/groups` | `GroupManagementPage` |
| `/departments` | `DepartmentManagementPage` |
| `/webhooks` | `WebhooksManagementPage` |
| `/import` | `ImportWizardPage` |
| `/export` | `ExportWizardPage` |

#### 3.6.3 Form Layout Standard

All data entry forms use a two-section layout:
1. **Core Section:** Required and primary fields
2. **Additional Information Accordion:** Optional/secondary fields in a collapsible section at the bottom

#### 3.6.4 Theme & Branding

- Light and dark themes
- Primary/secondary colors driven by `BrandingConfig` from API
- Custom color palette management
- Logo injection into header and login page

---

### 3.7 Pluggable Provider Architecture

#### 3.7.1 Pattern

Each integration category has:
1. A **Port interface** (`CRM.Core/Ports/Output/Providers/I{Category}Port.cs`)
2. A **Factory** (`CRM.Infrastructure/Factories/{Category}ProviderFactory.cs`)
3. A **BuiltIn provider** (always available, no external dependency)
4. One or more **external provider adapters**

Provider selection is driven by `appsettings.json` + `FeatureManagement` flags, overridable by environment variable.

#### 3.7.2 Feature Flags for Provider Selection

| Flag | Controls |
|------|---------|
| `UseExternalSearch` | Switch to Meilisearch, Algolia, etc. |
| `UseExternalChat` | Switch to Chatwoot, Intercom, etc. |
| `UseExternalNotifications` | Switch to Novu, Twilio, SendGrid, etc. |
| `UseExternalAnalytics` | Switch to Superset, Power BI, etc. |
| `UseExternalSignatures` | Switch to DocuSeal, DocuSign, etc. |
| `UseExternalAI` | Switch to OpenAI, Azure, Anthropic, etc. |
| `UseExternalIntegrations` | Switch to n8n, Zapier, etc. |

#### 3.7.3 AI Provider Configuration (example)

```json
{
  "Providers": {
    "AI": {
      "Type": "OpenAI",
      "OpenAI": { "ApiKey": "sk-...", "Model": "gpt-4o" },
      "Ollama": { "Url": "http://crm-ollama:11434", "Model": "llama3.1:8b" },
      "AzureOpenAI": { "Endpoint": "https://xxx.openai.azure.com/", "DeploymentName": "gpt-4o" }
    }
  }
}
```

---

### 3.8 Infrastructure & Deployment

#### 3.8.1 Docker Network Stacks

| Stack | Purpose | Services |
|-------|---------|----------|
| `crm-core` | Application | crm-api (5000), crm-frontend (80) |
| `crm-db` | Databases | crm-mariadb (3306), crm-redis (6379), crm-postgres (5432) |
| `crm-components` | Pluggable providers | Meilisearch (7700), Ollama (11434), Chatwoot (3000), Novu (3000), Superset (8088), DocuSeal (3000), n8n (5678) |

#### 3.8.2 Deployment Targets

| Target | Tool | Notes |
|--------|------|-------|
| **Local Dev** | `docker compose` | `docker/docker-compose.unified.yml` |
| **Dev Server** | `deploy-to-dev-server.sh` | SSH + Docker push to 192.168.0.9 |
| **Production (Docker)** | `docker/docker-compose.production.yml` | TLS, production secrets |
| **Kubernetes** | Helm charts in `kubernetes/` | HPA, resource limits, Ingress |
| **Azure** | Bicep templates in `azure/` | AKS, Azure Container Registry, Azure MySQL |
| **AWS** | Shell scripts in `scripts/` | ECS, ECR, RDS |
| **GCP** | Planned | GKE, Cloud SQL |

#### 3.8.3 Cross-Platform Build

Development on macOS (arm64) targeting Linux (amd64) server:

```bash
docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend .
```

#### 3.8.4 CI/CD

- Azure DevOps Pipelines (`azure-pipelines.yml.disabled` — activatable)
- AKS-specific pipeline for Kubernetes deployment
- Build artifacts published to `artifacts/`

#### 3.8.5 Monitoring & Observability

- Structured logging via `ILogger<T>` throughout all services
- Performance metrics controller (`PerformanceMonitoringController`)
- Worker health controller (`WorkerHealthController`)
- Provider health endpoint (`/api/health/providers`)
- Test results dashboard (`TestResultsPage` + `TestResultsController`)
- Background worker status (`WorkerControlController`)

---

### 3.9 Non-Functional Requirements

#### 3.9.1 Performance

| Requirement | Target |
|-------------|--------|
| API response time (p95) | < 200 ms for list endpoints |
| API response time (p95) | < 500 ms for complex aggregate queries |
| Concurrent users | 500+ (single node), horizontally scalable via K8s |
| Database query optimization | EF Core `AsNoTracking()` on read-only queries |
| Redis caching | Applied to settings, feature flags, and hot lookup data |

#### 3.9.2 Security

| Requirement | Implementation |
|-------------|----------------|
| Transport security | TLS 1.2+ enforced in production |
| Password storage | BCrypt with configurable work factor |
| Token security | Short-lived JWT + rotating refresh tokens |
| CSRF protection | State parameter on OAuth flows |
| Input validation | FluentValidation on all DTOs, Yup on frontend |
| Concurrency safety | EF Core optimistic concurrency (`RowVersion`) |
| Soft delete | `IsDeleted` flag; hard delete not permitted |
| Audit trail | Immutable audit log on all mutations |

#### 3.9.3 Scalability

- Stateless API (sessions tracked in database, not in-memory)
- Redis for distributed cache and SignalR backplane
- Horizontal scaling via Kubernetes HPA
- Database read replicas supported via EF Core connection string configuration

#### 3.9.4 Reliability

| Requirement | Implementation |
|-------------|----------------|
| Database migrations | Automated on startup via `MigrateAsync()` |
| Background processing | ASP.NET Core `IHostedService` workers |
| Provider failover | Factory pattern falls back to BuiltIn on provider error |
| Health checks | `/health`, `/health/ready`, `/health/live` endpoints |

#### 3.9.5 Maintainability

- `StyleCop` enforced on backend code
- SonarQube integration for static analysis
- All enum changes must update `SPEC-GEN-001-EnumReference.md`
- Feature specifications maintained in `docs/11-specifications/`
- Version tracked in `version.json` on every commit

#### 3.9.6 Compliance

- GDPR access logging (`GdprAccessLog`)
- Field-level change log for audit obligations
- Soft delete to preserve referential integrity
- Data export capability for data portability requests

---

## 4. Module Inventory

| Module | Spec File(s) | Status |
|--------|-------------|--------|
| Account Management | SPEC-CRM-001 | ✅ Implemented |
| Lead Management | SPEC-CRM-002 | ✅ Implemented |
| Opportunity Management | SPEC-CRM-003 | ✅ Implemented |
| Contact Management | SPEC-CRM-004 | ✅ Implemented |
| Activity Management | SPEC-CRM-005 | ✅ Implemented |
| Pipeline Management | SPEC-CRM-006 | ✅ Implemented |
| Task Management | SPEC-CRM-007 | ✅ Implemented |
| Account Data Normalization | SPEC-CRM-008 | ✅ Implemented |
| Quote Management | SPEC-SALES-001 | ✅ Implemented |
| Order Management | SPEC-SALES-002 | ✅ Implemented |
| Invoice Management | SPEC-SALES-003 | ✅ Implemented |
| Payment Management | SPEC-SALES-004 | ✅ Implemented |
| Contract Management | SPEC-SALES-005 | ✅ Implemented |
| Subscription Management | SPEC-SALES-006 | ✅ Implemented |
| Commission Management | SPEC-SALES-007 | ✅ Implemented |
| Campaign Management | SPEC-MKT-001 | ✅ Implemented |
| Email Templates | SPEC-MKT-002 | ✅ Implemented |
| Email Sequences | SPEC-MKT-003 | ✅ Implemented |
| Web Form Builder | SPEC-MKT-004 | ✅ Implemented |
| Web Tracking | SPEC-MKT-005 | ✅ Implemented |
| Service Request Management | SPEC-SD-001 | ✅ Implemented |
| Knowledge Base | SPEC-SD-002 | ✅ Implemented |
| SLA Management | SPEC-SD-003 | ✅ Implemented |
| Workflow Engine | SPEC-SD-004 | ✅ Implemented |
| Escalation Management | SPEC-SD-005 | ✅ Implemented |
| Incident Management (ITSM) | SPEC-ITSM-001 | ✅ Implemented |
| Problem Management (ITSM) | SPEC-ITSM-002 | ✅ Implemented |
| Change Management (ITSM) | SPEC-ITSM-003 | ✅ Implemented |
| CMDB | SPEC-ITSM-004 | ✅ Implemented |
| AI Agent Framework | SPEC-AI-* | ✅ Implemented |
| Churn Prediction | SPEC-AI-003 | ✅ Implemented |
| Email Intelligence | SPEC-AI-004 | ✅ Implemented |
| Reporting & Analytics | SPEC-AI-005 | ✅ Implemented |
| User Management | SPEC-SYS-001 | ✅ Implemented |
| Authentication | SPEC-SYS-002 | ✅ Implemented |
| Group Management | SPEC-SYS-003 | ✅ Implemented |
| Feature Flag Management | SPEC-SYS-004 | ✅ Implemented |
| System Settings | SPEC-SYS-005 | ✅ Implemented |
| Audit Logging | SPEC-SYS-006 | ✅ Implemented |
| Navigation Management | SPEC-SYS-007 | ✅ Implemented |
| Admin Settings Suite | SPEC-SYS-008 | ✅ Implemented |
| Administration Module | SPEC-SYS-009 | ✅ Implemented |
| UI Management | SPEC-SYS-010 | ✅ Implemented |
| Non-Functional Requirements | SPEC-SYS-011 | ✅ Implemented |
| RBAC | SPEC-SYS-012 | ✅ Implemented |
| Webhook Management | SPEC-INT-001 | ✅ Implemented |
| Provider Integration | SPEC-INT-002 | ✅ Implemented |
| Import/Export | SPEC-INT-003 | ✅ Implemented |
| Configuration Management | SPEC-ADMIN-001 | ✅ Implemented |

---

## 5. Data Model Summary

### Core Entities and Key Relationships

```
User ──────────────────── UserGroup (M:M via UserGroupMembers)
User ── Department (M:1)
User ── Team (M:M)
User ── Territory (M:M)

Account ─── Contact (M:M via AccountContacts)
Account ─── Opportunity (1:M)
Account ─── Lead (1:M)
Account ─── Contract (1:M)
Account ─── Subscription (1:M)
Account ─── ServiceRequest (1:M)

Opportunity ─── Quote (1:M)
Quote ──────── QuoteLineItem (1:M)
Quote ──────── Order (1:1 conversion)
Order ──────── Invoice (1:M)
Invoice ────── Payment (1:M)
Invoice ────── CreditMemo (1:M)

MarketingCampaign ─── CampaignRecipient (1:M)
MarketingCampaign ─── CampaignMetric (1:M)
MarketingCampaign ─── CampaignConversion (1:M)

ServiceRequest ─── SLAPolicy (M:1)
ServiceRequest ─── EscalationRule (M:1)
ServiceRequest ─── WorkflowInstance (1:M)

KnowledgeBase ─── ServiceRequest (M:M lookup)

Lead ─────── LeadScoreRule (M:M)
Lead ─────── LeadRoutingRule (M:1)

WorkflowDefinition ── WorkflowInstance (1:M)
WorkflowInstance ──── WorkflowTask (1:M)

WebhookEndpoint ─── WebhookEvent (1:M)
WebhookEvent ────── WebhookDelivery (1:M)
```

### Polymorphic Contact Info

```
Address / PhoneNumber / EmailAddress / SocialMediaAccount
    └── EntityAddressLink / EntityPhoneLink / EntityEmailLink / EntitySocialMediaLink
            └── links to: Account | Contact | Lead | User (via EntityType + EntityId)
```

---

## 6. API Endpoint Reference

### Authentication

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/auth/login` | Local login |
| POST | `/api/auth/logout` | Logout, revoke session |
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/refresh-token` | Refresh JWT |
| POST | `/api/auth/forgot-password` | Request password reset |
| POST | `/api/auth/reset-password` | Execute password reset |
| POST | `/api/auth/verify-2fa` | Verify TOTP code |
| GET/POST | `/api/auth/google` | Google OAuth flow |
| GET/POST | `/api/auth/github` | GitHub OAuth flow |
| POST | `/api/auth/webauthn/register` | Passkey registration |
| POST | `/api/auth/webauthn/authenticate` | Passkey assertion |

### Core CRM

| Resource | Endpoints |
|----------|-----------|
| Accounts | `/api/accounts` (CRUD + search + duplicates + normalize) |
| Contacts | `/api/contacts` (CRUD + search) |
| Leads | `/api/leads` (CRUD + convert + score + route) |
| Opportunities | `/api/opportunities` (CRUD + forecast) |
| Activities | `/api/activities` (CRUD) |
| Interactions | `/api/interactions` (CRUD) |
| Notes | `/api/notes` (CRUD) |
| Tasks | `/api/tasks` (CRUD) |
| Relationships | `/api/relationships` (CRUD + map) |
| Pipelines | `/api/pipelines` (CRUD) |
| Stages | `/api/stages` (CRUD) |

### Sales

| Resource | Endpoints |
|----------|-----------|
| Quotes | `/api/quotes` (CRUD + convert + PDF + sign) |
| Orders | `/api/orders` (CRUD) |
| Invoices | `/api/invoices` (CRUD + PDF) |
| Payments | `/api/payments` (CRUD) |
| Contracts | `/api/contracts` (CRUD + sign) |
| Subscriptions | `/api/subscriptions` (CRUD) |
| Commissions | `/api/commissions`, `/api/commissionplans`, `/api/commissionpayouts` |
| Products | `/api/products` (CRUD) |
| Product Bundles | `/api/productbundles` |
| Price Books | `/api/pricebooks` |
| Sales Quotas | `/api/salesquotas` |
| Sales Forecasts | `/api/salesforecasts` |
| Credit Memos | `/api/creditmemos` |

### Marketing

| Resource | Endpoints |
|----------|-----------|
| Campaigns | `/api/campaigns` (CRUD + execute + metrics) |
| Email Templates | `/api/emailtemplates` |
| Email Sequences | `/api/emailsequences` |
| Campaign Recipients | `/api/campaignrecipients` |
| Campaign Metrics | `/api/campaignmetrics` |
| Campaign Conversions | `/api/campaignconversions` |
| Forms | `/api/forms` |
| Landing Pages | `/api/landingpages` |

### Service Desk

| Resource | Endpoints |
|----------|-----------|
| Service Requests | `/api/servicerequests` (CRUD + assign + comment + SLA) |
| Knowledge Base | `/api/knowledge` (CRUD + search) |
| SLA Policies | `/api/slapolicies` |
| Escalation Rules | `/api/escalationrules` |
| Escalation Policies | `/api/escalationpolicies` |
| Workflows | `/api/workflows` (CRUD + trigger + instances) |
| Workflow Tasks | `/api/workflowtasks` |

### ITSM

| Resource | Endpoints |
|----------|-----------|
| Incidents | `/api/incidents` |
| Problems | `/api/problems` |
| Changes | `/api/changes` |
| CMDB / CIs | `/api/configurationitems` |
| ITSM Dashboard | `/api/itsm/dashboard` |

### AI & Agents

| Resource | Endpoints |
|----------|-----------|
| Agents | `/api/agents` (list, chat, admin) |
| Agent Chat | `POST /api/agents/{agentId}/chat` |
| Agent Analytics | `/api/agentanalytics/usage` |
| AI Lead Scoring | `/api/aileadscoring` |
| AI Email | `/api/aiemail` |
| AI Chatbot | `/api/aichatbot` |
| AI Analytics | `/api/aianalytics` |

### System & Admin

| Resource | Endpoints |
|----------|-----------|
| Users | `/api/users` |
| Roles | `/api/roles` |
| Permissions | `/api/permissions` |
| Groups | `/api/usergroups` |
| Departments | `/api/departments` |
| Feature Flags | `/api/featureflags` |
| System Settings | `/api/systemsettings` |
| Audit Logs | `/api/auditlogs` |
| Dashboard | `/api/dashboard` |
| Reports | `/api/reports` |
| Analytics | `/api/analytics` |
| Webhooks | `/api/webhooks` |
| Import Jobs | `/api/importjobs` |
| Export Jobs | `/api/exportjobs` |
| Branding | `/api/branding` |
| Navigation | `/api/navigation` |
| Preferences | `/api/preferences` |
| Health | `/health`, `/api/health/providers` |

---

*This document reflects the solution as of version 0.577.0, February 24, 2026.*  
*For detailed per-feature specifications, refer to `docs/11-specifications/INDEX.md`.*  
*For architecture decisions, refer to `docs/01-architecture/`.*
