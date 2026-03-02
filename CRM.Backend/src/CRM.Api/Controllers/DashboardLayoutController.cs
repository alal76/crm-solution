// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Dashboard layout persistence endpoints.
/// Implements TODO-PORTAL-05.
/// </summary>
[ApiController]
[Route("api/users/{userId:int}/dashboard-layout")]
[Authorize]
public class DashboardLayoutController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<DashboardLayoutController> _logger;

    public DashboardLayoutController(ICrmDbContext db, ILogger<DashboardLayoutController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Returns all dashboard layouts for the specified user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDashboardLayout>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLayouts(int userId, CancellationToken ct)
    {
        var layouts = await _db.UserDashboardLayouts
            .AsNoTracking()
            .Where(l => l.UserId == userId && !l.IsDeleted)
            .OrderByDescending(l => l.IsDefault)
            .ThenByDescending(l => l.UpdatedAt)
            .ToListAsync(ct);

        return Ok(layouts);
    }

    /// <summary>Returns the default dashboard layout for the specified user.</summary>
    [HttpGet("default")]
    [ProducesResponseType(typeof(UserDashboardLayout), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault(int userId, CancellationToken ct)
    {
        var layout = await _db.UserDashboardLayouts
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.IsDefault && !l.IsDeleted, ct);

        return layout is null ? NotFound(new { message = "No default layout found" }) : Ok(layout);
    }

    /// <summary>Saves (upserts) the default dashboard layout for the specified user.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(UserDashboardLayout), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> SaveLayout(int userId, [FromBody] DashboardLayoutDto dto, CancellationToken ct)
    {
        // Only allow users to update their own layout (unless admin)
        var callerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if ((!int.TryParse(callerClaim, out var callerId) || callerId != userId) && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var existing = await _db.UserDashboardLayouts
            .FirstOrDefaultAsync(l => l.UserId == userId && l.IsDefault && !l.IsDeleted, ct);

        if (existing is null)
        {
            var newLayout = new UserDashboardLayout
            {
                UserId = userId,
                Name = dto.Name ?? "Default",
                LayoutJson = dto.LayoutJson,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.UserDashboardLayouts.Add(newLayout);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetDefault), new { userId }, newLayout);
        }

        existing.LayoutJson = dto.LayoutJson;
        existing.Name = dto.Name ?? existing.Name;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogDebug("Dashboard layout updated for user {UserId}", userId);
        return Ok(existing);
    }

    /// <summary>Resets the user's default layout to system defaults.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetLayout(int userId, CancellationToken ct)
    {
        var layouts = await _db.UserDashboardLayouts
            .Where(l => l.UserId == userId && !l.IsDeleted)
            .ToListAsync(ct);

        foreach (var layout in layouts)
        {
            layout.IsDeleted = true;
            layout.UpdatedAt = DateTime.UtcNow;
        }

        if (layouts.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return NoContent();
    }
}

/// <summary>DTO for saving/updating a dashboard layout.</summary>
public class DashboardLayoutDto
{
    public string? Name { get; set; }
    public string LayoutJson { get; set; } = "{}";
}
