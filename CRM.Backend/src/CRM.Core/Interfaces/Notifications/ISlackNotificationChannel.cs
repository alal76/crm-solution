// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces.Notifications;

/// <summary>
/// Interface for Slack notification channel.
/// Used for sending escalation notifications via Slack.
/// TODO-SD005-010: Slack escalation notification interface.
/// </summary>
public interface ISlackNotificationChannel
{
    /// <summary>
    /// Sends a message to a Slack channel via webhook.
    /// </summary>
    /// <param name="webhookUrl">Slack incoming webhook URL</param>
    /// <param name="message">Message content (supports Slack mrkdwn)</param>
    /// <param name="username">Optional bot username override</param>
    /// <param name="iconEmoji">Optional icon emoji (e.g., ":warning:")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendMessageAsync(
        string webhookUrl,
        string message,
        string? username = null,
        string? iconEmoji = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a rich message with attachments to Slack.
    /// </summary>
    /// <param name="webhookUrl">Slack incoming webhook URL</param>
    /// <param name="text">Fallback text</param>
    /// <param name="attachments">List of Slack attachments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendRichMessageAsync(
        string webhookUrl,
        string text,
        IEnumerable<SlackAttachment> attachments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a Block Kit message to Slack.
    /// </summary>
    /// <param name="webhookUrl">Slack incoming webhook URL</param>
    /// <param name="blocksJson">Block Kit blocks JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendBlockKitMessageAsync(
        string webhookUrl,
        string blocksJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connectivity to the Slack webhook.
    /// </summary>
    /// <param name="webhookUrl">Webhook URL to test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if webhook is valid and accessible</returns>
    Task<bool> TestWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a Slack message attachment.
/// </summary>
public class SlackAttachment
{
    /// <summary>Fallback text for notifications</summary>
    public string Fallback { get; set; } = string.Empty;

    /// <summary>Color bar (hex or predefined: good, warning, danger)</summary>
    public string? Color { get; set; }

    /// <summary>Attachment title</summary>
    public string? Title { get; set; }

    /// <summary>Optional title link</summary>
    public string? TitleLink { get; set; }

    /// <summary>Main text content</summary>
    public string? Text { get; set; }

    /// <summary>Author name</summary>
    public string? AuthorName { get; set; }

    /// <summary>Footer text</summary>
    public string? Footer { get; set; }

    /// <summary>Timestamp for footer</summary>
    public long? Timestamp { get; set; }

    /// <summary>Fields displayed in a table format</summary>
    public List<SlackField>? Fields { get; set; }
}

/// <summary>
/// Represents a Slack attachment field.
/// </summary>
public class SlackField
{
    /// <summary>Field title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Field value</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Whether field is short (displayed side-by-side)</summary>
    public bool Short { get; set; } = true;
}
