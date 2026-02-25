// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Represents a notification channel for ITSM events (e.g. Slack, Teams).
/// TODO-SD005-010: Slack/Teams integration for ITSM notifications.
/// </summary>
public interface IItsmNotificationChannel
{
    /// <summary>The human-readable name of this notification channel (e.g. "Slack", "Teams").</summary>
    string ChannelName { get; }

    /// <summary>Whether this channel is currently configured and available.</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Sends a notification to the configured channel.
    /// </summary>
    /// <param name="title">Short subject/title of the notification.</param>
    /// <param name="body">Detailed body/message text.</param>
    /// <param name="urgency">Urgency level string (e.g. "Low", "Medium", "High", "Critical").</param>
    /// <param name="serviceRequestId">Optional linked service request ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendNotificationAsync(
        string title,
        string body,
        string urgency,
        int? serviceRequestId = null,
        CancellationToken ct = default);
}

/// <summary>
/// Aggregates multiple ITSM notification channels and fans out messages to all configured channels.
/// </summary>
public interface IItsmNotificationDispatcher
{
    /// <summary>
    /// Send a notification to all enabled ITSM channels.
    /// </summary>
    Task DispatchAsync(
        string title,
        string body,
        string urgency,
        int? serviceRequestId = null,
        CancellationToken ct = default);
}
