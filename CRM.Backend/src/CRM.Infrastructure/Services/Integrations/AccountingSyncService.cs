// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IAccountingSyncService = CRM.Core.Ports.Input.IAccountingSyncService;
using AccountingSyncResult = CRM.Core.Ports.Input.AccountingSyncResult;
using AccountingSyncStatus = CRM.Core.Ports.Input.AccountingSyncStatus;
using AccountingBatchSyncResult = CRM.Core.Ports.Input.AccountingBatchSyncResult;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of IAccountingSyncService for QuickBooks/Xero integration.
/// Implements TODO-INT-08.
///
/// This is a placeholder that logs operations. Actual API calls to
/// QuickBooks or Xero will require their respective SDK packages and
/// OAuth2 authentication flows.
/// </summary>
public class AccountingSyncService : IAccountingSyncService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountingSyncService> _logger;

    public AccountingSyncService(
        IConfiguration configuration,
        ILogger<AccountingSyncService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<AccountingSyncResult> SyncAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Sync account {AccountId} to {Provider} (stub)", accountId, provider);

        // Stub: In production, map CRM Account → QuickBooks Customer / Xero Contact
        _logger.LogWarning("{Provider} sync not implemented. Install the {Provider} SDK to enable.", provider, provider);
        return Task.FromResult(AccountingSyncResult.Failed(
            $"{provider} sync not yet implemented. Install SDK and configure OAuth2 credentials.",
            provider));
    }

    /// <inheritdoc />
    public Task<AccountingSyncResult> SyncInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Sync invoice {InvoiceId} to {Provider} (stub)", invoiceId, provider);

        return Task.FromResult(AccountingSyncResult.Failed(
            $"{provider} invoice sync not yet implemented.",
            provider));
    }

    /// <inheritdoc />
    public Task<AccountingSyncResult> SyncPaymentAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Sync payment {ExternalPaymentId} from {Provider} (stub)", externalPaymentId, provider);

        return Task.FromResult(AccountingSyncResult.Failed(
            $"{provider} payment sync not yet implemented.",
            provider));
    }

    /// <inheritdoc />
    public Task<AccountingSyncStatus?> GetSyncStatusAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting sync status for {EntityType}#{EntityId}", entityType, entityId);

        // No sync data available in stub
        return Task.FromResult<AccountingSyncStatus?>(new AccountingSyncStatus
        {
            EntityType = entityType,
            EntityId = entityId,
            Status = "NotConfigured",
            Provider = GetProvider()
        });
    }

    /// <inheritdoc />
    public Task<AccountingBatchSyncResult> RunBatchSyncAsync(CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        _logger.LogInformation("Running batch sync for {Provider} (stub)", provider);

        return Task.FromResult(new AccountingBatchSyncResult
        {
            TotalProcessed = 0,
            SuccessCount = 0,
            FailureCount = 0,
            SkippedCount = 0,
            Errors = new[] { $"{provider} batch sync not yet implemented." },
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        var apiKey = _configuration[$"Integrations:Accounting:{provider}:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("{Provider} API key not configured", provider);
            return Task.FromResult(false);
        }

        _logger.LogInformation("Test connection to {Provider} (stub - returning false until SDK is installed)", provider);
        return Task.FromResult(false);
    }

    private string GetProvider()
    {
        return _configuration["Integrations:Accounting:Provider"] ?? "QuickBooks";
    }
}
