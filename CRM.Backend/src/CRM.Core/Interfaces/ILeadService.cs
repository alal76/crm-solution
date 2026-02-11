// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for lead operations used by API controllers and other services.
/// </summary>
public interface ILeadService
{
    Task<(IEnumerable<object> Items, int TotalCount, int Page, int PageSize, int TotalPages)> GetAllAsync(int page = 1, int pageSize = 25);
    Task<object?> GetByIdAsync(int id);
    Task<int> CreateAsync(Lead lead);
    Task<bool> UpdateAsync(int id, Action<Lead> applyChanges);
    Task<bool> DeleteAsync(int id);
    Task<(int OpportunityId, int LeadId)> ConvertAsync(int id, string? opportunityName, int? accountId, decimal? estimatedValue, DateTime? expectedCloseDate);
    Task<IEnumerable<object>> GetByStatusAsync(LeadLifecycleStatus status);
    Task<object> GetStatsAsync();
    // Search and assignment helpers
    Task<IEnumerable<object>> SearchAsync(string searchTerm);
    Task<bool> AssignOwnerAsync(int leadId, int ownerId);
}
