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

using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.DTOs.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class WorkflowTasksControllerTests
{
    private readonly Mock<IWorkflowInstanceService> _instanceService = new();
    private readonly Mock<ILogger<WorkflowTasksController>> _logger = new();

    private WorkflowTasksController CreateControllerWithUser(int userId = 1)
    {
        var controller = new WorkflowTasksController(_instanceService.Object, _logger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, "User")
                }, "Test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetTasks_ReturnsOk()
    {
        _instanceService
            .Setup(s => s.GetHumanTasksForUserAsync(1, It.IsAny<string[]?>()))
            .ReturnsAsync(new List<WorkflowTask>
            {
                new()
                {
                    Id = 10,
                    WorkflowNodeId = 1,
                    Name = "Approve",
                    Status = WorkflowTaskStatus.Pending,
                    TaskType = WorkflowTaskType.Human,
                    CreatedAt = DateTime.UtcNow
                }
            });

        var controller = CreateControllerWithUser(1);

        var result = await controller.GetTasks();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenMissing()
    {
        _instanceService
            .Setup(s => s.GetHumanTasksForUserAsync(1, It.IsAny<string[]?>()))
            .ReturnsAsync(new List<WorkflowTask>());

        var controller = CreateControllerWithUser(1);

        var result = await controller.GetTask(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CompleteTask_ReturnsBadRequest_WhenNotAssigned()
    {
        _instanceService
            .Setup(s => s.CompleteHumanTaskAsync(10, 1, It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(false);

        var controller = CreateControllerWithUser(1);

        var result = await controller.CompleteTask(10, new CompleteTaskDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ReassignTask_ReturnsOk_WhenSuccess()
    {
        _instanceService
            .Setup(s => s.ClaimTaskAsync(10, 2))
            .ReturnsAsync(true);

        var controller = CreateControllerWithUser(1);

        var result = await controller.ReassignTask(10, 2);

        result.Should().BeOfType<OkObjectResult>();
    }
}
