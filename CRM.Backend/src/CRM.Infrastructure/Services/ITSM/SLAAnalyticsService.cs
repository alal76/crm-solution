// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Implements SLA analytics aggregation logic.
/// AP-021: extracted from SLAPoliciesController.GetSLADashboard to eliminate fat-controller GroupBy analytics.
/// </summary>
public class SLAAnalyticsService : ISLAAnalyticsService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SLAAnalyticsService> _logger;

    public SLAAnalyticsService(ICrmDbContext dbContext, ILogger<SLAAnalyticsService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SLADashboardDto> GetDashboardAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SLAAnalyticsService.GetDashboardAsync: {Start} → {End}", startDate, endDate);

        // Query service requests within the date range
        var requests = await _dbContext.ServiceRequests
            .AsNoTracking()
            .Where(sr => !sr.IsDeleted && sr.CreatedAt >= startDate && sr.CreatedAt <= endDate)
            .Select(sr => new
            {
                sr.Id,
                sr.Priority,
                sr.Status,
                sr.CreatedAt,
                sr.ResponseDueDate,
                sr.ResolutionDueDate,
                sr.FirstResponseDate,
                sr.ResolvedDate,
                sr.ResponseSlaBreached,
                sr.ResolutionSlaBreached
            })
            .ToListAsync(cancellationToken);

        var totalTickets = requests.Count;
        var breachedCount = requests.Count(r => r.ResponseSlaBreached || r.ResolutionSlaBreached);
        var withinSLA = totalTickets - breachedCount;
        var complianceRate = totalTickets > 0 ? (double)withinSLA / totalTickets * 100.0 : 100.0;

        // Average response time (minutes) for tickets that have a first response
        var respondedTickets = requests.Where(r => r.FirstResponseDate.HasValue).ToList();
        var avgResponseTime = respondedTickets.Count > 0
            ? respondedTickets.Average(r => (r.FirstResponseDate!.Value - r.CreatedAt).TotalMinutes)
            : 0.0;

        // Average resolution time (minutes) for resolved tickets
        var resolvedTickets = requests.Where(r => r.ResolvedDate.HasValue).ToList();
        var avgResolutionTime = resolvedTickets.Count > 0
            ? resolvedTickets.Average(r => (r.ResolvedDate!.Value - r.CreatedAt).TotalMinutes)
            : 0.0;

        // Breaches by priority
        var breachesByPriority = requests
            .Where(r => r.ResponseSlaBreached || r.ResolutionSlaBreached)
            .GroupBy(r => r.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Daily trend
        var dailyTrend = requests
            .GroupBy(r => r.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var dayTotal = g.Count();
                var dayBreached = g.Count(r => r.ResponseSlaBreached || r.ResolutionSlaBreached);
                var dayWithin = dayTotal - dayBreached;
                return new SLATrendPoint
                {
                    Date = g.Key,
                    ComplianceRate = dayTotal > 0 ? (double)dayWithin / dayTotal * 100.0 : 100.0,
                    TotalTickets = dayTotal
                };
            })
            .ToList();

        _logger.LogInformation(
            "SLA dashboard: {Total} tickets, {Breached} breached, {Rate:F1}% compliance",
            totalTickets, breachedCount, complianceRate);

        return new SLADashboardDto
        {
            TotalTickets = totalTickets,
            WithinSLA = withinSLA,
            BreachedSLA = breachedCount,
            ComplianceRate = Math.Round(complianceRate, 2),
            AvgResponseTimeMinutes = Math.Round(avgResponseTime, 2),
            AvgResolutionTimeMinutes = Math.Round(avgResolutionTime, 2),
            BreachesByPriority = breachesByPriority,
            DailyTrend = dailyTrend
        };
    }
}
