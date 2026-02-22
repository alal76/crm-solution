// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.Security.Claims;
using System.Text.Json;
using CRM.Core.Entities.AI;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Agents;
using CRM.Infrastructure.AI.SK.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for AI agent interactions including chat, deal intelligence,
/// email drafting, ticket resolution, and multi-agent orchestration.
/// </summary>
[ApiController]
[Route("api/agents")]
[Authorize]
public class AgentController : ControllerBase
{
    #region Fields

    private readonly AgentExecutionService _executionService;
    private readonly AgentOrchestrator _orchestrator;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AgentController> _logger;
    private readonly IFeatureManager _featureManager;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentController"/> class.
    /// </summary>
    /// <param name="executionService">The agent execution service.</param>
    /// <param name="orchestrator">The agent orchestrator.</param>
    /// <param name="dbContext">The CRM database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AgentController(
        AgentExecutionService executionService,
        AgentOrchestrator orchestrator,
        ICrmDbContext dbContext,
        ILogger<AgentController> logger,
        IFeatureManager featureManager)
    {
        _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
    }

    #endregion

    #region DTOs

    /// <summary>
    /// Request DTO for chat interactions with an agent.
    /// </summary>
    /// <param name="Message">The user's message to the agent.</param>
    /// <param name="ConversationId">Optional existing conversation ID to continue.</param>
    /// <param name="EntityType">Optional entity type for context (e.g., "Account", "Lead").</param>
    /// <param name="EntityId">Optional entity ID for context.</param>
    public record ChatRequest(string Message, int? ConversationId = null, string? EntityType = null, int? EntityId = null);

    /// <summary>
    /// Response DTO for chat interactions.
    /// </summary>
    /// <param name="Response">The agent's response message.</param>
    /// <param name="ConversationId">The conversation ID (new or existing).</param>
    /// <param name="History">The full conversation history.</param>
    public record ChatResponse(string Response, int ConversationId, IReadOnlyList<AgentExecutionService.ChatMessageRecord> History);

    /// <summary>
    /// Request DTO for rating a conversation.
    /// </summary>
    /// <param name="Rating">Rating value (1-5).</param>
    /// <param name="Feedback">Optional textual feedback.</param>
    public record RateRequest(int Rating, string? Feedback = null);

    /// <summary>
    /// Request DTO for drafting an email via AI.
    /// </summary>
    /// <param name="Context">The context or topic for the email.</param>
    /// <param name="RecipientEmail">Optional recipient email address.</param>
    /// <param name="Tone">Optional tone (e.g., "formal", "casual", "friendly").</param>
    /// <param name="TemplateId">Optional email template ID to base the draft on.</param>
    public record DraftEmailRequest(string Context, string? RecipientEmail = null, string? Tone = null, int? TemplateId = null);

    /// <summary>
    /// Request DTO for multi-agent orchestration.
    /// </summary>
    /// <param name="Message">The user's message to orchestrate across agents.</param>
    /// <param name="AgentTypes">List of agent types to involve.</param>
    /// <param name="EntityType">Optional entity type for context.</param>
    /// <param name="EntityId">Optional entity ID for context.</param>
    public record OrchestrateRequest(string Message, List<string> AgentTypes, string? EntityType = null, int? EntityId = null);

    /// <summary>
    /// Request DTO for rejecting an approval.
    /// </summary>
    /// <param name="Reason">The reason for rejection.</param>
    public record RejectRequest(string Reason);

    /// <summary>
    /// Request DTO for creating a new AI agent.
    /// </summary>
    /// <param name="Name">Unique internal name (e.g., "lead-scorer").</param>
    /// <param name="DisplayName">Human-friendly display name.</param>
    /// <param name="Description">Optional description of the agent's purpose.</param>
    /// <param name="AgentType">The agent type enum value.</param>
    /// <param name="SystemPrompt">The system prompt defining agent behavior.</param>
    /// <param name="AllowedPlugins">Comma-separated list of allowed plugin names.</param>
    /// <param name="RequiresApproval">Whether actions require human approval.</param>
    /// <param name="ApprovalTier">Approval tier: "low", "medium", or "high".</param>
    /// <param name="Temperature">LLM temperature (0.0–1.0).</param>
    /// <param name="MaxTokens">Maximum tokens per response.</param>
    /// <param name="ModelOverride">Optional model override (e.g., "gpt-4o").</param>
    public record CreateAgentRequest(
        string Name,
        string DisplayName,
        string? Description = null,
        int AgentType = 13,
        string? SystemPrompt = null,
        string? AllowedPlugins = null,
        bool RequiresApproval = false,
        string? ApprovalTier = null,
        double Temperature = 0.3,
        int MaxTokens = 4096,
        string? ModelOverride = null);

    #endregion

    #region Agent Listing

    /// <summary>
    /// Lists all available AI agents.
    /// </summary>
    /// <returns>A list of available agents.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAgents()
    {
        try
        {
            // Check if agent subsystem is enabled
            if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableAgentSubsystem))
            {
                _logger.LogInformation("Agent subsystem is disabled via feature flag");
                return Ok(Array.Empty<object>());
            }

            var agents = await _orchestrator.GetAvailableAgentsAsync(HttpContext.RequestAborted);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing agents");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while listing agents.");
        }
    }

    /// <summary>
    /// Gets a specific AI agent by ID.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>The agent details.</returns>
    [HttpGet("{agentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgent(int agentId)
    {
        try
        {
            if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableAgentSubsystem))
            {
                return NotFound("AI Agent subsystem is currently disabled.");
            }

            var agent = await _dbContext.AIAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == agentId && !a.IsDeleted, HttpContext.RequestAborted);

            if (agent is null)
            {
                return NotFound($"Agent with ID {agentId} not found.");
            }

            return Ok(agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent {AgentId}", agentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the agent.");
        }
    }

    /// <summary>
    /// Creates a new AI agent.
    /// </summary>
    /// <param name="request">The agent creation request.</param>
    /// <returns>The created agent.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Agent name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest("Agent display name is required.");
            }

            // Check for duplicate name
            var existing = await _dbContext.AIAgents
                .AnyAsync(a => a.Name == request.Name && !a.IsDeleted, HttpContext.RequestAborted);

            if (existing)
            {
                return Conflict($"An agent with name '{request.Name}' already exists.");
            }

            var agent = new CRM.Core.Entities.AI.AIAgent
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                AgentType = (CRM.Core.Entities.AI.AgentType)request.AgentType,
                SystemPrompt = request.SystemPrompt ?? string.Empty,
                AllowedPlugins = request.AllowedPlugins ?? string.Empty,
                RequiresApproval = request.RequiresApproval,
                ApprovalTier = request.ApprovalTier,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                ModelOverride = request.ModelOverride,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.AIAgents.Add(agent);
            await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

            _logger.LogInformation("Created new agent '{AgentName}' (ID: {AgentId}) by user {UserId}",
                agent.Name, agent.Id, GetCurrentUserId());

            return CreatedAtAction(nameof(GetAgent), new { agentId = agent.Id }, agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the agent.");
        }
    }

    #endregion

    #region Chat

    /// <summary>
    /// Sends a chat message to an AI agent and returns the response.
    /// </summary>
    /// <param name="agentId">The agent ID to chat with.</param>
    /// <param name="request">The chat request containing the message and optional context.</param>
    /// <returns>The agent's response with conversation details.</returns>
    [HttpPost("{agentId:int}/chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Chat(int agentId, [FromBody] ChatRequest request)
    {
        try
        {
            if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableAgentSubsystem))
            {
                return BadRequest(new { message = "AI Agent subsystem is currently disabled. Please enable it in Feature Management settings." });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return BadRequest("Unable to identify the current user.");
            }

            var conversation = await _executionService.ChatAsync(
                agentId,
                userId,
                request.Message,
                request.ConversationId,
                request.EntityType,
                request.EntityId,
                HttpContext.RequestAborted);

            var history = DeserializeMessages(conversation.Messages);
            var response = history.LastOrDefault(m => m.Role == "assistant")?.Content ?? string.Empty;

            return Ok(new ChatResponse(response, conversation.Id, history));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Agent {AgentId} not found for chat", agentId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error chatting with agent {AgentId}", agentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the chat.");
        }
    }

    #endregion

    #region Conversations

    /// <summary>
    /// Gets the conversation history for a specific agent and the current user.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="limit">Maximum number of conversations to return (default: 20).</param>
    /// <returns>A list of conversations.</returns>
    [HttpGet("{agentId:int}/conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations(int agentId, [FromQuery] int limit = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return BadRequest("Unable to identify the current user.");
            }

            var conversations = await _dbContext.AgentConversations
                .AsNoTracking()
                .Where(c => c.AgentId == agentId && c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(limit)
                .ToListAsync(HttpContext.RequestAborted);

            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for agent {AgentId}", agentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving conversations.");
        }
    }

    /// <summary>
    /// Gets a specific conversation by ID.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <returns>The conversation details.</returns>
    [HttpGet("conversations/{conversationId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(int conversationId)
    {
        try
        {
            var conversation = await _dbContext.AgentConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, HttpContext.RequestAborted);

            if (conversation is null)
            {
                return NotFound($"Conversation with ID {conversationId} not found.");
            }

            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation {ConversationId}", conversationId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the conversation.");
        }
    }

    /// <summary>
    /// Rates a conversation with a score and optional feedback.
    /// </summary>
    /// <param name="conversationId">The conversation ID to rate.</param>
    /// <param name="request">The rating request.</param>
    /// <returns>A confirmation of the rating.</returns>
    [HttpPost("conversations/{conversationId:int}/rate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateConversation(int conversationId, [FromBody] RateRequest request)
    {
        try
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            await _executionService.RateConversationAsync(
                conversationId, request.Rating, request.Feedback, HttpContext.RequestAborted);

            return Ok(new { Message = "Conversation rated successfully.", ConversationId = conversationId, request.Rating });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Conversation {ConversationId} not found for rating", conversationId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating conversation {ConversationId}", conversationId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while rating the conversation.");
        }
    }

    #endregion

    #region Intelligence Endpoints

    /// <summary>
    /// Gets AI-powered next best action recommendations for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Account", "Lead", "Opportunity").</param>
    /// <param name="entityId">The entity ID.</param>
    /// <returns>A list of recommended next best actions.</returns>
    [HttpGet("next-best-actions/{entityType}/{entityId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNextBestActions(string entityType, int entityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest("Entity type is required.");
            }

            var message = $"What are the next best actions for {entityType} {entityId}?";
            var agentBase = await _orchestrator.RouteToAgentAsync(message, entityType, entityId, HttpContext.RequestAborted);
            var dbAgentId = await ResolveAgentDbIdAsync(agentBase, HttpContext.RequestAborted);

            var userId = GetCurrentUserId();
            var conversation = await _executionService.ChatAsync(
                dbAgentId,
                userId,
                message,
                conversationId: null,
                entityType,
                entityId,
                HttpContext.RequestAborted);

            var response = ExtractLastAssistantResponse(conversation);
            return Ok(new { EntityType = entityType, EntityId = entityId, Recommendations = response, ConversationId = conversation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next best actions for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating recommendations.");
        }
    }

    /// <summary>
    /// Gets AI-powered deal intelligence for a specific opportunity.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID.</param>
    /// <returns>Deal intelligence analysis including risk factors and recommendations.</returns>
    [HttpGet("deal-intelligence/{opportunityId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDealIntelligence(int opportunityId)
    {
        try
        {
            var message = $"Provide comprehensive deal intelligence analysis for opportunity {opportunityId} including risk factors, win probability assessment, and strategic recommendations.";
            var agentBase = await _orchestrator.RouteToAgentAsync(message, "Opportunity", opportunityId, HttpContext.RequestAborted);
            var dbAgentId = await ResolveAgentDbIdAsync(agentBase, HttpContext.RequestAborted);

            var userId = GetCurrentUserId();
            var conversation = await _executionService.ChatAsync(
                dbAgentId,
                userId,
                message,
                conversationId: null,
                "Opportunity",
                opportunityId,
                HttpContext.RequestAborted);

            var response = ExtractLastAssistantResponse(conversation);
            return Ok(new { OpportunityId = opportunityId, Analysis = response, ConversationId = conversation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deal intelligence for opportunity {OpportunityId}", opportunityId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while analyzing the deal.");
        }
    }

    #endregion

    #region Email & Support

    /// <summary>
    /// Drafts an email using the AI Email Assistant agent.
    /// </summary>
    /// <param name="request">The email draft request with context and parameters.</param>
    /// <returns>The drafted email content.</returns>
    [HttpPost("email/draft")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DraftEmail([FromBody] DraftEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Context))
            {
                return BadRequest("Email context is required.");
            }

            var prompt = $"Draft an email with the following context: {request.Context}";
            if (!string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                prompt += $" Recipient: {request.RecipientEmail}.";
            }

            if (!string.IsNullOrWhiteSpace(request.Tone))
            {
                prompt += $" Tone: {request.Tone}.";
            }

            if (request.TemplateId.HasValue)
            {
                prompt += $" Base on template ID: {request.TemplateId.Value}.";
            }

            var agentBase = await _orchestrator.RouteToAgentAsync(prompt, "Email", null, HttpContext.RequestAborted);
            var dbAgentId = await ResolveAgentDbIdAsync(agentBase, HttpContext.RequestAborted);

            var userId = GetCurrentUserId();
            var conversation = await _executionService.ChatAsync(
                dbAgentId,
                userId,
                prompt,
                conversationId: null,
                "Email",
                null,
                HttpContext.RequestAborted);

            var response = ExtractLastAssistantResponse(conversation);
            return Ok(new { Draft = response, ConversationId = conversation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error drafting email");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while drafting the email.");
        }
    }

    /// <summary>
    /// Uses AI to analyze and suggest resolution for a support ticket.
    /// </summary>
    /// <param name="serviceRequestId">The service request (ticket) ID.</param>
    /// <returns>Resolution suggestions from the Support Triage agent.</returns>
    [HttpPost("resolve/{serviceRequestId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResolveTicket(int serviceRequestId)
    {
        try
        {
            var message = $"Analyze service request {serviceRequestId} and suggest resolution steps, relevant knowledge articles, and recommended assignment.";
            var agentBase = await _orchestrator.RouteToAgentAsync(message, "ServiceRequest", serviceRequestId, HttpContext.RequestAborted);
            var dbAgentId = await ResolveAgentDbIdAsync(agentBase, HttpContext.RequestAborted);

            var userId = GetCurrentUserId();
            var conversation = await _executionService.ChatAsync(
                dbAgentId,
                userId,
                message,
                conversationId: null,
                "ServiceRequest",
                serviceRequestId,
                HttpContext.RequestAborted);

            var response = ExtractLastAssistantResponse(conversation);
            return Ok(new { ServiceRequestId = serviceRequestId, Resolution = response, ConversationId = conversation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving ticket {ServiceRequestId}", serviceRequestId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while resolving the ticket.");
        }
    }

    #endregion

    #region Orchestration

    /// <summary>
    /// Orchestrates a message across multiple AI agents and aggregates their responses.
    /// </summary>
    /// <param name="request">The orchestration request specifying agents and message.</param>
    /// <returns>Aggregated responses from all specified agents.</returns>
    [HttpPost("orchestrate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Orchestrate([FromBody] OrchestrateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message is required.");
            }

            if (request.AgentTypes is null || request.AgentTypes.Count == 0)
            {
                return BadRequest("At least one agent type must be specified.");
            }

            var results = await _orchestrator.ExecuteMultiAgentAsync(
                request.Message, request.AgentTypes, HttpContext.RequestAborted);

            return Ok(new { Message = request.Message, AgentResponses = results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error orchestrating multi-agent request");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during orchestration.");
        }
    }

    #endregion

    #region Approvals

    /// <summary>
    /// Gets all pending agent action approval requests.
    /// </summary>
    /// <returns>A list of pending approval requests.</returns>
    [HttpGet("approvals/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovals()
    {
        try
        {
            var pending = await _dbContext.AgentApprovalRequests
                .AsNoTracking()
                .Where(r => r.Status == CRM.Core.Entities.AI.ApprovalStatus.Pending && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(HttpContext.RequestAborted);

            return Ok(pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving pending approvals.");
        }
    }

    /// <summary>
    /// Approves a pending agent action.
    /// </summary>
    /// <param name="id">The approval request ID.</param>
    /// <returns>Confirmation of the approval.</returns>
    [HttpPost("approvals/{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveAction(int id)
    {
        try
        {
            var approval = await _dbContext.AgentApprovalRequests
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, HttpContext.RequestAborted);

            if (approval is null)
            {
                return NotFound($"Approval request with ID {id} not found.");
            }

            if (approval.Status != CRM.Core.Entities.AI.ApprovalStatus.Pending)
            {
                return BadRequest($"Approval request is not in pending status. Current status: {approval.Status}.");
            }

            var userId = GetCurrentUserId();
            approval.Status = CRM.Core.Entities.AI.ApprovalStatus.Approved;
            approval.ApprovedById = userId;
            approval.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

            _logger.LogInformation("Approval request {ApprovalId} approved by user {UserId}", id, userId);
            return Ok(new { Message = "Action approved successfully.", ApprovalId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving action {ApprovalId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while approving the action.");
        }
    }

    /// <summary>
    /// Rejects a pending agent action with a reason.
    /// </summary>
    /// <param name="id">The approval request ID.</param>
    /// <param name="request">The rejection reason.</param>
    /// <returns>Confirmation of the rejection.</returns>
    [HttpPost("approvals/{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectAction(int id, [FromBody] RejectRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest("Rejection reason is required.");
            }

            var approval = await _dbContext.AgentApprovalRequests
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, HttpContext.RequestAborted);

            if (approval is null)
            {
                return NotFound($"Approval request with ID {id} not found.");
            }

            if (approval.Status != CRM.Core.Entities.AI.ApprovalStatus.Pending)
            {
                return BadRequest($"Approval request is not in pending status. Current status: {approval.Status}.");
            }

            var userId = GetCurrentUserId();
            approval.Status = CRM.Core.Entities.AI.ApprovalStatus.Rejected;
            approval.ApprovedById = userId;
            approval.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

            _logger.LogInformation("Approval request {ApprovalId} rejected by user {UserId}. Reason: {Reason}", id, userId, request.Reason);
            return Ok(new { Message = "Action rejected.", ApprovalId = id, request.Reason });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting action {ApprovalId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while rejecting the action.");
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Extracts the current user ID from JWT claims.
    /// </summary>
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Resolves the database agent ID from an in-memory <see cref="CrmAgentBase"/> by matching AgentType.
    /// </summary>
    private async Task<int> ResolveAgentDbIdAsync(CrmAgentBase agentBase, CancellationToken cancellationToken)
    {
        var dbAgent = await _dbContext.AIAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AgentType == agentBase.AgentType && a.IsActive && !a.IsDeleted, cancellationToken);

        return dbAgent?.Id ?? throw new KeyNotFoundException(
            $"No database agent found for type {agentBase.AgentType}");
    }

    /// <summary>
    /// Extracts the last assistant response from a conversation's serialized messages.
    /// </summary>
    private static string ExtractLastAssistantResponse(AgentConversation conversation)
    {
        var messages = DeserializeMessages(conversation.Messages);
        return messages.LastOrDefault(m => m.Role == "assistant")?.Content ?? string.Empty;
    }

    /// <summary>
    /// Deserializes the JSON message array from a conversation record.
    /// </summary>
    private static IReadOnlyList<AgentExecutionService.ChatMessageRecord> DeserializeMessages(string? messagesJson)
    {
        if (string.IsNullOrWhiteSpace(messagesJson))
        {
            return Array.Empty<AgentExecutionService.ChatMessageRecord>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<AgentExecutionService.ChatMessageRecord>>(messagesJson)
                ?? new List<AgentExecutionService.ChatMessageRecord>();
        }
        catch
        {
            return Array.Empty<AgentExecutionService.ChatMessageRecord>();
        }
    }

    #endregion
}
