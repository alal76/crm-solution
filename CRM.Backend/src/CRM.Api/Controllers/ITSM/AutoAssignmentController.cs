// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.ITSM;

/// <summary>
/// API controller for automatic service request assignment.
/// Supports rule management, auto-assignment triggers, and agent suggestions.
/// </summary>
[ApiController]
[Route("api/autoassignment")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Auto Assignment")]
public class AutoAssignmentController : CrmControllerBase
{
    private readonly IAutoAssignmentService _autoAssignmentService;
    private readonly ILogger<AutoAssignmentController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoAssignmentController"/> class.
    /// </summary>
    public AutoAssignmentController(
        IAutoAssignmentService autoAssignmentService,
        ILogger<AutoAssignmentController> logger)
    {
        _autoAssignmentService = autoAssignmentService ?? throw new ArgumentNullException(nameof(autoAssignmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all assignment rules.
    /// </summary>
    /// <returns>A list of all assignment rules</returns>
    /// <response code="200">Returns the list of assignment rules</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("rules")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<AssignmentRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken = default)
    {
                var rules = await _autoAssignmentService.GetRulesAsync(cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Get an assignment rule by ID.
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The assignment rule</returns>
    /// <response code="200">Returns the assignment rule</response>
    /// <response code="404">Rule not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("rules/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(AssignmentRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRuleById(int id, CancellationToken cancellationToken = default)
    {
                var rule = await _autoAssignmentService.GetRuleByIdAsync(id, cancellationToken);
        if (rule == null)
            return NotFound(new { message = $"Assignment rule {id} not found" });
        return Ok(rule);
    }

    /// <summary>
    /// Create a new assignment rule.
    /// </summary>
    /// <param name="dto">The assignment rule data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created assignment rule</returns>
    /// <response code="201">Rule created successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("rules")]
    [Authorize]
    [ProducesResponseType(typeof(AssignmentRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRule([FromBody] CreateAssignmentRuleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Rule name is required" });

            var rule = await _autoAssignmentService.CreateRuleAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an assignment rule.
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <param name="dto">The update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated assignment rule</returns>
    /// <response code="200">Rule updated successfully</response>
    /// <response code="404">Rule not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("rules/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(AssignmentRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRule(int id, [FromBody] UpdateAssignmentRuleDto dto, CancellationToken cancellationToken = default)
    {
                var rule = await _autoAssignmentService.UpdateRuleAsync(id, dto, cancellationToken);
        if (rule == null)
            return NotFound(new { message = $"Assignment rule {id} not found" });
        return Ok(rule);
    }

    /// <summary>
    /// Delete an assignment rule.
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="204">Rule deleted successfully</response>
    /// <response code="404">Rule not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("rules/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken = default)
    {
                var deleted = await _autoAssignmentService.DeleteRuleAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { message = $"Assignment rule {id} not found" });
        return NoContent();
    }

    /// <summary>
    /// Trigger auto-assignment for a service request.
    /// </summary>
    /// <param name="serviceRequestId">The service request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The assignment result</returns>
    /// <response code="200">Assignment completed</response>
    /// <response code="404">Service request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("assign/{serviceRequestId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(AutoAssignmentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignServiceRequest(int serviceRequestId, CancellationToken cancellationToken = default)
    {
                var result = await _autoAssignmentService.AssignServiceRequestAsync(serviceRequestId, cancellationToken);
        if (!result.Success && result.Reason == "Service request not found")
            return NotFound(new { message = result.Reason });
        return Ok(result);
    }

    /// <summary>
    /// Suggest an agent for a service request without actually assigning.
    /// </summary>
    /// <param name="serviceRequestId">The service request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The suggested assignment</returns>
    /// <response code="200">Suggestion returned</response>
    /// <response code="404">Service request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("suggest/{serviceRequestId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(AutoAssignmentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SuggestAssignment(int serviceRequestId, CancellationToken cancellationToken = default)
    {
                var result = await _autoAssignmentService.SuggestAssignmentAsync(serviceRequestId, cancellationToken);
        if (!result.Success && result.Reason == "Service request not found")
            return NotFound(new { message = result.Reason });
        return Ok(result);
    }
}
