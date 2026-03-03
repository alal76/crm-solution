// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LeadService.
/// Covers CRUD operations, conversion, status filtering, search, and owner assignment.
/// </summary>
public class LeadServiceTests : ServiceTestFixtureBase<LeadService>
{    private readonly Mock<IEntityEventDispatcher> _mockEventDispatcher;    private readonly Mock<IDuplicateDetectionService> _mockDuplicateDetection;
    private readonly LeadService _service;

    private readonly List<Lead> _leads;
    private readonly List<Opportunity> _opportunities;

    public LeadServiceTests()
    {        _mockEventDispatcher = new Mock<IEntityEventDispatcher>();        _mockDuplicateDetection = new Mock<IDuplicateDetectionService>();

        _leads = new List<Lead>();
        _opportunities = new List<Opportunity>();

        SetupMockDbSets();

        _service = new LeadService(
            MockContext.Object,
            _mockEventDispatcher.Object,
            MockLogger.Object,
            _mockDuplicateDetection.Object);
    }

    private void SetupMockDbSets()
    {
        var mockLeads = MockDbSetFactory.CreateMockDbSet(_leads);
        var mockOpportunities = MockDbSetFactory.CreateMockDbSet(_opportunities);

        MockContext.Setup(c => c.Set<Lead>()).Returns(mockLeads.Object);
        MockContext.Setup(c => c.Set<Opportunity>()).Returns(mockOpportunities.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // DuplicateDetection won't find duplicates by default
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
        // Act & Assert — service was created in constructor, just verify it's not null
        _service.Should().NotBeNull();
    }

    // ========================================================================
    // GetByIdAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLead_WhenLeadExists()
    {
        // Arrange
        _leads.Add(new Lead
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            CompanyName = "Acme Corp",
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Web,
            Score = 75,
            CreatedAt = DateTime.UtcNow
        });
        RefreshMockDbSet();

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenLeadNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenLeadIsDeleted()
    {
        // Arrange
        _leads.Add(new Lead
        {
            Id = 1,
            FirstName = "Deleted",
            LastName = "Lead",
            Email = "deleted@example.com",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        });
        RefreshMockDbSet();

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetAllAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 1; i <= 30; i++)
        {
            _leads.Add(new Lead
            {
                Id = i,
                FirstName = $"Lead{i}",
                LastName = "Test",
                Email = $"lead{i}@example.com",
                Status = LeadLifecycleStatus.New,
                Source = LeadSource.Web,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        RefreshMockDbSet();

        // Act
        var (items, totalCount, page, pageSize, totalPages) = await _service.GetAllAsync(1, 10);

        // Assert
        totalCount.Should().Be(30);
        pageSize.Should().Be(10);
        page.Should().Be(1);
        totalPages.Should().Be(3);
        items.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedLeads()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "Active", LastName = "Lead", Email = "a@t.com", CreatedAt = DateTime.UtcNow });
        _leads.Add(new Lead { Id = 2, FirstName = "Deleted", LastName = "Lead", Email = "b@t.com", IsDeleted = true, CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        var (items, totalCount, _, _, _) = await _service.GetAllAsync(1, 25);

        // Assert
        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
    }

    // ========================================================================
    // CreateAsync Tests
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldAddLeadAndReturnId()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            CompanyName = "Tech Inc"
        };

        // Act
        var id = await _service.CreateAsync(lead);

        // Assert
        _leads.Should().HaveCount(1);
        lead.Status.Should().Be(LeadLifecycleStatus.New);
        lead.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetStatusToNew()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "Test",
            LastName = "Lead",
            Email = "test@example.com",
            Status = LeadLifecycleStatus.Qualified // should be overridden
        };

        // Act
        await _service.CreateAsync(lead);

        // Assert
        lead.Status.Should().Be(LeadLifecycleStatus.New);
    }

    [Fact]
    public async Task CreateAsync_ShouldDispatchCreateEvent()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "Event",
            LastName = "Test",
            Email = "event@example.com"
        };

        // Act
        await _service.CreateAsync(lead);

        // Assert
        _mockEventDispatcher.Verify(
            d => d.DispatchEntityEventAsync("Lead", It.IsAny<int>(), WorkflowTriggerType.OnCreate,
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ========================================================================
    // UpdateAsync Tests
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenLeadExists()
    {
        // Arrange
        _leads.Add(new Lead
        {
            Id = 1,
            FirstName = "Original",
            LastName = "Name",
            Email = "orig@example.com",
            CreatedAt = DateTime.UtcNow
        });
        RefreshMockDbSet();

        // Act
        var result = await _service.UpdateAsync(1, lead => { lead.FirstName = "Updated"; });

        // Assert
        result.Should().BeTrue();
        _leads[0].FirstName.Should().Be("Updated");
        _leads[0].UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenLeadNotFound()
    {
        // Act
        var result = await _service.UpdateAsync(999, _ => { });

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldDispatchUpdateEvent()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "Test", LastName = "Lead", Email = "t@t.com", CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        await _service.UpdateAsync(1, l => l.FirstName = "Changed");

        // Assert
        _mockEventDispatcher.Verify(
            d => d.DispatchEntityEventAsync("Lead", 1, WorkflowTriggerType.OnUpdate,
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ========================================================================
    // DeleteAsync Tests
    // ========================================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenLeadExists()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "Delete", LastName = "Me", Email = "del@t.com", CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _leads[0].IsDeleted.Should().BeTrue();
        _leads[0].UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenLeadNotFound()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDispatchDeleteEvent()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "Del", LastName = "Test", Email = "d@t.com", CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _mockEventDispatcher.Verify(
            d => d.DispatchEntityEventAsync("Lead", 1, WorkflowTriggerType.OnDelete,
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ========================================================================
    // ConvertAsync Tests
    // ========================================================================

    [Fact]
    public async Task ConvertAsync_ShouldConvertLeadToOpportunity()
    {
        // Arrange
        _leads.Add(new Lead
        {
            Id = 1,
            FirstName = "Convert",
            LastName = "Me",
            Email = "convert@t.com",
            CompanyName = "Acme",
            Status = LeadLifecycleStatus.Qualified,
            OwnerId = 5,
            CreatedAt = DateTime.UtcNow
        });
        RefreshMockDbSet();

        // Act
        var (opportunityId, leadId) = await _service.ConvertAsync(1, "New Opportunity", null, 50000m, null);

        // Assert
        leadId.Should().Be(1);
        _leads[0].Status.Should().Be(LeadLifecycleStatus.Converted);
        _opportunities.Should().HaveCount(1);
        _opportunities[0].Name.Should().Be("New Opportunity");
        _opportunities[0].Amount.Should().Be(50000m);
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrow_WhenLeadNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConvertAsync(999, null, null, null, null));
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrow_WhenLeadAlreadyConverted()
    {
        // Arrange
        _leads.Add(new Lead
        {
            Id = 1,
            FirstName = "Already",
            LastName = "Converted",
            Email = "ac@t.com",
            Status = LeadLifecycleStatus.Converted,
            CreatedAt = DateTime.UtcNow
        });
        RefreshMockDbSet();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConvertAsync(1, null, null, null, null));
    }

    [Fact]
    public async Task ConvertAsync_ShouldUseDefaultName_WhenNoNameProvided()
    {
        // Arrange
        _leads.Add(new Lead
        {
            Id = 1,
            FirstName = "Test",
            LastName = "Lead",
            Email = "t@t.com",
            CompanyName = "TestCo",
            Status = LeadLifecycleStatus.New,
            CreatedAt = DateTime.UtcNow
        });
        RefreshMockDbSet();

        // Act
        await _service.ConvertAsync(1, null, null, null, null);

        // Assert
        _opportunities[0].Name.Should().Contain("TestCo");
    }

    // ========================================================================
    // GetByStatusAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnLeadsMatchingStatus()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "New1", LastName = "L", Email = "n1@t.com", Status = LeadLifecycleStatus.New, Source = LeadSource.Web, CreatedAt = DateTime.UtcNow });
        _leads.Add(new Lead { Id = 2, FirstName = "Qual1", LastName = "L", Email = "q1@t.com", Status = LeadLifecycleStatus.Qualified, Source = LeadSource.Web, CreatedAt = DateTime.UtcNow });
        _leads.Add(new Lead { Id = 3, FirstName = "New2", LastName = "L", Email = "n2@t.com", Status = LeadLifecycleStatus.New, Source = LeadSource.Web, CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        var result = await _service.GetByStatusAsync(LeadLifecycleStatus.New);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnEmpty_WhenNoLeadsMatchStatus()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "New", LastName = "L", Email = "n@t.com", Status = LeadLifecycleStatus.New, Source = LeadSource.Web, CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        var result = await _service.GetByStatusAsync(LeadLifecycleStatus.Converted);

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // AssignOwnerAsync Tests
    // ========================================================================

    [Fact]
    public async Task AssignOwnerAsync_ShouldReturnTrue_WhenLeadExists()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "Assign", LastName = "Test", Email = "a@t.com", OwnerId = 1, CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        var result = await _service.AssignOwnerAsync(1, 42);

        // Assert
        result.Should().BeTrue();
        _leads[0].OwnerId.Should().Be(42);
    }

    [Fact]
    public async Task AssignOwnerAsync_ShouldReturnFalse_WhenLeadNotFound()
    {
        // Act
        var result = await _service.AssignOwnerAsync(999, 42);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // SearchAsync Tests
    // ========================================================================

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingLeads()
    {
        // Arrange
        _leads.Add(new Lead { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@acme.com", CompanyName = "Acme", CreatedAt = DateTime.UtcNow });
        _leads.Add(new Lead { Id = 2, FirstName = "Bob", LastName = "Jones", Email = "bob@other.com", CompanyName = "Other", CreatedAt = DateTime.UtcNow });
        RefreshMockDbSet();

        // Act
        var result = await _service.SearchAsync("Alice");

        // Assert
        result.Should().HaveCount(1);
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private void RefreshMockDbSet()
    {
        var mockLeads = MockDbSetFactory.CreateMockDbSet(_leads);
        var mockOpportunities = MockDbSetFactory.CreateMockDbSet(_opportunities);
        MockContext.Setup(c => c.Set<Lead>()).Returns(mockLeads.Object);
        MockContext.Setup(c => c.Set<Opportunity>()).Returns(mockOpportunities.Object);
    }

    // ========================================================================
    // GetSourceAnalyticsAsync Tests (TODO-CRM002-03)
    // ========================================================================

    [Fact]
    public async Task GetSourceAnalyticsAsync_ShouldReturnGroupedBySource_WhenLeadsExist()
    {
        // Arrange
        _leads.AddRange(new[]
        {
            new Lead { Id = 1, Source = LeadSource.Web, Status = LeadLifecycleStatus.New, Score = 60, IsDeleted = false },
            new Lead { Id = 2, Source = LeadSource.Web, Status = LeadLifecycleStatus.Converted, Score = 80, IsDeleted = false },
            new Lead { Id = 3, Source = LeadSource.Referral, Status = LeadLifecycleStatus.Qualified, Score = 90, IsDeleted = false },
            new Lead { Id = 4, Source = LeadSource.Referral, Status = LeadLifecycleStatus.Converted, Score = 70, IsDeleted = false }
        });
        RefreshMockDbSet();

        // Act
        var result = (await _service.GetSourceAnalyticsAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        var web = result.First(r => r.Source == "Web");
        web.TotalLeads.Should().Be(2);
        web.ConvertedLeads.Should().Be(1);
        web.ConversionRate.Should().Be(50m);
    }

    [Fact]
    public async Task GetSourceAnalyticsAsync_ShouldExcludeDeletedLeads()
    {
        // Arrange
        _leads.AddRange(new[]
        {
            new Lead { Id = 1, Source = LeadSource.Web, Status = LeadLifecycleStatus.Converted, IsDeleted = false },
            new Lead { Id = 2, Source = LeadSource.Web, Status = LeadLifecycleStatus.Converted, IsDeleted = true }
        });
        RefreshMockDbSet();

        // Act
        var result = (await _service.GetSourceAnalyticsAsync()).ToList();

        // Assert: only the non-deleted lead should appear
        result.Should().HaveCount(1);
        result[0].TotalLeads.Should().Be(1);
    }

    [Fact]
    public async Task GetSourceAnalyticsAsync_ShouldReturnEmptyList_WhenNoLeads()
    {
        // Arrange — _leads is empty
        RefreshMockDbSet();

        // Act
        var result = await _service.GetSourceAnalyticsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSourceAnalyticsAsync_ShouldComputeConversionRateCorrectly()
    {
        // Arrange — 3 leads from same source, 1 converted → 33.33%
        _leads.AddRange(new[]
        {
            new Lead { Id = 1, Source = LeadSource.Manual, Status = LeadLifecycleStatus.New, Score = 50, IsDeleted = false },
            new Lead { Id = 2, Source = LeadSource.Manual, Status = LeadLifecycleStatus.Qualified, Score = 55, IsDeleted = false },
            new Lead { Id = 3, Source = LeadSource.Manual, Status = LeadLifecycleStatus.Converted, Score = 75, IsDeleted = false }
        });
        RefreshMockDbSet();

        // Act
        var result = (await _service.GetSourceAnalyticsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalLeads.Should().Be(3);
        result[0].ConvertedLeads.Should().Be(1);
        result[0].ConversionRate.Should().BeApproximately(33.33m, 0.01m);
    }

    // ========================================================================
    // GetAttributionAnalyticsAsync Tests (TODO-CRM002-03)
    // ========================================================================

    [Fact]
    public async Task GetAttributionAnalyticsAsync_ShouldGroupByUtmTriple()
    {
        // Arrange
        _leads.AddRange(new[]
        {
            new Lead { Id = 1, UtmSource = "google", UtmMedium = "cpc", UtmCampaign = "q1", Status = LeadLifecycleStatus.Converted, Score = 70, IsDeleted = false },
            new Lead { Id = 2, UtmSource = "google", UtmMedium = "cpc", UtmCampaign = "q1", Status = LeadLifecycleStatus.New, Score = 50, IsDeleted = false },
            new Lead { Id = 3, UtmSource = "linkedin", UtmMedium = "organic", UtmCampaign = "brand", Status = LeadLifecycleStatus.Qualified, Score = 65, IsDeleted = false }
        });
        RefreshMockDbSet();

        // Act
        var result = (await _service.GetAttributionAnalyticsAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        var googleCpc = result.First(r => r.UtmSource == "google");
        googleCpc.TotalLeads.Should().Be(2);
        googleCpc.ConvertedLeads.Should().Be(1);
        googleCpc.ConversionRate.Should().Be(50m);
    }

    [Fact]
    public async Task GetAttributionAnalyticsAsync_ShouldIncludeNullUtmGroup_WhenLeadsHaveNoUtm()
    {
        // Arrange — leads without UTM parameters
        _leads.AddRange(new[]
        {
            new Lead { Id = 1, UtmSource = null, UtmMedium = null, UtmCampaign = null, Status = LeadLifecycleStatus.New, Score = 40, IsDeleted = false },
            new Lead { Id = 2, UtmSource = null, UtmMedium = null, UtmCampaign = null, Status = LeadLifecycleStatus.Qualified, Score = 60, IsDeleted = false }
        });
        RefreshMockDbSet();

        // Act
        var result = (await _service.GetAttributionAnalyticsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].UtmSource.Should().BeNull();
        result[0].TotalLeads.Should().Be(2);
        result[0].ConversionRate.Should().Be(0m);
    }

    [Fact]
    public async Task GetAttributionAnalyticsAsync_ShouldExcludeDeletedLeads()
    {
        // Arrange
        _leads.AddRange(new[]
        {
            new Lead { Id = 1, UtmCampaign = "spring", Status = LeadLifecycleStatus.New, IsDeleted = false },
            new Lead { Id = 2, UtmCampaign = "spring", Status = LeadLifecycleStatus.New, IsDeleted = true }
        });
        RefreshMockDbSet();

        // Act
        var result = (await _service.GetAttributionAnalyticsAsync()).ToList();

        // Assert: only 1 active lead
        result.Should().HaveCount(1);
        result[0].TotalLeads.Should().Be(1);
    }
}
