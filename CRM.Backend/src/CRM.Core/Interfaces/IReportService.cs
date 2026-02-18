// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos.Reports;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for report management, execution, scheduling, and sharing.
/// Provides comprehensive reporting functionality including CRUD operations,
/// report execution, scheduling, folder organization, and collaboration features.
/// </summary>
public interface IReportService
{
    #region Report CRUD Operations

    /// <summary>
    /// Gets all report definitions accessible to the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of all report definitions.</returns>
    Task<IEnumerable<ReportDefinitionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all report definitions in a specific category.
    /// </summary>
    /// <param name="category">The category name to filter by.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of report definitions in the specified category.</returns>
    Task<IEnumerable<ReportDefinitionDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all report definitions in a specific folder.
    /// </summary>
    /// <param name="folderId">The folder ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of report definitions in the specified folder.</returns>
    Task<IEnumerable<ReportDefinitionDto>> GetByFolderAsync(int folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all report definitions created by a specific user.
    /// </summary>
    /// <param name="userId">The user ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of report definitions created by the specified user.</returns>
    Task<IEnumerable<ReportDefinitionDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all standard (built-in) report definitions.
    /// Standard reports are system-provided and cannot be deleted.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of standard report definitions.</returns>
    Task<IEnumerable<ReportDefinitionDto>> GetStandardReportsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific report definition by its ID.
    /// </summary>
    /// <param name="id">The report definition ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The report definition if found; otherwise, null.</returns>
    Task<ReportDefinitionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new report definition.
    /// </summary>
    /// <param name="dto">The report definition data to create.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created report definition.</returns>
    Task<ReportDefinitionDto> CreateAsync(CreateReportDefinitionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report definition.
    /// </summary>
    /// <param name="dto">The report definition data to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated report definition.</returns>
    Task<ReportDefinitionDto> UpdateAsync(UpdateReportDefinitionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a report definition by its ID.
    /// </summary>
    /// <param name="id">The report definition ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the report was deleted; otherwise, false.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when attempting to delete a standard (built-in) report.</exception>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Report Execution

    /// <summary>
    /// Executes a report with the specified parameters.
    /// </summary>
    /// <param name="reportId">The report definition ID to execute.</param>
    /// <param name="parameters">Optional parameters for report execution.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The execution result containing report data.</returns>
    Task<ReportExecutionResultDto> ExecuteAsync(int reportId, ReportParametersDto? parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Previews a report with a limited number of rows.
    /// </summary>
    /// <param name="reportId">The report definition ID to preview.</param>
    /// <param name="limit">The maximum number of rows to return.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The execution result containing limited preview data.</returns>
    Task<ReportExecutionResultDto> PreviewAsync(int reportId, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a report to the specified format.
    /// </summary>
    /// <param name="reportId">The report definition ID to export.</param>
    /// <param name="format">The export format (e.g., "csv", "xlsx", "pdf").</param>
    /// <param name="parameters">Optional parameters for report execution.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The exported report data as a byte array.</returns>
    Task<byte[]> ExportAsync(int reportId, string format, ReportParametersDto? parameters, CancellationToken cancellationToken = default);

    #endregion

    #region Report Schedules

    /// <summary>
    /// Creates a new schedule for a report.
    /// </summary>
    /// <param name="dto">The schedule data to create.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created schedule.</returns>
    Task<ReportScheduleDto> CreateScheduleAsync(CreateReportScheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all schedules for a specific report.
    /// </summary>
    /// <param name="reportId">The report definition ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of schedules for the specified report.</returns>
    Task<IEnumerable<ReportScheduleDto>> GetSchedulesAsync(int reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report schedule.
    /// </summary>
    /// <param name="dto">The schedule data to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated schedule.</returns>
    Task<ReportScheduleDto> UpdateScheduleAsync(UpdateReportScheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a report schedule.
    /// </summary>
    /// <param name="scheduleId">The schedule ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the schedule was deleted; otherwise, false.</returns>
    Task<bool> DeleteScheduleAsync(int scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables a report schedule.
    /// </summary>
    /// <param name="scheduleId">The schedule ID to toggle.</param>
    /// <param name="enabled">True to enable the schedule; false to disable it.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the schedule was toggled successfully; otherwise, false.</returns>
    Task<bool> ToggleScheduleAsync(int scheduleId, bool enabled, CancellationToken cancellationToken = default);

    #endregion

    #region Report Folders

    /// <summary>
    /// Gets all report folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of all report folders.</returns>
    Task<IEnumerable<ReportFolderDto>> GetFoldersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new report folder.
    /// </summary>
    /// <param name="dto">The folder data to create.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created folder.</returns>
    Task<ReportFolderDto> CreateFolderAsync(CreateReportFolderDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report folder.
    /// </summary>
    /// <param name="dto">The folder data to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated folder.</returns>
    Task<ReportFolderDto> UpdateFolderAsync(UpdateReportFolderDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a report folder.
    /// </summary>
    /// <param name="folderId">The folder ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the folder was deleted; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to delete a non-empty folder.</exception>
    Task<bool> DeleteFolderAsync(int folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a report to a different folder.
    /// </summary>
    /// <param name="reportId">The report definition ID to move.</param>
    /// <param name="folderId">The target folder ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the report was moved successfully; otherwise, false.</returns>
    Task<bool> MoveToFolderAsync(int reportId, int folderId, CancellationToken cancellationToken = default);

    #endregion

    #region Execution History

    /// <summary>
    /// Gets the execution history for a specific report.
    /// </summary>
    /// <param name="reportId">The report definition ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of execution history entries for the specified report.</returns>
    Task<IEnumerable<ReportExecutionHistoryDto>> GetExecutionHistoryAsync(int reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the result of a specific report execution.
    /// </summary>
    /// <param name="executionId">The execution ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The execution result.</returns>
    Task<ReportExecutionResultDto> GetExecutionResultAsync(int executionId, CancellationToken cancellationToken = default);

    #endregion

    #region Clone and Share

    /// <summary>
    /// Creates a clone of an existing report.
    /// </summary>
    /// <param name="reportId">The report definition ID to clone.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The cloned report definition.</returns>
    Task<ReportDefinitionDto> CloneAsync(int reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shares a report with specified users or groups.
    /// </summary>
    /// <param name="reportId">The report definition ID to share.</param>
    /// <param name="dto">The sharing configuration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the report was shared successfully; otherwise, false.</returns>
    Task<bool> ShareAsync(int reportId, ShareReportDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sharing information for a report.
    /// </summary>
    /// <param name="reportId">The report definition ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The sharing information for the specified report.</returns>
    Task<ReportSharingDto> GetSharedWithAsync(int reportId, CancellationToken cancellationToken = default);

    #endregion

    #region Favorites

    /// <summary>
    /// Adds a report to a user's favorites.
    /// </summary>
    /// <param name="reportId">The report definition ID to add.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the report was added to favorites; otherwise, false.</returns>
    Task<bool> AddToFavoritesAsync(int reportId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a report from a user's favorites.
    /// </summary>
    /// <param name="reportId">The report definition ID to remove.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the report was removed from favorites; otherwise, false.</returns>
    Task<bool> RemoveFromFavoritesAsync(int reportId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all favorite reports for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of the user's favorite report definitions.</returns>
    Task<IEnumerable<ReportDefinitionDto>> GetFavoritesAsync(int userId, CancellationToken cancellationToken = default);

    #endregion
}
