// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// REM-ORPHAN-003 groundwork: backfills real <c>Lead</c> table rows from legacy
/// "Contacts-as-Leads" data (Contact rows with ContactType = Lead whose source/status
/// were packed as JSON into the free-text Notes field).
///
/// This service is a TOOL, not an automatic migration — it is never invoked at
/// startup and must be explicitly triggered (e.g. via the admin-only
/// <c>POST /api/admin/lead-backfill</c> endpoint), which defaults to dry-run.
///
/// Idempotency: a Contact is considered "already migrated" when a Lead row exists
/// with <c>Lead.ContactId</c> equal to the Contact's Id. No new column/flag is needed
/// on Contact because Lead already carries this foreign key back to its source Contact.
/// </summary>
public interface ILeadBackfillService
{
    /// <summary>
    /// Scans Contacts with ContactType == Lead that have not yet been migrated and
    /// creates corresponding Lead rows.
    /// </summary>
    /// <param name="dryRun">
    /// When true (the default/safe mode), no database writes occur — the result still
    /// reports accurate counts of what would happen.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<LeadBackfillResult> RunAsync(bool dryRun, CancellationToken ct = default);
}

/// <summary>
/// Outcome of a <see cref="ILeadBackfillService.RunAsync"/> run.
/// </summary>
public class LeadBackfillResult
{
    /// <summary>True if this run did not write to the database.</summary>
    public bool DryRun { get; set; }

    /// <summary>Total number of Lead-type Contacts examined (migrated + not yet migrated).</summary>
    public int TotalLeadContactsFound { get; set; }

    /// <summary>Number of Leads created (or, in dry-run mode, that would be created).</summary>
    public int CreatedCount { get; set; }

    /// <summary>Number of Contacts skipped because a Lead already exists for them (ContactId match).</summary>
    public int SkippedAlreadyMigratedCount { get; set; }

    /// <summary>
    /// Number of Contacts whose Notes field could not be parsed as the expected
    /// <c>{ source, status, notes }</c> JSON shape. These Contacts are NOT skipped —
    /// a Lead is still created for them with best-effort identity fields only, using
    /// default Source/Status. Their IDs are reported here for human review.
    /// </summary>
    public int ParseErrorCount { get; set; }

    /// <summary>Contact IDs whose Notes JSON failed to parse (see <see cref="ParseErrorCount"/>).</summary>
    public List<int> ParseErrorContactIds { get; set; } = new();

    /// <summary>
    /// Raw source/status string values encountered that did not map to a known
    /// Lead.Source / Lead.Status enum member. Reported (not thrown) so a human can
    /// extend the mapping; the affected Lead is still created using the default value.
    /// Keyed by ContactId, value is "field=rawValue" (e.g. "source=social").
    /// </summary>
    public List<string> UnmappedEnumValues { get; set; } = new();

    /// <summary>Ids of Leads created by this run (empty when DryRun is true).</summary>
    public List<int> CreatedLeadIds { get; set; } = new();
}
