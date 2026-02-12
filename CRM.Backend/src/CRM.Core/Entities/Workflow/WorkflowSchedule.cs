// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
