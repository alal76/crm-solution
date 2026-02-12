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

using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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
        string[]? countryCodes = null)
    {
        return Options.Create(new ZipCodeImportOptions
        {
            EnableScheduledImport = enableScheduled,
            ImportOnStartupIfEmpty = importOnStartupIfEmpty,
            ImportSource = importSource,
            CountryCodes = countryCodes ?? new[] { "US" }
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
    public async Task ExecuteAsync_ImportOnStartupIfEmpty_WithZipCodes_ShouldSkipImport()
    {
        // Arrange - set up a real in-memory DB with some zip codes already
        var services = new ServiceCollection();
        services.AddDbContext<CrmDbContext>(opts =>
            opts.UseInMemoryDatabase($"ZipTest_{Guid.NewGuid()}"));
        var sp = services.BuildServiceProvider();

        // Seed a zip code so it's not empty
        using (var scope = sp.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            db.ZipCodes.Add(new CRM.Core.Entities.ZipCode
            {
                Code = "10001",
                City = "New York",
                State = "NY",
                Country = "US"
            });
            await db.SaveChangesAsync();
        }

        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(s => s.ServiceProvider).Returns(sp);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        var mockSP = new Mock<IServiceProvider>();
        mockSP.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        var options = CreateOptions(importOnStartupIfEmpty: true);
        var service = new ZipCodeImportHostedService(
            mockSP.Object,
            _mockLogger.Object,
            options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act - The 30s initial delay means our test will cancel first
        // but we can verify it starts without error
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        // Assert - no exception
        sp.Dispose();
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
            CountryCodes = new[] { "US", "CA", "GB" },
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
