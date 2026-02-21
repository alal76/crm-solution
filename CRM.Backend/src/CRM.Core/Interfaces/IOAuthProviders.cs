// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

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
