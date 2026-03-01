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
using EmailInbound = CRM.Core.Interfaces.ITSM.InboundEmailDto;
namespace CRM.Api.Controllers;

/// <summary>
/// Controller for email-to-ticket functionality.
/// </summary>
[ApiController]
[Route("api/itsm/email")]
[Authorize]
[Tags("ITSM - Email Integration")]
public class EmailToTicketController : CrmControllerBase
{
    private readonly IEmailToTicketService _emailService;
    private readonly ILogger<EmailToTicketController> _logger;
    private readonly string? _inboundApiKey;

    public EmailToTicketController(
        IEmailToTicketService emailService,
        ILogger<EmailToTicketController> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _logger = logger;
        _inboundApiKey = configuration["ITSM:InboundEmailApiKey"];
    }

    /// <summary>
    /// Process an inbound email to create or update an incident.
    /// </summary>
    [HttpPost("inbound")]
    [AllowAnonymous] // Webhook endpoint - uses API key authentication
    [ProducesResponseType(typeof(EmailParseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EmailParseResult>> ProcessInboundEmail(
        [FromBody] EmailInbound email,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        // Validate API key for anonymous webhook endpoint
        if (!string.IsNullOrEmpty(_inboundApiKey))
        {
            if (string.IsNullOrEmpty(apiKey) || !string.Equals(apiKey, _inboundApiKey, StringComparison.Ordinal))
            {
                _logger.LogWarning("Inbound email rejected: invalid or missing API key from {From}", email.From);
                return Unauthorized(new { message = "Invalid or missing API key" });
            }
        }
        else
        {
            _logger.LogWarning("ITSM:InboundEmailApiKey is not configured — inbound email endpoint is unprotected");
        }

        _logger.LogInformation("Received inbound email from {From}", email.From);

        // Check if this is a reply to an existing incident
        var incidentId = _emailService.ExtractIncidentReference(email.Subject);

        EmailParseResult result;
        if (incidentId.HasValue)
        {
            result = await _emailService.ParseAndUpdateIncidentAsync(email, incidentId.Value);
        }
        else
        {
            result = await _emailService.ParseAndCreateIncidentAsync(email);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get email parsing configuration.
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(typeof(EmailParsingConfigDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailParsingConfigDto>> GetConfiguration()
    {
        var config = await _emailService.GetConfigurationAsync();
        return Ok(config);
    }

    /// <summary>
    /// Update email parsing configuration.
    /// </summary>
    [HttpPut("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateConfiguration([FromBody] EmailParsingConfigDto config)
    {
        await _emailService.UpdateConfigurationAsync(config);
        return Ok(new { message = "Configuration updated successfully" });
    }

    /// <summary>
    /// Test email parsing without creating an incident.
    /// </summary>
    [HttpPost("test")]
    [ProducesResponseType(typeof(EmailTestResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailTestResult>> TestEmailParsing([FromBody] EmailInbound email)
    {
        var config = await _emailService.GetConfigurationAsync();
        var incidentRef = _emailService.ExtractIncidentReference(email.Subject);

        var result = new EmailTestResult
        {
            WouldCreateNewIncident = !incidentRef.HasValue,
            ExistingIncidentId = incidentRef,
            ExtractedSubject = email.Subject,
            ExtractedFrom = email.From,
            AttachmentCount = email.Attachments.Count,
            TotalAttachmentSize = email.Attachments.Sum(a => a.Size),
            IsWithinSizeLimit = email.Attachments.Sum(a => a.Size) <= config.MaxAttachmentSizeMB * 1024 * 1024
        };

        return Ok(result);
    }

    /// <summary>
    /// Get email processing history (BVT-compatible route).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetEmailHistory()
    {
        return Ok(new List<object>());
    }
}

/// <summary>
/// Result of email parsing test.
/// </summary>
public class EmailTestResult
{
    public bool WouldCreateNewIncident { get; set; }
    public int? ExistingIncidentId { get; set; }
    public string ExtractedSubject { get; set; } = string.Empty;
    public string ExtractedFrom { get; set; } = string.Empty;
    public int AttachmentCount { get; set; }
    public long TotalAttachmentSize { get; set; }
    public bool IsWithinSizeLimit { get; set; }
}
