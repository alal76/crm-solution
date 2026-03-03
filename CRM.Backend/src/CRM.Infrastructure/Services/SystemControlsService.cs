// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Singleton runtime system controls service.
/// Manages the rate limiting override toggle and JWT secret hot-rotation.
/// </summary>
public sealed class SystemControlsService : ISystemControlsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemControlsService> _logger;

    // In-memory mutable rate limiting state (thread-safe with volatile + lock)
    private readonly object _lock = new();
    private volatile bool _rateLimitingEnabled;
    private readonly bool _startupDefault;
    private DateTime _rateLimitLastChangedAt = DateTime.UtcNow;
    private string? _rateLimitLastChangedBy;

    // JWT rotation audit
    private DateTime? _lastJwtRotationAt;
    private string? _lastJwtRotatedBy;

    public SystemControlsService(
        IConfiguration configuration,
        ILogger<SystemControlsService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Seed initial state from config
        var isDevelopment = string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);

        _startupDefault = _configuration.GetSection("RateLimiting")
            .GetValue("EnableEndpointRateLimiting", !isDevelopment);

        _rateLimitingEnabled = _startupDefault;
    }

    // ── Rate Limiting ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public RateLimitingStatus GetRateLimitingStatus()
    {
        lock (_lock)
        {
            return new RateLimitingStatus
            {
                Enabled = _rateLimitingEnabled,
                StartupDefault = _startupDefault,
                OverrideActive = _rateLimitingEnabled != _startupDefault,
                LastChangedAt = _rateLimitLastChangedAt,
                LastChangedBy = _rateLimitLastChangedBy
            };
        }
    }

    /// <inheritdoc/>
    public void SetRateLimiting(bool enabled, string? changedBy = null)
    {
        lock (_lock)
        {
            var previous = _rateLimitingEnabled;
            _rateLimitingEnabled = enabled;
            _rateLimitLastChangedAt = DateTime.UtcNow;
            _rateLimitLastChangedBy = changedBy;

            _logger.LogWarning(
                "Rate limiting toggled: {Previous} → {New} by {User} at {Time}",
                previous ? "ENABLED" : "DISABLED",
                enabled ? "ENABLED" : "DISABLED",
                changedBy ?? "unknown",
                _rateLimitLastChangedAt);
        }
    }

    // ── JWT Rotation ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public JwtRotationResult RotateJwtSecret(string? rotatedBy = null)
    {
        // Generate a cryptographically strong 64-char Base64 secret
        var bytes = new byte[48];
        RandomNumberGenerator.Fill(bytes);
        var newSecret = Convert.ToBase64String(bytes);

        // Hot-swap the in-memory configuration
        // Note: this affects AddJwtBearer token validation via IConfiguration binding
        _configuration["Jwt:Secret"] = newSecret;

        _lastJwtRotationAt = DateTime.UtcNow;
        _lastJwtRotatedBy = rotatedBy;

        var fingerprint = ComputeFingerprint(newSecret);

        _logger.LogWarning(
            "JWT secret rotated by {User} at {Time}. New fingerprint: {Fingerprint}. " +
            "All active sessions are now invalidated.",
            rotatedBy ?? "unknown",
            _lastJwtRotationAt,
            fingerprint);

        return new JwtRotationResult
        {
            Success = true,
            SecretFingerprint = fingerprint,
            RotatedAt = _lastJwtRotationAt.Value,
            RotatedBy = rotatedBy
        };
    }

    /// <inheritdoc/>
    public string GetJwtSecretFingerprint()
    {
        var secret = _configuration["Jwt:Secret"] ?? string.Empty;
        return ComputeFingerprint(secret);
    }

    /// <inheritdoc/>
    public DateTime? GetLastJwtRotationTime() => _lastJwtRotationAt;

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ComputeFingerprint(string secret)
    {
        if (string.IsNullOrEmpty(secret))
        {
            return "(none)";
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        // Return safe 12-char hex prefix — enough to confirm it changed, not enough to reconstruct
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }
}
