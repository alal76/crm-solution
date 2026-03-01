// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for CI/CD pipeline integration with change management.
/// </summary>
[ApiController]
[Route("api/itsm/cicd")]
[Tags("ITSM - CI/CD Integration")]
public class CICDIntegrationController : CrmControllerBase
{
    private readonly ICICDIntegrationService _cicdService;
    private readonly ILogger<CICDIntegrationController> _logger;

    public CICDIntegrationController(
        ICICDIntegrationService cicdService,
        ILogger<CICDIntegrationController> logger)
    {
        _cicdService = cicdService;
        _logger = logger;
    }

    /// <summary>
    /// Create a change request from a deployment.
    /// </summary>
    [HttpPost("deployments")]
    [AllowAnonymous] // Uses API key authentication
    [ProducesResponseType(typeof(DeploymentChangeResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentChangeResult>> CreateDeploymentChange(
        [FromBody] DeploymentChangeRequestDto request,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        _logger.LogInformation("Received deployment request from {PipelineName}", request.PipelineName);
        var result = await _cicdService.CreateDeploymentChangeAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Update deployment status.
    /// </summary>
    [HttpPut("deployments/{changeId}/status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeploymentChangeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeploymentChangeResult>> UpdateDeploymentStatus(
        int changeId,
        [FromBody] DeploymentStatusUpdateDto update,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        var result = await _cicdService.UpdateDeploymentStatusAsync(changeId, update);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get deployment history.
    /// </summary>
    [HttpGet("deployments")]
    [Authorize]
    [ProducesResponseType(typeof(List<DeploymentHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DeploymentHistoryDto>>> GetDeploymentHistory(
        [FromQuery] string? environment,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var history = await _cicdService.GetDeploymentHistoryAsync(environment, startDate, endDate);
        return Ok(history);
    }

    /// <summary>
    /// Validate deployment prerequisites.
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeploymentValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentValidationResult>> ValidateDeployment(
        [FromBody] DeploymentValidationRequestDto request,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        var result = await _cicdService.ValidateDeploymentAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Register a new CI/CD pipeline.
    /// </summary>
    [HttpPost("pipelines")]
    [Authorize]
    [ProducesResponseType(typeof(PipelineRegistrationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PipelineRegistrationDto>> RegisterPipeline(
        [FromBody] RegisterPipelineDto request)
    {
        var pipeline = await _cicdService.RegisterPipelineAsync(request);
        return CreatedAtAction(nameof(GetPipeline), new { id = pipeline.Id }, pipeline);
    }

    /// <summary>
    /// Get all registered pipelines.
    /// </summary>
    [HttpGet("pipelines")]
    [Authorize]
    [ProducesResponseType(typeof(List<PipelineRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PipelineRegistrationDto>>> GetPipelines()
    {
        var pipelines = await _cicdService.GetPipelinesAsync();
        return Ok(pipelines);
    }

    /// <summary>
    /// Get a specific pipeline.
    /// </summary>
    [HttpGet("pipelines/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PipelineRegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PipelineRegistrationDto>> GetPipeline(int id)
    {
        var pipeline = await _cicdService.GetPipelineAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        return Ok(pipeline);
    }

    /// <summary>
    /// Delete a pipeline.
    /// </summary>
    [HttpDelete("pipelines/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePipeline(int id)
    {
        var deleted = await _cicdService.DeletePipelineAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Webhook endpoint for Azure DevOps.
    /// </summary>
    [HttpPost("webhooks/azure-devops")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> AzureDevOpsWebhook(
        [FromBody] AzureDevOpsWebhookPayload payload,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        _logger.LogInformation("Received Azure DevOps webhook: {EventType}", payload.EventType);

        // Transform Azure DevOps payload to deployment request
        var request = new DeploymentChangeRequestDto
        {
            PipelineId = payload.Resource?.Pipeline?.Id?.ToString() ?? "",
            PipelineName = payload.Resource?.Pipeline?.Name ?? "Azure DevOps Pipeline",
            BuildNumber = payload.Resource?.Build?.BuildNumber ?? "",
            CommitHash = payload.Resource?.SourceVersion ?? "",
            Author = payload.Resource?.RequestedFor?.DisplayName ?? "Unknown",
            Branch = payload.Resource?.SourceBranch ?? "main",
            Environment = "staging",
            DeploymentType = DeploymentType.Standard
        };

        var result = await _cicdService.CreateDeploymentChangeAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Webhook endpoint for GitHub Actions.
    /// </summary>
    [HttpPost("webhooks/github")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GitHubWebhook(
        [FromBody] GitHubWebhookPayload payload,
        [FromHeader(Name = "X-Hub-Signature-256")] string? signature)
    {
        _logger.LogInformation("Received GitHub webhook: {Action} for {Repository}", payload.Action, payload.Repository?.FullName);

        if (payload.Action == "completed" && payload.WorkflowRun != null)
        {
            var request = new DeploymentChangeRequestDto
            {
                PipelineId = payload.WorkflowRun.WorkflowId.ToString(),
                PipelineName = payload.WorkflowRun.Name ?? "GitHub Actions",
                BuildNumber = payload.WorkflowRun.RunNumber.ToString(),
                CommitHash = payload.WorkflowRun.HeadSha ?? "",
                Author = payload.Sender?.Login ?? "Unknown",
                Branch = payload.WorkflowRun.HeadBranch ?? "main",
                Environment = "staging",
                DeploymentType = DeploymentType.Standard
            };

            var result = await _cicdService.CreateDeploymentChangeAsync(request);
            return Ok(result);
        }

        return Ok(new { message = "Webhook received" });
    }

    /// <summary>
    /// Create a deployment change request (singular route).
    /// </summary>
    [HttpPost("deployment")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeploymentChangeResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> CreateDeploymentSingular([FromBody] DeploymentChangeRequestDto request)
    {
        var result = await _cicdService.CreateDeploymentChangeAsync(request);
        return Ok(new { changeRequestId = result.ChangeId, message = result.Message, status = result.Status });
    }

    /// <summary>
    /// Mark a deployment as complete.
    /// </summary>
    [HttpPost("deployment-complete")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeploymentChangeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkDeploymentComplete(
        [FromBody] DeploymentStatusUpdateDto request,
        [FromQuery] int? changeId = null)
    {
        if (changeId == null || changeId <= 0)
        {
            _logger.LogWarning("deployment-complete called without a valid changeId query parameter");
            return BadRequest(new { message = "Query parameter 'changeId' is required" });
        }

        request.Status = string.IsNullOrEmpty(request.Status) ? "Completed" : request.Status;
        request.CompletedAt ??= DateTime.UtcNow;

        var result = await _cicdService.UpdateDeploymentStatusAsync(changeId.Value, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

// Azure DevOps Webhook Payload
public class AzureDevOpsWebhookPayload
{
    public string EventType { get; set; } = string.Empty;
    public AzureDevOpsResource? Resource { get; set; }
}

public class AzureDevOpsResource
{
    public AzureDevOpsPipeline? Pipeline { get; set; }
    public AzureDevOpsBuild? Build { get; set; }
    public string? SourceVersion { get; set; }
    public string? SourceBranch { get; set; }
    public AzureDevOpsUser? RequestedFor { get; set; }
}

public class AzureDevOpsPipeline
{
    public int? Id { get; set; }
    public string? Name { get; set; }
}

public class AzureDevOpsBuild
{
    public string? BuildNumber { get; set; }
}

public class AzureDevOpsUser
{
    public string? DisplayName { get; set; }
}

// GitHub Webhook Payload
public class GitHubWebhookPayload
{
    public string? Action { get; set; }
    public GitHubRepository? Repository { get; set; }
    public GitHubWorkflowRun? WorkflowRun { get; set; }
    public GitHubUser? Sender { get; set; }
}

public class GitHubRepository
{
    public string? FullName { get; set; }
}

public class GitHubWorkflowRun
{
    public int WorkflowId { get; set; }
    public string? Name { get; set; }
    public int RunNumber { get; set; }
    public string? HeadSha { get; set; }
    public string? HeadBranch { get; set; }
    public string? Conclusion { get; set; }
}

public class GitHubUser
{
    public string? Login { get; set; }
}
