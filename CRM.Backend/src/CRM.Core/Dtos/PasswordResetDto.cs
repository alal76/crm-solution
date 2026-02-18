// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for initiating a password reset request
/// User provides their email address to receive reset token
/// </summary>
public class CreatePasswordResetDto
{
    /// <summary>
    /// Email address of the user requesting password reset
    /// </summary>
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// DTO for confirming password reset with token
/// User provides reset token and new password
/// </summary>
public class ConfirmPasswordResetDto
{
    /// <summary>
    /// Reset token received via email
    /// </summary>
    [Required(ErrorMessage = "Reset token is required")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password (must meet password policy requirements)
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match NewPassword)
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("NewPassword", ErrorMessage = "Password and confirmation password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO for admin-initiated password reset
/// Admin can set a new password for a user directly
/// </summary>
public class AdminPasswordResetDto
{
    /// <summary>
    /// User ID whose password is being reset
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid user ID")]
    public int UserId { get; set; }

    /// <summary>
    /// New password set by administrator
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Flag to require user to change password on next login
    /// </summary>
    public bool RequireChangeOnNextLogin { get; set; } = true;

    /// <summary>
    /// Optional: Reason for admin password reset
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for password reset response
/// Returns confirmation details after successful reset
/// </summary>
public class PasswordResetResponseDto
{
    /// <summary>
    /// Indicates if password reset was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User-friendly message about the password reset operation
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email address that was reset (partially masked for security)
    /// </summary>
    public string? MaskedEmail { get; set; }

    /// <summary>
    /// Timestamp of the password reset operation
    /// </summary>
    public DateTime ResetAt { get; set; }
}

// ============================================================================
// DEPRECATED: Use CreatePasswordResetDto instead
// Kept for backward compatibility
// ============================================================================

/// <summary>
/// [DEPRECATED] Use ConfirmPasswordResetDto instead
/// DTO for password reset confirmation
/// </summary>
[Obsolete("Use ConfirmPasswordResetDto instead", false)]
public class PasswordResetConfirm
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// [DEPRECATED] Use CreatePasswordResetDto instead
/// DTO for password reset request
/// </summary>
[Obsolete("Use CreatePasswordResetDto instead", false)]
public class PasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// [DEPRECATED] Use AdminPasswordResetDto instead
/// DTO for admin password reset request
/// </summary>
[Obsolete("Use AdminPasswordResetDto instead", false)]
public class AdminPasswordResetRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
