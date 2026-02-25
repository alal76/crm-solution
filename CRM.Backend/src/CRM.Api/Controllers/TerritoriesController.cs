// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountTerritory>> ActivateTerritory(int id, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.ActivateTerritoryAsync(id, cancellationToken);
        return Ok(territory);
    }

    /// <summary>
    /// Deactivate a territory.
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritoryAssignment), StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritoryAssignment>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<Account>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Account>>> GetTerritoryAccounts(int territoryId, CancellationToken cancellationToken)
    {
        var accounts = await _territoryService.GetTerritoryAccountsAsync(territoryId, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// Set the primary territory for an account.
    /// </summary>
    [HttpPut("accounts/{accountId:int}/primary")]
    [ProducesResponseType(typeof(AccountTerritoryAssignment), StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> GetUserTerritories(int userId, CancellationToken cancellationToken)
    {
        var territories = await _territoryService.GetUserTerritoriesAsync(userId, cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Link a territory to a team.
    /// </summary>
    [HttpPut("{territoryId:int}/team")]
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritoryAssignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(TerritoryQuotaStatus), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<TerritoryQuotaStatus>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(TerritoryStatistics), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<TerritoryRanking>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(TerritoryDistribution), StatusCodes.Status200OK)]
    public async Task<ActionResult<TerritoryDistribution>> GetAccountDistribution(CancellationToken cancellationToken)
    {
        var distribution = await _territoryService.GetAccountDistributionAsync(cancellationToken);
        return Ok(distribution);
    }

    /// <summary>
    /// Get territories with unassigned accounts.
    /// </summary>
    [HttpGet("unassigned-accounts")]
    [ProducesResponseType(typeof(IEnumerable<UnassignedAccountsSummary>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
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

    #region Hierarchy

    /// <summary>
    /// Get territory hierarchy (flat list with parent info — no parent/child entity exists, returns all territories).
    /// </summary>
    [HttpGet("hierarchy")]
    [ProducesResponseType(typeof(IEnumerable<TerritoryHierarchyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TerritoryHierarchyDto>>> GetHierarchy(CancellationToken cancellationToken)
    {
        var territories = await _territoryService.GetAllTerritoriesAsync(cancellationToken: cancellationToken);
        var hierarchy = territories.Select(t => new TerritoryHierarchyDto
        {
            Id = t.Id,
            Name = t.TerritoryName,
            Code = t.TerritoryCode,
            Description = t.Description,
            IsActive = t.IsActive,
            ParentId = null,
            Children = new List<TerritoryHierarchyDto>()
        });
        return Ok(hierarchy);
    }

    #endregion

    #region Aggregate Statistics

    /// <summary>
    /// Get statistics for all territories.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(IEnumerable<TerritoryStatistics>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TerritoryStatistics>>> GetAllStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var territories = await _territoryService.GetAllTerritoriesAsync(cancellationToken: cancellationToken);
        var statsList = new List<TerritoryStatistics>();
        foreach (var t in territories)
        {
            try
            {
                var stats = await _territoryService.GetTerritoryStatisticsAsync(t.Id, fromDate, toDate, cancellationToken);
                statsList.Add(stats);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get statistics for territory {TerritoryId}", t.Id);
            }
        }
        return Ok(statsList);
    }

    /// <summary>
    /// Get territory leaderboard by metric.
    /// </summary>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(IEnumerable<TerritoryRanking>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TerritoryRanking>>> GetLeaderboard(
        [FromQuery] string metric = "revenue",
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var rankingMetric = metric.ToLowerInvariant() switch
        {
            "deals" => TerritoryRankingMetric.OpportunitiesWon,
            "pipeline" => TerritoryRankingMetric.PipelineValue,
            "winrate" or "quota" => TerritoryRankingMetric.QuotaAttainment,
            "newaccounts" => TerritoryRankingMetric.NewAccounts,
            _ => TerritoryRankingMetric.Revenue
        };
        var rankings = await _territoryService.GetTerritoryRankingsAsync(limit, rankingMetric, cancellationToken: cancellationToken);
        return Ok(rankings);
    }

    #endregion

    #region Region Lookup

    /// <summary>
    /// Get territories by region name.
    /// </summary>
    [HttpGet("region/{region}")]
    [ProducesResponseType(typeof(IEnumerable<AccountTerritory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountTerritory>>> GetByRegion(
        string region,
        CancellationToken cancellationToken)
    {
        var territories = await _territoryService.GetTerritoriesByLocationAsync(region: region, cancellationToken: cancellationToken);
        return Ok(territories);
    }

    /// <summary>
    /// Get territory by code.
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(AccountTerritory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountTerritory>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.GetTerritoryByCodeAsync(code, cancellationToken);
        if (territory == null)
            return NotFound();
        return Ok(territory);
    }

    #endregion

    #region Rules & Quotas (Stub endpoints)

    /// <summary>
    /// Get rules for a territory (territory matching rules derived from territory config).
    /// </summary>
    [HttpGet("{territoryId:int}/rules")]
    [ProducesResponseType(typeof(IEnumerable<TerritoryRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TerritoryRuleDto>>> GetRules(int territoryId, CancellationToken cancellationToken)
    {
        var territory = await _territoryService.GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            return NotFound();

        // Derive rules from territory config fields
        var rules = new List<TerritoryRuleDto>();
        int ruleId = 1;

        if (!string.IsNullOrWhiteSpace(territory.Countries))
            rules.Add(new TerritoryRuleDto { Id = ruleId++, TerritoryId = territoryId, Field = "Country", Operator = "in", Value = territory.Countries, IsActive = true });
        if (!string.IsNullOrWhiteSpace(territory.Regions))
            rules.Add(new TerritoryRuleDto { Id = ruleId++, TerritoryId = territoryId, Field = "Region", Operator = "in", Value = territory.Regions, IsActive = true });
        if (!string.IsNullOrWhiteSpace(territory.States))
            rules.Add(new TerritoryRuleDto { Id = ruleId++, TerritoryId = territoryId, Field = "State", Operator = "in", Value = territory.States, IsActive = true });
        if (!string.IsNullOrWhiteSpace(territory.Industries))
            rules.Add(new TerritoryRuleDto { Id = ruleId++, TerritoryId = territoryId, Field = "Industry", Operator = "in", Value = territory.Industries, IsActive = true });
        if (territory.RevenueRangeMin.HasValue || territory.RevenueRangeMax.HasValue)
            rules.Add(new TerritoryRuleDto { Id = ruleId++, TerritoryId = territoryId, Field = "AnnualRevenue", Operator = "between", Value = $"{territory.RevenueRangeMin ?? 0}-{territory.RevenueRangeMax ?? decimal.MaxValue}", IsActive = true });

        return Ok(rules);
    }

    /// <summary>
    /// Get quotas for a territory.
    /// </summary>
    [HttpGet("{territoryId:int}/quotas")]
    [ProducesResponseType(typeof(IEnumerable<TerritoryQuotaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TerritoryQuotaDto>>> GetQuotas(
        int territoryId,
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        var territory = await _territoryService.GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            return NotFound();

        var quotas = new List<TerritoryQuotaDto>();
        if (territory.AnnualQuota.HasValue && territory.AnnualQuota.Value > 0)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            quotas.Add(new TerritoryQuotaDto
            {
                Id = 1,
                TerritoryId = territoryId,
                TerritoryName = territory.TerritoryName,
                Year = targetYear,
                Quarter = null,
                Amount = territory.AnnualQuota.Value,
                Currency = territory.QuotaCurrency,
                Achieved = 0, // Would need opportunity data to calculate
                Period = "Annual"
            });
        }

        return Ok(quotas);
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

    // =========================================================================
    // Lead Territory Assignment (TODO-GAP-04)
    // =========================================================================

    /// <summary>
    /// Assigns a lead to this territory and optionally changes its owner.
    /// </summary>
    [HttpPost("{id:int}/assign-lead")]
    [ProducesResponseType(typeof(Lead), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignLead(
        int id,
        [FromBody] AssignLeadToTerritoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _territoryService.AssignLeadToTerritoryAsync(
                request.LeadId,
                id,
                request.UserId,
                cancellationToken);

            return Ok(updated);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning lead {LeadId} to territory {TerritoryId}", request.LeadId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Returns all open leads assigned to this territory.
    /// </summary>
    [HttpGet("{id:int}/leads")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeadsByTerritory(
        int id,
        [FromServices] CRM.Core.Interfaces.ICrmDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var leads = await dbContext.Leads
                .Where(l => !l.IsDeleted && l.TerritoryId == id)
                .Select(l => new
                {
                    l.Id,
                    l.FirstName,
                    l.LastName,
                    l.Email,
                    l.CompanyName,
                    Status = l.Status.ToString(),
                    l.OwnerId,
                    l.TerritoryId,
                    l.CreatedAt
                })
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(cancellationToken);

            return Ok(leads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leads for territory {TerritoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
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

/// <summary>Request body for assigning a lead to a territory (TODO-GAP-04).</summary>
public class AssignLeadToTerritoryRequest
{
    /// <summary>ID of the lead to assign.</summary>
    public int LeadId { get; set; }

    /// <summary>Optional user ID to set as lead owner after assignment.</summary>
    public int? UserId { get; set; }
}

public class TerritoryHierarchyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? ParentId { get; set; }
    public List<TerritoryHierarchyDto> Children { get; set; } = new();
}

public class TerritoryRuleDto
{
    public int Id { get; set; }
    public int TerritoryId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class TerritoryQuotaDto
{
    public int Id { get; set; }
    public int TerritoryId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int? Quarter { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Achieved { get; set; }
    public string Period { get; set; } = "Annual";
}

#endregion
