// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Okta Single Sign-On (SSO) service interface (TODO-AUTH-003).
/// Handles enterprise authentication via Okta.
/// </summary>
public interface IOktaSsoService
{
    /// <summary>
    /// Generates Okta SSO authorization URL.
    /// </summary>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <param name="codeChallenge">PKCE code challenge</param>
    /// <returns>URL to redirect user to Okta login</returns>
    string GetAuthorizationUrl(string state, string codeChallenge);

    /// <summary>
    /// Exchanges authorization code for tokens.
    /// </summary>
    /// <param name="code">Authorization code from Okta</param>
    /// <param name="codeVerifier">PKCE code verifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OAuth token response with access token, id token, and refresh token</returns>
    Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(
        string code,
        string codeVerifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user information from Okta userinfo endpoint.
    /// </summary>
    /// <param name="accessToken">Access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information from Okta</returns>
    Task<OAuthUserInfoDto> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    /// <param name="refreshToken">Okta refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New token response</returns>
    Task<OAuthTokenResponseDto> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates ID token signature and claims.
    /// </summary>
    /// <param name="idToken">ID token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid</returns>
    Task<bool> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates logout from Okta (single logout).
    /// </summary>
    /// <param name="idTokenHint">ID token hint for logout</param>
    /// <param name="postLogoutRedirectUri">URI to redirect after logout</param>
    /// <returns>Logout URL to redirect to</returns>
    string GetLogoutUrl(string? idTokenHint, string? postLogoutRedirectUri);
}

/// <summary>
/// Configuration options for Okta SSO
/// </summary>
public class OktaSsoOptions
{
    /// <summary>
    /// Okta tenant domain (e.g., "company.okta.com")
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI after authentication
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Authorization server ID (default: "default")
    /// </summary>
    public string AuthorizationServerId { get; set; } = "default";

    /// <summary>
    /// Requested scopes
    /// </summary>
    public string Scopes { get; set; } = "openid profile email";
}
