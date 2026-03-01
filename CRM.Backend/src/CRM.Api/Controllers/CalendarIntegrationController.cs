// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing calendar integrations with Google and Outlook.
/// Part of Marketing and Sales gap analysis implementation (G4).
/// </summary>
[ApiController]
[Route("api/calendar")]
[Authorize]
public class CalendarIntegrationController : CrmControllerBase
{
    private readonly ICalendarSyncService _calendarSyncService;
    private readonly ILogger<CalendarIntegrationController> _logger;

    public CalendarIntegrationController(
        ICalendarSyncService calendarSyncService,
        ILogger<CalendarIntegrationController> logger)
    {
        _calendarSyncService = calendarSyncService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region OAuth Connection Endpoints

    /// <summary>
    /// Get Google Calendar OAuth authorization URL.
    /// </summary>
    /// <returns>Authorization URL to redirect user</returns>
    [HttpGet("connect/google")]
    [ProducesResponseType(typeof(AuthUrlResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConnectGoogle()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var authUrl = await _calendarSyncService.GetGoogleAuthUrlAsync(userId);
        return Ok(new AuthUrlResponse { AuthorizationUrl = authUrl });
    }

    /// <summary>
    /// Handle Google OAuth callback.
    /// </summary>
    /// <param name="code">Authorization code from Google</param>
    /// <param name="state">State parameter (user ID)</param>
    /// <returns>Redirect to success/failure page</returns>
    [HttpGet("callback/google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            if (!int.TryParse(state, out var userId) || userId <= 0)
            {
                return BadRequest("Invalid state parameter");
            }

            await _calendarSyncService.HandleGoogleCallbackAsync(code, userId);

            // Redirect to frontend success page
            return Redirect("/settings/integrations?connected=google");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google OAuth callback failed");
            return Redirect($"/settings/integrations?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    /// <summary>
    /// Get Outlook Calendar OAuth authorization URL.
    /// </summary>
    /// <returns>Authorization URL to redirect user</returns>
    [HttpGet("connect/outlook")]
    [ProducesResponseType(typeof(AuthUrlResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConnectOutlook()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var authUrl = await _calendarSyncService.GetOutlookAuthUrlAsync(userId);
        return Ok(new AuthUrlResponse { AuthorizationUrl = authUrl });
    }

    /// <summary>
    /// Handle Outlook OAuth callback.
    /// </summary>
    /// <param name="code">Authorization code from Microsoft</param>
    /// <param name="state">State parameter (user ID)</param>
    /// <returns>Redirect to success/failure page</returns>
    [HttpGet("callback/outlook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> OutlookCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            if (!int.TryParse(state, out var userId) || userId <= 0)
            {
                return BadRequest("Invalid state parameter");
            }

            await _calendarSyncService.HandleOutlookCallbackAsync(code, userId);

            // Redirect to frontend success page
            return Redirect("/settings/integrations?connected=outlook");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Outlook OAuth callback failed");
            return Redirect($"/settings/integrations?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    #endregion

    #region Integration Management Endpoints

    /// <summary>
    /// Get all calendar integrations for the current user.
    /// </summary>
    /// <returns>List of calendar integrations</returns>
    [HttpGet("integrations")]
    [ProducesResponseType(typeof(IEnumerable<CalendarIntegrationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIntegrations()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var integrations = await _calendarSyncService.GetUserIntegrationsAsync(userId);
        var dtos = integrations.Select(i => new CalendarIntegrationDto
        {
            Id = i.Id,
            Provider = i.Provider.ToString(),
            ExternalEmail = i.ExternalEmail,
            CalendarName = i.CalendarName,
            SyncDirection = i.SyncDirection.ToString(),
            SyncIntervalMinutes = i.SyncIntervalMinutes,
            IsActive = i.IsActive,
            LastSyncAt = i.LastSyncAt,
            LastSyncStatus = i.LastSyncStatus.ToString(),
            LastSyncError = i.LastSyncError,
            NextSyncAt = i.NextSyncAt,
            TotalEventsSynced = i.TotalEventsSynced
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific calendar integration.
    /// </summary>
    /// <param name="provider">Provider name (Google or Outlook)</param>
    /// <returns>Calendar integration details</returns>
    [HttpGet("integrations/{provider}")]
    [ProducesResponseType(typeof(CalendarIntegrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIntegration(string provider)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<CalendarProvider>(provider, true, out var calendarProvider))
        {
            return BadRequest($"Invalid provider: {provider}");
        }

        var integration = await _calendarSyncService.GetIntegrationAsync(userId, calendarProvider);
        if (integration == null)
        {
            return NotFound();
        }

        return Ok(new CalendarIntegrationDto
        {
            Id = integration.Id,
            Provider = integration.Provider.ToString(),
            ExternalEmail = integration.ExternalEmail,
            CalendarName = integration.CalendarName,
            SyncDirection = integration.SyncDirection.ToString(),
            SyncIntervalMinutes = integration.SyncIntervalMinutes,
            IsActive = integration.IsActive,
            LastSyncAt = integration.LastSyncAt,
            LastSyncStatus = integration.LastSyncStatus.ToString(),
            LastSyncError = integration.LastSyncError,
            NextSyncAt = integration.NextSyncAt,
            TotalEventsSynced = integration.TotalEventsSynced
        });
    }

    /// <summary>
    /// Update calendar integration settings.
    /// </summary>
    /// <param name="id">Integration ID</param>
    /// <param name="dto">Updated settings</param>
    /// <returns>Updated integration</returns>
    [HttpPut("integrations/{id}")]
    [ProducesResponseType(typeof(CalendarIntegrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIntegration(int id, [FromBody] UpdateIntegrationDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        try
        {
            if (!Enum.TryParse<CalendarSyncDirection>(dto.SyncDirection, true, out var direction))
            {
                direction = CalendarSyncDirection.Bidirectional;
            }

            var integration = await _calendarSyncService.UpdateSettingsAsync(id, direction, dto.SyncIntervalMinutes);

            return Ok(new CalendarIntegrationDto
            {
                Id = integration.Id,
                Provider = integration.Provider.ToString(),
                ExternalEmail = integration.ExternalEmail,
                CalendarName = integration.CalendarName,
                SyncDirection = integration.SyncDirection.ToString(),
                SyncIntervalMinutes = integration.SyncIntervalMinutes,
                IsActive = integration.IsActive,
                LastSyncAt = integration.LastSyncAt,
                LastSyncStatus = integration.LastSyncStatus.ToString(),
                LastSyncError = integration.LastSyncError,
                NextSyncAt = integration.NextSyncAt,
                TotalEventsSynced = integration.TotalEventsSynced
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Disconnect a calendar integration.
    /// </summary>
    /// <param name="provider">Provider name (Google or Outlook)</param>
    /// <returns>Success status</returns>
    [HttpDelete("integrations/{provider}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disconnect(string provider)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<CalendarProvider>(provider, true, out var calendarProvider))
        {
            return BadRequest($"Invalid provider: {provider}");
        }

        var result = await _calendarSyncService.DisconnectAsync(userId, calendarProvider);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion

    #region Sync Endpoints

    /// <summary>
    /// Trigger manual sync for a calendar integration.
    /// </summary>
    /// <param name="provider">Provider name (Google or Outlook)</param>
    /// <returns>Sync result</returns>
    [HttpPost("sync/{provider}")]
    [ProducesResponseType(typeof(CalendarSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncNow(string provider)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<CalendarProvider>(provider, true, out var calendarProvider))
        {
            return BadRequest($"Invalid provider: {provider}");
        }

        try
        {
            var log = await _calendarSyncService.SyncNowAsync(userId, calendarProvider);

            return Ok(new CalendarSyncResultDto
            {
                Status = log.Status.ToString(),
                StartedAt = log.StartedAt,
                CompletedAt = log.CompletedAt,
                EventsCreated = log.EventsCreated,
                EventsUpdated = log.EventsUpdated,
                EventsDeleted = log.EventsDeleted,
                ErrorMessage = log.ErrorMessage
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"No active {provider} integration found");
        }
    }

    /// <summary>
    /// Get sync history for a calendar integration.
    /// </summary>
    /// <param name="provider">Provider name (Google or Outlook)</param>
    /// <param name="limit">Maximum number of logs to return</param>
    /// <returns>List of sync logs</returns>
    [HttpGet("sync/{provider}/history")]
    [ProducesResponseType(typeof(IEnumerable<CalendarSyncResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSyncHistory(string provider, [FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<CalendarProvider>(provider, true, out var calendarProvider))
        {
            return BadRequest($"Invalid provider: {provider}");
        }

        var integration = await _calendarSyncService.GetIntegrationAsync(userId, calendarProvider);
        if (integration == null)
        {
            return NotFound();
        }

        // Get sync logs from the integration's SyncLogs navigation property
        var logs = integration.SyncLogs
            .OrderByDescending(l => l.StartedAt)
            .Take(limit)
            .Select(l => new CalendarSyncResultDto
            {
                Status = l.Status.ToString(),
                StartedAt = l.StartedAt,
                CompletedAt = l.CompletedAt,
                EventsCreated = l.EventsCreated,
                EventsUpdated = l.EventsUpdated,
                EventsDeleted = l.EventsDeleted,
                ErrorMessage = l.ErrorMessage
            });

        return Ok(logs);
    }

    #endregion
}

#region DTOs

/// <summary>
/// Response containing OAuth authorization URL.
/// </summary>
public class AuthUrlResponse
{
    public string AuthorizationUrl { get; set; } = string.Empty;
}

/// <summary>
/// Calendar integration details DTO.
/// </summary>
public class CalendarIntegrationDto
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ExternalEmail { get; set; }
    public string? CalendarName { get; set; }
    public string SyncDirection { get; set; } = string.Empty;
    public int SyncIntervalMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string LastSyncStatus { get; set; } = string.Empty;
    public string? LastSyncError { get; set; }
    public DateTime? NextSyncAt { get; set; }
    public int TotalEventsSynced { get; set; }
}

/// <summary>
/// Update integration settings DTO.
/// </summary>
public class UpdateIntegrationDto
{
    public string SyncDirection { get; set; } = "Bidirectional";
    public int SyncIntervalMinutes { get; set; } = 15;
}

/// <summary>
/// Calendar sync result DTO.
/// </summary>
public class CalendarSyncResultDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int EventsCreated { get; set; }
    public int EventsUpdated { get; set; }
    public int EventsDeleted { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion
