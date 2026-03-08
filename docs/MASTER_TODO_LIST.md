# CRM Solution — Master TODO List

> **Last Updated:** March 8, 2026 (restructured)
> **Version:** 0.617.0
> **Active Backlog:** 90 items — 4 P0 critical + 14 P1 pre-GA + 72 P2/P3 post-GA
> **Build:** ✅ 0 errors (backend + frontend) | **Tests:** ✅ 4,818+ passing, 22 skipped

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
| **Total Completed** | **~995** | | |

---

## Section 2 — Active Backlog

### P0 — Critical (Fix Before Any Production Deployment)

> Carry data corruption or thread-pool starvation risk. All can run in parallel.

| ID | File | Issue | Fix |
|----|------|-------|-----|
| AP-004 | `ServiceRequestService.cs` L35-36 | `static int _ticketCounter` + `static bool _counterInitialized` not atomic | Use `Interlocked.Increment`; add `volatile` to flag |
| AP-015 | 7 Provider Factories: Search, Chat, AI, Notification, Analytics, Signature, Integration | Each calls `.IsEnabledAsync().GetAwaiter().GetResult()` in factory creation — blocks thread pool | Cache feature flag at startup via `IHostedService`, or expose `CreateAsync()` factory |
| AP-016 | `OptionalAuditLoggingService.cs` L172 | `IsEnabledAsync(...).GetAwaiter().GetResult()` in sync path | Cache on startup or make caller async |
| AP-017 | `AdminSearchAnalyticsController.cs` L113-116 | `.Result` on completed tasks after `Task.WhenAll` | Replace with `await` destructuring |

---

### P1 — Pre-GA Blockers

> Groups B1–B5 are independent; all groups can run in parallel.

#### Group B1 — Obsolete Attribute Cleanup (PRA-001→003)

| ID | File | Issue | Fix |
|----|------|-------|-----|
| PRA-001 | Auth DTOs | `ForgotPasswordRequestDto`, `ResetPasswordRequestDto`, `VerifyResetTokenRequestDto` marked `[Obsolete]` but actively bound to controller endpoints | Remove `[Obsolete]` attributes from all 3 DTOs |
| PRA-002 | `SystemSettingsDto.cs` | `ShowDemoData` still returned in API response | Remove from DTO mapping |
| PRA-003 | `SystemSettings.cs` | `SampleDataSeeded` + `SampleDataLastSeeded` marked `[Obsolete]` but are intentional seed-tracking fields | Remove `[Obsolete]` attributes |

#### Group B2 — Hardcoded Data / Stub Stubs (PRA-004→005)

| ID | File | Issue | Fix |
|----|------|-------|-----|
| PRA-004 | `WorkflowController.cs` | 12 field schemas hardcoded inline — schema drift risk | Migrate to `IFieldConfigurationService` or a config file |
| PRA-005 | `MonitoringController.cs` `GetActiveSessions()` | Always returns empty list — misleading in production monitoring | Implement Redis/auth store query, or add `// stub` doc comment |

#### Group B3 — Service Locator Pattern (AP-018→019)

| ID | File | Issue | Fix |
|----|------|-------|-----|
| AP-018 | `ContactInfoController.cs` L708, 740, 783 | Resolves `IContactInfoValidationService` from `HttpContext.RequestServices` at runtime | Move to constructor injection |
| AP-019 | `Program.cs` | 11 instances of `GetService`/`GetRequiredService` in seeding and factory lambdas | Extract to `AddXyzServices()` extension methods |

#### Group B4 — Fat Controllers (AP-020→023, all independent)

| ID | Controller | Issue | Extract To |
|----|-----------|-------|-----------|
| AP-020 | `QuotesController.cs` L68-107 | DB query logic in controller (`Where/Select/Include`) | `IQuoteService.GetFilteredAsync()` |
| AP-021 | `SLAPoliciesController.cs` L239-297 | Complex `GroupBy()` analytics aggregation | `ISLAAnalyticsService` |
| AP-022 | `SubscriptionUsageController.cs` L83-249 | Inline usage metrics aggregation | `ISubscriptionUsageService` |
| AP-023 | `PipelinesController.cs` L82-83 | `GroupBy/Select` pipeline analysis | `IPipelineAnalyticsService` |

#### Group B5 — Missing Input Validation at API Boundary (AP-024→027, all independent)

| ID | Endpoint | Add |
|----|---------|-----|
| AP-024 | `POST /api/subscriptions` | `[Required]`, `[Range]` on `CreateSubscriptionDto` |
| AP-025 | `POST /api/payments` | Validation on amounts, dates, methods |
| AP-026 | `POST /api/dunning-schedules` | Validation on dates, amounts |
| AP-027 | `POST /api/service-requests` | Required field checks |

---

### P2 — Post-GA Improvements (Active Code Work)

> Organized by dependency. Work within each group is mostly parallel unless noted.

#### Frontend Type Consolidation (PRA-006→007, parallel)

| ID | Issue | Fix |
|----|-------|-----|
| PRA-006 | 5 pages define local `interface Customer` instead of shared `Account` type from `types/` | Import shared `Account` type in all 5 pages |
| PRA-007 | `validation.ts` has `customerSchema` missing 5 fields (`firstName`, `lastName`, `customerCategory`, `customerType`, `annualRevenue`) | Rename to `accountSchema`; add missing Yup validations |

#### Test Suite Re-enablement (PRA-008, PRA-016, PRA-017)

| ID | Issue | Fix |
|----|-------|-----|
| PRA-008 | 3 fully disabled test files + 5 partially disabled test blocks | Fix compilation issues; remove skip markers |
| PRA-016 | `AccountServiceIntegrationTests.cs` — `Expression<Func>` vs `Func` signature mismatch | Fix mock setup signatures |
| PRA-017 | `QuoteServiceTests.cs` — missing `UpdateStatusAsync` method | Verify `IQuoteService`; add or stub method |

#### Orphaned Code (PRA-011, independent)

| ID | Issue | Fix |
|----|-------|-----|
| PRA-011 | `ContractExpirationJob` defined but never registered in DI or Hangfire | Wire to `IHostedService` or `AddHangfireJob()`; or delete if dead code |

#### CancellationToken Propagation (AP-028→031, all independent)

| ID | File | Missing Calls |
|----|------|---------------|
| AP-028 | `CommunicationService.cs` | `SaveChangesAsync()` at L134, 207, 237, 293, 419 |
| AP-029 | `MasterDataController.cs` | `SaveChangesAsync()` at L326, 352 |
| AP-030 | `UsersController.cs` | `SaveChangesAsync()` at L134, 782 |
| AP-031 | `ApiUsersController.cs` | `SaveChangesAsync()` at L162, 203, 239, 303, 334, 361 |

#### Error Handling Standardization (AP-032 first, then AP-033→035 in parallel)

| ID | Action | Depends On |
|----|--------|-----------|
| AP-032 | **Define convention**: services throw typed exceptions (`NotFoundException`, `ValidationException`, `ConflictException`); global middleware maps to HTTP status codes | — (must be done first) |
| AP-033 | `ApprovalWorkflowService.cs` — convert `InvalidOperationException` to typed domain exceptions | AP-032 |
| AP-034 | `CommissionRulesEngine.cs` — unify return-null vs throw pattern | AP-032 |
| AP-035 | `CommunicationService.cs` — align with convention | AP-032 |

#### God Class Splits (AP-036→038, all independent)

| ID | Class | Lines | Extract To |
|----|-------|-------|-----------|
| AP-036 | `LLMService.cs` | 1500+ | `ILLMProvider` interface + per-provider classes + `LLMProviderFactory` |
| AP-037 | `AccountService.cs` | 1600+ | `AccountContactService`, `AccountPreferencesService` |
| AP-038 | `MonitoringService.cs` | 1200+ | `DatabaseHealthService`, `DockerMonitoringService`, `KubernetesMonitoringService` |

#### Knowledge Base — General KB Backend (KB-001→009)

> KB-002→KB-005 are sequential. KB-006, KB-008 independent. KB-007, KB-009 require KB-005.

| ID | Priority | Description | Depends On |
|----|----------|-------------|-----------|
| KB-001 | P1 | Resolve `ArticleType` enum collision — ITSM (`FAQ=3`) vs General KB (`FAQ=1`). Add disambiguating XML doc comments. Update `SPEC-GEN-001-EnumReference.md` | — |
| KB-002 | P1 | Create `IKnowledgeBaseService` — 14 methods: GetAll (paginated), GetById, GetBySlug, Create, Update, Delete, Publish, Archive, SubmitFeedback, GetCategories, GetPopular, GetRecent, GetByProduct, TrackCaseDeflection | — |
| KB-003 | P1 | Create General KB DTOs — `KnowledgeBaseArticleDto`, `CreateKnowledgeBaseArticleDto`, `UpdateKnowledgeBaseArticleDto`, `KnowledgeBaseFeedbackDto`, `KnowledgeCategoryDto` | KB-002 |
| KB-004 | P1 | Implement `KnowledgeBaseService` using `context.KnowledgeArticles` DbSet; Draft→InReview→Published→Archived state machine | KB-002, KB-003 |
| KB-005 | P1 | Create `KnowledgeBaseController` at `/api/knowledge` — 13 endpoints; register DI in `Program.cs` | KB-004 |
| KB-006 | P1 | Unit tests for `KnowledgeBaseService` — all 14 methods, state transitions, slug collision, edge cases | KB-004 |
| KB-007 | P1 | Fix `KnowledgeBasePage.tsx` — update 404 API calls from `/knowledge/*` to `/api/knowledge/*` | KB-005 |
| KB-008 | P2 | Create `knowledgeBaseService.ts` frontend service — typed axios calls for all CRUD operations | — |
| KB-009 | P2 | Controller tests for `KnowledgeBaseController` — 13 endpoints, auth requirements, pagination | KB-005 |

---

### P2 — Feature Flag and External Integration Enablements (Blocked)

> Scaffolding complete. Blocked on external prerequisites. No code work needed until unblocked.

| ID | Feature | Blocker |
|----|---------|---------|
| FLAG-001 | Enable `EnableCustomerPortal` | SMTP credentials deployed to environment |
| FLAG-002 | Enable `EnablePartnerPortal` | Partner dashboard FE needs to be built first |
| FLAG-003 | Enable `NewSearchExperience` | External search provider (Meilisearch/Algolia) not configured |
| FLAG-004 | AIAssistant floating chat widget | Build widget with SK agent endpoint + conversation history |
| FLAG-005 | `UseOptionalAuditLogging` extended audit | Needs async queue (Redis Stream / RabbitMQ) + log rotation policy |
| FLAG-006 | `Stripe.EnableSubscriptionTracking` | Stripe account + webhook setup |
| COMM-001 | WhatsApp Business API | Meta Business API credentials |
| COMM-002 | Facebook Messenger | Facebook Graph API credentials |
| COMM-003 | Twitter/X API v2 DM | X API credentials (expensive tier — low priority) |
| COMM-004 | LinkedIn Messaging | LinkedIn Sales Navigator Enterprise license |
| INT-001 | QuickBooks/Xero accounting sync | OAuth2 app credentials; wire `AccountingSyncService` stub |
| INT-002 | Mailchimp/HubSpot marketing sync | API credentials; wire `MarketingSyncService` stub |
| INT-003 | LinkedIn Sales Navigator integration | Sales Navigator license (very low priority) |
| INT-004 | Calendly/Cal.com scheduling | Better suited for n8n workflow template |

---

### P2 — KB Unified Search Facade (KB-010→014)

> KB-010 must come first. Then KB-011/KB-014 in parallel. Then KB-012/KB-013 in parallel.

| ID | Description | Depends On |
|----|-------------|-----------|
| KB-010 | Create `IUnifiedKnowledgeSearchService` — `SearchAsync(query, maxResults, source?, ct)` + `IndexAllAsync`; `KnowledgeSource` enum (General/ITSM); `UnifiedKnowledgeSearchResultDto` | — |
| KB-011 | Implement `UnifiedKnowledgeSearchService` — parallel queries on both DbSets; merge by relevance score; optional source filter | KB-010 |
| KB-012 | Add `GET /api/knowledge/search` unified search endpoint | KB-011 |
| KB-013 | Wire `SelfServiceChatbotService` to `IUnifiedKnowledgeSearchService` — replace hardcoded mock articles | KB-011 |
| KB-014 | Extend `AIKnowledgeSearchService` to index `KnowledgeArticles` (General KB) alongside existing `ITSMKnowledgeArticles` | KB-010 |

---

### P3 — Technical Debt Cleanup (Opportunistic)

> No dependencies between items. Do not block GA on any P3 item.

| ID | Action |
|----|--------|
| PRA-013 | Delete `ProcessPaymentRequestDto` obsolete alias — unused |
| PRA-014 | Delete shell aliases `Customer`/`AccountStatus`/`CustomerContact` from `AccountDtos.cs` — confirm no external API consumers first |
| PRA-015 | Audit 31 `.disabled` archive files — delete or move to `archive/` branch |
| PRA-018 | Implement `RoslynScriptEngine` / `TypeScriptScriptEngine` stubs (currently `NotImplementedException`) — or document as post-GA roadmap |
| PRA-019 | Wire `EmailDigestPage.tsx`, `ReportTemplatesPage.tsx`, `InvoiceDetailsPage.tsx` TODO stubs to actual endpoints |
| PRA-020 | Re-enable `AccountMergeDialog`, `AccountHierarchyTree`, `AccountTimeline` — implement merge API, MUI tree-view fix, activityService |
| AP-039 | Split `Program.cs` (1500+ lines) into `AddAuthServices()`, `AddDatabaseServices()`, `AddHealthCheckServices()` extension methods |
| AP-040 | Split `CrmDbContext.cs` (4000+ lines) — extract `OnModelCreating` config into `IEntityTypeConfiguration<T>` per entity group |
| KB-015 | Add Meilisearch unified knowledge index (both `KnowledgeArticles` + `ITSMKnowledgeArticles` with `source` discriminator) |
| KB-016 | Consolidate `KnowledgeArticleVersion.cs` + `ArticleVersion.cs` in ITSM namespace (near-identical schemas) |
| KB-017 | Add General KB category management UI — hierarchical CRUD for `KnowledgeCategory` entities |

### Deferred (will not be addressed this wave)

| ID | Reason |
|----|--------|
| AP-059 | Anemic domain model — adding behavior to 90+ entities requires a dedicated DDD migration phase with full team coordination |
| XMOD-011 | `KnowledgeArticle` entity consolidation — ITSM + General KB versions have separate DbSets and incompatible schemas; needs architectural decision first |

---

## Section 3 — Execution Plan

### Tier 1: Immediate (P0 — all in parallel, ~6h total)

```
AP-004   ServiceRequestService counters       (Backend, ~1h)
AP-015   7 Provider Factory sync-over-async   (Backend, ~4h)
AP-016   OptionalAuditLoggingService           (Backend, ~1h)
AP-017   AdminSearchAnalytics .Result         (Backend, ~1h)
```

### Tier 2: Pre-GA Sprint (P1 — 5 groups, mostly parallel)

```
B1 (PRA-001,002,003)            — ~2h, parallel within group
B2 (PRA-004,005)                — ~4h, parallel
B3 (AP-018,019)                 — ~3h, parallel
B4 (AP-020,021,022,023)         — ~8h, all independent
B5 (AP-024,025,026,027)         — ~4h, all independent
── all B groups can run in parallel ──
```

### Tier 3: Post-GA Sprint 1 (P2 code work)

```
PRA-006,007       Frontend type alignment     (parallel, ~3h)
PRA-008,016,017   Test re-enablement          (sequential within, ~4h)
PRA-011           ContractExpirationJob       (~1h)
AP-028,029,030,031  CancellationToken         (parallel, ~3h)
AP-032            Error handling convention   (~2h, must be first)
AP-033,034,035    Apply convention            (parallel after AP-032, ~3h)
AP-036,037,038    God class splits            (parallel, ~12h)
KB-001            Enum collision fix          (~1h)
KB-002→005        General KB backend          (sequential, ~8h)
KB-006            KB unit tests               (parallel with KB-005, ~3h)
KB-007            Fix frontend routing        (after KB-005, ~1h)
KB-008            knowledgeBaseService.ts     (independent, ~2h)
KB-009            KB controller tests         (after KB-005, ~2h)
```

### Tier 4: Post-GA Sprint 2 (KB Unified Search)

```
KB-010            IUnifiedKnowledgeSearchService      (~2h)
KB-011            Implementation                       (after KB-010, ~4h)
KB-014            Extend AIKnowledgeSearchService      (after KB-010, parallel with KB-011, ~2h)
KB-012            Unified search endpoint              (after KB-011, ~1h)
KB-013            Wire chatbot                         (after KB-011, ~2h)
```

### Tier 5: Cleanup (P3, ongoing, no blockers)

```
PRA-013→015, PRA-018→020    Cleanup + test re-enable    (~4h total)
AP-039                       Split Program.cs             (~6h)
AP-040                       Split CrmDbContext.cs        (~12h)
KB-015→017                   KB cleanup                   (~4h)
```

---

## Item Counts

| Priority | Count | Status |
|----------|-------|--------|
| P0 Critical | 4 | Block production deploy |
| P1 Pre-GA | 14 | Block GA release |
| P2 Active code | 37 | Post-GA sprint work |
| P2 Blocked (external) | 14 | Waiting on prerequisites |
| P3 Tech debt | 11 | Opportunistic cleanup |
| Deferred | 2 | Future DDD roadmap |
| **Total** | **82** | |

---

**Document Maintained By:** GitHub Copilot
**Restructured:** March 8, 2026 — Summarized ~995 completed items; remaining ~82 items organized with prerequisites and parallel execution plan.
**Next Review:** After P0 phase complete or pre-GA release sprint.
