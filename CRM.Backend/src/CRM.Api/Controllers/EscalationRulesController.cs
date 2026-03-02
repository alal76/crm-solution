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

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing escalation rules via the admin service.
/// Provides CRUD operations for escalation rule configuration.
/// </summary>
[ApiController]
[Route("api/escalation-rules")]
[Authorize]
[Produces("application/json")]
public class EscalationRulesController : CrmControllerBase
{
    private readonly IEscalationRuleService _service;
    private readonly ILogger<EscalationRulesController> _logger;

    public EscalationRulesController(IEscalationRuleService service, ILogger<EscalationRulesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Create a new escalation rule.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EscalationRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EscalationRuleDto>> Create([FromBody] CreateEscalationRuleDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex) // NOSONAR - controller top-level handler returns 500 on unexpected errors
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get an escalation rule by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EscalationRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationRuleDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Get all escalation rules.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EscalationRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EscalationRuleDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Delete an escalation rule.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting escalation rule {Id}", id);
            return NotFound();
        }
    }
}
