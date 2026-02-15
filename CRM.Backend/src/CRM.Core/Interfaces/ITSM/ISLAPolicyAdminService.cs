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

using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Admin service interface for managing SLA policies
/// </summary>
public interface ISLAPolicyAdminService
{
    /// <summary>Creates a new SLA policy</summary>
    Task<SLAPolicyDto> CreateAsync(CreateSLAPolicyDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing SLA policy</summary>
    Task<SLAPolicyDto> UpdateAsync(int id, UpdateSLAPolicyDto dto, CancellationToken ct = default);

    /// <summary>Gets an SLA policy by ID</summary>
    Task<SLAPolicyDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Gets all SLA policies</summary>
    Task<List<SLAPolicyDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Deletes an SLA policy (soft delete)</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Assigns an SLA policy to a service request</summary>
    Task<SLAInstanceDto> AssignPolicyAsync(int policyId, int serviceRequestId, CancellationToken ct = default);

    /// <summary>Gets applicable SLA policies for given criteria</summary>
    Task<List<SLAPolicyDto>> GetApplicablePoliciesAsync(
        string? priority,
        string? category,
        CancellationToken ct = default);
}
