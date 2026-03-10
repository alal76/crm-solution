// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for CampaignMetricService (TCOV-031).</summary>
public class CampaignMetricServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly Mock<ILogger<CampaignMetricService>> _logger;
    private readonly CampaignMetricService _service;

    private readonly List<CampaignMetric> _metrics;
    private readonly List<MarketingCampaign> _campaigns;
    private readonly List<CampaignRecipient> _recipients;

    public CampaignMetricServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        _logger = new Mock<ILogger<CampaignMetricService>>();

        _metrics = new List<CampaignMetric>();
        _campaigns = new List<MarketingCampaign>();
        _recipients = new List<CampaignRecipient>();

        _mockCtx.Setup(c => c.CampaignMetrics).Returns(MockDbSetFactory.CreateMockDbSet(_metrics).Object);
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);
        _mockCtx.Setup(c => c.CampaignRecipients).Returns(MockDbSetFactory.CreateMockDbSet(_recipients).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new CampaignMetricService(_mockCtx.Object, _logger.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ShouldSetCreatedAtAndReturnMetric()
    {
        var metric = new CampaignMetric { CampaignId = 1, TotalSent = 100 };

        var result = await _service.CreateAsync(metric);

        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallSaveChanges()
    {
        var metric = new CampaignMetric { CampaignId = 2 };

        await _service.CreateAsync(metric);

        _mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetMetricsAsync ──────────────────────────────────────────────────────
    [Fact]
    public async Task GetMetricsAsync_ShouldReturnNull_WhenCampaignNotFound()
    {
        var result = await _service.GetMetricsAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldReturnDto_WhenCampaignExists()
    {
        _campaigns.Add(new MarketingCampaign
        {
            Id = 1,
            Name = "Spring Promo",
            IsDeleted = false,
            Budget = 5000m,
            ActualCost = 2000m
        });
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);

        var result = await _service.GetMetricsAsync(1);

        result.Should().NotBeNull();
        result!.CampaignId.Should().Be(1);
        result.CampaignName.Should().Be("Spring Promo");
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldCalculateBudgetRemaining()
    {
        _campaigns.Add(new MarketingCampaign
        {
            Id = 2,
            Name = "Budget Test",
            IsDeleted = false,
            Budget = 10000m,
            ActualCost = 3000m
        });
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);

        var result = await _service.GetMetricsAsync(2);

        result!.BudgetRemaining.Should().Be(7000);
    }

    [Fact]
    public async Task Constructor_ShouldThrow_WhenContextIsNull()
    {
        Action act = () => new CampaignMetricService(null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }
}
