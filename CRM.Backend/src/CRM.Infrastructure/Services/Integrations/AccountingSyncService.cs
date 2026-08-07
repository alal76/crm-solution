// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net.Http.Headers;
using CRM.Core.Interfaces;
using CRM.Core.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IAccountingSyncService = CRM.Core.Ports.Input.IAccountingSyncService;
using AccountingSyncResult = CRM.Core.Ports.Input.AccountingSyncResult;
using AccountingSyncStatus = CRM.Core.Ports.Input.AccountingSyncStatus;
using AccountingBatchSyncResult = CRM.Core.Ports.Input.AccountingBatchSyncResult;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implements IAccountingSyncService (REV-STUB-002) for QuickBooks/Xero.
///
/// This service is a thin orchestration layer: the real OAuth2 + REST work for
/// QuickBooks and Xero already exists in <see cref="IQuickBooksService"/> and
/// <see cref="IXeroService"/> (INT-001), which this class delegates to rather than
/// re-implementing the OAuth2 token exchange/refresh dance a second time.
///
/// Credentials/active-provider selection is resolved via <see cref="IProviderConfigurationService"/> —
/// the same DB-backed, encrypted store used by the Admin &gt; Providers UI — using the
/// "Accounting" category and "QuickBooks"/"Xero" provider types registered in
/// <c>ProviderRegistryService</c>.
///
/// <see cref="SyncPaymentAsync"/> has no equivalent method on IQuickBooksService/IXeroService
/// (both only push CRM data outward), so it is implemented here as a direct, real read-only
/// REST call against the active provider, reusing the same OAuth2 access token already
/// negotiated and cached by QuickBooksService/XeroService in <see cref="IntegrationTokenStore"/>.
/// </summary>
public class AccountingSyncService : IAccountingSyncService
{
    private const string Category = "Accounting";
    private const string QuickBooks = "QuickBooks";
    private const string Xero = "Xero";

    // Mirrors QuickBooksService's token-store keys (kept in sync intentionally; see IntegrationTokenStore).
    private const string QbKeyAccessToken = "qb:access_token";
    private const string QbKeyRealmId = "qb:realm_id";
    private const string QbSandboxBaseUrl = "https://sandbox-quickbooks.api.intuit.com";
    private const string QbProductionBaseUrl = "https://quickbooks.api.intuit.com";

    // Mirrors XeroService's token-store keys.
    private const string XeroKeyAccessToken = "xero:access_token";
    private const string XeroKeyTenantId = "xero:tenant_id";
    private const string XeroApiBaseUrl = "https://api.xero.com/api.xro/2.0";

    private readonly IProviderConfigurationService _configService;
    private readonly IQuickBooksService _quickBooks;
    private readonly IXeroService _xero;
    private readonly IntegrationTokenStore _tokenStore;
    private readonly ICrmDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountingSyncService> _logger;

    public AccountingSyncService(
        IProviderConfigurationService configService,
        IQuickBooksService quickBooks,
        IXeroService xero,
        IntegrationTokenStore tokenStore,
        ICrmDbContext db,
        IHttpClientFactory httpClientFactory,
        ILogger<AccountingSyncService> logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _quickBooks = quickBooks ?? throw new ArgumentNullException(nameof(quickBooks));
        _xero = xero ?? throw new ArgumentNullException(nameof(xero));
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<AccountingSyncResult> SyncAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        _logger.LogInformation("Syncing account {AccountId} to {Provider}", accountId, provider);

        var ok = provider == QuickBooks
            ? await _quickBooks.SyncAccountAsync(accountId, cancellationToken)
            : await _xero.SyncContactAsync(accountId, cancellationToken);

        return ok
            ? AccountingSyncResult.Succeeded(accountId.ToString(), provider)
            : AccountingSyncResult.Failed($"{provider} account sync failed — verify the integration is enabled, connected (OAuth2), and the account exists.", provider);
    }

    /// <inheritdoc />
    public async Task<AccountingSyncResult> SyncInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        _logger.LogInformation("Syncing invoice {InvoiceId} to {Provider}", invoiceId, provider);

        var ok = provider == QuickBooks
            ? await _quickBooks.SyncInvoiceAsync(invoiceId, cancellationToken)
            : await _xero.SyncInvoiceAsync(invoiceId, cancellationToken);

        return ok
            ? AccountingSyncResult.Succeeded(invoiceId.ToString(), provider)
            : AccountingSyncResult.Failed($"{provider} invoice sync failed — verify the integration is enabled, connected (OAuth2), and the invoice exists.", provider);
    }

    /// <inheritdoc />
    public async Task<AccountingSyncResult> SyncPaymentAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalPaymentId);

        var provider = await ResolveActiveProviderAsync(cancellationToken);
        _logger.LogInformation("Reading payment {ExternalPaymentId} from {Provider}", externalPaymentId, provider);

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(AccountingSyncService));

            if (provider == QuickBooks)
            {
                var accessToken = _tokenStore.Get(QbKeyAccessToken);
                var realmId = _tokenStore.Get(QbKeyRealmId);
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(realmId))
                {
                    return AccountingSyncResult.Failed("QuickBooks is not connected. Complete OAuth2 authorization first.", provider);
                }

                var baseUrl = QbProductionBaseUrl; // Environment is resolved by QuickBooksOptions at connect-time; default to production endpoint shape.
                var url = $"{baseUrl}/v3/company/{realmId}/payment/{Uri.EscapeDataString(externalPaymentId)}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("QuickBooks payment read failed for {PaymentId}: {Status} — {Error}", externalPaymentId, response.StatusCode, error);
                    return AccountingSyncResult.Failed($"QuickBooks payment read failed: HTTP {(int)response.StatusCode}", provider);
                }

                return AccountingSyncResult.Succeeded(externalPaymentId, provider);
            }
            else
            {
                var accessToken = _tokenStore.Get(XeroKeyAccessToken);
                var tenantId = _tokenStore.Get(XeroKeyTenantId);
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(tenantId))
                {
                    return AccountingSyncResult.Failed("Xero is not connected. Complete OAuth2 authorization first.", provider);
                }

                var url = $"{XeroApiBaseUrl}/Payments/{Uri.EscapeDataString(externalPaymentId)}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("Xero-tenant-id", tenantId);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Xero payment read failed for {PaymentId}: {Status} — {Error}", externalPaymentId, response.StatusCode, error);
                    return AccountingSyncResult.Failed($"Xero payment read failed: HTTP {(int)response.StatusCode}", provider);
                }

                return AccountingSyncResult.Succeeded(externalPaymentId, provider);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error reading payment {PaymentId} from {Provider}", externalPaymentId, provider);
            return AccountingSyncResult.Failed($"Network error: {ex.Message}", provider);
        }
    }

    /// <inheritdoc />
    public async Task<AccountingSyncStatus?> GetSyncStatusAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        var connected = provider == QuickBooks
            ? (await _quickBooks.GetConnectionStatusAsync(cancellationToken)).IsConnected
            : (await _xero.GetConnectionStatusAsync(cancellationToken)).IsConnected;

        return new AccountingSyncStatus
        {
            EntityType = entityType,
            EntityId = entityId,
            Status = connected ? "Connected" : "NotConnected",
            Provider = provider
        };
    }

    /// <inheritdoc />
    public async Task<AccountingBatchSyncResult> RunBatchSyncAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        _logger.LogInformation("Running batch sync for {Provider}", provider);

        var connected = provider == QuickBooks
            ? (await _quickBooks.GetConnectionStatusAsync(cancellationToken)).IsConnected
            : (await _xero.GetConnectionStatusAsync(cancellationToken)).IsConnected;

        if (!connected)
        {
            return new AccountingBatchSyncResult
            {
                TotalProcessed = 0,
                SuccessCount = 0,
                FailureCount = 0,
                SkippedCount = 0,
                Errors = new[] { $"{provider} is not connected. Complete OAuth2 authorization via /api/integrations/{provider.ToLowerInvariant()}/connect first." },
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };
        }

        var errors = new List<string>();
        var successCount = 0;
        var failureCount = 0;

        // Bound the batch to a sane page size — this is a best-effort periodic sync, not a full backfill tool.
        var accountIds = await _db.Accounts
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => a.Id)
            .Take(200)
            .ToListAsync(cancellationToken);

        foreach (var accountId in accountIds)
        {
            var result = await SyncAccountAsync(accountId, cancellationToken);
            if (result.Success) successCount++;
            else { failureCount++; errors.Add($"Account {accountId}: {result.ErrorMessage}"); }
        }

        var invoiceIds = await _db.Invoices
            .Where(i => i.Status == Core.Entities.InvoiceStatus.Sent
                     || i.Status == Core.Entities.InvoiceStatus.Viewed
                     || i.Status == Core.Entities.InvoiceStatus.PartiallyPaid
                     || i.Status == Core.Entities.InvoiceStatus.Overdue)
            .OrderByDescending(i => i.UpdatedAt)
            .Select(i => i.Id)
            .Take(200)
            .ToListAsync(cancellationToken);

        foreach (var invoiceId in invoiceIds)
        {
            var result = await SyncInvoiceAsync(invoiceId, cancellationToken);
            if (result.Success) successCount++;
            else { failureCount++; errors.Add($"Invoice {invoiceId}: {result.ErrorMessage}"); }
        }

        return new AccountingBatchSyncResult
        {
            TotalProcessed = accountIds.Count + invoiceIds.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            SkippedCount = 0,
            Errors = errors,
            StartedAt = startedAt,
            CompletedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var provider = await ResolveActiveProviderAsync(cancellationToken);
        return provider == QuickBooks
            ? (await _quickBooks.GetConnectionStatusAsync(cancellationToken)).IsConnected
            : (await _xero.GetConnectionStatusAsync(cancellationToken)).IsConnected;
    }

    /// <summary>
    /// Resolves which accounting provider is active by checking which one has a saved
    /// configuration in the DB-backed provider store. QuickBooks takes precedence if both
    /// are configured. Defaults to QuickBooks (label only) when neither is configured yet.
    /// </summary>
    private async Task<string> ResolveActiveProviderAsync(CancellationToken cancellationToken)
    {
        var qbFields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, QuickBooks, cancellationToken);
        if (ProviderConfigReader.HasRequiredFields(qbFields, "ClientId", "ClientSecret"))
        {
            return QuickBooks;
        }

        var xeroFields = await ProviderConfigReader.ReadFieldsAsync(_configService, Category, Xero, cancellationToken);
        if (ProviderConfigReader.HasRequiredFields(xeroFields, "ClientId", "ClientSecret"))
        {
            return Xero;
        }

        return QuickBooks;
    }
}
