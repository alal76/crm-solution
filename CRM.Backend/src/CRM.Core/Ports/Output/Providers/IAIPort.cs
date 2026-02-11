// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Ports.Output.Providers;

#region AI Port Interface

/// <summary>
/// Output port for AI/LLM operations.
/// Consolidates existing multi-provider LLM support into pluggable architecture.
/// Implementations: Ollama (local), OpenAI, Azure OpenAI, Anthropic, AWS Bedrock, Google Gemini.
/// </summary>
public interface IAIPort
{
    /// <summary>
    /// Gets the unique identifier for this AI provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the AI provider is properly configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the models available from this provider.
    /// </summary>
    Task<IEnumerable<AIModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

    #region Text Generation

    /// <summary>
    /// Generates a text completion.
    /// </summary>
    /// <param name="request">Completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated completion.</returns>
    Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion (conversation-style).
    /// </summary>
    /// <param name="request">Chat request with message history.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chat response.</returns>
    Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a chat completion for real-time responses.
    /// </summary>
    /// <param name="request">Chat request.</param>
    /// <param name="onToken">Callback for each token received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete response after streaming finishes.</returns>
    Task<AIChatResponse> StreamChatAsync(AIChatRequest request, Action<string> onToken, CancellationToken cancellationToken = default);

    #endregion

    #region Embeddings

    /// <summary>
    /// Generates embeddings for text.
    /// </summary>
    /// <param name="text">Text to embed.</param>
    /// <param name="model">Optional embedding model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embedding vector.</returns>
    Task<AIEmbeddingResponse> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts.
    /// </summary>
    /// <param name="texts">Texts to embed.</param>
    /// <param name="model">Optional embedding model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of embedding vectors.</returns>
    Task<AIBatchEmbeddingResponse> GetEmbeddingsAsync(IEnumerable<string> texts, string? model = null, CancellationToken cancellationToken = default);

    #endregion

    #region CRM-Specific Operations

    /// <summary>
    /// Generates an email draft based on context.
    /// </summary>
    /// <param name="request">Email generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated email content.</returns>
    Task<AIEmailDraft> GenerateEmailDraftAsync(EmailDraftRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests a reply to an email.
    /// </summary>
    /// <param name="originalEmail">The email to reply to.</param>
    /// <param name="context">Optional context about the relationship.</param>
    /// <param name="tone">Desired tone (formal, friendly, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suggested reply.</returns>
    Task<AIEmailDraft> SuggestReplyAsync(string originalEmail, string? context = null, string? tone = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Summarizes content (email thread, notes, etc.).
    /// </summary>
    /// <param name="content">Content to summarize.</param>
    /// <param name="maxLength">Maximum summary length.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary text.</returns>
    Task<string> SummarizeAsync(string content, int? maxLength = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts entities from text (names, companies, dates, etc.).
    /// </summary>
    /// <param name="text">Text to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted entities.</returns>
    Task<AIEntityExtractionResult> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes sentiment of text.
    /// </summary>
    /// <param name="text">Text to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sentiment analysis result.</returns>
    Task<AISentimentResult> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets next best action recommendations.
    /// </summary>
    /// <param name="context">Context about the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Action recommendations.</returns>
    Task<AIActionRecommendations> GetNextBestActionsAsync(AIActionContext context, CancellationToken cancellationToken = default);

    #endregion

    #region Token/Usage Tracking

    /// <summary>
    /// Estimates tokens for a text.
    /// </summary>
    /// <param name="text">Text to count.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokens(string text);

    /// <summary>
    /// Gets usage statistics.
    /// </summary>
    /// <param name="startDate">Start of period.</param>
    /// <param name="endDate">End of period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Usage statistics.</returns>
    Task<AIUsageStats> GetUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Gets the health status of the AI provider.
    /// </summary>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region AI DTOs

/// <summary>
/// AI model information.
/// </summary>
public class AIModelInfo
{
    /// <summary>
    /// Model identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Model provider.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Model capabilities.
    /// </summary>
    public List<string> Capabilities { get; set; } = new(); // chat, completion, embedding, vision

    /// <summary>
    /// Maximum context length.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Whether model is available.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Cost per 1K input tokens (if applicable).
    /// </summary>
    public decimal? InputCostPer1K { get; set; }

    /// <summary>
    /// Cost per 1K output tokens.
    /// </summary>
    public decimal? OutputCostPer1K { get; set; }
}

/// <summary>
/// Text completion request.
/// </summary>
public class AICompletionRequest
{
    /// <summary>
    /// The prompt to complete.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Model to use.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Temperature (0-2, lower = more deterministic).
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Top-p sampling.
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Stop sequences.
    /// </summary>
    public List<string>? StopSequences { get; set; }

    /// <summary>
    /// Whether to stream the response.
    /// </summary>
    public bool Stream { get; set; }
}

/// <summary>
/// Text completion response.
/// </summary>
public class AICompletionResponse
{
    public string Text { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public AIUsage Usage { get; set; } = new();
    public string? FinishReason { get; set; }
}

/// <summary>
/// Chat request with message history.
/// </summary>
public class AIChatRequest
{
    /// <summary>
    /// Conversation messages.
    /// </summary>
    public List<AIChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// System prompt/instructions.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Model to use.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Maximum tokens.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Temperature.
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Top-p sampling.
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Tool/function definitions for function calling.
    /// </summary>
    public List<AITool>? Tools { get; set; }

    /// <summary>
    /// Whether to stream.
    /// </summary>
    public bool Stream { get; set; }

    /// <summary>
    /// JSON mode (force JSON output).
    /// </summary>
    public bool JsonMode { get; set; }
}

/// <summary>
/// Chat message.
/// </summary>
public class AIChatMessage
{
    /// <summary>
    /// Role: system, user, assistant, tool.
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// Message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Name (for multi-user scenarios).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Tool call results (for function calling).
    /// </summary>
    public List<AIToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Tool/function definition.
/// </summary>
public class AITool
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Tool call from the model.
/// </summary>
public class AIToolCall
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
}

/// <summary>
/// Chat response.
/// </summary>
public class AIChatResponse
{
    public AIChatMessage Message { get; set; } = new();
    public string Model { get; set; } = string.Empty;
    public AIUsage Usage { get; set; } = new();
    public string? FinishReason { get; set; }
    public List<AIToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Token usage information.
/// </summary>
public class AIUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

/// <summary>
/// Embedding response.
/// </summary>
public class AIEmbeddingResponse
{
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string Model { get; set; } = string.Empty;
    public int TokenCount { get; set; }
}

/// <summary>
/// Batch embedding response.
/// </summary>
public class AIBatchEmbeddingResponse
{
    public List<float[]> Embeddings { get; set; } = new();
    public string Model { get; set; } = string.Empty;
    public int TotalTokens { get; set; }
}

/// <summary>
/// Email draft generation request.
/// </summary>
public class EmailDraftRequest
{
    /// <summary>
    /// Purpose of the email.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Recipient name.
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Key points to include.
    /// </summary>
    public List<string>? KeyPoints { get; set; }

    /// <summary>
    /// Desired tone.
    /// </summary>
    public string Tone { get; set; } = "professional";

    /// <summary>
    /// Additional context.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Previous email in thread (for replies).
    /// </summary>
    public string? PreviousEmail { get; set; }

    /// <summary>
    /// Desired length.
    /// </summary>
    public string? Length { get; set; } // short, medium, long
}

/// <summary>
/// Generated email draft.
/// </summary>
public class AIEmailDraft
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public AIUsage Usage { get; set; } = new();
}

/// <summary>
/// Entity extraction result.
/// </summary>
public class AIEntityExtractionResult
{
    public List<ExtractedEntity> Entities { get; set; } = new();
    public AIUsage Usage { get; set; } = new();
}

/// <summary>
/// Extracted entity.
/// </summary>
public class ExtractedEntity
{
    public string Type { get; set; } = string.Empty; // person, company, date, email, phone, location, etc.
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int? StartIndex { get; set; }
    public int? EndIndex { get; set; }
}

/// <summary>
/// Sentiment analysis result.
/// </summary>
public class AISentimentResult
{
    /// <summary>
    /// Overall sentiment: positive, negative, neutral, mixed.
    /// </summary>
    public string Sentiment { get; set; } = "neutral";

    /// <summary>
    /// Sentiment score (-1 to 1).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Confidence level.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Detected emotions.
    /// </summary>
    public Dictionary<string, double>? Emotions { get; set; }

    public AIUsage Usage { get; set; } = new();
}

/// <summary>
/// Context for action recommendations.
/// </summary>
public class AIActionContext
{
    public string EntityType { get; set; } = string.Empty; // Account, Contact, Opportunity
    public int EntityId { get; set; }
    public Dictionary<string, object>? EntityData { get; set; }
    public List<string>? RecentActivities { get; set; }
    public string? Goal { get; set; } // close deal, nurture, resolve issue
}

/// <summary>
/// Action recommendations result.
/// </summary>
public class AIActionRecommendations
{
    public List<AIRecommendedAction> Actions { get; set; } = new();
    public AIUsage Usage { get; set; } = new();
}

/// <summary>
/// A recommended action.
/// </summary>
public class AIRecommendedAction
{
    public string Type { get; set; } = string.Empty; // call, email, meeting, task
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Reasoning { get; set; }
    public double Confidence { get; set; }
    public int Priority { get; set; }
    public DateTime? SuggestedDate { get; set; }
}

/// <summary>
/// AI usage statistics.
/// </summary>
public class AIUsageStats
{
    public string Provider { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRequests { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
    public Dictionary<string, int>? RequestsByModel { get; set; }
}

#endregion
