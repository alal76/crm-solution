# SPEC-SALES-006: Subscription Management

> **Module:** Sales  
> **Feature:** Subscription Management  
> **Status:** ✅ Complete  
> **Priority:** P1  
> **Created:** 2026-02-08  
> **Last Updated:** 2026-02-14  
> **Dependencies:** SPEC-CRM-001 (Account), SPEC-SALES-002 (Order), SPEC-SALES-003 (Invoice), SPEC-SALES-004 (Payment)  
> **Reference:** PHASE4_SERVICE_SPECIFICATIONS.md - ISubscriptionService

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

### 1.3 Use Cases (12 Critical Scenarios)
| UC-ID | Title | Actor(s) | Preconditions | Steps | Expected Outcome | Priority |
|-------|-------|---------|----------------|-------|-----------------|----------|
| UC-001 | Create Subscription | Sales Ops | Account exists, Plan available | 1. Select account 2. Choose plan/amount 3. Set billing date 4. Set trial period (optional) 5. Enable auto-renewal | New subscription ACTIVE (or TRIAL if trial period set) | P0 |
| UC-002 | Activate Subscription | Billing Manager | Subscription exists in DRAFT | 1. Set activation date 2. Record start date | Subscription transitions to ACTIVE; first invoice scheduled | P0 |
| UC-003 | Pause Subscription | Account Manager | Subscription ACTIVE | 1. Select pause date 2. Enter reason | Subscription PAUSED; no invoices until resumed | P1 |
| UC-004 | Resume Paused Subscription | Account Manager | Subscription PAUSED | 1. Select resume date | Subscription returns to ACTIVE; billing resumes | P1 |
| UC-005 | Upgrade Plan | Sales Rep | Subscription ACTIVE | 1. Select new plan 2. Choose effective date (immediate/end-of-period) | If immediate: prorate credit/charge; if EOD: change applies at next cycle | P1 |
| UC-006 | Downgrade Plan | Account Manager | Subscription ACTIVE | 1. Select new plan 2. Choose effective date | If immediate: issue credit; if EOD: apply at next cycle; send approval request if price > threshold | P1 |
| UC-007 | Add On | Sales Rep | Subscription ACTIVE | 1. Select add-on product 2. Set quantity | Subscription updated; prorated charge added to next invoice | P1 |
| UC-008 | Remove Add-On | Account Manager | Subscription with active add-on | 1. Deselect add-on | Add-on removed; prorated credit applied | P1 |
| UC-009 | Cancel Subscription | Customer (via Portal) or Agent | Subscription ACTIVE | 1. Select cancellation date (immediate/EOD) 2. Enter reason 3. If cancellation fee: approve/pay | If immediate: last invoice issued, CANCELLED status. If EOD: flag for cancellation at next renewal, send confirmation | P0 |
| UC-010 | Generate Invoice | Billing System | Subscription ACTIVE, billing date = today | 1. Fetch subscription 2. Calculate proration if plan changed 3. Generate invoice number 4. Create line items 5. Calculate total + tax | Invoice created in DRAFT; added to customer's Invoice tab; notification sent | P0 |
| UC-011 | Record Usage | System/Integration | Subscription with usage-based metering | 1. Receive usage event (metric name, quantity, timestamp) 2. Check usage limits 3. Calculate overage if > limit 4. Accumulate for next invoice | Usage record persisted; overage flagged for billing cycle; customer notified if approaching limit | P1 |
| UC-012 | Process Renewal | Billing System (Scheduled) | Subscription ACTIVE, renewal date = today | 1. Check auto-renewal flag 2. If auto-renew: generate new renewal record, schedule next billing 3. Send renewal notification | New SubscriptionRenewal record created; billing dates extended; customer notified of next charge | P0 |

---

## 1.4 Proration Algorithms

**Proration applies when plan changes mid-billing-cycle or subscription is paused/resumed/cancelled early.**

### Algorithm 1: Pro-Rata (Time-Based)
```
Daily Rate = Monthly Amount / Number of Days in Month
Days Used = Last Day of Use - First Day of Use + 1
Prorated Amount = Daily Rate × Days Used
```
**Example:** Month has 30 days; customer used 10 days. Daily = $30/30 = $1/day. Prorated = $1 × 10 = $10.

**Implementation:** Use DECIMAL(18,4) for calculations; round to DECIMAL(18,2) for storage.

### Algorithm 2: Full Price (No Proration)
```
Prorated Amount = Plan Amount (no adjustment)
```
**Use Case:** Plan change on renewal date only; no mid-month charges.

### Algorithm 3: One Month (Grace Period)
```
Prorated Amount = Plan Amount (full charge for month)
```
**Use Case:** Customer downgrading mid-cycle gets credit equal to 1 month.

---

## 1.5 Dunning Management (Failed Payment Recovery)

**Dunning applies when an invoice payment fails; retry up to 3 times before auto-canceling.**

| Attempt | Day Offset | Action | Conditions |
|---------|-----------|--------|------------|
| 1st Retry | +3 days | Charge payment method; send retry email | If payment fails |
| 2nd Retry | +6 days | Charge payment method; send escalation email | If 1st fails |
| 3rd Retry | +9 days | Charge payment method; send final warning email | If 2nd fails |
| Final | +12 days | Auto-cancel subscription; send cancellation notice | If all 3 fail |

**Implementation:** Use Hangfire to schedule retry jobs; store DunningRecord with attempt count and next retry date.

---

## 1.6 Revenue Metrics

### Monthly Recurring Revenue (MRR)
```
MRR = SUM(Subscription.Amount WHERE BillingCycle = 'Monthly' AND Status IN ['Active', 'Paused']) 
    + SUM(Subscription.Amount WHERE BillingCycle = 'Quarterly' / 3 AND Status IN ['Active', 'Paused'])
    + SUM(Subscription.Amount WHERE BillingCycle = 'Yearly' / 12 AND Status IN ['Active', 'Paused'])
```

### Annual Recurring Revenue (ARR)
```
ARR = MRR × 12
```

### Churn Rate
```
Churn Rate = (Subscriptions Cancelled This Month / Active Subscriptions at Month Start) × 100
```

**Example:** 100 active subs on day 1; 5 cancelled in month. Churn = (5/100) × 100 = 5%.

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

### 3.1 Entities (Full Property Lists)

#### 3.1.1 Subscription Entity
```csharp
public class Subscription : BaseEntity
{
    // Core
    public string SubscriptionNumber { get; set; } = string.Empty;  // SUB-yyMM-0001
    public int AccountId { get; set; }
    public int? PlanId { get; set; }
    public Account? Account { get; set; }
    
    // Billing
    public string BillingCycle { get; set; } = "Monthly";  // Weekly/Monthly/Quarterly/Yearly
    public decimal Amount { get; set; }  // Subscription amount
    public DateTime BillingStartDate { get; set; }
    public DateTime? BillingEndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    // Status
    public string Status { get; set; } = "Active";  // Draft/Active/Paused/Suspended/Cancelled
    public string? StatusReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Trial
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsTrialActive { get; set; }
    
    // Contract
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public int? ContractLengthMonths { get; set; }
    
    // Auto Renewal & Timing
    public bool IsAutoRenewal { get; set; } = true;
    public int RenewalCount { get; set; }
    public DateTime? CancelAtPeriodEnd { get; set; }
    public string? ProrationType { get; set; } = "ProRata";  // ProRata/FullPrice/OneMonth/None
    public DateTime? PauseUntilDate { get; set; }
    
    // Metrics
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public decimal LifetimeValue { get; set; }
    
    // Notes & Tags
    public string? Notes { get; set; }
    public List<SubscriptionItem> Items { get; set; } = new();
    public List<SubscriptionUsage> Usage { get; set; } = new();
    public List<SubscriptionRenewal> Renewals { get; set; } = new();
    public List<BillingHistory> BillingHistory { get; set; } = new();
    public List<DunningRecord> DunningRecords { get; set; } = new();
}
```

#### 3.1.2 SubscriptionItem Entity (Add-ons, Line Items)
```csharp
public class SubscriptionItem : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public bool IsAddon { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
```

#### 3.1.3 SubscriptionUsage Entity (Usage-Based Billing)
```csharp
public class SubscriptionUsage : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public string MetricName { get; set; } = string.Empty;  // e.g., "API Calls", "GB Storage"
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }  // "count", "GB", etc.
    public DateTime? UsageDate { get; set; }
    public int BillingCycle { get; set; }  // Month/Year to group usage
    public decimal OverageAmount { get; set; }
    public bool Invoiced { get; set; }
}
```

#### 3.1.4 SubscriptionRenewal Entity (Renewal History)
```csharp
public class SubscriptionRenewal : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public DateTime RenewalDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool AutoRenewed { get; set; }
    public int RenewalCount { get; set; }
    public string? Status { get; set; }  // Pending/Completed/Failed
    public int? InvoiceId { get; set; }  // Link to generated invoice
}
```

#### 3.1.5 BillingHistory Entity (Event Audit)
```csharp
public class BillingHistory : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public string EventType { get; set; } = string.Empty;  // Created/Activated/PlanChanged/Invoiced/Cancelled/Renewed
    public string? EventDetails { get; set; }
    public int? UserId { get; set; }
    public DateTime EventDate { get; set; }
}
```

#### 3.1.6 DunningRecord Entity (Payment Recovery)
```csharp
public class DunningRecord : BaseEntity
{
    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public int? InvoiceId { get; set; }
    public int RetryAttempt { get; set; }  // 1, 2, 3
    public DateTime? NextRetryDate { get; set; }
    public string? LastErrorMessage { get; set; }
    public bool Exhausted { get; set; }  // 3 retries failed
    public DateTime? CancelledAt { get; set; }
}
```

### 3.2 DTOs

```csharp
public class CreateSubscriptionDto
{
    public int AccountId { get; set; }
    public int? PlanId { get; set; }
    public decimal Amount { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
    public DateTime BillingStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsAutoRenewal { get; set; } = true;
}

public class SubscriptionDto
{
    public int Id { get; set; }
    public string SubscriptionNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public DateTime BillingStartDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public List<SubscriptionItemDto> Items { get; set; } = new();
}

public class SubscriptionItemDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsAddon { get; set; }
}

public class PlanChangeDto
{
    public int NewPlanId { get; set; }
    public string ChangeType { get; set; } = string.Empty;  // Immediate/EndOfPeriod
    public string? ProrationType { get; set; } = "ProRata";
}

public class SubscriptionUsageDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
}

public class BillingDetailsDto
{
    public string? BillingEmail { get; set; }
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string? BillingCountry { get; set; }
}

public class SubscriptionStatisticsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int PausedSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public double ChurnRate { get; set; }
    public double ConversionRate { get; set; }
}
```

### 3.3 Interfaces (From PHASE4_SERVICE_SPECIFICATIONS.md)

**ISubscriptionService**: 50+ methods covering:
- CRUD Operations (GetAll, GetById, Create, Update, Delete)
- Subscription Lifecycle (Activate, Pause, Resume, Cancel, Suspend, Reactivate)
- Plan Changes (Upgrade, Downgrade, ChangePlan)
- Billing (GenerateInvoice, CalculateProratedAmount, GetNextBillingDate, UpdateBillingDetails)
- Renewal (Renew, GetDueForRenewal, SetAutoRenewal)
- Usage (RecordUsage, GetUsage, GetUsageLimits)
- Queries (GetActiveSubscriptions, GetExpiringSubscriptions, GetByDateRange, SearchAsync)
- Metrics (CalculateMRR, CalculateARR, GetChurnRate, GetStatistics)

**Specialized Interfaces:**
- `IRecurringBillingEngine`: ProcessMonthlyBilling, ProcessAnnualBilling, CalculateProration
- `IUsageMetricsService`: RecordUsage, CalculateOverage, GetProjection
- `IDunningManager`: InitiateDunning, ProcessRetry, HandleExhaustion
- `IRenewalService`: ScheduleRenewal, ProcessRenewal, SendNotification
- `IProrateCalculator`: Calculate methods for each proration type
- `ISubscriptionMetricsAggregator`: CalculateMRR, CalculateARR, CalculateChurn, CalculateLTV

### 3.4 Services (Implementation Classes)

| Service | Methods | Key Responsibilities |
|---------|---------|----------------------|
| SubscriptionService | 50+ | CRUD, lifecycle (activate/pause/resume/cancel), plan changes, billing, renewal |
| RecurringBillingEngine | 15+ | Monthly/annual invoice generation, prorated charge calculations |
| UsageMetricsService | 20+ | Record usage events, calculate overage charges, track usage limits |
| DunningManager | 12+ | Retry failed payments up to 3 times, cancel on exhaustion, send notifications |
| RenewalService | 18+ | Schedule renewals, process auto-renewals, send renewal notifications, handle failures |
| ProrateCalculator | 4+ | Pro-rata, full price, one-month, and no-proration calculations |
| SubscriptionMetricsAggregator | 4+ | Calculate MRR, ARR, churn rate, lifetime value (LTV) |

### 3.5 Controllers (4 Controllers, 49+ Endpoints)

| Controller | Endpoints | Purpose |
|------------|-----------|---------|
| SubscriptionsController | 25+ | CRUD, lifecycle operations, plan changes, add-ons, billing history |
| SubscriptionBillingController | 8+ | Generate invoices, record payments, view billing history, proration details |
| SubscriptionUsageController | 10+ | Record usage, retrieve metrics, check limits, get projections |
| SubscriptionAnalyticsController | 6+ | MRR/ARR/churn queries, statistics, leaderboards, forecasts |

### 3.6 Key API Endpoints

```
GET    /api/subscriptions                           List with filters (status, customerId, billingCycle)
GET    /api/subscriptions/{id}                      Get by ID
POST   /api/subscriptions                           Create new
PUT    /api/subscriptions/{id}                      Update
DELETE /api/subscriptions/{id}                      Soft delete

POST   /api/subscriptions/{id}/activate             Activate
POST   /api/subscriptions/{id}/pause                Pause with reason
POST   /api/subscriptions/{id}/resume               Resume
POST   /api/subscriptions/{id}/cancel               Cancel (immediate or EOD)
POST   /api/subscriptions/{id}/suspend              Suspend
POST   /api/subscriptions/{id}/reactivate           Reactivate
POST   /api/subscriptions/{id}/plan                 Change/upgrade/downgrade plan
POST   /api/subscriptions/{id}/addons               Add/remove add-ons

POST   /api/subscriptions/{id}/invoice              Generate invoice
GET    /api/subscriptions/{id}/billing-history     Billing history
GET    /api/subscriptions/due-for-renewal          List subscriptions due for renewal

POST   /api/subscriptions/{id}/usage                Record usage
GET    /api/subscriptions/{id}/usage                Get usage metrics
GET    /api/subscriptions/{id}/usage-limits        Get usage limits

GET    /api/subscriptions/analytics/mrr            MRR calculation
GET    /api/subscriptions/analytics/arr            ARR calculation
GET    /api/subscriptions/analytics/churn          Churn rate
GET    /api/subscriptions/analytics/statistics     Summary statistics
```

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

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|----------|-------|------------|
| Subscription.Amount (DECIMAL 18,2) | Invoice.Amount (DECIMAL 18,2) | Type match ✓ | No action |
| SubscriptionUsage.Quantity (DECIMAL 18,4) | InvoiceLineItem.Quantity (DECIMAL 18,2) | Precision mismatch: usage tracks 4 decimals, invoice 2 | TODO-SALES006-004: Standardize to DECIMAL(18,2) or handle rounding |
| BillingHistory.EventType (VARCHAR 50) | Activity.ActivityType (INT enum) | Inconsistent typing: string vs enum | TODO-SALES006-005: Create EventType enum, use throughout |
| Subscription.BillingCycle (VARCHAR 20) | No enum in code | String-based instead of enum | TODO-SALES006-006: Create BillingCycle enum in CRM.Core/Enums |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| SubscriptionItem entity | CRM.Core/Entities/SubscriptionItem.cs | Tracks plan + add-ons within subscription | TODO-SALES006-010 |
| SubscriptionRenewal entity | CRM.Core/Entities/SubscriptionRenewal.cs | Tracks renewal history and dates | TODO-SALES006-011 |
| BillingHistory entity | CRM.Core/Entities/BillingHistory.cs | Audit trail for billing events | TODO-SALES006-012 |
| DunningRecord entity | CRM.Core/Entities/DunningRecord.cs | Failed payment recovery tracking | TODO-SALES006-013 |
| SubscriptionsController | CRM.Api/Controllers/SubscriptionsController.cs | 25+ endpoints for subscription management | ✅ TODO-SALES006-001 |
| SubscriptionBillingController | CRM.Api/Controllers/SubscriptionBillingController.cs | Invoice, payment, billing endpoints (11) | ✅ TODO-SALES006-002 — Completed Feb 24 |
| SubscriptionUsageController | CRM.Api/Controllers/SubscriptionUsageController.cs | Usage tracking and limits (10) | ✅ TODO-SALES006-003 — Completed Feb 24 |
| RecurringBillingEngine service | CRM.Infrastructure/Services/RecurringBillingEngine.cs | Automatic monthly/annual billing | TODO-SALES006-014 |
| DunningManager service | CRM.Infrastructure/Services/DunningManager.cs | Retry logic for failed payments | TODO-SALES006-015 |
| ProrateCalculator service | CRM.Infrastructure/Services/ProrateCalculator.cs | 4 proration algorithms | TODO-SALES006-016 |
| SubscriptionMetricsAggregator service | CRM.Infrastructure/Services/SubscriptionMetricsAggregator.cs | MRR/ARR/churn calculations | TODO-SALES006-017 |
| Frontend pages | CRM.Frontend/src/pages/Subscription*.tsx | 5 pages (Dashboard, Details, PlanSelector, BillingHistory, UsageAnalytics) | TODO-SALES006-030 |
| Frontend components | CRM.Frontend/src/components/sales/ | 10 components (cards, forms, widgets) | TODO-SALES006-031 |
| subscriptionService.ts | CRM.Frontend/src/services/ | API client methods | TODO-SALES006-032 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| SubscriptionNumber | No format validation or uniqueness enforcement at DB level | TODO-SALES006-018 |
| Amount | No validation that amount >= 0.01 | TODO-SALES006-018 |
| BillingCycle | No validation that value is in allowed set (Weekly/Monthly/Quarterly/Yearly/Custom) | TODO-SALES006-018 |
| BillingStartDate | No validation that date is >= today (or >= now if immediate activation) | TODO-SALES006-018 |
| BillingEndDate | No validation that EndDate >= StartDate | TODO-SALES006-018 |
| TrialEndDate | No validation that trial end > trial start | TODO-SALES006-019 |
| ProrationType | No validation that value is one of: ProRata, FullPrice, OneMonth, None | TODO-SALES006-019 |
| Auto-renewal flag | Not validated against cancellation status | TODO-SALES006-020 |
| Timezone handling | Billing dates calculated in UTC; customer displays may be incorrect | TODO-SALES006-029 |
| Usage limits | No validation of usage limit values (should be >= 0) | TODO-SALES006-020 |

### 6.4 Known Technical Challenges
| Challenge | Impact | Mitigation | TODO ID |
|-----------|--------|-----------|---------|
| Floating-point precision in proration | Rounding errors accumulate; total charged != sum of itemized | Use DECIMAL(18,4) for calculations, round to DECIMAL(18,2) at storage | TODO-SALES006-021 |
| Concurrent plan changes | Race condition if user changes plan twice rapidly | Implement optimistic locking (RowVersion); reject 2nd with 409 Conflict | TODO-SALES006-022 |
| Timezone handling | Billing cycle boundaries depend on customer timezone, not UTC | Store customer timezone in Account; calculate billing dates in TZ; display in TZ | TODO-SALES006-023 |
| Large-scale usage recording | Metering 1000s of events/second causes DB contention | Batch usage records; aggregate hourly before creating invoice line items | TODO-SALES006-024 |
| Dunning retry exhaustion | After 3 failed retries, subscription auto-cancels; customer loses access | Grace period (3 days) after exhaustion; escalation emails before cancellation | TODO-SALES006-025 |
| Refund reconciliation | Credits issued but not consistently applied to future invoices | Create CreditTransaction entity; link to future invoices; maintain audit trail | TODO-SALES006-026 |

---

## 7. TODO Items → Master TODO List (50 Items)

### Service & Entity Implementation (13)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| ~~TODO-SALES006-001~~ | Create SubscriptionsController with 25+ CRUD/lifecycle/plan/addon endpoints | P0 | Backend/API | ✅ Implemented |
| ~~TODO-SALES006-002~~ | Create SubscriptionBillingController with invoice/payment/history endpoints (8+) | P0 | Backend/API | ✅ Completed Feb 24 — 11 endpoints |
| ~~TODO-SALES006-003~~ | Create SubscriptionUsageController with usage/limits/projection endpoints (10+) | P1 | Backend/API | ✅ Completed Feb 24 — 10 endpoints |
| ~~TODO-SALES006-027~~ | Implement subscription pause with scheduled resume | P1 | Backend/Feature | ✅ Completed Feb 24 — ResumeAt field + migration |
| ~~TODO-SALES006-028~~ | Implement trial to paid conversion workflow | P1 | Backend/Feature | ✅ Completed Feb 24 — convert-trial endpoints |
| ~~TODO-SALES006-040~~ | Create SubscriptionAnalyticsController (MRR/ARR/churn/growth) | P1 | Backend/Controller | ✅ Completed Feb 24 — 7 endpoints |
| TODO-SALES006-010 | Create SubscriptionItem entity for tracking plan + add-ons within subscription | P0 | Entity |
| TODO-SALES006-011 | Create SubscriptionRenewal entity for renewal history and dates | P0 | Entity |
| TODO-SALES006-012 | Create BillingHistory entity for billing event audit trail | P0 | Entity |
| TODO-SALES006-013 | Create DunningRecord entity for failed payment recovery tracking | P1 | Entity |
| TODO-SALES006-014 | Implement RecurringBillingEngine service with ProcessMonthlyBilling scheduled job (Hangfire) | P0 | Service |
| TODO-SALES006-015 | Implement DunningManager service with 3-retry exhaustion and escalation workflow | P0 | Service |
| TODO-SALES006-016 | Implement ProrateCalculator service with 4 proration methods (ProRata/FullPrice/OneMonth/None) | P0 | Service |
| TODO-SALES006-017 | Implement SubscriptionMetricsAggregator for MRR/ARR/churn/LTV calculations | P1 | Service |
| TODO-SALES006-032 | Implement subscriptionService.ts frontend API client with all methods | P1 | Frontend |
| TODO-SALES006-033 | Implement billingService.ts frontend API client (history, invoices, payments) | P1 | Frontend |

### Database & Schema (5)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES006-034 | Create Subscriptions table with status/billing/contract/metrics columns | P0 | Database |
| TODO-SALES006-035 | Create SubscriptionItems table (plan + add-ons) with FK to Subscriptions/Products | P0 | Database |
| TODO-SALES006-036 | Create SubscriptionUsages table with metric name/quantity/billing cycle | P0 | Database |
| TODO-SALES006-037 | Create SubscriptionRenewals table with renewal dates and invoice links | P0 | Database |
| TODO-SALES006-038 | Create BillingHistory and DunningRecords tables with audit/retry tracking | P0 | Database |

### Validation & Data Consistency (8)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES006-004 | Standardize usage quantity precision: DECIMAL(18,4) vs (18,2) across entities | P1 | Data |
| TODO-SALES006-005 | Create EventType enum for BillingHistory; refactor string to enum | P2 | Code Quality |
| TODO-SALES006-006 | Create BillingCycle enum; replace string-based values in code | P2 | Code Quality |
| TODO-SALES006-018 | Add validation for SubscriptionNumber format, Amount >= 0.01, BillingCycle enum, date ordering | P1 | Validation |
| TODO-SALES006-019 | Add validation for trial date ordering, proration type, usage limits >= 0 | P2 | Validation |
| TODO-SALES006-020 | Add validation: auto-renewal cannot be true if cancelled; usage limits >= 0 | P2 | Validation |
| TODO-SALES006-021 | Use DECIMAL(18,4) for proration calculations; implement safe rounding to (18,2) at storage | P1 | Data |
| TODO-SALES006-022 | Implement optimistic locking (RowVersion) on Subscriptions; return 409 on race conditions | P1 | Concurrency |

### Feature Implementation (14)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES006-023 | Add timezone support: store customer timezone in Account; calculate/display billing dates in TZ | P2 | Feature |
| TODO-SALES006-024 | Implement usage record batching: collect in memory, aggregate hourly before invoice line items | P2 | Performance |
| TODO-SALES006-025 | Add dunning grace period (3 days after exhaustion); send escalation emails before cancellation | P2 | Feature |
| TODO-SALES006-026 | Create CreditTransaction entity; explicitly link credits to future invoice line items | P2 | Entity |
| TODO-SALES006-027 | Implement subscription pause with scheduled resume: PauseUntilDate calculation and automation | P1 | Feature |
| TODO-SALES006-028 | Implement trial to paid conversion: auto-activate after trial, charge first full amount | P1 | Feature |
| TODO-SALES006-029 | Implement timezone handling for billing date display in UI (use account timezone) | P2 | Frontend |
| TODO-SALES006-030 | Create 5 frontend pages: Dashboard, Details, PlanSelector, BillingHistory, UsageAnalytics | P0 | Frontend |
| TODO-SALES006-031 | Create 10 frontend components: SubscriptionCard, PlanComparison, UsageGauge, BillingTimeline, etc. | P0 | Frontend |
| TODO-SALES006-040 | Create SubscriptionAnalyticsController with MRR/ARR/churn/stats endpoints (6+) | P1 | Backend/API |

### Testing (10)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-SALES006-041 | Unit tests: Proration accuracy (ProRata, FullPrice, OneMonth, None) with 20+ scenarios | P0 | Testing |
| TODO-SALES006-042 | Unit tests: Usage billing accuracy (tiered rates, overage, limits) with 15+ scenarios | P0 | Testing |
| TODO-SALES006-043 | Unit tests: MRR/ARR calculation precision with 100+ sample subscriptions | P0 | Testing |
| TODO-SALES006-044 | Unit tests: Churn rate calculation (month-over-month active count) | P1 | Testing |
| TODO-SALES006-045 | Integration tests: Auto-renewal workflow (expiry → payment → new cycle → invoice) | P1 | Testing |
| TODO-SALES006-046 | Integration tests: Dunning workflow (3 retries, escalation, cancellation) | P1 | Testing |
| TODO-SALES006-047 | Integration tests: Plan change with proration (immediate and end-of-period) | P1 | Testing |
| TODO-SALES006-048 | E2E tests: Customer subscribes → upgrades → views usage → renews automatically | P1 | Testing |
| TODO-SALES006-049 | E2E tests: Payment failure → dunning retries → cancellation workflow | P2 | Testing |
| TODO-SALES006-050 | E2E tests: Pause/resume subscription with billing cycle continuation | P2 | Testing |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-08 | System | Initial specification |
