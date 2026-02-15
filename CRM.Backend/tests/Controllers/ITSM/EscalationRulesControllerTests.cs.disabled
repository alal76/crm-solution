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

public class EscalationRulesControllerTests
{
    private readonly Mock<IEscalationRuleService> _mockService;
    private readonly Mock<ILogger<EscalationRulesController>> _mockLogger;
    private readonly EscalationRulesController _controller;

    public EscalationRulesControllerTests()
    {
        _mockService = new Mock<IEscalationRuleService>();
        _mockLogger = new Mock<ILogger<EscalationRulesController>>();
        _controller = new EscalationRulesController(_mockService.Object, _mockLogger.Object);
    }

    #region GET Tests

    [Fact]
    public async Task GetRules_ShouldReturnRules_WhenRulesExist()
    {
        // Arrange
        var filter = new EscalationRuleFilterDto { PageNumber = 1, PageSize = 10 };
        var rules = new List<EscalationRuleDto>
        {
            new EscalationRuleDto
            {
                Id = 1,
                Name = "Test Rule",
                SLAPolicyId = 1,
                TriggerAtPercent = 75,
                IsActive = true,
                ExecutionOrder = 0
            }
        };

        _mockService.Setup(x => x.GetRulesAsync(filter, default))
            .ReturnsAsync((rules, 1));

        // Act
        var result = await _controller.GetRules(filter);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockService.Verify(x => x.GetRulesAsync(filter, default), Times.Once);
    }

    [Fact]
    public async Task GetRuleById_ShouldReturnRule_WhenRuleExists()
    {
        // Arrange
        var rule = new EscalationRuleDto
        {
            Id = 1,
            Name = "Test Rule",
            SLAPolicyId = 1,
            IsActive = true
        };

        _mockService.Setup(x => x.GetRuleByIdAsync(1, default))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.GetRuleById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull((EscalationRuleDto)okResult.Value);
        _mockService.Verify(x => x.GetRuleByIdAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task GetRuleById_ShouldReturn404_WhenRuleNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.GetRuleByIdAsync(999, default))
            .ReturnsAsync((EscalationRuleDto)null);

        // Act
        var result = await _controller.GetRuleById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region CREATE Tests

    [Fact]
    public async Task CreateRule_ShouldReturn201_WhenValid()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            SLAPolicyId = 1,
            Name = "New Rule",
            TriggerAtPercent = 75,
            IsActive = true
        };

        var createdRule = new EscalationRuleDto { Id = 1, Name = "New Rule" };

        _mockService.Setup(x => x.CreateRuleAsync(dto, 1, default))
            .ReturnsAsync(createdRule);

        // Act
        var result = await _controller.CreateRule(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EscalationRulesController.GetRuleById), createdResult.ActionName);
        _mockService.Verify(x => x.CreateRuleAsync(dto, 1, default), Times.Once);
    }

    [Fact]
    public async Task CreateRule_ShouldReturn400_WhenInvalid()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto(); // Invalid - missing required fields
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.CreateRule(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region UPDATE Tests

    [Fact]
    public async Task UpdateRule_ShouldReturnUpdatedRule_WhenValid()
    {
        // Arrange
        var dto = new UpdateEscalationRuleDto { Name = "Updated Rule" };
        var updated = new EscalationRuleDto { Id = 1, Name = "Updated Rule" };

        _mockService.Setup(x => x.UpdateRuleAsync(1, dto, 1, default))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateRule(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.UpdateRuleAsync(1, dto, 1, default), Times.Once);
    }

    [Fact]
    public async Task UpdateRule_ShouldReturn404_WhenRuleNotFound()
    {
        // Arrange
        var dto = new UpdateEscalationRuleDto { Name = "Updated" };

        _mockService.Setup(x => x.UpdateRuleAsync(999, dto, 1, default))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.UpdateRule(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeleteRule_ShouldReturn204_WhenRuleDeleted()
    {
        // Arrange
        _mockService.Setup(x => x.DeleteRuleAsync(1, default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteRule(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(x => x.DeleteRuleAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task DeleteRule_ShouldReturn404_WhenRuleNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.DeleteRuleAsync(999, default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteRule(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region Enable/Disable Tests

    [Fact]
    public async Task EnableRule_ShouldReturnEnabledRule_WhenSuccess()
    {
        // Arrange
        var rule = new EscalationRuleDto { Id = 1, IsActive = true };
        _mockService.Setup(x => x.EnableRuleAsync(1, 1, default))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.EnableRule(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.EnableRuleAsync(1, 1, default), Times.Once);
    }

    [Fact]
    public async Task DisableRule_ShouldReturnDisabledRule_WhenSuccess()
    {
        // Arrange
        var rule = new EscalationRuleDto { Id = 1, IsActive = false };
        _mockService.Setup(x => x.DisableRuleAsync(1, 1, default))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.DisableRule(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.DisableRuleAsync(1, 1, default), Times.Once);
    }

    #endregion
}
