// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Constants;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class WorkerControlControllerTests
{
    private readonly Mock<ISystemSettingsService> _settingsService = new();
    private readonly Mock<ILogger<WorkerControlController>> _logger = new();

    private WorkerControlController CreateController()
    {
        return new WorkerControlController(_settingsService.Object, _logger.Object);
    }

    [Fact]
    public async Task UpdateMaxWorkers_ShouldRejectInvalidValue()
    {
        var controller = CreateController();

        var result = await controller.UpdateMaxWorkers(new UpdateWorkerMaxInstancesRequest { MaxWorkers = 0 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RestartWorkers_ShouldRequestRestartState()
    {
        _settingsService
            .Setup(service => service.UpdateSettingsAsync(It.IsAny<UpdateSystemSettingsRequest>(), null))
            .ReturnsAsync(new SystemSettingsDto
            {
                WorkerControlState = WorkerControlStates.RestartRequested,
                WorkerMaxInstances = 1
            });

        var controller = CreateController();
        var result = await controller.RestartWorkers();

        result.Result.Should().BeOfType<OkObjectResult>();
        _settingsService.Verify(service => service.UpdateSettingsAsync(
            It.Is<UpdateSystemSettingsRequest>(req => req.WorkerControlState == WorkerControlStates.RestartRequested),
            null),
            Times.Once);
    }
}
