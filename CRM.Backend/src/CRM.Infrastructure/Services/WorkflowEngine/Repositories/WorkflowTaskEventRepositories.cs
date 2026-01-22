using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.Repositories;

/// <summary>
/// Repository for WorkflowTask entities
/// </summary>
public class WorkflowTaskRepository : IWorkflowTaskRepository
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowTaskRepository> _logger;

    public WorkflowTaskRepository(
        CrmDbContext context,
        ILogger<WorkflowTaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowTasks
            .Include(t => t.WorkflowInstance)
            .ThenInclude(i => i!.WorkflowDefinition)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<WorkflowTask>> GetPendingTasksAsync(
        int? userId = null, 
        int? groupId = null, 
        string? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowTasks
            .Include(t => t.WorkflowInstance)
            .ThenInclude(i => i!.WorkflowDefinition)
            .Where(t => t.Status == WorkflowTaskStatus.Pending || t.Status == WorkflowTaskStatus.InProgress);

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == userId);
        }

        if (groupId.HasValue)
        {
            query = query.Where(t => t.AssignedToGroupId == groupId);
        }

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(t => t.AssignedToRole == role);
        }

        return await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueAt)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WorkflowTask>> GetByInstanceAsync(
        int instanceId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowTasks
            .Include(t => t.AssignedToUser)
            .Where(t => t.WorkflowInstanceId == instanceId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WorkflowTask>> GetOverdueTasksAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.WorkflowTasks
            .Include(t => t.WorkflowInstance)
            .ThenInclude(i => i!.WorkflowDefinition)
            .Where(t => (t.Status == WorkflowTaskStatus.Pending || t.Status == WorkflowTaskStatus.InProgress)
                     && t.DueAt.HasValue 
                     && t.DueAt < now)
            .OrderBy(t => t.DueAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowTask> CreateAsync(
        WorkflowTask task, 
        CancellationToken cancellationToken = default)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.WorkflowTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created workflow task {Id} for instance {InstanceId}",
            task.Id, task.WorkflowInstanceId);

        return task;
    }

    public async Task<WorkflowTask> UpdateAsync(
        WorkflowTask task, 
        CancellationToken cancellationToken = default)
    {
        task.UpdatedAt = DateTime.UtcNow;

        _context.WorkflowTasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);

        return task;
    }

    public async Task<WorkflowTask> CompleteAsync(
        int taskId, 
        CompleteTaskDto request, 
        int userId,
        CancellationToken cancellationToken = default)
    {
        var task = await _context.WorkflowTasks.FindAsync(new object[] { taskId }, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }

        task.Status = WorkflowTaskStatus.Completed;
        task.CompletedByUserId = userId;
        task.CompletedAt = DateTime.UtcNow;
        task.ActionTaken = request.ActionTaken;
        task.FormData = request.FormData != null ? JsonSerializer.Serialize(request.FormData) : null;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} completed by user {UserId} with action {Action}",
            taskId, userId, request.ActionTaken);

        return task;
    }
}

/// <summary>
/// Repository for WorkflowEvent entities (audit log)
/// </summary>
public class WorkflowEventRepository : IWorkflowEventRepository
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WorkflowEventRepository> _logger;

    public WorkflowEventRepository(
        CrmDbContext context,
        ILogger<WorkflowEventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<WorkflowEvent>> GetByInstanceAsync(
        int instanceId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowEvents
            .Where(e => e.WorkflowInstanceId == instanceId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowEvent> LogEventAsync(
        WorkflowEvent workflowEvent,
        CancellationToken cancellationToken = default)
    {
        workflowEvent.Timestamp = DateTime.UtcNow;

        _context.WorkflowEvents.Add(workflowEvent);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Logged workflow event {EventType} for instance {InstanceId}",
            workflowEvent.EventType, workflowEvent.WorkflowInstanceId);

        return workflowEvent;
    }

    public async Task<List<WorkflowEvent>> QueryEventsAsync(
        DateTime? from = null, 
        DateTime? to = null, 
        string? eventType = null, 
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowEvents.AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(e => e.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.Timestamp <= to.Value);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        query = query.OrderByDescending(e => e.Timestamp);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
