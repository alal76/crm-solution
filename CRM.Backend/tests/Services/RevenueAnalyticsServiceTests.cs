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
/// Unit tests for RevenueAnalyticsService — covers MRR/ARR calculation,
/// snapshot creation, growth rate, churn rate, trend, NRR, and edge cases.
/// </summary>
public class RevenueAnalyticsServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<RevenueAnalyticsService>> _mockLogger;
    private readonly RevenueAnalyticsService _service;

    public RevenueAnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"RevenueAnalyticsTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<RevenueAnalyticsService>>();
        _service = new RevenueAnalyticsService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<RevenueSnapshot> SeedSnapshotAsync(
        decimal mrr,
        DateTime? snapshotDate = null,
        int customerCount = 10,
        int churnedCustomers = 0,
        decimal expansionMrr = 0,
        decimal contractionMrr = 0,
        decimal churnMrr = 0,
        decimal newMrr = 0)
    {
        var snapshot = new RevenueSnapshot
        {
            SnapshotDate = snapshotDate ?? DateTime.UtcNow.Date,
            MRR = mrr,
            ARR = mrr * 12,
            NewMRR = newMrr,
            ExpansionMRR = expansionMrr,
            ContractionMRR = contractionMrr,
            ChurnMRR = churnMrr,
            NetNewMRR = newMrr + expansionMrr - contractionMrr - churnMrr,
            CustomerCount = customerCount,
            NewCustomers = 0,
            ChurnedCustomers = churnedCustomers,
            SnapshotType = "Monthly",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
        _dbContext.RevenueSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync();
        return snapshot;
    }

    // ── GetCurrentMRR ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentMRRAsync_ShouldReturnZero_WhenNoSnapshotsAndNoSubscriptionsExist()
    {
        // Arrange: empty db

        // Act
        var mrr = await _service.GetCurrentMRRAsync();

        // Assert
        mrr.Should().Be(0m);
    }

    [Fact]
    public async Task GetCurrentMRRAsync_ShouldReturnLatestSnapshotMRR_WhenSnapshotsExist()
    {
        // Arrange
        await SeedSnapshotAsync(1000m, DateTime.UtcNow.AddMonths(-2));
        await SeedSnapshotAsync(1500m, DateTime.UtcNow.AddMonths(-1));
        await SeedSnapshotAsync(2000m, DateTime.UtcNow); // latest

        // Act
        var mrr = await _service.GetCurrentMRRAsync();

        // Assert
        mrr.Should().Be(2000m);
    }

    // ── GetCurrentARR ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentARRAsync_ShouldReturnMRRTimestwelve()
    {
        // Arrange
        await SeedSnapshotAsync(5000m, DateTime.UtcNow);

        // Act
        var arr = await _service.GetCurrentARRAsync();

        // Assert
        arr.Should().Be(60000m); // 5000 * 12
    }

    // ── CreateSnapshot ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSnapshotAsync_ShouldPersistAllFields_WhenDtoIsValid()
    {
        // Arrange
        var dto = new CreateRevenueSnapshotDto
        {
            SnapshotDate = new DateTime(2026, 1, 31),
            MRR = 12000m,
            NewMRR = 1500m,
            ExpansionMRR = 500m,
            ContractionMRR = 200m,
            ChurnMRR = 300m,
            CustomerCount = 120,
            NewCustomers = 8,
            ChurnedCustomers = 3,
            Notes = "January snapshot",
            SnapshotType = "Monthly"
        };

        // Act
        var result = await _service.CreateSnapshotAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.MRR.Should().Be(12000m);
        result.ARR.Should().Be(144000m); // 12000 * 12
        result.NewMRR.Should().Be(1500m);
        result.ExpansionMRR.Should().Be(500m);
        result.ContractionMRR.Should().Be(200m);
        result.ChurnMRR.Should().Be(300m);
        result.NetNewMRR.Should().Be(1500m); // 1500 + 500 - 200 - 300
        result.CustomerCount.Should().Be(120);
        result.Notes.Should().Be("January snapshot");

        // Verify persisted
        var count = await _dbContext.RevenueSnapshots.CountAsync();
        count.Should().Be(1);
    }

    // ── GetMetrics — MoM Growth ───────────────────────────────────────────────

    [Fact]
    public async Task GetMetricsAsync_ShouldCalculateMoMGrowthRate_WhenTwoMonthlySnapshotsExist()
    {
        // Arrange: previous MRR=1000, current MRR=1200 → growth = (1200-1000)/1000*100 = 20%
        await SeedSnapshotAsync(1000m, DateTime.UtcNow.AddMonths(-1), customerCount: 10);
        await SeedSnapshotAsync(1200m, DateTime.UtcNow, customerCount: 12);

        // Act
        var metrics = await _service.GetMetricsAsync(null, null);

        // Assert
        metrics.CurrentMRR.Should().Be(1200m);
        metrics.MoMGrowthRate.Should().Be(20m);
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldReturnZeroGrowthRate_WhenOnlyOneSnapshotExists()
    {
        // Arrange: single snapshot, no previous
        await SeedSnapshotAsync(5000m, DateTime.UtcNow);

        // Act
        var metrics = await _service.GetMetricsAsync(null, null);

        // Assert — no previous, so MoM growth should be 0 (handle divide by zero)
        metrics.MoMGrowthRate.Should().Be(0m);
    }

    // ── GetChurnRate ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetChurnRateAsync_ShouldCalculateCorrectly_WhenSnapshotsExist()
    {
        // Arrange: previous=100 customers, 5 churned → 5%
        await SeedSnapshotAsync(10000m, DateTime.UtcNow.AddMonths(-1), customerCount: 100, churnedCustomers: 5);
        await SeedSnapshotAsync(9000m, DateTime.UtcNow, customerCount: 95, churnedCustomers: 0);

        // Act
        var churnRate = await _service.GetChurnRateAsync(null, null);

        // Assert: total churned=5 / first snapshot customerCount=100 = 5%
        churnRate.Should().Be(5m);
    }

    [Fact]
    public async Task GetChurnRateAsync_ShouldReturnZero_WhenOnlyOneSnapshotExists()
    {
        // Arrange
        await SeedSnapshotAsync(10000m, DateTime.UtcNow, customerCount: 50);

        // Act
        var churnRate = await _service.GetChurnRateAsync(null, null);

        // Assert
        churnRate.Should().Be(0m);
    }

    // ── GetTrend ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTrendAsync_ShouldReturnSnapshotsSortedAscending_BySnapshotDate()
    {
        // Arrange: seed 3 snapshots in reverse order
        await SeedSnapshotAsync(3000m, DateTime.UtcNow.AddMonths(-1));
        await SeedSnapshotAsync(4000m, DateTime.UtcNow);
        await SeedSnapshotAsync(2000m, DateTime.UtcNow.AddMonths(-2));

        // Act
        var trend = (await _service.GetTrendAsync(12)).ToList();

        // Assert: should be sorted ascending
        trend.Should().HaveCount(3);
        trend[0].MRR.Should().Be(2000m);
        trend[1].MRR.Should().Be(3000m);
        trend[2].MRR.Should().Be(4000m);
    }

    // ── NRR Calculation ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetMetricsAsync_ShouldCalculateNRRCorrectly()
    {
        // Arrange: MRR=1000, Expansion=200, Contraction=50, Churn=100
        // NRR = (1000 + 200 - 50 - 100) / 1000 * 100 = 105%
        await SeedSnapshotAsync(
            mrr: 1000m,
            snapshotDate: DateTime.UtcNow,
            expansionMrr: 200m,
            contractionMrr: 50m,
            churnMrr: 100m,
            customerCount: 20);

        // Act
        var metrics = await _service.GetMetricsAsync(null, null);

        // Assert
        metrics.NetRevenueRetention.Should().Be(105m);
    }

    // ── CalculateCurrentSnapshot ──────────────────────────────────────────────

    [Fact]
    public async Task CalculateCurrentSnapshotAsync_ShouldCreateSnapshot_WithZeroMRRWhenNoSubscriptionsExist()
    {
        // Arrange: no subscriptions in DB

        // Act
        var snapshot = await _service.CalculateCurrentSnapshotAsync();

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.MRR.Should().Be(0m);
        snapshot.ARR.Should().Be(0m);
        snapshot.SnapshotType.Should().Be("Monthly");

        var dbCount = await _dbContext.RevenueSnapshots.CountAsync();
        dbCount.Should().Be(1);
    }
}
