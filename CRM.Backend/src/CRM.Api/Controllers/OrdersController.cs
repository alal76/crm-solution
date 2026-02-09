using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for order management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all orders with optional filters.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(
        [FromQuery] int? customerId = null,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetAllAsync(customerId, status, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets an order by ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id, cancellationToken);
            if (order == null) return NotFound($"Order {id} not found");
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets an order by order number.</summary>
    [HttpGet("by-number/{orderNumber}")]
    public async Task<ActionResult<Order>> GetByOrderNumber(string orderNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
            if (order == null) return NotFound($"Order '{orderNumber}' not found");
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Creates a new order.</summary>
    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _orderService.CreateAsync(order, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an existing order.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Order>> Update(int id, [FromBody] Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (id != order.Id) return BadRequest("ID mismatch");
            var updated = await _orderService.UpdateAsync(order, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Deletes an order.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.DeleteAsync(id, cancellationToken);
            if (!result) return NotFound($"Order {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Order Operations

    /// <summary>Creates an order from an existing quote.</summary>
    [HttpPost("from-quote/{quoteId}")]
    public async Task<ActionResult<Order>> CreateFromQuote(int quoteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.CreateFromQuoteAsync(quoteId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from quote {QuoteId}", quoteId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Creates an order from an opportunity.</summary>
    [HttpPost("from-opportunity/{opportunityId}")]
    public async Task<ActionResult<Order>> CreateFromOpportunity(int opportunityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.CreateFromOpportunityAsync(opportunityId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from opportunity {OpportunityId}", opportunityId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Generates a new order number.</summary>
    [HttpGet("generate-number")]
    public async Task<ActionResult<string>> GenerateOrderNumber(CancellationToken cancellationToken = default)
    {
        try
        {
            var number = await _orderService.GenerateOrderNumberAsync(cancellationToken);
            return Ok(number);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating order number");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Clones an existing order.</summary>
    [HttpPost("{id}/clone")]
    public async Task<ActionResult<Order>> CloneOrder(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.CloneOrderAsync(id, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Status Management

    /// <summary>Updates the status of an order.</summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<Order>> UpdateStatus(int id, [FromBody] OrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Submits an order for approval.</summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<Order>> SubmitForApproval(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.SubmitForApprovalAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting order {OrderId} for approval", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Approves an order.</summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<Order>> Approve(int id, [FromBody] OrderApproveRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.ApproveAsync(id, request.ApprovedById, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Rejects an order.</summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<Order>> Reject(int id, [FromBody] OrderRejectRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.RejectAsync(id, request.RejectedById, request.Reason, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Cancels an order.</summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<Order>> Cancel(int id, [FromBody] OrderCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.CancelAsync(id, request.Reason, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Puts an order on hold.</summary>
    [HttpPost("{id}/hold")]
    public async Task<ActionResult<Order>> PutOnHold(int id, [FromBody] OrderHoldRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.PutOnHoldAsync(id, request.Reason, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error putting order {OrderId} on hold", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Releases an order from hold.</summary>
    [HttpPost("{id}/release")]
    public async Task<ActionResult<Order>> ReleaseFromHold(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.ReleaseFromHoldAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing order {OrderId} from hold", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Fulfillment

    /// <summary>Marks an order as fully fulfilled.</summary>
    [HttpPost("{id}/fulfill")]
    public async Task<ActionResult<Order>> MarkAsFulfilled(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.MarkAsFulfilledAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as fulfilled", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Marks an order as partially fulfilled.</summary>
    [HttpPost("{id}/partial-fulfill")]
    public async Task<ActionResult<Order>> MarkAsPartiallyFulfilled(int id, [FromBody] PartialFulfillRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.MarkAsPartiallyFulfilledAsync(id, request.FulfilledLineItemIds, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as partially fulfilled", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Marks an order as delivered.</summary>
    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<Order>> MarkAsDelivered(int id, [FromQuery] DateTime? deliveryDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.MarkAsDeliveredAsync(id, deliveryDate, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as delivered", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Processes a return for an order.</summary>
    [HttpPost("{id}/return")]
    public async Task<ActionResult<Order>> ProcessReturn(int id, [FromBody] ProcessReturnRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.ProcessReturnAsync(id, request.ReturnItems, request.Reason, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing return for order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Line Items

    /// <summary>Adds a line item to an order.</summary>
    [HttpPost("{id}/line-items")]
    public async Task<ActionResult<OrderLineItem>> AddLineItem(int id, [FromBody] OrderLineItem lineItem, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var item = await _orderService.AddLineItemAsync(id, lineItem, cancellationToken);
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding line item to order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an order line item.</summary>
    [HttpPut("line-items/{lineItemId}")]
    public async Task<ActionResult<OrderLineItem>> UpdateLineItem(int lineItemId, [FromBody] OrderLineItem lineItem, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            lineItem.Id = lineItemId;
            var item = await _orderService.UpdateLineItemAsync(lineItem, cancellationToken);
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating line item {LineItemId}", lineItemId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Removes a line item from an order.</summary>
    [HttpDelete("line-items/{lineItemId}")]
    public async Task<IActionResult> RemoveLineItem(int lineItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.RemoveLineItemAsync(lineItemId, cancellationToken);
            if (!result) return NotFound($"Line item {lineItemId} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing line item {LineItemId}", lineItemId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets all line items for an order.</summary>
    [HttpGet("{id}/line-items")]
    public async Task<ActionResult<IEnumerable<OrderLineItem>>> GetLineItems(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _orderService.GetLineItemsAsync(id, cancellationToken);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving line items for order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Queries

    /// <summary>Gets orders by status.</summary>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetByStatus(OrderStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetByStatusAsync(status, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by status {Status}", status);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets orders within a date range.</summary>
    [HttpGet("by-date-range")]
    public async Task<ActionResult<IEnumerable<Order>>> GetByDateRange(
        [FromQuery][Required] DateTime fromDate,
        [FromQuery][Required] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by date range");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets orders requiring action.</summary>
    [HttpGet("requiring-action")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersRequiringAction(CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetOrdersRequiringActionAsync(cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders requiring action");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets order statistics.</summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<OrderStatistics>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _orderService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order statistics");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Searches orders.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Order>>> Search([FromQuery][Required] string query, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.SearchAsync(query, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orders");
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Calculations

    /// <summary>Recalculates order totals.</summary>
    [HttpPost("{id}/recalculate")]
    public async Task<ActionResult<Order>> RecalculateTotals(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.RecalculateTotalsAsync(id, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating totals for order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Applies a discount to an order.</summary>
    [HttpPost("{id}/discount")]
    public async Task<ActionResult<Order>> ApplyDiscount(int id, [FromBody] OrderDiscountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.ApplyDiscountAsync(id, request.DiscountAmount, request.DiscountReason, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount to order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Applies a coupon to an order.</summary>
    [HttpPost("{id}/coupon")]
    public async Task<ActionResult<Order>> ApplyCoupon(int id, [FromBody] OrderCouponRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.ApplyCouponAsync(id, request.CouponCode, cancellationToken);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying coupon to order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Invoicing

    /// <summary>Creates an invoice from an order.</summary>
    [HttpPost("{id}/invoice")]
    public async Task<ActionResult<Invoice>> CreateInvoice(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _orderService.CreateInvoiceAsync(id, cancellationToken);
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets invoices for an order.</summary>
    [HttpGet("{id}/invoices")]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _orderService.GetInvoicesAsync(id, cancellationToken);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Helpers

    private ActionResult HandleServiceException(Exception ex)
    {
        if (ex is InvalidOperationException ioe && ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ioe.Message);
        }

        return BadRequest(ex.Message);
    }

    #endregion

    #region Request DTOs

    public class OrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }
    }

    public class OrderApproveRequest
    {
        [Required]
        public int ApprovedById { get; set; }
    }

    public class OrderRejectRequest
    {
        [Required]
        public int RejectedById { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class OrderCancelRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class OrderHoldRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class PartialFulfillRequest
    {
        [Required]
        public IEnumerable<int> FulfilledLineItemIds { get; set; } = new List<int>();
    }

    public class ProcessReturnRequest
    {
        [Required]
        public IEnumerable<OrderReturnItem> ReturnItems { get; set; } = new List<OrderReturnItem>();

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class OrderDiscountRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        public string? DiscountReason { get; set; }
    }

    public class OrderCouponRequest
    {
        [Required]
        public string CouponCode { get; set; } = string.Empty;
    }

    #endregion
}
