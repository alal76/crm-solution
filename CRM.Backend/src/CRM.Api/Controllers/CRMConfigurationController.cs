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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for CRM-specific configuration management.
/// Manages AI providers, integrations, worker configuration, and AI agents.
/// </summary>
[ApiController]
[Route("api/admin/config/crm")]
[RequireRole(UserRole.Admin)]
public class CRMConfigurationController : CrmControllerBase
{
    private readonly ICRMConfigurationService _crmConfig;

    public CRMConfigurationController(
        ICRMConfigurationService crmConfig)
    {
        _crmConfig = crmConfig;
    }

    #region CRM Configuration

    /// <summary>
    /// Get all CRM configurations (AI, integrations, workers, agents)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CRMConfigResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CRMConfigResponseDto>> GetCRMConfig(CancellationToken ct)
    {
                var config = await _crmConfig.GetCRMConfigAsync(ct);
        return Ok(config);
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
                var result = await _crmConfig.TestAIProviderAsync(provider, config, ct);
        return Ok(result);
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
                var result = await _crmConfig.TestIntegrationAsync(type, provider, config, ct);
        return Ok(result);
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
