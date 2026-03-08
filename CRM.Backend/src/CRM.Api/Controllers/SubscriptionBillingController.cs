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
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

// Disambiguate DTOs that exist in both CRM.Core.Dtos and CRM.Core.Interfaces
using BillingHistoryDto = CRM.Core.Dtos.BillingHistoryDto;
using DunningRecordDto = CRM.Core.Dtos.DunningRecordDto;
namespace CRM.Api.Controllers;

/// <summary>
/// API controller for subscription billing operations including invoices, payments, dunning, and metrics.
/// TODO-SALES006-002 / TODO-SALES004-002
/// </summary>
[ApiController]
[Route("api/subscriptions/{subscriptionId:int}/billing")]
[Authorize]
[Produces("application/json")]
public class SubscriptionBillingController : CrmControllerBase
{
    private const string SubscriptionNotFoundMessage = "Subscription {0} not found";
    private readonly ISubscriptionService _subscriptionService;
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SubscriptionBillingController> _logger;

    public SubscriptionBillingController(
        ISubscriptionService subscriptionService,
        IInvoiceService invoiceService,
        IPaymentService paymentService,
        ICrmDbContext dbContext,
        ILogger<SubscriptionBillingController> logger)
    {
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Invoice Management

    /// <summary>
    /// List invoices for a subscription with pagination.
    /// GET /api/subscriptions/{subscriptionId}/billing/invoices
    /// </summary>
    [HttpGet("invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BillingPaginatedResponse<InvoiceDto>>> GetInvoices(
        int subscriptionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var invoices = await _invoiceService.GetAllAsync(subscription.AccountId, null, cancellationToken);
            var subscriptionInvoices = invoices
                .Where(i => i.SubscriptionId == subscriptionId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToList();

            var totalCount = subscriptionInvoices.Count;
            var paginatedInvoices = subscriptionInvoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToInvoiceDto)
                .ToList();

            return Ok(new BillingPaginatedResponse<InvoiceDto>
            {
                Items = paginatedInvoices,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (totalCount + pageSize - 1) / pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get a specific invoice for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/billing/invoices/{invoiceId}
    /// </summary>
    [HttpGet("invoices/{invoiceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(
        int subscriptionId,
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.GetByIdAsync(invoiceId, cancellationToken);
            if (invoice == null || invoice.SubscriptionId != subscriptionId)
            {
                return NotFound($"Invoice {invoiceId} not found for subscription {subscriptionId}");
            }

            return Ok(MapToInvoiceDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId} for subscription {SubscriptionId}", invoiceId, subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Manually trigger invoice generation for a subscription.
    /// POST /api/subscriptions/{subscriptionId}/billing/generate-invoice
    /// </summary>
    [HttpPost("generate-invoice")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> GenerateInvoice(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var invoice = await _subscriptionService.GenerateInvoiceAsync(subscriptionId, cancellationToken);
            return CreatedAtAction(nameof(GetInvoice), new { subscriptionId, invoiceId = invoice.Id }, MapToInvoiceDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Get upcoming scheduled invoices for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/billing/upcoming-invoices
    /// </summary>
    [HttpGet("upcoming-invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetUpcomingInvoices(
        int subscriptionId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var upcomingDate = DateTime.UtcNow.AddDays(days);
            var invoices = await _invoiceService.GetInvoicesDueAsync(DateTime.UtcNow, upcomingDate, cancellationToken);

            var subscriptionInvoices = invoices
                .Where(i => i.SubscriptionId == subscriptionId)
                .Select(MapToInvoiceDto)
                .ToList();

            return Ok(subscriptionInvoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming invoices for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Payment Management

    /// <summary>
    /// List payments for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/billing/payments
    /// </summary>
    [HttpGet("payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var payments = await _paymentService.GetAllAsync(subscription.AccountId, null, null, cancellationToken);
            var subscriptionPayments = payments
                .Where(p => p.Invoice?.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(MapToPaymentDto)
                .ToList();

            return Ok(subscriptionPayments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Record a manual payment for a subscription.
    /// POST /api/subscriptions/{subscriptionId}/billing/payments
    /// </summary>
    [HttpPost("payments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> RecordPayment(
        int subscriptionId,
        [FromBody] RecordSubscriptionPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var invoice = await _invoiceService.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null || invoice.SubscriptionId != subscriptionId)
            {
                return NotFound($"Invoice {request.InvoiceId} not found for subscription {subscriptionId}");
            }

            // Parse payment method string → enum; default to Other for manual entries
            var paymentMethodEnum = Enum.TryParse<PaymentMethod>(request.PaymentMethod ?? "Other", ignoreCase: true, out var pm)
                ? pm
                : PaymentMethod.Other;

            var payment = new Payment
            {
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
                PaymentMethod = paymentMethodEnum,
                Status = PaymentStatus.Completed,
                Notes = request.Notes
            };

            var created = await _paymentService.CreateAsync(payment, cancellationToken);
            return CreatedAtAction(nameof(GetPayments), new { subscriptionId }, MapToPaymentDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Billing History (BillingHistory entity)

    /// <summary>
    /// Get billing history records for a subscription (BillingHistory audit trail).
    /// GET /api/subscriptions/{subscriptionId}/billing/history
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<BillingHistoryDto>>> GetBillingHistory(
        int subscriptionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var records = await _dbContext.BillingHistories
                .Where(b => b.SubscriptionId == subscriptionId && !b.IsDeleted)
                .OrderByDescending(b => b.EventDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = records.Select(MapToBillingHistoryDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing history for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Dunning Management

    /// <summary>
    /// Get dunning records for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/billing/dunning
    /// </summary>
    [HttpGet("dunning")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DunningRecordDto>>> GetDunningRecords(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var records = await _dbContext.DunningRecords
                .Where(d => d.SubscriptionId == subscriptionId && !d.IsDeleted)
                .OrderByDescending(d => d.InitialFailureDate)
                .ToListAsync(cancellationToken);

            var dtos = records.Select(MapToDunningRecordDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dunning records for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Retry a failed payment in dunning.
    /// POST /api/subscriptions/{subscriptionId}/billing/retry-dunning
    /// TODO: Implement full retry logic via IDunningManager once that service is enabled.
    /// </summary>
    [HttpPost("retry-dunning")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RetryDunningResponse>> RetryDunning(
        int subscriptionId,
        [FromBody] RetryDunningRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            // Find the active dunning record
            var dunning = await _dbContext.DunningRecords
                .Where(d => d.SubscriptionId == subscriptionId && !d.IsDeleted && d.Status == DunningStatus.Active)
                .OrderByDescending(d => d.InitialFailureDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (dunning == null)
            {
                return NotFound($"No active dunning record found for subscription {subscriptionId}");
            }

            // TODO: Wire to IDunningManager.RetryAsync(dunning.Id, cancellationToken) once enabled // NOSONAR
            // For now, mark the next retry date as now to trigger the background processor
            dunning.NextRetryDate = DateTime.UtcNow;
            dunning.RetryAttempt++;
            dunning.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Dunning retry queued for subscription {SubscriptionId}, attempt {Attempt}",
                subscriptionId, dunning.RetryAttempt);

            return Accepted(new RetryDunningResponse
            {
                DunningRecordId = dunning.Id,
                SubscriptionId = subscriptionId,
                RetryAttempt = dunning.RetryAttempt,
                NextRetryDate = dunning.NextRetryDate,
                Status = "Queued"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying dunning for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Billing Metrics

    /// <summary>
    /// Get billing metrics for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/billing/metrics
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionBillingMetricsResponse>> GetBillingMetrics(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var invoices = await _invoiceService.GetAllAsync(subscription.AccountId, null, cancellationToken);
            var subInvoices = invoices.Where(i => i.SubscriptionId == subscriptionId).ToList();

            var payments = await _paymentService.GetAllAsync(subscription.AccountId, null, null, cancellationToken);
            var subPayments = payments.Where(p => p.Invoice?.SubscriptionId == subscriptionId).ToList();

            var totalBilled = subInvoices.Sum(i => i.TotalAmount);
            var totalPaid = subPayments.Sum(p => p.Amount);
            var totalOutstanding = subInvoices.Sum(i => i.BalanceDue);

            var unpaidCount = subInvoices.Count(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Voided);
            var overdueCount = subInvoices.Count(i => i.DueDate < DateTime.UtcNow && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Voided);

            return Ok(new SubscriptionBillingMetricsResponse
            {
                SubscriptionId = subscriptionId,
                TotalBilled = totalBilled,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                InvoiceCount = subInvoices.Count,
                UnpaidInvoiceCount = unpaidCount,
                OverdueInvoiceCount = overdueCount,
                MRR = subscription.MRR ?? 0,
                ARR = subscription.ARR ?? 0,
                CalculatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing metrics for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Credits

    /// <summary>
    /// Get upcoming billing information for a subscription.
    /// GET /api/subscriptions/{subscriptionId}/billing/upcoming
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUpcoming(
        int subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var invoices = await _dbContext.Invoices.AsNoTracking()
                .Where(i => i.SubscriptionId == subscriptionId && i.Status != InvoiceStatus.Paid)
                .OrderBy(i => i.DueDate)
                .ToListAsync(cancellationToken);
            return Ok(new
            {
                subscriptionId,
                nextBillingDate = subscription.NextBillingDate,
                upcomingInvoices = invoices.Select(MapToInvoiceDto)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming billing for subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>
    /// Apply a credit memo to a subscription invoice.
    /// POST /api/subscriptions/{subscriptionId}/billing/apply-credit
    /// </summary>
    [HttpPost("apply-credit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> ApplyCredit(
        int subscriptionId,
        [FromBody] ApplyCreditRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var subscription = await _subscriptionService.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return NotFound(string.Format(SubscriptionNotFoundMessage, subscriptionId));
            }

            var invoice = await _invoiceService.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null || invoice.SubscriptionId != subscriptionId)
            {
                return NotFound($"Invoice {request.InvoiceId} not found for subscription {subscriptionId}");
            }

            var updated = await _invoiceService.ApplyDiscountAsync(request.InvoiceId, request.CreditAmount, request.Reason, cancellationToken);
            return Ok(MapToInvoiceDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying credit to subscription {SubscriptionId}", subscriptionId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Helpers

    private static InvoiceDto MapToInvoiceDto(Invoice invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        AccountId = invoice.AccountId,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        Status = invoice.Status,
        Subtotal = invoice.Subtotal,
        TaxAmount = invoice.TaxAmount,
        DiscountAmount = invoice.DiscountAmount,
        TotalAmount = invoice.TotalAmount,
        AmountPaid = invoice.AmountPaid,
        BalanceDue = invoice.BalanceDue,
        CurrencyCode = invoice.CurrencyCode,
        Description = invoice.Description,
        Notes = invoice.Notes,
        SubscriptionId = invoice.SubscriptionId
    };

    private static PaymentDto MapToPaymentDto(Payment payment) => new()
    {
        Id = payment.Id,
        InvoiceId = payment.InvoiceId,
        Amount = payment.Amount,
        PaymentDate = payment.PaymentDate,
        PaymentMethod = payment.PaymentMethod,
        Status = payment.Status
    };

    private static BillingHistoryDto MapToBillingHistoryDto(BillingHistory b) => new()
    {
        Id = b.Id,
        SubscriptionId = b.SubscriptionId,
        InvoiceId = b.InvoiceId,
        CycleStartDate = b.CycleStartDate,
        CycleEndDate = b.CycleEndDate,
        Amount = b.Amount,
        ProratedAmount = b.ProratedAmount,
        UsageCharges = b.UsageCharges,
        DiscountAmount = b.DiscountAmount,
        TaxAmount = b.TaxAmount,
        EventType = b.EventType.ToString(),
        EventDetails = b.EventDetails,
        Status = b.Status,
        BilledDate = b.BilledDate,
        PaidDate = b.PaidDate,
        EventDate = b.EventDate
    };

    private static DunningRecordDto MapToDunningRecordDto(DunningRecord d) => new()
    {
        Id = d.Id,
        SubscriptionId = d.SubscriptionId,
        InvoiceId = d.InvoiceId,
        RetryAttempt = d.RetryAttempt,
        NextRetryDate = d.NextRetryDate,
        Status = d.Status.ToString(),
        Reason = d.Reason,
        InitialFailureDate = d.InitialFailureDate,
        IsExhausted = d.IsExhausted,
        OutstandingAmount = d.OutstandingAmount,
        RecoveredAmount = d.RecoveredAmount
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

    #region Request/Response DTOs

    public class RecordSubscriptionPaymentRequest
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        /// <summary>Payment method string (CreditCard, BankTransfer, WireTransfer, Check, Cash, Other). Defaults to Other.</summary>
        public string? PaymentMethod { get; set; }

        public string? Notes { get; set; }
    }

    public class ApplyCreditRequest
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CreditAmount { get; set; }

        public string? Reason { get; set; }
    }

    public class RetryDunningRequest
    {
        /// <summary>Optional note about the retry attempt.</summary>
        public string? Notes { get; set; }
    }

    public class RetryDunningResponse
    {
        public int DunningRecordId { get; set; }
        public int SubscriptionId { get; set; }
        public int RetryAttempt { get; set; }
        public DateTime NextRetryDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SubscriptionBillingMetricsResponse
    {
        public int SubscriptionId { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int InvoiceCount { get; set; }
        public int UnpaidInvoiceCount { get; set; }
        public int OverdueInvoiceCount { get; set; }
        public decimal MRR { get; set; }
        public decimal ARR { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    public class BillingPaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}
