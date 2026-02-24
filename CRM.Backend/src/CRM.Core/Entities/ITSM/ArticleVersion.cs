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
