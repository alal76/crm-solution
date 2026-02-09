// <copyright file="ScheduledWorkflowService.cs" company="CRM Solution">
// Copyright (c) CRM Solution. All rights reserved.
// </copyright>

using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that periodically evaluates scheduled workflow triggers
/// and fires any that are due for execution.
/// </summary>
public class ScheduledWorkflowService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledWorkflowService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledWorkflowService"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for creating scoped services.</param>
    /// <param name="logger">Logger instance.</param>
    public ScheduledWorkflowService(
        IServiceProvider serviceProvider,
        ILogger<ScheduledWorkflowService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledWorkflowService started. Checking every {Interval} minute(s)", _checkInterval.TotalMinutes);

        // Small startup delay to let the application finish initialization
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueTriggersAsync(stoppingToken);
                await ProcessDueWaitNodesAsync(stoppingToken);
                await ProcessTimedOutItemsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ScheduledWorkflowService stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in ScheduledWorkflowService loop");
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("ScheduledWorkflowService stopped");
    }

    private async Task ProcessDueTriggersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var triggerService = scope.ServiceProvider.GetRequiredService<IWorkflowTriggerService>();

        var now = DateTime.UtcNow;
        var dueTriggers = await triggerService.GetScheduledTriggersDueAsync(now, stoppingToken);
        var triggerList = dueTriggers.ToList();

        if (triggerList.Count == 0)
        {
            _logger.LogDebug("No scheduled workflow triggers due at {Timestamp}", now);
            return;
        }

        _logger.LogInformation(
            "Found {Count} scheduled workflow trigger(s) due for execution at {Timestamp}",
            triggerList.Count,
            now);

        foreach (var trigger in triggerList)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                _logger.LogInformation(
                    "Firing scheduled trigger {TriggerId} ({TriggerName}) for workflow {WorkflowId}",
                    trigger.Id,
                    trigger.Name,
                    trigger.WorkflowDefinitionId);

                // Fire the trigger with entityId=0 (scheduled triggers are not entity-specific)
                // RecordTriggerExecutionAsync is called internally by FireTriggerAsync,
                // which also advances NextScheduledAt via CronExpression
                var result = await triggerService.FireTriggerAsync(
                    trigger.Id,
                    entityId: 0,
                    initiatedById: null,
                    stoppingToken);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Scheduled trigger {TriggerId} executed successfully, started {WorkflowCount} workflow(s)",
                        trigger.Id,
                        result.WorkflowsTriggered);
                }
                else
                {
                    _logger.LogWarning(
                        "Scheduled trigger {TriggerId} execution reported failure: {Errors}",
                        trigger.Id,
                        string.Join("; ", result.Errors));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing scheduled trigger {TriggerId} ({TriggerName})",
                    trigger.Id,
                    trigger.Name);

                // Continue processing remaining triggers even if one fails
            }
        }

        _logger.LogDebug("Completed processing {Count} scheduled trigger(s)", triggerList.Count);
    }

    private async Task ProcessDueWaitNodesAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var instanceService = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceService>();

            var processed = await instanceService.ProcessDueWaitNodesAsync(stoppingToken);
            if (processed > 0)
            {
                _logger.LogInformation("Processed {Count} due wait node(s)", processed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing due wait nodes");
        }
    }

    private async Task ProcessTimedOutItemsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var instanceService = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceService>();

            var processed = await instanceService.ProcessTimedOutInstancesAsync(stoppingToken);
            if (processed > 0)
            {
                _logger.LogInformation("Processed {Count} timed-out workflow item(s)", processed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing timed-out items");
        }
    }
}
