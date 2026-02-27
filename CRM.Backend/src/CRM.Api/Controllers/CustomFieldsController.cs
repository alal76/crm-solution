// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Manages structured custom field definitions for any CRM entity type (CUST-01/02).
/// </summary>
[ApiController]
[Route("api/custom-fields")]
[Authorize]
public class CustomFieldsController : ControllerBase
{
    private const string FieldDefinitionNotFoundMessage = "Custom field definition not found";

    private readonly ICrmDbContext _db;
    private readonly ICustomFieldValidationService _validationSvc;
    private readonly ILogger<CustomFieldsController> _logger;

    public CustomFieldsController(
        ICrmDbContext db,
        ICustomFieldValidationService validationSvc,
        ILogger<CustomFieldsController> logger)
    {
        _db = db;
        _validationSvc = validationSvc;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    // ── Field Definitions ──────────────────────────────────────────────────

    /// <summary>Gets all custom field definitions, optionally filtered by entity type.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, CancellationToken ct)
    {
        var query = _db.CustomFieldDefinitions.AsNoTracking()
            .Include(d => d.ValidationRules)
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(d => d.EntityType == entityType);

        var results = await query.OrderBy(d => d.EntityType).ThenBy(d => d.DisplayOrder).ToListAsync(ct);
        return Ok(results);
    }

    /// <summary>Gets a single custom field definition by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var def = await _db.CustomFieldDefinitions.AsNoTracking()
            .Include(d => d.ValidationRules)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);

        return def == null ? NotFound(new { message = FieldDefinitionNotFoundMessage }) : Ok(def);
    }

    /// <summary>Creates a new custom field definition.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CRM.Core.Entities.CustomFieldDefinition dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.EntityType) || string.IsNullOrWhiteSpace(dto.FieldKey))
            return BadRequest(new { message = "EntityType and FieldKey are required." });

        var duplicate = await _db.CustomFieldDefinitions
            .AnyAsync(d => d.EntityType == dto.EntityType && d.FieldKey == dto.FieldKey && !d.IsDeleted, ct);
        if (duplicate)
            return BadRequest(new { message = $"A field with key '{dto.FieldKey}' already exists for entity type '{dto.EntityType}'." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.CustomFieldDefinitions.Add(dto);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Custom field '{FieldKey}' created for entity '{EntityType}' by user {UserId}",
            dto.FieldKey, dto.EntityType, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Updates an existing custom field definition.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CRM.Core.Entities.CustomFieldDefinition dto, CancellationToken ct)
    {
        var existing = await _db.CustomFieldDefinitions.FindAsync(new object[] { id }, ct);
        if (existing == null || existing.IsDeleted)
            return NotFound(new { message = FieldDefinitionNotFoundMessage });

        existing.Label = dto.Label;
        existing.FieldType = dto.FieldType;
        existing.IsRequired = dto.IsRequired;
        existing.IsActive = dto.IsActive;
        existing.DisplayOrder = dto.DisplayOrder;
        existing.DefaultValue = dto.DefaultValue;
        existing.OptionsJson = dto.OptionsJson;
        existing.GroupName = dto.GroupName;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    /// <summary>Soft-deletes a custom field definition.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.CustomFieldDefinitions.FindAsync(new object[] { id }, ct);
        if (existing == null || existing.IsDeleted)
            return NotFound(new { message = FieldDefinitionNotFoundMessage });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Validation Rules ───────────────────────────────────────────────────

    /// <summary>Gets all validation rules for a custom field definition.</summary>
    [HttpGet("{id:int}/validation-rules")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValidationRules(int id, CancellationToken ct)
    {
        var rules = await _db.CustomFieldValidationRules.AsNoTracking()
            .Where(r => r.CustomFieldDefinitionId == id && !r.IsDeleted)
            .ToListAsync(ct);
        return Ok(rules);
    }

    /// <summary>Adds a validation rule to a custom field definition.</summary>
    [HttpPost("{id:int}/validation-rules")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddValidationRule(int id, [FromBody] CRM.Core.Entities.CustomFieldValidationRule rule, CancellationToken ct)
    {
        var defExists = await _db.CustomFieldDefinitions.AnyAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (!defExists)
            return NotFound(new { message = FieldDefinitionNotFoundMessage });

        rule.CustomFieldDefinitionId = id;
        rule.CreatedAt = DateTime.UtcNow;
        rule.UpdatedAt = DateTime.UtcNow;
        _db.CustomFieldValidationRules.Add(rule);
        await _db.SaveChangesAsync(ct);
        return Created($"api/custom-fields/{id}/validation-rules/{rule.Id}", rule);
    }

    /// <summary>Removes a validation rule.</summary>
    [HttpDelete("{id:int}/validation-rules/{ruleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteValidationRule(int id, int ruleId, CancellationToken ct)
    {
        var rule = await _db.CustomFieldValidationRules
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.CustomFieldDefinitionId == id && !r.IsDeleted, ct);
        if (rule == null)
            return NotFound(new { message = "Validation rule not found" });

        rule.IsDeleted = true;
        rule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Value Validation ───────────────────────────────────────────────────

    /// <summary>Validates a field value against rules defined for that field.</summary>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateValue([FromBody] ValidateFieldValueRequest req, CancellationToken ct)
    {
        var result = await _validationSvc.ValidateAsync(req.EntityType, req.FieldKey, req.Value, ct);
        return Ok(result);
    }
}

/// <summary>Request body for field-value validation.</summary>
public record ValidateFieldValueRequest(string EntityType, string FieldKey, string? Value);
