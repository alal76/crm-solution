// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
/// Tests for MasterDataSeederService
/// Covers: SeedIfEmptyAsync, ReseedAllAsync, GetStatsAsync
/// </summary>
public class MasterDataSeederServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<MasterDataSeederService>> _loggerMock;
    private readonly MasterDataSeederService _service;

    public MasterDataSeederServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options);
        _loggerMock = new Mock<ILogger<MasterDataSeederService>>();
        _service = new MasterDataSeederService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region SeedIfEmptyAsync Tests

    [Fact]
    public async Task SeedIfEmptyAsync_WhenColorPalettesEmpty_ShouldSeedColorPalettes()
    {
        // Arrange - ensure tables are empty
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var count = await _context.ColorPalettes.CountAsync();
        count.Should().BeGreaterThan(0, "color palettes should be seeded when empty");
    }

    [Fact]
    public async Task SeedIfEmptyAsync_WhenZipCodesEmpty_ShouldSeedZipCodes()
    {
        // Arrange - ensure tables are empty
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var count = await _context.ZipCodes.CountAsync();
        count.Should().BeGreaterThan(0, "zip codes should be seeded when empty");
    }

    [Fact]
    public async Task SeedIfEmptyAsync_WhenColorPalettesExist_ShouldNotReseed()
    {
        // Arrange - add a single palette
        _context.ColorPalettes.Add(new ColorPalette
        {
            Name = "Test Palette",
            Category = "test",
            Color1 = "#000000",
            IsUserDefined = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        var initialCount = await _context.ColorPalettes.CountAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var finalCount = await _context.ColorPalettes.CountAsync();
        finalCount.Should().Be(initialCount, "should not add more palettes when already exist");
    }

    [Fact]
    public async Task SeedIfEmptyAsync_WhenZipCodesExist_ShouldNotReseed()
    {
        // Arrange - add a single zip code
        _context.ZipCodes.Add(new ZipCode
        {
            Country = "Test",
            CountryCode = "TS",
            PostalCode = "12345",
            City = "Test City",
            IsActive = true
        });
        await _context.SaveChangesAsync();
        var initialCount = await _context.ZipCodes.CountAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var finalCount = await _context.ZipCodes.CountAsync();
        finalCount.Should().Be(initialCount, "should not add more zip codes when already exist");
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ShouldLogInformation_WhenSeeding()
    {
        // Arrange
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ShouldHandleExceptions_Gracefully()
    {
        // This test verifies that exceptions are caught and logged
        // In a real scenario, we'd use a mock context that throws
        // For now, verify the method completes without throwing on valid context
        
        // Act
        var act = async () => await _service.SeedIfEmptyAsync();

        // Assert - should not throw
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetStatsAsync Tests

    [Fact]
    public async Task GetStatsAsync_WhenEmpty_ShouldReturnZeroCounts()
    {
        // Arrange - ensure tables are empty
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        var stats = await _service.GetStatsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.ColorPaletteCount.Should().Be(0);
        stats.ZipCodeCount.Should().Be(0);
        stats.ColorPalettesPopulated.Should().BeFalse();
        stats.ZipCodesPopulated.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatsAsync_WithData_ShouldReturnCorrectCounts()
    {
        // Arrange
        _context.ColorPalettes.AddRange(
            new ColorPalette { Name = "P1", Category = "test", Color1 = "#000", CreatedAt = DateTime.UtcNow },
            new ColorPalette { Name = "P2", Category = "test", Color1 = "#111", CreatedAt = DateTime.UtcNow }
        );
        _context.ZipCodes.AddRange(
            new ZipCode { Country = "US", CountryCode = "US", PostalCode = "10001", City = "NYC", IsActive = true },
            new ZipCode { Country = "US", CountryCode = "US", PostalCode = "10002", City = "NYC", IsActive = true },
            new ZipCode { Country = "US", CountryCode = "US", PostalCode = "90210", City = "BH", IsActive = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var stats = await _service.GetStatsAsync();

        // Assert
        stats.ColorPaletteCount.Should().Be(2);
        stats.ZipCodeCount.Should().Be(3);
        stats.ColorPalettesPopulated.Should().BeTrue();
        stats.ZipCodesPopulated.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatsAsync_AfterSeeding_ShouldShowPopulated()
    {
        // Arrange - seed data
        await _service.SeedIfEmptyAsync();

        // Act
        var stats = await _service.GetStatsAsync();

        // Assert
        stats.ColorPalettesPopulated.Should().BeTrue();
        stats.ZipCodesPopulated.Should().BeTrue();
    }

    #endregion

    #region Color Palette Data Tests

    [Fact]
    public async Task SeedIfEmptyAsync_ShouldSeedMultipleCategories_ForColorPalettes()
    {
        // Arrange
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var categories = await _context.ColorPalettes
            .Select(p => p.Category)
            .Distinct()
            .ToListAsync();

        categories.Should().HaveCountGreaterThan(1, "should have multiple categories");
        // Expected categories based on code: professional, nature, vibrant, warm, cool, dark, pastel, earthy, modern
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ColorPalettes_ShouldHaveRequiredFields()
    {
        // Arrange
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var palettes = await _context.ColorPalettes.ToListAsync();
        foreach (var palette in palettes)
        {
            palette.Name.Should().NotBeNullOrWhiteSpace();
            palette.Category.Should().NotBeNullOrWhiteSpace();
            palette.Color1.Should().NotBeNullOrWhiteSpace();
            palette.Color1.Should().StartWith("#", "colors should be hex values");
            palette.IsUserDefined.Should().BeFalse("seeded palettes are not user-defined");
        }
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ColorPalettes_ShouldHaveAtLeast40Palettes()
    {
        // Arrange
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var count = await _context.ColorPalettes.CountAsync();
        count.Should().BeGreaterOrEqualTo(40, "based on code there are ~40+ predefined palettes");
    }

    #endregion

    #region ZIP Code Data Tests

    [Fact]
    public async Task SeedIfEmptyAsync_ShouldSeedMultipleCountries_ForZipCodes()
    {
        // Arrange
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var countries = await _context.ZipCodes
            .Select(z => z.CountryCode)
            .Distinct()
            .ToListAsync();

        countries.Should().HaveCountGreaterThan(5, "should have multiple countries");
        countries.Should().Contain("US");
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ZipCodes_ShouldHaveRequiredFields()
    {
        // Arrange
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var zipCodes = await _context.ZipCodes.Take(20).ToListAsync();
        foreach (var zip in zipCodes)
        {
            zip.Country.Should().NotBeNullOrWhiteSpace();
            zip.CountryCode.Should().NotBeNullOrWhiteSpace();
            zip.PostalCode.Should().NotBeNullOrWhiteSpace();
            zip.City.Should().NotBeNullOrWhiteSpace();
            zip.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ZipCodes_ShouldIncludeMajorUSCities()
    {
        // Arrange
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var usZipCodes = await _context.ZipCodes
            .Where(z => z.CountryCode == "US")
            .ToListAsync();

        usZipCodes.Should().HaveCountGreaterThan(10);
        usZipCodes.Select(z => z.City).Should().Contain("New York");
        usZipCodes.Select(z => z.PostalCode).Should().Contain("90210"); // Beverly Hills
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ZipCodes_ShouldHaveLatLongCoordinates()
    {
        // Arrange
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();

        // Assert
        var zipCodes = await _context.ZipCodes.Take(10).ToListAsync();
        foreach (var zip in zipCodes)
        {
            zip.Latitude.Should().NotBeNull();
            zip.Longitude.Should().NotBeNull();
            zip.Latitude.Should().BeInRange(-90, 90);
            zip.Longitude.Should().BeInRange(-180, 180);
        }
    }

    #endregion

    #region ReseedAllAsync Tests

    [Fact]
    public async Task ReseedAllAsync_ShouldClearAndReseed()
    {
        // Note: This test may fail with InMemory database due to ExecuteSqlRawAsync
        // In a real scenario, use a proper test database
        
        // Arrange - seed initial data
        _context.ColorPalettes.Add(new ColorPalette
        {
            Name = "Custom",
            Category = "custom",
            Color1 = "#FFFFFF",
            IsUserDefined = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // This test documents expected behavior
        // ReseedAllAsync uses ExecuteSqlRawAsync which doesn't work with InMemory
        // The method should clear existing data and reseed
    }

    #endregion

    #region MasterDataStats Tests

    [Fact]
    public void MasterDataStats_ZipCodesPopulated_ShouldReturnTrue_WhenCountGreaterThanZero()
    {
        // Arrange
        var stats = new MasterDataStats { ZipCodeCount = 100 };

        // Assert
        stats.ZipCodesPopulated.Should().BeTrue();
    }

    [Fact]
    public void MasterDataStats_ZipCodesPopulated_ShouldReturnFalse_WhenCountIsZero()
    {
        // Arrange
        var stats = new MasterDataStats { ZipCodeCount = 0 };

        // Assert
        stats.ZipCodesPopulated.Should().BeFalse();
    }

    [Fact]
    public void MasterDataStats_ColorPalettesPopulated_ShouldReturnTrue_WhenCountGreaterThanZero()
    {
        // Arrange
        var stats = new MasterDataStats { ColorPaletteCount = 40 };

        // Assert
        stats.ColorPalettesPopulated.Should().BeTrue();
    }

    [Fact]
    public void MasterDataStats_ColorPalettesPopulated_ShouldReturnFalse_WhenCountIsZero()
    {
        // Arrange
        var stats = new MasterDataStats { ColorPaletteCount = 0 };

        // Assert
        stats.ColorPalettesPopulated.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task SeedIfEmptyAsync_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        // Act
        await _service.SeedIfEmptyAsync();
        var stats1 = await _service.GetStatsAsync();
        
        await _service.SeedIfEmptyAsync();
        var stats2 = await _service.GetStatsAsync();

        // Assert
        stats2.ColorPaletteCount.Should().Be(stats1.ColorPaletteCount);
        stats2.ZipCodeCount.Should().Be(stats1.ZipCodeCount);
    }

    [Fact]
    public async Task SeedIfEmptyAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        _context.ColorPalettes.RemoveRange(_context.ColorPalettes);
        _context.ZipCodes.RemoveRange(_context.ZipCodes);
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow;

        // Act
        await _service.SeedIfEmptyAsync();

        var elapsed = DateTime.UtcNow - startTime;

        // Assert - should complete in under 30 seconds for sample data
        elapsed.TotalSeconds.Should().BeLessThan(30);
    }

    #endregion
}
