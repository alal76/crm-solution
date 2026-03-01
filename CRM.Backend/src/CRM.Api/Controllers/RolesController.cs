// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for role management.
/// Provides endpoints for CRUD operations on roles and role-permission assignments.
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : CrmControllerBase
{
    private readonly IRBACService _rbacService;

    public RolesController(IRBACService rbacService)
    {
        _rbacService = rbacService;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
                var roles = await _rbacService.GetAllRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    [HttpGet("{roleId}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(int roleId, CancellationToken cancellationToken)
    {
                var role = await _rbacService.GetRoleByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    /// <summary>
    /// Get a role by name
    /// </summary>
    [HttpGet("name/{roleName}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByName(string roleName, CancellationToken cancellationToken)
    {
                var role = await _rbacService.GetRoleByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            return NotFound();
        }

        return Ok(role);
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
            {
                return BadRequest(ModelState);
            }

            var result = await _rbacService.CreateRoleAsync(createRoleDto, cancellationToken);
            return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
    }

    /// <summary>
    /// Get all permissions for a role
    /// </summary>
    [HttpGet("{roleId}/permissions")]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolePermissions(int roleId, CancellationToken cancellationToken)
    {
                var permissions = await _rbacService.GetRolePermissionsAsync(roleId, cancellationToken);
        return Ok(permissions);
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
    }

    /// <summary>
    /// Get role hierarchy
    /// </summary>
    [HttpGet("hierarchy")]
    [ProducesResponseType(typeof(RoleHierarchyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleHierarchy(CancellationToken cancellationToken)
    {
                var hierarchy = await _rbacService.GetRoleHierarchyAsync(cancellationToken);
        return Ok(hierarchy);
    }

    /// <summary>
    /// Check if role A is higher in hierarchy than role B
    /// </summary>
    [HttpGet("hierarchy/compare")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsRoleHigherInHierarchy(int roleIdA, int roleIdB, CancellationToken cancellationToken)
    {
                var result = await _rbacService.IsRoleHigherInHierarchyAsync(roleIdA, roleIdB, cancellationToken);
        return Ok(new { isHigher = result });
    }
}
