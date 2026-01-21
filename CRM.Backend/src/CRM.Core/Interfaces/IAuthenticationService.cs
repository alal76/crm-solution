// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
}
