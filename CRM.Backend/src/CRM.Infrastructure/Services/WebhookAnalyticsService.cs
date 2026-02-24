// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IWebhookAnalyticsService for webhook analytics and metrics.
/// Provides success rate, latency, and failure analysis for webhook deliveries.
/// </summary>
public class WebhookAnalyticsService : IWebhookAnalyticsService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WebhookAnalyticsService> _logger;

    public WebhookAnalyticsService(ICrmDbContext context, ILogger<WebhookAnalyticsService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<double> GetSuccessRateAsync(int? webhookId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating success rate for webhook {WebhookId} from {StartDate} to {EndDate}",
            webhookId?.ToString() ?? "all", startDate, endDate);

        var query = _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= startDate && d.CreatedAt <= endDate);

        if (webhookId.HasValue)
            query = query.Where(d => d.WebhookSubscriptionId == webhookId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return 0;

        var successCount = await query.CountAsync(d => d.Success, cancellationToken);
        return successCount * 100.0 / totalCount;
    }

    /// <inheritdoc />
    public async Task<double> GetAverageLatencyAsync(int? webhookId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating average latency for webhook {WebhookId} from {StartDate} to {EndDate}",
            webhookId?.ToString() ?? "all", startDate, endDate);

        var query = _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= startDate && d.CreatedAt <= endDate && d.DurationMs.HasValue);

        if (webhookId.HasValue)
            query = query.Where(d => d.WebhookSubscriptionId == webhookId.Value);

        var avgLatency = await query
            .Select(d => d.DurationMs!.Value)
            .DefaultIfEmpty()
            .AverageAsync(cancellationToken);

        return avgLatency;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WebhookFailureInfo>> GetTopFailuresAsync(int count, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting top {Count} failing webhooks from {StartDate} to {EndDate}", count, startDate, endDate);

        var failedDeliveries = await _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => !d.IsDeleted && !d.Success && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
            .Include(d => d.Subscription)
            .ToListAsync(cancellationToken);

        var grouped = failedDeliveries
            .GroupBy(d => d.WebhookSubscriptionId)
            .Select(g => new WebhookFailureInfo
            {
                WebhookId = g.Key,
                WebhookName = g.First().Subscription?.Name ?? "Unknown",
                TargetUrl = g.First().TargetUrl,
                FailureCount = g.Count(),
                MostCommonError = g.Where(d => !string.IsNullOrEmpty(d.ErrorMessage))
                    .GroupBy(d => d.ErrorMessage)
                    .OrderByDescending(eg => eg.Count())
                    .Select(eg => eg.Key!)
                    .FirstOrDefault() ?? "Unknown error",
                LastFailureAt = g.Max(d => d.UpdatedAt).GetValueOrDefault(),
                ConsecutiveFailures = CalculateConsecutiveFailures(g.ToList()),
                MostCommonResponseCode = g.Where(d => d.ResponseStatusCode.HasValue)
                    .GroupBy(d => d.ResponseStatusCode!.Value)
                    .OrderByDescending(rg => rg.Count())
                    .Select(rg => rg.Key)
                    .FirstOrDefault()
            })
            .OrderByDescending(f => f.FailureCount)
            .Take(count)
            .ToList();

        return grouped;
    }

    /// <inheritdoc />
    public async Task<WebhookAnalyticsSummary> GetAnalyticsSummaryAsync(int webhookId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting analytics summary for webhook {WebhookId} from {StartDate} to {EndDate}", webhookId, startDate, endDate);

        var deliveries = await _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.WebhookSubscriptionId == webhookId && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalDeliveries = deliveries.Count;
        var successfulDeliveries = deliveries.Count(d => d.Success);
        var failedDeliveries = totalDeliveries - successfulDeliveries;

        var latencies = deliveries
            .Where(d => d.DurationMs.HasValue && d.DurationMs.Value > 0)
            .Select(d => (double)d.DurationMs!.Value)
            .OrderBy(l => l)
            .ToList();

        var eventTypeBreakdown = deliveries
            .GroupBy(d => d.EventType)
            .ToDictionary(g => g.Key, g => g.Count());

        var responseCodeDist = deliveries
            .Where(d => d.ResponseStatusCode.HasValue)
            .GroupBy(d => d.ResponseStatusCode!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalRetries = deliveries.Sum(d => Math.Max(0, d.AttemptNumber - 1));

        return new WebhookAnalyticsSummary
        {
            WebhookId = webhookId,
            TotalDeliveries = totalDeliveries,
            SuccessfulDeliveries = successfulDeliveries,
            FailedDeliveries = failedDeliveries,
            SuccessRate = totalDeliveries > 0 ? (successfulDeliveries * 100.0 / totalDeliveries) : 0,
            AverageLatencyMs = latencies.Count > 0 ? latencies.Average() : 0,
            P50LatencyMs = latencies.Count > 0 ? GetPercentile(latencies, 50) : 0,
            P95LatencyMs = latencies.Count > 0 ? GetPercentile(latencies, 95) : 0,
            P99LatencyMs = latencies.Count > 0 ? GetPercentile(latencies, 99) : 0,
            AverageRetries = totalDeliveries > 0 ? (double)totalRetries / totalDeliveries : 0,
            EventTypeBreakdown = eventTypeBreakdown,
            ResponseCodeDistribution = responseCodeDist,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WebhookTrendDataPoint>> GetDeliveryTrendsAsync(int? webhookId, DateTime startDate, DateTime endDate, TrendInterval interval, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting delivery trends for webhook {WebhookId} from {StartDate} to {EndDate} by {Interval}",
            webhookId?.ToString() ?? "all", startDate, endDate, interval);

        var query = _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= startDate && d.CreatedAt <= endDate);

        if (webhookId.HasValue)
            query = query.Where(d => d.WebhookSubscriptionId == webhookId.Value);

        var deliveries = await query.ToListAsync(cancellationToken);

        var grouped = deliveries
            .GroupBy(d => TruncateToInterval(d.CreatedAt, interval))
            .Select(g => new WebhookTrendDataPoint
            {
                Timestamp = g.Key,
                TotalCount = g.Count(),
                SuccessCount = g.Count(d => d.Success),
                FailureCount = g.Count(d => !d.Success),
                AverageLatencyMs = g.Where(d => d.DurationMs.HasValue).Select(d => d.DurationMs!.Value).DefaultIfEmpty().Average()
            })
            .OrderBy(p => p.Timestamp)
            .ToList();

        return grouped;
    }

    private static int CalculateConsecutiveFailures(List<CRM.Core.Entities.ITSM.WebhookDelivery> deliveries)
    {
        var consecutiveFailures = 0;
        foreach (var delivery in deliveries.OrderByDescending(d => d.CreatedAt))
        {
            if (!delivery.Success)
                consecutiveFailures++;
            else
                break;
        }
        return consecutiveFailures;
    }

    private static double GetPercentile(List<double> sortedData, int percentile)
    {
        if (sortedData.Count == 0)
            return 0;

        var index = (percentile / 100.0) * (sortedData.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper)
            return sortedData[lower];

        return sortedData[lower] + (index - lower) * (sortedData[upper] - sortedData[lower]);
    }

    private static DateTime TruncateToInterval(DateTime dateTime, TrendInterval interval)
    {
        return interval switch
        {
            TrendInterval.Hour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, DateTimeKind.Utc),
            TrendInterval.Day => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc),
            TrendInterval.Week => dateTime.AddDays(-(int)dateTime.DayOfWeek).Date,
            TrendInterval.Month => new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => dateTime.Date
        };
    }
}
