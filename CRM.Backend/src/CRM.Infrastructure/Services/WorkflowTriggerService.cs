// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.Text.Json;
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing workflow triggers.
/// </summary>
public class WorkflowTriggerService : IWorkflowTriggerService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowTriggerService> _logger;

    public WorkflowTriggerService(CrmDbContext context, ILogger<WorkflowTriggerService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<IEnumerable<WorkflowTriggerDto>> GetAllAsync(
        int? workflowDefinitionId = null,
        WorkflowTriggerType? triggerType = null,
        string? entityType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowTriggers
            .Include(t => t.WorkflowDefinition)
            .Include(t => t.CreatedBy)
            .Where(t => !t.IsDeleted);

        if (workflowDefinitionId.HasValue)
            query = query.Where(t => t.WorkflowDefinitionId == workflowDefinitionId.Value);

        if (triggerType.HasValue)
            query = query.Where(t => t.TriggerType == triggerType.Value);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(t => t.EntityType == entityType);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        var triggers = await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return triggers.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<WorkflowTriggerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .Include(t => t.WorkflowDefinition)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        return trigger == null ? null : MapToDto(trigger);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkflowTriggerDto>> GetByWorkflowAsync(int workflowDefinitionId, CancellationToken cancellationToken = default)
    {
        var triggers = await _context.WorkflowTriggers
            .Include(t => t.WorkflowDefinition)
            .Include(t => t.CreatedBy)
            .Where(t => t.WorkflowDefinitionId == workflowDefinitionId && !t.IsDeleted)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        return triggers.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<WorkflowTriggerDto> CreateAsync(CreateWorkflowTriggerDto dto, int? createdById = null, CancellationToken cancellationToken = default)
    {
        // Validate workflow exists
        var workflowExists = await _context.WorkflowDefinitions
            .AnyAsync(w => w.Id == dto.WorkflowDefinitionId && !w.IsDeleted, cancellationToken);

        if (!workflowExists)
            throw new ArgumentException($"Workflow definition {dto.WorkflowDefinitionId} not found.");

        // Validate trigger type specific requirements
        ValidateTriggerTypeRequirements(dto);

        var trigger = new WorkflowTrigger
        {
            WorkflowDefinitionId = dto.WorkflowDefinitionId,
            Name = dto.Name,
            TriggerType = dto.TriggerType,
            EntityType = dto.EntityType,
            EventName = dto.EventName,
            CronExpression = dto.CronExpression,
            FilterConditions = dto.FilterConditions,
            WatchedField = dto.WatchedField,
            OldValue = dto.OldValue,
            NewValue = dto.NewValue,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            Description = dto.Description,
            DelaySeconds = dto.DelaySeconds,
            RunAsync = dto.RunAsync,
            MaxRetries = dto.MaxRetries,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Calculate next scheduled time for scheduled triggers
        if (dto.TriggerType == WorkflowTriggerType.Scheduled && !string.IsNullOrEmpty(dto.CronExpression))
        {
            trigger.NextScheduledAt = CalculateNextScheduledTime(dto.CronExpression);
        }

        _context.WorkflowTriggers.Add(trigger);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created workflow trigger {TriggerId} ({TriggerName}) for workflow {WorkflowId}",
            trigger.Id, trigger.Name, trigger.WorkflowDefinitionId);

        await WriteConfigAuditLogAsync("TriggerCreated", trigger, createdById, new { dto.TriggerType, dto.EntityType, dto.EventName }, cancellationToken);

        return await GetByIdAsync(trigger.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve created trigger.");
    }

    /// <inheritdoc />
    public async Task<WorkflowTriggerDto> UpdateAsync(UpdateWorkflowTriggerDto dto, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == dto.Id && !t.IsDeleted, cancellationToken);

        if (trigger == null)
            throw new KeyNotFoundException($"Workflow trigger {dto.Id} not found.");

        // Update fields
        if (dto.Name != null) trigger.Name = dto.Name;
        if (dto.TriggerType.HasValue) trigger.TriggerType = dto.TriggerType.Value;
        if (dto.EntityType != null) trigger.EntityType = dto.EntityType;
        if (dto.EventName != null) trigger.EventName = dto.EventName;
        if (dto.CronExpression != null) trigger.CronExpression = dto.CronExpression;
        if (dto.FilterConditions != null) trigger.FilterConditions = dto.FilterConditions;
        if (dto.WatchedField != null) trigger.WatchedField = dto.WatchedField;
        if (dto.OldValue != null) trigger.OldValue = dto.OldValue;
        if (dto.NewValue != null) trigger.NewValue = dto.NewValue;
        if (dto.IsActive.HasValue) trigger.IsActive = dto.IsActive.Value;
        if (dto.Priority.HasValue) trigger.Priority = dto.Priority.Value;
        if (dto.Description != null) trigger.Description = dto.Description;
        if (dto.DelaySeconds.HasValue) trigger.DelaySeconds = dto.DelaySeconds.Value;
        if (dto.RunAsync.HasValue) trigger.RunAsync = dto.RunAsync.Value;
        if (dto.MaxRetries.HasValue) trigger.MaxRetries = dto.MaxRetries.Value;

        trigger.UpdatedAt = DateTime.UtcNow;

        // Recalculate next scheduled time if cron changed
        if (dto.CronExpression != null && trigger.TriggerType == WorkflowTriggerType.Scheduled)
        {
            trigger.NextScheduledAt = CalculateNextScheduledTime(trigger.CronExpression);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated workflow trigger {TriggerId} ({TriggerName})", trigger.Id, trigger.Name);

        await WriteConfigAuditLogAsync("TriggerUpdated", trigger, null, new { UpdatedFields = dto }, cancellationToken);

        return await GetByIdAsync(trigger.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve updated trigger.");
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (trigger == null)
            return false;

        trigger.IsDeleted = true;
        trigger.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted workflow trigger {TriggerId} ({TriggerName})", trigger.Id, trigger.Name);

        await WriteConfigAuditLogAsync("TriggerDeleted", trigger, null, new { trigger.TriggerType, trigger.EntityType, trigger.EventName }, cancellationToken);

        return true;
    }

    #endregion

    #region Activation

    /// <inheritdoc />
    public async Task<WorkflowTriggerDto> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (trigger == null)
            throw new KeyNotFoundException($"Workflow trigger {id} not found.");

        trigger.IsActive = true;
        trigger.UpdatedAt = DateTime.UtcNow;

        // Update next scheduled time for scheduled triggers
        if (trigger.TriggerType == WorkflowTriggerType.Scheduled && !string.IsNullOrEmpty(trigger.CronExpression))
        {
            trigger.NextScheduledAt = CalculateNextScheduledTime(trigger.CronExpression);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated workflow trigger {TriggerId} ({TriggerName})", trigger.Id, trigger.Name);

        await WriteConfigAuditLogAsync("TriggerActivated", trigger, null, null, cancellationToken);

        return await GetByIdAsync(trigger.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve trigger.");
    }

    /// <inheritdoc />
    public async Task<WorkflowTriggerDto> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (trigger == null)
            throw new KeyNotFoundException($"Workflow trigger {id} not found.");

        trigger.IsActive = false;
        trigger.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated workflow trigger {TriggerId} ({TriggerName})", trigger.Id, trigger.Name);

        await WriteConfigAuditLogAsync("TriggerDeactivated", trigger, null, null, cancellationToken);

        return await GetByIdAsync(trigger.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve trigger.");
    }

    #endregion

    #region Trigger Execution

    /// <inheritdoc />
    public async Task<TriggerExecutionResult> EvaluateTriggersAsync(TriggerExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var result = new TriggerExecutionResult { Success = true };

        try
        {
            // Get matching triggers
            var triggers = await _context.WorkflowTriggers
                .Include(t => t.WorkflowDefinition)
                .Where(t => !t.IsDeleted && t.IsActive
                    && t.EntityType == request.EntityType
                    && t.TriggerType == request.TriggerType)
                .OrderBy(t => t.Priority)
                .ToListAsync(cancellationToken);

            // Filter by event name if applicable
            if (request.TriggerType == WorkflowTriggerType.OnEvent && !string.IsNullOrEmpty(request.EventName))
            {
                triggers = triggers.Where(t => t.EventName == request.EventName).ToList();
            }

            // Filter by watched field if applicable
            if (request.TriggerType == WorkflowTriggerType.OnFieldChange && !string.IsNullOrEmpty(request.ChangedField))
            {
                triggers = triggers.Where(t =>
                    string.IsNullOrEmpty(t.WatchedField) || t.WatchedField == request.ChangedField).ToList();
            }

            foreach (var trigger in triggers)
            {
                var triggerResult = new TriggerResult
                {
                    TriggerId = trigger.Id,
                    TriggerName = trigger.Name,
                    WorkflowDefinitionId = trigger.WorkflowDefinitionId,
                    WorkflowName = trigger.WorkflowDefinition?.Name ?? "Unknown"
                };

                try
                {
                    // Check filter conditions
                    if (!await EvaluateFilterConditionsAsync(trigger, request, cancellationToken))
                    {
                        triggerResult.Matched = false;
                        triggerResult.SkippedReason = "Filter conditions not met";
                        result.TriggerResults.Add(triggerResult);
                        continue;
                    }

                    // Check field value conditions for OnFieldChange
                    if (trigger.TriggerType == WorkflowTriggerType.OnFieldChange)
                    {
                        if (!string.IsNullOrEmpty(trigger.OldValue) && trigger.OldValue != request.OldValue)
                        {
                            triggerResult.Matched = false;
                            triggerResult.SkippedReason = "Old value doesn't match";
                            result.TriggerResults.Add(triggerResult);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(trigger.NewValue) && trigger.NewValue != request.NewValue)
                        {
                            triggerResult.Matched = false;
                            triggerResult.SkippedReason = "New value doesn't match";
                            result.TriggerResults.Add(triggerResult);
                            continue;
                        }
                    }

                    triggerResult.Matched = true;

                    // Execute the workflow (create a workflow instance)
                    var instanceId = await StartWorkflowAsync(trigger, request.EntityId, request.InitiatedById, cancellationToken);

                    triggerResult.Executed = true;
                    triggerResult.WorkflowInstanceId = instanceId;
                    result.WorkflowInstanceIds.Add(instanceId);
                    result.WorkflowsTriggered++;

                    // Record execution
                    await RecordTriggerExecutionAsync(trigger.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing trigger {TriggerId}", trigger.Id);
                    triggerResult.Matched = true;
                    triggerResult.Executed = false;
                    triggerResult.ErrorMessage = ex.Message;
                    result.Errors.Add($"Trigger {trigger.Id}: {ex.Message}");
                }

                result.TriggerResults.Add(triggerResult);
            }

            if (result.Errors.Count > 0 && result.WorkflowsTriggered == 0)
                result.Success = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating triggers for {EntityType} {EntityId}", request.EntityType, request.EntityId);
            result.Success = false;
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TriggerExecutionResult> FireTriggerAsync(int triggerId, int entityId, int? initiatedById = null, CancellationToken cancellationToken = default)
    {
        var result = new TriggerExecutionResult { Success = true };

        var trigger = await _context.WorkflowTriggers
            .Include(t => t.WorkflowDefinition)
            .FirstOrDefaultAsync(t => t.Id == triggerId && !t.IsDeleted, cancellationToken);

        if (trigger == null)
        {
            result.Success = false;
            result.Errors.Add($"Trigger {triggerId} not found.");
            return result;
        }

        var triggerResult = new TriggerResult
        {
            TriggerId = trigger.Id,
            TriggerName = trigger.Name,
            WorkflowDefinitionId = trigger.WorkflowDefinitionId,
            WorkflowName = trigger.WorkflowDefinition?.Name ?? "Unknown",
            Matched = true
        };

        try
        {
            var instanceId = await StartWorkflowAsync(trigger, entityId, initiatedById, cancellationToken);

            triggerResult.Executed = true;
            triggerResult.WorkflowInstanceId = instanceId;
            result.WorkflowInstanceIds.Add(instanceId);
            result.WorkflowsTriggered = 1;

            await RecordTriggerExecutionAsync(trigger.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error firing trigger {TriggerId}", triggerId);
            triggerResult.Executed = false;
            triggerResult.ErrorMessage = ex.Message;
            result.Success = false;
            result.Errors.Add(ex.Message);
        }

        result.TriggerResults.Add(triggerResult);
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkflowTriggerDto>> GetMatchingTriggersAsync(
        string entityType,
        WorkflowTriggerType triggerType,
        string? eventName = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowTriggers
            .Include(t => t.WorkflowDefinition)
            .Include(t => t.CreatedBy)
            .Where(t => !t.IsDeleted && t.IsActive
                && t.EntityType == entityType
                && t.TriggerType == triggerType);

        if (!string.IsNullOrEmpty(eventName))
            query = query.Where(t => t.EventName == eventName);

        var triggers = await query
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        return triggers.Select(MapToDto);
    }

    #endregion

    #region Scheduled Triggers

    /// <inheritdoc />
    public async Task<IEnumerable<WorkflowTriggerDto>> GetScheduledTriggersDueAsync(DateTime asOfTime, CancellationToken cancellationToken = default)
    {
        var triggers = await _context.WorkflowTriggers
            .Include(t => t.WorkflowDefinition)
            .Include(t => t.CreatedBy)
            .Where(t => !t.IsDeleted && t.IsActive
                && t.TriggerType == WorkflowTriggerType.Scheduled
                && t.NextScheduledAt != null
                && t.NextScheduledAt <= asOfTime)
            .OrderBy(t => t.NextScheduledAt)
            .ToListAsync(cancellationToken);

        return triggers.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task UpdateNextScheduledTimeAsync(int triggerId, DateTime nextTime, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == triggerId && !t.IsDeleted, cancellationToken);

        if (trigger != null)
        {
            trigger.NextScheduledAt = nextTime;
            trigger.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task RecordTriggerExecutionAsync(int triggerId, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == triggerId && !t.IsDeleted, cancellationToken);

        if (trigger != null)
        {
            trigger.LastTriggeredAt = DateTime.UtcNow;
            trigger.ExecutionCount++;
            trigger.UpdatedAt = DateTime.UtcNow;

            // Update next scheduled time for scheduled triggers
            if (trigger.TriggerType == WorkflowTriggerType.Scheduled && !string.IsNullOrEmpty(trigger.CronExpression))
            {
                trigger.NextScheduledAt = CalculateNextScheduledTime(trigger.CronExpression);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Statistics

    /// <inheritdoc />
    public async Task<TriggerStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var triggers = await _context.WorkflowTriggers
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);

        return new TriggerStatisticsDto
        {
            TotalTriggers = triggers.Count,
            ActiveTriggers = triggers.Count(t => t.IsActive),
            InactiveTriggers = triggers.Count(t => !t.IsActive),
            ScheduledTriggers = triggers.Count(t => t.TriggerType == WorkflowTriggerType.Scheduled),
            RecordTriggers = triggers.Count(t => t.TriggerType == WorkflowTriggerType.OnCreate
                || t.TriggerType == WorkflowTriggerType.OnUpdate
                || t.TriggerType == WorkflowTriggerType.OnDelete
                || t.TriggerType == WorkflowTriggerType.OnFieldChange),
            EventTriggers = triggers.Count(t => t.TriggerType == WorkflowTriggerType.OnEvent),
            TotalExecutions = triggers.Sum(t => t.ExecutionCount),
            ExecutionsToday = 0, // Would need execution log table for this
            ExecutionsThisWeek = 0,
            LastExecutionAt = triggers.Where(t => t.LastTriggeredAt.HasValue)
                .OrderByDescending(t => t.LastTriggeredAt)
                .Select(t => t.LastTriggeredAt)
                .FirstOrDefault(),
            TriggersByType = triggers.GroupBy(t => t.TriggerType)
                .ToDictionary(g => g.Key, g => g.Count()),
            TriggersByEntityType = triggers.Where(t => !string.IsNullOrEmpty(t.EntityType))
                .GroupBy(t => t.EntityType!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    #endregion

    #region Validation

    /// <inheritdoc />
    public bool ValidateCronExpression(string cronExpression, out string? errorMessage)
    {
        try
        {
            var cron = CronExpression.Parse(cronExpression);
            errorMessage = null;
            return true;
        }
        catch (CronFormatException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    /// <inheritdoc />
    public bool ValidateFilterConditions(string filterConditions, out string? errorMessage)
    {
        try
        {
            JsonDocument.Parse(filterConditions);
            errorMessage = null;
            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    #endregion

    #region Private Methods

    private void ValidateTriggerTypeRequirements(CreateWorkflowTriggerDto dto)
    {
        switch (dto.TriggerType)
        {
            case WorkflowTriggerType.OnCreate:
            case WorkflowTriggerType.OnUpdate:
            case WorkflowTriggerType.OnDelete:
            case WorkflowTriggerType.OnFieldChange:
                if (string.IsNullOrEmpty(dto.EntityType))
                    throw new ArgumentException($"EntityType is required for {dto.TriggerType} triggers.");
                break;

            case WorkflowTriggerType.Scheduled:
                if (string.IsNullOrEmpty(dto.CronExpression))
                    throw new ArgumentException("CronExpression is required for Scheduled triggers.");
                if (!ValidateCronExpression(dto.CronExpression, out var cronError))
                    throw new ArgumentException($"Invalid cron expression: {cronError}");
                break;

            case WorkflowTriggerType.OnEvent:
                if (string.IsNullOrEmpty(dto.EventName))
                    throw new ArgumentException("EventName is required for OnEvent triggers.");
                break;
        }

        if (!string.IsNullOrEmpty(dto.FilterConditions) && !ValidateFilterConditions(dto.FilterConditions, out var filterError))
        {
            throw new ArgumentException($"Invalid filter conditions JSON: {filterError}");
        }
    }

    private DateTime? CalculateNextScheduledTime(string? cronExpression)
    {
        if (string.IsNullOrEmpty(cronExpression))
            return null;

        try
        {
            var cron = CronExpression.Parse(cronExpression);
            return cron.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Utc);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> EvaluateFilterConditionsAsync(WorkflowTrigger trigger, TriggerExecutionRequest request, CancellationToken cancellationToken)
    {
        // If no filter conditions, always match
        if (string.IsNullOrEmpty(trigger.FilterConditions))
            return true;

        // TODO: Implement dynamic filter condition evaluation
        // For now, return true if we have filter conditions (assume they match)
        // A full implementation would parse the JSON filter and evaluate against the entity
        _logger.LogDebug("Filter conditions not fully implemented, assuming match for trigger {TriggerId}", trigger.Id);
        return true;
    }

    private async Task<int> StartWorkflowAsync(WorkflowTrigger trigger, int entityId, int? initiatedById, CancellationToken cancellationToken)
    {
        // Get the active version of the workflow
        var workflow = await _context.WorkflowDefinitions
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == trigger.WorkflowDefinitionId && !w.IsDeleted, cancellationToken);

        if (workflow == null)
            throw new InvalidOperationException($"Workflow definition {trigger.WorkflowDefinitionId} not found.");

        if (workflow.Status != WorkflowStatus.Active)
            throw new InvalidOperationException($"Workflow {workflow.Name} is not active (Status: {workflow.Status}).");

        var activeVersion = workflow.Versions
            .Where(v => v.Status == WorkflowVersionStatus.Active && !v.IsDeleted)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();

        if (activeVersion == null)
            throw new InvalidOperationException($"Workflow {workflow.Name} has no active version.");

        // Create workflow instance
        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = workflow.Id,
            WorkflowVersionId = activeVersion.Id,
            EntityType = trigger.EntityType,
            EntityId = entityId,
            Status = WorkflowInstanceStatus.Running,
            StartedAt = DateTime.UtcNow,
            TriggeredById = initiatedById,
            TriggerEvent = trigger.TriggerType.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Started workflow instance {InstanceId} for workflow {WorkflowName} triggered by {TriggerName}",
            instance.Id, workflow.Name, trigger.Name);

        return instance.Id;
    }

    private static WorkflowTriggerDto MapToDto(WorkflowTrigger trigger)
    {
        return new WorkflowTriggerDto
        {
            Id = trigger.Id,
            WorkflowDefinitionId = trigger.WorkflowDefinitionId,
            WorkflowName = trigger.WorkflowDefinition?.Name,
            Name = trigger.Name,
            TriggerType = trigger.TriggerType,
            EntityType = trigger.EntityType,
            EventName = trigger.EventName,
            CronExpression = trigger.CronExpression,
            FilterConditions = trigger.FilterConditions,
            WatchedField = trigger.WatchedField,
            OldValue = trigger.OldValue,
            NewValue = trigger.NewValue,
            IsActive = trigger.IsActive,
            Priority = trigger.Priority,
            Description = trigger.Description,
            LastTriggeredAt = trigger.LastTriggeredAt,
            NextScheduledAt = trigger.NextScheduledAt,
            ExecutionCount = trigger.ExecutionCount,
            DelaySeconds = trigger.DelaySeconds,
            RunAsync = trigger.RunAsync,
            MaxRetries = trigger.MaxRetries,
            CreatedById = trigger.CreatedById,
            CreatedByName = trigger.CreatedBy != null ? $"{trigger.CreatedBy.FirstName} {trigger.CreatedBy.LastName}" : null,
            CreatedAt = trigger.CreatedAt,
            UpdatedAt = trigger.UpdatedAt
        };
    }

    #endregion

    #region Audit Logging

    /// <summary>
    /// Writes a configuration-level audit log entry for trigger CRUD operations.
    /// These logs have no associated workflow instance (WorkflowInstanceId = null).
    /// </summary>
    private async Task WriteConfigAuditLogAsync(
        string action,
        WorkflowTrigger trigger,
        int? userId,
        object? details,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new WorkflowLog
            {
                WorkflowInstanceId = null,
                Level = WorkflowLogLevel.Info,
                Category = "TriggerConfiguration",
                Message = $"{action}: {trigger.Name} (Id={trigger.Id}, WorkflowDef={trigger.WorkflowDefinitionId})",
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                UserId = userId ?? trigger.CreatedById,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkflowLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit logging should never block the main operation
            _logger.LogWarning(ex, "Failed to write audit log for trigger {Action} on {TriggerId}", action, trigger.Id);
        }
    }

    #endregion
}
