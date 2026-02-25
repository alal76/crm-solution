// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Persists a user's custom dashboard widget layout.
/// Implements TODO-PORTAL-05.
/// </summary>
public class UserDashboardLayout : BaseEntity
{
    /// <summary>ID of the user who owns this layout.</summary>
    public int UserId { get; set; }

    /// <summary>JSON-serialised layout configuration (widget positions, sizes, enabled).</summary>
    public string LayoutJson { get; set; } = "{}";

    /// <summary>Human-readable name for the saved layout.</summary>
    public string Name { get; set; } = "Default";

    /// <summary>Whether this is the user's default active layout.</summary>
    public bool IsDefault { get; set; } = true;

    // Navigation
    public User? User { get; set; }
}
