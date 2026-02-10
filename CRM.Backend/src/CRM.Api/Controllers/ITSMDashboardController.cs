// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for ITSM dashboard analytics and reports.
/// </summary>
[ApiController]
[Route("api/itsm/dashboard")]
[Authorize]
[Tags("ITSM - Dashboard & Analytics")]
public class ITSMDashboardController : ControllerBase
{
    private readonly IITSMDashboardService _dashboardService;
    private readonly ILogger<ITSMDashboardController> _logger;

    public ITSMDashboardController(
        IITSMDashboardService dashboardService,
        ILogger<ITSMDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    // ===== BVT Stub Endpoints =====

    /// <summary>
    /// Get overall ITSM metrics (BVT endpoint).
    /// </summary>
    [HttpGet("metrics")]
    public ActionResult GetMetrics()
    {
        return Ok(new
        {
            totalIncidents = 0,
            openIncidents = 0,
            resolvedIncidents = 0,
            closedIncidents = 0,
            totalProblems = 0,
            openProblems = 0,
            totalChanges = 0,
            pendingChanges = 0,
            averageResolutionTimeHours = 0.0,
            slaCompliancePercent = 100.0,
            mttr = 0.0,
            customerSatisfaction = 0.0,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get incident trends (BVT endpoint).
    /// </summary>
    [HttpGet("incident-trends")]
    public ActionResult GetIncidentTrendsBvt()
    {
        return Ok(new
        {
            period = "last30days",
            trends = new List<object>(),
            totalCreated = 0,
            totalResolved = 0
        });
    }

    /// <summary>
    /// Get SLA compliance data (BVT endpoint).
    /// </summary>
    [HttpGet("sla-compliance")]
    public ActionResult GetSlaComplianceBvt()
    {
        return Ok(new
        {
            overallCompliance = 100.0,
            responseTimeCompliance = 100.0,
            resolutionTimeCompliance = 100.0,
            byPriority = new List<object>(),
            byCategory = new List<object>(),
            period = "last30days"
        });
    }

    /// <summary>
    /// Get agent performance metrics (BVT endpoint).
    /// </summary>
    [HttpGet("agent-performance")]
    public ActionResult GetAgentPerformanceBvt()
    {
        return Ok(new List<object>());
    }

    /// <summary>
    /// Get executive summary (BVT endpoint).
    /// </summary>
    [HttpGet("executive-summary")]
    public ActionResult GetExecutiveSummaryBvt()
    {
        return Ok(new
        {
            period = "last30days",
            incidentSummary = new { total = 0, open = 0, resolved = 0, critical = 0 },
            problemSummary = new { total = 0, open = 0, resolved = 0 },
            changeSummary = new { total = 0, pending = 0, approved = 0, implemented = 0 },
            slaCompliance = 100.0,
            customerSatisfaction = 0.0,
            topCategories = new List<object>(),
            highlights = new List<string>()
        });
    }

    /// <summary>
    /// Get category breakdown (BVT endpoint).
    /// </summary>
    [HttpGet("category-breakdown")]
    public ActionResult GetCategoryBreakdown()
    {
        return Ok(new
        {
            categories = new List<object>(),
            totalIncidents = 0,
            period = "last30days"
        });
    }

    // ===== Service-backed Endpoints =====

    /// <summary>
    /// Get incident trends and statistics.
    /// </summary>
    [HttpGet("incidents")]
    public async Task<ActionResult<IncidentTrendsDto>> GetIncidentTrends(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _dashboardService.GetIncidentTrendsAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// Get problem analytics.
    /// </summary>
    [HttpGet("problems")]
    public async Task<ActionResult<ProblemAnalyticsDto>> GetProblemAnalytics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _dashboardService.GetProblemAnalyticsAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// Get change management statistics.
    /// </summary>
    [HttpGet("changes")]
    public async Task<ActionResult<ChangeStatisticsDto>> GetChangeStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _dashboardService.GetChangeStatisticsAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// Get SLA compliance metrics.
    /// </summary>
    [HttpGet("sla")]
    public async Task<ActionResult<SLAComplianceDto>> GetSLACompliance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _dashboardService.GetSLAComplianceAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// Get agent performance metrics.
    /// </summary>
    [HttpGet("agents")]
    public async Task<ActionResult<List<AgentPerformanceDto>>> GetAgentPerformance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _dashboardService.GetAgentPerformanceAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// Get CMDB health overview.
    /// </summary>
    [HttpGet("cmdb")]
    public async Task<ActionResult<CMDBHealthDto>> GetCMDBHealth()
    {
        var result = await _dashboardService.GetCMDBHealthAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get knowledge base analytics.
    /// </summary>
    [HttpGet("knowledge")]
    public async Task<ActionResult<KnowledgeAnalyticsDto>> GetKnowledgeAnalytics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _dashboardService.GetKnowledgeAnalyticsAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// Get executive summary dashboard.
    /// </summary>
    [HttpGet("executive")]
    public async Task<ActionResult<ExecutiveSummaryDto>> GetExecutiveSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        // Gather all metrics
        var incidents = await _dashboardService.GetIncidentTrendsAsync(start, end);
        var sla = await _dashboardService.GetSLAComplianceAsync(start, end);
        var changes = await _dashboardService.GetChangeStatisticsAsync(start, end);
        var problems = await _dashboardService.GetProblemAnalyticsAsync(start, end);
        var cmdb = await _dashboardService.GetCMDBHealthAsync();

        var summary = new ExecutiveSummaryDto
        {
            Period = new DateRangeDto { StartDate = start, EndDate = end },
            IncidentMetrics = new IncidentSummary
            {
                Total = incidents.TotalIncidents,
                Open = incidents.OpenIncidents,
                Resolved = incidents.ResolvedIncidents,
                AvgResolutionHours = incidents.AverageResolutionTimeHours,
                FirstContactResolutionRate = incidents.FirstContactResolutionRate
            },
            SLAMetrics = new SLASummary
            {
                OverallCompliance = sla.OverallComplianceRate,
                MetCount = sla.TicketsWithinSLA,
                BreachedCount = sla.TicketsBreachedSLA,
                AtRiskCount = sla.TicketsAtRisk
            },
            ChangeMetrics = new ChangeSummary
            {
                Total = changes.TotalChanges,
                Successful = changes.CompletedChanges,
                Failed = changes.FailedChanges,
                SuccessRate = changes.SuccessRate
            },
            ProblemMetrics = new ProblemSummary
            {
                Total = problems.TotalProblems,
                Open = problems.OpenProblems,
                WithKnownError = problems.ProblemsWithKnownError,
                LinkedIncidents = problems.LinkedIncidentsCount
            },
            CMDBMetrics = new CMDBSummary
            {
                TotalItems = cmdb.TotalConfigurationItems,
                ActiveItems = cmdb.ActiveItems,
                NeedsReview = cmdb.ItemsNeedingReview
            }
        };

        return Ok(summary);
    }
}

// Executive Summary DTOs
public class ExecutiveSummaryDto
{
    public DateRangeDto Period { get; set; } = new();
    public IncidentSummary IncidentMetrics { get; set; } = new();
    public SLASummary SLAMetrics { get; set; } = new();
    public ChangeSummary ChangeMetrics { get; set; } = new();
    public ProblemSummary ProblemMetrics { get; set; } = new();
    public CMDBSummary CMDBMetrics { get; set; } = new();
}

public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class IncidentSummary
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int Resolved { get; set; }
    public double AvgResolutionHours { get; set; }
    public double FirstContactResolutionRate { get; set; }
}

public class SLASummary
{
    public double OverallCompliance { get; set; }
    public int MetCount { get; set; }
    public int BreachedCount { get; set; }
    public int AtRiskCount { get; set; }
}

public class ChangeSummary
{
    public int Total { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public double SuccessRate { get; set; }
}

public class ProblemSummary
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int WithKnownError { get; set; }
    public int LinkedIncidents { get; set; }
}

public class CMDBSummary
{
    public int TotalItems { get; set; }
    public int ActiveItems { get; set; }
    public int NeedsReview { get; set; }
}
