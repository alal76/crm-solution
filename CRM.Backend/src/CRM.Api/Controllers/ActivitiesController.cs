// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing activities (timeline/activity feed)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ActivitiesController> _logger;
    private readonly NormalizationService _normalization;

    public ActivitiesController(CrmDbContext context, ILogger<ActivitiesController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    /// <summary>
    /// Get all activities with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Activity>>> GetActivities(
        [FromQuery] int? customerId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] ActivityType? activityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 50)
    {
        var query = _context.Activities
            .Include(a => a.User)
            .Include(a => a.Account)
            .Include(a => a.Opportunity)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(a => a.AccountId == customerId);

        if (opportunityId.HasValue)
            query = query.Where(a => a.OpportunityId == opportunityId);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (activityType.HasValue)
            query = query.Where(a => a.ActivityType == activityType);

        if (fromDate.HasValue)
            query = query.Where(a => a.ActivityDate >= fromDate);

        if (toDate.HasValue)
            query = query.Where(a => a.ActivityDate <= toDate);

        var activities = await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
        }

        return Ok(activities);
    }

    /// <summary>
    /// Get an activity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(int id)
    {
        var activity = await _context.Activities
            .Include(a => a.User)
            .Include(a => a.Account)
            .Include(a => a.Opportunity)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Activity", activity.Id);
        if (!string.IsNullOrWhiteSpace(nt)) activity.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Activity", activity.Id);
        if (!string.IsNullOrWhiteSpace(cf)) activity.CustomFields = cf;

        return Ok(activity);
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
    {
        activity.CreatedAt = DateTime.UtcNow;
        activity.UpdatedAt = DateTime.UtcNow;

        if (activity.ActivityDate == default)
            activity.ActivityDate = DateTime.UtcNow;

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
    }

    /// <summary>
    /// Delete an activity
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
            return NotFound();

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get activities for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<Activity>>> GetActivitiesByEntity(
        string entityType,
        int entityId,
        [FromQuery] int limit = 50)
    {
        var query = _context.Activities
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId);

        var activities = await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
        }

        return Ok(activities);
    }

    /// <summary>
    /// Get customer timeline (all activities related to a customer)
    /// </summary>
    [HttpGet("customer/{customerId}/timeline")]
    public async Task<ActionResult<IEnumerable<Activity>>> GetCustomerTimeline(int customerId, [FromQuery] int limit = 100)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Where(a => a.AccountId == customerId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        return Ok(activities);
    }

    /// <summary>
    /// Get opportunity timeline
    /// </summary>
    [HttpGet("opportunity/{opportunityId}/timeline")]
    public async Task<ActionResult<IEnumerable<Activity>>> GetOpportunityTimeline(int opportunityId, [FromQuery] int limit = 100)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Where(a => a.OpportunityId == opportunityId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
        }

        return Ok(activities);
    }

    /// <summary>
    /// Get recent activities for dashboard
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<Activity>>> GetRecentActivities([FromQuery] int limit = 20)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Include(a => a.Account)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
        }

        return Ok(activities);
    }

    /// <summary>
    /// Get activity statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetActivityStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var query = _context.Activities.Where(a => a.ActivityDate >= from && a.ActivityDate <= to);

        var stats = new
        {
            TotalActivities = await query.CountAsync(),
            EmailsSent = await query.CountAsync(a => a.ActivityType == ActivityType.EmailSent),
            CallsMade = await query.CountAsync(a => a.ActivityType == ActivityType.CallMade),
            MeetingsCompleted = await query.CountAsync(a => a.ActivityType == ActivityType.MeetingCompleted),
            OpportunitiesCreated = await query.CountAsync(a => a.ActivityType == ActivityType.OpportunityCreated),
            OpportunitiesWon = await query.CountAsync(a => a.ActivityType == ActivityType.OpportunityWon),
            OpportunitiesLost = await query.CountAsync(a => a.ActivityType == ActivityType.OpportunityLost),
            QuotesSent = await query.CountAsync(a => a.ActivityType == ActivityType.QuoteSent),
            QuotesAccepted = await query.CountAsync(a => a.ActivityType == ActivityType.QuoteAccepted),
            TasksCompleted = await query.CountAsync(a => a.ActivityType == ActivityType.TaskCompleted),
            ActivitiesByType = await query
                .GroupBy(a => a.ActivityType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            ActivitiesByDay = await query
                .GroupBy(a => a.ActivityDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync()
        };

        return Ok(stats);
    }

    #region Event Attendees

    /// <summary>
    /// Get attendees for an activity/event
    /// </summary>
    [HttpGet("{activityId}/attendees")]
    public async Task<ActionResult<IEnumerable<EventAttendee>>> GetActivityAttendees(int activityId)
    {
        var activity = await _context.Activities.FindAsync(activityId);
        if (activity == null)
            return NotFound("Activity not found");

        var attendees = await _context.EventAttendees
            .Where(a => a.ActivityId == activityId)
            .OrderByDescending(a => a.IsOrganizer)
            .ThenBy(a => a.AttendeeType)
            .ToListAsync();

        return Ok(attendees);
    }

    /// <summary>
    /// Add an attendee to an activity/event
    /// </summary>
    [HttpPost("{activityId}/attendees")]
    public async Task<ActionResult<EventAttendee>> AddAttendee(int activityId, [FromBody] EventAttendeeCreateDto dto)
    {
        var activity = await _context.Activities.FindAsync(activityId);
        if (activity == null)
            return NotFound("Activity not found");

        var attendee = new EventAttendee
        {
            ActivityId = activityId,
            AttendeeType = dto.AttendeeType,
            AttendeeId = dto.AttendeeId,
            ResponseStatus = AttendeeResponseStatus.NotResponded,
            IsOrganizer = dto.IsOrganizer,
            IsRequired = dto.IsRequired,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.EventAttendees.Add(attendee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAttendee), new { activityId, attendeeId = attendee.Id }, attendee);
    }

    /// <summary>
    /// Get a specific attendee
    /// </summary>
    [HttpGet("{activityId}/attendees/{attendeeId}")]
    public async Task<ActionResult<EventAttendee>> GetAttendee(int activityId, int attendeeId)
    {
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.ActivityId == activityId);

        if (attendee == null)
            return NotFound();

        return Ok(attendee);
    }

    /// <summary>
    /// Update attendee response status
    /// </summary>
    [HttpPatch("{activityId}/attendees/{attendeeId}/respond")]
    public async Task<ActionResult<EventAttendee>> UpdateAttendeeResponse(
        int activityId,
        int attendeeId,
        [FromBody] AttendeeResponseDto dto)
    {
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.ActivityId == activityId);

        if (attendee == null)
            return NotFound();

        attendee.ResponseStatus = dto.ResponseStatus;
        attendee.RespondedAt = DateTime.UtcNow;
        attendee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(attendee);
    }

    /// <summary>
    /// Mark attendee as attended/not attended (for completed events)
    /// </summary>
    [HttpPatch("{activityId}/attendees/{attendeeId}/attendance")]
    public async Task<ActionResult<EventAttendee>> UpdateAttendance(
        int activityId,
        int attendeeId,
        [FromBody] AttendanceDto dto)
    {
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.ActivityId == activityId);

        if (attendee == null)
            return NotFound();

        attendee.DidAttend = dto.DidAttend;
        attendee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(attendee);
    }

    /// <summary>
    /// Remove an attendee from an activity/event
    /// </summary>
    [HttpDelete("{activityId}/attendees/{attendeeId}")]
    public async Task<IActionResult> RemoveAttendee(int activityId, int attendeeId)
    {
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.ActivityId == activityId);

        if (attendee == null)
            return NotFound();

        _context.EventAttendees.Remove(attendee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get all events a user/contact/lead is attending
    /// </summary>
    [HttpGet("attendee/{attendeeType}/{attendeeId:int}/events")]
    public async Task<ActionResult<IEnumerable<Activity>>> GetEventsForAttendee(
        AttendeeType attendeeType,
        int attendeeId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = _context.EventAttendees
            .Include(ea => ea.Activity)
            .Where(ea => ea.AttendeeType == attendeeType && ea.AttendeeId == attendeeId);

        if (fromDate.HasValue)
            query = query.Where(ea => ea.Activity!.ActivityDate >= fromDate);

        if (toDate.HasValue)
            query = query.Where(ea => ea.Activity!.ActivityDate <= toDate);

        var activities = await query
            .Select(ea => ea.Activity!)
            .OrderByDescending(a => a.ActivityDate)
            .ToListAsync();

        return Ok(activities);
    }

    #endregion
}

/// <summary>
/// DTO for creating a new event attendee
/// </summary>
public class EventAttendeeCreateDto
{
    public AttendeeType AttendeeType { get; set; }
    public int AttendeeId { get; set; }
    public bool IsOrganizer { get; set; }
    public bool IsRequired { get; set; } = true;
    public string? Role { get; set; }
}

/// <summary>
/// DTO for updating attendee response
/// </summary>
public class AttendeeResponseDto
{
    public AttendeeResponseStatus ResponseStatus { get; set; }
}

/// <summary>
/// DTO for updating attendance
/// </summary>
public class AttendanceDto
{
    public bool DidAttend { get; set; }
}
