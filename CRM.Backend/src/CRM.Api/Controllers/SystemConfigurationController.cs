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
/// Controller for system-level configuration management.
/// Manages email server, two-factor authentication, social login,
/// provider configurations, change history and rollback.
/// </summary>
[ApiController]
[Route("api/admin/config")]
[RequireRole(UserRole.Admin)]
public class SystemConfigurationController : CrmControllerBase
{
    private readonly ISystemConfigurationService _systemConfig;
    private readonly IProviderConfigurationService _providerConfig;
    private readonly ILogger<SystemConfigurationController> _logger;

    public SystemConfigurationController(
        ISystemConfigurationService systemConfig,
        IProviderConfigurationService providerConfig,
        ILogger<SystemConfigurationController> logger)
    {
        _systemConfig = systemConfig;
        _providerConfig = providerConfig;
        _logger = logger;
    }

    #region System Configuration

    /// <summary>
    /// Get all system configurations (email, 2FA, social login)
    /// </summary>
    [HttpGet("system")]
    [ProducesResponseType(typeof(SystemConfigResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemConfigResponseDto>> GetSystemConfig(CancellationToken ct)
    {
                var config = await _systemConfig.GetSystemConfigAsync(ct);
        return Ok(config);
    }

    /// <summary>
    /// Update email server configuration
    /// </summary>
    [HttpPut("system/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEmailServer([FromBody] EmailServerConfigDto config, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _systemConfig.UpdateEmailServerAsync(config, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test email server connectivity
    /// </summary>
    [HttpPost("system/email/test")]
    [ProducesResponseType(typeof(ConfigurationTestResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfigurationTestResultDto>> TestEmailServer([FromBody] EmailServerConfigDto config, CancellationToken ct)
    {
                var result = await _systemConfig.TestEmailServerAsync(config, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update two-factor authentication configuration
    /// </summary>
    [HttpPut("system/2fa")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTwoFactor([FromBody] TwoFactorConfigDto config, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _systemConfig.UpdateTwoFactorAsync(config, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update social login configuration
    /// </summary>
    [HttpPut("system/social")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSocialLogin([FromBody] SocialLoginConfigDto config, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            await _systemConfig.UpdateSocialLoginAsync(config, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test social login provider OAuth configuration
    /// </summary>
    [HttpPost("system/social/{provider}/test")]
    [ProducesResponseType(typeof(ConfigurationTestResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfigurationTestResultDto>> TestSocialProvider(
        string provider,
        [FromBody] Dictionary<string, string> credentials,
        CancellationToken ct)
    {
                var result = await _systemConfig.TestSocialProviderAsync(provider, credentials, ct);
        return Ok(result);
    }

    #endregion

    #region Change History & Rollback

    /// <summary>
    /// Get configuration change history
    /// </summary>
    [HttpGet("changelog")]
    [ProducesResponseType(typeof(List<ConfigurationChangeLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConfigurationChangeLogDto>>> GetChangeHistory(
        [FromQuery] string? configKey = null,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
                var history = await _providerConfig.GetChangeHistoryAsync(configKey, pageSize, ct);
        return Ok(history);
    }

    /// <summary>
    /// Rollback a configuration to a previous state
    /// </summary>
    [HttpPost("changelog/{changeId}/rollback")]
    [ProducesResponseType(typeof(ConfigurationTestResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigurationTestResultDto>> RollbackConfiguration(int changeId, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            var result = await _providerConfig.RollbackConfigurationAsync(changeId, userId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    #endregion

    #region Provider Discovery

    /// <summary>
    /// Get available providers for a given type
    /// </summary>
    [HttpGet("providers/{type}")]
    [ProducesResponseType(typeof(List<ProviderInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProviderInfoDto>>> GetAvailableProviders(string type, CancellationToken ct)
    {
                var providers = await _providerConfig.GetAvailableProvidersAsync(type, ct);
        return Ok(providers);
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
