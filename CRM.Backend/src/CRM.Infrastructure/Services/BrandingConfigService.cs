// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.RegularExpressions;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing branding configuration.
/// Handles custom logos, solution names, favicons, and white-label settings with complete audit trail.
/// </summary>
public class BrandingConfigService : IBrandingConfigService
{
    private readonly ICrmDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<BrandingConfigService> _logger;

    // Validation constraints from specification
    private const string AllowedLogoMimeTypes = "image/png,image/jpeg";
    private const string AllowedFaviconMimeTypes = "image/x-icon,image/png,image/vnd.microsoft.icon";
    private const long MaxLogoSizeBytes = 2 * 1024 * 1024; // 2 MB
    private const long MaxFaviconSizeBytes = 500 * 1024; // 500 KB
    private const int MaxSolutionNameLength = 100;
    private const int MinLogoDimension = 200;
    private const int MaxLogoDimension = 500;
    private static readonly int[] AllowedFaviconDimensions = { 32, 64 };
    private static readonly Regex SolutionNameRegex = new("^[A-Za-z0-9 ]+$", RegexOptions.Compiled);

    public BrandingConfigService(
        ICrmDbContext context,
        IFileStorageService fileStorageService,
        ILogger<BrandingConfigService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current branding configuration or creates a default one if none exists.
    /// </summary>
    public async Task<BrandingConfigDto> GetCurrentBrandingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _context.Set<BrandingConfig>()
                .FirstOrDefaultAsync(b => !b.IsDeleted, cancellationToken);

            if (config == null)
            {
                _logger.LogInformation("No branding configuration found, creating default");
                config = new BrandingConfig
                {
                    SolutionName = "CRM Solution",
                    SoftwareLogoPath = "/assets/logo.png",
                    IsCustomBrandingEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Set<BrandingConfig>().Add(config);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current branding configuration");
            throw;
        }
    }

    /// <summary>
    /// Gets branding configuration by ID.
    /// </summary>
    public async Task<BrandingConfigDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _context.Set<BrandingConfig>()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

            return config == null ? null : MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branding configuration with ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Updates the solution name with audit trail.
    /// </summary>
    public async Task<BrandingConfigDto> UpdateSolutionNameAsync(string solutionName, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(solutionName))
                throw new ArgumentException("Solution name cannot be empty", nameof(solutionName));

            if (solutionName.Length > MaxSolutionNameLength)
                throw new ArgumentException($"Solution name cannot exceed {MaxSolutionNameLength} characters", nameof(solutionName));

            var config = await GetOrCreateBrandingConfigAsync(cancellationToken);
            var oldValue = config.SolutionName;

            var normalizedSolutionName = solutionName.Trim();
            if (!SolutionNameRegex.IsMatch(normalizedSolutionName))
                throw new ArgumentException("Solution name can only contain letters, numbers, and spaces", nameof(solutionName));

            config.SolutionName = normalizedSolutionName;
            config.UpdatedAt = DateTime.UtcNow;

            _context.Set<BrandingConfig>().Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Solution name updated from '{OldName}' to '{NewName}' by user {UserId}",
                oldValue, solutionName, userId);

            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating solution name");
            throw;
        }
    }

    /// <summary>
    /// Uploads a custom branding logo with validation and audit trail.
    /// </summary>
    public async Task<BrandingOperationResponse> UploadCustomLogoAsync(UploadLogoRequest request, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the file
            var (isValid, errorMessage) = await ValidateLogoAsync(request.FileName, request.MimeType, request.FileSizeBytes);
            if (!isValid)
            {
                return new BrandingOperationResponse
                {
                    Success = false,
                    Message = errorMessage ?? "Logo validation failed",
                    ValidationErrors = new Dictionary<string, string> { { "logo", errorMessage ?? "Invalid logo file" } }
                };
            }

            // Decode base64 and validate dimensions
            var fileData = Convert.FromBase64String(request.FileContent);
            var (logoDimensionsValid, logoDimensionError) = ValidateLogoDimensions(fileData);
            if (!logoDimensionsValid)
            {
                return new BrandingOperationResponse
                {
                    Success = false,
                    Message = logoDimensionError ?? "Logo dimension validation failed",
                    ValidationErrors = new Dictionary<string, string> { { "logo", logoDimensionError ?? "Invalid logo dimensions" } }
                };
            }

            // Save file
            var filePathUrl = await _fileStorageService.SaveFileAsync(fileData, request.FileName, "branding/logos", cancellationToken);

            // Update configuration
            var config = await GetOrCreateBrandingConfigAsync(cancellationToken);

            // Delete old logo if exists
            if (!string.IsNullOrEmpty(config.CustomLogoPath))
            {
                await _fileStorageService.DeleteFileAsync(config.CustomLogoPath, cancellationToken);
            }

            config.CustomLogoPath = filePathUrl;
            config.CustomLogoFileName = request.FileName;
            config.LastLogoUploadedAt = DateTime.UtcNow;
            config.LastLogoUploadedById = userId;
            config.UpdatedAt = DateTime.UtcNow;

            _context.Set<BrandingConfig>().Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Custom logo '{FileName}' uploaded by user {UserId}",
                request.FileName, userId);

            return new BrandingOperationResponse
            {
                Success = true,
                Message = "Logo uploaded successfully",
                Data = MapToDto(config)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading custom logo");
            return new BrandingOperationResponse
            {
                Success = false,
                Message = $"Error uploading logo: {ex.Message}",
                ValidationErrors = new Dictionary<string, string> { { "logo", ex.Message } }
            };
        }
    }

    /// <summary>
    /// Uploads a favicon for the browser tab with validation and audit trail.
    /// </summary>
    public async Task<BrandingOperationResponse> UploadFaviconAsync(UploadFaviconRequest request, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the file
            var (isValid, errorMessage) = await ValidateFaviconAsync(request.FileName, request.MimeType, request.FileSizeBytes);
            if (!isValid)
            {
                return new BrandingOperationResponse
                {
                    Success = false,
                    Message = errorMessage ?? "Favicon validation failed",
                    ValidationErrors = new Dictionary<string, string> { { "favicon", errorMessage ?? "Invalid favicon file" } }
                };
            }

            // Decode base64 and validate dimensions
            var fileData = Convert.FromBase64String(request.FileContent);
            var (faviconDimensionsValid, faviconDimensionError) = ValidateFaviconDimensions(fileData);
            if (!faviconDimensionsValid)
            {
                return new BrandingOperationResponse
                {
                    Success = false,
                    Message = faviconDimensionError ?? "Favicon dimension validation failed",
                    ValidationErrors = new Dictionary<string, string> { { "favicon", faviconDimensionError ?? "Invalid favicon dimensions" } }
                };
            }

            // Save file
            var filePathUrl = await _fileStorageService.SaveFileAsync(fileData, request.FileName, "branding/favicons", cancellationToken);

            // Update configuration
            var config = await GetOrCreateBrandingConfigAsync(cancellationToken);

            // Delete old favicon if exists
            if (!string.IsNullOrEmpty(config.FaviconPath))
            {
                await _fileStorageService.DeleteFileAsync(config.FaviconPath, cancellationToken);
            }

            config.FaviconPath = filePathUrl;
            config.FaviconFileName = request.FileName;
            config.LastFaviconUploadedAt = DateTime.UtcNow;
            config.LastFaviconUploadedById = userId;
            config.UpdatedAt = DateTime.UtcNow;

            _context.Set<BrandingConfig>().Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Favicon '{FileName}' uploaded by user {UserId}",
                request.FileName, userId);

            return new BrandingOperationResponse
            {
                Success = true,
                Message = "Favicon uploaded successfully",
                Data = MapToDto(config)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading favicon");
            return new BrandingOperationResponse
            {
                Success = false,
                Message = $"Error uploading favicon: {ex.Message}",
                ValidationErrors = new Dictionary<string, string> { { "favicon", ex.Message } }
            };
        }
    }

    /// <summary>
    /// Deletes the custom branding logo.
    /// </summary>
    public async Task<BrandingConfigDto> DeleteCustomLogoAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetOrCreateBrandingConfigAsync(cancellationToken);

            if (!string.IsNullOrEmpty(config.CustomLogoPath))
            {
                await _fileStorageService.DeleteFileAsync(config.CustomLogoPath, cancellationToken);
            }

            config.CustomLogoPath = null;
            config.CustomLogoFileName = null;
            config.UpdatedAt = DateTime.UtcNow;

            _context.Set<BrandingConfig>().Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Custom logo deleted by user {UserId}", userId);

            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom logo");
            throw;
        }
    }

    /// <summary>
    /// Deletes the uploaded favicon.
    /// </summary>
    public async Task<BrandingConfigDto> DeleteFaviconAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetOrCreateBrandingConfigAsync(cancellationToken);

            if (!string.IsNullOrEmpty(config.FaviconPath))
            {
                await _fileStorageService.DeleteFileAsync(config.FaviconPath, cancellationToken);
            }

            config.FaviconPath = null;
            config.FaviconFileName = null;
            config.FaviconDataUrl = null;
            config.UpdatedAt = DateTime.UtcNow;

            _context.Set<BrandingConfig>().Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Favicon deleted by user {UserId}", userId);

            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting favicon");
            throw;
        }
    }

    /// <summary>
    /// Validates a logo file before uploading.
    /// </summary>
    public Task<(bool IsValid, string? ErrorMessage)> ValidateLogoAsync(string fileName, string mimeType, long fileSizeBytes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "File name cannot be empty"));

            if (!AllowedLogoMimeTypes.Split(',').Contains(mimeType, StringComparer.OrdinalIgnoreCase))
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Only PNG and JPEG files are allowed for logos"));

            if (fileSizeBytes > MaxLogoSizeBytes)
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, $"Logo file size cannot exceed {MaxLogoSizeBytes / (1024 * 1024)} MB"));

            if (fileSizeBytes == 0)
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Logo file cannot be empty"));

            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((true, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating logo");
            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Error validating logo"));
        }
    }

    /// <summary>
    /// Validates a favicon file before uploading.
    /// </summary>
    public Task<(bool IsValid, string? ErrorMessage)> ValidateFaviconAsync(string fileName, string mimeType, long fileSizeBytes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "File name cannot be empty"));

            if (!AllowedFaviconMimeTypes.Split(',').Contains(mimeType, StringComparer.OrdinalIgnoreCase))
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Only ICO and PNG files are allowed for favicons"));

            if (fileSizeBytes > MaxFaviconSizeBytes)
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, $"Favicon file size cannot exceed {MaxFaviconSizeBytes / 1024} KB"));

            if (fileSizeBytes == 0)
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Favicon file cannot be empty"));

            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((true, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating favicon");
            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Error validating favicon"));
        }
    }

    private static (bool IsValid, string? ErrorMessage) ValidateLogoDimensions(byte[] fileData)
    {
        try
        {
            using var image = Image.Load(fileData);
            if (image.Width != image.Height)
                return (false, "Logo must be a square image");

            if (image.Width < MinLogoDimension || image.Width > MaxLogoDimension)
            {
                return (false, $"Logo dimensions must be between {MinLogoDimension}x{MinLogoDimension} and {MaxLogoDimension}x{MaxLogoDimension}px");
            }

            return (true, null);
        }
        catch
        {
            return (false, "Unable to read logo dimensions");
        }
    }

    private static (bool IsValid, string? ErrorMessage) ValidateFaviconDimensions(byte[] fileData)
    {
        try
        {
            using var image = Image.Load(fileData);
            if (image.Width != image.Height)
                return (false, "Favicon must be a square image");

            if (!AllowedFaviconDimensions.Contains(image.Width))
                return (false, "Favicon dimensions must be 32x32 or 64x64px");

            return (true, null);
        }
        catch
        {
            return (false, "Unable to read favicon dimensions");
        }
    }

    /// <summary>
    /// Toggles custom branding on or off.
    /// </summary>
    public async Task<BrandingConfigDto> SetCustomBrandingEnabledAsync(bool isEnabled, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetOrCreateBrandingConfigAsync(cancellationToken);
            var previousState = config.IsCustomBrandingEnabled;

            config.IsCustomBrandingEnabled = isEnabled;
            config.UpdatedAt = DateTime.UtcNow;

            _context.Set<BrandingConfig>().Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Custom branding toggled from {PreviousState} to {NewState} by user {UserId}",
                previousState, isEnabled, userId);

            return MapToDto(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling custom branding");
            throw;
        }
    }

    /// <summary>
    /// Gets the branding configuration or creates a new default one if none exists.
    /// </summary>
    private async Task<BrandingConfig> GetOrCreateBrandingConfigAsync(CancellationToken cancellationToken)
    {
        var config = await _context.Set<BrandingConfig>()
            .FirstOrDefaultAsync(b => !b.IsDeleted, cancellationToken);

        if (config == null)
        {
            config = new BrandingConfig
            {
                SolutionName = "CRM Solution",
                SoftwareLogoPath = "/assets/logo.png",
                IsCustomBrandingEnabled = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<BrandingConfig>().Add(config);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return config;
    }

    /// <summary>
    /// Maps entity to DTO.
    /// </summary>
    private static BrandingConfigDto MapToDto(BrandingConfig entity)
    {
        return new BrandingConfigDto
        {
            Id = entity.Id,
            SolutionName = entity.SolutionName,
            CustomLogoPath = entity.CustomLogoPath,
            CustomLogoFileName = entity.CustomLogoFileName,
            FaviconPath = entity.FaviconPath,
            FaviconFileName = entity.FaviconFileName,
            FaviconDataUrl = entity.FaviconDataUrl,
            SoftwareLogoPath = entity.SoftwareLogoPath,
            IsCustomBrandingEnabled = entity.IsCustomBrandingEnabled,
            LastLogoUploadedAt = entity.LastLogoUploadedAt,
            LastFaviconUploadedAt = entity.LastFaviconUploadedAt
        };
    }
}
