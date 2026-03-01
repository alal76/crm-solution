// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for managing cloud deployments, providers, and health checks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CloudDeploymentController : CrmControllerBase
{
    private readonly ICloudDeploymentService _deploymentService;

    public CloudDeploymentController(
        ICloudDeploymentService deploymentService)
    {
        _deploymentService = deploymentService;
    }

    #region Cloud Providers

    /// <summary>
    /// Get all cloud providers
    /// </summary>
    [HttpGet("providers")]
    [ProducesResponseType(typeof(IEnumerable<CloudProviderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CloudProviderDto>>> GetProviders()
    {
                var providers = await _deploymentService.GetProvidersAsync();
        return Ok(providers);
    }

    /// <summary>
    /// Get a specific cloud provider
    /// </summary>
    [HttpGet("providers/{id}")]
    [ProducesResponseType(typeof(CloudProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CloudProviderDto>> GetProvider(int id)
    {
                var provider = await _deploymentService.GetProviderByIdAsync(id);
        if (provider == null)
        {
            return NotFound($"Provider {id} not found");
        }
        return Ok(provider);
    }

    /// <summary>
    /// Create a new cloud provider
    /// </summary>
    [HttpPost("providers")]
    [ProducesResponseType(typeof(CloudProviderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CloudProviderDto>> CreateProvider([FromBody] CreateCloudProviderRequest request)
    {
                var provider = await _deploymentService.CreateProviderAsync(request);
        return CreatedAtAction(nameof(GetProvider), new { id = provider.Id }, provider);
    }

    /// <summary>
    /// Update a cloud provider
    /// </summary>
    [HttpPut("providers/{id}")]
    [ProducesResponseType(typeof(CloudProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CloudProviderDto>> UpdateProvider(int id, [FromBody] UpdateCloudProviderRequest request)
    {
        try
        {
            var provider = await _deploymentService.UpdateProviderAsync(id, request);
            return Ok(provider);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Provider {id} not found");
        }
    }

    /// <summary>
    /// Delete a cloud provider
    /// </summary>
    [HttpDelete("providers/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProvider(int id)
    {
                var deleted = await _deploymentService.DeleteProviderAsync(id);
        if (!deleted)
        {
            return NotFound($"Provider {id} not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Test connection to a cloud provider
    /// </summary>
    [HttpPost("providers/test")]
    [ProducesResponseType(typeof(ProviderConnectionResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderConnectionResult>> TestProviderConnection([FromBody] TestProviderConnectionRequest request)
    {
                var result = await _deploymentService.TestProviderConnectionAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get available resources for a provider
    /// </summary>
    [HttpGet("providers/{id}/resources/{resourceType}")]
    [ProducesResponseType(typeof(IEnumerable<ResourceOption>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResourceOption>>> GetProviderResources(int id, string resourceType)
    {
                var resources = await _deploymentService.GetProviderResourcesAsync(id, resourceType);
        return Ok(resources);
    }

    #endregion

    #region Deployments

    /// <summary>
    /// Get all deployments
    /// </summary>
    [HttpGet("deployments")]
    [ProducesResponseType(typeof(IEnumerable<CloudDeploymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CloudDeploymentDto>>> GetDeployments(
        [FromQuery] int? providerId = null,
        [FromQuery] string? status = null)
    {
                var deployments = await _deploymentService.GetDeploymentsAsync(providerId, status);
        return Ok(deployments);
    }

    /// <summary>
    /// Get a specific deployment
    /// </summary>
    [HttpGet("deployments/{id}")]
    [ProducesResponseType(typeof(CloudDeploymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CloudDeploymentDto>> GetDeployment(int id)
    {
                var deployment = await _deploymentService.GetDeploymentByIdAsync(id);
        if (deployment == null)
        {
            return NotFound($"Deployment {id} not found");
        }
        return Ok(deployment);
    }

    /// <summary>
    /// Create a new deployment
    /// </summary>
    [HttpPost("deployments")]
    [ProducesResponseType(typeof(CloudDeploymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CloudDeploymentDto>> CreateDeployment([FromBody] CreateDeploymentRequest request)
    {
        try
        {
            var deployment = await _deploymentService.CreateDeploymentAsync(request);
            return CreatedAtAction(nameof(GetDeployment), new { id = deployment.Id }, deployment);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a deployment
    /// </summary>
    [HttpPut("deployments/{id}")]
    [ProducesResponseType(typeof(CloudDeploymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CloudDeploymentDto>> UpdateDeployment(int id, [FromBody] UpdateDeploymentRequest request)
    {
        try
        {
            var deployment = await _deploymentService.UpdateDeploymentAsync(id, request);
            return Ok(deployment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Deployment {id} not found");
        }
    }

    /// <summary>
    /// Delete a deployment
    /// </summary>
    [HttpDelete("deployments/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDeployment(int id)
    {
                var deleted = await _deploymentService.DeleteDeploymentAsync(id);
        if (!deleted)
        {
            return NotFound($"Deployment {id} not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Trigger a deployment
    /// </summary>
    [HttpPost("deployments/{id}/deploy")]
    [ProducesResponseType(typeof(DeploymentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentResult>> TriggerDeployment(int id, [FromBody] TriggerDeploymentRequest request)
    {
                // Get user ID from token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            request.TriggeredByUserId = userId;
        }

        var result = await _deploymentService.TriggerDeploymentAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Stop a deployment
    /// </summary>
    [HttpPost("deployments/{id}/stop")]
    [ProducesResponseType(typeof(DeploymentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentResult>> StopDeployment(int id)
    {
                var result = await _deploymentService.StopDeploymentAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Restart a deployment
    /// </summary>
    [HttpPost("deployments/{id}/restart")]
    [ProducesResponseType(typeof(DeploymentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentResult>> RestartDeployment(int id)
    {
                var result = await _deploymentService.RestartDeploymentAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Scale a deployment
    /// </summary>
    [HttpPost("deployments/{id}/scale")]
    [ProducesResponseType(typeof(DeploymentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentResult>> ScaleDeployment(int id, [FromQuery] int replicas)
    {
                var result = await _deploymentService.ScaleDeploymentAsync(id, replicas);
        return Ok(result);
    }

    #endregion

    #region Deployment Attempts

    /// <summary>
    /// Get deployment attempts for a deployment
    /// </summary>
    [HttpGet("deployments/{deploymentId}/attempts")]
    [ProducesResponseType(typeof(IEnumerable<DeploymentAttemptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeploymentAttemptDto>>> GetDeploymentAttempts(int deploymentId)
    {
                var attempts = await _deploymentService.GetDeploymentAttemptsAsync(deploymentId);
        return Ok(attempts);
    }

    /// <summary>
    /// Get a specific deployment attempt
    /// </summary>
    [HttpGet("attempts/{attemptId}")]
    [ProducesResponseType(typeof(DeploymentAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeploymentAttemptDto>> GetDeploymentAttempt(int attemptId)
    {
                var attempt = await _deploymentService.GetDeploymentAttemptByIdAsync(attemptId);
        if (attempt == null)
        {
            return NotFound($"Attempt {attemptId} not found");
        }
        return Ok(attempt);
    }

    /// <summary>
    /// Get logs for a deployment attempt
    /// </summary>
    [HttpGet("attempts/{attemptId}/logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> GetDeploymentAttemptLogs(int attemptId)
    {
                var logs = await _deploymentService.GetDeploymentAttemptLogsAsync(attemptId);
        return Ok(new { logs });
    }

    #endregion

    #region Health Checks

    /// <summary>
    /// Run health check on a deployment
    /// </summary>
    [HttpPost("deployments/{deploymentId}/health-check")]
    [ProducesResponseType(typeof(HealthCheckResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthCheckResult>> RunHealthCheck(int deploymentId)
    {
                var result = await _deploymentService.RunHealthCheckAsync(deploymentId);
        return Ok(result);
    }

    /// <summary>
    /// Get health check history for a deployment
    /// </summary>
    [HttpGet("deployments/{deploymentId}/health-history")]
    [ProducesResponseType(typeof(IEnumerable<HealthCheckDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetHealthCheckHistory(
        int deploymentId,
        [FromQuery] int? limit = 20)
    {
                var history = await _deploymentService.GetHealthCheckHistoryAsync(deploymentId, limit);
        return Ok(history);
    }

    /// <summary>
    /// Get health status for all deployments
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(IEnumerable<HealthCheckDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetAllDeploymentHealth()
    {
                var health = await _deploymentService.GetAllDeploymentHealthAsync();
        return Ok(health);
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// Get deployment dashboard summary
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DeploymentDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeploymentDashboardDto>> GetDashboard()
    {
                var dashboard = await _deploymentService.GetDashboardAsync();
        return Ok(dashboard);
    }

    #endregion
}
