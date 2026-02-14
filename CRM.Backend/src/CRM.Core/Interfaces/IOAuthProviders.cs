using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Interfaces;

/// <summary>
/// Google OAuth 2.0 provider interface.
/// Handles authentication via Google accounts with OpenID Connect.
/// </summary>
public interface IGoogleOAuthProvider
{
    /// <summary>Generates Google OAuth authorization URL with PKCE.</summary>
    string GetAuthorizationUrl(string state, string codeChallenge);

    /// <summary>Exchanges authorization code for tokens.</summary>
    Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(string code, string codeVerifier, CancellationToken cancellationToken = default);

    /// <summary>Gets user information from Google API.</summary>
    Task<OAuthUserInfoDto> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>Refreshes access token using refresh token.</summary>
    Task<OAuthTokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Microsoft OAuth 2.0 / Azure AD provider interface.
/// Handles authentication via Microsoft accounts and Azure AD.
/// </summary>
public interface IMicrosoftOAuthProvider
{
    /// <summary>Generates Microsoft OAuth authorization URL with PKCE.</summary>
    string GetAuthorizationUrl(string state, string codeChallenge, string? tenant = null);

    /// <summary>Exchanges authorization code for tokens.</summary>
    Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(string code, string codeVerifier, string? tenant = null, CancellationToken cancellationToken = default);

    /// <summary>Gets user information from Microsoft Graph API.</summary>
    Task<OAuthUserInfoDto> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>Refreshes access token using refresh token.</summary>
    Task<OAuthTokenResponseDto> RefreshTokenAsync(string refreshToken, string? tenant = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// GitHub OAuth 2.0 provider interface.
/// Handles authentication via GitHub accounts.
/// </summary>
public interface IGitHubOAuthProvider
{
    /// <summary>Generates GitHub OAuth authorization URL.</summary>
    string GetAuthorizationUrl(string state);

    /// <summary>Exchanges authorization code for token.</summary>
    Task<OAuthTokenResponseDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Gets user information from GitHub API.</summary>
    Task<OAuthUserInfoDto> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>Gets user email from GitHub API (separate endpoint).</summary>
    Task<string?> GetUserEmailAsync(string accessToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic OAuth token response DTO (consistent across providers).
/// </summary>
public class OAuthTokenResponseDto
{
    /// <summary>The access token for API requests.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Type of access token (usually "Bearer").</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>Lifetime of the access token in seconds.</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Refresh token for obtaining new access tokens (if provided by provider).</summary>
    public string? RefreshToken { get; set; }

    /// <summary>Space-delimited list of scopes granted.</summary>
    public string? Scope { get; set; }

    /// <summary>ID token for OpenID Connect (if provider supports it).</summary>
    public string? IdToken { get; set; }

    /// <summary>Additional provider-specific data.</summary>
    public Dictionary<string, string> AdditionalParameters { get; set; } = new();

    /// <summary>Calculated expiration time.</summary>
    public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);
}

/// <summary>
/// Standardized OAuth user information DTO (normalized across providers).
/// </summary>
public class OAuthUserInfoDto
{
    /// <summary>Unique identifier at the provider (sub in OpenID Connect).</summary>
    public string ProviderId { get; set; } = string.Empty;

    /// <summary>User's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>User's display name or full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL to user's profile picture.</summary>
    public string? PictureUrl { get; set; }

    /// <summary>User's given name (first name).</summary>
    public string? GivenName { get; set; }

    /// <summary>User's family name (last name).</summary>
    public string? FamilyName { get; set; }

    /// <summary>Provider name (google, microsoft, github).</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Whether email is verified at the provider.</summary>
    public bool EmailVerified { get; set; }

    /// <summary>Locale/language preference.</summary>
    public string? Locale { get; set; }

    /// <summary>Additional provider-specific claims.</summary>
    public Dictionary<string, object> Claims { get; set; } = new();
}
