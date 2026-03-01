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
/// API controller for managing ITSM incident categories.
/// </summary>
[ApiController]
[Route("api/incident-categories")]
[Authorize]
[Produces("application/json")]
public class IncidentCategoriesController : CrmControllerBase
{
    private readonly IIncidentCategoryService _service;

    public IncidentCategoriesController(IIncidentCategoryService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncidentCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentCategoryDto>> Create([FromBody] CreateIncidentCategoryDto dto, CancellationToken ct)
    {
                var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentCategoryDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<IncidentCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IncidentCategoryDto>>> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(includeInactive, ct);
        return Ok(result);
    }
}
