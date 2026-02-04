// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for ITSM dashboard analytics and metrics.
/// </summary>
public interface IITSMDashboardService
{
    /// <summary>
    /// Get incident trends over a time period.
    /// </summary>
    Task<IncidentTrendsDto> GetIncidentTrendsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get problem analytics.
    /// </summary>
    Task<ProblemAnalyticsDto> GetProblemAnalyticsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get change management statistics.
    /// </summary>
    Task<ChangeStatisticsDto> GetChangeStatisticsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get SLA compliance metrics.
    /// </summary>
    Task<SLAComplianceDto> GetSLAComplianceAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get agent performance metrics.
    /// </summary>
    Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get CMDB health overview.
    /// </summary>
    Task<CMDBHealthDto> GetCMDBHealthAsync();

    /// <summary>
    /// Get knowledge base usage analytics.
    /// </summary>
    Task<KnowledgeAnalyticsDto> GetKnowledgeAnalyticsAsync(DateTime startDate, DateTime endDate);
}

// ====== DTOs ======

public class IncidentTrendsDto
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int ClosedIncidents { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double FirstContactResolutionRate { get; set; }
    public List<DailyTrendItem> DailyTrends { get; set; } = new();
    public List<CategoryBreakdown> ByCategory { get; set; } = new();
    public List<PriorityBreakdown> ByPriority { get; set; } = new();
}

public class DailyTrendItem
{
    public DateTime Date { get; set; }
    public int Created { get; set; }
    public int Resolved { get; set; }
    public int Backlog { get; set; }
}

public class CategoryBreakdown
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class PriorityBreakdown
{
    public int Priority { get; set; }
    public string PriorityLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ProblemAnalyticsDto
{
    public int TotalProblems { get; set; }
    public int OpenProblems { get; set; }
    public int ProblemsWithKnownError { get; set; }
    public int ProblemsWithWorkaround { get; set; }
    public int LinkedIncidentsCount { get; set; }
    public List<RootCauseBreakdown> ByRootCause { get; set; } = new();
    public List<TopProblem> TopRecurringProblems { get; set; } = new();
}

public class RootCauseBreakdown
{
    public string RootCause { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class TopProblem
{
    public int ProblemId { get; set; }
    public string ProblemNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int LinkedIncidents { get; set; }
}

public class ChangeStatisticsDto
{
    public int TotalChanges { get; set; }
    public int ScheduledChanges { get; set; }
    public int CompletedChanges { get; set; }
    public int FailedChanges { get; set; }
    public int RolledBackChanges { get; set; }
    public double SuccessRate { get; set; }
    public List<ChangeTypeBreakdown> ByType { get; set; } = new();
    public List<ChangeRiskBreakdown> ByRisk { get; set; } = new();
}

public class ChangeTypeBreakdown
{
    public string ChangeType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ChangeRiskBreakdown
{
    public string RiskLevel { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class SLAComplianceDto
{
    public double OverallComplianceRate { get; set; }
    public int TotalTickets { get; set; }
    public int TicketsWithinSLA { get; set; }
    public int TicketsBreachedSLA { get; set; }
    public int TicketsAtRisk { get; set; }
    public List<SLAByPriority> ByPriority { get; set; } = new();
    public List<SLAByCategory> ByCategory { get; set; } = new();
    public List<SLATrendItem> Trends { get; set; } = new();
}

public class SLAByPriority
{
    public int Priority { get; set; }
    public string PriorityLabel { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Met { get; set; }
    public int Breached { get; set; }
    public double ComplianceRate { get; set; }
}

public class SLAByCategory
{
    public string Category { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Met { get; set; }
    public int Breached { get; set; }
    public double ComplianceRate { get; set; }
}

public class SLATrendItem
{
    public DateTime Date { get; set; }
    public double ComplianceRate { get; set; }
    public int Met { get; set; }
    public int Breached { get; set; }
}

public class AgentPerformanceDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int TicketsAssigned { get; set; }
    public int TicketsResolved { get; set; }
    public int TicketsReopened { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double FirstContactResolutionRate { get; set; }
    public double SLAComplianceRate { get; set; }
    public double? CustomerSatisfactionScore { get; set; }
    public int CurrentBacklog { get; set; }
}

public class CMDBHealthDto
{
    public int TotalConfigurationItems { get; set; }
    public int ActiveItems { get; set; }
    public int RetiredItems { get; set; }
    public int ItemsNeedingReview { get; set; }
    public int OrphanedItems { get; set; }
    public int TotalRelationships { get; set; }
    public DateTime LastAuditDate { get; set; }
    public List<CITypeBreakdown> ByType { get; set; } = new();
    public List<CIStatusBreakdown> ByStatus { get; set; } = new();
}

public class CITypeBreakdown
{
    public string CIType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CIStatusBreakdown
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class KnowledgeAnalyticsDto
{
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public int ArticlesNeedingReview { get; set; }
    public int TotalViews { get; set; }
    public int TotalSearches { get; set; }
    public double HelpfulRate { get; set; }
    public List<TopArticle> MostViewedArticles { get; set; } = new();
    public List<TopSearchTerm> TopSearchTerms { get; set; } = new();
    public List<CategoryUsage> UsageByCategory { get; set; } = new();
}

public class TopArticle
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public int HelpfulVotes { get; set; }
    public int NotHelpfulVotes { get; set; }
}

public class TopSearchTerm
{
    public string SearchTerm { get; set; } = string.Empty;
    public int Count { get; set; }
    public int ResultsFound { get; set; }
}

public class CategoryUsage
{
    public string Category { get; set; } = string.Empty;
    public int Articles { get; set; }
    public int Views { get; set; }
}
