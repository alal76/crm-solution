# CRM Solution — Master TODO List

> **Last Updated:** February 28, 2026
> **Version:** 0.601.1
> **Status:** ✅ Core complete — 3 backlog areas + 1 deferred stub remain
> **Historical Completion:** 668 items completed across Batches 1–3 and all scripting/portal features

---

## Pending Items

### P1 — Scripting Engine (1 item)

| ID | Priority | Description | Spec |
|----|----------|-------------|------|
| SCRIPT-006 | P2 | **PythonScriptEngine — full Python.NET wiring.** Stub exists at `CRM.Infrastructure/Scripting/PythonScriptEngine.cs`. Full implementation requires Python.NET host setup (`pythonnet` NuGet, `RestrictedPython` sandbox, CPython runtime embedding). Gated by `FeatureManagement:EnablePythonScripting=false`; factory throws `NotSupportedException` when false. All unit tests currently mock this engine. | [SPEC-AI-006](11-specifications/SPEC-AI-006-AgentScripting.md) |

---

### P2 — Database HA / DR Infrastructure

> **Spec:** [SPEC-DB-001-DatabaseManagement](11-specifications/SPEC-DB-001-DatabaseManagement.md) — All items below are ❌ in that spec. Infrastructure-only; no application code changes needed.

| ID | Priority | Description |
|----|----------|-------------|
| DB-001 | P2 | Automated backup agent (`crm-backup-agent` container) — full + incremental + binary log backups scheduled to MinIO/S3 |
| DB-002 | P2 | Backup retention policy — tiered lifecycle rules (daily 7d / weekly 4w / monthly 12m) + automated pruning |
| DB-003 | P2 | RTO/RPO formal SLA targets + quarterly DR failover test schedule |
| DB-004 | P2 | MariaDB Galera active-active HA cluster (3-node minimum) — config in `docker-compose.galera.yml` |
| DB-007 | P2 | Analytics read replica — dedicated read-only MariaDB replica for Superset to prevent OLAP load on primary |
| DB-011 | P2 | DB monitoring & alerting — export MariaDB/Redis metrics to existing Grafana/Prometheus stack |
| DB-012 | P2 | Connection pooling — deploy ProxySQL middleware (`docker-compose.proxysql.yml`) |

---

### P2 — Marketing Module

> **Current State:** ~20% complete — entities and service interfaces exist; campaign execution, template builder, and analytics UI are not implemented.
> **Ref:** [SOLUTION_GAPS_REMEDIATION_PLAN](development/SOLUTION_GAPS_REMEDIATION_PLAN.md) Phase 6

| ID | Priority | Description |
|----|----------|-------------|
| MKT-001 | P2 | Campaign execution engine — multi-step email sequences with scheduling, send intervals, open/click tracking |
| MKT-002 | P2 | Email template builder — drag-drop / Monaco+MJML editor with live preview |
| MKT-003 | P2 | Campaign analytics dashboard — open rate, click rate, unsubscribe rate, bounce rate |
| MKT-004 | P2 | Lead nurture sequences — auto-enroll leads matching segment criteria |
| MKT-005 | P2 | UTM link tracking — auto-append UTM params; capture source in `Lead.LeadSource` |
| MKT-006 | P2 | Unsubscribe / preference centre — public unsubscribe endpoint + recipient preference page |
| MKT-007 | P2 | Campaign recipient segmentation UI — dynamic list builder with filter rules |
| MKT-008 | P2 | A/B test support — two subject-line variants; auto-promote winner after N hours |
| MKT-009 | P2 | Unit + integration + E2E tests for MKT-001→008 |

---

### P3 — Future Features (Backlog Code TODOs)

Pre-existing tagged items in source. Not blocking any release.

| ID | Tag | Description |
|----|-----|-------------|
| BACK-001 | TODO-AUTH-003 | Okta Enterprise SSO provider (`OktaSsoOptions`, interface stub exists) |
| BACK-002 | TODO-AUTH-004 | Generic OIDC provider (`OpenIdConnectOptions`, interface stub exists) |
| BACK-003 | TODO-AUTH-010 | Platform biometric authentication (`IBiometricAuthService` stub exists) |
| BACK-004 | TODO-CRM003-03 | Competitor tracking on opportunities (`Competitor` entity exists; service/API/UI missing) |
| BACK-005 | TODO-GAP-04 | Territory-based lead & opportunity assignment (`Territory` entity exists) |
| BACK-006 | TODO-CRM002-04 | Web-to-lead form builder (`ILeadCaptureService` stub exists) |
| BACK-007 | TODO-GAP-SALES-001 | Order returns workflow (credit notes, return receipts) |
| BACK-008 | TODO-SALES003-010 | PDF generation service for quotes/invoices (`IPdfGenerationService` stub) |
| BACK-009 | TODO-SALES006-023 | Billing timezone support (`IBillingTimezoneService` stub) |
| BACK-010 | TODO-SALES003-012 | Automated dunning email scheduler (`IDunningSchedulerService` stub) |
| BACK-011 | TODO-CRM003-06 | Opportunity cloning endpoint (`IOpportunityService.CloneAsync`) |
| BACK-012 | TODO-CRM002-07 | Lead aging alerts / stale lead notifications (`ILeadAlertService` stub) |
| BACK-013 | TODO-SD005-011 | Escalation analytics reports (`EscalationAnalyticsSummaryDto` stub) |
| BACK-014 | TODO-SYS006-004 | GDPR right-to-erasure & data-export service (`IGdprService` stub) |
| BACK-015 | TODO-GAP-05 | Multi-currency / FX rate conversion (`ICurrencyService` stub) |

---

### P3 — Technical Debt

| ID | Description | Effort |
|----|-------------|--------|
| TD-001 | Consolidate duplicate `CommissionCalculationResultDto` / `CommissionStatisticsDto` from `CommissionCalculationService.cs` lines 200–229 into `CRM.Core/Dtos/CommissionManagementDtos.cs`. CS0535 currently suppressed via `SuppressMessage`. | 4–6 h |
| TD-002 | Update stale ❌ markers in `SPEC-AI-006-AgentScripting.md` and `SPEC-GEN-002-ConfigurableEnums.md` — both specs predate completion of SCRIPT-001→024 and ENUM-001→067; all items are done but still show as ❌ Not Implemented. | 2 h |

---

## Completed — Summary

All 668 items below are done. Detail archived to keep this file actionable.

| Batch / Group | Items | Completed |
|---------------|-------|-----------|
| Core CRM + Sales + Service Desk + ITSM + System modules (legacy sessions 1–7) | ~502 | 2025–2026 |
| Scripting Phases 1–6 (SCRIPT-001→024) | 24 | Feb 26, 2026 |
| Scripting Architecture (SARCH-001→094) | 94 | Feb 26–28, 2026 |
| Batch 3 — Configurable Enums (ENUM-001→067) | 67 | Feb 28, 2026 |
| Batch 2 — FEAT-COLLAB (Record Comments & @Mentions) | 10 | Feb 2026 |
| Batch 2 — FEAT-CSAT (Satisfaction Surveys / NPS) | 9 | Feb 2026 |
| Batch 2 — FEAT-REVENUE (ARR/MRR Analytics) | 8 | Feb 2026 |
| Batch 2 — FEAT-PORTAL (Customer Self-Service Portal, PORTAL-001→043) | 43 | Feb 28, 2026 |
| Batch 2 — FEAT-AISCORING (Lead Scoring Real-time + History) | 9 | Feb 2026 |
| Batch 2 — FEAT-E2E (E2E Suite Stabilization) | 7 | Feb 2026 |
| Field Gap Remediation (DTO / Entity / FE Type / UI — all 16 entities) | full audit | Feb 22, 2026 |
| **Total** | **~668** | |

---

## Current Health Snapshot

| Metric | Status |
|--------|--------|
| Backend build | ✅ 0 errors, 0 warnings |
| Unit tests | ✅ Passing |
| BVT suite (dev server 192.168.0.9) | ✅ 118/118 passing |
| E2E (chromium + firefox) | ✅ Passing (Mobile Safari excluded — macOS WebKit limitation) |
| Frontend TypeScript | ✅ 0 errors (tsc --noEmit) |
| Deployment | ✅ crm-api + crm-frontend live on dev server |
| Rate limiting | ✅ Disabled on dev server (`RATE_LIMITING_ENABLED=false`) |
| Notable feature flags | `EnablePythonScripting=false` (stub only) \| `EnableCustomerPortal=false` (ready to enable per env) |

---

**Document Maintained By:** GitHub Copilot
**Next Review:** When Marketing Module or DB HA sprint is planned
