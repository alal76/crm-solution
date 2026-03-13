// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// TCOV2-D09 — KnowledgeBaseController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Dtos.KnowledgeBase;
using CRM.Core.Ports.Input;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for KnowledgeBaseController (TCOV2-D09).
/// Route: /api/knowledge
/// [Authorize] at class level; some GET endpoints have [AllowAnonymous].
/// [Authorize] is middleware — not exercised in unit tests.
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
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
            }
        };
    }

    private static KnowledgeBaseArticleDto MakeArticleDto(int id = 1) => new()
    {
        Id = id,
        Title = $"Article {id}",
        Slug = $"article-{id}",
        Status = 1
    };

    // ── GetAll (articles) ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithArticles()
    {
        // Arrange
        var pagedResult = new PagedResultDto<KnowledgeBaseArticleDto>
        {
            Items = new List<KnowledgeBaseArticleDto> { MakeArticleDto(1) },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsServiceOnce()
    {
        _mockService.Setup(s => s.GetAllAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResultDto<KnowledgeBaseArticleDto> { Items = new List<KnowledgeBaseArticleDto>() });

        await _controller.GetAll();

        _mockService.Verify(s => s.GetAllAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenArticleExists()
    {
        // Arrange
        var dto = MakeArticleDto(7);
        _mockService.Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(7);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenArticleMissing()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((KnowledgeBaseArticleDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    // ── GetBySlug ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySlug_ShouldReturnNotFound_WhenArticleMissing()
    {
        _mockService.Setup(s => s.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((KnowledgeBaseArticleDto?)null);

        var result = await _controller.GetBySlug("missing-slug");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnOk_WhenArticleExists()
    {
        var dto = MakeArticleDto(3);
        _mockService.Setup(s => s.GetBySlugAsync("article-3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetBySlug("article-3");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDtoIsValid()
    {
        // Arrange
        var createDto = new CreateKnowledgeBaseArticleDto { Title = "How to start", Content = "..." };
        var returned = MakeArticleDto(20);
        _mockService.Setup(s => s.CreateAsync(
                It.IsAny<CreateKnowledgeBaseArticleDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(returned);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result).StatusCode.Should().Be(201);
    }
}
