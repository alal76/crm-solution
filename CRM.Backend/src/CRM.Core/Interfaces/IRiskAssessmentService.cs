// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for risk-based authentication (TODO-AUTH-022).
/// Calculates authentication risk and determines verification requirements.
/// </summary>
public interface IRiskAssessmentService
{
    /// <summary>
    /// Assesses the risk of an authentication attempt.
    /// </summary>
    /// <param name="request">Risk assessment request with context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Risk assessment result with score and required actions</returns>
    Task<RiskAssessmentResult> AssessLoginRiskAsync(
        RiskAssessmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets risk thresholds configuration.
    /// </summary>
    RiskThresholds GetRiskThresholds();

    /// <summary>
    /// Updates risk thresholds configuration.
    /// </summary>
    Task UpdateRiskThresholdsAsync(
        RiskThresholds thresholds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates individual risk factors for an authentication attempt.
    /// </summary>
    Task<IEnumerable<RiskFactor>> CalculateRiskFactorsAsync(
        RiskAssessmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records risk assessment result for analytics.
    /// </summary>
    Task RecordRiskAssessmentAsync(
        int? userId,
        RiskAssessmentResult result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets risk assessment history for a user.
    /// </summary>
    Task<IEnumerable<RiskAssessmentHistory>> GetRiskHistoryAsync(
        int userId,
        int days = 30,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for risk assessment
/// </summary>
public class RiskAssessmentRequest
{
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceFingerprint { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string>? AdditionalContext { get; set; }
}

/// <summary>
/// Result of risk assessment
/// </summary>
public class RiskAssessmentResult
{
    /// <summary>
    /// Overall risk score (0-100, higher = more risky)
    /// </summary>
    public int RiskScore { get; set; }

    /// <summary>
    /// Risk level classification
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Individual risk factors that contributed to the score
    /// </summary>
    public List<RiskFactor> RiskFactors { get; set; } = new();

    /// <summary>
    /// Required authentication actions based on risk
    /// </summary>
    public RequiredAuthActions RequiredActions { get; set; } = new();

    /// <summary>
    /// Whether to allow the authentication to proceed
    /// </summary>
    public bool AllowAuthentication { get; set; } = true;

    /// <summary>
    /// Reason for blocking if AllowAuthentication is false
    /// </summary>
    public string? BlockReason { get; set; }

    /// <summary>
    /// Recommendations for the user
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Risk level classification
/// </summary>
public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Individual risk factor
/// </summary>
public class RiskFactor
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Score { get; set; }
    public RiskLevel Level { get; set; }
}

/// <summary>
/// Required authentication actions
/// </summary>
public class RequiredAuthActions
{
    public bool RequireMfa { get; set; }
    public bool RequireEmailVerification { get; set; }
    public bool RequirePasswordChange { get; set; }
    public bool RequireCaptcha { get; set; }
    public bool RequireAdminApproval { get; set; }
    public int? DelaySeconds { get; set; }
}

/// <summary>
/// Risk thresholds configuration
/// </summary>
public class RiskThresholds
{
    public int LowThreshold { get; set; } = 25;
    public int MediumThreshold { get; set; } = 50;
    public int HighThreshold { get; set; } = 75;
    public int BlockThreshold { get; set; } = 90;
    public bool EnableRiskBasedMfa { get; set; } = true;
    public int MfaRequiredThreshold { get; set; } = 50;
}

/// <summary>
/// Risk assessment history record
/// </summary>
public class RiskAssessmentHistory
{
    public DateTime Timestamp { get; set; }
    public int RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string? IpAddress { get; set; }
    public string? CountryCode { get; set; }
    public bool AuthenticationAllowed { get; set; }
}
