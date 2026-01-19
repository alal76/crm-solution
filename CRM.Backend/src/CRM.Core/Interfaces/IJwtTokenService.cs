namespace CRM.Core.Interfaces;

using CRM.Core.Entities;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    int GetUserIdFromToken(string token);
}
