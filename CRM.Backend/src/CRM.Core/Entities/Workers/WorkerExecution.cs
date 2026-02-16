// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.Workers;

public enum WorkerExecutionStatus
{
    Started = 0,
    Succeeded = 1,
    Failed = 2
}

[Table("WorkerExecutions")]
public class WorkerExecution : BaseEntity
{
    public int WorkerJobId { get; set; }
    public WorkerExecutionStatus Status { get; set; } = WorkerExecutionStatus.Started;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? NodeId { get; set; }
    public virtual WorkerJob? WorkerJob { get; set; }
}
