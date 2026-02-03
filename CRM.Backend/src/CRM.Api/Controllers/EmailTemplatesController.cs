// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing email templates
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailTemplatesController : ControllerBase
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EmailTemplatesController> _logger;

    public EmailTemplatesController(ICrmDbContext context, ILogger<EmailTemplatesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all email templates
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        try
        {
            var query = _context.Set<EmailTemplate>().Where(t => !t.IsDeleted);

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<EmailTemplateCategory>(category, true, out var cat))
            {
                query = query.Where(t => t.Category == cat);
            }

            var templates = await query
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .Select(t => new EmailTemplateListDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Category = t.Category.ToString(),
                    Subject = t.Subject,
                    IsActive = t.IsActive,
                    IsSystem = t.IsSystem,
                    UsageCount = t.UsageCount,
                    LastUsedAt = t.LastUsedAt,
                    CreatedAt = t.CreatedAt,
                })
                .ToListAsync();

            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email templates");
            return StatusCode(500, new { message = "Error retrieving templates" });
        }
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var template = await _context.Set<EmailTemplate>()
                .Where(t => t.Id == id && !t.IsDeleted)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                return NotFound(new { message = "Template not found" });
            }

            var dto = new EmailTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Category = template.Category.ToString(),
                Subject = template.Subject,
                PlainTextBody = template.PlainTextBody,
                HtmlBody = template.HtmlBody,
                IsActive = template.IsActive,
                IsSystem = template.IsSystem,
                FromEmail = template.FromEmail,
                FromName = template.FromName,
                ReplyToEmail = template.ReplyToEmail,
                UsageCount = template.UsageCount,
                LastUsedAt = template.LastUsedAt,
                CreatedAt = template.CreatedAt,
            };

            if (!string.IsNullOrEmpty(template.MergeFieldsJson))
            {
                dto.MergeFields = JsonSerializer.Deserialize<List<string>>(template.MergeFieldsJson);
            }

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error retrieving template" });
        }
    }

    /// <summary>
    /// Get template categories
    /// </summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = Enum.GetValues<EmailTemplateCategory>()
            .Select(c => new { value = (int)c, label = c.ToString() })
            .ToList();

        return Ok(categories);
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmailTemplateCreateDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Subject))
            {
                return BadRequest(new { message = "Name and Subject are required" });
            }

            var template = new EmailTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = Enum.TryParse<EmailTemplateCategory>(dto.Category, true, out var cat)
                    ? cat
                    : EmailTemplateCategory.General,
                Subject = dto.Subject,
                PlainTextBody = dto.PlainTextBody,
                HtmlBody = dto.HtmlBody,
                IsActive = dto.IsActive,
                FromEmail = dto.FromEmail,
                FromName = dto.FromName,
                ReplyToEmail = dto.ReplyToEmail,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Set<EmailTemplate>().Add(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created email template {TemplateName} with ID {TemplateId}", template.Name, template.Id);

            return CreatedAtAction(nameof(GetById), new { id = template.Id }, new
            {
                id = template.Id,
                message = "Template created successfully",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template");
            return StatusCode(500, new { message = "Error creating template" });
        }
    }

    /// <summary>
    /// Update an email template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailTemplateCreateDto dto)
    {
        try
        {
            var template = await _context.Set<EmailTemplate>().FindAsync(id);

            if (template == null || template.IsDeleted)
            {
                return NotFound(new { message = "Template not found" });
            }

            if (template.IsSystem)
            {
                return BadRequest(new { message = "Cannot modify system templates" });
            }

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.Category = Enum.TryParse<EmailTemplateCategory>(dto.Category, true, out var cat)
                ? cat
                : EmailTemplateCategory.General;
            template.Subject = dto.Subject;
            template.PlainTextBody = dto.PlainTextBody;
            template.HtmlBody = dto.HtmlBody;
            template.IsActive = dto.IsActive;
            template.FromEmail = dto.FromEmail;
            template.FromName = dto.FromName;
            template.ReplyToEmail = dto.ReplyToEmail;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated email template {TemplateId}", id);

            return Ok(new { message = "Template updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error updating template" });
        }
    }

    /// <summary>
    /// Delete an email template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var template = await _context.Set<EmailTemplate>().FindAsync(id);

            if (template == null || template.IsDeleted)
            {
                return NotFound(new { message = "Template not found" });
            }

            if (template.IsSystem)
            {
                return BadRequest(new { message = "Cannot delete system templates" });
            }

            template.IsDeleted = true;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted email template {TemplateId}", id);

            return Ok(new { message = "Template deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error deleting template" });
        }
    }

    /// <summary>
    /// Duplicate an email template
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var original = await _context.Set<EmailTemplate>()
                .Where(t => t.Id == id && !t.IsDeleted)
                .FirstOrDefaultAsync();

            if (original == null)
            {
                return NotFound(new { message = "Template not found" });
            }

            var copy = new EmailTemplate
            {
                Name = $"{original.Name} (Copy)",
                Description = original.Description,
                Category = original.Category,
                Subject = original.Subject,
                PlainTextBody = original.PlainTextBody,
                HtmlBody = original.HtmlBody,
                IsActive = false, // Start as inactive
                IsSystem = false,
                MergeFieldsJson = original.MergeFieldsJson,
                FromEmail = original.FromEmail,
                FromName = original.FromName,
                ReplyToEmail = original.ReplyToEmail,
                DefaultAttachmentsJson = original.DefaultAttachmentsJson,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Set<EmailTemplate>().Add(copy);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Duplicated email template {OriginalId} to {NewId}", id, copy.Id);

            return Ok(new
            {
                id = copy.Id,
                message = "Template duplicated successfully",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error duplicating template" });
        }
    }

    /// <summary>
    /// Preview an email template with sample data
    /// </summary>
    [HttpPost("{id}/preview")]
    public async Task<IActionResult> Preview(int id, [FromBody] Dictionary<string, string>? mergeData = null)
    {
        try
        {
            var template = await _context.Set<EmailTemplate>()
                .Where(t => t.Id == id && !t.IsDeleted)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                return NotFound(new { message = "Template not found" });
            }

            var subject = template.Subject;
            var body = template.HtmlBody ?? template.PlainTextBody ?? string.Empty;

            // Apply merge fields
            if (mergeData != null)
            {
                foreach (var kvp in mergeData)
                {
                    subject = subject.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                    body = body.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                }
            }

            return Ok(new
            {
                subject,
                body,
                plainText = template.PlainTextBody,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error previewing template" });
        }
    }
}

#region DTOs

public class EmailTemplateListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class EmailTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public string? HtmlBody { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? PreviewText { get; set; }
    public List<string>? MergeFields { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class EmailTemplateCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public string? HtmlBody { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? PreviewText { get; set; }
}

#endregion
