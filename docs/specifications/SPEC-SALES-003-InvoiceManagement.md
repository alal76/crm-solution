# SPEC-SALES-003: Invoice Management

> **Version:** 1.0  
> **Created:** 2026-02-12  
> **Last Updated:** 2026-02-12  
> **Status:** ✅ Complete  
> **Module:** Sales  
> **Priority:** P1 - High  
> **Dependencies:** SPEC-CRM-001 (Account Management), SPEC-SALES-001 (Quote Management), SPEC-SALES-002 (Order Management)

---

## 1. Business Context

### 1.1 Overview
Invoice Management handles the billing lifecycle from invoice creation through payment collection. Invoices can be created from Orders or Quotes, support multiple payment terms, track partial payments, and manage dunning workflows for overdue accounts.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Invoice Creation | Create invoices manually or from orders/quotes | ✅ Implemented |
| SF-002 | Invoice Status Lifecycle | Draft → Sent → Paid flow | ✅ Implemented |
| SF-003 | Line Item Management | Add, update, remove invoice line items | ✅ Implemented |
| SF-004 | Payment Recording | Record full/partial payments | ✅ Implemented |
| SF-005 | Payment Terms | Net30, Net60, custom terms | ✅ Implemented |
| SF-006 | Dunning Management | Reminders, late fees, collections | ✅ Entity Support |
| SF-007 | Invoice Approval | Approval workflow for invoices | ✅ Implemented |
| SF-008 | Void & Credit | Void invoices, create credit memos | ✅ Implemented |

### 1.3 Key Functionalities

| Functionality | Description | Implementation Status |
|---------------|-------------|----------------------|
| Create Invoice | Create new invoice from scratch or from order/quote | ✅ Backend Service |
| Invoice Numbering | Auto-generate unique invoice numbers (INV-YYMM-0001) | ✅ Backend Service |
| Status Transitions | Manage invoice status workflow | ✅ Backend Service |
| Payment Processing | Record payments, track balances | ✅ Backend Service |
| Overdue Tracking | Identify and report overdue invoices | ✅ Backend Service |
| Customer Statistics | Invoice/payment statistics per account | ✅ Backend Service |

### 1.4 Use Cases

| UC-ID | Use Case | Actor | Description |
|-------|----------|-------|-------------|
| UC-001 | Create Invoice from Order | Finance | Convert fulfilled order to invoice |
| UC-002 | Record Payment | Finance | Record customer payment against invoice |
| UC-003 | Track Overdue Invoices | Finance | View list of overdue invoices |
| UC-004 | Void Invoice | Finance Manager | Cancel an invoice |
| UC-005 | Send Invoice | Finance | Email invoice to customer |
| UC-006 | Apply Discount | Sales Manager | Apply discount to invoice |

---

## 2. Frontend Specification

### 2.1 Pages

| Page | Route | Component | Status |
|------|-------|-----------|--------|
| Invoices List | /invoices | InvoicesPage.tsx | ❌ Not Implemented |
| Invoice Details | /invoices/:id | InvoiceDetailsPage.tsx | ❌ Not Implemented |
| Create Invoice | /invoices/new | CreateInvoicePage.tsx | ❌ Not Implemented |
| Edit Invoice | /invoices/:id/edit | EditInvoicePage.tsx | ❌ Not Implemented |

### 2.2 Components

| Component | Location | Description | Status |
|-----------|----------|-------------|--------|
| InvoiceDataGrid | components/invoices/ | Grid display with filtering | ❌ Not Implemented |
| InvoiceForm | components/invoices/ | Create/edit form | ❌ Not Implemented |
| InvoiceLineItemsEditor | components/invoices/ | Line item management | ❌ Not Implemented |
| InvoiceStatusBadge | components/invoices/ | Status indicator | ❌ Not Implemented |
| InvoicePaymentPanel | components/invoices/ | Payment recording | ❌ Not Implemented |
| InvoicePdfPreview | components/invoices/ | PDF preview/download | ❌ Not Implemented |
| InvoiceTimeline | components/invoices/ | Status/payment history | ❌ Not Implemented |
| InvoiceStatisticsCard | components/invoices/ | Summary statistics | ❌ Not Implemented |

### 2.3 Services

| Service | File | Methods | Status |
|---------|------|---------|--------|
| invoiceService | services/invoiceService.ts | All CRUD + operations | ❌ Not Implemented |

#### Expected Service Methods
```typescript
// invoiceService.ts
interface InvoiceService {
  // CRUD
  getAll(params?: InvoiceQueryParams): Promise<Invoice[]>;
  getById(id: number): Promise<Invoice>;
  getByInvoiceNumber(invoiceNumber: string): Promise<Invoice>;
  create(invoice: CreateInvoiceDto): Promise<Invoice>;
  update(id: number, invoice: UpdateInvoiceDto): Promise<Invoice>;
  delete(id: number): Promise<void>;
  
  // Operations
  createFromOrder(orderId: number): Promise<Invoice>;
  createFromQuote(quoteId: number): Promise<Invoice>;
  send(id: number): Promise<void>;
  markAsViewed(id: number): Promise<void>;
  
  // Status
  updateStatus(id: number, status: InvoiceStatus): Promise<Invoice>;
  approve(id: number): Promise<Invoice>;
  void(id: number, reason: string): Promise<Invoice>;
  markAsPaid(id: number): Promise<Invoice>;
  
  // Payments
  recordPayment(id: number, amount: number, method: PaymentMethod): Promise<Invoice>;
  getPayments(id: number): Promise<Payment[]>;
  getOutstandingBalance(id: number): Promise<number>;
  
  // Queries
  getOverdue(daysPastDue?: number): Promise<Invoice[]>;
  getDueInRange(fromDate: Date, toDate: Date): Promise<Invoice[]>;
  getCustomerStatistics(customerId: number): Promise<InvoiceStatistics>;
  
  // Line Items
  addLineItem(invoiceId: number, lineItem: InvoiceLineItem): Promise<InvoiceLineItem>;
  updateLineItem(lineItem: InvoiceLineItem): Promise<InvoiceLineItem>;
  removeLineItem(lineItemId: number): Promise<void>;
  getLineItems(invoiceId: number): Promise<InvoiceLineItem[]>;
  
  // Calculations
  recalculateTotals(id: number): Promise<Invoice>;
  applyDiscount(id: number, amount: number, code?: string): Promise<Invoice>;
}
```

### 2.4 Frontend Validation Rules

| Field | Validation | Error Message |
|-------|------------|---------------|
| AccountId | Required | "Account is required" |
| DueDate | Required, >= InvoiceDate | "Due date must be on or after invoice date" |
| LineItems | Min 1 required | "At least one line item is required" |
| Quantity | > 0 | "Quantity must be greater than 0" |
| UnitPrice | >= 0 | "Unit price cannot be negative" |
| PaymentAmount | > 0, <= BalanceDue | "Payment amount must be between 0 and balance due" |

---

## 3. Backend Specification

### 3.1 Entities

#### 3.1.1 Invoice Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs`  
**Status:** ✅ Implemented (582 lines)

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **Identification** |
| InvoiceNumber | string | ✅ | System-generated unique number (INV-YYMM-0001) |
| ExternalInvoiceId | string? | | External reference number |
| ReferenceNumber | string? | | Related document reference |
| BatchNumber | string? | | Batch processing number |
| **Invoice Details** |
| Description | string? | | Invoice memo/description |
| Status | InvoiceStatus | ✅ | Current lifecycle status |
| InvoiceType | InvoiceType | ✅ | Standard, Credit, Proforma, etc. |
| PaymentTerms | PaymentTerms | ✅ | Net30, Net60, etc. |
| PaymentTermsDescription | string? | | Custom terms text |
| **Dates** |
| InvoiceDate | DateTime | ✅ | Creation date |
| DueDate | DateTime | ✅ | Payment due date |
| SentDate | DateTime? | | Date sent to customer |
| ViewedDate | DateTime? | | Date customer viewed |
| PaidDate | DateTime? | | Date fully paid |
| VoidedDate | DateTime? | | Date voided |
| ServicePeriodStart | DateTime? | | Service period start |
| ServicePeriodEnd | DateTime? | | Service period end |
| **Amounts** |
| Subtotal | decimal | ✅ | Line items total before adjustments |
| DiscountAmount | decimal | | Total discount |
| DiscountPercent | decimal | | Discount percentage |
| TaxAmount | decimal | | Total tax |
| TaxRate | decimal | | Tax rate percentage |
| ShippingAmount | decimal | | Shipping/freight charges |
| FeesAmount | decimal | | Additional fees |
| TotalAmount | decimal | ✅ | Final invoice total |
| AmountPaid | decimal | | Total payments received |
| AmountCredited | decimal | | Credits applied |
| BalanceDue | decimal | | Calculated: Total - Paid - Credited |
| IsPaid | bool | | Calculated: BalanceDue <= 0 |
| CurrencyCode | string | ✅ | ISO 4217 currency code |
| ExchangeRate | decimal? | | Foreign currency exchange rate |
| **Early Payment Discount** |
| EarlyPaymentDiscountPercent | decimal? | | Early payment discount % |
| EarlyPaymentDiscountDays | int? | | Days for early discount |
| EarlyPaymentDiscountAmount | decimal? | | Early discount amount |
| **Late Fees** |
| LateFeePercent | decimal? | | Late fee percentage |
| LateFeeAmount | decimal? | | Flat late fee amount |
| LateFeeTotal | decimal | | Accrued late fees |
| DaysOverdue | int | | Calculated days past due |
| **Billing Address** |
| BillingName | string? | | Billing contact name |
| BillingCompany | string? | | Company name |
| BillingStreet | string? | | Street address |
| BillingCity | string? | | City |
| BillingState | string? | | State/Province |
| BillingPostalCode | string? | | ZIP/Postal code |
| BillingCountry | string? | | Country |
| BillingEmail | string? | | Billing email |
| BillingPhone | string? | | Billing phone |
| **Dunning & Collections** |
| ReminderCount | int | | Number of reminders sent |
| LastReminderDate | DateTime? | | Last reminder date |
| NextReminderDate | DateTime? | | Scheduled next reminder |
| InCollections | bool | | Whether in collections |
| CollectionsDate | DateTime? | | Date sent to collections |
| CollectionsReference | string? | | Collections agency reference |
| **Relationships** |
| AccountId | int | ✅ | Customer account FK |
| Account | Account? | | Navigation property |
| OrderId | int? | | Related order FK |
| Order | Order? | | Navigation property |
| SubscriptionId | int? | | Related subscription FK |
| Subscription | Subscription? | | Navigation property |
| ContactId | int? | | Primary contact FK |
| Contact | Contact? | | Navigation property |
| VoidedById | int? | | User who voided FK |
| VoidedBy | User? | | Navigation property |
| OriginalInvoiceId | int? | | Original invoice (for credit memos) |
| OriginalInvoice | Invoice? | | Navigation property |
| CreditMemos | ICollection<Invoice> | | Related credit memos |
| LineItems | ICollection<InvoiceLineItem> | | Invoice line items |
| Payments | ICollection<Payment> | | Received payments |
| **Notes & Documents** |
| Notes | string? | | Customer-visible notes |
| InternalNotes | string? | | Internal notes |
| Footer | string? | | Invoice footer text |
| TermsAndConditions | string? | | Terms and conditions |
| VoidReason | string? | | Reason for voiding |
| DisputeReason | string? | | Dispute reason |
| PdfUrl | string? | | Generated PDF URL |

#### 3.1.2 InvoiceLineItem Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs` (same file, lines 440-582)  
**Status:** ✅ Implemented

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **Identification** |
| LineNumber | int | ✅ | Display order number |
| ExternalLineId | string? | | External reference |
| **Product Details** |
| Name | string | ✅ | Item name |
| Description | string? | | Detailed description |
| SKU | string? | | Product SKU |
| ProductCode | string? | | Product code |
| **Quantity & Pricing** |
| Quantity | decimal | ✅ | Quantity billed |
| UnitOfMeasure | string? | | Unit of measure |
| UnitPrice | decimal | ✅ | Price per unit |
| DiscountAmount | decimal | | Line discount |
| DiscountPercent | decimal | | Discount percentage |
| ExtendedAmount | decimal | | Quantity × Price - Discount |
| TaxAmount | decimal | | Line tax amount |
| TaxRate | decimal? | | Line tax rate |
| TotalAmount | decimal | | Line total including tax |
| **Service Period** |
| ServiceStartDate | DateTime? | | Service period start |
| ServiceEndDate | DateTime? | | Service period end |
| **Revenue Recognition** |
| RevenueRecognitionStartDate | DateTime? | | Rev rec start |
| RevenueRecognitionEndDate | DateTime? | | Rev rec end |
| DeferredRevenue | decimal? | | Deferred amount |
| RecognizedRevenue | decimal? | | Recognized amount |
| **Relationships** |
| InvoiceId | int | ✅ | Parent invoice FK |
| Invoice | Invoice? | | Navigation property |
| ProductId | int? | | Product FK |
| Product | Product? | | Navigation property |
| OrderLineItemId | int? | | Source order line FK |
| OrderLineItem | OrderLineItem? | | Navigation property |
| SubscriptionId | int? | | Related subscription FK |
| Subscription | Subscription? | | Navigation property |
| **Notes** |
| Notes | string? | | Line item notes |

### 3.2 Enumerations

#### 3.2.1 InvoiceStatus
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs`  
**Values:** 13

| Value | Int | Description |
|-------|-----|-------------|
| Draft | 0 | Invoice created, not finalized |
| PendingApproval | 1 | Awaiting approval |
| Approved | 2 | Approved, ready to send |
| Sent | 3 | Sent to customer |
| Viewed | 4 | Viewed by customer |
| PartiallyPaid | 5 | Partial payment received |
| Paid | 6 | Fully paid |
| Overdue | 7 | Past due date |
| Disputed | 8 | Customer disputed |
| Voided | 9 | Cancelled/voided |
| WrittenOff | 10 | Written off as bad debt |
| Collections | 11 | Sent to collections |
| Refunded | 12 | Refunded to customer |

#### 3.2.2 InvoiceType
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs`  
**Values:** 9

| Value | Int | Description |
|-------|-----|-------------|
| Standard | 0 | Standard goods/services invoice |
| Credit | 1 | Credit memo/note |
| Proforma | 2 | Proforma/estimate |
| Recurring | 3 | Recurring subscription invoice |
| Deposit | 4 | Deposit/advance invoice |
| Progress | 5 | Progress billing |
| Final | 6 | Final invoice |
| Adjustment | 7 | Adjustment invoice |
| DebitMemo | 8 | Debit memo |

#### 3.2.3 PaymentTerms
**File:** `CRM.Backend/src/CRM.Core/Entities/Invoice.cs`  
**Values:** 11

| Value | Int | Description |
|-------|-----|-------------|
| DueOnReceipt | 0 | Due upon receipt |
| Net7 | 1 | Net 7 days |
| Net10 | 2 | Net 10 days |
| Net15 | 3 | Net 15 days |
| Net30 | 4 | Net 30 days |
| Net45 | 5 | Net 45 days |
| Net60 | 6 | Net 60 days |
| Net90 | 7 | Net 90 days |
| TwoTenNet30 | 8 | 2% 10 Net 30 |
| EndOfMonth | 9 | End of month |
| Custom | 10 | Custom terms |

### 3.3 DTOs

| DTO | Location | Status |
|-----|----------|--------|
| InvoiceDto | CRM.Core/DTOs/InvoiceDto.cs | ❌ Not Found |
| CreateInvoiceDto | CRM.Core/DTOs/InvoiceDto.cs | ❌ Not Found |
| UpdateInvoiceDto | CRM.Core/DTOs/InvoiceDto.cs | ❌ Not Found |
| InvoiceLineItemDto | CRM.Core/DTOs/InvoiceDto.cs | ❌ Not Found |
| InvoiceStatisticsDto | Embedded in IInvoiceService | ✅ Implemented |

### 3.4 Interfaces

#### 3.4.1 IInvoiceService
**File:** `CRM.Backend/src/CRM.Core/Interfaces/IInvoiceService.cs`  
**Status:** ✅ Implemented  
**Methods:** 21

| Region | Method | Parameters | Return Type |
|--------|--------|------------|-------------|
| **CRUD** |
| | GetAllAsync | customerId?, status?, cancellationToken | Task<IEnumerable<Invoice>> |
| | GetByIdAsync | id, cancellationToken | Task<Invoice?> |
| | GetByInvoiceNumberAsync | invoiceNumber, cancellationToken | Task<Invoice?> |
| | CreateAsync | invoice, cancellationToken | Task<Invoice> |
| | UpdateAsync | invoice, cancellationToken | Task<Invoice> |
| | DeleteAsync | id, cancellationToken | Task<bool> |
| **Operations** |
| | CreateFromOrderAsync | orderId, cancellationToken | Task<Invoice> |
| | CreateFromQuoteAsync | quoteId, cancellationToken | Task<Invoice> |
| | GenerateInvoiceNumberAsync | cancellationToken | Task<string> |
| | SendInvoiceAsync | invoiceId, cancellationToken | Task<bool> |
| | MarkAsViewedAsync | invoiceId, cancellationToken | Task<bool> |
| **Status** |
| | UpdateStatusAsync | invoiceId, status, cancellationToken | Task<Invoice> |
| | ApproveAsync | invoiceId, approvedById, cancellationToken | Task<Invoice> |
| | VoidAsync | invoiceId, reason, cancellationToken | Task<Invoice> |
| | MarkAsPaidAsync | invoiceId, cancellationToken | Task<Invoice> |
| **Payments** |
| | RecordPaymentAsync | invoiceId, amount, method, cancellationToken | Task<Invoice> |
| | GetOutstandingBalanceAsync | invoiceId, cancellationToken | Task<decimal> |
| | GetPaymentsAsync | invoiceId, cancellationToken | Task<IEnumerable<Payment>> |
| **Queries** |
| | GetOverdueInvoicesAsync | daysPastDue?, cancellationToken | Task<IEnumerable<Invoice>> |
| | GetInvoicesDueAsync | fromDate, toDate, cancellationToken | Task<IEnumerable<Invoice>> |
| | GetCustomerStatisticsAsync | customerId, cancellationToken | Task<InvoiceStatistics> |
| **Line Items** |
| | AddLineItemAsync | invoiceId, lineItem, cancellationToken | Task<InvoiceLineItem> |
| | UpdateLineItemAsync | lineItem, cancellationToken | Task<InvoiceLineItem> |
| | RemoveLineItemAsync | lineItemId, cancellationToken | Task<bool> |
| | GetLineItemsAsync | invoiceId, cancellationToken | Task<IEnumerable<InvoiceLineItem>> |
| **Calculations** |
| | RecalculateTotalsAsync | invoiceId, cancellationToken | Task<Invoice> |
| | ApplyDiscountAsync | invoiceId, discountAmount, discountCode?, cancellationToken | Task<Invoice> |

### 3.5 Services

#### 3.5.1 InvoiceService
**File:** `CRM.Backend/src/CRM.Infrastructure/Services/InvoiceService.cs`  
**Status:** ✅ Implemented (652 lines)

**Key Implementation Details:**

1. **Invoice Number Generation:**
   - Format: `INV-YYMM-NNNN` (e.g., INV-2602-0001)
   - Auto-increments within month
   - Uses database query to find last sequence

2. **Create from Order:**
   - Copies line items from Order to Invoice
   - Links OrderLineItemId to InvoiceLineItem
   - Sets AccountId, amounts from Order

3. **Create from Quote:**
   - Copies line items from QuoteLineItems
   - Auto-assigns line numbers
   - Sets default payment terms

4. **Payment Recording:**
   - Creates Payment entity
   - Updates AmountPaid on Invoice
   - Auto-transitions to PartiallyPaid or Paid status

5. **Total Recalculation:**
   - Sums line items for Subtotal
   - Applies discounts, tax, shipping, fees
   - Updates TotalAmount

6. **Customer Statistics:**
   - Aggregates invoice counts and amounts
   - Calculates overdue count
   - Computes average days to payment

### 3.6 Controllers

| Controller | File | Status |
|------------|------|--------|
| InvoicesController | CRM.Api/Controllers/InvoicesController.cs | ❌ Not Implemented |

#### Expected Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/invoices | Get all invoices with filtering |
| GET | /api/invoices/{id} | Get invoice by ID |
| GET | /api/invoices/number/{invoiceNumber} | Get by invoice number |
| POST | /api/invoices | Create new invoice |
| PUT | /api/invoices/{id} | Update invoice |
| DELETE | /api/invoices/{id} | Delete invoice (soft) |
| POST | /api/invoices/from-order/{orderId} | Create from order |
| POST | /api/invoices/from-quote/{quoteId} | Create from quote |
| POST | /api/invoices/{id}/send | Send invoice to customer |
| POST | /api/invoices/{id}/viewed | Mark as viewed |
| PUT | /api/invoices/{id}/status | Update status |
| POST | /api/invoices/{id}/approve | Approve invoice |
| POST | /api/invoices/{id}/void | Void invoice |
| POST | /api/invoices/{id}/mark-paid | Mark as paid |
| POST | /api/invoices/{id}/payments | Record payment |
| GET | /api/invoices/{id}/payments | Get payments |
| GET | /api/invoices/{id}/balance | Get outstanding balance |
| GET | /api/invoices/overdue | Get overdue invoices |
| GET | /api/invoices/due | Get invoices due in range |
| GET | /api/invoices/customer/{customerId}/statistics | Get customer statistics |
| POST | /api/invoices/{id}/line-items | Add line item |
| PUT | /api/invoices/line-items/{id} | Update line item |
| DELETE | /api/invoices/line-items/{id} | Remove line item |
| GET | /api/invoices/{id}/line-items | Get line items |
| POST | /api/invoices/{id}/recalculate | Recalculate totals |
| POST | /api/invoices/{id}/discount | Apply discount |

### 3.7 Backend Validation

| Entity | Field | Validation | Message |
|--------|-------|------------|---------|
| Invoice | AccountId | Required | "Account is required" |
| Invoice | InvoiceDate | Required | "Invoice date is required" |
| Invoice | DueDate | Required, >= InvoiceDate | "Due date must be on or after invoice date" |
| Invoice | TotalAmount | >= 0 | "Total amount cannot be negative" |
| InvoiceLineItem | Name | Required | "Line item name is required" |
| InvoiceLineItem | Quantity | > 0 | "Quantity must be greater than 0" |
| InvoiceLineItem | UnitPrice | >= 0 | "Unit price cannot be negative" |

---

## 4. Database Specification

### 4.1 Tables

#### 4.1.1 Invoices Table
**Table Name:** `Invoices`

| Column | Data Type | Nullable | Default | Constraints |
|--------|-----------|----------|---------|-------------|
| Id | int | NO | AUTO_INCREMENT | PK |
| InvoiceNumber | varchar(50) | NO | | UNIQUE |
| ExternalInvoiceId | varchar(100) | YES | | |
| ReferenceNumber | varchar(100) | YES | | |
| BatchNumber | varchar(50) | YES | | |
| Description | varchar(1000) | YES | | |
| Status | int | NO | 0 | |
| InvoiceType | int | NO | 0 | |
| PaymentTerms | int | NO | 4 | |
| PaymentTermsDescription | varchar(500) | YES | | |
| InvoiceDate | datetime | NO | CURRENT_TIMESTAMP | |
| DueDate | datetime | NO | | |
| SentDate | datetime | YES | | |
| ViewedDate | datetime | YES | | |
| PaidDate | datetime | YES | | |
| VoidedDate | datetime | YES | | |
| ServicePeriodStart | datetime | YES | | |
| ServicePeriodEnd | datetime | YES | | |
| Subtotal | decimal(18,2) | NO | 0 | |
| DiscountAmount | decimal(18,2) | NO | 0 | |
| DiscountPercent | decimal(5,2) | NO | 0 | |
| TaxAmount | decimal(18,2) | NO | 0 | |
| TaxRate | decimal(5,2) | NO | 0 | |
| ShippingAmount | decimal(18,2) | NO | 0 | |
| FeesAmount | decimal(18,2) | NO | 0 | |
| TotalAmount | decimal(18,2) | NO | 0 | |
| AmountPaid | decimal(18,2) | NO | 0 | |
| AmountCredited | decimal(18,2) | NO | 0 | |
| CurrencyCode | varchar(3) | NO | 'USD' | |
| ExchangeRate | decimal(18,6) | YES | | |
| EarlyPaymentDiscountPercent | decimal(5,2) | YES | | |
| EarlyPaymentDiscountDays | int | YES | | |
| EarlyPaymentDiscountAmount | decimal(18,2) | YES | | |
| LateFeePercent | decimal(5,2) | YES | | |
| LateFeeAmount | decimal(18,2) | YES | | |
| LateFeeTotal | decimal(18,2) | NO | 0 | |
| BillingName | varchar(255) | YES | | |
| BillingCompany | varchar(255) | YES | | |
| BillingStreet | varchar(500) | YES | | |
| BillingCity | varchar(100) | YES | | |
| BillingState | varchar(100) | YES | | |
| BillingPostalCode | varchar(20) | YES | | |
| BillingCountry | varchar(100) | YES | | |
| BillingEmail | varchar(255) | YES | | |
| BillingPhone | varchar(30) | YES | | |
| ReminderCount | int | NO | 0 | |
| LastReminderDate | datetime | YES | | |
| NextReminderDate | datetime | YES | | |
| InCollections | bit | NO | 0 | |
| CollectionsDate | datetime | YES | | |
| CollectionsReference | varchar(100) | YES | | |
| AccountId | int | NO | | FK → Accounts |
| OrderId | int | YES | | FK → Orders |
| SubscriptionId | int | YES | | FK → Subscriptions |
| ContactId | int | YES | | FK → Contacts |
| VoidedById | int | YES | | FK → Users |
| OriginalInvoiceId | int | YES | | FK → Invoices |
| Notes | nvarchar(2000) | YES | | |
| InternalNotes | nvarchar(2000) | YES | | |
| Footer | varchar(1000) | YES | | |
| TermsAndConditions | nvarchar(5000) | YES | | |
| VoidReason | varchar(500) | YES | | |
| DisputeReason | varchar(500) | YES | | |
| PdfUrl | varchar(500) | YES | | |
| CreatedAt | datetime | NO | CURRENT_TIMESTAMP | |
| UpdatedAt | datetime | YES | | |
| IsDeleted | bit | NO | 0 | |
| RowVersion | timestamp | NO | | |

#### 4.1.2 InvoiceLineItems Table
**Table Name:** `InvoiceLineItems`

| Column | Data Type | Nullable | Default | Constraints |
|--------|-----------|----------|---------|-------------|
| Id | int | NO | AUTO_INCREMENT | PK |
| LineNumber | int | NO | | |
| ExternalLineId | varchar(100) | YES | | |
| Name | varchar(255) | NO | | |
| Description | varchar(1000) | YES | | |
| SKU | varchar(50) | YES | | |
| ProductCode | varchar(50) | YES | | |
| Quantity | decimal(18,4) | NO | 1 | |
| UnitOfMeasure | varchar(50) | YES | | |
| UnitPrice | decimal(18,4) | NO | 0 | |
| DiscountAmount | decimal(18,2) | NO | 0 | |
| DiscountPercent | decimal(5,2) | NO | 0 | |
| ExtendedAmount | decimal(18,2) | NO | 0 | |
| TaxAmount | decimal(18,2) | NO | 0 | |
| TaxRate | decimal(5,2) | YES | | |
| TotalAmount | decimal(18,2) | NO | 0 | |
| ServiceStartDate | datetime | YES | | |
| ServiceEndDate | datetime | YES | | |
| RevenueRecognitionStartDate | datetime | YES | | |
| RevenueRecognitionEndDate | datetime | YES | | |
| DeferredRevenue | decimal(18,2) | YES | | |
| RecognizedRevenue | decimal(18,2) | YES | | |
| InvoiceId | int | NO | | FK → Invoices |
| ProductId | int | YES | | FK → Products |
| OrderLineItemId | int | YES | | FK → OrderLineItems |
| SubscriptionId | int | YES | | FK → Subscriptions |
| Notes | varchar(1000) | YES | | |
| CreatedAt | datetime | NO | CURRENT_TIMESTAMP | |
| UpdatedAt | datetime | YES | | |
| IsDeleted | bit | NO | 0 | |
| RowVersion | timestamp | NO | | |

### 4.2 Indexes

| Index Name | Table | Columns | Type |
|------------|-------|---------|------|
| IX_Invoices_InvoiceNumber | Invoices | InvoiceNumber | Unique |
| IX_Invoices_AccountId | Invoices | AccountId | Non-unique |
| IX_Invoices_Status | Invoices | Status | Non-unique |
| IX_Invoices_DueDate | Invoices | DueDate | Non-unique |
| IX_Invoices_InvoiceDate | Invoices | InvoiceDate | Non-unique |
| IX_Invoices_OrderId | Invoices | OrderId | Non-unique |
| IX_InvoiceLineItems_InvoiceId | InvoiceLineItems | InvoiceId | Non-unique |
| IX_InvoiceLineItems_ProductId | InvoiceLineItems | ProductId | Non-unique |

### 4.3 Foreign Keys

| FK Name | From Table.Column | To Table.Column |
|---------|-------------------|-----------------|
| FK_Invoices_Account | Invoices.AccountId | Accounts.Id |
| FK_Invoices_Order | Invoices.OrderId | Orders.Id |
| FK_Invoices_Contact | Invoices.ContactId | Contacts.Id |
| FK_Invoices_Subscription | Invoices.SubscriptionId | Subscriptions.Id |
| FK_Invoices_VoidedBy | Invoices.VoidedById | Users.Id |
| FK_Invoices_OriginalInvoice | Invoices.OriginalInvoiceId | Invoices.Id |
| FK_InvoiceLineItems_Invoice | InvoiceLineItems.InvoiceId | Invoices.Id |
| FK_InvoiceLineItems_Product | InvoiceLineItems.ProductId | Products.Id |
| FK_InvoiceLineItems_OrderLineItem | InvoiceLineItems.OrderLineItemId | OrderLineItems.Id |
| FK_InvoiceLineItems_Subscription | InvoiceLineItems.SubscriptionId | Subscriptions.Id |

---

## 5. Test Specification

### 5.1 Unit Tests

| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| InvoiceServiceTests | CreateAsync_GeneratesInvoiceNumber | Should auto-generate invoice number | ❌ Not Implemented |
| InvoiceServiceTests | CreateFromOrderAsync_CopiesLineItems | Should copy order line items | ❌ Not Implemented |
| InvoiceServiceTests | RecordPaymentAsync_UpdatesBalance | Should update amount paid and status | ❌ Not Implemented |
| InvoiceServiceTests | RecalculateTotalsAsync_CalculatesCorrectly | Should sum line items correctly | ❌ Not Implemented |
| InvoiceServiceTests | VoidAsync_PreventsPaidInvoice | Should throw for paid invoices | ❌ Not Implemented |

### 5.2 Integration Tests

| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| InvoicesControllerTests | CreateInvoice_ReturnsCreated | POST /api/invoices | ⚠️ Test exists, no controller |
| InvoicesControllerTests | GetOverdue_ReturnsOverdueInvoices | GET /api/invoices/overdue | ⚠️ Test exists, no controller |

### 5.3 E2E Tests

| Test File | Test Name | Description | Status |
|-----------|-----------|-------------|--------|
| invoices.spec.ts | should create invoice from order | Full workflow test | ❌ Not Implemented |
| invoices.spec.ts | should record payment | Payment recording flow | ❌ Not Implemented |
| invoices.spec.ts | should void invoice | Void workflow test | ❌ Not Implemented |

---

## 6. Issues & Inconsistencies

### 6.1 Naming Inconsistencies
| Issue | Current | Expected | Impact |
|-------|---------|----------|--------|
| None identified | - | - | - |

### 6.2 Validation Gaps
| Entity | Gap | Recommendation |
|--------|-----|----------------|
| Invoice | No maximum for TotalAmount | Add reasonable maximum validation |
| Invoice | DueDate validation not enforced in service | Add validation in CreateAsync |
| InvoiceLineItem | No validation for service period dates | Ensure StartDate < EndDate |

### 6.3 Missing Components
| Component | Type | Priority |
|-----------|------|----------|
| InvoicesController.cs | API Controller | P1 - High |
| InvoiceDto.cs | DTO | P1 - High |
| invoiceService.ts | Frontend Service | P1 - High |
| InvoicesPage.tsx | Frontend Page | P1 - High |
| InvoiceDetailsPage.tsx | Frontend Page | P1 - High |
| InvoiceServiceTests.cs | Unit Tests | P2 - Medium |

---

## 7. TODO Items

| ID | Description | Priority | Category | Status |
|----|-------------|----------|----------|--------|
| TODO-SALES003-001 | Create InvoicesController.cs with all endpoints | P1 | Backend | ⬜ Pending |
| TODO-SALES003-002 | Create InvoiceDto, CreateInvoiceDto, UpdateInvoiceDto | P1 | Backend | ⬜ Pending |
| TODO-SALES003-003 | Create invoiceService.ts frontend service | P1 | Frontend | ⬜ Pending |
| TODO-SALES003-004 | Create InvoicesPage.tsx with data grid | P1 | Frontend | ⬜ Pending |
| TODO-SALES003-005 | Create InvoiceDetailsPage.tsx | P1 | Frontend | ⬜ Pending |
| TODO-SALES003-006 | Create InvoiceForm.tsx component | P2 | Frontend | ⬜ Pending |
| TODO-SALES003-007 | Create InvoiceLineItemsEditor.tsx | P2 | Frontend | ⬜ Pending |
| TODO-SALES003-008 | Create InvoicePaymentPanel.tsx | P2 | Frontend | ⬜ Pending |
| TODO-SALES003-009 | Add DueDate >= InvoiceDate validation in service | P2 | Validation | ⬜ Pending |
| TODO-SALES003-010 | Create InvoiceServiceTests.cs unit tests | P2 | Testing | ⬜ Pending |
| TODO-SALES003-011 | Create invoice E2E tests | P3 | Testing | ⬜ Pending |
| TODO-SALES003-012 | Implement invoice PDF generation | P3 | Backend | ⬜ Pending |

---

## 8. Related Specifications

| Spec ID | Name | Relationship |
|---------|------|--------------|
| SPEC-CRM-001 | Account Management | Invoices belong to Accounts |
| SPEC-SALES-001 | Quote Management | Invoices can be created from Quotes |
| SPEC-SALES-002 | Order Management | Invoices can be created from Orders |
| SPEC-SALES-004 | Payment Management | Payments applied to Invoices |

---

## Appendix A: Invoice Status Flow

```
                                    ┌─────────┐
                                    │  Draft  │
                                    └────┬────┘
                                         │ Submit
                                         ▼
                                ┌─────────────────┐
                                │ PendingApproval │
                                └────────┬────────┘
                           Reject │      │ Approve
                                  ▼      ▼
                             ┌────────────────┐
                             │    Approved    │
                             └───────┬────────┘
                                     │ Send
                                     ▼
    ┌───────────────────────────────────────────────────────┐
    │                        Sent                            │
    └───────────────────────────┬───────────────────────────┘
                                │ Customer Views
                                ▼
    ┌───────────────────────────────────────────────────────┐
    │                       Viewed                           │
    └────────┬──────────────────┬──────────────────┬────────┘
             │                  │                  │
    Partial Payment      Full Payment          Past Due
             │                  │                  │
             ▼                  ▼                  ▼
    ┌─────────────────┐  ┌─────────────┐  ┌─────────────────┐
    │  PartiallyPaid  │  │    Paid     │  │     Overdue     │
    └────────┬────────┘  └─────────────┘  └────────┬────────┘
             │                                      │
        Full Payment                         Collections
             │                                      │
             ▼                                      ▼
    ┌─────────────────┐                   ┌─────────────────┐
    │      Paid       │                   │   Collections   │
    └─────────────────┘                   └────────┬────────┘
                                                   │
                                            Write Off
                                                   │
                                                   ▼
                                          ┌─────────────────┐
                                          │   WrittenOff    │
                                          └─────────────────┘

    * Voided: Can transition from Draft, Approved, Sent, Viewed, Overdue
    * Disputed: Can transition from Sent, Viewed, Overdue
    * Refunded: Can transition from Paid
```

---

## Appendix B: Invoice Number Format

```
INV-YYMM-NNNN

Where:
  INV  = Fixed prefix
  YY   = 2-digit year
  MM   = 2-digit month
  NNNN = 4-digit sequence number (resets monthly)

Examples:
  INV-2602-0001  (First invoice of Feb 2026)
  INV-2602-0042  (42nd invoice of Feb 2026)
  INV-2603-0001  (First invoice of Mar 2026)
```

---

**Document End**
