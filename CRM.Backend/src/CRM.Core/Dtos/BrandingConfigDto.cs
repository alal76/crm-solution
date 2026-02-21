// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// Data Transfer Object for BrandingConfig entity.
/// Used to transfer branding configuration data between frontend and backend.
/// </summary>
public class BrandingConfigDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the branding configuration.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the custom solution/application name.
    /// </summary>
    public string SolutionName { get; set; } = "CRM Solution";

    /// <summary>
    /// Gets or sets the file path to the custom branding logo.
    /// </summary>
    public string? CustomLogoPath { get; set; }

    /// <summary>
    /// Gets or sets the file path to the favicon.
    /// </summary>
    public string? FaviconPath { get; set; }

    /// <summary>
    /// Gets or sets the file path to the software logo.
    /// </summary>
    public string SoftwareLogoPath { get; set; } = "/assets/logo.png";

    /// <summary>
    /// Gets or sets a value indicating whether custom branding is enabled.
    /// </summary>
    public bool IsCustomBrandingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the favicon data URL for browser display.
    /// </summary>
    public string? FaviconDataUrl { get; set; }

    /// <summary>
    /// Gets or sets the last date the logo was uploaded.
    /// </summary>
    public DateTime? LastLogoUploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the last date the favicon was uploaded.
    /// </summary>
    public DateTime? LastFaviconUploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the original filename of the custom logo.
    /// </summary>
    public string? CustomLogoFileName { get; set; }

    /// <summary>
    /// Gets or sets the original filename of the favicon.
    /// </summary>
    public string? FaviconFileName { get; set; }
}

/// <summary>
/// Request DTO for uploading a custom logo.
/// </summary>
public class UploadLogoRequest
{
    /// <summary>
    /// Gets or sets the logo file content (base64 encoded).
    /// </summary>
    public string FileContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original filename of the logo.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type of the file (image/png, image/jpeg).
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Request DTO for updating the solution name.
/// </summary>
public class UpdateSolutionNameRequest
{
    /// <summary>
    /// Gets or sets the new solution name.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for uploading a favicon.
/// </summary>
public class UploadFaviconRequest
{
    /// <summary>
    /// Gets or sets the favicon file content (base64 encoded).
    /// </summary>
    public string FileContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original filename of the favicon.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type of the file (image/x-icon, image/png).
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Response DTO for branding operations.
/// </summary>
public class BrandingOperationResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result of the operation.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated branding configuration (if successful).
    /// </summary>
    public BrandingConfigDto? Data { get; set; }

    /// <summary>
    /// Gets or sets validation error details.
    /// </summary>
    public Dictionary<string, string> ValidationErrors { get; set; } = new();
}
