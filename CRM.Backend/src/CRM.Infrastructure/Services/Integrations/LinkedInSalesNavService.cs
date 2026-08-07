// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ILinkedInSalesNavService = CRM.Core.Ports.Input.ILinkedInSalesNavService;
using LinkedInSearchQuery = CRM.Core.Ports.Input.LinkedInSearchQuery;
using LinkedInSearchResult = CRM.Core.Ports.Input.LinkedInSearchResult;
using LinkedInProfile = CRM.Core.Ports.Input.LinkedInProfile;
using LinkedInEnrichResult = CRM.Core.Ports.Input.LinkedInEnrichResult;
using LinkedInImportResult = CRM.Core.Ports.Input.LinkedInImportResult;
using LinkedInActivity = CRM.Core.Ports.Input.LinkedInActivity;
using LinkedInMessageResult = CRM.Core.Ports.Input.LinkedInMessageResult;

#pragma warning disable SA1648 // inheritdoc used on interface-implementing member; interface resolved via alias
namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implementation of ILinkedInSalesNavService.
/// Implements TODO-INT-10.
///
/// Lead search, profile enrichment, saved-lead import, and activity feed methods below
/// remain stubbed — that functionality is tracked separately under INT-003 and is
/// intentionally left untouched here.
///
/// REV-STUB-004 wires the messaging path only: <see cref="SendInMailAsync"/> and
/// <see cref="TestConnectionAsync"/> now make real, OAuth2-authenticated REST calls against
/// LinkedIn's API, with the access token resolved via <see cref="IProviderConfigurationService"/>
/// (the DB-backed, encrypted provider store used by the Admin &gt; Providers UI, category
/// "LinkedIn", provider "SalesNavigator").
/// </summary>
public class LinkedInSalesNavService : ILinkedInSalesNavService
{
    private const string Category = "LinkedIn";
    private const string Provider = "SalesNavigator";
    private const string ApiBaseUrl = "https://api.linkedin.com";

    private readonly IConfiguration _configuration;
    private readonly IProviderConfigurationService _configService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LinkedInSalesNavService> _logger;

    public LinkedInSalesNavService(
        IConfiguration configuration,
        IProviderConfigurationService configService,
        IHttpClientFactory httpClientFactory,
        ILogger<LinkedInSalesNavService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<LinkedInProfile>> SearchProspectsAsync(
        LinkedInSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        _logger.LogInformation(
            "LinkedIn Sales Nav prospect search: title={Title}, company={Company}, location={Location} (stub)",
            query.Title, query.Company, query.Location);

        // Return empty until OAuth2 credentials are configured
        return Task.FromResult<IReadOnlyCollection<LinkedInProfile>>(Array.Empty<LinkedInProfile>());
    }

    /// <inheritdoc />
    public Task<LinkedInEnrichResult> EnrichLeadAsync(int leadId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enrich lead {LeadId} from LinkedIn (stub)", leadId);

        return Task.FromResult(LinkedInEnrichResult.Failed(leadId, "LinkedIn Sales Navigator integration not configured. Set Integrations:LinkedIn:AccessToken."));
    }

    /// <inheritdoc />
    public Task<LinkedInEnrichResult> EnrichContactAsync(int contactId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enrich contact {ContactId} from LinkedIn (stub)", contactId);

        return Task.FromResult(LinkedInEnrichResult.Failed(contactId, "LinkedIn Sales Navigator integration not configured."));
    }

    /// <inheritdoc />
    public Task<LinkedInImportResult> ImportSavedLeadsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Import saved leads from LinkedIn Sales Navigator (stub)");

        return Task.FromResult(new LinkedInImportResult
        {
            TotalProcessed = 0,
            NewLeads = 0,
            Skipped = 0,
            Errors = new[] { "LinkedIn saved-leads import not yet implemented." }
        });
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<LinkedInActivity>> GetRecentActivitiesAsync(
        string linkedInProfileUrl,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetch recent LinkedIn activities for {Profile}, max={Max} (stub)",
            linkedInProfileUrl, maxResults);

        return Task.FromResult<IReadOnlyCollection<LinkedInActivity>>(Array.Empty<LinkedInActivity>());
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await ResolveAccessTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("LinkedIn Sales Navigator access token not configured");
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(LinkedInSalesNavService));
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}/v2/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn connection test failed: HTTP {StatusCode}", (int)response.StatusCode);
                return false;
            }

            _logger.LogInformation("LinkedIn Sales Navigator connection verified.");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error testing LinkedIn connection");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<LinkedInMessageResult> SendInMailAsync(
        string linkedInProfileUrl,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linkedInProfileUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var accessToken = await ResolveAccessTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return LinkedInMessageResult.Failed(
                "LinkedIn access token not configured. Add it under Admin > Providers > LinkedIn > Sales Navigator.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(LinkedInSalesNavService));

            // LinkedIn's messaging REST API requires the recipient's URN (urn:li:person:{id}),
            // not a profile URL. Resolving a public profile URL to a URN requires a partner-scoped
            // People Search/Lookup call not available on a standard token; the caller is expected
            // to supply the URN via linkedInProfileUrl when it is already known (e.g. captured
            // during a prior OAuth-authenticated profile fetch). We pass it through as recipient.
            var payload = JsonSerializer.Serialize(new
            {
                recipients = new[] { linkedInProfileUrl },
                subject,
                body
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/rest/messages")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            // LinkedIn versioned REST APIs require these headers on every call.
            request.Headers.Add("LinkedIn-Version", "202401");
            request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // A 403 here is expected for apps without LinkedIn Marketing/Sales Navigator
                // Partner Program approval — LinkedIn does not offer self-service InMail sending.
                _logger.LogWarning(
                    "LinkedIn SendInMail failed for {Profile}: HTTP {Status} — {Body}",
                    linkedInProfileUrl, (int)response.StatusCode, responseBody);
                return LinkedInMessageResult.Failed(
                    $"LinkedIn message send failed: HTTP {(int)response.StatusCode}. " +
                    "Note: LinkedIn requires Marketing/Sales Navigator Partner Program approval for outbound messaging.");
            }

            string? messageId = null;
            if (response.Headers.TryGetValues("x-restli-id", out var idValues))
            {
                messageId = idValues.FirstOrDefault();
            }

            _logger.LogInformation("LinkedIn message sent to {Profile}.", linkedInProfileUrl);
            return LinkedInMessageResult.Succeeded(messageId ?? Guid.NewGuid().ToString("N"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error sending LinkedIn message to {Profile}", linkedInProfileUrl);
            return LinkedInMessageResult.Failed($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves the LinkedIn OAuth2 access token, preferring the DB-backed provider
    /// configuration (Admin &gt; Providers) and falling back to the legacy
    /// <c>Integrations:LinkedIn:AccessToken</c> appsettings key for backward compatibility.
    /// </summary>
    private async Task<string?> ResolveAccessTokenAsync(CancellationToken cancellationToken)
    {
        var fields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Provider, cancellationToken);
        var token = ProviderConfigReader.GetValueOrDefault(fields, "AccessToken");
        return token ?? _configuration["Integrations:LinkedIn:AccessToken"];
    }

    public Task<LinkedInSearchResult> SearchLeadsAsync(LinkedInSearchQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SearchLeadsAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult(new LinkedInSearchResult { Profiles = new List<LinkedInProfile>(), TotalResults = 0 });
    }

    public Task<LinkedInProfile?> GetProfileAsync(string linkedInUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetProfileAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult<LinkedInProfile?>(null);
    }

    public Task<LinkedInEnrichResult> EnrichContactAsync(int contactId, string linkedInUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("EnrichContactAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult(LinkedInEnrichResult.Failed(contactId, "Not implemented"));
    }

    public Task<LinkedInImportResult> ImportSavedLeadsAsync(string listId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ImportSavedLeadsAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult(new LinkedInImportResult { TotalProcessed = 0 });
    }

    public Task<IReadOnlyList<LinkedInActivity>> GetRecentActivitiesAsync(IReadOnlyList<int> contactIds, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetRecentActivitiesAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult<IReadOnlyList<LinkedInActivity>>(new List<LinkedInActivity>());
    }
}
