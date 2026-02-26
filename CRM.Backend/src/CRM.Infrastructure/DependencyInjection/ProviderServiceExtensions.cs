// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Providers.AI;
using CRM.Infrastructure.Providers.Algolia;
using CRM.Infrastructure.Providers.BuiltIn;
using CRM.Infrastructure.Providers.Chatwoot;
using CRM.Infrastructure.Providers.DocuSeal;
using CRM.Infrastructure.Providers.DocuSign;
using CRM.Infrastructure.Providers.Integration;
using CRM.Infrastructure.Providers.Intercom;
using CRM.Infrastructure.Providers.Meilisearch;
using CRM.Infrastructure.Providers.Novu;
using CRM.Infrastructure.Providers.PowerBI;
using CRM.Infrastructure.Providers.SendGrid;
using CRM.Infrastructure.Providers.Slack;
using CRM.Infrastructure.Providers.Stripe;
using CRM.Infrastructure.Providers.Superset;
using CRM.Infrastructure.Providers.Teams;
using CRM.Infrastructure.Providers.Twilio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

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

        // Register provider factories (scoped - they resolve scoped providers via GetServices<T>())
        services.AddScoped<IProviderFactory<ISearchPort>, SearchProviderFactory>();
        services.AddScoped<IProviderFactory<IChatPort>, ChatProviderFactory>();
        services.AddScoped<IProviderFactory<INotificationPort>, NotificationProviderFactory>();
        services.AddScoped<IProviderFactory<IAnalyticsPort>, AnalyticsProviderFactory>();
        services.AddScoped<IProviderFactory<ISignaturePort>, SignatureProviderFactory>();
        services.AddScoped<IProviderFactory<IAIPort>, AIProviderFactory>();
        services.AddScoped<IProviderFactory<IIntegrationPort>, IntegrationProviderFactory>();

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

        // Phase 4: Signature Provider - BuiltInSignatureProvider
        // Manual signature workflow tracking without external e-signature services
        services.AddScoped<ISignaturePort, BuiltInSignatureProvider>();
        services.AddScoped<BuiltInSignatureProvider>();

        // Phase 5: Analytics Provider - BuiltInAnalyticsProvider
        // Basic dashboard and reporting using direct database queries via EF Core
        services.AddScoped<IAnalyticsPort, BuiltInAnalyticsProvider>();
        services.AddScoped<BuiltInAnalyticsProvider>();

        // Note: Remaining BuiltIn providers will be created in subsequent phases:
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

        // Payment providers
        AddPaymentProviders(services, providersSection.GetSection("Payment"));

        // AI/LLM providers (Phase 7)
        AddAIProviders(services, providersSection.GetSection("AI"));

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

        // Microsoft Teams (Incoming Webhook)
        var teamsConfig = config.GetSection("Teams");
        if (!string.IsNullOrEmpty(teamsConfig["WebhookUrl"]))
        {
            services.Configure<TeamsConfiguration>(teamsConfig);
            var webhookUrl = teamsConfig["WebhookUrl"]!;
            var teamsTimeout = int.TryParse(teamsConfig["TimeoutSeconds"], out var tt) ? tt : 30;
            services.AddHttpClient<TeamsNotificationProvider>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(teamsTimeout);
            });
        }

        // Slack (Incoming Webhook)
        var slackConfig = config.GetSection("Slack");
        if (!string.IsNullOrEmpty(slackConfig["WebhookUrl"]))
        {
            services.Configure<SlackConfiguration>(slackConfig);
            var slackTimeout = int.TryParse(slackConfig["TimeoutSeconds"], out var st) ? st : 30;
            services.AddHttpClient<SlackNotificationProvider>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(slackTimeout);
            });
        }
    }

    private static void AddAnalyticsProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];

        // Superset
        var supersetConfig = config.GetSection("Superset");
        if (!string.IsNullOrEmpty(supersetConfig["BaseUrl"]))
        {
            // Register Superset configuration
            services.Configure<SupersetConfiguration>(supersetConfig);

            // Register HttpClient for Superset provider
            var baseUrl = supersetConfig["BaseUrl"]!;
            var timeoutSeconds = int.TryParse(supersetConfig["TimeoutSeconds"], out var t) ? t : 30;

            services.AddHttpClient<SupersetProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            // Register as IAnalyticsPort for factory resolution
            services.AddScoped<IAnalyticsPort, SupersetProvider>();
        }

        // Power BI
        var powerBiConfig = config.GetSection("PowerBI");
        if (!string.IsNullOrEmpty(powerBiConfig["TenantId"]))
        {
            // Register Power BI configuration
            services.Configure<PowerBIConfiguration>(powerBiConfig);

            // Register HttpClient for Power BI provider
            var timeoutSeconds = int.TryParse(powerBiConfig["TimeoutSeconds"], out var t) ? t : 30;

            services.AddHttpClient<PowerBIProvider>(client =>
            {
                client.BaseAddress = new Uri("https://api.powerbi.com/v1.0/myorg/");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            // Register as IAnalyticsPort for factory resolution
            services.AddScoped<IAnalyticsPort, PowerBIProvider>();
        }
    }

    private static void AddSignatureProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];

        // DocuSeal
        var docuSealConfig = config.GetSection("DocuSeal");
        if (!string.IsNullOrEmpty(docuSealConfig["Url"]))
        {
            services.Configure<DocuSealConfiguration>(docuSealConfig);
            services.AddHttpClient<DocuSealProvider>(client =>
            {
                client.BaseAddress = new Uri(docuSealConfig["Url"]!.TrimEnd('/') + "/api/");
                if (!string.IsNullOrEmpty(docuSealConfig["ApiKey"]))
                {
                    client.DefaultRequestHeaders.Add("X-Auth-Token", docuSealConfig["ApiKey"]);
                }
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).ConfigureHttpClient(client =>
            {
                var timeout = int.TryParse(docuSealConfig["TimeoutSeconds"], out var t) ? t : 30;
                client.Timeout = TimeSpan.FromSeconds(timeout);
            });
            // Register as ISignaturePort for factory resolution
            services.AddScoped<ISignaturePort, DocuSealProvider>();
        }

        // DocuSign
        var docuSignConfig = config.GetSection("DocuSign");
        if (!string.IsNullOrEmpty(docuSignConfig["IntegrationKey"]))
        {
            services.Configure<DocuSignConfiguration>(docuSignConfig);
            services.AddScoped<DocuSignProvider>();

            // Register as ISignaturePort for factory resolution
            services.AddScoped<ISignaturePort, DocuSignProvider>();
        }
    }

    private static void AddIntegrationProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];

        // BuiltIn Integration Provider (webhook-based)
        services.Configure<BuiltInIntegrationConfiguration>(config.GetSection("BuiltIn"));
        services.AddHttpClient<BuiltInIntegrationProvider>();
        services.AddScoped<IIntegrationPort, BuiltInIntegrationProvider>();

        // n8n
        var n8nConfig = config.GetSection("N8n");
        if (!string.IsNullOrEmpty(n8nConfig["BaseUrl"]))
        {
            services.Configure<N8nConfiguration>(n8nConfig);

            var baseUrl = n8nConfig["BaseUrl"]!;
            var apiKey = n8nConfig["ApiKey"] ?? "";
            var timeoutSeconds = int.TryParse(n8nConfig["TimeoutSeconds"], out var t) ? t : 30;

            services.AddHttpClient<N8nProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-N8N-API-KEY", apiKey);
                }
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            services.AddScoped<IIntegrationPort, N8nProvider>();
        }

        // Zapier
        var zapierConfig = config.GetSection("Zapier");
        if (!string.IsNullOrEmpty(zapierConfig["WebhookBaseUrl"]) ||
            zapierConfig.GetSection("EventWebhooks").GetChildren().Any())
        {
            services.Configure<ZapierConfiguration>(zapierConfig);

            var timeoutSeconds = int.TryParse(zapierConfig["TimeoutSeconds"], out var t) ? t : 30;

            services.AddHttpClient<ZapierProvider>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            services.AddScoped<IIntegrationPort, ZapierProvider>();
        }
    }

    private static void AddPaymentProviders(IServiceCollection services, IConfiguration config)
    {
        // Stripe
        var stripeConfig = config.GetSection("Stripe");
        if (!string.IsNullOrEmpty(stripeConfig["WebhookSecret"]))
        {
            services.Configure<StripeConfiguration>(stripeConfig);
        }
    }

    private static void AddAIProviders(IServiceCollection services, IConfiguration config)
    {
        var providerType = config["Type"];

        // Ollama (local LLM)
        var ollamaConfig = config.GetSection("Ollama");
        if (!string.IsNullOrEmpty(ollamaConfig["BaseUrl"]))
        {
            services.Configure<OllamaConfiguration>(ollamaConfig);

            var baseUrl = ollamaConfig["BaseUrl"]!;
            var timeoutSeconds = int.TryParse(ollamaConfig["TimeoutSeconds"], out var t) ? t : 120;

            services.AddHttpClient<OllamaProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            services.AddScoped<IAIPort, OllamaProvider>();
        }

        // Azure OpenAI
        var azureOpenAiConfig = config.GetSection("AzureOpenAI");
        if (!string.IsNullOrEmpty(azureOpenAiConfig["Endpoint"]))
        {
            services.Configure<AzureOpenAIConfiguration>(azureOpenAiConfig);

            var endpoint = azureOpenAiConfig["Endpoint"]!;
            var apiKey = azureOpenAiConfig["ApiKey"] ?? "";
            var timeoutSeconds = int.TryParse(azureOpenAiConfig["TimeoutSeconds"], out var t) ? t : 120;

            services.AddHttpClient<AzureOpenAIProvider>(client =>
            {
                client.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("api-key", apiKey);
                }
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            services.AddScoped<IAIPort, AzureOpenAIProvider>();
        }

        // AWS Bedrock
        var bedrockConfig = config.GetSection("Bedrock");
        if (!string.IsNullOrEmpty(bedrockConfig["Region"]))
        {
            services.Configure<BedrockConfiguration>(bedrockConfig);

            var region = bedrockConfig["Region"]!;
            var timeoutSeconds = int.TryParse(bedrockConfig["TimeoutSeconds"], out var t) ? t : 120;

            // Note: For production, use AWS SDK with proper credential chain
            // This HttpClient setup expects SigV4 signing to be handled externally
            // or via AWS SDK's credential provider
            services.AddHttpClient<BedrockProvider>(client =>
            {
                client.BaseAddress = new Uri($"https://bedrock-runtime.{region}.amazonaws.com/");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            services.AddScoped<IAIPort, BedrockProvider>();
        }

        // OpenRouter (multi-model AI gateway)
        var openRouterConfig = config.GetSection("OpenRouter");
        if (!string.IsNullOrEmpty(openRouterConfig["ApiKey"]))
        {
            services.Configure<OpenRouterConfiguration>(openRouterConfig);

            var baseUrl = openRouterConfig["BaseUrl"] ?? "https://openrouter.ai/api/v1";
            var apiKey = openRouterConfig["ApiKey"]!;
            var timeoutSeconds = int.TryParse(openRouterConfig["TimeoutSeconds"], out var t) ? t : 120;

            services.AddHttpClient<OpenRouterProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            services.AddScoped<IAIPort, OpenRouterProvider>();
        }
    }
}
