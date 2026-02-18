// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using System.Text.RegularExpressions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IEmailTemplateService for email template management operations.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EmailTemplateService> _logger;
    private static readonly Regex VariablePattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public EmailTemplateService(ICrmDbContext context, ILogger<EmailTemplateService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(
        EmailTemplateCategory? category = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.EmailTemplates.Where(t => !t.IsDeleted);

        if (category.HasValue)
        {
            query = query.Where(t => t.Category == category.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        return await query.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted, cancellationToken);
    }

    public async Task<EmailTemplate?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted, cancellationToken);
    }

    public async Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        template.Slug ??= GenerateSlug(template.Name);
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;
        template.IsActive = true;

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        // Create initial version
        await CreateVersionAsync(template.Id, "Initial version", cancellationToken);

        _logger.LogInformation("Created email template {TemplateName} with ID {TemplateId}", template.Name, template.Id);
        return template;
    }

    public async Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        var existing = await _context.EmailTemplates.FindAsync(new object[] { template.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Email template {template.Id} not found");
        }

        template.UpdatedAt = DateTime.UtcNow;
        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated email template {TemplateId}", template.Id);
        return template;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template == null) return false;

        if (template.IsSystem)
        {
            throw new InvalidOperationException("Cannot delete system templates");
        }

        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted email template {TemplateId}", id);
        return true;
    }

    #endregion

    #region Template Rendering

    public async Task<RenderedEmail> RenderAsync(int templateId, Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Email template {templateId} not found");
        }

        return RenderTemplate(template, data);
    }

    public async Task<RenderedEmail> RenderByNameAsync(string templateName, Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var template = await GetByNameAsync(templateName, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Email template '{templateName}' not found");
        }

        return RenderTemplate(template, data);
    }

    public async Task<RenderedEmail> RenderForEntityAsync(int templateId, string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Email template {templateId} not found");
        }

        var data = await GetEntityDataAsync(entityType, entityId, cancellationToken);
        return RenderTemplate(template, data);
    }

    public async Task<RenderedEmail> PreviewAsync(int templateId, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Email template {templateId} not found");
        }

        var sampleData = await GetSampleDataAsync(template.Category, cancellationToken);
        return RenderTemplate(template, sampleData);
    }

    public async Task<TemplateValidationResult> ValidateAsync(string templateContent, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var result = new TemplateValidationResult
        {
            IsValid = true,
            Errors = new List<TemplateValidationError>(),
            Warnings = new List<string>(),
            UsedVariables = new List<string>()
        };

        try
        {
            var variables = ExtractVariablesSync(templateContent);
            result.UsedVariables = variables;

            // Check for unclosed variable tags
            var unclosed = Regex.Matches(templateContent, @"\{\{(?![^{}]*\}\})");
            foreach (Match match in unclosed)
            {
                result.Errors.Add(new TemplateValidationError
                {
                    Line = GetLineNumber(templateContent, match.Index),
                    Column = GetColumnNumber(templateContent, match.Index),
                    Message = "Unclosed variable tag",
                    Code = "UNCLOSED_TAG"
                });
            }

            if (result.Errors.Count > 0)
            {
                result.IsValid = false;
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add(new TemplateValidationError
            {
                Message = ex.Message,
                Code = "PARSE_ERROR"
            });
        }

        return result;
    }

    private RenderedEmail RenderTemplate(EmailTemplate template, Dictionary<string, object> data)
    {
        var warnings = new List<string>();
        var subject = ReplaceVariables(template.Subject, data, warnings);
        var htmlBody = ReplaceVariables(template.HtmlBody ?? string.Empty, data, warnings);
        var textBody = template.PlainTextBody != null ? ReplaceVariables(template.PlainTextBody, data, warnings) : null;

        return new RenderedEmail
        {
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            FromName = template.FromName,
            FromEmail = template.FromEmail,
            ReplyTo = template.ReplyToEmail,
            Warnings = warnings
        };
    }

    private string ReplaceVariables(string content, Dictionary<string, object> data, List<string> warnings)
    {
        return VariablePattern.Replace(content, match =>
        {
            var varName = match.Groups[1].Value;
            if (data.TryGetValue(varName, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }
            warnings.Add($"Variable '{varName}' not found in data");
            return match.Value;
        });
    }

    private async Task<Dictionary<string, object>> GetEntityDataAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, object>();

        switch (entityType.ToLower())
        {
            case "account":
            case "customer":
                var account = await _context.Accounts.FindAsync(new object[] { entityId }, cancellationToken);
                if (account != null)
                {
                    data["CompanyName"] = account.Company ?? string.Empty;
                    data["FirstName"] = account.FirstName ?? string.Empty;
                    data["LastName"] = account.LastName ?? string.Empty;
                    data["Email"] = account.Email ?? string.Empty;
                }
                break;

            case "contact":
                var contact = await _context.Contacts.FindAsync(new object[] { entityId }, cancellationToken);
                if (contact != null)
                {
                    data["FirstName"] = contact.FirstName ?? string.Empty;
                    data["LastName"] = contact.LastName ?? string.Empty;
                    data["Email"] = contact.Email ?? string.Empty;
                    data["Title"] = contact.Title ?? string.Empty;
                }
                break;

            case "opportunity":
                var opp = await _context.Opportunities.FindAsync(new object[] { entityId }, cancellationToken);
                if (opp != null)
                {
                    data["OpportunityName"] = opp.Name;
                    data["Amount"] = opp.Amount.ToString("C");
                    data["Stage"] = opp.Stage.ToString();
                }
                break;
        }

        return data;
    }

    #endregion

    #region Template Testing

    public async Task<bool> SendTestAsync(int templateId, string recipientEmail, Dictionary<string, object>? testData = null, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Email template {templateId} not found");
        }

        var data = testData ?? await GetSampleDataAsync(template.Category, cancellationToken);
        var rendered = RenderTemplate(template, data);

        // Log instead of sending (actual sending would use an email service)
        _logger.LogInformation("Test email for template {TemplateId} to {Recipient}: Subject='{Subject}'",
            templateId, recipientEmail, rendered.Subject);

        return true;
    }

    public async Task<Dictionary<string, object>> GetSampleDataAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var data = new Dictionary<string, object>
        {
            ["FirstName"] = "John",
            ["LastName"] = "Doe",
            ["Email"] = "john.doe@example.com",
            ["CompanyName"] = "Acme Corporation",
            ["Date"] = DateTime.UtcNow.ToString("MMMM d, yyyy")
        };

        switch (category)
        {
            case EmailTemplateCategory.Sales:
                data["OpportunityName"] = "Enterprise Software Deal";
                data["Amount"] = "$50,000";
                data["Stage"] = "Negotiation";
                data["SalesRepName"] = "Jane Smith";
                break;

            case EmailTemplateCategory.Marketing:
                data["CampaignName"] = "Summer Promotion 2025";
                data["OfferDetails"] = "Save 20% on all products";
                data["ExpirationDate"] = DateTime.UtcNow.AddDays(30).ToString("MMMM d, yyyy");
                break;

            case EmailTemplateCategory.Support:
                data["TicketNumber"] = "TKT-2025-0001";
                data["TicketSubject"] = "Unable to login";
                data["Priority"] = "High";
                data["AgentName"] = "Support Team";
                break;

            case EmailTemplateCategory.Billing:
                data["InvoiceNumber"] = "INV-2025-0001";
                data["Amount"] = "$1,500.00";
                data["DueDate"] = DateTime.UtcNow.AddDays(30).ToString("MMMM d, yyyy");
                break;
        }

        return data;
    }

    #endregion

    #region Template Versioning

    public async Task<IEnumerable<EmailTemplateVersion>> GetVersionHistoryAsync(int templateId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplateVersions
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailTemplateVersion?> GetVersionAsync(int templateId, int version, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplateVersions
            .FirstOrDefaultAsync(v => v.TemplateId == templateId && v.Version == version, cancellationToken);
    }

    public async Task<EmailTemplate> RestoreVersionAsync(int templateId, int version, CancellationToken cancellationToken = default)
    {
        var templateVersion = await GetVersionAsync(templateId, version, cancellationToken);
        if (templateVersion == null)
        {
            throw new InvalidOperationException($"Version {version} not found for template {templateId}");
        }

        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        template.Subject = templateVersion.Subject;
        template.HtmlBody = templateVersion.HtmlBody;
        template.PlainTextBody = templateVersion.TextBody;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Create new version entry for the restore
        await CreateVersionAsync(templateId, $"Restored from version {version}", cancellationToken);

        _logger.LogInformation("Restored template {TemplateId} to version {Version}", templateId, version);
        return template;
    }

    public async Task<EmailTemplateVersion> CreateVersionAsync(int templateId, string changeDescription, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        var lastVersion = await _context.EmailTemplateVersions
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync(cancellationToken);

        var newVersion = new EmailTemplateVersion
        {
            TemplateId = templateId,
            Version = (lastVersion?.Version ?? 0) + 1,
            Subject = template.Subject,
            HtmlBody = template.HtmlBody ?? string.Empty,
            TextBody = template.PlainTextBody,
            ChangeDescription = changeDescription,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailTemplateVersions.Add(newVersion);
        await _context.SaveChangesAsync(cancellationToken);

        return newVersion;
    }

    #endregion

    #region Template Categories

    public async Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(t => t.Category == category && !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TemplateCategoryInfo>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _context.EmailTemplates
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        return Enum.GetValues<EmailTemplateCategory>()
            .Select(category => new TemplateCategoryInfo
            {
                Category = category,
                Name = category.ToString(),
                Description = GetCategoryDescription(category),
                TemplateCount = templates.Count(t => t.Category == category)
            })
            .ToList();
    }

    private string GetCategoryDescription(EmailTemplateCategory category)
    {
        return category switch
        {
            EmailTemplateCategory.Sales => "Sales and opportunity-related communications",
            EmailTemplateCategory.Marketing => "Marketing campaigns and promotional emails",
            EmailTemplateCategory.Support => "Customer support and service desk communications",
            EmailTemplateCategory.Billing => "Invoice, payment, and billing notifications",
            EmailTemplateCategory.System => "System notifications and alerts",
            EmailTemplateCategory.Internal => "Internal team communications",
            _ => "General email templates"
        };
    }

    #endregion

    #region Template Variables

    public async Task<IEnumerable<TemplateVariable>> GetAvailableVariablesAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var variables = new List<TemplateVariable>
        {
            new() { Name = "FirstName", Description = "Recipient's first name", DataType = "string", IsRequired = false, SampleValue = "John" },
            new() { Name = "LastName", Description = "Recipient's last name", DataType = "string", IsRequired = false, SampleValue = "Doe" },
            new() { Name = "Email", Description = "Recipient's email address", DataType = "string", IsRequired = false, SampleValue = "john@example.com" },
            new() { Name = "CompanyName", Description = "Company name", DataType = "string", IsRequired = false, SampleValue = "Acme Corp" },
            new() { Name = "Date", Description = "Current date", DataType = "date", IsRequired = false, SampleValue = "January 15, 2025" }
        };

        switch (category)
        {
            case EmailTemplateCategory.Sales:
                variables.Add(new() { Name = "OpportunityName", Description = "Opportunity name", DataType = "string", SampleValue = "Enterprise Deal" });
                variables.Add(new() { Name = "Amount", Description = "Deal amount", DataType = "currency", SampleValue = "$50,000" });
                variables.Add(new() { Name = "Stage", Description = "Sales stage", DataType = "string", SampleValue = "Negotiation" });
                variables.Add(new() { Name = "SalesRepName", Description = "Sales rep name", DataType = "string", SampleValue = "Jane Smith" });
                break;

            case EmailTemplateCategory.Billing:
                variables.Add(new() { Name = "InvoiceNumber", Description = "Invoice number", DataType = "string", SampleValue = "INV-2025-0001" });
                variables.Add(new() { Name = "InvoiceAmount", Description = "Invoice total", DataType = "currency", SampleValue = "$1,500.00" });
                variables.Add(new() { Name = "DueDate", Description = "Payment due date", DataType = "date", SampleValue = "February 15, 2025" });
                break;

            case EmailTemplateCategory.Support:
                variables.Add(new() { Name = "TicketNumber", Description = "Support ticket number", DataType = "string", SampleValue = "TKT-2025-0001" });
                variables.Add(new() { Name = "TicketSubject", Description = "Ticket subject", DataType = "string", SampleValue = "Login issue" });
                variables.Add(new() { Name = "Priority", Description = "Ticket priority", DataType = "string", SampleValue = "High" });
                variables.Add(new() { Name = "AgentName", Description = "Support agent name", DataType = "string", SampleValue = "Support Team" });
                break;
        }

        return variables;
    }

    public async Task<IEnumerable<string>> ExtractVariablesAsync(string templateContent, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return ExtractVariablesSync(templateContent);
    }

    private List<string> ExtractVariablesSync(string content)
    {
        var matches = VariablePattern.Matches(content);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    #endregion

    #region Cloning & Import/Export

    public async Task<EmailTemplate> CloneAsync(int templateId, string newName, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        var clone = new EmailTemplate
        {
            Name = newName,
            Description = template.Description,
            Category = template.Category,
            Subject = template.Subject,
            HtmlBody = template.HtmlBody,
            PlainTextBody = template.PlainTextBody,
            FromName = template.FromName,
            FromEmail = template.FromEmail,
            ReplyToEmail = template.ReplyToEmail,
            IsActive = true,
            IsSystem = false,
            Slug = GenerateSlug(newName),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailTemplates.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned template {SourceId} to new template {NewId}", templateId, clone.Id);
        return clone;
    }

    public async Task<EmailTemplate> ImportAsync(string templateJson, CancellationToken cancellationToken = default)
    {
        var templateData = JsonSerializer.Deserialize<Dictionary<string, object>>(templateJson);
        if (templateData == null)
        {
            throw new ArgumentException("Invalid template JSON");
        }

        var template = new EmailTemplate
        {
            Name = templateData.TryGetValue("Name", out var name) ? name?.ToString() ?? "Imported Template" : "Imported Template",
            Subject = templateData.TryGetValue("Subject", out var subject) ? subject?.ToString() ?? string.Empty : string.Empty,
            HtmlBody = templateData.TryGetValue("HtmlBody", out var html) ? html?.ToString() : null,
            PlainTextBody = templateData.TryGetValue("TextBody", out var text) ? text?.ToString() : null,
            IsActive = true,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        template.Slug = GenerateSlug(template.Name);

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Imported template {TemplateName}", template.Name);
        return template;
    }

    public async Task<string> ExportAsync(int templateId, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        var exportData = new Dictionary<string, object?>
        {
            ["Name"] = template.Name,
            ["Description"] = template.Description,
            ["Category"] = template.Category.ToString(),
            ["Subject"] = template.Subject,
            ["HtmlBody"] = template.HtmlBody,
            ["TextBody"] = template.PlainTextBody,
            ["FromName"] = template.FromName,
            ["FromEmail"] = template.FromEmail,
            ["ReplyTo"] = template.ReplyToEmail,
            ["ExportedAt"] = DateTime.UtcNow.ToString("O")
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
    }

    #endregion

    #region Statistics & Usage

    public async Task<TemplateUsageStats> GetUsageStatsAsync(int templateId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        return new TemplateUsageStats
        {
            TemplateId = templateId,
            TemplateName = template.Name,
            TotalUsages = template.UsageCount,
            UniqueUsers = 0, // Would need usage tracking table
            LastUsedAt = template.LastUsedAt,
            UsageHistory = new List<UsageByDay>()
        };
    }

    public async Task RecordUsageAsync(int templateId, int? userId = null, string? context = null, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates.FindAsync(new object[] { templateId }, cancellationToken);
        if (template == null) return;

        template.UsageCount = template.UsageCount + 1;
        template.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded usage for template {TemplateId} by user {UserId}", templateId, userId);
    }

    public async Task<IEnumerable<TemplateUsageSummary>> GetMostUsedAsync(int topN = 10, DateTime? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.EmailTemplates.Where(t => !t.IsDeleted && t.UsageCount > 0);

        return await query
            .OrderByDescending(t => t.UsageCount)
            .Take(topN)
            .Select(t => new TemplateUsageSummary
            {
                TemplateId = t.Id,
                TemplateName = t.Name,
                Category = t.Category,
                UsageCount = t.UsageCount
            })
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Default Templates

    public async Task<EmailTemplate?> GetDefaultTemplateAsync(EmailTemplatePurpose purpose, CancellationToken cancellationToken = default)
    {
        var purposeName = purpose.ToString();
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Slug == purposeName.ToLower() && !t.IsDeleted && t.IsActive, cancellationToken);
    }

    public async Task<bool> SetAsDefaultAsync(int templateId, EmailTemplatePurpose purpose, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        template.Slug = purpose.ToString().ToLower();
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Set template {TemplateId} as default for purpose {Purpose}", templateId, purpose);
        return true;
    }

    #endregion

    #region Helper Methods

    private string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("_", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    private int GetLineNumber(string content, int position)
    {
        return content.Substring(0, position).Count(c => c == '\n') + 1;
    }

    private int GetColumnNumber(string content, int position)
    {
        var lastNewLine = content.LastIndexOf('\n', position);
        return position - lastNewLine;
    }

    #endregion
}
