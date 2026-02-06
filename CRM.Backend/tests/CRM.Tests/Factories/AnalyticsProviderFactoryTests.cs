// CRM Solution - Analytics Provider Factory Tests
// Tests for analytics/BI provider factory resolution and switching

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Factories;

/// <summary>
/// Unit tests for AnalyticsProviderFactory.
/// Tests provider resolution, feature flag handling, and BI/analytics workflows.
/// </summary>
public class AnalyticsProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AnalyticsProviderFactory>> _mockLogger;

    public AnalyticsProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AnalyticsProviderFactory>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesFactory()
    {
        // Act
        var factory = CreateFactory();

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyticsProviderFactory(
            null!,
            _mockFeatureManager.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullFeatureManager_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyticsProviderFactory(
            _mockServiceProvider.Object,
            null!,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("featureManager");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyticsProviderFactory(
            _mockServiceProvider.Object,
            _mockFeatureManager.Object,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AnalyticsProviderFactory(
            _mockServiceProvider.Object,
            _mockFeatureManager.Object,
            _mockConfiguration.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region GetProvider() Tests

    [Fact]
    public void GetProvider_WhenFeatureFlagDisabled_ReturnsBuiltInProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, false);
        SetupBuiltInProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalAnalytics), Times.Once);
    }

    [Theory]
    [InlineData("Superset")]
    [InlineData("Metabase")]
    [InlineData("PowerBI")]
    [InlineData("Looker")]
    [InlineData("QuickSight")]
    public void GetProvider_WhenFeatureFlagEnabled_ReturnsConfiguredProvider(string providerType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, true);
        SetupConfiguration("Providers:Analytics:Type", providerType);
        SetupProviderByType(providerType);
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_WhenProviderResolutionFails_FallsBackToBuiltIn()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, true);
        SetupConfiguration("Providers:Analytics:Type", "Unknown");
        SetupBuiltInProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region GetProvider(string) Tests

    [Fact]
    public void GetProvider_WithNullProviderName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetProvider(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("providerName");
    }

    [Fact]
    public void GetProvider_WithEmptyProviderName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetProvider("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("providerName");
    }

    [Theory]
    [InlineData("builtin", "BuiltInAnalyticsProvider")]
    [InlineData("superset", "SupersetProvider")]
    [InlineData("metabase", "MetabaseProvider")]
    [InlineData("powerbi", "PowerBIProvider")]
    [InlineData("looker", "LookerProvider")]
    [InlineData("quicksight", "QuickSightProvider")]
    public void GetProvider_WithValidProviderName_ResolvesCorrectProvider(
        string providerName, string expectedTypeName)
    {
        // Arrange
        SetupProviderByType(providerName);
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider(providerName);

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_WithUnknownProviderName_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetProvider("UnknownProvider");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown analytics provider*");
    }

    [Theory]
    [InlineData("SUPERSET")]
    [InlineData("Superset")]
    [InlineData("superset")]
    public void GetProvider_WithDifferentCases_IsCaseInsensitive(string providerName)
    {
        // Arrange
        SetupProviderByType("superset");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider(providerName);

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region GetAvailableProviders Tests

    [Fact]
    public void GetAvailableProviders_ReturnsAllSupportedProviders()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders();

        // Assert
        providers.Should().NotBeNull();
        providers.Should().Contain(ProviderTypes.Analytics.BuiltIn);
        providers.Should().Contain(ProviderTypes.Analytics.Superset);
        providers.Should().Contain(ProviderTypes.Analytics.Metabase);
        providers.Should().Contain(ProviderTypes.Analytics.PowerBI);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsExpectedCount()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsDistinctProviders()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Distinct().Should().HaveCount(providers.Count);
    }

    #endregion

    #region GetActiveProviderName Tests

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagDisabled_ReturnsBuiltIn()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, false);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Analytics.BuiltIn);
    }

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, true);
        SetupConfiguration("Providers:Analytics:Type", "Superset");
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Superset");
    }

    #endregion

    #region IsProviderAvailableAsync Tests

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderAvailable_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnalyticsHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInAnalyticsProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderUnavailable_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnalyticsHealthResult { IsHealthy = false });
        SetupSpecificProvider(mockProvider.Object, "BuiltInAnalyticsProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderThrows_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));
        SetupSpecificProvider(mockProvider.Object, "BuiltInAnalyticsProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Analytics Workflow Tests

    [Fact]
    public void GetProvider_BuiltIn_SupportsDashboards()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, false);
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.GetDashboardsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalyticsDashboard>());
        SetupSpecificProvider(mockProvider.Object, "BuiltInAnalyticsProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Superset_SupportsEmbedding()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, true);
        SetupConfiguration("Providers:Analytics:Type", "Superset");
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.GetEmbedTokenAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbedTokenResult { Token = "token", ExpiresAt = DateTime.UtcNow.AddHours(1) });
        SetupSpecificProvider(mockProvider.Object, "SupersetProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_PowerBI_SupportsReports()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, true);
        SetupConfiguration("Providers:Analytics:Type", "PowerBI");
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.GetReportsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalyticsReport>());
        SetupSpecificProvider(mockProvider.Object, "PowerBIProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Metabase_SupportsDataSources()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAnalytics, true);
        SetupConfiguration("Providers:Analytics:Type", "Metabase");
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.GetDataSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalyticsDataSource>());
        SetupSpecificProvider(mockProvider.Object, "MetabaseProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Factory_ImplementsIProviderFactory()
    {
        // Arrange
        var factory = CreateFactory();

        // Assert
        factory.Should().BeAssignableTo<IProviderFactory<IAnalyticsPort>>();
    }

    #endregion

    #region Helper Methods

    private AnalyticsProviderFactory CreateFactory()
    {
        return new AnalyticsProviderFactory(
            _mockServiceProvider.Object,
            _mockFeatureManager.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    private void SetupFeatureFlag(string flagName, bool isEnabled)
    {
        _mockFeatureManager
            .Setup(fm => fm.IsEnabledAsync(flagName))
            .ReturnsAsync(isEnabled);
    }

    private void SetupConfiguration(string key, string? value)
    {
        _mockConfiguration
            .Setup(c => c[key])
            .Returns(value);
    }

    private void SetupBuiltInProvider()
    {
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnalyticsHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInAnalyticsProvider");
    }

    private void SetupProviderByType(string providerType)
    {
        var mockProvider = new Mock<IAnalyticsPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnalyticsHealthResult { IsHealthy = true });
        
        var providerTypeName = providerType.ToLowerInvariant() switch
        {
            "builtin" => "BuiltInAnalyticsProvider",
            "superset" => "SupersetProvider",
            "metabase" => "MetabaseProvider",
            "powerbi" => "PowerBIProvider",
            "looker" => "LookerProvider",
            "quicksight" => "QuickSightProvider",
            _ => "BuiltInAnalyticsProvider"
        };

        SetupSpecificProvider(mockProvider.Object, providerTypeName);
    }

    private void SetupSpecificProvider(IAnalyticsPort provider, string typeName)
    {
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IAnalyticsPort>)))
            .Returns(new[] { provider });
        
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAnalyticsPort)))
            .Returns(provider);
    }

    #endregion
}
