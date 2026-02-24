// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for accounting system synchronization (QuickBooks/Xero).
/// Provides methods for syncing invoices, payments, and customers.
/// </summary>
public interface IAccountingSyncService
{
    /// <summary>
    /// Syncs a customer/account to the accounting system.
    /// </summary>
    /// <param name="accountId">The CRM account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with external ID.</returns>
    Task<AccountingSyncResult> SyncCustomerAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs an invoice to the accounting system.
    /// </summary>
    /// <param name="invoiceId">The CRM invoice ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with external ID.</returns>
    Task<AccountingSyncResult> SyncInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs a payment to the accounting system.
    /// </summary>
    /// <param name="paymentId">The CRM payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with external ID.</returns>
    Task<AccountingSyncResult> SyncPaymentAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sync status for an entity.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sync status.</returns>
    Task<AccountingSyncStatus?> GetSyncStatusAsync(string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports data from the accounting system.
    /// </summary>
    /// <param name="importType">Type of data to import.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Import result.</returns>
    Task<AccountingImportResult> ImportFromAccountingAsync(AccountingImportType importType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the accounting system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connection test result.</returns>
    Task<AccountingConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an accounting sync operation.
/// </summary>
public record AccountingSyncResult
{
    /// <summary>Whether the sync succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>External ID in the accounting system.</summary>
    public string? ExternalId { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Timestamp of the sync.</summary>
    public DateTime SyncedAt { get; init; } = DateTime.UtcNow;

    /// <summary>The accounting provider used.</summary>
    public AccountingProvider Provider { get; init; }
}

/// <summary>
/// Sync status for an entity.
/// </summary>
public record AccountingSyncStatus
{
    /// <summary>Entity type.</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Entity ID.</summary>
    public int EntityId { get; init; }

    /// <summary>External ID in the accounting system.</summary>
    public string? ExternalId { get; init; }

    /// <summary>Last sync timestamp.</summary>
    public DateTime? LastSyncedAt { get; init; }

    /// <summary>Last sync status.</summary>
    public SyncStatus Status { get; init; }

    /// <summary>Error from last sync if any.</summary>
    public string? LastError { get; init; }
}

/// <summary>
/// Status of a sync operation.
/// </summary>
public enum SyncStatus
{
    /// <summary>Never synced.</summary>
    NotSynced = 0,

    /// <summary>Successfully synced.</summary>
    Synced = 1,

    /// <summary>Sync failed.</summary>
    Failed = 2,

    /// <summary>Sync pending.</summary>
    Pending = 3,

    /// <summary>Out of sync (local changes).</summary>
    OutOfSync = 4
}

/// <summary>
/// Result of an accounting import operation.
/// </summary>
public record AccountingImportResult
{
    /// <summary>Whether the import succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Number of records imported.</summary>
    public int ImportedCount { get; init; }

    /// <summary>Number of records skipped.</summary>
    public int SkippedCount { get; init; }

    /// <summary>Number of records with errors.</summary>
    public int ErrorCount { get; init; }

    /// <summary>Error messages.</summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>Import type.</summary>
    public AccountingImportType ImportType { get; init; }
}

/// <summary>
/// Type of data to import from accounting.
/// </summary>
public enum AccountingImportType
{
    /// <summary>Import customers.</summary>
    Customers = 0,

    /// <summary>Import invoices.</summary>
    Invoices = 1,

    /// <summary>Import payments.</summary>
    Payments = 2,

    /// <summary>Import products/items.</summary>
    Products = 3,

    /// <summary>Import all.</summary>
    All = 4
}

/// <summary>
/// Accounting provider.
/// </summary>
public enum AccountingProvider
{
    /// <summary>None configured.</summary>
    None = 0,

    /// <summary>QuickBooks Online.</summary>
    QuickBooks = 1,

    /// <summary>Xero.</summary>
    Xero = 2,

    /// <summary>FreshBooks.</summary>
    FreshBooks = 3,

    /// <summary>Wave.</summary>
    Wave = 4
}

/// <summary>
/// Result of accounting connection test.
/// </summary>
public record AccountingConnectionResult
{
    /// <summary>Whether connection succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Provider connected to.</summary>
    public AccountingProvider Provider { get; init; }

    /// <summary>Company name in accounting system.</summary>
    public string? CompanyName { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }
}
