// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

public enum SocialNetwork
{
    Unknown = 0,
    LinkedIn = 1,
    Twitter = 2,
    Facebook = 3,
    Instagram = 4,
    YouTube = 5,
    Other = 99
}

public class SocialAccount : BaseEntity
{
    public SocialNetwork Network { get; set; } = SocialNetwork.Unknown;
    public string HandleOrUrl { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}
