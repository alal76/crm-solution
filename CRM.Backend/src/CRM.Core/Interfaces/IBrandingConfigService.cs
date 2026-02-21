// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for managing branding configuration.
/// Handles custom logos, solution names, favicons, and white-label settings.
/// </summary>
public interface IBrandingConfigService
{
    /// <summary>
    /// Gets the current branding configuration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current branding configuration.</returns>
    Task<BrandingConfigDto> GetCurrentBrandingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets branding configuration by ID.
    /// </summary>
    /// <param name="id">Branding configuration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Branding configuration or null if not found.</returns>
    Task<BrandingConfigDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the solution name.
    /// </summary>
    /// <param name="solutionName">New solution name.</param>
    /// <param name="userId">ID of the user making the change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    Task<BrandingConfigDto> UpdateSolutionNameAsync(string solutionName, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a custom branding logo.
    /// </summary>
    /// <param name="request">Upload request containing file data.</param>
    /// <param name="userId">ID of the user uploading the logo.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result with updated configuration.</returns>
    Task<BrandingOperationResponse> UploadCustomLogoAsync(UploadLogoRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a favicon for the browser tab.
    /// </summary>
    /// <param name="request">Upload request containing file data.</param>
    /// <param name="userId">ID of the user uploading the favicon.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result with updated configuration.</returns>
    Task<BrandingOperationResponse> UploadFaviconAsync(UploadFaviconRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the custom branding logo.
    /// </summary>
    /// <param name="userId">ID of the user deleting the logo.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    Task<BrandingConfigDto> DeleteCustomLogoAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the uploaded favicon.
    /// </summary>
    /// <param name="userId">ID of the user deleting the favicon.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    Task<BrandingConfigDto> DeleteFaviconAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a logo file before uploading.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="mimeType">MIME type of the file.</param>
    /// <param name="fileSizeBytes">Size of the file in bytes.</param>
    /// <returns>Validation result.</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateLogoAsync(string fileName, string mimeType, long fileSizeBytes);

    /// <summary>
    /// Validates a favicon file before uploading.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="mimeType">MIME type of the file.</param>
    /// <param name="fileSizeBytes">Size of the file in bytes.</param>
    /// <returns>Validation result.</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateFaviconAsync(string fileName, string mimeType, long fileSizeBytes);

    /// <summary>
    /// Toggles custom branding on or off.
    /// </summary>
    /// <param name="isEnabled">Whether custom branding should be enabled.</param>
    /// <param name="userId">ID of the user making the change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated branding configuration.</returns>
    Task<BrandingConfigDto> SetCustomBrandingEnabledAsync(bool isEnabled, int userId, CancellationToken cancellationToken = default);
}
