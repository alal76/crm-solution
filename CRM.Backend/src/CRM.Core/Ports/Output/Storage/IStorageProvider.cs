using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Storage;

/// <summary>
/// Represents the type of storage provider.
/// </summary>
public enum StorageProviderType
{
    Local,
    AzureBlob,
    AwsS3,
    GoogleCloudStorage,
    Minio,
    Sftp
}

/// <summary>
/// Base interface for all storage providers following hexagonal architecture.
/// This port defines the contract that storage adapters must implement.
/// </summary>
public interface IStorageProvider : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the type of the storage provider.
    /// </summary>
    StorageProviderType ProviderType { get; }

    /// <summary>
    /// Gets the provider name as a string.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether the provider is initialized and ready.
    /// </summary>
    bool IsReady { get; }

    #region Basic Operations

    /// <summary>
    /// Uploads a file from a stream.
    /// </summary>
    Task<StorageResult> UploadAsync(string path, Stream content, UploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file from bytes.
    /// </summary>
    Task<StorageResult> UploadAsync(string path, byte[] content, UploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file as a stream.
    /// </summary>
    Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file as bytes.
    /// </summary>
    Task<byte[]?> DownloadBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file to a local path.
    /// </summary>
    Task<bool> DownloadToFileAsync(string remotePath, string localPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple files.
    /// </summary>
    Task<int> DeleteManyAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file to a new location.
    /// </summary>
    Task<bool> CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a file to a new location.
    /// </summary>
    Task<bool> MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    #endregion

    #region File Information

    /// <summary>
    /// Gets file metadata.
    /// </summary>
    Task<StorageFileInfo?> GetFileInfoAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files in a path/directory.
    /// </summary>
    Task<IEnumerable<StorageFileInfo>> ListFilesAsync(string path, ListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files with pagination.
    /// </summary>
    Task<PagedFileResult> ListFilesPagedAsync(string path, string? continuationToken = null, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for files matching a pattern.
    /// </summary>
    Task<IEnumerable<StorageFileInfo>> SearchAsync(string path, string pattern, bool recursive = false, CancellationToken cancellationToken = default);

    #endregion

    #region Directory Operations

    /// <summary>
    /// Creates a directory/folder.
    /// </summary>
    Task<bool> CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a directory and all its contents.
    /// </summary>
    Task<bool> DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists directories in a path.
    /// </summary>
    Task<IEnumerable<string>> ListDirectoriesAsync(string path, CancellationToken cancellationToken = default);

    #endregion

    #region URL Generation

    /// <summary>
    /// Generates a public URL for a file.
    /// </summary>
    Task<string?> GetPublicUrlAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a signed/pre-signed URL for a file.
    /// </summary>
    Task<string> GetSignedUrlAsync(string path, TimeSpan expiration, SignedUrlOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a signed upload URL.
    /// </summary>
    Task<SignedUploadUrl> GetSignedUploadUrlAsync(string path, TimeSpan expiration, SignedUploadOptions? options = null, CancellationToken cancellationToken = default);

    #endregion

    #region Metadata Operations

    /// <summary>
    /// Sets metadata on a file.
    /// </summary>
    Task<bool> SetMetadataAsync(string path, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata from a file.
    /// </summary>
    Task<IDictionary<string, string>> GetMetadataAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the content type of a file.
    /// </summary>
    Task<bool> SetContentTypeAsync(string path, string contentType, CancellationToken cancellationToken = default);

    #endregion

    #region Health and Statistics

    /// <summary>
    /// Performs a health check.
    /// </summary>
    Task<StorageHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets storage statistics.
    /// </summary>
    Task<StorageStatistics?> GetStatisticsAsync(string? path = null, CancellationToken cancellationToken = default);

    #endregion
}

#region Supporting Types

/// <summary>
/// Result of a storage upload operation.
/// </summary>
public record StorageResult(
    bool Success,
    string Path,
    string? Url,
    string? ETag,
    long SizeBytes,
    string? ContentType,
    DateTimeOffset? CreatedAt
);

/// <summary>
/// File information.
/// </summary>
public record StorageFileInfo(
    string Path,
    string Name,
    long SizeBytes,
    string? ContentType,
    string? ETag,
    DateTimeOffset? LastModified,
    DateTimeOffset? CreatedAt,
    bool IsDirectory,
    IDictionary<string, string>? Metadata
);

/// <summary>
/// Paged file listing result.
/// </summary>
public record PagedFileResult(
    IEnumerable<StorageFileInfo> Files,
    string? ContinuationToken,
    bool HasMore
);

/// <summary>
/// Options for uploading files.
/// </summary>
public record UploadOptions
{
    /// <summary>
    /// Content type/MIME type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Cache control header.
    /// </summary>
    public string? CacheControl { get; init; }

    /// <summary>
    /// Content disposition header.
    /// </summary>
    public string? ContentDisposition { get; init; }

    /// <summary>
    /// Whether to overwrite if file exists.
    /// </summary>
    public bool Overwrite { get; init; } = true;

    /// <summary>
    /// Server-side encryption type.
    /// </summary>
    public string? ServerSideEncryption { get; init; }

    /// <summary>
    /// Storage tier/class.
    /// </summary>
    public string? StorageClass { get; init; }
}

/// <summary>
/// Options for listing files.
/// </summary>
public record ListOptions
{
    /// <summary>
    /// Whether to include subdirectories.
    /// </summary>
    public bool Recursive { get; init; } = false;

    /// <summary>
    /// Maximum number of results.
    /// </summary>
    public int? MaxResults { get; init; }

    /// <summary>
    /// File pattern to match.
    /// </summary>
    public string? Pattern { get; init; }

    /// <summary>
    /// Whether to include metadata.
    /// </summary>
    public bool IncludeMetadata { get; init; } = false;
}

/// <summary>
/// Options for signed URLs.
/// </summary>
public record SignedUrlOptions
{
    /// <summary>
    /// HTTP method allowed.
    /// </summary>
    public string HttpMethod { get; init; } = "GET";

    /// <summary>
    /// Content type for uploads.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Content disposition header.
    /// </summary>
    public string? ContentDisposition { get; init; }

    /// <summary>
    /// Response cache control.
    /// </summary>
    public string? ResponseCacheControl { get; init; }
}

/// <summary>
/// Options for signed upload URLs.
/// </summary>
public record SignedUploadOptions
{
    /// <summary>
    /// Allowed content types.
    /// </summary>
    public IEnumerable<string>? AllowedContentTypes { get; init; }

    /// <summary>
    /// Maximum file size in bytes.
    /// </summary>
    public long? MaxSizeBytes { get; init; }

    /// <summary>
    /// Metadata to set on upload.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Signed upload URL with additional information.
/// </summary>
public record SignedUploadUrl(
    string Url,
    string HttpMethod,
    IDictionary<string, string>? RequiredHeaders,
    DateTimeOffset ExpiresAt
);

/// <summary>
/// Health check result.
/// </summary>
public record StorageHealthResult(
    bool IsHealthy,
    string Status,
    TimeSpan ResponseTime,
    string? ErrorMessage = null
);

/// <summary>
/// Storage statistics.
/// </summary>
public record StorageStatistics(
    long TotalSizeBytes,
    long FileCount,
    long DirectoryCount,
    DateTimeOffset? OldestFile,
    DateTimeOffset? NewestFile
);

#endregion

/// <summary>
/// Factory interface for creating storage providers.
/// </summary>
public interface IStorageProviderFactory
{
    /// <summary>
    /// Creates a storage provider based on the specified type.
    /// </summary>
    IStorageProvider CreateProvider(StorageProviderType providerType, string connectionString);

    /// <summary>
    /// Creates a storage provider from configuration.
    /// </summary>
    IStorageProvider CreateFromConfiguration();

    /// <summary>
    /// Gets all supported provider types.
    /// </summary>
    IEnumerable<StorageProviderType> GetSupportedProviders();
}
