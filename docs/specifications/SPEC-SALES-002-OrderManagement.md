# SPEC-SALES-002: Order Management

> **Module:** Sales  
> **Feature:** Order Management  
> **Version:** 1.0  
> **Last Updated:** 2026-02-12  
> **Status:** ✅ Backend Complete | ❌ API Controller Missing | ❌ Frontend Missing  
> **Dependencies:** SPEC-SALES-001 (Quote Management), SPEC-CRM-001 (Account Management), SPEC-CRM-003 (Opportunity Management)

---

## 1. Business Context

### 1.1 Overview

Order Management handles the complete order lifecycle from creation through fulfillment and delivery. Orders can be created from approved quotes or directly from opportunities, supporting multiple order types including standard sales, renewals, upgrades, and returns.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Order CRUD | Create, read, update, delete orders | ✅ Implemented |
| SF-002 | Order from Quote | Convert approved quotes to orders | ✅ Implemented |
| SF-003 | Order from Opportunity | Create orders directly from opportunities | ✅ Implemented |
| SF-004 | Order Status Workflow | Draft → Approved → Processing → Fulfilled → Delivered | ✅ Implemented |
| SF-005 | Approval Workflow | Submit, approve, reject orders | ✅ Implemented |
| SF-006 | Fulfillment Tracking | Track partial and full fulfillment | ✅ Implemented |
| SF-007 | Returns Processing | Handle order returns with quantity tracking | ✅ Implemented |
| SF-008 | Order Line Items | Manage products/services on orders | ✅ Implemented |
| SF-009 | Order Calculations | Subtotal, discount, tax, shipping, total | ✅ Implemented |
| SF-010 | Invoice Generation | Create invoices from orders | ✅ Implemented |
| SF-011 | Order Cloning | Duplicate orders for repeat business | ✅ Implemented |
| SF-012 | Order Search | Search orders by various criteria | ✅ Implemented |
| SF-013 | Order Statistics | Reporting metrics and KPIs | ✅ Implemented |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition |
|-------|----------|-------|--------------|---------------|
| UC-001 | Create Order from Quote | Sales Rep | Quote is approved | Order created with quote data copied |
| UC-002 | Create Order from Opportunity | Sales Rep | Opportunity exists with products | Order created with opportunity products |
| UC-003 | Submit Order for Approval | Sales Rep | Order is in Draft status | Order status changes to PendingApproval |
| UC-004 | Approve Order | Manager | Order is PendingApproval | Order status changes to Approved |
| UC-005 | Reject Order | Manager | Order is PendingApproval | Order status changes to Draft with rejection reason |
| UC-006 | Process Order | Fulfillment | Order is Approved | Order status changes to Processing |
| UC-007 | Mark Order Fulfilled | Fulfillment | Order is Processing | Order status changes to Fulfilled |
| UC-008 | Mark Order Delivered | Fulfillment | Order is Fulfilled | Order status changes to Delivered |
| UC-009 | Process Return | Customer Service | Order is Delivered | Line items updated with return quantities |
| UC-010 | Put Order on Hold | Manager | Order not cancelled | Order status changes to OnHold with reason |
| UC-011 | Cancel Order | Sales Rep | Order not fulfilled | Order status changes to Cancelled with reason |
| UC-012 | Generate Invoice | Finance | Order is Approved+ | Invoice created from order |
| UC-013 | Clone Order | Sales Rep | Order exists | New Draft order created with copied data |
| UC-014 | Apply Discount | Sales Rep | Order in editable status | Discount applied and totals recalculated |

---

## 2. Backend Implementation

### 2.1 Entities

#### 2.1.1 OrderStatus Enum
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented

```csharp
public enum OrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Processing = 3,
    PartiallyFulfilled = 4,
    Fulfilled = 5,
    Delivered = 6,
    Completed = 7,
    Cancelled = 8,
    Returned = 9,
    Refunded = 10,
    OnHold = 11,
    ActionRequired = 12
}
```

#### 2.1.2 OrderType Enum
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented

```csharp
public enum OrderType
{
    Standard = 0,
    Renewal = 1,
    Upgrade = 2,
    Downgrade = 3,
    Amendment = 4,
    TrialConversion = 5,
    Trial = 6,
    Partner = 7,
    Internal = 8,
    Return = 9,
    Credit = 10,
    MultiYear = 11
}
```

#### 2.1.3 FulfillmentMethod Enum
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented

```csharp
public enum FulfillmentMethod
{
    Ship = 0,
    Digital = 1,
    Pickup = 2,
    Provision = 3,
    Activate = 4,
    ServiceDelivery = 5,
    ThirdParty = 6,
    None = 7
}
```

#### 2.1.4 OrderPriority Enum
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented

```csharp
public enum OrderPriority
{
    Normal = 0,
    High = 1,
    Urgent = 2,
    Low = 3,
    Critical = 4
}
```

#### 2.1.5 Order Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented  
**Line Count:** ~450 lines

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **Identification** |
| Id | int | Yes | Primary key |
| OrderNumber | string | Yes | Unique order number (ORD-YYMM-####) |
| ExternalOrderId | string | No | External system reference |
| **Order Details** |
| OrderType | OrderType | Yes | Type of order (Standard, Renewal, etc.) |
| Status | OrderStatus | Yes | Current order status |
| Priority | OrderPriority | Yes | Order priority level |
| FulfillmentMethod | FulfillmentMethod | Yes | How order is fulfilled |
| Description | string | No | Order description |
| InternalNotes | string | No | Internal notes |
| CustomerPONumber | string | No | Customer purchase order number |
| Terms | string | No | Payment/delivery terms |
| **Date Fields** |
| OrderDate | DateTime | Yes | Date order was created |
| RequestedDeliveryDate | DateTime? | No | Customer requested delivery |
| PromisedDeliveryDate | DateTime? | No | Promised delivery date |
| ActualDeliveryDate | DateTime? | No | Actual delivery date |
| ExpiryDate | DateTime? | No | Order validity expiry |
| CancellationDate | DateTime? | No | When order was cancelled |
| FulfillmentDate | DateTime? | No | When order was fulfilled |
| InvoiceDate | DateTime? | No | When invoice was generated |
| PaymentDueDate | DateTime? | No | When payment is due |
| ShipDate | DateTime? | No | When order was shipped |
| **Pricing (15 fields)** |
| Subtotal | decimal | Yes | Sum of line items |
| DiscountAmount | decimal | Yes | Discount amount |
| DiscountPercent | decimal | Yes | Discount percentage |
| TaxAmount | decimal | Yes | Tax amount |
| TaxPercent | decimal | Yes | Tax percentage |
| ShippingAmount | decimal | Yes | Shipping cost |
| HandlingFee | decimal | Yes | Handling fee |
| TotalAmount | decimal | Yes | Final total |
| Currency | string | Yes | Currency code (default: USD) |
| ExchangeRate | decimal | Yes | Exchange rate (default: 1) |
| BaseAmount | decimal | Yes | Amount in base currency |
| CostOfGoods | decimal | Yes | Cost of goods sold |
| GrossProfit | decimal | Yes | Gross profit amount |
| GrossProfitPercent | decimal | Yes | Gross profit percentage |
| CommissionAmount | decimal | Yes | Commission amount |
| **Revenue Recognition** |
| RecognitionMethod | string | No | Revenue recognition method |
| RecognitionStartDate | DateTime? | No | Recognition start |
| RecognitionEndDate | DateTime? | No | Recognition end |
| RecognizedRevenue | decimal | Yes | Recognized revenue amount |
| DeferredRevenue | decimal | Yes | Deferred revenue amount |
| RevenueScheduleId | int? | No | Revenue schedule reference |
| **Billing Address (9 fields)** |
| BillingStreet | string | No | Billing street address |
| BillingCity | string | No | Billing city |
| BillingState | string | No | Billing state/province |
| BillingPostalCode | string | No | Billing postal code |
| BillingCountry | string | No | Billing country |
| BillingContactName | string | No | Billing contact name |
| BillingContactEmail | string | No | Billing contact email |
| BillingContactPhone | string | No | Billing contact phone |
| BillingNotes | string | No | Billing notes |
| **Shipping Address (10 fields)** |
| ShippingStreet | string | No | Shipping street address |
| ShippingCity | string | No | Shipping city |
| ShippingState | string | No | Shipping state/province |
| ShippingPostalCode | string | No | Shipping postal code |
| ShippingCountry | string | No | Shipping country |
| ShippingContactName | string | No | Shipping contact name |
| ShippingContactEmail | string | No | Shipping contact email |
| ShippingContactPhone | string | No | Shipping contact phone |
| ShippingInstructions | string | No | Shipping instructions |
| ShippingMethod | string | No | Shipping method |
| **Shipping Details** |
| TrackingNumber | string | No | Shipment tracking number |
| Carrier | string | No | Shipping carrier |
| EstimatedWeight | decimal | Yes | Estimated weight |
| ShippedWeight | decimal | Yes | Actual shipped weight |
| PackageCount | int | Yes | Number of packages |
| **Payment Information** |
| PaymentMethod | string | No | Payment method |
| PaymentStatus | string | No | Payment status |
| PaymentReference | string | No | Payment reference |
| PaidAmount | decimal | Yes | Amount paid |
| BalanceDue | decimal | Yes | Outstanding balance |
| PaymentTerms | string | No | Payment terms |
| **Relationships** |
| QuoteId | int? | No | Source quote |
| AccountId | int? | No | Customer account |
| ContactId | int? | No | Order contact |
| OpportunityId | int? | No | Related opportunity |
| OwnerId | int? | No | Order owner |
| ApprovedById | int? | No | Approver |
| ParentOrderId | int? | No | Parent order (for amendments) |
| ContractId | int? | No | Related contract |
| SubscriptionId | int? | No | Related subscription |
| InvoiceId | int? | No | Generated invoice |
| **Notes & Attachments** |
| Notes | string | No | General notes |
| AttachmentsJson | string | No | JSON attachments |
| **Workflow Dates** |
| SubmittedDate | DateTime? | No | When submitted for approval |
| ApprovedDate | DateTime? | No | When approved |
| RejectedDate | DateTime? | No | When rejected |
| **Hold Status** |
| IsOnHold | bool | Yes | Whether order is on hold |
| HoldReason | string | No | Reason for hold |
| HoldDate | DateTime? | No | When put on hold |
| HoldReleasedDate | DateTime? | No | When released from hold |
| **Rejection** |
| RejectionReason | string | No | Reason for rejection |
| RejectedById | int? | No | Who rejected |
| **Return** |
| ReturnReason | string | No | Return reason |
| ReturnAuthorizationNumber | string | No | RMA number |
| ReturnDate | DateTime? | No | Return date |
| **Discount Codes** |
| DiscountCode | string | No | Applied discount code |
| CouponCode | string | No | Applied coupon code |
| **Audit Fields** |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |
| CreatedBy | int? | No | Creator user ID |
| UpdatedBy | int? | No | Updater user ID |
| IsDeleted | bool | Yes | Soft delete flag |
| RowVersion | byte[] | No | Optimistic concurrency |
| **Navigation Properties** |
| Quote | Quote | No | Source quote navigation |
| Account | Account | No | Account navigation |
| Contact | Contact | No | Contact navigation |
| Opportunity | Opportunity | No | Opportunity navigation |
| Owner | User | No | Owner navigation |
| ApprovedBy | User | No | Approver navigation |
| ParentOrder | Order | No | Parent order navigation |
| ChildOrders | ICollection | No | Child orders collection |
| Invoice | Invoice | No | Invoice navigation |
| Subscription | Subscription | No | Subscription navigation |
| LineItems | ICollection | Yes | Order line items |

#### 2.1.6 OrderLineItem Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented  
**Line Count:** ~200 lines

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **Identification** |
| Id | int | Yes | Primary key |
| OrderId | int | Yes | Parent order |
| LineNumber | int | Yes | Line sequence |
| **Product Details** |
| ProductId | int? | No | Product reference |
| ProductCode | string | No | Product code |
| ProductName | string | Yes | Product name |
| Description | string | No | Line item description |
| **Quantity & Pricing** |
| Quantity | decimal | Yes | Ordered quantity |
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
| **Fulfillment** |
| QuantityShipped | decimal | Yes | Quantity shipped |
| QuantityBackordered | decimal | Yes | Backorder quantity |
| QuantityReturned | decimal | Yes | Returned quantity |
| FulfilledQuantity | decimal | Yes | Fulfilled quantity |
| RemainingQuantity | decimal | Yes | Remaining to fulfill |
| FulfillmentStatus | string | No | Line fulfillment status |
| ShipDate | DateTime? | No | Line ship date |
| DeliveryDate | DateTime? | No | Line delivery date |
| **Subscription Details** |
| IsSubscription | bool | Yes | Is subscription item |
| SubscriptionTermMonths | int | Yes | Term in months |
| SubscriptionStartDate | DateTime? | No | Subscription start |
| SubscriptionEndDate | DateTime? | No | Subscription end |
| BillingFrequency | string | No | Billing frequency |
| RecurringAmount | decimal | Yes | Recurring charge |
| **Relationships** |
| QuoteLineItemId | int? | No | Source quote line |
| **Audit Fields** |
| Notes | string | No | Line notes |
| SortOrder | int | Yes | Display order |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |
| IsDeleted | bool | Yes | Soft delete flag |
| **Navigation** |
| Order | Order | No | Parent order |
| Product | Product | No | Product navigation |

### 2.2 Interfaces

#### 2.2.1 IOrderService
**File:** `CRM.Backend/src/CRM.Core/Interfaces/IOrderService.cs`  
**Status:** ✅ Implemented  
**Line Count:** ~160 lines

**CRUD Operations:**
```csharp
Task<IEnumerable<Order>> GetAllAsync(int? customerId = null, OrderStatus? status = null, CancellationToken cancellationToken = default);
Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
```

**Order Operations:**
```csharp
Task<Order> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);
Task<Order> CreateFromOpportunityAsync(int opportunityId, CancellationToken cancellationToken = default);
Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
Task<Order> CloneOrderAsync(int orderId, CancellationToken cancellationToken = default);
```

**Status Management:**
```csharp
Task<Order> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
Task<Order> SubmitForApprovalAsync(int orderId, CancellationToken cancellationToken = default);
Task<Order> ApproveAsync(int orderId, int approvedById, CancellationToken cancellationToken = default);
Task<Order> RejectAsync(int orderId, int rejectedById, string reason, CancellationToken cancellationToken = default);
Task<Order> CancelAsync(int orderId, string reason, CancellationToken cancellationToken = default);
Task<Order> PutOnHoldAsync(int orderId, string reason, CancellationToken cancellationToken = default);
Task<Order> ReleaseFromHoldAsync(int orderId, CancellationToken cancellationToken = default);
```

**Fulfillment:**
```csharp
Task<Order> MarkAsFulfilledAsync(int orderId, CancellationToken cancellationToken = default);
Task<Order> MarkAsPartiallyFulfilledAsync(int orderId, IEnumerable<int> fulfilledLineItemIds, CancellationToken cancellationToken = default);
Task<Order> MarkAsDeliveredAsync(int orderId, CancellationToken cancellationToken = default);
Task<Order> ProcessReturnAsync(int orderId, IEnumerable<OrderReturnItem> returnItems, string reason, CancellationToken cancellationToken = default);
```

**Line Items:**
```csharp
Task<OrderLineItem> AddLineItemAsync(int orderId, OrderLineItem lineItem, CancellationToken cancellationToken = default);
Task<OrderLineItem> UpdateLineItemAsync(OrderLineItem lineItem, CancellationToken cancellationToken = default);
Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default);
Task<IEnumerable<OrderLineItem>> GetLineItemsAsync(int orderId, CancellationToken cancellationToken = default);
```

**Queries:**
```csharp
Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
Task<IEnumerable<Order>> GetOrdersRequiringActionAsync(CancellationToken cancellationToken = default);
Task<OrderStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
Task<IEnumerable<Order>> SearchAsync(string query, CancellationToken cancellationToken = default);
```

**Calculations:**
```csharp
Task<Order> RecalculateTotalsAsync(int orderId, CancellationToken cancellationToken = default);
Task<Order> ApplyDiscountAsync(int orderId, decimal discountAmount, string? discountReason = null, CancellationToken cancellationToken = default);
Task<Order> ApplyCouponAsync(int orderId, string couponCode, CancellationToken cancellationToken = default);
```

**Invoicing:**
```csharp
Task<Invoice> CreateInvoiceAsync(int orderId, CancellationToken cancellationToken = default);
Task<IEnumerable<Invoice>> GetInvoicesAsync(int orderId, CancellationToken cancellationToken = default);
```

### 2.3 Supporting DTOs

#### OrderReturnItem
```csharp
public class OrderReturnItem
{
    public int OrderLineItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Condition { get; set; }
}
```

#### OrderStatistics
```csharp
public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int FulfilledOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public double FulfillmentRate { get; set; }
    public double AverageFulfillmentTime { get; set; }
    public Dictionary<OrderType, int> OrdersByType { get; set; } = new();
}
```

### 2.4 Services

#### 2.4.1 OrderService
**File:** `CRM.Backend/src/CRM.Infrastructure/Services/OrderService.cs`  
**Status:** ✅ Implemented  
**Line Count:** ~870 lines

**Key Implementation Details:**

1. **Order Number Generation:**
   - Format: `ORD-{YY}{MM}-{####}` (e.g., ORD-2602-0001)
   - Sequence resets monthly
   - Thread-safe generation

2. **CreateFromQuoteAsync:**
   - Copies all quote header fields (account, contact, pricing, addresses)
   - Copies all quote line items with quantities and pricing
   - Links back to source quote via QuoteId

3. **CreateFromOpportunityAsync:**
   - Copies opportunity account and contact
   - Copies opportunity products as line items
   - Sets order total from opportunity amount

4. **Status Transition Rules:**
   - Draft → PendingApproval (via SubmitForApproval)
   - PendingApproval → Approved/Draft (via Approve/Reject)
   - Approved → Processing → Fulfilled → Delivered → Completed
   - Any non-fulfilled → Cancelled (with reason)
   - Any non-cancelled → OnHold (with reason)

5. **ProcessReturnAsync:**
   - Updates ReturnedQuantity on each line item
   - Sets order status to Returned if all items fully returned
   - Records RMA number and return reason

6. **RecalculateTotalsAsync:**
   - Subtotal = Sum of line item totals
   - TotalAmount = Subtotal - DiscountAmount + TaxAmount + ShippingAmount

7. **CreateInvoiceAsync:**
   - Creates Invoice entity from order header
   - Creates InvoiceLineItem for each OrderLineItem
   - Links invoice back to order

### 2.5 Controllers

#### 2.5.1 OrdersController
**File:** `CRM.Backend/src/CRM.Api/Controllers/OrdersController.cs`  
**Status:** ❌ NOT IMPLEMENTED

**Required Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/orders | List orders with filtering |
| GET | /api/orders/{id} | Get order by ID |
| GET | /api/orders/number/{orderNumber} | Get order by order number |
| POST | /api/orders | Create order |
| PUT | /api/orders/{id} | Update order |
| DELETE | /api/orders/{id} | Delete order (soft) |
| POST | /api/orders/from-quote/{quoteId} | Create from quote |
| POST | /api/orders/from-opportunity/{opportunityId} | Create from opportunity |
| POST | /api/orders/{id}/clone | Clone order |
| POST | /api/orders/{id}/submit | Submit for approval |
| POST | /api/orders/{id}/approve | Approve order |
| POST | /api/orders/{id}/reject | Reject order |
| POST | /api/orders/{id}/cancel | Cancel order |
| POST | /api/orders/{id}/hold | Put on hold |
| POST | /api/orders/{id}/release-hold | Release from hold |
| POST | /api/orders/{id}/fulfill | Mark fulfilled |
| POST | /api/orders/{id}/partial-fulfill | Mark partially fulfilled |
| POST | /api/orders/{id}/deliver | Mark delivered |
| POST | /api/orders/{id}/return | Process return |
| GET | /api/orders/{id}/line-items | Get line items |
| POST | /api/orders/{id}/line-items | Add line item |
| PUT | /api/orders/line-items/{lineItemId} | Update line item |
| DELETE | /api/orders/line-items/{lineItemId} | Remove line item |
| POST | /api/orders/{id}/recalculate | Recalculate totals |
| POST | /api/orders/{id}/discount | Apply discount |
| POST | /api/orders/{id}/coupon | Apply coupon |
| POST | /api/orders/{id}/invoice | Create invoice |
| GET | /api/orders/{id}/invoices | Get invoices |
| GET | /api/orders/status/{status} | Get by status |
| GET | /api/orders/statistics | Get statistics |
| GET | /api/orders/requiring-action | Get orders requiring action |
| GET | /api/orders/search | Search orders |

### 2.6 Validations

| Field | Rule | Backend | Frontend |
|-------|------|---------|----------|
| OrderNumber | Required, Unique, Auto-generated | ✅ | N/A |
| OrderType | Required, Valid enum | ✅ | ⏳ |
| Status | Required, Valid enum, Valid transition | ✅ | ⏳ |
| AccountId | Required | ✅ | ⏳ |
| OrderDate | Required | ✅ | ⏳ |
| LineItems | At least one required | ✅ | ⏳ |
| TotalAmount | Calculated, non-negative | ✅ | ⏳ |
| HoldReason | Required when OnHold | ✅ | ⏳ |
| CancellationReason | Required when Cancelled | ✅ | ⏳ |
| ApprovedById | Required when Approved | ✅ | ⏳ |
| RejectionReason | Required when Rejected | ✅ | ⏳ |

---

## 3. Frontend Implementation

### 3.1 Services

#### 3.1.1 orderService.ts
**File:** `CRM.Frontend/src/services/orderService.ts`  
**Status:** ❌ NOT IMPLEMENTED

**Required Methods:**
```typescript
interface OrderService {
  // CRUD
  getAll(customerId?: number, status?: OrderStatus): Promise<Order[]>;
  getById(id: number): Promise<Order>;
  getByOrderNumber(orderNumber: string): Promise<Order>;
  create(order: OrderCreateDto): Promise<Order>;
  update(id: number, order: OrderUpdateDto): Promise<Order>;
  delete(id: number): Promise<void>;
  
  // Order Operations
  createFromQuote(quoteId: number): Promise<Order>;
  createFromOpportunity(opportunityId: number): Promise<Order>;
  clone(orderId: number): Promise<Order>;
  
  // Status Management
  submitForApproval(orderId: number): Promise<Order>;
  approve(orderId: number): Promise<Order>;
  reject(orderId: number, reason: string): Promise<Order>;
  cancel(orderId: number, reason: string): Promise<Order>;
  putOnHold(orderId: number, reason: string): Promise<Order>;
  releaseFromHold(orderId: number): Promise<Order>;
  
  // Fulfillment
  markFulfilled(orderId: number): Promise<Order>;
  markPartiallyFulfilled(orderId: number, lineItemIds: number[]): Promise<Order>;
  markDelivered(orderId: number): Promise<Order>;
  processReturn(orderId: number, items: OrderReturnItem[], reason: string): Promise<Order>;
  
  // Line Items
  getLineItems(orderId: number): Promise<OrderLineItem[]>;
  addLineItem(orderId: number, lineItem: LineItemCreateDto): Promise<OrderLineItem>;
  updateLineItem(lineItemId: number, lineItem: LineItemUpdateDto): Promise<OrderLineItem>;
  removeLineItem(lineItemId: number): Promise<void>;
  
  // Calculations
  recalculateTotals(orderId: number): Promise<Order>;
  applyDiscount(orderId: number, amount: number, reason?: string): Promise<Order>;
  applyCoupon(orderId: number, couponCode: string): Promise<Order>;
  
  // Invoicing
  createInvoice(orderId: number): Promise<Invoice>;
  getInvoices(orderId: number): Promise<Invoice[]>;
  
  // Queries
  getByStatus(status: OrderStatus): Promise<Order[]>;
  getStatistics(fromDate?: Date, toDate?: Date): Promise<OrderStatistics>;
  getRequiringAction(): Promise<Order[]>;
  search(query: string): Promise<Order[]>;
}
```

### 3.2 Pages

#### 3.2.1 OrdersPage.tsx
**File:** `CRM.Frontend/src/pages/OrdersPage.tsx`  
**Status:** ❌ NOT IMPLEMENTED

**Required Features:**
- Order list with DataGrid
- Status filter chips
- Date range filter
- Account filter
- Quick actions (Approve, Fulfill, Cancel)
- Create order button
- Export to CSV/PDF

#### 3.2.2 OrderDetailsPage.tsx
**File:** `CRM.Frontend/src/pages/OrderDetailsPage.tsx`  
**Status:** ❌ NOT IMPLEMENTED

**Required Sections:**
- Order header (number, status, dates)
- Account/Contact information
- Line items table with edit capability
- Pricing summary
- Billing/Shipping addresses
- Status history timeline
- Related invoices
- Action buttons based on current status

### 3.3 Components

| Component | Status | Description |
|-----------|--------|-------------|
| OrderForm.tsx | ❌ Missing | Create/Edit order form |
| OrderLineItemsTable.tsx | ❌ Missing | Editable line items grid |
| OrderStatusBadge.tsx | ❌ Missing | Status chip with color coding |
| OrderTimeline.tsx | ❌ Missing | Status change history |
| OrderSummary.tsx | ❌ Missing | Pricing summary card |
| OrderAddressCard.tsx | ❌ Missing | Billing/Shipping address display |
| OrderActionButtons.tsx | ❌ Missing | Context-aware action buttons |
| OrderStatisticsCard.tsx | ❌ Missing | Dashboard statistics |

---

## 4. Database

### 4.1 Tables

#### Orders Table
**Status:** ✅ Implemented via EF Core

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, Identity |
| OrderNumber | VARCHAR(50) | NOT NULL, UNIQUE |
| ExternalOrderId | VARCHAR(100) | NULL |
| OrderType | INT | NOT NULL, DEFAULT 0 |
| Status | INT | NOT NULL, DEFAULT 0 |
| Priority | INT | NOT NULL, DEFAULT 0 |
| FulfillmentMethod | INT | NOT NULL, DEFAULT 0 |
| Description | TEXT | NULL |
| InternalNotes | TEXT | NULL |
| CustomerPONumber | VARCHAR(50) | NULL |
| Terms | VARCHAR(500) | NULL |
| OrderDate | DATETIME | NOT NULL |
| RequestedDeliveryDate | DATETIME | NULL |
| PromisedDeliveryDate | DATETIME | NULL |
| ActualDeliveryDate | DATETIME | NULL |
| ... (80+ columns) ... |
| CreatedAt | DATETIME | NOT NULL, DEFAULT NOW() |
| UpdatedAt | DATETIME | NULL |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 |
| RowVersion | BINARY(8) | NULL |

#### OrderLineItems Table
**Status:** ✅ Implemented via EF Core

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, Identity |
| OrderId | INT | FK → Orders.Id, NOT NULL |
| LineNumber | INT | NOT NULL |
| ProductId | INT | FK → Products.Id, NULL |
| ProductCode | VARCHAR(50) | NULL |
| ProductName | VARCHAR(200) | NOT NULL |
| Description | TEXT | NULL |
| Quantity | DECIMAL(18,4) | NOT NULL |
| UnitPrice | DECIMAL(18,4) | NOT NULL |
| ... (40+ columns) ... |
| CreatedAt | DATETIME | NOT NULL, DEFAULT NOW() |
| UpdatedAt | DATETIME | NULL |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 |

### 4.2 Indexes

| Index | Table | Columns | Type |
|-------|-------|---------|------|
| IX_Orders_OrderNumber | Orders | OrderNumber | UNIQUE |
| IX_Orders_AccountId | Orders | AccountId | NON-UNIQUE |
| IX_Orders_Status | Orders | Status | NON-UNIQUE |
| IX_Orders_OrderDate | Orders | OrderDate | NON-UNIQUE |
| IX_Orders_QuoteId | Orders | QuoteId | NON-UNIQUE |
| IX_OrderLineItems_OrderId | OrderLineItems | OrderId | NON-UNIQUE |
| IX_OrderLineItems_ProductId | OrderLineItems | ProductId | NON-UNIQUE |

### 4.3 Foreign Keys

| FK | From | To |
|----|------|-----|
| FK_Orders_Quotes | Orders.QuoteId | Quotes.Id |
| FK_Orders_Accounts | Orders.AccountId | Customers.Id |
| FK_Orders_Contacts | Orders.ContactId | Contacts.Id |
| FK_Orders_Opportunities | Orders.OpportunityId | Opportunities.Id |
| FK_Orders_Users_Owner | Orders.OwnerId | Users.Id |
| FK_Orders_Users_Approver | Orders.ApprovedById | Users.Id |
| FK_Orders_Parent | Orders.ParentOrderId | Orders.Id |
| FK_OrderLineItems_Orders | OrderLineItems.OrderId | Orders.Id |
| FK_OrderLineItems_Products | OrderLineItems.ProductId | Products.Id |

---

## 5. Testing

### 5.1 Unit Tests

**File:** `CRM.Backend/tests/CRM.Tests/Services/OrderServiceTests.cs`  
**Status:** ⚠️ Partial (test file exists for controller)

| Test | Description | Status |
|------|-------------|--------|
| GetAllAsync_ReturnsAllOrders | Returns all non-deleted orders | ⏳ |
| GetByIdAsync_ExistingOrder_ReturnsOrder | Returns order by ID | ⏳ |
| GetByOrderNumberAsync_ReturnsOrder | Returns order by number | ⏳ |
| CreateAsync_ValidOrder_CreatesOrder | Creates new order | ⏳ |
| CreateFromQuoteAsync_CopiesQuoteData | Copies quote to order | ⏳ |
| CreateFromOpportunityAsync_CopiesOppData | Copies opp to order | ⏳ |
| ApproveAsync_ValidOrder_ApprovesOrder | Approves pending order | ⏳ |
| RejectAsync_SetsRejectionReason | Records rejection reason | ⏳ |
| CancelAsync_NonFulfilledOrder_Cancels | Cancels order | ⏳ |
| CancelAsync_FulfilledOrder_ThrowsException | Prevents fulfilled cancel | ⏳ |
| ProcessReturnAsync_UpdatesQuantities | Updates return quantities | ⏳ |
| RecalculateTotalsAsync_CorrectCalculation | Calculates totals correctly | ⏳ |

### 5.2 Integration Tests

**File:** `CRM.Backend/tests/Controllers/OrdersControllerTests.cs`  
**Status:** ✅ File exists (tests for non-existent controller)

### 5.3 E2E Tests

**File:** `e2e-tests/tests/orders/orders.spec.ts`  
**Status:** ❌ NOT IMPLEMENTED

---

## 6. Issues & Gaps

### 6.1 Critical Gaps

| ID | Issue | Impact | Resolution |
|----|-------|--------|------------|
| GAP-001 | No OrdersController | API endpoints unavailable | Create controller |
| GAP-002 | No frontend pages | Cannot manage orders in UI | Create OrdersPage, OrderDetailsPage |
| GAP-003 | No frontend service | Cannot call API | Create orderService.ts |

### 6.2 Naming Inconsistencies

| Location | Issue | Recommendation |
|----------|-------|----------------|
| - | No issues identified | - |

### 6.3 Validation Gaps

| Field | Missing Validation | Recommendation |
|-------|-------------------|----------------|
| Currency | No currency code validation | Validate against ISO 4217 |
| Email fields | No email format validation | Add email regex validation |
| Postal codes | No format validation | Add country-specific validation |

---

## 7. TODOs

### High Priority

| ID | Task | Effort | Assigned |
|----|------|--------|----------|
| TODO-SALES002-001 | Create OrdersController.cs | 8 hrs | - |
| TODO-SALES002-002 | Create orderService.ts | 4 hrs | - |
| TODO-SALES002-003 | Create OrdersPage.tsx | 8 hrs | - |
| TODO-SALES002-004 | Create OrderDetailsPage.tsx | 8 hrs | - |

### Medium Priority

| ID | Task | Effort | Assigned |
|----|------|--------|----------|
| TODO-SALES002-005 | Create OrderForm.tsx | 4 hrs | - |
| TODO-SALES002-006 | Create OrderLineItemsTable.tsx | 4 hrs | - |
| TODO-SALES002-007 | Create OrderStatusBadge.tsx | 1 hr | - |
| TODO-SALES002-008 | Create OrderTimeline.tsx | 2 hrs | - |
| TODO-SALES002-009 | Add currency validation | 1 hr | - |
| TODO-SALES002-010 | Add email format validation | 1 hr | - |

### Low Priority

| ID | Task | Effort | Assigned |
|----|------|--------|----------|
| TODO-SALES002-011 | Create E2E tests | 4 hrs | - |
| TODO-SALES002-012 | Add order export (CSV/PDF) | 4 hrs | - |
| TODO-SALES002-013 | Add order cloning UI | 2 hrs | - |

---

## 8. Appendix

### A. Order Status State Machine

```
                    ┌─────────────┐
                    │    Draft    │
                    └──────┬──────┘
                           │ submit
                           ▼
                    ┌─────────────┐
            ┌───────│  Pending    │───────┐
            │reject │  Approval   │approve│
            │       └─────────────┘       │
            ▼                             ▼
     ┌─────────────┐               ┌─────────────┐
     │    Draft    │               │  Approved   │
     └─────────────┘               └──────┬──────┘
                                          │ process
                                          ▼
                                   ┌─────────────┐
                         ┌─────────│ Processing  │─────────┐
                         │partial  └──────┬──────┘ fulfill │
                         ▼                │                ▼
                  ┌─────────────┐         │         ┌─────────────┐
                  │  Partially  │─────────┘         │  Fulfilled  │
                  │  Fulfilled  │                   └──────┬──────┘
                  └─────────────┘                          │ deliver
                                                          ▼
                                                   ┌─────────────┐
                                                   │  Delivered  │
                                                   └──────┬──────┘
                                              complete│    │return
                                                     ▼    ▼
                                              ┌─────────────┐
                                              │  Completed  │
                                              │  /Returned  │
                                              └─────────────┘

    Any state (except Fulfilled+) ──cancel──► Cancelled
    Any state (except Cancelled) ──hold──► OnHold ──release──► Previous State
```

### B. Related Specifications

- [SPEC-SALES-001](SPEC-SALES-001-QuoteManagement.md) - Quote Management (source for orders)
- [SPEC-SALES-003](SPEC-SALES-003-InvoiceManagement.md) - Invoice Management (created from orders)
- [SPEC-CRM-001](SPEC-CRM-001-AccountManagement.md) - Account Management (order customer)
- [SPEC-CRM-003](SPEC-CRM-003-OpportunityManagement.md) - Opportunity Management (source for orders)

---

**END OF SPECIFICATION**
