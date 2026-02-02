# Deduplication Feature - Detailed Implementation Plan

**Version:** 1.0  
**Created:** February 1, 2026  
**Author:** Implementation Team  
**Target CRM Version:** 0.0.28

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Feature Overview](#feature-overview)
3. [Implementation Phases](#implementation-phases)
4. [Phase 1: Database Schema](#phase-1-database-schema)
5. [Phase 2: Backend Services](#phase-2-backend-services)
6. [Phase 3: API Layer](#phase-3-api-layer)
7. [Phase 4: Frontend Components](#phase-4-frontend-components)
8. [Phase 5: Testing Strategy](#phase-5-testing-strategy)
9. [Phase 6: Documentation](#phase-6-documentation)
10. [Rollback Plan](#rollback-plan)
11. [Risk Assessment](#risk-assessment)

---

## Executive Summary

This document outlines the implementation plan for enhanced duplicate detection and entity merging functionality in the CRM system. The feature will:

1. **Real-time Duplicate Detection**: Detect potential duplicates when creating new entities
2. **Confirmation Dialog**: Present users with existing similar records before creation
3. **Merge Functionality**: Allow merging multiple duplicate records with master selection
4. **Unmerge Support**: Enable reverting merges via soft-delete with full audit trail

### Existing Infrastructure

The system already has foundational duplicate detection tables:
- `DuplicateRules` - Rule definitions
- `DuplicateMatchFields` - Field matching configuration
- `DuplicateCandidates` - Detected duplicate pairs
- `DuplicateMergeHistories` - Merge audit trail

This implementation will **extend** the existing infrastructure rather than replace it.

---

## Feature Overview

### User Stories

| ID | Story | Priority |
|----|-------|----------|
| US-01 | As a user, when I create a new Lead/Contact/Account, I want to see if similar records exist | High |
| US-02 | As a user, I want to choose between updating an existing record or creating new | High |
| US-03 | As a user, I want to merge multiple duplicate records into one master record | High |
| US-04 | As a user, I want to select which record becomes the master during merge | High |
| US-05 | As a user, I want to unmerge previously merged records | Medium |
| US-06 | As an admin, I want to configure duplicate detection rules per entity type | Medium |
| US-07 | As a user, I want to see field-by-field comparison when reviewing duplicates | High |

### Entity Types Supported

| Entity | Duplicate Fields | Priority |
|--------|-----------------|----------|
| **Lead** | Email, Name, Company, Phone | High |
| **Contact** | Email, Name, Phone | High |
| **Account** | Name, Domain, Phone | High |
| **Opportunity** | (Future phase) | Low |

---

## Implementation Phases

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        IMPLEMENTATION TIMELINE                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Phase 1 ─────► Phase 2 ─────► Phase 3 ─────► Phase 4 ─────► Phase 5       │
│  Database       Backend        API Layer     Frontend       Testing         │
│  (3 days)       (5 days)       (3 days)      (5 days)       (4 days)       │
│                                                                             │
│  ────────────────────────────────────────────────────────────────────────   │
│  Week 1                       Week 2                       Week 3           │
│                                                                             │
│  ◉ Migration        ◉ Services         ◉ Controllers      ◉ E2E Tests      │
│  ◉ Indexes          ◉ Detection Logic  ◉ DTOs             ◉ Unit Tests     │
│  ◉ Seed Data        ◉ Merge Logic      ◉ Endpoints        ◉ Documentation  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Database Schema

### 1.1 New Tables

#### `DuplicateMergeGroups` Table

Tracks groups of records that have been merged together (supports multi-record merges and unmerge).

```sql
CREATE TABLE DuplicateMergeGroups (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EntityType VARCHAR(50) NOT NULL,           -- 'Lead', 'Contact', 'Account'
    MasterRecordId INT NOT NULL,               -- The surviving master record
    GroupIdentifier VARCHAR(100) NOT NULL,     -- Unique identifier for this merge group
    Status VARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'Unmerged', 'PartialUnmerge'
    MergedAt DATETIME NOT NULL,
    MergedById INT NULL,
    UnmergedAt DATETIME NULL,
    UnmergedById INT NULL,
    Notes TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    RowVersion BINARY(8),
    
    INDEX IX_DuplicateMergeGroups_EntityType (EntityType),
    INDEX IX_DuplicateMergeGroups_MasterRecordId (MasterRecordId),
    INDEX IX_DuplicateMergeGroups_Status (Status),
    FOREIGN KEY (MergedById) REFERENCES Users(Id),
    FOREIGN KEY (UnmergedById) REFERENCES Users(Id)
);
```

#### `DuplicateMergeGroupMembers` Table

Tracks individual records that were part of a merge group.

```sql
CREATE TABLE DuplicateMergeGroupMembers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    MergeGroupId INT NOT NULL,
    RecordId INT NOT NULL,
    RecordType VARCHAR(50) NOT NULL,           -- Entity type for polymorphic reference
    IsMaster BOOLEAN NOT NULL DEFAULT FALSE,   -- Is this the master record?
    RecordSnapshot JSON NULL,                  -- Complete snapshot before merge
    FieldValuesUsed JSON NULL,                 -- Which fields from this record were used
    RelinkedRecords JSON NULL,                 -- Related records that were relinked
    Status VARCHAR(20) NOT NULL DEFAULT 'Merged', -- 'Merged', 'Unmerged'
    MergedAt DATETIME NOT NULL,
    UnmergedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    RowVersion BINARY(8),
    
    INDEX IX_MergeGroupMembers_MergeGroupId (MergeGroupId),
    INDEX IX_MergeGroupMembers_RecordId (RecordId, RecordType),
    INDEX IX_MergeGroupMembers_Status (Status),
    FOREIGN KEY (MergeGroupId) REFERENCES DuplicateMergeGroups(Id) ON DELETE CASCADE
);
```

### 1.2 Entity Modifications

Add merge-related fields to existing entities:

```csharp
// Add to Lead, Contact, Account entities
public int? MergedIntoId { get; set; }           // If merged, ID of master record
public int? MergeGroupId { get; set; }           // Reference to merge group
public bool IsMergedDuplicate { get; set; }      // Quick flag for filtering
public DateTime? MergedAt { get; set; }          // When this record was merged
```

### 1.3 Migration Script

**File:** `database/migrations/20260201_AddDeduplicationEnhancements.sql`

```sql
-- =====================================================
-- Migration: Enhanced Deduplication Support
-- Date: 2026-02-01
-- Author: CRM Team
-- =====================================================

-- Add merge tracking fields to Leads
ALTER TABLE Leads 
ADD COLUMN MergedIntoId INT NULL,
ADD COLUMN MergeGroupId INT NULL,
ADD COLUMN IsMergedDuplicate BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN MergedAt DATETIME NULL;

-- Add merge tracking fields to Contacts
ALTER TABLE Contacts 
ADD COLUMN MergedIntoId INT NULL,
ADD COLUMN MergeGroupId INT NULL,
ADD COLUMN IsMergedDuplicate BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN MergedAt DATETIME NULL;

-- Add merge tracking fields to Accounts (Customers)
ALTER TABLE Customers 
ADD COLUMN MergedIntoId INT NULL,
ADD COLUMN MergeGroupId INT NULL,
ADD COLUMN IsMergedDuplicate BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN MergedAt DATETIME NULL;

-- Create indexes for merged record queries
CREATE INDEX IX_Leads_MergedIntoId ON Leads(MergedIntoId);
CREATE INDEX IX_Leads_IsMergedDuplicate ON Leads(IsMergedDuplicate);
CREATE INDEX IX_Contacts_MergedIntoId ON Contacts(MergedIntoId);
CREATE INDEX IX_Contacts_IsMergedDuplicate ON Contacts(IsMergedDuplicate);
CREATE INDEX IX_Customers_MergedIntoId ON Customers(MergedIntoId);
CREATE INDEX IX_Customers_IsMergedDuplicate ON Customers(IsMergedDuplicate);
```

### 1.4 EF Core Migration

**File:** `CRM.Backend/src/CRM.Infrastructure/Migrations/[Timestamp]_AddDeduplicationEnhancements.cs`

```csharp
public partial class AddDeduplicationEnhancements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create DuplicateMergeGroups table
        migrationBuilder.CreateTable(
            name: "DuplicateMergeGroups",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                EntityType = table.Column<string>(maxLength: 50, nullable: false),
                MasterRecordId = table.Column<int>(nullable: false),
                GroupIdentifier = table.Column<string>(maxLength: 100, nullable: false),
                Status = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                MergedAt = table.Column<DateTime>(nullable: false),
                MergedById = table.Column<int>(nullable: true),
                UnmergedAt = table.Column<DateTime>(nullable: true),
                UnmergedById = table.Column<int>(nullable: true),
                Notes = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                UpdatedAt = table.Column<DateTime>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DuplicateMergeGroups", x => x.Id);
                table.ForeignKey(
                    name: "FK_DuplicateMergeGroups_Users_MergedById",
                    column: x => x.MergedById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Add merge tracking columns to entities
        migrationBuilder.AddColumn<int>("MergedIntoId", "Leads", nullable: true);
        migrationBuilder.AddColumn<int>("MergeGroupId", "Leads", nullable: true);
        migrationBuilder.AddColumn<bool>("IsMergedDuplicate", "Leads", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTime?>("MergedAt", "Leads", nullable: true);

        // Repeat for Contacts and Customers...
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse all changes
    }
}
```

---

## Phase 2: Backend Services

### 2.1 New Interfaces

**File:** `CRM.Backend/src/CRM.Core/Interfaces/IDuplicateDetectionService.cs`

```csharp
namespace CRM.Core.Interfaces;

/// <summary>
/// Service for detecting and managing duplicate records
/// </summary>
public interface IDuplicateDetectionService
{
    /// <summary>
    /// Find potential duplicates for a new record before creation
    /// </summary>
    Task<DuplicateCheckResult> CheckForDuplicatesAsync<T>(T record, CancellationToken cancellationToken = default)
        where T : BaseEntity;

    /// <summary>
    /// Find potential duplicates using specific field values
    /// </summary>
    Task<DuplicateCheckResult> CheckForDuplicatesAsync(
        string entityType,
        Dictionary<string, string> fieldValues,
        int? excludeRecordId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active duplicate detection rules for an entity type
    /// </summary>
    Task<IEnumerable<DuplicateRule>> GetActiveRulesAsync(DuplicateEntityType entityType);

    /// <summary>
    /// Calculate match score between two records
    /// </summary>
    Task<DuplicateMatchScore> CalculateMatchScoreAsync<T>(T record1, T record2, DuplicateRule rule)
        where T : BaseEntity;
}

/// <summary>
/// Result of a duplicate check operation
/// </summary>
public class DuplicateCheckResult
{
    public bool HasDuplicates => Duplicates.Any();
    public List<DuplicateMatch> Duplicates { get; set; } = new();
    public DuplicateRule? AppliedRule { get; set; }
    public DuplicateAction RecommendedAction { get; set; }
}

/// <summary>
/// A single duplicate match with comparison details
/// </summary>
public class DuplicateMatch
{
    public int RecordId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public Dictionary<string, FieldComparison> FieldComparisons { get; set; } = new();
    public object? RecordSummary { get; set; }
}

/// <summary>
/// Field-level comparison detail
/// </summary>
public class FieldComparison
{
    public string FieldName { get; set; } = string.Empty;
    public string? NewValue { get; set; }
    public string? ExistingValue { get; set; }
    public bool IsMatch { get; set; }
    public int MatchWeight { get; set; }
    public MatchType MatchType { get; set; }
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
```

**File:** `CRM.Backend/src/CRM.Core/Interfaces/IMergeService.cs`

```csharp
namespace CRM.Core.Interfaces;

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
    /// Get merge history for a record
    /// </summary>
    Task<IEnumerable<DuplicateMergeGroup>> GetMergeHistoryAsync(int recordId, string entityType);

    /// <summary>
    /// Get records that were merged into a master record
    /// </summary>
    Task<IEnumerable<MergedRecordInfo>> GetMergedRecordsAsync(int masterRecordId, string entityType);

    /// <summary>
    /// Preview what a merge would look like without executing
    /// </summary>
    Task<MergePreview> PreviewMergeAsync(MergeRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to merge multiple records
/// </summary>
public class MergeRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int MasterRecordId { get; set; }
    public List<int> RecordsToMerge { get; set; } = new();
    public Dictionary<string, int>? FieldSourceOverrides { get; set; } // field -> recordId to use
    public bool RelinkRelatedRecords { get; set; } = true;
    public string? Notes { get; set; }
    public int UserId { get; set; }
}

/// <summary>
/// Result of a merge operation
/// </summary>
public class MergeResult
{
    public bool Success { get; set; }
    public int MergeGroupId { get; set; }
    public int MasterRecordId { get; set; }
    public int RecordsMerged { get; set; }
    public int RelatedRecordsRelinked { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to unmerge records
/// </summary>
public class UnmergeRequest
{
    public int MergeGroupId { get; set; }
    public List<int>? SpecificRecordsToRestore { get; set; } // null = restore all
    public bool RestoreRelatedRecords { get; set; } = true;
    public string? Notes { get; set; }
    public int UserId { get; set; }
}

/// <summary>
/// Result of an unmerge operation
/// </summary>
public class UnmergeResult
{
    public bool Success { get; set; }
    public List<int> RestoredRecordIds { get; set; } = new();
    public int RelatedRecordsRestored { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Preview of merge before execution
/// </summary>
public class MergePreview
{
    public object? PreviewMasterRecord { get; set; }
    public Dictionary<string, FieldMergePreview> FieldPreviews { get; set; } = new();
    public List<RelatedRecordPreview> RelatedRecords { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Preview of how a field will be merged
/// </summary>
public class FieldMergePreview
{
    public string FieldName { get; set; } = string.Empty;
    public string? FinalValue { get; set; }
    public int SourceRecordId { get; set; }
    public Dictionary<int, string?> AllValues { get; set; } = new();
}
```

### 2.2 Service Implementations

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/DuplicateDetectionService.cs`

```csharp
namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of duplicate detection service with configurable matching
/// </summary>
public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DuplicateDetectionService> _logger;

    public DuplicateDetectionService(ICrmDbContext context, ILogger<DuplicateDetectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync<T>(T record, CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        var entityType = GetEntityType<T>();
        var fieldValues = ExtractFieldValues(record);
        
        return await CheckForDuplicatesAsync(
            entityType.ToString(),
            fieldValues,
            record.Id > 0 ? record.Id : null,
            cancellationToken);
    }

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync(
        string entityType,
        Dictionary<string, string> fieldValues,
        int? excludeRecordId = null,
        CancellationToken cancellationToken = default)
    {
        var result = new DuplicateCheckResult();

        // Get active rules for this entity type
        if (!Enum.TryParse<DuplicateEntityType>(entityType, out var duplicateEntityType))
        {
            return result;
        }

        var rules = await GetActiveRulesAsync(duplicateEntityType);
        if (!rules.Any())
        {
            return result;
        }

        var rule = rules.OrderBy(r => r.Priority).First();
        result.AppliedRule = rule;
        result.RecommendedAction = rule.Action;

        // Get potential matches based on entity type
        var candidates = await GetCandidateRecordsAsync(duplicateEntityType, fieldValues, excludeRecordId, cancellationToken);

        foreach (var candidate in candidates)
        {
            var matchScore = await CalculateMatchScoreAsync(fieldValues, candidate, rule);
            
            if (matchScore.TotalScore >= rule.MatchThreshold)
            {
                result.Duplicates.Add(new DuplicateMatch
                {
                    RecordId = candidate.Id,
                    EntityType = entityType,
                    MatchScore = matchScore.TotalScore,
                    FieldComparisons = matchScore.FieldResults.ToDictionary(
                        f => f.FieldName,
                        f => new FieldComparison
                        {
                            FieldName = f.FieldName,
                            NewValue = fieldValues.GetValueOrDefault(f.FieldName),
                            ExistingValue = f.ExistingValue,
                            IsMatch = f.IsMatch,
                            MatchWeight = f.Weight,
                            MatchType = f.MatchType
                        }),
                    RecordSummary = GetRecordSummary(candidate)
                });
            }
        }

        return result;
    }

    // String matching methods
    private bool IsExactMatch(string? value1, string? value2)
        => string.Equals(value1?.Trim(), value2?.Trim(), StringComparison.OrdinalIgnoreCase);

    private bool IsFuzzyMatch(string? value1, string? value2, int tolerance)
    {
        if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2))
            return false;
        
        // Levenshtein distance calculation
        var distance = CalculateLevenshteinDistance(value1.ToLower(), value2.ToLower());
        var maxLength = Math.Max(value1.Length, value2.Length);
        var similarity = (maxLength - distance) * 100 / maxLength;
        
        return similarity >= (100 - tolerance);
    }

    private int CalculateLevenshteinDistance(string s1, string s2)
    {
        // Standard Levenshtein implementation
        var m = s1.Length;
        var n = s2.Length;
        var d = new int[m + 1, n + 1];

        for (var i = 0; i <= m; i++) d[i, 0] = i;
        for (var j = 0; j <= n; j++) d[0, j] = j;

        for (var j = 1; j <= n; j++)
        {
            for (var i = 1; i <= m; i++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[m, n];
    }

    private bool IsPhoneticMatch(string? value1, string? value2)
    {
        if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2))
            return false;

        // Soundex implementation
        var soundex1 = GetSoundex(value1);
        var soundex2 = GetSoundex(value2);
        
        return soundex1 == soundex2;
    }

    private string GetSoundex(string input)
    {
        // Standard Soundex algorithm
        if (string.IsNullOrEmpty(input))
            return "0000";

        var soundex = new StringBuilder();
        soundex.Append(char.ToUpper(input[0]));

        var prevCode = GetSoundexCode(input[0]);
        for (var i = 1; i < input.Length && soundex.Length < 4; i++)
        {
            var code = GetSoundexCode(input[i]);
            if (code != '0' && code != prevCode)
            {
                soundex.Append(code);
            }
            prevCode = code;
        }

        while (soundex.Length < 4)
            soundex.Append('0');

        return soundex.ToString();
    }

    private char GetSoundexCode(char c)
    {
        c = char.ToUpper(c);
        return c switch
        {
            'B' or 'F' or 'P' or 'V' => '1',
            'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z' => '2',
            'D' or 'T' => '3',
            'L' => '4',
            'M' or 'N' => '5',
            'R' => '6',
            _ => '0'
        };
    }

    private bool IsEmailDomainMatch(string? email1, string? email2)
    {
        if (string.IsNullOrEmpty(email1) || string.IsNullOrEmpty(email2))
            return false;

        var domain1 = email1.Split('@').LastOrDefault()?.ToLower();
        var domain2 = email2.Split('@').LastOrDefault()?.ToLower();
        
        return !string.IsNullOrEmpty(domain1) && domain1 == domain2;
    }
}
```

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/MergeService.cs`

```csharp
namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of entity merge/unmerge service
/// </summary>
public class MergeService : IMergeService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<MergeService> _logger;

    public MergeService(ICrmDbContext context, ILogger<MergeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MergeResult> MergeRecordsAsync(MergeRequest request, CancellationToken cancellationToken = default)
    {
        var result = new MergeResult { MasterRecordId = request.MasterRecordId };

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Validate all records exist and are not already merged
            var validationResult = await ValidateMergeRequest(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                result.ErrorMessage = validationResult.ErrorMessage;
                return result;
            }

            // 2. Create merge group
            var mergeGroup = new DuplicateMergeGroup
            {
                EntityType = request.EntityType,
                MasterRecordId = request.MasterRecordId,
                GroupIdentifier = Guid.NewGuid().ToString("N"),
                Status = "Active",
                MergedAt = DateTime.UtcNow,
                MergedById = request.UserId,
                Notes = request.Notes
            };
            
            _context.Set<DuplicateMergeGroup>().Add(mergeGroup);
            await _context.SaveChangesAsync(cancellationToken);

            result.MergeGroupId = mergeGroup.Id;

            // 3. Process each record to merge
            foreach (var recordId in request.RecordsToMerge)
            {
                await ProcessMergedRecordAsync(
                    mergeGroup.Id,
                    recordId,
                    request.MasterRecordId,
                    request.EntityType,
                    request.FieldSourceOverrides,
                    request.RelinkRelatedRecords,
                    cancellationToken);
                
                result.RecordsMerged++;
            }

            // 4. Add master record to group
            await AddMasterToGroupAsync(mergeGroup.Id, request.MasterRecordId, request.EntityType, cancellationToken);

            // 5. Commit transaction
            await transaction.CommitAsync(cancellationToken);
            result.Success = true;

            _logger.LogInformation(
                "Successfully merged {Count} records into master {MasterId} for {EntityType}",
                result.RecordsMerged, request.MasterRecordId, request.EntityType);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error merging records for {EntityType}", request.EntityType);
            result.ErrorMessage = "An error occurred during merge: " + ex.Message;
            return result;
        }
    }

    public async Task<UnmergeResult> UnmergeRecordsAsync(UnmergeRequest request, CancellationToken cancellationToken = default)
    {
        var result = new UnmergeResult();

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Get merge group
            var mergeGroup = await _context.Set<DuplicateMergeGroup>()
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == request.MergeGroupId && !g.IsDeleted, cancellationToken);

            if (mergeGroup == null)
            {
                result.ErrorMessage = "Merge group not found";
                return result;
            }

            // 2. Determine which records to restore
            var recordsToRestore = request.SpecificRecordsToRestore != null
                ? mergeGroup.Members.Where(m => request.SpecificRecordsToRestore.Contains(m.RecordId) && !m.IsMaster)
                : mergeGroup.Members.Where(m => !m.IsMaster);

            // 3. Restore each record from snapshot
            foreach (var member in recordsToRestore)
            {
                await RestoreRecordFromSnapshotAsync(member, request.RestoreRelatedRecords, cancellationToken);
                result.RestoredRecordIds.Add(member.RecordId);
                
                member.Status = "Unmerged";
                member.UnmergedAt = DateTime.UtcNow;
            }

            // 4. Update merge group status
            mergeGroup.Status = request.SpecificRecordsToRestore != null ? "PartialUnmerge" : "Unmerged";
            mergeGroup.UnmergedAt = DateTime.UtcNow;
            mergeGroup.UnmergedById = request.UserId;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error unmerging records for group {GroupId}", request.MergeGroupId);
            result.ErrorMessage = "An error occurred during unmerge: " + ex.Message;
            return result;
        }
    }

    public async Task<MergePreview> PreviewMergeAsync(MergeRequest request, CancellationToken cancellationToken = default)
    {
        var preview = new MergePreview();
        
        // Get all records involved
        var allRecordIds = new List<int> { request.MasterRecordId };
        allRecordIds.AddRange(request.RecordsToMerge);

        // Build field preview showing which value would be used for each field
        // Master record values take precedence unless overridden
        
        return preview;
    }

    private async Task ProcessMergedRecordAsync(
        int mergeGroupId,
        int recordId,
        int masterRecordId,
        string entityType,
        Dictionary<string, int>? fieldOverrides,
        bool relinkRelated,
        CancellationToken cancellationToken)
    {
        // Implementation varies by entity type
        switch (entityType.ToLower())
        {
            case "lead":
                await MergeLeadAsync(mergeGroupId, recordId, masterRecordId, fieldOverrides, relinkRelated, cancellationToken);
                break;
            case "contact":
                await MergeContactAsync(mergeGroupId, recordId, masterRecordId, fieldOverrides, relinkRelated, cancellationToken);
                break;
            case "account":
                await MergeAccountAsync(mergeGroupId, recordId, masterRecordId, fieldOverrides, relinkRelated, cancellationToken);
                break;
        }
    }

    private async Task MergeLeadAsync(
        int mergeGroupId,
        int recordId,
        int masterRecordId,
        Dictionary<string, int>? fieldOverrides,
        bool relinkRelated,
        CancellationToken cancellationToken)
    {
        var record = await _context.Set<Lead>().FindAsync(new object[] { recordId }, cancellationToken);
        var master = await _context.Set<Lead>().FindAsync(new object[] { masterRecordId }, cancellationToken);
        
        if (record == null || master == null) return;

        // Create snapshot before merge
        var snapshot = JsonSerializer.Serialize(record);

        // Create group member entry
        var member = new DuplicateMergeGroupMember
        {
            MergeGroupId = mergeGroupId,
            RecordId = recordId,
            RecordType = "Lead",
            IsMaster = false,
            RecordSnapshot = snapshot,
            Status = "Merged",
            MergedAt = DateTime.UtcNow
        };

        // Apply field overrides to master if this record is the source
        var fieldsUsed = new Dictionary<string, string>();
        if (fieldOverrides != null)
        {
            foreach (var (field, sourceId) in fieldOverrides)
            {
                if (sourceId == recordId)
                {
                    ApplyFieldValue(master, record, field);
                    fieldsUsed[field] = GetFieldValue(record, field) ?? "";
                }
            }
        }
        member.FieldValuesUsed = JsonSerializer.Serialize(fieldsUsed);

        // Relink related records
        if (relinkRelated)
        {
            var relinkedRecords = await RelinkLeadRelatedRecordsAsync(recordId, masterRecordId, cancellationToken);
            member.RelinkedRecords = JsonSerializer.Serialize(relinkedRecords);
        }

        // Soft-delete the merged record
        record.IsDeleted = true;
        record.IsMergedDuplicate = true;
        record.MergedIntoId = masterRecordId;
        record.MergeGroupId = mergeGroupId;
        record.MergedAt = DateTime.UtcNow;

        _context.Set<DuplicateMergeGroupMember>().Add(member);
    }
}
```

### 2.3 Entity Updates

**File:** `CRM.Backend/src/CRM.Core/Entities/DuplicateRule.cs` (additions)

```csharp
/// <summary>
/// Group of merged duplicate records
/// </summary>
public class DuplicateMergeGroup : BaseEntity
{
    /// <summary>Entity type that was merged</summary>
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>ID of the master/surviving record</summary>
    public int MasterRecordId { get; set; }
    
    /// <summary>Unique identifier for this merge group</summary>
    [MaxLength(100)]
    public string GroupIdentifier { get; set; } = string.Empty;
    
    /// <summary>Status: Active, Unmerged, PartialUnmerge</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Active";
    
    /// <summary>When the merge occurred</summary>
    public DateTime MergedAt { get; set; }
    
    /// <summary>User who performed the merge</summary>
    public int? MergedById { get; set; }
    public User? MergedBy { get; set; }
    
    /// <summary>When unmerge occurred (if applicable)</summary>
    public DateTime? UnmergedAt { get; set; }
    
    /// <summary>User who performed the unmerge</summary>
    public int? UnmergedById { get; set; }
    public User? UnmergedBy { get; set; }
    
    /// <summary>Notes about the merge</summary>
    public string? Notes { get; set; }
    
    /// <summary>Members of this merge group</summary>
    public ICollection<DuplicateMergeGroupMember> Members { get; set; } = new List<DuplicateMergeGroupMember>();
}

/// <summary>
/// Individual record that was part of a merge group
/// </summary>
public class DuplicateMergeGroupMember : BaseEntity
{
    /// <summary>Parent merge group ID</summary>
    public int MergeGroupId { get; set; }
    public DuplicateMergeGroup? MergeGroup { get; set; }
    
    /// <summary>Record ID that was merged</summary>
    public int RecordId { get; set; }
    
    /// <summary>Entity type for polymorphic reference</summary>
    [MaxLength(50)]
    public string RecordType { get; set; } = string.Empty;
    
    /// <summary>Is this the master record?</summary>
    public bool IsMaster { get; set; }
    
    /// <summary>Complete JSON snapshot of record before merge</summary>
    public string? RecordSnapshot { get; set; }
    
    /// <summary>Which field values from this record were used in master</summary>
    public string? FieldValuesUsed { get; set; }
    
    /// <summary>Related records that were relinked to master</summary>
    public string? RelinkedRecords { get; set; }
    
    /// <summary>Status: Merged, Unmerged</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Merged";
    
    /// <summary>When this record was merged</summary>
    public DateTime MergedAt { get; set; }
    
    /// <summary>When this record was unmerged (if applicable)</summary>
    public DateTime? UnmergedAt { get; set; }
}
```

---

## Phase 3: API Layer

### 3.1 New Controllers

**File:** `CRM.Backend/src/CRM.Api/Controllers/DuplicatesController.cs`

```csharp
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for duplicate detection and merge operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DuplicatesController : ControllerBase
{
    private readonly IDuplicateDetectionService _duplicateService;
    private readonly IMergeService _mergeService;
    private readonly ILogger<DuplicatesController> _logger;

    public DuplicatesController(
        IDuplicateDetectionService duplicateService,
        IMergeService mergeService,
        ILogger<DuplicatesController> logger)
    {
        _duplicateService = duplicateService;
        _mergeService = mergeService;
        _logger = logger;
    }

    #region Duplicate Detection

    /// <summary>
    /// Check for duplicates before creating a new record
    /// </summary>
    /// <param name="request">Field values to check</param>
    /// <returns>List of potential duplicates with match scores</returns>
    [HttpPost("check")]
    [ProducesResponseType(typeof(DuplicateCheckResult), 200)]
    public async Task<IActionResult> CheckForDuplicates([FromBody] DuplicateCheckRequest request)
    {
        try
        {
            var result = await _duplicateService.CheckForDuplicatesAsync(
                request.EntityType,
                request.FieldValues,
                request.ExcludeRecordId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for duplicates");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get duplicate detection rules for an entity type
    /// </summary>
    [HttpGet("rules/{entityType}")]
    [ProducesResponseType(typeof(IEnumerable<DuplicateRule>), 200)]
    public async Task<IActionResult> GetRules(string entityType)
    {
        try
        {
            if (!Enum.TryParse<DuplicateEntityType>(entityType, true, out var type))
            {
                return BadRequest("Invalid entity type");
            }

            var rules = await _duplicateService.GetActiveRulesAsync(type);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving duplicate rules");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Merge Operations

    /// <summary>
    /// Merge multiple records into a master record
    /// </summary>
    [HttpPost("merge")]
    [ProducesResponseType(typeof(MergeResult), 200)]
    public async Task<IActionResult> MergeRecords([FromBody] MergeRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            
            var mergeRequest = new MergeRequest
            {
                EntityType = request.EntityType,
                MasterRecordId = request.MasterRecordId,
                RecordsToMerge = request.RecordsToMerge,
                FieldSourceOverrides = request.FieldSourceOverrides,
                RelinkRelatedRecords = request.RelinkRelatedRecords ?? true,
                Notes = request.Notes,
                UserId = userId
            };

            var result = await _mergeService.MergeRecordsAsync(mergeRequest);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging records");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Preview what a merge would look like without executing
    /// </summary>
    [HttpPost("merge/preview")]
    [ProducesResponseType(typeof(MergePreview), 200)]
    public async Task<IActionResult> PreviewMerge([FromBody] MergeRequestDto request)
    {
        try
        {
            var mergeRequest = new MergeRequest
            {
                EntityType = request.EntityType,
                MasterRecordId = request.MasterRecordId,
                RecordsToMerge = request.RecordsToMerge,
                FieldSourceOverrides = request.FieldSourceOverrides
            };

            var preview = await _mergeService.PreviewMergeAsync(mergeRequest);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing merge");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unmerge previously merged records
    /// </summary>
    [HttpPost("unmerge")]
    [ProducesResponseType(typeof(UnmergeResult), 200)]
    public async Task<IActionResult> UnmergeRecords([FromBody] UnmergeRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            
            var unmergeRequest = new UnmergeRequest
            {
                MergeGroupId = request.MergeGroupId,
                SpecificRecordsToRestore = request.SpecificRecordsToRestore,
                RestoreRelatedRecords = request.RestoreRelatedRecords ?? true,
                Notes = request.Notes,
                UserId = userId
            };

            var result = await _mergeService.UnmergeRecordsAsync(unmergeRequest);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unmerging records");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get merge history for a record
    /// </summary>
    [HttpGet("history/{entityType}/{recordId}")]
    public async Task<IActionResult> GetMergeHistory(string entityType, int recordId)
    {
        try
        {
            var history = await _mergeService.GetMergeHistoryAsync(recordId, entityType);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merge history");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get records that were merged into a master
    /// </summary>
    [HttpGet("merged/{entityType}/{masterRecordId}")]
    public async Task<IActionResult> GetMergedRecords(string entityType, int masterRecordId)
    {
        try
        {
            var records = await _mergeService.GetMergedRecordsAsync(masterRecordId, entityType);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merged records");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

#region DTOs

public class DuplicateCheckRequest
{
    public string EntityType { get; set; } = string.Empty;
    public Dictionary<string, string> FieldValues { get; set; } = new();
    public int? ExcludeRecordId { get; set; }
}

public class MergeRequestDto
{
    public string EntityType { get; set; } = string.Empty;
    public int MasterRecordId { get; set; }
    public List<int> RecordsToMerge { get; set; } = new();
    public Dictionary<string, int>? FieldSourceOverrides { get; set; }
    public bool? RelinkRelatedRecords { get; set; }
    public string? Notes { get; set; }
}

public class UnmergeRequestDto
{
    public int MergeGroupId { get; set; }
    public List<int>? SpecificRecordsToRestore { get; set; }
    public bool? RestoreRelatedRecords { get; set; }
    public string? Notes { get; set; }
}

#endregion
```

### 3.2 Controller Modifications

Add duplicate detection to existing create endpoints:

**Modified:** `LeadsController.cs`

```csharp
/// <summary>
/// Create a new lead with optional duplicate check
/// </summary>
[HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateLeadDto request,
    [FromQuery] bool skipDuplicateCheck = false,
    [FromQuery] bool forceCreate = false)
{
    try
    {
        // Check for duplicates unless skipped
        if (!skipDuplicateCheck && !forceCreate)
        {
            var fieldValues = new Dictionary<string, string>
            {
                { "Email", request.Email ?? "" },
                { "FirstName", request.FirstName ?? "" },
                { "LastName", request.LastName ?? "" },
                { "CompanyName", request.Company ?? request.CompanyName ?? "" },
                { "Phone", request.Phone ?? "" }
            };

            var duplicateResult = await _duplicateService.CheckForDuplicatesAsync(
                "Lead", fieldValues);

            if (duplicateResult.HasDuplicates)
            {
                return Ok(new
                {
                    hasDuplicates = true,
                    duplicates = duplicateResult.Duplicates,
                    recommendedAction = duplicateResult.RecommendedAction.ToString(),
                    message = "Potential duplicates found. Use forceCreate=true to create anyway."
                });
            }
        }

        // Proceed with creation...
        var lead = new Lead { /* ... */ };
        _context.Set<Lead>().Add(lead);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, 
            new { id = lead.Id, message = "Lead created successfully" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating lead");
        return StatusCode(500, "Internal server error");
    }
}
```

---

## Phase 4: Frontend Components

### 4.1 New Services

**File:** `CRM.Frontend/src/services/duplicateService.ts`

```typescript
import apiClient from './apiClient';

export interface FieldComparison {
  fieldName: string;
  newValue: string | null;
  existingValue: string | null;
  isMatch: boolean;
  matchWeight: number;
  matchType: string;
}

export interface DuplicateMatch {
  recordId: number;
  entityType: string;
  matchScore: number;
  fieldComparisons: Record<string, FieldComparison>;
  recordSummary: Record<string, any>;
}

export interface DuplicateCheckResult {
  hasDuplicates: boolean;
  duplicates: DuplicateMatch[];
  appliedRule: any;
  recommendedAction: string;
}

export interface MergeRequest {
  entityType: string;
  masterRecordId: number;
  recordsToMerge: number[];
  fieldSourceOverrides?: Record<string, number>;
  relinkRelatedRecords?: boolean;
  notes?: string;
}

export interface MergeResult {
  success: boolean;
  mergeGroupId: number;
  masterRecordId: number;
  recordsMerged: number;
  relatedRecordsRelinked: number;
  warnings: string[];
  errorMessage?: string;
}

export interface UnmergeRequest {
  mergeGroupId: number;
  specificRecordsToRestore?: number[];
  restoreRelatedRecords?: boolean;
  notes?: string;
}

export interface UnmergeResult {
  success: boolean;
  restoredRecordIds: number[];
  relatedRecordsRestored: number;
  warnings: string[];
  errorMessage?: string;
}

export interface MergePreview {
  previewMasterRecord: any;
  fieldPreviews: Record<string, FieldMergePreview>;
  relatedRecords: any[];
  warnings: string[];
}

export interface FieldMergePreview {
  fieldName: string;
  finalValue: string | null;
  sourceRecordId: number;
  allValues: Record<number, string | null>;
}

export const duplicateService = {
  /**
   * Check for potential duplicates before creating a record
   */
  async checkForDuplicates(
    entityType: string,
    fieldValues: Record<string, string>,
    excludeRecordId?: number
  ): Promise<DuplicateCheckResult> {
    const response = await apiClient.post('/api/duplicates/check', {
      entityType,
      fieldValues,
      excludeRecordId
    });
    return response.data;
  },

  /**
   * Get duplicate detection rules for an entity type
   */
  async getRules(entityType: string): Promise<any[]> {
    const response = await apiClient.get(`/api/duplicates/rules/${entityType}`);
    return response.data;
  },

  /**
   * Merge multiple records into a master record
   */
  async mergeRecords(request: MergeRequest): Promise<MergeResult> {
    const response = await apiClient.post('/api/duplicates/merge', request);
    return response.data;
  },

  /**
   * Preview what a merge would look like
   */
  async previewMerge(request: MergeRequest): Promise<MergePreview> {
    const response = await apiClient.post('/api/duplicates/merge/preview', request);
    return response.data;
  },

  /**
   * Unmerge previously merged records
   */
  async unmergeRecords(request: UnmergeRequest): Promise<UnmergeResult> {
    const response = await apiClient.post('/api/duplicates/unmerge', request);
    return response.data;
  },

  /**
   * Get merge history for a record
   */
  async getMergeHistory(entityType: string, recordId: number): Promise<any[]> {
    const response = await apiClient.get(`/api/duplicates/history/${entityType}/${recordId}`);
    return response.data;
  },

  /**
   * Get records that were merged into a master record
   */
  async getMergedRecords(entityType: string, masterRecordId: number): Promise<any[]> {
    const response = await apiClient.get(`/api/duplicates/merged/${entityType}/${masterRecordId}`);
    return response.data;
  }
};

export default duplicateService;
```

### 4.2 New Components

**File:** `CRM.Frontend/src/components/duplicates/DuplicateDetectionDialog.tsx`

```tsx
import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Card,
  CardContent,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Radio,
  RadioGroup,
  FormControlLabel,
  Alert,
  Divider,
  IconButton,
  Collapse,
} from '@mui/material';
import {
  Close as CloseIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  CheckCircle as MatchIcon,
  Cancel as NoMatchIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { DuplicateMatch, FieldComparison } from '../../services/duplicateService';

interface DuplicateDetectionDialogProps {
  open: boolean;
  onClose: () => void;
  duplicates: DuplicateMatch[];
  newRecordData: Record<string, any>;
  entityType: string;
  recommendedAction: string;
  onCreateNew: () => void;
  onUpdateExisting: (recordId: number) => void;
  onViewRecord: (recordId: number) => void;
}

export const DuplicateDetectionDialog: React.FC<DuplicateDetectionDialogProps> = ({
  open,
  onClose,
  duplicates,
  newRecordData,
  entityType,
  recommendedAction,
  onCreateNew,
  onUpdateExisting,
  onViewRecord,
}) => {
  const [selectedRecord, setSelectedRecord] = useState<number | null>(null);
  const [expandedRecords, setExpandedRecords] = useState<Record<number, boolean>>({});

  const toggleExpand = (recordId: number) => {
    setExpandedRecords(prev => ({
      ...prev,
      [recordId]: !prev[recordId]
    }));
  };

  const getScoreColor = (score: number) => {
    if (score >= 90) return 'error';
    if (score >= 70) return 'warning';
    return 'info';
  };

  const renderFieldComparison = (fieldName: string, comparison: FieldComparison) => (
    <TableRow key={fieldName}>
      <TableCell>
        <Typography variant="body2" fontWeight="medium">
          {comparison.fieldName}
        </Typography>
      </TableCell>
      <TableCell>
        <Typography variant="body2">
          {comparison.newValue || <em>Empty</em>}
        </Typography>
      </TableCell>
      <TableCell>
        <Typography variant="body2">
          {comparison.existingValue || <em>Empty</em>}
        </Typography>
      </TableCell>
      <TableCell align="center">
        {comparison.isMatch ? (
          <MatchIcon color="success" fontSize="small" />
        ) : (
          <NoMatchIcon color="disabled" fontSize="small" />
        )}
      </TableCell>
    </TableRow>
  );

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box display="flex" alignItems="center" gap={1}>
            <WarningIcon color="warning" />
            <Typography variant="h6">Potential Duplicates Found</Typography>
          </Box>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent>
        <Alert severity="warning" sx={{ mb: 2 }}>
          We found {duplicates.length} existing record{duplicates.length > 1 ? 's' : ''} that 
          may be duplicates of the {entityType.toLowerCase()} you're trying to create.
          Please review and choose an action.
        </Alert>

        <RadioGroup
          value={selectedRecord}
          onChange={(e) => setSelectedRecord(Number(e.target.value))}
        >
          {duplicates.map((duplicate) => (
            <Card key={duplicate.recordId} sx={{ mb: 2 }}>
              <CardContent>
                <Box display="flex" alignItems="center" gap={2}>
                  <FormControlLabel
                    value={duplicate.recordId}
                    control={<Radio />}
                    label=""
                  />
                  <Box flex={1}>
                    <Box display="flex" alignItems="center" gap={1} mb={1}>
                      <Typography variant="subtitle1" fontWeight="bold">
                        {duplicate.recordSummary?.name || 
                         `${duplicate.recordSummary?.firstName} ${duplicate.recordSummary?.lastName}`}
                      </Typography>
                      <Chip
                        label={`${duplicate.matchScore}% Match`}
                        size="small"
                        color={getScoreColor(duplicate.matchScore)}
                      />
                    </Box>
                    <Typography variant="body2" color="text.secondary">
                      {duplicate.recordSummary?.email}
                      {duplicate.recordSummary?.companyName && 
                        ` • ${duplicate.recordSummary.companyName}`}
                    </Typography>
                  </Box>
                  <IconButton onClick={() => toggleExpand(duplicate.recordId)}>
                    {expandedRecords[duplicate.recordId] ? 
                      <ExpandLessIcon /> : <ExpandMoreIcon />}
                  </IconButton>
                </Box>

                <Collapse in={expandedRecords[duplicate.recordId]}>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="subtitle2" gutterBottom>
                    Field Comparison
                  </Typography>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Field</TableCell>
                        <TableCell>New Value</TableCell>
                        <TableCell>Existing Value</TableCell>
                        <TableCell align="center">Match</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {Object.entries(duplicate.fieldComparisons).map(([field, comparison]) =>
                        renderFieldComparison(field, comparison)
                      )}
                    </TableBody>
                  </Table>
                  <Box mt={2}>
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={() => onViewRecord(duplicate.recordId)}
                    >
                      View Full Record
                    </Button>
                  </Box>
                </Collapse>
              </CardContent>
            </Card>
          ))}
        </RadioGroup>
      </DialogContent>

      <DialogActions sx={{ p: 2, gap: 1 }}>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          variant="outlined"
          onClick={onCreateNew}
        >
          Create New Anyway
        </Button>
        <Button
          variant="contained"
          disabled={selectedRecord === null}
          onClick={() => selectedRecord && onUpdateExisting(selectedRecord)}
        >
          Update Selected Record
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default DuplicateDetectionDialog;
```

**File:** `CRM.Frontend/src/components/duplicates/MergeDialog.tsx`

```tsx
import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Stepper,
  Step,
  StepLabel,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Radio,
  RadioGroup,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress,
  Chip,
  TextField,
} from '@mui/material';
import { MergeType as MergeIcon } from '@mui/icons-material';
import duplicateService, { MergeRequest, MergePreview } from '../../services/duplicateService';

interface MergeDialogProps {
  open: boolean;
  onClose: () => void;
  entityType: string;
  records: Array<{ id: number; [key: string]: any }>;
  displayFields: Array<{ key: string; label: string }>;
  onMergeComplete: (result: any) => void;
}

const steps = ['Select Master Record', 'Choose Field Values', 'Review & Confirm'];

export const MergeDialog: React.FC<MergeDialogProps> = ({
  open,
  onClose,
  entityType,
  records,
  displayFields,
  onMergeComplete,
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [masterRecordId, setMasterRecordId] = useState<number | null>(null);
  const [fieldSources, setFieldSources] = useState<Record<string, number>>({});
  const [relinkRelatedRecords, setRelinkRelatedRecords] = useState(true);
  const [notes, setNotes] = useState('');
  const [preview, setPreview] = useState<MergePreview | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize field sources to master record
  useEffect(() => {
    if (masterRecordId) {
      const initialSources: Record<string, number> = {};
      displayFields.forEach(field => {
        initialSources[field.key] = masterRecordId;
      });
      setFieldSources(initialSources);
    }
  }, [masterRecordId, displayFields]);

  const handleNext = async () => {
    if (activeStep === steps.length - 2) {
      // Load preview before final step
      await loadPreview();
    }
    setActiveStep(prev => prev + 1);
  };

  const handleBack = () => {
    setActiveStep(prev => prev - 1);
  };

  const loadPreview = async () => {
    if (!masterRecordId) return;
    
    setLoading(true);
    try {
      const request: MergeRequest = {
        entityType,
        masterRecordId,
        recordsToMerge: records.filter(r => r.id !== masterRecordId).map(r => r.id),
        fieldSourceOverrides: fieldSources,
        relinkRelatedRecords,
      };
      const previewResult = await duplicateService.previewMerge(request);
      setPreview(previewResult);
    } catch (err) {
      setError('Failed to load merge preview');
    } finally {
      setLoading(false);
    }
  };

  const handleMerge = async () => {
    if (!masterRecordId) return;

    setLoading(true);
    setError(null);

    try {
      const request: MergeRequest = {
        entityType,
        masterRecordId,
        recordsToMerge: records.filter(r => r.id !== masterRecordId).map(r => r.id),
        fieldSourceOverrides: fieldSources,
        relinkRelatedRecords,
        notes,
      };

      const result = await duplicateService.mergeRecords(request);
      
      if (result.success) {
        onMergeComplete(result);
        onClose();
      } else {
        setError(result.errorMessage || 'Merge failed');
      }
    } catch (err) {
      setError('An error occurred during merge');
    } finally {
      setLoading(false);
    }
  };

  const renderStepContent = () => {
    switch (activeStep) {
      case 0:
        return (
          <Box>
            <Typography variant="body1" gutterBottom>
              Select which record should be the master (surviving) record.
              Other records will be merged into this one.
            </Typography>
            <RadioGroup
              value={masterRecordId}
              onChange={(e) => setMasterRecordId(Number(e.target.value))}
            >
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Master</TableCell>
                    {displayFields.slice(0, 4).map(field => (
                      <TableCell key={field.key}>{field.label}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {records.map(record => (
                    <TableRow 
                      key={record.id}
                      selected={masterRecordId === record.id}
                      sx={{ cursor: 'pointer' }}
                      onClick={() => setMasterRecordId(record.id)}
                    >
                      <TableCell>
                        <FormControlLabel
                          value={record.id}
                          control={<Radio />}
                          label=""
                        />
                      </TableCell>
                      {displayFields.slice(0, 4).map(field => (
                        <TableCell key={field.key}>
                          {record[field.key] || '-'}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </RadioGroup>
          </Box>
        );

      case 1:
        return (
          <Box>
            <Typography variant="body1" gutterBottom>
              For each field, select which record's value should be used in the merged record.
            </Typography>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Field</TableCell>
                  {records.map(record => (
                    <TableCell key={record.id}>
                      Record #{record.id}
                      {record.id === masterRecordId && (
                        <Chip label="Master" size="small" sx={{ ml: 1 }} />
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              </TableHead>
              <TableBody>
                {displayFields.map(field => (
                  <TableRow key={field.key}>
                    <TableCell>{field.label}</TableCell>
                    {records.map(record => (
                      <TableCell key={record.id}>
                        <FormControlLabel
                          control={
                            <Radio
                              checked={fieldSources[field.key] === record.id}
                              onChange={() => setFieldSources(prev => ({
                                ...prev,
                                [field.key]: record.id
                              }))}
                            />
                          }
                          label={record[field.key] || <em>Empty</em>}
                        />
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            <Box mt={2}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={relinkRelatedRecords}
                    onChange={(e) => setRelinkRelatedRecords(e.target.checked)}
                  />
                }
                label="Re-link related records (activities, notes, etc.) to master record"
              />
            </Box>
          </Box>
        );

      case 2:
        return (
          <Box>
            <Alert severity="warning" sx={{ mb: 2 }}>
              This action will merge {records.length - 1} record(s) into the master record.
              Merged records will be soft-deleted but can be unmerged later.
            </Alert>
            
            {preview && (
              <Box mb={2}>
                <Typography variant="subtitle1" gutterBottom>
                  Final Merged Record Preview
                </Typography>
                <Table size="small">
                  <TableBody>
                    {Object.entries(preview.fieldPreviews || {}).map(([field, data]) => (
                      <TableRow key={field}>
                        <TableCell>{field}</TableCell>
                        <TableCell>{data.finalValue || '-'}</TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            from Record #{data.sourceRecordId}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Box>
            )}

            <TextField
              fullWidth
              multiline
              rows={2}
              label="Merge Notes (optional)"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Add any notes about why these records are being merged..."
            />
          </Box>
        );

      default:
        return null;
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Box display="flex" alignItems="center" gap={1}>
          <MergeIcon />
          <Typography variant="h6">Merge {entityType} Records</Typography>
        </Box>
      </DialogTitle>

      <DialogContent>
        <Stepper activeStep={activeStep} sx={{ mb: 3 }}>
          {steps.map(label => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box display="flex" justifyContent="center" p={4}>
            <CircularProgress />
          </Box>
        ) : (
          renderStepContent()
        )}
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Box flex={1} />
        {activeStep > 0 && (
          <Button onClick={handleBack}>
            Back
          </Button>
        )}
        {activeStep < steps.length - 1 ? (
          <Button
            variant="contained"
            onClick={handleNext}
            disabled={activeStep === 0 && !masterRecordId}
          >
            Next
          </Button>
        ) : (
          <Button
            variant="contained"
            color="primary"
            onClick={handleMerge}
            disabled={loading}
            startIcon={<MergeIcon />}
          >
            Merge Records
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default MergeDialog;
```

**File:** `CRM.Frontend/src/components/duplicates/MergeHistoryPanel.tsx`

```tsx
import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableRow,
  IconButton,
  Collapse,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Undo as UndoIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  History as HistoryIcon,
} from '@mui/icons-material';
import duplicateService, { UnmergeRequest } from '../../services/duplicateService';

interface MergeHistoryPanelProps {
  entityType: string;
  recordId: number;
  onUnmergeComplete?: () => void;
}

interface MergeHistoryItem {
  id: number;
  groupIdentifier: string;
  masterRecordId: number;
  status: string;
  mergedAt: string;
  mergedBy?: { name: string };
  members: Array<{
    recordId: number;
    recordType: string;
    isMaster: boolean;
    status: string;
    recordSnapshot?: string;
  }>;
}

export const MergeHistoryPanel: React.FC<MergeHistoryPanelProps> = ({
  entityType,
  recordId,
  onUnmergeComplete,
}) => {
  const [history, setHistory] = useState<MergeHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedGroups, setExpandedGroups] = useState<Record<number, boolean>>({});
  const [unmergeDialogOpen, setUnmergeDialogOpen] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<MergeHistoryItem | null>(null);
  const [unmerging, setUnmerging] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadHistory();
  }, [entityType, recordId]);

  const loadHistory = async () => {
    setLoading(true);
    try {
      const result = await duplicateService.getMergeHistory(entityType, recordId);
      setHistory(result);
    } catch (err) {
      console.error('Failed to load merge history', err);
    } finally {
      setLoading(false);
    }
  };

  const toggleExpand = (groupId: number) => {
    setExpandedGroups(prev => ({
      ...prev,
      [groupId]: !prev[groupId]
    }));
  };

  const handleUnmerge = async () => {
    if (!selectedGroup) return;

    setUnmerging(true);
    setError(null);

    try {
      const request: UnmergeRequest = {
        mergeGroupId: selectedGroup.id,
        restoreRelatedRecords: true,
      };

      const result = await duplicateService.unmergeRecords(request);

      if (result.success) {
        setUnmergeDialogOpen(false);
        loadHistory();
        onUnmergeComplete?.();
      } else {
        setError(result.errorMessage || 'Unmerge failed');
      }
    } catch (err) {
      setError('An error occurred during unmerge');
    } finally {
      setUnmerging(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={2}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  if (history.length === 0) {
    return (
      <Box p={2}>
        <Typography variant="body2" color="text.secondary">
          No merge history for this record.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" alignItems="center" gap={1} mb={2}>
        <HistoryIcon />
        <Typography variant="h6">Merge History</Typography>
      </Box>

      {history.map((group) => (
        <Card key={group.id} sx={{ mb: 2 }}>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Box display="flex" alignItems="center" gap={1}>
                  <Typography variant="subtitle1">
                    Merge Group #{group.groupIdentifier.substring(0, 8)}
                  </Typography>
                  <Chip
                    label={group.status}
                    size="small"
                    color={group.status === 'Active' ? 'success' : 'default'}
                  />
                </Box>
                <Typography variant="body2" color="text.secondary">
                  Merged on {new Date(group.mergedAt).toLocaleDateString()}
                  {group.mergedBy && ` by ${group.mergedBy.name}`}
                </Typography>
              </Box>
              <Box>
                {group.status === 'Active' && (
                  <Button
                    size="small"
                    startIcon={<UndoIcon />}
                    onClick={() => {
                      setSelectedGroup(group);
                      setUnmergeDialogOpen(true);
                    }}
                  >
                    Unmerge
                  </Button>
                )}
                <IconButton onClick={() => toggleExpand(group.id)}>
                  {expandedGroups[group.id] ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                </IconButton>
              </Box>
            </Box>

            <Collapse in={expandedGroups[group.id]}>
              <Box mt={2}>
                <Typography variant="subtitle2" gutterBottom>
                  Merged Records
                </Typography>
                <Table size="small">
                  <TableBody>
                    {group.members.map((member) => (
                      <TableRow key={member.recordId}>
                        <TableCell>
                          Record #{member.recordId}
                          {member.isMaster && (
                            <Chip label="Master" size="small" sx={{ ml: 1 }} />
                          )}
                        </TableCell>
                        <TableCell>{member.recordType}</TableCell>
                        <TableCell>
                          <Chip
                            label={member.status}
                            size="small"
                            color={member.status === 'Merged' ? 'warning' : 'default'}
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Box>
            </Collapse>
          </CardContent>
        </Card>
      ))}

      {/* Unmerge Confirmation Dialog */}
      <Dialog open={unmergeDialogOpen} onClose={() => setUnmergeDialogOpen(false)}>
        <DialogTitle>Confirm Unmerge</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <Typography>
            Are you sure you want to unmerge these records? This will restore
            the previously merged records from their snapshots.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUnmergeDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleUnmerge}
            disabled={unmerging}
            startIcon={unmerging ? <CircularProgress size={16} /> : <UndoIcon />}
          >
            Unmerge
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default MergeHistoryPanel;
```

### 4.3 Integration with Existing Pages

**Modify:** `LeadsPage.tsx` (example)

```tsx
// Add import
import { DuplicateDetectionDialog } from '../components/duplicates/DuplicateDetectionDialog';
import { MergeDialog } from '../components/duplicates/MergeDialog';
import duplicateService, { DuplicateMatch } from '../services/duplicateService';

// Add state in component
const [duplicateDialogOpen, setDuplicateDialogOpen] = useState(false);
const [duplicates, setDuplicates] = useState<DuplicateMatch[]>([]);
const [pendingFormData, setPendingFormData] = useState<LeadFormData | null>(null);
const [mergeDialogOpen, setMergeDialogOpen] = useState(false);
const [selectedForMerge, setSelectedForMerge] = useState<Lead[]>([]);

// Modify save handler
const handleSave = async () => {
  if (!formData) return;

  // Check for duplicates first
  const fieldValues = {
    Email: formData.emailPrimary || '',
    FirstName: formData.firstName || '',
    LastName: formData.lastName || '',
    CompanyName: formData.company || '',
    Phone: formData.phonePrimary || '',
  };

  try {
    const duplicateResult = await duplicateService.checkForDuplicates('Lead', fieldValues);
    
    if (duplicateResult.hasDuplicates) {
      setDuplicates(duplicateResult.duplicates);
      setPendingFormData(formData);
      setDuplicateDialogOpen(true);
      return;
    }
  } catch (err) {
    console.error('Duplicate check failed, proceeding with save');
  }

  // No duplicates, proceed with save
  await saveRecord(formData);
};

const handleCreateNew = async () => {
  if (pendingFormData) {
    await saveRecord(pendingFormData, true); // forceCreate = true
    setDuplicateDialogOpen(false);
    setPendingFormData(null);
  }
};

const handleUpdateExisting = async (recordId: number) => {
  // Navigate to edit the existing record
  // Or merge the new data into the existing record
  setDuplicateDialogOpen(false);
};

// Add merge button to toolbar
<Button
  variant="outlined"
  startIcon={<MergeIcon />}
  disabled={selectedRows.length < 2}
  onClick={() => {
    setSelectedForMerge(leads.filter(l => selectedRows.includes(l.id)));
    setMergeDialogOpen(true);
  }}
>
  Merge Selected
</Button>

// Add dialogs at end of component
<DuplicateDetectionDialog
  open={duplicateDialogOpen}
  onClose={() => {
    setDuplicateDialogOpen(false);
    setPendingFormData(null);
  }}
  duplicates={duplicates}
  newRecordData={pendingFormData || {}}
  entityType="Lead"
  recommendedAction="Warn"
  onCreateNew={handleCreateNew}
  onUpdateExisting={handleUpdateExisting}
  onViewRecord={(id) => router.push(`/leads/${id}`)}
/>

<MergeDialog
  open={mergeDialogOpen}
  onClose={() => setMergeDialogOpen(false)}
  entityType="Lead"
  records={selectedForMerge}
  displayFields={[
    { key: 'firstName', label: 'First Name' },
    { key: 'lastName', label: 'Last Name' },
    { key: 'email', label: 'Email' },
    { key: 'companyName', label: 'Company' },
    { key: 'phone', label: 'Phone' },
  ]}
  onMergeComplete={(result) => {
    loadLeads();
    setSelectedRows([]);
  }}
/>
```

---

## Phase 5: Testing Strategy

### 5.1 Unit Tests

**File:** `CRM.Backend/tests/Services/DuplicateDetectionServiceTests.cs`

```csharp
namespace CRM.Tests.Services;

public class DuplicateDetectionServiceTests
{
    private readonly Mock<ICrmDbContext> _contextMock;
    private readonly DuplicateDetectionService _service;

    public DuplicateDetectionServiceTests()
    {
        _contextMock = new Mock<ICrmDbContext>();
        _service = new DuplicateDetectionService(
            _contextMock.Object,
            Mock.Of<ILogger<DuplicateDetectionService>>());
    }

    [Fact]
    public async Task CheckForDuplicates_WithExactEmailMatch_ReturnsHighScore()
    {
        // Arrange
        var fieldValues = new Dictionary<string, string>
        {
            { "Email", "test@example.com" }
        };

        // Setup mock data...

        // Act
        var result = await _service.CheckForDuplicatesAsync("Lead", fieldValues);

        // Assert
        result.HasDuplicates.Should().BeTrue();
        result.Duplicates.First().MatchScore.Should().BeGreaterOrEqualTo(80);
    }

    [Fact]
    public async Task CheckForDuplicates_WithFuzzyNameMatch_ReturnsModerateScore()
    {
        // Test fuzzy matching
    }

    [Fact]
    public async Task CheckForDuplicates_WithPhoneticMatch_ReturnsTrueForSoundingAlike()
    {
        // Test phonetic matching (Smith vs Smyth)
    }

    [Fact]
    public async Task CalculateLevenshteinDistance_ReturnsCorrectDistance()
    {
        // Test string distance calculation
    }

    [Fact]
    public async Task GetSoundex_ReturnsCorrectCode()
    {
        // Test soundex generation
    }
}
```

**File:** `CRM.Backend/tests/Services/MergeServiceTests.cs`

```csharp
namespace CRM.Tests.Services;

public class MergeServiceTests
{
    [Fact]
    public async Task MergeRecords_WithValidRequest_CreatesMergeGroup()
    {
        // Test merge group creation
    }

    [Fact]
    public async Task MergeRecords_SoftDeletesMergedRecords()
    {
        // Test that merged records are soft-deleted
    }

    [Fact]
    public async Task MergeRecords_PreservesSnapshot()
    {
        // Test that record snapshot is saved
    }

    [Fact]
    public async Task MergeRecords_RelinksRelatedRecords()
    {
        // Test related record relinking
    }

    [Fact]
    public async Task UnmergeRecords_RestoresFromSnapshot()
    {
        // Test unmerge restoration
    }

    [Fact]
    public async Task UnmergeRecords_RestoresRelatedRecords()
    {
        // Test related record restoration
    }

    [Fact]
    public async Task MergeRecords_WithFieldOverrides_UsesCorrectValues()
    {
        // Test field source override functionality
    }
}
```

### 5.2 Integration Tests

**File:** `CRM.Backend/tests/CRM.Tests.Integration/DuplicateDetectionIntegrationTests.cs`

```csharp
namespace CRM.Tests.Integration;

[Collection("Integration")]
public class DuplicateDetectionIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateLead_WithDuplicate_ReturnsWarning()
    {
        // Test full flow from API to database
    }

    [Fact]
    public async Task MergeLeads_UpdatesAllRelatedEntities()
    {
        // Test that activities, notes, etc. are relinked
    }

    [Fact]
    public async Task UnmergeLeads_RestoresAllData()
    {
        // Test complete unmerge flow
    }
}
```

### 5.3 Frontend Tests

**File:** `CRM.Frontend/src/__tests__/components/DuplicateDetectionDialog.test.tsx`

```tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { DuplicateDetectionDialog } from '../../components/duplicates/DuplicateDetectionDialog';

describe('DuplicateDetectionDialog', () => {
  const mockDuplicates = [
    {
      recordId: 1,
      entityType: 'Lead',
      matchScore: 85,
      fieldComparisons: {
        Email: { fieldName: 'Email', newValue: 'test@example.com', existingValue: 'test@example.com', isMatch: true, matchWeight: 100, matchType: 'Exact' }
      },
      recordSummary: { firstName: 'John', lastName: 'Doe', email: 'test@example.com' }
    }
  ];

  it('renders duplicate records correctly', () => {
    render(
      <DuplicateDetectionDialog
        open={true}
        onClose={() => {}}
        duplicates={mockDuplicates}
        newRecordData={{}}
        entityType="Lead"
        recommendedAction="Warn"
        onCreateNew={() => {}}
        onUpdateExisting={() => {}}
        onViewRecord={() => {}}
      />
    );

    expect(screen.getByText('Potential Duplicates Found')).toBeInTheDocument();
    expect(screen.getByText('85% Match')).toBeInTheDocument();
  });

  it('calls onCreateNew when Create New Anyway is clicked', async () => {
    const onCreateNew = jest.fn();
    
    render(
      <DuplicateDetectionDialog
        open={true}
        onClose={() => {}}
        duplicates={mockDuplicates}
        newRecordData={{}}
        entityType="Lead"
        recommendedAction="Warn"
        onCreateNew={onCreateNew}
        onUpdateExisting={() => {}}
        onViewRecord={() => {}}
      />
    );

    fireEvent.click(screen.getByText('Create New Anyway'));
    expect(onCreateNew).toHaveBeenCalled();
  });

  it('enables Update button only when record is selected', () => {
    render(
      <DuplicateDetectionDialog
        open={true}
        onClose={() => {}}
        duplicates={mockDuplicates}
        newRecordData={{}}
        entityType="Lead"
        recommendedAction="Warn"
        onCreateNew={() => {}}
        onUpdateExisting={() => {}}
        onViewRecord={() => {}}
      />
    );

    const updateButton = screen.getByText('Update Selected Record');
    expect(updateButton).toBeDisabled();

    // Select the record
    fireEvent.click(screen.getByRole('radio'));
    
    expect(updateButton).not.toBeDisabled();
  });
});
```

### 5.4 E2E Tests

**File:** `e2e-tests/tests/duplicate-detection.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Duplicate Detection', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    // Login...
  });

  test('shows duplicate warning when creating similar lead', async ({ page }) => {
    // Create first lead
    await page.goto('/leads');
    await page.click('button:has-text("Add Lead")');
    await page.fill('[name="firstName"]', 'John');
    await page.fill('[name="lastName"]', 'Doe');
    await page.fill('[name="email"]', 'john.doe@example.com');
    await page.click('button:has-text("Save")');
    await expect(page.locator('.MuiAlert-standardSuccess')).toBeVisible();

    // Try to create duplicate
    await page.click('button:has-text("Add Lead")');
    await page.fill('[name="firstName"]', 'John');
    await page.fill('[name="lastName"]', 'Doe');
    await page.fill('[name="email"]', 'john.doe@example.com');
    await page.click('button:has-text("Save")');

    // Should show duplicate dialog
    await expect(page.locator('text=Potential Duplicates Found')).toBeVisible();
    await expect(page.locator('text=85% Match')).toBeVisible();
  });

  test('can merge multiple leads', async ({ page }) => {
    await page.goto('/leads');
    
    // Select multiple leads
    await page.click('[data-testid="lead-checkbox-1"]');
    await page.click('[data-testid="lead-checkbox-2"]');
    
    // Click merge button
    await page.click('button:has-text("Merge Selected")');
    
    // Complete merge wizard
    await expect(page.locator('text=Select Master Record')).toBeVisible();
    await page.click('[data-testid="master-radio-1"]');
    await page.click('button:has-text("Next")');
    
    // Choose field values
    await page.click('button:has-text("Next")');
    
    // Confirm merge
    await page.click('button:has-text("Merge Records")');
    
    await expect(page.locator('.MuiAlert-standardSuccess')).toBeVisible();
  });

  test('can unmerge previously merged records', async ({ page }) => {
    // Navigate to merged record
    await page.goto('/leads/1');
    
    // Go to history tab
    await page.click('text=Merge History');
    
    // Click unmerge
    await page.click('button:has-text("Unmerge")');
    await page.click('button:has-text("Confirm")');
    
    await expect(page.locator('.MuiAlert-standardSuccess')).toBeVisible();
  });
});
```

### 5.5 Test Coverage Requirements

| Component | Minimum Coverage |
|-----------|-----------------|
| DuplicateDetectionService | 90% |
| MergeService | 90% |
| DuplicatesController | 85% |
| Frontend Components | 80% |
| E2E Critical Paths | 100% |

---

## Phase 6: Documentation

### 6.1 API Documentation

Update Swagger/OpenAPI documentation with new endpoints:

```yaml
/api/duplicates/check:
  post:
    summary: Check for potential duplicates
    tags: [Duplicates]
    requestBody:
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/DuplicateCheckRequest'
    responses:
      200:
        description: Duplicate check result
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/DuplicateCheckResult'

/api/duplicates/merge:
  post:
    summary: Merge multiple records
    tags: [Duplicates]
    requestBody:
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/MergeRequest'
    responses:
      200:
        description: Merge result
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/MergeResult'
```

### 6.2 User Documentation

**File:** `docs/features/DUPLICATE_MANAGEMENT.md`

```markdown
# Duplicate Management

## Overview

The CRM system includes intelligent duplicate detection and merge capabilities
to help maintain data quality.

## Duplicate Detection

When creating new Leads, Contacts, or Accounts, the system automatically
checks for potential duplicates based on:

- Email address (exact match)
- Name (fuzzy matching)
- Phone number (normalized)
- Company name (fuzzy matching)

### Detection Rules

Administrators can configure duplicate detection rules in 
Settings > Data Quality > Duplicate Rules.

## Merging Records

To merge duplicate records:

1. Navigate to the entity list (e.g., Leads)
2. Select 2 or more records using checkboxes
3. Click "Merge Selected"
4. Follow the merge wizard:
   - Select which record is the "master"
   - Choose which field values to keep
   - Review and confirm

### What Happens During Merge

- Related records (notes, activities, etc.) are relinked to the master
- Merged records are soft-deleted (not permanently removed)
- A snapshot is saved for potential unmerge

## Unmerging Records

To unmerge previously merged records:

1. Open the master record
2. Go to the "Merge History" tab
3. Click "Unmerge" on the merge group
4. Confirm the action

Records will be restored from their snapshots.
```

### 6.3 Admin Documentation

**File:** `docs/admin/DUPLICATE_DETECTION_CONFIG.md`

```markdown
# Duplicate Detection Configuration

## Rule Configuration

### Match Fields

| Field | Match Types | Recommended |
|-------|-------------|-------------|
| Email | Exact, Domain | Exact |
| Name | Exact, Fuzzy, Phonetic | Fuzzy (80%) |
| Phone | Exact, Normalized | Normalized |
| Company | Exact, Fuzzy | Fuzzy (70%) |

### Actions

- **Warn**: Show warning, allow creation
- **Block**: Prevent creation
- **AutoMerge**: Automatically merge (use carefully)
- **QueueForReview**: Add to review queue
- **LogOnly**: Log for analytics only

### Thresholds

Recommended match thresholds:
- High confidence: 90-100%
- Medium confidence: 70-89%
- Low confidence: 50-69%

## Performance Considerations

- Index key fields for faster detection
- Limit detection to active records only
- Consider batch scanning for existing data
```

---

## Rollback Plan

### Database Rollback

```sql
-- Rollback migration
ALTER TABLE Leads 
DROP COLUMN MergedIntoId,
DROP COLUMN MergeGroupId,
DROP COLUMN IsMergedDuplicate,
DROP COLUMN MergedAt;

-- Similar for Contacts and Customers...

DROP TABLE IF EXISTS DuplicateMergeGroupMembers;
DROP TABLE IF EXISTS DuplicateMergeGroups;
```

### Code Rollback

1. Revert Git commits in reverse order
2. Redeploy previous version
3. Clear any cached configurations

### Feature Flags

Implement feature flags for gradual rollout:

```csharp
// appsettings.json
{
  "Features": {
    "DuplicateDetection": {
      "Enabled": true,
      "EntityTypes": ["Lead", "Contact", "Account"],
      "MergeEnabled": true,
      "UnmergeEnabled": true
    }
  }
}
```

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Data loss during merge | Low | High | Snapshot all data before merge |
| Performance degradation | Medium | Medium | Index key fields, async detection |
| False positive matches | Medium | Low | Allow user override, tune thresholds |
| Circular merge references | Low | High | Validate merge requests |
| Orphaned related records | Medium | Medium | Transaction-based operations |

---

## Implementation Checklist

### Phase 1: Database (Days 1-3)
- [ ] Create migration script
- [ ] Add new tables (DuplicateMergeGroups, DuplicateMergeGroupMembers)
- [ ] Add columns to Leads, Contacts, Accounts
- [ ] Create indexes
- [ ] Test migration on dev database
- [ ] Document schema changes

### Phase 2: Backend Services (Days 4-8)
- [ ] Create IDuplicateDetectionService interface
- [ ] Create IMergeService interface
- [ ] Implement DuplicateDetectionService
- [ ] Implement MergeService
- [ ] Add string matching algorithms (Levenshtein, Soundex)
- [ ] Register services in DI container
- [ ] Write unit tests

### Phase 3: API Layer (Days 9-11)
- [ ] Create DuplicatesController
- [ ] Add DTOs
- [ ] Modify existing Create endpoints
- [ ] Add Swagger documentation
- [ ] Write controller tests

### Phase 4: Frontend (Days 12-16)
- [ ] Create duplicateService.ts
- [ ] Create DuplicateDetectionDialog component
- [ ] Create MergeDialog component
- [ ] Create MergeHistoryPanel component
- [ ] Integrate with LeadsPage
- [ ] Integrate with ContactsPage
- [ ] Integrate with AccountsPage
- [ ] Write component tests

### Phase 5: Testing (Days 17-20)
- [ ] Complete unit tests (90% coverage)
- [ ] Integration tests
- [ ] E2E tests
- [ ] Performance testing
- [ ] User acceptance testing

### Phase 6: Documentation & Deployment
- [ ] API documentation
- [ ] User guide
- [ ] Admin guide
- [ ] Update CHANGELOG.md
- [ ] Deploy to staging
- [ ] Deploy to production

---

## Appendix: File Summary

### New Files to Create

| File | Description |
|------|-------------|
| `database/migrations/20260201_AddDeduplicationEnhancements.sql` | Database migration |
| `CRM.Core/Interfaces/IDuplicateDetectionService.cs` | Detection service interface |
| `CRM.Core/Interfaces/IMergeService.cs` | Merge service interface |
| `CRM.Infrastructure/Services/DuplicateDetectionService.cs` | Detection implementation |
| `CRM.Infrastructure/Services/MergeService.cs` | Merge implementation |
| `CRM.Api/Controllers/DuplicatesController.cs` | API controller |
| `src/services/duplicateService.ts` | Frontend API service |
| `src/components/duplicates/DuplicateDetectionDialog.tsx` | Detection UI |
| `src/components/duplicates/MergeDialog.tsx` | Merge UI |
| `src/components/duplicates/MergeHistoryPanel.tsx` | History UI |
| `tests/Services/DuplicateDetectionServiceTests.cs` | Unit tests |
| `tests/Services/MergeServiceTests.cs` | Unit tests |
| `e2e-tests/tests/duplicate-detection.spec.ts` | E2E tests |
| `docs/features/DUPLICATE_MANAGEMENT.md` | User docs |
| `docs/admin/DUPLICATE_DETECTION_CONFIG.md` | Admin docs |

### Files to Modify

| File | Changes |
|------|---------|
| `CRM.Core/Entities/DuplicateRule.cs` | Add DuplicateMergeGroup entities |
| `CRM.Core/Entities/Lead.cs` | Add merge tracking fields |
| `CRM.Core/Entities/Contact.cs` | Add merge tracking fields |
| `CRM.Core/Entities/Account.cs` | Add merge tracking fields |
| `CRM.Infrastructure/Data/CrmDbContext.cs` | Add new DbSets |
| `CRM.Api/Program.cs` | Register new services |
| `CRM.Api/Controllers/LeadsController.cs` | Add duplicate check |
| `CRM.Api/Controllers/ContactsController.cs` | Add duplicate check |
| `CRM.Api/Controllers/AccountsController.cs` | Add duplicate check |
| `src/pages/LeadsPage.tsx` | Integrate duplicate UI |
| `src/pages/ContactsPage.tsx` | Integrate duplicate UI |
| `src/pages/AccountsPage.tsx` | Integrate duplicate UI |

---

*Document Version: 1.0*  
*Last Updated: February 1, 2026*
