// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Links a CRM user account to an external OAuth provider identity.
/// Supports multi-provider linking: google, microsoft, github, linkedin, apple.
/// </summary>
public class UserOAuthLink
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Provider identifier: google | microsoft | github | linkedin | apple</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>The unique user ID returned by the OAuth provider.</summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>Email address associated with the provider account (may differ from CRM user email).</summary>
    public string? ProviderEmail { get; set; }

    /// <summary>OAuth access token — may be null if only identity linking is required.</summary>
    public string? AccessToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
