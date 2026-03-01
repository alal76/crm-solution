// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing event attendees
/// </summary>
public class EventAttendeeService : IEventAttendeeService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EventAttendeeService> _logger;

    public EventAttendeeService(ICrmDbContext context, ILogger<EventAttendeeService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventAttendee>> GetAllAsync(
        int? activityId = null,
        AttendeeType? attendeeType = null,
        AttendeeResponseStatus? responseStatus = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting attendees with filters: ActivityId={ActivityId}, AttendeeType={AttendeeType}, ResponseStatus={ResponseStatus}",
            activityId, attendeeType, responseStatus);

        var query = _context.EventAttendees.AsNoTracking().Where(a => !a.IsDeleted);

        if (activityId.HasValue)
        {
            query = query.Where(a => a.ActivityId == activityId.Value);
        }

        if (attendeeType.HasValue)
        {
            query = query.Where(a => a.AttendeeType == attendeeType.Value);
        }

        if (responseStatus.HasValue)
        {
            query = query.Where(a => a.ResponseStatus == responseStatus.Value);
        }

        var attendees = await query
            .OrderBy(a => a.AttendeeName)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} attendees", attendees.Count);
        return attendees;
    }

    /// <inheritdoc />
    public async Task<EventAttendee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting attendee by ID: {AttendeeId}", id);

        var attendee = await _context.EventAttendees
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (attendee == null)
        {
            _logger.LogWarning("Attendee not found: {AttendeeId}", id);
        }

        return attendee;
    }

    /// <inheritdoc />
    public async Task<EventAttendee> CreateAsync(EventAttendee attendee, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendee);

        _logger.LogDebug("Creating attendee for activity {ActivityId}: {AttendeeName}", attendee.ActivityId, attendee.AttendeeName);

        attendee.CreatedAt = DateTime.UtcNow;
        attendee.IsDeleted = false;

        _context.EventAttendees.Add(attendee);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created attendee with ID: {AttendeeId} for activity {ActivityId}", attendee.Id, attendee.ActivityId);
        return attendee;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, EventAttendee attendee, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendee);

        _logger.LogDebug("Updating attendee: {AttendeeId}", id);

        var existing = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Attendee not found for update: {AttendeeId}", id);
            return false;
        }

        existing.AttendeeType = attendee.AttendeeType;
        existing.AttendeeId = attendee.AttendeeId;
        existing.AttendeeEmail = attendee.AttendeeEmail;
        existing.AttendeeName = attendee.AttendeeName;
        existing.ResponseStatus = attendee.ResponseStatus;
        existing.RespondedAt = attendee.RespondedAt;
        existing.ResponseComment = attendee.ResponseComment;
        existing.IsOrganizer = attendee.IsOrganizer;
        existing.IsRequired = attendee.IsRequired;
        existing.Role = attendee.Role;
        existing.DidAttend = attendee.DidAttend;
        existing.AttendanceDurationMinutes = attendee.AttendanceDurationMinutes;
        existing.AttendanceNotes = attendee.AttendanceNotes;
        existing.ExternalCalendarEventId = attendee.ExternalCalendarEventId;
        existing.InvitationSentAt = attendee.InvitationSentAt;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated attendee: {AttendeeId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting attendee: {AttendeeId}", id);

        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (attendee == null)
        {
            _logger.LogWarning("Attendee not found for deletion: {AttendeeId}", id);
            return false;
        }

        // Soft delete
        attendee.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted attendee: {AttendeeId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventAttendee>> GetByActivityAsync(int activityId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting attendees for activity: {ActivityId}", activityId);

        var attendees = await _context.EventAttendees
            .AsNoTracking()
            .Where(a => a.ActivityId == activityId && !a.IsDeleted)
            .OrderByDescending(a => a.IsOrganizer)
            .ThenByDescending(a => a.IsRequired)
            .ThenBy(a => a.AttendeeName)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} attendees for activity {ActivityId}", attendees.Count, activityId);
        return attendees;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateResponseAsync(int id, AttendeeResponseStatus status, string? comment = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating response for attendee: {AttendeeId} to {Status}", id, status);

        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (attendee == null)
        {
            _logger.LogWarning("Attendee not found for response update: {AttendeeId}", id);
            return false;
        }

        attendee.ResponseStatus = status;
        attendee.RespondedAt = DateTime.UtcNow;
        attendee.ResponseComment = comment;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated response for attendee: {AttendeeId} to {Status}", id, status);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RecordAttendanceAsync(int id, bool didAttend, int? durationMinutes = null, string? notes = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Recording attendance for attendee: {AttendeeId}, DidAttend: {DidAttend}", id, didAttend);

        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (attendee == null)
        {
            _logger.LogWarning("Attendee not found for attendance recording: {AttendeeId}", id);
            return false;
        }

        attendee.DidAttend = didAttend;
        attendee.AttendanceDurationMinutes = durationMinutes;
        attendee.AttendanceNotes = notes;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded attendance for attendee: {AttendeeId}, DidAttend: {DidAttend}", id, didAttend);
        return true;
    }
}
