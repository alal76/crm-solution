// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for Department creation and updates
/// </summary>
public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public int? ParentDepartmentId { get; set; }
}

/// <summary>
/// DTO for Department responses
/// </summary>
public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public bool IsActive { get; set; }
    public int? ParentDepartmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// DTO for Department with details
/// </summary>
public class DepartmentDetailDto : DepartmentDto
{
    public List<UserDto> Users { get; set; } = new();
    public List<UserProfileDto> Profiles { get; set; } = new();
}
