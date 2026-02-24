// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Generic OpenID Connect provider service implementation (TODO-AUTH-004).
/// Supports multiple OIDC providers via configuration.
/// </summary>
public class OidcProviderService : IOidcProviderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OidcProviderService> _logger;
    private readonly ConcurrentDictionary<string, OidcProviderConfig> _providers = new();
    private readonly ConcurrentDictionary<string, OidcDiscoveryDocument> _discoveryCache = new();

    public OidcProviderService(
        HttpClient httpClient,
        ILogger<OidcProviderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IEnumerable<OidcProviderInfo>> GetProvidersAsync(CancellationToken cancellationToken = default)
    {
        var providers = _providers.Values.Select(p => new OidcProviderInfo
        {
            ProviderId = p.ProviderId,
            DisplayName = p.DisplayName,
            LogoUrl = p.LogoUrl,
            IsEnabled = p.IsEnabled
        });

        return Task.FromResult(providers);
    }

    /// <inheritdoc />
    public Task<OidcProviderConfig?> GetProviderAsync(string providerId, CancellationToken cancellationToken = default)
    {
        _providers.TryGetValue(providerId, out var config);
        return Task.FromResult(config);
    }

    /// <inheritdoc />
    public async Task<OidcDiscoveryDocument?> DiscoverEndpointsAsync(
        string discoveryUrl,
        CancellationToken cancellationToken = default)
    {
        if (_discoveryCache.TryGetValue(discoveryUrl, out var cached))
            return cached;

        try
        {
            var response = await _httpClient.GetAsync(discoveryUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to discover OIDC endpoints from {Url}: {Status}", discoveryUrl, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var discovery = JsonSerializer.Deserialize<OidcDiscoveryResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var document = new OidcDiscoveryDocument
            {
                Issuer = discovery?.Issuer ?? string.Empty,
                AuthorizationEndpoint = discovery?.AuthorizationEndpoint ?? string.Empty,
                TokenEndpoint = discovery?.TokenEndpoint ?? string.Empty,
                UserinfoEndpoint = discovery?.UserinfoEndpoint ?? string.Empty,
                JwksUri = discovery?.JwksUri ?? string.Empty,
                EndSessionEndpoint = discovery?.EndSessionEndpoint,
                RevocationEndpoint = discovery?.RevocationEndpoint,
                ScopesSupported = discovery?.ScopesSupported ?? Array.Empty<string>(),
                ResponseTypesSupported = discovery?.ResponseTypesSupported ?? Array.Empty<string>(),
                ClaimsSupported = discovery?.ClaimsSupported ?? Array.Empty<string>()
            };

            _discoveryCache[discoveryUrl] = document;
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering OIDC endpoints from {Url}", discoveryUrl);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetAuthorizationUrlAsync(
        string providerId,
        string state,
        string codeChallenge,
        string nonce,
        CancellationToken cancellationToken = default)
    {
        var config = await GetProviderAsync(providerId, cancellationToken)
            ?? throw new ArgumentException($"Provider '{providerId}' not found");

        var discovery = await DiscoverEndpointsAsync(config.DiscoveryUrl, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to discover endpoints for provider '{providerId}'");

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = config.ClientId,
            ["response_type"] = "code",
            ["scope"] = config.Scopes,
            ["redirect_uri"] = config.RedirectUri,
            ["state"] = state,
            ["nonce"] = nonce,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return $"{discovery.AuthorizationEndpoint}?{queryString}";
    }

    /// <inheritdoc />
    public async Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(
        string providerId,
        string code,
        string codeVerifier,
        CancellationToken cancellationToken = default)
    {
        var config = await GetProviderAsync(providerId, cancellationToken)
            ?? throw new ArgumentException($"Provider '{providerId}' not found");

        var discovery = await DiscoverEndpointsAsync(config.DiscoveryUrl, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to discover endpoints for provider '{providerId}'");

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["code"] = code,
            ["code_verifier"] = codeVerifier,
            ["redirect_uri"] = config.RedirectUri
        });

        var response = await _httpClient.PostAsync(discovery.TokenEndpoint, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Token exchange failed for provider {ProviderId}: {Response}", providerId, responseContent);
            throw new InvalidOperationException($"Token exchange failed: {response.StatusCode}");
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return new OAuthTokenResponseDto
        {
            AccessToken = tokenResponse?.AccessToken ?? string.Empty,
            RefreshToken = tokenResponse?.RefreshToken,
            IdToken = tokenResponse?.IdToken,
            ExpiresIn = tokenResponse?.ExpiresIn ?? 3600,
            TokenType = tokenResponse?.TokenType ?? "Bearer"
        };
    }

    /// <inheritdoc />
    public async Task<OAuthUserInfoDto> GetUserInfoAsync(
        string providerId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var config = await GetProviderAsync(providerId, cancellationToken)
            ?? throw new ArgumentException($"Provider '{providerId}' not found");

        var discovery = await DiscoverEndpointsAsync(config.DiscoveryUrl, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to discover endpoints for provider '{providerId}'");

        var request = new HttpRequestMessage(HttpMethod.Get, discovery.UserinfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get user info for provider {ProviderId}: {Response}", providerId, content);
            throw new InvalidOperationException($"Failed to get user info: {response.StatusCode}");
        }

        var userInfo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

        return new OAuthUserInfoDto
        {
            Id = GetStringClaim(userInfo, "sub") ?? string.Empty,
            Email = GetStringClaim(userInfo, "email") ?? string.Empty,
            Name = GetStringClaim(userInfo, "name") ?? string.Empty,
            GivenName = GetStringClaim(userInfo, "given_name"),
            FamilyName = GetStringClaim(userInfo, "family_name"),
            Picture = GetStringClaim(userInfo, "picture"),
            EmailVerified = GetBoolClaim(userInfo, "email_verified"),
            Provider = providerId
        };
    }

    /// <inheritdoc />
    public async Task<OidcTokenValidationResult> ValidateIdTokenAsync(
        string providerId,
        string idToken,
        string? nonce = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
                return new OidcTokenValidationResult { IsValid = false, Error = "Invalid token format" };

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var decodedPayload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(paddedPayload));
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(decodedPayload);

            // Validate nonce if provided
            if (!string.IsNullOrEmpty(nonce))
            {
                var tokenNonce = GetStringClaim(claims, "nonce");
                if (tokenNonce != nonce)
                    return new OidcTokenValidationResult { IsValid = false, Error = "Nonce mismatch" };
            }

            // Validate expiration
            if (claims?.TryGetValue("exp", out var exp) == true)
            {
                var expTime = exp.GetInt64();
                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expTime).UtcDateTime;
                if (expDateTime < DateTime.UtcNow)
                    return new OidcTokenValidationResult { IsValid = false, Error = "Token expired" };
            }

            return new OidcTokenValidationResult
            {
                IsValid = true,
                Subject = GetStringClaim(claims, "sub"),
                Email = GetStringClaim(claims, "email"),
                Name = GetStringClaim(claims, "name"),
                Claims = claims?.ToDictionary(
                    k => k.Key,
                    v => (object)(v.Value.ValueKind == JsonValueKind.String ? v.Value.GetString()! : v.Value.ToString()))
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ID token validation failed for provider {ProviderId}", providerId);
            return new OidcTokenValidationResult { IsValid = false, Error = ex.Message };
        }
    }

    /// <inheritdoc />
    public async Task<OAuthTokenResponseDto> RefreshTokenAsync(
        string providerId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var config = await GetProviderAsync(providerId, cancellationToken)
            ?? throw new ArgumentException($"Provider '{providerId}' not found");

        var discovery = await DiscoverEndpointsAsync(config.DiscoveryUrl, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to discover endpoints for provider '{providerId}'");

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["refresh_token"] = refreshToken
        });

        var response = await _httpClient.PostAsync(discovery.TokenEndpoint, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Token refresh failed for provider {ProviderId}: {Response}", providerId, responseContent);
            throw new InvalidOperationException($"Token refresh failed: {response.StatusCode}");
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return new OAuthTokenResponseDto
        {
            AccessToken = tokenResponse?.AccessToken ?? string.Empty,
            RefreshToken = tokenResponse?.RefreshToken,
            IdToken = tokenResponse?.IdToken,
            ExpiresIn = tokenResponse?.ExpiresIn ?? 3600,
            TokenType = tokenResponse?.TokenType ?? "Bearer"
        };
    }

    /// <inheritdoc />
    public Task<OidcProviderConfig> RegisterProviderAsync(
        OidcProviderConfig config,
        CancellationToken cancellationToken = default)
    {
        _providers[config.ProviderId] = config;
        _logger.LogInformation("Registered OIDC provider: {ProviderId}", config.ProviderId);
        return Task.FromResult(config);
    }

    /// <inheritdoc />
    public Task<OidcProviderConfig> UpdateProviderAsync(
        string providerId,
        OidcProviderConfig config,
        CancellationToken cancellationToken = default)
    {
        config.ProviderId = providerId;
        _providers[providerId] = config;
        _discoveryCache.TryRemove(config.DiscoveryUrl, out _);
        _logger.LogInformation("Updated OIDC provider: {ProviderId}", providerId);
        return Task.FromResult(config);
    }

    /// <inheritdoc />
    public Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default)
    {
        if (_providers.TryRemove(providerId, out var config))
        {
            _discoveryCache.TryRemove(config.DiscoveryUrl, out _);
            _logger.LogInformation("Deleted OIDC provider: {ProviderId}", providerId);
        }
        return Task.CompletedTask;
    }

    // Helper methods
    private static string? GetStringClaim(Dictionary<string, JsonElement>? claims, string key)
    {
        if (claims?.TryGetValue(key, out var value) == true && value.ValueKind == JsonValueKind.String)
            return value.GetString();
        return null;
    }

    private static bool GetBoolClaim(Dictionary<string, JsonElement>? claims, string key)
    {
        if (claims?.TryGetValue(key, out var value) == true)
        {
            if (value.ValueKind == JsonValueKind.True) return true;
            if (value.ValueKind == JsonValueKind.False) return false;
        }
        return false;
    }

    // Internal response types
    private class OidcDiscoveryResponse
    {
        public string? Issuer { get; set; }
        public string? AuthorizationEndpoint { get; set; }
        public string? TokenEndpoint { get; set; }
        public string? UserinfoEndpoint { get; set; }
        public string? JwksUri { get; set; }
        public string? EndSessionEndpoint { get; set; }
        public string? RevocationEndpoint { get; set; }
        public string[]? ScopesSupported { get; set; }
        public string[]? ResponseTypesSupported { get; set; }
        public string[]? ClaimsSupported { get; set; }
    }

    private class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? TokenType { get; set; }
    }
}
