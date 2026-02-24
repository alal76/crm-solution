// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Service for OAuth Device Authorization Grant (RFC 8628) (TODO-AUTH-023).
/// Enables authentication on devices with limited input capabilities.
/// </summary>
public class DeviceAuthorizationService : IDeviceAuthorizationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<DeviceAuthorizationService> _logger;
    private readonly string _verificationUri;
    private readonly int _codeExpirationMinutes;
    private readonly int _pollIntervalSeconds;

    public DeviceAuthorizationService(
        ICrmDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<DeviceAuthorizationService> logger)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _verificationUri = configuration.GetValue<string>("DeviceAuth:VerificationUri") ?? "https://crm.example.com/device";
        _codeExpirationMinutes = configuration.GetValue("DeviceAuth:CodeExpirationMinutes", 15);
        _pollIntervalSeconds = configuration.GetValue("DeviceAuth:PollIntervalSeconds", 5);
    }

    /// <inheritdoc />
    public async Task<DeviceAuthorizationResponse> InitiateDeviceAuthorizationAsync(
        string clientId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        // Generate device code (URL-safe, longer for security)
        var deviceCode = GenerateSecureCode(32);

        // Generate user code (short, easy to type)
        var userCode = GenerateUserCode();

        var deviceAuth = new DeviceAuthorizationCode
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            ClientId = clientId,
            Scope = scope,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_codeExpirationMinutes),
            Interval = _pollIntervalSeconds,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DeviceAuthorizationCodes.Add(deviceAuth);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device authorization initiated for client {ClientId}, user code {UserCode}",
            clientId, userCode);

        return new DeviceAuthorizationResponse
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            VerificationUri = _verificationUri,
            VerificationUriComplete = $"{_verificationUri}?user_code={userCode}",
            ExpiresIn = _codeExpirationMinutes * 60,
            Interval = _pollIntervalSeconds
        };
    }

    /// <inheritdoc />
    public async Task<DeviceTokenResponse> PollForTokenAsync(
        string deviceCode,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var deviceAuth = await _dbContext.DeviceAuthorizationCodes
            .FirstOrDefaultAsync(d =>
                d.DeviceCode == deviceCode &&
                d.ClientId == clientId,
                cancellationToken);

        if (deviceAuth == null)
        {
            return new DeviceTokenResponse
            {
                Success = false,
                Error = DeviceTokenResponse.ErrorCodes.ExpiredToken,
                ErrorDescription = "Device code not found or expired"
            };
        }

        // Check if expired
        if (deviceAuth.ExpiresAt < DateTime.UtcNow)
        {
            return new DeviceTokenResponse
            {
                Success = false,
                Error = DeviceTokenResponse.ErrorCodes.ExpiredToken,
                ErrorDescription = "The device code has expired"
            };
        }

        // Check if already used
        if (deviceAuth.IsUsed)
        {
            return new DeviceTokenResponse
            {
                Success = false,
                Error = DeviceTokenResponse.ErrorCodes.ExpiredToken,
                ErrorDescription = "The device code has already been used"
            };
        }

        // Check if denied
        if (deviceAuth.IsDenied)
        {
            return new DeviceTokenResponse
            {
                Success = false,
                Error = DeviceTokenResponse.ErrorCodes.AccessDenied,
                ErrorDescription = "The user denied the authorization request"
            };
        }

        // Check if authorized
        if (!deviceAuth.IsAuthorized || !deviceAuth.AuthorizedUserId.HasValue)
        {
            return new DeviceTokenResponse
            {
                Success = false,
                Error = DeviceTokenResponse.ErrorCodes.AuthorizationPending,
                ErrorDescription = "The authorization request is still pending"
            };
        }

        // Mark as used
        deviceAuth.IsUsed = true;
        deviceAuth.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var user = await _dbContext.Users.FindAsync(new object[] { deviceAuth.AuthorizedUserId.Value }, cancellationToken);
        if (user == null)
        {
            return new DeviceTokenResponse
            {
                Success = false,
                Error = DeviceTokenResponse.ErrorCodes.AccessDenied,
                ErrorDescription = "User not found"
            };
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("Device authorization completed for user {UserId}, client {ClientId}",
            user.Id, clientId);

        return new DeviceTokenResponse
        {
            Success = true,
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 3600, // 1 hour
            RefreshToken = refreshToken,
            Scope = deviceAuth.Scope
        };
    }

    /// <inheritdoc />
    public async Task<DeviceAuthorizationCode?> GetPendingAuthorizationAsync(
        string userCode,
        CancellationToken cancellationToken = default)
    {
        // Normalize user code (remove dashes, uppercase)
        var normalizedCode = userCode.ToUpperInvariant().Replace("-", "").Replace(" ", "");

        return await _dbContext.DeviceAuthorizationCodes
            .FirstOrDefaultAsync(d =>
                d.UserCode.Replace("-", "") == normalizedCode &&
                !d.IsAuthorized &&
                !d.IsDenied &&
                !d.IsUsed &&
                d.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AuthorizeDeviceAsync(
        string userCode,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var deviceAuth = await GetPendingAuthorizationAsync(userCode, cancellationToken);
        if (deviceAuth == null)
        {
            _logger.LogWarning("Device authorization failed: code not found or expired");
            return false;
        }

        deviceAuth.IsAuthorized = true;
        deviceAuth.AuthorizedUserId = userId;
        deviceAuth.AuthorizedAt = DateTime.UtcNow;
        deviceAuth.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device authorized by user {UserId}, code {UserCode}",
            userId, userCode[..Math.Min(4, userCode.Length)] + "****");

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DenyDeviceAsync(
        string userCode,
        CancellationToken cancellationToken = default)
    {
        var deviceAuth = await GetPendingAuthorizationAsync(userCode, cancellationToken);
        if (deviceAuth == null)
        {
            return false;
        }

        deviceAuth.IsDenied = true;
        deviceAuth.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device authorization denied for code {UserCode}",
            userCode[..Math.Min(4, userCode.Length)] + "****");

        return true;
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredCodesAsync(CancellationToken cancellationToken = default)
    {
        var expired = await _dbContext.DeviceAuthorizationCodes
            .Where(d => !d.IsDeleted && d.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var code in expired)
        {
            code.IsDeleted = true;
            code.UpdatedAt = DateTime.UtcNow;
        }

        if (expired.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} expired device authorization codes", expired.Count);
        }

        return expired.Count;
    }

    /// <summary>
    /// Generates a cryptographically secure random code.
    /// </summary>
    private static string GenerateSecureCode(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    /// <summary>
    /// Generates a user-friendly code (e.g., "ABCD-1234").
    /// </summary>
    private static string GenerateUserCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude confusing chars like 0, O, 1, I
        var bytes = RandomNumberGenerator.GetBytes(8);

        var code = new char[8];
        for (int i = 0; i < 8; i++)
        {
            code[i] = chars[bytes[i] % chars.Length];
        }

        // Format as XXXX-XXXX
        return $"{new string(code[..4])}-{new string(code[4..])}";
    }
}
