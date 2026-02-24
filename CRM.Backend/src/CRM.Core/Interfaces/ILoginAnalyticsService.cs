// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for login analytics and anomaly detection (TODO-AUTH-021).
/// Tracks login patterns and detects suspicious activity.
/// </summary>
public interface ILoginAnalyticsService
{
    /// <summary>
    /// Records a login attempt for analytics.
    /// </summary>
    /// <param name="attempt">Login attempt details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<LoginAttempt> RecordLoginAttemptAsync(
        LoginAttemptRecord attempt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets login statistics for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<LoginStatistics> GetLoginStatisticsAsync(
        int userId,
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent login attempts for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="count">Number of attempts to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<LoginAttempt>> GetRecentLoginsAsync(
        int userId,
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects anomalies in a login attempt.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ipAddress">IP address of the attempt</param>
    /// <param name="userAgent">User agent string</param>
    /// <param name="timestamp">Timestamp of the attempt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<AnomalyDetectionResult> DetectAnomaliesAsync(
        int userId,
        string ipAddress,
        string userAgent,
        DateTime timestamp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed login attempts for an IP address within a time window.
    /// </summary>
    /// <param name="ipAddress">IP address</param>
    /// <param name="minutes">Time window in minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> GetFailedAttemptsFromIpAsync(
        string ipAddress,
        int minutes = 15,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets login patterns for a user (typical login hours, IPs, etc.).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<LoginPatterns> GetLoginPatternsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if login is from a new location for the user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="countryCode">Country code from GeoIP</param>
    /// <param name="city">City from GeoIP</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsNewLocationAsync(
        int userId,
        string countryCode,
        string city,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Record for creating a login attempt
/// </summary>
public class LoginAttemptRecord
{
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? DeviceFingerprint { get; set; }
}

/// <summary>
/// Login statistics summary
/// </summary>
public class LoginStatistics
{
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public int UniqueIpAddresses { get; set; }
    public int UniqueDevices { get; set; }
    public int AnomalousLogins { get; set; }
    public DateTime? LastSuccessfulLogin { get; set; }
    public DateTime? LastFailedLogin { get; set; }
    public Dictionary<string, int> LoginsByHour { get; set; } = new();
    public Dictionary<string, int> LoginsByDay { get; set; } = new();
    public Dictionary<string, int> LoginsByCountry { get; set; } = new();
}

/// <summary>
/// Result of anomaly detection
/// </summary>
public class AnomalyDetectionResult
{
    public bool IsAnomalous { get; set; }
    public int RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public bool RequiresAdditionalVerification { get; set; }
    public string? RecommendedAction { get; set; }
}

/// <summary>
/// User's typical login patterns
/// </summary>
public class LoginPatterns
{
    public int[] TypicalHours { get; set; } = Array.Empty<int>();
    public int[] TypicalDays { get; set; } = Array.Empty<int>();
    public string[] KnownIpAddresses { get; set; } = Array.Empty<string>();
    public string[] KnownCountries { get; set; } = Array.Empty<string>();
    public string[] KnownDevices { get; set; } = Array.Empty<string>();
    public double AverageLoginsPerWeek { get; set; }
}
