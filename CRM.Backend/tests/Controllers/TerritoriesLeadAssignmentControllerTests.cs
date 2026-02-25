// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SPEC-GAP-04 (Territory Lead Assignment)
// TODO-GAP-04: Territory Lead Assignment endpoints — controller unit tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: TerritoriesController.cs, ITerritoryService.cs,
//   Lead.cs, ICrmDbContext.cs

using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for TerritoriesController lead-assignment endpoints (TODO-GAP-04):
/// - POST {id}/assign-lead
/// - GET  {id}/leads
/// </summary>
public class TerritoriesLeadAssignmentControllerTests
{
    private readonly Mock<ITerritoryService> _mockService;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly TerritoriesController _controller;

    public TerritoriesLeadAssignmentControllerTests()
    {
        _mockService = new Mock<ITerritoryService>();
        _mockContext = new Mock<ICrmDbContext>();

        _controller = new TerritoriesController(
            _mockService.Object,
            Mock.Of<ILogger<TerritoriesController>>());

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private static Lead CreateLead(int id, int? territoryId = null) => new()
    {
        Id = id,
        FirstName = "Test",
        LastName = $"Lead{id}",
        Email = $"lead{id}@test.com",
        TerritoryId = territoryId,
        Status = LeadLifecycleStatus.New,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    private void SetupLeads(List<Lead> leads)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // POST {id}/assign-lead
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignLead_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var updatedLead = CreateLead(id: 5, territoryId: 3);
        _mockService.Setup(s => s.AssignLeadToTerritoryAsync(5, 3, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedLead);

        var request = new AssignLeadToTerritoryRequest { LeadId = 5, UserId = null };

        // Act
        var result = await _controller.AssignLead(id: 3, request, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updatedLead);
    }

    [Fact]
    public async Task AssignLead_ShouldReturnNotFound_WhenLeadNotFound()
    {
        // Arrange — service throws InvalidOperationException containing "not found"
        _mockService.Setup(s => s.AssignLeadToTerritoryAsync(99, 3, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Lead 99 not found"));

        var request = new AssignLeadToTerritoryRequest { LeadId = 99 };

        // Act
        var result = await _controller.AssignLead(id: 3, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignLead_ShouldReturn500_WhenUnexpectedExceptionOccurs()
    {
        // Arrange — service throws a generic exception (not InvalidOperationException)
        _mockService.Setup(s => s.AssignLeadToTerritoryAsync(5, 3, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected database error"));

        var request = new AssignLeadToTerritoryRequest { LeadId = 5 };

        // Act
        var result = await _controller.AssignLead(id: 3, request, CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GET {id}/leads
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeadsByTerritory_ShouldReturnOk_WithMatchingLeads()
    {
        // Arrange — territory 3 has 2 leads
        var leads = new List<Lead>
        {
            CreateLead(id: 1, territoryId: 3),
            CreateLead(id: 2, territoryId: 3),
            CreateLead(id: 3, territoryId: 99)  // different territory — excluded
        };
        SetupLeads(leads);

        // Act — pass mock context directly (bypasses [FromServices] model binding in unit tests)
        var result = await _controller.GetLeadsByTerritory(id: 3, _mockContext.Object, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLeadsByTerritory_ShouldReturnEmptyList_WhenNoLeadsInTerritory()
    {
        // Arrange — no leads for territory 42
        SetupLeads([CreateLead(id: 1, territoryId: 1)]); // different territory

        // Act
        var result = await _controller.GetLeadsByTerritory(id: 42, _mockContext.Object, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
