// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;

namespace CRM.Core.DTOs.ITSM;

/// <summary>
/// DTO for knowledge article version history entries.
/// </summary>
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
