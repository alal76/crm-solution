// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for ProviderFeatureFlagCacheInitializer (AP-015).
/// Verifies that the hosted service correctly populates ProviderFeatureFlagCache
/// from IFeatureManager at startup, eliminating sync-over-async in provider factories.
/// </summary>
public class ProviderFeatureFlagCacheInitializerTests
{
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly ProviderFeatureFlagCache _cache;
    private readonly Mock<ILogger<ProviderFeatureFlagCacheInitializer>> _loggerMock;

    public ProviderFeatureFlagCacheInitializerTests()
    {
        _featureManagerMock = new Mock<IFeatureManager>();
        _cache = new ProviderFeatureFlagCache();
        _loggerMock = new Mock<ILogger<ProviderFeatureFlagCacheInitializer>>();
    }

    [Fact]
    public async Task StartAsync_ShouldPopulateAllFlags_WhenAllEnabled()
    {
        // Arrange — all flags return true
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSearch)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalChat)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalNotifications)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalAnalytics)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSignatures)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalAI)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalIntegrations)).ReturnsAsync(true);

        var sut = new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, _cache, _loggerMock.Object);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        _cache.UseExternalSearch.Should().BeTrue();
        _cache.UseExternalChat.Should().BeTrue();
        _cache.UseExternalNotifications.Should().BeTrue();
        _cache.UseExternalAnalytics.Should().BeTrue();
        _cache.UseExternalSignatures.Should().BeTrue();
        _cache.UseExternalAI.Should().BeTrue();
        _cache.UseExternalIntegrations.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_ShouldPopulateAllFlags_WhenAllDisabled()
    {
        // Arrange — all flags return false
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

        var sut = new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, _cache, _loggerMock.Object);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        _cache.UseExternalSearch.Should().BeFalse();
        _cache.UseExternalChat.Should().BeFalse();
        _cache.UseExternalNotifications.Should().BeFalse();
        _cache.UseExternalAnalytics.Should().BeFalse();
        _cache.UseExternalSignatures.Should().BeFalse();
        _cache.UseExternalAI.Should().BeFalse();
        _cache.UseExternalIntegrations.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_ShouldPopulateSelectively_WhenOnlySearchEnabled()
    {
        // Arrange — only Search is enabled
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSearch)).ReturnsAsync(true);

        var sut = new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, _cache, _loggerMock.Object);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        _cache.UseExternalSearch.Should().BeTrue();
        _cache.UseExternalChat.Should().BeFalse();
        _cache.UseExternalNotifications.Should().BeFalse();
        _cache.UseExternalAnalytics.Should().BeFalse();
        _cache.UseExternalSignatures.Should().BeFalse();
        _cache.UseExternalAI.Should().BeFalse();
        _cache.UseExternalIntegrations.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_ShouldCallIsEnabledAsyncForAllSevenFlags()
    {
        // Arrange
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        var sut = new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, _cache, _loggerMock.Object);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert — verify all seven flags were queried exactly once
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSearch), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalChat), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalNotifications), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalAnalytics), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSignatures), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalAI), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalIntegrations), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldComplete_WithoutModifyingCache()
    {
        // Arrange
        _cache.UseExternalSearch = true;
        var sut = new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, _cache, _loggerMock.Object);

        // Act
        await sut.StopAsync(CancellationToken.None);

        // Assert — cache unchanged
        _cache.UseExternalSearch.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFeatureManagerIsNull()
    {
        // Act & Assert
        var act = () => new ProviderFeatureFlagCacheInitializer(null!, _cache, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("featureManager");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCacheIsNull()
    {
        // Act & Assert
        var act = () => new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("cache");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        // Act & Assert
        var act = () => new ProviderFeatureFlagCacheInitializer(_featureManagerMock.Object, _cache, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}
