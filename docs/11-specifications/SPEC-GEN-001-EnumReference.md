# SPEC-GEN-001: Enum Reference

> **Module:** Global / Cross-cutting
> **Feature:** Enumeration catalog
> **Status:** ✅ Draft
> **Created:** 2026-02-21
> **Purpose:** Provide a centralized reference of all public enums defined in `CRM.Core.Entities`. Intended for developers and documentation audits.

---

## 1. Overview

The CRM solution relies heavily on enums for domain concepts such as statuses, types, roles, and categories. This document enumerates the current set of enums, their values, and where they are declared. Whenever a new enum is added, this file should be updated (see Field Gap Audit policy).

## 2. Entity Enums

Below is a non‑exhaustive list of enums found under `CRM.Core/Entities` (as of 2026-02-21). Each entry includes the file path and a table of named values with their integer representation and a brief description.

### 2.1 Commission.cs
| Enum | Values (int) | Description |
|------|--------------|-------------|
| `CommissionType` | FlatPercentage (0), TieredPercentage (1), FixedAmount (2), TieredAmount (3), MarginBased (4), Custom (5) | Calculation method |
| `CommissionTrigger` | OnClose (0), OnOrder (1), OnInvoice (2), OnPayment (3), OnSubscriptionStart (4), OnSignature (5), Monthly (6) | Event that triggers calculation |
| `CommissionStatus` | Pending (0), Approved (1), Held (2), Paid (3), ClawedBack (4), Clawback (4)*, Adjusted (5), Cancelled (6), Rejected (7) | Commission payout value |
| `CommissionPlanStatus` | Draft (0), Active (1), Inactive (2), Archived (3) | Status of a commission plan |


global note: `Clawback` is an alias for `ClawedBack`.

### 2.2 User.cs
| Enum | Values (int) | Description |
|------|--------------|-------------|
| `UserRole` | Admin (0), Manager (1), User (2), Guest (3) | Role assigned to a user |

### 2.3 Quote.cs
| Enum | Values (int) | Description |
|------|--------------|-------------|
| `QuoteStatus` | Draft (0), Sent (1), Viewed (2), Accepted (3), Rejected (4), Expired (5), Cancelled (6) | Lifecycle state of a quote |

### 2.4 Order.cs
| Enum | Values (int) | Description |
|------|--------------|-------------|
| `OrderStatus` | Draft (0), PendingApproval (1), Approved (2), Processing (3), PartiallyFulfilled (4), Fulfilled (5), Shipped (6), Delivered (7), Completed (8), Cancelled (9), OnHold (10), Returned (11), Refunded (12) |
| `OrderType` | Standard (0), Renewal (1), Amendment (2), Upgrade (3), Downgrade (4), AddOn (5), Replacement (6), Trial (7), Sample (8), Return (9), Credit (10), MultiYear (11) |

### 2.5 Subscription.cs / BillingHistory.cs / DunningRecord.cs / SubscriptionRenewal.cs
| Enum | File | Values (int) | Description |
|------|------|--------------|-------------|
| `SubscriptionStatus` | `Subscription.cs` | Active (0), Paused (1), Cancelled (2), Suspended (3), PendingCancellation (4), Expired (5), Trial (6); aliased: Current=0, Churned=2 | Subscription lifecycle state |
| `BillingEventType` | `BillingHistory.cs` | Created (0), Activated (1), PlanChanged (2), Invoiced (3), Cancelled (4), Renewed (5), Paused (6), Resumed (7), Suspended (8), PaymentCollected (9), PaymentFailed (10), ProrationApplied (11), UsageChargeApplied (12) | Audit trail event type |
| `DunningStatus` | `DunningRecord.cs` | Active (0), Resolved (1), Exhausted (2), WrittenOff (3), GracePeriod (4) | Payment recovery workflow state |
| `SubscriptionRenewalStatus` | `SubscriptionRenewal.cs` | Pending (0), Completed (1), Failed (2), Skipped (3) | Renewal attempt outcome |
| `BillingCycle` | `Entities/Enums/BillingCycle.cs` | Monthly (1), Quarterly (2), Annual (3), Weekly (4), Daily (5), Biannual (6), Custom (99) | Billing frequency — numeric enum for future migration of Subscription.BillingCycle string field (TODO-SALES006-014) |

> **Note:** `BillingCycle` starts at 1 (not 0) to distinguish unset/default from "Monthly". The `Custom = 99` value is intentionally non-sequential to allow future values 7–98 without breaking existing stored data.

### 2.6 Additional enums
List additional enums similarly as they are discovered (e.g. `AddressType`, `PaymentStatus`, etc.).

### 2.7 ReportDtos.cs — Cohort Analysis (TODO-RPT-07)

| Enum | Values (int) | Description |
|------|--------------|-------------|
| `ReportCohortType` | Monthly (0), Quarterly (1) | Cohort grouping granularity for cohort analysis requests (`CohortAnalysisRequestDto.CohortType`). Renamed from the conflicting `CohortType` to avoid clash with `CRM.Infrastructure.Services.CohortType` (Acquisition/Revenue/Activity/etc.). Defined in `CRM.Core.Dtos.Reports`. |
| `CohortMetricType` | Retention (0), Revenue (1), Activity (2) | Which metric to measure per cohort period. Defined in `CRM.Core.Dtos.Reports`. |
| `SegmentBy` | Industry (0), Region (1), Revenue (2), Lifecycle (3) | Dimension used for customer segmentation (`SegmentationCriteria.SegmentBy`). Defined in `CRM.Core.Dtos.Reports`. |

> **Note:** `SegmentBy.Region` (value 1) uses annual revenue tiers as a geographic proxy since `Account` uses a polymorphic address model (no `BillingState` column). Bands: Small Market (<$100K), Mid Market ($100K–$1M), Enterprise ($1M–$10M), Strategic (>$10M).

> **Share permission levels** for `ReportShare` are stored as plain strings (`"View"`, `"Edit"`, `"Admin"`) in `ReportShares.Permission` — no enum type; validated in service layer.

### 2.8 Workflow Engine & Scripting (SPEC-SD-004, SPEC-AI-006)

| Enum | File | Values (int) | Description |
|------|------|--------------|-------------|
| `WorkflowNodeType` | `CRM.Core/Enums/WorkflowNodeType.cs` | Start (0), End (1), Action (2), Decision (3), Fork (4), Join (5), Task (6), Approval (7), Notification (8), Wait (9), Script (10), Subprocess (11) | Type of node in a workflow definition |
| `WorkflowStatus` | `CRM.Core/Enums/WorkflowStatus.cs` | Draft (0), Active (1), Inactive (2), Archived (3) | Publication state of a workflow definition |
| `WorkflowInstanceStatus` | `CRM.Core/Enums/WorkflowInstanceStatus.cs` | Running (0), Completed (1), Cancelled (2), Failed (3), Paused (4), Waiting (5) | Execution state of a workflow instance |
| `WorkflowTaskStatus` | `CRM.Core/Enums/WorkflowTaskStatus.cs` | Pending (0), InProgress (1), Completed (2), Rejected (3), Cancelled (4), Expired (5) | State of a human task within a workflow |
| `WorkflowTriggerType` | `CRM.Core/Enums/WorkflowTriggerType.cs` | Manual (0), OnCreate (1), OnUpdate (2), OnStatusChange (3), Scheduled (4), OnEvent (5) | Event that starts a workflow |
| `ScriptLanguage` | `CRM.Core/Enums/ScriptLanguage.cs` ✅ Implemented | JavaScript (0), Python (1), CSharp (2) | Scripting language for Workflow Script nodes and Agent ScriptPlugins. `JavaScript` uses Jint engine (always available). `Python` requires CPython 3.11+ and `FeatureManagement:EnablePythonScripting=true`. `CSharp` reserved for future developer tooling. |

> **Note:** `ScriptLanguage` must be added to `CRM.Core/Enums/ScriptLanguage.cs` as part of SPEC-AI-006. Add corresponding unit test in `ScriptLanguageEnumTests`.

## 3. Maintenance

- **New enum added:** update this document and the corresponding feature spec.
- **Enum value changed/removed:** update tests (`EnumTests`), docs, and FIELD_GAP_REMEDIATION_PLAN.md.
- **Alias introduced:** note the alias in both code comment and this catalog.

---

*This document is intended to satisfy the user request to maintain enum documentation/specs. Place under `docs/11-specifications` consistent with existing specs; if an alternative folder (`cocs/spec`) is later specified, create a symlink or move accordingly.*