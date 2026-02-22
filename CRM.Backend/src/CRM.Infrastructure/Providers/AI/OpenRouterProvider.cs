// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1011 // Closing square bracket should be followed by a space

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
/// Configuration options for OpenRouter AI gateway.
/// </summary>
public class OpenRouterConfiguration
{
    /// <summary>
    /// OpenRouter API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for OpenRouter API.
    /// Default: https://openrouter.ai/api/v1
    /// </summary>
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    /// <summary>
    /// Default model to use for chat/completion.
    /// Popular options: openai/gpt-4o, anthropic/claude-3-opus, google/gemini-pro, meta-llama/llama-3-70b-instruct
    /// </summary>
    public string DefaultModel { get; set; } = "openai/gpt-4o-mini";

    /// <summary>
    /// Default model for embeddings.
    /// </summary>
    public string EmbeddingModel { get; set; } = "openai/text-embedding-3-small";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Default maximum tokens.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Default temperature.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Site URL for rankings (optional).
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// Site name for rankings (optional).
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// Fallback models to try if primary fails.
    /// </summary>
    public List<string> FallbackModels { get; set; } = new()
    {
        "anthropic/claude-3-haiku",
        "google/gemini-flash-1.5",
        "meta-llama/llama-3-8b-instruct"
    };

    /// <summary>
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            return (false, "ApiKey is required");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, "BaseUrl is required");
        if (string.IsNullOrWhiteSpace(DefaultModel))
            return (false, "DefaultModel is required");
        return (true, null);
    }
}

#endregion

#region OpenRouter DTOs

internal class OpenRouterChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenRouterMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("response_format")]
    public OpenRouterResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("tools")]
    public List<OpenRouterTool>? Tools { get; set; }
}

internal class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal class OpenRouterResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text"; // "text" or "json_object"
}

internal class OpenRouterTool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public OpenRouterFunction? Function { get; set; }
}

internal class OpenRouterFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public Dictionary<string, object>? Parameters { get; set; }
}

internal class OpenRouterChatResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<OpenRouterChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenRouterUsage? Usage { get; set; }
}

internal class OpenRouterChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenRouterMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public OpenRouterMessage? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

internal class OpenRouterUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

internal class OpenRouterModelsResponse
{
    [JsonPropertyName("data")]
    public List<OpenRouterModelInfo>? Data { get; set; }
}

internal class OpenRouterModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }

    [JsonPropertyName("pricing")]
    public OpenRouterPricing? Pricing { get; set; }

    [JsonPropertyName("architecture")]
    public OpenRouterArchitecture? Architecture { get; set; }
}

internal class OpenRouterPricing
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("completion")]
    public string? Completion { get; set; }
}

internal class OpenRouterArchitecture
{
    [JsonPropertyName("modality")]
    public string? Modality { get; set; }

    [JsonPropertyName("tokenizer")]
    public string? Tokenizer { get; set; }
}

internal class OpenRouterStreamChunk
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenRouterChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenRouterUsage? Usage { get; set; }
}

internal class OpenRouterEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public object Input { get; set; } = string.Empty; // string or string[]
}

internal class OpenRouterEmbeddingResponse
{
    [JsonPropertyName("data")]
    public List<OpenRouterEmbeddingData>? Data { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("usage")]
    public OpenRouterUsage? Usage { get; set; }
}

internal class OpenRouterEmbeddingData
{
    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

#endregion

/// <summary>
/// AI provider implementation for OpenRouter (multi-model gateway).
/// Provides access to 100+ models including GPT-4, Claude, Gemini, Llama, etc.
/// </summary>
public class OpenRouterProvider : IAIPort
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterConfiguration _config;
    private readonly ILogger<OpenRouterProvider> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenRouterProvider(
        HttpClient httpClient,
        IOptions<OpenRouterConfiguration> config,
        ILogger<OpenRouterProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderName => "OpenRouter";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenRouter availability check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AIModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/models", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenRouterModelsResponse>(JsonOptions, cancellationToken);

            return result?.Data?.Select(m => new AIModelInfo
            {
                Id = m.Id,
                Name = m.Name ?? m.Id,
                Provider = ProviderName,
                Capabilities = GetModelCapabilities(m),
                MaxTokens = m.ContextLength,
                IsAvailable = true,
                InputCostPer1K = ParseCost(m.Pricing?.Prompt),
                OutputCostPer1K = ParseCost(m.Pricing?.Completion)
            }) ?? Enumerable.Empty<AIModelInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available models from OpenRouter");
            return Enumerable.Empty<AIModelInfo>();
        }
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
    {
        // OpenRouter uses chat completions, so we convert
        var chatRequest = new AIChatRequest
        {
            Model = request.Model,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = request.Prompt }
            },
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP
        };

        var chatResponse = await ChatAsync(chatRequest, cancellationToken);

        return new AICompletionResponse
        {
            Text = chatResponse.Message.Content,
            Model = chatResponse.Model,
            Usage = chatResponse.Usage,
            FinishReason = chatResponse.FinishReason
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? _config.DefaultModel;

        var messages = new List<OpenRouterMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new OpenRouterMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.AddRange(request.Messages.Select(m => new OpenRouterMessage
        {
            Role = m.Role,
            Content = m.Content,
            Name = m.Name
        }));

        var openRouterRequest = new OpenRouterChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = false,
            MaxTokens = request.MaxTokens ?? _config.DefaultMaxTokens,
            Temperature = request.Temperature ?? _config.DefaultTemperature,
            TopP = request.TopP,
            ResponseFormat = request.JsonMode ? new OpenRouterResponseFormat { Type = "json_object" } : null,
            Tools = request.Tools?.Select(t => new OpenRouterTool
            {
                Type = "function",
                Function = new OpenRouterFunction
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.Parameters
                }
            }).ToList()
        };

        var response = await SendChatRequestAsync(openRouterRequest, cancellationToken);

        var choice = response.Choices?.FirstOrDefault();

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
            FinishReason = choice?.FinishReason
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> StreamChatAsync(AIChatRequest request, Action<string> onToken, CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? _config.DefaultModel;

        var messages = new List<OpenRouterMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new OpenRouterMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.AddRange(request.Messages.Select(m => new OpenRouterMessage
        {
            Role = m.Role,
            Content = m.Content,
            Name = m.Name
        }));

        var openRouterRequest = new OpenRouterChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = true,
            MaxTokens = request.MaxTokens ?? _config.DefaultMaxTokens,
            Temperature = request.Temperature ?? _config.DefaultTemperature,
            TopP = request.TopP,
            ResponseFormat = request.JsonMode ? new OpenRouterResponseFormat { Type = "json_object" } : null
        };

        var content = new StringContent(
            JsonSerializer.Serialize(openRouterRequest, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat/completions")
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fullContent = new StringBuilder();
        var totalPromptTokens = 0;
        var totalCompletionTokens = 0;
        var finalModel = model;
        string? finishReason = null;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (!line.StartsWith("data: "))
                continue;

            var data = line.Substring(6);
            if (data == "[DONE]")
                break;

            try
            {
                var chunk = JsonSerializer.Deserialize<OpenRouterStreamChunk>(data, JsonOptions);
                var delta = chunk?.Choices?.FirstOrDefault()?.Delta;

                if (delta?.Content != null)
                {
                    onToken(delta.Content);
                    fullContent.Append(delta.Content);
                }

                var choiceFinishReason = chunk?.Choices?.FirstOrDefault()?.FinishReason;
                if (choiceFinishReason != null)
                {
                    finishReason = choiceFinishReason;
                }

                if (chunk?.Usage != null)
                {
                    totalPromptTokens = chunk.Usage.PromptTokens;
                    totalCompletionTokens = chunk.Usage.CompletionTokens;
                }

                if (chunk?.Model != null)
                {
                    finalModel = chunk.Model;
                }
            }
            catch (JsonException)
            {
                // Skip malformed lines
            }
        }

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = "assistant",
                Content = fullContent.ToString()
            },
            Model = finalModel,
            Usage = new AIUsage
            {
                PromptTokens = totalPromptTokens,
                CompletionTokens = totalCompletionTokens,
                TotalTokens = totalPromptTokens + totalCompletionTokens
            },
            FinishReason = finishReason ?? "stop"
        };
    }

    /// <inheritdoc />
    public async Task<AIEmbeddingResponse> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
    {
        var embeddingModel = model ?? _config.EmbeddingModel;

        var request = new OpenRouterEmbeddingRequest
        {
            Model = embeddingModel,
            Input = text
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/embeddings", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterEmbeddingResponse>(JsonOptions, cancellationToken);

        return new AIEmbeddingResponse
        {
            Embedding = result?.Data?.FirstOrDefault()?.Embedding ?? Array.Empty<float>(),
            Model = result?.Model ?? embeddingModel,
            TokenCount = result?.Usage?.TotalTokens ?? EstimateTokens(text)
        };
    }

    /// <inheritdoc />
    public async Task<AIBatchEmbeddingResponse> GetEmbeddingsAsync(IEnumerable<string> texts, string? model = null, CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        var embeddingModel = model ?? _config.EmbeddingModel;

        var request = new OpenRouterEmbeddingRequest
        {
            Model = embeddingModel,
            Input = textList
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/embeddings", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterEmbeddingResponse>(JsonOptions, cancellationToken);

        var embeddings = result?.Data?
            .OrderBy(d => d.Index)
            .Select(d => d.Embedding ?? Array.Empty<float>())
            .ToList() ?? new List<float[]>();

        return new AIBatchEmbeddingResponse
        {
            Embeddings = embeddings,
            Model = result?.Model ?? embeddingModel,
            TotalTokens = result?.Usage?.TotalTokens ?? textList.Sum(t => EstimateTokens(t))
        };
    }

    /// <inheritdoc />
    public async Task<AIEmailDraft> GenerateEmailDraftAsync(EmailDraftRequest request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a professional email writer for a CRM system.
Write clear, concise, and professional emails.
Always include a subject line at the start (format: Subject: <subject>)
Then write the email body.";

        var userPrompt = new StringBuilder();
        userPrompt.AppendLine($"Write a {request.Tone} email for the following purpose:");
        userPrompt.AppendLine($"Purpose: {request.Purpose}");

        if (!string.IsNullOrEmpty(request.RecipientName))
            userPrompt.AppendLine($"Recipient: {request.RecipientName}");
        if (!string.IsNullOrEmpty(request.CompanyName))
            userPrompt.AppendLine($"Company: {request.CompanyName}");
        if (request.KeyPoints?.Any() == true)
            userPrompt.AppendLine($"Key points: {string.Join(", ", request.KeyPoints)}");
        if (!string.IsNullOrEmpty(request.Context))
            userPrompt.AppendLine($"Additional context: {request.Context}");
        if (!string.IsNullOrEmpty(request.Length))
            userPrompt.AppendLine($"Length: {request.Length}");

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt.ToString() }
            },
            Temperature = 0.7
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        return ParseEmailDraft(response.Message.Content, response.Usage);
    }

    /// <inheritdoc />
    public async Task<AIEmailDraft> SuggestReplyAsync(string originalEmail, string? context = null, string? tone = null, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are a professional email writer for a CRM system.
Generate a thoughtful reply to the email provided.
Always include a subject line at the start (format: Subject: <subject>)
Then write the reply body.";

        var userPrompt = new StringBuilder();
        userPrompt.AppendLine("Generate a reply to this email:");
        userPrompt.AppendLine(originalEmail);

        if (!string.IsNullOrEmpty(context))
            userPrompt.AppendLine($"\nContext: {context}");
        if (!string.IsNullOrEmpty(tone))
            userPrompt.AppendLine($"\nTone: {tone}");

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt.ToString() }
            },
            Temperature = 0.7
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        return ParseEmailDraft(response.Message.Content, response.Usage);
    }

    /// <inheritdoc />
    public async Task<string> SummarizeAsync(string content, int? maxLength = null, CancellationToken cancellationToken = default)
    {
        var userPrompt = $"Summarize the following content{(maxLength.HasValue ? $" in {maxLength} words or less" : "")}:\n\n{content}";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = "You are a helpful assistant that creates concise summaries.",
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt }
            },
            Temperature = 0.3
        };

        var response = await ChatAsync(chatRequest, cancellationToken);
        return response.Message.Content;
    }

    /// <inheritdoc />
    public async Task<AIEntityExtractionResult> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"You are an entity extraction assistant.
Extract named entities from the text and return them in JSON format.
Categories: person, company, email, phone, date, location, money, product.
Format: [{""type"": ""category"", ""value"": ""extracted text"", ""confidence"": 0.9}]";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = $"Extract entities from: {text}" }
            },
            Temperature = 0.1,
            JsonMode = true
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
        var systemPrompt = @"You are a sentiment analysis assistant.
Analyze the sentiment and return JSON format:
{""sentiment"": ""positive|negative|neutral|mixed"", ""score"": -1.0 to 1.0, ""confidence"": 0.0 to 1.0, ""emotions"": {""joy"": 0.0-1.0, ""sadness"": 0.0-1.0, ""anger"": 0.0-1.0, ""fear"": 0.0-1.0}}";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = $"Analyze sentiment: {text}" }
            },
            Temperature = 0.1,
            JsonMode = true
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
                    emotions[prop.Name] = prop.Value.GetDouble();
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
            _logger.LogWarning(ex, "Failed to parse sentiment analysis response");
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
        var systemPrompt = @"You are a CRM action recommendation assistant.
Based on the entity context and recent activities, suggest next best actions.
Return JSON format:
[{""type"": ""call|email|meeting|task"", ""title"": ""action title"", ""description"": ""details"", ""reasoning"": ""why this action"", ""confidence"": 0.0-1.0, ""priority"": 1-5}]";

        var userPrompt = new StringBuilder();
        userPrompt.AppendLine($"Entity Type: {context.EntityType}");
        userPrompt.AppendLine($"Entity ID: {context.EntityId}");
        if (context.EntityData != null)
            userPrompt.AppendLine($"Entity Data: {JsonSerializer.Serialize(context.EntityData)}");
        if (context.RecentActivities?.Any() == true)
            userPrompt.AppendLine($"Recent Activities: {string.Join("; ", context.RecentActivities)}");
        if (!string.IsNullOrEmpty(context.Goal))
            userPrompt.AppendLine($"Goal: {context.Goal}");

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = userPrompt.ToString() }
            },
            Temperature = 0.5,
            JsonMode = true
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
            _logger.LogWarning(ex, "Failed to parse action recommendations response");
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
        // Rough estimation: ~4 characters per token for English
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <inheritdoc />
    public Task<AIUsageStats> GetUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // OpenRouter provides usage via API but requires account-level access
        // Return placeholder stats
        return Task.FromResult(new AIUsageStats
        {
            Provider = ProviderName,
            StartDate = startDate,
            EndDate = endDate,
            TotalRequests = 0,
            TotalInputTokens = 0,
            TotalOutputTokens = 0,
            EstimatedCost = null
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
                    Message = "OpenRouter API not reachable",
                    CheckedAt = DateTime.UtcNow
                };
            }

            var models = await GetAvailableModelsAsync(cancellationToken);
            var modelList = models.ToList();

            return new ProviderHealthResult
            {
                IsHealthy = true,
                ProviderName = ProviderName,
                Message = $"Connected with access to {modelList.Count} models",
                CheckedAt = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["models_count"] = modelList.Count,
                    ["default_model"] = _config.DefaultModel,
                    ["embedding_model"] = _config.EmbeddingModel,
                    ["base_url"] = _config.BaseUrl
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

    private async Task<OpenRouterChatResponse> SendChatRequestAsync(OpenRouterChatRequest request, CancellationToken cancellationToken)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/chat/completions", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenRouter API error: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Try fallback models
            if (_config.FallbackModels?.Any() == true && request.Model != _config.FallbackModels.First())
            {
                foreach (var fallbackModel in _config.FallbackModels)
                {
                    _logger.LogWarning("Trying fallback model: {Model}", fallbackModel);
                    request.Model = fallbackModel;

                    content = new StringContent(
                        JsonSerializer.Serialize(request, JsonOptions),
                        Encoding.UTF8,
                        "application/json");

                    response = await _httpClient.PostAsync("/chat/completions", content, cancellationToken);
                    if (response.IsSuccessStatusCode)
                        break;
                }
            }

            response.EnsureSuccessStatusCode();
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OpenRouterChatResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize OpenRouter response");
    }

    private List<string> GetModelCapabilities(OpenRouterModelInfo model)
    {
        var capabilities = new List<string> { "chat" };

        var modality = model.Architecture?.Modality?.ToLowerInvariant() ?? "";
        var modelId = model.Id.ToLowerInvariant();

        if (modality.Contains("image") || modelId.Contains("vision") || modelId.Contains("gpt-4o"))
            capabilities.Add("vision");
        if (modelId.Contains("embed"))
            capabilities.Add("embedding");
        if (modelId.Contains("code") || modelId.Contains("deepseek-coder"))
            capabilities.Add("code");

        return capabilities;
    }

    private decimal? ParseCost(string? costString)
    {
        if (string.IsNullOrEmpty(costString))
            return null;
        if (decimal.TryParse(costString, out var cost))
        {
            // OpenRouter reports cost per token, multiply by 1000 for per-1K
            return cost * 1000;
        }
        return null;
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
                body = content.Substring(content.IndexOf('\n') + 1).Trim();
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
