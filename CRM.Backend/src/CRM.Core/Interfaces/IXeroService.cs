// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for Xero OAuth2 accounting integration.
/// Implements INT-001: QuickBooks/Xero accounting sync.
/// </summary>
public interface IXeroService
{
    /// <summary>
    /// Builds the Xero OAuth2 authorization URL to redirect the user to.
    /// </summary>
    /// <param name="state">CSRF state token to validate on callback.</param>
    /// <returns>Full authorization URL including required OAuth2 parameters.</returns>
    string GetAuthorizationUrl(string state);

    /// <summary>
    /// Exchanges an authorization code for Xero access and refresh tokens.
    /// Stores tokens in the in-memory token store.
    /// </summary>
    /// <param name="code">Authorization code from Xero callback.</param>
    /// <param name="tenantId">Xero tenant ID from callback query parameter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if token exchange succeeded.</returns>
    Task<bool> ExchangeCodeForTokensAsync(string code, string tenantId, CancellationToken ct = default);

    /// <summary>
    /// Syncs a CRM account to Xero as a Contact entity.
    /// </summary>
    /// <param name="crmAccountId">CRM account ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if integration is disabled or not connected.</returns>
    Task<bool> SyncContactAsync(int crmAccountId, CancellationToken ct = default);

    /// <summary>
    /// Syncs a CRM invoice to Xero as an Invoice entity (type ACCREC).
    /// </summary>
    /// <param name="crmInvoiceId">CRM invoice ID to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if sync succeeded, false if integration is disabled or not connected.</returns>
    Task<bool> SyncInvoiceAsync(int crmInvoiceId, CancellationToken ct = default);

    /// <summary>
    /// Returns the current Xero OAuth2 connection status.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection status including tenant ID and token expiry.</returns>
    Task<XeroConnectionStatus> GetConnectionStatusAsync(CancellationToken ct = default);
}

/// <summary>
/// Xero OAuth2 connection status information.
/// </summary>
public class XeroConnectionStatus
{
    /// <summary>Whether there is an active OAuth2 connection to Xero.</summary>
    public bool IsConnected { get; set; }

    /// <summary>The Xero tenant ID, or null if not connected.</summary>
    public string? TenantId { get; set; }

    /// <summary>When the access token expires, or null if not connected.</summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>The Xero organisation name, or null.</summary>
    public string? OrganisationName { get; set; }
}
