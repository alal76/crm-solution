// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Interfaces;
using CRM.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Generic OpenID Connect service implementation (TODO-AUTH-004).
/// Supports multiple OIDC providers with dynamic configuration.
/// </summary>
public class OpenIdConnectService : IOpenIdConnectService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenIdConnectService> _logger;
    private readonly ConcurrentDictionary<string, OidcDiscoveryDocument> _discoveryCache = new();
    private readonly ConcurrentDictionary<string, OpenIdConnectOptions> _providerOptions = new();

    public OpenIdConnectService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenIdConnectService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        LoadProviderConfigurations();
    }

    private void LoadProviderConfigurations()
    {
        var oidcSection = _configuration.GetSection("OIDC:Providers");
        foreach (var providerSection in oidcSection.GetChildren())
        {
            var options = new OpenIdConnectOptions();
            providerSection.Bind(options);
            options.ProviderName = providerSection.Key;
            _providerOptions[providerSection.Key.ToLowerInvariant()] = options;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetAuthorizationUrlAsync(string providerName, string state, string nonce, string? codeVerifier = null)
    {
        var options = GetProviderOptions(providerName);
        var discovery = await GetDiscoveryDocumentAsync(providerName);

        var authEndpoint = discovery?.AuthorizationEndpoint ?? $"{options.Authority}/authorize";

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = options.ClientId,
            ["response_type"] = options.ResponseType,
            ["scope"] = options.Scopes,
            ["redirect_uri"] = options.RedirectUri,
            ["state"] = state,
            ["nonce"] = nonce
        };

        if (!string.IsNullOrEmpty(options.ResponseMode))
        {
            queryParams["response_mode"] = options.ResponseMode;
        }

        if (options.UsePkce && !string.IsNullOrEmpty(codeVerifier))
        {
            queryParams["code_challenge"] = GenerateCodeChallenge(codeVerifier);
            queryParams["code_challenge_method"] = "S256";
        }

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{authEndpoint}?{queryString}";
    }

    /// <inheritdoc />
    public async Task<OidcAuthResult> ExchangeCodeAsync(string providerName, string code, string? codeVerifier = null, string? expectedNonce = null, CancellationToken ct = default)
    {
        var options = GetProviderOptions(providerName);
        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);

        var tokenEndpoint = discovery?.TokenEndpoint ?? $"{options.Authority}/token";

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = options.RedirectUri,
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret
        };

        if (!string.IsNullOrEmpty(codeVerifier))
        {
            tokenRequest["code_verifier"] = codeVerifier;
        }

        try
        {
            var response = await _httpClient.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(tokenRequest),
                ct);

            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OIDC token exchange failed for {Provider}: {StatusCode} - {Content}",
                    providerName, response.StatusCode, content);
                return new OidcAuthResult
                {
                    Success = false,
                    ProviderName = providerName,
                    Error = "token_exchange_failed",
                    ErrorDescription = content
                };
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var result = new OidcAuthResult
            {
                Success = true,
                ProviderName = providerName,
                AccessToken = tokenResponse.GetProperty("access_token").GetString(),
                IdToken = tokenResponse.TryGetProperty("id_token", out var idToken) ? idToken.GetString() : null,
                RefreshToken = tokenResponse.TryGetProperty("refresh_token", out var refreshToken) ? refreshToken.GetString() : null,
                ExpiresIn = tokenResponse.TryGetProperty("expires_in", out var expiresIn) ? expiresIn.GetInt32() : 3600
            };

            // Get user profile
            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                result.UserProfile = await GetUserProfileAsync(providerName, result.AccessToken, ct);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging OIDC code for {Provider}", providerName);
            return new OidcAuthResult
            {
                Success = false,
                ProviderName = providerName,
                Error = "exception",
                ErrorDescription = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateIdTokenAsync(string providerName, string idToken, string? expectedNonce = null, CancellationToken ct = default)
    {
        // Basic validation - production should use proper JWT validation with JWKS
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/')));
            var claims = JsonSerializer.Deserialize<JsonElement>(payloadJson);

            // Check expiration
            if (claims.TryGetProperty("exp", out var exp))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64());
                if (expTime < DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("ID token expired for {Provider}", providerName);
                    return false;
                }
            }

            // Check nonce if provided
            if (!string.IsNullOrEmpty(expectedNonce) && claims.TryGetProperty("nonce", out var nonce))
            {
                if (nonce.GetString() != expectedNonce)
                {
                    _logger.LogWarning("ID token nonce mismatch for {Provider}", providerName);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ID token for {Provider}", providerName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<OidcUserProfile?> GetUserProfileAsync(string providerName, string accessToken, CancellationToken ct = default)
    {
        var options = GetProviderOptions(providerName);
        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);

        var userInfoEndpoint = discovery?.UserinfoEndpoint ?? $"{options.Authority}/userinfo";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OIDC userinfo request failed for {Provider}: {StatusCode}",
                    providerName, response.StatusCode);
                return null;
            }

            var userInfo = JsonSerializer.Deserialize<JsonElement>(content);
            return new OidcUserProfile
            {
                Sub = userInfo.GetProperty("sub").GetString() ?? string.Empty,
                Email = userInfo.TryGetProperty(options.EmailClaimType, out var email) ? email.GetString() ?? string.Empty : string.Empty,
                Name = userInfo.TryGetProperty(options.NameClaimType, out var name) ? name.GetString() : null,
                GivenName = userInfo.TryGetProperty("given_name", out var givenName) ? givenName.GetString() : null,
                FamilyName = userInfo.TryGetProperty("family_name", out var familyName) ? familyName.GetString() : null,
                PreferredUsername = userInfo.TryGetProperty("preferred_username", out var username) ? username.GetString() : null,
                Picture = userInfo.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
                EmailVerified = userInfo.TryGetProperty("email_verified", out var emailVerified) && emailVerified.GetBoolean()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile from {Provider}", providerName);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<OidcAuthResult> RefreshTokenAsync(string providerName, string refreshToken, CancellationToken ct = default)
    {
        var options = GetProviderOptions(providerName);
        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);

        var tokenEndpoint = discovery?.TokenEndpoint ?? $"{options.Authority}/token";

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret
        };

        try
        {
            var response = await _httpClient.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(tokenRequest),
                ct);

            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return new OidcAuthResult
                {
                    Success = false,
                    ProviderName = providerName,
                    Error = "refresh_failed",
                    ErrorDescription = content
                };
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
            return new OidcAuthResult
            {
                Success = true,
                ProviderName = providerName,
                AccessToken = tokenResponse.GetProperty("access_token").GetString(),
                IdToken = tokenResponse.TryGetProperty("id_token", out var idToken) ? idToken.GetString() : null,
                RefreshToken = tokenResponse.TryGetProperty("refresh_token", out var newRefresh) ? newRefresh.GetString() : refreshToken,
                ExpiresIn = tokenResponse.TryGetProperty("expires_in", out var expiresIn) ? expiresIn.GetInt32() : 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for {Provider}", providerName);
            return new OidcAuthResult
            {
                Success = false,
                ProviderName = providerName,
                Error = "exception",
                ErrorDescription = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task RevokeTokenAsync(string providerName, string token, string tokenTypeHint = "access_token", CancellationToken ct = default)
    {
        var options = GetProviderOptions(providerName);
        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);

        var revokeEndpoint = discovery?.RevocationEndpoint;
        if (string.IsNullOrEmpty(revokeEndpoint))
        {
            _logger.LogDebug("Revocation endpoint not available for {Provider}", providerName);
            return;
        }

        var revokeRequest = new Dictionary<string, string>
        {
            ["token"] = token,
            ["token_type_hint"] = tokenTypeHint,
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret
        };

        try
        {
            await _httpClient.PostAsync(revokeEndpoint, new FormUrlEncodedContent(revokeRequest), ct);
            _logger.LogDebug("Token revoked for {Provider}", providerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error revoking token for {Provider} (non-fatal)", providerName);
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetLogoutUrlAsync(string providerName, string idToken, string? postLogoutRedirectUri = null)
    {
        var discovery = await GetDiscoveryDocumentAsync(providerName);
        var logoutEndpoint = discovery?.EndSessionEndpoint;

        if (string.IsNullOrEmpty(logoutEndpoint))
        {
            return null;
        }

        var queryParams = new Dictionary<string, string>
        {
            ["id_token_hint"] = idToken
        };

        if (!string.IsNullOrEmpty(postLogoutRedirectUri))
        {
            queryParams["post_logout_redirect_uri"] = postLogoutRedirectUri;
        }

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{logoutEndpoint}?{queryString}";
    }

    /// <inheritdoc />
    public async Task<OidcDiscoveryDocument?> GetDiscoveryDocumentAsync(string providerName, CancellationToken ct = default)
    {
        var key = providerName.ToLowerInvariant();

        if (_discoveryCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var options = GetProviderOptions(providerName);
        var metadataUrl = !string.IsNullOrEmpty(options.MetadataAddress)
            ? options.MetadataAddress
            : $"{options.Authority}/.well-known/openid-configuration";

        try
        {
            var response = await _httpClient.GetAsync(metadataUrl, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch OIDC discovery document for {Provider}", providerName);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonSerializer.Deserialize<JsonElement>(content);

            var discovery = new OidcDiscoveryDocument
            {
                Issuer = doc.GetProperty("issuer").GetString() ?? string.Empty,
                AuthorizationEndpoint = doc.GetProperty("authorization_endpoint").GetString() ?? string.Empty,
                TokenEndpoint = doc.GetProperty("token_endpoint").GetString() ?? string.Empty,
                UserinfoEndpoint = doc.TryGetProperty("userinfo_endpoint", out var userInfo) ? userInfo.GetString() ?? string.Empty : string.Empty,
                JwksUri = doc.GetProperty("jwks_uri").GetString() ?? string.Empty,
                EndSessionEndpoint = doc.TryGetProperty("end_session_endpoint", out var endSession) ? endSession.GetString() : null,
                RevocationEndpoint = doc.TryGetProperty("revocation_endpoint", out var revocation) ? revocation.GetString() : null
            };

            _discoveryCache[key] = discovery;
            return discovery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching OIDC discovery document for {Provider}", providerName);
            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetConfiguredProviders()
    {
        return _providerOptions.Keys;
    }

    /// <inheritdoc />
    public bool IsProviderConfigured(string providerName)
    {
        return _providerOptions.ContainsKey(providerName.ToLowerInvariant());
    }

    /// <inheritdoc />
    public string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <inheritdoc />
    public string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private OpenIdConnectOptions GetProviderOptions(string providerName)
    {
        if (_providerOptions.TryGetValue(providerName.ToLowerInvariant(), out var options))
        {
            return options;
        }

        throw new InvalidOperationException($"OIDC provider '{providerName}' is not configured.");
    }
}
