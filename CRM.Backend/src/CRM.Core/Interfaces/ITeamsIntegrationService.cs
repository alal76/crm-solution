// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for Microsoft Teams integration.
/// Provides methods for posting notifications and messages to Teams channels.
/// </summary>
public interface ITeamsIntegrationService
{
    /// <summary>
    /// Posts a notification to a Teams channel via incoming webhook.
    /// </summary>
    /// <param name="webhookUrl">The Teams incoming webhook URL.</param>
    /// <param name="message">The message to post.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> PostMessageAsync(string webhookUrl, TeamsMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a CRM notification to Teams.
    /// </summary>
    /// <param name="notification">The CRM notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SendCrmNotificationAsync(CrmTeamsNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the Teams webhook connection.
    /// </summary>
    /// <param name="webhookUrl">The webhook URL to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test result.</returns>
    Task<TeamsConnectionTestResult> TestConnectionAsync(string webhookUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Teams message structure for adaptive cards.
/// </summary>
public record TeamsMessage
{
    /// <summary>Message title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Message text/body.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Theme color (hex without #).</summary>
    public string ThemeColor { get; init; } = "0076D7";

    /// <summary>Summary for notifications.</summary>
    public string? Summary { get; init; }

    /// <summary>Additional sections.</summary>
    public List<TeamsMessageSection>? Sections { get; init; }

    /// <summary>Action buttons.</summary>
    public List<TeamsMessageAction>? Actions { get; init; }
}

/// <summary>
/// Section in a Teams message.
/// </summary>
public record TeamsMessageSection
{
    /// <summary>Section title.</summary>
    public string? ActivityTitle { get; init; }

    /// <summary>Section subtitle.</summary>
    public string? ActivitySubtitle { get; init; }

    /// <summary>Activity image URL.</summary>
    public string? ActivityImage { get; init; }

    /// <summary>Facts (key-value pairs).</summary>
    public List<TeamsMessageFact>? Facts { get; init; }

    /// <summary>Section text.</summary>
    public string? Text { get; init; }
}

/// <summary>
/// Fact (key-value pair) in a Teams message.
/// </summary>
public record TeamsMessageFact(string Name, string Value);

/// <summary>
/// Action button in a Teams message.
/// </summary>
public record TeamsMessageAction
{
    /// <summary>Action type (OpenUri, HttpPOST).</summary>
    public string Type { get; init; } = "OpenUri";

    /// <summary>Button text.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Target URLs by platform.</summary>
    public List<TeamsMessageActionTarget>? Targets { get; init; }
}

/// <summary>
/// Target for an action button.
/// </summary>
public record TeamsMessageActionTarget(string Os, string Uri);

/// <summary>
/// CRM-specific notification for Teams.
/// </summary>
public record CrmTeamsNotification
{
    /// <summary>The Teams webhook URL (from configuration).</summary>
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
/// Type of CRM notification.
/// </summary>
public enum CrmNotificationType
{
    /// <summary>Entity was created.</summary>
    Created = 0,

    /// <summary>Entity was updated.</summary>
    Updated = 1,

    /// <summary>Entity was deleted.</summary>
    Deleted = 2,

    /// <summary>Task assigned.</summary>
    TaskAssigned = 3,

    /// <summary>Task completed.</summary>
    TaskCompleted = 4,

    /// <summary>Deal won.</summary>
    DealWon = 5,

    /// <summary>Deal lost.</summary>
    DealLost = 6,

    /// <summary>Support ticket created.</summary>
    TicketCreated = 7,

    /// <summary>Support ticket resolved.</summary>
    TicketResolved = 8,

    /// <summary>General alert.</summary>
    Alert = 9
}

/// <summary>
/// Result of Teams connection test.
/// </summary>
public record TeamsConnectionTestResult
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
