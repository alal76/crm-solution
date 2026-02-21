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
/// API controller for invoice management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all invoices with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] InvoiceStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _invoiceService.GetAllAsync(accountId, status, cancellationToken);
            return Ok(invoices.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets an invoice by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.GetByIdAsync(id, cancellationToken);
            if (invoice == null)
                return NotFound($"Invoice {id} not found");
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets an invoice by invoice number.</summary>
    [HttpGet("by-number/{invoiceNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> GetByInvoiceNumber(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.GetByInvoiceNumberAsync(invoiceNumber, cancellationToken);
            if (invoice == null)
                return NotFound($"Invoice '{invoiceNumber}' not found");
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice by number {InvoiceNumber}", invoiceNumber);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Creates a new invoice.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var invoice = new Invoice
            {
                AccountId = request.AccountId,
                InvoiceDate = request.InvoiceDate ?? DateTime.UtcNow,
                DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(30),
                Status = request.Status,
                InvoiceType = request.InvoiceType,
                PaymentTerms = request.PaymentTerms,
                Subtotal = request.Subtotal,
                DiscountAmount = request.DiscountAmount,
                TaxAmount = request.TaxAmount,
                ShippingAmount = request.ShippingAmount,
                FeesAmount = request.FeesAmount,
                CurrencyCode = request.CurrencyCode,
                Description = request.Description,
                Notes = request.Notes,
                OrderId = request.OrderId,
                InternalNotes = request.InternalNotes,
                TermsAndConditions = request.TermsAndConditions,
                BillingName = request.BillingName,
                BillingStreet = request.BillingStreet,
                BillingCity = request.BillingCity,
                BillingState = request.BillingState,
                BillingCountry = request.BillingCountry,
            };
            var created = await _invoiceService.CreateAsync(invoice, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an existing invoice.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> Update(int id, [FromBody] Invoice invoice, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            if (id != invoice.Id)
                return BadRequest("ID mismatch");
            var updated = await _invoiceService.UpdateAsync(invoice, cancellationToken);
            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Deletes an invoice.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _invoiceService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound($"Invoice {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Invoice Operations

    /// <summary>Creates an invoice from an existing order.</summary>
    [HttpPost("from-order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> CreateFromOrder(int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.CreateFromOrderAsync(orderId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from order {OrderId}", orderId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Creates an invoice from an existing quote.</summary>
    [HttpPost("from-quote/{quoteId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> CreateFromQuote(int quoteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.CreateFromQuoteAsync(quoteId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from quote {QuoteId}", quoteId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Generates the next invoice number.</summary>
    [HttpGet("next-number")]
    public async Task<ActionResult<string>> GenerateInvoiceNumber(CancellationToken cancellationToken = default)
    {
        try
        {
            var number = await _invoiceService.GenerateInvoiceNumberAsync(cancellationToken);
            return Ok(number);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice number");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Sends an invoice to the customer.</summary>
    [HttpPost("{id}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendInvoice(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _invoiceService.SendInvoiceAsync(id, cancellationToken);
            if (!result)
                return NotFound($"Invoice {id} not found");
            return Ok(new { message = "Invoice sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Marks an invoice as viewed.</summary>
    [HttpPost("{id}/viewed")]
    public async Task<IActionResult> MarkAsViewed(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _invoiceService.MarkAsViewedAsync(id, cancellationToken);
            if (!result)
                return NotFound($"Invoice {id} not found");
            return Ok(new { message = "Invoice marked as viewed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking invoice {InvoiceId} as viewed", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Status Management

    /// <summary>Updates the status of an invoice.</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> UpdateStatus(int id, [FromBody] InvoiceStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Approves an invoice.</summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> Approve(int id, [FromBody] ApproveRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.ApproveAsync(id, request.ApprovedById, cancellationToken);
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Voids an invoice.</summary>
    [HttpPost("{id}/void")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> VoidInvoice(int id, [FromBody] VoidRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.VoidAsync(id, request.Reason, cancellationToken);
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Marks an invoice as paid.</summary>
    [HttpPost("{id}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> MarkAsPaid(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.MarkAsPaidAsync(id, cancellationToken);
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Payment Operations

    /// <summary>Records a payment against an invoice.</summary>
    [HttpPost("{id}/payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Payment>> RecordPayment(int id, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var payment = await _invoiceService.RecordPaymentAsync(id, request.Amount, request.Method, cancellationToken);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment for invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets the outstanding balance for an invoice.</summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<decimal>> GetOutstandingBalance(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _invoiceService.GetOutstandingBalanceAsync(id, cancellationToken);
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets all payments for an invoice.</summary>
    [HttpGet("{id}/payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _invoiceService.GetPaymentsAsync(id, cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Queries

    /// <summary>Gets all overdue invoices.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetOverdueInvoices([FromQuery] int? daysPastDue = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _invoiceService.GetOverdueInvoicesAsync(daysPastDue, cancellationToken);
            return Ok(invoices.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue invoices");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets invoices due within the specified date range.</summary>
    [HttpGet("due")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoicesDue([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _invoiceService.GetInvoicesDueAsync(fromDate, toDate, cancellationToken);
            return Ok(invoices.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices due between {FromDate} and {ToDate}", fromDate, toDate);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets invoice statistics for an account.</summary>
    [HttpGet("statistics/{accountId}")]
    public async Task<ActionResult<InvoiceStatistics>> GetAccountStatistics(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _invoiceService.GetAccountStatisticsAsync(accountId, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice statistics for account {AccountId}", accountId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Line Items

    /// <summary>Adds a line item to an invoice.</summary>
    [HttpPost("{id}/line-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceLineItem>> AddLineItem(int id, [FromBody] InvoiceLineItem lineItem, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var created = await _invoiceService.AddLineItemAsync(id, lineItem, cancellationToken);
            return Ok(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding line item to invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates a line item.</summary>
    [HttpPut("line-items/{lineItemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceLineItem>> UpdateLineItem(int lineItemId, [FromBody] InvoiceLineItem lineItem, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            if (lineItemId != lineItem.Id)
                return BadRequest("ID mismatch");
            var updated = await _invoiceService.UpdateLineItemAsync(lineItem, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating line item {LineItemId}", lineItemId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Removes a line item from an invoice.</summary>
    [HttpDelete("line-items/{lineItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveLineItem(int lineItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _invoiceService.RemoveLineItemAsync(lineItemId, cancellationToken);
            if (!result)
                return NotFound($"Line item {lineItemId} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing line item {LineItemId}", lineItemId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets all line items for an invoice.</summary>
    [HttpGet("{id}/line-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InvoiceLineItem>>> GetLineItems(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _invoiceService.GetLineItemsAsync(id, cancellationToken);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting line items for invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Calculations

    /// <summary>Recalculates the totals for an invoice.</summary>
    [HttpPost("{id}/recalculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceDto>> RecalculateTotals(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.RecalculateTotalsAsync(id, cancellationToken);
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating totals for invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Applies a discount to an invoice.</summary>
    [HttpPost("{id}/discount")]
    public async Task<ActionResult<InvoiceDto>> ApplyDiscount(int id, [FromBody] DiscountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _invoiceService.ApplyDiscountAsync(id, request.DiscountAmount, request.Reason, cancellationToken);
            return Ok(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount to invoice {InvoiceId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Mapping

    private static InvoiceDto MapToDto(Invoice invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        AccountId = invoice.AccountId,
        AccountName = invoice.Account?.Company
            ?? invoice.Account?.FirstName
            ?? invoice.Account?.LastName,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        SentDate = invoice.SentDate,
        ViewedDate = invoice.ViewedDate,
        PaidDate = invoice.PaidDate,
        VoidedDate = invoice.VoidedDate,
        Status = invoice.Status,
        InvoiceType = invoice.InvoiceType,
        PaymentTerms = invoice.PaymentTerms,
        Subtotal = invoice.Subtotal,
        DiscountAmount = invoice.DiscountAmount,
        TaxAmount = invoice.TaxAmount,
        ShippingAmount = invoice.ShippingAmount,
        FeesAmount = invoice.FeesAmount,
        TotalAmount = invoice.TotalAmount,
        AmountPaid = invoice.AmountPaid,
        BalanceDue = invoice.BalanceDue,
        CurrencyCode = invoice.CurrencyCode,
        Description = invoice.Description,
        Notes = invoice.Notes,
        VoidReason = invoice.VoidReason,
        OrderId = invoice.OrderId,
        ServicePeriodStart = invoice.ServicePeriodStart,
        ServicePeriodEnd = invoice.ServicePeriodEnd,
        DiscountPercent = invoice.DiscountPercent,
        AmountCredited = invoice.AmountCredited,
        ExchangeRate = invoice.ExchangeRate,
        EarlyPaymentDiscountPercent = invoice.EarlyPaymentDiscountPercent,
        EarlyPaymentDiscountDays = invoice.EarlyPaymentDiscountDays,
        EarlyPaymentDiscountAmount = invoice.EarlyPaymentDiscountAmount,
        LateFeePercent = invoice.LateFeePercent,
        LateFeeAmount = invoice.LateFeeAmount,
        BillingName = invoice.BillingName,
        BillingCompany = invoice.BillingCompany,
        BillingStreet = invoice.BillingStreet,
        BillingCity = invoice.BillingCity,
        BillingState = invoice.BillingState,
        BillingPostalCode = invoice.BillingPostalCode,
        BillingCountry = invoice.BillingCountry,
        BillingEmail = invoice.BillingEmail,
        BillingPhone = invoice.BillingPhone,
        ReminderCount = invoice.ReminderCount,
        LastReminderDate = invoice.LastReminderDate,
        NextReminderDate = invoice.NextReminderDate,
        InCollections = invoice.InCollections,
        InternalNotes = invoice.InternalNotes,
        Footer = invoice.Footer,
        TermsAndConditions = invoice.TermsAndConditions,
        DisputeReason = invoice.DisputeReason,
        PdfUrl = invoice.PdfUrl,
        ContactId = invoice.ContactId,
        SubscriptionId = invoice.SubscriptionId,
        OriginalInvoiceId = invoice.OriginalInvoiceId,
        LineItems = invoice.LineItems.Select(li => new InvoiceLineItemDto
        {
            Id = li.Id,
            InvoiceId = li.InvoiceId,
            LineNumber = li.LineNumber,
            ProductId = li.ProductId,
            ProductName = li.Product?.Name,
            Description = li.Description ?? li.Name,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice,
            DiscountAmount = li.DiscountAmount,
            TaxAmount = li.TaxAmount,
            TotalAmount = li.TotalAmount,
        }).ToList(),
    };

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

    public class InvoiceStatusRequest
    {
        [Required]
        public InvoiceStatus Status { get; set; }
    }

    public class ApproveRequest
    {
        [Required]
        public int ApprovedById { get; set; }
    }

    public class VoidRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class RecordPaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }
    }

    public class DiscountRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        public string? Reason { get; set; }
    }

    #endregion
}
