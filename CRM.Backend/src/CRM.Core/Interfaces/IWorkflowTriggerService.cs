// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for managing workflow triggers.
/// </summary>
public interface IWorkflowTriggerService
{
    // CRUD Operations

    /// <summary>
    /// Gets all workflow triggers with optional filtering.
    /// </summary>
    Task<IEnumerable<WorkflowTriggerDto>> GetAllAsync(
        int? workflowDefinitionId = null,
        WorkflowTriggerType? triggerType = null,
        string? entityType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workflow trigger by ID.
    /// </summary>
    Task<WorkflowTriggerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all triggers for a specific workflow definition.
    /// </summary>
    Task<IEnumerable<WorkflowTriggerDto>> GetByWorkflowAsync(int workflowDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workflow trigger.
    /// </summary>
    Task<WorkflowTriggerDto> CreateAsync(CreateWorkflowTriggerDto dto, int? createdById = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workflow trigger.
    /// </summary>
    Task<WorkflowTriggerDto> UpdateAsync(UpdateWorkflowTriggerDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workflow trigger.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Activation

    /// <summary>
    /// Activates a workflow trigger.
    /// </summary>
    Task<WorkflowTriggerDto> ActivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a workflow trigger.
    /// </summary>
    Task<WorkflowTriggerDto> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    // Trigger Execution

    /// <summary>
    /// Evaluates and executes matching triggers for an entity event.
    /// </summary>
    Task<TriggerExecutionResult> EvaluateTriggersAsync(TriggerExecutionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually fires a specific trigger.
    /// </summary>
    Task<TriggerExecutionResult> FireTriggerAsync(int triggerId, int entityId, int? initiatedById = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets triggers that match a specific entity and trigger type.
    /// </summary>
    Task<IEnumerable<WorkflowTriggerDto>> GetMatchingTriggersAsync(
        string entityType,
        WorkflowTriggerType triggerType,
        string? eventName = null,
        CancellationToken cancellationToken = default);

    // Scheduled Triggers

    /// <summary>
    /// Gets all scheduled triggers that need to run.
    /// </summary>
    Task<IEnumerable<WorkflowTriggerDto>> GetScheduledTriggersDueAsync(DateTime asOfTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the next scheduled time for a trigger.
    /// </summary>
    Task UpdateNextScheduledTimeAsync(int triggerId, DateTime nextTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that a trigger was executed.
    /// </summary>
    Task RecordTriggerExecutionAsync(int triggerId, CancellationToken cancellationToken = default);

    // Statistics

    /// <summary>
    /// Gets trigger statistics.
    /// </summary>
    Task<TriggerStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);

    // Validation

    /// <summary>
    /// Validates a cron expression.
    /// </summary>
    bool ValidateCronExpression(string cronExpression, out string? errorMessage);

    /// <summary>
    /// Validates filter conditions JSON.
    /// </summary>
    bool ValidateFilterConditions(string filterConditions, out string? errorMessage);
}
