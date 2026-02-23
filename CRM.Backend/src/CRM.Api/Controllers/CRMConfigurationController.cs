// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Api.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for CRM-specific configuration management.
/// Manages AI providers, integrations, worker configuration, and AI agents.
/// </summary>
[ApiController]
[Route("api/admin/config/crm")]
[RequireRole(UserRole.Admin)]
public class CRMConfigurationController : ControllerBase
{
    private readonly ICRMConfigurationService _crmConfig;
    private readonly ILogger<CRMConfigurationController> _logger;

    public CRMConfigurationController(
        ICRMConfigurationService crmConfig,
        ILogger<CRMConfigurationController> logger)
    {
        _crmConfig = crmConfig;
        _logger = logger;
    }

    #region CRM Configuration

    /// <summary>
    /// Get all CRM configurations (AI, integrations, workers, agents)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CRMConfigResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CRMConfigResponseDto>> GetCRMConfig(CancellationToken ct)
    {
        try
        {
            var config = await _crmConfig.GetCRMConfigAsync(ct);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CRM configuration");
            return StatusCode(500, new { error = "Failed to retrieve CRM configuration" });
        }
    }

    #endregion

    #region AI Provider

    /// <summary>
    /// Update AI provider configuration
    /// </summary>
    [HttpPut("ai/{provider}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAIProvider(
        string provider,
        [FromBody] AIProviderConfigDto config,
        CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _crmConfig.UpdateAIProviderAsync(provider, config, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI provider {Provider}", provider);
            return StatusCode(500, new { error = $"Failed to update AI provider '{provider}'" });
        }
    }

    /// <summary>
    /// Test AI provider connectivity
    /// </summary>
    [HttpPost("ai/{provider}/test")]
    [ProducesResponseType(typeof(ConfigurationTestResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfigurationTestResultDto>> TestAIProvider(
        string provider,
        [FromBody] AIProviderConfigDto config,
        CancellationToken ct)
    {
        try
        {
            var result = await _crmConfig.TestAIProviderAsync(provider, config, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing AI provider {Provider}", provider);
            return StatusCode(500, new { error = $"Failed to test AI provider '{provider}'" });
        }
    }

    #endregion

    #region Integrations

    /// <summary>
    /// Update integration configuration
    /// </summary>
    [HttpPut("integration/{type}/{provider}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateIntegration(
        string type,
        string provider,
        [FromBody] IntegrationConfigDto config,
        CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _crmConfig.UpdateIntegrationAsync(type, provider, config, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating integration {Type}/{Provider}", type, provider);
            return StatusCode(500, new { error = $"Failed to update integration '{type}/{provider}'" });
        }
    }

    /// <summary>
    /// Test integration connectivity
    /// </summary>
    [HttpPost("integration/{type}/{provider}/test")]
    [ProducesResponseType(typeof(ConfigurationTestResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfigurationTestResultDto>> TestIntegration(
        string type,
        string provider,
        [FromBody] IntegrationConfigDto config,
        CancellationToken ct)
    {
        try
        {
            var result = await _crmConfig.TestIntegrationAsync(type, provider, config, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing integration {Type}/{Provider}", type, provider);
            return StatusCode(500, new { error = $"Failed to test integration '{type}/{provider}'" });
        }
    }

    #endregion

    #region Worker Configuration

    /// <summary>
    /// Update worker/background job configuration
    /// </summary>
    [HttpPut("worker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateWorkerConfig([FromBody] WorkerConfigDto config, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _crmConfig.UpdateWorkerConfigAsync(config, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating worker configuration");
            return StatusCode(500, new { error = "Failed to update worker configuration" });
        }
    }

    #endregion

    #region AI Agents

    /// <summary>
    /// Update AI agents configuration and enablement
    /// </summary>
    [HttpPut("agents")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAIAgents([FromBody] List<AIAgentConfigDto> agents, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _crmConfig.UpdateAIAgentsAsync(agents, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI agents configuration");
            return StatusCode(500, new { error = "Failed to update AI agents configuration" });
        }
    }

    #endregion

    #region Helpers

    private int GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    #endregion
}
