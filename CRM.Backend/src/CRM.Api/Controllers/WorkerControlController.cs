// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Constants;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin control endpoints for worker operations.
/// </summary>
[ApiController]
[Route("api/workers/control")]
[Authorize(Roles = "Admin")]
public class WorkerControlController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;
    private readonly ILogger<WorkerControlController> _logger;

    public WorkerControlController(ISystemSettingsService settingsService, ILogger<WorkerControlController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WorkerControlStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkerControlStatusDto>> GetControlStatus()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(ToStatusDto(settings));
    }

    [HttpPut("max-workers")]
    [ProducesResponseType(typeof(WorkerControlStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkerControlStatusDto>> UpdateMaxWorkers([FromBody] UpdateWorkerMaxInstancesRequest request)
    {
        if (request.MaxWorkers < 1)
        {
            return BadRequest(new { error = "MaxWorkers must be at least 1." });
        }

        var updated = await _settingsService.UpdateSettingsAsync(new UpdateSystemSettingsRequest
        {
            WorkerMaxInstances = request.MaxWorkers
        });

        return Ok(ToStatusDto(updated));
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(WorkerControlStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkerControlStatusDto>> StartWorkers()
    {
        var updated = await _settingsService.UpdateSettingsAsync(new UpdateSystemSettingsRequest
        {
            WorkerControlState = WorkerControlStates.Running
        });

        _logger.LogInformation("Worker start requested");
        return Ok(ToStatusDto(updated));
    }

    [HttpPost("stop")]
    [ProducesResponseType(typeof(WorkerControlStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkerControlStatusDto>> StopWorkers()
    {
        var updated = await _settingsService.UpdateSettingsAsync(new UpdateSystemSettingsRequest
        {
            WorkerControlState = WorkerControlStates.StopRequested
        });

        _logger.LogWarning("Worker stop requested");
        return Ok(ToStatusDto(updated));
    }

    [HttpPost("restart")]
    [ProducesResponseType(typeof(WorkerControlStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkerControlStatusDto>> RestartWorkers()
    {
        var updated = await _settingsService.UpdateSettingsAsync(new UpdateSystemSettingsRequest
        {
            WorkerControlState = WorkerControlStates.RestartRequested
        });

        _logger.LogWarning("Worker restart requested");
        return Ok(ToStatusDto(updated));
    }

    private static WorkerControlStatusDto ToStatusDto(SystemSettingsDto settings)
    {
        return new WorkerControlStatusDto
        {
            ControlState = settings.WorkerControlState,
            MaxWorkers = settings.WorkerMaxInstances,
            Timestamp = DateTime.UtcNow
        };
    }
}
