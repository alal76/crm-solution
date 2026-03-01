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
/// Notification preference management endpoints.
/// Implements TODO-PORTAL-07.
/// </summary>
[ApiController]
[Route("api/users/{userId:int}/notification-preferences")]
[Authorize]
public class NotificationPreferencesController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<NotificationPreferencesController> _logger;

    public NotificationPreferencesController(ICrmDbContext db, ILogger<NotificationPreferencesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private bool CanAccessUser(int userId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) && (id == userId || User.IsInRole("Admin"));
    }

    /// <summary>Returns all notification preferences for the user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationPreference>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int userId, CancellationToken ct)
    {
        if (!CanAccessUser(userId)) return Forbid();

        var prefs = await _db.NotificationPreferences
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderBy(p => p.EntityType)
            .ThenBy(p => p.EventType)
            .ToListAsync(ct);

        return Ok(prefs);
    }

    /// <summary>Bulk upserts notification preferences for the user.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdate(
        int userId,
        [FromBody] List<NotificationPreferenceDto> preferences,
        CancellationToken ct)
    {
        if (!CanAccessUser(userId)) return Forbid();

        foreach (var dto in preferences)
        {
            var existing = await _db.NotificationPreferences
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.EntityType == dto.EntityType &&
                    p.EventType == dto.EventType &&
                    p.Channel == dto.Channel &&
                    !p.IsDeleted, ct);

            if (existing is null)
            {
                _db.NotificationPreferences.Add(new NotificationPreference
                {
                    UserId = userId,
                    EntityType = dto.EntityType,
                    EventType = dto.EventType,
                    Channel = dto.Channel,
                    IsEnabled = dto.IsEnabled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.IsEnabled = dto.IsEnabled;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Notification preferences updated for user {UserId}", userId);
        return Ok(new { message = "Preferences saved", count = preferences.Count });
    }

    /// <summary>Resets all notification preferences to defaults for the user.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetAll(int userId, CancellationToken ct)
    {
        if (!CanAccessUser(userId)) return Forbid();

        var prefs = await _db.NotificationPreferences
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync(ct);

        foreach (var pref in prefs)
        {
            pref.IsDeleted = true;
            pref.UpdatedAt = DateTime.UtcNow;
        }

        if (prefs.Count > 0)
            await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>Returns a single preference by entity type, event type, and channel.</summary>
    [HttpGet("{entityType}/{eventType}/{channel}")]
    [ProducesResponseType(typeof(NotificationPreference), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSingle(
        int userId, string entityType, string eventType, NotificationChannel channel, CancellationToken ct)
    {
        if (!CanAccessUser(userId)) return Forbid();

        var pref = await _db.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.EntityType == entityType &&
                p.EventType == eventType &&
                p.Channel == channel &&
                !p.IsDeleted, ct);

        return pref is null ? NotFound() : Ok(pref);
    }
}

/// <summary>DTO for notification preference update.</summary>
public class NotificationPreferenceDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public bool IsEnabled { get; set; } = true;
}
