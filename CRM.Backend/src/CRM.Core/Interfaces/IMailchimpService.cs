// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for Mailchimp contact-list sync integration.
/// Implements INT-002.
/// </summary>
public interface IMailchimpService
{
    /// <summary>
    /// Syncs a single CRM contact to the configured Mailchimp audience list.
    /// </summary>
    /// <param name="crmContactId">CRM contact ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if disabled or an error occurred.</returns>
    Task<bool> SyncContactAsync(int crmContactId, CancellationToken ct = default);

    /// <summary>
    /// Syncs all active CRM contacts to the configured Mailchimp audience list.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of contacts successfully synced.</returns>
    Task<int> SyncAllContactsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets a subscriber's status to "unsubscribed" in Mailchimp.
    /// </summary>
    /// <param name="email">Email address to unsubscribe.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if unsubscribed successfully, false otherwise.</returns>
    Task<bool> UnsubscribeContactAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Returns the current Mailchimp connection status by querying the configured list.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection status including list name and subscriber count.</returns>
    Task<MailchimpConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default);
}

/// <summary>
/// Mailchimp connection status information returned by <see cref="IMailchimpService.GetConnectionStatusAsync"/>.
/// </summary>
public class MailchimpConnectionStatus
{
    /// <summary>Indicates whether the API key and list ID are valid and reachable.</summary>
    public bool IsConnected { get; set; }

    /// <summary>The Mailchimp audience list name, or null when not connected.</summary>
    public string? ListName { get; set; }

    /// <summary>Total member count for the configured list, or null when not connected.</summary>
    public int? MemberCount { get; set; }

    /// <summary>The Mailchimp account name, or null when not connected.</summary>
    public string? AccountName { get; set; }
}
