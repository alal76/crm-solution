// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for Mailchimp/HubSpot marketing platform synchronization.
/// Implements TODO-INT-09: Mailchimp/HubSpot marketing sync.
/// </summary>
public interface IMarketingSyncService
{
    /// <summary>
    /// Syncs a CRM contact to the marketing platform as a subscriber/contact.
    /// </summary>
    /// <param name="contactId">The CRM contact ID.</param>
    /// <param name="listId">The marketing list/audience ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the sync operation.</returns>
    Task<MarketingSyncResult> SyncContactAsync(int contactId, string listId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs CRM campaign metrics from the marketing platform.
    /// </summary>
    /// <param name="campaignId">The CRM campaign ID.</param>
    /// <param name="externalCampaignId">The external campaign ID in the marketing platform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the sync operation.</returns>
    Task<MarketingSyncResult> SyncCampaignMetricsAsync(int campaignId, string externalCampaignId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports subscribers from the marketing platform into CRM as leads.
    /// </summary>
    /// <param name="listId">The marketing list/audience ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the import operation.</returns>
    Task<MarketingImportResult> ImportSubscribersAsLeadsAsync(string listId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available marketing lists/audiences from the platform.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available audiences.</returns>
    Task<IReadOnlyList<MarketingList>> GetListsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs a CRM segment to the marketing platform.
    /// </summary>
    /// <param name="segmentName">The segment name.</param>
    /// <param name="contactIds">The CRM contact IDs in the segment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the sync operation.</returns>
    Task<MarketingSyncResult> SyncSegmentAsync(string segmentName, IReadOnlyList<int> contactIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the marketing platform.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the connection is valid.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a marketing sync operation.
/// </summary>
public record MarketingSyncResult
{
    public bool Success { get; init; }
    public string? ExternalId { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime SyncedAt { get; init; } = DateTime.UtcNow;
    public string Provider { get; init; } = string.Empty;

    public static MarketingSyncResult Succeeded(string externalId, string provider) =>
        new() { Success = true, ExternalId = externalId, Provider = provider };

    public static MarketingSyncResult Failed(string error, string provider) =>
        new() { Success = false, ErrorMessage = error, Provider = provider };
}

/// <summary>
/// Result of a marketing import operation.
/// </summary>
public record MarketingImportResult
{
    public int TotalImported { get; init; }
    public int NewLeads { get; init; }
    public int UpdatedContacts { get; init; }
    public int Skipped { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Marketing list/audience from external platform.
/// </summary>
public record MarketingList
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int MemberCount { get; init; }
    public DateTime? LastSyncedAt { get; init; }
    public string Provider { get; init; } = string.Empty;
}
