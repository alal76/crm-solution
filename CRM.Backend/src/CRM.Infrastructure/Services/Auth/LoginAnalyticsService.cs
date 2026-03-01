// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Service for login analytics and anomaly detection (TODO-AUTH-021).
/// Tracks login patterns and detects suspicious activity.
/// </summary>
public class LoginAnalyticsService : ILoginAnalyticsService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IGeoLocationService? _geoLocationService;
    private readonly ILogger<LoginAnalyticsService> _logger;

    public LoginAnalyticsService(
        ICrmDbContext dbContext,
        ILogger<LoginAnalyticsService> logger,
        IGeoLocationService? geoLocationService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _geoLocationService = geoLocationService;
    }

    /// <inheritdoc />
    public async Task<LoginAttempt> RecordLoginAttemptAsync(
        LoginAttemptRecord attempt,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Get geolocation if service is available
        GeoLocationResult? geoLocation = null;
        if (_geoLocationService != null && !string.IsNullOrEmpty(attempt.IpAddress))
        {
            try
            {
                geoLocation = await _geoLocationService.LookupAsync(attempt.IpAddress, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to lookup geolocation for IP {IpAddress}", attempt.IpAddress);
            }
        }

        // Calculate risk score
        var anomalyResult = attempt.UserId.HasValue
            ? await DetectAnomaliesAsync(attempt.UserId.Value, attempt.IpAddress ?? "", attempt.UserAgent ?? "", now, cancellationToken)
            : new AnomalyDetectionResult { IsAnomalous = false, RiskScore = 0 };

        var loginAttempt = new LoginAttempt
        {
            UserId = attempt.UserId,
            Email = attempt.Email,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            Success = attempt.Success,
            FailureReason = attempt.FailureReason,
            DeviceFingerprint = attempt.DeviceFingerprint,
            RiskScore = anomalyResult.RiskScore,
            RiskFactors = anomalyResult.RiskFactors.Count > 0
                ? JsonSerializer.Serialize(anomalyResult.RiskFactors)
                : null,
            IsAnomalous = anomalyResult.IsAnomalous,
            CountryCode = geoLocation?.CountryCode,
            City = geoLocation?.City,
            Latitude = geoLocation?.Latitude,
            Longitude = geoLocation?.Longitude,
            HourOfDay = now.Hour,
            DayOfWeek = (int)now.DayOfWeek,
            CreatedAt = now
        };

        _dbContext.LoginAttempts.Add(loginAttempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Recorded login attempt for {Email} from {IpAddress}, success: {Success}, risk: {RiskScore}",
            attempt.Email, attempt.IpAddress, attempt.Success, anomalyResult.RiskScore);

        return loginAttempt;
    }

    /// <inheritdoc />
    public async Task<LoginStatistics> GetLoginStatisticsAsync(
        int userId,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        var attempts = await _dbContext.LoginAttempts
            .Where(a => a.UserId == userId && a.CreatedAt >= since)
            .ToListAsync(cancellationToken);

        var stats = new LoginStatistics
        {
            TotalLogins = attempts.Count,
            SuccessfulLogins = attempts.Count(a => a.Success),
            FailedLogins = attempts.Count(a => !a.Success),
            UniqueIpAddresses = attempts.Where(a => !string.IsNullOrEmpty(a.IpAddress)).Select(a => a.IpAddress).Distinct().Count(),
            UniqueDevices = attempts.Where(a => !string.IsNullOrEmpty(a.DeviceFingerprint)).Select(a => a.DeviceFingerprint).Distinct().Count(),
            AnomalousLogins = attempts.Count(a => a.IsAnomalous),
            LastSuccessfulLogin = attempts.Where(a => a.Success).OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt,
            LastFailedLogin = attempts.Where(a => !a.Success).OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt
        };

        // Logins by hour
        stats.LoginsByHour = attempts
            .GroupBy(a => a.HourOfDay)
            .ToDictionary(g => g.Key.ToString("D2"), g => g.Count());

        // Logins by day of week
        stats.LoginsByDay = attempts
            .GroupBy(a => a.DayOfWeek)
            .ToDictionary(g => ((DayOfWeek)g.Key).ToString(), g => g.Count());

        // Logins by country
        stats.LoginsByCountry = attempts
            .Where(a => !string.IsNullOrEmpty(a.CountryCode))
            .GroupBy(a => a.CountryCode!)
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LoginAttempt>> GetRecentLoginsAsync(
        int userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoginAttempts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(
        int userId,
        string ipAddress,
        string userAgent,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var result = new AnomalyDetectionResult
        {
            RiskScore = 0,
            RiskFactors = new List<string>()
        };

        // Get user's login patterns
        var patterns = await GetLoginPatternsAsync(userId, cancellationToken);

        // Check for unusual login time
        var hour = timestamp.Hour;
        if (patterns.TypicalHours.Length > 0 && !patterns.TypicalHours.Contains(hour))
        {
            result.RiskFactors.Add("Unusual login time");
            result.RiskScore += 15;
        }

        // Check for unusual day of week
        var dayOfWeek = (int)timestamp.DayOfWeek;
        if (patterns.TypicalDays.Length > 0 && !patterns.TypicalDays.Contains(dayOfWeek))
        {
            result.RiskFactors.Add("Unusual day of week");
            result.RiskScore += 10;
        }

        // Check for new IP address
        if (patterns.KnownIpAddresses.Length > 0 && !patterns.KnownIpAddresses.Contains(ipAddress))
        {
            result.RiskFactors.Add("New IP address");
            result.RiskScore += 20;
        }

        // Check for rapid login attempts (more than 5 in last 5 minutes)
        var recentAttempts = await GetFailedAttemptsFromIpAsync(ipAddress, 5, cancellationToken);
        if (recentAttempts >= 5)
        {
            result.RiskFactors.Add("Multiple recent login attempts");
            result.RiskScore += 25;
        }

        // Check for new device
        if (patterns.KnownDevices.Length > 0 && !patterns.KnownDevices.Any(d => userAgent.Contains(d)))
        {
            result.RiskFactors.Add("New device");
            result.RiskScore += 15;
        }

        // Determine if anomalous (score >= 40)
        result.IsAnomalous = result.RiskScore >= 40;
        result.RequiresAdditionalVerification = result.RiskScore >= 50;
        result.RecommendedAction = result.RiskScore >= 70 ? "Block and require password reset"
            : result.RiskScore >= 50 ? "Require 2FA"
            : result.RiskScore >= 40 ? "Send security alert"
            : null;

        return result;
    }

    /// <inheritdoc />
    public async Task<int> GetFailedAttemptsFromIpAsync(
        string ipAddress,
        int minutes = 15,
        CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutes);

        return await _dbContext.LoginAttempts
            .CountAsync(a =>
                a.IpAddress == ipAddress &&
                a.CreatedAt >= since &&
                !a.Success,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LoginPatterns> GetLoginPatternsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddDays(-90); // Analyze last 90 days

        var successfulLogins = await _dbContext.LoginAttempts
            .Where(a => a.UserId == userId && a.Success && a.CreatedAt >= since)
            .ToListAsync(cancellationToken);

        if (successfulLogins.Count < 5)
        {
            // Not enough data to establish patterns
            return new LoginPatterns();
        }

        // Find typical hours (hours with > 10% of logins)
        var hourCounts = successfulLogins.GroupBy(a => a.HourOfDay)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToList();
        var threshold = successfulLogins.Count * 0.1;
        var typicalHours = hourCounts.Where(h => h.Count >= threshold).Select(h => h.Hour).ToArray();

        // Find typical days
        var dayCounts = successfulLogins.GroupBy(a => a.DayOfWeek)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToList();
        var typicalDays = dayCounts.Where(d => d.Count >= threshold).Select(d => d.Day).ToArray();

        // Get known IPs (most recent 10)
        var knownIps = successfulLogins
            .Where(a => !string.IsNullOrEmpty(a.IpAddress))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.IpAddress!)
            .Distinct()
            .Take(10)
            .ToArray();

        // Get known countries
        var knownCountries = successfulLogins
            .Where(a => !string.IsNullOrEmpty(a.CountryCode))
            .Select(a => a.CountryCode!)
            .Distinct()
            .ToArray();

        // Get known device patterns from user agents
        var knownDevices = successfulLogins
            .Where(a => !string.IsNullOrEmpty(a.UserAgent))
            .Select(a => ExtractDeviceIdentifier(a.UserAgent!))
            .Distinct()
            .Take(5)
            .ToArray();

        // Calculate average logins per week
        var weeks = (DateTime.UtcNow - since).TotalDays / 7;
        var avgLoginsPerWeek = weeks > 0 ? successfulLogins.Count / weeks : 0;

        return new LoginPatterns
        {
            TypicalHours = typicalHours,
            TypicalDays = typicalDays,
            KnownIpAddresses = knownIps,
            KnownCountries = knownCountries,
            KnownDevices = knownDevices,
            AverageLoginsPerWeek = avgLoginsPerWeek
        };
    }

    /// <inheritdoc />
    public async Task<bool> IsNewLocationAsync(
        int userId,
        string countryCode,
        string city,
        CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddDays(-90);

        var hasLoggedFromLocation = await _dbContext.LoginAttempts
            .AnyAsync(a =>
                a.UserId == userId &&
                a.Success &&
                a.CreatedAt >= since &&
                a.CountryCode == countryCode &&
                a.City == city,
                cancellationToken);

        return !hasLoggedFromLocation;
    }

    /// <summary>
    /// Extracts a simple device identifier from a user agent string.
    /// </summary>
    private static string ExtractDeviceIdentifier(string userAgent)
    {
        // Simple extraction - in production use a proper UA parser
        if (userAgent.Contains("Windows"))
        {
            return "Windows";
        }
        if (userAgent.Contains("Mac"))
        {
            return "Mac";
        }
        if (userAgent.Contains("iPhone"))
        {
            return "iPhone";
        }
        if (userAgent.Contains("iPad"))
        {
            return "iPad";
        }
        if (userAgent.Contains("Android"))
        {
            return "Android";
        }
        if (userAgent.Contains("Linux"))
        {
            return "Linux";
        }
        return "Unknown";
    }
}
