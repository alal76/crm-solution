// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

#region Order Return Enumerations

/// <summary>
/// Status of an order return request.
/// </summary>
public enum OrderReturnStatus
{
    /// <summary>Return request submitted, pending review</summary>
    Pending = 0,

    /// <summary>Return request approved</summary>
    Approved = 1,

    /// <summary>Return request rejected</summary>
    Rejected = 2,

    /// <summary>Items received from customer</summary>
    Received = 3,

    /// <summary>Return is being processed</summary>
    Processing = 4,

    /// <summary>Refund issued</summary>
    Refunded = 5,

    /// <summary>Return completed</summary>
    Completed = 6,

    /// <summary>Return cancelled</summary>
    Cancelled = 7
}

/// <summary>
/// Reason for order return.
/// </summary>
public enum OrderReturnReason
{
    /// <summary>Product defective or damaged</summary>
    Defective = 0,

    /// <summary>Wrong item received</summary>
    WrongItem = 1,

    /// <summary>Item not as described</summary>
    NotAsDescribed = 2,

    /// <summary>Changed mind / no longer needed</summary>
    ChangedMind = 3,

    /// <summary>Found a better price elsewhere</summary>
    BetterPrice = 4,

    /// <summary>Arrived too late</summary>
    ArrivedLate = 5,

    /// <summary>Duplicate order</summary>
    DuplicateOrder = 6,

    /// <summary>Other reason (see notes)</summary>
    Other = 7
}

#endregion

/// <summary>
/// Order return entity for managing product returns and refunds.
/// </summary>
public class OrderReturn : BaseEntity
{
    #region Identification

    /// <summary>Auto-generated return number</summary>
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>Customer's RMA number (if provided)</summary>
    public string? RmaNumber { get; set; }

    #endregion

    #region Relationships

    /// <summary>Associated order ID</summary>
    public int OrderId { get; set; }

    /// <summary>Navigation to order</summary>
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    /// <summary>Associated account ID</summary>
    public int? AccountId { get; set; }

    /// <summary>Navigation to account</summary>
    [ForeignKey("AccountId")]
    public Account? Account { get; set; }

    /// <summary>User who initiated the return</summary>
    public int? InitiatedById { get; set; }

    /// <summary>Navigation to initiating user</summary>
    [ForeignKey("InitiatedById")]
    public User? InitiatedBy { get; set; }

    /// <summary>User who processed/approved the return</summary>
    public int? ProcessedById { get; set; }

    /// <summary>Navigation to processing user</summary>
    [ForeignKey("ProcessedById")]
    public User? ProcessedBy { get; set; }

    #endregion

    #region Return Details

    /// <summary>Current return status</summary>
    public OrderReturnStatus Status { get; set; } = OrderReturnStatus.Pending;

    /// <summary>Reason for return</summary>
    public OrderReturnReason Reason { get; set; }

    /// <summary>Detailed description of return reason</summary>
    public string? ReasonDescription { get; set; }

    /// <summary>Additional notes or comments</summary>
    public string? Notes { get; set; }

    #endregion

    #region Financial

    /// <summary>Original order amount</summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>Amount to be refunded</summary>
    public decimal RefundAmount { get; set; }

    /// <summary>Restocking fee (if applicable)</summary>
    public decimal RestockingFee { get; set; }

    /// <summary>Shipping refund amount</summary>
    public decimal ShippingRefund { get; set; }

    /// <summary>Net refund amount (RefundAmount - RestockingFee + ShippingRefund)</summary>
    public decimal NetRefundAmount => RefundAmount - RestockingFee + ShippingRefund;

    /// <summary>Currency code</summary>
    public string Currency { get; set; } = "USD";

    #endregion

    #region Processing

    /// <summary>Date return was requested</summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Date return was approved</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Date items were received</summary>
    public DateTime? ReceivedAt { get; set; }

    /// <summary>Date refund was processed</summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>Date return was completed</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Tracking number for return shipment</summary>
    public string? ReturnTrackingNumber { get; set; }

    /// <summary>Return shipping carrier</summary>
    public string? ReturnCarrier { get; set; }

    /// <summary>Refund transaction reference</summary>
    public string? RefundTransactionId { get; set; }

    #endregion

    #region Line Items (JSON serialized for simplicity)

    /// <summary>JSON array of return line items</summary>
    public string? LineItemsJson { get; set; }

    #endregion
}

/// <summary>
/// DTO for order return line items.
/// </summary>
public class OrderReturnLineItem
{
    /// <summary>Order line item ID</summary>
    public int OrderLineItemId { get; set; }

    /// <summary>Product ID</summary>
    public int ProductId { get; set; }

    /// <summary>Product name</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantity being returned</summary>
    public int Quantity { get; set; }

    /// <summary>Unit price</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Total refund for this line</summary>
    public decimal RefundAmount { get; set; }

    /// <summary>Reason for returning this item</summary>
    public string? Reason { get; set; }

    /// <summary>Condition of returned item</summary>
    public string? ItemCondition { get; set; }
}
