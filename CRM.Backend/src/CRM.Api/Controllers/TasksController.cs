using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing CRM tasks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
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
    /// Get all tasks with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CrmTask>>> GetTasks(
        [FromQuery] int? customerId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? assignedToUserId = null,
        [FromQuery] CrmTaskStatus? status = null,
        [FromQuery] CrmTaskPriority? priority = null,
        [FromQuery] bool? overdue = null)
    {
        var query = _context.CrmTasks
            .Include(t => t.Customer)
            .Include(t => t.Opportunity)
            .Include(t => t.AssignedToUser)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(t => t.CustomerId == customerId);
        
        if (opportunityId.HasValue)
            query = query.Where(t => t.OpportunityId == opportunityId);
        
        if (assignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
        
        if (status.HasValue)
            query = query.Where(t => t.Status == status);
        
        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority);
        
        if (overdue == true)
            query = query.Where(t => t.DueDate < DateTime.UtcNow && t.Status != CrmTaskStatus.Completed);

        var tasks = await query.OrderByDescending(t => t.DueDate).ToListAsync();
        foreach (var t in tasks)
        {
            var nt = await _normalization.GetTagsAsync("CrmTask", t.Id);
            if (!string.IsNullOrWhiteSpace(nt)) t.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("CrmTask", t.Id);
            if (!string.IsNullOrWhiteSpace(cf)) t.CustomFields = cf;
        }
        return Ok(tasks);
    }

    /// <summary>
    /// Get a task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CrmTask>> GetTask(int id)
    {
        var task = await _context.CrmTasks
            .Include(t => t.Customer)
            .Include(t => t.Opportunity)
            .Include(t => t.AssignedToUser)
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("CrmTask", task.Id);
        if (!string.IsNullOrWhiteSpace(nt)) task.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("CrmTask", task.Id);
        if (!string.IsNullOrWhiteSpace(cf)) task.CustomFields = cf;

        return Ok(task);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CrmTask>> CreateTask(CrmTask task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.CrmTasks.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} created: {Subject}", task.Id, task.Subject);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, CrmTask task)
    {
        if (id != task.Id)
            return BadRequest();

        var existingTask = await _context.CrmTasks.FindAsync(id);
        if (existingTask == null)
            return NotFound();

        // Track completion
        if (task.Status == CrmTaskStatus.Completed && existingTask.Status != CrmTaskStatus.Completed)
        {
            task.CompletedDate = DateTime.UtcNow;
            task.PercentComplete = 100;
        }

        _context.Entry(existingTask).CurrentValues.SetValues(task);
        existingTask.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.CrmTasks.FindAsync(id);
        if (task == null)
            return NotFound();

        _context.CrmTasks.Remove(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} deleted", id);
        return NoContent();
    }

    /// <summary>
    /// Mark a task as complete
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTask(int id)
    {
        var task = await _context.CrmTasks.FindAsync(id);
        if (task == null)
            return NotFound();

        task.Status = CrmTaskStatus.Completed;
        task.CompletedDate = DateTime.UtcNow;
        task.PercentComplete = 100;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(task);
    }

    /// <summary>
    /// Get tasks due today
    /// </summary>
    [HttpGet("due-today")]
    public async Task<ActionResult<IEnumerable<CrmTask>>> GetTasksDueToday()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var tasks = await _context.CrmTasks
            .Include(t => t.Customer)
            .Include(t => t.AssignedToUser)
            .Where(t => t.DueDate >= today && t.DueDate < tomorrow && t.Status != CrmTaskStatus.Completed)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<CrmTask>>> GetOverdueTasks()
    {
        var tasks = await _context.CrmTasks
            .Include(t => t.Customer)
            .Include(t => t.AssignedToUser)
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != CrmTaskStatus.Completed && t.Status != CrmTaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// Get My Queue - tasks where action is pending for the logged-in user's group
    /// For workflow admin users (CanActivateWorkflows), return all tasks with all statuses
    /// </summary>
    [HttpGet("my-queue")]
    public async Task<ActionResult<object>> GetMyQueue()
    {
        try
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

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
                .Include(t => t.Customer)
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
                t.CustomerId,
                CustomerName = t.Customer?.Company,
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
