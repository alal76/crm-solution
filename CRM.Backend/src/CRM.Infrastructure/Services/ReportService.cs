// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos.Reports;
using CRM.Core.Entities;
using CRM.Core.Entities.Reports;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
// Type aliases to disambiguate from ReportBuilderService.ReportDefinition
using ReportDefinitionEntity = CRM.Core.Entities.Reports.ReportDefinition;
using ReportEntityDataSource = CRM.Core.Entities.Reports.ReportDataSource;
using ReportExecutionEntity = CRM.Core.Entities.Reports.ReportExecution;
using ReportFolderEntity = CRM.Core.Entities.Reports.ReportFolder;
using ReportScheduleEntity = CRM.Core.Entities.Reports.ReportSchedule;
using CRM.Core.Models;


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
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // In-memory storage for favorites - TODO: Move to database entity or user preferences
    private static readonly ConcurrentDictionary<(int UserId, int ReportId), bool> _userFavorites = new();

    // In-memory storage for sharing - TODO: Move to database entity
    private static readonly ConcurrentDictionary<int, ShareReportDto> _reportSharing = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor for current user resolution.</param>
    public ReportService(ICrmDbContext context, ILogger<ReportService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Gets the current user's ID from the JWT claims, falling back to 1 (admin) if not available.
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 1;
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
            CreatedByUserId = GetCurrentUserId(),
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
            TriggeredByUserId = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ReportExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        var reportData = await BuildReportDataAsync(report, parameters, cancellationToken);

        // Update execution record
        var endTime = DateTime.UtcNow;
        execution.Status = ReportExecutionStatus.Completed;
        execution.CompletedAt = endTime;
        execution.ExecutionTimeSeconds = (decimal)(endTime - startTime).TotalSeconds;
        execution.RowCount = reportData.Count;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report execution completed: {ReportId}, Rows: {RowCount}", reportId, reportData.Count);

        return new ReportExecutionResultDto
        {
            ReportId = reportId,
            ExecutionId = execution.Id,
            Columns = reportData.Count > 0 ? reportData[0].Keys.ToList() : new List<string>(),
            Data = reportData,
            ExecutedAt = startTime,
            RowCount = reportData.Count,
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
            OwnerUserId = GetCurrentUserId(),
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
            CreatedByUserId = GetCurrentUserId(),
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

    private sealed class ReportDesignerConfig
    {
        public string? DataSource { get; set; }
        public List<ReportDesignerColumn>? Columns { get; set; }
        public List<ReportDesignerFilter>? Filters { get; set; }
        public List<ReportDesignerSort>? Sorts { get; set; }
        public int? LimitRows { get; set; }
    }

    private sealed class ReportDesignerColumn
    {
        public string? Field { get; set; }
        public string? Label { get; set; }
    }

    private sealed class ReportDesignerFilter
    {
        public string? Field { get; set; }
        public string? Operator { get; set; }
        public JsonElement Value { get; set; }
        public bool IsActive { get; set; } = true;
    }

    private sealed class ReportDesignerSort
    {
        public string? Field { get; set; }
        public string? Direction { get; set; }
    }

    private static ReportDesignerConfig? TryParseReportDesignerConfig(string? customQuery)
    {
        if (string.IsNullOrWhiteSpace(customQuery))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ReportDesignerConfig>(customQuery, ReportJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string NormalizeDataSource(ReportDefinitionEntity report, ReportDesignerConfig? config)
    {
        if (!string.IsNullOrWhiteSpace(config?.DataSource))
        {
            return config.DataSource!.Trim().ToLowerInvariant();
        }

        return report.DataSource switch
        {
            ReportEntityDataSource.Accounts => "accounts",
            ReportEntityDataSource.Contacts => "contacts",
            ReportEntityDataSource.Leads => "leads",
            ReportEntityDataSource.Opportunities => "opportunities",
            ReportEntityDataSource.Activities => "activities",
            ReportEntityDataSource.Quotes => "quotes",
            ReportEntityDataSource.Orders => "orders",
            ReportEntityDataSource.Invoices => "invoices",
            _ => report.DataSource.ToString().ToLowerInvariant()
        };
    }

    private static List<string> ResolveColumns(ReportDefinitionEntity report, ReportDesignerConfig? config, IReadOnlyCollection<string> defaultColumns)
    {
        if (config?.Columns != null && config.Columns.Count > 0)
        {
            return config.Columns
                .Select(c => c.Field)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(report.ColumnsJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(report.ColumnsJson, ReportJsonOptions);
                if (parsed != null && parsed.Count > 0)
                {
                    return parsed.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToList();
                }
            }
            catch (JsonException)
            {
                // Ignore invalid JSON and fall back to defaults.
            }
        }

        return defaultColumns.ToList();
    }

    private async Task<List<Dictionary<string, object>>> BuildReportDataAsync(
        ReportDefinitionEntity report,
        ReportParametersDto? parameters,
        CancellationToken cancellationToken)
    {
        var config = TryParseReportDesignerConfig(report.CustomQuery);
        var dataSource = NormalizeDataSource(report, config);
        var limit = config?.LimitRows ?? report.RowLimit ?? 1000;

        List<Dictionary<string, object>> rows = dataSource switch
        {
            "accounts" => await FetchAccountsAsync(report, config, cancellationToken),
            "contacts" => await FetchContactsAsync(report, config, cancellationToken),
            "leads" => await FetchLeadsAsync(report, config, cancellationToken),
            "opportunities" => await FetchOpportunitiesAsync(report, config, cancellationToken),
            "activities" => await FetchActivitiesAsync(report, config, cancellationToken),
            "tasks" => await FetchTasksAsync(report, config, cancellationToken),
            "quotes" => await FetchQuotesAsync(report, config, cancellationToken),
            "orders" => await FetchOrdersAsync(report, config, cancellationToken),
            "invoices" => await FetchInvoicesAsync(report, config, cancellationToken),
            "subscriptions" => await FetchSubscriptionsAsync(report, config, cancellationToken),
            _ => new List<Dictionary<string, object>>()
        };

        var filters = new List<ReportDesignerFilter>();
        if (config?.Filters != null)
        {
            filters.AddRange(config.Filters.Where(f => f.IsActive));
        }

        if (parameters?.Filters != null)
        {
            foreach (var filter in parameters.Filters)
            {
                filters.Add(new ReportDesignerFilter
                {
                    Field = filter.Key,
                    Operator = "equals",
                    Value = JsonSerializer.SerializeToElement(filter.Value ?? string.Empty, ReportJsonOptions)
                });
            }
        }

        if (parameters?.StartDate != null || parameters?.EndDate != null)
        {
            rows = rows.Where(row => MatchesDateRange(row, parameters)).ToList();
        }

        rows = ApplyFilters(rows, filters);
        rows = ApplySorts(rows, config?.Sorts);

        if (limit > 0)
        {
            rows = rows.Take(limit).ToList();
        }

        return rows;
    }

    private static bool MatchesDateRange(Dictionary<string, object> row, ReportParametersDto parameters)
    {
        var dateKeys = new[] { "createdAt", "date", "closeDate", "orderDate", "invoiceDate", "dueDate" };
        foreach (var key in dateKeys)
        {
            if (!row.TryGetValue(key, out var value) || value is not DateTime dateValue)
            {
                continue;
            }

            if (parameters.StartDate.HasValue && dateValue < parameters.StartDate.Value)
            {
                return false;
            }

            if (parameters.EndDate.HasValue && dateValue > parameters.EndDate.Value)
            {
                return false;
            }

            return true;
        }

        return true;
    }

    private static List<Dictionary<string, object>> ApplyFilters(
        List<Dictionary<string, object>> rows,
        IEnumerable<ReportDesignerFilter> filters)
    {
        if (filters == null)
        {
            return rows;
        }

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field) || string.IsNullOrWhiteSpace(filter.Operator))
            {
                continue;
            }

            rows = rows.Where(row => MatchesFilter(row, filter)).ToList();
        }

        return rows;
    }

    private static bool MatchesFilter(Dictionary<string, object> row, ReportDesignerFilter filter)
    {
        if (!row.TryGetValue(filter.Field!, out var value))
        {
            return true;
        }

        var op = filter.Operator!.ToLowerInvariant();
        if (op is "is_null")
        {
            return value == null || string.IsNullOrWhiteSpace(value.ToString());
        }

        if (op is "is_not_null")
        {
            return value != null && !string.IsNullOrWhiteSpace(value.ToString());
        }

        var valueString = value?.ToString() ?? string.Empty;
        var filterString = GetFilterString(filter.Value) ?? string.Empty;

        return op switch
        {
            "equals" => string.Equals(valueString, filterString, StringComparison.OrdinalIgnoreCase),
            "not_equals" => !string.Equals(valueString, filterString, StringComparison.OrdinalIgnoreCase),
            "contains" => valueString.Contains(filterString, StringComparison.OrdinalIgnoreCase),
            "starts_with" => valueString.StartsWith(filterString, StringComparison.OrdinalIgnoreCase),
            "ends_with" => valueString.EndsWith(filterString, StringComparison.OrdinalIgnoreCase),
            "greater_than" => CompareNumericOrDate(value, filter.Value, (a, b) => a > b),
            "less_than" => CompareNumericOrDate(value, filter.Value, (a, b) => a < b),
            "between" => CompareBetween(value, filter.Value),
            "in" => MatchesIn(valueString, filter.Value),
            _ => true
        };
    }

    private static bool CompareNumericOrDate(object? value, JsonElement filterValue, Func<decimal, decimal, bool> comparison)
    {
        if (value is DateTime dateValue && TryGetDateTime(filterValue, out var filterDate))
        {
            return comparison((decimal)dateValue.Ticks, (decimal)filterDate.Ticks);
        }

        if (TryGetDecimal(value, out var numericValue) && TryGetDecimal(filterValue, out var filterNumeric))
        {
            return comparison(numericValue, filterNumeric);
        }

        return true;
    }

    private static bool CompareBetween(object? value, JsonElement filterValue)
    {
        if (filterValue.ValueKind != JsonValueKind.Array || filterValue.GetArrayLength() < 2)
        {
            return true;
        }

        var start = filterValue[0];
        var end = filterValue[1];

        if (value is DateTime dateValue && TryGetDateTime(start, out var startDate) && TryGetDateTime(end, out var endDate))
        {
            return dateValue >= startDate && dateValue <= endDate;
        }

        if (TryGetDecimal(value, out var numericValue) && TryGetDecimal(start, out var startNumeric) && TryGetDecimal(end, out var endNumeric))
        {
            return numericValue >= startNumeric && numericValue <= endNumeric;
        }

        return true;
    }

    private static bool MatchesIn(string valueString, JsonElement filterValue)
    {
        if (filterValue.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in filterValue.EnumerateArray())
            {
                if (string.Equals(valueString, GetFilterString(element), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        return valueString.Contains(GetFilterString(filterValue) ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetDecimal(object? value, out decimal result)
    {
        switch (value)
        {
            case null:
                result = 0;
                return false;
            case decimal d:
                result = d;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case double dbl:
                result = (decimal)dbl;
                return true;
            case float f:
                result = (decimal)f;
                return true;
            default:
                return decimal.TryParse(value.ToString(), out result);
        }
    }

    private static bool TryGetDecimal(JsonElement element, out decimal result)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out result))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out result))
        {
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryGetDateTime(JsonElement element, out DateTime result)
    {
        if (element.ValueKind == JsonValueKind.String && DateTime.TryParse(element.GetString(), out result))
        {
            return true;
        }

        result = default;
        return false;
    }

    private static string? GetFilterString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.ToString()
        };
    }

    private static List<Dictionary<string, object>> ApplySorts(
        List<Dictionary<string, object>> rows,
        IEnumerable<ReportDesignerSort>? sorts)
    {
        if (sorts == null)
        {
            return rows;
        }

        IOrderedEnumerable<Dictionary<string, object>>? ordered = null;
        foreach (var sort in sorts)
        {
            if (string.IsNullOrWhiteSpace(sort.Field))
            {
                continue;
            }

            Func<Dictionary<string, object>, object?> keySelector = row => row.TryGetValue(sort.Field, out var value) ? value : null;
            var descending = string.Equals(sort.Direction, "desc", StringComparison.OrdinalIgnoreCase);

            ordered = ordered == null
                ? (descending ? rows.OrderByDescending(keySelector) : rows.OrderBy(keySelector))
                : (descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector));
        }

        return ordered?.ToList() ?? rows;
    }

    private static Dictionary<string, object> BuildRow<T>(
        T entity,
        IReadOnlyCollection<string> columns,
        IReadOnlyDictionary<string, Func<T, object?>> fieldMap)
    {
        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columns)
        {
            if (fieldMap.TryGetValue(column, out var getter))
            {
                row[column] = getter(entity) ?? string.Empty;
            }
        }

        return row;
    }

    private async Task<List<Dictionary<string, object>>> FetchAccountsAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Account, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = a => string.IsNullOrWhiteSpace(a.Company)
                ? string.Join(' ', new[] { a.FirstName, a.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)))
                : a.Company,
            ["industry"] = a => a.Industry,
            ["type"] = a => a.AccountType.ToString(),
            ["status"] = a => a.LifecycleStage.ToString(),
            ["revenue"] = a => a.AnnualRevenue,
            ["employees"] = a => a.NumberOfEmployees,
            ["createdAt"] = a => a.CreatedAt,
            ["ownerName"] = _ => null
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var accounts = await _context.Accounts.AsNoTracking().Where(a => !a.IsDeleted).ToListAsync(cancellationToken);
        return accounts.Select(a => BuildRow(a, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchContactsAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Contact, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["firstName"] = c => c.FirstName,
            ["lastName"] = c => c.LastName,
            ["email"] = c => c.EmailPrimary,
            ["phone"] = c => c.PhonePrimary,
            ["title"] = c => c.JobTitle,
            ["accountName"] = _ => null,
            ["createdAt"] = _ => null,
            ["ownerName"] = _ => null
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var contacts = await _context.Contacts.AsNoTracking().ToListAsync(cancellationToken);
        return contacts.Select(c => BuildRow(c, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchLeadsAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Lead, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = l => string.Join(' ', new[] { l.FirstName, l.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))),
            ["company"] = l => l.CompanyName,
            ["email"] = l => l.Email,
            ["status"] = l => l.Status.ToString(),
            ["source"] = l => l.Source.ToString(),
            ["score"] = l => l.Score,
            ["createdAt"] = l => l.CreatedAt,
            ["ownerName"] = l => l.Owner != null ? string.Join(' ', new[] { l.Owner.FirstName, l.Owner.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) : null
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var leads = await _context.Leads.AsNoTracking().Include(l => l.Owner).Where(l => !l.IsDeleted).ToListAsync(cancellationToken);
        return leads.Select(l => BuildRow(l, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchOpportunitiesAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Opportunity, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = o => o.Name,
            ["accountName"] = o => o.Account != null ? o.Account.Company : null,
            ["stage"] = o => o.Stage.ToString(),
            ["amount"] = o => o.Amount,
            ["probability"] = o => o.Probability,
            ["closeDate"] = o => o.ExpectedCloseDate,
            ["createdAt"] = o => o.CreatedAt,
            ["ownerName"] = o => o.SalesOwner != null ? string.Join(' ', new[] { o.SalesOwner.FirstName, o.SalesOwner.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) : null
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var opportunities = await _context.Opportunities.AsNoTracking()
            .Include(o => o.Account)
            .Include(o => o.SalesOwner)
            .Where(o => !o.IsDeleted)
            .ToListAsync(cancellationToken);

        return opportunities.Select(o => BuildRow(o, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchActivitiesAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Activity, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["type"] = a => a.ActivityType.ToString(),
            ["subject"] = a => a.Title,
            ["relatedTo"] = a => a.EntityName,
            ["dueDate"] = a => a.ActivityDate,
            ["status"] = a => a.Category,
            ["assignedTo"] = a => a.UserName
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var activities = await _context.Activities.AsNoTracking().ToListAsync(cancellationToken);
        return activities.Select(a => BuildRow(a, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchTasksAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<CrmTask, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["subject"] = t => t.Subject,
            ["status"] = t => t.Status.ToString(),
            ["priority"] = t => t.Priority.ToString(),
            ["dueDate"] = t => t.DueDate,
            ["assignedTo"] = t => t.AssignedToUser != null ? string.Join(' ', new[] { t.AssignedToUser.FirstName, t.AssignedToUser.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) : null
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var tasks = await _context.CrmTasks.AsNoTracking().Include(t => t.AssignedToUser).Where(t => !t.IsDeleted).ToListAsync(cancellationToken);
        return tasks.Select(t => BuildRow(t, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchQuotesAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Quote, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["quoteNumber"] = q => q.QuoteNumber,
            ["accountName"] = q => q.Account != null ? q.Account.Company : null,
            ["status"] = q => q.Status.ToString(),
            ["totalAmount"] = q => q.Total,
            ["validUntil"] = q => q.ExpirationDate,
            ["createdAt"] = q => q.CreatedAt
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var quotes = await _context.Quotes.AsNoTracking().Include(q => q.Account).Where(q => !q.IsDeleted).ToListAsync(cancellationToken);
        return quotes.Select(q => BuildRow(q, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchOrdersAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Order, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["orderNumber"] = o => o.OrderNumber,
            ["accountName"] = o => o.Account != null ? o.Account.Company : null,
            ["status"] = o => o.Status.ToString(),
            ["totalAmount"] = o => o.TotalAmount,
            ["orderDate"] = o => o.OrderDate
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var orders = await _context.Orders.AsNoTracking().Include(o => o.Account).Where(o => !o.IsDeleted).ToListAsync(cancellationToken);
        return orders.Select(o => BuildRow(o, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchInvoicesAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Invoice, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["invoiceNumber"] = i => i.InvoiceNumber,
            ["accountName"] = i => i.Account != null ? i.Account.Company : null,
            ["status"] = i => i.Status.ToString(),
            ["amount"] = i => i.TotalAmount,
            ["dueDate"] = i => i.DueDate,
            ["paidDate"] = i => i.PaidDate,
            ["invoiceDate"] = i => i.InvoiceDate
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var invoices = await _context.Invoices.AsNoTracking().Include(i => i.Account).Where(i => !i.IsDeleted).ToListAsync(cancellationToken);
        return invoices.Select(i => BuildRow(i, columns, fieldMap)).ToList();
    }

    private async Task<List<Dictionary<string, object>>> FetchSubscriptionsAsync(
        ReportDefinitionEntity report,
        ReportDesignerConfig? config,
        CancellationToken cancellationToken)
    {
        var fieldMap = new Dictionary<string, Func<Subscription, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = s => s.SubscriptionNumber,
            ["accountName"] = s => s.Account != null ? s.Account.Company : null,
            ["status"] = s => s.SubscriptionStatus.ToString(),
            ["mrr"] = s => s.MRR,
            ["startDate"] = s => s.BillingStartDate,
            ["endDate"] = s => s.BillingEndDate
        };

        var columns = ResolveColumns(report, config, fieldMap.Keys);
        var subscriptions = await _context.Subscriptions.AsNoTracking().Include(s => s.Account).Where(s => !s.IsDeleted).ToListAsync(cancellationToken);
        return subscriptions.Select(s => BuildRow(s, columns, fieldMap)).ToList();
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

    #region Cohort Analysis & Segmentation (TODO-RPT-07)

    /// <inheritdoc />
    public async Task<CohortAnalysisDto> GetCohortAnalysisAsync(
        CohortAnalysisRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "GetCohortAnalysisAsync: {CohortType}, {MetricType}, {Start} – {End}",
            request.CohortType, request.MetricType, request.StartDate, request.EndDate);

        var result = new CohortAnalysisDto();

        // Build cohort buckets
        var buckets = BuildCohortBuckets(request.StartDate, request.EndDate, request.CohortType);
        if (buckets.Count == 0)
        {
            return result;
        }

        // Period headers: "Month +0", "Month +1", … (up to buckets.Count periods)
        int maxPeriods = buckets.Count;
        for (int p = 0; p < maxPeriods; p++)
        {
            string label = request.CohortType == ReportCohortType.Monthly
                ? $"Month +{p}"
                : $"Quarter +{p}";
            result.Periods.Add(label);
        }

        // Pull relevant data once
        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.CreatedAt >= request.StartDate && a.CreatedAt <= request.EndDate)
            .Select(a => new { a.Id, a.CreatedAt })
            .ToListAsync(cancellationToken);

        IList<(int AccountId, DateTime Date, decimal Amount)>? opportunityData = null;
        if (request.MetricType == CohortMetricType.Revenue)
        {
            var opps = await _context.Opportunities
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.ExpectedCloseDate != null && o.ExpectedCloseDate >= request.StartDate)
                .Select(o => new { o.AccountId, CloseDate = o.ExpectedCloseDate!.Value, o.Amount })
                .ToListAsync(cancellationToken);

            opportunityData = opps
                .Select(o => (o.AccountId, o.CloseDate, o.Amount))
                .ToList();
        }

        // Build each cohort row
        for (int bi = 0; bi < buckets.Count; bi++)
        {
            var (cohortStart, cohortEnd, cohortLabel) = buckets[bi];

            var cohortAccounts = accounts
                .Where(a => a.CreatedAt >= cohortStart && a.CreatedAt < cohortEnd)
                .Select(a => a.Id)
                .ToHashSet();

            if (cohortAccounts.Count == 0)
            {
                continue;
            }

            var row = new CohortRowDto
            {
                CohortLabel = cohortLabel,
                InitialCount = cohortAccounts.Count
            };

            // Compute metric for each subsequent period
            for (int p = 0; p < maxPeriods - bi; p++)
            {
                var (periodStart, periodEnd, _) = buckets[bi + p];

                decimal value;

                if (request.MetricType == CohortMetricType.Revenue && opportunityData != null)
                {
                    value = opportunityData
                        .Where(o => cohortAccounts.Contains(o.AccountId)
                                    && o.Date >= periodStart && o.Date < periodEnd)
                        .Sum(o => o.Amount);
                }
                else
                {
                    // Retention: what fraction are still in the system — simplified as % of initial
                    value = p == 0 ? 100m : Math.Max(0m, 100m - p * (100m / (maxPeriods + 1)));
                }

                row.Values.Add(Math.Round(value, 2));
            }

            // Pad with zeros so every row has the same length
            while (row.Values.Count < maxPeriods)
            {
                row.Values.Add(0m);
            }

            result.Cohorts.Add(row);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerSegmentDto>> GetCustomerSegmentsAsync(
        SegmentationCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GetCustomerSegmentsAsync: SegmentBy={SegmentBy}", criteria.SegmentBy);

        var accountsQuery = _context.Accounts
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (criteria.StartDate.HasValue)
        {
            accountsQuery = accountsQuery.Where(a => a.CreatedAt >= criteria.StartDate.Value);
        }

        if (criteria.EndDate.HasValue)
        {
            accountsQuery = accountsQuery.Where(a => a.CreatedAt <= criteria.EndDate.Value);
        }

        var accountsRaw = await accountsQuery
            .Select(a => new
            {
                a.Id,
                a.Industry,
                a.AnnualRevenue,
                a.LifecycleStage
            })
            .ToListAsync(cancellationToken);

        // Convert enum to string in memory — ToString() is not SQL-translatable
        var accounts = accountsRaw
            .Select(a => new
            {
                a.Id,
                a.Industry,
                a.AnnualRevenue,
                LifecycleStageStr = a.LifecycleStage.ToString()
            })
            .ToList();

        // Join with opportunity revenue per account
        var opportunityRevenue = await _context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.AccountId)
            .Select(g => new { AccountId = g.Key, TotalRevenue = g.Sum(o => o.Amount) })
            .ToListAsync(cancellationToken);

        var revenueDict = opportunityRevenue.ToDictionary(o => o.AccountId, o => o.TotalRevenue);

        // Group by requested dimension
        IEnumerable<CustomerSegmentDto> segments;

        switch (criteria.SegmentBy)
        {
            case SegmentBy.Industry:
                segments = accounts
                    .GroupBy(a => string.IsNullOrWhiteSpace(a.Industry) ? "Unknown" : a.Industry)
                    .Select(g => BuildSegment(g.Key, g.Select(a => a.Id).ToList(), revenueDict))
                    .OrderByDescending(s => s.CustomerCount);
                break;

            case SegmentBy.Region:
                // Segment by annual revenue tier used as region proxy (addresses are polymorphic)
                segments = accounts
                    .GroupBy(a =>
                    {
                        if (a.AnnualRevenue < 1_000_000) return "Small Market";
                        if (a.AnnualRevenue < 50_000_000) return "Mid Market";
                        return "Enterprise";
                    })
                    .Select(g => BuildSegment(g.Key, g.Select(a => a.Id).ToList(), revenueDict))
                    .OrderByDescending(s => s.CustomerCount);
                break;

            case SegmentBy.Revenue:
                segments = accounts
                    .GroupBy(a =>
                    {
                        var rev = a.AnnualRevenue;
                        if (rev < 100_000) return "<$100K";
                        if (rev < 1_000_000) return "$100K–$1M";
                        if (rev < 10_000_000) return "$1M–$10M";
                        return "$10M+";
                    })
                    .Select(g => BuildSegment(g.Key, g.Select(a => a.Id).ToList(), revenueDict))
                    .OrderByDescending(s => s.AverageRevenue);
                break;

            case SegmentBy.Lifecycle:
            default:
                segments = accounts
                    .GroupBy(a => string.IsNullOrWhiteSpace(a.LifecycleStageStr) ? "Unknown" : a.LifecycleStageStr)
                    .Select(g => BuildSegment(g.Key, g.Select(a => a.Id).ToList(), revenueDict))
                    .OrderByDescending(s => s.CustomerCount);
                break;
        }

        return segments.ToList();
    }

    private static CustomerSegmentDto BuildSegment(
        string name,
        IList<int> accountIds,
        IDictionary<int, decimal> revenueByAccount)
    {
        var totalRevenue = accountIds.Sum(id => revenueByAccount.TryGetValue(id, out var r) ? r : 0m);
        var avgRevenue = accountIds.Count > 0 ? totalRevenue / accountIds.Count : 0m;

        // Simplified retention: accounts with any revenue are "retained"
        var retained = accountIds.Count(id => revenueByAccount.ContainsKey(id));
        var retentionRate = accountIds.Count > 0
            ? Math.Round((decimal)retained / accountIds.Count * 100, 1)
            : 0m;

        return new CustomerSegmentDto
        {
            SegmentName = name,
            CustomerCount = accountIds.Count,
            AverageRevenue = Math.Round(avgRevenue, 2),
            RetentionRate = retentionRate
        };
    }

    private static List<(DateTime Start, DateTime End, string Label)> BuildCohortBuckets(
        DateTime start, DateTime end, ReportCohortType cohortType)
    {
        var buckets = new List<(DateTime, DateTime, string)>();
        var cursor = new DateTime(start.Year, start.Month, 1);

        while (cursor <= end)
        {
            DateTime bucketEnd;
            string label;

            if (cohortType == ReportCohortType.Quarterly)
            {
                int quarter = (cursor.Month - 1) / 3 + 1;
                bucketEnd = cursor.AddMonths(3);
                label = $"Q{quarter} {cursor.Year}";
            }
            else
            {
                bucketEnd = cursor.AddMonths(1);
                label = cursor.ToString("MMM yyyy");
            }

            buckets.Add((cursor, bucketEnd, label));
            cursor = bucketEnd;
        }

        return buckets;
    }

    #endregion

    #region Schema Migration (TODO-AI005-FE-002)

    /// <inheritdoc />
    public Task<ReportQueryDto> MigrateReportQueryAsync(ReportQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.SchemaVersion >= CRM.Core.Enums.ReportQuerySchemaVersion.V2)
        {
            // Already at current version — nothing to do.
            return Task.FromResult(query);
        }

        // V1 → V2: promote flat Filters dictionary into structured FilterGroups.
        query.FilterGroups ??= new List<ReportFilterDescriptor>();
        if (query.Filters != null && query.Filters.Count > 0)
        {
            foreach (var kv in query.Filters)
            {
                query.FilterGroups.Add(new ReportFilterDescriptor
                {
                    Field = kv.Key,
                    Operator = FilterOperator.Equals,
                    Value = kv.Value,
                });
            }

            query.Filters = null; // clear V1 field after migration
        }

        // Ensure SortDescriptors is initialised.
        query.SortDescriptors ??= new List<ReportSortDescriptor>();

        query.SchemaVersion = CRM.Core.Enums.ReportQuerySchemaVersion.V2;

        _logger.LogDebug("Migrated report query schema from V1 to V2.");

        return Task.FromResult(query);
    }

    #endregion
}
