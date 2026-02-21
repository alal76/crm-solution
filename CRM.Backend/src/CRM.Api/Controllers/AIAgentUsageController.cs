// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing AI agent usage records.
/// </summary>
[ApiController]
[Route("api/ai-agent-usage")]
[Authorize]
[Produces("application/json")]
public class AIAgentUsageController : ControllerBase
{
    private readonly IAIAgentUsageService _service;
    private readonly ILogger<AIAgentUsageController> _logger;

    public AIAgentUsageController(IAIAgentUsageService service, ILogger<AIAgentUsageController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AIAgentUsageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AIAgentUsageDto>> Create([FromBody] CreateAIAgentUsageDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI agent usage record");
            return StatusCode(500, new { error = "Failed to create AI agent usage record" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AIAgentUsageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AIAgentUsageDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AIAgentUsageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AIAgentUsageDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }
}
