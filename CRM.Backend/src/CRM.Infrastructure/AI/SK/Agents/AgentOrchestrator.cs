// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using CRM.Core.Entities.AI;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// Orchestrates AI agent selection and execution. Routes user messages to the most
/// appropriate agent based on intent and entity type detection, and supports
/// parallel multi-agent execution for complex queries.
/// </summary>
public sealed class AgentOrchestrator
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AgentOrchestrator> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentOrchestrator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving agent instances.</param>
    /// <param name="dbContext">The CRM database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is <c>null</c>.
    /// </exception>
    public AgentOrchestrator(
        IServiceProvider serviceProvider,
        ICrmDbContext dbContext,
        ILogger<AgentOrchestrator> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Agent Routing

    /// <summary>
    /// Routes a user message to the most appropriate AI agent. Uses intent and entity type
    /// detection to select the best match. Falls back to <see cref="GeneralAssistantAgent"/>
    /// if no specialized agent can handle the request.
    /// </summary>
    /// <param name="userMessage">The user's message text.</param>
    /// <param name="entityType">The optional CRM entity type for context.</param>
    /// <param name="entityId">The optional CRM entity identifier for context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The selected <see cref="CrmAgentBase"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no agents are registered and no fallback is available.
    /// </exception>
    public Task<CrmAgentBase> RouteToAgentAsync(
        string userMessage,
        string? entityType = null,
        int? entityId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Routing message to agent. EntityType={EntityType}, EntityId={EntityId}.",
            entityType, entityId);

        var agents = _serviceProvider.GetServices<CrmAgentBase>().ToList();

        if (agents.Count == 0)
        {
            _logger.LogError("No AI agents registered in the service provider.");
            throw new InvalidOperationException(
                "No AI agents are registered. Ensure agents are added to the DI container.");
        }

        // Detect intent and entity type from the message if not explicitly provided
        var detectedIntent = AgentSelectionStrategy.DetectIntent(userMessage);
        var resolvedEntityType = entityType
            ?? AgentSelectionStrategy.DetectEntityType(userMessage)
            ?? string.Empty;

        _logger.LogDebug(
            "Detection results: Intent={Intent}, ResolvedEntityType={EntityType}.",
            detectedIntent, resolvedEntityType);

        // Find the first specialized agent that can handle this request
        // (excluding GeneralAssistant, which is the catch-all fallback)
        var selectedAgent = agents
            .Where(a => a.AgentType != AgentType.GeneralAssistant)
            .FirstOrDefault(a => a.CanHandle(resolvedEntityType, detectedIntent));

        // Fallback to GeneralAssistant if no specialized agent matched
        selectedAgent ??= agents.FirstOrDefault(a => a.AgentType == AgentType.GeneralAssistant);

        if (selectedAgent is null)
        {
            _logger.LogError("No suitable agent found, not even GeneralAssistant.");
            throw new InvalidOperationException(
                "No suitable agent found. Ensure GeneralAssistantAgent is registered.");
        }

        _logger.LogInformation(
            "Routed to agent '{AgentName}' ({AgentType}) for entity '{EntityType}'.",
            selectedAgent.AgentName, selectedAgent.AgentType, resolvedEntityType);

        return Task.FromResult(selectedAgent);
    }

    #endregion

    #region Multi-Agent Execution

    /// <summary>
    /// Executes multiple agents in parallel for a single user message.
    /// Useful for gathering diverse perspectives on complex queries.
    /// </summary>
    /// <param name="userMessage">The user's message text.</param>
    /// <param name="agentTypes">The agent type names to execute (e.g., "SalesAssistant", "DealIntelligence").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A dictionary mapping agent names to their responses.
    /// Agents that fail are included with an error message value.
    /// </returns>
    public async Task<Dictionary<string, string>> ExecuteMultiAgentAsync(
        string userMessage,
        IEnumerable<string> agentTypes,
        CancellationToken cancellationToken = default)
    {
        var allAgents = _serviceProvider.GetServices<CrmAgentBase>().ToList();
        var requestedTypeNames = new HashSet<string>(agentTypes, StringComparer.OrdinalIgnoreCase);

        var matchedAgents = allAgents
            .Where(a => requestedTypeNames.Contains(a.AgentType.ToString()))
            .ToList();

        _logger.LogInformation(
            "Executing multi-agent request with {Count} agents: {Types}.",
            matchedAgents.Count,
            string.Join(", ", matchedAgents.Select(a => a.AgentName)));

        var tasks = matchedAgents.Select(async agent =>
        {
            try
            {
                var enrichedMessage = await agent.EnrichContextAsync(
                    userMessage, null, null, cancellationToken);

                // Note: actual LLM invocation would happen here via the kernel.
                // This returns the enriched message as a placeholder for the orchestration layer.
                var response = await agent.PostProcessAsync(enrichedMessage, cancellationToken);

                return (agent.AgentName, Response: response, Error: (string?)null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Agent '{AgentName}' failed during multi-agent execution.",
                    agent.AgentName);

                return (agent.AgentName, Response: string.Empty,
                    Error: $"Agent failed: {ex.Message}");
            }
        });

        var results = await Task.WhenAll(tasks);

        var output = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (agentName, response, error) in results)
        {
            output[agentName] = error ?? response;
        }

        return output;
    }

    #endregion

    #region Agent Discovery

    /// <summary>
    /// Returns all available AI agents, optionally filtered by those enabled in the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An enumerable of <see cref="AIAgent"/> records representing the available agents,
    /// merging database-configured agents with code-registered agents.
    /// </returns>
    public async Task<IEnumerable<AIAgent>> GetAvailableAgentsAsync(
        CancellationToken cancellationToken = default)
    {
        var registeredAgents = _serviceProvider.GetServices<CrmAgentBase>().ToList();
        _logger.LogDebug("Found {Count} DI-registered AI agents.", registeredAgents.Count);

        // Query all active DB agents (both code-seeded and user-created via wizard)
        var dbAgents = await _dbContext.AIAgents
            .AsNoTracking()
            .Where(a => a.IsActive && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} active DB AI agents.", dbAgents.Count);

        // Build a lookup of DB agents by AgentType for fast merge
        var dbByType = dbAgents
            .GroupBy(a => a.AgentType)
            .ToDictionary(g => g.Key, g => g.First());

        var result = new List<AIAgent>();

        // 1. DB agents take precedence — include all active DB agents
        foreach (var db in dbAgents)
        {
            result.Add(new AIAgent
            {
                Id = db.Id,
                Name = db.Name,
                DisplayName = db.DisplayName,
                Description = db.Description,
                AgentType = db.AgentType,
                SystemPrompt = db.SystemPrompt,
                Temperature = db.Temperature,
                MaxTokens = db.MaxTokens,
                ModelOverride = db.ModelOverride,
                IsEnabled = db.IsActive,
                RequiresApproval = db.RequiresApproval,
                TotalConversations = db.TotalConversations,
                AverageRating = db.AverageRating,
                AllowedPlugins = (db.AllowedPlugins ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
            });
        }

        // 2. For DI agents whose AgentType has no DB record yet, surface them with Id=0
        foreach (var diAgent in registeredAgents)
        {
            if (!dbByType.ContainsKey(diAgent.AgentType))
            {
                result.Add(new AIAgent
                {
                    Id = 0,
                    Name = diAgent.AgentName,
                    DisplayName = diAgent.AgentName,
                    AgentType = diAgent.AgentType,
                    SystemPrompt = diAgent.SystemPrompt,
                    Temperature = diAgent.Temperature,
                    MaxTokens = diAgent.MaxTokens,
                    IsEnabled = true,
                    AllowedPlugins = diAgent.AllowedPlugins.ToList(),
                });
            }
        }

        return result;
    }

    #endregion
}

/// <summary>
/// Lightweight DTO representing an AI agent's metadata for discovery and management.
/// </summary>
public class AIAgent
{
    /// <summary>Gets or sets the database ID of the agent (0 = code-only, not persisted).</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the unique internal name of the agent.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-friendly display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description of the agent's purpose.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the agent type classification.</summary>
    public AgentType AgentType { get; set; }

    /// <summary>Gets or sets the system prompt defining agent behavior.</summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>Gets or sets the LLM temperature setting.</summary>
    public double Temperature { get; set; }

    /// <summary>Gets or sets the maximum token limit for responses.</summary>
    public int MaxTokens { get; set; }

    /// <summary>Gets or sets an optional model override (e.g. "gpt-4o").</summary>
    public string? ModelOverride { get; set; }

    /// <summary>Gets or sets whether the agent is currently enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets whether agent actions require human approval.</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Gets or sets the total number of conversations handled.</summary>
    public int TotalConversations { get; set; }

    /// <summary>Gets or sets the average user rating (null if unrated).</summary>
    public double? AverageRating { get; set; }

    /// <summary>Gets or sets the list of plugins this agent may invoke.</summary>
    public List<string> AllowedPlugins { get; set; } = new();
}
