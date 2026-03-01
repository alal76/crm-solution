// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Authentication Controller for user login, registration, token management, and two-factor authentication.
/// Provides endpoints for secure user authentication, password management, and OAuth integration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : CrmControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;
    private readonly LinkedInOAuthProvider _linkedInOAuthProvider;
    private readonly AppleOAuthProvider _appleOAuthProvider;
    private readonly IWebAuthnService _webAuthnService;
    private readonly IOAuthStateService _oauthStateService;
    private readonly ITwoFactorPolicyService _twoFactorPolicyService;
    private readonly CRM.Core.Interfaces.ITotpService _totpService;
    private readonly ISessionManager _sessionManager;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IAuthAuditService _authAuditService;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IUserOAuthLinkService _userOAuthLinkService;
    private readonly IOktaSsoService _oktaSsoService;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly ILoginAnalyticsService _loginAnalyticsService;
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly IDeviceAuthorizationService _deviceAuthorizationService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly IOpenIdConnectService _openIdConnectService;
    private readonly IBiometricAuthService _biometricAuthService;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger,
        LinkedInOAuthProvider linkedInOAuthProvider,
        AppleOAuthProvider appleOAuthProvider,
        IWebAuthnService webAuthnService,
        IOAuthStateService oauthStateService,
        ITwoFactorPolicyService twoFactorPolicyService,
        CRM.Core.Interfaces.ITotpService totpService,
        ISessionManager sessionManager,
        IPasswordHistoryService passwordHistoryService,
        IAuthAuditService authAuditService,
        IMagicLinkService magicLinkService,
        IUserOAuthLinkService userOAuthLinkService,
        IOktaSsoService oktaSsoService,
        ITrustedDeviceService trustedDeviceService,
        ILoginAnalyticsService loginAnalyticsService,
        IRiskAssessmentService riskAssessmentService,
        IDeviceAuthorizationService deviceAuthorizationService,
        IGeoLocationService geoLocationService,
        IOpenIdConnectService openIdConnectService,
        IBiometricAuthService biometricAuthService)
    {
        _authenticationService = authenticationService;
        _logger = logger;
        _linkedInOAuthProvider = linkedInOAuthProvider;
        _appleOAuthProvider = appleOAuthProvider;
        _webAuthnService = webAuthnService;
        _oauthStateService = oauthStateService;
        _twoFactorPolicyService = twoFactorPolicyService;
        _totpService = totpService;
        _sessionManager = sessionManager;
        _passwordHistoryService = passwordHistoryService;
        _authAuditService = authAuditService;
        _magicLinkService = magicLinkService;
        _userOAuthLinkService = userOAuthLinkService;
        _oktaSsoService = oktaSsoService;
        _trustedDeviceService = trustedDeviceService;
        _loginAnalyticsService = loginAnalyticsService;
        _riskAssessmentService = riskAssessmentService;
        _deviceAuthorizationService = deviceAuthorizationService;
        _geoLocationService = geoLocationService;
        _openIdConnectService = openIdConnectService;
        _biometricAuthService = biometricAuthService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="request">The registration request containing user details</param>
    /// <returns>The authentication response with tokens</returns>
    /// <response code="200">Returns the authentication tokens for the new user</response>
    /// <response code="400">If the registration data is invalid or email already exists</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Registration failed: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Registration validation failed: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Login with email and password credentials.
    /// </summary>
    /// <param name="request">The login request containing email and password</param>
    /// <returns>The authentication response with tokens or 2FA challenge</returns>
    /// <response code="200">Returns authentication tokens or 2FA token if enabled</response>
    /// <response code="400">If the login data is invalid</response>
    /// <response code="401">If credentials are incorrect or account is locked</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.LoginAsync(request);

            // Enforce concurrent session limit and record session (TODO-AUTH-013)
            try
            {
                await _sessionManager.EnforceSessionLimitAsync(response.UserId);
                await _sessionManager.CreateSessionAsync(
                    response.UserId,
                    response.AccessToken,
                    ipAddress,
                    userAgent,
                    response.ExpiresAt);
            }
            catch (Exception sessionEx)
            {
                _logger.LogWarning(sessionEx, "Session management error for user {UserId} — login proceeds", response.UserId);
            }

            // Audit log successful login (TODO-AUTH-016)
            await _authAuditService.LogLoginAttemptAsync(response.UserId, ipAddress, userAgent, true);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Login failed: {ex.Message}");
            await _authAuditService.LogLoginAttemptAsync(null, ipAddress, userAgent, false, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verify 2FA code during login to complete authentication.
    /// </summary>
    /// <param name="request">The 2FA verification request with token and code</param>
    /// <returns>The authentication response with tokens</returns>
    /// <response code="200">Returns authentication tokens upon successful 2FA verification</response>
    /// <response code="400">If the verification data is invalid</response>
    /// <response code="401">If the 2FA code is incorrect or expired</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("login/2fa")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginWith2FA([FromBody] TwoFactorLoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.VerifyTwoFactorLoginAsync(request.TwoFactorToken, request.Code);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"2FA login verification failed: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refresh an expired access token using a valid refresh token.
    /// Implements token rotation: the old refresh token is revoked and a new one is issued.
    /// If a revoked token is reused, all refresh tokens for the user are invalidated (theft detection).
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <returns>New authentication tokens (access + refresh)</returns>
    /// <response code="200">Returns new authentication tokens</response>
    /// <response code="400">If the refresh token is missing</response>
    /// <response code="401">If the refresh token is invalid, expired, or revoked</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Login using OAuth provider (Google, Microsoft, etc.).
    /// </summary>
    /// <param name="request">The OAuth login request with provider and access token</param>
    /// <returns>The authentication response with tokens</returns>
    /// <response code="200">Returns authentication tokens for the OAuth user</response>
    /// <response code="400">If the OAuth data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("oauth-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authenticationService.OAuthLoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Verify if a JWT token is valid.
    /// </summary>
    /// <param name="token">The JWT token to verify</param>
    /// <returns>Token validity status</returns>
    /// <response code="200">Returns token validity status</response>
    /// <response code="401">If the request is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("verify")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyToken([FromBody] string token)
    {
                var isValid = await _authenticationService.VerifyTokenAsync(token);
        return Ok(new { isValid });
    }

    /// <summary>
    /// Get the current authenticated user's profile.
    /// </summary>
    /// <returns>The current user's basic profile information</returns>
    /// <response code="200">Returns the current user's profile</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the user is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var user = await _authenticationService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new { user.Id, user.Username, user.Email, user.FirstName, user.LastName });
    }

    /// <summary>
    /// Setup two-factor authentication for the current user.
    /// </summary>
    /// <returns>The 2FA setup response with QR code and secret</returns>
    /// <response code="200">Returns 2FA setup details including QR code</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("2fa/setup")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Setup2FA()
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var response = await _authenticationService.SetupTwoFactorAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Verify and enable two-factor authentication with a code from authenticator app.
    /// </summary>
    /// <param name="request">The verification request containing the 2FA code</param>
    /// <returns>Verification success status</returns>
    /// <response code="200">Returns success message if verification passed</response>
    /// <response code="400">If the verification code is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("2fa/verify")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Verify2FA([FromBody] TwoFactorVerification request)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var isValid = await _authenticationService.VerifyTwoFactorCodeAsync(userId, request.Code);
        if (!isValid)
            return BadRequest(new { message = "Invalid verification code" });

        return Ok(new { message = "2FA verification successful" });
    }

    /// <summary>
    /// Enable two-factor authentication with secret and backup codes.
    /// </summary>
    /// <param name="request">The enable request containing secret and backup codes</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message if 2FA enabled</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Enable2FA([FromBody] TwoFactorEnableRequest request)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        await _authenticationService.EnableTwoFactorAsync(userId, request.Secret, request.BackupCodes);
        return Ok(new { message = "2FA enabled successfully" });
    }

    /// <summary>
    /// Disable two-factor authentication for the current user.
    /// </summary>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message if 2FA disabled</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("2fa/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Disable2FA()
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        await _authenticationService.DisableTwoFactorAsync(userId);
        return Ok(new { message = "2FA disabled successfully" });
    }

    /// <summary>
    /// Request a password reset email.
    /// </summary>
    /// <param name="request">The password reset request containing email</param>
    /// <returns>Confirmation message (does not reveal if email exists)</returns>
    /// <response code="200">Returns confirmation message</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] CreatePasswordResetDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authenticationService.RequestPasswordResetAsync(request.Email);
            // In production, send email with reset link containing the token
            return Ok(new { message = "Password reset email sent. Check your inbox." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Password reset request failed: {ex.Message}");
            // Don't reveal if email exists
            return Ok(new { message = "If an account exists with that email, a password reset link has been sent." });
        }
    }

    /// <summary>
    /// Confirm password reset with token and new password.
    /// </summary>
    /// <param name="request">The confirmation request with token and new password</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message if password reset</response>
    /// <response code="400">If the token is invalid/expired or passwords don't match</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });

            await _authenticationService.ResetPasswordAsync(request.Token, request.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Password reset failed: {ex.Message}");
            return BadRequest(new { message = "Invalid or expired reset token" });
        }
    }

    /// <summary>
    /// Admin reset user password by user ID. Requires admin role.
    /// </summary>
    /// <param name="userId">The ID of the user to reset password for</param>
    /// <param name="request">The admin password reset request containing new password</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message if password reset</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("reset-password/{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdminResetPassword(int userId, [FromBody] AdminPasswordResetDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify user is admin
            var userRoleClaim = User.FindFirst("role");
            if (userRoleClaim?.Value != "0") // 0 = Admin role
                return Forbid();

            await _authenticationService.AdminResetPasswordAsync(userId, request.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Admin password reset failed: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Admin password reset unauthorized: {ex.Message}");
            return Forbid();
        }
    }

    /// <summary>
    /// Set password for first-time login or expired password.
    /// </summary>
    /// <param name="request">The set password request with new password</param>
    /// <returns>The authentication response with tokens</returns>
    /// <response code="200">Returns authentication tokens after password set</response>
    /// <response code="400">If the password doesn't meet requirements</response>
    /// <response code="401">If the setup token is invalid or expired</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("setup-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetupPassword([FromBody] SetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.SetupPasswordAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Password setup failed: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Password setup validation failed: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get password complexity requirements for client-side validation.
    /// </summary>
    /// <returns>The password complexity requirements</returns>
    /// <response code="200">Returns password complexity requirements</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("password-requirements")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordComplexityRequirements), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPasswordRequirements()
    {
                var requirements = await _authenticationService.GetPasswordRequirementsAsync();
        return Ok(requirements);
    }

    /// <summary>
    /// Logout the current user by revoking all refresh tokens.
    /// </summary>
    /// <returns>Success status</returns>
    /// <response code="200">User successfully logged out</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return new UnauthorizedObjectResult(new { message = "User ID not found in token" });
        }

        var success = await _authenticationService.LogoutAsync(userId);

        // Revoke all sessions and audit (TODO-AUTH-013, TODO-AUTH-016)
        try { await _sessionManager.RevokeAllSessionsAsync(userId); } catch { /* non-critical */ }
        await _authAuditService.LogLogoutAsync(userId, ipAddress, userAgent);

        if (success)
        {
            return new OkObjectResult(new { message = "User logged out successfully" });
        }
        else
        {
            return StatusCode(500, new { message = "Failed to logout user" });
        }
    }

    /// <summary>
    /// Change the password for the current authenticated user.
    /// Requires verification of the old password.
    /// Enforces password complexity requirements.
    /// Revokes all existing refresh tokens (forces re-login).
    /// </summary>
    /// <param name="request">The change password request with old and new passwords</param>
    /// <returns>New authentication tokens</returns>
    /// <response code="200">Password successfully changed, returns new tokens</response>
    /// <response code="400">If the request is invalid or passwords don't meet requirements</response>
    /// <response code="401">If the old password is incorrect or user not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        try
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UnauthorizedObjectResult(new { message = "User ID not found in token" });
            }

            // Password history validation — reject reuse of last 5 passwords (TODO-AUTH-014)
            var isReused = await _passwordHistoryService.IsPasswordReusedAsync(userId, request.NewPassword);
            if (isReused)
            {
                await _authAuditService.LogPasswordChangeAsync(userId, ipAddress, userAgent, false,
                    "Password reuse rejected (last 5 passwords)");
                return new BadRequestObjectResult(new { message = "You cannot reuse one of your last 5 passwords. Please choose a different password." });
            }

            var response = await _authenticationService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

            // Record new password hash in history (TODO-AUTH-014)
            await _passwordHistoryService.RecordNewPasswordAsync(userId, request.NewPassword);

            // Audit log (TODO-AUTH-016)
            await _authAuditService.LogPasswordChangeAsync(userId, ipAddress, userAgent, true);

            return new OkObjectResult(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            var userIdClaim2 = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            int.TryParse(userIdClaim2?.Value, out var uid2);
            await _authAuditService.LogPasswordChangeAsync(uid2 > 0 ? uid2 : (int?)null ?? 0, ipAddress, userAgent, false, ex.Message);
            _logger.LogWarning($"Change password failed: {ex.Message}");
            return new UnauthorizedObjectResult(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Change password validation failed: {ex.Message}");
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }

    // ==========================================
    // LinkedIn OAuth Endpoints
    // ==========================================

    /// <summary>
    /// Initiate LinkedIn OAuth login flow — returns the authorization URL.
    /// </summary>
    /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
    /// <param name="redirectUri">The OAuth redirect URI registered with LinkedIn</param>
    /// <returns>Authorization URL and anti-CSRF state token</returns>
    /// <response code="200">Returns the LinkedIn authorization URL</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("oauth/linkedin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuthRedirectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetLinkedInAuthUrl(
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? redirectUri = null)
    {
                var state = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var effectiveRedirectUri = redirectUri ?? $"{Request.Scheme}://{Request.Host}/api/auth/oauth/linkedin/callback";

        var authorizationUrl = _linkedInOAuthProvider.GetAuthorizationUrl(state, effectiveRedirectUri);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            authorizationUrl += $"&return_url={Uri.EscapeDataString(returnUrl)}";
        }

        _logger.LogInformation("Generated LinkedIn OAuth authorization URL");
        return Ok(new OAuthRedirectDto
        {
            AuthorizationUrl = authorizationUrl,
            State = state
        });
    }

    /// <summary>
    /// LinkedIn OAuth callback — exchanges authorization code for token and logs in.
    /// </summary>
    /// <param name="dto">The OAuth callback data with authorization code</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication response with JWT tokens</returns>
    /// <response code="200">Returns authentication tokens for the LinkedIn user</response>
    /// <response code="400">If the callback data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("oauth/linkedin/callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LinkedInCallback([FromBody] OAuthCallbackDto dto, CancellationToken ct)
    {
                if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Authorization code is required" });

        var effectiveRedirectUri = dto.RedirectUri ?? $"{Request.Scheme}://{Request.Host}/api/auth/oauth/linkedin/callback";

        // Exchange authorization code for access token
        var tokenResponse = await _linkedInOAuthProvider.ExchangeCodeForTokenAsync(dto.Code, effectiveRedirectUri, ct);

        // Get user profile from LinkedIn
        var profile = await _linkedInOAuthProvider.GetUserProfileAsync(tokenResponse.Access_token, ct);

        if (string.IsNullOrEmpty(profile.Email))
        {
            _logger.LogWarning("LinkedIn OAuth: No email returned for user {LinkedInId}", profile.Id);
            return BadRequest(new { message = "LinkedIn account does not have an accessible email address" });
        }

        // Delegate to the existing OAuthLoginAsync which handles find-or-create user and JWT generation
        var oauthRequest = new OAuthLoginRequest
        {
            Provider = "linkedin",
            Token = tokenResponse.Access_token
        };

        var response = await _authenticationService.OAuthLoginAsync(oauthRequest);
        _logger.LogInformation("LinkedIn OAuth login successful for {Email}", profile.Email);
        return Ok(response);
    }

    // ==========================================
    // Apple OAuth Endpoints
    // ==========================================

    /// <summary>
    /// Initiate Apple OAuth login flow — returns the authorization URL.
    /// </summary>
    /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
    /// <param name="redirectUri">The OAuth redirect URI registered with Apple</param>
    /// <returns>Authorization URL and anti-CSRF state token</returns>
    /// <response code="200">Returns the Apple authorization URL</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("oauth/apple")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuthRedirectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAppleAuthUrl(
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? redirectUri = null)
    {
                var state = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var effectiveRedirectUri = redirectUri ?? $"{Request.Scheme}://{Request.Host}/api/auth/oauth/apple/callback";

        var authorizationUrl = _appleOAuthProvider.GetAuthorizationUrl(state, effectiveRedirectUri);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            authorizationUrl += $"&return_url={Uri.EscapeDataString(returnUrl)}";
        }

        _logger.LogInformation("Generated Apple OAuth authorization URL");
        return Ok(new OAuthRedirectDto
        {
            AuthorizationUrl = authorizationUrl,
            State = state
        });
    }

    /// <summary>
    /// Apple OAuth callback — exchanges authorization code for token and logs in.
    /// Apple uses JWT client assertion and returns user info only on first sign-in.
    /// </summary>
    /// <param name="dto">The OAuth callback data with authorization code</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication response with JWT tokens</returns>
    /// <response code="200">Returns authentication tokens for the Apple user</response>
    /// <response code="400">If the callback data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("oauth/apple/callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AppleCallback([FromBody] OAuthCallbackDto dto, CancellationToken ct)
    {
                if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Authorization code is required" });

        var effectiveRedirectUri = dto.RedirectUri ?? $"{Request.Scheme}://{Request.Host}/api/auth/oauth/apple/callback";

        // Exchange authorization code for access token (Apple uses JWT client secret)
        var tokenResponse = await _appleOAuthProvider.ExchangeCodeForTokenAsync(dto.Code, effectiveRedirectUri, ct);

        // Decode the ID token to get user info
        AppleUserProfile? profile = null;
        if (!string.IsNullOrEmpty(tokenResponse.Id_token))
        {
            profile = _appleOAuthProvider.DecodeIdToken(tokenResponse.Id_token);
        }

        // Apple sends user data only on first authorization — merge if present
        if (!string.IsNullOrEmpty(dto.UserData))
        {
            var parsedProfile = _appleOAuthProvider.ParseUserResponse(dto.UserData, profile?.Email);
            if (profile != null)
            {
                profile.FirstName ??= parsedProfile.FirstName;
                profile.LastName ??= parsedProfile.LastName;
            }
            else
            {
                profile = parsedProfile;
            }
        }

        if (profile == null || string.IsNullOrEmpty(profile.Email))
        {
            _logger.LogWarning("Apple OAuth: No email could be extracted from callback");
            return BadRequest(new { message = "Apple account did not provide an email address" });
        }

        // Delegate to the existing OAuthLoginAsync
        var oauthRequest = new OAuthLoginRequest
        {
            Provider = "apple",
            Token = tokenResponse.Id_token ?? tokenResponse.Access_token ?? string.Empty
        };

        var response = await _authenticationService.OAuthLoginAsync(oauthRequest);
        _logger.LogInformation("Apple OAuth login successful for {Email}", profile.Email);
        return Ok(response);
    }

    // ==========================================
    // WebAuthn / FIDO2 Endpoints
    // ==========================================

    /// <summary>
    /// Get WebAuthn registration options for creating a new credential.
    /// Returns a challenge and relying party info for navigator.credentials.create().
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>WebAuthn registration options with challenge</returns>
    /// <response code="200">Returns registration options (challenge, RP info, user entity)</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("webauthn/register/options")]
    [Authorize]
    [ProducesResponseType(typeof(WebAuthnRegistrationOptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWebAuthnRegistrationOptions(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var emailClaim = User.FindFirst("email") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            var nameClaim = User.FindFirst("name") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

            var userEmail = emailClaim?.Value ?? string.Empty;
            var userName = nameClaim?.Value ?? userEmail;

            var options = await _webAuthnService.InitiateRegistrationAsync(userId, userEmail, userName, ct);
            return Ok(options);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("WebAuthn registration options failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Complete WebAuthn registration with client attestation response.
    /// Verifies the attestation and stores the new credential.
    /// </summary>
    /// <param name="dto">Attestation response and credential name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The registered credential details</returns>
    /// <response code="200">Returns the registered credential</response>
    /// <response code="400">If the attestation is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("webauthn/register/complete")]
    [Authorize]
    [ProducesResponseType(typeof(WebAuthnCredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteWebAuthnRegistration([FromBody] WebAuthnRegistrationCompleteDto dto, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var credential = await _webAuthnService.CompleteRegistrationAsync(
                userId,
                dto.CredentialName,
                dto.Attestation,
                ct);

            _logger.LogInformation("WebAuthn credential registered for user {UserId}", userId);
            return Ok(credential);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("WebAuthn registration completion failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("WebAuthn registration invalid: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get WebAuthn authentication options (challenge) for passwordless login.
    /// Returns a challenge for navigator.credentials.get().
    /// </summary>
    /// <param name="dto">Optional email to scope the challenge to a specific user</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>WebAuthn authentication options with challenge and allowed credentials</returns>
    /// <response code="200">Returns authentication options (challenge, allowed credentials)</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("webauthn/login/options")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WebAuthnAuthenticationOptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWebAuthnLoginOptions([FromBody] WebAuthnLoginInitiateDto dto, CancellationToken ct)
    {
        try
        {
            var userEmail = dto.Email ?? string.Empty;
            var options = await _webAuthnService.InitiateAuthenticationAsync(userEmail, ct);
            return Ok(options);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("WebAuthn login options failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Complete WebAuthn passwordless authentication.
    /// Verifies the assertion and returns JWT tokens.
    /// </summary>
    /// <param name="dto">Assertion response with credential ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication response with JWT tokens</returns>
    /// <response code="200">Returns authentication tokens</response>
    /// <response code="400">If the assertion is invalid</response>
    /// <response code="401">If authentication fails</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("webauthn/login/complete")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteWebAuthnLogin([FromBody] WebAuthnLoginCompleteDto dto, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CredentialId))
                return BadRequest(new { message = "Credential ID is required" });

            var result = await _webAuthnService.CompleteAuthenticationAsync(
                dto.Email ?? string.Empty,
                dto.CredentialId,
                dto.Assertion,
                ct);

            if (!result.IsValid || !result.UserId.HasValue)
            {
                _logger.LogWarning("WebAuthn authentication failed: {Error}", result.ErrorMessage);
                return Unauthorized(new { message = result.ErrorMessage ?? "WebAuthn authentication failed" });
            }

            // Generate JWT tokens for the authenticated user
            var user = await _authenticationService.GetUserByIdAsync(result.UserId.Value);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Use the existing OAuthLogin flow to generate tokens for the WebAuthn-authenticated user
            // This reuses the token generation logic without requiring a password
            var oauthRequest = new OAuthLoginRequest
            {
                Provider = "webauthn",
                Token = result.CredentialId ?? dto.CredentialId
            };

            var response = await _authenticationService.OAuthLoginAsync(oauthRequest);
            _logger.LogInformation("WebAuthn login successful for user {UserId} with credential {CredentialId}",
                result.UserId.Value, result.CredentialId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("WebAuthn login completion failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("WebAuthn login unauthorized: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// List all registered WebAuthn credentials for the current user.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of registered WebAuthn credentials</returns>
    /// <response code="200">Returns list of credentials</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("webauthn/credentials")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<WebAuthnCredentialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWebAuthnCredentials(CancellationToken ct)
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var credentials = await _webAuthnService.GetCredentialsAsync(userId, ct);
        return Ok(credentials);
    }

    /// <summary>
    /// Remove a registered WebAuthn credential.
    /// </summary>
    /// <param name="credentialId">The credential ID to remove</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Credential successfully removed</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the credential was not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("webauthn/credentials/{credentialId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveWebAuthnCredential(string credentialId, CancellationToken ct)
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var success = await _webAuthnService.RemoveCredentialAsync(userId, credentialId, ct);
        if (!success)
            return NotFound(new { message = "Credential not found" });

        _logger.LogInformation("WebAuthn credential {CredentialId} removed for user {UserId}", credentialId, userId);
        return Ok(new { message = "WebAuthn credential removed successfully" });
    }

    // ==========================================
    // OAuth State / CSRF Protection Endpoints
    // ==========================================

    /// <summary>
    /// Generate an OAuth state token for CSRF protection.
    /// The returned state should be passed as the state parameter when initiating OAuth flows.
    /// </summary>
    /// <param name="returnUrl">Optional URL to redirect to after OAuth callback</param>
    /// <returns>A cryptographically random state token</returns>
    /// <response code="200">Returns the generated state token</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("oauth/state")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GenerateOAuthState([FromQuery] string? returnUrl = null)
    {
                var state = _oauthStateService.GenerateState(returnUrl);
        return Ok(new { state });
    }

    /// <summary>
    /// Validate an OAuth state token (typically called internally during callback processing).
    /// Consumes the token (one-time use) and returns the embedded return URL.
    /// </summary>
    /// <param name="request">The request containing the state token to validate</param>
    /// <returns>Validation result with optional return URL</returns>
    /// <response code="200">Returns validation result</response>
    /// <response code="400">If the state token is invalid, expired, or already consumed</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("oauth/state/validate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult ValidateOAuthState([FromBody] OAuthStateValidateRequest request)
    {
                if (string.IsNullOrWhiteSpace(request.State))
            return BadRequest(new { message = "State parameter is required" });

        var isValid = _oauthStateService.ValidateState(request.State, out var returnUrl);
        if (!isValid)
            return BadRequest(new { message = "Invalid, expired, or already consumed state token" });

        return Ok(new { isValid = true, returnUrl });
    }

    // ==========================================
    // OAuth Token Refresh Endpoint
    // ==========================================

    /// <summary>
    /// Refresh an OAuth provider token (not JWT refresh, which is at POST /api/auth/refresh).
    /// Uses the provider's refresh token to obtain a new access token from the external OAuth provider.
    /// </summary>
    /// <param name="dto">The OAuth refresh request with provider name and refresh token</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>New OAuth token response from the provider</returns>
    /// <response code="200">Returns new OAuth tokens from the provider</response>
    /// <response code="400">If the request data is invalid or provider is unsupported</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("oauth/refresh")]
    [Authorize]
    [ProducesResponseType(typeof(OAuthTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshOAuthToken([FromBody] OAuthRefreshDto dto, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Provider))
                return BadRequest(new { message = "Provider is required" });

            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(new { message = "Refresh token is required" });

            var provider = dto.Provider.ToLowerInvariant();

            OAuthTokenResponseDto tokenResponse;

            switch (provider)
            {
                case "linkedin":
                    var linkedInResult = await _linkedInOAuthProvider.RefreshTokenAsync(dto.RefreshToken, ct);
                    tokenResponse = new OAuthTokenResponseDto
                    {
                        AccessToken = linkedInResult.Access_token,
                        ExpiresIn = linkedInResult.Expires_in,
                        RefreshToken = linkedInResult.Refresh_token
                    };
                    break;

                case "apple":
                    var appleResult = await _appleOAuthProvider.RefreshTokenAsync(dto.RefreshToken, ct);
                    tokenResponse = new OAuthTokenResponseDto
                    {
                        AccessToken = appleResult.Access_token ?? string.Empty,
                        ExpiresIn = appleResult.Expires_in,
                        RefreshToken = appleResult.Refresh_token,
                        IdToken = appleResult.Id_token
                    };
                    break;

                default:
                    return BadRequest(new { message = $"OAuth token refresh not supported for provider: {dto.Provider}" });
            }

            _logger.LogInformation("OAuth token refreshed for provider {Provider}", dto.Provider);
            return Ok(tokenResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "OAuth token refresh failed for provider {Provider}", dto.Provider);
            return BadRequest(new { message = $"Failed to refresh token with provider: {ex.Message}" });
        }
    }

    // ==========================================
    // 2FA Policy Enforcement Endpoints
    // ==========================================

    /// <summary>
    /// Get all 2FA enforcement policies for all user groups.
    /// Requires Admin role.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of 2FA policies per group</returns>
    /// <response code="200">Returns all 2FA policies</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an Admin</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("2fa/policies")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<TwoFactorPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get2FAPolicies(CancellationToken ct)
    {
                var policies = await _twoFactorPolicyService.GetAllPoliciesAsync(ct);
        return Ok(policies);
    }

    /// <summary>
    /// Set or update the 2FA enforcement policy for a specific user group.
    /// Requires Admin role.
    /// </summary>
    /// <param name="groupId">The user group ID to set the policy for</param>
    /// <param name="policy">The 2FA policy configuration</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated policy</returns>
    /// <response code="200">Returns success message</response>
    /// <response code="400">If the policy data is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an Admin</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("2fa/policies/{groupId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set2FAPolicy(int groupId, [FromBody] TwoFactorPolicyDto policy, CancellationToken ct)
    {
                if (groupId <= 0)
            return BadRequest(new { message = "Invalid group ID" });

        await _twoFactorPolicyService.SetPolicyForGroupAsync(groupId, policy, ct);

        var updated = await _twoFactorPolicyService.GetPolicyForGroupAsync(groupId, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Check if 2FA is required for the current authenticated user based on their group memberships.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Whether 2FA is required</returns>
    /// <response code="200">Returns 2FA requirement status</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("2fa/required")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Check2FARequired(CancellationToken ct)
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var isRequired = await _twoFactorPolicyService.Is2FARequiredForUserAsync(userId, ct);
        return Ok(new { required = isRequired });
    }

    // ==========================================
    // Backup Code Regeneration Endpoint
    // ==========================================

    /// <summary>
    /// Regenerate 2FA backup codes for the current user.
    /// Generates 10 new backup codes, invalidates all previous ones,
    /// and returns the plain-text codes ONCE (user must save them).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>New set of backup codes</returns>
    /// <response code="200">Returns new backup codes in plain text (one-time display)</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("2fa/backup-codes/regenerate")]
    [Authorize]
    [ProducesResponseType(typeof(BackupCodesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegenerateBackupCodes(CancellationToken ct)
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var result = await _totpService.RegenerateBackupCodesAsync(userId);

        _logger.LogInformation("Backup codes regenerated for user {UserId}, {Count} codes issued", userId, result.TotalCodes);
        return Ok(result);
    }

    // ==========================================
    // Supporting Request DTOs
    // ==========================================

    // ==========================================
    // Auth Audit Logs (TODO-AUTH-016)
    // ==========================================

    /// <summary>
    /// Get paginated authentication audit logs. Admins can retrieve all users' logs; pass
    /// userId query parameter to filter. Requires Admin role.
    /// </summary>
    [HttpGet("audit-logs")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int? userId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
                pageSize = Math.Min(pageSize, 100);
        page = Math.Max(page, 1);

        var (items, total) = await _authAuditService.GetUserAuditLogsAsync(userId, page, pageSize, ct);

        return Ok(new
        {
            items,
            totalCount = total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    // ==========================================
    // Magic Link Passwordless Login (TODO-AUTH-017)
    // ==========================================

    /// <summary>
    /// Request a passwordless magic-link login email. Token expires in 15 minutes and
    /// is single-use. Responds 200 even when the email is not found (to prevent enumeration).
    /// </summary>
    [HttpPost("magic-link/request")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestMagicLink(
        [FromBody] MagicLinkRequestDto request,
        CancellationToken ct = default)
    {
                if (string.IsNullOrWhiteSpace(request?.Email))
            return BadRequest(new { message = "Email is required." });

        MagicLinkToken? magic = null;
        try
        {
            magic = await _magicLinkService.GenerateMagicLinkAsync(request.Email, ct);
        }
        catch (KeyNotFoundException)
        {
            // Do not reveal whether the email exists
            return Ok(new { message = "If that email is registered, a magic link has been sent." });
        }

        // Build the link — front-end route handles the verification page
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var magicLink = $"{baseUrl}/auth/magic-link?token={Uri.EscapeDataString(magic.Token)}";

        await _magicLinkService.SendMagicLinkEmailAsync(request.Email, magicLink, ct);

        _logger.LogInformation("Magic link sent to {Email}", request.Email);
        return Ok(new { message = "If that email is registered, a magic link has been sent." });
    }

    /// <summary>
    /// Verify a magic-link token and exchange it for a JWT access token.
    /// The token is invalidated on first use and expires in 15 minutes.
    /// </summary>
    [HttpPost("magic-link/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyMagicLink(
        [FromBody] MagicLinkVerifyDto request,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
                return BadRequest(new { message = "Token is required." });

            var response = await _magicLinkService.ValidateMagicLinkAsync(request.Token, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Magic link verification failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    // ==========================================
    // OAuth Account Linking (TODO-AUTH-018)
    // ==========================================

    /// <summary>
    /// Get all linked OAuth providers for the current authenticated user.
    /// </summary>
    [HttpGet("oauth/links")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOAuthLinks(CancellationToken ct = default)
    {
                var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var links = await _userOAuthLinkService.GetLinksAsync(userId, ct);

        return Ok(links.Select(l => new
        {
            l.Id,
            l.Provider,
            l.ProviderEmail,
            l.CreatedAt
        }));
    }

    /// <summary>
    /// Link an OAuth provider account to the current authenticated user.
    /// </summary>
    [HttpPost("oauth/link")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LinkOAuthProvider(
        [FromBody] OAuthLinkRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Provider) || string.IsNullOrWhiteSpace(request.ProviderUserId))
                return BadRequest(new { message = "Provider and ProviderUserId are required." });

            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var link = await _userOAuthLinkService.LinkProviderAsync(
                userId, request.Provider, request.ProviderUserId,
                request.ProviderEmail, request.AccessToken, ct);

            _logger.LogInformation("OAuth provider '{Provider}' linked for user {UserId}", request.Provider, userId);
            return Ok(new { link.Id, link.Provider, link.ProviderEmail, link.CreatedAt });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Unlink an OAuth provider from the current authenticated user.
    /// Fails if this is the user's only remaining authentication method.
    /// </summary>
    [HttpDelete("oauth/link/{provider}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnlinkOAuthProvider(
        string provider,
        CancellationToken ct = default)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            await _userOAuthLinkService.UnlinkProviderAsync(userId, provider, ct);

            _logger.LogInformation("OAuth provider '{Provider}' unlinked for user {UserId}", provider, userId);
            return Ok(new { message = $"Provider '{provider}' successfully unlinked." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =========================================================================
    // Generic OIDC Endpoints (TODO-AUTH-004)
    // =========================================================================

    /// <summary>
    /// Get the authorization URL for a configured OIDC provider.
    /// </summary>
    [HttpGet("oidc/authorize")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OidcAuthorize(
        [FromQuery] string provider,
        [FromQuery] string? redirectUri = null,
        CancellationToken ct = default)
    {
                if (string.IsNullOrWhiteSpace(provider))
            return BadRequest(new { message = "Provider name is required." });

        if (!_openIdConnectService.IsProviderConfigured(provider))
            return BadRequest(new { message = $"OIDC provider '{provider}' is not configured." });

        var state = Guid.NewGuid().ToString("N");
        var nonce = Guid.NewGuid().ToString("N");
        var codeVerifier = _openIdConnectService.GenerateCodeVerifier();

        // Store state/nonce/verifier in session or cache for callback validation
        HttpContext.Session.SetString($"oidc_{state}_nonce", nonce);
        HttpContext.Session.SetString($"oidc_{state}_verifier", codeVerifier);
        HttpContext.Session.SetString($"oidc_{state}_provider", provider);

        var authUrl = await _openIdConnectService.GetAuthorizationUrlAsync(provider, state, nonce, codeVerifier);

        return Ok(new { authorizationUrl = authUrl, state });
    }

    /// <summary>
    /// Handle OIDC provider callback with authorization code exchange.
    /// </summary>
    [HttpPost("oidc/callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OidcCallback(
        [FromBody] OidcCallbackDto dto,
        CancellationToken ct = default)
    {
                if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Authorization code is required." });

        // Retrieve stored state data
        var provider = dto.Provider;
        string? codeVerifier = null;
        string? nonce = null;

        if (!string.IsNullOrEmpty(dto.State))
        {
            nonce = HttpContext.Session.GetString($"oidc_{dto.State}_nonce");
            codeVerifier = HttpContext.Session.GetString($"oidc_{dto.State}_verifier");
            var storedProvider = HttpContext.Session.GetString($"oidc_{dto.State}_provider");

            if (!string.IsNullOrEmpty(storedProvider))
                provider = storedProvider;

            // Clean up session
            HttpContext.Session.Remove($"oidc_{dto.State}_nonce");
            HttpContext.Session.Remove($"oidc_{dto.State}_verifier");
            HttpContext.Session.Remove($"oidc_{dto.State}_provider");
        }

        if (string.IsNullOrWhiteSpace(provider))
            return BadRequest(new { message = "Provider name is required." });

        var result = await _openIdConnectService.ExchangeCodeAsync(
            provider, dto.Code, codeVerifier, nonce, ct);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorDescription ?? result.Error ?? "OIDC authentication failed." });

        if (result.UserProfile == null || string.IsNullOrEmpty(result.UserProfile.Email))
            return BadRequest(new { message = "Email not returned from OIDC provider." });

        // Auto-provision or login existing user
        var response = await _authenticationService.OAuthLoginAsync(new OAuthLoginRequest
        {
            Provider = provider,
            ProviderUserId = result.UserProfile.Sub,
            Email = result.UserProfile.Email,
            FirstName = result.UserProfile.GivenName,
            LastName = result.UserProfile.FamilyName
        });

        return Ok(response);
    }

    /// <summary>
    /// List configured OIDC providers.
    /// </summary>
    [HttpGet("oidc/providers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetOidcProviders()
    {
        var providers = _openIdConnectService.GetConfiguredProviders();
        return Ok(providers);
    }

    // =========================================================================
    // Biometric Authentication Endpoints (TODO-AUTH-010)
    // =========================================================================

    /// <summary>
    /// Get biometric authentication options (challenge) for verifying a credential.
    /// </summary>
    [HttpPost("biometric/options")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BiometricAuthenticationOptions), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBiometricAuthOptions(
        [FromBody] BiometricAuthOptionsRequestDto? request = null,
        CancellationToken ct = default)
    {
                var options = await _biometricAuthService.GetAuthenticationOptionsAsync(request?.UserId, ct);
        return Ok(options);
    }

    /// <summary>
    /// Verify biometric authentication (WebAuthn platform credential).
    /// POST /api/auth/biometric/verify
    /// </summary>
    [HttpPost("biometric/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyBiometric(
        [FromBody] BiometricAuthenticationResponse request,
        CancellationToken ct = default)
    {
                if (string.IsNullOrWhiteSpace(request.CredentialId))
            return BadRequest(new { message = "Credential ID is required." });

        var result = await _biometricAuthService.ValidateAuthenticationAsync(request, ct);

        if (!result.Success || !result.UserId.HasValue)
        {
            _logger.LogWarning("Biometric verification failed: {Error} ({Code})", result.Error, result.ErrorCode);
            return Unauthorized(new { message = result.Error ?? "Biometric verification failed.", errorCode = result.ErrorCode });
        }

        // Generate auth tokens for the authenticated user
        var response = await _authenticationService.GenerateTokensForUserAsync(result.UserId.Value);
        if (response == null)
            return Unauthorized(new { message = "User not found or inactive." });

        // Record login analytics
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        await _loginAnalyticsService.RecordLoginAttemptAsync(new LoginAttemptRecord
        {
            UserId = result.UserId,
            Email = response.Email ?? "",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = true,
            DeviceFingerprint = request.CredentialId
        }, ct);

        return Ok(response);
    }

    /// <summary>
    /// Get biometric credentials for the current user.
    /// </summary>
    [HttpGet("biometric/credentials")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<BiometricCredentialInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBiometricCredentials(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var credentials = await _biometricAuthService.GetUserCredentialsAsync(userId.Value, ct);
        return Ok(credentials);
    }

    /// <summary>
    /// Register a new biometric credential for the current user.
    /// </summary>
    [HttpPost("biometric/register")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterBiometric(
        [FromBody] BiometricRegistrationResponse response,
        [FromQuery] string? deviceName = null,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = await _biometricAuthService.CompleteRegistrationAsync(userId.Value, response, deviceName, ct);
        if (!success)
            return BadRequest(new { message = "Biometric registration failed." });

        return Ok(new { message = "Biometric credential registered successfully." });
    }

    // =========================================================================
    // Okta SSO Endpoints (TODO-AUTH-003)
    // =========================================================================

    /// <summary>
    /// Initiate Okta SSO login flow.
    /// </summary>
    [HttpGet("sso/okta")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult InitiateOktaSso([FromQuery] string? redirectUri = null, [FromQuery] string? state = null)
    {
        var effectiveRedirectUri = redirectUri ?? $"{Request.Scheme}://{Request.Host}/api/auth/sso/okta/callback";
        var authUrl = _oktaSsoService.GetAuthorizationUrl(effectiveRedirectUri, state ?? Guid.NewGuid().ToString());
        return Redirect(authUrl);
    }

    /// <summary>
    /// Handle Okta SSO callback.
    /// </summary>
    [HttpPost("sso/okta/callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OktaSsoCallback(
        [FromBody] OktaSsoCallbackDto dto,
        CancellationToken ct = default)
    {
                if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Authorization code is required." });

        var redirectUri = dto.RedirectUri ?? $"{Request.Scheme}://{Request.Host}/api/auth/sso/okta/callback";
        var tokens = await _oktaSsoService.ExchangeCodeForTokenAsync(dto.Code, redirectUri, ct);
        var userInfo = await _oktaSsoService.GetUserInfoAsync(tokens.AccessToken!, ct);

        if (string.IsNullOrWhiteSpace(userInfo.Email))
            return BadRequest(new { message = "Email not returned from Okta." });

        // Auto-provision or login existing user
        var response = await _authenticationService.OAuthLoginAsync(new OAuthLoginRequest
        {
            Provider = "okta",
            ProviderUserId = userInfo.ProviderId,
            Email = userInfo.Email,
            FirstName = userInfo.GivenName,
            LastName = userInfo.FamilyName
        });

        return Ok(response);
    }

    // =========================================================================
    // Trusted Device Endpoints (TODO-AUTH-019)
    // =========================================================================

    /// <summary>
    /// Get trusted devices for the current user.
    /// </summary>
    [HttpGet("devices/trusted")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TrustedDeviceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrustedDevices(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var devices = await _trustedDeviceService.GetTrustedDevicesAsync(userId.Value, false, ct);
        return Ok(devices.Select(d => new TrustedDeviceDto
        {
            Id = d.Id,
            DeviceFingerprint = d.DeviceFingerprint,
            DeviceName = d.DeviceName,
            LastUsedAt = d.LastUsedAt,
            TrustedUntil = d.TrustedUntil,
            TrustedFromIp = d.TrustedFromIp,
            CreatedAt = d.CreatedAt
        }));
    }

    /// <summary>
    /// Trust the current device for 2FA.
    /// </summary>
    [HttpPost("devices/trust")]
    [Authorize]
    [ProducesResponseType(typeof(TrustedDeviceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> TrustDevice(
        [FromBody] TrustDeviceRequestDto request,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var device = await _trustedDeviceService.TrustDeviceAsync(
            userId.Value,
            request.DeviceId,
            request.DeviceName ?? userAgent,
            ipAddress,
            userAgent,
            30,
            ct);

        return Ok(new TrustedDeviceDto
        {
            Id = device.Id,
            DeviceFingerprint = device.DeviceFingerprint,
            DeviceName = device.DeviceName,
            LastUsedAt = device.LastUsedAt,
            TrustedUntil = device.TrustedUntil,
            TrustedFromIp = device.TrustedFromIp,
            CreatedAt = device.CreatedAt
        });
    }

    /// <summary>
    /// Revoke trust for a specific device.
    /// </summary>
    [HttpDelete("devices/trusted/{deviceId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeTrustedDevice(string deviceId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var deviceIdInt = int.TryParse(deviceId, out var did) ? did : 0;
        var revoked = await _trustedDeviceService.RevokeDeviceAsync(userId.Value, deviceIdInt, ct);
        if (!revoked) return NotFound(new { message = "Device not found." });

        return Ok(new { message = "Device trust revoked." });
    }

    // =========================================================================
    // Login Analytics Endpoints (TODO-AUTH-021)
    // =========================================================================

    /// <summary>
    /// Get login statistics for the current user.
    /// </summary>
    [HttpGet("analytics/login-stats")]
    [Authorize]
    [ProducesResponseType(typeof(LoginStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginStatistics(
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var stats = await _loginAnalyticsService.GetLoginStatisticsAsync(userId.Value, days, ct);
        return Ok(stats);
    }

    /// <summary>
    /// Get recent login history for the current user.
    /// </summary>
    [HttpGet("analytics/recent-logins")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LoginAttemptRecord>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentLogins(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var logins = await _loginAnalyticsService.GetRecentLoginsAsync(userId.Value, count, ct);
        return Ok(logins);
    }

    // =========================================================================
    // Risk Assessment Endpoints (TODO-AUTH-022)
    // =========================================================================

    /// <summary>
    /// Assess risk for a login attempt (for administrative purposes).
    /// </summary>
    [HttpPost("risk/assess")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RiskAssessmentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssessRisk(
        [FromBody] RiskAssessmentRequestDto request,
        CancellationToken ct = default)
    {
        var result = await _riskAssessmentService.AssessLoginRiskAsync(new RiskAssessmentRequest
        {
            UserId = request.UserId,
            IpAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = request.UserAgent ?? Request.Headers.UserAgent.ToString(),
            DeviceFingerprint = request.DeviceId
        }, ct);

        return Ok(result);
    }

    /// <summary>
    /// Get risk thresholds configuration.
    /// </summary>
    [HttpGet("risk/thresholds")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RiskThresholds), StatusCodes.Status200OK)]
    public IActionResult GetRiskThresholds()
    {
        var thresholds = _riskAssessmentService.GetRiskThresholds();
        return Ok(thresholds);
    }

    // =========================================================================
    // Device Authorization Flow Endpoints (TODO-AUTH-023)
    // =========================================================================

    /// <summary>
    /// Initiate device authorization flow (RFC 8628).
    /// Returns device_code and user_code for the device to display.
    /// </summary>
    [HttpPost("device/authorize")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeviceAuthorizationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> InitiateDeviceAuthorization(
        [FromBody] DeviceAuthorizationRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            return BadRequest(new { message = "client_id is required." });

        var response = await _deviceAuthorizationService.InitiateDeviceAuthorizationAsync(
            request.ClientId,
            request.Scope,
            ct);

        // Include verification_uri as per RFC 8628
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new
        {
            device_code = response.DeviceCode,
            user_code = response.UserCode,
            verification_uri = $"{baseUrl}/device",
            verification_uri_complete = $"{baseUrl}/device?user_code={response.UserCode}",
            expires_in = response.ExpiresIn,
            interval = response.Interval
        });
    }

    /// <summary>
    /// Poll for device authorization token (called by the device).
    /// </summary>
    [HttpPost("device/token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeviceTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeviceToken(
        [FromBody] DeviceTokenRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceCode))
            return BadRequest(new { error = "invalid_request", error_description = "device_code is required." });

        var response = await _deviceAuthorizationService.PollForTokenAsync(request.DeviceCode, request.ClientId ?? "default", ct);

        if (response.Error != null)
        {
            return BadRequest(new { error = response.Error, error_description = response.ErrorDescription });
        }

        return Ok(new
        {
            access_token = response.AccessToken,
            token_type = response.TokenType,
            expires_in = response.ExpiresIn,
            refresh_token = response.RefreshToken
        });
    }

    /// <summary>
    /// User authorizes a device using the user_code (called from the web UI).
    /// </summary>
    [HttpPost("device/confirm")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmDeviceAuthorization(
        [FromBody] DeviceConfirmRequestDto request,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.UserCode))
            return BadRequest(new { message = "user_code is required." });

        var success = await _deviceAuthorizationService.AuthorizeDeviceAsync(request.UserCode, userId.Value, ct);
        if (!success)
            return NotFound(new { message = "Invalid or expired user code." });

        return Ok(new { message = "Device authorized successfully." });
    }

    // =========================================================================
    // Geolocation Alerts (TODO-AUTH-024)
    // =========================================================================

    /// <summary>
    /// Check if a login location is anomalous for the user.
    /// </summary>
    [HttpPost("geo/check")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GeoLocationCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckGeoLocation(
        [FromBody] GeoLocationCheckRequestDto request,
        CancellationToken ct = default)
    {
        var geoResult = await _geoLocationService.LookupAsync(request.IpAddress, ct);
        var isNew = await _loginAnalyticsService.IsNewLocationAsync(request.UserId, geoResult?.CountryCode ?? "unknown", geoResult?.City ?? "unknown", ct);

        return Ok(new GeoLocationCheckResult
        {
            IpAddress = request.IpAddress,
            Country = geoResult?.CountryCode,
            City = geoResult?.City,
            Latitude = geoResult?.Latitude,
            Longitude = geoResult?.Longitude,
            IsNewLocation = isNew,
            IsVpnOrProxy = geoResult?.IsVpn ?? false
        });
    }

    // ─── Helper Methods ───────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return null;
    }

    // ─── DTOs for Auth Advanced Features ──────────────────────────────────────

    /// <summary>Okta SSO callback request.</summary>
    public class OktaSsoCallbackDto
    {
        public string Code { get; set; } = string.Empty;
        public string? RedirectUri { get; set; }
        public string? State { get; set; }
    }

    /// <summary>OIDC callback request.</summary>
    public class OidcCallbackDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public string? State { get; set; }
        public string? RedirectUri { get; set; }
    }

    /// <summary>Biometric auth options request.</summary>
    public class BiometricAuthOptionsRequestDto
    {
        public int? UserId { get; set; }
    }

    /// <summary>Trust device request.</summary>
    public class TrustDeviceRequestDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
    }

    // TrustedDeviceDto is defined in CRM.Core.Dtos

    /// <summary>Risk assessment request.</summary>
    public class RiskAssessmentRequestDto
    {
        public int UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceId { get; set; }
    }

    /// <summary>Device authorization request (RFC 8628).</summary>
    public class DeviceAuthorizationRequestDto
    {
        public string ClientId { get; set; } = string.Empty;
        public string? Scope { get; set; }
    }

    /// <summary>Device token polling request.</summary>
    public class DeviceTokenRequestDto
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string? ClientId { get; set; }
    }

    /// <summary>Device confirmation request.</summary>
    public class DeviceConfirmRequestDto
    {
        public string UserCode { get; set; } = string.Empty;
    }

    /// <summary>Geolocation check request.</summary>
    public class GeoLocationCheckRequestDto
    {
        public int UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }

    /// <summary>Geolocation check result.</summary>
    public class GeoLocationCheckResult
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? City { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsNewLocation { get; set; }
        public bool IsVpnOrProxy { get; set; }
    }

    /// <summary>
    /// Request DTO for validating an OAuth state token.
    /// </summary>
    public class OAuthStateValidateRequest
    {
        /// <summary>The state token to validate.</summary>
        public string State { get; set; } = string.Empty;
    }

    // ─── Supporting DTOs for new auth features ───────────────────────────────

    /// <summary>Request DTO for magic link generation.</summary>
    public class MagicLinkRequestDto
    {
        /// <summary>The email address to send the magic link to.</summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>Request DTO for magic link token verification.</summary>
    public class MagicLinkVerifyDto
    {
        /// <summary>The one-time token extracted from the magic link URL.</summary>
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>Request DTO for linking an OAuth provider account.</summary>
    public class OAuthLinkRequestDto
    {
        /// <summary>Provider identifier: google | microsoft | github | linkedin | apple</summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>The unique user identifier returned by the OAuth provider.</summary>
        public string ProviderUserId { get; set; } = string.Empty;

        /// <summary>Optional email address from the OAuth provider.</summary>
        public string? ProviderEmail { get; set; }

        /// <summary>Optional OAuth access token.</summary>
        public string? AccessToken { get; set; }
    }
}
