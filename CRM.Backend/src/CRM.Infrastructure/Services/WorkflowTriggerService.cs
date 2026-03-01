// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
        if (dto.Name != null)
            trigger.Name = dto.Name;
        if (dto.TriggerType.HasValue)
            trigger.TriggerType = dto.TriggerType.Value;
        if (dto.EntityType != null)
            trigger.EntityType = dto.EntityType;
        if (dto.EventName != null)
            trigger.EventName = dto.EventName;
        if (dto.CronExpression != null)
            trigger.CronExpression = dto.CronExpression;
        if (dto.FilterConditions != null)
            trigger.FilterConditions = dto.FilterConditions;
        if (dto.WatchedField != null)
            trigger.WatchedField = dto.WatchedField;
        if (dto.OldValue != null)
            trigger.OldValue = dto.OldValue;
        if (dto.NewValue != null)
            trigger.NewValue = dto.NewValue;
        if (dto.IsActive.HasValue)
            trigger.IsActive = dto.IsActive.Value;
        if (dto.Priority.HasValue)
            trigger.Priority = dto.Priority.Value;
        if (dto.Description != null)
            trigger.Description = dto.Description;
        if (dto.DelaySeconds.HasValue)
            trigger.DelaySeconds = dto.DelaySeconds.Value;
        if (dto.RunAsync.HasValue)
            trigger.RunAsync = dto.RunAsync.Value;
        if (dto.MaxRetries.HasValue)
            trigger.MaxRetries = dto.MaxRetries.Value;


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

            // Filter by event name if applicable (case-insensitive)
            if (request.TriggerType == WorkflowTriggerType.OnEvent && !string.IsNullOrEmpty(request.EventName))
            {
                triggers = triggers.Where(t => !string.IsNullOrEmpty(t.EventName) &&
                    string.Equals(t.EventName, request.EventName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filter by watched field if applicable (case-insensitive)
            if (request.TriggerType == WorkflowTriggerType.OnFieldChange && !string.IsNullOrEmpty(request.ChangedField))
            {
                triggers = triggers.Where(t =>
                    string.IsNullOrEmpty(t.WatchedField) || string.Equals(t.WatchedField, request.ChangedField, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var trigger in triggers)
            {
                // Defensive: ensure TriggerType maps to the enum
                if (!Enum.IsDefined(typeof(WorkflowTriggerType), trigger.TriggerType))
                {
                    _logger.LogWarning("Trigger {TriggerId}: TriggerType value '{TriggerType}' is not a valid WorkflowTriggerType - skipping.", trigger.Id, trigger.TriggerType);
                    var skipped = new TriggerResult
                    {
                        TriggerId = trigger.Id,
                        TriggerName = trigger.Name,
                        WorkflowDefinitionId = trigger.WorkflowDefinitionId,
                        WorkflowName = trigger.WorkflowDefinition?.Name ?? "Unknown",
                        Matched = false,
                        Executed = false,
                        SkippedReason = "Invalid TriggerType"
                    };

                    result.TriggerResults.Add(skipped);
                    continue;
                }
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
            query = query.Where(t => !string.IsNullOrEmpty(t.EventName) && t.EventName.ToLower() == eventName.ToLower());

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
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task RecordTriggerExecutionAsync(int triggerId, CancellationToken cancellationToken = default)
    {
        var trigger = await _context.WorkflowTriggers
            .FirstOrDefaultAsync(t => t.Id == triggerId && !t.IsDeleted, cancellationToken);

        if (trigger == null)
            throw new KeyNotFoundException($"Trigger with ID {triggerId} not found");

        trigger.LastTriggeredAt = DateTime.UtcNow;
        trigger.ExecutionCount++;

        // Update next scheduled time for scheduled triggers
        if (trigger.TriggerType == WorkflowTriggerType.Scheduled && !string.IsNullOrEmpty(trigger.CronExpression))
        {
            trigger.NextScheduledAt = CalculateNextScheduledTime(trigger.CronExpression);
        }

        await _context.SaveChangesAsync(cancellationToken);
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
            ExecutionsToday = triggers
                .Where(t => t.LastTriggeredAt.HasValue && t.LastTriggeredAt.Value >= today)
                .Sum(t => t.LastTriggeredAt!.Value.Date == today ? 1 : 0),
            ExecutionsThisWeek = triggers
                .Where(t => t.LastTriggeredAt.HasValue && t.LastTriggeredAt.Value >= weekAgo)
                .Count(),
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
            _ = CronExpression.Parse(cronExpression);
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
        await Task.CompletedTask;

        // If no filter conditions, always match
        if (string.IsNullOrEmpty(trigger.FilterConditions))
            return true;

        try
        {
            using var doc = JsonDocument.Parse(trigger.FilterConditions);
            var root = doc.RootElement;

            // Support both array of conditions and object with "conditions" array
            JsonElement conditions;
            if (root.ValueKind == JsonValueKind.Array)
            {
                conditions = root;
            }
            else if (root.TryGetProperty("conditions", out var condArray) && condArray.ValueKind == JsonValueKind.Array)
            {
                conditions = condArray;
            }
            else
            {
                _logger.LogWarning("Trigger {TriggerId}: FilterConditions JSON is not an array and has no 'conditions' property. Assuming match.", trigger.Id);
                return true;
            }

            // Determine logical operator (AND by default)
            var logicOperator = "and";
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("logic", out var logicProp))
                logicOperator = logicProp.GetString()?.ToLowerInvariant() ?? "and";

            // Parse context/entity data if present. Combine into a single, case-insensitive dictionary.
            Dictionary<string, string>? dataFields = null;
            try
            {
                dataFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // If TriggerExecutionRequest carries an EntityData property (some callers), prefer it.
                var entityDataProp = request.GetType().GetProperty("EntityData");
                if (entityDataProp != null)
                {
                    var val = entityDataProp.GetValue(request) as string;
                    if (!string.IsNullOrEmpty(val))
                    {
                        using var edoc = JsonDocument.Parse(val);
                        FlattenJson(edoc.RootElement, string.Empty, dataFields);
                    }
                }

                // ContextData is kept for backward compatibility
                if (!string.IsNullOrEmpty(request.ContextData))
                {
                    try
                    {
                        using var cdoc = JsonDocument.Parse(request.ContextData);
                        FlattenJson(cdoc.RootElement, string.Empty, dataFields);
                    }
                    catch
                    {
                        // ContextData might be a simple dictionary string; try deserialize
                        try
                        {
                            var simple = JsonSerializer.Deserialize<Dictionary<string, string>>(request.ContextData);
                            if (simple != null)
                            {
                                foreach (var kv in simple)
                                    dataFields[kv.Key] = kv.Value;
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }

                if (dataFields.Count == 0)
                    dataFields = null;
            }
            catch
            {
                dataFields = null;
            }

            // Evaluate each condition
            var results = new List<bool>();
            foreach (var condition in conditions.EnumerateArray())
            {
                var result = EvaluateSingleCondition(condition, request, dataFields);
                results.Add(result);
            }

            if (results.Count == 0)
                return true;

            var match = logicOperator == "or"
                ? results.Any(r => r)
                : results.All(r => r);

            _logger.LogDebug(
                "Trigger {TriggerId}: Evaluated {Count} filter conditions with '{Logic}' logic. Result: {Match}",
                trigger.Id, results.Count, logicOperator, match);

            return match;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Trigger {TriggerId}: Failed to parse FilterConditions JSON. Assuming match.", trigger.Id);
            return true;
        }
    }

    /// <summary>
    /// Evaluates a single filter condition against the trigger execution request.
    /// Expected JSON shape: { "field": "Status", "operator": "equals", "value": "Active" }
    /// </summary>
    private bool EvaluateSingleCondition(
        JsonElement condition,
        TriggerExecutionRequest request,
        Dictionary<string, string>? contextFields)
    {
        if (!condition.TryGetProperty("field", out var fieldProp) ||
            !condition.TryGetProperty("operator", out var opProp))
        {
            return true; // Malformed condition — skip (treat as match)
        }

        var fieldName = fieldProp.GetString() ?? string.Empty;
        var op = opProp.GetString() ?? "equals";
        var conditionValue = condition.TryGetProperty("value", out var valProp) ? valProp.GetString() : null;
        var conditionValue2 = condition.TryGetProperty("value2", out var val2Prop) ? val2Prop.GetString() : null;

        // Resolve the actual field value from the request
        var actualValue = ResolveFieldValue(fieldName, request, contextFields);

        return EvaluateOperator(op, actualValue, conditionValue, conditionValue2);
    }

    /// <summary>
    /// Resolves the value of a field from the trigger request.
    /// Checks ChangedField/NewValue first, then ContextData dictionary.
    /// </summary>
    private static string? ResolveFieldValue(
        string fieldName,
        TriggerExecutionRequest request,
        Dictionary<string, string>? contextFields)
    {
        // If the condition targets the changed field specifically, use OldValue/NewValue
        if (!string.IsNullOrEmpty(request.ChangedField) &&
            string.Equals(request.ChangedField, fieldName, StringComparison.OrdinalIgnoreCase))
        {
            return request.NewValue;
        }

        // Check context data for the field value
        if (contextFields != null)
        {
            // Try exact match first, then case-insensitive
            if (contextFields.TryGetValue(fieldName, out var val))
                return val;

            var key = contextFields.Keys.FirstOrDefault(k => string.Equals(k, fieldName, StringComparison.OrdinalIgnoreCase));
            if (key != null)
                return contextFields[key];
        }

        // Special built-in fields
        return fieldName.ToLowerInvariant() switch
        {
            "entitytype" => request.EntityType,
            "entityid" => request.EntityId.ToString(),
            "changedfield" => request.ChangedField,
            "oldvalue" => request.OldValue,
            "newvalue" => request.NewValue,
            "eventname" => request.EventName,
            _ => null
        };
    }

    /// <summary>
    /// Evaluates a condition operator against actual and expected values.
    /// Supports: equals, notEquals, contains, notContains, startsWith, endsWith,
    /// greaterThan, lessThan, greaterThanOrEqual, lessThanOrEqual,
    /// isNull, isNotNull, in, notIn, between, regex
    /// </summary>
    private static bool EvaluateOperator(string op, string? actual, string? expected, string? expected2)
    {
        switch (op.ToLowerInvariant())
        {
            case "equals":
            case "eq":
                return string.Equals(actual ?? "", expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "notequals":
            case "not_equals":
            case "neq":
                return !string.Equals(actual ?? "", expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "contains":
                return (actual ?? "").Contains(expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "notcontains":
            case "not_contains":
                return !(actual ?? "").Contains(expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "startswith":
            case "starts_with":
                return (actual ?? "").StartsWith(expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "endswith":
            case "ends_with":
                return (actual ?? "").EndsWith(expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "greaterthan":
            case "greater_than":
            case "gt":
                return CompareNumeric(actual, expected) > 0;

            case "lessthan":
            case "less_than":
            case "lt":
                return CompareNumeric(actual, expected) < 0;

            case "greaterthanorequal":
            case "greater_than_or_equal":
            case "gte":
                return CompareNumeric(actual, expected) >= 0;

            case "lessthanorequal":
            case "less_than_or_equal":
            case "lte":
                return CompareNumeric(actual, expected) <= 0;

            case "isnull":
            case "is_null":
            case "is_empty":
                return string.IsNullOrEmpty(actual);

            case "isnotnull":
            case "is_not_null":
            case "is_not_empty":
                return !string.IsNullOrEmpty(actual);

            case "in":
                if (string.IsNullOrEmpty(expected))
                    return false;
                var inValues = expected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return inValues.Any(v => string.Equals(v, actual, StringComparison.OrdinalIgnoreCase));

            case "notin":
            case "not_in":
                if (string.IsNullOrEmpty(expected))
                    return true;
                var notInValues = expected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return !notInValues.Any(v => string.Equals(v, actual, StringComparison.OrdinalIgnoreCase));

            case "between":
                return EvaluateBetween(actual, expected, expected2);

            case "regex":
                if (string.IsNullOrEmpty(expected))
                    return true;
                try
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(actual ?? "", expected, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                catch
                {
                    return false; // Invalid regex pattern
                }

            // Frontend-specific aliases (field change triggers)
            case "changed_to":
                return string.Equals(actual ?? "", expected ?? "", StringComparison.OrdinalIgnoreCase);

            case "changed_from":
                // For changed_from, compare against OldValue — but we only get actual=NewValue here
                // The caller should have set actual to OldValue for this operator
                return string.Equals(actual ?? "", expected ?? "", StringComparison.OrdinalIgnoreCase);

            default:
                return true; // Unknown operator — treat as match
        }
    }

    /// <summary>
    /// Compares two string values as decimals. Returns -1, 0, or 1.
    /// Falls back to string comparison if not numeric.
    /// </summary>
    private static int CompareNumeric(string? left, string? right)
    {
        if (decimal.TryParse(left, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var leftNum) &&
            decimal.TryParse(right, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rightNum))
        {
            return leftNum.CompareTo(rightNum);
        }

        // Fall back to date comparison
        if (DateTime.TryParse(left, out var leftDate) && DateTime.TryParse(right, out var rightDate))
        {
            return leftDate.CompareTo(rightDate);
        }

        // Fall back to ordinal string comparison
        return string.Compare(left ?? "", right ?? "", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Evaluates whether actual is between expected (low) and expected2 (high), inclusive.
    /// </summary>
    private static bool EvaluateBetween(string? actual, string? low, string? high)
    {
        if (string.IsNullOrEmpty(low) || string.IsNullOrEmpty(high))
            return true; // Incomplete between — skip

        return CompareNumeric(actual, low) >= 0 && CompareNumeric(actual, high) <= 0;
    }

    /// <summary>
    /// Flattens a JsonElement into a dictionary of path->string values.
    /// Case-insensitive keys should be provided by the caller's dictionary comparer.
    /// </summary>
    private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> dict)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : prefix + "." + prop.Name;
                    FlattenJson(prop.Value, key, dict);
                }
                break;

            case JsonValueKind.Array:
                int i = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var key = string.IsNullOrEmpty(prefix) ? i.ToString() : prefix + "." + i;
                    FlattenJson(item, key, dict);
                    i++;
                }
                break;

            case JsonValueKind.String:
                dict[prefix] = element.GetString() ?? string.Empty;
                break;

            case JsonValueKind.Number:
                dict[prefix] = element.GetRawText();
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                dict[prefix] = element.GetBoolean().ToString();
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                dict[prefix] = string.Empty;
                break;

            default:
                dict[prefix] = element.GetRawText();
                break;
        }
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
            EntityType = trigger.EntityType!,
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
