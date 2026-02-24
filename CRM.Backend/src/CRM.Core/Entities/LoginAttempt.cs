// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Tracks login attempts for analytics and anomaly detection (TODO-AUTH-021, TODO-AUTH-022).
/// Used for risk-based authentication and security monitoring.
/// </summary>
public class LoginAttempt : BaseEntity
{
    /// <summary>
    /// User ID (null for failed attempts with unknown users)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Email address used in the login attempt
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the login attempt
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Whether the login succeeded
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Failure reason if not successful
    /// </summary>
    [MaxLength(255)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Risk score calculated for this attempt (0-100)
    /// </summary>
    public int RiskScore { get; set; } = 0;

    /// <summary>
    /// Risk factors detected (JSON array)
    /// </summary>
    public string? RiskFactors { get; set; }

    /// <summary>
    /// Country code from GeoIP lookup
    /// </summary>
    [MaxLength(3)]
    public string? CountryCode { get; set; }

    /// <summary>
    /// City from GeoIP lookup
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// Latitude from GeoIP lookup
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude from GeoIP lookup
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Whether this attempt was flagged as anomalous
    /// </summary>
    public bool IsAnomalous { get; set; } = false;

    /// <summary>
    /// Whether user was alerted about this login
    /// </summary>
    public bool AlertSent { get; set; } = false;

    /// <summary>
    /// Device fingerprint hash
    /// </summary>
    [MaxLength(128)]
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// Hour of day (0-23) for pattern analysis
    /// </summary>
    public int HourOfDay { get; set; }

    /// <summary>
    /// Day of week (0=Sunday) for pattern analysis
    /// </summary>
    public int DayOfWeek { get; set; }

    // Navigation property
    public virtual User? User { get; set; }
}
