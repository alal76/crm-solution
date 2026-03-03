// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

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
    public bool IsLocked { get; set; }
    public bool IsApiUser { get; set; }
    public string? ApiKeyPrefix { get; set; }
    public DateTime? ApiKeyCreatedAt { get; set; }
    public DateTime? ApiKeyLastUsedAt { get; set; }
    public DateTime? ApiKeyExpiresAt { get; set; }
    public string? ApiUserDescription { get; set; }
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

    // Security / account status
    /// <summary>Whether two-factor authentication is enabled (public status flag — does NOT expose the secret)</summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>When the user last changed their password</summary>
    public DateTime? PasswordLastChangedAt { get; set; }

    /// <summary>Whether the user must reset their password on next login</summary>
    public bool MustResetPassword { get; set; }

    /// <summary>Whether the user's email address has been verified</summary>
    public bool EmailVerified { get; set; }

    // Preferences
    /// <summary>User's theme preference (system, light, dark, high-contrast)</summary>
    public string? ThemePreference { get; set; }

    /// <summary>User's preferred language code (e.g., en, es, fr)</summary>
    public string? Language { get; set; }

    /// <summary>User's preferred IANA timezone (e.g., America/New_York)</summary>
    public string? Timezone { get; set; }

    /// <summary>User's preferred date format (e.g., MM/DD/YYYY)</summary>
    public string? DateFormat { get; set; }

    /// <summary>User's preferred time format (12h or 24h)</summary>
    public string? TimeFormat { get; set; }

    /// <summary>Default number of rows per page in tables</summary>
    public int? RowsPerPage { get; set; }

    /// <summary>Whether to receive email notifications</summary>
    public bool? EmailNotifications { get; set; }

    /// <summary>Whether to enable desktop/browser notifications</summary>
    public bool? DesktopNotifications { get; set; }

    /// <summary>Whether to use compact UI mode</summary>
    public bool? CompactMode { get; set; }
}

/// <summary>
/// DTO for creating a new user
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Optional username (if not provided, email is used as username)
    /// </summary>
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// User's email address (required, used as username if Username not provided)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name (required)
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name (required)
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's password (optional - if not provided, user will be prompted to set password on first login)
    /// </summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string? Password { get; set; }

    private int _roleId = 2;

    /// <summary>
    /// Role ID (default: 2 = Sales). Accepts both "roleId" and "role" from JSON.
    /// Values: 0=Admin, 1=Manager, 2=Sales, 3=Support, 4=Guest
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("roleId")]
    public int RoleId
    {
        get => _roleId;
        set => _roleId = value;
    }

    /// <summary>
    /// Alias for RoleId — accepts "role" from JSON (frontend compatibility).
    /// If both "role" and "roleId" are sent, the last one deserialized wins.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("role")]
    public int Role
    {
        get => _roleId;
        set => _roleId = value;
    }

    /// <summary>
    /// Department ID (optional)
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Primary Group ID (optional)
    /// </summary>
    public int? PrimaryGroupId { get; set; }
}
