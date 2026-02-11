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
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ITeamService for team management operations.
/// </summary>
public class TeamService : ITeamService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<TeamService> _logger;

    public TeamService(ICrmDbContext context, ILogger<TeamService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<Team>> GetAllAsync(
        bool? isActive = null,
        int? managerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members)
            .Where(t => !t.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        if (managerId.HasValue)
        {
            query = query.Where(t => t.ManagerId == managerId.Value);
        }

        return await query.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.ParentTeam)
            .Include(t => t.ChildTeams)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted, cancellationToken);
    }

    public async Task<Team> CreateAsync(Team team, CancellationToken cancellationToken = default)
    {
        team.CreatedAt = DateTime.UtcNow;
        team.UpdatedAt = DateTime.UtcNow;
        team.IsActive = true;

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created team {TeamName} with ID {TeamId}", team.Name, team.Id);
        return team;
    }

    public async Task<Team> UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Teams.FindAsync(new object[] { team.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Team {team.Id} not found");
        }

        team.UpdatedAt = DateTime.UtcNow;
        _context.Teams.Update(team);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated team {TeamId}", team.Id);
        return team;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FindAsync(new object[] { id }, cancellationToken);
        if (team == null) return false;

        team.IsDeleted = true;
        team.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted team {TeamId}", id);
        return true;
    }

    #endregion

    #region Member Management

    public async Task<TeamMember> AddMemberAsync(int teamId, int userId, TeamRole role = TeamRole.Member, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var existingMember = await _context.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (existingMember != null)
        {
            throw new InvalidOperationException($"User {userId} is already a member of team {teamId}");
        }

        var member = new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            Role = role.ToString(),
            IsTeamLead = role == TeamRole.Lead || role == TeamRole.Manager,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TeamMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added user {UserId} to team {TeamId} with role {Role}", userId, teamId, role);
        return member;
    }

    public async Task<bool> RemoveMemberAsync(int teamId, int userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (member == null) return false;

        member.IsDeleted = true;
        member.EndDate = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed user {UserId} from team {TeamId}", userId, teamId);
        return true;
    }

    public async Task<TeamMember> UpdateMemberRoleAsync(int teamId, int userId, TeamRole newRole, CancellationToken cancellationToken = default)
    {
        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (member == null)
        {
            throw new InvalidOperationException($"Member {userId} not found in team {teamId}");
        }

        member.Role = newRole.ToString();
        member.IsTeamLead = newRole == TeamRole.Lead || newRole == TeamRole.Manager;
        member.UpdatedAt = DateTime.UtcNow;

        _context.TeamMembers.Update(member);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated role for user {UserId} in team {TeamId} to {Role}", userId, teamId, newRole);
        return member;
    }

    public async Task<IEnumerable<TeamMember>> GetMembersAsync(int teamId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Include(m => m.User)
            .Where(m => m.TeamId == teamId && !m.IsDeleted)
            .OrderBy(m => m.User != null ? m.User.LastName : string.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Team>> GetTeamsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var teamIds = await _context.TeamMembers
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.TeamId)
            .ToListAsync(cancellationToken);

        return await _context.Teams
            .Include(t => t.Manager)
            .Where(t => teamIds.Contains(t.Id) && !t.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsMemberAsync(int teamId, int userId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId && !m.IsDeleted, cancellationToken);
    }

    #endregion

    #region Team Manager

    public async Task<Team> SetManagerAsync(int teamId, int managerId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        team.ManagerId = managerId;
        team.UpdatedAt = DateTime.UtcNow;

        _context.Teams.Update(team);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Set manager {ManagerId} for team {TeamId}", managerId, teamId);
        return team;
    }

    public async Task<IEnumerable<Team>> GetManagedTeamsAsync(int managerId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.ManagerId == managerId && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Territory Management

    public async Task<bool> AssignTerritoryAsync(int teamId, int territoryId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var territory = await _context.AccountTerritories.FindAsync(new object[] { territoryId }, cancellationToken);
        if (territory == null)
        {
            throw new InvalidOperationException($"Territory {territoryId} not found");
        }

        territory.TeamId = teamId;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned territory {TerritoryId} to team {TeamId}", territoryId, teamId);
        return true;
    }

    public async Task<bool> RemoveTerritoryAsync(int teamId, int territoryId, CancellationToken cancellationToken = default)
    {
        var territory = await _context.AccountTerritories
            .FirstOrDefaultAsync(t => t.Id == territoryId && t.TeamId == teamId, cancellationToken);

        if (territory == null) return false;

        territory.TeamId = null;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed territory {TerritoryId} from team {TeamId}", territoryId, teamId);
        return true;
    }

    public async Task<IEnumerable<AccountTerritory>> GetTerritoriesAsync(int teamId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTerritories
            .Where(t => t.TeamId == teamId && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetTeamByTerritoryAsync(int territoryId, CancellationToken cancellationToken = default)
    {
        var territory = await _context.AccountTerritories
            .FirstOrDefaultAsync(t => t.Id == territoryId, cancellationToken);

        if (territory?.TeamId == null) return null;

        return await GetByIdAsync(territory.TeamId.Value, cancellationToken);
    }

    #endregion

    #region Account Assignment

    public async Task<bool> AssignAccountAsync(int teamId, int accountId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var account = await _context.Customers.FindAsync(new object[] { accountId }, cancellationToken);
        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        // Assign account to team by setting OwnerId to the team manager or first available member.
        // Team accounts are accounts owned by team members — the standard CRM pattern.
        var assigneeId = team.ManagerId;

        if (assigneeId == null || assigneeId == 0)
        {
            var firstMember = await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId && !tm.IsDeleted)
                .OrderBy(tm => tm.Id)
                .FirstOrDefaultAsync(cancellationToken);

            assigneeId = firstMember?.UserId;
        }

        if (assigneeId == null || assigneeId == 0)
        {
            throw new InvalidOperationException($"Team {teamId} has no manager or members to assign the account to");
        }

        account.AssignedToUserId = assigneeId;
        account.UpdatedAt = DateTime.UtcNow;
        _context.Customers.Update(account);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned account {AccountId} to team {TeamId} via assignee {AssigneeId}", accountId, teamId, assigneeId);
        return true;
    }

    public async Task<bool> RemoveAccountAsync(int teamId, int accountId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var account = await _context.Customers.FindAsync(new object[] { accountId }, cancellationToken);
        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        // Verify the account is currently owned by a member of this team
        var teamMemberIds = await _context.TeamMembers
            .Where(tm => tm.TeamId == teamId && !tm.IsDeleted)
            .Select(tm => tm.UserId)
            .ToListAsync(cancellationToken);

        if (account.AssignedToUserId == null || !teamMemberIds.Contains(account.AssignedToUserId.Value))
        {
            _logger.LogWarning("Account {AccountId} is not assigned to a member of team {TeamId}", accountId, teamId);
            return false;
        }

        account.AssignedToUserId = null;
        account.UpdatedAt = DateTime.UtcNow;
        _context.Customers.Update(account);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed account {AccountId} from team {TeamId} by clearing AssignedToUserId", accountId, teamId);
        return true;
    }

    public async Task<IEnumerable<Account>> GetAssignedAccountsAsync(int teamId, CancellationToken cancellationToken = default)
    {
        // Team accounts = accounts owned by team members (via OwnerId → TeamMembers.UserId)
        var teamMemberUserIds = await _context.TeamMembers
            .Where(tm => tm.TeamId == teamId && !tm.IsDeleted)
            .Select(tm => tm.UserId)
            .ToListAsync(cancellationToken);

        if (!teamMemberUserIds.Any())
        {
            return Enumerable.Empty<Account>();
        }

        return await _context.Customers
            .Where(a => !a.IsDeleted && a.AssignedToUserId != null && teamMemberUserIds.Contains(a.AssignedToUserId.Value))
            .OrderBy(a => a.Company)
            .ThenBy(a => a.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetTeamByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        if (account?.AssignedToUserId == null)
        {
            return null;
        }

        // Find the first team that this account assignee belongs to
        var teamMember = await _context.TeamMembers
            .Include(tm => tm.Team)
            .Where(tm => tm.UserId == account.AssignedToUserId.Value && !tm.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return teamMember?.Team;
    }

    public async Task<int> BulkAssignAccountsAsync(int teamId, IEnumerable<int> accountIds, CancellationToken cancellationToken = default)
    {
        var count = 0;
        foreach (var accountId in accountIds)
        {
            if (await AssignAccountAsync(teamId, accountId, cancellationToken))
            {
                count++;
            }
        }
        return count;
    }

    #endregion

    #region Performance & Stats

    public async Task<TeamPerformance> GetPerformanceAsync(int teamId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var memberIds = team.Members?.Select(m => m.UserId).ToList() ?? new List<int>();

        var opportunities = await _context.Opportunities
            .Where(o => memberIds.Contains(o.SalesOwnerId ?? 0))
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var won = opportunities.Where(o => o.Stage == OpportunityStage.ClosedWon).ToList();
        var lost = opportunities.Where(o => o.Stage == OpportunityStage.ClosedLost).ToList();

        return new TeamPerformance
        {
            TeamId = teamId,
            TeamName = team.Name,
            FromDate = from,
            ToDate = to,
            TotalRevenue = won.Sum(o => o.Amount),
            TotalQuotaValue = 0, // Would need quota tracking
            QuotaAttainment = 0,
            DealsWon = won.Count,
            DealsLost = lost.Count,
            WinRate = won.Count + lost.Count > 0 ? (double)won.Count / (won.Count + lost.Count) * 100 : 0,
            AverageDealSize = won.Count > 0 ? won.Average(o => o.Amount) : 0,
            NewAccounts = 0, // Would need account assignment tracking
            ActiveOpportunities = opportunities.Count(o => o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost),
            PipelineValue = opportunities.Where(o => o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost).Sum(o => o.Amount)
        };
    }

    public async Task<TeamStatistics> GetStatisticsAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var members = await GetMembersAsync(teamId, cancellationToken);
        var territories = await GetTerritoriesAsync(teamId, cancellationToken);

        var memberIds = members.Select(m => m.UserId).ToList();
        var activeOpportunities = await _context.Opportunities
            .CountAsync(o => memberIds.Contains(o.SalesOwnerId ?? 0) &&
                           o.Stage != OpportunityStage.ClosedWon &&
                           o.Stage != OpportunityStage.ClosedLost, cancellationToken);

        return new TeamStatistics
        {
            TeamId = teamId,
            TotalMembers = members.Count(),
            ActiveMembers = members.Count(m => m.EndDate == null),
            AssignedAccounts = 0, // Would need account assignment tracking
            ActiveOpportunities = activeOpportunities,
            AssignedTerritories = territories.Count(),
            CreatedAt = team.CreatedAt
        };
    }

    public async Task<IEnumerable<TeamRanking>> GetLeaderboardAsync(int topN = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var teams = await _context.Teams
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        var rankings = new List<TeamRanking>();

        foreach (var team in teams)
        {
            var performance = await GetPerformanceAsync(team.Id, fromDate, toDate, cancellationToken);
            rankings.Add(new TeamRanking
            {
                TeamId = team.Id,
                TeamName = team.Name,
                Revenue = performance.TotalRevenue,
                DealsWon = performance.DealsWon,
                QuotaAttainment = performance.QuotaAttainment
            });
        }

        var ranked = rankings
            .OrderByDescending(r => r.Revenue)
            .Take(topN)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
        {
            ranked[i].Rank = i + 1;
        }

        return ranked;
    }

    public async Task<IEnumerable<MemberPerformance>> GetMemberPerformanceAsync(int teamId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var members = await GetMembersAsync(teamId, cancellationToken);
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var performances = new List<MemberPerformance>();

        foreach (var member in members)
        {
            var opportunities = await _context.Opportunities
                .Where(o => o.SalesOwnerId == member.UserId)
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            var won = opportunities.Where(o => o.Stage == OpportunityStage.ClosedWon).ToList();
            var lost = opportunities.Where(o => o.Stage == OpportunityStage.ClosedLost).ToList();

            performances.Add(new MemberPerformance
            {
                UserId = member.UserId,
                UserName = member.User != null ? $"{member.User.FirstName} {member.User.LastName}" : "Unknown",
                Revenue = won.Sum(o => o.Amount),
                DealsWon = won.Count,
                DealsLost = lost.Count,
                WinRate = won.Count + lost.Count > 0 ? (double)won.Count / (won.Count + lost.Count) * 100 : 0,
                PipelineValue = opportunities.Where(o => o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost).Sum(o => o.Amount),
                QuotaAttainment = 0 // Would need quota tracking
            });
        }

        return performances.OrderByDescending(p => p.Revenue);
    }

    #endregion

    #region Hierarchy

    public async Task<IEnumerable<Team>> GetChildTeamsAsync(int parentTeamId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Manager)
            .Where(t => t.ParentTeamId == parentTeamId && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetParentTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team?.ParentTeamId == null) return null;

        return await GetByIdAsync(team.ParentTeamId.Value, cancellationToken);
    }

    public async Task<Team> SetParentTeamAsync(int teamId, int? parentTeamId, CancellationToken cancellationToken = default)
    {
        var team = await GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        // Prevent circular references
        if (parentTeamId.HasValue)
        {
            var parent = await GetByIdAsync(parentTeamId.Value, cancellationToken);
            var current = parent;
            while (current != null)
            {
                if (current.Id == teamId)
                {
                    throw new InvalidOperationException("Cannot create circular team hierarchy");
                }
                current = current.ParentTeamId.HasValue
                    ? await GetByIdAsync(current.ParentTeamId.Value, cancellationToken)
                    : null;
            }
        }

        team.ParentTeamId = parentTeamId;
        team.UpdatedAt = DateTime.UtcNow;

        _context.Teams.Update(team);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Set parent team {ParentTeamId} for team {TeamId}", parentTeamId, teamId);
        return team;
    }

    public async Task<TeamHierarchy> GetHierarchyAsync(int? rootTeamId = null, CancellationToken cancellationToken = default)
    {
        if (rootTeamId.HasValue)
        {
            var rootTeam = await GetByIdAsync(rootTeamId.Value, cancellationToken);
            if (rootTeam == null)
            {
                throw new InvalidOperationException($"Team {rootTeamId} not found");
            }

            return await BuildHierarchyNodeAsync(rootTeam, cancellationToken);
        }

        // Build from all root teams (no parent)
        var rootTeams = await _context.Teams
            .Where(t => t.ParentTeamId == null && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!rootTeams.Any())
        {
            return new TeamHierarchy
            {
                TeamId = 0,
                TeamName = "No Teams",
                Children = new List<TeamHierarchy>()
            };
        }

        // Return first root or wrap in a virtual root
        var firstRoot = rootTeams.First();
        var hierarchy = await BuildHierarchyNodeAsync(firstRoot, cancellationToken);

        // Add other root teams as siblings
        foreach (var otherRoot in rootTeams.Skip(1))
        {
            hierarchy.Children.Add(await BuildHierarchyNodeAsync(otherRoot, cancellationToken));
        }

        return hierarchy;
    }

    private async Task<TeamHierarchy> BuildHierarchyNodeAsync(Team team, CancellationToken cancellationToken)
    {
        var members = await GetMembersAsync(team.Id, cancellationToken);
        var childTeams = await GetChildTeamsAsync(team.Id, cancellationToken);

        var node = new TeamHierarchy
        {
            TeamId = team.Id,
            TeamName = team.Name,
            ManagerId = team.ManagerId,
            ManagerName = team.Manager != null ? $"{team.Manager.FirstName} {team.Manager.LastName}" : null,
            MemberCount = members.Count(),
            Children = new List<TeamHierarchy>()
        };

        foreach (var child in childTeams)
        {
            node.Children.Add(await BuildHierarchyNodeAsync(child, cancellationToken));
        }

        return node;
    }

    #endregion
}
