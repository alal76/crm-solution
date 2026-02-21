// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing sales quotas
/// </summary>
public interface ISalesQuotaService
{
    /// <summary>
    /// Get all quotas with optional filtering
    /// </summary>
    Task<IEnumerable<SalesQuota>> GetAllAsync(
        int? userId = null,
        int? teamId = null,
        int? fiscalYear = null,
        QuotaPeriodType? periodType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a quota by ID
    /// </summary>
    Task<SalesQuota?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new quota
    /// </summary>
    Task<SalesQuota> CreateAsync(SalesQuota quota, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing quota
    /// </summary>
    Task<bool> UpdateAsync(int id, SalesQuota quota, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a quota (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get quotas for a specific user and fiscal year
    /// </summary>
    Task<IEnumerable<SalesQuota>> GetByUserAndYearAsync(int userId, int fiscalYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get quotas for a specific team and fiscal year
    /// </summary>
    Task<IEnumerable<SalesQuota>> GetByTeamAndYearAsync(int teamId, int fiscalYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the actual attainment amount for a quota
    /// </summary>
    Task<bool> UpdateAttainmentAsync(int id, decimal actualAmount, CancellationToken cancellationToken = default);
}
