// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents an asynchronous job in the workflow engine's internal queue.
/// </summary>
public class WorkflowJob : BaseEntity
{
    /// <summary>
    /// Type of job (e.g., "ExecuteNode", "SendNotification", "HttpCallout").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the job (e.g., "Pending", "Processing", "Completed", "Failed").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Numeric priority for job ordering (lower values = higher priority).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Optional foreign key to the workflow instance this job belongs to.
    /// </summary>
    public int? WorkflowInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the workflow instance.
    /// </summary>
    public virtual WorkflowInstance? WorkflowInstance { get; set; }

    /// <summary>
    /// Key of the workflow step this job is associated with.
    /// </summary>
    [MaxLength(200)]
    public string? StepKey { get; set; }

    /// <summary>
    /// Optional foreign key to a workflow task this job is associated with.
    /// </summary>
    public int? WorkflowTaskId { get; set; }

    /// <summary>
    /// Navigation property to the workflow task.
    /// </summary>
    public virtual WorkflowTask? WorkflowTask { get; set; }

    /// <summary>
    /// JSON payload containing job-specific input data.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// When this job is scheduled to run.
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// When a worker started processing this job.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the job finished (successfully or with an error).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Identifier of the worker process currently handling this job.
    /// </summary>
    [MaxLength(200)]
    public string? ProcessingWorkerId { get; set; }

    /// <summary>
    /// Number of times this job has been attempted.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Maximum number of retry attempts before the job is marked as failed.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Error message from the most recent failed attempt.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// JSON result data produced by a successful job execution.
    /// </summary>
    public string? ResultData { get; set; }

    /// <summary>
    /// Timestamp until which this job is invisible to other workers (for at-least-once delivery).
    /// </summary>
    public DateTime? VisibilityTimeoutAt { get; set; }

    /// <summary>
    /// Correlation identifier for tracing related jobs across the system.
    /// </summary>
    [MaxLength(200)]
    public string? CorrelationId { get; set; }
}
