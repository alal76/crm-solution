// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// Represents a request to change a user's password.
/// Requires verification of the current password before allowing the change.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// The user's current password for verification.
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 255 characters")]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// The new password to set.
    /// Must meet password complexity requirements configured in the system.
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Optional confirmation of the new password for client-side validation.
    /// </summary>
    [StringLength(255, ErrorMessage = "Password confirmation must not exceed 255 characters")]
    public string? ConfirmPassword { get; set; }
}
