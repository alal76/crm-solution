// CRM Solution - Customer Relationship Management System
// ZipCode Import Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.IO;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ZipCodeImportService
/// Covers: ZIP code import, validation, batch processing
/// </summary>
public class ZipCodeImportServiceTests
{
    private readonly Mock<IRepository<ZipCode>> _mockZipCodeRepository;
    private readonly Mock<IRepository<Locality>> _mockLocalityRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ZipCodeImportService>> _mockLogger;
    private readonly ZipCodeImportService _service;

    public ZipCodeImportServiceTests()
    {
        _mockZipCodeRepository = new Mock<IRepository<ZipCode>>();
        _mockLocalityRepository = new Mock<IRepository<Locality>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ZipCodeImportService>>();

        _service = new ZipCodeImportService(
            _mockZipCodeRepository.Object,
            _mockLocalityRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Import Tests

    [Fact]
    public async Task ImportFromCsvAsync_ValidCsv_ImportsRecords()
    {
        // Arrange
        var csvContent = "zip,city,state,county,latitude,longitude\n10001,New York,NY,New York,40.7128,-74.0060\n10002,New York,NY,New York,40.7157,-73.9863";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        _mockZipCodeRepository.Setup(r => r.AddAsync(It.IsAny<ZipCode>()))
            .ReturnsAsync((ZipCode z) => { z.Id = 1; return z; });

        // Act
        var result = await _service.ImportFromCsvAsync(stream);

        // Assert
        result.ImportedCount.Should().Be(2);
        result.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportFromCsvAsync_InvalidRows_SkipsInvalid()
    {
        // Arrange
        var csvContent = "zip,city,state,county,latitude,longitude\n10001,New York,NY,New York,40.7128,-74.0060\n,Invalid,,,,";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        _mockZipCodeRepository.Setup(r => r.AddAsync(It.IsAny<ZipCode>()))
            .ReturnsAsync((ZipCode z) => { z.Id = 1; return z; });

        // Act
        var result = await _service.ImportFromCsvAsync(stream);

        // Assert
        result.ImportedCount.Should().Be(1);
        result.FailedCount.Should().Be(1);
    }

    [Fact]
    public async Task ImportFromCsvAsync_EmptyCsv_ReturnsZero()
    {
        // Arrange
        var csvContent = "zip,city,state,county,latitude,longitude\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _service.ImportFromCsvAsync(stream);

        // Assert
        result.ImportedCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportFromCsvAsync_DuplicateZips_UpdatesExisting()
    {
        // Arrange
        var csvContent = "zip,city,state,county,latitude,longitude\n10001,New York Updated,NY,New York,40.7128,-74.0060";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        var existing = new ZipCode { Id = 1, Code = "10001", City = "New York" };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(new List<ZipCode> { existing });

        _mockZipCodeRepository.Setup(r => r.UpdateAsync(It.IsAny<ZipCode>()))
            .ReturnsAsync((ZipCode z) => z);

        // Act
        var result = await _service.ImportFromCsvAsync(stream, updateExisting: true);

        // Assert
        result.UpdatedCount.Should().Be(1);
    }

    #endregion

    #region Batch Import Tests

    [Fact]
    public async Task ImportBatchAsync_ValidBatch_ImportsAll()
    {
        // Arrange
        var batch = new List<ZipCodeImportDto>
        {
            new ZipCodeImportDto { Code = "10001", City = "New York", State = "NY" },
            new ZipCodeImportDto { Code = "10002", City = "New York", State = "NY" }
        };

        _mockZipCodeRepository.Setup(r => r.AddAsync(It.IsAny<ZipCode>()))
            .ReturnsAsync((ZipCode z) => { z.Id = 1; return z; });

        // Act
        var result = await _service.ImportBatchAsync(batch);

        // Assert
        result.ImportedCount.Should().Be(2);
    }

    [Fact]
    public async Task ImportBatchAsync_LargeBatch_ProcessesInChunks()
    {
        // Arrange
        var batch = Enumerable.Range(10000, 1000).Select(i => new ZipCodeImportDto
        {
            Code = i.ToString(),
            City = $"City {i}",
            State = "NY"
        }).ToList();

        _mockZipCodeRepository.Setup(r => r.AddAsync(It.IsAny<ZipCode>()))
            .ReturnsAsync((ZipCode z) => { z.Id = 1; return z; });

        // Act
        var result = await _service.ImportBatchAsync(batch, batchSize: 100);

        // Assert
        result.ImportedCount.Should().Be(1000);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ValidateZipCode_ValidUSZip_ReturnsTrue()
    {
        // Act
        var result = _service.ValidateZipCode("10001", "US");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateZipCode_InvalidUSZip_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateZipCode("1234", "US");

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateZipCode_ValidCanadianZip_ReturnsTrue()
    {
        // Act
        var result = _service.ValidateZipCode("M5V 3A8", "CA");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateZipCode_ValidUKPostcode_ReturnsTrue()
    {
        // Act
        var result = _service.ValidateZipCode("SW1A 1AA", "UK");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "US")]
    [InlineData(null, "US")]
    [InlineData("   ", "US")]
    public void ValidateZipCode_EmptyInput_ReturnsFalse(string code, string country)
    {
        // Act
        var result = _service.ValidateZipCode(code, country);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task ExportToCsvAsync_ValidData_ExportsCsv()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Code = "10001", City = "New York", State = "NY" },
            new ZipCode { Code = "10002", City = "New York", State = "NY" }
        };

        _mockZipCodeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.ExportToCsvAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("10001");
    }

    [Fact]
    public async Task ExportByStateAsync_ValidState_ExportsState()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Code = "10001", City = "New York", State = "NY" },
            new ZipCode { Code = "10002", City = "New York", State = "NY" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.ExportByStateAsync("NY");

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteByStateAsync_ValidState_DeletesRecords()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1, State = "NY" },
            new ZipCode { Id = 2, State = "NY" }
        };

        _mockZipCodeRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ZipCode, bool>>>()))
            .ReturnsAsync(zipCodes);

        _mockZipCodeRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteByStateAsync("NY");

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task DeleteAllAsync_DeletesAllRecords()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { Id = 1 },
            new ZipCode { Id = 2 }
        };

        _mockZipCodeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(zipCodes);

        _mockZipCodeRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAllAsync();

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region Locality Import Tests

    [Fact]
    public async Task ImportLocalitiesAsync_ValidData_ImportsLocalities()
    {
        // Arrange
        var localities = new List<LocalityImportDto>
        {
            new LocalityImportDto { Name = "Manhattan", Type = "Borough", State = "NY" },
            new LocalityImportDto { Name = "Brooklyn", Type = "Borough", State = "NY" }
        };

        _mockLocalityRepository.Setup(r => r.AddAsync(It.IsAny<Locality>()))
            .ReturnsAsync((Locality l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.ImportLocalitiesAsync(localities);

        // Assert
        result.ImportedCount.Should().Be(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var zipCodes = new List<ZipCode>
        {
            new ZipCode { State = "NY" },
            new ZipCode { State = "NY" },
            new ZipCode { State = "CA" }
        };

        _mockZipCodeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(zipCodes);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalRecords.Should().Be(3);
        result.StateCount.Should().Be(2);
    }

    [Fact]
    public async Task GetImportHistoryAsync_ReturnsHistory()
    {
        // Act
        var result = await _service.GetImportHistoryAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Preview Tests

    [Fact]
    public async Task PreviewImportAsync_ValidFile_ReturnsPreview()
    {
        // Arrange
        var csvContent = "zip,city,state,county,latitude,longitude\n10001,New York,NY,New York,40.7128,-74.0060";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _service.PreviewImportAsync(stream, 10);

        // Assert
        result.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task PreviewImportAsync_WithErrors_ShowsErrors()
    {
        // Arrange
        var csvContent = "zip,city,state,county,latitude,longitude\n10001,New York,NY,New York,40.7128,-74.0060\n,Invalid,,,,";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _service.PreviewImportAsync(stream, 10);

        // Assert
        result.ValidRows.Should().Be(1);
        result.InvalidRows.Should().Be(1);
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
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class LocalityImportDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
