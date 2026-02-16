// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Entities.Workers;

namespace CRM.Core.Interfaces.Workers;

public interface IWorkerQueue
{
    Task EnqueueAsync(WorkerJob job, CancellationToken ct = default);
    Task<WorkerJob?> DequeueAsync(CancellationToken ct = default);
    Task CompleteAsync(int jobId, CancellationToken ct = default);
    Task FailAsync(int jobId, string error, CancellationToken ct = default);
}
