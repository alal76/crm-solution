// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace CRM.Core.Interfaces;

using CRM.Core.Entities;

/// <summary>
/// Service for managing sales territories and account assignments.
/// Territories define geographic and business rule boundaries for account ownership.
/// </summary>
public interface ITerritoryService
{
    #region Territory CRUD

    /// <summary>
    /// Gets all territories with optional filtering.
    /// </summary>
    Task<IEnumerable<AccountTerritory>> GetAllTerritoriesAsync(
        bool? isActive = null,
        int? teamId = null,
        int? ownerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a territory by ID.
    /// </summary>
    Task<AccountTerritory?> GetTerritoryByIdAsync(int territoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a territory by code.
    /// </summary>
    Task<AccountTerritory?> GetTerritoryByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new territory.
    /// </summary>
    Task<AccountTerritory> CreateTerritoryAsync(AccountTerritory territory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing territory.
    /// </summary>
    Task<AccountTerritory> UpdateTerritoryAsync(AccountTerritory territory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a territory.
    /// </summary>
    Task<bool> DeleteTerritoryAsync(int territoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a territory.
    /// </summary>
    Task<AccountTerritory> ActivateTerritoryAsync(int territoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a territory.
    /// </summary>
    Task<AccountTerritory> DeactivateTerritoryAsync(int territoryId, CancellationToken cancellationToken = default);

    #endregion

    #region Territory Assignment

    /// <summary>
    /// Assigns an account to a territory.
    /// </summary>
    Task<AccountTerritoryAssignment> AssignAccountAsync(
        int accountId,
        int territoryId,
        int? assignedById = null,
        bool isPrimary = true,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an account from a territory.
    /// </summary>
    Task<bool> UnassignAccountAsync(int accountId, int territoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all territory assignments for an account.
    /// </summary>
    Task<IEnumerable<AccountTerritoryAssignment>> GetAccountAssignmentsAsync(
        int accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts in a territory.
    /// </summary>
    Task<IEnumerable<Account>> GetTerritoryAccountsAsync(int territoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the primary territory for an account.
    /// </summary>
    Task<AccountTerritoryAssignment> SetPrimaryTerritoryAsync(
        int accountId,
        int territoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk assigns accounts to a territory.
    /// </summary>
    Task<int> BulkAssignAccountsAsync(
        IEnumerable<int> accountIds,
        int territoryId,
        int? assignedById = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfers accounts from one territory to another.
    /// </summary>
    Task<int> TransferAccountsAsync(
        int fromTerritoryId,
        int toTerritoryId,
        IEnumerable<int>? accountIds = null,
        int? transferredById = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Territory Ownership

    /// <summary>
    /// Sets the primary owner of a territory.
    /// </summary>
    Task<AccountTerritory> SetTerritoryOwnerAsync(
        int territoryId,
        int ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds team members to a territory.
    /// </summary>
    Task<AccountTerritory> AddTeamMembersAsync(
        int territoryId,
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes team members from a territory.
    /// </summary>
    Task<AccountTerritory> RemoveTeamMembersAsync(
        int territoryId,
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all territories for a user (as owner or team member).
    /// </summary>
    Task<IEnumerable<AccountTerritory>> GetUserTerritoriesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a territory to a team.
    /// </summary>
    Task<AccountTerritory> LinkToTeamAsync(
        int territoryId,
        int teamId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Territory Matching

    /// <summary>
    /// Finds matching territories for an account based on territory rules.
    /// </summary>
    Task<IEnumerable<AccountTerritory>> FindMatchingTerritoriesAsync(
        int accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds matching territories for account data (without requiring saved account).
    /// </summary>
    Task<IEnumerable<AccountTerritory>> FindMatchingTerritoriesAsync(
        TerritoryMatchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-assigns an account to the best matching territory.
    /// </summary>
    Task<AccountTerritoryAssignment?> AutoAssignAccountAsync(
        int accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an account matches a specific territory's rules.
    /// </summary>
    Task<bool> IsAccountInTerritoryAsync(
        int accountId,
        int territoryId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Quota Management

    /// <summary>
    /// Sets the quota for a territory.
    /// </summary>
    Task<AccountTerritory> SetQuotaAsync(
        int territoryId,
        decimal quota,
        string currency = "USD",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quota attainment for a territory.
    /// </summary>
    Task<TerritoryQuotaStatus> GetQuotaStatusAsync(
        int territoryId,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quota summary for all territories.
    /// </summary>
    Task<IEnumerable<TerritoryQuotaStatus>> GetAllQuotaStatusesAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Gets statistics for a territory.
    /// </summary>
    Task<TerritoryStatistics> GetTerritoryStatisticsAsync(
        int territoryId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets performance rankings across all territories.
    /// </summary>
    Task<IEnumerable<TerritoryRanking>> GetTerritoryRankingsAsync(
        int topN = 10,
        TerritoryRankingMetric metric = TerritoryRankingMetric.Revenue,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets account distribution across territories.
    /// </summary>
    Task<TerritoryDistribution> GetAccountDistributionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets territories with unassigned accounts (accounts matching territory rules but not assigned).
    /// </summary>
    Task<IEnumerable<UnassignedAccountsSummary>> GetUnassignedAccountsSummaryAsync(
        CancellationToken cancellationToken = default);

    #endregion

    #region Territory Search

    /// <summary>
    /// Searches territories by name, code, or description.
    /// </summary>
    Task<IEnumerable<AccountTerritory>> SearchTerritoriesAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets territories by geographic criteria.
    /// </summary>
    Task<IEnumerable<AccountTerritory>> GetTerritoriesByLocationAsync(
        string? country = null,
        string? region = null,
        string? state = null,
        string? city = null,
        CancellationToken cancellationToken = default);

    #endregion
}

#region Supporting Types

/// <summary>
/// Criteria for matching territories to accounts.
/// </summary>
public class TerritoryMatchCriteria
{
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Industry { get; set; }
    public string? CustomerType { get; set; }
    public decimal? AnnualRevenue { get; set; }
}

/// <summary>
/// Quota status for a territory.
/// </summary>
public class TerritoryQuotaStatus
{
    public int TerritoryId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public decimal Quota { get; set; }
    public string QuotaCurrency { get; set; } = "USD";
    public decimal Achieved { get; set; }
    public decimal Remaining => Quota - Achieved;
    public double AttainmentPercentage => Quota > 0 ? (double)(Achieved / Quota) * 100 : 0;
    public DateTime AsOfDate { get; set; }
    public int? TargetAccountCount { get; set; }
    public int CurrentAccountCount { get; set; }
}

/// <summary>
/// Statistics for a territory.
/// </summary>
public class TerritoryStatistics
{
    public int TerritoryId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public int TotalAccounts { get; set; }
    public int ActiveAccounts { get; set; }
    public int NewAccountsInPeriod { get; set; }
    public int TotalOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal AverageAccountValue { get; set; }
    public decimal QuotaAttainment { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Territory ranking for leaderboards.
/// </summary>
public class TerritoryRanking
{
    public int Rank { get; set; }
    public int TerritoryId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public decimal MetricValue { get; set; }
    public double QuotaAttainment { get; set; }
    public int AccountCount { get; set; }
}

/// <summary>
/// Metric for ranking territories.
/// </summary>
public enum TerritoryRankingMetric
{
    Revenue,
    OpportunitiesWon,
    NewAccounts,
    PipelineValue,
    QuotaAttainment
}

/// <summary>
/// Account distribution across territories.
/// </summary>
public class TerritoryDistribution
{
    public int TotalAccounts { get; set; }
    public int AssignedAccounts { get; set; }
    public int UnassignedAccounts { get; set; }
    public List<TerritoryAccountCount> TerritoryBreakdown { get; set; } = new();
}

/// <summary>
/// Account count for a territory.
/// </summary>
public class TerritoryAccountCount
{
    public int TerritoryId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public int AccountCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Summary of unassigned accounts for a territory.
/// </summary>
public class UnassignedAccountsSummary
{
    public int TerritoryId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;
    public int UnassignedCount { get; set; }
    public List<int> AccountIds { get; set; } = new();
}

#endregion
