// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.DTOs;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing activities (timeline/activity feed)
/// </summary>
public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetActivitiesAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? userId = null,
        int? activityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50);

    Task<ActivityDto?> GetByIdAsync(int id);

    Task<ActivityDto> CreateAsync(CreateActivityDto dto);

    Task<ActivityDto> CreateAsync(Activity activity);

    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, int entityId, int limit = 50);

    Task<IEnumerable<ActivityDto>> GetAccountTimelineAsync(int accountId, int limit = 100);

    Task<IEnumerable<ActivityDto>> GetOpportunityTimelineAsync(int opportunityId, int limit = 100);

    Task<IEnumerable<ActivityDto>> GetRecentAsync(int limit = 20);

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
