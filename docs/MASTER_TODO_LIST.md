# CRM Solution - Master TODO List (Pending Items Only)

> **Last Updated:** February 11, 2026
> **Purpose:** Consolidated list of all PENDING enhancements and action items
> **Total Pending Items:** 142
> **Completed items have been archived — see git history for prior state**

---

## Implementation Plan Reference

> **[specifications/IMPLEMENTATION_PLAN.md](specifications/IMPLEMENTATION_PLAN.md)** - Detailed 16-week implementation guide
>
> **Specification Progress:** 9/40 complete (22.5%) — see [specifications/INDEX.md](specifications/INDEX.md)

---

## Table of Contents

1. [Feature Specification TODOs](#1-feature-specification-todos)
2. [Audit Remediation TODOs](#2-audit-remediation-todos)
3. [ITSM Remaining Work](#3-itsm-remaining-work)
4. [Infrastructure & DevOps](#4-infrastructure--devops)
5. [Self-Service Portal](#5-self-service-portal)
6. [Documentation](#6-documentation)
7. [UX/UI Improvements](#7-uxui-improvements)
8. [AI & Machine Learning](#8-ai--machine-learning)
9. [Analytics & Reporting](#9-analytics--reporting)
10. [Integration Framework](#10-integration-framework)
11. [Customization Engine](#11-customization-engine)
12. [CRM Gaps](#12-crm-gaps)
13. [Priority Matrix](#13-priority-matrix)

---

## 1. Feature Specification TODOs

*Extracted from completed feature specifications. See individual spec files for full context.*

### SPEC-CRM-001 (Account Management) — 10 Items

| ID | Priority | Description | Spec Section |
|----|----------|-------------|--------------|
| TODO-CRM001-01 | P1 | Implement frontend field-level validation matching backend rules | 2.3 |
| TODO-CRM001-02 | P2 | Add bulk import/export functionality for accounts | 2.2 |
| TODO-CRM001-03 | P2 | Implement account merge UI for duplicate resolution | 2.2 |
| TODO-CRM001-04 | P2 | Add account hierarchy visualization (parent/child tree) | 2.2 |
| TODO-CRM001-05 | P2 | Implement territory assignment UI in account details | 2.2 |
| TODO-CRM001-06 | P2 | Add health score calculation service and display | 2.2 |
| TODO-CRM001-07 | P3 | Implement account timeline aggregation from all related entities | 2.2 |
| TODO-CRM001-08 | P1 | Add missing backend validations (duplicate email check, phone format) | 3.5 |
| TODO-CRM001-09 | P2 | Implement soft delete cascade for related contacts/opportunities | 3.4 |
| TODO-CRM001-10 | P1 | Add database indexes for frequently queried columns | 4.5 |

### SPEC-CRM-002 (Lead Management) — 8 Items

| ID | Priority | Description | Spec Section |
|----|----------|-------------|--------------|
| TODO-CRM002-01 | P2 | Implement lead scoring algorithm with configurable weights | 2.2 |
| TODO-CRM002-02 | P1 | Implement lead conversion workflow (Lead → Account + Contact + Opportunity) | 2.2 |
| TODO-CRM002-03 | P2 | Add lead source tracking and attribution | 2.2 |
| TODO-CRM002-04 | P2 | Implement web-to-lead form builder integration | 2.2 |
| TODO-CRM002-05 | P2 | Add duplicate lead detection during creation | 3.5 |
| TODO-CRM002-06 | P2 | Implement lead nurturing campaign integration | 2.2 |
| TODO-CRM002-07 | P3 | Add lead aging alerts and stale lead notifications | 2.2 |
| TODO-CRM002-08 | P3 | Implement lead qualification matrix (BANT/MEDDIC) | 2.2 |

### SPEC-CRM-003 (Opportunity Management) — 8 Items

| ID | Priority | Description | Spec Section |
|----|----------|-------------|--------------|
| TODO-CRM003-01 | P2 | Implement weighted pipeline value calculation | 2.2 |
| TODO-CRM003-02 | P1 | Add sales stage probability automation | 2.2 |
| TODO-CRM003-03 | P2 | Implement competitor tracking on opportunities | 2.2 |
| TODO-CRM003-04 | P2 | Add opportunity product line items management | 2.2 |
| TODO-CRM003-05 | P2 | Implement win/loss analysis reports | 2.2 |
| TODO-CRM003-06 | P3 | Add opportunity cloning functionality | 2.2 |
| TODO-CRM003-07 | P2 | Implement forecast category assignment | 2.2 |
| TODO-CRM003-08 | P2 | Add opportunity team/split commission tracking | 2.2 |

### SPEC-SALES-006 (Subscription Management) — 5 Items

| ID | Priority | Description | Spec Section |
|----|----------|-------------|--------------|
| TODO-SALES006-01 | P2 | Implement MRR/ARR dashboard widgets | 2.2 |
| TODO-SALES006-02 | P2 | Add usage-based billing metering | 3.4 |
| TODO-SALES006-03 | P2 | Implement subscription upgrade/downgrade proration | 3.4 |
| TODO-SALES006-04 | P3 | Add churn prediction integration with AI module | 3.4 |
| TODO-SALES006-05 | P2 | Implement dunning management for failed payments | 3.4 |

### SPEC-SALES-007 (Commission Management) — 5 Items

| ID | Priority | Description | Spec Section |
|----|----------|-------------|--------------|
| TODO-SALES007-01 | P2 | Implement tiered commission calculation engine | 3.4 |
| TODO-SALES007-02 | P2 | Add commission statement PDF generation | 3.4 |
| TODO-SALES007-03 | P2 | Implement accelerator/decelerator rules | 3.4 |
| TODO-SALES007-04 | P3 | Add commission forecast based on pipeline | 3.4 |
| TODO-SALES007-05 | P2 | Implement clawback automation for churned deals | 3.4 |

---

## 2. Audit Remediation TODOs

*From Phase 9 multi-agent audit (February 13, 2026). See SOLUTION_GAPS_REMEDIATION_PLAN.md Phase 9.4.*

### 2.1 Orphaned Frontend Components (21 total)

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-AUDIT-01~~ | ~~P2~~ | ✅ **DONE** — Wired 16 ITSM components into 9 pages (IncidentDetail, ProblemDetail, CMDBDetail, SLADashboard, ChangeDetail, KnowledgeArticleDetail, ServiceCatalog, ServiceCatalogRequestCreate, IncidentForm) |
| ~~TODO-AUDIT-02~~ | ~~P3~~ | ✅ **DONE** — ChatTimelineItem.tsx used in CustomerOverviewPage.tsx; AnalyticsEmbed.tsx used in DashboardPage.tsx. Both actively imported. |
| ~~TODO-AUDIT-03~~ | ~~P3~~ | ✅ **DONE** — Deleted dead ModuleFieldSettingsTab.tsx (superseded by ModuleFieldSettingsTabNew.tsx) |

### 2.2 Orphaned Admin Pages (3)

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-AUDIT-04~~ | ~~P2~~ | ✅ **DONE** — Added lazy imports + routes for /admin/database-settings, /admin/duplicate-rules, /admin/lead-score-rules |

### 2.3 Dead Custom Hooks (3)

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-AUDIT-05~~ | ~~P3~~ | ✅ **DONE** — Deleted 3 hook files + removed barrel export |

### 2.4 ITSM Architecture Gap

| ID | Priority | Description |
|----|----------|-------------|
| TODO-AUDIT-06 | P2 | Create itsmService.ts with typed service objects + interfaces (previously marked done but file does not exist) |
| TODO-AUDIT-07 | P3 | Migrate 31 ITSM pages from Tailwind CSS to MUI components |

### 2.5 Backend Test Coverage

| ID | Priority | Description |
|----|----------|-------------|
| TODO-AUDIT-08 | P2 | Re-enable ~87 excluded test files in CRM.Tests.csproj (entity property drift, mock setup) |
| ~~TODO-AUDIT-09~~ | ~~P3~~ | ✅ **DONE** — Created 5 interfaces + 5 service implementations + 6 DbSets + DI registration |
| ~~TODO-AUDIT-10~~ | ~~P3~~ | ✅ **DONE** — Removed 7 legacy alias routes (/incidents, /knowledge, /catalog) from App.tsx |

### 2.6 Remaining Service Gaps

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-AUDIT-11~~ | ~~P2~~ | ✅ **DONE** — Implemented RestoreEntityFromSnapshot, ReverseRelatedRecordRelinking, ReverseFieldOverrides in MergeService.cs |
| TODO-AUDIT-12 | P2 | Align ITSM_ADVANCED entity models (28 services, 460+ build errors from property mismatches) |

---

## 3. ITSM Remaining Work

### 3.1 ITSM Advanced Services (Deferred)

| ID | Priority | Description |
|----|----------|-------------|
| TODO-ITSM-01 | P3 | Align entity models for 28 ITSM_ADVANCED services (ITSM_ADVANCED constant is active in Directory.Build.props) |
| TODO-ITSM-02 | P3 | Fix 460+ build errors in advanced services (AssetLifecycle, KCSWorkflow, ImpactAnalysis, CABWorkflow, etc.) |
| TODO-ITSM-03 | P2 | Implement KnowledgeManagementService AI-powered semantic search |

### 3.2 Database & Testing

| ID | Priority | Description |
|----|----------|-------------|
| TODO-ITSM-04 | P2 | Execute database migration 010_itsm_module.sql on production |
| TODO-ITSM-05 | P2 | Execute seed data 011_itsm_seed_data.sql on production |
| TODO-ITSM-06 | P2 | Create ITSM service unit tests (7 files for core ITSM services) |
| TODO-ITSM-07 | P2 | Create ITSM controller integration tests |
| TODO-ITSM-08 | P3 | Create Playwright E2E tests for ITSM flows |

### 3.3 Frontend

| ID | Priority | Description |
|----|----------|-------------|
| TODO-ITSM-09 | P2 | Create frontend unit tests (Jest) for ITSM components |

---

## 4. Infrastructure & DevOps

### 4.1 Background Processing

| ID | Priority | Description |
|----|----------|-------------|
| TODO-INFRA-01 | P2 | Implement background job processing (Hangfire or Quartz.NET) |
| TODO-INFRA-02 | P2 | Add retry policies for external provider calls |
| TODO-INFRA-03 | P2 | Implement circuit breaker for provider failover |

### 4.2 Message Queue

| ID | Priority | Description |
|----|----------|-------------|
| TODO-INFRA-04 | P3 | Add RabbitMQ/Redis Streams for async event processing |
| TODO-INFRA-05 | P3 | Implement event sourcing for audit-critical entities |
| TODO-INFRA-06 | P3 | Add dead letter queue handling |
| TODO-INFRA-07 | P3 | Implement saga pattern for distributed transactions |

### 4.3 Search

| ID | Priority | Description |
|----|----------|-------------|
| TODO-INFRA-08 | P2 | Implement full-text search indexing for all entities |
| TODO-INFRA-09 | P2 | Add search result highlighting and faceted search |
| TODO-INFRA-10 | P3 | Implement search analytics (popular queries, zero results) |

---

## 5. Self-Service Portal

### 5.1 Community Features

| ID | Priority | Description |
|----|----------|-------------|
| TODO-PORTAL-01 | P3 | Customer portal with ticket submission and tracking |
| TODO-PORTAL-02 | P3 | Self-service KB search with article feedback |
| TODO-PORTAL-03 | P3 | Partner portal with deal registration |
| TODO-PORTAL-04 | P3 | Community forums with moderation tools |

### 5.2 Personalization

| ID | Priority | Description |
|----|----------|-------------|
| TODO-PORTAL-05 | P3 | User-configurable dashboard layouts |
| TODO-PORTAL-06 | P3 | Saved search and filter presets |
| TODO-PORTAL-07 | P3 | Custom notification preferences per entity type |
| TODO-PORTAL-08 | P3 | Personalized email digest configuration |

### 5.3 Mobile & PWA

| ID | Priority | Description |
|----|----------|-------------|
| TODO-PORTAL-09 | P3 | Progressive Web App (PWA) support |
| TODO-PORTAL-10 | P3 | Offline mode for core CRM features |
| TODO-PORTAL-11 | P3 | Push notifications for mobile |
| TODO-PORTAL-12 | P3 | Touch-optimized UI for tablets |

---

## 6. Documentation

### 6.1 ITSM Documentation

| ID | Priority | Description |
|----|----------|-------------|
| TODO-DOC-01 | P2 | Create ITSM User Guide |
| TODO-DOC-02 | P2 | Update README.md with ITSM module section |
| TODO-DOC-03 | P2 | Update architecture diagrams for ITSM services |

### 6.2 General Documentation

| ID | Priority | Description |
|----|----------|-------------|
| TODO-DOC-04 | P2 | Update Swagger/OpenAPI documentation for all new endpoints |
| TODO-DOC-05 | P3 | Fix critical StyleCop warnings (~1895 remaining) |
| TODO-DOC-06 | P3 | Add missing XML documentation to public APIs |
| TODO-DOC-07 | P2 | Final integration testing documentation |

---

## 7. UX/UI Improvements

### 7.1 Accessibility (WCAG 2.1 AA)

| ID | Priority | Description |
|----|----------|-------------|
| TODO-UX-01 | P2 | Add ARIA labels to all interactive components |
| TODO-UX-02 | P2 | Implement keyboard navigation for data grids |
| TODO-UX-03 | P2 | Add screen reader support for charts and dashboards |
| TODO-UX-04 | P3 | High contrast theme option |
| TODO-UX-05 | P3 | Font size adjustment controls |

### 7.2 Important UI Features

| ID | Priority | Description |
|----|----------|-------------|
| TODO-UX-06 | P1 | Implement global search with typeahead |
| TODO-UX-07 | P1 | Add inline editing for data grid cells |
| TODO-UX-08 | P2 | Implement drag-and-drop pipeline board |
| TODO-UX-09 | P2 | Add bulk action toolbar for list views |
| TODO-UX-10 | P2 | Implement advanced filter builder UI |

### 7.3 Nice-to-Have Enhancements

| ID | Priority | Description |
|----|----------|-------------|
| TODO-UX-11 | P3 | Dark mode toggle |
| TODO-UX-12 | P3 | Customizable sidebar navigation |
| TODO-UX-13 | P3 | Split view for comparing records |
| ~~TODO-UX-14~~ | ~~P3~~ | ✅ **DONE** — Breadcrumbs.tsx component implemented and rendered in App.tsx |
| TODO-UX-15 | P3 | Recent items quick access |

---

## 8. AI & Machine Learning

### 8.1 Predictive Analytics

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-AI-01~~ | ~~P2~~ | ✅ **DONE** — LeadScoringAgent with BANT rubric via Semantic Kernel |
| ~~TODO-AI-02~~ | ~~P2~~ | ✅ **DONE** — DealIntelligenceAgent analyzes deal health |
| TODO-AI-03 | P3 | Customer churn prediction |
| TODO-AI-04 | P3 | Next best action recommendations |

### 8.2 Conversational AI

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-AI-05~~ | ~~P2~~ | ✅ **DONE** — KnowledgeExpertAgent + Qdrant vector search |
| ~~TODO-AI-06~~ | ~~P3~~ | ✅ **DONE** — EmailAssistantAgent with template-aware drafting |
| TODO-AI-07 | P3 | Automated email sentiment analysis |
| TODO-AI-08 | P3 | Meeting summary generation |

### 8.3 Revenue Intelligence

| ID | Priority | Description |
|----|----------|-------------|
| TODO-AI-09 | P3 | Deal risk scoring |
| TODO-AI-10 | P3 | Revenue forecasting with ML |

---

## 9. Analytics & Reporting

### 9.1 Report Builder

| ID | Priority | Description |
|----|----------|-------------|
| TODO-RPT-01 | P2 | Custom report designer component |
| TODO-RPT-02 | P2 | Scheduled report delivery (email PDF/CSV) |
| TODO-RPT-03 | P2 | Report sharing and permissions |
| TODO-RPT-04 | P3 | Report templates marketplace |

### 9.2 Advanced Analytics

| ID | Priority | Description |
|----|----------|-------------|
| TODO-RPT-05 | P2 | Custom dashboard builder with drag-and-drop widgets |
| TODO-RPT-06 | P2 | Real-time dashboard with WebSocket live updates |
| TODO-RPT-07 | P2 | Cohort analysis and customer segmentation |
| TODO-RPT-08 | P3 | Funnel visualization with stage conversion rates |
| TODO-RPT-09 | P3 | Geographic data visualization (map charts) |

---

## 10. Integration Framework

### 10.1 Framework Enhancements

| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT-01 | P2 | Add Stripe webhook handlers for payment processing |
| TODO-INT-02 | P2 | Add SendGrid event tracking integration |
| TODO-INT-03 | P2 | Complete Chatwoot timeline integration |

### 10.2 Native Integrations

| ID | Priority | Description |
|----|----------|-------------|
| TODO-INT-04 | P2 | Google Workspace (Calendar, Contacts, Drive) |
| TODO-INT-05 | P2 | Microsoft 365 (Outlook, Teams, SharePoint) |
| TODO-INT-06 | P2 | Slack integration for notifications |
| TODO-INT-07 | P3 | Twilio enhanced voice call logging |
| TODO-INT-08 | P3 | QuickBooks/Xero accounting sync |
| TODO-INT-09 | P3 | Mailchimp/HubSpot marketing sync |
| TODO-INT-10 | P3 | LinkedIn Sales Navigator integration |
| TODO-INT-11 | P3 | Calendly/Cal.com scheduling integration |
| TODO-INT-12 | P3 | Jira/Azure DevOps project management sync |

---

## 11. Customization Engine

### 11.1 Dynamic Fields

| ID | Priority | Description |
|----|----------|-------------|
| TODO-CUST-01 | P2 | Custom field builder with drag-and-drop UI |
| TODO-CUST-02 | P2 | Custom field validation rules |
| TODO-CUST-03 | P2 | Custom field search and filtering |

### 11.2 UI Customization

| ID | Priority | Description |
|----|----------|-------------|
| TODO-CUST-04 | P3 | Custom page layouts per entity type |
| TODO-CUST-05 | P3 | Configurable list view columns |
| TODO-CUST-06 | P3 | Custom button/action definitions |

### 11.3 Calculated Fields & Environments

| ID | Priority | Description |
|----|----------|-------------|
| TODO-CUST-07 | P3 | Formula fields with expression engine |
| TODO-CUST-08 | P3 | Rollup summary fields |
| TODO-CUST-09 | P3 | Cross-object formula references |
| TODO-CUST-10 | P3 | Sandbox environment support |
| TODO-CUST-11 | P3 | Configuration migration between environments |
| TODO-CUST-12 | P3 | Feature flag management UI |

---

## 12. CRM Gaps

### 12.1 Sales Process

| ID | Priority | Description |
|----|----------|-------------|
| ~~TODO-GAP-01~~ | ~~P1~~ | ✅ **DONE** — MergeService UnmergeRecords fully implemented with reflection-based snapshot restoration |
| TODO-GAP-02 | P1 | Implement lead conversion workflow end-to-end |
| TODO-GAP-03 | P2 | Add sales forecasting service implementation |
| TODO-GAP-04 | P2 | Implement territory-based lead assignment |
| TODO-GAP-05 | P2 | Add multi-currency support for opportunities/quotes |

### 12.2 CPQ Enhancements

| ID | Priority | Description |
|----|----------|-------------|
| TODO-GAP-06 | P2 | Bundle configuration wizard UI |
| TODO-GAP-07 | P2 | Dynamic pricing rules engine integration |
| TODO-GAP-08 | P2 | Quote approval workflow with email notifications |

### 12.3 Lead Intelligence

| ID | Priority | Description |
|----|----------|-------------|
| TODO-GAP-09 | P2 | Company enrichment from external data sources |

---

## 13. Priority Matrix

### Summary by Priority

| Priority | Count | Description |
|----------|-------|-------------|
| **P0 — Critical** | 0 | No critical blockers |
| **P1 — High** | 8 | Core functionality gaps: validations, lead conversion, global search, inline editing |
| **P2 — Medium** | 80 | Service completion, testing, integrations, reporting, AI features |
| **P3 — Low** | 50 | Portal, mobile, advanced customization, nice-to-have UX |
| **Total** | **138** | |

### Recommended Implementation Order

| Phase | Focus | Items | Timeline |
|-------|-------|-------|----------|
| **Next Sprint** | P1 items + Test coverage | ~12 | Q1 2026 |
| **Sprint 2** | ITSM wiring + Audit cleanup | ~15 | Q1 2026 |
| **Sprint 3** | Documentation + Integration framework | ~15 | Q1 2026 |
| **Sprint 4** | AI/Analytics + Reporting | ~20 | Q2 2026 |
| **Backlog** | Portal, Mobile, Customization | ~57 | 2026-2027 |

---

**END OF MASTER TODO LIST**
