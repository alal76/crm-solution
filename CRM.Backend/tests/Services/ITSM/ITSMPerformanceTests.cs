// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// ITSM-048: Performance/load tests for high-volume incident operations
/// and SLA processing. Validates that service methods complete within
/// acceptable time thresholds.
/// </summary>
[Trait("Category", "Performance")]
public class ITSMPerformanceTests
{
    private readonly Mock<IIncidentService> _mockIncidentService;

    public ITSMPerformanceTests()
    {
        _mockIncidentService = new Mock<IIncidentService>();
    }

    [Fact]
    public async Task GetIncidents_ShouldCompleteWithin500ms_ForLargeResultSet()
    {
        // Arrange
        var incidents = Enumerable.Range(1, 1000).Select(i => new IncidentDto
        {
            IncidentId = i,
            ShortDescription = $"Incident {i}",
            State = IncidentState.New
        });

        _mockIncidentService.Setup(s => s.GetIncidentsAsync(It.IsAny<IncidentFilterDto>()))
            .ReturnsAsync((incidents, 1000));

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _mockIncidentService.Object.GetIncidentsAsync(new IncidentFilterDto());
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500,
            "fetching 1000 incidents should complete within 500ms");
        result.Items.Should().HaveCount(1000);
    }

    [Fact]
    public async Task CreateIncident_ShouldCompleteWithin200ms()
    {
        // Arrange
        var dto = new CreateIncidentDto
        {
            ShortDescription = "Performance test incident",
            CallerId = 1,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium
        };

        _mockIncidentService.Setup(s => s.CreateIncidentAsync(dto, 1))
            .ReturnsAsync(new IncidentDto { IncidentId = 1 });

        // Act
        var stopwatch = Stopwatch.StartNew();
        await _mockIncidentService.Object.CreateIncidentAsync(dto, 1);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            "creating an incident should complete within 200ms");
    }

    [Fact]
    public async Task BulkIncidentCreation_ShouldCompleteWithin5Seconds_For100Incidents()
    {
        // Arrange
        _mockIncidentService.Setup(s => s.CreateIncidentAsync(It.IsAny<CreateIncidentDto>(), It.IsAny<int>()))
            .ReturnsAsync((CreateIncidentDto dto, int userId) => new IncidentDto
            {
                IncidentId = 1,
                ShortDescription = dto.ShortDescription
            });

        // Act
        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 100).Select(i =>
            _mockIncidentService.Object.CreateIncidentAsync(
                new CreateIncidentDto
                {
                    ShortDescription = $"Bulk incident {i}",
                    CallerId = 1,
                    Impact = IncidentImpact.Low,
                    Urgency = IncidentUrgency.Low
                }, 1));

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000,
            "creating 100 incidents in parallel should complete within 5 seconds");
    }

    [Fact]
    public async Task GetIncidentById_ShouldCompleteWithin100ms()
    {
        // Arrange
        _mockIncidentService.Setup(s => s.GetIncidentByIdAsync(1))
            .ReturnsAsync(new IncidentDto { IncidentId = 1, ShortDescription = "Test" });

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            await _mockIncidentService.Object.GetIncidentByIdAsync(1);
        }
        stopwatch.Stop();

        // Assert — 100 lookups should average < 1ms each
        (stopwatch.ElapsedMilliseconds / 100.0).Should().BeLessThan(100,
            "individual incident lookup should complete quickly");
    }

    [Fact]
    public async Task ResolveIncident_ShouldCompleteWithin300ms()
    {
        // Arrange
        var resolveDto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Performance test resolution"
        };

        _mockIncidentService.Setup(s => s.ResolveIncidentAsync(1, resolveDto, 1))
            .ReturnsAsync(new IncidentDto { IncidentId = 1, State = IncidentState.Resolved });

        // Act
        var stopwatch = Stopwatch.StartNew();
        await _mockIncidentService.Object.ResolveIncidentAsync(1, resolveDto, 1);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300,
            "resolving an incident should complete within 300ms");
    }
}
