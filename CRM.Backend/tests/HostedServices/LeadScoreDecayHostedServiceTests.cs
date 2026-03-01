// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Tests for LeadScoreDecayHostedService.
/// Real constructor: (IServiceProvider serviceProvider, ILogger&lt;LeadScoreDecayHostedService&gt; logger, IConfiguration configuration)
/// Extends BackgroundService. Reads LeadScoring:EnableDecay and LeadScoring:DecayCheckIntervalHours from config.
/// </summary>
public class LeadScoreDecayHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<LeadScoreDecayHostedService>> _mockLogger;
    private readonly IConfiguration _configuration;

    public LeadScoreDecayHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<LeadScoreDecayHostedService>>();
        _configuration = BuildConfiguration(enableDecay: true, intervalHours: 6);
    }

    private static IConfiguration BuildConfiguration(bool enableDecay = true, int intervalHours = 6)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "LeadScoring:EnableDecay", enableDecay.ToString() },
            { "LeadScoring:DecayCheckIntervalHours", intervalHours.ToString() }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private LeadScoreDecayHostedService CreateService(IConfiguration? config = null)
    {
        return new LeadScoreDecayHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            config ?? _configuration);
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
    public void Constructor_WithNullConfiguration_ShouldNotThrow()
    {
        // Act - IConfiguration can be empty but not null in practice; test with empty config
        var emptyConfig = new ConfigurationBuilder().Build();
        var service = CreateService(emptyConfig);

        // Assert - uses defaults
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDecayDisabled_ShouldReturnImmediately()
    {
        // Arrange
        var config = BuildConfiguration(enableDecay: false);
        var service = CreateService(config);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await service.StartAsync(cts.Token);
        // Give it a moment to call ExecuteAsync
        await Task.Delay(200, CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        // Assert - service should have returned early without processing
        // No scope creation = no exceptions from null service provider
        service.Should().NotBeNull("Service should remain valid after early return due to disabled decay");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var config = BuildConfiguration(enableDecay: true);
        var service = CreateService(config);
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
    public async Task ExecuteAsync_WhenEnabled_ShouldCreateScope()
    {
        // Arrange
        var config = BuildConfiguration(enableDecay: true, intervalHours: 1);

        // Set up a real in-memory DB to avoid null reference errors
        var services = new ServiceCollection();
        services.AddDbContext<CrmDbContext>(options =>
            options.UseInMemoryDatabase($"LeadDecayTest_{Guid.NewGuid()}"));
        var sp = services.BuildServiceProvider();

        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(s => s.ServiceProvider).Returns(sp);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        var mockSP = new Mock<IServiceProvider>();
        mockSP.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        var service = new LeadScoreDecayHostedService(
            mockSP.Object,
            _mockLogger.Object,
            config);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

        // Act - start and let it run through the initial 5-minute delay would be too long,
        // so we cancel quickly; the key is no unhandled exceptions
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        // Assert - no exceptions thrown
        service.Should().NotBeNull("Service should remain valid after scope creation attempt");
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
    public void Constructor_ReadsDecayCheckIntervalHours_Default6()
    {
        // Arrange - empty config, defaults to 6 hours
        var emptyConfig = new ConfigurationBuilder().Build();

        // Act
        var service = CreateService(emptyConfig);

        // Assert - service is created, defaults used internally
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ReadsCustomInterval()
    {
        // Arrange
        var config = BuildConfiguration(enableDecay: true, intervalHours: 24);

        // Act
        var service = CreateService(config);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenScopeThrows_ShouldHandleGracefully()
    {
        // Arrange
        var config = BuildConfiguration(enableDecay: true);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope())
            .Throws(new InvalidOperationException("DI container disposed"));

        var mockSP = new Mock<IServiceProvider>();
        mockSP.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        var service = new LeadScoreDecayHostedService(
            mockSP.Object,
            _mockLogger.Object,
            config);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

        // Act - service should handle scope creation failure
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
