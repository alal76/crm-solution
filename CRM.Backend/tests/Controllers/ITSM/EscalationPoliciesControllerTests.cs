// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers.ITSM;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.ITSM;

/// <summary>
/// Tests for EscalationPoliciesController.
/// </summary>
public class EscalationPoliciesControllerTests
{
    private readonly Mock<IEscalationPolicyService> _mockService;
    private readonly EscalationPoliciesController _controller;

    public EscalationPoliciesControllerTests()
    {
        _mockService = new Mock<IEscalationPolicyService>();
        _controller = new EscalationPoliciesController(_mockService.Object);

        // Set up a mock user context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    #region GetPolicies Tests

    [Fact]
    public async Task GetPolicies_ShouldReturnOk_WhenPoliciesExist()
    {
        // Arrange
        var policies = new List<EscalationPolicyDto>
        {
            new() { Id = 1, Name = "Policy 1", IsActive = true },
            new() { Id = 2, Name = "Policy 2", IsActive = true }
        };

        _mockService.Setup(x => x.GetPoliciesAsync(null))
            .ReturnsAsync(policies);

        // Act
        var result = await _controller.GetPolicies(null);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPolicies = okResult.Value.Should().BeAssignableTo<IEnumerable<EscalationPolicyDto>>().Subject;
        returnedPolicies.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPolicies_ShouldReturnEmptyList_WhenNoPoliciesExist()
    {
        // Arrange
        _mockService.Setup(x => x.GetPoliciesAsync(null))
            .ReturnsAsync(new List<EscalationPolicyDto>());

        // Act
        var result = await _controller.GetPolicies(null);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPolicies = okResult.Value.Should().BeAssignableTo<IEnumerable<EscalationPolicyDto>>().Subject;
        returnedPolicies.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPolicies_ShouldFilterByActiveStatus()
    {
        // Arrange
        var activePolicies = new List<EscalationPolicyDto>
        {
            new() { Id = 1, Name = "Active Policy", IsActive = true }
        };

        _mockService.Setup(x => x.GetPoliciesAsync(true))
            .ReturnsAsync(activePolicies);

        // Act
        var result = await _controller.GetPolicies(true);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPolicies = okResult.Value.Should().BeAssignableTo<IEnumerable<EscalationPolicyDto>>().Subject;
        returnedPolicies.Should().OnlyContain(p => p.IsActive);
    }

    #endregion

    #region GetPolicyById Tests

    [Fact]
    public async Task GetPolicyById_ShouldReturnOk_WhenPolicyExists()
    {
        // Arrange
        var policy = new EscalationPolicyDto
        {
            Id = 1,
            Name = "Test Policy",
            IsActive = true,
            Description = "Test description"
        };

        _mockService.Setup(x => x.GetPolicyByIdAsync(1))
            .ReturnsAsync(policy);

        // Act
        var result = await _controller.GetPolicyById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPolicy = okResult.Value.Should().BeOfType<EscalationPolicyDto>().Subject;
        returnedPolicy.Id.Should().Be(1);
        returnedPolicy.Name.Should().Be("Test Policy");
    }

    [Fact]
    public async Task GetPolicyById_ShouldReturnNotFound_WhenPolicyDoesNotExist()
    {
        // Arrange
        _mockService.Setup(x => x.GetPolicyByIdAsync(999))
            .ReturnsAsync((EscalationPolicyDto?)null);

        // Act
        var result = await _controller.GetPolicyById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Service Verification Tests

    [Fact]
    public async Task GetPolicies_ShouldCallServiceOnce()
    {
        // Arrange
        _mockService.Setup(x => x.GetPoliciesAsync(null))
            .ReturnsAsync(new List<EscalationPolicyDto>());

        // Act
        await _controller.GetPolicies(null);

        // Assert
        _mockService.Verify(x => x.GetPoliciesAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetPolicyById_ShouldCallServiceWithCorrectId()
    {
        // Arrange
        var policy = new EscalationPolicyDto { Id = 42, Name = "Test" };
        _mockService.Setup(x => x.GetPolicyByIdAsync(42))
            .ReturnsAsync(policy);

        // Act
        await _controller.GetPolicyById(42);

        // Assert
        _mockService.Verify(x => x.GetPolicyByIdAsync(42), Times.Once);
    }

    #endregion

    #region DTO Tests

    [Fact]
    public void EscalationPolicyDto_ShouldHaveCorrectStructure()
    {
        // Arrange & Act
        var dto = new EscalationPolicyDto
        {
            Id = 1,
            Name = "Production Escalation",
            Description = "Policy for production issues",
            IsActive = true,
            IsDefault = true,
            Levels = new List<EscalationLevelDto>
            {
                new() { Id = 1, LevelNumber = 1, EscalateAfterMinutes = 30 }
            }
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Production Escalation");
        dto.Description.Should().Be("Policy for production issues");
        dto.IsActive.Should().BeTrue();
        dto.IsDefault.Should().BeTrue();
        dto.Levels.Should().HaveCount(1);
    }

    [Fact]
    public void EscalationLevelDto_ShouldHaveCorrectStructure()
    {
        // Arrange & Act
        var dto = new EscalationLevelDto
        {
            Id = 1,
            PolicyId = 10,
            LevelNumber = 1,
            Name = "First Level",
            EscalateAfterMinutes = 30,
            NotifyUserId = 5,
            NotifyUserName = "Admin User",
            NotifyTeamId = 2,
            NotifyTeamName = "IT Support",
            SendEmail = true,
            SendSms = false
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.PolicyId.Should().Be(10);
        dto.LevelNumber.Should().Be(1);
        dto.Name.Should().Be("First Level");
        dto.EscalateAfterMinutes.Should().Be(30);
        dto.NotifyUserId.Should().Be(5);
        dto.NotifyTeamId.Should().Be(2);
        dto.SendEmail.Should().BeTrue();
        dto.SendSms.Should().BeFalse();
    }

    [Fact]
    public void CreateEscalationPolicyDto_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var dto = new CreateEscalationPolicyDto();

        // Assert
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.IsActive.Should().BeTrue(); // Default per class definition
        dto.IsDefault.Should().BeFalse();
        dto.Levels.Should().BeNull();
    }

    #endregion
}
