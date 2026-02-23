// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System;
using System.Collections.Generic;

namespace CRM.Core.Dtos
{
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

    /// <summary>
    /// DTO returned when initiating an OAuth flow — contains the provider's authorization URL.
    /// </summary>
    public class OAuthRedirectDto
    {
        /// <summary>
        /// The full OAuth authorization URL the client should redirect to.
        /// </summary>
        public string AuthorizationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Anti-CSRF state token to verify on callback.
        /// </summary>
        public string State { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO submitted by the client after the OAuth provider redirects back with an authorization code.
    /// </summary>
    public class OAuthCallbackDto
    {
        /// <summary>
        /// Authorization code returned by the OAuth provider.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Anti-CSRF state token for verification.
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// The redirect URI that was used for the OAuth flow (must match the one used to start the flow).
        /// </summary>
        public string? RedirectUri { get; set; }

        /// <summary>
        /// Optional return URL to redirect to after successful login.
        /// </summary>
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Optional user data sent by providers (e.g., Apple sends user info only on first sign-in).
        /// </summary>
        public string? UserData { get; set; }
    }

    /// <summary>
    /// DTO for WebAuthn registration completion request.
    /// Wraps the attestation response with a friendly credential name.
    /// </summary>
    public class WebAuthnRegistrationCompleteDto
    {
        /// <summary>
        /// Friendly name for this credential (e.g., "My Laptop", "YubiKey 5").
        /// </summary>
        public string CredentialName { get; set; } = string.Empty;

        /// <summary>
        /// Client attestation response from navigator.credentials.create().
        /// </summary>
        public WebAuthnAttestationResponseDto Attestation { get; set; } = new();
    }

    /// <summary>
    /// DTO for WebAuthn login initiation request.
    /// </summary>
    public class WebAuthnLoginInitiateDto
    {
        /// <summary>
        /// User email or username to generate authentication challenge for.
        /// Optional for discoverable credentials (resident keys).
        /// </summary>
        public string? Email { get; set; }
    }

    /// <summary>
    /// DTO for WebAuthn login completion request.
    /// Wraps the assertion response with the credential ID.
    /// </summary>
    public class WebAuthnLoginCompleteDto
    {
        /// <summary>
        /// User email for credential lookup.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Credential ID used for assertion.
        /// </summary>
        public string CredentialId { get; set; } = string.Empty;

        /// <summary>
        /// Client assertion response from navigator.credentials.get().
        /// </summary>
        public WebAuthnAssertionResponseDto Assertion { get; set; } = new();
    }
}
