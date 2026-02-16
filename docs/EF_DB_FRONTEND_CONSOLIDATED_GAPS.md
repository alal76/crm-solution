# EF / Database / Frontend: Consolidated Gap Analysis & Remediation Tasks

> **Created:** February 9, 2026
> **Last Updated:** February 12, 2026
> **Methodology:** Systematic comparison of EF DbSet entities (221), SQL schema tables (222), and Frontend TypeScript types (200+)
> **Scope:** Naming inconsistencies, missing schema, missing types, orphaned definitions, and cross-layer alignment

---

## Remediation History

| Date | Commit | Description |
|------|--------|-------------|
| 2026-02-09 | `3c468c6` | Fixed duplicate DbSets and territory naming |
| 2026-02-09 | `08bf463` | Created 27 missing database tables (migration 019) |
| 2026-02-09 | `43d5433` | Fixed Subscription/LLMProviderSetting table mappings |
| 2026-02-09 | `23bb8e6` | Fixed ITSM SLA columns and ticket number generation |
| 2026-02-09 | `c8ba986` | Dropped orphan tables and bogus FK constraints |
| 2026-02-12 | `f31c638` | Renamed Customer→Account terminology across 34 frontend files |
| 2026-02-12 | *(pending)* | Fixed EntitySelect.tsx customerForm→accountForm state variable naming |

> **Related Documents:**
> - [EF_DB_FRONTEND_REMEDIATION_SUMMARY.md](EF_DB_FRONTEND_REMEDIATION_SUMMARY.md) -- Detailed remediation log
> - [ENTITY_DB_ALIGNMENT_REPORT.md](ENTITY_DB_ALIGNMENT_REPORT.md) -- Full 221-entity cross-reference

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Naming Inconsistencies](#2-naming-inconsistencies)
3. [EF Entities Missing Database Tables](#3-ef-entities-missing-database-tables)
4. [Database Tables Missing EF Entities](#4-database-tables-missing-ef-entities)
5. [Backend Entities Missing Frontend Types](#5-backend-entities-missing-frontend-types)
6. [Frontend Types Missing Backend Entities](#6-frontend-types-missing-backend-entities)
7. [Controllers Without Frontend Services](#7-controllers-without-frontend-services)
8. [EF Entities Without API Controllers](#8-ef-entities-without-api-controllers)
9. [Duplicate / Conflicting Definitions](#9-duplicate--conflicting-definitions)
10. [Workflow Schema Divergence](#10-workflow-schema-divergence)
11. [Consolidated Remediation Task List](#11-consolidated-remediation-task-list)

---

## Overview

This document consolidates the most recent audits and remediation work across the EF model, database schema, and frontend TypeScript types. It is intended to present a high-level summary, a short list of recent audits with timestamps, a prioritized consolidated gaps summary (P0/P1/P2), and a concise actionable TODO list referencing canonical task IDs so teams can pick up work immediately.

Scope: EF DbSet entities, MariaDB schema, Backend controllers, Frontend types/services, and cross-layer alignment for the Quote-to-Cash, Sales Performance, Pricing, Marketing Automation, ITSM, and Workflow subsystems.

## Recent Audits (timestamps)

- `docs/audits/WORKFLOW_BACKEND_AUDIT.md` — Generated: 2026-02-12 (comprehensive workflow entities, DTOs, services, controllers, DbSet and DI registration audit)
- `docs/SOLUTION_GAPS_REMEDIATION_PLAN.md` — Last Updated: 2026-02-11 (active remediation plan & phase progress)
- `docs/11-11-11-specifications/INDEX.md` — Last Updated: 2026-02-12 (spec index reflecting completed specs and planned items)
- `docs/MASTER_TODO_LIST.md` — Last Updated: 2026-02-08 (master list of extracted TODOs and priorities)
- `docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md` — Last Updated: 2026-02-05 (provider implementation tracker)
- `docs/EF_DB_FRONTEND_REMEDIATION_SUMMARY.md` — (see remediation history commits listed above) — latest updates applied Feb 12, 2026

Notes: Use `WORKFLOW_BACKEND_AUDIT.md` (2026-02-12) as the source of truth for workflow-specific entity/enum/interface alignment. Use the remediation plan (2026-02-11) for phase and priority context when scheduling work.

## Consolidated Gaps Summary (grouped by priority)

P0 — Critical (Data integrity, naming conflicts, duplicates)
- Current open P0 items: NONE — Primary P0 work (duplicate DbSet, Account/Customer naming, territory rename) completed 2026-02-09 → 2026-02-12 (see Phase 1: Naming & Duplicate Resolution). Continue monitoring for regressions.

P1 — High (Missing core API surface and controller coverage)
- Controllers that remain to be implemented (high priority for enabling core UI/workflows):
	- `OrdersController` (3.1)
	- `InvoicesController` (3.2)
	- `SubscriptionsController` (3.3) — see TODO-SALES006-001
	- `ContractsController` (3.4)
	- `CommissionsController` (3.5) — see TODO-SALES007-001
	- `SalesQuotasController` (3.6)
	- `PriceBooksController` (3.7)
	- `ProductBundlesController` (3.8)
	- `ESignaturesController` (3.9)
	- `EmailSequencesController` (3.10)
	- `ReportsController` (3.11)
	- `AIModelsController` (3.12)
	- `WebAnalyticsController` (3.13)
	- `SLAPoliciesController` (3.14)

P2 — Medium (Frontend types & services, auxiliary API surface)
- Frontend service/type work required to expose backend capabilities to the UI:
	- `orderService.ts` (4.1), `subscriptionService.ts` (4.2), `commissionService.ts` (4.3), `pricingService.ts` (4.4)
	- `eSignatureService.ts` (4.5), `emailSequenceService.ts` (4.6), `reportService.ts` (4.7), `aiAnalyticsService.ts` (4.8)
	- `knowledgeService.ts` (4.9), `slaService.ts` (4.10), `calendarService.ts` (4.11), `importExportService.ts` (4.12)
	- These are required to unblock UI/product teams and to make the newly-created or planned controllers consumable.

P3 — Low (Testing, documentation, polishing)
- Integration tests for new controllers, updated `DATABASE_SCHEMA.md`, updated feature checklists and a generated EF migration to snapshot the current model are in Phase 6 (6.1–6.4). These are scheduled but lower priority than P1/P2.

Summary counts (approx): P0=0 open, P1=~14 controllers, P2=~12 frontend services/types, P3=~4 documentation/test items.

## Short Actionable TODOs (top-priority, task IDs)
- (P1) Implement core Quote-to-Cash API: `OrdersController` (task 3.1), `InvoicesController` (3.2). Owner: Backend team — deliver skeleton + CRUD + tests.
- (P1 → P2) Implement subscriptions flow: `SubscriptionsController` (3.3) and frontend `subscriptionService.ts` (4.2). Link to TODO-SALES006-001.
- (P1) Implement commissions: `CommissionsController` (3.5). Link to TODO-SALES007-001 — include plan assignment and calculation endpoints first.
- (P2) Create `orderService.ts` (4.1) and `invoice`/`payment` types to unblock billing UI.
- (P2) Create `eSignatureService.ts` (4.5) to consume E-Signature endpoints once `ESignaturesController` exists.
- (P1) Create `ReportsController` (3.11) to expose report definitions and scheduling (needed by analytics/front-end embed work).
- (P2) Create `aiAnalyticsService.ts` (4.8) to consume `AIModelsController` (3.12) for model listing and predictions.
- (P3) Add integration tests for each newly implemented controller (6.1) and update `DATABASE_SCHEMA.md` (6.2).

Reference TODO IDs from the master todo list: `TODO-SALES006-001` (SubscriptionsController), `TODO-SALES007-001` (CommissionsController), `TODO-CRM001-008` (Territory service full implementation), and `TODO-CRM003-006` (Enforce valid opportunity stage transitions) — include these in sprint planning for Q2.

Next steps: create one PR per high-priority controller (grouped by domain: Orders/Invoices/Payments; Subscriptions/Contracts; Commissions/Quotas). Each PR must include: interface, controller skeleton, DTOs, unit tests, and a minimal frontend service stub.


## 1. Executive Summary

| Layer | Count | Notes |
|-------|-------|-------|
| **EF DbSet Entities** | 221 | Registered in `CrmDbContext.cs` |
| **SQL Database Tables** | 222 | On MariaDB server (1 extra auto-junction: `MarketingCampaignProduct`) |
| **Frontend TS Types** | 200+ | Across `services/` and `types/` |
| **Backend Controllers** | 69 | In `CRM.Api/Controllers/` |
| **Frontend Services** | 20+ | In `CRM.Frontend/src/services/` |

### Gap Summary

| Gap Category | Original | Current | Severity |
|-------------|----------|---------|----------|
| EF entities without SQL tables | ~60 | 0 | ~~Red~~ RESOLVED |
| Naming inconsistencies (Account/Customer) | 5 locations + 34 frontend files | 0 | ~~Red~~ RESOLVED |
| Duplicate entity definitions | 2 entities | 0 (separate by design) | ~~Red~~ RESOLVED |
| Duplicate DbSet declaration | 1 | 0 | ~~Red~~ RESOLVED |
| Workflow schema divergence | 8 entities | 0 | ~~Yellow~~ VERIFIED |
| EF entities without controllers | 50+ | 50+ | Yellow -- Medium |
| Controllers without frontend services | ~15 | ~15 | Green -- Low (mostly infra) |
| Backend entities without frontend types | ~40 | ~40 | Yellow -- Medium |

### Verification (BVT)

- **52/52 tests passing** (100%) after all remediation
- **All 14 test groups:** Auth, Accounts, Contacts, Leads, Opportunities, Service Requests, Products, Campaigns, Users, User Groups, Dashboard, Notes, Settings, Health

---

## 2. Naming Inconsistencies

### 2.1 Account / Customer Split -- RESOLVED

The solution underwent a `Customer -> Account` migration. The remaining ambiguity has been resolved:

| Location | Name Used | Status |
|----------|-----------|--------|
| **EF Entity class** | `Account` | `CRM.Core/Entities/Account.cs` |
| **DbSet property (primary)** | `Customers` | `DbSet<Account> Customers` -- maps to `Customers` table |
| **DbSet property (alias)** | `Accounts` | `DbSet<Account> Accounts => Customers;` (read-only alias, commit `3c468c6`) |
| **SQL table (baseline)** | `Customers` | Table name retained for backward compatibility |
| **API endpoint** | `/api/accounts` | Controller uses `Accounts` naming |
| **Frontend type** | `Account` (with `Customer` alias) | Renamed in 34 files (commit `f31c638`). `Customer` kept as backward-compat type alias in `apiService.ts`. |
| **Frontend API path** | `/accounts` (primary) | `accountService.ts` uses `/accounts`. Legacy `/customers` alias retained in `apiService.ts` for backward compat. |

**Resolution:** The duplicate `Accounts` DbSet was converted to a read-only alias (`=> Customers`) in commit `3c468c6`. The `Customers` table name is retained in the database for backward compatibility while the API layer uses `Accounts`. Frontend rename completed in commit `f31c638` (34 files: types, services, pages, components). `Customer` type alias and `customerService` alias kept in `apiService.ts` for backward compatibility. EntitySelect.tsx `customerForm`→`accountForm` state variable fixed for full consistency.

### 2.2 Territory Naming -- RESOLVED

| Item | Before | After | Commit |
|------|--------|-------|--------|
| Primary DbSet | `CustomerTerritoryAssignments` | `AccountTerritoryAssignments` | `3c468c6` |
| Alias | `AccountTerritoryAssignments => CustomerTerritoryAssignments` | `CustomerTerritoryAssignments => AccountTerritoryAssignments` | `3c468c6` |
| `[Table]` attribute | None | `[Table("CustomerTerritoryAssignments")]` | Maps correctly |

**Resolution:** `AccountTerritoryAssignments` is now the primary DbSet property; `CustomerTerritoryAssignments` is the backward-compatible alias.

---

## 3. EF Entities Missing Database Tables -- RESOLVED

All ~60 EF entities that previously had no corresponding database table now have tables. Resolution was via two mechanisms:

1. **EF `EnsureCreated()`** -- Most tables were auto-created by EF Core on startup
2. **Migration 019** (`019_create_missing_entity_tables.sql`, commit `08bf463`) -- 27 tables explicitly created

### 3.1 Sales & Quote-to-Cash (12 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `Order` | `Orders` | Orders | EnsureCreated |
| `OrderLineItem` | `OrderLineItems` | OrderLineItems | Migration 019 |
| `Invoice` | `Invoices` | Invoices | EnsureCreated |
| `InvoiceLineItem` | `InvoiceLineItems` | InvoiceLineItems | Migration 019 |
| `Payment` | `Payments` | Payments | EnsureCreated |
| `Subscription` | `Subscriptions` | Subscriptions | EnsureCreated + Table fix (commit `43d5433`) |
| `SubscriptionItem` | `SubscriptionItems` | SubscriptionItems | Migration 019 |
| `SubscriptionUsage` | `SubscriptionUsages` | SubscriptionUsages | Migration 019 |
| `Contract` | `Contracts` | Contracts | EnsureCreated |
| `CreditMemo` | `CreditMemos` | CreditMemos | EnsureCreated |
| `CreditMemoLineItem` | `CreditMemoLineItems` | CreditMemoLineItems | Migration 019 |
| `CreditApplication` | `CreditApplications` | CreditApplications | Migration 019 |

### 3.2 Sales Performance (9 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `CommissionPlan` | `CommissionPlans` | CommissionPlans | EnsureCreated |
| `CommissionTier` | `CommissionTiers` | CommissionTiers | Migration 019 |
| `CommissionPlanAssignment` | `CommissionPlanAssignments` | CommissionPlanAssignments | Migration 019 |
| `Commission` | `Commissions` | Commissions | EnsureCreated |
| `CommissionStatement` | `CommissionStatements` | CommissionStatements | Migration 019 |
| `SalesQuota` | `SalesQuotas` | SalesQuotas | Migration 019 |
| `SalesForecast` | `SalesForecasts` | SalesForecasts | EnsureCreated |
| `ForecastLineItem` | `ForecastLineItems` | ForecastLineItems | Migration 019 |
| `ForecastHistory` | `ForecastHistories` | ForecastHistories | Migration 019 |

### 3.3 Product & Pricing (7 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `ProductBundle` | `ProductBundles` | ProductBundles | EnsureCreated |
| `ProductBundleItem` | `ProductBundleItems` | ProductBundleItems | EnsureCreated |
| `ProductBundleRule` | `ProductBundleRules` | ProductBundleRules | EnsureCreated |
| `PriceBook` | `PriceBooks` | PriceBooks | EnsureCreated |
| `PriceBookEntry` | `PriceBookEntries` | PriceBookEntries | EnsureCreated |
| `PricingRule` | `PricingRules` | PricingRules | EnsureCreated |
| `PricingRuleUsage` | `PricingRuleUsages` | PricingRuleUsages | EnsureCreated |

### 3.4 Workflow Engine (8 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `WorkflowDefinition` | `WorkflowDefinitions` | WorkflowDefinitions | EnsureCreated |
| `WorkflowVersion` | `WorkflowVersions` | WorkflowVersions | EnsureCreated |
| `WorkflowNode` | `WorkflowNodes` | WorkflowNodes | EnsureCreated |
| `WorkflowTransition` | `WorkflowTransitions` | WorkflowTransitions | EnsureCreated |
| `WorkflowNodeInstance` | `WorkflowNodeInstances` | WorkflowNodeInstances | EnsureCreated |
| `WorkflowTask` | `WorkflowTasks` | WorkflowTasks | EnsureCreated |
| `WorkflowLog` | `WorkflowLogs` | WorkflowLogs | EnsureCreated |
| `WorkflowInstance` | `WorkflowInstances` | WorkflowInstances | EnsureCreated |

### 3.5 AI & Analytics (7 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `AIModel` | `AIModels` | AIModels | EnsureCreated |
| `Prediction` | `Predictions` | Predictions | EnsureCreated |
| `LeadScore` | `LeadScores` | LeadScores | EnsureCreated |
| `OpportunityInsight` | `OpportunityInsights` | OpportunityInsights | EnsureCreated |
| `ChurnRisk` | `ChurnRisks` | ChurnRisks | EnsureCreated |
| `ActionRecommendation` | `ActionRecommendations` | ActionRecommendations | EnsureCreated |
| `EmailIntelligence` | `EmailIntelligences` | EmailIntelligences | EnsureCreated |

### 3.6 Reporting (5 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `ReportDefinition` | `ReportDefinitions` | ReportDefinitions | EnsureCreated |
| `ReportFolder` | `ReportFolders` | ReportFolders | EnsureCreated |
| `ReportSchedule` | `ReportSchedules` | ReportSchedules | EnsureCreated |
| `ReportExecution` | `ReportExecutions` | ReportExecutions | EnsureCreated |
| `ReportWidgetConfig` | `ReportWidgetConfigs` | ReportWidgetConfigs | EnsureCreated |

### 3.7 Marketing Automation (14 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `CampaignRecipient` | `CampaignRecipients` | CampaignRecipients | EnsureCreated |
| `CampaignLinkClick` | `CampaignLinkClicks` | CampaignLinkClicks | EnsureCreated |
| `CampaignABTest` | `CampaignABTests` | CampaignABTests | EnsureCreated |
| `CampaignConversion` | `CampaignConversions` | CampaignConversions | EnsureCreated |
| `CampaignWorkflow` | `CampaignWorkflows` | CampaignWorkflows | EnsureCreated |
| `CampaignTouchpoint` | `CampaignTouchpoints` | CampaignTouchpoints | EnsureCreated |
| `AttributionSetting` | `AttributionSettings` | AttributionSettings | EnsureCreated |
| `CampaignAttributionSummary` | `CampaignAttributionSummaries` | CampaignAttributionSummaries | EnsureCreated |
| `EmailSequence` | `EmailSequences` | EmailSequences | EnsureCreated |
| `EmailSequenceStep` | `EmailSequenceSteps` | EmailSequenceSteps | EnsureCreated |
| `EmailSequenceEnrollment` | `EmailSequenceEnrollments` | EmailSequenceEnrollments | EnsureCreated |
| `EmailSequenceStepExecution` | `EmailSequenceStepExecutions` | EmailSequenceStepExecutions | EnsureCreated |
| `LandingPageBlock` | `LandingPageBlocks` | LandingPageBlocks | EnsureCreated |
| `LandingPageVisit` | `LandingPageVisits` | LandingPageVisits | EnsureCreated |

### 3.8 Web Analytics (3 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `WebVisitor` | `WebVisitors` | WebVisitors | EnsureCreated |
| `WebSession` | `WebSessions` | WebSessions | EnsureCreated |
| `WebPageView` | `WebPageViews` | WebPageViews | EnsureCreated |

### 3.9 Lead Management (7 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `LeadRoutingRule` | `LeadRoutingRules` | LeadRoutingRules | EnsureCreated |
| `LeadRoutingCriteria` | `LeadRoutingCriterias` | LeadRoutingCriterias | EnsureCreated |
| `LeadRoutingTarget` | `LeadRoutingTargets` | LeadRoutingTargets | EnsureCreated |
| `LeadRoutingLog` | `LeadRoutingLogs` | LeadRoutingLogs | EnsureCreated |
| `LeadScoreRule` | `LeadScoreRules` | LeadScoreRules | EnsureCreated |
| `DuplicateMatchField` | `DuplicateMatchFields` | DuplicateMatchFields | EnsureCreated |
| `DuplicateCandidate` | `DuplicateCandidates` | DuplicateCandidates | EnsureCreated |

### 3.10 Approval Workflow (5 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `DiscountApprovalMatrix` | `DiscountApprovalMatrices` | DiscountApprovalMatrices | EnsureCreated |
| `ApprovalLevel` | `ApprovalLevels` | ApprovalLevels | EnsureCreated |
| `ApprovalGroup` | `ApprovalGroups` | ApprovalGroups | EnsureCreated |
| `ApprovalGroupMember` | `ApprovalGroupMembers` | ApprovalGroupMembers | EnsureCreated |
| `ApprovalStep` | `ApprovalSteps` | ApprovalSteps | EnsureCreated |

### 3.11 E-Signature (4 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `ESignatureRequest` | `ESignatureRequests` | ESignatureRequests | EnsureCreated |
| `ESignatureSigner` | `ESignatureSigners` | ESignatureSigners | EnsureCreated |
| `ESignatureDocument` | `ESignatureDocuments` | ESignatureDocuments | EnsureCreated |
| `ESignatureAuditEvent` | `ESignatureAuditEvents` | ESignatureAuditEvents | EnsureCreated |

### 3.12 Relationships (3 entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `RelationshipInteraction` | `RelationshipInteractions` | RelationshipInteractions | EnsureCreated |
| `RelationshipMap` | `RelationshipMaps` | RelationshipMaps | EnsureCreated |
| `AccountHealthSnapshot` | `AccountHealthSnapshots` | AccountHealthSnapshots | EnsureCreated |

### 3.13 ITSM Catalog & Calendar (10 entities, Migration 019)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `ITSMServiceCatalogItem` | `ITSMServiceCatalogItems` | ITSMServiceCatalogItems | Migration 019 |
| `ITSMServiceCatalogCategory` | `ITSMServiceCatalogCategories` | ITSMServiceCatalogCategories | Migration 019 |
| `ITSMApprovalWorkflow` | `ITSMApprovalWorkflows` | ITSMApprovalWorkflows | Migration 019 |
| `ITSMApprovalStep` | `ITSMApprovalSteps` | ITSMApprovalSteps | Migration 019 |
| `ITSMBlackoutPeriod` | `ITSMBlackoutPeriods` | ITSMBlackoutPeriods | Migration 019 |
| `ITSMMaintenanceWindow` | `ITSMMaintenanceWindows` | ITSMMaintenanceWindows | Migration 019 |
| `ITSMRelease` | `ITSMReleases` | ITSMReleases | Migration 019 |
| `ITSMReleaseItem` | `ITSMReleaseItems` | ITSMReleaseItems | Migration 019 |
| `CalendarEvent` | `CalendarEvents` | CalendarEvents | Migration 019 |
| `CalendarReminder` | `CalendarReminders` | CalendarReminders | Migration 019 |

### 3.14 Email & Marketing (Migration 019)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `EmailSequenceStepAction` | `EmailSequenceStepActions` | EmailSequenceStepActions | Migration 019 |
| `CampaignSegment` | `CampaignSegments` | CampaignSegments | Migration 019 |
| `CampaignSegmentCriteria` | `CampaignSegmentCriterias` | CampaignSegmentCriterias | Migration 019 |
| `LandingPage` | `LandingPages` | LandingPages | Migration 019 |
| `LandingPageVersion` | `LandingPageVersions` | LandingPageVersions | Migration 019 |

### 3.15 Other (15+ entities)

| EF Entity | DbSet Name | SQL Table | Resolution |
|-----------|-----------|-----------|------------|
| `ContactDetail` | `ContactDetails` | ContactDetails | EnsureCreated |
| `ContactInfoLink` | `ContactInfoLinks` | ContactInfoLinks | EnsureCreated |
| `SocialMediaLink` | `SocialMediaLinks` | SocialMediaLinks | EnsureCreated |
| `OAuthToken` | `OAuthTokens` | OAuthTokens | EnsureCreated |
| `Conversation` | `Conversations` | Conversations | EnsureCreated |
| `FieldMasterDataLink` | `FieldMasterDataLinks` | FieldMasterDataLinks | EnsureCreated |
| `EmailTemplateHistoryEntry` | `EmailTemplateHistoryEntries` | EmailTemplateHistoryEntries | EnsureCreated |
| `EmailTemplateUsage` | `EmailTemplateUsages` | EmailTemplateUsages | EnsureCreated |
| `Dashboard` | `Dashboards` | Dashboards | EnsureCreated |
| `DashboardWidget` | `DashboardWidgets` | DashboardWidgets | EnsureCreated |
| `CloudProvider` | `CloudProviders` | CloudProviders | EnsureCreated |
| `DeploymentAttempt` | `DeploymentAttempts` | DeploymentAttempts | EnsureCreated |
| `HealthCheckLog` | `HealthCheckLogs` | HealthCheckLogs | EnsureCreated |
| `FormField` | `FormFields` | FormFields | EnsureCreated |
| `FormSubmission` | `FormSubmissions` | FormSubmissions | EnsureCreated |

> **Note:** All 221 EF DbSet entities now have corresponding database tables. 194 were created by `EnsureCreated()` on server startup; 27 were explicitly created via migration 019. The database has 222 tables total (1 extra: `MarketingCampaignProduct` auto-junction table). See [ENTITY_DB_ALIGNMENT_REPORT.md](ENTITY_DB_ALIGNMENT_REPORT.md) for the full cross-reference.

---

## 4. Database Tables Missing EF Entities

The following SQL tables exist in schema files but have no corresponding `DbSet<>` in `CrmDbContext`. Most are legacy artifacts, ITSM child tables managed through parent entities, or infrastructure tables.

| SQL Table | Schema File | Status | Notes |
|-----------|-------------|--------|-------|
| `EmailLogs` | `006_activities_communication.sql` | Legacy | Replaced by `CommunicationMessage` entity |
| `Attachments` | `006_activities_communication.sql` | Legacy | Handled inline via entity properties |
| `AuditLogs` | `006_activities_communication.sql` | Legacy | Observability gap -- no EF entity |
| `WorkflowSteps` | `005_workflow_tables.sql` | Legacy | Replaced by `WorkflowNode` (redesign) |
| `WorkflowStepExecutions` | `005_workflow_tables.sql` | Legacy | Replaced by `WorkflowNodeInstance` (redesign) |
| `WorkflowTriggers` | `005_workflow_tables.sql` | Legacy | Removed in redesign |
| `Workflows` | `005_workflow_tables.sql` | Legacy | Replaced by `WorkflowDefinition` (redesign) |
| `CatalogRequestComments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `IncidentComments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `IncidentAttachments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `IncidentHistory` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ProblemAttachments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ProblemComments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ChangeComments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ChangeAttachments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ChangeTasks` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ArticleAttachments` | `010_itsm_module.sql` | ITSM child | Managed through parent entity |
| `ITSMNumberSequences` | `010_itsm_module.sql` | Infrastructure | Auto-number generation |
| `ArticleRelationships` | `010_itsm_module.sql` | Has EF entity | `ArticleRelationship` exists |
| `ArticleIncidents` | `010_itsm_module.sql` | Has EF entity | `ArticleIncident` exists |
| `Services` | `010_itsm_module.sql` | Has EF entity | Maps to `Service` entity |
| `ServiceCIs` | `010_itsm_module.sql` | Has EF entity | Maps to `ServiceCI` entity |
| `DuplicateMergeGroups` | `20250713_add_duplicate_merge_tracking.sql` | Has EF entity | `DuplicateMergeGroup` exists |
| `DuplicateMergeGroupMembers` | same migration | Has EF entity | `DuplicateMergeGroupMember` exists |
| `Addresses_New` | `007_consolidated_contact_info.sql` | Artifact | Migration artifact -- now just `Addresses` |
| `MarketingCampaignProduct` | Auto-created | Auto-junction | EF many-to-many auto-junction table |
 
---

## Status Update — Implemented Fixes & TODO Markers (concise)

**Implemented Fixes (applied Feb 9–12, 2026):**
- **Entity / DB fixes:** Duplicate DbSet removal, territory renames, and explicit table creations via Migration 019 (see commits referenced in remediation summary).
- **ITSM & SLA fixes:** Business hours, SLA calculations, ticket numbering and related ITSM entity alignment applied and covered by unit tests in the ITSM test suite.
- **Provider & Integration work:** Provider factory and provider implementations (Search, Notifications, Chat, Signatures, AI) registered and tested; Novu/Chatwoot integration wiring added.
- **Frontend alignment:** `Customer → Account` renames applied across frontend types/services; legacy aliases preserved for backward compatibility to avoid breaking existing integrations.

**TODO Status Markers (actionable):**
- **[TODO/P1]** Implement `OrdersController`, `InvoicesController`, `SubscriptionsController`, `CommissionsController` — include DTOs, skeleton endpoints, and unit tests. (See `TODO-SALES006-001`, `TODO-SALES007-001` in master TODO list.)
- **[TODO/P2]** Add frontend service stubs for `orderService.ts`, `subscriptionService.ts`, `commissionService.ts`, and `eSignatureService.ts` to consume the new controller surfaces.
- **[TODO/P3]** Add integration and E2E tests for subscription and quote-to-cash flows, and create controller-level integration tests for all newly added controllers.

Notes: This section is a concise pointer to the remediation logs and the master TODO list. Teams should consult `docs/MASTER_TODO_LIST.md` and the remediation plan for ownership and scheduling.


## 5. Backend Entities Missing Frontend Types

The following significant EF entities have **no corresponding TypeScript interface** in the frontend:

### 5.1 Sales & Financial Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `Order` | None | No order management UI |
| `OrderLineItem` | None | No order line items |
| `Invoice` | None | No invoice management UI |
| `InvoiceLineItem` | None | No invoice lines |
| `Payment` | None | No payment UI |
| `Subscription` | None | No subscription management |
| `Contract` | None | No contract management UI |
| `CreditMemo` | None | No credit memo UI |
| `Commission` | None | No commission tracking UI |
| `SalesQuota` | None | No quota management UI |
| `SalesForecast` | None | No forecast UI |

### 5.2 Product & Pricing Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `ProductBundle` | None | No bundle configuration UI |
| `PriceBook` | None | No price book management |
| `PricingRule` | None | No pricing rules UI |
| `DiscountApprovalMatrix` | None | No discount matrix UI |

### 5.3 Marketing Automation Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `EmailSequence` | None | No drip campaign UI |
| `EmailSequenceStep` | None | No sequence builder |
| `WebVisitor` | None | No visitor tracking UI |
| `WebSession` | None | No session analytics |
| `LandingPageBlock` | None | Landing page editor incomplete |

### 5.4 AI & Analytics Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `AIModel` | None | No AI model management UI |
| `Prediction` | None | No prediction display |
| `OpportunityInsight` | None | No opportunity AI insights |
| `ChurnRisk` | None | No churn risk dashboard |
| `ActionRecommendation` | None | No recommendation engine UI |
| `ReportDefinition` | None | No report builder UI |
| `ReportSchedule` | None | No report scheduling UI |

### 5.5 E-Signature Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `ESignatureRequest` | None | No e-signature UI |
| `ESignatureSigner` | None | No signer management |
| `ESignatureDocument` | None | No document preview |

### 5.6 Knowledge Base Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `KnowledgeArticle` | None (partial in ITSM pages) | No dedicated KB types |
| `KnowledgeCategory` | None | No category management |
| `SLAPolicy` | None | No SLA management UI |

---

## 6. Frontend Types Missing Backend Entities

These frontend-only types exist in services but have no direct backend entity counterpart. They are typically DTOs, aggregations, or UI-specific types. No remediation needed for most.

| Frontend Type | Service File | Notes |
|--------------|-------------|-------|
| `ServiceRequestStatistics` | `apiService.ts` | Aggregation -- computed server-side, no entity needed |
| `ApprovalStatistics` | `approvalService.ts` | Aggregation -- computed server-side |
| `ApproverPerformance` | `approvalService.ts` | Aggregation -- computed server-side |
| `TerritoryStatistics` | `territoryService.ts` | Aggregation -- computed server-side |
| `DuplicateCheckResult` | `duplicateService.ts` | DTO -- no entity needed |
| `MergeResult` / `UnmergeResult` | `duplicateService.ts` | DTO -- no entity needed |

> No remediation needed -- these are correctly implemented as DTOs/computed responses.

---

## 7. Controllers Without Frontend Services

The following backend controllers have **no dedicated frontend service file**. Most are infrastructure or internal endpoints.

### 7.1 Expected -- Internal/Infrastructure (No Frontend Needed)

| Controller | Purpose | Frontend Needed? |
|-----------|---------|-----------------|
| `HealthController` | Health checks | No |
| `ProviderHealthController` | Provider monitoring | No |
| `MonitoringController` | System monitoring | No |
| `MonitoringIntegrationController` | External monitoring | No |
| `CICDIntegrationController` | CI/CD webhooks | No |
| `DocuSealWebhookController` | E-signature webhooks | No |
| `ITSMWebhooksController` | ITSM webhooks | No |
| `WebhooksController` | Generic webhooks | No |
| `SampleDataController` | Dev/testing | No |
| `FeaturesController` | Feature flags | No (admin settings covers) |
| `FileUploadController` | File uploads | No (used inline) |

### 7.2 Gaps -- Should Have Frontend Service

| Controller | Purpose | Frontend Gap |
|-----------|---------|-------------|
| `ImportExportController` | Data import/export | No `importExportService.ts` |
| `CalendarIntegrationController` | Calendar sync | No `calendarService.ts` |
| `EmailIntegrationController` | Email sync | No `emailIntegrationService.ts` |
| `KnowledgeAndCatalogControllers` | KB & Catalog | No `knowledgeService.ts` |
| `EmailToTicketController` | Email-to-ticket | No `emailToTicketService.ts` |

---

## 8. EF Entities Without API Controllers

The following groups of EF entities have **no dedicated API controller** exposing CRUD endpoints:

| Entity Group | Entities | Gap Severity |
|-------------|----------|-------------|
| **Quote-to-Cash** | Order, OrderLineItem, Invoice, InvoiceLineItem, Payment, Subscription, Contract, CreditMemo | High |
| **Sales Performance** | Commission, CommissionPlan, SalesQuota, SalesForecast, ForecastLineItem | Medium |
| **Product Pricing** | ProductBundle, PriceBook, PriceBookEntry, PricingRule, DiscountApprovalMatrix | Medium |
| **AI/Analytics** | AIModel, Prediction, ChurnRisk, ActionRecommendation, EmailIntelligence | Medium |
| **Reporting** | ReportDefinition, ReportFolder, ReportSchedule, ReportExecution | Medium |
| **E-Signature** | ESignatureRequest, ESignatureSigner, ESignatureDocument | Medium |
| **Email Sequences** | EmailSequence, EmailSequenceStep, EmailSequenceEnrollment | Medium |
| **Web Analytics** | WebVisitor, WebSession, WebPageView | Low |
| **ITSM Child** | IncidentComment, ProblemTask, ChangeTask (nested under parent controller) | Low |

---

## 9. Duplicate / Conflicting Definitions -- ASSESSED

### 9.1 KnowledgeArticle -- Separate Entities by Design

**Two entity definitions exist mapping to separate database tables:**

| File | Namespace | DbSet | DB Table | Purpose |
|------|-----------|-------|----------|---------|
| `Entities/KnowledgeBase/KnowledgeArticle.cs` | Core.Entities | `KnowledgeArticles` | KnowledgeArticles | General KB articles |
| `Entities/ITSM/KnowledgeArticle.cs` | Core.Entities.ITSM | `ITSMKnowledgeArticles` | ITSMKnowledgeArticles | ITSM-specific KB |

**Assessment:** These are intentionally separate entities in different namespaces, mapping to different tables. The `ArticleType` enum ordinal difference (HowTo=0 vs HowTo=1) is acceptable since each enum belongs to its own namespace. No consolidation needed.

### 9.2 SLAPolicy -- Separate Entities by Design

| File | Namespace | DbSet | DB Table | Purpose |
|------|-----------|-------|----------|---------|
| `Entities/KnowledgeBase/SLAPolicy.cs` | Core.Entities | `SLAPolicies` | SLAPolicies | Service Desk SLA (complex model) |
| `Entities/ITSM/SLA.cs` | Core.Entities.ITSM | `ITSMSLAPolicies` | ITSMSLAPolicies | ITSM SLA (simpler model) |

**Assessment:** Two different SLA models serving different modules. Service Desk SLA uses complex target-based model with business hours; ITSM SLA uses simpler priority-based response/resolution times. Separate by design -- no consolidation needed.

### 9.3 Duplicate DbSet Declaration -- RESOLVED

**Before (commit `3c468c6`):**
```csharp
public DbSet<Account> Customers { get; set; }   // Line 44 -- primary
public DbSet<Account> Accounts { get; set; }     // Line 134 -- duplicate!
```

**After:**
```csharp
public DbSet<Account> Customers { get; set; }           // Primary DbSet
public DbSet<Account> Accounts => Customers;             // Read-only alias
```

**Resolution:** The duplicate `Accounts` DbSet was converted to a read-only property alias pointing to `Customers`.

---

## 10. Workflow Schema Divergence -- VERIFIED

The SQL schema files and EF model used different names for the workflow engine. This has been verified as a **completed redesign** -- the EF model is authoritative, and all EF workflow entities now have matching database tables.

| Concept | SQL Schema (legacy) | EF Entity (current) | DB Table Exists |
|---------|-------------------|---------------------|-----------------|
| Workflow definition | `Workflows` | `WorkflowDefinition` / `WorkflowDefinitions` | Yes (EnsureCreated) |
| Workflow step | `WorkflowSteps` | `WorkflowNode` / `WorkflowNodes` | Yes (EnsureCreated) |
| Step execution | `WorkflowStepExecutions` | `WorkflowNodeInstance` / `WorkflowNodeInstances` | Yes (EnsureCreated) |
| Triggers | `WorkflowTriggers` | (Removed -- built into WorkflowDefinition) | N/A |
| Version tracking | (None) | `WorkflowVersion` / `WorkflowVersions` | Yes (EnsureCreated) |
| Transitions | (None) | `WorkflowTransition` / `WorkflowTransitions` | Yes (EnsureCreated) |
| Tasks | (None) | `WorkflowTask` / `WorkflowTasks` | Yes (EnsureCreated) |
| Logs | (None) | `WorkflowLog` / `WorkflowLogs` | Yes (EnsureCreated) |
| Instance | `WorkflowInstances` | `WorkflowInstance` / `WorkflowInstances` | Yes (EnsureCreated) |

**Status:** The legacy SQL schema files (`005_workflow_tables.sql`) remain for reference only. The EF model is the source of truth and all tables exist via `EnsureCreated()`.

---

## 11. Consolidated Remediation Task List

### Priority Legend

- P0 Critical: Data integrity risk, naming conflicts, duplicate definitions
- P1 High: Missing schema, missing API endpoints for core features
- P2 Medium: Missing frontend types, missing services for secondary features
- P3 Low: Nice-to-have improvements, documentation alignment

---

### Phase 1: Naming & Duplicate Resolution (P0) -- COMPLETE

| # | Task | Status | Resolution |
|---|------|--------|------------|
| 1.1 | Remove duplicate `Accounts` DbSet | DONE | Converted to read-only alias (commit `3c468c6`) |
| 1.2 | Standardize Account/Customer naming | DONE | `Accounts` alias + `Customers` table retained (commit `3c468c6`) |
| 1.3 | Rename `CustomerTerritoryAssignments` | DONE | `AccountTerritoryAssignments` is now primary (commit `3c468c6`) |
| 1.4 | Rename `CustomerContacts` junction table | DEFERRED | Table name retained -- not a data integrity risk |
| 1.5 | Consolidate `KnowledgeArticle` entities | ASSESSED | Separate by design -- no consolidation needed |
| 1.6 | Consolidate `SLAPolicy` entities | ASSESSED | Separate by design -- no consolidation needed |
| 1.7 | Unify `ArticleType` enum ordinals | ASSESSED | Separate namespaces -- no conflict |

### Phase 2: SQL Schema Alignment (P1) -- COMPLETE

| # | Task | Status | Resolution |
|---|------|--------|------------|
| 2.1 | Add SQL schema for Quote-to-Cash entities | DONE | EnsureCreated + Migration 019 (commit `08bf463`) |
| 2.2 | Add SQL schema for Sales Performance entities | DONE | EnsureCreated + Migration 019 |
| 2.3 | Add SQL schema for Product/Pricing entities | DONE | EnsureCreated |
| 2.4 | Add SQL schema for Marketing Automation entities | DONE | EnsureCreated + Migration 019 |
| 2.5 | Add SQL schema for AI/Analytics entities | DONE | EnsureCreated |
| 2.6 | Add SQL schema for Reporting entities | DONE | EnsureCreated |
| 2.7 | Add SQL schema for E-Signature entities | DONE | EnsureCreated |
| 2.8 | Add SQL schema for Approval entities | DONE | EnsureCreated |
| 2.9 | Add SQL schema for Lead Routing entities | DONE | EnsureCreated |
| 2.10 | Add SQL schema for Relationship entities | DONE | EnsureCreated |
| 2.11 | Add SQL schema for Dashboard entities | DONE | EnsureCreated |
| 2.12 | Add SQL schema for miscellaneous entities | DONE | EnsureCreated |
| 2.13 | Update Workflow SQL schema | VERIFIED | EF model is authoritative; all tables exist |
| 2.14 | Remove orphaned SQL tables | DONE | Dropped `Accounts` phantom + `ArticleFeedback` (commit `c8ba986`) |

### Phase 3: API Controller Coverage (P1) -- PENDING

| # | Task | Layer | Details |
|---|------|-------|---------|
| 3.1 | Create `OrdersController` | Backend | CRUD for Orders and OrderLineItems |
| 3.2 | Create `InvoicesController` | Backend | CRUD for Invoices, InvoiceLineItems, Payments |
| 3.3 | Create `SubscriptionsController` | Backend | CRUD for Subscriptions, SubscriptionItems |
| 3.4 | Create `ContractsController` | Backend | CRUD for Contracts |
| 3.5 | Create `CommissionsController` | Backend | CRUD for CommissionPlans, Commissions, CommissionStatements |
| 3.6 | Create `SalesQuotasController` | Backend | CRUD for SalesQuotas, SalesForecasts |
| 3.7 | Create `PriceBooksController` | Backend | CRUD for PriceBooks, PriceBookEntries, PricingRules |
| 3.8 | Create `ProductBundlesController` | Backend | CRUD for ProductBundles, ProductBundleItems |
| 3.9 | Create `ESignaturesController` | Backend | CRUD for ESignatureRequests, Signers, Documents |
| 3.10 | Create `EmailSequencesController` | Backend | CRUD for EmailSequences, Steps, Enrollments |
| 3.11 | Create `ReportsController` | Backend | CRUD for ReportDefinitions, ReportSchedules |
| 3.12 | Create `AIModelsController` | Backend | CRUD for AIModels, Predictions |
| 3.13 | Create `WebAnalyticsController` | Backend | Read endpoints for WebVisitors, WebSessions |
| 3.14 | Create `SLAPoliciesController` | Backend | CRUD for SLAPolicies, SLATargets |

### Phase 4: Frontend Types & Services (P2) -- PENDING

| # | Task | Layer | Details |
|---|------|-------|---------|
| 4.1 | Create `orderService.ts` | Frontend | Types and API calls for Orders, Invoices, Payments |
| 4.2 | Create `subscriptionService.ts` | Frontend | Types and API calls for Subscriptions, Contracts |
| 4.3 | Create `commissionService.ts` | Frontend | Types and API calls for Commissions, Quotas, Forecasts |
| 4.4 | Create `pricingService.ts` | Frontend | Types and API calls for PriceBooks, PricingRules, ProductBundles |
| 4.5 | Create `eSignatureService.ts` | Frontend | Types and API calls for E-Signature management |
| 4.6 | Create `emailSequenceService.ts` | Frontend | Types and API calls for Email Sequences |
| 4.7 | Create `reportService.ts` | Frontend | Types and API calls for Report Builder |
| 4.8 | Create `aiAnalyticsService.ts` | Frontend | Types and API calls for AI Models, Predictions, Insights |
| 4.9 | Create `knowledgeService.ts` | Frontend | Types and API calls for Knowledge Base articles |
| 4.10 | Create `slaService.ts` | Frontend | Types and API calls for SLA Policies |
| 4.11 | Create `calendarService.ts` | Frontend | Types and API calls for Calendar Integration |
| 4.12 | Create `importExportService.ts` | Frontend | Types and API calls for Import/Export operations |
| 4.13 | Rename `Customer` to `Account` in types | DONE | Commit `f31c638` -- 34 files renamed. `Customer` kept as type alias. |
| 4.14 | Update API paths `/customers` to `/accounts` | DONE | `accountService.ts` uses `/accounts`. Legacy alias in `apiService.ts`. |

### Phase 5: ITSM Sub-Entity Alignment (P2) -- VERIFIED (No Work Needed)

| # | Task | Status | Resolution |
|---|------|--------|------------|
| 5.1 | Add EF entities for ITSM child tables | VERIFIED | ITSM child tables are managed through parent entities; 0 mismatches found in 35-table, 447-property audit |
| 5.2 | Add `ITSMNumberSequences` entity | ASSESSED | Infrastructure table -- not needed as EF entity |
| 5.3 | Add `CatalogRequestComment` entity | ASSESSED | Managed through parent `ITSMServiceCatalogItem` |
| 5.4 | Add `ArticleAttachment` entity | ASSESSED | Managed through parent `KnowledgeArticle` |

### Phase 6: Test & Documentation (P3) -- PENDING

| # | Task | Layer | Details |
|---|------|-------|---------|
| 6.1 | Add integration tests for new controllers | Testing | Test CRUD operations for Phase 3 controllers |
| 6.2 | Update `DATABASE_SCHEMA.md` | Docs | Reflect current EF-managed schema (222 tables) |
| 6.3 | Update `FEATURE_CHECKLIST.md` | Docs | Mark newly aligned features |
| 6.4 | Add EF migration for schema alignment | Backend | Generate migration that brings SQL in sync with EF model |

---

## Appendix A: Enum Alignment Status

| Enum | Backend | Frontend | Status |
|------|---------|----------|--------|
| `ServiceRequestStatus` | 11 values (New..Reopened) | 11 values | Aligned |
| `ServiceRequestPriority` | 5 values (Low..Urgent) | 5 values | Aligned |
| `ServiceRequestChannel` | 8 values | 8 values | Aligned |
| `OpportunityStage` | 6 values | 6 values | Aligned |
| `CustomFieldType` | 12 values | 12 values | Aligned |
| `ArticleType` (KnowledgeBase) | 10 values (0-indexed) | N/A | No frontend (separate entity by design) |
| `ArticleType` (ITSM) | 6 values (1-indexed) | N/A | No frontend (separate entity by design) |
| `ApprovalStatus` | 7 values | 7 values | Aligned |
| `WidgetType` | 16 values | 16 values | Aligned |

## Appendix B: Cross-Reference Matrix

| Module | EF Entities | SQL Tables | Controller | Frontend Service | Frontend Page |
|--------|------------|------------|-----------|-----------------|---------------|
| **Accounts** | Account | Customers | AccountsController | accountService + apiService (alias) | AccountsPage |
| **Contacts** | Contact | Contacts | ContactsController | contactInfoService | ContactsPage |
| **Leads** | Lead | Leads | LeadsController | apiService | LeadsPage |
| **Opportunities** | Opportunity | Opportunities | OpportunitiesController | apiService | OpportunitiesPage |
| **Products** | Product | Products | ProductsController | apiService | ProductsPage |
| **Quotes** | Quote | Quotes | QuotesController | apiService | QuotesPage |
| **Orders** | Order | Orders | Missing | Missing | Missing |
| **Invoices** | Invoice | Invoices | Missing | Missing | Missing |
| **Payments** | Payment | Payments | Missing | Missing | Missing |
| **Subscriptions** | Subscription | Subscriptions | Missing | Missing | Missing |
| **Contracts** | Contract | Contracts | Missing | Missing | ContractsPage |
| **Campaigns** | MarketingCampaign | MarketingCampaigns | CampaignsController | campaignExecutionService | CampaignsPage |
| **Service Requests** | ServiceRequest | ServiceRequests | ServiceRequestsController | apiService | ServiceRequestsPage |
| **ITSM Incidents** | Incident | Incidents | IncidentsController | (ITSM pages) | IncidentPages |
| **ITSM Problems** | Problem | Problems | (via ITSM) | (ITSM pages) | ProblemPages |
| **ITSM Changes** | Change | Changes | (via ITSM) | (ITSM pages) | ChangePages |
| **Knowledge Base** | KnowledgeArticle (x2) | KnowledgeArticles + ITSMKnowledgeArticles | KnowledgeController | Missing | KnowledgeBasePage |
| **SLA Policies** | SLAPolicy (x2) | SLAPolicies + ITSMSLAPolicies | Missing | Missing | Missing |
| **Workflows** | 8 entities | All present (EnsureCreated) | WorkflowController | workflowService | (embedded) |
| **Dashboards** | Dashboard | Dashboards | DashboardController | dashboardService | DashboardPage |
| **Territories** | AccountTerritory | AccountTerritories | TerritoriesController | territoryService | TerritoriesPage |
| **Approvals** | ApprovalRequest | ApprovalRequests | ApprovalsController | approvalService | ApprovalsPage |
| **Relationships** | AccountRelationship | AccountRelationships | RelationshipsController | relationshipService | RelationshipsPage |
| **Duplicates** | DuplicateRule | DuplicateRules | DuplicatesController | duplicateService | (embedded) |
| **Lead Routing** | LeadRoutingRule | LeadRoutingRules | LeadRoutingController | leadRoutingService | LeadRoutingPage |
| **Email Templates** | EmailTemplate | EmailTemplates | EmailTemplatesController | apiService | EmailTemplatesPage |
| **Forms** | FormDefinition | FormDefinitions | FormsController | formBuilderService | FormBuilderPage |
| **Landing Pages** | LandingPage | LandingPages | LandingPageController | (inline) | LandingPagesPage |
| **Commissions** | Commission | Commissions | Missing | Missing | Missing |
| **E-Signatures** | ESignatureRequest | ESignatureRequests | Missing | Missing | Missing |
| **Email Sequences** | EmailSequence | EmailSequences | Missing | Missing | Missing |
| **Reports** | ReportDefinition | ReportDefinitions | Missing | Missing | Missing |
| **AI/Analytics** | AIModel | AIModels | Missing | Missing | Missing |
| **Product Bundles** | ProductBundle | ProductBundles | Missing | Missing | Missing |
| **Price Books** | PriceBook | PriceBooks | Missing | Missing | Missing |
| **Web Analytics** | WebVisitor | WebVisitors | Missing | Missing | Missing |

---

*This document should be reviewed alongside [SOLUTION_GAPS_REMEDIATION_PLAN.md](docs/development/SOLUTION_GAPS_REMEDIATION_PLAN.md), [MASTER_TODO_LIST.md](MASTER_TODO_LIST.md), and [EF_DB_FRONTEND_REMEDIATION_SUMMARY.md](EF_DB_FRONTEND_REMEDIATION_SUMMARY.md) for full context on implementation status.*
