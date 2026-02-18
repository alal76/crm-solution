// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Factories;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for provider health monitoring.
/// Provides endpoints to check the health status of all pluggable providers.
/// </summary>
[ApiController]
[Route("api/health")]
public class ProviderHealthController : ControllerBase
{
    private readonly AdapterRegistry _registry;
    private readonly IProviderFactory<ISearchPort> _searchFactory;
    private readonly IProviderFactory<IChatPort> _chatFactory;
    private readonly IProviderFactory<INotificationPort> _notificationFactory;
    private readonly IProviderFactory<IAnalyticsPort> _analyticsFactory;
    private readonly IProviderFactory<ISignaturePort> _signatureFactory;
    private readonly IProviderFactory<IAIPort> _aiFactory;
    private readonly IProviderFactory<IIntegrationPort> _integrationFactory;
    private readonly ILogger<ProviderHealthController> _logger;

    public ProviderHealthController(
        AdapterRegistry registry,
        IProviderFactory<ISearchPort> searchFactory,
        IProviderFactory<IChatPort> chatFactory,
        IProviderFactory<INotificationPort> notificationFactory,
        IProviderFactory<IAnalyticsPort> analyticsFactory,
        IProviderFactory<ISignaturePort> signatureFactory,
        IProviderFactory<IAIPort> aiFactory,
        IProviderFactory<IIntegrationPort> integrationFactory,
        ILogger<ProviderHealthController> logger)
    {
        _registry = registry;
        _searchFactory = searchFactory;
        _chatFactory = chatFactory;
        _notificationFactory = notificationFactory;
        _analyticsFactory = analyticsFactory;
        _signatureFactory = signatureFactory;
        _aiFactory = aiFactory;
        _integrationFactory = integrationFactory;
        _logger = logger;
    }

    /// <summary>
    /// Gets the health status of all pluggable providers.
    /// </summary>
    /// <returns>Provider health report with status for each category</returns>
    [HttpGet("providers")]
    [AllowAnonymous] // Health checks should be accessible for monitoring
    [ProducesResponseType(typeof(ProviderHealthReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProviderHealthReport), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ProviderHealthReport>> GetProviderHealth()
    {
        var report = new ProviderHealthReport
        {
            Timestamp = DateTime.UtcNow,
            Providers = new Dictionary<string, ProviderStatus>()
        };

        // Check each provider category
        await CheckProviderAsync(report, "Search", _searchFactory);
        await CheckProviderAsync(report, "Chat", _chatFactory);
        await CheckProviderAsync(report, "Notifications", _notificationFactory);
        await CheckProviderAsync(report, "Analytics", _analyticsFactory);
        await CheckProviderAsync(report, "Signatures", _signatureFactory);
        await CheckProviderAsync(report, "AI", _aiFactory);
        await CheckProviderAsync(report, "Integrations", _integrationFactory);

        // Add registry metrics
        report.RegistryStats = _registry.GetHealthSummary();

        // Determine overall health
        report.OverallHealthy = report.Providers.Values.All(p => p.IsHealthy);

        return report.OverallHealthy ? Ok(report) : StatusCode(503, report);
    }

    /// <summary>
    /// Gets the health status of a specific provider category.
    /// </summary>
    /// <param name="category">The provider category (Search, Chat, Notifications, etc.)</param>
    [HttpGet("providers/{category}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProviderStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderStatus>> GetProviderHealthByCategory(string category)
    {
        var report = new ProviderHealthReport { Providers = new Dictionary<string, ProviderStatus>() };

        var success = category.ToLowerInvariant() switch
        {
            "search" => await CheckProviderAsync(report, "Search", _searchFactory),
            "chat" => await CheckProviderAsync(report, "Chat", _chatFactory),
            "notifications" => await CheckProviderAsync(report, "Notifications", _notificationFactory),
            "analytics" => await CheckProviderAsync(report, "Analytics", _analyticsFactory),
            "signatures" => await CheckProviderAsync(report, "Signatures", _signatureFactory),
            "ai" => await CheckProviderAsync(report, "AI", _aiFactory),
            "integrations" => await CheckProviderAsync(report, "Integrations", _integrationFactory),
            _ => false
        };

        if (!success)
        {
            return NotFound(new { message = $"Unknown provider category: {category}" });
        }

        return Ok(report.Providers[category.ToLowerInvariant() switch
        {
            "search" => "Search",
            "chat" => "Chat",
            "notifications" => "Notifications",
            "analytics" => "Analytics",
            "signatures" => "Signatures",
            "ai" => "AI",
            "integrations" => "Integrations",
            _ => category
        }]);
    }

    /// <summary>
    /// Gets adapter registry details for all registered providers.
    /// </summary>
    [HttpGet("providers/registry")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RegistryDetailsResponse), StatusCodes.Status200OK)]
    public ActionResult<RegistryDetailsResponse> GetRegistryDetails()
    {
        var categories = new[] { "Search", "Chat", "Notifications", "Analytics", "Signatures", "AI", "Integrations" };
        var response = new RegistryDetailsResponse
        {
            Timestamp = DateTime.UtcNow,
            Categories = new Dictionary<string, List<AdapterDetails>>()
        };

        foreach (var category in categories)
        {
            var adapters = _registry.GetAdaptersByCategory(category);
            response.Categories[category] = adapters.Select(a => new AdapterDetails
            {
                ProviderName = a.ProviderName,
                Category = a.Category,
                IsActive = a.IsActive,
                Status = a.Status.ToString(),
                SuccessCount = a.SuccessCount,
                FailureCount = a.FailureCount,
                TotalOperationTime = a.TotalOperationTime,
                LastHealthCheck = a.LastHealthCheck,
                LastHealthMessage = a.LastHealthMessage,
                LastFailureTime = a.LastFailureTime,
                LastFailureMessage = a.LastFailureMessage
            }).ToList();
        }

        response.Summary = _registry.GetHealthSummary();
        return Ok(response);
    }

    private async Task<bool> CheckProviderAsync<TProvider>(
        ProviderHealthReport report,
        string category,
        IProviderFactory<TProvider> factory) where TProvider : class
    {
        try
        {
            var activeProvider = factory.GetActiveProviderName();
            var isAvailable = await factory.IsProviderAvailableAsync(activeProvider);

            report.Providers[category] = new ProviderStatus
            {
                ActiveProvider = activeProvider,
                IsHealthy = isAvailable,
                AvailableProviders = factory.GetAvailableProviders().ToList(),
                LastChecked = DateTime.UtcNow
            };

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for provider category {Category}", category);
            report.Providers[category] = new ProviderStatus
            {
                ActiveProvider = "Unknown",
                IsHealthy = false,
                Error = ex.Message,
                AvailableProviders = new List<string>(),
                LastChecked = DateTime.UtcNow
            };
            return true;
        }
    }
}

/// <summary>
/// Overall provider health report.
/// </summary>
public class ProviderHealthReport
{
    public DateTime Timestamp { get; set; }
    public bool OverallHealthy { get; set; }
    public Dictionary<string, ProviderStatus> Providers { get; set; } = new();
    public AdapterHealthSummary? RegistryStats { get; set; }
}

/// <summary>
/// Health status for a single provider category.
/// </summary>
public class ProviderStatus
{
    public string ActiveProvider { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string? Error { get; set; }
    public List<string> AvailableProviders { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

/// <summary>
/// Response for registry details endpoint.
/// </summary>
public class RegistryDetailsResponse
{
    public DateTime Timestamp { get; set; }
    public Dictionary<string, List<AdapterDetails>> Categories { get; set; } = new();
    public AdapterHealthSummary? Summary { get; set; }
}

/// <summary>
/// Detailed adapter information.
/// </summary>
public class AdapterDetails
{
    public string ProviderName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public long SuccessCount { get; set; }
    public long FailureCount { get; set; }
    public TimeSpan TotalOperationTime { get; set; }
    public DateTime? LastHealthCheck { get; set; }
    public string? LastHealthMessage { get; set; }
    public DateTime? LastFailureTime { get; set; }
    public string? LastFailureMessage { get; set; }
}
