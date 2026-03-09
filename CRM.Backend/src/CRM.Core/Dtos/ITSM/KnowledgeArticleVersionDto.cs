// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for knowledge article version history entries.
/// </summary>
/// <remarks>
/// KB-016: Orphan DTO paired with the unmapped <see cref="KnowledgeArticleVersion"/> entity.
/// Neither is referenced outside their own definition files.
/// Use <c>ArticleVersionDto</c> for all API contract needs.
/// </remarks>
[Obsolete("KB-016: Use ArticleVersionDto instead. This DTO mirrors the obsolete KnowledgeArticleVersion entity.", error: false)] // KB-016: orphan DTO — never used in any service or controller
public class KnowledgeArticleVersionDto
{
    public int VersionId { get; set; }
    public int ArticleId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string ArticleBody { get; set; } = string.Empty;
    public ArticleType ArticleType { get; set; }
    public string? ChangesSummary { get; set; }
    public int CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}
