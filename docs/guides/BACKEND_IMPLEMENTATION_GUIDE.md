# Configuration Management System - Backend Implementation Guide

**Version:** 0.625.0  
**Last Updated:** March 2026  
**Status:** Ready for Service Implementation  
**Target:** Complete Phase 2 within 2-3 days  

---

## Overview

This guide provides step-by-step instructions for implementing the three main configuration services and their corresponding API controllers.

---

## Service 1: ProviderConfigurationService

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/ProviderConfigurationService.cs`

### Key Responsibilities
1. Get/List provider configurations
2. Update provider configurations with encryption
3. Test provider connectivity (delegated to provider-specific testers)
4. Maintain audit trail via ConfigurationChangeLog
5. Support rollback to previous states

### Implementation Checklist

```csharp
public class ProviderConfigurationService : IProviderConfigurationService
{
    private readonly ICrmDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<ProviderConfigurationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    // Constructor with DI

    public async Task<ProviderConfigurationDto?> GetConfigurationAsync(string configKey, ...)
    {
        // 1. Query database by ConfigurationKey
        // 2. Decrypt ConfigurationData if IsEncrypted=true
        // 3. Deserialize JSON
        // 4. Map to DTO (mask sensitive fields)
        // 5. Return
    }

    public async Task<List<ProviderConfigurationDto>> GetAllConfigurationsAsync(string? configurationType, ...)
    {
        // 1. Query all configs, optionally filter by ConfigurationType
        // 2. Decrypt all
        // 3. Map to DTOs
        // 4. Return list
    }

    public async Task<ProviderConfigurationDto> UpdateConfigurationAsync(
        string configKey,
        Dictionary<string, object> configData,
        int userId,
        ...)
    {
        // 1. Validate configKey exists or create new
        // 2. Serialize configData to JSON
        // 3. Encrypt JSON using IEncryptionService
        // 4. Save to database
        // 5. Create ConfigurationChangeLog entry
        //    - OldValue = previous encrypted value
        //    - NewValue = new encrypted value
        //    - ChangeType = 'updated' or 'created'
        //    - ChangedByUserId = userId
        // 6. Clear any caches
        // 7. Return updated DTO
    }

    public async Task<ConfigurationTestResultDto> TestConfigurationAsync(
        string providerType,  // 'ai', 'email', 'search', etc.
        string provider,      // 'openai', 'smtp', 'meilisearch', etc.
        Dictionary<string, object> configData,
        ...)
    {
        // Dispatch to provider-specific tester
        // Example logic:
        // switch (providerType)
        // {
        //     case "email":
        //         return await TestEmailServerAsync((EmailServerConfigDto)config);
        //     case "ai":
        //         return await TestAIProviderAsync(provider, (AIProviderConfigDto)config);
        //     case "search":
        //         return await TestSearchProviderAsync(provider, config);
        //     // etc.
        // }
    }

    public async Task<List<ProviderInfoDto>> GetAvailableProvidersAsync(string type, ...)
    {
        // Return hardcoded list of available providers for each type
        // Example:
        // if (type == "ai")
        //     return new List<ProviderInfoDto>
        //     {
        //         new() { Id = "openai", Name = "OpenAI", },
        //         new() { Id = "azure", Name = "Azure OpenAI", },
        //         // etc.
        //     };
    }

    public async Task<List<ConfigurationChangeLogDto>> GetChangeHistoryAsync(
        string? configKey,
        int pageSize,
        ...)
    {
        // 1. Query ConfigurationChangeLogs
        // 2. Filter by configKey if provided
        // 3. Order by ChangedAt DESC
        // 4. Take top pageSize
        // 5. Map to DTOs (include UserName from join with Users table)
        // 6. Return
    }

    private async Task<ConfigurationTestResultDto> TestEmailServerAsync(EmailServerConfigDto config, ...)
    {
        // Use SMTP client to test connectivity
        // 1. Create SmtpClient with settings from config
        // 2. Try to connect and authenticate
        // 3. Send test email to admin or provided address
        // 4. Return success/error result
    }

    private async Task<ConfigurationTestResultDto> TestAIProviderAsync(string provider, AIProviderConfigDto config, ...)
    {
        // Call provider's API with config to test
        // Example for OpenAI:
        // 1. Create HttpClient
        // 2. Call POST /v1/chat/completions with test message
        // 3. If 200 OK, test passed
        // 4. If 401 Unauthorized, API key invalid
        // 5. If connection error, endpoint unreachable
        // 6. Return result with descriptive message
    }

    private async Task<ConfigurationTestResultDto> TestSearchProviderAsync(string provider, ...) { }
    private async Task<ConfigurationTestResultDto> TestChatProviderAsync(string provider, ...) { }
    private async Task<ConfigurationTestResultDto> TestNotificationProviderAsync(string provider, ...) { }
    // ... etc.
}
```

### Key Points
- **Encryption:** Always encrypt sensitive data before saving
- **Decryption:** Only decrypt when returning to API (mask in response)
- **Audit Trail:** Create ChangeLog entry for every update
- **Caching:** Consider caching configurations (with TTL)
- **Error Handling:** Return friendly error messages to API consumers

---

## Service 2: SystemConfigurationService

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/SystemConfigurationService.cs`

### Key Responsibilities
1. Manage system-level configurations (email, 2FA, social login)
2. Delegate to ProviderConfigurationService for persistence
3. Provide domain-specific validation
4. Test provider connectivity

### Implementation Checklist

```csharp
public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly IProviderConfigurationService _configService;
    private readonly ILogger<SystemConfigurationService> _logger;

    public async Task<SystemConfigResponseDto> GetSystemConfigAsync(CancellationToken cancellationToken = default)
    {
        // 1. Get email server config
        var emailConfig = await _configService.GetConfigurationAsync("system.email.smtp", cancellationToken);
        
        // 2. Get 2FA config
        var twoFactorConfig = await _configService.GetConfigurationAsync("system.2fa.config", cancellationToken);
        
        // 3. Get social login configs
        var googleConfig = await _configService.GetConfigurationAsync("system.sso.google", cancellationToken);
        var microsoftConfig = await _configService.GetConfigurationAsync("system.sso.microsoft", cancellationToken);
        // ... etc.
        
        // 4. Assemble and return response
        return new SystemConfigResponseDto
        {
            EmailServer = MapToEmailServerConfigDto(emailConfig),
            TwoFactor = MapToTwoFactorConfigDto(twoFactorConfig),
            SocialLogin = new SocialLoginConfigDto
            {
                Google = MapToGoogleOAuthDto(googleConfig),
                Microsoft = MapToMicrosoftOAuthDto(microsoftConfig),
                // ... etc.
            },
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = "system" // Or actual user name from context
        };
    }

    public async Task UpdateEmailServerAsync(
        EmailServerConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate dto
        ValidateEmailServerConfig(config);
        
        // 2. Serialize to dictionary
        var configData = new Dictionary<string, object>
        {
            ["smtpServer"] = config.SmtpServer,
            ["smtpPort"] = config.SmtpPort,
            ["useTls"] = config.UseTls,
            ["fromEmail"] = config.FromEmail,
            ["fromName"] = config.FromName,
            ["username"] = config.Username ?? "",
            ["password"] = config.Password ?? ""  // Will be encrypted by ProviderConfigurationService
        };
        
        // 3. Update via ProviderConfigurationService
        await _configService.UpdateConfigurationAsync(
            "system.email.smtp",
            configData,
            userId,
            cancellationToken);
    }

    public async Task<ConfigurationTestResultDto> TestEmailServerAsync(
        EmailServerConfigDto config,
        CancellationToken cancellationToken = default)
    {
        // Delegate to ProviderConfigurationService
        return await _configService.TestConfigurationAsync(
            "email",
            "smtp",
            ToDictionary(config),
            cancellationToken);
    }

    public async Task UpdateTwoFactorAsync(
        TwoFactorConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ValidateTwoFactorConfig(config);
        var configData = ToDictionary(config);
        await _configService.UpdateConfigurationAsync(
            "system.2fa.config",
            configData,
            userId,
            cancellationToken);
    }

    public async Task UpdateSocialLoginAsync(
        SocialLoginConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        // Update each provider separately
        if (config.Google?.Enabled == true)
        {
            await _configService.UpdateConfigurationAsync(
                "system.sso.google",
                ToDictionary(config.Google),
                userId,
                cancellationToken);
        }

        if (config.Microsoft?.Enabled == true)
        {
            await _configService.UpdateConfigurationAsync(
                "system.sso.microsoft",
                ToDictionary(config.Microsoft),
                userId,
                cancellationToken);
        }

        // ... etc.
    }

    // Validation methods
    private void ValidateEmailServerConfig(EmailServerConfigDto config)
    {
        if (string.IsNullOrWhiteSpace(config.SmtpServer))
            throw new ValidationException("SMTP server is required");
        
        if (config.SmtpPort < 1 || config.SmtpPort > 65535)
            throw new ValidationException("SMTP port must be between 1 and 65535");
        
        if (string.IsNullOrWhiteSpace(config.FromEmail))
            throw new ValidationException("From email is required");
        
        // Validate email format
        if (!IsValidEmail(config.FromEmail))
            throw new ValidationException("Invalid email format");
    }

    private void ValidateTwoFactorConfig(TwoFactorConfigDto config)
    {
        if (!string.IsNullOrEmpty(config.Provider))
        {
            var validProviders = new[] { "email", "sms", "totp", "disabled" };
            if (!validProviders.Contains(config.Provider))
                throw new ValidationException($"Invalid 2FA provider: {config.Provider}");
            
            if (config.Provider == "sms" && config.SmsProvider == "twilio")
            {
                if (string.IsNullOrEmpty(config.TwilioAccountSid))
                    throw new ValidationException("Twilio Account SID required");
                // ... etc.
            }
        }
    }
}
```

### Key Points
- **Delegation:** Use ProviderConfigurationService for CRUD operations
- **Validation:** Domain-specific validation before delegating
- **Organization:** Separate configuration keys for each provider
- **Mapping:** Provide helper methods to convert DTOs to dictionaries

---

## Service 3: CRMConfigurationService

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/CRMConfigurationService.cs`

### Key Responsibilities
1. Manage CRM-specific configurations (AI, integrations, agents)
2. Delegate to ProviderConfigurationService for persistence
3. Support provider selection and switching
4. Test integration connectivity

### Implementation Checklist

```csharp
public class CRMConfigurationService : ICRMConfigurationService
{
    private readonly IProviderConfigurationService _configService;
    private readonly ILogger<CRMConfigurationService> _logger;

    public async Task<CRMConfigResponseDto> GetCRMConfigAsync(CancellationToken cancellationToken = default)
    {
        // 1. Gather all CRM configurations
        var aiProviders = await GetAIProvidersAsync(cancellationToken);
        var integrations = await GetIntegrationsAsync(cancellationToken);
        var workerConfig = await GetWorkerConfigAsync(cancellationToken);
        var agents = await GetAIAgentsAsync(cancellationToken);

        return new CRMConfigResponseDto
        {
            AIProviders = aiProviders,
            Integrations = integrations,
            WorkerConfig = workerConfig,
            AIAgents = agents,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task UpdateAIProviderAsync(
        string provider,
        AIProviderConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ValidateAIProviderConfig(config);

        var configData = new Dictionary<string, object>
        {
            ["provider"] = config.Provider,
            ["enabled"] = config.Enabled,
            ["apiKey"] = config.ApiKey ?? "",
            ["apiUrl"] = config.ApiUrl ?? "",
            ["model"] = config.Model ?? "",
            ["temperature"] = config.Temperature ?? 0.7,
            ["maxTokens"] = config.MaxTokens ?? 2000,
            ["costTrackingEnabled"] = config.CostTrackingEnabled
        };

        await _configService.UpdateConfigurationAsync(
            $"crm.ai.{provider}",
            configData,
            userId,
            cancellationToken);
    }

    public async Task UpdateIntegrationAsync(
        string type,  // 'search', 'chat', 'notifications', etc.
        string provider,
        IntegrationConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ValidateIntegrationConfig(type, provider, config);

        var configData = new Dictionary<string, object>
        {
            ["type"] = type,
            ["provider"] = provider,
            ["enabled"] = config.Enabled,
            ["useBuiltIn"] = config.UseBuiltIn,
            ["configuration"] = JsonSerializer.Serialize(config.Configuration),
            ["credentials"] = JsonSerializer.Serialize(config.Credentials)
        };

        await _configService.UpdateConfigurationAsync(
            $"crm.integration.{type}.{provider}",
            configData,
            userId,
            cancellationToken);
    }

    public async Task UpdateWorkerConfigAsync(
        WorkerConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ValidateWorkerConfig(config);

        var configData = new Dictionary<string, object>
        {
            ["enabled"] = config.Enabled,
            ["maxConcurrentJobs"] = config.MaxConcurrentJobs,
            ["jobTimeoutMinutes"] = config.JobTimeoutMinutes,
            ["retryAttempts"] = config.RetryAttempts,
            ["retryDelaySeconds"] = config.RetryDelaySeconds,
            ["scheduleExpression"] = config.ScheduleExpression ?? ""
        };

        await _configService.UpdateConfigurationAsync(
            "crm.worker.default",
            configData,
            userId,
            cancellationToken);
    }

    public async Task UpdateAIAgentsAsync(
        List<AIAgentConfigDto> agents,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var configData = new Dictionary<string, object>
        {
            ["agents"] = JsonSerializer.Serialize(agents)
        };

        await _configService.UpdateConfigurationAsync(
            "crm.agent.config",
            configData,
            userId,
            cancellationToken);
    }

    public async Task<ConfigurationTestResultDto> TestAIProviderAsync(
        string provider,
        AIProviderConfigDto config,
        CancellationToken cancellationToken = default)
    {
        return await _configService.TestConfigurationAsync(
            "ai",
            provider,
            ToDictionary(config),
            cancellationToken);
    }

    public async Task<ConfigurationTestResultDto> TestIntegrationAsync(
        string type,
        string provider,
        IntegrationConfigDto config,
        CancellationToken cancellationToken = default)
    {
        return await _configService.TestConfigurationAsync(
            type,
            provider,
            ToDictionary(config),
            cancellationToken);
    }

    // Helper methods
    private async Task<List<AIProviderConfigDto>> GetAIProvidersAsync(CancellationToken cancellationToken)
    {
        var providers = new[]
        {
            "openai", "azure", "anthropic", "ollama", "bedrock", "openrouter", "gemini"
        };

        var configs = new List<AIProviderConfigDto>();
        foreach (var provider in providers)
        {
            var config = await _configService.GetConfigurationAsync($"crm.ai.{provider}", cancellationToken);
            if (config != null)
            {
                configs.Add(MapToAIProviderConfigDto(config, provider));
            }
        }

        return configs;
    }

    private async Task<List<IntegrationConfigDto>> GetIntegrationsAsync(CancellationToken cancellationToken)
    {
        // Get all integrations
        // Pattern: crm.integration.{type}.{provider}
        var integrations = new List<IntegrationConfigDto>();

        // Could optimize by querying all at once with LIKE
        var allConfigs = await _configService.GetAllConfigurationsAsync("crm", cancellationToken);
        
        foreach (var config in allConfigs.Where(c => c.ConfigurationKey?.StartsWith("crm.integration") == true))
        {
            integrations.Add(MapToIntegrationConfigDto(config));
        }

        return integrations;
    }

    private void ValidateAIProviderConfig(AIProviderConfigDto config)
    {
        var validProviders = new[] { "openai", "azure", "anthropic", "ollama", "bedrock", "openrouter", "gemini" };
        if (!validProviders.Contains(config.Provider))
            throw new ValidationException($"Invalid AI provider: {config.Provider}");

        // Provider-specific validation
        switch (config.Provider)
        {
            case "openai":
                if (string.IsNullOrEmpty(config.ApiKey))
                    throw new ValidationException("OpenAI API key is required");
                if (!config.ApiKey.StartsWith("sk-"))
                    throw new ValidationException("Invalid OpenAI API key format");
                break;

            case "azure":
                if (string.IsNullOrEmpty(config.ApiKey))
                    throw new ValidationException("Azure API key is required");
                if (string.IsNullOrEmpty(config.ApiUrl))
                    throw new ValidationException("Azure endpoint URL is required");
                break;

            // ... etc.
        }
    }

    private void ValidateIntegrationConfig(string type, string provider, IntegrationConfigDto config)
    {
        var validTypes = new[] { "search", "chat", "notifications", "analytics", "signatures", "workflows" };
        if (!validTypes.Contains(type))
            throw new ValidationException($"Invalid integration type: {type}");

        // Type-specific validation
        switch (type)
        {
            case "search":
                ValidateSearchProviderConfig(provider, config);
                break;
            case "chat":
                ValidateChatProviderConfig(provider, config);
                break;
            // ... etc.
        }
    }

    private void ValidateSearchProviderConfig(string provider, IntegrationConfigDto config)
    {
        var validProviders = new[] { "meilisearch", "elasticsearch", "algolia" };
        if (!validProviders.Contains(provider))
            throw new ValidationException($"Invalid search provider: {provider}");

        if (provider == "meilisearch" && config.Credentials?.ContainsKey("apiKey") != true)
            throw new ValidationException("Meilisearch API key is required");
    }

    private void ValidateChatProviderConfig(string provider, IntegrationConfigDto config)
    {
        // Similar validation...
    }

    private void ValidateWorkerConfig(WorkerConfigDto config)
    {
        if (config.MaxConcurrentJobs <= 0)
            throw new ValidationException("Max concurrent jobs must be greater than 0");

        if (config.JobTimeoutMinutes <= 0)
            throw new ValidationException("Job timeout must be greater than 0");

        if (config.RetryAttempts < 0)
            throw new ValidationException("Retry attempts cannot be negative");
    }
}
```

### Key Points
- **Provider Discovery:** Maintain list of known providers for each type
- **Validation:** Provider-specific validation rules
- **Configuration Keys:** Consistent naming for easy querying
- **Organization:** Separate configs for each integration type

---

## DI Registration

**File:** `CRM.Backend/src/CRM.Api/Program.cs`

Add the following registrations:

```csharp
// Configuration Management Services
services.AddScoped<IProviderConfigurationService, ProviderConfigurationService>();
services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
services.AddScoped<ICRMConfigurationService, CRMConfigurationService>();
```

---

## API Controller Templates

### SystemConfigurationController
```csharp
[ApiController]
[Route("api/admin/config/system")]
[Authorize(Roles = "Admin")]
public class SystemConfigurationController : ControllerBase
{
    private readonly ISystemConfigurationService _service;
    private readonly ILogger<SystemConfigurationController> _logger;

    [HttpGet]
    public async Task<ActionResult<SystemConfigResponseDto>> GetSystemConfig(CancellationToken cancellationToken)
    {
        var config = await _service.GetSystemConfigAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmailServer([FromBody] EmailServerConfigDto dto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _service.UpdateEmailServerAsync(dto, userId, cancellationToken);
        return Ok(new { message = "Email server configuration updated successfully" });
    }

    // etc.
}
```

---

## Testing Helpers

Consider creating a `ProviderTestHelper` class for common testing logic:

```csharp
public class ProviderTestHelper
{
    public static async Task<ConfigurationTestResultDto> TestSmtpAsync(EmailServerConfigDto config)
    {
        try
        {
            using var client = new SmtpClient(config.SmtpServer, config.SmtpPort)
            {
                EnableSsl = config.UseTls,
                Credentials = new NetworkCredential(config.Username, config.Password)
            };
            
            await client.ConnectAsync();
            return new ConfigurationTestResultDto
            {
                Success = true,
                Message = "SMTP connection successful"
            };
        }
        catch (Exception ex)
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                ErrorDetails = ex.Message
            };
        }
    }

    public static async Task<ConfigurationTestResultDto> TestOpenAIAsync(AIProviderConfigDto config)
    {
        // Similar pattern for OpenAI API testing
    }

    // ... etc.
}
```

---

## Caching Strategy (Optional)

Consider adding caching to improve performance:

```csharp
public class CachedProviderConfigurationService : IProviderConfigurationService
{
    private readonly IProviderConfigurationService _inner;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY_PREFIX = "config_";
    private const int CACHE_MINUTES = 5;

    public async Task<ProviderConfigurationDto?> GetConfigurationAsync(string configKey, ...)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{configKey}";
        
        if (_cache.TryGetValue(cacheKey, out ProviderConfigurationDto? cached))
            return cached;

        var config = await _inner.GetConfigurationAsync(configKey, ...);
        
        if (config != null)
            _cache.Set(cacheKey, config, TimeSpan.FromMinutes(CACHE_MINUTES));

        return config;
    }

    // Invalidate caches on update
    public async Task<ProviderConfigurationDto> UpdateConfigurationAsync(...)
    {
        var result = await _inner.UpdateConfigurationAsync(...);
        _cache.Remove($"{CACHE_KEY_PREFIX}{configKey}");
        return result;
    }
}
```

---

## Error Handling

Standard error responses:

```json
{
  "error": "Invalid Configuration",
  "message": "Email server is required",
  "details": "null"
}
```

HTTP Status Codes:
- `200 OK` - Success
- `201 Created` - New configuration created
- `204 No Content` - Update successful
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not authorized (not Admin)
- `404 Not Found` - Configuration not found
- `500 Internal Server Error` - Unexpected error

---

## Next Steps

1. **Create ProviderConfigurationService** with all CRUD and test methods
2. **Create SystemConfigurationService** delegating to ProviderConfigurationService
3. **Create CRMConfigurationService** for business integrations
4. **Create API Controllers** with proper authorization
5. **Register Services in DI**
6. **Test endpoints** with Postman or similar

---

**Ready to implement!** 🚀
