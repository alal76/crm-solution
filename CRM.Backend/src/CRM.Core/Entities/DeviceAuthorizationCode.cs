// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a device authorization code for OAuth device flow (RFC 8628) (TODO-AUTH-023).
/// Used for devices with limited input capabilities (smart TVs, CLI tools, etc.).
/// </summary>
public class DeviceAuthorizationCode : BaseEntity
{
    /// <summary>
    /// The device code sent to the device for polling
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>
    /// Short user-friendly code displayed on device for user to enter
    /// </summary>
    [Required]
    [MaxLength(12)]
    public string UserCode { get; set; } = string.Empty;

    /// <summary>
    /// Client ID requesting authorization
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Requested scopes (space-separated)
    /// </summary>
    [MaxLength(500)]
    public string? Scope { get; set; }

    /// <summary>
    /// When this code expires (typically 15 minutes)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Poll interval in seconds (typically 5)
    /// </summary>
    public int Interval { get; set; } = 5;

    /// <summary>
    /// Whether the user has authorized this request
    /// </summary>
    public bool IsAuthorized { get; set; } = false;

    /// <summary>
    /// Whether the code has been used/redeemed
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Whether the user explicitly denied authorization
    /// </summary>
    public bool IsDenied { get; set; } = false;

    /// <summary>
    /// User ID (set when user authorizes)
    /// </summary>
    public int? AuthorizedUserId { get; set; }

    /// <summary>
    /// Timestamp when user authorized
    /// </summary>
    public DateTime? AuthorizedAt { get; set; }

    // Navigation property
    public virtual User? AuthorizedUser { get; set; }
}
