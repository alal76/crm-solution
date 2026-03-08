// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing sales quotes
/// </summary>
public class QuoteService : IQuoteService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(ICrmDbContext context, ILogger<QuoteService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quote>> GetQuotesAsync(
        int? accountId = null,
        int? opportunityId = null,
        QuoteStatus? status = null,
        bool? expired = null)
    {
        _logger.LogDebug(
            "Getting quotes with filters: AccountId={AccountId}, OpportunityId={OpportunityId}, Status={Status}, Expired={Expired}",
            accountId, opportunityId, status, expired);

        var query = _context.Quotes.AsNoTracking().Where(q => !q.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(q => q.AccountId == accountId);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(q => q.OpportunityId == opportunityId);
        }

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status);
        }

        if (expired.HasValue)
        {
            var now = DateTime.UtcNow;
            if (expired.Value)
            {
                query = query.Where(q => q.ExpirationDate.HasValue && q.ExpirationDate < now);
            }
            else
            {
                query = query.Where(q => !q.ExpirationDate.HasValue || q.ExpirationDate >= now);
            }
        }

        var quotes = await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} quotes", quotes.Count);
        return quotes;
    }

    /// <inheritdoc />
    public async Task<Quote?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting quote by ID: {QuoteId}", id);

        var quote = await _context.Quotes
            .AsNoTracking()
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found: {QuoteId}", id);
        }

        return quote;
    }

    /// <inheritdoc />
    public async Task<Quote?> GetByQuoteNumberAsync(string quoteNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(quoteNumber);

        _logger.LogDebug("Getting quote by number: {QuoteNumber}", quoteNumber);

        var quote = await _context.Quotes
            .AsNoTracking()
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found: {QuoteNumber}", quoteNumber);
        }

        return quote;
    }

    /// <inheritdoc />
    public async Task<Quote> CreateAsync(Quote quote)
    {
        ArgumentNullException.ThrowIfNull(quote);

        _logger.LogDebug("Creating quote: {Name}", quote.Name);

        quote.CreatedAt = DateTime.UtcNow;
        quote.IsDeleted = false;

        // Generate quote number if not provided
        if (string.IsNullOrWhiteSpace(quote.QuoteNumber))
        {
            quote.QuoteNumber = await GenerateQuoteNumberAsync();
        }

        // Set default status
        if (quote.Status == default)
        {
            quote.Status = QuoteStatus.New;
        }

        // Set expiration date based on validity days
        if (!quote.ExpirationDate.HasValue && quote.ValidityDays.HasValue)
        {
            quote.ExpirationDate = DateTime.UtcNow.AddDays(quote.ValidityDays.Value);
        }

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created quote with ID: {QuoteId}, Number: {QuoteNumber}", quote.Id, quote.QuoteNumber);
        return quote;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, Quote quote)
    {
        ArgumentNullException.ThrowIfNull(quote);

        _logger.LogDebug("Updating quote: {QuoteId}", id);

        var existingQuote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (existingQuote == null)
        {
            _logger.LogWarning("Quote not found for update: {QuoteId}", id);
            return false;
        }

        // Update properties
        existingQuote.Name = quote.Name;
        existingQuote.Description = quote.Description;
        existingQuote.Status = quote.Status;
        existingQuote.ExpirationDate = quote.ExpirationDate;
        existingQuote.Subtotal = quote.Subtotal;
        existingQuote.Discount = quote.Discount;
        existingQuote.DiscountPercent = quote.DiscountPercent;
        existingQuote.DiscountReason = quote.DiscountReason;
        existingQuote.Tax = quote.Tax;
        existingQuote.TaxRate = quote.TaxRate;
        existingQuote.ShippingCost = quote.ShippingCost;
        existingQuote.Total = quote.Total;
        existingQuote.CurrencyCode = quote.CurrencyCode;
        existingQuote.PaymentTerms = quote.PaymentTerms;
        existingQuote.DeliveryTerms = quote.DeliveryTerms;
        existingQuote.TermsAndConditions = quote.TermsAndConditions;
        existingQuote.Notes = quote.Notes;
        existingQuote.InternalNotes = quote.InternalNotes;
        existingQuote.AccountId = quote.AccountId;
        existingQuote.ContactId = quote.ContactId;
        existingQuote.OpportunityId = quote.OpportunityId;
        existingQuote.AssignedToUserId = quote.AssignedToUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated quote: {QuoteId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting quote: {QuoteId}", id);

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found for deletion: {QuoteId}", id);
            return false;
        }

        // Only allow deletion of draft quotes
        if (quote.Status != QuoteStatus.Draft && quote.Status != QuoteStatus.New)
        {
            _logger.LogWarning("Cannot delete quote {QuoteId} with status {Status}", id, quote.Status);
            return false;
        }

        // Soft delete
        quote.IsDeleted = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted quote: {QuoteId}", id);
        return true;
    }

    /// <inheritdoc /> // PRA-017: added UpdateStatusAsync
    public async Task<Quote?> UpdateStatusAsync(int id, QuoteStatus newStatus)
    {
        _logger.LogDebug("Updating status for quote: {QuoteId} to {NewStatus}", id, newStatus);

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found for status update: {QuoteId}", id);
            return null;
        }

        quote.Status = newStatus;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated status of quote: {QuoteId} to {NewStatus}", id, newStatus);
        return quote;
    }

    /// <inheritdoc />
    public async Task<bool> SendAsync(int id)    {
        _logger.LogDebug("Sending quote: {QuoteId}", id);

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found for sending: {QuoteId}", id);
            return false;
        }

        // Update status to Shared and set sent date
        quote.Status = QuoteStatus.Shared;
        quote.SentDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Sent quote: {QuoteId}, Status changed to Shared", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> AcceptAsync(int id)
    {
        _logger.LogDebug("Accepting quote: {QuoteId}", id);

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found for acceptance: {QuoteId}", id);
            return false;
        }

        // Update status to Accepted
        quote.Status = QuoteStatus.Accepted;
        quote.AcceptedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Accepted quote: {QuoteId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RejectAsync(int id, string? reason = null)
    {
        _logger.LogDebug("Rejecting quote: {QuoteId}", id);

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (quote == null)
        {
            _logger.LogWarning("Quote not found for rejection: {QuoteId}", id);
            return false;
        }

        // Update status to Rejected
        quote.Status = QuoteStatus.Rejected;
        quote.RejectedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            quote.InternalNotes = string.IsNullOrWhiteSpace(quote.InternalNotes)
                ? $"Rejection reason: {reason}"
                : $"{quote.InternalNotes}\n\nRejection reason: {reason}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Rejected quote: {QuoteId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<Quote> CreateRevisionAsync(int id)
    {
        _logger.LogDebug("Creating revision for quote: {QuoteId}", id);

        var originalQuote = await _context.Quotes
            .AsNoTracking()
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (originalQuote == null)
        {
            _logger.LogWarning("Quote not found for revision: {QuoteId}", id);
            throw new InvalidOperationException($"Quote with ID {id} not found");
        }

        // Mark original as revised
        var tracked = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        if (tracked != null)
        {
            tracked.Status = QuoteStatus.Revised;
        }

        // Create new revision
        var newQuote = new Quote
        {
            QuoteNumber = await GenerateQuoteNumberAsync(),
            Name = originalQuote.Name,
            Description = originalQuote.Description,
            Status = QuoteStatus.Draft,
            Version = originalQuote.Version + 1,
            ParentQuoteId = originalQuote.Id,
            QuoteDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(originalQuote.ValidityDays ?? 30),
            ValidityDays = originalQuote.ValidityDays,
            Subtotal = originalQuote.Subtotal,
            Discount = originalQuote.Discount,
            DiscountPercent = originalQuote.DiscountPercent,
            DiscountReason = originalQuote.DiscountReason,
            Tax = originalQuote.Tax,
            TaxRate = originalQuote.TaxRate,
            ShippingCost = originalQuote.ShippingCost,
            Total = originalQuote.Total,
            CurrencyCode = originalQuote.CurrencyCode,
            PaymentTerms = originalQuote.PaymentTerms,
            DeliveryTerms = originalQuote.DeliveryTerms,
            TermsAndConditions = originalQuote.TermsAndConditions,
            Notes = originalQuote.Notes,
            InternalNotes = $"Revision of quote #{originalQuote.QuoteNumber}",
            AccountId = originalQuote.AccountId,
            ContactId = originalQuote.ContactId,
            OpportunityId = originalQuote.OpportunityId,
            AssignedToUserId = originalQuote.AssignedToUserId,
            CreatedByUserId = originalQuote.CreatedByUserId,
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
            ContactName = originalQuote.ContactName,
            ContactEmail = originalQuote.ContactEmail,
            ContactPhone = originalQuote.ContactPhone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Quotes.Add(newQuote);
        await _context.SaveChangesAsync();

        // Note: Line items are copied through navigation property if available
        // The calling code should handle copying line items if needed using
        // the QuoteLineItems navigation property on the Quote entity
        _logger.LogInformation("Created revision for quote: {OriginalQuoteId} -> {NewQuoteId}", id, newQuote.Id);
        return newQuote;
    }

    /// <inheritdoc />
    public async Task<QuoteStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        _logger.LogDebug("Getting quote statistics from {FromDate} to {ToDate}", fromDate, toDate);

        var query = _context.Quotes.AsNoTracking().Where(q => !q.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(q => q.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(q => q.CreatedAt <= toDate.Value);
        }

        var quotes = await query.ToListAsync();
        var now = DateTime.UtcNow;

        var totalQuotes = quotes.Count;
        var acceptedQuotes = quotes.Count(q => q.Status == QuoteStatus.Accepted);

        var statistics = new QuoteStatistics
        {
            TotalQuotes = totalQuotes,
            DraftQuotes = quotes.Count(q => q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.New),
            SentQuotes = quotes.Count(q => q.Status == QuoteStatus.Shared || q.Status == QuoteStatus.Viewed),
            AcceptedQuotes = acceptedQuotes,
            RejectedQuotes = quotes.Count(q => q.Status == QuoteStatus.Rejected),
            ExpiredQuotes = quotes.Count(q => q.Status == QuoteStatus.Expired ||
                                         (q.ExpirationDate.HasValue && q.ExpirationDate < now &&
                                          q.Status != QuoteStatus.Accepted &&
                                          q.Status != QuoteStatus.Rejected &&
                                          q.Status != QuoteStatus.Converted)),
            TotalValue = quotes.Sum(q => q.Total),
            AcceptedValue = quotes.Where(q => q.Status == QuoteStatus.Accepted).Sum(q => q.Total),
            AcceptanceRate = totalQuotes > 0 ? (double)acceptedQuotes / totalQuotes * 100 : 0
        };

        _logger.LogInformation(
            "Quote statistics - Total: {Total}, Draft: {Draft}, Sent: {Sent}, Accepted: {Accepted}, TotalValue: {TotalValue}",
            statistics.TotalQuotes, statistics.DraftQuotes, statistics.SentQuotes, statistics.AcceptedQuotes, statistics.TotalValue);

        return statistics;
    }

    /// <summary>
    /// Generates a unique quote number
    /// </summary>
    private async Task<string> GenerateQuoteNumberAsync()
    {
        var prefix = "Q";
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var month = DateTime.UtcNow.Month.ToString("D2");

        // Get the count of quotes created this month
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var count = await _context.Quotes
            .CountAsync(q => q.CreatedAt >= startOfMonth);

        var sequence = (count + 1).ToString("D4");
        return $"{prefix}{year}{month}-{sequence}";
    }
}
