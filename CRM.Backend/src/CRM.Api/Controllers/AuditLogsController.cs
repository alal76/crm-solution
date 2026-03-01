// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for Audit Log operations.
///
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Viewing and querying audit logs
/// - Creating new audit log entries
/// - Retrieving entity change history
/// - Exporting audit logs for compliance
///
/// TECHNICAL VIEW:
/// - Uses IAuditLogService for business logic (dependency injected)
/// - Most endpoints require Admin role
/// - Returns standardized JSON responses with appropriate HTTP status codes
/// - Implements proper error handling with logging
///
/// API ROUTES:
/// - GET    /api/audit-logs              - Get all audit logs (paginated)
/// - GET    /api/audit-logs/{id}         - Get audit log by ID (not implemented)
/// - POST   /api/audit-logs              - Create new audit log entry
/// - GET    /api/audit-logs/entity/{entityType}/{entityId} - Get entity history
/// - GET    /api/audit-logs/user/{userId} - Get user activity
/// - GET    /api/audit-logs/search       - Search audit logs
/// - GET    /api/audit-logs/statistics   - Get audit statistics
/// - GET    /api/audit-logs/export       - Export audit logs to CSV
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : CrmControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly IAuditLogExportService? _exportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogsController"/> class.
    /// </summary>
    /// <param name="auditLogService">Service for audit log business logic.</param>
    /// <param name="logger">Logger for error and audit logging.</param>
    /// <param name="exportService">Dedicated export service (optional).</param>
    public AuditLogsController(
        IAuditLogService auditLogService,
        IAuditLogExportService? exportService = null)
    {
        _auditLogService = auditLogService;
        _exportService = exportService;
    }

    /// <summary>
    /// Get all audit logs with optional filtering and pagination.
    /// </summary>
    /// <param name="entityType">Filter by entity type (optional).</param>
    /// <param name="entityId">Filter by entity ID (optional).</param>
    /// <param name="userId">Filter by user ID (optional).</param>
    /// <param name="action">Filter by action type (optional).</param>
    /// <param name="fromDate">Filter by start date (optional).</param>
    /// <param name="toDate">Filter by end date (optional).</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of audit logs.</returns>
    [HttpGet]
    [RequireRole(UserRole.Admin, UserRole.Manager)]
    [ProducesResponseType(typeof(AuditLogPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll( // NOSONAR
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
                var result = await _auditLogService.GetAuditLogsAsync(
            entityType, entityId, userId, action,
            fromDate, toDate, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new audit log entry.
    /// Used for logging custom actions or external system events.
    /// </summary>
    /// <param name="dto">The audit log entry to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the created audit log entry.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAuditLogDto dto,
        CancellationToken cancellationToken = default)
    {
                if (dto == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Action))
        {
            return BadRequest(new { error = "Action is required" });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var id = await _auditLogService.LogActionAsync(
            dto.Action,
            dto.EntityType,
            dto.EntityId,
            dto.UserId,
            dto.Details,
            ipAddress,
            userAgent,
            cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    /// <summary>
    /// Get audit history for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of audit logs for the entity.</returns>
    [HttpGet("entity/{entityType}/{entityId:int}")]
    [RequireRole(UserRole.Admin, UserRole.Manager)]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
                var result = await _auditLogService.GetEntityHistoryAsync(entityType, entityId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get activity logs for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="fromDate">Filter by start date (optional).</param>
    /// <param name="toDate">Filter by end date (optional).</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of user activity logs.</returns>
    [HttpGet("user/{userId:int}")]
    [RequireRole(UserRole.Admin, UserRole.Manager)]
    [ProducesResponseType(typeof(AuditLogPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserActivity(
        int userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
                var result = await _auditLogService.GetUserActivityAsync(
            userId, fromDate, toDate, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Search audit logs by free-text query.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated search results.</returns>
    [HttpGet("search")]
    [RequireRole(UserRole.Admin, UserRole.Manager)]
    [ProducesResponseType(typeof(AuditLogPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
                if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Query parameter is required" });
        }

        var result = await _auditLogService.SearchAsync(query, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get audit statistics for a date range.
    /// </summary>
    /// <param name="fromDate">Start date.</param>
    /// <param name="toDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Audit statistics.</returns>
    [HttpGet("statistics")]
    [RequireRole(UserRole.Admin)]
    [ProducesResponseType(typeof(AuditStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
                if (fromDate > toDate)
        {
            return BadRequest(new { error = "fromDate must be before toDate" });
        }

        var result = await _auditLogService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get entity change history with before/after comparison.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <param name="fromDate">Filter by start date (optional).</param>
    /// <param name="toDate">Filter by end date (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detailed change history.</returns>
    [HttpGet("changes/{entityType}/{entityId:int}")]
    [RequireRole(UserRole.Admin, UserRole.Manager)]
    [ProducesResponseType(typeof(EntityChangeHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntityChangeHistory(
        string entityType,
        int entityId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
                var result = await _auditLogService.GetEntityChangeHistoryAsync(
            entityType, entityId, fromDate, toDate, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Export audit logs to CSV format.
    /// </summary>
    /// <param name="entityType">Filter by entity type (optional).</param>
    /// <param name="fromDate">Filter by start date (optional).</param>
    /// <param name="toDate">Filter by end date (optional).</param>
    /// <param name="format">Export format: csv (default), json, or pdf. TODO-SYS006-008</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File download in the requested format.</returns>
    [HttpGet("export")]
    [RequireRole(UserRole.Admin)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Export(
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
                var (data, contentType, fileName) = await _auditLogService.ExportAuditLogsAsync(
            format, entityType, fromDate, toDate, cancellationToken);

        return File(data, contentType, fileName);
    }

    /// <summary>
    /// Export audit logs via POST with rich filter body.
    /// Returns CSV by default; pass <c>?format=json</c> for JSON.
    /// TODO-SYS006-008
    /// </summary>
    /// <param name="request">Export filter and page-size parameters.</param>
    /// <param name="format">Output format: csv (default) or json.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File download.</returns>
    [HttpPost("export")]
    [RequireRole(UserRole.Admin)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportPost(
        [FromBody] AuditLogExportRequestDto request,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (_exportService == null)
        {
            return StatusCode(501, new { error = "Export service is not configured" });
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        byte[] data;
        string contentType;
        string fileName;

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        {
            data = await _exportService.ExportToJsonAsync(request, cancellationToken);
            contentType = "application/json";
            fileName = $"audit-logs-{timestamp}.json";
        }
        else
        {
            data = await _exportService.ExportToCsvAsync(request, cancellationToken);
            contentType = "text/csv";
            fileName = $"audit-logs-{timestamp}.csv";
        }

        return File(data, contentType, fileName);
    }
}
