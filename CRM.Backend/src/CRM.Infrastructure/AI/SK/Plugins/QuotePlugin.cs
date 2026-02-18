// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

#nullable enable

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM quote management operations.
/// Provides AI-accessible functions for querying, creating, and updating sales quotes.
/// </summary>
public class QuotePlugin : CrmPluginBase
{
    private readonly IQuoteService _quoteService;

    /// <inheritdoc />
    public override string PluginName => "Quote";

    /// <inheritdoc />
    public override string Description => "Manage CRM sales quotes — search quotes, view details, check statistics, create new quotes, and send quotes to customers.";

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotePlugin"/> class.
    /// </summary>
    /// <param name="quoteService">The quote service for quote operations.</param>
    /// <param name="logger">The logger instance.</param>
    public QuotePlugin(
        IQuoteService quoteService,
        ILogger<QuotePlugin> logger) : base(logger)
    {
        _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
    }

    #region Read Operations

    /// <summary>
    /// Retrieves a specific quote by its ID.
    /// </summary>
    /// <param name="quoteId">The quote ID to retrieve.</param>
    /// <returns>A JSON object with the quote details.</returns>
    [KernelFunction("GetQuote")]
    [Description("Get a specific sales quote by its ID including all line items and totals.")]
    public async Task<string> GetQuoteAsync(
        [Description("The ID of the quote to retrieve")] int quoteId)
    {
        try
        {
            var quote = await _quoteService.GetByIdAsync(quoteId);

            if (quote == null)
            {
                return SuccessResult(new { found = false, message = $"Quote {quoteId} not found" });
            }

            return SuccessResult(new
            {
                quote.Id,
                quote.QuoteNumber,
                quote.Name,
                Status = quote.Status.ToString(),
                quote.ExpirationDate,
                quote.SentDate,
                quote.Subtotal,
                quote.Discount,
                quote.Tax,
                quote.Total,
                quote.AccountId,
                quote.OpportunityId,
                quote.Notes,
                quote.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("GetQuote", ex.Message);
        }
    }

    /// <summary>
    /// Searches quotes with optional filters for account, opportunity, and status.
    /// </summary>
    /// <param name="accountId">Optional account ID to filter by.</param>
    /// <param name="opportunityId">Optional opportunity ID to filter by.</param>
    /// <param name="status">Optional status filter (e.g., "Draft", "Sent", "Accepted", "Rejected").</param>
    /// <returns>A JSON array of matching quotes.</returns>
    [KernelFunction("SearchQuotes")]
    [Description("Search sales quotes filtered by account, opportunity, or status.")]
    public async Task<string> SearchQuotesAsync(
        [Description("Account ID to filter by (optional)")] int? accountId = null,
        [Description("Opportunity ID to filter by (optional)")] int? opportunityId = null,
        [Description("Status filter: Draft, Sent, Accepted, Rejected, Expired (optional)")] string? status = null)
    {
        try
        {
            QuoteStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<QuoteStatus>(status, true, out var qs))
            {
                parsedStatus = qs;
            }

            var quotes = await _quoteService.GetQuotesAsync(
                accountId: accountId,
                opportunityId: opportunityId,
                status: parsedStatus);

            var summaries = quotes.Select(q => new
            {
                q.Id,
                q.QuoteNumber,
                q.Name,
                Status = q.Status.ToString(),
                q.Total,
                q.ExpirationDate,
                q.SentDate,
                q.AccountId,
                q.OpportunityId
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("SearchQuotes", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves quote statistics for a given date range.
    /// </summary>
    /// <param name="daysBack">Number of days to look back for statistics. Defaults to 90.</param>
    /// <returns>A JSON object with quote statistics.</returns>
    [KernelFunction("GetQuoteStatistics")]
    [Description("Get quote statistics including counts by status and total values for a given period.")]
    public async Task<string> GetQuoteStatisticsAsync(
        [Description("Number of days to look back for statistics")] int daysBack = 90)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-daysBack);
            var stats = await _quoteService.GetStatisticsAsync(fromDate, DateTime.UtcNow);

            return SuccessResult(stats);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetQuoteStatistics", ex.Message);
        }
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Creates a new sales quote.
    /// </summary>
    /// <param name="name">The quote name/title.</param>
    /// <param name="accountId">The account ID the quote is for.</param>
    /// <param name="opportunityId">Optional related opportunity ID.</param>
    /// <param name="expirationDays">Number of days until the quote expires. Defaults to 30.</param>
    /// <returns>A JSON object with the created quote details.</returns>
    [KernelFunction("CreateQuote")]
    [Description("Create a new sales quote for an account.")]
    [RequiresApproval(Tier = "standard", Description = "Creates a new sales quote")]
    public async Task<string> CreateQuoteAsync(
        [Description("Quote name or title")] string name,
        [Description("Account ID the quote is for")] int accountId,
        [Description("Related opportunity ID (optional)")] int? opportunityId = null,
        [Description("Number of days until the quote expires")] int expirationDays = 30)
    {
        try
        {
            var quote = new Quote
            {
                Name = name,
                AccountId = accountId,
                OpportunityId = opportunityId,
                Status = QuoteStatus.Draft,
                ExpirationDate = DateTime.UtcNow.AddDays(expirationDays),
                CreatedAt = DateTime.UtcNow
            };

            var created = await _quoteService.CreateAsync(quote);

            return SuccessResult(new
            {
                success = true,
                quoteId = created.Id,
                quoteNumber = created.QuoteNumber,
                name = created.Name,
                status = created.Status.ToString(),
                expirationDate = created.ExpirationDate,
                message = "Quote created successfully"
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("CreateQuote", ex.Message);
        }
    }

    /// <summary>
    /// Sends a quote to the account for review.
    /// </summary>
    /// <param name="quoteId">The ID of the quote to send.</param>
    /// <returns>A JSON object indicating send success or failure.</returns>
    [KernelFunction("SendQuote")]
    [Description("Send a quote to the account for review.")]
    [RequiresApproval(Tier = "standard", Description = "Sends a quote to the account")]
    public async Task<string> SendQuoteAsync(
        [Description("The ID of the quote to send")] int quoteId)
    {
        try
        {
            var success = await _quoteService.SendAsync(quoteId);

            if (!success)
            {
                return SuccessResult(new { success = false, quoteId, message = "Failed to send quote." });
            }

            var quote = await _quoteService.GetByIdAsync(quoteId);

            return SuccessResult(new
            {
                success = true,
                quoteId,
                quoteNumber = quote?.QuoteNumber,
                status = quote?.Status.ToString(),
                sentDate = quote?.SentDate,
                message = "Quote sent to customer successfully"
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("SendQuote", ex.Message);
        }
    }

    #endregion
}
