// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for QuickBooks Online OAuth2 accounting integration.
/// Implements INT-001: QuickBooks/Xero accounting sync.
/// </summary>
public interface IQuickBooksService
{
    /// <summary>
    /// Builds the Intuit OAuth2 authorization URL to redirect the user to.
    /// </summary>
    /// <param name="state">CSRF state token to validate on callback.</param>
    /// <returns>Full authorization URL including required OAuth2 parameters.</returns>
    string GetAuthorizationUrl(string state);

    /// <summary>
    /// Exchanges an authorization code for QuickBooks access and refresh tokens.
    /// Stores tokens in the in-memory token store.
    /// </summary>
    /// <param name="code">Authorization code from Intuit callback.</param>
    /// <param name="realmId">QuickBooks company realm ID from callback.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if token exchange succeeded.</returns>
    Task<bool> ExchangeCodeForTokensAsync(string code, string realmId, CancellationToken ct = default);

    /// <summary>
    /// Syncs a CRM account to QuickBooks as a Customer entity.
    /// </summary>
    /// <param name="crmAccountId">CRM account ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if integration is disabled or not connected.</returns>
    Task<bool> SyncAccountAsync(int crmAccountId, CancellationToken ct = default);

    /// <summary>
    /// Syncs a CRM invoice to QuickBooks as an Invoice entity.
    /// </summary>
    /// <param name="crmInvoiceId">CRM invoice ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if integration is disabled or not connected.</returns>
    Task<bool> SyncInvoiceAsync(int crmInvoiceId, CancellationToken ct = default);

    /// <summary>
    /// Returns the current QuickBooks OAuth2 connection status.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection status including realm ID and token expiry.</returns>
    Task<QuickBooksConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default);
}

/// <summary>
/// QuickBooks OAuth2 connection status information.
/// </summary>
public class QuickBooksConnectionStatus
{
    /// <summary>Whether there is an active OAuth2 connection to QuickBooks.</summary>
    public bool IsConnected { get; set; }

    /// <summary>The QuickBooks company realm ID, or null if not connected.</summary>
    public string? RealmId { get; set; }

    /// <summary>When the access token expires, or null if not connected.</summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>The QuickBooks company name (from company info endpoint), or null.</summary>
    public string? CompanyName { get; set; }
}
