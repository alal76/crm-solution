// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

namespace CRM.Core.Dtos.Workers;

public class WorkerJobDto
{
    public int Id { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CorrelationId { get; set; }
}
