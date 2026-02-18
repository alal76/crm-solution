// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for updating user information
/// </summary>
public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? Role { get; set; }
    public bool? IsActive { get; set; }
    public int? DepartmentId { get; set; }
    public int? UserProfileId { get; set; }
    public int? ContactId { get; set; }
    public int? PrimaryGroupId { get; set; }
}

/// <summary>
/// DTO for linking/unlinking user to contact
/// </summary>
public class LinkUserContactDto
{
    public int? ContactId { get; set; }
}
