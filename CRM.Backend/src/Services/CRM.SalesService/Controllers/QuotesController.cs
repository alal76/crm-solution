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

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing quotes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<QuotesController> _logger;
    private readonly NormalizationService _normalization;

    public QuotesController(CrmDbContext context, ILogger<QuotesController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    /// <summary>
    /// Get all quotes with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> GetQuotes(
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
            if (!string.IsNullOrWhiteSpace(nt)) q.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Quote", q.Id);
            if (!string.IsNullOrWhiteSpace(cf)) q.CustomFields = cf;
        }
        return Ok(quotes);
    }

    /// <summary>
    /// Get a quote by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Quote>> GetQuote(int id)
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
        if (!string.IsNullOrWhiteSpace(nt)) quote.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(cf)) quote.CustomFields = cf;

        return Ok(quote);
    }

    /// <summary>
    /// Get a quote by quote number
    /// </summary>
    [HttpGet("number/{quoteNumber}")]
    public async Task<ActionResult<Quote>> GetQuoteByNumber(string quoteNumber)
    {
        var quote = await _context.Quotes
            .Include(q => q.Account)
            .Include(q => q.Opportunity)
            .FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber);

        if (quote == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(nt)) quote.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Quote", quote.Id);
        if (!string.IsNullOrWhiteSpace(cf)) quote.CustomFields = cf;

        return Ok(quote);
    }

    /// <summary>
    /// Create a new quote
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Quote>> CreateQuote(Quote quote)
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
        return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
    }

    /// <summary>
    /// Update an existing quote
    /// </summary>
    [HttpPut("{id}")]
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
    /// Delete a quote
    /// </summary>
    [HttpDelete("{id}")]
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
    /// Send a quote to account
    /// </summary>
    [HttpPost("{id}/send")]
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
        return Ok(quote);
    }

    /// <summary>
    /// Mark quote as viewed
    /// </summary>
    [HttpPost("{id}/viewed")]
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

        return Ok(quote);
    }

    /// <summary>
    /// Accept a quote
    /// </summary>
    [HttpPost("{id}/accept")]
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
        return Ok(quote);
    }

    /// <summary>
    /// Reject a quote
    /// </summary>
    [HttpPost("{id}/reject")]
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
        return Ok(quote);
    }

    /// <summary>
    /// Create a revision of an existing quote
    /// </summary>
    [HttpPost("{id}/revise")]
    public async Task<ActionResult<Quote>> CreateRevision(int id)
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
        return CreatedAtAction(nameof(GetQuote), new { id = revision.Id }, revision);
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
    [HttpGet("{quoteId}/lineitems")]
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
    /// Get a specific line item
    /// </summary>
    [HttpGet("{quoteId}/lineitems/{lineItemId}")]
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
    [HttpPost("{quoteId}/lineitems")]
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
    /// Update a line item
    /// </summary>
    [HttpPut("{quoteId}/lineitems/{lineItemId}")]
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
    /// Delete a line item
    /// </summary>
    [HttpDelete("{quoteId}/lineitems/{lineItemId}")]
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
    /// Reorder line items
    /// </summary>
    [HttpPost("{quoteId}/lineitems/reorder")]
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
