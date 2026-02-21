// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.AI;

#region Configuration

/// <summary>
/// Configuration options for AWS Bedrock provider.
/// </summary>
public class BedrockConfiguration
{
    /// <summary>
    /// AWS Region for Bedrock.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// AWS Access Key ID.
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// AWS Secret Access Key.
    /// </summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// AWS Session Token (for temporary credentials).
    /// </summary>
    public string? SessionToken { get; set; }

    /// <summary>
    /// Use IAM role credentials from environment/instance profile.
    /// </summary>
    public bool UseDefaultCredentials { get; set; } = true;

    /// <summary>
    /// Default model ID for chat/completion.
    /// Format: anthropic.claude-3-sonnet-20240229-v1:0
    /// </summary>
    public string DefaultModelId { get; set; } = "anthropic.claude-3-sonnet-20240229-v1:0";

    /// <summary>
    /// Model ID for embeddings (e.g., amazon.titan-embed-text-v1).
    /// </summary>
    public string EmbeddingModelId { get; set; } = "amazon.titan-embed-text-v1";

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
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(Region))
            return (false, "Region is required");
        if (!UseDefaultCredentials && (string.IsNullOrWhiteSpace(AccessKeyId) || string.IsNullOrWhiteSpace(SecretAccessKey)))
            return (false, "AccessKeyId and SecretAccessKey are required when not using default credentials");
        if (string.IsNullOrWhiteSpace(DefaultModelId))
            return (false, "DefaultModelId is required");
        return (true, null);
    }
}

#endregion

#region Bedrock DTOs

// Claude model request/response structures (most common Bedrock model)

internal class BedrockClaudeRequest
{
    [JsonPropertyName("anthropic_version")]
    public string AnthropicVersion { get; set; } = "bedrock-2023-05-31";

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    public List<BedrockClaudeMessage> Messages { get; set; } = new();

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stop_sequences")]
    public List<string>? StopSequences { get; set; }
}

internal class BedrockClaudeMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public object Content { get; set; } = string.Empty; // Can be string or array of content blocks
}

internal class BedrockClaudeContentBlock
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

internal class BedrockClaudeResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public List<BedrockClaudeContentBlock>? Content { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }

    [JsonPropertyName("usage")]
    public BedrockClaudeUsage? Usage { get; set; }
}

internal class BedrockClaudeUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
}

// Titan embedding model structures

internal class BedrockTitanEmbeddingRequest
{
    [JsonPropertyName("inputText")]
    public string InputText { get; set; } = string.Empty;
}

internal class BedrockTitanEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }

    [JsonPropertyName("inputTextTokenCount")]
    public int InputTextTokenCount { get; set; }
}

// Streaming structures

internal class BedrockStreamEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("delta")]
    public BedrockStreamDelta? Delta { get; set; }

    [JsonPropertyName("message")]
    public BedrockClaudeResponse? Message { get; set; }

    [JsonPropertyName("usage")]
    public BedrockClaudeUsage? Usage { get; set; }
}

internal class BedrockStreamDelta
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }
}

#endregion

/// <summary>
/// AI provider implementation for AWS Bedrock.
/// Provides access to foundation models like Claude, Titan, etc.
/// </summary>
public class BedrockProvider : IAIPort
{
    private readonly HttpClient _httpClient;
    private readonly BedrockConfiguration _config;
    private readonly ILogger<BedrockProvider> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public BedrockProvider(
        HttpClient httpClient,
        IOptions<BedrockConfiguration> config,
        ILogger<BedrockProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderName => "Bedrock";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Bedrock doesn't have a simple ping endpoint
            // We check by listing foundation models (requires bedrock:ListFoundationModels permission)
            var endpoint = $"https://bedrock.{_config.Region}.amazonaws.com/foundation-models";
            var request = await CreateSignedRequestAsync(HttpMethod.Get, endpoint, null, cancellationToken);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bedrock availability check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<AIModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        // Common Bedrock models
        var models = new List<AIModelInfo>
        {
            new AIModelInfo
            {
                Id = "anthropic.claude-3-sonnet-20240229-v1:0",
                Name = "Claude 3 Sonnet",
                Provider = ProviderName,
                Capabilities = new List<string> { "chat", "completion" },
                MaxTokens = 200000,
                IsAvailable = true
            },
            new AIModelInfo
            {
                Id = "anthropic.claude-3-haiku-20240307-v1:0",
                Name = "Claude 3 Haiku",
                Provider = ProviderName,
                Capabilities = new List<string> { "chat", "completion" },
                MaxTokens = 200000,
                IsAvailable = true
            },
            new AIModelInfo
            {
                Id = "anthropic.claude-3-opus-20240229-v1:0",
                Name = "Claude 3 Opus",
                Provider = ProviderName,
                Capabilities = new List<string> { "chat", "completion" },
                MaxTokens = 200000,
                IsAvailable = true
            },
            new AIModelInfo
            {
                Id = "amazon.titan-embed-text-v1",
                Name = "Titan Text Embeddings V1",
                Provider = ProviderName,
                Capabilities = new List<string> { "embedding" },
                IsAvailable = true
            },
            new AIModelInfo
            {
                Id = "amazon.titan-embed-text-v2:0",
                Name = "Titan Text Embeddings V2",
                Provider = ProviderName,
                Capabilities = new List<string> { "embedding" },
                IsAvailable = true
            },
            new AIModelInfo
            {
                Id = "meta.llama3-8b-instruct-v1:0",
                Name = "Llama 3 8B Instruct",
                Provider = ProviderName,
                Capabilities = new List<string> { "chat", "completion" },
                MaxTokens = 8192,
                IsAvailable = true
            },
            new AIModelInfo
            {
                Id = "meta.llama3-70b-instruct-v1:0",
                Name = "Llama 3 70B Instruct",
                Provider = ProviderName,
                Capabilities = new List<string> { "chat", "completion" },
                MaxTokens = 8192,
                IsAvailable = true
            }
        };

        return Task.FromResult<IEnumerable<AIModelInfo>>(models);
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default)
    {
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
        var modelId = request.Model ?? _config.DefaultModelId;

        // Route to appropriate handler based on model
        if (modelId.StartsWith("anthropic.claude"))
        {
            return await ChatWithClaudeAsync(request, modelId, cancellationToken);
        }
        else if (modelId.StartsWith("meta.llama"))
        {
            return await ChatWithLlamaAsync(request, modelId, cancellationToken);
        }
        else
        {
            // Default to Claude format for unknown models
            return await ChatWithClaudeAsync(request, modelId, cancellationToken);
        }
    }

    private async Task<AIChatResponse> ChatWithClaudeAsync(AIChatRequest request, string modelId, CancellationToken cancellationToken)
    {
        var claudeRequest = new BedrockClaudeRequest
        {
            MaxTokens = request.MaxTokens ?? _config.DefaultMaxTokens,
            Temperature = request.Temperature ?? _config.DefaultTemperature,
            TopP = request.TopP,
            System = request.SystemPrompt,
            Messages = request.Messages.Select(m => new BedrockClaudeMessage
            {
                Role = m.Role == "user" ? "user" : "assistant",
                Content = m.Content
            }).ToList()
        };

        var endpoint = $"https://bedrock-runtime.{_config.Region}.amazonaws.com/model/{modelId}/invoke";
        var body = JsonSerializer.Serialize(claudeRequest, JsonOptions);

        var httpRequest = await CreateSignedRequestAsync(HttpMethod.Post, endpoint, body, cancellationToken);
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Bedrock request failed: {StatusCode} - {Error}", response.StatusCode, error);
            response.EnsureSuccessStatusCode();
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var claudeResponse = JsonSerializer.Deserialize<BedrockClaudeResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize Bedrock response");

        var text = claudeResponse.Content?.FirstOrDefault(c => c.Type == "text")?.Text ?? string.Empty;

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = "assistant",
                Content = text
            },
            Model = modelId,
            Usage = new AIUsage
            {
                PromptTokens = claudeResponse.Usage?.InputTokens ?? 0,
                CompletionTokens = claudeResponse.Usage?.OutputTokens ?? 0,
                TotalTokens = (claudeResponse.Usage?.InputTokens ?? 0) + (claudeResponse.Usage?.OutputTokens ?? 0)
            },
            FinishReason = claudeResponse.StopReason
        };
    }

    private async Task<AIChatResponse> ChatWithLlamaAsync(AIChatRequest request, string modelId, CancellationToken cancellationToken)
    {
        // Llama models use a different request format
        var prompt = BuildLlamaPrompt(request);

        var llamaRequest = new
        {
            prompt = prompt,
            max_gen_len = request.MaxTokens ?? _config.DefaultMaxTokens,
            temperature = request.Temperature ?? _config.DefaultTemperature,
            top_p = request.TopP ?? 0.9
        };

        var endpoint = $"https://bedrock-runtime.{_config.Region}.amazonaws.com/model/{modelId}/invoke";
        var body = JsonSerializer.Serialize(llamaRequest);

        var httpRequest = await CreateSignedRequestAsync(HttpMethod.Post, endpoint, body, cancellationToken);
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var llamaResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        var generation = llamaResponse.GetProperty("generation").GetString() ?? string.Empty;
        var promptTokenCount = llamaResponse.TryGetProperty("prompt_token_count", out var ptc) ? ptc.GetInt32() : EstimateTokens(prompt);
        var generationTokenCount = llamaResponse.TryGetProperty("generation_token_count", out var gtc) ? gtc.GetInt32() : EstimateTokens(generation);

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = "assistant",
                Content = generation
            },
            Model = modelId,
            Usage = new AIUsage
            {
                PromptTokens = promptTokenCount,
                CompletionTokens = generationTokenCount,
                TotalTokens = promptTokenCount + generationTokenCount
            },
            FinishReason = llamaResponse.TryGetProperty("stop_reason", out var sr) ? sr.GetString() : "stop"
        };
    }

    /// <inheritdoc />
    public async Task<AIChatResponse> StreamChatAsync(AIChatRequest request, Action<string> onToken, CancellationToken cancellationToken = default)
    {
        var modelId = request.Model ?? _config.DefaultModelId;

        if (!modelId.StartsWith("anthropic.claude"))
        {
            // Fallback to non-streaming for non-Claude models
            var response = await ChatAsync(request, cancellationToken);
            onToken(response.Message.Content);
            return response;
        }

        var claudeRequest = new BedrockClaudeRequest
        {
            MaxTokens = request.MaxTokens ?? _config.DefaultMaxTokens,
            Temperature = request.Temperature ?? _config.DefaultTemperature,
            TopP = request.TopP,
            System = request.SystemPrompt,
            Messages = request.Messages.Select(m => new BedrockClaudeMessage
            {
                Role = m.Role == "user" ? "user" : "assistant",
                Content = m.Content
            }).ToList()
        };

        var endpoint = $"https://bedrock-runtime.{_config.Region}.amazonaws.com/model/{modelId}/invoke-with-response-stream";
        var body = JsonSerializer.Serialize(claudeRequest, JsonOptions);

        var httpRequest = await CreateSignedRequestAsync(HttpMethod.Post, endpoint, body, cancellationToken);
        var httpResponse = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var fullContent = new StringBuilder();
        var inputTokens = 0;
        var outputTokens = 0;
        string? stopReason = null;

        using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                // Bedrock streaming uses event-stream format
                if (line.StartsWith(":") || !line.Contains("{"))
                    continue;

                var eventStart = line.IndexOf('{');
                if (eventStart < 0)
                    continue;

                var jsonData = line.Substring(eventStart);
                var streamEvent = JsonSerializer.Deserialize<BedrockStreamEvent>(jsonData, JsonOptions);

                if (streamEvent?.Delta?.Text != null)
                {
                    onToken(streamEvent.Delta.Text);
                    fullContent.Append(streamEvent.Delta.Text);
                    outputTokens++;
                }

                if (!string.IsNullOrEmpty(streamEvent?.Delta?.StopReason))
                {
                    stopReason = streamEvent.Delta.StopReason;
                }

                if (streamEvent?.Usage != null)
                {
                    inputTokens = streamEvent.Usage.InputTokens;
                    outputTokens = streamEvent.Usage.OutputTokens;
                }
            }
            catch (JsonException)
            {
                // Skip malformed events
            }
        }

        return new AIChatResponse
        {
            Message = new AIChatMessage
            {
                Role = "assistant",
                Content = fullContent.ToString()
            },
            Model = modelId,
            Usage = new AIUsage
            {
                PromptTokens = inputTokens,
                CompletionTokens = outputTokens,
                TotalTokens = inputTokens + outputTokens
            },
            FinishReason = stopReason ?? "stop"
        };
    }

    /// <inheritdoc />
    public async Task<AIEmbeddingResponse> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
    {
        var modelId = model ?? _config.EmbeddingModelId;
        var endpoint = $"https://bedrock-runtime.{_config.Region}.amazonaws.com/model/{modelId}/invoke";

        var request = new BedrockTitanEmbeddingRequest { InputText = text };
        var body = JsonSerializer.Serialize(request, JsonOptions);

        var httpRequest = await CreateSignedRequestAsync(HttpMethod.Post, endpoint, body, cancellationToken);
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var embeddingResponse = JsonSerializer.Deserialize<BedrockTitanEmbeddingResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize embedding response");

        return new AIEmbeddingResponse
        {
            Embedding = embeddingResponse.Embedding ?? Array.Empty<float>(),
            Model = modelId,
            TokenCount = embeddingResponse.InputTextTokenCount
        };
    }

    /// <inheritdoc />
    public async Task<AIBatchEmbeddingResponse> GetEmbeddingsAsync(IEnumerable<string> texts, string? model = null, CancellationToken cancellationToken = default)
    {
        // Titan embedding model doesn't support batch, so we process sequentially
        var embeddings = new List<float[]>();
        var totalTokens = 0;
        var modelId = model ?? _config.EmbeddingModelId;

        foreach (var text in texts)
        {
            var result = await GetEmbeddingAsync(text, modelId, cancellationToken);
            embeddings.Add(result.Embedding);
            totalTokens += result.TokenCount;
        }

        return new AIBatchEmbeddingResponse
        {
            Embeddings = embeddings,
            Model = modelId,
            TotalTokens = totalTokens
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
Extract entities from the provided text and return them as a JSON array.
Entity types to identify: person, company, email, phone, date, location, money, product, job_title.
Format: [{""type"": ""entity_type"", ""value"": ""extracted_text"", ""confidence"": 0.0-1.0}]
Only return the JSON array, no other text.";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = text }
            },
            Temperature = 0.1,
            MaxTokens = 1000
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        var entities = new List<ExtractedEntity>();
        try
        {
            // Extract JSON from response (Claude might add explanatory text)
            var jsonContent = ExtractJsonArray(response.Message.Content);
            var jsonEntities = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
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
Return only the JSON object, nothing else.";

        var chatRequest = new AIChatRequest
        {
            SystemPrompt = systemPrompt,
            Messages = new List<AIChatMessage>
            {
                new() { Role = "user", Content = text }
            },
            Temperature = 0.1,
            MaxTokens = 200
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        try
        {
            var jsonContent = ExtractJsonObject(response.Message.Content);
            var result = JsonSerializer.Deserialize<JsonElement>(jsonContent);
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
[{""type"": ""call|email|meeting|task"", ""title"": ""short action title"", ""description"": ""action details"", ""reasoning"": ""why recommended"", ""confidence"": 0.0-1.0, ""priority"": 1-5}]
Prioritize actions that will move deals forward or strengthen relationships.
Return only the JSON array.";

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
            MaxTokens = 1000
        };

        var response = await ChatAsync(chatRequest, cancellationToken);

        var actions = new List<AIRecommendedAction>();
        try
        {
            var jsonContent = ExtractJsonArray(response.Message.Content);
            var jsonActions = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
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
        // Claude tokenization is similar to GPT: roughly 4 characters per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <inheritdoc />
    public Task<AIUsageStats> GetUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // AWS Bedrock usage is tracked via CloudWatch metrics
        // Would need to integrate with AWS Cost Explorer API for real stats
        return Task.FromResult(new AIUsageStats
        {
            Provider = ProviderName,
            StartDate = startDate,
            EndDate = endDate,
            TotalRequests = 0,
            TotalInputTokens = 0,
            TotalOutputTokens = 0,
            EstimatedCost = null // Check AWS Cost Explorer for costs
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
                    Message = "AWS Bedrock not reachable",
                    CheckedAt = DateTime.UtcNow
                };
            }

            // Try a simple completion
            var testRequest = new AIChatRequest
            {
                Messages = new List<AIChatMessage>
                {
                    new() { Role = "user", Content = "Hello" }
                },
                MaxTokens = 10
            };

            await ChatAsync(testRequest, cancellationToken);

            return new ProviderHealthResult
            {
                IsHealthy = true,
                ProviderName = ProviderName,
                Message = "AWS Bedrock is operational",
                CheckedAt = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["region"] = _config.Region,
                    ["default_model"] = _config.DefaultModelId
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

    private Task<HttpRequestMessage> CreateSignedRequestAsync(HttpMethod method, string url, string? body, CancellationToken cancellationToken)
    {
        // Note: In production, use AWS SDK or implement AWS Signature Version 4
        // This is a simplified implementation
        var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        // Add AWS authentication headers
        // In production, use AWS SDK's credential chain or implement SigV4
        if (!_config.UseDefaultCredentials)
        {
            // For non-default credentials, we'd need to implement SigV4 signing
            // For now, assume AWS SDK handles this via the HttpClient configuration
            _logger.LogDebug("Using explicit credentials for Bedrock request");
        }

        return Task.FromResult(request);
    }

    private string BuildLlamaPrompt(AIChatRequest request)
    {
        var prompt = new StringBuilder();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            prompt.AppendLine($"<|begin_of_text|><|start_header_id|>system<|end_header_id|>");
            prompt.AppendLine();
            prompt.AppendLine(request.SystemPrompt);
            prompt.AppendLine("<|eot_id|>");
        }

        foreach (var message in request.Messages)
        {
            var role = message.Role == "assistant" ? "assistant" : "user";
            prompt.AppendLine($"<|start_header_id|>{role}<|end_header_id|>");
            prompt.AppendLine();
            prompt.AppendLine(message.Content);
            prompt.AppendLine("<|eot_id|>");
        }

        prompt.AppendLine("<|start_header_id|>assistant<|end_header_id|>");
        prompt.AppendLine();

        return prompt.ToString();
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

    private string ExtractJsonArray(string content)
    {
        var start = content.IndexOf('[');
        var end = content.LastIndexOf(']');
        if (start >= 0 && end > start)
        {
            return content.Substring(start, end - start + 1);
        }
        return content;
    }

    private string ExtractJsonObject(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return content.Substring(start, end - start + 1);
        }
        return content;
    }

    #endregion
}
