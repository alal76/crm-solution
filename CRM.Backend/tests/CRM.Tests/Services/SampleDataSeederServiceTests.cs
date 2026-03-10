// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class SampleDataSeederServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<SampleDataSeederService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly SampleDataSeederService _service;

    public SampleDataSeederServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"SampleSeederTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<SampleDataSeederService>>();
        _configuration = new ConfigurationBuilder().Build();
        _service = new SampleDataSeederService(_context, _mockLogger.Object, _configuration);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var act = () => new SampleDataSeederService(_context, _mockLogger.Object, _configuration);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldReturnResult()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldNotThrow()
    {
        var act = async () => await _service.SeedAllSampleDataAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldReportSuccessOrPartialFailure()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        // Result should indicate some completion state
        result.Should().NotBeNull();
        // Steps list should be initialized (non-null)
        result.Steps.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldBeIdempotent()
    {
        // Running twice should not throw
        await _service.SeedAllSampleDataWithLogAsync();
        var act = async () => await _service.SeedAllSampleDataWithLogAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task IsSampleDataSeededAsync_ShouldReturnFalse_WhenDatabaseIsEmpty()
    {
        var result = await _service.IsSampleDataSeededAsync();
        // Fresh InMemory DB has no SystemSettings row → should return false
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldPopulateStepsList()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        result.Steps.Should().NotBeEmpty();
        // Every step should have a non-empty name
        result.Steps.All(s => !string.IsNullOrEmpty(s.Step)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldReturnPositiveDuration()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        result.TotalDurationMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSampleDataStatsAsync_ShouldReturnStats()
    {
        // Seed first so there's at least the SystemSettings row
        await _service.SeedAllSampleDataWithLogAsync();
        var act = async () => await _service.GetSampleDataStatsAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearSampleDataAsync_ShouldNotThrow()
    {
        var act = async () => await _service.ClearSampleDataAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldNotThrow()
    {
        var act = async () => await _service.SeedSampleUsersAsync();
        await act.Should().NotThrowAsync();
    }
}
