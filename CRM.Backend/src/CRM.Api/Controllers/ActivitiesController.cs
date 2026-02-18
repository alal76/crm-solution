// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing activities (timeline/activity feed).
/// </summary>
/// <remarks>
/// Provides operations for:
/// - Activity feed management (create, read, delete)
/// - Timeline views for accounts, opportunities, and entities
/// - Event attendee management (add, update response, track attendance)
/// - Activity statistics and recent activity retrieval
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
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
    /// Get all activities with optional filtering.
    /// </summary>
    /// <param name="accountId">Filter by account ID.</param>
    /// <param name="opportunityId">Filter by opportunity ID.</param>
    /// <param name="userId">Filter by user ID who created the activity.</param>
    /// <param name="activityType">Filter by activity type.</param>
    /// <param name="fromDate">Filter activities from this date.</param>
    /// <param name="toDate">Filter activities until this date.</param>
    /// <param name="limit">Maximum number of activities to return (default: 50).</param>
    /// <returns>List of activities matching the filter criteria.</returns>
    /// <response code="200">Returns the list of activities.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Activity>>> GetActivities(
        [FromQuery] int? accountId = null,
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

        if (accountId.HasValue)
            query = query.Where(a => a.AccountId == accountId);

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
    /// Get a specific activity by ID.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <returns>The activity with the specified ID.</returns>
    /// <response code="200">Returns the activity.</response>
    /// <response code="404">Activity not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Create a new activity.
    /// </summary>
    /// <param name="activity">The activity data to create.</param>
    /// <returns>The created activity.</returns>
    /// <response code="201">Returns the newly created activity.</response>
    /// <response code="400">Invalid activity data provided.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(activity.Title) || activity.Title.Length > 255)
            return BadRequest("Title is required and must be <= 255 characters.");
        if (activity.ActivityType < 0)
            return BadRequest("ActivityType is required.");
        if (!string.IsNullOrEmpty(activity.EntityType) && activity.EntityType.Length > 100)
            return BadRequest("EntityType must be <= 100 characters.");
        if (!string.IsNullOrEmpty(activity.SecondaryEntityType) && activity.SecondaryEntityType.Length > 100)
            return BadRequest("SecondaryEntityType must be <= 100 characters.");

        activity.CreatedAt = DateTime.UtcNow;
        activity.UpdatedAt = DateTime.UtcNow;

        if (activity.ActivityDate == default)
            activity.ActivityDate = DateTime.UtcNow;

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
    }

    /// <summary>
    /// Delete an activity.
    /// </summary>
    /// <param name="id">The activity ID to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Activity successfully deleted.</response>
    /// <response code="404">Activity not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get activities for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type (e.g., Account, Contact, Lead).</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="limit">Maximum number of activities to return (default: 50).</param>
    /// <returns>List of activities for the specified entity.</returns>
    /// <response code="200">Returns the list of activities for the entity.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get account timeline (all activities related to an account).
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="limit">Maximum number of activities to return (default: 100).</param>
    /// <returns>Timeline of activities for the account.</returns>
    /// <response code="200">Returns the account timeline.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("account/{accountId}/timeline")]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Activity>>> GetAccountTimeline(int accountId, [FromQuery] int limit = 100)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Where(a => a.AccountId == accountId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        return Ok(activities);
    }

    /// <summary>
    /// Get opportunity timeline.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID.</param>
    /// <param name="limit">Maximum number of activities to return (default: 100).</param>
    /// <returns>Timeline of activities for the opportunity.</returns>
    /// <response code="200">Returns the opportunity timeline.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("opportunity/{opportunityId}/timeline")]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get recent activities for dashboard.
    /// </summary>
    /// <param name="limit">Maximum number of activities to return (default: 20).</param>
    /// <returns>List of recent activities.</returns>
    /// <response code="200">Returns the list of recent activities.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get activity statistics.
    /// </summary>
    /// <param name="fromDate">Start date for statistics (default: 30 days ago).</param>
    /// <param name="toDate">End date for statistics (default: today).</param>
    /// <returns>Activity statistics including counts by type and day.</returns>
    /// <response code="200">Returns activity statistics.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get attendees for an activity/event.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <returns>List of attendees for the activity.</returns>
    /// <response code="200">Returns the list of attendees.</response>
    /// <response code="404">Activity not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{activityId}/attendees")]
    [ProducesResponseType(typeof(IEnumerable<EventAttendee>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Add an attendee to an activity/event.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="dto">The attendee data.</param>
    /// <returns>The created attendee.</returns>
    /// <response code="201">Returns the newly created attendee.</response>
    /// <response code="400">Invalid attendee data provided.</response>
    /// <response code="404">Activity not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{activityId}/attendees")]
    [ProducesResponseType(typeof(EventAttendee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get a specific attendee.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="attendeeId">The attendee ID.</param>
    /// <returns>The attendee details.</returns>
    /// <response code="200">Returns the attendee.</response>
    /// <response code="404">Attendee not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{activityId}/attendees/{attendeeId}")]
    [ProducesResponseType(typeof(EventAttendee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventAttendee>> GetAttendee(int activityId, int attendeeId)
    {
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.ActivityId == activityId);

        if (attendee == null)
            return NotFound();

        return Ok(attendee);
    }

    /// <summary>
    /// Update attendee response status.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="attendeeId">The attendee ID.</param>
    /// <param name="dto">The response status update.</param>
    /// <returns>The updated attendee.</returns>
    /// <response code="200">Returns the updated attendee.</response>
    /// <response code="400">Invalid response data provided.</response>
    /// <response code="404">Attendee not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPatch("{activityId}/attendees/{attendeeId}/respond")]
    [ProducesResponseType(typeof(EventAttendee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Mark attendee as attended/not attended (for completed events).
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="attendeeId">The attendee ID.</param>
    /// <param name="dto">The attendance status update.</param>
    /// <returns>The updated attendee.</returns>
    /// <response code="200">Returns the updated attendee.</response>
    /// <response code="400">Invalid attendance data provided.</response>
    /// <response code="404">Attendee not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPatch("{activityId}/attendees/{attendeeId}/attendance")]
    [ProducesResponseType(typeof(EventAttendee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Remove an attendee from an activity/event.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="attendeeId">The attendee ID to remove.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Attendee successfully removed.</response>
    /// <response code="404">Attendee not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{activityId}/attendees/{attendeeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get all events a user/contact/lead is attending.
    /// </summary>
    /// <param name="attendeeType">The type of attendee (User, Contact, Lead).</param>
    /// <param name="attendeeId">The attendee's entity ID.</param>
    /// <param name="fromDate">Filter events starting from this date.</param>
    /// <param name="toDate">Filter events until this date.</param>
    /// <returns>List of activities/events the attendee is part of.</returns>
    /// <response code="200">Returns the list of events for the attendee.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("attendee/{attendeeType}/{attendeeId:int}/events")]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
