# CRM Solution — Master TODO List

> **Last Updated:** March 10, 2026 (v0.623.4)
> **Version:** 0.623.4
> **Active Backlog:** 1 blocked (INT-003) + 1 deferred by architectural decision (XMOD-011)
> **Build:** ✅ 0 errors, 0 SA warnings (backend + frontend) | **Tests:** ✅ 1939/1939 passing (CRM.Tests), 0 failures
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
| **Total Completed** | **~1104** | | |

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

## Section 3 — Completed Work Detail

All completed item details have been archived to [docs/DONE_LOG.md](DONE_LOG.md).

---

**Document Maintained By:** GitHub Copilot
**Last Cleaned:** March 10, 2026 — Pre-existing test failure cleared (test passes); StyleCop SA warnings all eliminated (v0.623.4).
**Current Version:** 0.623.4

