// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for subscription usage record retrieval.
/// AP-022: extracted from SubscriptionUsageController to eliminate fat-controller inline DB queries.
/// </summary>
public interface ISubscriptionUsageService
{
    /// <summary>
    /// Retrieve raw usage records for a subscription within the given date range,
    /// ordered by usage date descending.
    /// </summary>
    /// <param name="subscriptionId">The subscription to query.</param>
    /// <param name="start">Period start (inclusive).</param>
    /// <param name="end">Period end (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All non-deleted usage records in the period.</returns>
    Task<List<SubscriptionUsage>> GetUsageRecordsAsync(
        int subscriptionId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);
}
