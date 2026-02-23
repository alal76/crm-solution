# CRM Solution - Master TODO List (Reviewed & Updated)

> **Last Updated:** February 23, 2026 - Evening Session Round 4 (Sprint 1 Continued)
> **Version:** 0.574.0
> **Purpose:** Master list of all pending, partial, and completed items — validated against actual code
> **Audit Method:** 6 parallel sub-agent code reviews across Backend, Frontend, Database/DTOs, Integration/Tests, Auth/SYS, and UX/CRM + Manual Fixes
> **Prior Update:** February 23, 2026 (Evening Round 2)

---

## Audit Summary (February 23, 2026 - Evening Session Round 3)

### Sprint 1 Continued - P1 Items & Technical Debt (Feb 23 Evening Round 3)

**Branch:** feature/master-todo-sprint1-implementation  
**Objective:** Fix technical debt (AdminConfig + ChangeManagementSvc) and implement P1 ITSM backend controllers + Escalation frontend pages using 3 parallel subagents

| Work Item | Status | Notes |
|-----------|--------|-------|
| **AdminConfigurationService.cs** | ✅ FIXED | All 12 compilation errors resolved: `EscalationRules` → `ITSMEscalationRules` (added ITSM DbSet), ServiceQueue root→ITSM namespace, extended fields serialized to RoutingConfiguration JSON |
| **ICrmDbContext + CrmDbContext** | ✅ UPDATED | Added `DbSet<ITSM.EscalationRule> ITSMEscalationRules` to both interface and concrete context |
| **ChangeManagementServiceEx.cs** | ✅ FIXED & ENABLED | Added `using CRM.Infrastructure.Data;` for IDbContextResolver, aliased `ApprovalStatus = ITSM.ApprovalStatus`, renamed from `.disabled`, registered in Program.cs |
| **AdminConfigurationController.cs** | ✅ ENABLED | `IAdminConfigurationService` re-registered in Program.cs (no longer commented out) |
| **ServiceQueuesController.cs** | ✅ CREATED | 282 lines, 8 endpoints — CRUD + AssignToQueue + GetQueueItems + GetQueueStats |
| **SLAPoliciesController.cs** | ✅ CREATED | 246 lines, 7 endpoints — CRUD + AssignPolicy + GetApplicablePolicies |
| **EscalationRulesController.cs** | ✅ CREATED | 250 lines, 7 endpoints — CRUD + TestRule + GetApplicableRules |
| **escalationService.ts** | ✅ CREATED | 78 lines — full CRUD + test/applicable endpoints |
| **EscalationRulesPage.tsx** | ✅ CREATED | 597 lines — CRUD + delete confirm + test-rule dialog + filter chips + stats cards |
| **EscalationDashboardPage.tsx** | ✅ CREATED | 279 lines — priority distribution + mock escalation events |
| **App.tsx + Navigation.tsx** | ✅ UPDATED | Routes: /itsm/escalation/rules + /itsm/escalation/dashboard; sidebar links added under ITSM |
| **Backend Build** | ✅ PASSING | 0 errors after all fixes |
| **Frontend Build** | ✅ PASSING | 0 errors, production build succeeded |

### Summary Metrics - Round 3
- **Technical Debt Resolved:** 2 disabled services re-enabled, 12 compilation errors fixed
- **Backend Controllers Created:** 3 (778 lines total, 22 new HTTP endpoints)
- **Frontend Pages Created:** 2 (876 lines total)
- **Frontend Service Created:** 1 (escalationService.ts, 78 lines)
- **Backend Build Status:** ✅ PASSING (0 errors)
- **Frontend Build Status:** ✅ PASSING (0 errors)
- **Commits:** 4 (e6397800, 00095481, ba528ec4, e5ee2b10)
- **Todos Completed:** AdminConfigFix, ChangeManagementSvcFix, ServiceQueuesCtrl, SLAPoliciesCtrl, EscalationRulesCtrl, EscalationRulesPage, EscalationDashboardPage

---

## Audit Summary (February 23, 2026 - Evening Session Round 2)

### Sprint 1 Continued - Frontend & Documentation (Feb 23 Evening Round 2)

**Branch:** feature/master-todo-sprint1-implementation  
**Objective:** Continue P1/P2 implementation using parallel subagents

| Work Item | Status | Notes |
|-----------|--------|-------|
| **InvoiceDetailsPage.tsx** | ✅ COMPLETED | 573 lines - Full invoice details, line items table, payment recording, PDF/email actions, status badges, timeline |
| **ContractDetailsPage.tsx** | ✅ COMPLETED | 690 lines - Contract lifecycle, renewal/termination dialogs, document management, days-remaining alerts |
| **InvoiceForm.tsx** | ✅ COMPLETED | Extracted standalone component - Account selector, date pickers, amount calculations, validation |
| **PaymentForm.tsx** | ✅ COMPLETED | Extracted standalone component - Invoice selector, payment methods (17 types), validation with balance due check |
| **InvoiceStatusBadge.tsx** | ✅ COMPLETED | Reusable status badge - 13 invoice statuses with color coding and icons |
| **PaymentHistory.tsx** | ✅ COMPLETED | Reusable payment history table - Auto-fetches payments, displays totals, loading/error states |
| **SPEC-ARCH-007** | ✅ COMPLETED | Middleware Pipeline Architecture (37KB) - 8+ middleware components, execution order, pipeline patterns |
| **SPEC-ARCH-008** | ✅ COMPLETED | Provider Plugin Architecture (50KB) - Hexagonal architecture, 7 provider categories, factory patterns, feature flags |
| **SPEC-ARCH-009** | ✅ COMPLETED | Concurrency Control (43KB) - Optimistic concurrency with RowVersion, ETag/If-Match patterns, database-agnostic implementation |
| **INDEX.md** | ✅ UPDATED | Architecture specs: 5 → 8 complete |
| **App.tsx Routing** | ✅ UPDATED | Added /invoices/:id and /contracts/:id routes with lazy loading |
| **Frontend Build** | ✅ PASSING | Zero TypeScript errors, all components compile successfully |

### Summary Metrics - Round 2
- **Frontend Pages Created:** 2 (1,263 lines total)
- **Frontend Components Extracted:** 4 reusable components
- **Architecture Specs Created:** 3 (~130KB documentation)
- **Frontend Build Status:** ✅ PASSING (0 errors)
- **Commits:** 1 (6,078 insertions, 15 files changed)
- **Todos Completed:** 9 (TODO-SALES003-001, 003, 005, 006, TODO-SALES004-008, TODO-SALES005-002, TODO-ARCH-007, 008, 009)

---

## Audit Summary (February 23, 2026 - Evening Session Round 1)

### Sprint 1 Implementation Status (Feb 23 Evening)

**Branch:** feature/master-todo-sprint1-implementation  
**Objective:** Complete P0 critical todos and advance to P1 items

| Work Item | Status | Notes |
|-----------|--------|-------|
| **SubscriptionRenewal Entity** | ✅ COMPLETED | Created with 6 fields + entity tests. 34 unit tests passing. |
| **BillingHistory DbSet** | ✅ COMPLETED | Added to CrmDbContext + EF config (1:N relationship with invoice). |
| **DunningRecord DbSet** | ✅ COMPLETED | Added to CrmDbContext + EF config (1:1 relationship with billing history). |
| **BillingCycle Enum** | ✅ COMPLETED | Created with 7 values (Monthly, Quarterly, Annual, Weekly, Daily, Biannual, Custom). Updated SPEC-GEN-001-EnumReference.md. |
| **EF Core Migration** | ✅ COMPLETED | Migration `20260223190000_AddSubscriptionRenewalBillingHistoryDunningRecordTables` created. All 3 tables created in migration. |
| **SubscriptionBillingController** | ⚠️ STUBBED | Created 9 endpoints (GET/POST billing/invoices, payments, metrics). Disabled due to missing entity properties. |
| **SubscriptionUsageController** | ⚠️ STUBBED | Created 10 endpoints (GET/POST usage, limits, resets, seat mgmt, aggregation). Disabled due to entity mismatch errors. |
| **ChangeManagementServiceEx** | ✅ FIXED (Round 3) | Added `using CRM.Infrastructure.Data;`, aliased `ApprovalStatus = ITSM.ApprovalStatus`, renamed from `.disabled`, registered in DI. |
| **AdminConfigurationController** | ✅ FIXED (Round 3) | All 12 errors resolved: EscalationRules→ITSMEscalationRules DbSet added, ServiceQueue root→ITSM namespace, missing fields remapped to RoutingConfiguration JSON. |
| **Build Status** | ✅ PASSING | Zero compile errors after disabling broken files. ~36 pre -existing StyleCop warnings. |

### Summary
- **P0 DB Gaps:** 100% Complete (SubscriptionRenewal, BillingHistory, DunningRecord all added to DbContext with migration)
- **P0 Controllers:** Stubbed frameworks created, disabled due to entity integration issues
- **P0 ITSM Services:** ChangeManagementService compilation issues documented
- **Regression Prevention:** All disastrous files disabled to maintain clean build
- **Commits:** 2 commits made (DB work + fixes)

---

## Earlier Audit Summary (February 23, 2026)

| Area | ✅ Confirmed Done | ⚠️ Partial | ❌ Still Pending |
|------|-----------------|-----------|----------------|
| **Sales Backend** | PaymentsController, CommissionModule (100%), Subscription services + core entities, InvoiceDTOs, ContractService, SubscriptionRenewal entity ✅, BillingHistory/DunningRecord DbSet ✅, BillingCycle enum ✅, Contract EndDate/Value validations ✅, Subscription validations ✅ | Stripe (webhooks only), ProcessPaymentDto naming | SubscriptionBilling/UsageControllers (disabled) |
| **Sales Frontend** | PaymentsPage, ContractsPage, SubscriptionsPage, subscriptionService, commissionService, InvoiceDetailsPage ✅, ContractDetailsPage ✅, InvoiceForm ✅, PaymentForm ✅, InvoiceStatusBadge ✅, PaymentHistory ✅ | Inline forms (not standalone components), SubscriptionsPage is 1 page not 5 | ~14 sub-components remaining (RefundDialog, etc.) |
| **Service Desk Backend** | EmailToTicket, SLAEnforcementService, EscalationControllers/Services, BusinessHours, SLA compliance report, ServiceQueuesController ✅, SLAPoliciesController ✅, EscalationRulesController ✅, ChangeManagementServiceEx ✅ | ProblemManagementService (24/26 methods), ChangeManagementService (13/39 methods) | Auto-assignment rules, SLA countdown SignalR |
| **ITSM** | 17 unit tests, 6 controller tests, itsmService.ts, MUI migration complete, ChangeManagement pages, EscalationRulesPage ✅, EscalationDashboardPage ✅, AdminConfigurationController ✅, SLAManagementPage ✅, ServiceQueuesPage ✅, 6 SD sub-components ✅ | ItsmSeed data | Advanced services gaps |
| **Auth** | Google/MS/GitHub OAuth, TOTP 2FA, SMS OTP, Email OTP, LinkedIn OAuth endpoints ✅, Apple OAuth endpoints ✅, WebAuthn/FIDO2 endpoints (6) ✅ | — | Okta SSO, magic link, concurrent session limits, password history |
| **Integration** | WebhookService, Import/Export controllers, StripeWebhook, Polly retry+circuit-breaker, WebhookSignatureGenerator ✅, WebhookRetryPolicy ✅, WebhookCircuitBreaker ✅ | Webhook general entity missing (ITSM-only) | ProviderRegistryService, AdminProvidersController, ImportMapping, IDataValidator, Hangfire disabled |
| **CRM Core** | Lead conversion, Lead scoring, Sales forecast, Weighted pipeline, Quote approval | Stage probability (not auto-updated), multi-currency (fields only), CPQ bundles (no UI) | Dynamic pricing engine, opportunity product line items endpoints |
| **Architecture Specs** | SPEC-ARCH-001–006, 007–012, 013 (13/13 done) ✅ | — | — |
| **Missing Specs** | ALL previously missing specs now exist (ITSM, INT, AI-003/004, SYS-002) | — | — |
| **Tests** | 425 test files, 0 excluded, ITSM unit+controller tests, Sales unit+integration tests, SD E2E | Subscription (no E2E), ChangeManagement partial | Analytics E2E, invoice/contract E2E, Subscription E2E |
| **Infra** | .NET 10 ✅, Polly retry/circuit-breaker ✅ | Hangfire coded but 100% commented out | Global search typeahead, secrets manager docs |

---

## Table of Contents

1. [Architecture Specifications](#1-architecture-specifications)
2. [Sales Module](#2-sales-module)
3. [Service Desk Module](#3-service-desk-module)
4. [ITSM Advanced](#4-itsm-advanced)
5. [Integration & Webhooks](#5-integration--webhooks)
6. [Auth & Security](#6-auth--security)
7. [System & Admin](#7-system--admin)
8. [CRM Core Gaps](#8-crm-core-gaps)
9. [Infrastructure & DevOps](#9-infrastructure--devops)
10. [Frontend UX](#10-frontend-ux)
11. [Analytics & Reporting](#11-analytics--reporting)
12. [Self-Service Portal & Mobile](#12-self-service-portal--mobile)
13. [AI & ML](#13-ai--ml)
14. [Integration Framework](#14-integration-framework)
15. [Customization Engine](#15-customization-engine)
16. [Documentation](#16-documentation)
17. [Completed Archive](#17-completed-archive)
18. [Priority Matrix & Timeline](#18-priority-matrix--timeline)

---

## 1. 🏗️ ARCHITECTURE SPECIFICATIONS

> **Status:** 7/13 complete (SPEC-ARCH-001–006 + 013 exist)
> **Remaining:** 6 mid-priority specs

### 1.1 Completed Architecture Specs ✅
- SPEC-ARCH-001: DTO Standardization
- SPEC-ARCH-002: Error Handling Strategy
- SPEC-ARCH-003: Dependency Injection Patterns
- SPEC-ARCH-004: Caching Strategy
- SPEC-ARCH-005: Validation Framework
- SPEC-ARCH-006: Worker Service Architecture
- SPEC-ARCH-013: Infrastructure & Deployment Standards

### 1.2 Remaining Architecture Specs ❌

| ID | Spec | Hours | Priority |
|----|------|-------|----------|
| ~~SPEC-ARCH-007~~ | ~~Middleware Pipeline~~ | ~~3h~~ | ✅ Created Feb 23 |
| ~~SPEC-ARCH-008~~ | ~~Provider Plugin Architecture~~ | ~~5h~~ | ✅ Created Feb 23 |
| ~~SPEC-ARCH-009~~ | ~~Concurrency Control~~ | ~~3h~~ | ✅ Created Feb 23 |
| ~~SPEC-ARCH-010~~ | ~~Data Isolation & Multi-Tenancy~~ | ~~4h~~ | ✅ Created Feb 23 (Round 4) |
| ~~SPEC-ARCH-011~~ | ~~API Versioning Strategy~~ | ~~3h~~ | ✅ Created Feb 23 (Round 4) |
| ~~SPEC-ARCH-012~~ | ~~Frontend Architecture Patterns~~ | ~~4h~~ | ✅ Created Feb 23 (Round 4) |

### 1.3 Infrastructure Arch Items

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-ARCH-013-003 | P1 | Add secrets manager guidance (Vault/AWS/Azure/GCP) — documented as gap in SPEC-ARCH-013 | ❌ Not Started |
| TODO-ARCH-013-004 | P2 | Validate WorkerControlState values in API (SystemSettingsService accepts any string) | ❌ Not Started |

---

## 2. 💼 SALES MODULE

### 2.1 SPEC-SALES-003 (Invoice Management)

**Backend Status:** ✅ Core Complete | **Frontend Status:** ❌ Sub-components missing

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SALES003-001 | P1 | Create InvoiceDetailsPage.tsx (dedicated detail view) | Frontend/Page | ✅ Created Feb 23 |
| TODO-SALES003-003 | P2 | Extract InvoiceForm.tsx as standalone component (currently inline in InvoicesPage) | Frontend/Component | ✅ Created Feb 23 |
| TODO-SALES003-004 | P2 | Create InvoiceLineItemsTable.tsx editable grid | Frontend/Component | ❌ Not Started |
| TODO-SALES003-005 | P2 | Create InvoiceStatusBadge.tsx | Frontend/Component | ✅ Created Feb 23 |
| TODO-SALES003-006 | P2 | Create InvoicePaymentHistory.tsx | Frontend/Component | ✅ Created as PaymentHistory.tsx Feb 23 |
| TODO-SALES003-010 | P3 | Implement PDF generation for invoices (PdfUrl field exists, no generation service) | Feature | ❌ Not Started |
| TODO-SALES003-011 | P3 | Create E2E tests for invoice workflows | Testing | ❌ Not Started |
| TODO-SALES003-012 | P3 | Automated dunning email sequence scheduler (DunningManager exists, no scheduler) | Feature | ⚠️ Partial |

*Completed: InvoiceDto/CreateInvoiceDto/UpdateInvoiceDto ✅, CurrencyCode validation ✅, email validation ✅, InvoiceServiceTests ✅*

### 2.2 SPEC-SALES-004 (Payment Management)

**Backend Status:** ✅ Mostly Complete | **Frontend Status:** ⚠️ Page exists, sub-components missing

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SALES004-002 | P0 | Create SubscriptionBillingController (8+ invoice/payment endpoints) | Backend/Controller | ⚠️ Stubbed (disabled Feb 23) |
| TODO-SALES004-003 | P1 | Create SubscriptionUsageController (10+ usage/limits endpoints) | Backend/Controller | ⚠️ Stubbed (disabled Feb 23) |
| TODO-SALES004-004 | P1 | Rename ProcessPaymentRequestDto → ProcessPaymentDto (naming mismatch vs spec) | Backend/DTO | ⚠️ Partial |
| TODO-SALES004-005 | P1 | Implement PCI-compliant tokenization (Stripe wired for webhooks only, no charge creation) | Security | ❌ Not Started |
| TODO-SALES004-008 | P2 | Extract PaymentForm.tsx as standalone component (currently inline in PaymentsPage) | Frontend/Component | ✅ Created Feb 23 |
| TODO-SALES004-009 | P2 | Create PaymentHistory.tsx standalone component | Frontend/Component | ❌ Not Started |
| TODO-SALES004-010 | P2 | Create RefundDialog.tsx (partial/full refund UI) | Frontend/Component | ❌ Not Started |
| TODO-SALES004-011 | P2 | Complete Stripe integration — add charge/payment-intent creation (receive-only currently) | Backend/Integration | ⚠️ Partial |

*Completed: PaymentsController ✅, PaymentDto/CreatePaymentDto ✅, paymentService.ts ✅, PaymentsPage.tsx ✅, StripeWebhookController ✅, PaymentServiceTests ✅, PaymentsControllerTests ✅*

### 2.3 SPEC-SALES-005 (Contract Management)

**Backend Status:** ⚠️ Partial | **Frontend Status:** ⚠️ List page only

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SALES005-002 | P1 | Create ContractDetailsPage.tsx (no dedicated detail view) | Frontend/Page | ✅ Created Feb 23 |
| TODO-SALES005-003 | P2 | Extract ContractForm.tsx as standalone component | Frontend/Component | ⚠️ In ContractDetailsPage (inline) |
| ~~TODO-SALES005-005~~ | P2 | Add EndDate > StartDate backend validation (missing from ContractService) | Validation | ✅ Done (Round 4) |
| ~~TODO-SALES005-006~~ | P2 | Add Value >= 0 backend validation (missing from ContractService) | Validation | ✅ Done (Round 4) |
| TODO-SALES005-010 | P2 | Create contracts.spec.ts E2E tests | Testing | ❌ Not Started |
| TODO-SALES005-011 | P2 | Create ContractRenewalDialog.tsx component | Frontend/Component | ❌ Not Started |
| TODO-SALES005-012 | P2 | Create ContractExpirationWidget for dashboard | Frontend/Component | ❌ Not Started |
| TODO-SALES005-013 | P3 | Add bulk status update operations | Backend/Feature | ❌ Not Started |
| TODO-SALES005-014 | P3 | Add contract export (PDF, Excel) | Backend/Feature | ❌ Not Started |
| TODO-SALES005-015 | P3 | Implement automated expiration background job | Backend/Feature | ❌ Not Started |
| TODO-SALES005-016 | P3 | Add contract versioning and change history | Backend/Feature | ❌ Not Started |

*Completed: ContractsPage ✅, contractService.ts ✅, status transitions ✅, ContractServiceTests ✅, ContractsControllerTests ✅*

### 2.4 SPEC-SALES-006 (Subscription Management)

**Backend Services:** ✅ Complete | **DB:** ✅ Complete (SubscriptionRenewal entity + DbSet registered + migration created Feb 23) | **Frontend:** ⚠️ Single page, no sub-components

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SALES006-002 | P0 | Create SubscriptionBillingController (8+ invoice/payment endpoints) | Backend/Controller | ❌ Not Started |
| TODO-SALES006-003 | P1 | Create SubscriptionUsageController (10+ usage/limits endpoints) | Backend/Controller | ❌ Not Started |
| TODO-SALES006-011 | P0 | Create SubscriptionRenewal entity (no class exists) | Entity | ✅ Created Feb 23 |
| TODO-SALES006-012 | P0 | Register BillingHistory entity as DbSet + add migration (entity exists, not in DbContext) | Database | ✅ Registered Feb 23 |
| TODO-SALES006-013 | P0 | Register DunningRecord entity as DbSet + add migration (entity exists, not in DbContext) | Database | ✅ Registered Feb 23 |
| TODO-SALES006-037 | P0 | Create SubscriptionRenewals table (entity, DbSet, migration all missing) | Database | ✅ Created Feb 23 |
| TODO-SALES006-006 | P2 | Create BillingCycle enum (currently stored as string in Subscription + Account entities) | Code/Quality | ✅ Created Feb 23 |
| ~~TODO-SALES006-018~~ | P1 | Add validation for SubscriptionNumber, Amount, BillingCycle | Validation | ✅ Done (Round 4) |
| TODO-SALES006-019 | P2 | Add validation for trial dates, proration type, usage limits | Validation | ❌ Not Started |
| ~~TODO-SALES006-020~~ | P2 | Add validation: auto-renewal/cancelled mutual exclusion | Validation | ✅ Done (Round 4) |
| TODO-SALES006-022 | P1 | Implement optimistic locking (RowVersion) on Subscriptions | Concurrency | ❌ Not Started |
| TODO-SALES006-023 | P2 | Add timezone support for billing date calculations | Feature | ❌ Not Started |
| TODO-SALES006-024 | P2 | Implement usage record batching for performance | Performance | ❌ Not Started |
| TODO-SALES006-025 | P2 | Add dunning grace period + escalation emails | Feature | ❌ Not Started |
| TODO-SALES006-027 | P1 | Implement subscription pause with scheduled resume | Feature | ❌ Not Started |
| TODO-SALES006-028 | P1 | Implement trial to paid conversion workflow | Feature | ❌ Not Started |
| TODO-SALES006-030 | P0 | Decompose SubscriptionsPage into 5 separate pages (Details, PlanSelector, BillingHistory, Analytics) | Frontend/Page | ⚠️ Partial |
| TODO-SALES006-031 | P0 | Create 10 subscription components (SubscriptionCard, BillingStats, UsageChart, etc.) | Frontend/Component | ❌ Not Started |
| TODO-SALES006-033 | P1 | Create billingService.ts frontend API client | Frontend/Service | ❌ Not Started |
| TODO-SALES006-040 | P1 | Create SubscriptionAnalyticsController (6+ endpoints) | Backend/Controller | ❌ Not Started |
| TODO-SALES006-004 | P1 | Standardize usage quantity precision (18,4 vs 18,2) | Data/Quality | ❌ Not Started |
| TODO-SALES006-041 | P0 | Unit tests: Proration accuracy (20+ scenarios) | Testing | ⚠️ Partial |
| TODO-SALES006-042 | P0 | Unit tests: Usage billing accuracy (15+ scenarios) | Testing | ⚠️ Partial |
| TODO-SALES006-043 | P0 | Unit tests: MRR/ARR calculation precision | Testing | ⚠️ Partial |
| TODO-SALES006-045 | P1 | Integration tests: Auto-renewal workflow | Testing | ❌ Not Started |
| TODO-SALES006-046 | P1 | Integration tests: Dunning retry + cancellation | Testing | ❌ Not Started |
| TODO-SALES006-047 | P1 | Integration tests: Plan change with proration | Testing | ❌ Not Started |
| TODO-SALES006-048 | P1 | E2E tests: Customer subscribes → upgrades → renews | Testing | ❌ Not Started |
| TODO-SALES006-049 | P2 | E2E tests: Payment failure → dunning → cancellation | Testing | ❌ Not Started |
| TODO-SALES006-050 | P2 | E2E tests: Pause/resume subscription workflow | Testing | ❌ Not Started |

*Completed: SubscriptionsController ✅, RecurringBillingEngine ✅, DunningManager ✅, ProrateCalculator ✅, SubscriptionMetricsAggregator ✅, SubscriptionItem entity + DB ✅, SubscriptionUsages DB ✅, subscriptionService.ts ✅, BillingEventType enum (as BillingEventType) ✅, SubscriptionRenewal entity + DbSet ✅ (Feb 23), BillingHistory + DunningRecord DbSet ✅ (Feb 23), BillingCycle enum ✅ (Feb 23)*

### 2.5 SPEC-SALES-007 (Commission Management) — ✅ FULLY COMPLETE

**Status:** ✅ 100% Complete as of Feb 23, 2026 audit.
- CommissionsController, CommissionPlansController, CommissionCalculationsController, CommissionPayoutsController ✅
- CommissionPlanAssignment with effective dating ✅
- CommissionCalculationService with tiers ✅
- CommissionsPage.tsx + commissionService.ts ✅
- Full test suite (6 files) ✅

**Remaining gaps:**
| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SALES007-003 | P2 | Add caps, splits, triggers to commission calculation (tiers done, caps/splits/triggers missing) | ⚠️ Partial |
| TODO-GAP-BACKEND-005 | P2 | Implement CommissionRulesEngine full implementation (no caps, no splits, no trigger logic) | ❌ Not Started |
| TODO-SALES007-004-EXT | P2 | Create separate CommissionStatementsPage + CommissionPlansPage (currently tabs in CommissionsPage) | Frontend | ⚠️ Partial |

---

## 3. 🎧 SERVICE DESK MODULE

### 3.1 SPEC-SD-001 (Service Request Management)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| ~~TODO-SD001-001~~ | P2 | Create ServiceRequestCard standalone component | Frontend | ✅ Done (Round 4) |
| TODO-SD001-002 | P2 | Extract ServiceRequestTimeline as standalone component (inline in detail page) | Frontend | ⚠️ Partial |
| TODO-SD001-003 | P2 | Create CustomFieldRenderer reusable component | Frontend | ⚠️ Partial |
| TODO-SD001-004 | P2 | Extract AssignmentPanel as standalone component (inline in detail page) | Frontend | ⚠️ Partial |
| TODO-SD001-005 | P2 | Extract SLAStatusBadge as standalone component (inline in detail page) | Frontend | ⚠️ Partial |
| ~~TODO-SD001-006~~ | P2 | Create StatusTransitionButtons component | Frontend | ✅ Done (Round 4) |
| ~~TODO-SD001-007~~ | P2 | Create ResolutionForm component | Frontend | ✅ Done (Round 4) |
| ~~TODO-SD001-008~~ | P2 | Create FeedbackForm component | Frontend | ✅ Done (Round 4) |
| ~~TODO-SD001-009~~ | P2 | Create ServiceRequestStats component | Frontend | ✅ Done (Round 4) |
| TODO-SD001-012 | P1 | Implement auto-assignment rules (round-robin, skill-based) | Backend | ❌ Not Started |

*Completed: EmailToTicketService + controller ✅, SLA auto-calc on create ✅, E2E tests ✅*

### 3.2 SPEC-SD-002 (Knowledge Base)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SD002-001 | P2 | Create CategoryTree component | Frontend | ❌ Not Started |
| TODO-SD002-003 | P2 | Create RelatedArticles component | Frontend | ❌ Not Started |
| TODO-SD002-004 | P2 | Create PopularArticles component | Frontend | ❌ Not Started |
| TODO-SD002-005 | P2 | Create ArticleMetrics component | Frontend | ❌ Not Started |
| TODO-SD002-006 | P3 | Create VersionHistory component | Frontend | ❌ Not Started |
| TODO-SD002-007 | P2 | Create PublishWorkflow component | Frontend | ❌ Not Started |
| TODO-SD002-010 | P3 | Add version history API endpoint | Backend | ❌ Not Started |
| TODO-SD002-011 | P2 | Create E2E tests for knowledge base | Testing | ❌ Not Started |
| TODO-SD002-012 | P1 | Configure dedicated KB search index schema (Meilisearch) | Database | ⚠️ Partial |

*Completed: ArticleFeedbackWidget ✅, AI embedding generation ✅, semantic search ✅*

### 3.3 SPEC-SD-003 (SLA Management)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SD003-002 | P2 | Create HolidayCalendar component | Frontend | ❌ Not Started |
| ~~TODO-SD003-003~~ | P2 | Create SLAComplianceChart component | Frontend | ✅ Done (Round 4 - in SLAManagementPage) |
| ~~TODO-SD003-005~~ | P2 | Create SLAMetricsCard component | Frontend | ✅ Done (Round 4 - in SLAManagementPage) |
| TODO-SD003-008 | P2 | Add DST handling to SLA time calculations | Backend | ❌ Not Started |
| TODO-SD003-010 | P2 | Create E2E tests for SLA workflows | Testing | ❌ Not Started |
| TODO-SD003-011 | P2 | Add SLA dashboard API endpoints | Backend | ❌ Not Started |
| TODO-SD003-012 | P1 | Implement real-time SLA countdown via SignalR (SLACountdownWidget uses polling, not SignalR) | Frontend | ❌ Not Started |

*Completed: SLACountdownWidget ✅, SLABreachAlert ✅, SLAEnforcementHostedService ✅, BusinessHoursCalculator with timezone ✅, SLA compliance report endpoint ✅*

### 3.4 SPEC-SD-004 (Workflow Engine) — ✅ FULLY COMPLETE

### 3.5 SPEC-SD-005 (Escalation Management)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-SD005-003 | P2 | Rename IEscalationRuleAdminService → IEscalationRuleService to match spec | Backend | ⚠️ Partial |
| ~~TODO-SD005-005~~ | P1 | Create escalationService.ts frontend API client | Frontend | ✅ Done (Round 3) |
| ~~TODO-SD005-006~~ | P1 | Create EscalationRulesPage + components | Frontend | ✅ Done (Round 3) |
| TODO-SD005-007 | P1 | Create EscalationPoliciesPage with level editor | Frontend | ❌ Not Started |
| ~~TODO-SD005-008~~ | P2 | Create EscalationDashboardPage with metrics | Frontend | ✅ Done (Round 3) |
| TODO-SD005-009 | P2 | Implement SMS notification channel | Backend | ❌ Not Started |
| TODO-SD005-010 | P3 | Implement Slack/Teams integration | Backend | ❌ Not Started |
| TODO-SD005-011 | P2 | Create escalation analytics reports | Backend | ❌ Not Started |
| TODO-SD005-012 | P2 | Add complex condition expression support | Backend | ❌ Not Started |
| TODO-SD005-014 | P2 | Create E2E tests for escalation workflows | Testing | ❌ Not Started |

*Completed: EscalationRulesController ✅, EscalationPoliciesController ✅, EscalationPolicyService ✅, EscalationHostedService + EscalationWorker ✅*

---

## 4. 🔧 ITSM ADVANCED

### 4.1 ITSM Core & Build

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-ITSM-01 | P2 | Complete ChangeManagementService — 26 of 39 interface methods missing (IChangeManagementServiceEx) | ✅ Re-enabled (Round 3) — 38/39 methods compile, service registered in DI |
| TODO-ITSM-02 | P2 | Complete ProblemManagementService — 2 of 26 interface methods missing | ⚠️ Partial |
| TODO-ITSM-03 | P2 | Implement Knowledge AI semantic search — **DONE** via AIKnowledgeSearchService | ✅ Done |
| ~~TODO-GAP-BACKEND-004~~ | P1 | Re-enable AdminConfigurationController and AdminConfigurationService (currently .disabled files) | ✅ Done (Round 3) — All 12 errors fixed, DI re-enabled |

### 4.2 ITSM Database & Seeding

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-ITSM-04 | P2 | Migration 010_itsm_module.sql — **EXISTS** ✅ | ✅ Done |
| TODO-ITSM-05 | P2 | Seed data 011_itsm_seed_data.sql — file MISSING (only 011_add_itsm_permission.sql exists) | ❌ Not Started |

### 4.3 ITSM Testing

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-ITSM-06 | ✅ DONE | ITSM service unit tests (17 files in tests/Services/ITSM/) | ✅ Done |
| TODO-ITSM-07 | ✅ DONE | ITSM controller integration tests (6 controller test files) | ✅ Done |
| TODO-ITSM-08 | P3 | Create Playwright E2E tests for ITSM flows | ❌ Not Started |
| TODO-ITSM-09 | P2 | Create frontend unit tests (Jest) for ITSM components | ❌ Not Started |

### 4.4 Admin Config (SYS008-015 to 026)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| ~~TODO-SYS008-017~~ | P1 | Create ServiceQueuesController (ServiceQueueService exists, no HTTP exposure) | Backend | ✅ Done (Round 3) |
| ~~TODO-SYS008-018~~ | P1 | Create dedicated SLAPoliciesController (endpoints currently inside KnowledgeAndCatalogControllers) | Backend | ✅ Done (Round 3) |
| TODO-SYS008-020 | P2 | Create dedicated SlaMatchingService class (logic is inline in SLAPolicyAdminService) | Backend | ⚠️ Partial |
| ~~TODO-SYS008-021~~ | P1 | Create SLAManagementPage React component | Frontend | ✅ Done (Round 4) — 467 lines, CRUD + summary cards + priority filters |
| TODO-SYS008-022 | P1 | Create EscalationRulesPanel React component | Frontend | ❌ Not Started |
| ~~TODO-SYS008-023~~ | P1 | Create QueueConfigPanel/ServiceQueuesPage React component | Frontend | ✅ Done (Round 4) — 512 lines, CRUD + queue items drawer |
| ~~TODO-SYS008-024~~ | P2 | Integrate Service Desk admin pages into navigation | Frontend | ✅ Done (Round 4) — SLA Policies + Service Queues in sidebar |
| TODO-SYS008-025 | P2 | Add SLA policy E2E tests | Testing | ❌ Not Started |
| TODO-SYS008-026 | P2 | Add escalation rule unit tests | Testing | ❌ Not Started |

*Completed: SLAPolicy entity/service ✅, EscalationRule entity/service ✅, ServiceQueueService ✅, EscalationRulesController ✅, SLA compliance endpoint ✅*

---

## 5. 🔗 INTEGRATION & WEBHOOKS

### 5.1 SPEC-INT-001 (Webhook Management)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-INT001-01 | P1 | Create general Webhook entity (only ITSM-scoped WebhookSubscription exists) | Backend | ❌ Not Started |
| TODO-INT001-02 | P1 | Create WebhookEvent entity (no general entity; ITSM-only DTOs exist) | Backend | ❌ Not Started |
| TODO-INT001-03 | P1 | Create general WebhookDelivery entity (ITSM-scoped version exists) | Backend | ⚠️ Partial |
| ~~TODO-INT001-06~~ | P2 | Extract SignatureGenerator as dedicated class (currently inline HMAC in WebhookService) | Backend | ✅ Done (Round 4) — WebhookSignatureGenerator.cs + IWebhookSignatureGenerator + 11 tests |
| ~~TODO-INT001-09~~ | P2 | Implement RetryPolicyEngine with exponential backoff (fixed 300s interval currently) | Backend | ✅ Done (Round 4) — WebhookRetryPolicy.cs + 16 tests |
| TODO-INT001-10 | P2 | Implement IDeliveryTracker interface | Backend | ❌ Not Started |
| TODO-INT001-11 | P2 | Implement DeliveryTracker for logging/metrics | Backend | ❌ Not Started |
| TODO-INT001-15 | P3 | Add feature flag for webhook system | Configuration | ❌ Not Started |
| TODO-INT001-21 | P2 | Extract WebhookList as standalone component (currently inline in WebhooksManagementPage) | Frontend | ⚠️ Partial |
| TODO-INT001-23 | P1 | Create EventTypeSelector multi-select component | Frontend | ❌ Not Started |
| TODO-INT001-24 | P2 | Implement EventFilterBuilder for advanced filters | Frontend | ❌ Not Started |
| TODO-INT001-25 | P1 | Implement WebhookTestSender UI with payload editor | Frontend | ❌ Not Started |
| TODO-INT001-26 | P2 | Implement DeliveryHistoryTable with sorting/filtering | Frontend | ❌ Not Started |
| TODO-INT001-27 | P2 | Implement DeliveryDetail modal for debugging | Frontend | ❌ Not Started |
| TODO-INT001-28 | P2 | Implement SignatureVerificationUI | Frontend | ❌ Not Started |
| TODO-INT001-31 | P2 | Implement webhook health monitoring dashboard | Frontend | ❌ Not Started |
| TODO-INT001-32 | P2 | Create unit tests for WebhookService | Testing | ❌ Not Started |
| ~~TODO-INT001-33~~ | P2 | Create unit tests for SignatureGenerator | Testing | ✅ Done (Round 4) — 11 tests in WebhookSignatureGeneratorTests.cs |
| ~~TODO-INT001-34~~ | P2 | Create unit tests for RetryPolicyEngine | Testing | ✅ Done (Round 4) — 16 tests in WebhookRetryPolicyTests.cs |
| TODO-INT001-35 | P2 | Create unit tests for WebhookDispatcher | Testing | ❌ Not Started |
| TODO-INT001-40 | P3 | Create E2E tests for webhook management | Testing | ❌ Not Started |
| ~~TODO-INT001-45~~ | P1 | Implement infinite loop prevention mechanism | Feature | ✅ Done (Round 4) — WebhookCircuitBreaker.cs + 8 tests |
| ~~TODO-INT001-46~~ | P1 | Implement auto-disable dead webhook logic | Feature | ✅ Done (Round 4) — ShouldDisableWebhook in RetryPolicy (10 consecutive failures) |
| TODO-INT001-47 | P2 | Implement large payload handling/chunking | Feature | ❌ Not Started |
| TODO-INT001-48 | P1 | Implement event chain tracking and cycle detection | Feature | ❌ Not Started |
| TODO-INT001-50 | P2 | Implement webhook analytics (success rate, latency) | Feature | ❌ Not Started |

*Completed: IWebhookService + WebhookService ✅, IWebhookDispatcher + WebhookDispatcherService ✅, WebhooksController + WebhookRegistrationsController ✅, WebhooksManagementPage.tsx ✅, WebhookForm.tsx ✅, webhookService.ts ✅*

### 5.2 SPEC-INT-002 (Provider Integration)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-INT002-002 | P2 | Create ProviderRegistryService implementation | Backend | ❌ Not Started |
| TODO-INT002-003 | P1 | Create AdminProvidersController endpoints | Backend | ❌ Not Started |
| TODO-INT002-004 | P2 | Implement provider switching UI (ProviderSelector component) | Frontend | ❌ Not Started |
| TODO-INT002-005 | P2 | Create provider configuration management page in admin | Frontend | ❌ Not Started |

*Completed: ProviderConfigurationService ✅, ProviderConfiguration entity ✅*

### 5.3 SPEC-INT-003 (Import/Export)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| TODO-INT003-002 | P1 | Create ImportMapping entity for reusable mappings | Backend | ❌ Not Started |
| TODO-INT003-003 | P1 | Create ImportError entity (currently inline in IImportExportService) | Backend | ⚠️ Partial |
| TODO-INT003-006 | P1 | Implement IDataValidator interface | Backend | ❌ Not Started |
| TODO-INT003-007 | P1 | Implement BatchProcessor for large files | Backend | ❌ Not Started |
| TODO-INT003-011 | P1 | Create ImportWizardPage React component | Frontend | ❌ Not Started |
| TODO-INT003-012 | P1 | Create ExportWizardPage React component | Frontend | ❌ Not Started |
| TODO-INT003-013 | P2 | Implement ColumnMapper component for field mapping | Frontend | ❌ Not Started |
| TODO-INT003-014 | P2 | Implement ImportPreview component | Frontend | ❌ Not Started |
| TODO-INT003-015 | P2 | Implement DuplicateHandler component | Frontend | ❌ Not Started |
| TODO-INT003-016 | P2 | Implement ImportProgress component with real-time updates | Frontend | ❌ Not Started |
| TODO-INT003-017 | P2 | Create unit tests for import validation | Testing | ❌ Not Started |
| TODO-INT003-018 | P2 | Create E2E tests for import/export flow | Testing | ❌ Not Started |

*Completed: ImportJob entity + DbSet ✅, IImportService (IImportJobService) ✅, IExportService (IExportJobService) ✅, ImportJobsController + ImportExportController ✅, ExportJobsController ✅, BulkImportDialog.tsx (basic CSV dialog) ✅*

---

## 6. 🔐 AUTH & SECURITY

### 6.1 SPEC-SYS-002 (Authentication)

| ID | Priority | Description | Category | Status |
|----|----------|-------------|----------|--------|
| ~~TODO-AUTH-001~~ | P1 | Wire LinkedIn OAuth to /api/auth/oauth-login endpoint (service+DI registered, no endpoint routing) | Backend | ✅ Done (Round 4) — GET/POST /api/auth/oauth/linkedin |
| ~~TODO-AUTH-002~~ | P1 | Wire Apple OAuth to /api/auth/oauth-login endpoint (service+DI registered, no endpoint routing) | Backend | ✅ Done (Round 4) — GET/POST /api/auth/oauth/apple |
| TODO-AUTH-003 | P1 | Implement Okta/Enterprise SSO support | Backend | ❌ Not Started |
| TODO-AUTH-004 | P1 | Add generic OpenID Connect provider | Backend | ❌ Not Started |
| TODO-AUTH-005 | P1 | Add OAuth provider state validation and CSRF protection | Backend | ❌ Not Started |
| TODO-AUTH-006 | P1 | Implement OAuth token refresh for long-lived sessions | Backend | ❌ Not Started |
| ~~TODO-AUTH-009~~ | P1 | Wire WebAuthn/FIDO2 to API endpoints (service exists, no controller endpoints) | Backend | ✅ Done (Round 4) — 6 endpoints (register options/complete, login options/complete, credentials list/delete) |
| TODO-AUTH-010 | P1 | Add biometric login (platform-specific) | Backend | ❌ Not Started |
| TODO-AUTH-011 | P1 | Add 2FA enforcement policies per user group | Backend | ❌ Not Started |
| TODO-AUTH-012 | P1 | Implement backup code regeneration | Backend | ❌ Not Started |
| TODO-AUTH-013 | P2 | Add concurrent session limit enforcement | Backend | ❌ Not Started |
| TODO-AUTH-014 | P2 | Implement password history validation (last 5) | Backend | ❌ Not Started |
| TODO-AUTH-015 | P2 | Implement IP-based session binding | Backend | ❌ Not Started |
| TODO-AUTH-016 | P2 | Add audit logging for all auth events | Backend | ❌ Not Started |
| TODO-AUTH-017 | P2 | Implement passwordless login (magic links) | Backend | ❌ Not Started |
| TODO-AUTH-018 | P2 | Add OAuth provider account linking/unlinking | Backend | ❌ Not Started |
| TODO-AUTH-019 | P2 | Implement 2FA device trust (remember device) | Backend | ❌ Not Started |
| TODO-AUTH-020 | P3 | Implement session activity tracking dashboard | Frontend | ❌ Not Started |
| TODO-AUTH-021 | P3 | Add login analytics and anomaly detection | Backend | ❌ Not Started |
| TODO-AUTH-022 | P3 | Implement risk-based authentication | Backend | ❌ Not Started |
| TODO-AUTH-023 | P3 | Add OAuth provider device flow support | Backend | ❌ Not Started |
| TODO-AUTH-024 | P3 | Implement geolocation-based login alerts | Backend | ❌ Not Started |

*Completed: Google/MS/GitHub OAuth ✅, TOTP 2FA ✅, SMS OTP ✅, Email OTP ✅, standard 2FA (setup/verify/enable/disable) ✅*

---

## 7. ⚙️ SYSTEM & ADMIN

### 7.1 SPEC-SYS-001 (User Management)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS001-001 | P2 | Align ALL password forms with backend policy (only SetupPasswordPage fetches requirements; login/profile change use hardcoded defaults) | ⚠️ Partial |
| TODO-SYS001-002 | P2 | Wire IAuditLogService into UserService for create/update/delete events (service exists, not wired) | ❌ Not Started |

*Completed: Centralized role-to-permission mapping (RBACService) ✅*

### 7.2 SPEC-SYS-003 (Group Management)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS003-001 | P2 | Enforce single default group rule (UserGroupService sets IsDefault with no check for existing default) | ❌ Not Started |
| TODO-SYS003-002 | P2 | Normalize AccessibleMenuItems with navigation config (stored as raw JSON string, no validation) | ❌ Not Started |
| TODO-SYS003-003 | P3 | Add membership audit logs | ❌ Not Started |

### 7.3 SPEC-SYS-006 (Audit Logging)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS006-001 | P1 | Implement field-level audit trail tracking (entity-level exists, no field-diff logic) | ❌ Not Started |
| TODO-SYS006-003 | P1 | Implement change history timeline visualization frontend | ❌ Not Started |
| TODO-SYS006-004 | P2 | Implement GDPR data access logging (Article 15) | ❌ Not Started |
| TODO-SYS006-005 | P2 | Create GDPR data export workflow | ❌ Not Started |
| TODO-SYS006-006 | P2 | Implement audit retention policy and archival | ❌ Not Started |
| TODO-SYS006-007 | P2 | Add audit log performance optimization (partitioning, cleanup jobs) | ❌ Not Started |
| TODO-SYS006-008 | P3 | Create audit log export (CSV/PDF/JSON) | ❌ Not Started |

*Completed: AuditLogService (entity-level) ✅, AuditLogsController ✅, AuditLoggingPage.tsx ✅*

### 7.4 SPEC-SYS-007 (Navigation Management)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS007-002 | P2 | Add role-based navigation filtering E2E tests | ❌ Not Started |
| TODO-SYS007-003 | P3 | Implement dynamic navigation reordering with drag-and-drop | ❌ Not Started |

*Completed: Audit logging for navigation changes ✅*

### 7.5 SPEC-SYS-008 (Admin Settings Suite — Testing)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS008-001 | P2 | Add admin settings navigation E2E tests | ❌ Not Started |
| TODO-SYS008-002 | P2 | Add unit tests for database/duplicate/lead-score controllers | ❌ Not Started |
| TODO-SYS008-003 | P2 | Validate admin pages against API contract | ❌ Not Started |
| TODO-SYS008-004 | P3 | Add missing UI empty states + loading UX | ❌ Not Started |
| TODO-SYS008-005 | P2 | Add sales settings (commission/discount) E2E tests | ❌ Not Started |
| TODO-SYS008-014 | P2 | Add commission rule unit tests for caps/splits/triggers | ❌ Not Started |

### 7.6 SPEC-SYS-009 (Administration Module)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS009-001 | P2 | Add admin settings E2E tests | ❌ Not Started |
| TODO-SYS009-002 | P2 | Add unit tests for navigation + system settings | ❌ Not Started |
| TODO-SYS009-003 | P2 | Complete provider-aware navigation merge | ❌ Not Started |
| TODO-SYS009-004 | P3 | Add audit logging for admin changes | ❌ Not Started |

### 7.7 SPEC-SYS-005 (System Settings)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS005-001 | P2 | Implement business hours configuration and validation | ❌ Not Started |
| TODO-SYS005-002 | P2 | Implement rate limiting service with quota tracking | ❌ Not Started |
| TODO-SYS005-003 | P1 | Add localization settings validation (timezone, currency, language) | ❌ Not Started |
| TODO-SYS005-004 | P2 | Create business hours configuration UI component | ❌ Not Started |

### 7.8 SPEC-SYS-012 (RBAC)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-SYS012-002 | P2 | Normalize group permission flags with navigation filtering | ❌ Not Started |
| TODO-SYS012-003 | P2 | Add audit logging for RBAC permission changes | ❌ Not Started |

*Completed: Centralized role/permission mapping (RBACService) ✅*

---

## 8. 📋 CRM CORE GAPS

### 8.1 Lead Management (SPEC-CRM-002)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-CRM002-01 | ✅ DONE | Lead scoring algorithm (AILeadScoringService + LeadScoreRulesController) | ✅ Done |
| TODO-CRM002-02 | ✅ DONE | Lead conversion workflow (LeadsController.Convert + ConvertLeadDto) | ✅ Done |
| TODO-CRM002-03 | P2 | Add lead source tracking and attribution | ❌ Not Started |
| TODO-CRM002-04 | P2 | Implement web-to-lead form builder integration | ❌ Not Started |
| TODO-CRM002-05 | P2 | Add duplicate lead detection during creation | ❌ Not Started |
| TODO-CRM002-06 | P2 | Implement lead nurturing campaign integration | ❌ Not Started |
| TODO-CRM002-07 | P3 | Add lead aging alerts and stale lead notifications | ❌ Not Started |
| TODO-CRM002-08 | P3 | Implement lead qualification matrix (BANT/MEDDIC) | ❌ Not Started |

### 8.2 Opportunity Management (SPEC-CRM-003)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-CRM003-01 | ✅ DONE | Weighted pipeline value (DashboardController + OpportunitiesController) | ✅ Done |
| TODO-CRM003-02 | P1 | Auto-update Probability when Stage changes (StagesController has static mapping, not auto-applied) | ⚠️ Partial |
| TODO-CRM003-03 | P2 | Implement competitor tracking on opportunities | ❌ Not Started |
| TODO-CRM003-04 | P2 | Add POST/DELETE /api/opportunities/{id}/products endpoints for post-creation line item management | ⚠️ Partial |
| TODO-CRM003-05 | P2 | Implement win/loss analysis reports | ❌ Not Started |
| TODO-CRM003-06 | P3 | Add opportunity cloning functionality | ❌ Not Started |
| TODO-CRM003-07 | P2 | Implement forecast category assignment | ❌ Not Started |
| TODO-CRM003-08 | P2 | Add opportunity team/split commission tracking | ❌ Not Started |

### 8.3 Sales Process Gaps

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-GAP-03 | ✅ DONE | Sales forecasting service (SalesForecastService + SalesForecastsController) | ✅ Done |
| TODO-GAP-04 | P2 | Implement territory-based lead assignment | ❌ Not Started |
| TODO-GAP-05 | P2 | Add full multi-currency service (ExchangeRate fields exist, no CurrencyService or live rates) | ⚠️ Partial |
| TODO-GAP-06 | P2 | CPQ bundle wizard UI (ProductBundle entity + controller exist, no frontend wizard) | ⚠️ Partial |
| TODO-GAP-07 | P2 | Dynamic pricing rules engine (PriceBooksController exists, no dynamic rules engine) | ❌ Not Started |
| TODO-GAP-08 | ✅ DONE | Quote approval workflow (ApprovalWorkflowService + ApprovalsController) | ✅ Done |
| TODO-GAP-SALES-001 | P2 | Complete order returns workflow | ❌ Not Started |
| TODO-GAP-SALES-002 | P2 | Commission details panel & UI | ❌ Not Started |

---

## 9. 🔧 INFRASTRUCTURE & DEVOPS

### 9.1 Background Processing

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-INFRA-01 | P2 | Enable Hangfire (fully coded in Program.cs lines 712–763 but 100% commented out) | ⚠️ Partial |
| TODO-INFRA-04 | P3 | Add RabbitMQ/Redis Streams for async event processing | ❌ Not Started |
| TODO-INFRA-05 | P3 | Implement event sourcing for audit-critical entities | ❌ Not Started |
| TODO-INFRA-06 | P3 | Add dead letter queue handling | ❌ Not Started |
| TODO-INFRA-07 | P3 | Implement saga pattern for distributed transactions | ❌ Not Started |

*Completed: .NET 10 upgrade ✅, Polly retry policies ✅, circuit breaker (ResilienceService) ✅*

### 9.2 Search

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-INFRA-08 | P2 | Extend full-text search indexing to all entities (currently only 5: accounts, contacts, opportunities, products, knowledge_articles) | ⚠️ Partial |
| TODO-INFRA-09 | P2 | Add search result highlighting and faceted search | ❌ Not Started |
| TODO-INFRA-10 | P3 | Implement search analytics (popular queries, zero results) | ❌ Not Started |

---

## 10. 🎨 FRONTEND UX

### 10.1 Important UI Features

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-UX-06 | P1 | Implement global search with typeahead (cross-entity, no component exists) | ❌ Not Started |
| TODO-UX-07 | P1 | Add inline editing for data grid cells | ❌ Not Started |
| TODO-UX-09 | P2 | Implement generic bulk action toolbar for list views (IncidentBulkActionTools exists for ITSM only) | ⚠️ Partial |
| TODO-UX-10 | P2 | Implement advanced filter builder widget (AdvancedSearch.tsx is per-page, not a reusable builder) | ⚠️ Partial |

*Completed: UX-08 Drag-and-drop pipeline board (PipelineKanban.tsx) ✅*

### 10.2 Type Safety (GAP-FRONTEND-001)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-GAP-FRONTEND-001 | P2 | Fix remaining `any` type usages in service files (53 instances found — significantly fewer than Feb 16 estimate of 200+) | ⚠️ Partial |

### 10.3 Accessibility (WCAG 2.1 AA)

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-UX-01 | P2 | Add ARIA labels to all interactive components | ❌ Not Started |
| TODO-UX-02 | P2 | Implement keyboard navigation for data grids | ❌ Not Started |
| TODO-UX-03 | P2 | Add screen reader support for charts and dashboards | ❌ Not Started |
| TODO-UX-04 | P3 | High contrast theme option | ❌ Not Started |
| TODO-UX-05 | P3 | Font size adjustment controls | ❌ Not Started |

### 10.4 Nice-to-Have UX

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-UX-11 | P3 | Dark mode toggle | ❌ Not Started |
| TODO-UX-12 | P3 | Customizable sidebar navigation | ❌ Not Started |
| TODO-UX-13 | P3 | Split view for comparing records | ❌ Not Started |
| TODO-UX-15 | P3 | Recent items quick access | ❌ Not Started |

*Completed: Breadcrumbs.tsx ✅, PipelineKanban drag-and-drop ✅*

---

## 11. 📊 ANALYTICS & REPORTING

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-AI005-FE-001 | P2 | Add end-to-end analytics tests for dashboards and reports | ❌ Not Started |
| TODO-AI005-FE-002 | P2 | Define JSON schema versioning for report query payloads | ❌ Not Started |
| TODO-AI005-FE-005 | P2 | Align analytics embed API routes with backend controllers | ❌ Not Started |
| TODO-AI005-FE-006 | P3 | Validate filter value types in ReportDesigner | ❌ Not Started |
| TODO-RPT-03 | P2 | Report sharing and permissions | ❌ Not Started |
| TODO-RPT-04 | P3 | Report templates marketplace | ❌ Not Started |
| TODO-RPT-06 | P2 | Real-time dashboard with WebSocket live updates | ❌ Not Started |
| TODO-RPT-07 | P2 | Cohort analysis and customer segmentation | ❌ Not Started |
| TODO-RPT-08 | P3 | Funnel visualization with stage conversion rates | ❌ Not Started |
| TODO-RPT-09 | P3 | Geographic data visualization (map charts) | ❌ Not Started |
| TODO-GAP-MARKETING-001 | P2 | Campaign & lead scoring widgets for dashboard | ❌ Not Started |

*Completed: ReportDesigner.tsx ✅, DashboardBuilder.tsx ✅, DashboardBuilder save flow wired ✅, report scheduling/export UI ✅*

---

## 12. 🌐 SELF-SERVICE PORTAL & MOBILE

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-PORTAL-01 | P3 | Customer portal with ticket submission and tracking | ❌ Not Started |
| TODO-PORTAL-02 | P3 | Self-service KB search with article feedback | ❌ Not Started |
| TODO-PORTAL-03 | P3 | Partner portal with deal registration | ❌ Not Started |
| TODO-PORTAL-04 | P3 | Community forums with moderation tools | ❌ Not Started |
| TODO-PORTAL-05 | P3 | User-configurable dashboard layouts | ❌ Not Started |
| TODO-PORTAL-06 | P3 | Saved search and filter presets | ❌ Not Started |
| TODO-PORTAL-07 | P3 | Custom notification preferences per entity type | ❌ Not Started |
| TODO-PORTAL-08 | P3 | Personalized email digest configuration | ❌ Not Started |
| TODO-PORTAL-09 | P3 | Progressive Web App (PWA) support | ❌ Not Started |
| TODO-PORTAL-10 | P3 | Offline mode for core CRM features | ❌ Not Started |
| TODO-PORTAL-11 | P3 | Push notifications for mobile | ❌ Not Started |
| TODO-PORTAL-12 | P3 | Touch-optimized UI for tablets | ❌ Not Started |

---

## 13. 🤖 AI & MACHINE LEARNING

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-AI-03 | P3 | Customer churn prediction | ❌ Not Started |
| TODO-AI-04 | P3 | Next best action recommendations | ❌ Not Started |
| TODO-AI-07 | P3 | Automated email sentiment analysis | ❌ Not Started |
| TODO-AI-08 | P3 | Meeting summary generation | ❌ Not Started |
| TODO-AI-09 | P3 | Deal risk scoring | ❌ Not Started |
| TODO-AI-10 | P3 | Revenue forecasting with ML | ❌ Not Started |

*Completed: LeadScoringAgent ✅, DealIntelligenceAgent ✅, KnowledgeExpertAgent ✅, EmailAssistantAgent ✅, AIKnowledgeSearchService (semantic search + embeddings) ✅*

---

## 14. 🔌 INTEGRATION FRAMEWORK

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-INT-01 | ✅ DONE | Stripe webhook handlers (StripeWebhookController, 10+ event types) | ✅ Done |
| TODO-INT-05 | P2 | Microsoft Teams integration for notifications | ❌ Not Started |
| TODO-INT-06 | P3 | Slack integration for notifications | ❌ Not Started |
| TODO-INT-07 | P3 | Twilio enhanced voice call logging | ❌ Not Started |
| TODO-INT-08 | P3 | QuickBooks/Xero accounting sync | ❌ Not Started |
| TODO-INT-09 | P3 | Mailchimp/HubSpot marketing sync | ❌ Not Started |
| TODO-INT-10 | P3 | LinkedIn Sales Navigator integration | ❌ Not Started |
| TODO-INT-11 | P3 | Calendly/Cal.com scheduling integration | ❌ Not Started |

*Completed: SendGrid event tracking integration ✅, Chatwoot timeline integration ✅*

---

## 15. 🛠️ CUSTOMIZATION ENGINE

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-CUST-01 | P2 | Custom field builder with drag-and-drop UI | ❌ Not Started |
| TODO-CUST-02 | P2 | Custom field validation rules | ❌ Not Started |
| TODO-CUST-03 | P2 | Custom field search and filtering | ❌ Not Started |
| TODO-CUST-04 | P3 | Custom page layouts per entity type | ❌ Not Started |
| TODO-CUST-05 | P3 | Configurable list view columns | ❌ Not Started |
| TODO-CUST-06 | P3 | Custom button/action definitions | ❌ Not Started |
| TODO-CUST-07 | P3 | Formula fields with expression engine | ❌ Not Started |
| TODO-CUST-08 | P3 | Rollup summary fields | ❌ Not Started |
| TODO-CUST-09 | P3 | Cross-object formula references | ❌ Not Started |
| TODO-CUST-10 | P3 | Sandbox environment support | ❌ Not Started |
| TODO-CUST-11 | P3 | Configuration migration between environments | ❌ Not Started |
| TODO-CUST-12 | P3 | Feature flag management UI | ❌ Not Started |

---

## 16. 📚 DOCUMENTATION

| ID | Priority | Description | Status |
|----|----------|-------------|--------|
| TODO-DOC-01 | P2 | Create ITSM User Guide | ❌ Not Started |
| TODO-DOC-02 | P2 | Update README.md with ITSM module section | ❌ Not Started |
| TODO-DOC-03 | P2 | Update architecture diagrams for ITSM services | ❌ Not Started |
| TODO-DOC-04 | P2 | Update Swagger/OpenAPI documentation for all new endpoints | ❌ Not Started |
| TODO-DOC-05 | P3 | Fix remaining StyleCop warnings | ❌ Not Started |
| TODO-DOC-06 | P3 | Add missing XML documentation to public APIs | ❌ Not Started |

---

## 17. ✅ COMPLETED ARCHIVE (Verified Feb 23, 2026)

> Items confirmed in codebase by 6-agent audit. Preserved for regression tracing.

### Backend Services Completed
- GAP-BACKEND-001: BusinessHoursCalculator, IncidentService, SLAService, ServiceQueueService ✅
- GAP-BACKEND-007: EmailSequenceService, EmailSequenceManagementService, CampaignExecutionService ✅
- SALES-007: CommissionsController, CommissionPlansController, CommissionCalculationsController, CommissionPayoutsController, CommissionCalculationService, CommissionPlanAssignment, full test suite ✅
- SALES-006 services: RecurringBillingEngine, DunningManager, ProrateCalculator, SubscriptionMetricsAggregator ✅
- SALES-004: PaymentsController, PaymentDto, CreatePaymentDto, StripeWebhookController ✅
- SD-005: EscalationRulesController, EscalationPoliciesController, EscalationHostedService, EscalationWorker, EscalationPolicyService ✅
- SD-003: SLAEnforcementHostedService, BusinessHoursCalculator (full timezone), SLA compliance endpoint ✅
- SD-001: EmailToTicketService, EmailToTicketController, SLA auto-calc on create ✅
- SD-002: AIKnowledgeSearchService (embeddings + semantic search) ✅
- SYS-008: SLAPolicyAdminService, ServiceQueueService, EscalationRuleAdminService ✅
- CRM-003: Lead conversion (LeadsController.Convert), SalesForecastService, Quote approval workflow ✅
- CRM-002: AILeadScoringService, LeadScoreRulesController ✅
- AUTH: Google/MS/GitHub OAuth, TOTP 2FA, SMS OTP, Email OTP, standard 2FA endpoints ✅
- RBAC: RBACService, RolePermission entity, centralized mapping ✅
- Infra: .NET 10 upgrade ✅, ResilienceService (Polly retry + circuit breaker) ✅
- Integration: ProviderConfigurationService ✅, IWebhookService + WebhookService + dispatching ✅
- Import/Export: ImportJob entity, ImportJobsController, ExportJobsController, IImportJobService, IExportJobService ✅
- DB: EmailSequence EF config ✅, Web tracking indexes ✅, ITSM migration 010 ✅

### Database Entities Completed
- Subscriptions, SubscriptionItems, SubscriptionUsages tables (DB + migration) ✅
- BillingEventType enum (as BillingEventType) ✅
- CommissionPlanAssignment entity + DbSet ✅
- ProviderConfiguration entity + DbSet ✅

### DTOs Completed
- InvoiceDto, CreateInvoiceDto, UpdateInvoiceDto ✅
- PaymentDto, CreatePaymentDto, ProcessPaymentRequestDto (note: named differently from spec) ✅
- SubscriptionMetricsDto ✅
- WebAuthnDtos ✅

### Frontend Completed
- ContractsPage.tsx, contractService.ts ✅
- PaymentsPage.tsx, paymentService.ts ✅
- SubscriptionsPage.tsx (single page with tabs), subscriptionService.ts ✅
- CommissionsPage.tsx, commissionService.ts ✅
- WebhooksManagementPage.tsx, WebhookForm.tsx, webhookService.ts ✅
- itsmService.ts (fully typed) ✅
- All 31 ITSM pages migrated to MUI (no Tailwind) ✅
- ChangeManagement pages (ChangeManagementPage, ChangeListPage, ChangeDetailPage, ChangeFormPage, ChangeApprovalPage, ChangeCalendarPage) ✅
- SLACountdownWidget.tsx, SLABreachAlert.tsx, ArticleFeedbackWidget.tsx ✅
- PipelineKanban.tsx (drag-and-drop) ✅
- AuditLoggingPage.tsx, admin/AuditLoggingPage.tsx ✅
- DashboardBuilder.tsx, ReportDesigner.tsx, reportService.ts ✅
- SignalRContext.tsx + CrmNotificationHub backend ✅
- BulkImportDialog.tsx (basic CSV upload stepper) ✅

### Tests Completed
- 425 test files, 0 excluded, all active ✅
- 17 ITSM unit test files (tests/Services/ITSM/) ✅
- 6 ITSM controller test files ✅
- InvoiceServiceTests, ContractServiceTests, PaymentServiceTests ✅
- InvoicesControllerTests, ContractsControllerTests, PaymentsControllerTests ✅
- CommissionServiceTests, CommissionRuleServiceTests, 4 commission controller test files ✅
- SubscriptionServiceTests, SubscriptionsControllerTests ✅
- E2E service-requests.spec.ts ✅

### Specification Files Created (since Feb 16)
- SPEC-SYS-002-Authentication.md ✅
- SPEC-ITSM-001-IncidentManagement.md through SPEC-ITSM-004-CMDB.md ✅
- SPEC-INT-001-WebhookManagement.md, SPEC-INT-002-ProviderIntegration.md, SPEC-INT-003-ImportExport.md ✅
- SPEC-AI-003-ChurnPrediction.md, SPEC-AI-004-EmailIntelligence.md ✅
- SPEC-ARCH-001 through SPEC-ARCH-006 + SPEC-ARCH-013 ✅

---

## 18. 🎯 PRIORITY MATRIX & IMPLEMENTATION TIMELINE

### Summary by Priority (Revised Feb 23, 2026)

| Priority | Count | Key Items |
|----------|-------|-----------|
| **P0 — Critical** | 8 | SubscriptionRenewal entity+DB, BillingHistory/DunningRecord DbSet registration, SubscriptionBillingController, ChangeManagementService gaps (26 methods) |
| **P1 — High** | 52 | Frontend pages (InvoiceDetailsPage, ContractDetailsPage, EscalationPages), AdminConfigurationController re-enable, Auth OAuth wiring, ServiceQueuesController, ARCH specs 007-012, secrets manager, field-level audit trail |
| **P2 — Medium** | 118 | Sub-components (forms, badges, dialogs), contract/subscription validations, SD testing, auth advanced (concurrent sessions, device trust), GDPR, import/export wizard, commission engine (caps/splits) |
| **P3 — Low** | 65 | Portal, mobile, customization engine, advanced analytics, nice-to-have UX, remaining integrations |
| **Total Pending** | **~243** | (down from 301 on Feb 16 — ~58 items confirmed complete) |

### Recommended Spring Schedule

| Sprint | Focus | Items | Timeline |
|--------|-------|-------|----------|
| **Sprint 1** | Critical DB gaps (BillingHistory/DunningRecord registration, SubscriptionRenewal), ChangeManagementService completion | 12 | Week 1-2 |
| **Sprint 2** | Frontend sub-component decomposition (Invoice, Payment, Contract, Subscription) | 20 | Week 3-4 |
| **Sprint 3** | Escalation frontend (4 pages), ServiceQueuesController, AdminConfigurationController re-enable | 12 | Week 5-6 |
| **Sprint 4** | Auth wiring (LinkedIn/Apple OAuth endpoints, WebAuthn controller), 2FA policies | 10 | Week 7-8 |
| **Sprint 5** | Import/Export wizard UI, remaining arch specs (007-012) | 12 | Week 9-10 |
| **Sprint 6** | Test coverage expansion (E2E for invoices, contracts, subscriptions, escalation) | 15 | Week 11-12 |
| **Backlog** | Portal, mobile, customization engine, P3 items | 162 | Q3-Q4 2026 |

---

## 19. 🔎 REGRESSION PREVENTION

### Build Validation

```bash
# Backend tests — target: all 425 files passing
cd CRM.Backend && dotnet test --verbosity normal

# Frontend tests
cd CRM.Frontend && npm test -- --coverage

# E2E smoke tests
cd e2e-tests && npx playwright test --grep @smoke

# Build validation
dotnet build --configuration Release && npm run build
```

### Current Baselines (Feb 23, 2026)
- ✅ 425 test files, 0 excluded
- ✅ .NET 10 across all projects
- ✅ 0 disabled test files
- ⚠️ Hangfire commented out (subscription billing jobs not executing)
- ⚠️ BillingHistory, DunningRecord, SubscriptionRenewal entities have no DB tables

---

**Document Maintenance:** Updated February 23, 2026 (6-Agent Codebase Audit)  
**Prepared by:** GitHub Copilot + 6 Specialized Sub-Agents  
**Version:** 0.571.0  
**Next Review:** March 2, 2026

**END OF MASTER TODO LIST**
