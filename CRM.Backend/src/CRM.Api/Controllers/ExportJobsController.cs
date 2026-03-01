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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing data export jobs.
/// </summary>
[ApiController]
[Route("api/export-jobs")]
[Authorize]
[Produces("application/json")]
public class ExportJobsController : CrmControllerBase
{
    private readonly IExportJobService _service;

    public ExportJobsController(IExportJobService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExportJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExportJobDto>> Create([FromBody] CreateExportJobDto dto, CancellationToken ct)
    {
                var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExportJobDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExportJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExportJobDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }
}
