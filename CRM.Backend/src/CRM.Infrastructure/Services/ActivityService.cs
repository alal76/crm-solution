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

    /// <inheritdoc />
    public async Task<IEnumerable<Activity>> GetActivitiesAsync(
        int? customerId = null,
        int? opportunityId = null,
        int? userId = null,
        ActivityType? activityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50)
    {
        var query = _context.Activities.AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(a => a.AccountId == customerId);
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
            query = query.Where(a => a.ActivityType == activityType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.ActivityDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.ActivityDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Activity?> GetByIdAsync(int id)
    {
        return await _context.Activities
            .Include(a => a.User)
            .Include(a => a.Account)
            .Include(a => a.Opportunity)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <inheritdoc />
    public async Task<Activity> CreateAsync(Activity activity)
    {
        // Set defaults
        if (activity.ActivityDate == default)
        {
            activity.ActivityDate = DateTime.UtcNow;
        }

        activity.CreatedAt = DateTime.UtcNow;
        activity.UpdatedAt = DateTime.UtcNow;

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        _logger.LogDebug(
            "Created activity {Id} of type {Type} for entity {EntityType}:{EntityId}",
            activity.Id, activity.ActivityType, activity.EntityType, activity.EntityId);

        return activity;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IEnumerable<Activity>> GetByEntityAsync(
        string entityType,
        int entityId,
        int limit = 50)
    {
        return await _context.Activities
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Activity>> GetCustomerTimelineAsync(
        int customerId,
        int limit = 100)
    {
        // Get all activities related to a customer (account)
        // This includes direct account activities, contact activities, opportunity activities, and chat messages
        return await _context.Activities
            .Where(a =>
                a.AccountId == customerId ||
                (a.EntityType == "Account" && a.EntityId == customerId) ||
                (a.EntityType == "Customer" && a.EntityId == customerId))
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Activity>> GetOpportunityTimelineAsync(
        int opportunityId,
        int limit = 100)
    {
        return await _context.Activities
            .Where(a =>
                a.OpportunityId == opportunityId ||
                (a.EntityType == "Opportunity" && a.EntityId == opportunityId))
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Activity>> GetRecentAsync(int limit = 20)
    {
        return await _context.Activities
            .OrderByDescending(a => a.ActivityDate)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ActivityStats> GetStatsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null)
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
        if (string.IsNullOrEmpty(details)) return null;
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
        if (string.IsNullOrEmpty(details)) return null;
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
        if (string.IsNullOrEmpty(details)) return null;
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
