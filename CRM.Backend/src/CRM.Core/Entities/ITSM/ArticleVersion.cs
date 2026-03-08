// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Tracks version history of knowledge articles.
/// Each edit creates a new version for audit and rollback purposes.
/// </summary>
/// <remarks>
/// KB-016: This is the single authoritative entity for article version history.
/// It is DB-mapped to the <c>ArticleVersions</c> table and used throughout
/// <c>KnowledgeManagementService</c> and the API controllers.
/// <para>
/// A near-duplicate entity <c>KnowledgeArticleVersion</c> (same namespace/file) was found
/// but is an unmapped orphan — never referenced outside its own definition file.
/// It has been marked <c>[Obsolete]</c>; do not use it.
/// </para>
/// <para>
/// Field-name mapping between the two (for historical reference):
/// <list type="table">
/// <item><term>Content</term><description>Equivalent to KnowledgeArticleVersion.ArticleBody</description></item>
/// <item><term>ChangedById / ChangedAt</term><description>Equivalent to CreatedById / CreatedAt</description></item>
/// <item><term>ChangeNote</term><description>Equivalent to KnowledgeArticleVersion.ChangesSummary</description></item>
/// </list>
/// </para>
/// <para>
/// NOTE — ArticleType gap (KB-016): <c>KnowledgeArticleVersion</c> snapshots the
/// <c>ArticleType</c> at the time of each version. This <c>ArticleVersion</c> entity does
/// not store that snapshot; callers should read <c>KnowledgeArticle.ArticleType</c> for
/// the current type. A future migration can add an <c>ArticleType</c> column here if
/// point-in-time type tracking is required.
/// </para>
/// </remarks>
public class ArticleVersion
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the knowledge article this version belongs to.
    /// </summary>
    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public KnowledgeArticle? Article { get; set; }

    /// <summary>
    /// Sequential version number for this article (1, 2, 3, ...).
    /// </summary>
    [Required]
    public int VersionNumber { get; set; }

    /// <summary>
    /// The article title at this version.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The article body/content at this version.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Short description/summary at this version.
    /// </summary>
    [StringLength(500)]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// ID of the user who made this change.
    /// </summary>
    [Required]
    public int ChangedById { get; set; }

    [ForeignKey(nameof(ChangedById))]
    public User? ChangedBy { get; set; }

    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    [Required]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional note/reason for this version change.
    /// </summary>
    [StringLength(500)]
    public string? ChangeNote { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
