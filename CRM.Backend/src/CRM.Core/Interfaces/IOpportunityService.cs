// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs;
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
    // In future: switch to DTOs for all contracts
}
