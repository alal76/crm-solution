// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Contract for a Twitter/X Direct Messaging provider.
/// <para>
/// COMM-003: Twitter outbound DMs require the $100/month Basic API tier or higher.
/// This interface exposes outbound send as a stub only. The <see cref="IsMockMode"/>
/// property will always return <c>true</c> in the current implementation.
/// Inbound events are handled via the <c>TwitterWebhookController</c> using
/// Mockoon-simulated webhooks in development.
/// </para>
/// </summary>
public interface ITwitterMessagingProvider
{
    /// <summary>
    /// Sends a Direct Message to the specified Twitter/X user.
    /// </summary>
    /// <param name="recipientUserId">The numeric Twitter User ID of the recipient.</param>
    /// <param name="message">The plain-text message to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the message was delivered; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// COMM-003: Twitter outbound DMs require $100/month Basic API tier.
    /// This method is a stub only. MockMode = true always.
    /// </remarks>
    Task<bool> SendDirectMessageAsync(string recipientUserId, string message, CancellationToken ct = default);

    /// <summary>
    /// Gets a value indicating whether the provider is fully configured and ready.
    /// Always <c>false</c> — outbound DMs are unavailable without a paid API tier.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets a value indicating that this provider operates in mock mode only.
    /// Always <c>true</c> — outbound DMs require a paid Twitter/X API tier.
    /// </summary>
    bool IsMockMode { get; }
}
