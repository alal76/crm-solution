// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

#region Duplicate Detection Enumerations

/// <summary>
/// FUNCTIONAL: Action to take when duplicate is detected.
/// TECHNICAL: Controls UI behavior and workflow.
/// </summary>
public enum DuplicateAction
{
    /// <summary>Allow creation with warning</summary>
    Warn = 0,

    /// <summary>Block creation of duplicate</summary>
    Block = 1,

    /// <summary>Auto-merge with existing record</summary>
    AutoMerge = 2,

    /// <summary>Queue for manual review</summary>
    QueueForReview = 3,

    /// <summary>Log only, no user action</summary>
    LogOnly = 4
}

/// <summary>
/// FUNCTIONAL: How to match field values.
/// TECHNICAL: Determines comparison algorithm.
/// </summary>
public enum MatchType
{
    /// <summary>Exact match required</summary>
    Exact = 0,

    /// <summary>Fuzzy/approximate match</summary>
    Fuzzy = 1,

    /// <summary>Phonetic match (sounds like)</summary>
    Phonetic = 2,

    /// <summary>Contains the value</summary>
    Contains = 3,

    /// <summary>Starts with value</summary>
    StartsWith = 4,

    /// <summary>Normalized match (case/accent insensitive)</summary>
    Normalized = 5,

    /// <summary>Domain match (for emails)</summary>
    EmailDomain = 6
}

/// <summary>
/// FUNCTIONAL: Entity type the rule applies to.
/// TECHNICAL: Determines which table to search.
/// </summary>
public enum DuplicateEntityType
{
    /// <summary>Lead records</summary>
    Lead = 0,

    /// <summary>Contact records</summary>
    Contact = 1,

    /// <summary>Account records</summary>
    Account = 2,

    /// <summary>Lead and Contact cross-match</summary>
    LeadContact = 3,

    /// <summary>All person records</summary>
    AllPersons = 4
}

/// <summary>
/// FUNCTIONAL: Status of duplicate candidate review.
/// TECHNICAL: Controls workflow and merge eligibility.
/// </summary>
public enum DuplicateCandidateStatus
{
    /// <summary>Pending review</summary>
    Pending = 0,

    /// <summary>Confirmed as duplicate</summary>
    Confirmed = 1,

    /// <summary>Rejected - not a duplicate</summary>
    Rejected = 2,

    /// <summary>Merged successfully</summary>
    Merged = 3,

    /// <summary>Ignored by user</summary>
    Ignored = 4
}

#endregion

/// <summary>
/// Duplicate detection rule definition.
/// Configures which fields to match and what action to take.
/// </summary>
public class DuplicateRule : BaseEntity
{
    #region Identification

    /// <summary>Rule name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rule description</summary>
    public string? Description { get; set; }

    /// <summary>Whether rule is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Entity type this rule applies to</summary>
    public DuplicateEntityType EntityType { get; set; } = DuplicateEntityType.Lead;

    #endregion

    #region Matching Configuration

    /// <summary>Minimum score to consider a match (0-100)</summary>
    public int MatchThreshold { get; set; } = 80;

    /// <summary>Action to take on match</summary>
    public DuplicateAction Action { get; set; } = DuplicateAction.Warn;

    /// <summary>Whether to run on create</summary>
    public bool RunOnCreate { get; set; } = true;

    /// <summary>Whether to run on update</summary>
    public bool RunOnUpdate { get; set; } = true;

    /// <summary>Whether to run on import</summary>
    public bool RunOnImport { get; set; } = true;

    /// <summary>Priority for rule evaluation (lower = higher priority)</summary>
    public int Priority { get; set; } = 100;

    #endregion

    #region Scheduling

    /// <summary>Run batch duplicate scan</summary>
    public bool EnableBatchScan { get; set; } = false;

    /// <summary>Batch scan frequency (daily, weekly, etc.)</summary>
    public string? BatchScanFrequency { get; set; }

    /// <summary>Last batch scan date</summary>
    public DateTime? LastBatchScanDate { get; set; }

    /// <summary>Next scheduled batch scan</summary>
    public DateTime? NextBatchScanDate { get; set; }

    #endregion

    #region Statistics

    /// <summary>Total duplicates found</summary>
    public int TotalDuplicatesFound { get; set; } = 0;

    /// <summary>Total duplicates merged</summary>
    public int TotalDuplicatesMerged { get; set; } = 0;

    /// <summary>Total false positives rejected</summary>
    public int TotalFalsePositives { get; set; } = 0;

    #endregion

    #region Relationships

    /// <summary>Match fields for this rule</summary>
    public ICollection<DuplicateMatchField> MatchFields { get; set; } = new List<DuplicateMatchField>();

    /// <summary>Duplicate candidates found by this rule</summary>
    public ICollection<DuplicateCandidate> Candidates { get; set; } = new List<DuplicateCandidate>();

    #endregion
}

/// <summary>
/// Field configuration for duplicate matching.
/// </summary>
public class DuplicateMatchField : BaseEntity
{
    /// <summary>Field name to match</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Display name for UI</summary>
    public string? DisplayName { get; set; }

    /// <summary>How to match this field</summary>
    public MatchType MatchType { get; set; } = MatchType.Exact;

    /// <summary>Weight of this field in scoring (0-100)</summary>
    public int Weight { get; set; } = 50;

    /// <summary>Whether this field is required for a match</summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>Fuzzy match tolerance (for fuzzy matching)</summary>
    public int? FuzzyTolerance { get; set; }

    /// <summary>Whether to ignore null values</summary>
    public bool IgnoreNullValues { get; set; } = true;

    /// <summary>Transform to apply before matching (lowercase, trim, etc.)</summary>
    public string? Transform { get; set; }

    /// <summary>Order of field evaluation</summary>
    public int Order { get; set; } = 0;

    /// <summary>Parent duplicate rule ID</summary>
    public int DuplicateRuleId { get; set; }

    /// <summary>Navigation to duplicate rule</summary>
    public DuplicateRule? DuplicateRule { get; set; }
}

/// <summary>
/// Detected duplicate candidate pair for review.
/// </summary>
public class DuplicateCandidate : BaseEntity
{
    #region Record References

    /// <summary>Entity type of records</summary>
    public DuplicateEntityType EntityType { get; set; }

    /// <summary>First record ID (newer/source)</summary>
    public int SourceRecordId { get; set; }

    /// <summary>First record type</summary>
    public string SourceRecordType { get; set; } = string.Empty;

    /// <summary>Second record ID (older/target)</summary>
    public int TargetRecordId { get; set; }

    /// <summary>Second record type</summary>
    public string TargetRecordType { get; set; } = string.Empty;

    #endregion

    #region Match Details

    /// <summary>Match confidence score (0-100)</summary>
    public int MatchScore { get; set; }

    /// <summary>Fields that matched (JSON array)</summary>
    public string? MatchingFields { get; set; }

    /// <summary>Field-by-field comparison data (JSON)</summary>
    public string? ComparisonData { get; set; }

    #endregion

    #region Review Status

    /// <summary>Current review status</summary>
    public DuplicateCandidateStatus Status { get; set; } = DuplicateCandidateStatus.Pending;

    /// <summary>Detected date</summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Reviewed date</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Merged date</summary>
    public DateTime? MergedAt { get; set; }

    #endregion

    #region Relationships

    /// <summary>Duplicate rule that detected this</summary>
    public int? DuplicateRuleId { get; set; }

    /// <summary>Navigation to duplicate rule</summary>
    public DuplicateRule? DuplicateRule { get; set; }

    /// <summary>User who reviewed</summary>
    public int? ReviewedById { get; set; }

    /// <summary>Navigation to reviewer</summary>
    public User? ReviewedBy { get; set; }

    /// <summary>User who merged</summary>
    public int? MergedById { get; set; }

    /// <summary>Navigation to merger</summary>
    public User? MergedBy { get; set; }

    #endregion

    #region Notes

    /// <summary>Review notes</summary>
    public string? Notes { get; set; }

    #endregion
}

/// <summary>
/// Merge history for auditing duplicate merges.
/// </summary>
public class DuplicateMergeHistory : BaseEntity
{
    /// <summary>Entity type that was merged</summary>
    public DuplicateEntityType EntityType { get; set; }

    /// <summary>Surviving record ID</summary>
    public int SurvivingRecordId { get; set; }

    /// <summary>Merged (deleted) record ID</summary>
    public int MergedRecordId { get; set; }

    /// <summary>Snapshot of merged record data (JSON)</summary>
    public string? MergedRecordData { get; set; }

    /// <summary>Field values used from merged record (JSON)</summary>
    public string? FieldsFromMergedRecord { get; set; }

    /// <summary>Related records that were re-linked</summary>
    public string? RelinkedRecords { get; set; }

    /// <summary>Date of merge</summary>
    public DateTime MergedAt { get; set; } = DateTime.UtcNow;

    /// <summary>User who performed merge</summary>
    public int? MergedById { get; set; }

    /// <summary>Navigation to merger</summary>
    public User? MergedBy { get; set; }

    /// <summary>Related duplicate candidate ID</summary>
    public int? DuplicateCandidateId { get; set; }

    /// <summary>Navigation to duplicate candidate</summary>
    public DuplicateCandidate? DuplicateCandidate { get; set; }

    /// <summary>Related merge group ID</summary>
    public int? MergeGroupId { get; set; }

    /// <summary>Navigation to merge group</summary>
    public DuplicateMergeGroup? MergeGroup { get; set; }
}

#region Merge Group Entities

/// <summary>
/// Merge group status
/// </summary>
public enum MergeGroupStatus
{
    /// <summary>Merge is active</summary>
    Active = 0,

    /// <summary>All records unmerged</summary>
    Unmerged = 1,

    /// <summary>Some records unmerged</summary>
    PartialUnmerge = 2
}

/// <summary>
/// Group of merged duplicate records.
/// Supports multi-record merges and unmerge functionality.
/// </summary>
public class DuplicateMergeGroup : BaseEntity
{
    /// <summary>Entity type that was merged (Lead, Contact, Account)</summary>
    public DuplicateEntityType EntityType { get; set; }

    /// <summary>ID of the master/surviving record</summary>
    public int MasterRecordId { get; set; }

    /// <summary>Unique identifier for this merge group</summary>
    public string GroupIdentifier { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Status of the merge group</summary>
    public MergeGroupStatus Status { get; set; } = MergeGroupStatus.Active;

    /// <summary>When the merge occurred</summary>
    public DateTime MergedAt { get; set; } = DateTime.UtcNow;

    /// <summary>User who performed the merge</summary>
    public int? MergedById { get; set; }

    /// <summary>Navigation to merger</summary>
    public User? MergedBy { get; set; }

    /// <summary>When unmerge occurred (if applicable)</summary>
    public DateTime? UnmergedAt { get; set; }

    /// <summary>User who performed the unmerge</summary>
    public int? UnmergedById { get; set; }

    /// <summary>Navigation to unmerger</summary>
    public User? UnmergedBy { get; set; }

    /// <summary>Notes about the merge</summary>
    public string? Notes { get; set; }

    /// <summary>Members of this merge group</summary>
    public ICollection<DuplicateMergeGroupMember> Members { get; set; } = new List<DuplicateMergeGroupMember>();

    /// <summary>Related merge histories</summary>
    public ICollection<DuplicateMergeHistory> MergeHistories { get; set; } = new List<DuplicateMergeHistory>();
}

/// <summary>
/// Member status in a merge group
/// </summary>
public enum MergeGroupMemberStatus
{
    /// <summary>Record is merged</summary>
    Merged = 0,

    /// <summary>Record has been unmerged</summary>
    Unmerged = 1
}

/// <summary>
/// Individual record that was part of a merge group.
/// Stores snapshot for unmerge functionality.
/// </summary>
public class DuplicateMergeGroupMember : BaseEntity
{
    /// <summary>Parent merge group ID</summary>
    public int MergeGroupId { get; set; }

    /// <summary>Navigation to merge group</summary>
    public DuplicateMergeGroup? MergeGroup { get; set; }

    /// <summary>Record ID that was merged</summary>
    public int RecordId { get; set; }

    /// <summary>Entity type for polymorphic reference</summary>
    public DuplicateEntityType RecordType { get; set; }

    /// <summary>Is this the master record?</summary>
    public bool IsMaster { get; set; }

    /// <summary>Complete JSON snapshot of record before merge</summary>
    public string? RecordSnapshot { get; set; }

    /// <summary>Which field values from this record were used in master (JSON)</summary>
    public string? FieldValuesUsed { get; set; }

    /// <summary>Related records that were relinked to master (JSON)</summary>
    public string? RelinkedRecords { get; set; }

    /// <summary>Status of this member</summary>
    public MergeGroupMemberStatus Status { get; set; } = MergeGroupMemberStatus.Merged;

    /// <summary>When this record was merged</summary>
    public DateTime MergedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this record was unmerged (if applicable)</summary>
    public DateTime? UnmergedAt { get; set; }
}

#endregion
