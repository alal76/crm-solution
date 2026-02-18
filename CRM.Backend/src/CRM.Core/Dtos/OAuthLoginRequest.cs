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
    public string Provider { get; set; } = string.Empty; // google, github, microsoft
    public string Token { get; set; } = string.Empty; // ID token or access token from provider
}
