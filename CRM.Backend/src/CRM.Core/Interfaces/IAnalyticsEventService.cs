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
/// Service interface for managing analytics events.
/// </summary>
public interface IAnalyticsEventService
{
    /// <summary>
    /// Creates a new analytics event.
    /// </summary>
    /// <param name="dto">The event creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created analytics event DTO.</returns>
    Task<AnalyticsEventDto> CreateAsync(CreateAnalyticsEventDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an analytics event by ID.
    /// </summary>
    /// <param name="id">The event ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The analytics event DTO, or null if not found.</returns>
    Task<AnalyticsEventDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets analytics events with optional filtering.
    /// </summary>
    /// <param name="filter">Filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching analytics event DTOs.</returns>
    Task<IEnumerable<AnalyticsEventDto>> GetAllAsync(AnalyticsEventFilterDto? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets analytics events for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="limit">Maximum number of events to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of analytics event DTOs.</returns>
    Task<IEnumerable<AnalyticsEventDto>> GetByEntityAsync(string entityType, int entityId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets analytics events for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="limit">Maximum number of events to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of analytics event DTOs.</returns>
    Task<IEnumerable<AnalyticsEventDto>> GetByUserAsync(int userId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an analytics event (soft delete).
    /// </summary>
    /// <param name="id">The event ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
