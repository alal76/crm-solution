// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a trusted device for 2FA bypass (TODO-AUTH-019).
/// When a user authenticates with 2FA and trusts the device, subsequent logins
/// from that device can skip 2FA within the trust period.
/// </summary>
public class TrustedDevice : BaseEntity
{
    /// <summary>
    /// The user who trusts this device
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Unique device identifier (fingerprint or generated token)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Device name/description for user reference (e.g., "Chrome on Windows")
    /// </summary>
    [MaxLength(255)]
    public string? DeviceName { get; set; }

    /// <summary>
    /// User agent string of the device
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP address when device was trusted
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// When this device trust was last used for 2FA bypass
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// When this device trust expires (typically 30 days)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Hash of device fingerprint for additional verification
    /// </summary>
    [MaxLength(128)]
    public string? FingerprintHash { get; set; }

    // Navigation property
    public virtual User? User { get; set; }
}
