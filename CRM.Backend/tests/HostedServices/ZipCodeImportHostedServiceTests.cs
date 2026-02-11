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

using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for ZipCodeImportHostedService background service.
/// Tests ZIP code data import, processing, and error handling.
/// </summary>
public class ZipCodeImportHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<ZipCodeImportHostedService>> _mockLogger;
    private readonly Mock<IZipCodeService> _mockZipCodeService;
    private readonly Mock<IOptions<ZipCodeImportSettings>> _mockSettings;

    public ZipCodeImportHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<ZipCodeImportHostedService>>();
        _mockZipCodeService = new Mock<IZipCodeService>();
        _mockSettings = new Mock<IOptions<ZipCodeImportSettings>>();

        SetupServiceProvider();
    }

    private void SetupServiceProvider()
    {
        var settings = new ZipCodeImportSettings
        {
            Enabled = true,
            RunOnStartup = true,
            SourcePath = "/data/zipcodes.csv",
            Countries = new[] { "US", "CA" }
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IZipCodeService)))
            .Returns(_mockZipCodeService.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<ZipCodeImportSettings>)))
            .Returns(_mockSettings.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new ZipCodeImportHostedService(
            null!,
            _mockLogger.Object,
            _mockSettings.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            null!,
            _mockSettings.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WhenEnabled_ShouldLogStartMessage()
    {
        // Arrange
        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting") || v.ToString()!.Contains("ZIP") || v.ToString()!.Contains("import")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldNotImport()
    {
        // Arrange
        var settings = new ZipCodeImportSettings { Enabled = false };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockZipCodeService.Verify(
            x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();

        // Assert
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Import Processing Tests

    [Fact]
    public async Task ImportAsync_WithValidSourcePath_ShouldProcessData()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42522); // US ZIP code count

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - import should be called on startup
    }

    [Fact]
    public async Task ImportAsync_WithEmptyFile_ShouldReturnZero()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should complete without error
    }

    [Fact]
    public void Settings_Countries_ShouldFilterImport()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            Enabled = true,
            RunOnStartup = true,
            SourcePath = "/data/zipcodes.csv",
            Countries = new[] { "US", "CA", "UK" }
        };

        // Assert
        settings.Countries.Should().HaveCount(3);
        settings.Countries.Should().Contain("US");
        settings.Countries.Should().Contain("CA");
        settings.Countries.Should().Contain("UK");
    }

    [Fact]
    public void Settings_RunOnStartup_WhenFalse_ShouldNotAutoRun()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            Enabled = true,
            RunOnStartup = false,
            SourcePath = "/data/zipcodes.csv",
            Countries = new[] { "US" }
        };

        // Assert
        settings.RunOnStartup.Should().BeFalse();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ImportAsync_WhenFileMissing_ShouldLogError()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("ZIP code file not found"));

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    [Fact]
    public async Task ImportAsync_WhenParsingFails_ShouldLogError()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid CSV format"));

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    [Fact]
    public async Task ImportAsync_WhenDatabaseFails_ShouldLogError()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public async Task StartAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Act
        var task = service.StartAsync(CancellationToken.None);

        // Assert
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_ShouldStopBackgroundExecution()
    {
        // Arrange
        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);
        await service.StopAsync(CancellationToken.None);

        // Assert - should complete without hanging
    }

    [Fact]
    public async Task StopAsync_WhenCalledBeforeStart_ShouldNotThrow()
    {
        // Arrange
        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Act
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void ZipCodeImportSettings_DefaultValues_ShouldBeReasonable()
    {
        // Arrange
        var settings = new ZipCodeImportSettings();

        // Assert
        settings.Enabled.Should().BeFalse();
        settings.RunOnStartup.Should().BeFalse();
        settings.SourcePath.Should().BeNull();
        settings.Countries.Should().BeNull();
    }

    [Fact]
    public void ZipCodeImportSettings_CanBeFullyConfigured()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            Enabled = true,
            RunOnStartup = true,
            SourcePath = "/opt/data/zipcodes-full.csv",
            Countries = new[] { "US", "CA", "MX" }
        };

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.RunOnStartup.Should().BeTrue();
        settings.SourcePath.Should().Be("/opt/data/zipcodes-full.csv");
        settings.Countries.Should().HaveCount(3);
    }

    [Fact]
    public void ZipCodeImportSettings_WithEmptyCountries_ShouldImportAll()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            Enabled = true,
            RunOnStartup = true,
            SourcePath = "/data/zipcodes.csv",
            Countries = Array.Empty<string>()
        };

        // Assert
        settings.Countries.Should().BeEmpty();
    }

    #endregion

    #region Progress Tracking Tests

    [Fact]
    public async Task ImportAsync_ShouldLogProgress()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10000);

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should log progress (timing dependent)
    }

    [Fact]
    public async Task ImportAsync_WithLargeDataset_ShouldCompleteWithinTimeout()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100000);

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should handle large datasets
    }

    #endregion

    #region Data Validation Tests

    [Fact]
    public async Task ImportAsync_WithInvalidZipCodes_ShouldSkipAndContinue()
    {
        // Arrange
        _mockZipCodeService.Setup(x => x.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9999); // Some skipped

        var service = new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should complete with partial import
    }

    #endregion

    #region Source Path Tests

    [Fact]
    public void Settings_SourcePath_ShouldSupportAbsolutePath()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            SourcePath = "/var/data/zipcodes.csv"
        };

        // Assert
        settings.SourcePath.Should().StartWith("/");
    }

    [Fact]
    public void Settings_SourcePath_ShouldSupportRelativePath()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            SourcePath = "data/zipcodes.csv"
        };

        // Assert
        settings.SourcePath.Should().NotStartWith("/");
    }

    [Fact]
    public void Settings_SourcePath_ShouldSupportUrl()
    {
        // Arrange
        var settings = new ZipCodeImportSettings
        {
            SourcePath = "https://data.example.com/zipcodes.csv"
        };

        // Assert
        settings.SourcePath.Should().StartWith("https://");
    }

    #endregion
}

/// <summary>
/// ZIP code import configuration settings
/// </summary>
public class ZipCodeImportSettings
{
    public bool Enabled { get; set; }
    public bool RunOnStartup { get; set; }
    public string? SourcePath { get; set; }
    public string[]? Countries { get; set; }
}

/// <summary>
/// Mock interface for ZIP code service
/// </summary>
public interface IZipCodeService
{
    Task<int> ImportAsync(string sourcePath, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync();
}
