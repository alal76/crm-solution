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
/// Represents a version history entry for a knowledge article.
/// Every time an article is updated, a version snapshot is created.
/// </summary>
public class KnowledgeArticleVersion
{
    [Key]
    public int VersionId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public KnowledgeArticle? Article { get; set; }

    /// <summary>
    /// The version number (incrementing from 1).
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Title at time of version snapshot.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Short description at time of version snapshot.
    /// </summary>
    [StringLength(500)]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// Full article body at time of version snapshot.
    /// </summary>
    [Required]
    public string ArticleBody { get; set; } = string.Empty;

    /// <summary>
    /// Article type at time of version snapshot.
    /// </summary>
    public ArticleType ArticleType { get; set; }

    /// <summary>
    /// Summary of changes made in this version.
    /// </summary>
    [StringLength(500)]
    public string? ChangesSummary { get; set; }

    /// <summary>
    /// ID of user who created this version.
    /// </summary>
    [Required]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this version entry has been deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
