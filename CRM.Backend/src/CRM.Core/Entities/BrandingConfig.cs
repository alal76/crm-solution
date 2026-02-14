using System;

namespace CRM.Core.Entities;

/// <summary>
/// Represents the branding configuration for the CRM application.
/// Stores custom logos, solution names, and favicon settings for white-label deployments.
/// </summary>
public class BrandingConfig : BaseEntity
{
    /// <summary>
    /// Gets or sets the custom solution/application name (e.g., "Acme CRM").
    /// Used in header, browser tab title, and throughout the UI.
    /// </summary>
    public string SolutionName { get; set; } = "CRM Solution";

    /// <summary>
    /// Gets or sets the file path to the custom branding logo image.
    /// User-uploaded logo for white-label deployment.
    /// Format: PNG or JPG, dimensions 200x200 to 500x500px, max 2MB.
    /// </summary>
    public string? CustomLogoPath { get; set; }

    /// <summary>
    /// Gets or sets the original filename of the uploaded custom logo.
    /// Used for audit trail and re-download capabilities.
    /// </summary>
    public string? CustomLogoFileName { get; set; }

    /// <summary>
    /// Gets or sets the file path to the favicon (browser tab icon).
    /// User-uploaded favicon for white-label deployment.
    /// Format: ICO or PNG, dimensions 32x32 or 64x64px, max 500KB.
    /// </summary>
    public string? FaviconPath { get; set; }

    /// <summary>
    /// Gets or sets the original filename of the uploaded favicon.
    /// Used for audit trail and re-download capabilities.
    /// </summary>
    public string? FaviconFileName { get; set; }

    /// <summary>
    /// Gets or sets the file path to the static software logo (always visible).
    /// This is the CRM software's own logo that remains unchanged.
    /// Default: /assets/logo.png
    /// </summary>
    public string SoftwareLogoPath { get; set; } = "/assets/logo.png";

    /// <summary>
    /// Gets or sets a value indicating whether custom branding is enabled.
    /// When false, only the software logo displays.
    /// </summary>
    public bool IsCustomBrandingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL or base64 data for favicon display in browser tab.
    /// This is the processed favicon ready for HTML meta tag use.
    /// </summary>
    public string? FaviconDataUrl { get; set; }

    /// <summary>
    /// Gets or sets the upload date/time of the most recent logo.
    /// Used for cache busting and tracking changes.
    /// </summary>
    public DateTime? LastLogoUploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who uploaded the most recent logo.
    /// Used for audit logging.
    /// </summary>
    public int? LastLogoUploadedById { get; set; }

    /// <summary>
    /// Gets or sets the upload date/time of the most recent favicon.
    /// Used for cache busting and tracking changes.
    /// </summary>
    public DateTime? LastFaviconUploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who uploaded the most recent favicon.
    /// Used for audit logging.
    /// </summary>
    public int? LastFaviconUploadedById { get; set; }
}
