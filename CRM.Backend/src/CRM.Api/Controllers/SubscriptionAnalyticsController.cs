// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for subscription analytics: MRR, ARR, churn, growth, cohorts, revenue breakdown.
/// TODO-SALES006-040
/// </summary>
[ApiController]
[Route("api/subscriptions/analytics")]
[Authorize]
[Produces("application/json")]
public class SubscriptionAnalyticsController : CrmControllerBase
{
    private readonly ISubscriptionMetricsAggregator _metricsAggregator;

    public SubscriptionAnalyticsController(
        ISubscriptionMetricsAggregator metricsAggregator)
    {
        _metricsAggregator = metricsAggregator ?? throw new ArgumentNullException(nameof(metricsAggregator));
    }

    /// <summary>
    /// Get Monthly Recurring Revenue (MRR).
    /// GET /api/subscriptions/analytics/mrr
    /// </summary>
    [HttpGet("mrr")]
    [ProducesResponseType(typeof(RevenueMetricResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevenueMetricResponse>> GetMRR(
        CancellationToken cancellationToken = default)
    {
                var mrr = await _metricsAggregator.CalculateMRRAsync(cancellationToken);

        return Ok(new RevenueMetricResponse
        {
            MetricType = "MRR",
            Value = mrr,
            Description = "Monthly Recurring Revenue — sum of all active subscriptions normalized to monthly value.",
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get Annual Recurring Revenue (ARR).
    /// GET /api/subscriptions/analytics/arr
    /// </summary>
    [HttpGet("arr")]
    [ProducesResponseType(typeof(RevenueMetricResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevenueMetricResponse>> GetARR(
        CancellationToken cancellationToken = default)
    {
                var arr = await _metricsAggregator.CalculateARRAsync(cancellationToken);

        return Ok(new RevenueMetricResponse
        {
            MetricType = "ARR",
            Value = arr,
            Description = "Annual Recurring Revenue = MRR × 12.",
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get churn rate metrics.
    /// GET /api/subscriptions/analytics/churn
    /// </summary>
    [HttpGet("churn")]
    [ProducesResponseType(typeof(ChurnRateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChurnRateResponse>> GetChurnRate(
        [FromQuery] int monthsBack = 3,
        CancellationToken cancellationToken = default)
    {
                var history = new List<MonthlyChurnItem>();
        for (var i = monthsBack - 1; i >= 0; i--)
        {
            var rate = await _metricsAggregator.CalculateChurnRateAsync(i, cancellationToken);
            var targetDate = DateTime.UtcNow.AddMonths(-i);
            history.Add(new MonthlyChurnItem
            {
                Month = targetDate.ToString("yyyy-MM"),
                ChurnRate = rate
            });
        }

        var currentChurn = history.LastOrDefault()?.ChurnRate ?? 0;

        return Ok(new ChurnRateResponse
        {
            CurrentChurnRate = currentChurn,
            MonthlyHistory = history,
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get subscription growth metrics.
    /// GET /api/subscriptions/analytics/growth
    /// </summary>
    [HttpGet("growth")]
    [ProducesResponseType(typeof(GrowthMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GrowthMetricsResponse>> GetGrowthMetrics(
        CancellationToken cancellationToken = default)
    {
                var currentMRR = await _metricsAggregator.CalculateMRRAsync(cancellationToken);
        var currentARR = await _metricsAggregator.CalculateARRAsync(cancellationToken);
        var nrr = await _metricsAggregator.CalculateNRRAsync(cancellationToken);
        var churnRate = await _metricsAggregator.CalculateChurnRateAsync(0, cancellationToken);
        var companyMetrics = await _metricsAggregator.CalculateCompanyMetricsAsync(null, cancellationToken);

        return Ok(new GrowthMetricsResponse
        {
            CurrentMRR = currentMRR,
            CurrentARR = currentARR,
            NetRevenueRetention = nrr,
            ChurnRate = churnRate,
            ActiveSubscriptions = companyMetrics.ActiveSubscriptions,
            TotalSubscriptions = companyMetrics.TotalSubscriptions,
            AverageContractValue = companyMetrics.AverageContractValue,
            CustomerLifetimeValue = companyMetrics.CustomerLifetimeValue,
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get MRR cohort analysis — MRR grouped by subscription start month.
    /// GET /api/subscriptions/analytics/cohorts
    /// </summary>
    [HttpGet("cohorts")]
    [ProducesResponseType(typeof(CohortAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CohortAnalysisResponse>> GetCohortAnalysis(
        [FromQuery] int monthsBack = 12,
        CancellationToken cancellationToken = default)
    {
                // TODO: Implement full cohort analysis (MRR by cohort) using historical subscription data. // NOSONAR
        // Cohort analysis requires querying subscriptions by their start month and tracking their MRR over time.
        // This is a stub that returns computed monthly MRR as a single-cohort approximation.
        var cohorts = new List<CohortItem>();
        for (var i = monthsBack - 1; i >= 0; i--)
        {
            var mrr = await _metricsAggregator.CalculateMRRAsync(cancellationToken);
            var targetDate = DateTime.UtcNow.AddMonths(-i);
            cohorts.Add(new CohortItem
            {
                CohortMonth = targetDate.ToString("yyyy-MM"),
                MRR = mrr,
                SubscriptionCount = await _metricsAggregator.GetCohortSubscriptionCountAsync(
                    targetDate.Year, targetDate.Month, cancellationToken)
            });
        }

        return Ok(new CohortAnalysisResponse
        {
            Cohorts = cohorts,
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get revenue breakdown by billing plan / product.
    /// GET /api/subscriptions/analytics/revenue-breakdown
    /// </summary>
    [HttpGet("revenue-breakdown")]
    [ProducesResponseType(typeof(RevenueBreakdownResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevenueBreakdownResponse>> GetRevenueBreakdown(
        CancellationToken cancellationToken = default)
    {
                var companyMetrics = await _metricsAggregator.CalculateCompanyMetricsAsync(null, cancellationToken);
        var mrr = await _metricsAggregator.CalculateMRRAsync(cancellationToken);
        var arr = await _metricsAggregator.CalculateARRAsync(cancellationToken);

        // TODO: Break down MRR by billing cycle (Weekly/Monthly/Quarterly/Yearly) and product // NOSONAR
        // when detailed subscription product data is included in the aggregator.
        return Ok(new RevenueBreakdownResponse
        {
            TotalMRR = mrr,
            TotalARR = arr,
            ActiveSubscriptions = companyMetrics.ActiveSubscriptions,
            // Placeholder — full breakdown requires grouping by ProductId/BillingCycle
            BreakdownItems = new List<RevenueBreakdownItem>
            {
                new()
                {
                    Label = "All Active Subscriptions",
                    MRR = mrr,
                    ARR = arr,
                    SubscriptionCount = companyMetrics.ActiveSubscriptions
                }
            },
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get comprehensive metrics dashboard.
    /// GET /api/subscriptions/analytics/dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(SubscriptionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionAnalyticsDto>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
                var metrics = await _metricsAggregator.CalculateCompanyMetricsAsync(null, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get Net Revenue Retention (NRR).
    /// GET /api/subscriptions/analytics/nrr
    /// </summary>
    [HttpGet("nrr")]
    [ProducesResponseType(typeof(RevenueMetricResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevenueMetricResponse>> GetNRR(
        CancellationToken cancellationToken = default)
    {
        var nrr = await _metricsAggregator.CalculateNRRAsync(cancellationToken);
        return Ok(new RevenueMetricResponse
        {
            MetricType = "NRR",
            Value = nrr,
            Description = "Net Revenue Retention — percentage of recurring revenue retained from existing customers, including expansion and contraction.",
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get subscription retention metrics.
    /// GET /api/subscriptions/analytics/retention
    /// </summary>
    [HttpGet("retention")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRetention(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var companyMetrics = await _metricsAggregator.CalculateCompanyMetricsAsync(null, cancellationToken);
        var nrr = await _metricsAggregator.CalculateNRRAsync(cancellationToken);

        return Ok(new
        {
            netRevenueRetention = nrr,
            activeSubscriptions = companyMetrics?.ActiveSubscriptions ?? 0,
            churnedSubscriptions = companyMetrics?.CancelledSubscriptions ?? 0,
            retentionRate = companyMetrics != null && companyMetrics.TotalSubscriptions > 0
                ? Math.Round((decimal)companyMetrics.ActiveSubscriptions / companyMetrics.TotalSubscriptions * 100, 2)
                : 0,
            period = $"{months} months",
            calculatedAt = DateTime.UtcNow
        });
    }

    #region Response DTOs

    public class RevenueMetricResponse
    {
        public string MetricType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Description { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    public class ChurnRateResponse
    {
        public decimal CurrentChurnRate { get; set; }
        public List<MonthlyChurnItem> MonthlyHistory { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class MonthlyChurnItem
    {
        public string Month { get; set; } = string.Empty;
        public decimal ChurnRate { get; set; }
    }

    public class GrowthMetricsResponse
    {
        public decimal CurrentMRR { get; set; }
        public decimal CurrentARR { get; set; }
        public decimal NetRevenueRetention { get; set; }
        public decimal ChurnRate { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalSubscriptions { get; set; }
        public decimal AverageContractValue { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    public class CohortAnalysisResponse
    {
        public List<CohortItem> Cohorts { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class CohortItem
    {
        public string CohortMonth { get; set; } = string.Empty;
        public decimal MRR { get; set; }
        public int SubscriptionCount { get; set; }
    }

    public class RevenueBreakdownResponse
    {
        public decimal TotalMRR { get; set; }
        public decimal TotalARR { get; set; }
        public int ActiveSubscriptions { get; set; }
        public List<RevenueBreakdownItem> BreakdownItems { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class RevenueBreakdownItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal MRR { get; set; }
        public decimal ARR { get; set; }
        public int SubscriptionCount { get; set; }
        public decimal? Percentage { get; set; }
    }

    #endregion
}
