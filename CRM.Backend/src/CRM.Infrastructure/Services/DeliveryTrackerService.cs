// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IDeliveryTracker for logging and tracking webhook delivery metrics.
/// Stores delivery attempts, success/failure, and latency for analytics.
/// </summary>
public class DeliveryTrackerService : IDeliveryTracker
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DeliveryTrackerService> _logger;

    public DeliveryTrackerService(ICrmDbContext context, ILogger<DeliveryTrackerService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<int> TrackDeliveryAsync(DeliveryAttemptInfo deliveryAttempt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deliveryAttempt);

        _logger.LogInformation(
            "Tracking delivery attempt for webhook {WebhookId}, event {EventType}, attempt #{AttemptNumber}",
            deliveryAttempt.WebhookId,
            deliveryAttempt.EventType,
            deliveryAttempt.AttemptNumber);

        var delivery = new CRM.Core.Entities.ITSM.WebhookDelivery
        {
            WebhookSubscriptionId = deliveryAttempt.WebhookId,
            EventType = deliveryAttempt.EventType,
            TargetUrl = deliveryAttempt.TargetUrl,
            RequestBody = deliveryAttempt.Payload,
            AttemptNumber = deliveryAttempt.AttemptNumber,
            Success = false,
            CreatedAt = deliveryAttempt.AttemptedAt,
            UpdatedAt = DateTime.UtcNow,
            CorrelationId = deliveryAttempt.CorrelationId,
            ParentEventId = deliveryAttempt.ParentEventId
        };

        _context.WebhookDeliveries.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Created delivery record {DeliveryId} for webhook {WebhookId}", delivery.WebhookDeliveryId, deliveryAttempt.WebhookId);

        return delivery.WebhookDeliveryId;
    }

    /// <inheritdoc />
    public async Task<DeliveryStatusInfo?> GetDeliveryStatusAsync(int deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.WebhookDeliveryId == deliveryId && !d.IsDeleted, cancellationToken);

        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
            return null;
        }

        return new DeliveryStatusInfo
        {
            DeliveryId = delivery.WebhookDeliveryId,
            WebhookId = delivery.WebhookSubscriptionId,
            Status = MapToStatus(delivery),
            AttemptCount = delivery.AttemptNumber,
            LastAttemptAt = delivery.UpdatedAt,
            NextRetryAt = delivery.NextRetryAt,
            LastResponseCode = delivery.ResponseStatusCode,
            ErrorMessage = delivery.ErrorMessage
        };
    }

    /// <inheritdoc />
    public async Task<DeliveryMetrics> GetMetricsAsync(DeliveryMetricsFilter filter, CancellationToken cancellationToken = default)
    {
        filter ??= new DeliveryMetricsFilter();

        var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = filter.EndDate ?? DateTime.UtcNow;

        _logger.LogInformation("Calculating delivery metrics from {StartDate} to {EndDate}", startDate, endDate);

        var query = _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= startDate && d.CreatedAt <= endDate);

        if (filter.WebhookId.HasValue)
            query = query.Where(d => d.WebhookSubscriptionId == filter.WebhookId.Value);

        if (!string.IsNullOrEmpty(filter.EventType))
            query = query.Where(d => d.EventType == filter.EventType);

        var deliveries = await query.ToListAsync(cancellationToken);

        var totalDeliveries = deliveries.Count;
        var successfulDeliveries = deliveries.Count(d => d.Success);
        var failedDeliveries = deliveries.Count(d => !d.Success && d.AttemptNumber >= 3);
        var pendingDeliveries = deliveries.Count(d => !d.Success && d.AttemptNumber < 3);

        var latencies = deliveries
            .Where(d => d.DurationMs.HasValue && d.DurationMs.Value > 0)
            .Select(d => (double)d.DurationMs!.Value)
            .OrderBy(l => l)
            .ToList();

        var avgLatency = latencies.Count > 0 ? latencies.Average() : 0;
        var p95Latency = latencies.Count > 0 ? GetPercentile(latencies, 95) : 0;
        var p99Latency = latencies.Count > 0 ? GetPercentile(latencies, 99) : 0;

        var byEventType = deliveries
            .GroupBy(d => d.EventType)
            .ToDictionary(g => g.Key, g => g.Count());

        var byResponseCode = deliveries
            .Where(d => d.ResponseStatusCode.HasValue)
            .GroupBy(d => d.ResponseStatusCode!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalRetries = deliveries.Sum(d => Math.Max(0, d.AttemptNumber - 1));

        return new DeliveryMetrics
        {
            TotalDeliveries = totalDeliveries,
            SuccessfulDeliveries = successfulDeliveries,
            FailedDeliveries = failedDeliveries,
            PendingDeliveries = pendingDeliveries,
            SuccessRate = totalDeliveries > 0 ? (successfulDeliveries * 100.0 / totalDeliveries) : 0,
            AverageLatencyMs = avgLatency,
            P95LatencyMs = p95Latency,
            P99LatencyMs = p99Latency,
            TotalRetries = totalRetries,
            ByEventType = byEventType,
            ByResponseCode = byResponseCode,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }

    /// <inheritdoc />
    public async Task UpdateDeliveryResultAsync(int deliveryId, DeliveryResultInfo result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var delivery = await _context.WebhookDeliveries
            .FirstOrDefaultAsync(d => d.WebhookDeliveryId == deliveryId && !d.IsDeleted, cancellationToken);

        if (delivery == null)
        {
            _logger.LogWarning("Cannot update delivery {DeliveryId} - not found", deliveryId);
            return;
        }

        delivery.Success = result.Success;
        delivery.ResponseStatusCode = result.ResponseStatusCode;
        delivery.ResponseBody = result.ResponseBody;
        delivery.ErrorMessage = result.ErrorMessage;
        delivery.DurationMs = result.DurationMs;
        delivery.CompletedAt = result.CompletedAt;
        delivery.UpdatedAt = DateTime.UtcNow;

        _context.WebhookDeliveries.Update(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated delivery {DeliveryId} with result: {Success}, status code: {StatusCode}, duration: {DurationMs}ms",
            deliveryId,
            result.Success,
            result.ResponseStatusCode,
            result.DurationMs);
    }

    private static DeliveryStatus MapToStatus(CRM.Core.Entities.ITSM.WebhookDelivery delivery)
    {
        if (delivery.Success)
            return DeliveryStatus.Succeeded;

        if (delivery.AttemptNumber >= 3)
            return DeliveryStatus.FailedPermanent;

        if (delivery.AttemptNumber > 0)
            return DeliveryStatus.FailedRetrying;

        return DeliveryStatus.Pending;
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
}
