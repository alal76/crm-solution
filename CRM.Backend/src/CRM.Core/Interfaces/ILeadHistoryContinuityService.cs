// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// REM-LEAD-HISTORY-CONTINUITY: a follow-up to the <see cref="ILeadBackfillService"/> tool.
///
/// The backfill tool creates real <c>Lead</c> rows from legacy "Contacts-as-Leads" data and
/// records provenance via <c>Lead.ContactId</c>, but it does NOT re-point the <c>Activity</c>
/// and <c>RecordComment</c> history that was created against the OLD Contact record
/// (<c>EntityType == "Contact"</c>, <c>EntityId == &lt;old contact id&gt;</c>). Without this
/// tool, that history stays "orphaned" from the new Lead's point of view — it still shows on
/// the legacy Contact's timeline instead of the migrated Lead's timeline.
///
/// This service is a TOOL, not an automatic migration — it is never invoked at startup and
/// must be explicitly triggered (e.g. via the admin-only
/// <c>POST /api/admin/lead-backfill/history-continuity</c> endpoint), which defaults to dry-run.
///
/// Idempotency: only <c>Activity</c>/<c>RecordComment</c> rows still pointing at
/// <c>EntityType == "Contact"</c> with the old Contact id are matched and updated. Once a row
/// has been re-pointed to <c>EntityType == "Lead"</c>, it no longer matches the old-Contact
/// query, so re-running this tool naturally skips it without any extra bookkeeping.
/// </summary>
public interface ILeadHistoryContinuityService
{
    /// <summary>
    /// Scans every <see cref="CRM.Core.Entities.Lead"/> with a non-null <c>ContactId</c>
    /// (i.e. a Lead migrated from the legacy Contacts-as-Leads system) and re-parents any
    /// <c>Activity</c>/<c>RecordComment</c> rows still referencing the old Contact
    /// (<c>EntityType == "Contact"</c>, <c>EntityId == Lead.ContactId</c>) onto the Lead
    /// (<c>EntityType == "Lead"</c>, <c>EntityId == Lead.Id</c>).
    /// </summary>
    /// <param name="dryRun">
    /// When true (the default/safe mode), no database writes occur — the result still
    /// reports accurate counts of what would happen.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<LeadHistoryContinuityResult> RunAsync(bool dryRun, CancellationToken ct = default);
}

/// <summary>
/// Outcome of a <see cref="ILeadHistoryContinuityService.RunAsync"/> run.
/// </summary>
public class LeadHistoryContinuityResult
{
    /// <summary>True if this run did not write to the database.</summary>
    public bool DryRun { get; set; }

    /// <summary>Total number of Leads examined that have a non-null ContactId (i.e. backfill-migrated).</summary>
    public int TotalMigratedLeadsFound { get; set; }

    /// <summary>Number of Leads for which at least one Activity or RecordComment was (or would be) re-parented.</summary>
    public int LeadsProcessedCount { get; set; }

    /// <summary>Number of migrated Leads with no old-Contact-typed Activity/RecordComment history to move.</summary>
    public int LeadsSkippedNoHistoryCount { get; set; }

    /// <summary>Number of Activity rows re-parented (or, in dry-run mode, that would be re-parented).</summary>
    public int ActivitiesReparentedCount { get; set; }

    /// <summary>Number of RecordComment rows re-parented (or, in dry-run mode, that would be re-parented).</summary>
    public int CommentsReparentedCount { get; set; }
}
