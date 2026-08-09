// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class ITSMKnowledgeControllerTests
{
    private readonly Mock<IKnowledgeManagementService> _mockService;
    private readonly Mock<IArticleRecommendationService> _mockRecommendationService;
    private readonly Mock<IKCSWorkflowService> _mockKcsService;
    private readonly KnowledgeController _controller;

    public ITSMKnowledgeControllerTests()
    {
        _mockService = new Mock<IKnowledgeManagementService>();
        _mockRecommendationService = new Mock<IArticleRecommendationService>();
        _mockKcsService = new Mock<IKCSWorkflowService>();
        _controller = new KnowledgeController(_mockService.Object, _mockRecommendationService.Object, _mockKcsService.Object);

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

    // ────────────────────────────────────────────────────────────────
    // GET /incidents/{incidentId}/article-recommendations
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetArticleRecommendations_ShouldReturnOk_WithRecommendations()
    {
        var recommendations = new List<ArticleRecommendation>
        {
            new(1, "Reset your password", "Steps to reset a password", 0.85, 120)
        };
        _mockRecommendationService.Setup(s => s.GetRecommendationsAsync(42, default)).ReturnsAsync(recommendations);

        var result = await _controller.GetArticleRecommendations(42);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<ArticleRecommendation>>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetArticleRecommendations_ShouldReturnOk_WithEmptyList_WhenIncidentNotFound()
    {
        _mockRecommendationService
            .Setup(s => s.GetRecommendationsAsync(999, default))
            .ReturnsAsync(Enumerable.Empty<ArticleRecommendation>());

        var result = await _controller.GetArticleRecommendations(999);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<ArticleRecommendation>>().Subject;
        returned.Should().BeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /articles/trending
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTrendingArticles_ShouldReturnOkWithDefaultCount()
    {
        var articles = new List<TrendingArticle> { new(1, "Trending article", 500, TrendDirection.Up) };
        _mockRecommendationService.Setup(s => s.GetTrendingArticlesAsync(10, default)).ReturnsAsync(articles);

        var result = await _controller.GetTrendingArticles();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<TrendingArticle>>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTrendingArticles_ShouldReturnOk_WithEmptyList_WhenNoPublishedArticles()
    {
        _mockRecommendationService
            .Setup(s => s.GetTrendingArticlesAsync(5, default))
            .ReturnsAsync(Enumerable.Empty<TrendingArticle>());

        var result = await _controller.GetTrendingArticles(5);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<TrendingArticle>>().Subject;
        returned.Should().BeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /incidents/{incidentId}/kcs/capture-session
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartKCSCaptureSession_ShouldReturnOk_WhenIncidentExists()
    {
        var session = new KCSSession { SessionId = 1, IncidentId = 42, AgentId = 1, State = KCSSessionState.Active };
        _mockKcsService.Setup(s => s.StartCaptureSessionAsync(42, 1)).ReturnsAsync(session);

        var result = await _controller.StartKCSCaptureSession(42);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(session);
    }

    [Fact]
    public async Task StartKCSCaptureSession_ShouldReturnNotFound_WhenIncidentDoesNotExist()
    {
        _mockKcsService
            .Setup(s => s.StartCaptureSessionAsync(999, 1))
            .ThrowsAsync(new ArgumentException("Incident 999 not found"));

        var result = await _controller.StartKCSCaptureSession(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /incidents/{incidentId}/kcs/draft
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateKCSDraft_ShouldReturnOk_WhenIncidentExists()
    {
        var draft = new KCSDraftArticle { DraftId = 1, SourceIncidentId = 42, Title = "Draft title", AuthorId = 1 };
        _mockKcsService.Setup(s => s.CreateDraftFromIncidentAsync(42, 1)).ReturnsAsync(draft);

        var result = await _controller.CreateKCSDraft(42);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(draft);
    }

    [Fact]
    public async Task CreateKCSDraft_ShouldReturnNotFound_WhenIncidentDoesNotExist()
    {
        _mockKcsService
            .Setup(s => s.CreateDraftFromIncidentAsync(999, 1))
            .ThrowsAsync(new ArgumentException("Incident 999 not found"));

        var result = await _controller.CreateKCSDraft(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/kcs/submit-review
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitKCSReview_ShouldReturnOk_WhenArticleExists()
    {
        var reviewRequest = new KCSReviewRequest { ReviewRequestId = 1, ArticleId = 5, SubmittedById = 1 };
        _mockKcsService.Setup(s => s.SubmitForReviewAsync(5, 1)).ReturnsAsync(reviewRequest);

        var result = await _controller.SubmitKCSReview(5);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(reviewRequest);
    }

    [Fact]
    public async Task SubmitKCSReview_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        _mockKcsService
            .Setup(s => s.SubmitForReviewAsync(999, 1))
            .ThrowsAsync(new ArgumentException("Article 999 not found"));

        var result = await _controller.SubmitKCSReview(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /kcs/reviews/{reviewRequestId}/decide
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DecideKCSReview_ShouldReturnOk_WhenReviewRequestExists()
    {
        var reviewResult = new KCSReviewResult { ReviewRequestId = 7, ArticleId = 5, Decision = KCSReviewDecision.Approve, ReviewerId = 1 };
        _mockKcsService
            .Setup(s => s.ReviewArticleAsync(7, 1, KCSReviewDecision.Approve, "Looks good"))
            .ReturnsAsync(reviewResult);

        var dto = new KCSReviewDecisionRequest { Decision = KCSReviewDecision.Approve, Feedback = "Looks good" };
        var result = await _controller.DecideKCSReview(7, dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(reviewResult);
    }

    [Fact]
    public async Task DecideKCSReview_ShouldReturnNotFound_WhenReviewRequestDoesNotExist()
    {
        _mockKcsService
            .Setup(s => s.ReviewArticleAsync(999, 1, KCSReviewDecision.Reject, null))
            .ThrowsAsync(new ArgumentException("Review request 999 not found"));

        var dto = new KCSReviewDecisionRequest { Decision = KCSReviewDecision.Reject };
        var result = await _controller.DecideKCSReview(999, dto);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /kcs/agents/{agentId}/metrics
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetKCSAgentMetrics_ShouldReturnOk()
    {
        var fromDate = new DateTime(2026, 1, 1);
        var toDate = new DateTime(2026, 2, 1);
        var metrics = new KCSAgentMetrics { AgentId = 3, FromDate = fromDate, ToDate = toDate, ArticlesCreated = 4 };
        _mockKcsService.Setup(s => s.GetAgentMetricsAsync(3, fromDate, toDate)).ReturnsAsync(metrics);

        var result = await _controller.GetKCSAgentMetrics(3, fromDate, toDate);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(metrics);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /{id}/kcs/lifecycle
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetKCSArticleLifecycle_ShouldReturnOk_WhenArticleExists()
    {
        var lifecycle = new KCSArticleLifecycle { ArticleId = 5, CurrentStage = KCSLifecycleStage.Published };
        _mockKcsService.Setup(s => s.GetArticleLifecycleAsync(5)).ReturnsAsync(lifecycle);

        var result = await _controller.GetKCSArticleLifecycle(5);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(lifecycle);
    }

    [Fact]
    public async Task GetKCSArticleLifecycle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        _mockKcsService
            .Setup(s => s.GetArticleLifecycleAsync(999))
            .ThrowsAsync(new ArgumentException("Article 999 not found"));

        var result = await _controller.GetKCSArticleLifecycle(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /{id}/kcs/flag
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FlagKCSArticle_ShouldReturnOk_WhenSuccessful()
    {
        _mockKcsService.Setup(s => s.FlagArticleAsync(5, KCSFlag.Outdated, 1, "Steps no longer apply")).ReturnsAsync(true);

        var dto = new KCSFlagRequest { Flag = KCSFlag.Outdated, Reason = "Steps no longer apply" };
        var result = await _controller.FlagKCSArticle(5, dto);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task FlagKCSArticle_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        _mockKcsService.Setup(s => s.FlagArticleAsync(999, KCSFlag.Outdated, 1, "Missing")).ReturnsAsync(false);

        var dto = new KCSFlagRequest { Flag = KCSFlag.Outdated, Reason = "Missing" };
        var result = await _controller.FlagKCSArticle(999, dto);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /kcs/coaching-queue
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetKCSCoachingQueue_ShouldReturnOk_WithItems()
    {
        var queue = new List<KCSCoachingItem>
        {
            new() { ItemId = 1, ArticleId = 5, AuthorId = 2, Reason = CoachingReason.NewAuthor }
        };
        _mockKcsService.Setup(s => s.GetCoachingQueueAsync(1)).ReturnsAsync(queue);

        var result = await _controller.GetKCSCoachingQueue();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<List<KCSCoachingItem>>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetKCSCoachingQueue_ShouldReturnOk_WithEmptyList_WhenNoItems()
    {
        _mockKcsService.Setup(s => s.GetCoachingQueueAsync(1)).ReturnsAsync(new List<KCSCoachingItem>());

        var result = await _controller.GetKCSCoachingQueue();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<List<KCSCoachingItem>>().Subject;
        returned.Should().BeEmpty();
    }
}
