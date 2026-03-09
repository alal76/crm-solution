// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Models;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities;

#region Order Enumerations

/// <summary>
/// FUNCTIONAL: Order lifecycle status from creation to fulfillment.
/// TECHNICAL: Controls order workflow, billing triggers, and fulfillment.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order created but not yet submitted</summary>
    Draft = 0,

    /// <summary>Order submitted, pending approval</summary>
    PendingApproval = 1,

    /// <summary>Order approved, ready for processing</summary>
    Approved = 2,

    /// <summary>Order is being processed/fulfilled</summary>
    Processing = 3,

    /// <summary>Order partially shipped/fulfilled</summary>
    PartiallyFulfilled = 4,

    /// <summary>Order fully shipped/fulfilled</summary>
    Fulfilled = 5,

    /// <summary>Order delivered to customer</summary>
    Delivered = 6,

    /// <summary>Order completed successfully</summary>
    Completed = 7,

    /// <summary>Order cancelled before fulfillment</summary>
    Cancelled = 8,

    /// <summary>Order returned by customer</summary>
    Returned = 9,

    /// <summary>Order refunded</summary>
    Refunded = 10,

    /// <summary>Order on hold (payment/inventory issues)</summary>
    OnHold = 11,

    /// <summary>Order requires action (backordered, etc.)</summary>
    ActionRequired = 12
}

/// <summary>
/// Return status for order return workflow.
/// TODO-GAP-SALES-001: Complete order returns workflow
/// </summary>
public enum ReturnStatus
{
    /// <summary>No return requested</summary>
    None = 0,

    /// <summary>Return requested by customer</summary>
    Requested = 1,

    /// <summary>Return request approved</summary>
    Approved = 2,

    /// <summary>Return request rejected</summary>
    Rejected = 3,

    /// <summary>Items shipped back</summary>
    ItemsShipped = 4,

    /// <summary>Items received</summary>
    ItemsReceived = 5,

    /// <summary>Quality inspection in progress</summary>
    Inspecting = 6,

    /// <summary>Return processing complete</summary>
    Processed = 7,

    /// <summary>Refund issued</summary>
    Refunded = 8,

    /// <summary>Exchange issued</summary>
    Exchanged = 9
}

/// <summary>
/// Reason category for order returns.
/// </summary>
public enum ReturnReasonCategory
{
    /// <summary>Other/unspecified</summary>
    Other = 0,

    /// <summary>Defective or damaged product</summary>
    Defective = 1,

    /// <summary>Wrong item received</summary>
    WrongItem = 2,

    /// <summary>Changed mind</summary>
    ChangedMind = 3,

    /// <summary>Better price found elsewhere</summary>
    PriceMatch = 4,

    /// <summary>Does not match description</summary>
    NotAsDescribed = 5,

    /// <summary>Late delivery</summary>
    LateDelivery = 6,

    /// <summary>Duplicate order</summary>
    DuplicateOrder = 7,

    /// <summary>Quality issue</summary>
    QualityIssue = 8
}

/// <summary>
/// FUNCTIONAL: Type of order transaction.
/// TECHNICAL: Determines processing rules and revenue recognition.
/// </summary>
public enum OrderType
{
    /// <summary>Standard new order</summary>
    Standard = 0,

    /// <summary>Renewal of existing subscription/contract</summary>
    Renewal = 1,

    /// <summary>Upgrade from existing product/service</summary>
    Upgrade = 2,

    /// <summary>Downgrade from existing product/service</summary>
    Downgrade = 3,

    /// <summary>Amendment to existing order</summary>
    Amendment = 4,

    /// <summary>Trial conversion to paid</summary>
    TrialConversion = 5,

    /// <summary>Free trial order</summary>
    Trial = 6,

    /// <summary>Partner/reseller order</summary>
    Partner = 7,

    /// <summary>Internal/employee order</summary>
    Internal = 8,

    /// <summary>Return/exchange order</summary>
    Return = 9,

    /// <summary>Credit order (negative)</summary>
    Credit = 10,

    /// <summary>Multi-year order</summary>
    MultiYear = 11
}

/// <summary>
/// FUNCTIONAL: Order fulfillment method.
/// TECHNICAL: Drives shipping/delivery workflow.
/// </summary>
public enum FulfillmentMethod
{
    /// <summary>Ship physical goods</summary>
    Ship = 0,

    /// <summary>Digital delivery (download/email)</summary>
    Digital = 1,

    /// <summary>Customer pickup</summary>
    Pickup = 2,

    /// <summary>Service provisioning</summary>
    Provision = 3,

    /// <summary>Subscription activation</summary>
    Activate = 4,

    /// <summary>Professional service delivery</summary>
    ServiceDelivery = 5,

    /// <summary>Third-party fulfillment</summary>
    ThirdParty = 6,

    /// <summary>No fulfillment required</summary>
    None = 7
}

/// <summary>
/// FUNCTIONAL: Priority level for order processing.
/// TECHNICAL: Affects queue position and SLA.
/// </summary>
public enum OrderPriority
{
    /// <summary>Normal processing priority</summary>
    Normal = 0,

    /// <summary>High priority - expedited processing</summary>
    High = 1,

    /// <summary>Urgent - immediate attention</summary>
    Urgent = 2,

    /// <summary>Low priority - process when convenient</summary>
    Low = 3,

    /// <summary>Critical - escalate immediately</summary>
    Critical = 4
}

#endregion

/// <summary>
/// Order entity representing a confirmed sales transaction.
/// Created from accepted Quote, drives Invoice and Fulfillment.
/// </summary>
public class Order : BaseEntity, IHasDomainEvents
{
    #region Identification

    /// <summary>System-generated unique order number</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>External reference (customer PO number, etc.)</summary>
    public string? ExternalOrderId { get; set; }

    /// <summary>Customer's purchase order number</summary>
    public string? CustomerPONumber { get; set; }

    /// <summary>Reference number for internal tracking</summary>
    public string? ReferenceNumber { get; set; }

    #endregion

    #region Order Details

    /// <summary>Order name/description</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Detailed order description</summary>
    public string? Description { get; set; }

    /// <summary>Current order status</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    /// <summary>Type of order transaction</summary>
    public OrderType OrderType { get; set; } = OrderType.Standard;

    /// <summary>How the order will be fulfilled</summary>
    public FulfillmentMethod FulfillmentMethod { get; set; } = FulfillmentMethod.Ship;

    /// <summary>Processing priority</summary>
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;

    #endregion

    #region Dates

    /// <summary>Date order was placed</summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date order was approved</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Requested delivery date</summary>
    public DateTime? RequestedDeliveryDate { get; set; }

    /// <summary>Promised delivery date</summary>
    public DateTime? PromisedDeliveryDate { get; set; }

    /// <summary>Actual shipment date</summary>
    public DateTime? ShippedDate { get; set; }

    /// <summary>Actual delivery date</summary>
    public DateTime? DeliveredDate { get; set; }

    /// <summary>Date order was completed</summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>Date order was cancelled</summary>
    public DateTime? CancelledDate { get; set; }

    /// <summary>Contract/service start date</summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>Contract/service end date</summary>
    public DateTime? ContractEndDate { get; set; }

    #endregion

    #region Pricing

    /// <summary>Sum of line items before discounts and tax</summary>
    public decimal Subtotal { get; set; } = 0;

    /// <summary>Total discount amount</summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>Discount percentage applied</summary>
    public decimal DiscountPercent { get; set; } = 0;

    /// <summary>Reason for discount</summary>
    public string? DiscountReason { get; set; }

    /// <summary>Tax amount</summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>Tax rate percentage</summary>
    public decimal TaxRate { get; set; } = 0;

    /// <summary>Shipping/freight cost</summary>
    public decimal ShippingAmount { get; set; } = 0;

    /// <summary>Handling fees</summary>
    public decimal HandlingAmount { get; set; } = 0;

    /// <summary>Total order amount (subtotal - discount + tax + shipping + handling)</summary>
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>Currency code (ISO 4217)</summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>Exchange rate if foreign currency</summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>Amount in base currency</summary>
    public decimal? BaseCurrencyAmount { get; set; }

    #endregion

    #region Revenue Recognition

    /// <summary>Monthly Recurring Revenue</summary>
    public decimal? MRR { get; set; }

    /// <summary>Annual Recurring Revenue</summary>
    public decimal? ARR { get; set; }

    /// <summary>Total Contract Value</summary>
    public decimal? TCV { get; set; }

    /// <summary>Annual Contract Value</summary>
    public decimal? ACV { get; set; }

    /// <summary>One-time revenue component</summary>
    public decimal? OneTimeRevenue { get; set; }

    /// <summary>Recurring revenue component</summary>
    public decimal? RecurringRevenue { get; set; }

    #endregion

    #region Billing Address

    public string? BillingName { get; set; }
    public string? BillingCompany { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPhone { get; set; }
    public string? BillingEmail { get; set; }

    #endregion

    #region Shipping Address

    public string? ShippingName { get; set; }
    public string? ShippingCompany { get; set; }
    public string? ShippingStreet { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingEmail { get; set; }
    public string? ShippingInstructions { get; set; }

    #endregion

    #region Shipping Details

    /// <summary>Shipping carrier/method</summary>
    public string? ShippingMethod { get; set; }

    /// <summary>Tracking number</summary>
    public string? TrackingNumber { get; set; }

    /// <summary>Tracking URL</summary>
    public string? TrackingUrl { get; set; }

    /// <summary>Weight for shipping</summary>
    public decimal? ShippingWeight { get; set; }

    /// <summary>Number of packages</summary>
    public int? PackageCount { get; set; }

    #endregion

    #region Payment Information

    /// <summary>Payment terms (Net 30, etc.)</summary>
    public string? PaymentTerms { get; set; }

    /// <summary>Payment method on file</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Amount invoiced so far</summary>
    public decimal AmountInvoiced { get; set; } = 0;

    /// <summary>Amount paid so far</summary>
    public decimal AmountPaid { get; set; } = 0;

    /// <summary>Outstanding balance</summary>
    public decimal BalanceDue => TotalAmount - AmountPaid;

    /// <summary>Whether fully paid</summary>
    public bool IsPaid => AmountPaid >= TotalAmount;

    #endregion

    #region Relationships

    /// <summary>Source quote ID</summary>
    public int? QuoteId { get; set; }

    /// <summary>Navigation to source quote</summary>
    public Quote? Quote { get; set; }

    /// <summary>Customer account ID</summary>
    public int AccountId { get; set; }

    /// <summary>Navigation to customer account</summary>
    public Account? Account { get; set; }

    /// <summary>Primary contact ID</summary>
    public int? ContactId { get; set; }

    /// <summary>Navigation to primary contact</summary>
    public Contact? Contact { get; set; }

    /// <summary>Related opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Navigation to related opportunity</summary>
    public Opportunity? Opportunity { get; set; }

    /// <summary>Sales rep who owns the order</summary>
    public int? OwnerId { get; set; }

    /// <summary>Alias for OwnerId - User ID who owns this order</summary>
    [NotMapped]
    public int? UserId
    {
        get => OwnerId;
        set => OwnerId = value;
    }

    /// <summary>Navigation to sales rep</summary>
    public User? Owner { get; set; }

    /// <summary>Alias for OwnerId for service compatibility</summary>
    public int? SalesRepId
    {
        get => OwnerId;
        set => OwnerId = value;
    }

    /// <summary>User who approved the order</summary>
    public int? ApprovedById { get; set; }

    /// <summary>Navigation to approver</summary>
    public User? ApprovedBy { get; set; }

    /// <summary>Parent order for amendments</summary>
    public int? ParentOrderId { get; set; }

    /// <summary>Navigation to parent order</summary>
    public Order? ParentOrder { get; set; }

    /// <summary>Child orders (amendments, renewals)</summary>
    public ICollection<Order> ChildOrders { get; set; } = new List<Order>();

    /// <summary>Order line items</summary>
    public ICollection<OrderLineItem> LineItems { get; set; } = new List<OrderLineItem>();

    /// <summary>Related invoices</summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    /// <summary>Related subscriptions</summary>
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    #endregion

    #region Notes & Attachments

    /// <summary>Internal notes</summary>
    public string? InternalNotes { get; set; }

    /// <summary>Special instructions</summary>
    public string? SpecialInstructions { get; set; }

    /// <summary>Cancellation reason</summary>
    public string? CancellationReason { get; set; }

    /// <summary>Terms and conditions</summary>
    public string? TermsAndConditions { get; set; }

    #endregion

    #region Workflow Dates

    /// <summary>Date order was submitted for approval</summary>
    public DateTime? SubmittedDate { get; set; }

    /// <summary>Date order was fulfilled</summary>
    public DateTime? FulfilledDate { get; set; }

    #endregion

    #region Hold Status

    /// <summary>Reason for hold</summary>
    public string? HoldReason { get; set; }

    /// <summary>Date order was put on hold</summary>
    public DateTime? HoldDate { get; set; }

    #endregion

    #region Rejection

    /// <summary>Reason for rejection</summary>
    public string? RejectionReason { get; set; }

    #endregion

    #region Return Information (TODO-GAP-SALES-001)

    /// <summary>Current return status</summary>
    public ReturnStatus ReturnStatus { get; set; } = ReturnStatus.None;

    /// <summary>Return reason category</summary>
    public ReturnReasonCategory? ReturnReasonCategory { get; set; }

    /// <summary>Detailed reason for return</summary>
    [MaxLength(2000)]
    public string? ReturnReason { get; set; }

    /// <summary>Date return was requested</summary>
    public DateTime? ReturnRequestedDate { get; set; }

    /// <summary>Date return was approved</summary>
    public DateTime? ReturnApprovedDate { get; set; }

    /// <summary>Date items were received back</summary>
    public DateTime? ReturnReceivedDate { get; set; }

    /// <summary>Date return was processed</summary>
    public DateTime? ReturnProcessedDate { get; set; }

    /// <summary>Return authorization number (RMA)</summary>
    [MaxLength(100)]
    public string? ReturnAuthorizationNumber { get; set; }

    /// <summary>Amount to be refunded</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RefundAmount { get; set; }

    /// <summary>Restocking fee applied</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RestockingFee { get; set; }

    /// <summary>Return shipping tracking number</summary>
    [MaxLength(255)]
    public string? ReturnTrackingNumber { get; set; }

    /// <summary>Quality inspection notes</summary>
    [MaxLength(2000)]
    public string? QualityInspectionNotes { get; set; }

    /// <summary>Whether return qualifies for exchange</summary>
    public bool IsEligibleForExchange { get; set; } = false;

    /// <summary>Exchange order ID if exchanged</summary>
    public int? ExchangeOrderId { get; set; }

    #endregion

    #region Discount Codes

    /// <summary>Applied discount code</summary>
    public string? DiscountCode { get; set; }

    /// <summary>Applied coupon code</summary>
    public string? CouponCode { get; set; }

    #endregion

    #region Audit Fields

    /// <summary>Source system (web, API, import)</summary>
    public string? Source { get; set; }

    /// <summary>IP address of order submission</summary>
    public string? SourceIpAddress { get; set; }

    #endregion

    #region Domain Events

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion

    #region Business Methods

    /// <summary>
    /// Confirms (approves) the order. Throws if already approved, cancelled, or completed.
    /// </summary>
    public void Confirm(int confirmedByUserId)
    {
        if (Status == OrderStatus.Approved)
            throw new BusinessRuleException("Order.Confirm", "Order is already confirmed.");
        if (Status == OrderStatus.Cancelled)
            throw new BusinessRuleException("Order.Confirm", "Cannot confirm a cancelled order.");
        if (Status == OrderStatus.Completed)
            throw new BusinessRuleException("Order.Confirm", "Cannot confirm a completed order.");

        Status = OrderStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        ApprovedById = confirmedByUserId;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderConfirmedEvent(Id, confirmedByUserId, DateTime.UtcNow));
    }

    /// <summary>
    /// Marks the order as shipped. Throws if not in a shippable state.
    /// </summary>
    public void Ship(string? trackingNumber = null)
    {
        if (Status != OrderStatus.Approved && Status != OrderStatus.Processing && Status != OrderStatus.PartiallyFulfilled)
            throw new BusinessRuleException("Order.Ship", "Order must be Approved, Processing, or PartiallyFulfilled to ship.");

        Status = OrderStatus.Fulfilled;
        ShippedDate = DateTime.UtcNow;
        if (trackingNumber != null)
            TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderShippedEvent(Id, trackingNumber, DateTime.UtcNow));
    }

    /// <summary>
    /// Cancels the order. Throws if already cancelled, completed, or refunded.
    /// </summary>
    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Order.Cancel", "Cancellation reason is required.");
        if (Status == OrderStatus.Cancelled)
            throw new BusinessRuleException("Order.Cancel", "Order is already cancelled.");
        if (Status == OrderStatus.Completed)
            throw new BusinessRuleException("Order.Cancel", "Cannot cancel a completed order.");
        if (Status == OrderStatus.Refunded)
            throw new BusinessRuleException("Order.Cancel", "Cannot cancel a refunded order.");

        Status = OrderStatus.Cancelled;
        CancelledDate = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderCancelledEvent(Id, reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Factory method for unit testing — creates an Order with specified status.
    /// </summary>
    internal static Order CreateForTesting(
        OrderStatus status = OrderStatus.Draft)
    {
        return new Order
        {
            Id = 1,
            Status = status,
            Name = "Test Order",
            OrderNumber = "ORD-TEST-001",
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

/// <summary>
/// Line item within an Order.
/// </summary>
public class OrderLineItem : BaseEntity
{
    #region Identification

    /// <summary>Line item number for display</summary>
    public int LineNumber { get; set; }

    /// <summary>External line reference</summary>
    public string? ExternalLineId { get; set; }

    #endregion

    #region Product Details

    /// <summary>Product/service name (copied at order time)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Product description (copied at order time)</summary>
    public string? Description { get; set; }

    /// <summary>Product SKU (copied at order time)</summary>
    public string? SKU { get; set; }

    /// <summary>Product code</summary>
    public string? ProductCode { get; set; }

    #endregion

    #region Quantity & Pricing

    /// <summary>Quantity ordered</summary>
    public decimal Quantity { get; set; } = 1;

    /// <summary>Unit of measure</summary>
    public string? UnitOfMeasure { get; set; }

    /// <summary>Unit list price</summary>
    public decimal UnitPrice { get; set; } = 0;

    /// <summary>Unit cost (for margin calculation)</summary>
    public decimal? UnitCost { get; set; }

    /// <summary>Discount amount per unit</summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>Discount percentage</summary>
    public decimal DiscountPercent { get; set; } = 0;

    /// <summary>Extended amount (quantity × unit price - discount)</summary>
    public decimal ExtendedAmount { get; set; } = 0;

    /// <summary>Tax amount for this line</summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>Total line amount including tax</summary>
    public decimal TotalAmount { get; set; } = 0;

    #endregion

    #region Fulfillment

    /// <summary>Quantity shipped</summary>
    public decimal QuantityShipped { get; set; } = 0;

    /// <summary>Quantity remaining to ship</summary>
    public decimal QuantityRemaining => Quantity - QuantityShipped;

    /// <summary>Quantity invoiced</summary>
    public decimal QuantityInvoiced { get; set; } = 0;

    /// <summary>Quantity returned</summary>
    public decimal QuantityReturned { get; set; } = 0;

    /// <summary>Quantity fulfilled</summary>
    public decimal FulfilledQuantity { get; set; } = 0;

    /// <summary>Date line item was fulfilled</summary>
    public DateTime? FulfilledDate { get; set; }

    /// <summary>Quantity returned (for returns)</summary>
    public decimal ReturnedQuantity { get; set; } = 0;

    /// <summary>Reason for return</summary>
    public string? ReturnReason { get; set; }

    /// <summary>Fulfillment status</summary>
    public OrderStatus FulfillmentStatus { get; set; } = OrderStatus.Draft;

    /// <summary>Estimated ship date</summary>
    public DateTime? EstimatedShipDate { get; set; }

    /// <summary>Actual ship date</summary>
    public DateTime? ShippedDate { get; set; }

    #endregion

    #region Subscription Details

    /// <summary>Billing frequency for subscription items</summary>
    public BillingFrequency? BillingFrequency { get; set; }

    /// <summary>Service start date</summary>
    public DateTime? ServiceStartDate { get; set; }

    /// <summary>Service end date</summary>
    public DateTime? ServiceEndDate { get; set; }

    /// <summary>Term length in months</summary>
    public int? TermLengthMonths { get; set; }

    /// <summary>Auto-renewal flag</summary>
    public bool AutoRenew { get; set; } = false;

    #endregion

    #region Relationships

    /// <summary>Parent order ID</summary>
    public int OrderId { get; set; }

    /// <summary>Navigation to parent order</summary>
    public Order? Order { get; set; }

    /// <summary>Product ID</summary>
    public int? ProductId { get; set; }

    /// <summary>Navigation to product</summary>
    public Product? Product { get; set; }

    /// <summary>Source quote line item ID</summary>
    public int? QuoteLineItemId { get; set; }

    /// <summary>Navigation to quote line item</summary>
    public QuoteLineItem? QuoteLineItem { get; set; }

    /// <summary>Parent bundle line item</summary>
    public int? ParentLineItemId { get; set; }

    /// <summary>Navigation to parent bundle line item</summary>
    public OrderLineItem? ParentLineItem { get; set; }

    /// <summary>Child bundle items</summary>
    public ICollection<OrderLineItem> BundleItems { get; set; } = new List<OrderLineItem>();

    #endregion

    #region Notes

    /// <summary>Line item notes</summary>
    public string? Notes { get; set; }

    /// <summary>Internal notes</summary>
    public string? InternalNotes { get; set; }

    #endregion
}
