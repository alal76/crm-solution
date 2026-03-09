// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Contract for a WhatsApp Business messaging provider.
/// Default implementation uses the Twilio WhatsApp Sandbox via the Twilio REST API.
/// Supports graceful degradation: when not configured, all operations return false
/// without throwing exceptions.
/// </summary>
public interface IWhatsAppProvider
{
    /// <summary>
    /// Sends a plain-text WhatsApp message to the specified number.
    /// </summary>
    /// <param name="toNumber">Destination number in E.164 format (e.g. +15551234567).
    /// The <c>whatsapp:</c> prefix is added automatically if absent.</param>
    /// <param name="message">Message text body.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the message was accepted by the API; <c>false</c> otherwise.</returns>
    Task<bool> SendMessageAsync(string toNumber, string message, CancellationToken ct = default);

    /// <summary>
    /// Sends a pre-approved WhatsApp template message.
    /// For Twilio Sandbox the template body is rendered as plain text from
    /// <paramref name="templateName"/> and <paramref name="parameters"/>.
    /// </summary>
    /// <param name="toNumber">Destination number.</param>
    /// <param name="templateName">Template identifier / name.</param>
    /// <param name="parameters">Named variables to inject into the template.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if delivery was accepted.</returns>
    Task<bool> SendTemplateAsync(
        string toNumber,
        string templateName,
        Dictionary<string, string> parameters,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a value indicating whether the provider is fully configured and ready.
    /// Returns <c>false</c> when <c>AccountSid</c> or <c>AuthToken</c> are absent,
    /// or when <c>Enabled = false</c> in configuration.
    /// </summary>
    bool IsAvailable { get; }
}
