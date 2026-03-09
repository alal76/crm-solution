// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for SLA analytics and dashboard aggregation.
/// AP-021: extracted from SLAPoliciesController to eliminate fat-controller GroupBy analytics.
/// </summary>
public interface ISLAAnalyticsService
{
    /// <summary>
    /// Calculate SLA dashboard metrics (compliance rate, breach counts, trend data) for the given period.
    /// </summary>
    /// <param name="startDate">Period start (inclusive).</param>
    /// <param name="endDate">Period end (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated SLA dashboard data.</returns>
    Task<SLADashboardDto> GetDashboardAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
