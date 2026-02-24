// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for webhook analytics and metrics.
/// Implements TODO-INT001-50: Webhook analytics service.
/// </summary>
public interface IWebhookAnalyticsService
{
    /// <summary>
    /// Gets the success rate for a specific webhook over a time period.
    /// </summary>
    /// <param name="webhookId">The webhook endpoint ID.</param>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success rate as a percentage (0-100).</returns>
    Task<double> GetSuccessRateAsync(int webhookId, AnalyticsPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the average latency for a specific webhook.
    /// </summary>
    /// <param name="webhookId">The webhook endpoint ID.</param>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Average latency in milliseconds.</returns>
    Task<double> GetAverageLatencyAsync(int webhookId, AnalyticsPeriod period = AnalyticsPeriod.Last24Hours, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets delivery volume metrics for a time period.
    /// </summary>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="webhookId">Optional webhook ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Volume metrics including counts by status.</returns>
    Task<DeliveryVolumeMetrics> GetDeliveryVolumeAsync(AnalyticsPeriod period, int? webhookId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comprehensive analytics for a webhook.
    /// </summary>
    /// <param name="webhookId">The webhook endpoint ID.</param>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Comprehensive analytics data.</returns>
    Task<WebhookAnalytics> GetWebhookAnalyticsAsync(int webhookId, AnalyticsPeriod period = AnalyticsPeriod.Last7Days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets analytics for all webhooks.
    /// </summary>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of analytics for all webhooks.</returns>
    Task<IReadOnlyList<WebhookAnalytics>> GetAllWebhookAnalyticsAsync(AnalyticsPeriod period = AnalyticsPeriod.Last7Days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets time-series data for webhook deliveries.
    /// </summary>
    /// <param name="webhookId">Optional webhook ID to filter by.</param>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="granularity">Time granularity for data points.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Time-series data points.</returns>
    Task<IReadOnlyList<TimeSeriesDataPoint>> GetTimeSeriesAsync(
        int? webhookId,
        AnalyticsPeriod period,
        TimeGranularity granularity = TimeGranularity.Hour,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the top webhooks by delivery volume.
    /// </summary>
    /// <param name="period">Time period to analyze.</param>
    /// <param name="limit">Maximum number of webhooks to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of top webhooks by volume.</returns>
    Task<IReadOnlyList<WebhookVolumeRank>> GetTopWebhooksByVolumeAsync(
        AnalyticsPeriod period,
        int limit = 10,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Time period for analytics queries.
/// </summary>
public enum AnalyticsPeriod
{
    /// <summary>Last hour.</summary>
    LastHour,

    /// <summary>Last 24 hours.</summary>
    Last24Hours,

    /// <summary>Last 7 days.</summary>
    Last7Days,

    /// <summary>Last 30 days.</summary>
    Last30Days,

    /// <summary>Last 90 days.</summary>
    Last90Days,

    /// <summary>All time.</summary>
    AllTime
}

/// <summary>
/// Time granularity for time-series data.
/// </summary>
public enum TimeGranularity
{
    /// <summary>Per minute.</summary>
    Minute,

    /// <summary>Per hour.</summary>
    Hour,

    /// <summary>Per day.</summary>
    Day,

    /// <summary>Per week.</summary>
    Week,

    /// <summary>Per month.</summary>
    Month
}

/// <summary>
/// Delivery volume metrics.
/// </summary>
public record DeliveryVolumeMetrics
{
    /// <summary>Total number of deliveries.</summary>
    public int TotalDeliveries { get; init; }

    /// <summary>Number of successful deliveries.</summary>
    public int SuccessfulDeliveries { get; init; }

    /// <summary>Number of failed deliveries.</summary>
    public int FailedDeliveries { get; init; }

    /// <summary>Number of pending deliveries.</summary>
    public int PendingDeliveries { get; init; }

    /// <summary>Number of deliveries currently retrying.</summary>
    public int RetryingDeliveries { get; init; }

    /// <summary>Overall success rate as percentage.</summary>
    public double SuccessRate => TotalDeliveries > 0 ? (SuccessfulDeliveries * 100.0 / TotalDeliveries) : 0;

    /// <summary>The time period these metrics cover.</summary>
    public AnalyticsPeriod Period { get; init; }

    /// <summary>Start of the analysis period.</summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>End of the analysis period.</summary>
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Comprehensive analytics for a single webhook.
/// </summary>
public record WebhookAnalytics
{
    /// <summary>The webhook endpoint ID.</summary>
    public int WebhookId { get; init; }

    /// <summary>The webhook name.</summary>
    public string? WebhookName { get; init; }

    /// <summary>The webhook URL.</summary>
    public string? WebhookUrl { get; init; }

    /// <summary>Whether the webhook is currently active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Delivery volume metrics.</summary>
    public DeliveryVolumeMetrics VolumeMetrics { get; init; } = new();

    /// <summary>Average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>95th percentile latency in milliseconds.</summary>
    public double P95LatencyMs { get; init; }

    /// <summary>99th percentile latency in milliseconds.</summary>
    public double P99LatencyMs { get; init; }

    /// <summary>Minimum latency in milliseconds.</summary>
    public long MinLatencyMs { get; init; }

    /// <summary>Maximum latency in milliseconds.</summary>
    public long MaxLatencyMs { get; init; }

    /// <summary>Most common error messages.</summary>
    public IReadOnlyList<ErrorFrequency> TopErrors { get; init; } = Array.Empty<ErrorFrequency>();

    /// <summary>Delivery counts by event type.</summary>
    public IReadOnlyDictionary<string, int> DeliveriesByEventType { get; init; } = new Dictionary<string, int>();

    /// <summary>Date/time of last delivery.</summary>
    public DateTime? LastDeliveryAt { get; init; }

    /// <summary>Date/time of last successful delivery.</summary>
    public DateTime? LastSuccessAt { get; init; }

    /// <summary>Date/time of last failure.</summary>
    public DateTime? LastFailureAt { get; init; }
}

/// <summary>
/// Error frequency data.
/// </summary>
public record ErrorFrequency
{
    /// <summary>The error message.</summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>Number of occurrences.</summary>
    public int Count { get; init; }

    /// <summary>Most recent occurrence.</summary>
    public DateTime LastOccurrence { get; init; }
}

/// <summary>
/// Time-series data point.
/// </summary>
public record TimeSeriesDataPoint
{
    /// <summary>The timestamp for this data point.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Total deliveries in this period.</summary>
    public int TotalDeliveries { get; init; }

    /// <summary>Successful deliveries in this period.</summary>
    public int SuccessfulDeliveries { get; init; }

    /// <summary>Failed deliveries in this period.</summary>
    public int FailedDeliveries { get; init; }

    /// <summary>Average latency in this period.</summary>
    public double AverageLatencyMs { get; init; }
}

/// <summary>
/// Webhook ranking by volume.
/// </summary>
public record WebhookVolumeRank
{
    /// <summary>The webhook endpoint ID.</summary>
    public int WebhookId { get; init; }

    /// <summary>The webhook name.</summary>
    public string? WebhookName { get; init; }

    /// <summary>Total delivery volume.</summary>
    public int TotalDeliveries { get; init; }

    /// <summary>Success rate as percentage.</summary>
    public double SuccessRate { get; init; }

    /// <summary>Rank position (1 = highest volume).</summary>
    public int Rank { get; init; }
}
