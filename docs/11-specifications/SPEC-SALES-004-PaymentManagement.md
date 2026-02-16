# SPEC-SALES-004: Payment Management

> **Module:** Sales  
> **Version:** 1.0  
> **Status:** ✅ Complete  
> **Last Updated:** February 2026  
> **Author:** System Analysis  
> **Dependencies:** SPEC-SALES-003 (Invoice Management), SPEC-CRM-001 (Account Management)

---

## Table of Contents

1. [Business Context](#1-business-context)
2. [Frontend Specification](#2-frontend-specification)
3. [Backend Specification](#3-backend-specification)
4. [Database Specification](#4-database-specification)
5. [Test Specification](#5-test-specification)
6. [Issues & Inconsistencies](#6-issues--inconsistencies)
7. [TODO Items](#7-todo-items)

---

## 1. Business Context

### 1.1 Overview

Payment Management handles all monetary transactions in the CRM, including:
- Processing payments against invoices
- Recording different payment methods (credit card, bank transfer, check, etc.)
- Managing refunds and chargebacks
- Payment reconciliation with bank records
- Multi-invoice payment allocation
- Scheduled and recurring payments
- Gateway integration for automated processing

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Priority |
|----|-------------|-------------|----------|
| SF-01 | Payment Processing | Process payments via multiple methods | P0 |
| SF-02 | Payment Recording | Record manual and automated payments | P0 |
| SF-03 | Refund Management | Full and partial refund processing | P0 |
| SF-04 | Payment Allocation | Apply payments to multiple invoices | P1 |
| SF-05 | Payment Reconciliation | Match payments with bank records | P1 |
| SF-06 | Scheduled Payments | Set up future-dated payments | P2 |
| SF-07 | Payment Retry | Retry failed payment attempts | P2 |
| SF-08 | Gateway Integration | Connect to payment processors | P1 |
| SF-09 | Fraud Detection | Flag suspicious transactions | P2 |
| SF-10 | Payment Reporting | Payment analytics and statistics | P1 |

### 1.3 Key Functionalities

| Functionality | Description | Implementation Status |
|---------------|-------------|----------------------|
| Create payment record | Record new payment transaction | ✅ Implemented |
| Process payment | Submit to gateway for processing | ✅ Implemented (simulated) |
| Process refund | Create refund against original payment | ✅ Implemented |
| Void payment | Cancel pending payment | ✅ Implemented |
| Capture authorization | Capture pre-authorized payment | ✅ Implemented |
| Reconcile payment | Match with bank reference | ✅ Implemented |
| Allocate to invoices | Apply payment to multiple invoices | ✅ Implemented |
| Retry failed payment | Reattempt failed transaction | ✅ Implemented |
| Schedule payment | Set future payment date | ✅ Implemented |
| Get payment statistics | Generate payment analytics | ✅ Implemented |

### 1.4 Business Rules

| Rule ID | Rule Description | Enforcement |
|---------|------------------|-------------|
| BR-01 | Payment amount must be positive | Backend validation |
| BR-02 | Refund cannot exceed original payment minus existing refunds | Backend validation |
| BR-03 | Only Pending/Processing payments can be voided | Status check |
| BR-04 | Only Authorization type can be captured | Type check |
| BR-05 | Only Failed payments can be retried | Status check |
| BR-06 | Total allocation cannot exceed payment amount | Backend validation |
| BR-07 | Payment number auto-generated: PAY-YYMM-NNNN | Backend generation |
| BR-08 | Completed payments update invoice AmountPaid | Backend auto-update |

### 1.5 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition |
|-------|----------|-------|--------------|---------------|
| UC-01 | Process credit card payment | Sales Rep | Valid invoice exists | Payment completed, invoice updated |
| UC-02 | Record check payment | Accountant | Check received | Payment recorded manually |
| UC-03 | Process full refund | Manager | Completed payment exists | Refund created, original updated |
| UC-04 | Process partial refund | Manager | Completed payment exists | Partial refund, original partially refunded |
| UC-05 | Allocate overpayment | Accountant | Overpayment exists | Applied to multiple invoices |
| UC-06 | Reconcile bank deposit | Accountant | Payment completed | Payment marked reconciled |
| UC-07 | Void pending payment | Sales Rep | Pending payment | Payment voided |
| UC-08 | Retry failed payment | System | Failed payment, retry < max | Payment retried |

---

## 2. Frontend Specification

### 2.1 Pages

| Page | Route | Component | Status |
|------|-------|-----------|--------|
| Payments List | `/payments` | `PaymentsPage.tsx` | ❌ NOT IMPLEMENTED |
| Payment Details | `/payments/:id` | `PaymentDetailsPage.tsx` | ❌ NOT IMPLEMENTED |
| Process Payment | `/payments/new` | `ProcessPaymentPage.tsx` | ❌ NOT IMPLEMENTED |
| Payment Reconciliation | `/payments/reconciliation` | `ReconciliationPage.tsx` | ❌ NOT IMPLEMENTED |

### 2.2 Components

| Component | Purpose | Props | Status |
|-----------|---------|-------|--------|
| `PaymentForm.tsx` | Capture payment details | `invoiceId`, `onSubmit`, `onCancel` | ❌ NOT IMPLEMENTED |
| `PaymentCard.tsx` | Display payment summary | `payment` | ❌ NOT IMPLEMENTED |
| `PaymentHistory.tsx` | List payment transactions | `customerId`, `invoiceId` | ❌ NOT IMPLEMENTED |
| `PaymentMethodSelector.tsx` | Select payment method | `value`, `onChange` | ❌ NOT IMPLEMENTED |
| `RefundDialog.tsx` | Process refund | `paymentId`, `maxAmount`, `onConfirm` | ❌ NOT IMPLEMENTED |
| `AllocationGrid.tsx` | Allocate to invoices | `paymentId`, `amount`, `invoices` | ❌ NOT IMPLEMENTED |
| `ReconciliationPanel.tsx` | Reconcile payments | `payments`, `onReconcile` | ❌ NOT IMPLEMENTED |
| `PaymentStatistics.tsx` | Display payment stats | `dateRange` | ❌ NOT IMPLEMENTED |
| `PaymentStatusBadge.tsx` | Show payment status | `status` | ❌ NOT IMPLEMENTED |
| `CardDetailsForm.tsx` | Capture card information | `onSubmit` | ❌ NOT IMPLEMENTED |

### 2.3 Frontend Service Interface

**File:** `src/services/paymentService.ts` (NOT IMPLEMENTED)

```typescript
// Expected interface based on IPaymentService
export interface Payment {
  id: number;
  paymentNumber: string;
  externalPaymentId?: string;
  gatewayTransactionId?: string;
  authorizationCode?: string;
  status: PaymentStatus;
  paymentMethod: PaymentMethod;
  paymentType: PaymentType;
  amount: number;
  amountApplied: number;
  amountUnapplied: number;
  processingFee: number;
  netAmount: number;
  refundedAmount: number;
  currencyCode: string;
  paymentDate: string;
  processedDate?: string;
  settledDate?: string;
  accountId: number;
  invoiceId?: number;
  cardLast4?: string;
  cardBrand?: string;
  isReconciled: boolean;
  notes?: string;
  failureReason?: string;
}

export enum PaymentStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Failed = 3,
  Declined = 4,
  Cancelled = 5,
  Refunded = 6,
  PartiallyRefunded = 7,
  Disputed = 8,
  Voided = 9,
  OnHold = 10,
  Expired = 11
}

export enum PaymentMethod {
  CreditCard = 0,
  DebitCard = 1,
  BankTransfer = 2,
  WireTransfer = 3,
  Check = 4,
  Cash = 5,
  PayPal = 6,
  Stripe = 7,
  ApplePay = 8,
  GooglePay = 9,
  Venmo = 10,
  Crypto = 11,
  StoreCredit = 12,
  GiftCard = 13,
  Financing = 14,
  PurchaseOrder = 15,
  Other = 16
}

export enum PaymentType {
  Payment = 0,
  Refund = 1,
  Authorization = 2,
  Capture = 3,
  Void = 4,
  Deposit = 5,
  WriteOff = 6,
  CreditApplication = 7,
  Chargeback = 8,
  ChargebackReversal = 9
}

export interface PaymentDetails {
  cardNumber?: string;
  expiryMonth?: string;
  expiryYear?: string;
  cvv?: string;
  cardholderName?: string;
  bankAccountNumber?: string;
  routingNumber?: string;
  paymentToken?: string;
  billingAddress?: string;
  billingCity?: string;
  billingState?: string;
  billingZip?: string;
  billingCountry?: string;
}

export interface PaymentAllocation {
  invoiceId: number;
  amount: number;
}

export interface PaymentResult {
  success: boolean;
  transactionId?: string;
  authorizationCode?: string;
  errorCode?: string;
  errorMessage?: string;
  payment?: Payment;
  metadata: Record<string, string>;
}

export interface PaymentStatistics {
  totalPayments: number;
  successfulPayments: number;
  failedPayments: number;
  pendingPayments: number;
  totalAmount: number;
  successfulAmount: number;
  refundedAmount: number;
  successRate: number;
  averagePaymentAmount: number;
  paymentsByMethod: Record<PaymentMethod, number>;
}

const paymentService = {
  // CRUD Operations
  getAll: (customerId?: number, invoiceId?: number, status?: PaymentStatus) => 
    api.get<Payment[]>('/payments', { params: { customerId, invoiceId, status } }),
  getById: (id: number) => api.get<Payment>(`/payments/${id}`),
  getByTransactionId: (transactionId: string) => api.get<Payment>(`/payments/transaction/${transactionId}`),
  create: (payment: Partial<Payment>) => api.post<Payment>('/payments', payment),
  update: (id: number, payment: Partial<Payment>) => api.put<Payment>(`/payments/${id}`, payment),
  delete: (id: number) => api.delete(`/payments/${id}`),
  
  // Payment Processing
  processPayment: (invoiceId: number, amount: number, method: PaymentMethod, details: PaymentDetails) =>
    api.post<PaymentResult>('/payments/process', { invoiceId, amount, method, details }),
  processRefund: (paymentId: number, amount: number, reason: string) =>
    api.post<PaymentResult>(`/payments/${paymentId}/refund`, { amount, reason }),
  voidPayment: (paymentId: number, reason: string) =>
    api.post<PaymentResult>(`/payments/${paymentId}/void`, { reason }),
  capturePayment: (paymentId: number, amount?: number) =>
    api.post<PaymentResult>(`/payments/${paymentId}/capture`, { amount }),
  
  // Status Management
  updateStatus: (id: number, status: PaymentStatus) => api.patch<Payment>(`/payments/${id}/status`, { status }),
  markAsCompleted: (id: number) => api.post<Payment>(`/payments/${id}/complete`),
  markAsFailed: (id: number, failureReason: string) => api.post<Payment>(`/payments/${id}/fail`, { failureReason }),
  
  // Queries
  getByDateRange: (fromDate: string, toDate: string) =>
    api.get<Payment[]>('/payments/range', { params: { fromDate, toDate } }),
  getPending: () => api.get<Payment[]>('/payments/pending'),
  getFailed: (maxRetries?: number) => api.get<Payment[]>('/payments/failed', { params: { maxRetries } }),
  getStatistics: (fromDate?: string, toDate?: string) =>
    api.get<PaymentStatistics>('/payments/statistics', { params: { fromDate, toDate } }),
  getCustomerHistory: (customerId: number) => api.get<Payment[]>(`/payments/customer/${customerId}/history`),
  
  // Reconciliation
  reconcile: (id: number, bankReference: string) =>
    api.post<boolean>(`/payments/${id}/reconcile`, { bankReference }),
  getUnreconciled: () => api.get<Payment[]>('/payments/unreconciled'),
  applyToInvoices: (id: number, allocations: PaymentAllocation[]) =>
    api.post<PaymentAllocation[]>(`/payments/${id}/apply`, { allocations }),
  
  // Retry & Scheduling
  retry: (id: number) => api.post<PaymentResult>(`/payments/${id}/retry`),
  schedule: (payment: Partial<Payment>, scheduledDate: string) =>
    api.post<Payment>('/payments/schedule', { payment, scheduledDate }),
};

export default paymentService;
```

### 2.4 Form Validations

| Field | Validation Rules | Error Message |
|-------|------------------|---------------|
| amount | Required, > 0 | "Payment amount is required and must be positive" |
| paymentMethod | Required | "Payment method is required" |
| cardNumber | Required for card, valid format | "Valid card number is required" |
| expiryMonth | Required for card, 01-12 | "Valid expiry month is required" |
| expiryYear | Required for card, >= current year | "Valid expiry year is required" |
| cvv | Required for card, 3-4 digits | "Valid CVV is required" |
| cardholderName | Required for card | "Cardholder name is required" |
| refundAmount | Required, > 0, <= availableRefundAmount | "Refund amount must be within available amount" |
| bankReference | Required for reconciliation | "Bank reference is required" |

---

## 3. Backend Specification

### 3.1 Entity: Payment

**File:** `CRM.Backend/src/CRM.Core/Entities/Payment.cs`  
**Lines:** 421  
**Status:** ✅ IMPLEMENTED

#### Entity Regions

| Region | Properties | Lines |
|--------|------------|-------|
| Identification | PaymentNumber, ExternalPaymentId, GatewayTransactionId, GatewayReference, AuthorizationCode, CheckNumber | ~165-192 |
| Payment Details | Description, Status, PaymentMethod, PaymentType | ~194-204 |
| Amounts | Amount, AmountApplied, AmountUnapplied, ProcessingFee, NetAmount, RefundedAmount, CurrencyCode, ExchangeRate | ~206-238 |
| Dates | PaymentDate, ProcessedDate, SettledDate, RefundDate, DepositDate | ~240-260 |
| Card Details | CardBrand, CardLast4, CardExpMonth, CardExpYear, CardholderName | ~262-282 |
| Bank Details | BankName, AccountLast4, AccountType, RoutingNumberLast4 | ~284-300 |
| Gateway Response | Gateway, GatewayResponseCode, GatewayResponseMessage, AvsResponseCode, CvvResponseCode, RiskScore, GatewayResponseRaw | ~302-330 |
| Fraud & Risk | FraudFlagged, FraudNotes, IpAddress, DeviceFingerprint | ~332-348 |
| Relationships | AccountId, Account, InvoiceId, Invoice, OrderId, Order, SubscriptionId, Subscription, OriginalPaymentId, OriginalPayment, Refunds, ProcessedById, ProcessedBy | ~350-395 |
| Scheduling & Retry | ScheduledDate, RetryCount | ~397-404 |
| Reconciliation | BankReference, IsReconciled, ReconciledDate | ~406-416 |
| Notes | Notes, InternalNotes, FailureReason, RefundReason | ~418-430 |
| Aliases | TransactionId (alias for GatewayTransactionId) | ~432-437 |

#### Payment Properties Summary (~80 properties)

**Identification (6 properties):**
- `PaymentNumber` (string) - System-generated: PAY-YYMM-NNNN
- `ExternalPaymentId` (string?) - External reference
- `GatewayTransactionId` (string?) - Gateway transaction ID
- `GatewayReference` (string?) - Gateway reference number
- `AuthorizationCode` (string?) - Authorization code
- `CheckNumber` (string?) - Check number for check payments

**Payment Details (4 properties):**
- `Description` (string?) - Payment description/memo
- `Status` (PaymentStatus) - Current status (enum, default: Pending)
- `PaymentMethod` (PaymentMethod) - Method used (enum, default: CreditCard)
- `PaymentType` (PaymentType) - Transaction type (enum, default: Payment)

**Amounts (8 properties):**
- `Amount` (decimal) - Payment amount
- `AmountApplied` (decimal) - Amount applied to invoices
- `AmountUnapplied` (computed) - Unapplied/overpayment amount
- `ProcessingFee` (decimal) - Processing fee
- `NetAmount` (computed) - Amount minus fees
- `RefundedAmount` (decimal) - Amount refunded
- `CurrencyCode` (string) - ISO 4217 code, default "USD"
- `ExchangeRate` (decimal?) - Exchange rate if foreign currency

**Dates (5 properties):**
- `PaymentDate` (DateTime) - Date payment was made
- `ProcessedDate` (DateTime?) - Date processed
- `SettledDate` (DateTime?) - Date settled
- `RefundDate` (DateTime?) - Date refunded
- `DepositDate` (DateTime?) - Bank deposit date

**Card Details (5 properties):**
- `CardBrand` (string?) - Visa, Mastercard, etc.
- `CardLast4` (string?) - Last 4 digits
- `CardExpMonth` (int?) - Expiration month
- `CardExpYear` (int?) - Expiration year
- `CardholderName` (string?) - Cardholder name

**Bank Details (4 properties):**
- `BankName` (string?) - Bank name
- `AccountLast4` (string?) - Last 4 digits of account
- `AccountType` (string?) - Checking, savings
- `RoutingNumberLast4` (string?) - Last 4 of routing

**Gateway Response (7 properties):**
- `Gateway` (string?) - Gateway used
- `GatewayResponseCode` (string?) - Response code
- `GatewayResponseMessage` (string?) - Response message
- `AvsResponseCode` (string?) - AVS response
- `CvvResponseCode` (string?) - CVV response
- `RiskScore` (decimal?) - Risk score
- `GatewayResponseRaw` (string?) - Raw response JSON

**Fraud & Risk (4 properties):**
- `FraudFlagged` (bool) - Flagged for fraud review
- `FraudNotes` (string?) - Fraud notes
- `IpAddress` (string?) - Payment IP address
- `DeviceFingerprint` (string?) - Device fingerprint

**Relationships (13 properties):**
- `AccountId` (int) - Customer account ID (FK)
- `Account` (Account?) - Navigation
- `InvoiceId` (int?) - Related invoice ID (FK)
- `Invoice` (Invoice?) - Navigation
- `OrderId` (int?) - Related order ID (FK)
- `Order` (Order?) - Navigation
- `SubscriptionId` (int?) - Related subscription ID (FK)
- `Subscription` (Subscription?) - Navigation
- `OriginalPaymentId` (int?) - Original payment for refunds (FK)
- `OriginalPayment` (Payment?) - Navigation
- `Refunds` (ICollection<Payment>) - Refund payments
- `ProcessedById` (int?) - User who processed (FK)
- `ProcessedBy` (User?) - Navigation

**Scheduling & Retry (2 properties):**
- `ScheduledDate` (DateTime?) - Scheduled payment date
- `RetryCount` (int) - Number of retry attempts

**Reconciliation (3 properties):**
- `BankReference` (string?) - Bank reference for reconciliation
- `IsReconciled` (bool) - Whether reconciled
- `ReconciledDate` (DateTime?) - Date reconciled

**Notes (4 properties):**
- `Notes` (string?) - Payment notes
- `InternalNotes` (string?) - Internal notes
- `FailureReason` (string?) - Failure reason
- `RefundReason` (string?) - Refund reason

### 3.2 Enumerations

#### PaymentStatus (12 values)
```csharp
public enum PaymentStatus
{
    Pending = 0,           // Initiated, pending processing
    Processing = 1,        // Being processed by gateway
    Completed = 2,         // Successfully completed
    Failed = 3,            // Failed
    Declined = 4,          // Declined by gateway/bank
    Cancelled = 5,         // Cancelled
    Refunded = 6,          // Fully refunded
    PartiallyRefunded = 7, // Partial refund issued
    Disputed = 8,          // Disputed/chargeback
    Voided = 9,            // Voided before settlement
    OnHold = 10,           // Held for review
    Expired = 11           // Expired
}
```

#### PaymentMethod (17 values)
```csharp
public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    BankTransfer = 2,
    WireTransfer = 3,
    Check = 4,
    Cash = 5,
    PayPal = 6,
    Stripe = 7,
    ApplePay = 8,
    GooglePay = 9,
    Venmo = 10,
    Crypto = 11,
    StoreCredit = 12,
    GiftCard = 13,
    Financing = 14,
    PurchaseOrder = 15,
    Other = 16
}
```

#### PaymentType (10 values)
```csharp
public enum PaymentType
{
    Payment = 0,           // Standard payment
    Refund = 1,            // Refund to customer
    Authorization = 2,     // Pre-authorization hold
    Capture = 3,           // Capture of authorization
    Void = 4,              // Void of previous transaction
    Deposit = 5,           // Deposit/advance payment
    WriteOff = 6,          // Write-off adjustment
    CreditApplication = 7, // Credit application
    Chargeback = 8,        // Chargeback
    ChargebackReversal = 9 // Chargeback reversal
}
```

### 3.3 Interface: IPaymentService

**File:** `CRM.Backend/src/CRM.Core/Interfaces/IPaymentService.cs`  
**Lines:** ~170  
**Status:** ✅ IMPLEMENTED

#### Interface Methods (21 methods in 6 regions)

| Region | Methods |
|--------|---------|
| CRUD Operations | GetAllAsync, GetByIdAsync, GetByTransactionIdAsync, CreateAsync, UpdateAsync, DeleteAsync |
| Payment Processing | ProcessPaymentAsync, ProcessRefundAsync, VoidPaymentAsync, CapturePaymentAsync |
| Status Management | UpdateStatusAsync, MarkAsCompletedAsync, MarkAsFailedAsync |
| Queries | GetPaymentsByDateRangeAsync, GetPendingPaymentsAsync, GetFailedPaymentsAsync, GetStatisticsAsync, GetCustomerPaymentHistoryAsync |
| Reconciliation | ReconcilePaymentAsync, GetUnreconciledPaymentsAsync, ApplyPaymentToInvoicesAsync |
| Retry & Recovery | RetryPaymentAsync, SchedulePaymentAsync |

#### Method Signatures

```csharp
#region CRUD Operations
Task<IEnumerable<Payment>> GetAllAsync(int? customerId = null, int? invoiceId = null, PaymentStatus? status = null, CancellationToken cancellationToken = default);
Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
#endregion

#region Payment Processing
Task<PaymentResult> ProcessPaymentAsync(int invoiceId, decimal amount, PaymentMethod method, PaymentDetails details, CancellationToken cancellationToken = default);
Task<PaymentResult> ProcessRefundAsync(int paymentId, decimal amount, string reason, CancellationToken cancellationToken = default);
Task<PaymentResult> VoidPaymentAsync(int paymentId, string reason, CancellationToken cancellationToken = default);
Task<PaymentResult> CapturePaymentAsync(int paymentId, decimal? amount = null, CancellationToken cancellationToken = default);
#endregion

#region Status Management
Task<Payment> UpdateStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default);
Task<Payment> MarkAsCompletedAsync(int paymentId, CancellationToken cancellationToken = default);
Task<Payment> MarkAsFailedAsync(int paymentId, string failureReason, CancellationToken cancellationToken = default);
#endregion

#region Queries
Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
Task<IEnumerable<Payment>> GetFailedPaymentsAsync(int maxRetries = 3, CancellationToken cancellationToken = default);
Task<PaymentStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
Task<IEnumerable<Payment>> GetCustomerPaymentHistoryAsync(int customerId, CancellationToken cancellationToken = default);
#endregion

#region Reconciliation
Task<bool> ReconcilePaymentAsync(int paymentId, string bankReference, CancellationToken cancellationToken = default);
Task<IEnumerable<Payment>> GetUnreconciledPaymentsAsync(CancellationToken cancellationToken = default);
Task<IEnumerable<PaymentAllocation>> ApplyPaymentToInvoicesAsync(int paymentId, IEnumerable<PaymentAllocation> allocations, CancellationToken cancellationToken = default);
#endregion

#region Retry & Recovery
Task<PaymentResult> RetryPaymentAsync(int paymentId, CancellationToken cancellationToken = default);
Task<Payment> SchedulePaymentAsync(Payment payment, DateTime scheduledDate, CancellationToken cancellationToken = default);
#endregion
```

### 3.4 Supporting DTOs (in IPaymentService.cs)

```csharp
public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Payment? Payment { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PaymentDetails
{
    public string? CardNumber { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public string? CardholderName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? PaymentToken { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string? BillingCountry { get; set; }
}

public class PaymentAllocation
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public double SuccessRate { get; set; }
    public double AveragePaymentAmount { get; set; }
    public Dictionary<PaymentMethod, int> PaymentsByMethod { get; set; } = new();
}
```

### 3.5 Service Implementation: PaymentService

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/PaymentService.cs`  
**Lines:** 728  
**Status:** ✅ IMPLEMENTED

#### Implementation Summary

| Region | Methods | Lines |
|--------|---------|-------|
| CRUD Operations | 6 | 28-119 |
| Payment Processing | 4 | 123-373 |
| Status Management | 3 | 400-455 |
| Queries | 5 | 460-540 |
| Reconciliation | 3 | 546-625 |
| Retry & Recovery | 2 | 630-695 |
| Private Helpers | 2 | 700-728 |

#### Key Implementation Details

**Payment Number Generation:**
```csharp
private async Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken)
{
    var prefix = "PAY";
    var year = DateTime.UtcNow.ToString("yy");
    var month = DateTime.UtcNow.ToString("MM");
    // Format: PAY-YYMM-NNNN (sequential within month)
}
```

**Gateway Simulation:**
- ProcessPaymentAsync simulates gateway processing
- In production, would integrate with actual payment gateways
- Currently generates mock TransactionId and AuthorizationCode

**Invoice Auto-Update:**
- ProcessPaymentAsync updates Invoice.AmountPaid
- Updates Invoice.Status to Paid/PartiallyPaid
- Sets Invoice.PaidDate when fully paid

**Refund Handling:**
- Creates new Payment with PaymentType.Refund
- Links to OriginalPaymentId
- Updates original payment RefundedAmount
- Updates original payment status to Refunded/PartiallyRefunded

### 3.6 Controller: PaymentsController

**File:** `CRM.Backend/src/CRM.Api/Controllers/PaymentsController.cs`  
**Status:** ❌ NOT IMPLEMENTED

#### Expected Endpoints

| Method | Route | Description | Request Body | Response |
|--------|-------|-------------|--------------|----------|
| GET | `/api/payments` | List payments | Query params | `Payment[]` |
| GET | `/api/payments/{id}` | Get by ID | - | `Payment` |
| GET | `/api/payments/transaction/{transactionId}` | Get by transaction ID | - | `Payment` |
| POST | `/api/payments` | Create payment | `CreatePaymentDto` | `Payment` |
| PUT | `/api/payments/{id}` | Update payment | `UpdatePaymentDto` | `Payment` |
| DELETE | `/api/payments/{id}` | Delete payment | - | `204 No Content` |
| POST | `/api/payments/process` | Process payment | `ProcessPaymentDto` | `PaymentResult` |
| POST | `/api/payments/{id}/refund` | Process refund | `RefundDto` | `PaymentResult` |
| POST | `/api/payments/{id}/void` | Void payment | `VoidDto` | `PaymentResult` |
| POST | `/api/payments/{id}/capture` | Capture authorization | `CaptureDto` | `PaymentResult` |
| PATCH | `/api/payments/{id}/status` | Update status | `StatusUpdateDto` | `Payment` |
| POST | `/api/payments/{id}/complete` | Mark completed | - | `Payment` |
| POST | `/api/payments/{id}/fail` | Mark failed | `FailureDto` | `Payment` |
| GET | `/api/payments/range` | Get by date range | Query params | `Payment[]` |
| GET | `/api/payments/pending` | Get pending payments | - | `Payment[]` |
| GET | `/api/payments/failed` | Get failed payments | Query params | `Payment[]` |
| GET | `/api/payments/statistics` | Get statistics | Query params | `PaymentStatistics` |
| GET | `/api/payments/customer/{customerId}/history` | Customer history | - | `Payment[]` |
| POST | `/api/payments/{id}/reconcile` | Reconcile payment | `ReconcileDto` | `bool` |
| GET | `/api/payments/unreconciled` | Get unreconciled | - | `Payment[]` |
| POST | `/api/payments/{id}/apply` | Apply to invoices | `AllocationDto[]` | `PaymentAllocation[]` |
| POST | `/api/payments/{id}/retry` | Retry payment | - | `PaymentResult` |
| POST | `/api/payments/schedule` | Schedule payment | `ScheduleDto` | `Payment` |

### 3.7 Backend Validations

| Validation | Location | Error Response |
|------------|----------|----------------|
| Amount > 0 | ProcessPaymentAsync | 400 Bad Request |
| Invoice exists | ProcessPaymentAsync | PaymentResult with error |
| Payment exists | RefundAsync | PaymentResult with error |
| Status is Completed for refund | ProcessRefundAsync | PaymentResult with error |
| Refund <= available amount | ProcessRefundAsync | PaymentResult with error |
| Status is Pending/Processing for void | VoidPaymentAsync | PaymentResult with error |
| Type is Authorization for capture | CapturePaymentAsync | PaymentResult with error |
| Status is Failed for retry | RetryPaymentAsync | PaymentResult with error |
| Allocation total <= payment amount | ApplyToInvoicesAsync | InvalidOperationException |

---

## 4. Database Specification

### 4.1 Table: Payments

```sql
CREATE TABLE Payments (
    -- Primary Key & BaseEntity
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion BINARY(8) NULL,
    
    -- Identification
    PaymentNumber VARCHAR(50) NOT NULL,
    ExternalPaymentId VARCHAR(100) NULL,
    GatewayTransactionId VARCHAR(100) NULL,
    GatewayReference VARCHAR(100) NULL,
    AuthorizationCode VARCHAR(50) NULL,
    CheckNumber VARCHAR(50) NULL,
    
    -- Payment Details
    Description TEXT NULL,
    Status INT NOT NULL DEFAULT 0,
    PaymentMethod INT NOT NULL DEFAULT 0,
    PaymentType INT NOT NULL DEFAULT 0,
    
    -- Amounts
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    AmountApplied DECIMAL(18,2) NOT NULL DEFAULT 0,
    ProcessingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    RefundedAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CurrencyCode VARCHAR(3) NOT NULL DEFAULT 'USD',
    ExchangeRate DECIMAL(18,6) NULL,
    
    -- Dates
    PaymentDate DATETIME(6) NOT NULL,
    ProcessedDate DATETIME(6) NULL,
    SettledDate DATETIME(6) NULL,
    RefundDate DATETIME(6) NULL,
    DepositDate DATETIME(6) NULL,
    
    -- Card Details (Masked)
    CardBrand VARCHAR(50) NULL,
    CardLast4 VARCHAR(4) NULL,
    CardExpMonth INT NULL,
    CardExpYear INT NULL,
    CardholderName VARCHAR(255) NULL,
    
    -- Bank Details (Masked)
    BankName VARCHAR(255) NULL,
    AccountLast4 VARCHAR(4) NULL,
    AccountType VARCHAR(50) NULL,
    RoutingNumberLast4 VARCHAR(4) NULL,
    
    -- Gateway Response
    Gateway VARCHAR(100) NULL,
    GatewayResponseCode VARCHAR(50) NULL,
    GatewayResponseMessage VARCHAR(500) NULL,
    AvsResponseCode VARCHAR(10) NULL,
    CvvResponseCode VARCHAR(10) NULL,
    RiskScore DECIMAL(5,2) NULL,
    GatewayResponseRaw TEXT NULL,
    
    -- Fraud & Risk
    FraudFlagged TINYINT(1) NOT NULL DEFAULT 0,
    FraudNotes TEXT NULL,
    IpAddress VARCHAR(45) NULL,
    DeviceFingerprint VARCHAR(255) NULL,
    
    -- Relationships (Foreign Keys)
    AccountId INT NOT NULL,
    InvoiceId INT NULL,
    OrderId INT NULL,
    SubscriptionId INT NULL,
    OriginalPaymentId INT NULL,
    ProcessedById INT NULL,
    
    -- Scheduling & Retry
    ScheduledDate DATETIME(6) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    
    -- Reconciliation
    BankReference VARCHAR(100) NULL,
    IsReconciled TINYINT(1) NOT NULL DEFAULT 0,
    ReconciledDate DATETIME(6) NULL,
    
    -- Notes
    Notes TEXT NULL,
    InternalNotes TEXT NULL,
    FailureReason TEXT NULL,
    RefundReason TEXT NULL,
    
    -- Foreign Key Constraints
    CONSTRAINT FK_Payments_Accounts FOREIGN KEY (AccountId) REFERENCES Customers(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Payments_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Payments_OriginalPayment FOREIGN KEY (OriginalPaymentId) REFERENCES Payments(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Payments_ProcessedBy FOREIGN KEY (ProcessedById) REFERENCES Users(Id) ON DELETE SET NULL
);
```

### 4.2 Indexes

```sql
-- Primary identifiers
CREATE UNIQUE INDEX IX_Payments_PaymentNumber ON Payments(PaymentNumber);
CREATE INDEX IX_Payments_GatewayTransactionId ON Payments(GatewayTransactionId);
CREATE INDEX IX_Payments_ExternalPaymentId ON Payments(ExternalPaymentId);

-- Status and filtering
CREATE INDEX IX_Payments_Status ON Payments(Status);
CREATE INDEX IX_Payments_PaymentMethod ON Payments(PaymentMethod);
CREATE INDEX IX_Payments_PaymentType ON Payments(PaymentType);

-- Date-based queries
CREATE INDEX IX_Payments_PaymentDate ON Payments(PaymentDate);
CREATE INDEX IX_Payments_ProcessedDate ON Payments(ProcessedDate);
CREATE INDEX IX_Payments_ScheduledDate ON Payments(ScheduledDate);

-- Relationships
CREATE INDEX IX_Payments_AccountId ON Payments(AccountId);
CREATE INDEX IX_Payments_InvoiceId ON Payments(InvoiceId);
CREATE INDEX IX_Payments_OrderId ON Payments(OrderId);
CREATE INDEX IX_Payments_SubscriptionId ON Payments(SubscriptionId);
CREATE INDEX IX_Payments_OriginalPaymentId ON Payments(OriginalPaymentId);

-- Reconciliation
CREATE INDEX IX_Payments_IsReconciled ON Payments(IsReconciled);
CREATE INDEX IX_Payments_BankReference ON Payments(BankReference);

-- Soft delete + common queries
CREATE INDEX IX_Payments_IsDeleted_Status ON Payments(IsDeleted, Status);
CREATE INDEX IX_Payments_IsDeleted_AccountId ON Payments(IsDeleted, AccountId);
```

### 4.3 Foreign Key Relationships

```
Payments.AccountId → Customers.Id (ON DELETE RESTRICT)
Payments.InvoiceId → Invoices.Id (ON DELETE SET NULL)
Payments.OrderId → Orders.Id (ON DELETE SET NULL)
Payments.SubscriptionId → Subscriptions.Id (ON DELETE SET NULL)
Payments.OriginalPaymentId → Payments.Id (ON DELETE SET NULL) [Self-reference for refunds]
Payments.ProcessedById → Users.Id (ON DELETE SET NULL)
```

---

## 5. Test Specification

### 5.1 Backend Unit Tests

**File:** `tests/CRM.Tests/Services/PaymentServiceTests.cs`  
**Status:** ⏳ TO BE CREATED

| Test Case | Method | Expected Result |
|-----------|--------|-----------------|
| GetAllAsync_ReturnsPayments | GetAllAsync | Returns list of payments |
| GetAllAsync_FiltersByCustomerId | GetAllAsync | Returns only customer's payments |
| GetAllAsync_FiltersByInvoiceId | GetAllAsync | Returns only invoice's payments |
| GetAllAsync_FiltersByStatus | GetAllAsync | Returns only matching status |
| GetByIdAsync_ReturnsPayment | GetByIdAsync | Returns payment when exists |
| GetByIdAsync_ReturnsNull | GetByIdAsync | Returns null when not found |
| GetByTransactionIdAsync_ReturnsPayment | GetByTransactionIdAsync | Returns payment by transaction ID |
| CreateAsync_CreatesPayment | CreateAsync | Creates with generated PaymentNumber |
| CreateAsync_SetsTimestamps | CreateAsync | Sets CreatedAt and UpdatedAt |
| UpdateAsync_UpdatesPayment | UpdateAsync | Updates and sets UpdatedAt |
| DeleteAsync_SoftDeletes | DeleteAsync | Sets IsDeleted = true |
| ProcessPaymentAsync_ProcessesSuccessfully | ProcessPaymentAsync | Returns success, updates invoice |
| ProcessPaymentAsync_ReturnsError_InvoiceNotFound | ProcessPaymentAsync | Returns error for missing invoice |
| ProcessRefundAsync_ProcessesFullRefund | ProcessRefundAsync | Creates refund, updates original |
| ProcessRefundAsync_ProcessesPartialRefund | ProcessRefundAsync | Creates partial refund correctly |
| ProcessRefundAsync_RejectsExcessiveAmount | ProcessRefundAsync | Returns error when amount too high |
| VoidPaymentAsync_VoidsPendingPayment | VoidPaymentAsync | Voids pending payment |
| VoidPaymentAsync_RejectsCompletedPayment | VoidPaymentAsync | Returns error for completed |
| CapturePaymentAsync_CapturesAuthorization | CapturePaymentAsync | Captures auth correctly |
| ReconcilePaymentAsync_ReconcilesPayment | ReconcilePaymentAsync | Sets reconciliation fields |
| ApplyToInvoicesAsync_AppliesCorrectly | ApplyToInvoicesAsync | Updates invoices correctly |
| ApplyToInvoicesAsync_RejectsExcessiveAllocation | ApplyToInvoicesAsync | Throws when over payment amount |
| RetryPaymentAsync_RetriesFailedPayment | RetryPaymentAsync | Retries and increments count |
| SchedulePaymentAsync_SchedulesPayment | SchedulePaymentAsync | Sets ScheduledDate correctly |
| GetStatisticsAsync_CalculatesCorrectly | GetStatisticsAsync | Returns accurate statistics |

### 5.2 Controller Integration Tests

**File:** `tests/CRM.Tests/Integration/PaymentsControllerTests.cs`  
**Status:** ⏳ TO BE CREATED (after controller implemented)

| Test Case | Endpoint | Expected Result |
|-----------|----------|-----------------|
| GetPayments_ReturnsOk | GET /payments | 200 OK with list |
| GetPayment_ReturnsOk | GET /payments/{id} | 200 OK with payment |
| GetPayment_ReturnsNotFound | GET /payments/{invalid} | 404 Not Found |
| CreatePayment_ReturnsCreated | POST /payments | 201 Created |
| ProcessPayment_ReturnsOk | POST /payments/process | 200 OK with result |
| ProcessRefund_ReturnsOk | POST /payments/{id}/refund | 200 OK with result |
| VoidPayment_ReturnsOk | POST /payments/{id}/void | 200 OK with result |
| ReconcilePayment_ReturnsOk | POST /payments/{id}/reconcile | 200 OK |
| GetStatistics_ReturnsOk | GET /payments/statistics | 200 OK with stats |

### 5.3 E2E Tests

**File:** `e2e-tests/tests/payments/payments.spec.ts`  
**Status:** ⏳ TO BE CREATED (after frontend implemented)

| Test Case | Steps | Expected Result |
|-----------|-------|-----------------|
| View payment list | Navigate to /payments | See payments grid |
| View payment details | Click payment row | See payment details |
| Process credit card payment | Select invoice, enter card, submit | Payment processed |
| Process refund | Click refund, enter amount, confirm | Refund created |
| Void pending payment | Click void, confirm | Payment voided |
| Reconcile payment | Enter bank reference, save | Payment reconciled |
| View customer payment history | Navigate to customer, click payments | See payment history |

---

## 6. Issues & Inconsistencies

### 6.1 Missing Components

| Component | Type | Priority | Notes |
|-----------|------|----------|-------|
| PaymentsController.cs | Controller | P1 | Required for API access |
| PaymentDto.cs | DTO | P1 | Required for API layer |
| CreatePaymentDto.cs | DTO | P1 | Required for API layer |
| ProcessPaymentDto.cs | DTO | P1 | Required for API layer |
| paymentService.ts | Frontend Service | P2 | Required for frontend |
| PaymentsPage.tsx | Frontend Page | P2 | Required for frontend |
| PaymentForm.tsx | Frontend Component | P2 | Required for frontend |
| PaymentHistory.tsx | Frontend Component | P2 | Required for frontend |

### 6.2 Implementation Gaps

| Gap | Current State | Expected State | Priority |
|-----|---------------|----------------|----------|
| Gateway integration | Simulated | Real gateway (Stripe, etc.) | P2 |
| PCI compliance | Card data in PaymentDetails | Tokenization only | P1 |
| Webhook handling | Not implemented | Gateway webhook endpoints | P2 |
| Recurring payments | Not implemented | Subscription billing support | P2 |
| Payment disputes | Not implemented | Chargeback workflow | P3 |

### 6.3 Data Consistency Notes

- `TransactionId` property is an alias for `GatewayTransactionId` (both point to same value)
- `AmountUnapplied` and `NetAmount` are computed properties (not stored)
- Refund payments have negative Amount values

---

## 7. TODO Items

| TODO ID | Description | Priority | Category | Spec Section |
|---------|-------------|----------|----------|--------------|
| TODO-SALES004-001 | Create PaymentsController.cs | P1 | Backend | 3.6 |
| TODO-SALES004-002 | Create PaymentDto.cs | P1 | Backend | 3.4 |
| TODO-SALES004-003 | Create CreatePaymentDto.cs | P1 | Backend | 3.4 |
| TODO-SALES004-004 | Create ProcessPaymentDto.cs | P1 | Backend | 3.4 |
| TODO-SALES004-005 | Implement PCI-compliant tokenization | P1 | Security | 6.2 |
| TODO-SALES004-006 | Create paymentService.ts | P2 | Frontend | 2.3 |
| TODO-SALES004-007 | Create PaymentsPage.tsx | P2 | Frontend | 2.1 |
| TODO-SALES004-008 | Create PaymentForm.tsx | P2 | Frontend | 2.2 |
| TODO-SALES004-009 | Create PaymentHistory.tsx | P2 | Frontend | 2.2 |
| TODO-SALES004-010 | Create RefundDialog.tsx | P2 | Frontend | 2.2 |
| TODO-SALES004-011 | Implement Stripe gateway integration | P2 | Backend | 6.2 |
| TODO-SALES004-012 | Create gateway webhook endpoints | P2 | Backend | 6.2 |
| TODO-SALES004-013 | Create PaymentServiceTests.cs | P2 | Testing | 5.1 |
| TODO-SALES004-014 | Create PaymentsControllerTests.cs | P2 | Testing | 5.2 |

---

## Appendix A: Payment Status Flow

```
                    ┌──────────────┐
                    │   Pending    │ ◀──── New Payment
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
        ┌───────────│  Processing  │───────────┐
        │           └──────────────┘           │
        │                  │                   │
        ▼                  ▼                   ▼
 ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
 │   Declined   │   │  Completed   │   │    Failed    │
 └──────────────┘   └──────┬───────┘   └──────┬───────┘
                           │                   │
                           │                   │ Retry
                           │                   ▼
                           │           ┌──────────────┐
                           │           │  Processing  │ (RetryCount++)
                           │           └──────────────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  Refunded    │ │  Partially   │ │   Disputed   │
    │  (Full)      │ │  Refunded    │ │ (Chargeback) │
    └──────────────┘ └──────────────┘ └──────────────┘
```

---

## Appendix B: Payment Number Format

| Component | Format | Example |
|-----------|--------|---------|
| Prefix | PAY | PAY |
| Year | YY | 26 |
| Month | MM | 02 |
| Sequence | NNNN | 0001 |
| **Full Format** | PAY-YYMM-NNNN | PAY-2602-0001 |

**Rules:**
- Sequence resets each month
- Sequence is 4 digits, zero-padded
- Generated automatically by PaymentService

---

**END OF SPECIFICATION**
