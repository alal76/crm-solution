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

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for User with profile, department, and contact information
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? UserProfileId { get; set; }
    public string? UserProfileName { get; set; }
    public int? PrimaryGroupId { get; set; }
    public string? PrimaryGroupName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Custom header color for this user (hex format)
    /// </summary>
    public string? HeaderColor { get; set; }

    /// <summary>
    /// URL to user's profile photo
    /// </summary>
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// DTO for creating a new user
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// User's email address (required, used as username)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name (required)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name (required)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's password (optional - if not provided, user will be prompted to set password on first login)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Role ID (default: 2 = User)
    /// </summary>
    public int RoleId { get; set; } = 2;

    /// <summary>
    /// Department ID (optional)
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Primary Group ID (optional)
    /// </summary>
    public int? PrimaryGroupId { get; set; }
}
