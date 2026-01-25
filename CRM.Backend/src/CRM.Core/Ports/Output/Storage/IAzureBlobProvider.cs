using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Storage;

/// <summary>
/// Azure Blob Storage specific provider interface.
/// Extends the base IStorageProvider with Azure-specific operations.
/// </summary>
public interface IAzureBlobProvider : IStorageProvider
{
    #region Container Management

    /// <summary>
    /// Creates a container.
    /// </summary>
    Task CreateContainerAsync(string containerName, AzureContainerOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a container.
    /// </summary>
    Task DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a container exists.
    /// </summary>
    Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all containers.
    /// </summary>
    Task<IEnumerable<AzureContainerInfo>> ListContainersAsync(string? prefix = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current container name.
    /// </summary>
    string CurrentContainer { get; }

    /// <summary>
    /// Switches to a different container.
    /// </summary>
    void SetContainer(string containerName);

    /// <summary>
    /// Gets container properties.
    /// </summary>
    Task<AzureContainerProperties?> GetContainerPropertiesAsync(string? containerName = null, CancellationToken cancellationToken = default);

    #endregion

    #region Blob Operations

    /// <summary>
    /// Uploads a block blob.
    /// </summary>
    Task<AzureBlobResult> UploadBlockBlobAsync(string blobName, Stream content, AzureBlobUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a page blob.
    /// </summary>
    Task<AzureBlobResult> UploadPageBlobAsync(string blobName, long size, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads an append blob.
    /// </summary>
    Task<AzureBlobResult> CreateAppendBlobAsync(string blobName, AzureBlobUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends data to an append blob.
    /// </summary>
    Task<bool> AppendBlockAsync(string blobName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets blob properties.
    /// </summary>
    Task<AzureBlobProperties?> GetBlobPropertiesAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets blob properties.
    /// </summary>
    Task<bool> SetBlobPropertiesAsync(string blobName, AzureBlobProperties properties, CancellationToken cancellationToken = default);

    #endregion

    #region Block Blob Operations

    /// <summary>
    /// Stages a block for a block blob.
    /// </summary>
    Task StageBlockAsync(string blobName, string blockId, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits staged blocks.
    /// </summary>
    Task<AzureBlobResult> CommitBlockListAsync(string blobName, IEnumerable<string> blockIds, AzureBlobUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of committed blocks.
    /// </summary>
    Task<IEnumerable<AzureBlockInfo>> GetBlockListAsync(string blobName, AzureBlockListType type = AzureBlockListType.Committed, CancellationToken cancellationToken = default);

    #endregion

    #region Snapshots and Versions

    /// <summary>
    /// Creates a snapshot of a blob.
    /// </summary>
    Task<string> CreateSnapshotAsync(string blobName, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists blob snapshots.
    /// </summary>
    Task<IEnumerable<AzureBlobSnapshot>> ListSnapshotsAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a snapshot.
    /// </summary>
    Task<bool> DeleteSnapshotAsync(string blobName, string snapshotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Promotes a snapshot to the current version.
    /// </summary>
    Task<AzureBlobResult> PromoteSnapshotAsync(string blobName, string snapshotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists blob versions (if versioning is enabled).
    /// </summary>
    Task<IEnumerable<AzureBlobVersion>> ListVersionsAsync(string blobName, CancellationToken cancellationToken = default);

    #endregion

    #region Lease Management

    /// <summary>
    /// Acquires a lease on a blob.
    /// </summary>
    Task<AzureLease> AcquireLeaseAsync(string blobName, TimeSpan? duration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a lease on a container.
    /// </summary>
    Task<AzureLease> AcquireContainerLeaseAsync(string? containerName = null, TimeSpan? duration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews a lease.
    /// </summary>
    Task<bool> RenewLeaseAsync(string blobName, string leaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a lease.
    /// </summary>
    Task<bool> ReleaseLeaseAsync(string blobName, string leaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Breaks a lease.
    /// </summary>
    Task<TimeSpan> BreakLeaseAsync(string blobName, TimeSpan? breakPeriod = null, CancellationToken cancellationToken = default);

    #endregion

    #region Access Tiers

    /// <summary>
    /// Sets the access tier for a blob.
    /// </summary>
    Task<bool> SetAccessTierAsync(string blobName, AzureAccessTier tier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the access tier for a blob.
    /// </summary>
    Task<AzureAccessTier?> GetAccessTierAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rehydrates a blob from archive tier.
    /// </summary>
    Task<bool> RehydrateBlobAsync(string blobName, AzureRehydratePriority priority = AzureRehydratePriority.Standard, CancellationToken cancellationToken = default);

    #endregion

    #region Immutability and Legal Hold

    /// <summary>
    /// Sets immutability policy on a blob.
    /// </summary>
    Task SetImmutabilityPolicyAsync(string blobName, DateTimeOffset expiresOn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes immutability policy.
    /// </summary>
    Task DeleteImmutabilityPolicyAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets legal hold on a blob.
    /// </summary>
    Task SetLegalHoldAsync(string blobName, bool enabled, CancellationToken cancellationToken = default);

    #endregion

    #region Container Access Policy

    /// <summary>
    /// Sets container access level.
    /// </summary>
    Task SetContainerAccessAsync(string? containerName, AzurePublicAccessType accessType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets container access level.
    /// </summary>
    Task<AzurePublicAccessType> GetContainerAccessAsync(string? containerName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets stored access policies on a container.
    /// </summary>
    Task SetAccessPoliciesAsync(string? containerName, IEnumerable<AzureStoredAccessPolicy> policies, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stored access policies.
    /// </summary>
    Task<IEnumerable<AzureStoredAccessPolicy>> GetAccessPoliciesAsync(string? containerName = null, CancellationToken cancellationToken = default);

    #endregion

    #region Tags and Index

    /// <summary>
    /// Sets tags on a blob.
    /// </summary>
    Task SetBlobTagsAsync(string blobName, IDictionary<string, string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tags from a blob.
    /// </summary>
    Task<IDictionary<string, string>> GetBlobTagsAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds blobs by tags using a query.
    /// </summary>
    Task<IEnumerable<AzureTaggedBlob>> FindBlobsByTagsAsync(string tagFilterExpression, CancellationToken cancellationToken = default);

    #endregion

    #region Copy Operations

    /// <summary>
    /// Starts an async copy operation.
    /// </summary>
    Task<string> StartCopyAsync(string sourceBlobUrl, string destinationBlobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a copy operation.
    /// </summary>
    Task AbortCopyAsync(string blobName, string copyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets copy status.
    /// </summary>
    Task<AzureCopyStatus?> GetCopyStatusAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a blob synchronously (for small blobs).
    /// </summary>
    Task<AzureBlobResult> CopyFromUrlAsync(string sourceUrl, string destinationBlobName, CancellationToken cancellationToken = default);

    #endregion

    #region Account Information

    /// <summary>
    /// Gets storage account information.
    /// </summary>
    Task<AzureStorageAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets service statistics (for GRS accounts).
    /// </summary>
    Task<AzureServiceStats?> GetServiceStatsAsync(CancellationToken cancellationToken = default);

    #endregion
}

#region Azure Specific Types

/// <summary>
/// Options for container creation.
/// </summary>
public record AzureContainerOptions
{
    public AzurePublicAccessType PublicAccess { get; init; } = AzurePublicAccessType.None;
    public IDictionary<string, string>? Metadata { get; init; }
    public string? DefaultEncryptionScope { get; init; }
}

/// <summary>
/// Container information.
/// </summary>
public record AzureContainerInfo(
    string Name,
    DateTimeOffset? LastModified,
    AzurePublicAccessType PublicAccess,
    IDictionary<string, string>? Metadata
);

/// <summary>
/// Container properties.
/// </summary>
public record AzureContainerProperties(
    string Name,
    DateTimeOffset LastModified,
    string ETag,
    AzurePublicAccessType PublicAccess,
    bool HasLegalHold,
    bool HasImmutabilityPolicy,
    string? DefaultEncryptionScope
);

/// <summary>
/// Options for blob upload.
/// </summary>
public record AzureBlobUploadOptions
{
    public string? ContentType { get; init; }
    public string? ContentEncoding { get; init; }
    public string? ContentLanguage { get; init; }
    public string? CacheControl { get; init; }
    public string? ContentDisposition { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
    public IDictionary<string, string>? Tags { get; init; }
    public AzureAccessTier? AccessTier { get; init; }
}

/// <summary>
/// Result of blob operation.
/// </summary>
public record AzureBlobResult(
    bool Success,
    string BlobName,
    string? ETag,
    DateTimeOffset? LastModified,
    string? VersionId,
    string? EncryptionScope
);

/// <summary>
/// Blob properties.
/// </summary>
public record AzureBlobProperties
{
    public string? ContentType { get; init; }
    public string? ContentEncoding { get; init; }
    public string? ContentLanguage { get; init; }
    public string? CacheControl { get; init; }
    public string? ContentDisposition { get; init; }
    public byte[]? ContentHash { get; init; }
    public long? ContentLength { get; init; }
    public AzureBlobType? BlobType { get; init; }
    public AzureAccessTier? AccessTier { get; init; }
    public DateTimeOffset? LastModified { get; init; }
    public string? ETag { get; init; }
}

/// <summary>
/// Blob types.
/// </summary>
public enum AzureBlobType
{
    BlockBlob,
    PageBlob,
    AppendBlob
}

/// <summary>
/// Block information.
/// </summary>
public record AzureBlockInfo(
    string BlockId,
    long Size
);

/// <summary>
/// Block list types.
/// </summary>
public enum AzureBlockListType
{
    Committed,
    Uncommitted,
    All
}

/// <summary>
/// Blob snapshot information.
/// </summary>
public record AzureBlobSnapshot(
    string BlobName,
    string SnapshotId,
    DateTimeOffset CreatedOn,
    long Size,
    IDictionary<string, string>? Metadata
);

/// <summary>
/// Blob version information.
/// </summary>
public record AzureBlobVersion(
    string BlobName,
    string VersionId,
    DateTimeOffset CreatedOn,
    bool IsCurrentVersion,
    long Size
);

/// <summary>
/// Lease information.
/// </summary>
public record AzureLease(
    string LeaseId,
    AzureLeaseStatus Status,
    AzureLeaseState State,
    TimeSpan? Duration
);

/// <summary>
/// Lease status.
/// </summary>
public enum AzureLeaseStatus
{
    Locked,
    Unlocked
}

/// <summary>
/// Lease state.
/// </summary>
public enum AzureLeaseState
{
    Available,
    Leased,
    Expired,
    Breaking,
    Broken
}

/// <summary>
/// Access tiers.
/// </summary>
public enum AzureAccessTier
{
    Hot,
    Cool,
    Cold,
    Archive
}

/// <summary>
/// Rehydrate priority.
/// </summary>
public enum AzureRehydratePriority
{
    Standard,
    High
}

/// <summary>
/// Public access types.
/// </summary>
public enum AzurePublicAccessType
{
    None,
    BlobContainer,
    Blob
}

/// <summary>
/// Stored access policy.
/// </summary>
public record AzureStoredAccessPolicy(
    string Id,
    DateTimeOffset? StartsOn,
    DateTimeOffset? ExpiresOn,
    string Permissions
);

/// <summary>
/// Tagged blob result.
/// </summary>
public record AzureTaggedBlob(
    string BlobName,
    string ContainerName,
    IDictionary<string, string> Tags
);

/// <summary>
/// Copy status.
/// </summary>
public record AzureCopyStatus(
    string CopyId,
    AzureCopyState State,
    string? Source,
    double? Progress,
    DateTimeOffset? CompletedOn,
    string? StatusDescription
);

/// <summary>
/// Copy states.
/// </summary>
public enum AzureCopyState
{
    Pending,
    Success,
    Aborted,
    Failed
}

/// <summary>
/// Storage account information.
/// </summary>
public record AzureStorageAccountInfo(
    string SkuName,
    string AccountKind,
    bool IsHnsEnabled
);

/// <summary>
/// Service statistics.
/// </summary>
public record AzureServiceStats(
    AzureGeoReplicationStatus GeoReplicationStatus,
    DateTimeOffset? LastSyncTime
);

/// <summary>
/// Geo-replication status.
/// </summary>
public enum AzureGeoReplicationStatus
{
    Live,
    Bootstrap,
    Unavailable
}

#endregion
