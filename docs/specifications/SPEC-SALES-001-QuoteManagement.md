# SPEC-SALES-001: Quote Management

> **Version:** 1.0  
> **Last Updated:** February 2026  
> **Status:** ✅ Complete  
> **Module:** Sales  
> **Priority:** P1  
> **Dependencies:** SPEC-CRM-001 (Account Management), SPEC-CRM-003 (Opportunity Management)

---

## 1. Business Context

### 1.1 Overview

Quote Management provides comprehensive functionality for creating, sending, and managing sales quotes. The system supports the full quote lifecycle from draft creation through customer acceptance, including revision management, line item editing, pricing calculations, and e-signature integration.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Quote Creation | Create quotes manually or from opportunities | ✅ Implemented |
| SF-002 | Quote Numbering | Auto-generation of quote numbers (Q{YY}{MM}-{####}) | ✅ Implemented |
| SF-003 | Line Item Management | Add/edit/delete/reorder line items with products | ✅ Implemented |
| SF-004 | Pricing Calculations | Automatic totals, discounts, tax, shipping | ✅ Implemented |
| SF-005 | Quote Lifecycle | Draft → Send → View → Accept/Reject workflow | ✅ Implemented |
| SF-006 | Quote Revisions | Create new versions of existing quotes | ✅ Implemented |
| SF-007 | Approval Workflow | Submit quotes for approval before sending | ✅ Implemented |
| SF-008 | E-Signature Integration | Track signature status on quotes | ✅ Implemented |
| SF-009 | PDF Generation | Export quotes to PDF format | ✅ Implemented |
| SF-010 | Address Management | Billing and shipping address handling | ✅ Implemented |

### 1.3 Functionalities

| ID | Functionality | Description | Status |
|----|---------------|-------------|--------|
| F-001 | List Quotes | Display all quotes with filtering by customer, opportunity, status | ✅ Implemented |
| F-002 | Create Quote | Create new quote with header and line items | ✅ Implemented |
| F-003 | Edit Quote | Modify quote details (restricted by status) | ✅ Implemented |
| F-004 | Delete Quote | Soft delete (Draft/New status only) | ✅ Implemented |
| F-005 | Send Quote | Mark quote as shared, set sent date | ✅ Implemented |
| F-006 | Mark Viewed | Record when customer views the quote | ✅ Implemented |
| F-007 | Accept Quote | Customer acceptance with optional signature | ✅ Implemented |
| F-008 | Reject Quote | Customer rejection with reason tracking | ✅ Implemented |
| F-009 | Create Revision | Clone quote with incremented version number | ✅ Implemented |
| F-010 | Get Statistics | Calculate quote metrics (counts, values, rates) | ✅ Implemented |
| F-011 | Manage Line Items | Full CRUD on line items with auto-recalculation | ✅ Implemented |
| F-012 | Reorder Line Items | Drag-and-drop line item reordering | ✅ Implemented |
| F-013 | Print/Export PDF | Generate professional PDF quotes | ✅ Implemented |

### 1.4 Use Cases

| ID | Use Case | Actor | Description |
|----|----------|-------|-------------|
| UC-001 | Create Sales Quote | Sales Rep | Create quote for customer from opportunity |
| UC-002 | Add Products to Quote | Sales Rep | Add line items from product catalog |
| UC-003 | Apply Discount | Sales Rep | Apply percentage or fixed discount to line items |
| UC-004 | Submit for Approval | Sales Rep | Submit quote for manager approval (if required) |
| UC-005 | Approve Quote | Sales Manager | Review and approve quote |
| UC-006 | Send Quote to Customer | Sales Rep | Email/share quote with customer |
| UC-007 | View Quote | Customer | Customer views quote online |
| UC-008 | Accept Quote | Customer | Customer accepts and optionally signs |
| UC-009 | Request Revision | Customer | Customer requests changes |
| UC-010 | Revise Quote | Sales Rep | Create new version addressing customer feedback |
| UC-011 | Convert to Order | Sales Rep | Convert accepted quote to order |

---

## 2. Frontend Specification

### 2.1 Pages

| Page | Route | Component | Description | Status |
|------|-------|-----------|-------------|--------|
| Quotes List | `/quotes` | QuotesPage.tsx | Main quotes management page | ✅ Implemented |

### 2.2 Components

| Component | File | Description | Status |
|-----------|------|-------------|--------|
| QuotesPage | pages/QuotesPage.tsx | Main page (755 lines) | ✅ Implemented |
| QuoteLineItemsEditor | components/QuoteLineItemsEditor.tsx | Line item management (637 lines) | ✅ Implemented |
| QuoteDialog | (embedded in QuotesPage) | Create/Edit dialog with tabs | ✅ Implemented |

### 2.3 Dialog Tabs

| Tab | Index | Description | Status |
|-----|-------|-------------|--------|
| Details | 0 | Title, customer, status, valid until, description | ✅ Implemented |
| Line Items | 1 | QuoteLineItemsEditor component | ✅ Implemented |
| Pricing | 2 | Subtotal, discount, tax, shipping, totals | ✅ Implemented |
| Addresses | 3 | Billing and shipping addresses | ✅ Implemented |
| Terms | 4 | Terms and conditions, notes | ✅ Implemented |
| Related | 5 | Related accounts, contacts, opportunities (edit only) | ✅ Implemented |
| Notes | 6 | Notes attached to quote | ✅ Implemented |

### 2.4 Services

| Service | File | Methods | Status |
|---------|------|---------|--------|
| Direct API | apiClient | GET, POST, PUT, DELETE /quotes/* | ✅ Implemented |

### 2.5 Frontend Validations

| Field | Rule | Error Message | Status |
|-------|------|---------------|--------|
| Title | Required | "Please enter a quote title" | ✅ Implemented |
| Line Item Name | Required (if no product) | (validated on add) | ✅ Implemented |
| Quantity | Must be > 0 | (validated on add) | ✅ Implemented |

---

## 3. Backend Specification

### 3.1 Entities

#### Quote Entity
**File:** `CRM.Core/Entities/Quote.cs` (498 lines)

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| Id | int | Primary key | Yes (auto) |
| QuoteNumber | string | Auto-generated (Q{YY}{MM}-{####}) | Yes (auto) |
| ExternalQuoteId | string? | External reference | No |
| Version | int | Revision number (starts at 1) | Yes |
| Title | string | Quote title | Yes |
| Description | string? | Quote description | No |
| Status | QuoteStatus | Current status (enum) | Yes |
| QuoteDate | DateTime | Date quote created | Yes |
| ExpirationDate | DateTime? | When quote expires | No |
| SentDate | DateTime? | Date sent to customer | No |
| ViewedDate | DateTime? | Date customer viewed | No |
| AcceptedDate | DateTime? | Date accepted | No |
| RejectedDate | DateTime? | Date rejected | No |
| Subtotal | decimal | Sum of line items | Yes |
| Discount | decimal | Total discount amount | Yes |
| DiscountPercent | decimal | Discount percentage | Yes |
| Tax | decimal | Tax amount | Yes |
| TaxRate | decimal | Tax percentage | Yes |
| ShippingCost | decimal | Shipping charges | Yes |
| Total | decimal | Final total | Yes |
| CurrencyCode | string | Currency (default USD) | Yes |
| PaymentTerms | string? | Payment terms | No |
| DeliveryTerms | string? | Delivery terms | No |
| TermsAndConditions | string? | Terms text | No |
| Warranty | string? | Warranty info | No |
| ValidityDays | int? | Days until expiration | No |
| BillingName | string? | Billing contact name | No |
| BillingAddress | string? | Billing street address | No |
| BillingCity | string? | Billing city | No |
| BillingState | string? | Billing state | No |
| BillingZipCode | string? | Billing ZIP | No |
| BillingCountry | string? | Billing country | No |
| ShippingName | string? | Shipping contact name | No |
| ShippingAddress | string? | Shipping street address | No |
| ShippingCity | string? | Shipping city | No |
| ShippingState | string? | Shipping state | No |
| ShippingZipCode | string? | Shipping ZIP | No |
| ShippingCountry | string? | Shipping country | No |
| AccountId | int? | Related customer/account | No |
| ContactId | int? | Related contact | No |
| OpportunityId | int? | Related opportunity | No |
| AssignedToUserId | int? | Owner/assignee | No |
| CreatedByUserId | int? | Creator | No |
| ApprovedByUserId | int? | Approver | No |
| ParentQuoteId | int? | Original quote (for revisions) | No |
| RelationshipManagerId | int? | Account manager | No |
| RequiresApproval | bool | Needs approval before send | Yes |
| IsApproved | bool | Has been approved | Yes |
| ApprovalDate | DateTime? | When approved | No |
| ApprovalNotes | string? | Approval comments | No |
| SubmittedForApprovalDate | DateTime? | When submitted | No |
| IsSigned | bool | Has signature | Yes |
| SignedDate | DateTime? | When signed | No |
| SignedBy | string? | Signer name | No |
| SignatureUrl | string? | Signature image URL | No |
| InternalNotes | string? | Internal notes | No |
| Tags | string? | JSON tags array | No |
| CustomFields | string? | JSON custom fields | No |

#### QuoteStatus Enum

| Value | Name | Description |
|-------|------|-------------|
| 0 | New | Just created |
| 1 | Draft | In progress |
| 2 | UnderApproval | Awaiting approval |
| 3 | Approved | Approved by manager |
| 4 | Shared | Sent to customer |
| 5 | Viewed | Customer has viewed |
| 6 | Accepted | Customer accepted |
| 7 | Rejected | Customer rejected |
| 8 | Expired | Past expiration date |
| 9 | Revised | Superseded by new version |
| 10 | Cancelled | Manually cancelled |
| 11 | Converted | Converted to order |
| 12 | EndOfLife | No longer valid |

#### QuoteLineItem Entity
**File:** `CRM.Core/Entities/QuoteLineItem.cs` (317 lines)

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| Id | int | Primary key | Yes (auto) |
| QuoteId | int | Parent quote | Yes |
| LineNumber | int | Sequence number | Yes |
| ProductId | int? | Product reference | No |
| SKU | string? | Stock keeping unit | No |
| Name | string | Item name | Yes |
| Description | string? | Item description | No |
| Category | string? | Product category | No |
| Quantity | decimal | Quantity (default 1) | Yes |
| UnitOfMeasure | string? | Unit (default "each") | No |
| UnitPrice | decimal | Price per unit | Yes |
| ListPrice | decimal? | MSRP | No |
| CostPrice | decimal? | Cost for margin calc | No |
| DiscountType | LineItemDiscountType | None/Percentage/FixedAmount | Yes |
| DiscountPercent | decimal | Discount % (0-100) | Yes |
| DiscountAmount | decimal | Fixed discount | Yes |
| DiscountReason | string? | Reason for discount | No |
| DiscountRequiresApproval | bool | Needs approval | Yes |
| DiscountApproved | bool | Has been approved | Yes |
| TaxRate | decimal | Tax % for this item | Yes |
| IsTaxable | bool | Subject to tax | Yes |
| TaxCode | string? | Tax code reference | No |
| Subtotal | decimal | Qty × UnitPrice | Yes |
| TotalDiscount | decimal | Calculated discount | Yes |
| TaxAmount | decimal | Calculated tax | Yes |
| Total | decimal | Final line total | Yes |
| Margin | decimal? | Profit margin | No |
| BillingPeriod | string? | Subscription period | No |
| WarrantyMonths | int? | Warranty duration | No |
| DeliveryDate | DateTime? | Expected delivery | No |
| ServiceStartDate | DateTime? | Service start | No |
| ServiceEndDate | DateTime? | Service end | No |
| IsOptional | bool | Optional item | Yes |
| IsIncluded | bool | Include in totals | Yes |
| ParentLineItemId | int? | Bundle parent | No |
| IsBundle | bool | Is bundle header | Yes |
| InternalNotes | string? | Internal notes | No |
| QuoteNotes | string? | Notes on quote | No |
| CustomFields | string? | JSON custom fields | No |

#### LineItemDiscountType Enum

| Value | Name | Description |
|-------|------|-------------|
| 0 | None | No discount |
| 1 | Percentage | Percentage discount |
| 2 | FixedAmount | Fixed amount discount |

### 3.2 Entity Methods

#### Quote.RecalculateFromLineItems()
Recalculates quote totals from line items:
- Sums only `IsIncluded` items
- Sets Subtotal, Tax, Discount, Total

#### Quote Computed Properties
- `IsExpired`: ExpirationDate < Now (if set and status is editable)
- `CanEdit`: Status is New, Draft, or UnderApproval
- `CanSubmitForApproval`: Status is Draft and RequiresApproval
- `CanShare`: Status is Approved (if RequiresApproval) or Draft (if not)

#### QuoteLineItem.RecalculateTotals()
```csharp
Subtotal = Quantity * UnitPrice;
TotalDiscount = DiscountType switch {
    Percentage => Subtotal * (DiscountPercent / 100),
    FixedAmount => DiscountAmount,
    _ => 0
};
var afterDiscount = Subtotal - TotalDiscount;
TaxAmount = IsTaxable ? afterDiscount * (TaxRate / 100) : 0;
Total = afterDiscount + TaxAmount;
if (CostPrice.HasValue) {
    Margin = Total - (CostPrice.Value * Quantity);
}
```

### 3.3 DTOs

#### QuoteStatistics DTO
**File:** `CRM.Core/Interfaces/IQuoteService.cs`

| Property | Type | Description |
|----------|------|-------------|
| TotalQuotes | int | Total count |
| DraftQuotes | int | Draft status count |
| SentQuotes | int | Shared status count |
| AcceptedQuotes | int | Accepted status count |
| RejectedQuotes | int | Rejected status count |
| ExpiredQuotes | int | Expired status count |
| TotalValue | decimal | Sum of all quote totals |
| AcceptedValue | decimal | Sum of accepted quote totals |
| AcceptanceRate | double | Accepted / (Accepted + Rejected) % |

### 3.4 Interfaces

#### IQuoteService
**File:** `CRM.Core/Interfaces/IQuoteService.cs` (101 lines)

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| GetQuotesAsync | customerId?, opportunityId?, status?, includeExpired | IEnumerable<Quote> | Filter quotes |
| GetByIdAsync | id | Quote? | Get single quote |
| GetByQuoteNumberAsync | quoteNumber | Quote? | Find by number |
| CreateAsync | quote | Quote | Create new quote |
| UpdateAsync | quote | Quote | Update existing |
| DeleteAsync | id | bool | Soft delete (Draft/New only) |
| SendAsync | id | Quote | Mark as Shared |
| AcceptAsync | id | Quote | Mark as Accepted |
| RejectAsync | id, reason? | Quote | Mark as Rejected |
| CreateRevisionAsync | originalQuoteId | Quote | Clone with new version |
| GetStatisticsAsync | customerId? | QuoteStatistics | Get metrics |

### 3.5 Services

#### QuoteService
**File:** `CRM.Infrastructure/Services/QuoteService.cs` (473 lines)

| Method | Implementation Notes | Status |
|--------|---------------------|--------|
| GetQuotesAsync | Filters by customer, opportunity, status; excludes expired unless requested | ✅ Implemented |
| GetByIdAsync | Includes Account, Contact, Opportunity, LineItems with Products | ✅ Implemented |
| GetByQuoteNumberAsync | Includes Account, Contact, Opportunity | ✅ Implemented |
| CreateAsync | Auto-generates number (Q{YY}{MM}-{####}), sets expiration from ValidityDays | ✅ Implemented |
| UpdateAsync | Recalculates totals, updates timestamp | ✅ Implemented |
| DeleteAsync | Only allows Draft/New status, soft delete | ✅ Implemented |
| SendAsync | Sets Status=Shared, SentDate=Now | ✅ Implemented |
| AcceptAsync | Sets Status=Accepted, AcceptedDate=Now | ✅ Implemented |
| RejectAsync | Sets Status=Rejected, appends reason to InternalNotes | ✅ Implemented |
| CreateRevisionAsync | Clones quote, increments Version, marks original as Revised | ✅ Implemented |
| GetStatisticsAsync | Calculates counts, values, acceptance rate | ✅ Implemented |
| GenerateQuoteNumberAsync | Format: Q{YY}{MM}-{####} sequential | ✅ Implemented |

### 3.6 Controllers

#### QuotesController
**File:** `CRM.Api/Controllers/QuotesController.cs` (613 lines)

| Endpoint | Method | Description | Status |
|----------|--------|-------------|--------|
| GET /api/quotes | GetQuotes | List with filters | ✅ Implemented |
| GET /api/quotes/{id} | GetQuote | Get by ID | ✅ Implemented |
| GET /api/quotes/number/{quoteNumber} | GetByQuoteNumber | Get by number | ✅ Implemented |
| POST /api/quotes | CreateQuote | Create new quote | ✅ Implemented |
| PUT /api/quotes/{id} | UpdateQuote | Update quote | ✅ Implemented |
| DELETE /api/quotes/{id} | DeleteQuote | Soft delete | ✅ Implemented |
| POST /api/quotes/{id}/send | SendQuote | Mark as shared | ✅ Implemented |
| POST /api/quotes/{id}/viewed | MarkViewed | Record view date | ✅ Implemented |
| POST /api/quotes/{id}/accept | AcceptQuote | Accept with optional signature | ✅ Implemented |
| POST /api/quotes/{id}/reject | RejectQuote | Reject with reason | ✅ Implemented |
| POST /api/quotes/{id}/revise | CreateRevision | Create new version | ✅ Implemented |
| GET /api/quotes/{quoteId}/lineitems | GetLineItems | List line items | ✅ Implemented |
| GET /api/quotes/{quoteId}/lineitems/{lineItemId} | GetLineItem | Get single line item | ✅ Implemented |
| POST /api/quotes/{quoteId}/lineitems | AddLineItem | Add line item | ✅ Implemented |
| PUT /api/quotes/{quoteId}/lineitems/{lineItemId} | UpdateLineItem | Update line item | ✅ Implemented |
| DELETE /api/quotes/{quoteId}/lineitems/{lineItemId} | DeleteLineItem | Soft delete line item | ✅ Implemented |
| POST /api/quotes/{quoteId}/lineitems/reorder | ReorderLineItems | Reorder line items | ✅ Implemented |

#### Request DTOs

**AcceptQuoteRequest:**
```csharp
public class AcceptQuoteRequest
{
    public bool IsSigned { get; set; }
    public string? SignedBy { get; set; }
}
```

**RejectQuoteRequest:**
```csharp
public class RejectQuoteRequest
{
    public string? Reason { get; set; }
}
```

### 3.7 Backend Validations

| Rule | Location | Behavior | Status |
|------|----------|----------|--------|
| Quote delete only Draft/New | QuoteService.DeleteAsync | Throws if wrong status | ✅ Implemented |
| Line item auto-numbering | Controller.AddLineItem | Auto-assigns LineNumber | ✅ Implemented |
| Product data population | Controller.AddLineItem | Copies from Product if ProductId set | ✅ Implemented |
| Quote recalculation | After line item changes | RecalculateFromLineItems() called | ✅ Implemented |
| Line item recalculation | AddLineItem/UpdateLineItem | RecalculateTotals() called | ✅ Implemented |

---

## 4. Database Specification

### 4.1 Tables

#### Quotes Table

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | INT | NO | AUTO_INCREMENT | Primary key |
| QuoteNumber | VARCHAR(50) | NO | | Unique number |
| ExternalQuoteId | VARCHAR(100) | YES | | External reference |
| Version | INT | NO | 1 | Revision number |
| Title | VARCHAR(255) | NO | | Quote title |
| Description | TEXT | YES | | Description |
| Status | INT | NO | 0 | QuoteStatus enum |
| QuoteDate | DATETIME | NO | CURRENT_TIMESTAMP | Created date |
| ExpirationDate | DATETIME | YES | | Expiry date |
| SentDate | DATETIME | YES | | When sent |
| ViewedDate | DATETIME | YES | | When viewed |
| AcceptedDate | DATETIME | YES | | When accepted |
| RejectedDate | DATETIME | YES | | When rejected |
| Subtotal | DECIMAL(18,2) | NO | 0 | Line items total |
| Discount | DECIMAL(18,2) | NO | 0 | Discount amount |
| DiscountPercent | DECIMAL(5,2) | NO | 0 | Discount % |
| Tax | DECIMAL(18,2) | NO | 0 | Tax amount |
| TaxRate | DECIMAL(5,2) | NO | 0 | Tax % |
| ShippingCost | DECIMAL(18,2) | NO | 0 | Shipping |
| Total | DECIMAL(18,2) | NO | 0 | Grand total |
| CurrencyCode | VARCHAR(3) | NO | 'USD' | Currency |
| PaymentTerms | VARCHAR(500) | YES | | Payment terms |
| DeliveryTerms | VARCHAR(500) | YES | | Delivery terms |
| TermsAndConditions | TEXT | YES | | T&C |
| Warranty | VARCHAR(500) | YES | | Warranty info |
| ValidityDays | INT | YES | | Days valid |
| BillingName | VARCHAR(200) | YES | | Billing contact |
| BillingAddress | VARCHAR(500) | YES | | Billing street |
| BillingCity | VARCHAR(100) | YES | | Billing city |
| BillingState | VARCHAR(100) | YES | | Billing state |
| BillingZipCode | VARCHAR(20) | YES | | Billing ZIP |
| BillingCountry | VARCHAR(100) | YES | | Billing country |
| ShippingName | VARCHAR(200) | YES | | Shipping contact |
| ShippingAddress | VARCHAR(500) | YES | | Shipping street |
| ShippingCity | VARCHAR(100) | YES | | Shipping city |
| ShippingState | VARCHAR(100) | YES | | Shipping state |
| ShippingZipCode | VARCHAR(20) | YES | | Shipping ZIP |
| ShippingCountry | VARCHAR(100) | YES | | Shipping country |
| AccountId | INT | YES | | FK to Customers |
| ContactId | INT | YES | | FK to Contacts |
| OpportunityId | INT | YES | | FK to Opportunities |
| AssignedToUserId | INT | YES | | FK to Users |
| CreatedByUserId | INT | YES | | FK to Users |
| ApprovedByUserId | INT | YES | | FK to Users |
| ParentQuoteId | INT | YES | | FK to Quotes (self) |
| RelationshipManagerId | INT | YES | | FK to Users |
| RequiresApproval | BIT | NO | 0 | Needs approval |
| IsApproved | BIT | NO | 0 | Is approved |
| ApprovalDate | DATETIME | YES | | When approved |
| ApprovalNotes | TEXT | YES | | Approval notes |
| SubmittedForApprovalDate | DATETIME | YES | | When submitted |
| IsSigned | BIT | NO | 0 | Has signature |
| SignedDate | DATETIME | YES | | When signed |
| SignedBy | VARCHAR(200) | YES | | Signer name |
| SignatureUrl | VARCHAR(500) | YES | | Signature URL |
| InternalNotes | TEXT | YES | | Internal notes |
| Tags | TEXT | YES | | JSON tags |
| CustomFields | TEXT | YES | | JSON custom |
| CreatedAt | DATETIME | NO | CURRENT_TIMESTAMP | Created timestamp |
| UpdatedAt | DATETIME | YES | | Updated timestamp |
| IsDeleted | BIT | NO | 0 | Soft delete |
| RowVersion | BINARY(8) | YES | | Concurrency |

#### QuoteLineItems Table

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | INT | NO | AUTO_INCREMENT | Primary key |
| QuoteId | INT | NO | | FK to Quotes |
| LineNumber | INT | NO | | Sequence |
| ProductId | INT | YES | | FK to Products |
| SKU | VARCHAR(100) | YES | | Product SKU |
| Name | VARCHAR(255) | NO | | Item name |
| Description | TEXT | YES | | Description |
| Category | VARCHAR(100) | YES | | Category |
| Quantity | DECIMAL(18,4) | NO | 1 | Quantity |
| UnitOfMeasure | VARCHAR(50) | YES | 'each' | Unit |
| UnitPrice | DECIMAL(18,2) | NO | 0 | Unit price |
| ListPrice | DECIMAL(18,2) | YES | | List price |
| CostPrice | DECIMAL(18,2) | YES | | Cost price |
| DiscountType | INT | NO | 0 | Discount type enum |
| DiscountPercent | DECIMAL(5,2) | NO | 0 | Discount % |
| DiscountAmount | DECIMAL(18,2) | NO | 0 | Fixed discount |
| DiscountReason | VARCHAR(500) | YES | | Reason |
| DiscountRequiresApproval | BIT | NO | 0 | Needs approval |
| DiscountApproved | BIT | NO | 0 | Is approved |
| TaxRate | DECIMAL(5,2) | NO | 0 | Tax % |
| IsTaxable | BIT | NO | 1 | Is taxable |
| TaxCode | VARCHAR(50) | YES | | Tax code |
| Subtotal | DECIMAL(18,2) | NO | 0 | Qty × Price |
| TotalDiscount | DECIMAL(18,2) | NO | 0 | Calc discount |
| TaxAmount | DECIMAL(18,2) | NO | 0 | Calc tax |
| Total | DECIMAL(18,2) | NO | 0 | Line total |
| Margin | DECIMAL(18,2) | YES | | Profit margin |
| BillingPeriod | VARCHAR(50) | YES | | Subscription |
| WarrantyMonths | INT | YES | | Warranty |
| DeliveryDate | DATETIME | YES | | Expected delivery |
| ServiceStartDate | DATETIME | YES | | Service start |
| ServiceEndDate | DATETIME | YES | | Service end |
| IsOptional | BIT | NO | 0 | Optional item |
| IsIncluded | BIT | NO | 1 | In totals |
| ParentLineItemId | INT | YES | | FK to self |
| IsBundle | BIT | NO | 0 | Bundle header |
| InternalNotes | TEXT | YES | | Internal notes |
| QuoteNotes | TEXT | YES | | Visible notes |
| CustomFields | TEXT | YES | | JSON custom |
| CreatedAt | DATETIME | NO | CURRENT_TIMESTAMP | Created |
| UpdatedAt | DATETIME | YES | | Updated |
| IsDeleted | BIT | NO | 0 | Soft delete |

### 4.2 Indexes

| Index | Table | Columns | Type |
|-------|-------|---------|------|
| IX_Quotes_QuoteNumber | Quotes | QuoteNumber | UNIQUE |
| IX_Quotes_AccountId | Quotes | AccountId | INDEX |
| IX_Quotes_OpportunityId | Quotes | OpportunityId | INDEX |
| IX_Quotes_Status | Quotes | Status | INDEX |
| IX_Quotes_ParentQuoteId | Quotes | ParentQuoteId | INDEX |
| IX_QuoteLineItems_QuoteId | QuoteLineItems | QuoteId | INDEX |
| IX_QuoteLineItems_ProductId | QuoteLineItems | ProductId | INDEX |

### 4.3 Foreign Keys

| FK Name | Table | Column | References |
|---------|-------|--------|------------|
| FK_Quotes_Accounts | Quotes | AccountId | Customers(Id) |
| FK_Quotes_Contacts | Quotes | ContactId | Contacts(Id) |
| FK_Quotes_Opportunities | Quotes | OpportunityId | Opportunities(Id) |
| FK_Quotes_AssignedTo | Quotes | AssignedToUserId | Users(Id) |
| FK_Quotes_CreatedBy | Quotes | CreatedByUserId | Users(Id) |
| FK_Quotes_ApprovedBy | Quotes | ApprovedByUserId | Users(Id) |
| FK_Quotes_Parent | Quotes | ParentQuoteId | Quotes(Id) |
| FK_QuoteLineItems_Quote | QuoteLineItems | QuoteId | Quotes(Id) |
| FK_QuoteLineItems_Product | QuoteLineItems | ProductId | Products(Id) |
| FK_QuoteLineItems_Parent | QuoteLineItems | ParentLineItemId | QuoteLineItems(Id) |

---

## 5. Tests

### 5.1 Backend Unit Tests

| Test File | Test Cases | Status |
|-----------|------------|--------|
| QuotesControllerTests.cs | Controller endpoint tests | ⚠️ Exists |
| QuoteRepositoryTests.cs | Repository tests | ⚠️ Exists |
| QuoteInvoiceEntityTests.cs | Entity tests | ⚠️ Exists |

### 5.2 Integration Tests

| Test | Description | Status |
|------|-------------|--------|
| Quote CRUD | Create, read, update, delete quotes | ⚠️ Partial |
| Quote Workflow | Send, view, accept, reject flow | ⚠️ Partial |
| Line Item Management | Add, update, delete, reorder | ⚠️ Partial |
| Revision Creation | Create revision from original | ⚠️ Partial |

### 5.3 E2E Tests

| Test | Description | Status |
|------|-------------|--------|
| Create Quote | Create quote through UI | ❌ Not Implemented |
| Add Line Items | Add products to quote | ❌ Not Implemented |
| Quote Lifecycle | Full workflow test | ❌ Not Implemented |

---

## 6. Issues & Inconsistencies

### 6.1 Naming Issues

| Issue | Current | Expected | Severity |
|-------|---------|----------|----------|
| None identified | - | - | - |

### 6.2 Validation Gaps

| Gap | Description | Severity |
|-----|-------------|----------|
| GAP-001 | No server-side validation for discount approval threshold | Low |
| GAP-002 | Expiration date not validated against current date on create | Low |

---

## 7. TODO Items

| ID | Description | Priority | Category | Status |
|----|-------------|----------|----------|--------|
| TODO-SALES001-001 | Add discount approval threshold validation | P3 | Validation | ⏳ Pending |
| TODO-SALES001-002 | Add E2E tests for quote workflow | P2 | Testing | ⏳ Pending |
| TODO-SALES001-003 | Implement quote template feature | P3 | Enhancement | ⏳ Pending |
| TODO-SALES001-004 | Add quote comparison view | P3 | Enhancement | ⏳ Pending |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 2026 | System | Initial specification |

---

**END OF SPECIFICATION**
