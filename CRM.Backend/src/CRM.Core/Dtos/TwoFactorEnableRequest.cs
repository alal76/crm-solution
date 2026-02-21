// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for enabling 2FA with secret and backup codes
/// </summary>
public class TwoFactorEnableRequest
{
    /// <summary>
    /// TOTP secret key
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Backup codes for recovery
    /// </summary>
    public List<string> BackupCodes { get; set; } = new();
}
