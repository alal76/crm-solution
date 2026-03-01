// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ENUM-BE-005: Service interface for configurable enum management
using CRM.Core.DTOs;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service contract for administering and consuming configurable enumerations.
/// All category/value CRUD is routed through this service, which enforces
/// cache invalidation and system-value protection rules.
/// </summary>
public interface IEnumManagementService
{
    // ─── Category operations ──────────────────────────────────────────────────

    /// <summary>Returns all non-deleted enum categories, ordered by name.</summary>
    Task<IEnumerable<EnumCategoryDto>> GetAllCategoriesAsync(CancellationToken ct = default);

    /// <summary>Returns a single category by its machine-readable name.</summary>
    Task<EnumCategoryDto?> GetCategoryByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Creates a new enum category (Admin only).</summary>
    Task<EnumCategoryDto> CreateCategoryAsync(CreateEnumCategoryDto dto, CancellationToken ct = default);

    /// <summary>Updates mutable fields on an existing category (Admin only).</summary>
    Task<EnumCategoryDto> UpdateCategoryAsync(int categoryId, UpdateEnumCategoryDto dto, CancellationToken ct = default);

    // ─── Value operations ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns all active values for the specified category, ordered by SortOrder.
    /// Results are cached with a 1-hour TTL.
    /// </summary>
    Task<IEnumerable<EnumValueDto>> GetValuesByCategoryNameAsync(string categoryName, CancellationToken ct = default);

    /// <summary>Returns all active values for the specified category by its Id.</summary>
    Task<IEnumerable<EnumValueDto>> GetValuesByCategoryIdAsync(int categoryId, CancellationToken ct = default);

    /// <summary>Creates a new value within the given category.</summary>
    Task<EnumValueDto> CreateValueAsync(int categoryId, CreateEnumValueDto dto, CancellationToken ct = default);

    /// <summary>Updates mutable fields on an existing value.</summary>
    Task<EnumValueDto> UpdateValueAsync(int valueId, UpdateEnumValueDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a value. Throws <see cref="InvalidOperationException"/> if the
    /// value is referenced by any entity record.
    /// </summary>
    Task DeleteValueAsync(int valueId, CancellationToken ct = default);

    /// <summary>
    /// Reorders the values in a category to match the supplied ordered id list.
    /// The server assigns SortOrder = index position.
    /// </summary>
    Task ReorderValuesAsync(int categoryId, IEnumerable<int> orderedIds, CancellationToken ct = default);

    // ─── Transition operations ────────────────────────────────────────────────

    /// <summary>Returns all transition rules for the given category.</summary>
    Task<IEnumerable<EnumTransitionDto>> GetTransitionsAsync(int categoryId, CancellationToken ct = default);

    /// <summary>Creates a new transition rule.</summary>
    Task<EnumTransitionDto> CreateTransitionAsync(int categoryId, CreateEnumTransitionDto dto, CancellationToken ct = default);

    /// <summary>Permanently deletes a transition rule.</summary>
    Task DeleteTransitionAsync(int transitionId, CancellationToken ct = default);

    // ─── Validation & transition-check ────────────────────────────────────────

    /// <summary>
    /// Checks whether a transition from <paramref name="fromValueId"/> to
    /// <paramref name="toValueId"/> is permitted.  If no explicit rule exists the
    /// default is <c>true</c> (permissive).
    /// </summary>
    Task<bool> IsTransitionAllowedAsync(string categoryName, int fromValueId, int toValueId, CancellationToken ct = default);

    /// <summary>Validates whether <paramref name="value"/> is a recognised key or label in the category.</summary>
    Task<EnumValidationResult> ValidateValueAsync(string categoryName, string value, CancellationToken ct = default);

    // ─── Cache management ─────────────────────────────────────────────────────

    /// <summary>
    /// Removes cached value list(s).  Pass <c>null</c> to invalidate all enum caches.
    /// </summary>
    Task InvalidateCacheAsync(string? categoryName = null, CancellationToken ct = default);
}
