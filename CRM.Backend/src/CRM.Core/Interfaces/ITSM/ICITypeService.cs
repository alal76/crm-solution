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
/// Service interface for managing Configuration Item Types.
/// </summary>
public interface ICITypeService
{
    /// <summary>
    /// Creates a new CI Type.
    /// </summary>
    /// <param name="dto">The CI type creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created CI type.</returns>
    Task<CITypeDto> CreateAsync(CreateCITypeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a CI Type by ID.
    /// </summary>
    /// <param name="id">The CI type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The CI type if found; otherwise null.</returns>
    Task<CITypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all CI Types with optional filtering.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="activeOnly">If true, returns only active types.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of CI types.</returns>
    Task<IEnumerable<CITypeDto>> GetAllAsync(string? category = null, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing CI Type.
    /// </summary>
    /// <param name="id">The CI type ID.</param>
    /// <param name="dto">The update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated CI type if found; otherwise null.</returns>
    Task<CITypeDto?> UpdateAsync(int id, UpdateCITypeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a CI Type (soft delete).
    /// </summary>
    /// <param name="id">The CI type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted; false if not found.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct categories from existing CI Types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of distinct categories.</returns>
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
