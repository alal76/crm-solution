// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// Assembled content for a single user's email digest send (REV-FE-002).
/// Populated section-by-section by EmailDigestService.BuildDigestContentAsync
/// based on which sections are enabled in the user's EmailDigestConfig.
/// Null section = not requested/not included; empty list = requested but nothing to report.
/// </summary>
public class EmailDigestContentDto
{
    public int UserId { get; set; }

    public string UserDisplayName { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Start of the reporting window (since last digest, or a sensible default for previews).</summary>
    public DateTime PeriodStartUtc { get; set; }

    public List<EmailDigestLeadItem>? NewLeads { get; set; }

    public List<EmailDigestOpportunityItem>? OpenOpportunities { get; set; }

    public List<EmailDigestActivityItem>? RecentActivities { get; set; }

    public List<EmailDigestTaskItem>? UpcomingTasks { get; set; }

    public List<EmailDigestTaskItem>? OverdueTasks { get; set; }

    /// <summary>
    /// v1 scope: simple counts of deals closed and activities logged by the user's direct reports
    /// (department members), not a full analytics build. Null when the section is disabled or the
    /// user manages no one.
    /// </summary>
    public EmailDigestTeamPerformance? TeamPerformance { get; set; }

    /// <summary>
    /// v1 scope: a small set of obvious counts (open pipeline value/count, deals closed this period,
    /// tasks completed this period), not a full analytics build.
    /// </summary>
    public EmailDigestKpiSummary? KpiSummary { get; set; }
}

public class EmailDigestLeadItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmailDigestOpportunityItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public DateTime? ExpectedCloseDate { get; set; }
}

public class EmailDigestActivityItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
}

public class EmailDigestTaskItem
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = string.Empty;
}

/// <summary>v1 scope decision — see EmailDigestContentDto.TeamPerformance remarks.</summary>
public class EmailDigestTeamPerformance
{
    public int DirectReportCount { get; set; }
    public int DealsClosedByTeam { get; set; }
    public int ActivitiesLoggedByTeam { get; set; }
}

/// <summary>v1 scope decision — see EmailDigestContentDto.KpiSummary remarks.</summary>
public class EmailDigestKpiSummary
{
    public int OpenPipelineCount { get; set; }
    public decimal OpenPipelineValue { get; set; }
    public int DealsClosedWonThisPeriod { get; set; }
    public decimal RevenueClosedWonThisPeriod { get; set; }
    public int TasksCompletedThisPeriod { get; set; }
}
