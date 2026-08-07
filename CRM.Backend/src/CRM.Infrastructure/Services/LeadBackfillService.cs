// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContactModel = CRM.Core.Models.Contact;
using ContactTypeEnum = CRM.Core.Models.ContactType;

namespace CRM.Infrastructure.Services;

/// <summary>
/// REM-ORPHAN-003 groundwork: backfills the real <see cref="Lead"/> table from legacy
/// "Contacts-as-Leads" data (<see cref="ContactModel"/> rows with ContactType == Lead
/// whose source/status were packed as JSON into the free-text Notes field).
///
/// This is a TOOL, not an automatic migration. It is registered in DI but never invoked
/// automatically — it must be explicitly triggered (see the admin-only lead-backfill
/// controller endpoint, which defaults to dry-run).
/// </summary>
public class LeadBackfillService : ILeadBackfillService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadBackfillService> _logger;

    /// <summary>
    /// Legacy frontend values (see CRM.Frontend/src/pages/LeadsPage.tsx LEAD_SOURCES) mapped
    /// to the closest matching <see cref="LeadSource"/> enum member. Values with no exact
    /// counterpart in the real enum (e.g. "social", "other") use a best-effort match here
    /// and are NOT reported as unmapped — <see cref="LeadBackfillResult.UnmappedEnumValues"/>
    /// only records raw values outside this known table entirely.
    /// </summary>
    private static readonly Dictionary<string, LeadSource> KnownSourceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["website"] = LeadSource.Web,
        ["referral"] = LeadSource.Referral,
        ["event"] = LeadSource.Event,
        ["cold_call"] = LeadSource.Manual,
        ["social"] = LeadSource.Campaign,
        ["other"] = LeadSource.Manual,
    };

    /// <summary>
    /// Legacy frontend values (see CRM.Frontend/src/pages/LeadsPage.tsx LEAD_STATUSES) mapped
    /// to the closest matching <see cref="LeadLifecycleStatus"/> enum member.
    /// </summary>
    private static readonly Dictionary<string, LeadLifecycleStatus> KnownStatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["new"] = LeadLifecycleStatus.New,
        ["contacted"] = LeadLifecycleStatus.Working,
        ["qualified"] = LeadLifecycleStatus.Qualified,
        ["converted"] = LeadLifecycleStatus.Converted,
        ["lost"] = LeadLifecycleStatus.Disqualified,
    };

    public LeadBackfillService(ICrmDbContext context, ILogger<LeadBackfillService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LeadBackfillResult> RunAsync(bool dryRun, CancellationToken ct = default)
    {
        var result = new LeadBackfillResult { DryRun = dryRun };

        // Legacy leads: Contacts with ContactType == Lead.
        var leadContacts = await _context.Contacts
            .Where(c => c.ContactType == ContactTypeEnum.Lead)
            .ToListAsync(ct);

        result.TotalLeadContactsFound = leadContacts.Count;

        if (leadContacts.Count == 0)
        {
            _logger.LogInformation("LeadBackfill: no legacy Lead-type Contacts found. Nothing to do.");
            return result;
        }

        // Idempotency check: a Contact is "already migrated" if a Lead already exists
        // referencing it via Lead.ContactId. No new column on Contact is required.
        var contactIds = leadContacts.Select(c => c.Id).ToList();
        var alreadyMigratedContactIds = await _context.Leads
            .Where(l => l.ContactId.HasValue && contactIds.Contains(l.ContactId.Value))
            .Select(l => l.ContactId!.Value)
            .Distinct()
            .ToListAsync(ct);
        var alreadyMigratedSet = new HashSet<int>(alreadyMigratedContactIds);
        var createdLeads = new List<Lead>();

        foreach (var contact in leadContacts)
        {
            ct.ThrowIfCancellationRequested();

            if (alreadyMigratedSet.Contains(contact.Id))
            {
                result.SkippedAlreadyMigratedCount++;
                continue;
            }

            var (source, status, parsedNotes, parseSucceeded) = ParseLeadMetadata(contact.Notes);
            if (!parseSucceeded)
            {
                result.ParseErrorCount++;
                result.ParseErrorContactIds.Add(contact.Id);
            }

            var leadSource = MapSource(source, contact.Id, result);
            var leadStatus = MapStatus(status, contact.Id, result);

            var lead = new Lead
            {
                ContactId = contact.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.EmailPrimary ?? string.Empty,
                Phone = contact.PhonePrimary,
                Title = contact.JobTitle,
                CompanyName = contact.Company,
                Website = contact.Website,
                Source = leadSource,
                Status = leadStatus,
                OriginalSource = source,
                QualificationNotes = parsedNotes,
                OwnerId = contact.OwnerId ?? contact.AssignedToUserId,
                CreatedAt = contact.DateAdded == default ? DateTime.UtcNow : contact.DateAdded,
                UpdatedAt = DateTime.UtcNow,
            };

            result.CreatedCount++;

            if (!dryRun)
            {
                _context.Leads.Add(lead);
                createdLeads.Add(lead);
            }
        }

        if (!dryRun && createdLeads.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            // Ids are only assigned by the provider once SaveChangesAsync completes.
            result.CreatedLeadIds = createdLeads.Select(l => l.Id).ToList();
        }

        _logger.LogInformation(
            "LeadBackfill run complete. DryRun={DryRun} Found={Found} Created={Created} SkippedAlreadyMigrated={Skipped} ParseErrors={ParseErrors}",
            dryRun, result.TotalLeadContactsFound, result.CreatedCount, result.SkippedAlreadyMigratedCount, result.ParseErrorCount);

        return result;
    }

    /// <summary>
    /// Parses the Contact.Notes field as legacy lead metadata JSON: <c>{ source, status, notes }</c>.
    /// Handles malformed/non-JSON Notes gracefully by returning defaults and parseSucceeded=false —
    /// callers must NOT throw, a Lead should still be created with identity fields only.
    /// </summary>
    private (string? Source, string? Status, string? Notes, bool ParseSucceeded) ParseLeadMetadata(string? notes)
    {
        // No notes at all is not an error condition - there is simply no metadata to parse.
        if (string.IsNullOrWhiteSpace(notes))
        {
            return (null, null, notes, true);
        }

        // Has content but does not even look like JSON - a genuine parse error to report.
        if (!notes.TrimStart().StartsWith('{'))
        {
            return (null, null, notes, false);
        }

        try
        {
            var meta = JsonSerializer.Deserialize<LeadNotesMetadata>(notes, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            if (meta is null)
            {
                return (null, null, notes, false);
            }

            return (meta.Source, meta.Status, meta.Notes, true);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "LeadBackfill: failed to parse Notes JSON for a Contact; falling back to defaults.");
            return (null, null, notes, false);
        }
    }

    private static LeadSource MapSource(string? raw, int contactId, LeadBackfillResult result)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return LeadSource.Web;
        }

        if (KnownSourceMap.TryGetValue(raw, out var mapped))
        {
            return mapped;
        }

        if (Enum.TryParse<LeadSource>(raw, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        result.UnmappedEnumValues.Add($"contactId={contactId};field=source;rawValue={raw}");
        return LeadSource.Web;
    }

    private static LeadLifecycleStatus MapStatus(string? raw, int contactId, LeadBackfillResult result)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return LeadLifecycleStatus.New;
        }

        if (KnownStatusMap.TryGetValue(raw, out var mapped))
        {
            return mapped;
        }

        if (Enum.TryParse<LeadLifecycleStatus>(raw, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        result.UnmappedEnumValues.Add($"contactId={contactId};field=status;rawValue={raw}");
        return LeadLifecycleStatus.New;
    }

    /// <summary>Shape of the JSON packed into Contact.Notes for legacy leads (frontend LeadsPage.tsx).</summary>
    private sealed class LeadNotesMetadata
    {
        public string? Source { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
