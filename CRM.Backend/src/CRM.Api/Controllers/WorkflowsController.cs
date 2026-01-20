using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing workflows and rules
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ICrmDbContext _context;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        IWorkflowService workflowService,
        ICrmDbContext context,
        ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all workflows for a specific entity type
    /// </summary>
    [HttpGet("{entityType}")]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetWorkflows(string entityType)
    {
        try
        {
            var workflows = await _workflowService.GetWorkflowsAsync(entityType);
            var dtos = workflows.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving workflows: {ex.Message}");
            return BadRequest(new { message = "Error retrieving workflows" });
        }
    }

    /// <summary>
    /// Get a specific workflow by ID
    /// </summary>
    [HttpGet("detail/{id}")]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(int id)
    {
        try
        {
            var workflow = await _workflowService.GetWorkflowAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            return Ok(MapToDto(workflow));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving workflow: {ex.Message}");
            return BadRequest(new { message = "Error retrieving workflow" });
        }
    }

    /// <summary>
    /// Create a new workflow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowDto>> CreateWorkflow([FromBody] WorkflowDto dto)
    {
        try
        {
            var workflow = new Workflow
            {
                Name = dto.Name,
                Description = dto.Description,
                EntityType = dto.EntityType,
                IsActive = dto.IsActive,
                Priority = dto.Priority
            };

            var created = await _workflowService.CreateWorkflowAsync(workflow);
            if (created == null)
            {
                return BadRequest(new { message = "Failed to create workflow" });
            }
            return CreatedAtAction(nameof(GetWorkflow), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating workflow: {ex.Message}");
            return BadRequest(new { message = "Error creating workflow" });
        }
    }

    /// <summary>
    /// Update a workflow
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkflowDto>> UpdateWorkflow(int id, [FromBody] WorkflowDto dto)
    {
        try
        {
            var workflow = new Workflow
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                Priority = dto.Priority
            };

            var updated = await _workflowService.UpdateWorkflowAsync(id, workflow);
            if (updated == null)
                return NotFound(new { message = "Workflow not found" });

            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating workflow: {ex.Message}");
            return BadRequest(new { message = "Error updating workflow" });
        }
    }

    /// <summary>
    /// Delete a workflow
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkflow(int id)
    {
        try
        {
            var deleted = await _workflowService.DeleteWorkflowAsync(id);
            if (!deleted)
                return NotFound(new { message = "Workflow not found" });

            return Ok(new { message = "Workflow deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting workflow: {ex.Message}");
            return BadRequest(new { message = "Error deleting workflow" });
        }
    }

    /// <summary>
    /// Create a workflow rule
    /// </summary>
    [HttpPost("{workflowId}/rules")]
    public async Task<ActionResult<WorkflowRuleDto>> CreateRule(int workflowId, [FromBody] WorkflowRuleDto dto)
    {
        try
        {
            var workflow = await _workflowService.GetWorkflowAsync(workflowId);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            var rule = new WorkflowRule
            {
                WorkflowId = workflowId,
                Name = dto.Name,
                Description = dto.Description,
                TargetUserGroupId = dto.TargetUserGroupId,
                IsEnabled = dto.IsEnabled,
                Priority = dto.Priority,
                ConditionLogic = dto.ConditionLogic
            };

            _context.WorkflowRules.Add(rule);
            await _context.SaveChangesAsync();

            // Add conditions
            foreach (var conditionDto in dto.Conditions)
            {
                var condition = new WorkflowRuleCondition
                {
                    WorkflowRuleId = rule.Id,
                    FieldName = conditionDto.FieldName,
                    Operator = conditionDto.Operator,
                    Value = conditionDto.Value,
                    ValueTwo = conditionDto.ValueTwo,
                    Priority = conditionDto.Priority
                };
                _context.WorkflowRuleConditions.Add(condition);
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, MapRuleToDto(rule));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating rule: {ex.Message}");
            return BadRequest(new { message = "Error creating rule" });
        }
    }

    /// <summary>
    /// Get a specific workflow rule
    /// </summary>
    [HttpGet("rules/{id}")]
    public async Task<ActionResult<WorkflowRuleDto>> GetRule(int id)
    {
        try
        {
            var rule = await _context.WorkflowRules
                .Include(r => r.Conditions)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (rule == null)
                return NotFound(new { message = "Rule not found" });

            return Ok(MapRuleToDto(rule));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving rule: {ex.Message}");
            return BadRequest(new { message = "Error retrieving rule" });
        }
    }

    /// <summary>
    /// Update a workflow rule
    /// </summary>
    [HttpPut("rules/{id}")]
    public async Task<ActionResult<WorkflowRuleDto>> UpdateRule(int id, [FromBody] WorkflowRuleDto dto)
    {
        try
        {
            var rule = await _context.WorkflowRules
                .Include(r => r.Conditions)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (rule == null)
                return NotFound(new { message = "Rule not found" });

            rule.Name = dto.Name;
            rule.Description = dto.Description;
            rule.TargetUserGroupId = dto.TargetUserGroupId;
            rule.IsEnabled = dto.IsEnabled;
            rule.Priority = dto.Priority;
            rule.ConditionLogic = dto.ConditionLogic;

            _context.WorkflowRules.Update(rule);
            await _context.SaveChangesAsync();

            return Ok(MapRuleToDto(rule));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating rule: {ex.Message}");
            return BadRequest(new { message = "Error updating rule" });
        }
    }

    /// <summary>
    /// Delete a workflow rule
    /// </summary>
    [HttpDelete("rules/{id}")]
    public async Task<ActionResult> DeleteRule(int id)
    {
        try
        {
            var rule = await _context.WorkflowRules.FindAsync(new object[] { id });
            if (rule == null)
                return NotFound(new { message = "Rule not found" });

            _context.WorkflowRules.Remove(rule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rule deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting rule: {ex.Message}");
            return BadRequest(new { message = "Error deleting rule" });
        }
    }

    /// <summary>
    /// Get workflow execution history
    /// </summary>
    [HttpGet("{workflowId}/executions")]
    public async Task<ActionResult<IEnumerable<WorkflowExecutionDto>>> GetExecutionHistory(int workflowId)
    {
        try
        {
            var executions = await _workflowService.GetExecutionHistoryAsync(workflowId);
            var dtos = executions.Select(MapExecutionToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving execution history: {ex.Message}");
            return BadRequest(new { message = "Error retrieving execution history" });
        }
    }

    /// <summary>
    /// Execute a workflow manually for an entity
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecuteWorkflow([FromBody] ExecuteWorkflowRequestDto request)
    {
        try
        {
            var success = await _workflowService.ExecuteWorkflowAsync(
                request.EntityType,
                request.EntityId,
                request.EntityData);

            return Ok(new { success, message = success ? "Workflow executed successfully" : "No matching rules found" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing workflow: {ex.Message}");
            return BadRequest(new { message = "Error executing workflow" });
        }
    }

    // Helper methods for mapping entities to DTOs
    private WorkflowDto MapToDto(Workflow workflow)
    {
        return new WorkflowDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            EntityType = workflow.EntityType,
            IsActive = workflow.IsActive,
            Priority = workflow.Priority,
            Rules = workflow.Rules?.Select(MapRuleToDto).ToList() ?? new()
        };
    }

    private WorkflowRuleDto MapRuleToDto(WorkflowRule rule)
    {
        return new WorkflowRuleDto
        {
            Id = rule.Id,
            WorkflowId = rule.WorkflowId,
            Name = rule.Name,
            Description = rule.Description,
            TargetUserGroupId = rule.TargetUserGroupId,
            TargetUserGroupName = rule.TargetUserGroup?.Name ?? "",
            IsEnabled = rule.IsEnabled,
            Priority = rule.Priority,
            ConditionLogic = rule.ConditionLogic,
            Conditions = rule.Conditions?.Select(c => new WorkflowRuleConditionDto
            {
                Id = c.Id,
                WorkflowRuleId = c.WorkflowRuleId,
                FieldName = c.FieldName,
                Operator = c.Operator,
                Value = c.Value,
                ValueTwo = c.ValueTwo,
                Priority = c.Priority
            }).ToList() ?? new()
        };
    }

    private WorkflowExecutionDto MapExecutionToDto(WorkflowExecution execution)
    {
        return new WorkflowExecutionDto
        {
            Id = execution.Id,
            WorkflowId = execution.WorkflowId,
            WorkflowRuleId = execution.WorkflowRuleId,
            EntityType = execution.EntityType,
            EntityId = execution.EntityId,
            SourceUserGroupId = execution.SourceUserGroupId,
            TargetUserGroupId = execution.TargetUserGroupId,
            Status = execution.Status,
            ErrorMessage = execution.ErrorMessage,
            EntitySnapshotJson = execution.EntitySnapshotJson,
            CreatedAt = execution.CreatedAt
        };
    }
}
