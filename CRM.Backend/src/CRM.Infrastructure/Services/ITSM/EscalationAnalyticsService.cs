// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Core.Dtos.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for escalation analytics and reporting.
/// Provides methods for analyzing escalation patterns, trends, and performance metrics.
/// TODO-SD005-011: Create escalation analytics reports.
/// </summary>
public class EscalationAnalyticsService : IEscalationAnalyticsService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<EscalationAnalyticsService> _logger;

    public EscalationAnalyticsService(
        ICrmDbContext dbContext,
        ILogger<EscalationAnalyticsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<EscalationByCategoryDto>> GetEscalationsByCategoryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var escalationLogs = await _dbContext.Set<EscalationLog>()
                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate)
                .Include(e => e.ServiceRequest)
                .ThenInclude(sr => sr!.Category)
                .ToListAsync(cancellationToken);

            var totalCount = escalationLogs.Count;
            if (totalCount == 0)
            {
                return Array.Empty<EscalationByCategoryDto>();
            }

            var grouped = escalationLogs
                .GroupBy(e => new
                {
                    CategoryId = e.ServiceRequest?.CategoryId ?? 0,
                    CategoryName = e.ServiceRequest?.Category?.Name ?? "Uncategorized"
                })
                .Select(g => new EscalationByCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    EscalationCount = g.Count(),
                    PercentageOfTotal = Math.Round((double)g.Count() / totalCount * 100, 2)
                })
                .OrderByDescending(c => c.EscalationCount)
                .ToList();

            return grouped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalations by category");
            throw;
        }
    }

    public async Task<AverageEscalationTimeDto> GetAverageEscalationTimeAsync(
        DateTime startDate,
        DateTime endDate,
        string? priority = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Set<EscalationLog>()
                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate)
                .Include(e => e.ServiceRequest)
                .AsQueryable();

            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(e => e.ServiceRequest != null &&
                    e.ServiceRequest.Priority.ToString() == priority);
            }

            var logs = await query.ToListAsync(cancellationToken);

            if (logs.Count == 0)
            {
                return new AverageEscalationTimeDto
                {
                    TotalEscalations = 0,
                    AverageMinutesToFirstEscalation = 0,
                    AverageMinutesToResolution = 0,
                    MedianMinutesToFirstEscalation = 0
                };
            }

            var timeToFirstEscalation = logs
                .Where(e => e.ServiceRequest != null)
                .Select(e => (e.EscalatedAt - e.ServiceRequest!.CreatedAt).TotalMinutes)
                .ToList();

            var sortedTimes = timeToFirstEscalation.OrderBy(t => t).ToList();
            var median = sortedTimes.Count % 2 == 0
                ? (sortedTimes[sortedTimes.Count / 2 - 1] + sortedTimes[sortedTimes.Count / 2]) / 2
                : sortedTimes[sortedTimes.Count / 2];

            var byLevel = logs
                .GroupBy(e => e.LevelNumber)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(e => e.ServiceRequest != null
                        ? (e.EscalatedAt - e.ServiceRequest.CreatedAt).TotalMinutes
                        : 0));

            return new AverageEscalationTimeDto
            {
                TotalEscalations = logs.Count,
                AverageMinutesToFirstEscalation = Math.Round(timeToFirstEscalation.Average(), 2),
                MedianMinutesToFirstEscalation = Math.Round(median, 2),
                AverageMinutesToResolution = 0, // Would need resolution data
                AverageByEscalationLevel = byLevel
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average escalation time");
            throw;
        }
    }

    public async Task<IEnumerable<EscalationTrendDto>> GetEscalationTrendsAsync(
        DateTime startDate,
        DateTime endDate,
        TrendGranularity granularity = TrendGranularity.Daily,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var escalations = await _dbContext.Set<EscalationLog>()
                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate)
                .ToListAsync(cancellationToken);

            var serviceRequests = await _dbContext.ServiceRequests
                .Where(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted)
                .ToListAsync(cancellationToken);

            var trends = new List<EscalationTrendDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate)
            {
                DateTime periodEnd;
                switch (granularity)
                {
                    case TrendGranularity.Weekly:
                        periodEnd = currentDate.AddDays(7);
                        break;
                    case TrendGranularity.Monthly:
                        periodEnd = currentDate.AddMonths(1);
                        break;
                    default: // Daily
                        periodEnd = currentDate.AddDays(1);
                        break;
                }

                if (periodEnd > endDate)
                {
                    periodEnd = endDate.AddDays(1);
                }

                var periodEscalations = escalations
                    .Count(e => e.EscalatedAt >= currentDate && e.EscalatedAt < periodEnd);

                var periodRequests = serviceRequests
                    .Count(sr => sr.CreatedAt >= currentDate && sr.CreatedAt < periodEnd);

                trends.Add(new EscalationTrendDto
                {
                    PeriodStart = currentDate,
                    PeriodEnd = periodEnd.AddSeconds(-1),
                    EscalationCount = periodEscalations,
                    TotalServiceRequests = periodRequests,
                    EscalationRate = periodRequests > 0
                        ? Math.Round((double)periodEscalations / periodRequests * 100, 2)
                        : 0
                });

                currentDate = periodEnd;
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation trends");
            throw;
        }
    }

    public async Task<IEnumerable<EscalationByPriorityDto>> GetEscalationsByPriorityAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _dbContext.Set<EscalationLog>()
                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate)
                .Include(e => e.ServiceRequest)
                .ToListAsync(cancellationToken);

            var grouped = logs
                .Where(e => e.ServiceRequest != null)
                .GroupBy(e => e.ServiceRequest!.Priority.ToString())
                .Select(g => new EscalationByPriorityDto
                {
                    Priority = g.Key,
                    EscalationCount = g.Count(),
                    AverageTimeToEscalationMinutes = Math.Round(
                        g.Average(e => (e.EscalatedAt - e.ServiceRequest!.CreatedAt).TotalMinutes), 2)
                })
                .OrderByDescending(p => p.EscalationCount)
                .ToList();

            return grouped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalations by priority");
            throw;
        }
    }

    public async Task<IEnumerable<TopEscalatingAssigneeDto>> GetTopEscalatingAssigneesAsync(
        DateTime startDate,
        DateTime endDate,
        int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _dbContext.Set<EscalationLog>()
                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate)
                .Include(e => e.ServiceRequest)
                .ThenInclude(sr => sr!.AssignedToUser)
                .ToListAsync(cancellationToken);

            var assigneeTotals = await _dbContext.ServiceRequests
                .Where(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted)
                .Where(sr => sr.AssignedToUserId != null)
                .GroupBy(sr => sr.AssignedToUserId)
                .Select(g => new { AssigneeId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var grouped = logs
                .Where(e => e.ServiceRequest?.AssignedToUserId != null)
                .GroupBy(e => new
                {
                    AssigneeId = e.ServiceRequest!.AssignedToUserId!.Value,
                    AssigneeName = e.ServiceRequest.AssignedToUser?.Username ?? "Unknown"
                })
                .Select(g =>
                {
                    var totalAssigned = assigneeTotals
                        .FirstOrDefault(a => a.AssigneeId == g.Key.AssigneeId)?.Count ?? 0;

                    return new TopEscalatingAssigneeDto
                    {
                        AssigneeId = g.Key.AssigneeId,
                        AssigneeName = g.Key.AssigneeName,
                        EscalationCount = g.Count(),
                        TotalAssignedRequests = totalAssigned,
                        EscalationRate = totalAssigned > 0
                            ? Math.Round((double)g.Count() / totalAssigned * 100, 2)
                            : 0
                    };
                })
                .OrderByDescending(a => a.EscalationCount)
                .Take(topCount)
                .ToList();

            return grouped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top escalating assignees");
            throw;
        }
    }

    public async Task<EscalationDashboardDto> GetEscalationDashboardAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var byCategory = await GetEscalationsByCategoryAsync(startDate, endDate, cancellationToken);
            var byPriority = await GetEscalationsByPriorityAsync(startDate, endDate, cancellationToken);
            var trends = await GetEscalationTrendsAsync(startDate, endDate, TrendGranularity.Daily, cancellationToken);
            var topAssignees = await GetTopEscalatingAssigneesAsync(startDate, endDate, 10, cancellationToken);
            var avgTime = await GetAverageEscalationTimeAsync(startDate, endDate, null, cancellationToken);

            var totalRequests = await _dbContext.ServiceRequests
                .CountAsync(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted, cancellationToken);

            return new EscalationDashboardDto
            {
                TotalEscalations = avgTime.TotalEscalations,
                TotalServiceRequests = totalRequests,
                OverallEscalationRate = totalRequests > 0
                    ? Math.Round((double)avgTime.TotalEscalations / totalRequests * 100, 2)
                    : 0,
                AverageTimeToFirstEscalation = avgTime.AverageMinutesToFirstEscalation,
                ByCategory = byCategory,
                ByPriority = byPriority,
                Trend = trends,
                TopAssignees = topAssignees
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting escalation dashboard");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EscalationAnalyticsSummaryDto> GetAnalyticsSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30);

            // --- Raw escalation logs in the window ---
            var logs = await _dbContext.Set<EscalationLog>()
                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate)
                .Include(e => e.ServiceRequest)
                    .ThenInclude(sr => sr!.Category)
                .ToListAsync(cancellationToken);

            // --- Total service requests ---
            var totalRequests = await _dbContext.ServiceRequests
                .CountAsync(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted, cancellationToken);

            // --- Avg time-to-escalate by severity ---
            var bySeverity = logs
                .Where(e => e.ServiceRequest != null)
                .GroupBy(e => e.ServiceRequest!.Priority.ToString())
                .Select(g => new EscalationTimeBySeverityDto
                {
                    Priority = g.Key,
                    EscalationCount = g.Count(),
                    AverageMinutesToEscalate = Math.Round(
                        g.Average(e => (e.EscalatedAt - e.ServiceRequest!.CreatedAt).TotalMinutes), 2)
                })
                .OrderByDescending(x => x.EscalationCount)
                .ToList();

            // --- Escalation rate by category ---
            var categoryRequests = await _dbContext.ServiceRequests
                .Where(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted && sr.CategoryId != null)
                .Include(sr => sr.Category)
                .ToListAsync(cancellationToken);

            var escalationsByCategory = logs
                .Where(e => e.ServiceRequest?.CategoryId != null)
                .GroupBy(e => e.ServiceRequest!.CategoryId ?? 0)
                .ToDictionary(g => g.Key, g => g.Count());

            var categoryGroups = categoryRequests
                .GroupBy(sr => new { sr.CategoryId, CategoryName = sr.Category != null ? sr.Category.Name : "Uncategorized" });

            var byCategory = categoryGroups
                .Select(g =>
                {
                    var escaped = escalationsByCategory.TryGetValue(g.Key.CategoryId ?? 0, out var ec) ? ec : 0;
                    return new EscalationRateByCategoryDto
                    {
                        CategoryId = g.Key.CategoryId ?? 0,
                        CategoryName = g.Key.CategoryName,
                        TotalRequests = g.Count(),
                        EscalatedRequests = escaped,
                        EscalationRate = Math.Round((double)escaped / g.Count() * 100, 2)
                    };
                })
                .OrderByDescending(x => x.EscalatedRequests)
                .ToList();

            // --- Top 5 most-escalated request types (by category) ---
            var top5 = logs
                .Where(e => e.ServiceRequest != null)
                .GroupBy(e => new
                {
                    CategoryId = e.ServiceRequest!.CategoryId ?? 0,
                    CategoryName = e.ServiceRequest.Category?.Name ?? "Uncategorized"
                })
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select((g, i) => new TopEscalatedRequestTypeDto
                {
                    Rank = i + 1,
                    CategoryName = g.Key.CategoryName,
                    EscalationCount = g.Count(),
                    PercentageOfTotal = logs.Count > 0
                        ? Math.Round((double)g.Count() / logs.Count * 100, 2)
                        : 0
                })
                .ToList();

            // --- Resolution rate after escalation ---
            var escalatedSrIds = logs.Select(e => e.ServiceRequestId).Distinct().ToList();
            double resolutionRate = 0;
            if (escalatedSrIds.Count > 0)
            {
                var resolvedCount = await _dbContext.ServiceRequests
                    .CountAsync(sr =>
                        escalatedSrIds.Contains(sr.Id) &&
                        (sr.Status == ServiceRequestStatus.Resolved || sr.Status == ServiceRequestStatus.Closed),
                        cancellationToken);

                resolutionRate = Math.Round((double)resolvedCount / escalatedSrIds.Count * 100, 2);
            }

            var summary = new EscalationAnalyticsSummaryDto
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalEscalations = logs.Count,
                TotalServiceRequests = totalRequests,
                OverallEscalationRate = totalRequests > 0
                    ? Math.Round((double)logs.Count / totalRequests * 100, 2)
                    : 0,
                AverageTimeToEscalateBySeverity = bySeverity,
                EscalationRateByCategory = byCategory,
                TopEscalatedRequestTypes = top5,
                ResolutionRateAfterEscalation = resolutionRate,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Escalation analytics summary generated: {Total} escalations / {TotalSR} SRs in last 30 days",
                summary.TotalEscalations, summary.TotalServiceRequests);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating escalation analytics summary");
            throw;
        }
    }
}
