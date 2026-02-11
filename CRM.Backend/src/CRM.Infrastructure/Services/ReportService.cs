// -----------------------------------------------------------------------
// CRM Solution - Enterprise Customer Relationship Management
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

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos.Reports;
using CRM.Core.Entities.Reports;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Type aliases to disambiguate from ReportBuilderService.ReportDefinition
using ReportDefinitionEntity = CRM.Core.Entities.Reports.ReportDefinition;
using ReportFolderEntity = CRM.Core.Entities.Reports.ReportFolder;
using ReportScheduleEntity = CRM.Core.Entities.Reports.ReportSchedule;
using ReportExecutionEntity = CRM.Core.Entities.Reports.ReportExecution;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for report management, execution, scheduling, and sharing.
/// Provides comprehensive reporting functionality including CRUD operations,
/// report execution, scheduling, folder organization, and collaboration features.
/// </summary>
public class ReportService : IReportService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ReportService> _logger;

    // In-memory storage for favorites - TODO: Move to database entity or user preferences
    private static readonly ConcurrentDictionary<(int UserId, int ReportId), bool> _userFavorites = new();

    // In-memory storage for sharing - TODO: Move to database entity
    private static readonly ConcurrentDictionary<int, ShareReportDto> _reportSharing = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ReportService(ICrmDbContext context, ILogger<ReportService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Report CRUD Operations

    /// <inheritdoc />
    public async Task<IEnumerable<ReportDefinitionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all report definitions");

        var reports = await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportDefinitionDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting report definitions for category: {Category}", category);

        var reports = await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Category == category)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportDefinitionDto>> GetByFolderAsync(int folderId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting report definitions for folder: {FolderId}", folderId);

        var reports = await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.FolderId == folderId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportDefinitionDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting report definitions for user: {UserId}", userId);

        var reports = await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.CreatedByUserId == userId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportDefinitionDto>> GetStandardReportsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting standard report definitions");

        // Standard reports are identified by CreatedByUserId == 0 (system-created)
        var reports = await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.CreatedByUserId == 0)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<ReportDefinitionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting report definition by ID: {Id}", id);

        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        return report != null ? MapToDto(report) : null;
    }

    /// <inheritdoc />
    public async Task<ReportDefinitionDto> CreateAsync(CreateReportDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new report definition: {Name}", dto.Name);

        // Check for duplicate name
        var existingReport = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == dto.Name && !r.IsDeleted, cancellationToken);

        if (existingReport != null)
        {
            throw new InvalidOperationException($"A report with the name '{dto.Name}' already exists.");
        }

        var entity = new ReportDefinitionEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            FolderId = dto.FolderId,
            CustomQuery = dto.Query,
            CreatedByUserId = 1, // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ReportStatus.Active
        };

        _context.ReportDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created report definition with ID: {Id}", entity.Id);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<ReportDefinitionDto> UpdateAsync(UpdateReportDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating report definition: {Id}", dto.Id);

        var entity = await _context.ReportDefinitions
            .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Report with ID {dto.Id} not found.");
        }

        // Check for duplicate name (excluding current report)
        var duplicateName = await _context.ReportDefinitions
            .AsNoTracking()
            .AnyAsync(r => r.Name == dto.Name && r.Id != dto.Id && !r.IsDeleted, cancellationToken);

        if (duplicateName)
        {
            throw new InvalidOperationException($"A report with the name '{dto.Name}' already exists.");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Category = dto.Category;
        entity.FolderId = dto.FolderId;
        entity.CustomQuery = dto.Query;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated report definition: {Id}", dto.Id);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting report definition: {Id}", id);

        var entity = await _context.ReportDefinitions
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Report definition not found: {Id}", id);
            return false;
        }

        // Check if it's a standard report (system-created)
        if (IsStandardReport(entity))
        {
            throw new UnauthorizedAccessException("Cannot delete standard reports");
        }

        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted report definition: {Id}", id);
        return true;
    }

    #endregion

    #region Report Execution

    /// <inheritdoc />
    public async Task<ReportExecutionResultDto> ExecuteAsync(int reportId, ReportParametersDto? parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing report: {ReportId}", reportId);

        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

        if (report == null)
        {
            throw new InvalidOperationException($"Report with ID {reportId} not found.");
        }

        var startTime = DateTime.UtcNow;

        // Create execution record
        var execution = new ReportExecutionEntity
        {
            ReportDefinitionId = reportId,
            Status = ReportExecutionStatus.Running,
            StartedAt = startTime,
            ParametersJson = parameters != null ? JsonSerializer.Serialize(parameters) : null,
            TriggeredByUserId = 1, // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow
        };

        _context.ReportExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Implement actual report execution logic based on report.CustomQuery or report.DataSource
        // For now, return sample data
        var sampleData = GenerateSampleReportData(report);

        // Update execution record
        var endTime = DateTime.UtcNow;
        execution.Status = ReportExecutionStatus.Completed;
        execution.CompletedAt = endTime;
        execution.ExecutionTimeSeconds = (decimal)(endTime - startTime).TotalSeconds;
        execution.RowCount = sampleData.Count;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report execution completed: {ReportId}, Rows: {RowCount}", reportId, sampleData.Count);

        return new ReportExecutionResultDto
        {
            ReportId = reportId,
            ExecutionId = execution.Id,
            Columns = sampleData.Count > 0 ? sampleData[0].Keys.ToList() : new List<string>(),
            Data = sampleData,
            ExecutedAt = startTime,
            RowCount = sampleData.Count,
            ExecutionTimeMs = (long)(endTime - startTime).TotalMilliseconds
        };
    }

    /// <inheritdoc />
    public async Task<ReportExecutionResultDto> PreviewAsync(int reportId, int limit, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Previewing report: {ReportId} with limit: {Limit}", reportId, limit);

        var result = await ExecuteAsync(reportId, null, cancellationToken);

        // Limit the preview data
        if (result.Data.Count > limit)
        {
            result.Data = result.Data.Take(limit).ToList();
            result.RowCount = limit;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportAsync(int reportId, string format, ReportParametersDto? parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting report: {ReportId} as {Format}", reportId, format);

        var validFormats = new[] { "csv", "xlsx", "pdf" };
        if (!validFormats.Contains(format.ToLowerInvariant()))
        {
            throw new ArgumentException($"Invalid export format: {format}. Supported formats: {string.Join(", ", validFormats)}", nameof(format));
        }

        var result = await ExecuteAsync(reportId, parameters, cancellationToken);

        // TODO: Implement actual export logic for each format
        // For now, return placeholder content
        return format.ToLowerInvariant() switch
        {
            "csv" => GenerateCsvExport(result),
            "xlsx" => GenerateXlsxExportPlaceholder(result),
            "pdf" => GeneratePdfExportPlaceholder(result),
            _ => throw new ArgumentException($"Unsupported format: {format}", nameof(format))
        };
    }

    #endregion

    #region Report Schedules

    /// <inheritdoc />
    public async Task<ReportScheduleDto> CreateScheduleAsync(CreateReportScheduleDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating schedule for report: {ReportId}", dto.ReportId);

        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == dto.ReportId && !r.IsDeleted, cancellationToken);

        if (report == null)
        {
            throw new InvalidOperationException($"Report with ID {dto.ReportId} not found.");
        }

        var entity = new ReportScheduleEntity
        {
            ReportDefinitionId = dto.ReportId,
            Name = $"Schedule for {report.Name}",
            Frequency = ParseScheduleFrequency(dto.Frequency),
            TimeOfDay = dto.Time.ToTimeSpan(),
            EmailRecipientsJson = JsonSerializer.Serialize(dto.Recipients),
            OutputFormat = ParseOutputFormat(dto.ExportFormat),
            Status = ScheduleStatus.Active,
            NextRunAt = CalculateNextRunTime(ParseScheduleFrequency(dto.Frequency), dto.Time),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ReportSchedules.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created schedule with ID: {Id}", entity.Id);

        return MapToScheduleDto(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportScheduleDto>> GetSchedulesAsync(int reportId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting schedules for report: {ReportId}", reportId);

        var schedules = await _context.ReportSchedules
            .AsNoTracking()
            .Where(s => s.ReportDefinitionId == reportId && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return schedules.Select(MapToScheduleDto);
    }

    /// <inheritdoc />
    public async Task<ReportScheduleDto> UpdateScheduleAsync(UpdateReportScheduleDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating schedule: {Id}", dto.Id);

        var entity = await _context.ReportSchedules
            .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Schedule with ID {dto.Id} not found.");
        }

        entity.Frequency = ParseScheduleFrequency(dto.Frequency);
        entity.TimeOfDay = dto.Time.ToTimeSpan();
        entity.EmailRecipientsJson = JsonSerializer.Serialize(dto.Recipients);
        entity.OutputFormat = ParseOutputFormat(dto.ExportFormat);
        entity.NextRunAt = CalculateNextRunTime(entity.Frequency, dto.Time);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated schedule: {Id}", dto.Id);

        return MapToScheduleDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteScheduleAsync(int scheduleId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting schedule: {Id}", scheduleId);

        var entity = await _context.ReportSchedules
            .FirstOrDefaultAsync(s => s.Id == scheduleId && !s.IsDeleted, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Schedule not found: {Id}", scheduleId);
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted schedule: {Id}", scheduleId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ToggleScheduleAsync(int scheduleId, bool enabled, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Toggling schedule {Id} to enabled: {Enabled}", scheduleId, enabled);

        var entity = await _context.ReportSchedules
            .FirstOrDefaultAsync(s => s.Id == scheduleId && !s.IsDeleted, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Schedule not found: {Id}", scheduleId);
            return false;
        }

        entity.Status = enabled ? ScheduleStatus.Active : ScheduleStatus.Paused;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Toggled schedule {Id} to {Status}", scheduleId, entity.Status);
        return true;
    }

    #endregion

    #region Report Folders

    /// <inheritdoc />
    public async Task<IEnumerable<ReportFolderDto>> GetFoldersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all report folders");

        var folders = await _context.ReportFolders
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .Include(f => f.Reports.Where(r => !r.IsDeleted))
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        return folders.Select(MapToFolderDto);
    }

    /// <inheritdoc />
    public async Task<ReportFolderDto> CreateFolderAsync(CreateReportFolderDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating report folder: {Name}", dto.Name);

        var entity = new ReportFolderEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentFolderId = dto.ParentId,
            OwnerUserId = 1, // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ReportFolders.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created folder with ID: {Id}", entity.Id);

        return MapToFolderDto(entity);
    }

    /// <inheritdoc />
    public async Task<ReportFolderDto> UpdateFolderAsync(UpdateReportFolderDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating folder: {Id}", dto.Id);

        var entity = await _context.ReportFolders
            .FirstOrDefaultAsync(f => f.Id == dto.Id && !f.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Folder with ID {dto.Id} not found.");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ParentFolderId = dto.ParentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated folder: {Id}", dto.Id);

        return MapToFolderDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFolderAsync(int folderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting folder: {Id}", folderId);

        var entity = await _context.ReportFolders
            .Include(f => f.Reports.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(f => f.Id == folderId && !f.IsDeleted, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Folder not found: {Id}", folderId);
            return false;
        }

        // Check if folder has any reports
        if (entity.Reports.Any())
        {
            throw new InvalidOperationException("Cannot delete folder that contains reports");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted folder: {Id}", folderId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> MoveToFolderAsync(int reportId, int folderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Moving report {ReportId} to folder {FolderId}", reportId, folderId);

        var report = await _context.ReportDefinitions
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

        if (report == null)
        {
            _logger.LogWarning("Report not found: {Id}", reportId);
            return false;
        }

        // Verify folder exists
        var folderExists = await _context.ReportFolders
            .AsNoTracking()
            .AnyAsync(f => f.Id == folderId && !f.IsDeleted, cancellationToken);

        if (!folderExists)
        {
            _logger.LogWarning("Folder not found: {Id}", folderId);
            return false;
        }

        report.FolderId = folderId;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Moved report {ReportId} to folder {FolderId}", reportId, folderId);
        return true;
    }

    #endregion

    #region Execution History

    /// <inheritdoc />
    public async Task<IEnumerable<ReportExecutionHistoryDto>> GetExecutionHistoryAsync(int reportId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting execution history for report: {ReportId}", reportId);

        var executions = await _context.ReportExecutions
            .AsNoTracking()
            .Where(e => e.ReportDefinitionId == reportId)
            .Include(e => e.TriggeredByUser)
            .OrderByDescending(e => e.StartedAt)
            .Take(100) // Limit history
            .ToListAsync(cancellationToken);

        return executions.Select(MapToExecutionHistoryDto);
    }

    /// <inheritdoc />
    public async Task<ReportExecutionResultDto> GetExecutionResultAsync(int executionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting execution result: {ExecutionId}", executionId);

        var execution = await _context.ReportExecutions
            .AsNoTracking()
            .Include(e => e.ReportDefinition)
            .FirstOrDefaultAsync(e => e.Id == executionId, cancellationToken);

        if (execution == null)
        {
            throw new InvalidOperationException($"Execution with ID {executionId} not found.");
        }

        // TODO: Retrieve cached execution results from storage
        // For now, return basic execution info
        return new ReportExecutionResultDto
        {
            ReportId = execution.ReportDefinitionId,
            ExecutionId = execution.Id,
            Columns = new List<string>(),
            Data = new List<Dictionary<string, object>>(),
            ExecutedAt = execution.StartedAt,
            RowCount = execution.RowCount ?? 0,
            ExecutionTimeMs = (long)((execution.ExecutionTimeSeconds ?? 0) * 1000)
        };
    }

    #endregion

    #region Clone and Share

    /// <inheritdoc />
    public async Task<ReportDefinitionDto> CloneAsync(int reportId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cloning report: {ReportId}", reportId);

        var original = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

        if (original == null)
        {
            throw new InvalidOperationException($"Report with ID {reportId} not found.");
        }

        var clone = new ReportDefinitionEntity
        {
            Name = $"{original.Name} (Copy)",
            Description = original.Description,
            Category = original.Category,
            FolderId = original.FolderId,
            ReportType = original.ReportType,
            DataSource = original.DataSource,
            Status = ReportStatus.Draft,
            AccessLevel = ReportAccessLevel.Private,
            ColumnsJson = original.ColumnsJson,
            FiltersJson = original.FiltersJson,
            GroupByJson = original.GroupByJson,
            SortByJson = original.SortByJson,
            AggregationsJson = original.AggregationsJson,
            CustomQuery = original.CustomQuery,
            RowLimit = original.RowLimit,
            TimePeriod = original.TimePeriod,
            DateField = original.DateField,
            ChartConfigJson = original.ChartConfigJson,
            ConditionalFormattingJson = original.ConditionalFormattingJson,
            ShowDataLabels = original.ShowDataLabels,
            ShowLegend = original.ShowLegend,
            ShowTotals = original.ShowTotals,
            CreatedByUserId = 1, // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ReportDefinitions.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned report {OriginalId} to new report {CloneId}", reportId, clone.Id);

        return MapToDto(clone);
    }

    /// <inheritdoc />
    public async Task<bool> ShareAsync(int reportId, ShareReportDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sharing report {ReportId} with {UserCount} users and {GroupCount} groups",
            reportId, dto.UserIds.Count, dto.GroupIds.Count);

        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

        if (report == null)
        {
            _logger.LogWarning("Report not found: {Id}", reportId);
            return false;
        }

        // TODO: Implement proper sharing logic with database entity
        // For now, store in memory
        _reportSharing[reportId] = dto;

        _logger.LogInformation("Shared report {ReportId}", reportId);
        return true;
    }

    /// <inheritdoc />
    public async Task<ReportSharingDto> GetSharedWithAsync(int reportId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting sharing info for report: {ReportId}", reportId);

        var report = await _context.ReportDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);

        if (report == null)
        {
            throw new InvalidOperationException($"Report with ID {reportId} not found.");
        }

        // TODO: Implement proper sharing retrieval from database
        // For now, return from in-memory storage or empty result
        if (_reportSharing.TryGetValue(reportId, out var shareDto))
        {
            return new ReportSharingDto
            {
                Users = new List<CRM.Core.Dtos.UserDto>(),
                Groups = new List<CRM.Core.Dtos.UserGroupDto>()
            };
        }

        return new ReportSharingDto
        {
            Users = new List<CRM.Core.Dtos.UserDto>(),
            Groups = new List<CRM.Core.Dtos.UserGroupDto>()
        };
    }

    #endregion

    #region Favorites

    /// <inheritdoc />
    public Task<bool> AddToFavoritesAsync(int reportId, int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding report {ReportId} to favorites for user {UserId}", reportId, userId);

        // TODO: Move to database entity (e.g., UserReportFavorites table)
        // For now, use in-memory storage
        _userFavorites[(userId, reportId)] = true;

        _logger.LogInformation("Added report {ReportId} to favorites for user {UserId}", reportId, userId);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> RemoveFromFavoritesAsync(int reportId, int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing report {ReportId} from favorites for user {UserId}", reportId, userId);

        // TODO: Move to database entity
        _userFavorites.TryRemove((userId, reportId), out _);

        _logger.LogInformation("Removed report {ReportId} from favorites for user {UserId}", reportId, userId);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportDefinitionDto>> GetFavoritesAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting favorite reports for user: {UserId}", userId);

        // TODO: Move to database query with proper join
        // For now, filter from in-memory favorites
        var favoriteReportIds = _userFavorites
            .Where(kv => kv.Key.UserId == userId && kv.Value)
            .Select(kv => kv.Key.ReportId)
            .ToList();

        if (!favoriteReportIds.Any())
        {
            return Enumerable.Empty<ReportDefinitionDto>();
        }

        var reports = await _context.ReportDefinitions
            .AsNoTracking()
            .Where(r => !r.IsDeleted && favoriteReportIds.Contains(r.Id))
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Maps a ReportDefinition entity to a ReportDefinitionDto.
    /// </summary>
    private ReportDefinitionDto MapToDto(ReportDefinitionEntity entity)
    {
        return new ReportDefinitionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            Category = entity.Category,
            FolderId = entity.FolderId,
            CreatedById = entity.CreatedByUserId,
            IsStandard = IsStandardReport(entity),
            CreatedAt = entity.CreatedAt
        };
    }

    /// <summary>
    /// Maps a ReportSchedule entity to a ReportScheduleDto.
    /// </summary>
    private ReportScheduleDto MapToScheduleDto(ReportScheduleEntity entity)
    {
        var recipients = new List<string>();
        if (!string.IsNullOrEmpty(entity.EmailRecipientsJson))
        {
            try
            {
                recipients = JsonSerializer.Deserialize<List<string>>(entity.EmailRecipientsJson) ?? new List<string>();
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        return new ReportScheduleDto
        {
            Id = entity.Id,
            ReportId = entity.ReportDefinitionId,
            Frequency = entity.Frequency.ToString(),
            Time = entity.TimeOfDay.HasValue ? TimeOnly.FromTimeSpan(entity.TimeOfDay.Value) : TimeOnly.MinValue,
            Recipients = recipients,
            ExportFormat = entity.OutputFormat.ToString(),
            IsEnabled = entity.Status == ScheduleStatus.Active,
            NextRunAt = entity.NextRunAt,
            LastRunAt = entity.LastRunAt
        };
    }

    /// <summary>
    /// Maps a ReportFolder entity to a ReportFolderDto.
    /// </summary>
    private ReportFolderDto MapToFolderDto(ReportFolderEntity entity)
    {
        return new ReportFolderDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            ParentId = entity.ParentFolderId,
            ReportCount = entity.Reports?.Count(r => !r.IsDeleted) ?? 0
        };
    }

    /// <summary>
    /// Maps a ReportExecution entity to a ReportExecutionHistoryDto.
    /// </summary>
    private ReportExecutionHistoryDto MapToExecutionHistoryDto(ReportExecutionEntity entity)
    {
        return new ReportExecutionHistoryDto
        {
            Id = entity.Id,
            ReportId = entity.ReportDefinitionId,
            ExecutedAt = entity.StartedAt,
            Status = entity.Status.ToString(),
            RowCount = entity.RowCount,
            ExecutionTimeMs = entity.ExecutionTimeSeconds.HasValue ? (long)(entity.ExecutionTimeSeconds.Value * 1000) : null,
            TriggeredBy = entity.TriggeredByUser?.Email ?? (entity.ReportScheduleId.HasValue ? "Scheduled" : "Unknown")
        };
    }

    /// <summary>
    /// Determines if a report is a standard (system-created) report.
    /// Standard reports are those created by the system (CreatedByUserId == 0).
    /// </summary>
    private static bool IsStandardReport(ReportDefinitionEntity entity)
    {
        return entity.CreatedByUserId == 0;
    }

    /// <summary>
    /// Parses a string frequency to ScheduleFrequency enum.
    /// </summary>
    private static ScheduleFrequency ParseScheduleFrequency(string frequency)
    {
        return frequency?.ToLowerInvariant() switch
        {
            "once" => ScheduleFrequency.Once,
            "hourly" => ScheduleFrequency.Hourly,
            "daily" => ScheduleFrequency.Daily,
            "weekly" => ScheduleFrequency.Weekly,
            "biweekly" => ScheduleFrequency.BiWeekly,
            "monthly" => ScheduleFrequency.Monthly,
            "quarterly" => ScheduleFrequency.Quarterly,
            "yearly" => ScheduleFrequency.Yearly,
            _ => ScheduleFrequency.Daily
        };
    }

    /// <summary>
    /// Parses a string output format to ReportOutputFormat enum.
    /// </summary>
    private static ReportOutputFormat ParseOutputFormat(string format)
    {
        return format?.ToUpperInvariant() switch
        {
            "PDF" => ReportOutputFormat.PDF,
            "EXCEL" or "XLSX" => ReportOutputFormat.Excel,
            "CSV" => ReportOutputFormat.CSV,
            "HTML" => ReportOutputFormat.HTML,
            "PNG" => ReportOutputFormat.PNG,
            "JSON" => ReportOutputFormat.JSON,
            _ => ReportOutputFormat.PDF
        };
    }

    /// <summary>
    /// Calculates the next run time for a schedule.
    /// </summary>
    private static DateTime CalculateNextRunTime(ScheduleFrequency frequency, TimeOnly time)
    {
        var today = DateTime.UtcNow.Date;
        var nextRun = today.Add(time.ToTimeSpan());

        if (nextRun <= DateTime.UtcNow)
        {
            nextRun = frequency switch
            {
                ScheduleFrequency.Hourly => DateTime.UtcNow.AddHours(1),
                ScheduleFrequency.Daily => nextRun.AddDays(1),
                ScheduleFrequency.Weekly => nextRun.AddDays(7),
                ScheduleFrequency.BiWeekly => nextRun.AddDays(14),
                ScheduleFrequency.Monthly => nextRun.AddMonths(1),
                ScheduleFrequency.Quarterly => nextRun.AddMonths(3),
                ScheduleFrequency.Yearly => nextRun.AddYears(1),
                _ => nextRun.AddDays(1)
            };
        }

        return nextRun;
    }

    /// <summary>
    /// Generates sample report data for testing/placeholder purposes.
    /// </summary>
    private static List<Dictionary<string, object>> GenerateSampleReportData(ReportDefinitionEntity report)
    {
        // TODO: Implement actual report data generation based on report.DataSource and report.CustomQuery
        var data = new List<Dictionary<string, object>>();

        for (int i = 1; i <= 10; i++)
        {
            data.Add(new Dictionary<string, object>
            {
                ["Id"] = i,
                ["Name"] = $"Sample Record {i}",
                ["Value"] = i * 100.0m,
                ["Date"] = DateTime.UtcNow.AddDays(-i),
                ["Status"] = i % 2 == 0 ? "Active" : "Pending"
            });
        }

        return data;
    }

    /// <summary>
    /// Generates a CSV export from the report result.
    /// </summary>
    private static byte[] GenerateCsvExport(ReportExecutionResultDto result)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", result.Columns));

        // Data rows
        foreach (var row in result.Data)
        {
            var values = result.Columns.Select(col =>
            {
                var value = row.TryGetValue(col, out var v) ? v?.ToString() ?? string.Empty : string.Empty;
                // Escape quotes and wrap in quotes if contains comma
                if (value.Contains(',') || value.Contains('"'))
                {
                    value = $"\"{value.Replace("\"", "\"\"")}\"";
                }
                return value;
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Generates a placeholder XLSX export.
    /// </summary>
    private static byte[] GenerateXlsxExportPlaceholder(ReportExecutionResultDto result)
    {
        // TODO: Implement actual Excel export using a library like EPPlus or ClosedXML
        return Encoding.UTF8.GetBytes($"XLSX export placeholder - {result.RowCount} rows");
    }

    /// <summary>
    /// Generates a placeholder PDF export.
    /// </summary>
    private static byte[] GeneratePdfExportPlaceholder(ReportExecutionResultDto result)
    {
        // TODO: Implement actual PDF export using a library like iTextSharp or QuestPDF
        return Encoding.UTF8.GetBytes($"PDF export placeholder - {result.RowCount} rows");
    }

    #endregion
}
