// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Competitor Service Interface (TODO-CRM003-03)
/// Manages competitor data and opportunity-competitor relationships.
/// </summary>
public interface ICompetitorService
{
    /// <summary>
    /// Gets all competitors.
    /// </summary>
    /// <param name="includeInactive">Include inactive competitors</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<Competitor>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a competitor by ID.
    /// </summary>
    /// <param name="id">Competitor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Competitor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new competitor.
    /// </summary>
    /// <param name="competitor">Competitor to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> CreateAsync(Competitor competitor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing competitor.
    /// </summary>
    /// <param name="competitor">Competitor to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> UpdateAsync(Competitor competitor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a competitor (soft delete).
    /// </summary>
    /// <param name="id">Competitor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets competitors for an opportunity.
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<OpportunityCompetitor>> GetOpportunityCompetitorsAsync(int opportunityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a competitor to an opportunity.
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="competitorId">Competitor ID</param>
    /// <param name="isPrimary">Is primary competitor</param>
    /// <param name="threatLevel">Threat level</param>
    /// <param name="notes">Notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<OpportunityCompetitor> AddToOpportunityAsync(
        int opportunityId,
        int competitorId,
        bool isPrimary = false,
        string? threatLevel = null,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a competitor from an opportunity.
    /// </summary>
    /// <param name="opportunityId">Opportunity ID</param>
    /// <param name="competitorId">Competitor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> RemoveFromOpportunityAsync(int opportunityId, int competitorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches competitors by name.
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<Competitor>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
