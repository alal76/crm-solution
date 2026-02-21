// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Models;

/// <summary>
/// Represents a social media link for a contact
/// </summary>
public class SocialMediaLink
{
    public int Id { get; set; }

    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    /// <summary>
    /// Type of social media platform
    /// </summary>
    public SocialMediaPlatform Platform { get; set; }

    /// <summary>
    /// URL or handle for the social media profile
    /// </summary>
    [Required]
    [Url]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Username or handle (if applicable)
    /// </summary>
    [MaxLength(100)]
    public string? Handle { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Social media platform enumeration
/// </summary>
public enum SocialMediaPlatform
{
    LinkedIn = 0,
    Twitter = 1,
    Facebook = 2,
    Instagram = 3,
    GitHub = 4,
    Website = 5,
    Other = 6
}
