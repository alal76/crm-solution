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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Authentication Controller for user login, registration, token management, and two-factor authentication.
/// Provides endpoints for secure user authentication, password management, and OAuth integration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;
    private readonly LinkedInOAuthProvider _linkedInOAuthProvider;
    private readonly AppleOAuthProvider _appleOAuthProvider;
    private readonly IWebAuthnService _webAuthnService;

    public AuthController(
        IAuthenticationService authenticationService,
        ILogger<AuthController> logger,
        LinkedInOAuthProvider linkedInOAuthProvider,
        AppleOAuthProvider appleOAuthProvider,
        IWebAuthnService webAuthnService)
    {
        _authenticationService = authenticationService;
        _logger = logger;
        _linkedInOAuthProvider = linkedInOAuthProvider;
        _appleOAuthProvider = appleOAuthProvider;
        _webAuthnService = webAuthnService;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration", detail = ex.Message });
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
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Login failed: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login", detail = ex.Message });
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
        catch (Exception ex)
        {
            _logger.LogError($"2FA login error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during 2FA verification" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh error");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
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
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authenticationService.OAuthLoginAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"OAuth login error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during OAuth login" });
        }
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
        try
        {
            var isValid = await _authenticationService.VerifyTokenAsync(token);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Token verification error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during token verification" });
        }
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
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _authenticationService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new { user.Id, user.Username, user.Email, user.FirstName, user.LastName });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Get user error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while retrieving user profile" });
        }
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
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var response = await _authenticationService.SetupTwoFactorAsync(userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"2FA setup error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during 2FA setup" });
        }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError($"2FA verification error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during 2FA verification" });
        }
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
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            await _authenticationService.EnableTwoFactorAsync(userId, request.Secret, request.BackupCodes);
            return Ok(new { message = "2FA enabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"2FA enable error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while enabling 2FA" });
        }
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
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            await _authenticationService.DisableTwoFactorAsync(userId);
            return Ok(new { message = "2FA disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"2FA disable error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while disabling 2FA" });
        }
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

            var token = await _authenticationService.RequestPasswordResetAsync(request.Email);
            // In production, send email with reset link containing the token
            return Ok(new { message = "Password reset email sent. Check your inbox." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Password reset request failed: {ex.Message}");
            // Don't reveal if email exists
            return Ok(new { message = "If an account exists with that email, a password reset link has been sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Password reset request error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during password reset request" });
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
        catch (Exception ex)
        {
            _logger.LogError($"Password reset error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during password reset" });
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
        catch (Exception ex)
        {
            _logger.LogError($"Admin password reset error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during password reset" });
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
        catch (Exception ex)
        {
            _logger.LogError($"Password setup error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during password setup" });
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
        try
        {
            var requirements = await _authenticationService.GetPasswordRequirementsAsync();
            return Ok(requirements);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Get password requirements error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred retrieving password requirements" });
        }
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
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UnauthorizedObjectResult(new { message = "User ID not found in token" });
            }

            var success = await _authenticationService.LogoutAsync(userId);
            if (success)
            {
                return new OkObjectResult(new { message = "User logged out successfully" });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to logout user" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Logout error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during logout" });
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
        try
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UnauthorizedObjectResult(new { message = "User ID not found in token" });
            }

            var response = await _authenticationService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            return new OkObjectResult(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Change password failed: {ex.Message}");
            return new UnauthorizedObjectResult(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Change password validation failed: {ex.Message}");
            return new BadRequestObjectResult(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Change password error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during password change" });
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate LinkedIn OAuth authorization URL");
            return StatusCode(500, new { message = "Failed to initiate LinkedIn OAuth flow" });
        }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "LinkedIn OAuth callback error");
            return StatusCode(500, new { message = "An error occurred during LinkedIn OAuth login" });
        }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Apple OAuth authorization URL");
            return StatusCode(500, new { message = "Failed to initiate Apple OAuth flow" });
        }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Apple OAuth callback error");
            return StatusCode(500, new { message = "An error occurred during Apple OAuth login" });
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebAuthn registration options error");
            return StatusCode(500, new { message = "An error occurred generating WebAuthn registration options" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebAuthn registration completion error");
            return StatusCode(500, new { message = "An error occurred completing WebAuthn registration" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebAuthn login options error");
            return StatusCode(500, new { message = "An error occurred generating WebAuthn login options" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebAuthn login completion error");
            return StatusCode(500, new { message = "An error occurred during WebAuthn login" });
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
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var credentials = await _webAuthnService.GetCredentialsAsync(userId, ct);
            return Ok(credentials);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WebAuthn credentials");
            return StatusCode(500, new { message = "An error occurred retrieving WebAuthn credentials" });
        }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing WebAuthn credential");
            return StatusCode(500, new { message = "An error occurred removing WebAuthn credential" });
        }
    }
}
