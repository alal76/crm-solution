# SPEC-SALES-003: Invoice Management

> **Version:** 2.1  
> **Last Updated:** February 14, 2026  
> **Status:** ✅ Complete  
> **Module:** Sales & Billing  
> **Priority:** P1  
> **Dependencies:** SPEC-SALES-002 (Order Management), SPEC-SALES-001 (Quote Management), SPEC-CRM-001 (Account Management), SPEC-SYS-008 (Admin Settings)

---

## 1. Business Context

### 1.1 Overview

Invoice Management provides end-to-end invoicing functionality including invoice creation (manual, from orders, or from quotes), lifecycle management, payment tracking, line item management, and overdue/collections handling. The system supports multiple invoice types, configurable payment terms, early payment discounts, late fee calculations, and dunning management.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Invoice Creation | Create invoices manually, from orders, or from quotes | ✅ Implemented |
| SF-002 | Invoice Numbering | Auto-generation of invoice numbers (INV-YYMM-####) | ✅ Implemented |
| SF-003 | Line Item Management | Add/edit/delete invoice line items | ✅ Implemented |
| SF-004 | Pricing Calculations | Automatic totals, tax, discounts, balance | ✅ Implemented |
| SF-005 | Invoice Lifecycle | Draft → Sent → Viewed → Paid/Overdue/Void | ✅ Implemented |
| SF-006 | Payment Recording | Record payments against invoices, track balance | ✅ Implemented |
| SF-007 | Approval Workflow | Invoice approval before sending | ✅ Implemented |
| SF-008 | Invoice Sending | Send invoices to customers via email | ✅ Implemented |
| SF-009 | Overdue Tracking | Track overdue invoices with aging | ✅ Implemented |
| SF-010 | Dunning Management | Automated dunning for overdue invoices | ✅ Implemented |
| SF-011 | Late Fee Calculation | Automatic late fee computation | ✅ Implemented |
| SF-012 | Early Payment Discount | Discount for early payment | ✅ Implemented |
| SF-013 | Credit Memo | Credit memo generation | ⚠️ Partial (entity exists, service pending) |

### 1.3 Functionalities

| ID | Functionality | Description | Status |
|----|---------------|-------------|--------|
| F-001 | List Invoices | Display all invoices with filtering by account, status, dates | ✅ Implemented |
| F-002 | Create Invoice | Create new invoice with header and line items | ✅ Implemented |
| F-003 | Edit Invoice | Modify invoice details (restricted by status) | ✅ Implemented |
| F-004 | Delete Invoice | Soft delete (Draft status only) | ✅ Implemented |
| F-005 | Create from Order | Generate invoice from fulfilled order | ✅ Implemented |
| F-006 | Create from Quote | Generate invoice from accepted quote | ✅ Implemented |
| F-007 | Generate Number | Auto-generate invoice number | ✅ Implemented |
| F-008 | Send Invoice | Send invoice to customer via email | ✅ Implemented |
| F-009 | Mark as Viewed | Record when customer views invoice | ✅ Implemented |
| F-010 | Update Status | Transition invoice through lifecycle | ✅ Implemented |
| F-011 | Approve Invoice | Approve invoice for sending | ✅ Implemented |
| F-012 | Void Invoice | Void invoice with reason | ✅ Implemented |
| F-013 | Mark as Paid | Mark invoice as fully paid | ✅ Implemented |
| F-014 | Record Payment | Record partial or full payment | ✅ Implemented |
| F-015 | Get Outstanding Balance | Calculate remaining balance | ✅ Implemented |
| F-016 | Get Overdue Invoices | List invoices past due date | ✅ Implemented |
| F-017 | Get Invoices Due | List invoices due within N days | ✅ Implemented |
| F-018 | Customer Statistics | Invoice statistics per customer | ✅ Implemented |
| F-019 | Add Line Item | Add line item to invoice | ✅ Implemented |
| F-020 | Recalculate Totals | Recalculate invoice totals from line items | ✅ Implemented |
| F-021 | Apply Discount | Apply discount amount to invoice | ✅ Implemented |

### 1.4 Use Cases

| ID | Use Case | Actor | Precondition | Steps | Postcondition |
|----|----------|-------|--------------|-------|---------------|
| UC-001 | Create Invoice from Order | Finance | Order is fulfilled | 1. Open order 2. Click "Create Invoice" 3. Review data 4. Submit | Invoice created as Draft |
| UC-002 | Send Invoice | Finance | Invoice in Draft/Approved | 1. Open invoice 2. Optionally specify email 3. Click "Send" | Status → Sent |
| UC-003 | Record Payment | Finance | Invoice is Sent/PartiallyPaid | 1. Open invoice 2. Enter payment amount/method 3. Submit | Payment recorded, balance updated |
| UC-004 | Mark as Paid | Finance | Invoice fully paid | 1. Open invoice 2. Click "Mark as Paid" | Status → Paid |
| UC-005 | Void Invoice | Finance/Manager | Invoice not paid | 1. Open invoice 2. Enter void reason 3. Confirm | Status → Void |
| UC-006 | Review Overdue | Finance | Invoices past due | 1. Navigate to invoices 2. Filter by overdue | List of overdue invoices with aging |
| UC-007 | Apply Discount | Finance | Invoice in Draft | 1. Open invoice 2. Enter discount amount/reason 3. Save | Discount applied, totals recalculated |
| UC-008 | Approve Invoice | Manager | Invoice pending approval | 1. Review invoice 2. Click "Approve" | Status → Approved, ApprovedById set |

---

## 2. Frontend Implementation

### 2.1 Pages

| Page | File | Status | Description |
|------|------|--------|-------------|
| InvoicesPage | `CRM.Frontend/src/pages/InvoicesPage.tsx` | ✅ Implemented | Invoice list with DataGrid, filtering, status chips |
| InvoiceDetailsPage | `CRM.Frontend/src/pages/InvoiceDetailsPage.tsx` | ❌ Not Implemented | Invoice detail view with payment tracking |

### 2.2 Services

#### 2.2.1 invoiceService.ts
**File:** `CRM.Frontend/src/services/invoiceService.ts`  
**Status:** ✅ Implemented (205 lines)

**TypeScript Types:**

```typescript
export enum InvoiceStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Sent = 3,
  Viewed = 4,
  PartiallyPaid = 5,
  Paid = 6,
  Overdue = 7,
  Void = 8,
  Disputed = 9,
  WrittenOff = 10,
  InCollections = 11,
  Refunded = 12,
}

export enum PaymentMethod {
  Cash = 0,
  Check = 1,
  CreditCard = 2,
  DebitCard = 3,
  BankTransfer = 4,
  WireTransfer = 5,
  ACH = 6,
  PayPal = 7,
  Stripe = 8,
  Square = 9,
  ApplePay = 10,
  GooglePay = 11,
  Cryptocurrency = 12,
  StoreCredit = 13,
  GiftCard = 14,
  FinancingPlan = 15,
  Other = 16,
}

export interface Invoice {
  id: number;
  invoiceNumber: string;
  invoiceType: number;
  status: InvoiceStatus;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  currency: string;
  accountId: number;
  orderId: number;
  lineItems: InvoiceLineItem[];
}

export interface InvoiceLineItem { ... }
export interface InvoiceStatistics { ... }
export interface RecordPaymentRequest { invoiceId: number; amount: number; method: PaymentMethod; }
export interface DiscountRequest { amount: number; reason: string; }
```

**API Methods (27 total):**

| Method | HTTP | Endpoint | Status |
|--------|------|----------|--------|
| getAll | GET | /api/invoices | ✅ |
| getById | GET | /api/invoices/{id} | ✅ |
| getByInvoiceNumber | GET | /api/invoices/by-number/{invoiceNumber} | ✅ |
| create | POST | /api/invoices | ✅ |
| update | PUT | /api/invoices/{id} | ✅ |
| delete | DELETE | /api/invoices/{id} | ✅ |
| createFromOrder | POST | /api/invoices/from-order/{orderId} | ✅ |
| createFromQuote | POST | /api/invoices/from-quote/{quoteId} | ✅ |
| getNextNumber | GET | /api/invoices/next-number | ✅ |
| send | POST | /api/invoices/{id}/send | ✅ |
| markViewed | POST | /api/invoices/{id}/viewed | ✅ |
| updateStatus | PATCH | /api/invoices/{id}/status | ✅ |
| approve | POST | /api/invoices/{id}/approve | ✅ |
| void | POST | /api/invoices/{id}/void | ✅ |
| markPaid | POST | /api/invoices/{id}/mark-paid | ✅ |
| recordPayment | POST | /api/invoices/{id}/payments | ✅ |
| getBalance | GET | /api/invoices/{id}/balance | ✅ |
| getPayments | GET | /api/invoices/{id}/payments | ✅ |
| getOverdue | GET | /api/invoices/overdue | ✅ |
| getDue | GET | /api/invoices/due | ✅ |
| getCustomerStatistics | GET | /api/invoices/statistics/{customerId} | ✅ |
| addLineItem | POST | /api/invoices/{id}/line-items | ✅ |
| updateLineItem | PUT | /api/invoices/line-items/{lineItemId} | ✅ |
| removeLineItem | DELETE | /api/invoices/line-items/{lineItemId} | ✅ |
| getLineItems | GET | /api/invoices/{id}/line-items | ✅ |
| recalculateTotals | POST | /api/invoices/{id}/recalculate | ✅ |
| applyDiscount | POST | /api/invoices/{id}/discount | ✅ |

**Helper Functions:**
- `getInvoiceStatusLabel(status: InvoiceStatus): string` — Returns display label
- `getInvoiceStatusColor(status: InvoiceStatus): string` — Returns MUI color

### 2.3 Components

| Component | Status | Description |
|-----------|--------|-------------|
| InvoiceForm.tsx | ❌ Not Implemented | Create/Edit invoice form |
| InvoiceLineItemsTable.tsx | ❌ Not Implemented | Editable line items grid |
| InvoiceStatusBadge.tsx | ❌ Not Implemented | Status chip with color coding |
| InvoicePaymentHistory.tsx | ❌ Not Implemented | Payment records list |
| InvoiceSummary.tsx | ❌ Not Implemented | Pricing summary card |
| InvoiceAgingChart.tsx | ❌ Not Implemented | Overdue aging visualization |
| InvoiceActionButtons.tsx | ❌ Not Implemented | Context-aware action buttons |
| InvoicePdfPreview.tsx | ❌ Not Implemented | PDF preview and generation |

### 2.4 Frontend Validation Rules

| Field | Validation | Error Message |
|-------|------------|---------------|
| AccountId | Required | "Account is required" |
| InvoiceDate | Required | "Invoice date is required" |
| DueDate | Required, >= InvoiceDate | "Due date must be on or after invoice date" |
| InvoiceType | Required, valid enum | "Invoice type is required" |
| LineItems | Min 1 required | "At least one line item is required" |
| Quantity | > 0 | "Quantity must be greater than 0" |
| UnitPrice | >= 0 | "Unit price cannot be negative" |
| PaymentAmount | > 0, <= BalanceDue | "Payment amount must be between 0 and balance due" |
| VoidReason | Required when voiding | "Void reason is required" |

---

## 3. Backend Implementation

### 3.1 Entities

#### 3.1.1 Invoice Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs`  
**Status:** ✅ Implemented (~380 lines)

**Enumerations:**

**InvoiceStatus** (13 values):

| Value | Int | Description |
|-------|-----|-------------|
| Draft | 0 | Invoice created, not sent |
| PendingApproval | 1 | Awaiting approval |
| Approved | 2 | Approved for sending |
| Sent | 3 | Sent to customer |
| Viewed | 4 | Customer has viewed invoice |
| PartiallyPaid | 5 | Partial payment received |
| Paid | 6 | Fully paid |
| Overdue | 7 | Past due date |
| Void | 8 | Voided |
| Disputed | 9 | Customer disputes invoice |
| WrittenOff | 10 | Written off as bad debt |
| InCollections | 11 | Sent to collections |
| Refunded | 12 | Refunded to customer |

**InvoiceType** (9 values):

| Value | Int | Description |
|-------|-----|-------------|
| Standard | 0 | Standard invoice |
| Recurring | 1 | Recurring invoice |
| ProForma | 2 | Pro forma (preview) |
| CreditNote | 3 | Credit note |
| DebitNote | 4 | Debit note |
| Interim | 5 | Interim/progress invoice |
| Final | 6 | Final invoice |
| Deposit | 7 | Deposit invoice |
| DebitMemo | 8 | Debit memo |

**PaymentTerms** (11 values):

| Value | Int | Description |
|-------|-----|-------------|
| DueOnReceipt | 0 | Due immediately |
| Net15 | 1 | Net 15 days |
| Net30 | 2 | Net 30 days |
| Net45 | 3 | Net 45 days |
| Net60 | 4 | Net 60 days |
| Net90 | 5 | Net 90 days |
| TwoPercent10Net30 | 6 | 2/10 Net 30 |
| EndOfMonth | 7 | End of month |
| FifteenthOfMonth | 8 | 15th of month |
| DueOnDate | 9 | Due on specific date |
| Custom | 10 | Custom payment terms |

**Entity Properties (60+):**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **Identification** ||||
| Id | int | Yes | Primary key |
| InvoiceNumber | string | Yes | Unique invoice number (INV-YYMM-####) |
| ExternalInvoiceId | string | No | External system reference |
| ReferenceNumber | string | No | Customer reference number |
| **Invoice Details** ||||
| InvoiceType | InvoiceType | Yes | Type of invoice |
| Status | InvoiceStatus | Yes | Current invoice status |
| PaymentTerms | PaymentTerms | Yes | Payment terms |
| Description | string | No | Invoice description |
| Notes | string | No | Invoice notes |
| InternalNotes | string | No | Internal notes |
| Terms | string | No | Terms and conditions |
| FooterNotes | string | No | Footer text |
| **Date Fields** ||||
| InvoiceDate | DateTime | Yes | Invoice issue date |
| DueDate | DateTime | Yes | Payment due date |
| SentDate | DateTime? | No | When sent to customer |
| ViewedDate | DateTime? | No | When customer viewed |
| PaidDate | DateTime? | No | When fully paid |
| VoidDate | DateTime? | No | When voided |
| ApprovedDate | DateTime? | No | When approved |
| **Amount Fields (12)** ||||
| Subtotal | decimal | Yes | Sum of line items |
| DiscountAmount | decimal | Yes | Total discount |
| DiscountPercent | decimal | Yes | Discount percentage |
| TaxAmount | decimal | Yes | Total tax |
| TaxPercent | decimal | Yes | Tax percentage |
| ShippingAmount | decimal | Yes | Shipping charges |
| AdjustmentAmount | decimal | Yes | Manual adjustment |
| TotalAmount | decimal | Yes | Final invoice total |
| PaidAmount | decimal | Yes | Total amount paid |
| CreditApplied | decimal | Yes | Credits applied |
| WriteOffAmount | decimal | Yes | Written off amount |
| Currency | string | Yes | Currency code (default: USD) |
| **Computed Properties** ||||
| BalanceDue | decimal | — | TotalAmount - PaidAmount - CreditApplied |
| IsPaid | bool | — | BalanceDue <= 0 && TotalAmount > 0 |
| DaysOverdue | int | — | (DateTime.UtcNow - DueDate).Days when overdue |
| **Early Payment Discount** ||||
| EarlyPaymentDiscountPercent | decimal | Yes | Discount % for early pay |
| EarlyPaymentDiscountDate | DateTime? | No | Deadline for early discount |
| EarlyPaymentDiscountAmount | decimal | Yes | Discount amount for early pay |
| **Late Fees** ||||
| LateFeePercent | decimal | Yes | Late fee percentage |
| LateFeeAmount | decimal | Yes | Late fee amount |
| LateFeeApplied | bool | Yes | Whether late fee has been applied |
| LateFeeDate | DateTime? | No | When late fee was applied |
| **Billing Address (9 fields)** ||||
| BillingStreet | string | No | Street address |
| BillingCity | string | No | City |
| BillingState | string | No | State/Province |
| BillingPostalCode | string | No | Postal code |
| BillingCountry | string | No | Country |
| BillingContactName | string | No | Contact name |
| BillingContactEmail | string | No | Contact email |
| BillingContactPhone | string | No | Contact phone |
| BillingNotes | string | No | Billing notes |
| **Dunning & Collections** ||||
| DunningLevel | int | Yes | Current dunning level (0-4) |
| LastDunningDate | DateTime? | No | Last dunning notice date |
| DunningNotes | string | No | Dunning notes |
| CollectionAgency | string | No | Collection agency name |
| CollectionDate | DateTime? | No | When sent to collections |
| DisputeReason | string | No | Reason for dispute |
| **Relationships** ||||
| AccountId | int? | No | Customer account FK |
| OrderId | int? | No | Source order FK |
| SubscriptionId | int? | No | Related subscription FK |
| ContactId | int? | No | Invoice contact FK |
| VoidedById | int? | No | Who voided FK |
| ApprovedById | int? | No | Who approved FK |
| OriginalInvoiceId | int? | No | Original invoice for credits |
| VoidReason | string | No | Reason for voiding |
| **Document Fields** ||||
| PdfUrl | string | No | Generated PDF URL |
| TemplateId | string | No | Template used |
| LogoUrl | string | No | Company logo |
| CompanyName | string | No | Company name on invoice |
| CompanyAddress | string | No | Company address on invoice |
| CompanyTaxId | string | No | Company tax ID |
| CompanyBankDetails | string | No | Bank details for payment |
| **Audit Fields** ||||
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |
| IsDeleted | bool | Yes | Soft delete flag |
| RowVersion | byte[] | No | Optimistic concurrency |

**Navigation Properties:**
- `Account`, `Order`, `Subscription`, `Contact`, `VoidedBy`, `ApprovedBy`, `OriginalInvoice`
- `CreditMemos` (ICollection), `LineItems` (ICollection), `Payments` (ICollection)

#### 3.1.2 InvoiceLineItem Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs` (same file, ~200 lines)  
**Status:** ✅ Implemented

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | int | Yes | Primary key |
| InvoiceId | int | Yes | Parent invoice FK |
| LineNumber | int | Yes | Line sequence |
| ProductId | int? | No | Product reference |
| ProductCode | string | No | Product code |
| ProductName | string | Yes | Product name |
| Description | string | No | Line description |
| Quantity | decimal | Yes | Billed quantity |
| UnitOfMeasure | string | No | Unit of measure |
| UnitPrice | decimal | Yes | Price per unit |
| ListPrice | decimal | Yes | List price |
| DiscountAmount | decimal | Yes | Line discount |
| DiscountPercent | decimal | Yes | Line discount % |
| TaxAmount | decimal | Yes | Line tax |
| TaxPercent | decimal | Yes | Line tax % |
| ExtendedPrice | decimal | Yes | Quantity × UnitPrice |
| TotalPrice | decimal | Yes | Final line total |
| CostPrice | decimal | Yes | Cost per unit |
| TotalCost | decimal | Yes | Total cost |
| ServicePeriodStart | DateTime? | No | Service period start |
| ServicePeriodEnd | DateTime? | No | Service period end |
| RevenueAccount | string | No | Revenue GL account |
| RevenueRecognized | bool | Yes | Revenue recognized flag |
| RevenueRecognitionDate | DateTime? | No | Recognition date |
| OrderLineItemId | int? | No | Source order line item |
| Notes | string | No | Line notes |
| SortOrder | int | Yes | Display order |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |
| IsDeleted | bool | Yes | Soft delete flag |

### 3.2 DTOs

| DTO | Location | Status |
|-----|----------|--------|
| InvoiceDto | CRM.Core/DTOs/ | ❌ Not Implemented |
| CreateInvoiceDto | CRM.Core/DTOs/ | ❌ Not Implemented |
| UpdateInvoiceDto | CRM.Core/DTOs/ | ❌ Not Implemented |
| InvoiceLineItemDto | CRM.Core/DTOs/ | ❌ Not Implemented |

**Inline Supporting Types (defined in IInvoiceService.cs):**

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

### 3.3 Interfaces

#### 3.3.1 IInvoiceService
**File:** `CRM.Backend/src/CRM.Core/Interfaces/IInvoiceService.cs`  
**Status:** ✅ Implemented (~120 lines, 26 methods)

| Category | Method | Return Type |
|----------|--------|-------------|
| **CRUD** |||
| | GetAllAsync(customerId?, status?, ct) | Task\<IEnumerable\<Invoice>> |
| | GetByIdAsync(id, ct) | Task\<Invoice?> |
| | GetByInvoiceNumberAsync(invoiceNumber, ct) | Task\<Invoice?> |
| | CreateAsync(invoice, ct) | Task\<Invoice> |
| | UpdateAsync(invoice, ct) | Task\<Invoice> |
| | DeleteAsync(id, ct) | Task\<bool> |
| **Operations** |||
| | CreateFromOrderAsync(orderId, ct) | Task\<Invoice> |
| | CreateFromQuoteAsync(quoteId, ct) | Task\<Invoice> |
| | GenerateInvoiceNumberAsync(ct) | Task\<string> |
| | SendInvoiceAsync(invoiceId, recipientEmail?, ct) | Task\<bool> |
| | MarkAsViewedAsync(invoiceId, ct) | Task\<Invoice> |
| **Status** |||
| | UpdateStatusAsync(invoiceId, status, ct) | Task\<Invoice> |
| | ApproveAsync(invoiceId, approvedById, ct) | Task\<Invoice> |
| | VoidAsync(invoiceId, reason, ct) | Task\<Invoice> |
| | MarkAsPaidAsync(invoiceId, ct) | Task\<Invoice> |
| **Payments** |||
| | RecordPaymentAsync(invoiceId, amount, method, ct) | Task\<Payment> |
| | GetOutstandingBalanceAsync(invoiceId, ct) | Task\<decimal> |
| | GetPaymentsAsync(invoiceId, ct) | Task\<IEnumerable\<Payment>> |
| **Queries** |||
| | GetOverdueInvoicesAsync(ct) | Task\<IEnumerable\<Invoice>> |
| | GetInvoicesDueAsync(withinDays, ct) | Task\<IEnumerable\<Invoice>> |
| | GetCustomerStatisticsAsync(customerId, ct) | Task\<InvoiceStatistics> |
| **Line Items** |||
| | AddLineItemAsync(invoiceId, lineItem, ct) | Task\<InvoiceLineItem> |
| | UpdateLineItemAsync(lineItem, ct) | Task\<InvoiceLineItem> |
| | RemoveLineItemAsync(lineItemId, ct) | Task\<bool> |
| | GetLineItemsAsync(invoiceId, ct) | Task\<IEnumerable\<InvoiceLineItem>> |
| **Calculations** |||
| | RecalculateTotalsAsync(invoiceId, ct) | Task\<Invoice> |
| | ApplyDiscountAsync(invoiceId, amount, reason?, ct) | Task\<Invoice> |

### 3.4 Services

#### 3.4.1 InvoiceService
**File:** `CRM.Backend/src/CRM.Infrastructure/Services/InvoiceService.cs`  
**Status:** ✅ Implemented (~652 lines)

**Key Implementation Details:**

1. **Invoice Number Generation:**
   - Format: `INV-{YY}{MM}-{####}` (e.g., INV-2602-0001)
   - Sequence resets monthly
   - Thread-safe generation

2. **CreateFromOrderAsync:**
   - Copies order header (account, contact, billing address, currency)
   - Copies order line items with quantities and pricing
   - Links back to source order via OrderId

3. **CreateFromQuoteAsync:**
   - Copies quote header fields (account, contact, pricing, terms)
   - Copies quote line items
   - Links back to source quote (indirect through order)

4. **RecordPaymentAsync:**
   - Creates Payment entity linked to invoice
   - Updates PaidAmount on invoice
   - Auto-transitions status: PartiallyPaid → Paid when balance = 0

5. **RecalculateTotalsAsync:**
   - Subtotal = Sum of line item totals
   - TotalAmount = Subtotal - DiscountAmount + TaxAmount + ShippingAmount + AdjustmentAmount
   - BalanceDue = TotalAmount - PaidAmount - CreditApplied

6. **SendInvoiceAsync:**
   - Sets status to Sent
   - Records SentDate
   - Optional recipientEmail override

7. **VoidAsync:**
   - Sets status to Void
   - Records VoidDate, VoidReason, VoidedById
   - Only allowed for non-paid invoices

8. **Late Fee Calculation:**
   - Based on LateFeePercent of outstanding balance
   - Applied after DueDate passes
   - Tracks via LateFeeApplied and LateFeeDate

### 3.5 Controllers

#### 3.5.1 InvoicesController
**File:** `CRM.Backend/src/CRM.Api/Controllers/InvoicesController.cs`  
**Status:** ✅ Implemented (27 endpoints)

| Method | Route | Description | Status |
|--------|-------|-------------|--------|
| GET | /api/invoices | List invoices with filtering | ✅ |
| GET | /api/invoices/{id} | Get invoice by ID | ✅ |
| GET | /api/invoices/by-number/{invoiceNumber} | Get by invoice number | ✅ |
| POST | /api/invoices | Create invoice | ✅ |
| PUT | /api/invoices/{id} | Update invoice | ✅ |
| DELETE | /api/invoices/{id} | Soft delete invoice | ✅ |
| POST | /api/invoices/from-order/{orderId} | Create from order | ✅ |
| POST | /api/invoices/from-quote/{quoteId} | Create from quote | ✅ |
| GET | /api/invoices/next-number | Generate next invoice number | ✅ |
| POST | /api/invoices/{id}/send | Send invoice to customer | ✅ |
| POST | /api/invoices/{id}/viewed | Mark as viewed | ✅ |
| PATCH | /api/invoices/{id}/status | Update status | ✅ |
| POST | /api/invoices/{id}/approve | Approve invoice | ✅ |
| POST | /api/invoices/{id}/void | Void invoice | ✅ |
| POST | /api/invoices/{id}/mark-paid | Mark as paid | ✅ |
| POST | /api/invoices/{id}/payments | Record payment | ✅ |
| GET | /api/invoices/{id}/balance | Get outstanding balance | ✅ |
| GET | /api/invoices/{id}/payments | Get payments | ✅ |
| GET | /api/invoices/overdue | Get overdue invoices | ✅ |
| GET | /api/invoices/due | Get invoices due within N days | ✅ |
| GET | /api/invoices/statistics/{customerId} | Get customer statistics | ✅ |
| POST | /api/invoices/{id}/line-items | Add line item | ✅ |
| PUT | /api/invoices/line-items/{lineItemId} | Update line item | ✅ |
| DELETE | /api/invoices/line-items/{lineItemId} | Remove line item | ✅ |
| GET | /api/invoices/{id}/line-items | Get line items | ✅ |
| POST | /api/invoices/{id}/recalculate | Recalculate totals | ✅ |
| POST | /api/invoices/{id}/discount | Apply discount | ✅ |

### 3.6 Backend Validations

| Field | Rule | Status |
|-------|------|--------|
| InvoiceNumber | Required, Unique, Auto-generated | ✅ |
| InvoiceType | Required, Valid enum | ✅ |
| Status | Required, Valid enum, Valid transition | ✅ |
| InvoiceDate | Required | ✅ |
| DueDate | Required, >= InvoiceDate | ✅ |
| LineItems | At least one required | ✅ |
| TotalAmount | Calculated, non-negative | ✅ |
| PaymentAmount | > 0, <= BalanceDue when recording payment | ✅ |
| VoidReason | Required when voiding | ✅ |
| ApprovedById | Required when approving | ✅ |

---

## 4. Database

### 4.1 Tables

#### 4.1.1 Invoices Table
**Status:** ✅ Implemented via EF Core

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, Identity |
| InvoiceNumber | VARCHAR(50) | NOT NULL, UNIQUE |
| ExternalInvoiceId | VARCHAR(100) | NULL |
| ReferenceNumber | VARCHAR(100) | NULL |
| InvoiceType | INT | NOT NULL, DEFAULT 0 |
| Status | INT | NOT NULL, DEFAULT 0 |
| PaymentTerms | INT | NOT NULL, DEFAULT 2 |
| Description | TEXT | NULL |
| Notes | TEXT | NULL |
| InternalNotes | TEXT | NULL |
| InvoiceDate | DATETIME | NOT NULL |
| DueDate | DATETIME | NOT NULL |
| SentDate | DATETIME | NULL |
| ViewedDate | DATETIME | NULL |
| PaidDate | DATETIME | NULL |
| VoidDate | DATETIME | NULL |
| ApprovedDate | DATETIME | NULL |
| Subtotal | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| DiscountAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| DiscountPercent | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| TaxAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| TaxPercent | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| ShippingAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| AdjustmentAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| TotalAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| PaidAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| CreditApplied | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| WriteOffAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| Currency | VARCHAR(10) | NOT NULL, DEFAULT 'USD' |
| EarlyPaymentDiscountPercent | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| EarlyPaymentDiscountDate | DATETIME | NULL |
| EarlyPaymentDiscountAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| LateFeePercent | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| LateFeeAmount | DECIMAL(18,4) | NOT NULL, DEFAULT 0 |
| LateFeeApplied | BIT | NOT NULL, DEFAULT 0 |
| LateFeeDate | DATETIME | NULL |
| DunningLevel | INT | NOT NULL, DEFAULT 0 |
| LastDunningDate | DATETIME | NULL |
| AccountId | INT | FK → Customers.Id, NULL |
| OrderId | INT | FK → Orders.Id, NULL |
| SubscriptionId | INT | FK → Subscriptions.Id, NULL |
| ContactId | INT | FK → Contacts.Id, NULL |
| VoidedById | INT | FK → Users.Id, NULL |
| ApprovedById | INT | FK → Users.Id, NULL |
| OriginalInvoiceId | INT | FK → Invoices.Id, NULL |
| ... (billing address, document fields) ... |||
| CreatedAt | DATETIME | NOT NULL, DEFAULT NOW() |
| UpdatedAt | DATETIME | NULL |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 |
| RowVersion | BINARY(8) | NULL |

#### 4.1.2 InvoiceLineItems Table
**Status:** ✅ Implemented via EF Core

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, Identity |
| InvoiceId | INT | FK → Invoices.Id, NOT NULL |
| LineNumber | INT | NOT NULL |
| ProductId | INT | FK → Products.Id, NULL |
| ProductName | VARCHAR(200) | NOT NULL |
| Quantity | DECIMAL(18,4) | NOT NULL |
| UnitPrice | DECIMAL(18,4) | NOT NULL |
| ExtendedPrice | DECIMAL(18,4) | NOT NULL |
| TotalPrice | DECIMAL(18,4) | NOT NULL |
| ... (30+ columns) ... |||
| CreatedAt | DATETIME | NOT NULL, DEFAULT NOW() |
| UpdatedAt | DATETIME | NULL |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 |

### 4.2 Indexes

| Index | Table | Columns | Type |
|-------|-------|---------|------|
| IX_Invoices_InvoiceNumber | Invoices | InvoiceNumber | UNIQUE |
| IX_Invoices_AccountId | Invoices | AccountId | NON-UNIQUE |
| IX_Invoices_OrderId | Invoices | OrderId | NON-UNIQUE |
| IX_Invoices_Status | Invoices | Status | NON-UNIQUE |
| IX_Invoices_DueDate | Invoices | DueDate | NON-UNIQUE |
| IX_Invoices_InvoiceDate | Invoices | InvoiceDate | NON-UNIQUE |
| IX_InvoiceLineItems_InvoiceId | InvoiceLineItems | InvoiceId | NON-UNIQUE |
| IX_InvoiceLineItems_ProductId | InvoiceLineItems | ProductId | NON-UNIQUE |

### 4.3 Foreign Keys

| FK | From | To |
|----|------|-----|
| FK_Invoices_Accounts | Invoices.AccountId | Customers.Id |
| FK_Invoices_Orders | Invoices.OrderId | Orders.Id |
| FK_Invoices_Subscriptions | Invoices.SubscriptionId | Subscriptions.Id |
| FK_Invoices_Contacts | Invoices.ContactId | Contacts.Id |
| FK_Invoices_Users_Voider | Invoices.VoidedById | Users.Id |
| FK_Invoices_Users_Approver | Invoices.ApprovedById | Users.Id |
| FK_Invoices_OriginalInvoice | Invoices.OriginalInvoiceId | Invoices.Id |
| FK_InvoiceLineItems_Invoices | InvoiceLineItems.InvoiceId | Invoices.Id |
| FK_InvoiceLineItems_Products | InvoiceLineItems.ProductId | Products.Id |

---

## 5. Testing

### 5.1 Unit Tests

| Test | Description | Status |
|------|-------------|--------|
| GetAllAsync_ReturnsAllInvoices | Returns all non-deleted invoices | ❌ Not Implemented |
| GetByIdAsync_ExistingInvoice_ReturnsInvoice | Returns invoice by ID | ❌ Not Implemented |
| GetByInvoiceNumberAsync_ReturnsInvoice | Returns invoice by number | ❌ Not Implemented |
| CreateAsync_ValidInvoice_CreatesInvoice | Creates new invoice | ❌ Not Implemented |
| CreateFromOrderAsync_CopiesOrderData | Copies order to invoice | ❌ Not Implemented |
| RecordPaymentAsync_ValidPayment_UpdatesBalance | Records payment correctly | ❌ Not Implemented |
| RecordPaymentAsync_OverPayment_ThrowsException | Prevents overpayment | ❌ Not Implemented |
| MarkAsPaidAsync_ZeroBalance_SetsStatusPaid | Sets paid status when balance is 0 | ❌ Not Implemented |
| VoidAsync_NonPaidInvoice_Voids | Voids non-paid invoice | ❌ Not Implemented |
| VoidAsync_PaidInvoice_ThrowsException | Prevents voiding paid invoice | ❌ Not Implemented |
| RecalculateTotalsAsync_CorrectCalculation | Calculates totals correctly | ❌ Not Implemented |
| GetOverdueInvoicesAsync_ReturnsOverdue | Returns overdue invoices | ❌ Not Implemented |
| ApplyDiscountAsync_UpdatesTotals | Applies discount and recalculates | ❌ Not Implemented |

### 5.2 Integration Tests

| Test | Description | Status |
|------|-------------|--------|
| InvoicesController_GetAll_Returns200 | GET /api/invoices returns list | ❌ Not Implemented |
| InvoicesController_Create_Returns201 | POST /api/invoices creates invoice | ❌ Not Implemented |
| InvoicesController_CreateFromOrder_Returns201 | POST /api/invoices/from-order/{id} | ❌ Not Implemented |
| InvoicesController_RecordPayment_Returns200 | POST /api/invoices/{id}/payments | ❌ Not Implemented |

### 5.3 E2E Tests

| Test | Description | Status |
|------|-------------|--------|
| should display invoices list page | Navigate to /invoices | ❌ Not Implemented |
| should create invoice from order | Full create-from-order workflow | ❌ Not Implemented |
| should record payment on invoice | Payment recording workflow | ❌ Not Implemented |
| should void an invoice | Invoice voiding workflow | ❌ Not Implemented |

---

## 6. Issues & Gaps

### 6.1 Missing Components

| Component | Type | Priority | Impact |
|-----------|------|----------|--------|
| InvoiceDto / CreateInvoiceDto / UpdateInvoiceDto | Backend DTO | P2 | Controller passes entities directly — DTOs would improve API contract |
| InvoiceDetailsPage.tsx | Frontend Page | P2 | Cannot view full invoice details or record payments in UI |
| InvoiceForm.tsx | Frontend Component | P2 | No dedicated create/edit form component |
| InvoicePaymentHistory.tsx | Frontend Component | P2 | No payment history visualization |
| InvoicePdfPreview.tsx | Frontend Component | P3 | No PDF generation or preview |

### 6.2 Validation Gaps

| Field | Missing Validation | Recommendation |
|-------|-------------------|----------------|
| Currency | No ISO 4217 currency code validation | Validate against currency code list |
| BillingContactEmail | No email format validation | Add email regex validation |
| PaymentTerms | Custom terms not validated | Validate custom payment terms logic |
| LateFeePercent | No maximum cap | Add maximum late fee percentage |

### 6.3 Naming Inconsistencies

| Location | Issue | Recommendation |
|----------|-------|----------------|
| Frontend PaymentMethod enum | Has 17 values; not in backend Invoice.cs | Align — either add to backend or reference Payment entity |
| InvoiceStatistics class | Defined in IInvoiceService.cs inline | Consider separate DTO file |

---

## 7. TODOs

### High Priority

| ID | Task | Effort |
|----|------|--------|
| TODO-SALES003-001 | Create InvoiceDetailsPage.tsx with payment recording | 8 hrs |
| TODO-SALES003-002 | Create InvoiceDto, CreateInvoiceDto, UpdateInvoiceDto | 4 hrs |

### Medium Priority

| ID | Task | Effort |
|----|------|--------|
| TODO-SALES003-003 | Create InvoiceForm.tsx component | 4 hrs |
| TODO-SALES003-004 | Create InvoiceLineItemsTable.tsx component | 4 hrs |
| TODO-SALES003-005 | Create InvoiceStatusBadge.tsx component | 1 hr |
| TODO-SALES003-006 | Create InvoicePaymentHistory.tsx component | 3 hrs |
| TODO-SALES003-007 | Add currency code validation | 1 hr |
| TODO-SALES003-008 | Add email format validation for billing contact | 1 hr |
| TODO-SALES003-009 | Create InvoiceServiceTests.cs unit tests | 4 hrs |
| TODO-SALES003-010 | Implement PDF generation for invoices | 6 hrs |

### Low Priority

| ID | Task | Effort |
|----|------|--------|
| TODO-SALES003-011 | Create E2E tests for invoice workflows | 4 hrs |
| TODO-SALES003-012 | Implement automated dunning email sequence | 6 hrs |

---

## 8. Appendix

### A. Invoice Status Flow Diagram

```
                    ┌─────────────┐
                    │    Draft    │
                    └──────┬──────┘
                           │ submit/approve
                           ▼
                    ┌─────────────┐
                    │  Pending    │──approve──┐
                    │  Approval   │           │
                    └──────┬──────┘           │
                           │reject           │
                           ▼                 │
                    ┌─────────────┐           │
                    │    Draft    │           ▼
                    └─────────────┘     ┌─────────────┐
                                       │  Approved   │
                                       └──────┬──────┘
                                              │ send
                                              ▼
                                       ┌─────────────┐
                                       │    Sent     │
                                       └──────┬──────┘
                                              │ view
                                              ▼
                                       ┌─────────────┐
                                       │   Viewed    │
                                       └──────┬──────┘
                                              │ payment
                              ┌───────────────┼────────────────┐
                              ▼                                ▼
                       ┌─────────────┐                  ┌─────────────┐
                       │  Partially  │─────full pay────▶│    Paid     │
                       │    Paid     │                  └─────────────┘
                       └─────────────┘

    Sent/Viewed/PartiallyPaid + past due ──► Overdue ──► InCollections
    Non-paid invoices ──void──► Void
    Any invoice ──dispute──► Disputed
    Overdue ──write off──► WrittenOff
    Paid ──refund──► Refunded
```

### B. Invoice Number Format

```
INV-{YY}{MM}-{####}

INV   = Fixed prefix
YY    = 2-digit year
MM    = 2-digit month (01-12)
####  = 4-digit sequence (resets monthly)

Examples:
  INV-2602-0001  (First invoice of February 2026)
  INV-2602-0042  (42nd invoice of February 2026)
  INV-2603-0001  (First invoice of March 2026)
```

### C. Payment Terms Reference

| Term | Days | Description |
|------|------|-------------|
| Due on Receipt | 0 | Payment due immediately |
| Net 15 | 15 | Due within 15 days |
| Net 30 | 30 | Due within 30 days |
| Net 45 | 45 | Due within 45 days |
| Net 60 | 60 | Due within 60 days |
| Net 90 | 90 | Due within 90 days |
| 2/10 Net 30 | 30 | 2% discount if paid in 10 days, otherwise net 30 |
| End of Month | Varies | Due end of invoice month |
| 15th of Month | Varies | Due 15th of following month |
| Due on Date | Varies | Due on a specific date |
| Custom | Varies | Custom payment schedule |

### D. Related Specifications

| Spec ID | Name | Relationship |
|---------|------|--------------|
| [SPEC-SALES-002](SPEC-SALES-002-OrderManagement.md) | Order Management | Source for invoices |
| [SPEC-SALES-001](SPEC-SALES-001-QuoteManagement.md) | Quote Management | Source for invoices |
| [SPEC-CRM-001](SPEC-CRM-001-AccountManagement.md) | Account Management | Invoice customer |
| SPEC-SALES-004 | Payment Management | Payment processing |

### E. Change History

| Date | Version | Changes |
|------|---------|---------|
| February 2026 | 1.0 | Initial specification created |
| February 2026 | 2.0 | Updated statuses — InvoicesController ✅, invoiceService.ts ✅, InvoicesPage.tsx ✅ now implemented. Restructured to match SPEC-TEMPLATE format (Frontend = Section 2, Backend = Section 3). |
| February 14, 2026 | 2.1 | Marked as ✅ Complete. All Phase 4 service 11-specifications aligned. Updated module to "Sales & Billing". Added SPEC-SYS-008 dependency. All sub-features implemented or partial with clear status indicators. |

---

**END OF SPECIFICATION**
