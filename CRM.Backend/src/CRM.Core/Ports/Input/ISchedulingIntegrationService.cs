// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for Calendly/Cal.com scheduling integration.
/// Implements TODO-INT-11: Calendly/Cal.com scheduling integration.
/// </summary>
public interface ISchedulingIntegrationService
{
    /// <summary>
    /// Gets available scheduling links for CRM users.
    /// </summary>
    /// <param name="userId">Optional CRM user ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of scheduling links.</returns>
    Task<IReadOnlyList<SchedulingLink>> GetSchedulingLinksAsync(int? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a scheduling link for a CRM user.
    /// </summary>
    /// <param name="request">The scheduling link creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created scheduling link.</returns>
    Task<SchedulingLink> CreateSchedulingLinkAsync(CreateSchedulingLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming scheduled meetings.
    /// </summary>
    /// <param name="userId">Optional CRM user ID to filter by.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of scheduled meetings.</returns>
    Task<IReadOnlyList<ScheduledMeeting>> GetUpcomingMeetingsAsync(
        int? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a scheduling webhook event (booking created, cancelled, rescheduled).
    /// </summary>
    /// <param name="webhookEvent">The webhook event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of processing the event.</returns>
    Task<SchedulingEventResult> ProcessWebhookEventAsync(SchedulingWebhookEvent webhookEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a scheduled meeting to a CRM entity.
    /// </summary>
    /// <param name="meetingId">The external meeting ID.</param>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="entityId">The CRM entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LinkMeetingToEntityAsync(string meetingId, string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the scheduling platform.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if connected.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// A scheduling link (booking page).
/// </summary>
public record SchedulingLink
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public int DurationMinutes { get; init; }
    public int? CrmUserId { get; init; }
    public bool IsActive { get; init; } = true;
    public string Provider { get; init; } = string.Empty;
}

/// <summary>
/// Request to create a scheduling link.
/// </summary>
public record CreateSchedulingLinkRequest
{
    public string Name { get; init; } = string.Empty;
    public int DurationMinutes { get; init; } = 30;
    public int CrmUserId { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
}

/// <summary>
/// A scheduled meeting.
/// </summary>
public record ScheduledMeeting
{
    public string ExternalId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? Location { get; init; }
    public string? MeetingUrl { get; init; }
    public string Status { get; init; } = "Scheduled";
    public string? InviteeName { get; init; }
    public string? InviteeEmail { get; init; }
    public int? CrmUserId { get; init; }
    public string? LinkedEntityType { get; init; }
    public int? LinkedEntityId { get; init; }
    public string Provider { get; init; } = string.Empty;
}

/// <summary>
/// Webhook event from the scheduling platform.
/// </summary>
public record SchedulingWebhookEvent
{
    public string EventType { get; init; } = string.Empty;
    public string MeetingId { get; init; } = string.Empty;
    public string? InviteeName { get; init; }
    public string? InviteeEmail { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public string? CancellationReason { get; init; }
    public string RawPayload { get; init; } = "{}";
}

/// <summary>
/// Result of processing a scheduling webhook event.
/// </summary>
public record SchedulingEventResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int? CreatedActivityId { get; init; }
    public int? LinkedContactId { get; init; }

    public static SchedulingEventResult Succeeded(int? activityId = null, int? contactId = null) =>
        new() { Success = true, CreatedActivityId = activityId, LinkedContactId = contactId };

    public static SchedulingEventResult Failed(string error) =>
        new() { Success = false, ErrorMessage = error };
}
