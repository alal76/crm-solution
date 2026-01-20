using CRM.Core.Entities;
using CRM.Infrastructure.Data;
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

    public ActivitiesController(CrmDbContext context, ILogger<ActivitiesController> logger)
    {
        _context = context;
        _logger = logger;
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
            .Include(a => a.Customer)
            .Include(a => a.Opportunity)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId);
        
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
            .Include(a => a.Customer)
            .Include(a => a.Opportunity)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
            return NotFound();

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
            .Where(a => a.CustomerId == customerId)
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
            .Include(a => a.Customer)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();

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
}
