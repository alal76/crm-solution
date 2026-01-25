// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for ZipCodeService
/// 
/// FUNCTIONAL VIEW:
/// - Tests geographic data lookup operations
/// - Tests hierarchical country/state/city retrieval
/// - Tests zip code validation and search
/// 
/// TECHNICAL VIEW:
/// - Uses InMemory database for realistic testing
/// - Tests service layer business logic
/// </summary>
public class ZipCodeServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly ZipCodeService _service;
    private readonly Mock<ILogger<ZipCodeService>> _loggerMock;

    public ZipCodeServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_ZipCodes_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _loggerMock = new Mock<ILogger<ZipCodeService>>();
        _service = new ZipCodeService(_dbContext, _loggerMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Seed countries
        _dbContext.Countries.AddRange(
            new Country { Id = 1, Code = "US", Name = "United States", Iso3 = "USA" },
            new Country { Id = 2, Code = "CA", Name = "Canada", Iso3 = "CAN" },
            new Country { Id = 3, Code = "GB", Name = "United Kingdom", Iso3 = "GBR" }
        );

        // Seed states
        _dbContext.States.AddRange(
            new State { Id = 1, Code = "CA", Name = "California", CountryCode = "US" },
            new State { Id = 2, Code = "NY", Name = "New York", CountryCode = "US" },
            new State { Id = 3, Code = "TX", Name = "Texas", CountryCode = "US" },
            new State { Id = 4, Code = "ON", Name = "Ontario", CountryCode = "CA" },
            new State { Id = 5, Code = "BC", Name = "British Columbia", CountryCode = "CA" }
        );

        // Seed zip codes
        _dbContext.ZipCodes.AddRange(
            new ZipCode { Id = 1, PostalCode = "90210", City = "Beverly Hills", StateCode = "CA", CountryCode = "US", Latitude = 34.0901, Longitude = -118.4065 },
            new ZipCode { Id = 2, PostalCode = "90211", City = "Beverly Hills", StateCode = "CA", CountryCode = "US", Latitude = 34.0622, Longitude = -118.3825 },
            new ZipCode { Id = 3, PostalCode = "10001", City = "New York", StateCode = "NY", CountryCode = "US", Latitude = 40.7484, Longitude = -73.9967 },
            new ZipCode { Id = 4, PostalCode = "10002", City = "New York", StateCode = "NY", CountryCode = "US", Latitude = 40.7157, Longitude = -73.9863 },
            new ZipCode { Id = 5, PostalCode = "75001", City = "Dallas", StateCode = "TX", CountryCode = "US", Latitude = 32.7767, Longitude = -96.7970 },
            new ZipCode { Id = 6, PostalCode = "M5A1A1", City = "Toronto", StateCode = "ON", CountryCode = "CA", Latitude = 43.6532, Longitude = -79.3832 },
            new ZipCode { Id = 7, PostalCode = "V6B1A1", City = "Vancouver", StateCode = "BC", CountryCode = "CA", Latitude = 49.2827, Longitude = -123.1207 }
        );

        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region GetCountriesAsync Tests

    [Fact]
    public async Task GetCountriesAsync_ReturnsAllCountries()
    {
        // Act
        var result = await _service.GetCountriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Code == "US");
        result.Should().Contain(c => c.Code == "CA");
        result.Should().Contain(c => c.Code == "GB");
    }

    [Fact]
    public async Task GetCountriesAsync_ReturnsSortedByName()
    {
        // Act
        var result = (await _service.GetCountriesAsync()).ToList();

        // Assert
        result[0].Name.Should().Be("Canada");
        result[1].Name.Should().Be("United Kingdom");
        result[2].Name.Should().Be("United States");
    }

    #endregion

    #region GetStatesAsync Tests

    [Fact]
    public async Task GetStatesAsync_WithValidCountry_ReturnsStates()
    {
        // Act
        var result = await _service.GetStatesAsync("US");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(s => s.CountryCode.Should().Be("US"));
    }

    [Fact]
    public async Task GetStatesAsync_WithInvalidCountry_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetStatesAsync("XX");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatesAsync_ReturnsStateSortedByName()
    {
        // Act
        var result = (await _service.GetStatesAsync("US")).ToList();

        // Assert
        result[0].Name.Should().Be("California");
        result[1].Name.Should().Be("New York");
        result[2].Name.Should().Be("Texas");
    }

    #endregion

    #region GetCitiesAsync Tests

    [Fact]
    public async Task GetCitiesAsync_WithValidStateAndCountry_ReturnsCities()
    {
        // Act
        var result = await _service.GetCitiesAsync("US", "CA");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // Beverly Hills (deduped)
    }

    [Fact]
    public async Task GetCitiesAsync_WithInvalidState_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetCitiesAsync("US", "XX");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region LookupZipCodeAsync Tests

    [Fact]
    public async Task LookupZipCodeAsync_WithValidZipCode_ReturnsData()
    {
        // Act
        var result = await _service.LookupZipCodeAsync("90210", "US");

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be("Beverly Hills");
        result.StateCode.Should().Be("CA");
        result.Latitude.Should().BeApproximately(34.0901, 0.001);
    }

    [Fact]
    public async Task LookupZipCodeAsync_WithInvalidZipCode_ReturnsNull()
    {
        // Act
        var result = await _service.LookupZipCodeAsync("00000", "US");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LookupZipCodeAsync_WithWrongCountry_ReturnsNull()
    {
        // Act
        var result = await _service.LookupZipCodeAsync("90210", "CA"); // US zip in Canada

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchZipCodesAsync Tests

    [Fact]
    public async Task SearchZipCodesAsync_WithPartialCode_ReturnsMatches()
    {
        // Act
        var result = await _service.SearchZipCodesAsync("902", "US", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(z => z.PostalCode.Should().StartWith("902"));
    }

    [Fact]
    public async Task SearchZipCodesAsync_WithCitySearch_ReturnsMatches()
    {
        // Act
        var result = await _service.SearchZipCodesAsync("Beverly", "US", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(z => z.City.Should().Contain("Beverly"));
    }

    [Fact]
    public async Task SearchZipCodesAsync_WithLimit_RespectsLimit()
    {
        // Act
        var result = await _service.SearchZipCodesAsync("1000", "US", 1);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region ValidateZipCodeAsync Tests

    [Fact]
    public async Task ValidateZipCodeAsync_WithValidZipCode_ReturnsTrue()
    {
        // Act
        var result = await _service.ValidateZipCodeAsync("90210", "US");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateZipCodeAsync_WithInvalidZipCode_ReturnsFalse()
    {
        // Act
        var result = await _service.ValidateZipCodeAsync("00000", "US");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateZipCodeAsync_CaseInsensitive()
    {
        // Act
        var resultUpper = await _service.ValidateZipCodeAsync("M5A1A1", "CA");
        var resultLower = await _service.ValidateZipCodeAsync("m5a1a1", "CA");

        // Assert
        resultUpper.Should().BeTrue();
        resultLower.Should().BeTrue();
    }

    #endregion

    #region GetZipCodesByCity Tests

    [Fact]
    public async Task GetZipCodesByCity_ReturnsAllZipCodesForCity()
    {
        // Act
        var result = await _service.GetZipCodesByCityAsync("New York", "US");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(z => z.PostalCode == "10001");
        result.Should().Contain(z => z.PostalCode == "10002");
    }

    #endregion
}
