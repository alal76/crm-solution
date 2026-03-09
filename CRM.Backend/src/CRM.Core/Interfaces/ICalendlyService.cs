// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// DTO representing a Calendly scheduled event.
/// </summary>
public class CalendlyEventDto
{
    /// <summary>Name of the event type (e.g., "30 Minute Meeting").</summary>
    public string? EventName { get; set; }

    /// <summary>Full name of the invitee.</summary>
    public string? InviteeName { get; set; }

    /// <summary>Email address of the invitee.</summary>
    public string? InviteeEmail { get; set; }

    /// <summary>Scheduled start time (UTC).</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Scheduled end time (UTC).</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Virtual meeting join URL (Zoom, Teams, etc.), if available.</summary>
    public string? JoinUrl { get; set; }
}

/// <summary>
/// Service interface for the Calendly scheduling integration.
/// Provides webhook registration and upcoming-event retrieval.
/// Implements INT-004.
/// </summary>
public interface ICalendlyService
{
    /// <summary>
    /// Registers the CRM webhook URL with Calendly for <c>invitee.created</c>
    /// and <c>invitee.canceled</c> events.
    /// </summary>
    /// <param name="webhookUrl">Publicly reachable URL for the Calendly webhook endpoint.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if registration succeeded; false if disabled or an error occurred.</returns>
    Task<bool> RegisterWebhookAsync(string webhookUrl, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the next 10 active (not cancelled) Calendly scheduled events
    /// for the authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Sequence of upcoming events, or empty if disabled/not configured.</returns>
    Task<IEnumerable<CalendlyEventDto>> GetUpcomingEventsAsync(CancellationToken ct = default);
}
