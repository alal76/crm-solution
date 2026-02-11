// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Infrastructure.Services;

using System.Text.Json;
using System.Text.RegularExpressions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for form builder operations - creating, managing, and processing web forms
/// for lead capture and data collection.
/// </summary>
public class FormBuilderService : IFormBuilderService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<FormBuilderService> _logger;
    private readonly ILeadRoutingService? _leadRoutingService;

    public FormBuilderService(
        ICrmDbContext context,
        ILogger<FormBuilderService> logger,
        ILeadRoutingService? leadRoutingService = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _leadRoutingService = leadRoutingService;
    }

    #region Form Definition CRUD

    public async Task<IEnumerable<FormDefinition>> GetAllFormsAsync(
        FormStatus? status = null,
        int? ownerId = null,
        int? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FormDefinitions
            .Include(f => f.Owner)
            .Include(f => f.Fields.OrderBy(ff => ff.Order))
            .Where(f => !f.IsDeleted);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        if (ownerId.HasValue)
            query = query.Where(f => f.OwnerId == ownerId.Value);

        if (campaignId.HasValue)
            query = query.Where(f => f.CampaignId == campaignId.Value);

        return await query.OrderBy(f => f.Name).ToListAsync(cancellationToken);
    }

    public async Task<FormDefinition?> GetFormByIdAsync(int formId, CancellationToken cancellationToken = default)
    {
        return await _context.FormDefinitions
            .Include(f => f.Owner)
            .Include(f => f.Fields.OrderBy(ff => ff.Order))
            .Include(f => f.Campaign)
            .FirstOrDefaultAsync(f => f.Id == formId && !f.IsDeleted, cancellationToken);
    }

    public async Task<FormDefinition?> GetFormByKeyAsync(string formKey, CancellationToken cancellationToken = default)
    {
        return await _context.FormDefinitions
            .Include(f => f.Owner)
            .Include(f => f.Fields.OrderBy(ff => ff.Order))
            .FirstOrDefaultAsync(f => f.FormKey == formKey && !f.IsDeleted, cancellationToken);
    }

    public async Task<FormDefinition> CreateFormAsync(FormDefinition form, CancellationToken cancellationToken = default)
    {
        // Generate form key if not provided
        if (string.IsNullOrEmpty(form.FormKey))
        {
            form.FormKey = GenerateFormKey(form.Name);
        }

        form.CreatedAt = DateTime.UtcNow;
        form.Status = FormStatus.Draft;

        _context.FormDefinitions.Add(form);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created form {FormId}: {FormName}", form.Id, form.Name);
        return form;
    }

    public async Task<FormDefinition> UpdateFormAsync(FormDefinition form, CancellationToken cancellationToken = default)
    {
        var existing = await _context.FormDefinitions
            .FirstOrDefaultAsync(f => f.Id == form.Id && !f.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Form {form.Id} not found");

        existing.Name = form.Name;
        existing.Description = form.Description;
        existing.Title = form.Title;
        existing.Subtitle = form.Subtitle;
        existing.SubmitButtonText = form.SubmitButtonText;
        existing.Width = form.Width;
        existing.CssClasses = form.CssClasses;
        existing.CustomCss = form.CustomCss;
        existing.CustomJs = form.CustomJs;
        existing.Theme = form.Theme;
        existing.SubmitAction = form.SubmitAction;
        existing.ThankYouMessage = form.ThankYouMessage;
        existing.RedirectUrl = form.RedirectUrl;
        existing.DoubleOptIn = form.DoubleOptIn;
        existing.DoubleOptInTemplateId = form.DoubleOptInTemplateId;
        existing.SpamProtection = form.SpamProtection;
        existing.CaptchaType = form.CaptchaType;
        existing.HoneypotFieldName = form.HoneypotFieldName;
        existing.CreateLead = form.CreateLead;
        existing.LeadSource = form.LeadSource;
        existing.DefaultLeadOwnerId = form.DefaultLeadOwnerId;
        existing.LeadRoutingRuleId = form.LeadRoutingRuleId;
        existing.UpdateExistingLead = form.UpdateExistingLead;
        existing.ExistingLeadMatchField = form.ExistingLeadMatchField;
        existing.CampaignId = form.CampaignId;
        existing.CampaignMemberStatus = form.CampaignMemberStatus;
        existing.NotifyOwner = form.NotifyOwner;
        existing.NotificationRecipients = form.NotificationRecipients;
        existing.NotificationTemplateId = form.NotificationTemplateId;
        existing.SendAutoresponder = form.SendAutoresponder;
        existing.AutoresponderTemplateId = form.AutoresponderTemplateId;
        existing.AllowedDomains = form.AllowedDomains;
        existing.OwnerId = form.OwnerId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated form {FormId}: {FormName}", form.Id, form.Name);
        return existing;
    }

    public async Task<bool> DeleteFormAsync(int formId, CancellationToken cancellationToken = default)
    {
        var form = await _context.FormDefinitions
            .FirstOrDefaultAsync(f => f.Id == formId && !f.IsDeleted, cancellationToken);

        if (form == null)
            return false;

        form.IsDeleted = true;
        form.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted form {FormId}: {FormName}", formId, form.Name);
        return true;
    }

    public async Task<FormDefinition> CloneFormAsync(int formId, string newName, CancellationToken cancellationToken = default)
    {
        var original = await GetFormByIdAsync(formId, cancellationToken);
        if (original == null)
            throw new InvalidOperationException($"Form {formId} not found");

        var clone = new FormDefinition
        {
            Name = newName,
            FormKey = GenerateFormKey(newName),
            Description = original.Description,
            Status = FormStatus.Draft,
            Title = original.Title,
            Subtitle = original.Subtitle,
            SubmitButtonText = original.SubmitButtonText,
            Width = original.Width,
            CssClasses = original.CssClasses,
            CustomCss = original.CustomCss,
            CustomJs = original.CustomJs,
            Theme = original.Theme,
            SubmitAction = original.SubmitAction,
            ThankYouMessage = original.ThankYouMessage,
            RedirectUrl = original.RedirectUrl,
            DoubleOptIn = original.DoubleOptIn,
            SpamProtection = original.SpamProtection,
            CaptchaType = original.CaptchaType,
            HoneypotFieldName = original.HoneypotFieldName,
            CreateLead = original.CreateLead,
            LeadSource = original.LeadSource,
            OwnerId = original.OwnerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.FormDefinitions.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        // Clone fields
        foreach (var field in original.Fields)
        {
            var clonedField = new FormField
            {
                FormDefinitionId = clone.Id,
                FieldName = field.FieldName,
                Label = field.Label,
                FieldType = field.FieldType,
                Order = field.Order,
                IsRequired = field.IsRequired,
                RequiredMessage = field.RequiredMessage,
                MinLength = field.MinLength,
                MaxLength = field.MaxLength,
                MinValue = field.MinValue,
                MaxValue = field.MaxValue,
                ValidationPattern = field.ValidationPattern,
                ValidationMessage = field.ValidationMessage,
                Placeholder = field.Placeholder,
                HelpText = field.HelpText,
                DefaultValue = field.DefaultValue,
                Width = field.Width,
                CssClasses = field.CssClasses,
                Options = field.Options,
                CrmFieldMapping = field.CrmFieldMapping,
                CrmEntityMapping = field.CrmEntityMapping,
                HasConditionalLogic = field.HasConditionalLogic,
                ConditionalLogic = field.ConditionalLogic,
                CreatedAt = DateTime.UtcNow
            };
            _context.FormFields.Add(clonedField);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cloned form {OriginalId} to new form {CloneId}: {CloneName}",
            formId, clone.Id, newName);

        return clone;
    }

    #endregion

    #region Form Status Management

    public async Task<FormDefinition> PublishFormAsync(int formId, CancellationToken cancellationToken = default)
    {
        return await UpdateFormStatusAsync(formId, FormStatus.Published, cancellationToken);
    }

    public async Task<FormDefinition> UnpublishFormAsync(int formId, CancellationToken cancellationToken = default)
    {
        return await UpdateFormStatusAsync(formId, FormStatus.Paused, cancellationToken);
    }

    public async Task<FormDefinition> ArchiveFormAsync(int formId, CancellationToken cancellationToken = default)
    {
        return await UpdateFormStatusAsync(formId, FormStatus.Archived, cancellationToken);
    }

    public async Task<FormDefinition> UpdateFormStatusAsync(int formId, FormStatus status, CancellationToken cancellationToken = default)
    {
        var form = await GetFormByIdAsync(formId, cancellationToken);
        if (form == null)
            throw new InvalidOperationException($"Form {formId} not found");

        var previousStatus = form.Status;
        form.Status = status;
        form.UpdatedAt = DateTime.UtcNow;

        // Generate embed code when publishing
        if (status == FormStatus.Published && string.IsNullOrEmpty(form.EmbedCode))
        {
            form.EmbedCode = await GenerateEmbedCodeAsync(formId, null, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated form {FormId} status from {OldStatus} to {NewStatus}",
            formId, previousStatus, status);

        return form;
    }

    #endregion

    #region Form Field Management

    public async Task<IEnumerable<FormField>> GetFormFieldsAsync(int formId, CancellationToken cancellationToken = default)
    {
        return await _context.FormFields
            .Where(f => f.FormDefinitionId == formId && !f.IsDeleted)
            .OrderBy(f => f.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<FormField?> GetFieldByIdAsync(int fieldId, CancellationToken cancellationToken = default)
    {
        return await _context.FormFields
            .FirstOrDefaultAsync(f => f.Id == fieldId && !f.IsDeleted, cancellationToken);
    }

    public async Task<FormField> AddFieldAsync(int formId, FormField field, CancellationToken cancellationToken = default)
    {
        field.FormDefinitionId = formId;
        field.CreatedAt = DateTime.UtcNow;

        // Auto-assign order if not set
        if (field.Order == 0)
        {
            var maxOrder = await _context.FormFields
                .Where(f => f.FormDefinitionId == formId && !f.IsDeleted)
                .MaxAsync(f => (int?)f.Order, cancellationToken) ?? 0;
            field.Order = maxOrder + 1;
        }

        _context.FormFields.Add(field);
        await _context.SaveChangesAsync(cancellationToken);
        return field;
    }

    public async Task<FormField> UpdateFieldAsync(FormField field, CancellationToken cancellationToken = default)
    {
        var existing = await _context.FormFields
            .FirstOrDefaultAsync(f => f.Id == field.Id && !f.IsDeleted, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"Field {field.Id} not found");

        existing.FieldName = field.FieldName;
        existing.Label = field.Label;
        existing.FieldType = field.FieldType;
        existing.Order = field.Order;
        existing.IsRequired = field.IsRequired;
        existing.RequiredMessage = field.RequiredMessage;
        existing.MinLength = field.MinLength;
        existing.MaxLength = field.MaxLength;
        existing.MinValue = field.MinValue;
        existing.MaxValue = field.MaxValue;
        existing.ValidationPattern = field.ValidationPattern;
        existing.ValidationMessage = field.ValidationMessage;
        existing.Placeholder = field.Placeholder;
        existing.HelpText = field.HelpText;
        existing.DefaultValue = field.DefaultValue;
        existing.Width = field.Width;
        existing.CssClasses = field.CssClasses;
        existing.IsHidden = field.IsHidden;
        existing.IsReadOnly = field.IsReadOnly;
        existing.Options = field.Options;
        existing.AllowOther = field.AllowOther;
        existing.CrmFieldMapping = field.CrmFieldMapping;
        existing.CrmEntityMapping = field.CrmEntityMapping;
        existing.HasConditionalLogic = field.HasConditionalLogic;
        existing.ConditionalLogic = field.ConditionalLogic;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveFieldAsync(int fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _context.FormFields
            .FirstOrDefaultAsync(f => f.Id == fieldId && !f.IsDeleted, cancellationToken);

        if (field == null)
            return false;

        field.IsDeleted = true;
        field.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<FormField>> ReorderFieldsAsync(
        int formId,
        IEnumerable<int> fieldIdsInOrder,
        CancellationToken cancellationToken = default)
    {
        var fields = await _context.FormFields
            .Where(f => f.FormDefinitionId == formId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        var order = 1;
        foreach (var fieldId in fieldIdsInOrder)
        {
            var field = fields.FirstOrDefault(f => f.Id == fieldId);
            if (field != null)
            {
                field.Order = order++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return fields.OrderBy(f => f.Order);
    }

    public async Task<IEnumerable<FormField>> BulkUpdateFieldsAsync(
        int formId,
        IEnumerable<FormField> fields,
        CancellationToken cancellationToken = default)
    {
        var result = new List<FormField>();
        foreach (var field in fields)
        {
            if (field.Id > 0)
            {
                result.Add(await UpdateFieldAsync(field, cancellationToken));
            }
            else
            {
                result.Add(await AddFieldAsync(formId, field, cancellationToken));
            }
        }
        return result;
    }

    #endregion

    #region Form Submission Processing

    public async Task<IEnumerable<FormSubmission>> GetSubmissionsAsync(
        int formId,
        SubmissionStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FormSubmissions
            .Include(s => s.Lead)
            .Include(s => s.Contact)
            .Where(s => s.FormDefinitionId == formId && !s.IsDeleted);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.SubmittedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.SubmittedAt <= toDate.Value);

        return await query.OrderByDescending(s => s.SubmittedAt).ToListAsync(cancellationToken);
    }

    public async Task<FormSubmission?> GetSubmissionByIdAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        return await _context.FormSubmissions
            .Include(s => s.FormDefinition)
            .Include(s => s.Lead)
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(s => s.Id == submissionId && !s.IsDeleted, cancellationToken);
    }

    public async Task<FormSubmission?> GetSubmissionByNumberAsync(string submissionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.FormSubmissions
            .Include(s => s.FormDefinition)
            .FirstOrDefaultAsync(s => s.SubmissionNumber == submissionNumber && !s.IsDeleted, cancellationToken);
    }

    public async Task<FormSubmissionResult> ProcessSubmissionAsync(
        int formId,
        Dictionary<string, object> formData,
        FormSubmissionContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var form = await GetFormByIdAsync(formId, cancellationToken);
        if (form == null)
            return new FormSubmissionResult { Success = false, ErrorMessage = "Form not found" };

        if (form.Status != FormStatus.Published)
            return new FormSubmissionResult { Success = false, ErrorMessage = "Form is not accepting submissions" };

        // Validate form data
        var validation = await ValidateFormDataAsync(formId, formData, cancellationToken);
        if (!validation.IsValid)
            return new FormSubmissionResult { Success = false, ValidationResult = validation };

        // Check for spam
        if (form.SpamProtection)
        {
            var isSpam = await IsSpamAsync(formId, formData, context, cancellationToken);
            if (isSpam)
            {
                // Create submission but mark as spam
                var spamSubmission = await CreateSubmissionRecord(form, formData, context, SubmissionStatus.Spam, cancellationToken);
                return new FormSubmissionResult
                {
                    Success = false,
                    Submission = spamSubmission,
                    ErrorMessage = "Submission flagged as spam"
                };
            }
        }

        // Create submission record
        var submission = await CreateSubmissionRecord(form, formData, context, SubmissionStatus.Processing, cancellationToken);

        int? leadId = null;
        int? contactId = null;

        // Create/update lead if configured
        if (form.CreateLead)
        {
            try
            {
                leadId = await CreateOrUpdateLeadAsync(form, formData, cancellationToken);
                submission.LeadId = leadId;
                submission.Status = SubmissionStatus.LeadCreated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create lead from form submission {SubmissionId}", submission.Id);
                submission.Status = SubmissionStatus.Failed;
                submission.ErrorMessage = ex.Message;
            }
        }
        else
        {
            submission.Status = SubmissionStatus.New;
        }

        submission.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Increment submission count
        form.TotalSubmissions++;
        await _context.SaveChangesAsync(cancellationToken);

        return new FormSubmissionResult
        {
            Success = true,
            Submission = submission,
            LeadId = leadId,
            ContactId = contactId,
            RequiresOptIn = form.DoubleOptIn && !submission.OptInConfirmed,
            RedirectUrl = form.SubmitAction == FormSubmitAction.Redirect ? form.RedirectUrl : null,
            ThankYouMessage = form.SubmitAction == FormSubmitAction.ShowMessage ? form.ThankYouMessage : null
        };
    }

    public async Task<FormSubmissionResult> ReprocessSubmissionAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await GetSubmissionByIdAsync(submissionId, cancellationToken);
        if (submission == null)
            return new FormSubmissionResult { Success = false, ErrorMessage = "Submission not found" };

        var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(submission.FormData) ?? new();
        return await ProcessSubmissionAsync(submission.FormDefinitionId, formData, null, cancellationToken);
    }

    public async Task<FormSubmission> MarkAsSpamAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await GetSubmissionByIdAsync(submissionId, cancellationToken);
        if (submission == null)
            throw new InvalidOperationException($"Submission {submissionId} not found");

        submission.Status = SubmissionStatus.Spam;
        submission.IsSpam = true;
        await _context.SaveChangesAsync(cancellationToken);
        return submission;
    }

    public async Task<FormSubmission> MarkAsNotSpamAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await GetSubmissionByIdAsync(submissionId, cancellationToken);
        if (submission == null)
            throw new InvalidOperationException($"Submission {submissionId} not found");

        submission.Status = SubmissionStatus.New;
        submission.IsSpam = false;
        await _context.SaveChangesAsync(cancellationToken);
        return submission;
    }

    public async Task<bool> DeleteSubmissionAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && !s.IsDeleted, cancellationToken);

        if (submission == null)
            return false;

        submission.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    #endregion

    #region Double Opt-In

    public async Task<bool> SendOptInConfirmationAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await GetSubmissionByIdAsync(submissionId, cancellationToken);
        if (submission?.FormDefinition?.DoubleOptIn != true)
            return false;

        if (submission.OptInConfirmed)
        {
            _logger.LogInformation("Submission {SubmissionId} already confirmed, skipping", submissionId);
            return true;
        }

        // Generate a unique confirmation token
        var confirmationToken = Guid.NewGuid().ToString("N");

        // Store the token in RawData as JSON metadata so ConfirmOptInAsync can look it up
        var metadata = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(submission.RawData))
        {
            try
            {
                var existing = JsonSerializer.Deserialize<Dictionary<string, object>>(submission.RawData);
                if (existing != null)
                    metadata = existing;
            }
            catch (JsonException)
            {
                // RawData wasn't valid JSON; preserve original under a key
                metadata["_originalRawData"] = submission.RawData;
            }
        }

        metadata["_confirmationToken"] = confirmationToken;
        metadata["_confirmationTokenCreatedAt"] = DateTime.UtcNow.ToString("O");
        submission.RawData = JsonSerializer.Serialize(metadata);

        await _context.SaveChangesAsync(cancellationToken);

        // Build the confirmation URL that an integration or notification service can use
        var confirmationUrl = $"/api/forms/confirm-optin?token={confirmationToken}";

        _logger.LogInformation(
            "Opt-in confirmation token generated for submission {SubmissionId}. " +
            "Token: {Token}, ConfirmationUrl: {Url}",
            submissionId, confirmationToken, confirmationUrl);

        return true;
    }

    public async Task<FormSubmission?> ConfirmOptInAsync(string confirmationToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(confirmationToken))
        {
            _logger.LogWarning("ConfirmOptInAsync called with empty token");
            return null;
        }

        // Search for the submission whose RawData JSON contains this confirmation token
        var tokenFragment = $"\"_confirmationToken\":\"{confirmationToken}\"";
        var submission = await _context.FormSubmissions
            .Include(s => s.FormDefinition)
            .FirstOrDefaultAsync(
                s => !s.IsDeleted
                     && !s.OptInConfirmed
                     && s.RawData != null
                     && s.RawData.Contains(confirmationToken),
                cancellationToken);

        if (submission == null)
        {
            _logger.LogWarning("No pending submission found for confirmation token {Token}", confirmationToken);
            return null;
        }

        // Double-check the token matches exactly by deserializing RawData
        try
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(submission.RawData!);
            if (metadata == null
                || !metadata.TryGetValue("_confirmationToken", out var storedToken)
                || storedToken.GetString() != confirmationToken)
            {
                _logger.LogWarning(
                    "Token mismatch during opt-in confirmation for submission {SubmissionId}",
                    submission.Id);
                return null;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse RawData for submission {SubmissionId}", submission.Id);
            return null;
        }

        // Mark the submission as confirmed
        submission.OptInConfirmed = true;
        submission.OptInConfirmedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Opt-in confirmed for submission {SubmissionId} (form {FormId})",
            submission.Id, submission.FormDefinitionId);

        return submission;
    }

    #endregion

    #region Validation

    public async Task<FormValidationResult> ValidateFormDataAsync(
        int formId,
        Dictionary<string, object> formData,
        CancellationToken cancellationToken = default)
    {
        var fields = await GetFormFieldsAsync(formId, cancellationToken);
        var result = new FormValidationResult { IsValid = true };

        foreach (var field in fields)
        {
            var value = formData.TryGetValue(field.FieldName, out var v) ? v : null;
            var fieldValidation = await ValidateFieldValueAsync(field, value);

            if (!fieldValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.Add(new FieldValidationError
                {
                    FieldName = field.FieldName,
                    Message = fieldValidation.ErrorMessage ?? "Invalid value",
                    ErrorCode = fieldValidation.ErrorCode
                });
            }
        }

        return result;
    }

    public async Task<FieldValidationResult> ValidateFieldAsync(
        int fieldId,
        object? value,
        CancellationToken cancellationToken = default)
    {
        var field = await GetFieldByIdAsync(fieldId, cancellationToken);
        if (field == null)
            return new FieldValidationResult { IsValid = false, ErrorMessage = "Field not found" };

        return await ValidateFieldValueAsync(field, value);
    }

    private Task<FieldValidationResult> ValidateFieldValueAsync(FormField field, object? value)
    {
        var stringValue = value?.ToString();
        var isEmpty = string.IsNullOrWhiteSpace(stringValue);

        // Required validation
        if (field.IsRequired && isEmpty)
        {
            return Task.FromResult(new FieldValidationResult
            {
                IsValid = false,
                ErrorMessage = field.RequiredMessage ?? $"{field.Label} is required",
                ErrorCode = "REQUIRED"
            });
        }

        if (isEmpty)
            return Task.FromResult(new FieldValidationResult { IsValid = true });

        // Length validation
        if (field.MinLength.HasValue && stringValue!.Length < field.MinLength.Value)
        {
            return Task.FromResult(new FieldValidationResult
            {
                IsValid = false,
                ErrorMessage = $"{field.Label} must be at least {field.MinLength} characters",
                ErrorCode = "MIN_LENGTH"
            });
        }

        if (field.MaxLength.HasValue && stringValue!.Length > field.MaxLength.Value)
        {
            return Task.FromResult(new FieldValidationResult
            {
                IsValid = false,
                ErrorMessage = $"{field.Label} must not exceed {field.MaxLength} characters",
                ErrorCode = "MAX_LENGTH"
            });
        }

        // Pattern validation
        if (!string.IsNullOrEmpty(field.ValidationPattern))
        {
            var regex = new Regex(field.ValidationPattern);
            if (!regex.IsMatch(stringValue!))
            {
                return Task.FromResult(new FieldValidationResult
                {
                    IsValid = false,
                    ErrorMessage = field.ValidationMessage ?? $"{field.Label} format is invalid",
                    ErrorCode = "PATTERN"
                });
            }
        }

        // Type-specific validation
        switch (field.FieldType)
        {
            case FormFieldType.Email:
                if (!IsValidEmail(stringValue!))
                {
                    return Task.FromResult(new FieldValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Invalid email format",
                        ErrorCode = "INVALID_EMAIL"
                    });
                }
                break;

            case FormFieldType.Number:
            case FormFieldType.Range:
                if (!decimal.TryParse(stringValue, out var numValue))
                {
                    return Task.FromResult(new FieldValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Must be a valid number",
                        ErrorCode = "INVALID_NUMBER"
                    });
                }

                if (field.MinValue.HasValue && numValue < field.MinValue.Value)
                {
                    return Task.FromResult(new FieldValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Value must be at least {field.MinValue}",
                        ErrorCode = "MIN_VALUE"
                    });
                }

                if (field.MaxValue.HasValue && numValue > field.MaxValue.Value)
                {
                    return Task.FromResult(new FieldValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Value must not exceed {field.MaxValue}",
                        ErrorCode = "MAX_VALUE"
                    });
                }
                break;

            case FormFieldType.Url:
                if (!Uri.TryCreate(stringValue, UriKind.Absolute, out _))
                {
                    return Task.FromResult(new FieldValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Invalid URL format",
                        ErrorCode = "INVALID_URL"
                    });
                }
                break;
        }

        return Task.FromResult(new FieldValidationResult { IsValid = true });
    }

    #endregion

    #region Spam Protection

    public async Task<int> CalculateSpamScoreAsync(
        int formId,
        Dictionary<string, object> formData,
        FormSubmissionContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var form = await GetFormByIdAsync(formId, cancellationToken);
        if (form == null)
            return 0;

        int score = 0;

        // Check honeypot
        if (!string.IsNullOrEmpty(form.HoneypotFieldName) &&
            !string.IsNullOrEmpty(context?.HoneypotValue))
        {
            score += 50;
        }

        // Check submission speed (too fast = suspicious)
        if (context?.SubmissionDuration.HasValue == true &&
            context.SubmissionDuration.Value.TotalSeconds < 3)
        {
            score += 30;
        }

        // Check for spam keywords in text fields
        var spamKeywords = new[] { "viagra", "casino", "lottery", "prize", "winner" };
        foreach (var (key, value) in formData)
        {
            var strValue = value?.ToString()?.ToLower();
            if (!string.IsNullOrEmpty(strValue))
            {
                if (spamKeywords.Any(k => strValue.Contains(k)))
                {
                    score += 20;
                }

                // Check for excessive links
                var urlCount = Regex.Matches(strValue, @"https?://").Count;
                if (urlCount > 3)
                {
                    score += 10 * urlCount;
                }
            }
        }

        return Math.Min(score, 100);
    }

    public async Task<bool> IsSpamAsync(
        int formId,
        Dictionary<string, object> formData,
        FormSubmissionContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var score = await CalculateSpamScoreAsync(formId, formData, context, cancellationToken);
        return score >= 50;
    }

    #endregion

    #region Embedding & URLs

    public Task<string> GenerateEmbedCodeAsync(int formId, string? baseUrl = null, CancellationToken cancellationToken = default)
    {
        var url = $"{baseUrl ?? "https://yoursite.com"}/forms/embed/{formId}";
        var embedCode = $@"<iframe src=""{url}"" width=""100%"" height=""500"" frameborder=""0""></iframe>";
        return Task.FromResult(embedCode);
    }

    public Task<string> GenerateDirectUrlAsync(int formId, string? baseUrl = null, CancellationToken cancellationToken = default)
    {
        var url = $"{baseUrl ?? "https://yoursite.com"}/forms/{formId}";
        return Task.FromResult(url);
    }

    public async Task<bool> IsValidEmbeddingDomainAsync(int formId, string domain, CancellationToken cancellationToken = default)
    {
        var form = await GetFormByIdAsync(formId, cancellationToken);
        if (form == null)
            return false;

        if (string.IsNullOrEmpty(form.AllowedDomains))
            return true; // No restrictions

        var allowed = form.AllowedDomains.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return allowed.Any(d => d.Trim().Equals(domain, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Statistics & Analytics

    public async Task IncrementViewCountAsync(int formId, CancellationToken cancellationToken = default)
    {
        var form = await _context.FormDefinitions
            .FirstOrDefaultAsync(f => f.Id == formId && !f.IsDeleted, cancellationToken);

        if (form != null)
        {
            form.TotalViews++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<FormStatistics> GetFormStatisticsAsync(
        int formId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var form = await GetFormByIdAsync(formId, cancellationToken);
        if (form == null)
            throw new InvalidOperationException($"Form {formId} not found");

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var submissions = await _context.FormSubmissions
            .Where(s => s.FormDefinitionId == formId &&
                       s.SubmittedAt >= from &&
                       s.SubmittedAt <= to &&
                       !s.IsDeleted)
            .ToListAsync(cancellationToken);

        return new FormStatistics
        {
            FormId = form.Id,
            FormName = form.Name,
            TotalViews = form.TotalViews,
            TotalSubmissions = submissions.Count,
            SuccessfulSubmissions = submissions.Count(s => s.Status == SubmissionStatus.LeadCreated || s.Status == SubmissionStatus.ContactCreated),
            FailedSubmissions = submissions.Count(s => s.Status == SubmissionStatus.Failed),
            SpamSubmissions = submissions.Count(s => s.Status == SubmissionStatus.Spam),
            ConversionRate = form.TotalViews > 0 ? (decimal)submissions.Count / form.TotalViews * 100 : 0,
            LeadsCreated = submissions.Count(s => s.LeadId.HasValue),
            ContactsCreated = submissions.Count(s => s.ContactId.HasValue),
            LastSubmissionAt = submissions.OrderByDescending(s => s.SubmittedAt).FirstOrDefault()?.SubmittedAt,
            FromDate = from,
            ToDate = to
        };
    }

    public async Task<FormSubmissionStatistics> GetSubmissionStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var forms = await _context.FormDefinitions.Where(f => !f.IsDeleted).ToListAsync(cancellationToken);
        var submissions = await _context.FormSubmissions
            .Where(s => s.SubmittedAt >= from && s.SubmittedAt <= to && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var byDay = submissions
            .GroupBy(s => s.SubmittedAt.Date)
            .Select(g => new FormSubmissionByDay { Date = g.Key, Count = g.Count() })
            .OrderBy(d => d.Date)
            .ToList();

        var topForms = forms
            .Select(f => new TopFormSummary
            {
                FormId = f.Id,
                FormName = f.Name,
                Submissions = submissions.Count(s => s.FormDefinitionId == f.Id),
                ConversionRate = f.TotalViews > 0 ? (decimal)submissions.Count(s => s.FormDefinitionId == f.Id) / f.TotalViews * 100 : 0
            })
            .OrderByDescending(f => f.Submissions)
            .Take(10)
            .ToList();

        return new FormSubmissionStatistics
        {
            TotalForms = forms.Count,
            ActiveForms = forms.Count(f => f.Status == FormStatus.Published),
            TotalSubmissions = submissions.Count,
            NewSubmissions = submissions.Count(s => s.Status == SubmissionStatus.New),
            ProcessedSubmissions = submissions.Count(s => s.Status == SubmissionStatus.LeadCreated || s.Status == SubmissionStatus.ContactCreated),
            FailedSubmissions = submissions.Count(s => s.Status == SubmissionStatus.Failed),
            SpamSubmissions = submissions.Count(s => s.Status == SubmissionStatus.Spam),
            SubmissionsByDay = byDay,
            TopForms = topForms
        };
    }

    public async Task<IEnumerable<FormFieldStatistics>> GetFieldStatisticsAsync(
        int formId,
        CancellationToken cancellationToken = default)
    {
        var fields = await GetFormFieldsAsync(formId, cancellationToken);
        var submissions = await _context.FormSubmissions
            .Where(s => s.FormDefinitionId == formId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var stats = new List<FormFieldStatistics>();

        foreach (var field in fields)
        {
            var filledCount = 0;
            foreach (var submission in submissions)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(submission.FormData) ?? new();
                if (data.TryGetValue(field.FieldName, out var value) && !string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    filledCount++;
                }
            }

            stats.Add(new FormFieldStatistics
            {
                FieldId = field.Id,
                FieldName = field.FieldName,
                FieldLabel = field.Label,
                FieldType = field.FieldType,
                TotalSubmissions = submissions.Count,
                FilledCount = filledCount,
                EmptyCount = submissions.Count - filledCount,
                CompletionRate = submissions.Count > 0 ? (decimal)filledCount / submissions.Count * 100 : 0
            });
        }

        return stats;
    }

    #endregion

    #region Templates

    public Task<IEnumerable<FormTemplate>> GetFormTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = new List<FormTemplate>
        {
            new FormTemplate
            {
                Key = "contact-us",
                Name = "Contact Us",
                Description = "Simple contact form with name, email, and message",
                Category = "General",
                Fields = new List<FormFieldTemplate>
                {
                    new() { FieldName = "firstName", Label = "First Name", FieldType = FormFieldType.Text, Order = 1, IsRequired = true, CrmFieldMapping = "Lead.FirstName" },
                    new() { FieldName = "lastName", Label = "Last Name", FieldType = FormFieldType.Text, Order = 2, IsRequired = true, CrmFieldMapping = "Lead.LastName" },
                    new() { FieldName = "email", Label = "Email", FieldType = FormFieldType.Email, Order = 3, IsRequired = true, CrmFieldMapping = "Lead.Email" },
                    new() { FieldName = "message", Label = "Message", FieldType = FormFieldType.TextArea, Order = 4, IsRequired = true }
                }
            },
            new FormTemplate
            {
                Key = "newsletter",
                Name = "Newsletter Signup",
                Description = "Simple newsletter subscription form",
                Category = "Marketing",
                Fields = new List<FormFieldTemplate>
                {
                    new() { FieldName = "email", Label = "Email Address", FieldType = FormFieldType.Email, Order = 1, IsRequired = true, CrmFieldMapping = "Lead.Email" },
                    new() { FieldName = "consent", Label = "I agree to receive marketing emails", FieldType = FormFieldType.Consent, Order = 2, IsRequired = true }
                }
            },
            new FormTemplate
            {
                Key = "demo-request",
                Name = "Request a Demo",
                Description = "Demo request form for sales team",
                Category = "Sales",
                Fields = new List<FormFieldTemplate>
                {
                    new() { FieldName = "firstName", Label = "First Name", FieldType = FormFieldType.Text, Order = 1, IsRequired = true },
                    new() { FieldName = "lastName", Label = "Last Name", FieldType = FormFieldType.Text, Order = 2, IsRequired = true },
                    new() { FieldName = "email", Label = "Work Email", FieldType = FormFieldType.Email, Order = 3, IsRequired = true },
                    new() { FieldName = "company", Label = "Company", FieldType = FormFieldType.Text, Order = 4, IsRequired = true },
                    new() { FieldName = "phone", Label = "Phone", FieldType = FormFieldType.Phone, Order = 5 },
                    new() { FieldName = "companySize", Label = "Company Size", FieldType = FormFieldType.Dropdown, Order = 6, Options = "[\"1-10\",\"11-50\",\"51-200\",\"201-500\",\"500+\"]" }
                }
            }
        };

        return Task.FromResult<IEnumerable<FormTemplate>>(templates);
    }

    public async Task<FormDefinition> CreateFromTemplateAsync(
        string templateKey,
        string formName,
        int? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        var templates = await GetFormTemplatesAsync(cancellationToken);
        var template = templates.FirstOrDefault(t => t.Key == templateKey);

        if (template == null)
            throw new InvalidOperationException($"Template {templateKey} not found");

        var form = new FormDefinition
        {
            Name = formName,
            FormKey = GenerateFormKey(formName),
            Description = template.Description,
            OwnerId = ownerId,
            Status = FormStatus.Draft,
            SubmitButtonText = "Submit",
            ThankYouMessage = "Thank you for your submission!"
        };

        form = await CreateFormAsync(form, cancellationToken);

        foreach (var fieldTemplate in template.Fields)
        {
            var field = new FormField
            {
                FormDefinitionId = form.Id,
                FieldName = fieldTemplate.FieldName,
                Label = fieldTemplate.Label,
                FieldType = fieldTemplate.FieldType,
                Order = fieldTemplate.Order,
                IsRequired = fieldTemplate.IsRequired,
                Placeholder = fieldTemplate.Placeholder,
                CrmFieldMapping = fieldTemplate.CrmFieldMapping,
                Options = fieldTemplate.Options
            };
            await AddFieldAsync(form.Id, field, cancellationToken);
        }

        return await GetFormByIdAsync(form.Id, cancellationToken) ?? form;
    }

    #endregion

    #region Private Methods

    private string GenerateFormKey(string name)
    {
        var key = Regex.Replace(name.ToLower(), @"[^a-z0-9]", "-");
        key = Regex.Replace(key, @"-+", "-").Trim('-');
        return $"{key}-{DateTime.UtcNow.Ticks % 10000}";
    }

    private async Task<FormSubmission> CreateSubmissionRecord(
        FormDefinition form,
        Dictionary<string, object> formData,
        FormSubmissionContext? context,
        SubmissionStatus status,
        CancellationToken cancellationToken)
    {
        var submission = new FormSubmission
        {
            FormDefinitionId = form.Id,
            SubmissionNumber = $"SUB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            FormData = JsonSerializer.Serialize(formData),
            RawData = JsonSerializer.Serialize(formData),
            Status = status,
            SubmittedAt = DateTime.UtcNow,
            IpAddress = context?.IpAddress,
            UserAgent = context?.UserAgent,
            Referrer = context?.Referrer,
            PageUrl = context?.PageUrl,
            UtmSource = context?.UtmSource,
            UtmMedium = context?.UtmMedium,
            UtmCampaign = context?.UtmCampaign,
            UtmContent = context?.UtmContent,
            UtmTerm = context?.UtmTerm,
            WebVisitorId = context?.WebVisitorId,
            CreatedAt = DateTime.UtcNow
        };

        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);
        return submission;
    }

    private async Task<int?> CreateOrUpdateLeadAsync(
        FormDefinition form,
        Dictionary<string, object> formData,
        CancellationToken cancellationToken)
    {
        // Map form fields to lead properties
        var lead = new Lead
        {
            FirstName = GetFormValue(formData, "firstName") ?? GetFormValue(formData, "first_name") ?? "Unknown",
            LastName = GetFormValue(formData, "lastName") ?? GetFormValue(formData, "last_name") ?? "Unknown",
            Email = GetFormValue(formData, "email") ?? "",
            Phone = GetFormValue(formData, "phone"),
            CompanyName = GetFormValue(formData, "company") ?? GetFormValue(formData, "companyName"),
            Title = GetFormValue(formData, "title") ?? GetFormValue(formData, "jobTitle"),
            Source = ParseLeadSource(form.LeadSource),
            Status = LeadLifecycleStatus.New,
            OwnerId = form.DefaultLeadOwnerId,
            CampaignId = form.CampaignId,
            CreatedAt = DateTime.UtcNow
        };

        // Check for existing lead
        if (form.UpdateExistingLead && !string.IsNullOrEmpty(lead.Email))
        {
            var existingLead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Email == lead.Email && !l.IsDeleted, cancellationToken);

            if (existingLead != null)
            {
                existingLead.Phone = lead.Phone ?? existingLead.Phone;
                existingLead.CompanyName = lead.CompanyName ?? existingLead.CompanyName;
                existingLead.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return existingLead.Id;
            }
        }

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);

        // Route lead if routing service is available
        if (_leadRoutingService != null && form.LeadRoutingRuleId.HasValue)
        {
            await _leadRoutingService.RouteLeadAsync(lead.Id, cancellationToken);
        }

        return lead.Id;
    }

    private string? GetFormValue(Dictionary<string, object> formData, string key)
    {
        if (formData.TryGetValue(key, out var value))
            return value?.ToString();
        return null;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static LeadSource ParseLeadSource(string? source)
    {
        if (string.IsNullOrEmpty(source))
            return LeadSource.Web;

        return source.ToLowerInvariant() switch
        {
            "website" or "web" or "web form" or "webform" => LeadSource.Web,
            "referral" => LeadSource.Referral,
            "partner" => LeadSource.Partner,
            "campaign" or "email" or "email campaign" => LeadSource.Campaign,
            "event" or "trade show" or "tradeshow" or "webinar" or "conference" => LeadSource.Event,
            "manual" or "import" or "cold call" => LeadSource.Manual,
            _ => LeadSource.Manual
        };
    }

    #endregion
}
