// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities.Workflow;

namespace CRM.Core.Interfaces;

/// <summary>
/// Dispatches entity lifecycle events (create, update, delete) to the workflow trigger engine.
/// Inject this into entity services to enable event-driven workflow triggers.
/// </summary>
public interface IEntityEventDispatcher
{
    /// <summary>
    /// Dispatches an entity lifecycle event to evaluate matching workflow triggers.
    /// This method is fire-and-forget: it will not throw exceptions or block the caller.
    /// </summary>
    /// <param name="entityType">Entity type name (e.g., "Account", "Contact", "Lead", "Opportunity", "Order").</param>
    /// <param name="entityId">The ID of the affected entity.</param>
    /// <param name="triggerType">The type of trigger event (OnCreate, OnUpdate, OnDelete, OnFieldChange).</param>
    /// <param name="initiatedById">The user ID who performed the action (null for system actions).</param>
    /// <param name="changedField">For OnFieldChange: the name of the changed field.</param>
    /// <param name="oldValue">For OnFieldChange: the previous value.</param>
    /// <param name="newValue">For OnFieldChange: the new value.</param>
    /// <param name="contextData">Optional JSON context data for the trigger.</param>
    void DispatchEntityEvent(
        string entityType,
        int entityId,
        WorkflowTriggerType triggerType,
        int? initiatedById = null,
        string? changedField = null,
        string? oldValue = null,
        string? newValue = null,
        string? contextData = null);

    /// <summary>
    /// Dispatches an entity lifecycle event asynchronously, awaiting the trigger evaluation.
    /// Use this when you need to know the result of trigger evaluation.
    /// </summary>
    Task DispatchEntityEventAsync(
        string entityType,
        int entityId,
        WorkflowTriggerType triggerType,
        int? initiatedById = null,
        string? changedField = null,
        string? oldValue = null,
        string? newValue = null,
        string? contextData = null,
        CancellationToken cancellationToken = default);
}
