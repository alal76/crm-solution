using System.Text.Json;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.Workers;

/// <summary>
/// Background worker that processes workflow jobs from the queue
/// </summary>
public class WorkflowJobProcessorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowJobProcessorHostedService> _logger;
    private readonly string _workerId;
    private readonly int _batchSize = 5;
    private readonly int _pollingIntervalMs = 1000;

    public WorkflowJobProcessorHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkflowJobProcessorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _workerId = $"worker-{Environment.MachineName}-{Guid.NewGuid():N}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow Job Processor {WorkerId} starting", _workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in workflow job processor");
            }

            await Task.Delay(_pollingIntervalMs, stoppingToken);
        }

        _logger.LogInformation("Workflow Job Processor {WorkerId} stopping", _workerId);
    }

    private async Task ProcessJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobQueue = scope.ServiceProvider.GetRequiredService<IWorkflowJobQueue>();
        var workflowEngine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();

        var jobs = await jobQueue.DequeueMultipleAsync(_workerId, _batchSize, stoppingToken);

        foreach (var job in jobs)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await ProcessJobAsync(job, workflowEngine, jobQueue, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job {JobId}", job.Id);
                await jobQueue.FailAsync(job.Id, ex.Message, true, stoppingToken);
            }
        }
    }

    private async Task ProcessJobAsync(
        WorkflowJob job,
        IWorkflowEngine workflowEngine,
        IWorkflowJobQueue jobQueue,
        CancellationToken stoppingToken)
    {
        _logger.LogDebug("Processing job {JobId} of type {JobType}", job.Id, job.JobType);

        switch (job.JobType)
        {
            case WorkflowJobTypes.ExecuteStep:
                await ProcessExecuteStepJobAsync(job, workflowEngine, stoppingToken);
                break;

            case WorkflowJobTypes.ContinueWorkflow:
                if (job.WorkflowInstanceId.HasValue)
                {
                    await workflowEngine.ProcessWorkflowAsync(job.WorkflowInstanceId.Value, stoppingToken);
                }
                break;

            case WorkflowJobTypes.CheckTimeout:
                await ProcessTimeoutCheckAsync(job, workflowEngine, stoppingToken);
                break;

            case WorkflowJobTypes.SendNotification:
                await ProcessNotificationJobAsync(job, stoppingToken);
                break;

            case WorkflowJobTypes.CleanupCompleted:
                await ProcessCleanupJobAsync(job, jobQueue, stoppingToken);
                break;

            default:
                _logger.LogWarning("Unknown job type: {JobType}", job.JobType);
                break;
        }

        await jobQueue.CompleteAsync(job.Id, null, stoppingToken);
    }

    private async Task ProcessExecuteStepJobAsync(
        WorkflowJob job,
        IWorkflowEngine workflowEngine,
        CancellationToken stoppingToken)
    {
        // Continue workflow execution
        if (job.WorkflowInstanceId.HasValue)
        {
            await workflowEngine.ProcessWorkflowAsync(job.WorkflowInstanceId.Value, stoppingToken);
        }
    }

    private async Task ProcessTimeoutCheckAsync(
        WorkflowJob job,
        IWorkflowEngine workflowEngine,
        CancellationToken stoppingToken)
    {
        if (!job.WorkflowInstanceId.HasValue) return;
        
        // Get instance and check for timeouts
        using var scope = _scopeFactory.CreateScope();
        var instanceRepo = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
        
        var instance = await instanceRepo.GetByIdAsync(job.WorkflowInstanceId.Value, false, stoppingToken);
        if (instance == null) return;

        // Check if current step has timed out
        if (instance.Status == WorkflowInstanceStatus.WaitingForInput)
        {
            // Parse job payload for timeout info
            if (!string.IsNullOrEmpty(job.Payload))
            {
                var payload = JsonSerializer.Deserialize<TimeoutPayload>(job.Payload);
                if (payload != null && DateTime.UtcNow > payload.TimeoutAt)
                {
                    _logger.LogWarning("Instance {InstanceId} step {StepKey} has timed out",
                        job.WorkflowInstanceId, payload.StepKey);
                    
                    // Handle timeout - could trigger escalation or automatic transition
                    // For now, log an event
                }
            }
        }
    }

    private Task ProcessNotificationJobAsync(
        WorkflowJob job,
        CancellationToken stoppingToken)
    {
        // Process notification from payload
        if (!string.IsNullOrEmpty(job.Payload))
        {
            var notification = JsonSerializer.Deserialize<NotificationPayload>(job.Payload);
            if (notification != null)
            {
                _logger.LogInformation("Sending notification: {Type} to {Recipient}",
                    notification.Type, notification.Recipient);
                // TODO: Integrate with notification service
            }
        }

        return Task.CompletedTask;
    }

    private async Task ProcessCleanupJobAsync(
        WorkflowJob job,
        IWorkflowJobQueue jobQueue,
        CancellationToken stoppingToken)
    {
        // Cleanup old completed jobs
        await jobQueue.CleanupCompletedAsync(TimeSpan.FromDays(30), stoppingToken);
    }

    private class TimeoutPayload
    {
        public string? StepKey { get; set; }
        public DateTime TimeoutAt { get; set; }
    }

    private class NotificationPayload
    {
        public string? Type { get; set; }
        public string? Recipient { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}

/// <summary>
/// Job types for the workflow queue
/// </summary>
public static class WorkflowJobTypes
{
    public const string ExecuteStep = "ExecuteStep";
    public const string ContinueWorkflow = "ContinueWorkflow";
    public const string CheckTimeout = "CheckTimeout";
    public const string SendNotification = "SendNotification";
    public const string CleanupCompleted = "CleanupCompleted";
    public const string RetryFailed = "RetryFailed";
}

/// <summary>
/// Background service that monitors for stuck jobs and recovers them
/// </summary>
public class WorkflowJobRecoveryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowJobRecoveryHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _stuckJobTimeout = TimeSpan.FromMinutes(10);

    public WorkflowJobRecoveryHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkflowJobRecoveryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow Job Recovery Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var jobQueue = scope.ServiceProvider.GetRequiredService<IWorkflowJobQueue>();

                await jobQueue.RecoverStuckJobsAsync(_stuckJobTimeout, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in job recovery service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Workflow Job Recovery Service stopping");
    }
}

/// <summary>
/// Background service that monitors workflow SLAs and triggers escalations
/// </summary>
public class WorkflowSlaMonitorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowSlaMonitorHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public WorkflowSlaMonitorHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkflowSlaMonitorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow SLA Monitor Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckSlasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SLA monitor service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Workflow SLA Monitor Service stopping");
    }

    private async Task CheckSlasAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskRepo = scope.ServiceProvider.GetRequiredService<IWorkflowTaskRepository>();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IWorkflowEventRepository>();

        // Check for overdue tasks
        var overdueTasks = await taskRepo.GetOverdueTasksAsync(stoppingToken);

        foreach (var task in overdueTasks)
        {
            _logger.LogWarning("Task {TaskId} is overdue (due: {DueAt})", task.Id, task.DueAt);

            // Log SLA breach event
            await eventRepo.LogEventAsync(new WorkflowEvent
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                EventType = WorkflowEventTypes.SlaBreached,
                StepKey = task.StepKey,
                Severity = "Warning",
                OutputData = JsonSerializer.Serialize(new
                {
                    taskId = task.Id,
                    dueAt = task.DueAt,
                    overdueSince = DateTime.UtcNow - task.DueAt
                })
            }, stoppingToken);

            // TODO: Trigger escalation based on workflow configuration
        }
    }
}

/// <summary>
/// Background service that processes scheduled workflow triggers
/// </summary>
public class WorkflowScheduleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowScheduleHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public WorkflowScheduleHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkflowScheduleHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workflow Schedule Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSchedulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in schedule service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Workflow Schedule Service stopping");
    }

    private async Task ProcessSchedulesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CRM.Infrastructure.Data.CrmDbContext>();
        var workflowEngine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();

        var now = DateTime.UtcNow;

        // Find schedules that are due
        var dueSchedules = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(
                context.WorkflowSchedules
                    .Include(s => s.WorkflowDefinition)
                    .Where(s => s.IsEnabled
                        && s.NextTriggerAt <= now
                        && (s.ValidFrom == null || s.ValidFrom <= now)
                        && (s.ValidUntil == null || s.ValidUntil >= now)),
                stoppingToken);

        foreach (var schedule in dueSchedules)
        {
            try
            {
                _logger.LogInformation("Triggering scheduled workflow {WorkflowId}: {WorkflowName}",
                    schedule.WorkflowDefinitionId, schedule.WorkflowDefinition?.Name);

                // Start the workflow
                var request = new CRM.Core.Dtos.WorkflowEngine.StartWorkflowDto
                {
                    WorkflowDefinitionId = schedule.WorkflowDefinitionId,
                    EntityType = "Scheduled",
                    EntityId = 0,
                    InitialContext = new Dictionary<string, object?>
                    {
                        ["triggeredBy"] = "schedule",
                        ["scheduleId"] = schedule.Id,
                        ["triggeredAt"] = now
                    }
                };
                
                await workflowEngine.StartWorkflowAsync(request, null, stoppingToken);

                // Update schedule
                schedule.LastTriggeredAt = now;
                schedule.ExecutionCount++;
                schedule.NextTriggerAt = CalculateNextTrigger(schedule.CronExpression, now);

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering scheduled workflow {ScheduleId}", schedule.Id);
            }
        }
    }

    private DateTime? CalculateNextTrigger(string? cronExpression, DateTime from)
    {
        if (string.IsNullOrEmpty(cronExpression))
        {
            return null;
        }

        // Simple cron parsing - in production use NCrontab or similar
        // For now, just add an hour as a placeholder
        // TODO: Implement proper cron parsing
        return from.AddHours(1);
    }
}
