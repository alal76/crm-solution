// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LandingPageService using InMemory database.
/// Tests CRUD operations, publish/unpublish, slug operations, duplicate,
/// visits, conversions, analytics, and block management.
/// </summary>
public class LandingPageServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<LandingPageService>> _mockLogger;
    private readonly LandingPageService _service;

    public LandingPageServiceTests()
    {
        _dbContext = CreateDbContext();
        _mockLogger = new Mock<ILogger<LandingPageService>>();
        _service = new LandingPageService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CrmDbContext(options, null!);
    }

    private LandingPage CreateTestLandingPage(
        string name = "Test Landing Page",
        string slug = "test-landing-page",
        LandingPageStatus status = LandingPageStatus.Draft,
        LandingPageTemplate template = LandingPageTemplate.Blank,
        int createdByUserId = 1,
        int? campaignId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new LandingPage
        {
            Name = name,
            Slug = slug,
            Title = $"Title for {name}",
            MetaDescription = $"Meta description for {name}",
            Status = status,
            Template = template,
            CreatedByUserId = createdByUserId,
            CampaignId = campaignId,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<LandingPage> SeedLandingPageAsync(
        string name = "Seeded Page",
        string slug = "seeded-page",
        LandingPageStatus status = LandingPageStatus.Draft,
        LandingPageTemplate template = LandingPageTemplate.Blank,
        int createdByUserId = 1,
        int? campaignId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        await EnsureUserExistsAsync(createdByUserId);
        var page = CreateTestLandingPage(name, slug, status, template, createdByUserId, campaignId, isActive, isDeleted);
        _dbContext.LandingPages.Add(page);
        await _dbContext.SaveChangesAsync();
        return page;
    }

    private async Task EnsureUserExistsAsync(int userId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
        {
            _dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"user{userId}@test.com",
                FirstName = "Test",
                LastName = $"User{userId}",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<LandingPageBlock> SeedBlockAsync(
        int landingPageId,
        LandingPageBlockType blockType = LandingPageBlockType.Text,
        int sortOrder = 0,
        bool isVisible = true)
    {
        var block = new LandingPageBlock
        {
            LandingPageId = landingPageId,
            BlockType = blockType,
            SortOrder = sortOrder,
            ContentJson = "{\"text\":\"test content\"}",
            StyleJson = "{\"color\":\"red\"}",
            IsVisible = isVisible,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.LandingPageBlocks.Add(block);
        await _dbContext.SaveChangesAsync();
        return block;
    }

    private async Task<LandingPageVisit> SeedVisitAsync(
        int landingPageId,
        string? visitorId = "visitor-1",
        bool converted = false,
        string? deviceType = "desktop",
        string? country = "US",
        DateTime? visitedAt = null)
    {
        var visit = new LandingPageVisit
        {
            LandingPageId = landingPageId,
            VisitorId = visitorId,
            Converted = converted,
            ConvertedAt = converted ? DateTime.UtcNow : null,
            DeviceType = deviceType,
            Country = country,
            VisitedAt = visitedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.LandingPageVisits.Add(visit);
        await _dbContext.SaveChangesAsync();
        return visit;
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedPages_WhenNoFilterProvided()
    {
        // Arrange
        await SeedLandingPageAsync("Page 1", "page-1");
        await SeedLandingPageAsync("Page 2", "page-2");
        await SeedLandingPageAsync("Deleted Page", "deleted-page", isDeleted: true);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(p => p.Name == "Deleted Page");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCampaignId_WhenCampaignIdProvided()
    {
        // Arrange
        await SeedLandingPageAsync("Campaign Page", "campaign-page", campaignId: 10);
        await SeedLandingPageAsync("Other Page", "other-page", campaignId: 20);

        // Act
        var result = await _service.GetAllAsync(campaignId: 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Campaign Page");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus_WhenStatusProvided()
    {
        // Arrange
        await SeedLandingPageAsync("Draft Page", "draft-page", status: LandingPageStatus.Draft);
        await SeedLandingPageAsync("Published Page", "published-page", status: LandingPageStatus.Published);

        // Act
        var result = await _service.GetAllAsync(status: LandingPageStatus.Published);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Published Page");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoPagesExist()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPage_WhenPageExists()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Find Me", "find-me");

        // Act
        var result = await _service.GetByIdAsync(page.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
        result.Slug.Should().Be("find-me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPageNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPageIsDeleted()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Deleted", "deleted", isDeleted: true);

        // Act
        var result = await _service.GetByIdAsync(page.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetBySlugAsync Tests

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnPage_WhenSlugMatchesPublishedActivePage()
    {
        // Arrange
        await SeedLandingPageAsync("Published", "my-slug", status: LandingPageStatus.Published, isActive: true);

        // Act
        var result = await _service.GetBySlugAsync("my-slug");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("my-slug");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenPageIsDraft()
    {
        // Arrange
        await SeedLandingPageAsync("Draft", "draft-slug", status: LandingPageStatus.Draft);

        // Act
        var result = await _service.GetBySlugAsync("draft-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenPageIsNotActive()
    {
        // Arrange
        await SeedLandingPageAsync("Inactive", "inactive-slug", status: LandingPageStatus.Published, isActive: false);

        // Act
        var result = await _service.GetBySlugAsync("inactive-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugNotFound()
    {
        // Act
        var result = await _service.GetBySlugAsync("nonexistent-slug");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreatePage_WithDraftStatus()
    {
        // Arrange
        var page = CreateTestLandingPage("New Page", "new-page");

        // Act
        var result = await _service.CreateAsync(page, userId: 1);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Status.Should().Be(LandingPageStatus.Draft);
        result.CreatedByUserId.Should().Be(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateSlug_WhenSlugIsEmpty()
    {
        // Arrange
        var page = CreateTestLandingPage("My New Campaign Page", "");

        // Act
        var result = await _service.CreateAsync(page, userId: 1);

        // Assert
        result.Slug.Should().NotBeNullOrEmpty();
        result.Slug.Should().Contain("my-new-campaign-page");
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateUniqueSlug_WhenSlugAlreadyExists()
    {
        // Arrange
        await SeedLandingPageAsync("Existing", "test-slug");
        var page = CreateTestLandingPage("Test Slug", "test-slug");

        // Act
        var result = await _service.CreateAsync(page, userId: 1);

        // Assert
        result.Slug.Should().NotBe("test-slug");
        result.Slug.Should().StartWith("test-slug");
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var page = CreateTestLandingPage("Persisted Page", "persisted");

        // Act
        var result = await _service.CreateAsync(page, userId: 1);

        // Assert
        var fromDb = await _dbContext.LandingPages.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted Page");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProperties_WhenPageExists()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Original", "original");
        page.Name = "Updated Name";
        page.Title = "Updated Title";
        page.MetaDescription = "Updated Description";

        // Act
        var result = await _service.UpdateAsync(page);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Title.Should().Be("Updated Title");
        result.MetaDescription.Should().Be("Updated Description");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenPageNotFound()
    {
        // Arrange
        var page = new LandingPage { Id = 999, Name = "Ghost" };

        // Act
        var act = () => _service.UpdateAsync(page);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*999*not found*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldAllowSlugChange_WhenPageIsInDraftStatus()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Draft Page", "old-slug", status: LandingPageStatus.Draft);
        page.Slug = "new-slug";

        // Act
        var result = await _service.UpdateAsync(page);

        // Assert
        result.Slug.Should().Be("new-slug");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenPageExists()
    {
        // Arrange
        var page = await SeedLandingPageAsync("To Delete", "to-delete");

        // Act
        var result = await _service.DeleteAsync(page.Id);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.LandingPages.FindAsync(page.Id);
        fromDb!.IsDeleted.Should().BeTrue();
        fromDb.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenPageNotFound()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PublishAsync Tests

    [Fact]
    public async Task PublishAsync_ShouldSetStatusToPublished_WhenPageExists()
    {
        // Arrange
        var page = await SeedLandingPageAsync("To Publish", "to-publish");

        // Act
        var result = await _service.PublishAsync(page.Id);

        // Assert
        result.Status.Should().Be(LandingPageStatus.Published);
        result.IsActive.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
        result.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PublishAsync_ShouldThrowArgumentException_WhenPageNotFound()
    {
        // Act
        var act = () => _service.PublishAsync(999);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*999*not found*");
    }

    [Fact]
    public async Task PublishAsync_ShouldCompileHtmlContent()
    {
        // Arrange
        var page = await SeedLandingPageAsync("HTML Page", "html-page");

        // Act
        var result = await _service.PublishAsync(page.Id);

        // Assert
        result.HtmlContent.Should().NotBeNullOrEmpty();
        result.HtmlContent.Should().Contain("<!DOCTYPE html>");
    }

    #endregion

    #region UnpublishAsync Tests

    [Fact]
    public async Task UnpublishAsync_ShouldSetStatusToDraft_WhenPageExists()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Published", "published", status: LandingPageStatus.Published);

        // Act
        var result = await _service.UnpublishAsync(page.Id);

        // Assert
        result.Status.Should().Be(LandingPageStatus.Draft);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnpublishAsync_ShouldThrowArgumentException_WhenPageNotFound()
    {
        // Act
        var act = () => _service.UnpublishAsync(999);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*999*not found*");
    }

    #endregion

    #region DuplicateAsync Tests

    [Fact]
    public async Task DuplicateAsync_ShouldCreateCopyWithDraftStatus()
    {
        // Arrange
        var original = await SeedLandingPageAsync("Original Page", "original-page", status: LandingPageStatus.Published);

        // Act
        var result = await _service.DuplicateAsync(original.Id, "Copied Page", userId: 2);

        // Assert
        result.Id.Should().NotBe(original.Id);
        result.Name.Should().Be("Copied Page");
        result.Status.Should().Be(LandingPageStatus.Draft);
        result.CreatedByUserId.Should().Be(2);
        result.Template.Should().Be(original.Template);
    }

    [Fact]
    public async Task DuplicateAsync_ShouldGenerateNewSlug()
    {
        // Arrange
        var original = await SeedLandingPageAsync("Original", "original-slug");

        // Act
        var result = await _service.DuplicateAsync(original.Id, "Copied", userId: 1);

        // Assert
        result.Slug.Should().NotBeNullOrEmpty();
        result.Slug.Should().NotBe(original.Slug);
    }

    [Fact]
    public async Task DuplicateAsync_ShouldCopyBlocks()
    {
        // Arrange
        var original = await SeedLandingPageAsync("With Blocks", "with-blocks");
        await SeedBlockAsync(original.Id, LandingPageBlockType.Hero, 0);
        await SeedBlockAsync(original.Id, LandingPageBlockType.Text, 1);

        // Act
        var result = await _service.DuplicateAsync(original.Id, "Copied With Blocks", userId: 1);

        // Assert
        var duplicatedBlocks = await _dbContext.LandingPageBlocks
            .Where(b => b.LandingPageId == result.Id)
            .ToListAsync();
        duplicatedBlocks.Should().HaveCount(2);
    }

    [Fact]
    public async Task DuplicateAsync_ShouldThrowArgumentException_WhenOriginalNotFound()
    {
        // Act
        var act = () => _service.DuplicateAsync(999, "Copy", userId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*999*not found*");
    }

    #endregion

    #region IsSlugAvailableAsync Tests

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnTrue_WhenSlugNotUsed()
    {
        // Act
        var result = await _service.IsSlugAvailableAsync("available-slug");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnFalse_WhenSlugAlreadyExists()
    {
        // Arrange
        await SeedLandingPageAsync("Existing", "taken-slug");

        // Act
        var result = await _service.IsSlugAvailableAsync("taken-slug");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnTrue_WhenExcludingOwnId()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Own Page", "own-slug");

        // Act
        var result = await _service.IsSlugAvailableAsync("own-slug", excludeId: page.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnTrue_WhenSlugUsedByDeletedPage()
    {
        // Arrange
        await SeedLandingPageAsync("Deleted", "deleted-slug", isDeleted: true);

        // Act
        var result = await _service.IsSlugAvailableAsync("deleted-slug");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GenerateSlugAsync Tests

    [Fact]
    public async Task GenerateSlugAsync_ShouldConvertToLowercaseHyphenated()
    {
        // Act
        var result = await _service.GenerateSlugAsync("My Great Page");

        // Assert
        result.Should().Be("my-great-page");
    }

    [Fact]
    public async Task GenerateSlugAsync_ShouldRemoveSpecialCharacters()
    {
        // Act
        var result = await _service.GenerateSlugAsync("Hello! World @ 2025");

        // Assert
        result.Should().Be("hello-world-2025");
    }

    [Fact]
    public async Task GenerateSlugAsync_ShouldAppendSuffix_WhenSlugExists()
    {
        // Arrange
        await SeedLandingPageAsync("Existing", "existing-name");

        // Act
        var result = await _service.GenerateSlugAsync("Existing Name");

        // Assert
        result.Should().StartWith("existing-name");
        result.Should().NotBe("existing-name");
    }

    #endregion

    #region RecordVisitAsync Tests

    [Fact]
    public async Task RecordVisitAsync_ShouldCreateVisitRecord()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Visited Page", "visited-page");
        var visit = new LandingPageVisit
        {
            LandingPageId = page.Id,
            VisitorId = "visitor-abc",
            DeviceType = "mobile",
            Country = "UK"
        };

        // Act
        var result = await _service.RecordVisitAsync(visit);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.VisitedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RecordVisitAsync_ShouldIncrementPageViews()
    {
        // Arrange
        var page = await SeedLandingPageAsync("View Count Page", "view-count-page");
        var visit = new LandingPageVisit
        {
            LandingPageId = page.Id,
            VisitorId = "visitor-1"
        };

        // Act
        await _service.RecordVisitAsync(visit);

        // Assert
        var updatedPage = await _dbContext.LandingPages.FindAsync(page.Id);
        updatedPage!.PageViews.Should().Be(1);
    }

    [Fact]
    public async Task RecordVisitAsync_ShouldIncrementUniqueVisitors_WhenNewVisitor()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Unique Page", "unique-page");
        var visit = new LandingPageVisit
        {
            LandingPageId = page.Id,
            VisitorId = "new-visitor"
        };

        // Act
        await _service.RecordVisitAsync(visit);

        // Assert
        var updatedPage = await _dbContext.LandingPages.FindAsync(page.Id);
        updatedPage!.UniqueVisitors.Should().Be(1);
    }

    #endregion

    #region RecordConversionAsync Tests

    [Fact]
    public async Task RecordConversionAsync_ShouldIncrementConversions_WhenPageExists()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Conversion Page", "conversion-page");

        // Act
        var result = await _service.RecordConversionAsync(page.Id, null, null);

        // Assert
        result.Should().BeTrue();
        var updatedPage = await _dbContext.LandingPages.FindAsync(page.Id);
        updatedPage!.Conversions.Should().Be(1);
    }

    [Fact]
    public async Task RecordConversionAsync_ShouldReturnFalse_WhenPageNotFound()
    {
        // Act
        var result = await _service.RecordConversionAsync(999, null, null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RecordConversionAsync_ShouldUpdateVisitRecord_WhenVisitorIdProvided()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Conversion Track", "conversion-track");
        await SeedVisitAsync(page.Id, visitorId: "visitor-convert");

        // Act
        var result = await _service.RecordConversionAsync(page.Id, "visitor-convert", leadId: 42);

        // Assert
        result.Should().BeTrue();
        var visit = await _dbContext.LandingPageVisits
            .FirstOrDefaultAsync(v => v.LandingPageId == page.Id && v.VisitorId == "visitor-convert");
        visit!.Converted.Should().BeTrue();
        visit.ConvertedAt.Should().NotBeNull();
        visit.LeadId.Should().Be(42);
    }

    #endregion

    #region GetAnalyticsAsync Tests

    [Fact]
    public async Task GetAnalyticsAsync_ShouldReturnAnalytics_WithCorrectPageViews()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Analytics Page", "analytics-page");
        await SeedVisitAsync(page.Id, visitorId: "v1", deviceType: "desktop", country: "US");
        await SeedVisitAsync(page.Id, visitorId: "v2", deviceType: "mobile", country: "UK");
        await SeedVisitAsync(page.Id, visitorId: "v1", deviceType: "desktop", country: "US");

        // Act
        var result = await _service.GetAnalyticsAsync(page.Id);

        // Assert
        result.TotalPageViews.Should().Be(3);
        result.UniqueVisitors.Should().Be(2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Date Range Page", "date-range-page");
        await SeedVisitAsync(page.Id, visitorId: "v1", visitedAt: DateTime.UtcNow.AddDays(-5));
        await SeedVisitAsync(page.Id, visitorId: "v2", visitedAt: DateTime.UtcNow.AddDays(-60));

        // Act
        var result = await _service.GetAnalyticsAsync(page.Id, startDate: DateTime.UtcNow.AddDays(-10));

        // Assert
        result.TotalPageViews.Should().Be(1);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ShouldCalculateConversionRate()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Rate Page", "rate-page");
        await SeedVisitAsync(page.Id, visitorId: "v1", converted: true);
        await SeedVisitAsync(page.Id, visitorId: "v2", converted: false);

        // Act
        var result = await _service.GetAnalyticsAsync(page.Id);

        // Assert
        result.Conversions.Should().Be(1);
        result.ConversionRate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ShouldReturnViewsByDevice()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Device Page", "device-page");
        await SeedVisitAsync(page.Id, visitorId: "v1", deviceType: "desktop");
        await SeedVisitAsync(page.Id, visitorId: "v2", deviceType: "mobile");
        await SeedVisitAsync(page.Id, visitorId: "v3", deviceType: "desktop");

        // Act
        var result = await _service.GetAnalyticsAsync(page.Id);

        // Assert
        result.ViewsByDevice.Should().ContainKey("desktop");
        result.ViewsByDevice["desktop"].Should().Be(2);
        result.ViewsByDevice.Should().ContainKey("mobile");
        result.ViewsByDevice["mobile"].Should().Be(1);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ShouldReturnViewsByCountry()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Country Page", "country-page");
        await SeedVisitAsync(page.Id, visitorId: "v1", country: "US");
        await SeedVisitAsync(page.Id, visitorId: "v2", country: "UK");

        // Act
        var result = await _service.GetAnalyticsAsync(page.Id);

        // Assert
        result.ViewsByCountry.Should().ContainKey("US");
        result.ViewsByCountry.Should().ContainKey("UK");
    }

    [Fact]
    public async Task GetAnalyticsAsync_ShouldReturnZeros_WhenNoVisits()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Empty Analytics", "empty-analytics");

        // Act
        var result = await _service.GetAnalyticsAsync(page.Id);

        // Assert
        result.TotalPageViews.Should().Be(0);
        result.UniqueVisitors.Should().Be(0);
        result.Conversions.Should().Be(0);
        result.ConversionRate.Should().Be(0);
    }

    #endregion

    #region GetBlocksAsync Tests

    [Fact]
    public async Task GetBlocksAsync_ShouldReturnOrderedBlocks()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Block Page", "block-page");
        await SeedBlockAsync(page.Id, LandingPageBlockType.Text, sortOrder: 2);
        await SeedBlockAsync(page.Id, LandingPageBlockType.Hero, sortOrder: 0);
        await SeedBlockAsync(page.Id, LandingPageBlockType.Button, sortOrder: 1);

        // Act
        var result = (await _service.GetBlocksAsync(page.Id)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].BlockType.Should().Be(LandingPageBlockType.Hero);
        result[1].BlockType.Should().Be(LandingPageBlockType.Button);
        result[2].BlockType.Should().Be(LandingPageBlockType.Text);
    }

    [Fact]
    public async Task GetBlocksAsync_ShouldExcludeDeletedBlocks()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Block Delete Page", "block-delete-page");
        var block = await SeedBlockAsync(page.Id, LandingPageBlockType.Text, sortOrder: 0);
        block.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetBlocksAsync(page.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBlocksAsync_ShouldReturnEmpty_WhenNoBlocks()
    {
        // Arrange
        var page = await SeedLandingPageAsync("No Blocks", "no-blocks");

        // Act
        var result = await _service.GetBlocksAsync(page.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateBlocksAsync Tests

    [Fact]
    public async Task UpdateBlocksAsync_ShouldReplaceExistingBlocks()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Update Blocks", "update-blocks");
        await SeedBlockAsync(page.Id, LandingPageBlockType.Text, 0);

        var newBlocks = new List<LandingPageBlock>
        {
            new LandingPageBlock { BlockType = LandingPageBlockType.Hero, ContentJson = "{\"heading\":\"New\"}" },
            new LandingPageBlock { BlockType = LandingPageBlockType.Button, ContentJson = "{\"text\":\"Click\"}" }
        };

        // Act
        var result = (await _service.UpdateBlocksAsync(page.Id, newBlocks)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].BlockType.Should().Be(LandingPageBlockType.Hero);
        result[0].SortOrder.Should().Be(0);
        result[1].BlockType.Should().Be(LandingPageBlockType.Button);
        result[1].SortOrder.Should().Be(1);
    }

    [Fact]
    public async Task UpdateBlocksAsync_ShouldUpdatePageTimestamp()
    {
        // Arrange
        var page = await SeedLandingPageAsync("Timestamp Page", "timestamp-page");
        var originalUpdatedAt = page.UpdatedAt ?? DateTime.UtcNow.AddMinutes(-1);
        await Task.Delay(50); // Ensure time difference

        var newBlocks = new List<LandingPageBlock>
        {
            new LandingPageBlock { BlockType = LandingPageBlockType.Text }
        };

        // Act
        await _service.UpdateBlocksAsync(page.Id, newBlocks);

        // Assert
        var updatedPage = await _dbContext.LandingPages.FindAsync(page.Id);
        updatedPage!.UpdatedAt.Should().NotBeNull();
        updatedPage.UpdatedAt!.Value.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region CompileToHtmlAsync Tests

    [Fact]
    public async Task CompileToHtmlAsync_ShouldReturnValidHtml()
    {
        // Arrange
        var page = await SeedLandingPageAsync("HTML Compile", "html-compile");

        // Act
        var result = await _service.CompileToHtmlAsync(page.Id);

        // Assert
        result.Should().Contain("<!DOCTYPE html>");
        result.Should().Contain("<html");
        result.Should().Contain("</html>");
        result.Should().Contain(page.Name);
    }

    [Fact]
    public async Task CompileToHtmlAsync_ShouldThrowArgumentException_WhenPageNotFound()
    {
        // Act
        var act = () => _service.CompileToHtmlAsync(999);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*999*not found*");
    }

    #endregion

    #region CreateVariantAsync Tests

    [Fact]
    public async Task CreateVariantAsync_ShouldSetABTestProperties()
    {
        // Arrange
        var original = await SeedLandingPageAsync("Original AB", "original-ab");

        // Act
        var result = await _service.CreateVariantAsync(original.Id, "Variant B", 50, userId: 1);

        // Assert
        result.ABTestVariant.Should().Be("Variant B");
        result.OriginalPageId.Should().Be(original.Id);
        result.ABTestTrafficPercentage.Should().Be(50);
        result.Status.Should().Be(LandingPageStatus.Draft);
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowArgumentException_WhenOriginalNotFound()
    {
        // Act
        var act = () => _service.CreateVariantAsync(999, "Variant", 50, userId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*999*not found*");
    }

    #endregion
}
