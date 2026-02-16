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

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing change requests.
/// </summary>
public interface IChangeService
{
    /// <summary>
    /// Get all changes with pagination.
    /// </summary>
    Task<PaginatedDto<ChangeDto>> GetAllAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a change by ID.
    /// </summary>
    Task<ChangeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new change request.
    /// </summary>
    Task<ChangeDto> CreateAsync(CreateChangeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing change request.
    /// </summary>
    Task<ChangeDto> UpdateAsync(int id, UpdateChangeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a change request.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a change request for approval.
    /// </summary>
    Task<ChangeDto> SubmitAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a change request.
    /// </summary>
    Task<ChangeDto> ApproveAsync(int id, ChangeApprovalDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject a change request.
    /// </summary>
    Task<ChangeDto> RejectAsync(int id, ChangeRejectionDto dto, CancellationToken cancellationToken = default);
}
