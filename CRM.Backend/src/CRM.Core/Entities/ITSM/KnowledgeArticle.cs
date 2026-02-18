// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

public enum ArticleType
{
    HowTo = 1,
    Troubleshooting = 2,
    FAQ = 3,
    KnownError = 4,
    Reference = 5,
    BestPractice = 6
}

public enum PublishingState
{
    Draft = 1,
    Review = 2,
    Approved = 3,
    Published = 4,
    Retired = 5
}

public class KnowledgeArticle
{
    [Key]
    public int ArticleId { get; set; }

    [Required]
    [StringLength(20)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    [Required]
    public string ArticleBody { get; set; } = string.Empty;

    [Required]
    public ArticleType ArticleType { get; set; }

    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ServiceRequestCategory? Category { get; set; }

    public int? SubcategoryId { get; set; }

    [ForeignKey(nameof(SubcategoryId))]
    public ServiceRequestSubcategory? Subcategory { get; set; }

    // Publishing
    [Required]
    public int AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }

    [Required]
    public int OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    [Required]
    public PublishingState PublishingState { get; set; } = PublishingState.Draft;

    public DateTime? PublishedDate { get; set; }

    public int? PublishedById { get; set; }

    [ForeignKey(nameof(PublishedById))]
    public User? PublishedBy { get; set; }

    public DateTime? ReviewDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int Version { get; set; } = 1;

    // Audience
    public bool IsInternal { get; set; } = true;

    public bool IsExternal { get; set; } = false;

    public bool IsPublic { get; set; } = false;

    // Metadata
    public string? Tags { get; set; }

    // Metrics
    public int ViewCount { get; set; } = 0;

    public int HelpfulCount { get; set; } = 0;

    public int NotHelpfulCount { get; set; } = 0;

    public int AttachedToIncidentCount { get; set; } = 0;

    public DateTime? LastViewedAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedById { get; set; }

    [ForeignKey(nameof(ModifiedById))]
    public User? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<ArticleRelationship>? RelatedArticles { get; set; }

    public ICollection<ArticleIncident>? Incidents { get; set; }

    public ICollection<ArticleFeedback>? Feedback { get; set; }

    public ICollection<ArticleAttachment>? Attachments { get; set; }
}

public class ArticleRelationship
{
    [Key]
    public int RelationshipId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public KnowledgeArticle? Article { get; set; }

    [Required]
    public int RelatedArticleId { get; set; }

    [ForeignKey(nameof(RelatedArticleId))]
    public KnowledgeArticle? RelatedArticle { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ArticleIncident
{
    [Key]
    public int ArticleIncidentId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public KnowledgeArticle? Article { get; set; }

    [Required]
    public int IncidentId { get; set; }

    [ForeignKey(nameof(IncidentId))]
    public Incident? Incident { get; set; }

    public bool UsedToResolve { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
}

public class ArticleFeedback
{
    [Key]
    public int FeedbackId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public KnowledgeArticle? Article { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public bool IsHelpful { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ArticleAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public KnowledgeArticle? Article { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [Required]
    public int UploadedById { get; set; }

    [ForeignKey(nameof(UploadedById))]
    public User? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
