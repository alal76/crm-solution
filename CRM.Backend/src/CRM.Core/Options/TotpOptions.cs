// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Options;

/// <summary>
/// Configuration options for TOTP (RFC 6238) setup and backup codes.
/// </summary>
public class TotpOptions
{
    /// <summary>
    /// Gets or sets the issuer name displayed in authenticator apps.
    /// </summary>
    public string IssuerName { get; set; } = "CRM Solution";

    /// <summary>
    /// Gets or sets the number of minutes before a setup secret expires.
    /// </summary>
    public int SetupExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of backup codes generated per user.
    /// </summary>
    public int BackupCodeCount { get; set; } = 10;
}
