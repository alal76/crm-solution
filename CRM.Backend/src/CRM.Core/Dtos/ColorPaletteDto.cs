// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos
{
    /// <summary>
    /// Data transfer object for color palette configuration.
    /// Contains theme colors for UI customization.
    /// </summary>
    public class ColorPaletteDto
    {
        /// <summary>
        /// Unique identifier for the color palette.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the color palette (e.g., "Dark Blue", "Modern Green").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the color palette.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Primary brand color (hex format: #RRGGBB).
        /// </summary>
        public string PrimaryColor { get; set; } = string.Empty;

        /// <summary>
        /// Secondary brand color (hex format).
        /// </summary>
        public string SecondaryColor { get; set; } = string.Empty;

        /// <summary>
        /// Success state color (hex format).
        /// </summary>
        public string SuccessColor { get; set; } = "#4CAF50";

        /// <summary>
        /// Warning state color (hex format).
        /// </summary>
        public string WarningColor { get; set; } = "#FF9800";

        /// <summary>
        /// Error/danger state color (hex format).
        /// </summary>
        public string ErrorColor { get; set; } = "#F44336";

        /// <summary>
        /// Information state color (hex format).
        /// </summary>
        public string InfoColor { get; set; } = "#2196F3";

        /// <summary>
        /// Background color for light surfaces (hex format).
        /// </summary>
        public string BackgroundLight { get; set; } = "#FFFFFF";

        /// <summary>
        /// Background color for dark surfaces (hex format).
        /// </summary>
        public string BackgroundDark { get; set; } = "#F5F5F5";

        /// <summary>
        /// Text color for light backgrounds (hex format).
        /// </summary>
        public string TextLight { get; set; } = "#000000";

        /// <summary>
        /// Text color for dark backgrounds (hex format).
        /// </summary>
        public string TextDark { get; set; } = "#FFFFFF";

        /// <summary>
        /// Border color (hex format).
        /// </summary>
        public string BorderColor { get; set; } = "#CCCCCC";

        /// <summary>
        /// Indicates if this is the default palette.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Indicates if this palette is active/enabled.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the palette was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the palette was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Category of the color palette (e.g., "Brand", "Theme", "Custom").
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// List of hex color codes in the palette.
        /// </summary>
        public List<string> Colors { get; set; } = new();

        /// <summary>
        /// Indicates if this is a user-defined custom palette.
        /// </summary>
        public bool IsUserDefined { get; set; }
    }

    /// <summary>
    /// Request to create a custom palette
    /// </summary>
    public class CreateCustomPaletteRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Colors { get; set; } = new();
    }
}
