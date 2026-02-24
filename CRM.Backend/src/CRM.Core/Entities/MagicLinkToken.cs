// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// One-time magic-link token used for passwordless authentication.
/// Expires in 15 minutes and is single-use only.
/// </summary>
public class MagicLinkToken
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Cryptographically-random URL-safe token.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Email address the magic link was sent to.</summary>
    public string Email { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
