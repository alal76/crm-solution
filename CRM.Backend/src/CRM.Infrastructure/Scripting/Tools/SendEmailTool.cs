// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting.Tools;

/// <summary>
/// Script tool that sends an email to a contact.
/// Requires the <c>send:email</c> permission.
/// In production, replace the stub with a call to the notification provider.
/// </summary>
[ScriptTool("SendEmail", "Send an email to a contact", "send:email")]
public class SendEmailTool
{
    private readonly ILogger<SendEmailTool> _logger;

    /// <summary>Initialises a new <see cref="SendEmailTool"/>.</summary>
    public SendEmailTool(ILogger<SendEmailTool> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the tool. <paramref name="parameters"/> should contain
    /// <c>To</c>, <c>Subject</c>, and <c>Body</c> properties.
    /// </summary>
    public async Task<object?> InvokeAsync(object parameters, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SendEmailTool: email queued");

        // TODO: Inject INotificationPort and dispatch email via notification provider
        await Task.CompletedTask.ConfigureAwait(false);
        return new { Queued = true };
    }
}
