// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Revenue Analytics service providing ARR/MRR metrics, trend data, and snapshot management.
/// </summary>
public class RevenueAnalyticsService : IRevenueAnalyticsService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<RevenueAnalyticsService> _logger;

    public RevenueAnalyticsService(ICrmDbContext dbContext, ILogger<RevenueAnalyticsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<decimal> GetCurrentMRRAsync(CancellationToken ct = default)
    {
        // First try from snapshots
        var latestSnapshot = await _dbContext.RevenueSnapshots
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync(ct);

        if (latestSnapshot != null)
        {
            return latestSnapshot.MRR;
        }

        // Fallback: calculate from active subscriptions
        var subscriptionMrr = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Active && s.MRR.HasValue)
            .SumAsync(s => s.MRR!.Value, ct);

        return subscriptionMrr;
    }

    /// <inheritdoc/>
    public async Task<decimal> GetCurrentARRAsync(CancellationToken ct = default)
    {
        var mrr = await GetCurrentMRRAsync(ct);
        return mrr * 12;
    }

    /// <inheritdoc/>
    public async Task<decimal> GetChurnRateAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = _dbContext.RevenueSnapshots.Where(s => !s.IsDeleted);

        if (from.HasValue)
        {
            query = query.Where(s => s.SnapshotDate >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(s => s.SnapshotDate <= to.Value);
        }

        var snapshots = await query.OrderBy(s => s.SnapshotDate).ToListAsync(ct);

        if (snapshots.Count < 2)
        {
            return 0m;
        }

        var totalChurned = snapshots.Sum(s => s.ChurnedCustomers);
        var previousCustomers = snapshots.First().CustomerCount;

        if (previousCustomers == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)totalChurned / previousCustomers * 100m, 2);
    }

    /// <inheritdoc/>
    public async Task<RevenueMetricsDto> GetMetricsAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = _dbContext.RevenueSnapshots.Where(s => !s.IsDeleted);

        if (from.HasValue)
        {
            query = query.Where(s => s.SnapshotDate >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(s => s.SnapshotDate <= to.Value);
        }

        var snapshots = await query.OrderBy(s => s.SnapshotDate).ToListAsync(ct);

        var currentMrr = await GetCurrentMRRAsync(ct);
        var currentArr = currentMrr * 12;

        var currentSnapshot = snapshots.LastOrDefault();
        var previousSnapshot = snapshots.Count >= 2 ? snapshots[^2] : null;

        // MoM growth rate
        decimal momGrowthRate = 0m;
        if (previousSnapshot?.MRR > 0)
        {
            momGrowthRate = Math.Round(((currentMrr - previousSnapshot.MRR) / previousSnapshot.MRR) * 100m, 2);
        }

        // Churn rate
        decimal churnRate = 0m;
        if (previousSnapshot?.CustomerCount > 0 && currentSnapshot != null)
        {
            churnRate = Math.Round((decimal)currentSnapshot.ChurnedCustomers / previousSnapshot.CustomerCount * 100m, 2);
        }

        // Expansion rate
        decimal expansionRate = 0m;
        if (previousSnapshot?.MRR > 0 && currentSnapshot != null)
        {
            expansionRate = Math.Round(currentSnapshot.ExpansionMRR / previousSnapshot.MRR * 100m, 2);
        }

        // Net Revenue Retention
        decimal nrr = 0m;
        if (currentSnapshot?.MRR > 0)
        {
            var baseMrr = currentSnapshot.MRR;
            nrr = Math.Round(
                (baseMrr + currentSnapshot.ExpansionMRR - currentSnapshot.ContractionMRR - currentSnapshot.ChurnMRR)
                / baseMrr * 100m, 2);
        }

        int totalCustomers = currentSnapshot?.CustomerCount ?? 0;

        decimal avgRevenuePerCustomer = 0m;
        if (totalCustomers > 0)
        {
            avgRevenuePerCustomer = Math.Round(currentMrr / totalCustomers, 2);
        }

        // Trend: last 12 months
        var trend = await GetTrendAsync(12, ct);

        return new RevenueMetricsDto
        {
            CurrentMRR = currentMrr,
            CurrentARR = currentArr,
            MoMGrowthRate = momGrowthRate,
            ChurnRate = churnRate,
            ExpansionRate = expansionRate,
            NetRevenueRetention = nrr,
            TotalCustomers = totalCustomers,
            AverageRevenuePerCustomer = avgRevenuePerCustomer,
            Trend = trend.ToList()
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RevenueSnapshotDto>> GetTrendAsync(int months, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);
        var snapshots = await _dbContext.RevenueSnapshots
            .Where(s => !s.IsDeleted && s.SnapshotDate >= cutoff)
            .OrderBy(s => s.SnapshotDate)
            .ToListAsync(ct);

        return snapshots.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RevenueMRRMovementDto>> GetMRRMovementsAsync(int months, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);
        var snapshots = await _dbContext.RevenueSnapshots
            .Where(s => !s.IsDeleted && s.SnapshotDate >= cutoff)
            .OrderBy(s => s.SnapshotDate)
            .ToListAsync(ct);

        var movements = new List<RevenueMRRMovementDto>();
        for (var i = 0; i < snapshots.Count; i++)
        {
            var current = snapshots[i];
            var prior = i > 0 ? snapshots[i - 1] : null;
            var openingMrr = prior?.MRR ?? (current.MRR - current.NetNewMRR);

            movements.Add(new RevenueMRRMovementDto
            {
                Period = current.SnapshotDate,
                Label = current.SnapshotDate.ToString("MMM yyyy"),
                OpeningMRR = openingMrr,
                NewMRR = current.NewMRR,
                ExpansionMRR = current.ExpansionMRR,
                ContractionMRR = current.ContractionMRR,
                ChurnMRR = current.ChurnMRR,
                ClosingMRR = current.MRR
            });
        }

        return movements;
    }

    /// <inheritdoc/>
    public async Task<RevenueSnapshotDto> CreateSnapshotAsync(CreateRevenueSnapshotDto dto, CancellationToken ct = default)
    {
        var snapshot = new RevenueSnapshot
        {
            SnapshotDate = dto.SnapshotDate,
            MRR = dto.MRR,
            ARR = dto.MRR * 12,
            NewMRR = dto.NewMRR,
            ExpansionMRR = dto.ExpansionMRR,
            ContractionMRR = dto.ContractionMRR,
            ChurnMRR = dto.ChurnMRR,
            NetNewMRR = dto.NewMRR + dto.ExpansionMRR - dto.ContractionMRR - dto.ChurnMRR,
            CustomerCount = dto.CustomerCount,
            NewCustomers = dto.NewCustomers,
            ChurnedCustomers = dto.ChurnedCustomers,
            Notes = dto.Notes,
            SnapshotType = dto.SnapshotType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.RevenueSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Revenue snapshot created for {Date} with MRR={MRR}", dto.SnapshotDate, dto.MRR);
        return MapToDto(snapshot);
    }

    /// <inheritdoc/>
    public async Task<RevenueSnapshotDto> CalculateCurrentSnapshotAsync(CancellationToken ct = default)
    {
        // Calculate MRR from active subscriptions
        var activeSubs = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Active)
            .ToListAsync(ct);

        var calculatedMrr = activeSubs.Where(s => s.MRR.HasValue).Sum(s => s.MRR!.Value);
        var customerCount = activeSubs.Select(s => s.AccountId).Distinct().Count();

        // Get last snapshot to calculate movements
        var lastSnapshot = await _dbContext.RevenueSnapshots
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync(ct);

        var previousMrr = lastSnapshot?.MRR ?? 0m;
        var netNewMrr = calculatedMrr - previousMrr;

        var snapshot = new RevenueSnapshot
        {
            SnapshotDate = DateTime.UtcNow.Date,
            MRR = calculatedMrr,
            ARR = calculatedMrr * 12,
            NewMRR = netNewMrr > 0 ? netNewMrr : 0m,
            ExpansionMRR = 0m,
            ContractionMRR = 0m,
            ChurnMRR = netNewMrr < 0 ? Math.Abs(netNewMrr) : 0m,
            NetNewMRR = netNewMrr,
            CustomerCount = customerCount,
            NewCustomers = 0,
            ChurnedCustomers = 0,
            Notes = "Auto-calculated from active subscriptions",
            SnapshotType = "Monthly",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.RevenueSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Revenue snapshot auto-calculated: MRR={MRR}, Customers={Count}", calculatedMrr, customerCount);
        return MapToDto(snapshot);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static RevenueSnapshotDto MapToDto(RevenueSnapshot s) => new()
    {
        Id = s.Id,
        SnapshotDate = s.SnapshotDate,
        MRR = s.MRR,
        ARR = s.ARR,
        NewMRR = s.NewMRR,
        ExpansionMRR = s.ExpansionMRR,
        ContractionMRR = s.ContractionMRR,
        ChurnMRR = s.ChurnMRR,
        NetNewMRR = s.NetNewMRR,
        CustomerCount = s.CustomerCount,
        NewCustomers = s.NewCustomers,
        ChurnedCustomers = s.ChurnedCustomers,
        Notes = s.Notes,
        SnapshotType = s.SnapshotType,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
