# SPEC-SALES-002: Order Management

> **Version:** 2.0  
> **Last Updated:** February 2026  
> **Status:** ✅ Complete  
> **Module:** Sales  
> **Priority:** P1  
> **Dependencies:** SPEC-SALES-001 (Quote Management), SPEC-CRM-001 (Account Management), SPEC-CRM-003 (Opportunity Management)

---

## 1. Business Context

### 1.1 Overview

Order Management provides comprehensive functionality for creating, processing, and fulfilling customer orders. The system supports the full order lifecycle from creation through fulfillment and delivery, including approval workflows, line item management, return processing, and invoice generation. Orders can be created manually, from accepted quotes, or from won opportunities.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Order Creation | Create orders manually, from quotes, or from opportunities | ✅ Implemented |
| SF-002 | Order Numbering | Auto-generation of order numbers (ORD-YYMM-####) | ✅ Implemented |
| SF-003 | Line Item Management | Add/edit/delete order line items with products | ✅ Implemented |
| SF-004 | Pricing Calculations | Automatic totals, discounts, tax, shipping, commissions | ✅ Implemented |
| SF-005 | Order Lifecycle | Draft → Approved → Processing → Fulfilled → Delivered → Completed | ✅ Implemented |
| SF-006 | Approval Workflow | Submit orders for approval before processing | ✅ Implemented |
| SF-007 | Fulfillment Tracking | Track full and partial order fulfillment | ✅ Implemented |
| SF-008 | Return Processing | Process product returns with RMA tracking | ✅ Implemented |
| SF-009 | Invoice Generation | Create invoices from completed orders | ✅ Implemented |
| SF-010 | Address Management | Billing and shipping address handling | ✅ Implemented |
| SF-011 | Order Cloning | Clone existing orders for repeat business | ✅ Implemented |
| SF-012 | Revenue Recognition | Revenue recognition scheduling and tracking | ✅ Implemented |
| SF-013 | Hold Management | Place orders on hold with reason tracking | ✅ Implemented |

### 1.3 Functionalities

| ID | Functionality | Description | Status |
|----|---------------|-------------|--------|
| F-001 | List Orders | Display all orders with filtering by account, status, date range | ✅ Implemented |
| F-002 | Create Order | Create new order with header and line items | ✅ Implemented |
| F-003 | Edit Order | Modify order details (restricted by status) | ✅ Implemented |
| F-004 | Delete Order | Soft delete (Draft status only) | ✅ Implemented |
| F-005 | Create from Quote | Generate order from accepted quote with line items | ✅ Implemented |
| F-006 | Create from Opportunity | Generate order from won opportunity | ✅ Implemented |
| F-007 | Clone Order | Duplicate order for repeat business | ✅ Implemented |
| F-008 | Submit for Approval | Submit draft order for approval | ✅ Implemented |
| F-009 | Approve / Reject | Approve or reject pending orders | ✅ Implemented |
| F-010 | Cancel Order | Cancel order with reason tracking | ✅ Implemented |
| F-011 | Put on Hold | Place order on hold with reason | ✅ Implemented |
| F-012 | Mark Fulfilled | Record full or partial fulfillment | ✅ Implemented |
| F-013 | Mark Delivered | Record order delivery | ✅ Implemented |
| F-014 | Process Return | Record returns with RMA tracking | ✅ Implemented |
| F-015 | Generate Invoice | Create invoice from order | ✅ Implemented |
| F-016 | Recalculate Totals | Recalculate order pricing from line items | ✅ Implemented |
| F-017 | Apply Discount | Apply discount amount to order | ✅ Implemented |
| F-018 | Apply Coupon | Apply coupon code to order | ✅ Implemented |
| F-019 | Get Statistics | Calculate order metrics (counts, revenue, fulfillment rates) | ✅ Implemented |
| F-020 | Search Orders | Full-text search across order fields | ✅ Implemented |

### 1.4 Use Cases

| ID | Use Case | Actor | Precondition | Steps | Postcondition |
|----|----------|-------|--------------|-------|---------------|
| UC-001 | Create Order from Quote | Sales Rep | Quote is accepted | 1. Navigate to quote 2. Click "Create Order" 3. Review mapped data 4. Submit | Order created with Draft status |
| UC-002 | Submit Order for Approval | Sales Rep | Order in Draft | 1. Open order 2. Click "Submit for Approval" | Status → PendingApproval |
| UC-003 | Approve Order | Manager | Order in PendingApproval | 1. Review order details 2. Click "Approve" | Status → Approved, ApprovedById set |
| UC-004 | Reject Order | Manager | Order in PendingApproval | 1. Review order 2. Enter rejection reason 3. Click "Reject" | Status → Draft, RejectionReason recorded |
| UC-005 | Fulfill Order | Warehouse | Order Approved/Processing | 1. Select line items 2. Enter quantities fulfilled 3. Confirm | Status → Fulfilled or PartiallyFulfilled |
| UC-006 | Process Return | Support | Order Fulfilled/Delivered | 1. Select return items 2. Enter quantities and reasons 3. Submit | ReturnedQuantity updated, RMA generated |
| UC-007 | Generate Invoice | Finance | Order Fulfilled | 1. Open order 2. Click "Create Invoice" | Invoice created from order data |
| UC-008 | Cancel Order | Sales Rep | Order not yet fulfilled | 1. Open order 2. Enter cancellation reason 3. Confirm | Status → Cancelled |
| UC-009 | Put Order on Hold | Manager | Order not cancelled | 1. Open order 2. Enter hold reason 3. Confirm | IsOnHold = true, HoldReason set |
| UC-010 | Release from Hold | Manager | Order on hold | 1. Open order 2. Click "Release Hold" | IsOnHold = false, HoldReleasedDate set |
| UC-011 | Clone Order | Sales Rep | Any existing order | 1. Open order 2. Click "Clone" | New Draft order with copied data |
| UC-012 | Apply Discount | Sales Rep | Order in Draft | 1. Open order 2. Enter discount amount/reason 3. Save | DiscountAmount updated, totals recalculated |

---

## 2. Frontend Implementation

### 2.1 Pages

| Page | File | Status | Description |
|------|------|--------|-------------|
| OrdersPage | `CRM.Frontend/src/pages/OrdersPage.tsx` | ✅ Implemented | Order list with DataGrid, filtering, status chips |
| OrderDetailsPage | `CRM.Frontend/src/pages/OrderDetailsPage.tsx` | ❌ Not Implemented | Order detail view with tabs |

### 2.2 Services

#### 2.2.1 orderService.ts
**File:** `CRM.Frontend/src/services/orderService.ts`  
**Status:** ✅ Implemented (193 lines)

**TypeScript Types:**

```typescript
export enum OrderStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Processing = 3,
  PartiallyFulfilled = 4,
  Fulfilled = 5,
  Shipped = 6,
  Delivered = 7,
  Completed = 8,
  Cancelled = 9,
  OnHold = 10,
  Returned = 11,
  Refunded = 12,
}

export interface Order {
  id: number;
  orderNumber: string;
  orderType: number;
  status: OrderStatus;
  priority: number;
  fulfillmentMethod: number;
  description: string;
  customerPONumber: string;
  orderDate: string;
  requestedDeliveryDate: string;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  shippingAmount: number;
  totalAmount: number;
  currency: string;
  accountId: number;
  contactId: number;
  quoteId: number;
  lineItems: OrderLineItem[];
}

export interface OrderLineItem { ... }
export interface OrderReturnItem { ... }
export interface OrderStatistics { ... }
```

**API Methods (35 total):**

| Method | HTTP | Endpoint | Status |
|--------|------|----------|--------|
| getAll | GET | /api/orders | ✅ |
| getById | GET | /api/orders/{id} | ✅ |
| getByOrderNumber | GET | /api/orders/by-number/{orderNumber} | ✅ |
| create | POST | /api/orders | ✅ |
| update | PUT | /api/orders/{id} | ✅ |
| delete | DELETE | /api/orders/{id} | ✅ |
| createFromQuote | POST | /api/orders/from-quote/{quoteId} | ✅ |
| createFromOpportunity | POST | /api/orders/from-opportunity/{opportunityId} | ✅ |
| generateOrderNumber | GET | /api/orders/generate-number | ✅ |
| clone | POST | /api/orders/{id}/clone | ✅ |
| updateStatus | PATCH | /api/orders/{id}/status | ✅ |
| submitForApproval | POST | /api/orders/{id}/submit | ✅ |
| approve | POST | /api/orders/{id}/approve | ✅ |
| reject | POST | /api/orders/{id}/reject | ✅ |
| cancel | POST | /api/orders/{id}/cancel | ✅ |
| putOnHold | POST | /api/orders/{id}/hold | ✅ |
| releaseFromHold | POST | /api/orders/{id}/release | ✅ |
| markFulfilled | POST | /api/orders/{id}/fulfill | ✅ |
| markPartiallyFulfilled | POST | /api/orders/{id}/partial-fulfill | ✅ |
| markDelivered | POST | /api/orders/{id}/deliver | ✅ |
| processReturn | POST | /api/orders/{id}/return | ✅ |
| addLineItem | POST | /api/orders/{id}/line-items | ✅ |
| updateLineItem | PUT | /api/orders/line-items/{lineItemId} | ✅ |
| removeLineItem | DELETE | /api/orders/line-items/{lineItemId} | ✅ |
| getLineItems | GET | /api/orders/{id}/line-items | ✅ |
| getByStatus | GET | /api/orders/by-status/{status} | ✅ |
| getByDateRange | GET | /api/orders/by-date-range | ✅ |
| getRequiringAction | GET | /api/orders/requiring-action | ✅ |
| getStatistics | GET | /api/orders/statistics | ✅ |
| search | GET | /api/orders/search | ✅ |
| recalculateTotals | POST | /api/orders/{id}/recalculate | ✅ |
| applyDiscount | POST | /api/orders/{id}/discount | ✅ |
| applyCoupon | POST | /api/orders/{id}/coupon | ✅ |
| createInvoice | POST | /api/orders/{id}/invoice | ✅ |
| getInvoices | GET | /api/orders/{id}/invoices | ✅ |

**Helper Functions:**
- `getOrderStatusLabel(status: OrderStatus): string` — Returns display label
- `getOrderStatusColor(status: OrderStatus): string` — Returns MUI color

### 2.3 Components

| Component | Status | Description |
|-----------|--------|-------------|
| OrderForm.tsx | ❌ Not Implemented | Create/Edit order form |
| OrderLineItemsTable.tsx | ❌ Not Implemented | Editable line items grid |
| OrderStatusBadge.tsx | ❌ Not Implemented | Status chip with color coding |
| OrderTimeline.tsx | ❌ Not Implemented | Status change history |
| OrderSummary.tsx | ❌ Not Implemented | Pricing summary card |
| OrderAddressCard.tsx | ❌ Not Implemented | Billing/Shipping address display |
| OrderActionButtons.tsx | ❌ Not Implemented | Context-aware action buttons |
| OrderStatisticsCard.tsx | ❌ Not Implemented | Dashboard statistics |

### 2.4 Frontend Validation Rules

| Field | Validation | Error Message |
|-------|------------|---------------|
| AccountId | Required | "Account is required" |
| OrderDate | Required | "Order date is required" |
| OrderType | Required, valid enum | "Order type is required" |
| LineItems | Min 1 required | "At least one line item is required" |
| Quantity | > 0 | "Quantity must be greater than 0" |
| UnitPrice | >= 0 | "Unit price cannot be negative" |
| HoldReason | Required when OnHold | "Hold reason is required" |
| CancellationReason | Required when Cancelled | "Cancellation reason is required" |
| RejectionReason | Required when Rejected | "Rejection reason is required" |

---

## 3. Backend Implementation

### 3.1 Entities

#### 3.1.1 Order Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs`  
**Status:** ✅ Implemented (~450 lines)

**Enumerations:**

**OrderStatus** (13 values):

| Value | Int | Description |
|-------|-----|-------------|
| Draft | 0 | Order created, not submitted |
| PendingApproval | 1 | Awaiting approval |
| Approved | 2 | Approved for processing |
| Processing | 3 | Being processed |
| PartiallyFulfilled | 4 | Some items fulfilled |
| Fulfilled | 5 | All items fulfilled |
| Shipped | 6 | Shipped to customer |
| Delivered | 7 | Delivered to customer |
| Completed | 8 | Order completed |
| Cancelled | 9 | Order cancelled |
| OnHold | 10 | Order on hold |
| Returned | 11 | Order returned |
| Refunded | 12 | Order refunded |

**OrderType** (12 values):

| Value | Int | Description |
|-------|-----|-------------|
| Standard | 0 | Standard order |
| Renewal | 1 | Renewal order |
| Amendment | 2 | Contract amendment |
| Upgrade | 3 | Product upgrade |
| Downgrade | 4 | Product downgrade |
| AddOn | 5 | Add-on purchase |
| Replacement | 6 | Replacement order |
| Trial | 7 | Trial order |
| Sample | 8 | Sample order |
| Return | 9 | Return order |
| Credit | 10 | Credit order |
| MultiYear | 11 | Multi-year order |

**FulfillmentMethod** (8 values):

| Value | Int | Description |
|-------|-----|-------------|
| Ship | 0 | Physical shipping |
| Digital | 1 | Digital delivery |
| Pickup | 2 | Customer pickup |
| Provision | 3 | Service provisioning |
| Activate | 4 | License activation |
| ServiceDelivery | 5 | Professional services |
| ThirdParty | 6 | Third-party fulfillment |
| None | 7 | No fulfillment required |

**OrderPriority** (5 values):

| Value | Int | Description |
|-------|-----|-------------|
| Normal | 0 | Normal priority |
| High | 1 | High priority |
| Urgent | 2 | Urgent |
| Low | 3 | Low priority |
| Critical | 4 | Critical |

**Entity Properties (80+):**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **Identification** ||||
| Id | int | Yes | Primary key |
| OrderNumber | string | Yes | Unique order number (ORD-YYMM-####) |
| ExternalOrderId | string | No | External system reference |
| **Order Details** ||||
| OrderType | OrderType | Yes | Type of order |
| Status | OrderStatus | Yes | Current order status |
| Priority | OrderPriority | Yes | Order priority level |
| FulfillmentMethod | FulfillmentMethod | Yes | How order is fulfilled |
| Description | string | No | Order description |
| InternalNotes | string | No | Internal notes |
| CustomerPONumber | string | No | Customer purchase order number |
| Terms | string | No | Payment/delivery terms |
| **Date Fields** ||||
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
| **Pricing (15 fields)** ||||
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
| **Revenue Recognition** ||||
| RecognitionMethod | string | No | Revenue recognition method |
| RecognitionStartDate | DateTime? | No | Recognition start |
| RecognitionEndDate | DateTime? | No | Recognition end |
| RecognizedRevenue | decimal | Yes | Recognized revenue amount |
| DeferredRevenue | decimal | Yes | Deferred revenue amount |
| RevenueScheduleId | int? | No | Revenue schedule reference |
| **Billing Address (9 fields)** ||||
| BillingStreet | string | No | Billing street address |
| BillingCity | string | No | Billing city |
| BillingState | string | No | Billing state/province |
| BillingPostalCode | string | No | Billing postal code |
| BillingCountry | string | No | Billing country |
| BillingContactName | string | No | Billing contact name |
| BillingContactEmail | string | No | Billing contact email |
| BillingContactPhone | string | No | Billing contact phone |
| BillingNotes | string | No | Billing notes |
| **Shipping Address (10 fields)** ||||
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
| **Shipping Details** ||||
| TrackingNumber | string | No | Shipment tracking number |
| Carrier | string | No | Shipping carrier |
| EstimatedWeight | decimal | Yes | Estimated weight |
| ShippedWeight | decimal | Yes | Actual shipped weight |
| PackageCount | int | Yes | Number of packages |
| **Payment Information** ||||
| PaymentMethod | string | No | Payment method |
| PaymentStatus | string | No | Payment status |
| PaymentReference | string | No | Payment reference |
| PaidAmount | decimal | Yes | Amount paid |
| BalanceDue | decimal | Yes | Outstanding balance (computed) |
| PaymentTerms | string | No | Payment terms |
| **Relationships** ||||
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
| **Hold Status** ||||
| IsOnHold | bool | Yes | Whether order is on hold |
| HoldReason | string | No | Reason for hold |
| HoldDate | DateTime? | No | When put on hold |
| HoldReleasedDate | DateTime? | No | When released from hold |
| **Rejection** ||||
| RejectionReason | string | No | Reason for rejection |
| RejectedById | int? | No | Who rejected |
| **Return** ||||
| ReturnReason | string | No | Return reason |
| ReturnAuthorizationNumber | string | No | RMA number |
| ReturnDate | DateTime? | No | Return date |
| **Discount Codes** ||||
| DiscountCode | string | No | Applied discount code |
| CouponCode | string | No | Applied coupon code |
| **Audit Fields** ||||
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |
| IsDeleted | bool | Yes | Soft delete flag |
| RowVersion | byte[] | No | Optimistic concurrency |

**Computed Properties:**
- `BalanceDue` → `TotalAmount - PaidAmount`
- `IsPaid` → `BalanceDue <= 0 && TotalAmount > 0`

**Navigation Properties:**
- `Quote`, `Account`, `Contact`, `Opportunity`, `Owner`, `ApprovedBy`, `ParentOrder`
- `ChildOrders` (ICollection), `LineItems` (ICollection), `Invoice`, `Subscription`

#### 3.1.2 OrderLineItem Entity
**File:** `CRM.Backend/src/CRM.Core/Entities/Order.cs` (same file, ~200 lines)  
**Status:** ✅ Implemented

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | int | Yes | Primary key |
| OrderId | int | Yes | Parent order FK |
| LineNumber | int | Yes | Line sequence |
| ProductId | int? | No | Product reference |
| ProductCode | string | No | Product code |
| ProductName | string | Yes | Product name |
| Description | string | No | Line item description |
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
| QuantityShipped | decimal | Yes | Quantity shipped |
| QuantityBackordered | decimal | Yes | Backorder quantity |
| QuantityReturned | decimal | Yes | Returned quantity |
| FulfilledQuantity | decimal | Yes | Fulfilled quantity |
| RemainingQuantity | decimal | Yes | Remaining to fulfill (computed) |
| FulfillmentStatus | string | No | Line fulfillment status |
| ShipDate | DateTime? | No | Line ship date |
| DeliveryDate | DateTime? | No | Line delivery date |
| IsSubscription | bool | Yes | Is subscription item |
| SubscriptionTermMonths | int | Yes | Term in months |
| SubscriptionStartDate | DateTime? | No | Subscription start |
| SubscriptionEndDate | DateTime? | No | Subscription end |
| BillingFrequency | string | No | Billing frequency |
| RecurringAmount | decimal | Yes | Recurring charge |
| QuoteLineItemId | int? | No | Source quote line |
| Notes | string | No | Line notes |
| SortOrder | int | Yes | Display order |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |
| IsDeleted | bool | Yes | Soft delete flag |

### 3.2 DTOs

| DTO | Location | Status |
|-----|----------|--------|
| OrderDto | CRM.Core/DTOs/ | ❌ Not Implemented |
| CreateOrderDto | CRM.Core/DTOs/ | ❌ Not Implemented |
| UpdateOrderDto | CRM.Core/DTOs/ | ❌ Not Implemented |
| OrderLineItemDto | CRM.Core/DTOs/ | ❌ Not Implemented |

**Inline Supporting Types (defined in IOrderService.cs):**

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

### 3.3 Interfaces

#### 3.3.1 IOrderService
**File:** `CRM.Backend/src/CRM.Core/Interfaces/IOrderService.cs`  
**Status:** ✅ Implemented (~160 lines, 34 methods)

| Category | Method | Return Type |
|----------|--------|-------------|
| **CRUD** |||
| | GetAllAsync(customerId?, status?, ct) | Task\<IEnumerable\<Order>> |
| | GetByIdAsync(id, ct) | Task\<Order?> |
| | GetByOrderNumberAsync(orderNumber, ct) | Task\<Order?> |
| | CreateAsync(order, ct) | Task\<Order> |
| | UpdateAsync(order, ct) | Task\<Order> |
| | DeleteAsync(id, ct) | Task\<bool> |
| **Operations** |||
| | CreateFromQuoteAsync(quoteId, ct) | Task\<Order> |
| | CreateFromOpportunityAsync(opportunityId, ct) | Task\<Order> |
| | GenerateOrderNumberAsync(ct) | Task\<string> |
| | CloneOrderAsync(orderId, ct) | Task\<Order> |
| **Status** |||
| | UpdateStatusAsync(orderId, status, ct) | Task\<Order> |
| | SubmitForApprovalAsync(orderId, ct) | Task\<Order> |
| | ApproveAsync(orderId, approvedById, ct) | Task\<Order> |
| | RejectAsync(orderId, rejectedById, reason, ct) | Task\<Order> |
| | CancelAsync(orderId, reason, ct) | Task\<Order> |
| | PutOnHoldAsync(orderId, reason, ct) | Task\<Order> |
| | ReleaseFromHoldAsync(orderId, ct) | Task\<Order> |
| **Fulfillment** |||
| | MarkAsFulfilledAsync(orderId, ct) | Task\<Order> |
| | MarkAsPartiallyFulfilledAsync(orderId, lineItemIds, ct) | Task\<Order> |
| | MarkAsDeliveredAsync(orderId, ct) | Task\<Order> |
| | ProcessReturnAsync(orderId, returnItems, reason, ct) | Task\<Order> |
| **Line Items** |||
| | AddLineItemAsync(orderId, lineItem, ct) | Task\<OrderLineItem> |
| | UpdateLineItemAsync(lineItem, ct) | Task\<OrderLineItem> |
| | RemoveLineItemAsync(lineItemId, ct) | Task\<bool> |
| | GetLineItemsAsync(orderId, ct) | Task\<IEnumerable\<OrderLineItem>> |
| **Queries** |||
| | GetByStatusAsync(status, ct) | Task\<IEnumerable\<Order>> |
| | GetByDateRangeAsync(fromDate, toDate, ct) | Task\<IEnumerable\<Order>> |
| | GetOrdersRequiringActionAsync(ct) | Task\<IEnumerable\<Order>> |
| | GetStatisticsAsync(fromDate?, toDate?, ct) | Task\<OrderStatistics> |
| | SearchAsync(query, ct) | Task\<IEnumerable\<Order>> |
| **Calculations** |||
| | RecalculateTotalsAsync(orderId, ct) | Task\<Order> |
| | ApplyDiscountAsync(orderId, amount, reason?, ct) | Task\<Order> |
| | ApplyCouponAsync(orderId, couponCode, ct) | Task\<Order> |
| **Invoicing** |||
| | CreateInvoiceAsync(orderId, ct) | Task\<Invoice> |
| | GetInvoicesAsync(orderId, ct) | Task\<IEnumerable\<Invoice>> |

### 3.4 Services

#### 3.4.1 OrderService
**File:** `CRM.Backend/src/CRM.Infrastructure/Services/OrderService.cs`  
**Status:** ✅ Implemented (~870 lines)

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

### 3.5 Controllers

#### 3.5.1 OrdersController
**File:** `CRM.Backend/src/CRM.Api/Controllers/OrdersController.cs`  
**Status:** ✅ Implemented (35 endpoints)

| Method | Route | Description | Status |
|--------|-------|-------------|--------|
| GET | /api/orders | List orders with filtering | ✅ |
| GET | /api/orders/{id} | Get order by ID | ✅ |
| GET | /api/orders/by-number/{orderNumber} | Get by order number | ✅ |
| POST | /api/orders | Create order | ✅ |
| PUT | /api/orders/{id} | Update order | ✅ |
| DELETE | /api/orders/{id} | Soft delete order | ✅ |
| POST | /api/orders/from-quote/{quoteId} | Create from quote | ✅ |
| POST | /api/orders/from-opportunity/{opportunityId} | Create from opportunity | ✅ |
| GET | /api/orders/generate-number | Generate order number | ✅ |
| POST | /api/orders/{id}/clone | Clone order | ✅ |
| PATCH | /api/orders/{id}/status | Update status | ✅ |
| POST | /api/orders/{id}/submit | Submit for approval | ✅ |
| POST | /api/orders/{id}/approve | Approve order | ✅ |
| POST | /api/orders/{id}/reject | Reject order | ✅ |
| POST | /api/orders/{id}/cancel | Cancel order | ✅ |
| POST | /api/orders/{id}/hold | Put on hold | ✅ |
| POST | /api/orders/{id}/release | Release from hold | ✅ |
| POST | /api/orders/{id}/fulfill | Mark fulfilled | ✅ |
| POST | /api/orders/{id}/partial-fulfill | Mark partially fulfilled | ✅ |
| POST | /api/orders/{id}/deliver | Mark delivered | ✅ |
| POST | /api/orders/{id}/return | Process return | ✅ |
| POST | /api/orders/{id}/line-items | Add line item | ✅ |
| PUT | /api/orders/line-items/{lineItemId} | Update line item | ✅ |
| DELETE | /api/orders/line-items/{lineItemId} | Remove line item | ✅ |
| GET | /api/orders/{id}/line-items | Get line items | ✅ |
| GET | /api/orders/by-status/{status} | Get by status | ✅ |
| GET | /api/orders/by-date-range | Get by date range | ✅ |
| GET | /api/orders/requiring-action | Get orders requiring action | ✅ |
| GET | /api/orders/statistics | Get statistics | ✅ |
| GET | /api/orders/search | Search orders | ✅ |
| POST | /api/orders/{id}/recalculate | Recalculate totals | ✅ |
| POST | /api/orders/{id}/discount | Apply discount | ✅ |
| POST | /api/orders/{id}/coupon | Apply coupon | ✅ |
| POST | /api/orders/{id}/invoice | Create invoice | ✅ |
| GET | /api/orders/{id}/invoices | Get invoices | ✅ |

### 3.6 Backend Validations

| Field | Rule | Status |
|-------|------|--------|
| OrderNumber | Required, Unique, Auto-generated | ✅ |
| OrderType | Required, Valid enum | ✅ |
| Status | Required, Valid enum, Valid transition | ✅ |
| AccountId | Required | ✅ |
| OrderDate | Required | ✅ |
| LineItems | At least one required | ✅ |
| TotalAmount | Calculated, non-negative | ✅ |
| HoldReason | Required when OnHold | ✅ |
| CancellationReason | Required when Cancelled | ✅ |
| ApprovedById | Required when Approved | ✅ |
| RejectionReason | Required when Rejected | ✅ |

---

## 4. Database

### 4.1 Tables

#### 4.1.1 Orders Table
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
| ... (80+ columns total) ... | | |
| CreatedAt | DATETIME | NOT NULL, DEFAULT NOW() |
| UpdatedAt | DATETIME | NULL |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 |
| RowVersion | BINARY(8) | NULL |

#### 4.1.2 OrderLineItems Table
**Status:** ✅ Implemented via EF Core

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, Identity |
| OrderId | INT | FK → Orders.Id, NOT NULL |
| LineNumber | INT | NOT NULL |
| ProductId | INT | FK → Products.Id, NULL |
| ProductName | VARCHAR(200) | NOT NULL |
| Quantity | DECIMAL(18,4) | NOT NULL |
| UnitPrice | DECIMAL(18,4) | NOT NULL |
| ... (40+ columns) ... | | |
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

| Test | Description | Status |
|------|-------------|--------|
| GetAllAsync_ReturnsAllOrders | Returns all non-deleted orders | ❌ Not Implemented |
| GetByIdAsync_ExistingOrder_ReturnsOrder | Returns order by ID | ❌ Not Implemented |
| GetByOrderNumberAsync_ReturnsOrder | Returns order by number | ❌ Not Implemented |
| CreateAsync_ValidOrder_CreatesOrder | Creates new order | ❌ Not Implemented |
| CreateFromQuoteAsync_CopiesQuoteData | Copies quote to order | ❌ Not Implemented |
| CreateFromOpportunityAsync_CopiesOppData | Copies opp to order | ❌ Not Implemented |
| ApproveAsync_ValidOrder_ApprovesOrder | Approves pending order | ❌ Not Implemented |
| RejectAsync_SetsRejectionReason | Records rejection reason | ❌ Not Implemented |
| CancelAsync_NonFulfilledOrder_Cancels | Cancels order | ❌ Not Implemented |
| CancelAsync_FulfilledOrder_ThrowsException | Prevents fulfilled cancel | ❌ Not Implemented |
| ProcessReturnAsync_UpdatesQuantities | Updates return quantities | ❌ Not Implemented |
| RecalculateTotalsAsync_CorrectCalculation | Calculates totals correctly | ❌ Not Implemented |

### 5.2 Integration Tests

| Test | Description | Status |
|------|-------------|--------|
| OrdersController_GetAll_Returns200 | GET /api/orders returns list | ❌ Not Implemented |
| OrdersController_Create_Returns201 | POST /api/orders creates order | ❌ Not Implemented |
| OrdersController_CreateFromQuote_Returns201 | POST /api/orders/from-quote/{id} | ❌ Not Implemented |

### 5.3 E2E Tests

| Test | Description | Status |
|------|-------------|--------|
| should display orders list page | Navigate to /orders | ❌ Not Implemented |
| should create order from quote | Full create-from-quote workflow | ❌ Not Implemented |
| should approve and fulfill order | Approval + fulfillment workflow | ❌ Not Implemented |
| should process order return | Return processing workflow | ❌ Not Implemented |

---

## 6. Issues & Gaps

### 6.1 Missing Components

| Component | Type | Priority | Impact |
|-----------|------|----------|--------|
| OrderDto / CreateOrderDto / UpdateOrderDto | Backend DTO | P2 | Controller passes entities directly — DTOs would improve API contract |
| OrderDetailsPage.tsx | Frontend Page | P2 | Cannot view full order details in UI |
| OrderForm.tsx | Frontend Component | P2 | No dedicated create/edit form component |
| OrderLineItemsTable.tsx | Frontend Component | P2 | No reusable line items editor |

### 6.2 Validation Gaps

| Field | Missing Validation | Recommendation |
|-------|-------------------|----------------|
| Currency | No ISO 4217 currency code validation | Validate against currency code list |
| BillingContactEmail | No email format validation | Add email regex validation |
| ShippingContactEmail | No email format validation | Add email regex validation |
| BillingPostalCode | No format validation | Add country-specific validation |

---

## 7. TODOs

### High Priority

| ID | Task | Effort |
|----|------|--------|
| TODO-SALES002-001 | Create OrderDetailsPage.tsx | 8 hrs |
| TODO-SALES002-002 | Create OrderDto, CreateOrderDto, UpdateOrderDto | 4 hrs |

### Medium Priority

| ID | Task | Effort |
|----|------|--------|
| TODO-SALES002-003 | Create OrderForm.tsx component | 4 hrs |
| TODO-SALES002-004 | Create OrderLineItemsTable.tsx component | 4 hrs |
| TODO-SALES002-005 | Create OrderStatusBadge.tsx component | 1 hr |
| TODO-SALES002-006 | Create OrderTimeline.tsx component | 2 hrs |
| TODO-SALES002-007 | Add currency code validation | 1 hr |
| TODO-SALES002-008 | Add email format validation for billing/shipping | 1 hr |
| TODO-SALES002-009 | Create OrderServiceTests.cs unit tests | 4 hrs |

### Low Priority

| ID | Task | Effort |
|----|------|--------|
| TODO-SALES002-010 | Create E2E tests for order workflows | 4 hrs |
| TODO-SALES002-011 | Add order export (CSV/PDF) | 4 hrs |
| TODO-SALES002-012 | Add order cloning UI in OrderDetailsPage | 2 hrs |

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

| Spec ID | Name | Relationship |
|---------|------|--------------|
| [SPEC-SALES-001](SPEC-SALES-001-QuoteManagement.md) | Quote Management | Source for orders |
| [SPEC-SALES-003](SPEC-SALES-003-InvoiceManagement.md) | Invoice Management | Created from orders |
| [SPEC-CRM-001](SPEC-CRM-001-AccountManagement.md) | Account Management | Order customer |
| [SPEC-CRM-003](SPEC-CRM-003-OpportunityManagement.md) | Opportunity Management | Source for orders |

### C. Change History

| Date | Version | Changes |
|------|---------|---------|
| February 2026 | 1.0 | Initial specification created |
| February 2026 | 2.0 | Updated statuses — OrdersController ✅, orderService.ts ✅, OrdersPage.tsx ✅ now implemented. Restructured to match SPEC-TEMPLATE format (Frontend = Section 2, Backend = Section 3). |

---

**END OF SPECIFICATION**
