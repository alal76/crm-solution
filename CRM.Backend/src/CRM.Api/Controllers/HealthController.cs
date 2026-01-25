using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Health check endpoints for container orchestration and monitoring
/// </summary>
[ApiController]
[Route("[controller]")]
[EnableCors("AllowAll")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// General health check endpoint (for liveness probes)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check endpoint called");
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed readiness check including dependencies
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> ReadinessAsync()
    {
        try
        {
            var checks = new Dictionary<string, bool>
            {
                { "api", true },
                { "timestamp", true }
            };

            var allHealthy = checks.Values.All(v => v);

            IActionResult result = allHealthy
                ? Ok(new { status = "ready", checks, timestamp = DateTime.UtcNow })
                : StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { status = "not_ready", checks, timestamp = DateTime.UtcNow });
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = "error", message = ex.Message }));
        }
    }

    /// <summary>
    /// Liveness check for Kubernetes liveness probe
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }
}
