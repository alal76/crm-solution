/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Cached color palette from YourPalettes GitHub repository
/// </summary>
public class ColorPalette : BaseEntity
{
    /// <summary>
    /// Unique name of the palette
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the palette (e.g., Trending, Earthy, Pastel, Vibrant, Neon)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }
    
    /// <summary>
    /// First color (hex)
    /// </summary>
    [MaxLength(10)]
    public string Color1 { get; set; } = "#000000";
    
    /// <summary>
    /// Second color (hex)
    /// </summary>
    [MaxLength(10)]
    public string Color2 { get; set; } = "#333333";
    
    /// <summary>
    /// Third color (hex)
    /// </summary>
    [MaxLength(10)]
    public string Color3 { get; set; } = "#666666";
    
    /// <summary>
    /// Fourth color (hex)
    /// </summary>
    [MaxLength(10)]
    public string Color4 { get; set; } = "#999999";
    
    /// <summary>
    /// Fifth color (hex)
    /// </summary>
    [MaxLength(10)]
    public string Color5 { get; set; } = "#CCCCCC";
    
    /// <summary>
    /// Whether this is a user-defined custom palette (not from GitHub)
    /// </summary>
    public bool IsUserDefined { get; set; } = false;
    
    /// <summary>
    /// User ID who created this palette (for user-defined palettes)
    /// </summary>
    public int? CreatedByUserId { get; set; }
}
