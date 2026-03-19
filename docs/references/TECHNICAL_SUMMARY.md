# CRM Solution — Technical Summary

> Generated: March 2026 | Version: 0.625.1

---

## 1. Quick Summary

The CRM Solution is an enterprise-grade Customer Relationship Management platform built as a **modular monolith** with an optional microservices split. It covers the full CRM lifecycle — leads, opportunities, accounts, contacts, service desk, marketing, billing, and analytics — plus an extensible open-source pluggable stack for search, chat, notifications, AI, e-signatures, integrations, and workflow automation.

---

## 2. Architecture Writeup

### Architectural Style

| Aspect | Decision |
|---|---|
| Primary runtime | Modular monolith (ASP.NET Core) |
| Optional runtime | Microservices split (8 services) |
| API contract style | DTO-first; entities never exposed directly |
| External capability model | Ports & Adapters (provider factories, feature-flag controlled) |
| Persistence | EF Core code-first; single `crm_db` database |
| Frontend | React 18 SPA via Nginx |
| Realtime | SignalR hub |
| AI/Agents | Semantic Kernel + provider abstraction (OpenAI, Groq, Ollama) |

### High-Level Diagram

```
Users / Browsers
       │
       ▼
crm-frontend  (React 18 SPA, TypeScript, Material UI)
       │
       ▼
crm-api  (ASP.NET Core 10, 209 REST controllers)
       │
       ├──► crm-mariadb   (system of record, EF Core)
       ├──► crm-redis     (cache / session / SignalR backplane)
       └──► Provider ports  (resolved via factory + feature flags)
               ├── Meilisearch   (search)
               ├── Chatwoot      (live chat)
               ├── Novu          (notifications)
               ├── Apache Superset (analytics)
               ├── DocuSeal      (e-signatures)
               ├── n8n           (integrations / workflow)
               └── Ollama        (local AI inference)
```

### Core Layers

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `CRM.Frontend` | React SPA — pages, components, hooks, contexts, service wrappers |
| API | `CRM.Api` | REST controllers, auth middleware, validation, health endpoints |
| Domain / Application | `CRM.Core` | Service interfaces, entities, DTOs, domain events |
| Infrastructure | `CRM.Infrastructure` | EF Core DbContext, migrations, provider factories, adapters |
| Workers | `CRM.Workers` | Background hosted services (billing, dunning, analytics flush) |
| Script Engine | `crm-script-runner` | Node.js/TypeScript plug-in script runtime |

### Microservices Split (optional, not primary runtime)

| Service | Domain |
|---|---|
| CRM.Gateway | API gateway / routing |
| CRM.Identity | Auth / JWT / user management |
| CRM.CustomerService | Accounts, contacts |
| CRM.CoreService | Core CRM shared services |
| CRM.SalesService | Leads, opportunities, quotes, orders |
| CRM.MarketingService | Campaigns, segments, lead scoring |
| CRM.ServiceDeskService | Incidents, changes, problems, ITSM |
| CRM.ServiceDefaults | Shared Aspire-style service defaults |

---

## 3. Module Count

| Module / Component | Description |
|---|---|
| **CRM.Api** | REST API host — 209 controllers across CRM, ITSM, Marketing, Admin, AI, Portal, and Webhook sub-domains |
| **CRM.Core** | Domain layer — 259 entities, 206 service interfaces |
| **CRM.Infrastructure** | EF Core persistence, 3 applied migrations, provider adapters |
| **CRM.Workers** | Background services (billing cycles, dunning, event processors) |
| **CRM.Frontend** | React SPA — ~80 pages, ~60 component directories |
| **crm-script-runner** | Node.js/TypeScript scripting engine with stdlib and tool bridge |
| **Database** | 25 SQL migration scripts, 17 seed data scripts |
| **e2e-tests** | 77 Playwright spec files across 3 configuration profiles (BVT, comprehensive, default) |
| **Microservices** | 8 optional microservice projects |

---

## 4. Lines of Code

| Language / Type | Lines | Notes |
|---|---|---|
| **C#** | ~63,800 | Backend (API, Core, Infrastructure, Workers, tests) |
| **TypeScript / TSX** | ~272,900 | Frontend SPA + E2E Playwright tests |
| **SQL** | ~24,600 | Schema migrations and seed data |
| **Python** | ~73,200 | Analysis, data conversion, and tooling scripts |
| **JavaScript** | ~1,500 | Config scripts and legacy helpers |
| **Total** | **~436,000** | Excluding `node_modules`, `dist`, `obj`, `bin` |

---

## 5. Test Cases by Type

### Backend (xUnit — C#)

| Test Category | Test Methods | Location |
|---|---|---|
| Unit — Services | ~2,219 | `tests/Services/` |
| Unit — Controllers | ~454 | `tests/Controllers/` |
| Unit — Validators | ~329 | `tests/Validators/` |
| Unit — Entities / Models | counted in services | `tests/Entities/`, `tests/Models/` |
| Integration | ~638 | `tests/Integration/` |
| BVT (Build Verification) | ~221 | `tests/BVT/` |
| Functional | ~127 | `tests/Functional/` |
| Performance | ~1 | `tests/Performance/` |
| **Backend Total** | **~10,820** | All `[Fact]` / `[Theory]` methods |

### Frontend (Jest — TypeScript/TSX)

| Test Category | Test Cases | Location |
|---|---|---|
| Unit / Component tests | ~1,041 | `CRM.Frontend/src/__tests__/` |
| **Frontend Total** | **~1,041** | |

### End-to-End (Playwright — TypeScript)

| Test Category | Test Cases | Location |
|---|---|---|
| E2E scenarios | ~1,547 | `e2e-tests/tests/` (77 spec files) |
| **E2E Total** | **~1,547** | |

### Script Engine (Jest — TypeScript)

| Test Category | Test Cases | Location |
|---|---|---|
| CDT engine unit tests | ~12 | `crm-script-runner/tests/` |

### Overall Test Summary

| Category | Count |
|---|---|
| Backend unit + integration + BVT | ~10,820 |
| Frontend unit | ~1,041 |
| E2E (Playwright) | ~1,547 |
| CDT engine | ~12 |
| **Grand Total** | **~13,420** |

---

## 6. Technology Stack

| Area | Technology |
|---|---|
| Backend runtime | ASP.NET Core 10, C# 13 |
| ORM | Entity Framework Core (code-first, MariaDB primary) |
| Frontend | React 18, TypeScript, Material UI v5 |
| Database | MariaDB (primary), PostgreSQL and SQL Server (selected scenarios) |
| Caching | Redis |
| Realtime | SignalR |
| AI / LLM | Semantic Kernel, OpenAI, Groq, Ollama |
| Search | Meilisearch (optional), built-in full-text |
| Notifications | Novu (optional), built-in SMTP/SMS |
| Chat | Chatwoot (optional) |
| Analytics | Apache Superset (optional), built-in dashboards |
| E-Signatures | DocuSeal (optional) |
| Workflow / Integration | n8n (optional) |
| Script Engine | Node.js, TypeScript |
| Packaging | Docker Compose (primary), Kubernetes manifests (optional) |
| CI / CD | Azure DevOps Pipelines |
| Code Quality | SonarCloud, StyleCop |
| Testing | xUnit (backend), Jest + React Testing Library (frontend), Playwright (E2E) |

---

## 7. Key Functional Domains

| Domain | Key Features |
|---|---|
| **CRM Core** | Accounts, Contacts, Leads, Opportunities, Activities, Notes, Tasks |
| **Sales** | Pipelines, Quotes, CPQ, Orders, Contracts, Invoices, Commissions, Forecasting |
| **Subscriptions & Billing** | Subscription lifecycle, usage billing, dunning, revenue analytics |
| **Marketing** | Campaigns, Segments, Lead Scoring, Email Sequences, Landing Pages |
| **Service Desk / ITSM** | Incidents, Problems, Changes, CI/CMDB, SLA Policies, Escalations, Knowledge Base |
| **Customer Portal** | Self-service ticket submission, knowledge base, CSAT surveys |
| **Analytics & Reports** | Dashboards, cohort analysis, real-time SignalR widgets |
| **AI & Agents** | Lead scoring agents, AI insights, chatbot, LLM provider abstraction |
| **Admin** | User management, RBAC, audit logs, feature flags, system settings, import/export |
| **Integrations** | Webhooks, Stripe, Twilio, SendGrid, Calendly, n8n, DocuSign, LinkedIn, Slack, Teams |

---

*This document reflects the repository state as of March 2026 (v0.625.1).*
