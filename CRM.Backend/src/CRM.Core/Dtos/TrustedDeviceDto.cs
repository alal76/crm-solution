// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for trusted device information.
/// </summary>
public class TrustedDeviceDto
{
    public int Id { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? TrustedFromIp { get; set; }
    public DateTime TrustedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
