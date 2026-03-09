// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
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
}
