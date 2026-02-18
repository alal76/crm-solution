// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Tests for ZipCodeImportHostedService.
/// Real constructor: (IServiceProvider serviceProvider, ILogger&lt;ZipCodeImportHostedService&gt; logger, IOptions&lt;ZipCodeImportOptions&gt; options)
/// Extends BackgroundService. Checks ImportOnStartupIfEmpty, then scheduled imports.
/// </summary>
public class ZipCodeImportHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<ZipCodeImportHostedService>> _mockLogger;

    public ZipCodeImportHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<ZipCodeImportHostedService>>();
    }

    private static IOptions<ZipCodeImportOptions> CreateOptions(
        bool enableScheduled = false,
        bool importOnStartupIfEmpty = false,
        string importSource = "GeoNames",
        List<string>? countryCodes = null)
    {
        return Options.Create(new ZipCodeImportOptions
        {
            EnableScheduledImport = enableScheduled,
            ImportOnStartupIfEmpty = importOnStartupIfEmpty,
            ImportSource = importSource,
            CountryCodes = countryCodes ?? new List<string> { "US" }
        });
    }

    private ZipCodeImportHostedService CreateService(IOptions<ZipCodeImportOptions>? options = null)
    {
        return new ZipCodeImportHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            options ?? CreateOptions());
    }

    [Fact]
    public void Constructor_ShouldAcceptValidParameters()
    {
        // Act
        var service = CreateService();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithDefaultOptions_ShouldNotThrow()
    {
        // Arrange - default options (everything disabled/default)
        var options = Options.Create(new ZipCodeImportOptions());

        // Act
        var service = CreateService(options);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_AllDisabled_ShouldNotCrash()
    {
        // Arrange
        var options = CreateOptions(enableScheduled: false, importOnStartupIfEmpty: false);
        var service = CreateService(options);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = CreateService();
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ImportOnStartupIfEmpty_ShouldStartWithoutError()
    {
        // Arrange - enable ImportOnStartupIfEmpty; service will try to create scope
        var options = CreateOptions(importOnStartupIfEmpty: true);
        var service = CreateService(options);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        // Act - service starts, initial delay means it won't reach import before cancel
        await service.StartAsync(cts.Token);
        await Task.Delay(600, CancellationToken.None);

        // Assert - no exception during startup
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);
        var act = () => service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ZipCodeImportOptions_Defaults_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ZipCodeImportOptions();

        // Assert
        options.EnableScheduledImport.Should().BeFalse();
        options.ImportOnStartupIfEmpty.Should().BeTrue();
        options.ImportSource.Should().Be("GeoNames");
        options.CountryCodes.Should().Contain("US");
    }

    [Fact]
    public void ZipCodeImportOptions_CanSetAllProperties()
    {
        // Arrange
        var options = new ZipCodeImportOptions
        {
            EnableScheduledImport = true,
            CronExpression = "0 0 * * 0",
            ImportSource = "GitHub",
            GitHubUrl = "https://example.com/data.csv",
            CountryCodes = new List<string> { "US", "CA", "GB" },
            ImportOnStartupIfEmpty = false,
            MinimumHoursBetweenImports = 48
        };

        // Assert
        options.EnableScheduledImport.Should().BeTrue();
        options.CronExpression.Should().Be("0 0 * * 0");
        options.ImportSource.Should().Be("GitHub");
        options.GitHubUrl.Should().Be("https://example.com/data.csv");
        options.CountryCodes.Should().HaveCount(3);
        options.ImportOnStartupIfEmpty.Should().BeFalse();
        options.MinimumHoursBetweenImports.Should().Be(48);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScopeCreationFails_ShouldHandleGracefully()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope())
            .Throws(new ObjectDisposedException("ServiceProvider"));

        var mockSP = new Mock<IServiceProvider>();
        mockSP.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        var options = CreateOptions(importOnStartupIfEmpty: true);
        var service = new ZipCodeImportHostedService(
            mockSP.Object,
            _mockLogger.Object,
            options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
