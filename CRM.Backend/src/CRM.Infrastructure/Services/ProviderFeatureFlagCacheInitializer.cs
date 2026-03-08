// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Hosted service that populates <see cref="ProviderFeatureFlagCache"/> at application startup.
/// By awaiting <see cref="IFeatureManager.IsEnabledAsync"/> here (rather than in per-request
/// factory constructors), we eliminate all sync-over-async (GetAwaiter().GetResult()) patterns
/// in the seven provider factories (AP-015).
/// The host guarantees this completes before the HTTP server begins accepting requests.
/// </summary>
public class ProviderFeatureFlagCacheInitializer : IHostedService
{
    private readonly IFeatureManager _featureManager;
    private readonly ProviderFeatureFlagCache _cache;
    private readonly ILogger<ProviderFeatureFlagCacheInitializer> _logger;

    public ProviderFeatureFlagCacheInitializer(
        IFeatureManager featureManager,
        ProviderFeatureFlagCache cache,
        ILogger<ProviderFeatureFlagCacheInitializer> logger)
    {
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AP-015: Populating ProviderFeatureFlagCache at startup");

        // AP-015: Properly await each flag — no GetAwaiter().GetResult() here
        _cache.UseExternalSearch = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch);
        _cache.UseExternalChat = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat);
        _cache.UseExternalNotifications = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications);
        _cache.UseExternalAnalytics = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics);
        _cache.UseExternalSignatures = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures);
        _cache.UseExternalAI = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI);
        _cache.UseExternalIntegrations = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalIntegrations);

        _logger.LogInformation(
            "AP-015: ProviderFeatureFlagCache populated — Search={Search}, Chat={Chat}, " +
            "Notifications={Notifications}, Analytics={Analytics}, Signatures={Signatures}, " +
            "AI={AI}, Integrations={Integrations}",
            _cache.UseExternalSearch,
            _cache.UseExternalChat,
            _cache.UseExternalNotifications,
            _cache.UseExternalAnalytics,
            _cache.UseExternalSignatures,
            _cache.UseExternalAI,
            _cache.UseExternalIntegrations);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
