// CRM Solution - Chat Provider Factory
// Phase 0 Week 3 Task 3.3: Factory for resolving chat providers
// Part of the Pluggable Architecture implementation

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using CRM.Core.Ports.Output.Providers;
using CRM.Core.Features;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Factory for resolving chat provider implementations.
/// Supports runtime switching between BuiltIn, Chatwoot, Intercom, Zendesk, and Freshchat.
/// </summary>
public class ChatProviderFactory : IProviderFactory<IChatPort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatProviderFactory> _logger;

    public ChatProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<ChatProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IChatPort GetProvider()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat)
            .GetAwaiter().GetResult();
        
        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using BuiltIn chat provider");
            return GetBuiltInProvider();
        }

        var providerType = _configuration["Providers:Chat:Type"] ?? ProviderTypes.Chat.BuiltIn;
        _logger.LogDebug("Resolving chat provider: {ProviderType}", providerType);
        
        try
        {
            return GetProvider(providerType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve {ProviderType}. Falling back to BuiltIn", providerType);
            return GetBuiltInProvider();
        }
    }

    /// <inheritdoc />
    public IChatPort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving chat provider by name: {ProviderName}", providerName);
        
        return providerName.ToLowerInvariant() switch
        {
            "builtin" => GetBuiltInProvider(),
            "chatwoot" => GetProviderOrFallback<IChatPort>("ChatwootProvider"),
            "intercom" => GetProviderOrFallback<IChatPort>("IntercomProvider"),
            "zendesk" => GetProviderOrFallback<IChatPort>("ZendeskProvider"),
            "freshchat" => GetProviderOrFallback<IChatPort>("FreshchatProvider"),
            _ => throw new InvalidOperationException($"Unknown chat provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.Chat.BuiltIn,
            ProviderTypes.Chat.Chatwoot,
            ProviderTypes.Chat.Intercom,
            ProviderTypes.Chat.Zendesk,
            ProviderTypes.Chat.Freshchat
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat)
            .GetAwaiter().GetResult();
        
        if (!useExternal)
        {
            return ProviderTypes.Chat.BuiltIn;
        }
        
        return _configuration["Providers:Chat:Type"] ?? ProviderTypes.Chat.BuiltIn;
    }

    /// <inheritdoc />
    public async Task<bool> IsProviderAvailableAsync(string providerName)
    {
        try
        {
            var provider = GetProvider(providerName);
            return await provider.IsAvailableAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Provider {ProviderName} is not available", providerName);
            return false;
        }
    }

    private IChatPort GetBuiltInProvider()
    {
        return GetProviderOrFallback<IChatPort>("BuiltInChatProvider");
    }

    private TPort GetProviderOrFallback<TPort>(string providerTypeName) where TPort : class
    {
        var providers = _serviceProvider.GetServices<TPort>();
        
        foreach (var provider in providers)
        {
            if (provider.GetType().Name.Equals(providerTypeName, StringComparison.OrdinalIgnoreCase))
            {
                return provider;
            }
        }

        throw new InvalidOperationException($"Provider {providerTypeName} is not registered. Ensure it is configured in appsettings and registered in DI.");
    }
}
