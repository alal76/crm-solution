// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Local file system implementation of IFileStorageService.
/// Stores files in a local directory. Can be extended to use cloud storage (Azure Blob, S3, etc.).
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Store files in wwwroot/uploads directory
        _basePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
        _baseUrl = "/uploads";

        // Ensure directory exists
        try
        {
            Directory.CreateDirectory(_basePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating uploads directory at {BasePath}", _basePath);
        }
    }

    /// <summary>
    /// Saves a file to the local file system.
    /// </summary>
    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string category, CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileContent == null || fileContent.Length == 0)
            {
                throw new ArgumentException("File content cannot be empty", nameof(fileContent));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            // Create category subdirectory
            var categoryPath = Path.Combine(_basePath, category);
            Directory.CreateDirectory(categoryPath);

            // Generate unique filename to avoid collisions
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var filePath = Path.Combine(categoryPath, uniqueFileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, fileContent, cancellationToken);

            // Return relative URL path
            var relativeUrl = $"{_baseUrl}/{category}/{uniqueFileName}".Replace("\\", "/");
            _logger.LogInformation("File saved successfully: {RelativeUrl}", relativeUrl);

            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from the local file system.
    /// </summary>
    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult(false);
            }

            // Convert URL path to file system path
            var localPath = ConvertUrlToLocalPath(filePath);

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
                _logger.LogInformation("File deleted successfully: {LocalPath}", localPath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {LocalPath}", localPath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Gets the public URL for a file.
    /// </summary>
    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        // If already a URL path, return as-is
        if (filePath.StartsWith("/"))
        {
            return filePath;
        }

        // If it's a full local path, convert to URL
        return ConvertLocalPathToUrl(filePath);
    }

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var localPath = ConvertUrlToLocalPath(filePath);
            return Task.FromResult(File.Exists(localPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var localPath = ConvertUrlToLocalPath(filePath);

            if (!File.Exists(localPath))
            {
                return Task.FromResult(null as long?);
            }

            var fileInfo = new FileInfo(localPath);
            return Task.FromResult((long?)fileInfo.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file size: {FilePath}", filePath);
            return Task.FromResult(null as long?);
        }
    }

    /// <summary>
    /// Converts a URL path to a local file system path.
    /// </summary>
    private string ConvertUrlToLocalPath(string urlPath)
    {
        if (string.IsNullOrWhiteSpace(urlPath))
        {
            return string.Empty;
        }

        // Remove leading slash and base URL prefix
        var relativePath = urlPath
            .TrimStart('/')
            .Replace(_baseUrl.TrimStart('/'), string.Empty)
            .TrimStart('/');

        return Path.Combine(_basePath, relativePath);
    }

    /// <summary>
    /// Converts a local file system path to a URL path.
    /// </summary>
    private string ConvertLocalPathToUrl(string localPath)
    {
        if (string.IsNullOrWhiteSpace(localPath))
        {
            return string.Empty;
        }

        var relativePath = localPath
            .Replace(_basePath, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace("\\", "/");

        return $"{_baseUrl}/{relativePath}";
    }
}
