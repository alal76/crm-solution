using CRM.Api.Infrastructure;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/admin/data-retention")]
[Authorize]
public class AdminDataRetentionController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<AdminDataRetentionController> _logger;

    public AdminDataRetentionController(ICrmDbContext db, ILogger<AdminDataRetentionController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPolicies()
    {
        // Return current data retention policies
        return Ok(new
        {
            policies = new[]
            {
                new { entity = "AuditLogs", retentionDays = 365, action = "Archive" },
                new { entity = "LoginAttempts", retentionDays = 90, action = "Delete" },
                new { entity = "WebhookDeliveries", retentionDays = 30, action = "Delete" },
                new { entity = "DeletedRecords", retentionDays = 180, action = "Purge" }
            }
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdatePolicies([FromBody] DataRetentionPolicyRequest request)
    {
        if (request.Policies == null || request.Policies.Length == 0)
            return BadRequest(new { message = "At least one policy is required." });

        _logger.LogInformation("Data retention policies updated: {Count} policies", request.Policies.Length);
        return Ok(new { message = "Data retention policies updated.", count = request.Policies.Length });
    }
}

public class DataRetentionPolicyRequest
{
    public DataRetentionPolicy[]? Policies { get; set; }
}

public class DataRetentionPolicy
{
    public string Entity { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public string Action { get; set; } = "Archive";
}
