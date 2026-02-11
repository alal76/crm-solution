// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for KnowledgeBaseController
/// Covers: Articles, Categories, Search, Feedback, Publishing workflow
/// </summary>
public class KnowledgeBaseControllerTests
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ILogger<KnowledgeBaseController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly KnowledgeBaseController _controller;

    public KnowledgeBaseControllerTests()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockLogger = new Mock<ILogger<KnowledgeBaseController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new KnowledgeBaseController(_mockKnowledgeBaseService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region Article GetAll Tests

    [Fact]
    public async Task GetArticles_ReturnsOkResult_WithArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, Title = "How to reset password", Status = "Published" },
            new KnowledgeArticleDto { Id = 2, Title = "How to create account", Status = "Draft" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetArticlesAsync())
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetArticles();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedArticles = okResult.Value as IEnumerable<KnowledgeArticleDto>;
        returnedArticles.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPublishedArticles_ReturnsOnlyPublished()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, Title = "Published Article", Status = "Published" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetPublishedArticlesAsync())
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetPublishedArticles();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetDraftArticles_ReturnsDrafts()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, Status = "Draft" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetDraftArticlesAsync())
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetDraftArticles();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByCategory_ReturnsFilteredArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, CategoryId = 1 }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetArticlesByCategoryAsync(1))
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetByCategory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetPopular_ReturnsPopularArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, ViewCount = 1000 }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetPopularArticlesAsync(10))
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetPopular(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetRecent_ReturnsRecentArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, PublishedDate = DateTime.Today }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetRecentArticlesAsync(10))
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetRecent(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Article GetById Tests

    [Fact]
    public async Task GetArticleById_ExistingArticle_ReturnsOk()
    {
        // Arrange
        var article = new KnowledgeArticleDto { Id = 1, Title = "Test Article" };

        _mockKnowledgeBaseService.Setup(s => s.GetArticleByIdAsync(1))
            .ReturnsAsync(article);

        // Act
        var result = await _controller.GetArticleById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetArticleById_NonExisting_ReturnsNotFound()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.GetArticleByIdAsync(999))
            .ReturnsAsync((KnowledgeArticleDto?)null);

        // Act
        var result = await _controller.GetArticleById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetArticleBySlug_ExistingArticle_ReturnsOk()
    {
        // Arrange
        var article = new KnowledgeArticleDto { Id = 1, Slug = "how-to-reset-password" };

        _mockKnowledgeBaseService.Setup(s => s.GetArticleBySlugAsync("how-to-reset-password"))
            .ReturnsAsync(article);

        // Act
        var result = await _controller.GetArticleBySlug("how-to-reset-password");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Article Create Tests

    [Fact]
    public async Task CreateArticle_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateKnowledgeArticleDto
        {
            Title = "New Article",
            Content = "Article content here",
            CategoryId = 1
        };

        var createdArticle = new KnowledgeArticleDto
        {
            Id = 1,
            Title = "New Article",
            Status = "Draft"
        };

        _mockKnowledgeBaseService.Setup(s => s.CreateArticleAsync(createDto))
            .ReturnsAsync(createdArticle);

        // Act
        var result = await _controller.CreateArticle(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task CreateArticle_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateArticle(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateArticle_MissingTitle_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateKnowledgeArticleDto { Content = "Content only" };

        _mockKnowledgeBaseService.Setup(s => s.CreateArticleAsync(createDto))
            .ThrowsAsync(new ArgumentException("Title is required"));

        // Act
        var result = await _controller.CreateArticle(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateArticle_DuplicateSlug_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateKnowledgeArticleDto
        {
            Title = "Existing Article",
            Slug = "existing-article"
        };

        _mockKnowledgeBaseService.Setup(s => s.CreateArticleAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Slug already exists"));

        // Act
        var result = await _controller.CreateArticle(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Article Update Tests

    [Fact]
    public async Task UpdateArticle_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateKnowledgeArticleDto
        {
            Id = 1,
            Title = "Updated Title",
            Content = "Updated content"
        };

        var updatedArticle = new KnowledgeArticleDto
        {
            Id = 1,
            Title = "Updated Title"
        };

        _mockKnowledgeBaseService.Setup(s => s.UpdateArticleAsync(updateDto))
            .ReturnsAsync(updatedArticle);

        // Act
        var result = await _controller.UpdateArticle(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateArticle_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateKnowledgeArticleDto { Id = 2 };

        // Act
        var result = await _controller.UpdateArticle(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateArticle_NonExisting_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateKnowledgeArticleDto { Id = 999 };

        _mockKnowledgeBaseService.Setup(s => s.UpdateArticleAsync(updateDto))
            .ReturnsAsync((KnowledgeArticleDto?)null);

        // Act
        var result = await _controller.UpdateArticle(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Article Publishing Workflow Tests

    [Fact]
    public async Task PublishArticle_ValidDraft_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.PublishArticleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PublishArticle(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task PublishArticle_AlreadyPublished_ReturnsConflict()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.PublishArticleAsync(1))
            .ThrowsAsync(new InvalidOperationException("Article is already published"));

        // Act
        var result = await _controller.PublishArticle(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task UnpublishArticle_ValidPublished_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.UnpublishArticleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UnpublishArticle(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SubmitForReview_ValidDraft_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.SubmitForReviewAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SubmitForReview(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ApproveArticle_ValidReview_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.ApproveArticleAsync(1, "Looks good"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ApproveArticle(1, "Looks good");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RejectArticle_ValidReview_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.RejectArticleAsync(1, "Needs more details"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectArticle(1, "Needs more details");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Article Version Tests

    [Fact]
    public async Task GetVersionHistory_ReturnsVersions()
    {
        // Arrange
        var versions = new List<ArticleVersionDto>
        {
            new ArticleVersionDto { VersionNumber = 1, CreatedAt = DateTime.Today.AddDays(-7) },
            new ArticleVersionDto { VersionNumber = 2, CreatedAt = DateTime.Today }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetVersionHistoryAsync(1))
            .ReturnsAsync(versions);

        // Act
        var result = await _controller.GetVersionHistory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetVersion_ValidVersion_ReturnsVersion()
    {
        // Arrange
        var version = new ArticleVersionDto { VersionNumber = 1 };

        _mockKnowledgeBaseService.Setup(s => s.GetVersionAsync(1, 1))
            .ReturnsAsync(version);

        // Act
        var result = await _controller.GetVersion(1, 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task RestoreVersion_ValidVersion_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.RestoreVersionAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RestoreVersion(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Category Tests

    [Fact]
    public async Task GetCategories_ReturnsCategories()
    {
        // Arrange
        var categories = new List<KnowledgeCategoryDto>
        {
            new KnowledgeCategoryDto { Id = 1, Name = "General" },
            new KnowledgeCategoryDto { Id = 2, Name = "Troubleshooting" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetCategoryById_ExistingCategory_ReturnsOk()
    {
        // Arrange
        var category = new KnowledgeCategoryDto { Id = 1, Name = "General" };

        _mockKnowledgeBaseService.Setup(s => s.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategoryById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateCategory_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateKnowledgeCategoryDto
        {
            Name = "New Category",
            Description = "Category description"
        };

        var createdCategory = new KnowledgeCategoryDto { Id = 1, Name = "New Category" };

        _mockKnowledgeBaseService.Setup(s => s.CreateCategoryAsync(createDto))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _controller.CreateCategory(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateCategory_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateKnowledgeCategoryDto
        {
            Id = 1,
            Name = "Updated Category"
        };

        var updatedCategory = new KnowledgeCategoryDto { Id = 1, Name = "Updated Category" };

        _mockKnowledgeBaseService.Setup(s => s.UpdateCategoryAsync(updateDto))
            .ReturnsAsync(updatedCategory);

        // Act
        var result = await _controller.UpdateCategory(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_ReturnsNoContent()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.DeleteCategoryAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCategory(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCategory_HasArticles_ReturnsConflict()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.DeleteCategoryAsync(1))
            .ThrowsAsync(new InvalidOperationException("Category has articles"));

        // Act
        var result = await _controller.DeleteCategory(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchArticles_ValidQuery_ReturnsResults()
    {
        // Arrange
        var results = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, Title = "How to reset password" }
        };

        _mockKnowledgeBaseService.Setup(s => s.SearchArticlesAsync("reset password"))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.SearchArticles("reset password");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task SearchArticles_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchArticles("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchArticles_NoResults_ReturnsEmptyList()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.SearchArticlesAsync("nonexistent"))
            .ReturnsAsync(new List<KnowledgeArticleDto>());

        // Act
        var result = await _controller.SearchArticles("nonexistent");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value as IEnumerable<KnowledgeArticleDto>;
        returnedResults.Should().BeEmpty();
    }

    #endregion

    #region Feedback Tests

    [Fact]
    public async Task RateArticle_ValidRating_ReturnsOk()
    {
        // Arrange
        var ratingDto = new ArticleRatingDto
        {
            Rating = 5,
            Comment = "Very helpful!"
        };

        _mockKnowledgeBaseService.Setup(s => s.RateArticleAsync(1, ratingDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RateArticle(1, ratingDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RateArticle_InvalidRating_ReturnsBadRequest()
    {
        // Arrange
        var ratingDto = new ArticleRatingDto { Rating = 10 };

        _mockKnowledgeBaseService.Setup(s => s.RateArticleAsync(1, ratingDto))
            .ThrowsAsync(new ArgumentException("Rating must be between 1 and 5"));

        // Act
        var result = await _controller.RateArticle(1, ratingDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MarkHelpful_ValidArticle_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.MarkHelpfulAsync(1, true))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkHelpful(1, true);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetFeedback_ReturnsArticleFeedback()
    {
        // Arrange
        var feedback = new List<ArticleFeedbackDto>
        {
            new ArticleFeedbackDto { Id = 1, Rating = 5, Comment = "Great!" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetArticleFeedbackAsync(1))
            .ReturnsAsync(feedback);

        // Act
        var result = await _controller.GetFeedback(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region View Tracking Tests

    [Fact]
    public async Task IncrementViewCount_ValidArticle_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.IncrementViewCountAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.IncrementViewCount(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetViewStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new ArticleViewStatisticsDto
        {
            ArticleId = 1,
            TotalViews = 1000,
            UniqueViews = 500,
            ViewsThisWeek = 50
        };

        _mockKnowledgeBaseService.Setup(s => s.GetViewStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetViewStatistics(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Tag Tests

    [Fact]
    public async Task GetTags_ReturnsTags()
    {
        // Arrange
        var tags = new List<string> { "password", "login", "account" };

        _mockKnowledgeBaseService.Setup(s => s.GetTagsAsync())
            .ReturnsAsync(tags);

        // Act
        var result = await _controller.GetTags();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetArticlesByTag_ReturnsTaggedArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 1, Tags = new List<string> { "password" } }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetArticlesByTagAsync("password"))
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetArticlesByTag("password");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task AddTag_ValidTag_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.AddTagAsync(1, "newtag"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddTag(1, "newtag");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveTag_ValidTag_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.RemoveTagAsync(1, "oldtag"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveTag(1, "oldtag");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Related Articles Tests

    [Fact]
    public async Task GetRelatedArticles_ReturnsRelated()
    {
        // Arrange
        var related = new List<KnowledgeArticleDto>
        {
            new KnowledgeArticleDto { Id = 2, Title = "Related Article" }
        };

        _mockKnowledgeBaseService.Setup(s => s.GetRelatedArticlesAsync(1))
            .ReturnsAsync(related);

        // Act
        var result = await _controller.GetRelatedArticles(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task LinkRelatedArticle_ValidArticles_ReturnsOk()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.LinkRelatedArticleAsync(1, 2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.LinkRelatedArticle(1, 2);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteArticle_ExistingArticle_ReturnsNoContent()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.DeleteArticleAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteArticle(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteArticle_NonExisting_ReturnsNotFound()
    {
        // Arrange
        _mockKnowledgeBaseService.Setup(s => s.DeleteArticleAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteArticle(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
