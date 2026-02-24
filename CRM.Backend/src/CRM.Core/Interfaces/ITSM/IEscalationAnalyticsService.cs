// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for escalation analytics and reporting.
/// Provides methods for analyzing escalation patterns, trends, and performance metrics.
/// TODO-SD005-011: Create escalation analytics reports.
/// </summary>
public interface IEscalationAnalyticsService
{
    /// <summary>
    /// Gets escalation statistics grouped by category.
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Escalation counts by category.</returns>
    Task<IEnumerable<EscalationByCategoryDto>> GetEscalationsByCategoryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the average time to escalation for service requests.
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="priority">Optional priority filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Average escalation time metrics.</returns>
    Task<AverageEscalationTimeDto> GetAverageEscalationTimeAsync(
        DateTime startDate,
        DateTime endDate,
        string? priority = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets escalation trends over time (daily, weekly, or monthly).
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="granularity">Time granularity (day, week, month).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Escalation trend data points.</returns>
    Task<IEnumerable<EscalationTrendDto>> GetEscalationTrendsAsync(
        DateTime startDate,
        DateTime endDate,
        TrendGranularity granularity = TrendGranularity.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets escalation statistics grouped by priority level.
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Escalation counts by priority.</returns>
    Task<IEnumerable<EscalationByPriorityDto>> GetEscalationsByPriorityAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the top escalating assignees/teams.
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="topCount">Number of top results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Top assignees by escalation count.</returns>
    Task<IEnumerable<TopEscalatingAssigneeDto>> GetTopEscalatingAssigneesAsync(
        DateTime startDate,
        DateTime endDate,
        int topCount = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a comprehensive escalation dashboard summary.
    /// </summary>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete escalation dashboard data.</returns>
    Task<EscalationDashboardDto> GetEscalationDashboardAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Time granularity for trend analysis.
/// </summary>
public enum TrendGranularity
{
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// Escalation count by category.
/// </summary>
public class EscalationByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EscalationCount { get; set; }
    public double PercentageOfTotal { get; set; }
}

/// <summary>
/// Average escalation time metrics.
/// </summary>
public class AverageEscalationTimeDto
{
    public double AverageMinutesToFirstEscalation { get; set; }
    public double AverageMinutesToResolution { get; set; }
    public double MedianMinutesToFirstEscalation { get; set; }
    public int TotalEscalations { get; set; }
    public Dictionary<int, double> AverageByEscalationLevel { get; set; } = new();
}

/// <summary>
/// Escalation trend data point.
/// </summary>
public class EscalationTrendDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int EscalationCount { get; set; }
    public int TotalServiceRequests { get; set; }
    public double EscalationRate { get; set; }
}

/// <summary>
/// Escalation count by priority level.
/// </summary>
public class EscalationByPriorityDto
{
    public string Priority { get; set; } = string.Empty;
    public int EscalationCount { get; set; }
    public double AverageTimeToEscalationMinutes { get; set; }
}

/// <summary>
/// Top escalating assignee statistics.
/// </summary>
public class TopEscalatingAssigneeDto
{
    public int AssigneeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public int EscalationCount { get; set; }
    public int TotalAssignedRequests { get; set; }
    public double EscalationRate { get; set; }
}

/// <summary>
/// Complete escalation dashboard data.
/// </summary>
public class EscalationDashboardDto
{
    public int TotalEscalations { get; set; }
    public int TotalServiceRequests { get; set; }
    public double OverallEscalationRate { get; set; }
    public double AverageTimeToFirstEscalation { get; set; }
    public IEnumerable<EscalationByCategoryDto> ByCategory { get; set; } = Array.Empty<EscalationByCategoryDto>();
    public IEnumerable<EscalationByPriorityDto> ByPriority { get; set; } = Array.Empty<EscalationByPriorityDto>();
    public IEnumerable<EscalationTrendDto> Trend { get; set; } = Array.Empty<EscalationTrendDto>();
    public IEnumerable<TopEscalatingAssigneeDto> TopAssignees { get; set; } = Array.Empty<TopEscalatingAssigneeDto>();
}
