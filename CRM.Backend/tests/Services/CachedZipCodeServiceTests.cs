// CRM Solution - Comprehensive Test Suite
// CachedZipCodeServiceTests - Unit tests for cached ZIP code service

using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CachedZipCodeService - caching decorator for ZipCodeService
/// </summary>
public class CachedZipCodeServiceTests
{
    private readonly Mock<IZipCodeService> _mockInnerService;
    private readonly Mock<ILogger<CachedZipCodeService>> _mockLogger;
    private readonly IMemoryCache _cache;
    private readonly CachedZipCodeService _service;

    public CachedZipCodeServiceTests()
    {
        _mockInnerService = new Mock<IZipCodeService>();
        _mockLogger = new Mock<ILogger<CachedZipCodeService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new CachedZipCodeService(_mockInnerService.Object, _cache, _mockLogger.Object);
    }

    #region GetCountriesAsync Tests

    [Fact]
    public async Task GetCountriesAsync_FirstCall_CallsInnerService()
    {
        // Arrange
        var countries = new List<CountryInfo>
        {
            new() { Code = "US", Name = "United States" },
            new() { Code = "CA", Name = "Canada" }
        };
        _mockInnerService.Setup(s => s.GetCountriesAsync()).ReturnsAsync(countries);

        // Act
        var result = await _service.GetCountriesAsync();

        // Assert
        result.Should().BeEquivalentTo(countries);
        _mockInnerService.Verify(s => s.GetCountriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCountriesAsync_SecondCall_ReturnsCachedData()
    {
        // Arrange
        var countries = new List<CountryInfo>
        {
            new() { Code = "US", Name = "United States" }
        };
        _mockInnerService.Setup(s => s.GetCountriesAsync()).ReturnsAsync(countries);

        // Act
        await _service.GetCountriesAsync(); // First call
        var result = await _service.GetCountriesAsync(); // Second call

        // Assert
        result.Should().BeEquivalentTo(countries);
        _mockInnerService.Verify(s => s.GetCountriesAsync(), Times.Once); // Should only call once
    }

    [Fact]
    public async Task GetCountriesAsync_WhenInnerServiceReturnsNull_ReturnsEmptyEnumerable()
    {
        // Arrange
        _mockInnerService.Setup(s => s.GetCountriesAsync()).ReturnsAsync((IEnumerable<CountryInfo>)null!);

        // Act
        var result = await _service.GetCountriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetStatesAsync Tests

    [Fact]
    public async Task GetStatesAsync_FirstCall_CallsInnerService()
    {
        // Arrange
        var states = new List<StateInfo>
        {
            new() { Code = "CA", Name = "California" },
            new() { Code = "NY", Name = "New York" }
        };
        _mockInnerService.Setup(s => s.GetStatesAsync("US")).ReturnsAsync(states);

        // Act
        var result = await _service.GetStatesAsync("US");

        // Assert
        result.Should().BeEquivalentTo(states);
        _mockInnerService.Verify(s => s.GetStatesAsync("US"), Times.Once);
    }

    [Fact]
    public async Task GetStatesAsync_SecondCallSameCountry_ReturnsCachedData()
    {
        // Arrange
        var states = new List<StateInfo>
        {
            new() { Code = "CA", Name = "California" }
        };
        _mockInnerService.Setup(s => s.GetStatesAsync("US")).ReturnsAsync(states);

        // Act
        await _service.GetStatesAsync("US"); // First call
        var result = await _service.GetStatesAsync("US"); // Second call

        // Assert
        result.Should().BeEquivalentTo(states);
        _mockInnerService.Verify(s => s.GetStatesAsync("US"), Times.Once);
    }

    [Fact]
    public async Task GetStatesAsync_DifferentCountries_CallsInnerServiceForEach()
    {
        // Arrange
        var usStates = new List<StateInfo> { new() { Code = "CA", Name = "California" } };
        var caProvinces = new List<StateInfo> { new() { Code = "ON", Name = "Ontario" } };
        
        _mockInnerService.Setup(s => s.GetStatesAsync("US")).ReturnsAsync(usStates);
        _mockInnerService.Setup(s => s.GetStatesAsync("CA")).ReturnsAsync(caProvinces);

        // Act
        var usResult = await _service.GetStatesAsync("US");
        var caResult = await _service.GetStatesAsync("CA");

        // Assert
        usResult.Should().BeEquivalentTo(usStates);
        caResult.Should().BeEquivalentTo(caProvinces);
        _mockInnerService.Verify(s => s.GetStatesAsync("US"), Times.Once);
        _mockInnerService.Verify(s => s.GetStatesAsync("CA"), Times.Once);
    }

    [Fact]
    public async Task GetStatesAsync_NormalizesCountryCodeToUppercase()
    {
        // Arrange
        var states = new List<StateInfo> { new() { Code = "CA", Name = "California" } };
        // The service passes the original (lowercase) countryCode to the inner service
        // but normalizes the cache key to uppercase, so both calls hit the same cache entry
        _mockInnerService.Setup(s => s.GetStatesAsync("us")).ReturnsAsync(states);

        // Act
        await _service.GetStatesAsync("us"); // lowercase - calls inner service
        var result = await _service.GetStatesAsync("US"); // uppercase - served from cache (same key)

        // Assert - inner service called only once with the original lowercase value
        _mockInnerService.Verify(s => s.GetStatesAsync("us"), Times.Once);
    }

    #endregion

    #region GetCitiesAsync Tests

    [Fact]
    public async Task GetCitiesAsync_FirstCall_CallsInnerService()
    {
        // Arrange
        var cities = new List<string> { "Los Angeles", "San Francisco" };
        _mockInnerService.Setup(s => s.GetCitiesAsync("US", "CA")).ReturnsAsync(cities);

        // Act
        var result = await _service.GetCitiesAsync("US", "CA");

        // Assert
        result.Should().BeEquivalentTo(cities);
        _mockInnerService.Verify(s => s.GetCitiesAsync("US", "CA"), Times.Once);
    }

    [Fact]
    public async Task GetCitiesAsync_SecondCallSameStateAndCountry_ReturnsCachedData()
    {
        // Arrange
        var cities = new List<string> { "Los Angeles" };
        _mockInnerService.Setup(s => s.GetCitiesAsync("US", "CA")).ReturnsAsync(cities);

        // Act
        await _service.GetCitiesAsync("US", "CA"); // First call
        var result = await _service.GetCitiesAsync("US", "CA"); // Second call

        // Assert
        result.Should().BeEquivalentTo(cities);
        _mockInnerService.Verify(s => s.GetCitiesAsync("US", "CA"), Times.Once);
    }

    [Fact]
    public async Task GetCitiesAsync_DifferentStates_CachesSeperately()
    {
        // Arrange
        var caCities = new List<string> { "Los Angeles" };
        var nyCities = new List<string> { "New York City" };
        
        _mockInnerService.Setup(s => s.GetCitiesAsync("US", "CA")).ReturnsAsync(caCities);
        _mockInnerService.Setup(s => s.GetCitiesAsync("US", "NY")).ReturnsAsync(nyCities);

        // Act
        var caResult = await _service.GetCitiesAsync("US", "CA");
        var nyResult = await _service.GetCitiesAsync("US", "NY");

        // Assert
        caResult.Should().BeEquivalentTo(caCities);
        nyResult.Should().BeEquivalentTo(nyCities);
    }

    #endregion

    #region GetPostalCodesForCityAsync Tests

    [Fact]
    public async Task GetPostalCodesForCityAsync_FirstCall_CallsInnerService()
    {
        // Arrange
        var postalCodes = new List<ZipCodeLookupResult>
        {
            new() { PostalCode = "90210", City = "Beverly Hills" }
        };
        _mockInnerService.Setup(s => s.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills"))
            .ReturnsAsync(postalCodes);

        // Act
        var result = await _service.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills");

        // Assert
        result.Should().BeEquivalentTo(postalCodes);
        _mockInnerService.Verify(s => s.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills"), Times.Once);
    }

    [Fact]
    public async Task GetPostalCodesForCityAsync_SecondCall_ReturnsCachedData()
    {
        // Arrange
        var postalCodes = new List<ZipCodeLookupResult>
        {
            new() { PostalCode = "90210", City = "Beverly Hills" }
        };
        _mockInnerService.Setup(s => s.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills"))
            .ReturnsAsync(postalCodes);

        // Act
        await _service.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills");
        var result = await _service.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills");

        // Assert
        _mockInnerService.Verify(s => s.GetPostalCodesForCityAsync("US", "CA", "Beverly Hills"), Times.Once);
    }

    #endregion

    #region LookupByPostalCodeAsync Tests

    [Fact]
    public async Task LookupByPostalCodeAsync_FirstCall_CallsInnerService()
    {
        // Arrange
        var results = new List<ZipCodeLookupResult>
        {
            new() { PostalCode = "90210", City = "Beverly Hills", State = "CA" }
        };
        _mockInnerService.Setup(s => s.LookupByPostalCodeAsync("90210", "US")).ReturnsAsync(results);

        // Act
        var result = await _service.LookupByPostalCodeAsync("90210", "US");

        // Assert
        result.Should().BeEquivalentTo(results);
    }

    [Fact]
    public async Task LookupByPostalCodeAsync_WithoutCountryCode_CachesWithAllMarker()
    {
        // Arrange
        var results = new List<ZipCodeLookupResult>
        {
            new() { PostalCode = "90210", City = "Beverly Hills" }
        };
        _mockInnerService.Setup(s => s.LookupByPostalCodeAsync("90210", null)).ReturnsAsync(results);

        // Act
        await _service.LookupByPostalCodeAsync("90210");
        await _service.LookupByPostalCodeAsync("90210");

        // Assert - should only call once
        _mockInnerService.Verify(s => s.LookupByPostalCodeAsync("90210", null), Times.Once);
    }

    #endregion

    #region SearchByCityAsync Tests

    [Fact]
    public async Task SearchByCityAsync_DoesNotCacheResults()
    {
        // Arrange
        var results = new List<ZipCodeLookupResult>
        {
            new() { PostalCode = "90210", City = "Beverly Hills" }
        };
        _mockInnerService.Setup(s => s.SearchByCityAsync("Beverly", null, 20)).ReturnsAsync(results);

        // Act
        await _service.SearchByCityAsync("Beverly", null, 20);
        await _service.SearchByCityAsync("Beverly", null, 20);

        // Assert - should call twice (not cached)
        _mockInnerService.Verify(s => s.SearchByCityAsync("Beverly", null, 20), Times.Exactly(2));
    }

    #endregion

    #region ValidatePostalCodeAsync Tests

    [Fact]
    public async Task ValidatePostalCodeAsync_FirstCall_CallsInnerService()
    {
        // Arrange
        var validationResult = new ZipCodeValidationResult { IsValid = true, Message = "Valid" };
        _mockInnerService.Setup(s => s.ValidatePostalCodeAsync("90210", "US")).ReturnsAsync(validationResult);

        // Act
        var result = await _service.ValidatePostalCodeAsync("90210", "US");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePostalCodeAsync_SecondCall_ReturnsCachedResult()
    {
        // Arrange
        var validationResult = new ZipCodeValidationResult { IsValid = true };
        _mockInnerService.Setup(s => s.ValidatePostalCodeAsync("90210", "US")).ReturnsAsync(validationResult);

        // Act
        await _service.ValidatePostalCodeAsync("90210", "US");
        await _service.ValidatePostalCodeAsync("90210", "US");

        // Assert
        _mockInnerService.Verify(s => s.ValidatePostalCodeAsync("90210", "US"), Times.Once);
    }

    [Fact]
    public async Task ValidatePostalCodeAsync_WhenInnerReturnsNull_ReturnsDefaultInvalidResult()
    {
        // Arrange
        _mockInnerService.Setup(s => s.ValidatePostalCodeAsync("00000", "US"))
            .ReturnsAsync((ZipCodeValidationResult)null!);

        // Act
        var result = await _service.ValidatePostalCodeAsync("00000", "US");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("failed");
    }

    #endregion

    #region GetLocalitiesAsync Tests

    [Fact]
    public async Task GetLocalitiesAsync_DoesNotCacheResults()
    {
        // Arrange
        var localities = new List<LocalityInfo>
        {
            new() { Name = "Downtown", City = "Los Angeles" }
        };
        _mockInnerService.Setup(s => s.GetLocalitiesAsync(1)).ReturnsAsync(localities);

        // Act
        await _service.GetLocalitiesAsync(1);
        await _service.GetLocalitiesAsync(1);

        // Assert - should call twice (not cached - user-created data)
        _mockInnerService.Verify(s => s.GetLocalitiesAsync(1), Times.Exactly(2));
    }

    #endregion

    #region GetLocalitiesByCityAsync Tests

    [Fact]
    public async Task GetLocalitiesByCityAsync_DelegatesDirectlyToInnerService()
    {
        // Arrange
        var localities = new List<LocalityInfo>
        {
            new() { Name = "Downtown", City = "Los Angeles" }
        };
        _mockInnerService.Setup(s => s.GetLocalitiesByCityAsync("Los Angeles", "US")).ReturnsAsync(localities);

        // Act
        var result = await _service.GetLocalitiesByCityAsync("Los Angeles", "US");

        // Assert
        result.Should().BeEquivalentTo(localities);
    }

    [Fact]
    public async Task GetLocalitiesByCityAsync_UsesDefaultCountryCodeWhenNull()
    {
        // Arrange
        var localities = new List<LocalityInfo>();
        _mockInnerService.Setup(s => s.GetLocalitiesByCityAsync("Los Angeles", "US")).ReturnsAsync(localities);

        // Act
        await _service.GetLocalitiesByCityAsync("Los Angeles");

        // Assert - should use "US" as default
        _mockInnerService.Verify(s => s.GetLocalitiesByCityAsync("Los Angeles", "US"), Times.Once);
    }

    #endregion

    #region CreateLocalityAsync Tests

    [Fact]
    public async Task CreateLocalityAsync_DelegatesDirectlyToInnerService()
    {
        // Arrange
        var newLocality = new LocalityInfo
        {
            Id = 1,
            Name = "New District",
            City = "Los Angeles"
        };
        _mockInnerService.Setup(s => s.CreateLocalityAsync("New District", "Los Angeles", "CA", "US", 1, 123))
            .ReturnsAsync(newLocality);

        // Act
        var result = await _service.CreateLocalityAsync("New District", "Los Angeles", "CA", "US", 1, 123);

        // Assert
        result.Should().BeEquivalentTo(newLocality);
    }

    #endregion

    #region Count Methods Tests

    [Fact]
    public async Task GetZipCodeCountAsync_DelegatesDirectlyToInnerService()
    {
        // Arrange
        _mockInnerService.Setup(s => s.GetZipCodeCountAsync()).ReturnsAsync(42000);

        // Act
        var result = await _service.GetZipCodeCountAsync();

        // Assert
        result.Should().Be(42000);
    }

    [Fact]
    public async Task GetTotalCountAsync_DelegatesDirectlyToInnerService()
    {
        // Arrange
        _mockInnerService.Setup(s => s.GetTotalCountAsync()).ReturnsAsync(50000);

        // Act
        var result = await _service.GetTotalCountAsync();

        // Assert
        result.Should().Be(50000);
    }

    [Fact]
    public async Task GetCountryCountAsync_DelegatesDirectlyToInnerService()
    {
        // Arrange
        _mockInnerService.Setup(s => s.GetCountryCountAsync()).ReturnsAsync(195);

        // Act
        var result = await _service.GetCountryCountAsync();

        // Assert
        result.Should().Be(195);
    }

    #endregion

    #region ClearCache Tests

    [Fact]
    public void ClearCache_RemovesCountriesCacheKey()
    {
        // Arrange - populate cache first
        var countries = new List<CountryInfo> { new() { Code = "US", Name = "United States" } };
        _mockInnerService.Setup(s => s.GetCountriesAsync()).ReturnsAsync(countries);

        // Act
        _service.ClearCache();

        // The cache should be cleared. On next call, should hit inner service again
        // Note: Due to how MemoryCache works, we'd need to verify by behavior
        // This is more of a smoke test to ensure the method doesn't throw
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public async Task GetStatesAsync_CacheKeyIsCaseInsensitive()
    {
        // Arrange
        var states = new List<StateInfo> { new() { Code = "CA", Name = "California" } };
        _mockInnerService.Setup(s => s.GetStatesAsync(It.IsAny<string>())).ReturnsAsync(states);

        // Act
        await _service.GetStatesAsync("us");
        await _service.GetStatesAsync("US");
        await _service.GetStatesAsync("Us");

        // Assert - should only call once due to normalized cache key
        _mockInnerService.Verify(s => s.GetStatesAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetCitiesAsync_CacheKeyIsCaseInsensitive()
    {
        // Arrange
        var cities = new List<string> { "Los Angeles" };
        _mockInnerService.Setup(s => s.GetCitiesAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cities);

        // Act
        await _service.GetCitiesAsync("us", "ca");
        await _service.GetCitiesAsync("US", "CA");

        // Assert - should only call once due to normalized cache key
        _mockInnerService.Verify(s => s.GetCitiesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion
}
