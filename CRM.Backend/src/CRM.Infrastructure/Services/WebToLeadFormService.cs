// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Web-to-Lead Form Service (TODO-CRM002-04)
/// Manages web-to-lead form configurations and submissions.
/// </summary>
public class WebToLeadFormService : IWebToLeadFormService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILeadService _leadService;
    private readonly ILogger<WebToLeadFormService> _logger;

    public WebToLeadFormService(
        ICrmDbContext dbContext,
        ILeadService leadService,
        ILogger<WebToLeadFormService> logger)
    {
        _dbContext = dbContext;
        _leadService = leadService;
        _logger = logger;
    }

    public async Task<IEnumerable<WebToLeadForm>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.WebToLeadForms
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<WebToLeadForm>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _dbContext.WebToLeadForms
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<WebToLeadForm?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbContext.WebToLeadForms
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<WebToLeadForm?> GetByEmbedKeyAsync(string embedKey, CancellationToken ct = default)
    {
        return await _dbContext.WebToLeadForms
            .FirstOrDefaultAsync(x => x.EmbedKey == embedKey && !x.IsDeleted && x.IsActive, ct);
    }

    public async Task<WebToLeadForm> CreateAsync(WebToLeadForm form, CancellationToken ct = default)
    {
        form.CreatedAt = DateTime.UtcNow;
        form.EmbedKey ??= GenerateEmbedKey();
        _dbContext.WebToLeadForms.Add(form);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Created web-to-lead form {Id}: {Name}", form.Id, form.Name);
        return form;
    }

    public async Task<WebToLeadForm?> UpdateAsync(int id, WebToLeadForm form, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (existing == null) return null;

        existing.Name = form.Name;
        existing.Description = form.Description;
        existing.FieldsJson = form.FieldsJson;
        existing.TargetLeadSourceId = form.TargetLeadSourceId;
        existing.CaptchaEnabled = form.CaptchaEnabled;
        existing.NotifyEmail = form.NotifyEmail;
        existing.NotifyEmails = form.NotifyEmails;
        existing.RedirectUrl = form.RedirectUrl;
        existing.ThankYouMessage = form.ThankYouMessage;
        existing.IsActive = form.IsActive;
        existing.DefaultOwnerId = form.DefaultOwnerId;
        existing.CustomStyling = form.CustomStyling;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Updated web-to-lead form {Id}: {Name}", id, form.Name);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (existing == null) return false;

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted web-to-lead form {Id}", id);
        return true;
    }

    public async Task<(bool Success, int? LeadId, string? ErrorMessage)> ProcessSubmissionAsync(
        WebToLeadSubmissionDto submission,
        CancellationToken ct = default)
    {
        try
        {
            var form = await GetByEmbedKeyAsync(submission.FormEmbedKey, ct);
            if (form == null)
                return (false, null, "Form not found or inactive");

            // TODO: Validate CAPTCHA if enabled
            if (form.CaptchaEnabled && string.IsNullOrEmpty(submission.CaptchaToken))
                return (false, null, "CAPTCHA validation required");

            // Create lead from field values
            var lead = new Lead
            {
                FirstName = submission.FieldValues.GetValueOrDefault("firstName", string.Empty),
                LastName = submission.FieldValues.GetValueOrDefault("lastName", string.Empty),
                Email = submission.FieldValues.GetValueOrDefault("email", string.Empty),
                Phone = submission.FieldValues.GetValueOrDefault("phone"),
                CompanyName = submission.FieldValues.GetValueOrDefault("company"),
                Title = submission.FieldValues.GetValueOrDefault("title"),
                Website = submission.FieldValues.GetValueOrDefault("website"),
                LeadSourceId = form.TargetLeadSourceId,
                OwnerId = form.DefaultOwnerId,
                Source = LeadSource.Web,
                Status = LeadLifecycleStatus.New,
                OriginalSource = submission.SourceUrl,
                FirstTouchDate = DateTime.UtcNow
            };

            var leadId = await _leadService.CreateAsync(lead);

            // Update form statistics
            form.SubmissionCount++;
            form.LastSubmissionAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Processed web-to-lead submission for form {FormId}, created lead {LeadId}", form.Id, leadId);

            // TODO: Send notification emails if configured

            return (true, leadId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing web-to-lead submission");
            return (false, null, "An error occurred processing the submission");
        }
    }

    public async Task<string> GenerateEmbedKeyAsync(int formId, CancellationToken ct = default)
    {
        var form = await GetByIdAsync(formId, ct);
        if (form == null)
            throw new ArgumentException("Form not found", nameof(formId));

        form.EmbedKey = GenerateEmbedKey();
        form.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
        return form.EmbedKey;
    }

    public async Task<string> GetEmbedHtmlAsync(int formId, CancellationToken ct = default)
    {
        var form = await GetByIdAsync(formId, ct);
        if (form == null)
            throw new ArgumentException("Form not found", nameof(formId));

        // Generate basic embed HTML
        return $@"
<iframe 
    src=""/web-forms/{form.EmbedKey}"" 
    width=""100%"" 
    height=""600"" 
    frameborder=""0"" 
    title=""{form.Name}"">
</iframe>
";
    }

    private static string GenerateEmbedKey()
    {
        return $"wlf_{Guid.NewGuid():N}";
    }
}
