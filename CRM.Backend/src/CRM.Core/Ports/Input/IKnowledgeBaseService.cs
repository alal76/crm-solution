// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Dtos.KnowledgeBase;

namespace CRM.Core.Ports.Input;

/// <summary>
/// Input port for the general Knowledge Base module.
/// Manages article CRUD, publishing workflow, categories, feedback, and case deflection.
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>Get a paginated, filtered list of articles.</summary>
    Task<PagedResultDto<KnowledgeBaseArticleDto>> GetAllAsync(
        int page, int pageSize, string? search, int? categoryId, string? status,
        CancellationToken ct = default);

    /// <summary>Get a single article by its database ID.</summary>
    Task<KnowledgeBaseArticleDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Get a single article by its URL slug.</summary>
    Task<KnowledgeBaseArticleDto?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Create a new article in Draft status.</summary>
    Task<KnowledgeBaseArticleDto> CreateAsync(
        CreateKnowledgeBaseArticleDto dto, int authorId, CancellationToken ct = default);

    /// <summary>Update an existing article's content or metadata.</summary>
    Task<KnowledgeBaseArticleDto> UpdateAsync(
        int id, UpdateKnowledgeBaseArticleDto dto, CancellationToken ct = default);

    /// <summary>Soft-delete an article.</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Transition article to Published status.</summary>
    Task<KnowledgeBaseArticleDto> PublishAsync(int id, CancellationToken ct = default);

    /// <summary>Transition article to Archived status.</summary>
    Task<KnowledgeBaseArticleDto> ArchiveAsync(int id, CancellationToken ct = default);

    /// <summary>Submit user feedback (rating / helpful vote) for an article.</summary>
    Task SubmitFeedbackAsync(int id, KnowledgeBaseFeedbackDto feedback, CancellationToken ct = default);

    /// <summary>Get all active categories with article counts.</summary>
    Task<IEnumerable<KnowledgeCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>Create a new category.</summary>
    Task<KnowledgeCategoryDto> CreateCategoryAsync(CreateKnowledgeCategoryDto dto, CancellationToken ct = default);

    /// <summary>Update an existing category.</summary>
    Task<KnowledgeCategoryDto> UpdateCategoryAsync(int id, UpdateKnowledgeCategoryDto dto, CancellationToken ct = default);

    /// <summary>Soft-delete a category.</summary>
    Task DeleteCategoryAsync(int id, CancellationToken ct = default);

    /// <summary>Get the most-viewed published articles.</summary>
    Task<IEnumerable<KnowledgeBaseArticleDto>> GetPopularAsync(int count = 10, CancellationToken ct = default);

    /// <summary>Get the most recently published articles.</summary>
    Task<IEnumerable<KnowledgeBaseArticleDto>> GetRecentAsync(int count = 10, CancellationToken ct = default);

    /// <summary>Get articles tagged with a specific product ID.</summary>
    Task<IEnumerable<KnowledgeBaseArticleDto>> GetByProductAsync(int productId, CancellationToken ct = default);

    /// <summary>Record a case deflection event — article was viewed instead of creating a service request.</summary>
    Task TrackCaseDeflectionAsync(int articleId, int? serviceRequestId, CancellationToken ct = default);
}
