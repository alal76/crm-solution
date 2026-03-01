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
/// API controller for managing escalation rules.
/// Provides CRUD operations plus rule testing and applicable-rule queries.
/// </summary>
[ApiController]
[Route("api/escalationrules")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Escalation Rules")]
public class EscalationRulesController : CrmControllerBase
{
    private readonly IEscalationRuleService _escalationRuleService; // TODO-SD005-003: renamed from IEscalationRuleAdminService
    private readonly ILogger<EscalationRulesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EscalationRulesController"/> class.
    /// </summary>
    public EscalationRulesController(IEscalationRuleService escalationRuleService, ILogger<EscalationRulesController> logger)
    {
        _escalationRuleService = escalationRuleService ?? throw new ArgumentNullException(nameof(escalationRuleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all escalation rules.
    /// </summary>
    /// <returns>A list of all escalation rules</returns>
    /// <response code="200">Returns the list of escalation rules</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<EscalationRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
                var rules = await _escalationRuleService.GetAllAsync(cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Get escalation rules applicable to a given priority.
    /// </summary>
    /// <param name="priority">The priority to filter by (e.g. Critical, High)</param>
    /// <returns>Matching escalation rules</returns>
    /// <response code="200">Returns applicable escalation rules</response>
    /// <response code="400">Priority parameter is required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("applicable")]
    [Authorize]
    [ProducesResponseType(typeof(List<EscalationRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetApplicable([FromQuery] string priority, CancellationToken cancellationToken = default)
    {
                if (string.IsNullOrWhiteSpace(priority))
            return BadRequest(new { message = "Priority parameter is required" });

        var rules = await _escalationRuleService.GetApplicableRulesAsync(priority, cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Get an escalation rule by ID.
    /// </summary>
    /// <param name="id">The escalation rule ID</param>
    /// <returns>The escalation rule</returns>
    /// <response code="200">Returns the escalation rule</response>
    /// <response code="404">Rule not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(EscalationRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
                var rule = await _escalationRuleService.GetByIdAsync(id, cancellationToken);
        if (rule == null)
            return NotFound(new { message = $"Escalation rule with ID {id} not found" });
        return Ok(rule);
    }

    /// <summary>
    /// Create a new escalation rule.
    /// </summary>
    /// <param name="dto">The escalation rule creation data</param>
    /// <returns>The created escalation rule</returns>
    /// <response code="201">Rule created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateEscalationRuleDto dto, CancellationToken cancellationToken = default)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var rule = await _escalationRuleService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Update an existing escalation rule.
    /// </summary>
    /// <param name="id">The escalation rule ID</param>
    /// <param name="dto">The update data</param>
    /// <returns>The updated escalation rule</returns>
    /// <response code="200">Rule updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Rule not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEscalationRuleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rule = await _escalationRuleService.UpdateAsync(id, dto, cancellationToken);
            return Ok(rule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Escalation rule with ID {id} not found" });
        }
    }

    /// <summary>
    /// Delete an escalation rule.
    /// </summary>
    /// <param name="id">The escalation rule ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Rule deleted successfully</response>
    /// <response code="404">Rule not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _escalationRuleService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Escalation rule with ID {id} not found" });
        }
    }

    /// <summary>
    /// Test an escalation rule against a specific service request to check if it would apply.
    /// </summary>
    /// <param name="ruleId">The escalation rule ID</param>
    /// <param name="serviceRequestId">The service request ID to test against</param>
    /// <returns>Test result showing whether the rule matched and why</returns>
    /// <response code="200">Returns the test result</response>
    /// <response code="404">Rule or service request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{ruleId:int}/test/{serviceRequestId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EscalationRuleTestResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestRule(int ruleId, int serviceRequestId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _escalationRuleService.TestRuleAsync(ruleId, serviceRequestId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
