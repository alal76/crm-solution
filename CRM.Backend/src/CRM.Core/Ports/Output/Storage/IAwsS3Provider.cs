using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Storage;

/// <summary>
/// AWS S3 specific storage provider interface.
/// Extends the base IStorageProvider with S3-specific operations.
/// </summary>
public interface IAwsS3Provider : IStorageProvider
{
    #region Bucket Management

    /// <summary>
    /// Creates a new bucket.
    /// </summary>
    Task CreateBucketAsync(string bucketName, S3BucketOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bucket.
    /// </summary>
    Task DeleteBucketAsync(string bucketName, bool forceDeleteContents = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a bucket exists.
    /// </summary>
    Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all buckets.
    /// </summary>
    Task<IEnumerable<S3BucketInfo>> ListBucketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bucket location/region.
    /// </summary>
    Task<string> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current bucket name.
    /// </summary>
    string CurrentBucket { get; }

    /// <summary>
    /// Switches to a different bucket.
    /// </summary>
    void SetBucket(string bucketName);

    #endregion

    #region Multipart Upload

    /// <summary>
    /// Initiates a multipart upload.
    /// </summary>
    Task<string> InitiateMultipartUploadAsync(string key, MultipartUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a part in a multipart upload.
    /// </summary>
    Task<S3PartInfo> UploadPartAsync(string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a multipart upload.
    /// </summary>
    Task<StorageResult> CompleteMultipartUploadAsync(string key, string uploadId, IEnumerable<S3PartInfo> parts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a multipart upload.
    /// </summary>
    Task AbortMultipartUploadAsync(string key, string uploadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists ongoing multipart uploads.
    /// </summary>
    Task<IEnumerable<S3MultipartUpload>> ListMultipartUploadsAsync(string? prefix = null, CancellationToken cancellationToken = default);

    #endregion

    #region Object Versioning

    /// <summary>
    /// Enables versioning on the bucket.
    /// </summary>
    Task EnableVersioningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends versioning on the bucket.
    /// </summary>
    Task SuspendVersioningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets versioning status.
    /// </summary>
    Task<S3VersioningStatus> GetVersioningStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all versions of an object.
    /// </summary>
    Task<IEnumerable<S3ObjectVersion>> ListObjectVersionsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a specific version of an object.
    /// </summary>
    Task<Stream?> DownloadVersionAsync(string key, string versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific version of an object.
    /// </summary>
    Task<bool> DeleteVersionAsync(string key, string versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a previous version as the current version.
    /// </summary>
    Task<StorageResult> RestoreVersionAsync(string key, string versionId, CancellationToken cancellationToken = default);

    #endregion

    #region Object Lock and Retention

    /// <summary>
    /// Sets object lock configuration.
    /// </summary>
    Task SetObjectLockAsync(string key, S3ObjectLockConfig lockConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets object lock configuration.
    /// </summary>
    Task<S3ObjectLockConfig?> GetObjectLockAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets retention policy on an object.
    /// </summary>
    Task SetRetentionAsync(string key, S3RetentionMode mode, DateTimeOffset retainUntil, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets legal hold on an object.
    /// </summary>
    Task SetLegalHoldAsync(string key, bool enabled, CancellationToken cancellationToken = default);

    #endregion

    #region Lifecycle Management

    /// <summary>
    /// Sets lifecycle rules for the bucket.
    /// </summary>
    Task SetLifecycleRulesAsync(IEnumerable<S3LifecycleRule> rules, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets lifecycle rules for the bucket.
    /// </summary>
    Task<IEnumerable<S3LifecycleRule>> GetLifecycleRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes lifecycle configuration.
    /// </summary>
    Task DeleteLifecycleRulesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Storage Classes and Glacier

    /// <summary>
    /// Changes the storage class of an object.
    /// </summary>
    Task<bool> ChangeStorageClassAsync(string key, S3StorageClass storageClass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a restore from Glacier.
    /// </summary>
    Task<bool> RestoreFromGlacierAsync(string key, int expirationDays, S3GlacierRetrievalTier tier = S3GlacierRetrievalTier.Standard, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks restore status.
    /// </summary>
    Task<S3RestoreStatus?> GetRestoreStatusAsync(string key, CancellationToken cancellationToken = default);

    #endregion

    #region Access Control

    /// <summary>
    /// Sets ACL on an object.
    /// </summary>
    Task SetObjectAclAsync(string key, S3CannedAcl acl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ACL for an object.
    /// </summary>
    Task<S3AccessControlList> GetObjectAclAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets bucket policy.
    /// </summary>
    Task SetBucketPolicyAsync(string policyJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bucket policy.
    /// </summary>
    Task<string?> GetBucketPolicyAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Tagging

    /// <summary>
    /// Sets tags on an object.
    /// </summary>
    Task SetObjectTagsAsync(string key, IDictionary<string, string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tags for an object.
    /// </summary>
    Task<IDictionary<string, string>> GetObjectTagsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes tags from an object.
    /// </summary>
    Task DeleteObjectTagsAsync(string key, CancellationToken cancellationToken = default);

    #endregion

    #region Server-Side Encryption

    /// <summary>
    /// Sets default encryption for the bucket.
    /// </summary>
    Task SetBucketEncryptionAsync(S3EncryptionConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bucket encryption configuration.
    /// </summary>
    Task<S3EncryptionConfig?> GetBucketEncryptionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Transfer Acceleration

    /// <summary>
    /// Enables transfer acceleration.
    /// </summary>
    Task EnableTransferAccelerationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables transfer acceleration.
    /// </summary>
    Task DisableTransferAccelerationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfer acceleration status.
    /// </summary>
    Task<bool> GetTransferAccelerationStatusAsync(CancellationToken cancellationToken = default);

    #endregion
}

#region S3 Specific Types

/// <summary>
/// Options for bucket creation.
/// </summary>
public record S3BucketOptions
{
    public string? Region { get; init; }
    public bool? ObjectLockEnabled { get; init; }
    public S3CannedAcl? Acl { get; init; }
}

/// <summary>
/// Bucket information.
/// </summary>
public record S3BucketInfo(
    string Name,
    DateTimeOffset CreationDate,
    string? Region
);

/// <summary>
/// Options for multipart upload.
/// </summary>
public record MultipartUploadOptions
{
    public string? ContentType { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
    public S3StorageClass? StorageClass { get; init; }
    public string? ServerSideEncryption { get; init; }
}

/// <summary>
/// Part information for multipart upload.
/// </summary>
public record S3PartInfo(
    int PartNumber,
    string ETag,
    long Size
);

/// <summary>
/// Multipart upload information.
/// </summary>
public record S3MultipartUpload(
    string Key,
    string UploadId,
    DateTimeOffset Initiated,
    string? StorageClass
);

/// <summary>
/// Versioning status.
/// </summary>
public enum S3VersioningStatus
{
    Disabled,
    Enabled,
    Suspended
}

/// <summary>
/// Object version information.
/// </summary>
public record S3ObjectVersion(
    string Key,
    string VersionId,
    bool IsLatest,
    DateTimeOffset LastModified,
    long Size,
    string? ETag,
    bool IsDeleteMarker
);

/// <summary>
/// Object lock configuration.
/// </summary>
public record S3ObjectLockConfig(
    S3ObjectLockMode Mode,
    DateTimeOffset RetainUntilDate
);

/// <summary>
/// Object lock modes.
/// </summary>
public enum S3ObjectLockMode
{
    Governance,
    Compliance
}

/// <summary>
/// Retention modes.
/// </summary>
public enum S3RetentionMode
{
    Governance,
    Compliance
}

/// <summary>
/// Lifecycle rule.
/// </summary>
public record S3LifecycleRule
{
    public string Id { get; init; } = string.Empty;
    public string? Prefix { get; init; }
    public bool Enabled { get; init; } = true;
    public int? ExpirationDays { get; init; }
    public DateTimeOffset? ExpirationDate { get; init; }
    public IEnumerable<S3Transition>? Transitions { get; init; }
    public int? NoncurrentVersionExpirationDays { get; init; }
    public bool? ExpireDeleteMarkers { get; init; }
}

/// <summary>
/// Storage class transition.
/// </summary>
public record S3Transition(
    int Days,
    S3StorageClass StorageClass
);

/// <summary>
/// S3 storage classes.
/// </summary>
public enum S3StorageClass
{
    Standard,
    ReducedRedundancy,
    StandardIA,
    OneZoneIA,
    IntelligentTiering,
    Glacier,
    GlacierInstantRetrieval,
    GlacierDeepArchive
}

/// <summary>
/// Glacier retrieval tiers.
/// </summary>
public enum S3GlacierRetrievalTier
{
    Expedited,
    Standard,
    Bulk
}

/// <summary>
/// Restore status.
/// </summary>
public record S3RestoreStatus(
    bool IsRestoreInProgress,
    DateTimeOffset? RestoreExpiryDate
);

/// <summary>
/// Canned ACLs.
/// </summary>
public enum S3CannedAcl
{
    Private,
    PublicRead,
    PublicReadWrite,
    AuthenticatedRead,
    AwsExecRead,
    BucketOwnerRead,
    BucketOwnerFullControl
}

/// <summary>
/// Access control list.
/// </summary>
public record S3AccessControlList(
    string OwnerId,
    string OwnerDisplayName,
    IEnumerable<S3Grant> Grants
);

/// <summary>
/// ACL grant.
/// </summary>
public record S3Grant(
    string GranteeId,
    string GranteeType,
    string Permission
);

/// <summary>
/// Encryption configuration.
/// </summary>
public record S3EncryptionConfig(
    string Algorithm,
    string? KmsKeyId
);

#endregion
