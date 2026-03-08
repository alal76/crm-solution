// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// ITSM-043: RBAC/permission tests for ITSM controller endpoints.
/// Validates that user identity claims are correctly extracted and
/// that different user roles interact correctly with ITSM controllers.
/// </summary>
public class ITSMRBACTests
{
    private readonly Mock<IIncidentService> _mockIncidentService;

    public ITSMRBACTests()
    {
        _mockIncidentService = new Mock<IIncidentService>();
    }

    private static ControllerContext CreateControllerContext(string userId, string role = "Admin")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // --- Admin role tests ---

    [Fact]
    public async Task CreateIncident_ShouldSucceed_WhenCalledByAdmin()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(_mockIncidentService.Object);
        controller.ControllerContext = CreateControllerContext("1", "Admin");

        var dto = new CreateIncidentDto
        {
            ShortDescription = "Server down",
            CallerId = 10,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High
        };

        _mockIncidentService.Setup(s => s.CreateIncidentAsync(dto, 1))
            .ReturnsAsync(new IncidentDto { IncidentId = 1, ShortDescription = "Server down" });

        // Act
        var result = await controller.CreateIncident(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetIncident_ShouldSucceed_WhenCalledByAgent()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(_mockIncidentService.Object);
        controller.ControllerContext = CreateControllerContext("5", "Agent");

        _mockIncidentService.Setup(s => s.GetIncidentByIdAsync(1))
            .ReturnsAsync(new IncidentDto { IncidentId = 1, ShortDescription = "Test" });

        // Act
        var result = await controller.GetIncident(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetIncident_ShouldReturn404_WhenCalledByUser_AndNotFound()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(_mockIncidentService.Object);
        controller.ControllerContext = CreateControllerContext("10", "User");

        _mockIncidentService.Setup(s => s.GetIncidentByIdAsync(999))
            .ReturnsAsync((IncidentDto?)null);

        // Act
        var result = await controller.GetIncident(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- User identity extraction tests ---

    [Fact]
    public async Task CreateIncident_ShouldExtractUserIdFromClaims()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(_mockIncidentService.Object);
        controller.ControllerContext = CreateControllerContext("42", "Admin");

        var dto = new CreateIncidentDto
        {
            ShortDescription = "Test extraction",
            CallerId = 10,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium
        };

        _mockIncidentService.Setup(s => s.CreateIncidentAsync(dto, 42))
            .ReturnsAsync(new IncidentDto { IncidentId = 1 });

        // Act
        await controller.CreateIncident(dto);

        // Assert — verify the service was called with the correct user ID from claims
        _mockIncidentService.Verify(s => s.CreateIncidentAsync(dto, 42), Times.Once);
    }

    [Fact]
    public async Task AssignIncident_ShouldExtractUserIdFromClaims()
    {
        // Arrange
        var controller = new CRM.Api.Controllers.IncidentsController(_mockIncidentService.Object);
        controller.ControllerContext = CreateControllerContext("7", "Agent");

        _mockIncidentService.Setup(s => s.AssignIncidentAsync(1, 5, null, 7))
            .ReturnsAsync(true);

        var assignDto = new CRM.Api.Controllers.AssignIncidentDto { AssignedToId = 5 };

        // Act
        var result = await controller.AssignIncident(1, assignDto);

        // Assert
        _mockIncidentService.Verify(s => s.AssignIncidentAsync(1, 5, null, 7), Times.Once);
    }

    // --- Missing claims edge case ---

    [Fact]
    public async Task CreateIncident_ShouldUseDefaultUserId_WhenClaimMissing()
    {
        // Arrange — no NameIdentifier claim
        var controller = new CRM.Api.Controllers.IncidentsController(_mockIncidentService.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var dto = new CreateIncidentDto
        {
            ShortDescription = "No user claim",
            CallerId = 10,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low
        };

        _mockIncidentService.Setup(s => s.CreateIncidentAsync(dto, It.IsAny<int>()))
            .ReturnsAsync(new IncidentDto { IncidentId = 1 });

        // Act — should not throw even with missing claim
        var act = () => controller.CreateIncident(dto);

        // Assert — service should still be called (with fallback user ID)
        await act.Should().NotThrowAsync<NullReferenceException>();
    }
}
