// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for email template management operations.
/// Handles email template lifecycle, rendering, and testing.
/// </summary>
public interface IEmailTemplateService
{
    #region CRUD Operations

    /// <summary>Gets all email templates with optional filtering.</summary>
    Task<IEnumerable<EmailTemplate>> GetAllAsync(
        EmailTemplateCategory? category = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an email template by ID.</summary>
    Task<EmailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets an email template by name.</summary>
    Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Gets an email template by slug/key.</summary>
    Task<EmailTemplate?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Creates a new email template.</summary>
    Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing email template.</summary>
    Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    /// <summary>Deletes an email template (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Template Rendering

    /// <summary>Renders a template with data.</summary>
    Task<RenderedEmail> RenderAsync(int templateId, Dictionary<string, object> data, CancellationToken cancellationToken = default);

    /// <summary>Renders a template by name with data.</summary>
    Task<RenderedEmail> RenderByNameAsync(string templateName, Dictionary<string, object> data, CancellationToken cancellationToken = default);

    /// <summary>Renders a template for a specific entity.</summary>
    Task<RenderedEmail> RenderForEntityAsync(int templateId, string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>Previews a template with sample data.</summary>
    Task<RenderedEmail> PreviewAsync(int templateId, CancellationToken cancellationToken = default);

    /// <summary>Validates template syntax.</summary>
    Task<TemplateValidationResult> ValidateAsync(string templateContent, CancellationToken cancellationToken = default);

    #endregion

    #region Template Testing

    /// <summary>Sends a test email using a template.</summary>
    Task<bool> SendTestAsync(int templateId, string recipientEmail, Dictionary<string, object>? testData = null, CancellationToken cancellationToken = default);

    /// <summary>Gets sample data for a template category.</summary>
    Task<Dictionary<string, object>> GetSampleDataAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default);

    #endregion

    #region Template Versioning

    /// <summary>Gets version history for a template.</summary>
    Task<IEnumerable<EmailTemplateVersion>> GetVersionHistoryAsync(int templateId, CancellationToken cancellationToken = default);

    /// <summary>Gets a specific version of a template.</summary>
    Task<EmailTemplateVersion?> GetVersionAsync(int templateId, int version, CancellationToken cancellationToken = default);

    /// <summary>Restores a previous version.</summary>
    Task<EmailTemplate> RestoreVersionAsync(int templateId, int version, CancellationToken cancellationToken = default);

    /// <summary>Creates a new version of a template.</summary>
    Task<EmailTemplateVersion> CreateVersionAsync(int templateId, string changeDescription, CancellationToken cancellationToken = default);

    #endregion

    #region Template Categories

    /// <summary>Gets templates by category.</summary>
    Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default);

    /// <summary>Gets available template categories.</summary>
    Task<IEnumerable<TemplateCategoryInfo>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Template Variables

    /// <summary>Gets available variables for a template category.</summary>
    Task<IEnumerable<TemplateVariable>> GetAvailableVariablesAsync(EmailTemplateCategory category, CancellationToken cancellationToken = default);

    /// <summary>Extracts variables used in a template.</summary>
    Task<IEnumerable<string>> ExtractVariablesAsync(string templateContent, CancellationToken cancellationToken = default);

    #endregion

    #region Cloning & Duplication

    /// <summary>Clones a template.</summary>
    Task<EmailTemplate> CloneAsync(int templateId, string newName, CancellationToken cancellationToken = default);

    /// <summary>Imports a template from JSON.</summary>
    Task<EmailTemplate> ImportAsync(string templateJson, CancellationToken cancellationToken = default);

    /// <summary>Exports a template to JSON.</summary>
    Task<string> ExportAsync(int templateId, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics & Usage

    /// <summary>Gets template usage statistics.</summary>
    Task<TemplateUsageStats> GetUsageStatsAsync(int templateId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Records template usage.</summary>
    Task RecordUsageAsync(int templateId, int? userId = null, string? context = null, CancellationToken cancellationToken = default);

    /// <summary>Gets most used templates.</summary>
    Task<IEnumerable<TemplateUsageSummary>> GetMostUsedAsync(int topN = 10, DateTime? fromDate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Default Templates

    /// <summary>Gets the default template for a specific purpose.</summary>
    Task<EmailTemplate?> GetDefaultTemplateAsync(EmailTemplatePurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>Sets a template as the default for a purpose.</summary>
    Task<bool> SetAsDefaultAsync(int templateId, EmailTemplatePurpose purpose, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Rendered email result.
/// </summary>
public class RenderedEmail
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public string? ReplyTo { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Template validation result.
/// </summary>
public class TemplateValidationResult
{
    public bool IsValid { get; set; }
    public List<TemplateValidationError> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> UsedVariables { get; set; } = new();
}

/// <summary>
/// Template validation error.
/// </summary>
public class TemplateValidationError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
}

/// <summary>
/// Email template version.
/// </summary>
public class EmailTemplateVersion
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int Version { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? ChangeDescription { get; set; }
    public int? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Template category information.
/// </summary>
public class TemplateCategoryInfo
{
    public EmailTemplateCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TemplateCount { get; set; }
}

/// <summary>
/// Template variable information.
/// </summary>
public class TemplateVariable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? SampleValue { get; set; }
}

/// <summary>
/// Template usage statistics.
/// </summary>
public class TemplateUsageStats
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalUsages { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public List<UsageByDay> UsageHistory { get; set; } = new();
}

/// <summary>
/// Usage by day.
/// </summary>
public class UsageByDay
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Template usage summary.
/// </summary>
public class TemplateUsageSummary
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public EmailTemplateCategory Category { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// Email template purpose.
/// </summary>
public enum EmailTemplatePurpose
{
    WelcomeEmail,
    PasswordReset,
    OrderConfirmation,
    InvoiceNotification,
    QuoteApproval,
    LeadFollowUp,
    OpportunityCreated,
    ContractSigning,
    SubscriptionRenewal,
    SupportTicketCreated,
    SupportTicketResolved,
    CampaignMarketing,
    NewsletterWeekly,
    NewsletterMonthly,
    EventInvitation,
    EventReminder,
    ReferralRequest,
    FeedbackRequest,
    AccountActivation,
    AccountDeactivation
}
