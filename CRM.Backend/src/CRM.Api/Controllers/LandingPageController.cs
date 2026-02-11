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

using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing landing pages.
/// Part of Marketing and Sales gap analysis implementation (G6).
/// </summary>
[ApiController]
[Route("api/landing-pages")]
public class LandingPageController : ControllerBase
{
    private readonly ILandingPageService _landingPageService;
    private readonly ILogger<LandingPageController> _logger;

    public LandingPageController(ILandingPageService landingPageService, ILogger<LandingPageController> logger)
    {
        _landingPageService = landingPageService;
        _logger = logger;
    }

    /// <summary>
    /// Get all landing pages with optional filtering.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LandingPageListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LandingPageListDto>>> GetAll(
        [FromQuery] int? campaignId = null,
        [FromQuery] LandingPageStatus? status = null)
    {
        var pages = await _landingPageService.GetAllAsync(campaignId, status);
        var dtos = pages.Select(p => new LandingPageListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Status = p.Status.ToString(),
            Template = p.Template.ToString(),
            CampaignId = p.CampaignId,
            CampaignName = p.Campaign?.Name,
            FormDefinitionId = p.FormDefinitionId,
            FormName = p.FormDefinition?.Name,
            PageViews = p.PageViews,
            UniqueVisitors = p.UniqueVisitors,
            Conversions = p.Conversions,
            ConversionRate = p.ConversionRate,
            PublishedAt = p.PublishedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CreatedByUserName = p.CreatedByUser?.Username,
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Get a landing page by ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageDto>> GetById(int id)
    {
        var page = await _landingPageService.GetByIdAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(page));
    }

    /// <summary>
    /// Create a new landing page.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LandingPageDto>> Create([FromBody] CreateLandingPageDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var landingPage = new LandingPage
        {
            Name = dto.Name,
            Slug = dto.Slug ?? "",
            Title = dto.Title,
            MetaDescription = dto.MetaDescription,
            MetaKeywords = dto.MetaKeywords,
            Template = Enum.TryParse<LandingPageTemplate>(dto.Template, out var template) ? template : LandingPageTemplate.Blank,
            FormDefinitionId = dto.FormDefinitionId,
            CampaignId = dto.CampaignId,
            ThankYouPageId = dto.ThankYouPageId,
            RedirectUrl = dto.RedirectUrl,
            ContentJson = dto.ContentJson,
            CustomCss = dto.CustomCss,
            CustomJs = dto.CustomJs,
            FacebookPixelId = dto.FacebookPixelId,
            GoogleAnalyticsId = dto.GoogleAnalyticsId,
            TrackingCode = dto.TrackingCode,
            FeaturedImageUrl = dto.FeaturedImageUrl,
            SettingsJson = dto.SettingsJson,
        };

        var created = await _landingPageService.CreateAsync(landingPage, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Update a landing page.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageDto>> Update(int id, [FromBody] UpdateLandingPageDto dto)
    {
        var existing = await _landingPageService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = dto.Name ?? existing.Name;
        existing.Slug = dto.Slug ?? existing.Slug;
        existing.Title = dto.Title ?? existing.Title;
        existing.MetaDescription = dto.MetaDescription ?? existing.MetaDescription;
        existing.MetaKeywords = dto.MetaKeywords ?? existing.MetaKeywords;
        existing.FormDefinitionId = dto.FormDefinitionId ?? existing.FormDefinitionId;
        existing.CampaignId = dto.CampaignId ?? existing.CampaignId;
        existing.ThankYouPageId = dto.ThankYouPageId ?? existing.ThankYouPageId;
        existing.RedirectUrl = dto.RedirectUrl ?? existing.RedirectUrl;
        existing.ContentJson = dto.ContentJson ?? existing.ContentJson;
        existing.CustomCss = dto.CustomCss ?? existing.CustomCss;
        existing.CustomJs = dto.CustomJs ?? existing.CustomJs;
        existing.FacebookPixelId = dto.FacebookPixelId ?? existing.FacebookPixelId;
        existing.GoogleAnalyticsId = dto.GoogleAnalyticsId ?? existing.GoogleAnalyticsId;
        existing.TrackingCode = dto.TrackingCode ?? existing.TrackingCode;
        existing.FeaturedImageUrl = dto.FeaturedImageUrl ?? existing.FeaturedImageUrl;
        existing.ScheduledPublishAt = dto.ScheduledPublishAt ?? existing.ScheduledPublishAt;
        existing.ScheduledUnpublishAt = dto.ScheduledUnpublishAt ?? existing.ScheduledUnpublishAt;
        existing.SettingsJson = dto.SettingsJson ?? existing.SettingsJson;

        var updated = await _landingPageService.UpdateAsync(existing);
        return Ok(MapToDto(updated));
    }

    /// <summary>
    /// Delete a landing page.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _landingPageService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Publish a landing page.
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageDto>> Publish(int id)
    {
        try
        {
            var page = await _landingPageService.PublishAsync(id);
            return Ok(MapToDto(page));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Unpublish a landing page.
    /// </summary>
    [HttpPost("{id}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageDto>> Unpublish(int id)
    {
        try
        {
            var page = await _landingPageService.UnpublishAsync(id);
            return Ok(MapToDto(page));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Duplicate a landing page.
    /// </summary>
    [HttpPost("{id}/duplicate")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageDto>> Duplicate(int id, [FromBody] DuplicateLandingPageDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var page = await _landingPageService.DuplicateAsync(id, dto.NewName, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = page.Id }, MapToDto(page));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get blocks for a landing page.
    /// </summary>
    [HttpGet("{id}/blocks")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LandingPageBlockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LandingPageBlockDto>>> GetBlocks(int id)
    {
        var blocks = await _landingPageService.GetBlocksAsync(id);
        var dtos = blocks.Select(b => new LandingPageBlockDto
        {
            Id = b.Id,
            BlockType = b.BlockType.ToString(),
            SortOrder = b.SortOrder,
            ContentJson = b.ContentJson,
            StyleJson = b.StyleJson,
            VisibilityCondition = b.VisibilityCondition,
            IsVisible = b.IsVisible,
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Update blocks for a landing page.
    /// </summary>
    [HttpPut("{id}/blocks")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LandingPageBlockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LandingPageBlockDto>>> UpdateBlocks(int id, [FromBody] List<UpdateLandingPageBlockDto> dtos)
    {
        var blocks = dtos.Select(dto => new LandingPageBlock
        {
            BlockType = Enum.TryParse<LandingPageBlockType>(dto.BlockType, out var bt) ? bt : LandingPageBlockType.Text,
            SortOrder = dto.SortOrder,
            ContentJson = dto.ContentJson,
            StyleJson = dto.StyleJson,
            VisibilityCondition = dto.VisibilityCondition,
            IsVisible = dto.IsVisible,
        });

        var updatedBlocks = await _landingPageService.UpdateBlocksAsync(id, blocks);
        var result = updatedBlocks.Select(b => new LandingPageBlockDto
        {
            Id = b.Id,
            BlockType = b.BlockType.ToString(),
            SortOrder = b.SortOrder,
            ContentJson = b.ContentJson,
            StyleJson = b.StyleJson,
            VisibilityCondition = b.VisibilityCondition,
            IsVisible = b.IsVisible,
        });

        return Ok(result);
    }

    /// <summary>
    /// Get analytics for a landing page.
    /// </summary>
    [HttpGet("{id}/analytics")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageAnalytics), StatusCodes.Status200OK)]
    public async Task<ActionResult<LandingPageAnalytics>> GetAnalytics(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _landingPageService.GetAnalyticsAsync(id, startDate, endDate);
        return Ok(analytics);
    }

    /// <summary>
    /// Preview compiled HTML.
    /// </summary>
    [HttpGet("{id}/preview")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Preview(int id)
    {
        try
        {
            var html = await _landingPageService.CompileToHtmlAsync(id);
            return Content(html, "text/html");
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if a slug is available.
    /// </summary>
    [HttpGet("check-slug")]
    [Authorize]
    [ProducesResponseType(typeof(SlugAvailabilityDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SlugAvailabilityDto>> CheckSlug([FromQuery] string slug, [FromQuery] int? excludeId = null)
    {
        var isAvailable = await _landingPageService.IsSlugAvailableAsync(slug, excludeId);
        var suggestion = isAvailable ? slug : await _landingPageService.GenerateSlugAsync(slug);

        return Ok(new SlugAvailabilityDto
        {
            Slug = slug,
            IsAvailable = isAvailable,
            SuggestedSlug = suggestion,
        });
    }

    /// <summary>
    /// Create an A/B test variant.
    /// </summary>
    [HttpPost("{id}/variant")]
    [Authorize]
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandingPageDto>> CreateVariant(int id, [FromBody] CreateVariantDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var variant = await _landingPageService.CreateVariantAsync(id, dto.VariantName, dto.TrafficPercentage, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = variant.Id }, MapToDto(variant));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // =============================
    // PUBLIC ENDPOINTS (No Auth)
    // =============================

    /// <summary>
    /// Serve a published landing page by slug.
    /// </summary>
    [HttpGet("p/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ServePage(string slug)
    {
        var page = await _landingPageService.GetBySlugAsync(slug);
        if (page == null)
        {
            return NotFound();
        }

        // Record visit
        var visit = new LandingPageVisit
        {
            LandingPageId = page.Id,
            VisitorId = Request.Cookies["visitor_id"] ?? Guid.NewGuid().ToString(),
            IpAddressHash = HashIpAddress(HttpContext.Connection.RemoteIpAddress?.ToString()),
            UserAgent = Request.Headers.UserAgent.ToString(),
            Referrer = Request.Headers.Referer.ToString(),
            UtmSource = Request.Query["utm_source"],
            UtmMedium = Request.Query["utm_medium"],
            UtmCampaign = Request.Query["utm_campaign"],
            UtmTerm = Request.Query["utm_term"],
            UtmContent = Request.Query["utm_content"],
            DeviceType = DetectDeviceType(Request.Headers.UserAgent.ToString()),
            Browser = DetectBrowser(Request.Headers.UserAgent.ToString()),
            OperatingSystem = DetectOS(Request.Headers.UserAgent.ToString()),
        };

        await _landingPageService.RecordVisitAsync(visit);

        // Set visitor cookie
        Response.Cookies.Append("visitor_id", visit.VisitorId!, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
        });

        // Return compiled HTML or regenerate if needed
        var html = !string.IsNullOrEmpty(page.HtmlContent)
            ? page.HtmlContent
            : await _landingPageService.CompileToHtmlAsync(page.Id);

        return Content(html, "text/html");
    }

    /// <summary>
    /// Record time on page (beacon endpoint).
    /// </summary>
    [HttpPost("{id}/time")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult RecordTimeOnPage(int id, [FromQuery] int seconds)
    {
        // This would update the visit record with time on page
        // For now, just acknowledge
        _logger.LogDebug("Time on page {Id}: {Seconds} seconds", id, seconds);
        return Ok();
    }

    // =============================
    // HELPER METHODS
    // =============================
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static LandingPageDto MapToDto(LandingPage page)
    {
        return new LandingPageDto
        {
            Id = page.Id,
            Name = page.Name,
            Slug = page.Slug,
            Title = page.Title,
            MetaDescription = page.MetaDescription,
            MetaKeywords = page.MetaKeywords,
            Status = page.Status.ToString(),
            Template = page.Template.ToString(),
            ContentJson = page.ContentJson,
            HtmlContent = page.HtmlContent,
            CustomCss = page.CustomCss,
            CustomJs = page.CustomJs,
            FeaturedImageUrl = page.FeaturedImageUrl,
            FacebookPixelId = page.FacebookPixelId,
            GoogleAnalyticsId = page.GoogleAnalyticsId,
            TrackingCode = page.TrackingCode,
            FormDefinitionId = page.FormDefinitionId,
            FormName = page.FormDefinition?.Name,
            CampaignId = page.CampaignId,
            CampaignName = page.Campaign?.Name,
            ThankYouPageId = page.ThankYouPageId,
            RedirectUrl = page.RedirectUrl,
            PublishedAt = page.PublishedAt,
            ScheduledPublishAt = page.ScheduledPublishAt,
            ScheduledUnpublishAt = page.ScheduledUnpublishAt,
            IsActive = page.IsActive,
            ABTestVariant = page.ABTestVariant,
            OriginalPageId = page.OriginalPageId,
            ABTestTrafficPercentage = page.ABTestTrafficPercentage,
            PageViews = page.PageViews,
            UniqueVisitors = page.UniqueVisitors,
            Conversions = page.Conversions,
            ConversionRate = page.ConversionRate,
            AverageTimeOnPage = page.AverageTimeOnPage,
            BounceRate = page.BounceRate,
            SettingsJson = page.SettingsJson,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt,
            CreatedByUserId = page.CreatedByUserId,
            CreatedByUserName = page.CreatedByUser?.Username,
            Blocks = page.Blocks?.Select(b => new LandingPageBlockDto
            {
                Id = b.Id,
                BlockType = b.BlockType.ToString(),
                SortOrder = b.SortOrder,
                ContentJson = b.ContentJson,
                StyleJson = b.StyleJson,
                VisibilityCondition = b.VisibilityCondition,
                IsVisible = b.IsVisible,
            }).ToList(),
        };
    }

    private static string? HashIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return null;
        }

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToBase64String(hash);
    }

    private static string DetectDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "unknown";
        }

        var ua = userAgent.ToLower();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
        {
            return "mobile";
        }

        if (ua.Contains("tablet") || ua.Contains("ipad"))
        {
            return "tablet";
        }

        return "desktop";
    }

    private static string DetectBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "unknown";
        }

        var ua = userAgent.ToLower();
        if (ua.Contains("edg/"))
        {
            return "Edge";
        }

        if (ua.Contains("chrome/") && !ua.Contains("edg/"))
        {
            return "Chrome";
        }

        if (ua.Contains("firefox/"))
        {
            return "Firefox";
        }

        if (ua.Contains("safari/") && !ua.Contains("chrome/"))
        {
            return "Safari";
        }

        return "Other";
    }

    private static string DetectOS(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "unknown";
        }

        var ua = userAgent.ToLower();
        if (ua.Contains("windows"))
        {
            return "Windows";
        }

        if (ua.Contains("mac os") || ua.Contains("macos"))
        {
            return "macOS";
        }

        if (ua.Contains("linux"))
        {
            return "Linux";
        }

        if (ua.Contains("android"))
        {
            return "Android";
        }

        if (ua.Contains("iphone") || ua.Contains("ipad"))
        {
            return "iOS";
        }

        return "Other";
    }
}

// =============================
// DTOs
// =============================
public class LandingPageListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int? FormDefinitionId { get; set; }
    public string? FormName { get; set; }
    public int PageViews { get; set; }
    public int UniqueVisitors { get; set; }
    public int Conversions { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserName { get; set; }
}

public class LandingPageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string? ContentJson { get; set; }
    public string? HtmlContent { get; set; }
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? FacebookPixelId { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? TrackingCode { get; set; }
    public int? FormDefinitionId { get; set; }
    public string? FormName { get; set; }
    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int? ThankYouPageId { get; set; }
    public string? RedirectUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? ScheduledUnpublishAt { get; set; }
    public bool IsActive { get; set; }
    public string? ABTestVariant { get; set; }
    public int? OriginalPageId { get; set; }
    public int? ABTestTrafficPercentage { get; set; }
    public int PageViews { get; set; }
    public int UniqueVisitors { get; set; }
    public int Conversions { get; set; }
    public decimal ConversionRate { get; set; }
    public double AverageTimeOnPage { get; set; }
    public decimal BounceRate { get; set; }
    public string? SettingsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public List<LandingPageBlockDto>? Blocks { get; set; }
}

public class LandingPageBlockDto
{
    public int Id { get; set; }
    public string BlockType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? ContentJson { get; set; }
    public string? StyleJson { get; set; }
    public string? VisibilityCondition { get; set; }
    public bool IsVisible { get; set; }
}

public class CreateLandingPageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Template { get; set; }
    public int? FormDefinitionId { get; set; }
    public int? CampaignId { get; set; }
    public int? ThankYouPageId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ContentJson { get; set; }
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
    public string? FacebookPixelId { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? TrackingCode { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? SettingsJson { get; set; }
}

public class UpdateLandingPageDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public int? FormDefinitionId { get; set; }
    public int? CampaignId { get; set; }
    public int? ThankYouPageId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ContentJson { get; set; }
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
    public string? FacebookPixelId { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? TrackingCode { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? ScheduledUnpublishAt { get; set; }
    public string? SettingsJson { get; set; }
}

public class UpdateLandingPageBlockDto
{
    public string BlockType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? ContentJson { get; set; }
    public string? StyleJson { get; set; }
    public string? VisibilityCondition { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class DuplicateLandingPageDto
{
    public string NewName { get; set; } = string.Empty;
}

public class SlugAvailabilityDto
{
    public string Slug { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string SuggestedSlug { get; set; } = string.Empty;
}

public class CreateVariantDto
{
    public string VariantName { get; set; } = string.Empty;
    public int TrafficPercentage { get; set; } = 50;
}
