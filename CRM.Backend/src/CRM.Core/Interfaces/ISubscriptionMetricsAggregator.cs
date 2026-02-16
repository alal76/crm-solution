// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Subscription Metrics Aggregator - Calculates key SaaS metrics for analytics and reporting.
/// 
/// Metrics:
/// - MRR: Monthly Recurring Revenue (sum of active subscriptions)
/// - ARR: Annual Recurring Revenue (MRR × 12)
/// - Churn Rate: (Cancelled subscriptions / starting subscriptions) × 100
/// - LTV: Customer Lifetime Value (ARPU × 1/churn rate)
/// - CAC: Customer Acquisition Cost (sales/marketing spend / new customers)
/// - NRR: Net Revenue Retention (includes upsells and downgrades)
/// 
/// Results cached in Redis with configurable TTL (default 1 hour).
/// 
/// SPEC: PHASE 6 - Subscription Billing Services (25 hours)
/// </summary>
public interface ISubscriptionMetricsAggregator
{
    /// <summary>
    /// Calculate Monthly Recurring Revenue (MRR).
    /// Sum of all active subscription monthly amounts as of a given date.
    /// </summary>
    /// <param name="asOfDate">Date to calculate MRR for (default: Today UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>MRR amount in base currency</returns>
    Task<decimal> CalculateMRRAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate Annual Recurring Revenue (ARR).
    /// = MRR × 12
    /// </summary>
    /// <param name="asOfDate">Date to calculate ARR for (default: Today UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ARR amount in base currency</returns>
    Task<decimal> CalculateARRAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate Churn Rate for a date range.
    /// = (Subscriptions Cancelled / Starting Subscriptions) × 100
    /// Expressed as percentage (e.g., 5.2 for 5.2%)
    /// </summary>
    /// <param name="startDate">Period start date</param>
    /// <param name="endDate">Period end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Churn rate percentage</returns>
    Task<decimal> CalculateChurnRateAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculate Customer Lifetime Value (LTV).
    /// = ARPU (Average Revenue Per User) × (1 / Churn Rate)
    /// Estimates total revenue expected from average customer over lifetime.
    /// </summary>
    /// <param name="asOfDate">Date to calculate LTV for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>LTV amount in base currency</returns>
    Task<decimal> CalculateLTVAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate Customer Acquisition Cost (CAC).
    /// = Total Sales & Marketing Spend / Number of New Customers Acquired
    /// Must be configured with spending data per period.
    /// </summary>
    /// <param name="startDate">Period start date</param>
    /// <param name="endDate">Period end date</param>
    /// <param name="totalSpend">Total sales & marketing spend in period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CAC amount in base currency</returns>
    Task<decimal> CalculateCACAsync(
        DateTime startDate,
        DateTime endDate,
        decimal totalSpend,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get comprehensive metrics dashboard with all key metrics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete metrics snapshot for period</returns>
    Task<SubscriptionMetricsDashboardDto> GetMetricsDashboardAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Get historical metrics for trending and forecasting.
    /// Returns metrics for last N months for graphing.
    /// </summary>
    /// <param name="monthsBack">Number of months to retrieve (default: 12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of monthly metrics</returns>
    Task<List<SubscriptionMetricsHistoryDto>> GetHistoricalMetricsAsync(
        int monthsBack = 12,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription breakdown by status.
    /// Shows active, paused, cancelled, expired counts.
    /// </summary>
    /// <param name="asOfDate">Date to calculate for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscription counts by status</returns>
    Task<SubscriptionBreakdownDto> GetSubscriptionBreakdownAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Comprehensive subscription metrics dashboard.
/// </summary>
public class SubscriptionMetricsDashboardDto
{
    public DateTime AsOfDate { get; set; }
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public decimal Churn { get; set; }
    public decimal LTV { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int CancelledThisMonth { get; set; }
    public int NewSubscriptionsThisMonth { get; set; }
    public decimal NetRevenueRetention { get; set; }
    public List<SubscriptionBreakdownItemDto> BreakdownByPlan { get; set; } = new();
}

/// <summary>
/// Metrics for a single month in historical trend.
/// </summary>
public class SubscriptionMetricsHistoryDto
{
    public string Month { get; set; } = string.Empty; // YYYY-MM format
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public decimal ChurnRate { get; set; }
    public int NewSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
}

/// <summary>
/// Breakdown of subscriptions by status.
/// </summary>
public class SubscriptionBreakdownDto
{
    public int Active { get; set; }
    public int Paused { get; set; }
    public int Cancelled { get; set; }
    public int Suspended { get; set; }
    public int Trial { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// Subscription metrics broken down by plan.
/// </summary>
public class SubscriptionBreakdownItemDto
{
    public string PlanName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
    public decimal PercentageOfMRR { get; set; }
}
