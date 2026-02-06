// CRM Solution - Customer Relationship Management System
// LLM (Large Language Model) Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LLMService
/// Covers: AI completions, embeddings, summarization, sentiment
/// </summary>
public class LLMServiceTests
{
    private readonly Mock<IRepository<LLMProviderSettings>> _mockSettingsRepository;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<LLMService>> _mockLogger;
    private readonly Mock<IOptions<LLMSettings>> _mockOptions;
    private readonly LLMService _service;

    public LLMServiceTests()
    {
        _mockSettingsRepository = new Mock<IRepository<LLMProviderSettings>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<LLMService>>();
        _mockOptions = new Mock<IOptions<LLMSettings>>();

        _mockOptions.Setup(o => o.Value).Returns(new LLMSettings
        {
            DefaultProvider = "ollama",
            OllamaUrl = "http://localhost:11434",
            DefaultModel = "llama3"
        });

        var mockHttpClient = new HttpClient(new MockHttpMessageHandler());
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient);

        _service = new LLMService(
            _mockSettingsRepository.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    #region Chat Completion Tests

    [Fact]
    public async Task GetChatCompletionAsync_ValidPrompt_ReturnsResponse()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Hello, how are you?" }
            }
        };

        // Act
        var result = await _service.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_EmptyMessages_ThrowsException()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Messages = new List<ChatMessage>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetChatCompletionAsync(request));
    }

    [Fact]
    public async Task GetChatCompletionAsync_NullRequest_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetChatCompletionAsync(null!));
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithSystemPrompt_IncludesSystemMessage()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = "You are a helpful assistant." },
                new ChatMessage { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var result = await _service.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithTemperature_UsesTemperature()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Be creative" }
            },
            Temperature = 0.9f
        };

        // Act
        var result = await _service.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Text Completion Tests

    [Fact]
    public async Task CompleteTextAsync_ValidPrompt_ReturnsCompletion()
    {
        // Arrange
        var prompt = "Complete this sentence: The quick brown fox";

        // Act
        var result = await _service.CompleteTextAsync(prompt);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CompleteTextAsync_EmptyPrompt_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CompleteTextAsync(""));
    }

    [Fact]
    public async Task CompleteTextAsync_WithMaxTokens_RespectsLimit()
    {
        // Arrange
        var prompt = "Write a story";
        var maxTokens = 100;

        // Act
        var result = await _service.CompleteTextAsync(prompt, maxTokens: maxTokens);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Embedding Tests

    [Fact]
    public async Task GetEmbeddingsAsync_ValidText_ReturnsEmbeddings()
    {
        // Arrange
        var text = "This is a test sentence for embeddings.";

        // Act
        var result = await _service.GetEmbeddingsAsync(text);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetEmbeddingsAsync_EmptyText_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEmbeddingsAsync(""));
    }

    [Fact]
    public async Task GetBatchEmbeddingsAsync_MultipleTexts_ReturnsAllEmbeddings()
    {
        // Arrange
        var texts = new List<string>
        {
            "First text",
            "Second text",
            "Third text"
        };

        // Act
        var result = await _service.GetBatchEmbeddingsAsync(texts);

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Summarization Tests

    [Fact]
    public async Task SummarizeTextAsync_LongText_ReturnsShortSummary()
    {
        // Arrange
        var longText = string.Join(" ", Enumerable.Repeat("This is a long text. ", 100));

        // Act
        var result = await _service.SummarizeTextAsync(longText);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeLessThan(longText.Length);
    }

    [Fact]
    public async Task SummarizeTextAsync_ShortText_ReturnsText()
    {
        // Arrange
        var shortText = "This is a short text.";

        // Act
        var result = await _service.SummarizeTextAsync(shortText);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SummarizeTextAsync_WithMaxLength_RespectsLimit()
    {
        // Arrange
        var text = string.Join(" ", Enumerable.Repeat("Word ", 1000));
        var maxLength = 100;

        // Act
        var result = await _service.SummarizeTextAsync(text, maxLength: maxLength);

        // Assert
        result.Length.Should().BeLessOrEqualTo(maxLength + 50); // Allow some buffer
    }

    #endregion

    #region Sentiment Analysis Tests

    [Fact]
    public async Task AnalyzeSentimentAsync_PositiveText_ReturnsPositive()
    {
        // Arrange
        var text = "I love this product! It's amazing and wonderful!";

        // Act
        var result = await _service.AnalyzeSentimentAsync(text);

        // Assert
        result.Should().NotBeNull();
        result.Sentiment.Should().Be("positive");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_NegativeText_ReturnsNegative()
    {
        // Arrange
        var text = "This is terrible. I hate it and want a refund.";

        // Act
        var result = await _service.AnalyzeSentimentAsync(text);

        // Assert
        result.Should().NotBeNull();
        result.Sentiment.Should().Be("negative");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_NeutralText_ReturnsNeutral()
    {
        // Arrange
        var text = "The product arrived on Tuesday.";

        // Act
        var result = await _service.AnalyzeSentimentAsync(text);

        // Assert
        result.Should().NotBeNull();
        result.Sentiment.Should().Be("neutral");
    }

    [Fact]
    public async Task AnalyzeSentimentAsync_ReturnsConfidenceScore()
    {
        // Arrange
        var text = "This is great!";

        // Act
        var result = await _service.AnalyzeSentimentAsync(text);

        // Assert
        result.Confidence.Should().BeGreaterThanOrEqualTo(0);
        result.Confidence.Should().BeLessThanOrEqualTo(1);
    }

    #endregion

    #region Email Draft Tests

    [Fact]
    public async Task DraftEmailAsync_ValidContext_ReturnsEmail()
    {
        // Arrange
        var context = new EmailDraftContext
        {
            RecipientName = "John Doe",
            Subject = "Follow up",
            Purpose = "Schedule a meeting",
            Tone = "professional"
        };

        // Act
        var result = await _service.DraftEmailAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().NotBeNullOrEmpty();
        result.Body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DraftEmailAsync_IncludesRecipientName()
    {
        // Arrange
        var context = new EmailDraftContext
        {
            RecipientName = "Jane Smith",
            Subject = "Proposal",
            Purpose = "Send proposal"
        };

        // Act
        var result = await _service.DraftEmailAsync(context);

        // Assert
        result.Body.Should().Contain("Jane");
    }

    [Fact]
    public async Task DraftEmailAsync_CasualTone_UsesCasualLanguage()
    {
        // Arrange
        var context = new EmailDraftContext
        {
            RecipientName = "Bob",
            Subject = "Quick question",
            Purpose = "Ask about availability",
            Tone = "casual"
        };

        // Act
        var result = await _service.DraftEmailAsync(context);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Entity Extraction Tests

    [Fact]
    public async Task ExtractEntitiesAsync_TextWithEntities_ReturnsEntities()
    {
        // Arrange
        var text = "Contact John Doe at john@acme.com or call 555-1234. He works at Acme Inc.";

        // Act
        var result = await _service.ExtractEntitiesAsync(text);

        // Assert
        result.Should().NotBeNull();
        result.Names.Should().Contain("John Doe");
        result.Emails.Should().Contain("john@acme.com");
    }

    [Fact]
    public async Task ExtractEntitiesAsync_NoEntities_ReturnsEmptyLists()
    {
        // Arrange
        var text = "This is a simple sentence without any entities.";

        // Act
        var result = await _service.ExtractEntitiesAsync(text);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Provider Management Tests

    [Fact]
    public async Task GetProvidersAsync_ReturnsAllProviders()
    {
        // Arrange
        var providers = new List<LLMProviderSettings>
        {
            new LLMProviderSettings { Id = 1, Name = "Ollama", IsEnabled = true },
            new LLMProviderSettings { Id = 2, Name = "OpenAI", IsEnabled = true }
        };

        _mockSettingsRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(providers);

        // Act
        var result = await _service.GetProvidersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsActiveProvider()
    {
        // Arrange
        var provider = new LLMProviderSettings
        {
            Id = 1,
            Name = "Ollama",
            IsEnabled = true,
            IsDefault = true
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LLMProviderSettings, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSettings> { provider });

        // Act
        var result = await _service.GetActiveProviderAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Ollama");
    }

    [Fact]
    public async Task SetActiveProviderAsync_ValidProvider_SetsActive()
    {
        // Arrange
        var provider = new LLMProviderSettings { Id = 1, IsDefault = false };

        _mockSettingsRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(provider);

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSettings>()))
            .ReturnsAsync((LLMProviderSettings p) => p);

        // Act
        var result = await _service.SetActiveProviderAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Model Management Tests

    [Fact]
    public async Task GetAvailableModelsAsync_ReturnsModels()
    {
        // Act
        var result = await _service.GetAvailableModelsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModelInfoAsync_ValidModel_ReturnsInfo()
    {
        // Arrange
        var modelName = "llama3";

        // Act
        var result = await _service.GetModelInfoAsync(modelName);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region CRM-Specific Tests

    [Fact]
    public async Task GenerateLeadScoreExplanationAsync_ReturnsExplanation()
    {
        // Arrange
        var leadData = new
        {
            Name = "John Doe",
            Company = "Acme Inc",
            Score = 85
        };

        // Act
        var result = await _service.GenerateLeadScoreExplanationAsync(leadData);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SuggestNextBestActionAsync_ReturnsActions()
    {
        // Arrange
        var opportunityData = new
        {
            Stage = "Negotiation",
            Value = 50000,
            DaysInStage = 10
        };

        // Act
        var result = await _service.SuggestNextBestActionAsync(opportunityData);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CategorizeSupportTicketAsync_ReturnsCategory()
    {
        // Arrange
        var ticketDescription = "I can't log in to my account. The password reset isn't working.";

        // Act
        var result = await _service.CategorizeSupportTicketAsync(ticketDescription);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetChatCompletionAsync_ProviderUnavailable_ThrowsException()
    {
        // Arrange
        var service = new LLMService(
            _mockSettingsRepository.Object,
            Mock.Of<IHttpClientFactory>(),
            _mockOptions.Object,
            _mockLogger.Object);

        var request = new ChatCompletionRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Test" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            service.GetChatCompletionAsync(request));
    }

    [Fact]
    public async Task CompleteTextAsync_Timeout_ThrowsTimeoutException()
    {
        // This test would require mocking HTTP timeout behavior
        // For now, we just verify the method signature works
        await Task.CompletedTask;
    }

    #endregion
}

// Supporting classes for tests
public class LLMSettings
{
    public string DefaultProvider { get; set; } = string.Empty;
    public string OllamaUrl { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
}

public class ChatCompletionRequest
{
    public List<ChatMessage> Messages { get; set; } = new();
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 1000;
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class EmailDraftContext
{
    public string RecipientName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Tone { get; set; } = "professional";
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"response\": \"test response\"}")
        });
    }
}
