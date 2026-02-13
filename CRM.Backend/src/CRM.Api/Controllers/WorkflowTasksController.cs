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

using System.Security.Claims;
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for workflow human task actions aligned to /api/workflows/tasks
/// </summary>
[ApiController]
[Route("api/workflows/tasks")]
[Authorize]
public class WorkflowTasksController : ControllerBase
{
    private readonly IWorkflowInstanceService _instanceService;
    private readonly ILogger<WorkflowTasksController> _logger;

    public WorkflowTasksController(
        IWorkflowInstanceService instanceService,
        ILogger<WorkflowTasksController> logger)
    {
        _instanceService = instanceService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
    }

    private string[] GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }

    /// <summary>
    /// Get human tasks for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] string? status = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var roles = GetCurrentUserRoles();

            var tasks = await _instanceService.GetHumanTasksForUserAsync(userId, roles);

            WorkflowTaskStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkflowTaskStatus>(status, true, out var parsed))
                statusFilter = parsed;

            var result = tasks
                .Where(t => !statusFilter.HasValue || t.Status == statusFilter.Value)
                .Select(t => new WorkflowTaskDto
                {
                    Id = t.Id,
                    NodeId = t.WorkflowNodeId,
                    NodeName = t.WorkflowNode?.Name ?? string.Empty,
                    TaskType = t.TaskType.ToString(),
                    Name = t.Name,
                    Status = t.Status.ToString(),
                    Priority = t.Priority,
                    DueAt = t.DueAt,
                    AssignedToId = t.AssignedToId,
                    AssignedToRole = t.AssignedToRole,
                    RetryCount = t.RetryCount,
                    IsDeadLetter = t.IsDeadLetter,
                    CreatedAt = t.CreatedAt
                })
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow tasks");
            return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Get a specific workflow task by ID
    /// </summary>
    [HttpGet("{taskId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(int taskId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var roles = GetCurrentUserRoles();

            var tasks = await _instanceService.GetHumanTasksForUserAsync(userId, roles);
            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            var result = new WorkflowTaskDto
            {
                Id = task.Id,
                NodeId = task.WorkflowNodeId,
                NodeName = task.WorkflowNode?.Name ?? string.Empty,
                TaskType = task.TaskType.ToString(),
                Name = task.Name,
                Status = task.Status.ToString(),
                Priority = task.Priority,
                DueAt = task.DueAt,
                AssignedToId = task.AssignedToId,
                AssignedToRole = task.AssignedToRole,
                RetryCount = task.RetryCount,
                IsDeadLetter = task.IsDeadLetter,
                CreatedAt = task.CreatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow task {TaskId}", taskId);
            return StatusCode(500, new { message = "An error occurred while retrieving the task" });
        }
    }

    /// <summary>
    /// Complete a human task
    /// </summary>
    [HttpPost("{taskId:int}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _instanceService.CompleteHumanTaskAsync(taskId, userId, dto.FormData, dto.OutputData);
            if (!success)
                return BadRequest(new { message = "Cannot complete this task. It may not exist or is not assigned to you." });

            return Ok(new { message = "Task completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing workflow task {TaskId}", taskId);
            return StatusCode(500, new { message = "An error occurred while completing the task" });
        }
    }

    /// <summary>
    /// Reassign a human task to another user
    /// </summary>
    [HttpPost("{taskId:int}/reassign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReassignTask(int taskId, [FromQuery] int assignToUserId)
    {
        try
        {
            var success = await _instanceService.ClaimTaskAsync(taskId, assignToUserId);
            if (!success)
                return BadRequest(new { message = "Cannot reassign this task" });

            return Ok(new { message = "Task reassigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reassigning workflow task {TaskId}", taskId);
            return StatusCode(500, new { message = "An error occurred while reassigning the task" });
        }
    }
}
