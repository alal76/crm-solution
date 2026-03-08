// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for order management operations.
/// Handles order lifecycle from creation to fulfillment.
/// </summary>
public interface IOrderService
{
    #region CRUD Operations

    /// <summary>Gets all orders with optional filtering.</summary>
    Task<IEnumerable<OrderDto>> GetAllAsync(
        int? accountId = null,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an order by ID.</summary>
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets an order by order number.</summary>
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>Creates a new order.</summary>
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing order.</summary>
    Task<OrderDto> UpdateAsync(UpdateOrderDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes an order (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Order Operations

    /// <summary>Creates an order from a quote.</summary>
    Task<OrderDto> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default);

    /// <summary>Creates an order from an opportunity.</summary>
    Task<OrderDto> CreateFromOpportunityAsync(int opportunityId, CancellationToken cancellationToken = default);

    /// <summary>Generates the next order number.</summary>
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>Clones an existing order.</summary>
    Task<Order> CloneOrderAsync(int orderId, CancellationToken cancellationToken = default);

    #endregion

    #region Status Management

    /// <summary>Updates order status.</summary>
    Task<Order> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>Submits a draft order for approval.</summary>
    Task<Order> SubmitForApprovalAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Approves an order.</summary>
    Task<Order> ApproveAsync(int orderId, int approvedById, CancellationToken cancellationToken = default);

    /// <summary>Rejects an order.</summary>
    Task<Order> RejectAsync(int orderId, int rejectedById, string reason, CancellationToken cancellationToken = default);

    /// <summary>Cancels an order.</summary>
    Task<Order> CancelAsync(int orderId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Puts an order on hold.</summary>
    Task<Order> PutOnHoldAsync(int orderId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Releases an order from hold.</summary>
    Task<Order> ReleaseFromHoldAsync(int orderId, CancellationToken cancellationToken = default);

    #endregion

    #region Fulfillment

    /// <summary>Marks an order as fulfilled.</summary>
    Task<Order> MarkAsFulfilledAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Marks an order as partially fulfilled.</summary>
    Task<Order> MarkAsPartiallyFulfilledAsync(int orderId, IEnumerable<int> fulfilledLineItemIds, CancellationToken cancellationToken = default);

    /// <summary>Marks an order as delivered.</summary>
    Task<Order> MarkAsDeliveredAsync(int orderId, DateTime? deliveryDate = null, CancellationToken cancellationToken = default);

    /// <summary>Processes a return for an order.</summary>
    Task<Order> ProcessReturnAsync(int orderId, IEnumerable<OrderReturnItem> returnItems, string reason, CancellationToken cancellationToken = default);

    #endregion

    #region Line Items

    /// <summary>Adds a line item to an order.</summary>
    Task<OrderLineItem> AddLineItemAsync(int orderId, OrderLineItem lineItem, CancellationToken cancellationToken = default);

    /// <summary>Updates a line item.</summary>
    Task<OrderLineItem> UpdateLineItemAsync(OrderLineItem lineItem, CancellationToken cancellationToken = default);

    /// <summary>Removes a line item.</summary>
    Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default);

    /// <summary>Gets all line items for an order.</summary>
    Task<IEnumerable<OrderLineItem>> GetLineItemsAsync(int orderId, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets orders by status.</summary>
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>Gets orders within a date range.</summary>
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets orders requiring action (on hold, backordered).</summary>
    Task<IEnumerable<Order>> GetOrdersRequiringActionAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets order statistics.</summary>
    Task<OrderStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Searches orders by criteria.</summary>
    Task<IEnumerable<Order>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    #endregion

    #region Calculations

    /// <summary>Recalculates order totals.</summary>
    Task<Order> RecalculateTotalsAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Applies a discount to an order.</summary>
    Task<Order> ApplyDiscountAsync(int orderId, decimal discountAmount, string? discountCode = null, CancellationToken cancellationToken = default);

    /// <summary>Applies a coupon code to an order.</summary>
    Task<Order> ApplyCouponAsync(int orderId, string couponCode, CancellationToken cancellationToken = default);

    #endregion

    #region Invoicing

    /// <summary>Creates an invoice for an order.</summary>
    Task<Invoice> CreateInvoiceAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Gets invoices for an order.</summary>
    Task<IEnumerable<Invoice>> GetInvoicesAsync(int orderId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Order return item details.
/// </summary>
public class OrderReturnItem
{
    public int LineItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Condition { get; set; }
}

/// <summary>
/// Order statistics for reporting.
/// </summary>
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
