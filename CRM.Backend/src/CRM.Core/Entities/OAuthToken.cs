// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// OAuth provider tokens for social login
/// </summary>
public class OAuthToken : BaseEntity
{
    public int UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // Google, GitHub, Microsoft, etc.
    public string ProviderUserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public virtual User? User { get; set; }
}
