// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces.Notifications;

/// <summary>
/// Interface for Microsoft Teams notification service.
/// Supports sending messages to channels and adaptive cards for rich content.
/// TODO-SD005-010: Slack/Teams integration for escalation notifications.
/// </summary>
public interface ITeamsNotificationService
{
    /// <summary>
    /// Sends a message to a Teams channel via webhook.
    /// </summary>
    /// <param name="webhookUrl">The Teams incoming webhook URL.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendChannelMessageAsync(
        string webhookUrl,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an adaptive card to a Teams channel.
    /// </summary>
    /// <param name="webhookUrl">The Teams incoming webhook URL.</param>
    /// <param name="adaptiveCardJson">The adaptive card JSON payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendAdaptiveCardAsync(
        string webhookUrl,
        string adaptiveCardJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an escalation alert with formatted card.
    /// </summary>
    /// <param name="webhookUrl">The Teams incoming webhook URL.</param>
    /// <param name="escalationInfo">The escalation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendEscalationAlertAsync(
        string webhookUrl,
        TeamsEscalationInfo escalationInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an adaptive card JSON for an escalation alert.
    /// </summary>
    /// <param name="escalationInfo">The escalation details.</param>
    /// <returns>The adaptive card JSON string.</returns>
    string CreateEscalationCard(TeamsEscalationInfo escalationInfo);
}

/// <summary>
/// Information for a Teams escalation alert.
/// </summary>
public class TeamsEscalationInfo
{
    public string ServiceRequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int EscalationLevel { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime SlaBreachTime { get; set; }
    public string? TicketUrl { get; set; }
    public string? Description { get; set; }
    public IEnumerable<TeamsCardAction>? Actions { get; set; }
}

/// <summary>
/// Represents a Teams adaptive card action.
/// </summary>
public class TeamsCardAction
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
