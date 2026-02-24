// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Stored WebAuthn credential for passwordless authentication.
/// </summary>
public class WebAuthnCredential : BaseEntity
{
    public int UserId { get; set; }
    public string CredentialId { get; set; } = string.Empty;
    public byte[] CredentialIdBytes { get; set; } = Array.Empty<byte>();
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
    public long SignatureCounter { get; set; }
    public string AttestationFormat { get; set; } = string.Empty;
    public List<string>? Transports { get; set; }
    public string Name { get; set; } = string.Empty;

    // Biometric auth properties (TODO-AUTH-010)
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public bool IsPlatformCredential { get; set; } = false;
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public long SignCount { get; set; }

    // Navigation
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
