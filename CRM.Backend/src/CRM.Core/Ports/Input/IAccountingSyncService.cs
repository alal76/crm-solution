// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for QuickBooks/Xero accounting system synchronization.
/// Implements TODO-INT-08: QuickBooks/Xero accounting sync.
/// </summary>
public interface IAccountingSyncService
{
    /// <summary>
    /// Synchronizes a CRM account to the accounting system as a customer.
    /// </summary>
    /// <param name="accountId">The CRM account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the sync operation.</returns>
    Task<AccountingSyncResult> SyncAccountAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes an invoice to the accounting system.
    /// </summary>
    /// <param name="invoiceId">The CRM invoice ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the sync operation.</returns>
    Task<AccountingSyncResult> SyncInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a payment record from the accounting system back to CRM.
    /// </summary>
    /// <param name="externalPaymentId">The external payment ID from accounting system.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the sync operation.</returns>
    Task<AccountingSyncResult> SyncPaymentAsync(string externalPaymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the synchronization status for a CRM entity.
    /// </summary>
    /// <param name="entityType">The CRM entity type (Account, Invoice, Payment).</param>
    /// <param name="entityId">The CRM entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync status information.</returns>
    Task<AccountingSyncStatus?> GetSyncStatusAsync(string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a full synchronization of all pending changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the batch sync operation.</returns>
    Task<AccountingBatchSyncResult> RunBatchSyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the accounting system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the connection is valid.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an accounting sync operation.
/// </summary>
public record AccountingSyncResult
{
    /// <summary>Whether the sync succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>The external ID in the accounting system.</summary>
    public string? ExternalId { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>When the sync occurred.</summary>
    public DateTime SyncedAt { get; init; } = DateTime.UtcNow;

    /// <summary>The accounting provider used.</summary>
    public string Provider { get; init; } = string.Empty;

    public static AccountingSyncResult Succeeded(string externalId, string provider) =>
        new() { Success = true, ExternalId = externalId, Provider = provider };

    public static AccountingSyncResult Failed(string error, string provider) =>
        new() { Success = false, ErrorMessage = error, Provider = provider };
}

/// <summary>
/// Status of a synced entity.
/// </summary>
public record AccountingSyncStatus
{
    /// <summary>CRM entity type.</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>CRM entity ID.</summary>
    public int EntityId { get; init; }

    /// <summary>External ID in accounting system.</summary>
    public string? ExternalId { get; init; }

    /// <summary>Last successful sync timestamp.</summary>
    public DateTime? LastSyncedAt { get; init; }

    /// <summary>Current sync status.</summary>
    public string Status { get; init; } = "NotSynced";

    /// <summary>Provider name.</summary>
    public string Provider { get; init; } = string.Empty;
}

/// <summary>
/// Result of a batch sync operation.
/// </summary>
public record AccountingBatchSyncResult
{
    /// <summary>Total entities processed.</summary>
    public int TotalProcessed { get; init; }

    /// <summary>Successfully synced count.</summary>
    public int SuccessCount { get; init; }

    /// <summary>Failed sync count.</summary>
    public int FailureCount { get; init; }

    /// <summary>Skipped count.</summary>
    public int SkippedCount { get; init; }

    /// <summary>Details of failures.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>When the batch sync started.</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>When the batch sync completed.</summary>
    public DateTime CompletedAt { get; init; }
}
