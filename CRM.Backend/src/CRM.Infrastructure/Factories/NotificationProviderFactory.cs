// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Novu;
using CRM.Infrastructure.Providers.SendGrid;
using CRM.Infrastructure.Providers.Slack;
using CRM.Infrastructure.Providers.Teams;
using CRM.Infrastructure.Providers.Twilio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Factory for resolving notification provider implementations.
/// Supports runtime switching between BuiltIn, Novu, Twilio, SendGrid, OneSignal, and AWS SNS.
/// </summary>
public class NotificationProviderFactory : IProviderFactory<INotificationPort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationProviderFactory> _logger;
    private readonly bool _useExternalProvider;

    public NotificationProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<NotificationProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // AP-015: Cache feature flag once per request scope; avoids per-call blocking on async flag check
        _useExternalProvider = _configuration.GetValue<bool>("FeatureManagement:UseExternalNotifications");
    }

    /// <inheritdoc />
    public INotificationPort GetProvider()
    {
        var useExternal = _useExternalProvider;

        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using BuiltIn notification provider");
            return GetBuiltInProvider();
        }

        var providerType = _configuration["Providers:Notifications:Type"] ?? ProviderTypes.Notifications.BuiltIn;
        _logger.LogDebug("Resolving notification provider: {ProviderType}", providerType);

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
    public INotificationPort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving notification provider by name: {ProviderName}", providerName);

        return providerName.ToLowerInvariant() switch
        {
            "builtin" => GetBuiltInProvider(),
            "novu" => GetProviderOrFallback<INotificationPort>("NovuProvider"),
            "twilio" => GetProviderOrFallback<INotificationPort>("TwilioProvider"),
            "sendgrid" => GetProviderOrFallback<INotificationPort>("SendGridProvider"),
            "onesignal" => GetProviderOrFallback<INotificationPort>("OneSignalProvider"),
            "awssns" => GetProviderOrFallback<INotificationPort>("AwsSnsProvider"),
            "teams" => GetProviderOrFallback<INotificationPort>("TeamsNotificationProvider"),
            "slack" => GetProviderOrFallback<INotificationPort>("SlackNotificationProvider"),
            _ => throw new InvalidOperationException($"Unknown notification provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.Notifications.BuiltIn,
            ProviderTypes.Notifications.Novu,
            ProviderTypes.Notifications.Twilio,
            ProviderTypes.Notifications.SendGrid,
            ProviderTypes.Notifications.OneSignal,
            ProviderTypes.Notifications.AwsSns,
            ProviderTypes.Notifications.Teams,
            ProviderTypes.Notifications.Slack
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _useExternalProvider;

        if (!useExternal)
        {
            return ProviderTypes.Notifications.BuiltIn;
        }

        return _configuration["Providers:Notifications:Type"] ?? ProviderTypes.Notifications.BuiltIn;
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

    private INotificationPort GetBuiltInProvider()
    {
        return GetProviderOrFallback<INotificationPort>("BuiltInNotificationProvider");
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
