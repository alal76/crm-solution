// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

#region Result Types

/// <summary>
/// Result of a duplicate check operation
/// </summary>
public class DuplicateCheckResult
{
    /// <summary>Whether any duplicates were found</summary>
    public bool HasDuplicates => Duplicates.Any();

    /// <summary>List of potential duplicate matches</summary>
    public List<DuplicateMatch> Duplicates { get; set; } = new();

    /// <summary>The rule that was applied for detection</summary>
    public DuplicateRuleInfo? AppliedRule { get; set; }

    /// <summary>Recommended action based on the rule</summary>
    public string RecommendedAction { get; set; } = "Warn";

    /// <summary>Total records scanned</summary>
    public int RecordsScanned { get; set; }

    /// <summary>Time taken for detection in milliseconds</summary>
    public long DetectionTimeMs { get; set; }
}

/// <summary>
/// Information about the duplicate rule that was applied
/// </summary>
public class DuplicateRuleInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MatchThreshold { get; set; }
    public DuplicateAction Action { get; set; }
}

/// <summary>
/// A single duplicate match with comparison details
/// </summary>
public class DuplicateMatch
{
    /// <summary>ID of the matching record</summary>
    public int RecordId { get; set; }

    /// <summary>Type of entity (Lead, Contact, Account)</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Overall match score (0-100)</summary>
    public int MatchScore { get; set; }

    /// <summary>Field-by-field comparison details</summary>
    public Dictionary<string, FieldComparison> FieldComparisons { get; set; } = new();

    /// <summary>Summary of the record for display</summary>
    public RecordSummary? RecordSummary { get; set; }
}

/// <summary>
/// Field-level comparison detail
/// </summary>
public class FieldComparison
{
    /// <summary>Name of the field</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Display label for the field</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Value from the new record</summary>
    public string? NewValue { get; set; }

    /// <summary>Value from the existing record</summary>
    public string? ExistingValue { get; set; }

    /// <summary>Whether the values match according to the rule</summary>
    public bool IsMatch { get; set; }

    /// <summary>Weight of this field in scoring</summary>
    public int MatchWeight { get; set; }

    /// <summary>Type of matching used</summary>
    public string MatchType { get; set; } = "Exact";

    /// <summary>Similarity percentage for fuzzy matches</summary>
    public int? SimilarityPercent { get; set; }
}

/// <summary>
/// Summary of a record for display in duplicate detection results
/// </summary>
public class RecordSummary
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Match score calculation result
/// </summary>
public class DuplicateMatchScore
{
    public int TotalScore { get; set; }
    public int MaxPossibleScore { get; set; }
    public double PercentageMatch => MaxPossibleScore > 0 ? (double)TotalScore / MaxPossibleScore * 100 : 0;
    public List<FieldMatchResult> FieldResults { get; set; } = new();
}

/// <summary>
/// Result of matching a single field
/// </summary>
public class FieldMatchResult
{
    public string FieldName { get; set; } = string.Empty;
    public string? Value1 { get; set; }
    public string? Value2 { get; set; }
    public bool IsMatch { get; set; }
    public int Weight { get; set; }
    public int Score { get; set; }
    public CRM.Core.Entities.MatchType MatchingType { get; set; }
    public int? SimilarityPercent { get; set; }
}

#endregion

/// <summary>
/// Service for detecting duplicate records
/// </summary>
public interface IDuplicateDetectionService
{
    /// <summary>
    /// Find potential duplicates for field values before creation
    /// </summary>
    /// <param name="entityType">Type of entity (Lead, Contact, Account)</param>
    /// <param name="fieldValues">Field values to check</param>
    /// <param name="excludeRecordId">Optional record ID to exclude (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Duplicate check result with matches</returns>
    Task<DuplicateCheckResult> CheckForDuplicatesAsync(
        string entityType,
        Dictionary<string, string?> fieldValues,
        int? excludeRecordId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active duplicate detection rules for an entity type
    /// </summary>
    /// <param name="entityType">Entity type to get rules for</param>
    /// <returns>List of active rules</returns>
    Task<IEnumerable<DuplicateRule>> GetActiveRulesAsync(DuplicateEntityType entityType);

    /// <summary>
    /// Get all duplicate detection rules
    /// </summary>
    Task<IEnumerable<DuplicateRule>> GetAllRulesAsync();

    /// <summary>
    /// Create or update a duplicate detection rule
    /// </summary>
    Task<DuplicateRule> SaveRuleAsync(DuplicateRule rule);

    /// <summary>
    /// Delete a duplicate detection rule
    /// </summary>
    Task<bool> DeleteRuleAsync(int ruleId);

    /// <summary>
    /// Find duplicate candidates across all records (batch scan)
    /// </summary>
    Task<IEnumerable<DuplicateCandidate>> ScanForDuplicatesAsync(
        DuplicateEntityType entityType,
        int? ruleId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending duplicate candidates for review
    /// </summary>
    Task<IEnumerable<DuplicateCandidate>> GetPendingCandidatesAsync(
        DuplicateEntityType? entityType = null,
        int page = 1,
        int pageSize = 25);

    /// <summary>
    /// Update the status of a duplicate candidate
    /// </summary>
    Task<DuplicateCandidate?> UpdateCandidateStatusAsync(
        int candidateId,
        DuplicateCandidateStatus status,
        int userId,
        string? notes = null);
}
