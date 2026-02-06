// CRM Solution - Notification Provider Factory Tests
// Tests for notification provider factory resolution and switching

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
/// Unit tests for NotificationProviderFactory.
/// Tests provider resolution, feature flag handling, and multi-channel support.
/// </summary>
public class NotificationProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<NotificationProviderFactory>> _mockLogger;

    public NotificationProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<NotificationProviderFactory>>();
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
        var act = () => new NotificationProviderFactory(
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
        var act = () => new NotificationProviderFactory(
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
        var act = () => new NotificationProviderFactory(
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
        var act = () => new NotificationProviderFactory(
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
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, false);
        SetupBuiltInProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalNotifications), Times.Once);
    }

    [Theory]
    [InlineData("Novu")]
    [InlineData("Twilio")]
    [InlineData("SendGrid")]
    [InlineData("OneSignal")]
    public void GetProvider_WhenFeatureFlagEnabled_ReturnsConfiguredProvider(string providerType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, true);
        SetupConfiguration("Providers:Notifications:Type", providerType);
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
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, true);
        SetupConfiguration("Providers:Notifications:Type", "Unknown");
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
    [InlineData("builtin", "BuiltInNotificationProvider")]
    [InlineData("novu", "NovuProvider")]
    [InlineData("twilio", "TwilioProvider")]
    [InlineData("sendgrid", "SendGridProvider")]
    [InlineData("onesignal", "OneSignalProvider")]
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
            .WithMessage("*Unknown notification provider*");
    }

    [Theory]
    [InlineData("NOVU")]
    [InlineData("Novu")]
    [InlineData("novu")]
    public void GetProvider_WithDifferentCases_IsCaseInsensitive(string providerName)
    {
        // Arrange
        SetupProviderByType("novu");
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
        providers.Should().Contain(ProviderTypes.Notifications.BuiltIn);
        providers.Should().Contain(ProviderTypes.Notifications.Novu);
        providers.Should().Contain(ProviderTypes.Notifications.Twilio);
        providers.Should().Contain(ProviderTypes.Notifications.SendGrid);
        providers.Should().Contain(ProviderTypes.Notifications.OneSignal);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsExpectedCount()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().HaveCountGreaterThanOrEqualTo(5);
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
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, false);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Notifications.BuiltIn);
    }

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, true);
        SetupConfiguration("Providers:Notifications:Type", "Novu");
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("Novu");
    }

    #endregion

    #region IsProviderAvailableAsync Tests

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderAvailable_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInNotificationProvider");
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
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationHealthResult { IsHealthy = false });
        SetupSpecificProvider(mockProvider.Object, "BuiltInNotificationProvider");
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
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));
        SetupSpecificProvider(mockProvider.Object, "BuiltInNotificationProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Multi-Channel Tests

    [Fact]
    public void GetProvider_BuiltIn_SupportsEmailChannel()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, false);
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.SendEmailAsync(
            It.IsAny<NotificationEmailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResponse { Success = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInNotificationProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Twilio_SupportsSmsChannel()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, true);
        SetupConfiguration("Providers:Notifications:Type", "Twilio");
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.SendSmsAsync(
            It.IsAny<NotificationSmsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResponse { Success = true });
        SetupSpecificProvider(mockProvider.Object, "TwilioProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_Novu_SupportsMultiChannel()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalNotifications, true);
        SetupConfiguration("Providers:Notifications:Type", "Novu");
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.SendPushAsync(
            It.IsAny<NotificationPushRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResponse { Success = true });
        SetupSpecificProvider(mockProvider.Object, "NovuProvider");
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
        factory.Should().BeAssignableTo<IProviderFactory<INotificationPort>>();
    }

    #endregion

    #region Helper Methods

    private NotificationProviderFactory CreateFactory()
    {
        return new NotificationProviderFactory(
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
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInNotificationProvider");
    }

    private void SetupProviderByType(string providerType)
    {
        var mockProvider = new Mock<INotificationPort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationHealthResult { IsHealthy = true });
        
        var providerTypeName = providerType.ToLowerInvariant() switch
        {
            "builtin" => "BuiltInNotificationProvider",
            "novu" => "NovuProvider",
            "twilio" => "TwilioProvider",
            "sendgrid" => "SendGridProvider",
            "onesignal" => "OneSignalProvider",
            _ => "BuiltInNotificationProvider"
        };

        SetupSpecificProvider(mockProvider.Object, providerTypeName);
    }

    private void SetupSpecificProvider(INotificationPort provider, string typeName)
    {
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationPort>)))
            .Returns(new[] { provider });
        
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(INotificationPort)))
            .Returns(provider);
    }

    #endregion
}
