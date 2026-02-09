# Entity-to-Database Alignment Report (Non-ITSM)

> **Generated:** February 2026  
> **Scope:** All non-ITSM entities — Workflow, CRM Core, Sales, Marketing, System  
> **Method:** Automated cross-reference of 221 EF DbSets vs 224 DB tables  
> **Related:** [EF_DB_FRONTEND_CONSOLIDATED_GAPS.md](EF_DB_FRONTEND_CONSOLIDATED_GAPS.md) Section 10

---

## Executive Summary

| Metric | Value |
|--------|-------|
| EF DbSets Declared | 221 |
| Database Tables | 224 |
| Tables Fully Aligned | **218** (98.6%) |
| Orphan DB Tables (no DbSet) | **3** |
| Missing DB Tables (no table) | **0** (1 false positive) |
| [Table] Attributes | 6 |
| ToTable() Calls | 1 |
| **Workflow Schema Status** | ✅ Fully Aligned — gap doc claim is INCORRECT |
| **Critical Issue Found** | ⚠️ Migration snapshot divergence for Account entity |

---

## 1. Workflow Tables — ALL ALIGNED ✅

### Gap Doc Claim (Section 10)
> "SQL uses legacy names (Workflows, WorkflowSteps) while EF uses new names (WorkflowDefinitions, WorkflowNodes, etc.)"

### Reality: **This claim is INCORRECT**

There are **no legacy "Workflows" or "WorkflowSteps" tables** in the database. All 10 workflow-related tables already use the new EF entity names:

| EF Entity Class | DbSet Name | DB Table | Status |
|-----------------|------------|----------|--------|
| WorkflowDefinition | WorkflowDefinitions | WorkflowDefinitions | ✅ Match |
| WorkflowVersion | WorkflowVersions | WorkflowVersions | ✅ Match |
| WorkflowNode | WorkflowNodes | WorkflowNodes | ✅ Match |
| WorkflowTransition | WorkflowTransitions | WorkflowTransitions | ✅ Match |
| WorkflowInstance | WorkflowInstances | WorkflowInstances | ✅ Match |
| WorkflowNodeInstance | WorkflowNodeInstances | WorkflowNodeInstances | ✅ Match |
| WorkflowTask | WorkflowTasks | WorkflowTasks | ✅ Match |
| WorkflowLog | WorkflowLogs | WorkflowLogs | ✅ Match |
| WorkflowTrigger | WorkflowTriggers | WorkflowTriggers | ✅ Match |
| CampaignWorkflow | CampaignWorkflows | CampaignWorkflows | ✅ Match |

**Why they match:** None of the 9 workflow entity classes have `[Table]` attributes. They all use the default EF convention where the **DbSet property name becomes the table name**. The DbSet names match the DB tables exactly.

**Entity location:** `CRM.Backend/src/CRM.Core/Entities/Workflow/` (9 files)  
**DbSet declarations:** `CrmDbContext.cs` lines 165–173

### Action Required
- **Update gap doc Section 10** to mark Workflow schema as ✅ Aligned
- No code changes needed for workflow entities

---

## 2. [Table] Attribute Inventory

Six entity classes across the project have explicit `[Table]` attributes:

| # | Entity Class | File | [Table] Value | DB Table Exists | Purpose |
|---|-------------|------|---------------|-----------------|---------|
| 1 | `Account` | Account.cs:82 | `"Customers"` | ✅ `Customers` | Customer→Account rename; backward compatibility |
| 2 | `AccountTerritoryAssignment` | AccountTerritory.cs:149 | `"CustomerTerritoryAssignments"` | ✅ `CustomerTerritoryAssignments` | Rename backward compat; also has `[Column("CustomerId")]` on AccountId |
| 3 | `OpportunityProduct` | Opportunity.cs:227 | `"OpportunityProducts"` | ✅ `OpportunityProducts` | Explicit mapping (matches convention anyway) |
| 4 | `LeadProductInterest` | Lead.cs:245 | `"LeadProductInterests"` | ✅ `LeadProductInterests` | Explicit mapping (matches convention anyway) |
| 5 | `LLMProviderSetting` | LLMProviderSetting.cs:42 | `"llm_provider_settings"` | ✅ `llm_provider_settings` | Lowercase snake_case naming |
| 6 | `Subscription` | Subscription.cs:45 | `"Subscriptions"` | ✅ `Subscriptions` | Fixed from incorrect `[Table("Accounts")]` |

### Observations
- Items 3 and 4 (`OpportunityProduct`, `LeadProductInterest`) have `[Table]` attributes that match what EF convention would generate anyway — the attributes are redundant but harmless
- Items 1 and 2 are essential for backward compatibility after the Customer→Account rename
- Item 5 uses snake_case (the only non-PascalCase table in the system)
- Item 6 was previously incorrect (`[Table("Accounts")]`) and was fixed to `[Table("Subscriptions")]`

---

## 3. ToTable() Calls in CrmDbContext

Only **1 actual `ToTable()` call** found in `CrmDbContext.OnModelCreating()`:

| Location | Entity | ToTable Value | Purpose |
|----------|--------|---------------|---------|
| CrmDbContext.cs line 885 | LLMProviderSetting | `"llm_provider_settings"` | Reinforces the [Table] attribute (redundant but intentional) |

Line 2884 contains a **comment only** (not executable code):
```csharp
// Do NOT add ToTable() mappings that would cause shared-table conflicts
```

---

## 4. Cross-Reference Results: 221 DbSets vs 224 DB Tables

### 4.1 False Positive — AccountTerritoryAssignments

The cross-reference script flagged `AccountTerritoryAssignments` as a "missing table" because no DB table has that exact name. However, this is correctly handled:

- DbSet: `AccountTerritoryAssignments` → entity `AccountTerritoryAssignment`
- Entity has `[Table("CustomerTerritoryAssignments")]`
- DB table `CustomerTerritoryAssignments` exists ✅
- Result: **FALSE POSITIVE** — no issue

The alias DbSet `CustomerTerritoryAssignments => AccountTerritoryAssignments` at line 184 is a convenience alias for code using the old name.

### 4.2 Orphan Tables — 3 DB Tables With No DbSet

#### Table 1: `Accounts` — ⚠️ ORPHAN (Ghost Table from Stale Migration)

| Aspect | Details |
|--------|---------|
| **Table** | `Accounts` |
| **Schema** | AccountNumber, CustomerId, ProductId, MRR, ARR, BillingCycle, BillingStartDate, etc. |
| **Created By** | EF migrations using stale `CrmDbContextModelSnapshot.cs` |
| **Root Cause** | Snapshot has `Account → ToTable("Accounts")` (line 197), but source code has `[Table("Customers")]` |
| **Related** | `Customers` table also exists with the REAL Account data (FirstName, LastName, Company, Email) |
| **Severity** | ⚠️ Medium — table is unused at runtime but causes confusion |

**This is NOT the same entity twice.** The schemas are completely different:
- `Accounts` table: AccountNumber, MRR, ARR, BillingCycle (old Account entity shape from an earlier migration)
- `Customers` table: FirstName, LastName, Email, Company, Category (current Account entity shape)

#### Table 2: `ArticleFeedback` — ⚠️ ORPHAN (SQL Migration Leftover)

| Aspect | Details |
|--------|---------|
| **Table** | `ArticleFeedback` (singular) |
| **Schema** | PK: `FeedbackId`, FK: `ArticleId`, columns: UserId, IsHelpful, Comment, CreatedAt |
| **Counterpart** | `ArticleFeedbacks` (plural) — EF-managed with standard `Id` PK, `RowVersion`, BaseEntity pattern |
| **Created By** | Likely a raw SQL migration script |
| **Severity** | 🟡 Low — unused orphan |

The EF-managed table `ArticleFeedbacks` (plural) has the standard BaseEntity pattern with `Id` PK, `KnowledgeArticleId`, `Rating`, `RowVersion`. The singular `ArticleFeedback` table has a non-standard `FeedbackId` PK and different schema.

#### Table 3: `MarketingCampaignProduct` — ✅ Expected (EF Auto-Generated Junction)

| Aspect | Details |
|--------|---------|
| **Table** | `MarketingCampaignProduct` |
| **Schema** | `MarketingCampaignsId` (PK), `ProductsId` (PK) — composite primary key |
| **Created By** | EF Core many-to-many convention for `MarketingCampaign ↔ Product` relationship |
| **Severity** | ✅ None — this is expected behavior |

Confirmed in `CrmDbContextModelSnapshot.cs` at lines 15153–15165: `modelBuilder.Entity("MarketingCampaignProduct")` with `b.ToTable("MarketingCampaignProduct")`. This is an implicit join table — no explicit DbSet is needed.

---

## 5. ⚠️ CRITICAL ISSUE: Migration Snapshot Divergence

### The Problem

The `CrmDbContextModelSnapshot.cs` (line 197) maps the `Account` entity to the `"Accounts"` table:

```csharp
modelBuilder.Entity("CRM.Core.Entities.Account", b =>
{
    // ... properties ...
    b.ToTable("Accounts");  // ← STALE — doesn't match source code
});
```

But the `Account` entity in source code has:

```csharp
[Table("Customers")]  // ← CURRENT mapping
public class Account : BaseEntity { ... }
```

### Impact

| Scenario | Behavior |
|----------|----------|
| **Runtime (`EnsureCreated`)** | Uses `[Table("Customers")]` → reads/writes `Customers` table ✅ |
| **`dotnet ef migrations add`** | Compares snapshot (`Accounts`) to model (`Customers`) → generates a RENAME migration ❌ |
| **`dotnet ef database update`** | Would try to rename `Accounts` → `Customers` or create conflicts ❌ |

### Root Cause

The `[Table("Customers")]` attribute was added to the Account entity AFTER the last EF migration was generated. The snapshot was never regenerated.

### Recommended Fix

**Option A** (Preferred): Regenerate the snapshot to match current code
```bash
cd CRM.Backend
dotnet ef migrations add FixAccountTableMapping --project src/CRM.Infrastructure --startup-project src/CRM.Api
# Review generated migration, then:
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

**Option B**: Manually update the snapshot
- Change line 197 in `CrmDbContextModelSnapshot.cs` from `b.ToTable("Accounts")` to `b.ToTable("Customers")`

**Option C** (Safest if not using EF migrations): Leave as-is since runtime uses `EnsureCreated()` which correctly maps to `Customers`

---

## 6. Subscription [Table] Fix Verification

Migration 019 (`019_create_missing_entity_tables.sql`) noted at line 594:
> "Note: The Subscription entity has [Table("Accounts")] which is incorrect. Creating as 'Subscriptions' to match the DbSet name."

**Current state verification:**
- Entity source: `[Table("Subscriptions")]` ✅ Fixed
- DB table: `Subscriptions` ✅ Exists
- Snapshot: `b.ToTable("Subscriptions")` ✅ Matches

The Subscription entity [Table] bug has been properly fixed.

---

## 7. Summary of Required Actions

### 🔴 High Priority

| # | Action | File | Details |
|---|--------|------|---------|
| 1 | Fix Account snapshot divergence | CrmDbContextModelSnapshot.cs:197 | Change `ToTable("Accounts")` to `ToTable("Customers")` or regenerate snapshot |

### 🟡 Medium Priority

| # | Action | Details |
|---|--------|---------|
| 2 | Drop orphan `Accounts` table | Contains stale Account entity schema; unused at runtime. **Verify no data** before dropping. |
| 3 | Drop orphan `ArticleFeedback` table | Singular orphan; `ArticleFeedbacks` (plural) is the EF-managed table. **Verify no data** before dropping. |
| 4 | Update gap doc Section 10 | Mark Workflow schema as ✅ Aligned. Remove false claim about legacy table names. |

### ✅ No Action Required

| Item | Status |
|------|--------|
| Workflow entities (9) | All aligned — no [Table] attributes needed |
| [Table] attributes (6) | All correct and intentional |
| ToTable() calls (1) | Correct — `llm_provider_settings` |
| `MarketingCampaignProduct` table | Expected EF auto-generated junction table |
| `AccountTerritoryAssignment` mapping | Correctly handled by `[Table("CustomerTerritoryAssignments")]` |
| `Subscription` mapping | Fixed to `[Table("Subscriptions")]` |

---

## 8. Full DbSet ↔ Table Mapping Reference

<details>
<summary>Click to expand — 221 DbSets with table mappings</summary>

All DbSets use **convention-based mapping** (DbSet name = table name) unless marked with `[override]`:

### Core CRM (lines 44–59)
| DbSet | Entity | Table | Override |
|-------|--------|-------|----------|
| Customers | Account | Customers | `[Table("Customers")]` on entity |
| Contacts | Contact | Contacts | — |
| Leads | Lead | Leads | — |
| Opportunities | Opportunity | Opportunities | — |
| Products | Product | Products | — |
| Interactions | Interaction | Interactions | — |
| CrmTasks | CrmTask | CrmTasks | — |
| Notes | Note | Notes | — |

### Alias DbSets (do NOT create tables)
| DbSet | Points To | Purpose |
|-------|-----------|---------|
| `Accounts` | `=> Customers` | Code backward compat |
| `CustomerTerritoryAssignments` | `=> AccountTerritoryAssignments` | Code backward compat |

### Contact Information (lines 62–105)
| DbSet | Table |
|-------|-------|
| AccountContacts | AccountContacts |
| OpportunityProducts | OpportunityProducts (`[Table]` override — matches convention) |
| Quotes | Quotes |
| QuoteLineItems | QuoteLineItems |
| CalendarEvents | CalendarEvents |
| CalendarIntegrations | CalendarIntegrations |
| CalendarSyncLogs | CalendarSyncLogs |
| EmailIntegrations | EmailIntegrations |
| EmailSyncLogs | EmailSyncLogs |
| EmailMessageMappings | EmailMessageMappings |
| Addresses | Addresses |
| PhoneNumbers | PhoneNumbers |
| EmailAddresses | EmailAddresses |
| SocialMediaAccounts | SocialMediaAccounts |
| EntityAddressLinks | EntityAddressLinks |
| EntityPhoneLinks | EntityPhoneLinks |
| EntityEmailLinks | EntityEmailLinks |
| EntitySocialMediaLinks | EntitySocialMediaLinks |
| ContactDetails | ContactDetails |
| SocialAccounts | SocialAccounts |
| ContactInfoLinks | ContactInfoLinks |

### System (lines 106–136)
| DbSet | Table |
|-------|-------|
| LookupCategories | LookupCategories |
| LookupItems | LookupItems |
| Tags | Tags |
| EntityTags | EntityTags |
| CustomFields | CustomFields |
| ModuleFieldConfigurations | ModuleFieldConfigurations |
| ModuleUIConfigs | ModuleUIConfigs |
| ServiceRequests | ServiceRequests |
| ServiceRequestCategories | ServiceRequestCategories |
| ServiceRequestSubcategories | ServiceRequestSubcategories |
| ServiceRequestTypes | ServiceRequestTypes |
| ServiceRequestCustomFieldDefinitions | ServiceRequestCustomFieldDefinitions |
| ServiceRequestCustomFieldValues | ServiceRequestCustomFieldValues |
| SystemSettings | SystemSettings |
| Users | Users |
| UserGroups | UserGroups |
| UserGroupMembers | UserGroupMembers |
| UserProfiles | UserProfiles |
| UserApprovalRequests | UserApprovalRequests |
| Departments | Departments |
| OAuthTokens | OAuthTokens |

### Communications & Templates (lines 139–149)
| DbSet | Table |
|-------|-------|
| CommunicationChannels | CommunicationChannels |
| CommunicationMessages | CommunicationMessages |
| Conversations | Conversations |
| EmailTemplates | EmailTemplates |
| EmailSequences | EmailSequences |
| EmailSequenceSteps | EmailSequenceSteps |
| EmailSequenceEnrollments | EmailSequenceEnrollments |
| ZipCodes | ZipCodes |
| Localities | Localities |

### Social, Cloud, Dashboards (lines 152–162)
| DbSet | Table |
|-------|-------|
| SocialMediaFollows | SocialMediaFollows |
| CloudProviders | CloudProviders |
| CloudDeployments | CloudDeployments |
| DeploymentAttempts | DeploymentAttempts |
| HealthCheckLogs | HealthCheckLogs |
| Dashboards | Dashboards |
| DashboardWidgets | DashboardWidgets |

### Workflow (lines 165–173)
| DbSet | Table |
|-------|-------|
| WorkflowDefinitions | WorkflowDefinitions |
| WorkflowVersions | WorkflowVersions |
| WorkflowNodes | WorkflowNodes |
| WorkflowTransitions | WorkflowTransitions |
| WorkflowInstances | WorkflowInstances |
| WorkflowNodeInstances | WorkflowNodeInstances |
| WorkflowTasks | WorkflowTasks |
| WorkflowLogs | WorkflowLogs |
| WorkflowTriggers | WorkflowTriggers |

### Relationships & Territories (lines 176–191)
| DbSet | Table | Note |
|-------|-------|------|
| RelationshipTypes | RelationshipTypes | |
| AccountRelationships | AccountRelationships | |
| RelationshipInteractions | RelationshipInteractions | |
| AccountHealthSnapshots | AccountHealthSnapshots | |
| RelationshipMaps | RelationshipMaps | |
| AccountTerritories | AccountTerritories | |
| AccountTerritoryAssignments | CustomerTerritoryAssignments | `[Table]` override |
| MarketingCampaigns | MarketingCampaigns | |
| CampaignMetrics | CampaignMetrics | |
| CampaignRecipients | CampaignRecipients | |
| CampaignWorkflows | CampaignWorkflows | |

### Sales & Billing (lines 196–208)
| DbSet | Table |
|-------|-------|
| Orders | Orders |
| OrderLineItems | OrderLineItems |
| Invoices | Invoices |
| InvoiceLineItems | InvoiceLineItems |
| Payments | Payments |
| Subscriptions | Subscriptions (`[Table]` override — matches convention) |
| SubscriptionItems | SubscriptionItems |
| SubscriptionUsages | SubscriptionUsages |
| Contracts | Contracts |
| CreditMemos | CreditMemos |
| CreditMemoLineItems | CreditMemoLineItems |
| CreditApplications | CreditApplications |

### Lead Management (lines 213–248)
| DbSet | Table |
|-------|-------|
| LeadProductInterests | LeadProductInterests (`[Table]` override — matches convention) |
| LeadRoutingRules | LeadRoutingRules |
| LeadRoutingCriteria | LeadRoutingCriteria |
| LeadRoutingTargets | LeadRoutingTargets |
| LeadRoutingLogs | LeadRoutingLogs |
| DuplicateRules | DuplicateRules |
| DuplicateMatchFields | DuplicateMatchFields |
| DuplicateCandidates | DuplicateCandidates |
| DuplicateMergeHistories | DuplicateMergeHistories |
| WebVisitors | WebVisitors |
| WebSessions | WebSessions |
| WebPageViews | WebPageViews |
| FormDefinitions | FormDefinitions |
| FormFields | FormFields |
| FormSubmissions | FormSubmissions |
| AttributionSettings | AttributionSettings |
| CampaignTouchpoints | CampaignTouchpoints |
| CampaignAttributionSummaries | CampaignAttributionSummaries |
| EmailSequenceStepExecutions | EmailSequenceStepExecutions |
| CampaignLinkClicks | CampaignLinkClicks |
| CampaignABTests | CampaignABTests |
| CampaignConversions | CampaignConversions |

### Marketing Automation (lines 231–248)
| DbSet | Table |
|-------|-------|
| LandingPages | LandingPages |
| LandingPageBlocks | LandingPageBlocks |
| LandingPageVisits | LandingPageVisits |
| EventAttendees | EventAttendees |
| LeadScoreRules | LeadScoreRules |

### CPQ & Approvals (lines 252–271)
| DbSet | Table |
|-------|-------|
| ProductBundles | ProductBundles |
| ProductBundleItems | ProductBundleItems |
| ProductBundleRules | ProductBundleRules |
| PriceBooks | PriceBooks |
| PriceBookEntries | PriceBookEntries |
| PricingRules | PricingRules |
| PricingRuleUsages | PricingRuleUsages |
| DiscountApprovalMatrices | DiscountApprovalMatrices |
| ApprovalLevels | ApprovalLevels |
| ApprovalGroups | ApprovalGroups |
| ApprovalGroupMembers | ApprovalGroupMembers |
| ApprovalRequests | ApprovalRequests |
| ApprovalSteps | ApprovalSteps |
| ESignatureRequests | ESignatureRequests |
| ESignatureSigners | ESignatureSigners |
| ESignatureDocuments | ESignatureDocuments |
| ESignatureAuditEvents | ESignatureAuditEvents |

### Sales Performance (lines 275–303)
| DbSet | Table |
|-------|-------|
| CommissionPlans | CommissionPlans |
| CommissionTiers | CommissionTiers |
| CommissionPlanAssignments | CommissionPlanAssignments |
| Commissions | Commissions |
| CommissionStatements | CommissionStatements |
| SalesQuotas | SalesQuotas |
| SalesForecasts | SalesForecasts |
| ForecastLineItems | ForecastLineItems |
| ForecastHistories | ForecastHistories |
| Teams | Teams |
| TeamMembers | TeamMembers |
| LLMProviderSettings | llm_provider_settings | `[Table]` + `ToTable()` override |
| AIModels | AIModels |
| Predictions | Predictions |
| LeadScores | LeadScores |
| OpportunityInsights | OpportunityInsights |
| ChurnRisks | ChurnRisks |
| ActionRecommendations | ActionRecommendations |
| EmailIntelligences | EmailIntelligences |
| ReportDefinitions | ReportDefinitions |
| ReportFolders | ReportFolders |
| ReportSchedules | ReportSchedules |
| ReportExecutions | ReportExecutions |
| ReportWidgetConfigs | ReportWidgetConfigs |

### Knowledge Base & SLA (lines 307–315)
| DbSet | Table |
|-------|-------|
| KnowledgeArticles | KnowledgeArticles |
| KnowledgeCategories | KnowledgeCategories |
| ServiceRequestArticles | ServiceRequestArticles |
| ArticleFeedbacks | ArticleFeedbacks |
| SLAPolicies | SLAPolicies |
| SLATargets | SLATargets |
| SLAInstances | SLAInstances |
| BusinessHoursConfigs | BusinessHoursConfigs |
| EscalationRules | EscalationRules |

### ITSM (lines 319–370) — 52 DbSets
*(Out of scope for this report — see ITSM alignment docs)*

</details>

---

## 9. Migration 019 Table Coverage

Migration `019_create_missing_entity_tables.sql` creates 27 tables. All verified present in DB:

| Group | Tables Created | All Present in DB |
|-------|----------------|-------------------|
| ITSM | ITSMKnowledgeArticles, ITSMSLAPolicies, ITSMSLAInstances, BusinessHoursSchedules, CatalogCategories, CatalogVariables, CatalogRequestApprovals, CatalogRequestComments, ChangeAttachments, ITSMArticleFeedback, ArticleIncidents | ✅ |
| Calendar | CalendarIntegrations, CalendarSyncLogs, CalendarEventMappings | ✅ |
| Email | EmailIntegrations, EmailSyncLogs, EmailMessageMappings | ✅ |
| Email Template | EmailTemplateHistoryEntries, EmailTemplateUsages, EmailTemplateVersions | ✅ |
| Landing Pages | LandingPages, LandingPageBlocks, LandingPageVisits | ✅ |
| Events | EventAttendees | ✅ |
| Lead Scoring | LeadScoreRules | ✅ |
| Subscription | Subscriptions, SubscriptionUsageLimits | ✅ |
| Workflow | WorkflowTriggers | ✅ |

---

## 10. Methodology

### Data Sources
1. **EF DbSets:** `grep "public DbSet"` on `CRM.Infrastructure/Data/CrmDbContext.cs` (3046 lines)
2. **DB Tables:** `SHOW TABLES` on live MariaDB (`crm_db` on 192.168.0.9)
3. **[Table] Attributes:** `grep "[Table("` across all entity files
4. **ToTable() Calls:** `grep "ToTable"` in CrmDbContext.cs
5. **Migration Snapshot:** `CrmDbContextModelSnapshot.cs` (18245 lines)
6. **Migration 019:** `019_create_missing_entity_tables.sql` (731 lines)

### Cross-Reference Script
Automated bash script comparing sorted DbSet names against sorted DB table names, accounting for:
- Alias DbSets (excluded from comparison)
- `[Table]` attribute overrides (Account→Customers, AccountTerritoryAssignment→CustomerTerritoryAssignments)
- EF auto-generated junction tables (MarketingCampaignProduct)

---

**END OF REPORT**
