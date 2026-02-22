// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CRM.Infrastructure.Services;

/// <summary>
/// OAuth provider for Apple Sign In.
/// Implements privacy-focused authentication with private email relay.
/// </summary>
public class AppleOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly AppleOAuthOptions _options;
    private readonly ILogger<AppleOAuthProvider> _logger;

    private const string AuthorizeUrl = "https://appleid.apple.com/auth/authorize";
    private const string TokenUrl = "https://appleid.apple.com/auth/token";

    public AppleOAuthProvider(
        HttpClient httpClient,
        IOptions<AppleOAuthOptions> options,
        ILogger<AppleOAuthProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get the Apple authorization URL.
    /// </summary>
    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        var scopes = "name%20email";
        return $"{AuthorizeUrl}?response_type=code&response_mode=form_post&client_id={_options.ClientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={scopes}&state={state}";
    }

    /// <summary>
    /// Exchange authorization code for access token using JWT client assertion.
    /// Apple uses JWT for client authentication instead of client_secret.
    /// </summary>
    public async Task<AppleTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate JWT client assertion
            var clientSecret = GenerateClientSecret();

            var parameters = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", _options.ClientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(TokenUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<AppleTokenResponse>(cancellationToken);
            _logger.LogInformation("Successfully exchanged Apple auth code for access token");
            return tokenResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange Apple auth code for token");
            throw;
        }
    }

    /// <summary>
    /// Decode and validate the Apple ID token to get user information.
    /// </summary>
    public AppleUserProfile DecodeIdToken(string idToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            // Note: In production, validate the token signature using Apple's public keys
            var token = handler.ReadJwtToken(idToken);

            var profile = new AppleUserProfile
            {
                User = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value,
                Email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                IsPrivateEmail = bool.TryParse(
                    token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value,
                    out var verified) && verified
            };

            _logger.LogInformation($"Successfully decoded Apple ID token for user {profile.User}");
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decode Apple ID token");
            throw;
        }
    }

    /// <summary>
    /// Get Apple user profile from user object sent in initial request.
    /// Apple sends user info only on first authorization.
    /// </summary>
    public AppleUserProfile ParseUserResponse(string? userJson, string? email = null)
    {
        try
        {
            var profile = new AppleUserProfile();

            if (!string.IsNullOrEmpty(userJson))
            {
                var userObj = System.Text.Json.JsonDocument.Parse(userJson).RootElement;
                profile.User = userObj.GetProperty("sub").GetString();

                var nameObj = userObj.GetProperty("name");
                profile.FirstName = nameObj.GetProperty("firstName").GetString();
                profile.LastName = nameObj.GetProperty("lastName").GetString();
            }

            if (!string.IsNullOrEmpty(email))
            {
                profile.Email = email;
                profile.IsPrivateEmail = email.Contains("@privaterelay.appleid.com");
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Apple user response");
            throw;
        }
    }

    /// <summary>
    /// Generate JWT client secret for Apple authentication.
    /// </summary>
    private string GenerateClientSecret()
    {
        try
        {
            if (string.IsNullOrEmpty(_options.PrivateKey))
                throw new InvalidOperationException("Apple private key is not configured");

            var header = new JwtHeader(
                new SigningCredentials(
                    GetSigningKey(_options.PrivateKey),
                    SecurityAlgorithms.EcdsaSha256));

            var now = DateTimeOffset.UtcNow;
            var payload = new JwtPayload
            {
                { "iss", _options.TeamId },
                { "sub", _options.ClientId },
                { "aud", "https://appleid.apple.com" },
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(5).ToUnixTimeSeconds() }
            };

            var token = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();
            var clientSecret = handler.WriteToken(token);

            _logger.LogDebug("Generated Apple JWT client secret");
            return clientSecret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Apple JWT client secret");
            throw;
        }
    }

    /// <summary>
    /// Convert PEM-formatted private key to signing credentials.
    /// </summary>
    private SecurityKey GetSigningKey(string privateKeyPem)
    {
        try
        {
            // Remove PEM headers
            var pemLines = privateKeyPem
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim();

            var keyBytes = Convert.FromBase64String(pemLines);
            var ecdsa = ECDsa.Create();
            ecdsa.ImportPkcs8PrivateKey(keyBytes, out _);

            var key = new ECDsaSecurityKey(ecdsa) { KeyId = _options.KeyId };
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Apple private key");
            throw;
        }
    }

    /// <summary>
    /// Revoke access token (optional, used for logout).
    /// </summary>
    public async Task<bool> RevokeTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var clientSecret = GenerateClientSecret();

            var parameters = new Dictionary<string, string>
            {
                { "client_id", _options.ClientId },
                { "client_secret", clientSecret },
                { "token", token },
                { "token_type_hint", "access_token" }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(
                "https://appleid.apple.com/auth/revoke",
                content,
                cancellationToken);

            var success = response.IsSuccessStatusCode;
            if (success)
                _logger.LogInformation("Successfully revoked Apple access token");
            else
                _logger.LogWarning($"Failed to revoke Apple token: {response.StatusCode}");

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking Apple token");
            return false;
        }
    }
}

/// <summary>
/// Configuration options for Apple OAuth provider.
/// </summary>
public class AppleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrEmpty(ClientId))
            throw new InvalidOperationException("Apple ClientId is required");
        if (string.IsNullOrEmpty(TeamId))
            throw new InvalidOperationException("Apple TeamId is required");
        if (string.IsNullOrEmpty(KeyId))
            throw new InvalidOperationException("Apple KeyId is required");
        if (string.IsNullOrEmpty(PrivateKey))
            throw new InvalidOperationException("Apple PrivateKey is required");
    }
}

/// <summary>
/// Apple OAuth token response.
/// </summary>
public class AppleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? Access_token { get; set; }

    [JsonPropertyName("expires_in")]
    public int Expires_in { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? Refresh_token { get; set; }

    [JsonPropertyName("id_token")]
    public string? Id_token { get; set; }

    [JsonPropertyName("token_type")]
    public string? Token_type { get; set; }
}

/// <summary>
/// Apple user profile.
/// </summary>
public class AppleUserProfile
{
    /// <summary>
    /// Unique Apple user identifier (sub claim).
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// User email address (may be private relay).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's first name (only provided on initial sign-in).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name (only provided on initial sign-in).
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Indicates if email is a private Apple relay address.
    /// </summary>
    public bool IsPrivateEmail { get; set; }
}
