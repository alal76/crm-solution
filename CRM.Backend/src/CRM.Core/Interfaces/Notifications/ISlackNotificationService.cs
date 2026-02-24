// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces.Notifications;

/// <summary>
/// Interface for Slack notification service.
/// Supports sending messages to channels, users, and formatted escalation alerts.
/// TODO-SD005-010: Slack/Teams integration for escalation notifications.
/// </summary>
public interface ISlackNotificationService
{
    /// <summary>
    /// Sends a message to a Slack channel.
    /// </summary>
    /// <param name="channelId">The Slack channel ID (e.g., C012345).</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendChannelMessageAsync(
        string channelId,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a direct message to a Slack user.
    /// </summary>
    /// <param name="userId">The Slack user ID.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendDirectMessageAsync(
        string userId,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an escalation alert with rich formatting.
    /// </summary>
    /// <param name="channelId">The Slack channel ID.</param>
    /// <param name="escalationInfo">The escalation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendEscalationAlertAsync(
        string channelId,
        SlackEscalationInfo escalationInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts an interactive message with action buttons.
    /// </summary>
    /// <param name="channelId">The Slack channel ID.</param>
    /// <param name="message">The message text.</param>
    /// <param name="actions">The action buttons to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> PostInteractiveMessageAsync(
        string channelId,
        string message,
        IEnumerable<SlackAction> actions,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Information for a Slack escalation alert.
/// </summary>
public class SlackEscalationInfo
{
    public string ServiceRequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int EscalationLevel { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime SlaBreachTime { get; set; }
    public string? TicketUrl { get; set; }
}

/// <summary>
/// Represents a Slack interactive action button.
/// </summary>
public class SlackAction
{
    public string ActionId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Style { get; set; } = "default"; // primary, danger, default
    public string? Value { get; set; }
}
