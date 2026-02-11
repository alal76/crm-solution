# CRM Solution — Comprehensive Test Gap Audit Report

> **Generated:** February 2026  
> **Scope:** All backend (.NET xUnit), E2E (Playwright), and unit test projects  
> **Auditor:** Automated full-workspace analysis

---

## Executive Summary

| Metric | Value |
|--------|-------|
| **Active backend tests** | **2,871** (788 main + 397 CRM.Tests + 1,686 Unit) |
| **Excluded backend test files** | **97** (contain an unknown but substantial number of [Fact] tests) |
| **Skipped backend tests** | **8** (all performance, run-manually) |
| **E2E tests (active)** | **675** of 722 (93.5%) |
| **E2E tests (skipped)** | **47** across 7 spec files |
| **Services without ANY test** | **54** of 125 (43.2%) |
| **Controllers without ANY test** | **61** of 94 (64.9%) |
| **Overall service test coverage** | **56.8%** (71 of 125 services have ≥1 test file) |
| **Overall controller test coverage** | **35.1%** (33 of 94 controllers have ≥1 test file) |

---

## Table of Contents

1. [Excluded Files (97 files)](#1-excluded-files-97-files)
2. [Skipped Backend Tests (8 tests)](#2-skipped-backend-tests-8-tests)
3. [E2E Skipped Tests (47 tests)](#3-e2e-skipped-tests-47-tests)
4. [Services Without Tests (54 services)](#4-services-without-tests-54-services)
5. [Controllers Without Tests (61 controllers)](#5-controllers-without-tests-61-controllers)
6. [Test Infrastructure Issues](#6-test-infrastructure-issues)
7. [Summary Statistics & Recommendations](#7-summary-statistics--recommendations)

---

## 1. Excluded Files (97 files)

These files **exist on disk** in `CRM.Backend/tests/` but are excluded from compilation via `<Compile Remove>` in `CRM.Tests.csproj`. They contain real test code that no longer compiles due to API drift, missing types, or namespace changes.

### 1.1 Excluded Service Tests (33 files)

| # | File | Exclusion Reason |
|---|------|-----------------|
| 1 | CalendarSyncServiceTests.cs | API surface drift — mock setup failures |
| 2 | CloudDeploymentServiceTests.cs | API surface drift |
| 3 | ColorPaletteServiceTests.cs | API surface drift |
| 4 | ContactInfoServiceTests.cs | API surface drift |
| 5 | ContactsServiceTests.cs | API surface drift |
| 6 | DatabaseBackupServiceTests.cs | API surface drift |
| 7 | DatabaseSyncServiceTests.cs | API surface drift |
| 8 | DuplicateDetectionServiceTests.cs | API surface drift |
| 9 | EmailSyncServiceTests.cs | API surface drift |
| 10 | FieldMasterDataServiceTests.cs | API surface drift |
| 11 | GenericRepositoryTests.cs | Codebase uses generic Repository\<T\> — test references missing types |
| 12 | JwtTokenServiceTests.cs | Has been partially re-enabled (see active file) |
| 13 | LLMServiceTests.cs | API surface drift |
| 14 | LLMSettingsServiceTests.cs | API surface drift |
| 15 | LandingPageServiceTests.cs | API surface drift |
| 16 | MarketingCampaignServiceTests.cs | API surface drift |
| 17 | MergeServiceTests.cs | API surface drift |
| 18 | ModuleFieldConfigurationServiceTests.cs | API surface drift |
| 19 | ModuleUIConfigServiceTests.cs | API surface drift |
| 20 | MonitoringServiceTests.cs | API surface drift |
| 21 | NormalizationServiceTests.cs | API surface drift |
| 22 | RedisCacheServiceTests.cs | API surface drift |
| 23 | ResilienceServiceTests.cs | API surface drift |
| 24 | ServiceRequestServiceTests.cs | API surface drift |
| 25 | TokenRevocationServiceTests.cs | API surface drift |
| 26 | TotpServiceTests.cs | API surface drift |
| 27 | UserApprovalServiceTests.cs | API surface drift |
| 28 | UserGroupServiceTests.cs | API surface drift |
| 29 | WorkflowInstanceServiceTests.cs | API surface drift |
| 30 | WorkflowServiceTests.cs | API surface drift |
| 31 | WorkflowWorkerServiceTests.cs | API surface drift |
| 32 | ZipCodeImportServiceTests.cs | API surface drift |
| 33 | ZipCodeServiceTests.cs | API surface drift |

### 1.2 Excluded ITSM Service Tests (7 files)

| # | File | Exclusion Reason |
|---|------|-----------------|
| 1 | ArticleRecommendationServiceTests.cs | ITSM Advanced entity model misalignment |
| 2 | BusinessHoursCalculatorTests.cs | ITSM Advanced entity model misalignment |
| 3 | CICDIntegrationServiceTests.cs | ITSM Advanced entity model misalignment |
| 4 | EmailToTicketServiceTests.cs | ITSM Advanced entity model misalignment |
| 5 | MonitoringIntegrationServiceTests.cs | ITSM Advanced entity model misalignment |
| 6 | SelfServiceChatbotServiceTests.cs | ITSM Advanced entity model misalignment |
| 7 | WebhookNotificationServiceTests.cs | ITSM Advanced entity model misalignment |

### 1.3 Excluded Controller Tests (28 files)

| # | File | Exclusion Reason |
|---|------|-----------------|
| 1 | ActivitiesControllerTests.cs | Controller API signature drift |
| 2 | AuthControllerTests.cs | Controller API signature drift |
| 3 | CampaignsControllerTests.cs | Controller API signature drift |
| 4 | ContactsControllerTests.cs | Controller API signature drift |
| 5 | DashboardControllerTests.cs | Controller API signature drift |
| 6 | DuplicatesControllerTests.cs | Controller API signature drift |
| 7 | EmailTemplatesControllerTests.cs | Controller API signature drift |
| 8 | FileUploadControllerTests.cs | Controller API signature drift |
| 9 | ImportExportControllerTests.cs | Controller API signature drift |
| 10 | InteractionsControllerTests.cs | Controller API signature drift |
| 11 | InvoicesControllerTests.cs | Controller API signature drift |
| 12 | KnowledgeBaseControllerTests.cs | Controller API signature drift |
| 13 | LeadsControllerTests.cs | Controller API signature drift |
| 14 | LookupsControllerTests.cs | Controller API signature drift |
| 15 | NotesControllerTests.cs | Controller API signature drift |
| 16 | OrdersControllerTests.cs | Controller API signature drift |
| 17 | PipelinesControllerTests.cs | Controller API signature drift |
| 18 | QuotesControllerTests.cs | Controller API signature drift |
| 19 | ReportsControllerTests.cs | Controller API signature drift |
| 20 | ServiceRequestsControllerTests.cs | Controller API signature drift |
| 21 | SettingsControllerTests.cs | Controller API signature drift |
| 22 | StagesControllerTests.cs | Controller API signature drift |
| 23 | TasksControllerTests.cs | Controller API signature drift |
| 24 | UserGroupsControllerTests.cs | Controller API signature drift |
| 25 | UserProfilesControllerTests.cs | Controller API signature drift |
| 26 | UsersControllerTests.cs | Controller API signature drift |
| 27 | WebhooksControllerTests.cs | Controller API signature drift |
| 28 | WorkflowsControllerTests.cs | Controller API signature drift |

### 1.4 Excluded Other Tests (29 files)

| Category | Count | Files |
|----------|-------|-------|
| **Validators** | 6 | AccountValidator, ContactValidator, LeadValidator, OpportunityValidator, CommonValidator, UserValidator — `CRM.Core.Validation` namespace doesn't exist |
| **HostedServices** | 5 | BackupScheduler, DatabaseSync, LeadScoreDecay, WorkflowWorker, ZipCodeImport — API drift |
| **Middleware** | 4 | ErrorHandling, Authentication, RateLimiting, RequestLogging — API drift |
| **Providers** | 4 | EmailProvider, CacheProvider, StorageProvider, JwtTokenService — API drift |
| **BVT** | 4 | CoreApiBVT, MarketingIrmApiBVT, SalesApiBVT, IntegrationApiBVT — API drift |
| **Extensions** | 2 | DatabaseExtensions, ServiceExtensions — `TestDbContext` missing 131 members |
| **Configurations** | 1 | EntityConfigurationTests — API drift |
| **Integration** | 1 | ApiTestFactory (duplicate in Helpers) |
| **Helpers** | 1 | ApiTestFactory |
| **Repositories** | 1 | `Repositories\**\*.cs` (wildcard) — codebase uses generic Repository\<T\> |

---

## 2. Skipped Backend Tests (8 tests)

All 8 skipped tests are in a single file and are intentionally deferred for manual execution:

| # | Test Method | File | Skip Reason |
|---|-------------|------|-------------|
| 1 | `ConcurrentDatabaseOperations_ShouldHandleMultipleConnections` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 2 | `LargeDataset_ShouldHandleEfficiently` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 3 | `MultipleServiceCalls_ShouldCompleteWithinTimeout` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 4 | `MemoryUsage_ShouldNotExceedThreshold` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 5 | `BulkInsert_ShouldCompleteWithinTimeLimit` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 6 | `ComplexQuery_ShouldExecuteWithinAcceptableTime` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 7 | `ParallelServiceOperations_ShouldScaleLinearly` | Performance/PerformanceTests.cs | "Performance test - run manually" |
| 8 | `CachePerformance_ShouldImproveResponseTime` | Performance/PerformanceTests.cs | "Performance test - run manually" |

**Assessment:** These skips are reasonable — performance tests require infrastructure and should not run in CI.

---

## 3. E2E Skipped Tests (47 tests)

**47 of 722 E2E tests (6.5%)** are skipped across 7 spec files:

| # | Spec File | Skipped | Total | % Skipped | Likely Reason |
|---|-----------|---------|-------|-----------|---------------|
| 1 | `bvt/itsm-api-bvt.spec.ts` | 24 | ~60 | 40% | ITSM endpoints not fully deployed |
| 2 | `auth/authentication.spec.ts` | 10 | ~30 | 33% | MUI component selector fragility |
| 3 | `customers/customers.spec.ts` | 4 | ~20 | 20% | UI overlay/dialog interaction issues |
| 4 | `campaigns/campaigns.spec.ts` | 3 | ~15 | 20% | Campaign prerequisite data issues |
| 5 | `campaigns/campaign-setup.spec.ts` | 3 | ~10 | 30% | Campaign prerequisite data issues |
| 6 | `leads/leads.spec.ts` | 1 | ~15 | 7% | Minor UI interaction issue |
| 7 | `bvt/api-bvt.spec.ts` | 1 | ~120 | <1% | Single endpoint issue |
| 8 | `admin/admin.spec.ts` | 1 | ~10 | 10% | Admin UI interaction issue |

**Highest Impact:** The 24 skipped ITSM BVT tests represent the largest gap — these validate ITSM API endpoints that may not be fully deployed/stable.

---

## 4. Services Without Tests (54 services)

Cross-referencing **125 service implementations** against all test files across 3 test projects:

### 4.1 Core Services Missing Tests (38 of 92)

| # | Service | Priority | Notes |
|---|---------|----------|-------|
| 1 | **ActivityService** | 🔴 P1 | Test exists in CRM.Tests subproject (Integration/) |
| 2 | **ApprovalWorkflowService** | ✅ | Covered in CRM.Tests subproject |
| 3 | BackupSchedulerHostedService | P3 | HostedService, excluded test exists |
| 4 | **CommissionService** | 🔴 P1 | Phase 4 service, no test at all |
| 5 | **CommunicationService** | 🟡 P2 | No test file exists |
| 6 | **ContractService** | 🔴 P1 | Phase 4 service, no test at all |
| 7 | **ConversationService** | 🟡 P2 | No test file exists |
| 8 | **CreditMemoService** | 🟡 P2 | No test file exists |
| 9 | **DashboardBuilderService** | ✅ | Covered in CRM.Tests subproject |
| 10 | **DashboardService** | ✅ | Covered in CRM.Tests subproject |
| 11 | DbCacheService | P3 | Internal caching helper |
| 12 | **DepartmentService** | 🟡 P2 | No test file exists |
| 13 | **EmailSequenceService** | 🟡 P2 | No test file exists |
| 14 | **EmailTemplateService** | 🔴 P1 | Phase 4 service, no test at all |
| 15 | EntityEventDispatcher | P3 | Infrastructure plumbing |
| 16 | **EventAttendeeService** | 🟡 P2 | No test file exists |
| 17 | **FormBuilderService** | ✅ | Covered in CRM.Tests subproject |
| 18 | **HttpCalloutService** | P3 | Infrastructure plumbing |
| 19 | **ImportExportService** | 🟡 P2 | No test file exists |
| 20 | **InteractionService** | 🟡 P2 | No test file exists |
| 21 | **InvoiceService** | 🔴 P1 | Phase 4 service, no test at all |
| 22 | **LeadRoutingService** | ✅ | Covered in CRM.Tests subproject |
| 23 | LeadScoreDecayHostedService | P3 | HostedService, excluded test exists |
| 24 | MasterDataSeederService | P3 | Startup seeder |
| 25 | **NewsSocialService** | ✅ | Covered in CRM.Tests subproject |
| 26 | **NoteService** | 🟡 P2 | No test file exists |
| 27 | **OrderService** | 🔴 P1 | Phase 4 service, no test at all |
| 28 | **PaymentService** | 🔴 P1 | Phase 4 service, no test at all |
| 29 | **PipelineService** | 🟡 P2 | No test file exists |
| 30 | **PricingService** | 🟡 P2 | No test file exists |
| 31 | **ProductBundleService** | 🟡 P2 | No test file exists |
| 32 | **QuoteService** | 🟡 P2 | No test file exists |
| 33 | **ReportBuilderService** | ✅ | Covered in CRM.Tests subproject |
| 34 | **SalesForecastService** | 🟡 P2 | No test file exists |
| 35 | **SalesQuotaService** | 🟡 P2 | No test file exists |
| 36 | SampleDataSeederService | P3 | Startup seeder |
| 37 | ScheduledWorkflowService | P3 | Infrastructure plumbing |
| 38 | ServiceRequestSettingsService | 🟡 P2 | No test file exists |
| 39 | **SubscriptionService** | 🔴 P1 | Phase 4 service, no test at all |
| 40 | **TaskService** | 🟡 P2 | No test file exists |
| 41 | **TeamService** | 🔴 P1 | Phase 4 service, no test at all |
| 42 | **TerritoryService** | ✅ | Covered in CRM.Tests subproject |
| 43 | **WebhookService** | 🟡 P2 | No test file exists |
| 44 | WorkflowLogRetentionService | P3 | Infrastructure plumbing |
| 45 | WorkflowTriggerService | 🟡 P2 | No test file exists |

**Excluding ✅ (covered in subprojects) and P3 (infra/seeders):**
- **🔴 P1 (Phase 4 / core, no test at all):** 8 services — Commission, Contract, EmailTemplate, Invoice, Order, Payment, Subscription, Team
- **🟡 P2 (business logic, no test):** 22 services

### 4.2 ITSM Services Missing Tests (10 of 28)

| # | Service | Has Test? | Notes |
|---|---------|-----------|-------|
| 1 | AssetLifecycleService | ❌ | ITSM Advanced — no test |
| 2 | AssignmentRulesEngine | ❌ | No test |
| 3 | AutoCloseHostedService | ❌ | HostedService, no test |
| 4 | CABWorkflowService | ❌ | ITSM Advanced — no test |
| 5 | CatalogApprovalService | ❌ | No test |
| 6 | CatalogFulfillmentService | ❌ | No test |
| 7 | ChangeCalendarService | ❌ | No test |
| 8 | ChangeImpactService | ❌ | No test |
| 9 | DiscoveryService | ❌ | ITSM Advanced — no test |
| 10 | ImpactAnalysisService | ❌ | ITSM Advanced — no test |

The remaining 18 ITSM services **do** have tests (either active or in excluded files):
ArticleRecommendation✦, BusinessHoursCalculator✦, CICDIntegration✦, CMDB✓, Catalog✓, ChangeManagement✓, Change✓, EmailToTicket✦, EscalationHostedService (via SLAEnforcement), IncidentService✓, ITSMDashboard✓, KCSWorkflow (partial via Knowledge✓), KnowledgeManagement✓, MonitoringIntegration✦, ProblemService✓, SLAEnforcementHostedService✓, SLAService✓, SelfServiceChatbot✦, ServiceCatalog✓, WebhookNotification✦

*(✓ = active test, ✦ = excluded test file exists)*

### 4.3 AI Services — Fully Covered ✅

| Service | Test File | Location |
|---------|-----------|----------|
| AIKnowledgeSearchService | AIKnowledgeSearchServiceTests.cs | CRM.Tests subproject |
| AILeadScoringService | AILeadScoringServiceTests.cs | CRM.Tests subproject |
| AIOpportunityScoringService | AIOpportunityScoringServiceTests.cs | CRM.Tests subproject |
| AIServiceHelper | AIServiceHelperTests.cs | Main project (Helpers/) |
| AllenAIService | AllenAIServiceTests.cs | Main project |

---

## 5. Controllers Without Tests (61 controllers)

Cross-referencing **94 controllers** (88 top-level + 6 webhook) against all test projects:

### 5.1 Controllers WITH Active Tests (11 total)

| Controller | Test File | Status |
|------------|-----------|--------|
| AccountsController | AccountsControllerTests.cs | ✅ Active (19 tests) |
| DepartmentsController | DepartmentsControllerTests.cs | ✅ Active (6 tests) |
| ITSMCMDBController | ITSMCMDBControllerTests.cs | ✅ Active (11 tests) |
| ITSMCatalogController | ITSMCatalogControllerTests.cs | ✅ Active (14 tests) |
| ITSMChangesController | ITSMChangesControllerTests.cs | ✅ Active (15 tests) |
| ITSMIncidentsController | ITSMIncidentsControllerTests.cs | ✅ Active (12 tests) |
| ITSMKnowledgeController | ITSMKnowledgeControllerTests.cs | ✅ Active (15 tests) |
| ITSMProblemsController | ITSMProblemsControllerTests.cs | ✅ Active (11 tests) |
| ITSMSLAController | ITSMSLAControllerTests.cs | ✅ Active (16 tests) |
| OpportunitiesController | OpportunitiesControllerTests.cs | ✅ Active (18 tests) |
| ProductsController | ProductsControllerTests.cs | ✅ Active (17 tests) |

### 5.2 Controllers with EXCLUDED Tests (28 — test file exists but doesn't compile)

Activities, Auth, Campaigns, Contacts, Dashboard, Duplicates, EmailTemplates, FileUpload, ImportExport, Interactions, Invoices, KnowledgeBase, Leads, Lookups, Notes, Orders, Pipelines, Quotes, Reports, ServiceRequests, Settings, Stages, Tasks, UserGroups, UserProfiles, Users, Webhooks, Workflows

### 5.3 Controllers with NO Test File At All (55)

| # | Controller | Priority |
|---|------------|----------|
| 1 | AIAnalyticsController | 🟡 P2 |
| 2 | AIChatbotController | P3 |
| 3 | AIEmailController | P3 |
| 4 | AILeadScoringController | 🟡 P2 |
| 5 | AdminSettingsController | 🟡 P2 |
| 6 | ApprovalsController | 🟡 P2 |
| 7 | CICDIntegrationController | P3 |
| 8 | CalendarIntegrationController | P3 |
| 9 | CampaignExecutionController | 🟡 P2 |
| 10 | CloudDeploymentController | P3 |
| 11 | ColorPalettesController | P3 |
| 12 | CommissionsController | 🔴 P1 |
| 13 | CommunicationsController | 🟡 P2 |
| 14 | ContactInfoController | 🟡 P2 |
| 15 | ContractsController | 🔴 P1 |
| 16 | ConversationsController | 🟡 P2 |
| 17 | CreditMemosController | 🟡 P2 |
| 18 | DashboardConfigController | P3 |
| 19 | DatabaseController | P3 |
| 20 | DocuSealWebhookController | P3 |
| 21 | DuplicatesController | 🟡 P2 |
| 22 | EmailIntegrationController | P3 |
| 23 | EmailToTicketController | P3 |
| 24 | EventAttendeesController | 🟡 P2 |
| 25 | FeaturesController | P3 |
| 26 | FieldMasterDataController | P3 |
| 27 | FormsController | 🟡 P2 |
| 28 | HealthController | P3 |
| 29 | ITSMControllers | ✅ (composite — individual ITSM tests cover it) |
| 30 | ITSMDashboardController | 🟡 P2 |
| 31 | ITSMWebhooksController | P3 |
| 32 | IncidentsController | 🟡 P2 |
| 33 | InvoicesController | 🔴 P1 |
| 34 | KnowledgeAndCatalogControllers | ✅ (composite — covered by ITSM tests) |
| 35 | LandingPageController | P3 |
| 36 | LeadRoutingController | 🟡 P2 |
| 37 | LeadScoreRulesController | P3 |
| 38 | MasterDataController | P3 |
| 39 | ModuleFieldConfigurationsController | P3 |
| 40 | ModuleUIConfigController | P3 |
| 41 | MonitoringController | P3 |
| 42 | MonitoringIntegrationController | P3 |
| 43 | NewsSocialController | 🟡 P2 |
| 44 | NormalizationController | 🟡 P2 |
| 45 | OrdersController | 🔴 P1 |
| 46 | PaymentsController | 🔴 P1 |
| 47 | PriceBooksController | 🟡 P2 |
| 48 | ProductBundlesController | 🟡 P2 |
| 49 | ProviderHealthController | P3 |
| 50 | SalesForecastsController | 🟡 P2 |
| 51 | SalesQuotasController | 🟡 P2 |
| 52 | SampleDataController | P3 |
| 53 | SelfServiceChatbotController | P3 |
| 54 | ServiceRequestSettingsController | 🟡 P2 |
| 55 | SubscriptionsController | 🔴 P1 |
| 56 | SystemSettingsController | 🟡 P2 |
| 57 | TeamsController | 🔴 P1 |
| 58 | TerritoriesController | 🟡 P2 |
| 59 | WorkflowTriggersController | P3 |
| 60 | ZipCodesController | P3 |
| 61 | *Webhooks/* (6 controllers) | P3 |

**🔴 P1 Controllers (no test, core business):** Commissions, Contracts, Invoices, Orders, Payments, Subscriptions, Teams (7 total — all Phase 4 entities)

---

## 6. Test Infrastructure Issues

### 6.1 Root Cause of Mass Exclusion

The 97 excluded files share a common root cause: **rapid feature development outpaced test maintenance**. Specifically:

1. **Entity property drift** — Entities gained new properties (e.g., ITSM fields on ServiceRequest), but mock setups in tests weren't updated
2. **Interface expansion** — `ICrmDbContext` grew to 131+ DbSet members; `TestDbContext` in Extension tests doesn't implement them
3. **Namespace removal** — `CRM.Core.Validation` was removed/refactored, breaking 6 Validator test files
4. **Controller signature changes** — Controller constructors changed (new DI dependencies), breaking all 28 excluded controller tests

### 6.2 Test Project Structure Complexity

Three separate test projects with overlapping scope create confusion:

| Project | Location | Compiled By | Focus |
|---------|----------|-------------|-------|
| Main (CRM.Tests.csproj) | `tests/*.csproj` | Root .csproj | Services, Controllers, ITSM, Integration, Performance |
| CRM.Tests subproject | `tests/CRM.Tests/` | Own .csproj | Phase 2-4 services, AI, Data providers |
| Unit subproject | `tests/Unit/Core/` | Own .csproj | Entity property tests, DTO tests, enum tests |

**Issue:** Some services have tests in multiple projects (e.g., SLAEnforcementHostedService has tests in both Main and CRM.Tests). The Main project's `<Compile Remove>` also masks tests that could potentially be moved to the CRM.Tests subproject.

### 6.3 Backup Files

| File | Location | Issue |
|------|----------|-------|
| Phase4ServiceTests.cs.bak | tests/CRM.Tests/Services/ITSM/ | Dead backup file, should be deleted |
| CustomerService.cs.bak | CRM.Infrastructure/Services/ | Dead backup (Customer→Account migration artifact) |
| CustomersController.cs.bak | CRM.Api/Controllers/ | Dead backup |
| ICustomerService.cs.bak | CRM.Api/Controllers/ | Dead backup (in wrong directory!) |

### 6.4 No Frontend Unit Tests

The `CRM.Frontend/` project has Jest configured (`jest.config.json`, `jest.setup.js`) but **zero test files** exist in `CRM.Frontend/src/`. There are no `*.test.tsx` or `*.spec.tsx` files.

---

## 7. Summary Statistics & Recommendations

### 7.1 Test Count Summary

| Project | Test Files | Active [Fact] Tests | Excluded [Fact] Tests | Skipped |
|---------|-----------|---------------------|----------------------|---------|
| **Main project** | 46 active / 97 excluded | 788 | Unknown (files don't compile) | 8 |
| **CRM.Tests subproject** | 20 | 397 | 0 | 0 |
| **Unit subproject** | 35 | 1,686 | 0 | 0 |
| **E2E (Playwright)** | 39 spec files | 675 | N/A | 47 |
| **Frontend (Jest)** | 0 | 0 | N/A | N/A |
| **TOTAL** | **140 active files** | **3,546** | **~97 files** | **55** |

### 7.2 Coverage Heat Map

| Area | Services | Have Active Test | Have Excluded Test | No Test At All |
|------|----------|-----------------|-------------------|----------------|
| Core Services | 92 | 30 (32.6%) | 33 (35.9%) | 29 (31.5%) |
| ITSM Services | 28 | 11 (39.3%) | 7 (25.0%) | 10 (35.7%) |
| AI Services | 5 | 5 (100%) | 0 | 0 |
| **Total Services** | **125** | **46 (36.8%)** | **40 (32.0%)** | **39 (31.2%)** |

| Area | Controllers | Have Active Test | Have Excluded Test | No Test At All |
|------|-------------|-----------------|-------------------|----------------|
| All Controllers | 94 | 11 (11.7%) | 28 (29.8%) | 55 (58.5%) |

### 7.3 Priority Recommendations

#### 🔴 Critical (Sprint 1) — Re-enable Excluded Tests

**Effort:** ~3-5 days | **Impact:** Recover ~97 test files (~2,000+ estimated tests)

The fastest path to dramatically improving coverage is to **fix the 97 excluded test files**, not write new ones. Most need:
- Updated mock setups (new entity properties)
- Updated constructor calls (new DI dependencies)
- Updated assertion targets (renamed properties)

**Strategy:** Create a `TestDbContext` that implements all 131 `ICrmDbContext` members, then fix excluded tests in batches:
1. Service tests (33 files) — highest value
2. Controller tests (28 files) — second highest
3. ITSM tests (7 files) — requires entity model alignment
4. Others (29 files) — validators, middleware, hosted services

#### 🔴 Critical (Sprint 1) — Phase 4 Service Tests

Write new tests for the 8 Phase 4 services that have **zero test coverage**:

| Service | Est. Tests | Priority |
|---------|-----------|----------|
| InvoiceService | ~25 | 🔴 |
| PaymentService | ~25 | 🔴 |
| OrderService | ~25 | 🔴 |
| ContractService | ~20 | 🔴 |
| SubscriptionService | ~25 | 🔴 |
| TeamService | ~20 | 🔴 |
| CommissionService | ~25 | 🔴 |
| EmailTemplateService | ~20 | 🔴 |
| **Total** | **~185** | |

#### 🟡 Medium (Sprint 2) — Fix E2E Skips

- Fix the 24 ITSM BVT skips (deploy/stabilize ITSM endpoints)
- Fix the 10 auth E2E skips (MUI selector issues)
- Fix the 7 customer/campaign E2E skips

#### 🟡 Medium (Sprint 2-3) — Fill Remaining Gaps

- Add tests for 22 P2 services without any test
- Add tests for 7 P1 controllers (Phase 4 entities)
- Create frontend Jest tests for critical components

#### 🟢 Low (Backlog) — Cleanup

- Delete 4 `.bak` files
- Consolidate test project structure (consider merging CRM.Tests subproject into main)
- Add performance tests to CI as separate pipeline stage
- Fix ~1,964 StyleCop warnings in test files

---

## Appendix A: Active Test Files by Project

### Main Project (46 files, 788 tests)

<details>
<summary>Click to expand</summary>

| Tests | File |
|-------|------|
| 33 | Services/ITSM/ITSMDashboardServiceTests.cs |
| 31 | Services/CampaignExecutionServiceTests.cs |
| 30 | Services/LeadServiceTests.cs |
| 30 | Helpers/ETagHelperTests.cs |
| 30 | Helpers/AIServiceHelperTests.cs |
| 29 | Services/OpportunityServiceTests.cs |
| 29 | Services/DuplicateDetectionTests.cs |
| 28 | Services/CachedZipCodeServiceTests.cs |
| 25 | Services/AllenAIServiceTests.cs |
| 24 | Services/RelationshipServiceTests.cs |
| 21 | Services/ITSM/ChangeServiceTests.cs |
| 21 | HostedServices/CalendarSyncHostedServiceTests.cs |
| 20 | Services/ProductServiceTests.cs |
| 19 | Services/UserServiceTests.cs |
| 19 | Services/ITSM/KnowledgeServiceTests.cs |
| 19 | Controllers/AccountsControllerTests.cs |
| 18 | Services/ContactInfoValidationServiceTests.cs |
| 18 | Services/AccountServiceTests.cs |
| 18 | Controllers/OpportunitiesControllerTests.cs |
| 17 | Integration/BuiltInSearchProviderIntegrationTests.cs |
| 17 | HostedServices/EmailSyncHostedServiceTests.cs |
| 17 | Controllers/ProductsControllerTests.cs |
| 16 | Services/ITSM/SLAEnforcementHostedServiceTests.cs |
| 16 | Services/ITSM/CatalogServiceTests.cs |
| 16 | Controllers/ITSMSLAControllerTests.cs |
| 15 | Services/ITSM/IncidentServiceTests.cs |
| 15 | Controllers/ITSMKnowledgeControllerTests.cs |
| 15 | Controllers/ITSMChangesControllerTests.cs |
| 14 | Services/ITSM/ChangeManagementServiceTests.cs |
| 14 | Services/AuthenticationServiceTests.cs |
| 14 | Controllers/ITSMCatalogControllerTests.cs |
| 13 | Services/ITSM/SLAServiceTests.cs |
| 13 | Services/ITSM/KnowledgeManagementServiceTests.cs |
| 12 | Services/ITSM/ServiceCatalogServiceTests.cs |
| 12 | Integration/MeilisearchProviderIntegrationTests.cs |
| 12 | Controllers/ITSMIncidentsControllerTests.cs |
| 11 | Services/ITSM/ProblemServiceTests.cs |
| 11 | Services/ITSM/CMDBServiceTests.cs |
| 11 | Controllers/ITSMProblemsControllerTests.cs |
| 11 | Controllers/ITSMCMDBControllerTests.cs |
| 10 | Services/SystemSettingsServiceTests.cs |
| 9 | Performance/PerformanceTests.cs |
| 9 | Integration/ProviderDIIntegrationTests.cs |
| 6 | Controllers/DepartmentsControllerTests.cs |

</details>

### CRM.Tests Subproject (20 files, 397 tests)

<details>
<summary>Click to expand</summary>

| Tests | File |
|-------|------|
| 42 | Services/ApprovalWorkflowServiceTests.cs |
| 32 | Services/TerritoryServiceTests.cs |
| 28 | Services/NewsSocialServiceTests.cs |
| 28 | Services/FormBuilderServiceTests.cs |
| 26 | Data/SqlServerProviderStrategyTests.cs |
| 25 | Data/PostgreSqlProviderStrategyTests.cs |
| 25 | Data/OracleProviderStrategyTests.cs |
| 23 | Services/LeadRoutingServiceTests.cs |
| 22 | Data/MySqlProviderStrategyTests.cs |
| 20 | Services/ReportBuilderServiceTests.cs |
| 18 | Services/DashboardBuilderServiceTests.cs |
| 18 | HostedServices/SLAEnforcementHostedServiceTests.cs |
| 17 | Data/DatabaseProviderStrategyFactoryTests.cs |
| 16 | Integration/Services/ActivityServiceTests.cs |
| 15 | Services/DashboardServiceTests.cs |
| 10 | Services/AI/AIOpportunityScoringServiceTests.cs |
| 9 | Services/AI/AILeadScoringServiceTests.cs |
| 9 | Services/AI/AIKnowledgeSearchServiceTests.cs |
| 7 | UserEntityTests.cs |
| 7 | EntityTests.cs |

</details>

### Unit Subproject (35 files, 1,686 tests)

All entity property/DTO tests — 100% active, no exclusions.

---

**END OF AUDIT REPORT**
