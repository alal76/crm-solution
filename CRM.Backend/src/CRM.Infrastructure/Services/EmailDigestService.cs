// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IEmailDigestService (REV-FE-002).
///
/// Config CRUD backs GET/PUT /api/users/me/email-digest. Content aggregation assembles each
/// digest section from real, existing data sources (ITaskService, IActivityService, Leads,
/// Opportunities). TeamPerformance and KpiSummary are intentionally simple v1 aggregates rather
/// than a full analytics build — see remarks below and on EmailDigestContentDto.
/// </summary>
public class EmailDigestService : IEmailDigestService
{
    private readonly ICrmDbContext _context;
    private readonly ITaskService _taskService;
    private readonly IActivityService _activityService;
    private readonly INotificationPort _notificationPort;
    private readonly ILogger<EmailDigestService> _logger;

    public EmailDigestService(
        ICrmDbContext context,
        ITaskService taskService,
        IActivityService activityService,
        INotificationPort notificationPort,
        ILogger<EmailDigestService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _notificationPort = notificationPort ?? throw new ArgumentNullException(nameof(notificationPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Config CRUD

    public async Task<EmailDigestConfigDto> GetConfigAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.EmailDigestConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

        return entity == null ? DefaultDto() : ToDto(entity);
    }

    public async Task<EmailDigestConfigDto> UpdateConfigAsync(int userId, EmailDigestConfigDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await _context.EmailDigestConfigs
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

        var frequency = ParseFrequency(dto.Frequency);

        if (entity == null)
        {
            entity = new EmailDigestConfig
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.EmailDigestConfigs.Add(entity);
        }

        entity.IsEnabled = dto.Enabled;
        entity.Frequency = frequency;
        entity.DayOfWeek = frequency == EmailDigestFrequency.Weekly ? dto.DayOfWeek : null;
        entity.DayOfMonth = frequency == EmailDigestFrequency.Monthly ? dto.DayOfMonth : null;
        entity.TimeOfDay = ParseTimeOfDay(dto.TimeOfDay);
        entity.Timezone = string.IsNullOrWhiteSpace(dto.Timezone) ? "UTC" : dto.Timezone;

        entity.IncludeNewLeads = dto.Sections.NewLeads;
        entity.IncludeOpenOpportunities = dto.Sections.OpenOpportunities;
        entity.IncludeRecentActivities = dto.Sections.RecentActivities;
        entity.IncludeUpcomingTasks = dto.Sections.UpcomingTasks;
        entity.IncludeOverdueTasks = dto.Sections.OverdueTasks;
        entity.IncludeTeamPerformance = dto.Sections.TeamPerformance;
        entity.IncludeKpiSummary = dto.Sections.KpiSummary;

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Email digest config saved for user {UserId} (enabled={Enabled}, frequency={Frequency})",
            userId, entity.IsEnabled, entity.Frequency);

        return ToDto(entity);
    }

    #endregion

    #region Content Aggregation

    public async Task<EmailDigestContentDto> BuildDigestContentAsync(int userId, EmailDigestConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        var periodStart = config.LastSentAt ?? DateTime.UtcNow.AddDays(-7);

        var content = new EmailDigestContentDto
        {
            UserId = userId,
            UserDisplayName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : $"User {userId}",
            GeneratedAtUtc = DateTime.UtcNow,
            PeriodStartUtc = periodStart
        };

        if (config.IncludeNewLeads)
        {
            content.NewLeads = await _context.Leads
                .AsNoTracking()
                .Where(l => l.OwnerId == userId && !l.IsDeleted && l.CreatedAt >= periodStart)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new EmailDigestLeadItem
                {
                    Id = l.Id,
                    Name = (l.FirstName + " " + l.LastName).Trim(),
                    Company = l.CompanyName,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }

        if (config.IncludeOpenOpportunities)
        {
            content.OpenOpportunities = await _context.Opportunities
                .AsNoTracking()
                .Where(o => o.SalesOwnerId == userId && !o.IsDeleted
                    && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .OrderByDescending(o => o.Amount)
                .Select(o => new EmailDigestOpportunityItem
                {
                    Id = o.Id,
                    Name = o.Name,
                    Amount = o.Amount,
                    Stage = o.Stage.ToString(),
                    ExpectedCloseDate = o.ExpectedCloseDate
                })
                .ToListAsync(cancellationToken);
        }

        if (config.IncludeRecentActivities)
        {
            var activities = await _activityService.GetActivitiesAsync(
                userId: userId,
                fromDate: periodStart,
                toDate: DateTime.UtcNow,
                limit: 25);

            content.RecentActivities = activities
                .Select(a => new EmailDigestActivityItem
                {
                    Id = a.Id,
                    Title = a.Title,
                    ActivityType = ((ActivityType)a.ActivityType).ToString(),
                    ActivityDate = a.ActivityDate
                })
                .ToList();
        }

        if (config.IncludeUpcomingTasks)
        {
            var dueToday = await _taskService.GetTasksDueTodayAsync(userId);
            content.UpcomingTasks = dueToday
                .Select(ToTaskItem)
                .ToList();
        }

        if (config.IncludeOverdueTasks)
        {
            var overdue = await _taskService.GetOverdueTasksAsync();
            content.OverdueTasks = overdue
                .Where(t => t.AssignedToUserId == userId)
                .Select(ToTaskItem)
                .ToList();
        }

        if (config.IncludeTeamPerformance)
        {
            content.TeamPerformance = await BuildTeamPerformanceAsync(userId, periodStart, cancellationToken);
        }

        if (config.IncludeKpiSummary)
        {
            content.KpiSummary = await BuildKpiSummaryAsync(userId, periodStart, cancellationToken);
        }

        return content;
    }

    /// <summary>
    /// v1 scope decision (REV-FE-002): this codebase has no explicit manager/direct-report
    /// hierarchy (no ManagerId on User/Department). As a best-effort proxy for "team performance",
    /// direct reports = other active users in the same department as the requesting user. This is
    /// an honest, simple aggregate — not a full analytics/org-chart feature.
    /// </summary>
    private async Task<EmailDigestTeamPerformance> BuildTeamPerformanceAsync(int userId, DateTime periodStart, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var teamUserIds = new List<int>();
        if (user?.DepartmentId != null)
        {
            teamUserIds = await _context.Users
                .AsNoTracking()
                .Where(u => u.DepartmentId == user.DepartmentId && u.Id != userId && !u.IsDeleted && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
        }

        if (teamUserIds.Count == 0)
        {
            return new EmailDigestTeamPerformance { DirectReportCount = 0, DealsClosedByTeam = 0, ActivitiesLoggedByTeam = 0 };
        }

        var dealsClosedByTeam = await _context.Opportunities
            .AsNoTracking()
            .Where(o => o.SalesOwnerId != null && teamUserIds.Contains(o.SalesOwnerId.Value)
                && o.Stage == OpportunityStage.ClosedWon
                && o.ClosedDate != null && o.ClosedDate >= periodStart)
            .CountAsync(cancellationToken);

        var activitiesLoggedByTeam = await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId != null && teamUserIds.Contains(a.UserId.Value)
                && !a.IsDeleted && a.ActivityDate >= periodStart)
            .CountAsync(cancellationToken);

        return new EmailDigestTeamPerformance
        {
            DirectReportCount = teamUserIds.Count,
            DealsClosedByTeam = dealsClosedByTeam,
            ActivitiesLoggedByTeam = activitiesLoggedByTeam
        };
    }

    /// <summary>
    /// v1 scope decision (REV-FE-002): a small set of obvious, cheap-to-compute counts rather than a
    /// full KPI/analytics dashboard build. Open pipeline + deals/revenue closed this period + tasks
    /// completed this period, all scoped to the requesting user.
    /// </summary>
    private async Task<EmailDigestKpiSummary> BuildKpiSummaryAsync(int userId, DateTime periodStart, CancellationToken cancellationToken)
    {
        var openOpps = await _context.Opportunities
            .AsNoTracking()
            .Where(o => o.SalesOwnerId == userId && !o.IsDeleted
                && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
            .ToListAsync(cancellationToken);

        var closedWon = await _context.Opportunities
            .AsNoTracking()
            .Where(o => o.SalesOwnerId == userId && !o.IsDeleted
                && o.Stage == OpportunityStage.ClosedWon
                && o.ClosedDate != null && o.ClosedDate >= periodStart)
            .ToListAsync(cancellationToken);

        var tasksCompleted = await _context.CrmTasks
            .AsNoTracking()
            .Where(t => t.AssignedToUserId == userId && !t.IsDeleted
                && t.Status == CrmTaskStatus.Completed
                && t.CompletedDate != null && t.CompletedDate >= periodStart)
            .CountAsync(cancellationToken);

        return new EmailDigestKpiSummary
        {
            OpenPipelineCount = openOpps.Count,
            OpenPipelineValue = openOpps.Sum(o => o.Amount),
            DealsClosedWonThisPeriod = closedWon.Count,
            RevenueClosedWonThisPeriod = closedWon.Sum(o => o.Amount),
            TasksCompletedThisPeriod = tasksCompleted
        };
    }

    private static EmailDigestTaskItem ToTaskItem(CrmTask t) => new()
    {
        Id = t.Id,
        Subject = t.Subject,
        DueDate = t.DueDate,
        Priority = t.Priority.ToString()
    };

    #endregion

    #region Render + Send

    public string RenderHtml(User user, EmailDigestContentDto content, EmailDigestConfig config)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(content);

        var sb = new StringBuilder();
        sb.Append("<html><body style=\"font-family:sans-serif;color:#222;\">");
        sb.Append($"<h2>Your CRM Digest</h2>");
        sb.Append($"<p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)}, here's what's new since {content.PeriodStartUtc:yyyy-MM-dd}.</p>");

        AppendSection(sb, "New Leads", content.NewLeads, l => $"{l.Name} {(string.IsNullOrEmpty(l.Company) ? string.Empty : $"({l.Company})")}");
        AppendSection(sb, "Open Opportunities", content.OpenOpportunities, o => $"{o.Name} — {o.Stage} — {o.Amount:C}");
        AppendSection(sb, "Recent Activities", content.RecentActivities, a => $"{a.Title} ({a.ActivityType}) — {a.ActivityDate:g}");
        AppendSection(sb, "Upcoming Tasks", content.UpcomingTasks, t => $"{t.Subject} — due {(t.DueDate.HasValue ? t.DueDate.Value.ToString("d") : "n/a")}");
        AppendSection(sb, "Overdue Tasks", content.OverdueTasks, t => $"{t.Subject} — was due {(t.DueDate.HasValue ? t.DueDate.Value.ToString("d") : "n/a")}");

        if (content.TeamPerformance != null)
        {
            var tp = content.TeamPerformance;
            sb.Append("<h3>Team Performance</h3>");
            sb.Append($"<p>{tp.DirectReportCount} team member(s) — {tp.DealsClosedByTeam} deal(s) closed, {tp.ActivitiesLoggedByTeam} activities logged.</p>");
        }

        if (content.KpiSummary != null)
        {
            var kpi = content.KpiSummary;
            sb.Append("<h3>KPI Summary</h3>");
            sb.Append("<ul>");
            sb.Append($"<li>Open pipeline: {kpi.OpenPipelineCount} deal(s), {kpi.OpenPipelineValue:C}</li>");
            sb.Append($"<li>Closed won this period: {kpi.DealsClosedWonThisPeriod} deal(s), {kpi.RevenueClosedWonThisPeriod:C}</li>");
            sb.Append($"<li>Tasks completed this period: {kpi.TasksCompletedThisPeriod}</li>");
            sb.Append("</ul>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static void AppendSection<T>(StringBuilder sb, string title, List<T>? items, Func<T, string> render)
    {
        if (items == null)
        {
            return;
        }

        sb.Append($"<h3>{title}</h3>");
        if (items.Count == 0)
        {
            sb.Append("<p><em>Nothing to report.</em></p>");
            return;
        }

        sb.Append("<ul>");
        foreach (var item in items)
        {
            sb.Append($"<li>{System.Net.WebUtility.HtmlEncode(render(item))}</li>");
        }
        sb.Append("</ul>");
    }

    public async Task<bool> SendDigestAsync(User user, EmailDigestConfig config, bool isPreview, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(config);

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogWarning("Cannot send email digest to user {UserId} — no email address on file", user.Id);
            return false;
        }

        var content = await BuildDigestContentAsync(user.Id, config, cancellationToken);
        var html = RenderHtml(user, content, config);
        var subject = isPreview ? "[Preview] Your CRM Email Digest" : "Your CRM Email Digest";

        var result = await _notificationPort.SendEmailAsync(new EmailNotificationRequest
        {
            To = user.Email,
            ToName = user.FullName,
            Subject = subject,
            Body = html,
            IsHtml = true
        }, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Email digest send failed for user {UserId}: {Error}", user.Id, result.Error);
            return false;
        }

        if (!isPreview)
        {
            config.LastSentAt = DateTime.UtcNow;
            _context.EmailDigestConfigs.Update(config);
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Email digest sent to user {UserId} (preview={IsPreview})", user.Id, isPreview);
        return true;
    }

    #endregion

    #region Mapping helpers

    private static EmailDigestConfigDto DefaultDto() => new()
    {
        Enabled = false,
        Frequency = "daily",
        TimeOfDay = "08:00",
        Timezone = "UTC",
        Sections = new EmailDigestSectionsDto()
    };

    private static EmailDigestConfigDto ToDto(EmailDigestConfig entity) => new()
    {
        Enabled = entity.IsEnabled,
        Frequency = entity.Frequency.ToString().ToLowerInvariant(),
        DayOfWeek = entity.DayOfWeek,
        DayOfMonth = entity.DayOfMonth,
        TimeOfDay = entity.TimeOfDay.ToString(@"hh\:mm"),
        Timezone = entity.Timezone,
        Sections = new EmailDigestSectionsDto
        {
            NewLeads = entity.IncludeNewLeads,
            OpenOpportunities = entity.IncludeOpenOpportunities,
            RecentActivities = entity.IncludeRecentActivities,
            UpcomingTasks = entity.IncludeUpcomingTasks,
            OverdueTasks = entity.IncludeOverdueTasks,
            TeamPerformance = entity.IncludeTeamPerformance,
            KpiSummary = entity.IncludeKpiSummary
        }
    };

    private static EmailDigestFrequency ParseFrequency(string? frequency) => frequency?.Trim().ToLowerInvariant() switch
    {
        "weekly" => EmailDigestFrequency.Weekly,
        "monthly" => EmailDigestFrequency.Monthly,
        _ => EmailDigestFrequency.Daily
    };

    private static TimeSpan ParseTimeOfDay(string? timeOfDay)
    {
        if (!string.IsNullOrWhiteSpace(timeOfDay) && TimeSpan.TryParse(timeOfDay, out var parsed))
        {
            return parsed;
        }

        return new TimeSpan(8, 0, 0);
    }

    #endregion
}
