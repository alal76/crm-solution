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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Intercepts entity CRUD operations to evaluate and fire workflow triggers automatically.
/// Called from entity services on create/update/delete operations.
/// </summary>
public class WorkflowEventInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowEventInterceptor> _logger;

    public WorkflowEventInterceptor(
        IServiceProvider serviceProvider,
        ILogger<WorkflowEventInterceptor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Fire workflow triggers when an entity is created.
    /// </summary>
    public async Task OnEntityCreatedAsync(string entityType, int entityId, object? entityData = null, CancellationToken ct = default)
    {
        await EvaluateTriggersAsync(WorkflowTriggerType.OnCreate, entityType, entityId, entityData, null, null, null, ct);
    }

    /// <summary>
    /// Fire workflow triggers when an entity is updated.
    /// </summary>
    public async Task OnEntityUpdatedAsync(string entityType, int entityId, object? entityData = null,
        string? changedField = null, string? oldValue = null, string? newValue = null, CancellationToken ct = default)
    {
        await EvaluateTriggersAsync(WorkflowTriggerType.OnUpdate, entityType, entityId, entityData, changedField, oldValue, newValue, ct);

        // Also fire OnFieldChange if a specific field changed
        if (!string.IsNullOrEmpty(changedField))
        {
            await EvaluateTriggersAsync(WorkflowTriggerType.OnFieldChange, entityType, entityId, entityData, changedField, oldValue, newValue, ct);
        }
    }

    /// <summary>
    /// Fire workflow triggers when an entity is deleted.
    /// </summary>
    public async Task OnEntityDeletedAsync(string entityType, int entityId, object? entityData = null, CancellationToken ct = default)
    {
        await EvaluateTriggersAsync(WorkflowTriggerType.OnDelete, entityType, entityId, entityData, null, null, null, ct);
    }

    /// <summary>
    /// Fire workflow triggers when an event occurs (e.g., SLA breach, escalation).
    /// </summary>
    public async Task OnEventAsync(string eventName, string? entityType = null, int? entityId = null,
        object? eventData = null, CancellationToken ct = default)
    {
        await EvaluateTriggersAsync(WorkflowTriggerType.OnEvent, entityType ?? "System", entityId ?? 0, eventData, null, null, null, ct, eventName);
    }

    private async Task EvaluateTriggersAsync(
        WorkflowTriggerType triggerType,
        string entityType,
        int entityId,
        object? entityData,
        string? changedField,
        string? oldValue,
        string? newValue,
        CancellationToken ct,
        string? eventName = null)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var triggerService = scope.ServiceProvider.GetService<IWorkflowTriggerService>();

            if (triggerService == null)
            {
                _logger.LogDebug("IWorkflowTriggerService not available, skipping trigger evaluation");
                return;
            }

            var request = new TriggerExecutionRequest
            {
                TriggerType = triggerType,
                EntityType = entityType,
                EntityId = entityId,
                EventName = eventName,
                ChangedField = changedField,
                OldValue = oldValue,
                NewValue = newValue,
                ContextData = entityData != null ? JsonSerializer.Serialize(entityData) : null
            };

            var result = await triggerService.EvaluateTriggersAsync(request, ct);

            if (result.WorkflowsTriggered > 0)
            {
                _logger.LogInformation(
                    "Entity {EntityType}:{EntityId} triggered {Count} workflow(s) on {TriggerType}",
                    entityType, entityId, result.WorkflowsTriggered, triggerType);
            }
        }
        catch (Exception ex)
        {
            // Never let trigger evaluation failures break the main operation
            _logger.LogError(ex,
                "Error evaluating workflow triggers for {EntityType}:{EntityId} ({TriggerType}). This does not affect the main operation.",
                entityType, entityId, triggerType);
        }
    }
}
