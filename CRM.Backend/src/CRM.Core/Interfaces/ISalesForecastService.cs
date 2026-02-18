// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
