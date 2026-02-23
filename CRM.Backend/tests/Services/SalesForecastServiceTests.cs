// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
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
/// Unit tests for SalesForecastService using InMemory database.
/// Tests CRUD, submission, history snapshots, and line items.
/// </summary>
public class SalesForecastServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<SalesForecastService>> _mockLogger;
    private readonly SalesForecastService _service;

    public SalesForecastServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"SalesForecastServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<SalesForecastService>>();
        _service = new SalesForecastService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helpers

    private SalesForecast CreateTestForecast(
        string name = "Q1 2026 Forecast",
        int fiscalYear = 2026,
        int? userId = null,
        int? teamId = null,
        bool isSubmitted = false)
    {
        return new SalesForecast
        {
            Name = name,
            Period = "Q1",
            PeriodStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PeriodEndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
            FiscalYear = fiscalYear,
            FiscalQuarter = 1,
            QuotaAmount = 100000m,
            ClosedWonAmount = 25000m,
            CommitAmount = 30000m,
            BestCaseAmount = 20000m,
            PipelineAmount = 50000m,
            UserId = userId,
            TeamId = teamId,
            IsSubmitted = isSubmitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<SalesForecast> SeedForecastAsync(
        string name = "Seeded Forecast",
        int fiscalYear = 2026,
        int? userId = null,
        int? teamId = null,
        bool isSubmitted = false,
        bool isDeleted = false)
    {
        var forecast = CreateTestForecast(name, fiscalYear, userId, teamId, isSubmitted);
        forecast.IsDeleted = isDeleted;
        _dbContext.SalesForecasts.Add(forecast);
        await _dbContext.SaveChangesAsync();
        return forecast;
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateForecast_WhenValid()
    {
        var forecast = CreateTestForecast("New Forecast");
        var result = await _service.CreateAsync(forecast);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Forecast");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistInDatabase()
    {
        var forecast = CreateTestForecast("Persist Test");
        await _service.CreateAsync(forecast);

        var dbForecast = await _dbContext.SalesForecasts.FirstOrDefaultAsync(f => f.Name == "Persist Test");
        dbForecast.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldSetTimestamps()
    {
        var forecast = CreateTestForecast();
        var result = await _service.CreateAsync(forecast);

        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnForecast_WhenExists()
    {
        var seeded = await SeedForecastAsync("Find Me");
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
        var seeded = await SeedForecastAsync("Deleted", isDeleted: true);
        var result = await _service.GetByIdAsync(seeded.Id);
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeleted()
    {
        await SeedForecastAsync("F1");
        await SeedForecastAsync("F2");
        await SeedForecastAsync("Deleted", isDeleted: true);

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByUserId()
    {
        await SeedForecastAsync("User 1", userId: 1);
        await SeedForecastAsync("User 2", userId: 2);

        var result = await _service.GetAllAsync(userId: 1);
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("User 1");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByTeamId()
    {
        await SeedForecastAsync("Team 1", teamId: 10);
        await SeedForecastAsync("Team 2", teamId: 20);

        var result = await _service.GetAllAsync(teamId: 10);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByFiscalYear()
    {
        await SeedForecastAsync("2025", fiscalYear: 2025);
        await SeedForecastAsync("2026", fiscalYear: 2026);

        var result = await _service.GetAllAsync(fiscalYear: 2026);
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("2026");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterBySubmittedStatus()
    {
        await SeedForecastAsync("Submitted", isSubmitted: true);
        await SeedForecastAsync("Draft", isSubmitted: false);

        var result = await _service.GetAllAsync(isSubmitted: true);
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Submitted");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedForecastAsync("Original");
        var update = CreateTestForecast("Updated");

        var result = await _service.UpdateAsync(seeded.Id, update);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProperties()
    {
        var seeded = await SeedForecastAsync("Original");
        var update = CreateTestForecast("Updated");
        update.ClosedWonAmount = 50000m;

        await _service.UpdateAsync(seeded.Id, update);

        var updated = await _dbContext.SalesForecasts.FindAsync(seeded.Id);
        updated!.Name.Should().Be("Updated");
        updated.ClosedWonAmount.Should().Be(50000m);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNotFound()
    {
        var update = CreateTestForecast("Update");
        var result = await _service.UpdateAsync(999, update);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenDeleted()
    {
        var seeded = await SeedForecastAsync("Deleted", isDeleted: true);
        var update = CreateTestForecast("Update");

        var result = await _service.UpdateAsync(seeded.Id, update);
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedForecastAsync("Delete Me");
        var result = await _service.DeleteAsync(seeded.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var seeded = await SeedForecastAsync("Soft Delete");
        await _service.DeleteAsync(seeded.Id);

        var deleted = await _dbContext.SalesForecasts.FindAsync(seeded.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }

    #endregion

    #region SubmitAsync Tests

    [Fact]
    public async Task SubmitAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedForecastAsync("Submit Me");
        var result = await _service.SubmitAsync(seeded.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitAsync_ShouldSetSubmittedFlags()
    {
        var seeded = await SeedForecastAsync("Submit Me");
        await _service.SubmitAsync(seeded.Id);

        var submitted = await _dbContext.SalesForecasts.FindAsync(seeded.Id);
        submitted!.IsSubmitted.Should().BeTrue();
        submitted.SubmittedAt.Should().NotBeNull();
        submitted.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SubmitAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.SubmitAsync(999);
        result.Should().BeFalse();
    }

    #endregion

    #region GetHistoryAsync Tests

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnHistory()
    {
        var history = new ForecastHistory
        {
            Period = "Q1",
            UserId = 1,
            ClosedWonAmount = 25000m,
            CommitAmount = 30000m,
            BestCaseAmount = 20000m,
            PipelineAmount = 50000m,
            SnapshotDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.ForecastHistories.Add(history);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetHistoryAsync("Q1");
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldFilterByUserId()
    {
        _dbContext.ForecastHistories.AddRange(
            new ForecastHistory { Period = "Q1", UserId = 1, SnapshotDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ForecastHistory { Period = "Q1", UserId = 2, SnapshotDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetHistoryAsync("Q1", userId: 1);
        result.Should().HaveCount(1);
    }

    #endregion

    #region CreateSnapshotAsync Tests

    [Fact]
    public async Task CreateSnapshotAsync_ShouldCreateHistoryFromForecast()
    {
        var forecast = await SeedForecastAsync("Snapshot Me");
        var snapshot = await _service.CreateSnapshotAsync(forecast.Id);

        snapshot.Should().NotBeNull();
        snapshot.ClosedWonAmount.Should().Be(forecast.ClosedWonAmount);
        snapshot.CommitAmount.Should().Be(forecast.CommitAmount);
    }

    #endregion

    #region GetLineItemsAsync Tests

    [Fact]
    public async Task GetLineItemsAsync_ShouldReturnLineItems()
    {
        var forecast = await SeedForecastAsync("With Items");
        _dbContext.ForecastLineItems.Add(new ForecastLineItem
        {
            SalesForecastId = forecast.Id,
            OpportunityId = 1,
            Amount = 50000m,
            Category = ForecastCategory.Commit,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetLineItemsAsync(forecast.Id);
        result.Should().HaveCount(1);
        result.First().OpportunityId.Should().Be(1);
    }

    [Fact]
    public async Task GetLineItemsAsync_ShouldReturnEmpty_WhenNoItems()
    {
        var forecast = await SeedForecastAsync("Empty");
        var result = await _service.GetLineItemsAsync(forecast.Id);
        result.Should().BeEmpty();
    }

    #endregion
}
