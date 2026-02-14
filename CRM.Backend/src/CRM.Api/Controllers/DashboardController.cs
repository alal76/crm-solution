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
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Dashboard Controller for analytics, metrics, and statistics.
/// Provides comprehensive dashboard data for CRM insights.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<DashboardController> _logger;
    private readonly IDashboardService _dashboardService;

    public DashboardController(CrmDbContext context, ILogger<DashboardController> logger, IDashboardService dashboardService)
    {
        _context = context;
        _logger = logger;
        _dashboardService = dashboardService;
    }

    #region Core Statistics

    /// <summary>
    /// Get comprehensive dashboard statistics.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var customerCount = await _context.Customers.CountAsync(c => !c.IsDeleted, cancellationToken);
            var contactCount = await _context.Contacts.CountAsync(cancellationToken);
            var opportunityCount = await _context.Opportunities.CountAsync(o => !o.IsDeleted, cancellationToken);
            var openOpportunityValue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .SumAsync(o => o.Amount, cancellationToken);
            var wonOpportunityValue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == OpportunityStage.ClosedWon)
                .SumAsync(o => o.Amount, cancellationToken);
            var productCount = await _context.Products.CountAsync(p => !p.IsDeleted, cancellationToken);
            var taskCount = await _context.CrmTasks.CountAsync(t => !t.IsDeleted, cancellationToken);
            var notStartedTaskCount = await _context.CrmTasks.CountAsync(t => !t.IsDeleted && t.Status == CrmTaskStatus.NotStarted, cancellationToken);
            var activeUserCount = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive, cancellationToken);
            var leadCount = await _context.Leads.CountAsync(l => !l.IsDeleted, cancellationToken);

            return Ok(new DashboardStatsResponse
            {
                Customers = new CountStat { Total = customerCount },
                Contacts = new CountStat { Total = contactCount },
                Opportunities = new OpportunityStat
                {
                    Total = opportunityCount,
                    OpenValue = openOpportunityValue,
                    WonValue = wonOpportunityValue
                },
                Products = new CountStat { Total = productCount },
                Tasks = new TaskStat
                {
                    Total = taskCount,
                    Pending = notStartedTaskCount
                },
                Users = new UserStat { Active = activeUserCount },
                Leads = new CountStat { Total = leadCount },
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics" });
        }
    }

    /// <summary>
    /// Stream dashboard statistics using Server-Sent Events (SSE).
    /// </summary>
    [HttpGet("stream")]
    [Produces("text/event-stream")]
    public async Task StreamStats(CancellationToken cancellationToken = default)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers.Append("X-Accel-Buffering", "no");

        while (!cancellationToken.IsCancellationRequested)
        {
            var stats = await _dashboardService.GetStatsAsync();
            var payload = JsonSerializer.Serialize(stats);
            await Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    /// <summary>
    /// Get summary dashboard metrics for quick overview.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfQuarter = new DateTime(now.Year, (((now.Month - 1) / 3) * 3) + 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // MTD Revenue (ClosedWon this month)
            var mtdRevenue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == OpportunityStage.ClosedWon && o.ExpectedCloseDate >= startOfMonth)
                .SumAsync(o => o.Amount, cancellationToken);

            // QTD Revenue
            var qtdRevenue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage == OpportunityStage.ClosedWon && o.ExpectedCloseDate >= startOfQuarter)
                .SumAsync(o => o.Amount, cancellationToken);

            // Pipeline Value
            var pipelineValue = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .SumAsync(o => o.Amount, cancellationToken);

            // New leads this month
            var newLeadsThisMonth = await _context.Leads
                .CountAsync(l => !l.IsDeleted && l.CreatedAt >= startOfMonth, cancellationToken);

            // Deals closed this month
            var dealsClosedThisMonth = await _context.Opportunities
                .CountAsync(o => !o.IsDeleted && o.Stage == OpportunityStage.ClosedWon && o.ExpectedCloseDate >= startOfMonth, cancellationToken);

            // Win rate calculation
            var closedDealsThisMonth = await _context.Opportunities
                .Where(o => !o.IsDeleted && (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost) && o.ExpectedCloseDate >= startOfMonth)
                .ToListAsync(cancellationToken);

            var winRate = closedDealsThisMonth.Count > 0
                ? (double)closedDealsThisMonth.Count(o => o.Stage == OpportunityStage.ClosedWon) / closedDealsThisMonth.Count * 100
                : 0;

            return Ok(new DashboardSummaryResponse
            {
                MtdRevenue = mtdRevenue,
                QtdRevenue = qtdRevenue,
                PipelineValue = pipelineValue,
                NewLeadsThisMonth = newLeadsThisMonth,
                DealsClosedThisMonth = dealsClosedThisMonth,
                WinRate = Math.Round(winRate, 1),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard summary" });
        }
    }

    #endregion

    #region Pipeline & Sales

    /// <summary>
    /// Get pipeline summary statistics by stage.
    /// </summary>
    [HttpGet("pipeline")]
    [ProducesResponseType(typeof(PipelineSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPipelineSummary(CancellationToken cancellationToken = default)
    {
        try
        {
            var pipelineData = await _context.Opportunities
                .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                .GroupBy(o => o.Stage)
                .Select(g => new PipelineStageData
                {
                    Stage = g.Key.ToString(),
                    StageValue = (int)g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.Amount),
                    WeightedValue = g.Sum(o => o.Amount * ((decimal)o.Probability / 100m))
                })
                .OrderBy(p => p.StageValue)
                .ToListAsync(cancellationToken);

            var totalPipeline = pipelineData.Sum(p => p.TotalValue);
            var weightedPipeline = pipelineData.Sum(p => p.WeightedValue);

            return Ok(new PipelineSummaryResponse
            {
                Stages = pipelineData,
                Summary = new PipelineTotals
                {
                    TotalValue = totalPipeline,
                    WeightedValue = weightedPipeline,
                    OpportunityCount = pipelineData.Sum(p => p.Count)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pipeline summary");
            return StatusCode(500, new { message = "An error occurred while retrieving pipeline data" });
        }
    }

    /// <summary>
    /// Get sales forecast data.
    /// </summary>
    [HttpGet("forecast")]
    [ProducesResponseType(typeof(SalesForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalesForecast(
        [FromQuery] int months = 3,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var forecastPeriods = new List<ForecastPeriod>();

            for (int i = 0; i < months; i++)
            {
                var periodStart = new DateTime(now.Year, now.Month, 1).AddMonths(i);
                var periodEnd = periodStart.AddMonths(1);

                var periodOpportunities = await _context.Opportunities
                    .Where(o => !o.IsDeleted
                        && o.Stage != OpportunityStage.ClosedWon
                        && o.Stage != OpportunityStage.ClosedLost
                        && o.ExpectedCloseDate >= periodStart
                        && o.ExpectedCloseDate < periodEnd)
                    .ToListAsync(cancellationToken);

                var totalValue = periodOpportunities.Sum(o => o.Amount);
                var weightedValue = periodOpportunities.Sum(o => o.Amount * (decimal)o.Probability / 100m);

                forecastPeriods.Add(new ForecastPeriod
                {
                    Period = periodStart.ToString("yyyy-MM"),
                    Month = periodStart.ToString("MMMM yyyy"),
                    OpportunityCount = periodOpportunities.Count,
                    TotalValue = totalValue,
                    WeightedValue = weightedValue,
                    BestCase = totalValue,
                    MostLikely = weightedValue,
                    WorstCase = weightedValue * 0.7m
                });
            }

            return Ok(new SalesForecastResponse
            {
                Periods = forecastPeriods,
                TotalForecast = forecastPeriods.Sum(p => p.WeightedValue),
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales forecast");
            return StatusCode(500, new { message = "An error occurred while retrieving sales forecast" });
        }
    }

    /// <summary>
    /// Get deals closing soon.
    /// </summary>
    [HttpGet("deals-closing-soon")]
    [ProducesResponseType(typeof(IEnumerable<DealSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDealsClosingSoon(
        [FromQuery] int days = 30,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = DateTime.UtcNow.AddDays(days);
            var deals = await _context.Opportunities
                .Where(o => !o.IsDeleted
                    && o.Stage != OpportunityStage.ClosedWon
                    && o.Stage != OpportunityStage.ClosedLost
                    && o.ExpectedCloseDate <= targetDate)
                .OrderBy(o => o.ExpectedCloseDate)
                .Take(limit)
                .Select(o => new DealSummary
                {
                    Id = o.Id,
                    Name = o.Name,
                    AccountId = o.AccountId,
                    AccountName = o.Account != null ? o.Account.Company : null,
                    Amount = o.Amount,
                    Stage = o.Stage.ToString(),
                    Probability = o.Probability,
                    ExpectedCloseDate = o.ExpectedCloseDate ?? DateTime.UtcNow,
                    DaysUntilClose = o.ExpectedCloseDate.HasValue ? (int)(o.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays : 0
                })
                .ToListAsync(cancellationToken);

            return Ok(deals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deals closing soon");
            return StatusCode(500, new { message = "An error occurred while retrieving deals closing soon" });
        }
    }

    #endregion

    #region Activities & Tasks

    /// <summary>
    /// Get recent activities.
    /// </summary>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(IEnumerable<ActivitySummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentActivities(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activities = await _context.Activities
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.ActivityDate)
                .Take(count)
                .Select(a => new ActivitySummary
                {
                    Id = a.Id,
                    Type = a.ActivityType.ToString(),
                    Title = a.Title,
                    ActivityDate = a.ActivityDate,
                    Description = a.Description,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId
                })
                .ToListAsync(cancellationToken);

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activities");
            return StatusCode(500, new { message = "An error occurred while retrieving activities" });
        }
    }

    /// <summary>
    /// Get overdue and upcoming tasks.
    /// </summary>
    [HttpGet("tasks")]
    [ProducesResponseType(typeof(TaskDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTasksDashboard(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            var overdueTasks = await _context.CrmTasks
                .Where(t => !t.IsDeleted && t.Status != CrmTaskStatus.Completed && t.DueDate < now)
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .Select(t => new TaskSummary
                {
                    Id = t.Id,
                    Title = t.Subject,
                    DueDate = t.DueDate,
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    AssignedToId = t.AssignedToUserId,
                    EntityType = t.AccountId != null ? "Account" : t.ContactId != null ? "Contact" : t.OpportunityId != null ? "Opportunity" : null,
                    EntityId = t.AccountId ?? t.ContactId ?? t.OpportunityId
                })
                .ToListAsync(cancellationToken);

            var upcomingTasks = await _context.CrmTasks
                .Where(t => !t.IsDeleted && t.Status != CrmTaskStatus.Completed && t.DueDate >= now && t.DueDate <= now.AddDays(7))
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .Select(t => new TaskSummary
                {
                    Id = t.Id,
                    Title = t.Subject,
                    DueDate = t.DueDate,
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    AssignedToId = t.AssignedToUserId,
                    EntityType = t.AccountId != null ? "Account" : t.ContactId != null ? "Contact" : t.OpportunityId != null ? "Opportunity" : null,
                    EntityId = t.AccountId ?? t.ContactId ?? t.OpportunityId
                })
                .ToListAsync(cancellationToken);

            var taskStats = new TaskStatsDetail
            {
                Total = await _context.CrmTasks.CountAsync(t => !t.IsDeleted, cancellationToken),
                Completed = await _context.CrmTasks.CountAsync(t => !t.IsDeleted && t.Status == CrmTaskStatus.Completed, cancellationToken),
                InProgress = await _context.CrmTasks.CountAsync(t => !t.IsDeleted && t.Status == CrmTaskStatus.InProgress, cancellationToken),
                NotStarted = await _context.CrmTasks.CountAsync(t => !t.IsDeleted && t.Status == CrmTaskStatus.NotStarted, cancellationToken),
                Overdue = await _context.CrmTasks.CountAsync(t => !t.IsDeleted && t.Status != CrmTaskStatus.Completed && t.DueDate < now, cancellationToken)
            };

            return Ok(new TaskDashboardResponse
            {
                OverdueTasks = overdueTasks,
                UpcomingTasks = upcomingTasks,
                Stats = taskStats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks dashboard");
            return StatusCode(500, new { message = "An error occurred while retrieving tasks dashboard" });
        }
    }

    #endregion

    #region Leaderboards & Performance

    /// <summary>
    /// Get sales leaderboard.
    /// </summary>
    [HttpGet("leaderboard/sales")]
    [ProducesResponseType(typeof(IEnumerable<SalesLeaderboardEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalesLeaderboard(
        [FromQuery] int topN = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = toDate ?? DateTime.UtcNow;

            var leaderboard = await _context.Opportunities
                .Where(o => !o.IsDeleted
                    && o.Stage == OpportunityStage.ClosedWon
                    && o.ExpectedCloseDate >= from
                    && o.ExpectedCloseDate <= to
                    && o.SalesOwnerId != null)
                .GroupBy(o => o.SalesOwnerId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalRevenue = g.Sum(o => o.Amount),
                    DealsWon = g.Count(),
                    AverageDealSize = g.Average(o => o.Amount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(topN)
                .ToListAsync(cancellationToken);

            // Get user details
            var userIds = leaderboard.Select(l => l.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.FirstName, u.LastName }, cancellationToken);

            var result = leaderboard.Select((l, index) => new SalesLeaderboardEntry
            {
                Rank = index + 1,
                UserId = l.UserId ?? 0,
                UserName = users.ContainsKey(l.UserId ?? 0)
                    ? $"{users[l.UserId ?? 0].FirstName} {users[l.UserId ?? 0].LastName}"
                    : "Unknown",
                TotalRevenue = l.TotalRevenue,
                DealsWon = l.DealsWon,
                AverageDealSize = l.AverageDealSize
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales leaderboard");
            return StatusCode(500, new { message = "An error occurred while retrieving sales leaderboard" });
        }
    }

    /// <summary>
    /// Get activity leaderboard.
    /// </summary>
    [HttpGet("leaderboard/activities")]
    [ProducesResponseType(typeof(IEnumerable<ActivityLeaderboardEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityLeaderboard(
        [FromQuery] int topN = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var leaderboard = await _context.Activities
                .Where(a => !a.IsDeleted && a.ActivityDate >= from && a.ActivityDate <= to && a.UserId != null)
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalActivities = g.Count(),
                    Calls = g.Count(a => a.ActivityType == ActivityType.CallMade || a.ActivityType == ActivityType.CallReceived),
                    Emails = g.Count(a => a.ActivityType == ActivityType.EmailSent || a.ActivityType == ActivityType.EmailReceived),
                    Meetings = g.Count(a => a.ActivityType == ActivityType.MeetingScheduled || a.ActivityType == ActivityType.MeetingCompleted)
                })
                .OrderByDescending(x => x.TotalActivities)
                .Take(topN)
                .ToListAsync(cancellationToken);

            var userIds = leaderboard.Select(l => l.UserId).Where(id => id.HasValue).Select(id => id!.Value).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.FirstName, u.LastName }, cancellationToken);

            var result = leaderboard.Select((l, index) => new ActivityLeaderboardEntry
            {
                Rank = index + 1,
                UserId = l.UserId ?? 0,
                UserName = l.UserId.HasValue && users.ContainsKey(l.UserId.Value)
                    ? $"{users[l.UserId.Value].FirstName} {users[l.UserId.Value].LastName}"
                    : "Unknown",
                TotalActivities = l.TotalActivities,
                Calls = l.Calls,
                Emails = l.Emails,
                Meetings = l.Meetings
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity leaderboard");
            return StatusCode(500, new { message = "An error occurred while retrieving activity leaderboard" });
        }
    }

    #endregion

    #region Trends & Analytics

    /// <summary>
    /// Get revenue trends over time.
    /// </summary>
    [HttpGet("trends/revenue")]
    [ProducesResponseType(typeof(RevenueTrendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRevenueTrends(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var trends = new List<RevenueTrendPoint>();
            var now = DateTime.UtcNow;

            for (int i = months - 1; i >= 0; i--)
            {
                var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                var periodEnd = periodStart.AddMonths(1);

                var revenue = await _context.Opportunities
                    .Where(o => !o.IsDeleted
                        && o.Stage == OpportunityStage.ClosedWon
                        && o.ExpectedCloseDate >= periodStart
                        && o.ExpectedCloseDate < periodEnd)
                    .SumAsync(o => o.Amount, cancellationToken);

                var dealCount = await _context.Opportunities
                    .CountAsync(o => !o.IsDeleted
                        && o.Stage == OpportunityStage.ClosedWon
                        && o.ExpectedCloseDate >= periodStart
                        && o.ExpectedCloseDate < periodEnd, cancellationToken);

                trends.Add(new RevenueTrendPoint
                {
                    Period = periodStart.ToString("yyyy-MM"),
                    Month = periodStart.ToString("MMM yyyy"),
                    Revenue = revenue,
                    DealCount = dealCount
                });
            }

            return Ok(new RevenueTrendResponse
            {
                Trends = trends,
                TotalRevenue = trends.Sum(t => t.Revenue),
                TotalDeals = trends.Sum(t => t.DealCount),
                AverageMonthlyRevenue = trends.Count > 0 ? trends.Average(t => t.Revenue) : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revenue trends");
            return StatusCode(500, new { message = "An error occurred while retrieving revenue trends" });
        }
    }

    /// <summary>
    /// Get lead conversion funnel.
    /// </summary>
    [HttpGet("funnel/leads")]
    [ProducesResponseType(typeof(LeadFunnelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeadFunnel(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
            var to = toDate ?? DateTime.UtcNow;

            var totalLeads = await _context.Leads
                .CountAsync(l => !l.IsDeleted && l.CreatedAt >= from && l.CreatedAt <= to, cancellationToken);

            var qualifiedLeads = await _context.Leads
                .CountAsync(l => !l.IsDeleted && l.CreatedAt >= from && l.CreatedAt <= to && l.Status == LeadLifecycleStatus.Qualified, cancellationToken);

            var convertedLeads = await _context.Leads
                .CountAsync(l => !l.IsDeleted && l.CreatedAt >= from && l.CreatedAt <= to && l.Status == LeadLifecycleStatus.Converted, cancellationToken);

            var opportunitiesFromLeads = await _context.Opportunities
                .CountAsync(o => !o.IsDeleted && o.LeadId != null && o.CreatedAt >= from && o.CreatedAt <= to, cancellationToken);

            var closedWonFromLeads = await _context.Opportunities
                .CountAsync(o => !o.IsDeleted && o.LeadId != null && o.Stage == OpportunityStage.ClosedWon && o.ExpectedCloseDate >= from && o.ExpectedCloseDate <= to, cancellationToken);

            return Ok(new LeadFunnelResponse
            {
                TotalLeads = totalLeads,
                QualifiedLeads = qualifiedLeads,
                ConvertedLeads = convertedLeads,
                Opportunities = opportunitiesFromLeads,
                ClosedWon = closedWonFromLeads,
                QualificationRate = totalLeads > 0 ? Math.Round((double)qualifiedLeads / totalLeads * 100, 1) : 0,
                ConversionRate = totalLeads > 0 ? Math.Round((double)convertedLeads / totalLeads * 100, 1) : 0,
                WinRate = opportunitiesFromLeads > 0 ? Math.Round((double)closedWonFromLeads / opportunitiesFromLeads * 100, 1) : 0,
                FromDate = from,
                ToDate = to
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead funnel");
            return StatusCode(500, new { message = "An error occurred while retrieving lead funnel" });
        }
    }

    /// <summary>
    /// Get win/loss analysis.
    /// </summary>
    [HttpGet("analysis/win-loss")]
    [ProducesResponseType(typeof(WinLossAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWinLossAnalysis(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddMonths(-6);
            var to = toDate ?? DateTime.UtcNow;

            var closedOpportunities = await _context.Opportunities
                .Where(o => !o.IsDeleted
                    && (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost)
                    && o.ExpectedCloseDate >= from
                    && o.ExpectedCloseDate <= to)
                .ToListAsync(cancellationToken);

            var won = closedOpportunities.Where(o => o.Stage == OpportunityStage.ClosedWon).ToList();
            var lost = closedOpportunities.Where(o => o.Stage == OpportunityStage.ClosedLost).ToList();

            return Ok(new WinLossAnalysisResponse
            {
                TotalClosed = closedOpportunities.Count,
                WonCount = won.Count,
                LostCount = lost.Count,
                WonValue = won.Sum(o => o.Amount),
                LostValue = lost.Sum(o => o.Amount),
                WinRate = closedOpportunities.Count > 0 ? Math.Round((double)won.Count / closedOpportunities.Count * 100, 1) : 0,
                AverageWonDealSize = won.Count > 0 ? won.Average(o => o.Amount) : 0,
                AverageLostDealSize = lost.Count > 0 ? lost.Average(o => o.Amount) : 0,
                AverageWonCycleTime = won.Count > 0 ? Math.Round(won.Where(o => o.ExpectedCloseDate.HasValue).Average(o => (o.ExpectedCloseDate!.Value - o.CreatedAt).TotalDays), 1) : 0,
                AverageLostCycleTime = lost.Count > 0 ? Math.Round(lost.Where(o => o.ExpectedCloseDate.HasValue).Average(o => (o.ExpectedCloseDate!.Value - o.CreatedAt).TotalDays), 1) : 0,
                FromDate = from,
                ToDate = to
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving win/loss analysis");
            return StatusCode(500, new { message = "An error occurred while retrieving win/loss analysis" });
        }
    }

    #endregion

    #region Customer Insights

    /// <summary>
    /// Get top customers by revenue.
    /// </summary>
    [HttpGet("customers/top")]
    [ProducesResponseType(typeof(IEnumerable<TopCustomerEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTopCustomers(
        [FromQuery] int topN = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = toDate ?? DateTime.UtcNow;

            var topCustomers = await _context.Opportunities
                .Where(o => !o.IsDeleted
                    && o.Stage == OpportunityStage.ClosedWon
                    && o.ExpectedCloseDate >= from
                    && o.ExpectedCloseDate <= to
                    && o.AccountId > 0)
                .GroupBy(o => o.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalRevenue = g.Sum(o => o.Amount),
                    DealCount = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(topN)
                .ToListAsync(cancellationToken);

            var accountIds = topCustomers.Select(c => c.AccountId).ToList();
            var accounts = await _context.Customers
                .Where(c => accountIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => new { c.Company, c.Industry }, cancellationToken);

            var result = topCustomers.Select((c, index) => new TopCustomerEntry
            {
                Rank = index + 1,
                AccountId = c.AccountId,
                AccountName = accounts.ContainsKey(c.AccountId)
                    ? accounts[c.AccountId].Company ?? "Unknown"
                    : "Unknown",
                Industry = accounts.ContainsKey(c.AccountId)
                    ? accounts[c.AccountId].Industry
                    : null,
                TotalRevenue = c.TotalRevenue,
                DealCount = c.DealCount
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top customers");
            return StatusCode(500, new { message = "An error occurred while retrieving top customers" });
        }
    }

    /// <summary>
    /// Get customer acquisition metrics.
    /// </summary>
    [HttpGet("customers/acquisition")]
    [ProducesResponseType(typeof(CustomerAcquisitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerAcquisition(
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var trends = new List<AcquisitionTrendPoint>();
            var now = DateTime.UtcNow;

            for (int i = months - 1; i >= 0; i--)
            {
                var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                var periodEnd = periodStart.AddMonths(1);

                var newCustomers = await _context.Customers
                    .CountAsync(c => !c.IsDeleted && c.CreatedAt >= periodStart && c.CreatedAt < periodEnd, cancellationToken);

                var newLeads = await _context.Leads
                    .CountAsync(l => !l.IsDeleted && l.CreatedAt >= periodStart && l.CreatedAt < periodEnd, cancellationToken);

                trends.Add(new AcquisitionTrendPoint
                {
                    Period = periodStart.ToString("yyyy-MM"),
                    Month = periodStart.ToString("MMM yyyy"),
                    NewCustomers = newCustomers,
                    NewLeads = newLeads
                });
            }

            return Ok(new CustomerAcquisitionResponse
            {
                Trends = trends,
                TotalNewCustomers = trends.Sum(t => t.NewCustomers),
                TotalNewLeads = trends.Sum(t => t.NewLeads),
                AverageMonthlyCustomers = trends.Count > 0 ? Math.Round(trends.Average(t => (double)t.NewCustomers), 1) : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer acquisition data");
            return StatusCode(500, new { message = "An error occurred while retrieving customer acquisition data" });
        }
    }

    #endregion

    #region Widgets Configuration

    /// <summary>
    /// Get available dashboard widgets.
    /// </summary>
    [HttpGet("widgets")]
    [ProducesResponseType(typeof(IEnumerable<DashboardWidget>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAvailableWidgets()
    {
        var widgets = new List<DashboardWidget>
        {
            new() { Id = "stats", Name = "Key Statistics", Description = "Overview of key CRM metrics", Category = "Overview", DefaultSize = "large" },
            new() { Id = "summary", Name = "Dashboard Summary", Description = "Quick summary of MTD/QTD performance", Category = "Overview", DefaultSize = "medium" },
            new() { Id = "pipeline", Name = "Pipeline Summary", Description = "Sales pipeline by stage", Category = "Sales", DefaultSize = "large" },
            new() { Id = "forecast", Name = "Sales Forecast", Description = "Projected revenue for upcoming periods", Category = "Sales", DefaultSize = "medium" },
            new() { Id = "deals-closing", Name = "Deals Closing Soon", Description = "Opportunities closing within selected period", Category = "Sales", DefaultSize = "medium" },
            new() { Id = "activities", Name = "Recent Activities", Description = "Latest activities across CRM", Category = "Activity", DefaultSize = "medium" },
            new() { Id = "tasks", Name = "Tasks Dashboard", Description = "Overdue and upcoming tasks", Category = "Activity", DefaultSize = "medium" },
            new() { Id = "sales-leaderboard", Name = "Sales Leaderboard", Description = "Top performers by revenue", Category = "Leaderboard", DefaultSize = "medium" },
            new() { Id = "activity-leaderboard", Name = "Activity Leaderboard", Description = "Top performers by activity count", Category = "Leaderboard", DefaultSize = "medium" },
            new() { Id = "revenue-trends", Name = "Revenue Trends", Description = "Monthly revenue over time", Category = "Trends", DefaultSize = "large" },
            new() { Id = "lead-funnel", Name = "Lead Funnel", Description = "Lead conversion funnel analysis", Category = "Analytics", DefaultSize = "medium" },
            new() { Id = "win-loss", Name = "Win/Loss Analysis", Description = "Deal win/loss breakdown", Category = "Analytics", DefaultSize = "medium" },
            new() { Id = "top-customers", Name = "Top Customers", Description = "Highest revenue customers", Category = "Customers", DefaultSize = "medium" },
            new() { Id = "customer-acquisition", Name = "Customer Acquisition", Description = "New customer and lead trends", Category = "Customers", DefaultSize = "medium" }
        };

        return Ok(widgets);
    }

    #endregion

    #region Response DTOs

    /// <summary>Dashboard statistics response.</summary>
    public class DashboardStatsResponse
    {
        public CountStat Customers { get; set; } = new();
        public CountStat Contacts { get; set; } = new();
        public OpportunityStat Opportunities { get; set; } = new();
        public CountStat Products { get; set; } = new();
        public TaskStat Tasks { get; set; } = new();
        public UserStat Users { get; set; } = new();
        public CountStat Leads { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class CountStat
    {
        public int Total { get; set; }
    }
    public class OpportunityStat
    {
        public int Total { get; set; }
        public decimal OpenValue { get; set; }
        public decimal WonValue { get; set; }
    }
    public class TaskStat
    {
        public int Total { get; set; }
        public int Pending { get; set; }
    }
    public class UserStat
    {
        public int Active { get; set; }
    }

    public class DashboardSummaryResponse
    {
        public decimal MtdRevenue { get; set; }
        public decimal QtdRevenue { get; set; }
        public decimal PipelineValue { get; set; }
        public int NewLeadsThisMonth { get; set; }
        public int DealsClosedThisMonth { get; set; }
        public double WinRate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PipelineSummaryResponse
    {
        public List<PipelineStageData> Stages { get; set; } = new();
        public PipelineTotals Summary { get; set; } = new();
    }

    public class PipelineStageData
    {
        public string Stage { get; set; } = string.Empty;
        public int StageValue { get; set; }
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal WeightedValue { get; set; }
    }

    public class PipelineTotals
    {
        public decimal TotalValue { get; set; }
        public decimal WeightedValue { get; set; }
        public int OpportunityCount { get; set; }
    }

    public class SalesForecastResponse
    {
        public List<ForecastPeriod> Periods { get; set; } = new();
        public decimal TotalForecast { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ForecastPeriod
    {
        public string Period { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public int OpportunityCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal WeightedValue { get; set; }
        public decimal BestCase { get; set; }
        public decimal MostLikely { get; set; }
        public decimal WorstCase { get; set; }
    }

    public class DealSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? AccountId { get; set; }
        public string? AccountName { get; set; }
        public decimal Amount { get; set; }
        public string Stage { get; set; } = string.Empty;
        public int Probability { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public int DaysUntilClose { get; set; }
    }

    public class ActivitySummary
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Title { get; set; }
        public DateTime ActivityDate { get; set; }
        public string? Description { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
    }

    public class TaskDashboardResponse
    {
        public List<TaskSummary> OverdueTasks { get; set; } = new();
        public List<TaskSummary> UpcomingTasks { get; set; } = new();
        public TaskStatsDetail Stats { get; set; } = new();
    }

    public class TaskSummary
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? AssignedToId { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
    }

    public class TaskStatsDetail
    {
        public int Total { get; set; }
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int NotStarted { get; set; }
        public int Overdue { get; set; }
    }

    public class SalesLeaderboardEntry
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int DealsWon { get; set; }
        public decimal AverageDealSize { get; set; }
    }

    public class ActivityLeaderboardEntry
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalActivities { get; set; }
        public int Calls { get; set; }
        public int Emails { get; set; }
        public int Meetings { get; set; }
    }

    public class RevenueTrendResponse
    {
        public List<RevenueTrendPoint> Trends { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalDeals { get; set; }
        public decimal AverageMonthlyRevenue { get; set; }
    }

    public class RevenueTrendPoint
    {
        public string Period { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int DealCount { get; set; }
    }

    public class LeadFunnelResponse
    {
        public int TotalLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public int Opportunities { get; set; }
        public int ClosedWon { get; set; }
        public double QualificationRate { get; set; }
        public double ConversionRate { get; set; }
        public double WinRate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class WinLossAnalysisResponse
    {
        public int TotalClosed { get; set; }
        public int WonCount { get; set; }
        public int LostCount { get; set; }
        public decimal WonValue { get; set; }
        public decimal LostValue { get; set; }
        public double WinRate { get; set; }
        public decimal AverageWonDealSize { get; set; }
        public decimal AverageLostDealSize { get; set; }
        public double AverageWonCycleTime { get; set; }
        public double AverageLostCycleTime { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class TopCustomerEntry
    {
        public int Rank { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public decimal TotalRevenue { get; set; }
        public int DealCount { get; set; }
    }

    public class CustomerAcquisitionResponse
    {
        public List<AcquisitionTrendPoint> Trends { get; set; } = new();
        public int TotalNewCustomers { get; set; }
        public int TotalNewLeads { get; set; }
        public double AverageMonthlyCustomers { get; set; }
    }

    public class AcquisitionTrendPoint
    {
        public string Period { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public int NewCustomers { get; set; }
        public int NewLeads { get; set; }
    }

    public class DashboardWidget
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DefaultSize { get; set; } = "medium";
    }

    #endregion
}
