using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Authentication Controller for user login, registration, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
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
            _logger.LogError($"Registration error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
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
            _logger.LogError($"Login error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Login with OAuth provider
    /// </summary>
    [HttpPost("oauth-login")]
    [AllowAnonymous]
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
    /// Verify JWT token
    /// </summary>
    [HttpPost("verify")]
    [Authorize]
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
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
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
    /// Setup two-factor authentication
    /// </summary>
    [HttpPost("2fa/setup")]
    [Authorize]
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
    /// Verify and enable two-factor authentication
    /// </summary>
    [HttpPost("2fa/verify")]
    [Authorize]
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
    /// Disable two-factor authentication
    /// </summary>
    [HttpPost("2fa/disable")]
    [Authorize]
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
    /// Request password reset
    /// </summary>
    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
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
    /// Confirm password reset
    /// </summary>
    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirm request)
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
    /// Admin reset user password (by user ID)
    /// </summary>
    [HttpPost("reset-password/{userId}")]
    [Authorize]
    public async Task<IActionResult> AdminResetPassword(int userId, [FromBody] AdminPasswordResetRequest request)
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
}
