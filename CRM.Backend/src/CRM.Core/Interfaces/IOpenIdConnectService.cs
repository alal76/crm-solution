// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// DTO representing an OIDC user profile.
/// </summary>
public class OidcUserProfile
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? PreferredUsername { get; set; }
    public string? Picture { get; set; }
    public string[]? Groups { get; set; }
    public bool EmailVerified { get; set; }
    public Dictionary<string, object>? AdditionalClaims { get; set; }
}

/// <summary>
/// DTO for OIDC authentication result.
/// </summary>
public class OidcAuthResult
{
    public bool Success { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public OidcUserProfile? UserProfile { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// DTO for OIDC provider discovery metadata.
/// </summary>
public class OidcDiscoveryDocument
{
    public string Issuer { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
    public string JwksUri { get; set; } = string.Empty;
    public string? EndSessionEndpoint { get; set; }
    public string? RevocationEndpoint { get; set; }
    public string[]? ScopesSupported { get; set; }
    public string[]? ResponseTypesSupported { get; set; }
    public string[]? ClaimsSupported { get; set; }
}

/// <summary>
/// Service interface for generic OpenID Connect provider support.
/// TODO-AUTH-004: Generic OIDC Provider Support
/// </summary>
public interface IOpenIdConnectService
{
    /// <summary>
    /// Generates the authorization URL for initiating OIDC login.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="state">CSRF state token</param>
    /// <param name="nonce">Nonce for ID token validation</param>
    /// <param name="codeVerifier">PKCE code verifier (optional)</param>
    /// <returns>The authorization URL to redirect the user to</returns>
    Task<string> GetAuthorizationUrlAsync(string providerName, string state, string nonce, string? codeVerifier = null);

    /// <summary>
    /// Exchanges the authorization code for tokens and retrieves user profile.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="code">The authorization code from callback</param>
    /// <param name="codeVerifier">PKCE code verifier (if used)</param>
    /// <param name="expectedNonce">The expected nonce for ID token validation</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication result with tokens and user profile</returns>
    Task<OidcAuthResult> ExchangeCodeAsync(string providerName, string code, string? codeVerifier = null, string? expectedNonce = null, CancellationToken ct = default);

    /// <summary>
    /// Validates an OIDC ID token.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="idToken">The ID token to validate</param>
    /// <param name="expectedNonce">The expected nonce</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if the token is valid</returns>
    Task<bool> ValidateIdTokenAsync(string providerName, string idToken, string? expectedNonce = null, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the user profile using an access token.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="accessToken">The access token</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The OIDC user profile</returns>
    Task<OidcUserProfile?> GetUserProfileAsync(string providerName, string accessToken, CancellationToken ct = default);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>New authentication result with refreshed tokens</returns>
    Task<OidcAuthResult> RefreshTokenAsync(string providerName, string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Revokes a token.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="token">The token to revoke</param>
    /// <param name="tokenTypeHint">Token type hint</param>
    /// <param name="ct">Cancellation token</param>
    Task RevokeTokenAsync(string providerName, string token, string tokenTypeHint = "access_token", CancellationToken ct = default);

    /// <summary>
    /// Generates the logout URL for SSO logout.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="idToken">The ID token for logout</param>
    /// <param name="postLogoutRedirectUri">Redirect URI after logout</param>
    /// <returns>The logout URL</returns>
    Task<string?> GetLogoutUrlAsync(string providerName, string idToken, string? postLogoutRedirectUri = null);

    /// <summary>
    /// Retrieves the OIDC discovery document for a provider.
    /// </summary>
    /// <param name="providerName">The configured provider name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The discovery document</returns>
    Task<OidcDiscoveryDocument?> GetDiscoveryDocumentAsync(string providerName, CancellationToken ct = default);

    /// <summary>
    /// Lists all configured OIDC providers.
    /// </summary>
    /// <returns>List of provider names</returns>
    IEnumerable<string> GetConfiguredProviders();

    /// <summary>
    /// Checks if a specific provider is configured and available.
    /// </summary>
    /// <param name="providerName">The provider name</param>
    /// <returns>True if the provider is configured</returns>
    bool IsProviderConfigured(string providerName);

    /// <summary>
    /// Generates a secure PKCE code verifier.
    /// </summary>
    /// <returns>A random code verifier string</returns>
    string GenerateCodeVerifier();

    /// <summary>
    /// Generates a PKCE code challenge from a code verifier.
    /// </summary>
    /// <param name="codeVerifier">The code verifier</param>
    /// <returns>The code challenge (S256 hash)</returns>
    string GenerateCodeChallenge(string codeVerifier);
}
