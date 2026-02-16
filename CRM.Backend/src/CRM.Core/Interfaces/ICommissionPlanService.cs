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
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for commission plan management operations.
/// Handles creation, modification, and management of commission plans.
/// </summary>
public interface ICommissionPlanService
{
    #region CRUD Operations

    /// <summary>Gets all commission plans.</summary>
    Task<IEnumerable<CommissionPlanDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a commission plan by ID.</summary>
    Task<CommissionPlanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new commission plan.</summary>
    Task<CommissionPlanDto> CreateAsync(CreateCommissionPlanDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing commission plan.</summary>
    Task<CommissionPlanDto> UpdateAsync(int id, UpdateCommissionPlanDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes a commission plan (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Plan Management

    /// <summary>Activates a commission plan.</summary>
    Task<bool> ActivateAsync(int planId, CancellationToken cancellationToken = default);

    /// <summary>Deactivates a commission plan.</summary>
    Task<bool> DeactivateAsync(int planId, CancellationToken cancellationToken = default);

    /// <summary>Assigns a plan to a user."""
    Task<bool> AssignToUserAsync(int planId, int userId, DateTime? effectiveDate = null, CancellationToken cancellationToken = default);

    /// <summary>Removes a plan assignment from a user.</summary>
    Task<bool> RemoveFromUserAsync(int planId, int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets the active plan for a user.</summary>
    Task<CommissionPlanDto?> GetUserPlanAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets users assigned to a plan.</summary>
    Task<IEnumerable<UserDto>> GetPlanUsersAsync(int planId, CancellationToken cancellationToken = default);

    #endregion

    #region Tier Management

    /// <summary>Gets tiers for a commission plan.</summary>
    Task<IEnumerable<CommissionTierDto>> GetTiersAsync(int planId, CancellationToken cancellationToken = default);

    /// <summary>Adds a tier to a plan."""
    Task<CommissionTierDto> AddTierAsync(int planId, CreateCommissionTierDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates a tier.</summary>
    Task<CommissionTierDto> UpdateTierAsync(int tierId, UpdateCommissionTierDto dto, CancellationToken cancellationToken = default);

    /// <summary>Removes a tier from a plan.</summary>
    Task<bool> RemoveTierAsync(int tierId, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets active plans only.</summary>
    Task<IEnumerable<CommissionPlanDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Clones/duplicates a plan.</summary>
    Task<CommissionPlanDto> DuplicateAsync(int planId, string newName, CancellationToken cancellationToken = default);

    /// <summary>Gets commission history for a plan.</summary>
    Task<IEnumerable<CommissionDto>> GetCommissionHistoryAsync(int planId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    #endregion
}
