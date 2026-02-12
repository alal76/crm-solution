// -----------------------------------------------------------------------
// CRM Solution - Semantic Kernel AI Plugins
// Copyright (c) 2024-2026 Abhishek Lal (CRM Solution). All rights reserved.
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.
//
// This file is part of the CRM Solution, an enterprise-grade
// Customer Relationship Management system.
//
// Author: Abhishek Lal
// Repository: https://github.com/abhisheklal04/crm-solution
// Documentation: See /docs folder for architecture and API reference
//
// IMPORTANT: This is proprietary code. Unauthorized copying, modification,
// or distribution is strictly prohibited.
// -----------------------------------------------------------------------

#nullable enable

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM calendar and activity management.
/// Provides AI-accessible functions for querying activities, viewing upcoming events, and logging new activities.
/// </summary>
public class CalendarPlugin : CrmPluginBase
{
    private readonly IActivityService _activityService;

    /// <inheritdoc />
    public override string PluginName => "Calendar";

    /// <inheritdoc />
    public override string Description => "Manage CRM activities and calendar — view upcoming events, query activity history, check statistics, and log new activities.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarPlugin"/> class.
    /// </summary>
    /// <param name="activityService">The activity service for activity CRUD operations.</param>
    /// <param name="logger">The logger instance.</param>
    public CalendarPlugin(
        IActivityService activityService,
        ILogger<CalendarPlugin> logger) : base(logger)
    {
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
    }

    #region Read Operations

    /// <summary>
    /// Retrieves activities with optional filters for customer, user, type, and date range.
    /// </summary>
    /// <param name="customerId">Optional customer ID to filter by.</param>
    /// <param name="userId">Optional user ID to filter by.</param>
    /// <param name="activityType">Optional activity type to filter by (e.g., "EmailSent", "CallMade", "MeetingScheduled").</param>
    /// <param name="daysBack">Number of days to look back. Defaults to 30.</param>
    /// <param name="limit">Maximum number of activities to return. Defaults to 25.</param>
    /// <returns>A JSON array of matching activities.</returns>
    [KernelFunction("GetActivities")]
    [Description("Get activities filtered by customer, user, type, and date range.")]
    public async Task<string> GetActivitiesAsync(
        [Description("Customer ID to filter by (optional)")] int? customerId = null,
        [Description("User ID to filter by (optional)")] int? userId = null,
        [Description("Activity type filter: EmailSent, CallMade, MeetingScheduled, TaskCompleted, NoteAdded (optional)")] string? activityType = null,
        [Description("Number of days to look back from today")] int daysBack = 30,
        [Description("Maximum number of activities to return")] int limit = 25)
    {
        try
        {
            ActivityType? parsedType = null;
            if (!string.IsNullOrEmpty(activityType) && Enum.TryParse<ActivityType>(activityType, true, out var at))
            {
                parsedType = at;
            }

            var fromDate = DateTime.UtcNow.AddDays(-daysBack);

            var activities = await _activityService.GetActivitiesAsync(
                customerId: customerId,
                opportunityId: null,
                userId: userId,
                activityType: parsedType,
                fromDate: fromDate,
                toDate: DateTime.UtcNow,
                limit: limit);

            var summaries = activities.Select(a => new
            {
                a.Id,
                Type = a.ActivityType.ToString(),
                a.Title,
                a.Details,
                a.EntityType,
                a.EntityId,
                a.UserId,
                a.CreatedAt
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetActivities", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the most recent activities across the CRM.
    /// </summary>
    /// <param name="limit">Maximum number of recent activities to return. Defaults to 20.</param>
    /// <returns>A JSON array of the most recent activities.</returns>
    [KernelFunction("GetUpcoming")]
    [Description("Get the most recent CRM activities across all entities.")]
    public async Task<string> GetUpcomingAsync(
        [Description("Maximum number of recent activities to return")] int limit = 20)
    {
        try
        {
            var activities = await _activityService.GetRecentAsync(limit);

            var summaries = activities.Select(a => new
            {
                a.Id,
                Type = a.ActivityType.ToString(),
                a.Title,
                a.Details,
                a.EntityType,
                a.EntityId,
                a.UserId,
                a.CreatedAt
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetUpcoming", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves activity statistics for a given date range.
    /// </summary>
    /// <param name="daysBack">Number of days to look back for statistics. Defaults to 30.</param>
    /// <returns>A JSON object with activity statistics and counts by type.</returns>
    [KernelFunction("GetActivityStats")]
    [Description("Get activity statistics including counts by type for a given period.")]
    public async Task<string> GetActivityStatsAsync(
        [Description("Number of days to look back for statistics")] int daysBack = 30)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-daysBack);
            var stats = await _activityService.GetStatsAsync(fromDate, DateTime.UtcNow);

            return SuccessResult(stats);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetActivityStats", ex.Message);
        }
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Logs a new activity in the CRM.
    /// </summary>
    /// <param name="subject">The activity subject/title.</param>
    /// <param name="details">The activity details or description.</param>
    /// <param name="activityType">The type of activity: EmailSent, CallMade, MeetingScheduled, TaskCompleted, NoteAdded.</param>
    /// <param name="entityType">The related entity type (e.g., "Account", "Contact", "Opportunity").</param>
    /// <param name="entityId">The related entity ID.</param>
    /// <returns>A JSON object with the created activity details.</returns>
    [KernelFunction("LogActivity")]
    [Description("Log a new activity (call, email, meeting, note, task) in the CRM.")]
    [RequiresApproval(Tier = "low", Description = "Creates a new activity record in the CRM")]
    public async Task<string> LogActivityAsync(
        [Description("Activity subject or title")] string subject,
        [Description("Activity details or description")] string details,
        [Description("Activity type: EmailSent, CallMade, MeetingScheduled, TaskCompleted, NoteAdded")] string activityType,
        [Description("Related entity type (Account, Contact, Opportunity)")] string entityType,
        [Description("Related entity ID")] int entityId)
    {
        try
        {
            if (!Enum.TryParse<ActivityType>(activityType, true, out var parsedType))
            {
                return SuccessResult(new { success = false, message = $"Invalid activity type '{activityType}'. Valid types: EmailSent, CallMade, MeetingScheduled, TaskCompleted, NoteAdded" });
            }

            var activity = new Activity
            {
                Title = subject,
                Details = details,
                ActivityType = parsedType,
                EntityType = entityType,
                EntityId = entityId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _activityService.CreateAsync(activity);

            return SuccessResult(new
            {
                success = true,
                activityId = created.Id,
                type = created.ActivityType.ToString(),
                subject = created.Title,
                entityType = created.EntityType,
                entityId = created.EntityId,
                createdAt = created.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("LogActivity", ex.Message);
        }
    }

    #endregion
}
