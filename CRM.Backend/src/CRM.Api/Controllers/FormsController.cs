// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for form builder operations - creating, managing, and processing web forms.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly IFormBuilderService _formService;
    private readonly ILogger<FormsController> _logger;

    public FormsController(IFormBuilderService formService, ILogger<FormsController> logger)
    {
        _formService = formService;
        _logger = logger;
    }

    #region Form Definition CRUD

    /// <summary>
    /// Get all form definitions with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormDefinition>>> GetAllForms(
        [FromQuery] FormStatus? status = null,
        [FromQuery] int? ownerId = null,
        [FromQuery] int? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var forms = await _formService.GetAllFormsAsync(status, ownerId, campaignId, cancellationToken);
        return Ok(forms);
    }

    /// <summary>
    /// Get a form definition by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FormDefinition>> GetFormById(int id, CancellationToken cancellationToken)
    {
        var form = await _formService.GetFormByIdAsync(id, cancellationToken);
        if (form == null)
        {
            return NotFound($"Form with ID {id} not found.");
        }
        return Ok(form);
    }

    /// <summary>
    /// Get a form by its form key.
    /// </summary>
    [HttpGet("by-key/{formKey}")]
    [AllowAnonymous]
    public async Task<ActionResult<FormDefinition>> GetFormByKey(string formKey, CancellationToken cancellationToken)
    {
        var form = await _formService.GetFormByKeyAsync(formKey, cancellationToken);
        if (form == null)
        {
            return NotFound($"Form with key '{formKey}' not found.");
        }
        return Ok(form);
    }

    /// <summary>
    /// Create a new form definition.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FormDefinition>> CreateForm(
        [FromBody] FormDefinition form,
        CancellationToken cancellationToken)
    {
        var created = await _formService.CreateFormAsync(form, cancellationToken);
        return CreatedAtAction(nameof(GetFormById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing form definition.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FormDefinition>> UpdateForm(
        int id,
        [FromBody] FormDefinition form,
        CancellationToken cancellationToken)
    {
        if (id != form.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _formService.UpdateFormAsync(form, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Delete a form definition (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteForm(int id, CancellationToken cancellationToken)
    {
        var result = await _formService.DeleteFormAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound($"Form with ID {id} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Clone an existing form.
    /// </summary>
    [HttpPost("{id:int}/clone")]
    public async Task<ActionResult<FormDefinition>> CloneForm(
        int id,
        [FromQuery] string newName,
        CancellationToken cancellationToken)
    {
        var cloned = await _formService.CloneFormAsync(id, newName, cancellationToken);
        return CreatedAtAction(nameof(GetFormById), new { id = cloned.Id }, cloned);
    }

    #endregion

    #region Form Status Management

    /// <summary>
    /// Publish a form (make it active for submissions).
    /// </summary>
    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<FormDefinition>> PublishForm(int id, CancellationToken cancellationToken)
    {
        var form = await _formService.PublishFormAsync(id, cancellationToken);
        return Ok(form);
    }

    /// <summary>
    /// Unpublish a form.
    /// </summary>
    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult<FormDefinition>> UnpublishForm(int id, CancellationToken cancellationToken)
    {
        var form = await _formService.UnpublishFormAsync(id, cancellationToken);
        return Ok(form);
    }

    /// <summary>
    /// Archive a form.
    /// </summary>
    [HttpPost("{id:int}/archive")]
    public async Task<ActionResult<FormDefinition>> ArchiveForm(int id, CancellationToken cancellationToken)
    {
        var form = await _formService.ArchiveFormAsync(id, cancellationToken);
        return Ok(form);
    }

    /// <summary>
    /// Update form status.
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<FormDefinition>> UpdateFormStatus(
        int id,
        [FromBody] FormStatusRequest request,
        CancellationToken cancellationToken)
    {
        var form = await _formService.UpdateFormStatusAsync(id, request.Status, cancellationToken);
        return Ok(form);
    }

    #endregion

    #region Form Field Management

    /// <summary>
    /// Get all fields for a form.
    /// </summary>
    [HttpGet("{formId:int}/fields")]
    public async Task<ActionResult<IEnumerable<FormField>>> GetFormFields(int formId, CancellationToken cancellationToken)
    {
        var fields = await _formService.GetFormFieldsAsync(formId, cancellationToken);
        return Ok(fields);
    }

    /// <summary>
    /// Get a specific field by ID.
    /// </summary>
    [HttpGet("fields/{fieldId:int}")]
    public async Task<ActionResult<FormField>> GetFieldById(int fieldId, CancellationToken cancellationToken)
    {
        var field = await _formService.GetFieldByIdAsync(fieldId, cancellationToken);
        if (field == null)
        {
            return NotFound($"Field with ID {fieldId} not found.");
        }
        return Ok(field);
    }

    /// <summary>
    /// Add a new field to a form.
    /// </summary>
    [HttpPost("{formId:int}/fields")]
    public async Task<ActionResult<FormField>> AddField(
        int formId,
        [FromBody] FormField field,
        CancellationToken cancellationToken)
    {
        var created = await _formService.AddFieldAsync(formId, field, cancellationToken);
        return CreatedAtAction(nameof(GetFieldById), new { fieldId = created.Id }, created);
    }

    /// <summary>
    /// Update an existing field.
    /// </summary>
    [HttpPut("fields/{fieldId:int}")]
    public async Task<ActionResult<FormField>> UpdateField(
        int fieldId,
        [FromBody] FormField field,
        CancellationToken cancellationToken)
    {
        if (fieldId != field.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        var updated = await _formService.UpdateFieldAsync(field, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Remove a field from a form.
    /// </summary>
    [HttpDelete("fields/{fieldId:int}")]
    public async Task<ActionResult> RemoveField(int fieldId, CancellationToken cancellationToken)
    {
        var result = await _formService.RemoveFieldAsync(fieldId, cancellationToken);
        if (!result)
        {
            return NotFound($"Field with ID {fieldId} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Reorder fields within a form.
    /// </summary>
    [HttpPut("{formId:int}/fields/reorder")]
    public async Task<ActionResult<IEnumerable<FormField>>> ReorderFields(
        int formId,
        [FromBody] List<int> fieldIdsInOrder,
        CancellationToken cancellationToken)
    {
        var fields = await _formService.ReorderFieldsAsync(formId, fieldIdsInOrder, cancellationToken);
        return Ok(fields);
    }

    /// <summary>
    /// Bulk update fields.
    /// </summary>
    [HttpPut("{formId:int}/fields/bulk")]
    public async Task<ActionResult<IEnumerable<FormField>>> BulkUpdateFields(
        int formId,
        [FromBody] List<FormField> fields,
        CancellationToken cancellationToken)
    {
        var updated = await _formService.BulkUpdateFieldsAsync(formId, fields, cancellationToken);
        return Ok(updated);
    }

    #endregion

    #region Form Submissions

    /// <summary>
    /// Get all submissions for a form.
    /// </summary>
    [HttpGet("{formId:int}/submissions")]
    public async Task<ActionResult<IEnumerable<FormSubmission>>> GetSubmissions(
        int formId,
        [FromQuery] SubmissionStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var submissions = await _formService.GetSubmissionsAsync(formId, status, fromDate, toDate, cancellationToken);
        return Ok(submissions);
    }

    /// <summary>
    /// Get a specific submission by ID.
    /// </summary>
    [HttpGet("submissions/{submissionId:int}")]
    public async Task<ActionResult<FormSubmission>> GetSubmissionById(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _formService.GetSubmissionByIdAsync(submissionId, cancellationToken);
        if (submission == null)
        {
            return NotFound($"Submission with ID {submissionId} not found.");
        }
        return Ok(submission);
    }

    /// <summary>
    /// Get a submission by reference number.
    /// </summary>
    [HttpGet("submissions/by-number/{submissionNumber}")]
    public async Task<ActionResult<FormSubmission>> GetSubmissionByNumber(string submissionNumber, CancellationToken cancellationToken)
    {
        var submission = await _formService.GetSubmissionByNumberAsync(submissionNumber, cancellationToken);
        if (submission == null)
        {
            return NotFound($"Submission with number '{submissionNumber}' not found.");
        }
        return Ok(submission);
    }

    /// <summary>
    /// Process a new form submission.
    /// </summary>
    [HttpPost("{formId:int}/submit")]
    [AllowAnonymous]
    public async Task<ActionResult<FormSubmissionResult>> ProcessSubmission(
        int formId,
        [FromBody] FormSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var context = new FormSubmissionContext
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            Referrer = Request.Headers.Referer.ToString(),
            PageUrl = request.PageUrl,
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            UtmContent = request.UtmContent,
            UtmTerm = request.UtmTerm,
            HoneypotValue = request.HoneypotValue,
            SubmissionDuration = request.SubmissionDuration
        };

        var result = await _formService.ProcessSubmissionAsync(formId, request.FormData, context, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reprocess a failed submission.
    /// </summary>
    [HttpPost("submissions/{submissionId:int}/reprocess")]
    public async Task<ActionResult<FormSubmissionResult>> ReprocessSubmission(int submissionId, CancellationToken cancellationToken)
    {
        var result = await _formService.ReprocessSubmissionAsync(submissionId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Mark a submission as spam.
    /// </summary>
    [HttpPost("submissions/{submissionId:int}/mark-spam")]
    public async Task<ActionResult<FormSubmission>> MarkAsSpam(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _formService.MarkAsSpamAsync(submissionId, cancellationToken);
        return Ok(submission);
    }

    /// <summary>
    /// Mark a submission as not spam.
    /// </summary>
    [HttpPost("submissions/{submissionId:int}/mark-not-spam")]
    public async Task<ActionResult<FormSubmission>> MarkAsNotSpam(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _formService.MarkAsNotSpamAsync(submissionId, cancellationToken);
        return Ok(submission);
    }

    /// <summary>
    /// Delete a submission.
    /// </summary>
    [HttpDelete("submissions/{submissionId:int}")]
    public async Task<ActionResult> DeleteSubmission(int submissionId, CancellationToken cancellationToken)
    {
        var result = await _formService.DeleteSubmissionAsync(submissionId, cancellationToken);
        if (!result)
        {
            return NotFound($"Submission with ID {submissionId} not found.");
        }
        return NoContent();
    }

    #endregion

    #region Double Opt-In

    /// <summary>
    /// Send opt-in confirmation email.
    /// </summary>
    [HttpPost("submissions/{submissionId:int}/send-optin")]
    public async Task<ActionResult> SendOptInConfirmation(int submissionId, CancellationToken cancellationToken)
    {
        var result = await _formService.SendOptInConfirmationAsync(submissionId, cancellationToken);
        if (!result)
        {
            return BadRequest("Failed to send opt-in confirmation.");
        }
        return Ok(new { message = "Opt-in confirmation sent." });
    }

    /// <summary>
    /// Confirm double opt-in from email link.
    /// </summary>
    [HttpGet("confirm-optin/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<FormSubmission>> ConfirmOptIn(string token, CancellationToken cancellationToken)
    {
        var submission = await _formService.ConfirmOptInAsync(token, cancellationToken);
        if (submission == null)
        {
            return NotFound("Invalid or expired confirmation token.");
        }
        return Ok(submission);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate form data before submission.
    /// </summary>
    [HttpPost("{formId:int}/validate")]
    [AllowAnonymous]
    public async Task<ActionResult<FormValidationResult>> ValidateFormData(
        int formId,
        [FromBody] Dictionary<string, object> formData,
        CancellationToken cancellationToken)
    {
        var result = await _formService.ValidateFormDataAsync(formId, formData, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validate a single field value.
    /// </summary>
    [HttpPost("fields/{fieldId:int}/validate")]
    [AllowAnonymous]
    public async Task<ActionResult<FieldValidationResult>> ValidateField(
        int fieldId,
        [FromBody] object? value,
        CancellationToken cancellationToken)
    {
        var result = await _formService.ValidateFieldAsync(fieldId, value, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Spam Protection

    /// <summary>
    /// Calculate spam score for submission data.
    /// </summary>
    [HttpPost("{formId:int}/spam-score")]
    public async Task<ActionResult<int>> CalculateSpamScore(
        int formId,
        [FromBody] FormSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var context = new FormSubmissionContext
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            HoneypotValue = request.HoneypotValue,
            SubmissionDuration = request.SubmissionDuration
        };

        var score = await _formService.CalculateSpamScoreAsync(formId, request.FormData, context, cancellationToken);
        return Ok(new { spamScore = score });
    }

    #endregion

    #region Embedding & URLs

    /// <summary>
    /// Generate embed code for a form.
    /// </summary>
    [HttpGet("{formId:int}/embed-code")]
    public async Task<ActionResult<string>> GenerateEmbedCode(
        int formId,
        [FromQuery] string? baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var embedCode = await _formService.GenerateEmbedCodeAsync(formId, baseUrl, cancellationToken);
        return Ok(new { embedCode });
    }

    /// <summary>
    /// Generate direct URL for standalone form page.
    /// </summary>
    [HttpGet("{formId:int}/direct-url")]
    public async Task<ActionResult<string>> GenerateDirectUrl(
        int formId,
        [FromQuery] string? baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var directUrl = await _formService.GenerateDirectUrlAsync(formId, baseUrl, cancellationToken);
        return Ok(new { directUrl });
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get form statistics.
    /// </summary>
    [HttpGet("{formId:int}/statistics")]
    public async Task<ActionResult<FormStatistics>> GetFormStatistics(
        int formId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _formService.GetFormStatisticsAsync(formId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get submission statistics across all forms.
    /// </summary>
    [HttpGet("statistics/submissions")]
    public async Task<ActionResult<FormSubmissionStatistics>> GetSubmissionStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _formService.GetSubmissionStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get field-level statistics.
    /// </summary>
    [HttpGet("{formId:int}/field-statistics")]
    public async Task<ActionResult<IEnumerable<FormFieldStatistics>>> GetFieldStatistics(
        int formId,
        CancellationToken cancellationToken)
    {
        var stats = await _formService.GetFieldStatisticsAsync(formId, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Increment view count for a form.
    /// </summary>
    [HttpPost("{formId:int}/view")]
    [AllowAnonymous]
    public async Task<ActionResult> IncrementViewCount(int formId, CancellationToken cancellationToken)
    {
        await _formService.IncrementViewCountAsync(formId, cancellationToken);
        return Ok();
    }

    #endregion

    #region Templates

    /// <summary>
    /// Get available form templates.
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<IEnumerable<FormTemplate>>> GetFormTemplates(CancellationToken cancellationToken)
    {
        var templates = await _formService.GetFormTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Create a form from a template.
    /// </summary>
    [HttpPost("from-template")]
    public async Task<ActionResult<FormDefinition>> CreateFromTemplate(
        [FromBody] CreateFromTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var form = await _formService.CreateFromTemplateAsync(request.TemplateKey, request.FormName, request.OwnerId, cancellationToken);
        return CreatedAtAction(nameof(GetFormById), new { id = form.Id }, form);
    }

    #endregion
}

#region Request DTOs

public class FormStatusRequest
{
    public FormStatus Status { get; set; }
}

public class FormSubmissionRequest
{
    public Dictionary<string, object> FormData { get; set; } = new();
    public string? PageUrl { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    public string? HoneypotValue { get; set; }
    public TimeSpan? SubmissionDuration { get; set; }
}

public class CreateFromTemplateRequest
{
    public string TemplateKey { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
}

#endregion
