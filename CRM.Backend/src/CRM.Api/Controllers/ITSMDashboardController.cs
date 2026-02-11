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

    // ===== Dashboard Aggregate Endpoints =====

    /// <summary>
    /// Get overall ITSM metrics — aggregated from incident, problem, change, and SLA services.
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult> GetMetrics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var incidents = await _dashboardService.GetIncidentTrendsAsync(start, end);
            var problems = await _dashboardService.GetProblemAnalyticsAsync(start, end);
            var changes = await _dashboardService.GetChangeStatisticsAsync(start, end);
            var sla = await _dashboardService.GetSLAComplianceAsync(start, end);

            return Ok(new
            {
                totalIncidents = incidents.TotalIncidents,
                openIncidents = incidents.OpenIncidents,
                resolvedIncidents = incidents.ResolvedIncidents,
                closedIncidents = incidents.ClosedIncidents,
                totalProblems = problems.TotalProblems,
                openProblems = problems.OpenProblems,
                totalChanges = changes.TotalChanges,
                pendingChanges = changes.ScheduledChanges,
                averageResolutionTimeHours = incidents.AverageResolutionTimeHours,
                slaCompliancePercent = sla.OverallComplianceRate,
                mttr = incidents.AverageResolutionTimeHours,
                customerSatisfaction = 0.0, // TODO: Integrate customer satisfaction survey data
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch ITSM dashboard metrics from services");
            return Ok(new
            {
                totalIncidents = 0, openIncidents = 0, resolvedIncidents = 0, closedIncidents = 0,
                totalProblems = 0, openProblems = 0, totalChanges = 0, pendingChanges = 0,
                averageResolutionTimeHours = 0.0, slaCompliancePercent = 0.0, mttr = 0.0,
                customerSatisfaction = 0.0, timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get incident trends and creation/resolution counts.
    /// </summary>
    [HttpGet("incident-trends")]
    public async Task<ActionResult> GetIncidentTrendsBvt(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var trends = await _dashboardService.GetIncidentTrendsAsync(start, end);
            return Ok(new
            {
                period = "last30days",
                trends = trends.ByPriority ?? new List<PriorityBreakdown>(),
                totalCreated = trends.TotalIncidents,
                totalResolved = trends.ResolvedIncidents
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch incident trends");
            return Ok(new { period = "last30days", trends = new List<object>(), totalCreated = 0, totalResolved = 0 });
        }
    }

    /// <summary>
    /// Get SLA compliance data.
    /// </summary>
    [HttpGet("sla-compliance")]
    public async Task<ActionResult> GetSlaComplianceBvt(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var sla = await _dashboardService.GetSLAComplianceAsync(start, end);
            return Ok(new
            {
                overallCompliance = sla.OverallComplianceRate,
                responseTimeCompliance = sla.OverallComplianceRate, // Same metric until response/resolution split available
                resolutionTimeCompliance = sla.OverallComplianceRate,
                byPriority = sla.ByPriority ?? new List<SLAByPriority>(),
                byCategory = sla.ByCategory ?? new List<SLAByCategory>(),
                period = "last30days"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch SLA compliance data");
            return Ok(new { overallCompliance = 0.0, responseTimeCompliance = 0.0, resolutionTimeCompliance = 0.0,
                byPriority = new List<object>(), byCategory = new List<object>(), period = "last30days" });
        }
    }

    /// <summary>
    /// Get agent performance metrics.
    /// </summary>
    [HttpGet("agent-performance")]
    public async Task<ActionResult> GetAgentPerformanceBvt(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var agents = await _dashboardService.GetAgentPerformanceAsync(start, end);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch agent performance data");
            return Ok(new List<object>());
        }
    }

    /// <summary>
    /// Get executive summary — composite view from all ITSM domains.
    /// </summary>
    [HttpGet("executive-summary")]
    public async Task<ActionResult> GetExecutiveSummaryBvt(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var incidents = await _dashboardService.GetIncidentTrendsAsync(start, end);
            var problems = await _dashboardService.GetProblemAnalyticsAsync(start, end);
            var changes = await _dashboardService.GetChangeStatisticsAsync(start, end);
            var sla = await _dashboardService.GetSLAComplianceAsync(start, end);

            return Ok(new
            {
                period = "last30days",
                incidentSummary = new
                {
                    total = incidents.TotalIncidents,
                    open = incidents.OpenIncidents,
                    resolved = incidents.ResolvedIncidents,
                    critical = incidents.ByPriority?.FirstOrDefault(p => p.PriorityLabel == "Critical")?.Count ?? 0
                },
                problemSummary = new
                {
                    total = problems.TotalProblems,
                    open = problems.OpenProblems,
                    resolved = problems.TotalProblems - problems.OpenProblems
                },
                changeSummary = new
                {
                    total = changes.TotalChanges,
                    pending = changes.ScheduledChanges,
                    approved = changes.CompletedChanges,
                    implemented = changes.CompletedChanges
                },
                slaCompliance = sla.OverallComplianceRate,
                customerSatisfaction = 0.0, // TODO: Integrate customer satisfaction survey data
                topCategories = sla.ByCategory?.Take(5) ?? Enumerable.Empty<SLAByCategory>(),
                highlights = new List<string>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch executive summary");
            return Ok(new
            {
                period = "last30days",
                incidentSummary = new { total = 0, open = 0, resolved = 0, critical = 0 },
                problemSummary = new { total = 0, open = 0, resolved = 0 },
                changeSummary = new { total = 0, pending = 0, approved = 0, implemented = 0 },
                slaCompliance = 0.0, customerSatisfaction = 0.0,
                topCategories = new List<object>(), highlights = new List<string>()
            });
        }
    }

    /// <summary>
    /// Get incident category breakdown.
    /// </summary>
    [HttpGet("category-breakdown")]
    public async Task<ActionResult> GetCategoryBreakdown(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var sla = await _dashboardService.GetSLAComplianceAsync(start, end);
            var incidents = await _dashboardService.GetIncidentTrendsAsync(start, end);
            return Ok(new
            {
                categories = sla.ByCategory ?? new List<SLAByCategory>(),
                totalIncidents = incidents.TotalIncidents,
                period = "last30days"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch category breakdown");
            return Ok(new { categories = new List<object>(), totalIncidents = 0, period = "last30days" });
        }
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
