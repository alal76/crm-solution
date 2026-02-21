// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.Integration;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Integration;

public class OutboxDispatcher : IOutboxDispatcher
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(ICrmDbContext dbContext, ILogger<OutboxDispatcher> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task EnqueueAsync(OutboxEvent outboxEvent, CancellationToken ct = default)
    {
        _dbContext.Set<OutboxEvent>().Add(outboxEvent);
        _logger.LogInformation("Outbox enqueue requested for {EventType}", outboxEvent.EventType);
        return _dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> DispatchPendingAsync(int batchSize = 100, CancellationToken ct = default)
    {
        var pending = await _dbContext.Set<OutboxEvent>()
            .Where(e => e.Status == OutboxEventStatus.Pending)
            .OrderBy(e => e.OccurredAt)
            .Take(batchSize)
            .ToListAsync(ct);

        foreach (var outboxEvent in pending)
        {
            outboxEvent.Status = OutboxEventStatus.Completed;
            outboxEvent.ProcessedAt = DateTime.UtcNow;
            outboxEvent.UpdatedAt = DateTime.UtcNow;
        }

        if (pending.Count > 0)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Outbox dispatch completed for {Count} events", pending.Count);
        return pending.Count;
    }

    public async Task FailAsync(int outboxEventId, string error, CancellationToken ct = default)
    {
        var outboxEvent = await _dbContext.Set<OutboxEvent>().FindAsync(new object[] { outboxEventId }, ct);
        if (outboxEvent == null)
        {
            return;
        }

        outboxEvent.RetryCount++;
        outboxEvent.LastError = error;
        outboxEvent.Status = OutboxEventStatus.Failed;
        outboxEvent.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogWarning("Outbox event {Id} failed: {Error}", outboxEventId, error);
    }
}
