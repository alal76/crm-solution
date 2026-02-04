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
/// DTO for setting or resetting a password (first-time setup or expired password change)
/// </summary>
public class SetPasswordRequest
{
    /// <summary>
    /// Token from login response for password setup/change authentication
    /// </summary>
    [Required]
    public string PasswordSetupToken { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password
    /// </summary>
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response for password complexity requirements
/// </summary>
public class PasswordComplexityRequirements
{
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialChars { get; set; } = false;
}
