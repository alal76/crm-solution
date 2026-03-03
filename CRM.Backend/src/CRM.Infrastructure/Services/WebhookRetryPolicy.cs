// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implements exponential backoff retry policy for webhook deliveries.
/// Replaces the fixed 300s interval.
/// </summary>
public interface IWebhookRetryPolicy
{
    /// <summary>Calculate next retry delay based on attempt number</summary>
    TimeSpan GetRetryDelay(int attemptNumber);

    /// <summary>Check if the webhook should be retried</summary>
    bool ShouldRetry(int attemptNumber, int httpStatusCode);

    /// <summary>Check if webhook should be disabled after failures</summary>
    bool ShouldDisableWebhook(int consecutiveFailures);

    /// <summary>Maximum number of retry attempts</summary>
    int MaxRetries { get; }
}

public class WebhookRetryPolicy : IWebhookRetryPolicy
{
    private const int DefaultMaxRetries = 5;
    private const int BaseDelaySeconds = 30;
    private const double BackoffMultiplier = 2.0;
    private const int MaxDelaySeconds = 3600; // 1 hour cap
    private const int DisableThreshold = 10; // consecutive failures before auto-disable

    public int MaxRetries => DefaultMaxRetries;

    public TimeSpan GetRetryDelay(int attemptNumber)
    {
        if (attemptNumber <= 0) return TimeSpan.Zero;

        // Exponential backoff: 30s, 60s, 120s, 240s, 480s (capped at 1hr)
        var delay = BaseDelaySeconds * Math.Pow(BackoffMultiplier, attemptNumber - 1);
        var jitter = Random.Shared.Next(0, (int)(delay * 0.1)); // 10% jitter // NOSONAR - S2245: non-security RNG for exponential backoff jitter
        var totalSeconds = Math.Min(delay + jitter, MaxDelaySeconds);

        return TimeSpan.FromSeconds(totalSeconds);
    }

    public bool ShouldRetry(int attemptNumber, int httpStatusCode)
    {
        if (attemptNumber >= MaxRetries) return false;

        // Don't retry client errors (4xx) except 408 (timeout) and 429 (rate limit)
        if (httpStatusCode >= 400 && httpStatusCode < 500
            && httpStatusCode != 408 && httpStatusCode != 429)
            return false;

        // Retry server errors (5xx), timeouts, rate limits
        return true;
    }

    public bool ShouldDisableWebhook(int consecutiveFailures)
    {
        return consecutiveFailures >= DisableThreshold;
    }
}
