// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a workflow trigger configuration that determines when a workflow should execute.
/// </summary>
public class WorkflowTrigger : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow definition.
    /// </summary>
    public int WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Navigation property to the workflow definition.
    /// </summary>
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>
    /// Name of the trigger for display purposes.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of trigger (Manual, OnCreate, OnUpdate, etc.).
    /// </summary>
    public WorkflowTriggerType TriggerType { get; set; } = WorkflowTriggerType.Manual;

    /// <summary>
    /// Entity type this trigger applies to (e.g., Lead, Opportunity, ServiceRequest).
    /// Required for record-triggered workflows.
    /// </summary>
    [MaxLength(100)]
    public string? EntityType { get; set; }

    /// <summary>
    /// Event name for OnEvent triggers (e.g., "lead.converted", "opportunity.won").
    /// </summary>
    [MaxLength(200)]
    public string? EventName { get; set; }

    /// <summary>
    /// Cron expression for Scheduled triggers (e.g., "0 8 * * 1" = 8 AM every Monday).
    /// </summary>
    [MaxLength(100)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// Filter conditions as JSON (e.g., {"field": "Status", "operator": "equals", "value": "Qualified"}).
    /// The workflow only triggers when conditions are met.
    /// </summary>
    public string? FilterConditions { get; set; }

    /// <summary>
    /// Field to monitor for OnUpdate triggers (e.g., "Status", "AssignedToId").
    /// If null, triggers on any field change.
    /// </summary>
    [MaxLength(100)]
    public string? WatchedField { get; set; }

    /// <summary>
    /// Specific old value that triggers the workflow (for OnUpdate with watched field).
    /// </summary>
    [MaxLength(500)]
    public string? OldValue { get; set; }

    /// <summary>
    /// Specific new value that triggers the workflow (for OnUpdate with watched field).
    /// </summary>
    [MaxLength(500)]
    public string? NewValue { get; set; }

    /// <summary>
    /// Whether this trigger is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority order when multiple triggers match (lower = higher priority).
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Description of what this trigger does.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Last time this trigger was executed.
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Next scheduled execution time (for Scheduled triggers).
    /// </summary>
    public DateTime? NextScheduledAt { get; set; }

    /// <summary>
    /// Count of times this trigger has fired.
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    /// Delay in seconds before executing the workflow after trigger fires.
    /// </summary>
    public int DelaySeconds { get; set; } = 0;

    /// <summary>
    /// Whether to run the workflow asynchronously (fire and forget).
    /// </summary>
    public bool RunAsync { get; set; } = true;

    /// <summary>
    /// Maximum number of retries if workflow execution fails.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// User ID who created this trigger.
    /// </summary>
    public int? CreatedById { get; set; }

    /// <summary>
    /// Navigation property to creator.
    /// </summary>
    public virtual User? CreatedBy { get; set; }
}

/// <summary>
/// Types of workflow triggers.
/// </summary>
public enum WorkflowTriggerType
{
    /// <summary>
    /// Manually triggered by user action.
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Triggered when a new record is created.
    /// </summary>
    OnCreate = 1,

    /// <summary>
    /// Triggered when a record is updated.
    /// </summary>
    OnUpdate = 2,

    /// <summary>
    /// Triggered when a record is deleted.
    /// </summary>
    OnDelete = 3,

    /// <summary>
    /// Triggered on specific field value change (e.g., Status changed to "Closed").
    /// </summary>
    OnFieldChange = 4,

    /// <summary>
    /// Triggered on a schedule (cron expression).
    /// </summary>
    Scheduled = 5,

    /// <summary>
    /// Triggered by a custom event (e.g., "lead.converted").
    /// </summary>
    OnEvent = 6,

    /// <summary>
    /// Triggered by a webhook from external system.
    /// </summary>
    OnWebhook = 7,

    /// <summary>
    /// Triggered when SLA is breached or at risk.
    /// </summary>
    OnSLABreach = 8,

    /// <summary>
    /// Triggered when escalation conditions are met.
    /// </summary>
    OnEscalation = 9
}
