// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace CRM.Infrastructure.Services;

using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing sales territories and account assignments.
/// </summary>
public class TerritoryService : ITerritoryService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<TerritoryService> _logger;

    public TerritoryService(ICrmDbContext context, ILogger<TerritoryService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Territory CRUD

    public async Task<IEnumerable<AccountTerritory>> GetAllTerritoriesAsync(
        bool? isActive = null,
        int? teamId = null,
        int? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AccountTerritories
            .Include(t => t.PrimaryOwner)
            .Include(t => t.Team)
            .Where(t => !t.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        if (teamId.HasValue)
            query = query.Where(t => t.TeamId == teamId.Value);

        if (ownerId.HasValue)
            query = query.Where(t => t.PrimaryOwnerId == ownerId.Value);

        return await query
            .OrderBy(t => t.TerritoryName)
            .ToListAsync(cancellationToken);
    }

    public async Task<AccountTerritory?> GetTerritoryByIdAsync(int territoryId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTerritories
            .Include(t => t.PrimaryOwner)
            .Include(t => t.Team)
            .Include(t => t.AccountAssignments)
            .ThenInclude(a => a.Account)
            .FirstOrDefaultAsync(t => t.Id == territoryId && !t.IsDeleted, cancellationToken);
    }

    public async Task<AccountTerritory?> GetTerritoryByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTerritories
            .Include(t => t.PrimaryOwner)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.TerritoryCode == code && !t.IsDeleted, cancellationToken);
    }

    public async Task<AccountTerritory> CreateTerritoryAsync(AccountTerritory territory, CancellationToken cancellationToken = default)
    {
        territory.CreatedAt = DateTime.UtcNow;
        _context.AccountTerritories.Add(territory);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created territory {TerritoryId}: {TerritoryName}", territory.Id, territory.TerritoryName);
        return territory;
    }

    public async Task<AccountTerritory> UpdateTerritoryAsync(AccountTerritory territory, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AccountTerritories
            .FirstOrDefaultAsync(t => t.Id == territory.Id && !t.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Territory {territory.Id} not found");

        existing.TerritoryName = territory.TerritoryName;
        existing.TerritoryCode = territory.TerritoryCode;
        existing.Description = territory.Description;
        existing.Countries = territory.Countries;
        existing.Regions = territory.Regions;
        existing.States = territory.States;
        existing.Cities = territory.Cities;
        existing.Industries = territory.Industries;
        existing.CustomerTypes = territory.CustomerTypes;
        existing.RevenueRangeMin = territory.RevenueRangeMin;
        existing.RevenueRangeMax = territory.RevenueRangeMax;
        existing.PrimaryOwnerId = territory.PrimaryOwnerId;
        existing.TeamMemberIds = territory.TeamMemberIds;
        existing.AnnualQuota = territory.AnnualQuota;
        existing.QuotaCurrency = territory.QuotaCurrency;
        existing.TargetAccountCount = territory.TargetAccountCount;
        existing.IsActive = territory.IsActive;
        existing.TeamId = territory.TeamId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated territory {TerritoryId}: {TerritoryName}", territory.Id, territory.TerritoryName);
        return existing;
    }

    public async Task<bool> DeleteTerritoryAsync(int territoryId, CancellationToken cancellationToken = default)
    {
        var territory = await _context.AccountTerritories
            .FirstOrDefaultAsync(t => t.Id == territoryId && !t.IsDeleted, cancellationToken);

        if (territory == null)
            return false;

        territory.IsDeleted = true;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted territory {TerritoryId}: {TerritoryName}", territoryId, territory.TerritoryName);
        return true;
    }

    public async Task<AccountTerritory> ActivateTerritoryAsync(int territoryId, CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        territory.IsActive = true;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    public async Task<AccountTerritory> DeactivateTerritoryAsync(int territoryId, CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        territory.IsActive = false;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    #endregion

    #region Territory Assignment

    public async Task<AccountTerritoryAssignment> AssignAccountAsync(
        int accountId,
        int territoryId,
        int? assignedById = null,
        bool isPrimary = true,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        // Check if assignment already exists
        var existing = await _context.AccountTerritoryAssignments
            .FirstOrDefaultAsync(a => a.AccountId == accountId && a.TerritoryId == territoryId, cancellationToken);

        if (existing != null)
        {
            existing.IsPrimary = isPrimary;
            existing.Notes = notes;
            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        // If setting as primary, clear other primary flags
        if (isPrimary)
        {
            var currentPrimaries = await _context.AccountTerritoryAssignments
                .Where(a => a.AccountId == accountId && a.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var primary in currentPrimaries)
            {
                primary.IsPrimary = false;
            }
        }

        var assignment = new AccountTerritoryAssignment
        {
            AccountId = accountId,
            TerritoryId = territoryId,
            AssignedBy = assignedById,
            IsPrimary = isPrimary,
            Notes = notes,
            AssignedDate = DateTime.UtcNow
        };

        _context.AccountTerritoryAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned account {AccountId} to territory {TerritoryId}", accountId, territoryId);
        return assignment;
    }

    public async Task<bool> UnassignAccountAsync(int accountId, int territoryId, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.AccountTerritoryAssignments
            .FirstOrDefaultAsync(a => a.AccountId == accountId && a.TerritoryId == territoryId, cancellationToken);

        if (assignment == null)
            return false;

        _context.AccountTerritoryAssignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Unassigned account {AccountId} from territory {TerritoryId}", accountId, territoryId);
        return true;
    }

    public async Task<IEnumerable<AccountTerritoryAssignment>> GetAccountAssignmentsAsync(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AccountTerritoryAssignments
            .Include(a => a.Territory)
            .Include(a => a.AssignedByUser)
            .Where(a => a.AccountId == accountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetTerritoryAccountsAsync(int territoryId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTerritoryAssignments
            .Where(a => a.TerritoryId == territoryId)
            .Include(a => a.Account)
            .Select(a => a.Account!)
            .Where(a => !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<AccountTerritoryAssignment> SetPrimaryTerritoryAsync(
        int accountId,
        int territoryId,
        CancellationToken cancellationToken = default)
    {
        // Clear existing primary
        var currentPrimaries = await _context.AccountTerritoryAssignments
            .Where(a => a.AccountId == accountId && a.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var primary in currentPrimaries)
        {
            primary.IsPrimary = false;
        }

        // Set new primary
        var assignment = await _context.AccountTerritoryAssignments
            .FirstOrDefaultAsync(a => a.AccountId == accountId && a.TerritoryId == territoryId, cancellationToken);

        if (assignment == null)
        {
            assignment = new AccountTerritoryAssignment
            {
                AccountId = accountId,
                TerritoryId = territoryId,
                IsPrimary = true,
                AssignedDate = DateTime.UtcNow
            };
            _context.AccountTerritoryAssignments.Add(assignment);
        }
        else
        {
            assignment.IsPrimary = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<int> BulkAssignAccountsAsync(
        IEnumerable<int> accountIds,
        int territoryId,
        int? assignedById = null,
        CancellationToken cancellationToken = default)
    {
        int count = 0;
        foreach (var accountId in accountIds)
        {
            await AssignAccountAsync(accountId, territoryId, assignedById, false, null, cancellationToken);
            count++;
        }
        return count;
    }

    public async Task<int> TransferAccountsAsync(
        int fromTerritoryId,
        int toTerritoryId,
        IEnumerable<int>? accountIds = null,
        int? transferredById = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AccountTerritoryAssignments
            .Where(a => a.TerritoryId == fromTerritoryId);

        if (accountIds != null && accountIds.Any())
            query = query.Where(a => accountIds.Contains(a.AccountId));

        var assignments = await query.ToListAsync(cancellationToken);
        int count = 0;

        foreach (var assignment in assignments)
        {
            // Remove from old territory
            _context.AccountTerritoryAssignments.Remove(assignment);

            // Add to new territory
            await AssignAccountAsync(
                assignment.AccountId,
                toTerritoryId,
                transferredById,
                assignment.IsPrimary,
                $"Transferred from territory {fromTerritoryId}",
                cancellationToken);

            count++;
        }

        _logger.LogInformation("Transferred {Count} accounts from territory {From} to territory {To}",
            count, fromTerritoryId, toTerritoryId);

        return count;
    }

    #endregion

    #region Territory Ownership

    public async Task<AccountTerritory> SetTerritoryOwnerAsync(
        int territoryId,
        int ownerId,
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        territory.PrimaryOwnerId = ownerId;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    public async Task<AccountTerritory> AddTeamMembersAsync(
        int territoryId,
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        var existingIds = string.IsNullOrEmpty(territory.TeamMemberIds)
            ? new List<int>()
            : JsonSerializer.Deserialize<List<int>>(territory.TeamMemberIds) ?? new List<int>();

        foreach (var userId in userIds)
        {
            if (!existingIds.Contains(userId))
                existingIds.Add(userId);
        }

        territory.TeamMemberIds = JsonSerializer.Serialize(existingIds);
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    public async Task<AccountTerritory> RemoveTeamMembersAsync(
        int territoryId,
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        if (string.IsNullOrEmpty(territory.TeamMemberIds))
            return territory;

        var existingIds = JsonSerializer.Deserialize<List<int>>(territory.TeamMemberIds) ?? new List<int>();
        existingIds.RemoveAll(id => userIds.Contains(id));

        territory.TeamMemberIds = JsonSerializer.Serialize(existingIds);
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    public async Task<IEnumerable<AccountTerritory>> GetUserTerritoriesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var territories = await _context.AccountTerritories
            .Include(t => t.PrimaryOwner)
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        return territories.Where(t =>
            t.PrimaryOwnerId == userId ||
            (!string.IsNullOrEmpty(t.TeamMemberIds) &&
             JsonSerializer.Deserialize<List<int>>(t.TeamMemberIds)?.Contains(userId) == true));
    }

    public async Task<AccountTerritory> LinkToTeamAsync(
        int territoryId,
        int teamId,
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        territory.TeamId = teamId;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    #endregion

    #region Territory Matching

    public async Task<IEnumerable<AccountTerritory>> FindMatchingTerritoriesAsync(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        if (account == null)
            return Enumerable.Empty<AccountTerritory>();

        var criteria = new TerritoryMatchCriteria
        {
            Country = account.Country,
            Region = account.Region,
            State = account.State,
            City = account.City,
            Industry = account.Industry,
            CustomerType = account.AccountType.ToString(),
            AnnualRevenue = account.AnnualRevenue
        };

        return await FindMatchingTerritoriesAsync(criteria, cancellationToken);
    }

    public async Task<IEnumerable<AccountTerritory>> FindMatchingTerritoriesAsync(
        TerritoryMatchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var territories = await _context.AccountTerritories
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        var matching = new List<AccountTerritory>();

        foreach (var territory in territories)
        {
            if (TerritoryMatchesCriteria(territory, criteria))
            {
                matching.Add(territory);
            }
        }

        return matching;
    }

    public async Task<AccountTerritoryAssignment?> AutoAssignAccountAsync(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        var matchingTerritories = await FindMatchingTerritoriesAsync(accountId, cancellationToken);
        var bestMatch = matchingTerritories.FirstOrDefault();

        if (bestMatch == null)
            return null;

        return await AssignAccountAsync(accountId, bestMatch.Id, null, true, "Auto-assigned", cancellationToken);
    }

    public async Task<bool> IsAccountInTerritoryAsync(
        int accountId,
        int territoryId,
        CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);

        if (account == null || territory == null)
            return false;

        var criteria = new TerritoryMatchCriteria
        {
            Country = account.Country,
            Region = account.Region,
            State = account.State,
            City = account.City,
            Industry = account.Industry,
            CustomerType = account.AccountType.ToString(),
            AnnualRevenue = account.AnnualRevenue
        };

        return TerritoryMatchesCriteria(territory, criteria);
    }

    #endregion

    #region Quota Management

    public async Task<AccountTerritory> SetQuotaAsync(
        int territoryId,
        decimal quota,
        string currency = "USD",
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        territory.AnnualQuota = quota;
        territory.QuotaCurrency = currency;
        territory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return territory;
    }

    public async Task<TerritoryQuotaStatus> GetQuotaStatusAsync(
        int territoryId,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        var date = asOfDate ?? DateTime.UtcNow;
        var startOfYear = new DateTime(date.Year, 1, 1);

        // Get accounts in territory
        var accountIds = await _context.AccountTerritoryAssignments
            .Where(a => a.TerritoryId == territoryId)
            .Select(a => a.AccountId)
            .ToListAsync(cancellationToken);

        // Calculate achieved revenue from won opportunities
        var achieved = await _context.Opportunities
            .Where(o => accountIds.Contains(o.AccountId) &&
                       o.Stage == OpportunityStage.ClosedWon &&
                       o.ExpectedCloseDate >= startOfYear &&
                       o.ExpectedCloseDate <= date)
            .SumAsync(o => o.Amount, cancellationToken);

        return new TerritoryQuotaStatus
        {
            TerritoryId = territory.Id,
            TerritoryName = territory.TerritoryName,
            Quota = territory.AnnualQuota ?? 0,
            QuotaCurrency = territory.QuotaCurrency,
            Achieved = achieved,
            AsOfDate = date,
            TargetAccountCount = territory.TargetAccountCount,
            CurrentAccountCount = accountIds.Count
        };
    }

    public async Task<IEnumerable<TerritoryQuotaStatus>> GetAllQuotaStatusesAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var territories = await _context.AccountTerritories
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        var statuses = new List<TerritoryQuotaStatus>();
        foreach (var territory in territories)
        {
            var status = await GetQuotaStatusAsync(territory.Id, asOfDate, cancellationToken);
            statuses.Add(status);
        }

        return statuses.OrderByDescending(s => s.AttainmentPercentage);
    }

    #endregion

    #region Statistics & Analytics

    public async Task<TerritoryStatistics> GetTerritoryStatisticsAsync(
        int territoryId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var territory = await GetTerritoryByIdAsync(territoryId, cancellationToken);
        if (territory == null)
            throw new InvalidOperationException($"Territory {territoryId} not found");

        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var accountIds = await _context.AccountTerritoryAssignments
            .Where(a => a.TerritoryId == territoryId)
            .Select(a => a.AccountId)
            .ToListAsync(cancellationToken);

        var accounts = await _context.Accounts
            .Where(a => accountIds.Contains(a.Id) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var opportunities = await _context.Opportunities
            .Where(o => accountIds.Contains(o.AccountId) &&
                       o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var newAccounts = accounts.Count(a => a.CreatedAt >= from && a.CreatedAt <= to);
        var wonOpps = opportunities.Count(o => o.Stage == OpportunityStage.ClosedWon);
        var lostOpps = opportunities.Count(o => o.Stage == OpportunityStage.ClosedLost);
        var revenue = opportunities.Where(o => o.Stage == OpportunityStage.ClosedWon).Sum(o => o.Amount);
        var pipeline = opportunities.Where(o => o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost).Sum(o => o.Amount);

        var quotaStatus = await GetQuotaStatusAsync(territoryId, to, cancellationToken);

        return new TerritoryStatistics
        {
            TerritoryId = territory.Id,
            TerritoryName = territory.TerritoryName,
            TotalAccounts = accounts.Count,
            ActiveAccounts = accounts.Count(a => a.LifecycleStage == AccountLifecycleStage.Active),
            NewAccountsInPeriod = newAccounts,
            TotalOpportunities = opportunities.Count,
            WonOpportunities = wonOpps,
            LostOpportunities = lostOpps,
            TotalRevenue = revenue,
            PipelineValue = pipeline,
            AverageAccountValue = accounts.Count > 0 ? accounts.Average(a => a.AnnualRevenue) : 0,
            QuotaAttainment = (decimal)quotaStatus.AttainmentPercentage,
            FromDate = from,
            ToDate = to
        };
    }

    public async Task<IEnumerable<TerritoryRanking>> GetTerritoryRankingsAsync(
        int topN = 10,
        TerritoryRankingMetric metric = TerritoryRankingMetric.Revenue,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var territories = await _context.AccountTerritories
            .Include(t => t.PrimaryOwner)
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        var rankings = new List<TerritoryRanking>();

        foreach (var territory in territories)
        {
            var stats = await GetTerritoryStatisticsAsync(territory.Id, fromDate, toDate, cancellationToken);
            var quotaStatus = await GetQuotaStatusAsync(territory.Id, toDate, cancellationToken);

            var value = metric switch
            {
                TerritoryRankingMetric.Revenue => stats.TotalRevenue,
                TerritoryRankingMetric.OpportunitiesWon => stats.WonOpportunities,
                TerritoryRankingMetric.NewAccounts => stats.NewAccountsInPeriod,
                TerritoryRankingMetric.PipelineValue => stats.PipelineValue,
                TerritoryRankingMetric.QuotaAttainment => stats.QuotaAttainment,
                _ => stats.TotalRevenue
            };

            rankings.Add(new TerritoryRanking
            {
                TerritoryId = territory.Id,
                TerritoryName = territory.TerritoryName,
                OwnerId = territory.PrimaryOwnerId,
                OwnerName = territory.PrimaryOwner?.Username,
                MetricValue = value,
                QuotaAttainment = quotaStatus.AttainmentPercentage,
                AccountCount = stats.TotalAccounts
            });
        }

        return rankings
            .OrderByDescending(r => r.MetricValue)
            .Take(topN)
            .Select((r, i) => { r.Rank = i + 1; return r; });
    }

    public async Task<TerritoryDistribution> GetAccountDistributionAsync(CancellationToken cancellationToken = default)
    {
        var totalAccounts = await _context.Accounts.CountAsync(a => !a.IsDeleted, cancellationToken);
        var assignedAccountIds = await _context.AccountTerritoryAssignments
            .Select(a => a.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var territories = await _context.AccountTerritories
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        var breakdown = new List<TerritoryAccountCount>();
        foreach (var territory in territories)
        {
            var count = await _context.AccountTerritoryAssignments
                .CountAsync(a => a.TerritoryId == territory.Id, cancellationToken);

            breakdown.Add(new TerritoryAccountCount
            {
                TerritoryId = territory.Id,
                TerritoryName = territory.TerritoryName,
                AccountCount = count,
                Percentage = totalAccounts > 0 ? (double)count / totalAccounts * 100 : 0
            });
        }

        return new TerritoryDistribution
        {
            TotalAccounts = totalAccounts,
            AssignedAccounts = assignedAccountIds.Count,
            UnassignedAccounts = totalAccounts - assignedAccountIds.Count,
            TerritoryBreakdown = breakdown.OrderByDescending(b => b.AccountCount).ToList()
        };
    }

    public async Task<IEnumerable<UnassignedAccountsSummary>> GetUnassignedAccountsSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var territories = await _context.AccountTerritories
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        var assignedAccountIds = await _context.AccountTerritoryAssignments
            .Select(a => a.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var unassignedAccounts = await _context.Accounts
            .Where(a => !a.IsDeleted && !assignedAccountIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        var summaries = new List<UnassignedAccountsSummary>();

        foreach (var territory in territories)
        {
            var matchingUnassigned = unassignedAccounts
                .Where(a => TerritoryMatchesCriteria(territory, new TerritoryMatchCriteria
                {
                    Country = a.Country,
                    Region = a.Region,
                    State = a.State,
                    City = a.City,
                    Industry = a.Industry,
                    CustomerType = a.AccountType.ToString(),
                    AnnualRevenue = a.AnnualRevenue
                }))
                .ToList();

            if (matchingUnassigned.Any())
            {
                summaries.Add(new UnassignedAccountsSummary
                {
                    TerritoryId = territory.Id,
                    TerritoryName = territory.TerritoryName,
                    UnassignedCount = matchingUnassigned.Count,
                    AccountIds = matchingUnassigned.Select(a => a.Id).ToList()
                });
            }
        }

        return summaries.OrderByDescending(s => s.UnassignedCount);
    }

    #endregion

    #region Territory Search

    public async Task<IEnumerable<AccountTerritory>> SearchTerritoriesAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        return await _context.AccountTerritories
            .Include(t => t.PrimaryOwner)
            .Where(t => !t.IsDeleted &&
                       (t.TerritoryName.Contains(query) ||
                        (t.TerritoryCode != null && t.TerritoryCode.Contains(query)) ||
                        (t.Description != null && t.Description.Contains(query))))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccountTerritory>> GetTerritoriesByLocationAsync(
        string? country = null,
        string? region = null,
        string? state = null,
        string? city = null,
        CancellationToken cancellationToken = default)
    {
        var territories = await _context.AccountTerritories
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync(cancellationToken);

        return territories.Where(t =>
        {
            if (country != null && !ContainsInJsonArray(t.Countries, country))
                return false;
            if (region != null && !ContainsInJsonArray(t.Regions, region))
                return false;
            if (state != null && !ContainsInJsonArray(t.States, state))
                return false;
            if (city != null && !ContainsInJsonArray(t.Cities, city))
                return false;
            return true;
        });
    }

    #endregion

    #region Private Methods

    private bool TerritoryMatchesCriteria(AccountTerritory territory, TerritoryMatchCriteria criteria)
    {
        // Check country match
        if (!string.IsNullOrEmpty(criteria.Country) && !string.IsNullOrEmpty(territory.Countries))
        {
            if (!ContainsInJsonArray(territory.Countries, criteria.Country))
                return false;
        }

        // Check region match
        if (!string.IsNullOrEmpty(criteria.Region) && !string.IsNullOrEmpty(territory.Regions))
        {
            if (!ContainsInJsonArray(territory.Regions, criteria.Region))
                return false;
        }

        // Check state match
        if (!string.IsNullOrEmpty(criteria.State) && !string.IsNullOrEmpty(territory.States))
        {
            if (!ContainsInJsonArray(territory.States, criteria.State))
                return false;
        }

        // Check city match
        if (!string.IsNullOrEmpty(criteria.City) && !string.IsNullOrEmpty(territory.Cities))
        {
            if (!ContainsInJsonArray(territory.Cities, criteria.City))
                return false;
        }

        // Check industry match
        if (!string.IsNullOrEmpty(criteria.Industry) && !string.IsNullOrEmpty(territory.Industries))
        {
            if (!ContainsInJsonArray(territory.Industries, criteria.Industry))
                return false;
        }

        // Check customer type match
        if (!string.IsNullOrEmpty(criteria.CustomerType) && !string.IsNullOrEmpty(territory.CustomerTypes))
        {
            if (!ContainsInJsonArray(territory.CustomerTypes, criteria.CustomerType))
                return false;
        }

        // Check revenue range
        if (criteria.AnnualRevenue.HasValue)
        {
            if (territory.RevenueRangeMin.HasValue && criteria.AnnualRevenue < territory.RevenueRangeMin)
                return false;
            if (territory.RevenueRangeMax.HasValue && criteria.AnnualRevenue > territory.RevenueRangeMax)
                return false;
        }

        return true;
    }

    private bool ContainsInJsonArray(string? jsonArray, string value)
    {
        if (string.IsNullOrEmpty(jsonArray))
            return true; // Empty means "all" / no restriction

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(jsonArray);
            return items?.Any(i => string.Equals(i, value, StringComparison.OrdinalIgnoreCase)) == true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
