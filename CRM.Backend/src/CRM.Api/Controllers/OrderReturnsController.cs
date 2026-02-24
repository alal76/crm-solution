// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for order return management.
/// Handles product returns, refunds, and RMA processing.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrderReturnsController : ControllerBase
{
    private readonly IOrderReturnService _returnService;
    private readonly ILogger<OrderReturnsController> _logger;

    public OrderReturnsController(
        IOrderReturnService returnService,
        ILogger<OrderReturnsController> logger)
    {
        _returnService = returnService ?? throw new ArgumentNullException(nameof(returnService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all order returns with optional filtering.
    /// </summary>
    /// <param name="orderId">Filter by order ID</param>
    /// <param name="accountId">Filter by account ID</param>
    /// <param name="status">Filter by status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of order returns</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderReturnDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? orderId = null,
        [FromQuery] int? accountId = null,
        [FromQuery] OrderReturnStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var returns = await _returnService.GetAllAsync(orderId, accountId, status, cancellationToken);
            var dtos = returns.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order returns");
            return StatusCode(500, new { error = "An error occurred while retrieving order returns" });
        }
    }

    /// <summary>
    /// Gets an order return by ID.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order return details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var orderReturn = await _returnService.GetByIdAsync(id, cancellationToken);
            if (orderReturn == null)
            {
                return NotFound(new { error = $"Order return {id} not found" });
            }
            return Ok(MapToDto(orderReturn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the order return" });
        }
    }

    /// <summary>
    /// Creates a new order return request.
    /// </summary>
    /// <param name="dto">Order return details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order return</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderReturnDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderReturnDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var orderReturn = await _returnService.CreateAsync(dto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = orderReturn.Id }, MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid order return request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order return");
            return StatusCode(500, new { error = "An error occurred while creating the order return" });
        }
    }

    /// <summary>
    /// Updates an order return.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="dto">Update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOrderReturnDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orderReturn = await _returnService.UpdateAsync(id, dto, cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the order return" });
        }
    }

    /// <summary>
    /// Deletes an order return (soft delete).
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _returnService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { error = $"Order return {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the order return" });
        }
    }

    /// <summary>
    /// Approves an order return request.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="notes">Optional approval notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] ApproveRejectRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var orderReturn = await _returnService.ApproveAsync(id, userId, request?.Notes, cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred while approving the order return" });
        }
    }

    /// <summary>
    /// Rejects an order return request.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="request">Rejection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody] ApproveRejectRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.Reason))
            {
                return BadRequest(new { error = "Rejection reason is required" });
            }

            var userId = GetCurrentUserId();
            var orderReturn = await _returnService.RejectAsync(id, userId, request.Reason, cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred while rejecting the order return" });
        }
    }

    /// <summary>
    /// Marks return items as received.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="request">Receive details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPost("{id:int}/receive")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    public async Task<IActionResult> MarkReceived(
        int id,
        [FromBody] ReceiveRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orderReturn = await _returnService.MarkReceivedAsync(id, request?.TrackingNumber, cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order return {Id} as received", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Processes refund for an order return.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="request">Refund details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPost("{id:int}/refund")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ProcessRefund(
        int id,
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.TransactionId))
            {
                return BadRequest(new { error = "Transaction ID is required" });
            }

            var orderReturn = await _returnService.ProcessRefundAsync(id, request.TransactionId, cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Completes an order return.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var orderReturn = await _returnService.CompleteAsync(id, cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Cancels an order return.
    /// </summary>
    /// <param name="id">Order return ID</param>
    /// <param name="request">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order return</returns>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(OrderReturnDto), 200)]
    public async Task<IActionResult> Cancel(
        int id,
        [FromBody] CancelRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orderReturn = await _returnService.CancelAsync(id, request?.Reason ?? "Cancelled by user", cancellationToken);
            return Ok(MapToDto(orderReturn));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order return {Id}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Gets pending order returns.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending returns</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<OrderReturnDto>), 200)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken = default)
    {
        try
        {
            var returns = await _returnService.GetPendingReturnsAsync(cancellationToken);
            var dtos = returns.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending order returns");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Gets order return statistics.
    /// </summary>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(OrderReturnStatisticsDto), 200)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _returnService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order return statistics");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    #region Private Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 1;
    }

    private static OrderReturnDto MapToDto(OrderReturn r)
    {
        return new OrderReturnDto
        {
            Id = r.Id,
            ReturnNumber = r.ReturnNumber,
            RmaNumber = r.RmaNumber,
            OrderId = r.OrderId,
            OrderNumber = r.Order?.OrderNumber,
            AccountId = r.AccountId,
            AccountName = r.Account?.Company,
            Status = (int)r.Status,
            StatusName = r.Status.ToString(),
            Reason = (int)r.Reason,
            ReasonName = r.Reason.ToString(),
            ReasonDescription = r.ReasonDescription,
            Notes = r.Notes,
            OriginalAmount = r.OriginalAmount,
            RefundAmount = r.RefundAmount,
            RestockingFee = r.RestockingFee,
            ShippingRefund = r.ShippingRefund,
            NetRefundAmount = r.NetRefundAmount,
            Currency = r.Currency,
            RequestedAt = r.RequestedAt,
            ApprovedAt = r.ApprovedAt,
            ReceivedAt = r.ReceivedAt,
            RefundedAt = r.RefundedAt,
            CompletedAt = r.CompletedAt,
            ReturnTrackingNumber = r.ReturnTrackingNumber,
            ReturnCarrier = r.ReturnCarrier,
            RefundTransactionId = r.RefundTransactionId,
            InitiatedById = r.InitiatedById,
            InitiatedByName = r.InitiatedBy != null ? $"{r.InitiatedBy.FirstName} {r.InitiatedBy.LastName}" : null,
            ProcessedById = r.ProcessedById,
            ProcessedByName = r.ProcessedBy != null ? $"{r.ProcessedBy.FirstName} {r.ProcessedBy.LastName}" : null,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt ?? r.CreatedAt
        };
    }

    #endregion

    #region Request DTOs

    public class ApproveRejectRequest
    {
        public string? Notes { get; set; }
        public string? Reason { get; set; }
    }

    public class ReceiveRequest
    {
        public string? TrackingNumber { get; set; }
    }

    public class RefundRequest
    {
        public string TransactionId { get; set; } = string.Empty;
    }

    public class CancelRequest
    {
        public string? Reason { get; set; }
    }

    #endregion
}
