// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Authorization;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin API for managing pluggable provider configurations, activation, health checks,
/// and provider-related feature flags.
///
/// All endpoints require the Admin role.
/// </summary>
[ApiController]
[Route("api/admin/providers")]
[RequireRole(UserRole.Admin)]
public class AdminProvidersController : CrmControllerBase
{
    private readonly IProviderRegistryService _registry;
    private readonly ILogger<AdminProvidersController> _logger;

    public AdminProvidersController(
        IProviderRegistryService registry,
        ILogger<AdminProvidersController> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ────────────────────────────────────── Registry endpoints ──

    /// <summary>
    /// List all registered providers grouped by category.
    /// GET /api/admin/providers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProviderRegistryEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProviders(CancellationToken cancellationToken = default)
    {
        var providers = await _registry.GetAllProvidersAsync(cancellationToken);
        return Ok(providers);
    }

    /// <summary>
    /// List all providers for a specific category.
    /// GET /api/admin/providers/{category}
    /// </summary>
    [HttpGet("{category}")]
    [ProducesResponseType(typeof(IEnumerable<ProviderRegistryEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProvidersByCategory(
        [FromRoute] string category,
        CancellationToken cancellationToken = default)
    {
        var providers = await _registry.GetProvidersByCategoryAsync(category, cancellationToken);
        var list = providers.ToList();
        if (!list.Any())
        {
            return NotFound(new { message = $"No providers found for category '{category}'" });
        }

        return Ok(list);
    }

    /// <summary>
    /// Get the active provider configuration for a category.
    /// GET /api/admin/providers/{category}/active
    /// </summary>
    [HttpGet("{category}/active")]
    [ProducesResponseType(typeof(ProviderConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveProvider(
        [FromRoute] string category,
        CancellationToken cancellationToken = default)
    {
        var config = await _registry.GetActiveProviderConfigAsync(category, cancellationToken);
        if (config == null)
        {
            return NotFound(new { message = $"No active provider configuration found for category '{category}'" });
        }

        return Ok(config);
    }

    /// <summary>
    /// Activate a provider for a category, saving its configuration.
    /// PUT /api/admin/providers/{category}/active
    /// Body: { "providerType": "Meilisearch", "config": { "Url": "...", "ApiKey": "..." } }
    /// </summary>
    [HttpPut("{category}/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActiveProvider(
        [FromRoute] string category,
        [FromBody] SetActiveProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderType))
        {
            return BadRequest(new { message = "providerType is required" });
        }

        var available = await _registry.IsProviderAvailableAsync(category, request.ProviderType, cancellationToken);
        if (!available)
        {
            return NotFound(new { message = $"Provider '{request.ProviderType}' is not registered in category '{category}'" });
        }

        await _registry.SetActiveProviderAsync(category, request.ProviderType, request.Config ?? new(), cancellationToken);

        _logger.LogInformation(
            "Admin set active provider for {Category} to {ProviderType}",
            category, request.ProviderType);

        return Ok(new { message = $"Provider '{request.ProviderType}' activated for category '{category}'" });
    }

    // ────────────────────────────────────── Health endpoints ──

    /// <summary>
    /// Check the health of a specific provider.
    /// GET /api/admin/providers/{category}/{type}/health
    /// </summary>
    [HttpGet("{category}/{type}/health")]
    [ProducesResponseType(typeof(ProviderHealthStatusResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderHealth(
        [FromRoute] string category,
        [FromRoute] string type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _registry.CheckProviderHealthAsync(category, type, cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for {Category}/{Type}", category, type);
            return Ok(new ProviderHealthStatusResult
            {
                IsHealthy = false,
                Message = "Health check error",
                CheckedAt = DateTime.UtcNow,
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Test provider connectivity (same as health but uses POST for explicit action).
    /// POST /api/admin/providers/{category}/{type}/test
    /// </summary>
    [HttpPost("{category}/{type}/test")]
    [ProducesResponseType(typeof(ProviderHealthStatusResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestProviderConnection(
        [FromRoute] string category,
        [FromRoute] string type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin testing connection for {Category}/{Type}", category, type);
            var health = await _registry.CheckProviderHealthAsync(category, type, cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing provider connection for {Category}/{Type}", category, type);
            return Ok(new ProviderHealthStatusResult
            {
                IsHealthy = false,
                Message = "Connection test failed",
                CheckedAt = DateTime.UtcNow,
                ErrorDetails = ex.Message
            });
        }
    }

    // ────────────────────────────────────── Feature flags endpoints ──

    /// <summary>
    /// Get all provider-related feature flags and active provider types.
    /// GET /api/admin/providers/feature-flags
    /// </summary>
    [HttpGet("feature-flags")]
    [ProducesResponseType(typeof(ProviderFeatureFlagsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatureFlags(CancellationToken cancellationToken = default)
    {
        var flags = await _registry.GetProviderFeatureFlagsAsync(cancellationToken);
        return Ok(flags);
    }

    /// <summary>
    /// Update provider feature flags (UseExternalSearch, UseExternalAI, etc.).
    /// PUT /api/admin/providers/feature-flags
    /// Body: { "flags": { "UseExternalSearch": true, "UseExternalAI": false } }
    /// </summary>
    [HttpPut("feature-flags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFeatureFlags(
        [FromBody] UpdateProviderFlagsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.Flags == null || !request.Flags.Any())
        {
            return BadRequest(new { message = "At least one feature flag must be provided" });
        }

        await _registry.UpdateProviderFeatureFlagsAsync(request.Flags, cancellationToken);
        _logger.LogInformation("Admin updated {Count} provider feature flag(s)", request.Flags.Count);
        return Ok(new { message = $"{request.Flags.Count} feature flag(s) updated successfully" });
    }
}
