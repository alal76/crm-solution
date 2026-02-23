// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Exceptions;

/// <summary>
/// Exception thrown when an exact duplicate record is detected during creation.
/// Controllers should catch this and return HTTP 409 Conflict.
/// </summary>
public class DuplicateExistsException : Exception
{
    /// <summary>ID of the existing record that matches</summary>
    public int ExistingRecordId { get; }

    /// <summary>Entity type (Account, Contact, Lead, etc.)</summary>
    public string EntityType { get; }

    /// <summary>Match score (100 = exact match)</summary>
    public int MatchScore { get; }

    public DuplicateExistsException(string entityType, int existingRecordId, int matchScore)
        : base($"A {entityType} record already exists (ID: {existingRecordId}, match score: {matchScore}%).")
    {
        EntityType = entityType;
        ExistingRecordId = existingRecordId;
        MatchScore = matchScore;
    }

    public DuplicateExistsException(string entityType, int existingRecordId, int matchScore, string message)
        : base(message)
    {
        EntityType = entityType;
        ExistingRecordId = existingRecordId;
        MatchScore = matchScore;
    }
}
