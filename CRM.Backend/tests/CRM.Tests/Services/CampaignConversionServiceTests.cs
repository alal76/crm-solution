// CRM Solution — Unit Tests
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

/// <summary>Unit tests for CampaignConversionService (TCOV-032).</summary>
public class CampaignConversionServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly CampaignConversionService _service;

    private readonly List<CampaignConversion> _conversions;
    private readonly List<MarketingCampaign> _campaigns;

    public CampaignConversionServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        var logger = new Mock<ILogger<CampaignConversionService>>().Object;

        _conversions = new List<CampaignConversion>();
        _campaigns = new List<MarketingCampaign>();

        _mockCtx.Setup(c => c.CampaignConversions).Returns(MockDbSetFactory.CreateMockDbSet(_conversions).Object);
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new CampaignConversionService(_mockCtx.Object, logger);
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoConversionsExist()
    {
        var (items, total) = await _service.GetAllAsync();
        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedConversions()
    {
        _conversions.AddRange(new[]
        {
            new CampaignConversion { Id = 1, CampaignId = 1, IsDeleted = false },
            new CampaignConversion { Id = 2, CampaignId = 1, IsDeleted = true }
        });
        _mockCtx.Setup(c => c.CampaignConversions).Returns(MockDbSetFactory.CreateMockDbSet(_conversions).Object);

        var (items, total) = await _service.GetAllAsync();
        total.Should().Be(1);
        items.Should().HaveCount(1);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    // ── GetByCampaignIdAsync ─────────────────────────────────────────────────
    [Fact]
    public async Task GetByCampaignIdAsync_ShouldReturnEmpty_WhenNoneForCampaign()
    {
        var result = await _service.GetByCampaignIdAsync(42);
        result.Should().BeEmpty();
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCampaignDoesNotExist()
    {
        var dto = new CreateCampaignConversionDto { CampaignId = 99 };
        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Campaign*99*");
    }
}
