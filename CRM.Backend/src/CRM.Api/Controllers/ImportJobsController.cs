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
/// API controller for managing data import jobs.
/// </summary>
[ApiController]
[Route("api/import-jobs")]
[Authorize]
[Produces("application/json")]
public class ImportJobsController : CrmControllerBase
{
    private readonly IImportJobService _service;

    public ImportJobsController(IImportJobService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportJobDto>> Create([FromBody] CreateImportJobDto dto, CancellationToken ct)
    {
                var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportJobDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ImportJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ImportJobDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Get the status of a specific import job.
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImportStatus(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            result.Id,
            result.Status,
            result.TotalRecords,
            result.SuccessCount,
            result.FailureCount,
            result.CreatedAt,
            result.CompletedDate
        });
    }

    /// <summary>
    /// Get available import templates.
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTemplates()
    {
        var templates = new[]
        {
            new { EntityType = "accounts", Name = "Accounts Import Template", Columns = new[] { "Name", "Industry", "Website", "Phone", "Email" } },
            new { EntityType = "contacts", Name = "Contacts Import Template", Columns = new[] { "FirstName", "LastName", "Email", "Phone", "AccountId" } },
            new { EntityType = "leads", Name = "Leads Import Template", Columns = new[] { "FirstName", "LastName", "Email", "Company", "Source" } },
            new { EntityType = "products", Name = "Products Import Template", Columns = new[] { "Name", "SKU", "Price", "Category", "Description" } },
            new { EntityType = "opportunities", Name = "Opportunities Import Template", Columns = new[] { "Name", "Amount", "Stage", "CloseDate", "AccountId" } }
        };

        return Ok(templates);
    }

    /// <summary>
    /// Get import template for a specific entity type.
    /// </summary>
    [HttpGet("templates/{entityType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTemplateByType(string entityType)
    {
        var templates = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["accounts"] = new { EntityType = "accounts", Name = "Accounts Import Template", Columns = new[] { "Name", "Industry", "Website", "Phone", "Email" } },
            ["contacts"] = new { EntityType = "contacts", Name = "Contacts Import Template", Columns = new[] { "FirstName", "LastName", "Email", "Phone", "AccountId" } },
            ["leads"] = new { EntityType = "leads", Name = "Leads Import Template", Columns = new[] { "FirstName", "LastName", "Email", "Company", "Source" } },
            ["products"] = new { EntityType = "products", Name = "Products Import Template", Columns = new[] { "Name", "SKU", "Price", "Category", "Description" } },
            ["opportunities"] = new { EntityType = "opportunities", Name = "Opportunities Import Template", Columns = new[] { "Name", "Amount", "Stage", "CloseDate", "AccountId" } }
        };

        if (!templates.TryGetValue(entityType, out var template))
        {
            return NotFound(new { message = $"No import template found for entity type '{entityType}'" });
        }

        return Ok(template);
    }
}
