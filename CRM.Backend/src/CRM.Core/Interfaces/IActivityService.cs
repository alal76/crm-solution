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
