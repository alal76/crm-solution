// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// DTO for rate limit quota check result.
/// </summary>
public class RateLimitQuotaResult
{
    /// <summary>
    /// Whether the request is allowed (quota not exceeded).
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Remaining quota for the current period.
    /// </summary>
    public int RemainingQuota { get; set; }

    /// <summary>
    /// Maximum quota for the period.
    /// </summary>
    public int MaxQuota { get; set; }

    /// <summary>
    /// When the current quota window resets.
    /// </summary>
    public DateTime ResetAt { get; set; }

    /// <summary>
    /// How many seconds until the quota resets.
    /// </summary>
    public int RetryAfterSeconds { get; set; }

    /// <summary>
    /// The endpoint or resource being rate limited.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user rate limit usage statistics.
/// </summary>
public class RateLimitUsageStats
{
    /// <summary>
    /// User ID for these stats.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Breakdown of usage by endpoint.
    /// </summary>
    public Dictionary<string, EndpointUsageStats> EndpointStats { get; set; } = new();

    /// <summary>
    /// Total requests in the current period.
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// Period start time.
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end time.
    /// </summary>
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// DTO for per-endpoint usage statistics.
/// </summary>
public class EndpointUsageStats
{
    /// <summary>
    /// Endpoint name/path.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Current usage count.
    /// </summary>
    public int CurrentUsage { get; set; }

    /// <summary>
    /// Maximum allowed for this endpoint.
    /// </summary>
    public int MaxAllowed { get; set; }

    /// <summary>
    /// Percentage of quota used.
    /// </summary>
    public double UsagePercentage => MaxAllowed > 0 ? (double)CurrentUsage / MaxAllowed * 100 : 0;
}

/// <summary>
/// Service interface for application-level rate limiting.
/// TODO-SYS005-002: Rate Limiting Service
///
/// Provides per-user, per-endpoint rate limiting with configurable quotas.
/// Supports both in-memory and Redis-backed implementations.
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Checks if the user has remaining quota for the specified endpoint.
    /// Does NOT increment the usage counter.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="endpoint">Endpoint identifier (e.g., "/api/accounts")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Quota check result with remaining quota info</returns>
    Task<RateLimitQuotaResult> CheckQuotaAsync(int userId, string endpoint, CancellationToken ct = default);

    /// <summary>
    /// Checks quota and increments usage if allowed.
    /// Use this for atomic check-and-increment operations.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="endpoint">Endpoint identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Quota check result (IsAllowed=true if request was accepted)</returns>
    Task<RateLimitQuotaResult> CheckAndIncrementAsync(int userId, string endpoint, CancellationToken ct = default);

    /// <summary>
    /// Increments usage for a user/endpoint.
    /// Use after CheckQuotaAsync if you need separate check and increment.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="endpoint">Endpoint identifier</param>
    /// <param name="count">Number to increment by (default: 1)</param>
    /// <param name="ct">Cancellation token</param>
    Task IncrementUsageAsync(int userId, string endpoint, int count = 1, CancellationToken ct = default);

    /// <summary>
    /// Gets usage statistics for a user across all endpoints.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Usage statistics</returns>
    Task<RateLimitUsageStats> GetUsageStatsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Resets the quota for a specific user and endpoint.
    /// Admin operation for emergency quota reset.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="endpoint">Endpoint identifier (or "*" for all endpoints)</param>
    /// <param name="ct">Cancellation token</param>
    Task ResetQuotaAsync(int userId, string endpoint, CancellationToken ct = default);

    /// <summary>
    /// Gets the configured rate limit for an endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint identifier</param>
    /// <returns>Configured limit per period, or default if not configured</returns>
    int GetEndpointLimit(string endpoint);

    /// <summary>
    /// Gets the rate limit period in seconds for an endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint identifier</param>
    /// <returns>Period in seconds (e.g., 60 for per-minute limits)</returns>
    int GetEndpointPeriodSeconds(string endpoint);
}
