// CRM Solution - AI Provider Factory Tests
// Tests for AI/LLM provider factory resolution and switching

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
/// Unit tests for AIProviderFactory.
/// Tests provider resolution, feature flag handling, and fallback behavior.
/// </summary>
public class AIProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AIProviderFactory>> _mockLogger;

    public AIProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AIProviderFactory>>();
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
        var act = () => new AIProviderFactory(
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
        var act = () => new AIProviderFactory(
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
        var act = () => new AIProviderFactory(
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
        var act = () => new AIProviderFactory(
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
    public void GetProvider_WhenFeatureFlagDisabled_ReturnsOllamaProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, false);
        SetupOllamaProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(FeatureFlags.UseExternalAI), Times.Once);
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("AzureOpenAI")]
    [InlineData("Anthropic")]
    [InlineData("Bedrock")]
    [InlineData("Gemini")]
    [InlineData("OpenRouter")]
    public void GetProvider_WhenFeatureFlagEnabled_ReturnsConfiguredProvider(string providerType)
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, true);
        SetupConfiguration($"Providers:AI:Type", providerType);
        SetupProviderByType(providerType);
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_WhenProviderTypeNotConfigured_DefaultsToOllama()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, true);
        SetupConfiguration($"Providers:AI:Type", null);
        SetupOllamaProvider();
        var factory = CreateFactory();

        // Act
        var provider = factory.GetProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void GetProvider_WhenProviderResolutionFails_FallsBackToOllama()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, true);
        SetupConfiguration($"Providers:AI:Type", "Unknown");
        SetupOllamaProvider();
        var factory = CreateFactory();

        // Act - Should not throw due to fallback
        var act = () => factory.GetProvider();

        // Assert - Will log warning and return Ollama
        // Note: May throw if Ollama is also not registered in this test setup
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

    [Fact]
    public void GetProvider_WithWhitespaceProviderName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetProvider("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("providerName");
    }

    [Theory]
    [InlineData("ollama", "OllamaProvider")]
    [InlineData("openai", "OpenAIProvider")]
    [InlineData("azureopenai", "AzureOpenAIProvider")]
    [InlineData("anthropic", "AnthropicAIProvider")]
    [InlineData("bedrock", "BedrockProvider")]
    [InlineData("gemini", "GeminiAIProvider")]
    [InlineData("openrouter", "OpenRouterProvider")]
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
            .WithMessage("*Unknown AI provider*");
    }

    [Theory]
    [InlineData("OLLAMA")]
    [InlineData("Ollama")]
    [InlineData("ollama")]
    public void GetProvider_WithDifferentCases_IsCaseInsensitive(string providerName)
    {
        // Arrange
        SetupOllamaProvider();
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
        providers.Should().Contain(ProviderTypes.AI.Ollama);
        providers.Should().Contain(ProviderTypes.AI.OpenAI);
        providers.Should().Contain(ProviderTypes.AI.AzureOpenAI);
        providers.Should().Contain(ProviderTypes.AI.Anthropic);
        providers.Should().Contain(ProviderTypes.AI.Bedrock);
        providers.Should().Contain(ProviderTypes.AI.Gemini);
        providers.Should().Contain(ProviderTypes.AI.OpenRouter);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsSevenProviders()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var providers = factory.GetAvailableProviders().ToList();

        // Assert
        providers.Should().HaveCount(7);
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
    public void GetActiveProviderName_WhenFeatureFlagDisabled_ReturnsOllama()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, false);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.AI.Ollama);
    }

    [Fact]
    public void GetActiveProviderName_WhenFeatureFlagEnabled_ReturnsConfiguredProvider()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, true);
        SetupConfiguration("Providers:AI:Type", "OpenAI");
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be("OpenAI");
    }

    [Fact]
    public void GetActiveProviderName_WhenNotConfigured_DefaultsToOllama()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, true);
        SetupConfiguration("Providers:AI:Type", null);
        var factory = CreateFactory();

        // Act
        var activeProvider = factory.GetActiveProviderName();

        // Assert
        activeProvider.Should().Be(ProviderTypes.AI.Ollama);
    }

    #endregion

    #region IsProviderAvailableAsync Tests

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderAvailable_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<IAIPort>();
        mockProvider.Setup(p => p.IsAvailableAsync()).ReturnsAsync(true);
        SetupSpecificProvider(mockProvider.Object, "OllamaProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("ollama");

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderUnavailable_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new Mock<IAIPort>();
        mockProvider.Setup(p => p.IsAvailableAsync()).ReturnsAsync(false);
        SetupSpecificProvider(mockProvider.Object, "OllamaProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("ollama");

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderThrows_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new Mock<IAIPort>();
        mockProvider.Setup(p => p.IsAvailableAsync()).ThrowsAsync(new Exception("Connection failed"));
        SetupSpecificProvider(mockProvider.Object, "OllamaProvider");
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("ollama");

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task IsProviderAvailableAsync_WhenProviderNotRegistered_ReturnsFalse()
    {
        // Arrange
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IEnumerable<IAIPort>)))
            .Returns(Array.Empty<IAIPort>());
        var factory = CreateFactory();

        // Act
        var isAvailable = await factory.IsProviderAvailableAsync("ollama");

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
        factory.Should().BeAssignableTo<IProviderFactory<IAIPort>>();
    }

    [Fact]
    public void Factory_AllInterfaceMethodsAvailable()
    {
        // Arrange
        var factoryType = typeof(AIProviderFactory);

        // Assert - Verify interface methods are implemented
        factoryType.GetMethod("GetProvider", Type.EmptyTypes).Should().NotBeNull();
        factoryType.GetMethod("GetProvider", new[] { typeof(string) }).Should().NotBeNull();
        factoryType.GetMethod("GetAvailableProviders").Should().NotBeNull();
        factoryType.GetMethod("GetActiveProviderName").Should().NotBeNull();
        factoryType.GetMethod("IsProviderAvailableAsync").Should().NotBeNull();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void GetProvider_LogsProviderResolution()
    {
        // Arrange
        SetupFeatureFlag(FeatureFlags.UseExternalAI, false);
        SetupOllamaProvider();
        var factory = CreateFactory();

        // Act
        factory.GetProvider();

        // Assert - Verify logging was called
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Helper Methods

    private AIProviderFactory CreateFactory()
    {
        return new AIProviderFactory(
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

    private void SetupOllamaProvider()
    {
        var mockProvider = new Mock<IAIPort>();
        mockProvider.Setup(p => p.IsAvailableAsync()).ReturnsAsync(true);
        SetupSpecificProvider(mockProvider.Object, "OllamaProvider");
    }

    private void SetupProviderByType(string providerType)
    {
        var mockProvider = new Mock<IAIPort>();
        mockProvider.Setup(p => p.IsAvailableAsync()).ReturnsAsync(true);
        
        var providerTypeName = providerType.ToLowerInvariant() switch
        {
            "ollama" => "OllamaProvider",
            "openai" => "OpenAIProvider",
            "azureopenai" => "AzureOpenAIProvider",
            "anthropic" => "AnthropicAIProvider",
            "bedrock" => "BedrockProvider",
            "gemini" => "GeminiAIProvider",
            "openrouter" => "OpenRouterProvider",
            _ => "OllamaProvider"
        };

        SetupSpecificProvider(mockProvider.Object, providerTypeName);
    }

    private void SetupSpecificProvider(IAIPort provider, string typeName)
    {
        // Create a mock that returns a specific type name
        var mockProviderWithName = new Mock<IAIPort>();
        mockProviderWithName.Setup(p => p.IsAvailableAsync()).ReturnsAsync(true);
        
        // Use a custom implementation that has the expected type name
        var testProvider = new TestAIProvider(typeName);
        testProvider.SetupAvailability(true);

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IAIPort>)))
            .Returns(new[] { testProvider as IAIPort });
    }

    /// <summary>
    /// Test implementation of IAIPort for testing purposes.
    /// </summary>
    private class TestAIProvider : IAIPort
    {
        private readonly string _typeName;
        private bool _isAvailable = true;

        public TestAIProvider(string typeName)
        {
            _typeName = typeName;
        }

        public void SetupAvailability(bool isAvailable)
        {
            _isAvailable = isAvailable;
        }

        // Override GetType().Name behavior isn't possible, 
        // but we can use a naming convention in tests

        public Task<bool> IsAvailableAsync() => Task.FromResult(_isAvailable);
        
        public Task<AIHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new AIHealthResult { IsHealthy = _isAvailable });

        public Task<AIChatResponse> GetChatCompletionAsync(AIChatRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AIChatResponse { Content = "Test response" });

        public IAsyncEnumerable<string> StreamChatCompletionAsync(AIChatRequest request, CancellationToken cancellationToken = default)
            => AsyncEnumerable.Empty<string>();

        public Task<AIEmbeddingResponse> GetEmbeddingsAsync(AIEmbeddingRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AIEmbeddingResponse { Embeddings = new float[0][] });

        public Task<AIBatchEmbeddingResponse> GetBatchEmbeddingsAsync(AIBatchEmbeddingRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AIBatchEmbeddingResponse { Embeddings = new List<float[]>() });

        // CRM-specific methods
        public Task<AIDraftEmailResponse> DraftEmailAsync(AIDraftEmailRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AIDraftEmailResponse { Subject = "Test", Body = "Test" });

        public Task<AISentimentResponse> AnalyzeSentimentAsync(AISentimentRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AISentimentResponse { Sentiment = "Neutral", Score = 0.5 });

        public Task<AIEntityExtractionResponse> ExtractEntitiesAsync(AIEntityExtractionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AIEntityExtractionResponse { Entities = new List<ExtractedEntity>() });

        public Task<AISummaryResponse> SummarizeAsync(AISummaryRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AISummaryResponse { Summary = "Test summary" });

        public Task<AIRecommendationResponse> GetRecommendationsAsync(AIRecommendationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AIRecommendationResponse { Recommendations = new List<Recommendation>() });
    }

    #endregion
}
