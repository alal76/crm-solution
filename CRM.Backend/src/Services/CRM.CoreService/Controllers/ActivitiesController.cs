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
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities(
        [FromQuery] int? accountId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? activityType = null,
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
            query = query.Where(a => (int)a.ActivityType == activityType);

        if (fromDate.HasValue)
            query = query.Where(a => a.ActivityDate >= fromDate);

        if (toDate.HasValue)
            query = query.Where(a => a.ActivityDate <= toDate);

        var activities = await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ActivityDto>();
        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
            dtos.Add(MapToDto(a));
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get an activity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityDto>> GetActivity(int id)
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

        return Ok(MapToDto(activity));
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ActivityDto>> CreateActivity([FromBody] CreateActivityDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var activity = new Activity
        {
            ActivityType = (ActivityType)dto.ActivityType,
            Title = dto.Title,
            Description = dto.Description,
            Details = dto.Details,
            ActivityDate = dto.ActivityDate ?? DateTime.UtcNow,
            DurationMinutes = dto.DurationMinutes,
            UserId = dto.UserId,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            AccountId = dto.AccountId,
            ContactId = dto.ContactId,
            OpportunityId = dto.OpportunityId,
            IsSystem = dto.IsSystem,
            IsPrivate = dto.IsPrivate,
            IsImportant = dto.IsImportant,
            Tags = dto.Tags,
            Source = dto.Source,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, MapToDto(activity));
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
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivitiesByEntity(
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

        var dtos = new List<ActivityDto>();
        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
            dtos.Add(MapToDto(a));
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get account timeline (all activities related to an account)
    /// </summary>
    [HttpGet("account/{accountId}/timeline")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAccountTimeline(int accountId, [FromQuery] int limit = 100)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Where(a => a.AccountId == accountId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        var dtos = activities.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get opportunity timeline
    /// </summary>
    [HttpGet("opportunity/{opportunityId}/timeline")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetOpportunityTimeline(int opportunityId, [FromQuery] int limit = 100)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Where(a => a.OpportunityId == opportunityId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ActivityDto>();
        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
            dtos.Add(MapToDto(a));
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get recent activities for dashboard
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetRecentActivities([FromQuery] int limit = 20)
    {
        var activities = await _context.Activities
            .Include(a => a.User)
            .Include(a => a.Account)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

        var dtos = new List<ActivityDto>();
        foreach (var a in activities)
        {
            var nt = await _normalization.GetTagsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(nt)) a.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Activity", a.Id);
            if (!string.IsNullOrWhiteSpace(cf)) a.CustomFields = cf;
            dtos.Add(MapToDto(a));
        }

        return Ok(dtos);
    }
    /// <summary>
    /// Update an activity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ActivityDto>> UpdateActivity(int id, [FromBody] UpdateActivityDto dto)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
            return NotFound();

        if (dto.Title != null) activity.Title = dto.Title;
        if (dto.Description != null) activity.Description = dto.Description;
        if (dto.Details != null) activity.Details = dto.Details;
        if (dto.ActivityDate.HasValue) activity.ActivityDate = dto.ActivityDate.Value;
        if (dto.DurationMinutes.HasValue) activity.DurationMinutes = dto.DurationMinutes;
        if (dto.UserId.HasValue) activity.UserId = dto.UserId;
        if (dto.AccountId.HasValue) activity.AccountId = dto.AccountId;
        if (dto.ContactId.HasValue) activity.ContactId = dto.ContactId;
        if (dto.OpportunityId.HasValue) activity.OpportunityId = dto.OpportunityId;
        if (dto.IsPrivate.HasValue) activity.IsPrivate = dto.IsPrivate.Value;
        if (dto.IsImportant.HasValue) activity.IsImportant = dto.IsImportant.Value;
        if (dto.Tags != null) activity.Tags = dto.Tags;
        activity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(activity));
    }

    // Helper method to map Activity entity to ActivityDto
    private static ActivityDto MapToDto(Activity a)
    {
        return new ActivityDto
        {
            Id = a.Id,
            ActivityType = (int)a.ActivityType,
            Title = a.Title,
            Description = a.Description,
            Details = a.Details,
            ActivityDate = a.ActivityDate,
            DurationMinutes = a.DurationMinutes,
            UserId = a.UserId,
            UserName = a.User?.FullName,
            UserEmail = a.User?.Email,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            EntityName = null, // Populate if needed
            SecondaryEntityType = a.SecondaryEntityType,
            SecondaryEntityId = a.SecondaryEntityId,
            SecondaryEntityName = a.SecondaryEntityName,
            AccountId = a.AccountId,
            ContactId = a.ContactId,
            OpportunityId = a.OpportunityId,
            CampaignId = a.CampaignId,
            ProductId = a.ProductId,
            TaskId = a.TaskId,
            QuoteId = a.QuoteId,
            InteractionId = a.InteractionId,
            NoteId = a.NoteId,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            FieldsChanged = a.FieldsChanged,
            IsSystem = a.IsSystem,
            IsPrivate = a.IsPrivate,
            IsImportant = a.IsImportant,
            Tags = a.Tags,
            Category = a.Category,
            Source = a.Source,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            IsDeleted = a.IsDeleted,
            RowVersion = a.RowVersion
        };
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
}
