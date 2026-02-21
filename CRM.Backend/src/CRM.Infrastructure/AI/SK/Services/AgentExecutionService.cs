// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Configuration;
using CRM.Infrastructure.AI.SK.Connectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CRM.Infrastructure.AI.SK.Services;

/// <summary>
/// Core service that manages the lifecycle of agent conversations and executions.
/// Handles message persistence, chat history reconstruction, kernel creation, and
/// recording of agent actions/metrics.
/// </summary>
public class AgentExecutionService
{
    #region Fields

    private readonly ICrmDbContext _context;
    private readonly CrmKernelFactory _kernelFactory;
    private readonly SemanticKernelOptions _options;
    private readonly ILogger<AgentExecutionService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentExecutionService"/> class.
    /// </summary>
    /// <param name="context">Database context for persisting conversations and actions.</param>
    /// <param name="kernelFactory">Factory for creating agent-scoped Semantic Kernel instances.</param>
    /// <param name="options">Semantic Kernel configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public AgentExecutionService(
        ICrmDbContext context,
        CrmKernelFactory kernelFactory,
        IOptions<SemanticKernelOptions> options,
        ILogger<AgentExecutionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _kernelFactory = kernelFactory ?? throw new ArgumentNullException(nameof(kernelFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sends a message to an agent and returns the updated conversation with the agent's response.
    /// Creates a new conversation if <paramref name="conversationId"/> is not provided.
    /// </summary>
    /// <param name="agentId">The ID of the AI agent to converse with.</param>
    /// <param name="userId">The ID of the user sending the message.</param>
    /// <param name="message">The user's message text.</param>
    /// <param name="conversationId">Optional existing conversation ID to continue.</param>
    /// <param name="entityType">Optional CRM entity type for context (e.g. "Account", "Lead").</param>
    /// <param name="entityId">Optional CRM entity ID for context loading.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="AgentConversation"/> including the agent's response.</returns>
    public async Task<AgentConversation> ChatAsync(
        int agentId,
        int userId,
        string message,
        int? conversationId = null,
        string? entityType = null,
        int? entityId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        // Load and validate agent
        var agent = await LoadAgentAsync(agentId, cancellationToken);

        // Get or create conversation
        var conversation = conversationId.HasValue
            ? await LoadConversationAsync(conversationId.Value, cancellationToken)
            : await CreateConversationAsync(agentId, userId, entityType, entityId, cancellationToken);

        // Reconstruct chat history from stored messages
        var existingMessages = DeserializeMessages(conversation.Messages);
        var chatHistory = BuildChatHistory(agent.SystemPrompt, existingMessages);

        // Add the new user message
        chatHistory.AddUserMessage(message);

        // Create a kernel scoped to this agent's allowed plugins
        var kernel = _kernelFactory.CreateKernelForAgent(agent);

        // Execute LLM call
        var responseContent = await ExecuteChatAsync(kernel, chatHistory, agent, cancellationToken);

        // Persist messages
        existingMessages.Add(new ChatMessageRecord("user", message));
        existingMessages.Add(new ChatMessageRecord("assistant", responseContent));
        conversation.Messages = JsonSerializer.Serialize(existingMessages, _jsonOptions);
        conversation.MessageCount = existingMessages.Count;
        conversation.UpdatedAt = DateTime.UtcNow;

        // Record the action for audit trail
        RecordAction(conversation.Id, agentId, message, responseContent);

        // Update agent metrics
        agent.TotalConversations++;
        agent.TotalActions++;
        agent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Agent '{AgentName}' responded to conversation {ConversationId} ({MessageCount} messages)",
            agent.Name,
            conversation.Id,
            conversation.MessageCount);

        return conversation;
    }

    /// <summary>
    /// Records a user rating and optional feedback for a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation to rate.</param>
    /// <param name="rating">Rating value (e.g. 1-5).</param>
    /// <param name="feedback">Optional free-text feedback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RateConversationAsync(
        int conversationId,
        int rating,
        string? feedback,
        CancellationToken cancellationToken = default)
    {
        var conversation = await LoadConversationAsync(conversationId, cancellationToken);

        conversation.UserRating = rating;
        conversation.UserFeedback = feedback;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Conversation {ConversationId} rated {Rating}/5",
            conversationId,
            rating);
    }

    /// <summary>
    /// Retrieves the full conversation history for a given conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list of chat messages in chronological order.</returns>
    public async Task<IReadOnlyList<ChatMessageRecord>> GetConversationHistoryAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await LoadConversationAsync(conversationId, cancellationToken);
        return DeserializeMessages(conversation.Messages).AsReadOnly();
    }

    /// <summary>
    /// Closes a conversation, marking it as completed.
    /// </summary>
    /// <param name="conversationId">The conversation to close.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task CloseConversationAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await LoadConversationAsync(conversationId, cancellationToken);

        conversation.Status = CRM.Core.Entities.AI.ConversationStatus.Completed;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Conversation {ConversationId} closed", conversationId);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads an active, non-deleted agent by ID.
    /// </summary>
    private async Task<AIAgent> LoadAgentAsync(int agentId, CancellationToken cancellationToken)
    {
        var agent = await _context.Set<AIAgent>()
            .FirstOrDefaultAsync(a => a.Id == agentId && a.IsActive && !a.IsDeleted, cancellationToken);

        return agent ?? throw new InvalidOperationException($"Agent {agentId} not found or inactive");
    }

    /// <summary>
    /// Loads a non-deleted conversation by ID.
    /// </summary>
    private async Task<AgentConversation> LoadConversationAsync(int conversationId, CancellationToken cancellationToken)
    {
        var conversation = await _context.Set<AgentConversation>()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        return conversation ?? throw new InvalidOperationException($"Conversation {conversationId} not found");
    }

    /// <summary>
    /// Creates a new conversation entity and persists it.
    /// </summary>
    private async Task<AgentConversation> CreateConversationAsync(
        int agentId,
        int userId,
        string? entityType,
        int? entityId,
        CancellationToken cancellationToken)
    {
        var conversation = new AgentConversation
        {
            AgentId = agentId,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Status = CRM.Core.Entities.AI.ConversationStatus.Active,
            Messages = "[]",
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AgentConversation>().Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Created new conversation {ConversationId} for agent {AgentId}", conversation.Id, agentId);

        return conversation;
    }

    /// <summary>
    /// Builds an SK ChatHistory from the system prompt and stored messages.
    /// </summary>
    private static ChatHistory BuildChatHistory(string systemPrompt, List<ChatMessageRecord> existingMessages)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);

        foreach (var msg in existingMessages)
        {
            switch (msg.Role)
            {
                case "user":
                    chatHistory.AddUserMessage(msg.Content);
                    break;
                case "assistant":
                    chatHistory.AddAssistantMessage(msg.Content);
                    break;
                case "system":
                    chatHistory.AddSystemMessage(msg.Content);
                    break;
            }
        }

        return chatHistory;
    }

    /// <summary>
    /// Executes the chat completion via the kernel's chat service.
    /// </summary>
    private async Task<string> ExecuteChatAsync(
        Kernel kernel,
        ChatHistory chatHistory,
        AIAgent agent,
        CancellationToken cancellationToken)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var executionSettings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = agent.Temperature,
                ["max_tokens"] = agent.MaxTokens
            }
        };

        var result = await chatService.GetChatMessageContentsAsync(
            chatHistory,
            executionSettings,
            kernel,
            cancellationToken);

        return result.FirstOrDefault()?.Content ?? string.Empty;
    }

    /// <summary>
    /// Records an agent action in the database for audit.
    /// </summary>
    private void RecordAction(int conversationId, int agentId, string input, string output)
    {
        var action = new AgentAction
        {
            ConversationId = conversationId,
            AgentId = agentId,
            ActionType = ActionType.Analyze,
            PluginName = "Chat",
            FunctionName = "SendMessage",
            InputParameters = JsonSerializer.Serialize(new { message = input }, _jsonOptions),
            OutputResult = output,
            Status = ActionStatus.Executed,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AgentAction>().Add(action);
    }

    /// <summary>
    /// Deserializes the JSON message array from a conversation record.
    /// </summary>
    private static List<ChatMessageRecord> DeserializeMessages(string? messagesJson)
    {
        if (string.IsNullOrWhiteSpace(messagesJson))
        {
            return new List<ChatMessageRecord>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ChatMessageRecord>>(messagesJson) ?? new List<ChatMessageRecord>();
        }
        catch
        {
            return new List<ChatMessageRecord>();
        }
    }

    #endregion
}

/// <summary>
/// Record type for serializing chat messages in conversation history.
/// </summary>
/// <param name="Role">The message role: "system", "user", or "assistant".</param>
/// <param name="Content">The message text content.</param>
public record ChatMessageRecord(string Role, string Content);
