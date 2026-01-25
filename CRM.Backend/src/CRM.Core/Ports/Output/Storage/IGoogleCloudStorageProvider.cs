using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Storage;

/// <summary>
/// Google Cloud Storage specific provider interface.
/// Extends the base IStorageProvider with GCS-specific operations.
/// </summary>
public interface IGoogleCloudStorageProvider : IStorageProvider
{
    #region Bucket Management

    /// <summary>
    /// Creates a bucket.
    /// </summary>
    Task CreateBucketAsync(string bucketName, GcsBucketOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bucket.
    /// </summary>
    Task DeleteBucketAsync(string bucketName, bool forceDeleteContents = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a bucket exists.
    /// </summary>
    Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all buckets in the project.
    /// </summary>
    Task<IEnumerable<GcsBucketInfo>> ListBucketsAsync(string? projectId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bucket metadata.
    /// </summary>
    Task<GcsBucketMetadata?> GetBucketMetadataAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates bucket metadata.
    /// </summary>
    Task UpdateBucketMetadataAsync(string bucketName, GcsBucketMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current bucket name.
    /// </summary>
    string CurrentBucket { get; }

    /// <summary>
    /// Switches to a different bucket.
    /// </summary>
    void SetBucket(string bucketName);

    #endregion

    #region Resumable Upload

    /// <summary>
    /// Initiates a resumable upload.
    /// </summary>
    Task<string> InitiateResumableUploadAsync(string objectName, GcsUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a chunk in a resumable upload.
    /// </summary>
    Task<GcsUploadProgress> UploadChunkAsync(string uploadUri, Stream content, long offset, long totalSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a resumable upload.
    /// </summary>
    Task<GcsUploadProgress> GetUploadStatusAsync(string uploadUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a resumable upload.
    /// </summary>
    Task CancelResumableUploadAsync(string uploadUri, CancellationToken cancellationToken = default);

    #endregion

    #region Object Versioning

    /// <summary>
    /// Enables versioning on a bucket.
    /// </summary>
    Task EnableVersioningAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables versioning on a bucket.
    /// </summary>
    Task DisableVersioningAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if versioning is enabled.
    /// </summary>
    Task<bool> IsVersioningEnabledAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all versions of an object.
    /// </summary>
    Task<IEnumerable<GcsObjectVersion>> ListObjectVersionsAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a specific version/generation of an object.
    /// </summary>
    Task<Stream?> DownloadVersionAsync(string objectName, long generation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific generation of an object.
    /// </summary>
    Task<bool> DeleteVersionAsync(string objectName, long generation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores an object to a previous generation.
    /// </summary>
    Task<GcsObjectResult> RestoreVersionAsync(string objectName, long generation, CancellationToken cancellationToken = default);

    #endregion

    #region Object Composition

    /// <summary>
    /// Composes multiple objects into one.
    /// </summary>
    Task<GcsObjectResult> ComposeObjectsAsync(IEnumerable<string> sourceObjects, string destinationObject, GcsComposeOptions? options = null, CancellationToken cancellationToken = default);

    #endregion

    #region Lifecycle Management

    /// <summary>
    /// Sets lifecycle rules on a bucket.
    /// </summary>
    Task SetLifecycleRulesAsync(IEnumerable<GcsLifecycleRule> rules, string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets lifecycle rules for a bucket.
    /// </summary>
    Task<IEnumerable<GcsLifecycleRule>> GetLifecycleRulesAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all lifecycle rules.
    /// </summary>
    Task DeleteLifecycleRulesAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    #endregion

    #region Storage Classes

    /// <summary>
    /// Changes the storage class of an object.
    /// </summary>
    Task<bool> SetStorageClassAsync(string objectName, GcsStorageClass storageClass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the default storage class for a bucket.
    /// </summary>
    Task SetDefaultStorageClassAsync(GcsStorageClass storageClass, string? bucketName = null, CancellationToken cancellationToken = default);

    #endregion

    #region Retention and Hold

    /// <summary>
    /// Sets retention policy on a bucket.
    /// </summary>
    Task SetRetentionPolicyAsync(TimeSpan retentionPeriod, string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks a bucket's retention policy.
    /// </summary>
    Task LockRetentionPolicyAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets temporary hold on an object.
    /// </summary>
    Task SetTemporaryHoldAsync(string objectName, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets event-based hold on an object.
    /// </summary>
    Task SetEventBasedHoldAsync(string objectName, bool enabled, CancellationToken cancellationToken = default);

    #endregion

    #region IAM and Access Control

    /// <summary>
    /// Gets IAM policy for a bucket.
    /// </summary>
    Task<GcsIamPolicy> GetBucketIamPolicyAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets IAM policy for a bucket.
    /// </summary>
    Task SetBucketIamPolicyAsync(GcsIamPolicy policy, string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ACL for an object.
    /// </summary>
    Task<IEnumerable<GcsAclEntry>> GetObjectAclAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets ACL for an object.
    /// </summary>
    Task SetObjectAclAsync(string objectName, IEnumerable<GcsAclEntry> aclEntries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes an object public.
    /// </summary>
    Task MakeObjectPublicAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes an object private.
    /// </summary>
    Task MakeObjectPrivateAsync(string objectName, CancellationToken cancellationToken = default);

    #endregion

    #region Notifications (Pub/Sub)

    /// <summary>
    /// Creates a notification configuration.
    /// </summary>
    Task<string> CreateNotificationAsync(GcsNotificationConfig config, string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all notification configurations.
    /// </summary>
    Task<IEnumerable<GcsNotificationInfo>> ListNotificationsAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification configuration.
    /// </summary>
    Task DeleteNotificationAsync(string notificationId, string? bucketName = null, CancellationToken cancellationToken = default);

    #endregion

    #region CORS Configuration

    /// <summary>
    /// Sets CORS configuration for a bucket.
    /// </summary>
    Task SetCorsConfigurationAsync(IEnumerable<GcsCorsRule> corsRules, string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CORS configuration.
    /// </summary>
    Task<IEnumerable<GcsCorsRule>> GetCorsConfigurationAsync(string? bucketName = null, CancellationToken cancellationToken = default);

    #endregion

    #region Customer-Managed Encryption Keys

    /// <summary>
    /// Sets default encryption key for a bucket.
    /// </summary>
    Task SetDefaultEncryptionKeyAsync(string kmsKeyName, string? bucketName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads with customer-supplied encryption key.
    /// </summary>
    Task<GcsObjectResult> UploadWithCsekAsync(string objectName, Stream content, byte[] encryptionKey, GcsUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads with customer-supplied encryption key.
    /// </summary>
    Task<Stream?> DownloadWithCsekAsync(string objectName, byte[] encryptionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a customer-supplied encryption key.
    /// </summary>
    Task RotateCsekAsync(string objectName, byte[] oldKey, byte[] newKey, CancellationToken cancellationToken = default);

    #endregion
}

#region GCS Specific Types

/// <summary>
/// Options for bucket creation.
/// </summary>
public record GcsBucketOptions
{
    public string? Location { get; init; }
    public GcsStorageClass StorageClass { get; init; } = GcsStorageClass.Standard;
    public bool? VersioningEnabled { get; init; }
    public bool? UniformBucketLevelAccess { get; init; }
    public IDictionary<string, string>? Labels { get; init; }
    public string? DefaultKmsKeyName { get; init; }
}

/// <summary>
/// Bucket information.
/// </summary>
public record GcsBucketInfo(
    string Name,
    string Location,
    GcsStorageClass StorageClass,
    DateTimeOffset TimeCreated,
    DateTimeOffset Updated
);

/// <summary>
/// Bucket metadata.
/// </summary>
public record GcsBucketMetadata(
    string Name,
    string Location,
    GcsStorageClass StorageClass,
    bool VersioningEnabled,
    bool? UniformBucketLevelAccess,
    TimeSpan? RetentionPeriod,
    bool? RetentionPolicyLocked,
    string? DefaultKmsKeyName,
    IDictionary<string, string>? Labels,
    DateTimeOffset TimeCreated
);

/// <summary>
/// Options for upload.
/// </summary>
public record GcsUploadOptions
{
    public string? ContentType { get; init; }
    public string? ContentEncoding { get; init; }
    public string? CacheControl { get; init; }
    public string? ContentDisposition { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
    public GcsStorageClass? StorageClass { get; init; }
    public string? PredefinedAcl { get; init; }
    public bool? EventBasedHold { get; init; }
    public bool? TemporaryHold { get; init; }
}

/// <summary>
/// Upload progress.
/// </summary>
public record GcsUploadProgress(
    bool IsComplete,
    long BytesUploaded,
    long? TotalBytes,
    string? ObjectName,
    long? Generation
);

/// <summary>
/// Object version information.
/// </summary>
public record GcsObjectVersion(
    string Name,
    long Generation,
    long Metageneration,
    DateTimeOffset TimeCreated,
    DateTimeOffset Updated,
    long Size,
    bool IsLatest,
    bool IsDeleted
);

/// <summary>
/// Object operation result.
/// </summary>
public record GcsObjectResult(
    bool Success,
    string ObjectName,
    long Generation,
    long Metageneration,
    string? MediaLink,
    string? SelfLink
);

/// <summary>
/// Options for object composition.
/// </summary>
public record GcsComposeOptions
{
    public string? ContentType { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Lifecycle rule.
/// </summary>
public record GcsLifecycleRule
{
    public GcsLifecycleAction Action { get; init; } = new();
    public GcsLifecycleCondition Condition { get; init; } = new();
}

/// <summary>
/// Lifecycle action.
/// </summary>
public record GcsLifecycleAction
{
    public string Type { get; init; } = "Delete";
    public GcsStorageClass? StorageClass { get; init; }
}

/// <summary>
/// Lifecycle condition.
/// </summary>
public record GcsLifecycleCondition
{
    public int? Age { get; init; }
    public DateTimeOffset? CreatedBefore { get; init; }
    public bool? IsLive { get; init; }
    public IEnumerable<GcsStorageClass>? MatchesStorageClass { get; init; }
    public int? NumNewerVersions { get; init; }
    public int? DaysSinceNoncurrentTime { get; init; }
    public int? DaysSinceCustomTime { get; init; }
}

/// <summary>
/// Storage classes.
/// </summary>
public enum GcsStorageClass
{
    Standard,
    Nearline,
    Coldline,
    Archive
}

/// <summary>
/// IAM policy.
/// </summary>
public record GcsIamPolicy(
    int Version,
    IEnumerable<GcsIamBinding> Bindings,
    string? Etag
);

/// <summary>
/// IAM binding.
/// </summary>
public record GcsIamBinding(
    string Role,
    IEnumerable<string> Members,
    GcsIamCondition? Condition
);

/// <summary>
/// IAM condition.
/// </summary>
public record GcsIamCondition(
    string Title,
    string Expression,
    string? Description
);

/// <summary>
/// ACL entry.
/// </summary>
public record GcsAclEntry(
    string Entity,
    string Role
);

/// <summary>
/// Notification configuration.
/// </summary>
public record GcsNotificationConfig
{
    public string Topic { get; init; } = string.Empty;
    public IEnumerable<string>? EventTypes { get; init; }
    public string? ObjectNamePrefix { get; init; }
    public string PayloadFormat { get; init; } = "JSON_API_V1";
    public IDictionary<string, string>? CustomAttributes { get; init; }
}

/// <summary>
/// Notification information.
/// </summary>
public record GcsNotificationInfo(
    string Id,
    string Topic,
    IEnumerable<string> EventTypes,
    string PayloadFormat,
    string? ObjectNamePrefix
);

/// <summary>
/// CORS rule.
/// </summary>
public record GcsCorsRule
{
    public IEnumerable<string>? Origin { get; init; }
    public IEnumerable<string>? Method { get; init; }
    public IEnumerable<string>? ResponseHeader { get; init; }
    public int? MaxAgeSeconds { get; init; }
}

#endregion
