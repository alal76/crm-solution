# CRM Solution — Master TODO List

> **Last Updated:** August 9, 2026 (full end-to-end review — see [Section 2A](#section-2a--engineering-review-findings-august-6-2026) — followed by eight rounds of remediation work on `tech-debt-cleanup-aug2026`, see [Section 2B](#section-2b--findings-discovered-while-implementing-section-2a-august-6-2026); round 4 closed all 5 previously decision-gated items; round 5 was a documentation-accuracy + StyleCop cleanup pass; round 6 closed REV-DOC-005's remaining test-coverage gaps, REV-FE-004/007, REV-STUB-001, REV-ORPHAN-005, and REV-BUG-006's outbound Stripe half; round 7 closed the remaining Section 2A decision-driven batch; round 8 closed everything else still open in Section 2A — REV-FE-003 (new ReportTemplate backend), REV-STUB-012/013 (real DunningManager Stripe charges), the REV-FE-005/006/008 stale-finding corrections, and a full REV-STUB-014 investigation confirming all 29 archived files are genuinely dead)
> **Version:** 0.628.33 (branch `tech-debt-cleanup-aug2026`, not yet merged to `main`)
> **Single Source of Truth:** This file is the canonical backlog, gap register, remediation plan, and execution tracker for the repository. All active TODOs, gaps, remediation work, and implementation priorities should be recorded here.
> **Active Backlog:** 1 blocked (INT-003) + 1 deferred by architectural decision (XMOD-011) + Section 2A (27 items — **all closed**) + Section 2B (25 items — **all fixed/wired except one tooling-built-not-run item**) + 6 REV-FGAP-003–008 field-gap findings (**all 6 fixed**). Two follow-ups deliberately left open, both needing you rather than more agent work: actually running the Lead backfill + history-continuity tools against real data (built, tested, safe-by-default dry-run — single production database, no staging copy), and Stripe live-sandbox verification (REM-BUG-006's outbound wiring is code-complete but unverified against a live account — needs your API credentials). One newly-tracked, low-priority side finding from round 8's REV-STUB-014 investigation: ~12 active ITSM/commission services are DI-registered and tested but have no controller consumer (same pattern as the already-closed `REV-ORPHAN-*` findings) — not yet catalogued as formal findings, worth a future pass if this backlog is revisited.
> **Build:** ✅ 0 backend build errors across **all three** backend test projects (`tests/CRM.Tests.csproj`, `tests/CRM.Tests/CRM.Tests.csproj`, `tests/Unit/CRM.Tests.Unit.csproj`) | ✅ 0 live StyleCop SA warnings | ✅ Frontend `tsc --noEmit` clean | **Tests:** ✅ 7,596 + 3,121 + 79 = **10,796 backend tests passing, 0 failures** (re-verified fresh Aug 9 2026 after round 8 — up from 10,780; 17 skips in the Unit project are documented pre-existing pythonnet-assumption placeholders) | ✅ **1,162 of 1,163 frontend tests passing** (51 suites) — the 1 failure (`AccountOverviewPage`) is timeout-under-parallel-load flakiness, confirmed passing 3/3 when re-run in isolation, same pattern as previously-documented flakiness elsewhere in this doc, not a regression | **Coverage:** ~70% (TCOV-001–068 done) — AccountService/LeadsController/WorkflowController/AllenAIService method-level gaps closed in round 6 | **Next:** nothing decision-gated remains anywhere in this document; the only open items are the Lead-migration execution go-ahead and Stripe live-sandbox verification, both needing you
>
> **⚠️ Process note for future sessions on this branch:** subagents repeatedly reported work as "completed" that had actually been silently lost, reverted, or left in a broken intermediate state mid-session — this happened in both round 2 (`TasksController.cs`, `FeatureFlagsPanel.tsx`, `IntegrationsSettingsPage.tsx`, `OrderService`/`ContactsService`/`MergeService`) and round 3 (a stalled agent still left real, high-quality work on disk that just needed a manual build/test pass to confirm). Most likely cause: concurrent agents racing on the same working tree, combined with an environment auto-commit/checkpoint feature that fires mid-session outside normal `git commit` calls (confirmed once: commit `972eda3d` was not authored by any explicit commit in this session). **Do not trust an agent's self-report of "done" on this branch without independently re-reading the file and re-running the full build/test suite** — every round in this backlog was only actually closed after that independent verification, and it caught a real bug each time (a broken build, a NullReferenceException, an unpersisted feature). Round 4 added a new variant: two of five parallel agents ended their turn mid-verification ("waiting for the build monitor...") without ever reporting a result — resumable via sending them a follow-up message asking for the actual final build/test output, which they then provided correctly. Consider `isolation: 'worktree'` for parallel agents touching overlapping files next time.
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
| REV-BUILD-001 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 5). Was 255 live SA warnings (286 total); a mechanical sweep in round 3 brought it to 9 SA warnings / 42 total, with the remaining 9 left open pending individual judgment calls. Round 4's new code grew that to 12 unique warnings (24 total across both test projects). Round 5 resolved all 12 individually: missing `<summary>` tags on two obsolete-marked methods, a `file`-modifier SA1400 false positive (worked around via `internal`, no other `FakeHttpMessageHandler` in scope to collide with), `MockLogger`→`_mockLogger` casing in 3 classes, a missing copyright file header, 2 stray blank lines before closing braces, an extra closing-paren-on-own-line, a doc-header/code-element spacing issue, and one trailing embedded comment moved above its `foreach`. **0 SA warnings**, re-verified with `dotnet build CRM.sln --no-incremental` and a full test re-run (10,424 passing, unchanged). The v0.623.4 "0 SA warnings" entry (Section 1) was true at the time and warnings reaccumulated as expected — this is a periodic-recheck item, not a permanent fix; worth another pass after future feature work. | `dotnet build CRM.sln`, Aug 7 2026 (round 5) |
| REV-BUILD-002 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). Frontend production build failed under `CI=true` on one ESLint error: a `// NOSONAR` comment placed directly in JSX children instead of inside `{/* */}`. Wrapped in `{/* */}`; `npm run build` with `CI=true` now succeeds (re-verified). | `CRM.Frontend/src/pages/admin/MonitoringDashboard.tsx:1061` (`react/jsx-no-comment-textnodes`) |

**Untracked stub/incomplete integrations (P1/P2 — real code, not represented as open work anywhere in Section 2):**

| ID | Description |
|----|---|
| REV-STUB-001 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 6). `TwilioCallLoggingService` calls the real Twilio `CallResource.FetchAsync` API (21 tests from round 3) and now has a real caller: `POST /api/webhooks/twilio/call-status` (new, round 6) validates the `X-Twilio-Signature` header via `WhatsAppWebhookController.IsValidTwilioSignature` and routes call-status events to it. 8 new tests. (`Services/Integrations/TwilioCallLoggingService.cs`, `Controllers/Webhooks/TwilioWebhookController.cs`) |
| REV-STUB-002 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). `AccountingSyncService` (QuickBooks/Xero) rewired to `IProviderConfigurationService`. Discovery: real, already-tested `QuickBooksService`/`XeroService` OAuth2 clients already existed from prior INT-001/002/004 work — delegates to those rather than duplicating OAuth logic; only `SyncPaymentAsync` (no equivalent) makes its own real HTTP call. Registry entries added to `/admin/providers` (no new credentials UI needed — one already existed and was undiscovered by the original finding). Vendor free-tier/signup steps documented in `docs/INTEGRATION_SETUP.md` (untracked, matches this repo's docs/ convention) — no accounts created. |
| REV-STUB-003 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). `MarketingSyncService` (Mailchimp/HubSpot) — same treatment as REV-STUB-002, delegates to pre-existing real `MailchimpService`/`HubSpotService` where they overlap. HubSpot uses a private-app access token (simpler current recommended model). |
| REV-STUB-004 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). `LinkedInSalesNavService` messaging: added `SendInMailAsync`/`LinkedInMessageResult` to `ILinkedInSalesNavService` (no messaging method existed before) with a real OAuth2-authenticated implementation; `TestConnectionAsync` upgraded to a real `GET /v2/userinfo` call. INT-003's search/enrich scope untouched. |
| REV-STUB-005 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). `SchedulingIntegrationService` rewired to the pre-existing real `CalendlyService`; honest about Calendly's API having no create-event-type endpoint (finds the closest existing one rather than fabricating a link). |
| REV-STUB-006 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). Real, read-only connection-test calls for all 4 providers: AWS STS `GetCallerIdentity`, Azure `ClientSecretCredential` token + ARM subscription lookup, GCP service-account JWT-bearer exchange + Resource Manager project lookup, DigitalOcean `GET /v2/account`. Adds `AWSSDK.SecurityToken`/`Azure.Identity`/`Azure.ResourceManager`/`Google.Apis.Auth`. Explicitly still out of scope: deployment execution for these 4 providers (`TriggerDeploymentAsync` has no `DeployTo{Aws,Azure,Gcp,DigitalOcean}` — much larger effort, the separate CDT tool already covers this better). 20 new tests. |
| REV-STUB-007 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). `GeoLocationService` wired to MaxMind GeoIP2 Precision Insights (live web service, not a local GeoLite2 DB — `LookupAsync` isn't a hot path, and Insights is the only MaxMind product returning the VPN/Tor/hosting-provider traits this interface needs; tradeoff documented in code). Existing Haversine distance math untouched. |
| REV-STUB-008 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). Built as a subprocess-isolated Python sidecar (`python-script-runner/` at repo root, stdlib-only, mirrors `crm-script-runner`'s route shape), not in-process pythonnet — no proven in-process sandbox story for Python existed in this codebase. AST-based allowlist (not denylist) security gate rejects anything outside ~24 pure-computation modules, all dunder Name/Attribute access (closing the canonical `().__class__.__bases__[0].__subclasses__()` escape), dangerous builtins even when merely referenced, and dunder-like string literals as a backstop. Runs twice (pre-flight + inside the subprocess) for defense in depth, plus `RLIMIT_AS`/`RLIMIT_CPU`/`RLIMIT_NPROC=0`. Independently re-verified: 39/39 denylist tests re-run clean, 10 manually-probed attack vectors all correctly blocked. `class` definitions unsupported for v1 (pure functions only, documented scope-down). 37 new C# tests + 39 Python tests. |
| REV-STUB-009 | ✅ **Fixed for the generic pair** (`tech-debt-cleanup-aug2026`). `SlackNotificationService` and `TeamsNotificationService` now do real Incoming Webhook POSTs (Block Kit / Adaptive Cards), reusing the already-proven `TeamsNotificationChannelService`/ITSM pattern, config `Providers:Notifications:Slack/Teams:WebhookUrl` (20 new tests). **Correction to the original finding:** `TeamsNotificationChannelService` was already a real implementation, not a stub — it was misclassified above. `SmsNotificationService` was NOT touched by this fix and remains a stub. Also newly confirmed: none of `SlackNotificationService`/`TeamsNotificationService`/`TeamsNotificationChannelService` have a DI registration or any caller anywhere in the codebase — wired correctly now but still unreachable at runtime, same shape as REV-STUB-001. (`Services/Notifications/*.cs`) |
| REV-STUB-010 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). `PaymentTokenizationService` confirmed genuinely dead (no controller ever injects it) and marked `[Obsolete]` on both interface and implementation, pointing to `StripeIntegrationService`. Not deleted, DI registration untouched, per CLAUDE.md's do-not-delete-without-confirmation guidance. (`Services/PaymentTokenizationService.cs`) |
| REV-STUB-011 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). `ExecuteAsync` now loads real `CampaignRecipients` (confirmed already populated by existing `CampaignRecipientService.AddRecipientsAsync` — not a fabricated gap), sends via `INotificationPort.SendBulkEmailAsync`/`SendBulkSmsAsync`, and writes real `CampaignEmailTracking` rows. `StartCampaignAsync` enqueues via a new Hangfire job (`CampaignExecutionJob`, mirrors `ContractExpirationJob`). Real guarded `Pause`/`Resume` state transitions. Still out of scope, unchanged: `CustomerSegment.CriteriaJson` is still never evaluated to auto-populate recipients from a segment — recipients must be added explicitly by ID. 45 new tests. |
| REV-STUB-012 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 8). `SubscriptionBillingController`'s dunning retry endpoint calls real `IDunningManager.RetryFailedPaymentAsync` (round-of-origin fix). The remaining card-charge gap closed round 8: `DunningManager` now attempts a real off-session Stripe charge when `Subscription.StripeCustomerId`/`StripePaymentMethodId` are on file (new nullable fields, migration `AddSubscriptionStripePaymentFields`); falls through to the existing schedule-next-retry behavior when no payment method is on file (true for all subscriptions until checkout captures one — separate, out-of-scope task). 61 tests. (`Controllers/SubscriptionBillingController.cs`, `Services/DunningManager.cs`) |
| REV-STUB-013 | ✅ **Closed — finding was partly stale** (`tech-debt-cleanup-aug2026`, round 8). Recurring dunning processing IS actually scheduled — `DunningSchedulerService` is a real, registered `BackgroundService` (`Program.cs:875`) that calls `IDunningManager.ProcessDunningAsync` on a real interval; it's just not a Hangfire job specifically, which the original finding's phrasing implied was missing entirely. The card-charge half closed alongside REV-STUB-012 above. `DunningManager.cs.disabled`/`RecurringBillingEngine.cs.disabled` in `Services/archive/` confirmed genuinely dead — see REV-STUB-014. |
| REV-STUB-014 | ✅ **Investigated, confirmed** (`tech-debt-cleanup-aug2026`, round 8). All 29 archived `.cs.disabled` files (under `CRM.Api/archive/`, `CRM.Api/Controllers/archive/`, `Services/archive/`, `Services/ITSM/archive/`) individually verified: every one has a real, non-stub, same-named active reimplementation, DI-registered at minimum, 14 of 29 confirmed reached by a live controller/hosted service. None land in the "disabled mid-build-fix, never revisited" bucket the `ChangeManagementServiceEx` precedent worried about — that precedent (a file that was never itself disabled, just unwired) is unrelated to this list; two of the 29 (`ChangeManagementService.cs.disabled`, `CABWorkflowService.cs.disabled`) are in fact predecessor implementations `ChangeManagementServiceEx` superseded, both now `[Obsolete]` on the active side. Files themselves not deleted (per CLAUDE.md's don't-delete-without-confirmation policy) — deletion is now a safe, informed action pending your go-ahead, not more investigation. **Side finding, not yet tracked:** ~12 of the active ITSM replacements (`ArticleRecommendationService`, `AssetLifecycleService`, `AssignmentRulesEngine`, `CatalogApprovalService`, `CatalogFulfillmentService`, `ChangeCalendarService`, `ChangeImpactService`, `DiscoveryService`, `ImpactAnalysisService`, `KCSWorkflowService`, plus `CommissionRuleService`/`DiscountRuleService`) are DI-registered and tested but have no controller consumer — the same pattern as the 5 already-closed `REV-ORPHAN-*` findings, just not yet catalogued as new ones. |

**Untracked non-functional frontend UI (P2 — not represented as open work anywhere in Section 2):**

| ID | Description |
|----|---|
| REV-FE-001 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). Discovery: a full merge wizard (`components/duplicates/MergeDialog.tsx`) already existed, wired into `AccountsPage.tsx`'s bulk-select toolbar — building `AccountMergeDialog` would have duplicated it, so the dead commented-out export was removed (not aliased). `AccountHierarchyTree.tsx` was the genuinely missing piece: client-side grouping by `parentAccountId` (no `ChildAccounts` nav property exists) via `SimpleTreeView`, mirroring `KnowledgeCategoryManagementPage.tsx`'s existing pattern. Also added the missing single-account merge entry point ("Merge with..." + `EntitySelect` picker on `AccountOverviewPage`) reusing `MergeDialog`. 11 new tests. |
| REV-FE-002 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 7). New `EmailDigestConfig` entity + migration, `GET/PUT/POST-preview /api/users/me/email-digest`, content aggregation from real `ITaskService`/`IActivityService`/Lead/Opportunity queries, new hourly Hangfire `EmailDigestJob` (mirrors `ContractExpirationJob`) sending via the existing `INotificationPort`. `TeamPerformance`/`KpiSummary` are v1-scoped, documented-as-such approximations (no manager/direct-report hierarchy exists in this codebase, so `TeamPerformance` uses department co-membership as a proxy) rather than a full analytics build. 41 new tests (37 backend + 4 frontend). |
| REV-FE-003 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 8). New `ReportTemplate` entity + migration, `GET /api/reports/templates` + `POST .../{id}/apply` on `ReportsController`, seeded via `CoreDataSeederService` (same idempotent pattern as `SeedDepartmentsAsync`). Frontend's `mockTemplates` replaced with a real `reportTemplateService.ts`. 63 new backend tests + 4 frontend tests. |
| REV-FE-004 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 6). Wired to the existing (previously undiscovered) backend `GET /invoices/{id}/pdf` endpoint, mirroring `ContractDetailsPage.tsx`'s blob-download pattern exactly. Note: `PdfGenerationService` itself is a pre-existing stub returning placeholder content — this closes the frontend wiring gap only; real PDF content generation is a separate, unscoped decision (picking/adding a PDF library). 3 new tests. (`pages/InvoiceDetailsPage.tsx`, `services/invoiceService.ts`) |
| REV-FE-005 | ✅ **Closed — finding was stale** (`tech-debt-cleanup-aug2026`, round 8 doc correction, no code change needed). `handleSave` already calls the real `featureFlagService.updateFlag` — verified by reading the current file. Fixed in an earlier round without this row getting its ✅ marker. |
| REV-FE-006 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 8; health-check portion already fixed in an earlier round). The provider-health cards already loaded real data via `providerHealthService.getProviderHealth()`. The remaining stub part — static "Coming Soon" cards for QuickBooks/Mailchimp/Calendly/LinkedIn — closed in round 8: now real cards linking to `/admin/providers`, extended to Xero/HubSpot too. See REV-FE round-8 entries in the Section 2B-adjacent round-8 summary below. |
| REV-FE-007 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 6). This finding was stale — per-filter inline validation (`filterErrors` state, `validateFilterValue`, `error`/`helperText` wiring) was already fully built. The one genuinely missing piece — nothing blocked Save/Run while an invalid filter value was still in place — is now fixed via a memoized `hasFilterErrors` check (active filters only) guarding both button `disabled` state and the handlers themselves. 5 tests. (`components/analytics/ReportDesigner.tsx`) |
| REV-FE-008 | ✅ **Closed — finding was stale** (`tech-debt-cleanup-aug2026`, round 8 doc correction, no code change needed). This was already fully implemented: `handleNavDragEnd` reorders and persists to `localStorage` (`crm-nav-order` key), a `sortedCategories` memo applies the persisted order on top of `navConfig`'s default order, and the render path uses real `DragDropContext`/`Draggable` from `@hello-pangea/dnd`. Verified by reading the current file — the leftover `TODO-SYS007-003` comments no longer reflect the code beneath them. |

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
| REV-DOC-005 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 6). TCOV/TCOV2 ✅ markers are honest about file-exists/passes but overstated method-level completeness. `MergeService`'s zero-coverage gap was closed in round 3 (21 tests, uncovering **REV-BUG-001/002**). Round 6 closed the remaining four: `AccountService` 3→24 of 24 public methods (30 new tests), `LeadsController` 5→16 of 16 endpoints (23 new tests), `WorkflowController` 6→35 of 35 actions (50 new tests), `AllenAIService` 5→17 of 17 methods (22 new tests). All HTTP/AI-provider calls mocked, no live network calls. Treat "✅" in Section 3/3B as "a test file exists and passes, and now genuinely covers the public surface" for these four classes specifically — the general caveat about TCOV/TCOV2 markers elsewhere in this doc still stands for classes not explicitly re-verified. |

**Recommended next actions:** triage remaining REV-STUB-* and REV-FE-* into the phased plan (most are P1/P2 — high-value but non-blocking stubs), and — now the higher priority given Section 2B — decide the fate of each orphaned-but-tested implementation (REV-ORPHAN-\*) before adding any more parallel logic to those modules.

---

## Section 2B — Findings Discovered While Implementing Section 2A (August 6, 2026)

> **How these were found:** while implementing the Section 2A fixes on branch `tech-debt-cleanup-aug2026` (DTO gaps, notification/Twilio/dunning wiring, MergeService coverage, and authoring the 13 specs from REV-DOC-001), the agents doing the work read deeply enough into each area to surface problems well beyond what the original review's spot-checks caught. These are generally more severe than the Section 2A findings — several are silent data-integrity bugs or "tested" code that the live application never actually runs. Nothing in this section was invented or spot-checked from a distance; each finding cites the spec or commit where it was verified against real code.

### REV-ORPHAN-\* — Fully-implemented, tested services the live API doesn't call

A single pattern shows up **five separate times** across unrelated modules: a well-built, unit-tested service exists, is even DI-registered, but the actual controller serving that module's live API either never calls it or was never wired to it at all — instead duplicating (often more thinly, sometimes untested) logic directly against `CrmDbContext`. This is worth fixing as its own workstream, not module-by-module, because it's clearly a repeated process gap (services built before/without checking what the controller already does) rather than five unrelated bugs.

| ID | Description | Source |
|----|---|---|
| REV-ORPHAN-001 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 4). Research confirmed the frontend (`changeService.ts`) already called `/schedule`, `/complete`, `/rollback`, `/comments`, `/calendar`, `/statistics`, `/conflicts`, `/status`, `/submit-approval`, `/activity`, `/related-incidents` — none of which existed on the live controller, so they 404'd. `ChangesController` now depends entirely on `IChangeManagementServiceEx`, with a `ChangeContractMapper` aligning response shapes to the frontend's existing contract (no frontend changes needed). `ChangeService`/`IChangeService`, `ChangeManagementService`, `CABWorkflowService`/`ICABWorkflowService` marked `[Obsolete]`, not deleted. 129+54 tests passing across both test projects. | SPEC-SD-004-ChangeManagement.md |
| REV-ORPHAN-002 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 4). Research found the live `CommissionsController` actually used a fourth, separate `CommissionService`/`CommissionPlan`+`CommissionTier` model that was flat-rate only, ignoring its own tier data. `CommissionsController`'s calculate/deal and calculate/order endpoints now route through `ICommissionRulesEngine` (tiered rates, caps, team splits, trigger-event gating); CRUD/plan/tier/statement endpoints untouched. `CommissionService`'s flat-rate calculation methods marked `[Obsolete]`. `TerritoryAssignmentService`/interface also marked `[Obsolete]` — confirmed a stub (`MatchesTerritoryRules` unconditionally returns `true`) strictly redundant with live `TerritoryService`. 172+36 tests passing. | SPEC-SLS-006-CommissionTerritoryManagement.md |
| REV-ORPHAN-003 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 4). Decision: migrate the UI onto the richer `Lead` system. `LeadsPage.tsx` rewritten against `/api/leads` — `notes` JSON pack/unpack removed, field names remapped, ad-hoc convert flow replaced with `POST /leads/{id}/convert`, SignalR subscription moved to `'Lead'`. Side effect: AI lead-scoring (`/ai/leads/{id}/score`) already operated on real `Lead.Id`, so this migration fixes a previously-silent ID mismatch. `ContactInfoPanel`/`NotesTab`/`RecordComments` were already entity-type-generic, no changes needed. A companion idempotent backfill tool (`LeadBackfillService`, admin-only, dry-run by default) converts legacy Contact-as-Lead rows into real `Lead` rows using `Lead.ContactId` for idempotency — **built and tested (10 tests) but deliberately NOT run against real data**, see the Section 2B Remediation Plan note below. `ContactsController`/`ContactsService`/`/contacts/type/Lead` left untouched (dead-but-present). | SPEC-CRM-003-LeadManagement.md |
| REV-ORPHAN-004 | ✅ **Fixed** (`tech-debt-cleanup-aug2026`). `QuotesController` now delegates all CRUD/lifecycle operations to `IQuoteService` instead of reimplementing them against `CrmDbContext` directly. 3 narrow, justified additions to `QuoteService`/`IQuoteService` along the way: `Include()`s so `AccountName`/`ContactName`/`ProductName` still populate after delegating; a new `MarkViewedAsync`; extended `UpdateAsync`'s copied-fields list to include signature/lifecycle-date fields (this also fixed a pre-existing dormant bug in `DocuSealWebhookController`, which already tried to set those fields but they were silently dropped). `CrmDbContext` is still used directly only for line-item endpoints and PDF generation, which `IQuoteService` has no methods for — documented as intentionally out of scope. 38/38 and 168/168 Quote-filtered tests pass across both test projects. | SPEC-SLS-003-QuoteManagement.md |
| REV-ORPHAN-005 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 6). Slack/Teams: real caller via `EscalationRuleService.ExecuteRuleActionAsync` (round 3). Twilio: previously blocked on no call-status-callback webhook existing — round 6 added `POST /api/webhooks/twilio/call-status` (signature-validated), giving `ITwilioCallLoggingService` a real caller. All three notification services now DI-registered and reachable. | Commits on `tech-debt-cleanup-aug2026`, this section |
| REV-ORPHAN-006 | 🔶 **In progress** (`tech-debt-cleanup-aug2026`, round 9). Formalizes the round-8 REV-STUB-014 side finding: ~12 active ITSM/commission services DI-registered and tested but unreached by any controller. Split into safe wire-ups (no competing implementation) and decision-gated items (a live competitor already exists — not safe to wire unilaterally, same rule as REM-ORPHAN-001/002). **`CommissionRuleService`/`ICommissionRuleService` — done.** Investigation found its CRUD methods (`Create/Update/GetById/GetAll/Delete/GetApplicableRulesAsync`) do NOT compete with `ICommissionRulesEngine` (the round-4 REM-ORPHAN-002 winner, which is calculation-only with no CRUD) — so only `CalculateCommissionAsync` was marked `[Obsolete]` (mirroring `CommissionService`'s round-4 precedent of obsoleting just the competing method, not the whole class), and the non-competing CRUD methods were wired into `CommissionsController` as new `GET/POST/PUT/DELETE api/commissions/rules[/{id}]` + `GET api/commissions/rules/applicable/{saleType}` endpoints — closing a genuine, previously-nonexistent way to manage `CommissionRule` records via the API. 12 new controller tests added across both test projects (`tests/Controllers/ServiceControllersTests.cs`, `tests/CRM.Tests/Controllers/CommissionsControllerTests.cs`), all passing. Remaining, not yet started: safe wire-ups for `ChangeCalendarService`, `DiscountRuleService`, `AssetLifecycleService`, `CatalogApprovalService`/`CatalogFulfillmentService`, `DiscoveryService`; decision-gated items for `AssignmentRulesEngine` (vs. `AutoAssignmentService`), `ChangeImpactService`/`ImpactAnalysisService` (vs. `CMDBController`'s shallow existing impact endpoint), `ArticleRecommendationService`/`KCSWorkflowService` (vs. `KnowledgeController`'s existing workflow). | SPEC-SLS-006-CommissionTerritoryManagement.md |

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
| REV-BUG-006 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 6). Round 4: webhook fails closed on unconfigured `STRIPE_WEBHOOK_SECRET` (13 tests). Round 6, per explicit decision: wired real `Stripe.net` 52.2.0 SDK calls for all six outbound methods (create/confirm/capture/cancel PaymentIntent, charge, refund) via `PaymentIntentService`/`ChargeService`/`RefundService`, using a DI-injected `HttpClient` as Stripe.net's supported test seam. 26 tests, all HTTP-mocked — **cannot be verified against a live Stripe sandbox without real API credentials**, which aren't available in this environment; the implementation and error handling (StripeException mapped to the existing failure-DTO convention) are real, end-to-end verification against Stripe's actual API is the one thing still outstanding and needs your credentials to close. |
| REV-BUG-007 | `territoryService.ts`'s "Auto-Assign" button calls a route that doesn't exist on the backend. Three separate frontend `OrderStatus` enums exist, one (`types/sales.ts`) with numeric values that don't match the backend at all — contradicts `FIELD_GAP_REMEDIATION_PLAN.md`'s "Complete"/"mapped" claim for Order. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — the Order-enum consolidation and the `territoryService.ts` dead-route removal are both done (the button/handler/type were removed rather than inventing a bulk endpoint; a real per-account auto-assign endpoint exists and is noted in a comment). |
| REV-BUG-008 | `CrmTask` write DTOs (`CreateCrmTaskDto`/`UpdateCrmTaskDto`) still lack `TaskType`/`StartDate` (update also lacks `AccountId`/`OpportunityId`), even though the read DTO now has all 5 after REV-FGAP-002. Narrower, separate gap — a task's type/start date can be read back but not set via the API. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — both write DTOs now have all the fields, wired through `TasksController`'s create/update mapping. |
| REV-BUG-009 | `Contact`: `PreferredContactMethod` is accepted in request DTOs but silently dropped — never written to the entity. `Update`/`Delete`/social-media endpoints have zero unit-test coverage at both service and controller layers. Account-assignment methods exist on `ContactsService` but have no controller route. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — `PreferredContactMethod`/`DoNotContact` persistence (round 2), real test coverage for `UpdateAsync`/`DeleteAsync`/social-media-link methods at both service and controller layers, and `POST /api/contacts/{id}/account/{accountId}` + `DELETE /api/contacts/{id}/account` routes for the account-assignment methods (round 3). |
| REV-BUG-010 | `TasksController` bypasses `TaskService`/`ITaskService` entirely — queries `CrmDbContext` directly with its own duplicate mapping logic. The two paths disagree on delete semantics: the controller hard-deletes, the service soft-deletes. | ✅ **Fixed** on `tech-debt-cleanup-aug2026` — `TasksController` now delegates Create/Get/Update/Delete/Complete to `ITaskService` (soft-delete included); `CrmDbContext` retained only for the `my-queue` endpoint, which needs `UserGroup` joins `ITaskService` doesn't expose. Regression test confirms delete soft-deletes (row still exists, `IsDeleted=true`, not visible via `GetTask`). |
| REV-BUG-011 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 4). Built the full frontend: `salesForecastService.ts` (all 9 routes) + `SalesForecastsPage.tsx` (KPI cards, Recharts trend via the history endpoint, filterable list, line-item drill-down grouped by `ForecastCategory`, submit action), modeled on `RevenueAnalyticsPage.tsx`. Route + nav entry registered (category `sales`). 17 new tests passing, `tsc --noEmit` clean. |

### REV-FGAP-\* — Additional field-gap findings from the FIELD_GAP_REMEDIATION_PLAN.md re-audit (REV-DOC-003)

Found while re-verifying the 9 entities not covered by the original Aug 6 review (see [FIELD_GAP_REMEDIATION_PLAN.md](FIELD_GAP_REMEDIATION_PLAN.md) for full detail per entity). Same "DTO/FE-type silently drops a field" shape as REV-FGAP-001/002, just not yet fixed in code.

| ID | Description | Status |
|----|---|---|
| REV-FGAP-003 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 4). Added the ~24 missing fields (attribution, BANT, MEDDIC, `QualificationFrameworkType`, `NurtureCampaignId`, `TerritoryId`, etc.) to `LeadDto`/`LeadSummaryDto` and `LeadService`'s mapping. 3 new tests, 64/64 `LeadService` tests passing. Surfaced read-only in `LeadsPage.tsx`'s new "Qualification & Attribution" tab as part of REV-ORPHAN-003's frontend migration. |
| REV-FGAP-004 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 3; doc status corrected round 5). `Quote`: the 8 fields (`warrantyEndDate`, `termsAndConditions`, `expectedDeliveryDate`, `actualDeliveryDate`, `serviceStartDate`, `serviceEndDate`, `attachments`, `customFields`) added to `QuoteDto` and `QuotesController.MapToDto` — verified present in `QuoteDtos.cs`. See REM-FGAP-004. |
| REV-FGAP-005 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 3; doc status corrected round 5). `Payment`: the 14 missing fields (including `fraudFlagged`, `riskScore`, `retryCount`) added to the frontend `Payment` type — verified present in `types/sales.ts`. See REM-FGAP-005. |
| REV-FGAP-006 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 3; doc status corrected round 5). `Contract`: dead `AnnualValue`/`RenewalTermMonths` fields removed from `ContractDto` (confirmed zero frontend references, no live DB to migrate) — verified absent from `ContractDto.cs`. See REM-FGAP-006. |
| REV-FGAP-007 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 3; doc status corrected round 5). `ServiceRequest`: `DueDate`, `StatusCode`, `LastModifiedByUserId`, `ConversationId` added to `ServiceRequestDto` — verified present in `ServiceRequestDto.cs`. See REM-FGAP-007. |
| REV-FGAP-008 | ✅ **Closed** (`tech-debt-cleanup-aug2026`, round 3; doc status corrected round 5). `User`: `PasswordNeverSet`/`CommissionPlanId` added to `UserDto`; frontend `User` type consolidated. See REM-FGAP-008. |

### Section 2B Remediation Plan

Each open finding gets one concrete remediation item below — priority, size, and whether it needs a human decision before an agent/engineer can safely start (the REV-ORPHAN-\* items in particular are NOT safe to "just fix" unilaterally: picking the wrong side to delete loses real functionality, e.g. the tested `Lead`/`QuoteService`/`ChangeManagementService` implementations are in some ways *better* than what the live controller does today).

| ID | Status | Priority | Effort | Proposed Remediation | Decision Needed Before Starting |
|----|--------|----------|--------|------------------------|----------------------------------|
| REM-ORPHAN-001 | ✅ Fixed (round 4) | P1 | L | Decision made after a dedicated research pass: wire `ChangeManagementServiceEx` in (the frontend already called routes only it implements; git history showed it was the intended real implementation, disabled mid-build-fix, never wired). Done — see REV-ORPHAN-001. | Resolved. |
| REM-ORPHAN-002 | ✅ Fixed (round 4) | P1 | L | Decision made after a dedicated comparison pass: wire `CommissionRulesEngine` into `CommissionsController` (most complete of the 3 unwired engines; live `CommissionService` was flat-rate only, ignoring its own tier data). `TerritoryAssignmentService` marked `[Obsolete]` (confirmed stub, strictly redundant with live `TerritoryService`). Done — see REV-ORPHAN-002. Business-rules validation of the new tier/cap/split logic against the actual comp plan is still worth a sales-ops sanity check, but the engineering decision is closed. | Resolved. |
| REM-ORPHAN-003 | ✅ Fixed (round 4) | P2 | L | Decision made: migrate the Contacts-as-Leads UI onto the richer `Lead`/`LeadService` system. Done — see REV-ORPHAN-003. Backfill tooling built and tested but deliberately not run against real data (see the new follow-up row below). | Resolved. |
| REM-ORPHAN-004 | ✅ **Fixed** | P1 | M | ~~Wire `QuotesController` to `QuoteService`~~ — done, paired with REM-TESTFAKE-001. | — |
| REM-ORPHAN-005 | ✅ Fixed (round 6) | P2 | S | Slack/Teams: done round 3. Twilio: done round 6 — `POST /api/webhooks/twilio/call-status` gives `ITwilioCallLoggingService` a real, signature-validated caller. | Resolved. |
| REM-TESTFAKE-001 | ✅ **Fixed** | P0 | S | ~~Rewrite `QuotesControllerTests.cs` with real assertions~~ — done, both copies of the file. | — |
| REM-BUG-003 | ✅ **Fixed** | P2 | S | ~~Add cycle-safe serialization to `GetRecordSnapshotAsync`~~ — done (`ReferenceHandler.IgnoreCycles`). | — |
| REM-BUG-004 | ✅ **Fixed** | P1 | S | ~~Fix the `.Include()` property name~~ — done, plus a real (non-mocked) regression test. | — |
| REM-BUG-005 | ✅ **Fixed** | P1 | M | ~~Fix the 4 confirmed Contract frontend/backend mismatches~~ — done (Renew body, signature routes, consolidated `Contract` type, dead upload/download UI removed). | — |
| REM-BUG-006 | ✅ Fixed (round 6) | P2 | L | Decision: commit to real Stripe.NET integration for outbound calls. Done — see REV-BUG-006. Real end-to-end verification against a live Stripe sandbox still needs your credentials; the code/tests are real and complete. | Resolved (pending live-sandbox verification with real credentials). |
| REM-BUG-007 | ✅ **Fixed** | P2 | S | ~~Order-enum consolidation~~ and ~~`territoryService.ts`'s dead "Auto-Assign" route~~ — both done. The dead button/handler/type were removed (no matching backend route exists — a per-account endpoint does, noted in a comment) rather than inventing a bulk endpoint. | — |
| REM-BUG-008 | ✅ **Fixed** | P2 | S | ~~Add the missing write-DTO fields~~ — done, mirroring the REV-FGAP-002 pattern. | — |
| REM-BUG-009 | ✅ **Fixed** | P2 | M | ~~Wire `PreferredContactMethod`~~ (done in round 2) ~~, add missing test coverage, add the account-assignment route~~ — done in round 3: real tests for `UpdateAsync`/`DeleteAsync`/social-media-link methods at both service and controller layers, plus `POST /api/contacts/{id}/account/{accountId}` and `DELETE /api/contacts/{id}/account` routes for the previously-orphaned `AssignToAccountAsync`/`UnassignFromAccountAsync`. | — |
| REM-BUG-010 | ✅ **Fixed** | P0 | M | ~~Fix the delete-semantics inconsistency~~ — done; `TasksController` now delegates to `TaskService` (soft-delete), with a regression test guarding against the hard-delete regression recurring. | — |
| REM-BUG-011 | ✅ Fixed (round 4) | P3 | L | Decision: build the full frontend. Done — see REV-BUG-011. | Resolved. |
| REM-FGAP-003 | ✅ Fixed (round 4) | P1 | M | Bundled with REM-ORPHAN-003 as planned. Done — see REV-FGAP-003. | Resolved. |
| REM-LEAD-BACKFILL-EXEC | ❌ Open (new, round 4) | P2 | S | Actually run the Lead backfill (`POST /api/admin/lead-backfill?dryRun=false`) against the real database to migrate existing Contact-as-Lead rows into real `Lead` rows. Tooling is built, tested (10 EF-InMemory tests), and safe-by-default (dry-run unless explicitly overridden), but was deliberately not executed — this repo has a single production database (no staging copy), so a bulk data-creation operation across all legacy lead Contacts needs an explicit human go-ahead, not an agent's judgment call. | **Yes** — run `?dryRun=true` first, review the reported counts/parse-errors/unmapped-enum-values, then explicitly ask for the real run. |
| REM-LEAD-HISTORY-CONTINUITY | ⚠️ Tooling built (round 6), not yet run | P3 | M | `LeadHistoryContinuityService` re-parents Activity/RecordComment rows from `EntityType="Contact"` to `EntityType="Lead"` for backfill-migrated leads, mirroring `LeadBackfillService`'s dry-run/idempotent conventions exactly. Admin-only `POST /api/admin/lead-backfill/history-continuity?dryRun=true` (safe by default). 8 EF-InMemory tests. **Not run against real data** — same single-production-database reasoning as REM-LEAD-BACKFILL-EXEC below; it only makes sense to run after the backfill itself runs. | Bundled with REM-LEAD-BACKFILL-EXEC's go/no-go — run backfill first, then this. |
| REM-FGAP-004 | ✅ **Fixed** | P2 | S | ~~Add the 8 missing fields to `QuoteDto`~~ — done. All 8 existed on the `Quote` entity; added to the DTO and `QuotesController.MapToDto`, no frontend type change needed. | — |
| REM-FGAP-005 | ✅ **Fixed** | P2 | S | ~~Add the 14 missing fields to the frontend `Payment` type~~ — done. Confirmed none are on the security-exclusion list before adding. | — |
| REM-FGAP-006 | ✅ **Fixed** | P2 | S | ~~Wire or remove `ContractDto.AnnualValue`/`RenewalTermMonths`~~ — removed. Confirmed zero frontend references to either field and no live DB to run a migration against even if wiring were the right call; also removed 15 tests that specifically validated these two dead fields (genuinely dead test code, not lost coverage). | — |
| REM-FGAP-007 | ✅ **Fixed** | P2 | S | ~~Add `DueDate`, `StatusCode`, `LastModifiedByUserId`, `ConversationId` to `ServiceRequestDto`~~ — done. Also fixed a pre-existing test-setup gap the new test surfaced: the shared `ServiceRequestServiceTests` mock context never set up the `ServiceRequestCustomFieldValues` DbSet, causing an NRE on any test exercising the full `GetServiceRequestByIdAsync` mapping path. | — |
| REM-FGAP-008 | ✅ **Fixed** | P3 | M | ~~Add `PasswordNeverSet`/`CommissionPlanId` to `UserDto`; add the missing fields to the frontend `User` type; consolidate `UserManagementPage.tsx`'s duplicate~~ — all done. The page's local type now `extends Omit<SharedUser, 'username' \| 'role'>`, narrowing only the two fields it genuinely needs typed differently, instead of re-declaring ~15 fields. | — |

**Sequencing:** as of the round-7 pass, all 5 originally decision-gated Section 2B items are closed, plus REM-ORPHAN-005's Twilio half and REM-LEAD-HISTORY-CONTINUITY's tooling (round 6), plus the full Section 2A decision-driven batch (REV-FE-001/002, REV-STUB-002/003/004/005/006/007/008/011 — round 7). Every non-obvious item was decided explicitly by the user (via research-backed multiple-choice questions, never a blind agent call) before any code was written; round 7's research turned up two premise corrections worth remembering for future audits — REV-FE-001's merge UI was mostly already built (`MergeDialog.tsx`), and a full credentials-management UI already existed (`ProvidersPage.tsx`) before REV-STUB-002/003/004/005/007 assumed one needed building — both cases where the original finding's "missing" claim was checked with too shallow a grep. What remains genuinely open in this entire document: REM-LEAD-BACKFILL-EXEC and REM-LEAD-HISTORY-CONTINUITY's actual execution against real data (tools built/tested/safe-by-default, deliberately not run — single production database, no staging copy, needs your explicit go-ahead), and REM-BUG-006's live-sandbox verification (code-complete, needs your real Stripe credentials to verify end-to-end). Both need you, not more agent work.

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

