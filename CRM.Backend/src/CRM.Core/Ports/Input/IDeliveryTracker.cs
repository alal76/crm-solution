// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

#pragma warning disable SA1206 // C# 11 'required' modifier is not supported by this StyleCop version
namespace CRM.Core.Ports.Input;

/// <summary>
/// Port for tracking webhook delivery attempts, success/failure, and latency metrics.
/// Used for logging, analytics, and monitoring of webhook delivery operations.
/// </summary>
public interface IDeliveryTracker
{
    /// <summary>
    /// Tracks a delivery attempt for a webhook.
    /// </summary>
    /// <param name="deliveryAttempt">Details of the delivery attempt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tracked delivery ID.</returns>
    Task<int> TrackDeliveryAsync(DeliveryAttemptInfo deliveryAttempt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current delivery status for a specific delivery.
    /// </summary>
    /// <param name="deliveryId">The delivery ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delivery status.</returns>
    Task<DeliveryStatusInfo?> GetDeliveryStatusAsync(int deliveryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets delivery metrics for a time period.
    /// </summary>
    /// <param name="filter">Filter parameters for metrics retrieval.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delivery metrics.</returns>
    Task<DeliveryMetrics> GetMetricsAsync(DeliveryMetricsFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a delivery after completion or failure.
    /// </summary>
    /// <param name="deliveryId">The delivery ID.</param>
    /// <param name="result">The result of the delivery.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateDeliveryResultAsync(int deliveryId, DeliveryResultInfo result, CancellationToken cancellationToken = default);
}

/// <summary>
/// Information about a delivery attempt.
/// </summary>
public record DeliveryAttemptInfo
{
    /// <summary>The webhook subscription ID.</summary>
    public int WebhookId { get; init; }

    /// <summary>The event type being delivered.</summary>
    public required string EventType { get; init; }

    /// <summary>The target URL for delivery.</summary>
    public required string TargetUrl { get; init; }

    /// <summary>The payload being delivered.</summary>
    public required string Payload { get; init; }

    /// <summary>The attempt number (1 = first attempt).</summary>
    public int AttemptNumber { get; init; } = 1;

    /// <summary>Correlation ID for tracking across systems.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Parent event ID for chain tracking.</summary>
    public string? ParentEventId { get; init; }

    /// <summary>Timestamp when the attempt started.</summary>
    public DateTime AttemptedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Current status of a delivery.
/// </summary>
public record DeliveryStatusInfo
{
    /// <summary>The delivery ID.</summary>
    public int DeliveryId { get; init; }

    /// <summary>The webhook ID.</summary>
    public int WebhookId { get; init; }

    /// <summary>Current status.</summary>
    public DeliveryStatus Status { get; init; }

    /// <summary>Number of attempts made.</summary>
    public int AttemptCount { get; init; }

    /// <summary>Last attempt timestamp.</summary>
    public DateTime? LastAttemptAt { get; init; }

    /// <summary>Next retry scheduled at.</summary>
    public DateTime? NextRetryAt { get; init; }

    /// <summary>HTTP response code from last attempt.</summary>
    public int? LastResponseCode { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Delivery status enumeration.
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Delivery is pending.</summary>
    Pending = 0,

    /// <summary>Delivery is in progress.</summary>
    InProgress = 1,

    /// <summary>Delivery succeeded.</summary>
    Succeeded = 2,

    /// <summary>Delivery failed, will retry.</summary>
    FailedRetrying = 3,

    /// <summary>Delivery failed permanently.</summary>
    FailedPermanent = 4
}

/// <summary>
/// Filter for delivery metrics queries.
/// </summary>
public record DeliveryMetricsFilter
{
    /// <summary>Start of time period.</summary>
    public DateTime? StartDate { get; init; }

    /// <summary>End of time period.</summary>
    public DateTime? EndDate { get; init; }

    /// <summary>Filter by specific webhook ID.</summary>
    public int? WebhookId { get; init; }

    /// <summary>Filter by event type.</summary>
    public string? EventType { get; init; }

    /// <summary>Filter by status.</summary>
    public DeliveryStatus? Status { get; init; }
}

/// <summary>
/// Aggregated delivery metrics.
/// </summary>
public record DeliveryMetrics
{
    /// <summary>Total number of deliveries.</summary>
    public int TotalDeliveries { get; init; }

    /// <summary>Number of successful deliveries.</summary>
    public int SuccessfulDeliveries { get; init; }

    /// <summary>Number of failed deliveries.</summary>
    public int FailedDeliveries { get; init; }

    /// <summary>Number of pending deliveries.</summary>
    public int PendingDeliveries { get; init; }

    /// <summary>Success rate percentage (0-100).</summary>
    public double SuccessRate { get; init; }

    /// <summary>Average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>P95 latency in milliseconds.</summary>
    public double P95LatencyMs { get; init; }

    /// <summary>P99 latency in milliseconds.</summary>
    public double P99LatencyMs { get; init; }

    /// <summary>Total retries attempted.</summary>
    public int TotalRetries { get; init; }

    /// <summary>Metrics grouped by event type.</summary>
    public Dictionary<string, int> ByEventType { get; init; } = new();

    /// <summary>Metrics grouped by response code.</summary>
    public Dictionary<int, int> ByResponseCode { get; init; } = new();

    /// <summary>Start of the metrics period.</summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>End of the metrics period.</summary>
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Result of a delivery attempt.
/// </summary>
public record DeliveryResultInfo
{
    /// <summary>Whether the delivery succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>HTTP response status code.</summary>
    public int? ResponseStatusCode { get; init; }

    /// <summary>Response body.</summary>
    public string? ResponseBody { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public int DurationMs { get; init; }

    /// <summary>Timestamp when completed.</summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}
