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
/// Factory for resolving search provider implementations.
/// Supports runtime switching between BuiltIn, Meilisearch, Algolia, Typesense, and Elasticsearch.
/// </summary>
public class SearchProviderFactory : IProviderFactory<ISearchPort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SearchProviderFactory> _logger;

    public SearchProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<SearchProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ISearchPort GetProvider()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using BuiltIn search provider");
            return GetBuiltInProvider();
        }

        var providerType = _configuration["Providers:Search:Type"] ?? ProviderTypes.Search.BuiltIn;
        _logger.LogDebug("Resolving search provider: {ProviderType}", providerType);

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
    public ISearchPort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving search provider by name: {ProviderName}", providerName);

        return providerName.ToLowerInvariant() switch
        {
            "builtin" => GetBuiltInProvider(),
            "meilisearch" => GetProviderOrFallback<ISearchPort>("MeilisearchProvider"),
            "algolia" => GetProviderOrFallback<ISearchPort>("AlgoliaProvider"),
            "typesense" => GetProviderOrFallback<ISearchPort>("TypesenseProvider"),
            "elasticsearch" => GetProviderOrFallback<ISearchPort>("ElasticsearchProvider"),
            "azuresearch" => GetProviderOrFallback<ISearchPort>("AzureSearchProvider"),
            _ => throw new InvalidOperationException($"Unknown search provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.Search.BuiltIn,
            ProviderTypes.Search.Meilisearch,
            ProviderTypes.Search.Algolia,
            ProviderTypes.Search.Typesense,
            ProviderTypes.Search.Elasticsearch,
            ProviderTypes.Search.AzureSearch
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            return ProviderTypes.Search.BuiltIn;
        }

        return _configuration["Providers:Search:Type"] ?? ProviderTypes.Search.BuiltIn;
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

    private ISearchPort GetBuiltInProvider()
    {
        return GetProviderOrFallback<ISearchPort>("BuiltInSearchProvider");
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
