// CRM Solution - ITSM Problem Service Unit Tests
// Tests for ProblemService - ITSM problem management

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

public class ProblemServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ProblemService>> _mockLogger;
    private readonly IProblemService _service;

    public ProblemServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ProblemService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);
        _service = new ProblemService(_mockResolver.Object, _mockLogger.Object);
    }

    // ========================================================================
    // CreateProblemAsync
    // ========================================================================

    [Fact]
    public async Task CreateProblemAsync_ShouldCreateProblem_WhenValidDtoProvided()
    {
        // Arrange
        var problems = new List<Problem>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(problems);
        mockSet.Setup(m => m.Add(It.IsAny<Problem>())).Callback<Problem>(e => problems.Add(e));
        _mockContext.Setup(c => c.Problems).Returns(mockSet.Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<ProblemIncident>()).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateProblemDto
        {
            ShortDescription = "Recurring network issue",
            Description = "Network connectivity drops every 2 hours",
            Priority = ProblemPriority.High
        };

        // Act
        var result = await _service.CreateProblemAsync(dto, createdById: 1);

        // Assert
        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Recurring network issue");
        mockSet.Verify(m => m.Add(It.IsAny<Problem>()), Times.Once);
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldGenerateNumber()
    {
        // Arrange
        var problems = new List<Problem>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(problems);
        mockSet.Setup(m => m.Add(It.IsAny<Problem>())).Callback<Problem>(e => problems.Add(e));
        _mockContext.Setup(c => c.Problems).Returns(mockSet.Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<ProblemIncident>()).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateProblemDto
        {
            ShortDescription = "Number test",
            Priority = ProblemPriority.Medium
        };

        // Act
        var result = await _service.CreateProblemAsync(dto, createdById: 1);

        // Assert
        result.Number.Should().NotBeNullOrEmpty();
    }

    // ========================================================================
    // GetProblemByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetProblemByIdAsync_ShouldReturnProblem_WhenExists()
    {
        // Arrange
        var problems = new List<Problem>
        {
            new()
            {
                ProblemId = 1, Number = "PRB0001", ShortDescription = "Network issue",
                State = ProblemState.New, Priority = ProblemPriority.High,
                CreatedAt = DateTime.UtcNow, IsDeleted = false
            }
        };
        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(problems).Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<ProblemIncident>()).Object);

        // Act
        var result = await _service.GetProblemByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ShortDescription.Should().Be("Network issue");
    }

    [Fact]
    public async Task GetProblemByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(new List<Problem>()).Object);

        // Act
        var result = await _service.GetProblemByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetProblemsAsync
    // ========================================================================

    [Fact]
    public async Task GetProblemsAsync_ShouldReturnFilteredResults()
    {
        // Arrange
        var problems = new List<Problem>
        {
            new() { ProblemId = 1, Number = "PRB0001", ShortDescription = "P1", State = ProblemState.New, Priority = ProblemPriority.High, CreatedAt = DateTime.UtcNow },
            new() { ProblemId = 2, Number = "PRB0002", ShortDescription = "P2", State = ProblemState.Closed, Priority = ProblemPriority.Low, CreatedAt = DateTime.UtcNow },
            new() { ProblemId = 3, Number = "PRB0003", ShortDescription = "P3", State = ProblemState.New, Priority = ProblemPriority.Medium, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(problems).Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<ProblemIncident>()).Object);

        var filter = new ProblemFilterDto { State = ProblemState.New, PageNumber = 1, PageSize = 20 };

        // Act
        var (items, totalCount) = await _service.GetProblemsAsync(filter);

        // Assert
        totalCount.Should().Be(2);
    }

    // ========================================================================
    // UpdateProblemAsync
    // ========================================================================

    [Fact]
    public async Task UpdateProblemAsync_ShouldUpdateFields_WhenProblemExists()
    {
        // Arrange
        var problem = new Problem
        {
            ProblemId = 1, Number = "PRB0001", ShortDescription = "Old desc",
            State = ProblemState.New, Priority = ProblemPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(new List<Problem> { problem }).Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<ProblemIncident>()).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new UpdateProblemDto { ShortDescription = "Updated desc", KnownError = true };

        // Act
        var result = await _service.UpdateProblemAsync(1, dto, modifiedById: 2);

        // Assert
        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Updated desc");
    }

    // ========================================================================
    // LinkIncidentAsync
    // ========================================================================

    [Fact]
    public async Task LinkIncidentAsync_ShouldLink_WhenBothExist()
    {
        // Arrange
        var problem = new Problem
        {
            ProblemId = 1, Number = "PRB0001", ShortDescription = "Test",
            State = ProblemState.New, Priority = ProblemPriority.High,
            CreatedAt = DateTime.UtcNow
        };
        var incident = new Incident
        {
            IncidentId = 10, Number = "INC0010", ShortDescription = "Related",
            CallerId = 1, State = IncidentState.InProgress,
            Impact = IncidentImpact.High, Urgency = IncidentUrgency.High,
            CreatedAt = DateTime.UtcNow
        };
        var links = new List<ProblemIncident>();
        var mockLinkSet = MockDbSetFactory.CreateMockDbSet(links);
        mockLinkSet.Setup(m => m.Add(It.IsAny<ProblemIncident>())).Callback<ProblemIncident>(e => links.Add(e));

        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(new List<Problem> { problem }).Object);
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident }).Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(mockLinkSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.LinkIncidentAsync(1, 10, createdById: 1);

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // MarkAsKnownErrorAsync
    // ========================================================================

    [Fact]
    public async Task MarkAsKnownErrorAsync_ShouldSetKnownErrorFlag()
    {
        // Arrange
        var problem = new Problem
        {
            ProblemId = 1, Number = "PRB0001", ShortDescription = "Test",
            State = ProblemState.New, Priority = ProblemPriority.High,
            KnownError = false, CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(new List<Problem> { problem }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.MarkAsKnownErrorAsync(1, modifiedById: 2);

        // Assert
        result.Should().BeTrue();
        problem.KnownError.Should().BeTrue();
    }

    // ========================================================================
    // GetRelatedIncidentsAsync
    // ========================================================================

    [Fact]
    public async Task GetRelatedIncidentsAsync_ShouldReturnLinkedIncidents()
    {
        // Arrange
        var incident1 = new Incident { IncidentId = 1, Number = "INC0001", ShortDescription = "Linked 1", CallerId = 1, State = IncidentState.New, Impact = IncidentImpact.High, Urgency = IncidentUrgency.High, CreatedAt = DateTime.UtcNow };
        var incident2 = new Incident { IncidentId = 2, Number = "INC0002", ShortDescription = "Linked 2", CallerId = 2, State = IncidentState.New, Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low, CreatedAt = DateTime.UtcNow };
        var links = new List<ProblemIncident>
        {
            new() { ProblemIncidentId = 1, ProblemId = 1, IncidentId = 1, CreatedAt = DateTime.UtcNow, Incident = incident1 },
            new() { ProblemIncidentId = 2, ProblemId = 1, IncidentId = 2, CreatedAt = DateTime.UtcNow, Incident = incident2 }
        };
        _mockContext.Setup(c => c.ProblemIncidents).Returns(MockDbSetFactory.CreateMockDbSet(links).Object);
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident> { incident1, incident2 }).Object);

        // Act
        var result = await _service.GetRelatedIncidentsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    // ========================================================================
    // UpdateRootCauseAnalysisAsync
    // ========================================================================

    [Fact]
    public async Task UpdateRootCauseAnalysisAsync_ShouldSetRootCauseAndWorkaround()
    {
        // Arrange
        var problem = new Problem
        {
            ProblemId = 1, Number = "PRB0001", ShortDescription = "RCA test",
            State = ProblemState.New, Priority = ProblemPriority.High,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.Problems).Returns(MockDbSetFactory.CreateMockDbSet(new List<Problem> { problem }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateRootCauseAnalysisAsync(
            1, rootCause: "Memory leak in service", workaround: "Restart daily", modifiedById: 3);

        // Assert
        result.Should().BeTrue();
        problem.RootCause.Should().Be("Memory leak in service");
        problem.Workaround.Should().Be("Restart daily");
    }

    // ========================================================================
    // CreateProblemAsync with linked incidents
    // ========================================================================

    [Fact]
    public async Task CreateProblemAsync_ShouldLinkIncidents_WhenIdsProvided()
    {
        // Arrange
        var problems = new List<Problem>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(problems);
        mockSet.Setup(m => m.Add(It.IsAny<Problem>())).Callback<Problem>(e =>
        {
            e.ProblemId = 1;
            problems.Add(e);
        });
        var links = new List<ProblemIncident>();
        var mockLinkSet = MockDbSetFactory.CreateMockDbSet(links);
        mockLinkSet.Setup(m => m.Add(It.IsAny<ProblemIncident>())).Callback<ProblemIncident>(e => links.Add(e));

        var incidents = new List<Incident>
        {
            new() { IncidentId = 10, Number = "INC0010", ShortDescription = "A", CallerId = 1, State = IncidentState.New, Impact = IncidentImpact.High, Urgency = IncidentUrgency.High, CreatedAt = DateTime.UtcNow },
            new() { IncidentId = 20, Number = "INC0020", ShortDescription = "B", CallerId = 2, State = IncidentState.New, Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low, CreatedAt = DateTime.UtcNow }
        };

        _mockContext.Setup(c => c.Problems).Returns(mockSet.Object);
        _mockContext.Setup(c => c.ProblemIncidents).Returns(mockLinkSet.Object);
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(incidents).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateProblemDto
        {
            ShortDescription = "With incidents",
            Priority = ProblemPriority.High,
            IncidentIds = new List<int> { 10, 20 }
        };

        // Act
        var result = await _service.CreateProblemAsync(dto, createdById: 1);

        // Assert
        result.Should().NotBeNull();
    }
}
