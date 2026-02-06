// CRM Solution - Customer Relationship Management System
// Marketing Campaign Service Unit Tests

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
/// Unit tests for MarketingCampaignService
/// Covers: Campaign CRUD, execution, analytics, A/B testing
/// </summary>
public class MarketingCampaignServiceTests
{
    private readonly Mock<IRepository<MarketingCampaign>> _mockCampaignRepository;
    private readonly Mock<IRepository<CampaignRecipient>> _mockRecipientRepository;
    private readonly Mock<IRepository<CampaignMetrics>> _mockMetricsRepository;
    private readonly Mock<IRepository<CampaignABTest>> _mockABTestRepository;
    private readonly Mock<IRepository<EmailTemplate>> _mockTemplateRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<MarketingCampaignService>> _mockLogger;
    private readonly MarketingCampaignService _service;

    public MarketingCampaignServiceTests()
    {
        _mockCampaignRepository = new Mock<IRepository<MarketingCampaign>>();
        _mockRecipientRepository = new Mock<IRepository<CampaignRecipient>>();
        _mockMetricsRepository = new Mock<IRepository<CampaignMetrics>>();
        _mockABTestRepository = new Mock<IRepository<CampaignABTest>>();
        _mockTemplateRepository = new Mock<IRepository<EmailTemplate>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<MarketingCampaignService>>();

        _service = new MarketingCampaignService(
            _mockCampaignRepository.Object,
            _mockRecipientRepository.Object,
            _mockMetricsRepository.Object,
            _mockABTestRepository.Object,
            _mockTemplateRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidCampaign_ReturnsCampaign()
    {
        // Arrange
        var request = new CreateCampaignDto
        {
            Name = "Summer Sale",
            Type = CampaignType.Email,
            Subject = "Summer Sale - 50% Off!",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        _mockCampaignRepository.Setup(r => r.AddAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Summer Sale");
    }

    [Fact]
    public async Task CreateAsync_WithTemplate_AssociatesTemplate()
    {
        // Arrange
        var template = new EmailTemplate { Id = 1, Name = "Sale Template" };
        var request = new CreateCampaignDto
        {
            Name = "New Campaign",
            Type = CampaignType.Email,
            TemplateId = 1
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(template);

        _mockCampaignRepository.Setup(r => r.AddAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var existing = new MarketingCampaign { Id = 1, Name = "Existing Campaign" };
        var request = new CreateCampaignDto
        {
            Name = "Existing Campaign",
            Type = CampaignType.Email
        };

        _mockCampaignRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MarketingCampaign, bool>>>()))
            .ReturnsAsync(new List<MarketingCampaign> { existing });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(request));
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetByIdAsync_ExistingCampaign_ReturnsCampaign()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Id = 1,
            Name = "Test Campaign",
            Status = CampaignStatus.Draft
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Campaign");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCampaigns()
    {
        // Arrange
        var campaigns = new List<MarketingCampaign>
        {
            new MarketingCampaign { Id = 1, Name = "Campaign 1" },
            new MarketingCampaign { Id = 2, Name = "Campaign 2" }
        };

        _mockCampaignRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsFilteredCampaigns()
    {
        // Arrange
        var campaigns = new List<MarketingCampaign>
        {
            new MarketingCampaign { Id = 1, Status = CampaignStatus.Active }
        };

        _mockCampaignRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MarketingCampaign, bool>>>()))
            .ReturnsAsync(campaigns);

        // Act
        var result = await _service.GetByStatusAsync(CampaignStatus.Active);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsFilteredCampaigns()
    {
        // Arrange
        var campaigns = new List<MarketingCampaign>
        {
            new MarketingCampaign { Id = 1, Type = CampaignType.Email }
        };

        _mockCampaignRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MarketingCampaign, bool>>>()))
            .ReturnsAsync(campaigns);

        // Act
        var result = await _service.GetByTypeAsync(CampaignType.Email);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ValidCampaign_UpdatesCampaign()
    {
        // Arrange
        var existing = new MarketingCampaign { Id = 1, Name = "Old Name", Status = CampaignStatus.Draft };
        var updateDto = new UpdateCampaignDto { Id = 1, Name = "New Name" };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _mockCampaignRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => c);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateAsync_ActiveCampaign_ThrowsException()
    {
        // Arrange
        var existing = new MarketingCampaign { Id = 1, Status = CampaignStatus.Active };
        var updateDto = new UpdateCampaignDto { Id = 1, Name = "New Name" };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(updateDto));
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task ActivateAsync_DraftCampaign_ActivatesCampaign()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1, Status = CampaignStatus.Draft };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockCampaignRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.Status = CampaignStatus.Active; return c; });

        // Act
        var result = await _service.ActivateAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PauseAsync_ActiveCampaign_PausesCampaign()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1, Status = CampaignStatus.Active };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockCampaignRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.Status = CampaignStatus.Paused; return c; });

        // Act
        var result = await _service.PauseAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_ActiveCampaign_CompletesCampaign()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1, Status = CampaignStatus.Active };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockCampaignRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.Status = CampaignStatus.Completed; return c; });

        // Act
        var result = await _service.CompleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Recipient Management Tests

    [Fact]
    public async Task AddRecipientsAsync_ValidRecipients_AddsRecipients()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1 };
        var recipients = new List<AddRecipientDto>
        {
            new AddRecipientDto { Email = "test1@example.com" },
            new AddRecipientDto { Email = "test2@example.com" }
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockRecipientRepository.Setup(r => r.AddAsync(It.IsAny<CampaignRecipient>()))
            .ReturnsAsync((CampaignRecipient r) => { r.Id = 1; return r; });

        // Act
        var result = await _service.AddRecipientsAsync(1, recipients);

        // Assert
        result.AddedCount.Should().Be(2);
    }

    [Fact]
    public async Task RemoveRecipientAsync_ExistingRecipient_RemovesRecipient()
    {
        // Arrange
        _mockRecipientRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RemoveRecipientAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetRecipientsAsync_ReturnsRecipients()
    {
        // Arrange
        var recipients = new List<CampaignRecipient>
        {
            new CampaignRecipient { Id = 1, CampaignId = 1, Email = "test@example.com" }
        };

        _mockRecipientRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CampaignRecipient, bool>>>()))
            .ReturnsAsync(recipients);

        // Act
        var result = await _service.GetRecipientsAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Metrics Tests

    [Fact]
    public async Task GetMetricsAsync_ValidCampaign_ReturnsMetrics()
    {
        // Arrange
        var metrics = new CampaignMetrics
        {
            Id = 1,
            CampaignId = 1,
            Sent = 1000,
            Delivered = 950,
            Opened = 400,
            Clicked = 100
        };

        _mockMetricsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CampaignMetrics, bool>>>()))
            .ReturnsAsync(new List<CampaignMetrics> { metrics });

        // Act
        var result = await _service.GetMetricsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Sent.Should().Be(1000);
        result.OpenRate.Should().BeApproximately(42.1, 0.1);
    }

    [Fact]
    public async Task RecordOpenAsync_ValidCampaign_RecordsOpen()
    {
        // Arrange
        var metrics = new CampaignMetrics { Id = 1, CampaignId = 1, Opened = 0 };

        _mockMetricsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CampaignMetrics, bool>>>()))
            .ReturnsAsync(new List<CampaignMetrics> { metrics });

        _mockMetricsRepository.Setup(r => r.UpdateAsync(It.IsAny<CampaignMetrics>()))
            .ReturnsAsync((CampaignMetrics m) => { m.Opened++; return m; });

        // Act
        var result = await _service.RecordOpenAsync(1, "test@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordClickAsync_ValidLink_RecordsClick()
    {
        // Arrange
        var metrics = new CampaignMetrics { Id = 1, CampaignId = 1, Clicked = 0 };

        _mockMetricsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CampaignMetrics, bool>>>()))
            .ReturnsAsync(new List<CampaignMetrics> { metrics });

        _mockMetricsRepository.Setup(r => r.UpdateAsync(It.IsAny<CampaignMetrics>()))
            .ReturnsAsync((CampaignMetrics m) => { m.Clicked++; return m; });

        // Act
        var result = await _service.RecordClickAsync(1, "test@example.com", "https://example.com");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region A/B Test Tests

    [Fact]
    public async Task CreateABTestAsync_ValidTest_CreatesTest()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1 };
        var request = new CreateABTestDto
        {
            CampaignId = 1,
            VariantASubject = "Subject A",
            VariantBSubject = "Subject B",
            TestPercentage = 20
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockABTestRepository.Setup(r => r.AddAsync(It.IsAny<CampaignABTest>()))
            .ReturnsAsync((CampaignABTest t) => { t.Id = 1; return t; });

        // Act
        var result = await _service.CreateABTestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetABTestResultsAsync_CompletedTest_ReturnsResults()
    {
        // Arrange
        var test = new CampaignABTest
        {
            Id = 1,
            CampaignId = 1,
            VariantASent = 100,
            VariantAOpened = 40,
            VariantBSent = 100,
            VariantBOpened = 50
        };

        _mockABTestRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CampaignABTest, bool>>>()))
            .ReturnsAsync(new List<CampaignABTest> { test });

        // Act
        var result = await _service.GetABTestResultsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Winner.Should().Be("B");
    }

    #endregion

    #region Schedule Tests

    [Fact]
    public async Task ScheduleAsync_ValidSchedule_SchedulesCampaign()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1, Status = CampaignStatus.Draft };
        var scheduledDate = DateTime.UtcNow.AddDays(1);

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockCampaignRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.ScheduledDate = scheduledDate; return c; });

        // Act
        var result = await _service.ScheduleAsync(1, scheduledDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelScheduleAsync_ScheduledCampaign_CancelsSchedule()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Id = 1,
            Status = CampaignStatus.Scheduled,
            ScheduledDate = DateTime.UtcNow.AddDays(1)
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(campaign);

        _mockCampaignRepository.Setup(r => r.UpdateAsync(It.IsAny<MarketingCampaign>()))
            .ReturnsAsync((MarketingCampaign c) => { c.Status = CampaignStatus.Draft; return c; });

        // Act
        var result = await _service.CancelScheduleAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var campaigns = new List<MarketingCampaign>
        {
            new MarketingCampaign { Id = 1, Status = CampaignStatus.Active },
            new MarketingCampaign { Id = 2, Status = CampaignStatus.Completed },
            new MarketingCampaign { Id = 3, Status = CampaignStatus.Draft }
        };

        _mockCampaignRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalCampaigns.Should().Be(3);
        result.ActiveCount.Should().Be(1);
        result.CompletedCount.Should().Be(1);
    }

    #endregion
}

// Supporting classes for tests
public enum CampaignType
{
    Email,
    SMS,
    Social,
    WebPush
}

public enum CampaignStatus
{
    Draft,
    Scheduled,
    Active,
    Paused,
    Completed,
    Cancelled
}

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public CampaignType Type { get; set; }
    public string? Subject { get; set; }
    public int? TemplateId { get; set; }
    public DateTime? StartDate { get; set; }
}

public class UpdateCampaignDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Subject { get; set; }
}

public class AddRecipientDto
{
    public string Email { get; set; } = string.Empty;
    public int? ContactId { get; set; }
}

public class CreateABTestDto
{
    public int CampaignId { get; set; }
    public string VariantASubject { get; set; } = string.Empty;
    public string VariantBSubject { get; set; } = string.Empty;
    public int TestPercentage { get; set; }
}
