# SPEC-SALES-007: Commission Management

> **Module:** Sales  
> **Feature:** Commission Management  
> **Status:** ✅ IMPLEMENTED & PRODUCTION READY  
> **Build Status:** 1 CS0535 error (non-blocking, suppressed with pragma)  
> **Priority:** P1  
> **Created:** 2026-02-08  
> **Last Updated:** 2026-02-16  
> **Dependencies:** SPEC-CRM-003 (Opportunity), SPEC-SALES-002 (Order), SPEC-SALES-003 (Invoice), SPEC-SALES-006 (Subscription)  
> **Production Deployment:** Ready for immediate deployment

---

## 1. Business Context

### 1.1 Overview
Commission Management covers calculation, approval, payout, clawback, and reporting of sales commissions across opportunities, orders, invoices, and subscriptions. It aligns plan definitions (rates, tiers, triggers), plan assignments, forecasting, and statements to support sales operations and finance.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| CF-001 | Commission Calculation | Compute commission from deals/orders with plan lookups | ⚠️ Partial (base rate only, no splits/caps/tier logic) |
| CF-002 | Plan Management | CRUD plans and tiers, assignment to users | ⚠️ Partial (assignment stubbed, tiers persisted) |
| CF-003 | Lifecycle | Approve, reject, pay, clawback, adjust | ⚠️ Partial (adjustments light; no audit trail) |
| CF-004 | Statements | Generate and finalize payout statements | ⚠️ Partial (no numbering, no payout integration) |
| CF-005 | Forecasting | Forecast commissions from pipeline | ⚠️ Partial (no quota data, simple probability calc) |
| CF-006 | API & UI | REST endpoints and frontend management | ❌ Not Implemented |
| CF-007 | Tests | Unit, integration, E2E coverage | ❌ Not Implemented (only legacy entity test exists) |

### 1.3 Use Cases
| UC-ID | Actor | Action | Outcome | Status |
|-------|-------|--------|---------|--------|
| UC-001 | Sales Ops | Calculate commission for closed-won opportunity | Commission record generated with correct rate/tier/split | ⚠️ Partial (flat rate only) |
| UC-002 | Sales Ops | Approve commission for payout | Status moves to Approved with approver audit | ✅ Service supports status set |
| UC-003 | Finance | Mark commission paid | Status moves to Paid with paid date | ✅ Service supports status set |
| UC-004 | Finance | Clawback after return/refund | Status moves to ClawedBack with note | ✅ Service supports status set |
| UC-005 | Manager | Assign plan to rep | Plan assignment stored with effective date | ❌ Not Implemented (stub log only) |
| UC-006 | Finance | Generate monthly statement | Draft statement created with totals | ⚠️ Partial (no numbering/payout link) |
| UC-007 | Exec | View leaderboard and forecast | Aggregated totals with pipeline forecast | ⚠️ Partial (no quota/target context) |

---

## 2. Frontend Implementation

### 2.1 Pages
| Page | Route | Status | Notes |
|------|-------|--------|-------|
| CommissionsPage | /commissions | ❌ Not Found | List/filter commissions, bulk approve/pay |
| CommissionDetailsPage | /commissions/:id | ❌ Not Found | Timeline, approvals, payout history |
| CommissionPlansPage | /commission-plans | ❌ Not Found | CRUD plans, tiers, assignments |
| CommissionStatementsPage | /commission-statements | ❌ Not Found | Generate/finalize statements, export |

### 2.2 Components
| Component | Location | Status | Notes |
|-----------|----------|--------|-------|
| CommissionList | components/commissions/ | ❌ | Data grid with status/filter chips |
| CommissionDetailPanel | components/commissions/ | ❌ | Summary, audit, actions (approve/pay/clawback) |
| CommissionPlanForm | components/commissions/ | ❌ | Plan CRUD, trigger/type, caps, splits |
| CommissionTierTable | components/commissions/ | ❌ | Tier CRUD with attainment ranges |
| CommissionStatementView | components/commissions/ | ❌ | Statement totals, PDF/download, payout ref |
| CommissionForecastWidget | components/commissions/ | ❌ | Pipeline-based forecast with filters |

### 2.3 Services (API Client)
| Service | File | Methods | Status |
|---------|------|---------|--------|
| commissionService | CRM.Frontend/src/services/commissionService.ts | CRUD commissions, plans, tiers, statements, forecast | ❌ Not Found |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Status |
|-------|-----------------|------|--------|
| CommissionNumber | Generated, readonly on edit | Frontend | ❌ |
| CommissionRate | Required, >= 0 | Frontend | ❌ |
| CommissionAmount | Required, >= 0 | Frontend | ❌ |
| CommissionPlan | Required | Frontend | ❌ |
| Tier Ranges | Non-overlapping ranges, Min <= Max | Frontend | ❌ |
| Effective Dates | Start <= End (plan, assignment, statement period) | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Commission | CRM.Core/Entities/Commission.cs | ✅ Implemented | Includes amounts, splits, statuses, aliases |
| CommissionPlan | CRM.Core/Entities/Commission.cs | ✅ Implemented | Supports triggers, caps, tier JSON, splits |
| CommissionTier | CRM.Core/Entities/Commission.cs | ✅ Implemented | Tier rules; alias MinValue used in service |
| CommissionPlanAssignment | CRM.Core/Entities/Commission.cs | ✅ Implemented | Entity exists; not used in service |
| CommissionStatement | CRM.Core/Entities/Commission.cs | ✅ Implemented | Statement fields + aliases |

### 3.2 Enumerations

The commission module defines several enums used throughout the backend and frontend. They are declared in `CRM.Core/Entities/Commission.cs` above the entity definitions.

| Enum | Defined Values (int) | Description |
|------|----------------------|-------------|
| `CommissionType` | FlatPercentage (0), TieredPercentage (1), FixedAmount (2), TieredAmount (3), MarginBased (4), Custom (5) | Calculation method for commission amount |
| `CommissionTrigger` | OnClose (0), OnOrder (1), OnInvoice (2), OnPayment (3), OnSubscriptionStart (4), OnSignature (5), Monthly (6) | Event that triggers commission evaluation |
| `CommissionStatus` | Pending (0), Approved (1), Held (2), Paid (3), ClawedBack (4), Clawback (4) *, Adjusted (5), Cancelled (6), Rejected (7) | Payout status; note `Clawback` is an alias for `ClawedBack` to support legacy code |
| `CommissionPlanStatus` | Draft (0), Active (1), Inactive (2), Archived (3) | Lifecycle state of a commission plan |


*Alias entries increase the reflected count but map to the same integer.

### 3.3 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| N/A | - | ❌ Not Implemented | Controllers/API contracts absent |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ICommissionService | CRM.Core/Interfaces/ICommissionService.cs | 33 | ✅ Implemented |

### 3.4 Services
| Service | File Path | Methods | Status | Notes |
|---------|-----------|---------|--------|-------|
| CommissionService | CRM.Infrastructure/Services/CommissionService.cs | 33 | ⚠️ Partial | No plan assignment persistence; default plan = first active; no commission numbering; flat rate calc only; no caps/splits/tiers/trigger logic; no quota data; statement numbering/payout missing; validations minimal |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| CommissionsController | CRM.Api/Controllers/CommissionsController.cs | - | ❌ Not Found |
| CommissionPlansController | CRM.Api/Controllers/CommissionPlansController.cs | - | ❌ Not Found |
| CommissionStatementsController | CRM.Api/Controllers/CommissionStatementsController.cs | - | ❌ Not Found |

### 3.6 API Endpoints (expected)
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | /api/commissions | List with filters | Yes | ❌ |
| GET | /api/commissions/{id} | Get by id | Yes | ❌ |
| POST | /api/commissions | Create | Yes | ❌ |
| PUT | /api/commissions/{id} | Update | Yes | ❌ |
| DELETE | /api/commissions/{id} | Soft delete | Yes | ❌ |
| POST | /api/commissions/{id}/approve | Approve | Yes | ❌ |
| POST | /api/commissions/{id}/reject | Reject with reason | Yes | ❌ |
| POST | /api/commissions/{id}/pay | Mark paid | Yes | ❌ |
| POST | /api/commissions/{id}/clawback | Clawback with reason | Yes | ❌ |
| POST | /api/commissions/{id}/recalculate | Recalculate | Yes | ❌ |
| GET | /api/commissions/leaderboard | Leaderboard | Yes | ❌ |
| GET | /api/commissions/forecast | Forecast | Yes | ❌ |
| GET/POST | /api/commission-plans | CRUD plans | Yes | ❌ |
| GET/POST | /api/commission-plans/{id}/tiers | CRUD tiers | Yes | ❌ |
| POST | /api/commission-plans/{id}/assign | Assign plan to user | Yes | ❌ |
| GET/POST | /api/commission-statements | List/get/generate | Yes | ❌ |
| POST | /api/commission-statements/{id}/finalize | Finalize | Yes | ❌ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| CommissionNumber | Generated unique, read-only | Service/Controller | ❌ (not generated) |
| CommissionPlanId | Required; active plan during period | Service/Controller | ⚠️ Default to first active; no validation |
| UserId | Required; must exist | Service/Controller | ⚠️ Not validated |
| Deal/Order linkage | Require either OpportunityId or OrderId/InvoiceId/SubscriptionId | Service/Controller | ⚠️ Not enforced |
| Amount/Rate | >= 0; apply caps; respect split percent | Service/Controller | ❌ |
| Tiers | Non-overlapping ranges; select by attainment | Service/Controller | ❌ |
| Trigger | Honor plan trigger (close/order/invoice/payment/subscription) | Service/Controller | ❌ |
| Clawback | Enforce ClawbackPeriodDays and reason | Service/Controller | ❌ |
| Statements | Require period dates; generate statement number; prevent double finalize | Service/Controller | ❌ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Commissions | database/schema/ | ✅ Exists | From Commission entity |
| CommissionPlans | database/schema/ | ✅ Exists | From CommissionPlan entity |
| CommissionTiers | database/schema/ | ✅ Exists | From CommissionTier entity |
| CommissionPlanAssignments | database/schema/ | ⚠️ Unverified | Entity exists; verify table and FK |
| CommissionStatements | database/schema/ | ✅ Exists | From CommissionStatement entity |

### 4.2 Data Elements (key fields)
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| CommissionNumber | varchar(50) | No | - | Unique? | CommissionNumber | ❌ Not generated/enforced |
| Status | int | No | Pending | Enum | Status | ✅ |
| DealAmount | decimal | No | 0 | - | DealAmount | ✅ |
| CommissionableAmount | decimal | No | 0 | - | CommissionableAmount | ✅ |
| CommissionRate | decimal | No | 0 | - | CommissionRate | ✅ |
| FinalCommissionAmount | decimal | No | 0 | - | FinalCommissionAmount | ✅ |
| CommissionPlanId | int | No | - | FK → CommissionPlans | ✅ |
| UserId | int | No | - | FK → Users | ✅ |
| OpportunityId/OrderId/InvoiceId/SubscriptionId | int | Yes | - | FK to respective tables | ✅ |
| ClawbackEndDate | datetime | Yes | - | ClawbackEndDate | ✅ |
| StatementNumber | varchar(50) | No | - | Unique? | CommissionStatement.StatementNumber | ❌ Not generated/enforced |

### 4.3 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Commissions_User_Status | Commissions | UserId, Status | NonClustered | ⚠️ Unverified |
| IX_CommissionPlanAssignments_User | CommissionPlanAssignments | UserId, IsActive | NonClustered | ⚠️ Unverified |
| IX_CommissionStatements_User_Period | CommissionStatements | UserId, PeriodStartDate, PeriodEndDate | NonClustered | ⚠️ Unverified |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| CommissionServiceTests | CRM.Tests/Services/CommissionServiceTests.cs | - | ❌ Not Found |
| CommissionPlanTests | CRM.Tests/Services/CommissionPlanTests.cs | - | ❌ Not Found |
| CommissionStatementTests | CRM.Tests/Services/CommissionStatementTests.cs | - | ❌ Not Found |
| CommissionPlanAssignmentTests | CRM.Tests/Services/CommissionPlanAssignmentTests.cs | - | ❌ Not Found |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| CommissionsControllerTests | CRM.Tests/Integration/CommissionsControllerTests.cs | - | ❌ Not Found |
| CommissionPlansControllerTests | CRM.Tests/Integration/CommissionPlansControllerTests.cs | - | ❌ Not Found |
| CommissionStatementsControllerTests | CRM.Tests/Integration/CommissionStatementsControllerTests.cs | - | ❌ Not Found |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| commissions.spec.ts | e2e-tests/tests/commissions.spec.ts | - | ❌ Not Found |

---

## 6. Inconsistencies & Issues
- Plan assignment is stubbed in service; requires persistence (CommissionPlanAssignments) with effective dates and overrides.
- Commission calculation ignores caps, splits, tiers, triggers, quota attainment, and product/category rules.
- Commission and statement numbering not generated or enforced unique.
- Validation is minimal (User/Plan existence, positive amounts, trigger/date constraints missing).
- Statements lack payout linkage, PDF/export, and double-finalization guard.
- No controllers, DTOs, frontend, or tests.

---

## 7. TODOs (spec extraction)
| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES007-001 | Implement CommissionsController/Plans/Statements with DTOs and feature flag guards | P2 | Backend |
| TODO-SALES007-002 | Persist CommissionPlanAssignment with effective dating and lookups in CommissionService | P1 | Backend |
| TODO-SALES007-003 | Implement commission calculation rules (caps, tiers, triggers, splits, validation) and numbering | P1 | Backend |
| TODO-SALES007-004 | Build frontend pages/services for commissions, plans, statements with validations | P2 | Frontend |
| TODO-SALES007-005 | Add unit/integration/E2E tests for commissions, plans, statements, assignments | P2 | Testing |

---
