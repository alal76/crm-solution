// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Concurrent;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Circuit breaker to prevent infinite loops and cascading webhook failures.
/// Tracks webhook endpoint health and breaks circuit when failures exceed threshold.
/// </summary>
public interface IWebhookCircuitBreaker
{
    /// <summary>Check if circuit is open (requests should be blocked)</summary>
    bool IsCircuitOpen(string webhookUrl);

    /// <summary>Record a successful delivery</summary>
    void RecordSuccess(string webhookUrl);

    /// <summary>Record a failed delivery</summary>
    void RecordFailure(string webhookUrl);

    /// <summary>Get circuit state for a webhook URL</summary>
    WebhookCircuitState GetState(string webhookUrl);
}

public enum WebhookCircuitState
{
    Closed,
    Open,
    HalfOpen
}

public class WebhookCircuitBreaker : IWebhookCircuitBreaker
{
    private const int FailureThreshold = 5;
    private static readonly TimeSpan ResetTimeout = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, CircuitInfo> _circuits = new();

    public bool IsCircuitOpen(string webhookUrl)
    {
        var state = GetState(webhookUrl);
        return state == WebhookCircuitState.Open;
    }

    public void RecordSuccess(string webhookUrl)
    {
        _circuits.AddOrUpdate(
            webhookUrl,
            _ => new CircuitInfo(), // new entry → healthy
            (_, existing) =>
            {
                lock (existing.Lock)
                {
                    existing.ConsecutiveFailures = 0;
                    existing.State = WebhookCircuitState.Closed;
                    existing.LastSuccessUtc = DateTime.UtcNow;
                    return existing;
                }
            });
    }

    public void RecordFailure(string webhookUrl)
    {
        _circuits.AddOrUpdate(
            webhookUrl,
            _ =>
            {
                var info = new CircuitInfo();
                info.ConsecutiveFailures = 1;
                info.LastFailureUtc = DateTime.UtcNow;
                return info;
            },
            (_, existing) =>
            {
                lock (existing.Lock)
                {
                    existing.ConsecutiveFailures++;
                    existing.LastFailureUtc = DateTime.UtcNow;

                    if (existing.ConsecutiveFailures >= FailureThreshold)
                    {
                        existing.State = WebhookCircuitState.Open;
                        existing.OpenedAtUtc = DateTime.UtcNow;
                    }

                    return existing;
                }
            });
    }

    public WebhookCircuitState GetState(string webhookUrl)
    {
        if (!_circuits.TryGetValue(webhookUrl, out var info))
        {
            return WebhookCircuitState.Closed;
        }

        lock (info.Lock)
        {
            if (info.State == WebhookCircuitState.Open)
            {
                // Check if reset timeout has elapsed → transition to half-open
                if (info.OpenedAtUtc.HasValue &&
                    DateTime.UtcNow - info.OpenedAtUtc.Value >= ResetTimeout)
                {
                    info.State = WebhookCircuitState.HalfOpen;
                    return WebhookCircuitState.HalfOpen;
                }
            }

            return info.State;
        }
    }

    /// <summary>
    /// Internal circuit state tracker for a single webhook URL.
    /// </summary>
    private sealed class CircuitInfo
    {
        public readonly object Lock = new();
        public int ConsecutiveFailures { get; set; }
        public WebhookCircuitState State { get; set; } = WebhookCircuitState.Closed;
        public DateTime? LastFailureUtc { get; set; }
        public DateTime? LastSuccessUtc { get; set; }
        public DateTime? OpenedAtUtc { get; set; }
    }
}
