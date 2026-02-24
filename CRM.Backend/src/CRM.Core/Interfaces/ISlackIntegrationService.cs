// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for Slack integration.
/// Provides methods for posting notifications and messages to Slack channels.
/// </summary>
public interface ISlackIntegrationService
{
    /// <summary>
    /// Posts a message to a Slack channel via incoming webhook.
    /// </summary>
    /// <param name="webhookUrl">The Slack incoming webhook URL.</param>
    /// <param name="message">The message to post.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> PostMessageAsync(string webhookUrl, SlackMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a CRM notification to Slack.
    /// </summary>
    /// <param name="notification">The CRM notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendCrmNotificationAsync(CrmSlackNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the Slack webhook connection.
    /// </summary>
    /// <param name="webhookUrl">The webhook URL to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test result.</returns>
    Task<SlackConnectionTestResult> TestConnectionAsync(string webhookUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Slack message structure using Block Kit.
/// </summary>
public record SlackMessage
{
    /// <summary>Simple text message (fallback).</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Username to display.</summary>
    public string? Username { get; init; }

    /// <summary>Icon emoji (e.g., :robot_face:).</summary>
    public string? IconEmoji { get; init; }

    /// <summary>Channel override (optional).</summary>
    public string? Channel { get; init; }

    /// <summary>Block Kit attachments.</summary>
    public List<SlackAttachment>? Attachments { get; init; }

    /// <summary>Block Kit blocks.</summary>
    public List<SlackBlock>? Blocks { get; init; }
}

/// <summary>
/// Slack attachment (legacy format, still supported).
/// </summary>
public record SlackAttachment
{
    /// <summary>Attachment color (hex or preset).</summary>
    public string? Color { get; init; }

    /// <summary>Pretext appears before the attachment.</summary>
    public string? Pretext { get; init; }

    /// <summary>Author name.</summary>
    public string? AuthorName { get; init; }

    /// <summary>Author link.</summary>
    public string? AuthorLink { get; init; }

    /// <summary>Attachment title.</summary>
    public string? Title { get; init; }

    /// <summary>Title link.</summary>
    public string? TitleLink { get; init; }

    /// <summary>Attachment text.</summary>
    public string? Text { get; init; }

    /// <summary>Fields (key-value pairs).</summary>
    public List<SlackField>? Fields { get; init; }

    /// <summary>Footer text.</summary>
    public string? Footer { get; init; }

    /// <summary>Timestamp for the attachment.</summary>
    public long? Ts { get; init; }
}

/// <summary>
/// Field in a Slack attachment.
/// </summary>
public record SlackField(string Title, string Value, bool Short = true);

/// <summary>
/// Slack Block Kit block.
/// </summary>
public record SlackBlock
{
    /// <summary>Block type (section, divider, header, etc.).</summary>
    public string Type { get; init; } = "section";

    /// <summary>Text element for section blocks.</summary>
    public SlackTextElement? Text { get; init; }

    /// <summary>Fields for section blocks.</summary>
    public List<SlackTextElement>? Fields { get; init; }

    /// <summary>Accessory element.</summary>
    public SlackAccessory? Accessory { get; init; }
}

/// <summary>
/// Text element in Slack Block Kit.
/// </summary>
public record SlackTextElement
{
    /// <summary>Text type (mrkdwn or plain_text).</summary>
    public string Type { get; init; } = "mrkdwn";

    /// <summary>The text content.</summary>
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Accessory element in Slack Block Kit (buttons, images, etc.).
/// </summary>
public record SlackAccessory
{
    /// <summary>Element type (button, image, etc.).</summary>
    public string Type { get; init; } = "button";

    /// <summary>Button text.</summary>
    public SlackTextElement? Text { get; init; }

    /// <summary>URL for buttons/images.</summary>
    public string? Url { get; init; }

    /// <summary>Image URL.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Alt text for images.</summary>
    public string? AltText { get; init; }

    /// <summary>Action ID for interactive elements.</summary>
    public string? ActionId { get; init; }
}

/// <summary>
/// CRM-specific notification for Slack.
/// </summary>
public record CrmSlackNotification
{
    /// <summary>The Slack webhook URL (from configuration).</summary>
    public string? WebhookUrl { get; init; }

    /// <summary>Notification type.</summary>
    public CrmNotificationType Type { get; init; }

    /// <summary>Entity type (Account, Contact, Opportunity, etc.).</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Entity ID.</summary>
    public int EntityId { get; init; }

    /// <summary>Notification title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Notification message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Link to the entity in CRM.</summary>
    public string? EntityLink { get; init; }

    /// <summary>Additional data for the notification.</summary>
    public Dictionary<string, string>? AdditionalData { get; init; }
}

/// <summary>
/// Result of Slack connection test.
/// </summary>
public record SlackConnectionTestResult
{
    /// <summary>Whether the test succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>HTTP status code received.</summary>
    public int? StatusCode { get; init; }

    /// <summary>Response received.</summary>
    public string? Response { get; init; }
}
