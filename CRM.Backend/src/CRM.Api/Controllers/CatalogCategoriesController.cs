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

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing ITSM catalog categories.
/// </summary>
[ApiController]
[Route("api/catalog-categories")]
[Authorize]
[Produces("application/json")]
public class CatalogCategoriesController : ControllerBase
{
    private readonly ICatalogCategoryService _service;
    private readonly ILogger<CatalogCategoriesController> _logger;

    public CatalogCategoriesController(ICatalogCategoryService service, ILogger<CatalogCategoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CatalogCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CatalogCategoryDto>> Create([FromBody] CreateCatalogCategoryDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.CategoryId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating catalog category");
            return StatusCode(500, new { error = "Failed to create catalog category" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CatalogCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CatalogCategoryDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CatalogCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CatalogCategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }
}
