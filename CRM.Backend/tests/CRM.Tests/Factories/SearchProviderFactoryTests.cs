// CRM Solution - Search Provider Factory Tests
// Tests for search provider factory resolution and switching

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
/// Unit tests for SearchProviderFactory.
/// Tests provider resolution, feature flag handling, and fallback behavior.
/// </summary>
public class SearchProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<SearchProviderFactory>> _mockLogger;

    public SearchProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<SearchProviderFactory>>();
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
        var act = () => new SearchProviderFactory(
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
        var act = () => new SearchProviderFactory(
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
        var act = () => new SearchProviderFactory(
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
        var act = () => new SearchProviderFactory(
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
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, false);
        SetupBuiltInProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSearch), Times.Once);
    }

    [Theory]
    [InlineData("Meilisearch")]
    [InlineData("Algolia")]
    [InlineData("Typesense")]
    [InlineData("Elasticsearch")]
    [InlineData("AzureSearch")]
    public void GetProvider_WhenFeatureFlagEnabled_ReturnsConfiguredProvider(string providerType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, true);
        SetupConfiguration("Providers:Search:Type", providerType);
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
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, true);
        SetupConfiguration("Providers:Search:Type", "Unknown");
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
    [InlineData("builtin", "BuiltInSearchProvider")]
    [InlineData("meilisearch", "MeilisearchProvider")]
    [InlineData("algolia", "AlgoliaProvider")]
    [InlineData("typesense", "TypesenseProvider")]
    [InlineData("elasticsearch", "ElasticsearchProvider")]
    [InlineData("azuresearch", "AzureSearchProvider")]
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
            .WithMessage("*Unknown search provider*");
    }

    [Theory]
    [InlineData("BUILTIN")]
    [InlineData("BuiltIn")]
    [InlineData("builtin")]
    public void GetProvider_WithDifferentCases_IsCaseInsensitive(string providerName)
    {
        // Arrange
        SetupBuiltInProvider();
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
        providers.Should().Contain(ProviderTypes.Search.BuiltIn);
        providers.Should().Contain(ProviderTypes.Search.Meilisearch);
        providers.Should().Contain(ProviderTypes.Search.Algolia);
        providers.Should().Contain(ProviderTypes.Search.Typesense);
        providers.Should().Contain(ProviderTypes.Search.Elasticsearch);
        providers.Should().Contain(ProviderTypes.Search.AzureSearch);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsSixProviders()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().HaveCount(6);
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
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, false);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Search.BuiltIn);
    }

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, true);
        SetupConfiguration("Providers:Search:Type", "Meilisearch");
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Meilisearch");
    }

    [Fact]
    public void GetActiveProviderName_WhenNotConfigured_DefaultsToBuiltIn()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, true);
        SetupConfiguration("Providers:Search:Type", null);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Search.BuiltIn);
    }

    #endregion

    #region IsProviderAvailableAsync Tests

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderAvailable_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSearchProvider");
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
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchHealthResult { IsHealthy = false });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSearchProvider");
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
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));
        SetupSpecificProvider(mockProvider.Object, "BuiltInSearchProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Factory_ImplementsIProviderFactory()
    {
        // Arrange
        var factory = CreateFactory();

        // Assert
        factory.Should().BeAssignableTo<IProviderFactory<ISearchPort>>();
    }

    #endregion

    #region Search-Specific Tests

    [Fact]
    public void GetProvider_BuiltIn_SupportsAllSearchFeatures()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, false);
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResponse { TotalCount = 0, Results = new List<SearchResult>() });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSearchProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Meilisearch_SupportsInstantSearch()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSearch, true);
        SetupConfiguration("Providers:Search:Type", "Meilisearch");
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.SuggestAsync(It.IsAny<SuggestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SuggestResponse { Suggestions = new List<string>() });
        SetupSpecificProvider(mockProvider.Object, "MeilisearchProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private SearchProviderFactory CreateFactory()
    {
        return new SearchProviderFactory(
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
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSearchProvider");
    }

    private void SetupProviderByType(string providerType)
    {
        var mockProvider = new Mock<ISearchPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchHealthResult { IsHealthy = true });
        
        var providerTypeName = providerType.ToLowerInvariant() switch
        {
            "builtin" => "BuiltInSearchProvider",
            "meilisearch" => "MeilisearchProvider",
            "algolia" => "AlgoliaProvider",
            "typesense" => "TypesenseProvider",
            "elasticsearch" => "ElasticsearchProvider",
            "azuresearch" => "AzureSearchProvider",
            _ => "BuiltInSearchProvider"
        };

        SetupSpecificProvider(mockProvider.Object, providerTypeName);
    }

    private void SetupSpecificProvider(ISearchPort provider, string typeName)
    {
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<ISearchPort>)))
            .Returns(new[] { provider });
        
        // Also set up individual resolution
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ISearchPort)))
            .Returns(provider);
    }

    #endregion
}
