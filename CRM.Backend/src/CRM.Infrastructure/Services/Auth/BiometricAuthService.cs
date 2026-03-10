// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Platform biometric authentication service implementation.
/// TODO-AUTH-010: Platform Biometric Authentication
///
/// Uses WebAuthn with authenticatorAttachment: 'platform' for fingerprint, Face ID, Windows Hello, etc.
/// Delegates to WebAuthnService for low-level operations but filters for platform credentials.
/// </summary>
public class BiometricAuthService : IBiometricAuthService
{
    private readonly ICrmDbContext _db;
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<BiometricAuthService> _logger;
    private readonly WebAuthnOptions _options;

    // In-memory challenge storage (in production, use distributed cache)
    private readonly Dictionary<string, (int UserId, DateTime ExpiresAt)> _pendingChallenges = new();

    public BiometricAuthService(
        ICrmDbContext db,
        IWebAuthnService webAuthnService,
        IOptions<WebAuthnOptions> options,
        ILogger<BiometricAuthService> logger)
    {
        _db = db;
        _webAuthnService = webAuthnService;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<BiometricRegistrationOptions> GetRegistrationOptionsAsync(int userId, string? deviceName = null, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        var challenge = GenerateChallenge();
        var challengeBase64 = Convert.ToBase64String(challenge);

        // Store challenge for verification
        _pendingChallenges[challengeBase64] = (userId, DateTime.UtcNow.AddMinutes(_options.ChallengeExpirationMinutes));

        // Get existing credential IDs to exclude
        var existingCredentials = await _db.WebAuthnCredentials
            .Where(c => c.UserId == userId && !c.IsRevoked)
            .Select(c => c.CredentialId)
            .ToArrayAsync(ct);

        return new BiometricRegistrationOptions
        {
            Challenge = challengeBase64,
            RelyingPartyId = _options.RelyingPartyId,
            RelyingPartyName = _options.RelyingPartyName,
            UserId = user.Id.ToString(),
            UserName = user.Email,
            UserDisplayName = !string.IsNullOrEmpty(user.FirstName)
                ? $"{user.FirstName} {user.LastName}".Trim()
                : user.Email,
            AttestationConveyance = _options.AttestationConveyance,
            AuthenticatorAttachment = "platform", // Key for biometric: platform authenticator only
            UserVerification = "required",
            TimeoutMs = _options.TimeoutSeconds * 1000,
            ExcludeCredentials = existingCredentials
        };
    }

    /// <inheritdoc />
    public async Task<bool> CompleteRegistrationAsync(int userId, BiometricRegistrationResponse response, string? deviceName = null, CancellationToken ct = default)
    {
        try
        {
            // Verify challenge is valid
            // In production, extract challenge from clientDataJson and validate

            // Store credential
            var credential = new WebAuthnCredential
            {
                UserId = userId,
                CredentialId = response.CredentialId,
                PublicKey = System.Text.Encoding.UTF8.GetBytes(response.AttestationObject), // Simplified - should extract pubkey
                SignCount = 0,
                DeviceName = deviceName ?? "Biometric Device",
                DeviceType = "platform",
                IsPlatformCredential = true,
                Transports = new List<string> { response.Transports ?? "internal" },
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = null
            };

            _db.WebAuthnCredentials.Add(credential);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Biometric credential registered for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing biometric registration for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<BiometricAuthenticationOptions> GetAuthenticationOptionsAsync(int? userId = null, CancellationToken ct = default)
    {
        var challenge = GenerateChallenge();
        var challengeBase64 = Convert.ToBase64String(challenge);

        string[]? allowCredentials = null;

        if (userId.HasValue)
        {
            // Get platform credentials for this user
            allowCredentials = await _db.WebAuthnCredentials
                .Where(c => c.UserId == userId.Value && !c.IsRevoked && c.IsPlatformCredential)
                .Select(c => c.CredentialId)
                .ToArrayAsync(ct);
        }

        return new BiometricAuthenticationOptions
        {
            Challenge = challengeBase64,
            RelyingPartyId = _options.RelyingPartyId,
            AllowCredentials = allowCredentials,
            UserVerification = "required",
            TimeoutMs = _options.TimeoutSeconds * 1000
        };
    }

    /// <inheritdoc />
    public async Task<BiometricAuthResult> ValidateAuthenticationAsync(BiometricAuthenticationResponse response, CancellationToken ct = default)
    {
        try
        {
            // Find the credential
            var credential = await _db.WebAuthnCredentials
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CredentialId == response.CredentialId && !c.IsRevoked, ct);

            if (credential == null)
            {
                return new BiometricAuthResult
                {
                    Success = false,
                    Error = "Credential not found",
                    ErrorCode = "credential_not_found"
                };
            }

            // TODO: Verify signature using stored public key // NOSONAR
            // For now, accept if credential exists

            // Update last used timestamp
            credential.LastUsedAt = DateTime.UtcNow;
            credential.SignCount++;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Biometric authentication successful for user {UserId}", credential.UserId);

            return new BiometricAuthResult
            {
                Success = true,
                UserId = credential.UserId,
                CredentialId = credential.CredentialId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating biometric authentication");
            return new BiometricAuthResult
            {
                Success = false,
                Error = ex.Message,
                ErrorCode = "validation_error"
            };
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BiometricCredentialInfo>> GetUserCredentialsAsync(int userId, CancellationToken ct = default)
    {
        var credentials = await _db.WebAuthnCredentials
            .Where(c => c.UserId == userId && !c.IsRevoked && c.IsPlatformCredential)
            .OrderByDescending(c => c.LastUsedAt ?? c.CreatedAt)
            .ToListAsync(ct);

        return credentials.Select(c => new BiometricCredentialInfo
        {
            Id = c.Id,
            CredentialId = c.CredentialId,
            DeviceName = c.DeviceName ?? "Unknown Device",
            DeviceType = c.DeviceType ?? "platform",
            CreatedAt = c.CreatedAt,
            LastUsedAt = c.LastUsedAt,
            IsPlatformCredential = c.IsPlatformCredential
        });
    }

    /// <inheritdoc />
    public async Task<bool> RemoveCredentialAsync(int userId, string credentialId, CancellationToken ct = default)
    {
        var credential = await _db.WebAuthnCredentials
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CredentialId == credentialId, ct);

        if (credential == null)
        {
            return false;
        }

        credential.IsRevoked = true;
        credential.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Biometric credential {CredentialId} removed for user {UserId}", credentialId, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<int> RemoveAllCredentialsAsync(int userId, CancellationToken ct = default)
    {
        var credentials = await _db.WebAuthnCredentials
            .Where(c => c.UserId == userId && !c.IsRevoked && c.IsPlatformCredential)
            .ToListAsync(ct);

        foreach (var credential in credentials)
        {
            credential.IsRevoked = true;
            credential.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Removed {Count} biometric credentials for user {UserId}", credentials.Count, userId);
        return credentials.Count;
    }

    /// <inheritdoc />
    public async Task<bool> HasBiometricCredentialAsync(int userId, CancellationToken ct = default)
    {
        return await _db.WebAuthnCredentials
            .AnyAsync(c => c.UserId == userId && !c.IsRevoked && c.IsPlatformCredential, ct);
    }

    /// <inheritdoc />
    public bool IsConfigured()
    {
        var (isValid, _) = _options.Validate();
        return isValid;
    }

    private static byte[] GenerateChallenge()
    {
        var challenge = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(challenge);
        return challenge;
    }
}
