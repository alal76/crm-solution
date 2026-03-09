// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for order management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : CrmControllerBase
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
    /// <param name="accountId">Filter by account ID</param>
    /// <param name="status">Filter by order status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders</returns>
    /// <response code="200">Returns the list of orders</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetAllAsync(accountId, status, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets an order by ID.</summary>
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The requested order</returns>
    /// <response code="200">Returns the order</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id, cancellationToken);
            if (order == null)
            {
                return NotFound($"Order {id} not found");
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets an order by order number.</summary>
    /// <param name="orderNumber">The order number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The requested order</returns>
    /// <response code="200">Returns the order</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetByOrderNumber(string orderNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
            if (order == null)
            {
                return NotFound($"Order '{orderNumber}' not found");
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Creates a new order.</summary>
    /// <param name="dto">The order to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created order</returns>
    /// <response code="201">Returns the newly created order</response>
    /// <response code="400">If the order data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            var created = await _orderService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an existing order.</summary>
    /// <param name="id">The order ID</param>
    /// <param name="dto">The updated order data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order</returns>
    /// <response code="200">Returns the updated order</response>
    /// <response code="400">If the order data is invalid or ID mismatch</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] UpdateOrderDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }
            var updated = await _orderService.UpdateAsync(dto, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Deletes an order.</summary>
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Order successfully deleted</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.DeleteAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound($"Order {id} not found");
            }
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
    /// <param name="quoteId">The quote ID to create order from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created order</returns>
    /// <response code="201">Returns the newly created order</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("from-quote/{quoteId}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> CreateFromQuote(int quoteId, CancellationToken cancellationToken = default)
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
    /// <param name="opportunityId">The opportunity ID to create order from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created order</returns>
    /// <response code="201">Returns the newly created order</response>
    /// <response code="404">If the opportunity is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("from-opportunity/{opportunityId}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> CreateFromOpportunity(int opportunityId, CancellationToken cancellationToken = default)
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new unique order number</returns>
    /// <response code="200">Returns the generated order number</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("generate-number")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID to clone</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cloned order</returns>
    /// <response code="201">Returns the cloned order</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/clone")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order</returns>
    /// <response code="200">Returns the updated order</response>
    /// <response code="400">If the status transition is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order</returns>
    /// <response code="200">Returns the order submitted for approval</response>
    /// <response code="400">If the order cannot be submitted</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The approval request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The approved order</returns>
    /// <response code="200">Returns the approved order</response>
    /// <response code="400">If the order cannot be approved</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The rejection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rejected order</returns>
    /// <response code="200">Returns the rejected order</response>
    /// <response code="400">If the order cannot be rejected</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The cancellation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cancelled order</returns>
    /// <response code="200">Returns the cancelled order</response>
    /// <response code="400">If the order cannot be cancelled</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The hold request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order on hold</returns>
    /// <response code="200">Returns the order put on hold</response>
    /// <response code="400">If the order cannot be put on hold</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/hold")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The released order</returns>
    /// <response code="200">Returns the released order</response>
    /// <response code="400">If the order cannot be released</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/release")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The fulfilled order</returns>
    /// <response code="200">Returns the fulfilled order</response>
    /// <response code="400">If the order cannot be fulfilled</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The partial fulfillment request with line item IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The partially fulfilled order</returns>
    /// <response code="200">Returns the partially fulfilled order</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/partial-fulfill")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="deliveryDate">Optional delivery date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The delivered order</returns>
    /// <response code="200">Returns the delivered order</response>
    /// <response code="400">If the order cannot be marked as delivered</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/deliver")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The return request with items and reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order with return processed</returns>
    /// <response code="200">Returns the order with return processed</response>
    /// <response code="400">If the return request is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="lineItem">The line item to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created line item</returns>
    /// <response code="200">Returns the created line item</response>
    /// <response code="400">If the line item data is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/line-items")]
    [ProducesResponseType(typeof(OrderLineItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderLineItem>> AddLineItem(int id, [FromBody] OrderLineItem lineItem, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
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
    /// <param name="lineItemId">The line item ID</param>
    /// <param name="lineItem">The updated line item data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated line item</returns>
    /// <response code="200">Returns the updated line item</response>
    /// <response code="400">If the line item data is invalid</response>
    /// <response code="404">If the line item is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("line-items/{lineItemId}")]
    [ProducesResponseType(typeof(OrderLineItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderLineItem>> UpdateLineItem(int lineItemId, [FromBody] OrderLineItem lineItem, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
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
    /// <param name="lineItemId">The line item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Line item successfully removed</response>
    /// <response code="404">If the line item is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("line-items/{lineItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveLineItem(int lineItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.RemoveLineItemAsync(lineItemId, cancellationToken);
            if (!result)
            {
                return NotFound($"Line item {lineItemId} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing line item {LineItemId}", lineItemId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets all line items for an order.</summary>
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of line items</returns>
    /// <response code="200">Returns the list of line items</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}/line-items")]
    [ProducesResponseType(typeof(IEnumerable<OrderLineItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="status">The order status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders with the specified status</returns>
    /// <response code="200">Returns the list of orders</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="fromDate">Start date of the range</param>
    /// <param name="toDate">End date of the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders in the date range</returns>
    /// <response code="200">Returns the list of orders</response>
    /// <response code="400">If the date range is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("by-date-range")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders requiring action</returns>
    /// <response code="200">Returns the list of orders</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("requiring-action")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="fromDate">Optional start date for statistics</param>
    /// <param name="toDate">Optional end date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order statistics</returns>
    /// <response code="200">Returns the order statistics</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(OrderStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="query">The search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching orders</returns>
    /// <response code="200">Returns the search results</response>
    /// <response code="400">If the query is empty</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order with recalculated totals</returns>
    /// <response code="200">Returns the updated order</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/recalculate")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The discount request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order with discount applied</returns>
    /// <response code="200">Returns the updated order</response>
    /// <response code="400">If the discount amount is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/discount")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="request">The coupon request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated order with coupon applied</returns>
    /// <response code="200">Returns the updated order</response>
    /// <response code="400">If the coupon code is invalid</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/coupon")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created invoice</returns>
    /// <response code="200">Returns the created invoice</response>
    /// <response code="400">If the order cannot be invoiced</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/invoice")]
    [ProducesResponseType(typeof(Invoice), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of invoices for the order</returns>
    /// <response code="200">Returns the list of invoices</response>
    /// <response code="404">If the order is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}/invoices")]
    [ProducesResponseType(typeof(IEnumerable<Invoice>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
