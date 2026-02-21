// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

using CRM.Core.Entities;

/// <summary>
/// Service interface for form builder operations - creating, managing, and processing web forms
/// for lead capture and data collection.
/// </summary>
public interface IFormBuilderService
{
    #region Form Definition CRUD

    /// <summary>
    /// Gets all form definitions with optional filtering.
    /// </summary>
    Task<IEnumerable<FormDefinition>> GetAllFormsAsync(
        FormStatus? status = null,
        int? ownerId = null,
        int? campaignId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a form definition by ID with all fields.
    /// </summary>
    Task<FormDefinition?> GetFormByIdAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a form by its form key (unique identifier).
    /// </summary>
    Task<FormDefinition?> GetFormByKeyAsync(string formKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new form definition.
    /// </summary>
    Task<FormDefinition> CreateFormAsync(FormDefinition form, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing form definition.
    /// </summary>
    Task<FormDefinition> UpdateFormAsync(FormDefinition form, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a form definition (soft delete).
    /// </summary>
    Task<bool> DeleteFormAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones an existing form to create a new one.
    /// </summary>
    Task<FormDefinition> CloneFormAsync(int formId, string newName, CancellationToken cancellationToken = default);

    #endregion

    #region Form Status Management

    /// <summary>
    /// Publishes a form (makes it active for submissions).
    /// </summary>
    Task<FormDefinition> PublishFormAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublishes/pauses a form.
    /// </summary>
    Task<FormDefinition> UnpublishFormAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a form.
    /// </summary>
    Task<FormDefinition> ArchiveFormAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates form status.
    /// </summary>
    Task<FormDefinition> UpdateFormStatusAsync(int formId, FormStatus status, CancellationToken cancellationToken = default);

    #endregion

    #region Form Field Management

    /// <summary>
    /// Gets all fields for a form.
    /// </summary>
    Task<IEnumerable<FormField>> GetFormFieldsAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific field by ID.
    /// </summary>
    Task<FormField?> GetFieldByIdAsync(int fieldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new field to a form.
    /// </summary>
    Task<FormField> AddFieldAsync(int formId, FormField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing field.
    /// </summary>
    Task<FormField> UpdateFieldAsync(FormField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a field from a form.
    /// </summary>
    Task<bool> RemoveFieldAsync(int fieldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders fields within a form.
    /// </summary>
    Task<IEnumerable<FormField>> ReorderFieldsAsync(int formId, IEnumerable<int> fieldIdsInOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk updates field properties.
    /// </summary>
    Task<IEnumerable<FormField>> BulkUpdateFieldsAsync(
        int formId,
        IEnumerable<FormField> fields,
        CancellationToken cancellationToken = default);

    #endregion

    #region Form Submission Processing

    /// <summary>
    /// Gets all submissions for a form with optional filtering.
    /// </summary>
    Task<IEnumerable<FormSubmission>> GetSubmissionsAsync(
        int formId,
        SubmissionStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific submission by ID.
    /// </summary>
    Task<FormSubmission?> GetSubmissionByIdAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a submission by its reference number.
    /// </summary>
    Task<FormSubmission?> GetSubmissionByNumberAsync(string submissionNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a new form submission.
    /// </summary>
    Task<FormSubmissionResult> ProcessSubmissionAsync(
        int formId,
        Dictionary<string, object> formData,
        FormSubmissionContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reprocesses a failed submission.
    /// </summary>
    Task<FormSubmissionResult> ReprocessSubmissionAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a submission as spam.
    /// </summary>
    Task<FormSubmission> MarkAsSpamAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a submission as not spam (restores).
    /// </summary>
    Task<FormSubmission> MarkAsNotSpamAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a submission.
    /// </summary>
    Task<bool> DeleteSubmissionAsync(int submissionId, CancellationToken cancellationToken = default);

    #endregion

    #region Double Opt-In

    /// <summary>
    /// Sends opt-in confirmation email.
    /// </summary>
    Task<bool> SendOptInConfirmationAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms double opt-in from email link.
    /// </summary>
    Task<FormSubmission?> ConfirmOptInAsync(string confirmationToken, CancellationToken cancellationToken = default);

    #endregion

    #region Validation

    /// <summary>
    /// Validates form data against form field definitions.
    /// </summary>
    Task<FormValidationResult> ValidateFormDataAsync(
        int formId,
        Dictionary<string, object> formData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a field value is valid according to field definition.
    /// </summary>
    Task<FieldValidationResult> ValidateFieldAsync(
        int fieldId,
        object? value,
        CancellationToken cancellationToken = default);

    #endregion

    #region Spam Protection

    /// <summary>
    /// Calculates spam score for a submission.
    /// </summary>
    Task<int> CalculateSpamScoreAsync(
        int formId,
        Dictionary<string, object> formData,
        FormSubmissionContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if submission appears to be spam.
    /// </summary>
    Task<bool> IsSpamAsync(
        int formId,
        Dictionary<string, object> formData,
        FormSubmissionContext? context = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Embedding & URLs

    /// <summary>
    /// Generates embed code for a form.
    /// </summary>
    Task<string> GenerateEmbedCodeAsync(int formId, string? baseUrl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates direct URL for standalone form page.
    /// </summary>
    Task<string> GenerateDirectUrlAsync(int formId, string? baseUrl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates embedding domain.
    /// </summary>
    Task<bool> IsValidEmbeddingDomainAsync(int formId, string domain, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Increments form view count.
    /// </summary>
    Task IncrementViewCountAsync(int formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets form statistics.
    /// </summary>
    Task<FormStatistics> GetFormStatisticsAsync(
        int formId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets submission statistics across all forms.
    /// </summary>
    Task<FormSubmissionStatistics> GetSubmissionStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets field-level statistics (completion rates, etc.).
    /// </summary>
    Task<IEnumerable<FormFieldStatistics>> GetFieldStatisticsAsync(
        int formId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Templates

    /// <summary>
    /// Gets available form templates.
    /// </summary>
    Task<IEnumerable<FormTemplate>> GetFormTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new form from a template.
    /// </summary>
    Task<FormDefinition> CreateFromTemplateAsync(
        string templateKey,
        string formName,
        int? ownerId = null,
        CancellationToken cancellationToken = default);

    #endregion
}

#region Supporting Types

/// <summary>
/// Context information about a form submission.
/// </summary>
public class FormSubmissionContext
{
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public string? PageUrl { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    public int? WebVisitorId { get; set; }
    public string? HoneypotValue { get; set; }
    public TimeSpan? SubmissionDuration { get; set; }
}

/// <summary>
/// Result of processing a form submission.
/// </summary>
public class FormSubmissionResult
{
    public bool Success { get; set; }
    public FormSubmission? Submission { get; set; }
    public int? LeadId { get; set; }
    public int? ContactId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresOptIn { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ThankYouMessage { get; set; }
    public FormValidationResult? ValidationResult { get; set; }
}

/// <summary>
/// Result of form validation.
/// </summary>
public class FormValidationResult
{
    public bool IsValid { get; set; }
    public List<FieldValidationError> Errors { get; set; } = new();
}

/// <summary>
/// A single field validation error.
/// </summary>
public class FieldValidationError
{
    public string FieldName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Result of validating a single field.
/// </summary>
public class FieldValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Form statistics.
/// </summary>
public class FormStatistics
{
    public int FormId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalSubmissions { get; set; }
    public int SuccessfulSubmissions { get; set; }
    public int FailedSubmissions { get; set; }
    public int SpamSubmissions { get; set; }
    public decimal ConversionRate { get; set; }
    public int LeadsCreated { get; set; }
    public int ContactsCreated { get; set; }
    public TimeSpan? AverageSubmissionTime { get; set; }
    public DateTime? LastSubmissionAt { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Overall submission statistics.
/// </summary>
public class FormSubmissionStatistics
{
    public int TotalForms { get; set; }
    public int ActiveForms { get; set; }
    public int TotalSubmissions { get; set; }
    public int NewSubmissions { get; set; }
    public int ProcessedSubmissions { get; set; }
    public int FailedSubmissions { get; set; }
    public int SpamSubmissions { get; set; }
    public decimal OverallConversionRate { get; set; }
    public List<FormSubmissionByDay> SubmissionsByDay { get; set; } = new();
    public List<TopFormSummary> TopForms { get; set; } = new();
}

/// <summary>
/// Submissions grouped by day.
/// </summary>
public class FormSubmissionByDay
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Summary of top-performing form.
/// </summary>
public class TopFormSummary
{
    public int FormId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public int Submissions { get; set; }
    public decimal ConversionRate { get; set; }
}

/// <summary>
/// Field-level statistics.
/// </summary>
public class FormFieldStatistics
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public FormFieldType FieldType { get; set; }
    public int TotalSubmissions { get; set; }
    public int FilledCount { get; set; }
    public int EmptyCount { get; set; }
    public decimal CompletionRate { get; set; }
    public Dictionary<string, int>? ValueDistribution { get; set; }
}

/// <summary>
/// Form template for quick form creation.
/// </summary>
public class FormTemplate
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? PreviewImageUrl { get; set; }
    public List<FormFieldTemplate> Fields { get; set; } = new();
}

/// <summary>
/// Field template within a form template.
/// </summary>
public class FormFieldTemplate
{
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FormFieldType FieldType { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public string? Placeholder { get; set; }
    public string? CrmFieldMapping { get; set; }
    public string? Options { get; set; }
}

#endregion
