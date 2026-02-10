// CRM Solution - DashboardBuilderService Unit Tests
// Phase 7, Task 7.4 - Tests for custom dashboard builder with drag-and-drop widgets

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class DashboardBuilderServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<DashboardBuilderService>> _mockLogger;
    private readonly DashboardBuilderService _service;

    public DashboardBuilderServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DashboardBuilderService>>();

        _service = new DashboardBuilderService(
            _mockContext.Object,
            _mockLogger.Object);
    }

    // ========================================================================
    // CreateDashboardAsync
    // ========================================================================

    [Fact]
    public async Task CreateDashboardAsync_ShouldReturnDashboard_WithId()
    {
        // Act
        var dashboard = await _service.CreateDashboardAsync(1, "My Dashboard", "Test description");

        // Assert
        dashboard.Should().NotBeNull();
        dashboard.Id.Should().NotBeNullOrEmpty();
        dashboard.Name.Should().Be("My Dashboard");
        dashboard.Description.Should().Be("Test description");
        dashboard.UserId.Should().Be(1);
        dashboard.Widgets.Should().NotBeNull();
        dashboard.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateDashboardAsync_ShouldGenerateUniqueIds()
    {
        // Act
        var d1 = await _service.CreateDashboardAsync(1, "Dashboard 1", null);
        var d2 = await _service.CreateDashboardAsync(1, "Dashboard 2", null);

        // Assert
        d1.Id.Should().NotBe(d2.Id);
    }

    // ========================================================================
    // GetDashboardAsync
    // ========================================================================

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnDashboard_WhenExists()
    {
        // Arrange
        var created = await _service.CreateDashboardAsync(1, "Test", null);

        // Act
        var retrieved = await _service.GetDashboardAsync(created.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetDashboardAsync("nonexistent-id");

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetDashboardsAsync
    // ========================================================================

    [Fact]
    public async Task GetDashboardsAsync_ShouldReturnOnlyUserDashboards()
    {
        // Arrange
        await _service.CreateDashboardAsync(1, "User 1 Dashboard", null);
        await _service.CreateDashboardAsync(2, "User 2 Dashboard", null);

        // Act
        var dashboards = await _service.GetDashboardsAsync(1);

        // Assert
        dashboards.Should().HaveCount(1);
        dashboards.First().Name.Should().Be("User 1 Dashboard");
    }

    [Fact]
    public async Task GetDashboardsAsync_ShouldReturnEmpty_WhenUserHasNone()
    {
        // Act
        var dashboards = await _service.GetDashboardsAsync(999);

        // Assert
        dashboards.Should().BeEmpty();
    }

    // ========================================================================
    // UpdateDashboardAsync
    // ========================================================================

    [Fact]
    public async Task UpdateDashboardAsync_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var created = await _service.CreateDashboardAsync(1, "Original", "Desc");

        var updated = new CustomDashboard
        {
            Id = created.Id,
            Name = "Updated Name",
            Description = "Updated Desc",
            UserId = 1,
            Widgets = new List<DashboardWidget>()
        };

        // Act
        var result = await _service.UpdateDashboardAsync(updated);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Desc");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateDashboardAsync_ShouldReturnNull_WhenDashboardNotFound()
    {
        // Arrange
        var dashboard = new CustomDashboard
        {
            Id = "nonexistent",
            Name = "Test",
            UserId = 1,
            Widgets = new List<DashboardWidget>()
        };

        // Act
        var result = await _service.UpdateDashboardAsync(dashboard);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateDashboardAsync_ShouldUpdateWidgets()
    {
        // Arrange
        var created = await _service.CreateDashboardAsync(1, "Dashboard", null);

        var withWidgets = new CustomDashboard
        {
            Id = created.Id,
            Name = "Dashboard",
            UserId = 1,
            Widgets = new List<DashboardWidget>
            {
                new() { Id = "w1", Type = "pipeline-chart", Title = "Pipeline", Column = 0, Row = 0, Width = 6, Height = 4 },
                new() { Id = "w2", Type = "revenue-kpi", Title = "Revenue", Column = 6, Row = 0, Width = 6, Height = 4 }
            }
        };

        // Act
        var result = await _service.UpdateDashboardAsync(withWidgets);

        // Assert
        result!.Widgets.Should().HaveCount(2);
    }

    // ========================================================================
    // DeleteDashboardAsync
    // ========================================================================

    [Fact]
    public async Task DeleteDashboardAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var created = await _service.CreateDashboardAsync(1, "To Delete", null);

        // Act
        var result = await _service.DeleteDashboardAsync(created.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDashboardAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.DeleteDashboardAsync("nonexistent-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDashboardAsync_ShouldRemoveDashboard()
    {
        // Arrange
        var created = await _service.CreateDashboardAsync(1, "To Delete", null);
        await _service.DeleteDashboardAsync(created.Id);

        // Act
        var retrieved = await _service.GetDashboardAsync(created.Id);

        // Assert
        retrieved.Should().BeNull();
    }

    // ========================================================================
    // GetAvailableWidgets
    // ========================================================================

    [Fact]
    public void GetAvailableWidgets_ShouldReturnWidgetCatalog()
    {
        // Act
        var widgets = _service.GetAvailableWidgets();

        // Assert
        widgets.Should().NotBeEmpty();
        widgets.Should().HaveCountGreaterOrEqualTo(5);
    }

    [Fact]
    public void GetAvailableWidgets_ShouldIncludePipelineChart()
    {
        // Act
        var widgets = _service.GetAvailableWidgets();

        // Assert
        widgets.Should().Contain(w => w.Type == "pipeline-chart");
    }

    [Fact]
    public void GetAvailableWidgets_ShouldHaveValidDimensions()
    {
        // Act
        var widgets = _service.GetAvailableWidgets();

        // Assert
        foreach (var widget in widgets)
        {
            widget.DefaultWidth.Should().BeGreaterThan(0);
            widget.DefaultHeight.Should().BeGreaterThan(0);
            widget.Name.Should().NotBeNullOrEmpty();
            widget.Type.Should().NotBeNullOrEmpty();
        }
    }

    // ========================================================================
    // GetWidgetDataAsync
    // ========================================================================

    [Fact]
    public async Task GetWidgetDataAsync_ShouldReturnData_ForPipelineChart()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Opp A", Stage = OpportunityStage.Proposal, Amount = 10000, IsDeleted = false },
            new() { Id = 2, Name = "Opp B", Stage = OpportunityStage.Negotiation, Amount = 20000, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.GetWidgetDataAsync("any-widget-id", "pipeline-chart");

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("pipeline-chart");
        result.Data.Should().NotBeNull();
        result.FetchedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetWidgetDataAsync_ShouldReturnData_ForLeadSummary()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", Status = LeadLifecycleStatus.New, IsDeleted = false },
            new() { Id = 2, FirstName = "C", LastName = "D", Email = "c@d.com", Status = LeadLifecycleStatus.Qualified, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var result = await _service.GetWidgetDataAsync("w1", "lead-summary");

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("lead-summary");
    }

    [Fact]
    public async Task GetWidgetDataAsync_ShouldReturnEmptyData_ForUnknownType()
    {
        // Act
        var result = await _service.GetWidgetDataAsync("w1", "unknown-type");

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("unknown-type");
    }
}
