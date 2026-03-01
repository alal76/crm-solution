// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.ITSM;

/// <summary>
/// API controller for managing escalation policies with multiple levels.
/// Provides CRUD operations and escalation management.
/// </summary>
[ApiController]
[Route("api/itsm/escalation-policies")]
[Authorize]
[Produces("application/json")]
[Tags("ITSM - Escalation Policies")]
public class EscalationPoliciesController : CrmControllerBase
{
    private readonly IEscalationPolicyService _escalationPolicyService;
    private readonly ILogger<EscalationPoliciesController> _logger;

    public EscalationPoliciesController(
        IEscalationPolicyService escalationPolicyService,
        ILogger<EscalationPoliciesController> logger)
    {
        _escalationPolicyService = escalationPolicyService;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all escalation policies.
    /// </summary>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <returns>List of escalation policies</returns>
    /// <response code="200">Returns the list of policies</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EscalationPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<EscalationPolicyDto>>> GetPolicies([FromQuery] bool? isActive = null)
    {
                var policies = await _escalationPolicyService.GetPoliciesAsync(isActive);
        return Ok(policies);
    }

    /// <summary>
    /// Get a specific escalation policy by ID.
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <returns>Escalation policy details with levels</returns>
    /// <response code="200">Returns the policy</response>
    /// <response code="404">If policy not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EscalationPolicyDto>> GetPolicyById(int id)
    {
                var policy = await _escalationPolicyService.GetPolicyByIdAsync(id);

        if (policy == null)
        {
            return NotFound($"Escalation policy with ID {id} not found");
        }

        return Ok(policy);
    }

    /// <summary>
    /// Create a new escalation policy.
    /// </summary>
    /// <param name="dto">Policy creation data</param>
    /// <returns>Created escalation policy</returns>
    /// <response code="201">Policy created successfully</response>
    /// <response code="400">If validation fails</response>
    /// <response code="401">If user is not authorized</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EscalationPolicyDto>> CreatePolicy([FromBody] CreateEscalationPolicyDto dto)
    {
                if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var policy = await _escalationPolicyService.CreatePolicyAsync(dto, userId ?? 1);

        return CreatedAtAction(nameof(GetPolicyById), new { id = policy.Id }, policy);
    }

    /// <summary>
    /// Update an existing escalation policy.
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <param name="dto">Update data</param>
    /// <returns>Updated escalation policy</returns>
    /// <response code="200">Policy updated successfully</response>
    /// <response code="400">If validation fails</response>
    /// <response code="401">If user is not authorized</response>
    /// <response code="404">If policy not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EscalationPolicyDto>> UpdatePolicy(
        int id,
        [FromBody] UpdateEscalationPolicyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var policy = await _escalationPolicyService.UpdatePolicyAsync(id, dto, userId ?? 1);

            return Ok(policy);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete an escalation policy (soft delete).
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <response code="204">Policy deleted successfully</response>
    /// <response code="401">If user is not authorized</response>
    /// <response code="404">If policy not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePolicy(int id)
    {
                var result = await _escalationPolicyService.DeletePolicyAsync(id);

        if (!result)
        {
            return NotFound($"Escalation policy with ID {id} not found");
        }

        return NoContent();
    }

    #endregion

    #region Level Management

    /// <summary>
    /// Get all levels for a policy.
    /// </summary>
    /// <param name="policyId">Policy ID</param>
    /// <returns>List of escalation levels</returns>
    [HttpGet("{policyId}/levels")]
    [ProducesResponseType(typeof(List<EscalationLevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<EscalationLevelDto>>> GetPolicyLevels(int policyId)
    {
                var levels = await _escalationPolicyService.GetPolicyLevelsAsync(policyId);
        return Ok(levels);
    }

    /// <summary>
    /// Add a level to a policy.
    /// </summary>
    /// <param name="policyId">Policy ID</param>
    /// <param name="dto">Level creation data</param>
    /// <returns>Created escalation level</returns>
    [HttpPost("{policyId}/levels")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationLevelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EscalationLevelDto>> AddLevel(
        int policyId,
        [FromBody] CreateEscalationLevelDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var level = await _escalationPolicyService.AddLevelAsync(policyId, dto, userId ?? 1);
            return CreatedAtAction(nameof(GetPolicyLevels), new { policyId }, level);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Update an escalation level.
    /// </summary>
    /// <param name="levelId">Level ID</param>
    /// <param name="dto">Update data</param>
    /// <returns>Updated escalation level</returns>
    [HttpPut("levels/{levelId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationLevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EscalationLevelDto>> UpdateLevel(
        int levelId,
        [FromBody] CreateEscalationLevelDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var level = await _escalationPolicyService.UpdateLevelAsync(levelId, dto, userId ?? 1);
            return Ok(level);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Remove a level from a policy.
    /// </summary>
    /// <param name="levelId">Level ID</param>
    /// <response code="204">Level removed successfully</response>
    [HttpDelete("levels/{levelId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveLevel(int levelId)
    {
                var result = await _escalationPolicyService.DeleteLevelAsync(levelId);

        if (!result)
        {
            return NotFound($"Escalation level with ID {levelId} not found");
        }

        return NoContent();
    }

    #endregion
}
