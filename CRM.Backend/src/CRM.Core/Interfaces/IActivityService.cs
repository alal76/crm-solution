using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing activities (timeline/activity feed)
/// </summary>
public interface IActivityService
{
    /// <summary>
    /// Get activities with optional filtering
    /// </summary>
    Task<IEnumerable<Activity>> GetActivitiesAsync(
        int? customerId = null,
        int? opportunityId = null,
        int? userId = null,
        ActivityType? activityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50);

    /// <summary>
    /// Get an activity by ID
    /// </summary>
    Task<Activity?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new activity
    /// </summary>
    Task<Activity> CreateAsync(Activity activity);

    /// <summary>
    /// Delete an activity
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Get activities for a specific entity
    /// </summary>
    Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, int entityId, int limit = 50);

    /// <summary>
    /// Get customer timeline (all activities related to a customer)
    /// </summary>
    Task<IEnumerable<Activity>> GetCustomerTimelineAsync(int customerId, int limit = 100);

    /// <summary>
    /// Get opportunity timeline
    /// </summary>
    Task<IEnumerable<Activity>> GetOpportunityTimelineAsync(int opportunityId, int limit = 100);

    /// <summary>
    /// Get recent activities for dashboard
    /// </summary>
    Task<IEnumerable<Activity>> GetRecentAsync(int limit = 20);

    /// <summary>
    /// Get activity statistics
    /// </summary>
    Task<ActivityStats> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Activity statistics DTO
/// </summary>
public class ActivityStats
{
    public int TotalActivities { get; set; }
    public int EmailsSent { get; set; }
    public int CallsMade { get; set; }
    public int MeetingsCompleted { get; set; }
    public int OpportunitiesCreated { get; set; }
    public int OpportunitiesWon { get; set; }
    public int OpportunitiesLost { get; set; }
    public int QuotesSent { get; set; }
    public int QuotesAccepted { get; set; }
    public int TasksCompleted { get; set; }
    public IEnumerable<ActivityTypeCount> ActivitiesByType { get; set; } = new List<ActivityTypeCount>();
    public IEnumerable<ActivityDayCount> ActivitiesByDay { get; set; } = new List<ActivityDayCount>();
}

public class ActivityTypeCount
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ActivityDayCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
