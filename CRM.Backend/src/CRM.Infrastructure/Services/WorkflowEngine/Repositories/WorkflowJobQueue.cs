using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.Repositories;

/// <summary>
/// Queue-based repository for workflow background jobs
/// </summary>
public class WorkflowJobQueue : IWorkflowJobQueue
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowJobQueue> _logger;

    public WorkflowJobQueue(
        CrmDbContext context,
        ILogger<WorkflowJobQueue> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnqueueAsync(
        WorkflowJob job, 
        CancellationToken cancellationToken = default)
    {
        job.Status = WorkflowJobStatus.Pending;
        job.CreatedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        
        if (job.ScheduledAt == default)
        {
            job.ScheduledAt = DateTime.UtcNow;
        }

        _context.WorkflowJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Enqueued job {JobId} of type {JobType} for instance {InstanceId}",
            job.Id, job.JobType, job.WorkflowInstanceId);
    }

    public async Task<WorkflowJob?> DequeueAsync(
        string workerId, 
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Find the next available job
        var job = await _context.WorkflowJobs
            .Where(j => j.Status == WorkflowJobStatus.Pending
                && j.ScheduledAt <= now
                && (j.VisibilityTimeoutAt == null || j.VisibilityTimeoutAt <= now))
            .OrderBy(j => j.Priority)
            .ThenBy(j => j.ScheduledAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return null;
        }

        // Try to claim the job
        job.Status = WorkflowJobStatus.Processing;
        job.ProcessingWorkerId = workerId;
        job.StartedAt = now;
        job.AttemptCount++;
        job.VisibilityTimeoutAt = now.AddMinutes(5);
        job.UpdatedAt = now;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Worker {WorkerId} dequeued job {JobId}", workerId, job.Id);
            return job;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another worker claimed it
            return null;
        }
    }

    public async Task CompleteAsync(
        int jobId, 
        string? resultData = null, 
        CancellationToken cancellationToken = default)
    {
        var job = await _context.WorkflowJobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job == null) return;

        job.Status = WorkflowJobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.ResultData = resultData;
        job.ProcessingWorkerId = null;
        job.VisibilityTimeoutAt = null;
        job.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed job {JobId}", jobId);
    }

    public async Task FailAsync(
        int jobId, 
        string errorMessage, 
        bool shouldRetry = true,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.WorkflowJobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job == null) return;

        job.ErrorMessage = errorMessage;
        job.UpdatedAt = DateTime.UtcNow;

        if (shouldRetry && job.AttemptCount < job.MaxAttempts)
        {
            // Schedule for retry with exponential backoff
            var backoffSeconds = Math.Pow(2, job.AttemptCount) * 30;
            job.Status = WorkflowJobStatus.Pending;
            job.ScheduledAt = DateTime.UtcNow.AddSeconds(backoffSeconds);
            job.ProcessingWorkerId = null;
            job.VisibilityTimeoutAt = null;

            _logger.LogWarning("Job {JobId} failed (attempt {Attempt}/{Max}), retrying at {RetryAt}",
                jobId, job.AttemptCount, job.MaxAttempts, job.ScheduledAt);
        }
        else
        {
            job.Status = WorkflowJobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.ProcessingWorkerId = null;

            _logger.LogError("Job {JobId} failed permanently after {Attempts} attempts: {Error}",
                jobId, job.AttemptCount, errorMessage);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<WorkflowJob>> GetPendingJobsAsync(
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowJobs
            .Where(j => j.Status == WorkflowJobStatus.Pending)
            .OrderBy(j => j.Priority)
            .ThenBy(j => j.ScheduledAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WorkflowJob>> DequeueMultipleAsync(
        string workerId, 
        int count, 
        CancellationToken cancellationToken = default)
    {
        var jobs = new List<WorkflowJob>();
        
        for (int i = 0; i < count; i++)
        {
            var job = await DequeueAsync(workerId, cancellationToken);
            if (job == null) break;
            jobs.Add(job);
        }
        
        return jobs;
    }

    public async Task CleanupCompletedAsync(
        TimeSpan olderThan, 
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - olderThan;
        
        var oldJobs = await _context.WorkflowJobs
            .Where(j => j.Status == WorkflowJobStatus.Completed && j.CompletedAt < cutoff)
            .ToListAsync(cancellationToken);
        
        if (oldJobs.Any())
        {
            _context.WorkflowJobs.RemoveRange(oldJobs);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Cleaned up {Count} completed jobs older than {Days} days",
                oldJobs.Count, olderThan.TotalDays);
        }
    }

    public async Task<int> RecoverStuckJobsAsync(
        TimeSpan timeout, 
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - timeout;
        
        var stuckJobs = await _context.WorkflowJobs
            .Where(j => j.Status == WorkflowJobStatus.Processing 
                     && j.StartedAt < cutoff)
            .ToListAsync(cancellationToken);
        
        foreach (var job in stuckJobs)
        {
            job.Status = WorkflowJobStatus.Pending;
            job.ProcessingWorkerId = null;
            job.VisibilityTimeoutAt = null;
            job.ErrorMessage = "Job timed out and was recovered";
            job.UpdatedAt = DateTime.UtcNow;
        }
        
        if (stuckJobs.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogWarning("Recovered {Count} stuck jobs", stuckJobs.Count);
        }
        
        return stuckJobs.Count;
    }
}
