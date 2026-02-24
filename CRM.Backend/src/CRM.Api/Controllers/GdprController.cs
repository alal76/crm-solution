// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// GDPR compliance endpoints — Article 15 (access), Article 17 (erasure).
/// TODO-SYS006-004
/// </summary>
[ApiController]
[Route("api/gdpr")]
[Authorize(Roles = "Admin")]
public class GdprController : ControllerBase
{
    private readonly IGdprService _gdprService;
    private readonly ILogger<GdprController> _logger;

    public GdprController(IGdprService gdprService, ILogger<GdprController> logger)
    {
        _gdprService = gdprService;
        _logger = logger;
    }

    /// <summary>
    /// Get GDPR access log for a data subject.
    /// GET /api/gdpr/access-logs/{subjectType}/{subjectId}
    /// </summary>
    [HttpGet("access-logs/{subjectType}/{subjectId:int}")]
    [ProducesResponseType(typeof(IEnumerable<GdprAccessLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GdprAccessLogDto>>> GetAccessLogs(
        string subjectType, int subjectId, CancellationToken ct)
    {
        try
        {
            var logs = await _gdprService.GetAccessLogsAsync(subjectType, subjectId, ct);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GDPR access logs for {SubjectType}/{SubjectId}",
                subjectType, subjectId);
            return StatusCode(500, "Error retrieving GDPR access logs");
        }
    }

    /// <summary>
    /// Export all personal data for a subject (Article 15 Right of Access).
    /// POST /api/gdpr/export/{subjectType}/{subjectId}
    /// </summary>
    [HttpPost("export/{subjectType}/{subjectId:int}")]
    [ProducesResponseType(typeof(PersonalDataExport), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonalDataExport>> ExportPersonalData(
        string subjectType, int subjectId, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ip = GetClientIpAddress();

            // Log the export access event
            await _gdprService.LogAccessAsync(userId, subjectType, subjectId, "export", ip,
                "GDPR Article 15 data export requested", ct);

            var export = await _gdprService.ExportPersonalDataAsync(subjectType, subjectId, ct);
            return Ok(export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting personal data for {SubjectType}/{SubjectId}",
                subjectType, subjectId);
            return StatusCode(500, "Error exporting personal data");
        }
    }

    /// <summary>
    /// Erase personal data for a subject (Article 17 Right to be Forgotten).
    /// DELETE /api/gdpr/erase/{subjectType}/{subjectId}
    /// </summary>
    [HttpDelete("erase/{subjectType}/{subjectId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ErasePersonalData(
        string subjectType, int subjectId, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ip = GetClientIpAddress();

            var erased = await _gdprService.ErasePersonalDataAsync(
                subjectType, subjectId, userId, ip, ct);

            if (!erased)
                return NotFound($"No {subjectType} record with ID {subjectId} found.");

            return Ok(new
            {
                message = $"Personal data for {subjectType}/{subjectId} has been anonymised.",
                subjectType,
                subjectId,
                erasedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error erasing personal data for {SubjectType}/{SubjectId}",
                subjectType, subjectId);
            return StatusCode(500, "Error erasing personal data");
        }
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
