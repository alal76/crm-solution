// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
namespace CRM.Core.Dtos;

/// <summary>
/// Record DTO for creating a new user.
/// Contains all required fields for user registration.
/// </summary>
public record CreateUserDto
{
    /// <summary>User's email address (required)</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>User's username (required)</summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>User's password (required)</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>User's first name (optional)</summary>
    public string? FirstName { get; init; }

    /// <summary>User's last name (optional)</summary>
    public string? LastName { get; init; }

    /// <summary>User's role (optional, defaults to standard user if not specified)</summary>
    public string? Role { get; init; }
}

/// <summary>
/// Record DTO for updating user information.
/// All fields are optional - only provided fields will be updated.
/// </summary>
public record UpdateUserRecordDto
{
    /// <summary>User's first name</summary>
    public string? FirstName { get; init; }

    /// <summary>User's last name</summary>
    public string? LastName { get; init; }

    /// <summary>User's email address</summary>
    public string? Email { get; init; }

    /// <summary>Whether the user account is active</summary>
    public bool? IsActive { get; init; }
}
