// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities.Workflow;
using CRM.Core.DTOs.Workflow;

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
        var request = new TriggerExecutionRequest
        {
            TriggerType = WorkflowTriggerType.OnCreate,
            EntityType = entityType,
            EntityId = entityId,
            ContextData = entityData != null ? JsonSerializer.Serialize(entityData) : null
        };
        await EvaluateTriggersAsync(request, ct);
    }

    /// <summary>
    /// Fire workflow triggers when an entity is updated.
    /// </summary>
    public async Task OnEntityUpdatedAsync(string entityType, int entityId, object? entityData = null,
        string? changedField = null, string? oldValue = null, string? newValue = null, CancellationToken ct = default)
    {
        var contextJson = entityData != null ? JsonSerializer.Serialize(entityData) : null;
        var request = new TriggerExecutionRequest
        {
            TriggerType = WorkflowTriggerType.OnUpdate,
            EntityType = entityType,
            EntityId = entityId,
            ChangedField = changedField,
            OldValue = oldValue,
            NewValue = newValue,
            ContextData = contextJson
        };
        await EvaluateTriggersAsync(request, ct);

        // Also fire OnFieldChange if a specific field changed
        if (!string.IsNullOrEmpty(changedField))
        {
            var fieldChangeRequest = new TriggerExecutionRequest
            {
                TriggerType = WorkflowTriggerType.OnFieldChange,
                EntityType = entityType,
                EntityId = entityId,
                ChangedField = changedField,
                OldValue = oldValue,
                NewValue = newValue,
                ContextData = contextJson
            };
            await EvaluateTriggersAsync(fieldChangeRequest, ct);
        }
    }

    /// <summary>
    /// Fire workflow triggers when an entity is deleted.
    /// </summary>
    public async Task OnEntityDeletedAsync(string entityType, int entityId, object? entityData = null, CancellationToken ct = default)
    {
        var request = new TriggerExecutionRequest
        {
            TriggerType = WorkflowTriggerType.OnDelete,
            EntityType = entityType,
            EntityId = entityId,
            ContextData = entityData != null ? JsonSerializer.Serialize(entityData) : null
        };
        await EvaluateTriggersAsync(request, ct);
    }

    /// <summary>
    /// Fire workflow triggers when an event occurs (e.g., SLA breach, escalation).
    /// </summary>
    public async Task OnEventAsync(string eventName, string? entityType = null, int? entityId = null,
        object? eventData = null, CancellationToken ct = default)
    {
        var request = new TriggerExecutionRequest
        {
            TriggerType = WorkflowTriggerType.OnEvent,
            EntityType = entityType ?? "System",
            EntityId = entityId ?? 0,
            EventName = eventName,
            ContextData = eventData != null ? JsonSerializer.Serialize(eventData) : null
        };
        await EvaluateTriggersAsync(request, ct);
    }

    /// <summary>
    /// Evaluates workflow triggers using the provided request object.
    /// Reduces parameter count by using TriggerExecutionRequest as a parameter object.
    /// </summary>
    private async Task EvaluateTriggersAsync(TriggerExecutionRequest request, CancellationToken ct)
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

            var result = await triggerService.EvaluateTriggersAsync(request, ct);

            if (result.WorkflowsTriggered > 0)
            {
                _logger.LogInformation(
                    "Entity {EntityType}:{EntityId} triggered {Count} workflow(s) on {TriggerType}",
                    request.EntityType, request.EntityId, result.WorkflowsTriggered, request.TriggerType);
            }
        }
        catch (Exception ex)
        {
            // Never let trigger evaluation failures break the main operation
            _logger.LogError(ex,
                "Error evaluating workflow triggers for {EntityType}:{EntityId} ({TriggerType}). This does not affect the main operation.",
                request.EntityType, request.EntityId, request.TriggerType);
        }
    }
}
