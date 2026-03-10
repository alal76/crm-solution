// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Auth;

/// <summary>
/// Okta Single Sign-On (SSO) service implementation (TODO-AUTH-003).
/// Provides enterprise authentication via Okta.
/// </summary>
public class OktaSsoService : IOktaSsoService
{
    private readonly ILogger<OktaSsoService> _logger;
    private readonly OktaSsoOptions _options;
    private readonly HttpClient _httpClient;

    public OktaSsoService(
        ILogger<OktaSsoService> logger,
        IOptions<OktaSsoOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("OktaSso");

        // Only set BaseAddress if Domain is configured
        if (!string.IsNullOrWhiteSpace(_options.Domain))
        {
            _httpClient.BaseAddress = new Uri($"https://{_options.Domain}/");
        }
        else
        {
            _logger.LogWarning("OktaSsoService: Domain not configured. SSO functionality will not be available.");
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Domain))
        {
            throw new InvalidOperationException(
                "Okta SSO is not configured. Please configure Okta settings before using SSO functionality.");
        }
    }

    /// <inheritdoc />
    public string GetAuthorizationUrl(string state, string codeChallenge)
    {
        ValidateConfiguration();

        var authServer = _options.AuthorizationServerId;
        var baseUrl = $"https://{_options.Domain}/oauth2/{authServer}/v1/authorize";

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["response_type"] = "code",
            ["scope"] = _options.Scopes,
            ["redirect_uri"] = _options.RedirectUri,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{baseUrl}?{queryString}";
    }

    /// <inheritdoc />
    public async Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(
        string code,
        string codeVerifier,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var authServer = _options.AuthorizationServerId;
        var tokenEndpoint = $"oauth2/{authServer}/v1/token";

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code_verifier"] = codeVerifier
        };

        var response = await _httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(tokenRequest),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Okta token exchange failed: {Error}", error);
            throw new InvalidOperationException($"Okta token exchange failed: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<OktaTokenResponse>(cancellationToken: cancellationToken);

        return new OAuthTokenResponseDto
        {
            AccessToken = tokenResponse?.AccessToken ?? string.Empty,
            RefreshToken = tokenResponse?.RefreshToken,
            IdToken = tokenResponse?.IdToken,
            TokenType = tokenResponse?.TokenType ?? "Bearer",
            ExpiresIn = tokenResponse?.ExpiresIn ?? 3600,
            Scope = tokenResponse?.Scope
        };
    }

    /// <inheritdoc />
    public async Task<OAuthUserInfoDto> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var authServer = _options.AuthorizationServerId;
        var userinfoEndpoint = $"oauth2/{authServer}/v1/userinfo";

        using var request = new HttpRequestMessage(HttpMethod.Get, userinfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Okta userinfo request failed: {Error}", error);
            throw new InvalidOperationException($"Okta userinfo request failed: {error}");
        }

        var userInfo = await response.Content.ReadFromJsonAsync<OktaUserInfo>(cancellationToken: cancellationToken);

        return new OAuthUserInfoDto
        {
            ProviderId = userInfo?.Sub ?? string.Empty,
            Email = userInfo?.Email ?? string.Empty,
            Name = userInfo?.Name ?? string.Empty,
            GivenName = userInfo?.GivenName,
            FamilyName = userInfo?.FamilyName,
            PictureUrl = userInfo?.Picture,
            EmailVerified = userInfo?.EmailVerified ?? false,
            Provider = "okta"
        };
    }

    /// <inheritdoc />
    public async Task<OAuthTokenResponseDto> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var authServer = _options.AuthorizationServerId;
        var tokenEndpoint = $"oauth2/{authServer}/v1/token";

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"] = _options.Scopes
        };

        var response = await _httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(tokenRequest),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Okta token refresh failed: {Error}", error);
            throw new InvalidOperationException($"Okta token refresh failed: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<OktaTokenResponse>(cancellationToken: cancellationToken);

        return new OAuthTokenResponseDto
        {
            AccessToken = tokenResponse?.AccessToken ?? string.Empty,
            RefreshToken = tokenResponse?.RefreshToken ?? refreshToken,
            IdToken = tokenResponse?.IdToken,
            TokenType = tokenResponse?.TokenType ?? "Bearer",
            ExpiresIn = tokenResponse?.ExpiresIn ?? 3600,
            Scope = tokenResponse?.Scope
        };
    }

    /// <inheritdoc />
    public async Task<bool> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        // For production, implement full JWT validation with JWKS
        // This is a simplified implementation
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
            var json = Encoding.UTF8.GetString(decodedPayload);
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            // Validate issuer
            if (claims != null && claims.TryGetValue("iss", out var issuer))
            {
                var expectedIssuer = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}";
                if (issuer.GetString() != expectedIssuer)
                {
                    _logger.LogWarning("Invalid issuer in Okta ID token");
                    return false;
                }
            }

            // Validate audience
            if (claims != null && claims.TryGetValue("aud", out var audience))
            {
                if (audience.GetString() != _options.ClientId)
                {
                    _logger.LogWarning("Invalid audience in Okta ID token");
                    return false;
                }
            }

            // Validate expiration
            if (claims != null && claims.TryGetValue("exp", out var exp))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64());
                if (expTime < DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Okta ID token has expired");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Okta ID token");
            return false;
        }
    }

    /// <inheritdoc />
    public string GetLogoutUrl(string? idTokenHint, string? postLogoutRedirectUri)
    {
        var authServer = _options.AuthorizationServerId;
        var baseUrl = $"https://{_options.Domain}/oauth2/{authServer}/v1/logout";

        var queryParams = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(idTokenHint))
        {
            queryParams["id_token_hint"] = idTokenHint;
        }

        if (!string.IsNullOrEmpty(postLogoutRedirectUri))
        {
            queryParams["post_logout_redirect_uri"] = postLogoutRedirectUri;
        }

        if (queryParams.Count == 0)
        {
            return baseUrl;
        }

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{baseUrl}?{queryString}";
    }

    // Internal DTOs for Okta responses
    private class OktaTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? IdToken { get; set; }
        public string? TokenType { get; set; }
        public int? ExpiresIn { get; set; }
        public string? Scope { get; set; }
    }

    private class OktaUserInfo
    {
        public string? Sub { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Picture { get; set; }
        public bool? EmailVerified { get; set; }
    }
}
