// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

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
        // Fire-and-forget: run trigger evaluation in background so entity operation is not blocked
        _ = Task.Run(async () =>
        {
            try
            {
                await DispatchEntityEventCoreAsync(
                    entityType, entityId, triggerType, initiatedById,
                    changedField, oldValue, newValue, contextData, CancellationToken.None);
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
        await DispatchEntityEventCoreAsync(
            entityType, entityId, triggerType, initiatedById,
            changedField, oldValue, newValue, contextData, cancellationToken);
    }

    /// <summary>
    /// Core dispatch logic — creates a new DI scope to resolve IWorkflowTriggerService
    /// (avoids captive dependency issues since this may outlive the original request scope).
    /// </summary>
    private async Task DispatchEntityEventCoreAsync(
        string entityType,
        int entityId,
        WorkflowTriggerType triggerType,
        int? initiatedById,
        string? changedField,
        string? oldValue,
        string? newValue,
        string? contextData,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var triggerService = scope.ServiceProvider.GetRequiredService<IWorkflowTriggerService>();

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

        _logger.LogDebug("Dispatching {TriggerType} event for {EntityType} {EntityId}",
            triggerType, entityType, entityId);

        var result = await triggerService.EvaluateTriggersAsync(request, cancellationToken);

        if (result.WorkflowsTriggered > 0)
        {
            _logger.LogInformation(
                "Entity event {TriggerType} on {EntityType} {EntityId} triggered {Count} workflow(s): [{Ids}]",
                triggerType, entityType, entityId,
                result.WorkflowsTriggered,
                string.Join(", ", result.WorkflowInstanceIds));
        }

        if (result.Errors.Count > 0)
        {
            _logger.LogWarning(
                "Entity event {TriggerType} on {EntityType} {EntityId} had {ErrorCount} error(s): {Errors}",
                triggerType, entityType, entityId,
                result.Errors.Count,
                string.Join("; ", result.Errors));
        }
    }
}
