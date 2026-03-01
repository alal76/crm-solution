// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing activities and timeline operations.
/// Supports all activity types including ChatMessage for chat provider integration.
/// </summary>
public class ActivityService : IActivityService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        ICrmDbContext context,
        ILogger<ActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Mapping helpers
    private static ActivityDto ToDto(Activity a) => new ActivityDto
    {
        Id = a.Id,
        ActivityType = (int)a.ActivityType,
        Title = a.Title,
        Description = a.Description,
        Details = a.Details,
        ActivityDate = a.ActivityDate,
        DurationMinutes = a.DurationMinutes,
        UserId = a.UserId,
        UserName = a.UserName,
        UserEmail = a.UserEmail,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        EntityName = a.EntityName,
        AccountId = a.AccountId,
        ContactId = a.ContactId,
        OpportunityId = a.OpportunityId,
        IsSystem = a.IsSystem,
        IsPrivate = a.IsPrivate,
        IsImportant = a.IsImportant,
        Tags = a.Tags,
        Source = a.Source,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt ?? a.CreatedAt,
        IsDeleted = a.IsDeleted,
        RowVersion = a.RowVersion
    };

    private static Activity ToEntity(CreateActivityDto dto)
    {
        return new Activity
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
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? userId = null,
        int? activityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50)
    {
        var query = _context.Activities.AsQueryable();

        if (accountId.HasValue)
        {
            query = query.Where(a => a.AccountId == accountId);
        }
        if (opportunityId.HasValue)
        {
            query = query.Where(a => a.OpportunityId == opportunityId);
        }
        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId);
        }
        if (activityType.HasValue)
        {
            query = query.Where(a => (int)a.ActivityType == activityType);
        }
        if (fromDate.HasValue)
        {
            query = query.Where(a => a.ActivityDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(a => a.ActivityDate <= toDate.Value);
        }

        var list = await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<ActivityDto?> GetByIdAsync(int id)
    {
        var a = await _context.Activities
            .Include(x => x.User)
            .Include(x => x.Account)
            .Include(x => x.Opportunity)
            .FirstOrDefaultAsync(x => x.Id == id);
        return a == null ? null : ToDto(a);
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityDto dto)
    {
        var entity = ToEntity(dto);
        _context.Activities.Add(entity);
        await _context.SaveChangesAsync();
        _logger.LogDebug("Created activity {Id} of type {Type}", entity.Id, entity.ActivityType);
        return ToDto(entity);
    }

    public async Task<ActivityDto> CreateAsync(Activity activity)
    {
        if (activity.ActivityDate == default)
        {
            activity.ActivityDate = DateTime.UtcNow;
        }
        activity.CreatedAt = DateTime.UtcNow;
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        _logger.LogDebug("Created activity {Id} of type {Type}", activity.Id, activity.ActivityType);
        return ToDto(activity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
        {
            return false;
        }
        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();
        _logger.LogDebug("Deleted activity {Id}", id);
        return true;
    }

    public async Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, int entityId, int limit = 50)
    {
        var list = await _context.Activities
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetAccountTimelineAsync(int accountId, int limit = 100)
    {
        var list = await _context.Activities
            .Where(a => a.AccountId == accountId || (a.EntityType == "Account" && a.EntityId == accountId))
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetOpportunityTimelineAsync(int opportunityId, int limit = 100)
    {
        var list = await _context.Activities
            .Where(a => a.OpportunityId == opportunityId || (a.EntityType == "Opportunity" && a.EntityId == opportunityId))
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetRecentAsync(int limit = 20)
    {
        var list = await _context.Activities
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<ActivityStats> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Activities.AsQueryable();
        if (fromDate.HasValue)
        {
            query = query.Where(a => a.ActivityDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(a => a.ActivityDate <= toDate.Value);
        }
        var activities = await query.ToListAsync();
        var stats = new ActivityStats
        {
            TotalActivities = activities.Count,
            EmailsSent = activities.Count(a => a.ActivityType == ActivityType.EmailSent),
            CallsMade = activities.Count(a => a.ActivityType == ActivityType.CallMade),
            MeetingsCompleted = activities.Count(a => a.ActivityType == ActivityType.MeetingCompleted),
            OpportunitiesCreated = activities.Count(a => a.ActivityType == ActivityType.OpportunityCreated),
            OpportunitiesWon = activities.Count(a => a.ActivityType == ActivityType.OpportunityWon),
            OpportunitiesLost = activities.Count(a => a.ActivityType == ActivityType.OpportunityLost),
            QuotesSent = activities.Count(a => a.ActivityType == ActivityType.QuoteSent),
            QuotesAccepted = activities.Count(a => a.ActivityType == ActivityType.QuoteAccepted),
            TasksCompleted = activities.Count(a => a.ActivityType == ActivityType.TaskCompleted),
            ActivitiesByType = activities
                .GroupBy(a => a.ActivityType)
                .Select(g => new ActivityTypeCount
                {
                    Type = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ActivitiesByDay = activities
                .GroupBy(a => a.ActivityDate.Date)
                .Select(g => new ActivityDayCount
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList()
        };
        return stats;
    }


    /// <summary>
    /// Gets chat activities grouped by conversation for timeline display.
    /// </summary>
    /// <param name="accountId">The account/customer ID</param>
    /// <param name="limit">Maximum number of conversations to return</param>
    /// <returns>Chat activities grouped by conversation</returns>
    public async Task<IEnumerable<ChatConversationGroup>> GetChatConversationsAsync(
        int accountId,
        int limit = 20)
    {
        var chatActivities = await _context.Activities
            .Where(a => a.AccountId == accountId && a.ActivityType == ActivityType.ChatMessage)
            .OrderByDescending(a => a.ActivityDate)
            .ToListAsync();

        // Group by conversation (using Details JSON field which contains chatwootConversationId)
        var grouped = chatActivities
            .GroupBy(a => ExtractConversationId(a.Details))
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .Select(g => new ChatConversationGroup
            {
                ConversationId = g.Key!,
                MessageCount = g.Count(),
                FirstMessageAt = g.Min(a => a.ActivityDate),
                LastMessageAt = g.Max(a => a.ActivityDate),
                Messages = g.OrderBy(a => a.ActivityDate).ToList(),
                Channel = ExtractChannel(g.First().Details),
                Status = ExtractStatus(g.First().Details)
            })
            .OrderByDescending(g => g.LastMessageAt)
            .Take(limit)
            .ToList();

        return grouped;
    }

    private static string? ExtractConversationId(string? details)
    {
        if (string.IsNullOrEmpty(details))
        {
            return null;
        }
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(details);
            if (doc.RootElement.TryGetProperty("chatwootConversationId", out var prop))
            {
                return prop.GetInt32().ToString();
            }
        }
        catch
        {
            // Ignore parse errors
        }
        return null;
    }

    private static string? ExtractChannel(string? details)
    {
        if (string.IsNullOrEmpty(details))
        {
            return null;
        }
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(details);
            if (doc.RootElement.TryGetProperty("channel", out var prop))
            {
                return prop.GetString();
            }
        }
        catch
        {
            // Ignore parse errors
        }
        return null;
    }

    private static string? ExtractStatus(string? details)
    {
        if (string.IsNullOrEmpty(details))
        {
            return null;
        }
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(details);
            if (doc.RootElement.TryGetProperty("status", out var prop))
            {
                return prop.GetString();
            }
        }
        catch
        {
            // Ignore parse errors
        }
        return null;
    }
}

/// <summary>
/// Represents a group of chat messages from the same conversation.
/// </summary>
public class ChatConversationGroup
{
    /// <summary>External conversation ID (e.g., Chatwoot conversation ID)</summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>Total number of messages in the conversation</summary>
    public int MessageCount { get; set; }

    /// <summary>When the first message was sent</summary>
    public DateTime FirstMessageAt { get; set; }

    /// <summary>When the last message was sent</summary>
    public DateTime LastMessageAt { get; set; }

    /// <summary>All messages in the conversation</summary>
    public List<Activity> Messages { get; set; } = new();

    /// <summary>Channel (web, whatsapp, facebook, etc.)</summary>
    public string? Channel { get; set; }

    /// <summary>Conversation status (open, resolved, etc.)</summary>
    public string? Status { get; set; }
}
