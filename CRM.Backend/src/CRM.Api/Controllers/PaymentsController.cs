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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] int? invoiceId = null,
        [FromQuery] PaymentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetAllAsync(accountId, invoiceId, status, cancellationToken);
            return Ok(payments.Select(MapToDto));
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
    public async Task<ActionResult<PaymentDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
            if (payment == null)
                return NotFound($"Payment {id} not found");
            return Ok(MapToDto(payment));
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
    public async Task<ActionResult<PaymentDto>> GetByTransactionId(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.GetByTransactionIdAsync(transactionId, cancellationToken);
            if (payment == null)
                return NotFound($"Payment with transaction '{transactionId}' not found");
            return Ok(MapToDto(payment));
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
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                PaymentType = dto.PaymentType,
                Status = dto.Status,
                ScheduledDate = dto.ScheduledDate,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var created = await _paymentService.CreateAsync(payment, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an existing payment.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PaymentDto>> Update(int id, [FromBody] Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            if (id != payment.Id)
                return BadRequest("ID mismatch");
            var updated = await _paymentService.UpdateAsync(payment, cancellationToken);
            return Ok(MapToDto(updated));
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
            if (!result)
                return NotFound($"Payment {id} not found");
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
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _paymentService.ProcessPaymentAsync(
                request.InvoiceId, request.Amount, request.Method, request.Details ?? new PaymentDetails(), cancellationToken);
            if (!result.Success)
                return BadRequest(result);
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
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _paymentService.ProcessRefundAsync(id, request.Amount, request.Reason, cancellationToken);
            if (!result.Success)
                return BadRequest(result);
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResult>> VoidPayment(int id, [FromBody] VoidPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _paymentService.VoidPaymentAsync(id, request.Reason, cancellationToken);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
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
            if (!result.Success)
                return BadRequest(result);
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
    public async Task<ActionResult<PaymentDto>> UpdateStatus(int id, [FromBody] PaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(MapToDto(payment));
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
    public async Task<ActionResult<PaymentDto>> MarkAsCompleted(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.MarkAsCompletedAsync(id, cancellationToken);
            return Ok(MapToDto(payment));
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
    public async Task<ActionResult<PaymentDto>> MarkAsFailed(int id, [FromBody] FailPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentService.MarkAsFailedAsync(id, request.FailureReason, cancellationToken);
            return Ok(MapToDto(payment));
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByDateRange(
        [FromQuery][Required] DateTime fromDate,
        [FromQuery][Required] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByDateRangeAsync(fromDate, toDate, cancellationToken);
            return Ok(payments.Select(MapToDto));
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPendingPayments(CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetPendingPaymentsAsync(cancellationToken);
            return Ok(payments.Select(MapToDto));
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetFailedPayments([FromQuery] int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetFailedPaymentsAsync(maxRetries, cancellationToken);
            return Ok(payments.Select(MapToDto));
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAccountPaymentHistory(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetAccountPaymentHistoryAsync(accountId, cancellationToken);
            return Ok(payments.Select(MapToDto));
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReconcilePayment(int id, [FromBody] ReconcileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _paymentService.ReconcilePaymentAsync(id, request.ExternalReference, cancellationToken);
            if (!success)
                return NotFound($"Payment {id} not found");
            return NoContent();
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUnreconciledPayments(CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentService.GetUnreconciledPaymentsAsync(cancellationToken);
            return Ok(payments.Select(MapToDto));
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
            if (!result.Success)
                return BadRequest(result);
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
    public async Task<ActionResult<PaymentDto>> SchedulePayment([FromBody] SchedulePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var paymentEntity = new Payment
            {
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                PaymentMethod = request.Method,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var payment = await _paymentService.SchedulePaymentAsync(paymentEntity, request.ScheduledDate, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, MapToDto(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling payment for invoice {InvoiceId}", request.InvoiceId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Helpers

    private static PaymentDto MapToDto(Payment payment) => new()
    {
        Id = payment.Id,
        PaymentNumber = payment.PaymentNumber,
        InvoiceId = payment.InvoiceId,
        InvoiceNumber = payment.Invoice?.InvoiceNumber,
        AccountId = payment.AccountId,
        AccountName = payment.Account?.DisplayName,
        Amount = payment.Amount,
        RefundedAmount = payment.RefundedAmount,
        AmountApplied = payment.AmountApplied,
        PaymentMethod = payment.PaymentMethod,
        PaymentType = payment.PaymentType,
        Status = payment.Status,
        PaymentDate = payment.PaymentDate,
        ProcessedDate = payment.ProcessedDate,
        RefundDate = payment.RefundDate,
        ScheduledDate = payment.ScheduledDate,
        TransactionId = payment.TransactionId,
        AuthorizationCode = payment.AuthorizationCode,
        CardLast4 = payment.CardLast4,
        CardholderName = payment.CardholderName,
        BankReference = payment.BankReference,
        IsReconciled = payment.IsReconciled,
        ReconciledDate = payment.ReconciledDate,
        Description = payment.Description,
        FailureReason = payment.FailureReason,
        RetryCount = payment.RetryCount,
        OriginalPaymentId = payment.OriginalPaymentId,
        ExternalPaymentId = payment.ExternalPaymentId,
        GatewayReference = payment.GatewayReference,
        CheckNumber = payment.CheckNumber,
        ProcessingFee = payment.ProcessingFee,
        NetAmount = payment.NetAmount,
        ExchangeRate = payment.ExchangeRate,
        SettledDate = payment.SettledDate,
        DepositDate = payment.DepositDate,
        CardBrand = payment.CardBrand,
        CardExpMonth = payment.CardExpMonth,
        CardExpYear = payment.CardExpYear,
        BankName = payment.BankName,
        AccountLast4 = payment.AccountLast4,
        AccountType = payment.AccountType,
        Gateway = payment.Gateway,
        GatewayResponseCode = payment.GatewayResponseCode,
        GatewayResponseMessage = payment.GatewayResponseMessage,
        OrderId = payment.OrderId,
        SubscriptionId = payment.SubscriptionId,
        InternalNotes = payment.InternalNotes,
    };

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
