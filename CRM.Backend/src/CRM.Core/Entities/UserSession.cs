// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Represents an active user session for concurrent session limit enforcement.
/// </summary>
public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>A unique token identifying this session (typically the JWT jti or a hashed refresh token).</summary>
    public string SessionToken { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    /// <summary>Optional device identifier for device-aware session management.</summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// If true, this session is bound to its originating IP address and will be invalidated
    /// if requests come from a different IP. Feature flag for TODO-AUTH-015.
    /// </summary>
    public bool IpBindingEnabled { get; set; } = false;

    // Navigation property
    public User? User { get; set; }
}
