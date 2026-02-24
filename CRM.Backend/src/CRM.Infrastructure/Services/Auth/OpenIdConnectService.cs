// CRM Solution - Customer Relationship Management System// CRM Solution - Customer Relationship Management System


























































































































































































































































































































































































































































}    }            .TrimEnd('=');            .Replace('/', '_')            .Replace('+', '-')        return Convert.ToBase64String(bytes)    {    private static string Base64UrlEncode(byte[] bytes)    }        return null;        }            return prop.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray();        {        if (element.TryGetProperty(claimName, out var prop) && prop.ValueKind == JsonValueKind.Array)    {    private static string[]? GetArrayClaimValue(JsonElement element, string claimName)    }        return element.TryGetProperty(claimName, out var prop) && prop.ValueKind == JsonValueKind.True;    {    private static bool GetBoolClaimValue(JsonElement element, string claimName)    }            : null;            ? prop.GetString()        return element.TryGetProperty(claimName, out var prop) && prop.ValueKind == JsonValueKind.String    {    private static string? GetClaimValue(JsonElement element, string claimName)    }        return options;        }            throw new InvalidOperationException($"OIDC provider '{providerName}' is not configured");        {        if (!_providers.TryGetValue(providerName, out var options))    {    private OpenIdConnectOptions GetProviderOptions(string providerName)    }        return Base64UrlEncode(hash);        var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));        using var sha256 = SHA256.Create();    {    public string GenerateCodeChallenge(string codeVerifier)    /// <inheritdoc />    }        return Base64UrlEncode(bytes);        rng.GetBytes(bytes);        using var rng = RandomNumberGenerator.Create();        var bytes = new byte[32];    {    public string GenerateCodeVerifier()    /// <inheritdoc />    }        return _providers.ContainsKey(providerName);    {    public bool IsProviderConfigured(string providerName)    /// <inheritdoc />    }        return _providers.Keys;    {    public IEnumerable<string> GetConfiguredProviders()    /// <inheritdoc />    }        }            return null;            _logger.LogError(ex, "Error fetching OIDC discovery document for provider {Provider}", providerName);        {        catch (Exception ex)        }            return discovery;            _discoveryCache[providerName] = discovery;            };                ClaimsSupported = GetArrayClaimValue(doc, "claims_supported")                ResponseTypesSupported = GetArrayClaimValue(doc, "response_types_supported"),                ScopesSupported = GetArrayClaimValue(doc, "scopes_supported"),                RevocationEndpoint = GetClaimValue(doc, "revocation_endpoint"),                EndSessionEndpoint = GetClaimValue(doc, "end_session_endpoint"),                JwksUri = GetClaimValue(doc, "jwks_uri") ?? string.Empty,                UserInfoEndpoint = GetClaimValue(doc, "userinfo_endpoint") ?? string.Empty,                TokenEndpoint = GetClaimValue(doc, "token_endpoint") ?? string.Empty,                AuthorizationEndpoint = GetClaimValue(doc, "authorization_endpoint") ?? string.Empty,                Issuer = GetClaimValue(doc, "issuer") ?? options.Authority,            {            var discovery = new OidcDiscoveryDocument            var doc = JsonSerializer.Deserialize<JsonElement>(content);            var content = await response.Content.ReadAsStringAsync(ct);            }                return null;                _logger.LogWarning("Failed to fetch OIDC discovery document from {Url}: {Status}", metadataUrl, response.StatusCode);            {            if (!response.IsSuccessStatusCode)            var response = await client.GetAsync(metadataUrl, ct);            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);            using var client = _httpClientFactory.CreateClient();                : $"{options.Authority.TrimEnd('/')}/.well-known/openid-configuration";                ? options.MetadataAddress            var metadataUrl = !string.IsNullOrEmpty(options.MetadataAddress)            var options = GetProviderOptions(providerName);        {        try        }            return cached;        {        if (_discoveryCache.TryGetValue(providerName, out var cached))    {    public async Task<OidcDiscoveryDocument?> GetDiscoveryDocumentAsync(string providerName, CancellationToken ct = default)    /// <inheritdoc />    }        return $"{discovery.EndSessionEndpoint}?{queryString}";        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));        }            parameters["post_logout_redirect_uri"] = postLogoutRedirectUri;        {        if (!string.IsNullOrEmpty(postLogoutRedirectUri))        };            ["id_token_hint"] = idToken,        {        var parameters = new Dictionary<string, string>        }            return null;        {        if (discovery?.EndSessionEndpoint == null)        var discovery = await GetDiscoveryDocumentAsync(providerName);    {    public async Task<string?> GetLogoutUrlAsync(string providerName, string idToken, string? postLogoutRedirectUri = null)    /// <inheritdoc />    }        }            _logger.LogError(ex, "Error revoking token with OIDC provider {Provider}", providerName);        {        catch (Exception ex)        }            await client.PostAsync(discovery.RevocationEndpoint, content, ct);            var content = new FormUrlEncodedContent(parameters);            using var client = _httpClientFactory.CreateClient();            };                ["client_secret"] = options.ClientSecret,                ["client_id"] = options.ClientId,                ["token_type_hint"] = tokenTypeHint,                ["token"] = token,            {            var parameters = new Dictionary<string, string>            }                return;                _logger.LogWarning("OIDC provider {Provider} does not support token revocation", providerName);            {            if (discovery?.RevocationEndpoint == null)                        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);            var options = GetProviderOptions(providerName);        {        try    {    public async Task RevokeTokenAsync(string providerName, string token, string tokenTypeHint = "access_token", CancellationToken ct = default)    /// <inheritdoc />    }        }            return new OidcAuthResult { Success = false, Error = "refresh_error", ErrorDescription = ex.Message };            _logger.LogError(ex, "Error refreshing token with OIDC provider {Provider}", providerName);        {        catch (Exception ex)        }            };                ExpiresIn = tokenResponse.TryGetProperty("expires_in", out var expProp) ? expProp.GetInt32() : 3600                RefreshToken = tokenResponse.TryGetProperty("refresh_token", out var refProp) ? refProp.GetString() : refreshToken,                IdToken = tokenResponse.TryGetProperty("id_token", out var idProp) ? idProp.GetString() : null,                AccessToken = tokenResponse.GetProperty("access_token").GetString(),                ProviderName = providerName,                Success = true,            {            return new OidcAuthResult            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);            }                return new OidcAuthResult { Success = false, Error = "refresh_failed", ErrorDescription = responseContent };            {            if (!response.IsSuccessStatusCode)            var responseContent = await response.Content.ReadAsStringAsync(ct);            var response = await client.PostAsync(discovery.TokenEndpoint, content, ct);            var content = new FormUrlEncodedContent(parameters);            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);            using var client = _httpClientFactory.CreateClient();            };                ["client_secret"] = options.ClientSecret,                ["client_id"] = options.ClientId,                ["refresh_token"] = refreshToken,                ["grant_type"] = "refresh_token",            {            var parameters = new Dictionary<string, string>            }                return new OidcAuthResult { Success = false, Error = "discovery_failed" };            {            if (discovery == null)                        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);            var options = GetProviderOptions(providerName);        {        try    {    public async Task<OidcAuthResult> RefreshTokenAsync(string providerName, string refreshToken, CancellationToken ct = default)    /// <inheritdoc />    }        }            return null;            _logger.LogError(ex, "Error getting user profile from OIDC provider {Provider}", providerName);        {        catch (Exception ex)        }            };                Groups = GetArrayClaimValue(userInfo, options.GroupsClaimType)                EmailVerified = GetBoolClaimValue(userInfo, "email_verified"),                Picture = GetClaimValue(userInfo, "picture"),                PreferredUsername = GetClaimValue(userInfo, "preferred_username"),                FamilyName = GetClaimValue(userInfo, "family_name"),                GivenName = GetClaimValue(userInfo, "given_name"),                Name = GetClaimValue(userInfo, options.NameClaimType) ?? GetClaimValue(userInfo, "name"),                Email = GetClaimValue(userInfo, options.EmailClaimType) ?? GetClaimValue(userInfo, "email"),                Sub = GetClaimValue(userInfo, "sub"),            {            return new OidcUserProfile            var userInfo = JsonSerializer.Deserialize<JsonElement>(content);            var content = await response.Content.ReadAsStringAsync(ct);            }                return null;                _logger.LogWarning("Failed to get user info from OIDC provider {Provider}: {Status}", providerName, response.StatusCode);            {            if (!response.IsSuccessStatusCode)            var response = await client.GetAsync(discovery.UserInfoEndpoint, ct);            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);            using var client = _httpClientFactory.CreateClient();            }                return null;            {            if (discovery == null || string.IsNullOrEmpty(discovery.UserInfoEndpoint))                        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);            var options = GetProviderOptions(providerName);        {        try    {    public async Task<OidcUserProfile?> GetUserProfileAsync(string providerName, string accessToken, CancellationToken ct = default)    /// <inheritdoc />    }        }            return false;            _logger.LogError(ex, "Error validating ID token for provider {Provider}", providerName);        {        catch (Exception ex)        }            return true;            }                return false;                _logger.LogWarning("Invalid ID token structure for provider {Provider}", providerName);            {            if (parts.Length != 3)            var parts = idToken.Split('.');        {        try        // For now, just check token structure        // TODO: Implement full JWT validation with JWKS    {    public async Task<bool> ValidateIdTokenAsync(string providerName, string idToken, string? expectedNonce = null, CancellationToken ct = default)    /// <inheritdoc />    }        }            return new OidcAuthResult { Success = false, Error = "exchange_error", ErrorDescription = ex.Message };            _logger.LogError(ex, "Error exchanging code for tokens with OIDC provider {Provider}", providerName);        {        catch (Exception ex)        }            };                UserProfile = userProfile                ExpiresIn = expiresIn,                RefreshToken = refreshToken,                IdToken = idToken,                AccessToken = accessToken,                ProviderName = providerName,                Success = true,            {            return new OidcAuthResult            }                userProfile = await GetUserProfileAsync(providerName, accessToken, ct);            {            if (!string.IsNullOrEmpty(accessToken))            OidcUserProfile? userProfile = null;            // Get user profile            var expiresIn = tokenResponse.TryGetProperty("expires_in", out var expiresProp) ? expiresProp.GetInt32() : 3600;            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var refreshProp) ? refreshProp.GetString() : null;            var idToken = tokenResponse.TryGetProperty("id_token", out var idTokenProp) ? idTokenProp.GetString() : null;            var accessToken = tokenResponse.GetProperty("access_token").GetString();            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);            }                return new OidcAuthResult { Success = false, Error = "token_exchange_failed", ErrorDescription = responseContent };                _logger.LogWarning("OIDC token exchange failed for {Provider}: {Status} - {Response}", providerName, response.StatusCode, responseContent);            {            if (!response.IsSuccessStatusCode)            var responseContent = await response.Content.ReadAsStringAsync(ct);            var response = await client.PostAsync(discovery.TokenEndpoint, content, ct);            var content = new FormUrlEncodedContent(parameters);            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);            using var client = _httpClientFactory.CreateClient();            }                parameters["code_verifier"] = codeVerifier;            {            if (options.UsePkce && !string.IsNullOrEmpty(codeVerifier))            };                ["redirect_uri"] = options.RedirectUri,                ["client_secret"] = options.ClientSecret,                ["client_id"] = options.ClientId,                ["code"] = code,                ["grant_type"] = "authorization_code",            {            var parameters = new Dictionary<string, string>            }                return new OidcAuthResult { Success = false, Error = "discovery_failed", ErrorDescription = "Could not retrieve discovery document" };            {            if (discovery == null)                        var discovery = await GetDiscoveryDocumentAsync(providerName, ct);            var options = GetProviderOptions(providerName);        {        try    {    public async Task<OidcAuthResult> ExchangeCodeAsync(string providerName, string code, string? codeVerifier = null, string? expectedNonce = null, CancellationToken ct = default)    /// <inheritdoc />    }        return $"{discovery.AuthorizationEndpoint}?{queryString}";        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));        }            parameters["code_challenge_method"] = "S256";            parameters["code_challenge"] = GenerateCodeChallenge(codeVerifier);        {        if (options.UsePkce && !string.IsNullOrEmpty(codeVerifier))        }            parameters["response_mode"] = options.ResponseMode;        {        if (!string.IsNullOrEmpty(options.ResponseMode))        };            ["nonce"] = nonce,            ["state"] = state,            ["scope"] = options.Scopes,            ["response_type"] = options.ResponseType,            ["redirect_uri"] = options.RedirectUri,            ["client_id"] = options.ClientId,        {        var parameters = new Dictionary<string, string>        }            throw new InvalidOperationException($"Could not retrieve discovery document for provider {providerName}");        {        if (discovery == null)                var discovery = await GetDiscoveryDocumentAsync(providerName);        var options = GetProviderOptions(providerName);    {    public async Task<string> GetAuthorizationUrlAsync(string providerName, string state, string nonce, string? codeVerifier = null)    /// <inheritdoc />    }        _providers = providersOptions?.Value ?? new Dictionary<string, OpenIdConnectOptions>();        _logger = logger;        _httpClientFactory = httpClientFactory;    {        ILogger<OpenIdConnectService> logger)        IOptions<Dictionary<string, OpenIdConnectOptions>> providersOptions,        IHttpClientFactory httpClientFactory,    public OpenIdConnectService(    private readonly Dictionary<string, OidcDiscoveryDocument> _discoveryCache = new();    private readonly Dictionary<string, OpenIdConnectOptions> _providers;    private readonly ILogger<OpenIdConnectService> _logger;    private readonly IHttpClientFactory _httpClientFactory;{public class OpenIdConnectService : IOpenIdConnectService/// </summary>/// Supports multiple OIDC providers with PKCE and standard flows./// /// TODO-AUTH-004: Generic OIDC Provider Support/// Generic OpenID Connect provider service implementation./// <summary>namespace CRM.Infrastructure.Services.Auth;using Microsoft.Extensions.Options;using Microsoft.Extensions.Logging;using CRM.Core.Options;using CRM.Core.Interfaces;using System.Text.Json;using System.Text;using System.Security.Cryptography;using System.Net.Http.Headers;// See the LICENSE file in the root directory for full terms.// the terms of the LICENSE file. Commercial use requires a separate license.// This software is source-available. Non-commercial use is permitted under//// Copyright (C) 2024-2026 Abhishek Lal// Copyright (C) 2024-2026 Abhishek Lal
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

        var userInfoEndpoint = discovery?.UserInfoEndpoint ?? $"{options.Authority}/userinfo";

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
                UserInfoEndpoint = doc.TryGetProperty("userinfo_endpoint", out var userInfo) ? userInfo.GetString() ?? string.Empty : string.Empty,
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
