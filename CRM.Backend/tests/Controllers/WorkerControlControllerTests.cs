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
