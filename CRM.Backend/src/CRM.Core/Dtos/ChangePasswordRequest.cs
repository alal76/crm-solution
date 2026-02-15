// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
