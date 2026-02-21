// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Runtime.CompilerServices;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CRM.Infrastructure.AI.SK.Connectors;

/// <summary>
/// Bridges the CRM <see cref="IAIPort"/> to Semantic Kernel's <see cref="IChatCompletionService"/>.
/// This allows SK agents and plugins to use whichever AI provider is configured
/// via the CRM's pluggable provider architecture (Ollama, OpenAI, Azure, Anthropic, etc.).
/// </summary>
public class CrmChatCompletionConnector : IChatCompletionService
{
    #region Fields

    private readonly IAIPort _aiPort;
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
    /// <param name="aiPort">The CRM AI port providing the underlying LLM implementation.</param>
    /// <param name="logger">Logger instance.</param>
    public CrmChatCompletionConnector(IAIPort aiPort, ILogger<CrmChatCompletionConnector> logger)
    {
        _aiPort = aiPort ?? throw new ArgumentNullException(nameof(aiPort));
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
        _logger.LogDebug("Converting SK ChatHistory ({MessageCount} messages) to IAIPort request", chatHistory.Count);

        var messages = chatHistory.Select(m => new AIChatMessage
        {
            Role = m.Role.ToString().ToLowerInvariant(),
            Content = m.Content ?? string.Empty
        }).ToList();

        // Extract temperature and max tokens from execution settings with safe defaults
        var temperature = 0.3;
        var maxTokens = 4096;
        string? modelOverride = null;

        if (executionSettings is OpenAIPromptExecutionSettings openAiSettings)
        {
            temperature = openAiSettings.Temperature ?? 0.3;
            maxTokens = openAiSettings.MaxTokens ?? 4096;
            modelOverride = openAiSettings.ModelId;
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

        var request = new AIChatRequest
        {
            Messages = messages,
            Temperature = temperature,
            MaxTokens = maxTokens,
            Model = modelOverride
        };

        try
        {
            var response = await _aiPort.ChatAsync(request, cancellationToken);

            _logger.LogDebug(
                "IAIPort returned response ({TokensUsed} tokens, model: {Model})",
                response.Usage?.TotalTokens ?? 0,
                response.Model ?? "unknown");

            var result = new ChatMessageContent(AuthorRole.Assistant, response.Message?.Content ?? string.Empty);

            // Store token usage in metadata if available
            if (response.Usage?.TotalTokens > 0)
            {
                result.Metadata = new Dictionary<string, object?>
                {
                    ["TokensUsed"] = response.Usage.TotalTokens,
                    ["ModelUsed"] = response.Model
                };
            }

            return new List<ChatMessageContent> { result };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IAIPort ChatAsync failed for {MessageCount} messages", messages.Count);
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
        // Fallback to non-streaming since IAIPort may not support streaming natively.
        // The full response is returned as a single streaming chunk.
        _logger.LogDebug("Streaming requested — falling back to non-streaming via IAIPort");

        var results = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);

        foreach (var result in results)
        {
            yield return new StreamingChatMessageContent(result.Role, result.Content);
        }
    }

    #endregion
}
