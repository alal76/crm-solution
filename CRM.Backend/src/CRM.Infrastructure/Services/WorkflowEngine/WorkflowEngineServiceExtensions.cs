using CRM.Core.Interfaces.WorkflowEngine;
using CRM.Infrastructure.Services.WorkflowEngine;
using CRM.Infrastructure.Services.WorkflowEngine.Repositories;
using CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;
using CRM.Infrastructure.Services.WorkflowEngine.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Services.WorkflowEngine;

/// <summary>
/// Extension methods for registering workflow engine services
/// </summary>
public static class WorkflowEngineServiceExtensions
{
    /// <summary>
    /// Adds workflow engine services to the service collection
    /// </summary>
    public static IServiceCollection AddWorkflowEngine(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
        services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
        services.AddScoped<IWorkflowTaskRepository, WorkflowTaskRepository>();
        services.AddScoped<IWorkflowEventRepository, WorkflowEventRepository>();
        services.AddScoped<IWorkflowJobQueue, WorkflowJobQueue>();

        // Register expression evaluator
        services.AddScoped<IWorkflowExpressionEvaluator, WorkflowExpressionEvaluator>();

        // Register step executors (Strategy Pattern)
        services.AddScoped<IStepExecutor, UserActionStepExecutor>();
        services.AddScoped<IStepExecutor, ApiCallStepExecutor>();
        services.AddScoped<IStepExecutor, ConditionalStepExecutor>();
        services.AddScoped<IStepExecutor, DelayStepExecutor>();
        services.AddScoped<IStepExecutor, NotificationStepExecutor>();
        services.AddScoped<IStepExecutor, ParallelStepExecutor>();
        services.AddScoped<IStepExecutor, JoinStepExecutor>();
        services.AddScoped<IStepExecutor, StartEndStepExecutor>();
        services.AddScoped<IStepExecutor, ScriptStepExecutor>();

        // Register main workflow engine
        services.AddScoped<IWorkflowEngine, WorkflowEngineService>();

        // Register HTTP client for API calls
        services.AddHttpClient("WorkflowApiClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "CRM-Workflow-Engine/1.0");
        });

        return services;
    }

    /// <summary>
    /// Adds workflow background workers to the service collection
    /// </summary>
    public static IServiceCollection AddWorkflowBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<WorkflowJobProcessorHostedService>();
        services.AddHostedService<WorkflowJobRecoveryHostedService>();
        services.AddHostedService<WorkflowSlaMonitorHostedService>();
        services.AddHostedService<WorkflowScheduleHostedService>();

        return services;
    }
}
