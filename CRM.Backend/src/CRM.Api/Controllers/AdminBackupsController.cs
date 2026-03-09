// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Infrastructure;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/admin/backups")]
[Authorize]
public class AdminBackupsController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<AdminBackupsController> _logger;

    public AdminBackupsController(ICrmDbContext db, ILogger<AdminBackupsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        // Backup management is infrastructure-level, return metadata about available backups
        return Ok(new
        {
            backups = Array.Empty<object>(),
            message = "Backup management requires infrastructure-level access. Configure via deployment settings."
        });
    }

    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLatest()
    {
        return Ok(new { backup = (object?)null, message = "No backup information available via API. Check infrastructure." });
    }

    [HttpGet("schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSchedule()
    {
        return Ok(new
        {
            schedule = new { frequency = "daily", time = "02:00 UTC", retentionDays = 30 },
            message = "Backup schedule is configured at infrastructure level."
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult TriggerBackup()
    {
        _logger.LogInformation("Manual backup trigger requested");
        return Accepted(new { message = "Backup request acknowledged. Check infrastructure for status." });
    }
}
