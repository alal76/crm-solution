// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmailInbound = CRM.Core.Interfaces.ITSM.InboundEmailDto;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for email-to-ticket functionality.
/// </summary>
[ApiController]
[Route("api/itsm/email")]
[Authorize]
[Tags("ITSM - Email Integration")]
public class EmailToTicketController : ControllerBase
{
    private readonly IEmailToTicketService _emailService;
    private readonly ILogger<EmailToTicketController> _logger;

    public EmailToTicketController(
        IEmailToTicketService emailService,
        ILogger<EmailToTicketController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Process an inbound email to create or update an incident.
    /// </summary>
    [HttpPost("inbound")]
    [AllowAnonymous] // Webhook endpoint - uses API key authentication
    public async Task<ActionResult<EmailParseResult>> ProcessInboundEmail(
        [FromBody] EmailInbound email,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        // In production, validate API key here
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
    public async Task<ActionResult<EmailParsingConfigDto>> GetConfiguration()
    {
        var config = await _emailService.GetConfigurationAsync();
        return Ok(config);
    }

    /// <summary>
    /// Update email parsing configuration.
    /// </summary>
    [HttpPut("config")]
    public async Task<ActionResult> UpdateConfiguration([FromBody] EmailParsingConfigDto config)
    {
        await _emailService.UpdateConfigurationAsync(config);
        return Ok(new { message = "Configuration updated successfully" });
    }

    /// <summary>
    /// Test email parsing without creating an incident.
    /// </summary>
    [HttpPost("test")]
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
