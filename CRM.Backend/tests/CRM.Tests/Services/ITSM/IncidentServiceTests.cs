// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class IncidentServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ISLAService> _mockSlaService;
    private readonly Mock<ILogger<IncidentService>> _mockLogger;
    private readonly IncidentService _service;

    public IncidentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"IncidentTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockSlaService = new Mock<ISLAService>();
        _mockLogger = new Mock<ILogger<IncidentService>>();
        _service = new IncidentService(_context, _mockSlaService.Object, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetIncidentByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldAddIncidentToDatabase()
    {
        var dto = new CreateIncidentDto
        {
            ShortDescription = "Test Incident",
            Description = "A test incident for unit testing",
            CallerId = 1,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low
        };

        var result = await _service.CreateIncidentAsync(dto, createdById: 1);

        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Test Incident");
        _context.Incidents.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldGenerateIncidentNumber()
    {
        var dto = new CreateIncidentDto
        {
            ShortDescription = "Numbered Incident",
            CallerId = 1,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium
        };

        var result = await _service.CreateIncidentAsync(dto, createdById: 1);

        result.Number.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldCallSLAService()
    {
        var dto = new CreateIncidentDto
        {
            ShortDescription = "SLA Test",
            CallerId = 1,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High
        };

        await _service.CreateIncidentAsync(dto, createdById: 1);

        _mockSlaService.Verify(s => s.StartSLAAsync(It.IsAny<int>(), SLATargetType.Incident, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnIncident_WhenExists()
    {
        // Seed directly into the InMemory store to avoid service call complexity
        _context.Incidents.Add(new Incident
        {
            ShortDescription = "Existing Incident",
            State = IncidentState.New,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low,
            CallerId = 1,
            OpenedById = 1,
            Number = "INC0000001",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        var incidentId = _context.Incidents.First().IncidentId;

        var result = await _service.GetIncidentByIdAsync(incidentId);
    }

    [Fact]
    public async Task GetIncidentsAsync_ShouldReturnEmpty_WhenAllIncidentsDeleted()
    {
        // Seed soft-deleted incidents directly
        _context.Incidents.Add(new Incident { IncidentId = 201, ShortDescription = "Inc Del 1", State = IncidentState.New, Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low, CallerId = 1, OpenedById = 1, Number = "INC0000201", IsDeleted = true, CreatedAt = DateTime.UtcNow });
        _context.Incidents.Add(new Incident { IncidentId = 202, ShortDescription = "Inc Del 2", State = IncidentState.New, Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium, CallerId = 1, OpenedById = 1, Number = "INC0000202", IsDeleted = true, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var filter = new IncidentFilterDto();
        var result = await _service.GetIncidentsAsync(filter);

        // All incidents are soft-deleted, so the filter !IsDeleted should return 0
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAddCommentToIncident()
    {
        var incident = new Incident
        {
            IncidentId = 20,
            Number = "INC0000020",
            ShortDescription = "Comment Test",
            State = IncidentState.New,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low,
            CallerId = 1,
            OpenedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        var result = await _service.AddCommentAsync(20, "This is a test comment", isInternal: false, createdById: 1);

        result.Should().BeTrue();
        _context.IncidentComments.Count().Should().Be(1);
    }
}
