// -----------------------------------------------------------------------
// CRM Solution - Semantic Kernel AI Integration
// Copyright (c) 2024-2026 Abhishek Lal (CRM Solution). All rights reserved.
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.
//
// This file is part of the CRM Solution, an enterprise-grade
// Customer Relationship Management system.
//
// Author: Abhishek Lal
// Repository: https://github.com/abhisheklal04/crm-solution
// Documentation: See /docs folder for architecture and API reference
//
// IMPORTANT: This is proprietary code. Unauthorized copying, modification,
// or distribution is strictly prohibited.
// -----------------------------------------------------------------------

#nullable enable

using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers;

/// <summary>
/// Administrative controller for managing AI agent configurations.
/// Provides endpoints for viewing, updating, and toggling agent settings.
/// </summary>
[ApiController]
[Route("api/agents/admin")]
[Authorize]
public class AgentAdminController : ControllerBase
{
    #region Fields

    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AgentAdminController> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentAdminController"/> class.
    /// </summary>
    /// <param name="dbContext">The CRM database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AgentAdminController(ICrmDbContext dbContext, ILogger<AgentAdminController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region DTOs

    /// <summary>
    /// Request DTO for updating an AI agent's configuration.
    /// </summary>
    /// <param name="SystemPrompt">Updated system prompt for the agent.</param>
    /// <param name="Temperature">Updated temperature setting (0.0 to 2.0).</param>
    /// <param name="MaxTokens">Updated maximum tokens per response.</param>
    /// <param name="AllowedPlugins">Updated comma-separated list of allowed plugin names.</param>
    /// <param name="ModelOverride">Optional model override (e.g., "gpt-4o", "llama3").</param>
    public record UpdateAgentRequest(
        string? SystemPrompt = null,
        double? Temperature = null,
        int? MaxTokens = null,
        string? AllowedPlugins = null,
        string? ModelOverride = null);

    #endregion

    #region Endpoints

    /// <summary>
    /// Gets all AI agent configurations.
    /// </summary>
    /// <returns>A list of all agent configurations including disabled agents.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgentConfigs()
    {
        try
        {
            var agents = await _dbContext.AIAgents
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.AgentType,
                    a.SystemPrompt,
                    a.AllowedPlugins,
                    a.IsEnabled,
                    a.Temperature,
                    a.MaxTokens,
                    a.CreatedAt,
                    a.UpdatedAt,
                })
                .ToListAsync(HttpContext.RequestAborted);

            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent configurations");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving agent configurations.");
        }
    }

    /// <summary>
    /// Updates an AI agent's configuration.
    /// </summary>
    /// <param name="agentId">The agent ID to update.</param>
    /// <param name="request">The update request with new configuration values.</param>
    /// <returns>The updated agent configuration.</returns>
    [HttpPut("{agentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAgentConfig(int agentId, [FromBody] UpdateAgentRequest request)
    {
        try
        {
            var agent = await _dbContext.AIAgents
                .FirstOrDefaultAsync(a => a.Id == agentId && !a.IsDeleted, HttpContext.RequestAborted);

            if (agent is null)
            {
                return NotFound($"Agent with ID {agentId} not found.");
            }

            if (request.Temperature.HasValue)
            {
                if (request.Temperature.Value < 0.0 || request.Temperature.Value > 2.0)
                {
                    return BadRequest("Temperature must be between 0.0 and 2.0.");
                }

                agent.Temperature = request.Temperature.Value;
            }

            if (request.MaxTokens.HasValue)
            {
                if (request.MaxTokens.Value < 1)
                {
                    return BadRequest("MaxTokens must be a positive integer.");
                }

                agent.MaxTokens = request.MaxTokens.Value;
            }

            if (request.SystemPrompt is not null)
            {
                agent.SystemPrompt = request.SystemPrompt;
            }

            if (request.AllowedPlugins is not null)
            {
                agent.AllowedPlugins = request.AllowedPlugins;
            }

            agent.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

            _logger.LogInformation("Agent {AgentId} ({AgentName}) configuration updated", agentId, agent.Name);
            return Ok(new
            {
                agent.Id,
                agent.Name,
                agent.AgentType,
                agent.SystemPrompt,
                agent.AllowedPlugins,
                agent.IsEnabled,
                agent.Temperature,
                agent.MaxTokens,
                agent.UpdatedAt,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent {AgentId} configuration", agentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the agent configuration.");
        }
    }

    /// <summary>
    /// Toggles an AI agent's enabled/disabled state.
    /// </summary>
    /// <param name="agentId">The agent ID to toggle.</param>
    /// <returns>The updated enabled state.</returns>
    [HttpPost("{agentId:int}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleAgent(int agentId)
    {
        try
        {
            var agent = await _dbContext.AIAgents
                .FirstOrDefaultAsync(a => a.Id == agentId && !a.IsDeleted, HttpContext.RequestAborted);

            if (agent is null)
            {
                return NotFound($"Agent with ID {agentId} not found.");
            }

            agent.IsEnabled = !agent.IsEnabled;
            agent.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

            _logger.LogInformation("Agent {AgentId} ({AgentName}) toggled to {IsEnabled}", agentId, agent.Name, agent.IsEnabled);
            return Ok(new { agent.Id, agent.Name, agent.IsEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling agent {AgentId}", agentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while toggling the agent.");
        }
    }

    #endregion
}
