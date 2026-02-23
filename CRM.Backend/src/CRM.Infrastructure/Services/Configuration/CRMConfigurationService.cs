// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Ports;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Configuration;

/// <summary>
/// Manages CRM-specific configurations: AI providers, integrations, workers, and agents.
/// Delegates persistence to <see cref="IProviderConfigurationService"/>.
/// </summary>
public class CRMConfigurationService : ICRMConfigurationService
{
    // Well-known configuration key patterns
    private const string AIProviderKeyPrefix = "crm.ai.";
    private const string IntegrationKeyPrefix = "crm.integration.";
    private const string WorkerConfigKey = "crm.worker.config";
    private const string AgentsConfigKey = "crm.agents.config";

    private readonly IProviderConfigurationService _providerConfig;
    private readonly ILogger<CRMConfigurationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Known AI provider identifiers used for key enumeration.
    /// </summary>
    private static readonly string[] KnownAIProviders =
        { "ollama", "openai", "azure", "anthropic", "bedrock", "openrouter", "gemini" };

    /// <summary>
    /// Known integration type/provider combinations.
    /// </summary>
    private static readonly (string Type, string Provider)[] KnownIntegrations =
    {
        ("search", "meilisearch"), ("search", "algolia"), ("search", "elasticsearch"), ("search", "typesense"),
        ("chat", "chatwoot"), ("chat", "intercom"),
        ("notifications", "novu"), ("notifications", "twilio"), ("notifications", "sendgrid"),
        ("analytics", "superset"), ("analytics", "metabase"), ("analytics", "powerbi"),
        ("signatures", "docuseal"), ("signatures", "docusign"),
        ("workflows", "n8n"), ("workflows", "zapier")
    };

    public CRMConfigurationService(
        IProviderConfigurationService providerConfig,
        ILogger<CRMConfigurationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _providerConfig = providerConfig;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<CRMConfigResponseDto> GetCRMConfigAsync(
        CancellationToken cancellationToken = default)
    {
        var response = new CRMConfigResponseDto();
        DateTime latestUpdate = DateTime.MinValue;
        string? latestUpdater = null;

        // Load AI provider configs
        foreach (var provider in KnownAIProviders)
        {
            var config = await _providerConfig.GetConfigurationAsync(
                $"{AIProviderKeyPrefix}{provider}", cancellationToken);

            if (config != null)
            {
                var dto = DeserializeDto<AIProviderConfigDto>(config.ConfigurationData);
                if (dto != null)
                {
                    dto.Provider = provider;
                    dto.LastTested = config.LastTestedAt;
                    dto.ConnectionStatus = config.LastTestedStatus;
                    dto.TestError = config.LastTestedError;
                    response.AIProviders.Add(dto);
                }

                TrackLatestUpdate(config.UpdatedAt, config.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
            }
        }

        // Load integration configs
        foreach (var (type, provider) in KnownIntegrations)
        {
            var config = await _providerConfig.GetConfigurationAsync(
                $"{IntegrationKeyPrefix}{type}.{provider}", cancellationToken);

            if (config != null)
            {
                var dto = DeserializeDto<IntegrationConfigDto>(config.ConfigurationData);
                if (dto != null)
                {
                    dto.Type = type;
                    dto.Provider = provider;
                    dto.LastTested = config.LastTestedAt;
                    dto.ConnectionStatus = config.LastTestedStatus;
                    dto.TestError = config.LastTestedError;
                    response.Integrations.Add(dto);
                }

                TrackLatestUpdate(config.UpdatedAt, config.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
            }
        }

        // Load worker config
        var workerConfig = await _providerConfig.GetConfigurationAsync(WorkerConfigKey, cancellationToken);
        if (workerConfig != null)
        {
            response.WorkerConfig = DeserializeDto<WorkerConfigDto>(workerConfig.ConfigurationData);
            TrackLatestUpdate(workerConfig.UpdatedAt, workerConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        // Load agents config
        var agentsConfig = await _providerConfig.GetConfigurationAsync(AgentsConfigKey, cancellationToken);
        if (agentsConfig != null)
        {
            var agents = DeserializeDto<List<AIAgentConfigDto>>(agentsConfig.ConfigurationData);
            if (agents != null)
            {
                response.AIAgents = agents;
            }

            TrackLatestUpdate(agentsConfig.UpdatedAt, agentsConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        response.LastUpdated = latestUpdate == DateTime.MinValue ? DateTime.UtcNow : latestUpdate;
        response.UpdatedBy = latestUpdater;

        return response;
    }

    /// <inheritdoc />
    public async Task UpdateAIProviderAsync(
        string provider,
        AIProviderConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var configKey = $"{AIProviderKeyPrefix}{provider.ToLowerInvariant()}";
        var data = DtoToDictionary(config);

        await _providerConfig.UpdateConfigurationAsync(configKey, data, userId, cancellationToken);
        _logger.LogInformation("AI provider {Provider} configuration updated by user {UserId}", provider, userId);
    }

    /// <inheritdoc />
    public async Task UpdateIntegrationAsync(
        string type,
        string provider,
        IntegrationConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var configKey = $"{IntegrationKeyPrefix}{type.ToLowerInvariant()}.{provider.ToLowerInvariant()}";
        var data = DtoToDictionary(config);

        await _providerConfig.UpdateConfigurationAsync(configKey, data, userId, cancellationToken);
        _logger.LogInformation(
            "Integration {Type}/{Provider} configuration updated by user {UserId}",
            type, provider, userId);
    }

    /// <inheritdoc />
    public async Task UpdateWorkerConfigAsync(
        WorkerConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var data = DtoToDictionary(config);

        await _providerConfig.UpdateConfigurationAsync(WorkerConfigKey, data, userId, cancellationToken);
        _logger.LogInformation("Worker configuration updated by user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task UpdateAIAgentsAsync(
        List<AIAgentConfigDto> agents,
        int userId,
        CancellationToken cancellationToken = default)
    {
        // Store the full list as one configuration entry
        var json = JsonSerializer.Serialize(agents, JsonOptions);
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(
            $$"""{"agents":{{json}}}""", JsonOptions) ?? new Dictionary<string, object>();

        // Store as a simple wrapper so the Dictionary<string, object> contract is met
        var agentsData = new Dictionary<string, object>
        {
            ["agents"] = JsonSerializer.SerializeToElement(agents, JsonOptions)
        };

        await _providerConfig.UpdateConfigurationAsync(AgentsConfigKey, agentsData, userId, cancellationToken);
        _logger.LogInformation("AI agents configuration ({Count} agents) updated by user {UserId}", agents.Count, userId);
    }

    /// <inheritdoc />
    public async Task<ConfigurationTestResultDto> TestAIProviderAsync(
        string provider,
        AIProviderConfigDto config,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing AI provider {Provider}", provider);

        var testUrl = GetAIProviderTestUrl(provider, config);

        if (string.IsNullOrEmpty(testUrl))
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"Cannot determine test URL for AI provider '{provider}'",
                TestedAt = DateTime.UtcNow
            };
        }

        try
        {
            using var client = _httpClientFactory.CreateClient("ConfigurationTest");
            client.Timeout = TimeSpan.FromSeconds(15);

            // Add authentication headers based on provider
            AddAIAuthHeaders(client, provider, config);

            var response = await client.GetAsync(testUrl, cancellationToken);

            // Update test status on the stored config (fire-and-forget; errors handled internally)
            await UpdateTestStatusAsync(
                $"{AIProviderKeyPrefix}{provider.ToLowerInvariant()}",
                response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}",
                cancellationToken);

            return new ConfigurationTestResultDto
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode
                    ? $"AI provider '{provider}' is reachable and responding"
                    : $"AI provider '{provider}' returned HTTP {(int)response.StatusCode}",
                TestedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI provider test failed for {Provider}", provider);

            await UpdateTestStatusAsync(
                $"{AIProviderKeyPrefix}{provider.ToLowerInvariant()}",
                false,
                ex.Message,
                cancellationToken);

            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"AI provider '{provider}' connection failed: {ex.Message}",
                ErrorDetails = ex.ToString(),
                TestedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public async Task<ConfigurationTestResultDto> TestIntegrationAsync(
        string type,
        string provider,
        IntegrationConfigDto config,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing integration {Type}/{Provider}", type, provider);

        var testUrl = GetIntegrationTestUrl(type, provider, config);

        if (string.IsNullOrEmpty(testUrl))
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"Cannot determine test URL for integration '{type}/{provider}'",
                TestedAt = DateTime.UtcNow
            };
        }

        try
        {
            using var client = _httpClientFactory.CreateClient("ConfigurationTest");
            client.Timeout = TimeSpan.FromSeconds(15);

            // Add auth if credentials present
            if (config.Credentials?.TryGetValue("apiKey", out var apiKey) == true && !string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
            }

            var response = await client.GetAsync(testUrl, cancellationToken);

            var configKey = $"{IntegrationKeyPrefix}{type.ToLowerInvariant()}.{provider.ToLowerInvariant()}";
            await UpdateTestStatusAsync(configKey, response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}", cancellationToken);

            return new ConfigurationTestResultDto
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode
                    ? $"Integration '{type}/{provider}' is reachable"
                    : $"Integration '{type}/{provider}' returned HTTP {(int)response.StatusCode}",
                TestedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Integration test failed for {Type}/{Provider}", type, provider);

            var configKey = $"{IntegrationKeyPrefix}{type.ToLowerInvariant()}.{provider.ToLowerInvariant()}";
            await UpdateTestStatusAsync(configKey, false, ex.Message, cancellationToken);

            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"Integration '{type}/{provider}' connection failed: {ex.Message}",
                ErrorDetails = ex.ToString(),
                TestedAt = DateTime.UtcNow
            };
        }
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static string? GetAIProviderTestUrl(string provider, AIProviderConfigDto config)
    {
        return provider.ToLowerInvariant() switch
        {
            "openai" => "https://api.openai.com/v1/models",
            "anthropic" => "https://api.anthropic.com/v1/models",
            "ollama" => !string.IsNullOrEmpty(config.ApiUrl)
                ? $"{config.ApiUrl.TrimEnd('/')}/api/tags"
                : null,
            "azure" => !string.IsNullOrEmpty(config.ApiUrl)
                ? $"{config.ApiUrl.TrimEnd('/')}/openai/models?api-version=2024-02-01"
                : null,
            "openrouter" => "https://openrouter.ai/api/v1/models",
            "gemini" => !string.IsNullOrEmpty(config.ApiKey)
                ? $"https://generativelanguage.googleapis.com/v1beta/models?key={config.ApiKey}"
                : null,
            "bedrock" => !string.IsNullOrEmpty(config.ApiUrl)
                ? $"{config.ApiUrl.TrimEnd('/')}/health"
                : null,
            _ => !string.IsNullOrEmpty(config.ApiUrl)
                ? $"{config.ApiUrl.TrimEnd('/')}/health"
                : null
        };
    }

    private static void AddAIAuthHeaders(HttpClient client, string provider, AIProviderConfigDto config)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
            return;

        switch (provider.ToLowerInvariant())
        {
            case "openai":
            case "openrouter":
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {config.ApiKey}");
                if (!string.IsNullOrEmpty(config.OrganizationId))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("OpenAI-Organization", config.OrganizationId);
                break;
            case "anthropic":
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", config.ApiKey);
                client.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", "2024-01-01");
                break;
            case "azure":
                client.DefaultRequestHeaders.TryAddWithoutValidation("api-key", config.ApiKey);
                break;
            default:
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {config.ApiKey}");
                break;
        }
    }

    private static string? GetIntegrationTestUrl(string type, string provider, IntegrationConfigDto config)
    {
        // Try to get URL from config.Configuration dictionary
        var baseUrl = GetConfigString(config.Configuration, "url")
            ?? GetConfigString(config.Configuration, "baseUrl")
            ?? config.TestEndpoint;

        if (string.IsNullOrEmpty(baseUrl))
            return null;

        var trimmedUrl = baseUrl.TrimEnd('/');

        // Provider-specific health endpoints
        return (type.ToLowerInvariant(), provider.ToLowerInvariant()) switch
        {
            ("search", "meilisearch") => $"{trimmedUrl}/health",
            ("search", "elasticsearch") => $"{trimmedUrl}/_cluster/health",
            ("search", "typesense") => $"{trimmedUrl}/health",
            ("chat", "chatwoot") => $"{trimmedUrl}/auth/sign_in",          // Chatwoot API check
            ("notifications", "novu") => $"{trimmedUrl}/v1/health",
            ("analytics", "superset") => $"{trimmedUrl}/health",
            ("signatures", "docuseal") => $"{trimmedUrl}/api/templates",
            ("workflows", "n8n") => $"{trimmedUrl}/healthz",
            _ => $"{trimmedUrl}/health"
        };
    }

    private static string? GetConfigString(Dictionary<string, object>? config, string key)
    {
        if (config == null || !config.TryGetValue(key, out var value))
            return null;

        return value?.ToString();
    }

    /// <summary>
    /// Updates the test status fields on a stored provider configuration.
    /// Fails silently so callers are not impacted by status tracking errors.
    /// </summary>
    private async Task UpdateTestStatusAsync(
        string configKey,
        bool success,
        string? error,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _providerConfig.GetConfigurationAsync(configKey, cancellationToken);
            if (existing == null)
                return;

            // We store the test status via a lightweight update (re-save current data with unchanged content).
            // The ProviderConfigurationService handles the actual DB write.
            // For now, log the test result; a future enhancement can write directly to the entity.
            _logger.LogDebug(
                "Test status for {ConfigKey}: success={Success}, error={Error}",
                configKey, success, error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update test status for {ConfigKey}", configKey);
        }
    }

    private static T? DeserializeDto<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object> DtoToDictionary<T>(T dto)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions)
            ?? new Dictionary<string, object>();
    }

    private static void TrackLatestUpdate(
        DateTime updatedAt,
        string? updatedBy,
        ref DateTime latestUpdate,
        ref string? latestUpdater)
    {
        if (updatedAt > latestUpdate)
        {
            latestUpdate = updatedAt;
            latestUpdater = updatedBy;
        }
    }
}
