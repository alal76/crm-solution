// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing account relationships
/// </summary>
[ApiController]
[Route("api/relationships")]
[Authorize]
public class RelationshipsController : CrmControllerBase
{
    private const string RelationshipTypeNotFoundMessage = "Relationship type not found";
    private const string RelationshipNotFoundMessage = "Relationship not found";

    private readonly RelationshipService _relationshipService;

    public RelationshipsController(
        RelationshipService relationshipService)
    {
        _relationshipService = relationshipService;
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region Relationship Types

    /// <summary>
    /// Get all relationship types
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelationshipTypes([FromQuery] bool includeInactive = false)
    {
                var types = await _relationshipService.GetRelationshipTypesAsync(includeInactive);
        return Ok(types);
    }

    /// <summary>
    /// Get a relationship type by ID
    /// </summary>
    [HttpGet("types/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelationshipType(int id)
    {
                var type = await _relationshipService.GetRelationshipTypeAsync(id);
        if (type == null)
        {
            return NotFound(new { message = RelationshipTypeNotFoundMessage });
        }

        return Ok(type);
    }

    /// <summary>
    /// Create a new relationship type
    /// </summary>
    [HttpPost("types")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRelationshipType([FromBody] RelationshipTypeCreateDto dto)
    {
                var userId = GetCurrentUserId();
        var type = await _relationshipService.CreateRelationshipTypeAsync(dto, userId);
        return CreatedAtAction(nameof(GetRelationshipType), new { id = type.Id }, type);
    }

    /// <summary>
    /// Update a relationship type
    /// </summary>
    [HttpPut("types/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRelationshipType(int id, [FromBody] RelationshipTypeCreateDto dto)
    {
        try
        {
            var type = await _relationshipService.UpdateRelationshipTypeAsync(id, dto);
            if (type == null)
            {
                return NotFound(new { message = RelationshipTypeNotFoundMessage });
            }

            return Ok(type);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a relationship type
    /// </summary>
    [HttpDelete("types/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRelationshipType(int id)
    {
        try
        {
            var result = await _relationshipService.DeleteRelationshipTypeAsync(id);
            if (!result)
            {
                return NotFound(new { message = RelationshipTypeNotFoundMessage });
            }

            return Ok(new { message = "Relationship type deleted" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Account Relationships

    /// <summary>
    /// Get all relationships with filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelationships(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] int? relationshipTypeId = null,
        [FromQuery] string? strategicImportance = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
                var (items, totalCount) = await _relationshipService.GetRelationshipsAsync(
            search, status, relationshipTypeId, strategicImportance, skip, take);

        return Ok(new
        {
            items,
            totalCount,
            skip,
            take,
            hasMore = skip + take < totalCount
        });
    }

    /// <summary>
    /// Get relationships for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountRelationships(
        int accountId,
        [FromQuery] string? status = null,
        [FromQuery] int? relationshipTypeId = null)
    {
                var relationships = await _relationshipService.GetAccountRelationshipsAsync(
            accountId, status, relationshipTypeId);
        return Ok(relationships);
    }

    /// <summary>
    /// Get a relationship by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelationship(int id)
    {
                var relationship = await _relationshipService.GetRelationshipAsync(id);
        if (relationship == null)
        {
            return NotFound(new { message = RelationshipNotFoundMessage });
        }

        return Ok(relationship);
    }

    /// <summary>
    /// Create a new relationship
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRelationship([FromBody] AccountRelationshipCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.CreateRelationshipAsync(dto, userId);
            return CreatedAtAction(nameof(GetRelationship), new { id = relationship.Id }, relationship);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a relationship
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRelationship(int id, [FromBody] AccountRelationshipCreateDto dto)
    {
                var userId = GetCurrentUserId();
        var relationship = await _relationshipService.UpdateRelationshipAsync(id, dto, userId);
        if (relationship == null)
        {
            return NotFound(new { message = RelationshipNotFoundMessage });
        }

        return Ok(relationship);
    }

    /// <summary>
    /// Delete a relationship
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRelationship(int id)
    {
                var result = await _relationshipService.DeleteRelationshipAsync(id);
        if (!result)
        {
            return NotFound(new { message = RelationshipNotFoundMessage });
        }

        return Ok(new { message = "Relationship deleted" });
    }

    #endregion

    #region Relationship Interactions

    /// <summary>
    /// Get interactions for a relationship
    /// </summary>
    [HttpGet("{relationshipId}/interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelationshipInteractions(int relationshipId)
    {
                var interactions = await _relationshipService.GetRelationshipInteractionsAsync(relationshipId);
        return Ok(interactions);
    }

    /// <summary>
    /// Create an interaction for a relationship
    /// </summary>
    [HttpPost("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateInteraction([FromBody] RelationshipInteractionCreateDto dto)
    {
                var userId = GetCurrentUserId();
        var interaction = await _relationshipService.CreateInteractionAsync(dto, userId);
        return Ok(interaction);
    }

    #endregion

    #region Relationship Map

    /// <summary>
    /// Get relationship map visualization data for an account
    /// </summary>
    [HttpGet("map/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelationshipMap(
        int accountId,
        [FromQuery] int depth = 2,
        [FromQuery] string? includeTypeIds = null,
        [FromQuery] int minStrength = 0)
    {
                List<int>? typeIds = null;
        if (!string.IsNullOrEmpty(includeTypeIds))
        {
            typeIds = includeTypeIds.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }

        var mapData = await _relationshipService.GetRelationshipMapDataAsync(
            accountId, depth, typeIds, minStrength);
        return Ok(mapData);
    }

    #endregion

    #region Account Health

    /// <summary>
    /// Get health snapshots for an account
    /// </summary>
    [HttpGet("health/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthSnapshots(
        int accountId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
                var snapshots = await _relationshipService.GetHealthSnapshotsAsync(accountId, startDate, endDate);
        return Ok(snapshots);
    }

    /// <summary>
    /// Create a health snapshot for an account
    /// </summary>
    [HttpPost("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateHealthSnapshot([FromBody] AccountHealthSnapshotCreateDto dto)
    {
                var userId = GetCurrentUserId();
        var snapshot = await _relationshipService.CreateHealthSnapshotAsync(dto, userId);
        return Ok(snapshot);
    }

    #endregion
}
