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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing CRM tasks
/// </summary>
public class TaskService : ITaskService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ICrmDbContext context, ILogger<TaskService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CrmTask>> GetTasksAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        CrmTaskStatus? status = null,
        CrmTaskPriority? priority = null,
        bool? overdue = null)
    {
        _logger.LogDebug(
            "Getting tasks with filters: AccountId={AccountId}, OpportunityId={OpportunityId}, AssignedTo={AssignedTo}, Status={Status}, Priority={Priority}, Overdue={Overdue}",
            accountId, opportunityId, assignedToUserId, status, priority, overdue);

        var query = _context.CrmTasks.AsNoTracking().Where(t => !t.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(t => t.AccountId == accountId);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(t => t.OpportunityId == opportunityId);
        }

        if (assignedToUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (overdue.HasValue && overdue.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != CrmTaskStatus.Completed && t.Status != CrmTaskStatus.Cancelled);
        }

        var tasks = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} tasks", tasks.Count);
        return tasks;
    }

    /// <inheritdoc />
    public async Task<CrmTask?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting task by ID: {TaskId}", id);

        var task = await _context.CrmTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (task == null)
        {
            _logger.LogWarning("Task not found: {TaskId}", id);
        }

        return task;
    }

    /// <inheritdoc />
    public async Task<CrmTask> CreateAsync(CrmTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _logger.LogDebug("Creating task: {Subject}", task.Subject);

        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        task.IsDeleted = false;

        if (task.Status == default)
        {
            task.Status = CrmTaskStatus.NotStarted;
        }

        if (task.Priority == default)
        {
            task.Priority = CrmTaskPriority.Normal;
        }

        _context.CrmTasks.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created task with ID: {TaskId}, Subject: {Subject}", task.Id, task.Subject);
        return task;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, CrmTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _logger.LogDebug("Updating task: {TaskId}", id);

        var existingTask = await _context.CrmTasks
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (existingTask == null)
        {
            _logger.LogWarning("Task not found for update: {TaskId}", id);
            return false;
        }

        // Update properties
        existingTask.Subject = task.Subject;
        existingTask.Description = task.Description;
        existingTask.TaskType = task.TaskType;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.StartDate = task.StartDate;
        existingTask.ReminderDate = task.ReminderDate;
        existingTask.AssignedToUserId = task.AssignedToUserId;
        existingTask.AccountId = task.AccountId;
        existingTask.ContactId = task.ContactId;
        existingTask.OpportunityId = task.OpportunityId;
        existingTask.CampaignId = task.CampaignId;
        existingTask.UpdatedAt = DateTime.UtcNow;

        // Handle status change to Completed
        if (task.Status == CrmTaskStatus.Completed && existingTask.CompletedDate == null)
        {
            existingTask.CompletedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated task: {TaskId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting task: {TaskId}", id);

        var task = await _context.CrmTasks
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (task == null)
        {
            _logger.LogWarning("Task not found for deletion: {TaskId}", id);
            return false;
        }

        // Soft delete
        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted task: {TaskId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CompleteAsync(int id)
    {
        _logger.LogDebug("Completing task: {TaskId}", id);

        var task = await _context.CrmTasks
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (task == null)
        {
            _logger.LogWarning("Task not found for completion: {TaskId}", id);
            return false;
        }

        task.Status = CrmTaskStatus.Completed;
        task.CompletedDate = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Completed task: {TaskId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CrmTask>> GetOverdueTasksAsync()
    {
        _logger.LogDebug("Getting all overdue tasks");

        var now = DateTime.UtcNow;

        var tasks = await _context.CrmTasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted &&
                        t.DueDate.HasValue &&
                        t.DueDate < now &&
                        t.Status != CrmTaskStatus.Completed &&
                        t.Status != CrmTaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} overdue tasks", tasks.Count);
        return tasks;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CrmTask>> GetTasksDueTodayAsync(int? userId = null)
    {
        _logger.LogDebug("Getting tasks due today for user: {UserId}", userId ?? 0);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _context.CrmTasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted &&
                        t.DueDate.HasValue &&
                        t.DueDate >= today &&
                        t.DueDate < tomorrow &&
                        t.Status != CrmTaskStatus.Completed &&
                        t.Status != CrmTaskStatus.Cancelled);

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == userId);
        }

        var tasks = await query
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} tasks due today", tasks.Count);
        return tasks;
    }

    /// <inheritdoc />
    public async Task<TaskStatistics> GetStatisticsAsync(int? userId = null)
    {
        _logger.LogDebug("Getting task statistics for user: {UserId}", userId ?? 0);

        var query = _context.CrmTasks.AsNoTracking().Where(t => !t.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == userId);
        }

        var tasks = await query.ToListAsync();
        var now = DateTime.UtcNow;
        var today = now.Date;
        var endOfWeek = today.AddDays(7);

        var statistics = new TaskStatistics
        {
            Total = tasks.Count,
            NotStarted = tasks.Count(t => t.Status == CrmTaskStatus.NotStarted),
            InProgress = tasks.Count(t => t.Status == CrmTaskStatus.InProgress),
            Completed = tasks.Count(t => t.Status == CrmTaskStatus.Completed),
            Overdue = tasks.Count(t => t.DueDate.HasValue &&
                                        t.DueDate < now &&
                                        t.Status != CrmTaskStatus.Completed &&
                                        t.Status != CrmTaskStatus.Cancelled),
            DueToday = tasks.Count(t => t.DueDate.HasValue &&
                                         t.DueDate >= today &&
                                         t.DueDate < today.AddDays(1) &&
                                         t.Status != CrmTaskStatus.Completed &&
                                         t.Status != CrmTaskStatus.Cancelled),
            DueThisWeek = tasks.Count(t => t.DueDate.HasValue &&
                                           t.DueDate >= today &&
                                           t.DueDate < endOfWeek &&
                                           t.Status != CrmTaskStatus.Completed &&
                                           t.Status != CrmTaskStatus.Cancelled)
        };

        _logger.LogInformation(
            "Task statistics - Total: {Total}, NotStarted: {NotStarted}, InProgress: {InProgress}, Completed: {Completed}, Overdue: {Overdue}",
            statistics.Total, statistics.NotStarted, statistics.InProgress, statistics.Completed, statistics.Overdue);

        return statistics;
    }
}
