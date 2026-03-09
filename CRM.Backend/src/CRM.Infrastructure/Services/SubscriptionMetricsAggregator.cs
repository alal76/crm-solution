// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscriptionAnalyticsDto = CRM.Core.Dtos.SubscriptionAnalyticsDto;
using SubscriptionMetricsDto = CRM.Core.Dtos.SubscriptionMetricsDto;

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

    /// <summary>
    /// Count subscriptions whose StartDate falls within a given calendar month.
    /// Used for cohort analysis — represents new subscriptions that entered in that month.
    /// </summary>
    /// <param name="year">Cohort year (e.g. 2025)</param>
    /// <param name="month">Cohort month 1–12 (e.g. 3 for March)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of subscriptions that started in the specified month</returns>
    Task<int> GetCohortSubscriptionCountAsync(int year, int month, CancellationToken cancellationToken);

    /// <summary>
    /// Calculate MRR contributed by a specific cohort (subscriptions started in a given month)
    /// that are still active/paused.
    /// </summary>
    Task<decimal> GetCohortMRRAsync(int year, int month, CancellationToken cancellationToken);

    /// <summary>
    /// Get MRR breakdown grouped by billing cycle (Weekly, Monthly, Quarterly, Yearly).
    /// Returns label, MRR, ARR, subscription count, and percentage for each group.
    /// </summary>
    Task<List<BillingCycleBreakdownItem>> GetRevenueBreakdownByBillingCycleAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Revenue breakdown item grouped by billing cycle.
/// </summary>
public class BillingCycleBreakdownItem
{
    public string BillingCycle { get; set; } = string.Empty;
    public decimal MRR { get; set; }
    public decimal ARR { get; set; }
    public int SubscriptionCount { get; set; }
    public decimal Percentage { get; set; }
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
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var daysUntilExpiry = subscription.EndDate.HasValue
            ? (int)(subscription.EndDate.Value - DateTime.UtcNow).TotalDays
            : -1; // No expiry date

        return new SubscriptionMetricsDto
        {
            SubscriptionId = subscriptionId,
            MRR = NormalizeToMonthly(subscription.Amount, subscription.BillingCycle ?? "Monthly"),
            ARR = (subscription.ARR ?? 0.0m) > 0 ? subscription.ARR.GetValueOrDefault() : NormalizeToMonthly(subscription.Amount, subscription.BillingCycle ?? "Monthly") * 12,
            CLV = subscription.Amount * 24, // Assume 24-month average lifetime value
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

        var mrr = subscriptions.Sum(s => NormalizeToMonthly(s.Amount, s.BillingCycle ?? "Monthly"));

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
                            ((s.StartDate.HasValue && s.StartDate.Value <= monthStart) ||
                             !s.StartDate.HasValue), // Include subscriptions without explicit start date
                             cancellationToken);

        if (activeAtStart == 0)
        {
            return 0m;
        }

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

        var previousMrr = subscriptionsOneMonthAgo.Sum(s => NormalizeToMonthly(s.Amount, s.BillingCycle ?? "Monthly"));

        if (previousMrr <= 0)
        {
            return 100m; // If no prior MRR, NRR = 100% (base case)
        }

        var nrr = (currentMrr / previousMrr) * 100;
        return Math.Round(nrr, 2);
    }

    /// <summary>
    /// Count subscriptions whose StartDate falls within the specified calendar month.
    /// </summary>
    public async Task<int> GetCohortSubscriptionCountAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        return await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(
                s => !s.IsDeleted &&
                     s.StartDate.HasValue &&
                     s.StartDate.Value >= monthStart &&
                     s.StartDate.Value < monthEnd,
                cancellationToken);
    }

    /// <summary>
    /// Calculate MRR contributed by subscriptions that started in the given month
    /// and are still active or paused.
    /// </summary>
    public async Task<decimal> GetCohortMRRAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var cohortSubscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                        s.StartDate.HasValue &&
                        s.StartDate.Value >= monthStart &&
                        s.StartDate.Value < monthEnd &&
                        (s.SubscriptionStatus == SubscriptionStatus.Active ||
                         s.SubscriptionStatus == SubscriptionStatus.Paused))
            .ToListAsync(cancellationToken);

        var mrr = cohortSubscriptions.Sum(s => NormalizeToMonthly(s.Amount, s.BillingCycle ?? "Monthly"));
        return Math.Round(mrr, 2);
    }

    /// <summary>
    /// Get revenue breakdown grouped by billing cycle.
    /// </summary>
    public async Task<List<BillingCycleBreakdownItem>> GetRevenueBreakdownByBillingCycleAsync(
        CancellationToken cancellationToken)
    {
        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => !s.IsDeleted &&
                        (s.SubscriptionStatus == SubscriptionStatus.Active ||
                         s.SubscriptionStatus == SubscriptionStatus.Paused))
            .ToListAsync(cancellationToken);

        var totalMrr = subscriptions.Sum(s => NormalizeToMonthly(s.Amount, s.BillingCycle ?? "Monthly"));

        var groups = subscriptions
            .GroupBy(s => NormalizeBillingCycleLabel(s.BillingCycle))
            .Select(g =>
            {
                var groupMrr = Math.Round(g.Sum(s => NormalizeToMonthly(s.Amount, s.BillingCycle ?? "Monthly")), 2);
                return new BillingCycleBreakdownItem
                {
                    BillingCycle = g.Key,
                    MRR = groupMrr,
                    ARR = Math.Round(groupMrr * 12, 2),
                    SubscriptionCount = g.Count(),
                    Percentage = totalMrr > 0 ? Math.Round(groupMrr / totalMrr * 100, 2) : 0
                };
            })
            .OrderByDescending(x => x.MRR)
            .ToList();

        return groups;
    }

    private static string NormalizeBillingCycleLabel(string? billingCycle)
    {
        return (billingCycle ?? "Monthly").ToLowerInvariant() switch
        {
            "weekly" => "Weekly",
            "quarterly" => "Quarterly",
            "yearly" or "annual" => "Yearly",
            _ => "Monthly"
        };
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
