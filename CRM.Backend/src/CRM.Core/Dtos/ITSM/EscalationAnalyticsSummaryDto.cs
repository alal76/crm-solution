// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.DTOs.ITSM;

/// <summary>
/// Consolidated 30-day escalation analytics summary.
/// Returned by GET /api/escalationanalytics/summary.
/// TODO-SD005-011: Escalation Analytics Reports.
/// </summary>
public class EscalationAnalyticsSummaryDto
{
    /// <summary>Start of the reporting window (30 days ago).</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>End of the reporting window (now).</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Total escalations in the period.</summary>
    public int TotalEscalations { get; set; }

    /// <summary>Total service requests created in the period.</summary>
    public int TotalServiceRequests { get; set; }

    /// <summary>Overall escalation rate as a percentage.</summary>
    public double OverallEscalationRate { get; set; }

    /// <summary>Average time-to-escalate by severity/priority.</summary>
    public List<EscalationTimeBySeverityDto> AverageTimeToEscalateBySeverity { get; set; } = new();

    /// <summary>Escalation rate grouped by service request category.</summary>
    public List<EscalationRateByCategoryDto> EscalationRateByCategory { get; set; } = new();

    /// <summary>Top 5 most-escalated request types.</summary>
    public List<TopEscalatedRequestTypeDto> TopEscalatedRequestTypes { get; set; } = new();

    /// <summary>Resolution rate of service requests that were escalated.</summary>
    public double ResolutionRateAfterEscalation { get; set; }

    /// <summary>When the report was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Average escalation time broken down by priority/severity.
/// </summary>
public class EscalationTimeBySeverityDto
{
    /// <summary>Priority name (e.g., Critical, High, Medium, Low).</summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>Number of escalations for this priority.</summary>
    public int EscalationCount { get; set; }

    /// <summary>Average minutes from SR creation to first escalation.</summary>
    public double AverageMinutesToEscalate { get; set; }
}

/// <summary>
/// Escalation rate grouped by service request category.
/// </summary>
public class EscalationRateByCategoryDto
{
    /// <summary>Category identifier.</summary>
    public int CategoryId { get; set; }

    /// <summary>Category name.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Total service requests in this category.</summary>
    public int TotalRequests { get; set; }

    /// <summary>Number of escalated requests in this category.</summary>
    public int EscalatedRequests { get; set; }

    /// <summary>Escalation rate as a percentage.</summary>
    public double EscalationRate { get; set; }
}

/// <summary>
/// One entry in the top-5 most-escalated request types list.
/// </summary>
public class TopEscalatedRequestTypeDto
{
    /// <summary>Display rank (1–5).</summary>
    public int Rank { get; set; }

    /// <summary>Category name.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Number of escalations.</summary>
    public int EscalationCount { get; set; }

    /// <summary>Percentage of total escalations.</summary>
    public double PercentageOfTotal { get; set; }
}
