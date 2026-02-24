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
