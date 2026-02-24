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
/// Implementation of IDeliveryTracker for tracking webhook delivery attempts.
/// Tracks success/failure rates, latency, and delivery status.
/// Implements TODO-INT001-11.
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
        _logger.LogDebug(
            "Tracking delivery attempt for webhook {WebhookId}, event {EventType}, attempt #{AttemptNumber}",
            deliveryAttempt.WebhookId, deliveryAttempt.EventType, deliveryAttempt.AttemptNumber);

        var delivery = new WebhookDeliveryGeneral
        {
            WebhookEndpointId = deliveryAttempt.WebhookId,
            WebhookEventId = deliveryAttempt.WebhookId, // Will be overwritten once the WebhookEvent is linked
            AttemptNumber = deliveryAttempt.AttemptNumber,
            Status = "Pending",
            CorrelationId = deliveryAttempt.CorrelationId,
            ParentEventId = deliveryAttempt.ParentEventId != null && int.TryParse(deliveryAttempt.ParentEventId, out var pid) ? pid : null,
            CreatedAt = deliveryAttempt.AttemptedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WebhookDeliveriesGeneral.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Delivery tracked with ID {DeliveryId} for webhook {WebhookId}",
            delivery.Id, deliveryAttempt.WebhookId);

        return delivery.Id;
    }

    /// <inheritdoc />
    public async Task<DeliveryStatusInfo?> GetDeliveryStatusAsync(int deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted, cancellationToken);

        if (delivery == null)
            return null;

        return new DeliveryStatusInfo
        {
            DeliveryId = delivery.Id,
            WebhookId = delivery.WebhookEndpointId,
            Status = MapStatus(delivery.Status),
            AttemptCount = delivery.AttemptNumber,
            LastAttemptAt = delivery.UpdatedAt,
            NextRetryAt = delivery.NextRetryAt,
            LastResponseCode = delivery.HttpStatusCode,
            ErrorMessage = delivery.ErrorMessage
        };
    }

    /// <inheritdoc />
    public async Task<DeliveryMetrics> GetMetricsAsync(DeliveryMetricsFilter filter, CancellationToken cancellationToken = default)
    {
        var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-7);
        var endDate = filter.EndDate ?? DateTime.UtcNow;

        _logger.LogDebug("Calculating delivery metrics from {StartDate} to {EndDate}", startDate, endDate);

        var query = _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.CreatedAt >= startDate && d.CreatedAt <= endDate);

        if (filter.WebhookId.HasValue)
            query = query.Where(d => d.WebhookEndpointId == filter.WebhookId.Value);

        if (!string.IsNullOrEmpty(filter.EventType))
            query = query.Where(d => d.EntityType == filter.EventType);

        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == MapStatusToString(filter.Status.Value));

        var deliveries = await query.ToListAsync(cancellationToken);

        var total = deliveries.Count;
        var successful = deliveries.Count(d => d.Status == "Success");
        var failed = deliveries.Count(d => d.Status == "Failed");
        var pending = deliveries.Count(d => d.Status == "Pending");

        var durations = deliveries
            .Where(d => d.DurationMs.HasValue)
            .Select(d => d.DurationMs!.Value)
            .OrderBy(d => d)
            .ToList();

        var avgLatency = durations.Count > 0 ? durations.Average() : 0;
        var p95Latency = durations.Count > 0 ? Percentile(durations, 95) : 0;
        var p99Latency = durations.Count > 0 ? Percentile(durations, 99) : 0;

        var byEventType = deliveries
            .Where(d => !string.IsNullOrEmpty(d.EntityType))
            .GroupBy(d => d.EntityType!)
            .ToDictionary(g => g.Key, g => g.Count());

        var byResponseCode = deliveries
            .Where(d => d.HttpStatusCode.HasValue)
            .GroupBy(d => d.HttpStatusCode!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalRetries = deliveries.Where(d => d.AttemptNumber > 1).Sum(d => d.AttemptNumber - 1);

        return new DeliveryMetrics
        {
            TotalDeliveries = total,
            SuccessfulDeliveries = successful,
            FailedDeliveries = failed,
            PendingDeliveries = pending,
            SuccessRate = total > 0 ? successful * 100.0 / total : 0,
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
        var delivery = await _context.WebhookDeliveriesGeneral
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted, cancellationToken);

        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found for result update", deliveryId);
            return;
        }

        delivery.Status = result.Success ? "Success" : "Failed";
        delivery.HttpStatusCode = result.ResponseStatusCode;
        delivery.ResponseBody = result.ResponseBody?.Length > 2000
            ? result.ResponseBody[..2000]
            : result.ResponseBody;
        delivery.ErrorMessage = result.ErrorMessage;
        delivery.DurationMs = result.DurationMs;
        delivery.DeliveredAt = result.CompletedAt;
        delivery.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Delivery {DeliveryId} updated: Success={Success}, StatusCode={StatusCode}, Duration={DurationMs}ms",
            deliveryId, result.Success, result.ResponseStatusCode, result.DurationMs);
    }

    private static DeliveryStatus MapStatus(string status) => status switch
    {
        "Pending" => DeliveryStatus.Pending,
        "InProgress" => DeliveryStatus.InProgress,
        "Success" => DeliveryStatus.Succeeded,
        "Failed" => DeliveryStatus.FailedPermanent,
        "Retrying" => DeliveryStatus.FailedRetrying,
        _ => DeliveryStatus.Pending
    };

    private static string MapStatusToString(DeliveryStatus status) => status switch
    {
        DeliveryStatus.Pending => "Pending",
        DeliveryStatus.InProgress => "InProgress",
        DeliveryStatus.Succeeded => "Success",
        DeliveryStatus.FailedPermanent => "Failed",
        DeliveryStatus.FailedRetrying => "Retrying",
        _ => "Pending"
    };

    private static double Percentile(List<long> sortedValues, int percentile)
    {
        if (sortedValues.Count == 0) return 0;
        var index = (percentile / 100.0) * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        if (lower == upper) return sortedValues[lower];
        return sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * (index - lower);
    }
}
