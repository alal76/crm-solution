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

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing sales forecasts
/// </summary>
public interface ISalesForecastService
{
    /// <summary>
    /// Get all forecasts with optional filtering
    /// </summary>
    Task<IEnumerable<SalesForecast>> GetAllAsync(
        int? userId = null,
        int? teamId = null,
        int? fiscalYear = null,
        bool? isSubmitted = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a forecast by ID
    /// </summary>
    Task<SalesForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new forecast
    /// </summary>
    Task<SalesForecast> CreateAsync(SalesForecast forecast, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing forecast
    /// </summary>
    Task<bool> UpdateAsync(int id, SalesForecast forecast, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a forecast (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a forecast for review
    /// </summary>
    Task<bool> SubmitAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the forecast history (snapshots) for a specific period/user
    /// </summary>
    Task<IEnumerable<ForecastHistory>> GetHistoryAsync(
        string period,
        int? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a snapshot of the current forecast for historical tracking
    /// </summary>
    Task<ForecastHistory> CreateSnapshotAsync(int forecastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get forecast line items for a forecast
    /// </summary>
    Task<IEnumerable<ForecastLineItem>> GetLineItemsAsync(int forecastId, CancellationToken cancellationToken = default);
}
