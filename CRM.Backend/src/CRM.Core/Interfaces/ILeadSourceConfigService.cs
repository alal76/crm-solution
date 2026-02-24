// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for lead source configuration management (TODO-CRM002-03)
/// </summary>
public interface ILeadSourceConfigService
{
    /// <summary>
    /// Get all lead sources
    /// </summary>
    Task<IEnumerable<LeadSourceConfig>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Get active lead sources only
    /// </summary>
    Task<IEnumerable<LeadSourceConfig>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get lead source by ID
    /// </summary>
    Task<LeadSourceConfig?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Get lead source by tracking code
    /// </summary>
    Task<LeadSourceConfig?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);

    /// <summary>
    /// Create a new lead source
    /// </summary>
    Task<LeadSourceConfig> CreateAsync(LeadSourceConfig source, CancellationToken ct = default);

    /// <summary>
    /// Update an existing lead source
    /// </summary>
    Task<LeadSourceConfig?> UpdateAsync(int id, LeadSourceConfig source, CancellationToken ct = default);

    /// <summary>
    /// Soft delete a lead source
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Get lead count by source
    /// </summary>
    Task<Dictionary<int, int>> GetLeadCountBySourceAsync(CancellationToken ct = default);

    /// <summary>
    /// Calculate ROI for a lead source based on conversions
    /// </summary>
    Task<decimal?> CalculateRoiAsync(int sourceId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
}
