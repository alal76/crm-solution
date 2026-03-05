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
/// Service interface for lead operations used by API controllers and other services.
/// </summary>
public interface ILeadService
{
    Task<(IEnumerable<LeadSummaryDto> Items, int TotalCount, int Page, int PageSize, int TotalPages)> GetAllAsync(int page = 1, int pageSize = 25);
    Task<LeadDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(int id, Action<Lead> applyChanges);
    Task<bool> DeleteAsync(int id);
    Task<(int OpportunityId, int LeadId)> ConvertAsync(int id, string? opportunityName, int? accountId, decimal? estimatedValue, DateTime? expectedCloseDate);
    Task<IEnumerable<LeadSummaryDto>> GetByStatusAsync(LeadLifecycleStatus status);
    Task<object> GetStatsAsync();
    // Search and assignment helpers
    Task<IEnumerable<LeadSummaryDto>> SearchAsync(string searchTerm);
    Task<bool> AssignOwnerAsync(int leadId, int? ownerId);

    /// <summary>
    /// Pre-flight duplicate check by email OR (firstName + lastName + company).
    /// Soft-deleted leads are not considered.
    /// TODO-CRM002-05
    /// </summary>
    /// <returns>(isDuplicate, existingLeadId, matchedOn) where matchedOn is "email" or "name"</returns>
    Task<(bool IsDuplicate, int? ExistingLeadId, string? MatchedOn)> CheckDuplicateAsync(
        string? email,
        string? firstName,
        string? lastName,
        string? company,
        CancellationToken ct = default);

    /// <summary>
    /// Assigns a lead to a nurture campaign (TODO-CRM002-06).
    /// Sets the NurtureCampaignId field on the lead.
    /// </summary>
    /// <param name="leadId">The lead to assign.</param>
    /// <param name="campaignId">The nurture campaign to assign to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> AssignToNurtureCampaignAsync(int leadId, int campaignId, CancellationToken ct = default);

    /// <summary>
    /// Returns the nurture campaign a lead is currently enrolled in (TODO-CRM002-06).
    /// Returns null when no campaign is assigned.
    /// </summary>
    Task<CRM.Core.Entities.MarketingCampaign?> GetNurtureCampaignAsync(int leadId, CancellationToken ct = default);

    /// <summary>
    /// Removes a lead from a nurture campaign (TODO-CRM002-06).
    /// Clears NurtureCampaignId when it matches campaignId.
    /// </summary>
    /// <returns>True when cleared, false when the lead does not exist or is enrolled in a different campaign.</returns>
    Task<bool> RemoveFromNurtureCampaignAsync(int leadId, int campaignId, CancellationToken ct = default);

    /// <summary>
    /// Gets lead counts grouped by source channel with conversion rates (TODO-CRM002-03).
    /// </summary>
    Task<IEnumerable<LeadSourceAnalyticsDto>> GetSourceAnalyticsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets UTM campaign attribution breakdown grouped by source/medium/campaign (TODO-CRM002-03).
    /// </summary>
    Task<IEnumerable<LeadAttributionDto>> GetAttributionAnalyticsAsync(CancellationToken ct = default);
}
