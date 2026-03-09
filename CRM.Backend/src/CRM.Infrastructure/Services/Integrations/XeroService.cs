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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Xero OAuth2 accounting integration service.
/// Implements INT-001: syncs CRM Accounts → Xero Contacts and CRM Invoices → Xero Invoices.
///
/// OAuth2 flow:
///   1. Redirect user to GetAuthorizationUrl()
///   2. Xero calls back to /api/integrations/xero/callback with code (+ tenantId via query)
///   3. ExchangeCodeForTokensAsync() exchanges code for tokens
///   4. SyncContactAsync() / SyncInvoiceAsync() push data to Xero API
///
/// Token storage: in-memory via IntegrationTokenStore (singleton).
/// TODO: persist to DB — see IntegrationTokenStore for details.
/// </summary>
public class XeroService : IXeroService
{
    private const string TokenEndpoint = "https://identity.xero.com/connect/token";
    private const string AuthBaseUrl = "https://login.xero.com/identity/connect/authorize";
    private const string ApiBaseUrl = "https://api.xero.com/api.xro/2.0";
    private const string Scopes = "accounting.contacts accounting.transactions openid profile email offline_access";

    private const string KeyAccessToken = "xero:access_token";
    private const string KeyRefreshToken = "xero:refresh_token";
    private const string KeyTenantId = "xero:tenant_id";
    private const string KeyExpiresAt = "xero:expires_at";

    private readonly XeroOptions _opts;
    private readonly ICrmDbContext _db;
    private readonly ILogger<XeroService> _logger;
    private readonly HttpClient _http;
    private readonly IntegrationTokenStore _tokenStore;

    public XeroService(
        IOptions<XeroOptions> opts,
        ICrmDbContext db,
        ILogger<XeroService> logger,
        HttpClient http,
        IntegrationTokenStore tokenStore)
    {
        _opts = opts.Value;
        _db = db;
        _logger = logger;
        _http = http;
        _tokenStore = tokenStore;
    }

    // ------------------------------------------------------------------ //
    //  OAuth2 Authorization URL
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public string GetAuthorizationUrl(string state)
    {
        var uri = new StringBuilder(AuthBaseUrl)
            .Append("?client_id=").Append(Uri.EscapeDataString(_opts.ClientId))
            .Append("&redirect_uri=").Append(Uri.EscapeDataString(_opts.RedirectUri))
            .Append("&response_type=code")
            .Append("&scope=").Append(Uri.EscapeDataString(Scopes))
            .Append("&state=").Append(Uri.EscapeDataString(state));

        return uri.ToString();
    }

    // ------------------------------------------------------------------ //
    //  Token Exchange
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> ExchangeCodeForTokensAsync(string code, string tenantId, CancellationToken ct = default)
    {
        if (!_opts.Enabled)
        {
            _logger.LogInformation("Xero integration is disabled; skipping token exchange.");
            return false;
        }

        try
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _opts.RedirectUri)
            });

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_opts.ClientId}:{_opts.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = form };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Xero token exchange failed: {StatusCode} — {Error}",
                    response.StatusCode, error);
                return false;
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var root = document.RootElement;

            var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
            var refreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty;
            var expiresIn = root.GetProperty("expires_in").GetInt32();

            _tokenStore.Set(KeyAccessToken, accessToken);
            _tokenStore.Set(KeyRefreshToken, refreshToken);
            _tokenStore.Set(KeyTenantId, tenantId);
            _tokenStore.Set(KeyExpiresAt, DateTime.UtcNow.AddSeconds(expiresIn).ToString("O"));

            _logger.LogInformation("Xero tokens stored for tenant {TenantId}.", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Xero token exchange.");
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  Account → Xero Contact Sync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> SyncContactAsync(int crmAccountId, CancellationToken ct = default)
    {
        if (!_opts.Enabled)
        {
            _logger.LogDebug("Xero integration is disabled; skipping contact sync.");
            return false;
        }

        if (!IsConnected())
        {
            _logger.LogWarning("Xero is not connected; cannot sync account {AccountId}.", crmAccountId);
            return false;
        }

        try
        {
            var account = await _db.Accounts.FindAsync(new object[] { crmAccountId }, ct);
            if (account == null)
            {
                _logger.LogWarning("CRM account {AccountId} not found; skipping Xero sync.", crmAccountId);
                return false;
            }

            var name = account.Category == Core.Entities.AccountCategory.Organization
                ? account.Company
                : $"{account.FirstName} {account.LastName}".Trim();

            var contact = new
            {
                Name = name,
                EmailAddress = account.Email,
                Phones = string.IsNullOrEmpty(account.Phone)
                    ? null
                    : new[] { new { PhoneType = "DEFAULT", PhoneNumber = account.Phone } }
            };

            var payload = JsonSerializer.Serialize(new { Contacts = new[] { contact } });

            var accessToken = await GetAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(accessToken))
                return false;

            var tenantId = _tokenStore.Get(KeyTenantId)!;
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/Contacts")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("Xero-tenant-id", tenantId);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Xero contact sync failed for {AccountId}: {Status} — {Error}",
                    crmAccountId, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Xero contact synced for CRM account {AccountId}.", crmAccountId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing account {AccountId} to Xero.", crmAccountId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  Invoice → Xero Invoice Sync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> SyncInvoiceAsync(int crmInvoiceId, CancellationToken ct = default)
    {
        if (!_opts.Enabled)
        {
            _logger.LogDebug("Xero integration is disabled; skipping invoice sync.");
            return false;
        }

        if (!IsConnected())
        {
            _logger.LogWarning("Xero is not connected; cannot sync invoice {InvoiceId}.", crmInvoiceId);
            return false;
        }

        try
        {
            var invoice = await _db.Invoices.FindAsync(new object[] { crmInvoiceId }, ct);
            if (invoice == null)
            {
                _logger.LogWarning("CRM invoice {InvoiceId} not found; skipping Xero sync.", crmInvoiceId);
                return false;
            }

            var xeroInvoice = new
            {
                Type = "ACCREC",
                Contact = new { ContactID = invoice.AccountId.ToString() },
                DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
                Date = invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                LineItems = new[]
                {
                    new
                    {
                        Description = $"Invoice {invoice.InvoiceNumber}",
                        Quantity = 1,
                        UnitAmount = invoice.TotalAmount,
                        AccountCode = "200"
                    }
                },
                Status = "AUTHORISED"
            };

            var payload = JsonSerializer.Serialize(new { Invoices = new[] { xeroInvoice } });

            var accessToken = await GetAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(accessToken))
                return false;

            var tenantId = _tokenStore.Get(KeyTenantId)!;
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/Invoices")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("Xero-tenant-id", tenantId);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Xero invoice sync failed for {InvoiceId}: {Status} — {Error}",
                    crmInvoiceId, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Xero invoice synced for CRM invoice {InvoiceId}.", crmInvoiceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing invoice {InvoiceId} to Xero.", crmInvoiceId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  Connection Status
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public Task<XeroConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default)
    {
        var connected = IsConnected();
        var status = new XeroConnectionStatus
        {
            IsConnected = connected,
            TenantId = connected ? _tokenStore.Get(KeyTenantId) : null,
            TokenExpiresAt = connected && DateTime.TryParse(_tokenStore.Get(KeyExpiresAt), out var exp)
                ? exp
                : null
        };
        return Task.FromResult(status);
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private bool IsConnected() =>
        _tokenStore.Has(KeyAccessToken) && _tokenStore.Has(KeyTenantId);

    /// <summary>
    /// Returns a valid access token, refreshing it if expired.
    /// </summary>
    private async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        var expiresAtStr = _tokenStore.Get(KeyExpiresAt);
        if (DateTime.TryParse(expiresAtStr, out var expiresAt) && expiresAt > DateTime.UtcNow.AddMinutes(1))
        {
            return _tokenStore.Get(KeyAccessToken);
        }

        return await RefreshAccessTokenAsync(ct);
    }

    private async Task<string?> RefreshAccessTokenAsync(CancellationToken ct)
    {
        var refreshToken = _tokenStore.Get(KeyRefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No Xero refresh token available; re-authentication required.");
            return null;
        }

        try
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_opts.ClientId}:{_opts.ClientSecret}"));

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = form };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Xero token refresh failed: {StatusCode}", response.StatusCode);
                return null;
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var root = document.RootElement;

            var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
            var newRefreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty;
            var expiresIn = root.GetProperty("expires_in").GetInt32();

            _tokenStore.Set(KeyAccessToken, accessToken);
            _tokenStore.Set(KeyRefreshToken, newRefreshToken);
            _tokenStore.Set(KeyExpiresAt, DateTime.UtcNow.AddSeconds(expiresIn).ToString("O"));

            _logger.LogInformation("Xero access token refreshed.");
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception refreshing Xero access token.");
            return null;
        }
    }
}
