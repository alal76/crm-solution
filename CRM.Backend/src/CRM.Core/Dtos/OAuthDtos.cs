// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
}
