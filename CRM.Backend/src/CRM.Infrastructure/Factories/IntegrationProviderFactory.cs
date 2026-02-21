// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Factory for resolving integration platform provider implementations.
/// Supports runtime switching between BuiltIn webhooks, n8n, Zapier, Make, and Workato.
/// </summary>
public class IntegrationProviderFactory : IProviderFactory<IIntegrationPort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IntegrationProviderFactory> _logger;

    public IntegrationProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<IntegrationProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IIntegrationPort GetProvider()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using BuiltIn integration provider");
            return GetBuiltInProvider();
        }

        var providerType = _configuration["Providers:Integrations:Type"] ?? ProviderTypes.Integrations.BuiltIn;
        _logger.LogDebug("Resolving integration provider: {ProviderType}", providerType);

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
    public IIntegrationPort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving integration provider by name: {ProviderName}", providerName);

        return providerName.ToLowerInvariant() switch
        {
            "builtin" => GetBuiltInProvider(),
            "n8n" => GetProviderOrFallback<IIntegrationPort>("N8nProvider"),
            "zapier" => GetProviderOrFallback<IIntegrationPort>("ZapierProvider"),
            "make" => GetProviderOrFallback<IIntegrationPort>("MakeIntegrationProvider"),
            "workato" => GetProviderOrFallback<IIntegrationPort>("WorkatoIntegrationProvider"),
            "tray" => GetProviderOrFallback<IIntegrationPort>("TrayIntegrationProvider"),
            _ => throw new InvalidOperationException($"Unknown integration provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.Integrations.BuiltIn,
            ProviderTypes.Integrations.N8n,
            ProviderTypes.Integrations.Zapier,
            ProviderTypes.Integrations.Make,
            ProviderTypes.Integrations.Workato,
            ProviderTypes.Integrations.Tray
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            return ProviderTypes.Integrations.BuiltIn;
        }

        return _configuration["Providers:Integrations:Type"] ?? ProviderTypes.Integrations.BuiltIn;
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

    private IIntegrationPort GetBuiltInProvider()
    {
        return GetProviderOrFallback<IIntegrationPort>("BuiltInIntegrationProvider");
    }

    private TPort GetProviderOrFallback<TPort>(string providerTypeName) where TPort : class
    {
        var provider = ProviderResolution.ResolveByTypeName<TPort>(_serviceProvider, providerTypeName);
        if (provider != null)
        {
            return provider;
        }

        throw new InvalidOperationException($"Provider {providerTypeName} is not registered. Ensure it is configured in appsettings and registered in DI.");
    }
}
