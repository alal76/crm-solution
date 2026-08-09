// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos.Reports;
using CRM.Core.Entities.Reports;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service backing the Report Templates Marketplace (REV-FE-003).
/// Provides the catalog of pre-built report templates and applies a template by
/// incrementing its download counter and returning its saved report configuration
/// for the frontend to hand off to the report designer.
/// </summary>
public interface IReportTemplateService
{
    /// <summary>
    /// Gets all report templates in the marketplace. Filtering (search/category) is
    /// performed client-side by the frontend against this full list, matching the
    /// existing ReportTemplatesPage.filterTemplates() behavior.
    /// </summary>
    Task<IEnumerable<ReportTemplateDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a report template: increments its download counter and returns the
    /// saved report configuration. Returns null when the template does not exist.
    /// </summary>
    Task<ApplyReportTemplateResultDto?> ApplyAsync(int id, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IReportTemplateService" />
public class ReportTemplateService : IReportTemplateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ICrmDbContext _context;
    private readonly ILogger<ReportTemplateService> _logger;

    public ReportTemplateService(ICrmDbContext context, ILogger<ReportTemplateService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportTemplateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _context.ReportTemplates
            .Include(t => t.AuthorUser)
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.Downloads)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(ToDto);
    }

    /// <inheritdoc />
    public async Task<ApplyReportTemplateResultDto?> ApplyAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _context.ReportTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (template == null)
        {
            return null;
        }

        template.Downloads += 1;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Report template {TemplateId} ({TemplateName}) applied. Downloads now {Downloads}.",
            template.Id, template.Name, template.Downloads);

        return new ApplyReportTemplateResultDto
        {
            TemplateId = template.Id,
            TemplateName = template.Name,
            ReportConfig = DeserializeReportConfig(template.ReportConfigJson),
            Downloads = template.Downloads
        };
    }

    private static ReportTemplateDto ToDto(ReportTemplate entity)
    {
        return new ReportTemplateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Category = entity.Category,
            Author = entity.AuthorUser != null
                ? $"{entity.AuthorUser.FirstName} {entity.AuthorUser.LastName}".Trim()
                : entity.AuthorDisplayName,
            Rating = entity.Rating,
            Downloads = entity.Downloads,
            Tags = DeserializeTags(entity.TagsJson),
            PreviewImage = entity.PreviewImageUrl,
            ReportConfig = DeserializeReportConfig(entity.ReportConfigJson),
            CreatedAt = entity.CreatedAt
        };
    }

    private static List<string> DeserializeTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(tagsJson, JsonOptions) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static Dictionary<string, object> DeserializeReportConfig(string? reportConfigJson)
    {
        if (string.IsNullOrWhiteSpace(reportConfigJson))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(reportConfigJson, JsonOptions)
                ?? new Dictionary<string, object>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>();
        }
    }
}
