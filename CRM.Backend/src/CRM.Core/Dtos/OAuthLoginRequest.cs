// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for OAuth login request
/// </summary>
public class OAuthLoginRequest
{
    public string Provider { get; set; } = string.Empty; // google, github, microsoft, okta, oidc
    public string Token { get; set; } = string.Empty; // ID token or access token from provider
    public string? ProviderUserId { get; set; } // Unique user ID at the provider
    public string? Email { get; set; } // User's email from provider
    public string? FirstName { get; set; } // User's first name from provider
    public string? LastName { get; set; } // User's last name from provider
}
