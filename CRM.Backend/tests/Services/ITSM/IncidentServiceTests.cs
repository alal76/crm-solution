// CRM Solution - ITSM Incident Service Unit Tests
// Tests for IncidentService - ITSM incident management

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class IncidentServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ISLAService> _mockSlaService;
    private readonly Mock<ILogger<IncidentService>> _mockLogger;
    private readonly IIncidentService _service;

    public IncidentServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockSlaService = new Mock<ISLAService>();
        _mockLogger = new Mock<ILogger<IncidentService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);
        _service = new IncidentService(_mockResolver.Object, _mockSlaService.Object, _mockLogger.Object);
    }

    // ========================================================================
    // CreateIncidentAsync
    // ========================================================================

    [Fact]
    public async Task CreateIncidentAsync_ShouldCreateIncident_WhenValidDtoProvided()
    {
        // Arrange
        var incidents = new List<Incident>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(incidents);
        mockSet.Setup(m => m.Add(It.IsAny<Incident>())).Callback<Incident>(e => incidents.Add(e));
        _mockContext.Setup(c => c.Incidents).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateIncidentDto
        {
            ShortDescription = "Server is down",
            Description = "Production server is not responding",
            CallerId = 1,
            ContactType = ContactType.Phone,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High
        };

        // Act
        var result = await _service.CreateIncidentAsync(dto, createdById: 1);

        // Assert
        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Server is down");
        mockSet.Verify(m => m.Add(It.IsAny<Incident>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldStartSLA_WhenIncidentCreated()
    {
        // Arrange
        var incidents = new List<Incident>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(incidents);
        mockSet.Setup(m => m.Add(It.IsAny<Incident>())).Callback<Incident>(e =>
        {
            e.IncidentId = 1;
            incidents.Add(e);
        });
        _mockContext.Setup(c => c.Incidents).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateIncidentDto
        {
            ShortDescription = "Test incident for SLA",
            CallerId = 1,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium
        };

        // Act
        await _service.CreateIncidentAsync(dto, createdById: 1);

        // Assert
        _mockSlaService.Verify(
            s => s.StartSLAAsync(It.IsAny<int>(), SLATargetType.Incident, It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldGenerateNumber_WhenCreated()
    {
        // Arrange
        var incidents = new List<Incident>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(incidents);
        mockSet.Setup(m => m.Add(It.IsAny<Incident>())).Callback<Incident>(e => incidents.Add(e));
        _mockContext.Setup(c => c.Incidents).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateIncidentDto
        {
            ShortDescription = "Number gen test",
            CallerId = 1,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low
        };

        // Act
        var result = await _service.CreateIncidentAsync(dto, createdById: 1);

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().NotBeNullOrEmpty();
    }

    // ========================================================================
    // GetIncidentByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnIncident_WhenExists()
    {
        // Arrange
        var incidents = new List<Incident>
        {
            new()
            {
                IncidentId = 1,
                Number = "INC0001",
                ShortDescription = "Server down",
                CallerId = 1,
                Impact = IncidentImpact.High,
                Urgency = IncidentUrgency.High,
                State = IncidentState.New,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(incidents).Object);

        // Act
        var result = await _service.GetIncidentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Number.Should().Be("INC0001");
        result.ShortDescription.Should().Be("Server down");
    }

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident>()).Object);

        // Act
        var result = await _service.GetIncidentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetIncidentsAsync
    // ========================================================================

    [Fact]
    public async Task GetIncidentsAsync_ShouldReturnFilteredResults_WhenStateFilterApplied()
    {
        // Arrange
        var incidents = new List<Incident>
        {
            new() { IncidentId = 1, Number = "INC0001", ShortDescription = "Open", State = IncidentState.New, CallerId = 1, Impact = IncidentImpact.High, Urgency = IncidentUrgency.High, CreatedAt = DateTime.UtcNow },
            new() { IncidentId = 2, Number = "INC0002", ShortDescription = "Resolved", State = IncidentState.Resolved, CallerId = 1, Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low, CreatedAt = DateTime.UtcNow },
            new() { IncidentId = 3, Number = "INC0003", ShortDescription = "Also open", State = IncidentState.New, CallerId = 2, Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(incidents).Object);

        var filter = new IncidentFilterDto { State = IncidentState.New, PageNumber = 1, PageSize = 20 };

        // Act
        var (items, totalCount) = await _service.GetIncidentsAsync(filter);

        // Assert
        items.Should().NotBeNull();
        totalCount.Should().Be(2);
    }

    // ========================================================================
    // UpdateIncidentAsync
    // ========================================================================

    [Fact]
    public async Task UpdateIncidentAsync_ShouldUpdateFields_WhenIncidentExists()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Old desc",
            CallerId = 1, Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low,
            State = IncidentState.New, CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.IncidentHistory).Returns(MockDbSetFactory.CreateMockDbSet(new List<IncidentHistory>()).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new UpdateIncidentDto { ShortDescription = "Updated desc", Impact = IncidentImpact.High };

        // Act
        var result = await _service.UpdateIncidentAsync(1, dto, modifiedById: 2);

        // Assert
        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Updated desc");
    }

    // ========================================================================
    // AssignIncidentAsync
    // ========================================================================

    [Fact]
    public async Task AssignIncidentAsync_ShouldAssignUser_WhenIncidentExists()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Test",
            CallerId = 1, State = IncidentState.New,
            Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.AssignIncidentAsync(1, assignedToId: 5, assignmentGroupId: null, modifiedById: 1);

        // Assert
        result.Should().BeTrue();
        incident.AssignedToId.Should().Be(5);
    }

    [Fact]
    public async Task AssignIncidentAsync_ShouldReturnFalse_WhenIncidentNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident>()).Object);

        // Act
        var result = await _service.AssignIncidentAsync(999, 5, null, 1);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // ResolveIncidentAsync
    // ========================================================================

    [Fact]
    public async Task ResolveIncidentAsync_ShouldSetResolvedState_WhenValid()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Test",
            CallerId = 1, State = IncidentState.InProgress,
            Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Applied patch"
        };

        // Act
        var result = await _service.ResolveIncidentAsync(1, dto, resolvedById: 3);

        // Assert
        result.Should().NotBeNull();
        result.State.Should().Be(IncidentState.Resolved);
    }

    // ========================================================================
    // CloseIncidentAsync
    // ========================================================================

    [Fact]
    public async Task CloseIncidentAsync_ShouldClose_WhenResolved()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Test",
            CallerId = 1, State = IncidentState.Resolved,
            Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CloseIncidentAsync(1, closedById: 2);

        // Assert
        result.Should().BeTrue();
        incident.State.Should().Be(IncidentState.Closed);
    }

    // ========================================================================
    // EscalateIncidentAsync
    // ========================================================================

    [Fact]
    public async Task EscalateIncidentAsync_ShouldIncrementLevel_WhenIncidentExists()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Escalation test",
            CallerId = 1, State = IncidentState.InProgress, EscalationLevel = 0,
            Impact = IncidentImpact.High, Urgency = IncidentUrgency.High,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.EscalateIncidentAsync(1, modifiedById: 1);

        // Assert
        result.Should().BeTrue();
        incident.EscalationLevel.Should().BeGreaterThan(0);
    }

    // ========================================================================
    // ReopenIncidentAsync
    // ========================================================================

    [Fact]
    public async Task ReopenIncidentAsync_ShouldReopen_WhenResolvedOrClosed()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Reopen test",
            CallerId = 1, State = IncidentState.Resolved,
            Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ReopenIncidentAsync(1, modifiedById: 2);

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // AddCommentAsync / GetCommentsAsync
    // ========================================================================

    [Fact]
    public async Task AddCommentAsync_ShouldAddComment_WhenIncidentExists()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1, Number = "INC0001", ShortDescription = "Comment test",
            CallerId = 1, State = IncidentState.InProgress,
            Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium,
            CreatedAt = DateTime.UtcNow
        };
        var comments = new List<IncidentComment>();
        var mockCommentSet = MockDbSetFactory.CreateMockDbSet(comments);
        mockCommentSet.Setup(m => m.Add(It.IsAny<IncidentComment>())).Callback<IncidentComment>(e => comments.Add(e));

        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.IncidentComments).Returns(mockCommentSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.AddCommentAsync(1, "This is a test comment", isInternal: false, createdById: 1);

        // Assert
        result.Should().BeTrue();
        mockCommentSet.Verify(m => m.Add(It.IsAny<IncidentComment>()), Times.Once);
    }

    [Fact]
    public async Task GetCommentsAsync_ShouldReturnComments_ForIncident()
    {
        // Arrange
        var comments = new List<IncidentComment>
        {
            new() { CommentId = 1, IncidentId = 1, Comment = "First comment", CreatedById = 1, IsInternal = false, CreatedAt = DateTime.UtcNow },
            new() { CommentId = 2, IncidentId = 1, Comment = "Internal note", CreatedById = 2, IsInternal = true, CreatedAt = DateTime.UtcNow },
            new() { CommentId = 3, IncidentId = 2, Comment = "Other incident", CreatedById = 1, IsInternal = false, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.IncidentComments).Returns(MockDbSetFactory.CreateMockDbSet(comments).Object);

        // Act
        var result = await _service.GetCommentsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
}
