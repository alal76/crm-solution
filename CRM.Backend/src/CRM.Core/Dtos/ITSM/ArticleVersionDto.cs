// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for article version history display.
/// </summary>
public class ArticleVersionDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public int ChangedById { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangeNote { get; set; }
}

/// <summary>
/// DTO for restoring an article to a previous version.
/// </summary>
public class RestoreVersionDto
{
    public int VersionId { get; set; }
    public string? RestoreNote { get; set; }
}
