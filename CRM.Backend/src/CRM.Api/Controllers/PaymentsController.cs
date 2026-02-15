// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for payment management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all payments with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] int? invoiceId = null,
        [FromQuery] PaymentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetAllAsync(accountId, invoiceId, status, cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets a payment by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
            if (payment == null) return NotFound($"Payment {id} not found");
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets a payment by transaction ID.</summary>
    [HttpGet("by-transaction/{transactionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> GetByTransactionId(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.GetByTransactionIdAsync(transactionId, cancellationToken);
            if (payment == null) return NotFound($"Payment with transaction '{transactionId}' not found");
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment by transaction {TransactionId}", transactionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Creates a new payment record.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> Create([FromBody] Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _paymentService.CreateAsync(payment, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an existing payment.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Payment>> Update(int id, [FromBody] Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (id != payment.Id) return BadRequest("ID mismatch");
            var updated = await _paymentService.UpdateAsync(payment, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Deletes a payment.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paymentService.DeleteAsync(id, cancellationToken);
            if (!result) return NotFound($"Payment {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Payment Processing

    /// <summary>Processes a payment for an invoice.</summary>
    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResult>> ProcessPayment([FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var result = await _paymentService.ProcessPaymentAsync(
                request.InvoiceId, request.Amount, request.Method, request.Details ?? new PaymentDetails(), cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", request.InvoiceId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Processes a refund for a payment.</summary>
    [HttpPost("{id}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResult>> ProcessRefund(int id, [FromBody] RefundRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var result = await _paymentService.ProcessRefundAsync(id, request.Amount, request.Reason, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Voids a payment.</summary>
    [HttpPost("{id}/void")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> VoidPayment(int id, [FromBody] VoidPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.VoidPaymentAsync(id, request.Reason, cancellationToken);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Captures a previously authorized payment.</summary>
    [HttpPost("{id}/capture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResult>> CapturePayment(int id, [FromQuery] decimal? amount = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paymentService.CapturePaymentAsync(id, amount, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Status Management

    /// <summary>Updates the status of a payment.</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> UpdateStatus(int id, [FromBody] PaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Marks a payment as completed.</summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> MarkAsCompleted(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.MarkAsCompletedAsync(id, cancellationToken);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as completed", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Marks a payment as failed.</summary>
    [HttpPost("{id}/fail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> MarkAsFailed(int id, [FromBody] FailPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.MarkAsFailedAsync(id, request.FailureReason, cancellationToken);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as failed", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Queries

    /// <summary>Gets payments within a date range.</summary>
    [HttpGet("by-date-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetByDateRange(
        [FromQuery][Required] DateTime fromDate,
        [FromQuery][Required] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByDateRangeAsync(fromDate, toDate, cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments by date range");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets pending payments.</summary>
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPendingPayments(CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetPendingPaymentsAsync(cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending payments");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets failed payments.</summary>
    [HttpGet("failed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetFailedPayments([FromQuery] int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetFailedPaymentsAsync(maxRetries, cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving failed payments");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets payment statistics.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentStatistics>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _paymentService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment statistics");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets payment history for a customer.</summary>
    [HttpGet("account/{accountId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAccountPaymentHistory(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetAccountPaymentHistoryAsync(accountId, cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history for account {AccountId}", accountId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Reconciliation

    /// <summary>Reconciles a payment with an external reference.</summary>
    [HttpPost("{id}/reconcile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> ReconcilePayment(int id, [FromBody] ReconcileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.ReconcilePaymentAsync(id, request.ExternalReference, cancellationToken);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets unreconciled payments.</summary>
    [HttpGet("unreconciled")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetUnreconciledPayments(CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetUnreconciledPaymentsAsync(cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unreconciled payments");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Applies a payment to multiple invoices.</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{id}/apply")]
    public async Task<ActionResult<IEnumerable<PaymentAllocation>>> ApplyPaymentToInvoices(
        int id,
        [FromBody] IEnumerable<PaymentAllocation> allocations,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paymentService.ApplyPaymentToInvoicesAsync(id, allocations, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying payment {PaymentId} to invoices", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Retry & Recovery

    /// <summary>Retries a failed payment.</summary>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResult>> RetryPayment(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paymentService.RetryPaymentAsync(id, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment {PaymentId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Schedules a future payment.</summary>
    [HttpPost("schedule")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> SchedulePayment([FromBody] SchedulePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var paymentEntity = new Payment
            {
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                PaymentMethod = request.Method,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var payment = await _paymentService.SchedulePaymentAsync(paymentEntity, request.ScheduledDate, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling payment for invoice {InvoiceId}", request.InvoiceId);
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

    public class ProcessPaymentRequest
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public PaymentDetails? Details { get; set; }
    }

    public class RefundRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class VoidPaymentRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class PaymentStatusRequest
    {
        [Required]
        public PaymentStatus Status { get; set; }
    }

    public class FailPaymentRequest
    {
        [Required]
        public string FailureReason { get; set; } = string.Empty;
    }

    public class ReconcileRequest
    {
        [Required]
        public string ExternalReference { get; set; } = string.Empty;
    }

    public class SchedulePaymentRequest
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }
    }

    #endregion
}
