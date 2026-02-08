# SPEC-SALES-006: Subscription Management

> **Module:** Sales  
> **Feature:** Subscription Management  
> **Status:** ⚠️ Partial  
> **Priority:** P1  
> **Created:** 2026-02-08  
> **Last Updated:** 2026-02-08  
> **Dependencies:** SPEC-CRM-001 (Account), SPEC-SALES-002 (Order), SPEC-SALES-003 (Invoice)

---

## 1. Business Context

### 1.1 Overview
Subscription Management covers creation, lifecycle control, billing, renewals, and usage tracking for recurring products or services sold to Accounts. It aligns contract/billing dates, supports plan changes and add-ons, and calculates MRR/ARR and churn metrics.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Subscription Creation | Create manually or from orders | ✅ Implemented (service)
| SF-002 | Lifecycle & Status | Activate, pause, suspend, cancel, reactivate | ✅ Implemented (service)
| SF-003 | Plan Changes | Upgrade/downgrade/change plan with timing | ✅ Implemented (service)
| SF-004 | Billing & Invoicing | Generate invoices, compute prorations, next billing date | ⚠️ Partial (invoice number random, billing detail update minimal)
| SF-005 | Renewals | Auto/explicit renewal and due-for-renewal listings | ✅ Implemented (service)
| SF-006 | Usage Tracking | Record usage, retrieve usage metrics and limits | ⚠️ Partial (usage limits placeholder)
| SF-007 | Reporting | MRR/ARR/churn and statistics | ✅ Implemented (service)
| SF-008 | API & UI | REST endpoints, frontend pages/services | ❌ Not Implemented

### 1.3 Use Cases
| UC-ID | Actor | Action | Outcome | Status |
|-------|-------|--------|---------|--------|
| UC-001 | Sales Ops | Create subscription from an order | New active subscription with numbers seeded from order totals | ✅ Service only
| UC-002 | Account Manager | Pause/resume subscription | Status transitions to Paused/Active with audit note | ✅ Service only
| UC-003 | Billing | Generate invoice for period | Draft invoice created for subscription amount and due date | ⚠️ Partial (random number, flat 30-day terms)
| UC-004 | Customer Success | Schedule plan downgrade at period end | Notes recorded; change applied at next cycle | ✅ Service only
| UC-005 | System | Identify subscriptions due for renewal | Returns list filtered by cutoff date | ✅ Service only
| UC-006 | Billing | View usage and limits | Usage records returned; limits currently empty | ⚠️ Partial
| UC-007 | Sales Rep | Manage subscriptions via UI | CRUD, status changes, billing history | ❌ Not Implemented (UI/API missing)

---

## 2. Frontend Implementation

### 2.1 Pages
| Page | Route | Status | Notes |
|------|-------|--------|-------|
| SubscriptionsPage | /subscriptions | ❌ Not Found | List/filter subscriptions |
| SubscriptionDetailsPage | /subscriptions/:id | ❌ Not Found | Details, billing history, usage |
| SubscriptionFormPage | /subscriptions/new | ❌ Not Found | Create/edit with billing settings |

### 2.2 Components
| Component | Location | Status | Notes |
|-----------|----------|--------|-------|
| SubscriptionList | components/subscriptions/ | ❌ Not Found | Data grid with status filters |
| SubscriptionForm | components/subscriptions/ | ❌ Not Found | Validations for required fields and dates |
| SubscriptionStatusBadge | components/subscriptions/ | ❌ Not Found | Status indicator |
| SubscriptionBillingPanel | components/subscriptions/ | ❌ Not Found | Billing cycle, invoice history |
| SubscriptionUsageWidget | components/subscriptions/ | ❌ Not Found | Usage metrics and limits |

### 2.3 Services (API Client)
| Service | File | Methods | Status |
|---------|------|---------|--------|
| subscriptionService | CRM.Frontend/src/services/subscriptionService.ts | CRUD, lifecycle, billing, usage | ❌ Not Found |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| AccountId | Required | Frontend | ❌ |
| SubscriptionNumber | Required, readonly on edit | Frontend | ❌ |
| Amount | Required, >= 0 | Frontend | ❌ |
| BillingCycle | Required; allowed Weekly/Monthly/Quarterly/Yearly | Frontend | ❌ |
| StartDate/EndDate | Required; EndDate >= StartDate | Frontend | ❌ |
| Contact Email | Valid email if provided | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Subscription | CRM.Core/Entities/Subscription.cs | ✅ Implemented | Maps to Accounts table for backward compatibility; includes billing, contract, MRR/ARR fields |
| SubscriptionItem | CRM.Core/Entities/Subscription.cs | ✅ Implemented | Line items with Product linkage |
| SubscriptionUsage | CRM.Core/Entities/Subscription.cs | ✅ Implemented | Usage records with metric name/quantity |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| N/A | - | ❌ Not Implemented | Controller-level DTOs do not exist yet |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| ISubscriptionService | CRM.Core/Interfaces/ISubscriptionService.cs | 24 | ✅ Implemented |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| SubscriptionService | CRM.Infrastructure/Services/SubscriptionService.cs | 24 | ⚠️ Partial (usage limits placeholder; billing detail update minimal; invoice number random; validations light) |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| SubscriptionsController | CRM.Api/Controllers/SubscriptionsController.cs | - | ❌ Not Found |

### 3.6 API Endpoints (expected)
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | /api/subscriptions | List with filters | Yes | ❌ Not Implemented |
| GET | /api/subscriptions/{id} | Get by id | Yes | ❌ Not Implemented |
| POST | /api/subscriptions | Create | Yes | ❌ Not Implemented |
| PUT | /api/subscriptions/{id} | Update | Yes | ❌ Not Implemented |
| DELETE | /api/subscriptions/{id} | Soft delete | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/activate | Activate | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/pause | Pause with reason | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/resume | Resume | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/cancel | Cancel (immediate or period-end) | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/suspend | Suspend | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/reactivate | Reactivate | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/renew | Renew | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/plan | Change/upgrade/downgrade plan | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/addons | Add/remove add-ons | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/invoice | Generate invoice | Yes | ❌ Not Implemented |
| GET | /api/subscriptions/{id}/billing-history | Billing history | Yes | ❌ Not Implemented |
| POST | /api/subscriptions/{id}/usage | Record usage | Yes | ❌ Not Implemented |
| GET | /api/subscriptions/{id}/usage | Usage metrics | Yes | ❌ Not Implemented |
| GET | /api/subscriptions/{id}/usage-limits | Usage limits | Yes | ❌ Not Implemented |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| AccountId | Required | Service/Controller | ❌ Not Enforced |
| SubscriptionNumber | Generated unique | Service | ⚠️ Random format SUB-yyMM-#### without collision check |
| Amount | Required, >= 0 | Service/Controller | ❌ Not Enforced |
| BillingCycle/BillingPeriod | Must be Weekly/Monthly/Quarterly/Yearly | Service/Controller | ❌ Not Enforced |
| StartDate/EndDate | StartDate required; EndDate >= StartDate | Service/Controller | ❌ Not Enforced |
| CancelAtPeriodEnd | Only when not already cancelled | Service/Controller | ❌ Not Enforced |
| Usage Limits | Persisted limits for GetUsageLimitsAsync | Service/DB | ❌ Not Implemented |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Accounts (Subscription) | database/schema/ (Accounts table) | ✅ Exists | Subscription maps to Accounts via `[Table("Accounts")]` | 
| SubscriptionUsages | database/schema/ | ⚠️ Unverified | Table implied by entity; verify presence |
| SubscriptionItems | database/schema/ | ⚠️ Unverified | Table implied by entity; verify presence |

### 4.2 Data Elements (Subscription → Accounts)
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| AccountNumber | varchar(50) | No | - | Unique? (not enforced) | SubscriptionNumber | ⚠️ Partial |
| CustomerId | int | No | - | FK → Customers | AccountId | ✅ |
| Status | int | No | 0 | Enum | SubscriptionStatus | ✅ |
| MRR/ARR | decimal | Yes | 0 | - | MRR/ARR | ✅ |
| BillingCycle | varchar(50) | Yes | Monthly | - | BillingCycle | ✅ |
| BillingStartDate/EndDate | datetime | Yes | - | BillingStartDate/BillingEndDate | ✅ |
| ContractStartDate/EndDate | datetime | Yes | - | ContractStartDate/ContractEndDate | ✅ |
| IsAutoRenew | bool | Yes | false | - | IsAutoRenew | ✅ |
| Tags | varchar(500) | Yes | - | - | Tags | ✅ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Accounts (Subscription) | Products | N:1 | ProductId | ✅ |
| Accounts (Subscription) | Customers | N:1 | CustomerId | ✅ |
| SubscriptionItems | Products | N:1 | ProductId | ✅ |
| SubscriptionUsages | SubscriptionItems | N:1 | SubscriptionItemId | ✅ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Accounts_AccountNumber | Accounts | AccountNumber | NonClustered | ⚠️ Unverified |
| IX_SubscriptionUsages_SubscriptionId | SubscriptionUsages | SubscriptionId | NonClustered | ⚠️ Unverified |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| SubscriptionServiceTests | CRM.Tests/Services/SubscriptionServiceTests.cs | - | ❌ Not Found |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| SubscriptionsControllerTests | CRM.Tests/Integration/SubscriptionsControllerTests.cs | - | ❌ Not Found |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| subscriptions.spec.ts | e2e-tests/tests/subscriptions.spec.ts | - | ❌ Not Found |

---

## 6. Inconsistencies & Issues

### 6.1 Data/Model
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Subscription maps to Accounts table | Service uses Subscription entity | Name mismatch could confuse reporting/tools | Documented backward compatibility; keep alias |
| GenerateInvoiceAsync | Invoice numbering | Uses random 4-digit suffix without uniqueness check | TODO-SALES006-005 |
| UpdateBillingDetailsAsync | Billing fields | Only sets address/city; ignores other fields | TODO-SALES006-004 |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| SubscriptionsController with REST endpoints | CRM.Api/Controllers/ | Not created | TODO-SALES006-001 |
| Frontend pages/components/service | CRM.Frontend/src/ | Not created | TODO-SALES006-002 |
| Usage limits persistence & GetUsageLimitsAsync | DB + SubscriptionService | Placeholder returns empty list | TODO-SALES006-003 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| AccountId | Required not enforced | TODO-SALES006-004 |
| Amount/Plan price | Should be >= 0 | TODO-SALES006-004 |
| BillingCycle | Must be constrained to enum values | TODO-SALES006-004 |
| StartDate/EndDate | EndDate must be >= StartDate | TODO-SALES006-004 |
| SubscriptionNumber | Needs uniqueness check | TODO-SALES006-005 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES006-001 | Create SubscriptionsController with REST endpoints covering CRUD/lifecycle/billing/usage to match service | P1 | Backend/API |
| TODO-SALES006-002 | Build frontend subscriptions pages, components, and API client | P2 | Frontend |
| TODO-SALES006-003 | Implement usage limits persistence and wire GetUsageLimitsAsync | P2 | Backend |
| TODO-SALES006-004 | Add validations for required AccountId, Amount>=0, allowed BillingCycle, Start/End date ordering, and full billing detail updates | P1 | Validation |
| TODO-SALES006-005 | Make invoice number generation deterministic/unique and enforce SubscriptionNumber uniqueness | P2 | Backend |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-08 | System | Initial specification |
