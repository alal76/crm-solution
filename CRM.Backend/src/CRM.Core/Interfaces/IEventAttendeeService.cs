// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing event attendees
/// </summary>
public interface IEventAttendeeService
{
    /// <summary>
    /// Get all attendees with optional filtering
    /// </summary>
    Task<IEnumerable<EventAttendee>> GetAllAsync(
        int? activityId = null,
        AttendeeType? attendeeType = null,
        AttendeeResponseStatus? responseStatus = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an attendee by ID
    /// </summary>
    Task<EventAttendee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new event attendee
    /// </summary>
    Task<EventAttendee> CreateAsync(EventAttendee attendee, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing event attendee
    /// </summary>
    Task<bool> UpdateAsync(int id, EventAttendee attendee, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an event attendee (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all attendees for a specific activity/event
    /// </summary>
    Task<IEnumerable<EventAttendee>> GetByActivityAsync(int activityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the response status for an attendee
    /// </summary>
    Task<bool> UpdateResponseAsync(int id, AttendeeResponseStatus status, string? comment = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record attendance after an event
    /// </summary>
    Task<bool> RecordAttendanceAsync(int id, bool didAttend, int? durationMinutes = null, string? notes = null, CancellationToken cancellationToken = default);
}
