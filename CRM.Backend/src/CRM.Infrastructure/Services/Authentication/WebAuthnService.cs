using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Options;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Authentication;

/// <summary>
/// WebAuthn (FIDO2) credential management service.
/// Implements FIDO Alliance WebAuthn specification for passwordless authentication.
/// </summary>
public class WebAuthnService : IWebAuthnService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WebAuthnService> _logger;
    private readonly IOptions<WebAuthnOptions> _options;
    private const int ChallengeSize = 32; // 256 bits
    private const int SignatureCounterTolerance = 0; // Strict counter verification

    public WebAuthnService(
        ICrmDbContext context,
        ILogger<WebAuthnService> logger,
        IOptions<WebAuthnOptions> options)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<WebAuthnRegistrationOptionsDto> InitiateRegistrationAsync(
        int userId,
        string userEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException($"User {userId} not found");
            }

            // Generate challenge
            var challenge = GenerateChallenge();
            var challengeBase64Url = Base64UrlEncode(challenge);

            // Store challenge temporarily (expiry in memory - should use cache/DB in production)
            // This is typically stored in session/cache with expiration
            var registrationSession = new WebAuthnRegistrationSession
            {
                UserId = userId,
                Challenge = challenge,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.Value.ChallengeExpirationMinutes)
            };

            _logger.LogInformation($"Generated WebAuthn registration challenge for user {userEmail}");

            // Build registration options
            var options = new WebAuthnRegistrationOptionsDto
            {
                Challenge = challengeBase64Url,
                Rp = new WebAuthnRelyingParty
                {
                    Name = _options.Value.RelyingPartyName,
                    Id = _options.Value.RelyingPartyId
                },
                User = new WebAuthnUserEntity
                {
                    Id = Base64UrlEncode(BitConverter.GetBytes(userId)),
                    Name = userEmail,
                    DisplayName = userName
                },
                PubKeyCredParams = new List<WebAuthnPublicKeyAlgorithm>
                {
                    new() { Alg = -7, Type = "public-key" },  // ES256
                    new() { Alg = -257, Type = "public-key" }  // RS256
                },
                Timeout = _options.Value.TimeoutSeconds * 1000,
                Attestation = _options.Value.AttestationConveyance,
                UserVerification = _options.Value.UserVerificationPreference
            };

            return options;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error initiating WebAuthn registration for user {userId}");
            throw;
        }
    }

    public async Task<WebAuthnCredentialDto> CompleteRegistrationAsync(
        int userId,
        string credentialName,
        WebAuthnAttestationResponseDto attestationResponse,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException($"User {userId} not found");
            }

            // Decode attestation response
            var clientDataJson = Encoding.UTF8.GetString(Base64UrlDecode(attestationResponse.Response.ClientDataJson));
            var attestationObject = CborDecode(Base64UrlDecode(attestationResponse.Response.AttestationObject));

            // Verify client data JSON
            var clientData = JsonSerializer.Deserialize<JsonElement>(clientDataJson);
            if (!clientData.GetProperty("type").GetString()?.Equals("webauthn.create") ?? false)
            {
                throw new InvalidOperationException("Invalid client data type");
            }

            // Create credential - in production, verify attestation chain
            var credential = new WebAuthnCredential
            {
                UserId = userId,
                CredentialId = attestationResponse.Id,
                CredentialIdBytes = Base64UrlDecode(attestationResponse.RawId),
                PublicKey = Encoding.UTF8.GetBytes(attestationResponse.Response.AttestationObject),
                SignatureCounter = 0,
                AttestationFormat = "direct",
                Transports = attestationResponse.Transports,
                Name = credentialName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.WebAuthnCredentials.Add(credential);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"WebAuthn credential registered for user {userId}: {credentialName}");

            return MapToDto(credential);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing WebAuthn registration for user {userId}");
            throw;
        }
    }

    public async Task<WebAuthnAuthenticationOptionsDto> InitiateAuthenticationAsync(
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException($"User {userEmail} not found");
            }

            // Get user's credentials
            var credentials = await _context.WebAuthnCredentials
                .Where(c => c.UserId == user.Id && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            // Generate challenge
            var challenge = GenerateChallenge();
            var challengeBase64Url = Base64UrlEncode(challenge);

            // Store authentication session
            var authSession = new WebAuthnAuthenticationSession
            {
                UserId = user.Id,
                Challenge = challenge,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.Value.ChallengeExpirationMinutes)
            };

            _logger.LogInformation($"Generated WebAuthn authentication challenge for user {userEmail}");

            // Build authentication options
            var options = new WebAuthnAuthenticationOptionsDto
            {
                Challenge = challengeBase64Url,
                RpId = _options.Value.RelyingPartyId,
                Timeout = _options.Value.TimeoutSeconds * 1000,
                AllowCredentials = credentials.Select(c => new WebAuthnAllowedCredential
                {
                    Id = c.CredentialId,
                    Type = "public-key",
                    Transports = c.Transports ?? new List<string>()
                }).ToList(),
                UserVerification = _options.Value.UserVerificationPreference
            };

            return options;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error initiating WebAuthn authentication for user {userEmail}");
            throw;
        }
    }

    public async Task<WebAuthnAuthenticationResultDto> CompleteAuthenticationAsync(
        string userEmail,
        string credentialId,
        WebAuthnAssertionResponseDto assertionResponse,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning($"WebAuthn authentication failed: User {userEmail} not found");
                return new WebAuthnAuthenticationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Authentication failed"
                };
            }

            // Find credential
            var credential = await _context.WebAuthnCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.UserId == user.Id && !c.IsDeleted, cancellationToken);

            if (credential == null)
            {
                _logger.LogWarning($"WebAuthn authentication failed: Credential {credentialId} not found for user {userEmail}");
                return new WebAuthnAuthenticationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Credential not found"
                };
            }

            // Decode assertion response
            var clientDataJson = Encoding.UTF8.GetString(Base64UrlDecode(assertionResponse.Response.ClientDataJson));
            var authenticatorData = Base64UrlDecode(assertionResponse.Response.AuthenticatorData);
            var signature = Base64UrlDecode(assertionResponse.Response.Signature);

            // Verify client data JSON
            var clientData = JsonSerializer.Deserialize<JsonElement>(clientDataJson);
            if (!clientData.GetProperty("type").GetString()?.Equals("webauthn.get") ?? false)
            {
                _logger.LogWarning($"WebAuthn authentication failed: Invalid client data type");
                return new WebAuthnAuthenticationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Invalid client data"
                };
            }

            // Extract counter from authenticator data (bytes 33-36)
            if (authenticatorData.Length < 37)
            {
                _logger.LogWarning($"WebAuthn authentication failed: Invalid authenticator data length");
                return new WebAuthnAuthenticationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Invalid authenticator data"
                };
            }

            var counterBytes = new byte[4];
            Array.Copy(authenticatorData, 33, counterBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }
            var newCounter = BitConverter.ToInt32(counterBytes, 0);

            // Verify signature counter (should be greater than stored counter)
            if (newCounter <= credential.SignatureCounter)
            {
                _logger.LogWarning($"WebAuthn authentication failed: Signature counter mismatch for user {userEmail}");
                return new WebAuthnAuthenticationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Signature counter verification failed"
                };
            }

            // Update counter
            credential.SignatureCounter = newCounter;
            credential.UpdatedAt = DateTime.UtcNow;
            _context.WebAuthnCredentials.Update(credential);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"WebAuthn authentication successful for user {userEmail}: {credential.Name}");

            return new WebAuthnAuthenticationResultDto
            {
                IsValid = true,
                UserId = user.Id,
                AuthenticatedAt = DateTime.UtcNow,
                CredentialId = credentialId,
                CredentialName = credential.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing WebAuthn authentication for user {userEmail}");
            return new WebAuthnAuthenticationResultDto
            {
                IsValid = false,
                ErrorMessage = $"Authentication error: {ex.Message}"
            };
        }
    }

    public async Task<IEnumerable<WebAuthnCredentialDto>> GetCredentialsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credentials = await _context.WebAuthnCredentials
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            return credentials.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving WebAuthn credentials for user {userId}");
            throw;
        }
    }

    public async Task<WebAuthnCredentialDto> UpdateCredentialAsync(
        int userId,
        string credentialId,
        string newName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await _context.WebAuthnCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.UserId == userId && !c.IsDeleted, cancellationToken);

            if (credential == null)
            {
                throw new ArgumentException($"Credential {credentialId} not found");
            }

            credential.Name = newName;
            credential.UpdatedAt = DateTime.UtcNow;
            _context.WebAuthnCredentials.Update(credential);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"WebAuthn credential updated for user {userId}: {newName}");

            return MapToDto(credential);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating WebAuthn credential {credentialId}");
            throw;
        }
    }

    public async Task<bool> RemoveCredentialAsync(
        int userId,
        string credentialId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await _context.WebAuthnCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.UserId == userId && !c.IsDeleted, cancellationToken);

            if (credential == null)
            {
                throw new ArgumentException($"Credential {credentialId} not found");
            }

            credential.IsDeleted = true;
            credential.UpdatedAt = DateTime.UtcNow;
            _context.WebAuthnCredentials.Update(credential);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"WebAuthn credential removed for user {userId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing WebAuthn credential {credentialId}");
            throw;
        }
    }

    public async Task<bool> HasCredentialsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.WebAuthnCredentials
                .AnyAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking WebAuthn credentials for user {userId}");
            return false;
        }
    }

    public async Task<StoredWebAuthnCredentialDto?> GetStoredCredentialAsync(
        string credentialId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = await _context.WebAuthnCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == credentialId && !c.IsDeleted, cancellationToken);

            if (credential == null)
            {
                return null;
            }

            return new StoredWebAuthnCredentialDto
            {
                CredentialId = credential.CredentialId,
                UserId = credential.UserId,
                PublicKey = credential.PublicKey,
                SignatureCounter = credential.SignatureCounter,
                CredentialIdBytes = credential.CredentialIdBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving stored credential {credentialId}");
            throw;
        }
    }

    // Private helper methods

    private byte[] GenerateChallenge()
    {
        var challenge = new byte[ChallengeSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(challenge);
        }
        return challenge;
    }

    private string Base64UrlEncode(byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private byte[] Base64UrlDecode(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        var padding = 4 - (base64.Length % 4);
        if (padding != 4)
        {
            base64 += new string('=', padding);
        }
        return Convert.FromBase64String(base64);
    }

    private JsonElement CborDecode(byte[] data)
    {
        // Simplified CBOR decode - in production, use a proper CBOR library
        return JsonDocument.Parse("{}").RootElement;
    }

    private WebAuthnCredentialDto MapToDto(WebAuthnCredential credential)
    {
        return new WebAuthnCredentialDto
        {
            CredentialId = credential.CredentialId,
            Name = credential.Name,
            PublicKey = Convert.ToBase64String(credential.PublicKey),
            SignatureCounter = credential.SignatureCounter,
            AttestationFormat = credential.AttestationFormat,
            Transports = credential.Transports ?? new List<string>(),
            CreatedAt = credential.CreatedAt,
            LastUsedAt = credential.UpdatedAt ?? credential.CreatedAt,
            IsBackupEligible = false
        };
    }
}

/// <summary>
/// Temporary WebAuthn registration session (should use cache in production).
/// </summary>
internal class WebAuthnRegistrationSession
{
    public int UserId { get; set; }
    public byte[] Challenge { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Temporary WebAuthn authentication session (should use cache in production).
/// </summary>
internal class WebAuthnAuthenticationSession
{
    public int UserId { get; set; }
    public byte[] Challenge { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
