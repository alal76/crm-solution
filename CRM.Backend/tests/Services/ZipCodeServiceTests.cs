// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ZipCodeService
/// Covers: ZIP code lookup, city/state search, import/export
/// </summary>
public class ZipCodeServiceTests
{
    private readonly Mock<IRepository<ZipCode>> _mockZipCodeRepository;
    private readonly Mock<IRepository<Locality>> _mockLocalityRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<ZipCodeService>> _mockLogger;
    private readonly ZipCodeService _service;

    public ZipCodeServiceTests()
    {
        _mockZipCodeRepository = new Mock<IRepository<ZipCode>>();
        _mockLocalityRepository = new Mock<IRepository<Locality>>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<ZipCodeService>>();

        // Setup cache to return null (cache miss)
        object? cacheValue = null;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
            .Returns(false);

        _service = new ZipCodeService(
            _mockZipCodeRepository.Object,
            _mockLocalityRepository.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    #region Lookup Tests

    [Fact]
    public async Task GetByZipCodeAsync_ExistingZip_ReturnsZipCode()
    {
        // Arrange
        var zipCode = new ZipCode
        {
            Id = 1,
            Code = "12345",
            City = "Anytown",
            State = "CA",
            County = "Test County"
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode> { zipCode });

        // Act
        var result = await _service.GetByZipCodeAsync("12345");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("12345");
        result.City.Should().Be("Anytown");
    }

    [Fact]
    public async Task GetByZipCodeAsync_NonExistingZip_ReturnsNull()
    {
        // Arrange
        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode>());

        // Act
        var result = await _service.GetByZipCodeAsync("99999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByZipCodeAsync_WithCountry_ReturnsCountrySpecific()
    {
        // Arrange
        var zipCode = new ZipCode
        {
            Id = 1,
            Code = "12345",
            City = "Test City",
            Country = "US"
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode> { zipCode });

        // Act
        var result = await _service.GetByZipCodeAsync("12345", "US");

        // Assert
        result.Should().NotBeNull();
        result!.Country.Should().Be("US");
    }

    #endregion

    #region City Search Tests

    [Fact]
    public async Task SearchByCityAsync_ValidCity_ReturnsZipCodes()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, Code = "12345", City = "Springfield" },
            new ZipCode { Id = 2, Code = "12346", City = "Springfield" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.SearchByCityAsync("Springfield");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchByCityAsync_PartialMatch_ReturnsMatching()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, Code = "12345", City = "Springfield" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.SearchByCityAsync("Spring");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchByCityAsync_CaseInsensitive_ReturnsMatching()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, Code = "12345", City = "Springfield" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.SearchByCityAsync("SPRINGFIELD");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region State Search Tests

    [Fact]
    public async Task GetByStateAsync_ValidState_ReturnsZipCodes()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, Code = "90001", State = "CA" },
            new ZipCode { Id = 2, Code = "90002", State = "CA" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.GetByStateAsync("CA");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStateAndCityAsync_ValidInput_ReturnsFiltered()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, Code = "90001", City = "Los Angeles", State = "CA" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.GetByStateAndCityAsync("CA", "Los Angeles");

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region County Search Tests

    [Fact]
    public async Task GetByCountyAsync_ValidCounty_ReturnsZipCodes()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, Code = "12345", County = "Test County" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.GetByCountyAsync("Test County");

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Proximity Search Tests

    [Fact]
    public async Task GetZipCodesInRadiusAsync_ValidInput_ReturnsNearbyZips()
    {
        // Arrange
        var centerZip = new ZipCode
        {
            Id = 1,
            Code = "12345",
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        var nearbyZips = new List<ZipCode>
        {
            new ZipCode { Id = 2, Code = "12346", Latitude = 40.7138, Longitude = -74.0070 },
            new ZipCode { Id = 3, Code = "12347", Latitude = 40.7148, Longitude = -74.0080 }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode> { centerZip });

        _mockZipCodeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(nearbyZips.Concat(new[] { centerZip }).ToList());

        // Act
        var result = await _service.GetZipCodesInRadiusAsync("12345", 10);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculateDistanceAsync_ValidZipCodes_ReturnsDistance()
    {
        // Arrange
        var zip1 = new ZipCode { Code = "10001", Latitude = 40.7484, Longitude = -73.9967 };
        var zip2 = new ZipCode { Code = "90001", Latitude = 33.9425, Longitude = -118.2551 };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync((Expression<Func<ZipCode, bool>> pred) =>
            {
                var func = pred.Compile();
                return new List<ZipCode> { zip1, zip2 }.Where(z => func(z)).ToList();
            });

        // Act
        var result = await _service.CalculateDistanceAsync("10001", "90001");

        // Assert
        result.Should().BeGreaterThan(0);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateZipCodeAsync_ValidZip_ReturnsTrue()
    {
        // Arrange
        var zipCode = new ZipCode { Id = 1, Code = "12345" };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode> { zipCode });

        // Act
        var result = await _service.ValidateZipCodeAsync("12345");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateZipCodeAsync_InvalidZip_ReturnsFalse()
    {
        // Arrange
        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode>());

        // Act
        var result = await _service.ValidateZipCodeAsync("00000");

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateZipCodeFormatAsync_ValidUSFormat_ReturnsTrue()
    {
        // Act
        var result = await _service.ValidateZipCodeFormatAsync("12345", "US");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateZipCodeFormatAsync_InvalidFormat_ReturnsFalse()
    {
        // Act
        var result = await _service.ValidateZipCodeFormatAsync("ABC", "US");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Locality Tests

    [Fact]
    public async Task GetLocalitiesAsync_ReturnsLocalities()
    {
        // Arrange
        var localities = new List<Locality>
        {
            new Locality { Id = 1, Name = "Downtown", ZipCodeId = 1 },
            new Locality { Id = 2, Name = "Uptown", ZipCodeId = 1 }
        };

        _mockLocalityRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Locality, bool>>>()))
            .ReturnsAsync(localities);

        // Act
        var result = await _service.GetLocalitiesAsync("12345");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Autocomplete Tests

    [Fact]
    public async Task AutocompleteZipCodeAsync_PartialInput_ReturnsSuggestions()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Code = "12345", City = "Test City" },
            new ZipCode { Code = "12346", City = "Test Town" },
            new ZipCode { Code = "12347", City = "Test Village" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.AutocompleteZipCodeAsync("123");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task AutocompleteCityAsync_PartialInput_ReturnsSuggestions()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Code = "12345", City = "Springfield" },
            new ZipCode { Code = "67890", City = "Spring Valley" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.AutocompleteCityAsync("Spr");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Import/Export Tests

    [Fact]
    public async Task ImportZipCodesAsync_ValidData_ImportsSuccessfully()
    {
        // Arrange
        var zipCodes = new List<ZipCodeImportDto>
        {
            new ZipCodeImportDto { Code = "11111", City = "New City", State = "NY" }
        };

        _mockZipCodeRepository.Setup(r => r.AddAsync(It.IsAny<ZipCode>()))
            .ReturnsAsync((ZipCode z) => { z.Id = 1; return z; });

        // Act
        var result = await _service.ImportZipCodesAsync(zipCodes);

        // Assert
        result.ImportedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExportZipCodesAsync_ValidState_ReturnsExportData()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Code = "12345", City = "Test", State = "CA" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.ExportZipCodesAsync("CA");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { State = "CA" },
            new ZipCode { State = "CA" },
            new ZipCode { State = "NY" }
        };

        _mockZipCodeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalZipCodes.Should().Be(3);
        result.StateCount.Should().Be(2);
    }

    [Fact]
    public async Task GetStateListAsync_ReturnsDistinctStates()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { State = "CA" },
            new ZipCode { State = "NY" },
            new ZipCode { State = "CA" }
        };

        _mockZipCodeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.GetStateListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("CA");
        result.Should().Contain("NY");
    }

    #endregion
}

// Supporting classes for tests
public class ZipCodeImportDto
{
    public string Code { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? County { get; set; }
}
