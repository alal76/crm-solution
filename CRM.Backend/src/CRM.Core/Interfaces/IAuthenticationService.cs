// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
    /// Generate tokens for a user by ID (used after alternative auth methods like biometric/WebAuthn)
    /// </summary>
    Task<AuthResponse?> GenerateTokensForUserAsync(int userId);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Refresh access token with existing refresh token
    /// </summary>
    Task<AuthResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout user and invalidate session
    /// </summary>
    Task<bool> LogoutAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<AuthResponse> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);

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
    /// Verify two-factor authentication during login
    /// </summary>
    Task<AuthResponse> VerifyTwoFactorLoginAsync(string tempToken, string code);

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

    /// <summary>
    /// Admin reset user password by user ID
    /// </summary>
    Task<bool> AdminResetPasswordAsync(int userId, string newPassword);

    /// <summary>
    /// Set password for first-time login or expired password
    /// </summary>
    Task<AuthResponse> SetupPasswordAsync(SetPasswordRequest request);

    /// <summary>
    /// Get password complexity requirements from system settings
    /// </summary>
    Task<PasswordComplexityRequirements> GetPasswordRequirementsAsync();
}
