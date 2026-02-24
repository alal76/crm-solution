// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for MarketingCampaignService.
/// Covers CRUD operations, active campaign filtering, validation, metrics, and deletion.
/// </summary>
public class CampaignServiceTests
{
    private readonly Mock<IRepository<MarketingCampaign>> _mockRepository;
    private readonly Mock<IRepository<CampaignMetric>> _mockMetricRepository;
    private readonly Mock<IRepository<EntityTag>> _mockEntityTagRepository;
    private readonly Mock<IRepository<CustomField>> _mockCustomFieldRepository;
    private readonly NormalizationService _normalizationService;
    private readonly Mock<IDuplicateDetectionService> _mockDuplicateDetection;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<MarketingCampaignService>> _mockLogger;
    private readonly MarketingCampaignService _service;

    private readonly List<MarketingCampaign> _campaigns;
    private readonly List<CampaignMetric> _metrics;

    public CampaignServiceTests()
    {
        _mockRepository = new Mock<IRepository<MarketingCampaign>>();
        _mockMetricRepository = new Mock<IRepository<CampaignMetric>>();
        _mockEntityTagRepository = new Mock<IRepository<EntityTag>>();
        _mockCustomFieldRepository = new Mock<IRepository<CustomField>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<MarketingCampaignService>>();
        _mockDuplicateDetection = new Mock<IDuplicateDetectionService>();

        // NormalizationService is a concrete class with non-virtual methods;
        // use a real instance with mocked DbContext (empty DbSets)
        var emptyEntityTags = MockDbSetFactory.CreateMockDbSet(new List<EntityTag>());
        var emptyCustomFields = MockDbSetFactory.CreateMockDbSet(new List<CustomField>());
        var emptyContactInfoLinks = MockDbSetFactory.CreateMockDbSet(new List<ContactInfoLink>());
        _mockDbContext.Setup(c => c.EntityTags).Returns(emptyEntityTags.Object);
        _mockDbContext.Setup(c => c.CustomFields).Returns(emptyCustomFields.Object);
        _mockDbContext.Setup(c => c.ContactInfoLinks).Returns(emptyContactInfoLinks.Object);
        _normalizationService = new NormalizationService(_mockDbContext.Object);

        _campaigns = new List<MarketingCampaign>();
        _metrics = new List<CampaignMetric>();

        SetupDefaults();

        _service = new MarketingCampaignService(
            _mockRepository.Object,
            _mockMetricRepository.Object,
            _mockEntityTagRepository.Object,
            _mockCustomFieldRepository.Object,
            _normalizationService,
            _mockDuplicateDetection.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    private void SetupDefaults()
    {
        _mockDuplicateDetection
            .Setup(d => d.CheckForDuplicatesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string?>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());
    }

    // ========================================================================
    // Constructor Tests
    // ========================================================================

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    // ========================================================================
    // GetCampaignByIdAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetCampaignByIdAsync_ShouldReturnCampaign_WhenFound()
    {
        // Arrange
        var campaign = CreateTestCampaign(1, "Test Campaign");
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(campaign);

        // Act
        var result = await _service.GetCampaignByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Campaign");
    }

    [Fact]
    public async Task GetCampaignByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((MarketingCampaign?)null);

        // Act
        var result = await _service.GetCampaignByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetAllCampaignsAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetAllCampaignsAsync_ShouldReturnAllCampaigns()
    {
        // Arrange
        var campaigns = new List<MarketingCampaign>
        {
            CreateTestCampaign(1, "Campaign A"),
            CreateTestCampaign(2, "Campaign B")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(campaigns);

        // Act
        var result = await _service.GetAllCampaignsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllCampaignsAsync_ShouldReturnEmpty_WhenNoCampaigns()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<MarketingCampaign>());

        // Act
        var result = await _service.GetAllCampaignsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // GetActiveCampaignsAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetActiveCampaignsAsync_ShouldReturnOnlyActiveCampaigns()
    {
        // Arrange
        var activeCampaign = CreateTestCampaign(1, "Active Campaign");
        activeCampaign.Status = CampaignStatus.Active;

        _mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Func<MarketingCampaign, bool>>()))
            .ReturnsAsync(new List<MarketingCampaign> { activeCampaign });

        // Act
        var result = await _service.GetActiveCampaignsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveCampaignsAsync_ShouldReturnEmpty_WhenNoActiveCampaigns()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Func<MarketingCampaign, bool>>()))
            .ReturnsAsync(new List<MarketingCampaign>());

        // Act
        var result = await _service.GetActiveCampaignsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // CreateCampaignAsync Tests
    // ========================================================================

    [Fact]
    public async Task CreateCampaignAsync_ShouldCreateAndReturnId()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = "New Campaign",
            CampaignType = (int)CampaignType.Email
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<MarketingCampaign>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var id = await _service.CreateCampaignAsync(dto);

        // Assert
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<MarketingCampaign>()), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCampaignAsync_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = "",
            CampaignType = (int)CampaignType.Email
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateCampaignAsync(dto));
    }

    [Fact]
    public async Task CreateCampaignAsync_ShouldThrow_WhenNameIsWhitespace()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = "   ",
            CampaignType = (int)CampaignType.Email
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateCampaignAsync(dto));
    }

    // ========================================================================
    // UpdateCampaignAsync Tests
    // ========================================================================

    [Fact]
    public async Task UpdateCampaignAsync_ShouldUpdate_WhenCampaignExists()
    {
        // Arrange
        var campaign = CreateTestCampaign(1, "Existing Campaign");
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(campaign);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var updateDto = new UpdateCampaignDto { Name = "Updated Campaign" };

        // Act
        await _service.UpdateCampaignAsync(1, updateDto);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()), Times.Once);
        _mockRepository.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCampaignAsync_ShouldThrow_WhenCampaignNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((MarketingCampaign?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateCampaignAsync(999, new UpdateCampaignDto { Name = "X" }));
    }

    // ========================================================================
    // DeleteCampaignAsync Tests
    // ========================================================================

    [Fact]
    public async Task DeleteCampaignAsync_ShouldSoftDelete_WhenCampaignExists()
    {
        // Arrange
        var campaign = CreateTestCampaign(1, "Delete Me");
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(campaign);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteCampaignAsync(1);

        // Assert
        campaign.IsDeleted.Should().BeTrue();
        campaign.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<MarketingCampaign>(c => c.IsDeleted)), Times.Once);
    }

    [Fact]
    public async Task DeleteCampaignAsync_ShouldThrow_WhenCampaignNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((MarketingCampaign?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteCampaignAsync(999));
    }

    // ========================================================================
    // AddCampaignMetricAsync Tests
    // ========================================================================

    [Fact]
    public async Task AddCampaignMetricAsync_ShouldAddMetricSuccessfully()
    {
        // Arrange
        var metric = new CampaignMetric
        {
            CampaignId = 1,
            MetricName = "Opens",
            MetricValue = 150,
            RecordedDate = DateTime.UtcNow
        };

        _mockMetricRepository.Setup(r => r.AddAsync(It.IsAny<CampaignMetric>())).Returns(Task.CompletedTask);
        _mockMetricRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.AddCampaignMetricAsync(metric);

        // Assert
        _mockMetricRepository.Verify(r => r.AddAsync(metric), Times.Once);
        _mockMetricRepository.Verify(r => r.SaveAsync(), Times.Once);
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private static MarketingCampaign CreateTestCampaign(int id, string name)
    {
        return new MarketingCampaign
        {
            Id = id,
            Name = name,
            Status = CampaignStatus.Draft,
            CampaignType = CampaignType.Email,
            Budget = 10000m,
            CreatedAt = DateTime.UtcNow
        };
    }
}
