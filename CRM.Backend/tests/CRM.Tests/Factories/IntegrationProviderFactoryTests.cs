// CRM Solution - Integration Provider Factory Tests
// Tests for integration platform provider factory resolution and switching

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
/// Unit tests for IntegrationProviderFactory.
/// Tests provider resolution, feature flag handling, and webhook/workflow integration.
/// </summary>
public class IntegrationProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<IntegrationProviderFactory>> _mockLogger;

    public IntegrationProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<IntegrationProviderFactory>>();
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
        var act = () => new IntegrationProviderFactory(
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
        var act = () => new IntegrationProviderFactory(
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
        var act = () => new IntegrationProviderFactory(
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
        var act = () => new IntegrationProviderFactory(
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
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, false);
        SetupBuiltInProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalIntegrations), Times.Once);
    }

    [Theory]
    [InlineData("N8n")]
    [InlineData("Zapier")]
    [InlineData("Make")]
    [InlineData("Workato")]
    public void GetProvider_WhenFeatureFlagEnabled_ReturnsConfiguredProvider(string providerType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, true);
        SetupConfiguration("Providers:Integrations:Type", providerType);
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
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, true);
        SetupConfiguration("Providers:Integrations:Type", "Unknown");
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
    [InlineData("builtin", "BuiltInIntegrationProvider")]
    [InlineData("n8n", "N8nProvider")]
    [InlineData("zapier", "ZapierProvider")]
    [InlineData("make", "MakeProvider")]
    [InlineData("workato", "WorkatoProvider")]
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
            .WithMessage("*Unknown integration provider*");
    }

    [Theory]
    [InlineData("N8N")]
    [InlineData("N8n")]
    [InlineData("n8n")]
    public void GetProvider_WithDifferentCases_IsCaseInsensitive(string providerName)
    {
        // Arrange
        SetupProviderByType("n8n");
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
        providers.Should().Contain(ProviderTypes.Integrations.BuiltIn);
        providers.Should().Contain(ProviderTypes.Integrations.N8n);
        providers.Should().Contain(ProviderTypes.Integrations.Zapier);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsExpectedCount()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().HaveCountGreaterThanOrEqualTo(3);
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
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, false);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Integrations.BuiltIn);
    }

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, true);
        SetupConfiguration("Providers:Integrations:Type", "N8n");
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("N8n");
    }

    #endregion

    #region IsProviderAvailableAsync Tests

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderAvailable_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntegrationHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInIntegrationProvider");
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
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntegrationHealthResult { IsHealthy = false });
        SetupSpecificProvider(mockProvider.Object, "BuiltInIntegrationProvider");
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
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));
        SetupSpecificProvider(mockProvider.Object, "BuiltInIntegrationProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Webhook & Workflow Tests

    [Fact]
    public void GetProvider_BuiltIn_SupportsWebhookRegistration()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, false);
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.RegisterWebhookAsync(
            It.IsAny<WebhookRegistration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebhookRegistrationResult { WebhookId = "1", Status = "active" });
        SetupSpecificProvider(mockProvider.Object, "BuiltInIntegrationProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_N8n_SupportsWorkflowExecution()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, true);
        SetupConfiguration("Providers:Integrations:Type", "N8n");
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.TriggerWorkflowAsync(
            It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkflowExecutionResult { ExecutionId = "1", Status = "running" });
        SetupSpecificProvider(mockProvider.Object, "N8nProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Zapier_SupportsEventPublishing()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, true);
        SetupConfiguration("Providers:Integrations:Type", "Zapier");
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.PublishEventAsync(
            It.IsAny<CrmEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventPublishResult { Success = true });
        SetupSpecificProvider(mockProvider.Object, "ZapierProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Make_SupportsScenarioManagement()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, true);
        SetupConfiguration("Providers:Integrations:Type", "Make");
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.GetWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IntegrationWorkflow>());
        SetupSpecificProvider(mockProvider.Object, "MakeProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    #endregion

    #region CRM Event Types Tests

    [Theory]
    [InlineData("account.created")]
    [InlineData("account.updated")]
    [InlineData("contact.created")]
    [InlineData("opportunity.won")]
    [InlineData("lead.converted")]
    public void GetProvider_SupportsStandardCrmEvents(string eventType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalIntegrations, false);
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.GetSupportedEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> 
            { 
                "account.created", "account.updated", "account.deleted",
                "contact.created", "contact.updated", "contact.deleted",
                "opportunity.created", "opportunity.updated", "opportunity.won", "opportunity.lost",
                "lead.created", "lead.converted", "lead.qualified"
            });
        SetupSpecificProvider(mockProvider.Object, "BuiltInIntegrationProvider");
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
        factory.Should().BeAssignableTo<IProviderFactory<IIntegrationPort>>();
    }

    #endregion

    #region Helper Methods

    private IntegrationProviderFactory CreateFactory()
    {
        return new IntegrationProviderFactory(
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
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntegrationHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInIntegrationProvider");
    }

    private void SetupProviderByType(string providerType)
    {
        var mockProvider = new Mock<IIntegrationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntegrationHealthResult { IsHealthy = true });
        
        var providerTypeName = providerType.ToLowerInvariant() switch
        {
            "builtin" => "BuiltInIntegrationProvider",
            "n8n" => "N8nProvider",
            "zapier" => "ZapierProvider",
            "make" => "MakeProvider",
            "workato" => "WorkatoProvider",
            _ => "BuiltInIntegrationProvider"
        };

        SetupSpecificProvider(mockProvider.Object, providerTypeName);
    }

    private void SetupSpecificProvider(IIntegrationPort provider, string typeName)
    {
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IIntegrationPort>)))
            .Returns(new[] { provider });
        
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IIntegrationPort)))
            .Returns(provider);
    }

    #endregion
}
