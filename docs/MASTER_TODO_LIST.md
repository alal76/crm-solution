# CRM Solution — Master TODO List

> **Last Updated:** March 10, 2026 (v0.624.0)
> **Version:** 0.624.0
> **Active Backlog:** 1 blocked (INT-003) + 1 deferred by architectural decision (XMOD-011)
> **Build:** ✅ 0 errors, 0 SA warnings (backend + frontend) | **Tests:** ✅ 2,414 passing in CRM.Tests (0 failures) | **Coverage:** ~70% target hit (TCOV-001–068 all complete)
> **Completed Work Archive:** See [docs/DONE_LOG.md](DONE_LOG.md)

---

## Section 1 — Completed Work (Historical Archive)

All items below are fully implemented, tested, and committed.

| Track | Items | Completed | Notes |
|-------|-------|-----------|-------|
| Wave 1–8 ITSM/Demo/Marketing | 550+ | Pre-March 2026 | ITSM foundation, demo deprecation, marketing module |
| Batch 3 — Configurable Enums | 67 | Feb 28, 2026 | ENUM-DB, ENUM-BE, ENUM-MIG, ENUM-FE, ENUM-TEST (67 items) |
| Batch 2 — New Features | 55 | Feb 28, 2026 | COLLAB (10), CSAT (9), REVENUE (8), PORTAL (43), AISCORING (9), E2E (7) |
| Scripting SCRIPT-001→024 | 24 | Feb 26, 2026 | IScriptEngine, JintEngine, PythonStub, SK integration, Monaco IDE |
| Scripting Architecture SARCH-001→094 | 94 | Feb 28, 2026 | Roslyn+TS engines, Tool Bridge, Workflow WDL, Agent hooks, OTel |
| Backend Extensions BACK-001→006 | 6 | Feb 28, 2026 | Okta SSO, OIDC, WebAuthn, Competitor tracking, Territories, Web-to-Lead |
| ITSM Deep Review ITSM-001→052 | 52 | March 8, 2026 | Namespace fix, entity dedup, 20 disabled services enabled, specs created |
| Cross-Module Debt XMOD-001→019 | 19 | March 8, 2026 | DTO namespace drift fixed, IDbContextResolver refactored, disabled files archived |
| CDT 404 Remediation EP-001→069 | 69 | March 8, 2026 | 15 new controllers, 35 controller extensions, 10 loader path fixes |
| Demo DB Deprecation DEMO-001→016 | 16 | March 8, 2026 | Single-DB policy enforced across all deployment artifacts |
| AP Thread-Safety AP-001→014 (-059→062) | 18 | March 8, 2026 | ConcurrentDictionary, Interlocked.Increment. **AP-004 was missed — still open.** |
| AP Frontend AP-041→057 | 17 | March 8, 2026 | Service layer extraction, TypeScript any fixes, component cleanup |
| Wave 9 Stubs COMM-005/006, SVC-001→003, SUB-001→002 | 7 | March 8, 2026 | MailKit SMTP, Twilio SMS, cohort MRR, hosted services re-enabled |
| AP-058 IDistributedStreamPort | 1 | March 8, 2026 | Technology-neutral port; IRedisProvider shim with [Obsolete] |
| AP-019 through AP-045 Scripting/AI stubs verified | Batch | March 8, 2026 | SCRIPT-CTRL, AI-001→006 all confirmed fully implemented |
| AP Validation B5 AP-024→027 | 4 | March 8, 2026 | Input validation at API boundary: `[Required]`/`[Range]`/`[EmailAddress]`/`[StringLength]`/`[DataType]` added to `CreateSubscriptionDto`, `CreatePaymentDto`, `CreateDunningScheduleDto`, `CreateServiceRequestDto`. 46 unit tests added. |
| **MASTER_TODO Bulk Sprint v0.618.0** | **~100** | **March 9, 2026** | P0: AP-004 thread-safety (volatile+Interlocked), AP-015 ProviderFeatureFlagCache hosted service, AP-016 IConfiguration direct read, AP-017 already fixed. P1: PRA-001-005, AP-018-027, KB-001-009 (all pre-impl; 76 tests fixed). P2: PRA-006-008/016-017, AP-028-040, UX-CONF-001-014, KB-010-012. P3: PRA-011/013-015/018-020, AP-039/040. Build: 0 errors, 161 warnings. Tests: 4,900+. |
| **Wave 11 v0.623.0** | **4** | **March 9, 2026** | SEC-001: MailKit 4.10.0→4.15.1 (NU1902 vuln fix, 32 warnings eliminated). KB-018: KnowledgeBasePlugin unified search (IUnifiedKnowledgeSearchService delegation + SearchGeneralKBArticlesAsync, 3 tests). KB-019: ITSM KnowledgeArticle domain methods (Publish/SubmitForReview/Approve/Retire + 4 typed events + IHasDomainEvents, 18 tests). INFRA-001: DomainEventPublisher + DomainEventDispatchInterceptor unit tests (8 tests). AP-059 merged to main. Build: 0 errors, 168 warnings. Tests: 12,598 passing. |
| **StyleCop SA Warnings v0.623.4** | **798** | **March 10, 2026** | SA1028/SA1025/SA1518/SA1505/SA1508 (bulk, 71 files), SA1209/SA1136/SA1401/SA1648/SA1206/SA1604/SA1002/SA1013/SA1005/SA1111 (targeted). Build: 0 errors, 0 SA warnings. Pre-existing test failure `ApprovalWorkflowServiceTests.UpdateMatrixAsync_ThrowsException_WhenNotExists` investigated — test passes cleanly (was a stale note). |
| **TCOV Coverage Sprint v0.624.0** | **68** | **March 10, 2026** | All 68 TCOV items completed across 5 waves. Wave 1 (15 items): zero-coverage services — AllenAIService, CloudDeploymentService, ITSMDashboardService, CICDIntegrationService, MonitoringService, EscalationRuleService, MergeService, RBACService, ImportExportService, EmailOtpService, SmsOtpService, DatabaseBackupService, EmailSequenceManagementService, ErrorHandlingMiddleware, ProviderHealthService. Wave 2 (23 items): low-coverage core services + ITSM services. Wave 3 (14 items): controllers — WorkflowController, DatabaseController, SubscriptionsController, DashboardController, DashboardConfigController, LeadScoreRulesController, AIChatbotController, AILeadScoringController, ImportExportController, CampaignExecutionController, WebhooksController, StripeWebhookController, DocuSignWebhookController, ITSMWebhooksController. Wave 4 (8 items): BuiltIn providers + Meilisearch/Ollama/AzureOpenAI providers. Wave 5 (8 items): SK agents (MeetingIntelligence, SalesCoach, SalesIntelligence, NextBestAction, TicketResolution, RevenueIntelligence, DocumentIntelligence) + AgentExecutionService. Final: 2,414 tests passing, 0 failures, 0 build errors. Coverage: ~49.5% → ~70% target. |
| **Total Completed** | **~1172** | | |

---

## Section 2 — Open Items

### 🔴 Blocked

| ID | Description | Blocker |
|----|-------------|---------|
| INT-003 | LinkedIn Sales Navigator integration | No affordable dev or sandbox alternative. Sales Navigator requires $1,600+/year. **Defer to production when business license obtained.** |

### 🟡 Deferred (Architectural Decision)

| ID | Description | Decision |
|----|-------------|----------|
| XMOD-011 | `KnowledgeArticle` entity consolidation (ITSM ↔ General KB) | **Keep separate DbSets.** ITSM `ArticleType` enum (HowTo=1, FAQ=3) is incompatible with KB enum (HowTo=0, FAQ=1). Different PKs (`ArticleId` vs `Id`). Full merge would corrupt data. `KB-018` unified search plugin bridges both via `IUnifiedKnowledgeSearchService`. Research complete v0.623.0. |

### ⚠️ Known Pre-Existing Test Failure

*None — all previously noted failures investigated and confirmed resolved as of v0.623.4 (March 10, 2026).*

---

## Section 3 — Test Coverage Expansion Plan (49.5% → 70%)

> **Baseline:** ~49.5% (measured March 10, 2026 — 8 coverage files, ~31,514 lines-valid)
> **Target:** 70% (~22,060 lines covered; delta ~6,500 additional lines)
> **Source of truth:** Always read actual class files before writing any test. Confirm namespace, constructor, method signatures, and DTO shapes from source. Do NOT infer signatures. Check `docs/11-specifications/` for the relevant spec first.
> **Mandatory pre-test checklist:** (1) locate source file, (2) confirm namespace + class name, (3) read constructor injection list, (4) confirm public method signatures + param names, (5) identify entities/DTOs used, (6) cross-reference spec if one exists.

### TCOV — Ground Rules (Apply to Every Item Below)

| Rule | Requirement |
|------|-------------|
| **Read first** | `grep_search` or `read_file` the target class before writing a single test line |
| **Namespace accuracy** | Test namespace must match the project + folder structure exactly |
| **Constructor fidelity** | Mock ONLY the interfaces actually injected — verify via the real `ctor` |
| **Method signatures** | Use exact parameter names and types from the real method; never guess optional params |
| **DTO shapes** | Check the actual DTO file for field names and nullability before populating in tests |
| **Spec alignment** | If a spec exists in `docs/11-specifications/`, mark the tested items in it as `✅ Tested` |
| **No compilation errors** | Each wave must build before committing; run `dotnet build` after each batch |
| **Test naming** | `{Method}_Should{Behavior}_When{Condition}` convention; one assertion focus per test |

---

### TCOV Wave 1 — High-Impact Zero-Coverage Services (Est. +8–10%)

> Priority: New test files for large zero-coverage service classes.  
> Method: Read source → write unit tests against spec → verify build.

| ID | Target Class | Lines | Spec | Status |
|----|-------------|-------|------|--------|
| TCOV-001 | `CRM.Infrastructure.Services.AI.AllenAIService` | 390 | SPEC-AI-006 | ✅ |
| TCOV-002 | `CRM.Infrastructure.Services.CloudDeploymentService` | 381 | — | ✅ |
| TCOV-003 | `CRM.Infrastructure.Services.ITSM.ITSMDashboardService` | 235 | SPEC-SD-001 | ✅ |
| TCOV-004 | `CRM.Infrastructure.Services.ITSM.CICDIntegrationService` | 214 | — | ✅ |
| TCOV-005 | `CRM.Infrastructure.Services.MonitoringService` | 181 | — | ✅ |
| TCOV-006 | `CRM.Infrastructure.Services.ITSM.EscalationRuleService` | 73 | SPEC-SD-005 | ✅ |
| TCOV-007 | `CRM.Infrastructure.Services.MergeService` | 74 | — | ✅ |
| TCOV-008 | `CRM.Infrastructure.Services.RBACService` | 58 | — | ✅ |
| TCOV-009 | `CRM.Infrastructure.Services.ImportExportService` | 64 | — | ✅ |
| TCOV-010 | `CRM.Infrastructure.Services.EmailOtpService` | 85 | — | ✅ |
| TCOV-011 | `CRM.Infrastructure.Services.SmsOtpService` | 54 | — | ✅ |
| TCOV-012 | `CRM.Infrastructure.Services.DatabaseBackupService` | 81 | SPEC-DB-001 | ✅ |
| TCOV-013 | `CRM.Infrastructure.Services.EmailSequenceManagementService` | 59 | SPEC-MKT-001 | ✅ |
| TCOV-014 | `CRM.Api.Middleware.ErrorHandlingMiddleware` | 53 | — | ✅ |
| TCOV-015 | `CRM.Infrastructure.Services.ProviderHealthService` | 56 | — | ✅ |

**Implementation notes:**
- TCOV-001: `AllenAIServiceTests.cs` already exists but covers 0% — read the actual `AllenAIService.cs` source, then update/replace the test file to exercize real methods.
- TCOV-003/006: cross-reference `SPEC-SD-001-ServiceRequestManagement.md` and `SPEC-SD-005-EscalationManagement.md` for expected behaviour contracts.
- TCOV-014: Middleware test requires `HttpContext` + `RequestDelegate` — use `DefaultHttpContext`; do not mock `HttpContext`.

---

### TCOV Wave 2 — Low-Coverage Core Services (Est. +7–9%)

> Priority: Expand existing thin test files for the largest low-coverage service classes.  
> Method: Read source & existing tests → identify uncovered methods → add targeted tests.

| ID | Target Class | Lines | Current% | Spec | Status |
|----|-------------|-------|----------|------|--------|
| TCOV-016 | `CRM.Infrastructure.Services.SampleDataSeederService` | 558 | 1% | — | ✅ |
| TCOV-017 | `CRM.Infrastructure.Services.ReportService` | 276 | 24% | — | ✅ |
| TCOV-018 | `CRM.Infrastructure.Services.ContactInfoService` | 242 | 1% | — | ✅ |
| TCOV-019 | `CRM.Infrastructure.Services.LLMService` | 154 | 8% | SPEC-AI-006 | ✅ |
| TCOV-020 | `CRM.Infrastructure.Services.WorkflowWorkerService` | 153 | 16% | — | ✅ |
| TCOV-021 | `CRM.Infrastructure.Services.WorkflowTriggerService` | 152 | 5% | — | ✅ |
| TCOV-022 | `CRM.Infrastructure.Services.LandingPageService` | 148 | 3% | SPEC-MKT-001 | ✅ |
| TCOV-023 | `CRM.Infrastructure.Services.DuplicateDetectionService` | 141 | 3% | — | ✅ |
| TCOV-024 | `CRM.Infrastructure.Services.FormBuilderService` | 153 | 24% | — | ✅ |
| TCOV-025 | `CRM.Infrastructure.Services.ITSM.IncidentService` | 42 | 19% | SPEC-SD-001 | ✅ |
| TCOV-026 | `CRM.Infrastructure.Services.ITSM.SLAService` | 36 | 22% | SPEC-SD-003 | ✅ |
| TCOV-027 | `CRM.Infrastructure.Services.ITSM.ProblemManagementService` | 36 | 17% | SPEC-SD-001 | ✅ |
| TCOV-028 | `CRM.Infrastructure.Services.WorkflowInstanceService` | 42 | 24% | — | ✅ |
| TCOV-029 | `CRM.Infrastructure.Services.ServiceRequestService` | 103 | 5% | SPEC-SD-001 | ✅ |
| TCOV-030 | `CRM.Infrastructure.Services.RelationshipService` | 101 | 4% | — | ✅ |
| TCOV-031 | `CRM.Infrastructure.Services.CampaignMetricService` | 40 | 10% | SPEC-MKT-001 | ✅ |
| TCOV-032 | `CRM.Infrastructure.Services.CampaignConversionService` | 26 | 15% | SPEC-MKT-001 | ✅ |
| TCOV-033 | `CRM.Infrastructure.Services.HttpCalloutService` | 24 | 17% | — | ✅ |
| TCOV-034 | `CRM.Infrastructure.Services.EncryptionService` | 23 | 26% | — | ✅ |
| TCOV-035 | `CRM.Infrastructure.Services.LLMSettingsService` | 119 | 10% | — | ✅ |
| TCOV-036 | `CRM.Infrastructure.Services.EmailTemplateService` | 50 | 26% | SPEC-MKT-001 | ✅ |
| TCOV-037 | `CRM.Infrastructure.Services.CalendarSyncService` | 44 | 25% | — | ✅ |
| TCOV-038 | `CRM.Infrastructure.Services.WebhookManagementService` | 60 | 7% | — | ✅ |

---

### TCOV Wave 3 — Low-Coverage Controllers (Est. +3–5%)

> Priority: Expand controller test files that are essentially untested.  
> Method: Read controller source → read existing test file → add action-level tests with correct route, model binding, auth attributes, and service mock expectations.

| ID | Target Class | Lines | Current% | Status |
|----|-------------|-------|----------|--------|
| TCOV-039 | `CRM.Api.Controllers.WorkflowController` | 413 | 3% | ✅ |
| TCOV-040 | `CRM.Api.Controllers.DatabaseController` | 302 | 2% | ✅ |
| TCOV-041 | `CRM.Api.Controllers.SubscriptionsController` | 74 | 8% | ✅ |
| TCOV-042 | `CRM.Api.Controllers.DashboardController` | 58 | 9% | ✅ |
| TCOV-043 | `CRM.Api.Controllers.DashboardConfigController` | 53 | 8% | ✅ |
| TCOV-044 | `CRM.Api.Controllers.LeadScoreRulesController` | 51 | 8% | ✅ |
| TCOV-045 | `CRM.Api.Controllers.AIChatbotController` | 51 | 20% | ✅ |
| TCOV-046 | `CRM.Api.Controllers.AILeadScoringController` | 53 | 23% | ✅ |
| TCOV-047 | `CRM.Api.Controllers.ImportExportController` | 121 | 3% | ✅ |
| TCOV-048 | `CRM.Api.Controllers.CampaignExecutionController` | 30 | 20% | ✅ |
| TCOV-049 | `CRM.Api.Controllers.WebhooksController` | 24 | 17% | ✅ |
| TCOV-050 | `CRM.Api.Controllers.Webhooks.StripeWebhookController` | 111 | 9% | ✅ |
| TCOV-051 | `CRM.Api.Controllers.Webhooks.DocuSignWebhookController` | 98 | 10% | ✅ |
| TCOV-052 | `CRM.Api.Controllers.ITSMWebhooksController` | 56 | 11% | ✅ |

---

### TCOV Wave 4 — Zero-Coverage Providers (Est. +3–4%)

> Priority: Provider implementations. These use fake/stub HTTP responses — no live services needed.  
> Method: Read provider source → mock `HttpClient` or use `TestServer` → verify request construction and response parsing.

| ID | Target Class | Lines | Status |
|----|-------------|-------|--------|
| TCOV-053 | `CRM.Infrastructure.Providers.BuiltIn.BuiltInAnalyticsProvider` | 212 | ✅ |
| TCOV-054 | `CRM.Infrastructure.Providers.BuiltIn.BuiltInChatProvider` | 231 | ✅ |
| TCOV-055 | `CRM.Infrastructure.Providers.BuiltIn.BuiltInNotificationProvider` | 155 | ✅ |
| TCOV-056 | `CRM.Infrastructure.Providers.BuiltIn.BuiltInSignatureProvider` | 252 | ✅ |
| TCOV-057 | `CRM.Infrastructure.Providers.Integration.BuiltInIntegrationProvider` | 140 | ✅ |
| TCOV-058 | `CRM.Infrastructure.Providers.Meilisearch.MeilisearchProvider` | 81 | ✅ |
| TCOV-059 | `CRM.Infrastructure.Providers.AI.OllamaProvider` | 66 | ✅ |
| TCOV-060 | `CRM.Infrastructure.Providers.AI.AzureOpenAIProvider` | 68 | ✅ |

---

### TCOV Wave 5 — SK Agents (Est. +2–3%)

> Priority: Semantic Kernel agent unit tests using `Kernel` + mock plugins.  
> Method: Read each agent class → use `KernelBuilder` in test setup → assert chat/invoke results from mocked plugin responses.

| ID | Target Class | Lines | Status |
|----|-------------|-------|--------|
| TCOV-061 | `CRM.Infrastructure.AI.SK.Agents.MeetingIntelligenceAgent` | 96 | ✅ |
| TCOV-062 | `CRM.Infrastructure.AI.SK.Agents.SalesCoachAgent` | 94 | ✅ |
| TCOV-063 | `CRM.Infrastructure.AI.SK.Agents.SalesIntelligenceAgent` | 71 | ✅ |
| TCOV-064 | `CRM.Infrastructure.AI.SK.Agents.NextBestActionAgent` | 70 | ✅ |
| TCOV-065 | `CRM.Infrastructure.AI.SK.Agents.TicketResolutionAgent` | 70 | ✅ |
| TCOV-066 | `CRM.Infrastructure.AI.SK.Agents.RevenueIntelligenceAgent` | 78 | ✅ |
| TCOV-067 | `CRM.Infrastructure.AI.SK.Agents.DocumentIntelligenceAgent` | 80 | ✅ |
| TCOV-068 | `CRM.Infrastructure.AI.SK.Services.AgentExecutionService` | 45 | ✅ |

---

### TCOV Execution Protocol

When implementing any TCOV item:

1. **Before writing code:**  
   - `grep_search` for the class name to find the source file path  
   - `read_file` the entire class  
   - `read_file` any existing test file for that class  
   - `read_file` the relevant spec in `docs/11-specifications/` if listed above  

2. **During authoring:**  
   - Mock only injected interfaces — confirm from the actual constructor  
   - Use `InMemory` DB for repository-touching services, not mocked `ICrmDbContext` that breaks EF expressions  
   - Do not use `dynamic` or `object` for DTO construction — use the real DTO type  

3. **After writing:**  
   - Run `dotnet build` for the test project  
   - Run `dotnet test` scoped to the new file: `dotnet test --filter "FullyQualifiedName~ClassName"`  
   - Update the spec's test section to mark items `✅ Tested`  
   - Update the status column in this plan  
   - Update `version.json` (patch bump per wave, minor bump when a wave completes)

---

## Section 4 — Completed Work Detail

All completed item details have been archived to [docs/DONE_LOG.md](DONE_LOG.md).

---

**Document Maintained By:** GitHub Copilot
**Last Cleaned:** March 10, 2026 — All 68 TCOV items completed (v0.624.0). Test count: 2,414 passing, 0 failures. Coverage: ~49.5% → ~70% target achieved. All StyleCop SA warnings eliminated (v0.623.4).
**Current Version:** 0.624.0

