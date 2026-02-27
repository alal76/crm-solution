// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dispatches entity lifecycle events to the workflow trigger engine.
/// Uses fire-and-forget pattern for the synchronous method to avoid blocking entity operations.
/// </summary>
public class EntityEventDispatcher : IEntityEventDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EntityEventDispatcher> _logger;

    public EntityEventDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<EntityEventDispatcher> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void DispatchEntityEvent(
        string entityType,
        int entityId,
        WorkflowTriggerType triggerType,
        int? initiatedById = null,
        string? changedField = null,
        string? oldValue = null,
        string? newValue = null,
        string? contextData = null)
    {
        var request = new TriggerExecutionRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            TriggerType = triggerType,
            ChangedField = changedField,
            OldValue = oldValue,
            NewValue = newValue,
            InitiatedById = initiatedById,
            ContextData = contextData
        };

        // Fire-and-forget: run trigger evaluation in background so entity operation is not blocked
        _ = Task.Run(async () =>
        {
            try
            {
                await DispatchEntityEventCoreAsync(request, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in fire-and-forget trigger dispatch for {EntityType} {EntityId} ({TriggerType})",
                    entityType, entityId, triggerType);
            }
        });
    }

    /// <inheritdoc />
    public async Task DispatchEntityEventAsync(
        string entityType,
        int entityId,
        WorkflowTriggerType triggerType,
        int? initiatedById = null,
        string? changedField = null,
        string? oldValue = null,
        string? newValue = null,
        string? contextData = null,
        CancellationToken cancellationToken = default)
    {
        var request = new TriggerExecutionRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            TriggerType = triggerType,
            ChangedField = changedField,
            OldValue = oldValue,
            NewValue = newValue,
            InitiatedById = initiatedById,
            ContextData = contextData
        };

        await DispatchEntityEventCoreAsync(request, cancellationToken);
    }

    /// <summary>
    /// Core dispatch logic — creates a new DI scope to resolve IWorkflowTriggerService.
    /// Uses TriggerExecutionRequest parameter object to reduce parameter count (S107).
    /// </summary>
    private async Task DispatchEntityEventCoreAsync(
        TriggerExecutionRequest request,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var triggerService = scope.ServiceProvider.GetRequiredService<IWorkflowTriggerService>();

        _logger.LogDebug("Dispatching {TriggerType} event for {EntityType} {EntityId}",
            request.TriggerType, request.EntityType, request.EntityId);

        var result = await triggerService.EvaluateTriggersAsync(request, cancellationToken);

        if (result.WorkflowsTriggered > 0)
        {
            _logger.LogInformation(
                "Entity event {TriggerType} on {EntityType} {EntityId} triggered {Count} workflow(s): [{Ids}]",
                request.TriggerType, request.EntityType, request.EntityId,
                result.WorkflowsTriggered,
                string.Join(", ", result.WorkflowInstanceIds));
        }

        if (result.Errors.Count > 0)
        {
            _logger.LogWarning(
                "Entity event {TriggerType} on {EntityType} {EntityId} had {ErrorCount} error(s): {Errors}",
                request.TriggerType, request.EntityType, request.EntityId,
                result.Errors.Count,
                string.Join("; ", result.Errors));
        }
    }
}
