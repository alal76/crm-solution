// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Infrastructure;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin-only system controls: rate limiting toggle and JWT secret rotation.
/// All endpoints require Admin role.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/system-controls")]
[Produces("application/json")]
public class SystemControlsController : CrmControllerBase
{
    private readonly ISystemControlsService _systemControls;
    private readonly ILogger<SystemControlsController> _logger;

    public SystemControlsController(
        ISystemControlsService systemControls,
        ILogger<SystemControlsController> logger)
    {
        _systemControls = systemControls ?? throw new ArgumentNullException(nameof(systemControls));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── Rate Limiting ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current rate limiting status (enabled, override active, last changed).
    /// </summary>
    [HttpGet("rate-limiting")]
    [ProducesResponseType(typeof(RateLimitingStatus), 200)]
    public IActionResult GetRateLimitingStatus()
    {
        return Ok(_systemControls.GetRateLimitingStatus());
    }

    /// <summary>
    /// Enables or disables rate limiting at runtime.
    /// The change is in-memory and resets on service restart.
    /// </summary>
    /// <param name="enabled">True to enable, false to disable.</param>
    [HttpPut("rate-limiting")]
    [ProducesResponseType(typeof(RateLimitingStatus), 200)]
    public IActionResult SetRateLimiting([FromBody] bool enabled)
    {
        var user = User.Identity?.Name ?? "admin-api";
        _systemControls.SetRateLimiting(enabled, user);

        _logger.LogInformation(
            "Rate limiting {Action} via admin API by {User}",
            enabled ? "ENABLED" : "DISABLED",
            user);

        return Ok(_systemControls.GetRateLimitingStatus());
    }

    // ── JWT Secret Rotation ─────────────────────────────────────────────────

    /// <summary>
    /// Returns the fingerprint (SHA-256 prefix) of the current JWT secret and
    /// the timestamp of the last rotation. The secret itself is never returned.
    /// </summary>
    [HttpGet("jwt-secret")]
    [ProducesResponseType(200)]
    public IActionResult GetJwtSecretInfo()
    {
        return Ok(new
        {
            fingerprint = _systemControls.GetJwtSecretFingerprint(),
            lastRotatedAt = _systemControls.GetLastJwtRotationTime(),
            note = "Fingerprint is the first 12 hex chars of SHA-256(secret). Safe to display."
        });
    }

    /// <summary>
    /// Rotates the JWT signing secret immediately. All currently issued tokens become
    /// invalid — every user will need to log in again.
    /// This action is logged and irreversible for the current session.
    /// </summary>
    [HttpPost("rotate-jwt-secret")]
    [ProducesResponseType(typeof(JwtRotationResult), 200)]
    [ProducesResponseType(403)]
    public IActionResult RotateJwtSecret()
    {
        var user = User.Identity?.Name ?? "admin-api";

        _logger.LogWarning(
            "JWT secret rotation initiated by {User} from {IP}",
            user,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        var result = _systemControls.RotateJwtSecret(user);
        return Ok(result);
    }
}
