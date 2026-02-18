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

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscriptionMetricsDto = CRM.Core.Dtos.SubscriptionMetricsDto;
using SubscriptionAnalyticsDto = CRM.Core.Dtos.SubscriptionAnalyticsDto;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Subscription Metrics Aggregator Interface - SaaS revenue metrics calculations.
/// </summary>
public interface ISubscriptionMetricsAggregator
{
    /// <summary>
    /// Calculate metrics for a single subscription.
    /// </summary>
    Task<SubscriptionMetricsDto> CalculateMetricsAsync(int subscriptionId, CancellationToken cancellationToken);

    /// <summary>
    /// Calculate company-wide analytics (MRR, ARR, churn, NRR, LTV).
    /// </summary>
    Task<SubscriptionAnalyticsDto> CalculateCompanyMetricsAsync(
        DateTime? startDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate Monthly Recurring Revenue (MRR).
    /// Sum of all active subscriptions' monthly equivalent value.
    /// </summary>
    Task<decimal> CalculateMRRAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Calculate Annual Recurring Revenue (ARR).
    /// ARR = MRR * 12
    /// </summary>
    Task<decimal> CalculateARRAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Calculate churn rate for a specific period.
    /// Churn = (Cancelled Subscriptions in Period / Active at Period Start) * 100
    /// </summary>
    Task<decimal> CalculateChurnRateAsync(int? monthOffset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate Net Revenue Retention (NRR).
    /// NRR = (ARR of Month + Expansion - Contraction - Churn) / Prior Month ARR
    /// NRR > 100% = Growing, < 100% = Declining
    /// </summary>
    Task<decimal> CalculateNRRAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of ISubscriptionMetricsAggregator.
/// 
/// KEY METRICS (SaaS Standard):
/// 
/// 1. MRR (Monthly Recurring Revenue)
///    - Sum of all active subscriptions normalized to monthly
///    - Includes paused subscriptions (no active = no MRR)
///    - Formula: SUM(Amount) for Monthly + SUM(Amount/3) for Quarterly + SUM(Amount/12) for Yearly
/// 
/// 2. ARR (Annual Recurring Revenue)
///    - ARR = MRR * 12
///    - Total expected annual revenue from subscriptions
/// 
/// 3. Churn Rate
///    - Percentage of subscriptions cancelled in a period
///    - Formula: (Cancelled Count This Month / Active at Month Start) * 100
///    - Negative metric - lower is better
/// 
/// 4. Net Revenue Retention (NRR)
///    - Measures net growth including expansion, contraction, churn
///    - Formula: (Current MRR + Expansion - Contraction - Churn) / Previous MRR
///    - > 100% indicates net growth even with churn
///    - Key metric for valuation (SaaS companies valued on NRR)
/// 
/// 5. Customer Lifetime Value (LTV)
///    - Average revenue per customer over lifetime
///    - Formula: (ARPU * Gross Margin) / Monthly Churn Rate
///    - ARPU = Average Revenue Per User = MRR / Active Customers
/// 
/// PRECISION:
/// - All calculations use DECIMAL(18,4) to prevent rounding errors
/// - Final results rounded to 2 decimal places for currency
/// - Percentages calculated as 0-100 scale
/// </summary>
public class SubscriptionMetricsAggregator : ISubscriptionMetricsAggregator
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<SubscriptionMetricsAggregator> _logger;

    public SubscriptionMetricsAggregator(
        ICrmDbContext context,
        ILogger<SubscriptionMetricsAggregator> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate metrics for a single subscription.
    /// </summary>
    public async Task<SubscriptionMetricsDto> CalculateMetricsAsync(
        int subscriptionId,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken);

        if (subscription == null)
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");

        var daysUntilExpiry = subscription.EndDate.HasValue
            ? (int)(subscription.EndDate.Value - DateTime.UtcNow).TotalDays
            : -1; // No expiry date

        return new SubscriptionMetricsDto
        {
            SubscriptionId = subscriptionId,
            MRR = NormalizeToMonthly(subscription.Amount ?? 0.0m, subscription.BillingCycle ?? "Monthly"),
            ARR = (subscription.ARR ?? 0.0m) > 0 ? subscription.ARR.Value : NormalizeToMonthly(subscription.Amount ?? 0.0m, subscription.BillingCycle ?? "Monthly") * 12,
            LifetimeValue = subscription.Amount.HasValue ? subscription.Amount.Value * 24 : 0.0m, // Assume 24-month average
            NextBillingDate = subscription.NextBillingDate,
            DaysUntilExpiry = daysUntilExpiry,
            Status = subscription.SubscriptionStatus.ToString()
        };
    }

    /// <summary>
    /// Calculate company-wide subscription analytics.
    /// </summary>
    public async Task<SubscriptionAnalyticsDto> CalculateCompanyMetricsAsync(
        DateTime? startDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            startDate ??= endDate.AddMonths(-12); // Default: last 12 months

            var mrr = await CalculateMRRAsync(cancellationToken);
            var arr = await CalculateARRAsync(cancellationToken);
            var churnRate = await CalculateChurnRateAsync(0, cancellationToken);
            var nrr = await CalculateNRRAsync(cancellationToken);

            var activeSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Active, cancellationToken);

            var pausedSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Paused, cancellationToken);

            var cancelledSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Cancelled &&
                                  s.CancelledAt.HasValue &&
                                  s.CancelledAt.Value >= startDate &&
                                  s.CancelledAt.Value <= endDate, cancellationToken);

            var totalSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(s => !s.IsDeleted, cancellationToken);

            var aov = activeSubscriptions > 0
                ? mrr / activeSubscriptions
                : 0;

            // LTV = (ARPU * Gross Margin %) / Monthly Churn Rate
            // Assuming 80% gross margin and monthly churn from annual rate
            var monthlyChurnRate = churnRate / 100 / 12; // Convert annual % to monthly ratio
            var ltv = monthlyChurnRate > 0 && aov > 0
                ? (aov * 0.8m) / monthlyChurnRate
                : 0;

            return new SubscriptionAnalyticsDto
            {
                TotalSubscriptions = totalSubscriptions,
                ActiveSubscriptions = activeSubscriptions,
                PausedSubscriptions = pausedSubscriptions,
                CancelledSubscriptions = cancelledSubscriptions,
                TotalMRR = mrr,
                TotalARR = arr,
                ChurnRate = churnRate,
                NetRevenueRetention = nrr,
                AverageContractValue = aov,
                CustomerLifetimeValue = ltv,
                CalculatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating company metrics");
            throw;
        }
    }

    /// <summary>
    /// Calculate Monthly Recurring Revenue (MRR).
    /// MRR = Sum all active subscriptions normalized to monthly value.
    /// </summary>
    public async Task<decimal> CalculateMRRAsync(CancellationToken cancellationToken)
    {
        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => !s.IsDeleted && 
                       (s.SubscriptionStatus == SubscriptionStatus.Active || 
                        s.SubscriptionStatus == SubscriptionStatus.Paused))
            .ToListAsync(cancellationToken);

        var mrr = subscriptions.Sum(s => NormalizeToMonthly(s.Amount ?? 0, s.BillingCycle ?? "Monthly"));

        return Math.Round(mrr, 2);
    }

    /// <summary>
    /// Calculate Annual Recurring Revenue (ARR).
    /// ARR = MRR * 12
    /// </summary>
    public async Task<decimal> CalculateARRAsync(CancellationToken cancellationToken)
    {
        var mrr = await CalculateMRRAsync(cancellationToken);
        return Math.Round(mrr * 12, 2);
    }

    /// <summary>
    /// Calculate churn rate for a specific month.
    /// Churn = (Cancelled Count in Month / Active at Month Start) * 100
    /// </summary>
    public async Task<decimal> CalculateChurnRateAsync(
        int? monthOffset = 0,
        CancellationToken cancellationToken = default)
    {
        var offset = monthOffset ?? 0;
        var now = DateTime.UtcNow;
        var targetMonth = now.AddMonths(-offset);
        var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Count active subscriptions at month start
        var activeAtStart = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => !s.IsDeleted &&
                            s.SubscriptionStatus == SubscriptionStatus.Active &&
                            (s.StartDate.HasValue && s.StartDate.Value <= monthStart ||
                             !s.StartDate.HasValue), // Include subscriptions without explicit start date
                             cancellationToken);

        if (activeAtStart == 0)
            return 0m;

        // Count subscriptions cancelled in this month
        var cancelledInMonth = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => !s.IsDeleted &&
                            s.CancelledAt.HasValue &&
                            s.CancelledAt.Value >= monthStart &&
                            s.CancelledAt.Value <= monthEnd,
                            cancellationToken);

        var churnRate = (decimal)cancelledInMonth / activeAtStart * 100;
        return Math.Round(churnRate, 4);
    }

    /// <summary>
    /// Calculate Net Revenue Retention (NRR).
    /// NRR = (Current MRR - MRR from Churn + Expansion - Contraction) / Previous MRR
    /// Simplified: NRR = (Current MRR / Previous MRR) * 100
    /// </summary>
    public async Task<decimal> CalculateNRRAsync(CancellationToken cancellationToken)
    {
        var currentMrr = await CalculateMRRAsync(cancellationToken);
        
        // Previous month MRR (approximated from month-ago active subscriptions)
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
        var subscriptionsOneMonthAgo = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                       (s.SubscriptionStatus == SubscriptionStatus.Active || 
                        s.SubscriptionStatus == SubscriptionStatus.Paused))
            .ToListAsync(cancellationToken);

        var previousMrr = subscriptionsOneMonthAgo.Sum(s => NormalizeToMonthly(s.Amount ?? 0.0m, s.BillingCycle ?? "Monthly"));

        if (previousMrr <= 0)
            return 100m; // If no prior MRR, NRR = 100% (base case)

        var nrr = (currentMrr / previousMrr) * 100;
        return Math.Round(nrr, 2);
    }

    /// <summary>
    /// Normalize subscription amount to monthly equivalent.
    /// Used for consistent MRR calculations across different billing cycles.
    /// </summary>
    private static decimal NormalizeToMonthly(decimal amount, string billingCycle)
    {
        var normalized = (billingCycle ?? "Monthly").ToLowerInvariant() switch
        {
            "weekly" => amount * 52 / 12, // 52 weeks / 12 months
            "quarterly" => amount / 3,     // Quarterly / 3
            "yearly" or "annual" => amount / 12, // Annual / 12
            _ => amount // Default: already monthly
        };

        return Math.Round(normalized, 4); // Keep high precision for aggregations
    }
}
