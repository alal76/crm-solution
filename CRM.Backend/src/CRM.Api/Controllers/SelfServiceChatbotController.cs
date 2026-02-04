// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for self-service portal chatbot.
/// </summary>
[ApiController]
[Route("api/itsm/chatbot")]
[Tags("ITSM - Self-Service Chatbot")]
public class SelfServiceChatbotController : ControllerBase
{
    private readonly ISelfServiceChatbotService _chatbotService;
    private readonly ILogger<SelfServiceChatbotController> _logger;

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
    public async Task<ActionResult<ChatSessionDto>> StartSession()
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            // Extract user ID from claims
            var userIdClaim = User.FindFirst("userId");
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
    public async Task<ActionResult<ChatbotResponseDto>> SendMessage([FromBody] ChatbotMessageDto message)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("userId");
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
    public async Task<ActionResult<ChatbotResponseDto>> ExecuteQuickAction(string actionId)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("userId");
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
    public async Task<ActionResult<IncidentCreationResultDto>> CreateIncidentFromChat(string sessionId)
    {
        int? userId = null;
        var userIdClaim = User.FindFirst("userId");
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
    public async Task<ActionResult<IncidentStatusResponseDto>> CheckIncidentStatus(string incidentNumber)
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
            {
                userId = id;
            }
        }

        var status = await _chatbotService.CheckIncidentStatusAsync(incidentNumber, userId);
        if (status == null)
            return NotFound();

        return Ok(status);
    }
}
