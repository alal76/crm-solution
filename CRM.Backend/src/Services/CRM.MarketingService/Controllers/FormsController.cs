// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the Source-Available License (see LICENSE) as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.MarketingService.Controllers;

/// <summary>
/// API controller for managing form definitions, fields, and submissions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<FormsController> _logger;

    public FormsController(CrmDbContext context, ILogger<FormsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #region Form Definitions CRUD

    /// <summary>
    /// Get all form definitions with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.FormDefinitions
                .Where(f => !f.IsDeleted);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<FormStatus>(status, true, out var statusEnum))
            {
                query = query.Where(f => f.Status == statusEnum);
            }

            var totalCount = await query.CountAsync();
            var forms = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.FormKey,
                    f.Description,
                    Status = f.Status.ToString(),
                    f.Title,
                    f.Subtitle,
                    f.SubmitButtonText,
                    SubmitAction = f.SubmitAction.ToString(),
                    f.ThankYouMessage,
                    f.RedirectUrl,
                    f.SpamProtection,
                    f.CreateLead,
                    f.LeadSource,
                    f.TotalViews,
                    f.TotalSubmissions,
                    f.ConversionRate,
                    f.OwnerId,
                    f.CampaignId,
                    FieldCount = f.Fields.Count,
                    SubmissionCount = f.Submissions.Count,
                    f.CreatedAt,
                    f.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = forms,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving form definitions");
            return StatusCode(500, "An error occurred while retrieving forms");
        }
    }

    /// <summary>
    /// Get form definition by ID with all fields
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var form = await _context.FormDefinitions
                .Include(f => f.Fields.OrderBy(fld => fld.Order))
                .Where(f => f.Id == id && !f.IsDeleted)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.FormKey,
                    f.Description,
                    Status = (int)f.Status,
                    StatusName = f.Status.ToString(),
                    f.Title,
                    f.Subtitle,
                    f.SubmitButtonText,
                    f.Width,
                    f.CssClasses,
                    f.CustomCss,
                    f.CustomJs,
                    f.Theme,
                    SubmitAction = (int)f.SubmitAction,
                    SubmitActionName = f.SubmitAction.ToString(),
                    f.ThankYouMessage,
                    f.RedirectUrl,
                    f.DoubleOptIn,
                    f.DoubleOptInTemplateId,
                    f.SpamProtection,
                    f.CaptchaType,
                    f.HoneypotFieldName,
                    f.CreateLead,
                    f.LeadSource,
                    f.DefaultLeadOwnerId,
                    f.LeadRoutingRuleId,
                    f.UpdateExistingLead,
                    f.ExistingLeadMatchField,
                    f.CampaignId,
                    f.CampaignMemberStatus,
                    f.NotifyOwner,
                    f.NotificationRecipients,
                    f.NotificationTemplateId,
                    f.SendAutoresponder,
                    f.AutoresponderTemplateId,
                    f.EmbedCode,
                    f.DirectUrl,
                    f.AllowedDomains,
                    f.TotalViews,
                    f.TotalSubmissions,
                    f.ConversionRate,
                    f.OwnerId,
                    f.CreatedAt,
                    f.UpdatedAt,
                    Fields = f.Fields.OrderBy(fld => fld.Order).Select(fld => new
                    {
                        fld.Id,
                        fld.FieldName,
                        fld.Label,
                        FieldType = (int)fld.FieldType,
                        FieldTypeName = fld.FieldType.ToString(),
                        fld.Order,
                        fld.IsRequired,
                        fld.RequiredMessage,
                        fld.MinLength,
                        fld.MaxLength,
                        fld.MinValue,
                        fld.MaxValue,
                        fld.ValidationPattern,
                        fld.ValidationMessage,
                        fld.Placeholder,
                        fld.HelpText,
                        fld.DefaultValue,
                        fld.Width,
                        fld.CssClasses,
                        fld.IsHidden,
                        fld.IsReadOnly,
                        fld.Options,
                        fld.OptionValueField,
                        fld.OptionLabelField,
                        fld.AllowOther,
                        fld.CrmFieldMapping,
                        fld.CrmEntityMapping,
                        fld.HasConditionalLogic,
                        fld.ConditionalLogic
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (form == null)
                return NotFound(new { message = $"Form with ID {id} not found" });

            return Ok(form);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving form {FormId}", id);
            return StatusCode(500, "An error occurred while retrieving the form");
        }
    }

    /// <summary>
    /// Create a new form definition
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormRequest request)
    {
        try
        {
            var form = new FormDefinition
            {
                Name = request.Name,
                FormKey = request.FormKey ?? GenerateFormKey(request.Name),
                Description = request.Description,
                Status = Enum.TryParse<FormStatus>(request.Status, true, out var status) ? status : FormStatus.Draft,
                Title = request.Title,
                Subtitle = request.Subtitle,
                SubmitButtonText = request.SubmitButtonText ?? "Submit",
                Width = request.Width,
                CssClasses = request.CssClasses,
                CustomCss = request.CustomCss,
                CustomJs = request.CustomJs,
                Theme = request.Theme,
                SubmitAction = Enum.TryParse<FormSubmitAction>(request.SubmitAction, true, out var submitAction) ? submitAction : FormSubmitAction.ShowMessage,
                ThankYouMessage = request.ThankYouMessage,
                RedirectUrl = request.RedirectUrl,
                DoubleOptIn = request.DoubleOptIn,
                DoubleOptInTemplateId = request.DoubleOptInTemplateId,
                SpamProtection = request.SpamProtection,
                CaptchaType = request.CaptchaType,
                HoneypotFieldName = request.HoneypotFieldName,
                CreateLead = request.CreateLead,
                LeadSource = request.LeadSource,
                DefaultLeadOwnerId = request.DefaultLeadOwnerId,
                LeadRoutingRuleId = request.LeadRoutingRuleId,
                UpdateExistingLead = request.UpdateExistingLead,
                ExistingLeadMatchField = request.ExistingLeadMatchField,
                CampaignId = request.CampaignId,
                CampaignMemberStatus = request.CampaignMemberStatus,
                NotifyOwner = request.NotifyOwner,
                NotificationRecipients = request.NotificationRecipients,
                NotificationTemplateId = request.NotificationTemplateId,
                SendAutoresponder = request.SendAutoresponder,
                AutoresponderTemplateId = request.AutoresponderTemplateId,
                AllowedDomains = request.AllowedDomains,
                OwnerId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Generate embed code and direct URL
            form.DirectUrl = $"/forms/{form.FormKey}";
            form.EmbedCode = $"<iframe src=\"/forms/{form.FormKey}\" width=\"100%\" height=\"500\" frameborder=\"0\"></iframe>";

            _context.FormDefinitions.Add(form);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created form {FormId} - {FormName}", form.Id, form.Name);
            return CreatedAtAction(nameof(GetById), new { id = form.Id }, new { form.Id, form.Name, form.FormKey });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form");
            return StatusCode(500, "An error occurred while creating the form");
        }
    }

    /// <summary>
    /// Update a form definition
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFormRequest request)
    {
        try
        {
            var form = await _context.FormDefinitions
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (form == null)
                return NotFound(new { message = $"Form with ID {id} not found" });

            // Update basic fields
            if (request.Name != null) form.Name = request.Name;
            if (request.Description != null) form.Description = request.Description;
            if (request.Status != null && Enum.TryParse<FormStatus>(request.Status, true, out var status))
                form.Status = status;
            if (request.Title != null) form.Title = request.Title;
            if (request.Subtitle != null) form.Subtitle = request.Subtitle;
            if (request.SubmitButtonText != null) form.SubmitButtonText = request.SubmitButtonText;
            if (request.Width != null) form.Width = request.Width;
            if (request.CssClasses != null) form.CssClasses = request.CssClasses;
            if (request.CustomCss != null) form.CustomCss = request.CustomCss;
            if (request.CustomJs != null) form.CustomJs = request.CustomJs;
            if (request.Theme != null) form.Theme = request.Theme;
            if (request.SubmitAction != null && Enum.TryParse<FormSubmitAction>(request.SubmitAction, true, out var submitAction))
                form.SubmitAction = submitAction;
            if (request.ThankYouMessage != null) form.ThankYouMessage = request.ThankYouMessage;
            if (request.RedirectUrl != null) form.RedirectUrl = request.RedirectUrl;
            form.DoubleOptIn = request.DoubleOptIn ?? form.DoubleOptIn;
            if (request.DoubleOptInTemplateId.HasValue) form.DoubleOptInTemplateId = request.DoubleOptInTemplateId;
            form.SpamProtection = request.SpamProtection ?? form.SpamProtection;
            if (request.CaptchaType != null) form.CaptchaType = request.CaptchaType;
            if (request.HoneypotFieldName != null) form.HoneypotFieldName = request.HoneypotFieldName;
            form.CreateLead = request.CreateLead ?? form.CreateLead;
            if (request.LeadSource != null) form.LeadSource = request.LeadSource;
            if (request.DefaultLeadOwnerId.HasValue) form.DefaultLeadOwnerId = request.DefaultLeadOwnerId;
            if (request.LeadRoutingRuleId.HasValue) form.LeadRoutingRuleId = request.LeadRoutingRuleId;
            form.UpdateExistingLead = request.UpdateExistingLead ?? form.UpdateExistingLead;
            if (request.ExistingLeadMatchField != null) form.ExistingLeadMatchField = request.ExistingLeadMatchField;
            if (request.CampaignId.HasValue) form.CampaignId = request.CampaignId;
            if (request.CampaignMemberStatus != null) form.CampaignMemberStatus = request.CampaignMemberStatus;
            form.NotifyOwner = request.NotifyOwner ?? form.NotifyOwner;
            if (request.NotificationRecipients != null) form.NotificationRecipients = request.NotificationRecipients;
            if (request.NotificationTemplateId.HasValue) form.NotificationTemplateId = request.NotificationTemplateId;
            form.SendAutoresponder = request.SendAutoresponder ?? form.SendAutoresponder;
            if (request.AutoresponderTemplateId.HasValue) form.AutoresponderTemplateId = request.AutoresponderTemplateId;
            if (request.AllowedDomains != null) form.AllowedDomains = request.AllowedDomains;

            form.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated form {FormId}", id);
            return Ok(new { message = "Form updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating form {FormId}", id);
            return StatusCode(500, "An error occurred while updating the form");
        }
    }

    /// <summary>
    /// Delete a form definition (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var form = await _context.FormDefinitions
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (form == null)
                return NotFound(new { message = $"Form with ID {id} not found" });

            form.IsDeleted = true;
            form.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted form {FormId}", id);
            return Ok(new { message = "Form deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting form {FormId}", id);
            return StatusCode(500, "An error occurred while deleting the form");
        }
    }

    #endregion

    #region Form Fields

    /// <summary>
    /// Get fields for a form
    /// </summary>
    [HttpGet("{formId}/fields")]
    public async Task<IActionResult> GetFields(int formId)
    {
        try
        {
            var fields = await _context.Set<FormField>()
                .Where(f => f.FormDefinitionId == formId && !f.IsDeleted)
                .OrderBy(f => f.Order)
                .Select(f => new
                {
                    f.Id,
                    f.FieldName,
                    f.Label,
                    FieldType = (int)f.FieldType,
                    FieldTypeName = f.FieldType.ToString(),
                    f.Order,
                    f.IsRequired,
                    f.RequiredMessage,
                    f.MinLength,
                    f.MaxLength,
                    f.MinValue,
                    f.MaxValue,
                    f.ValidationPattern,
                    f.ValidationMessage,
                    f.Placeholder,
                    f.HelpText,
                    f.DefaultValue,
                    f.Width,
                    f.CssClasses,
                    f.IsHidden,
                    f.IsReadOnly,
                    f.Options,
                    f.OptionValueField,
                    f.OptionLabelField,
                    f.AllowOther,
                    f.CrmFieldMapping,
                    f.CrmEntityMapping,
                    f.HasConditionalLogic,
                    f.ConditionalLogic
                })
                .ToListAsync();

            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fields for form {FormId}", formId);
            return StatusCode(500, "An error occurred while retrieving form fields");
        }
    }

    /// <summary>
    /// Add a field to a form
    /// </summary>
    [HttpPost("{formId}/fields")]
    public async Task<IActionResult> AddField(int formId, [FromBody] CreateFieldRequest request)
    {
        try
        {
            var form = await _context.FormDefinitions
                .FirstOrDefaultAsync(f => f.Id == formId && !f.IsDeleted);

            if (form == null)
                return NotFound(new { message = $"Form with ID {formId} not found" });

            // Get next order
            var maxOrder = await _context.Set<FormField>()
                .Where(f => f.FormDefinitionId == formId && !f.IsDeleted)
                .MaxAsync(f => (int?)f.Order) ?? -1;

            var field = new FormField
            {
                FormDefinitionId = formId,
                FieldName = request.FieldName,
                Label = request.Label,
                FieldType = Enum.TryParse<FormFieldType>(request.FieldType, true, out var fieldType) ? fieldType : FormFieldType.Text,
                Order = request.Order ?? maxOrder + 1,
                IsRequired = request.IsRequired,
                RequiredMessage = request.RequiredMessage,
                MinLength = request.MinLength,
                MaxLength = request.MaxLength,
                MinValue = request.MinValue,
                MaxValue = request.MaxValue,
                ValidationPattern = request.ValidationPattern,
                ValidationMessage = request.ValidationMessage,
                Placeholder = request.Placeholder,
                HelpText = request.HelpText,
                DefaultValue = request.DefaultValue,
                Width = request.Width ?? "full",
                CssClasses = request.CssClasses,
                IsHidden = request.IsHidden,
                IsReadOnly = request.IsReadOnly,
                Options = request.Options,
                OptionValueField = request.OptionValueField,
                OptionLabelField = request.OptionLabelField,
                AllowOther = request.AllowOther,
                CrmFieldMapping = request.CrmFieldMapping,
                CrmEntityMapping = request.CrmEntityMapping,
                HasConditionalLogic = request.HasConditionalLogic,
                ConditionalLogic = request.ConditionalLogic,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<FormField>().Add(field);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added field {FieldId} to form {FormId}", field.Id, formId);
            return CreatedAtAction(nameof(GetFields), new { formId }, new { field.Id, field.FieldName, field.Label });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding field to form {FormId}", formId);
            return StatusCode(500, "An error occurred while adding the field");
        }
    }

    /// <summary>
    /// Update a form field
    /// </summary>
    [HttpPut("{formId}/fields/{fieldId}")]
    public async Task<IActionResult> UpdateField(int formId, int fieldId, [FromBody] UpdateFieldRequest request)
    {
        try
        {
            var field = await _context.Set<FormField>()
                .FirstOrDefaultAsync(f => f.Id == fieldId && f.FormDefinitionId == formId && !f.IsDeleted);

            if (field == null)
                return NotFound(new { message = $"Field with ID {fieldId} not found in form {formId}" });

            if (request.FieldName != null) field.FieldName = request.FieldName;
            if (request.Label != null) field.Label = request.Label;
            if (request.FieldType != null && Enum.TryParse<FormFieldType>(request.FieldType, true, out var fieldType))
                field.FieldType = fieldType;
            if (request.Order.HasValue) field.Order = request.Order.Value;
            field.IsRequired = request.IsRequired ?? field.IsRequired;
            if (request.RequiredMessage != null) field.RequiredMessage = request.RequiredMessage;
            if (request.MinLength.HasValue) field.MinLength = request.MinLength;
            if (request.MaxLength.HasValue) field.MaxLength = request.MaxLength;
            if (request.MinValue.HasValue) field.MinValue = request.MinValue;
            if (request.MaxValue.HasValue) field.MaxValue = request.MaxValue;
            if (request.ValidationPattern != null) field.ValidationPattern = request.ValidationPattern;
            if (request.ValidationMessage != null) field.ValidationMessage = request.ValidationMessage;
            if (request.Placeholder != null) field.Placeholder = request.Placeholder;
            if (request.HelpText != null) field.HelpText = request.HelpText;
            if (request.DefaultValue != null) field.DefaultValue = request.DefaultValue;
            if (request.Width != null) field.Width = request.Width;
            if (request.CssClasses != null) field.CssClasses = request.CssClasses;
            field.IsHidden = request.IsHidden ?? field.IsHidden;
            field.IsReadOnly = request.IsReadOnly ?? field.IsReadOnly;
            if (request.Options != null) field.Options = request.Options;
            if (request.OptionValueField != null) field.OptionValueField = request.OptionValueField;
            if (request.OptionLabelField != null) field.OptionLabelField = request.OptionLabelField;
            field.AllowOther = request.AllowOther ?? field.AllowOther;
            if (request.CrmFieldMapping != null) field.CrmFieldMapping = request.CrmFieldMapping;
            if (request.CrmEntityMapping != null) field.CrmEntityMapping = request.CrmEntityMapping;
            field.HasConditionalLogic = request.HasConditionalLogic ?? field.HasConditionalLogic;
            if (request.ConditionalLogic != null) field.ConditionalLogic = request.ConditionalLogic;

            field.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated field {FieldId} in form {FormId}", fieldId, formId);
            return Ok(new { message = "Field updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating field {FieldId} in form {FormId}", fieldId, formId);
            return StatusCode(500, "An error occurred while updating the field");
        }
    }

    /// <summary>
    /// Delete a form field
    /// </summary>
    [HttpDelete("{formId}/fields/{fieldId}")]
    public async Task<IActionResult> DeleteField(int formId, int fieldId)
    {
        try
        {
            var field = await _context.Set<FormField>()
                .FirstOrDefaultAsync(f => f.Id == fieldId && f.FormDefinitionId == formId && !f.IsDeleted);

            if (field == null)
                return NotFound(new { message = $"Field with ID {fieldId} not found in form {formId}" });

            field.IsDeleted = true;
            field.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted field {FieldId} from form {FormId}", fieldId, formId);
            return Ok(new { message = "Field deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting field {FieldId} from form {FormId}", fieldId, formId);
            return StatusCode(500, "An error occurred while deleting the field");
        }
    }

    /// <summary>
    /// Reorder form fields
    /// </summary>
    [HttpPut("{formId}/fields/reorder")]
    public async Task<IActionResult> ReorderFields(int formId, [FromBody] ReorderFieldsRequest request)
    {
        try
        {
            var fields = await _context.Set<FormField>()
                .Where(f => f.FormDefinitionId == formId && !f.IsDeleted && request.FieldIds.Contains(f.Id))
                .ToListAsync();

            for (int i = 0; i < request.FieldIds.Count; i++)
            {
                var field = fields.FirstOrDefault(f => f.Id == request.FieldIds[i]);
                if (field != null)
                {
                    field.Order = i;
                    field.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Fields reordered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering fields in form {FormId}", formId);
            return StatusCode(500, "An error occurred while reordering fields");
        }
    }

    #endregion

    #region Form Submissions

    /// <summary>
    /// Get submissions for a form
    /// </summary>
    [HttpGet("{formId}/submissions")]
    public async Task<IActionResult> GetSubmissions(int formId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.Set<FormSubmission>()
                .Where(s => s.FormDefinitionId == formId && !s.IsDeleted);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<SubmissionStatus>(status, true, out var statusEnum))
            {
                query = query.Where(s => s.Status == statusEnum);
            }

            var totalCount = await query.CountAsync();
            var submissions = await query
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.SubmissionNumber,
                    s.SubmittedAt,
                    Status = s.Status.ToString(),
                    s.FormData,
                    s.IpAddress,
                    s.PageUrl,
                    s.Referrer,
                    s.UtmSource,
                    s.UtmMedium,
                    s.UtmCampaign,
                    s.ProcessedAt,
                    s.LeadId,
                    s.ContactId,
                    s.IsSpam,
                    s.SpamScore,
                    s.ErrorMessage
                })
                .ToListAsync();

            return Ok(new
            {
                data = submissions,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving submissions for form {FormId}", formId);
            return StatusCode(500, "An error occurred while retrieving submissions");
        }
    }

    /// <summary>
    /// Get a specific submission
    /// </summary>
    [HttpGet("{formId}/submissions/{submissionId}")]
    public async Task<IActionResult> GetSubmission(int formId, int submissionId)
    {
        try
        {
            var submission = await _context.Set<FormSubmission>()
                .Where(s => s.Id == submissionId && s.FormDefinitionId == formId && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.SubmissionNumber,
                    s.SubmittedAt,
                    Status = s.Status.ToString(),
                    s.ErrorMessage,
                    s.FormData,
                    s.RawData,
                    s.IpAddress,
                    s.UserAgent,
                    s.Referrer,
                    s.PageUrl,
                    s.UtmSource,
                    s.UtmMedium,
                    s.UtmCampaign,
                    s.UtmContent,
                    s.UtmTerm,
                    s.ProcessedAt,
                    s.OptInConfirmed,
                    s.OptInConfirmedAt,
                    s.SpamScore,
                    s.IsSpam,
                    s.LeadId,
                    s.ContactId,
                    s.WebVisitorId
                })
                .FirstOrDefaultAsync();

            if (submission == null)
                return NotFound(new { message = $"Submission with ID {submissionId} not found" });

            return Ok(submission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving submission {SubmissionId}", submissionId);
            return StatusCode(500, "An error occurred while retrieving the submission");
        }
    }

    /// <summary>
    /// Submit a form (public endpoint for form submissions)
    /// </summary>
    [HttpPost("{formKey}/submit")]
    [AllowAnonymous] // NOSONAR - S4834: public form submission endpoint for marketing lead capture, anonymous access required
    public async Task<IActionResult> SubmitForm(string formKey, [FromBody] Dictionary<string, object> formData)
    {
        try
        {
            var form = await _context.FormDefinitions
                .FirstOrDefaultAsync(f => f.FormKey == formKey && f.Status == FormStatus.Published && !f.IsDeleted);

            if (form == null)
                return NotFound(new { message = "Form not found or not published" });

            // Check spam protection (honeypot)
            if (form.SpamProtection && !string.IsNullOrEmpty(form.HoneypotFieldName))
            {
                if (formData.ContainsKey(form.HoneypotFieldName) &&
                    formData[form.HoneypotFieldName]?.ToString() != string.Empty)
                {
                    _logger.LogWarning("Spam submission detected for form {FormKey}", formKey);
                    // Return success to not alert bots, but don't process
                    return Ok(new { message = "Submission received" });
                }
            }

            var submission = new FormSubmission
            {
                FormDefinitionId = form.Id,
                SubmissionNumber = $"SUB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.New,
                FormData = System.Text.Json.JsonSerializer.Serialize(formData),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Referrer = Request.Headers["Referer"].ToString(),
                UtmSource = formData.TryGetValue("utm_source", out var utmSource) ? utmSource?.ToString() : null,
                UtmMedium = formData.TryGetValue("utm_medium", out var utmMedium) ? utmMedium?.ToString() : null,
                UtmCampaign = formData.TryGetValue("utm_campaign", out var utmCampaign) ? utmCampaign?.ToString() : null,
                UtmContent = formData.TryGetValue("utm_content", out var utmContent) ? utmContent?.ToString() : null,
                UtmTerm = formData.TryGetValue("utm_term", out var utmTerm) ? utmTerm?.ToString() : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<FormSubmission>().Add(submission);

            // Update form statistics
            form.TotalSubmissions++;
            form.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Form submission {SubmissionId} received for form {FormKey}", submission.Id, formKey);

            // Return appropriate response based on form settings
            return Ok(new
            {
                success = true,
                message = form.ThankYouMessage ?? "Thank you for your submission!",
                submissionId = submission.SubmissionNumber,
                action = form.SubmitAction.ToString(),
                redirectUrl = form.SubmitAction == FormSubmitAction.Redirect ? form.RedirectUrl : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing form submission for {FormKey}", formKey);
            return StatusCode(500, "An error occurred while processing your submission");
        }
    }

    /// <summary>
    /// Update submission status
    /// </summary>
    [HttpPut("{formId}/submissions/{submissionId}/status")]
    public async Task<IActionResult> UpdateSubmissionStatus(int formId, int submissionId, [FromBody] UpdateSubmissionStatusRequest request)
    {
        try
        {
            var submission = await _context.Set<FormSubmission>()
                .FirstOrDefaultAsync(s => s.Id == submissionId && s.FormDefinitionId == formId && !s.IsDeleted);

            if (submission == null)
                return NotFound(new { message = $"Submission with ID {submissionId} not found" });

            if (Enum.TryParse<SubmissionStatus>(request.Status, true, out var status))
            {
                submission.Status = status;
                if (status == SubmissionStatus.Spam)
                    submission.IsSpam = true;
                submission.ProcessedAt = DateTime.UtcNow;
                submission.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Submission status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating submission status {SubmissionId}", submissionId);
            return StatusCode(500, "An error occurred while updating the submission status");
        }
    }

    /// <summary>
    /// Delete a submission
    /// </summary>
    [HttpDelete("{formId}/submissions/{submissionId}")]
    public async Task<IActionResult> DeleteSubmission(int formId, int submissionId)
    {
        try
        {
            var submission = await _context.Set<FormSubmission>()
                .FirstOrDefaultAsync(s => s.Id == submissionId && s.FormDefinitionId == formId && !s.IsDeleted);

            if (submission == null)
                return NotFound(new { message = $"Submission with ID {submissionId} not found" });

            submission.IsDeleted = true;
            submission.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Submission deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting submission {SubmissionId}", submissionId);
            return StatusCode(500, "An error occurred while deleting the submission");
        }
    }

    #endregion

    #region Helper Methods

    private string GenerateFormKey(string name)
    {
        var key = name.ToLower()
            .Replace(" ", "-")
            .Replace("_", "-");
        // Remove special characters
        key = System.Text.RegularExpressions.Regex.Replace(key, @"[^a-z0-9\-]", "", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1));
        return $"{key}-{DateTime.UtcNow:yyyyMMdd}";
    }

    #endregion
}

#region Request DTOs

public class CreateFormRequest
{
    public string Name { get; set; } = string.Empty;
    public string? FormKey { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? SubmitButtonText { get; set; }
    public string? Width { get; set; }
    public string? CssClasses { get; set; }
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
    public string? Theme { get; set; }
    public string? SubmitAction { get; set; }
    public string? ThankYouMessage { get; set; }
    public string? RedirectUrl { get; set; }
    public bool DoubleOptIn { get; set; }
    public int? DoubleOptInTemplateId { get; set; }
    public bool SpamProtection { get; set; } = true;
    public string? CaptchaType { get; set; }
    public string? HoneypotFieldName { get; set; }
    public bool CreateLead { get; set; } = true;
    public string? LeadSource { get; set; }
    public int? DefaultLeadOwnerId { get; set; }
    public int? LeadRoutingRuleId { get; set; }
    public bool UpdateExistingLead { get; set; } = true;
    public string? ExistingLeadMatchField { get; set; }
    public int? CampaignId { get; set; }
    public string? CampaignMemberStatus { get; set; }
    public bool NotifyOwner { get; set; } = true;
    public string? NotificationRecipients { get; set; }
    public int? NotificationTemplateId { get; set; }
    public bool SendAutoresponder { get; set; }
    public int? AutoresponderTemplateId { get; set; }
    public string? AllowedDomains { get; set; }
}

public class UpdateFormRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? SubmitButtonText { get; set; }
    public string? Width { get; set; }
    public string? CssClasses { get; set; }
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
    public string? Theme { get; set; }
    public string? SubmitAction { get; set; }
    public string? ThankYouMessage { get; set; }
    public string? RedirectUrl { get; set; }
    public bool? DoubleOptIn { get; set; }
    public int? DoubleOptInTemplateId { get; set; }
    public bool? SpamProtection { get; set; }
    public string? CaptchaType { get; set; }
    public string? HoneypotFieldName { get; set; }
    public bool? CreateLead { get; set; }
    public string? LeadSource { get; set; }
    public int? DefaultLeadOwnerId { get; set; }
    public int? LeadRoutingRuleId { get; set; }
    public bool? UpdateExistingLead { get; set; }
    public string? ExistingLeadMatchField { get; set; }
    public int? CampaignId { get; set; }
    public string? CampaignMemberStatus { get; set; }
    public bool? NotifyOwner { get; set; }
    public string? NotificationRecipients { get; set; }
    public int? NotificationTemplateId { get; set; }
    public bool? SendAutoresponder { get; set; }
    public int? AutoresponderTemplateId { get; set; }
    public string? AllowedDomains { get; set; }
}

public class CreateFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? FieldType { get; set; }
    public int? Order { get; set; }
    public bool IsRequired { get; set; }
    public string? RequiredMessage { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? Width { get; set; }
    public string? CssClasses { get; set; }
    public bool IsHidden { get; set; }
    public bool IsReadOnly { get; set; }
    public string? Options { get; set; }
    public string? OptionValueField { get; set; }
    public string? OptionLabelField { get; set; }
    public bool AllowOther { get; set; }
    public string? CrmFieldMapping { get; set; }
    public string? CrmEntityMapping { get; set; }
    public bool HasConditionalLogic { get; set; }
    public string? ConditionalLogic { get; set; }
}

public class UpdateFieldRequest
{
    public string? FieldName { get; set; }
    public string? Label { get; set; }
    public string? FieldType { get; set; }
    public int? Order { get; set; }
    public bool? IsRequired { get; set; }
    public string? RequiredMessage { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? Width { get; set; }
    public string? CssClasses { get; set; }
    public bool? IsHidden { get; set; }
    public bool? IsReadOnly { get; set; }
    public string? Options { get; set; }
    public string? OptionValueField { get; set; }
    public string? OptionLabelField { get; set; }
    public bool? AllowOther { get; set; }
    public string? CrmFieldMapping { get; set; }
    public string? CrmEntityMapping { get; set; }
    public bool? HasConditionalLogic { get; set; }
    public string? ConditionalLogic { get; set; }
}

public class ReorderFieldsRequest
{
    public List<int> FieldIds { get; set; } = new();
}

public class UpdateSubmissionStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

#endregion
