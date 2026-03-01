// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Diagnostics;
using CRM.Core.Dtos;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

// Alias to avoid ambiguity between the ProviderHealthStatus enum (CRM.Core.Interfaces)
// and any future class with the same name in other namespaces.
using ProviderHealthStatusEnum = CRM.Core.Interfaces.ProviderHealthStatus;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implements the provider registry — a catalogue of all known pluggable providers
/// across every category (Search, Chat, Notifications, Analytics, Signatures, AI, Integrations).
///
/// Responsibilities:
/// - Returns structured metadata for every provider in the registry
/// - Resolves which provider is currently active from IConfiguration
/// - Persists provider configuration changes via IProviderConfigurationService
/// - Delegates health checks to IProviderHealthService
/// - Reads / updates feature flags via IFeatureManager
/// </summary>
public class ProviderRegistryService : IProviderRegistryService
{
    private readonly IConfiguration _configuration;
    private readonly IFeatureManager _featureManager;
    private readonly IProviderHealthService _healthService;
    private readonly IProviderConfigurationService _configService;
    private readonly ILogger<ProviderRegistryService> _logger;

    // A static in-memory registry of all known providers + their metadata.
    // Keyed by "Category:ProviderType" (case-insensitive on use).
    private static readonly IReadOnlyList<ProviderRegistryEntry> _registry = BuildRegistry();

    public ProviderRegistryService(
        IConfiguration configuration,
        IFeatureManager featureManager,
        IProviderHealthService healthService,
        IProviderConfigurationService configService,
        ILogger<ProviderRegistryService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─────────────────────────────────────────────── IProviderRegistryService ──

    /// <inheritdoc/>
    public async Task<IEnumerable<ProviderRegistryEntry>> GetAllProvidersAsync(CancellationToken ct = default)
    {
        var entries = _registry.Select(e => e.ShallowClone()).ToList();
        EnrichWithActiveStatus(entries);
        EnrichWithConfiguredStatus(entries);
        return await Task.FromResult(entries);
    }

    /// <inheritdoc/>
    public async Task<ProviderRegistryEntry?> GetProviderAsync(string category, string providerType, CancellationToken ct = default)
    {
        var entry = FindEntry(category, providerType);
        if (entry == null)
        {
            return null;
        }

        var clone = entry.ShallowClone();
        EnrichWithActiveStatus(new[] { clone });
        EnrichWithConfiguredStatus(new[] { clone });
        return await Task.FromResult(clone);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProviderRegistryEntry>> GetProvidersByCategoryAsync(string category, CancellationToken ct = default)
    {
        var entries = _registry
            .Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.ShallowClone())
            .ToList();

        EnrichWithActiveStatus(entries);
        EnrichWithConfiguredStatus(entries);
        return await Task.FromResult(entries);
    }

    /// <inheritdoc/>
    public Task<bool> IsProviderAvailableAsync(string category, string providerType, CancellationToken ct = default)
    {
        var found = FindEntry(category, providerType) != null;
        return Task.FromResult(found);
    }

    /// <inheritdoc/>
    public async Task<ProviderHealthStatusResult> CheckProviderHealthAsync(string category, string providerType, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var dto = await _healthService.GetProviderHealthAsync(category, providerType, ct);
            sw.Stop();
            return new ProviderHealthStatusResult
            {
                IsHealthy = dto.Status == (int)ProviderHealthStatusEnum.Healthy,
                Message = dto.StatusText ?? "Unknown",
                CheckedAt = dto.LastCheckedAt ?? DateTime.UtcNow,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Health check failed for {Category}/{ProviderType}", category, providerType);
            return new ProviderHealthStatusResult
            {
                IsHealthy = false,
                Message = "Health check failed",
                CheckedAt = DateTime.UtcNow,
                LatencyMs = sw.ElapsedMilliseconds,
                ErrorDetails = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ProviderConfigurationDto?> GetActiveProviderConfigAsync(string category, CancellationToken ct = default)
    {
        var key = $"crm.{category.ToLowerInvariant()}";
        return await _configService.GetConfigurationAsync(key, ct);
    }

    /// <inheritdoc/>
    public async Task SetActiveProviderAsync(string category, string providerType, Dictionary<string, string> config, CancellationToken ct = default)
    {
        _logger.LogInformation("Setting active provider for {Category} to {ProviderType}", category, providerType);

        var configKey = $"crm.{category.ToLowerInvariant()}.{providerType.ToLowerInvariant()}";

        // Build the config dictionary as object (required by service)
        var configData = config.ToDictionary(
            kvp => kvp.Key,
            kvp => (object)kvp.Value);

        // Add the provider type
        configData["ProviderType"] = providerType;
        configData["Category"] = category;

        // Use a fixed system user (0 = system) since no user context at service layer
        await _configService.UpdateConfigurationAsync(configKey, configData, 0, ct);

        _logger.LogInformation(
            "Provider {Category}/{ProviderType} configuration saved under key={Key}",
            category, providerType, configKey);
    }

    /// <inheritdoc/>
    public async Task<ProviderFeatureFlagsDto> GetProviderFeatureFlagsAsync(CancellationToken ct = default)
    {
        var flags = new ProviderFeatureFlagsDto
        {
            UseExternalSearch = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch),
            UseExternalChat = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat),
            UseExternalNotifications = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications),
            UseExternalAnalytics = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics),
            UseExternalSignatures = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures),
            UseExternalAI = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI),
            UseExternalIntegrations = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations)
        };

        // Read the currently configured provider type per category
        foreach (var category in new[] { "Search", "Chat", "Notifications", "Analytics", "Signatures", "AI", "Integrations" })
        {
            var configuredType = _configuration[$"Providers:{category}:Type"];
            if (!string.IsNullOrEmpty(configuredType))
            {
                flags.ActiveProviders[category] = configuredType;
            }
        }

        return flags;
    }

    /// <inheritdoc/>
    public async Task UpdateProviderFeatureFlagsAsync(Dictionary<string, bool> flags, CancellationToken ct = default)
    {
        // Persist each flag to a ProviderConfiguration record so it survives restarts.
        // The actual IFeatureManager is backed by appsettings at startup; we persist the
        // intent in the DB so that the config can be applied on next restart / reload.
        foreach (var (flagName, enabled) in flags)
        {
            var configKey = $"feature.{flagName.ToLowerInvariant()}";
            var configData = new Dictionary<string, object>
            {
                ["FlagName"] = flagName,
                ["Enabled"] = enabled,
                ["UpdatedAt"] = DateTime.UtcNow.ToString("O")
            };

            try
            {
                await _configService.UpdateConfigurationAsync(configKey, configData, 0, ct);
                _logger.LogInformation("Feature flag {FlagName} set to {Enabled}", flagName, enabled);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not persist feature flag {FlagName}", flagName);
            }
        }
    }

    // ──────────────────────────────────────────────────── internal helpers ──

    private static ProviderRegistryEntry? FindEntry(string category, string providerType)
        => _registry.FirstOrDefault(e =>
            string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.ProviderType, providerType, StringComparison.OrdinalIgnoreCase));

    private void EnrichWithActiveStatus(IEnumerable<ProviderRegistryEntry> entries)
    {
        foreach (var entry in entries)
        {
            var configuredType = _configuration[$"Providers:{entry.Category}:Type"]
                ?? (entry.IsBuiltIn ? "BuiltIn" : string.Empty);

            var useExternalFlag = entry.Category switch
            {
                "Search" => FeatureFlags.UseExternalSearch,
                "Chat" => FeatureFlags.UseExternalChat,
                "Notifications" => FeatureFlags.UseExternalNotifications,
                "Analytics" => FeatureFlags.UseExternalAnalytics,
                "Signatures" => FeatureFlags.UseExternalSignatures,
                "AI" => FeatureFlags.UseExternalAI,
                "Integrations" => FeatureFlags.UseExternalIntegrations,
                _ => null
            };

            bool useExternal = useExternalFlag != null &&
                               _featureManager.IsEnabledAsync(useExternalFlag).GetAwaiter().GetResult(); // NOSONAR S4462 -- called during service initialization; sync interface requirement // NOSONAR S4462 -- called during service initialization; sync interface requirement

            // Built-in is active when external flag is off; external type is active when flag is on and type matches
            entry.IsActive = entry.IsBuiltIn
                ? !useExternal
                : (useExternal && string.Equals(configuredType, entry.ProviderType, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void EnrichWithConfiguredStatus(IEnumerable<ProviderRegistryEntry> entries)
    {
        foreach (var entry in entries)
        {
            // Built-in providers need no external config
            if (entry.IsBuiltIn)
            {
                entry.IsConfigured = true;
                continue;
            }

            // Check if any required config field has a value in IConfiguration
            var configSection = _configuration.GetSection($"Providers:{entry.Category}:{entry.ProviderType}");
            var requiredFields = entry.ConfigFields.Where(f => f.Required).ToList();

            if (!requiredFields.Any())
            {
                entry.IsConfigured = configSection.Exists();
            }
            else
            {
                entry.IsConfigured = requiredFields.All(f =>
                    !string.IsNullOrEmpty(configSection[f.Key]));
            }
        }
    }

    // ───────────────────────────────────────── static registry builder ──

    private static List<ProviderRegistryEntry> BuildRegistry()
    {
        var entries = new List<ProviderRegistryEntry>();

        // ── Search ──────────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Search",
            ProviderType = ProviderTypes.Search.BuiltIn,
            DisplayName = "Built-In Search",
            Description = "Entity Framework Core database queries. No external dependency required.",
            IsBuiltIn = true,
            IsSaaS = false
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Search",
            ProviderType = ProviderTypes.Search.Meilisearch,
            DisplayName = "Meilisearch",
            Description = "Open-source, fast and lightweight search engine. Self-hostable.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://www.meilisearch.com/docs",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "Url", Label = "Meilisearch URL", Type = "url", Required = true, Placeholder = "http://localhost:7700" },
                new() { Key = "ApiKey", Label = "Master API Key", Type = "password", Required = true, HelpText = "Set via MEILI_MASTER_KEY" },
                new() { Key = "IndexPrefix", Label = "Index Prefix", Type = "text", Required = false, DefaultValue = "crm_" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Search",
            ProviderType = ProviderTypes.Search.Algolia,
            DisplayName = "Algolia",
            Description = "SaaS search platform with advanced ranking and analytics.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://www.algolia.com/doc/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "AppId", Label = "Application ID", Type = "text", Required = true },
                new() { Key = "ApiKey", Label = "Admin API Key", Type = "password", Required = true },
                new() { Key = "IndexPrefix", Label = "Index Prefix", Type = "text", Required = false, DefaultValue = "crm_" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Search",
            ProviderType = ProviderTypes.Search.Elasticsearch,
            DisplayName = "Elasticsearch",
            Description = "Distributed search and analytics engine. Self-hostable or via Elastic Cloud.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://www.elastic.co/guide/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "Url", Label = "Elasticsearch URL", Type = "url", Required = true, Placeholder = "http://localhost:9200" },
                new() { Key = "Username", Label = "Username", Type = "text", Required = false },
                new() { Key = "Password", Label = "Password", Type = "password", Required = false }
            }
        });

        // ── Chat ─────────────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Chat",
            ProviderType = ProviderTypes.Chat.BuiltIn,
            DisplayName = "Built-In Messaging",
            Description = "Basic activity-based messaging stored in the CRM database.",
            IsBuiltIn = true,
            IsSaaS = false
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Chat",
            ProviderType = ProviderTypes.Chat.Chatwoot,
            DisplayName = "Chatwoot",
            Description = "Open-source customer engagement platform. Self-hostable.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://www.chatwoot.com/docs/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "BaseUrl", Label = "Chatwoot URL", Type = "url", Required = true, Placeholder = "http://localhost:3000" },
                new() { Key = "ApiKey", Label = "API Access Token", Type = "password", Required = true },
                new() { Key = "AccountId", Label = "Account ID", Type = "number", Required = true },
                new() { Key = "InboxId", Label = "Inbox ID", Type = "number", Required = false }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Chat",
            ProviderType = ProviderTypes.Chat.Intercom,
            DisplayName = "Intercom",
            Description = "SaaS customer messaging and engagement platform.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://developers.intercom.com/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "AccessToken", Label = "Access Token", Type = "password", Required = true },
                new() { Key = "AppId", Label = "App ID", Type = "text", Required = true }
            }
        });

        // ── Notifications ────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Notifications",
            ProviderType = ProviderTypes.Notifications.BuiltIn,
            DisplayName = "Built-In SMTP",
            Description = "Email notifications via configured SMTP server.",
            IsBuiltIn = true,
            IsSaaS = false
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Notifications",
            ProviderType = ProviderTypes.Notifications.Novu,
            DisplayName = "Novu",
            Description = "Open-source multi-channel notification infrastructure. Self-hostable.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://docs.novu.co/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "ApplicationId", Label = "Application ID", Type = "text", Required = true },
                new() { Key = "BaseUrl", Label = "Novu API URL", Type = "url", Required = false, DefaultValue = "https://api.novu.co" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Notifications",
            ProviderType = ProviderTypes.Notifications.SendGrid,
            DisplayName = "SendGrid",
            Description = "SaaS email delivery and marketing platform by Twilio.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://docs.sendgrid.com/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "FromEmail", Label = "From Email", Type = "text", Required = true },
                new() { Key = "FromName", Label = "From Name", Type = "text", Required = false }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Notifications",
            ProviderType = ProviderTypes.Notifications.Twilio,
            DisplayName = "Twilio",
            Description = "SaaS communications platform (SMS, Voice, Email).",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://www.twilio.com/docs",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "AccountSid", Label = "Account SID", Type = "text", Required = true },
                new() { Key = "AuthToken", Label = "Auth Token", Type = "password", Required = true },
                new() { Key = "FromNumber", Label = "From Phone Number", Type = "text", Required = true, Placeholder = "+1234567890" }
            }
        });

        // ── Analytics ────────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Analytics",
            ProviderType = ProviderTypes.Analytics.BuiltIn,
            DisplayName = "Built-In Reports",
            Description = "CRM built-in reports and dashboards. No external dependency.",
            IsBuiltIn = true,
            IsSaaS = false
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Analytics",
            ProviderType = ProviderTypes.Analytics.Superset,
            DisplayName = "Apache Superset",
            Description = "Open-source BI and data visualization platform. Self-hostable.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://superset.apache.org/docs/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "Url", Label = "Superset URL", Type = "url", Required = true, Placeholder = "http://localhost:8088" },
                new() { Key = "Username", Label = "Username", Type = "text", Required = true, DefaultValue = "admin" },
                new() { Key = "Password", Label = "Password", Type = "password", Required = true },
                new() { Key = "DatabaseId", Label = "Database ID", Type = "number", Required = false, DefaultValue = "1" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Analytics",
            ProviderType = ProviderTypes.Analytics.PowerBI,
            DisplayName = "Microsoft Power BI",
            Description = "SaaS business intelligence and visualization platform by Microsoft.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://learn.microsoft.com/en-us/power-bi/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "WorkspaceId", Label = "Workspace ID", Type = "text", Required = true },
                new() { Key = "ClientId", Label = "Client ID", Type = "text", Required = true },
                new() { Key = "ClientSecret", Label = "Client Secret", Type = "password", Required = true },
                new() { Key = "TenantId", Label = "Tenant ID", Type = "text", Required = true }
            }
        });

        // ── Signatures ───────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Signatures",
            ProviderType = ProviderTypes.Signatures.BuiltIn,
            DisplayName = "Built-In (No-Op)",
            Description = "Stub signature provider. Documents are marked signed without external workflow.",
            IsBuiltIn = true,
            IsSaaS = false
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Signatures",
            ProviderType = ProviderTypes.Signatures.DocuSeal,
            DisplayName = "DocuSeal",
            Description = "Open-source document signing platform. Self-hostable.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://www.docuseal.com/docs",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "Url", Label = "DocuSeal URL", Type = "url", Required = true, Placeholder = "http://localhost:3000" },
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "WebhookSecret", Label = "Webhook Secret", Type = "password", Required = false }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Signatures",
            ProviderType = ProviderTypes.Signatures.DocuSign,
            DisplayName = "DocuSign",
            Description = "Leading SaaS e-signature platform.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://developers.docusign.com/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "AccountId", Label = "Account ID", Type = "text", Required = true },
                new() { Key = "ClientId", Label = "Integration Key (Client ID)", Type = "text", Required = true },
                new() { Key = "ClientSecret", Label = "Client Secret", Type = "password", Required = true },
                new() { Key = "BaseUrl", Label = "Base URL", Type = "url", Required = false, DefaultValue = "https://demo.docusign.net/restapi" }
            }
        });

        // ── AI ───────────────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "AI",
            ProviderType = ProviderTypes.AI.Ollama,
            DisplayName = "Ollama (Local LLM)",
            Description = "Run large language models locally. No cloud dependency.",
            IsBuiltIn = false, // AI has no 'BuiltIn' type — Ollama is the default
            IsSaaS = false,
            DocumentationUrl = "https://ollama.com/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "Url", Label = "Ollama URL", Type = "url", Required = true, DefaultValue = "http://localhost:11434" },
                new() { Key = "Model", Label = "Default Model", Type = "text", Required = true, DefaultValue = "llama3.1:8b" },
                new() { Key = "EmbeddingModel", Label = "Embedding Model", Type = "text", Required = false, DefaultValue = "nomic-embed-text" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "AI",
            ProviderType = ProviderTypes.AI.OpenAI,
            DisplayName = "OpenAI",
            Description = "GPT-4o and other OpenAI models via API.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://platform.openai.com/docs/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "Model", Label = "Model", Type = "text", Required = true, DefaultValue = "gpt-4o" },
                new() { Key = "OrganizationId", Label = "Organization ID", Type = "text", Required = false }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "AI",
            ProviderType = ProviderTypes.AI.AzureOpenAI,
            DisplayName = "Azure OpenAI",
            Description = "Azure-hosted OpenAI models with enterprise compliance.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://learn.microsoft.com/en-us/azure/ai-services/openai/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "Endpoint", Label = "Azure Endpoint", Type = "url", Required = true, Placeholder = "https://xxx.openai.azure.com/" },
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "DeploymentName", Label = "Deployment Name", Type = "text", Required = true, DefaultValue = "gpt-4o" },
                new() { Key = "ApiVersion", Label = "API Version", Type = "text", Required = false, DefaultValue = "2024-02-01" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "AI",
            ProviderType = ProviderTypes.AI.Anthropic,
            DisplayName = "Anthropic (Claude)",
            Description = "Claude models (Sonnet, Haiku, Opus) via Anthropic API.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://docs.anthropic.com/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "Model", Label = "Model", Type = "text", Required = true, DefaultValue = "claude-3-sonnet-20240229" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "AI",
            ProviderType = ProviderTypes.AI.Gemini,
            DisplayName = "Google Gemini",
            Description = "Google Gemini Pro/Ultra models via Google AI API.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://ai.google.dev/docs",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "Model", Label = "Model", Type = "text", Required = true, DefaultValue = "gemini-1.5-pro" }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "AI",
            ProviderType = ProviderTypes.AI.OpenRouter,
            DisplayName = "OpenRouter",
            Description = "Unified gateway to 100+ AI models from multiple providers.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://openrouter.ai/docs",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "Model", Label = "Default Model", Type = "text", Required = true, DefaultValue = "openai/gpt-4o" }
            }
        });

        // ── Integrations ─────────────────────────────────────────────────────────
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Integrations",
            ProviderType = ProviderTypes.Integrations.BuiltIn,
            DisplayName = "Built-In Webhooks",
            Description = "Native webhook support. Sends HTTP POST events to configured endpoints.",
            IsBuiltIn = true,
            IsSaaS = false
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Integrations",
            ProviderType = ProviderTypes.Integrations.N8n,
            DisplayName = "n8n",
            Description = "Open-source workflow automation platform (400+ integrations). Self-hostable.",
            IsBuiltIn = false,
            IsSaaS = false,
            DocumentationUrl = "https://docs.n8n.io/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "BaseUrl", Label = "n8n URL", Type = "url", Required = true, Placeholder = "http://localhost:5678" },
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "WebhookBaseUrl", Label = "Webhook Base URL", Type = "url", Required = false }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Integrations",
            ProviderType = ProviderTypes.Integrations.Zapier,
            DisplayName = "Zapier",
            Description = "SaaS automation platform connecting 5,000+ apps.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://zapier.com/developer/documentation/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "WebhookUrl", Label = "Zap Webhook URL", Type = "url", Required = true },
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = false }
            }
        });
        entries.Add(new ProviderRegistryEntry
        {
            Category = "Integrations",
            ProviderType = ProviderTypes.Integrations.Make,
            DisplayName = "Make (Integromat)",
            Description = "SaaS visual automation platform for complex workflows.",
            IsBuiltIn = false,
            IsSaaS = true,
            DocumentationUrl = "https://www.make.com/en/help/",
            ConfigFields = new List<ProviderConfigField>
            {
                new() { Key = "ApiKey", Label = "API Key", Type = "password", Required = true },
                new() { Key = "TeamId", Label = "Team ID", Type = "text", Required = true }
            }
        });

        return entries;
    }
}

/// <summary>
/// Extension to make a shallow clone of a ProviderRegistryEntry.
/// </summary>
internal static class ProviderRegistryEntryExtensions
{
    internal static ProviderRegistryEntry ShallowClone(this ProviderRegistryEntry entry)
        => new()
        {
            Category = entry.Category,
            ProviderType = entry.ProviderType,
            DisplayName = entry.DisplayName,
            Description = entry.Description,
            IsActive = entry.IsActive,
            IsConfigured = entry.IsConfigured,
            IsBuiltIn = entry.IsBuiltIn,
            IsSaaS = entry.IsSaaS,
            DocumentationUrl = entry.DocumentationUrl,
            ConfigFields = entry.ConfigFields, // shared reference — read-only at this level
            HealthStatus = entry.HealthStatus
        };
}
