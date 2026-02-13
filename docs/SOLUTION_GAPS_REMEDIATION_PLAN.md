# CRM Solution Gaps Remediation Plan

> **Created:** February 8, 2026  
> **Last Updated:** February 13, 2026  
> **Status:** 99% Complete — All phases done except remaining test coverage items below. All webhook, infrastructure & security items fully resolved.

---

## Current State

| Metric | Value |
|--------|-------|
| **Build Status** | ✅ 0 Errors |
| **Test Status** | ✅ 5,160+ Active Tests |
| **BVT Status** | ✅ 118/118 Passing (100%) |
| **Phases Complete** | 11 of 11 |

---

## Pending Items

The following items remain from the remediation effort:

### Test Coverage

| ID | Priority | Description | Notes |
|----|----------|-------------|-------|
| P-05 | 🟡 Medium | Add tests for ~62 untested services | 33% coverage currently |
| P-06 | 🟡 Medium | Add tests for ~61 untested controllers | 65% untested |
| P-07 | 🟡 Medium | Create frontend unit tests (Jest) | Zero test files exist |
| P-08 | 🟢 Low | Create Playwright ITSM E2E tests | ~55 tests estimated |
| P-09 | 🟢 Low | Unskip ~47 E2E tests | 6.5% of tests skipped |

### AI/Semantic Kernel Integration (Pending)

| ID | Priority | Description | Notes |
|----|----------|-------------|-------|
| P-AI-05 | 🟢 Low | Qdrant production deployment | Init script ready, needs production hosting |
| P-AI-06 | 🟢 Low | Agent fine-tuning with production data | Requires usage data collection period |

---

## Granular Breakdown (Pending Items)

### P-05 — Add tests for untested services

- Generate inventory of untested services with owning module.
- Tag each service by CRUD surface area and dependencies (DbContext, providers).
- Prioritize by business criticality (Auth, Accounts, Leads, Opportunities, Payments).
- Create baseline tests: Create/Update/Delete, validation failures, and soft delete behavior.
- Add tests for filtering and pagination methods where applicable.
- Add tests for status transitions and business rules (approve/reject/cancel flows).
- Add coverage for concurrency/row version where applicable.
- Add negative tests (invalid IDs, missing required fields, duplicates).
- Ensure all service tests follow naming convention and use CancellationToken.
- Record coverage deltas per service to track progress.

### P-06 — Add tests for untested controllers

- Inventory controllers missing tests and group by domain.
- Capture endpoint matrices (route, verb, auth policy) per controller.
- Add smoke tests for core CRUD endpoints (200/400/404 paths).
- Add auth/role-based access tests for restricted endpoints.
- Add pagination and sorting tests for list endpoints.
- Validate model validation errors return consistent problem details.
- Add tests for specialized endpoints (search, stats, batch, toggle, status).
- Validate response shape and content-type for each endpoint.
- Add tests for soft delete behavior (deleted records hidden).

### P-07 — Create frontend unit tests (Jest)

- Configure Jest + React Testing Library baseline (if not already configured).
- Add a shared test utils module (renderWithProviders, mockRouter, mockApi).
- Mock API layer (Axios) and SignalR context for predictable results.
- Add tests for shared components: DataGrid, Form, ErrorBoundary, Loader.
- Add tests for critical pages: Accounts, Contacts, Leads, Opportunities.
- Add tests for auth flows (login, token refresh, route guards).
- Add tests for hooks (pagination, SignalR, auth context).
- Add accessibility checks for critical components (labels, roles).
- Establish coverage thresholds and CI gate.

### P-08 — Create Playwright ITSM E2E tests

- Define ITSM BVT scenarios (Incident, Problem, Change, CMDB, Knowledge).
- Add auth setup and seed data fixtures for ITSM modules.
- Create stable locators for MUI components (data-testid, role-based).
- Add tests for create/view/update/close flows per ITSM entity.
- Add tests for comments, attachments, and status changes.
- Add smoke tests for dashboards and analytics pages.
- Add role-based access checks for ITSM screens.
- Stabilize selectors for MUI components.
- Add trace and video capture for flaky tests to aid debugging.

### P-09 — Unskip E2E tests

- Enumerate skipped tests and categorize by failure reason.
- Identify tests failing due to data dependencies vs timing.
- Fix flaky waits (replace networkidle, add deterministic waits).
- Update selectors where UI changed (MUI id/class updates).
- Re-enable tests in batches and stabilize on CI.
- Add test retries and timeouts only where justified.
- Add targeted waitForResponse for key API calls.
- Track stability per test file and record fixes.

### P-AI-05 — Qdrant production deployment

- Provision production Qdrant (managed or self-hosted) with backups.
- Configure network access and secrets in environment.
- Run init script and validate collections/migrations.
- Add health checks and alerting for Qdrant availability.

### P-AI-06 — Agent fine-tuning with production data

- Define data collection window and governance approvals.
- Implement anonymization and PII redaction pipeline.
- Generate fine-tuning datasets from production logs.
- Run offline evaluation and rollback criteria.
- Deploy tuned models to staging, then production if metrics improve.

---

## Completed This Update

### P-05 — Add tests for untested services (In Progress)

- Added unit tests for `PipelineService`, `NoteService`, and `ConversationService`.
- Test run: `dotnet test CRM.Backend/tests/CRM.Tests.csproj --filter FullyQualifiedName~NoteServiceTests|FullyQualifiedName~ConversationServiceTests|FullyQualifiedName~PipelineServiceTests`.

### P-04 — Re-enable excluded test files

- Re-enabled tests previously blocked by missing `FindAsync(object[], CancellationToken)` mock support.
- Updated both MockDbSetFactory implementations to support the CancellationToken overload.

---

## Summary by Priority

| Priority | Count |
|----------|-------|
| 🔴 Critical | 0 |
| 🟡 Medium | 3 |
| 🟢 Low | 4 |
| **Total** | **7** |

---

## References

- [MASTER_TODO_LIST.md](MASTER_TODO_LIST.md) — 142 pending TODO items
- [specifications/INDEX.md](specifications/INDEX.md) — 10/40 specs complete
- [copilot-instructions.md](../.github/copilot-instructions.md)
- [THIRD_PARTY_LICENSES.md](THIRD_PARTY_LICENSES.md) — Complete third-party dependency licensing inventory

---

**END OF REMEDIATION PLAN**
