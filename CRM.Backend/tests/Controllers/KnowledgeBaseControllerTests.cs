// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Dtos.KnowledgeBase;
using CRM.Core.Ports.Input;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="KnowledgeBaseController"/>.
/// Verifies HTTP status codes and response shapes for all 16 endpoints.
/// </summary>
public class KnowledgeBaseControllerTests
{
    private readonly Mock<IKnowledgeBaseService> _mockService;
    private readonly Mock<IUnifiedKnowledgeSearchService> _mockUnifiedSearch;
    private readonly KnowledgeBaseController _controller;

    public KnowledgeBaseControllerTests()
    {
        _mockService = new Mock<IKnowledgeBaseService>();
        _mockUnifiedSearch = new Mock<IUnifiedKnowledgeSearchService>();
        _controller = new KnowledgeBaseController(_mockService.Object, _mockUnifiedSearch.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // =========================================================================
    // GET /api/knowledge/articles
    // =========================================================================

    [Fact]
    public async Task GetAll_ShouldReturn200_WithPagedResult()
    {
        var paged = new PagedResultDto<KnowledgeBaseArticleDto>
        {
            Items = new List<KnowledgeBaseArticleDto> { new() { Id = 1, Title = "Article" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _mockService.Setup(s => s.GetAllAsync(1, 20, null, null, null, default))
            .ReturnsAsync(paged);

        var result = await _controller.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(paged);
    }

    // =========================================================================
    // GET /api/knowledge/articles/{id}
    // =========================================================================

    [Fact]
    public async Task GetById_ShouldReturn200_WhenArticleFound()
    {
        var dto = new KnowledgeBaseArticleDto { Id = 1, Title = "Found" };
        _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(dto);

        var result = await _controller.GetById(1);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenArticleNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(999, default))
            .ReturnsAsync((KnowledgeBaseArticleDto?)null);

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    // =========================================================================
    // GET /api/knowledge/articles/slug/{slug}
    // =========================================================================

    [Fact]
    public async Task GetBySlug_ShouldReturn200_WhenSlugFound()
    {
        var dto = new KnowledgeBaseArticleDto { Id = 1, Slug = "test-slug" };
        _mockService.Setup(s => s.GetBySlugAsync("test-slug", default)).ReturnsAsync(dto);

        var result = await _controller.GetBySlug("test-slug");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturn404_WhenSlugNotFound()
    {
        _mockService.Setup(s => s.GetBySlugAsync("bad-slug", default))
            .ReturnsAsync((KnowledgeBaseArticleDto?)null);

        var result = await _controller.GetBySlug("bad-slug");

        result.Should().BeOfType<NotFoundResult>();
    }

    // =========================================================================
    // POST /api/knowledge/articles
    // =========================================================================

    [Fact]
    public async Task Create_ShouldReturn201_WithCreatedArticle()
    {
        var dto = new CreateKnowledgeBaseArticleDto { Title = "New" };
        var created = new KnowledgeBaseArticleDto { Id = 5, Title = "New" };
        _mockService.Setup(s => s.CreateAsync(dto, 1, default)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(KnowledgeBaseController.GetById));
        createdResult.Value.Should().Be(created);
    }

    // =========================================================================
    // PUT /api/knowledge/articles/{id}
    // =========================================================================

    [Fact]
    public async Task Update_ShouldReturn200_WhenArticleUpdated()
    {
        var dto = new UpdateKnowledgeBaseArticleDto { Title = "Updated" };
        var updated = new KnowledgeBaseArticleDto { Id = 1, Title = "Updated" };
        _mockService.Setup(s => s.UpdateAsync(1, dto, default)).ReturnsAsync(updated);

        var result = await _controller.Update(1, dto);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenArticleNotFound()
    {
        var dto = new UpdateKnowledgeBaseArticleDto { Title = "X" };
        _mockService.Setup(s => s.UpdateAsync(999, dto, default))
            .ThrowsAsync(new KeyNotFoundException());

        var act = () => _controller.Update(999, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // =========================================================================
    // DELETE /api/knowledge/articles/{id}
    // =========================================================================

    [Fact]
    public async Task Delete_ShouldReturn204_WhenArticleDeleted()
    {
        _mockService.Setup(s => s.DeleteAsync(1, default)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    // =========================================================================
    // PATCH /api/knowledge/articles/{id}/publish
    // =========================================================================

    [Fact]
    public async Task Publish_ShouldReturn200_WhenArticlePublished()
    {
        var published = new KnowledgeBaseArticleDto { Id = 1, Status = 2 };
        _mockService.Setup(s => s.PublishAsync(1, default)).ReturnsAsync(published);

        var result = await _controller.Publish(1);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(published);
    }

    [Fact]
    public async Task Publish_ShouldThrowInvalidOperation_WhenArticleIsArchived()
    {
        _mockService.Setup(s => s.PublishAsync(1, default))
            .ThrowsAsync(new InvalidOperationException("Cannot publish archived"));

        var act = () => _controller.Publish(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // =========================================================================
    // PATCH /api/knowledge/articles/{id}/archive
    // =========================================================================

    [Fact]
    public async Task Archive_ShouldReturn200_WhenArticleArchived()
    {
        var archived = new KnowledgeBaseArticleDto { Id = 1, Status = 4 };
        _mockService.Setup(s => s.ArchiveAsync(1, default)).ReturnsAsync(archived);

        var result = await _controller.Archive(1);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(archived);
    }

    // =========================================================================
    // POST /api/knowledge/articles/{id}/feedback
    // =========================================================================

    [Fact]
    public async Task SubmitFeedback_ShouldReturn204()
    {
        var feedback = new KnowledgeBaseFeedbackDto { IsHelpful = true };
        _mockService.Setup(s => s.SubmitFeedbackAsync(1, feedback, default)).Returns(Task.CompletedTask);

        var result = await _controller.SubmitFeedback(1, feedback);

        result.Should().BeOfType<NoContentResult>();
    }

    // =========================================================================
    // GET /api/knowledge/categories
    // =========================================================================

    [Fact]
    public async Task GetCategories_ShouldReturn200_WithCategories()
    {
        var categories = new List<KnowledgeCategoryDto>
        {
            new() { Id = 1, Name = "General" }
        };
        _mockService.Setup(s => s.GetCategoriesAsync(default))
            .ReturnsAsync(categories);

        var result = await _controller.GetCategories();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(categories);
    }

    // =========================================================================
    // POST /api/knowledge/categories
    // =========================================================================

    [Fact]
    public async Task CreateCategory_ShouldReturn201_WithCreatedCategory()
    {
        var dto = new CreateKnowledgeCategoryDto { Name = "New Cat" };
        var created = new KnowledgeCategoryDto { Id = 3, Name = "New Cat" };
        _mockService.Setup(s => s.CreateCategoryAsync(dto, default)).ReturnsAsync(created);

        var result = await _controller.CreateCategory(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(created);
    }

    // =========================================================================
    // PUT /api/knowledge/categories/{id}
    // =========================================================================

    [Fact]
    public async Task UpdateCategory_ShouldReturn200_WhenCategoryUpdated()
    {
        var dto = new UpdateKnowledgeCategoryDto { Name = "Updated" };
        var updated = new KnowledgeCategoryDto { Id = 1, Name = "Updated" };
        _mockService.Setup(s => s.UpdateCategoryAsync(1, dto, default)).ReturnsAsync(updated);

        var result = await _controller.UpdateCategory(1, dto);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    // =========================================================================
    // DELETE /api/knowledge/categories/{id}
    // =========================================================================

    [Fact]
    public async Task DeleteCategory_ShouldReturn204()
    {
        _mockService.Setup(s => s.DeleteCategoryAsync(1, default)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteCategory(1);

        result.Should().BeOfType<NoContentResult>();
    }

    // =========================================================================
    // GET /api/knowledge/articles/popular
    // =========================================================================

    [Fact]
    public async Task GetPopular_ShouldReturn200_WithArticles()
    {
        var articles = new List<KnowledgeBaseArticleDto> { new() { Id = 1 } };
        _mockService.Setup(s => s.GetPopularAsync(10, default)).ReturnsAsync(articles);

        var result = await _controller.GetPopular();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(articles);
    }

    // =========================================================================
    // GET /api/knowledge/articles/recent
    // =========================================================================

    [Fact]
    public async Task GetRecent_ShouldReturn200_WithArticles()
    {
        var articles = new List<KnowledgeBaseArticleDto> { new() { Id = 2 } };
        _mockService.Setup(s => s.GetRecentAsync(10, default)).ReturnsAsync(articles);

        var result = await _controller.GetRecent();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(articles);
    }

    // =========================================================================
    // GET /api/knowledge/articles/by-product/{productId}
    // =========================================================================

    [Fact]
    public async Task GetByProduct_ShouldReturn200_WithArticles()
    {
        var articles = new List<KnowledgeBaseArticleDto> { new() { Id = 3 } };
        _mockService.Setup(s => s.GetByProductAsync(5, default)).ReturnsAsync(articles);

        var result = await _controller.GetByProduct(5);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(articles);
    }

    // =========================================================================
    // POST /api/knowledge/articles/{id}/case-deflection
    // =========================================================================

    [Fact]
    public async Task TrackCaseDeflection_ShouldReturn204()
    {
        _mockService.Setup(s => s.TrackCaseDeflectionAsync(1, null, default)).Returns(Task.CompletedTask);

        var result = await _controller.TrackCaseDeflection(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task TrackCaseDeflection_ShouldReturn204_WhenServiceRequestIdProvided()
    {
        _mockService.Setup(s => s.TrackCaseDeflectionAsync(1, 42, default)).Returns(Task.CompletedTask);

        var result = await _controller.TrackCaseDeflection(1, serviceRequestId: 42);

        result.Should().BeOfType<NoContentResult>();
    }
}
