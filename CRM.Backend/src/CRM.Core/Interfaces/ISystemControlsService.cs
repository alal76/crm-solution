// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Status of the rate limiting toggle.
/// </summary>
public class RateLimitingStatus
{
    /// <summary>Whether rate limiting is currently active.</summary>
    public bool Enabled { get; set; }

    /// <summary>Original startup value from appsettings.</summary>
    public bool StartupDefault { get; set; }

    /// <summary>Whether the current state differs from the startup default.</summary>
    public bool OverrideActive { get; set; }

    /// <summary>UTC timestamp of when the state was last changed.</summary>
    public DateTime LastChangedAt { get; set; }

    /// <summary>User or system that last changed the state (if known).</summary>
    public string? LastChangedBy { get; set; }
}

/// <summary>
/// Result of a JWT secret rotation.
/// </summary>
public class JwtRotationResult
{
    /// <summary>Whether the rotation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Partial SHA-256 hash of the new secret (first 12 chars, safe to display).</summary>
    public string SecretFingerprint { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the rotation.</summary>
    public DateTime RotatedAt { get; set; }

    /// <summary>User that triggered the rotation.</summary>
    public string? RotatedBy { get; set; }

    /// <summary>
    /// Warning: all existing JWT tokens are now invalid and users must re-authenticate.
    /// </summary>
    public string Warning { get; set; } = "All active sessions have been invalidated. Users must log in again.";
}

/// <summary>
/// Runtime system controls: rate limiting toggle and JWT secret rotation.
/// This service exposes admin-only controls for live infrastructure management.
/// </summary>
public interface ISystemControlsService
{
    /// <summary>Returns the current rate limiting status including override state.</summary>
    RateLimitingStatus GetRateLimitingStatus();

    /// <summary>
    /// Enables or disables rate limiting at runtime.
    /// The change takes effect immediately for new requests.
    /// </summary>
    /// <param name="enabled">Whether to enable rate limiting.</param>
    /// <param name="changedBy">Identity of the admin making the change.</param>
    void SetRateLimiting(bool enabled, string? changedBy = null);

    /// <summary>
    /// Rotates the JWT signing secret. All currently issued tokens become invalid immediately.
    /// Use only in emergencies or as part of a planned secret rotation schedule.
    /// </summary>
    /// <param name="rotatedBy">Identity of the admin requesting rotation.</param>
    /// <returns>Rotation result with a fingerprint of the new secret.</returns>
    JwtRotationResult RotateJwtSecret(string? rotatedBy = null);

    /// <summary>Returns a partial fingerprint (safe substring) of the current JWT secret.</summary>
    string GetJwtSecretFingerprint();

    /// <summary>Returns the UTC timestamp of the last JWT rotation, or null if never rotated.</summary>
    DateTime? GetLastJwtRotationTime();
}
