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

### 2.5 Additional enums
List additional enums similarly as they are discovered (e.g. `AddressType`, `PaymentStatus`, etc.).

## 3. Maintenance

- **New enum added:** update this document and the corresponding feature spec.
- **Enum value changed/removed:** update tests (`EnumTests`), docs, and FIELD_GAP_REMEDIATION_PLAN.md.
- **Alias introduced:** note the alias in both code comment and this catalog.

---

*This document is intended to satisfy the user request to maintain enum documentation/specs. Place under `docs/11-specifications` consistent with existing specs; if an alternative folder (`cocs/spec`) is later specified, create a symlink or move accordingly.*