// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Mailchimp Marketing API v3 contact-list sync service.
/// Implements INT-002: syncs CRM Contacts → Mailchimp audience members.
///
/// Authentication: HTTP Basic Auth with credentials <c>anystring:{apiKey}</c>.
/// Member hash: MD5 of lowercase email address (Mailchimp requirement).
/// </summary>
public class MailchimpService : IMailchimpService
{
    private readonly MailchimpOptions _opts;
    private readonly ICrmDbContext _db;
    private readonly ILogger<MailchimpService> _logger;
    private readonly HttpClient _http;

    public MailchimpService(
        IOptions<MailchimpOptions> opts,
        ICrmDbContext db,
        ILogger<MailchimpService> logger,
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
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.ApiKey))
        {
            _logger.LogDebug("Mailchimp integration is disabled; skipping contact sync.");
            return false;
        }

        try
        {
            var contact = await _db.Contacts.FindAsync(new object[] { crmContactId }, ct);
            if (contact == null)
            {
                _logger.LogWarning("CRM contact {ContactId} not found; skipping Mailchimp sync.", crmContactId);
                return false;
            }

            var email = contact.EmailPrimary;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("CRM contact {ContactId} has no primary email; skipping Mailchimp sync.", crmContactId);
                return false;
            }

            var memberHash = ComputeMd5(email);
            var url = $"{GetBaseUrl()}/lists/{_opts.ListId}/members/{memberHash}";

            var payload = JsonSerializer.Serialize(new
            {
                email_address = email,
                status_if_new = "subscribed",
                merge_fields = new
                {
                    FNAME = contact.FirstName,
                    LNAME = contact.LastName,
                    PHONE = contact.PhonePrimary ?? contact.PhoneMobile ?? string.Empty
                }
            });

            var request = BuildRequest(HttpMethod.Put, url, payload);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Mailchimp sync failed for contact {ContactId}: {Status} — {Error}",
                    crmContactId, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Mailchimp member synced for CRM contact {ContactId}.", crmContactId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing contact {ContactId} to Mailchimp.", crmContactId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  SyncAllContactsAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<int> SyncAllContactsAsync(CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.ApiKey))
        {
            _logger.LogDebug("Mailchimp integration is disabled; skipping bulk contact sync.");
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

            _logger.LogInformation("Mailchimp bulk sync complete: {Synced}/{Total} contacts synced.",
                synced, contacts.Count);
            return synced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Mailchimp bulk contact sync.");
            return 0;
        }
    }

    // ------------------------------------------------------------------ //
    //  UnsubscribeContactAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> UnsubscribeContactAsync(string email, CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.ApiKey))
        {
            _logger.LogDebug("Mailchimp integration is disabled; skipping unsubscribe.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("UnsubscribeContactAsync called with empty email; skipping.");
            return false;
        }

        try
        {
            var memberHash = ComputeMd5(email);
            var url = $"{GetBaseUrl()}/lists/{_opts.ListId}/members/{memberHash}";
            var payload = JsonSerializer.Serialize(new { status = "unsubscribed" });

            var request = BuildRequest(HttpMethod.Put, url, payload);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Mailchimp unsubscribe failed for {Email}: {Status} — {Error}",
                    email, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Mailchimp member unsubscribed for {Email}.", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception unsubscribing {Email} from Mailchimp.", email);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  GetConnectionStatusAsync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<MailchimpConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default)
    {
        if (!_opts.Enabled || string.IsNullOrWhiteSpace(_opts.ApiKey))
        {
            return new MailchimpConnectionStatus { IsConnected = false };
        }

        try
        {
            var url = $"{GetBaseUrl()}/lists/{_opts.ListId}";
            var request = BuildRequest(HttpMethod.Get, url);
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Mailchimp status check failed: {StatusCode}", response.StatusCode);
                return new MailchimpConnectionStatus { IsConnected = false };
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var listName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null;

            int? memberCount = null;
            if (root.TryGetProperty("stats", out var stats) &&
                stats.TryGetProperty("member_count", out var mc))
            {
                memberCount = mc.GetInt32();
            }

            // Mailchimp list response includes account_name at root for GET /lists/{id}
            var accountName = root.TryGetProperty("account_name", out var accProp)
                ? accProp.GetString()
                : null;

            return new MailchimpConnectionStatus
            {
                IsConnected = true,
                ListName = listName,
                MemberCount = memberCount,
                AccountName = accountName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception checking Mailchimp connection status.");
            return new MailchimpConnectionStatus { IsConnected = false };
        }
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    /// <summary>Resolves the Mailchimp API base URL using the configured or parsed server prefix.</summary>
    private string GetBaseUrl()
    {
        var prefix = string.IsNullOrWhiteSpace(_opts.ServerPrefix)
            ? ParseServerPrefix(_opts.ApiKey)
            : _opts.ServerPrefix;

        return $"https://{prefix}.api.mailchimp.com/3.0";
    }

    /// <summary>
    /// Parses the server prefix from the API key (last segment after the final hyphen).
    /// E.g. <c>abc123-us1</c> → <c>us1</c>.
    /// </summary>
    private static string ParseServerPrefix(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return "us1";

        var idx = apiKey.LastIndexOf('-');
        return idx >= 0 ? apiKey[(idx + 1)..] : "us1";
    }

    /// <summary>Computes the MD5 hash of the lower-cased email (Mailchimp member hash).</summary>
    private static string ComputeMd5(string email)
    {
        var bytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());
        var hash = MD5.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>Builds an authenticated HTTP request with optional JSON body.</summary>
    private HttpRequestMessage BuildRequest(HttpMethod method, string url, string? jsonBody = null)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"anystring:{_opts.ApiKey}"));
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (jsonBody != null)
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        return request;
    }
}
