// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for team management operations.
/// Handles sales teams and territory assignments.
/// </summary>
public interface ITeamService
{
    #region CRUD Operations

    /// <summary>Gets all teams with optional filtering.</summary>
    Task<IEnumerable<Team>> GetAllAsync(
        bool? isActive = null,
        int? managerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a team by ID.</summary>
    Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a team by name.</summary>
    Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Creates a new team.</summary>
    Task<Team> CreateAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing team.</summary>
    Task<Team> UpdateAsync(Team team, CancellationToken cancellationToken = default);

    /// <summary>Deletes a team (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Member Management

    /// <summary>Adds a member to a team.</summary>
    Task<TeamMember> AddMemberAsync(int teamId, int userId, TeamRole role = TeamRole.Member, CancellationToken cancellationToken = default);

    /// <summary>Removes a member from a team.</summary>
    Task<bool> RemoveMemberAsync(int teamId, int userId, CancellationToken cancellationToken = default);

    /// <summary>Updates a member's role in a team.</summary>
    Task<TeamMember> UpdateMemberRoleAsync(int teamId, int userId, TeamRole newRole, CancellationToken cancellationToken = default);

    /// <summary>Gets all members of a team.</summary>
    Task<IEnumerable<TeamMember>> GetMembersAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>Gets teams for a user.</summary>
    Task<IEnumerable<Team>> GetTeamsForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Checks if user is member of team.</summary>
    Task<bool> IsMemberAsync(int teamId, int userId, CancellationToken cancellationToken = default);

    #endregion

    #region Team Manager

    /// <summary>Sets the team manager.</summary>
    Task<Team> SetManagerAsync(int teamId, int managerId, CancellationToken cancellationToken = default);

    /// <summary>Gets teams managed by a user.</summary>
    Task<IEnumerable<Team>> GetManagedTeamsAsync(int managerId, CancellationToken cancellationToken = default);

    #endregion

    #region Territory Management

    /// <summary>Assigns a territory to a team.</summary>
    Task<bool> AssignTerritoryAsync(int teamId, int territoryId, CancellationToken cancellationToken = default);

    /// <summary>Removes a territory from a team.</summary>
    Task<bool> RemoveTerritoryAsync(int teamId, int territoryId, CancellationToken cancellationToken = default);

    /// <summary>Gets territories assigned to a team.</summary>
    Task<IEnumerable<AccountTerritory>> GetTerritoriesAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>Gets team for a territory.</summary>
    Task<Team?> GetTeamByTerritoryAsync(int territoryId, CancellationToken cancellationToken = default);

    #endregion

    #region Account Assignment

    /// <summary>Assigns an account to a team.</summary>
    Task<bool> AssignAccountAsync(int teamId, int accountId, CancellationToken cancellationToken = default);

    /// <summary>Removes an account assignment from a team.</summary>
    Task<bool> RemoveAccountAsync(int teamId, int accountId, CancellationToken cancellationToken = default);

    /// <summary>Gets accounts assigned to a team.</summary>
    Task<IEnumerable<Account>> GetAssignedAccountsAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>Gets team for an account.</summary>
    Task<Team?> GetTeamByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>Bulk assigns accounts to a team.</summary>
    Task<int> BulkAssignAccountsAsync(int teamId, IEnumerable<int> accountIds, CancellationToken cancellationToken = default);

    #endregion

    #region Performance & Stats

    /// <summary>Gets team performance metrics.</summary>
    Task<TeamPerformance> GetPerformanceAsync(int teamId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets team statistics.</summary>
    Task<TeamStatistics> GetStatisticsAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>Gets leaderboard for teams.</summary>
    Task<IEnumerable<TeamRanking>> GetLeaderboardAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets member performance within a team.</summary>
    Task<IEnumerable<MemberPerformance>> GetMemberPerformanceAsync(int teamId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Hierarchy

    /// <summary>Gets child teams (sub-teams).</summary>
    Task<IEnumerable<Team>> GetChildTeamsAsync(int parentTeamId, CancellationToken cancellationToken = default);

    /// <summary>Gets parent team.</summary>
    Task<Team?> GetParentTeamAsync(int teamId, CancellationToken cancellationToken = default);

    /// <summary>Sets parent team for hierarchy.</summary>
    Task<Team> SetParentTeamAsync(int teamId, int? parentTeamId, CancellationToken cancellationToken = default);

    /// <summary>Gets full team hierarchy.</summary>
    Task<TeamHierarchy> GetHierarchyAsync(int? rootTeamId = null, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Team role within a team.
/// </summary>
public enum TeamRole
{
    Member = 0,
    Lead = 1,
    Manager = 2,
    Admin = 3
}

/// <summary>
/// Team performance metrics.
/// </summary>
public class TeamPerformance
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalQuotaValue { get; set; }
    public double QuotaAttainment { get; set; }
    public int DealsWon { get; set; }
    public int DealsLost { get; set; }
    public double WinRate { get; set; }
    public decimal AverageDealSize { get; set; }
    public int NewAccounts { get; set; }
    public int ActiveOpportunities { get; set; }
    public decimal PipelineValue { get; set; }
}

/// <summary>
/// Team statistics.
/// </summary>
public class TeamStatistics
{
    public int TeamId { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int AssignedAccounts { get; set; }
    public int ActiveOpportunities { get; set; }
    public int AssignedTerritories { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Team ranking for leaderboard.
/// </summary>
public class TeamRanking
{
    public int Rank { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int DealsWon { get; set; }
    public double QuotaAttainment { get; set; }
}

/// <summary>
/// Member performance within a team.
/// </summary>
public class MemberPerformance
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int DealsWon { get; set; }
    public int DealsLost { get; set; }
    public double WinRate { get; set; }
    public decimal PipelineValue { get; set; }
    public double QuotaAttainment { get; set; }
}

/// <summary>
/// Team hierarchy node.
/// </summary>
public class TeamHierarchy
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int MemberCount { get; set; }
    public List<TeamHierarchy> Children { get; set; } = new();
}
