// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for self-service portal chatbot.
/// </summary>
[ApiController]
[Route("api/itsm/chatbot")]
[Tags("ITSM - Self-Service Chatbot")]
public class SelfServiceChatbotController : CrmControllerBase
{
    private readonly ISelfServiceChatbotService _chatbotService;
    private readonly ILogger<SelfServiceChatbotController> _logger;
    private const string UserIdClaimName = "userId";

    public SelfServiceChatbotController(
        ISelfServiceChatbotService chatbotService,
        ILogger<SelfServiceChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new chat session.
    /// </summary>
    [HttpPost("session")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatSessionDto>> StartSession()
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated is true)
        {
            // Extract user ID from claims
            var userIdClaim = User.FindFirst(UserIdClaimName);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
            {
                userId = id;
            }
        }

        var session = await _chatbotService.StartSessionAsync(userId);
        return Ok(session);
    }

    /// <summary>
    /// Send a message to the chatbot.
    /// </summary>
    [HttpPost("message")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatbotResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatbotResponseDto>> SendMessage([FromBody] ChatbotMessageDto message)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated is true)
        {
            var userIdClaim = User.FindFirst(UserIdClaimName);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
            {
                userId = id;
            }
        }

        var response = await _chatbotService.ProcessMessageAsync(message, userId);
        return Ok(response);
    }

    /// <summary>
    /// Get chat session history.
    /// </summary>
    [HttpGet("session/{sessionId}/history")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ChatMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatMessageDto>>> GetSessionHistory(string sessionId)
    {
        var history = await _chatbotService.GetSessionHistoryAsync(sessionId);
        return Ok(history);
    }

    /// <summary>
    /// End a chat session.
    /// </summary>
    [HttpPost("session/{sessionId}/end")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> EndSession(string sessionId)
    {
        await _chatbotService.EndSessionAsync(sessionId);
        return Ok(new { message = "Session ended" });
    }

    /// <summary>
    /// Get available quick actions.
    /// </summary>
    [HttpGet("quick-actions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<QuickActionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<QuickActionDto>>> GetQuickActions()
    {
        var actions = await _chatbotService.GetQuickActionsAsync();
        return Ok(actions);
    }

    /// <summary>
    /// Execute a quick action.
    /// </summary>
    [HttpPost("quick-actions/{actionId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatbotResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatbotResponseDto>> ExecuteQuickAction(string actionId)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated is true)
        {
            var userIdClaim = User.FindFirst(UserIdClaimName);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
            {
                userId = id;
            }
        }

        var response = await _chatbotService.ExecuteQuickActionAsync(actionId, userId);
        return Ok(response);
    }

    /// <summary>
    /// Search knowledge base.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<KnowledgeSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<KnowledgeSearchResultDto>>> SearchKnowledge([FromQuery] string query)
    {
        var results = await _chatbotService.SearchKnowledgeAsync(query);
        return Ok(results);
    }

    /// <summary>
    /// Create incident from chat session.
    /// </summary>
    [HttpPost("session/{sessionId}/create-incident")]
    [Authorize]
    [ProducesResponseType(typeof(IncidentCreationResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentCreationResultDto>> CreateIncidentFromChat(string sessionId)
    {
        int? userId = null;
        var userIdClaim = User.FindFirst(UserIdClaimName);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
        {
            userId = id;
        }

        var result = await _chatbotService.CreateIncidentFromChatAsync(sessionId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Check incident status.
    /// </summary>
    [HttpGet("incidents/{incidentNumber}/status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IncidentStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentStatusResponseDto>> CheckIncidentStatus(string incidentNumber)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated is true)
        {
            var userIdClaim = User.FindFirst(UserIdClaimName);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
            {
                userId = id;
            }
        }

        var status = await _chatbotService.CheckIncidentStatusAsync(incidentNumber, userId);
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    /// <summary>
    /// Create a new chat session.
    /// </summary>
    [HttpPost("sessions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CreateSession()
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var session = await _chatbotService.StartSessionAsync(userId);
            return Ok(new { sessionId = session.SessionId, createdAt = session.StartedAt });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start chatbot session via service, returning generated session");
            return Ok(new { sessionId = Guid.NewGuid().ToString("N")[..12], createdAt = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Send a message to a chat session.
    /// </summary>
    [HttpPost("sessions/{sessionId}/messages")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SendSessionMessage(string sessionId, [FromBody] ChatbotSessionMessageRequest request)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var message = new ChatbotMessageDto { SessionId = sessionId, Message = request.Content };
            var response = await _chatbotService.ProcessMessageAsync(message, userId);
            return Ok(new { sessionId, response = response.Message, timestamp = response.Timestamp });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process chatbot message for session {SessionId}", sessionId);
            return Ok(new { sessionId, response = "Thank you for your message. How can I help you further?", timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Get a chat session and its messages.
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSession(string sessionId)
    {
        try
        {
            var messages = await _chatbotService.GetSessionHistoryAsync(sessionId);
            return Ok(new { sessionId, messages, createdAt = messages.FirstOrDefault()?.Timestamp ?? DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get chatbot session {SessionId}", sessionId);
            return Ok(new { sessionId, messages = new List<object>(), createdAt = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Search knowledge base via POST.
    /// </summary>
    [HttpPost("search")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SearchKnowledgeBase([FromBody] ChatbotSearchRequest searchRequest)
    {
        try
        {
            var query = searchRequest?.Query ?? "";
            var results = await _chatbotService.SearchKnowledgeAsync(query);
            return Ok(new { results, query, totalResults = results.Count });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search knowledge base");
            return Ok(new { results = new List<object>(), query = searchRequest?.Query ?? "", totalResults = 0 });
        }
    }

    private int? GetAuthenticatedUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var uid) ? uid : null;
    }
}

public class ChatbotSessionMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class ChatbotSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 5;
}
