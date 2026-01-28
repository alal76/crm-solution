// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for campaign execution and analytics
/// </summary>
[ApiController]
[Route("api/campaigns")]
[Authorize]
public class CampaignExecutionController : ControllerBase
{
    private readonly CampaignExecutionService _campaignExecutionService;
    private readonly ILogger<CampaignExecutionController> _logger;

    public CampaignExecutionController(
        CampaignExecutionService campaignExecutionService,
        ILogger<CampaignExecutionController> logger)
    {
        _campaignExecutionService = campaignExecutionService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region Campaign Execution

    /// <summary>
    /// Start a campaign
    /// </summary>
    [HttpPost("{campaignId}/start")]
    public async Task<IActionResult> StartCampaign(int campaignId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _campaignExecutionService.StartCampaignAsync(campaignId, userId);
            if (!result)
                return BadRequest(new { message = "Campaign cannot be started. Check the campaign status." });

            return Ok(new { message = "Campaign started successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error starting campaign" });
        }
    }

    /// <summary>
    /// Get campaign analytics
    /// </summary>
    [HttpGet("{campaignId}/analytics")]
    public async Task<IActionResult> GetCampaignAnalytics(int campaignId)
    {
        try
        {
            var analytics = await _campaignExecutionService.GetCampaignAnalyticsAsync(campaignId);
            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving campaign analytics" });
        }
    }

    #endregion

    #region Campaign Workflows

    /// <summary>
    /// Get workflows linked to a campaign
    /// </summary>
    [HttpGet("{campaignId}/workflows")]
    public async Task<IActionResult> GetCampaignWorkflows(int campaignId)
    {
        try
        {
            var workflows = await _campaignExecutionService.GetCampaignWorkflowsAsync(campaignId);
            return Ok(workflows.Select(cw => new
            {
                cw.Id,
                cw.CampaignId,
                cw.WorkflowDefinitionId,
                WorkflowName = cw.WorkflowDefinition?.Name,
                cw.WorkflowType,
                cw.TriggerEvent,
                cw.TriggerConditions,
                cw.IsActive,
                cw.MaxExecutionsPerContact,
                cw.CooldownHours,
                cw.Priority,
                cw.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflows for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving campaign workflows" });
        }
    }

    /// <summary>
    /// Link a workflow to a campaign
    /// </summary>
    [HttpPost("{campaignId}/workflows")]
    public async Task<IActionResult> LinkWorkflowToCampaign(
        int campaignId,
        [FromBody] LinkWorkflowRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var campaignWorkflow = await _campaignExecutionService.LinkWorkflowToCampaignAsync(
                campaignId,
                request.WorkflowDefinitionId,
                request.WorkflowType,
                request.TriggerEvent,
                request.TriggerConditions,
                request.MaxExecutionsPerContact,
                request.CooldownHours,
                userId);

            return Ok(campaignWorkflow);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking workflow to campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error linking workflow to campaign" });
        }
    }

    /// <summary>
    /// Update a campaign workflow configuration
    /// </summary>
    [HttpPut("{campaignId}/workflows/{workflowId}")]
    public async Task<IActionResult> UpdateCampaignWorkflow(
        int campaignId,
        int workflowId,
        [FromBody] UpdateCampaignWorkflowRequest request)
    {
        try
        {
            var result = await _campaignExecutionService.UpdateCampaignWorkflowAsync(
                workflowId,
                request.IsActive,
                request.TriggerConditions,
                request.MaxExecutionsPerContact,
                request.CooldownHours);

            if (result == null)
                return NotFound(new { message = "Campaign workflow not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { message = "Error updating campaign workflow" });
        }
    }

    /// <summary>
    /// Unlink a workflow from a campaign
    /// </summary>
    [HttpDelete("{campaignId}/workflows/{workflowId}")]
    public async Task<IActionResult> UnlinkWorkflow(int campaignId, int workflowId)
    {
        try
        {
            var result = await _campaignExecutionService.UnlinkWorkflowFromCampaignAsync(workflowId);
            if (!result)
                return NotFound(new { message = "Campaign workflow not found" });

            return Ok(new { message = "Workflow unlinked from campaign" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking workflow {WorkflowId} from campaign {CampaignId}", workflowId, campaignId);
            return StatusCode(500, new { message = "Error unlinking workflow from campaign" });
        }
    }

    #endregion

    #region Campaign Recipients

    /// <summary>
    /// Get campaign recipients with filtering
    /// </summary>
    [HttpGet("{campaignId}/recipients")]
    public async Task<IActionResult> GetCampaignRecipients(
        int campaignId,
        [FromQuery] string? status = null,
        [FromQuery] bool? hasOpened = null,
        [FromQuery] bool? hasClicked = null,
        [FromQuery] bool? hasConverted = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            var (items, totalCount) = await _campaignExecutionService.GetCampaignRecipientsAsync(
                campaignId, status, hasOpened, hasClicked, hasConverted, skip, take);

            return Ok(new
            {
                items = items.Select(r => new
                {
                    r.Id,
                    r.CampaignId,
                    r.ContactId,
                    r.CustomerId,
                    r.Email,
                    r.FirstName,
                    r.LastName,
                    CustomerName = r.Customer?.Company,
                    r.Status,
                    r.SendScheduledTime,
                    r.SendActualTime,
                    r.DeliveredAt,
                    r.FirstOpenedAt,
                    r.LastOpenedAt,
                    r.OpenCount,
                    r.FirstClickedAt,
                    r.LastClickedAt,
                    r.ClickCount,
                    r.ConvertedAt,
                    r.ConversionValue,
                    r.UnsubscribedAt,
                    r.ABTestVariant
                }),
                totalCount,
                skip,
                take,
                hasMore = skip + take < totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recipients for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving campaign recipients" });
        }
    }

    /// <summary>
    /// Record an email open (typically called from tracking pixel)
    /// </summary>
    [HttpPost("recipients/{recipientId}/open")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordEmailOpen(int recipientId)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            await _campaignExecutionService.RecordEmailOpenAsync(recipientId, ipAddress, userAgent);

            // Return a 1x1 transparent GIF
            var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(gif, "image/gif");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording email open for recipient {RecipientId}", recipientId);
            // Return the transparent GIF anyway to not break email display
            var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(gif, "image/gif");
        }
    }

    /// <summary>
    /// Record a link click
    /// </summary>
    [HttpGet("recipients/{recipientId}/click")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordLinkClick(
        int recipientId,
        [FromQuery] string url,
        [FromQuery] string? redirect = null)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceType = GetDeviceType(userAgent);
            var browser = GetBrowser(userAgent);

            await _campaignExecutionService.RecordLinkClickAsync(
                recipientId, url, deviceType, browser, ipAddress);

            // Redirect to the actual URL
            var redirectUrl = redirect ?? url;
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording link click for recipient {RecipientId}", recipientId);
            // Redirect anyway
            return Redirect(redirect ?? url);
        }
    }

    /// <summary>
    /// Record a conversion
    /// </summary>
    [HttpPost("recipients/{recipientId}/conversion")]
    public async Task<IActionResult> RecordConversion(
        int recipientId,
        [FromBody] RecordConversionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var conversion = await _campaignExecutionService.RecordConversionAsync(
                recipientId,
                request.ConversionType,
                request.ConversionValue,
                request.OrderId,
                request.OpportunityId,
                request.AttributionModel,
                request.AttributionPercentage,
                userId);

            return Ok(conversion);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording conversion for recipient {RecipientId}", recipientId);
            return StatusCode(500, new { message = "Error recording conversion" });
        }
    }

    #endregion

    #region A/B Testing

    /// <summary>
    /// Create an A/B test for a campaign
    /// </summary>
    [HttpPost("{campaignId}/abtests")]
    public async Task<IActionResult> CreateABTest(int campaignId, [FromBody] CreateABTestRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var abTest = await _campaignExecutionService.CreateABTestAsync(
                campaignId,
                request.TestName,
                request.TestType,
                request.TestMetric,
                request.VariantAConfig,
                request.VariantBConfig,
                request.TrafficSplit,
                request.MinimumSampleSize,
                request.TestDurationHours,
                request.AutoSelectWinner,
                userId);

            return Ok(abTest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating A/B test for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error creating A/B test" });
        }
    }

    /// <summary>
    /// Start an A/B test
    /// </summary>
    [HttpPost("{campaignId}/abtests/{testId}/start")]
    public async Task<IActionResult> StartABTest(int campaignId, int testId)
    {
        try
        {
            var result = await _campaignExecutionService.StartABTestAsync(testId);
            if (!result)
                return BadRequest(new { message = "A/B test cannot be started. Check the test status." });

            return Ok(new { message = "A/B test started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting A/B test {TestId}", testId);
            return StatusCode(500, new { message = "Error starting A/B test" });
        }
    }

    #endregion

    #region Helper Methods

    private static string GetDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "unknown";
        
        var ua = userAgent.ToLower();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return "mobile";
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "tablet";
        return "desktop";
    }

    private static string GetBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "unknown";
        
        var ua = userAgent.ToLower();
        if (ua.Contains("chrome")) return "Chrome";
        if (ua.Contains("firefox")) return "Firefox";
        if (ua.Contains("safari")) return "Safari";
        if (ua.Contains("edge")) return "Edge";
        if (ua.Contains("opera")) return "Opera";
        return "Other";
    }

    #endregion
}

#region Request DTOs

public class LinkWorkflowRequest
{
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowType { get; set; } = "automation";
    public string TriggerEvent { get; set; } = "campaign_start";
    public string? TriggerConditions { get; set; }
    public int MaxExecutionsPerContact { get; set; } = 1;
    public int CooldownHours { get; set; } = 24;
}

public class UpdateCampaignWorkflowRequest
{
    public bool? IsActive { get; set; }
    public string? TriggerConditions { get; set; }
    public int? MaxExecutionsPerContact { get; set; }
    public int? CooldownHours { get; set; }
}

public class RecordConversionRequest
{
    public string ConversionType { get; set; } = "purchase";
    public decimal? ConversionValue { get; set; }
    public int? OrderId { get; set; }
    public int? OpportunityId { get; set; }
    public string AttributionModel { get; set; } = "last_click";
    public decimal AttributionPercentage { get; set; } = 100;
}

public class CreateABTestRequest
{
    public string TestName { get; set; } = string.Empty;
    public string TestType { get; set; } = "subject_line";
    public string TestMetric { get; set; } = "open_rate";
    public string VariantAConfig { get; set; } = string.Empty;
    public string VariantBConfig { get; set; } = string.Empty;
    public int TrafficSplit { get; set; } = 50;
    public int? MinimumSampleSize { get; set; }
    public int? TestDurationHours { get; set; }
    public bool AutoSelectWinner { get; set; } = false;
}

#endregion
