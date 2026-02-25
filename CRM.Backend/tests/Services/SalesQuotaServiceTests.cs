// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SalesQuotaService using InMemory database.
/// Tests CRUD, user/team filtering, year filtering, and attainment updates.
/// </summary>
public class SalesQuotaServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<SalesQuotaService>> _mockLogger;
    private readonly SalesQuotaService _service;

    public SalesQuotaServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"SalesQuotaServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<SalesQuotaService>>();
        _service = new SalesQuotaService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helpers

    private SalesQuota CreateTestQuota(
        string name = "Q1 Quota",
        int fiscalYear = 2026,
        QuotaPeriodType periodType = QuotaPeriodType.Quarterly,
        int? userId = null,
        int? teamId = null,
        decimal targetAmount = 100000m,
        decimal actualAmount = 0m)
    {
        return new SalesQuota
        {
            Name = name,
            Period = "Q1",
            FiscalYear = fiscalYear,
            PeriodType = periodType,
            Metric = QuotaMetric.Revenue,
            PeriodStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PeriodEndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
            TargetAmount = targetAmount,
            ActualAmount = actualAmount,
            CurrencyCode = "USD",
            UserId = userId,
            TeamId = teamId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<SalesQuota> SeedQuotaAsync(
        string name = "Seeded Quota",
        int fiscalYear = 2026,
        QuotaPeriodType periodType = QuotaPeriodType.Quarterly,
        int? userId = null,
        int? teamId = null,
        decimal targetAmount = 100000m,
        decimal actualAmount = 0m,
        bool isDeleted = false)
    {
        var quota = CreateTestQuota(name, fiscalYear, periodType, userId, teamId, targetAmount, actualAmount);
        quota.IsDeleted = isDeleted;
        _dbContext.SalesQuotas.Add(quota);
        await _dbContext.SaveChangesAsync();
        return quota;
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateQuota_WhenValid()
    {
        var quota = CreateTestQuota("New Quota");
        var result = await _service.CreateAsync(quota);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Quota");
    }

    [Fact]
    public async Task CreateAsync_ShouldSetTimestamps()
    {
        var quota = CreateTestQuota();
        var result = await _service.CreateAsync(quota);

        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistInDatabase()
    {
        var quota = CreateTestQuota("Persist Test");
        await _service.CreateAsync(quota);

        var dbQuota = await _dbContext.SalesQuotas.FirstOrDefaultAsync(q => q.Name == "Persist Test");
        dbQuota.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnQuota_WhenExists()
    {
        var seeded = await SeedQuotaAsync("Find Me");
        var result = await _service.GetByIdAsync(seeded.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        var seeded = await SeedQuotaAsync("Deleted", isDeleted: true);
        var result = await _service.GetByIdAsync(seeded.Id);
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeleted()
    {
        await SeedQuotaAsync("Q1");
        await SeedQuotaAsync("Q2");
        await SeedQuotaAsync("Deleted", isDeleted: true);

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByUserId()
    {
        await SeedQuotaAsync("User 1", userId: 1);
        await SeedQuotaAsync("User 2", userId: 2);

        var result = await _service.GetAllAsync(userId: 1);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByTeamId()
    {
        await SeedQuotaAsync("Team 1", teamId: 10);
        await SeedQuotaAsync("Team 2", teamId: 20);

        var result = await _service.GetAllAsync(teamId: 10);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByFiscalYear()
    {
        await SeedQuotaAsync("2025", fiscalYear: 2025);
        await SeedQuotaAsync("2026", fiscalYear: 2026);

        var result = await _service.GetAllAsync(fiscalYear: 2026);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByPeriodType()
    {
        await SeedQuotaAsync("Quarterly", periodType: QuotaPeriodType.Quarterly);
        await SeedQuotaAsync("Monthly", periodType: QuotaPeriodType.Monthly);

        var result = await _service.GetAllAsync(periodType: QuotaPeriodType.Quarterly);
        result.Should().HaveCount(1);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedQuotaAsync("Original");
        var update = CreateTestQuota("Updated");

        var result = await _service.UpdateAsync(seeded.Id, update);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProperties()
    {
        var seeded = await SeedQuotaAsync("Original", targetAmount: 100000m);
        var update = CreateTestQuota("Updated", targetAmount: 150000m);

        await _service.UpdateAsync(seeded.Id, update);

        var updated = await _dbContext.SalesQuotas.FindAsync(seeded.Id);
        updated!.Name.Should().Be("Updated");
        updated.TargetAmount.Should().Be(150000m);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNotFound()
    {
        var update = CreateTestQuota("Update");
        var result = await _service.UpdateAsync(999, update);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenDeleted()
    {
        var seeded = await SeedQuotaAsync("Deleted", isDeleted: true);
        var update = CreateTestQuota("Update");

        var result = await _service.UpdateAsync(seeded.Id, update);
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedQuotaAsync("Delete Me");
        var result = await _service.DeleteAsync(seeded.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var seeded = await SeedQuotaAsync("Soft Delete");
        await _service.DeleteAsync(seeded.Id);

        var deleted = await _dbContext.SalesQuotas.FindAsync(seeded.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }

    #endregion

    #region GetByUserAndYearAsync Tests

    [Fact]
    public async Task GetByUserAndYearAsync_ShouldReturnMatchingQuotas()
    {
        await SeedQuotaAsync("U1 2026", userId: 1, fiscalYear: 2026);
        await SeedQuotaAsync("U1 2025", userId: 1, fiscalYear: 2025);
        await SeedQuotaAsync("U2 2026", userId: 2, fiscalYear: 2026);

        var result = await _service.GetByUserAndYearAsync(1, 2026);
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("U1 2026");
    }

    [Fact]
    public async Task GetByUserAndYearAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await SeedQuotaAsync("U1", userId: 1, fiscalYear: 2026);

        var result = await _service.GetByUserAndYearAsync(999, 2026);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByTeamAndYearAsync Tests

    [Fact]
    public async Task GetByTeamAndYearAsync_ShouldReturnMatchingQuotas()
    {
        await SeedQuotaAsync("T10 2026", teamId: 10, fiscalYear: 2026);
        await SeedQuotaAsync("T10 2025", teamId: 10, fiscalYear: 2025);
        await SeedQuotaAsync("T20 2026", teamId: 20, fiscalYear: 2026);

        var result = await _service.GetByTeamAndYearAsync(10, 2026);
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("T10 2026");
    }

    #endregion

    #region UpdateAttainmentAsync Tests

    [Fact]
    public async Task UpdateAttainmentAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedQuotaAsync("Update Attainment");
        var result = await _service.UpdateAttainmentAsync(seeded.Id, 75000m);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAttainmentAsync_ShouldUpdateActualAmount()
    {
        var seeded = await SeedQuotaAsync("Update Attainment", actualAmount: 0m);
        await _service.UpdateAttainmentAsync(seeded.Id, 75000m);

        var updated = await _dbContext.SalesQuotas.FindAsync(seeded.Id);
        updated!.ActualAmount.Should().Be(75000m);
    }

    [Fact]
    public async Task UpdateAttainmentAsync_ShouldSetLastRefreshedAt()
    {
        var seeded = await SeedQuotaAsync("Refresh Tests");
        await _service.UpdateAttainmentAsync(seeded.Id, 50000m);

        var updated = await _dbContext.SalesQuotas.FindAsync(seeded.Id);
        updated!.LastRefreshedAt.Should().NotBeNull();
        updated.LastRefreshedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAttainmentAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.UpdateAttainmentAsync(999, 50000m);
        result.Should().BeFalse();
    }

    #endregion

    #region Computed Properties Tests (entity-level)

    [Fact]
    public void SalesQuota_AttainmentPercent_ShouldCalculateCorrectly()
    {
        var quota = new SalesQuota { TargetAmount = 100000m, ActualAmount = 75000m };
        quota.AttainmentPercent.Should().Be(75m);
    }

    [Fact]
    public void SalesQuota_AttainmentPercent_ShouldReturnZero_WhenTargetIsZero()
    {
        var quota = new SalesQuota { TargetAmount = 0m, ActualAmount = 50000m };
        quota.AttainmentPercent.Should().Be(0m);
    }

    [Fact]
    public void SalesQuota_Variance_ShouldCalculateCorrectly()
    {
        var quota = new SalesQuota { TargetAmount = 100000m, ActualAmount = 120000m };
        quota.Variance.Should().Be(20000m);
    }

    [Fact]
    public void SalesQuota_GapToTarget_ShouldCalculateCorrectly()
    {
        var quota = new SalesQuota { TargetAmount = 100000m, ActualAmount = 60000m };
        quota.GapToTarget.Should().Be(40000m);
    }

    [Fact]
    public void SalesQuota_GapToTarget_ShouldBeZero_WhenExceeded()
    {
        var quota = new SalesQuota { TargetAmount = 100000m, ActualAmount = 120000m };
        quota.GapToTarget.Should().Be(0m);
    }

    [Fact]
    public void SalesQuota_IsAchieved_ShouldBeTrue_WhenActualMeetsTarget()
    {
        var quota = new SalesQuota { TargetAmount = 100000m, ActualAmount = 100000m };
        quota.IsAchieved.Should().BeTrue();
    }

    [Fact]
    public void SalesQuota_IsAchieved_ShouldBeFalse_WhenBelowTarget()
    {
        var quota = new SalesQuota { TargetAmount = 100000m, ActualAmount = 50000m };
        quota.IsAchieved.Should().BeFalse();
    }

    #endregion

    #region SalesForecast Computed Properties Tests

    [Fact]
    public void SalesForecast_ForecastAmount_ShouldSumClosedAndCommit()
    {
        var forecast = new SalesForecast { ClosedWonAmount = 25000m, CommitAmount = 30000m };
        forecast.ForecastAmount.Should().Be(55000m);
    }

    [Fact]
    public void SalesForecast_GapToQuota_ShouldCalculateCorrectly()
    {
        var forecast = new SalesForecast { QuotaAmount = 100000m, ClosedWonAmount = 25000m, CommitAmount = 30000m };
        forecast.GapToQuota.Should().Be(45000m);
    }

    [Fact]
    public void SalesForecast_GapToQuota_ShouldBeZero_WhenExceeded()
    {
        var forecast = new SalesForecast { QuotaAmount = 50000m, ClosedWonAmount = 30000m, CommitAmount = 30000m };
        forecast.GapToQuota.Should().Be(0m);
    }

    [Fact]
    public void SalesForecast_ForecastAttainmentPercent_ShouldCalculate()
    {
        var forecast = new SalesForecast { QuotaAmount = 100000m, ClosedWonAmount = 40000m, CommitAmount = 35000m };
        forecast.ForecastAttainmentPercent.Should().Be(75m);
    }

    [Fact]
    public void SalesForecast_ForecastAttainmentPercent_ShouldBeZero_WhenNoQuota()
    {
        var forecast = new SalesForecast { QuotaAmount = 0m, ClosedWonAmount = 50000m };
        forecast.ForecastAttainmentPercent.Should().Be(0m);
    }

    #endregion
}
