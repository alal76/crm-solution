# EF/DB/Frontend Gap Remediation Summary

> **Date:** February 9, 2026  
> **Source Document:** [EF_DB_FRONTEND_CONSOLIDATED_GAPS.md](EF_DB_FRONTEND_CONSOLIDATED_GAPS.md)  
> **BVT Status:** 52/52 (100%) ✅  
> **Commits:** 5 (3c468c6 → c8ba986)

---

## Completed Remediation (Phase 1 & 2)

### Commit 1: `3c468c6` — Duplicate DbSets & Territory Naming

| Issue | Fix |
|-------|-----|
| **Duplicate `Accounts` DbSet** (Gap Doc §1) | Converted to read-only alias: `public DbSet<Account> Accounts => Customers;` |
| **`CustomerTerritoryAssignments` vs `AccountTerritoryAssignments`** (Gap Doc §2) | Made `AccountTerritoryAssignments` the primary DbSet; `CustomerTerritoryAssignments` is now the alias |

**Files changed:** `CrmDbContext.cs`

---

### Assessment: KA/SLA Duplicates (Gap Doc §3)

| Claim | Finding | Action |
|-------|---------|--------|
| KnowledgeArticles duplicated | `KnowledgeArticles` (KB namespace) and `ITSMKnowledgeArticles` (ITSM namespace) map to **separate tables** | ✅ No fix needed |
| SLAPolicies duplicated | `SLAPolicies` (ServiceDesk namespace) and `ITSMSLAPolicies` (ITSM namespace) map to **separate tables** | ✅ No fix needed |
| KB `SlaPolicy` property | Dead code — KB entity references SLA but it's unused | ⚠️ Low priority cleanup |

---

### Commit 2: `08bf463` — Create 27 Missing Database Tables

**Migration:** `019_create_missing_entity_tables.sql` (731 lines)

Created tables for entities that existed in EF Core but had no corresponding database table:

| Category | Tables Created |
|----------|----------------|
| **ITSM** | `ITSMServiceCatalogItems`, `ITSMServiceCatalogCategories`, `ITSMApprovalWorkflows`, `ITSMApprovalSteps`, `ITSMBlackoutPeriods`, `ITSMMaintenanceWindows`, `ITSMReleases`, `ITSMReleaseItems` |
| **Calendar** | `CalendarEvents`, `CalendarReminders` |
| **Email** | `EmailSequenceStepActions`, `CampaignSegments`, `CampaignSegmentCriteria` |
| **Landing Pages** | `LandingPages`, `LandingPageVersions` |
| **Subscriptions** | `SubscriptionItems`, `SubscriptionUsages` |
| **Quotes/Orders** | `OrderLineItems`, `InvoiceLineItems`, `CreditMemoLineItems`, `CreditApplications` |
| **Commissions** | `CommissionTiers`, `CommissionPlanAssignments`, `CommissionStatements` |
| **Sales** | `SalesQuotas`, `ForecastLineItems`, `ForecastHistories` |

**Table count:** 196 → 224 (+27, later reduced to 222 after orphan cleanup)

**Key fixes during creation:**
- MariaDB reserved words (`MinValue` → `` `MinValue` ``, `MaxValue`, `Comment`) backtick-escaped
- All tables follow BaseEntity pattern (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion)

---

### Commit 3: `43d5433` — Fix Entity Table Mappings

| Entity | Before | After |
|--------|--------|-------|
| `Subscription.cs` | `[Table("Accounts")]` ❌ | `[Table("Subscriptions")]` ✅ |
| `Subscription.cs` | 5 unnecessary `[Column]` overrides | Removed (EF conventions match) |
| `LLMProviderSetting.cs` | No `[Table]` attribute | `[Table("llm_provider_settings")]` ✅ |

**Root cause:** The `Subscription` entity mapping to `"Accounts"` caused EF Core's `EnsureCreated()` to create a phantom `Accounts` table with subscription schema, which then accumulated bogus FK constraints.

---

### Commit 4: `23bb8e6` — ITSM SLA Columns & Ticket Number Fix

**Migration:** `020_fix_itsm_sla_instance_columns.sql`

| Old Column Name | New Column Name |
|-----------------|-----------------|
| `NotificationSent50` | `Notification50Sent` |
| `NotificationSent75` | `Notification75Sent` |
| `NotificationSent90` | `Notification90Sent` |
| `NotificationSent100` | `Notification100Sent` |
| `NotificationSentEscalation` | `EscalationNotificationSent` |
| `NotificationSentBreach` | `BreachNotificationSent` |

**Ticket number fix (`ServiceRequestService.cs`):**
- **Problem:** Static `_ticketCounter` reset to 0 on container restart, causing duplicate ticket numbers → 500 error on BVT-06-001
- **Fix:** New `GenerateTicketNumberAsync()` method queries DB for max existing ticket number on first call, then increments atomically using `Interlocked.Increment` with `SemaphoreSlim` synchronization

---

### Commit 5: `c8ba986` — Orphan Table & FK Cleanup

**Migration:** `021_drop_orphan_tables.sql`

**Root cause analysis:** The `Subscription.cs [Table("Accounts")]` bug (fixed in commit 3) caused `EnsureCreated()` to create a phantom `Accounts` table. EF Core then created FK constraints from 8 other tables pointing to this phantom table instead of the correct `Subscriptions` table.

**Fixes applied:**
1. **Dropped 8 bogus FK constraints:**
   - `FK_Payments_Accounts_SubscriptionId`
   - `FK_ContactInfoLinks_Accounts_SubscriptionId`
   - `FK_Commissions_Accounts_SubscriptionId`
   - `FK_InvoiceLineItems_Accounts_SubscriptionId`
   - `FK_Opportunities_Accounts_SubscriptionId`
   - `FK_SubscriptionItems_Accounts_SubscriptionId`
   - `FK_Invoices_Accounts_SubscriptionId`
   - `FK_SubscriptionUsages_Accounts_SubscriptionId`

2. **Added 2 correct FK constraints:**
   - `FK_SubscriptionItems_Subscriptions_SubscriptionId`
   - `FK_SubscriptionUsages_Subscriptions_SubscriptionId`

3. **Dropped orphan tables:**
   - `Accounts` (0 rows, phantom subscription data)
   - `ArticleFeedback` (0 rows, legacy duplicate of `ArticleFeedbacks`)

**Table count:** 224 → 222

---

## Verification Results

### ITSM Entity-DB Alignment (Task 9)
- **Scope:** 35 ITSM tables, 447 properties
- **Result:** **0 mismatches** — perfect alignment
- **Method:** Automated cross-reference of C# entity properties vs MariaDB `DESCRIBE` output

### Full Entity-DB Cross-Reference (Task 10)
- **221 DbSets** mapped to database tables
- **222 database tables** (1 extra: `MarketingCampaignProduct` auto-junction)
- **2 [Table] overrides:** `AccountTerritoryAssignments` → `CustomerTerritoryAssignments`, `LLMProviderSettings` → `llm_provider_settings`
- **Result:** All mismatches fully accounted for

### BVT Test Results
- **52/52 tests passing** (100%)
- **All 14 test groups:** Auth, Accounts, Contacts, Leads, Opportunities, Service Requests, Products, Campaigns, Users, User Groups, Dashboard, Notes, Settings, Health

---

## Remaining Work (Future Phases)

These items from the gap document were assessed but deferred as they require new feature development rather than alignment fixes:

### Phase 3: Missing API Controllers
14 controllers needed for entities that have services but no REST endpoints:
- Orders, Invoices, Payments, Subscriptions, Commissions
- E-Signatures, Email Sequences, Reports, AI Analytics
- Product Bundles, Price Books, Web Analytics
- Contracts, Teams

### Phase 4: Frontend Type/Service Alignment
14 frontend services need TypeScript types aligned with backend DTOs:
- Order, Invoice, Payment, Subscription, Commission services
- Contract, Team, Email Template services
- Product Bundle, Price Book services

### Phase 5: ITSM Sub-Entity Alignment
4 ITSM sub-entities flagged in gap doc — **verified as already aligned** (Task 9 found 0 mismatches). No work needed.

### Phase 6: Test & Documentation
- Unit tests for new services
- Integration tests for new controllers
- Update API documentation

---

## Files Created/Modified

| File | Action | Lines |
|------|--------|-------|
| `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs` | Modified | ~10 lines changed |
| `CRM.Backend/src/CRM.Core/Entities/Subscription.cs` | Modified | ~8 lines changed |
| `CRM.Backend/src/CRM.Core/Entities/LLMProviderSetting.cs` | Modified | ~1 line added |
| `CRM.Backend/src/CRM.Infrastructure/Services/ServiceRequestService.cs` | Modified | ~35 lines added |
| `CRM.Backend/migrations/019_create_missing_entity_tables.sql` | Created | 731 lines |
| `CRM.Backend/migrations/020_fix_itsm_sla_instance_columns.sql` | Created | ~30 lines |
| `CRM.Backend/migrations/021_drop_orphan_tables.sql` | Created | ~80 lines |
| `docs/ENTITY_DB_ALIGNMENT_REPORT.md` | Created | ~540 lines |
| `docs/EF_DB_FRONTEND_REMEDIATION_SUMMARY.md` | Created | This file |

---

**END OF REMEDIATION SUMMARY**
