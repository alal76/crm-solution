// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
