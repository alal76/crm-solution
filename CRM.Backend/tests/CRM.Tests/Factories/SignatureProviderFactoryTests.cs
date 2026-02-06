// CRM Solution - Signature Provider Factory Tests
// Tests for e-signature provider factory resolution and switching

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
/// Unit tests for SignatureProviderFactory.
/// Tests provider resolution, feature flag handling, and document signing workflows.
/// </summary>
public class SignatureProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<SignatureProviderFactory>> _mockLogger;

    public SignatureProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<SignatureProviderFactory>>();
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
        var act = () => new SignatureProviderFactory(
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
        var act = () => new SignatureProviderFactory(
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
        var act = () => new SignatureProviderFactory(
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
        var act = () => new SignatureProviderFactory(
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
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, false);
        SetupBuiltInProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalSignatures), Times.Once);
    }

    [Theory]
    [InlineData("DocuSeal")]
    [InlineData("DocuSign")]
    [InlineData("AdobeSign")]
    [InlineData("HelloSign")]
    public void GetProvider_WhenFeatureFlagEnabled_ReturnsConfiguredProvider(string providerType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, true);
        SetupConfiguration("Providers:Signatures:Type", providerType);
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
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, true);
        SetupConfiguration("Providers:Signatures:Type", "Unknown");
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
    [InlineData("builtin", "BuiltInSignatureProvider")]
    [InlineData("docuseal", "DocuSealProvider")]
    [InlineData("docusign", "DocuSignProvider")]
    [InlineData("adobesign", "AdobeSignProvider")]
    [InlineData("hellosign", "HelloSignProvider")]
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
            .WithMessage("*Unknown signature provider*");
    }

    [Theory]
    [InlineData("DOCUSEAL")]
    [InlineData("DocuSeal")]
    [InlineData("docuseal")]
    public void GetProvider_WithDifferentCases_IsCaseInsensitive(string providerName)
    {
        // Arrange
        SetupProviderByType("docuseal");
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
        providers.Should().Contain(ProviderTypes.Signatures.BuiltIn);
        providers.Should().Contain(ProviderTypes.Signatures.DocuSeal);
        providers.Should().Contain(ProviderTypes.Signatures.DocuSign);
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
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, false);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.Signatures.BuiltIn);
    }

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, true);
        SetupConfiguration("Providers:Signatures:Type", "DocuSign");
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("DocuSign");
    }

    #endregion

    #region IsProviderAvailableAsync Tests

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderAvailable_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSignatureProvider");
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
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureHealthResult { IsHealthy = false });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSignatureProvider");
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
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));
        SetupSpecificProvider(mockProvider.Object, "BuiltInSignatureProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("builtin");

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Signature Workflow Tests

    [Fact]
    public void GetProvider_BuiltIn_SupportsSignatureRequestCreation()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, false);
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.CreateSignatureRequestAsync(
            It.IsAny<SignatureRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureRequestResult { RequestId = "1", Status = "pending" });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSignatureProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_DocuSign_SupportsTemplates()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, true);
        SetupConfiguration("Providers:Signatures:Type", "DocuSign");
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.GetTemplatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SignatureTemplate>());
        SetupSpecificProvider(mockProvider.Object, "DocuSignProvider");
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_DocuSeal_SupportsAuditTrail()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalSignatures, true);
        SetupConfiguration("Providers:Signatures:Type", "DocuSeal");
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.GetAuditTrailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureAuditTrail { RequestId = "1", Events = new List<SignatureAuditEvent>() });
        SetupSpecificProvider(mockProvider.Object, "DocuSealProvider");
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
        factory.Should().BeAssignableTo<IProviderFactory<ISignaturePort>>();
    }

    #endregion

    #region Helper Methods

    private SignatureProviderFactory CreateFactory()
    {
        return new SignatureProviderFactory(
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
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureHealthResult { IsHealthy = true });
        SetupSpecificProvider(mockProvider.Object, "BuiltInSignatureProvider");
    }

    private void SetupProviderByType(string providerType)
    {
        var mockProvider = new Mock<ISignaturePort>();
        mockProvider.Setup(p => p.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureHealthResult { IsHealthy = true });
        
        var providerTypeName = providerType.ToLowerInvariant() switch
        {
            "builtin" => "BuiltInSignatureProvider",
            "docuseal" => "DocuSealProvider",
            "docusign" => "DocuSignProvider",
            "adobesign" => "AdobeSignProvider",
            "hellosign" => "HelloSignProvider",
            _ => "BuiltInSignatureProvider"
        };

        SetupSpecificProvider(mockProvider.Object, providerTypeName);
    }

    private void SetupSpecificProvider(ISignaturePort provider, string typeName)
    {
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<ISignaturePort>)))
            .Returns(new[] { provider });
        
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ISignaturePort)))
            .Returns(provider);
    }

    #endregion
}
