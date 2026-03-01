// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Admin endpoints for Dead Letter Queue management (INFRA-07).
/// Provides visibility and retry capabilities for failed messages.
/// </summary>
[ApiController]
[Route("api/admin/dead-letter-queue")]
[Authorize(Roles = "Admin")]
public class AdminDeadLetterQueueController : CrmControllerBase
{
    private readonly IDeadLetterQueueService _dlq;
    private readonly ILogger<AdminDeadLetterQueueController> _logger;

    public AdminDeadLetterQueueController(IDeadLetterQueueService dlq, ILogger<AdminDeadLetterQueueController> logger)
    {
        _dlq = dlq;
        _logger = logger;
    }

    /// <summary>Lists messages in the dead letter queue.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] string? source,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        IEnumerable<DeadLetterMessage> messages;

        if (!string.IsNullOrWhiteSpace(source))
            messages = await _dlq.GetBySourceAsync(source, ct);
        else
            messages = await _dlq.GetMessagesAsync(skip, take, ct);

        var count = await _dlq.GetCountAsync(ct);
        return Ok(new { messages, totalCount = count });
    }

    /// <summary>Gets the total count of dead letter messages.</summary>
    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount(CancellationToken ct = default)
    {
        var count = await _dlq.GetCountAsync(ct);
        return Ok(new { count });
    }

    /// <summary>Retries a specific dead letter message (moves back to main queue).</summary>
    [HttpPost("{messageId}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryMessage(string messageId, CancellationToken ct = default)
    {
        var success = await _dlq.RetryAsync(messageId, ct);
        if (!success)
            return NotFound(new { message = "Message not found in dead letter queue." });

        _logger.LogInformation("Admin retried DLQ message {MessageId}", messageId);
        return Ok(new { message = "Message requeued for processing.", messageId });
    }

    /// <summary>Retries all dead letter messages.</summary>
    [HttpPost("retry-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RetryAll(CancellationToken ct = default)
    {
        var count = await _dlq.RetryAllAsync(ct);
        _logger.LogInformation("Admin retried all {Count} DLQ messages", count);
        return Ok(new { message = $"{count} messages requeued for processing.", count });
    }

    /// <summary>Removes a specific dead letter message (discard).</summary>
    [HttpDelete("{messageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMessage(string messageId, CancellationToken ct = default)
    {
        var success = await _dlq.RemoveAsync(messageId, ct);
        if (!success)
            return NotFound(new { message = "Message not found in dead letter queue." });

        _logger.LogInformation("Admin removed DLQ message {MessageId}", messageId);
        return NoContent();
    }

    /// <summary>Purges all dead letter messages.</summary>
    [HttpDelete("purge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Purge(CancellationToken ct = default)
    {
        var count = await _dlq.PurgeAsync(ct);
        _logger.LogWarning("Admin purged DLQ: {Count} messages removed", count);
        return Ok(new { message = $"Dead letter queue purged. {count} messages removed.", count });
    }
}
