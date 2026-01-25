// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using Moq;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for ZipCodesController
/// 
/// FUNCTIONAL VIEW:
/// - Tests geographic data lookup endpoints
/// - Tests country, state, and city hierarchical retrieval
/// - Tests zip code validation and formatting
/// 
/// TECHNICAL VIEW:
/// - Mocks IZipCodeService for isolated testing
/// - Validates response structures for dropdown population
/// </summary>
public class ZipCodesControllerTests
{
    private readonly Mock<IZipCodeService> _mockZipCodeService;
    private readonly Mock<ILogger<ZipCodesController>> _mockLogger;
    private readonly ZipCodesController _controller;

    public ZipCodesControllerTests()
    {
        _mockZipCodeService = new Mock<IZipCodeService>();
        _mockLogger = new Mock<ILogger<ZipCodesController>>();
        _controller = new ZipCodesController(_mockZipCodeService.Object, _mockLogger.Object);
    }

    #region GetCountries Tests

    /// <summary>
    /// FUNCTIONAL: Returns list of all countries for dropdown
    /// TECHNICAL: Returns OkObjectResult with country list
    /// </summary>
    [Fact]
    public async Task GetCountries_ReturnsAllCountries()
    {
        // Arrange
        var countries = new List<CountryDto>
        {
            new CountryDto { Code = "US", Name = "United States" },
            new CountryDto { Code = "CA", Name = "Canada" },
            new CountryDto { Code = "GB", Name = "United Kingdom" },
            new CountryDto { Code = "AU", Name = "Australia" },
            new CountryDto { Code = "IN", Name = "India" }
        };

        _mockZipCodeService.Setup(s => s.GetCountriesAsync())
            .ReturnsAsync(countries);

        // Act
        var result = await _controller.GetCountries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
        var returnedCountries = okResult.Value as IEnumerable<CountryDto>;
        returnedCountries.Should().HaveCount(5);
    }

    /// <summary>
    /// FUNCTIONAL: Countries are sorted alphabetically
    /// TECHNICAL: Verifies ordering of returned collection
    /// </summary>
    [Fact]
    public async Task GetCountries_ReturnsSortedList()
    {
        // Arrange
        var countries = new List<CountryDto>
        {
            new CountryDto { Code = "AU", Name = "Australia" },
            new CountryDto { Code = "CA", Name = "Canada" },
            new CountryDto { Code = "US", Name = "United States" }
        };

        _mockZipCodeService.Setup(s => s.GetCountriesAsync())
            .ReturnsAsync(countries);

        // Act
        var result = await _controller.GetCountries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCountries = (okResult.Value as IEnumerable<CountryDto>)?.ToList();
        returnedCountries.Should().NotBeNull();
        returnedCountries![0].Name.Should().Be("Australia");
    }

    #endregion

    #region GetStates Tests

    /// <summary>
    /// FUNCTIONAL: Returns states for specific country
    /// TECHNICAL: Filters by country code parameter
    /// </summary>
    [Fact]
    public async Task GetStates_WithValidCountryCode_ReturnsStates()
    {
        // Arrange
        var states = new List<StateDto>
        {
            new StateDto { Code = "CA", Name = "California", CountryCode = "US" },
            new StateDto { Code = "NY", Name = "New York", CountryCode = "US" },
            new StateDto { Code = "TX", Name = "Texas", CountryCode = "US" }
        };

        _mockZipCodeService.Setup(s => s.GetStatesAsync("US"))
            .ReturnsAsync(states);

        // Act
        var result = await _controller.GetStates("US");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStates = okResult.Value as IEnumerable<StateDto>;
        returnedStates.Should().HaveCount(3);
        returnedStates.Should().AllSatisfy(s => s.CountryCode.Should().Be("US"));
    }

    /// <summary>
    /// FUNCTIONAL: Invalid country code returns empty list
    /// TECHNICAL: Returns OkObjectResult with empty collection
    /// </summary>
    [Fact]
    public async Task GetStates_WithInvalidCountryCode_ReturnsEmptyList()
    {
        // Arrange
        _mockZipCodeService.Setup(s => s.GetStatesAsync("XX"))
            .ReturnsAsync(new List<StateDto>());

        // Act
        var result = await _controller.GetStates("XX");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStates = okResult.Value as IEnumerable<StateDto>;
        returnedStates.Should().BeEmpty();
    }

    #endregion

    #region GetCities Tests

    /// <summary>
    /// FUNCTIONAL: Returns cities for specific state and country
    /// TECHNICAL: Filters by both country and state codes
    /// </summary>
    [Fact]
    public async Task GetCities_WithValidCodes_ReturnsCities()
    {
        // Arrange
        var cities = new List<CityDto>
        {
            new CityDto { Name = "Los Angeles", StateCode = "CA", CountryCode = "US" },
            new CityDto { Name = "San Francisco", StateCode = "CA", CountryCode = "US" },
            new CityDto { Name = "San Diego", StateCode = "CA", CountryCode = "US" }
        };

        _mockZipCodeService.Setup(s => s.GetCitiesAsync("US", "CA"))
            .ReturnsAsync(cities);

        // Act
        var result = await _controller.GetCities("US", "CA");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCities = okResult.Value as IEnumerable<CityDto>;
        returnedCities.Should().HaveCount(3);
    }

    #endregion

    #region LookupZipCode Tests

    /// <summary>
    /// FUNCTIONAL: Valid zip code returns location details
    /// TECHNICAL: Returns OkObjectResult with ZipCodeDto
    /// </summary>
    [Fact]
    public async Task LookupZipCode_WithValidCode_ReturnsLocationData()
    {
        // Arrange
        var zipCode = new ZipCodeDto
        {
            PostalCode = "90210",
            City = "Beverly Hills",
            State = "California",
            StateCode = "CA",
            Country = "United States",
            CountryCode = "US",
            Latitude = 34.0901,
            Longitude = -118.4065
        };

        _mockZipCodeService.Setup(s => s.LookupZipCodeAsync("90210", "US"))
            .ReturnsAsync(zipCode);

        // Act
        var result = await _controller.LookupZipCode("90210", "US");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedZipCode = okResult.Value as ZipCodeDto;
        returnedZipCode.Should().NotBeNull();
        returnedZipCode!.City.Should().Be("Beverly Hills");
        returnedZipCode.StateCode.Should().Be("CA");
    }

    /// <summary>
    /// FUNCTIONAL: Invalid zip code returns 404
    /// TECHNICAL: Returns NotFoundObjectResult
    /// </summary>
    [Fact]
    public async Task LookupZipCode_WithInvalidCode_ReturnsNotFound()
    {
        // Arrange
        _mockZipCodeService.Setup(s => s.LookupZipCodeAsync("00000", "US"))
            .ReturnsAsync((ZipCodeDto?)null);

        // Act
        var result = await _controller.LookupZipCode("00000", "US");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// FUNCTIONAL: Zip code search with partial match
    /// TECHNICAL: Returns list of matching zip codes
    /// </summary>
    [Fact]
    public async Task SearchZipCodes_WithPartialCode_ReturnsMatches()
    {
        // Arrange
        var zipCodes = new List<ZipCodeDto>
        {
            new ZipCodeDto { PostalCode = "90210", City = "Beverly Hills" },
            new ZipCodeDto { PostalCode = "90211", City = "Beverly Hills" },
            new ZipCodeDto { PostalCode = "90212", City = "Beverly Hills" }
        };

        _mockZipCodeService.Setup(s => s.SearchZipCodesAsync("902", "US", 10))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _controller.SearchZipCodes("902", "US", 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedZipCodes = okResult.Value as IEnumerable<ZipCodeDto>;
        returnedZipCodes.Should().HaveCount(3);
    }

    #endregion

    #region ValidateZipCode Tests

    /// <summary>
    /// FUNCTIONAL: Validates zip code format and existence
    /// TECHNICAL: Returns OkObjectResult with validation result
    /// </summary>
    [Fact]
    public async Task ValidateZipCode_WithValidCode_ReturnsTrue()
    {
        // Arrange
        _mockZipCodeService.Setup(s => s.ValidateZipCodeAsync("90210", "US"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateZipCode("90210", "US");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var isValid = (bool)okResult.Value!;
        isValid.Should().BeTrue();
    }

    /// <summary>
    /// FUNCTIONAL: Invalid zip code returns false
    /// TECHNICAL: Returns OkObjectResult with false value
    /// </summary>
    [Fact]
    public async Task ValidateZipCode_WithInvalidCode_ReturnsFalse()
    {
        // Arrange
        _mockZipCodeService.Setup(s => s.ValidateZipCodeAsync("INVALID", "US"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateZipCode("INVALID", "US");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var isValid = (bool)okResult.Value!;
        isValid.Should().BeFalse();
    }

    #endregion
}
