// -----------------------------------------------------------------------
// CRM Solution - Reports Controller
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------

using System.Security.Claims;
using CRM.Core.Dtos.Reports;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing reports, report schedules, folders, and execution.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
[Tags("Reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    /// <param name="reportService">The report service.</param>
    /// <param name="logger">The logger.</param>
    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
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
    /// <param name="id">The report ID.</param>
    /// <param name="dto">The updated report definition.</param>
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
    /// <param name="reportId">The report ID.</param>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <param name="dto">The updated schedule.</param>
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
    /// <param name="id">The folder ID.</param>
    /// <param name="dto">The updated folder.</param>
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
}
