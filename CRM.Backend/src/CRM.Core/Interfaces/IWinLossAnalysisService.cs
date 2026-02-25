// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Win/Loss Analysis Service interface (TODO-CRM003-05).
/// Provides analytics on won and lost opportunities.
/// </summary>
public interface IWinLossAnalysisService
{
    /// <summary>
    /// Gets an overall win/loss summary for the specified date range.
    /// </summary>
    Task<WinLossSummary> GetSummaryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets losses grouped by LossReasonCategory.
    /// </summary>
    Task<IEnumerable<WinLossByReason>> GetByReasonAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets win/loss stats grouped by competitor.
    /// </summary>
    Task<IEnumerable<WinLossByCompetitor>> GetByCompetitorAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets win rate trends over time.
    /// </summary>
    Task<IEnumerable<WinRateTrend>> GetWinRateTrendsAsync(
        DateTime fromDate,
        DateTime toDate,
        string period = "month",
        CancellationToken ct = default);

    /// <summary>
    /// Gets win/loss stats grouped by sales rep.
    /// </summary>
    Task<IEnumerable<WinLossBySalesRep>> GetBySalesRepAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a comprehensive loss analysis report.
    /// </summary>
    Task<LossAnalysisReport> GetLossAnalysisAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets win/loss stats by deal size segments.
    /// </summary>
    Task<IEnumerable<WinLossByDealSize>> GetByDealSizeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);
}

#region Model Classes

/// <summary>
/// Overall win/loss summary.
/// </summary>
public class WinLossSummary
{
    public int TotalOpportunities { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int StillOpen { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWonAmount { get; set; }
    public decimal TotalLostAmount { get; set; }
    public decimal AverageWonDealSize { get; set; }
    public decimal AverageLostDealSize { get; set; }
    public double AverageDaysToWin { get; set; }
    public double AverageDaysToLose { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Win/loss grouped by loss reason category.
/// </summary>
public class WinLossByReason
{
    public LossReasonCategory ReasonCategory { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Win/loss stats grouped by competitor.
/// </summary>
public class WinLossByCompetitor
{
    public int CompetitorId { get; set; }
    public string CompetitorName { get; set; } = string.Empty;
    public int TotalDeals { get; set; }
    public int WinsAgainst { get; set; }
    public int LossesTo { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWonAmount { get; set; }
    public decimal TotalLostAmount { get; set; }
}

/// <summary>
/// Win rate trend data point for a time period.
/// </summary>
public class WinRateTrend
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalOpportunities { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal WinRate { get; set; }
    public decimal WonAmount { get; set; }
    public decimal LostAmount { get; set; }
}

/// <summary>
/// Win/loss stats grouped by sales rep.
/// </summary>
public class WinLossBySalesRep
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalOpportunities { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWonAmount { get; set; }
    public decimal TotalLostAmount { get; set; }
    public decimal AverageDealSize { get; set; }
}

/// <summary>
/// Comprehensive loss analysis report.
/// </summary>
public class LossAnalysisReport
{
    public int TotalLosses { get; set; }
    public decimal TotalLostAmount { get; set; }
    public IEnumerable<WinLossByReason> ByReason { get; set; } = Enumerable.Empty<WinLossByReason>();
    public IEnumerable<WinLossByCompetitor> ByCompetitor { get; set; } = Enumerable.Empty<WinLossByCompetitor>();
    public string TopLossReason { get; set; } = "N/A";
    public string TopCompetitor { get; set; } = "N/A";
    public double AverageDaysToLose { get; set; }
    public List<LostOpportunityDetail> RecentLosses { get; set; } = new();
}

/// <summary>
/// Detail of a lost opportunity in a loss analysis report.
/// </summary>
public class LostOpportunityDetail
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? ClosedDate { get; set; }
    public LossReasonCategory? LossReasonCategory { get; set; }
    public string? LossReason { get; set; }
    public string? CompetitorWinner { get; set; }
    public string? SalesOwner { get; set; }
}

/// <summary>
/// Win/loss stats by deal size segment.
/// </summary>
public class WinLossByDealSize
{
    public string Segment { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int TotalDeals { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWonAmount { get; set; }
    public decimal TotalLostAmount { get; set; }
}

/// <summary>
/// Consolidated win/loss report for GET /api/reports/win-loss (TODO-CRM003-05).
/// Combines summary, by-stage, by-competitor, and time-period breakdown.
/// </summary>
public class WinLossReportDto
{
    /// <summary>Overall summary statistics.</summary>
    public WinLossSummary Summary { get; set; } = new();

    /// <summary>Win/loss counts and rates per loss reason category.</summary>
    public IEnumerable<WinLossByReason> ByReason { get; set; } = Enumerable.Empty<WinLossByReason>();

    /// <summary>Win/loss counts per competitor.</summary>
    public IEnumerable<WinLossByCompetitor> ByCompetitor { get; set; } = Enumerable.Empty<WinLossByCompetitor>();

    /// <summary>Monthly / quarterly win rate trend.</summary>
    public IEnumerable<WinRateTrend> Trends { get; set; } = Enumerable.Empty<WinRateTrend>();

    /// <summary>Date range used for this report.</summary>
    public DateTime FromDate { get; set; }

    /// <summary>Date range used for this report.</summary>
    public DateTime ToDate { get; set; }
}

#endregion
