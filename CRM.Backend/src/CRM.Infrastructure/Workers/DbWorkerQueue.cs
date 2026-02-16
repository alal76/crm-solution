// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Entities.Workers;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Workers;

public class DbWorkerQueue : IWorkerQueue
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<DbWorkerQueue> _logger;

    public DbWorkerQueue(ICrmDbContext dbContext, ILogger<DbWorkerQueue> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task EnqueueAsync(WorkerJob job, CancellationToken ct = default)
    {
        _dbContext.Set<WorkerJob>().Add(job);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Worker job enqueued {JobType} (ID {JobId})", job.JobType, job.Id);
    }

    public async Task<WorkerJob?> DequeueAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var job = await _dbContext.Set<WorkerJob>()
            .Where(j => j.Status == WorkerJobStatus.Queued &&
                        (!j.NextAttemptAt.HasValue || j.NextAttemptAt <= now))
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (job == null)
        {
            return null;
        }

        job.Status = WorkerJobStatus.InProgress;
        job.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        return job;
    }

    public async Task CompleteAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _dbContext.Set<WorkerJob>().FindAsync(new object[] { jobId }, ct);
        if (job == null)
        {
            return;
        }

        job.Status = WorkerJobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task FailAsync(int jobId, string error, CancellationToken ct = default)
    {
        var job = await _dbContext.Set<WorkerJob>().FindAsync(new object[] { jobId }, ct);
        if (job == null)
        {
            return;
        }

        job.RetryCount++;
        job.LastError = error;
        job.UpdatedAt = DateTime.UtcNow;

        if (job.RetryCount > job.MaxRetries)
        {
            job.Status = WorkerJobStatus.DeadLettered;
        }
        else
        {
            job.Status = WorkerJobStatus.Failed;
            job.NextAttemptAt = DateTime.UtcNow.AddMinutes(5);
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
