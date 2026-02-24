// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using IMarketingSyncService = CRM.Core.Ports.Input.IMarketingSyncService;
using MarketingSyncResult = CRM.Core.Ports.Input.MarketingSyncResult;
using MarketingImportResult = CRM.Core.Ports.Input.MarketingImportResult;
using MarketingList = CRM.Core.Ports.Input.MarketingList;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of IMarketingSyncService for Mailchimp / HubSpot integration.
/// Implements TODO-INT-09.
///
/// Bridges CRM contacts, segments, and campaign data to external
/// marketing platforms. Actual API calls require their respective
/// SDK packages or REST clients with OAuth2 authentication.
/// </summary>
public class MarketingSyncService : IMarketingSyncService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MarketingSyncService> _logger;

    public MarketingSyncService(
        IConfiguration configuration,
        ILogger<MarketingSyncService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<MarketingSyncResult> SyncContactAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Sync contact {ContactId} to {Provider} (stub)", contactId, provider);

        return Task.FromResult(new MarketingSyncResult
        {
            Success = false,
            Provider = provider,
            ErrorMessage = $"{provider} contact sync not yet implemented. Configure API key in Integrations:Marketing:{provider}:ApiKey."
        });
    }

    /// <inheritdoc />
    public Task<MarketingSyncResult> SyncSegmentAsync(int segmentId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Sync segment {SegmentId} to {Provider} (stub)", segmentId, provider);

        return Task.FromResult(new MarketingSyncResult
        {
            Success = false,
            Provider = provider,
            ErrorMessage = $"{provider} segment sync not yet implemented."
        });
    }

    /// <inheritdoc />
    public Task<MarketingSyncResult> PushCampaignResultsAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Push campaign {CampaignId} results from {Provider} (stub)", campaignId, provider);

        return Task.FromResult(new MarketingSyncResult
        {
            Success = false,
            Provider = provider,
            ErrorMessage = $"{provider} campaign result sync not yet implemented."
        });
    }

    /// <inheritdoc />
    public Task<MarketingImportResult> ImportListAsync(string externalListId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Import list {ExternalListId} from {Provider} (stub)", externalListId, provider);

        return Task.FromResult(new MarketingImportResult
        {
            TotalImported = 0,
            NewLeads = 0,
            UpdatedContacts = 0,
            Skipped = 0,
            Errors = new[] { $"Import from {provider} not yet implemented." }
        });
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<MarketingList>> GetExternalListsAsync(CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Fetching external lists from {Provider} (stub)", provider);

        // Return empty list until integration is configured
        return Task.FromResult<IReadOnlyCollection<MarketingList>>(Array.Empty<MarketingList>());
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        var apiKey = _configuration[$"Integrations:Marketing:{provider}:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("{Provider} API key not configured at Integrations:Marketing:{Provider}:ApiKey", provider, provider);
            return Task.FromResult(false);
        }

        _logger.LogInformation("Test connection to {Provider} (stub - returning false until SDK is installed)", provider);
        return Task.FromResult(false);
    }

    private string GetProvider()
    {
        return _configuration["Integrations:Marketing:Provider"] ?? "Mailchimp";
    }

    public Task<MarketingSyncResult> SyncContactAsync(int contactId, string listId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SyncContactAsync not implemented - requires marketing platform API");
        return Task.FromResult(MarketingSyncResult.Failed("Not implemented", "None"));
    }

    public Task<MarketingSyncResult> SyncCampaignMetricsAsync(int campaignId, string externalCampaignId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SyncCampaignMetricsAsync not implemented - requires marketing platform API");
        return Task.FromResult(MarketingSyncResult.Failed("Not implemented", "None"));
    }

    public Task<MarketingSyncResult> SyncSegmentAsync(string segmentName, IReadOnlyList<int> contactIds, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SyncSegmentAsync not implemented - requires marketing platform API");
        return Task.FromResult(MarketingSyncResult.Failed("Not implemented", "None"));
    }

    public Task<IReadOnlyList<MarketingList>> GetListsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetListsAsync not implemented - requires marketing platform API");
        return Task.FromResult<IReadOnlyList<MarketingList>>(new List<MarketingList>());
    }

    public Task<MarketingImportResult> ImportSubscribersAsLeadsAsync(string listId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ImportSubscribersAsLeadsAsync not implemented - requires marketing platform API");
        return Task.FromResult(new MarketingImportResult { TotalImported = 0 });
    }

}
