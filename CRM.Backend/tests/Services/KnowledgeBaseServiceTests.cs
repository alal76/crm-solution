// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.KnowledgeBase;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="KnowledgeBaseService"/>.
/// Tests CRUD operations, publishing state machine, feedback, categories, and slug generation.
/// </summary>
public class KnowledgeBaseServiceTests : ServiceTestFixtureBase<KnowledgeBaseService>
{
    private readonly KnowledgeBaseService _service;

    public KnowledgeBaseServiceTests()
    {
        _service = new KnowledgeBaseService(MockContext.Object, MockLogger.Object);
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static KnowledgeArticle MakeArticle(
        int id = 1,
        string title = "Test Article",
        ArticleStatus status = ArticleStatus.Draft,
        bool isDeleted = false,
        int? categoryId = null,
        string? slug = null,
        int version = 1,
        int helpfulCount = 0,
        int notHelpfulCount = 0,
        decimal? averageRating = null,
        int ratingCount = 0,
        int viewCount = 0,
        DateTime? publishedAt = null,
        int caseDeflectionCount = 0) => new()
    {
        Id = id,
        ArticleNumber = $"KB{id:D7}",
        Title = title,
        Slug = slug ?? title.ToLowerInvariant().Replace(" ", "-"),
        Content = "Content",
        Status = status,
        Visibility = ArticleVisibility.Internal,
        ArticleType = ArticleType.HowTo,
        CreatedAt = DateTime.UtcNow,
        IsDeleted = isDeleted,
        CategoryId = categoryId,
        Version = version,
        HelpfulCount = helpfulCount,
        NotHelpfulCount = notHelpfulCount,
        AverageRating = averageRating,
        RatingCount = ratingCount,
        ViewCount = viewCount,
        PublishedAt = publishedAt,
        CaseDeflectionCount = caseDeflectionCount
    };

    private static KnowledgeCategory MakeCategory(int id = 1, string name = "General", bool isActive = true) => new()
    {
        Id = id,
        Name = name,
        Slug = name.ToLowerInvariant(),
        IsActive = isActive,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow
    };

    private void SetupDbSets(
        List<KnowledgeArticle>? articles = null,
        List<KnowledgeCategory>? categories = null,
        List<ArticleFeedback>? feedbacks = null,
        List<ServiceRequestArticle>? srArticles = null)
    {
        articles ??= new List<KnowledgeArticle>();
        categories ??= new List<KnowledgeCategory>();
        feedbacks ??= new List<ArticleFeedback>();
        srArticles ??= new List<ServiceRequestArticle>();

        var mockArticles = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.KnowledgeArticles).Returns(mockArticles.Object);

        var mockCategories = MockDbSetFactory.CreateMockDbSet(categories);
        MockContext.Setup(c => c.KnowledgeCategories).Returns(mockCategories.Object);

        var mockFeedbacks = MockDbSetFactory.CreateMockDbSet(feedbacks);
        MockContext.Setup(c => c.ArticleFeedbacks).Returns(mockFeedbacks.Object);

        var mockSrArticles = MockDbSetFactory.CreateMockDbSet(srArticles);
        MockContext.Setup(c => c.ServiceRequestArticles).Returns(mockSrArticles.Object);

        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResult_WhenArticlesExist()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "Article A"),
            MakeArticle(2, "Article B"),
            MakeArticle(3, "Article C")
        };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetAllAsync(1, 20, null, null, null, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeleted_WhenArticlesDeleted()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "Visible"),
            MakeArticle(2, "Deleted", isDeleted: true)
        };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetAllAsync(1, 20, null, null, null, CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Visible");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus_WhenStatusProvided()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "Draft", status: ArticleStatus.Draft),
            MakeArticle(2, "Published", status: ArticleStatus.Published)
        };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetAllAsync(1, 20, null, null, "Published", CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Published");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCategoryId_WhenCategoryProvided()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "Cat1", categoryId: 10),
            MakeArticle(2, "Cat2", categoryId: 20),
        };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetAllAsync(1, 20, null, 10, null, CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Cat1");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterBySearch_WhenTermProvided()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "How to reset password"),
            MakeArticle(2, "Sales tips")
        };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetAllAsync(1, 20, "reset", null, null, CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Contain("reset");
    }

    // =========================================================================
    // GetByIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenArticleExists()
    {
        // Arrange
        var articles = new List<KnowledgeArticle> { MakeArticle(1, "Existing Article") };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Existing Article");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenArticleNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());

        // Act
        var result = await _service.GetByIdAsync(999, CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenArticleDeleted()
    {
        // Arrange
        var articles = new List<KnowledgeArticle> { MakeArticle(1, "Deleted", isDeleted: true) };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncrementViewCount_WhenArticleFound()
    {
        // Arrange
        var article = MakeArticle(1);
        article.ViewCount = 5;
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        await _service.GetByIdAsync(1, CancellationToken);

        // Assert
        article.ViewCount.Should().Be(6);
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================================
    // GetBySlugAsync
    // =========================================================================

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnDto_WhenSlugExists()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, slug: "my-slug")
        };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.GetBySlugAsync("my-slug", CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("my-slug");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());

        // Act
        var result = await _service.GetBySlugAsync("no-such-slug", CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_ShouldAddArticle_AndReturnDto()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        SetupDbSets(articles: articles);

        var dto = new CreateKnowledgeBaseArticleDto
        {
            Title = "New Article",
            Content = "Some content",
            ArticleType = ArticleType.HowTo,
            Status = ArticleStatus.Draft,
            Visibility = ArticleVisibility.Internal
        };

        // Act
        var result = await _service.CreateAsync(dto, authorId: 1, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Article");
        articles.Should().HaveCount(1);
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateSlug_WhenSlugNotProvided()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        SetupDbSets(articles: articles);

        var dto = new CreateKnowledgeBaseArticleDto
        {
            Title = "How To Reset Password",
            Content = "Steps",
            ArticleType = ArticleType.HowTo,
            Status = ArticleStatus.Draft,
            Visibility = ArticleVisibility.Internal
        };

        // Act
        var result = await _service.CreateAsync(dto, authorId: 1, CancellationToken);

        // Assert
        result.Slug.Should().Be("how-to-reset-password");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateUniqueSlug_WhenSlugCollisionExists()
    {
        // Arrange
        var existing = MakeArticle(1, "Test", slug: "test-article");
        var articles = new List<KnowledgeArticle> { existing };
        SetupDbSets(articles: articles);

        var dto = new CreateKnowledgeBaseArticleDto
        {
            Title = "Test Article",
            Content = "Content",
            ArticleType = ArticleType.FAQ,
            Status = ArticleStatus.Draft,
            Visibility = ArticleVisibility.Internal
        };

        // Act
        var result = await _service.CreateAsync(dto, authorId: 1, CancellationToken);

        // Assert
        result.Slug.Should().Be("test-article-2");
    }

    // =========================================================================
    // UpdateAsync
    // =========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenArticleExists()
    {
        // Arrange
        var article = MakeArticle(1, "Original Title");
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        var dto = new UpdateKnowledgeBaseArticleDto { Title = "Updated Title" };

        // Act
        var result = await _service.UpdateAsync(1, dto, CancellationToken);

        // Assert
        result.Title.Should().Be("Updated Title");
        article.Title.Should().Be("Updated Title");
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFound_WhenArticleNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());
        var dto = new UpdateKnowledgeBaseArticleDto { Title = "New" };

        // Act
        var act = () => _service.UpdateAsync(999, dto, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldIncrementVersion_WhenArticleUpdated()
    {
        // Arrange
        var article = MakeArticle(1, version: 3);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        var dto = new UpdateKnowledgeBaseArticleDto { Title = "New title" };

        // Act
        await _service.UpdateAsync(1, dto, CancellationToken);

        // Assert
        article.Version.Should().Be(4);
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_ShouldSetIsDeleted_WhenArticleExists()
    {
        // Arrange
        var article = MakeArticle(1);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        await _service.DeleteAsync(1, CancellationToken);

        // Assert
        article.IsDeleted.Should().BeTrue();
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFound_WhenArticleNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());

        // Act
        var act = () => _service.DeleteAsync(999, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // PublishAsync — state machine
    // =========================================================================

    [Fact]
    public async Task PublishAsync_ShouldSetPublishedStatus_WhenArticleIsDraft()
    {
        // Arrange
        var article = MakeArticle(1, status: ArticleStatus.Draft);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.PublishAsync(1, CancellationToken);

        // Assert
        result.Status.Should().Be((int)ArticleStatus.Published);
        article.Status.Should().Be(ArticleStatus.Published);
        article.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishAsync_ShouldSetPublishedStatus_WhenArticleIsInReview()
    {
        // Arrange
        var article = MakeArticle(1, status: ArticleStatus.InReview);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.PublishAsync(1, CancellationToken);

        // Assert
        result.Status.Should().Be((int)ArticleStatus.Published);
    }

    [Fact]
    public async Task PublishAsync_ShouldThrowInvalidOperation_WhenArticleIsArchived()
    {
        // Arrange
        var article = MakeArticle(1, status: ArticleStatus.Archived);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        var act = () => _service.PublishAsync(1, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*archived*");
    }

    [Fact]
    public async Task PublishAsync_ShouldThrowKeyNotFound_WhenArticleNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());

        // Act
        var act = () => _service.PublishAsync(999, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // ArchiveAsync
    // =========================================================================

    [Fact]
    public async Task ArchiveAsync_ShouldSetArchivedStatus_WhenArticleIsPublished()
    {
        // Arrange
        var article = MakeArticle(1, status: ArticleStatus.Published);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        var result = await _service.ArchiveAsync(1, CancellationToken);

        // Assert
        result.Status.Should().Be((int)ArticleStatus.Archived);
        article.Status.Should().Be(ArticleStatus.Archived);
    }

    [Fact]
    public async Task ArchiveAsync_ShouldThrowKeyNotFound_WhenArticleNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());

        // Act
        var act = () => _service.ArchiveAsync(999, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // SubmitFeedbackAsync
    // =========================================================================

    [Fact]
    public async Task SubmitFeedbackAsync_ShouldIncrementHelpfulCount_WhenIsHelpfulTrue()
    {
        // Arrange
        var article = MakeArticle(1, helpfulCount: 2);
        var articles = new List<KnowledgeArticle> { article };
        var feedbacks = new List<ArticleFeedback>();
        SetupDbSets(articles: articles, feedbacks: feedbacks);

        var feedback = new KnowledgeBaseFeedbackDto { IsHelpful = true };

        // Act
        await _service.SubmitFeedbackAsync(1, feedback, CancellationToken);

        // Assert
        article.HelpfulCount.Should().Be(3);
        feedbacks.Should().HaveCount(1);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ShouldIncrementNotHelpfulCount_WhenIsHelpfulFalse()
    {
        // Arrange
        var article = MakeArticle(1, notHelpfulCount: 1);
        var articles = new List<KnowledgeArticle> { article };
        var feedbacks = new List<ArticleFeedback>();
        SetupDbSets(articles: articles, feedbacks: feedbacks);

        var feedback = new KnowledgeBaseFeedbackDto { IsHelpful = false };

        // Act
        await _service.SubmitFeedbackAsync(1, feedback, CancellationToken);

        // Assert
        article.NotHelpfulCount.Should().Be(2);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ShouldUpdateAverageRating_WhenRatingProvided()
    {
        // Arrange
        var article = MakeArticle(1, averageRating: 4.0m, ratingCount: 1);
        var articles = new List<KnowledgeArticle> { article };
        var feedbacks = new List<ArticleFeedback>();
        SetupDbSets(articles: articles, feedbacks: feedbacks);

        var feedback = new KnowledgeBaseFeedbackDto { IsHelpful = true, Rating = 2 };

        // Act
        await _service.SubmitFeedbackAsync(1, feedback, CancellationToken);

        // Assert
        // NewTotal = 4.0 * 1 + 2 = 6; NewCount = 2; NewAvg = 6/2 = 3.0
        article.RatingCount.Should().Be(2);
        article.AverageRating.Should().Be(3.0m);
    }

    // =========================================================================
    // GetCategoriesAsync
    // =========================================================================

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnActiveCategories()
    {
        // Arrange
        var categories = new List<KnowledgeCategory>
        {
            MakeCategory(1, "General"),
            MakeCategory(2, "HowTo", isActive: false)
        };
        SetupDbSets(categories: categories);

        // Act
        var result = (await _service.GetCategoriesAsync(CancellationToken)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("General");
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldIncludeArticleCount()
    {
        // Arrange
        var categories = new List<KnowledgeCategory> { MakeCategory(1, "General") };
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, categoryId: 1),
            MakeArticle(2, categoryId: 1),
        };
        SetupDbSets(articles: articles, categories: categories);

        // Act
        var result = (await _service.GetCategoriesAsync(CancellationToken)).ToList();

        // Assert
        result[0].ArticleCount.Should().Be(2);
    }

    // =========================================================================
    // GetPopularAsync / GetRecentAsync
    // =========================================================================

    [Fact]
    public async Task GetPopularAsync_ShouldReturnPublishedArticles_OrderedByViewCount()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "Low", status: ArticleStatus.Published, viewCount: 5),
            MakeArticle(2, "High", status: ArticleStatus.Published, viewCount: 100),
            MakeArticle(3, "Draft", status: ArticleStatus.Draft, viewCount: 200)
        };
        SetupDbSets(articles: articles);

        // Act
        var result = (await _service.GetPopularAsync(5, CancellationToken)).ToList();

        // Assert
        result.Should().HaveCount(2); // only Published
        result[0].Title.Should().Be("High"); // highest view count first
    }

    [Fact]
    public async Task GetRecentAsync_ShouldReturnPublishedArticles_OrderedByPublishedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var articles = new List<KnowledgeArticle>
        {
            MakeArticle(1, "Older", status: ArticleStatus.Published, publishedAt: now.AddDays(-5)),
            MakeArticle(2, "Newer", status: ArticleStatus.Published, publishedAt: now.AddDays(-1)),
            MakeArticle(3, "Draft", status: ArticleStatus.Draft, publishedAt: now)
        };
        SetupDbSets(articles: articles);

        // Act
        var result = (await _service.GetRecentAsync(5, CancellationToken)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Newer");
    }

    // =========================================================================
    // TrackCaseDeflectionAsync
    // =========================================================================

    [Fact]
    public async Task TrackCaseDeflectionAsync_ShouldIncrementDeflectionCount()
    {
        // Arrange
        var article = MakeArticle(1, caseDeflectionCount: 3);
        var articles = new List<KnowledgeArticle> { article };
        SetupDbSets(articles: articles);

        // Act
        await _service.TrackCaseDeflectionAsync(1, null, CancellationToken);

        // Assert
        article.CaseDeflectionCount.Should().Be(4);
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackCaseDeflectionAsync_ShouldCreateSrLink_WhenServiceRequestIdProvided()
    {
        // Arrange
        var article = MakeArticle(1);
        var articles = new List<KnowledgeArticle> { article };
        var srArticles = new List<ServiceRequestArticle>();
        SetupDbSets(articles: articles, srArticles: srArticles);

        // Act
        await _service.TrackCaseDeflectionAsync(1, serviceRequestId: 42, CancellationToken);

        // Assert
        srArticles.Should().HaveCount(1);
        srArticles[0].ServiceRequestId.Should().Be(42);
        srArticles[0].DeflectedCase.Should().BeTrue();
    }

    [Fact]
    public async Task TrackCaseDeflectionAsync_ShouldNotSave_WhenArticleNotFound()
    {
        // Arrange
        SetupDbSets(articles: new List<KnowledgeArticle>());

        // Act — should not throw, just silently return
        var act = () => _service.TrackCaseDeflectionAsync(999, null, CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // =========================================================================
    // CreateCategoryAsync
    // =========================================================================

    [Fact]
    public async Task CreateCategoryAsync_ShouldAddCategory_AndReturnDto()
    {
        // Arrange
        var categories = new List<KnowledgeCategory>();
        SetupDbSets(categories: categories);

        var dto = new CreateKnowledgeCategoryDto { Name = "New Category" };

        // Act
        var result = await _service.CreateCategoryAsync(dto, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Category");
        result.Slug.Should().Be("new-category");
        categories.Should().HaveCount(1);
    }

    // =========================================================================
    // UpdateCategoryAsync
    // =========================================================================

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateFields_WhenCategoryExists()
    {
        // Arrange
        var category = MakeCategory(1, "Old Name");
        var categories = new List<KnowledgeCategory> { category };
        SetupDbSets(categories: categories);

        var dto = new UpdateKnowledgeCategoryDto { Name = "New Name" };

        // Act
        var result = await _service.UpdateCategoryAsync(1, dto, CancellationToken);

        // Assert
        result.Name.Should().Be("New Name");
        category.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrowKeyNotFound_WhenCategoryNotFound()
    {
        // Arrange
        SetupDbSets(categories: new List<KnowledgeCategory>());
        var dto = new UpdateKnowledgeCategoryDto { Name = "X" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateCategoryAsync(999, dto, CancellationToken));
    }

    // =========================================================================
    // DeleteCategoryAsync
    // =========================================================================

    [Fact]
    public async Task DeleteCategoryAsync_ShouldSetIsDeleted_WhenCategoryExists()
    {
        // Arrange
        var category = MakeCategory(1);
        var categories = new List<KnowledgeCategory> { category };
        SetupDbSets(categories: categories);

        // Act
        await _service.DeleteCategoryAsync(1, CancellationToken);

        // Assert
        category.IsDeleted.Should().BeTrue();
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldThrowKeyNotFound_WhenCategoryNotFound()
    {
        // Arrange
        SetupDbSets(categories: new List<KnowledgeCategory>());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteCategoryAsync(999, CancellationToken));
    }
}
