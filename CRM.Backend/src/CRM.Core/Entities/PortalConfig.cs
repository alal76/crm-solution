// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Customer Portal configuration — singleton table (only one row is expected).
/// </summary>
public class PortalConfig : BaseEntity
{
    /// <summary>Whether the portal is enabled / accessible to customers</summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>Whether customers may self-register without an invite</summary>
    public bool AllowSelfRegistration { get; set; } = true;

    /// <summary>Greeting text shown on the portal dashboard</summary>
    [MaxLength(500)]
    public string? WelcomeMessage { get; set; }

    /// <summary>Support contact email shown in the portal</summary>
    [MaxLength(200)]
    public string? SupportEmail { get; set; }

    /// <summary>URL to the portal logo image</summary>
    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>Primary hex color used for portal branding, e.g. "#1976d2"</summary>
    [MaxLength(20)]
    public string? PrimaryColor { get; set; }

    /// <summary>Comma-separated list of allowed email domains for self-registration</summary>
    [MaxLength(500)]
    public string? AllowedDomains { get; set; }

    /// <summary>Title / name displayed in the portal browser tab and header</summary>
    [MaxLength(100)]
    public string? PortalTitle { get; set; }
}
