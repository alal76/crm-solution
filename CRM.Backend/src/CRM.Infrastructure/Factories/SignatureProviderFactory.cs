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
/// Factory for resolving e-signature provider implementations.
/// Supports runtime switching between BuiltIn, DocuSeal, DocuSign, AdobeSign, HelloSign, and PandaDoc.
/// </summary>
public class SignatureProviderFactory : IProviderFactory<ISignaturePort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SignatureProviderFactory> _logger;

    public SignatureProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<SignatureProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ISignaturePort GetProvider()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using BuiltIn signature provider");
            return GetBuiltInProvider();
        }

        var providerType = _configuration["Providers:Signatures:Type"] ?? ProviderTypes.Signatures.BuiltIn;
        _logger.LogDebug("Resolving signature provider: {ProviderType}", providerType);

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
    public ISignaturePort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving signature provider by name: {ProviderName}", providerName);

        return providerName.ToLowerInvariant() switch
        {
            "builtin" => GetBuiltInProvider(),
            "docuseal" => GetProviderOrFallback<ISignaturePort>("DocuSealProvider"),
            "docusign" => GetProviderOrFallback<ISignaturePort>("DocuSignProvider"),
            "adobesign" => GetProviderOrFallback<ISignaturePort>("AdobeSignProvider"),
            "hellosign" => GetProviderOrFallback<ISignaturePort>("HelloSignProvider"),
            "pandadoc" => GetProviderOrFallback<ISignaturePort>("PandaDocProvider"),
            _ => throw new InvalidOperationException($"Unknown signature provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.Signatures.BuiltIn,
            ProviderTypes.Signatures.DocuSeal,
            ProviderTypes.Signatures.DocuSign,
            ProviderTypes.Signatures.AdobeSign,
            ProviderTypes.Signatures.HelloSign,
            ProviderTypes.Signatures.PandaDoc
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            return ProviderTypes.Signatures.BuiltIn;
        }

        return _configuration["Providers:Signatures:Type"] ?? ProviderTypes.Signatures.BuiltIn;
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

    private ISignaturePort GetBuiltInProvider()
    {
        return GetProviderOrFallback<ISignaturePort>("BuiltInSignatureProvider");
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
