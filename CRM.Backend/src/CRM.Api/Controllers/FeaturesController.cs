// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Authorization;
using CRM.Core.Entities;
using CRM.Core.Features;
using CRM.Infrastructure.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing feature flags and provider configuration
/// Implements the pluggable architecture feature flag system
/// See: docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md Section 17
/// </summary>
[ApiController]
[Route("api/admin/features")]
[RequireRole(UserRole.Admin)]
public class FeaturesController : CrmControllerBase
{
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly AdapterRegistry _adapterRegistry;
    private readonly ILogger<FeaturesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeaturesController"/> class.
    /// </summary>
    /// <param name="featureManager">The feature manager.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="adapterRegistry">The adapter registry.</param>
    /// <param name="logger">The logger instance.</param>
    public FeaturesController(
        IFeatureManager featureManager,
        IConfiguration configuration,
        AdapterRegistry adapterRegistry,
        ILogger<FeaturesController> logger)
    {
        _featureManager = featureManager;
        _configuration = configuration;
        _adapterRegistry = adapterRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Get all feature flags and their current status
    /// </summary>
    /// <returns>Dictionary of feature flags and their enabled status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(FeatureFlagsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FeatureFlagsResponse>> GetFeatureFlags()
    {
                var response = new FeatureFlagsResponse
        {
            ProviderSelection = new ProviderSelectionFlags
            {
                UseExternalChat = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat),
                UseExternalSearch = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch),
                UseExternalNotifications = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications),
                UseExternalAnalytics = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics),
                UseExternalSignatures = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures),
                UseExternalAI = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI),
                UseExternalIntegrations = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations)
            },
            Modules = new ModuleFlags
            {
                ITSMEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableITSM),
                MarketingEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableMarketing),
                CustomerPortalEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableCustomerPortal),
                PartnerPortalEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnablePartnerPortal),
                KnowledgeBaseEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.EnableKnowledgeBase)
            },
            Features = new FeatureRolloutFlags
            {
                NewSearchExperience = await _featureManager.IsEnabledAsync(FeatureFlags.NewSearchExperience),
                AIAssistant = await _featureManager.IsEnabledAsync(FeatureFlags.AIAssistant),
                RealTimeNotifications = await _featureManager.IsEnabledAsync(FeatureFlags.RealTimeNotifications),
                AdvancedWorkflows = await _featureManager.IsEnabledAsync(FeatureFlags.AdvancedWorkflows)
            },
            Providers = GetProviderConfiguration()
        };

        _logger.LogInformation("Feature flags retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Get a specific feature flag status
    /// </summary>
    /// <param name="featureName">The feature flag name</param>
    /// <returns>Feature flag status</returns>
    [HttpGet("{featureName}")]
    [ProducesResponseType(typeof(FeatureFlagStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<FeatureFlagStatus>> GetFeatureFlag(string featureName)
    {
                var isEnabled = await _featureManager.IsEnabledAsync(featureName);

        return Ok(new FeatureFlagStatus
        {
            Name = featureName,
            IsEnabled = isEnabled,
            RetrievedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get available provider types for each category
    /// </summary>
    /// <returns>Dictionary of provider categories and their available types</returns>
    [HttpGet("providers/available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Dictionary<string, IEnumerable<string>>> GetAvailableProviders()
    {
                var providers = new Dictionary<string, IEnumerable<string>>
        {
            { "Chat", new[] { ProviderTypes.Chat.BuiltIn, ProviderTypes.Chat.Chatwoot, ProviderTypes.Chat.Intercom, ProviderTypes.Chat.Zendesk, ProviderTypes.Chat.Freshchat, ProviderTypes.Chat.RocketChat } },
            { "Search", new[] { ProviderTypes.Search.BuiltIn, ProviderTypes.Search.Meilisearch, ProviderTypes.Search.Algolia, ProviderTypes.Search.Typesense, ProviderTypes.Search.Elasticsearch, ProviderTypes.Search.AzureCognitiveSearch } },
            { "Notifications", new[] { ProviderTypes.Notifications.BuiltIn, ProviderTypes.Notifications.Novu, ProviderTypes.Notifications.Twilio, ProviderTypes.Notifications.SendGrid, ProviderTypes.Notifications.AWSSES, ProviderTypes.Notifications.Courier } },
            { "Analytics", new[] { ProviderTypes.Analytics.BuiltIn, ProviderTypes.Analytics.Superset, ProviderTypes.Analytics.Metabase, ProviderTypes.Analytics.PowerBI, ProviderTypes.Analytics.Tableau, ProviderTypes.Analytics.Looker } },
            { "Signatures", new[] { ProviderTypes.Signatures.BuiltIn, ProviderTypes.Signatures.DocuSeal, ProviderTypes.Signatures.DocuSign, ProviderTypes.Signatures.HelloSign, ProviderTypes.Signatures.AdobeSign, ProviderTypes.Signatures.PandaDoc } },
            { "AI", new[] { ProviderTypes.AI.Ollama, ProviderTypes.AI.OpenAI, ProviderTypes.AI.Anthropic, ProviderTypes.AI.AzureOpenAI, ProviderTypes.AI.Gemini, ProviderTypes.AI.DeepSeek } },
            { "Integrations", new[] { ProviderTypes.Integrations.BuiltIn, ProviderTypes.Integrations.N8n, ProviderTypes.Integrations.Zapier, ProviderTypes.Integrations.Make, ProviderTypes.Integrations.Automatisch, ProviderTypes.Integrations.Workato } },
            { "DataSync", new[] { ProviderTypes.DataSync.BuiltIn, ProviderTypes.DataSync.Airbyte, ProviderTypes.DataSync.Fivetran, ProviderTypes.DataSync.Segment } },
            { "Compliance", new[] { ProviderTypes.Compliance.BuiltIn, ProviderTypes.Compliance.Fides, ProviderTypes.Compliance.OneTrust, ProviderTypes.Compliance.TrustArc } }
        };

        return Ok(providers);
    }

    /// <summary>
    /// Get current provider health status
    /// </summary>
    /// <returns>Health status for all configured providers</returns>
    [HttpGet("providers/health")]
    [ProducesResponseType(typeof(ProviderHealthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderHealthResponse>> GetProviderHealth()
    {
        await Task.CompletedTask; // Stub for future async implementation

                var checkedAt = DateTime.UtcNow;
        var categories = new[] { "Chat", "Search", "Notifications", "Analytics", "Signatures", "Integrations" };
        var health = new ProviderHealthResponse { CheckedAt = checkedAt };

        foreach (var category in categories)
        {
            var active = _adapterRegistry.GetActiveAdapter(category);
            if (active != null)
            {
                health.Providers[category] = new ProviderHealthStatus
                {
                    Type = active.ProviderName,
                    Status = active.Status.ToString(),
                    LastCheck = active.LastHealthCheck ?? checkedAt,
                    ErrorMessage = active.LastFailureMessage
                };
            }
            else
            {
                health.Providers[category] = new ProviderHealthStatus
                {
                    Type = GetProviderType(category),
                    Status = "Unknown",
                    LastCheck = checkedAt
                };
            }
        }

        return Ok(health);
    }

    private ProviderConfiguration GetProviderConfiguration()
    {
        var providersSection = _configuration.GetSection("Providers");

        return new ProviderConfiguration
        {
            Chat = GetProviderType("Chat"),
            Search = GetProviderType("Search"),
            Notifications = GetProviderType("Notifications"),
            Analytics = GetProviderType("Analytics"),
            Signatures = GetProviderType("Signatures"),
            Integrations = GetProviderType("Integrations"),
            Compliance = GetProviderType("Compliance")
        };
    }

    private string GetProviderType(string category)
    {
        return _configuration.GetValue<string>($"Providers:{category}:Type") ?? "BuiltIn";
    }
}

#region Response DTOs

public class FeatureFlagsResponse
{
    public ProviderSelectionFlags ProviderSelection { get; set; } = new();
    public ModuleFlags Modules { get; set; } = new();
    public FeatureRolloutFlags Features { get; set; } = new();
    public ProviderConfiguration Providers { get; set; } = new();
}

public class ProviderSelectionFlags
{
    public bool UseExternalChat { get; set; }
    public bool UseExternalSearch { get; set; }
    public bool UseExternalNotifications { get; set; }
    public bool UseExternalAnalytics { get; set; }
    public bool UseExternalSignatures { get; set; }
    public bool UseExternalAI { get; set; }
    public bool UseExternalIntegrations { get; set; }
}

public class ModuleFlags
{
    public bool ITSMEnabled { get; set; }
    public bool MarketingEnabled { get; set; }
    public bool CustomerPortalEnabled { get; set; }
    public bool PartnerPortalEnabled { get; set; }
    public bool KnowledgeBaseEnabled { get; set; }
}

public class FeatureRolloutFlags
{
    public bool NewSearchExperience { get; set; }
    public bool AIAssistant { get; set; }
    public bool RealTimeNotifications { get; set; }
    public bool AdvancedWorkflows { get; set; }
}

public class ProviderConfiguration
{
    public string Chat { get; set; } = "BuiltIn";
    public string Search { get; set; } = "BuiltIn";
    public string Notifications { get; set; } = "BuiltIn";
    public string Analytics { get; set; } = "BuiltIn";
    public string Signatures { get; set; } = "BuiltIn";
    public string Integrations { get; set; } = "BuiltIn";
    public string Compliance { get; set; } = "BuiltIn";
}

public class FeatureFlagStatus
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime RetrievedAt { get; set; }
}

public class ProviderHealthResponse
{
    public DateTime CheckedAt { get; set; }
    public Dictionary<string, ProviderHealthStatus> Providers { get; set; } = new();
}

public class ProviderHealthStatus
{
    public string Type { get; set; } = "BuiltIn";
    public string Status { get; set; } = "Unknown";
    public DateTime LastCheck { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion
