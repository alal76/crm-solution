// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for self-service portal chatbot functionality.
/// Provides AI-powered assistance for users to resolve issues and access knowledge.
/// </summary>
public interface ISelfServiceChatbotService
{
    /// <summary>
    /// Process a user message and generate a response.
    /// </summary>
    Task<ChatbotResponseDto> ProcessMessageAsync(ChatbotMessageDto message, int? userId);

    /// <summary>
    /// Get chat session history.
    /// </summary>
    Task<List<ChatMessageDto>> GetSessionHistoryAsync(string sessionId);

    /// <summary>
    /// Start a new chat session.
    /// </summary>
    Task<ChatSessionDto> StartSessionAsync(int? userId);

    /// <summary>
    /// End a chat session.
    /// </summary>
    Task EndSessionAsync(string sessionId);

    /// <summary>
    /// Get suggested quick actions.
    /// </summary>
    Task<List<QuickActionDto>> GetQuickActionsAsync();

    /// <summary>
    /// Execute a quick action.
    /// </summary>
    Task<ChatbotResponseDto> ExecuteQuickActionAsync(string actionId, int? userId);

    /// <summary>
    /// Search knowledge base from chatbot context.
    /// </summary>
    Task<List<KnowledgeSearchResultDto>> SearchKnowledgeAsync(string query);

    /// <summary>
    /// Create incident from chat session.
    /// </summary>
    Task<IncidentCreationResultDto> CreateIncidentFromChatAsync(string sessionId, int? userId);

    /// <summary>
    /// Check incident status.
    /// </summary>
    Task<IncidentStatusResponseDto?> CheckIncidentStatusAsync(string incidentNumber, int? userId);
}

// ====== DTOs ======
public class ChatbotMessageDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ChatMessageType Type { get; set; } = ChatMessageType.Text;
    public Dictionary<string, object>? Context { get; set; }
}

public enum ChatMessageType
{
    Text,
    QuickAction,
    Selection,
    Feedback
}

public class ChatbotResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ResponseType Type { get; set; } = ResponseType.Text;
    public List<ChatSuggestion>? Suggestions { get; set; }
    public List<KnowledgeSearchResultDto>? KnowledgeResults { get; set; }
    public ChatAction? Action { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum ResponseType
{
    Text,
    Options,
    KnowledgeResults,
    IncidentCreated,
    IncidentStatus,
    Escalation,
    Error
}

public class ChatSuggestion
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class ChatAction
{
    public string ActionType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = "active";
    public int MessageCount { get; set; }
}

public class QuickActionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class KnowledgeSearchResultDto
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Views { get; set; }
}

public class IncidentCreationResultDto
{
    public bool Success { get; set; }
    public int? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class IncidentStatusResponseDto
{
    public string IncidentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? LastUpdate { get; set; }
}
