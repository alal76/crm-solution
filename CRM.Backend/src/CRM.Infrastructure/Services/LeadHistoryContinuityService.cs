// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// REM-LEAD-HISTORY-CONTINUITY: a follow-up to <see cref="LeadBackfillService"/>. Re-parents
/// <see cref="Activity"/>/<see cref="RecordComment"/> history that was created against the OLD
/// Contact record (<c>EntityType == "Contact"</c>) onto the migrated <see cref="Lead"/> record
/// (<c>EntityType == "Lead"</c>) so history shows up on the new Lead's timeline.
///
/// This is a TOOL, not an automatic migration. It is registered in DI but never invoked
/// automatically — it must be explicitly triggered (see the admin-only lead-backfill
/// controller endpoint, which defaults to dry-run).
/// </summary>
public class LeadHistoryContinuityService : ILeadHistoryContinuityService
{
    private const string ContactEntityType = "Contact";
    private const string LeadEntityType = "Lead";

    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadHistoryContinuityService> _logger;

    public LeadHistoryContinuityService(ICrmDbContext context, ILogger<LeadHistoryContinuityService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LeadHistoryContinuityResult> RunAsync(bool dryRun, CancellationToken ct = default)
    {
        var result = new LeadHistoryContinuityResult { DryRun = dryRun };

        // Migrated leads: Leads with a non-null ContactId (created by LeadBackfillService).
        var migratedLeads = await _context.Leads
            .Where(l => l.ContactId.HasValue)
            .ToListAsync(ct);

        result.TotalMigratedLeadsFound = migratedLeads.Count;

        if (migratedLeads.Count == 0)
        {
            _logger.LogInformation("LeadHistoryContinuity: no backfill-migrated Leads found. Nothing to do.");
            return result;
        }

        // Map old ContactId -> Lead. If more than one Lead somehow points at the same
        // Contact (should not happen given LeadBackfillService's idempotency guarantee),
        // the first Lead found wins and a warning is logged; the tool must never crash.
        var contactIdToLead = new Dictionary<int, Lead>();
        foreach (var lead in migratedLeads)
        {
            var contactId = lead.ContactId!.Value;
            if (!contactIdToLead.TryAdd(contactId, lead))
            {
                _logger.LogWarning(
                    "LeadHistoryContinuity: multiple Leads reference ContactId={ContactId}; using LeadId={LeadId}, skipping LeadId={SkippedLeadId} for history re-parenting.",
                    contactId, contactIdToLead[contactId].Id, lead.Id);
            }
        }

        var contactIds = contactIdToLead.Keys.ToList();

        // Idempotency: only rows still pointing at EntityType == "Contact" with an old
        // Contact id are matched. Once re-parented to EntityType == "Lead" they no longer
        // match this query, so re-running naturally skips them.
        var activitiesToMove = await _context.Activities
            .Where(a => a.EntityType == ContactEntityType && a.EntityId.HasValue && contactIds.Contains(a.EntityId.Value))
            .ToListAsync(ct);

        var commentsToMove = await _context.RecordComments
            .Where(c => c.EntityType == ContactEntityType && contactIds.Contains(c.EntityId))
            .ToListAsync(ct);

        var leadsWithHistory = new HashSet<int>();

        foreach (var activity in activitiesToMove)
        {
            ct.ThrowIfCancellationRequested();
            var lead = contactIdToLead[activity.EntityId!.Value];
            leadsWithHistory.Add(lead.Id);
            result.ActivitiesReparentedCount++;

            if (!dryRun)
            {
                activity.EntityType = LeadEntityType;
                activity.EntityId = lead.Id;
            }
        }

        foreach (var comment in commentsToMove)
        {
            ct.ThrowIfCancellationRequested();
            var lead = contactIdToLead[comment.EntityId];
            leadsWithHistory.Add(lead.Id);
            result.CommentsReparentedCount++;

            if (!dryRun)
            {
                comment.EntityType = LeadEntityType;
                comment.EntityId = lead.Id;
            }
        }

        result.LeadsProcessedCount = leadsWithHistory.Count;
        result.LeadsSkippedNoHistoryCount = migratedLeads.Count - leadsWithHistory.Count;

        if (!dryRun && (activitiesToMove.Count > 0 || commentsToMove.Count > 0))
        {
            await _context.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "LeadHistoryContinuity run complete. DryRun={DryRun} MigratedLeads={MigratedLeads} LeadsProcessed={LeadsProcessed} LeadsSkippedNoHistory={LeadsSkipped} ActivitiesReparented={Activities} CommentsReparented={Comments}",
            dryRun, result.TotalMigratedLeadsFound, result.LeadsProcessedCount, result.LeadsSkippedNoHistoryCount,
            result.ActivitiesReparentedCount, result.CommentsReparentedCount);

        return result;
    }
}
