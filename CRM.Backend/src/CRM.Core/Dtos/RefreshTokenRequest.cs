// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for refresh token request — exchanges a valid refresh token for a new access + refresh token pair.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The current refresh token to exchange.
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
