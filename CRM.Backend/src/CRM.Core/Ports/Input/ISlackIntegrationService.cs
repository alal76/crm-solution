// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for Slack integration.
/// Implements TODO-INT-06: Slack notification service.
/// </summary>
public interface ISlackIntegrationService
{
    /// <summary>
    /// Sends a simple text message to a Slack channel via incoming webhook.
    /// </summary>
    /// <param name="webhookUrl">The Slack incoming webhook URL.</param>
    /// <param name="message">The message text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the notification.</returns>
    Task<SlackNotificationResult> SendMessageAsync(
        string webhookUrl,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a Slack Block Kit message to a channel.
    /// </summary>
    /// <param name="webhookUrl">The Slack incoming webhook URL.</param>
    /// <param name="blocks">JSON array of Block Kit blocks.</param>
    /// <param name="fallbackText">Fallback text for notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the notification.</returns>
    Task<SlackNotificationResult> SendBlocksAsync(
        string webhookUrl,
        string blocks,
        string? fallbackText = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a CRM entity notification to Slack with formatted blocks.
    /// </summary>
    /// <param name="webhookUrl">The Slack incoming webhook URL.</param>
    /// <param name="notification">The CRM notification details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the notification.</returns>
    Task<SlackNotificationResult> SendCrmNotificationAsync(
        string webhookUrl,
        CrmSlackNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a Slack webhook URL.
    /// </summary>
    /// <param name="webhookUrl">The webhook URL to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the URL is valid.</returns>
    Task<bool> ValidateWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a Slack notification operation.
/// </summary>
public record SlackNotificationResult
{
    /// <summary>Whether the notification was sent successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>HTTP status code from Slack API.</summary>
    public int? StatusCode { get; init; }

    /// <summary>When the notification was sent.</summary>
    public DateTime SentAt { get; init; } = DateTime.UtcNow;

    /// <summary>Delivery latency in milliseconds.</summary>
    public long? LatencyMs { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static SlackNotificationResult Succeeded(int statusCode, long latencyMs) =>
        new() { Success = true, StatusCode = statusCode, SentAt = DateTime.UtcNow, LatencyMs = latencyMs };

    /// <summary>Creates a failure result.</summary>
    public static SlackNotificationResult Failed(string error, int? statusCode = null) =>
        new() { Success = false, ErrorMessage = error, StatusCode = statusCode, SentAt = DateTime.UtcNow };
}

/// <summary>
/// CRM notification payload for Slack.
/// </summary>
public record CrmSlackNotification
{
    /// <summary>Type of CRM entity.</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>The entity ID.</summary>
    public int EntityId { get; init; }

    /// <summary>Notification title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Notification message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Event that triggered the notification.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>URL to view the entity in CRM.</summary>
    public string? EntityUrl { get; init; }

    /// <summary>Additional field key-value pairs to display.</summary>
    public IReadOnlyDictionary<string, string>? Fields { get; init; }

    /// <summary>Color for the Slack attachment (hex).</summary>
    public string? Color { get; init; }
}
