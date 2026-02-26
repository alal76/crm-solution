// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Runtime.CompilerServices;
using CRM.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CRM.Infrastructure.AI.SK.Connectors;

/// <summary>
/// Bridges the CRM <see cref="ILLMService"/> to Semantic Kernel's <see cref="IChatCompletionService"/>.
/// This allows SK agents and plugins to use whichever AI provider is configured
/// via the CRM's LLM settings (Groq, OpenAI, Azure, Anthropic, Ollama, etc.).
/// The active provider is determined by <see cref="LLMProviderOptions.DefaultProvider"/>.
/// </summary>
public class CrmChatCompletionConnector : IChatCompletionService
{
    #region Fields

    private readonly ILLMService _llmService;
    private readonly LLMProviderOptions _llmOptions;
    private readonly ILogger<CrmChatCompletionConnector> _logger;

    #endregion

    #region Properties

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CrmChatCompletionConnector"/> class.
    /// </summary>
    /// <param name="llmService">The CRM LLM service providing multi-provider AI access.</param>
    /// <param name="llmOptions">LLM provider options containing the default provider and models.</param>
    /// <param name="logger">Logger instance.</param>
    public CrmChatCompletionConnector(
        ILLMService llmService,
        IOptions<LLMProviderOptions> llmOptions,
        ILogger<CrmChatCompletionConnector> logger)
    {
        _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
        _llmOptions = llmOptions?.Value ?? throw new ArgumentNullException(nameof(llmOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IChatCompletionService Implementation

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var provider = _llmOptions.DefaultProvider;
        _logger.LogDebug(
            "Converting SK ChatHistory ({MessageCount} messages) to ILLMService request (provider: {Provider})",
            chatHistory.Count,
            provider);

        // Convert SK ChatHistory to LLMRequest messages
        var messages = chatHistory.Select(m => new LLMMessage
        {
            Role = m.Role.ToString().ToLowerInvariant(),
            Content = m.Content ?? string.Empty
        }).ToList();

        // Extract execution settings with safe defaults
        var temperature = _llmOptions.DefaultTemperature;
        var maxTokens = _llmOptions.DefaultMaxTokens;

        if (executionSettings is OpenAIPromptExecutionSettings openAiSettings)
        {
            temperature = openAiSettings.Temperature ?? temperature;
            maxTokens = openAiSettings.MaxTokens ?? maxTokens;
        }
        else if (executionSettings?.ExtensionData != null)
        {
            if (executionSettings.ExtensionData.TryGetValue("temperature", out var tempObj) && tempObj is double tempVal)
            {
                temperature = tempVal;
            }

            if (executionSettings.ExtensionData.TryGetValue("max_tokens", out var maxObj) && maxObj is int maxVal)
            {
                maxTokens = maxVal;
            }
        }

        // Leave Model empty so each provider picks its own configured default model.
        // Passing a model name here causes cross-provider mismatches during fallback
        // (e.g. a Groq model name being sent to Ollama).
        var request = new LLMRequest
        {
            Provider = provider,
            Model = string.Empty,
            Messages = messages,
            Temperature = temperature,
            MaxTokens = maxTokens
        };

        try
        {
            var response = await _llmService.ChatAsync(request, cancellationToken);

            if (!response.Success)
            {
                _logger.LogError(
                    "ILLMService chat failed (provider: {Provider}): {Error}",
                    response.Provider,
                    response.Error);
                throw new InvalidOperationException($"LLM call failed ({response.Provider}): {response.Error}");
            }

            _logger.LogDebug(
                "ILLMService returned response ({TokensUsed} tokens, provider: {Provider}, model: {Model})",
                response.TotalTokens,
                response.Provider,
                response.Model);

            var result = new ChatMessageContent(AuthorRole.Assistant, response.Content);

            if (response.TotalTokens > 0)
            {
                result.Metadata = new Dictionary<string, object?>
                {
                    ["TokensUsed"] = response.TotalTokens,
                    ["ModelUsed"] = response.Model,
                    ["Provider"] = response.Provider
                };
            }

            return new List<ChatMessageContent> { result };
        }
        catch (InvalidOperationException)
        {
            // Already logged above, re-throw as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ILLMService ChatAsync failed (provider: {Provider}) for {MessageCount} messages", provider, messages.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Fallback to non-streaming since ILLMService does not natively expose streaming to SK.
        // The full response is yielded as a single streaming chunk.
        _logger.LogDebug("Streaming requested — falling back to non-streaming via ILLMService");

        var results = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);

        foreach (var result in results)
        {
            yield return new StreamingChatMessageContent(result.Role, result.Content);
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Returns the default model name for the specified provider from LLM options.
    /// </summary>
    private string GetDefaultModelForProvider(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "openai" => _llmOptions.OpenAI?.DefaultModel ?? "gpt-4o-mini",
            "azure" or "azureopenai" => _llmOptions.AzureOpenAI?.DefaultModel ?? "gpt-4o",
            "anthropic" => _llmOptions.Anthropic?.DefaultModel ?? "claude-3-5-sonnet-20241022",
            "google" or "gemini" or "vertexai" => _llmOptions.GoogleCloud?.DefaultModel ?? "gemini-1.5-pro",
            "aws" or "bedrock" => _llmOptions.AWSBedrock?.DefaultModel ?? "anthropic.claude-3-sonnet-20240229-v1:0",
            "deepseek" => _llmOptions.DeepSeek?.DefaultModel ?? "deepseek-chat",
            "groq" => _llmOptions.Groq?.DefaultModel ?? "llama-3.3-70b-versatile",
            "allenai" or "ai2" => _llmOptions.AllenAI?.DefaultModel ?? "allenai/OLMo-7B-Instruct",
            "local" or "ollama" or "lmstudio" or "vllm" => _llmOptions.LocalLLM?.DefaultModel ?? "llama3",
            _ => _llmOptions.OpenAI?.DefaultModel ?? "gpt-4o-mini"
        };
    }

    #endregion
}
