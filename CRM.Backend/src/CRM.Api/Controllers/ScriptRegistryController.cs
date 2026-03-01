// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Interfaces;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API for script lifecycle governance: submit, approve, reject, deploy, and retire.
/// Approval/rejection and deployment require elevated roles (Admin or ScriptApprover).
/// </summary>
[ApiController]
[Route("api/script-registry")]
[Authorize]
public class ScriptRegistryController : CrmControllerBase
{
    private readonly IScriptRegistryService _registryService;

    public ScriptRegistryController(IScriptRegistryService registryService)
    {
        _registryService = registryService;
    }

    /// <summary>Moves a script from Draft → Under Review.</summary>
    [HttpPost("{id:int}/submit-review")]
    public async Task<IActionResult> SubmitForReview(int id, [FromBody] string? notes)
    {
        var result = await _registryService.SubmitForReviewAsync(
            id, User.Identity?.Name ?? "system", notes);
        return result
            ? Ok(new { message = "Submitted for review." })
            : BadRequest(new { error = "Cannot submit: script not in Draft state." });
    }

    /// <summary>Approves a script under review (Admin or ScriptApprover role required).</summary>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,ScriptApprover")]
    public async Task<IActionResult> Approve(int id, [FromBody] string? notes)
    {
        var result = await _registryService.ApproveAsync(
            id, User.Identity?.Name ?? "system", notes);
        return result
            ? Ok(new { message = "Approved." })
            : BadRequest(new { error = "Cannot approve: script not Under Review." });
    }

    /// <summary>Rejects a script under review, returning it to Draft (Admin or ScriptApprover).</summary>
    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin,ScriptApprover")]
    public async Task<IActionResult> Reject(int id, [FromBody] string? notes)
    {
        var result = await _registryService.RejectAsync(
            id, User.Identity?.Name ?? "system", notes);
        return result
            ? Ok(new { message = "Rejected." })
            : BadRequest(new { error = "Cannot reject: script not Under Review." });
    }

    /// <summary>Deploys an approved script to production (Admin only).</summary>
    [HttpPost("{id:int}/deploy")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deploy(int id)
    {
        var result = await _registryService.DeployAsync(
            id, User.Identity?.Name ?? "system");
        return result
            ? Ok(new { message = "Deployed." })
            : BadRequest(new { error = "Cannot deploy: script not in Approved state." });
    }

    /// <summary>Retires a deployed script (Admin only).</summary>
    [HttpPost("{id:int}/retire")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Retire(int id, [FromBody] string? notes)
    {
        var result = await _registryService.RetireAsync(
            id, User.Identity?.Name ?? "system", notes);
        return result
            ? Ok(new { message = "Retired." })
            : BadRequest(new { error = "Cannot retire: script not in Deployed state." });
    }

    /// <summary>Returns the full audit log for a given script, ordered newest-first.</summary>
    [HttpGet("{id:int}/audit-log")]
    public async Task<IActionResult> GetAuditLog(int id)
    {
        var logs = await _registryService.GetAuditLogAsync(id);
        return Ok(logs);
    }

    /// <summary>Returns the last 10 source-code versions for a given script.</summary>
    [HttpGet("{id:int}/versions")]
    public async Task<IActionResult> GetVersions(int id)
    {
        var versions = await _registryService.GetVersionHistoryAsync(id);
        return Ok(versions);
    }
}
