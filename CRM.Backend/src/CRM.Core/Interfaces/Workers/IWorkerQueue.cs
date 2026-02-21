// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.Workers;

namespace CRM.Core.Interfaces.Workers;

public interface IWorkerQueue
{
    Task EnqueueAsync(WorkerJob job, CancellationToken ct = default);
    Task<WorkerJob?> DequeueAsync(CancellationToken ct = default);
    Task CompleteAsync(int jobId, CancellationToken ct = default);
    Task FailAsync(int jobId, string error, CancellationToken ct = default);
}
