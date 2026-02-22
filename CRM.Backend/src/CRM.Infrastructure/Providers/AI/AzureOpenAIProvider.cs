// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.AI;

#region Configuration

/// <summary>
/// Configuration options for Azure OpenAI Service provider.
/// </summary>
public class AzureOpenAIConfiguration
{
    /// <summary>
    /// Azure OpenAI endpoint URL.
    /// Format: https://{resource-name}.openai.azure.com
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name for chat/completion model.
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name for embedding model.
    /// </summary>
    public string EmbeddingDeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// API version to use.
    /// </summary>
    public string ApiVersion { get; set; } = "2024-02-15-preview";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Maximum retries on transient errors.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Default maximum tokens for completions.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Default temperature.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Enable Azure AD authentication instead of API key.
    /// </summary>
    public bool UseAzureADAuth { get; set; } = false;

    /// <summary>
    /// Tenant ID for Azure AD authentication.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Client ID for Azure AD authentication.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Client secret for Azure AD authentication.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            return (false, "Endpoint is required");
        if (string.IsNullOrWhiteSpace(DeploymentName))
            return (false, "DeploymentName is required");
        if (!UseAzureADAuth && string.IsNullOrWhiteSpace(ApiKey))
            return (false, "ApiKey is required when not using Azure AD authentication");
        if (UseAzureADAuth && (string.IsNullOrWhiteSpace(TenantId) || string.IsNullOrWhiteSpace(ClientId)))
            return (false, "TenantId and ClientId are required for Azure AD authentication");
        return (true, null);
    }
}

#endregion

#region Azure OpenAI DTOs

internal class AzureOpenAIChatRequest
{
    [JsonPropertyName("messages")]
    public List<AzureOpenAIMessage> Messages { get; set; } = new();

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("response_format")]
    public AzureOpenAIResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("tools")]
    public List<AzureOpenAITool>? Tools { get; set; }
}

internal class AzureOpenAIMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<AzureOpenAIToolCall>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}

internal class AzureOpenAIResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text"; // "text" or "json_object"
}

internal class AzureOpenAITool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public AzureOpenAIFunction? Function { get; set; }
}

internal class AzureOpenAIFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }
}

internal class AzureOpenAIToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public AzureOpenAIFunctionCall? Function { get; set; }
}

internal class AzureOpenAIFunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

internal class AzureOpenAIChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<AzureOpenAIChatChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public AzureOpenAIUsage? Usage { get; set; }
}

internal class AzureOpenAIChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public AzureOpenAIMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public AzureOpenAIMessage? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

internal class AzureOpenAIUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

internal class AzureOpenAIEmbeddingRequest
{
    [JsonPropertyName("input")]
    public object Input { get; set; } = string.Empty; // string or string[]
}

internal class AzureOpenAIEmbeddingResponse
{
    [JsonPropertyName("data")]
    public List<AzureOpenAIEmbeddingData>? Data { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("usage")]
    public AzureOpenAIUsage? Usage { get; set; }
}

internal class AzureOpenAIEmbeddingData
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }
}

internal class AzureOpenAICompletionRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

internal class AzureOpenAICompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<AzureOpenAICompletionChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public AzureOpenAIUsage? Usage { get; set; }
}

internal class AzureOpenAICompletionChoice
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

#endregion

/// <summary>
/// AI provider implementation for Azure OpenAI Service.
/// Provides enterprise-grade AI with Azure security and compliance.
/// </summary>
public class AzureOpenAIProvider : IAIPort
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAIConfiguration _config;
    private readonly ILogger<AzureOpenAIProvider> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AzureOpenAIProvider(
        HttpClient httpClient,
        IOptions<AzureOpenAIConfiguration> config,
        ILogger<AzureOpenAIProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderName => "AzureOpenAI";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try a simple models endpoint to verify connectivity
            var endpoint = $"{_config.Endpoint}/openai/deployments?api-version={_config.ApiVersion}";
            var request = CreateRequest(HttpMethod.Get, endpoint);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Azure OpenAI availability check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<AIModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        // Azure OpenAI uses deployments, not a model catalog
        var models = new List<AIModelInfo>
        {
            new AIModelInfo
            {
                Id = _config.DeploymentName,
                Name = _config.DeploymentName,
                Provider = ProviderName,
                Capabilities = new List<string> { "chat", "completion" },
                MaxTokens = _config.DefaultMaxTokens,
                IsAvailable = true
            }
        };

        if (!string.IsNullOrEmpty(_config.EmbeddingDeploymentName))
        {
            models.Add(new AIModelInfo
            {
                Id = _config.EmbeddingDeploymentName,
                Name = _config.EmbeddingDeploymentName,
                Provider = ProviderName,
                Capabilities = new List<string> { "embedding" },
                IsAvailable = true
            });
        }

        return Task.FromResult<IEnumerable<AIModelInfo>>(models);
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
    {
        // Use chat completion API as it's more versatile
        var chatRequest = new AIChatRequest
        {
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = request.Prompt }
            },
            Model = request.Model,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        return new AICompletionResponse
        {
            Text = response.Message.Content,
            Model = response.Model,
            Usage = response.Usage,
            FinishReason = response.FinishReason
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken cancellationToken = default)
    {
        var messages = new List<AzureOpenAIMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new AzureOpenAIMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.AddRange(request.Messages.Select(m => new AzureOpenAIMessage
        {
            Role = m.Role,
            Content = m.Content,
            Name = m.Name
        }));

        var azureRequest = new AzureOpenAIChatRequest
        {
            Messages = messages,
            MaxTokens = request.MaxTokens ?? _config.DefaultMaxTokens,
            Temperature = request.Temperature ?? _config.DefaultTemperature,
            TopP = request.TopP,
            Stream = false,
            ResponseFormat = request.JsonMode ? new AzureOpenAIResponseFormat { Type = "json_object" } : null
        };

        if (request.Tools?.Any() == true)
        {
            azureRequest.Tools = request.Tools.Select(t => new AzureOpenAITool
            {
                Type = "function",
                Function = new AzureOpenAIFunction
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.Parameters
                }
            }).ToList();
        }

        var deployment = request.Model ?? _config.DeploymentName;
        var endpoint = $"{_config.Endpoint}/openai/deployments/{deployment}/chat/completions?api-version={_config.ApiVersion}";

        var response = await SendRequestAsync<AzureOpenAIChatResponse>(endpoint, azureRequest, cancellationToken);

        var choice = response.Choices?.FirstOrDefault();
        var toolCalls = choice?.Message?.ToolCalls?.Select(tc => new AIToolCall
        {
            Id = tc.Id,
            Name = tc.Function?.Name ?? string.Empty,
            Arguments = tc.Function?.Arguments != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(tc.Function.Arguments)
                : null
        }).ToList();

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = choice?.Message?.Role ?? "assistant",
                Content = choice?.Message?.Content ?? string.Empty
            },
            Model = response.Model,
            Usage = new AIUsage
            {
                PromptTokens = response.Usage?.PromptTokens ?? 0,
                CompletionTokens = response.Usage?.CompletionTokens ?? 0,
                TotalTokens = response.Usage?.TotalTokens ?? 0
            },
            FinishReason = choice?.FinishReason,
            ToolCalls = toolCalls
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> StreamChatAsync(AIChatRequest request, Action<string> onToken, CancellationToken cancellationToken = default)
    {
        var messages = new List<AzureOpenAIMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new AzureOpenAIMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.AddRange(request.Messages.Select(m => new AzureOpenAIMessage
        {
            Role = m.Role,
            Content = m.Content,
            Name = m.Name
        }));

        var azureRequest = new AzureOpenAIChatRequest
        {
            Messages = messages,
            MaxTokens = request.MaxTokens ?? _config.DefaultMaxTokens,
            Temperature = request.Temperature ?? _config.DefaultTemperature,
            TopP = request.TopP,
            Stream = true,
            ResponseFormat = request.JsonMode ? new AzureOpenAIResponseFormat { Type = "json_object" } : null
        };

        var deployment = request.Model ?? _config.DeploymentName;
        var endpoint = $"{_config.Endpoint}/openai/deployments/{deployment}/chat/completions?api-version={_config.ApiVersion}";

        var httpRequest = CreateRequest(HttpMethod.Post, endpoint);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(azureRequest, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fullContent = new StringBuilder();
        var model = _config.DeploymentName;
        string? finishReason = null;
        var promptTokens = 0;
        var completionTokens = 0;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var data = line.Substring(6);
            if (data == "[DONE]")
                break;

            try
            {
                var chunk = JsonSerializer.Deserialize<AzureOpenAIChatResponse>(data, JsonOptions);
                var choice = chunk?.Choices?.FirstOrDefault();

                if (choice?.Delta?.Content != null)
                {
                    onToken(choice.Delta.Content);
                    fullContent.Append(choice.Delta.Content);
                    completionTokens++;
                }

                if (!string.IsNullOrEmpty(choice?.FinishReason))
                {
                    finishReason = choice.FinishReason;
                }

                if (!string.IsNullOrEmpty(chunk?.Model))
                {
                    model = chunk.Model;
                }
            }
            catch (JsonException)
            {
                // Skip malformed chunks
            }
        }

        // Estimate prompt tokens
        promptTokens = EstimateTokens(string.Join(" ", messages.Select(m => m.Content)));

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = "assistant",
                Content = fullContent.ToString()
            },
            Model = model,
            Usage = new AIUsage
            {
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = promptTokens + completionTokens
            },
            FinishReason = finishReason ?? "stop"
        };
    }

    /// <inheritdoc />
    public async Task<AIEmbeddingResponse> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
    {
        var deployment = model ?? _config.EmbeddingDeploymentName ?? _config.DeploymentName;
        var endpoint = $"{_config.Endpoint}/openai/deployments/{deployment}/embeddings?api-version={_config.ApiVersion}";

        var request = new AzureOpenAIEmbeddingRequest { Input = text };
        var response = await SendRequestAsync<AzureOpenAIEmbeddingResponse>(endpoint, request, cancellationToken);

        return new AIEmbeddingResponse
        {
            Embedding = response.Data?.FirstOrDefault()?.Embedding ?? Array.Empty<float>(),
            Model = response.Model,
            TokenCount = response.Usage?.TotalTokens ?? EstimateTokens(text)
        };
    }

    /// <inheritdoc />
    public async Task<AIBatchEmbeddingResponse> GetEmbeddingsAsync(IEnumerable<string> texts, string? model = null, CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        var deployment = model ?? _config.EmbeddingDeploymentName ?? _config.DeploymentName;
        var endpoint = $"{_config.Endpoint}/openai/deployments/{deployment}/embeddings?api-version={_config.ApiVersion}";

        var request = new AzureOpenAIEmbeddingRequest { Input = textList };
        var response = await SendRequestAsync<AzureOpenAIEmbeddingResponse>(endpoint, request, cancellationToken);

        return new AIBatchEmbeddingResponse
        {
            Embeddings = response.Data?.OrderBy(d => d.Index).Select(d => d.Embedding ?? Array.Empty<float>()).ToList() ?? new List<float[]>(),
            Model = response.Model,
            TotalTokens = response.Usage?.TotalTokens ?? textList.Sum(t => EstimateTokens(t))
        };
    }

    /// <inheritdoc />
    public async Task<AIEmailDraft> GenerateEmailDraftAsync(EmailDraftRequest request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a professional email writer for an enterprise CRM system.
Write clear, concise, and professional emails appropriate for business communication.
Always start with a subject line in the format: Subject: <subject>
Then write the email body.
Maintain a professional yet approachable tone.";

        var userPrompt = new StringBuilder();
        userPrompt.AppendLine($"Write a {request.Tone} business email for the following:");
        userPrompt.AppendLine($"Purpose: {request.Purpose}");

        if (!string.IsNullOrEmpty(request.RecipientName))
            userPrompt.AppendLine($"Recipient: {request.RecipientName}");
        if (!string.IsNullOrEmpty(request.CompanyName))
            userPrompt.AppendLine($"Company: {request.CompanyName}");
        if (request.KeyPoints?.Any() == true)
            userPrompt.AppendLine($"Key points to include: {string.Join(", ", request.KeyPoints)}");
        if (!string.IsNullOrEmpty(request.Context))
            userPrompt.AppendLine($"Additional context: {request.Context}");
        if (!string.IsNullOrEmpty(request.PreviousEmail))
            userPrompt.AppendLine($"\nPrevious email in thread:\n{request.PreviousEmail}");
        if (!string.IsNullOrEmpty(request.Length))
            userPrompt.AppendLine($"Desired length: {request.Length}");

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt.ToString() }
            },
            Temperature = 0.7,
            MaxTokens = 1000
        };

        var response = await ChatAsync(chatRequest, cancellationToken);
        return ParseEmailDraft(response.Message.Content, response.Usage);
    }

    /// <inheritdoc />
    public async Task<AIEmailDraft> SuggestReplyAsync(string originalEmail, string? context = null, string? tone = null, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a professional email writer for an enterprise CRM system.
Generate a thoughtful, professional reply to the email provided.
Start with a subject line in the format: Subject: Re: <original subject>
Then write the reply body.";

        var userPrompt = new StringBuilder();
        userPrompt.AppendLine("Generate a professional reply to this email:");
        userPrompt.AppendLine("---");
        userPrompt.AppendLine(originalEmail);
        userPrompt.AppendLine("---");

        if (!string.IsNullOrEmpty(context))
            userPrompt.AppendLine($"\nContext about the relationship/situation: {context}");
        if (!string.IsNullOrEmpty(tone))
            userPrompt.AppendLine($"Desired tone: {tone}");

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt.ToString() }
            },
            Temperature = 0.7,
            MaxTokens = 1000
        };

        var response = await ChatAsync(chatRequest, cancellationToken);
        return ParseEmailDraft(response.Message.Content, response.Usage);
    }

    /// <inheritdoc />
    public async Task<string> SummarizeAsync(string content, int? maxLength = null, CancellationToken cancellationToken = default)
    {
        var lengthInstruction = maxLength.HasValue
            ? $"Keep the summary under {maxLength} words."
            : "Keep the summary concise.";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = $"You are a helpful assistant that creates clear, concise summaries. {lengthInstruction}",
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = $"Please summarize the following:\n\n{content}" }
            },
            Temperature = 0.3,
            MaxTokens = maxLength ?? 500
        };

        var response = await ChatAsync(chatRequest, cancellationToken);
        return response.Message.Content;
    }

    /// <inheritdoc />
    public async Task<AIEntityExtractionResult> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a named entity recognition system.
Extract entities from the provided text and return them in JSON format.
Entity types to identify: person, company, email, phone, date, location, money, product, job_title.
Return format: [{""type"": ""entity_type"", ""value"": ""extracted_text"", ""confidence"": 0.0-1.0}]
Only return the JSON array, no other text.";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = text }
            },
            Temperature = 0.1,
            JsonMode = true,
            MaxTokens = 1000
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        var entities = new List<ExtractedEntity>();
        try
        {
            var jsonEntities = JsonSerializer.Deserialize<List<JsonElement>>(response.Message.Content);
            if (jsonEntities != null)
            {
                foreach (var entity in jsonEntities)
                {
                    entities.Add(new ExtractedEntity
                    {
                        Type = entity.GetProperty("type").GetString() ?? "unknown",
                        Value = entity.GetProperty("value").GetString() ?? string.Empty,
                        Confidence = entity.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.8
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse entity extraction response");
        }

        return new AIEntityExtractionResult
        {
            Entities = entities,
            Usage = response.Usage
        };
    }

    /// <inheritdoc />
    public async Task<AISentimentResult> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a sentiment analysis system.
Analyze the sentiment of the provided text and return a JSON response with:
- sentiment: ""positive"", ""negative"", ""neutral"", or ""mixed""
- score: a number from -1.0 (very negative) to 1.0 (very positive)
- confidence: a number from 0.0 to 1.0 indicating confidence
- emotions: an object with emotion intensities (joy, sadness, anger, fear, surprise) from 0.0 to 1.0
Only return the JSON object.";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = text }
            },
            Temperature = 0.1,
            JsonMode = true,
            MaxTokens = 200
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        try
        {
            var result = JsonSerializer.Deserialize<JsonElement>(response.Message.Content);
            var emotions = new Dictionary<string, double>();

            if (result.TryGetProperty("emotions", out var emotionsProp))
            {
                foreach (var prop in emotionsProp.EnumerateObject())
                {
                    if (prop.Value.TryGetDouble(out var value))
                        emotions[prop.Name] = value;
                }
            }

            return new AISentimentResult
            {
                Sentiment = result.GetProperty("sentiment").GetString() ?? "neutral",
                Score = result.TryGetProperty("score", out var scoreProp) ? scoreProp.GetDouble() : 0,
                Confidence = result.TryGetProperty("confidence", out var confProp) ? confProp.GetDouble() : 0.8,
                Emotions = emotions.Any() ? emotions : null,
                Usage = response.Usage
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse sentiment response");
            return new AISentimentResult
            {
                Sentiment = "neutral",
                Score = 0,
                Confidence = 0,
                Usage = response.Usage
            };
        }
    }

    /// <inheritdoc />
    public async Task<AIActionRecommendations> GetNextBestActionsAsync(AIActionContext context, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a CRM sales intelligence assistant.
Based on the entity context and activities provided, recommend the next best actions.
Return a JSON array of recommendations:
[{""type"": ""call|email|meeting|task"", ""title"": ""short action title"", ""description"": ""action details"", ""reasoning"": ""why this action is recommended"", ""confidence"": 0.0-1.0, ""priority"": 1-5}]
Prioritize actions that will move deals forward or strengthen relationships.
Only return the JSON array.";

        var userPrompt = new StringBuilder();
        userPrompt.AppendLine($"Entity Type: {context.EntityType}");
        userPrompt.AppendLine($"Entity ID: {context.EntityId}");
        if (context.EntityData != null)
            userPrompt.AppendLine($"Entity Data: {JsonSerializer.Serialize(context.EntityData)}");
        if (context.RecentActivities?.Any() == true)
            userPrompt.AppendLine($"Recent Activities:\n- {string.Join("\n- ", context.RecentActivities)}");
        if (!string.IsNullOrEmpty(context.Goal))
            userPrompt.AppendLine($"Current Goal: {context.Goal}");

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt.ToString() }
            },
            Temperature = 0.5,
            JsonMode = true,
            MaxTokens = 1000
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        var actions = new List<AIRecommendedAction>();
        try
        {
            var jsonActions = JsonSerializer.Deserialize<List<JsonElement>>(response.Message.Content);
            if (jsonActions != null)
            {
                foreach (var action in jsonActions)
                {
                    actions.Add(new AIRecommendedAction
                    {
                        Type = action.GetProperty("type").GetString() ?? "task",
                        Title = action.GetProperty("title").GetString() ?? string.Empty,
                        Description = action.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                        Reasoning = action.TryGetProperty("reasoning", out var reasonProp) ? reasonProp.GetString() : null,
                        Confidence = action.TryGetProperty("confidence", out var confProp) ? confProp.GetDouble() : 0.7,
                        Priority = action.TryGetProperty("priority", out var priProp) ? priProp.GetInt32() : 3
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse action recommendations");
        }

        return new AIActionRecommendations
        {
            Actions = actions,
            Usage = response.Usage
        };
    }

    /// <inheritdoc />
    public int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        // GPT tokenization: roughly 4 characters per token for English
        // More accurate would use tiktoken, but this is a reasonable estimate
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <inheritdoc />
    public Task<AIUsageStats> GetUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // Azure OpenAI usage is tracked in Azure Portal, not via API
        // Would need to integrate with Azure Cost Management API for real stats
        return Task.FromResult(new AIUsageStats
        {
            Provider = ProviderName,
            StartDate = startDate,
            EndDate = endDate,
            TotalRequests = 0,
            TotalInputTokens = 0,
            TotalOutputTokens = 0,
            EstimatedCost = null // Check Azure Portal for costs
        });
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await IsAvailableAsync(cancellationToken);
            if (!isAvailable)
            {
                return new ProviderHealthResult
                {
                    IsHealthy = false,
                    ProviderName = ProviderName,
                    Message = "Azure OpenAI endpoint not reachable",
                    CheckedAt = DateTime.UtcNow
                };
            }

            // Try a simple completion to verify the deployment works
            var testRequest = new AIChatRequest
            {
                Messages = new List<AIChatMessage>
                {
                    new() { Role = "user", Content = "Hello" }
                },
                MaxTokens = 5
            };

            await ChatAsync(testRequest, cancellationToken);

            return new ProviderHealthResult
            {
                IsHealthy = true,
                ProviderName = ProviderName,
                Message = "Azure OpenAI is operational",
                CheckedAt = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["endpoint"] = _config.Endpoint,
                    ["deployment"] = _config.DeploymentName,
                    ["api_version"] = _config.ApiVersion
                }
            };
        }
        catch (Exception ex)
        {
            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                Message = $"Health check failed: {ex.Message}",
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    #region Private Helpers

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("api-key", _config.ApiKey);
        return request;
    }

    private async Task<TResponse> SendRequestAsync<TResponse>(string endpoint, object request, CancellationToken cancellationToken)
    {
        var httpRequest = CreateRequest(HttpMethod.Post, endpoint);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Azure OpenAI request failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode();
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private AIEmailDraft ParseEmailDraft(string content, AIUsage usage)
    {
        var subject = "Email Draft";
        var body = content;

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
            {
                subject = line.Substring(8).Trim();
                var subjectIndex = content.IndexOf('\n');
                if (subjectIndex >= 0)
                    body = content.Substring(subjectIndex + 1).Trim();
                break;
            }
        }

        return new AIEmailDraft
        {
            Subject = subject,
            Body = body,
            Usage = usage
        };
    }

    #endregion
}
