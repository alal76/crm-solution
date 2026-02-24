// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Webhooks;

/// <summary>
/// Implementation of IWebhookAnalyticsService (Ports.Input) for webhook analytics.
/// Provides success rate, latency, volume, and time-series metrics using the
/// general webhook entities (WebhookEndpoint/WebhookEvent/WebhookDeliveryGeneral).
/// Implements TODO-INT001-50.
/// </summary>
public class WebhookAnalyticsPortService : CRM.Core.Ports.Input.IWebhookAnalyticsService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WebhookAnalyticsPortService> _logger;

    public WebhookAnalyticsPortService(ICrmDbContext context, ILogger<WebhookAnalyticsPortService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<double> GetSuccessRateAsync(int webhookId, AnalyticsPeriod period, CancellationToken cancellationToken = default)
    {
        var (start, end) = ResolvePeriod(period);
        var query = _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.WebhookEndpointId == webhookId && d.CreatedAt >= start && d.CreatedAt <= end);

        var total = await query.CountAsync(cancellationToken);
        if (total == 0) return 0;

        var success = await query.CountAsync(d => d.Status == "Success", cancellationToken);
        return success * 100.0 / total;
    }

    /// <inheritdoc />
    public async Task<double> GetAverageLatencyAsync(int webhookId, AnalyticsPeriod period = AnalyticsPeriod.Last24Hours, CancellationToken cancellationToken = default)
    {
        var (start, end) = ResolvePeriod(period);
        return await _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.WebhookEndpointId == webhookId && d.CreatedAt >= start && d.CreatedAt <= end && d.DurationMs.HasValue)
            .Select(d => d.DurationMs!.Value)
            .DefaultIfEmpty()
            .AverageAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DeliveryVolumeMetrics> GetDeliveryVolumeAsync(AnalyticsPeriod period, int? webhookId = null, CancellationToken cancellationToken = default)
    {
        var (start, end) = ResolvePeriod(period);
        var query = _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= start && d.CreatedAt <= end);

        if (webhookId.HasValue)
            query = query.Where(d => d.WebhookEndpointId == webhookId.Value);

        var all = await query.ToListAsync(cancellationToken);

        return new DeliveryVolumeMetrics
        {
            TotalDeliveries = all.Count,
            SuccessfulDeliveries = all.Count(d => d.Status == "Success"),
            FailedDeliveries = all.Count(d => d.Status == "Failed"),
            PendingDeliveries = all.Count(d => d.Status == "Pending"),
            RetryingDeliveries = all.Count(d => d.Status == "Retrying"),
            Period = period,
            PeriodStart = start,
            PeriodEnd = end
        };
    }

    /// <inheritdoc />
    public async Task<WebhookAnalytics> GetWebhookAnalyticsAsync(int webhookId, AnalyticsPeriod period = AnalyticsPeriod.Last7Days, CancellationToken cancellationToken = default)
    {
        var (start, end) = ResolvePeriod(period);

        var endpoint = await _context.WebhookEndpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == webhookId && !e.IsDeleted, cancellationToken);

        var deliveries = await _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.WebhookEndpointId == webhookId && d.CreatedAt >= start && d.CreatedAt <= end)
            .ToListAsync(cancellationToken);

        var durations = deliveries
            .Where(d => d.DurationMs.HasValue)
            .Select(d => d.DurationMs!.Value)
            .OrderBy(d => d)
            .ToList();

        var successCount = deliveries.Count(d => d.Status == "Success");
        var failedCount = deliveries.Count(d => d.Status == "Failed");

        var topErrors = deliveries
            .Where(d => !string.IsNullOrEmpty(d.ErrorMessage))
            .GroupBy(d => d.ErrorMessage!)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new ErrorFrequency
            {
                ErrorMessage = g.Key,
                Count = g.Count(),
                LastOccurrence = g.Max(d => d.UpdatedAt).GetValueOrDefault()
            })
            .ToList();

        var byEventType = deliveries
            .Where(d => !string.IsNullOrEmpty(d.EntityType))
            .GroupBy(d => d.EntityType!)
            .ToDictionary(g => g.Key, g => g.Count());

        return new WebhookAnalytics
        {
            WebhookId = webhookId,
            WebhookName = endpoint?.Name,
            WebhookUrl = endpoint?.Url,
            IsActive = endpoint?.IsActive ?? false,
            VolumeMetrics = new DeliveryVolumeMetrics
            {
                TotalDeliveries = deliveries.Count,
                SuccessfulDeliveries = successCount,
                FailedDeliveries = failedCount,
                PendingDeliveries = deliveries.Count(d => d.Status == "Pending"),
                RetryingDeliveries = deliveries.Count(d => d.Status == "Retrying"),
                Period = period,
                PeriodStart = start,
                PeriodEnd = end
            },
            AverageLatencyMs = durations.Count > 0 ? durations.Average() : 0,
            P95LatencyMs = durations.Count > 0 ? Percentile(durations, 95) : 0,
            P99LatencyMs = durations.Count > 0 ? Percentile(durations, 99) : 0,
            MinLatencyMs = durations.Count > 0 ? durations.Min() : 0,
            MaxLatencyMs = durations.Count > 0 ? durations.Max() : 0,
            TopErrors = topErrors,
            DeliveriesByEventType = byEventType,
            LastDeliveryAt = deliveries.OrderByDescending(d => d.CreatedAt).FirstOrDefault()?.CreatedAt,
            LastSuccessAt = deliveries.Where(d => d.Status == "Success").OrderByDescending(d => d.CreatedAt).FirstOrDefault()?.CreatedAt,
            LastFailureAt = deliveries.Where(d => d.Status == "Failed").OrderByDescending(d => d.CreatedAt).FirstOrDefault()?.CreatedAt
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WebhookAnalytics>> GetAllWebhookAnalyticsAsync(AnalyticsPeriod period = AnalyticsPeriod.Last7Days, CancellationToken cancellationToken = default)
    {
        var endpoints = await _context.WebhookEndpoints
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .ToListAsync(cancellationToken);

        var results = new List<WebhookAnalytics>();
        foreach (var ep in endpoints)
        {
            var analytics = await GetWebhookAnalyticsAsync(ep.Id, period, cancellationToken);
            results.Add(analytics);
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimeSeriesDataPoint>> GetTimeSeriesAsync(
        int? webhookId,
        AnalyticsPeriod period,
        TimeGranularity granularity = TimeGranularity.Hour,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = ResolvePeriod(period);
        var query = _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= start && d.CreatedAt <= end);

        if (webhookId.HasValue)
            query = query.Where(d => d.WebhookEndpointId == webhookId.Value);

        var deliveries = await query.ToListAsync(cancellationToken);

        // Group by time bucket
        var grouped = deliveries
            .GroupBy(d => TruncateTime(d.CreatedAt, granularity))
            .OrderBy(g => g.Key)
            .Select(g => new TimeSeriesDataPoint
            {
                Timestamp = g.Key,
                TotalDeliveries = g.Count(),
                SuccessfulDeliveries = g.Count(d => d.Status == "Success"),
                FailedDeliveries = g.Count(d => d.Status == "Failed"),
                AverageLatencyMs = g.Where(d => d.DurationMs.HasValue).Select(d => (double)d.DurationMs!.Value).DefaultIfEmpty().Average()
            })
            .ToList();

        return grouped;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WebhookVolumeRank>> GetTopWebhooksByVolumeAsync(
        AnalyticsPeriod period,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = ResolvePeriod(period);
        var deliveries = await _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= start && d.CreatedAt <= end)
            .ToListAsync(cancellationToken);

        var endpoints = await _context.WebhookEndpoints
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        var ranked = deliveries
            .GroupBy(d => d.WebhookEndpointId)
            .Select(g =>
            {
                var total = g.Count();
                var success = g.Count(d => d.Status == "Success");
                return new
                {
                    WebhookId = g.Key,
                    Total = total,
                    SuccessRate = total > 0 ? success * 100.0 / total : 0
                };
            })
            .OrderByDescending(x => x.Total)
            .Take(limit)
            .Select((x, i) => new WebhookVolumeRank
            {
                WebhookId = x.WebhookId,
                WebhookName = endpoints.GetValueOrDefault(x.WebhookId),
                TotalDeliveries = x.Total,
                SuccessRate = x.SuccessRate,
                Rank = i + 1
            })
            .ToList();

        return ranked;
    }

    private static (DateTime Start, DateTime End) ResolvePeriod(AnalyticsPeriod period)
    {
        var now = DateTime.UtcNow;
        return period switch
        {
            AnalyticsPeriod.LastHour => (now.AddHours(-1), now),
            AnalyticsPeriod.Last24Hours => (now.AddHours(-24), now),
            AnalyticsPeriod.Last7Days => (now.AddDays(-7), now),
            AnalyticsPeriod.Last30Days => (now.AddDays(-30), now),
            AnalyticsPeriod.Last90Days => (now.AddDays(-90), now),
            AnalyticsPeriod.AllTime => (DateTime.MinValue, now),
            _ => (now.AddDays(-7), now)
        };
    }

    private static DateTime TruncateTime(DateTime dt, TimeGranularity granularity) => granularity switch
    {
        TimeGranularity.Minute => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, DateTimeKind.Utc),
        TimeGranularity.Hour => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc),
        TimeGranularity.Day => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc),
        TimeGranularity.Week => StartOfWeek(dt),
        TimeGranularity.Month => new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Utc),
        _ => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc)
    };

    private static DateTime StartOfWeek(DateTime dt)
    {
        var diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
        return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-diff);
    }

    private static double Percentile(List<long> sorted, int percentile)
    {
        if (sorted.Count == 0) return 0;
        var idx = (percentile / 100.0) * (sorted.Count - 1);
        var lo = (int)Math.Floor(idx);
        var hi = (int)Math.Ceiling(idx);
        if (lo == hi) return sorted[lo];
        return sorted[lo] + (sorted[hi] - sorted[lo]) * (idx - lo);
    }
}
