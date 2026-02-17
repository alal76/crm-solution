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
/// Service interface for campaign conversion management.
/// Handles tracking and attributing conversions to marketing campaigns.
/// </summary>
public interface ICampaignConversionService
{
    /// <summary>
    /// Gets all conversions with optional filtering and pagination.
    /// </summary>
    /// <param name="filter">Optional filter string for conversion type or external IDs.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated result with conversions and metadata.</returns>
    Task<(List<CampaignConversionDto> Items, int TotalCount)> GetAllAsync(
        string? filter = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific conversion by ID.
    /// </summary>
    /// <param name="id">The conversion ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversion DTO if found, null otherwise.</returns>
    Task<CampaignConversionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all conversions for a specific campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of conversions for the campaign.</returns>
    Task<List<CampaignConversionDto>> GetByCampaignIdAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new conversion record.
    /// </summary>
    /// <param name="dto">The conversion data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created conversion DTO.</returns>
    Task<CampaignConversionDto> CreateAsync(CreateCampaignConversionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conversion record.
    /// </summary>
    /// <param name="id">The conversion ID to update.</param>
    /// <param name="dto">The updated conversion data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated conversion DTO if found, null otherwise.</returns>
    Task<CampaignConversionDto?> UpdateAsync(int id, UpdateCampaignConversionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a conversion record.
    /// </summary>
    /// <param name="id">The conversion ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
