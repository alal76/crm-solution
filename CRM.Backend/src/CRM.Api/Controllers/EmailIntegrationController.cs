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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing email integrations and sync operations.
/// Part of Marketing and Sales gap analysis implementation (G5).
/// </summary>
[ApiController]
[Route("api/email")]
[Authorize]
public class EmailIntegrationController : CrmControllerBase
{
    private readonly IEmailSyncService _emailSyncService;

    public EmailIntegrationController(
        IEmailSyncService emailSyncService)
    {
        _emailSyncService = emailSyncService;
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get all email integrations for the current user.
    /// </summary>
    [HttpGet("integrations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIntegrations()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var integrations = await _emailSyncService.GetUserIntegrationsAsync(userId);
        var dtos = integrations.Select(i => new EmailIntegrationDto
        {
            Id = i.Id,
            Provider = i.Provider.ToString(),
            EmailAddress = i.EmailAddress,
            ImapServer = i.ImapServer,
            ImapPort = i.ImapPort,
            UseSsl = i.UseSsl,
            SyncIntervalMinutes = i.SyncIntervalMinutes,
            IsActive = i.IsActive,
            LastSyncAt = i.LastSyncAt,
            LastSyncStatus = i.LastSyncStatus.ToString(),
            LastSyncError = i.LastSyncError,
            NextSyncAt = i.NextSyncAt,
            TotalEmailsSynced = i.TotalEmailsSynced
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Create or update an email integration.
    /// </summary>
    [HttpPost("integrations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrUpdateIntegration([FromBody] EmailIntegrationCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<EmailProvider>(dto.Provider, true, out var provider))
        {
            return BadRequest($"Invalid provider: {dto.Provider}");
        }

        var integration = new EmailIntegration
        {
            UserId = userId,
            Provider = provider,
            EmailAddress = dto.EmailAddress,
            AccessToken = dto.AccessToken,
            RefreshToken = dto.RefreshToken,
            TokenExpiresAt = dto.TokenExpiresAt,
            ImapServer = dto.ImapServer,
            ImapPort = dto.ImapPort,
            ImapUsername = dto.ImapUsername,
            ImapPassword = dto.ImapPassword,
            UseSsl = dto.UseSsl,
            SyncIntervalMinutes = dto.SyncIntervalMinutes,
            IsActive = dto.IsActive
        };

        var result = await _emailSyncService.CreateOrUpdateIntegrationAsync(integration);

        return Ok(new EmailIntegrationDto
        {
            Id = result.Id,
            Provider = result.Provider.ToString(),
            EmailAddress = result.EmailAddress,
            ImapServer = result.ImapServer,
            ImapPort = result.ImapPort,
            UseSsl = result.UseSsl,
            SyncIntervalMinutes = result.SyncIntervalMinutes,
            IsActive = result.IsActive,
            LastSyncAt = result.LastSyncAt,
            LastSyncStatus = result.LastSyncStatus.ToString(),
            LastSyncError = result.LastSyncError,
            NextSyncAt = result.NextSyncAt,
            TotalEmailsSynced = result.TotalEmailsSynced
        });
    }

    /// <summary>
    /// Update integration settings.
    /// </summary>
    [HttpPut("integrations/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateIntegration(int id, [FromBody] EmailIntegrationUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var integration = new EmailIntegration
        {
            Id = id,
            UserId = userId,
            Provider = EmailProvider.Imap,
            EmailAddress = dto.EmailAddress ?? string.Empty,
            SyncIntervalMinutes = dto.SyncIntervalMinutes,
            IsActive = dto.IsActive
        };

        var result = await _emailSyncService.CreateOrUpdateIntegrationAsync(integration);

        return Ok(new EmailIntegrationDto
        {
            Id = result.Id,
            Provider = result.Provider.ToString(),
            EmailAddress = result.EmailAddress,
            ImapServer = result.ImapServer,
            ImapPort = result.ImapPort,
            UseSsl = result.UseSsl,
            SyncIntervalMinutes = result.SyncIntervalMinutes,
            IsActive = result.IsActive,
            LastSyncAt = result.LastSyncAt,
            LastSyncStatus = result.LastSyncStatus.ToString(),
            LastSyncError = result.LastSyncError,
            NextSyncAt = result.NextSyncAt,
            TotalEmailsSynced = result.TotalEmailsSynced
        });
    }

    /// <summary>
    /// Trigger sync for a specific integration.
    /// </summary>
    [HttpPost("integrations/{id}/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncNow(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        try
        {
            var log = await _emailSyncService.SyncNowAsync(userId, id);
            return Ok(new EmailSyncResultDto
            {
                Status = log.Status.ToString(),
                StartedAt = log.StartedAt,
                CompletedAt = log.CompletedAt,
                EmailsCreated = log.EmailsCreated,
                EmailsUpdated = log.EmailsUpdated,
                EmailsSkipped = log.EmailsSkipped,
                ErrorMessage = log.ErrorMessage
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Disconnect an email integration.
    /// </summary>
    [HttpDelete("integrations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disconnect(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var result = await _emailSyncService.DisconnectAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

#region DTOs

public class EmailIntegrationDto
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? ImapServer { get; set; }
    public int? ImapPort { get; set; }
    public bool UseSsl { get; set; }
    public int SyncIntervalMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string LastSyncStatus { get; set; } = string.Empty;
    public string? LastSyncError { get; set; }
    public DateTime? NextSyncAt { get; set; }
    public int TotalEmailsSynced { get; set; }
}

public class EmailIntegrationCreateDto
{
    public string Provider { get; set; } = "Imap";
    public string EmailAddress { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public string? ImapServer { get; set; }
    public int? ImapPort { get; set; }
    public string? ImapUsername { get; set; }
    public string? ImapPassword { get; set; }
    public bool UseSsl { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}

public class EmailIntegrationUpdateDto
{
    public string? EmailAddress { get; set; }
    public int SyncIntervalMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}

public class EmailSyncResultDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int EmailsCreated { get; set; }
    public int EmailsUpdated { get; set; }
    public int EmailsSkipped { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion
