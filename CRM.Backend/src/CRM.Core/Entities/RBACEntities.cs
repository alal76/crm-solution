// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

namespace CRM.Core.Entities;

/// <summary>
/// Role entity for RBAC system.
/// Defines a role that can be assigned to users.
///
/// HIERARCHY LEVELS:
/// 0 = SystemAdmin (highest privilege)
/// 1 = Admin
/// 2 = Manager
/// 3 = User
/// 4 = Guest (lowest privilege)
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Unique name of the role
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Hierarchy level (0-4, lower is higher privilege)
    /// </summary>
    public int HierarchyLevel { get; set; } = 3;

    /// <summary>
    /// Whether this is a system-defined role (can't be deleted)
    /// </summary>
    public bool IsSystemDefined { get; set; } = false;

    /// <summary>
    /// Whether this role is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Permissions assigned to this role
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Users assigned this role
    /// </summary>
    public virtual ICollection<UserRoleAssignment> UserRoles { get; set; } = new List<UserRoleAssignment>();
}

/// <summary>
/// Permission entity for RBAC system.
/// Represents a granular operational permission.
///
/// NAMING CONVENTION:
/// {Module}.{Action}
/// Examples:
/// - Accounts.Create, Accounts.Read, Accounts.Update, Accounts.Delete
/// - Contacts.Export
/// - Reports.Execute
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Unique permission name (e.g., "Accounts.Create")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Module this permission belongs to (e.g., "Accounts", "Contacts")
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Category within module (e.g., "Create", "Modify", "Delete")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a system-defined permission (can't be deleted)
    /// </summary>
    public bool IsSystemDefined { get; set; } = false;

    /// <summary>
    /// Whether this permission is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Roles that have this permission
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Junction table for Role-Permission many-to-many relationship.
/// Tracks which permissions are assigned to which roles.
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// Foreign key to Role
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public virtual Role? Role { get; set; }

    /// <summary>
    /// Foreign key to Permission
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Navigation property to Permission
    /// </summary>
    public virtual Permission? Permission { get; set; }

    /// <summary>
    /// When this permission was assigned to the role
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Junction table for User-Role many-to-many relationship.
/// Tracks which roles are assigned to which users with temporal validity.
/// 
/// A user can have multiple roles, each with optional effective date ranges.
/// Example: User is "Manager" from 2026-02-01 to 2026-06-30, then becomes "User".
/// 
/// NAMING NOTE: Class is renamed from "UserRole" to "UserRoleAssignment" to avoid
/// ambiguity with the UserRole enum in User.cs. Both are valid RBAC concepts:
/// - UserRole (enum): predefined role types (Admin, Manager, Sales, etc.)
/// - UserRoleAssignment (class): temporal assignment of a role to a specific user
/// </summary>
public class UserRoleAssignment : BaseEntity
{
    /// <summary>
    /// Foreign key to User
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Foreign key to Role
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public virtual Role? Role { get; set; }

    /// <summary>
    /// When this role becomes effective for the user
    /// </summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this role expires for the user (optional, null means indefinite)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this role assignment is currently active
    /// </summary>
    public bool IsActive => DateTime.UtcNow >= EffectiveFrom && (EffectiveTo == null || DateTime.UtcNow < EffectiveTo);

    /// <summary>
    /// When this assignment was created
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional notes about why this role was assigned
    /// </summary>
    public string? Notes { get; set; }
}
