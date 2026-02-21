// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for Role-Based Access Control (RBAC) operations.
/// Manages roles, permissions, and their assignments to users and groups.
///
/// HEXAGONAL ARCHITECTURE:
/// - Port: Defines contract for RBAC operations
/// - Accessed by: AdminDashboardService, Controllers, Middleware
/// - Depends on: IPermissionCacheService, ICrmDbContext
/// </summary>
public interface IRBACService
{
    #region Permission Checks

    /// <summary>
    /// Check if a user has a specific permission.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissionName">Permission name (e.g., "Accounts.Create")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has permission</returns>
    Task<bool> CheckPermissionAsync(int userId, string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all permissions for a user (including role and group permissions).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Set of permission names</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user has any of the specified permissions (OR logic).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissions">List of permission names</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has at least one permission</returns>
    Task<bool> CheckAnyPermissionAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user has all specified permissions (AND logic).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissions">List of permission names</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has all permissions</returns>
    Task<bool> CheckAllPermissionsAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    #endregion

    #region Role Management

    /// <summary>
    /// Get all roles in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of role DTOs</returns>
    Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific role by ID.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Role DTO or null if not found</returns>
    Task<RoleDto?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a role by name.
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Role DTO or null if not found</returns>
    Task<RoleDto?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new role.
    /// </summary>
    /// <param name="createRoleDto">Role creation DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created role DTO</returns>
    Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="updateRoleDto">Role update DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated role DTO</returns>
    Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a role (soft delete).
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);

    #endregion

    #region Permission Management

    /// <summary>
    /// Get all available permissions in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get permissions grouped by module.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of module name to permissions</returns>
    Task<IDictionary<string, IEnumerable<PermissionDto>>> GetPermissionsByModuleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific permission by ID.
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Permission DTO or null if not found</returns>
    Task<PermissionDto?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a custom permission.
    /// </summary>
    /// <param name="createPermissionDto">Permission creation DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created permission DTO</returns>
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a permission.
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default);

    #endregion

    #region Role-Permission Assignment

    /// <summary>
    /// Assign a permission to a role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment DTO</returns>
    Task<RolePermissionDto> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a permission from a role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign multiple permissions to a role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionIds">List of permission IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of assignment DTOs</returns>
    Task<IEnumerable<RolePermissionDto>> BulkAssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all permissions assigned to a role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);

    #endregion

    #region User-Role Assignment

    /// <summary>
    /// Assign a role to a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="effectiveFrom">When the role becomes effective</param>
    /// <param name="effectiveTo">When the role expires (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User role DTO</returns>
    Task<UserRoleDto> AssignRoleToUserAsync(int userId, int roleId, DateTime? effectiveFrom = null, DateTime? effectiveTo = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a role from a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all roles assigned to a user (including effective roles).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of role DTOs</returns>
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk assign roles to multiple users.
    /// </summary>
    /// <param name="userIds">List of user IDs</param>
    /// <param name="roleIds">List of role IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task BulkAssignRolesToUsersAsync(IEnumerable<int> userIds, IEnumerable<int> roleIds, CancellationToken cancellationToken = default);

    #endregion

    #region Role Hierarchy

    /// <summary>
    /// Verify if Role A has higher privilege than Role B.
    /// Hierarchy: SystemAdmin > Admin > Manager > User > Guest
    /// </summary>
    /// <param name="roleIdA">First role ID</param>
    /// <param name="roleIdB">Second role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if Role A has higher privilege</returns>
    Task<bool> IsRoleHigherInHierarchyAsync(int roleIdA, int roleIdB, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the role hierarchy as a tree structure.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Role hierarchy DTO</returns>
    Task<RoleHierarchyDto> GetRoleHierarchyAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Cache Management

    /// <summary>
    /// Invalidate permission cache for a user.
    /// Called when user roles or permissions change.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task InvalidateUserPermissionCacheAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all permission caches.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion</returns>
    Task InvalidateAllPermissionCachesAsync(CancellationToken cancellationToken = default);

    #endregion
}
