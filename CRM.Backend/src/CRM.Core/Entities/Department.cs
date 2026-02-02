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

namespace CRM.Core.Entities;

/// <summary>
/// Department entity for organizing users and access control
/// </summary>
public class Department : BaseEntity
{
    /// <summary>Department name</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Department description</summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Department code e.g., "SALES", "SUPPORT"</summary>
    [MaxLength(20)]
    public string? DepartmentCode { get; set; }

    /// <summary>Whether department is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Parent department ID for hierarchical departments</summary>
    public int? ParentDepartmentId { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    public virtual Department? ParentDepartment { get; set; }
    public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
