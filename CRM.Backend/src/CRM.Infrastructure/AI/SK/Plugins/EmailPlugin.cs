// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.ComponentModel;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for email template management and sending operations.
/// Provides AI-accessible functions for browsing templates, rendering previews, and sending emails.
/// </summary>
public class EmailPlugin : CrmPluginBase
{
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly INotificationPort _notificationPort;

    /// <inheritdoc />
    public override string PluginName => "Email";

    /// <inheritdoc />
    public override string Description => "Manage email templates and send emails — browse templates, preview rendered content, send individual or template-based emails.";

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailPlugin"/> class.
    /// </summary>
    /// <param name="emailTemplateService">The email template service for template operations.</param>
    /// <param name="notificationPort">The notification port for sending emails.</param>
    /// <param name="logger">The logger instance.</param>
    public EmailPlugin(
        IEmailTemplateService emailTemplateService,
        INotificationPort notificationPort,
        ILogger<EmailPlugin> logger) : base(logger)
    {
        _emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
        _notificationPort = notificationPort ?? throw new ArgumentNullException(nameof(notificationPort));
    }

    #region Read Operations

    /// <summary>
    /// Retrieves all email templates, optionally filtered by active status.
    /// </summary>
    /// <param name="activeOnly">When true, return only active templates. Defaults to true.</param>
    /// <returns>A JSON array of email template summaries.</returns>
    [KernelFunction("GetEmailTemplates")]
    [Description("Get all email templates. Optionally filter to active templates only.")]
    public async Task<string> GetEmailTemplatesAsync(
        [Description("When true, return only active templates")] bool activeOnly = true)
    {
        try
        {
            var templates = await _emailTemplateService.GetAllAsync(
                category: null,
                isActive: activeOnly ? true : null);

            var summaries = templates.Select(t => new
            {
                t.Id,
                t.Name,
                t.Subject,
                t.Category,
                t.IsActive,
                t.Slug
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetEmailTemplates", ex.Message);
        }
    }

    /// <summary>
    /// Searches email templates by name or subject keyword.
    /// </summary>
    /// <param name="keyword">The keyword to search for in template names or subjects.</param>
    /// <returns>A JSON array of matching email template summaries.</returns>
    [KernelFunction("SearchTemplates")]
    [Description("Search email templates by keyword in name or subject.")]
    public async Task<string> SearchTemplatesAsync(
        [Description("Keyword to search for in template name or subject")] string keyword)
    {
        try
        {
            var templates = await _emailTemplateService.GetAllAsync(category: null, isActive: null);

            var filtered = templates
                .Where(t => (t.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (t.Subject?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Subject,
                    t.Category,
                    t.IsActive,
                    t.Slug
                });

            return SuccessResult(filtered);
        }
        catch (Exception ex)
        {
            return ErrorResult("SearchTemplates", ex.Message);
        }
    }

    /// <summary>
    /// Previews a rendered email template with sample data.
    /// </summary>
    /// <param name="templateId">The ID of the template to preview.</param>
    /// <returns>A JSON object with the rendered subject and body.</returns>
    [KernelFunction("PreviewTemplate")]
    [Description("Preview a rendered email template with sample data.")]
    public async Task<string> PreviewTemplateAsync(
        [Description("The ID of the email template to preview")] int templateId)
    {
        try
        {
            var rendered = await _emailTemplateService.PreviewAsync(templateId);

            return SuccessResult(new
            {
                rendered.Subject,
                rendered.HtmlBody,
                rendered.TextBody,
                rendered.Warnings
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("PreviewTemplate", ex.Message);
        }
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The email body content (HTML supported).</param>
    /// <returns>A JSON object indicating send success or failure.</returns>
    [KernelFunction("SendEmail")]
    [Description("Send an email to a recipient. Body can be HTML.")]
    [RequiresApproval(Tier = "standard", Description = "Sends an email to an external recipient")]
    public async Task<string> SendEmailAsync(
        [Description("Recipient email address")] string to,
        [Description("Email subject line")] string subject,
        [Description("Email body content (HTML supported)")] string body)
    {
        try
        {
            var request = new EmailNotificationRequest
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            var result = await _notificationPort.SendEmailAsync(request);

            return SuccessResult(new
            {
                success = result.Success,
                messageId = result.MessageId,
                message = result.Success ? "Email sent successfully" : result.Error
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("SendEmail", ex.Message);
        }
    }

    /// <summary>
    /// Sends a template-based email to the specified recipient.
    /// </summary>
    /// <param name="templateName">The name or slug of the email template to use.</param>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <returns>A JSON object indicating send success or failure.</returns>
    [KernelFunction("SendTemplateEmail")]
    [Description("Send a template-based email to a recipient using a pre-defined template.")]
    [RequiresApproval(Tier = "standard", Description = "Sends a template email to an external recipient")]
    public async Task<string> SendTemplateEmailAsync(
        [Description("Template name or slug to use")] string templateName,
        [Description("Recipient email address")] string recipientEmail)
    {
        try
        {
            var template = await _emailTemplateService.GetByNameAsync(templateName)
                ?? await _emailTemplateService.GetBySlugAsync(templateName);

            if (template == null)
            {
                return SuccessResult(new { success = false, message = $"Template '{templateName}' not found" });
            }

            var result = await _notificationPort.SendTemplateEmailAsync(
                template.Id.ToString(),
                recipientEmail,
                new { },
                CancellationToken.None);

            return SuccessResult(new
            {
                success = result.Success,
                messageId = result.MessageId,
                templateUsed = template.Name,
                message = result.Success ? "Template email sent successfully" : result.Error
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("SendTemplateEmail", ex.Message);
        }
    }

    #endregion
}
