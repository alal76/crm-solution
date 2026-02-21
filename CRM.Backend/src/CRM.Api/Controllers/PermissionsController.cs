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

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for permission management.
/// Provides endpoints for listing and creating permissions.
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IRBACService _rbacService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IRBACService rbacService, ILogger<PermissionsController> logger)
    {
        _rbacService = rbacService;
        _logger = logger;
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _rbacService.GetAllPermissionsAsync(cancellationToken);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            return StatusCode(500, new { message = "Error retrieving permissions" });
        }
    }

    /// <summary>
    /// Get a specific permission by ID
    /// </summary>
    [HttpGet("{permissionId}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissionById(int permissionId, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _rbacService.GetPermissionByIdAsync(permissionId, cancellationToken);
            if (permission == null)
                return NotFound();

            return Ok(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting permission {permissionId}");
            return StatusCode(500, new { message = "Error retrieving permission" });
        }
    }

    /// <summary>
    /// Get permissions grouped by module
    /// </summary>
    [HttpGet("by-module")]
    [ProducesResponseType(typeof(IDictionary<string, IEnumerable<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionsByModule(CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _rbacService.GetPermissionsByModuleAsync(cancellationToken);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions by module");
            return StatusCode(500, new { message = "Error retrieving permissions" });
        }
    }

    /// <summary>
    /// Create a new permission
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto createPermissionDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _rbacService.CreatePermissionAsync(createPermissionDto, cancellationToken);
            return CreatedAtAction(nameof(GetPermissionById), new { permissionId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission");
            return StatusCode(500, new { message = "Error creating permission" });
        }
    }

    /// <summary>
    /// Delete a permission
    /// </summary>
    [HttpDelete("{permissionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePermission(int permissionId, CancellationToken cancellationToken)
    {
        try
        {
            await _rbacService.DeletePermissionAsync(permissionId, cancellationToken);
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
            _logger.LogError(ex, $"Error deleting permission {permissionId}");
            return StatusCode(500, new { message = "Error deleting permission" });
        }
    }
}
