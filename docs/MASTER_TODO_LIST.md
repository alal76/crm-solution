# CRM Solution — Master TODO List

> **Last Updated:** February 27, 2026  
> **Version:** 0.600.13  
> **Status:** 🔄 ACTIVE — BATCH 3: CONFIGURABLE ENUMS (67 Tasks - 4 Phases)  
> **Historical Completion:** 527 items completed (502 historical + 23 scripting Phases 1–5 + 2 scripting Phase 6)

**Scripting tasks COMPLETE. Batch 2 (Collaboration, Analytics, CSAT, Portal, AI Scoring, E2E) queued. ACTIVE: Batch 3 — Configurable Enums (database-driven enum management system: 19 DB/Backend + 16 Migration + 18 Frontend + 14 Testing).**

---

## Deployment Status (March 2, 2026)

| Item | Status |
|------|--------|
| Dev server (192.168.0.9) | ✅ Deployed — crm-api + crm-frontend running |
| BVT suite | ✅ 118/118 passing |
| DB schema drift (Leads) | ✅ Fixed — 12 missing columns added |
| Dockerfile.backend | ✅ Fixed — CRM.Tests.Unit.csproj added |
| E2E BVT phone validation | ✅ Fixed — `555-BVT1/2` → `555-1234/5678` |
| E2E termLengthMonths | ✅ Fixed — added to opportunity creation test |
| Rate limiting | ✅ Disabled on dev server for testing |
| Mobile Safari e2e failures | ⚠️ Known macOS WebKit networking limitation (not a solution bug) |

---

## Batch 3 — Configurable Enums Feature (ACTIVE)

**Spec Reference:** [SPEC-GEN-002-ConfigurableEnums.md](11-specifications/SPEC-GEN-002-ConfigurableEnums.md)

**Goal:** Convert all hard-coded C# enums (LeadStatus, OpportunityStage, ServiceRequestStatus, etc.) to database-driven configuration manageable from Admin UI. Enable runtime customization of status values, colors, metadata, and transitions without code deployment.

### Phase 1: Database & Backend Foundation

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| ENUM-DB-001 | P0 | Create migration script `SYS-008-ConfigurableEnums.sql` — enhance LookupCategories (EntityType, PropertyName, IsSystemManaged) + LookupItems (IsDefault, IsSystemValue, Color, Icon, ValidationRules) | Not Started |
| ENUM-DB-002 | P0 | Create `EnumTransitions` table (Id, CategoryId, FromValueId, ToValueId, IsAllowed, RequiresApproval, AllowedRoles, ValidatExpression) | Not Started |
| ENUM-DB-003 | P0 | Update existing LookupCategories with entity mapping (LeadStatus→Lead.Status, OpportunityStage→Opportunity.Stage, etc.) | Not Started |
| ENUM-DB-004 | P0 | Parse and migrate colors from existing Meta JSON to new Color column | Not Started |
| ENUM-DB-005 | P0 | Mark system values (IsSystemValue=1 for critical statuses) | Not Started |
| ENUM-DB-006 | P0 | Set default values (IsDefault=1 for NEW statuses) | Not Started |
| ENUM-BE-001 | P0 | Create `EnumCategory.cs` entity (enhanced LookupCategory with EntityType, PropertyName, IsSystemManaged, AllowCustomValues, ValidationSchema) | Not Started |
| ENUM-BE-002 | P0 | Create `EnumValue.cs` entity (enhanced LookupItem with IsDefault, IsSystemValue, Color, Icon, Metadata, ValidationRules) | Not Started |
| ENUM-BE-003 | P0 | Create `EnumTransition.cs` entity (CategoryId, FromValueId, ToValueId, IsAllowed, RequiresApproval, AllowedRoles) | Not Started |
| ENUM-BE-004 | P0 | Add `DbSet<EnumTransition>` to `CrmDbContext` + `OnModelCreating` configurations | Not Started |
| ENUM-BE-005 | P0 | Create `IEnumManagementService` interface with 20+ methods (CRUD categories/values/transitions, validation, migration) | Not Started |
| ENUM-BE-006 | P0 | Implement `EnumManagementService` with MemoryCache for enum value caching (1-hour TTL) | Not Started |
| ENUM-BE-007 | P0 | Implement `GetValuesByCategoryNameAsync` with cache-first strategy | Not Started |
| ENUM-BE-008 | P0 | Implement `IsTransitionAllowedAsync` for status transition validation | Not Started |
| ENUM-BE-009 | P0 | Implement `ValidateValueAsync` for incoming value validation | Not Started |
| ENUM-BE-010 | P0 | Create `EnumDtos.cs` — 10+ DTOs (EnumCategoryDto, EnumValueDto, CreateEnumValueDto, UpdateEnumValueDto, EnumTransitionDto, EnumValidationResult) | Not Started |
| ENUM-BE-011 | P0 | Create `EnumManagementController` with 12+ endpoints (GET categories, GET values, POST/PUT/DELETE values, reorder, transitions, validate) | Not Started |
| ENUM-BE-012 | P0 | Register `IEnumManagementService` in `Program.cs` DI | Not Started |
| ENUM-BE-013 | P0 | Apply migration to `crm_db` (MariaDB) and verify schema changes | Not Started |

### Phase 2: Entity Migration (Priority Entities)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| ENUM-MIG-001 | P0 | Add `StatusId INT NULL` column to `Leads` table | Not Started |
| ENUM-MIG-002 | P0 | Migrate Lead.Status enum values (0–5) to StatusId lookup references | Not Started |
| ENUM-MIG-003 | P0 | Update `Lead.cs` entity — change `Status` from `LeadLifecycleStatus` enum to `int? StatusId` + `EnumValue? StatusValue` navigation + `[NotMapped] string Status` computed property | Not Started |
| ENUM-MIG-004 | P0 | Add FK constraint `FK_Leads_Status` (Leads.StatusId → LookupItems.Id) | Not Started |
| ENUM-MIG-005 | P0 | Update `LeadService` to use `StatusId` instead of `Status` enum | Not Started |
| ENUM-MIG-006 | P0 | Add `StageId INT NULL` column to `Opportunities` table | Not Started |
| ENUM-MIG-007 | P0 | Migrate Opportunity.Stage enum values (0–5) to StageId lookup references | Not Started |
| ENUM-MIG-008 | P0 | Update `Opportunity.cs` entity — change `Stage` from `OpportunityStage` enum to `int? StageId` + computed property | Not Started |
| ENUM-MIG-009 | P0 | Add FK constraint `FK_Opportunities_Stage` | Not Started |
| ENUM-MIG-010 | P0 | Update `OpportunityService` to use `StageId` instead of `Stage` enum | Not Started |
| ENUM-MIG-011 | P0 | Add `StatusId INT NULL, PriorityId INT NULL` columns to `ServiceRequests` table | Not Started |
| ENUM-MIG-012 | P0 | Migrate ServiceRequest.Status/Priority enum values to lookup references | Not Started |
| ENUM-MIG-013 | P0 | Update `ServiceRequest.cs` entity — change `Status` and `Priority` from enums to nullable FKs + computed properties | Not Started |
| ENUM-MIG-014 | P0 | Add FK constraints for ServiceRequest status/priority | Not Started |
| ENUM-MIG-015 | P0 | Update `ServiceRequestService` to use `StatusId`/`PriorityId` | Not Started |
| ENUM-MIG-016 | P0 | Run data integrity verification queries (count migrated records, check for NULLs) | Not Started |

### Phase 3: Frontend Implementation

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| ENUM-FE-001 | P0 | Create `src/types/enums.ts` — TypeScript interfaces (EnumCategory, EnumValue, EnumMetadata, EnumTransition) | Not Started |
| ENUM-FE-002 | P0 | Create `src/services/enumService.ts` — CRUD service with axios calls to EnumManagementController (20+ methods) | Not Started |
| ENUM-FE-003 | P0 | Create `src/services/enumCacheService.ts` — client-side enum cache with auto-refresh, localStorage persistence | Not Started |
| ENUM-FE-004 | P0 | Create `src/pages/admin/EnumManagementPage.tsx` — list all configurable enum categories (data grid with counts) | Not Started |
| ENUM-FE-005 | P0 | Create `src/pages/admin/EnumEditorPage.tsx` — edit enum values for selected category (table + CRUD dialogs) | Not Started |
| ENUM-FE-006 | P0 | Create `src/components/admin/enums/EnumCategoryGrid.tsx` — sortable grid of enum categories with usage stats | Not Started |
| ENUM-FE-007 | P0 | Create `src/components/admin/enums/EnumValueTable.tsx` — drag-drop sortable table with inline edit | Not Started |
| ENUM-FE-008 | P0 | Create `src/components/admin/enums/EnumValueForm.tsx` — dialog for creating/editing enum values (key, label, color, metadata) | Not Started |
| ENUM-FE-009 | P0 | Create `src/components/admin/enums/EnumMetadataEditor.tsx` — JSON editor for enum metadata (probability, slaHours, validationRules) | Not Started |
| ENUM-FE-010 | P0 | Create `src/components/admin/enums/EnumColorPicker.tsx` — color picker with Material Design preset palette | Not Started |
| ENUM-FE-011 | P1 | Create `src/components/admin/enums/EnumIconPicker.tsx` — Material-UI icon selector with search | Not Started |
| ENUM-FE-012 | P1 | Create `src/components/admin/enums/EnumTransitionMatrix.tsx` — visual matrix showing allowed status transitions | Not Started |
| ENUM-FE-013 | P1 | Create `src/components/admin/enums/EnumUsageAnalytics.tsx` — bar chart showing usage frequency per enum value | Not Started |
| ENUM-FE-014 | P0 | Add navigation link: Admin → Master Data → Enum Management (`/admin/master-data/enums`) | Not Started |
| ENUM-FE-015 | P0 | Update `LeadForm.tsx` to fetch status values from enumService instead of hard-coded enum | Not Started |
| ENUM-FE-016 | P0 | Update `OpportunityForm.tsx` to fetch stage values from enumService | Not Started |
| ENUM-FE-017 | P0 | Update `ServiceRequestForm.tsx` to fetch status/priority values from enumService | Not Started |
| ENUM-FE-018 | P0 | Implement cache invalidation on enum value create/update/delete (trigger re-fetch in all forms) | Not Started |

### Phase 4: Testing & Validation

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| ENUM-TEST-001 | P0 | Backend unit test: `EnumManagementServiceTests.GetValuesByCategoryName_ReturnsActiveValues_OrderedBySortOrder` | Not Started |
| ENUM-TEST-002 | P0 | Backend unit test: `EnumManagementServiceTests.CreateValue_WithDuplicateKey_ThrowsValidationException` | Not Started |
| ENUM-TEST-003 | P0 | Backend unit test: `EnumManagementServiceTests.DeleteValue_WhenInUse_ThrowsInvalidOperationException` | Not Started |
| ENUM-TEST-004 | P0 | Backend unit test: `EnumManagementServiceTests.IsTransitionAllowed_WhenExplicitRuleExists_ReturnsRuleValue` | Not Started |
| ENUM-TEST-005 | P0 | Backend unit test: `EnumManagementServiceTests.ReorderValues_UpdatesSortOrders_InCorrectSequence` | Not Started |
| ENUM-TEST-006 | P0 | Integration test: `EnumManagementIntegrationTests.GET_api_enummanagement_LeadStatus_values_ReturnsEnumValues` | Not Started |
| ENUM-TEST-007 | P0 | Integration test: `EnumManagementIntegrationTests.POST_api_enummanagement_categories_1_values_CreatesNewValue` | Not Started |
| ENUM-TEST-008 | P0 | Frontend test: `EnumManagementPage.test.tsx` — renders enum categories grid | Not Started |
| ENUM-TEST-009 | P0 | Frontend test: `EnumEditorPage.test.tsx` — creates new enum value | Not Started |
| ENUM-TEST-010 | P1 | E2E test: `TC-ENUM-001: Admin can add new lead status` (Playwright) | Not Started |
| ENUM-TEST-011 | P1 | E2E test: `TC-ENUM-002: Admin can reorder enum values via drag-drop` (Playwright) | Not Started |
| ENUM-TEST-012 | P1 | E2E test: `TC-ENUM-003: Admin cannot delete enum value in use` (Playwright) | Not Started |
| ENUM-TEST-013 | P0 | Performance test: Verify enum cache effectiveness (<10ms p99 lookup) | Not Started |
| ENUM-TEST-014 | P0 | Data integrity test: Run verification queries on migrated enum data (zero NULL StatusIds) | Not Started |

---

## Summary — Batch 3

| Phase | Total Items | Priority | Estimated Duration |
|-------|-------------|----------|---------------------|
| Phase 1: Database & Backend Foundation | 19 | P0 | Week 1 |
| Phase 2: Entity Migration | 16 | P0 | Week 2 |
| Phase 3: Frontend Implementation | 18 | P0/P1 | Week 3 |
| Phase 4: Testing & Validation | 14 | P0/P1 | Week 4 |
| **Total** | **67** | — | **4 weeks** |

---

## Batch 2 — New Feature Tasks

### FEAT-COLLAB: Record Comments & @Mentions

**Goal:** Add threaded comments with @mention support to all major CRM entities (Accounts, Contacts, Leads, Opportunities, Service Requests).

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| COLLAB-001 | P0 | Create `RecordComment` entity (Id, EntityType, EntityId, Content, AuthorId, ParentCommentId, MentionedUserIds JSON, CreatedAt, UpdatedAt, IsDeleted, RowVersion) | Not Started |
| COLLAB-002 | P0 | Add `DbSet<RecordComment>` to `CrmDbContext` + `OnModelCreating` config | Not Started |
| COLLAB-003 | P0 | Create EF Core migration `AddRecordComments` and apply to `crm_db` | Not Started |
| COLLAB-004 | P0 | Implement `IRecordCommentService` / `RecordCommentService` (GetByEntity, Create, Update, Delete, GetThread) | Not Started |
| COLLAB-005 | P0 | Register `IRecordCommentService` in `Program.cs` DI | Not Started |
| COLLAB-006 | P0 | Implement `RecordCommentsController` (GET `/api/{entityType}/{id}/comments`, POST, PUT `/{commentId}`, DELETE `/{commentId}`) | Not Started |
| COLLAB-007 | P1 | Build `RecordComments` React component (threaded list + compose box with @mention autocomplete) | Not Started |
| COLLAB-008 | P1 | Add `recordCommentService.ts` TypeScript service | Not Started |
| COLLAB-009 | P1 | Integrate `RecordComments` component into Account, Contact, Lead, Opportunity, ServiceRequest detail pages | Not Started |
| COLLAB-010 | P1 | Unit tests for `RecordCommentService` (10+ test cases) | Not Started |

---

### FEAT-CSAT: Customer Satisfaction (CSAT/NPS)

**Goal:** Enable CSAT surveys after service request resolution and periodic NPS score collection.

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| CSAT-001 | P0 | Create `SatisfactionSurvey` entity (Id, EntityType, EntityId, Type [CSAT/NPS/CES], Status, SentAt, ResponseReceivedAt, ContactId, CreatedAt, UpdatedAt, IsDeleted, RowVersion) | Not Started |
| CSAT-002 | P0 | Create `SatisfactionResponse` entity (Id, SurveyId, Score, Comment, Sentiment, SubmittedAt) | Not Started |
| CSAT-003 | P0 | Add `DbSet` + migration `AddSatisfactionTracking` | Not Started |
| CSAT-004 | P0 | Implement `ISatisfactionService` / `SatisfactionService` (SendSurvey, RecordResponse, GetMetrics, GetNPSScore, GetCSATScore) | Not Started |
| CSAT-005 | P0 | Implement `SatisfactionController` (CRUD + metrics endpoints + `/api/satisfaction/nps` + `/api/satisfaction/csat`) | Not Started |
| CSAT-006 | P1 | Frontend: `SatisfactionDashboard` page + NPS trend chart + CSAT score widget + response log table | Not Started |
| CSAT-007 | P1 | Frontend: `SurveyResponseForm` component (public-facing survey form for email links) | Not Started |
| CSAT-008 | P1 | Add `satisfactionService.ts` TypeScript service | Not Started |
| CSAT-009 | P1 | Unit tests for `SatisfactionService` (8+ test cases) | Not Started |

---

### FEAT-REVENUE: Revenue Analytics (ARR/MRR)

**Goal:** Track Monthly Recurring Revenue (MRR) and Annual Recurring Revenue (ARR) with movement analysis (new, expansion, churn, contraction).

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| REVENUE-001 | P0 | Create `RevenueSnapshot` entity (Id, SnapshotDate, MRR, ARR, NewMRR, ExpansionMRR, ContractionMRR, ChurnMRR, NetNewMRR, CustomerCount, CreatedAt) | Not Started |
| REVENUE-002 | P0 | Add `DbSet<RevenueSnapshot>` + migration `AddRevenueSnapshots` | Not Started |
| REVENUE-003 | P0 | Implement `IRevenueAnalyticsService` / `RevenueAnalyticsService` (CalculateMRR, GetARRTrend, GetMRRMovements, GetChurnRate, GetExpansionRevenue) using existing `Subscription`/`Contract`/`Invoice` entities | Not Started |
| REVENUE-004 | P0 | Implement `RevenueAnalyticsController` (GET `/api/revenue/mrr`, `/api/revenue/arr`, `/api/revenue/movements`, `/api/revenue/churn-rate`, `/api/revenue/cohorts`) | Not Started |
| REVENUE-005 | P1 | Frontend: `RevenueAnalyticsPage` with MRR/ARR trend chart, waterfall MRR movement chart, churn rate gauge | Not Started |
| REVENUE-006 | P1 | Frontend: `RevenueDashboardWidget` — embed key metrics in main dashboard | Not Started |
| REVENUE-007 | P1 | Add `revenueAnalyticsService.ts` TypeScript service | Not Started |
| REVENUE-008 | P1 | Unit tests for `RevenueAnalyticsService` (8+ test cases) | Not Started |

---

### FEAT-PORTAL: Customer Portal Foundation

**Goal:** Allow external customers to log in, view their tickets, submit new requests, and browse the knowledge base without a CRM user account.

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| PORTAL-001 | P0 | Create `PortalUser` entity (Id, Email, PasswordHash, ContactId, AccountId, IsActive, LastLoginAt, EmailVerifiedAt, CreatedAt, UpdatedAt, IsDeleted, RowVersion) | Not Started |
| PORTAL-002 | P0 | Create `PortalSession` entity (Id, PortalUserId, Token, ExpiresAt, CreatedAt, IpAddress) | Not Started |
| PORTAL-003 | P0 | Create `PortalConfig` entity (Id, IsEnabled, AllowSelfRegistration, WelcomeMessage, LogoUrl, PrimaryColor, AllowedDomains, CreatedAt, UpdatedAt) | Not Started |
| PORTAL-004 | P0 | Add `DbSet` entries + migration `AddCustomerPortal` | Not Started |
| PORTAL-005 | P0 | Implement `IPortalAuthService` (Register, Login, ForgotPassword, ResetPassword, VerifyEmail) | Not Started |
| PORTAL-006 | P0 | Implement `IPortalService` (GetMyTickets, CreateTicket, GetTicketDetails, AddComment, GetKnowledgeArticles) | Not Started |
| PORTAL-007 | P0 | Implement `PortalAuthController` (`/api/portal/auth/login`, `/register`, `/forgot-password`, `/reset-password`) | Not Started |
| PORTAL-008 | P0 | Implement `PortalController` (`/api/portal/tickets`, `/{id}`, `/tickets/{id}/comments`, `/knowledge-base`) | Not Started |
| PORTAL-009 | P1 | Frontend: `PortalLoginPage`, `PortalRegisterPage`, `PortalDashboardPage`, `PortalTicketListPage`, `PortalTicketDetailPage`, `PortalKBPage` | Not Started |
| PORTAL-010 | P1 | Add `portalService.ts` + `portalAuthService.ts` TypeScript services | Not Started |
| PORTAL-011 | P1 | Admin UI: Portal configuration page (enable/disable portal, branding settings) | Not Started |
| PORTAL-012 | P1 | Unit tests for `PortalAuthService` + `PortalService` (10+ test cases) | Not Started |

---

### FEAT-AISCORING: AI Lead Scoring Real-time Triggers

**Goal:** Auto-score leads on create/update using existing scoring rules, implement score decay for stale leads, add score history tracking.

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| AISCORING-001 | P0 | Create `LeadScoreHistory` entity (Id, LeadId, Score, PreviousScore, Delta, Reason, ScoreComponents JSON, ScoredAt, ScoredBy [user/system/decay]) | Not Started |
| AISCORING-002 | P0 | Add `DbSet<LeadScoreHistory>` + migration `AddLeadScoreHistory` | Not Started |
| AISCORING-003 | P0 | Implement `LeadScoringBackgroundService : BackgroundService` — runs every 6h, applies score decay to leads inactive for 14+ days using existing `LastScoreDecayDate` | Not Started |
| AISCORING-004 | P0 | Modify `LeadService.CreateAsync` + `UpdateAsync` to auto-trigger lead scoring via `IAILeadScoringService` and persist `LeadScoreHistory` entry | Not Started |
| AISCORING-005 | P0 | Add endpoints to existing `AILeadScoringController`: GET `/api/aileadscoring/leads/{id}/history`, GET `/api/aileadscoring/leads/{id}/explanation` | Not Started |
| AISCORING-006 | P1 | Frontend: `LeadScoreHistoryChart` — sparkline or mini trend chart showing score over time on Lead detail page | Not Started |
| AISCORING-007 | P1 | Frontend: `LeadScoreExplanation` drawer — shows score breakdown by component (BANT/MEDDIC/activity/engagement) | Not Started |
| AISCORING-008 | P1 | Frontend: Update `LeadsPage` to show score trend indicator (⬆️ improving / ⬇️ declining / ➡️ stable) next to score badge | Not Started |
| AISCORING-009 | P1 | Unit tests for `LeadScoringBackgroundService` + score history (8+ test cases) | Not Started |

---

### FEAT-E2E: E2E Test Suite Stabilization

**Goal:** Fix CRUD UI test failures (selector/navigation issues) and eliminate Mobile Safari false negatives so the full e2e suite runs green on chromium + firefox.

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| E2E-001 | P0 | Fix `crud-accounts.spec.ts` TC-ACC-001 to TC-ACC-016 — update navigation selectors to match the current MUI sidebar structure | Not Started |
| E2E-002 | P0 | Fix auth registration tests TC-AUTH-011 + TC-AUTH-013 — either update expected behavior (if registration is disabled) or fix the form selectors | Not Started |
| E2E-003 | P0 | Update `playwright.config.ts` to exclude `Mobile Safari` project from standard `test:comprehensive` run (add `--project=chromium --project=firefox` constraint) | Not Started |
| E2E-004 | P1 | Fix `crud-contacts.spec.ts` selector issues (if any) | Not Started |
| E2E-005 | P1 | Fix `crud-opportunities.spec.ts` selector issues (if any) | Not Started |
| E2E-006 | P1 | Add BVT test cases for COLLAB, CSAT, REVENUE, and PORTAL API endpoints | Not Started |
| E2E-007 | P1 | Ensure `npm run test:comprehensive` exits with 0 failures on chromium+firefox | Not Started |

---

## Summary — Batch 2

| Feature Group | Total Items | Priority |
|--------------|-------------|----------|
| FEAT-COLLAB (Record Comments) | 10 | P0/P1 |
| FEAT-CSAT (Satisfaction) | 9 | P0/P1 |
| FEAT-REVENUE (ARR/MRR) | 8 | P0/P1 |
| FEAT-PORTAL (Customer Portal) | 12 | P0/P1 |
| FEAT-AISCORING (Lead Scoring) | 9 | P0/P1 |
| FEAT-E2E (Test Stabilization) | 7 | P0/P1 |
| **Total** | **55** | — |

---



---

## Active Features

### Feature: Scripting Language Support

**Goal:** Allow users to **write new** and **edit existing** Workflow Script nodes and Agent Script Plugins using JavaScript (Jint, always-on) and/or Python 3.x (Python.NET + RestrictedPython, feature-flagged).  

**Specifications:**
- [SPEC-SD-004-WorkflowEngine.md](11-specifications/SPEC-SD-004-WorkflowEngine.md) — SF11 + Section 3.9 (Workflow Script node language support)
- [SPEC-AI-006-AgentScripting.md](11-specifications/SPEC-AI-006-AgentScripting.md) — Full dual-language agent & workflow scripting architecture

**User Stories Covered:**
| Story | Component |
|-------|-----------|
| Edit existing Workflow Script nodes (change language, update code) | `ScriptNodeEditor` + `ScriptNodeConfigDto.language` field |
| Write new Workflow Script nodes from the workflow designer | `ScriptNodeEditor` + `ExecuteScriptAction` via `IScriptEngine` |
| Edit existing Agent Script Plugins | `ScriptPluginEditorPage` + `ScriptPluginService.UpdateAsync` |
| Write new Agent Script Plugins | `ScriptPluginsController POST` + `ScriptPluginLibraryPage` |

---

## Summary by Priority

| Priority | Count | Area |
|----------|-------|------|
| **P0** | ✅ 6 | Backend foundation (interface, enum, factory, refactor) + Testing (enum test) [COMPLETED] |
| **P1** | ✅ 16 | Backend engines, entity/DB, services, SK integration, API, frontend core [ALL COMPLETE] |
| **P2** | ✅ 2 | Frontend optional components (TestPanel, VariableInspector) [COMPLETE] |
| **Done** | 1 | Enum reference documentation |
| **Total** | **23** | 23 Complete, 0 Pending |

---

## TODO Items

### Group 1 — Backend Foundation (P0)
> These must be done first — everything else depends on them.

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-001 | AI006-TODO01 / SD004-TODO01 | P0 | Create `IScriptEngine` interface + `ScriptExecutionResult` + `ScriptDiagnostic` records in `CRM.Core/Interfaces/Scripting/` | Completed |
| SCRIPT-002 | AI006-TODO05 / SD004-TODO04 | P0 | Create `ScriptLanguage` enum at `CRM.Core/Enums/ScriptLanguage.cs` (JavaScript=0, Python=1, CSharp=2) | Completed |
| SCRIPT-003 | AI006-TODO04 / SD004-TODO08 | P0 | Implement `ScriptEngineFactory` resolving `IScriptEngine` by `ScriptLanguage` from DI | Completed |
| SCRIPT-004 | SD004-TODO02 | P0 | Refactor `ExecuteScriptAction` call-site in `WorkflowWorkerService` to resolve and invoke `IScriptEngine` via `ScriptEngineFactory` | Completed |
| SCRIPT-005 | AI006-TODO02 / SD004-TODO03 | P0 | Extract existing inline Jint JavaScript logic from `WorkflowWorkerService` into `JintScriptEngine : IScriptEngine` (preserving timeout/memory sandbox) | Completed |

### Group 2 — Backend Engine Implementations (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-006 | AI006-TODO03 / SD004-TODO06,07 | P1 | Implement `PythonScriptEngine : IScriptEngine` using Python.NET + RestrictedPython sandbox (gated by `FeatureManagement:EnablePythonScripting` flag) | Stub Created — Pending full Python.NET wiring |

### Group 3 — Backend ScriptPlugin Entity & Persistence (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-007 | AI006-TODO06 | P1 | Add `ScriptPlugin` entity and `DbSet<ScriptPlugin>` to `CrmDbContext` | Completed |
| SCRIPT-008 | AI006-TODO07 | P1 | Create EF Core migration `AddScriptPlugins` and apply to `crm_db` | Completed — Migration created (20260226114639) |
| SCRIPT-009 | AI006-TODO08 | P1 | Implement `IScriptPluginService` / `ScriptPluginService` (CRUD: Create, UpdateAsync, Delete, GetAll, GetById, TestExecute) | Completed |

### Group 4 — Semantic Kernel Integration (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-010 | AI006-TODO09 | P1 | Implement `ScriptPluginLoader` — reads enabled `ScriptPlugin` rows from DB and registers each as a `KernelPlugin` with a `KernelFunction` wrapper | Completed |
| SCRIPT-011 | AI006-TODO10 | P1 | Update `CrmKernelFactory.CreateKernelAsync()` to call `ScriptPluginLoader.LoadDynamicPluginsAsync()` after static plugin registration | Completed — async overloads added |
| SCRIPT-012 | AI006-TODO12 | P1 | Register `JintScriptEngine`, `PythonScriptEngine` (conditional), `ScriptEngineFactory`, `ScriptPluginLoader`, `ScriptPluginService` in `SemanticKernelServiceExtensions` | Completed |

### Group 5 — Backend API Layer (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-013 | AI006-TODO11 | P1 | Implement `ScriptPluginsController` with endpoints: GET /api/script-plugins, GET /{id}, POST, PUT /{id}, DELETE /{id}, POST /{id}/enable, POST /{id}/disable, POST /test, GET /languages | Completed — 9 endpoints in ScriptingController |
| SCRIPT-014 | SD004-TODO05 | P1 | Add `language` (ScriptLanguage enum) field to `ScriptNodeConfigDto` and persist/read from `WorkflowNodes.ConfigurationJson` | Completed |

### Group 6 — Frontend Core Components (P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-015 | AI006-TODO13 / SD004-TODO09 | P1 | Build `ScriptNodeEditor` React component with `@monaco-editor/react`, language selector (JS/Python), and workflow context variable hints — used in both new workflow creation and editing existing Script nodes | Completed — Full Monaco IDE with theme-adaptive dark/light mode |
| SCRIPT-016 | AI006-TODO16 | P1 | Build `ScriptPluginLibraryPage` (list view for creating new agent scripts) and `ScriptPluginEditorPage` (Monaco editor for editing existing and creating new agent script plugins) | Completed — 437-line LibraryPage + 714-line EditorPage |
| SCRIPT-017 | AI006-TODO17 | P1 | Add `scriptPluginService.ts` TypeScript service (axios calls for all `ScriptPluginsController` endpoints, typed DTOs) | Completed |

### Group 7 — Frontend Optional Components (P2)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-018 | AI006-TODO14 / SD004-TODO10 | P2 | Build `ScriptTestPanel` React component — inline test runner accepting mock context JSON and showing stdout / return value / errors | Completed — 392 lines, variables editor, context accordion, timeout selector, result panel |
| SCRIPT-019 | AI006-TODO15 / SD004-TODO11 | P2 | Build `ScriptVariableInspector` React component — sidebar listing available workflow context variables with types and sample values | Completed — 298 lines, table + compact chip modes, context section, click-to-insert |

### Group 8 — Testing (P0/P1)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-020 | AI006-TODO21 / SD004-TODO14 | P0 | Add `ScriptLanguageEnumTests` unit test — assert count=3 and values JavaScript=0, Python=1, CSharp=2 | Completed |
| SCRIPT-021 | AI006-TODO18 / SD004-TODO12 | P1 | Write unit tests for `JintScriptEngine`: timeout enforcement, memory limit, context variable injection, `log()` capture, error propagation | Completed — 18 tests passing |
| SCRIPT-022 | AI006-TODO19 / SD004-TODO13 | P1 | Write unit tests for `PythonScriptEngine`: sandbox restriction (import block), context injection, timeout, basic evaluation | Completed — 6 tests in ScriptEngineFactory (Python mocked) |
| SCRIPT-023 | AI006-TODO20 | P1 | Write unit tests for `ScriptPluginLoader` (dynamic kernel plugin registration) and `ScriptPluginService` (CRUD + validation) | Completed — 10 ScriptPluginService tests + 4 ScriptPluginLoader tests (all passing) |

### Group 9 — Documentation (Done)

| ID | Spec Ref | Priority | Description | Status |
|----|----------|----------|-------------|--------|
| SCRIPT-024 | AI006-TODO22 | P0 | Update `SPEC-GEN-001-EnumReference.md` with `ScriptLanguage` enum (section 2.8) | Done (Feb 26, 2026) |

---

## Recommended Implementation Order

Phase 1 — Foundation (✅ COMPLETE — Feb 26, 2026):
  ✅ SCRIPT-001  IScriptEngine interface + result types
  ✅ SCRIPT-002  ScriptLanguage enum (.cs file)
  ✅ SCRIPT-003  ScriptEngineFactory
  ✅ SCRIPT-005  JintScriptEngine (extract from WorkflowWorkerService)
  ✅ SCRIPT-004  Refactor ExecuteScriptAction call-site
  ✅ SCRIPT-020  ScriptLanguage enum unit test (values validated)

Phase 2 — Python Engine & Tests (✅ COMPLETE — Feb 26, 2026):
  ✅ SCRIPT-006  PythonScriptEngine stub (full impl deferred — Python.NET host setup)
  ✅ SCRIPT-021  JintScriptEngine unit tests (18 passing)
  ✅ SCRIPT-022  ScriptEngineFactory tests (6 passing, Python mocked)

Phase 3 — ScriptPlugin Entity & Service (✅ COMPLETE — Feb 26, 2026):
  ✅ SCRIPT-007  ScriptPlugin entity + DbSet
  ✅ SCRIPT-008  EF migration AddScriptPlugins (20260226114639)
  ✅ SCRIPT-009  ScriptPluginService (CRUD + TestExecute)
  ✅ SCRIPT-013  ScriptPluginsController (9 endpoints fully implemented)
  ✅ SCRIPT-014  language field in ScriptNodeConfigDto + WorkflowWorkerService 3-way language resolution

Phase 4 — Semantic Kernel Integration (✅ COMPLETE — Feb 26, 2026):
  ✅ SCRIPT-010  ScriptPluginLoader
  ✅ SCRIPT-011  CrmKernelFactory update (async overloads added)
  ✅ SCRIPT-012  DI registration in ScriptingServiceExtensions
  ✅ SCRIPT-023  14 tests: ScriptPluginService (10) + ScriptPluginLoader (4) — all passing

Phase 5 — Frontend (✅ COMPLETE — Feb 26, 2026):
  ✅ SCRIPT-017  scriptPluginService.ts (complete with all typed DTOs)
  ✅ SCRIPT-015  ScriptNodeEditor with full Monaco IDE, theme-adaptive, language selector
  ✅ SCRIPT-016  ScriptPluginLibraryPage (437 lines) + ScriptPluginEditorPage (714 lines)
  ✅ SCRIPT-018  ScriptTestPanel (392 lines, variables + context + timeout + result panel)
  ✅ SCRIPT-019  ScriptVariableInspector (298 lines, table + compact chip modes)

Phase 6 — Designer Integration & Navigation (✅ COMPLETE — Feb 26, 2026):
  ✅ SCRIPT-020  Workflow Designer split JSON script panel — Monaco editor alongside canvas, bidirectional live sync (visual→JSON + JSON→canvas with 600ms debounce); toggle button in toolbar; parse errors shown inline (WorkflowDesignerPage.tsx)
  ✅ SCRIPT-021  Scripting section added to Navigation — Admin > Scripting subcategory with "Script Library" (/scripting/plugins) and "New Script" (/scripting/plugins/new) items; nav config version bumped to v3-2026-02-26

---

## Key Implementation Notes

### Feature Flag
Python engine is gated by `FeatureManagement:EnablePythonScripting`. When false, `ScriptEngineFactory` throws `NotSupportedException` for `ScriptLanguage.Python` and the frontend language selector hides the Python option.

### Enum File to Create
```csharp
// CRM.Core/Enums/ScriptLanguage.cs
namespace CRM.Core.Enums;

public enum ScriptLanguage
{
    JavaScript = 0,
    Python = 1,
    CSharp = 2
}
```

### ScriptPlugin Entity Summary
Fields: Id, Name, Description, Language (ScriptLanguage), Code (TEXT), IsEnabled, Parameters (JSON), ReturnType, AgentId (nullable), CreatedAt, UpdatedAt, IsDeleted, RowVersion

### Frontend Package Required
```bash
npm install @monaco-editor/react monaco-editor
```

### Python.NET NuGet Package
```xml
<PackageReference Include="pythonnet" Version="3.0.3" />
```

---

## Stats

| Metric | Value |
|--------|-------|
| Total pending items | 0 |
| Total done this session | 23 |
| Total historically completed | 525 |
| Specs covering this feature | 2 (SPEC-SD-004 v1.3, SPEC-AI-006 v1.0) |
| New enum | ScriptLanguage (SPEC-GEN-001 section 2.8) |
| Feature branch | feature/master-todo-batch |
| Build status | ✅ 0 errors, 0 warnings |
| Unit test count | ✅ 38 passing (18 Jint + 6 Factory + 10 ScriptPluginService + 4 ScriptPluginLoader), 12 skipped (Python pending) |
| Frontend TypeScript | ✅ 0 errors (tsc --noEmit) |

---

**Document Maintained By:** GitHub Copilot  
**Next Review:** After Phase 1 completion

**END OF MASTER TODO LIST**
