// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for Role-Based Access Control (RBAC) operations.
/// Manages roles, permissions, and their assignments to users.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IRBACService (port)
/// - Uses ICrmDbContext (repository)
/// - Uses IPermissionCacheService (caching)
/// </summary>
public class RBACService : IRBACService
{
    private readonly ICrmDbContext _context;
    private readonly IPermissionCacheService _cacheService;
    private readonly ILogger<RBACService> _logger;
    private const string RoleNotFoundMessage = "Role {0} not found";
    private const string PermissionNotFoundMessage = "Permission {0} not found";
    private const string UserNotFoundMessage = "User {0} not found";

    // Predefined role hierarchy
    private static readonly Dictionary<string, int> PREDEFINED_ROLES = new()
    {
        { "SystemAdmin", 0 },
        { "Admin", 1 },
        { "Manager", 2 },
        { "User", 3 },
        { "Guest", 4 }
    };

    public RBACService(
        ICrmDbContext context,
        IPermissionCacheService cacheService,
        ILogger<RBACService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    #region Permission Checks

    public async Task<bool> CheckPermissionAsync(int userId, string permissionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
            return permissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking permission '{permissionName}' for user {userId}");
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try cache first
            var cached = await _cacheService.GetUserPermissionsFromCacheAsync(userId, cancellationToken);
            if (cached.Count > 0)
            {
                return cached;
            }

            // Load from database if not cached
            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Get user with active roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning(string.Format(UserNotFoundMessage, userId));
                return permissions;
            }

            // Collect all permissions from active roles
            foreach (var userRole in user.UserRoles)
            {
                if (!userRole.IsActive || userRole.Role == null || !userRole.Role.IsActive)
                    continue;

                foreach (var rolePermission in userRole.Role.RolePermissions)
                {
                    if (rolePermission.Permission != null && rolePermission.Permission.IsActive)
                    {
                        permissions.Add(rolePermission.Permission.Name);
                    }
                }
            }

            // Cache the permissions
            await _cacheService.SetUserPermissionsInCacheAsync(userId, permissions, cancellationToken: cancellationToken);

            _logger.LogDebug($"Loaded {permissions.Count} permissions for user {userId}");
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting permissions for user {userId}");
            return new HashSet<string>();
        }
    }

    public async Task<bool> CheckAnyPermissionAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
            return permissions.Any(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking any permission for user {userId}");
            return false;
        }
    }

    public async Task<bool> CheckAllPermissionsAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
            return permissions.All(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking all permissions for user {userId}");
            return false;
        }
    }

    #endregion

    #region Role Management

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.Roles
                .Where(r => !r.IsDeleted)
                .Include(r => r.RolePermissions)
                .Include(r => r.UserRoles)
                .ToListAsync(cancellationToken);

            return roles.Select(MapRoleToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            throw;
        }
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.Id == roleId && !r.IsDeleted)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(cancellationToken);

            return role != null ? MapRoleToDto(role) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting role {roleId}");
            throw;
        }
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.Name == roleName && !r.IsDeleted)
                .Include(r => r.RolePermissions)
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(cancellationToken);

            return role != null ? MapRoleToDto(role) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting role '{roleName}'");
            throw;
        }
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate unique name
            var existing = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == createRoleDto.Name && !r.IsDeleted, cancellationToken);

            if (existing != null)
                throw new InvalidOperationException($"Role '{createRoleDto.Name}' already exists");

            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                HierarchyLevel = createRoleDto.HierarchyLevel,
                IsActive = true,
                IsSystemDefined = false
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Created role '{role.Name}' with ID {role.Id}");
            return MapRoleToDto(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            throw;
        }
    }

    public async Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

            if (role == null)
                throw new KeyNotFoundException(string.Format(RoleNotFoundMessage, roleId));

            if (role.IsSystemDefined)
                throw new InvalidOperationException("Cannot modify system-defined roles");

            // Update allowed fields
            if (!string.IsNullOrWhiteSpace(updateRoleDto.Name))
                role.Name = updateRoleDto.Name;

            if (!string.IsNullOrWhiteSpace(updateRoleDto.Description))
                role.Description = updateRoleDto.Description;

            if (updateRoleDto.HierarchyLevel.HasValue)
                role.HierarchyLevel = updateRoleDto.HierarchyLevel.Value;

            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate permission caches for all users with this role
            await InvalidateRoleCacheAsync(roleId, cancellationToken);

            _logger.LogInformation($"Updated role {roleId}");
            return MapRoleToDto(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating role {roleId}");
            throw;
        }
    }

    public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

            if (role == null)
                throw new KeyNotFoundException(string.Format(RoleNotFoundMessage, roleId));

            if (role.IsSystemDefined)
                throw new InvalidOperationException("Cannot delete system-defined roles");

            // Soft delete
            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate caches
            await InvalidateRoleCacheAsync(roleId, cancellationToken);

            _logger.LogInformation($"Deleted role {roleId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting role {roleId}");
            throw;
        }
    }

    #endregion

    #region Permission Management

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => !p.IsDeleted)
                .ToListAsync(cancellationToken);

            return permissions.Select(MapPermissionToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            throw;
        }
    }

    public async Task<IDictionary<string, IEnumerable<PermissionDto>>> GetPermissionsByModuleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => !p.IsDeleted)
                .ToListAsync(cancellationToken);

            return permissions
                .GroupBy(p => p.Module)
                .ToDictionary(
                    g => g.Key,
                    g => (IEnumerable<PermissionDto>)g.Select(MapPermissionToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions by module");
            throw;
        }
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == permissionId && !p.IsDeleted, cancellationToken);

            return permission != null ? MapPermissionToDto(permission) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting permission {permissionId}");
            throw;
        }
    }

    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate unique name
            var existing = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == createPermissionDto.Name && !p.IsDeleted, cancellationToken);

            if (existing != null)
                throw new InvalidOperationException($"Permission '{createPermissionDto.Name}' already exists");

            var permission = new Permission
            {
                Name = createPermissionDto.Name,
                DisplayName = createPermissionDto.DisplayName,
                Module = createPermissionDto.Module,
                Category = createPermissionDto.Category,
                Description = createPermissionDto.Description,
                IsActive = true,
                IsSystemDefined = false
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Created permission '{permission.Name}' with ID {permission.Id}");
            return MapPermissionToDto(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission");
            throw;
        }
    }

    public async Task DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == permissionId && !p.IsDeleted, cancellationToken);

            if (permission == null)
                throw new KeyNotFoundException(string.Format(PermissionNotFoundMessage, permissionId));

            if (permission.IsSystemDefined)
                throw new InvalidOperationException("Cannot delete system-defined permissions");

            // Soft delete
            permission.IsDeleted = true;
            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate all permission caches
            await _cacheService.InvalidateAllAsync(cancellationToken);

            _logger.LogInformation($"Deleted permission {permissionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting permission {permissionId}");
            throw;
        }
    }

    #endregion

    #region Role-Permission Assignment

    public async Task<RolePermissionDto> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);
            if (role == null)
                throw new KeyNotFoundException(string.Format(RoleNotFoundMessage, roleId));

            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permissionId && !p.IsDeleted, cancellationToken);
            if (permission == null)
                throw new KeyNotFoundException(string.Format(PermissionNotFoundMessage, permissionId));

            // Check if already assigned
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted, cancellationToken);

            if (existing != null)
                return MapRolePermissionToDto(existing);

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                AssignedAt = DateTime.UtcNow
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate caches for users with this role
            await InvalidateRoleCacheAsync(roleId, cancellationToken);

            _logger.LogInformation($"Assigned permission {permissionId} to role {roleId}");
            return MapRolePermissionToDto(rolePermission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning permission {permissionId} to role {roleId}");
            throw;
        }
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted, cancellationToken);

            if (rolePermission == null)
                throw new KeyNotFoundException("Role-permission assignment not found");

            rolePermission.IsDeleted = true;
            rolePermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate caches
            await InvalidateRoleCacheAsync(roleId, cancellationToken);

            _logger.LogInformation($"Removed permission {permissionId} from role {roleId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing permission {permissionId} from role {roleId}");
            throw;
        }
    }

    public async Task<IEnumerable<RolePermissionDto>> BulkAssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);
            if (role == null)
                throw new KeyNotFoundException(string.Format(RoleNotFoundMessage, roleId));

            var results = new List<RolePermissionDto>();
            var permissionIdList = permissionIds.ToList();

            foreach (var permissionId in permissionIdList)
            {
                var result = await AssignPermissionToRoleAsync(roleId, permissionId, cancellationToken);
                results.Add(result);
            }

            // Invalidate caches once at the end
            await InvalidateRoleCacheAsync(roleId, cancellationToken);

            _logger.LogInformation($"Bulk assigned {permissionIdList.Count} permissions to role {roleId}");
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error bulk assigning permissions to role {roleId}");
            throw;
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!)
                .Where(p => !p.IsDeleted)
                .ToListAsync(cancellationToken);

            return permissions.Select(MapPermissionToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting permissions for role {roleId}");
            throw;
        }
    }

    #endregion

    #region User-Role Assignment

    public async Task<UserRoleDto> AssignRoleToUserAsync(int userId, int roleId, DateTime? effectiveFrom = null, DateTime? effectiveTo = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException(string.Format(UserNotFoundMessage, userId));

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);
            if (role == null)
                throw new KeyNotFoundException(string.Format(RoleNotFoundMessage, roleId));

            // Check if already assigned and active
            var existing = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);

            if (existing != null && existing.IsActive)
                return MapUserRoleAssignmentToDto(existing);

            var userRole = new UserRoleAssignment
            {
                UserId = userId,
                RoleId = roleId,
                EffectiveFrom = effectiveFrom ?? DateTime.UtcNow,
                EffectiveTo = effectiveTo,
                AssignedAt = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate user's permission cache
            await _cacheService.InvalidateUserCacheAsync(userId, cancellationToken);

            _logger.LogInformation($"Assigned role {roleId} to user {userId}");
            return MapUserRoleAssignmentToDto(userRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning role {roleId} to user {userId}");
            throw;
        }
    }

    public async Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, cancellationToken);

            if (userRole == null)
                throw new KeyNotFoundException("User-role assignment not found");

            userRole.IsDeleted = true;
            userRole.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.InvalidateUserCacheAsync(userId, cancellationToken);

            _logger.LogInformation($"Removed role {roleId} from user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing role {roleId} from user {userId}");
            throw;
        }
    }

    public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted && ur.Role != null && !ur.Role.IsDeleted)
                .Include(ur => ur.Role)
                .ThenInclude(r => r!.RolePermissions)
                .Select(ur => ur.Role!)
                .ToListAsync(cancellationToken);

            return roles.Select(MapRoleToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting roles for user {userId}");
            throw;
        }
    }

    public async Task BulkAssignRolesToUsersAsync(IEnumerable<int> userIds, IEnumerable<int> roleIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdList = userIds.ToList();
            var roleIdList = roleIds.ToList();

            foreach (var userId in userIdList)
            {
                foreach (var roleId in roleIdList)
                {
                    await AssignRoleToUserAsync(userId, roleId, cancellationToken: cancellationToken);
                }
            }

            // Invalidate all user caches
            await _cacheService.InvalidateMultipleUsersAsync(userIdList, cancellationToken);

            _logger.LogInformation($"Bulk assigned {roleIdList.Count} roles to {userIdList.Count} users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk assigning roles to users");
            throw;
        }
    }

    #endregion

    #region Role Hierarchy

    public async Task<bool> IsRoleHigherInHierarchyAsync(int roleIdA, int roleIdB, CancellationToken cancellationToken = default)
    {
        try
        {
            var roleA = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleIdA && !r.IsDeleted, cancellationToken);
            var roleB = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleIdB && !r.IsDeleted, cancellationToken);

            if (roleA == null || roleB == null)
                return false;

            // Lower hierarchy level = higher privilege
            return roleA.HierarchyLevel < roleB.HierarchyLevel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error comparing hierarchy for roles {roleIdA} and {roleIdB}");
            return false;
        }
    }

    public async Task<RoleHierarchyDto> GetRoleHierarchyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.Roles
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.HierarchyLevel)
                .ToListAsync(cancellationToken);

            // Build hierarchy tree (in this case, it's a flat list ordered by level)
            var hierarchy = new RoleHierarchyDto
            {
                Name = "System",
                Level = -1,
                Children = roles.Select(r => new RoleHierarchyDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Level = r.HierarchyLevel,
                    Children = new List<RoleHierarchyDto>()
                }).ToList()
            };

            return hierarchy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role hierarchy");
            throw;
        }
    }

    #endregion

    #region Cache Management

    public async Task InvalidateUserPermissionCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        await _cacheService.InvalidateUserCacheAsync(userId, cancellationToken);
    }

    public async Task InvalidateAllPermissionCachesAsync(CancellationToken cancellationToken = default)
    {
        await _cacheService.InvalidateAllAsync(cancellationToken);
    }

    #endregion

    #region Private Helpers

    private async Task InvalidateRoleCacheAsync(int roleId, CancellationToken cancellationToken)
    {
        // Get all users with this role and invalidate their caches
        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId && !ur.IsDeleted)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (userIds.Count > 0)
        {
            await _cacheService.InvalidateMultipleUsersAsync(userIds, cancellationToken);
        }
    }

    private RoleDto MapRoleToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            HierarchyLevel = role.HierarchyLevel,
            IsSystemDefined = role.IsSystemDefined,
            PermissionCount = role.RolePermissions.Count(rp => !rp.IsDeleted),
            UserCount = role.UserRoles.Count(ur => !ur.IsDeleted && ur.IsActive),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    private PermissionDto MapPermissionToDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            DisplayName = permission.DisplayName,
            Module = permission.Module,
            Category = permission.Category,
            Description = permission.Description,
            IsSystemDefined = permission.IsSystemDefined,
            RoleCount = permission.RolePermissions?.Count(rp => !rp.IsDeleted) ?? 0,
            CreatedAt = permission.CreatedAt
        };
    }

    private RolePermissionDto MapRolePermissionToDto(RolePermission rolePermission)
    {
        return new RolePermissionDto
        {
            Id = rolePermission.Id,
            RoleId = rolePermission.RoleId,
            PermissionId = rolePermission.PermissionId,
            Permission = rolePermission.Permission != null ? MapPermissionToDto(rolePermission.Permission) : null,
            AssignedAt = rolePermission.AssignedAt
        };
    }

    private UserRoleDto MapUserRoleAssignmentToDto(UserRoleAssignment userRoleAssignment)
    {
        return new UserRoleDto
        {
            Id = userRoleAssignment.Id,
            UserId = userRoleAssignment.UserId,
            RoleId = userRoleAssignment.RoleId,
            Role = userRoleAssignment.Role != null ? MapRoleToDto(userRoleAssignment.Role) : null,
            EffectiveFrom = userRoleAssignment.EffectiveFrom,
            EffectiveTo = userRoleAssignment.EffectiveTo,
            AssignedAt = userRoleAssignment.AssignedAt
        };
    }

    #endregion
}
