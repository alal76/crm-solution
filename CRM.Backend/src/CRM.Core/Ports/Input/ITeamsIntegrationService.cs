// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for Microsoft Teams integration.
/// Implements TODO-INT-05: Teams integration service.
/// </summary>
public interface ITeamsIntegrationService
{
    /// <summary>
    /// Sends a simple notification message to a Teams channel.
    /// </summary>
    /// <param name="channelId">The Teams channel ID or webhook URL.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<TeamsNotificationResult> SendNotificationAsync(
        string channelId,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts an Adaptive Card to a Teams channel.
    /// </summary>
    /// <param name="channelId">The Teams channel ID or webhook URL.</param>
    /// <param name="card">The Adaptive Card JSON payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<TeamsNotificationResult> PostAdaptiveCardAsync(
        string channelId,
        AdaptiveCardPayload card,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a CRM entity notification (lead, opportunity, etc.) to Teams.
    /// </summary>
    /// <param name="channelId">The Teams channel ID or webhook URL.</param>
    /// <param name="notification">The CRM notification details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<TeamsNotificationResult> SendCrmNotificationAsync(
        string channelId,
        CrmTeamsNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a Teams webhook URL.
    /// </summary>
    /// <param name="webhookUrl">The webhook URL to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the webhook is valid and reachable.</returns>
    Task<bool> ValidateWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configured Teams channels.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of configured Teams channels.</returns>
    Task<IReadOnlyList<TeamsChannelConfig>> GetConfiguredChannelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures a new Teams channel.
    /// </summary>
    /// <param name="config">The channel configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the configuration.</returns>
    Task<TeamsChannelConfigResult> ConfigureChannelAsync(
        TeamsChannelConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a configured Teams channel.
    /// </summary>
    /// <param name="channelId">The channel ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successfully removed.</returns>
    Task<bool> RemoveChannelAsync(string channelId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a Teams notification operation.
/// </summary>
public record TeamsNotificationResult
{
    /// <summary>Whether the notification was sent successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>HTTP status code from Teams API.</summary>
    public int? StatusCode { get; init; }

    /// <summary>Response body from Teams API.</summary>
    public string? ResponseBody { get; init; }

    /// <summary>When the notification was sent.</summary>
    public DateTime SentAt { get; init; }

    /// <summary>Delivery latency in milliseconds.</summary>
    public long? LatencyMs { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static TeamsNotificationResult Succeeded(int statusCode, long latencyMs) =>
        new() { Success = true, StatusCode = statusCode, SentAt = DateTime.UtcNow, LatencyMs = latencyMs };

    /// <summary>Creates a failure result.</summary>
    public static TeamsNotificationResult Failed(string error, int? statusCode = null) =>
        new() { Success = false, ErrorMessage = error, StatusCode = statusCode, SentAt = DateTime.UtcNow };
}

/// <summary>
/// Adaptive Card payload for Teams.
/// </summary>
public record AdaptiveCardPayload
{
    /// <summary>The type (always "message" for incoming webhooks).</summary>
    public string Type { get; init; } = "message";

    /// <summary>The Adaptive Card attachment.</summary>
    public IReadOnlyList<AdaptiveCardAttachment> Attachments { get; init; } = Array.Empty<AdaptiveCardAttachment>();

    /// <summary>Creates a simple card with title and text.</summary>
    public static AdaptiveCardPayload CreateSimple(string title, string text, string? actionUrl = null)
    {
        var body = new List<object>
        {
            new { type = "TextBlock", size = "Medium", weight = "Bolder", text = title },
            new { type = "TextBlock", text = text, wrap = true }
        };

        var actions = actionUrl != null
            ? new List<object> { new { type = "Action.OpenUrl", title = "View", url = actionUrl } }
            : new List<object>();

        return new AdaptiveCardPayload
        {
            Attachments = new[]
            {
                new AdaptiveCardAttachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = new { schema = "http://adaptivecards.io/schemas/adaptive-card.json", type = "AdaptiveCard", version = "1.4", body, actions }
                }
            }
        };
    }
}

/// <summary>
/// Adaptive Card attachment.
/// </summary>
public record AdaptiveCardAttachment
{
    /// <summary>Content type (application/vnd.microsoft.card.adaptive).</summary>
    public string ContentType { get; init; } = "application/vnd.microsoft.card.adaptive";

    /// <summary>URL to card content (optional).</summary>
    public string? ContentUrl { get; init; }

    /// <summary>The card content object.</summary>
    public object? Content { get; init; }
}

/// <summary>
/// CRM notification for Teams.
/// </summary>
public record CrmTeamsNotification
{
    /// <summary>Type of CRM entity (Lead, Opportunity, Account, etc.).</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>The entity ID.</summary>
    public int EntityId { get; init; }

    /// <summary>The notification title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The notification message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>The event that triggered this notification.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>URL to view the entity in CRM.</summary>
    public string? EntityUrl { get; init; }

    /// <summary>Additional facts to display.</summary>
    public IReadOnlyDictionary<string, string>? Facts { get; init; }

    /// <summary>Priority/severity level.</summary>
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;

    /// <summary>Color theme for the card.</summary>
    public string? ThemeColor { get; init; }
}

/// <summary>
/// Notification priority level.
/// </summary>
public enum NotificationPriority
{
    /// <summary>Low priority.</summary>
    Low,

    /// <summary>Normal priority.</summary>
    Normal,

    /// <summary>High priority.</summary>
    High,

    /// <summary>Urgent priority.</summary>
    Urgent
}

/// <summary>
/// Teams channel configuration.
/// </summary>
public record TeamsChannelConfig
{
    /// <summary>Unique identifier for this configuration.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Friendly name for the channel.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Description of what notifications go to this channel.</summary>
    public string? Description { get; init; }

    /// <summary>The Teams webhook URL.</summary>
    public string WebhookUrl { get; init; } = string.Empty;

    /// <summary>Whether this channel is active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Event types that trigger notifications to this channel.</summary>
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();

    /// <summary>Entity types to filter notifications for.</summary>
    public IReadOnlyList<string>? EntityTypeFilters { get; init; }

    /// <summary>When this configuration was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>When this configuration was last updated.</summary>
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Result of configuring a Teams channel.
/// </summary>
public record TeamsChannelConfigResult
{
    /// <summary>Whether the configuration was successful.</summary>
    public bool Success { get; init; }

    /// <summary>The channel ID if successful.</summary>
    public string? ChannelId { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Whether webhook validation passed.</summary>
    public bool WebhookValid { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static TeamsChannelConfigResult Succeeded(string channelId) =>
        new() { Success = true, ChannelId = channelId, WebhookValid = true };

    /// <summary>Creates a failure result.</summary>
    public static TeamsChannelConfigResult Failed(string error, bool webhookValid = false) =>
        new() { Success = false, ErrorMessage = error, WebhookValid = webhookValid };
}
