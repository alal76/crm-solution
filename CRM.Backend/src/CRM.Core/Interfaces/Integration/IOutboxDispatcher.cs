// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Entities.Integration;

namespace CRM.Core.Interfaces.Integration;

public interface IOutboxDispatcher
{
    Task EnqueueAsync(OutboxEvent outboxEvent, CancellationToken ct = default);
    Task<int> DispatchPendingAsync(int batchSize = 100, CancellationToken ct = default);
    Task FailAsync(int outboxEventId, string error, CancellationToken ct = default);
}
