// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Merge Service Interface

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

#region Request/Response Types

/// <summary>
/// Request to merge multiple records
/// </summary>
public class MergeRequest
{
    /// <summary>Entity type (Lead, Contact, Account)</summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>ID of the master/surviving record</summary>
    public int MasterRecordId { get; set; }
    
    /// <summary>IDs of records to merge into master</summary>
    public List<int> RecordsToMerge { get; set; } = new();
    
    /// <summary>
    /// Override which record provides specific field values
    /// Key: field name, Value: record ID to use for that field
    /// </summary>
    public Dictionary<string, int>? FieldSourceOverrides { get; set; }
    
    /// <summary>Whether to re-link related records to master</summary>
    public bool RelinkRelatedRecords { get; set; } = true;
    
    /// <summary>Notes about the merge</summary>
    public string? Notes { get; set; }
    
    /// <summary>User performing the merge</summary>
    public int UserId { get; set; }
}

/// <summary>
/// Result of a merge operation
/// </summary>
public class MergeResult
{
    /// <summary>Whether the merge was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>ID of the merge group created</summary>
    public int MergeGroupId { get; set; }
    
    /// <summary>ID of the master/surviving record</summary>
    public int MasterRecordId { get; set; }
    
    /// <summary>Number of records merged</summary>
    public int RecordsMerged { get; set; }
    
    /// <summary>Number of related records re-linked</summary>
    public int RelatedRecordsRelinked { get; set; }
    
    /// <summary>Warning messages</summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to unmerge records
/// </summary>
public class UnmergeRequest
{
    /// <summary>ID of the merge group to unmerge</summary>
    public int MergeGroupId { get; set; }
    
    /// <summary>Specific record IDs to restore (null = all)</summary>
    public List<int>? SpecificRecordsToRestore { get; set; }
    
    /// <summary>Whether to restore related records</summary>
    public bool RestoreRelatedRecords { get; set; } = true;
    
    /// <summary>Notes about the unmerge</summary>
    public string? Notes { get; set; }
    
    /// <summary>User performing the unmerge</summary>
    public int UserId { get; set; }
}

/// <summary>
/// Result of an unmerge operation
/// </summary>
public class UnmergeResult
{
    /// <summary>Whether the unmerge was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>IDs of restored records</summary>
    public List<int> RestoredRecordIds { get; set; } = new();
    
    /// <summary>Number of related records restored</summary>
    public int RelatedRecordsRestored { get; set; }
    
    /// <summary>Warning messages</summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Preview of merge before execution
/// </summary>
public class MergePreview
{
    /// <summary>Preview of the final merged record</summary>
    public Dictionary<string, object?> PreviewRecord { get; set; } = new();
    
    /// <summary>Field-by-field preview of merge sources</summary>
    public Dictionary<string, FieldMergePreview> FieldPreviews { get; set; } = new();
    
    /// <summary>Related records that would be relinked</summary>
    public List<RelatedRecordPreview> RelatedRecords { get; set; } = new();
    
    /// <summary>Warning messages</summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Preview of how a field will be merged
/// </summary>
public class FieldMergePreview
{
    /// <summary>Field name</summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>Display label</summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>Final value after merge</summary>
    public string? FinalValue { get; set; }
    
    /// <summary>ID of record the value comes from</summary>
    public int SourceRecordId { get; set; }
    
    /// <summary>All values from all records</summary>
    public Dictionary<int, string?> AllValues { get; set; } = new();
}

/// <summary>
/// Preview of related records that would be relinked
/// </summary>
public class RelatedRecordPreview
{
    /// <summary>Type of related entity</summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>Number of records that would be relinked</summary>
    public int Count { get; set; }
    
    /// <summary>Description for display</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Information about a merged record
/// </summary>
public class MergedRecordInfo
{
    public int RecordId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public bool IsMaster { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime MergedAt { get; set; }
    public object? RecordSnapshot { get; set; }
}

/// <summary>
/// Merge group information
/// </summary>
public class MergeGroupInfo
{
    public int Id { get; set; }
    public string GroupIdentifier { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int MasterRecordId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime MergedAt { get; set; }
    public string? MergedByName { get; set; }
    public DateTime? UnmergedAt { get; set; }
    public string? Notes { get; set; }
    public List<MergedRecordInfo> Members { get; set; } = new();
}

#endregion

/// <summary>
/// Service for merging and unmerging duplicate records
/// </summary>
public interface IMergeService
{
    /// <summary>
    /// Merge multiple records into a master record
    /// </summary>
    Task<MergeResult> MergeRecordsAsync(MergeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unmerge previously merged records (restore from soft delete)
    /// </summary>
    Task<UnmergeResult> UnmergeRecordsAsync(UnmergeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Preview what a merge would look like without executing
    /// </summary>
    Task<MergePreview> PreviewMergeAsync(MergeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get merge history for a record
    /// </summary>
    Task<IEnumerable<MergeGroupInfo>> GetMergeHistoryAsync(int recordId, string entityType);

    /// <summary>
    /// Get records that were merged into a master record
    /// </summary>
    Task<IEnumerable<MergedRecordInfo>> GetMergedRecordsAsync(int masterRecordId, string entityType);

    /// <summary>
    /// Get merge group by ID
    /// </summary>
    Task<MergeGroupInfo?> GetMergeGroupAsync(int mergeGroupId);
}
