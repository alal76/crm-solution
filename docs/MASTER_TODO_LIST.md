# CRM Solution — Master TODO List

> **Last Updated:** August 7, 2026 (full end-to-end review — see [Section 2A](#section-2a--engineering-review-findings-august-6-2026) — followed by three rounds of remediation work on `tech-debt-cleanup-aug2026`, see [Section 2B](#section-2b--findings-discovered-while-implementing-section-2a-august-6-2026))
> **Version:** 0.627.9 (branch `tech-debt-cleanup-aug2026`, not yet merged to `main`)
> **Single Source of Truth:** This file is the canonical backlog, gap register, remediation plan, and execution tracker for the repository. All active TODOs, gaps, remediation work, and implementation priorities should be recorded here.
> **Active Backlog:** 1 blocked (INT-003) + 1 deferred by architectural decision (XMOD-011) + Section 2A (27 items — most fixed) + Section 2B (25 items found while implementing 2A — **19 fixed, 1 mostly-fixed, 5 open**, all 5 of the open ones now behind an explicit decision gate — nothing "safe but not done" remains) + 6 REV-FGAP-003–008 field-gap findings from the FIELD_GAP_REMEDIATION_PLAN.md re-audit (**5 fixed, 1 deliberately deferred alongside REM-ORPHAN-003**)
> **Build:** ✅ 0 backend build errors across **both** backend test projects (`tests/CRM.Tests.csproj` and `tests/CRM.Tests/CRM.Tests.csproj` — this repo has two, don't check only one) | ⚠️ 9 live StyleCop SA warnings (42 total incl. non-SA), down from 255/286 | ✅ Frontend `tsc --noEmit` clean, `CI=true npm run build` succeeds, full Jest suite green | **Tests:** ✅ 2,903 + 7,480 = **10,383 backend tests passing, 0 failures** | ✅ **1,118 frontend tests passing, 0 failures** (44 suites) — all re-verified fresh Aug 7 2026 | **Coverage:** ~70% (TCOV-001–068 done) — still overstates method-level completeness for AccountService/LeadsController/WorkflowController/AllenAIService (unaddressed) | **Next:** the 5 remaining open Section 2B items (REM-ORPHAN-001/002/003, REM-BUG-006, REM-BUG-011) and REM-FGAP-003 all need a human decision before further work — see the Section 2B Remediation Plan table for what each decision is
>
> **⚠️ Process note for future sessions on this branch:** subagents repeatedly reported work as "completed" that had actually been silently lost, reverted, or left in a broken intermediate state mid-session — this happened in both round 2 (`TasksController.cs`, `FeatureFlagsPanel.tsx`, `IntegrationsSettingsPage.tsx`, `OrderService`/`ContactsService`/`MergeService`) and round 3 (a stalled agent still left real, high-quality work on disk that just needed a manual build/test pass to confirm). Most likely cause: concurrent agents racing on the same working tree, combined with an environment auto-commit/checkpoint feature that fires mid-session outside normal `git commit` calls (confirmed once: commit `972eda3d` was not authored by any explicit commit in this session). **Do not trust an agent's self-report of "done" on this branch without independently re-reading the file and re-running the full build/test suite** — every round in this backlog was only actually closed after that independent verification, and it caught a real bug each time (a broken build, a NullReferenceException, an unpersisted feature). Consider `isolation: 'worktree'` for parallel agents touching overlapping files next time.
> **Completed Work Archive:** See [docs/DONE_LOG.md](DONE_LOG.md)

---

## Section 0 — Consolidated Gap Summary and Plan

This document replaces the need to cross-reference multiple backlog and remediation files. Use this file as the only place to review:
- open gaps and TODOs
- architecture and implementation debt
- remediation phases and priorities
- testing and coverage follow-up
- engineering review findings and next actions

### Consolidated priorities

| Priority | Focus | Notes |
|---|---|---|
| P0 | Contract and architecture cleanup | Normalize DTOs, consolidate duplicate entities, archive superseded implementations, and reduce drift. |
| P0 | Operational hardening | Strengthen health checks, retries, background worker resilience, and recovery guidance. |
| P1 | High-value feature gap closure | Finish the most important partially implemented CRM and ITSM workflows before broadening scope. |
| P1 | Test hardening | Add contract and resilience coverage for services, controllers, and background workers. |
| P2 | Security and configuration | Continue hardening auth, secrets, and deployment-time configuration defaults. |
| P2 | Documentation and traceability | Keep specs, implementation notes, and remediation tracking aligned with the codebase. |

### Recommended execution order

1. Contract and architecture cleanup
2. Resilience and operations hardening
3. Feature gap closure for high-value workflows
4. Quality, regression prevention, and documentation maintenance

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
| **TCOV2 Extended Coverage Sprint v0.625.0** | **47** | **March 11, 2026** | All 47 TCOV2 items across 5 waves. Wave A (9 services, 91 tests): AccountService, ContactsService, OpportunitiesService, TaskService, WorkflowService, QuoteService, AccountContactService, AuditLogService, ConversationService. Wave B (8 services, 77 tests): StripeIntegrationService, CommissionRulesEngine, RevenueAnalyticsService, SalesForecastService, ContractService, CurrencyService, TerritoryAssignmentService, SatisfactionService. Wave C (6 ITSM services, 51 tests): ChangeManagementService, ServiceCatalogService, SLAAnalyticsService, CABWorkflowService, KnowledgeBaseService, AssetLifecycleService. Wave D (15 controllers, ~85 tests): AccountsController, ContactsController, OpportunitiesController, UsersController, TasksController, ReportsController, CampaignsController, KnowledgeBaseController, AnalyticsController, TerritoriesController, SalesForecastsController, CommissionsController, OrdersController, AuditLogsController + pre-existing. Wave E (8 infrastructure services, 66 tests): DomainEventPublisher, WebhookAnalyticsService, TotpService, DeadLetterQueueService, BusinessHoursCalculator, ZipCodeService, CampaignExecutionService, AccountingSyncService. Bugs caught+fixed: QuoteStatus enum, IRepository.FindAsync signature, TotpService Base32 binary parsing, EF InMemory required-FK seeding. Final: 2,785 passing, 0 failures. |
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

## Section 2A — Engineering Review Findings (August 6, 2026)

> **Methodology:** Independent full-solution verification against this document's claims. Six parallel review passes: (1) ran `dotnet build` + `dotnet test` directly rather than trusting the status line, (2) ran `tsc --noEmit` + `npm run build` (including `CI=true`) on the frontend, (3) verified INT-003/XMOD-011 against actual code and searched for spec-ID drift, (4) grepped the full backend+frontend source for TODO/FIXME/NotImplementedException/stub patterns not represented in Section 2, (5) spot-checked 5 entities' Entity→DTO→FE-type alignment against [FIELD_GAP_REMEDIATION_PLAN.md](FIELD_GAP_REMEDIATION_PLAN.md), (6) spot-checked 10 TCOV/TCOV2 ✅ items by reading the actual test files and re-running them. Findings below are net-new — i.e. not already represented by INT-003/XMOD-011 above. All are additive to the backlog; nothing here required a code change to produce (docs only, per review scope).

**Build/quality status corrections:**

| ID | Description | Evidence |
|----|---|---|
| REV-BUILD-001 | ⚠️ **Mostly fixed** (`tech-debt-cleanup-aug2026`). Was 255 live SA warnings (286 total); a mechanical, zero-behavior-change sweep across 117 files (SA1028 trailing whitespace, SA1636 file headers, and smaller categories) brought it to **9 SA warnings / 42 total**, re-verified with a full `dotnet build --no-incremental` and full test re-run (2,853 passing, unchanged). Remaining 9 (SA1306 ×3, SA1508 ×2, SA1633/SA1506/SA1400/SA1108 ×1 each) need individual judgment calls, left open rather than forced. The v0.623.4 "0 SA warnings" entry (Section 1) was true at the time; warnings reaccumulate over time and will again — worth a periodic re-check, not a one-time fix. | `dotnet build CRM.sln`, Aug 6 2026 |
| REV-BUILD-002 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). Frontend production build failed under `CI=true` on one ESLint error: a `// NOSONAR` comment placed directly in JSX children instead of inside `{/* */}`. Wrapped in `{/* */}`; `npm run build` with `CI=true` now succeeds (re-verified). | `CRM.Frontend/src/pages/admin/MonitoringDashboard.tsx:1061` (`react/jsx-no-comment-textnodes`) |

**Untracked stub/incomplete integrations (P1/P2 — real code, not represented as open work anywhere in Section 2):**

| ID | Description |
|----|---|
| REV-STUB-001 | ⚠️ **Partially fixed** (`tech-debt-cleanup-aug2026`). `TwilioCallLoggingService` now calls the real Twilio `CallResource.FetchAsync` API for authoritative call status/duration (21 new tests), using the same `Twilio` SDK/config already proven by `TwilioProvider`. Still open: the service isn't registered in DI anywhere and no controller injects it, so it's correctly wired now but still unreachable at runtime. (`Services/Integrations/TwilioCallLoggingService.cs`) |
| REV-STUB-002 | `AccountingSyncService` — QuickBooks/Xero sync all return "not yet implemented" (`Services/Integrations/AccountingSyncService.cs:47,58,69,100`) |
| REV-STUB-003 | `MarketingSyncService` — Mailchimp/HubSpot contact/segment/campaign sync all stubbed "not yet implemented" (`Services/Integrations/MarketingSyncService.cs:49,63,77,93`) |
| REV-STUB-004 | `LinkedInSalesNavService` messaging path and `CommunicationService` LinkedIn delivery are stubs separate from INT-003's Sales Navigator scope (`Services/Integrations/LinkedInSalesNavService.cs:22,82`; `Services/CommunicationService.cs:753`) |
| REV-STUB-005 | `SchedulingIntegrationService` — stub implementation (`Services/Integrations/SchedulingIntegrationService.cs:21`) |
| REV-STUB-006 | `CloudDeploymentService` — AWS/Azure/GCP/DigitalOcean connection tests all "not yet implemented" (`Services/CloudDeploymentService.cs:652,1230,1241,1252,1262`) |
| REV-STUB-007 | `GeoLocationService` — GeoIP lookup returns mock data only (`Services/Auth/GeoLocationService.cs:36,52,82,94`, tagged TODO-AUTH-024) |
| REV-STUB-008 | `PythonScriptEngine.IsAvailable => false` — Python scripting entirely non-functional pending pythonnet integration (`Infrastructure/Scripting/PythonScriptEngine.cs:16-30`) |
| REV-STUB-009 | ✅ **Fixed for the generic pair** (`tech-debt-cleanup-aug2026`). `SlackNotificationService` and `TeamsNotificationService` now do real Incoming Webhook POSTs (Block Kit / Adaptive Cards), reusing the already-proven `TeamsNotificationChannelService`/ITSM pattern, config `Providers:Notifications:Slack/Teams:WebhookUrl` (20 new tests). **Correction to the original finding:** `TeamsNotificationChannelService` was already a real implementation, not a stub — it was misclassified above. `SmsNotificationService` was NOT touched by this fix and remains a stub. Also newly confirmed: none of `SlackNotificationService`/`TeamsNotificationService`/`TeamsNotificationChannelService` have a DI registration or any caller anywhere in the codebase — wired correctly now but still unreachable at runtime, same shape as REV-STUB-001. (`Services/Notifications/*.cs`) |
| REV-STUB-010 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). `PaymentTokenizationService` confirmed genuinely dead (no controller ever injects it) and marked `[Obsolete]` on both interface and implementation, pointing to `StripeIntegrationService`. Not deleted, DI registration untouched, per CLAUDE.md's do-not-delete-without-confirmation guidance. (`Services/PaymentTokenizationService.cs`) |
| REV-STUB-011 | `CampaignExecutionService.cs:877` — "Stub implementation - returns basic success result" |
| REV-STUB-012 | ⚠️ **Partially fixed** (`tech-debt-cleanup-aug2026`). `SubscriptionBillingController`'s dunning retry endpoint now calls the real, DI-registered `IDunningManager.RetryFailedPaymentAsync` instead of faking a timestamp bump (5 new tests, EF InMemory). Still open, and explicitly NOT touched by this fix: `DunningManager`'s own card-charge step is a hardcoded-`false` TODO — see REV-STUB-013 and REV-BUG-006 below (`StripeIntegrationService`'s outbound calls are 100% simulated). (`Controllers/SubscriptionBillingController.cs`) |
| REV-STUB-013 | Recurring billing / dunning Hangfire jobs not actually scheduled in `Program.cs:1141-1143`; `DunningManager.cs.disabled` / `RecurringBillingEngine.cs.disabled` in `Services/archive/` correspond to this unwired flow. The controller-to-manager wiring half of this closed via REV-STUB-012; the card-charge half is still open (see REV-BUG-006). |
| REV-STUB-014 | 29 archived `.cs.disabled` files under `Services/archive/` and `Services/ITSM/archive/` — most appear superseded by active reimplementations; re-confirm each is genuinely dead before next cleanup pass, don't assume |

**Untracked non-functional frontend UI (P2 — not represented as open work anywhere in Section 2):**

| ID | Description |
|----|---|
| REV-FE-001 | `AccountMergeDialog.tsx` / `AccountHierarchyTree.tsx` referenced but don't exist — exports commented out (`components/crm/accounts/index.ts:6-15`) |
| REV-FE-002 | `EmailDigestPage.tsx` entirely non-functional — no backend endpoint; load/save/preview all no-op (`pages/EmailDigestPage.tsx:76-109`) |
| REV-FE-003 | `ReportTemplatesPage.tsx` not wired to a backend API (`pages/ReportTemplatesPage.tsx:155`) |
| REV-FE-004 | `InvoiceDetailsPage.tsx` PDF download not implemented (`pages/InvoiceDetailsPage.tsx:168`) |
| REV-FE-005 | `FeatureFlagsPanel.tsx` save is a no-op, doesn't call the API (`components/admin/FeatureFlagsPanel.tsx:75`) |
| REV-FE-006 | `IntegrationsSettingsPage.tsx` integration cards are stub placeholders (`pages/admin/IntegrationsSettingsPage.tsx:226`) |
| REV-FE-007 | `ReportDesigner.tsx` per-filter inline validation not implemented, TODO-AI005-FE-006 (`components/analytics/ReportDesigner.tsx:337,344,684`) |
| REV-FE-008 | `Navigation.tsx` drag-and-drop category reordering not persisted/applied, TODO-SYS007-003, 4 locations (`components/Navigation.tsx:160,868,969,1122`) |

**Field/DTO contract gaps (violates the mandatory Field Gap Audit policy in [copilot-instructions.md](../.github/copilot-instructions.md) §1.1 — not represented in [FIELD_GAP_REMEDIATION_PLAN.md](FIELD_GAP_REMEDIATION_PLAN.md)):**

| ID | Description |
|----|---|
| REV-FGAP-001 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). `OpportunityDto` was silently dropping 6 entity fields used by business logic — `ForecastCategory`, `LossReasonCategory`, `LossReason`, `CompetitorWinnerId`, `WinLossNotes`, `ClosedDate`. Added to the DTO, controller mapper, and frontend type, plus a new reflection-based DTO-superset contract test (`tests/Dtos/OpportunityCrmTaskDtoContractTests.cs`) so this class of gap fails CI going forward. |
| REV-FGAP-002 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). `CrmTaskDto` was missing 5 fields the frontend `CrmTask` type already declared — `TaskType`, `StartDate`, `EstimatedMinutes`, `AccountId`, `OpportunityId` — now added to the read DTO and controller mapper. **Narrower gap found while fixing this:** the *write* DTOs (`CreateCrmTaskDto`/`UpdateCrmTaskDto`) still lack `TaskType`/`StartDate` (update also lacks `AccountId`/`OpportunityId`) — see REV-BUG-008. |

**Documentation drift:**

| ID | Description |
|----|---|
| REV-DOC-001 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). All 13 dangling SPEC IDs now have files documenting the already-implemented code: `SPEC-CRM-002` through `-005`, `SPEC-SD-004`, `SPEC-SD-006`, `SPEC-SLS-002` through `-007`, `SPEC-WRK-001`. `docs/11-specifications/INDEX.md` updated to list all of them. Authoring them surfaced a major recurring architecture problem — see **Section 2B** below. Also discovered: `SPEC-TEMPLATE.md`, referenced throughout this repo's docs/instructions, does not actually exist. |
| REV-DOC-002 | `docs/11-specifications/INDEX.md`'s table omitted 7 of the 15 spec files that exist on disk (`SPEC-ARCH-001`, `SPEC-ARCH-002`, `SPEC-INF-001`, `SPEC-SD-001`, `SPEC-SD-002`, `SPEC-SD-003`, `SPEC-SD-005`). **Fixed as part of this review** — INDEX.md now lists all 15. |
| REV-DOC-003 | ✅ **Closed** (`tech-debt-cleanup-aug2026`). `FIELD_GAP_REMEDIATION_PLAN.md` (last full audit 2026-02-21) was stale in both directions: its documented gaps for Contact (38 fields), most of CrmTask's 15-field DTO list, Account DTO (37 fields), and Campaign frontend (~90 fields) were already closed in code but still shown as open; conversely it didn't mention REV-FGAP-001/002. All 16 entities are now re-verified against current code — 7 of the 9 entities checked in the follow-up pass had real, previously-undocumented gaps, now tracked as **REV-FGAP-003 through REV-FGAP-008** below. |
| REV-DOC-004 | INT-003 (LinkedIn Sales Navigator) wording overstates the gap. A full port interface, DI-registered service, and controller endpoints already exist — `ILinkedInSalesNavService`, `LinkedInSalesNavService` (`Services/Integrations/`), `IntegrationsController` `GET /linkedin/profile` + `POST /linkedin/enrich/contact/{id}`. Every method is an intentional stub returning `LinkedInEnrichResult.Failed(..., "not configured")`, consistent with the doc's functional conclusion (blocked pending the $1,600+/yr license) — but "no dev/sandbox alternative" undersells that the integration surface is already built and only needs real API wiring once licensed. |
| REV-DOC-005 | ⚠️ **Partially fixed** (`tech-debt-cleanup-aug2026`). TCOV/TCOV2 ✅ markers are honest about file-exists/passes (10-item spot-check: all files found, all tests passed, no fabricated or wrong-class tests) but overstate method-level completeness. `MergeService`'s zero-coverage merge/unmerge/preview gap is now closed (21 tests added) — and doing so uncovered two real data-integrity bugs, now fixed; see **REV-BUG-001/002** in Section 2B. Still open: `AccountService` covers 3 of 24 public methods; `LeadsController` covers 5 of 16 endpoints; `WorkflowController` — re-measured while authoring SPEC-WRK-001: **35** actions total (not ~24 as originally estimated here), only 6 tested; `AllenAIService` covers 5 of 17 methods. Treat "✅" in Section 3/3B as "a test file exists and passes," not "the class is thoroughly covered." |

**Recommended next actions:** triage remaining REV-STUB-* and REV-FE-* into the phased plan (most are P1/P2 — high-value but non-blocking stubs), and — now the higher priority given Section 2B — decide the fate of each orphaned-but-tested implementation (REV-ORPHAN-\*) before adding any more parallel logic to those modules.

---

## Section 2B — Findings Discovered While Implementing Section 2A (August 6, 2026)

> **How these were found:** while implementing the Section 2A fixes on branch `tech-debt-cleanup-aug2026` (DTO gaps, notification/Twilio/dunning wiring, MergeService coverage, and authoring the 13 specs from REV-DOC-001), the agents doing the work read deeply enough into each area to surface problems well beyond what the original review's spot-checks caught. These are generally more severe than the Section 2A findings — several are silent data-integrity bugs or "tested" code that the live application never actually runs. Nothing in this section was invented or spot-checked from a distance; each finding cites the spec or commit where it was verified against real code.

### REV-ORPHAN-\* — Fully-implemented, tested services the live API doesn't call

A single pattern shows up **five separate times** across unrelated modules: a well-built, unit-tested service exists, is even DI-registered, but the actual controller serving that module's live API either never calls it or was never wired to it at all — instead duplicating (often more thinly, sometimes untested) logic directly against `CrmDbContext`. This is worth fixing as its own workstream, not module-by-module, because it's clearly a repeated process gap (services built before/without checking what the controller already does) rather than five unrelated bugs.

| ID | Description | Source |
|----|---|---|
| REV-ORPHAN-001 | `ChangeManagementService` (13 methods) + `CABWorkflowService` (7 methods) are fully implemented and tested (TCOV2-C01/C04, 9+14 and 8 tests) but unreachable from any HTTP controller. The live `/api/changes` route uses a fourth, separate `IChangeService`/`ChangeService` pair. A fifth implementation, `IChangeManagementServiceEx` (1,361 lines, the richest of all of them), is also unwired. | SPEC-SD-004-ChangeManagement.md |
| REV-ORPHAN-002 | Three separate commission-calculation implementations (`CommissionRulesEngine`, `CommissionRuleService`, `CommissionRuleEvaluationService`) plus a second lead-territory-matching implementation (`TerritoryAssignmentService`) are fully built, unit-tested, and DI-registered but never injected by `CommissionsController`/`TerritoriesController`. | SPEC-SLS-006-CommissionTerritoryManagement.md |
| REV-ORPHAN-003 | Two parallel Lead systems exist. The real UI (`LeadsPage.tsx`) uses Contacts with `contactType=2`, source/status packed as JSON into `notes`. A second, far more capable `Lead`/`LeadService`/`LeadsController` system (BANT/MEDDIC qualification, nurture campaigns, UTM attribution, aging alerts, ~55 tests) sits almost entirely unused — only a minor lead-scoring widget touches it. | SPEC-CRM-003-LeadManagement.md |
| REV-ORPHAN-004 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). `QuotesController` now delegates all CRUD/lifecycle operations to `IQuoteService` instead of reimplementing them against `CrmDbContext` directly. 3 narrow, justified additions to `QuoteService`/`IQuoteService` along the way: `Include()`s so `AccountName`/`ContactName`/`ProductName` still populate after delegating; a new `MarkViewedAsync`; extended `UpdateAsync`'s copied-fields list to include signature/lifecycle-date fields (this also fixed a pre-existing dormant bug in `DocuSealWebhookController`, which already tried to set those fields but they were silently dropped). `CrmDbContext` is still used directly only for line-item endpoints and PDF generation, which `IQuoteService` has no methods for — documented as intentionally out of scope. 38/38 and 168/168 Quote-filtered tests pass across both test projects. | SPEC-SLS-003-QuoteManagement.md |
| REV-ORPHAN-005 | ⚠️ **Mostly fixed** (`tech-debt-cleanup-aug2026`). `ISlackNotificationService`/`ITeamsNotificationService`/`ITwilioCallLoggingService` are registered in DI (Scoped). Slack/Teams now have a real caller: `EscalationRuleService.ExecuteRuleActionAsync` sends a chat alert via `SendEscalationAlertAsync` after an escalation rule executes, wrapped per-channel so a delivery failure can't affect the escalation action that already succeeded. Still open: Twilio has no caller, because no call-status-callback webhook endpoint exists yet (`TwilioWebhookController` only handles SMS status callbacks) — inventing one was judged out of scope. | Commits on `tech-debt-cleanup-aug2026`, this section |

### REV-TESTFAKE-\* — Test files that provide zero real coverage despite existing

| ID | Description | Source |
|----|---|---|
| REV-TESTFAKE-001 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`, alongside REV-ORPHAN-004). `QuotesControllerTests.cs` — both the fake `Assert.True(true)` copy under `tests/Controllers/` and a second, stale-but-real copy under `tests/CRM.Tests/Controllers/` — rewritten with real Moq-based tests (mock `IQuoteService`, verify calls/arguments, assert HTTP result types). ~23 tests per file, all passing. | SPEC-SLS-003-QuoteManagement.md |

### REV-BUG-\* — Concrete bugs found while implementing/documenting, beyond MergeService

| ID | Description | Status |
|----|---|---|
| REV-BUG-001 | `MergeService.MergeRecordsAsync` allowed self-merge (`MasterRecordId` also present in `RecordsToMerge`) — master record would be soft-deleted into itself. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — validation now rejects the request. |
| REV-BUG-002 | `MergeService.MergeRecordsAsync` never called `SaveChangesAsync()` before committing its transaction (unlike `UnmergeRecordsAsync`, which does). For a single-record merge, or the last record in a multi-record merge, neither the audit row nor the soft-delete flag was ever persisted — even though `MergeResult.Success` reported `true`. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — added the missing `SaveChangesAsync()` call. |
| REV-BUG-003 | `MergeService.GetRecordSnapshotAsync` does a bare `JsonSerializer.Serialize(record)` on the raw EF entity with no cycle handling. If other tracked entities in the same `DbContext` create a circular navigation reference (e.g. `Lead.Opportunities` ↔ `Opportunity.Lead`), snapshot serialization throws and the whole merge fails. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — `ReferenceHandler.IgnoreCycles` added; regression test deliberately forces the cycle (unlike its sibling test, which works around it by clearing the change tracker) and confirms no throw. |
| REV-BUG-004 | `OrderService.CreateFromQuoteAsync` calls `.Include(q => q.LineItems)` where `Quote.LineItems` is actually a scalar `string` property, not the real navigation property (`QuoteLineItems`). Likely throws at runtime; invisible in tests because all `OrderService` tests mock `IOrderService`. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — corrected to `QuoteLineItems`; new tests exercise the real (non-mocked) `OrderService` with actual seeded line items. |
| REV-BUG-005 | Contract module has several confirmed frontend/backend mismatches: the Renew endpoint ignores its request body; `contractService.ts` calls `/signature` (doesn't exist) instead of the real `/send-for-signature` + `/signature-status`; frontend field names `title`/`contractStatus` don't match backend `name`/`status`, breaking `ContractDetailsPage.tsx` rendering; `ContractsPage.tsx` calls `/upload`/`/download` routes that don't exist at all; three divergent frontend `Contract` type definitions coexist. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — Renew now applies `NewStartDate`/`NewEndDate`/`NewValue`; frontend calls the real signature routes; `Contract`/`ContractStatus`/`ContractType` consolidated into one definition in `types/sales.ts` matching the backend DTO; dead upload/download UI removed (no backend endpoint exists for it — noted, not invented). |
| REV-BUG-006 | `StripeIntegrationService`'s outbound calls (create payment intent, charge, capture, refund) are **100% simulated** — no Stripe.NET SDK call anywhere, confirmed by the test file's own comment — despite being marked TCOV2-B01 ✅. Inbound webhook handling (`StripeWebhookController`) is real (HMAC-SHA256 verified). This is why REV-STUB-012/013's card-charge step can't be closed yet. Also: Stripe webhook signature verification silently no-ops when `STRIPE_WEBHOOK_SECRET` is unset. | ❌ Still open — needs a real Stripe.NET integration decision, deliberately not attempted this pass (see Section 2B Remediation Plan). |
| REV-BUG-007 | `territoryService.ts`'s "Auto-Assign" button calls a route that doesn't exist on the backend. Three separate frontend `OrderStatus` enums exist, one (`types/sales.ts`) with numeric values that don't match the backend at all — contradicts `FIELD_GAP_REMEDIATION_PLAN.md`'s "Complete"/"mapped" claim for Order. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — the Order-enum consolidation and the `territoryService.ts` dead-route removal are both done (the button/handler/type were removed rather than inventing a bulk endpoint; a real per-account auto-assign endpoint exists and is noted in a comment). |
| REV-BUG-008 | `CrmTask` write DTOs (`CreateCrmTaskDto`/`UpdateCrmTaskDto`) still lack `TaskType`/`StartDate` (update also lacks `AccountId`/`OpportunityId`), even though the read DTO now has all 5 after REV-FGAP-002. Narrower, separate gap — a task's type/start date can be read back but not set via the API. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — both write DTOs now have all the fields, wired through `TasksController`'s create/update mapping. |
| REV-BUG-009 | `Contact`: `PreferredContactMethod` is accepted in request DTOs but silently dropped — never written to the entity. `Update`/`Delete`/social-media endpoints have zero unit-test coverage at both service and controller layers. Account-assignment methods exist on `ContactsService` but have no controller route. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — `PreferredContactMethod`/`DoNotContact` persistence (round 2), real test coverage for `UpdateAsync`/`DeleteAsync`/social-media-link methods at both service and controller layers, and `POST /api/contacts/{id}/account/{accountId}` + `DELETE /api/contacts/{id}/account` routes for the account-assignment methods (round 3). |
| REV-BUG-010 | `TasksController` bypasses `TaskService`/`ITaskService` entirely — queries `CrmDbContext` directly with its own duplicate mapping logic. The two paths disagree on delete semantics: the controller hard-deletes, the service soft-deletes. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — `TasksController` now delegates Create/Get/Update/Delete/Complete to `ITaskService` (soft-delete included); `CrmDbContext` retained only for the `my-queue` endpoint, which needs `UserGroup` joins `ITaskService` doesn't expose. Regression test confirms delete soft-deletes (row still exists, `IsDeleted=true`, not visible via `GetTask`). |
| REV-BUG-011 | `SalesForecastService`/`SalesForecastsController` are solid and tested on the backend, but a repo-wide search confirms **zero frontend implementation** — no page, service, or route anywhere references `SalesForecast` or `/api/sales-forecasts`. | ❌ Still open — needs a frontend feature, not a wire-up; deliberately not attempted this pass. |

### REV-FGAP-\* — Additional field-gap findings from the FIELD_GAP_REMEDIATION_PLAN.md re-audit (REV-DOC-003)

Found while re-verifying the 9 entities not covered by the original Aug 6 review (see [FIELD_GAP_REMEDIATION_PLAN.md](FIELD_GAP_REMEDIATION_PLAN.md) for full detail per entity). Same "DTO/FE-type silently drops a field" shape as REV-FGAP-001/002, just not yet fixed in code.

| ID | Description | Status |
|----|---|---|
| REV-FGAP-003 | `Lead`: `LeadDto`/`LeadSummaryDto` drop ~20 entity fields entirely — full BANT/MEDDIC qualification scoring, UTM/attribution fields, nurture/territory fields. | ❌ Open. |
| REV-FGAP-004 | `Quote`: frontend `Quote` type declares 8 fields `QuoteDto` doesn't have (`warrantyEndDate`, `termsAndConditions`, `expectedDeliveryDate`, `actualDeliveryDate`, `serviceStartDate`, `serviceEndDate`, `attachments`, `customFields`) — silently `undefined` at runtime. | ❌ Open. |
| REV-FGAP-005 | `Payment`: frontend `Payment` type missing 14 DTO fields, including `FraudFlagged`, `RiskScore`, `AmountApplied`/`AmountUnapplied`, `RefundedAmount`, `RetryCount`. | ❌ Open. |
| REV-FGAP-006 | `Contract`: `ContractDto` declares `AnnualValue`/`RenewalTermMonths`, but neither exists on the entity nor is mapped anywhere in the backend — dead fields always serializing `0`, client writes silently discarded. | ❌ Open. |
| REV-FGAP-007 | `ServiceRequest`: DTO missing `DueDate`, `StatusCode`, `LastModifiedByUserId`, `ConversationId` — all already declared on the frontend type. | ❌ Open. |
| REV-FGAP-008 | `User`: DTO missing `PasswordNeverSet`/`CommissionPlanId`; frontend `User` type (in both `common.ts` and a local duplicate in `UserManagementPage.tsx`) missing ~20 preference/security fields the DTO already returns. | ❌ Open. |

### Section 2B Remediation Plan

Each open finding gets one concrete remediation item below — priority, size, and whether it needs a human decision before an agent/engineer can safely start (the REV-ORPHAN-\* items in particular are NOT safe to "just fix" unilaterally: picking the wrong side to delete loses real functionality, e.g. the tested `Lead`/`QuoteService`/`ChangeManagementService` implementations are in some ways *better* than what the live controller does today).

| ID | Status | Priority | Effort | Proposed Remediation | Decision Needed Before Starting |
|----|--------|----------|--------|------------------------|----------------------------------|
| REM-ORPHAN-001 | ❌ Open | P1 | L | Pick ONE Change Management implementation to be canonical. Likely candidate: wire `/api/changes` to `ChangeManagementService`/`CABWorkflowService` (already tested) instead of the untested `ChangeService`; retire the other three (`ChangeService`, `IChangeManagementServiceEx`) — or the reverse if `ChangeService` turns out to have functionality the tested pair lacks. Read all four before choosing. | **Yes** — which of the 4 implementations is functionally correct/current; a code review can't tell alone which one reflects the actual desired business process. |
| REM-ORPHAN-002 | ❌ Open | P1 | L | Same shape as REM-ORPHAN-001: consolidate `CommissionRulesEngine`/`CommissionRuleService`/`CommissionRuleEvaluationService` into one, wire `CommissionsController` to it; decide whether `TerritoryAssignmentService` replaces or complements whatever `TerritoriesController` uses today. | **Yes** — which commission engine encodes the current real commission plan rules; likely needs Sales-ops sign-off, not just an engineering call. |
| REM-ORPHAN-003 | ❌ Open | P2 | L | Decide product direction: either (a) migrate the Contacts-as-Leads UI onto the richer `Lead`/`LeadService` system and retire the JSON-in-notes approach, or (b) formally retire the standalone `Lead` system and fold any features worth keeping (BANT/MEDDIC, nurture, UTM) into the Contact-based flow. Both are real migrations, not quick fixes. | **Yes** — this is a product/UX decision (which lead model users should see), not something to infer from code alone. |
| REM-ORPHAN-004 | ✅ **Fixed** | P1 | M | ~~Wire `QuotesController` to `QuoteService`~~ — done, paired with REM-TESTFAKE-001. | — |
| REM-ORPHAN-005 | ⚠️ Mostly fixed | P2 | S | Slack/Teams: ✅ done — `EscalationRuleService.ExecuteRuleActionAsync` now sends a chat alert via both services' `SendEscalationAlertAsync` after a rule executes, wrapped per-channel so a delivery failure can't break the escalation action. Twilio: still open — no call-status webhook endpoint exists to wire it into (would mean inventing a new endpoint, judged out of scope). | No — pure wiring, now that REV-STUB-001/009 made the implementations real. |
| REM-TESTFAKE-001 | ✅ **Fixed** | P0 | S | ~~Rewrite `QuotesControllerTests.cs` with real assertions~~ — done, both copies of the file. | — |
| REM-BUG-003 | ✅ **Fixed** | P2 | S | ~~Add cycle-safe serialization to `GetRecordSnapshotAsync`~~ — done (`ReferenceHandler.IgnoreCycles`). | — |
| REM-BUG-004 | ✅ **Fixed** | P1 | S | ~~Fix the `.Include()` property name~~ — done, plus a real (non-mocked) regression test. | — |
| REM-BUG-005 | ✅ **Fixed** | P1 | M | ~~Fix the 4 confirmed Contract frontend/backend mismatches~~ — done (Renew body, signature routes, consolidated `Contract` type, dead upload/download UI removed). | — |
| REM-BUG-006 | ❌ Open | P2 | L | Product decision: commit to a real Stripe.NET integration for outbound calls (payment intent/charge/capture/refund) or explicitly document Stripe as inbound-webhook-only for now. Either way, fix the silent no-op when `STRIPE_WEBHOOK_SECRET` is unset (should fail closed/loud, not silently skip verification). | **Yes** — real Stripe wiring is a scoped effort of its own (sandbox credentials, PCI considerations); shouldn't be attempted piecemeal inside an unrelated task. |
| REM-BUG-007 | ✅ **Fixed** | P2 | S | ~~Order-enum consolidation~~ and ~~`territoryService.ts`'s dead "Auto-Assign" route~~ — both done. The dead button/handler/type were removed (no matching backend route exists — a per-account endpoint does, noted in a comment) rather than inventing a bulk endpoint. | — |
| REM-BUG-008 | ✅ **Fixed** | P2 | S | ~~Add the missing write-DTO fields~~ — done, mirroring the REV-FGAP-002 pattern. | — |
| REM-BUG-009 | ✅ **Fixed** | P2 | M | ~~Wire `PreferredContactMethod`~~ (done in round 2) ~~, add missing test coverage, add the account-assignment route~~ — done in round 3: real tests for `UpdateAsync`/`DeleteAsync`/social-media-link methods at both service and controller layers, plus `POST /api/contacts/{id}/account/{accountId}` and `DELETE /api/contacts/{id}/account` routes for the previously-orphaned `AssignToAccountAsync`/`UnassignFromAccountAsync`. | — |
| REM-BUG-010 | ✅ **Fixed** | P0 | M | ~~Fix the delete-semantics inconsistency~~ — done; `TasksController` now delegates to `TaskService` (soft-delete), with a regression test guarding against the hard-delete regression recurring. | — |
| REM-BUG-011 | ❌ Open | P3 | L | Build a Sales Forecasting frontend (list/detail view, likely a dashboard widget) against the already-solid `SalesForecastsController` API. Net-new UI work, not a wire-up. | **Yes** — needs product/design input on what the forecasting UI should look like; nothing to copy from an existing partial implementation. |
| REM-FGAP-003 | ❌ Open | P1 | M | Add the ~20 missing BANT/MEDDIC/UTM/nurture/territory fields to `LeadDto`/`LeadSummaryDto`, mirroring the REV-FGAP-001/002 pattern. Note: this DTO belongs to the orphaned `Lead` system (REV-ORPHAN-003/REM-ORPHAN-003) — deliberately skipped in round 3 for this same reason, low value to fix in isolation until that product decision lands. | Soft — technically no decision needed to add the fields, but consider bundling with REM-ORPHAN-003 rather than fixing a DTO for a system that might be retired. |
| REM-FGAP-004 | ✅ **Fixed** | P2 | S | ~~Add the 8 missing fields to `QuoteDto`~~ — done. All 8 existed on the `Quote` entity; added to the DTO and `QuotesController.MapToDto`, no frontend type change needed. | — |
| REM-FGAP-005 | ✅ **Fixed** | P2 | S | ~~Add the 14 missing fields to the frontend `Payment` type~~ — done. Confirmed none are on the security-exclusion list before adding. | — |
| REM-FGAP-006 | ✅ **Fixed** | P2 | S | ~~Wire or remove `ContractDto.AnnualValue`/`RenewalTermMonths`~~ — removed. Confirmed zero frontend references to either field and no live DB to run a migration against even if wiring were the right call; also removed 15 tests that specifically validated these two dead fields (genuinely dead test code, not lost coverage). | — |
| REM-FGAP-007 | ✅ **Fixed** | P2 | S | ~~Add `DueDate`, `StatusCode`, `LastModifiedByUserId`, `ConversationId` to `ServiceRequestDto`~~ — done. Also fixed a pre-existing test-setup gap the new test surfaced: the shared `ServiceRequestServiceTests` mock context never set up the `ServiceRequestCustomFieldValues` DbSet, causing an NRE on any test exercising the full `GetServiceRequestByIdAsync` mapping path. | — |
| REM-FGAP-008 | ✅ **Fixed** | P3 | M | ~~Add `PasswordNeverSet`/`CommissionPlanId` to `UserDto`; add the missing fields to the frontend `User` type; consolidate `UserManagementPage.tsx`'s duplicate~~ — all done. The page's local type now `extends Omit<SharedUser, 'username' \| 'role'>`, narrowing only the two fields it genuinely needs typed differently, instead of re-declaring ~15 fields. | — |

**Sequencing:** as of the round-3 pass, every no-decision-needed item above is closed except REM-ORPHAN-005's Twilio half (blocked on a webhook endpoint that doesn't exist) and REM-FGAP-003 (deliberately deferred alongside REM-ORPHAN-003, same underlying system). What remains all needs a human call: REM-BUG-006 and REM-ORPHAN-001/002/003 need explicit sign-off (architecture or product) before an agent should touch them — attempting them blind risks silently deleting the *better* implementation, which would make this exact class of problem worse, not better. REM-BUG-011 needs product/design input for a net-new Sales Forecasting UI.

---

## Section 3 — Test Coverage Expansion Plan (→ 70% Verified)

> **Post-TCOV Baseline:** ~55–60% estimated (TCOV-001–068 completed March 10, 2026 — 2,414 tests passing, 68 new/expanded test files).
> **Target:** 70% measured line coverage (`dotnet test --collect:"XPlat Code Coverage"` then `reportgenerator`).
> **Delta:** ~430 services without test files, ~188 controllers without test files — total source ~595 untested classes.
> **Approach:** Measure actual Cobertura XML first (`/tmp/cov-results/`), then select the highest-line-count zero-coverage classes for each wave to maximise coverage gain per effort.

---

### ⚠️ MANDATORY PRE-WRITE PROTOCOL (Zero-Tolerance — No Exceptions)

Every TCOV2 item **must** follow this exact sequence before a single test line is authored. Skipping any step causes constructor/signature mismatches that break the build or produce meaningless tests.

```
Step 1 — LOCATE SOURCE
  find .../CRM.Backend/src -name "ServiceName.cs" 2>/dev/null
  → confirm the exact file path; there may be multiple files with similar names

Step 2 — READ THE ENTIRE SOURCE FILE
  cat /full/path/ServiceName.cs
  → record: namespace, class name, ALL constructor parameters (exact types + names),
    ALL public method signatures (name, params, return type), any [Authorize] / [ApiController] attrs

Step 3 — READ THE SPEC (if one exists)
  find docs/11-specifications -name "SPEC-*" | xargs grep -l "ServiceName" 2>/dev/null
  → record: expected business rules, validation constraints, error responses

Step 4 — READ ANY EXISTING TEST FILE
  find .../tests/CRM.Tests -name "*ServiceNameTests.cs"
  → record: which methods are already tested; do NOT duplicate covered cases

Step 5 — CROSS-CHECK DTO SHAPES
  For each DTO used in the service, cat its source file
  → record: nullable vs non-nullable fields, required fields, enum types

Step 6 — WRITE TESTS
  → Only after steps 1–5 are complete
  → Mock ONLY the interfaces found in the real constructor (step 2)
  → Use InMemoryDatabase when ICrmDbContext or CrmDbContext is injected
  → Use real DTO types filled from step 5 data — never use anonymous objects

Step 7 — BUILD & RUN
  dotnet build tests/CRM.Tests/CRM.Tests.csproj -v q | grep "error CS"
  dotnet test tests/CRM.Tests/CRM.Tests.csproj --filter "FullyQualifiedName~ClassName" -v q
  → Fix ALL build errors before moving to next item

Step 8 — UPDATE SPEC + THIS DOC
  → Mark tested methods ✅ in the relevant SPEC-*.md
  → Change status column from ❌ to ✅ in this table
```

### TCOV Ground Rules

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

## Section 3B — TCOV2: Extended Coverage Plan (Measured 70% → 80%+)

> **Prerequisites before starting any TCOV2 item:**
> 1. Measure current coverage: `dotnet test tests/CRM.Tests/CRM.Tests.csproj --collect:"XPlat Code Coverage" --results-directory /tmp/cov-results && reportgenerator -reports:/tmp/cov-results/**/coverage.cobertura.xml -targetdir:/tmp/cov-html -reporttypes:Html`
> 2. Open `/tmp/cov-html/index.html` — identify the classes with the most uncovered lines.
> 3. Only target TCOV2 items whose measured coverage is actually < 30%. Skip any already above that.
>
> **Scope identified (March 10, 2026):**
> - `find CRM.Backend/src -name "*.cs"` yielded ~336 service files, ~209 controller files, ~50 provider files
> - `comm` comparison against existing test files: **188 controllers** and **242 services** have no dedicated test file
> - TCOV2 targets the highest-business-value subset from that list, grouped into 5 waves

---

### ⚠️ TCOV2 ANTI-MISMATCH PROTOCOL (Mandatory — Learned from Wave 4 Regressions)

Wave 4 provider tests had **4 files with build errors** due to wrong property names. The root cause was writing tests by _inferring_ field names rather than reading the actual source. TCOV2 items carry a zero-tolerance policy:

```
BEFORE writing any line of test code:

1. Read the source file in full:
   cat /full/path/TargetClass.cs | head -200
   → record: exact class name, exact namespace, exact constructor params (type + name),
     all public method names, all return types

2. Read every DTO/entity used:
   grep -rn "class.*Dto\|class.*Request\|class.*Response" SourceFile.cs
   cat /full/path/DtoClass.cs
   → record: exact property names (case-sensitive), nullable annotations (? suffix),
     default values, [Required] markers

3. Read the interface for each mocked dependency:
   cat /full/path/IServiceName.cs
   → record: exact method signatures including all overloads

4. Verify against spec if one exists:
   grep -rl "TargetClass\|topic" docs/11-specifications/
   cat the matching SPEC-*.md
   → record: expected validations, expected errors, expected data transformations

5. ONLY THEN write test code using the exact names found above

CHECKLIST FOR EACH TEST FILE BEFORE COMMITTING:
[ ] Class name in test matches class name in source (case-exact)
[ ] Namespace in test compiles without ambiguity error
[ ] Every mock uses an interface actually injected in constructor (not a concrete type)
[ ] Every DTO property access uses exact spelling from step 2
[ ] dotnet build tests/CRM.Tests/CRM.Tests.csproj 2>&1 | grep "error CS" returns EMPTY
[ ] dotnet test --filter "FullyQualifiedName~TargetClassTests" shows 0 failures
```

---

### TCOV2 Wave A — Core CRM Services (Target: +5–7% coverage)

> These are the highest-traffic business services. They are large, well-specified, and uncovered.
> Each item: read source + spec → use InMemory EF for DB-touching methods → mock external ports only.

| ID | Target Class | Est. Lines | Spec | Status |
|----|-------------|-----------|------|--------|
| TCOV2-A01 | `CRM.Infrastructure.Services.AccountService` | ~400 | SPEC-CRM-001 | ✅ |
| TCOV2-A02 | `CRM.Infrastructure.Services.ContactsService` | ~380 | SPEC-CRM-002 | ✅ |
| TCOV2-A03 | `CRM.Infrastructure.Services.OpportunitiesService` | ~350 | SPEC-CRM-004 | ✅ |
| TCOV2-A04 | `CRM.Infrastructure.Services.TaskService` | ~280 | SPEC-CRM-005 | ✅ |
| TCOV2-A05 | `CRM.Infrastructure.Services.WorkflowService` | ~320 | SPEC-WRK-001 | ✅ |
| TCOV2-A06 | `CRM.Infrastructure.Services.QuoteService` | ~290 | SPEC-SLS-003 | ✅ |
| TCOV2-A07 | `CRM.Infrastructure.Services.AccountContactService` | ~120 | SPEC-CRM-001 | ✅ |
| TCOV2-A08 | `CRM.Infrastructure.Services.LeadService` | ~250 | SPEC-CRM-003 | ✅ |
| TCOV2-A09 | `CRM.Infrastructure.Services.AuditLogService` | ~180 | — | ✅ |
| TCOV2-A10 | `CRM.Infrastructure.Services.ConversationService` | ~200 | — | ✅ |

**Implementation guidance for Wave A:**
- `AccountService` and `ContactsService` almost certainly inject `ICrmDbContext` — use `CrmDbContext` with `UseInMemoryDatabase("TestDb_{Guid.NewGuid()}")` to avoid test state leakage between test runs.
- For soft-delete: verify whether the service uses `IsDeleted = true` set on entity or calls a `SoftDelete` extension — read the source first.
- Workflow and Task services may use `IBackgroundJobClient` (Hangfire) or `IMediator` — confirm from source and mock accordingly.

---

### TCOV2 Wave B — Billing & Revenue Services (Target: +3–5% coverage)

> High-value financial services. Use stub HTTP handlers for Stripe/payment gateway calls.
> Never use live external credentials in tests.

| ID | Target Class | Est. Lines | Spec | Status |
|----|-------------|-----------|------|--------|
| TCOV2-B01 | `CRM.Infrastructure.Services.StripeIntegrationService` | ~300 | SPEC-SLS-007 | ✅ |
| TCOV2-B02 | `CRM.Infrastructure.Services.CommissionRulesEngine` | ~250 | SPEC-SLS-006 | ✅ |
| TCOV2-B03 | `CRM.Infrastructure.Services.RevenueAnalyticsService` | ~200 | — | ✅ |
| TCOV2-B04 | `CRM.Infrastructure.Services.SalesForecastService` | ~180 | SPEC-SLS-005 | ✅ |
| TCOV2-B05 | `CRM.Infrastructure.Services.ContractService` | ~160 | SPEC-SLS-004 | ✅ |
| TCOV2-B06 | `CRM.Infrastructure.Services.CurrencyService` | ~120 | — | ✅ |
| TCOV2-B07 | `CRM.Infrastructure.Services.TerritoryAssignmentService` | ~140 | SPEC-SLS-006 | ✅ |
| TCOV2-B08 | `CRM.Infrastructure.Services.SatisfactionService` | ~100 | — | ✅ |

**Implementation guidance for Wave B:**
- `StripeIntegrationService` likely wraps `Stripe.StripeClient` or uses its own `HttpClient` — read the source to determine: if it wraps `IStripeClient` mock that interface; if it uses `HttpClient` directly, inject via `IHttpClientFactory` stub.
- `CommissionRulesEngine` is probably a pure domain service — read to confirm; if so, test with real objects (no mocks needed) and table-driven test cases.
- `SalesForecastService` — check for `ICrmDbContext` injection; if present use InMemory DB. Seed realistic `Opportunity` + `OpportunityProduct` rows before asserting forecast totals.

---

### TCOV2 Wave C — ITSM & Change Management Services (Target: +2–3% coverage)

> ITSM services have published specs. Cross-reference SPEC-SD-* files for every behavior assertion.

| ID | Target Class | Est. Lines | Spec | Status |
|----|-------------|-----------|------|--------|
| TCOV2-C01 | `CRM.Infrastructure.Services.ITSM.ChangeManagementService` | ~280 | SPEC-SD-004 | ✅ |
| TCOV2-C02 | `CRM.Infrastructure.Services.ITSM.ServiceCatalogService` | ~220 | SPEC-SD-002 | ✅ |
| TCOV2-C03 | `CRM.Infrastructure.Services.ITSM.SLAAnalyticsService` | ~180 | SPEC-SD-003 | ✅ |
| TCOV2-C04 | `CRM.Infrastructure.Services.ITSM.CABWorkflowService` | ~150 | SPEC-SD-004 | ✅ |
| TCOV2-C05 | `CRM.Infrastructure.Services.KnowledgeBaseService` | ~200 | SPEC-SD-006 | ✅ |
| TCOV2-C06 | `CRM.Infrastructure.Services.ITSM.AssetManagementService` | ~160 | — | ✅ |

**Implementation guidance for Wave C:**
- All ITSM services should be checked against specs in `docs/11-specifications/` — run `grep -rl "ChangeManagement\|ServiceCatalog" docs/11-specifications/` to find the right SPEC file before asserting any business rule.
- `SLAAnalyticsService` — verify it uses `BusinessHoursCalculator` (likely injected). Read how biz-hour calculation works and seed test data that crosses midnight / weekends to validate edge cases.

---

### TCOV2 Wave D — High-Traffic Controllers (Target: +3–4% coverage)

> These controllers serve the most-used endpoints. Focus: correct HTTP status codes, route handling, auth attribute presence, and service delegation.
> Do NOT test business logic in controller tests — only test the HTTP contract.

| ID | Target Class | Actions | Spec | Status |
|----|-------------|---------|------|--------|
| TCOV2-D01 | `CRM.Api.Controllers.AccountsController` | ~12 | SPEC-CRM-001 | ✅ |
| TCOV2-D02 | `CRM.Api.Controllers.ContactsController` | ~12 | SPEC-CRM-002 | ✅ |
| TCOV2-D03 | `CRM.Api.Controllers.OpportunitiesController` | ~10 | SPEC-CRM-004 | ✅ |
| TCOV2-D04 | `CRM.Api.Controllers.UsersController` | ~10 | — | ✅ |
| TCOV2-D05 | `CRM.Api.Controllers.TasksController` | ~8 | SPEC-CRM-005 | ✅ |
| TCOV2-D06 | `CRM.Api.Controllers.ReportsController` | ~8 | — | ✅ |
| TCOV2-D07 | `CRM.Api.Controllers.CampaignsController` | ~10 | SPEC-MKT-001 | ✅ |
| TCOV2-D08 | `CRM.Api.Controllers.LeadsController` | ~10 | SPEC-CRM-003 | ✅ |
| TCOV2-D09 | `CRM.Api.Controllers.KnowledgeBaseController` | ~8 | SPEC-SD-006 | ✅ |
| TCOV2-D10 | `CRM.Api.Controllers.AnalyticsController` | ~8 | — | ✅ |
| TCOV2-D11 | `CRM.Api.Controllers.TerritoriesController` | ~8 | SPEC-SLS-006 | ✅ |
| TCOV2-D12 | `CRM.Api.Controllers.SalesForecastsController` | ~6 | SPEC-SLS-005 | ✅ |
| TCOV2-D13 | `CRM.Api.Controllers.CommissionsController` | ~8 | SPEC-SLS-006 | ✅ |
| TCOV2-D14 | `CRM.Api.Controllers.OrdersController` | ~8 | SPEC-SLS-002 | ✅ |
| TCOV2-D15 | `CRM.Api.Controllers.AuditLogsController` | ~6 | — | ✅ |

**Implementation guidance for Wave D:**
- Read each controller with `cat ControllerName.cs` before writing a single test. Controllers often have non-standard injection patterns (services, `ILogger`, `IMapper`, `ClaimsPrincipal`) that must be mocked correctly.
- Controller tests use `ControllerContext` + `DefaultHttpContext` + a populated `ClaimsPrincipal` to simulate authenticated requests. Read existing controller tests (e.g., `DashboardControllerTests.cs`) for the established pattern.
- Check `[Authorize(Roles = "...")]` vs `[Authorize(Policy = "...")]` — read the actual attribute on the controller action to assert the right behavior.
- Assert only: correct service method was called (Moq `Verify`), correct `OkObjectResult` / `NotFoundResult` / `BadRequestObjectResult` returned. Do not re-assert business logic.

---

### TCOV2 Wave E — Infrastructure & Cross-Cutting Services (Target: +2–3% coverage)

> Infrastructure services are often thin wrappers. Tests focus on correct delegation and error propagation.

| ID | Target Class | Est. Lines | Spec | Status |
|----|-------------|-----------|------|--------|
| TCOV2-E01 | `CRM.Infrastructure.Services.DomainEventPublisher` | ~100 | — | ✅ |
| TCOV2-E02 | `CRM.Infrastructure.Services.WebhookAnalyticsService` | ~120 | — | ✅ |
| TCOV2-E03 | `CRM.Infrastructure.Services.TotpService` | ~80 | — | ✅ |
| TCOV2-E04 | `CRM.Infrastructure.Services.DeadLetterQueueService` | ~90 | — | ✅ |
| TCOV2-E05 | `CRM.Infrastructure.Services.BusinessHoursCalculator` | ~150 | SPEC-SD-003 | ✅ |
| TCOV2-E06 | `CRM.Infrastructure.Services.ZipCodeService` | ~60 | — | ✅ |
| TCOV2-E07 | `CRM.Infrastructure.Services.CampaignExecutionService` | ~200 | SPEC-MKT-001 | ✅ |
| TCOV2-E08 | `CRM.Infrastructure.Services.AccountingSyncService` | ~140 | — | ✅ |

**Implementation guidance for Wave E:**
- `BusinessHoursCalculator` is a good candidate for pure/isolated tests — if it has no DB or external dependencies, write table-driven `[Theory]` tests for boundary conditions (start/end of business day, weekends, holidays). Confirm from source first.
- `TotpService` wraps a TOTP library — read if it uses `Otp.NET` or similar. Test secret generation, code validation, and expiry without a real authenticator app (use the library's own seed constants or fixed-time tests with a `ISystemClock` mock if present).
- `DomainEventPublisher` — likely uses `MediatR` or a custom event bus. Read source to determine: if `IMediator`, mock it; if `IEventBus`, mock that interface.

---

### TCOV2 Execution Checklist

> Apply to every TCOV2 item before marking ✅:

```
[ ] Source file read in full — class name, namespace, constructor confirmed
[ ] All DTOs/entities used in tests read from their own source files
[ ] All interfaces mocked from their real interface file (not guessed)
[ ] Spec checked — relevant SPEC-*.md section marked ✅ Tested
[ ] dotnet build tests/CRM.Tests/CRM.Tests.csproj | grep "error CS" → EMPTY
[ ] dotnet test --filter "FullyQualifiedName~{ClassName}Tests" → 0 failures
[ ] No tests duplicate test cases already in an existing test file
[ ] version.json patch bump done; minor bump if a full wave completes
[ ] Status in this table changed from ❌ to ✅
```

### TCOV2 Coverage Measurement Commands

```bash
# Step 1: Run with coverage collection
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend
dotnet test tests/CRM.Tests/CRM.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory /tmp/cov-results \
  --no-build

# Step 2: Install report generator if needed
dotnet tool install -g dotnet-reportgenerator-globaltool 2>/dev/null || true

# Step 3: Generate HTML + summary
reportgenerator \
  -reports:"/tmp/cov-results/**/coverage.cobertura.xml" \
  -targetdir:/tmp/cov-html \
  -reporttypes:"Html;TextSummary"

# Step 4: Check summary
cat /tmp/cov-html/Summary.txt | grep "Line coverage"

# Open full report (macOS)
open /tmp/cov-html/index.html
```

---

## Section 4 — Completed Work Detail

All completed item details have been archived to [docs/DONE_LOG.md](DONE_LOG.md).

---

**Document Maintained By:** GitHub Copilot
**Last Cleaned:** March 11, 2026 — TCOV2 Waves A–E complete (v0.625.0). 47 new items, ~370 new tests, 2,785 total passing, 0 failures. Bugs caught+fixed by anti-mismatch protocol: QuoteStatus enum value, IRepository.FindAsync signature, TotpService Base32 binary parsing, EF InMemory required-FK inner-join seeding.
**Current Version:** 0.625.0

