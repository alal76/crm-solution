// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Dtos;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing account relationships
/// </summary>
[ApiController]
[Route("api/relationships")]
[Authorize]
public class RelationshipsController : ControllerBase
{
    private readonly RelationshipService _relationshipService;
    private readonly ILogger<RelationshipsController> _logger;

    public RelationshipsController(
        RelationshipService relationshipService,
        ILogger<RelationshipsController> logger)
    {
        _relationshipService = relationshipService;
        _logger = logger;
    }

    private int GetCurrentUserId()
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
        try
        {
            var types = await _relationshipService.GetRelationshipTypesAsync(includeInactive);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationship types");
            return StatusCode(500, new { message = "Error retrieving relationship types" });
        }
    }

    /// <summary>
    /// Get a relationship type by ID
    /// </summary>
    [HttpGet("types/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelationshipType(int id)
    {
        try
        {
            var type = await _relationshipService.GetRelationshipTypeAsync(id);
            if (type == null)
                return NotFound(new { message = "Relationship type not found" });

            return Ok(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationship type {Id}", id);
            return StatusCode(500, new { message = "Error retrieving relationship type" });
        }
    }

    /// <summary>
    /// Create a new relationship type
    /// </summary>
    [HttpPost("types")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRelationshipType([FromBody] RelationshipTypeCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var type = await _relationshipService.CreateRelationshipTypeAsync(dto, userId);
            return CreatedAtAction(nameof(GetRelationshipType), new { id = type.Id }, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating relationship type");
            return StatusCode(500, new { message = "Error creating relationship type" });
        }
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
                return NotFound(new { message = "Relationship type not found" });

            return Ok(type);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relationship type {Id}", id);
            return StatusCode(500, new { message = "Error updating relationship type" });
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
                return NotFound(new { message = "Relationship type not found" });

            return Ok(new { message = "Relationship type deleted" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting relationship type {Id}", id);
            return StatusCode(500, new { message = "Error deleting relationship type" });
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationships");
            return StatusCode(500, new { message = "Error retrieving relationships" });
        }
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
        try
        {
            var relationships = await _relationshipService.GetAccountRelationshipsAsync(
                accountId, status, relationshipTypeId);
            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationships for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Error retrieving account relationships" });
        }
    }

    /// <summary>
    /// Get a relationship by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelationship(int id)
    {
        try
        {
            var relationship = await _relationshipService.GetRelationshipAsync(id);
            if (relationship == null)
                return NotFound(new { message = "Relationship not found" });

            return Ok(relationship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationship {Id}", id);
            return StatusCode(500, new { message = "Error retrieving relationship" });
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating relationship");
            return StatusCode(500, new { message = "Error creating relationship" });
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
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.UpdateRelationshipAsync(id, dto, userId);
            if (relationship == null)
                return NotFound(new { message = "Relationship not found" });

            return Ok(relationship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relationship {Id}", id);
            return StatusCode(500, new { message = "Error updating relationship" });
        }
    }

    /// <summary>
    /// Delete a relationship
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRelationship(int id)
    {
        try
        {
            var result = await _relationshipService.DeleteRelationshipAsync(id);
            if (!result)
                return NotFound(new { message = "Relationship not found" });

            return Ok(new { message = "Relationship deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting relationship {Id}", id);
            return StatusCode(500, new { message = "Error deleting relationship" });
        }
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
        try
        {
            var interactions = await _relationshipService.GetRelationshipInteractionsAsync(relationshipId);
            return Ok(interactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting interactions for relationship {RelationshipId}", relationshipId);
            return StatusCode(500, new { message = "Error retrieving interactions" });
        }
    }

    /// <summary>
    /// Create an interaction for a relationship
    /// </summary>
    [HttpPost("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateInteraction([FromBody] RelationshipInteractionCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var interaction = await _relationshipService.CreateInteractionAsync(dto, userId);
            return Ok(interaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating interaction");
            return StatusCode(500, new { message = "Error creating interaction" });
        }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationship map for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Error retrieving relationship map" });
        }
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
        try
        {
            var snapshots = await _relationshipService.GetHealthSnapshotsAsync(accountId, startDate, endDate);
            return Ok(snapshots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health snapshots for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Error retrieving health snapshots" });
        }
    }

    /// <summary>
    /// Create a health snapshot for an account
    /// </summary>
    [HttpPost("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateHealthSnapshot([FromBody] AccountHealthSnapshotCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var snapshot = await _relationshipService.CreateHealthSnapshotAsync(dto, userId);
            return Ok(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating health snapshot");
            return StatusCode(500, new { message = "Error creating health snapshot" });
        }
    }

    #endregion
}
