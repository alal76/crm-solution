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
/// Service interface for managing ITSM change types
/// </summary>
public interface IChangeTypeService
{
    /// <summary>Creates a new change type</summary>
    /// <param name="dto">The change type data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The created change type</returns>
    Task<ChangeTypeDto> CreateAsync(CreateChangeTypeDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing change type</summary>
    /// <param name="id">The change type ID</param>
    /// <param name="dto">The update data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The updated change type</returns>
    Task<ChangeTypeDto> UpdateAsync(int id, UpdateChangeTypeDto dto, CancellationToken ct = default);

    /// <summary>Gets a change type by ID</summary>
    /// <param name="id">The change type ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The change type or null if not found</returns>
    Task<ChangeTypeDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Gets all change types</summary>
    /// <param name="includeInactive">Whether to include inactive change types</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of change types</returns>
    Task<List<ChangeTypeDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>Deletes a change type (soft delete)</summary>
    /// <param name="id">The change type ID</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Gets a change type by name</summary>
    /// <param name="typeName">The change type name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The change type or null if not found</returns>
    Task<ChangeTypeDto?> GetByNameAsync(string typeName, CancellationToken ct = default);
}
