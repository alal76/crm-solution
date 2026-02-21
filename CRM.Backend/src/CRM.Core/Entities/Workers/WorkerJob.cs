// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.Workers;

public enum WorkerJobStatus
{
    Queued = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    DeadLettered = 4
}

[Table("WorkerJobs")]
public class WorkerJob : BaseEntity
{
    public string JobType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public WorkerJobStatus Status { get; set; } = WorkerJobStatus.Queued;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 5;
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
}
