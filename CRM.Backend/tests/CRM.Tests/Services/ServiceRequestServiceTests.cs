// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for ServiceRequestService (TCOV-029).</summary>
public class ServiceRequestServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly Mock<ILogger<ServiceRequestService>> _logger;
    private readonly NormalizationService _normalizationService;
    private readonly ServiceRequestService _service;

    private readonly List<ServiceRequest> _serviceRequests;
    private readonly List<EntityTag> _tags;
    private readonly List<CustomField> _customFields;
    private readonly List<ContactInfoLink> _contactInfoLinks;

    public ServiceRequestServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        _logger = new Mock<ILogger<ServiceRequestService>>();

        _serviceRequests = new List<ServiceRequest>();
        _tags = new List<EntityTag>();
        _customFields = new List<CustomField>();
        _contactInfoLinks = new List<ContactInfoLink>();

        // Setup NormalizationService dependencies
        var mockTags = MockDbSetFactory.CreateMockDbSet(_tags);
        var mockCustomFields = MockDbSetFactory.CreateMockDbSet(_customFields);
        var mockContactInfoLinks = MockDbSetFactory.CreateMockDbSet(_contactInfoLinks);
        _mockCtx.Setup(c => c.EntityTags).Returns(mockTags.Object);
        _mockCtx.Setup(c => c.CustomFields).Returns(mockCustomFields.Object);
        _mockCtx.Setup(c => c.ContactInfoLinks).Returns(mockContactInfoLinks.Object);
        _normalizationService = new NormalizationService(_mockCtx.Object);

        // Setup ServiceRequests DbSet
        var mockSrSet = MockDbSetFactory.CreateMockDbSet(_serviceRequests);
        _mockCtx.Setup(c => c.ServiceRequests).Returns(mockSrSet.Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new ServiceRequestService(_mockCtx.Object, _logger.Object, _normalizationService);
    }

    // ── GetServiceRequestByIdAsync ──────────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetServiceRequestByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceRequestByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        _serviceRequests.Add(new ServiceRequest { Id = 1, IsDeleted = true, TicketNumber = "SR-001" });
        // Reset mock so query hits updated list
        var mockSrSet = MockDbSetFactory.CreateMockDbSet(_serviceRequests);
        _mockCtx.Setup(c => c.ServiceRequests).Returns(mockSrSet.Object);

        var result = await _service.GetServiceRequestByIdAsync(1);
        result.Should().BeNull();
    }

    // ── GetServiceRequestByTicketNumberAsync ────────────────────────────────
    [Fact]
    public async Task GetServiceRequestByTicketNumberAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetServiceRequestByTicketNumberAsync("SR-99999");
        result.Should().BeNull();
    }

    // ── DeleteServiceRequestAsync ───────────────────────────────────────────
    [Fact]
    public async Task DeleteServiceRequestAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteServiceRequestAsync(999);
        result.Should().BeFalse();
    }

    // ── GetServiceRequestsByAccountAsync ────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByAccountAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByAccountAsync(42);
        result.Should().BeEmpty();
    }

    // ── GetServiceRequestsByContactAsync ────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByContactAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByContactAsync(99);
        result.Should().BeEmpty();
    }

    // ── GetServiceRequestsByAssigneeAsync ───────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByAssigneeAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByAssigneeAsync(5);
        result.Should().BeEmpty();
    }

    // ── GetServiceRequestsByGroupAsync ──────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByGroupAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByGroupAsync(3);
        result.Should().BeEmpty();
    }
}
