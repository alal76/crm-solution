using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing quotes
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// Get quotes with optional filtering
    /// </summary>
    Task<IEnumerable<Quote>> GetQuotesAsync(
        int? customerId = null,
        int? opportunityId = null,
        QuoteStatus? status = null,
        bool? expired = null);

    /// <summary>
    /// Get a quote by ID
    /// </summary>
    Task<Quote?> GetByIdAsync(int id);

    /// <summary>
    /// Get a quote by quote number
    /// </summary>
    Task<Quote?> GetByQuoteNumberAsync(string quoteNumber);

    /// <summary>
    /// Create a new quote
    /// </summary>
    Task<Quote> CreateAsync(Quote quote);

    /// <summary>
    /// Update an existing quote
    /// </summary>
    Task<bool> UpdateAsync(int id, Quote quote);

    /// <summary>
    /// Delete a quote (only drafts)
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Send quote to customer
    /// </summary>
    Task<bool> SendAsync(int id);

    /// <summary>
    /// Accept a quote
    /// </summary>
    Task<bool> AcceptAsync(int id);

    /// <summary>
    /// Reject a quote
    /// </summary>
    Task<bool> RejectAsync(int id, string? reason = null);

    /// <summary>
    /// Create a revision of an existing quote
    /// </summary>
    Task<Quote> CreateRevisionAsync(int id);

    /// <summary>
    /// Get quote statistics
    /// </summary>
    Task<QuoteStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Quote statistics DTO
/// </summary>
public class QuoteStatistics
{
    public int TotalQuotes { get; set; }
    public int DraftQuotes { get; set; }
    public int SentQuotes { get; set; }
    public int AcceptedQuotes { get; set; }
    public int RejectedQuotes { get; set; }
    public int ExpiredQuotes { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AcceptedValue { get; set; }
    public double AcceptanceRate { get; set; }
}
