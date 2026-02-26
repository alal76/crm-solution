// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for Revenue Analytics — ARR/MRR tracking, growth rates, churn, and NRR.
/// </summary>
public interface IRevenueAnalyticsService
{
    /// <summary>Returns aggregated metrics including current MRR/ARR, growth, churn, and trend.</summary>
    Task<RevenueMetricsDto> GetMetricsAsync(DateTime? from, DateTime? to, CancellationToken ct = default);

    /// <summary>Returns the last <paramref name="months"/> monthly snapshots sorted ascending by date.</summary>
    Task<IEnumerable<RevenueSnapshotDto>> GetTrendAsync(int months, CancellationToken ct = default);

    /// <summary>Returns MRR waterfall movements for the last <paramref name="months"/> periods.</summary>
    Task<IEnumerable<RevenueMRRMovementDto>> GetMRRMovementsAsync(int months, CancellationToken ct = default);

    /// <summary>Returns the current MRR from the most recent snapshot (or calculated from subscriptions).</summary>
    Task<decimal> GetCurrentMRRAsync(CancellationToken ct = default);

    /// <summary>Returns the current ARR (= CurrentMRR * 12).</summary>
    Task<decimal> GetCurrentARRAsync(CancellationToken ct = default);

    /// <summary>Returns the churn rate as a percentage for the given date range.</summary>
    Task<decimal> GetChurnRateAsync(DateTime? from, DateTime? to, CancellationToken ct = default);

    /// <summary>Creates and persists a manual revenue snapshot.</summary>
    Task<RevenueSnapshotDto> CreateSnapshotAsync(CreateRevenueSnapshotDto dto, CancellationToken ct = default);

    /// <summary>Calculates a snapshot from live subscription data and persists it.</summary>
    Task<RevenueSnapshotDto> CalculateCurrentSnapshotAsync(CancellationToken ct = default);
}
