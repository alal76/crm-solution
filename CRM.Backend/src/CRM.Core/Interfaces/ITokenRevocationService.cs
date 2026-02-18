// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for token revocation service - manages blacklisted/revoked JWT tokens.
/// </summary>
public interface ITokenRevocationService
{
    /// <summary>
    /// Revoke a JWT token (add to blacklist).
    /// </summary>
    /// <param name="token">The JWT token to revoke.</param>
    /// <param name="expirationMinutes">How long to keep in blacklist (should match token lifetime).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RevokeTokenAsync(string token, int? expirationMinutes = null);

    /// <summary>
    /// Revoke all tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be revoked.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RevokeAllUserTokensAsync(int userId);

    /// <summary>
    /// Check if a token has been revoked.
    /// </summary>
    /// <param name="token">The JWT token to check.</param>
    /// <returns>True if the token is revoked, false otherwise.</returns>
    Task<bool> IsTokenRevokedAsync(string token);

    /// <summary>
    /// Check if all tokens for a user have been revoked (logout all sessions).
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="tokenIssuedAt">When the token was issued.</param>
    /// <returns>True if the token was issued before the revocation timestamp.</returns>
    Task<bool> IsUserTokenRevokedAsync(int userId, DateTime tokenIssuedAt);
}
