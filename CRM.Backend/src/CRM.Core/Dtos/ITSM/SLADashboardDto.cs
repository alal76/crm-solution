// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for the SLA Dashboard endpoint providing aggregate SLA metrics.
/// </summary>
public class SLADashboardDto
{
    /// <summary>Total number of tickets in the queried period.</summary>
    public int TotalTickets { get; set; }

    /// <summary>Number of tickets within SLA compliance.</summary>
    public int WithinSLA { get; set; }

    /// <summary>Number of tickets that breached SLA.</summary>
    public int BreachedSLA { get; set; }

    /// <summary>SLA compliance rate as a percentage (0-100).</summary>
    public double ComplianceRate { get; set; }

    /// <summary>Average response time in minutes across all tickets.</summary>
    public double AvgResponseTimeMinutes { get; set; }

    /// <summary>Average resolution time in minutes across all tickets.</summary>
    public double AvgResolutionTimeMinutes { get; set; }

    /// <summary>Count of SLA breaches grouped by priority level.</summary>
    public Dictionary<string, int> BreachesByPriority { get; set; } = new();

    /// <summary>Daily SLA compliance trend data points.</summary>
    public List<SLATrendPoint> DailyTrend { get; set; } = new();
}

/// <summary>
/// A single data point in the SLA daily compliance trend.
/// </summary>
public class SLATrendPoint
{
    /// <summary>The date for this trend data point.</summary>
    public DateTime Date { get; set; }

    /// <summary>SLA compliance rate on this date (0-100).</summary>
    public double ComplianceRate { get; set; }

    /// <summary>Total tickets processed on this date.</summary>
    public int TotalTickets { get; set; }
}
