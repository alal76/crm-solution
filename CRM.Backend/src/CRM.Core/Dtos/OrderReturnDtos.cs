// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for order return data.
/// </summary>
public class OrderReturnDto
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string? RmaNumber { get; set; }
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Reason { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string? ReasonDescription { get; set; }
    public string? Notes { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RestockingFee { get; set; }
    public decimal ShippingRefund { get; set; }
    public decimal NetRefundAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ReturnTrackingNumber { get; set; }
    public string? ReturnCarrier { get; set; }
    public string? RefundTransactionId { get; set; }
    public int? InitiatedById { get; set; }
    public string? InitiatedByName { get; set; }
    public int? ProcessedById { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderReturnLineItemDto>? LineItems { get; set; }
}

/// <summary>
/// DTO for order return line item.
/// </summary>
public class OrderReturnLineItemDto
{
    public int OrderLineItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
    public string? ItemCondition { get; set; }
}

/// <summary>
/// DTO for creating an order return.
/// </summary>
public class CreateOrderReturnDto
{
    public int OrderId { get; set; }
    public int Reason { get; set; }
    public string? ReasonDescription { get; set; }
    public string? Notes { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RestockingFee { get; set; }
    public decimal ShippingRefund { get; set; }
    public List<CreateOrderReturnLineItemDto>? LineItems { get; set; }
}

/// <summary>
/// DTO for creating an order return line item.
/// </summary>
public class CreateOrderReturnLineItemDto
{
    public int OrderLineItemId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for updating an order return.
/// </summary>
public class UpdateOrderReturnDto
{
    public int Status { get; set; }
    public string? Notes { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? RestockingFee { get; set; }
    public decimal? ShippingRefund { get; set; }
    public string? ReturnTrackingNumber { get; set; }
    public string? ReturnCarrier { get; set; }
    public string? RefundTransactionId { get; set; }
}

/// <summary>
/// DTO for order return statistics.
/// </summary>
public class OrderReturnStatisticsDto
{
    public int TotalReturns { get; set; }
    public int PendingReturns { get; set; }
    public int ApprovedReturns { get; set; }
    public int CompletedReturns { get; set; }
    public int RejectedReturns { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal AverageRefundAmount { get; set; }
    public double ReturnRate { get; set; }
    public Dictionary<int, int> ReturnsByReason { get; set; } = new();
}
