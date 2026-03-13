// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
/// Unit tests for SalesForecastService.
/// Uses EF InMemory database to seed SalesForecast and ForecastHistory data.
/// </summary>
public class SalesForecastServiceTests
{
    private static CrmDbContext CreateDb() =>
        new CrmDbContext(
            new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase($"SalesForecast_{Guid.NewGuid()}")
                .Options,
            null!);

    private static SalesForecastService CreateService(CrmDbContext db) =>
        new SalesForecastService(db, new Mock<ILogger<SalesForecastService>>().Object);

    private static SalesForecast BuildForecast(int id = 1, int userId = 42, bool isDeleted = false) =>
        new SalesForecast
        {
            Id = id,
            Name = $"Q1 {id} Forecast",
            Period = "2026-Q1",
            FiscalYear = 2026,
            FiscalQuarter = 1,
            PeriodStartDate = new DateTime(2026, 1, 1),
            PeriodEndDate = new DateTime(2026, 3, 31),
            UserId = userId,
            QuotaAmount = 100000m,
            ClosedWonAmount = 20000m,
            CommitAmount = 40000m,
            BestCaseAmount = 60000m,
            PipelineAmount = 80000m,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SnapshotDate = DateTime.UtcNow
        };

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedForecasts()
    {
        using var db = CreateDb();
        db.SalesForecasts.AddRange(BuildForecast(1), BuildForecast(2), BuildForecast(3, isDeleted: true));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByUserId()
    {
        using var db = CreateDb();
        db.SalesForecasts.AddRange(BuildForecast(1, userId: 10), BuildForecast(2, userId: 20));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAllAsync(userId: 10);

        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByFiscalYear()
    {
        using var db = CreateDb();
        var f1 = BuildForecast(1); f1.FiscalYear = 2025;
        var f2 = BuildForecast(2); f2.FiscalYear = 2026;
        db.SalesForecasts.AddRange(f1, f2);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAllAsync(fiscalYear: 2026);

        result.Should().HaveCount(1);
        result.First().FiscalYear.Should().Be(2026);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnForecast_WhenIdExists()
    {
        using var db = CreateDb();
        db.SalesForecasts.Add(BuildForecast(5));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldPersistForecastAndSetTimestamps()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var forecast = new SalesForecast
        {
            Name = "New Forecast",
            Period = "2026-Q2",
            FiscalYear = 2026,
            FiscalQuarter = 2,
            PeriodStartDate = new DateTime(2026, 4, 1),
            PeriodEndDate = new DateTime(2026, 6, 30),
            UserId = 7
        };

        var result = await service.CreateAsync(forecast);

        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        db.SalesForecasts.Count().Should().Be(1);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenForecastExists()
    {
        using var db = CreateDb();
        db.SalesForecasts.Add(BuildForecast(1));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var updated = BuildForecast(1);
        updated.Name = "Updated Forecast";

        var result = await service.UpdateAsync(1, updated);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenForecastNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.UpdateAsync(999, BuildForecast(999));

        result.Should().BeFalse();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenForecastExists()
    {
        using var db = CreateDb();
        db.SalesForecasts.Add(BuildForecast(1));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.DeleteAsync(1);

        result.Should().BeTrue();
        db.SalesForecasts.IgnoreQueryFilters().First().IsDeleted.Should().BeTrue();
    }

    // ── SubmitAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitAsync_ShouldMarkForecastAsSubmitted_WhenForecastExists()
    {
        using var db = CreateDb();
        db.SalesForecasts.Add(BuildForecast(1));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.SubmitAsync(1);

        result.Should().BeTrue();
        var forecast = db.SalesForecasts.First();
        forecast.IsSubmitted.Should().BeTrue();
        forecast.SubmittedAt.Should().NotBeNull();
    }

    // ── CreateSnapshotAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateSnapshotAsync_ShouldThrow_WhenForecastNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateSnapshotAsync(9999));
    }

    [Fact]
    public async Task CreateSnapshotAsync_ShouldPersistHistory_WhenForecastExists()
    {
        using var db = CreateDb();
        db.SalesForecasts.Add(BuildForecast(1));
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var snapshot = await service.CreateSnapshotAsync(1);

        snapshot.Period.Should().Be("2026-Q1");
        snapshot.QuotaAmount.Should().Be(100000m);
        db.ForecastHistories.Count().Should().Be(1);
    }
}
