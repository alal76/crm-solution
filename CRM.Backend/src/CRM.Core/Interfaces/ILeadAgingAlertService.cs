// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Lead Aging Alert Service Interface (TODO-CRM002-07)
/// Identifies stale leads that need attention based on configurable thresholds.
/// </summary>
public interface ILeadAgingAlertService
{
    /// <summary>
    /// Gets leads that haven't been touched in the specified number of days.
    /// </summary>
    /// <param name="daysThreshold">Days since last activity to consider stale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stale leads</returns>
    Task<IEnumerable<Lead>> GetStaleLeadsAsync(int daysThreshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stale leads for a specific owner.
    /// </summary>
    /// <param name="ownerId">Owner user ID</param>
    /// <param name="daysThreshold">Days since last activity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stale leads owned by user</returns>
    Task<IEnumerable<Lead>> GetStaleLeadsByOwnerAsync(int ownerId, int daysThreshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets lead aging statistics (bucket distribution).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aging statistics</returns>
    Task<LeadAgingStatistics> GetAgingStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stale leads as alert DTOs with staleness classification.
    /// "Warning" = ≥ staleDaysThreshold days, "Critical" = ≥ 30 days (or 2× threshold when threshold ≥ 30).
    /// TODO-CRM002-07
    /// </summary>
    /// <param name="staleDaysThreshold">Days since last activity to consider a lead stale (minimum for Warning)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of <see cref="LeadAgingAlertDto"/> for all stale leads</returns>
    Task<IEnumerable<LeadAgingAlertDto>> GetStaledLeadsAsync(int staleDaysThreshold, CancellationToken ct = default);
}

/// <summary>
/// DTO returned by the lead aging alert endpoint (TODO-CRM002-07).
/// </summary>
public class LeadAgingAlertDto
{
    /// <summary>Lead identifier</summary>
    public int LeadId { get; set; }

    /// <summary>Full name of the lead</summary>
    public string LeadName { get; set; } = string.Empty;

    /// <summary>Assigned owner user ID (nullable)</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>Number of days since the lead's last activity</summary>
    public int DaysSinceLastActivity { get; set; }

    /// <summary>Date of last recorded activity (null if never active)</summary>
    public DateTime? LastActivityDate { get; set; }

    /// <summary>"Warning" (≥ threshold) or "Critical" (≥ critical threshold)</summary>
    public string StalenessLevel { get; set; } = "Warning";
}

/// <summary>
/// Lead aging statistics showing distribution of leads by age bucket.
/// </summary>
public class LeadAgingStatistics
{
    /// <summary>Total open (non-converted, non-disqualified) leads</summary>
    public int TotalOpenLeads { get; set; }

    /// <summary>Leads active within the last 7 days</summary>
    public int Under7Days { get; set; }

    /// <summary>Leads aged 7-14 days</summary>
    public int Days7To14 { get; set; }

    /// <summary>Leads aged 15-30 days</summary>
    public int Days15To30 { get; set; }

    /// <summary>Leads aged 31-60 days</summary>
    public int Days31To60 { get; set; }

    /// <summary>Leads older than 60 days</summary>
    public int Over60Days { get; set; }

    /// <summary>Average age in days across all open leads</summary>
    public double AverageAgeDays { get; set; }
}
