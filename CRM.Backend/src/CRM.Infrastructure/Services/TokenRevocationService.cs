// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using System.Security.Cryptography;
using System.Text;

using CRM.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Token revocation service using Redis/distributed cache for storing blacklisted tokens.
/// </summary>
public class TokenRevocationService : ITokenRevocationService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<TokenRevocationService> _logger;
    private readonly int _defaultExpirationMinutes;

    private const string TOKENPREFIX = "revoked_token:";
    private const string USERREVOCATIONPREFIX = "user_revoked_at:";

    public TokenRevocationService(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<TokenRevocationService> logger)
    {
        _cache = cache;
        _logger = logger;
        _defaultExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
    }

    /// <inheritdoc/>
    public async Task RevokeTokenAsync(string token, int? expirationMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        try
        {
            // Hash the token to use as a key (tokens can be very long)
            var tokenHash = HashToken(token);
            var key = $"{TOKENPREFIX}{tokenHash}";

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes ?? _defaultExpirationMinutes),
            };

            await _cache.SetStringAsync(key, "revoked", options);
            _logger.LogInformation("Token revoked successfully. Hash: {TokenHash}", tokenHash[..16]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke token");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RevokeAllUserTokensAsync(int userId)
    {
        try
        {
            var key = $"{USERREVOCATIONPREFIX}{userId}";
            var revocationTime = DateTime.UtcNow.ToString("O");

            // Store the revocation timestamp - all tokens issued before this time are invalid
            var options = new DistributedCacheEntryOptions
            {
                // Keep user revocation for 24 hours (longer than any token lifetime)
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
            };

            await _cache.SetStringAsync(key, revocationTime, options);
            _logger.LogInformation("All tokens revoked for user {UserId} at {RevocationTime}", userId, revocationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke all tokens for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return true;
        }

        try
        {
            var tokenHash = HashToken(token);
            var key = $"{TOKENPREFIX}{tokenHash}";

            var result = await _cache.GetStringAsync(key);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check token revocation status");

            // On error, assume token is valid to prevent lockouts
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserTokenRevokedAsync(int userId, DateTime tokenIssuedAt)
    {
        try
        {
            var key = $"{USERREVOCATIONPREFIX}{userId}";
            var revocationTimeStr = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(revocationTimeStr))
            {
                return false;
            }

            if (DateTime.TryParse(revocationTimeStr, out var revocationTime))
            {
                // Token is revoked if it was issued before the revocation time
                return tokenIssuedAt < revocationTime;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check user token revocation for user {UserId}", userId);

            // On error, assume token is valid to prevent lockouts
            return false;
        }
    }

    /// <summary>
    /// Hash the token using SHA256 to create a shorter, consistent key.
    /// </summary>
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
