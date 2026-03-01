// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Ports.Input;
using CRM.Api.Infrastructure;

namespace CRM.API.Controllers;

/// <summary>
/// GDPR compliance endpoints — Article 15 (access), Article 17 (erasure).
/// TODO-SYS006-004
/// </summary>
[ApiController]
[Route("api/gdpr")]
[Authorize(Roles = "Admin")]
public class GdprController : CrmControllerBase
{
    private readonly IGdprService _gdprService;

    public GdprController(IGdprService gdprService)
    {
        _gdprService = gdprService;
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
                var logs = await _gdprService.GetAccessLogsAsync(subjectType, subjectId, ct);
        return Ok(logs);
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
                var userId = GetCurrentUserId();
        var ip = GetClientIpAddress();

        // Log the export access event
        await _gdprService.LogAccessAsync(userId, subjectType, subjectId, "export", ip,
            "GDPR Article 15 data export requested", ct);

        var export = await _gdprService.ExportPersonalDataAsync(subjectType, subjectId, ct);
        return Ok(export);
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
                var userId = GetCurrentUserId();
        var ip = GetClientIpAddress();

        var erased = await _gdprService.ErasePersonalDataAsync(
            subjectType, subjectId, userId, ip, ct);

        if (!erased)
        {
            return NotFound($"No {subjectType} record with ID {subjectId} found.");
        }

        return Ok(new
        {
            message = $"Personal data for {subjectType}/{subjectId} has been anonymised.",
            subjectType,
            subjectId,
            erasedAt = DateTime.UtcNow
        });
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private int GetCurrentUserId() // NOSONAR
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    // ─── Export Request Workflow (TODO-SYS006-005) ───────────────────────────

    /// <summary>
    /// Submit a GDPR data export request. The export is prepared asynchronously.
    /// POST /api/gdpr/export-request
    /// </summary>
    [HttpPost("export-request")]
    [ProducesResponseType(typeof(GdprExportRequestResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitExportRequest(
        [FromBody] GdprExportRequestDto request, CancellationToken ct)
    {
                if (string.IsNullOrWhiteSpace(request.SubjectType) || request.SubjectId <= 0)
                {
            return BadRequest(new { error = "SubjectType and SubjectId are required" });
                }

        var userId = GetCurrentUserId();
        var ip = GetClientIpAddress();

        // Log the export request
        await _gdprService.LogAccessAsync(userId, request.SubjectType, request.SubjectId,
            "export-request", ip, $"GDPR export request submitted for {request.SubjectType}/{request.SubjectId}", ct);

        // Perform the export immediately (can be made async with background job later)
        var export = await _gdprService.ExportPersonalDataAsync(request.SubjectType, request.SubjectId, ct);

        var requestId = Guid.NewGuid().ToString("N")[..12];

        // Store in cache for later retrieval (using static dictionary as simple store)
        ExportRequestStore[requestId] = new GdprExportResult
        {
            RequestId = requestId,
            Status = "completed",
            SubjectType = request.SubjectType,
            SubjectId = request.SubjectId,
            RequestedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            RequestedByUserId = userId,
            ExportData = export
        };

        return Accepted(new GdprExportRequestResponse
        {
            RequestId = requestId,
            Status = "completed",
            Message = $"Export for {request.SubjectType}/{request.SubjectId} is ready for download",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }

    /// <summary>
    /// Retrieve a completed GDPR data export by request ID.
    /// GET /api/gdpr/export/{requestId}
    /// </summary>
    [HttpGet("export/{requestId}")]
    [ProducesResponseType(typeof(GdprExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetExportResult(string requestId)
    {
        if (!ExportRequestStore.TryGetValue(requestId, out var result))
        {
            return NotFound(new { error = $"Export request '{requestId}' not found or expired" });
        }

        return Ok(result);
    }

    // In-memory store for export requests (TODO: move to Redis/DB for production)
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, GdprExportResult>
        ExportRequestStore = new();
}

// ─── DTOs for GDPR Export Workflow (TODO-SYS006-005) ─────────────────────────

public class GdprExportRequestDto
{
    public string SubjectType { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string? Reason { get; set; }
}

public class GdprExportRequestResponse
{
    public string RequestId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class GdprExportResult
{
    public string RequestId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RequestedByUserId { get; set; }
    public object? ExportData { get; set; }
}
