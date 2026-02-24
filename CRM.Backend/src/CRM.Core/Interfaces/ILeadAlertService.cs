// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Lead alert service for stale leads and aging notifications.
/// TODO-CRM002-07: Add lead aging alerts and stale lead notifications
/// </summary>
public interface ILeadAlertService
{
    /// <summary>
    /// Check for stale leads that haven't been contacted.
    /// </summary>
    /// <param name="staleDaysThreshold">Days since last contact to consider stale</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of stale lead alerts</returns>
    Task<IEnumerable<StaleLeadAlert>> CheckStaleLeadsAsync(
        int staleDaysThreshold = 7,
        CancellationToken ct = default);

    /// <summary>
    /// Get leads that are aging without progress.
    /// </summary>
    /// <param name="agingDaysThreshold">Days since status change to consider aging</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of aging leads</returns>
    Task<IEnumerable<AgingLeadAlert>> GetAgingLeadsAsync(
        int agingDaysThreshold = 14,
        CancellationToken ct = default);

    /// <summary>
    /// Get leads at risk based on engagement drop.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of at-risk leads</returns>
    Task<IEnumerable<AtRiskLeadAlert>> GetAtRiskLeadsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get lead alert statistics for dashboard.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Alert statistics</returns>
    Task<LeadAlertStatistics> GetAlertStatisticsAsync(CancellationToken ct = default);

    /// <summary>
    /// Mark a lead as contacted to reset stale timer.
    /// </summary>
    /// <param name="leadId">Lead ID</param>
    /// <param name="ct">Cancellation token</param>
    Task MarkLeadContactedAsync(int leadId, CancellationToken ct = default);

    /// <summary>
    /// Send stale lead notifications to owners.
    /// </summary>
    /// <param name="staleDaysThreshold">Threshold for stale determination</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of notifications sent</returns>
    Task<int> SendStaleLeadNotificationsAsync(int staleDaysThreshold = 7, CancellationToken ct = default);
}

#region DTOs

/// <summary>
/// Stale lead alert information.
/// </summary>
public class StaleLeadAlert
{
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public LeadLifecycleStatus Status { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public int DaysSinceLastContact { get; set; }
    public int Score { get; set; }
    public string AlertLevel { get; set; } = "Medium"; // Low, Medium, High, Critical
}

/// <summary>
/// Aging lead alert information.
/// </summary>
public class AgingLeadAlert
{
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public LeadLifecycleStatus Status { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysInCurrentStatus { get; set; }
    public int TotalAgeDays { get; set; }
    public string AlertLevel { get; set; } = "Medium";
}

/// <summary>
/// At-risk lead alert information.
/// </summary>
public class AtRiskLeadAlert
{
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public int CurrentScore { get; set; }
    public int PreviousScore { get; set; }
    public int ScoreDropPercent { get; set; }
    public string RiskReason { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
}

/// <summary>
/// Lead alert statistics for dashboard.
/// </summary>
public class LeadAlertStatistics
{
    public int TotalStaleLeads { get; set; }
    public int TotalAgingLeads { get; set; }
    public int TotalAtRiskLeads { get; set; }
    public int LeadsNeverContacted { get; set; }
    public double AverageDaysSinceContact { get; set; }
    public int HighPriorityAlerts { get; set; }
    public Dictionary<string, int> AlertsByOwner { get; set; } = new();
    public Dictionary<LeadLifecycleStatus, int> StaleByStatus { get; set; } = new();
}

#endregion
