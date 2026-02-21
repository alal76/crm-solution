// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;
namespace CRM.Core.Entities.KnowledgeBase;

#region Knowledge Article Enumerations

/// <summary>
/// Knowledge article type.
/// </summary>
public enum ArticleType
{
    /// <summary>How-to guide</summary>
    HowTo = 0,

    /// <summary>FAQ</summary>
    FAQ = 1,

    /// <summary>Troubleshooting guide</summary>
    Troubleshooting = 2,

    /// <summary>Best practice</summary>
    BestPractice = 3,

    /// <summary>Product documentation</summary>
    Documentation = 4,

    /// <summary>Process/procedure</summary>
    Process = 5,

    /// <summary>Policy</summary>
    Policy = 6,

    /// <summary>Release notes</summary>
    ReleaseNotes = 7,

    /// <summary>Video tutorial</summary>
    Video = 8,

    /// <summary>Template</summary>
    Template = 9
}

/// <summary>
/// Article status.
/// </summary>
public enum ArticleStatus
{
    /// <summary>Draft - being written</summary>
    Draft = 0,

    /// <summary>In review</summary>
    InReview = 1,

    /// <summary>Published and active</summary>
    Published = 2,

    /// <summary>Needs update</summary>
    NeedsUpdate = 3,

    /// <summary>Archived</summary>
    Archived = 4,

    /// <summary>Deprecated</summary>
    Deprecated = 5
}

/// <summary>
/// Article visibility.
/// </summary>
public enum ArticleVisibility
{
    /// <summary>Internal only - agents and admins</summary>
    Internal = 0,

    /// <summary>Customer portal - authenticated customers</summary>
    CustomerPortal = 1,

    /// <summary>Public - anyone can access</summary>
    Public = 2
}

#endregion

/// <summary>
/// Knowledge base article for self-service and agent assistance.
/// </summary>
public class KnowledgeArticle : BaseEntity
{
    #region Identification

    /// <summary>Article number (unique identifier)</summary>
    public string ArticleNumber { get; set; } = $"KB{DateTime.UtcNow:yyyyMMddHHmmss}";

    /// <summary>Article title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Article summary</summary>
    public string? Summary { get; set; }

    /// <summary>URL-friendly slug</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Article type</summary>
    public ArticleType ArticleType { get; set; }

    #endregion

    #region Content

    /// <summary>Article body content (HTML/Markdown)</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Content format (html, markdown)</summary>
    public string ContentFormat { get; set; } = "html";

    /// <summary>Plain text version for search</summary>
    public string? PlainTextContent { get; set; }

    /// <summary>Table of contents (JSON)</summary>
    public string? TableOfContentsJson { get; set; }

    /// <summary>Attachments (JSON array)</summary>
    public string? AttachmentsJson { get; set; }

    /// <summary>Video URL if applicable</summary>
    public string? VideoUrl { get; set; }

    #endregion

    #region Publishing

    /// <summary>Article status</summary>
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    /// <summary>Visibility</summary>
    public ArticleVisibility Visibility { get; set; } = ArticleVisibility.Internal;

    /// <summary>Published date</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>Expires at</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Review date</summary>
    public DateTime? ReviewDate { get; set; }

    /// <summary>Version number</summary>
    public int Version { get; set; } = 1;

    #endregion

    #region Categorization

    /// <summary>Category ID</summary>
    public int? CategoryId { get; set; }

    /// <summary>Navigation to category</summary>
    public KnowledgeCategory? Category { get; set; }

    /// <summary>Products this applies to (JSON array)</summary>
    public string? ProductsJson { get; set; }

    /// <summary>Tags (JSON array)</summary>
    public string? TagsJson { get; set; }

    /// <summary>Keywords for search</summary>
    public string? Keywords { get; set; }

    #endregion

    #region SEO

    /// <summary>Meta title for SEO</summary>
    public string? MetaTitle { get; set; }

    /// <summary>Meta description for SEO</summary>
    public string? MetaDescription { get; set; }

    /// <summary>Canonical URL</summary>
    public string? CanonicalUrl { get; set; }

    #endregion

    #region Authorship

    /// <summary>Author user ID</summary>
    public int AuthorUserId { get; set; }

    /// <summary>Navigation to author</summary>
    public User? Author { get; set; }

    /// <summary>Last updated by user ID</summary>
    public int? LastUpdatedByUserId { get; set; }

    /// <summary>Navigation to last updater</summary>
    public User? LastUpdatedByUser { get; set; }

    /// <summary>Approver user ID</summary>
    public int? ApprovedByUserId { get; set; }

    /// <summary>Navigation to approver</summary>
    public User? ApprovedByUser { get; set; }

    #endregion

    #region Metrics

    /// <summary>View count</summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>Unique visitor count</summary>
    public int UniqueVisitorCount { get; set; } = 0;

    /// <summary>Helpful votes</summary>
    public int HelpfulCount { get; set; } = 0;

    /// <summary>Not helpful votes</summary>
    public int NotHelpfulCount { get; set; } = 0;

    /// <summary>Helpfulness score</summary>
    public decimal HelpfulnessScore => HelpfulCount + NotHelpfulCount > 0
        ? (decimal)HelpfulCount / (HelpfulCount + NotHelpfulCount) * 100
        : 0;

    /// <summary>Average rating (1-5)</summary>
    public decimal? AverageRating { get; set; }

    /// <summary>Rating count</summary>
    public int RatingCount { get; set; } = 0;

    /// <summary>Case deflection count</summary>
    public int CaseDeflectionCount { get; set; } = 0;

    /// <summary>Search impression count</summary>
    public int SearchImpressionCount { get; set; } = 0;

    /// <summary>Search click count</summary>
    public int SearchClickCount { get; set; } = 0;

    /// <summary>Average time on page (seconds)</summary>
    public decimal? AvgTimeOnPageSeconds { get; set; }

    #endregion

    #region Localization

    /// <summary>Language code</summary>
    public string LanguageCode { get; set; } = "en";

    /// <summary>Parent article ID (for translations)</summary>
    public int? ParentArticleId { get; set; }

    /// <summary>Navigation to parent article</summary>
    public KnowledgeArticle? ParentArticle { get; set; }

    /// <summary>Translations</summary>
    public ICollection<KnowledgeArticle> Translations { get; set; } = new List<KnowledgeArticle>();

    #endregion

    #region AI Features

    /// <summary>AI-generated embedding vector (JSON)</summary>
    public string? EmbeddingVectorJson { get; set; }

    /// <summary>AI-generated summary</summary>
    public string? AISummary { get; set; }

    /// <summary>Related articles (JSON array of IDs)</summary>
    public string? RelatedArticleIdsJson { get; set; }

    /// <summary>Suggested improvements from AI</summary>
    public string? AISuggestionsJson { get; set; }

    #endregion

    #region Relationships

    /// <summary>Linked service requests</summary>
    public ICollection<ServiceRequestArticle> ServiceRequests { get; set; } = new List<ServiceRequestArticle>();

    /// <summary>Article feedback</summary>
    public ICollection<ArticleFeedback> Feedback { get; set; } = new List<ArticleFeedback>();

    #endregion
}

/// <summary>
/// Knowledge base category.
/// </summary>
public class KnowledgeCategory : BaseEntity
{
    /// <summary>Category name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Category description</summary>
    public string? Description { get; set; }

    /// <summary>URL slug</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Category icon</summary>
    public string? Icon { get; set; }

    /// <summary>Display order</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Parent category ID</summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>Navigation to parent</summary>
    public KnowledgeCategory? ParentCategory { get; set; }

    /// <summary>Child categories</summary>
    public ICollection<KnowledgeCategory> ChildCategories { get; set; } = new List<KnowledgeCategory>();

    /// <summary>Articles in this category</summary>
    public ICollection<KnowledgeArticle> Articles { get; set; } = new List<KnowledgeArticle>();
}

/// <summary>
/// Link between service request and knowledge article.
/// </summary>
public class ServiceRequestArticle : BaseEntity
{
    /// <summary>Service request ID</summary>
    public int ServiceRequestId { get; set; }

    /// <summary>Navigation to service request</summary>
    public ServiceRequest? ServiceRequest { get; set; }

    /// <summary>Article ID</summary>
    public int KnowledgeArticleId { get; set; }

    /// <summary>Navigation to article</summary>
    public KnowledgeArticle? KnowledgeArticle { get; set; }

    /// <summary>Was article helpful for case</summary>
    public bool? WasHelpful { get; set; }

    /// <summary>Did article deflect case</summary>
    public bool DeflectedCase { get; set; } = false;

    /// <summary>When linked</summary>
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Linked by user ID</summary>
    public int? LinkedByUserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? LinkedByUser { get; set; }
}

/// <summary>
/// User feedback on knowledge article.
/// </summary>
public class ArticleFeedback : BaseEntity
{
    /// <summary>Article ID</summary>
    public int KnowledgeArticleId { get; set; }

    /// <summary>Navigation to article</summary>
    public KnowledgeArticle? KnowledgeArticle { get; set; }

    /// <summary>Was helpful</summary>
    public bool IsHelpful { get; set; }

    /// <summary>Rating (1-5)</summary>
    public int? Rating { get; set; }

    /// <summary>Feedback comment</summary>
    public string? Comment { get; set; }

    /// <summary>User ID (null if anonymous)</summary>
    public int? UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Account ID (for portal)</summary>
    [Column("AccountId")]
    public int? AccountId { get; set; }

    /// <summary>Navigation to account</summary>
    public Account? Account { get; set; }

    /// <summary>Session ID for anonymous</summary>
    public string? SessionId { get; set; }

    /// <summary>When submitted</summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
