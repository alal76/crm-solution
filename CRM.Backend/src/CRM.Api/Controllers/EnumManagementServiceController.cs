// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ENUM-BE-011: Controller for the Configurable Enums feature using IEnumManagementService
using CRM.Core.DTOs;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing configurable enumerations — categories, values, and transitions.
/// Operates via <see cref="IEnumManagementService"/> for all business logic and caching.
/// Route: <c>api/enummanagement</c>
/// </summary>
[ApiController]
[Route("api/enummanagement")]
[Authorize]
[Produces("application/json")]
public class EnumManagementServiceController : CrmControllerBase
{
    private readonly IEnumManagementService _service;
    private readonly ILogger<EnumManagementServiceController> _logger;

    public EnumManagementServiceController(
        IEnumManagementService service,
        ILogger<EnumManagementServiceController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ─── Category endpoints ───────────────────────────────────────────────────

    /// <summary>Returns all enum categories.</summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<EnumCategoryDto>>> GetAllCategories(CancellationToken ct)
    {
        var result = await _service.GetAllCategoriesAsync(ct);
        return Ok(result);
    }

    /// <summary>Returns a single category by name.</summary>
    [HttpGet("categories/{name}")]
    public async Task<ActionResult<EnumCategoryDto>> GetCategoryByName(string name, CancellationToken ct)
    {
        var result = await _service.GetCategoryByNameAsync(name, ct);
        if (result is null)
        {
            return NotFound(new { message = $"Category '{name}' not found." });
        }
        return Ok(result);
    }

    /// <summary>Creates a new enum category (Admin only).</summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EnumCategoryDto>> CreateCategory([FromBody] CreateEnumCategoryDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateCategoryAsync(dto, ct);
            return CreatedAtAction(nameof(GetCategoryByName), new { name = result.Name }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Updates an existing enum category (Admin only).</summary>
    [HttpPut("categories/{categoryId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EnumCategoryDto>> UpdateCategory(int categoryId, [FromBody] UpdateEnumCategoryDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateCategoryAsync(categoryId, dto, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Category {categoryId} not found." });
        }
    }

    // ─── Value endpoints ──────────────────────────────────────────────────────

    /// <summary>Returns all active values for the named category (cached).</summary>
    [HttpGet("categories/{name}/values")]
    public async Task<ActionResult<IEnumerable<EnumValueDto>>> GetValuesByCategory(string name, CancellationToken ct)
    {
        var result = await _service.GetValuesByCategoryNameAsync(name, ct);
        return Ok(result);
    }

    /// <summary>Creates a new value in the given category.</summary>
    [HttpPost("categories/{categoryId:int}/values")]
    public async Task<ActionResult<EnumValueDto>> CreateValue(int categoryId, [FromBody] CreateEnumValueDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateValueAsync(categoryId, dto, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Updates an existing value.</summary>
    [HttpPut("values/{valueId:int}")]
    public async Task<ActionResult<EnumValueDto>> UpdateValue(int valueId, [FromBody] UpdateEnumValueDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateValueAsync(valueId, dto, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Value {valueId} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Soft-deletes a value.</summary>
    [HttpDelete("values/{valueId:int}")]
    public async Task<IActionResult> DeleteValue(int valueId, CancellationToken ct)
    {
        try
        {
            await _service.DeleteValueAsync(valueId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Value {valueId} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Reorders values within a category.</summary>
    [HttpPost("categories/{categoryId:int}/values/reorder")]
    public async Task<IActionResult> ReorderValues(int categoryId, [FromBody] ReorderEnumValuesRequest request, CancellationToken ct)
    {
        await _service.ReorderValuesAsync(categoryId, request.OrderedIds, ct);
        return NoContent();
    }

    // ─── Transition endpoints ─────────────────────────────────────────────────

    /// <summary>Returns all transition rules for the given category.</summary>
    [HttpGet("categories/{categoryId:int}/transitions")]
    public async Task<ActionResult<IEnumerable<EnumTransitionDto>>> GetTransitions(int categoryId, CancellationToken ct)
    {
        var result = await _service.GetTransitionsAsync(categoryId, ct);
        return Ok(result);
    }

    /// <summary>Adds a transition rule to a category.</summary>
    [HttpPost("categories/{categoryId:int}/transitions")]
    public async Task<ActionResult<EnumTransitionDto>> CreateTransition(int categoryId, [FromBody] CreateEnumTransitionDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateTransitionAsync(categoryId, dto, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Deletes a transition rule.</summary>
    [HttpDelete("transitions/{transitionId:int}")]
    public async Task<IActionResult> DeleteTransition(int transitionId, CancellationToken ct)
    {
        try
        {
            await _service.DeleteTransitionAsync(transitionId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Transition {transitionId} not found." });
        }
    }

    // ─── Validation endpoint ──────────────────────────────────────────────────

    /// <summary>Validates whether a value is valid for the given category.</summary>
    [HttpPost("validate")]
    public async Task<ActionResult<EnumValidationResult>> ValidateValue([FromBody] ValidateEnumValueRequest request, CancellationToken ct)
    {
        var result = await _service.ValidateValueAsync(request.CategoryName, request.Value, ct);
        return Ok(result);
    }
}
