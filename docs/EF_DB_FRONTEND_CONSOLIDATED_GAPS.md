# EF ↔ Database ↔ Frontend: Consolidated Gap Analysis & Remediation Tasks

> **Created:** February 9, 2026
> **Methodology:** Systematic comparison of EF DbSet entities (152), SQL schema tables (~90), and Frontend TypeScript types (200+)
> **Scope:** Naming inconsistencies, missing schema, missing types, orphaned definitions, and cross-layer alignment

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

## 1. Executive Summary

| Layer | Count | Notes |
|-------|-------|-------|
| **EF DbSet Entities** | 152 | Registered in `CrmDbContext.cs` |
| **SQL Schema Tables** | ~90 | Across `database/schema/` and `database/migrations/` |
| **Frontend TS Types** | 200+ | Across `services/` and `types/` |
| **Backend Controllers** | 69 | In `CRM.Api/Controllers/` |
| **Frontend Services** | 20+ | In `CRM.Frontend/src/services/` |

### Gap Summary

| Gap Category | Count | Severity |
|-------------|-------|----------|
| EF entities without SQL tables | ~60 | 🔴 High |
| Naming inconsistencies (Account/Customer) | 5 locations | 🔴 High |
| Duplicate entity definitions | 2 entities | 🔴 High |
| Workflow schema divergence | 8 entities | 🟡 Medium |
| EF entities without controllers | 50+ | 🟡 Medium |
| Controllers without frontend services | ~15 | 🟢 Low (mostly infra) |
| Backend entities without frontend types | ~40 | 🟡 Medium |

---

## 2. Naming Inconsistencies

### 2.1 Account / Customer Split (🔴 Critical)

The solution underwent a `Customer → Account` migration that is incomplete across layers.

| Location | Name Used | Detail |
|----------|-----------|--------|
| **EF Entity class** | `Account` | `CRM.Core/Entities/Account.cs` |
| **DbSet property (1)** | `Customers` | `DbSet<Account> Customers` (line 44 of CrmDbContext) |
| **DbSet property (2)** | `Accounts` | `DbSet<Account> Accounts` (line 134) — **duplicate** |
| **SQL baseline (000)** | `Accounts` | `000_baseline_schema.sql` creates table `Accounts` |
| **SQL core (001)** | `Customers` | `001_core_tables.sql` creates table `Customers` |
| **Junction table (SQL)** | `CustomerContacts` | `001_core_tables.sql` — should be `AccountContacts` |
| **Territory DbSet** | `CustomerTerritoryAssignments` | Property named after old "Customer" convention |
| **Frontend type** | `Customer` | `apiService.ts` uses `Customer` interface |
| **Frontend API path** | `/customers` | Frontend service calls `/customers` endpoint |
| **Migration script** | Mixed | `100_customer_to_account_migration.sql` renames but not fully applied |

**Impact:** Two duplicate DbSet properties pointing to the same entity (`Customers` and `Accounts`); SQL schema files conflict between `Customers` and `Accounts` table names; frontend still uses `Customer` type name.

### 2.2 Territory Naming

| Item | Current Name | Expected Name |
|------|-------------|---------------|
| DbSet property | `CustomerTerritoryAssignments` | `AccountTerritoryAssignments` |
| Alias | `AccountTerritoryAssignments => CustomerTerritoryAssignments` | Direct property |

---

## 3. EF Entities Missing Database Tables

The following 60+ EF entities registered as `DbSet<>` in `CrmDbContext` have **no corresponding CREATE TABLE** statement in any SQL schema or migration file. They rely solely on EF migrations.

### 3.1 Sales & Quote-to-Cash (12 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `Order` | `Orders` | ❌ None | Missing |
| `OrderLineItem` | `OrderLineItems` | ❌ None | Missing |
| `Invoice` | `Invoices` | ❌ None | Missing |
| `InvoiceLineItem` | `InvoiceLineItems` | ❌ None | Missing |
| `Payment` | `Payments` | ❌ None | Missing |
| `Subscription` | `Subscriptions` | ❌ None | Missing |
| `SubscriptionItem` | `SubscriptionItems` | ❌ None | Missing |
| `SubscriptionUsage` | `SubscriptionUsages` | ❌ None | Missing |
| `Contract` | `Contracts` | ❌ None | Missing |
| `CreditMemo` | `CreditMemos` | ❌ None | Missing |
| `CreditMemoLineItem` | `CreditMemoLineItems` | ❌ None | Missing |
| `CreditApplication` | `CreditApplications` | ❌ None | Missing |

### 3.2 Sales Performance (9 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `CommissionPlan` | `CommissionPlans` | ❌ None | Missing |
| `CommissionTier` | `CommissionTiers` | ❌ None | Missing |
| `CommissionPlanAssignment` | `CommissionPlanAssignments` | ❌ None | Missing |
| `Commission` | `Commissions` | ❌ None | Missing |
| `CommissionStatement` | `CommissionStatements` | ❌ None | Missing |
| `SalesQuota` | `SalesQuotas` | ❌ None | Missing |
| `SalesForecast` | `SalesForecasts` | ❌ None | Missing |
| `ForecastLineItem` | `ForecastLineItems` | ❌ None | Missing |
| `ForecastHistory` | `ForecastHistories` | ❌ None | Missing |

### 3.3 Product & Pricing (7 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `ProductBundle` | `ProductBundles` | ❌ None | Missing |
| `ProductBundleItem` | `ProductBundleItems` | ❌ None | Missing |
| `ProductBundleRule` | `ProductBundleRules` | ❌ None | Missing |
| `PriceBook` | `PriceBooks` | ❌ None | Missing |
| `PriceBookEntry` | `PriceBookEntries` | ❌ None | Missing |
| `PricingRule` | `PricingRules` | ❌ None | Missing |
| `PricingRuleUsage` | `PricingRuleUsages` | ❌ None | Missing |

### 3.4 Workflow Engine (8 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `WorkflowDefinition` | `WorkflowDefinitions` | ❌ None (SQL has `Workflows`) | Schema divergence |
| `WorkflowVersion` | `WorkflowVersions` | ❌ None | Missing |
| `WorkflowNode` | `WorkflowNodes` | ❌ None (SQL has `WorkflowSteps`) | Schema divergence |
| `WorkflowTransition` | `WorkflowTransitions` | ❌ None | Missing |
| `WorkflowNodeInstance` | `WorkflowNodeInstances` | ❌ None (SQL has `WorkflowStepExecutions`) | Schema divergence |
| `WorkflowTask` | `WorkflowTasks` | ❌ None | Missing |
| `WorkflowLog` | `WorkflowLogs` | ❌ None | Missing |
| `WorkflowInstance` | `WorkflowInstances` | ✅ Exists | OK (name matches) |

### 3.5 AI & Analytics (7 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `AIModel` | `AIModels` | ❌ None | Missing |
| `Prediction` | `Predictions` | ❌ None | Missing |
| `LeadScore` | `LeadScores` | ❌ None | Missing |
| `OpportunityInsight` | `OpportunityInsights` | ❌ None | Missing |
| `ChurnRisk` | `ChurnRisks` | ❌ None | Missing |
| `ActionRecommendation` | `ActionRecommendations` | ❌ None | Missing |
| `EmailIntelligence` | `EmailIntelligences` | ❌ None | Missing |

### 3.6 Reporting (5 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `ReportDefinition` | `ReportDefinitions` | ❌ None | Missing |
| `ReportFolder` | `ReportFolders` | ❌ None | Missing |
| `ReportSchedule` | `ReportSchedules` | ❌ None | Missing |
| `ReportExecution` | `ReportExecutions` | ❌ None | Missing |
| `ReportWidgetConfig` | `ReportWidgetConfigs` | ❌ None | Missing |

### 3.7 Marketing Automation (14 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `CampaignRecipient` | `CampaignRecipients` | ❌ None | Missing |
| `CampaignLinkClick` | `CampaignLinkClicks` | ❌ None | Missing |
| `CampaignABTest` | `CampaignABTests` | ❌ None | Missing |
| `CampaignConversion` | `CampaignConversions` | ❌ None | Missing |
| `CampaignWorkflow` | `CampaignWorkflows` | ❌ None | Missing |
| `CampaignTouchpoint` | `CampaignTouchpoints` | ❌ None | Missing |
| `AttributionSetting` | `AttributionSettings` | ❌ None | Missing |
| `CampaignAttributionSummary` | `CampaignAttributionSummaries` | ❌ None | Missing |
| `EmailSequence` | `EmailSequences` | ❌ None | Missing |
| `EmailSequenceStep` | `EmailSequenceSteps` | ❌ None | Missing |
| `EmailSequenceEnrollment` | `EmailSequenceEnrollments` | ❌ None | Missing |
| `EmailSequenceStepExecution` | `EmailSequenceStepExecutions` | ❌ None | Missing |
| `LandingPageBlock` | `LandingPageBlocks` | ❌ None | Missing |
| `LandingPageVisit` | `LandingPageVisits` | ❌ None | Missing |

### 3.8 Web Analytics (3 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `WebVisitor` | `WebVisitors` | ❌ None | Missing |
| `WebSession` | `WebSessions` | ❌ None | Missing |
| `WebPageView` | `WebPageViews` | ❌ None | Missing |

### 3.9 Lead Management (7 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `LeadRoutingRule` | `LeadRoutingRules` | ❌ None | Missing |
| `LeadRoutingCriteria` | `LeadRoutingCriterias` | ❌ None | Missing |
| `LeadRoutingTarget` | `LeadRoutingTargets` | ❌ None | Missing |
| `LeadRoutingLog` | `LeadRoutingLogs` | ❌ None | Missing |
| `LeadScoreRule` | `LeadScoreRules` | ❌ None | Missing |
| `DuplicateMatchField` | `DuplicateMatchFields` | ❌ None | Missing |
| `DuplicateCandidate` | `DuplicateCandidates` | ❌ None | Missing |

### 3.10 Approval Workflow (5 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `DiscountApprovalMatrix` | `DiscountApprovalMatrices` | ❌ None | Missing |
| `ApprovalLevel` | `ApprovalLevels` | ❌ None | Missing |
| `ApprovalGroup` | `ApprovalGroups` | ❌ None | Missing |
| `ApprovalGroupMember` | `ApprovalGroupMembers` | ❌ None | Missing |
| `ApprovalStep` | `ApprovalSteps` | ❌ None | Missing |

### 3.11 E-Signature (4 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `ESignatureRequest` | `ESignatureRequests` | ❌ None | Missing |
| `ESignatureSigner` | `ESignatureSigners` | ❌ None | Missing |
| `ESignatureDocument` | `ESignatureDocuments` | ❌ None | Missing |
| `ESignatureAuditEvent` | `ESignatureAuditEvents` | ❌ None | Missing |

### 3.12 Relationships (3 entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `RelationshipInteraction` | `RelationshipInteractions` | ❌ None | Missing |
| `RelationshipMap` | `RelationshipMaps` | ❌ None | Missing |
| `AccountHealthSnapshot` | `AccountHealthSnapshots` | ❌ None | Missing |

### 3.13 Other Missing (10+ entities)

| EF Entity | DbSet Name | SQL Table | Status |
|-----------|-----------|-----------|--------|
| `ContactDetail` | `ContactDetails` | ❌ None | Missing |
| `ContactInfoLink` | `ContactInfoLinks` | ❌ None | Missing |
| `SocialMediaLink` | `SocialMediaLinks` | ❌ None | Missing |
| `OAuthToken` | `OAuthTokens` | ❌ None | Missing |
| `Conversation` | `Conversations` | ❌ None | Missing |
| `FieldMasterDataLink` | `FieldMasterDataLinks` | ❌ None | Missing |
| `EmailTemplateHistoryEntry` | `EmailTemplateHistoryEntries` | ❌ None | Missing |
| `EmailTemplateUsage` | `EmailTemplateUsages` | ❌ None | Missing |
| `Dashboard` | `Dashboards` | ❌ None | Missing |
| `DashboardWidget` | `DashboardWidgets` | ❌ None | Missing |
| `CloudProvider` | `CloudProviders` | ❌ None | Missing |
| `DeploymentAttempt` | `DeploymentAttempts` | ❌ None | Missing |
| `HealthCheckLog` | `HealthCheckLogs` | ❌ None | Missing |
| `FormField` | `FormFields` | ❌ None | Missing |
| `FormSubmission` | `FormSubmissions` | ❌ None | Missing |

> **Note:** These entities are managed through EF Code-First migrations (`CRM.Infrastructure/Migrations/`). The SQL schema files under `database/schema/` represent an initial hand-written schema that has diverged from the EF-managed schema.

---

## 4. Database Tables Missing EF Entities

The following SQL tables exist in schema files but have no corresponding `DbSet<>` in `CrmDbContext`:

| SQL Table | Schema File | Notes |
|-----------|-------------|-------|
| `EmailLogs` | `006_activities_communication.sql` | No EF entity — replaced by `CommunicationMessage`? |
| `Attachments` | `006_activities_communication.sql` | No dedicated entity — handled inline? |
| `AuditLogs` | `006_activities_communication.sql` | No EF entity — possible observability gap |
| `WorkflowSteps` | `005_workflow_tables.sql` | Replaced by `WorkflowNode` (schema divergence) |
| `WorkflowStepExecutions` | `005_workflow_tables.sql` | Replaced by `WorkflowNodeInstance` (schema divergence) |
| `WorkflowTriggers` | `005_workflow_tables.sql` | No EF entity |
| `Workflows` | `005_workflow_tables.sql` | Replaced by `WorkflowDefinition` (schema divergence) |
| `CatalogRequestComments` | `010_itsm_module.sql` | Exists in ITSM SQL but not in DbSet |
| `IncidentComments` | `010_itsm_module.sql` | ITSM-specific, may be in ITSM sub-context |
| `IncidentAttachments` | `010_itsm_module.sql` | ITSM-specific |
| `IncidentHistory` | `010_itsm_module.sql` | ITSM-specific |
| `ProblemAttachments` | `010_itsm_module.sql` | ITSM-specific |
| `ProblemComments` | `010_itsm_module.sql` | ITSM-specific |
| `ChangeComments` | `010_itsm_module.sql` | ITSM-specific |
| `ChangeAttachments` | `010_itsm_module.sql` | ITSM-specific |
| `ChangeTasks` | `010_itsm_module.sql` | ITSM-specific |
| `ArticleRelationships` | `010_itsm_module.sql` | In ITSM SQL, has EF entity `ArticleRelationship` |
| `ArticleIncidents` | `010_itsm_module.sql` | In ITSM SQL, has EF entity `ArticleIncident` |
| `ArticleAttachments` | `010_itsm_module.sql` | ITSM SQL only |
| `ITSMNumberSequences` | `010_itsm_module.sql` | ITSM infrastructure, no EF entity |
| `Services` | `010_itsm_module.sql` | Maps to `Service` entity |
| `ServiceCIs` | `010_itsm_module.sql` | Maps to `ServiceCI` entity |
| `DuplicateMergeGroups` | `20250713_add_duplicate_merge_tracking.sql` | Has EF entity `DuplicateMergeGroup` ✅ |
| `DuplicateMergeGroupMembers` | `20250713_add_duplicate_merge_tracking.sql` | Has EF entity ✅ |
| `Addresses_New` | `007_consolidated_contact_info.sql` | Migration artifact — now just `Addresses` |

---

## 5. Backend Entities Missing Frontend Types

The following significant EF entities have **no corresponding TypeScript interface** in the frontend:

### 5.1 Sales & Financial Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `Order` | ❌ None | No order management UI |
| `OrderLineItem` | ❌ None | No order line items |
| `Invoice` | ❌ None | No invoice management UI |
| `InvoiceLineItem` | ❌ None | No invoice lines |
| `Payment` | ❌ None | No payment UI |
| `Subscription` | ❌ None | No subscription management |
| `Contract` | ❌ None | No contract management UI |
| `CreditMemo` | ❌ None | No credit memo UI |
| `Commission` | ❌ None | No commission tracking UI |
| `SalesQuota` | ❌ None | No quota management UI |
| `SalesForecast` | ❌ None | No forecast UI |

### 5.2 Product & Pricing Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `ProductBundle` | ❌ None | No bundle configuration UI |
| `PriceBook` | ❌ None | No price book management |
| `PricingRule` | ❌ None | No pricing rules UI |
| `DiscountApprovalMatrix` | ❌ None | No discount matrix UI |

### 5.3 Marketing Automation Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `EmailSequence` | ❌ None | No drip campaign UI |
| `EmailSequenceStep` | ❌ None | No sequence builder |
| `WebVisitor` | ❌ None | No visitor tracking UI |
| `WebSession` | ❌ None | No session analytics |
| `LandingPageBlock` | ❌ None | Landing page editor incomplete |

### 5.4 AI & Analytics Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `AIModel` | ❌ None | No AI model management UI |
| `Prediction` | ❌ None | No prediction display |
| `OpportunityInsight` | ❌ None | No opportunity AI insights |
| `ChurnRisk` | ❌ None | No churn risk dashboard |
| `ActionRecommendation` | ❌ None | No recommendation engine UI |
| `ReportDefinition` | ❌ None | No report builder UI |
| `ReportSchedule` | ❌ None | No report scheduling UI |

### 5.5 E-Signature Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `ESignatureRequest` | ❌ None | No e-signature UI |
| `ESignatureSigner` | ❌ None | No signer management |
| `ESignatureDocument` | ❌ None | No document preview |

### 5.6 Knowledge Base Entities

| Backend Entity | Frontend Type | Gap |
|---------------|---------------|-----|
| `KnowledgeArticle` | ❌ None (partial in ITSM pages) | No dedicated KB types |
| `KnowledgeCategory` | ❌ None | No category management |
| `SLAPolicy` | ❌ None | No SLA management UI |

---

## 6. Frontend Types Missing Backend Entities

These frontend-only types exist in services but have no direct backend entity counterpart. They are typically DTOs, aggregations, or UI-specific types. No remediation needed for most.

| Frontend Type | Service File | Notes |
|--------------|-------------|-------|
| `ServiceRequestStatistics` | `apiService.ts` | Aggregation — computed server-side, no entity needed |
| `ApprovalStatistics` | `approvalService.ts` | Aggregation — computed server-side |
| `ApproverPerformance` | `approvalService.ts` | Aggregation — computed server-side |
| `TerritoryStatistics` | `territoryService.ts` | Aggregation — computed server-side |
| `DuplicateCheckResult` | `duplicateService.ts` | DTO — no entity needed |
| `MergeResult` / `UnmergeResult` | `duplicateService.ts` | DTO — no entity needed |

> ✅ No remediation needed — these are correctly implemented as DTOs/computed responses.

---

## 7. Controllers Without Frontend Services

The following backend controllers have **no dedicated frontend service file**. Most are infrastructure or internal endpoints.

### 7.1 Expected — Internal/Infrastructure (No Frontend Needed)

| Controller | Purpose | Frontend Needed? |
|-----------|---------|-----------------|
| `HealthController` | Health checks | ❌ No |
| `ProviderHealthController` | Provider monitoring | ❌ No |
| `MonitoringController` | System monitoring | ❌ No |
| `MonitoringIntegrationController` | External monitoring | ❌ No |
| `CICDIntegrationController` | CI/CD webhooks | ❌ No |
| `DocuSealWebhookController` | E-signature webhooks | ❌ No |
| `ITSMWebhooksController` | ITSM webhooks | ❌ No |
| `WebhooksController` | Generic webhooks | ❌ No |
| `SampleDataController` | Dev/testing | ❌ No |
| `FeaturesController` | Feature flags | ❌ No (admin settings covers) |
| `FileUploadController` | File uploads | ❌ No (used inline) |

### 7.2 Gaps — Should Have Frontend Service

| Controller | Purpose | Frontend Gap |
|-----------|---------|-------------|
| `ImportExportController` | Data import/export | 🟡 No `importExportService.ts` |
| `CalendarIntegrationController` | Calendar sync | 🟡 No `calendarService.ts` |
| `EmailIntegrationController` | Email sync | 🟡 No `emailIntegrationService.ts` |
| `KnowledgeAndCatalogControllers` | KB & Catalog | 🟡 No `knowledgeService.ts` |
| `EmailToTicketController` | Email-to-ticket | 🟡 No `emailToTicketService.ts` |

---

## 8. EF Entities Without API Controllers

The following groups of EF entities have **no dedicated API controller** exposing CRUD endpoints:

| Entity Group | Entities | Gap Severity |
|-------------|----------|-------------|
| **Quote-to-Cash** | Order, OrderLineItem, Invoice, InvoiceLineItem, Payment, Subscription, Contract, CreditMemo | 🔴 High |
| **Sales Performance** | Commission, CommissionPlan, SalesQuota, SalesForecast, ForecastLineItem | 🟡 Medium |
| **Product Pricing** | ProductBundle, PriceBook, PriceBookEntry, PricingRule, DiscountApprovalMatrix | 🟡 Medium |
| **AI/Analytics** | AIModel, Prediction, ChurnRisk, ActionRecommendation, EmailIntelligence | 🟡 Medium |
| **Reporting** | ReportDefinition, ReportFolder, ReportSchedule, ReportExecution | 🟡 Medium |
| **E-Signature** | ESignatureRequest, ESignatureSigner, ESignatureDocument | 🟡 Medium |
| **Email Sequences** | EmailSequence, EmailSequenceStep, EmailSequenceEnrollment | 🟡 Medium |
| **Web Analytics** | WebVisitor, WebSession, WebPageView | 🟢 Low |
| **ITSM Child** | IncidentComment, ProblemTask, ChangeTask (typically nested under parent controller) | 🟢 Low |

---

## 9. Duplicate / Conflicting Definitions

### 9.1 KnowledgeArticle (🔴 Critical Duplicate)

**Two completely different entity definitions exist:**

| File | Namespace | Inherits | Properties | Enums |
|------|-----------|----------|------------|-------|
| `Entities/KnowledgeBase/KnowledgeArticle.cs` | Core.Entities | `BaseEntity` | ~30 properties | `ArticleType`: HowTo(0), FAQ(1), Troubleshooting(2), BestPractice(3), Documentation(4), Process(5), Policy(6), ReleaseNotes(7), Video(8), Template(9) |
| `Entities/ITSM/KnowledgeArticle.cs` | Core.Entities.ITSM | None | ~20 properties | `ArticleType`: HowTo(1), Troubleshooting(2), FAQ(3), KnownError(4), Reference(5), BestPractice(6) |

**Issues:**
- Enum values have different ordinals (HowTo=0 vs HowTo=1)
- Different property sets
- Both registered in DbContext as separate DbSets (`KnowledgeArticles` and `ITSMKnowledgeArticles`)
- Potential confusion when querying

### 9.2 SLAPolicy (🔴 Critical Duplicate)

| File | Namespace | Inherits | Size |
|------|-----------|----------|------|
| `Entities/KnowledgeBase/SLAPolicy.cs` | Core.Entities | `BaseEntity` | ~426 lines (full SLA with targets, escalation, business hours) |
| `Entities/ITSM/SLA.cs` | Core.Entities.ITSM | None | ~193 lines (simpler priority-based SLA) |

**Issues:**
- Two different SLA models with different structures
- ITSM SLA uses flat priority-based response/resolution times
- KnowledgeBase SLA uses complex target-based model with business hours
- Both registered in DbContext (`SLAPolicies` and `ITSMSLAPolicies`)

### 9.3 Duplicate DbSet Declaration

```csharp
// Both point to Account entity:
public DbSet<Account> Customers { get; set; }   // Line 44
public DbSet<Account> Accounts { get; set; }     // Line 134
```

---

## 10. Workflow Schema Divergence

The SQL schema and EF model use completely different names for the workflow engine:

| Concept | SQL Schema (legacy) | EF Entity (current) | Status |
|---------|-------------------|---------------------|--------|
| Workflow definition | `Workflows` | `WorkflowDefinition` / `WorkflowDefinitions` | ⚠️ Diverged |
| Workflow step | `WorkflowSteps` | `WorkflowNode` / `WorkflowNodes` | ⚠️ Diverged |
| Step execution | `WorkflowStepExecutions` | `WorkflowNodeInstance` / `WorkflowNodeInstances` | ⚠️ Diverged |
| Triggers | `WorkflowTriggers` | (No equivalent — built into WorkflowDefinition) | ⚠️ Removed |
| Version tracking | (None) | `WorkflowVersion` / `WorkflowVersions` | ✨ New |
| Transitions | (None) | `WorkflowTransition` / `WorkflowTransitions` | ✨ New |
| Tasks | (None) | `WorkflowTask` / `WorkflowTasks` | ✨ New |
| Logs | (None) | `WorkflowLog` / `WorkflowLogs` | ✨ New |

**Root Cause:** The workflow engine was redesigned from a step-based model to a node-graph model. The SQL schema files were not updated to reflect this redesign.

---

## 11. Consolidated Remediation Task List

### Priority Legend

- 🔴 **P0 — Critical:** Data integrity risk, naming conflicts, duplicate definitions
- 🟡 **P1 — High:** Missing schema, missing API endpoints for core features
- 🟢 **P2 — Medium:** Missing frontend types, missing services for secondary features
- ⚪ **P3 — Low:** Nice-to-have improvements, documentation alignment

---

### Phase 1: Naming & Duplicate Resolution (🔴 P0)

| # | Task | Layer | Details |
|---|------|-------|---------|
| 1.1 | Remove duplicate `Accounts` DbSet | Backend | Remove duplicate `DbSet<Account> Accounts` (line 134 of CrmDbContext); keep `Customers` with table mapping or migrate fully to `Accounts` |
| 1.2 | Standardize Account/Customer naming | All | Choose one name and update: (a) DbSet property name, (b) SQL table name, (c) Frontend `Customer` type → `Account`, (d) Frontend API paths `/customers` → `/accounts` |
| 1.3 | Rename `CustomerTerritoryAssignments` | Backend | Rename DbSet property to `AccountTerritoryAssignments` and remove the alias |
| 1.4 | Rename `CustomerContacts` junction table | Database | SQL table should be `AccountContacts` to match EF entity |
| 1.5 | Consolidate `KnowledgeArticle` entities | Backend | Merge `Entities/KnowledgeBase/KnowledgeArticle.cs` and `Entities/ITSM/KnowledgeArticle.cs` into a single entity with a discriminator or unified enum |
| 1.6 | Consolidate `SLAPolicy` entities | Backend | Merge `Entities/KnowledgeBase/SLAPolicy.cs` and `Entities/ITSM/SLA.cs` into a single SLA model or use explicit namespacing |
| 1.7 | Unify `ArticleType` enum ordinals | Backend | Fix HowTo=0 vs HowTo=1 inconsistency across KnowledgeBase and ITSM |

### Phase 2: SQL Schema Alignment (🟡 P1)

| # | Task | Layer | Details |
|---|------|-------|---------|
| 2.1 | Add SQL schema for Quote-to-Cash entities | Database | Create `database/schema/010_quote_to_cash.sql` covering: Orders, OrderLineItems, Invoices, InvoiceLineItems, Payments, Subscriptions, SubscriptionItems, SubscriptionUsages, Contracts, CreditMemos, CreditMemoLineItems, CreditApplications |
| 2.2 | Add SQL schema for Sales Performance entities | Database | Create schema for: CommissionPlans, CommissionTiers, CommissionPlanAssignments, Commissions, CommissionStatements, SalesQuotas, SalesForecasts, ForecastLineItems, ForecastHistories |
| 2.3 | Add SQL schema for Product/Pricing entities | Database | Create schema for: ProductBundles, ProductBundleItems, ProductBundleRules, PriceBooks, PriceBookEntries, PricingRules, PricingRuleUsages, DiscountApprovalMatrices |
| 2.4 | Add SQL schema for Marketing Automation entities | Database | Create schema for: CampaignRecipients, CampaignLinkClicks, CampaignABTests, CampaignConversions, CampaignWorkflows, EmailSequences, EmailSequenceSteps, LandingPageBlocks, LandingPageVisits, WebVisitors, WebSessions, WebPageViews, CampaignTouchpoints, AttributionSettings |
| 2.5 | Add SQL schema for AI/Analytics entities | Database | Create schema for: AIModels, Predictions, LeadScores, OpportunityInsights, ChurnRisks, ActionRecommendations, EmailIntelligences |
| 2.6 | Add SQL schema for Reporting entities | Database | Create schema for: ReportDefinitions, ReportFolders, ReportSchedules, ReportExecutions, ReportWidgetConfigs |
| 2.7 | Add SQL schema for E-Signature entities | Database | Create schema for: ESignatureRequests, ESignatureSigners, ESignatureDocuments, ESignatureAuditEvents |
| 2.8 | Add SQL schema for Approval entities | Database | Create schema for: ApprovalLevels, ApprovalGroups, ApprovalGroupMembers, ApprovalRequests, ApprovalSteps |
| 2.9 | Add SQL schema for Lead Routing entities | Database | Create schema for: LeadRoutingRules, LeadRoutingCriterias, LeadRoutingTargets, LeadRoutingLogs, LeadScoreRules, DuplicateMatchFields, DuplicateCandidates |
| 2.10 | Add SQL schema for Relationship entities | Database | Create schema for: RelationshipInteractions, RelationshipMaps, AccountHealthSnapshots |
| 2.11 | Add SQL schema for Dashboard entities | Database | Create schema for: Dashboards, DashboardWidgets |
| 2.12 | Add SQL schema for miscellaneous entities | Database | Create schema for: ContactDetails, ContactInfoLinks, SocialMediaLinks, OAuthTokens, Conversations, FieldMasterDataLinks, EmailTemplateHistoryEntries, EmailTemplateUsages, CloudProviders, DeploymentAttempts, HealthCheckLogs, FormFields, FormSubmissions |
| 2.13 | Update Workflow SQL schema | Database | Replace legacy `Workflows`/`WorkflowSteps`/`WorkflowTriggers` with `WorkflowDefinitions`/`WorkflowVersions`/`WorkflowNodes`/`WorkflowTransitions`/`WorkflowNodeInstances`/`WorkflowTasks`/`WorkflowLogs` |
| 2.14 | Remove orphaned SQL tables | Database | Deprecate or add EF entities for: `EmailLogs`, `Attachments`, `AuditLogs`, `WorkflowTriggers`, `ITSMNumberSequences` |

### Phase 3: API Controller Coverage (🟡 P1)

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

### Phase 4: Frontend Types & Services (🟢 P2)

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
| 4.13 | Rename `Customer` → `Account` in types | Frontend | Update `apiService.ts` and all references |
| 4.14 | Update API paths `/customers` → `/accounts` | Frontend | Update all service files using the old path |

### Phase 5: ITSM Sub-Entity Alignment (🟢 P2)

| # | Task | Layer | Details |
|---|------|-------|---------|
| 5.1 | Add EF entities for ITSM child tables | Backend | Create entities for: IncidentComment, IncidentAttachment, IncidentHistory, ProblemComment, ProblemAttachment, ChangeComment, ChangeAttachment, ChangeTask (if not already nested) |
| 5.2 | Add `ITSMNumberSequences` entity | Backend | Create entity for auto-number generation |
| 5.3 | Add `CatalogRequestComment` entity | Backend | Create entity matching SQL table |
| 5.4 | Add `ArticleAttachment` entity | Backend | Create entity matching SQL table |

### Phase 6: Test & Documentation (⚪ P3)

| # | Task | Layer | Details |
|---|------|-------|---------|
| 6.1 | Add integration tests for new controllers | Testing | Test CRUD operations for Phase 3 controllers |
| 6.2 | Update `DATABASE_SCHEMA.md` | Docs | Reflect current EF-managed schema |
| 6.3 | Update `FEATURE_CHECKLIST.md` | Docs | Mark newly aligned features |
| 6.4 | Add EF migration for schema alignment | Backend | Generate migration that brings SQL in sync with EF model |

---

## Appendix A: Enum Alignment Status

| Enum | Backend | Frontend | Status |
|------|---------|----------|--------|
| `ServiceRequestStatus` | 11 values (New..Reopened) | 11 values | ✅ Aligned |
| `ServiceRequestPriority` | 5 values (Low..Urgent) | 5 values | ✅ Aligned |
| `ServiceRequestChannel` | 8 values | 8 values | ✅ Aligned |
| `OpportunityStage` | 6 values | 6 values | ✅ Aligned |
| `CustomFieldType` | 12 values | 12 values | ✅ Aligned |
| `ArticleType` (KnowledgeBase) | 10 values (0-indexed) | N/A | ⚠️ No frontend |
| `ArticleType` (ITSM) | 6 values (1-indexed) | N/A | ⚠️ No frontend; conflicts with above |
| `ApprovalStatus` | 7 values | 7 values | ✅ Aligned |
| `WidgetType` | 16 values | 16 values | ✅ Aligned |

## Appendix B: Cross-Reference Matrix

| Module | EF Entities | SQL Tables | Controller | Frontend Service | Frontend Page |
|--------|------------|------------|-----------|-----------------|---------------|
| **Accounts** | ✅ Account | ⚠️ Customers/Accounts | ✅ AccountsController | ✅ accountService | ✅ AccountPage |
| **Contacts** | ✅ Contact | ✅ Contacts | ✅ ContactsController | ✅ contactInfoService | ✅ ContactsPage |
| **Leads** | ✅ Lead | ✅ Leads | ✅ LeadsController | ✅ apiService | ✅ LeadsPage |
| **Opportunities** | ✅ Opportunity | ✅ Opportunities | ✅ OpportunitiesController | ✅ apiService | ✅ OpportunitiesPage |
| **Products** | ✅ Product | ✅ Products | ✅ ProductsController | ✅ apiService | ✅ ProductsPage |
| **Quotes** | ✅ Quote | ✅ Quotes | ✅ QuotesController | ✅ apiService | ✅ QuotesPage |
| **Orders** | ✅ Order | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Invoices** | ✅ Invoice | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Payments** | ✅ Payment | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Subscriptions** | ✅ Subscription | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Contracts** | ✅ Contract | ❌ Missing | ❌ Missing | ❌ Missing | ✅ ContractsPage |
| **Campaigns** | ✅ MarketingCampaign | ✅ MarketingCampaigns | ✅ CampaignsController | ✅ campaignExecutionService | ✅ CampaignsPage |
| **Service Requests** | ✅ ServiceRequest | ✅ ServiceRequests | ✅ ServiceRequestsController | ✅ apiService | ✅ ServiceRequestsPage |
| **ITSM Incidents** | ✅ Incident | ✅ Incidents | ✅ IncidentsController | ✅ (ITSM pages) | ✅ IncidentPages |
| **ITSM Problems** | ✅ Problem | ✅ Problems | ✅ (via ITSM) | ✅ (ITSM pages) | ✅ ProblemPages |
| **ITSM Changes** | ✅ Change | ✅ Changes | ✅ (via ITSM) | ✅ (ITSM pages) | ✅ ChangePages |
| **Knowledge Base** | ⚠️ Duplicate | ✅ KnowledgeArticles | ✅ KnowledgeController | ❌ Missing | ✅ KnowledgeBasePage |
| **SLA Policies** | ⚠️ Duplicate | ✅ SLAPolicies | ❌ Missing | ❌ Missing | ❌ Missing |
| **Workflows** | ✅ (8 entities) | ⚠️ Legacy schema | ✅ WorkflowController | ✅ workflowService | ✅ (embedded) |
| **Dashboards** | ✅ Dashboard | ❌ Missing | ✅ DashboardController | ✅ dashboardService | ✅ DashboardPage |
| **Territories** | ✅ AccountTerritory | ❌ Missing | ✅ TerritoriesController | ✅ territoryService | ✅ TerritoriesPage |
| **Approvals** | ✅ ApprovalRequest | ❌ Missing | ✅ ApprovalsController | ✅ approvalService | ✅ ApprovalsPage |
| **Relationships** | ✅ AccountRelationship | ❌ Missing | ✅ RelationshipsController | ✅ relationshipService | ✅ RelationshipsPage |
| **Duplicates** | ✅ DuplicateRule | ✅ DuplicateRules | ✅ DuplicatesController | ✅ duplicateService | ✅ (embedded) |
| **Lead Routing** | ✅ LeadRoutingRule | ❌ Missing | ✅ LeadRoutingController | ✅ leadRoutingService | ✅ LeadRoutingPage |
| **Email Templates** | ✅ EmailTemplate | ✅ EmailTemplates | ✅ EmailTemplatesController | ✅ apiService | ✅ EmailTemplatesPage |
| **Forms** | ✅ FormDefinition | ❌ Missing | ✅ FormsController | ✅ formBuilderService | ✅ FormBuilderPage |
| **Landing Pages** | ✅ LandingPage | ❌ Missing | ✅ LandingPageController | ✅ (inline) | ✅ LandingPagesPage |
| **Commissions** | ✅ Commission | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **E-Signatures** | ✅ ESignatureRequest | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Email Sequences** | ✅ EmailSequence | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Reports** | ✅ ReportDefinition | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **AI/Analytics** | ✅ AIModel | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Product Bundles** | ✅ ProductBundle | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Price Books** | ✅ PriceBook | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |
| **Web Analytics** | ✅ WebVisitor | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing |

---

*This document should be reviewed alongside `SOLUTION_GAPS_REMEDIATION_PLAN.md` and `MASTER_TODO_LIST.md` for full context on implementation status.*
