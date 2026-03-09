// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for service request fulfillment automation.
/// </summary>
public interface ICatalogFulfillmentService
{
    /// <summary>
    /// Start fulfillment of an approved service request.
    /// </summary>
    Task<FulfillmentWorkflow> StartFulfillmentAsync(int serviceRequestId);

    /// <summary>
    /// Get the current fulfillment status.
    /// </summary>
    Task<FulfillmentWorkflow?> GetFulfillmentStatusAsync(int serviceRequestId);

    /// <summary>
    /// Complete a fulfillment task.
    /// </summary>
    Task<FulfillmentTask> CompleteTaskAsync(int taskId, int completedById, string? notes = null);

    /// <summary>
    /// Execute an automated fulfillment action.
    /// </summary>
    Task<AutomationResult> ExecuteAutomationAsync(int taskId);

    /// <summary>
    /// Get fulfillment templates for a catalog item.
    /// </summary>
    Task<FulfillmentTemplate> GetFulfillmentTemplateAsync(int catalogItemId);

    /// <summary>
    /// Create or update a fulfillment template.
    /// </summary>
    Task<FulfillmentTemplate> SaveFulfillmentTemplateAsync(FulfillmentTemplate template);

    /// <summary>
    /// Get fulfillment metrics for reporting.
    /// </summary>
    Task<FulfillmentMetrics> GetMetricsAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Cancel fulfillment of a service request.
    /// </summary>
    Task<bool> CancelFulfillmentAsync(int serviceRequestId, string reason, int cancelledById);
}

/// <summary>
/// Fulfillment workflow for a service request.
/// </summary>
public class FulfillmentWorkflow
{
    public int WorkflowId { get; set; }
    public int ServiceRequestId { get; set; }
    public string ServiceRequestNumber { get; set; } = string.Empty;
    public int CatalogItemId { get; set; }
    public string CatalogItemName { get; set; } = string.Empty;
    public FulfillmentState State { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CompletedTaskCount { get; set; }
    public int TotalTaskCount { get; set; }
    public decimal ProgressPercent { get; set; }
    public List<FulfillmentTask> Tasks { get; set; } = new();
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public string? CurrentTaskName { get; set; }
}

public enum FulfillmentState
{
    NotStarted,
    InProgress,
    WaitingForInput,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// A task in the fulfillment workflow.
/// </summary>
public class FulfillmentTask
{
    public int TaskId { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType Type { get; set; }
    public TaskState State { get; set; }
    public int? AssignedGroupId { get; set; }
    public string? AssignedGroupName { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public AutomationConfig? Automation { get; set; }
    public List<string> DependsOnTaskNames { get; set; } = new();
}

public enum TaskType
{
    Manual,
    Automated,
    Approval,
    Notification,
    Verification
}

public enum TaskState
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Skipped,
    Blocked
}

/// <summary>
/// Configuration for automated task execution.
/// </summary>
public class AutomationConfig
{
    public string AutomationType { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 300;
    public bool ContinueOnFailure { get; set; }
}

/// <summary>
/// Result of an automation execution.
/// </summary>
public class AutomationResult
{
    public int TaskId { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTime ExecutedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, string> OutputParameters { get; set; } = new();
    public string? ErrorDetails { get; set; }
    public int AttemptNumber { get; set; }
}

/// <summary>
/// Template defining fulfillment tasks for a catalog item.
/// </summary>
public class FulfillmentTemplate
{
    public int TemplateId { get; set; }
    public int CatalogItemId { get; set; }
    public string CatalogItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<TaskDefinition> TaskDefinitions { get; set; } = new();
    public int EstimatedMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Definition of a task in a template.
/// </summary>
public class TaskDefinition
{
    public int Sequence { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType Type { get; set; }
    public int? AssignedGroupId { get; set; }
    public string? AssignedGroupName { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool IsRequired { get; set; } = true;
    public AutomationConfig? Automation { get; set; }
    public List<string> DependsOnTasks { get; set; } = new();
}

/// <summary>
/// Fulfillment metrics for reporting.
/// </summary>
public class FulfillmentMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalRequestsFulfilled { get; set; }
    public int TotalRequestsCancelled { get; set; }
    public int TotalRequestsPending { get; set; }
    public double AverageFulfillmentTimeHours { get; set; }
    public double AutomationSuccessRate { get; set; }
    public Dictionary<string, int> RequestsByCategory { get; set; } = new();
    public Dictionary<string, double> AverageTimeByCategory { get; set; } = new();
    public List<TopFulfilledItem> TopItems { get; set; } = new();
}

public class TopFulfilledItem
{
    public int CatalogItemId { get; set; }
    public string CatalogItemName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageTimeHours { get; set; }
}

/// <summary>
/// Service for automating service request fulfillment.
/// </summary>
public class CatalogFulfillmentService : ICatalogFulfillmentService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CatalogFulfillmentService> _logger;

    // In-memory storage (would be database in production)
    private static readonly List<FulfillmentWorkflow> _workflows = new();
    private static readonly Dictionary<int, FulfillmentTemplate> _templates = new();
    private static readonly object _workflowsLock = new();
    private static int _nextWorkflowId = 0;
    private static int _nextTaskId = 0;

    static CatalogFulfillmentService()
    {
        // Initialize default templates
        _templates[1] = new FulfillmentTemplate
        {
            TemplateId = 1,
            CatalogItemId = 1,
            CatalogItemName = "New Laptop Request",
            Description = "Fulfill a new laptop request for an employee",
            EstimatedMinutes = 480, // 8 hours
            TaskDefinitions = new List<TaskDefinition>
            {
                new()
                {
                    Sequence = 1,
                    Name = "Verify Stock Availability",
                    Type = TaskType.Automated,
                    AssignedGroupName = "IT Assets",
                    EstimatedMinutes = 5,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "CheckInventory",
                        Parameters = new Dictionary<string, string> { { "ItemType", "Laptop" } }
                    }
                },
                new()
                {
                    Sequence = 2,
                    Name = "Reserve Equipment",
                    Type = TaskType.Manual,
                    AssignedGroupId = 1,
                    AssignedGroupName = "IT Assets",
                    EstimatedMinutes = 15,
                    DependsOnTasks = new List<string> { "Verify Stock Availability" }
                },
                new()
                {
                    Sequence = 3,
                    Name = "Create User Account",
                    Type = TaskType.Automated,
                    EstimatedMinutes = 5,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "CreateADAccount",
                        ContinueOnFailure = false
                    }
                },
                new()
                {
                    Sequence = 4,
                    Name = "Configure Laptop",
                    Type = TaskType.Manual,
                    AssignedGroupId = 1,
                    AssignedGroupName = "IT Support",
                    EstimatedMinutes = 120,
                    DependsOnTasks = new List<string> { "Reserve Equipment", "Create User Account" }
                },
                new()
                {
                    Sequence = 5,
                    Name = "Install Required Software",
                    Type = TaskType.Automated,
                    EstimatedMinutes = 60,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "SoftwareDeployment",
                        TimeoutSeconds = 3600
                    },
                    DependsOnTasks = new List<string> { "Configure Laptop" }
                },
                new()
                {
                    Sequence = 6,
                    Name = "Quality Check",
                    Type = TaskType.Verification,
                    AssignedGroupId = 1,
                    AssignedGroupName = "IT Support",
                    EstimatedMinutes = 30,
                    DependsOnTasks = new List<string> { "Install Required Software" }
                },
                new()
                {
                    Sequence = 7,
                    Name = "Schedule Delivery",
                    Type = TaskType.Manual,
                    AssignedGroupId = 2,
                    AssignedGroupName = "Facilities",
                    EstimatedMinutes = 15,
                    DependsOnTasks = new List<string> { "Quality Check" }
                },
                new()
                {
                    Sequence = 8,
                    Name = "Notify User",
                    Type = TaskType.Notification,
                    EstimatedMinutes = 1,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "SendEmail",
                        Parameters = new Dictionary<string, string>
                        {
                            { "Template", "LaptopReady" }
                        }
                    },
                    DependsOnTasks = new List<string> { "Schedule Delivery" }
                }
            },
            CreatedAt = DateTime.UtcNow
        };

        _templates[2] = new FulfillmentTemplate
        {
            TemplateId = 2,
            CatalogItemId = 2,
            CatalogItemName = "Software License Request",
            Description = "Fulfill a software license request",
            EstimatedMinutes = 60,
            TaskDefinitions = new List<TaskDefinition>
            {
                new()
                {
                    Sequence = 1,
                    Name = "Check License Availability",
                    Type = TaskType.Automated,
                    EstimatedMinutes = 5,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "CheckLicensePool"
                    }
                },
                new()
                {
                    Sequence = 2,
                    Name = "Assign License",
                    Type = TaskType.Automated,
                    EstimatedMinutes = 5,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "AssignLicense"
                    },
                    DependsOnTasks = new List<string> { "Check License Availability" }
                },
                new()
                {
                    Sequence = 3,
                    Name = "Notify User",
                    Type = TaskType.Notification,
                    EstimatedMinutes = 1,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "SendEmail",
                        Parameters = new Dictionary<string, string>
                        {
                            { "Template", "LicenseAssigned" }
                        }
                    },
                    DependsOnTasks = new List<string> { "Assign License" }
                }
            },
            CreatedAt = DateTime.UtcNow
        };

        _templates[3] = new FulfillmentTemplate
        {
            TemplateId = 3,
            CatalogItemId = 3,
            CatalogItemName = "Password Reset",
            Description = "Reset user password",
            EstimatedMinutes = 5,
            TaskDefinitions = new List<TaskDefinition>
            {
                new()
                {
                    Sequence = 1,
                    Name = "Verify Identity",
                    Type = TaskType.Verification,
                    AssignedGroupId = 1,
                    AssignedGroupName = "Help Desk",
                    EstimatedMinutes = 3
                },
                new()
                {
                    Sequence = 2,
                    Name = "Reset Password",
                    Type = TaskType.Automated,
                    EstimatedMinutes = 1,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "ResetADPassword"
                    },
                    DependsOnTasks = new List<string> { "Verify Identity" }
                },
                new()
                {
                    Sequence = 3,
                    Name = "Send New Credentials",
                    Type = TaskType.Notification,
                    EstimatedMinutes = 1,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "SendSecureEmail",
                        Parameters = new Dictionary<string, string>
                        {
                            { "Template", "PasswordReset" }
                        }
                    },
                    DependsOnTasks = new List<string> { "Reset Password" }
                }
            },
            CreatedAt = DateTime.UtcNow
        };
    }

    public CatalogFulfillmentService(
        ICrmDbContext context,
        ILogger<CatalogFulfillmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FulfillmentWorkflow> StartFulfillmentAsync(int serviceRequestId)
    {
        var serviceRequest = await _context.CatalogRequests
            .Include(sr => sr.CatalogItem)
            .FirstOrDefaultAsync(sr => sr.RequestId == serviceRequestId);

        if (serviceRequest == null)
        {
            throw new ArgumentException($"Service request {serviceRequestId} not found");
        }

        // Check if workflow already exists
        FulfillmentWorkflow? existingWorkflow;
        lock (_workflowsLock) { existingWorkflow = _workflows.FirstOrDefault(w => w.ServiceRequestId == serviceRequestId); }
        if (existingWorkflow != null)
        {
            return existingWorkflow;
        }

        // Get template for catalog item
        var template = await GetFulfillmentTemplateAsync(serviceRequest.CatalogItemId);

        // Create workflow
        var workflow = new FulfillmentWorkflow
        {
            WorkflowId = Interlocked.Increment(ref _nextWorkflowId),
            ServiceRequestId = serviceRequestId,
            ServiceRequestNumber = serviceRequest.RequestId.ToString(),
            CatalogItemId = serviceRequest.CatalogItemId,
            CatalogItemName = serviceRequest.CatalogItem?.Name ?? "Unknown",
            State = FulfillmentState.InProgress,
            StartedAt = DateTime.UtcNow,
            TotalTaskCount = template.TaskDefinitions.Count,
            Tasks = new List<FulfillmentTask>()
        };

        // Create tasks from template
        foreach (var taskDef in template.TaskDefinitions)
        {
            var task = new FulfillmentTask
            {
                TaskId = Interlocked.Increment(ref _nextTaskId),
                Sequence = taskDef.Sequence,
                Name = taskDef.Name,
                Description = taskDef.Description,
                Type = taskDef.Type,
                State = TaskState.Pending,
                AssignedGroupId = taskDef.AssignedGroupId,
                AssignedGroupName = taskDef.AssignedGroupName,
                DueAt = DateTime.UtcNow.AddMinutes(taskDef.EstimatedMinutes),
                Automation = taskDef.Automation,
                DependsOnTaskNames = taskDef.DependsOnTasks
            };

            workflow.Tasks.Add(task);
        }

        // Start first task(s) - those with no dependencies
        foreach (var task in workflow.Tasks.Where(t => !t.DependsOnTaskNames.Any()))
        {
            task.State = TaskState.InProgress;
            task.StartedAt = DateTime.UtcNow;
        }

        workflow.CurrentTaskName = workflow.Tasks.FirstOrDefault(t => t.State == TaskState.InProgress)?.Name;
        workflow.EstimatedTimeRemaining = TimeSpan.FromMinutes(template.EstimatedMinutes);

        lock (_workflowsLock) { _workflows.Add(workflow); }

        // Update service request status
        serviceRequest.State = CatalogRequestState.InProgress;
        serviceRequest.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Started fulfillment workflow {WorkflowId} for service request {RequestId}",
            workflow.WorkflowId, serviceRequest.RequestId);

        // Auto-execute first automated tasks
        foreach (var task in workflow.Tasks.Where(t => t.State == TaskState.InProgress && t.Type == TaskType.Automated))
        {
            _ = ExecuteAutomationAsync(task.TaskId);
        }

        return workflow;
    }

    public async Task<FulfillmentWorkflow?> GetFulfillmentStatusAsync(int serviceRequestId)
    {
        FulfillmentWorkflow? workflow;
        lock (_workflowsLock) { workflow = _workflows.FirstOrDefault(w => w.ServiceRequestId == serviceRequestId); }
        if (workflow != null)
        {
            UpdateWorkflowProgress(workflow);
        }
        return await Task.FromResult(workflow);
    }

    public async Task<FulfillmentTask> CompleteTaskAsync(int taskId, int completedById, string? notes = null)
    {
        FulfillmentWorkflow? workflow;
        lock (_workflowsLock) { workflow = _workflows.FirstOrDefault(w => w.Tasks.Any(t => t.TaskId == taskId)); }
        if (workflow == null)
        {
            throw new ArgumentException($"Task {taskId} not found");
        }

        var task = workflow.Tasks.First(t => t.TaskId == taskId);

        if (task.State != TaskState.InProgress)
        {
            throw new InvalidOperationException($"Task {taskId} is not in progress");
        }
        var user = await _context.Users.FindAsync(completedById);

        task.State = TaskState.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.Notes = notes;
        task.AssignedToId = completedById;
        task.AssignedToName = user != null ? (user.FirstName + " " + user.LastName) : "Unknown";

        _logger.LogInformation(
            "Task {TaskName} completed for workflow {WorkflowId}",
            task.Name, workflow.WorkflowId);

        // Start dependent tasks
        StartDependentTasks(workflow, task.Name);

        // Check if workflow is complete
        UpdateWorkflowProgress(workflow);

        if (workflow.CompletedTaskCount == workflow.TotalTaskCount)
        {
            await CompleteWorkflowAsync(workflow);
        }

        return task;
    }

    public async Task<AutomationResult> ExecuteAutomationAsync(int taskId)
    {
        FulfillmentWorkflow? autoWorkflow;
        lock (_workflowsLock) { autoWorkflow = _workflows.FirstOrDefault(w => w.Tasks.Any(t => t.TaskId == taskId)); }
        if (autoWorkflow == null)
        {
            throw new ArgumentException($"Task {taskId} not found");
        }

        var workflow = autoWorkflow;
        var task = workflow.Tasks.First(t => t.TaskId == taskId);

        if (task.Automation == null)
        {
            throw new InvalidOperationException($"Task {taskId} has no automation configuration");
        }

        var startTime = DateTime.UtcNow;
        var result = new AutomationResult
        {
            TaskId = taskId,
            ExecutedAt = startTime,
            AttemptNumber = 1,
            OutputParameters = new Dictionary<string, string>()
        };

        try
        {
            // Simulate automation execution based on type
            await Task.Delay(100); // Simulate processing time

            switch (task.Automation.AutomationType)
            {
                case "CheckInventory":
                    result.Success = true;
                    result.Message = "Inventory check completed - item available";
                    result.OutputParameters["Available"] = "true";
                    result.OutputParameters["Quantity"] = "5";
                    break;

                case "CreateADAccount":
                    result.Success = true;
                    result.Message = "Active Directory account created successfully";
                    result.OutputParameters["Username"] = "jdoe";
                    break;

                case "SoftwareDeployment":
                    result.Success = true;
                    result.Message = "Software deployment initiated";
                    result.OutputParameters["DeploymentId"] = Guid.NewGuid().ToString();
                    break;

                case "CheckLicensePool":
                    result.Success = true;
                    result.Message = "License available in pool";
                    result.OutputParameters["AvailableLicenses"] = "10";
                    break;

                case "AssignLicense":
                    result.Success = true;
                    result.Message = "License assigned to user";
                    result.OutputParameters["LicenseKey"] = "XXXX-XXXX-XXXX";
                    break;

                case "ResetADPassword":
                    result.Success = true;
                    result.Message = "Password reset successfully";
                    break;

                case "SendEmail":
                case "SendSecureEmail":
                    result.Success = true;
                    result.Message = "Email notification sent";
                    break;

                default:
                    result.Success = true;
                    result.Message = $"Automation {task.Automation.AutomationType} executed";
                    break;
            }

            result.Duration = DateTime.UtcNow - startTime;

            if (result.Success)
            {
                task.State = TaskState.Completed;
                task.CompletedAt = DateTime.UtcNow;
                task.Notes = result.Message;
                task.Parameters = result.OutputParameters;

                // Start dependent tasks
                StartDependentTasks(workflow, task.Name);
                UpdateWorkflowProgress(workflow);

                if (workflow.CompletedTaskCount == workflow.TotalTaskCount)
                {
                    await CompleteWorkflowAsync(workflow);
                }
            }
            else if (!task.Automation.ContinueOnFailure)
            {
                task.State = TaskState.Failed;
                workflow.State = FulfillmentState.Failed;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorDetails = ex.Message;
            result.Duration = DateTime.UtcNow - startTime;

            if (!task.Automation.ContinueOnFailure)
            {
                task.State = TaskState.Failed;
                workflow.State = FulfillmentState.Failed;
            }

            _logger.LogError(ex, "Automation failed for task {TaskId}", taskId);
        }

        return result;
    }

    public async Task<FulfillmentTemplate> GetFulfillmentTemplateAsync(int catalogItemId)
    {
        if (_templates.TryGetValue(catalogItemId, out var template))
        {
            return await Task.FromResult(template);
        }

        // Default template if not configured
        return new FulfillmentTemplate
        {
            TemplateId = 0,
            CatalogItemId = catalogItemId,
            Description = "Default fulfillment template",
            EstimatedMinutes = 60,
            TaskDefinitions = new List<TaskDefinition>
            {
                new()
                {
                    Sequence = 1,
                    Name = "Fulfill Request",
                    Type = TaskType.Manual,
                    AssignedGroupId = 1,
                    AssignedGroupName = "Service Desk",
                    EstimatedMinutes = 60,
                    IsRequired = true
                },
                new()
                {
                    Sequence = 2,
                    Name = "Notify Requestor",
                    Type = TaskType.Notification,
                    EstimatedMinutes = 1,
                    Automation = new AutomationConfig
                    {
                        AutomationType = "SendEmail",
                        Parameters = new Dictionary<string, string>
                        {
                            { "Template", "RequestComplete" }
                        }
                    },
                    DependsOnTasks = new List<string> { "Fulfill Request" }
                }
            },
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<FulfillmentTemplate> SaveFulfillmentTemplateAsync(FulfillmentTemplate template)
    {
        if (template.TemplateId == 0)
        {
            template.TemplateId = _templates.Count + 1;
            template.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            template.ModifiedAt = DateTime.UtcNow;
        }

        _templates[template.CatalogItemId] = template;

        _logger.LogInformation(
            "Saved fulfillment template for catalog item {CatalogItemId}",
            template.CatalogItemId);

        return await Task.FromResult(template);
    }

    public async Task<FulfillmentMetrics> GetMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        List<FulfillmentWorkflow> wfSnapshot;
        lock (_workflowsLock) { wfSnapshot = _workflows.ToList(); }
        var workflowsInRange = wfSnapshot
            .Where(w => w.StartedAt >= fromDate && w.StartedAt <= toDate)
            .ToList();

        var completed = workflowsInRange.Where(w => w.State == FulfillmentState.Completed).ToList();
        var cancelled = workflowsInRange.Where(w => w.State == FulfillmentState.Cancelled).ToList();
        var pending = workflowsInRange.Where(w => w.State == FulfillmentState.InProgress).ToList();

        var metrics = new FulfillmentMetrics
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRequestsFulfilled = completed.Count,
            TotalRequestsCancelled = cancelled.Count,
            TotalRequestsPending = pending.Count,
            AverageFulfillmentTimeHours = completed.Any()
                ? completed.Where(w => w.CompletedAt.HasValue)
                           .Average(w => (w.CompletedAt!.Value - w.StartedAt).TotalHours)
                : 0,
            RequestsByCategory = workflowsInRange
                .GroupBy(w => w.CatalogItemName)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopItems = workflowsInRange
                .GroupBy(w => new { w.CatalogItemId, w.CatalogItemName })
                .Select(g => new TopFulfilledItem
                {
                    CatalogItemId = g.Key.CatalogItemId,
                    CatalogItemName = g.Key.CatalogItemName,
                    Count = g.Count(),
                    AverageTimeHours = g.Where(w => w.CompletedAt.HasValue)
                                        .Select(w => (w.CompletedAt!.Value - w.StartedAt).TotalHours)
                                        .DefaultIfEmpty(0)
                                        .Average()
                })
                .OrderByDescending(i => i.Count)
                .Take(5)
                .ToList()
        };

        // Calculate automation success rate
        var automatedTasks = workflowsInRange
            .SelectMany(w => w.Tasks)
            .Where(t => t.Type == TaskType.Automated)
            .ToList();

        metrics.AutomationSuccessRate = automatedTasks.Any()
            ? (double)automatedTasks.Count(t => t.State == TaskState.Completed) / automatedTasks.Count * 100
            : 100;

        return await Task.FromResult(metrics);
    }

    public async Task<bool> CancelFulfillmentAsync(int serviceRequestId, string reason, int cancelledById)
    {
        FulfillmentWorkflow? cancelWorkflow;
        lock (_workflowsLock) { cancelWorkflow = _workflows.FirstOrDefault(w => w.ServiceRequestId == serviceRequestId); }
        var workflow = cancelWorkflow;
        if (workflow == null) return false;

        if (workflow.State == FulfillmentState.Completed)
        {
            return false; // Can't cancel completed workflow
        }

        workflow.State = FulfillmentState.Cancelled;
        workflow.CompletedAt = DateTime.UtcNow;

        // Cancel all pending tasks
        foreach (var task in workflow.Tasks.Where(t => t.State == TaskState.Pending || t.State == TaskState.InProgress))
        {
            task.State = TaskState.Skipped;
            task.Notes = $"Cancelled: {reason}";
        }
        var sr = await _context.CatalogRequests.FindAsync(serviceRequestId);
        if (sr != null)
        {
            sr.State = CatalogRequestState.Cancelled;
            sr.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Fulfillment workflow {WorkflowId} cancelled: {Reason}",
            workflow.WorkflowId, reason);

        return true;
    }

    private void StartDependentTasks(FulfillmentWorkflow workflow, string completedTaskName)
    {
        var dependentTasks = workflow.Tasks
            .Where(t => t.State == TaskState.Pending &&
                       t.DependsOnTaskNames.Contains(completedTaskName))
            .ToList();

        foreach (var task in dependentTasks)
        {
            // Check if all dependencies are completed
            var allDependenciesMet = task.DependsOnTaskNames.All(depName =>
                workflow.Tasks.Any(t => t.Name == depName && t.State == TaskState.Completed));

            if (allDependenciesMet)
            {
                task.State = TaskState.InProgress;
                task.StartedAt = DateTime.UtcNow;

                // Auto-execute if automated
                if (task.Type == TaskType.Automated || task.Type == TaskType.Notification)
                {
                    _ = ExecuteAutomationAsync(task.TaskId);
                }
            }
        }
    }

    private void UpdateWorkflowProgress(FulfillmentWorkflow workflow)
    {
        workflow.CompletedTaskCount = workflow.Tasks.Count(t => t.State == TaskState.Completed);
        workflow.ProgressPercent = workflow.TotalTaskCount > 0
            ? (decimal)workflow.CompletedTaskCount / workflow.TotalTaskCount * 100
            : 0;

        var currentTask = workflow.Tasks.FirstOrDefault(t => t.State == TaskState.InProgress);
        workflow.CurrentTaskName = currentTask?.Name;

        // Estimate remaining time
        var remainingTasks = workflow.Tasks.Where(t =>
            t.State != TaskState.Completed && t.State != TaskState.Skipped).ToList();

        if (remainingTasks.Any() && remainingTasks.First().DueAt.HasValue)
        {
            workflow.EstimatedTimeRemaining = remainingTasks.First().DueAt - DateTime.UtcNow;
        }
    }

    private async Task CompleteWorkflowAsync(FulfillmentWorkflow workflow)
    {
        workflow.State = FulfillmentState.Completed;
        workflow.CompletedAt = DateTime.UtcNow;
        workflow.ProgressPercent = 100;
        var sr = await _context.CatalogRequests.FindAsync(workflow.ServiceRequestId);
        if (sr != null)
        {
            sr.State = CatalogRequestState.Completed;
            sr.CompletedAt = DateTime.UtcNow;
            sr.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Fulfillment workflow {WorkflowId} completed for service request {RequestNumber}",
            workflow.WorkflowId, workflow.ServiceRequestNumber);
    }
}


