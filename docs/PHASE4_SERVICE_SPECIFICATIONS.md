# Phase 4 Service Specifications

> **Version:** 1.0  
> **Created:** 2026-02-05  
> **Purpose:** Technical reference for Phase 4 service implementations  
> **Status:** Canonical reference - implementations MUST match these signatures exactly

---

## Table of Contents

1. [Overview](#overview)
2. [IInvoiceService](#1-iinvoiceservice)
3. [IPaymentService](#2-ipaymentservice)
4. [IOrderService](#3-iorderservice)
5. [IContractService](#4-icontractservice)
6. [ISubscriptionService](#5-isubscriptionservice)
7. [ITeamService](#6-iteamservice)
8. [ICommissionService](#7-icommissionservice)
9. [IEmailTemplateService](#8-iemailtemplateservice)
10. [Implementation Guidelines](#implementation-guidelines)
11. [DI Registration](#di-registration)

---

## Overview

Phase 4 implements 8 business services for Sales, Billing, and Team operations:

| Service | Entity | Interface Location | Purpose |
|---------|--------|-------------------|---------|
| InvoiceService | Invoice | `CRM.Core/Interfaces/IInvoiceService.cs` | Invoice lifecycle, payments, line items |
| PaymentService | Payment | `CRM.Core/Interfaces/IPaymentService.cs` | Payment processing, refunds, reconciliation |
| OrderService | Order | `CRM.Core/Interfaces/IOrderService.cs` | Order lifecycle, fulfillment, invoicing |
| ContractService | Contract | `CRM.Core/Interfaces/IContractService.cs` | Contract management, renewal, signatures |
| SubscriptionService | Subscription | `CRM.Core/Interfaces/ISubscriptionService.cs` | Subscription billing, usage, MRR/ARR |
| TeamService | Team | `CRM.Core/Interfaces/ITeamService.cs` | Team management, territories, performance |
| CommissionService | Commission | `CRM.Core/Interfaces/ICommissionService.cs` | Commission calculation, plans, payouts |
| EmailTemplateService | EmailTemplate | `CRM.Core/Interfaces/IEmailTemplateService.cs` | Template rendering, versioning, testing |

### Common Patterns

All services follow these patterns:

```csharp
// All async methods include CancellationToken as last parameter with default value
Task<T> MethodAsync(..., CancellationToken cancellationToken = default);

// CRUD operations return entity or bool for delete
Task<Entity> GetByIdAsync(int id, CancellationToken cancellationToken = default);
Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken = default);
Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

// GetAll methods use nullable parameters for optional filtering
Task<IEnumerable<Entity>> GetAllAsync(int? filter1 = null, Status? filter2 = null, CancellationToken cancellationToken = default);
```

---

## 1. IInvoiceService

**File:** `CRM.Core/Interfaces/IInvoiceService.cs`  
**Implementation:** `CRM.Infrastructure/Services/InvoiceService.cs`

### 1.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Invoice>> GetAllAsync(
    int? customerId = null,
    InvoiceStatus? status = null,
    CancellationToken cancellationToken = default);

Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);

Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default);

Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Invoice Operations

```csharp
Task<Invoice> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);

Task<Invoice> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);

Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);

Task<bool> SendInvoiceAsync(int invoiceId, string? recipientEmail = null, CancellationToken cancellationToken = default);

Task<Invoice> MarkAsViewedAsync(int invoiceId, CancellationToken cancellationToken = default);
```

#### Status Management

```csharp
Task<Invoice> UpdateStatusAsync(int invoiceId, InvoiceStatus status, CancellationToken cancellationToken = default);

Task<Invoice> ApproveAsync(int invoiceId, int approvedById, CancellationToken cancellationToken = default);

Task<Invoice> VoidAsync(int invoiceId, string reason, CancellationToken cancellationToken = default);

Task<Invoice> MarkAsPaidAsync(int invoiceId, CancellationToken cancellationToken = default);
```

#### Payment Management

```csharp
Task<Payment> RecordPaymentAsync(int invoiceId, decimal amount, PaymentMethod method, CancellationToken cancellationToken = default);

Task<decimal> GetOutstandingBalanceAsync(int invoiceId, CancellationToken cancellationToken = default);

Task<IEnumerable<Payment>> GetPaymentsAsync(int invoiceId, CancellationToken cancellationToken = default);
```

#### Queries

```csharp
Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);

Task<IEnumerable<Invoice>> GetInvoicesDueAsync(int withinDays, CancellationToken cancellationToken = default);

Task<InvoiceStatistics> GetCustomerStatisticsAsync(int customerId, CancellationToken cancellationToken = default);
```

#### Line Items

```csharp
Task<InvoiceLineItem> AddLineItemAsync(int invoiceId, InvoiceLineItem lineItem, CancellationToken cancellationToken = default);

Task<InvoiceLineItem> UpdateLineItemAsync(InvoiceLineItem lineItem, CancellationToken cancellationToken = default);

Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default);

Task<IEnumerable<InvoiceLineItem>> GetLineItemsAsync(int invoiceId, CancellationToken cancellationToken = default);
```

#### Calculations

```csharp
Task<Invoice> RecalculateTotalsAsync(int invoiceId, CancellationToken cancellationToken = default);

Task<Invoice> ApplyDiscountAsync(int invoiceId, decimal discountAmount, string? discountReason = null, CancellationToken cancellationToken = default);
```

### 1.2 Supporting Types

```csharp
public class InvoiceStatistics
{
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AverageInvoiceAmount { get; set; }
    public int AverageDaysToPayment { get; set; }
}
```

---

## 2. IPaymentService

**File:** `CRM.Core/Interfaces/IPaymentService.cs`  
**Implementation:** `CRM.Infrastructure/Services/PaymentService.cs`

### 2.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Payment>> GetAllAsync(
    int? customerId = null,
    int? invoiceId = null,
    PaymentStatus? status = null,
    CancellationToken cancellationToken = default);

Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);

Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);

Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Payment Processing

```csharp
Task<PaymentResult> ProcessPaymentAsync(
    int invoiceId,
    decimal amount,
    PaymentMethod method,
    PaymentDetails? details = null,
    CancellationToken cancellationToken = default);

Task<PaymentResult> ProcessRefundAsync(
    int paymentId,
    decimal amount,
    string reason,
    CancellationToken cancellationToken = default);

Task<Payment> VoidPaymentAsync(int paymentId, string reason, CancellationToken cancellationToken = default);

Task<PaymentResult> CapturePaymentAsync(int paymentId, decimal? amount = null, CancellationToken cancellationToken = default);
```

#### Status Management

```csharp
Task<Payment> UpdateStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default);

Task<Payment> MarkAsCompletedAsync(int paymentId, CancellationToken cancellationToken = default);

Task<Payment> MarkAsFailedAsync(int paymentId, string failureReason, CancellationToken cancellationToken = default);
```

#### Queries

```csharp
Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);

Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default);

Task<PaymentStatistics> GetStatisticsAsync(
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Payment>> GetCustomerPaymentHistoryAsync(
    int customerId,
    CancellationToken cancellationToken = default);
```

#### Reconciliation

```csharp
Task<Payment> ReconcilePaymentAsync(
    int paymentId,
    string externalReference,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Payment>> GetUnreconciledPaymentsAsync(CancellationToken cancellationToken = default);

Task<IEnumerable<PaymentAllocation>> ApplyPaymentToInvoicesAsync(
    int paymentId,
    IEnumerable<PaymentAllocation> allocations,
    CancellationToken cancellationToken = default);
```

#### Retry Operations

```csharp
Task<PaymentResult> RetryPaymentAsync(int paymentId, CancellationToken cancellationToken = default);

Task<Payment> SchedulePaymentAsync(
    int invoiceId,
    decimal amount,
    PaymentMethod method,
    DateTime scheduledDate,
    CancellationToken cancellationToken = default);
```

### 2.2 Supporting Types

```csharp
public class PaymentResult
{
    public bool Success { get; set; }
    public Payment? Payment { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PaymentDetails
{
    public string? CardNumber { get; set; }
    public string? CardExpiry { get; set; }
    public string? CardCvv { get; set; }
    public string? CardHolderName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string? PaymentToken { get; set; }
    public string? ExternalReference { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

public class PaymentAllocation
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    public double SuccessRate { get; set; }
    public decimal AveragePaymentAmount { get; set; }
    public Dictionary<PaymentMethod, decimal> AmountByMethod { get; set; } = new();
}
```

---

## 3. IOrderService

**File:** `CRM.Core/Interfaces/IOrderService.cs`  
**Implementation:** `CRM.Infrastructure/Services/OrderService.cs`

### 3.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Order>> GetAllAsync(
    int? customerId = null,
    OrderStatus? status = null,
    CancellationToken cancellationToken = default);

Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Order Operations

```csharp
Task<Order> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);

Task<Order> CreateFromOpportunityAsync(int opportunityId, CancellationToken cancellationToken = default);

Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);

Task<Order> CloneOrderAsync(int orderId, CancellationToken cancellationToken = default);
```

#### Status Management

```csharp
Task<Order> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);

Task<Order> SubmitForApprovalAsync(int orderId, CancellationToken cancellationToken = default);

Task<Order> ApproveAsync(int orderId, int approvedById, CancellationToken cancellationToken = default);

Task<Order> RejectAsync(int orderId, int rejectedById, string reason, CancellationToken cancellationToken = default);

Task<Order> CancelAsync(int orderId, string reason, CancellationToken cancellationToken = default);

Task<Order> PutOnHoldAsync(int orderId, string reason, CancellationToken cancellationToken = default);

Task<Order> ReleaseFromHoldAsync(int orderId, CancellationToken cancellationToken = default);
```

#### Fulfillment

```csharp
Task<Order> MarkAsFulfilledAsync(int orderId, CancellationToken cancellationToken = default);

Task<Order> MarkAsPartiallyFulfilledAsync(int orderId, IEnumerable<int> fulfilledLineItemIds, CancellationToken cancellationToken = default);

Task<Order> MarkAsDeliveredAsync(int orderId, CancellationToken cancellationToken = default);

Task<Order> ProcessReturnAsync(
    int orderId,
    IEnumerable<OrderReturnItem> returnItems,
    string reason,
    CancellationToken cancellationToken = default);
```

#### Line Items

```csharp
Task<OrderLineItem> AddLineItemAsync(int orderId, OrderLineItem lineItem, CancellationToken cancellationToken = default);

Task<OrderLineItem> UpdateLineItemAsync(OrderLineItem lineItem, CancellationToken cancellationToken = default);

Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default);

Task<IEnumerable<OrderLineItem>> GetLineItemsAsync(int orderId, CancellationToken cancellationToken = default);
```

#### Queries

```csharp
Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

Task<IEnumerable<Order>> GetByDateRangeAsync(
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Order>> GetOrdersRequiringActionAsync(CancellationToken cancellationToken = default);

Task<OrderStatistics> GetStatisticsAsync(
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Order>> SearchAsync(string query, CancellationToken cancellationToken = default);
```

#### Calculations

```csharp
Task<Order> RecalculateTotalsAsync(int orderId, CancellationToken cancellationToken = default);

Task<Order> ApplyDiscountAsync(
    int orderId,
    decimal discountAmount,
    string? discountReason = null,
    CancellationToken cancellationToken = default);

Task<Order> ApplyCouponAsync(int orderId, string couponCode, CancellationToken cancellationToken = default);
```

#### Invoicing

```csharp
Task<Invoice> CreateInvoiceAsync(int orderId, CancellationToken cancellationToken = default);

Task<IEnumerable<Invoice>> GetInvoicesAsync(int orderId, CancellationToken cancellationToken = default);
```

### 3.2 Supporting Types

```csharp
public class OrderReturnItem
{
    public int OrderLineItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Condition { get; set; }
}

public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int FulfilledOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new();
}
```

---

## 4. IContractService

**File:** `CRM.Core/Interfaces/IContractService.cs`  
**Implementation:** `CRM.Infrastructure/Services/ContractService.cs`

### 4.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Contract>> GetAllAsync(
    int? customerId = null,
    ContractStatus? status = null,
    CancellationToken cancellationToken = default);

Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken cancellationToken = default);

Task<Contract> CreateAsync(Contract contract, CancellationToken cancellationToken = default);

Task<Contract> UpdateAsync(Contract contract, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Contract Operations

```csharp
Task<Contract> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);

Task<Contract> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);

Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken = default);

Task<Contract> CloneForRenewalAsync(int contractId, CancellationToken cancellationToken = default);
```

#### Status Management

```csharp
Task<Contract> UpdateStatusAsync(int contractId, ContractStatus status, CancellationToken cancellationToken = default);

Task<Contract> ActivateAsync(int contractId, CancellationToken cancellationToken = default);

Task<Contract> SuspendAsync(int contractId, string reason, CancellationToken cancellationToken = default);

Task<Contract> TerminateAsync(
    int contractId,
    string reason,
    DateTime? terminationDate = null,
    CancellationToken cancellationToken = default);

Task<Contract> ExpireAsync(int contractId, CancellationToken cancellationToken = default);
```

#### Renewal Management

```csharp
Task<Contract> InitiateRenewalAsync(int contractId, CancellationToken cancellationToken = default);

Task<Contract> CompleteRenewalAsync(int contractId, int newContractId, CancellationToken cancellationToken = default);

Task<IEnumerable<Contract>> GetContractsDueForRenewalAsync(int withinDays, CancellationToken cancellationToken = default);

Task<IEnumerable<Contract>> GetRenewalHistoryAsync(int contractId, CancellationToken cancellationToken = default);
```

#### Amendment

```csharp
Task<Contract> CreateAmendmentAsync(int contractId, Contract amendment, CancellationToken cancellationToken = default);

Task<IEnumerable<Contract>> GetAmendmentsAsync(int contractId, CancellationToken cancellationToken = default);
```

#### Signature Management

```csharp
Task<Contract> SendForSignatureAsync(
    int contractId,
    IEnumerable<ContractSigner> signers,
    CancellationToken cancellationToken = default);

Task<Contract> RecordSignatureAsync(
    int contractId,
    string signerId,
    string signatureData,
    CancellationToken cancellationToken = default);

Task<ContractSignatureStatus> GetSignatureStatusAsync(int contractId, CancellationToken cancellationToken = default);
```

#### Queries

```csharp
Task<IEnumerable<Contract>> GetActiveContractsAsync(int customerId, CancellationToken cancellationToken = default);

Task<IEnumerable<Contract>> GetExpiringContractsAsync(
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<ContractStatistics> GetStatisticsAsync(
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Contract>> SearchAsync(string query, CancellationToken cancellationToken = default);

Task<decimal> GetTotalContractValueAsync(int customerId, CancellationToken cancellationToken = default);
```

#### Documents

```csharp
Task<ContractDocument> AttachDocumentAsync(
    int contractId,
    string documentPath,
    string documentType,
    CancellationToken cancellationToken = default);

Task<IEnumerable<ContractDocument>> GetDocumentsAsync(int contractId, CancellationToken cancellationToken = default);

Task<byte[]> GenerateContractPdfAsync(int contractId, CancellationToken cancellationToken = default);
```

### 4.2 Supporting Types

```csharp
public class ContractSigner
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class ContractSignatureStatus
{
    public int ContractId { get; set; }
    public bool IsFullySigned { get; set; }
    public int TotalSigners { get; set; }
    public int SignedCount { get; set; }
    public List<SignerStatus> Signers { get; set; } = new();
}

public class SignerStatus
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasSigned { get; set; }
    public DateTime? SignedAt { get; set; }
}

public class ContractDocument
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public int? UploadedById { get; set; }
}

public class ContractStatistics
{
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int ExpiringContracts { get; set; }
    public int ExpiredContracts { get; set; }
    public int PendingRenewals { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal ActiveContractValue { get; set; }
    public double RenewalRate { get; set; }
    public double AverageContractLength { get; set; }
    public Dictionary<ContractType, int> ContractsByType { get; set; } = new();
}
```

---

## 5. ISubscriptionService

**File:** `CRM.Core/Interfaces/ISubscriptionService.cs`  
**Implementation:** `CRM.Infrastructure/Services/SubscriptionService.cs`

### 5.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Subscription>> GetAllAsync(
    int? customerId = null,
    SubscriptionStatus? status = null,
    CancellationToken cancellationToken = default);

Task<Subscription?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Subscription?> GetBySubscriptionNumberAsync(string subscriptionNumber, CancellationToken cancellationToken = default);

Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default);

Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Subscription Operations

```csharp
Task<Subscription> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default);

Task<string> GenerateSubscriptionNumberAsync(CancellationToken cancellationToken = default);

Task<Subscription> ActivateAsync(int subscriptionId, CancellationToken cancellationToken = default);

Task<Subscription> PauseAsync(int subscriptionId, string? reason = null, CancellationToken cancellationToken = default);

Task<Subscription> ResumeAsync(int subscriptionId, CancellationToken cancellationToken = default);

Task<Subscription> CancelAsync(
    int subscriptionId,
    string reason,
    bool immediate = false,
    CancellationToken cancellationToken = default);
```

#### Status Management

```csharp
Task<Subscription> UpdateStatusAsync(int subscriptionId, SubscriptionStatus status, CancellationToken cancellationToken = default);

Task<Subscription> SuspendAsync(int subscriptionId, string reason, CancellationToken cancellationToken = default);

Task<Subscription> ReactivateAsync(int subscriptionId, CancellationToken cancellationToken = default);
```

#### Billing

```csharp
Task<Invoice> GenerateInvoiceAsync(int subscriptionId, CancellationToken cancellationToken = default);

Task<IEnumerable<Invoice>> GetBillingHistoryAsync(int subscriptionId, CancellationToken cancellationToken = default);

Task<decimal> CalculateProratedAmountAsync(
    int subscriptionId,
    DateTime changeDate,
    decimal newAmount,
    CancellationToken cancellationToken = default);

Task<DateTime?> GetNextBillingDateAsync(int subscriptionId, CancellationToken cancellationToken = default);

Task<Subscription> UpdateBillingDetailsAsync(
    int subscriptionId,
    BillingDetails details,
    CancellationToken cancellationToken = default);
```

#### Plan Changes

```csharp
Task<Subscription> UpgradeAsync(
    int subscriptionId,
    int newPlanId,
    bool immediate = true,
    CancellationToken cancellationToken = default);

Task<Subscription> DowngradeAsync(int subscriptionId, int newPlanId, CancellationToken cancellationToken = default);

Task<Subscription> ChangePlanAsync(
    int subscriptionId,
    int newPlanId,
    SubscriptionChangeType changeType,
    CancellationToken cancellationToken = default);

Task<Subscription> AddAddonAsync(
    int subscriptionId,
    int addonId,
    int quantity = 1,
    CancellationToken cancellationToken = default);

Task<Subscription> RemoveAddonAsync(int subscriptionId, int addonId, CancellationToken cancellationToken = default);
```

#### Renewal

```csharp
Task<Subscription> RenewAsync(int subscriptionId, CancellationToken cancellationToken = default);

Task<IEnumerable<Subscription>> GetDueForRenewalAsync(int withinDays, CancellationToken cancellationToken = default);

Task<Subscription> SetAutoRenewalAsync(int subscriptionId, bool autoRenew, CancellationToken cancellationToken = default);
```

#### Usage

```csharp
Task<bool> RecordUsageAsync(
    int subscriptionId,
    string metricName,
    decimal quantity,
    DateTime? timestamp = null,
    CancellationToken cancellationToken = default);

Task<SubscriptionUsageData> GetUsageAsync(
    int subscriptionId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<IEnumerable<UsageLimit>> GetUsageLimitsAsync(int subscriptionId, CancellationToken cancellationToken = default);
```

#### Queries

```csharp
Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(int customerId, CancellationToken cancellationToken = default);

Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<SubscriptionStatistics> GetStatisticsAsync(
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<decimal> CalculateMRRAsync(CancellationToken cancellationToken = default);

Task<decimal> CalculateARRAsync(CancellationToken cancellationToken = default);

Task<double> GetChurnRateAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
```

### 5.2 Supporting Types

```csharp
public class BillingDetails
{
    public string? BillingEmail { get; set; }
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string? BillingCountry { get; set; }
    public string? PaymentMethodId { get; set; }
}

public enum SubscriptionChangeType
{
    Immediate,
    EndOfPeriod,
    NextBillingCycle
}

public class SubscriptionUsageData
{
    public int SubscriptionId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<UsageMetric> Metrics { get; set; } = new();
}

public class UsageMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal TotalUsage { get; set; }
    public string? Unit { get; set; }
    public List<UsageRecord> Records { get; set; } = new();
}

public class UsageRecord
{
    public DateTime Timestamp { get; set; }
    public decimal Quantity { get; set; }
}

public class UsageLimit
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining => Limit - Used;
    public double UsagePercentage => Limit > 0 ? (double)(Used / Limit) * 100 : 0;
}

public class SubscriptionStatistics
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int PausedSubscriptions { get; set; }
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public double ChurnRate { get; set; }
    public double ConversionRate { get; set; }
    public decimal AverageRevenuePerUser { get; set; }
    public int NewSubscriptionsThisMonth { get; set; }
    public int CancellationsThisMonth { get; set; }
    public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
}
```

---

## 6. ITeamService

**File:** `CRM.Core/Interfaces/ITeamService.cs`  
**Implementation:** `CRM.Infrastructure/Services/TeamService.cs`

### 6.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Team>> GetAllAsync(
    bool? isActive = null,
    int? managerId = null,
    CancellationToken cancellationToken = default);

Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

Task<Team> CreateAsync(Team team, CancellationToken cancellationToken = default);

Task<Team> UpdateAsync(Team team, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Member Management

```csharp
Task<TeamMember> AddMemberAsync(
    int teamId,
    int userId,
    TeamRole role = TeamRole.Member,
    CancellationToken cancellationToken = default);

Task<bool> RemoveMemberAsync(int teamId, int userId, CancellationToken cancellationToken = default);

Task<TeamMember> UpdateMemberRoleAsync(
    int teamId,
    int userId,
    TeamRole newRole,
    CancellationToken cancellationToken = default);

Task<IEnumerable<TeamMember>> GetMembersAsync(int teamId, CancellationToken cancellationToken = default);

Task<IEnumerable<Team>> GetTeamsForUserAsync(int userId, CancellationToken cancellationToken = default);

Task<bool> IsMemberAsync(int teamId, int userId, CancellationToken cancellationToken = default);
```

#### Team Manager

```csharp
Task<Team> SetManagerAsync(int teamId, int managerId, CancellationToken cancellationToken = default);

Task<IEnumerable<Team>> GetManagedTeamsAsync(int managerId, CancellationToken cancellationToken = default);
```

#### Territory Management

```csharp
Task<bool> AssignTerritoryAsync(int teamId, int territoryId, CancellationToken cancellationToken = default);

Task<bool> RemoveTerritoryAsync(int teamId, int territoryId, CancellationToken cancellationToken = default);

Task<IEnumerable<AccountTerritory>> GetTerritoriesAsync(int teamId, CancellationToken cancellationToken = default);

Task<Team?> GetTeamByTerritoryAsync(int territoryId, CancellationToken cancellationToken = default);
```

#### Account Assignment

```csharp
Task<bool> AssignAccountAsync(int teamId, int accountId, CancellationToken cancellationToken = default);

Task<bool> RemoveAccountAsync(int teamId, int accountId, CancellationToken cancellationToken = default);

Task<IEnumerable<Account>> GetAssignedAccountsAsync(int teamId, CancellationToken cancellationToken = default);

Task<Team?> GetTeamByAccountAsync(int accountId, CancellationToken cancellationToken = default);

Task<int> BulkAssignAccountsAsync(int teamId, IEnumerable<int> accountIds, CancellationToken cancellationToken = default);
```

#### Performance & Stats

```csharp
Task<TeamPerformance> GetPerformanceAsync(
    int teamId,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<TeamStatistics> GetStatisticsAsync(int teamId, CancellationToken cancellationToken = default);

Task<IEnumerable<TeamRanking>> GetLeaderboardAsync(
    int topN = 10,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<MemberPerformance>> GetMemberPerformanceAsync(
    int teamId,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);
```

#### Hierarchy

```csharp
Task<IEnumerable<Team>> GetChildTeamsAsync(int parentTeamId, CancellationToken cancellationToken = default);

Task<Team?> GetParentTeamAsync(int teamId, CancellationToken cancellationToken = default);

Task<Team> SetParentTeamAsync(int teamId, int? parentTeamId, CancellationToken cancellationToken = default);

Task<TeamHierarchy> GetHierarchyAsync(int? rootTeamId = null, CancellationToken cancellationToken = default);
```

### 6.2 Supporting Types

```csharp
public enum TeamRole
{
    Member = 0,
    Lead = 1,
    Manager = 2,
    Admin = 3
}

public class TeamPerformance
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalQuotaValue { get; set; }
    public double QuotaAttainment { get; set; }
    public int DealsWon { get; set; }
    public int DealsLost { get; set; }
    public double WinRate { get; set; }
    public decimal AverageDealSize { get; set; }
    public int NewAccounts { get; set; }
    public int ActiveOpportunities { get; set; }
    public decimal PipelineValue { get; set; }
}

public class TeamStatistics
{
    public int TeamId { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int AssignedAccounts { get; set; }
    public int ActiveOpportunities { get; set; }
    public int AssignedTerritories { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class TeamRanking
{
    public int Rank { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int DealsWon { get; set; }
    public double QuotaAttainment { get; set; }
}

public class MemberPerformance
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int DealsWon { get; set; }
    public int DealsLost { get; set; }
    public double WinRate { get; set; }
    public decimal PipelineValue { get; set; }
    public double QuotaAttainment { get; set; }
}

public class TeamHierarchy
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int MemberCount { get; set; }
    public List<TeamHierarchy> Children { get; set; } = new();
}
```

---

## 7. ICommissionService

**File:** `CRM.Core/Interfaces/ICommissionService.cs`  
**Implementation:** `CRM.Infrastructure/Services/CommissionService.cs`

### 7.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<Commission>> GetAllAsync(
    int? userId = null,
    CommissionStatus? status = null,
    CancellationToken cancellationToken = default);

Task<Commission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<Commission> CreateAsync(Commission commission, CancellationToken cancellationToken = default);

Task<Commission> UpdateAsync(Commission commission, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Commission Calculation

```csharp
Task<CommissionCalculation> CalculateForDealAsync(int opportunityId, CancellationToken cancellationToken = default);

Task<CommissionCalculation> CalculateForOrderAsync(int orderId, CancellationToken cancellationToken = default);

Task<CommissionSummary> CalculateForPeriodAsync(
    int userId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<Commission> RecalculateAsync(int commissionId, CancellationToken cancellationToken = default);
```

#### Commission Plans

```csharp
Task<IEnumerable<CommissionPlan>> GetPlansAsync(bool? isActive = null, CancellationToken cancellationToken = default);

Task<CommissionPlan?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken = default);

Task<CommissionPlan> CreatePlanAsync(CommissionPlan plan, CancellationToken cancellationToken = default);

Task<CommissionPlan> UpdatePlanAsync(CommissionPlan plan, CancellationToken cancellationToken = default);

Task<bool> DeletePlanAsync(int planId, CancellationToken cancellationToken = default);

Task<bool> AssignPlanToUserAsync(
    int planId,
    int userId,
    DateTime? effectiveDate = null,
    CancellationToken cancellationToken = default); <!-- ✅ Implemented: Persists CommissionPlanAssignment, soft-deletes previous, test verifies persistence -->

Task<CommissionPlan?> GetUserPlanAsync(int userId, CancellationToken cancellationToken = default);
```

#### Status Management

```csharp
Task<Commission> UpdateStatusAsync(int commissionId, CommissionStatus status, CancellationToken cancellationToken = default);

Task<Commission> ApproveAsync(int commissionId, int approvedById, CancellationToken cancellationToken = default);

Task<Commission> RejectAsync(int commissionId, string reason, CancellationToken cancellationToken = default);

Task<Commission> MarkAsPaidAsync(int commissionId, DateTime? paidDate = null, CancellationToken cancellationToken = default);

Task<Commission> ClawbackAsync(int commissionId, string reason, CancellationToken cancellationToken = default);
```

#### Statements

```csharp
Task<CommissionStatement> GenerateStatementAsync(
    int userId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

Task<IEnumerable<CommissionStatement>> GetStatementsAsync(int userId, CancellationToken cancellationToken = default);

Task<CommissionStatement?> GetStatementByIdAsync(int statementId, CancellationToken cancellationToken = default);

Task<CommissionStatement> FinalizeStatementAsync(int statementId, CancellationToken cancellationToken = default);
```

#### Queries

```csharp
Task<IEnumerable<Commission>> GetByUserAsync(
    int userId,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<Commission>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);

Task<IEnumerable<Commission>> GetReadyForPayoutAsync(CancellationToken cancellationToken = default);

Task<CommissionStatistics> GetStatisticsAsync(
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<CommissionLeaderboard>> GetLeaderboardAsync(
    int topN = 10,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task<CommissionForecast> GetForecastAsync(int userId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
```

#### Tiers

```csharp
Task<IEnumerable<CommissionTier>> GetTiersAsync(int planId, CancellationToken cancellationToken = default);

Task<CommissionTier> AddTierAsync(int planId, CommissionTier tier, CancellationToken cancellationToken = default);

Task<CommissionTier> UpdateTierAsync(CommissionTier tier, CancellationToken cancellationToken = default);

Task<bool> RemoveTierAsync(int tierId, CancellationToken cancellationToken = default);
```

### 7.2 Supporting Types

```csharp
public class CommissionCalculation
{
    public int UserId { get; set; }
    public int? OpportunityId { get; set; }
    public int? OrderId { get; set; }
    public int? PlanId { get; set; }
    public string? PlanName { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CalculatedAmount { get; set; }
    public decimal? Accelerator { get; set; }
    public decimal FinalAmount { get; set; }
    public int? TierLevel { get; set; }
    public string? TierName { get; set; }
    public List<CommissionBreakdown> Breakdown { get; set; } = new();
}

public class CommissionBreakdown
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public decimal Result { get; set; }
}

public class CommissionSummary
{
    public int UserId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalClawedBack { get; set; }
    public int DealCount { get; set; }
    public decimal AverageCommission { get; set; }
    public List<Commission> Commissions { get; set; } = new();
}

public class CommissionStatistics
{
    public decimal TotalCommissions { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public int TotalRecords { get; set; }
    public int PendingApprovals { get; set; }
    public decimal AverageCommission { get; set; }
    public int ActivePlans { get; set; }
    public Dictionary<string, decimal> CommissionsByPlan { get; set; } = new();
}

public class CommissionLeaderboard
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalEarned { get; set; }
    public int DealCount { get; set; }
    public decimal AverageDealSize { get; set; }
}

public class CommissionForecast
{
    public int UserId { get; set; }
    public decimal CurrentEarned { get; set; }
    public decimal ForecastedEarnings { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal ExpectedFromPipeline { get; set; }
    public decimal QuotaProgress { get; set; }
    public decimal ProjectedQuotaAttainment { get; set; }
    public List<ForecastedDeal> ForecastedDeals { get; set; } = new();
}

public class ForecastedDeal
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public decimal DealValue { get; set; }
    public decimal ExpectedCommission { get; set; }
    public double Probability { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
}
```

---

## 8. IEmailTemplateService

**File:** `CRM.Core/Interfaces/IEmailTemplateService.cs`  
**Implementation:** `CRM.Infrastructure/Services/EmailTemplateService.cs`

### 8.1 Method Signatures

#### CRUD Operations

```csharp
Task<IEnumerable<EmailTemplate>> GetAllAsync(
    EmailTemplateCategory? category = null,
    bool? isActive = null,
    CancellationToken cancellationToken = default);

Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

Task<EmailTemplate?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken cancellationToken = default);

Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);

Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

#### Template Rendering

```csharp
Task<RenderedEmail> RenderAsync(
    int templateId,
    Dictionary<string, object> data,
    CancellationToken cancellationToken = default);

Task<RenderedEmail> RenderByNameAsync(
    string templateName,
    Dictionary<string, object> data,
    CancellationToken cancellationToken = default);

Task<RenderedEmail> RenderForEntityAsync(
    int templateId,
    string entityType,
    int entityId,
    CancellationToken cancellationToken = default);

Task<RenderedEmail> PreviewAsync(int templateId, CancellationToken cancellationToken = default);

Task<TemplateValidationResult> ValidateAsync(string templateContent, CancellationToken cancellationToken = default);
```

#### Template Testing

```csharp
Task<bool> SendTestAsync(
    int templateId,
    string recipientEmail,
    Dictionary<string, object>? testData = null,
    CancellationToken cancellationToken = default);

Task<Dictionary<string, object>> GetSampleDataAsync(
    EmailTemplateCategory category,
    CancellationToken cancellationToken = default);
```

#### Template Versioning

```csharp
Task<IEnumerable<EmailTemplateVersion>> GetVersionHistoryAsync(int templateId, CancellationToken cancellationToken = default);

Task<EmailTemplateVersion?> GetVersionAsync(int templateId, int version, CancellationToken cancellationToken = default);

Task<EmailTemplate> RestoreVersionAsync(int templateId, int version, CancellationToken cancellationToken = default);

Task<EmailTemplateVersion> CreateVersionAsync(
    int templateId,
    string changeDescription,
    CancellationToken cancellationToken = default);
```

#### Template Categories

```csharp
Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default);

Task<IEnumerable<TemplateCategoryInfo>> GetCategoriesAsync(CancellationToken cancellationToken = default);
```

#### Template Variables

```csharp
Task<IEnumerable<TemplateVariable>> GetAvailableVariablesAsync(
    EmailTemplateCategory category,
    CancellationToken cancellationToken = default);

Task<IEnumerable<string>> ExtractVariablesAsync(string templateContent, CancellationToken cancellationToken = default);
```

#### Cloning & Import/Export

```csharp
Task<EmailTemplate> CloneAsync(int templateId, string newName, CancellationToken cancellationToken = default);

Task<EmailTemplate> ImportAsync(string templateJson, CancellationToken cancellationToken = default);

Task<string> ExportAsync(int templateId, CancellationToken cancellationToken = default);
```

#### Statistics & Usage

```csharp
Task<TemplateUsageStats> GetUsageStatsAsync(
    int templateId,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken cancellationToken = default);

Task RecordUsageAsync(
    int templateId,
    int? userId = null,
    string? context = null,
    CancellationToken cancellationToken = default);

Task<IEnumerable<TemplateUsageSummary>> GetMostUsedAsync(
    int topN = 10,
    DateTime? fromDate = null,
    CancellationToken cancellationToken = default);
```

#### Default Templates

```csharp
Task<EmailTemplate?> GetDefaultTemplateAsync(EmailTemplatePurpose purpose, CancellationToken cancellationToken = default);

Task<bool> SetAsDefaultAsync(int templateId, EmailTemplatePurpose purpose, CancellationToken cancellationToken = default);
```

### 8.2 Supporting Types

```csharp
public class RenderedEmail
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public string? ReplyTo { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class TemplateValidationResult
{
    public bool IsValid { get; set; }
    public List<TemplateValidationError> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> UsedVariables { get; set; } = new();
}

public class TemplateValidationError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class EmailTemplateVersion
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int Version { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? ChangeDescription { get; set; }
    public int? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TemplateCategoryInfo
{
    public EmailTemplateCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TemplateCount { get; set; }
}

public class TemplateVariable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? SampleValue { get; set; }
}

public class TemplateUsageStats
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalUsages { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public List<UsageByDay> UsageHistory { get; set; } = new();
}

public class UsageByDay
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class TemplateUsageSummary
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public EmailTemplateCategory Category { get; set; }
    public int UsageCount { get; set; }
}

public enum EmailTemplatePurpose
{
    WelcomeEmail,
    PasswordReset,
    OrderConfirmation,
    InvoiceNotification,
    QuoteApproval,
    LeadFollowUp,
    OpportunityCreated,
    ContractSigning,
    SubscriptionRenewal,
    SupportTicketCreated,
    SupportTicketResolved,
    CampaignMarketing,
    NewsletterWeekly,
    NewsletterMonthly,
    EventInvitation,
    EventReminder,
    ReferralRequest,
    FeedbackRequest,
    AccountActivation,
    AccountDeactivation
}
```

---

## Implementation Guidelines

### Service Class Structure

Each service implementation MUST follow this structure:

```csharp
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of I{Name}Service for {entity} management.
/// </summary>
public class {Name}Service : I{Name}Service
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<{Name}Service> _logger;

    public {Name}Service(ICrmDbContext context, ILogger<{Name}Service> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Implement ALL interface methods exactly as specified
}
```

### Method Implementation Rules

1. **Return Types**: Use exactly the return type specified in the interface
2. **Parameters**: Include ALL parameters with their exact types and default values
3. **CancellationToken**: Always pass to async database operations
4. **Logging**: Log important operations at Info level, errors at Error level
5. **Soft Delete**: Set `IsDeleted = true` instead of removing records
6. **Timestamps**: Set `CreatedAt` on create, `UpdatedAt` on update
7. **Validation**: Validate inputs and throw `ArgumentException` for invalid data

### Database Access Patterns

```csharp
// Query with cancellation token
var entities = await _context.Entities
    .Where(e => !e.IsDeleted)
    .ToListAsync(cancellationToken);

// Find by ID
var entity = await _context.Entities
    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

// Create
_context.Entities.Add(entity);
await _context.SaveChangesAsync(cancellationToken);

// Update
_context.Entities.Update(entity);
await _context.SaveChangesAsync(cancellationToken);

// Soft Delete
entity.IsDeleted = true;
entity.UpdatedAt = DateTime.UtcNow;
await _context.SaveChangesAsync(cancellationToken);
```

---

## DI Registration

Add to `Program.cs` after the Phase 3 services comment:

```csharp
// Phase 4 services - Invoice, Payment, Order, Contract, Subscription, Team, Commission, EmailTemplate
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
```

---

## Entity Dependencies

Ensure these entities exist in `CRM.Core/Entities/`:

| Service | Primary Entity | Related Entities |
|---------|----------------|------------------|
| InvoiceService | Invoice | InvoiceLineItem, Payment, Order, Quote |
| PaymentService | Payment | Invoice |
| OrderService | Order | OrderLineItem, Quote, Opportunity, Invoice |
| ContractService | Contract | Quote, Order |
| SubscriptionService | Subscription | SubscriptionItem, Invoice, Order |
| TeamService | Team | TeamMember, AccountTerritory, Account, User |
| CommissionService | Commission | CommissionPlan, CommissionTier, CommissionStatement, Opportunity, Order, User |
| EmailTemplateService | EmailTemplate | (standalone) |

---

**END OF SPECIFICATION**
