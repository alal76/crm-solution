// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Authentication.OAuth;

/// <summary>
/// Google OAuth 2.0 provider implementation with PKCE support.
/// </summary>
public class GoogleOAuthProvider : IGoogleOAuthProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleOAuthProvider> _logger;
    private readonly GoogleOAuthOptions _options;

    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

    public GoogleOAuthProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleOAuthProvider> logger,
        IOptions<GoogleOAuthOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string GetAuthorizationUrl(string state, string codeChallenge)
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId },
            { "redirect_uri", _options.RedirectUri },
            { "response_type", "code" },
            { "scope", "openid email profile" },
            { "state", state },
            { "code_challenge", codeChallenge },
            { "code_challenge_method", "S256" },
            { "prompt", "consent" }
        };

        return BuildUrl(AuthorizationEndpoint, parameters);
    }

    public async Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(string code, string codeVerifier, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "redirect_uri", _options.RedirectUri },
                    { "code_verifier", codeVerifier }
                })
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthTokenResponseDto
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                TokenType = tokenData.GetProperty("token_type").GetString() ?? "Bearer",
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32(),
                Scope = tokenData.TryGetProperty("scope", out var scope) ? scope.GetString() : null,
                IdToken = tokenData.TryGetProperty("id_token", out var idToken) ? idToken.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Google OAuth code");
            throw;
        }
    }

    public async Task<OAuthUserInfoDto> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.GetAsync(UserInfoEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthUserInfoDto
            {
                ProviderId = userData.GetProperty("id").GetString() ?? string.Empty,
                Email = userData.GetProperty("email").GetString() ?? string.Empty,
                Name = userData.GetProperty("name").GetString() ?? string.Empty,
                PictureUrl = userData.TryGetProperty("picture", out var pic) ? pic.GetString() : null,
                GivenName = userData.TryGetProperty("given_name", out var given) ? given.GetString() : null,
                FamilyName = userData.TryGetProperty("family_name", out var family) ? family.GetString() : null,
                EmailVerified = userData.TryGetProperty("verified_email", out var verified) && verified.GetBoolean(),
                Provider = "google"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user info");
            throw;
        }
    }

    public async Task<OAuthTokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret }
                })
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthTokenResponseDto
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Google token");
            throw;
        }
    }

    private string BuildUrl(string baseUrl, Dictionary<string, string> parameters)
    {
        var query = string.Join("&", parameters.Values);
        var sb = new StringBuilder(baseUrl);
        sb.Append("?");
        bool first = true;
        foreach (var param in parameters)
        {
            if (!first)
                sb.Append("&");
            sb.Append(Uri.EscapeDataString(param.Key)).Append("=").Append(Uri.EscapeDataString(param.Value));
            first = false;
        }
        return sb.ToString();
    }
}

/// <summary>
/// Microsoft OAuth 2.0 / Azure AD provider implementation with PKCE support.
/// </summary>
public class MicrosoftOAuthProvider : IMicrosoftOAuthProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicrosoftOAuthProvider> _logger;
    private readonly MicrosoftOAuthOptions _options;

    private const string AuthorizationEndpoint = "https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize";
    private const string TokenEndpoint = "https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
    private const string UserInfoEndpoint = "https://graph.microsoft.com/v1.0/me";

    public MicrosoftOAuthProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<MicrosoftOAuthProvider> logger,
        IOptions<MicrosoftOAuthOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string GetAuthorizationUrl(string state, string codeChallenge, string? tenant = null)
    {
        tenant ??= _options.Tenant ?? "common";
        var endpoint = AuthorizationEndpoint.Replace("{tenant}", tenant);

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId },
            { "redirect_uri", _options.RedirectUri },
            { "response_type", "code" },
            { "scope", "openid profile email offline_access" },
            { "state", state },
            { "code_challenge", codeChallenge },
            { "code_challenge_method", "S256" },
            { "prompt", "login" }
        };

        return BuildUrl(endpoint, parameters);
    }

    public async Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(string code, string codeVerifier, string? tenant = null, CancellationToken cancellationToken = default)
    {
        try
        {
            tenant ??= _options.Tenant ?? "common";
            var endpoint = TokenEndpoint.Replace("{tenant}", tenant);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "redirect_uri", _options.RedirectUri },
                    { "code_verifier", codeVerifier }
                })
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthTokenResponseDto
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32(),
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : null,
                Scope = tokenData.TryGetProperty("scope", out var scope) ? scope.GetString() : null,
                IdToken = tokenData.TryGetProperty("id_token", out var idToken) ? idToken.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Microsoft OAuth code");
            throw;
        }
    }

    public async Task<OAuthUserInfoDto> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.GetAsync(UserInfoEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthUserInfoDto
            {
                ProviderId = userData.GetProperty("id").GetString() ?? string.Empty,
                Email = userData.GetProperty("userPrincipalName").GetString() ?? string.Empty,
                Name = userData.GetProperty("displayName").GetString() ?? string.Empty,
                GivenName = userData.TryGetProperty("givenName", out var given) ? given.GetString() : null,
                FamilyName = userData.TryGetProperty("surname", out var family) ? family.GetString() : null,
                Provider = "microsoft",
                EmailVerified = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Microsoft user info");
            throw;
        }
    }

    public async Task<OAuthTokenResponseDto> RefreshTokenAsync(string refreshToken, string? tenant = null, CancellationToken cancellationToken = default)
    {
        try
        {
            tenant ??= _options.Tenant ?? "common";
            var endpoint = TokenEndpoint.Replace("{tenant}", tenant);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret }
                })
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthTokenResponseDto
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                TokenType = "Bearer",
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32(),
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Microsoft token");
            throw;
        }
    }

    private string BuildUrl(string baseUrl, Dictionary<string, string> parameters)
    {
        var sb = new StringBuilder(baseUrl);
        sb.Append("?");
        bool first = true;
        foreach (var param in parameters)
        {
            if (!first)
                sb.Append("&");
            sb.Append(Uri.EscapeDataString(param.Key)).Append("=").Append(Uri.EscapeDataString(param.Value));
            first = false;
        }
        return sb.ToString();
    }
}

/// <summary>
/// GitHub OAuth 2.0 provider implementation.
/// </summary>
public class GitHubOAuthProvider : IGitHubOAuthProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubOAuthProvider> _logger;
    private readonly GitHubOAuthOptions _options;

    private const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    private const string TokenEndpoint = "https://github.com/login/oauth/access_token";
    private const string UserInfoEndpoint = "https://api.github.com/user";
    private const string UserEmailEndpoint = "https://api.github.com/user/emails";

    public GitHubOAuthProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubOAuthProvider> logger,
        IOptions<GitHubOAuthOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string GetAuthorizationUrl(string state)
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId },
            { "redirect_uri", _options.RedirectUri },
            { "scope", "user:email" },
            { "state", state },
            { "allow_signup", "true" }
        };

        var sb = new StringBuilder(AuthorizationEndpoint);
        sb.Append("?");
        bool first = true;
        foreach (var param in parameters)
        {
            if (!first)
                sb.Append("&");
            sb.Append(Uri.EscapeDataString(param.Key)).Append("=").Append(Uri.EscapeDataString(param.Value));
            first = false;
        }
        return sb.ToString();
    }

    public async Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "code", code },
                    { "redirect_uri", _options.RedirectUri }
                })
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthTokenResponseDto
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                TokenType = "token",
                Scope = tokenData.TryGetProperty("scope", out var scope) ? scope.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging GitHub OAuth code");
            throw;
        }
    }

    public async Task<OAuthUserInfoDto> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Authorization", $"token {accessToken}");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("User-Agent", "CRM-Solution");

            var response = await client.GetAsync(UserInfoEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userData = JsonSerializer.Deserialize<JsonElement>(content);

            // Get email separately
            var email = await GetUserEmailAsync(accessToken, cancellationToken) ?? userData.GetProperty("email").GetString() ?? string.Empty;

            return new OAuthUserInfoDto
            {
                ProviderId = userData.GetProperty("id").GetInt32().ToString(),
                Email = email,
                Name = userData.GetProperty("name").GetString() ?? userData.GetProperty("login").GetString() ?? string.Empty,
                PictureUrl = userData.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null,
                Provider = "github",
                EmailVerified = userData.TryGetProperty("email_verified", out var verified) && verified.GetBoolean()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitHub user info");
            throw;
        }
    }

    public async Task<string?> GetUserEmailAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Authorization", $"token {accessToken}");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("User-Agent", "CRM-Solution");

            var response = await client.GetAsync(UserEmailEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var emails = JsonSerializer.Deserialize<JsonElement[]>(content);

            // Return primary email or first verified email
            foreach (var emailData in emails ?? Array.Empty<JsonElement>())
            {
                if (emailData.GetProperty("primary").GetBoolean())
                {
                    return emailData.GetProperty("email").GetString();
                }
            }

            return emails?.Length > 0 ? emails[0].GetProperty("email").GetString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting GitHub user email");
            return null;
        }
    }
}
