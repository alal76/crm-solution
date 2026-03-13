// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for RevenueAnalyticsService.
/// Uses EF InMemory database to seed RevenueSnapshot and Subscription data.
/// </summary>
public class RevenueAnalyticsServiceTests
{
    private static CrmDbContext CreateDb() =>
        new CrmDbContext(
            new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase($"RevenueAnalytics_{Guid.NewGuid()}")
                .Options,
            null!);

    private static RevenueAnalyticsService CreateService(CrmDbContext db) =>
        new RevenueAnalyticsService(db, new Mock<ILogger<RevenueAnalyticsService>>().Object);

    // ── GetCurrentMRRAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentMRRAsync_ShouldReturnMRRFromLatestSnapshot_WhenSnapshotExists()
    {
        using var db = CreateDb();
        db.RevenueSnapshots.AddRange(
            new RevenueSnapshot
            {
                SnapshotDate = DateTime.UtcNow.AddMonths(-2), MRR = 8000m,
                ARR = 96000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
            },
            new RevenueSnapshot
            {
                SnapshotDate = DateTime.UtcNow.AddMonths(-1), MRR = 10000m,
                ARR = 120000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
            }
        );
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var mrr = await service.GetCurrentMRRAsync();

        mrr.Should().Be(10000m);
    }

    [Fact]
    public async Task GetCurrentMRRAsync_ShouldFallbackToSubscriptions_WhenNoSnapshot()
    {
        using var db = CreateDb();
        db.Subscriptions.Add(new Subscription
        {
            AccountId = 1, SubscriptionStatus = SubscriptionStatus.Active,
            MRR = 3500m, StartDate = DateTime.UtcNow.AddMonths(-3),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var mrr = await service.GetCurrentMRRAsync();

        mrr.Should().Be(3500m);
    }

    // ── GetCurrentARRAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentARRAsync_ShouldReturnTwelveTimesMRR()
    {
        using var db = CreateDb();
        db.RevenueSnapshots.Add(new RevenueSnapshot
        {
            SnapshotDate = DateTime.UtcNow.AddDays(-1), MRR = 5000m,
            ARR = 60000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var arr = await service.GetCurrentARRAsync();

        arr.Should().Be(60000m); // 5000 × 12
    }

    // ── CreateSnapshotAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateSnapshotAsync_ShouldPersistSnapshotAndCalcNetNewMRR()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var dto = new CreateRevenueSnapshotDto
        {
            SnapshotDate = DateTime.UtcNow.Date,
            MRR = 12000m,
            NewMRR = 2000m,
            ExpansionMRR = 500m,
            ContractionMRR = 200m,
            ChurnMRR = 100m,
            CustomerCount = 50,
            NewCustomers = 5,
            ChurnedCustomers = 1,
            Notes = "March snapshot",
            SnapshotType = "Monthly"
        };

        var result = await service.CreateSnapshotAsync(dto);

        result.MRR.Should().Be(12000m);
        result.ARR.Should().Be(144000m); // 12000 × 12
        result.NetNewMRR.Should().Be(2200m);  // 2000 + 500 - 200 - 100
        result.CustomerCount.Should().Be(50);

        db.RevenueSnapshots.Count().Should().Be(1);
    }

    // ── GetTrendAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTrendAsync_ShouldReturnSnapshotsWithinMonthWindow()
    {
        using var db = CreateDb();
        db.RevenueSnapshots.AddRange(
            new RevenueSnapshot
            {
                SnapshotDate = DateTime.UtcNow.AddMonths(-2), MRR = 7000m,
                ARR = 84000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
            },
            new RevenueSnapshot
            {
                SnapshotDate = DateTime.UtcNow.AddMonths(-15), MRR = 5000m,
                ARR = 60000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
            }
        );
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var trend = await service.GetTrendAsync(12); // last 12 months

        trend.Should().HaveCount(1); // only the one within 12 months
        trend.First().MRR.Should().Be(7000m);
    }

    // ── GetChurnRateAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetChurnRateAsync_ShouldReturnZero_WhenFewerThanTwoSnapshots()
    {
        using var db = CreateDb();
        db.RevenueSnapshots.Add(new RevenueSnapshot
        {
            SnapshotDate = DateTime.UtcNow.AddMonths(-1), MRR = 10000m,
            ARR = 120000m, ChurnedCustomers = 2, CustomerCount = 100,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var rate = await service.GetChurnRateAsync(null, null);

        rate.Should().Be(0m);
    }

    // ── GetMRRMovementsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetMRRMovementsAsync_ShouldReturnOneMovementPerSnapshot()
    {
        using var db = CreateDb();
        db.RevenueSnapshots.AddRange(
            new RevenueSnapshot
            {
                SnapshotDate = DateTime.UtcNow.AddMonths(-2), MRR = 8000m, ARR = 96000m,
                NewMRR = 1000m, ExpansionMRR = 200m, ContractionMRR = 100m, ChurnMRR = 50m,
                NetNewMRR = 1050m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
            },
            new RevenueSnapshot
            {
                SnapshotDate = DateTime.UtcNow.AddMonths(-1), MRR = 9000m, ARR = 108000m,
                NewMRR = 1200m, ExpansionMRR = 300m, ContractionMRR = 150m, ChurnMRR = 60m,
                NetNewMRR = 1290m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, SnapshotType = "Monthly"
            }
        );
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var movements = (await service.GetMRRMovementsAsync(6)).ToList();

        movements.Should().HaveCount(2);
        movements[1].ClosingMRR.Should().Be(9000m);
    }
}
