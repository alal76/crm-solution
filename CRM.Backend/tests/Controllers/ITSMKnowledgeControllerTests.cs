using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

public class ITSMKnowledgeControllerTests
{
    private readonly Mock<IKnowledgeManagementService> _mockService;
    private readonly KnowledgeController _controller;

    public ITSMKnowledgeControllerTests()
    {
        _mockService = new Mock<IKnowledgeManagementService>();
        _controller = new KnowledgeController(_mockService.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // POST /
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateArticle_ShouldReturnCreatedAtAction()
    {
        var createDto = new CreateKnowledgeArticleDto { Title = "How to reset password" };
        var created = new KnowledgeArticleDto { ArticleId = 1, Title = "How to reset password" };
        _mockService.Setup(s => s.CreateArticleAsync(createDto, 1)).ReturnsAsync(created);

        var result = await _controller.CreateArticle(createDto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(KnowledgeController.GetArticle));
        createdResult.Value.Should().Be(created);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetArticle_ShouldReturnOk_WhenArticleExists()
    {
        var article = new KnowledgeArticleDto { ArticleId = 1, Title = "KB001" };
        _mockService.Setup(s => s.GetArticleByIdAsync(1)).ReturnsAsync(article);

        var result = await _controller.GetArticle(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(article);
    }

    [Fact]
    public async Task GetArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        _mockService.Setup(s => s.GetArticleByIdAsync(999)).ReturnsAsync((KnowledgeArticleDto?)null);

        var result = await _controller.GetArticle(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /search
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchArticles_ShouldReturnOk()
    {
        var articles = new List<KnowledgeArticleDto>
        {
            new() { ArticleId = 1, Title = "Network troubleshooting" }
        };
        _mockService
            .Setup(s => s.SearchArticlesAsync("network", 1, 20))
            .ReturnsAsync(articles);

        var result = await _controller.SearchArticles("network", 1, 20);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<KnowledgeArticleDto>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /pending
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPendingArticles_ShouldFilterOutPublishedArticles()
    {
        var articles = new List<KnowledgeArticleDto>
        {
            new() { ArticleId = 1, Title = "Draft article", PublishingState = PublishingState.Draft },
            new() { ArticleId = 2, Title = "Published article", PublishingState = PublishingState.Published },
            new() { ArticleId = 3, Title = "Review article", PublishingState = PublishingState.Review }
        };
        _mockService.Setup(s => s.SearchArticlesAsync("", 1, 50)).ReturnsAsync(articles);

        var result = await _controller.GetPendingArticles(1, 50);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pending = okResult.Value.Should().BeAssignableTo<IEnumerable<KnowledgeArticleDto>>().Subject;
        pending.Should().HaveCount(2);
        pending.Should().NotContain(a => a.PublishingState == PublishingState.Published);
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateArticle_ShouldReturnOk()
    {
        var dto = new CreateKnowledgeArticleDto { Title = "Updated article" };
        var updated = new KnowledgeArticleDto { ArticleId = 1, Title = "Updated article" };
        _mockService.Setup(s => s.UpdateArticleAsync(1, dto, 1)).ReturnsAsync(updated);

        var result = await _controller.UpdateArticle(1, dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updated);
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/publish
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishArticle_ShouldReturnOk()
    {
        _mockService.Setup(s => s.PublishArticleAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.PublishArticle(1);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // PATCH /{id}/retire
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RetireArticle_ShouldReturnOk()
    {
        _mockService.Setup(s => s.RetireArticleAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.RetireArticle(1);

        result.Should().BeOfType<OkResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/feedback
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddFeedback_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.SubmitFeedbackAsync(1, 1, true, "Very helpful")).ReturnsAsync(true);

        var dto = new AddFeedbackDto { Helpful = true, Comments = "Very helpful" };
        var result = await _controller.AddFeedback(1, dto);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AddFeedback_ShouldReturnBadRequest_WhenFails()
    {
        _mockService.Setup(s => s.SubmitFeedbackAsync(1, 1, false, "Not useful")).ReturnsAsync(false);

        var dto = new AddFeedbackDto { Helpful = false, Comments = "Not useful" };
        var result = await _controller.AddFeedback(1, dto);

        result.Should().BeOfType<BadRequestResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /suggested
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSuggestedArticles_ShouldReturnOk()
    {
        var articles = new List<KnowledgeArticleDto>
        {
            new() { ArticleId = 1, Title = "Suggested KB" }
        };
        _mockService
            .Setup(s => s.GetSuggestedArticlesAsync("printer not working"))
            .ReturnsAsync(articles);

        var result = await _controller.GetSuggestedArticles("printer not working");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<KnowledgeArticleDto>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /popular
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPopularArticles_ShouldReturnOkWithDefaultCount()
    {
        var articles = new List<KnowledgeArticleDto> { new() { ArticleId = 1 } };
        _mockService.Setup(s => s.GetPopularArticlesAsync(10)).ReturnsAsync(articles);

        var result = await _controller.GetPopularArticles();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /recent
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecentArticles_ShouldReturnOkWithDefaultCount()
    {
        var articles = new List<KnowledgeArticleDto> { new() { ArticleId = 1 } };
        _mockService.Setup(s => s.GetRecentArticlesAsync(10)).ReturnsAsync(articles);

        var result = await _controller.GetRecentArticles();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /list
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListArticles_ShouldReturnOk_WithNullSearchDefaultsToEmpty()
    {
        var articles = new List<KnowledgeArticleDto>
        {
            new() { ArticleId = 1, Title = "Article A" }
        };
        _mockService.Setup(s => s.SearchArticlesAsync("", 1, 20)).ReturnsAsync(articles);

        var result = await _controller.ListArticles(null, 1, 20);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /categories
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_ShouldReturnOk()
    {
        var categories = new List<string> { "Hardware", "Software", "Network" };
        _mockService.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _controller.GetCategories();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returned.Should().HaveCount(3);
    }
}
