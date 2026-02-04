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
/// Opportunity service interface
/// </summary>
public interface IOpportunityService
{
    Task<Opportunity?> GetOpportunityByIdAsync(int id);
    Task<IEnumerable<Opportunity>> GetOpportunitiesByCustomerAsync(int customerId);
    Task<IEnumerable<Opportunity>> GetOpenOpportunitiesAsync();
    Task<int> CreateOpportunityAsync(Opportunity opportunity);
    Task UpdateOpportunityAsync(Opportunity opportunity);
    Task DeleteOpportunityAsync(int id);
    Task<decimal> GetTotalPipelineAsync();
}
