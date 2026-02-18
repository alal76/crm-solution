// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for 2FA login verification
/// </summary>
public class TwoFactorLoginRequest
{
    /// <summary>
    /// Temporary token received from initial login response
    /// </summary>
    public string TwoFactorToken { get; set; } = string.Empty;

    /// <summary>
    /// TOTP code from authenticator app or backup code
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
