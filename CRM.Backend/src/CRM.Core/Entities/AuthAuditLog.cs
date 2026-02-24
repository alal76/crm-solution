// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Audit log entry for authentication and security events.
/// </summary>
public class AuthAuditLog
{
    public int Id { get; set; }

    /// <summary>Nullable: some events (e.g., failed login with unknown email) have no associated user.</summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Action performed: Login, Logout, LoginFailed, PasswordChange,
    /// PasswordResetRequest, PasswordResetConfirm, TwoFactorEnabled,
    /// TwoFactorDisabled, TwoFactorVerified, OAuthLinked, OAuthUnlinked,
    /// MagicLinkRequested, MagicLinkVerified, SessionRevoked
    /// </summary>
    public string Action { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Success { get; set; }

    /// <summary>Reason for failure, if applicable.</summary>
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
