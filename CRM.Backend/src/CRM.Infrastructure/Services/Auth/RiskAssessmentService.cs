// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Service for risk-based authentication (TODO-AUTH-022).
/// Calculates authentication risk and determines verification requirements.
/// </summary>
public class RiskAssessmentService : IRiskAssessmentService
{
    private readonly ILoginAnalyticsService _loginAnalyticsService;
    private readonly IGeoLocationService? _geoLocationService;
    private readonly ILogger<RiskAssessmentService> _logger;
    private RiskThresholds _thresholds;

    public RiskAssessmentService(
        ILoginAnalyticsService loginAnalyticsService,
        IConfiguration configuration,
        ILogger<RiskAssessmentService> logger,
        IGeoLocationService? geoLocationService = null)
    {
        _loginAnalyticsService = loginAnalyticsService;
        _geoLocationService = geoLocationService;
        _logger = logger;

        // Load thresholds from configuration
        _thresholds = new RiskThresholds
        {
            LowThreshold = configuration.GetValue("RiskAssessment:LowThreshold", 25),
            MediumThreshold = configuration.GetValue("RiskAssessment:MediumThreshold", 50),
            HighThreshold = configuration.GetValue("RiskAssessment:HighThreshold", 75),
            BlockThreshold = configuration.GetValue("RiskAssessment:BlockThreshold", 90),
            EnableRiskBasedMfa = configuration.GetValue("RiskAssessment:EnableRiskBasedMfa", true),
            MfaRequiredThreshold = configuration.GetValue("RiskAssessment:MfaRequiredThreshold", 50)
        };
    }

    /// <inheritdoc />
    public async Task<RiskAssessmentResult> AssessLoginRiskAsync(
        RiskAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var riskFactors = await CalculateRiskFactorsAsync(request, cancellationToken);
        var totalScore = riskFactors.Sum(f => f.Score);

        // Cap the score at 100
        totalScore = Math.Min(totalScore, 100);

        var riskLevel = DetermineRiskLevel(totalScore);

        var result = new RiskAssessmentResult
        {
            RiskScore = totalScore,
            RiskLevel = riskLevel,
            RiskFactors = riskFactors.ToList(),
            AllowAuthentication = totalScore < _thresholds.BlockThreshold,
            BlockReason = totalScore >= _thresholds.BlockThreshold ? "Risk score exceeds threshold" : null,
            RequiredActions = new RequiredAuthActions
            {
                RequireMfa = _thresholds.EnableRiskBasedMfa && totalScore >= _thresholds.MfaRequiredThreshold,
                RequireEmailVerification = totalScore >= _thresholds.HighThreshold,
                RequireCaptcha = totalScore >= _thresholds.MediumThreshold,
                DelaySeconds = totalScore >= _thresholds.HighThreshold ? 5 : null
            }
        };

        // Add recommendations
        if (totalScore >= _thresholds.MediumThreshold)
        {
            result.Recommendations.Add("Enable two-factor authentication for enhanced security");
        }
        if (riskFactors.Any(f => f.Code == "NEW_LOCATION"))
        {
            result.Recommendations.Add("Verify this login was you");
        }
        if (riskFactors.Any(f => f.Code == "VPN_PROXY"))
        {
            result.Recommendations.Add("Consider disabling VPN/proxy for login");
        }

        _logger.LogDebug("Risk assessment for {Email}: score={Score}, level={Level}, allow={Allow}",
            request.Email, totalScore, riskLevel, result.AllowAuthentication);

        return result;
    }

    /// <inheritdoc />
    public RiskThresholds GetRiskThresholds() => _thresholds;

    /// <inheritdoc />
    public Task UpdateRiskThresholdsAsync(
        RiskThresholds thresholds,
        CancellationToken cancellationToken = default)
    {
        _thresholds = thresholds;
        _logger.LogInformation("Risk thresholds updated: Low={Low}, Medium={Medium}, High={High}, Block={Block}",
            thresholds.LowThreshold, thresholds.MediumThreshold, thresholds.HighThreshold, thresholds.BlockThreshold);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RiskFactor>> CalculateRiskFactorsAsync(
        RiskAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var factors = new List<RiskFactor>();

        // Check VPN/Proxy
        if (_geoLocationService != null && !string.IsNullOrEmpty(request.IpAddress))
        {
            try
            {
                var isVpn = await _geoLocationService.IsVpnOrProxyAsync(request.IpAddress, cancellationToken);
                if (isVpn)
                {
                    factors.Add(new RiskFactor
                    {
                        Code = "VPN_PROXY",
                        Name = "VPN/Proxy Detected",
                        Description = "Login attempt from a known VPN or proxy service",
                        Score = 20,
                        Level = RiskLevel.Medium
                    });
                }

                var isTor = await _geoLocationService.IsTorExitNodeAsync(request.IpAddress, cancellationToken);
                if (isTor)
                {
                    factors.Add(new RiskFactor
                    {
                        Code = "TOR_EXIT",
                        Name = "Tor Exit Node",
                        Description = "Login attempt from a Tor exit node",
                        Score = 40,
                        Level = RiskLevel.High
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check VPN/Tor status for {IpAddress}", request.IpAddress);
            }
        }

        // Check for new location
        if (request.UserId.HasValue && !string.IsNullOrEmpty(request.CountryCode))
        {
            var isNew = await _loginAnalyticsService.IsNewLocationAsync(
                request.UserId.Value,
                request.CountryCode,
                request.City ?? "",
                cancellationToken);

            if (isNew)
            {
                factors.Add(new RiskFactor
                {
                    Code = "NEW_LOCATION",
                    Name = "New Location",
                    Description = $"First login from {request.City ?? request.CountryCode}",
                    Score = 25,
                    Level = RiskLevel.Medium
                });
            }
        }

        // Check unusual login time
        var hour = request.Timestamp.Hour;
        // Late night/early morning
        if (hour >= 0 && hour < 6)
        {
            factors.Add(new RiskFactor
            {
                Code = "UNUSUAL_TIME",
                Name = "Unusual Login Time",
                Description = "Login attempt during unusual hours",
                Score = 10,
                Level = RiskLevel.Low
            });
        }

        // Check failed attempts from IP
        if (!string.IsNullOrEmpty(request.IpAddress))
        {
            var failedAttempts = await _loginAnalyticsService.GetFailedAttemptsFromIpAsync(
                request.IpAddress, 15, cancellationToken);

            if (failedAttempts >= 10)
            {
                factors.Add(new RiskFactor
                {
                    Code = "BRUTE_FORCE",
                    Name = "Potential Brute Force",
                    Description = $"{failedAttempts} failed attempts from this IP in last 15 minutes",
                    Score = 50,
                    Level = RiskLevel.Critical
                });
            }
            else if (failedAttempts >= 5)
            {
                factors.Add(new RiskFactor
                {
                    Code = "MULTIPLE_FAILURES",
                    Name = "Multiple Failed Attempts",
                    Description = $"{failedAttempts} failed attempts from this IP recently",
                    Score = 25,
                    Level = RiskLevel.Medium
                });
            }
        }

        // Check user's anomaly patterns
        if (request.UserId.HasValue)
        {
            var anomalyResult = await _loginAnalyticsService.DetectAnomaliesAsync(
                request.UserId.Value,
                request.IpAddress ?? "",
                request.UserAgent ?? "",
                request.Timestamp,
                cancellationToken);

            if (anomalyResult.IsAnomalous)
            {
                foreach (var factor in anomalyResult.RiskFactors)
                {
                    factors.Add(new RiskFactor
                    {
                        Code = "ANOMALY_" + factor.ToUpperInvariant().Replace(" ", "_"),
                        Name = factor,
                        Description = $"Detected anomaly: {factor}",
                        Score = 15,
                        Level = RiskLevel.Medium
                    });
                }
            }
        }

        return factors;
    }

    /// <inheritdoc />
    public Task RecordRiskAssessmentAsync(
        int? userId,
        RiskAssessmentResult result,
        CancellationToken cancellationToken = default)
    {
        // In a full implementation, persist this to database
        _logger.LogInformation("Risk assessment recorded for user {UserId}: score={Score}, level={Level}",
            userId, result.RiskScore, result.RiskLevel);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<RiskAssessmentHistory>> GetRiskHistoryAsync(
        int userId,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        // In a full implementation, query from database
        return Task.FromResult<IEnumerable<RiskAssessmentHistory>>(Array.Empty<RiskAssessmentHistory>());
    }

    private RiskLevel DetermineRiskLevel(int score)
    {
        if (score >= _thresholds.HighThreshold) return RiskLevel.Critical;
        if (score >= _thresholds.MediumThreshold) return RiskLevel.High;
        if (score >= _thresholds.LowThreshold) return RiskLevel.Medium;
        return RiskLevel.Low;
    }
}
