// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for role management.
/// Provides endpoints for CRUD operations on roles and role-permission assignments.
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRBACService _rbacService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRBACService rbacService, ILogger<RolesController> logger)
    {
        _rbacService = rbacService;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _rbacService.GetAllRolesAsync(cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return StatusCode(500, new { message = "Error retrieving roles" });
        }
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    [HttpGet("{roleId}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(int roleId, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _rbacService.GetRoleByIdAsync(roleId, cancellationToken);
            if (role == null)
                return NotFound();

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting role {roleId}");
            return StatusCode(500, new { message = "Error retrieving role" });
        }
    }

    /// <summary>
    /// Get a role by name
    /// </summary>
    [HttpGet("name/{roleName}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByName(string roleName, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _rbacService.GetRoleByNameAsync(roleName, cancellationToken);
            if (role == null)
                return NotFound();

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting role '{roleName}'");
            return StatusCode(500, new { message = "Error retrieving role" });
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _rbacService.CreateRoleAsync(createRoleDto, cancellationToken);
            return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { message = "Error creating role" });
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{roleId}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(int roleId, [FromBody] UpdateRoleDto updateRoleDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _rbacService.UpdateRoleAsync(roleId, updateRoleDto, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating role {roleId}");
            return StatusCode(500, new { message = "Error updating role" });
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{roleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(int roleId, CancellationToken cancellationToken)
    {
        try
        {
            await _rbacService.DeleteRoleAsync(roleId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting role {roleId}");
            return StatusCode(500, new { message = "Error deleting role" });
        }
    }

    /// <summary>
    /// Get all permissions for a role
    /// </summary>
    [HttpGet("{roleId}/permissions")]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolePermissions(int roleId, CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _rbacService.GetRolePermissionsAsync(roleId, cancellationToken);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting permissions for role {roleId}");
            return StatusCode(500, new { message = "Error retrieving permissions" });
        }
    }

    /// <summary>
    /// Assign a permission to a role
    /// </summary>
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [ProducesResponseType(typeof(RolePermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RolePermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignPermissionToRole(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _rbacService.AssignPermissionToRoleAsync(roleId, permissionId, cancellationToken);
            // Service returns existing if already assigned - return 200 OK in that case
            return CreatedAtAction(nameof(GetRolePermissions), new { roleId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate") == true
                                          || ex.InnerException?.Message.Contains("unique") == true)
        {
            return Conflict(new { message = "Permission is already assigned to this role" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already assigned"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);
            return StatusCode(500, new { message = "Error assigning permission", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a permission from a role
    /// </summary>
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        try
        {
            await _rbacService.RemovePermissionFromRoleAsync(roleId, permissionId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing permission {permissionId} from role {roleId}");
            return StatusCode(500, new { message = "Error removing permission" });
        }
    }

    /// <summary>
    /// Bulk assign permissions to a role
    /// </summary>
    [HttpPost("{roleId}/permissions/bulk")]
    [ProducesResponseType(typeof(IEnumerable<RolePermissionDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> BulkAssignPermissions(int roleId, [FromBody] int[] permissionIds, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _rbacService.BulkAssignPermissionsAsync(roleId, permissionIds, cancellationToken);
            return Created(string.Empty, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error bulk assigning permissions to role {roleId}");
            return StatusCode(500, new { message = "Error bulk assigning permissions" });
        }
    }

    /// <summary>
    /// Get role hierarchy
    /// </summary>
    [HttpGet("hierarchy")]
    [ProducesResponseType(typeof(RoleHierarchyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleHierarchy(CancellationToken cancellationToken)
    {
        try
        {
            var hierarchy = await _rbacService.GetRoleHierarchyAsync(cancellationToken);
            return Ok(hierarchy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role hierarchy");
            return StatusCode(500, new { message = "Error retrieving hierarchy" });
        }
    }

    /// <summary>
    /// Check if role A is higher in hierarchy than role B
    /// </summary>
    [HttpGet("hierarchy/compare")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsRoleHigherInHierarchy(int roleIdA, int roleIdB, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _rbacService.IsRoleHigherInHierarchyAsync(roleIdA, roleIdB, cancellationToken);
            return Ok(new { isHigher = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing role hierarchy");
            return StatusCode(500, new { message = "Error comparing roles" });
        }
    }
}
