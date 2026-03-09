// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for subscription usage record retrieval.
/// AP-022: extracted from SubscriptionUsageController.GetUsageRecords to eliminate fat-controller inline DB queries.
/// </summary>
public class SubscriptionUsageService : ISubscriptionUsageService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<SubscriptionUsageService> _logger;

    public SubscriptionUsageService(ICrmDbContext dbContext, ILogger<SubscriptionUsageService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<List<SubscriptionUsage>> GetUsageRecordsAsync(
        int subscriptionId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "GetUsageRecordsAsync: SubscriptionId={SubscriptionId}, {Start} → {End}",
            subscriptionId, start, end);

        var records = await _dbContext.SubscriptionUsages
            .Where(u => u.SubscriptionId == subscriptionId
                && !u.IsDeleted
                && u.UsageDate >= start
                && u.UsageDate <= end)
            .OrderByDescending(u => u.UsageDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "GetUsageRecordsAsync: {Count} records for subscription {SubscriptionId}",
            records.Count, subscriptionId);

        return records;
    }
}
