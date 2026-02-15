// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.ITSM;

public class EscalationPoliciesControllerTests
{
    private readonly Mock<IEscalationPolicyService> _mockService;
    private readonly Mock<ILogger<EscalationPoliciesController>> _mockLogger;
    private readonly EscalationPoliciesController _controller;

    public EscalationPoliciesControllerTests()
    {
        _mockService = new Mock<IEscalationPolicyService>();
        _mockLogger = new Mock<ILogger<EscalationPoliciesController>>();
        _controller = new EscalationPoliciesController(_mockService.Object, _mockLogger.Object);
    }

    #region GET Tests

    [Fact]
    public async Task GetPolicies_ShouldReturnPolicies_WhenPoliciesExist()
    {
        // Arrange
        var policies = new List<EscalationPolicyDto>
        {
            new EscalationPolicyDto
            {
                Id = 1,
                Name = "Test Policy",
                IsActive = true,
                IsDefault = false
            }
        };

        _mockService.Setup(x => x.GetPoliciesAsync(null, default))
            .ReturnsAsync(policies);

        // Act
        var result = await _controller.GetPolicies(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockService.Verify(x => x.GetPoliciesAsync(null, default), Times.Once);
    }

    [Fact]
    public async Task GetPolicyById_ShouldReturnPolicy_WhenPolicyExists()
    {
        // Arrange
        var policy = new EscalationPolicyDto
        {
            Id = 1,
            Name = "Test Policy",
            IsActive = true
        };

        _mockService.Setup(x => x.GetPolicyByIdAsync(1, default))
            .ReturnsAsync(policy);

        // Act
        var result = await _controller.GetPolicyById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull((EscalationPolicyDto)okResult.Value);
        _mockService.Verify(x => x.GetPolicyByIdAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task GetPolicyById_ShouldReturn404_WhenPolicyNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.GetPolicyByIdAsync(999, default))
            .ReturnsAsync((EscalationPolicyDto)null);

        // Act
        var result = await _controller.GetPolicyById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region CREATE Tests

    [Fact]
    public async Task CreatePolicy_ShouldReturn201_WhenValid()
    {
        // Arrange
        var dto = new CreateEscalationPolicyDto
        {
            Name = "New Policy",
            IsActive = true
        };

        var createdPolicy = new EscalationPolicyDto { Id = 1, Name = "New Policy" };

        _mockService.Setup(x => x.CreatePolicyAsync(dto, 1, default))
            .ReturnsAsync(createdPolicy);

        // Act
        var result = await _controller.CreatePolicy(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EscalationPoliciesController.GetPolicyById), createdResult.ActionName);
        _mockService.Verify(x => x.CreatePolicyAsync(dto, 1, default), Times.Once);
    }

    #endregion

    #region UPDATE Tests

    [Fact]
    public async Task UpdatePolicy_ShouldReturnUpdatedPolicy_WhenValid()
    {
        // Arrange
        var dto = new UpdateEscalationPolicyDto { Name = "Updated Policy" };
        var updated = new EscalationPolicyDto { Id = 1, Name = "Updated Policy" };

        _mockService.Setup(x => x.UpdatePolicyAsync(1, dto, 1, default))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdatePolicy(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.UpdatePolicyAsync(1, dto, 1, default), Times.Once);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeletePolicy_ShouldReturn204_WhenPolicyDeleted()
    {
        // Arrange
        _mockService.Setup(x => x.DeletePolicyAsync(1, default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePolicy(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(x => x.DeletePolicyAsync(1, default), Times.Once);
    }

    #endregion

    #region Level Management Tests

    [Fact]
    public async Task AddLevel_ShouldReturn201_WhenValid()
    {
        // Arrange
        var dto = new CreateEscalationLevelDto
        {
            LevelNumber = 1,
            Name = "Level 1",
            EscalateAfterMinutes = 15
        };

        var level = new EscalationLevelDto { Id = 1, LevelNumber = 1, Name = "Level 1" };

        _mockService.Setup(x => x.AddPolicyLevelAsync(1, dto, default))
            .ReturnsAsync(level);

        // Act
        var result = await _controller.AddLevel(1, dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        _mockService.Verify(x => x.AddPolicyLevelAsync(1, dto, default), Times.Once);
    }

    #endregion

    #region Escalation Tests

    [Fact]
    public async Task ExecuteEscalation_ShouldReturnHistory_WhenValid()
    {
        // Arrange
        var history = new EscalationHistoryDto
        {
            Id = 1,
            PolicyId = 1,
            ServiceRequestId = 1,
            EscalationLevel = 1,
            Status = "Completed"
        };

        _mockService.Setup(x => x.ExecuteEscalationAsync(1, 1, 1, default))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.ExecuteEscalation(1, 1, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.ExecuteEscalationAsync(1, 1, 1, default), Times.Once);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnHistory_WhenRecordsExist()
    {
        // Arrange
        var history = new List<EscalationHistoryDto>
        {
            new EscalationHistoryDto { Id = 1, ServiceRequestId = 1, EscalationLevel = 1 }
        };

        _mockService.Setup(x => x.GetHistoryAsync(1, default))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetHistory(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.GetHistoryAsync(1, default), Times.Once);
    }

    #endregion
}
