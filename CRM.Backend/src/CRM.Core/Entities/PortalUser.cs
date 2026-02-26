// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Customer Portal User entity — represents a customer / contact who has
/// self-service portal access.
/// </summary>
public class PortalUser : BaseEntity
{
    /// <summary>Login email address (unique, required)</summary>
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt password hash</summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Optional link to CRM Contact record</summary>
    public int? ContactId { get; set; }

    /// <summary>Optional link to CRM Account / Customer record</summary>
    public int? AccountId { get; set; }

    /// <summary>Whether the portal account is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether the email address has been verified</summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>One-time token sent in verification email</summary>
    [MaxLength(100)]
    public string? EmailVerificationToken { get; set; }

    /// <summary>Timestamp when email was verified</summary>
    public DateTime? EmailVerifiedAt { get; set; }

    /// <summary>One-time token for password reset</summary>
    [MaxLength(100)]
    public string? PasswordResetToken { get; set; }

    /// <summary>Expiry for the password reset token</summary>
    public DateTime? PasswordResetExpiry { get; set; }

    /// <summary>Timestamp of the most recent successful login</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Display name shown in the portal</summary>
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    // ── Navigation ---------------------------------------------------------
    public virtual CRM.Core.Models.Contact? Contact { get; set; }
    public virtual Account? Account { get; set; }
}
