using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for managing cloud deployments, providers, and health checks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CloudDeploymentController : ControllerBase
{
    private readonly ICloudDeploymentService _deploymentService;
    private readonly ILogger<CloudDeploymentController> _logger;

    public CloudDeploymentController(
        ICloudDeploymentService deploymentService,
        ILogger<CloudDeploymentController> logger)
    {
        _deploymentService = deploymentService;
        _logger = logger;
    }

    #region Cloud Providers

    /// <summary>
    /// Get all cloud providers
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<IEnumerable<CloudProviderDto>>> GetProviders()
    {
        try
        {
            var providers = await _deploymentService.GetProvidersAsync();
            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cloud providers");
            return StatusCode(500, "Error retrieving cloud providers");
        }
    }

    /// <summary>
    /// Get a specific cloud provider
    /// </summary>
    [HttpGet("providers/{id}")]
    public async Task<ActionResult<CloudProviderDto>> GetProvider(int id)
    {
        try
        {
            var provider = await _deploymentService.GetProviderByIdAsync(id);
            if (provider == null)
            {
                return NotFound($"Provider {id} not found");
            }
            return Ok(provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider {Id}", id);
            return StatusCode(500, "Error retrieving provider");
        }
    }

    /// <summary>
    /// Create a new cloud provider
    /// </summary>
    [HttpPost("providers")]
    public async Task<ActionResult<CloudProviderDto>> CreateProvider([FromBody] CreateCloudProviderRequest request)
    {
        try
        {
            var provider = await _deploymentService.CreateProviderAsync(request);
            return CreatedAtAction(nameof(GetProvider), new { id = provider.Id }, provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cloud provider");
            return StatusCode(500, "Error creating cloud provider");
        }
    }

    /// <summary>
    /// Update a cloud provider
    /// </summary>
    [HttpPut("providers/{id}")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider {Id}", id);
            return StatusCode(500, "Error updating provider");
        }
    }

    /// <summary>
    /// Delete a cloud provider
    /// </summary>
    [HttpDelete("providers/{id}")]
    public async Task<ActionResult> DeleteProvider(int id)
    {
        try
        {
            var deleted = await _deploymentService.DeleteProviderAsync(id);
            if (!deleted)
            {
                return NotFound($"Provider {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider {Id}", id);
            return StatusCode(500, "Error deleting provider");
        }
    }

    /// <summary>
    /// Test connection to a cloud provider
    /// </summary>
    [HttpPost("providers/test")]
    public async Task<ActionResult<ProviderConnectionResult>> TestProviderConnection([FromBody] TestProviderConnectionRequest request)
    {
        try
        {
            var result = await _deploymentService.TestProviderConnectionAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing provider connection");
            return StatusCode(500, "Error testing provider connection");
        }
    }

    /// <summary>
    /// Get available resources for a provider
    /// </summary>
    [HttpGet("providers/{id}/resources/{resourceType}")]
    public async Task<ActionResult<IEnumerable<ResourceOption>>> GetProviderResources(int id, string resourceType)
    {
        try
        {
            var resources = await _deploymentService.GetProviderResourcesAsync(id, resourceType);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider resources");
            return StatusCode(500, "Error retrieving provider resources");
        }
    }

    #endregion

    #region Deployments

    /// <summary>
    /// Get all deployments
    /// </summary>
    [HttpGet("deployments")]
    public async Task<ActionResult<IEnumerable<CloudDeploymentDto>>> GetDeployments(
        [FromQuery] int? providerId = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var deployments = await _deploymentService.GetDeploymentsAsync(providerId, status);
            return Ok(deployments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployments");
            return StatusCode(500, "Error retrieving deployments");
        }
    }

    /// <summary>
    /// Get a specific deployment
    /// </summary>
    [HttpGet("deployments/{id}")]
    public async Task<ActionResult<CloudDeploymentDto>> GetDeployment(int id)
    {
        try
        {
            var deployment = await _deploymentService.GetDeploymentByIdAsync(id);
            if (deployment == null)
            {
                return NotFound($"Deployment {id} not found");
            }
            return Ok(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment {Id}", id);
            return StatusCode(500, "Error retrieving deployment");
        }
    }

    /// <summary>
    /// Create a new deployment
    /// </summary>
    [HttpPost("deployments")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deployment");
            return StatusCode(500, "Error creating deployment");
        }
    }

    /// <summary>
    /// Update a deployment
    /// </summary>
    [HttpPut("deployments/{id}")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deployment {Id}", id);
            return StatusCode(500, "Error updating deployment");
        }
    }

    /// <summary>
    /// Delete a deployment
    /// </summary>
    [HttpDelete("deployments/{id}")]
    public async Task<ActionResult> DeleteDeployment(int id)
    {
        try
        {
            var deleted = await _deploymentService.DeleteDeploymentAsync(id);
            if (!deleted)
            {
                return NotFound($"Deployment {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting deployment {Id}", id);
            return StatusCode(500, "Error deleting deployment");
        }
    }

    /// <summary>
    /// Trigger a deployment
    /// </summary>
    [HttpPost("deployments/{id}/deploy")]
    public async Task<ActionResult<DeploymentResult>> TriggerDeployment(int id, [FromBody] TriggerDeploymentRequest request)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering deployment {Id}", id);
            return StatusCode(500, "Error triggering deployment");
        }
    }

    /// <summary>
    /// Stop a deployment
    /// </summary>
    [HttpPost("deployments/{id}/stop")]
    public async Task<ActionResult<DeploymentResult>> StopDeployment(int id)
    {
        try
        {
            var result = await _deploymentService.StopDeploymentAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping deployment {Id}", id);
            return StatusCode(500, "Error stopping deployment");
        }
    }

    /// <summary>
    /// Restart a deployment
    /// </summary>
    [HttpPost("deployments/{id}/restart")]
    public async Task<ActionResult<DeploymentResult>> RestartDeployment(int id)
    {
        try
        {
            var result = await _deploymentService.RestartDeploymentAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting deployment {Id}", id);
            return StatusCode(500, "Error restarting deployment");
        }
    }

    /// <summary>
    /// Scale a deployment
    /// </summary>
    [HttpPost("deployments/{id}/scale")]
    public async Task<ActionResult<DeploymentResult>> ScaleDeployment(int id, [FromQuery] int replicas)
    {
        try
        {
            var result = await _deploymentService.ScaleDeploymentAsync(id, replicas);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scaling deployment {Id}", id);
            return StatusCode(500, "Error scaling deployment");
        }
    }

    #endregion

    #region Deployment Attempts

    /// <summary>
    /// Get deployment attempts for a deployment
    /// </summary>
    [HttpGet("deployments/{deploymentId}/attempts")]
    public async Task<ActionResult<IEnumerable<DeploymentAttemptDto>>> GetDeploymentAttempts(int deploymentId)
    {
        try
        {
            var attempts = await _deploymentService.GetDeploymentAttemptsAsync(deploymentId);
            return Ok(attempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment attempts");
            return StatusCode(500, "Error retrieving deployment attempts");
        }
    }

    /// <summary>
    /// Get a specific deployment attempt
    /// </summary>
    [HttpGet("attempts/{attemptId}")]
    public async Task<ActionResult<DeploymentAttemptDto>> GetDeploymentAttempt(int attemptId)
    {
        try
        {
            var attempt = await _deploymentService.GetDeploymentAttemptByIdAsync(attemptId);
            if (attempt == null)
            {
                return NotFound($"Attempt {attemptId} not found");
            }
            return Ok(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attempt {AttemptId}", attemptId);
            return StatusCode(500, "Error retrieving attempt");
        }
    }

    /// <summary>
    /// Get logs for a deployment attempt
    /// </summary>
    [HttpGet("attempts/{attemptId}/logs")]
    public async Task<ActionResult<string>> GetDeploymentAttemptLogs(int attemptId)
    {
        try
        {
            var logs = await _deploymentService.GetDeploymentAttemptLogsAsync(attemptId);
            return Ok(new { logs });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attempt logs");
            return StatusCode(500, "Error retrieving attempt logs");
        }
    }

    #endregion

    #region Health Checks

    /// <summary>
    /// Run health check on a deployment
    /// </summary>
    [HttpPost("deployments/{deploymentId}/health-check")]
    public async Task<ActionResult<HealthCheckResult>> RunHealthCheck(int deploymentId)
    {
        try
        {
            var result = await _deploymentService.RunHealthCheckAsync(deploymentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running health check");
            return StatusCode(500, "Error running health check");
        }
    }

    /// <summary>
    /// Get health check history for a deployment
    /// </summary>
    [HttpGet("deployments/{deploymentId}/health-history")]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetHealthCheckHistory(
        int deploymentId,
        [FromQuery] int? limit = 20)
    {
        try
        {
            var history = await _deploymentService.GetHealthCheckHistoryAsync(deploymentId, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health check history");
            return StatusCode(500, "Error retrieving health check history");
        }
    }

    /// <summary>
    /// Get health status for all deployments
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetAllDeploymentHealth()
    {
        try
        {
            var health = await _deploymentService.GetAllDeploymentHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment health");
            return StatusCode(500, "Error retrieving deployment health");
        }
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// Get deployment dashboard summary
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DeploymentDashboardDto>> GetDashboard()
    {
        try
        {
            var dashboard = await _deploymentService.GetDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment dashboard");
            return StatusCode(500, "Error retrieving deployment dashboard");
        }
    }

    #endregion
}
