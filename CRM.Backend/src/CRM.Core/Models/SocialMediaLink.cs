// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
