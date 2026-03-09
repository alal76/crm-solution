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
/// QuickBooks Online OAuth2 accounting integration service.
/// Implements INT-001: syncs CRM Accounts → QB Customers and CRM Invoices → QB Invoices.
///
/// OAuth2 flow:
///   1. Redirect user to GetAuthorizationUrl()
///   2. Intuit calls back to /api/integrations/quickbooks/callback with code + realmId
///   3. ExchangeCodeForTokensAsync() exchanges code for tokens
///   4. SyncAccountAsync() / SyncInvoiceAsync() push data to QuickBooks REST API
///
/// Token storage: in-memory via IntegrationTokenStore (singleton).
/// TODO: persist to DB — see IntegrationTokenStore for details.
/// </summary>
public class QuickBooksService : IQuickBooksService
{
    private const string SandboxBaseUrl = "https://sandbox-quickbooks.api.intuit.com";
    private const string ProductionBaseUrl = "https://quickbooks.api.intuit.com";
    private const string TokenEndpoint = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer";
    private const string AuthBaseUrl = "https://appcenter.intuit.com/connect/oauth2";

    private const string KeyAccessToken = "qb:access_token";
    private const string KeyRefreshToken = "qb:refresh_token";
    private const string KeyRealmId = "qb:realm_id";
    private const string KeyExpiresAt = "qb:expires_at";

    private readonly QuickBooksOptions _opts;
    private readonly ICrmDbContext _db;
    private readonly ILogger<QuickBooksService> _logger;
    private readonly HttpClient _http;
    private readonly IntegrationTokenStore _tokenStore;

    public QuickBooksService(
        IOptions<QuickBooksOptions> opts,
        ICrmDbContext db,
        ILogger<QuickBooksService> logger,
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
            .Append("&scope=com.intuit.quickbooks.accounting")
            .Append("&state=").Append(Uri.EscapeDataString(state));

        return uri.ToString();
    }

    // ------------------------------------------------------------------ //
    //  Token Exchange
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> ExchangeCodeForTokensAsync(string code, string realmId, CancellationToken ct = default)
    {
        if (!_opts.Enabled)
        {
            _logger.LogInformation("QuickBooks integration is disabled; skipping token exchange.");
            return false;
        }

        try
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_opts.ClientId}:{_opts.ClientSecret}"));

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _opts.RedirectUri)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = form
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("QuickBooks token exchange failed: {StatusCode} — {Error}",
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
            _tokenStore.Set(KeyRealmId, realmId);
            _tokenStore.Set(KeyExpiresAt, DateTime.UtcNow.AddSeconds(expiresIn).ToString("O"));

            _logger.LogInformation("QuickBooks tokens stored for realm {RealmId}.", realmId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during QuickBooks token exchange.");
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  Account → QuickBooks Customer Sync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> SyncAccountAsync(int crmAccountId, CancellationToken ct = default)
    {
        if (!_opts.Enabled)
        {
            _logger.LogDebug("QuickBooks integration is disabled; skipping account sync.");
            return false;
        }

        if (!IsConnected())
        {
            _logger.LogWarning("QuickBooks is not connected; cannot sync account {AccountId}.", crmAccountId);
            return false;
        }

        try
        {
            var account = await _db.Accounts.FindAsync(new object[] { crmAccountId }, ct);
            if (account == null)
            {
                _logger.LogWarning("CRM account {AccountId} not found; skipping QB sync.", crmAccountId);
                return false;
            }

            var displayName = account.Category == Core.Entities.AccountCategory.Organization
                ? account.Company
                : $"{account.FirstName} {account.LastName}".Trim();

            var customer = new
            {
                DisplayName = displayName,
                CompanyName = account.Company,
                PrimaryEmailAddr = new { Address = account.Email },
                PrimaryPhone = string.IsNullOrEmpty(account.Phone)
                    ? null
                    : new { FreeFormNumber = account.Phone }
            };

            var payload = JsonSerializer.Serialize(new { Customer = customer });
            var realmId = _tokenStore.Get(KeyRealmId)!;
            var url = $"{GetApiBaseUrl()}/v3/company/{realmId}/customer";

            var accessToken = await GetAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(accessToken))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("QB account sync failed for {AccountId}: {Status} — {Error}",
                    crmAccountId, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("QB customer synced for CRM account {AccountId}.", crmAccountId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing account {AccountId} to QuickBooks.", crmAccountId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  Invoice → QuickBooks Invoice Sync
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public async Task<bool> SyncInvoiceAsync(int crmInvoiceId, CancellationToken ct = default)
    {
        if (!_opts.Enabled)
        {
            _logger.LogDebug("QuickBooks integration is disabled; skipping invoice sync.");
            return false;
        }

        if (!IsConnected())
        {
            _logger.LogWarning("QuickBooks is not connected; cannot sync invoice {InvoiceId}.", crmInvoiceId);
            return false;
        }

        try
        {
            var invoice = await _db.Invoices.FindAsync(new object[] { crmInvoiceId }, ct);
            if (invoice == null)
            {
                _logger.LogWarning("CRM invoice {InvoiceId} not found; skipping QB sync.", crmInvoiceId);
                return false;
            }

            var qbInvoice = new
            {
                CustomerRef = new { value = invoice.AccountId.ToString() },
                TxnDate = invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
                TotalAmt = invoice.TotalAmount,
                Line = new[]
                {
                    new
                    {
                        Amount = invoice.TotalAmount,
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new
                        {
                            ItemRef = new { value = "1", name = "Services" },
                            UnitPrice = invoice.TotalAmount,
                            Qty = 1
                        }
                    }
                }
            };

            var payload = JsonSerializer.Serialize(new { Invoice = qbInvoice });
            var realmId = _tokenStore.Get(KeyRealmId)!;
            var url = $"{GetApiBaseUrl()}/v3/company/{realmId}/invoice";

            var accessToken = await GetAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(accessToken))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("QB invoice sync failed for {InvoiceId}: {Status} — {Error}",
                    crmInvoiceId, response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("QB invoice synced for CRM invoice {InvoiceId}.", crmInvoiceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception syncing invoice {InvoiceId} to QuickBooks.", crmInvoiceId);
            return false;
        }
    }

    // ------------------------------------------------------------------ //
    //  Connection Status
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public Task<QuickBooksConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default)
    {
        var connected = IsConnected();
        var status = new QuickBooksConnectionStatus
        {
            IsConnected = connected,
            RealmId = connected ? _tokenStore.Get(KeyRealmId) : null,
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
        _tokenStore.Has(KeyAccessToken) && _tokenStore.Has(KeyRealmId);

    private string GetApiBaseUrl() =>
        string.Equals(_opts.Environment, "production", StringComparison.OrdinalIgnoreCase)
            ? ProductionBaseUrl
            : SandboxBaseUrl;

    /// <summary>
    /// Returns a valid access token, refreshing it first if it has expired.
    /// Returns null if the refresh also fails.
    /// </summary>
    private async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        var expiresAtStr = _tokenStore.Get(KeyExpiresAt);
        if (DateTime.TryParse(expiresAtStr, out var expiresAt) && expiresAt > DateTime.UtcNow.AddMinutes(1))
        {
            return _tokenStore.Get(KeyAccessToken);
        }

        // Token is expired or missing expiry — try refresh
        return await RefreshAccessTokenAsync(ct);
    }

    private async Task<string?> RefreshAccessTokenAsync(CancellationToken ct)
    {
        var refreshToken = _tokenStore.Get(KeyRefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No QuickBooks refresh token available; re-authentication required.");
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
                _logger.LogError("QB token refresh failed: {StatusCode}", response.StatusCode);
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

            _logger.LogInformation("QuickBooks access token refreshed.");
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception refreshing QuickBooks access token.");
            return null;
        }
    }
}
