// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of entity merge/unmerge service
/// </summary>
public class MergeService : IMergeService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<MergeService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public MergeService(ICrmDbContext context, ILogger<MergeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Merge Operations

    public async Task<MergeResult> MergeRecordsAsync(MergeRequest request, CancellationToken cancellationToken = default)
    {
        var result = new MergeResult { MasterRecordId = request.MasterRecordId };

        // Use a transaction for atomicity
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate entity type
            if (!Enum.TryParse<DuplicateEntityType>(request.EntityType, true, out var entityType))
            {
                result.ErrorMessage = $"Invalid entity type: {request.EntityType}";
                return result;
            }

            // Validate records exist
            var validationResult = await ValidateMergeRequestAsync(request, entityType, cancellationToken);
            if (!validationResult.IsValid)
            {
                result.ErrorMessage = validationResult.ErrorMessage;
                return result;
            }

            // Create merge group
            var mergeGroup = new DuplicateMergeGroup
            {
                EntityType = entityType,
                MasterRecordId = request.MasterRecordId,
                GroupIdentifier = Guid.NewGuid().ToString("N"),
                Status = MergeGroupStatus.Active,
                MergedAt = DateTime.UtcNow,
                MergedById = request.UserId,
                Notes = request.Notes
            };

            _context.Set<DuplicateMergeGroup>().Add(mergeGroup);
            await _context.SaveChangesAsync(cancellationToken);
            result.MergeGroupId = mergeGroup.Id;

            // Add master record to group
            await AddMemberToGroupAsync(
                mergeGroup.Id,
                request.MasterRecordId,
                entityType,
                isMaster: true,
                cancellationToken);

            // Process each record to merge
            foreach (var recordId in request.RecordsToMerge)
            {
                var relinkCount = await ProcessMergedRecordAsync(
                    mergeGroup.Id,
                    recordId,
                    request.MasterRecordId,
                    entityType,
                    request.FieldSourceOverrides,
                    request.RelinkRelatedRecords,
                    cancellationToken);

                result.RecordsMerged++;
                result.RelatedRecordsRelinked += relinkCount;
            }

            await transaction.CommitAsync(cancellationToken);
            result.Success = true;

            _logger.LogInformation(
                "Successfully merged {Count} records into master {MasterId} for {EntityType}. Group: {GroupId}",
                result.RecordsMerged, request.MasterRecordId, request.EntityType, result.MergeGroupId);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error merging records for {EntityType}", request.EntityType);
            result.ErrorMessage = $"An error occurred during merge: {ex.Message}";
            return result;
        }
    }

    public async Task<UnmergeResult> UnmergeRecordsAsync(UnmergeRequest request, CancellationToken cancellationToken = default)
    {
        var result = new UnmergeResult();

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get merge group with members
            var mergeGroup = await _context.Set<DuplicateMergeGroup>()
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == request.MergeGroupId && !g.IsDeleted, cancellationToken);

            if (mergeGroup == null)
            {
                result.ErrorMessage = "Merge group not found";
                return result;
            }

            if (mergeGroup.Status == MergeGroupStatus.Unmerged)
            {
                result.ErrorMessage = "This merge group has already been unmerged";
                return result;
            }

            // Determine which records to restore
            var membersToRestore = request.SpecificRecordsToRestore != null
                ? mergeGroup.Members.Where(m =>
                    request.SpecificRecordsToRestore.Contains(m.RecordId) &&
                    !m.IsMaster &&
                    m.Status == MergeGroupMemberStatus.Merged)
                : mergeGroup.Members.Where(m => !m.IsMaster && m.Status == MergeGroupMemberStatus.Merged);

            foreach (var member in membersToRestore.ToList())
            {
                // Ensure member has access to its parent group for master record ID
                member.MergeGroup = mergeGroup;

                var restored = await RestoreRecordFromSnapshotAsync(
                    member,
                    mergeGroup.EntityType,
                    request.RestoreRelatedRecords,
                    cancellationToken);

                if (restored)
                {
                    member.Status = MergeGroupMemberStatus.Unmerged;
                    member.UnmergedAt = DateTime.UtcNow;
                    result.RestoredRecordIds.Add(member.RecordId);

                    // Reverse field overrides applied to the master during merge
                    if (!string.IsNullOrEmpty(member.FieldValuesUsed))
                    {
                        await ReverseFieldOverridesOnMasterAsync(
                            mergeGroup.MasterRecordId,
                            mergeGroup.EntityType,
                            member.FieldValuesUsed,
                            cancellationToken);
                    }

                    result.RelatedRecordsRestored += await CountRelinkedRecordsAsync(member.RelinkedRecords);
                }
            }

            // Update merge group status
            var allUnmerged = mergeGroup.Members.All(m => m.IsMaster || m.Status == MergeGroupMemberStatus.Unmerged);
            mergeGroup.Status = allUnmerged ? MergeGroupStatus.Unmerged : MergeGroupStatus.PartialUnmerge;
            mergeGroup.UnmergedAt = DateTime.UtcNow;
            mergeGroup.UnmergedById = request.UserId;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result.Success = true;

            _logger.LogInformation(
                "Successfully unmerged {Count} records from group {GroupId}",
                result.RestoredRecordIds.Count, request.MergeGroupId);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error unmerging records for group {GroupId}", request.MergeGroupId);
            result.ErrorMessage = $"An error occurred during unmerge: {ex.Message}";
            return result;
        }
    }

    public async Task<MergePreview> PreviewMergeAsync(MergeRequest request, CancellationToken cancellationToken = default)
    {
        var preview = new MergePreview();

        if (!Enum.TryParse<DuplicateEntityType>(request.EntityType, true, out var entityType))
        {
            preview.Warnings.Add($"Invalid entity type: {request.EntityType}");
            return preview;
        }

        // Get all records involved
        var allRecordIds = new List<int> { request.MasterRecordId };
        allRecordIds.AddRange(request.RecordsToMerge);

        // Load records based on entity type
        var records = await LoadRecordsForPreviewAsync(entityType, allRecordIds, cancellationToken);

        if (!records.Any())
        {
            preview.Warnings.Add("No records found");
            return preview;
        }

        // Build field preview
        var fieldNames = GetMergeableFields(entityType);
        foreach (var fieldName in fieldNames)
        {
            var fieldPreview = new FieldMergePreview
            {
                FieldName = fieldName,
                DisplayName = GetFieldDisplayName(fieldName),
                AllValues = new Dictionary<int, string?>()
            };

            foreach (var record in records)
            {
                var value = GetFieldValue(record, fieldName);
                fieldPreview.AllValues[record.Id] = value;
            }

            // Determine final value (master unless overridden)
            var sourceId = request.FieldSourceOverrides?.GetValueOrDefault(fieldName) ?? request.MasterRecordId;
            fieldPreview.SourceRecordId = sourceId;
            fieldPreview.FinalValue = fieldPreview.AllValues.GetValueOrDefault(sourceId);

            preview.FieldPreviews[fieldName] = fieldPreview;
            preview.PreviewRecord[fieldName] = fieldPreview.FinalValue;
        }

        // Get related records preview
        preview.RelatedRecords = await GetRelatedRecordsPreviewAsync(
            entityType, request.RecordsToMerge, cancellationToken);

        return preview;
    }

    #endregion

    #region History & Info

    public async Task<IEnumerable<MergeGroupInfo>> GetMergeHistoryAsync(int recordId, string entityType)
    {
        if (!Enum.TryParse<DuplicateEntityType>(entityType, true, out var type))
        {
            return Enumerable.Empty<MergeGroupInfo>();
        }

        // Find merge groups where this record is the master or a member
        var groups = await _context.Set<DuplicateMergeGroup>()
            .Include(g => g.Members)
            .Include(g => g.MergedBy)
            .Where(g => !g.IsDeleted && g.EntityType == type &&
                       (g.MasterRecordId == recordId || g.Members.Any(m => m.RecordId == recordId)))
            .OrderByDescending(g => g.MergedAt)
            .ToListAsync();

        return groups.Select(g => new MergeGroupInfo
        {
            Id = g.Id,
            GroupIdentifier = g.GroupIdentifier,
            EntityType = g.EntityType.ToString(),
            MasterRecordId = g.MasterRecordId,
            Status = g.Status.ToString(),
            MergedAt = g.MergedAt,
            MergedByName = g.MergedBy != null ? $"{g.MergedBy.FirstName} {g.MergedBy.LastName}".Trim() : null,
            UnmergedAt = g.UnmergedAt,
            Notes = g.Notes,
            Members = g.Members.Select(m => new MergedRecordInfo
            {
                RecordId = m.RecordId,
                EntityType = m.RecordType.ToString(),
                IsMaster = m.IsMaster,
                Status = m.Status.ToString(),
                MergedAt = m.MergedAt
            }).ToList()
        });
    }

    public async Task<IEnumerable<MergedRecordInfo>> GetMergedRecordsAsync(int masterRecordId, string entityType)
    {
        if (!Enum.TryParse<DuplicateEntityType>(entityType, true, out var type))
        {
            return Enumerable.Empty<MergedRecordInfo>();
        }

        var members = await _context.Set<DuplicateMergeGroupMember>()
            .Include(m => m.MergeGroup)
            .Where(m => m.MergeGroup!.MasterRecordId == masterRecordId &&
                       m.MergeGroup.EntityType == type &&
                       !m.MergeGroup.IsDeleted &&
                       !m.IsMaster)
            .ToListAsync();

        return members.Select(m => new MergedRecordInfo
        {
            RecordId = m.RecordId,
            EntityType = m.RecordType.ToString(),
            IsMaster = m.IsMaster,
            Status = m.Status.ToString(),
            MergedAt = m.MergedAt,
            RecordSnapshot = m.RecordSnapshot != null
                ? JsonSerializer.Deserialize<object>(m.RecordSnapshot)
                : null
        });
    }

    public async Task<MergeGroupInfo?> GetMergeGroupAsync(int mergeGroupId)
    {
        var group = await _context.Set<DuplicateMergeGroup>()
            .Include(g => g.Members)
            .Include(g => g.MergedBy)
            .FirstOrDefaultAsync(g => g.Id == mergeGroupId && !g.IsDeleted);

        if (group == null)
            return null;

        return new MergeGroupInfo
        {
            Id = group.Id,
            GroupIdentifier = group.GroupIdentifier,
            EntityType = group.EntityType.ToString(),
            MasterRecordId = group.MasterRecordId,
            Status = group.Status.ToString(),
            MergedAt = group.MergedAt,
            MergedByName = group.MergedBy != null ? $"{group.MergedBy.FirstName} {group.MergedBy.LastName}".Trim() : null,
            UnmergedAt = group.UnmergedAt,
            Notes = group.Notes,
            Members = group.Members.Select(m => new MergedRecordInfo
            {
                RecordId = m.RecordId,
                EntityType = m.RecordType.ToString(),
                IsMaster = m.IsMaster,
                Status = m.Status.ToString(),
                MergedAt = m.MergedAt
            }).ToList()
        };
    }

    #endregion

    #region Private Methods

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateMergeRequestAsync(
        MergeRequest request,
        DuplicateEntityType entityType,
        CancellationToken cancellationToken)
    {
        // Validate master record exists
        var masterExists = entityType switch
        {
            DuplicateEntityType.Lead => await _context.Set<Lead>()
                .AnyAsync(l => l.Id == request.MasterRecordId && !l.IsDeleted, cancellationToken),
            DuplicateEntityType.Contact => await _context.Set<Contact>()
                .AnyAsync(c => c.Id == request.MasterRecordId, cancellationToken),
            DuplicateEntityType.Account => await _context.Set<Account>()
                .AnyAsync(a => a.Id == request.MasterRecordId && !a.IsDeleted, cancellationToken),
            _ => false
        };

        if (!masterExists)
        {
            return (false, $"Master record {request.MasterRecordId} not found");
        }

        // Validate records to merge
        foreach (var recordId in request.RecordsToMerge)
        {
            var exists = entityType switch
            {
                DuplicateEntityType.Lead => await _context.Set<Lead>()
                    .AnyAsync(l => l.Id == recordId && !l.IsDeleted && !l.IsMergedDuplicate, cancellationToken),
                DuplicateEntityType.Contact => await _context.Set<Contact>()
                    .AnyAsync(c => c.Id == recordId && !c.IsMergedDuplicate, cancellationToken),
                DuplicateEntityType.Account => await _context.Set<Account>()
                    .AnyAsync(a => a.Id == recordId && !a.IsDeleted && !a.IsMergedDuplicate, cancellationToken),
                _ => false
            };

            if (!exists)
            {
                return (false, $"Record {recordId} not found or already merged");
            }
        }

        return (true, null);
    }

    private async Task AddMemberToGroupAsync(
        int mergeGroupId,
        int recordId,
        DuplicateEntityType entityType,
        bool isMaster,
        CancellationToken cancellationToken)
    {
        // Get record snapshot
        string? snapshot = null;
        if (!isMaster)
        {
            snapshot = await GetRecordSnapshotAsync(recordId, entityType, cancellationToken);
        }

        var member = new DuplicateMergeGroupMember
        {
            MergeGroupId = mergeGroupId,
            RecordId = recordId,
            RecordType = entityType,
            IsMaster = isMaster,
            RecordSnapshot = snapshot,
            Status = MergeGroupMemberStatus.Merged,
            MergedAt = DateTime.UtcNow
        };

        _context.Set<DuplicateMergeGroupMember>().Add(member);
    }

    private async Task<int> ProcessMergedRecordAsync(
        int mergeGroupId,
        int recordId,
        int masterRecordId,
        DuplicateEntityType entityType,
        Dictionary<string, int>? fieldOverrides,
        bool relinkRelated,
        CancellationToken cancellationToken)
    {
        var relinkCount = 0;

        // Get snapshot before merge
        var snapshot = await GetRecordSnapshotAsync(recordId, entityType, cancellationToken);

        // Add to merge group
        var fieldsUsed = new Dictionary<string, string>();
        if (fieldOverrides != null)
        {
            foreach (var (field, sourceId) in fieldOverrides)
            {
                if (sourceId == recordId)
                {
                    var value = await GetFieldValueFromRecordAsync(recordId, entityType, field, cancellationToken);
                    fieldsUsed[field] = value ?? "";

                    // Apply value to master
                    await SetFieldValueOnRecordAsync(masterRecordId, entityType, field, value, cancellationToken);
                }
            }
        }

        // Relink related records
        string? relinkedJson = null;
        if (relinkRelated)
        {
            var relinked = await RelinkRelatedRecordsAsync(recordId, masterRecordId, entityType, cancellationToken);
            relinkCount = relinked.Sum(r => r.Value);
            relinkedJson = JsonSerializer.Serialize(relinked, JsonOptions);
        }

        // Create group member
        var member = new DuplicateMergeGroupMember
        {
            MergeGroupId = mergeGroupId,
            RecordId = recordId,
            RecordType = entityType,
            IsMaster = false,
            RecordSnapshot = snapshot,
            FieldValuesUsed = fieldsUsed.Any() ? JsonSerializer.Serialize(fieldsUsed, JsonOptions) : null,
            RelinkedRecords = relinkedJson,
            Status = MergeGroupMemberStatus.Merged,
            MergedAt = DateTime.UtcNow
        };

        _context.Set<DuplicateMergeGroupMember>().Add(member);

        // Soft-delete the merged record
        await MarkRecordAsMergedAsync(recordId, masterRecordId, mergeGroupId, entityType, cancellationToken);

        return relinkCount;
    }

    private async Task<string?> GetRecordSnapshotAsync(
        int recordId,
        DuplicateEntityType entityType,
        CancellationToken cancellationToken)
    {
        object? record = entityType switch
        {
            DuplicateEntityType.Lead => await _context.Set<Lead>().FindAsync(new object[] { recordId }, cancellationToken),
            DuplicateEntityType.Contact => await _context.Set<Contact>().FindAsync(new object[] { recordId }, cancellationToken),
            DuplicateEntityType.Account => await _context.Set<Account>().FindAsync(new object[] { recordId }, cancellationToken),
            _ => null
        };

        return record != null ? JsonSerializer.Serialize(record, JsonOptions) : null;
    }

    private async Task<Dictionary<string, int>> RelinkRelatedRecordsAsync(
        int sourceRecordId,
        int targetRecordId,
        DuplicateEntityType entityType,
        CancellationToken cancellationToken)
    {
        var relinked = new Dictionary<string, int>();

        switch (entityType)
        {
            case DuplicateEntityType.Lead:
                // Leads don't have direct Interaction/Activity links in current schema
                // Relink notes
                var leadNotes = await _context.Set<Note>()
                    .Where(n => n.EntityType == "Lead" && n.EntityId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var note in leadNotes)
                {
                    note.EntityId = targetRecordId;
                }
                relinked["Notes"] = leadNotes.Count;

                // Relink opportunities that originated from this lead
                var leadOpportunities = await _context.Set<Opportunity>()
                    .Where(o => o.LeadId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var opp in leadOpportunities)
                {
                    opp.LeadId = targetRecordId;
                }
                relinked["Opportunities"] = leadOpportunities.Count;
                break;

            case DuplicateEntityType.Contact:
                // Relink account contacts
                var accountContacts = await _context.Set<AccountContact>()
                    .Where(ac => ac.ContactId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var ac in accountContacts)
                {
                    // Check if target already has this relationship
                    var exists = await _context.Set<AccountContact>()
                        .AnyAsync(x => x.ContactId == targetRecordId && x.AccountId == ac.AccountId, cancellationToken);
                    if (!exists)
                    {
                        ac.ContactId = targetRecordId;
                    }
                }
                relinked["AccountContacts"] = accountContacts.Count;

                // Relink interactions
                var contactInteractions = await _context.Set<Interaction>()
                    .Where(i => i.ContactId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var interaction in contactInteractions)
                {
                    interaction.ContactId = targetRecordId;
                }
                relinked["Interactions"] = contactInteractions.Count;

                // Relink activities
                var contactActivities = await _context.Set<Activity>()
                    .Where(a => a.ContactId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var activity in contactActivities)
                {
                    activity.ContactId = targetRecordId;
                }
                relinked["Activities"] = contactActivities.Count;

                // Relink notes
                var contactNotes = await _context.Set<Note>()
                    .Where(n => n.EntityType == "Contact" && n.EntityId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var note in contactNotes)
                {
                    note.EntityId = targetRecordId;
                }
                relinked["Notes"] = contactNotes.Count;
                break;

            case DuplicateEntityType.Account:
                // Relink opportunities (AccountId property, mapped to legacy account column)
                var opportunities = await _context.Set<Opportunity>()
                    .Where(o => o.AccountId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var opp in opportunities)
                {
                    opp.AccountId = targetRecordId;
                }
                relinked["Opportunities"] = opportunities.Count;

                // Relink contacts
                var contacts = await _context.Set<Contact>()
                    .Where(c => c.AccountId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var contact in contacts)
                {
                    contact.AccountId = targetRecordId;
                }
                relinked["Contacts"] = contacts.Count;

                // Relink interactions
                var interactions = await _context.Set<Interaction>()
                    .Where(i => i.AccountId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var interaction in interactions)
                {
                    interaction.AccountId = targetRecordId;
                }
                relinked["Interactions"] = interactions.Count;

                // Relink activities
                var accountActivities = await _context.Set<Activity>()
                    .Where(a => a.AccountId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var activity in accountActivities)
                {
                    activity.AccountId = targetRecordId;
                }
                relinked["Activities"] = accountActivities.Count;

                // Relink notes
                var accountNotes = await _context.Set<Note>()
                    .Where(n => n.EntityType == "Account" && n.EntityId == sourceRecordId)
                    .ToListAsync(cancellationToken);
                foreach (var note in accountNotes)
                {
                    note.EntityId = targetRecordId;
                }
                relinked["Notes"] = accountNotes.Count;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return relinked;
    }

    private async Task MarkRecordAsMergedAsync(
        int recordId,
        int masterRecordId,
        int mergeGroupId,
        DuplicateEntityType entityType,
        CancellationToken cancellationToken)
    {
        switch (entityType)
        {
            case DuplicateEntityType.Lead:
                var lead = await _context.Set<Lead>().FindAsync(new object[] { recordId }, cancellationToken);
                if (lead != null)
                {
                    lead.IsDeleted = true;
                    lead.IsMergedDuplicate = true;
                    lead.MergedIntoId = masterRecordId;
                    lead.MergeGroupId = mergeGroupId;
                    lead.MergedAt = DateTime.UtcNow;
                }
                break;

            case DuplicateEntityType.Contact:
                var contact = await _context.Set<Contact>().FindAsync(new object[] { recordId }, cancellationToken);
                if (contact != null)
                {
                    contact.Status = ContactStatus.Archived;
                    contact.IsMergedDuplicate = true;
                    contact.MergedIntoId = masterRecordId;
                    contact.MergeGroupId = mergeGroupId;
                    contact.MergedAt = DateTime.UtcNow;
                }
                break;

            case DuplicateEntityType.Account:
                var account = await _context.Set<Account>().FindAsync(new object[] { recordId }, cancellationToken);
                if (account != null)
                {
                    account.IsDeleted = true;
                    account.IsMergedDuplicate = true;
                    account.MergedIntoId = masterRecordId;
                    account.MergeGroupId = mergeGroupId;
                    account.MergedAt = DateTime.UtcNow;
                }
                break;
        }
    }

    private async Task<bool> RestoreRecordFromSnapshotAsync(
        DuplicateMergeGroupMember member,
        DuplicateEntityType entityType,
        bool restoreRelatedRecords,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(member.RecordSnapshot))
        {
            _logger.LogWarning("No snapshot available for record {RecordId}", member.RecordId);
            return false;
        }

        try
        {
            switch (entityType)
            {
                case DuplicateEntityType.Lead:
                    await RestoreLeadFromSnapshotAsync(member, cancellationToken);
                    break;

                case DuplicateEntityType.Contact:
                    await RestoreContactFromSnapshotAsync(member, cancellationToken);
                    break;

                case DuplicateEntityType.Account:
                    await RestoreAccountFromSnapshotAsync(member, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unsupported entity type {EntityType} for unmerge", entityType);
                    return false;
            }

            // Reverse related record relinking if requested
            if (restoreRelatedRecords && !string.IsNullOrEmpty(member.RelinkedRecords))
            {
                var masterRecordId = member.MergeGroup?.MasterRecordId ?? 0;
                if (masterRecordId > 0)
                {
                    await ReverseRelinkRelatedRecordsAsync(
                        member.RecordId,
                        masterRecordId,
                        entityType,
                        member.RelinkedRecords,
                        cancellationToken);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore record {RecordId} from snapshot", member.RecordId);
            return false;
        }
    }

    private async Task RestoreLeadFromSnapshotAsync(
        DuplicateMergeGroupMember member,
        CancellationToken cancellationToken)
    {
        var lead = await _context.Set<Lead>().FindAsync(new object[] { member.RecordId }, cancellationToken);
        if (lead == null)
        {
            _logger.LogWarning("Lead {RecordId} not found for unmerge", member.RecordId);
            return;
        }

        // Deserialize snapshot to restore original field values
        var snapshot = JsonSerializer.Deserialize<JsonElement>(member.RecordSnapshot!);
        RestoreFieldsFromSnapshot(lead, snapshot, GetMergeableFields(DuplicateEntityType.Lead));

        // Clear merge flags
        lead.IsDeleted = false;
        lead.IsMergedDuplicate = false;
        lead.MergedIntoId = null;
        lead.MergeGroupId = null;
        lead.MergedAt = null;
        lead.UpdatedAt = DateTime.UtcNow;
    }

    private async Task RestoreContactFromSnapshotAsync(
        DuplicateMergeGroupMember member,
        CancellationToken cancellationToken)
    {
        var contact = await _context.Set<Contact>().FindAsync(new object[] { member.RecordId }, cancellationToken);
        if (contact == null)
        {
            _logger.LogWarning("Contact {RecordId} not found for unmerge", member.RecordId);
            return;
        }

        // Deserialize snapshot to restore original field values
        var snapshot = JsonSerializer.Deserialize<JsonElement>(member.RecordSnapshot!);
        RestoreFieldsFromSnapshot(contact, snapshot, GetMergeableFields(DuplicateEntityType.Contact));

        // Restore original status from snapshot if available
        if (snapshot.TryGetProperty("Status", out var statusProp))
        {
            if (Enum.TryParse<ContactStatus>(statusProp.GetRawText().Trim('"'), true, out var originalStatus))
            {
                contact.Status = originalStatus;
            }
            else if (statusProp.ValueKind == JsonValueKind.Number)
            {
                contact.Status = (ContactStatus)statusProp.GetInt32();
            }
        }
        else
        {
            contact.Status = ContactStatus.Active;
        }

        // Clear merge flags
        contact.IsMergedDuplicate = false;
        contact.MergedIntoId = null;
        contact.MergeGroupId = null;
        contact.MergedAt = null;
    }

    private async Task RestoreAccountFromSnapshotAsync(
        DuplicateMergeGroupMember member,
        CancellationToken cancellationToken)
    {
        var account = await _context.Set<Account>().FindAsync(new object[] { member.RecordId }, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account {RecordId} not found for unmerge", member.RecordId);
            return;
        }

        // Deserialize snapshot to restore original field values
        var snapshot = JsonSerializer.Deserialize<JsonElement>(member.RecordSnapshot!);
        RestoreFieldsFromSnapshot(account, snapshot, GetMergeableFields(DuplicateEntityType.Account));

        // Clear merge flags
        account.IsDeleted = false;
        account.IsMergedDuplicate = false;
        account.MergedIntoId = null;
        account.MergeGroupId = null;
        account.MergedAt = null;
        account.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores specific field values on an entity from a JSON snapshot.
    /// Only restores the mergeable fields to avoid overwriting system fields.
    /// </summary>
    private static void RestoreFieldsFromSnapshot(object entity, JsonElement snapshot, List<string> fieldNames)
    {
        var entityType = entity.GetType();

        foreach (var fieldName in fieldNames)
        {
            if (!snapshot.TryGetProperty(fieldName, out var jsonValue))
                continue;

            var property = entityType.GetProperty(fieldName);
            if (property == null || !property.CanWrite)
                continue;

            try
            {
                if (jsonValue.ValueKind == JsonValueKind.Null)
                {
                    // Only set null if property is nullable
                    if (Nullable.GetUnderlyingType(property.PropertyType) != null || !property.PropertyType.IsValueType)
                    {
                        property.SetValue(entity, null);
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(entity, jsonValue.GetString());
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                {
                    property.SetValue(entity, jsonValue.GetInt32());
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                {
                    property.SetValue(entity, jsonValue.GetDecimal());
                }
                else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    property.SetValue(entity, jsonValue.GetBoolean());
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    property.SetValue(entity, jsonValue.GetDateTime());
                }
                else
                {
                    // Fallback: deserialize the JSON element to the target type
                    var value = JsonSerializer.Deserialize(jsonValue.GetRawText(), property.PropertyType);
                    property.SetValue(entity, value);
                }
            }
            catch
            {
                // Skip fields that can't be deserialized — better to partially restore than fail entirely
            }
        }
    }

    /// <summary>
    /// Reverses related record relinking by moving records back from the master to the original source.
    /// Uses the RelinkedRecords JSON stored during merge to know what was moved.
    /// </summary>
    private async Task ReverseRelinkRelatedRecordsAsync(
        int sourceRecordId,
        int masterRecordId,
        DuplicateEntityType entityType,
        string relinkedRecordsJson,
        CancellationToken cancellationToken)
    {
        Dictionary<string, int>? relinked;
        try
        {
            relinked = JsonSerializer.Deserialize<Dictionary<string, int>>(relinkedRecordsJson);
        }
        catch
        {
            _logger.LogWarning(
                "Could not deserialize RelinkedRecords for record {RecordId}", sourceRecordId);
            return;
        }

        if (relinked == null || relinked.Count == 0)
            return;

        switch (entityType)
        {
            case DuplicateEntityType.Lead:
                if (relinked.ContainsKey("Notes") && relinked["Notes"] > 0)
                {
                    // Notes that were relinked have EntityType=Lead and EntityId=masterRecordId
                    // We relink the ones that were originally on the source (up to the count stored)
                    var notes = await _context.Set<Note>()
                        .Where(n => n.EntityType == "Lead" && n.EntityId == masterRecordId)
                        .Take(relinked["Notes"])
                        .ToListAsync(cancellationToken);
                    foreach (var note in notes)
                    {
                        note.EntityId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Opportunities") && relinked["Opportunities"] > 0)
                {
                    var opportunities = await _context.Set<Opportunity>()
                        .Where(o => o.LeadId == masterRecordId)
                        .Take(relinked["Opportunities"])
                        .ToListAsync(cancellationToken);
                    foreach (var opp in opportunities)
                    {
                        opp.LeadId = sourceRecordId;
                    }
                }
                break;

            case DuplicateEntityType.Contact:
                if (relinked.ContainsKey("AccountContacts") && relinked["AccountContacts"] > 0)
                {
                    var accountContacts = await _context.Set<AccountContact>()
                        .Where(ac => ac.ContactId == masterRecordId)
                        .Take(relinked["AccountContacts"])
                        .ToListAsync(cancellationToken);
                    foreach (var ac in accountContacts)
                    {
                        // Only relink if the source doesn't already have this relationship
                        var exists = await _context.Set<AccountContact>()
                            .AnyAsync(x => x.ContactId == sourceRecordId && x.AccountId == ac.AccountId, cancellationToken);
                        if (!exists)
                        {
                            ac.ContactId = sourceRecordId;
                        }
                    }
                }

                if (relinked.ContainsKey("Interactions") && relinked["Interactions"] > 0)
                {
                    var interactions = await _context.Set<Interaction>()
                        .Where(i => i.ContactId == masterRecordId)
                        .Take(relinked["Interactions"])
                        .ToListAsync(cancellationToken);
                    foreach (var interaction in interactions)
                    {
                        interaction.ContactId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Activities") && relinked["Activities"] > 0)
                {
                    var activities = await _context.Set<Activity>()
                        .Where(a => a.ContactId == masterRecordId)
                        .Take(relinked["Activities"])
                        .ToListAsync(cancellationToken);
                    foreach (var activity in activities)
                    {
                        activity.ContactId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Notes") && relinked["Notes"] > 0)
                {
                    var notes = await _context.Set<Note>()
                        .Where(n => n.EntityType == "Contact" && n.EntityId == masterRecordId)
                        .Take(relinked["Notes"])
                        .ToListAsync(cancellationToken);
                    foreach (var note in notes)
                    {
                        note.EntityId = sourceRecordId;
                    }
                }
                break;

            case DuplicateEntityType.Account:
                if (relinked.ContainsKey("Opportunities") && relinked["Opportunities"] > 0)
                {
                    var opportunities = await _context.Set<Opportunity>()
                        .Where(o => o.AccountId == masterRecordId)
                        .Take(relinked["Opportunities"])
                        .ToListAsync(cancellationToken);
                    foreach (var opp in opportunities)
                    {
                        opp.AccountId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Contacts") && relinked["Contacts"] > 0)
                {
                    var contacts = await _context.Set<Contact>()
                        .Where(c => c.AccountId == masterRecordId)
                        .Take(relinked["Contacts"])
                        .ToListAsync(cancellationToken);
                    foreach (var contact in contacts)
                    {
                        contact.AccountId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Interactions") && relinked["Interactions"] > 0)
                {
                    var interactions = await _context.Set<Interaction>()
                        .Where(i => i.AccountId == masterRecordId)
                        .Take(relinked["Interactions"])
                        .ToListAsync(cancellationToken);
                    foreach (var interaction in interactions)
                    {
                        interaction.AccountId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Activities") && relinked["Activities"] > 0)
                {
                    var activities = await _context.Set<Activity>()
                        .Where(a => a.AccountId == masterRecordId)
                        .Take(relinked["Activities"])
                        .ToListAsync(cancellationToken);
                    foreach (var activity in activities)
                    {
                        activity.AccountId = sourceRecordId;
                    }
                }

                if (relinked.ContainsKey("Notes") && relinked["Notes"] > 0)
                {
                    var notes = await _context.Set<Note>()
                        .Where(n => n.EntityType == "Account" && n.EntityId == masterRecordId)
                        .Take(relinked["Notes"])
                        .ToListAsync(cancellationToken);
                    foreach (var note in notes)
                    {
                        note.EntityId = sourceRecordId;
                    }
                }
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reversed related record relinking for {EntityType} record {RecordId} (master: {MasterId})",
            entityType, sourceRecordId, masterRecordId);
    }

    /// <summary>
    /// Reverses field overrides that were applied to the master record during merge.
    /// FieldValuesUsed stores which fields were copied from a source record to the master.
    /// Since we don't have the master's original pre-merge values, we null out the overridden
    /// fields on the master — the source record's snapshot already restored those values
    /// back onto the source entity.
    /// </summary>
    private async Task ReverseFieldOverridesOnMasterAsync(
        int masterRecordId,
        DuplicateEntityType entityType,
        string fieldValuesUsedJson,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string>? fieldsUsed;
        try
        {
            fieldsUsed = JsonSerializer.Deserialize<Dictionary<string, string>>(fieldValuesUsedJson);
        }
        catch
        {
            _logger.LogWarning(
                "Could not deserialize FieldValuesUsed for master record {MasterId}", masterRecordId);
            return;
        }

        if (fieldsUsed == null || fieldsUsed.Count == 0)
            return;

        object? masterEntity = entityType switch
        {
            DuplicateEntityType.Lead => await _context.Set<Lead>()
                .FindAsync(new object[] { masterRecordId }, cancellationToken),
            DuplicateEntityType.Contact => await _context.Set<Contact>()
                .FindAsync(new object[] { masterRecordId }, cancellationToken),
            DuplicateEntityType.Account => await _context.Set<Account>()
                .FindAsync(new object[] { masterRecordId }, cancellationToken),
            _ => null
        };

        if (masterEntity == null)
        {
            _logger.LogWarning("Master record {MasterId} not found for field override reversal", masterRecordId);
            return;
        }

        // Check if the master member has a snapshot we can use to restore original values
        var masterMember = await _context.Set<DuplicateMergeGroupMember>()
            .FirstOrDefaultAsync(m => m.RecordId == masterRecordId && m.IsMaster && m.RecordSnapshot != null,
                cancellationToken);

        if (masterMember?.RecordSnapshot != null)
        {
            // We have a master snapshot — restore only the overridden fields from it
            try
            {
                var masterSnapshot = JsonSerializer.Deserialize<JsonElement>(masterMember.RecordSnapshot);
                var overriddenFields = fieldsUsed.Keys.ToList();
                RestoreFieldsFromSnapshot(masterEntity, masterSnapshot, overriddenFields);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to restore master fields from snapshot, clearing overridden fields instead");
                ClearOverriddenFields(masterEntity, fieldsUsed.Keys);
            }
        }
        else
        {
            // No master snapshot available — clear the overridden fields to null
            // This is the safe fallback: we can't know the master's original values
            ClearOverriddenFields(masterEntity, fieldsUsed.Keys);
        }

        _logger.LogInformation(
            "Reversed {Count} field override(s) on master record {MasterId}",
            fieldsUsed.Count, masterRecordId);
    }

    /// <summary>
    /// Clears field values on an entity by setting them to null (for nullable/string properties).
    /// Used as a fallback when the master's original values are not available.
    /// </summary>
    private static void ClearOverriddenFields(object entity, IEnumerable<string> fieldNames)
    {
        var entityType = entity.GetType();
        foreach (var fieldName in fieldNames)
        {
            var property = entityType.GetProperty(fieldName);
            if (property == null || !property.CanWrite)
                continue;

            if (property.PropertyType == typeof(string) ||
                Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                property.SetValue(entity, null);
            }
        }
    }

    /// <summary>
    /// Counts total relinked records from the stored JSON.
    /// </summary>
    private static Task<int> CountRelinkedRecordsAsync(string? relinkedRecordsJson)
    {
        if (string.IsNullOrEmpty(relinkedRecordsJson))
            return Task.FromResult(0);

        try
        {
            var relinked = JsonSerializer.Deserialize<Dictionary<string, int>>(relinkedRecordsJson);
            return Task.FromResult(relinked?.Values.Sum() ?? 0);
        }
        catch
        {
            return Task.FromResult(0);
        }
    }

    private async Task<List<RecordForPreview>> LoadRecordsForPreviewAsync(
        DuplicateEntityType entityType,
        List<int> recordIds,
        CancellationToken cancellationToken)
    {
        var records = new List<RecordForPreview>();

        switch (entityType)
        {
            case DuplicateEntityType.Lead:
                var leads = await _context.Set<Lead>()
                    .Where(l => recordIds.Contains(l.Id))
                    .ToListAsync(cancellationToken);
                records.AddRange(leads.Select(l => new RecordForPreview
                {
                    Id = l.Id,
                    Data = l
                }));
                break;

            case DuplicateEntityType.Contact:
                var contacts = await _context.Set<Contact>()
                    .Where(c => recordIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);
                records.AddRange(contacts.Select(c => new RecordForPreview
                {
                    Id = c.Id,
                    Data = c
                }));
                break;

            case DuplicateEntityType.Account:
                var accounts = await _context.Set<Account>()
                    .Where(a => recordIds.Contains(a.Id))
                    .ToListAsync(cancellationToken);
                records.AddRange(accounts.Select(a => new RecordForPreview
                {
                    Id = a.Id,
                    Data = a
                }));
                break;
        }

        return records;
    }

    private async Task<List<RelatedRecordPreview>> GetRelatedRecordsPreviewAsync(
        DuplicateEntityType entityType,
        List<int> recordIds,
        CancellationToken cancellationToken)
    {
        var previews = new List<RelatedRecordPreview>();

        switch (entityType)
        {
            case DuplicateEntityType.Lead:
                // Count opportunities linked to these leads
                var leadOppCount = await _context.Set<Opportunity>()
                    .CountAsync(o => o.LeadId.HasValue && recordIds.Contains(o.LeadId.Value), cancellationToken);
                if (leadOppCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "Opportunity",
                        Count = leadOppCount,
                        Description = $"{leadOppCount} opportunity(ies) will be relinked"
                    });
                }

                // Count notes
                var leadNoteCount = await _context.Set<Note>()
                    .CountAsync(n => n.EntityType == "Lead" && n.EntityId.HasValue && recordIds.Contains(n.EntityId.Value), cancellationToken);
                if (leadNoteCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "Note",
                        Count = leadNoteCount,
                        Description = $"{leadNoteCount} note(s) will be relinked"
                    });
                }
                break;

            case DuplicateEntityType.Contact:
                // Count interactions linked to these contacts
                var contactInteractionCount = await _context.Set<Interaction>()
                    .CountAsync(i => i.ContactId.HasValue && recordIds.Contains(i.ContactId.Value), cancellationToken);
                if (contactInteractionCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "Interaction",
                        Count = contactInteractionCount,
                        Description = $"{contactInteractionCount} interaction(s) will be relinked"
                    });
                }

                // Count account contacts
                var accountContactCount = await _context.Set<AccountContact>()
                    .CountAsync(ac => recordIds.Contains(ac.ContactId), cancellationToken);
                if (accountContactCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "AccountContact",
                        Count = accountContactCount,
                        Description = $"{accountContactCount} account relationship(s) will be relinked"
                    });
                }
                break;

            case DuplicateEntityType.Account:
                var oppCount = await _context.Set<Opportunity>()
                    .CountAsync(o => recordIds.Contains(o.AccountId), cancellationToken);
                if (oppCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "Opportunity",
                        Count = oppCount,
                        Description = $"{oppCount} opportunity(ies) will be relinked"
                    });
                }

                // Count contacts
                var contactCount = await _context.Set<Contact>()
                    .CountAsync(c => c.AccountId.HasValue && recordIds.Contains(c.AccountId.Value), cancellationToken);
                if (contactCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "Contact",
                        Count = contactCount,
                        Description = $"{contactCount} contact(s) will be relinked"
                    });
                }

                // Count interactions
                var accountInteractionCount = await _context.Set<Interaction>()
                    .CountAsync(i => i.AccountId.HasValue && recordIds.Contains(i.AccountId.Value), cancellationToken);
                if (accountInteractionCount > 0)
                {
                    previews.Add(new RelatedRecordPreview
                    {
                        EntityType = "Interaction",
                        Count = accountInteractionCount,
                        Description = $"{accountInteractionCount} interaction(s) will be relinked"
                    });
                }
                break;
        }

        return previews;
    }

    private List<string> GetMergeableFields(DuplicateEntityType entityType)
    {
        return entityType switch
        {
            DuplicateEntityType.Lead => new List<string>
            {
                "FirstName", "LastName", "Email", "Phone", "CompanyName", "Title", "Website", "Region", "QualificationNotes"
            },
            DuplicateEntityType.Contact => new List<string>
            {
                "FirstName", "LastName", "EmailPrimary", "PhonePrimary", "Company", "JobTitle", "Address", "City", "State", "ZipCode"
            },
            DuplicateEntityType.Account => new List<string>
            {
                "FirstName", "LastName", "Company", "Email", "Phone", "Website", "Address", "City", "State", "ZipCode", "Industry"
            },
            _ => new List<string>()
        };
    }

    private string? GetFieldValue(RecordForPreview record, string fieldName)
    {
        var property = record.Data?.GetType().GetProperty(fieldName);
        return property?.GetValue(record.Data)?.ToString();
    }

    private async Task<string?> GetFieldValueFromRecordAsync(
        int recordId,
        DuplicateEntityType entityType,
        string fieldName,
        CancellationToken cancellationToken)
    {
        object? record = entityType switch
        {
            DuplicateEntityType.Lead => await _context.Set<Lead>().FindAsync(new object[] { recordId }, cancellationToken),
            DuplicateEntityType.Contact => await _context.Set<Contact>().FindAsync(new object[] { recordId }, cancellationToken),
            DuplicateEntityType.Account => await _context.Set<Account>().FindAsync(new object[] { recordId }, cancellationToken),
            _ => null
        };

        if (record == null)
            return null;

        var property = record.GetType().GetProperty(fieldName);
        return property?.GetValue(record)?.ToString();
    }

    private async Task SetFieldValueOnRecordAsync(
        int recordId,
        DuplicateEntityType entityType,
        string fieldName,
        string? value,
        CancellationToken cancellationToken)
    {
        object? record = entityType switch
        {
            DuplicateEntityType.Lead => await _context.Set<Lead>().FindAsync(new object[] { recordId }, cancellationToken),
            DuplicateEntityType.Contact => await _context.Set<Contact>().FindAsync(new object[] { recordId }, cancellationToken),
            DuplicateEntityType.Account => await _context.Set<Account>().FindAsync(new object[] { recordId }, cancellationToken),
            _ => null
        };

        if (record == null)
            return;

        var property = record.GetType().GetProperty(fieldName);
        if (property != null && property.CanWrite)
        {
            var convertedValue = Convert.ChangeType(value, property.PropertyType);
            property.SetValue(record, convertedValue);
        }
    }

    private string GetFieldDisplayName(string fieldName)
    {
        return fieldName switch
        {
            "FirstName" => "First Name",
            "LastName" => "Last Name",
            "CompanyName" => "Company",
            "EmailPrimary" => "Email",
            "PhonePrimary" => "Phone",
            "JobTitle" => "Job Title",
            "QualificationNotes" => "Notes",
            "ZipCode" => "ZIP Code",
            _ => fieldName
        };
    }

    #endregion

    #region Helper Classes

    private class RecordForPreview
    {
        public int Id { get; set; }
        public object? Data { get; set; }
    }

    #endregion
}
