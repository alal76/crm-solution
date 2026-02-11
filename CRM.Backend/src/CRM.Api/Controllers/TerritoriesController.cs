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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing sales territories and account assignments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TerritoriesController : ControllerBase
{
    private readonly ITerritoryService _territoryService;
    private readonly ILogger<TerritoriesController> _logger;

    public TerritoriesController(ITerritoryService territoryService, ILogger<TerritoriesController> logger)
    {
        _territoryService = territoryService;
        _logger = logger;
    }

    #region Territory CRUD

    /// <summary>
    /// Get all territories with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> GetAllTerritories(
        [FromQuery] bool? isActive = null,
        [FromQuery] int? teamId = null,
        [FromQuery] int? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        var territories = await _territoryService.GetAllTerritoriesAsync(isActive, teamId, ownerId, cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Get a territory by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountTerritory>> GetTerritoryById(int id, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.GetTerritoryByIdAsync(id, cancellationToken);
        if (territory == null)
        {
            return NotFound($"Territory with ID {id} not found.");
        }
        return Ok(territory);
    }

    /// <summary>
    /// Get a territory by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<AccountTerritory>> GetTerritoryByCode(string code, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.GetTerritoryByCodeAsync(code, cancellationToken);
        if (territory == null)
        {
            return NotFound($"Territory with code '{code}' not found.");
        }
        return Ok(territory);
    }

    /// <summary>
    /// Create a new territory.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccountTerritory>> CreateTerritory(
        [FromBody] AccountTerritory territory,
        CancellationToken cancellationToken)
    {
        var created = await _territoryService.CreateTerritoryAsync(territory, cancellationToken);
        return CreatedAtAction(nameof(GetTerritoryById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing territory.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountTerritory>> UpdateTerritory(
        int id,
        [FromBody] AccountTerritory territory,
        CancellationToken cancellationToken)
    {
        if (id != territory.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _territoryService.UpdateTerritoryAsync(territory, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Delete a territory (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTerritory(int id, CancellationToken cancellationToken)
    {
        var result = await _territoryService.DeleteTerritoryAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound($"Territory with ID {id} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Activate a territory.
    /// </summary>
    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<AccountTerritory>> ActivateTerritory(int id, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.ActivateTerritoryAsync(id, cancellationToken);
        return Ok(territory);
    }

    /// <summary>
    /// Deactivate a territory.
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<AccountTerritory>> DeactivateTerritory(int id, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.DeactivateTerritoryAsync(id, cancellationToken);
        return Ok(territory);
    }

    #endregion

    #region Territory Assignment

    /// <summary>
    /// Assign an account to a territory.
    /// </summary>
    [HttpPost("{territoryId:int}/accounts/{accountId:int}")]
    public async Task<ActionResult<AccountTerritoryAssignment>> AssignAccount(
        int territoryId,
        int accountId,
        [FromQuery] bool isPrimary = true,
        [FromQuery] string? notes = null,
        CancellationToken cancellationToken = default)
    {
        // Get current user ID from claims (simplified - in real app, get from HttpContext)
        var assignedById = GetCurrentUserId();
        var assignment = await _territoryService.AssignAccountAsync(accountId, territoryId, assignedById, isPrimary, notes, cancellationToken);
        return Ok(assignment);
    }

    /// <summary>
    /// Remove an account from a territory.
    /// </summary>
    [HttpDelete("{territoryId:int}/accounts/{accountId:int}")]
    public async Task<ActionResult> UnassignAccount(int territoryId, int accountId, CancellationToken cancellationToken)
    {
        var result = await _territoryService.UnassignAccountAsync(accountId, territoryId, cancellationToken);
        if (!result)
        {
            return NotFound("Account-territory assignment not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Get all territory assignments for an account.
    /// </summary>
    [HttpGet("accounts/{accountId:int}/assignments")]
    public async Task<ActionResult<IEnumerable<AccountTerritoryAssignment>>> GetAccountAssignments(
        int accountId,
        CancellationToken cancellationToken)
    {
        var assignments = await _territoryService.GetAccountAssignmentsAsync(accountId, cancellationToken);
        return Ok(assignments);
    }

    /// <summary>
    /// Get all accounts in a territory.
    /// </summary>
    [HttpGet("{territoryId:int}/accounts")]
    public async Task<ActionResult<IEnumerable<Account>>> GetTerritoryAccounts(int territoryId, CancellationToken cancellationToken)
    {
        var accounts = await _territoryService.GetTerritoryAccountsAsync(territoryId, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// Set the primary territory for an account.
    /// </summary>
    [HttpPut("accounts/{accountId:int}/primary")]
    public async Task<ActionResult<AccountTerritoryAssignment>> SetPrimaryTerritory(
        int accountId,
        [FromBody] SetPrimaryTerritoryRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await _territoryService.SetPrimaryTerritoryAsync(accountId, request.TerritoryId, cancellationToken);
        return Ok(assignment);
    }

    /// <summary>
    /// Bulk assign accounts to a territory.
    /// </summary>
    [HttpPost("{territoryId:int}/accounts/bulk")]
    public async Task<ActionResult<int>> BulkAssignAccounts(
        int territoryId,
        [FromBody] BulkAssignAccountsRequest request,
        CancellationToken cancellationToken)
    {
        var assignedById = GetCurrentUserId();
        var count = await _territoryService.BulkAssignAccountsAsync(request.AccountIds, territoryId, assignedById, cancellationToken);
        return Ok(new { assignedCount = count });
    }

    /// <summary>
    /// Transfer accounts from one territory to another.
    /// </summary>
    [HttpPost("transfer")]
    public async Task<ActionResult<int>> TransferAccounts(
        [FromBody] TransferAccountsRequest request,
        CancellationToken cancellationToken)
    {
        var transferredById = GetCurrentUserId();
        var count = await _territoryService.TransferAccountsAsync(
            request.FromTerritoryId,
            request.ToTerritoryId,
            request.AccountIds,
            transferredById,
            cancellationToken);
        return Ok(new { transferredCount = count });
    }

    #endregion

    #region Territory Ownership

    /// <summary>
    /// Set the primary owner of a territory.
    /// </summary>
    [HttpPut("{territoryId:int}/owner")]
    public async Task<ActionResult<AccountTerritory>> SetTerritoryOwner(
        int territoryId,
        [FromBody] SetTerritoryOwnerRequest request,
        CancellationToken cancellationToken)
    {
        var territory = await _territoryService.SetTerritoryOwnerAsync(territoryId, request.OwnerId, cancellationToken);
        return Ok(territory);
    }

    /// <summary>
    /// Add team members to a territory.
    /// </summary>
    [HttpPost("{territoryId:int}/members")]
    public async Task<ActionResult<AccountTerritory>> AddTeamMembers(
        int territoryId,
        [FromBody] TeamMembersRequest request,
        CancellationToken cancellationToken)
    {
        var territory = await _territoryService.AddTeamMembersAsync(territoryId, request.UserIds, cancellationToken);
        return Ok(territory);
    }

    /// <summary>
    /// Remove team members from a territory.
    /// </summary>
    [HttpDelete("{territoryId:int}/members")]
    public async Task<ActionResult<AccountTerritory>> RemoveTeamMembers(
        int territoryId,
        [FromBody] TeamMembersRequest request,
        CancellationToken cancellationToken)
    {
        var territory = await _territoryService.RemoveTeamMembersAsync(territoryId, request.UserIds, cancellationToken);
        return Ok(territory);
    }

    /// <summary>
    /// Get all territories for a user.
    /// </summary>
    [HttpGet("users/{userId:int}")]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> GetUserTerritories(int userId, CancellationToken cancellationToken)
    {
        var territories = await _territoryService.GetUserTerritoriesAsync(userId, cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Link a territory to a team.
    /// </summary>
    [HttpPut("{territoryId:int}/team")]
    public async Task<ActionResult<AccountTerritory>> LinkToTeam(
        int territoryId,
        [FromBody] LinkToTeamRequest request,
        CancellationToken cancellationToken)
    {
        var territory = await _territoryService.LinkToTeamAsync(territoryId, request.TeamId, cancellationToken);
        return Ok(territory);
    }

    #endregion

    #region Territory Matching

    /// <summary>
    /// Find matching territories for an account.
    /// </summary>
    [HttpGet("accounts/{accountId:int}/matching")]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> FindMatchingTerritories(
        int accountId,
        CancellationToken cancellationToken)
    {
        var territories = await _territoryService.FindMatchingTerritoriesAsync(accountId, cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Find matching territories by criteria.
    /// </summary>
    [HttpPost("matching")]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> FindMatchingTerritoriesByCriteria(
        [FromBody] TerritoryMatchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var territories = await _territoryService.FindMatchingTerritoriesAsync(criteria, cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Auto-assign an account to the best matching territory.
    /// </summary>
    [HttpPost("accounts/{accountId:int}/auto-assign")]
    public async Task<ActionResult<AccountTerritoryAssignment>> AutoAssignAccount(
        int accountId,
        CancellationToken cancellationToken)
    {
        var assignment = await _territoryService.AutoAssignAccountAsync(accountId, cancellationToken);
        if (assignment == null)
        {
            return NotFound("No matching territory found for this account.");
        }
        return Ok(assignment);
    }

    /// <summary>
    /// Check if an account matches a territory's rules.
    /// </summary>
    [HttpGet("{territoryId:int}/accounts/{accountId:int}/matches")]
    public async Task<ActionResult<bool>> IsAccountInTerritory(
        int territoryId,
        int accountId,
        CancellationToken cancellationToken)
    {
        var matches = await _territoryService.IsAccountInTerritoryAsync(accountId, territoryId, cancellationToken);
        return Ok(new { matches });
    }

    #endregion

    #region Quota Management

    /// <summary>
    /// Set the quota for a territory.
    /// </summary>
    [HttpPut("{territoryId:int}/quota")]
    public async Task<ActionResult<AccountTerritory>> SetQuota(
        int territoryId,
        [FromBody] SetQuotaRequest request,
        CancellationToken cancellationToken)
    {
        var territory = await _territoryService.SetQuotaAsync(territoryId, request.Quota, request.Currency, cancellationToken);
        return Ok(territory);
    }

    /// <summary>
    /// Get quota attainment for a territory.
    /// </summary>
    [HttpGet("{territoryId:int}/quota/status")]
    public async Task<ActionResult<TerritoryQuotaStatus>> GetQuotaStatus(
        int territoryId,
        [FromQuery] DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var status = await _territoryService.GetQuotaStatusAsync(territoryId, asOfDate, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Get quota summary for all territories.
    /// </summary>
    [HttpGet("quota/status")]
    public async Task<ActionResult<IEnumerable<TerritoryQuotaStatus>>> GetAllQuotaStatuses(
        [FromQuery] DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var statuses = await _territoryService.GetAllQuotaStatusesAsync(asOfDate, cancellationToken);
        return Ok(statuses);
    }

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Get statistics for a territory.
    /// </summary>
    [HttpGet("{territoryId:int}/statistics")]
    public async Task<ActionResult<TerritoryStatistics>> GetTerritoryStatistics(
        int territoryId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _territoryService.GetTerritoryStatisticsAsync(territoryId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get performance rankings across all territories.
    /// </summary>
    [HttpGet("rankings")]
    public async Task<ActionResult<IEnumerable<TerritoryRanking>>> GetTerritoryRankings(
        [FromQuery] int topN = 10,
        [FromQuery] TerritoryRankingMetric metric = TerritoryRankingMetric.Revenue,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var rankings = await _territoryService.GetTerritoryRankingsAsync(topN, metric, fromDate, toDate, cancellationToken);
        return Ok(rankings);
    }

    /// <summary>
    /// Get account distribution across territories.
    /// </summary>
    [HttpGet("distribution")]
    public async Task<ActionResult<TerritoryDistribution>> GetAccountDistribution(CancellationToken cancellationToken)
    {
        var distribution = await _territoryService.GetAccountDistributionAsync(cancellationToken);
        return Ok(distribution);
    }

    /// <summary>
    /// Get territories with unassigned accounts.
    /// </summary>
    [HttpGet("unassigned-accounts")]
    public async Task<ActionResult<IEnumerable<UnassignedAccountsSummary>>> GetUnassignedAccountsSummary(CancellationToken cancellationToken)
    {
        var summary = await _territoryService.GetUnassignedAccountsSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    #endregion

    #region Territory Search

    /// <summary>
    /// Search territories by name, code, or description.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> SearchTerritories(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var territories = await _territoryService.SearchTerritoriesAsync(query, cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Get territories by geographic criteria.
    /// </summary>
    [HttpGet("by-location")]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> GetTerritoriesByLocation(
        [FromQuery] string? country = null,
        [FromQuery] string? region = null,
        [FromQuery] string? state = null,
        [FromQuery] string? city = null,
        CancellationToken cancellationToken = default)
    {
        var territories = await _territoryService.GetTerritoriesByLocationAsync(country, region, state, city, cancellationToken);
        return Ok(territories);
    }

    #endregion

    #region Helper Methods

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    #endregion
}

#region Request DTOs

public class SetPrimaryTerritoryRequest
{
    public int TerritoryId { get; set; }
}

public class BulkAssignAccountsRequest
{
    public IEnumerable<int> AccountIds { get; set; } = new List<int>();
}

public class TransferAccountsRequest
{
    public int FromTerritoryId { get; set; }
    public int ToTerritoryId { get; set; }
    public IEnumerable<int>? AccountIds { get; set; }
}

public class SetTerritoryOwnerRequest
{
    public int OwnerId { get; set; }
}

public class TeamMembersRequest
{
    public IEnumerable<int> UserIds { get; set; } = new List<int>();
}

public class LinkToTeamRequest
{
    public int TeamId { get; set; }
}

public class SetQuotaRequest
{
    public decimal Quota { get; set; }
    public string Currency { get; set; } = "USD";
}

#endregion
