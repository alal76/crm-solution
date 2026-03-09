// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities.KnowledgeBase;

namespace CRM.Core.Dtos.KnowledgeBase;

// ===========================================================================
// Read DTO
// ===========================================================================

/// <summary>
/// Full article DTO returned by the Knowledge Base API.
/// </summary>
public class KnowledgeBaseArticleDto
{
    public int Id { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentFormat { get; set; } = "html";

    /// <summary>Numeric value of <see cref="ArticleType"/> enum.</summary>
    public int ArticleType { get; set; }

    /// <summary>Numeric value of <see cref="ArticleStatus"/> enum.</summary>
    public int Status { get; set; }

    /// <summary>Numeric value of <see cref="ArticleVisibility"/> enum.</summary>
    public int Visibility { get; set; }

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public string? Tags { get; set; }
    public string? Keywords { get; set; }

    public int AuthorUserId { get; set; }
    public string? AuthorName { get; set; }

    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int CaseDeflectionCount { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReviewDate { get; set; }
    public int Version { get; set; } = 1;
    public string LanguageCode { get; set; } = "en";

    public string? ProductsJson { get; set; }
    public string? RelatedArticleIdsJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ===========================================================================
// Create DTO
// ===========================================================================

/// <summary>
/// DTO for creating a new knowledge base article (POST).
/// </summary>
public class CreateKnowledgeBaseArticleDto
{
    [Required]
    [StringLength(300, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    /// <summary>Optional URL slug. Auto-generated from title if omitted.</summary>
    public string? Slug { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public string ContentFormat { get; set; } = "html";

    public ArticleType ArticleType { get; set; } = ArticleType.HowTo;

    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    public ArticleVisibility Visibility { get; set; } = ArticleVisibility.Internal;

    public int? CategoryId { get; set; }

    public string? Tags { get; set; }
    public string? Keywords { get; set; }

    public bool IsFeatured { get; set; } = false;

    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReviewDate { get; set; }

    public string LanguageCode { get; set; } = "en";

    public string? ProductsJson { get; set; }
}

// ===========================================================================
// Update DTO
// ===========================================================================

/// <summary>
/// DTO for updating an existing knowledge base article (PUT/PATCH).
/// All fields are optional — only non-null values are applied.
/// </summary>
public class UpdateKnowledgeBaseArticleDto
{
    [StringLength(300, MinimumLength = 3)]
    public string? Title { get; set; }

    public string? Summary { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? ContentFormat { get; set; }

    public ArticleType? ArticleType { get; set; }
    public ArticleStatus? Status { get; set; }
    public ArticleVisibility? Visibility { get; set; }

    public int? CategoryId { get; set; }
    public string? Tags { get; set; }
    public string? Keywords { get; set; }
    public bool? IsFeatured { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? ProductsJson { get; set; }
}

// ===========================================================================
// Feedback DTO
// ===========================================================================

/// <summary>
/// DTO for submitting user feedback on a knowledge base article.
/// </summary>
public class KnowledgeBaseFeedbackDto
{
    public int? UserId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public bool? IsHelpful { get; set; }
}

// ===========================================================================
// Category DTOs
// ===========================================================================

/// <summary>
/// DTO for a knowledge base category.
/// </summary>
public class KnowledgeCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ParentId { get; set; }
    public int ArticleCount { get; set; }
}

/// <summary>
/// DTO for creating a knowledge base category.
/// </summary>
public class CreateKnowledgeCategoryDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? ParentId { get; set; }
}

/// <summary>
/// DTO for updating a knowledge base category.
/// </summary>
public class UpdateKnowledgeCategoryDto
{
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? Name { get; set; }

    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public int? ParentId { get; set; }
}
