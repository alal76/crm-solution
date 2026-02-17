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
