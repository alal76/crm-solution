// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Territory Assignment Service Interface (TODO-GAP-04)
/// Assigns leads to users based on territory rules.
/// </summary>
public interface ITerritoryAssignmentService
{
    /// <summary>
    /// Assigns a lead to a user based on territory rules.
    /// </summary>
    /// <param name="lead">Lead to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User ID assigned, or null if no match</returns>
    Task<int?> AssignLeadToTerritoryAsync(Lead lead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the territory matching a lead based on lead attributes.
    /// </summary>
    /// <param name="lead">Lead to match</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching territory or null</returns>
    Task<Territory?> GetMatchingTerritoryAsync(Lead lead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active territories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<Territory>> GetActiveTerritoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new territory.
    /// </summary>
    /// <param name="territory">Territory to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> CreateTerritoryAsync(Territory territory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing territory.
    /// </summary>
    /// <param name="territory">Territory to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> UpdateTerritoryAsync(Territory territory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets territory by ID.
    /// </summary>
    /// <param name="id">Territory ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Territory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a territory.
    /// </summary>
    /// <param name="id">Territory ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> DeleteTerritoryAsync(int id, CancellationToken cancellationToken = default);
}
