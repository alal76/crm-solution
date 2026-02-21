// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing CRM tasks
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Get tasks with optional filtering
    /// </summary>
    Task<IEnumerable<CrmTask>> GetTasksAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        CrmTaskStatus? status = null,
        CrmTaskPriority? priority = null,
        bool? overdue = null);

    /// <summary>
    /// Get a task by ID
    /// </summary>
    Task<CrmTask?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new task
    /// </summary>
    Task<CrmTask> CreateAsync(CrmTask task);

    /// <summary>
    /// Update an existing task
    /// </summary>
    Task<bool> UpdateAsync(int id, CrmTask task);

    /// <summary>
    /// Delete a task
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Mark a task as complete
    /// </summary>
    Task<bool> CompleteAsync(int id);

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    Task<IEnumerable<CrmTask>> GetOverdueTasksAsync();

    /// <summary>
    /// Get tasks due today
    /// </summary>
    Task<IEnumerable<CrmTask>> GetTasksDueTodayAsync(int? userId = null);

    /// <summary>
    /// Get task statistics
    /// </summary>
    Task<TaskStatistics> GetStatisticsAsync(int? userId = null);
}

/// <summary>
/// Task statistics DTO
/// </summary>
public class TaskStatistics
{
    public int Total { get; set; }
    public int NotStarted { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Overdue { get; set; }
    public int DueToday { get; set; }
    public int DueThisWeek { get; set; }
}
