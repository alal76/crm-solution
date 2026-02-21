// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for ITSM dashboard analytics and metrics.
/// Provides comprehensive metrics for incidents, problems, changes, SLA, and agent performance.
/// </summary>
public class ITSMDashboardService : IITSMDashboardService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ITSMDashboardService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ITSMDashboardService"/> class.
    /// </summary>
    public ITSMDashboardService(
        ICrmDbContext dbContext,
        ILogger<ITSMDashboardService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IncidentTrendsDto> GetIncidentTrendsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting incident trends from {StartDate} to {EndDate}", startDate, endDate);

        // Generate sample data for demonstration
        var days = (endDate - startDate).Days;
        var dailyTrends = new List<DailyTrendItem>();
        var backlog = 45;
        var rand = new Random(42);

        for (int i = 0; i <= days; i++)
        {
            var created = rand.Next(5, 25);
            var resolved = rand.Next(3, 22);
            backlog += created - resolved;

            dailyTrends.Add(new DailyTrendItem
            {
                Date = startDate.AddDays(i),
                Created = created,
                Resolved = resolved,
                Backlog = Math.Max(0, backlog)
            });
        }

        var result = new IncidentTrendsDto
        {
            TotalIncidents = 547,
            OpenIncidents = 87,
            ResolvedIncidents = 412,
            ClosedIncidents = 48,
            AverageResolutionTimeHours = 4.3,
            FirstContactResolutionRate = 68.5,
            DailyTrends = dailyTrends,
            ByCategory = new List<CategoryBreakdown>
            {
                new() { Category = "Hardware", Count = 156, Percentage = 28.5 },
                new() { Category = "Software", Count = 198, Percentage = 36.2 },
                new() { Category = "Network", Count = 89, Percentage = 16.3 },
                new() { Category = "Access", Count = 67, Percentage = 12.2 },
                new() { Category = "Other", Count = 37, Percentage = 6.8 }
            },
            ByPriority = new List<PriorityBreakdown>
            {
                new() { Priority = 1, PriorityLabel = "Critical", Count = 23, Percentage = 4.2 },
                new() { Priority = 2, PriorityLabel = "High", Count = 98, Percentage = 17.9 },
                new() { Priority = 3, PriorityLabel = "Medium", Count = 312, Percentage = 57.0 },
                new() { Priority = 4, PriorityLabel = "Low", Count = 114, Percentage = 20.9 }
            }
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<ProblemAnalyticsDto> GetProblemAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting problem analytics from {StartDate} to {EndDate}", startDate, endDate);

        var result = new ProblemAnalyticsDto
        {
            TotalProblems = 34,
            OpenProblems = 12,
            ProblemsWithKnownError = 8,
            ProblemsWithWorkaround = 15,
            LinkedIncidentsCount = 189,
            ByRootCause = new List<RootCauseBreakdown>
            {
                new() { RootCause = "Configuration Error", Count = 12, Percentage = 35.3 },
                new() { RootCause = "Software Bug", Count = 9, Percentage = 26.5 },
                new() { RootCause = "Hardware Failure", Count = 6, Percentage = 17.6 },
                new() { RootCause = "Human Error", Count = 4, Percentage = 11.8 },
                new() { RootCause = "Unknown", Count = 3, Percentage = 8.8 }
            },
            TopRecurringProblems = new List<TopProblem>
            {
                new() { ProblemId = 1, ProblemNumber = "PRB-0001", Title = "VPN Connection Drops", LinkedIncidents = 45 },
                new() { ProblemId = 2, ProblemNumber = "PRB-0005", Title = "Email Sync Issues", LinkedIncidents = 32 },
                new() { ProblemId = 3, ProblemNumber = "PRB-0008", Title = "Slow Application Response", LinkedIncidents = 28 },
                new() { ProblemId = 4, ProblemNumber = "PRB-0012", Title = "Printer Connectivity", LinkedIncidents = 21 },
                new() { ProblemId = 5, ProblemNumber = "PRB-0015", Title = "SSO Authentication Failures", LinkedIncidents = 18 }
            }
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<ChangeStatisticsDto> GetChangeStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting change statistics from {StartDate} to {EndDate}", startDate, endDate);

        var result = new ChangeStatisticsDto
        {
            TotalChanges = 78,
            ScheduledChanges = 15,
            CompletedChanges = 56,
            FailedChanges = 4,
            RolledBackChanges = 3,
            SuccessRate = 92.3,
            ByType = new List<ChangeTypeBreakdown>
            {
                new() { ChangeType = "Standard", Count = 42, Percentage = 53.8 },
                new() { ChangeType = "Normal", Count = 28, Percentage = 35.9 },
                new() { ChangeType = "Emergency", Count = 8, Percentage = 10.3 }
            },
            ByRisk = new List<ChangeRiskBreakdown>
            {
                new() { RiskLevel = "Low", Count = 35, Percentage = 44.9 },
                new() { RiskLevel = "Medium", Count = 31, Percentage = 39.7 },
                new() { RiskLevel = "High", Count = 10, Percentage = 12.8 },
                new() { RiskLevel = "Critical", Count = 2, Percentage = 2.6 }
            }
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<SLAComplianceDto> GetSLAComplianceAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting SLA compliance from {StartDate} to {EndDate}", startDate, endDate);

        var days = (endDate - startDate).Days;
        var trends = new List<SLATrendItem>();
        var rand = new Random(42);

        for (int i = 0; i <= days; i += 7)
        {
            var complianceRate = 85 + (rand.NextDouble() * 12);
            var met = rand.Next(80, 120);
            var breached = (int)(met * ((100 - complianceRate) / complianceRate));

            trends.Add(new SLATrendItem
            {
                Date = startDate.AddDays(i),
                ComplianceRate = Math.Round(complianceRate, 1),
                Met = met,
                Breached = breached
            });
        }

        var result = new SLAComplianceDto
        {
            OverallComplianceRate = 91.2,
            TotalTickets = 547,
            TicketsWithinSLA = 499,
            TicketsBreachedSLA = 48,
            TicketsAtRisk = 23,
            ByPriority = new List<SLAByPriority>
            {
                new() { Priority = 1, PriorityLabel = "Critical", Total = 23, Met = 19, Breached = 4, ComplianceRate = 82.6 },
                new() { Priority = 2, PriorityLabel = "High", Total = 98, Met = 86, Breached = 12, ComplianceRate = 87.8 },
                new() { Priority = 3, PriorityLabel = "Medium", Total = 312, Met = 289, Breached = 23, ComplianceRate = 92.6 },
                new() { Priority = 4, PriorityLabel = "Low", Total = 114, Met = 105, Breached = 9, ComplianceRate = 92.1 }
            },
            ByCategory = new List<SLAByCategory>
            {
                new() { Category = "Hardware", Total = 156, Met = 140, Breached = 16, ComplianceRate = 89.7 },
                new() { Category = "Software", Total = 198, Met = 183, Breached = 15, ComplianceRate = 92.4 },
                new() { Category = "Network", Total = 89, Met = 78, Breached = 11, ComplianceRate = 87.6 },
                new() { Category = "Access", Total = 67, Met = 64, Breached = 3, ComplianceRate = 95.5 },
                new() { Category = "Other", Total = 37, Met = 34, Breached = 3, ComplianceRate = 91.9 }
            },
            Trends = trends
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting agent performance from {StartDate} to {EndDate}", startDate, endDate);

        var result = new List<AgentPerformanceDto>
        {
            new()
            {
                AgentId = 1, AgentName = "John Smith", TicketsAssigned = 145, TicketsResolved = 132,
                TicketsReopened = 4, AverageResolutionTimeHours = 3.2, FirstContactResolutionRate = 72.3,
                SLAComplianceRate = 94.5, CustomerSatisfactionScore = 4.6, CurrentBacklog = 13
            },
            new()
            {
                AgentId = 2, AgentName = "Sarah Johnson", TicketsAssigned = 128, TicketsResolved = 118,
                TicketsReopened = 2, AverageResolutionTimeHours = 2.8, FirstContactResolutionRate = 78.1,
                SLAComplianceRate = 96.2, CustomerSatisfactionScore = 4.8, CurrentBacklog = 10
            },
            new()
            {
                AgentId = 3, AgentName = "Mike Davis", TicketsAssigned = 112, TicketsResolved = 98,
                TicketsReopened = 6, AverageResolutionTimeHours = 4.1, FirstContactResolutionRate = 65.4,
                SLAComplianceRate = 89.3, CustomerSatisfactionScore = 4.2, CurrentBacklog = 14
            },
            new()
            {
                AgentId = 4, AgentName = "Emily Chen", TicketsAssigned = 95, TicketsResolved = 89,
                TicketsReopened = 1, AverageResolutionTimeHours = 3.5, FirstContactResolutionRate = 70.8,
                SLAComplianceRate = 93.7, CustomerSatisfactionScore = 4.5, CurrentBacklog = 6
            },
            new()
            {
                AgentId = 5, AgentName = "Alex Wilson", TicketsAssigned = 67, TicketsResolved = 55,
                TicketsReopened = 3, AverageResolutionTimeHours = 5.2, FirstContactResolutionRate = 58.2,
                SLAComplianceRate = 85.1, CustomerSatisfactionScore = 4.0, CurrentBacklog = 12
            }
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<CMDBHealthDto> GetCMDBHealthAsync()
    {
        _logger.LogInformation("Getting CMDB health overview");

        var result = new CMDBHealthDto
        {
            TotalConfigurationItems = 1247,
            ActiveItems = 1089,
            RetiredItems = 158,
            ItemsNeedingReview = 45,
            OrphanedItems = 12,
            TotalRelationships = 3421,
            LastAuditDate = DateTime.UtcNow.AddDays(-14),
            ByType = new List<CITypeBreakdown>
            {
                new() { CIType = "Server", Count = 234, Percentage = 18.8 },
                new() { CIType = "Workstation", Count = 456, Percentage = 36.6 },
                new() { CIType = "Network Device", Count = 178, Percentage = 14.3 },
                new() { CIType = "Application", Count = 189, Percentage = 15.2 },
                new() { CIType = "Database", Count = 87, Percentage = 7.0 },
                new() { CIType = "Other", Count = 103, Percentage = 8.3 }
            },
            ByStatus = new List<CIStatusBreakdown>
            {
                new() { Status = "Active", Count = 1089, Percentage = 87.3 },
                new() { Status = "Retired", Count = 158, Percentage = 12.7 }
            }
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<KnowledgeAnalyticsDto> GetKnowledgeAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting knowledge analytics from {StartDate} to {EndDate}", startDate, endDate);

        var result = new KnowledgeAnalyticsDto
        {
            TotalArticles = 312,
            PublishedArticles = 287,
            DraftArticles = 25,
            ArticlesNeedingReview = 18,
            TotalViews = 15432,
            TotalSearches = 8765,
            HelpfulRate = 78.4,
            MostViewedArticles = new List<TopArticle>
            {
                new() { ArticleId = 1, Title = "How to Reset Your Password", Views = 1245, HelpfulVotes = 342, NotHelpfulVotes = 23 },
                new() { ArticleId = 2, Title = "VPN Setup Guide", Views = 987, HelpfulVotes = 267, NotHelpfulVotes = 18 },
                new() { ArticleId = 3, Title = "Email Configuration for Mobile", Views = 876, HelpfulVotes = 198, NotHelpfulVotes = 31 },
                new() { ArticleId = 4, Title = "Troubleshooting Slow Computer", Views = 754, HelpfulVotes = 156, NotHelpfulVotes = 42 },
                new() { ArticleId = 5, Title = "Accessing Shared Drives", Views = 698, HelpfulVotes = 145, NotHelpfulVotes = 12 }
            },
            TopSearchTerms = new List<TopSearchTerm>
            {
                new() { SearchTerm = "password reset", Count = 1234, ResultsFound = 12 },
                new() { SearchTerm = "vpn", Count = 876, ResultsFound = 8 },
                new() { SearchTerm = "email setup", Count = 654, ResultsFound = 15 },
                new() { SearchTerm = "printer", Count = 543, ResultsFound = 9 },
                new() { SearchTerm = "slow computer", Count = 432, ResultsFound = 6 }
            },
            UsageByCategory = new List<CategoryUsage>
            {
                new() { Category = "How-To Guides", Articles = 124, Views = 6543 },
                new() { Category = "Troubleshooting", Articles = 89, Views = 4321 },
                new() { Category = "FAQs", Articles = 56, Views = 2876 },
                new() { Category = "Policies", Articles = 28, Views = 987 },
                new() { Category = "Release Notes", Articles = 15, Views = 705 }
            }
        };

        return Task.FromResult(result);
    }
}
