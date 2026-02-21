// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a cron-based schedule that triggers a workflow definition on a recurring basis.
/// </summary>
public class WorkflowSchedule : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow definition this schedule belongs to.
    /// </summary>
    public int WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Navigation property to the workflow definition.
    /// </summary>
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>
    /// Display name of the schedule.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the schedule's purpose.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Cron expression defining the recurrence pattern (e.g., "0 8 * * 1" = 8 AM every Monday).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// IANA time zone identifier (e.g., "America/New_York"). Defaults to UTC if not specified.
    /// </summary>
    [MaxLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Whether this schedule is currently active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Timestamp of the last successful trigger of this schedule.
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Computed next trigger timestamp based on the cron expression.
    /// </summary>
    public DateTime? NextTriggerAt { get; set; }

    /// <summary>
    /// Total number of times this schedule has triggered.
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Optional JSON context data to pass to the workflow instance on each trigger.
    /// </summary>
    public string? ContextData { get; set; }
}
