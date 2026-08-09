// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities.Reports;

/// <summary>
/// A reusable, pre-built report configuration published to the Report Templates
/// Marketplace (REV-FE-003). Users can preview a template and "apply" it, which
/// hands the stored <see cref="ReportConfigJson"/> to the report designer as a
/// starting point for a new report.
/// </summary>
public class ReportTemplate : BaseEntity
{
    /// <summary>Template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Template description shown in the marketplace card and preview dialog.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Marketplace category (Sales, Marketing, Customer Success, Finance, Service Desk, ...).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional author user ID for templates published by an actual CRM user.
    /// Nullable because most marketplace templates are published by a team/system
    /// (e.g. "CRM Solution Team", "Sales Ops") rather than an individual user account.
    /// </summary>
    public int? AuthorUserId { get; set; }

    /// <summary>Navigation to the author user, when the template is attributed to a real user.</summary>
    public User? AuthorUser { get; set; }

    /// <summary>
    /// Free-text author/publisher display name, used when there is no backing
    /// <see cref="AuthorUser"/> (e.g. team names like "Marketing Ops", "CS Analytics").
    /// Always populated; falls back to this value when <see cref="AuthorUserId"/> is null.
    /// </summary>
    public string AuthorDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Average rating (0-5). Stored as a simple aggregate for v1 scope — there is no
    /// per-user rating/review sub-entity yet.
    /// </summary>
    public decimal Rating { get; set; }

    /// <summary>Number of times this template has been applied ("downloaded").</summary>
    public int Downloads { get; set; }

    /// <summary>Tags for search/filtering (JSON array of strings) — mirrors ReportDefinition.TagsJson.</summary>
    public string? TagsJson { get; set; }

    /// <summary>Optional preview image URL shown on the marketplace card.</summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// The saved report configuration (JSON), structurally compatible with the
    /// frontend's ReportConfig shape (see components/analytics/ReportDesigner.tsx).
    /// Handed back to the frontend as-is when a template is applied.
    /// </summary>
    public string ReportConfigJson { get; set; } = "{}";
}
