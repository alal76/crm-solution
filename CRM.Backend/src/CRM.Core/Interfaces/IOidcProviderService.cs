// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Generic OpenID Connect provider service interface (TODO-AUTH-004).
/// Supports multiple OIDC providers via configuration.
/// </summary>
public interface IOidcProviderService
{
    /// <summary>
    /// Gets available OIDC provider configurations.
    /// </summary>
    Task<IEnumerable<OidcProviderInfo>> GetProvidersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific provider by ID.
    /// </summary>
    Task<OidcProviderConfig?> GetProviderAsync(string providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers OIDC endpoints from the well-known configuration URL.
    /// </summary>
    /// <param name="discoveryUrl">The .well-known/openid-configuration URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<OidcDiscoveryDocument?> DiscoverEndpointsAsync(
        string discoveryUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates authorization URL for a provider.
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <param name="codeChallenge">PKCE code challenge</param>
    /// <param name="nonce">Nonce for replay protection</param>
    /// <returns>Authorization URL</returns>
    Task<string> GetAuthorizationUrlAsync(
        string providerId,
        string state,
        string codeChallenge,
        string nonce,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges authorization code for tokens.
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <param name="code">Authorization code</param>
    /// <param name="codeVerifier">PKCE code verifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(
        string providerId,
        string code,
        string codeVerifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user information from the provider's userinfo endpoint.
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <param name="accessToken">Access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<OAuthUserInfoDto> GetUserInfoAsync(
        string providerId,
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates ID token signature and claims.
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <param name="idToken">ID token to validate</param>
    /// <param name="nonce">Expected nonce value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<OidcTokenValidationResult> ValidateIdTokenAsync(
        string providerId,
        string idToken,
        string? nonce = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    Task<OAuthTokenResponseDto> RefreshTokenAsync(
        string providerId,
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new OIDC provider configuration.
    /// </summary>
    Task<OidcProviderConfig> RegisterProviderAsync(
        OidcProviderConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing OIDC provider configuration.
    /// </summary>
    Task<OidcProviderConfig> UpdateProviderAsync(
        string providerId,
        OidcProviderConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an OIDC provider configuration.
    /// </summary>
    Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Basic information about an OIDC provider
/// </summary>
public class OidcProviderInfo
{
    public string ProviderId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// Full OIDC provider configuration
/// </summary>
public class OidcProviderConfig
{
    public string ProviderId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DiscoveryUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scopes { get; set; } = "openid profile email";
    public string? LogoUrl { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool AutoCreateUsers { get; set; } = false;
    public string? DefaultRole { get; set; }
    public Dictionary<string, string>? ClaimMappings { get; set; }
}

/// <summary>
/// OIDC discovery document
/// </summary>
public class OidcDiscoveryDocument
{
    public string Issuer { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserinfoEndpoint { get; set; } = string.Empty;
    public string JwksUri { get; set; } = string.Empty;
    public string? EndSessionEndpoint { get; set; }
    public string? RevocationEndpoint { get; set; }
    public string[] ScopesSupported { get; set; } = Array.Empty<string>();
    public string[] ResponseTypesSupported { get; set; } = Array.Empty<string>();
    public string[] ClaimsSupported { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Result of ID token validation
/// </summary>
public class OidcTokenValidationResult
{
    public bool IsValid { get; set; }
    public string? Subject { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, object>? Claims { get; set; }
    public string? Error { get; set; }
}
