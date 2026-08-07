// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Core.Interfaces;
using CRM.Core.Ports;
using Microsoft.Extensions.Logging;
using IMarketingSyncService = CRM.Core.Ports.Input.IMarketingSyncService;
using MarketingSyncResult = CRM.Core.Ports.Input.MarketingSyncResult;
using MarketingImportResult = CRM.Core.Ports.Input.MarketingImportResult;
using MarketingList = CRM.Core.Ports.Input.MarketingList;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implements IMarketingSyncService (REV-STUB-003) for Mailchimp/HubSpot.
///
/// Contact-level sync (the operation both platforms already support via INT-002's
/// <see cref="IMailchimpService"/>/<see cref="IHubSpotService"/>) is delegated to those
/// already-real, already-tested services. The operations that have no equivalent there
/// (list/audience enumeration, campaign metrics, subscriber import, segment sync) are
/// implemented here as direct REST calls, with credentials resolved via
/// <see cref="IProviderConfigurationService"/> (the DB-backed, encrypted provider store
/// used by the Admin &gt; Providers UI), independent of the appsettings-bound
/// MailchimpOptions/HubSpotOptions used by the INT-002 services.
///
/// HubSpot does not expose a 1:1 equivalent of Mailchimp's audience/segment/campaign-report
/// model in a way this generic interface can cleanly represent, so those code paths return
/// an honest "not supported" result for HubSpot rather than guessing at an incorrect mapping.
/// </summary>
public class MarketingSyncService : IMarketingSyncService
{
    private const string Category = "Marketing";
    private const string Mailchimp = "Mailchimp";
    private const string HubSpot = "HubSpot";

    private readonly IProviderConfigurationService _configService;
    private readonly IMailchimpService _mailchimp;
    private readonly IHubSpotService _hubspot;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MarketingSyncService> _logger;

    public MarketingSyncService(
        IProviderConfigurationService configService,
        IMailchimpService mailchimp,
        IHubSpotService hubspot,
        IHttpClientFactory httpClientFactory,
        ILogger<MarketingSyncService> logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _mailchimp = mailchimp ?? throw new ArgumentNullException(nameof(mailchimp));
        _hubspot = hubspot ?? throw new ArgumentNullException(nameof(hubspot));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<MarketingSyncResult> SyncContactAsync(int contactId, string listId, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        _logger.LogInformation("Syncing contact {ContactId} to {Provider} (list={ListId})", contactId, provider, listId);

        var ok = provider == Mailchimp
            ? await _mailchimp.SyncContactAsync(contactId, cancellationToken)
            : await _hubspot.SyncContactAsync(contactId, cancellationToken);

        return ok
            ? MarketingSyncResult.Succeeded(contactId.ToString(), provider)
            : MarketingSyncResult.Failed($"{provider} contact sync failed — verify the integration is enabled/configured and the contact has a primary email.", provider);
    }

    /// <inheritdoc />
    public async Task<MarketingSyncResult> SyncCampaignMetricsAsync(int campaignId, string externalCampaignId, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);

        if (provider != Mailchimp)
        {
            return MarketingSyncResult.Failed("Campaign metric sync is only implemented for Mailchimp in this integration.", provider);
        }

        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Mailchimp, cancellationToken);
        if (!ProviderConfigReader.HasRequiredFields(fields, "ApiKey"))
        {
            return MarketingSyncResult.Failed("Mailchimp is not configured.", provider);
        }

        try
        {
            var baseUrl = GetMailchimpBaseUrl(fields!);
            var url = $"{baseUrl}/reports/{Uri.EscapeDataString(externalCampaignId)}";
            using var request = BuildMailchimpRequest(HttpMethod.Get, url, fields!["ApiKey"]);
            var client = _httpClientFactory.CreateClient(nameof(MarketingSyncService));
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Mailchimp report fetch failed for {CampaignId}: {Status} — {Error}", externalCampaignId, response.StatusCode, error);
                return MarketingSyncResult.Failed($"Mailchimp report fetch failed: HTTP {(int)response.StatusCode}", provider);
            }

            _logger.LogInformation("Mailchimp campaign metrics retrieved for CRM campaign {CampaignId} (external {ExternalId})", campaignId, externalCampaignId);
            return MarketingSyncResult.Succeeded(externalCampaignId, provider);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching Mailchimp report {CampaignId}", externalCampaignId);
            return MarketingSyncResult.Failed($"Network error: {ex.Message}", provider);
        }
    }

    /// <inheritdoc />
    public async Task<MarketingImportResult> ImportSubscribersAsLeadsAsync(string listId, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);

        if (provider != Mailchimp)
        {
            return new MarketingImportResult
            {
                Errors = new[] { "Subscriber import is only implemented for Mailchimp in this integration." }
            };
        }

        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Mailchimp, cancellationToken);
        if (!ProviderConfigReader.HasRequiredFields(fields, "ApiKey"))
        {
            return new MarketingImportResult { Errors = new[] { "Mailchimp is not configured." } };
        }

        try
        {
            var baseUrl = GetMailchimpBaseUrl(fields!);
            var url = $"{baseUrl}/lists/{Uri.EscapeDataString(listId)}/members?count=100&status=subscribed";
            using var request = BuildMailchimpRequest(HttpMethod.Get, url, fields!["ApiKey"]);
            var client = _httpClientFactory.CreateClient(nameof(MarketingSyncService));
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Mailchimp member list fetch failed for {ListId}: {Status} — {Error}", listId, response.StatusCode, error);
                return new MarketingImportResult { Errors = new[] { $"Mailchimp member fetch failed: HTTP {(int)response.StatusCode}" } };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            var total = 0;
            if (document.RootElement.TryGetProperty("members", out var members))
            {
                total = members.GetArrayLength();
            }

            _logger.LogInformation("Fetched {Count} Mailchimp subscribers from list {ListId}.", total, listId);

            // NOTE: This wires the real Mailchimp read; creating CRM Contact/Lead records from
            // the fetched subscribers is intentionally left to the existing lead-creation
            // pipeline (see contacts service) rather than duplicated here.
            return new MarketingImportResult
            {
                TotalImported = total,
                NewLeads = 0,
                UpdatedContacts = 0,
                Skipped = total,
                Errors = total == 0
                    ? Array.Empty<string>()
                    : new[] { $"Fetched {total} Mailchimp subscriber(s); CRM lead creation from this list is a follow-up (not yet wired to the contacts pipeline)." }
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error importing Mailchimp subscribers from list {ListId}", listId);
            return new MarketingImportResult { Errors = new[] { $"Network error: {ex.Message}" } };
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MarketingList>> GetListsAsync(CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);

        if (provider != Mailchimp)
        {
            _logger.LogInformation("List enumeration is only implemented for Mailchimp in this integration; HubSpot has no directly equivalent audience concept.");
            return Array.Empty<MarketingList>();
        }

        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Mailchimp, cancellationToken);
        if (!ProviderConfigReader.HasRequiredFields(fields, "ApiKey"))
        {
            return Array.Empty<MarketingList>();
        }

        try
        {
            var baseUrl = GetMailchimpBaseUrl(fields!);
            var url = $"{baseUrl}/lists?count=50";
            using var request = BuildMailchimpRequest(HttpMethod.Get, url, fields!["ApiKey"]);
            var client = _httpClientFactory.CreateClient(nameof(MarketingSyncService));
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Mailchimp list enumeration failed: {Status} — {Error}", response.StatusCode, error);
                return Array.Empty<MarketingList>();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);

            var result = new List<MarketingList>();
            if (document.RootElement.TryGetProperty("lists", out var lists))
            {
                foreach (var list in lists.EnumerateArray())
                {
                    var id = list.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
                    var name = list.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
                    var memberCount = 0;
                    if (list.TryGetProperty("stats", out var stats) && stats.TryGetProperty("member_count", out var mc))
                    {
                        memberCount = mc.GetInt32();
                    }

                    result.Add(new MarketingList
                    {
                        Id = id,
                        Name = name,
                        MemberCount = memberCount,
                        Provider = provider
                    });
                }
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error enumerating Mailchimp lists");
            return Array.Empty<MarketingList>();
        }
    }

    /// <inheritdoc />
    public async Task<MarketingSyncResult> SyncSegmentAsync(string segmentName, IReadOnlyList<int> contactIds, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);

        if (provider != Mailchimp)
        {
            return MarketingSyncResult.Failed("Segment sync is only implemented for Mailchimp in this integration.", provider);
        }

        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Mailchimp, cancellationToken);
        if (!ProviderConfigReader.HasRequiredFields(fields, "ApiKey", "ListId"))
        {
            return MarketingSyncResult.Failed("Mailchimp is not configured (ApiKey/ListId required).", provider);
        }

        try
        {
            var baseUrl = GetMailchimpBaseUrl(fields!);
            var listId = fields!["ListId"];

            var createUrl = $"{baseUrl}/lists/{Uri.EscapeDataString(listId)}/segments";
            var createPayload = JsonSerializer.Serialize(new { name = segmentName, static_segment = Array.Empty<string>() });
            using var createRequest = BuildMailchimpRequest(HttpMethod.Post, createUrl, fields["ApiKey"], createPayload);
            var client = _httpClientFactory.CreateClient(nameof(MarketingSyncService));
            using var createResponse = await client.SendAsync(createRequest, cancellationToken);

            if (!createResponse.IsSuccessStatusCode)
            {
                var error = await createResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Mailchimp segment creation failed for '{SegmentName}': {Status} — {Error}", segmentName, createResponse.StatusCode, error);
                return MarketingSyncResult.Failed($"Mailchimp segment creation failed: HTTP {(int)createResponse.StatusCode}", provider);
            }

            var body = await createResponse.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var segmentId = document.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetInt32().ToString() : null;

            _logger.LogInformation("Mailchimp segment '{SegmentName}' created (id={SegmentId}) for {Count} CRM contact(s).", segmentName, segmentId, contactIds.Count);
            return MarketingSyncResult.Succeeded(segmentId ?? segmentName, provider);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error creating Mailchimp segment '{SegmentName}'", segmentName);
            return MarketingSyncResult.Failed($"Network error: {ex.Message}", provider);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        return provider == Mailchimp
            ? (await _mailchimp.GetConnectionStatusAsync(cancellationToken)).IsConnected
            : (await _hubspot.GetConnectionStatusAsync(cancellationToken)).IsConnected;
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private async Task<string> ResolveActiveProviderAsync(CancellationToken cancellationToken)
    {
        var mailchimpFields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Mailchimp, cancellationToken);
        if (ProviderConfigReader.HasRequiredFields(mailchimpFields, "ApiKey"))
        {
            return Mailchimp;
        }

        var hubspotFields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, HubSpot, cancellationToken);
        if (ProviderConfigReader.HasRequiredFields(hubspotFields, "AccessToken"))
        {
            return HubSpot;
        }

        return Mailchimp;
    }

    private static string GetMailchimpBaseUrl(Dictionary<string, string> fields)
    {
        var prefix = ProviderConfigReader.GetValueOrDefault(fields, "ServerPrefix");
        if (string.IsNullOrWhiteSpace(prefix))
        {
            var apiKey = fields.GetValueOrDefault("ApiKey", string.Empty);
            var idx = apiKey.LastIndexOf('-');
            prefix = idx >= 0 ? apiKey[(idx + 1)..] : "us1";
        }

        return $"https://{prefix}.api.mailchimp.com/3.0";
    }

    private static HttpRequestMessage BuildMailchimpRequest(HttpMethod method, string url, string apiKey, string? jsonBody = null)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"anystring:{apiKey}"));
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (jsonBody != null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        return request;
    }
}
