// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for webhook analytics operations.
/// Provides methods for calculating success rates, latency, and failure analysis.
/// </summary>
public interface IWebhookAnalyticsService
{
    /// <summary>
    /// Gets the success rate for webhooks over a time period.
    /// </summary>
    /// <param name="webhookId">Optional webhook ID filter. If null, calculates for all webhooks.</param>
    /// <param name="startDate">Start of the period.</param>
    /// <param name="endDate">End of the period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success rate percentage (0-100).</returns>
    Task<double> GetSuccessRateAsync(int? webhookId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the average latency for webhook deliveries.
    /// </summary>
    /// <param name="webhookId">Optional webhook ID filter. If null, calculates for all webhooks.</param>
    /// <param name="startDate">Start of the period.</param>
    /// <param name="endDate">End of the period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Average latency in milliseconds.</returns>
    Task<double> GetAverageLatencyAsync(int? webhookId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the top failing webhooks by failure count.
    /// </summary>
    /// <param name="count">Number of top failures to return.</param>
    /// <param name="startDate">Start of the period.</param>
    /// <param name="endDate">End of the period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of top failing webhooks with failure details.</returns>
    Task<IEnumerable<WebhookFailureInfo>> GetTopFailuresAsync(int count, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comprehensive analytics summary for a webhook.
    /// </summary>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="startDate">Start of the period.</param>
    /// <param name="endDate">End of the period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Analytics summary.</returns>
    Task<WebhookAnalyticsSummary> GetAnalyticsSummaryAsync(int webhookId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets delivery trend data for charting.
    /// </summary>
    /// <param name="webhookId">Optional webhook ID filter.</param>
    /// <param name="startDate">Start of the period.</param>
    /// <param name="endDate">End of the period.</param>
    /// <param name="interval">Grouping interval (hour, day, week).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Trend data points.</returns>
    Task<IEnumerable<WebhookTrendDataPoint>> GetDeliveryTrendsAsync(int? webhookId, DateTime startDate, DateTime endDate, TrendInterval interval, CancellationToken cancellationToken = default);
}

/// <summary>
/// Information about a failing webhook.
/// </summary>
public record WebhookFailureInfo
{
    /// <summary>The webhook ID.</summary>
    public int WebhookId { get; init; }

    /// <summary>The webhook name.</summary>
    public string WebhookName { get; init; } = string.Empty;

    /// <summary>Target URL.</summary>
    public string TargetUrl { get; init; } = string.Empty;

    /// <summary>Total failure count.</summary>
    public int FailureCount { get; init; }

    /// <summary>Most common error message.</summary>
    public string MostCommonError { get; init; } = string.Empty;

    /// <summary>Last failure timestamp.</summary>
    public DateTime LastFailureAt { get; init; }

    /// <summary>Consecutive failures count.</summary>
    public int ConsecutiveFailures { get; init; }

    /// <summary>Most common response code.</summary>
    public int? MostCommonResponseCode { get; init; }
}

/// <summary>
/// Comprehensive analytics summary for a webhook.
/// </summary>
public record WebhookAnalyticsSummary
{
    /// <summary>The webhook ID.</summary>
    public int WebhookId { get; init; }

    /// <summary>Total deliveries in the period.</summary>
    public int TotalDeliveries { get; init; }

    /// <summary>Successful deliveries.</summary>
    public int SuccessfulDeliveries { get; init; }

    /// <summary>Failed deliveries.</summary>
    public int FailedDeliveries { get; init; }

    /// <summary>Success rate percentage.</summary>
    public double SuccessRate { get; init; }

    /// <summary>Average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>P50 latency in milliseconds.</summary>
    public double P50LatencyMs { get; init; }

    /// <summary>P95 latency in milliseconds.</summary>
    public double P95LatencyMs { get; init; }

    /// <summary>P99 latency in milliseconds.</summary>
    public double P99LatencyMs { get; init; }

    /// <summary>Average retries per delivery.</summary>
    public double AverageRetries { get; init; }

    /// <summary>Events breakdown by type.</summary>
    public Dictionary<string, int> EventTypeBreakdown { get; init; } = new();

    /// <summary>Response code distribution.</summary>
    public Dictionary<int, int> ResponseCodeDistribution { get; init; } = new();

    /// <summary>Start of the analytics period.</summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>End of the analytics period.</summary>
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Data point for trend analysis.
/// </summary>
public record WebhookTrendDataPoint
{
    /// <summary>Timestamp of the data point.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Number of total deliveries.</summary>
    public int TotalCount { get; init; }

    /// <summary>Number of successful deliveries.</summary>
    public int SuccessCount { get; init; }

    /// <summary>Number of failed deliveries.</summary>
    public int FailureCount { get; init; }

    /// <summary>Average latency for the period.</summary>
    public double AverageLatencyMs { get; init; }
}

/// <summary>
/// Interval for trend grouping.
/// </summary>
public enum TrendInterval
{
    /// <summary>Group by hour.</summary>
    Hour = 0,

    /// <summary>Group by day.</summary>
    Day = 1,

    /// <summary>Group by week.</summary>
    Week = 2,

    /// <summary>Group by month.</summary>
    Month = 3
}
