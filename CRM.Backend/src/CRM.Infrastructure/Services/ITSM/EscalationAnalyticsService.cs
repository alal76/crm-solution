// CRM Solution - Customer Relationship Management System// CRM Solution - Customer Relationship Management System













































































































































































































































































































}    }        };            TopAssignees = topAssignees            Trend = trend,            ByPriority = byPriority,            ByCategory = byCategory,            AverageTimeToFirstEscalation = avgTime.AverageMinutesToFirstEscalation,                : 0,                ? Math.Round((double)avgTime.TotalEscalations / totalRequests * 100, 2)            OverallEscalationRate = totalRequests > 0            TotalServiceRequests = totalRequests,            TotalEscalations = avgTime.TotalEscalations,        {        return new EscalationDashboardDto            .CountAsync(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted, cancellationToken);        var totalRequests = await _dbContext.ServiceRequests        var avgTime = await GetAverageEscalationTimeAsync(startDate, endDate, null, cancellationToken);        var topAssignees = await GetTopEscalatingAssigneesAsync(startDate, endDate, 5, cancellationToken);        var trend = await GetEscalationTrendsAsync(startDate, endDate, TrendGranularity.Daily, cancellationToken);        var byPriority = await GetEscalationsByPriorityAsync(startDate, endDate, cancellationToken);        var byCategory = await GetEscalationsByCategoryAsync(startDate, endDate, cancellationToken);    {        CancellationToken cancellationToken = default)        DateTime endDate,        DateTime startDate,    public async Task<EscalationDashboardDto> GetEscalationDashboardAsync(    }        }            return Array.Empty<TopEscalatingAssigneeDto>();            _logger.LogError(ex, "Error getting top escalating assignees");        {        catch (Exception ex)        }            });                    : 0                    ? Math.Round((double)a.EscalationCount / total * 100, 2)                EscalationRate = totalAssigned.TryGetValue(a.AssigneeId, out var total) && total > 0                TotalAssignedRequests = totalAssigned.GetValueOrDefault(a.AssigneeId, 0),                EscalationCount = a.EscalationCount,                AssigneeName = a.AssigneeName,                AssigneeId = a.AssigneeId,            {            return assigneeEscalations.Select(a => new TopEscalatingAssigneeDto                .ToDictionaryAsync(x => x.AssigneeId, x => x.Count, cancellationToken);                .Select(g => new { AssigneeId = g.Key, Count = g.Count() })                .GroupBy(sr => sr.AssignedToId!.Value)                             !sr.IsDeleted)                             sr.CreatedAt <= endDate &&                             sr.CreatedAt >= startDate &&                             assigneeIds.Contains(sr.AssignedToId.Value) &&                .Where(sr => sr.AssignedToId.HasValue &&            var totalAssigned = await _dbContext.ServiceRequests            var assigneeIds = assigneeEscalations.Select(a => a.AssigneeId).ToList();            // Get total assigned requests for each assignee                .ToListAsync(cancellationToken);                .Take(topCount)                .OrderByDescending(x => x.EscalationCount)                })                    EscalationCount = g.Count()                    AssigneeName = g.Key.Name,                    AssigneeId = g.Key.AssignedToId!.Value,                {                .Select(g => new                .GroupBy(x => new { x.AssignedToId, Name = x.AssignedTo != null ? x.AssignedTo.Username : "Unknown" })                .Where(x => x.AssignedToId.HasValue)                    (e, sr) => new { sr.AssignedToId, sr.AssignedTo })                    sr => sr.Id,                    e => e.ServiceRequestId,                    _dbContext.ServiceRequests.Where(sr => !sr.IsDeleted && sr.AssignedToId.HasValue),                .Join(                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate && !e.IsDeleted)            var assigneeEscalations = await _dbContext.Set<CRM.Core.Entities.ITSM.EscalationHistory>()        {        try    {        CancellationToken cancellationToken = default)        int topCount = 10,        DateTime endDate,        DateTime startDate,    public async Task<IEnumerable<TopEscalatingAssigneeDto>> GetTopEscalatingAssigneesAsync(    }        }            return Array.Empty<EscalationByPriorityDto>();            _logger.LogError(ex, "Error getting escalations by priority");        {        catch (Exception ex)        }            return escalations;                .ToListAsync(cancellationToken);                })                    AverageTimeToEscalationMinutes = Math.Round(g.Average(x => x.MinutesToEscalation), 2)                    EscalationCount = g.Count(),                    Priority = g.Key,                {                .Select(g => new EscalationByPriorityDto                .GroupBy(x => x.Priority)                    })                        MinutesToEscalation = EF.Functions.DateDiffMinute(sr.CreatedAt, e.EscalatedAt)                        Priority = sr.Priority.ToString(),                    {                    (e, sr) => new                    sr => sr.Id,                    e => e.ServiceRequestId,                    _dbContext.ServiceRequests.Where(sr => !sr.IsDeleted),                .Join(                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate && !e.IsDeleted)            var escalations = await _dbContext.Set<CRM.Core.Entities.ITSM.EscalationHistory>()        {        try    {        CancellationToken cancellationToken = default)        DateTime endDate,        DateTime startDate,    public async Task<IEnumerable<EscalationByPriorityDto>> GetEscalationsByPriorityAsync(    }        }            return Array.Empty<EscalationTrendDto>();            _logger.LogError(ex, "Error getting escalation trends");        {        catch (Exception ex)        }            return results;            }                current = periodEnd;                });                    EscalationRate = requestCount > 0 ? Math.Round((double)escalationCount / requestCount * 100, 2) : 0                    TotalServiceRequests = requestCount,                    EscalationCount = escalationCount,                    PeriodEnd = periodEnd,                    PeriodStart = current,                {                results.Add(new EscalationTrendDto                var requestCount = serviceRequests.Count(sr => sr >= current && sr < periodEnd);                var escalationCount = escalations.Count(e => e.EscalatedAt >= current && e.EscalatedAt < periodEnd);                if (periodEnd > endDate) periodEnd = endDate;                };                    _ => current.AddDays(1)                    TrendGranularity.Monthly => current.AddMonths(1),                    TrendGranularity.Weekly => current.AddDays(7),                    TrendGranularity.Daily => current.AddDays(1),                {                var periodEnd = granularity switch            {            while (current < endDate)            var current = startDate;            var results = new List<EscalationTrendDto>();                .ToListAsync(cancellationToken);                .Select(sr => sr.CreatedAt)                .Where(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted)            var serviceRequests = await _dbContext.ServiceRequests                .ToListAsync(cancellationToken);                .Select(e => new { e.EscalatedAt, e.ServiceRequestId })                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate && !e.IsDeleted)            var escalations = await _dbContext.Set<CRM.Core.Entities.ITSM.EscalationHistory>()        {        try    {        CancellationToken cancellationToken = default)        TrendGranularity granularity = TrendGranularity.Daily,        DateTime endDate,        DateTime startDate,    public async Task<IEnumerable<EscalationTrendDto>> GetEscalationTrendsAsync(    }        }            return new AverageEscalationTimeDto { TotalEscalations = 0 };            _logger.LogError(ex, "Error getting average escalation time");        {        catch (Exception ex)        }            };                        g => Math.Round(g.Average(e => e.MinutesToEscalation), 2))                        g => g.Key,                    .ToDictionary(                    .GroupBy(e => e.EscalationLevel)                AverageByEscalationLevel = escalations                TotalEscalations = escalations.Count,                MedianMinutesToFirstEscalation = Math.Round(allMinutes[allMinutes.Count / 2], 2),                AverageMinutesToFirstEscalation = Math.Round(allMinutes.Average(), 2),            {            return new AverageEscalationTimeDto            var allMinutes = escalations.Select(e => (double)e.MinutesToEscalation).OrderBy(m => m).ToList();            }                return new AverageEscalationTimeDto { TotalEscalations = 0 };            {            if (!escalations.Any())                .ToListAsync(cancellationToken);                })                    MinutesToEscalation = EF.Functions.DateDiffMinute(x.ServiceRequest.CreatedAt, x.Escalation.EscalatedAt)                    x.Escalation.EscalationLevel,                {                .Select(x => new                .Where(x => priority == null || x.ServiceRequest.Priority.ToString() == priority)                    (e, sr) => new { Escalation = e, ServiceRequest = sr })                    sr => sr.Id,                    e => e.ServiceRequestId,                    _dbContext.ServiceRequests.Where(sr => !sr.IsDeleted),                .Join(            var escalations = await query                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate && !e.IsDeleted);            var query = _dbContext.Set<CRM.Core.Entities.ITSM.EscalationHistory>()        {        try    {        CancellationToken cancellationToken = default)        string? priority = null,        DateTime endDate,        DateTime startDate,    public async Task<AverageEscalationTimeDto> GetAverageEscalationTimeAsync(    }        }            return Array.Empty<EscalationByCategoryDto>();            _logger.LogError(ex, "Error getting escalations by category");        {        catch (Exception ex)        }            });                PercentageOfTotal = total > 0 ? Math.Round((double)e.Count / total * 100, 2) : 0                EscalationCount = e.Count,                CategoryName = e.CategoryName,                CategoryId = e.CategoryId ?? 0,            {            return escalations.Select(e => new EscalationByCategoryDto            var total = escalations.Sum(x => x.Count);                .ToListAsync(cancellationToken);                })                    Count = g.Count()                    g.Key.CategoryName,                    g.Key.CategoryId,                {                .Select(g => new                .GroupBy(x => new { x.sr.CategoryId, CategoryName = x.sr.Category != null ? x.sr.Category.Name : "Uncategorized" })                    (e, sr) => new { e, sr })                    sr => sr.Id,                    e => e.ServiceRequestId,                    _dbContext.ServiceRequests.Where(sr => !sr.IsDeleted),                .Join(                .Where(e => e.EscalatedAt >= startDate && e.EscalatedAt <= endDate && !e.IsDeleted)            var escalations = await _dbContext.Set<CRM.Core.Entities.ITSM.EscalationHistory>()        {        try    {        CancellationToken cancellationToken = default)        DateTime endDate,        DateTime startDate,    public async Task<IEnumerable<EscalationByCategoryDto>> GetEscalationsByCategoryAsync(    }        _logger = logger;        _dbContext = dbContext;    {        ILogger<EscalationAnalyticsService> logger)        ICrmDbContext dbContext,    public EscalationAnalyticsService(    private readonly ILogger<EscalationAnalyticsService> _logger;    private readonly ICrmDbContext _dbContext;{public class EscalationAnalyticsService : IEscalationAnalyticsService/// </summary>/// TODO-SD005-011: Create escalation analytics reports./// Provides methods for analyzing escalation patterns, trends, and performance metrics./// Service for escalation analytics and reporting./// <summary>namespace CRM.Infrastructure.Services.ITSM;using Microsoft.Extensions.Logging;using Microsoft.EntityFrameworkCore;using CRM.Core.Interfaces.ITSM;using CRM.Core.Interfaces;// See the LICENSE file in the root directory for full terms.// the terms of the LICENSE file. Commercial use requires a separate license.// This software is source-available. Non-commercial use is permitted under//// Copyright (C) 2024-2026 Abhishek Lal// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
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
                return Array.Empty<EscalationByCategoryDto>();

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
                    periodEnd = endDate.AddDays(1);

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
                .ThenInclude(sr => sr!.AssignedTo)
                .ToListAsync(cancellationToken);

            var assigneeTotals = await _dbContext.ServiceRequests
                .Where(sr => sr.CreatedAt >= startDate && sr.CreatedAt <= endDate && !sr.IsDeleted)
                .Where(sr => sr.AssignedToId != null)
                .GroupBy(sr => sr.AssignedToId)
                .Select(g => new { AssigneeId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var grouped = logs
                .Where(e => e.ServiceRequest?.AssignedToId != null)
                .GroupBy(e => new
                {
                    AssigneeId = e.ServiceRequest!.AssignedToId!.Value,
                    AssigneeName = e.ServiceRequest.AssignedTo?.Username ?? "Unknown"
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
}
