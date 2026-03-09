// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Partner portal service: exposes deal pipeline and resource data to partner organisations.
/// PORTAL-025.
/// </summary>
public interface IPartnerPortalService
{
    /// <summary>Returns deals (opportunities) owned by the specified partner account.</summary>
    Task<IEnumerable<OpportunityDto>> GetPartnerDealsAsync(int partnerAccountId, CancellationToken ct = default);

    /// <summary>Returns all open opportunities that involve the partner account.</summary>
    Task<IEnumerable<OpportunityDto>> GetPartnerOpportunitiesAsync(int partnerAccountId, CancellationToken ct = default);

    /// <summary>Returns general partner resources (documents, guides, links).</summary>
    Task<IEnumerable<PartnerResourceDto>> GetResourcesAsync(CancellationToken ct = default);

    /// <summary>Registers a new deal on behalf of a partner.</summary>
    Task RegisterDealAsync(RegisterPartnerDealDto dto, CancellationToken ct = default);

    /// <summary>Returns dashboard summary for the given partner user. FLAG-002.</summary>
    Task<PartnerDashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default);

    /// <summary>Returns leads owned by the given partner user. FLAG-002.</summary>
    Task<IEnumerable<PartnerLeadDto>> GetLeadsAsync(int userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns commission records for the given partner user. FLAG-002.</summary>
    Task<IEnumerable<PartnerCommissionDto>> GetCommissionsAsync(int userId, CancellationToken ct = default);
}
