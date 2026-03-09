// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Contract for a LinkedIn Messaging provider.
/// <para>
/// COMM-004: LinkedIn outbound messaging via the Messages API requires
/// LinkedIn Sales Navigator ($1,600+/year). This interface exposes outbound
/// send as a stub only. The <see cref="IsMockMode"/> property will always
/// return <c>true</c> in the current implementation. Inbound events are
/// handled via the <c>LinkedInWebhookController</c> using Mockoon-simulated
/// webhooks in development.
/// </para>
/// </summary>
public interface ILinkedInMessagingProvider
{
    /// <summary>
    /// Sends a message to the specified LinkedIn member URN.
    /// </summary>
    /// <param name="recipientUrn">
    /// The LinkedIn member URN of the recipient (e.g. <c>urn:li:person:AbCdEf</c>).
    /// </param>
    /// <param name="message">The plain-text message body to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the message was delivered; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// COMM-004: LinkedIn outbound messaging requires Sales Navigator ($1,600+/year).
    /// This method is a stub only. MockMode = true always.
    /// </remarks>
    Task<bool> SendMessageAsync(string recipientUrn, string message, CancellationToken ct = default);

    /// <summary>
    /// Gets a value indicating whether the provider is fully configured and ready.
    /// Always <c>false</c> — outbound messaging is unavailable without Sales Navigator.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets a value indicating that this provider operates in mock mode only.
    /// Always <c>true</c> — outbound messaging requires LinkedIn Sales Navigator.
    /// </summary>
    bool IsMockMode { get; }
}
