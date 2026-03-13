// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos.KnowledgeBase;
using CRM.Core.Entities;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class KnowledgeBaseServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<KnowledgeBaseService>> _mockLogger;
    private readonly KnowledgeBaseService _service;

    public KnowledgeBaseServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"KBTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        // Seed a user so Include(a => a.Author) navigation can be resolved
        _context.Users.Add(new User { Id = 1, Username = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User", PasswordHash = "hash", CreatedAt = DateTime.UtcNow });
        _context.SaveChanges();
        _mockLogger = new Mock<ILogger<KnowledgeBaseService>>();
        _service = new KnowledgeBaseService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    private KnowledgeArticle CreateArticle(string title, string slug, ArticleStatus status = ArticleStatus.Draft)
    {
        return new KnowledgeArticle
        {
            ArticleNumber = $"KB{Guid.NewGuid():N}".Substring(0, 12),
            Title = title,
            Slug = slug,
            Content = "Test content",
            Status = status,
            AuthorUserId = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnArticle_WhenExists()
    {
        var article = CreateArticle("How to Reset Password", "how-to-reset-password");
        _context.KnowledgeArticles.Add(article);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(article.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("How to Reset Password");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnArticle_WhenSlugMatches()
    {
        var article = CreateArticle("VPN Setup Guide", "vpn-setup-guide");
        _context.KnowledgeArticles.Add(article);
        await _context.SaveChangesAsync();

        var result = await _service.GetBySlugAsync("vpn-setup-guide");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("vpn-setup-guide");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugNotFound()
    {
        var result = await _service.GetBySlugAsync("nonexistent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddArticleToDatabase()
    {
        var dto = new CreateKnowledgeBaseArticleDto
        {
            Title = "New Article Title",
            Content = "Article content here",
            ArticleType = ArticleType.HowTo,
            Status = ArticleStatus.Draft
        };

        var result = await _service.CreateAsync(dto, authorId: 1);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Article Title");
        _context.KnowledgeArticles.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateArticleNumber()
    {
        var dto = new CreateKnowledgeBaseArticleDto
        {
            Title = "Article With Number",
            Content = "Content",
            ArticleType = ArticleType.HowTo,
            Status = ArticleStatus.Draft
        };

        var result = await _service.CreateAsync(dto, authorId: 1);

        result.ArticleNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PublishAsync_ShouldSetStatusToPublished()
    {
        var article = CreateArticle("Draft Article", "draft-article", ArticleStatus.Draft);
        _context.KnowledgeArticles.Add(article);
        await _context.SaveChangesAsync();

        var result = await _service.PublishAsync(article.Id);

        result.Status.Should().Be((int)ArticleStatus.Published);
        var updated = await _context.KnowledgeArticles.FindAsync(article.Id);
        updated!.Status.Should().Be(ArticleStatus.Published);
        updated.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ArchiveAsync_ShouldSetStatusToArchived()
    {
        var article = CreateArticle("Published Article", "published-article", ArticleStatus.Published);
        _context.KnowledgeArticles.Add(article);
        await _context.SaveChangesAsync();

        var result = await _service.ArchiveAsync(article.Id);

        result.Status.Should().Be((int)ArticleStatus.Archived);
        var updated = await _context.KnowledgeArticles.FindAsync(article.Id);
        updated!.Status.Should().Be(ArticleStatus.Archived);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteArticle()
    {
        var article = CreateArticle("Article To Delete", "article-to-delete");
        _context.KnowledgeArticles.Add(article);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(article.Id);

        var deleted = await _context.KnowledgeArticles.FindAsync(article.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResults()
    {
        _context.KnowledgeArticles.AddRange(
            CreateArticle("Article One", "article-one"),
            CreateArticle("Article Two", "article-two"),
            CreateArticle("Article Three", "article-three")
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(page: 1, pageSize: 2, search: null, categoryId: null, status: null);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }
}
