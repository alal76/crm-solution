// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.DTOs;
using CRM.Core.Dtos.Reports;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing reports, report schedules, folders, and execution.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
[Tags("Reports")]
public class ReportsController : CrmControllerBase
{
    private readonly IReportService _reportService;
    private readonly IWinLossAnalysisService _winLossService;
    private readonly IOpportunityService _opportunityService;
    private readonly IReportSharingService _sharingService;
    private readonly ILogger<ReportsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    public ReportsController(
        IReportService reportService,
        IWinLossAnalysisService winLossService,
        IOpportunityService opportunityService,
        IReportSharingService sharingService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _winLossService = winLossService ?? throw new ArgumentNullException(nameof(winLossService));
        _opportunityService = opportunityService ?? throw new ArgumentNullException(nameof(opportunityService));
        _sharingService = sharingService ?? throw new ArgumentNullException(nameof(sharingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Report CRUD Operations

    /// <summary>
    /// Gets all report definitions.
    /// </summary>
    /// <returns>A list of all report definitions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReportDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _reportService.GetAllAsync();
        return Ok(reports);
    }

    /// <summary>
    /// Gets report definitions by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A list of report definitions in the specified category.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<ReportDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var reports = await _reportService.GetByCategoryAsync(category);
        return Ok(reports);
    }

    /// <summary>
    /// Gets report definitions by folder.
    /// </summary>
    /// <param name="folderId">The folder ID to filter by.</param>
    /// <returns>A list of report definitions in the specified folder.</returns>
    [HttpGet("folder/{folderId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ReportDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByFolder(int folderId)
    {
        var reports = await _reportService.GetByFolderAsync(folderId);
        return Ok(reports);
    }

    /// <summary>
    /// Gets report definitions created by the current user.
    /// </summary>
    /// <returns>A list of report definitions owned by the current user.</returns>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<ReportDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReports()
    {
        var userId = GetCurrentUserId();
        var reports = await _reportService.GetByUserAsync(userId);
        return Ok(reports);
    }

    /// <summary>
    /// Gets all standard (system) reports.
    /// </summary>
    /// <returns>A list of standard report definitions.</returns>
    [HttpGet("standard")]
    [ProducesResponseType(typeof(IEnumerable<ReportDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStandardReports()
    {
        var reports = await _reportService.GetStandardReportsAsync();
        return Ok(reports);
    }

    /// <summary>
    /// Gets a report definition by ID.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>The report definition if found.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReportDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var report = await _reportService.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    /// <summary>
    /// Creates a new report definition.
    /// </summary>
    /// <param name="dto">The report definition to create.</param>
    /// <returns>The created report definition.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReportDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReportDefinitionDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            var report = await _reportService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict creating report: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing report definition.
    /// </summary>
    /// <param name="id">The identifier of the report to update.</param>
    /// <param name="dto">The updated report definition data.</param>
    /// <returns>The updated report definition.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReportDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReportDefinitionDto dto)
    {
        if (dto.Id != id)
        {
            return BadRequest("ID mismatch between route and body");
        }

        var report = await _reportService.UpdateAsync(dto);
        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    /// <summary>
    /// Deletes a report definition.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _reportService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to delete report {ReportId}", id);
            return Forbid();
        }
    }

    #endregion

    #region Report Execution

    /// <summary>
    /// Executes a report with optional parameters.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="parameters">Optional report parameters.</param>
    /// <returns>The report execution result.</returns>
    [HttpPost("{id:int}/execute")]
    [ProducesResponseType(typeof(ReportExecutionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Execute(int id, [FromBody] ReportParametersDto? parameters)
    {
        try
        {
            var result = await _reportService.ExecuteAsync(id, parameters);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for report {ReportId}", id);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Previews a report with a limited number of rows.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="limit">Maximum number of rows to return (default: 10).</param>
    /// <returns>The report preview result.</returns>
    [HttpGet("{id:int}/preview")]
    [ProducesResponseType(typeof(ReportExecutionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Preview(int id, [FromQuery] int limit = 10)
    {
        var result = await _reportService.PreviewAsync(id, limit);
        return Ok(result);
    }

    /// <summary>
    /// Exports a report in the specified format.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="format">Export format (csv, xlsx, pdf).</param>
    /// <param name="parameters">Optional report parameters.</param>
    /// <returns>The exported file.</returns>
    [HttpGet("{id:int}/export/{format}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(int id, string format, [FromQuery] ReportParametersDto? parameters)
    {
        var content = await _reportService.ExportAsync(id, format, parameters);

        var contentType = format.ToLowerInvariant() switch
        {
            "csv" => "text/csv",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var fileName = $"report_{id}.{format.ToLowerInvariant()}";
        return File(content, contentType, fileName);
    }

    #endregion

    #region Report Schedules

    /// <summary>
    /// Creates a schedule for a report.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <param name="dto">The schedule to create.</param>
    /// <returns>The created schedule.</returns>
    [HttpPost("{reportId:int}/schedules")]
    [ProducesResponseType(typeof(ReportScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSchedule(int reportId, [FromBody] CreateReportScheduleDto dto)
    {
        dto.ReportId = reportId;
        var schedule = await _reportService.CreateScheduleAsync(dto);
        return CreatedAtAction(nameof(GetSchedules), new { reportId }, schedule);
    }

    /// <summary>
    /// Gets all schedules for a report.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <returns>A list of schedules for the report.</returns>
    [HttpGet("{reportId:int}/schedules")]
    [ProducesResponseType(typeof(IEnumerable<ReportScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedules(int reportId)
    {
        var schedules = await _reportService.GetSchedulesAsync(reportId);
        return Ok(schedules);
    }

    /// <summary>
    /// Updates a schedule for a report.
    /// </summary>
    /// <param name="reportId">The identifier of the parent report.</param>
    /// <param name="scheduleId">The identifier of the schedule to update.</param>
    /// <param name="dto">The schedule configuration to apply.</param>
    /// <returns>The updated schedule.</returns>
    [HttpPut("{reportId:int}/schedules/{scheduleId:int}")]
    [ProducesResponseType(typeof(ReportScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSchedule(int reportId, int scheduleId, [FromBody] UpdateReportScheduleDto dto)
    {
        dto.Id = scheduleId;
        var schedule = await _reportService.UpdateScheduleAsync(dto);
        return Ok(schedule);
    }

    /// <summary>
    /// Deletes a schedule for a report.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{reportId:int}/schedules/{scheduleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSchedule(int reportId, int scheduleId)
    {
        var result = await _reportService.DeleteScheduleAsync(scheduleId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Enables or disables a schedule for a report.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <param name="enabled">Whether the schedule should be enabled.</param>
    /// <returns>True if the toggle was successful.</returns>
    [HttpPost("{reportId:int}/schedules/{scheduleId:int}/toggle")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleSchedule(int reportId, int scheduleId, [FromQuery] bool enabled)
    {
        var result = await _reportService.ToggleScheduleAsync(scheduleId, enabled);
        return Ok(result);
    }

    #endregion

    #region Report Folders

    /// <summary>
    /// Gets all report folders.
    /// </summary>
    /// <returns>A list of all report folders.</returns>
    [HttpGet("folders")]
    [ProducesResponseType(typeof(IEnumerable<ReportFolderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFolders()
    {
        var folders = await _reportService.GetFoldersAsync();
        return Ok(folders);
    }

    /// <summary>
    /// Creates a new report folder.
    /// </summary>
    /// <param name="dto">The folder to create.</param>
    /// <returns>The created folder.</returns>
    [HttpPost("folders")]
    [ProducesResponseType(typeof(ReportFolderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFolder([FromBody] CreateReportFolderDto dto)
    {
        var folder = await _reportService.CreateFolderAsync(dto);
        return CreatedAtAction(nameof(GetFolders), new { id = folder.Id }, folder);
    }

    /// <summary>
    /// Updates a report folder.
    /// </summary>
    /// <param name="id">The identifier of the folder to update.</param>
    /// <param name="dto">The folder properties to apply.</param>
    /// <returns>The updated folder.</returns>
    [HttpPut("folders/{id:int}")]
    [ProducesResponseType(typeof(ReportFolderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFolder(int id, [FromBody] UpdateReportFolderDto dto)
    {
        dto.Id = id;
        var folder = await _reportService.UpdateFolderAsync(dto);
        return Ok(folder);
    }

    /// <summary>
    /// Deletes a report folder.
    /// </summary>
    /// <param name="id">The folder ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("folders/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        try
        {
            await _reportService.DeleteFolderAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict deleting folder {FolderId}: {Message}", id, ex.Message);
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Moves a report to a different folder.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="folderId">The target folder ID.</param>
    /// <returns>True if the move was successful.</returns>
    [HttpPost("{id:int}/move/{folderId:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveReportToFolder(int id, int folderId)
    {
        var result = await _reportService.MoveToFolderAsync(id, folderId);
        return Ok(result);
    }

    #endregion

    #region Execution History

    /// <summary>
    /// Gets the execution history for a report.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>A list of execution history entries.</returns>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(IEnumerable<ReportExecutionHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExecutionHistory(int id)
    {
        var history = await _reportService.GetExecutionHistoryAsync(id);
        return Ok(history);
    }

    /// <summary>
    /// Gets a specific execution result.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <param name="executionId">The execution ID.</param>
    /// <returns>The execution result.</returns>
    [HttpGet("{reportId:int}/history/{executionId:int}")]
    [ProducesResponseType(typeof(ReportExecutionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionResult(int reportId, int executionId)
    {
        var result = await _reportService.GetExecutionResultAsync(executionId);
        return Ok(result);
    }

    #endregion

    #region Clone & Share

    /// <summary>
    /// Clones a report definition.
    /// </summary>
    /// <param name="id">The report ID to clone.</param>
    /// <returns>The cloned report definition.</returns>
    [HttpPost("{id:int}/clone")]
    [ProducesResponseType(typeof(ReportDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Clone(int id)
    {
        var report = await _reportService.CloneAsync(id);
        return Ok(report);
    }

    /// <summary>
    /// Shares a report with other users or groups.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="dto">The sharing configuration.</param>
    /// <returns>True if sharing was successful.</returns>
    [HttpPost("{id:int}/share")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Share(int id, [FromBody] ShareReportDto dto)
    {
        var result = await _reportService.ShareAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Gets the sharing configuration for a report.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>The sharing configuration.</returns>
    [HttpGet("{id:int}/sharing")]
    [ProducesResponseType(typeof(ReportSharingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSharedWith(int id)
    {
        var sharing = await _reportService.GetSharedWithAsync(id);
        return Ok(sharing);
    }

    #endregion

    #region Favorites

    /// <summary>
    /// Adds a report to the current user's favorites.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>True if the report was added to favorites.</returns>
    [HttpPost("{id:int}/favorite")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToFavorites(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.AddToFavoritesAsync(id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Removes a report from the current user's favorites.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>True if the report was removed from favorites.</returns>
    [HttpDelete("{id:int}/favorite")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromFavorites(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _reportService.RemoveFromFavoritesAsync(id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's favorite reports.
    /// </summary>
    /// <returns>A list of favorite report definitions.</returns>
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(IEnumerable<ReportDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = GetCurrentUserId();
        var reports = await _reportService.GetFavoritesAsync(userId);
        return Ok(reports);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the current user ID from the claims principal.
    /// </summary>
    /// <returns>The current user ID, or 0 if not found.</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #endregion

    #region Analytics Reports (TODO-CRM003-05, TODO-CRM003-07)

    /// <summary>
    /// Returns a consolidated Win/Loss report for the specified date range (TODO-CRM003-05).
    /// Includes summary, loss by reason, loss by competitor, and trend breakdown.
    /// </summary>
    /// <param name="fromDate">Start date filter (ISO 8601). Defaults to 1 year ago.</param>
    /// <param name="toDate">End date filter (ISO 8601). Defaults to today.</param>
    /// <param name="period">Trend grouping period: "month" or "quarter".</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("win-loss")]
    [ProducesResponseType(typeof(WinLossReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWinLossReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string period = "month",
        CancellationToken ct = default)
    {
                var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var summary = await _winLossService.GetSummaryAsync(from, to, ct);
        var byReason = await _winLossService.GetByReasonAsync(from, to, ct);
        var byCompetitor = await _winLossService.GetByCompetitorAsync(from, to, ct);
        var trends = await _winLossService.GetWinRateTrendsAsync(from, to, period, ct);

        var report = new WinLossReportDto
        {
            Summary = summary,
            ByReason = byReason,
            ByCompetitor = byCompetitor,
            Trends = trends,
            FromDate = from,
            ToDate = to
        };

        return Ok(report);
    }

    /// <summary>
    /// Returns a forecast summary report grouped by ForecastCategory bucket (TODO-CRM003-07).
    /// Includes MRR, ARR, weighted pipeline, and per-category breakdowns.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("forecast-summary")]
    [ProducesResponseType(typeof(ForecastSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetForecastSummary(CancellationToken ct = default)
    {
                var summary = await _opportunityService.GetForecastSummaryAsync(ct);
        return Ok(summary);
    }

    #endregion

    #region Report Sharing (TODO-RPT-03)

    /// <summary>
    /// Shares a report with one or more users.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <param name="dto">Share request with user IDs and permission level.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{id:int}/shares")]
    [ProducesResponseType(typeof(ReportShareResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareReport(
        int id,
        [FromBody] ShareReportDto dto,
        CancellationToken ct = default)
    {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        if (dto.UserIds == null || dto.UserIds.Count == 0)
        {
            return BadRequest("At least one user ID must be provided.");
        }

        var permission = Enum.TryParse<ReportSharePermission>(dto.Permission, ignoreCase: true, out var p)
            ? p
            : ReportSharePermission.View;

        var result = await _sharingService.ShareReportAsync(id, dto.UserIds, permission, currentUserId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of users a report is shared with.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id:int}/shares")]
    [ProducesResponseType(typeof(IEnumerable<ReportShareInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportShares(int id, CancellationToken ct = default)
    {
                var shares = await _sharingService.GetReportShareInfoAsync(id, ct);
        return Ok(shares);
    }

    /// <summary>
    /// Revokes a report share for a specific user.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <param name="userId">The user whose access should be revoked.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("{id:int}/shares/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeReportShare(int id, int userId, CancellationToken ct = default)
    {
                var revoked = await _sharingService.RevokeShareAsync(id, userId, ct);
        if (!revoked)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion

    #region Cohort Analysis (TODO-RPT-07)

    /// <summary>
    /// Executes a cohort analysis over customers and returns cohort retention/revenue data.
    /// </summary>
    /// <param name="dto">Cohort analysis parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("cohort-analysis")]
    [ProducesResponseType(typeof(CohortAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCohortAnalysis(
        [FromBody] CohortAnalysisRequestDto dto,
        CancellationToken ct = default)
    {
                if (dto.StartDate >= dto.EndDate)
        {
            return BadRequest("StartDate must be before EndDate.");
        }

        var result = await _reportService.GetCohortAnalysisAsync(dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Segments customers by industry, region, revenue band, or lifecycle stage.
    /// </summary>
    /// <param name="dto">Segmentation criteria.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("customer-segments")]
    [ProducesResponseType(typeof(IEnumerable<CustomerSegmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomerSegments(
        [FromBody] SegmentationCriteria dto,
        CancellationToken ct = default)
    {
                var result = await _reportService.GetCustomerSegmentsAsync(dto, ct);
        return Ok(result);
    }

    #endregion
}
