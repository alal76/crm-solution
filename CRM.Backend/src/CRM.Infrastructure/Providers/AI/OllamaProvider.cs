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
/// Configuration options for Ollama local LLM provider.
/// </summary>
public class OllamaConfiguration
{
    /// <summary>
    /// Base URL of the Ollama API server.
    /// Default: http://localhost:11434
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434"; // NOSONAR - S5332: internal container-to-container URL, HTTPS not applicable in Docker bridge network

    /// <summary>
    /// Default model to use for chat/completion.
    /// </summary>
    public string DefaultModel { get; set; } = "llama3";

    /// <summary>
    /// Default model for embeddings.
    /// </summary>
    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Default maximum tokens.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 2048;

    /// <summary>
    /// Default temperature.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Keep model loaded in memory (faster subsequent requests).
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    /// <summary>
    /// Number of GPU layers to use (0 = CPU only).
    /// </summary>
    public int? NumGpuLayers { get; set; }

    /// <summary>
    /// Context window size.
    /// </summary>
    public int? NumCtx { get; set; }

    /// <summary>
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            return (false, "BaseUrl is required");
        }
        if (string.IsNullOrWhiteSpace(DefaultModel))
        {
            return (false, "DefaultModel is required");
        }
        return (true, null);
    }
}

#endregion

#region Ollama DTOs

internal class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; } // "json" for JSON mode

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; } // e.g., "5m" or "24h"
}

internal class OllamaMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; set; } // max tokens

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; set; }

    [JsonPropertyName("num_gpu")]
    public int? NumGpu { get; set; }
}

internal class OllamaChatResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public OllamaMessage? Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("done_reason")]
    public string? DoneReason { get; set; }

    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    public long? LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; set; }

    [JsonPropertyName("eval_count")]
    public int? EvalCount { get; set; }
}

internal class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; }
}

internal class OllamaGenerateResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("done_reason")]
    public string? DoneReason { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; set; }

    [JsonPropertyName("eval_count")]
    public int? EvalCount { get; set; }
}

internal class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
}

internal class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
#pragma warning disable SA1011 // Closing square bracket should be spaced correctly
    public float[]? Embedding { get; set; }
#pragma warning restore SA1011 // Closing square bracket should be spaced correctly
}

internal class OllamaModelsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModelInfo>? Models { get; set; }
}

internal class OllamaModelInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("details")]
    public OllamaModelDetails? Details { get; set; }
}

internal class OllamaModelDetails
{
    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("parameter_size")]
    public string? ParameterSize { get; set; }

    [JsonPropertyName("quantization_level")]
    public string? QuantizationLevel { get; set; }
}

#endregion

/// <summary>
/// AI provider implementation for Ollama (local LLM).
/// Provides privacy-focused, self-hosted AI capabilities.
/// </summary>
public class OllamaProvider : IAIPort
{
    private const int MaxPredictTokensCap = 256;

    private readonly HttpClient _httpClient;
    private readonly OllamaConfiguration _config;
    private readonly ILogger<OllamaProvider> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OllamaProvider(
        HttpClient httpClient,
        IOptions<OllamaConfiguration> config,
        ILogger<OllamaProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderName => "Ollama";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama availability check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AIModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaModelsResponse>(JsonOptions, cancellationToken);

            return result?.Models?.Select(m => new AIModelInfo
            {
                Id = m.Name,
                Name = m.Name,
                Provider = ProviderName,
                Capabilities = GetModelCapabilities(m.Name),
                MaxTokens = GetModelMaxTokens(m.Name),
                IsAvailable = true
            }) ?? Enumerable.Empty<AIModelInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available models from Ollama");
            return Enumerable.Empty<AIModelInfo>();
        }
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _config.DefaultModel : request.Model;
        if (string.IsNullOrWhiteSpace(model))
        {
            _logger.LogWarning("Ollama model resolved to empty in CompleteAsync — falling back to 'olmo2:7b'");
            model = "olmo2:7b";
        }

        var numPredict = request.MaxTokens ?? _config.DefaultMaxTokens;
        if (numPredict > MaxPredictTokensCap)
        {
            numPredict = MaxPredictTokensCap;
        }

        var ollamaRequest = new OllamaGenerateRequest
        {
            Model = model,
            Prompt = request.Prompt,
            Stream = false,
            Options = new OllamaOptions
            {
                Temperature = request.Temperature ?? _config.DefaultTemperature,
                TopP = request.TopP,
                NumPredict = numPredict,
                Stop = request.StopSequences
            },
            KeepAlive = _config.KeepAlive ? "24h" : "5m"
        };

        var response = await SendRequestAsync<OllamaGenerateResponse>("/api/generate", ollamaRequest, cancellationToken);

        return new AICompletionResponse
        {
            Text = response.Response,
            Model = response.Model,
            Usage = new AIUsage
            {
                PromptTokens = response.PromptEvalCount ?? 0,
                CompletionTokens = response.EvalCount ?? 0,
                TotalTokens = (response.PromptEvalCount ?? 0) + (response.EvalCount ?? 0)
            },
            FinishReason = response.DoneReason ?? (response.Done ? "stop" : null)
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _config.DefaultModel : request.Model;
        if (string.IsNullOrWhiteSpace(model))
        {
            _logger.LogWarning("Ollama model resolved to empty in ChatAsync — falling back to 'olmo2:7b'");
            model = "olmo2:7b";
        }

        var numPredict = request.MaxTokens ?? _config.DefaultMaxTokens;
        if (numPredict > MaxPredictTokensCap)
        {
            numPredict = MaxPredictTokensCap;
        }

        var messages = new List<OllamaMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new OllamaMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.AddRange(request.Messages.Select(m => new OllamaMessage
        {
            Role = m.Role,
            Content = m.Content
        }));

        var ollamaRequest = new OllamaChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = false,
            Format = request.JsonMode ? "json" : null,
            Options = new OllamaOptions
            {
                Temperature = request.Temperature ?? _config.DefaultTemperature,
                TopP = request.TopP,
                NumPredict = numPredict
            },
            KeepAlive = _config.KeepAlive ? "24h" : "5m"
        };

        var response = await SendRequestAsync<OllamaChatResponse>("/api/chat", ollamaRequest, cancellationToken);

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = response.Message?.Role ?? "assistant",
                Content = response.Message?.Content ?? string.Empty
            },
            Model = response.Model,
            Usage = new AIUsage
            {
                PromptTokens = response.PromptEvalCount ?? 0,
                CompletionTokens = response.EvalCount ?? 0,
                TotalTokens = (response.PromptEvalCount ?? 0) + (response.EvalCount ?? 0)
            },
            FinishReason = response.DoneReason ?? (response.Done ? "stop" : null)
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> StreamChatAsync(AIChatRequest request, Action<string> onToken, CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _config.DefaultModel : request.Model;
        if (string.IsNullOrWhiteSpace(model))
        {
            _logger.LogWarning("Ollama model resolved to empty in StreamChatAsync — falling back to 'olmo2:7b'");
            model = "olmo2:7b";
        }

        var numPredict = request.MaxTokens ?? _config.DefaultMaxTokens;
        if (numPredict > MaxPredictTokensCap)
        {
            numPredict = MaxPredictTokensCap;
        }

        var messages = new List<OllamaMessage>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new OllamaMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.AddRange(request.Messages.Select(m => new OllamaMessage
        {
            Role = m.Role,
            Content = m.Content
        }));

        var ollamaRequest = new OllamaChatRequest
        {
            Model = model,
            Messages = messages,
            Stream = true,
            Format = request.JsonMode ? "json" : null,
            Options = new OllamaOptions
            {
                Temperature = request.Temperature ?? _config.DefaultTemperature,
                TopP = request.TopP,
                NumPredict = numPredict
            },
            KeepAlive = _config.KeepAlive ? "24h" : "5m"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(ollamaRequest, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
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
            {
                continue;
            }

            try
            {
                var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, JsonOptions);
                if (chunk?.Message?.Content != null)
                {
                    onToken(chunk.Message.Content);
                    fullContent.Append(chunk.Message.Content);
                }

                if (chunk?.Done == true)
                {
                    totalPromptTokens = chunk.PromptEvalCount ?? 0;
                    totalCompletionTokens = chunk.EvalCount ?? 0;
                    finalModel = chunk.Model;
                    finishReason = chunk.DoneReason ?? "stop";
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
            FinishReason = finishReason
        };
    }

    /// <inheritdoc />
    public async Task<AIEmbeddingResponse> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
    {
        var embeddingModel = model ?? _config.EmbeddingModel;

        var request = new OllamaEmbeddingRequest
        {
            Model = embeddingModel,
            Prompt = text
        };

        var response = await SendRequestAsync<OllamaEmbeddingResponse>("/api/embeddings", request, cancellationToken);

        return new AIEmbeddingResponse
        {
            Embedding = response.Embedding ?? Array.Empty<float>(),
            Model = embeddingModel,
            TokenCount = EstimateTokens(text)
        };
    }

    /// <inheritdoc />
    public async Task<AIBatchEmbeddingResponse> GetEmbeddingsAsync(IEnumerable<string> texts, string? model = null, CancellationToken cancellationToken = default)
    {
        var embeddings = new List<float[]>();
        var totalTokens = 0;
        var embeddingModel = model ?? _config.EmbeddingModel;

        foreach (var text in texts)
        {
            var result = await GetEmbeddingAsync(text, embeddingModel, cancellationToken);
            embeddings.Add(result.Embedding);
            totalTokens += result.TokenCount;
        }

        return new AIBatchEmbeddingResponse
        {
            Embeddings = embeddings,
            Model = embeddingModel,
            TotalTokens = totalTokens
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
        {
            userPrompt.AppendLine($"Recipient: {request.RecipientName}");
        }
        if (!string.IsNullOrEmpty(request.CompanyName))
        {
            userPrompt.AppendLine($"Company: {request.CompanyName}");
        }
        if (request.KeyPoints?.Any() == true)
        {
            userPrompt.AppendLine($"Key points: {string.Join(", ", request.KeyPoints)}");
        }
        if (!string.IsNullOrEmpty(request.Context))
        {
            userPrompt.AppendLine($"Additional context: {request.Context}");
        }
        if (!string.IsNullOrEmpty(request.Length))
        {
            userPrompt.AppendLine($"Length: {request.Length}");
        }

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
        {
            userPrompt.AppendLine($"\nContext: {context}");
        }
        if (!string.IsNullOrEmpty(tone))
        {
            userPrompt.AppendLine($"\nTone: {tone}");
        }

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
            Dictionary<string, double>? emotions = null;

            if (result.TryGetProperty("emotions", out var emotionsProp))
            {
                var emotionsDict = new Dictionary<string, double>();
                foreach (var prop in emotionsProp.EnumerateObject())
                {
                    emotionsDict[prop.Name] = prop.Value.GetDouble();
                }
                // NOSONAR S2583 - populated by EnumerateObject foreach above
                if (emotionsDict.Count > 0)
                {
                    emotions = emotionsDict;
                }
            }

            return new AISentimentResult
            {
                Sentiment = result.GetProperty("sentiment").GetString() ?? "neutral",
                Score = result.TryGetProperty("score", out var scoreProp) ? scoreProp.GetDouble() : 0,
                Confidence = result.TryGetProperty("confidence", out var confProp) ? confProp.GetDouble() : 0.8,
                Emotions = emotions,
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
        {
            userPrompt.AppendLine($"Entity Data: {JsonSerializer.Serialize(context.EntityData)}");
        }
        if (context.RecentActivities?.Any() == true)
        {
            userPrompt.AppendLine($"Recent Activities: {string.Join("; ", context.RecentActivities)}");
        }
        if (!string.IsNullOrEmpty(context.Goal))
        {
            userPrompt.AppendLine($"Goal: {context.Goal}");
        }

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
        {
            return 0;
        }
        // Rough estimation: ~4 characters per token for English
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <inheritdoc />
    public Task<AIUsageStats> GetUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // Ollama doesn't track usage statistics - return empty stats
        return Task.FromResult(new AIUsageStats
        {
            Provider = ProviderName,
            StartDate = startDate,
            EndDate = endDate,
            TotalRequests = 0,
            TotalInputTokens = 0,
            TotalOutputTokens = 0,
            EstimatedCost = 0 // Local = free
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
                    Message = "Ollama server not reachable",
                    CheckedAt = DateTime.UtcNow
                };
            }

            var models = await GetAvailableModelsAsync(cancellationToken);
            var modelList = models.ToList();

            return new ProviderHealthResult
            {
                IsHealthy = true,
                ProviderName = ProviderName,
                Message = $"Connected with {modelList.Count} models available",
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

    private async Task<TResponse> SendRequestAsync<TResponse>(string endpoint, object request, CancellationToken cancellationToken)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private List<string> GetModelCapabilities(string modelName)
    {
        var capabilities = new List<string> { "chat", "completion" };

        var lowerName = modelName.ToLowerInvariant();

        if (lowerName.Contains("embed") || lowerName.Contains("nomic"))
        {
            capabilities.Add("embedding");
        }
        if (lowerName.Contains("vision") || lowerName.Contains("llava"))
        {
            capabilities.Add("vision");
        }
        if (lowerName.Contains("code") || lowerName.Contains("deepseek-coder") || lowerName.Contains("codellama"))
        {
            capabilities.Add("code");
        }

        return capabilities;
    }

    private int? GetModelMaxTokens(string modelName)
    {
        var lowerName = modelName.ToLowerInvariant();

        // Rough estimates based on common models
        if (lowerName.Contains("llama3"))
        {
            return 8192;
        }
        if (lowerName.Contains("llama2"))
        {
            return 4096;
        }
        if (lowerName.Contains("mistral"))
        {
            return 32768;
        }
        if (lowerName.Contains("mixtral"))
        {
            return 32768;
        }
        if (lowerName.Contains("qwen"))
        {
            return 32768;
        }
        if (lowerName.Contains("phi"))
        {
            return 2048;
        }
        if (lowerName.Contains("gemma"))
        {
            return 8192;
        }

        return 4096; // Default
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
