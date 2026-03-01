// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing quotes and quote line items
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class QuotesController : CrmControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<QuotesController> _logger;
    private readonly NormalizationService _normalization;
    private readonly IPdfGenerationService _pdfService;

    public QuotesController(
        CrmDbContext context,
        ILogger<QuotesController> logger,
        NormalizationService normalization,
        IPdfGenerationService pdfService)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
        _pdfService = pdfService;
    }

    /// <summary>
    /// Get all quotes with optional filtering
    /// </summary>
    /// <param name="accountId">Filter by account ID</param>
    /// <param name="opportunityId">Filter by opportunity ID</param>
    /// <param name="status">Filter by quote status</param>
    /// <param name="expired">Filter by expired status (shared but past expiration date)</param>
    /// <returns>List of quotes matching the filters</returns>
    /// <response code="200">Returns the list of quotes</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetQuotes(
        [FromQuery] int? accountId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] QuoteStatus? status = null,
        [FromQuery] bool? expired = null)
    {
        var query = _context.Quotes
            .Include(q => q.Account)
            .Include(q => q.Opportunity)
            .Include(q => q.AssignedToUser)
            .AsQueryable();

        if (accountId.HasValue)
            query = query.Where(q => q.AccountId == accountId);

        if (opportunityId.HasValue)
            query = query.Where(q => q.OpportunityId == opportunityId);

        if (status.HasValue)
            query = query.Where(q => q.Status == status);

        if (expired == true)
            query = query.Where(q => q.ExpirationDate < DateTime.UtcNow && q.Status == QuoteStatus.Shared);

        var quotes = await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
        foreach (var q in quotes)
        {
            var nt = await _normalization.GetTagsAsync("Quote", q.Id);
            if (!string.IsNullOrWhiteSpace(nt))
                q.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Quote", q.Id);
            if (!string.IsNullOrWhiteSpace(cf))
                q.CustomFields = cf;
        }
        return Ok(quotes.Select(MapToDto));
    }

    /// <summary>
    /// Get a quote by ID
    /// </summary>
    /// <param name="id">The unique identifier of the quote</param>
    /// <returns>The quote with the specified ID</returns>
    /// <response code="200">Returns the quote</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuoteDto>> GetQuote(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.Account)
            .Include(q => q.Opportunity)
            .Include(q => q.AssignedToUser)
            .Include(q => q.Revisions)
            .Include(q => q.QuoteLineItems!.Where(li => !li.IsDeleted))
                .ThenInclude(li => li.Product)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(nt))
            quote.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(cf))
            quote.CustomFields = cf;

        return Ok(MapToDto(quote));
    }

    /// <summary>
    /// Get a quote by quote number
    /// </summary>
    /// <param name="quoteNumber">The quote number (e.g., Q-2024-00001)</param>
    /// <returns>The quote with the specified quote number</returns>
    /// <response code="200">Returns the quote</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("number/{quoteNumber}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuoteDto>> GetQuoteByNumber(string quoteNumber)
    {
        var quote = await _context.Quotes
            .Include(q => q.Account)
            .Include(q => q.Opportunity)
            .FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber);

        if (quote == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(nt))
            quote.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(cf))
            quote.CustomFields = cf;

        return Ok(MapToDto(quote));
    }

    /// <summary>
    /// Create a new quote
    /// </summary>
    /// <param name="quote">The quote to create</param>
    /// <returns>The created quote</returns>
    /// <response code="201">Returns the newly created quote</response>
    /// <response code="400">If the quote data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuoteDto>> CreateQuote(Quote quote)
    {
        // Generate quote number if not provided
        if (string.IsNullOrEmpty(quote.QuoteNumber))
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Quotes.CountAsync(q => q.CreatedAt.Year == year) + 1;
            quote.QuoteNumber = $"Q-{year}-{count:D5}";
        }

        quote.CreatedAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.QuoteDate = DateTime.UtcNow;

        // Set expiration date if validity days is set
        if (quote.ValidityDays.HasValue && !quote.ExpirationDate.HasValue)
        {
            quote.ExpirationDate = DateTime.UtcNow.AddDays(quote.ValidityDays.Value);
        }

        // Calculate totals
        CalculateTotals(quote);

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} created for account {AccountId}", quote.QuoteNumber, quote.AccountId);
        return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, MapToDto(quote));
    }

    /// <summary>
    /// Update an existing quote
    /// </summary>
    /// <param name="id">The unique identifier of the quote to update</param>
    /// <param name="quote">The updated quote data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Quote was successfully updated</response>
    /// <response code="400">If the ID mismatch or quote has been accepted/rejected</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateQuote(int id, Quote quote)
    {
        if (id != quote.Id)
            return BadRequest();

        var existingQuote = await _context.Quotes.FindAsync(id);
        if (existingQuote == null)
            return NotFound();

        // Prevent editing accepted/rejected quotes
        if (existingQuote.Status == QuoteStatus.Accepted || existingQuote.Status == QuoteStatus.Rejected)
        {
            return BadRequest("Cannot edit a quote that has been accepted or rejected. Create a revision instead.");
        }

        CalculateTotals(quote);

        _context.Entry(existingQuote).CurrentValues.SetValues(quote);
        existingQuote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a quote (only draft quotes can be deleted)
    /// </summary>
    /// <param name="id">The unique identifier of the quote to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Quote was successfully deleted</response>
    /// <response code="400">If the quote is not in draft status</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null)
            return NotFound();

        // Only allow deleting draft quotes
        if (quote.Status != QuoteStatus.Draft)
        {
            return BadRequest("Only draft quotes can be deleted.");
        }

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} deleted", quote.QuoteNumber);
        return NoContent();
    }

    /// <summary>
    /// Send a quote to account (changes status to Shared)
    /// </summary>
    /// <param name="id">The unique identifier of the quote</param>
    /// <returns>The updated quote</returns>
    /// <response code="200">Returns the updated quote</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/send")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendQuote(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null)
            return NotFound();

        quote.Status = QuoteStatus.Shared;
        quote.SentDate = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} sent", quote.QuoteNumber);
        return Ok(MapToDto(quote));
    }

    /// <summary>
    /// Mark quote as viewed by account
    /// </summary>
    /// <param name="id">The unique identifier of the quote</param>
    /// <returns>The updated quote</returns>
    /// <response code="200">Returns the updated quote</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/viewed")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkViewed(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null)
            return NotFound();

        if (quote.Status == QuoteStatus.Shared)
        {
            quote.Status = QuoteStatus.Viewed;
            quote.ViewedDate = DateTime.UtcNow;
            quote.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok(MapToDto(quote));
    }

    /// <summary>
    /// Accept a quote (account approval)
    /// </summary>
    /// <param name="id">The unique identifier of the quote</param>
    /// <param name="request">Optional signature details</param>
    /// <returns>The updated quote</returns>
    /// <response code="200">Returns the accepted quote</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/accept")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AcceptQuote(int id, [FromBody] AcceptQuoteRequest? request = null)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null)
            return NotFound();

        quote.Status = QuoteStatus.Accepted;
        quote.AcceptedDate = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;

        if (request != null)
        {
            quote.IsSigned = request.IsSigned;
            quote.SignedBy = request.SignedBy;
            quote.SignedDate = request.IsSigned ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} accepted", quote.QuoteNumber);
        return Ok(MapToDto(quote));
    }

    /// <summary>
    /// Reject a quote (account rejection)
    /// </summary>
    /// <param name="id">The unique identifier of the quote</param>
    /// <param name="request">Optional rejection reason</param>
    /// <returns>The updated quote</returns>
    /// <response code="200">Returns the rejected quote</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RejectQuote(int id, [FromBody] RejectQuoteRequest? request = null)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null)
            return NotFound();

        quote.Status = QuoteStatus.Rejected;
        quote.RejectedDate = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;

        if (request != null && !string.IsNullOrEmpty(request.Reason))
        {
            quote.Notes = $"{quote.Notes}\n\nRejection Reason: {request.Reason}".Trim();
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} rejected", quote.QuoteNumber);
        return Ok(MapToDto(quote));
    }

    /// <summary>
    /// Create a revision of an existing quote
    /// </summary>
    /// <param name="id">The unique identifier of the original quote</param>
    /// <returns>The newly created revision quote</returns>
    /// <response code="201">Returns the newly created revision</response>
    /// <response code="404">If the original quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/revise")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuoteDto>> CreateRevision(int id)
    {
        var originalQuote = await _context.Quotes.FindAsync(id);
        if (originalQuote == null)
            return NotFound();

        // Create a new quote as revision
        var revision = new Quote
        {
            QuoteNumber = $"{originalQuote.QuoteNumber}-R{originalQuote.Version + 1}",
            Name = originalQuote.Name,
            Description = originalQuote.Description,
            Status = QuoteStatus.Draft,
            Version = originalQuote.Version + 1,
            ParentQuoteId = originalQuote.Id,
            AccountId = originalQuote.AccountId,
            ContactId = originalQuote.ContactId,
            OpportunityId = originalQuote.OpportunityId,
            AssignedToUserId = originalQuote.AssignedToUserId,
            Subtotal = originalQuote.Subtotal,
            Discount = originalQuote.Discount,
            DiscountPercent = originalQuote.DiscountPercent,
            Tax = originalQuote.Tax,
            TaxRate = originalQuote.TaxRate,
            ShippingCost = originalQuote.ShippingCost,
            Total = originalQuote.Total,
            LineItems = originalQuote.LineItems,
            PaymentTerms = originalQuote.PaymentTerms,
            DeliveryTerms = originalQuote.DeliveryTerms,
            TermsAndConditions = originalQuote.TermsAndConditions,
            ValidityDays = originalQuote.ValidityDays,
            BillingName = originalQuote.BillingName,
            BillingAddress = originalQuote.BillingAddress,
            BillingCity = originalQuote.BillingCity,
            BillingState = originalQuote.BillingState,
            BillingZipCode = originalQuote.BillingZipCode,
            BillingCountry = originalQuote.BillingCountry,
            ShippingName = originalQuote.ShippingName,
            ShippingAddress = originalQuote.ShippingAddress,
            ShippingCity = originalQuote.ShippingCity,
            ShippingState = originalQuote.ShippingState,
            ShippingZipCode = originalQuote.ShippingZipCode,
            ShippingCountry = originalQuote.ShippingCountry,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            QuoteDate = DateTime.UtcNow
        };

        if (revision.ValidityDays.HasValue)
        {
            revision.ExpirationDate = DateTime.UtcNow.AddDays(revision.ValidityDays.Value);
        }

        // Mark original as revised
        originalQuote.Status = QuoteStatus.Revised;
        originalQuote.UpdatedAt = DateTime.UtcNow;

        _context.Quotes.Add(revision);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} revised to {NewQuoteNumber}", originalQuote.QuoteNumber, revision.QuoteNumber);
        return CreatedAtAction(nameof(GetQuote), new { id = revision.Id }, MapToDto(revision));
    }

    private static QuoteDto MapToDto(Quote quote)
    {
        return new QuoteDto
        {
            Id = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            Title = quote.Name,
            AccountId = quote.AccountId ?? 0,
            AccountName = quote.Account?.Company
                ?? quote.Account?.FirstName,
            ContactId = quote.ContactId,
            ContactName = quote.ContactName
                ?? (quote.Contact != null
                    ? $"{quote.Contact.FirstName} {quote.Contact.LastName}".Trim()
                    : null),
            Status = (int)quote.Status,
            Currency = quote.CurrencyCode,
            IssuedDate = quote.QuoteDate.ToString("yyyy-MM-dd"),
            ExpirationDate = quote.ExpirationDate?.ToString("yyyy-MM-dd"),
            Notes = quote.Notes,
            Subtotal = quote.Subtotal,
            DiscountTotal = quote.Discount,
            TaxTotal = quote.Tax,
            GrandTotal = quote.Total,
            LineItems = quote.QuoteLineItems?
                .Where(li => !li.IsDeleted)
                .Select(li => new QuoteLineItemDto
                {
                    Id = li.Id,
                    QuoteId = li.QuoteId,
                    ProductId = li.ProductId ?? 0,
                    ProductName = li.Product?.Name ?? li.Name,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    Discount = li.TotalDiscount,
                    Tax = li.TaxAmount,
                    LineTotal = li.Total,
                })
                .ToList(),
            CreatedAt = quote.CreatedAt.ToString("yyyy-MM-dd"),
            UpdatedAt = quote.UpdatedAt?.ToString("yyyy-MM-dd"),
            IsDeleted = false,
            RowVersion = null,
            CreatedByUserId = quote.CreatedByUserId ?? 0,
            UpdatedByUserId = null,
        };
    }

    private void CalculateTotals(Quote quote)
    {
        // Calculate expected revenue based on discount
        if (quote.DiscountPercent > 0)
        {
            quote.Discount = quote.Subtotal * (quote.DiscountPercent / 100);
        }

        var afterDiscount = quote.Subtotal - quote.Discount;

        if (quote.TaxRate > 0)
        {
            quote.Tax = afterDiscount * (quote.TaxRate / 100);
        }

        quote.Total = afterDiscount + quote.Tax + quote.ShippingCost;
    }

    #region Quote Line Items

    /// <summary>
    /// Get all line items for a quote
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote</param>
    /// <returns>List of line items for the quote</returns>
    /// <response code="200">Returns the list of line items</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{quoteId}/lineitems")]
    [ProducesResponseType(typeof(IEnumerable<QuoteLineItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuoteLineItem>>> GetLineItems(int quoteId)
    {
        var quote = await _context.Quotes.FindAsync(quoteId);
        if (quote == null)
            return NotFound("Quote not found");

        var lineItems = await _context.Set<QuoteLineItem>()
            .Where(li => li.QuoteId == quoteId && !li.IsDeleted)
            .Include(li => li.Product)
            .OrderBy(li => li.LineNumber)
            .ToListAsync();

        return Ok(lineItems);
    }

    /// <summary>
    /// Get a specific line item from a quote
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote</param>
    /// <param name="lineItemId">The unique identifier of the line item</param>
    /// <returns>The line item</returns>
    /// <response code="200">Returns the line item</response>
    /// <response code="404">If the quote or line item is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{quoteId}/lineitems/{lineItemId}")]
    [ProducesResponseType(typeof(QuoteLineItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuoteLineItem>> GetLineItem(int quoteId, int lineItemId)
    {
        var lineItem = await _context.Set<QuoteLineItem>()
            .Include(li => li.Product)
            .FirstOrDefaultAsync(li => li.Id == lineItemId && li.QuoteId == quoteId && !li.IsDeleted);

        if (lineItem == null)
            return NotFound();

        return Ok(lineItem);
    }

    /// <summary>
    /// Add a line item to a quote
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote</param>
    /// <param name="lineItem">The line item to add</param>
    /// <returns>The created line item</returns>
    /// <response code="201">Returns the newly created line item</response>
    /// <response code="400">If the line item data is invalid</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{quoteId}/lineitems")]
    [ProducesResponseType(typeof(QuoteLineItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuoteLineItem>> AddLineItem(int quoteId, [FromBody] QuoteLineItem lineItem)
    {
        var quote = await _context.Quotes
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null)
            return NotFound("Quote not found");

        // Auto-assign line number
        var maxLineNumber = quote.QuoteLineItems?.Where(li => !li.IsDeleted).Max(li => (int?)li.LineNumber) ?? 0;
        lineItem.LineNumber = maxLineNumber + 1;
        lineItem.QuoteId = quoteId;
        lineItem.CreatedAt = DateTime.UtcNow;
        lineItem.UpdatedAt = DateTime.UtcNow;

        // If product is selected, populate from product
        if (lineItem.ProductId.HasValue)
        {
            var product = await _context.Products.FindAsync(lineItem.ProductId.Value);
            if (product != null)
            {
                lineItem.Name = string.IsNullOrEmpty(lineItem.Name) ? product.Name : lineItem.Name;
                lineItem.SKU = product.SKU;
                lineItem.UnitPrice = lineItem.UnitPrice == 0 ? product.Price : lineItem.UnitPrice;
                lineItem.ListPrice = product.ListPrice ?? product.Price;
                lineItem.CostPrice = product.Cost;
                lineItem.Category = product.Category;
            }
        }

        // Calculate line item totals
        lineItem.RecalculateTotals();

        _context.Set<QuoteLineItem>().Add(lineItem);
        await _context.SaveChangesAsync();

        // Recalculate quote totals
        quote.QuoteLineItems = await _context.Set<QuoteLineItem>()
            .Where(li => li.QuoteId == quoteId && !li.IsDeleted)
            .ToListAsync();
        quote.RecalculateFromLineItems();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Line item {LineItemId} added to quote {QuoteId}", lineItem.Id, quoteId);
        return CreatedAtAction(nameof(GetLineItem), new { quoteId, lineItemId = lineItem.Id }, lineItem);
    }

    /// <summary>
    /// Update a line item in a quote
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote</param>
    /// <param name="lineItemId">The unique identifier of the line item</param>
    /// <param name="lineItem">The updated line item data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Line item was successfully updated</response>
    /// <response code="400">If the line item ID mismatch</response>
    /// <response code="404">If the quote or line item is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{quoteId}/lineitems/{lineItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLineItem(int quoteId, int lineItemId, [FromBody] QuoteLineItem lineItem)
    {
        if (lineItemId != lineItem.Id)
            return BadRequest("Line item ID mismatch");

        var existingItem = await _context.Set<QuoteLineItem>()
            .FirstOrDefaultAsync(li => li.Id == lineItemId && li.QuoteId == quoteId);

        if (existingItem == null)
            return NotFound();

        // Update fields
        existingItem.ProductId = lineItem.ProductId;
        existingItem.SKU = lineItem.SKU;
        existingItem.Name = lineItem.Name;
        existingItem.Description = lineItem.Description;
        existingItem.Category = lineItem.Category;
        existingItem.Quantity = lineItem.Quantity;
        existingItem.UnitOfMeasure = lineItem.UnitOfMeasure;
        existingItem.UnitPrice = lineItem.UnitPrice;
        existingItem.ListPrice = lineItem.ListPrice;
        existingItem.CostPrice = lineItem.CostPrice;
        existingItem.DiscountType = lineItem.DiscountType;
        existingItem.DiscountPercent = lineItem.DiscountPercent;
        existingItem.DiscountAmount = lineItem.DiscountAmount;
        existingItem.DiscountReason = lineItem.DiscountReason;
        existingItem.TaxRate = lineItem.TaxRate;
        existingItem.IsTaxable = lineItem.IsTaxable;
        existingItem.IsIncluded = lineItem.IsIncluded;
        existingItem.IsOptional = lineItem.IsOptional;
        existingItem.InternalNotes = lineItem.InternalNotes;
        existingItem.UpdatedAt = DateTime.UtcNow;

        // Recalculate line item totals
        existingItem.RecalculateTotals();

        await _context.SaveChangesAsync();

        // Recalculate quote totals
        var quote = await _context.Quotes
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote != null)
        {
            quote.RecalculateFromLineItems();
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a line item from a quote (soft delete)
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote</param>
    /// <param name="lineItemId">The unique identifier of the line item to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Line item was successfully deleted</response>
    /// <response code="404">If the quote or line item is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{quoteId}/lineitems/{lineItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLineItem(int quoteId, int lineItemId)
    {
        var lineItem = await _context.Set<QuoteLineItem>()
            .FirstOrDefaultAsync(li => li.Id == lineItemId && li.QuoteId == quoteId);

        if (lineItem == null)
            return NotFound();

        // Soft delete
        lineItem.IsDeleted = true;
        lineItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Recalculate quote totals
        var quote = await _context.Quotes
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote != null)
        {
            quote.RecalculateFromLineItems();
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Line item {LineItemId} deleted from quote {QuoteId}", lineItemId, quoteId);
        return NoContent();
    }

    /// <summary>
    /// Reorder line items in a quote
    /// </summary>
    /// <param name="quoteId">The unique identifier of the quote</param>
    /// <param name="lineItemIds">List of line item IDs in the desired order</param>
    /// <returns>OK on success</returns>
    /// <response code="200">Line items were successfully reordered</response>
    /// <response code="400">If the line item IDs are invalid</response>
    /// <response code="404">If the quote is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{quoteId}/lineitems/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReorderLineItems(int quoteId, [FromBody] List<int> lineItemIds)
    {
        var quote = await _context.Quotes.FindAsync(quoteId);
        if (quote == null)
            return NotFound("Quote not found");

        var lineItems = await _context.Set<QuoteLineItem>()
            .Where(li => li.QuoteId == quoteId && !li.IsDeleted)
            .ToListAsync();

        for (int i = 0; i < lineItemIds.Count; i++)
        {
            var item = lineItems.FirstOrDefault(li => li.Id == lineItemIds[i]);
            if (item != null)
            {
                item.LineNumber = i + 1;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    #endregion

    #region PDF Generation

    /// <summary>
    /// Downloads a quote as a PDF document.
    /// BACK-008: PDF Generation
    /// </summary>
    /// <param name="id">Quote ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file</returns>
    /// <response code="200">Returns PDF bytes with content-type application/pdf</response>
    /// <response code="404">Quote not found</response>
    [HttpGet("{id:int}/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuotePdf(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await _pdfService.GenerateQuotePdfAsync(id, cancellationToken);
            return File(bytes, "application/pdf", $"quote-{id}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = $"Quote {id} not found." });
        }
    }

    #endregion
}

public class AcceptQuoteRequest
{
    public bool IsSigned { get; set; }
    public string? SignedBy { get; set; }
}

public class RejectQuoteRequest
{
    public string? Reason { get; set; }
}
