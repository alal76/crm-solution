// Part of the Pluggable Architecture implementation
// Phase 0 Week 4: DI Registration Extensions
// Phase 1 Week 5: Added BuiltInSearchProvider registration
// Phase 1 Week 6: Added MeilisearchProvider registration
// Phase 1 Week 7: Added AlgoliaProvider registration
// Phase 2 Week 8: Added BuiltInNotificationProvider registration
// Phase 2 Week 9: Added NovuProvider registration
// Phase 2 Week 10: Added TwilioProvider and SendGridProvider registration
// Phase 3 Week 11: Added BuiltInChatProvider registration
// Phase 3 Week 12: Added ChatwootProvider registration
// Phase 3 Week 15: Added IntercomProvider registration

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Providers.BuiltIn;
using CRM.Infrastructure.Providers.Meilisearch;
using CRM.Infrastructure.Providers.Algolia;
using CRM.Infrastructure.Providers.Novu;
using CRM.Infrastructure.Providers.Twilio;
using CRM.Infrastructure.Providers.SendGrid;
using CRM.Infrastructure.Providers.Chatwoot;
using CRM.Infrastructure.Providers.Intercom;

namespace CRM.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering pluggable provider services.
/// Implements configuration-driven provider selection using feature flags.
/// </summary>
public static class ProviderServiceExtensions
{
    /// <summary>
    /// Registers all pluggable providers with feature flag support.
    /// This method wires up the factory pattern for runtime provider resolution.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPluggableProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Microsoft Feature Management
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        
        // Register the adapter registry for health monitoring (singleton)
        services.AddSingleton<AdapterRegistry>();
        
        // Register provider factories (singleton - they resolve providers)
        services.AddSingleton<IProviderFactory<ISearchPort>, SearchProviderFactory>();
        services.AddSingleton<IProviderFactory<IChatPort>, ChatProviderFactory>();
        services.AddSingleton<IProviderFactory<INotificationPort>, NotificationProviderFactory>();
        services.AddSingleton<IProviderFactory<IAnalyticsPort>, AnalyticsProviderFactory>();
        services.AddSingleton<IProviderFactory<ISignaturePort>, SignatureProviderFactory>();
        services.AddSingleton<IProviderFactory<IAIPort>, AIProviderFactory>();
        services.AddSingleton<IProviderFactory<IIntegrationPort>, IntegrationProviderFactory>();
        
        // Register BuiltIn providers (these are the default implementations)
        services.AddBuiltInProviders(configuration);
        
        // Register external providers based on configuration
        services.AddExternalProviders(configuration);
        
        // Register scoped port resolution via factories
        // This allows consumers to inject ISearchPort, IChatPort, etc. directly
        services.AddScoped<ISearchPort>(sp => 
            sp.GetRequiredService<IProviderFactory<ISearchPort>>().GetProvider());
        services.AddScoped<IChatPort>(sp => 
            sp.GetRequiredService<IProviderFactory<IChatPort>>().GetProvider());
        services.AddScoped<INotificationPort>(sp => 
            sp.GetRequiredService<IProviderFactory<INotificationPort>>().GetProvider());
        services.AddScoped<IAnalyticsPort>(sp => 
            sp.GetRequiredService<IProviderFactory<IAnalyticsPort>>().GetProvider());
        services.AddScoped<ISignaturePort>(sp => 
            sp.GetRequiredService<IProviderFactory<ISignaturePort>>().GetProvider());
        services.AddScoped<IAIPort>(sp => 
            sp.GetRequiredService<IProviderFactory<IAIPort>>().GetProvider());
        services.AddScoped<IIntegrationPort>(sp => 
            sp.GetRequiredService<IProviderFactory<IIntegrationPort>>().GetProvider());
        
        return services;
    }
    
    /// <summary>
    /// Registers BuiltIn provider implementations.
    /// These are the default providers that wrap existing CRM functionality.
    /// </summary>
    private static IServiceCollection AddBuiltInProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Phase 1: Search Provider - BuiltInSearchProvider
        // Registers as ISearchPort for factory resolution via GetServices<ISearchPort>()
        services.AddScoped<ISearchPort, BuiltInSearchProvider>();
        services.AddScoped<BuiltInSearchProvider>();
        
        // Phase 2: Notification Provider - BuiltInNotificationProvider
        // Basic SMTP email support with stubs for SMS, Push, In-App
        services.AddScoped<INotificationPort, BuiltInNotificationProvider>();
        services.AddScoped<BuiltInNotificationProvider>();
        
        // Phase 3: Chat Provider - BuiltInChatProvider
        // In-memory stub for development, use Chatwoot/Intercom for production
        services.AddScoped<IChatPort, BuiltInChatProvider>();
        services.AddScoped<BuiltInChatProvider>();
        
        // Note: Remaining BuiltIn providers will be created in subsequent phases:
        // - Phase 4: BuiltInSignatureProvider
        // - Phase 5: BuiltInAnalyticsProvider
        // - Phase 7: AI providers are already implemented
        
        return services;
    }
    
    /// <summary>
    /// Registers external provider implementations based on configuration.
    /// Only providers that are configured will be registered.
    /// </summary>
    private static IServiceCollection AddExternalProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var providersSection = configuration.GetSection("Providers");
        
        // Search providers
        AddSearchProviders(services, providersSection.GetSection("Search"));
        
        // Chat providers  
        AddChatProviders(services, providersSection.GetSection("Chat"));
        
        // Notification providers
        AddNotificationProviders(services, providersSection.GetSection("Notifications"));
        
        // Analytics providers
        AddAnalyticsProviders(services, providersSection.GetSection("Analytics"));
        
        // Signature providers
        AddSignatureProviders(services, providersSection.GetSection("Signatures"));
        
        // Integration providers
        AddIntegrationProviders(services, providersSection.GetSection("Integrations"));
        
        return services;
    }
    
    private static void AddSearchProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];
        
        // Meilisearch
        var meilisearchConfig = config.GetSection("Meilisearch");
        if (!string.IsNullOrEmpty(meilisearchConfig["Url"]))
        {
            // Register Meilisearch configuration and provider
            services.Configure<MeilisearchConfiguration>(meilisearchConfig);
            services.AddScoped<MeilisearchProvider>();
            services.AddScoped<MeilisearchHealthCheck>();
            
            // Register as ISearchPort for factory resolution
            services.AddScoped<ISearchPort, MeilisearchProvider>();
        }
        
        // Algolia
        var algoliaConfig = config.GetSection("Algolia");
        if (!string.IsNullOrEmpty(algoliaConfig["ApplicationId"]))
        {
            // Register Algolia configuration and provider
            services.Configure<AlgoliaConfiguration>(algoliaConfig);
            services.AddScoped<AlgoliaProvider>();
            services.AddScoped<AlgoliaHealthCheck>();
            
            // Register as ISearchPort for factory resolution
            services.AddScoped<ISearchPort, AlgoliaProvider>();
        }
    }
    
    private static void AddChatProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];
        
        // Chatwoot
        var chatwootConfig = config.GetSection("Chatwoot");
        if (!string.IsNullOrEmpty(chatwootConfig["BaseUrl"]))
        {
            // Register Chatwoot configuration
            services.Configure<ChatwootConfiguration>(chatwootConfig);
            
            // Register HttpClient for Chatwoot provider
            var baseUrl = chatwootConfig["BaseUrl"]!;
            var apiKey = chatwootConfig["ApiKey"] ?? "";
            var timeoutSeconds = int.TryParse(chatwootConfig["TimeoutSeconds"], out var t) ? t : 30;
            
            services.AddHttpClient<ChatwootProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("api_access_token", apiKey);
                }
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });
            
            // Register as IChatPort for factory resolution
            services.AddScoped<IChatPort, ChatwootProvider>();
        }
        
        // Intercom
        var intercomConfig = config.GetSection("Intercom");
        if (!string.IsNullOrEmpty(intercomConfig["AppId"]))
        {
            // Register Intercom configuration
            services.Configure<IntercomConfiguration>(intercomConfig);
            
            // Register HttpClient for Intercom provider
            var baseUrl = intercomConfig["BaseUrl"] ?? "https://api.intercom.io";
            var accessToken = intercomConfig["AccessToken"] ?? "";
            var apiVersion = intercomConfig["ApiVersion"] ?? "2.11";
            var timeoutSeconds = int.TryParse(intercomConfig["TimeoutSeconds"], out var t) ? t : 30;
            
            services.AddHttpClient<IntercomProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
                if (!string.IsNullOrEmpty(accessToken))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                }
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Intercom-Version", apiVersion);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });
            
            // Register as IChatPort for factory resolution
            services.AddScoped<IChatPort, IntercomProvider>();
        }
    }
    
    private static void AddNotificationProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];
        
        // Novu
        var novuConfig = config.GetSection("Novu");
        if (!string.IsNullOrEmpty(novuConfig["ApiKey"]))
        {
            // Register Novu configuration
            services.Configure<NovuConfiguration>(novuConfig);
            
            // Register HttpClient for Novu provider
            var novuUrl = novuConfig["Url"] ?? "https://api.novu.co";
            var novuApiKey = novuConfig["ApiKey"];
            var timeoutSeconds = int.TryParse(novuConfig["TimeoutSeconds"], out var t) ? t : 30;
            
            services.AddHttpClient<NovuProvider>(client =>
            {
                client.BaseAddress = new Uri(novuUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Add("Authorization", $"ApiKey {novuApiKey}");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });
            
            services.AddHttpClient<NovuHealthCheck>(client =>
            {
                client.BaseAddress = new Uri(novuUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Add("Authorization", $"ApiKey {novuApiKey}");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });
        }
        
        // Twilio (SMS/WhatsApp)
        var twilioConfig = config.GetSection("Twilio");
        if (!string.IsNullOrEmpty(twilioConfig["AccountSid"]))
        {
            // Register Twilio configuration and provider
            services.Configure<TwilioConfiguration>(twilioConfig);
            services.AddScoped<TwilioProvider>();
        }
        
        // SendGrid (Email)
        var sendGridConfig = config.GetSection("SendGrid");
        if (!string.IsNullOrEmpty(sendGridConfig["ApiKey"]))
        {
            // Register SendGrid configuration and provider
            services.Configure<SendGridConfiguration>(sendGridConfig);
            services.AddScoped<SendGridProvider>();
        }
    }
    
    private static void AddAnalyticsProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];
        
        // Superset
        var supersetConfig = config.GetSection("Superset");
        if (!string.IsNullOrEmpty(supersetConfig["Url"]))
        {
            // Will be registered when SupersetProvider is implemented in Phase 5
        }
        
        // Power BI
        var powerBiConfig = config.GetSection("PowerBI");
        if (!string.IsNullOrEmpty(powerBiConfig["TenantId"]))
        {
            // Will be registered when PowerBIProvider is implemented in Phase 5
        }
    }
    
    private static void AddSignatureProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];
        
        // DocuSeal
        var docuSealConfig = config.GetSection("DocuSeal");
        if (!string.IsNullOrEmpty(docuSealConfig["Url"]))
        {
            // Will be registered when DocuSealProvider is implemented in Phase 4
        }
        
        // DocuSign
        var docuSignConfig = config.GetSection("DocuSign");
        if (!string.IsNullOrEmpty(docuSignConfig["IntegrationKey"]))
        {
            // Will be registered when DocuSignProvider is implemented in Phase 4
        }
    }
    
    private static void AddIntegrationProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];
        
        // n8n
        var n8nConfig = config.GetSection("N8n");
        if (!string.IsNullOrEmpty(n8nConfig["BaseUrl"]))
        {
            // Will be registered when N8nProvider is implemented in Phase 6
        }
        
        // Zapier
        var zapierConfig = config.GetSection("Zapier");
        if (!string.IsNullOrEmpty(zapierConfig["WebhookBaseUrl"]))
        {
            // Will be registered when ZapierProvider is implemented in Phase 6
        }
    }
}
