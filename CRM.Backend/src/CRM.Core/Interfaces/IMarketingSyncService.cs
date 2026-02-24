// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for marketing platform synchronization (Mailchimp/HubSpot).
/// Provides methods for syncing contacts, campaigns, and lists.
/// </summary>
public interface IMarketingSyncService
{
    /// <summary>
    /// Syncs a contact to the marketing platform.
    /// </summary>
    /// <param name="contactId">The CRM contact ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with external ID.</returns>
    Task<MarketingSyncResult> SyncContactAsync(int contactId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs multiple contacts to the marketing platform.
    /// </summary>
    /// <param name="contactIds">The CRM contact IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch sync result.</returns>
    Task<MarketingBatchSyncResult> SyncContactsAsync(IEnumerable<int> contactIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a contact to a marketing list/audience.
    /// </summary>
    /// <param name="contactId">The CRM contact ID.</param>
    /// <param name="listId">The marketing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> AddToListAsync(int contactId, string listId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a contact from a marketing list/audience.
    /// </summary>
    /// <param name="contactId">The CRM contact ID.</param>
    /// <param name="listId">The marketing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> RemoveFromListAsync(int contactId, string listId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available marketing lists/audiences.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of marketing lists.</returns>
    Task<IEnumerable<MarketingList>> GetListsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets campaign analytics from the marketing platform.
    /// </summary>
    /// <param name="campaignId">The marketing campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Campaign analytics.</returns>
    Task<MarketingCampaignAnalytics?> GetCampaignAnalyticsAsync(string campaignId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the marketing platform.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connection test result.</returns>
    Task<MarketingConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a marketing sync operation.
/// </summary>
public record MarketingSyncResult
{
    /// <summary>Whether the sync succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>External ID in the marketing platform.</summary>
    public string? ExternalId { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Timestamp of the sync.</summary>
    public DateTime SyncedAt { get; init; } = DateTime.UtcNow;

    /// <summary>The marketing provider used.</summary>
    public MarketingProvider Provider { get; init; }
}

/// <summary>
/// Result of a batch marketing sync operation.
/// </summary>
public record MarketingBatchSyncResult
{
    /// <summary>Whether the overall operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Number of contacts synced.</summary>
    public int SyncedCount { get; init; }

    /// <summary>Number of contacts that failed.</summary>
    public int FailedCount { get; init; }

    /// <summary>Error messages keyed by contact ID.</summary>
    public Dictionary<int, string> Errors { get; init; } = new();

    /// <summary>The marketing provider used.</summary>
    public MarketingProvider Provider { get; init; }
}

/// <summary>
/// Marketing list/audience.
/// </summary>
public record MarketingList
{
    /// <summary>External list ID.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>List name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Number of members.</summary>
    public int MemberCount { get; init; }

    /// <summary>Whether the list is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Date created.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Marketing campaign analytics.
/// </summary>
public record MarketingCampaignAnalytics
{
    /// <summary>Campaign ID.</summary>
    public string CampaignId { get; init; } = string.Empty;

    /// <summary>Campaign name.</summary>
    public string CampaignName { get; init; } = string.Empty;

    /// <summary>Number of emails sent.</summary>
    public int Sent { get; init; }

    /// <summary>Number of opens.</summary>
    public int Opens { get; init; }

    /// <summary>Unique opens.</summary>
    public int UniqueOpens { get; init; }

    /// <summary>Number of clicks.</summary>
    public int Clicks { get; init; }

    /// <summary>Unique clicks.</summary>
    public int UniqueClicks { get; init; }

    /// <summary>Number of bounces.</summary>
    public int Bounces { get; init; }

    /// <summary>Number of unsubscribes.</summary>
    public int Unsubscribes { get; init; }

    /// <summary>Open rate percentage.</summary>
    public double OpenRate { get; init; }

    /// <summary>Click rate percentage.</summary>
    public double ClickRate { get; init; }

    /// <summary>Last updated.</summary>
    public DateTime LastUpdated { get; init; }
}

/// <summary>
/// Marketing platform provider.
/// </summary>
public enum MarketingProvider
{
    /// <summary>None configured.</summary>
    None = 0,

    /// <summary>Mailchimp.</summary>
    Mailchimp = 1,

    /// <summary>HubSpot.</summary>
    HubSpot = 2,

    /// <summary>ActiveCampaign.</summary>
    ActiveCampaign = 3,

    /// <summary>Constant Contact.</summary>
    ConstantContact = 4,

    /// <summary>Sendinblue/Brevo.</summary>
    Sendinblue = 5
}

/// <summary>
/// Result of marketing connection test.
/// </summary>
public record MarketingConnectionResult
{
    /// <summary>Whether connection succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Provider connected to.</summary>
    public MarketingProvider Provider { get; init; }

    /// <summary>Account name in marketing platform.</summary>
    public string? AccountName { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }
}
