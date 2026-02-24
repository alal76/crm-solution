// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces.Notifications;

/// <summary>
/// Interface for Microsoft Teams notification channel.
/// Used for sending escalation notifications via Teams.
/// TODO-SD005-010: Teams escalation notification interface.
/// </summary>
public interface ITeamsNotificationChannel
{
    /// <summary>
    /// Sends a message to a Teams channel via webhook.
    /// </summary>
    /// <param name="webhookUrl">Teams incoming webhook URL</param>
    /// <param name="title">Message card title</param>
    /// <param name="message">Message content</param>
    /// <param name="facts">Optional key-value facts to display</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendChannelMessageAsync(
        string webhookUrl,
        string title,
        string message,
        Dictionary<string, string>? facts = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a direct message to a user via Teams.
    /// Requires Graph API permissions.
    /// </summary>
    /// <param name="userEmail">User's email address</param>
    /// <param name="message">Message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendDirectMessageAsync(
        string userEmail,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an adaptive card to a Teams channel.
    /// </summary>
    /// <param name="webhookUrl">Teams incoming webhook URL</param>
    /// <param name="adaptiveCardJson">Adaptive card JSON payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendAdaptiveCardAsync(
        string webhookUrl,
        string adaptiveCardJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connectivity to the Teams webhook.
    /// </summary>
    /// <param name="webhookUrl">Webhook URL to test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if webhook is valid and accessible</returns>
    Task<bool> TestWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);
}
