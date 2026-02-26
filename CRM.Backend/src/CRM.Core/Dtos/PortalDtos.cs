// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

// ── Auth DTOs ─────────────────────────────────────────────────────────────────

/// <summary>Login credentials for a portal user.</summary>
public class PortalLoginDto
{
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Registration payload for a new portal user.</summary>
public class PortalRegisterDto
{
    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    /// <summary>Optional access code when self-registration requires approval.</summary>
    public string? AccessCode { get; set; }
}

/// <summary>Successful portal login response containing the JWT token.</summary>
public class PortalTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int PortalUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

// ── User DTOs ─────────────────────────────────────────────────────────────────

/// <summary>Portal user data exposed to admins and the user themselves.</summary>
public class PortalUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int? ContactId { get; set; }
    public int? AccountId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Config DTOs ───────────────────────────────────────────────────────────────

/// <summary>Portal configuration read model.</summary>
public class PortalConfigDto
{
    public bool IsEnabled { get; set; }
    public bool AllowSelfRegistration { get; set; }
    public string? WelcomeMessage { get; set; }
    public string? SupportEmail { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? PortalTitle { get; set; }
    public string? AllowedDomains { get; set; }
}

/// <summary>Portal configuration update payload (all fields optional).</summary>
public class UpdatePortalConfigDto
{
    public bool? IsEnabled { get; set; }
    public bool? AllowSelfRegistration { get; set; }
    [MaxLength(500)]
    public string? WelcomeMessage { get; set; }
    [MaxLength(200)]
    public string? SupportEmail { get; set; }
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
    [MaxLength(20)]
    public string? PrimaryColor { get; set; }
    [MaxLength(100)]
    public string? PortalTitle { get; set; }
    [MaxLength(500)]
    public string? AllowedDomains { get; set; }
}

// ── Ticket DTOs ───────────────────────────────────────────────────────────────

/// <summary>Service-request ticket as seen by a portal user.</summary>
public class PortalTicketDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastCommentAt { get; set; }
}

/// <summary>Payload to create a new portal ticket.</summary>
public class PortalCreateTicketDto
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(10000)]
    public string? Description { get; set; }

    /// <summary>Optional priority: Low | Medium | High | Critical</summary>
    public string? Priority { get; set; }
}

// ── Comment DTOs ──────────────────────────────────────────────────────────────

/// <summary>A single comment / note on a portal ticket.</summary>
public class PortalCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public bool IsStaff { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Payload to add a comment to a portal ticket.</summary>
public class PortalAddCommentDto
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}

// ── Knowledge-Base DTOs ───────────────────────────────────────────────────────

/// <summary>Knowledge-base article summary exposed in the portal.</summary>
public class PortalKBArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
