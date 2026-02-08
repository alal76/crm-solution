// CRM Solution - Customer Relationship Management System
// Landing Page Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LandingPageService
/// Covers: Landing page CRUD, form handling, conversion tracking
/// </summary>
public class LandingPageServiceTests
{
    private readonly Mock<IRepository<LandingPage>> _mockPageRepository;
    private readonly Mock<IRepository<FormSubmission>> _mockFormRepository;
    private readonly Mock<IRepository<Lead>> _mockLeadRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<LandingPageService>> _mockLogger;
    private readonly LandingPageService _service;

    public LandingPageServiceTests()
    {
        _mockPageRepository = new Mock<IRepository<LandingPage>>();
        _mockFormRepository = new Mock<IRepository<FormSubmission>>();
        _mockLeadRepository = new Mock<IRepository<Lead>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<LandingPageService>>();

        _service = new LandingPageService(
            _mockPageRepository.Object,
            _mockFormRepository.Object,
            _mockLeadRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidPage_ReturnsPage()
    {
        // Arrange
        var request = new CreateLandingPageDto
        {
            Name = "Product Launch",
            Slug = "product-launch",
            Content = "<html>...</html>"
        };

        _mockPageRepository.Setup(r => r.AddAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Product Launch");
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_ThrowsException()
    {
        // Arrange
        var existing = new LandingPage { Id = 1, Slug = "existing-slug" };
        var request = new CreateLandingPageDto { Slug = "existing-slug" };

        _mockPageRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LandingPage, bool>>>()))
            .ReturnsAsync(new List<LandingPage> { existing });

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_WithTemplate_AppliesTemplate()
    {
        // Arrange
        var request = new CreateLandingPageDto
        {
            Name = "Test Page",
            TemplateId = 1
        };

        _mockPageRepository.Setup(r => r.AddAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetByIdAsync_ExistingPage_ReturnsPage()
    {
        // Arrange
        var page = new LandingPage { Id = 1, Name = "Test Page" };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Page");
    }

    [Fact]
    public async Task GetBySlugAsync_ExistingSlug_ReturnsPage()
    {
        // Arrange
        var page = new LandingPage { Id = 1, Slug = "test-page" };

        _mockPageRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LandingPage, bool>>>()))
            .ReturnsAsync(new List<LandingPage> { page });

        // Act
        var result = await _service.GetBySlugAsync("test-page");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-page");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPages()
    {
        // Arrange
        var pages = new List<LandingPage>
        {
            new LandingPage { Id = 1, Name = "Page 1" },
            new LandingPage { Id = 2, Name = "Page 2" }
        };

        _mockPageRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(pages);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActivePagesAsync_ReturnsOnlyActive()
    {
        // Arrange
        var pages = new List<LandingPage>
        {
            new LandingPage { Id = 1, Status = LandingPageStatus.Published },
            new LandingPage { Id = 2, Status = LandingPageStatus.Draft }
        };

        _mockPageRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LandingPage, bool>>>()))
            .ReturnsAsync(pages.Where(p => p.Status == LandingPageStatus.Published).ToList());

        // Act
        var result = await _service.GetActivePagesAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ValidUpdate_UpdatesPage()
    {
        // Arrange
        var existing = new LandingPage { Id = 1, Name = "Old Name" };
        var updateDto = new UpdateLandingPageDto { Id = 1, Name = "New Name" };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _mockPageRepository.Setup(r => r.UpdateAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => p);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateLandingPageDto { Id = 999, Name = "Test" };

        _mockPageRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((LandingPage?)null);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingPage_DeletesPage()
    {
        // Arrange
        _mockPageRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Publish Tests

    [Fact]
    public async Task PublishAsync_DraftPage_PublishesPage()
    {
        // Arrange
        var page = new LandingPage { Id = 1, Status = LandingPageStatus.Draft };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        _mockPageRepository.Setup(r => r.UpdateAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => { p.Status = LandingPageStatus.Published; return p; });

        // Act
        var result = await _service.PublishAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UnpublishAsync_PublishedPage_UnpublishesPage()
    {
        // Arrange
        var page = new LandingPage { Id = 1, Status = LandingPageStatus.Published };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        _mockPageRepository.Setup(r => r.UpdateAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => { p.Status = LandingPageStatus.Draft; return p; });

        // Act
        var result = await _service.UnpublishAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Form Submission Tests

    [Fact]
    public async Task SubmitFormAsync_ValidSubmission_CreatesLead()
    {
        // Arrange
        var submission = new FormSubmissionDto
        {
            LandingPageId = 1,
            Data = new Dictionary<string, string>
            {
                { "email", "test@example.com" },
                { "name", "John Doe" }
            }
        };

        var page = new LandingPage { Id = 1, CreateLeadOnSubmission = true };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        _mockFormRepository.Setup(r => r.AddAsync(It.IsAny<FormSubmission>()))
            .ReturnsAsync((FormSubmission f) => { f.Id = 1; return f; });

        _mockLeadRepository.Setup(r => r.AddAsync(It.IsAny<Lead>()))
            .ReturnsAsync((Lead l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.SubmitFormAsync(submission);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetFormSubmissionsAsync_ValidPage_ReturnsSubmissions()
    {
        // Arrange
        var submissions = new List<FormSubmission>
        {
            new FormSubmission { Id = 1, LandingPageId = 1 },
            new FormSubmission { Id = 2, LandingPageId = 1 }
        };

        _mockFormRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FormSubmission, bool>>>()))
            .ReturnsAsync(submissions);

        // Act
        var result = await _service.GetFormSubmissionsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task RecordVisitAsync_ValidPage_RecordsVisit()
    {
        // Arrange
        var page = new LandingPage { Id = 1, ViewCount = 100 };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        _mockPageRepository.Setup(r => r.UpdateAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => { p.ViewCount++; return p; });

        // Act
        await _service.RecordVisitAsync(1);

        // Assert
        _mockPageRepository.Verify(r => r.UpdateAsync(It.Is<LandingPage>(p => p.ViewCount == 101)), Times.Once);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ValidPage_ReturnsAnalytics()
    {
        // Arrange
        var page = new LandingPage { Id = 1, ViewCount = 1000 };
        var submissions = new List<FormSubmission>
        {
            new FormSubmission { LandingPageId = 1 },
            new FormSubmission { LandingPageId = 1 }
        };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        _mockFormRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FormSubmission, bool>>>()))
            .ReturnsAsync(submissions);

        // Act
        var result = await _service.GetAnalyticsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Views.Should().Be(1000);
        result.Conversions.Should().Be(2);
    }

    [Fact]
    public async Task GetConversionRateAsync_ValidPage_CalculatesRate()
    {
        // Arrange
        var page = new LandingPage { Id = 1, ViewCount = 100 };
        var submissions = new List<FormSubmission>
        {
            new FormSubmission { LandingPageId = 1 },
            new FormSubmission { LandingPageId = 1 }
        };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page);

        _mockFormRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FormSubmission, bool>>>()))
            .ReturnsAsync(submissions);

        // Act
        var result = await _service.GetConversionRateAsync(1);

        // Assert
        result.Should().Be(2.0m); // 2 conversions / 100 views = 2%
    }

    #endregion

    #region Clone Tests

    [Fact]
    public async Task CloneAsync_ExistingPage_ClonesPage()
    {
        // Arrange
        var original = new LandingPage
        {
            Id = 1,
            Name = "Original",
            Slug = "original",
            Content = "<html>...</html>"
        };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(original);

        _mockPageRepository.Setup(r => r.AddAsync(It.IsAny<LandingPage>()))
            .ReturnsAsync((LandingPage p) => { p.Id = 2; return p; });

        // Act
        var result = await _service.CloneAsync(1, "Clone");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("Clone");
    }

    #endregion

    #region A/B Testing Tests

    [Fact]
    public async Task CreateABTestAsync_ValidPages_CreatesTest()
    {
        // Arrange
        var page1 = new LandingPage { Id = 1, Name = "Version A" };
        var page2 = new LandingPage { Id = 2, Name = "Version B" };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page1);

        _mockPageRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(page2);

        // Act
        var result = await _service.CreateABTestAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetABTestWinnerAsync_ValidTest_ReturnsWinner()
    {
        // Arrange
        var page1 = new LandingPage { Id = 1, ViewCount = 100 };
        var page2 = new LandingPage { Id = 2, ViewCount = 100 };

        _mockPageRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(page1);

        _mockPageRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(page2);

        _mockFormRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FormSubmission, bool>>>()))
            .ReturnsAsync(new List<FormSubmission>
            {
                new FormSubmission { LandingPageId = 1 },
                new FormSubmission { LandingPageId = 2 },
                new FormSubmission { LandingPageId = 2 }
            });

        // Act
        var result = await _service.GetABTestWinnerAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var pages = new List<LandingPage>
        {
            new LandingPage { Status = LandingPageStatus.Published, ViewCount = 500 },
            new LandingPage { Status = LandingPageStatus.Draft, ViewCount = 100 }
        };

        _mockPageRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(pages);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalPages.Should().Be(2);
        result.PublishedPages.Should().Be(1);
        result.TotalViews.Should().Be(600);
    }

    #endregion
}

// Supporting classes for tests
public enum LandingPageStatus
{
    Draft,
    Published,
    Archived
}

public class CreateLandingPageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public int? TemplateId { get; set; }
}

public class UpdateLandingPageDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Content { get; set; }
}

public class FormSubmissionDto
{
    public int LandingPageId { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}
