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
    Task<bool> AssignOwnerAsync(int leadId, int ownerId);
}
