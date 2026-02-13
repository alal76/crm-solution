// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using CRM.Core.Ports.Output.Providers;
using CRM.Core.Features;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Factory for resolving analytics provider implementations.
/// Supports runtime switching between BuiltIn, Superset, Metabase, PowerBI, Looker, and QuickSight.
/// </summary>
public class AnalyticsProviderFactory : IProviderFactory<IAnalyticsPort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsProviderFactory> _logger;

    public AnalyticsProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<AnalyticsProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IAnalyticsPort GetProvider()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using BuiltIn analytics provider");
            return GetBuiltInProvider();
        }

        var providerType = _configuration["Providers:Analytics:Type"] ?? ProviderTypes.Analytics.BuiltIn;
        _logger.LogDebug("Resolving analytics provider: {ProviderType}", providerType);

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
    public IAnalyticsPort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving analytics provider by name: {ProviderName}", providerName);

        return providerName.ToLowerInvariant() switch
        {
            "builtin" => GetBuiltInProvider(),
            "superset" => GetProviderOrFallback<IAnalyticsPort>("SupersetProvider"),
            "metabase" => GetProviderOrFallback<IAnalyticsPort>("MetabaseProvider"),
            "powerbi" => GetProviderOrFallback<IAnalyticsPort>("PowerBIProvider"),
            "looker" => GetProviderOrFallback<IAnalyticsPort>("LookerProvider"),
            "quicksight" => GetProviderOrFallback<IAnalyticsPort>("QuickSightProvider"),
            _ => throw new InvalidOperationException($"Unknown analytics provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.Analytics.BuiltIn,
            ProviderTypes.Analytics.Superset,
            ProviderTypes.Analytics.Metabase,
            ProviderTypes.Analytics.PowerBI,
            ProviderTypes.Analytics.Looker,
            ProviderTypes.Analytics.QuickSight
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            return ProviderTypes.Analytics.BuiltIn;
        }

        return _configuration["Providers:Analytics:Type"] ?? ProviderTypes.Analytics.BuiltIn;
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

    private IAnalyticsPort GetBuiltInProvider()
    {
        return GetProviderOrFallback<IAnalyticsPort>("BuiltInAnalyticsProvider");
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
