using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Login user with email and password
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Login with social provider token
    /// </summary>
    Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Verify JWT token
    /// </summary>
    Task<bool> VerifyTokenAsync(string token);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<User> UpdateUserAsync(int userId, User user);

    /// <summary>
    /// Setup two-factor authentication
    /// </summary>
    Task<TwoFactorSetupResponse> SetupTwoFactorAsync(int userId);

    /// <summary>
    /// Verify two-factor authentication code
    /// </summary>
    Task<bool> VerifyTwoFactorCodeAsync(int userId, string code);

    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    Task EnableTwoFactorAsync(int userId, string secret, List<string> backupCodes);

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    Task DisableTwoFactorAsync(int userId);

    /// <summary>
    /// Request password reset token
    /// </summary>
    Task<string> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Reset password with token
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
