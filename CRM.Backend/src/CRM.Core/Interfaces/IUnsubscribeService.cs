// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing email unsubscribe records and preference centre operations.
/// Implements CAN-SPAM / GDPR compliance requirements.
/// </summary>
public interface IUnsubscribeService
{
    /// <summary>Gets the current unsubscribe/preference status for an email address.</summary>
    /// <param name="email">Email address to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current status or a default "not unsubscribed" record if none exists.</returns>
    Task<UnsubscribeStatusDto> GetStatusAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Records an unsubscribe and marks all active nurture enrollments as opted-out.</summary>
    /// <param name="dto">Unsubscribe request containing email, reason, and preferences.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated status DTO.</returns>
    Task<UnsubscribeStatusDto> UnsubscribeAsync(UnsubscribeRequestDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates email preferences without fully unsubscribing.</summary>
    /// <param name="email">Email address to update.</param>
    /// <param name="dto">Updated preference values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated status DTO.</returns>
    Task<UnsubscribeStatusDto> UpdatePreferencesAsync(string email, UnsubscribeRequestDto dto, CancellationToken cancellationToken = default);

    /// <summary>Generates a time-limited signed token for a public unsubscribe link.</summary>
    /// <param name="email">Email address to embed in the token.</param>
    /// <param name="campaignId">Optional campaign ID to track the unsubscribe source.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Base64URL-encoded signed token (valid for 7 days).</returns>
    Task<string> GenerateUnsubscribeTokenAsync(string email, int? campaignId, CancellationToken cancellationToken = default);

    /// <summary>Returns true if the email has an active unsubscribe record.</summary>
    /// <param name="email">Email address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if unsubscribed.</returns>
    Task<bool> IsUnsubscribedAsync(string email, CancellationToken cancellationToken = default);
}
