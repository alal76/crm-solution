// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing CRM tasks.
/// Provides comprehensive task management including creation, updates, completion tracking,
/// filtering by status/priority/assignee, and user queue management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TasksController : CrmControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<TasksController> _logger;
    private readonly NormalizationService _normalization;

    public TasksController(CrmDbContext context, ILogger<TasksController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    /// <summary>
    /// Get all tasks with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CrmTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CrmTaskDto>>> GetTasks(
        [FromQuery] int? accountId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? assignedToUserId = null,
        [FromQuery] CrmTaskStatus? status = null,
        [FromQuery] CrmTaskPriority? priority = null,
        [FromQuery] bool? overdue = null)
    {
        var query = _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Opportunity)
            .Include(t => t.AssignedToUser)
            .AsQueryable();

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
        if (overdue == true)
        {
            query = query.Where(t => t.DueDate < DateTime.UtcNow && t.Status != CrmTaskStatus.Completed);
        }

        var tasks = await query.OrderByDescending(t => t.DueDate).ToListAsync();
        var dtos = tasks.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get a task by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <returns>The task with the specified ID.</returns>
    /// <response code="200">Returns the task.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CrmTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CrmTaskDto>> GetTask(int id)
    {
        var task = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Opportunity)
            .Include(t => t.AssignedToUser)
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Create a new task.
    /// </summary>
    /// <param name="dto">The task to create.</param>
    /// <returns>The created task with generated ID.</returns>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">Invalid task data provided.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CrmTaskDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CrmTaskDto>> CreateTask([FromBody] CreateCrmTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest("Title is required.");
        }
        var task = MapFromCreateDto(dto);
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.CrmTasks.Add(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} created: {Subject}", task.Id, task.Subject);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, MapToDto(task));
    }

    /// <summary>
    /// Update an existing task.
    /// </summary>
    /// <param name="id">The unique identifier of the task to update.</param>
    /// <param name="dto">The updated task data.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Task updated successfully.</response>
    /// <response code="400">Invalid task data or ID mismatch.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateCrmTaskDto dto)
    {
        var task = await _context.CrmTasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        MapFromUpdateDto(dto, task);
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Task deleted successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.CrmTasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        _context.CrmTasks.Remove(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} deleted", id);
        return NoContent();
    }

    /// <summary>
    /// Mark a task as complete.
    /// </summary>
    /// <param name="id">The unique identifier of the task to complete.</param>
    /// <returns>The completed task.</returns>
    /// <response code="200">Task marked as complete successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}/complete")]
    [ProducesResponseType(typeof(CrmTaskDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteTask(int id)
    {
        var task = await _context.CrmTasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        task.Status = CrmTaskStatus.Completed;
        task.CompletedDate = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Get tasks due today.
    /// </summary>
    /// <returns>A list of tasks due today that are not yet completed.</returns>
    /// <response code="200">Returns the list of tasks due today.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("due-today")]
    [ProducesResponseType(typeof(IEnumerable<CrmTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CrmTaskDto>>> GetTasksDueToday()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var tasks = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.AssignedToUser)
            .Where(t => t.DueDate >= today && t.DueDate < tomorrow && t.Status != CrmTaskStatus.Completed)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
        return Ok(tasks.Select(MapToDto));
    }

    /// <summary>
    /// Get overdue tasks.
    /// </summary>
    /// <returns>A list of tasks that are past their due date and not completed or cancelled.</returns>
    /// <response code="200">Returns the list of overdue tasks.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<CrmTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CrmTaskDto>>> GetOverdueTasks()
    {
        var tasks = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.AssignedToUser)
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != CrmTaskStatus.Completed && t.Status != CrmTaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
        return Ok(tasks.Select(MapToDto));
    }
    // Mapping helpers
    private static CrmTaskDto MapToDto(CrmTask t)
    {
        return new CrmTaskDto
        {
            Id = t.Id,
            Title = t.Subject,
            Description = t.Description,
            Status = (int)t.Status,
            Priority = (int)t.Priority,
            DueDate = t.DueDate?.ToString("o"),
            CompletedDate = t.CompletedDate?.ToString("o"),
            OwnerUserId = t.CreatedByUserId ?? 0,
            CreatedByUserId = t.CreatedByUserId ?? 0,
            CreatedAt = t.CreatedAt.ToString("o"),
            UpdatedAt = t.UpdatedAt?.ToString("o") ?? string.Empty,
            IsDeleted = t.IsDeleted,
            RowVersion = t.RowVersion,
            ReminderDate = t.ReminderDate?.ToString("o"),
            HasReminder = t.HasReminder,
            PercentComplete = t.PercentComplete,
            ActualMinutes = t.ActualMinutes,
            IsRecurring = t.IsRecurring,
            RecurrencePattern = t.RecurrencePattern,
            RecurrenceEndDate = t.RecurrenceEndDate?.ToString("o"),
            ParentTaskId = t.ParentTaskId,
            ContactId = t.ContactId,
            CampaignId = t.CampaignId,
            AssignedToGroupId = t.AssignedToGroupId,
            Tags = t.Tags,
            Category = t.Category,
            Attachments = t.Attachments,
            CustomFields = t.CustomFields,
            TaskType = (int)t.TaskType,
            StartDate = t.StartDate?.ToString("o"),
            EstimatedMinutes = t.EstimatedMinutes,
            AccountId = t.AccountId,
            OpportunityId = t.OpportunityId
        };
    }

    private static CrmTask MapFromCreateDto(CreateCrmTaskDto dto)
    {
        return new CrmTask
        {
            Subject = dto.Title,
            Description = dto.Description,
            Priority = (CrmTaskPriority)dto.Priority,
            DueDate = string.IsNullOrWhiteSpace(dto.DueDate) ? null : DateTime.Parse(dto.DueDate),
            CreatedByUserId = dto.OwnerUserId,
            AccountId = dto.AccountId,
            OpportunityId = dto.OpportunityId,
            AssignedToUserId = dto.AssignedToUserId,
            ReminderDate = string.IsNullOrWhiteSpace(dto.ReminderDate) ? null : DateTime.Parse(dto.ReminderDate),
            HasReminder = dto.HasReminder,
            PercentComplete = dto.PercentComplete,
            ActualMinutes = dto.ActualMinutes,
            IsRecurring = dto.IsRecurring,
            RecurrencePattern = dto.RecurrencePattern,
            RecurrenceEndDate = string.IsNullOrWhiteSpace(dto.RecurrenceEndDate) ? null : DateTime.Parse(dto.RecurrenceEndDate),
            ParentTaskId = dto.ParentTaskId,
            ContactId = dto.ContactId,
            CampaignId = dto.CampaignId,
            AssignedToGroupId = dto.AssignedToGroupId,
            Tags = dto.Tags,
            Category = dto.Category,
            Attachments = dto.Attachments,
            CustomFields = dto.CustomFields
        };
    }

    private static void MapFromUpdateDto(UpdateCrmTaskDto dto, CrmTask task)
    {
        if (dto.Title != null)
        {
            task.Subject = dto.Title;
        }
        if (dto.Description != null)
        {
            task.Description = dto.Description;
        }
        if (dto.Status.HasValue)
        {
            task.Status = (CrmTaskStatus)dto.Status.Value;
        }
        if (dto.Priority.HasValue)
        {
            task.Priority = (CrmTaskPriority)dto.Priority.Value;
        }
        if (dto.DueDate != null)
        {
            task.DueDate = DateTime.Parse(dto.DueDate);
        }
        if (dto.CompletedDate != null)
        {
            task.CompletedDate = DateTime.Parse(dto.CompletedDate);
        }
        if (dto.AssignedToUserId.HasValue)
        {
            task.AssignedToUserId = dto.AssignedToUserId;
        }
        if (dto.ReminderDate != null)
        {
            task.ReminderDate = string.IsNullOrWhiteSpace(dto.ReminderDate) ? null : DateTime.Parse(dto.ReminderDate);
        }
        if (dto.HasReminder.HasValue)
        {
            task.HasReminder = dto.HasReminder.Value;
        }
        if (dto.PercentComplete.HasValue)
        {
            task.PercentComplete = dto.PercentComplete.Value;
        }
        if (dto.ActualMinutes.HasValue)
        {
            task.ActualMinutes = dto.ActualMinutes;
        }
        if (dto.IsRecurring.HasValue)
        {
            task.IsRecurring = dto.IsRecurring.Value;
        }
        if (dto.RecurrencePattern != null)
        {
            task.RecurrencePattern = dto.RecurrencePattern;
        }
        if (dto.RecurrenceEndDate != null)
        {
            task.RecurrenceEndDate = string.IsNullOrWhiteSpace(dto.RecurrenceEndDate) ? null : DateTime.Parse(dto.RecurrenceEndDate);
        }
        if (dto.ParentTaskId.HasValue)
        {
            task.ParentTaskId = dto.ParentTaskId;
        }
        if (dto.ContactId.HasValue)
        {
            task.ContactId = dto.ContactId;
        }
        if (dto.CampaignId.HasValue)
        {
            task.CampaignId = dto.CampaignId;
        }
        if (dto.AssignedToGroupId.HasValue)
        {
            task.AssignedToGroupId = dto.AssignedToGroupId;
        }
        if (dto.Tags != null)
        {
            task.Tags = dto.Tags;
        }
        if (dto.Category != null)
        {
            task.Category = dto.Category;
        }
        if (dto.Attachments != null)
        {
            task.Attachments = dto.Attachments;
        }
        if (dto.CustomFields != null)
        {
            task.CustomFields = dto.CustomFields;
        }
    }

    /// <summary>
    /// Get My Queue - tasks where action is pending for the logged-in user's group.
    /// For workflow admin users (CanActivateWorkflows), return all tasks with all statuses.
    /// </summary>
    /// <returns>A list of tasks assigned to the current user or their groups with queue statistics.</returns>
    /// <response code="200">Returns the user's task queue with statistics.</response>
    /// <response code="400">Error retrieving queue.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("my-queue")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetMyQueue()
    {
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            // Get the user's groups
            var userGroupIds = await _context.UserGroupMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.UserGroupId)
                .ToListAsync();

            // Check if user is a workflow admin (any of their groups has CanActivateWorkflows)
            var isWorkflowAdmin = await _context.UserGroups
                .Where(g => userGroupIds.Contains(g.Id) && g.CanActivateWorkflows)
                .AnyAsync();

            IQueryable<CrmTask> query = _context.CrmTasks
                .Include(t => t.Account)
                .Include(t => t.Opportunity)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedToGroup)
                .AsQueryable();

            if (isWorkflowAdmin)
            {
                // Workflow admin sees all tasks
                query = query.OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate);
            }
            else
            {
                // Regular users see only tasks assigned to their groups with pending status
                query = query.Where(t =>
                    (t.AssignedToGroupId.HasValue && userGroupIds.Contains(t.AssignedToGroupId.Value)) ||
                    (t.AssignedToUserId.HasValue && t.AssignedToUserId == userId))
                    .Where(t => t.Status != CrmTaskStatus.Completed && t.Status != CrmTaskStatus.Cancelled)
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate);
            }

            var tasks = await query.ToListAsync();

            // Get group names for the tasks
            var groupIds = tasks.Where(t => t.AssignedToGroupId.HasValue).Select(t => t.AssignedToGroupId!.Value).Distinct().ToList();
            var groupNames = await _context.UserGroups
                .Where(g => groupIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g.Name);

            // Map to response with additional info
            var result = tasks.Select(t => new
            {
                t.Id,
                t.Subject,
                t.Description,
                TaskType = t.TaskType.ToString(),
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                t.DueDate,
                t.StartDate,
                t.CompletedDate,
                t.PercentComplete,
                t.EstimatedMinutes,
                t.ActualMinutes,
                t.AccountId,
                AccountName = t.Account?.Company,
                t.OpportunityId,
                OpportunityName = t.Opportunity?.Name,
                t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser != null ? $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}" : null,
                t.AssignedToGroupId,
                AssignedToGroupName = t.AssignedToGroupId.HasValue && groupNames.ContainsKey(t.AssignedToGroupId.Value)
                    ? groupNames[t.AssignedToGroupId.Value] : null,
                t.Tags,
                t.Category,
                t.CreatedAt,
                IsOverdue = t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != CrmTaskStatus.Completed
            }).ToList();

            var overdueCount = result.Count(r => r.IsOverdue);

            return Ok(new
            {
                isWorkflowAdmin,
                tasks = result,
                totalCount = result.Count,
                pendingCount = result.Count(r => r.Status != "Completed" && r.Status != "Cancelled"),
                overdueCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving my queue: {ex.Message}");
            return BadRequest(new { message = "Error retrieving queue" });
        }
    }
}
