// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing workflow triggers (events, schedules, field changes that start workflows)
/// </summary>
[ApiController]
[Route("api/workflow-triggers")]
[Authorize]
public class WorkflowTriggersController : CrmControllerBase
{
    private readonly IWorkflowTriggerService _triggerService;
    private readonly ILogger<WorkflowTriggersController> _logger;

    public WorkflowTriggersController(
        IWorkflowTriggerService triggerService,
        ILogger<WorkflowTriggersController> logger)
    {
        _triggerService = triggerService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all workflow triggers with optional filtering
    /// </summary>
    /// <param name="workflowDefinitionId">Filter by workflow definition</param>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="triggerType">Filter by trigger type</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>List of workflow triggers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkflowTriggerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTriggers(
        [FromQuery] int? workflowDefinitionId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] WorkflowTriggerType? triggerType = null,
        [FromQuery] bool? isActive = null)
    {
                var triggers = await _triggerService.GetAllAsync(workflowDefinitionId, triggerType, entityType, isActive);
        return Ok(triggers);
    }

    /// <summary>
    /// Get a specific workflow trigger by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkflowTriggerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrigger(int id)
    {
                var trigger = await _triggerService.GetByIdAsync(id);
        if (trigger == null)
            return NotFound(new { error = $"Trigger with ID {id} not found" });

        return Ok(trigger);
    }

    /// <summary>
    /// Get all triggers for a specific workflow definition
    /// </summary>
    [HttpGet("workflow/{workflowDefinitionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowTriggerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTriggersForWorkflow(int workflowDefinitionId)
    {
                var triggers = await _triggerService.GetByWorkflowAsync(workflowDefinitionId);
        return Ok(triggers);
    }

    /// <summary>
    /// Create a new workflow trigger
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkflowTriggerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTrigger([FromBody] CreateWorkflowTriggerDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate cron expression if scheduled trigger
            if (dto.TriggerType == WorkflowTriggerType.Scheduled && !string.IsNullOrEmpty(dto.CronExpression))
            {
                if (!_triggerService.ValidateCronExpression(dto.CronExpression, out var cronError))
                    return BadRequest(new { error = "Invalid cron expression", details = cronError });
            }

            // Validate filter conditions if provided
            if (!string.IsNullOrEmpty(dto.FilterConditions))
            {
                if (!_triggerService.ValidateFilterConditions(dto.FilterConditions, out var filterError))
                    return BadRequest(new { error = "Invalid filter conditions", details = filterError });
            }

            var trigger = await _triggerService.CreateAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetTrigger), new { id = trigger.Id }, trigger);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing workflow trigger
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WorkflowTriggerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTrigger(int id, [FromBody] UpdateWorkflowTriggerDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate cron expression if provided
            if (!string.IsNullOrEmpty(dto.CronExpression))
            {
                if (!_triggerService.ValidateCronExpression(dto.CronExpression, out var cronError))
                    return BadRequest(new { error = "Invalid cron expression", details = cronError });
            }

            // Validate filter conditions if provided
            if (!string.IsNullOrEmpty(dto.FilterConditions))
            {
                if (!_triggerService.ValidateFilterConditions(dto.FilterConditions, out var filterError))
                    return BadRequest(new { error = "Invalid filter conditions", details = filterError });
            }

            dto.Id = id;
            var trigger = await _triggerService.UpdateAsync(dto);

            return Ok(trigger);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a workflow trigger
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTrigger(int id)
    {
                var success = await _triggerService.DeleteAsync(id);
        if (!success)
            return NotFound(new { error = $"Trigger with ID {id} not found" });

        return NoContent();
    }

    #endregion

    #region Activation/Deactivation

    /// <summary>
    /// Activate a workflow trigger
    /// </summary>
    [HttpPost("{id:int}/activate")]
    [ProducesResponseType(typeof(WorkflowTriggerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateTrigger(int id)
    {
        try
        {
            var trigger = await _triggerService.ActivateAsync(id);

            _logger.LogInformation("Activated workflow trigger {TriggerId} by user {UserId}", id, GetCurrentUserId());
            return Ok(trigger);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a workflow trigger
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    [ProducesResponseType(typeof(WorkflowTriggerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateTrigger(int id)
    {
        try
        {
            var trigger = await _triggerService.DeactivateAsync(id);

            _logger.LogInformation("Deactivated workflow trigger {TriggerId} by user {UserId}", id, GetCurrentUserId());
            return Ok(trigger);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    #endregion

    #region Trigger Execution

    /// <summary>
    /// Manually fire a trigger to start a workflow instance
    /// </summary>
    [HttpPost("{id:int}/fire")]
    [ProducesResponseType(typeof(TriggerExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FireTrigger(int id, [FromBody] FireTriggerRequestDto request)
    {
        try
        {
            var initiatedById = request.InitiatedById ?? GetCurrentUserId();

            var result = await _triggerService.FireTriggerAsync(id, request.EntityId, initiatedById);

            if (!result.Success)
                return BadRequest(new { error = "Trigger execution failed", details = result.Errors });

            _logger.LogInformation(
                "Manually fired workflow trigger {TriggerId} for entity {EntityId}, triggered {WorkflowCount} workflows",
                id, request.EntityId, result.WorkflowsTriggered);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Evaluate all triggers for an entity type and event
    /// Used internally when entity changes occur
    /// </summary>
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(TriggerExecutionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> EvaluateTriggers([FromBody] TriggerExecutionRequest request)
    {
                var results = await _triggerService.EvaluateTriggersAsync(request);

        return Ok(results);
    }

    /// <summary>
    /// Record a trigger execution
    /// </summary>
    [HttpPost("{id:int}/record-execution")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordTriggerExecution(int id)
    {
        try
        {
            await _triggerService.RecordTriggerExecutionAsync(id);
            return Ok(new { message = $"Execution recorded for trigger {id}" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    #endregion

    #region Matching Triggers

    /// <summary>
    /// Get triggers matching a specific entity type and trigger type.
    /// Useful for discovering which triggers would fire for a given entity event.
    /// </summary>
    [HttpGet("matching")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowTriggerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatchingTriggers(
        [FromQuery] string entityType,
        [FromQuery] WorkflowTriggerType triggerType,
        [FromQuery] string? eventName = null)
    {
                if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest(new { error = "entityType is required" });

        var triggers = await _triggerService.GetMatchingTriggersAsync(entityType, triggerType, eventName);
        return Ok(triggers);
    }

    #endregion

    #region Scheduled Triggers

    /// <summary>
    /// Get all scheduled triggers that are due to run
    /// </summary>
    [HttpGet("scheduled/due")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowTriggerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueScheduledTriggers([FromQuery] DateTime? asOf = null)
    {
                var triggers = await _triggerService.GetScheduledTriggersDueAsync(asOf ?? DateTime.UtcNow);
        return Ok(triggers);
    }

    /// <summary>
    /// Update the next scheduled time for a trigger (used after execution)
    /// </summary>
    [HttpPost("{id:int}/update-schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequestDto request)
    {
        try
        {
            await _triggerService.UpdateNextScheduledTimeAsync(id, request.NextScheduledTime);

            return Ok(new { message = "Schedule updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get execution statistics for a workflow trigger
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(TriggerStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
                var stats = await _triggerService.GetStatisticsAsync();

        return Ok(stats);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate a cron expression
    /// </summary>
    [HttpPost("validate/cron")]
    [ProducesResponseType(typeof(CronValidationResultDto), StatusCodes.Status200OK)]
    public IActionResult ValidateCronExpression([FromBody] CronValidationRequestDto request)
    {
        var isValid = _triggerService.ValidateCronExpression(request.CronExpression, out var errorMessage);
        return Ok(new CronValidationResultDto
        {
            IsValid = isValid,
            ErrorMessage = errorMessage,
            CronExpression = request.CronExpression
        });
    }

    /// <summary>
    /// Validate filter conditions JSON
    /// </summary>
    [HttpPost("validate/filter")]
    [ProducesResponseType(typeof(FilterValidationResultDto), StatusCodes.Status200OK)]
    public IActionResult ValidateFilterConditions([FromBody] FilterValidationRequestDto request)
    {
        var isValid = _triggerService.ValidateFilterConditions(request.FilterConditions, out var errorMessage);
        return Ok(new FilterValidationResultDto
        {
            IsValid = isValid,
            ErrorMessage = errorMessage
        });
    }

    #endregion
}

// Request/Response DTOs are now in CRM.Core/DTOs/Workflow/WorkflowTriggerDtos.cs
