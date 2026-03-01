// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Helper that encapsulates the common duplicate-check-on-create pattern.
/// Called by each entity service's Create method to enforce dedup at the service layer.
///
/// Behavior:
///   - Score >= 95  → throw DuplicateExistsException (exact/near-exact match → reject)
///   - Score >= rule threshold but &lt; 95 → create DuplicateCandidate(Pending) for review
///   - Score &lt; threshold → no action, proceed with creation
/// </summary>
public static class DuplicateCheckHelper
{
    /// <summary>
    /// Exact-match threshold: match scores at or above this are considered
    /// "already exists" and the create is rejected with an exception.
    /// </summary>
    public const int ExactMatchThreshold = 95;

    /// <summary>
    /// Checks for duplicates before entity creation.
    /// Throws <see cref="DuplicateExistsException"/> for exact matches.
    /// Returns a list of fuzzy DuplicateCandidate records to persist for review.
    /// </summary>
    /// <param name="duplicateDetection">The duplicate detection service</param>
    /// <param name="dbContext">DB context to save DuplicateCandidate records</param>
    /// <param name="entityType">Entity type string (e.g. "Account", "Contact")</param>
    /// <param name="fieldValues">Key field values to check for duplicates</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of fuzzy-match candidates queued for review</returns>
    public static async Task<int> CheckAndHandleDuplicatesAsync(
        IDuplicateDetectionService duplicateDetection,
        ICrmDbContext dbContext,
        string entityType,
        Dictionary<string, string?> fieldValues,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (duplicateDetection == null)
        {
            return 0;
        }

        DuplicateCheckResult result;
        try
        {
            result = await duplicateDetection.CheckForDuplicatesAsync(
                entityType, fieldValues, excludeRecordId: null, cancellationToken);
        }
        catch (Exception ex)
        {
            // Dedup check failures should not block creation — log and proceed
            logger.LogWarning(ex, "Duplicate detection check failed for {EntityType}; proceeding with creation", entityType);
            return 0;
        }

        if (result == null || !result.HasDuplicates)
        {
            return 0;
        }

        // Check for exact matches (score >= ExactMatchThreshold)
        var exactMatch = result.Duplicates
            .Where(d => d.MatchScore >= ExactMatchThreshold)
            .OrderByDescending(d => d.MatchScore)
            .FirstOrDefault();

        if (exactMatch != null)
        {
            logger.LogWarning(
                "Exact duplicate detected for {EntityType}: existing record {RecordId} with score {Score}%",
                entityType, exactMatch.RecordId, exactMatch.MatchScore);

            throw new DuplicateExistsException(entityType, exactMatch.RecordId, exactMatch.MatchScore);
        }

        // Fuzzy matches — queue as DuplicateCandidate for review
        // (entity hasn't been saved yet so SourceRecordId will be set to 0;
        //  the caller should update it after SaveChanges if needed)
        int candidatesQueued = 0;
        foreach (var fuzzyMatch in result.Duplicates)
        {
            logger.LogInformation(
                "Potential duplicate detected for {EntityType}: existing record {RecordId} with score {Score}% — queuing for review",
                entityType, fuzzyMatch.RecordId, fuzzyMatch.MatchScore);

            if (!Enum.TryParse<DuplicateEntityType>(entityType, true, out var dupEntityType))
            {
                continue;
            }

            var candidate = new DuplicateCandidate
            {
                EntityType = dupEntityType,
                SourceRecordId = 0, // Will be updated after entity is saved
                SourceRecordType = entityType,
                TargetRecordId = fuzzyMatch.RecordId,
                TargetRecordType = entityType,
                MatchScore = fuzzyMatch.MatchScore,
                MatchingFields = JsonSerializer.Serialize(
                    fuzzyMatch.FieldComparisons
                        .Where(kv => kv.Value.IsMatch)
                        .Select(kv => kv.Key)),
                ComparisonData = JsonSerializer.Serialize(fuzzyMatch.FieldComparisons),
                DuplicateRuleId = result.AppliedRule?.Id,
                Status = DuplicateCandidateStatus.Pending,
                DetectedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Set<DuplicateCandidate>().Add(candidate);
            candidatesQueued++;
        }

        return candidatesQueued;
    }

    /// <summary>
    /// After entity creation, updates the SourceRecordId on any pending candidates
    /// that were queued with SourceRecordId = 0 during the pre-creation check.
    /// </summary>
    public static async Task UpdateCandidateSourceIdsAsync(
        ICrmDbContext dbContext,
        string entityType,
        int newEntityId,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DuplicateEntityType>(entityType, true, out var dupEntityType))
        {
            return;
        }

        // Find candidates that were just created with SourceRecordId = 0
        var pendingCandidates = dbContext.Set<DuplicateCandidate>()
            .Local
            .Where(c => c.EntityType == dupEntityType &&
                        c.SourceRecordId == 0 &&
                        c.Status == DuplicateCandidateStatus.Pending)
            .ToList();

        foreach (var candidate in pendingCandidates)
        {
            candidate.SourceRecordId = newEntityId;
        }

        if (pendingCandidates.Any())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
