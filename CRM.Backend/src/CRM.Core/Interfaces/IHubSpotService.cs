// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for HubSpot bidirectional contact and deal sync integration.
/// Implements INT-002.
/// </summary>
public interface IHubSpotService
{
    /// <summary>
    /// Syncs a single CRM contact to HubSpot (create or update by email).
    /// </summary>
    /// <param name="crmContactId">CRM contact ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if disabled or an error occurred.</returns>
    Task<bool> SyncContactAsync(int crmContactId, CancellationToken ct = default);

    /// <summary>
    /// Syncs a CRM opportunity to HubSpot as a Deal.
    /// </summary>
    /// <param name="crmOpportunityId">CRM opportunity ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if disabled or an error occurred.</returns>
    Task<bool> SyncDealAsync(int crmOpportunityId, CancellationToken ct = default);

    /// <summary>
    /// Syncs all active CRM contacts to HubSpot.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of contacts successfully synced.</returns>
    Task<int> SyncAllContactsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the current HubSpot connection status by probing the API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection status including portal ID and account name.</returns>
    Task<HubSpotConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default);
}

/// <summary>
/// HubSpot connection status information returned by <see cref="IHubSpotService.GetConnectionStatusAsync"/>.
/// </summary>
public class HubSpotConnectionStatus
{
    /// <summary>Indicates whether the access token is valid and the API is reachable.</summary>
    public bool IsConnected { get; set; }

    /// <summary>The HubSpot portal (hub) ID, or null when not connected.</summary>
    public string? PortalId { get; set; }

    /// <summary>The HubSpot account name, or null when not connected.</summary>
    public string? AccountName { get; set; }
}
