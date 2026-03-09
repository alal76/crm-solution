// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Opportunity service interface
/// </summary>
public interface IOpportunityService
{
    Task<Opportunity?> GetOpportunityByIdAsync(int id);
    Task<IEnumerable<Opportunity>> GetOpportunitiesByAccountAsync(int accountId);

    /// <summary>
    /// Get opportunities by customer ID (alias for GetOpportunitiesByAccountAsync for backward compatibility)
    /// </summary>
    Task<List<Opportunity>> GetOpportunitiesByCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync();
    Task<int> CreateOpportunityAsync(Opportunity opportunity);
    Task UpdateOpportunityAsync(Opportunity opportunity);
    Task DeleteOpportunityAsync(int id);
    Task<decimal> GetTotalPipelineAsync();

    // --- Product management (TODO-CRM003-04) ---

    /// <summary>Gets all products attached to an opportunity.</summary>
    Task<IEnumerable<OpportunityProduct>> GetOpportunityProductsAsync(int opportunityId, CancellationToken ct = default);

    /// <summary>Adds a product to an opportunity and recalculates TotalValue.</summary>
    Task<OpportunityProduct> AddOpportunityProductAsync(int opportunityId, OpportunityProduct product, CancellationToken ct = default);

    /// <summary>Updates a product line item on an opportunity and recalculates TotalValue.</summary>
    Task<OpportunityProduct?> UpdateOpportunityProductAsync(int opportunityId, int productId, OpportunityProduct updated, CancellationToken ct = default);

    /// <summary>Removes a product from an opportunity and recalculates TotalValue.</summary>
    Task<bool> RemoveOpportunityProductAsync(int opportunityId, int productId, CancellationToken ct = default);

    // --- Opportunity Cloning (TODO-CRM003-06) ---

    /// <summary>
    /// Clones an opportunity with all its products and team members.
    /// </summary>
    /// <param name="opportunityId">The ID of the opportunity to clone.</param>
    /// <param name="options">Clone options specifying what to include.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created cloned opportunity.</returns>
    Task<Opportunity> CloneAsync(int opportunityId, OpportunityCloneOptions? options = null, CancellationToken ct = default);

    // --- Team Member Management (TODO-CRM003-08) ---

    /// <summary>Gets all team members for an opportunity.</summary>
    Task<IEnumerable<OpportunityTeamMember>> GetTeamMembersAsync(int opportunityId, CancellationToken ct = default);

    /// <summary>Adds a team member to an opportunity.</summary>
    Task<OpportunityTeamMember> AddTeamMemberAsync(int opportunityId, OpportunityTeamMember member, CancellationToken ct = default);

    /// <summary>Updates a team member on an opportunity.</summary>
    Task<OpportunityTeamMember?> UpdateTeamMemberAsync(int opportunityId, int memberId, OpportunityTeamMember updated, CancellationToken ct = default);

    /// <summary>Removes a team member from an opportunity.</summary>
    Task<bool> RemoveTeamMemberAsync(int opportunityId, int memberId, CancellationToken ct = default);

    // --- Competitor Management (TODO-CRM003-03) ---

    /// <summary>Gets all competitors associated with an opportunity.</summary>
    Task<IEnumerable<OpportunityCompetitor>> GetCompetitorsAsync(int opportunityId, CancellationToken ct = default);

    /// <summary>Adds a competitor to an opportunity.</summary>
    Task<OpportunityCompetitor> AddCompetitorAsync(int opportunityId, OpportunityCompetitor competitor, CancellationToken ct = default);

    /// <summary>Updates competitor details on an opportunity (TODO-CRM003-03).</summary>
    Task<OpportunityCompetitor?> UpdateCompetitorAsync(int opportunityId, int competitorId, OpportunityCompetitor updated, CancellationToken ct = default);

    /// <summary>Removes a competitor from an opportunity.</summary>
    Task<bool> RemoveCompetitorAsync(int opportunityId, int competitorId, CancellationToken ct = default);

    // --- Forecast Category (TODO-CRM003-07) ---

    /// <summary>Patches the forecast category of a single opportunity.</summary>
    Task<bool> PatchForecastCategoryAsync(int opportunityId, ForecastCategory category, CancellationToken ct = default);

    /// <summary>Returns a forecast summary grouped by forecast category bucket.</summary>
    Task<ForecastSummaryDto> GetForecastSummaryAsync(CancellationToken ct = default);

    // In future: switch to DTOs for all contracts
}

/// <summary>
/// Options for cloning an opportunity.
/// TODO-CRM003-06: Opportunity cloning
/// </summary>
public class OpportunityCloneOptions
{
    /// <summary>New name for the cloned opportunity. Defaults to "Copy of [original name]".</summary>
    public string? NewName { get; set; }

    /// <summary>Whether to clone products. Default is true.</summary>
    public bool CloneProducts { get; set; } = true;

    /// <summary>Whether to clone team members. Default is true.</summary>
    public bool CloneTeamMembers { get; set; } = true;

    /// <summary>Whether to clone competitors. Default is true.</summary>
    public bool CloneCompetitors { get; set; } = true;

    /// <summary>New account ID for the cloned opportunity. If null, uses the original.</summary>
    public int? NewAccountId { get; set; }

    /// <summary>New expected close date. If null, uses the original.</summary>
    public DateTime? NewExpectedCloseDate { get; set; }

    /// <summary>Reset stage to the first stage (Lead/Prospecting). Default is false.</summary>
    public bool ResetStage { get; set; } = false;
}
