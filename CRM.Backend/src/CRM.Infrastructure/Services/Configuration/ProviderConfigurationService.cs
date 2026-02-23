// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Configuration;

/// <summary>
/// Core service for managing provider configurations in the database.
/// Handles encryption/decryption of sensitive data and audit trail creation.
/// </summary>
public class ProviderConfigurationService : IProviderConfigurationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<ProviderConfigurationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ProviderConfigurationService(
        ICrmDbContext dbContext,
        IEncryptionService encryption,
        ILogger<ProviderConfigurationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _encryption = encryption;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<ProviderConfigurationDto?> GetConfigurationAsync(
        string configKey,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProviderConfigurations
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .Include(p => p.UpdatedByUser)
            .Where(p => p.ConfigurationKey == configKey && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return null;

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<List<ProviderConfigurationDto>> GetAllConfigurationsAsync(
        string? configurationType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProviderConfigurations
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .Include(p => p.UpdatedByUser)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(configurationType))
        {
            query = query.Where(p => p.ConfigurationType == configurationType);
        }

        var entities = await query
            .OrderBy(p => p.ConfigurationType)
            .ThenBy(p => p.ConfigurationKey)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ProviderConfigurationDto> UpdateConfigurationAsync(
        string configKey,
        Dictionary<string, object> configData,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProviderConfigurations
            .Where(p => p.ConfigurationKey == configKey && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        var jsonData = JsonSerializer.Serialize(configData, JsonOptions);
        var isNewRecord = entity == null;
        string? oldValue = null;

        if (entity != null)
        {
            // Capture old value for audit log
            oldValue = entity.IsEncrypted
                ? _encryption.Decrypt(entity.ConfigurationData)
                : entity.ConfigurationData;

            entity.ConfigurationData = _encryption.Encrypt(jsonData);
            entity.IsEncrypted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedByUserId = userId;
        }
        else
        {
            // Determine configuration type from key prefix
            var configType = configKey.StartsWith("system.", StringComparison.OrdinalIgnoreCase)
                ? "system"
                : "crm";

            // Extract provider name from key (e.g., "crm.ai.openai" -> "openai")
            var keyParts = configKey.Split('.');
            var providerName = keyParts.Length >= 3 ? keyParts[^1] : null;

            entity = new ProviderConfiguration
            {
                ConfigurationKey = configKey,
                ConfigurationType = configType,
                ProviderName = providerName,
                ConfigurationData = _encryption.Encrypt(jsonData),
                IsEncrypted = true,
                IsActive = true,
                CanBeDisabledAtRuntime = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                UpdatedByUserId = userId
            };

            _dbContext.ProviderConfigurations.Add(entity);
        }

        // Create change log entry
        var changeLog = new ConfigurationChangeLog
        {
            ConfigurationKey = configKey,
            OldValue = oldValue != null ? _encryption.Encrypt(oldValue) : null,
            NewValue = _encryption.Encrypt(jsonData),
            ChangeType = isNewRecord ? "created" : "updated",
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = userId,
            ProviderConfigurationId = entity.Id > 0 ? entity.Id : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ConfigurationChangeLogs.Add(changeLog);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Re-link change log to entity if it was newly created
        if (isNewRecord && changeLog.ProviderConfigurationId == null)
        {
            changeLog.ProviderConfigurationId = entity.Id;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Configuration {ConfigKey} {Action} by user {UserId}",
            configKey, isNewRecord ? "created" : "updated", userId);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<ConfigurationTestResultDto> TestConfigurationAsync(
        string providerType,
        string provider,
        Dictionary<string, object> configData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Testing configuration for provider type={ProviderType}, provider={Provider}",
            providerType, provider);

        try
        {
            return providerType.ToLowerInvariant() switch
            {
                "email" or "smtp" => await TestSmtpConnectionAsync(configData, cancellationToken),
                "ai" => await TestHttpEndpointAsync(configData, GetAITestUrl(provider, configData), cancellationToken),
                "search" => await TestHttpEndpointAsync(configData, GetSearchTestUrl(provider, configData), cancellationToken),
                "chat" => await TestHttpEndpointAsync(configData, GetGenericHealthUrl(configData), cancellationToken),
                "notifications" => await TestHttpEndpointAsync(configData, GetGenericHealthUrl(configData), cancellationToken),
                "analytics" => await TestHttpEndpointAsync(configData, GetGenericHealthUrl(configData), cancellationToken),
                "signatures" => await TestHttpEndpointAsync(configData, GetGenericHealthUrl(configData), cancellationToken),
                "workflows" => await TestHttpEndpointAsync(configData, GetGenericHealthUrl(configData), cancellationToken),
                _ => new ConfigurationTestResultDto
                {
                    Success = false,
                    Message = $"Unknown provider type: {providerType}",
                    TestedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Configuration test failed for {ProviderType}/{Provider}", providerType, provider);
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = "Configuration test failed",
                ErrorDetails = ex.Message,
                TestedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public Task<List<ProviderInfoDto>> GetAvailableProvidersAsync(
        string type,
        CancellationToken cancellationToken = default)
    {
        var providers = type.ToLowerInvariant() switch
        {
            "ai" => new List<ProviderInfoDto>
            {
                new() { Id = "ollama", Name = "Ollama (Local)", Description = "Local LLM inference", RequiredFields = new() { "apiUrl", "model" }, IsBuiltIn = false },
                new() { Id = "openai", Name = "OpenAI", Description = "GPT models via OpenAI API", RequiredFields = new() { "apiKey", "model" }, IsSaaS = true },
                new() { Id = "azure", Name = "Azure OpenAI", Description = "GPT models via Azure", RequiredFields = new() { "apiKey", "apiUrl", "model" }, IsSaaS = true },
                new() { Id = "anthropic", Name = "Anthropic", Description = "Claude models", RequiredFields = new() { "apiKey", "model" }, IsSaaS = true },
                new() { Id = "bedrock", Name = "AWS Bedrock", Description = "AWS-managed AI models", RequiredFields = new() { "apiKey", "model" }, IsSaaS = true },
                new() { Id = "openrouter", Name = "OpenRouter", Description = "Multi-model routing", RequiredFields = new() { "apiKey", "model" }, IsSaaS = true },
                new() { Id = "gemini", Name = "Google Gemini", Description = "Google AI models", RequiredFields = new() { "apiKey", "model" }, IsSaaS = true }
            },
            "search" => new List<ProviderInfoDto>
            {
                new() { Id = "builtin", Name = "Built-In Search", Description = "Database full-text search", IsBuiltIn = true },
                new() { Id = "meilisearch", Name = "Meilisearch", Description = "Fast full-text search engine", RequiredFields = new() { "url", "apiKey" } },
                new() { Id = "algolia", Name = "Algolia", Description = "Hosted search-as-a-service", RequiredFields = new() { "applicationId", "apiKey" }, IsSaaS = true },
                new() { Id = "elasticsearch", Name = "Elasticsearch", Description = "Distributed search engine", RequiredFields = new() { "url" } },
                new() { Id = "typesense", Name = "Typesense", Description = "Open-source search engine", RequiredFields = new() { "url", "apiKey" } }
            },
            "chat" => new List<ProviderInfoDto>
            {
                new() { Id = "builtin", Name = "Built-In Chat", Description = "Internal messaging system", IsBuiltIn = true },
                new() { Id = "chatwoot", Name = "Chatwoot", Description = "Open-source customer chat", RequiredFields = new() { "baseUrl", "apiKey", "accountId" } },
                new() { Id = "intercom", Name = "Intercom", Description = "Customer messaging platform", RequiredFields = new() { "apiKey" }, IsSaaS = true }
            },
            "notifications" => new List<ProviderInfoDto>
            {
                new() { Id = "builtin", Name = "Built-In Notifications", Description = "In-app notifications", IsBuiltIn = true },
                new() { Id = "novu", Name = "Novu", Description = "Open-source notification infrastructure", RequiredFields = new() { "apiKey" } },
                new() { Id = "twilio", Name = "Twilio", Description = "SMS and voice notifications", RequiredFields = new() { "accountSid", "authToken" }, IsSaaS = true },
                new() { Id = "sendgrid", Name = "SendGrid", Description = "Email delivery service", RequiredFields = new() { "apiKey" }, IsSaaS = true }
            },
            "analytics" => new List<ProviderInfoDto>
            {
                new() { Id = "builtin", Name = "Built-In Analytics", Description = "Basic CRM reporting", IsBuiltIn = true },
                new() { Id = "superset", Name = "Apache Superset", Description = "Open-source BI platform", RequiredFields = new() { "url", "username", "password" } },
                new() { Id = "metabase", Name = "Metabase", Description = "Open-source analytics", RequiredFields = new() { "url" } },
                new() { Id = "powerbi", Name = "Power BI", Description = "Microsoft BI platform", RequiredFields = new() { "tenantId", "clientId", "clientSecret" }, IsSaaS = true }
            },
            "signatures" => new List<ProviderInfoDto>
            {
                new() { Id = "builtin", Name = "Built-In Signatures", Description = "Basic signature capture", IsBuiltIn = true },
                new() { Id = "docuseal", Name = "DocuSeal", Description = "Open-source e-signatures", RequiredFields = new() { "url", "apiKey" } },
                new() { Id = "docusign", Name = "DocuSign", Description = "Enterprise e-signatures", RequiredFields = new() { "integrationKey", "apiAccountId" }, IsSaaS = true }
            },
            "workflows" or "integrations" => new List<ProviderInfoDto>
            {
                new() { Id = "builtin", Name = "Built-In Workflows", Description = "Basic automation engine", IsBuiltIn = true },
                new() { Id = "n8n", Name = "n8n", Description = "Open-source workflow automation", RequiredFields = new() { "baseUrl", "apiKey" } },
                new() { Id = "zapier", Name = "Zapier", Description = "Cloud automation platform", RequiredFields = new() { "apiKey" }, IsSaaS = true }
            },
            _ => new List<ProviderInfoDto>()
        };

        return Task.FromResult(providers);
    }

    /// <inheritdoc />
    public async Task<List<ConfigurationChangeLogDto>> GetChangeHistoryAsync(
        string? configKey = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ConfigurationChangeLogs
            .AsNoTracking()
            .Include(c => c.ChangedByUser)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(configKey))
        {
            query = query.Where(c => c.ConfigurationKey == configKey);
        }

        var logs = await query
            .OrderByDescending(c => c.ChangedAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new ConfigurationChangeLogDto
        {
            Id = l.Id,
            ConfigurationKey = l.ConfigurationKey,
            OldValue = l.OldValue != null && _encryption.IsEncrypted(l.OldValue)
                ? "[encrypted]"
                : l.OldValue,
            NewValue = l.NewValue != null && _encryption.IsEncrypted(l.NewValue)
                ? "[encrypted]"
                : l.NewValue,
            ChangeType = l.ChangeType,
            ChangedAt = l.ChangedAt,
            ChangedByUserName = l.ChangedByUser?.Username,
            IpAddress = l.IpAddress
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<ConfigurationTestResultDto> RollbackConfigurationAsync(
        int changeLogId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var changeLog = await _dbContext.ConfigurationChangeLogs
            .AsNoTracking()
            .Where(c => c.Id == changeLogId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (changeLog == null)
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"Change log entry {changeLogId} not found",
                TestedAt = DateTime.UtcNow
            };
        }

        if (string.IsNullOrEmpty(changeLog.OldValue))
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = "Cannot rollback: no previous value exists (this was a creation event)",
                TestedAt = DateTime.UtcNow
            };
        }

        try
        {
            // Decrypt old value to get original config data
            var oldValueDecrypted = _encryption.IsEncrypted(changeLog.OldValue)
                ? _encryption.Decrypt(changeLog.OldValue)
                : changeLog.OldValue;

            if (oldValueDecrypted == null)
            {
                return new ConfigurationTestResultDto
                {
                    Success = false,
                    Message = "Cannot rollback: failed to decrypt previous configuration value",
                    TestedAt = DateTime.UtcNow
                };
            }

            var configData = JsonSerializer.Deserialize<Dictionary<string, object>>(oldValueDecrypted, JsonOptions)
                ?? new Dictionary<string, object>();

            await UpdateConfigurationAsync(changeLog.ConfigurationKey, configData, userId, cancellationToken);

            _logger.LogInformation(
                "Configuration {ConfigKey} rolled back to change log {ChangeLogId} by user {UserId}",
                changeLog.ConfigurationKey, changeLogId, userId);

            return new ConfigurationTestResultDto
            {
                Success = true,
                Message = $"Configuration '{changeLog.ConfigurationKey}' rolled back successfully",
                TestedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed for change log {ChangeLogId}", changeLogId);
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = "Rollback failed",
                ErrorDetails = ex.Message,
                TestedAt = DateTime.UtcNow
            };
        }
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private ProviderConfigurationDto MapToDto(ProviderConfiguration entity)
    {
        var decryptedData = entity.IsEncrypted
            ? (_encryption.Decrypt(entity.ConfigurationData) ?? entity.ConfigurationData)
            : entity.ConfigurationData;

        return new ProviderConfigurationDto
        {
            Id = entity.Id,
            ConfigurationKey = entity.ConfigurationKey,
            ConfigurationType = entity.ConfigurationType,
            ProviderName = entity.ProviderName,
            ConfigurationData = decryptedData,
            IsEncrypted = entity.IsEncrypted,
            IsActive = entity.IsActive,
            CanBeDisabledAtRuntime = entity.CanBeDisabledAtRuntime,
            LastTestedAt = entity.LastTestedAt,
            LastTestedStatus = entity.LastTestedStatus,
            LastTestedError = entity.LastTestedError,
            CreatedAt = entity.CreatedAt,
            CreatedByUserName = entity.CreatedByUser?.Username,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
            UpdatedByUserName = entity.UpdatedByUser?.Username
        };
    }

    private async Task<ConfigurationTestResultDto> TestSmtpConnectionAsync(
        Dictionary<string, object> configData,
        CancellationToken cancellationToken)
    {
        var server = GetStringValue(configData, "smtpServer");
        var port = GetIntValue(configData, "smtpPort", 587);

        if (string.IsNullOrEmpty(server))
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = "SMTP server address is required",
                TestedAt = DateTime.UtcNow
            };
        }

        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(server, port);
            var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));

            if (completed != connectTask || !client.Connected)
            {
                return new ConfigurationTestResultDto
                {
                    Success = false,
                    Message = $"Connection to {server}:{port} timed out",
                    TestedAt = DateTime.UtcNow
                };
            }

            return new ConfigurationTestResultDto
            {
                Success = true,
                Message = $"Successfully connected to SMTP server {server}:{port}",
                TestedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"SMTP connection failed: {ex.Message}",
                ErrorDetails = ex.ToString(),
                TestedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<ConfigurationTestResultDto> TestHttpEndpointAsync(
        Dictionary<string, object> configData,
        string? testUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(testUrl))
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = "No test URL could be determined from the configuration",
                TestedAt = DateTime.UtcNow
            };
        }

        try
        {
            using var client = _httpClientFactory.CreateClient("ConfigurationTest");
            client.Timeout = TimeSpan.FromSeconds(15);

            // Add API key header if present
            var apiKey = GetStringValue(configData, "apiKey");
            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
            }

            var response = await client.GetAsync(testUrl, cancellationToken);

            return new ConfigurationTestResultDto
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode
                    ? $"Successfully connected to {new Uri(testUrl).Host}"
                    : $"Received HTTP {(int)response.StatusCode} from {new Uri(testUrl).Host}",
                TestedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"Connection failed: {ex.Message}",
                ErrorDetails = ex.ToString(),
                TestedAt = DateTime.UtcNow
            };
        }
    }

    private static string? GetAITestUrl(string provider, Dictionary<string, object> configData)
    {
        var apiUrl = GetStringValue(configData, "apiUrl");

        return provider.ToLowerInvariant() switch
        {
            "openai" => "https://api.openai.com/v1/models",
            "anthropic" => "https://api.anthropic.com/v1/models",
            "ollama" => !string.IsNullOrEmpty(apiUrl) ? $"{apiUrl.TrimEnd('/')}/api/tags" : null,
            "azure" => !string.IsNullOrEmpty(apiUrl) ? $"{apiUrl.TrimEnd('/')}/openai/models?api-version=2024-02-01" : null,
            "openrouter" => "https://openrouter.ai/api/v1/models",
            "gemini" => "https://generativelanguage.googleapis.com/v1beta/models",
            _ => apiUrl != null ? $"{apiUrl.TrimEnd('/')}/health" : null
        };
    }

    private static string? GetSearchTestUrl(string provider, Dictionary<string, object> configData)
    {
        var url = GetStringValue(configData, "url");

        return provider.ToLowerInvariant() switch
        {
            "meilisearch" => !string.IsNullOrEmpty(url) ? $"{url.TrimEnd('/')}/health" : null,
            "elasticsearch" => !string.IsNullOrEmpty(url) ? $"{url.TrimEnd('/')}/_cluster/health" : null,
            "typesense" => !string.IsNullOrEmpty(url) ? $"{url.TrimEnd('/')}/health" : null,
            _ => url != null ? $"{url.TrimEnd('/')}/health" : null
        };
    }

    private static string? GetGenericHealthUrl(Dictionary<string, object> configData)
    {
        var baseUrl = GetStringValue(configData, "baseUrl")
            ?? GetStringValue(configData, "url");

        return !string.IsNullOrEmpty(baseUrl) ? $"{baseUrl.TrimEnd('/')}/health" : null;
    }

    private static string? GetStringValue(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out var value))
        {
            return value?.ToString();
        }

        return null;
    }

    private static int GetIntValue(Dictionary<string, object> data, string key, int defaultValue = 0)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (value is JsonElement je && je.TryGetInt32(out var intVal))
                return intVal;
            if (int.TryParse(value?.ToString(), out var parsed))
                return parsed;
        }

        return defaultValue;
    }
}
