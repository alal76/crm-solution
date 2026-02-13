# E2E Test Review Report

**Date:** 2026-02-13
**Source Run:** e2e-tests/test-logs/test-log-2026-02-13T22-30-59.json
**Environment:** BASE_URL=http://192.168.0.9

## Summary
- **Total:** 21
- **Passed:** 8
- **Failed:** 13
- **Skipped:** 0
- **Duration:** 45.981s

## Passing Tests
- **TC-UNKNOWN** — authenticate (auth.setup.ts)
- **TC-ADMIN-001** — Should display users list (admin)
- **TC-ADMIN-007** — Should display roles list (admin)
- **TC-ADMIN-008** — Should create new role (admin)
- **TC-ADMIN-009** — Should assign permissions to role (admin)
- **TC-ADMIN-004** — Should edit user (admin)
- **TC-ADMIN-011** — Should display settings page (admin)
- **TC-ADMIN-010** — Should delete test role (admin)

## Failing Tests
- **TC-UNKNOWN** — verify linked contacts appear in UI (account-contact-linking)
  - Error: Expected > 0 rows, received 0
- **TC-UNKNOWN** — create 10 accounts and link random contacts via API (account-contact-linking)
  - Error: Timeout POST http://localhost:5000/api/auth/login (10s)
- **TC-ADMIN-002** — Should have create user button (admin)
  - Error: Add/New/Create button not found
- **TC-ADMIN-003** — Should create new user (admin)
  - Error: Click timeout on Add/New/Create button
- **TC-ADMIN-012** — Should access security settings (admin)
  - Error: storageState missing (.auth/user.json)
- **TC-ADMIN-013** — Should configure password policy (admin)
  - Error: storageState missing (.auth/user.json)
- **TC-ADMIN-014** — Should configure session timeout (admin)
  - Error: storageState missing (.auth/user.json)
- **TC-ADMIN-015** — Should display LLM settings page (admin)
  - Error: storageState missing (.auth/user.json)
- **TC-ADMIN-016** — Should configure LLM endpoint (admin)
  - Error: storageState missing (.auth/user.json)
- **TC-ADMIN-018** — Should test LLM connection (admin)
  - Error: storageState missing (.auth/user.json)
- **TC-ADMIN-005** — Should disable user (admin)
  - Error: Marked as ERROR in reporter
- **TC-ADMIN-006** — Should delete test user (admin)
  - Error: Marked as ERROR in reporter
- **TC-ADMIN-017** — Should configure model selection (admin)
  - Error: Marked as ERROR in reporter

## GitHub CI/CD Failing Tests
**Run:** 22005127131 (GitHub Actions)
**Job:** Backend Tests & Build → Run Service tests

Failing tests:
- CRM.Tests.Services.ReportBuilderServiceTests.ExecuteReportAsync_ShouldReturnResults_ForAccountsSource
- CRM.Tests.Services.ReportBuilderServiceTests.ExportToCsvAsync_ShouldReturnCsvString
- CRM.Tests.Services.ReportBuilderServiceTests.DeleteReportAsync_ShouldReturnTrue_WhenExists
- CRM.Tests.Services.ReportBuilderServiceTests.ExecuteReportAsync_ShouldReturnResults_ForOpportunitiesSource
- CRM.Tests.Services.ReportBuilderServiceTests.UpdateReportAsync_ShouldModifyReport
- CRM.Tests.Services.ReportBuilderServiceTests.ExecuteReportAsync_ShouldRespectMaxRows
- CRM.Tests.Services.ReportBuilderServiceTests.CreateReportAsync_ShouldGenerateUniqueIds
- CRM.Tests.Services.ReportBuilderServiceTests.ExecuteReportAsync_ShouldReturnResults_ForLeadsSource
- CRM.Tests.Services.ReportBuilderServiceTests.ExportToCsvAsync_ShouldEscapeCommasInValues
- CRM.Tests.Services.ReportBuilderServiceTests.GetReportAsync_ShouldReturnReport_WhenExists
- CRM.Tests.Services.ReportBuilderServiceTests.GetReportsAsync_ShouldReturnOnlyUserReports
- CRM.Tests.Services.ReportBuilderServiceTests.CreateReportAsync_ShouldReturnReport_WithId

Notes:
- No additional failures were detected in the Frontend or BVT logs for this run.

## Artifacts
- Test log JSON: e2e-tests/test-logs/test-log-2026-02-13T22-30-59.json
- Test log TXT: e2e-tests/test-logs/test-log-2026-02-13T22-30-59.txt
- Report MD: e2e-tests/test-logs/test-report-2026-02-13T22-30-59.md
