// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Customer Portal authentication endpoints (public — no CRM auth required).
/// </summary>
[ApiController]
[Route("api/portal/auth")]
[AllowAnonymous]
public class PortalAuthController : ControllerBase
{
    private readonly IPortalAuthService _portalAuth;

    public PortalAuthController(
        IPortalAuthService portalAuth)
    {
        _portalAuth = portalAuth;
    }

    /// <summary>
    /// POST /api/portal/auth/login
    /// Authenticate a portal user. Returns a portal-scoped JWT on success.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] PortalLoginDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _portalAuth.LoginAsync(dto, ct);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }

    /// <summary>
    /// POST /api/portal/auth/register
    /// Self-register a portal account. Requires portal to be enabled and
    /// AllowSelfRegistration to be true.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] PortalRegisterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _portalAuth.RegisterAsync(dto, ct);
            return CreatedAtAction(nameof(Login), new { }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/portal/auth/forgot-password
    /// Request a password reset email.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto?.Email))
            return BadRequest(new { message = "Email is required." });

        await _portalAuth.ForgotPasswordAsync(dto.Email, ct);
        // Always return OK to avoid revealing whether the email is registered
        return Ok(new { message = "If that email address is registered, a reset link has been sent." });
    }

    /// <summary>
    /// POST /api/portal/auth/reset-password
    /// Reset password using a token from the reset email.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto?.Token) || string.IsNullOrWhiteSpace(dto?.NewPassword))
            return BadRequest(new { message = "Token and NewPassword are required." });

        var success = await _portalAuth.ResetPasswordAsync(dto.Token, dto.NewPassword, ct);
        if (!success)
            return BadRequest(new { message = "Invalid or expired reset token." });

        return Ok(new { message = "Password has been reset successfully." });
    }

    /// <summary>
    /// GET /api/portal/auth/verify-email?token={token}
    /// Verify an email address using the token sent in the verification email.
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Verification token is required." });

        var success = await _portalAuth.VerifyEmailAsync(token, ct);
        if (!success)
            return BadRequest(new { message = "Invalid or already-used verification token." });

        return Ok(new { message = "Email verified successfully." });
    }
}

/// <summary>Request body for forgot-password.</summary>
public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>Request body for reset-password.</summary>
public class ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
