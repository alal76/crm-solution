// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Contract for a Facebook Messenger messaging provider.
/// Default implementation uses the Facebook Graph API.
/// Supports graceful degradation: when not configured, all operations return false
/// without throwing exceptions.
/// </summary>
public interface IFacebookMessengerProvider
{
    /// <summary>
    /// Sends a plain-text message to the specified Page-Scoped User ID (PSID).
    /// </summary>
    /// <param name="recipientPsid">The recipient's Page-Scoped User ID assigned by Facebook.</param>
    /// <param name="message">Message text to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the message was accepted by the Graph API; <c>false</c> otherwise.</returns>
    Task<bool> SendMessageAsync(string recipientPsid, string message, CancellationToken ct = default);

    /// <summary>
    /// Indicates whether the provider is fully configured and ready to send messages.
    /// Returns <c>false</c> when <c>Enabled</c> is <c>false</c> or
    /// <c>PageAccessToken</c> is empty.
    /// </summary>
    bool IsAvailable { get; }
}
