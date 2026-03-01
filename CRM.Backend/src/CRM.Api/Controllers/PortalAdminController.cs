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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// CRM admin portal management endpoints.
/// Requires standard CRM JWT authentication (CRM staff only).
/// </summary>
[ApiController]
[Route("api/admin/portal")]
[Authorize]
public class PortalAdminController : CrmControllerBase
{
    private readonly IPortalAdminService _portalAdmin;

    public PortalAdminController(
        IPortalAdminService portalAdmin)
    {
        _portalAdmin = portalAdmin;
    }

    /// <summary>GET /api/admin/portal/config — get current portal configuration</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var config = await _portalAdmin.GetConfigAsync(ct);
        return Ok(config);
    }

    /// <summary>PUT /api/admin/portal/config — update portal configuration</summary>
    [HttpPut("config")]
    public async Task<IActionResult> UpdateConfig(
        [FromBody] UpdatePortalConfigDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var config = await _portalAdmin.UpdateConfigAsync(dto, ct);
        return Ok(config);
    }

    /// <summary>GET /api/admin/portal/users?page=1&pageSize=20</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetPortalUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _portalAdmin.GetPortalUsersAsync(page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>POST /api/admin/portal/users/{id}/activate</summary>
    [HttpPost("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id, CancellationToken ct)
    {
        var success = await _portalAdmin.ActivatePortalUserAsync(id, ct);
        if (!success)
        {
            return NotFound(new { message = $"Portal user {id} not found." });
        }

        return Ok(new { message = "Portal user activated." });
    }

    /// <summary>POST /api/admin/portal/users/{id}/deactivate</summary>
    [HttpPost("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id, CancellationToken ct)
    {
        var success = await _portalAdmin.DeactivatePortalUserAsync(id, ct);
        if (!success)
        {
            return NotFound(new { message = $"Portal user {id} not found." });
        }

        return Ok(new { message = "Portal user deactivated." });
    }
}
