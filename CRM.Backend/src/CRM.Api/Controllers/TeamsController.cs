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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for team management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamsController"/> class.
    /// </summary>
    /// <param name="teamService">The team service.</param>
    /// <param name="logger">The logger instance.</param>
    public TeamsController(ITeamService teamService, ILogger<TeamsController> logger)
    {
        _teamService = teamService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>Gets all teams with optional filtering.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null, [FromQuery] int? managerId = null, CancellationToken cancellationToken = default)
    {
        var teams = await _teamService.GetAllAsync(isActive, managerId, cancellationToken);
        return Ok(teams);
    }

    /// <summary>Gets a team by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var team = await _teamService.GetByIdAsync(id, cancellationToken);
        if (team == null)
        {
            return NotFound($"Team with ID {id} not found.");
        }

        return Ok(team);
    }

    /// <summary>Gets a team by name.</summary>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken = default)
    {
        var team = await _teamService.GetByNameAsync(name, cancellationToken);
        if (team == null)
        {
            return NotFound($"Team with name '{name}' not found.");
        }

        return Ok(team);
    }

    /// <summary>Creates a new team.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] Team team, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var created = await _teamService.CreateAsync(team, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates an existing team.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] Team team, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            team.Id = id;
            var updated = await _teamService.UpdateAsync(team, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", id);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Deletes a team (soft delete).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _teamService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound($"Team with ID {id} not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", id);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Member Management

    /// <summary>Adds a member to a team.</summary>
    [HttpPost("{teamId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddMember(int teamId, [FromBody] AddMemberRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _teamService.AddMemberAsync(teamId, request.UserId, request.Role, cancellationToken);
            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member {UserId} to team {TeamId}", request.UserId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Removes a member from a team.</summary>
    [HttpDelete("{teamId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveMember(int teamId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _teamService.RemoveMemberAsync(teamId, userId, cancellationToken);
            if (!result)
                return NotFound($"Member {userId} not found in team {teamId}.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member {UserId} from team {TeamId}", userId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Updates a member's role in a team.</summary>
    [HttpPut("{teamId}/members/{userId}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMemberRole(int teamId, int userId, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _teamService.UpdateMemberRoleAsync(teamId, userId, request.Role, cancellationToken);
            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for member {UserId} in team {TeamId}", userId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets all members of a team.</summary>
    [HttpGet("{teamId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(int teamId, CancellationToken cancellationToken = default)
    {
        var members = await _teamService.GetMembersAsync(teamId, cancellationToken);
        return Ok(members);
    }

    /// <summary>Gets teams for a user.</summary>
    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamsForUser(int userId, CancellationToken cancellationToken = default)
    {
        var teams = await _teamService.GetTeamsForUserAsync(userId, cancellationToken);
        return Ok(teams);
    }

    /// <summary>Checks if a user is a member of a team.</summary>
    [HttpGet("{teamId}/members/{userId}/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IsMember(int teamId, int userId, CancellationToken cancellationToken = default)
    {
        var isMember = await _teamService.IsMemberAsync(teamId, userId, cancellationToken);
        return Ok(new { teamId, userId, isMember });
    }

    #endregion

    #region Team Manager

    /// <summary>Sets the team manager.</summary>
    [HttpPut("{teamId}/manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetManager(int teamId, [FromBody] SetManagerRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var team = await _teamService.SetManagerAsync(teamId, request.ManagerId, cancellationToken);
            return Ok(team);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting manager for team {TeamId}", teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets teams managed by a user.</summary>
    [HttpGet("managed-by/{managerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetManagedTeams(int managerId, CancellationToken cancellationToken = default)
    {
        var teams = await _teamService.GetManagedTeamsAsync(managerId, cancellationToken);
        return Ok(teams);
    }

    #endregion

    #region Territory Management

    /// <summary>Assigns a territory to a team.</summary>
    [HttpPost("{teamId}/territories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignTerritory(int teamId, [FromBody] AssignTerritoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _teamService.AssignTerritoryAsync(teamId, request.TerritoryId, cancellationToken);
            if (!result)
                return BadRequest("Failed to assign territory.");
            return Ok(new { teamId, territoryId = request.TerritoryId, assigned = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning territory {TerritoryId} to team {TeamId}", request.TerritoryId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Removes a territory from a team.</summary>
    [HttpDelete("{teamId}/territories/{territoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveTerritory(int teamId, int territoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _teamService.RemoveTerritoryAsync(teamId, territoryId, cancellationToken);
            if (!result)
                return NotFound($"Territory {territoryId} not assigned to team {teamId}.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing territory {TerritoryId} from team {TeamId}", territoryId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets territories assigned to a team.</summary>
    [HttpGet("{teamId}/territories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTerritories(int teamId, CancellationToken cancellationToken = default)
    {
        var territories = await _teamService.GetTerritoriesAsync(teamId, cancellationToken);
        return Ok(territories);
    }

    /// <summary>Gets team for a territory.</summary>
    [HttpGet("by-territory/{territoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamByTerritory(int territoryId, CancellationToken cancellationToken = default)
    {
        var team = await _teamService.GetTeamByTerritoryAsync(territoryId, cancellationToken);
        if (team == null)
            return NotFound($"No team found for territory {territoryId}.");
        return Ok(team);
    }

    #endregion

    #region Account Assignment

    /// <summary>Assigns an account to a team.</summary>
    [HttpPost("{teamId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignAccount(int teamId, [FromBody] AssignAccountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _teamService.AssignAccountAsync(teamId, request.AccountId, cancellationToken);
            if (!result)
                return BadRequest("Failed to assign account.");
            return Ok(new { teamId, accountId = request.AccountId, assigned = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning account {AccountId} to team {TeamId}", request.AccountId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Removes an account assignment from a team.</summary>
    [HttpDelete("{teamId}/accounts/{accountId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveAccount(int teamId, int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _teamService.RemoveAccountAsync(teamId, accountId, cancellationToken);
            if (!result)
                return NotFound($"Account {accountId} not assigned to team {teamId}.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing account {AccountId} from team {TeamId}", accountId, teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets accounts assigned to a team.</summary>
    [HttpGet("{teamId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignedAccounts(int teamId, CancellationToken cancellationToken = default)
    {
        var accounts = await _teamService.GetAssignedAccountsAsync(teamId, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>Gets team for an account.</summary>
    [HttpGet("by-account/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamByAccount(int accountId, CancellationToken cancellationToken = default)
    {
        var team = await _teamService.GetTeamByAccountAsync(accountId, cancellationToken);
        if (team == null)
            return NotFound($"No team found for account {accountId}.");
        return Ok(team);
    }

    /// <summary>Bulk assigns accounts to a team.</summary>
    [HttpPost("{teamId}/accounts/bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BulkAssignAccounts(int teamId, [FromBody] BulkAssignAccountsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _teamService.BulkAssignAccountsAsync(teamId, request.AccountIds, cancellationToken);
            return Ok(new { teamId, assignedCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk assigning accounts to team {TeamId}", teamId);
            return HandleServiceException(ex);
        }
    }

    #endregion

    #region Performance & Stats

    /// <summary>Gets team performance metrics.</summary>
    [HttpGet("{teamId}/performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPerformance(int teamId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var performance = await _teamService.GetPerformanceAsync(teamId, fromDate, toDate, cancellationToken);
        return Ok(performance);
    }

    /// <summary>Gets team statistics.</summary>
    [HttpGet("{teamId}/statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics(int teamId, CancellationToken cancellationToken = default)
    {
        var stats = await _teamService.GetStatisticsAsync(teamId, cancellationToken);
        return Ok(stats);
    }

    /// <summary>Gets leaderboard for teams.</summary>
    [HttpGet("leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int topN = 10, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var rankings = await _teamService.GetLeaderboardAsync(topN, fromDate, toDate, cancellationToken);
        return Ok(rankings);
    }

    /// <summary>Gets member performance within a team.</summary>
    [HttpGet("{teamId}/members/performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMemberPerformance(int teamId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var performance = await _teamService.GetMemberPerformanceAsync(teamId, fromDate, toDate, cancellationToken);
        return Ok(performance);
    }

    #endregion

    #region Hierarchy

    /// <summary>Gets child teams (sub-teams).</summary>
    [HttpGet("{teamId}/children")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChildTeams(int teamId, CancellationToken cancellationToken = default)
    {
        var children = await _teamService.GetChildTeamsAsync(teamId, cancellationToken);
        return Ok(children);
    }

    /// <summary>Gets parent team.</summary>
    [HttpGet("{teamId}/parent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetParentTeam(int teamId, CancellationToken cancellationToken = default)
    {
        var parent = await _teamService.GetParentTeamAsync(teamId, cancellationToken);
        if (parent == null)
            return NotFound($"Team {teamId} has no parent team.");
        return Ok(parent);
    }

    /// <summary>Sets parent team for hierarchy.</summary>
    [HttpPut("{teamId}/parent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetParentTeam(int teamId, [FromBody] SetParentTeamRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var team = await _teamService.SetParentTeamAsync(teamId, request.ParentTeamId, cancellationToken);
            return Ok(team);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting parent team for team {TeamId}", teamId);
            return HandleServiceException(ex);
        }
    }

    /// <summary>Gets full team hierarchy.</summary>
    [HttpGet("hierarchy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHierarchy([FromQuery] int? rootTeamId = null, CancellationToken cancellationToken = default)
    {
        var hierarchy = await _teamService.GetHierarchyAsync(rootTeamId, cancellationToken);
        return Ok(hierarchy);
    }

    #endregion

    #region Request DTOs

    /// <summary>Request to add a member to a team.</summary>
    public class AddMemberRequest
    {
        public int UserId { get; set; }
        public TeamRole Role { get; set; } = TeamRole.Member;
    }

    /// <summary>Request to update a member's role.</summary>
    public class UpdateRoleRequest
    {
        public TeamRole Role { get; set; }
    }

    /// <summary>Request to set a team manager.</summary>
    public class SetManagerRequest
    {
        public int ManagerId { get; set; }
    }

    /// <summary>Request to assign a territory to a team.</summary>
    public class AssignTerritoryRequest
    {
        public int TerritoryId { get; set; }
    }

    /// <summary>Request to assign an account to a team.</summary>
    public class AssignAccountRequest
    {
        public int AccountId { get; set; }
    }

    /// <summary>Request to bulk assign accounts to a team.</summary>
    public class BulkAssignAccountsRequest
    {
        public List<int> AccountIds { get; set; } = new();
    }

    /// <summary>Request to set parent team.</summary>
    public class SetParentTeamRequest
    {
        public int? ParentTeamId { get; set; }
    }

    #endregion

    #region Private Helpers

    private IActionResult HandleServiceException(Exception ex)
    {
        if (ex is InvalidOperationException && ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(ex.Message);

        return BadRequest(ex.Message);
    }

    #endregion
}
