// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing landing pages.
/// Part of Marketing and Sales gap analysis implementation (G6).
/// </summary>
public class LandingPageService : ILandingPageService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<LandingPageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPageService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public LandingPageService(CrmDbContext context, ILogger<LandingPageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LandingPage>> GetAllAsync(int? campaignId = null, LandingPageStatus? status = null)
    {
        var query = _context.LandingPages
            .Include(lp => lp.FormDefinition)
            .Include(lp => lp.Campaign)
            .Include(lp => lp.CreatedByUser)
            .Where(lp => !lp.IsDeleted);

        if (campaignId.HasValue)
        {
            query = query.Where(lp => lp.CampaignId == campaignId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(lp => lp.Status == status.Value);
        }

        return await query.OrderByDescending(lp => lp.UpdatedAt).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<LandingPage?> GetByIdAsync(int id)
    {
        return await _context.LandingPages
            .Include(lp => lp.FormDefinition)
            .Include(lp => lp.Campaign)
            .Include(lp => lp.CreatedByUser)
            .Include(lp => lp.Blocks.OrderBy(b => b.SortOrder))
            .FirstOrDefaultAsync(lp => lp.Id == id && !lp.IsDeleted);
    }

    /// <inheritdoc />
    public async Task<LandingPage?> GetBySlugAsync(string slug)
    {
        return await _context.LandingPages
            .Include(lp => lp.FormDefinition)
            .ThenInclude(fd => fd!.Fields)
            .Include(lp => lp.Blocks.OrderBy(b => b.SortOrder))
            .FirstOrDefaultAsync(lp => lp.Slug == slug && lp.Status == LandingPageStatus.Published && lp.IsActive && !lp.IsDeleted);
    }

    /// <inheritdoc />
    public async Task<LandingPage> CreateAsync(LandingPage landingPage, int userId)
    {
        landingPage.CreatedByUserId = userId;
        landingPage.CreatedAt = DateTime.UtcNow;
        landingPage.Status = LandingPageStatus.Draft;

        // Ensure slug is unique
        if (string.IsNullOrEmpty(landingPage.Slug))
        {
            landingPage.Slug = await GenerateSlugAsync(landingPage.Name);
        }
        else if (!await IsSlugAvailableAsync(landingPage.Slug))
        {
            landingPage.Slug = await GenerateSlugAsync(landingPage.Slug);
        }

        _context.LandingPages.Add(landingPage);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created landing page {Id} with slug {Slug}", landingPage.Id, landingPage.Slug);
        return landingPage;
    }

    /// <inheritdoc />
    public async Task<LandingPage> UpdateAsync(LandingPage landingPage)
    {
        var existing = await _context.LandingPages.FindAsync(landingPage.Id);
        if (existing == null)
        {
            throw new ArgumentException($"Landing page with ID {landingPage.Id} not found");
        }

        // Update properties
        existing.Name = landingPage.Name;
        existing.Title = landingPage.Title;
        existing.MetaDescription = landingPage.MetaDescription;
        existing.MetaKeywords = landingPage.MetaKeywords;
        existing.ContentJson = landingPage.ContentJson;
        existing.CustomCss = landingPage.CustomCss;
        existing.CustomJs = landingPage.CustomJs;
        existing.FeaturedImageUrl = landingPage.FeaturedImageUrl;
        existing.FacebookPixelId = landingPage.FacebookPixelId;
        existing.GoogleAnalyticsId = landingPage.GoogleAnalyticsId;
        existing.TrackingCode = landingPage.TrackingCode;
        existing.FormDefinitionId = landingPage.FormDefinitionId;
        existing.CampaignId = landingPage.CampaignId;
        existing.ThankYouPageId = landingPage.ThankYouPageId;
        existing.RedirectUrl = landingPage.RedirectUrl;
        existing.ScheduledPublishAt = landingPage.ScheduledPublishAt;
        existing.ScheduledUnpublishAt = landingPage.ScheduledUnpublishAt;
        existing.SettingsJson = landingPage.SettingsJson;

        // Slug can only be changed if still in draft
        if (existing.Status == LandingPageStatus.Draft && !string.IsNullOrEmpty(landingPage.Slug) && landingPage.Slug != existing.Slug)
        {
            if (await IsSlugAvailableAsync(landingPage.Slug, existing.Id))
            {
                existing.Slug = landingPage.Slug;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated landing page {Id}", landingPage.Id);
        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var landingPage = await _context.LandingPages.FindAsync(id);
        if (landingPage == null)
        {
            return false;
        }

        landingPage.IsDeleted = true;
        landingPage.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted landing page {Id}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<LandingPage> PublishAsync(int id)
    {
        var landingPage = await _context.LandingPages.FindAsync(id);
        if (landingPage == null)
        {
            throw new ArgumentException($"Landing page with ID {id} not found");
        }

        landingPage.Status = LandingPageStatus.Published;
        landingPage.PublishedAt = DateTime.UtcNow;
        landingPage.IsActive = true;

        // Compile HTML content
        landingPage.HtmlContent = await CompileToHtmlAsync(id);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Published landing page {Id} at {PublishedAt}", id, landingPage.PublishedAt);
        return landingPage;
    }

    /// <inheritdoc />
    public async Task<LandingPage> UnpublishAsync(int id)
    {
        var landingPage = await _context.LandingPages.FindAsync(id);
        if (landingPage == null)
        {
            throw new ArgumentException($"Landing page with ID {id} not found");
        }

        landingPage.Status = LandingPageStatus.Draft;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Unpublished landing page {Id}", id);
        return landingPage;
    }

    /// <inheritdoc />
    public async Task<LandingPage> DuplicateAsync(int id, string newName, int userId)
    {
        var original = await GetByIdAsync(id);
        if (original == null)
        {
            throw new ArgumentException($"Landing page with ID {id} not found");
        }

        var duplicate = new LandingPage
        {
            Name = newName,
            Slug = await GenerateSlugAsync(newName),
            Title = original.Title,
            MetaDescription = original.MetaDescription,
            MetaKeywords = original.MetaKeywords,
            Template = original.Template,
            ContentJson = original.ContentJson,
            CustomCss = original.CustomCss,
            CustomJs = original.CustomJs,
            FeaturedImageUrl = original.FeaturedImageUrl,
            FacebookPixelId = original.FacebookPixelId,
            GoogleAnalyticsId = original.GoogleAnalyticsId,
            TrackingCode = original.TrackingCode,
            FormDefinitionId = original.FormDefinitionId,
            CampaignId = original.CampaignId,
            ThankYouPageId = original.ThankYouPageId,
            RedirectUrl = original.RedirectUrl,
            SettingsJson = original.SettingsJson,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = LandingPageStatus.Draft,
            IsActive = true,
        };

        _context.LandingPages.Add(duplicate);
        await _context.SaveChangesAsync();

        // Duplicate blocks
        foreach (var block in original.Blocks)
        {
            var duplicateBlock = new LandingPageBlock
            {
                LandingPageId = duplicate.Id,
                BlockType = block.BlockType,
                SortOrder = block.SortOrder,
                ContentJson = block.ContentJson,
                StyleJson = block.StyleJson,
                VisibilityCondition = block.VisibilityCondition,
                IsVisible = block.IsVisible,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.LandingPageBlocks.Add(duplicateBlock);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Duplicated landing page {OriginalId} to {NewId}", id, duplicate.Id);
        return duplicate;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LandingPageBlock>> GetBlocksAsync(int landingPageId)
    {
        return await _context.LandingPageBlocks
            .Where(b => b.LandingPageId == landingPageId && !b.IsDeleted)
            .OrderBy(b => b.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LandingPageBlock>> UpdateBlocksAsync(int landingPageId, IEnumerable<LandingPageBlock> blocks)
    {
        // Remove existing blocks
        var existingBlocks = await _context.LandingPageBlocks
            .Where(b => b.LandingPageId == landingPageId)
            .ToListAsync();
        _context.LandingPageBlocks.RemoveRange(existingBlocks);

        // Add new blocks
        var sortOrder = 0;
        foreach (var block in blocks)
        {
            block.LandingPageId = landingPageId;
            block.SortOrder = sortOrder++;
            block.CreatedAt = DateTime.UtcNow;
            _context.LandingPageBlocks.Add(block);
        }

        // Update landing page timestamp
        var landingPage = await _context.LandingPages.FindAsync(landingPageId);
        if (landingPage != null)
        {
            landingPage.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await GetBlocksAsync(landingPageId);
    }

    /// <inheritdoc />
    public async Task<LandingPageVisit> RecordVisitAsync(LandingPageVisit visit)
    {
        visit.CreatedAt = DateTime.UtcNow;
        visit.VisitedAt = DateTime.UtcNow;

        _context.LandingPageVisits.Add(visit);

        // Update page analytics
        var landingPage = await _context.LandingPages.FindAsync(visit.LandingPageId);
        if (landingPage != null)
        {
            landingPage.PageViews++;

            // Check if unique visitor
            var isUnique = !await _context.LandingPageVisits
                .AnyAsync(v => v.LandingPageId == visit.LandingPageId && v.VisitorId == visit.VisitorId && v.Id != visit.Id);

            if (isUnique)
            {
                landingPage.UniqueVisitors++;
            }
        }

        await _context.SaveChangesAsync();
        return visit;
    }

    /// <inheritdoc />
    public async Task<bool> RecordConversionAsync(int landingPageId, string? visitorId, int? leadId)
    {
        var landingPage = await _context.LandingPages.FindAsync(landingPageId);
        if (landingPage == null)
        {
            return false;
        }

        landingPage.Conversions++;

        // Update the visit record if visitor ID is provided
        if (!string.IsNullOrEmpty(visitorId))
        {
            var visit = await _context.LandingPageVisits
                .Where(v => v.LandingPageId == landingPageId && v.VisitorId == visitorId && !v.Converted)
                .OrderByDescending(v => v.VisitedAt)
                .FirstOrDefaultAsync();

            if (visit != null)
            {
                visit.Converted = true;
                visit.ConvertedAt = DateTime.UtcNow;
                visit.LeadId = leadId;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<LandingPageAnalytics> GetAnalyticsAsync(int landingPageId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var visits = await _context.LandingPageVisits
            .Where(v => v.LandingPageId == landingPageId && v.VisitedAt >= start && v.VisitedAt <= end)
            .ToListAsync();

        var landingPage = await _context.LandingPages.FindAsync(landingPageId);

        var analytics = new LandingPageAnalytics
        {
            TotalPageViews = visits.Count,
            UniqueVisitors = visits.Select(v => v.VisitorId).Distinct().Count(),
            Conversions = visits.Count(v => v.Converted),
            AverageTimeOnPage = visits.Where(v => v.TimeOnPageSeconds.HasValue).Select(v => v.TimeOnPageSeconds!.Value).DefaultIfEmpty(0).Average(),
        };

        analytics.ConversionRate = analytics.UniqueVisitors > 0
            ? (decimal)analytics.Conversions / analytics.UniqueVisitors * 100
            : 0;

        // Calculate bounce rate (visits with less than 10 seconds)
        var bounces = visits.Count(v => !v.TimeOnPageSeconds.HasValue || v.TimeOnPageSeconds < 10);
        analytics.BounceRate = visits.Count > 0 ? (decimal)bounces / visits.Count * 100 : 0;

        // Views by date
        analytics.ViewsByDate = visits
            .GroupBy(v => v.VisitedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        // Conversions by date
        analytics.ConversionsByDate = visits
            .Where(v => v.Converted && v.ConvertedAt.HasValue)
            .GroupBy(v => v.ConvertedAt!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        // Views by device
        analytics.ViewsByDevice = visits
            .Where(v => !string.IsNullOrEmpty(v.DeviceType))
            .GroupBy(v => v.DeviceType!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Top referrers
        analytics.TopReferrers = visits
            .Where(v => !string.IsNullOrEmpty(v.Referrer))
            .GroupBy(v => v.Referrer!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        // Views by country
        analytics.ViewsByCountry = visits
            .Where(v => !string.IsNullOrEmpty(v.Country))
            .GroupBy(v => v.Country!)
            .ToDictionary(g => g.Key, g => g.Count());

        // UTM sources
        analytics.UtmSources = visits
            .Where(v => !string.IsNullOrEmpty(v.UtmSource))
            .GroupBy(v => v.UtmSource!)
            .ToDictionary(g => g.Key, g => g.Count());

        return analytics;
    }

    /// <inheritdoc />
    public async Task<string> CompileToHtmlAsync(int landingPageId)
    {
        var landingPage = await GetByIdAsync(landingPageId);
        if (landingPage == null)
        {
            throw new ArgumentException($"Landing page with ID {landingPageId} not found");
        }

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"  <title>{System.Net.WebUtility.HtmlEncode(landingPage.Title ?? landingPage.Name)}</title>");

        if (!string.IsNullOrEmpty(landingPage.MetaDescription))
        {
            sb.AppendLine($"  <meta name=\"description\" content=\"{System.Net.WebUtility.HtmlEncode(landingPage.MetaDescription)}\">");
        }

        if (!string.IsNullOrEmpty(landingPage.MetaKeywords))
        {
            sb.AppendLine($"  <meta name=\"keywords\" content=\"{System.Net.WebUtility.HtmlEncode(landingPage.MetaKeywords)}\">");
        }

        if (!string.IsNullOrEmpty(landingPage.FeaturedImageUrl))
        {
            sb.AppendLine($"  <meta property=\"og:image\" content=\"{System.Net.WebUtility.HtmlEncode(landingPage.FeaturedImageUrl)}\">");
        }

        // Google Analytics
        if (!string.IsNullOrEmpty(landingPage.GoogleAnalyticsId))
        {
            sb.AppendLine($"  <script async src=\"https://www.googletagmanager.com/gtag/js?id={landingPage.GoogleAnalyticsId}\"></script>");
            sb.AppendLine("  <script>");
            sb.AppendLine("    window.dataLayer = window.dataLayer || [];");
            sb.AppendLine("    function gtag(){dataLayer.push(arguments);}");
            sb.AppendLine("    gtag('js', new Date());");
            sb.AppendLine($"    gtag('config', '{landingPage.GoogleAnalyticsId}');");
            sb.AppendLine("  </script>");
        }

        // Facebook Pixel
        if (!string.IsNullOrEmpty(landingPage.FacebookPixelId))
        {
            sb.AppendLine("  <script>");
            sb.AppendLine("    !function(f,b,e,v,n,t,s)");
            sb.AppendLine("    {if(f.fbq)return;n=f.fbq=function(){n.callMethod?");
            sb.AppendLine("    n.callMethod.apply(n,arguments):n.queue.push(arguments)};");
            sb.AppendLine("    if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';");
            sb.AppendLine("    n.queue=[];t=b.createElement(e);t.async=!0;");
            sb.AppendLine("    t.src=v;s=b.getElementsByTagName(e)[0];");
            sb.AppendLine("    s.parentNode.insertBefore(t,s)}(window, document,'script',");
            sb.AppendLine("    'https://connect.facebook.net/en_US/fbevents.js');");
            sb.AppendLine($"    fbq('init', '{landingPage.FacebookPixelId}');");
            sb.AppendLine("    fbq('track', 'PageView');");
            sb.AppendLine("  </script>");
        }

        // Custom tracking code
        if (!string.IsNullOrEmpty(landingPage.TrackingCode))
        {
            sb.AppendLine(landingPage.TrackingCode);
        }

        // Default styles
        sb.AppendLine("  <style>");
        sb.AppendLine("    * { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif; line-height: 1.6; }");
        sb.AppendLine("    .container { max-width: 1200px; margin: 0 auto; padding: 0 20px; }");
        sb.AppendLine("    .section { padding: 60px 0; }");
        sb.AppendLine("    .btn { display: inline-block; padding: 12px 24px; border-radius: 4px; text-decoration: none; font-weight: 600; cursor: pointer; transition: all 0.3s; }");
        sb.AppendLine("    .btn-primary { background: #1976d2; color: white; border: none; }");
        sb.AppendLine("    .btn-primary:hover { background: #1565c0; }");
        sb.AppendLine("  </style>");

        // Custom CSS
        if (!string.IsNullOrEmpty(landingPage.CustomCss))
        {
            sb.AppendLine("  <style>");
            sb.AppendLine(landingPage.CustomCss);
            sb.AppendLine("  </style>");
        }

        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Render blocks
        foreach (var block in landingPage.Blocks.Where(b => b.IsVisible).OrderBy(b => b.SortOrder))
        {
            sb.AppendLine(await RenderBlockAsync(block, landingPage));
        }

        // Custom JS
        if (!string.IsNullOrEmpty(landingPage.CustomJs))
        {
            sb.AppendLine("<script>");
            sb.AppendLine(landingPage.CustomJs);
            sb.AppendLine("</script>");
        }

        // Tracking script for time on page
        sb.AppendLine("<script>");
        sb.AppendLine("  var pageLoadTime = new Date();");
        sb.AppendLine("  window.addEventListener('beforeunload', function() {");
        sb.AppendLine("    var timeOnPage = Math.round((new Date() - pageLoadTime) / 1000);");
        sb.AppendLine($"    navigator.sendBeacon('/api/landing-pages/{landingPage.Id}/time?seconds=' + timeOnPage);");
        sb.AppendLine("  });");
        sb.AppendLine("</script>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private async Task<string> RenderBlockAsync(LandingPageBlock block, LandingPage landingPage)
    {
        var content = !string.IsNullOrEmpty(block.ContentJson)
            ? JsonDocument.Parse(block.ContentJson).RootElement
            : default;

        var style = !string.IsNullOrEmpty(block.StyleJson)
            ? JsonDocument.Parse(block.StyleJson).RootElement
            : default;

        return block.BlockType switch
        {
            LandingPageBlockType.Hero => RenderHeroBlock(content, style),
            LandingPageBlockType.Text => RenderTextBlock(content, style),
            LandingPageBlockType.Image => RenderImageBlock(content, style),
            LandingPageBlockType.Button => RenderButtonBlock(content, style),
            LandingPageBlockType.Form => await RenderFormBlockAsync(content, style, landingPage),
            LandingPageBlockType.TwoColumn => RenderTwoColumnBlock(content, style),
            LandingPageBlockType.Features => RenderFeaturesBlock(content, style),
            LandingPageBlockType.Testimonial => RenderTestimonialBlock(content, style),
            LandingPageBlockType.CustomHtml => RenderCustomHtmlBlock(content),
            LandingPageBlockType.Divider => RenderDividerBlock(style),
            _ => $"<!-- Block type {block.BlockType} not implemented -->",
        };
    }

    private string RenderHeroBlock(JsonElement content, JsonElement style)
    {
        var heading = GetJsonString(content, "heading") ?? "Welcome";
        var subheading = GetJsonString(content, "subheading") ?? "";
        var buttonText = GetJsonString(content, "buttonText") ?? "Get Started";
        var buttonUrl = GetJsonString(content, "buttonUrl") ?? "#";
        var backgroundImage = GetJsonString(content, "backgroundImage") ?? "";
        var backgroundColor = GetJsonString(style, "backgroundColor") ?? "#1976d2";
        var textColor = GetJsonString(style, "textColor") ?? "white";

        var bgStyle = !string.IsNullOrEmpty(backgroundImage)
            ? $"background: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('{backgroundImage}') center/cover;"
            : $"background: {backgroundColor};";

        return $@"
<section class=""section hero"" style=""{bgStyle} color: {textColor}; text-align: center; padding: 100px 0;"">
  <div class=""container"">
    <h1 style=""font-size: 3rem; margin-bottom: 20px;"">{System.Net.WebUtility.HtmlEncode(heading)}</h1>
    {(!string.IsNullOrEmpty(subheading) ? $"<p style=\"font-size: 1.25rem; margin-bottom: 30px;\">{System.Net.WebUtility.HtmlEncode(subheading)}</p>" : "")}
    <a href=""{System.Net.WebUtility.HtmlEncode(buttonUrl)}"" class=""btn btn-primary"" style=""font-size: 1.1rem;"">{System.Net.WebUtility.HtmlEncode(buttonText)}</a>
  </div>
</section>";
    }

    private string RenderTextBlock(JsonElement content, JsonElement style)
    {
        var text = GetJsonString(content, "text") ?? "";
        var alignment = GetJsonString(style, "textAlign") ?? "left";

        return $@"
<section class=""section text-block"" style=""text-align: {alignment};"">
  <div class=""container"">
    <div style=""max-width: 800px; margin: 0 auto;"">{text}</div>
  </div>
</section>";
    }

    private string RenderImageBlock(JsonElement content, JsonElement style)
    {
        var imageUrl = GetJsonString(content, "imageUrl") ?? "";
        var altText = GetJsonString(content, "altText") ?? "";
        var width = GetJsonString(style, "width") ?? "100%";

        return $@"
<section class=""section image-block"" style=""text-align: center;"">
  <div class=""container"">
    <img src=""{System.Net.WebUtility.HtmlEncode(imageUrl)}"" alt=""{System.Net.WebUtility.HtmlEncode(altText)}"" style=""max-width: {width}; height: auto;"">
  </div>
</section>";
    }

    private string RenderButtonBlock(JsonElement content, JsonElement style)
    {
        var buttonText = GetJsonString(content, "text") ?? "Click Here";
        var buttonUrl = GetJsonString(content, "url") ?? "#";
        var alignment = GetJsonString(style, "textAlign") ?? "center";
        var backgroundColor = GetJsonString(style, "backgroundColor") ?? "#1976d2";
        var textColor = GetJsonString(style, "color") ?? "white";

        return $@"
<section class=""section button-block"" style=""text-align: {alignment};"">
  <div class=""container"">
    <a href=""{System.Net.WebUtility.HtmlEncode(buttonUrl)}"" class=""btn"" style=""background: {backgroundColor}; color: {textColor};"">{System.Net.WebUtility.HtmlEncode(buttonText)}</a>
  </div>
</section>";
    }

    private Task<string> RenderFormBlockAsync(JsonElement content, JsonElement style, LandingPage landingPage)
    {
        if (!landingPage.FormDefinitionId.HasValue || landingPage.FormDefinition == null)
        {
            return Task.FromResult("<!-- No form configured -->");
        }

        var form = landingPage.FormDefinition;
        var heading = GetJsonString(content, "heading") ?? form.Name;
        var submitButtonText = GetJsonString(content, "submitButtonText") ?? "Submit";
        var backgroundColor = GetJsonString(style, "backgroundColor") ?? "#f5f5f5";

        var sb = new StringBuilder();
        sb.AppendLine($@"
<section class=""section form-block"" style=""background: {backgroundColor};"">
  <div class=""container"">
    <div style=""max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
      <h2 style=""margin-bottom: 30px; text-align: center;"">{System.Net.WebUtility.HtmlEncode(heading)}</h2>
      <form action=""/api/forms/{form.Id}/submit"" method=""POST"">");

        if (form.Fields != null)
        {
            foreach (var field in form.Fields.OrderBy(f => f.Order))
            {
                var required = field.IsRequired ? "required" : "";
                sb.AppendLine($@"
        <div style=""margin-bottom: 20px;"">
          <label style=""display: block; margin-bottom: 5px; font-weight: 600;"">{System.Net.WebUtility.HtmlEncode(field.Label)}{(field.IsRequired ? " *" : "")}</label>
          <input type=""{GetHtmlInputType(field.FieldType)}"" name=""{field.FieldName}"" placeholder=""{System.Net.WebUtility.HtmlEncode(field.Placeholder ?? "")}"" {required} style=""width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 16px;"">
        </div>");
            }
        }

        sb.AppendLine($@"
        <button type=""submit"" class=""btn btn-primary"" style=""width: 100%;"">{System.Net.WebUtility.HtmlEncode(submitButtonText)}</button>
      </form>
    </div>
  </div>
</section>");

        return Task.FromResult(sb.ToString());
    }

    private string RenderTwoColumnBlock(JsonElement content, JsonElement style)
    {
        var leftContent = GetJsonString(content, "leftContent") ?? "";
        var rightContent = GetJsonString(content, "rightContent") ?? "";

        return $@"
<section class=""section two-column"">
  <div class=""container"" style=""display: flex; gap: 40px; flex-wrap: wrap;"">
    <div style=""flex: 1; min-width: 300px;"">{leftContent}</div>
    <div style=""flex: 1; min-width: 300px;"">{rightContent}</div>
  </div>
</section>";
    }

    private string RenderFeaturesBlock(JsonElement content, JsonElement style)
    {
        var heading = GetJsonString(content, "heading") ?? "Features";
        var sb = new StringBuilder();
        sb.AppendLine($@"
<section class=""section features-block"">
  <div class=""container"">
    <h2 style=""text-align: center; margin-bottom: 40px;"">{System.Net.WebUtility.HtmlEncode(heading)}</h2>
    <div style=""display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 30px;"">");

        if (content.ValueKind != JsonValueKind.Undefined && content.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
        {
            foreach (var feature in features.EnumerateArray())
            {
                var title = GetJsonString(feature, "title") ?? "";
                var description = GetJsonString(feature, "description") ?? "";
                var icon = GetJsonString(feature, "icon") ?? "✓";

                sb.AppendLine($@"
      <div style=""text-align: center; padding: 20px;"">
        <div style=""font-size: 2rem; margin-bottom: 15px;"">{icon}</div>
        <h3 style=""margin-bottom: 10px;"">{System.Net.WebUtility.HtmlEncode(title)}</h3>
        <p style=""color: #666;"">{System.Net.WebUtility.HtmlEncode(description)}</p>
      </div>");
            }
        }

        sb.AppendLine(@"
    </div>
  </div>
</section>");

        return sb.ToString();
    }

    private string RenderTestimonialBlock(JsonElement content, JsonElement style)
    {
        var quote = GetJsonString(content, "quote") ?? "";
        var author = GetJsonString(content, "author") ?? "";
        var role = GetJsonString(content, "role") ?? "";
        var image = GetJsonString(content, "image") ?? "";

        return $@"
<section class=""section testimonial-block"" style=""background: #f9f9f9;"">
  <div class=""container"">
    <div style=""max-width: 700px; margin: 0 auto; text-align: center;"">
      <blockquote style=""font-size: 1.25rem; font-style: italic; margin-bottom: 20px;"">""{System.Net.WebUtility.HtmlEncode(quote)}""</blockquote>
      {(!string.IsNullOrEmpty(image) ? $"<img src=\"{System.Net.WebUtility.HtmlEncode(image)}\" alt=\"{System.Net.WebUtility.HtmlEncode(author)}\" style=\"width: 60px; height: 60px; border-radius: 50%; margin-bottom: 10px;\">" : "")}
      <div style=""font-weight: 600;"">{System.Net.WebUtility.HtmlEncode(author)}</div>
      {(!string.IsNullOrEmpty(role) ? $"<div style=\"color: #666;\">{System.Net.WebUtility.HtmlEncode(role)}</div>" : "")}
    </div>
  </div>
</section>";
    }

    private string RenderCustomHtmlBlock(JsonElement content)
    {
        var html = GetJsonString(content, "html") ?? "";
        return $"<section class=\"section custom-html\">{html}</section>";
    }

    private string RenderDividerBlock(JsonElement style)
    {
        var height = GetJsonString(style, "height") ?? "40px";
        var showLine = style.ValueKind != JsonValueKind.Undefined && style.TryGetProperty("showLine", out var sl) && sl.GetBoolean();

        return showLine
            ? $"<hr style=\"margin: {height} auto; max-width: 800px; border: 0; border-top: 1px solid #ddd;\">"
            : $"<div style=\"height: {height};\"></div>";
    }

    private static string GetHtmlInputType(FormFieldType fieldType)
    {
        return fieldType switch
        {
            FormFieldType.Email => "email",
            FormFieldType.Phone => "tel",
            FormFieldType.Number => "number",
            FormFieldType.Date => "date",
            FormFieldType.DateTime => "datetime-local",
            FormFieldType.Url => "url",
            FormFieldType.Hidden => "hidden",
            _ => "text",
        };
    }

    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> IsSlugAvailableAsync(string slug, int? excludeId = null)
    {
        var query = _context.LandingPages.Where(lp => lp.Slug == slug && !lp.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(lp => lp.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<string> GenerateSlugAsync(string name)
    {
        // Convert to lowercase and replace spaces/special chars with hyphens
        var slug = Regex.Replace(name.ToLower(), @"[^a-z0-9]+", "-", RegexOptions.None, TimeSpan.FromSeconds(1)).Trim('-');

        // Ensure uniqueness
        var baseSlug = slug;
        var counter = 1;
        while (!await IsSlugAvailableAsync(slug))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        return slug;
    }

    /// <inheritdoc />
    public async Task<LandingPage> CreateVariantAsync(int originalPageId, string variantName, int trafficPercentage, int userId)
    {
        var original = await GetByIdAsync(originalPageId);
        if (original == null)
        {
            throw new ArgumentException($"Landing page with ID {originalPageId} not found");
        }

        var variant = await DuplicateAsync(originalPageId, variantName, userId);
        variant.ABTestVariant = variantName;
        variant.OriginalPageId = originalPageId;
        variant.ABTestTrafficPercentage = trafficPercentage;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created A/B test variant {VariantId} for page {OriginalId}", variant.Id, originalPageId);
        return variant;
    }
}
