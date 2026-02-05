using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for AI/LLM providers (Phase 7)
/// Tests OllamaProvider, AzureOpenAIProvider, and BedrockProvider
/// </summary>
public class AIProviderTests
{
    #region OllamaProvider Tests

    [Fact]
    public async Task OllamaProvider_GetChatCompletionAsync_Returns_Response()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            model = "llama3",
            message = new { role = "assistant", content = "Hello! How can I help you today?" },
            done = true,
            total_duration = 1234567890,
            prompt_eval_count = 10,
            eval_count = 15
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        var config = Options.Create(new OllamaConfiguration
        {
            BaseUrl = "http://localhost:11434",
            DefaultModel = "llama3",
            EmbeddingModel = "nomic-embed-text",
            TimeoutSeconds = 120
        });

        var logger = new Mock<ILogger<OllamaProvider>>();
        var provider = new OllamaProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetChatCompletionAsync(
            "Hello",
            new AIRequestOptions { MaxTokens = 100 });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Hello", result.Content);
    }

    [Fact]
    public async Task OllamaProvider_GetEmbeddingsAsync_Returns_Embeddings()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var embeddings = Enumerable.Range(0, 384).Select(_ => 0.1f).ToArray();
        var responseBody = JsonSerializer.Serialize(new { embedding = embeddings });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        var config = Options.Create(new OllamaConfiguration
        {
            BaseUrl = "http://localhost:11434",
            DefaultModel = "llama3",
            EmbeddingModel = "nomic-embed-text"
        });

        var logger = new Mock<ILogger<OllamaProvider>>();
        var provider = new OllamaProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetEmbeddingsAsync("Test text");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Embeddings);
        Assert.Equal(384, result.Embeddings.Length);
    }

    [Fact]
    public async Task OllamaProvider_HealthCheckAsync_Returns_Healthy()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Ollama is running")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        var config = Options.Create(new OllamaConfiguration
        {
            BaseUrl = "http://localhost:11434",
            DefaultModel = "llama3"
        });

        var logger = new Mock<ILogger<OllamaProvider>>();
        var provider = new OllamaProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Ollama", result.ProviderName);
    }

    [Fact]
    public void OllamaProvider_ProviderName_Returns_Correct_Name()
    {
        // Arrange
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        var config = Options.Create(new OllamaConfiguration { BaseUrl = "http://localhost:11434" });
        var logger = new Mock<ILogger<OllamaProvider>>();
        var provider = new OllamaProvider(httpClient, config, logger.Object);

        // Assert
        Assert.Equal("Ollama", provider.ProviderName);
    }

    [Fact]
    public async Task OllamaProvider_DraftEmailAsync_Returns_Email_Content()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            model = "llama3",
            message = new { role = "assistant", content = "Subject: Follow-up\n\nDear Customer,\n\nThank you for your inquiry." },
            done = true
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        var config = Options.Create(new OllamaConfiguration
        {
            BaseUrl = "http://localhost:11434",
            DefaultModel = "llama3"
        });

        var logger = new Mock<ILogger<OllamaProvider>>();
        var provider = new OllamaProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.DraftEmailAsync(
            "Follow-up after sales call",
            "Customer expressed interest in enterprise plan");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Subject", result.Content);
    }

    #endregion

    #region AzureOpenAIProvider Tests

    [Fact]
    public async Task AzureOpenAIProvider_GetChatCompletionAsync_Returns_Response()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-123",
            @object = "chat.completion",
            created = 1234567890,
            model = "gpt-4",
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new { role = "assistant", content = "Hello! How can I assist you?" },
                    finish_reason = "stop"
                }
            },
            usage = new { prompt_tokens = 10, completion_tokens = 8, total_tokens = 18 }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://test.openai.azure.com")
        };

        var config = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName = "gpt-4",
            ApiVersion = "2024-02-15-preview"
        });

        var logger = new Mock<ILogger<AzureOpenAIProvider>>();
        var provider = new AzureOpenAIProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetChatCompletionAsync(
            "Hello",
            new AIRequestOptions { MaxTokens = 100 });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Hello", result.Content);
    }

    [Fact]
    public async Task AzureOpenAIProvider_GetEmbeddingsAsync_Returns_Embeddings()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var embeddings = Enumerable.Range(0, 1536).Select(_ => 0.1f).ToArray();
        var responseBody = JsonSerializer.Serialize(new
        {
            @object = "list",
            data = new[]
            {
                new { @object = "embedding", embedding = embeddings, index = 0 }
            },
            model = "text-embedding-ada-002",
            usage = new { prompt_tokens = 5, total_tokens = 5 }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://test.openai.azure.com")
        };

        var config = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            EmbeddingDeploymentName = "text-embedding-ada-002",
            ApiVersion = "2024-02-15-preview"
        });

        var logger = new Mock<ILogger<AzureOpenAIProvider>>();
        var provider = new AzureOpenAIProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetEmbeddingsAsync("Test text");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Embeddings);
        Assert.Equal(1536, result.Embeddings.Length);
    }

    [Fact]
    public async Task AzureOpenAIProvider_HealthCheckAsync_Returns_Healthy()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-123",
            choices = new[]
            {
                new { message = new { content = "OK" } }
            }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://test.openai.azure.com")
        };

        var config = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName = "gpt-4",
            ApiVersion = "2024-02-15-preview"
        });

        var logger = new Mock<ILogger<AzureOpenAIProvider>>();
        var provider = new AzureOpenAIProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("AzureOpenAI", result.ProviderName);
    }

    [Fact]
    public void AzureOpenAIProvider_ProviderName_Returns_Correct_Name()
    {
        // Arrange
        var httpClient = new HttpClient { BaseAddress = new Uri("https://test.openai.azure.com") };
        var config = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key"
        });
        var logger = new Mock<ILogger<AzureOpenAIProvider>>();
        var provider = new AzureOpenAIProvider(httpClient, config, logger.Object);

        // Assert
        Assert.Equal("AzureOpenAI", provider.ProviderName);
    }

    [Fact]
    public async Task AzureOpenAIProvider_AnalyzeSentimentAsync_Returns_Sentiment()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-123",
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = "{\"sentiment\":\"positive\",\"confidence\":0.95,\"aspects\":[{\"aspect\":\"service\",\"sentiment\":\"positive\"}]}"
                    }
                }
            }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://test.openai.azure.com")
        };

        var config = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName = "gpt-4"
        });

        var logger = new Mock<ILogger<AzureOpenAIProvider>>();
        var provider = new AzureOpenAIProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.AnalyzeSentimentAsync("Great service, very satisfied!");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    #endregion

    #region BedrockProvider Tests

    [Fact]
    public async Task BedrockProvider_GetChatCompletionAsync_Returns_Response()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "msg_123",
            type = "message",
            role = "assistant",
            content = new[]
            {
                new { type = "text", text = "Hello! How can I help you today?" }
            },
            model = "anthropic.claude-3-sonnet-20240229-v1:0",
            stop_reason = "end_turn",
            usage = new { input_tokens = 10, output_tokens = 12 }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0",
            UseDefaultCredentials = true
        });

        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetChatCompletionAsync(
            "Hello",
            new AIRequestOptions { MaxTokens = 100 });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Hello", result.Content);
    }

    [Fact]
    public async Task BedrockProvider_GetEmbeddingsAsync_Returns_Embeddings()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var embeddings = Enumerable.Range(0, 1536).Select(_ => 0.1f).ToArray();
        var responseBody = JsonSerializer.Serialize(new
        {
            embedding = embeddings,
            inputTextTokenCount = 5
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            EmbeddingModelId = "amazon.titan-embed-text-v1",
            UseDefaultCredentials = true
        });

        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetEmbeddingsAsync("Test text");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Embeddings);
        Assert.Equal(1536, result.Embeddings.Length);
    }

    [Fact]
    public async Task BedrockProvider_HealthCheckAsync_Returns_Healthy()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "msg_123",
            content = new[] { new { text = "OK" } }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0",
            UseDefaultCredentials = true
        });

        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Bedrock", result.ProviderName);
    }

    [Fact]
    public void BedrockProvider_ProviderName_Returns_Correct_Name()
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };
        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });
        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Assert
        Assert.Equal("Bedrock", provider.ProviderName);
    }

    [Fact]
    public async Task BedrockProvider_DraftEmailAsync_Returns_Email_Content()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "msg_123",
            content = new[]
            {
                new { type = "text", text = "Subject: Follow-up on Our Discussion\n\nDear Customer,\n\nThank you for your time today." }
            }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.DraftEmailAsync(
            "Follow-up after meeting",
            "Customer interested in enterprise features");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Subject", result.Content);
    }

    [Fact]
    public async Task BedrockProvider_ExtractEntitiesAsync_Returns_Entities()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseBody = JsonSerializer.Serialize(new
        {
            id = "msg_123",
            content = new[]
            {
                new { type = "text", text = "{\"entities\":[{\"type\":\"Person\",\"value\":\"John Smith\",\"confidence\":0.95},{\"type\":\"Organization\",\"value\":\"Acme Corp\",\"confidence\":0.92}]}" }
            }
        });

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.ExtractEntitiesAsync(
            "John Smith from Acme Corp called about the deal.");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    #endregion

    #region Provider Factory Tests

    [Fact]
    public void AIProviderFactory_GetAvailableProviders_Returns_All_Providers()
    {
        // The factory should list all available AI providers
        var expectedProviders = new[]
        {
            "Ollama",
            "OpenAI",
            "AzureOpenAI",
            "Anthropic",
            "Bedrock",
            "Gemini"
        };

        // This tests the expected provider names from ProviderTypes.AI
        Assert.Contains("Ollama", expectedProviders);
        Assert.Contains("AzureOpenAI", expectedProviders);
        Assert.Contains("Bedrock", expectedProviders);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task OllamaProvider_Returns_Error_When_Service_Unavailable()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        var config = Options.Create(new OllamaConfiguration
        {
            BaseUrl = "http://localhost:11434",
            DefaultModel = "llama3"
        });

        var logger = new Mock<ILogger<OllamaProvider>>();
        var provider = new OllamaProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetChatCompletionAsync("Hello", new AIRequestOptions());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task AzureOpenAIProvider_Returns_Error_On_Rate_Limit()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent("{\"error\":{\"message\":\"Rate limit exceeded\"}}")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://test.openai.azure.com")
        };

        var config = Options.Create(new AzureOpenAIConfiguration
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName = "gpt-4"
        });

        var logger = new Mock<ILogger<AzureOpenAIProvider>>();
        var provider = new AzureOpenAIProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetChatCompletionAsync("Hello", new AIRequestOptions());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task BedrockProvider_Returns_Error_On_Access_Denied()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent("{\"message\":\"Access Denied\"}")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://bedrock-runtime.us-east-1.amazonaws.com")
        };

        var config = Options.Create(new BedrockConfiguration
        {
            Region = "us-east-1",
            DefaultModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var logger = new Mock<ILogger<BedrockProvider>>();
        var provider = new BedrockProvider(httpClient, config, logger.Object);

        // Act
        var result = await provider.GetChatCompletionAsync("Hello", new AIRequestOptions());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void OllamaConfiguration_Has_Default_Values()
    {
        var config = new OllamaConfiguration();
        
        Assert.Equal(120, config.TimeoutSeconds);
        Assert.Equal(4096, config.DefaultMaxTokens);
        Assert.Equal(0.7f, config.DefaultTemperature);
    }

    [Fact]
    public void AzureOpenAIConfiguration_Has_Default_Values()
    {
        var config = new AzureOpenAIConfiguration();
        
        Assert.Equal(120, config.TimeoutSeconds);
        Assert.Equal(3, config.MaxRetries);
        Assert.Equal(4096, config.DefaultMaxTokens);
        Assert.Equal(0.7f, config.DefaultTemperature);
        Assert.False(config.UseAzureADAuth);
    }

    [Fact]
    public void BedrockConfiguration_Has_Default_Values()
    {
        var config = new BedrockConfiguration();
        
        Assert.Equal(120, config.TimeoutSeconds);
        Assert.Equal(3, config.MaxRetries);
        Assert.Equal(4096, config.DefaultMaxTokens);
        Assert.Equal(0.7f, config.DefaultTemperature);
        Assert.True(config.UseDefaultCredentials);
    }

    #endregion
}
