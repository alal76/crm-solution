// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// HubSpot CRM API v3 bidirectional contact/deal sync service.
/// Implements INT-002: syncs CRM Contacts → HubSpot Contacts and CRM Opportunities → HubSpot Deals.
///
/// Authentication: Bearer token (private-app access token).
/// Contacts are deduplicated by email before creating (search-then-upsert pattern).
/// </summary>
public class HubSpotService : IHubSpotService
{
    private const string BaseUrl = "https://api.hubapi.com/crm/v3";
    private const string AccountInfoUrl = "https://api.hubapi.com/oauth/v1/access-tokens/";

    private readonly HubSpotOptions _opts;
    private readonly ICrmDbContext _db;
    private readonly ILogger<HubSpotService> _logger;
    private readonly HttpClient _http;

    public HubSpotService(
        IOptions<HubSpotOptions> opts,
        ICrmDbContext db,
        ILogger<HubSpotService> logger,
        HttpClient http)
    {
        _opts = opts.Value;
        _db = db;
        _logger = logger;
        _http = http;
    }

    // ------------------------------------------------------------------ //
    //  SyncContactAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> SyncContactAsync(int crmContactId, CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.AccessToken))
        {
            _logger.LogDebug("HubSpot integration is disabled; skipping contact sync.");
            return false;
        }

        try
        {
            var contact = await _db.Contacts.FindAsync(new object[] { crmContactId }, ct);
            if (contact == null)
            {
                _logger.LogWarning("CRM contact {ContactId} not found; skipping HubSpot sync.", crmContactId);
                return false;
            }

            var email = contact.EmailPrimary;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("CRM contact {ContactId} has no primary email; skipping HubSpot sync.", crmContactId);
                return false;
            }

            var properties = new
            {
                email,
                firstname = contact.FirstName,
                lastname = contact.LastName,
                phone = contact.PhonePrimary ?? contact.PhoneMobile ?? string.Empty
            };

            // Search for existing HubSpot contact by email to avoid duplicates.
            var existingId = await FindHubSpotContactByEmailAsync(email, ct);

            if (existingId != null)
            {
                // PATCH existing contact
                var patchUrl = $"{BaseUrl}/objects/contacts/{existingId}";
                var patchPayload = JsonSerializer.Serialize(new { properties });
                var patchRequest = BuildRequest(new HttpMethod("PATCH"), patchUrl, patchPayload);
                var patchResponse = await _http.SendAsync(patchRequest, ct);

                if (!patchResponse.IsSuccessStatusCode)
                {
                    var error = await patchResponse.Content.ReadAsStringAsync(ct);
                    _logger.LogError("HubSpot PATCH contact {ContactId} failed: {Status} — {Error}",
                        crmContactId, patchResponse.StatusCode, error);
                    return false;
                }
            }
            else
            {
                // POST new contact
                var postUrl = $"{BaseUrl}/objects/contacts";
                var postPayload = JsonSerializer.Serialize(new { properties });
                var postRequest = BuildRequest(HttpMethod.Post, postUrl, postPayload);
                var postResponse = await _http.SendAsync(postRequest, ct);

                if (!postResponse.IsSuccessStatusCode)
                {
                    var error = await postResponse.Content.ReadAsStringAsync(ct);
                    _logger.LogError("HubSpot POST contact {ContactId} failed: {Status} — {Error}",
                        crmContactId, postResponse.StatusCode, error);
                    return false;
                }
            }

            _logger.LogInformation("HubSpot contact synced for CRM contact {ContactId}.", crmContactId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing contact {ContactId} to HubSpot.", crmContactId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  SyncDealAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> SyncDealAsync(int crmOpportunityId, CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.AccessToken))
        {
            _logger.LogDebug("HubSpot integration is disabled; skipping deal sync.");
            return false;
        }

        try
        {
            var opportunity = await _db.Opportunities.FindAsync(new object[] { crmOpportunityId }, ct);
            if (opportunity == null)
            {
                _logger.LogWarning("CRM opportunity {OpportunityId} not found; skipping HubSpot sync.", crmOpportunityId);
                return false;
            }

            var properties = new
            {
                dealname = opportunity.Name,
                amount = opportunity.Amount.ToString("F2"),
                dealstage = opportunity.Stage.ToString().ToLowerInvariant()
            };

            var url = $"{BaseUrl}/objects/deals";
            var payload = JsonSerializer.Serialize(new { properties });
            var request = BuildRequest(HttpMethod.Post, url, payload);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("HubSpot deal sync failed for opportunity {OpportunityId}: {Status} — {Error}",
                    crmOpportunityId, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("HubSpot deal synced for CRM opportunity {OpportunityId}.", crmOpportunityId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing opportunity {OpportunityId} to HubSpot.", crmOpportunityId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  SyncAllContactsAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<int> SyncAllContactsAsync(CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.AccessToken))
        {
            _logger.LogDebug("HubSpot integration is disabled; skipping bulk contact sync.");
            return 0;
        }

        try
        {
            var contacts = await _db.Contacts
                .Where(c => !c.IsMergedDuplicate
                         && c.Status == CRM.Core.Models.ContactStatus.Active
                         && c.EmailPrimary != null
                         && c.EmailPrimary != string.Empty)
                .ToListAsync(ct);

            var synced = 0;
            foreach (var contact in contacts)
            {
                var ok = await SyncContactAsync(contact.Id, ct);
                if (ok) synced++;
            }

            _logger.LogInformation("HubSpot bulk sync complete: {Synced}/{Total} contacts synced.",
                synced, contacts.Count);
            return synced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during HubSpot bulk contact sync.");
            return 0;
        }
    }

    // ------------------------------------------------------------------ //
    //  GetConnectionStatusAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<HubSpotConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.AccessToken))
        {
            return new HubSpotConnectionStatus { IsConnected = false };
        }

        try
        {
            // Probe the contacts endpoint with limit=1 to validate the token cheaply.
            var url = $"{BaseUrl}/objects/contacts?limit=1";
            var request = BuildRequest(HttpMethod.Get, url);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HubSpot status check failed: {StatusCode}", response.StatusCode);
                return new HubSpotConnectionStatus { IsConnected = false };
            }

            // Attempt to retrieve the portal ID from the account info endpoint.
            string? portalId = null;
            string? accountName = null;
            try
            {
                var infoUrl = $"{AccountInfoUrl}{Uri.EscapeDataString(_opts.AccessToken)}";
                var infoRequest = BuildRequest(HttpMethod.Get, infoUrl);
                var infoResponse = await _http.SendAsync(infoRequest, ct);

                if (infoResponse.IsSuccessStatusCode)
                {
                    var infoBody = await infoResponse.Content.ReadAsStringAsync(ct);
                    using var infoDoc = JsonDocument.Parse(infoBody);
                    var infoRoot = infoDoc.RootElement;

                    if (infoRoot.TryGetProperty("hub_id", out var hubId))
                        portalId = hubId.GetRawText();

                    if (infoRoot.TryGetProperty("hub_domain", out var domain))
                        accountName = domain.GetString();
                }
            }
            catch
            {
                // Non-critical; connection is still valid.
            }

            return new HubSpotConnectionStatus
            {
                IsConnected = true,
                PortalId = portalId,
                AccountName = accountName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception checking HubSpot connection status.");
            return new HubSpotConnectionStatus { IsConnected = false };
        }
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Searches HubSpot for a contact by email using the search API.
    /// Returns the HubSpot contact ID if found, or null.
    /// </summary>
    private async Task<string?> FindHubSpotContactByEmailAsync(string email, CancellationToken ct)
    {
        try
        {
            var searchUrl = $"{BaseUrl}/objects/contacts/search";
            var searchPayload = JsonSerializer.Serialize(new
            {
                filters = new[]
                {
                    new
                    {
                        propertyName = "email",
                        @operator = "EQ",
                        value = email
                    }
                },
                limit = 1,
                properties = new[] { "email" }
            });

            var request = BuildRequest(HttpMethod.Post, searchUrl, searchPayload);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.TryGetProperty("results", out var results) &&
                results.GetArrayLength() > 0 &&
                results[0].TryGetProperty("id", out var idProp))
            {
                return idProp.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Builds an authenticated HTTP request with optional JSON body.</summary>
    private HttpRequestMessage BuildRequest(HttpMethod method, string url, string? jsonBody = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opts.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (jsonBody != null)
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        return request;
    }
}
