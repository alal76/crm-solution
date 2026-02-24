// CRM Solution - Customer Relationship Management System// CRM Solution - Customer Relationship Management System

































































































































































































































































}    }        public bool EmailVerified { get; set; }        public string? Picture { get; set; }        public string? FamilyName { get; set; }        public string? GivenName { get; set; }        public string? Name { get; set; }        public string? Email { get; set; }        public string? Sub { get; set; }    {    private class OktaUserInfo    }        public string? TokenType { get; set; }        public int ExpiresIn { get; set; }        public string? IdToken { get; set; }        public string? RefreshToken { get; set; }        public string? AccessToken { get; set; }    {    private class OktaTokenResponse    // Internal response types    }        return queryParams.Count > 0 ? $"{logoutUrl}?{string.Join("&", queryParams)}" : logoutUrl;            queryParams.Add($"post_logout_redirect_uri={Uri.EscapeDataString(postLogoutRedirectUri)}");        if (!string.IsNullOrEmpty(postLogoutRedirectUri))            queryParams.Add($"id_token_hint={Uri.EscapeDataString(idTokenHint)}");        if (!string.IsNullOrEmpty(idTokenHint))        var queryParams = new List<string>();        var logoutUrl = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}/v1/logout";    {    public string GetLogoutUrl(string? idTokenHint, string? postLogoutRedirectUri)    /// <inheritdoc />    }        }            return false;            _logger.LogWarning(ex, "ID token validation failed");        {        catch (Exception ex)        }            return true;            }                    return false;                if (expDateTime < DateTime.UtcNow)                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expTime).UtcDateTime;                var expTime = long.Parse(exp?.ToString() ?? "0");            {            if (claims?.TryGetValue("exp", out var exp) == true)            // Validate expiration            }                    return false;                if (issuer?.ToString() != expectedIssuer)                var expectedIssuer = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}";            {            if (claims?.TryGetValue("iss", out var issuer) == true)            // Validate issuer            var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(decodedPayload);            var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(paddedPayload));            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');            var payload = parts[1];                return false;            if (parts.Length != 3)            var parts = idToken.Split('.');        {        try        // For now, perform basic validation        // In production, validate JWT signature using Okta JWKS    {        CancellationToken cancellationToken = default)        string idToken,    public async Task<bool> ValidateIdTokenAsync(    /// <inheritdoc />    }        };            TokenType = tokenResponse?.TokenType ?? "Bearer"            ExpiresIn = tokenResponse?.ExpiresIn ?? 3600,            IdToken = tokenResponse?.IdToken,            RefreshToken = tokenResponse?.RefreshToken,            AccessToken = tokenResponse?.AccessToken ?? string.Empty,        {        return new OAuthTokenResponseDto            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });        var tokenResponse = JsonSerializer.Deserialize<OktaTokenResponse>(responseContent,        }            throw new InvalidOperationException($"Okta token refresh failed: {response.StatusCode}");            _logger.LogError("Okta token refresh failed: {Response}", responseContent);        {        if (!response.IsSuccessStatusCode)        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);        var response = await _httpClient.PostAsync(tokenUrl, content, cancellationToken);        });            ["refresh_token"] = refreshToken            ["client_secret"] = _options.ClientSecret,            ["client_id"] = _options.ClientId,            ["grant_type"] = "refresh_token",        {        var content = new FormUrlEncodedContent(new Dictionary<string, string>        var tokenUrl = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}/v1/token";    {        CancellationToken cancellationToken = default)        string refreshToken,    public async Task<OAuthTokenResponseDto> RefreshTokenAsync(    /// <inheritdoc />    }        };            Provider = "okta"            EmailVerified = userInfo?.EmailVerified ?? false,            Picture = userInfo?.Picture,            FamilyName = userInfo?.FamilyName,            GivenName = userInfo?.GivenName,            Name = userInfo?.Name ?? string.Empty,            Email = userInfo?.Email ?? string.Empty,            Id = userInfo?.Sub ?? string.Empty,        {        return new OAuthUserInfoDto            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });        var userInfo = JsonSerializer.Deserialize<OktaUserInfo>(content,        }            throw new InvalidOperationException($"Failed to get Okta user info: {response.StatusCode}");            _logger.LogError("Failed to get Okta user info: {Response}", content);        {        if (!response.IsSuccessStatusCode)        var content = await response.Content.ReadAsStringAsync(cancellationToken);        var response = await _httpClient.SendAsync(request, cancellationToken);        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);        var request = new HttpRequestMessage(HttpMethod.Get, userInfoUrl);        var userInfoUrl = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}/v1/userinfo";    {        CancellationToken cancellationToken = default)        string accessToken,    public async Task<OAuthUserInfoDto> GetUserInfoAsync(    /// <inheritdoc />    }        }            throw;            _logger.LogError(ex, "Failed to exchange Okta authorization code");        {        catch (HttpRequestException ex)        }            };                TokenType = tokenResponse?.TokenType ?? "Bearer"                ExpiresIn = tokenResponse?.ExpiresIn ?? 3600,                IdToken = tokenResponse?.IdToken,                RefreshToken = tokenResponse?.RefreshToken,                AccessToken = tokenResponse?.AccessToken ?? string.Empty,            {            return new OAuthTokenResponseDto                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });            var tokenResponse = JsonSerializer.Deserialize<OktaTokenResponse>(responseContent,            }                throw new InvalidOperationException($"Okta token exchange failed: {response.StatusCode}");                _logger.LogError("Okta token exchange failed: {Response}", responseContent);            {            if (!response.IsSuccessStatusCode)            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);            var response = await _httpClient.PostAsync(tokenUrl, content, cancellationToken);        {        try        });            ["redirect_uri"] = _options.RedirectUri            ["code_verifier"] = codeVerifier,            ["code"] = code,            ["client_secret"] = _options.ClientSecret,            ["client_id"] = _options.ClientId,            ["grant_type"] = "authorization_code",        {        var content = new FormUrlEncodedContent(new Dictionary<string, string>        var tokenUrl = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}/v1/token";    {        CancellationToken cancellationToken = default)        string codeVerifier,        string code,    public async Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(    /// <inheritdoc />    }        return $"{baseUrl}?{queryString}";        var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));        };            ["code_challenge_method"] = "S256"            ["code_challenge"] = codeChallenge,            ["state"] = state,            ["redirect_uri"] = _options.RedirectUri,            ["scope"] = _options.Scopes,            ["response_type"] = "code",            ["client_id"] = _options.ClientId,        {        var queryParams = new Dictionary<string, string>        var baseUrl = $"https://{_options.Domain}/oauth2/{_options.AuthorizationServerId}/v1/authorize";    {    public string GetAuthorizationUrl(string state, string codeChallenge)    /// <inheritdoc />    }        _logger = logger;        _options = options.Value;        _httpClient = httpClient;    {        ILogger<OktaSsoService> logger)        IOptions<OktaSsoOptions> options,        HttpClient httpClient,    public OktaSsoService(    private readonly ILogger<OktaSsoService> _logger;    private readonly OktaSsoOptions _options;    private readonly HttpClient _httpClient;{public class OktaSsoService : IOktaSsoService/// </summary>/// Handles enterprise authentication via Okta./// Okta Single Sign-On (SSO) service implementation (TODO-AUTH-003)./// <summary>namespace CRM.Infrastructure.Services.Auth;using Microsoft.Extensions.Options;using Microsoft.Extensions.Logging;using CRM.Core.Interfaces;using CRM.Core.Dtos;using System.Text.Json;using System.Text;using System.Security.Cryptography;using System.Net.Http.Headers;// See the LICENSE file in the root directory for full terms.// the terms of the LICENSE file. Commercial use requires a separate license.// This software is source-available. Non-commercial use is permitted under//// Copyright (C) 2024-2026 Abhishek Lal// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
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
        _httpClient.BaseAddress = new Uri($"https://{_options.Domain}/");
    }

    /// <inheritdoc />
    public string GetAuthorizationUrl(string state, string codeChallenge)
    {
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
            Id = userInfo?.Sub ?? string.Empty,
            Email = userInfo?.Email ?? string.Empty,
            Name = userInfo?.Name,
            FirstName = userInfo?.GivenName,
            LastName = userInfo?.FamilyName,
            Picture = userInfo?.Picture,
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
                return false;

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
            queryParams["id_token_hint"] = idTokenHint;

        if (!string.IsNullOrEmpty(postLogoutRedirectUri))
            queryParams["post_logout_redirect_uri"] = postLogoutRedirectUri;

        if (queryParams.Count == 0)
            return baseUrl;

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
