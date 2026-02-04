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

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing landing pages.
/// Part of Marketing and Sales gap analysis implementation (G6).
/// </summary>
public interface ILandingPageService
{
    /// <summary>
    /// Get all landing pages with optional filtering.
    /// </summary>
    Task<IEnumerable<LandingPage>> GetAllAsync(int? campaignId = null, LandingPageStatus? status = null);

    /// <summary>
    /// Get a landing page by ID.
    /// </summary>
    Task<LandingPage?> GetByIdAsync(int id);

    /// <summary>
    /// Get a landing page by slug (for public rendering).
    /// </summary>
    Task<LandingPage?> GetBySlugAsync(string slug);

    /// <summary>
    /// Create a new landing page.
    /// </summary>
    Task<LandingPage> CreateAsync(LandingPage landingPage, int userId);

    /// <summary>
    /// Update an existing landing page.
    /// </summary>
    Task<LandingPage> UpdateAsync(LandingPage landingPage);

    /// <summary>
    /// Delete a landing page (soft delete).
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Publish a landing page.
    /// </summary>
    Task<LandingPage> PublishAsync(int id);

    /// <summary>
    /// Unpublish a landing page (return to draft).
    /// </summary>
    Task<LandingPage> UnpublishAsync(int id);

    /// <summary>
    /// Duplicate a landing page.
    /// </summary>
    Task<LandingPage> DuplicateAsync(int id, string newName, int userId);

    /// <summary>
    /// Get blocks for a landing page.
    /// </summary>
    Task<IEnumerable<LandingPageBlock>> GetBlocksAsync(int landingPageId);

    /// <summary>
    /// Update blocks for a landing page.
    /// </summary>
    Task<IEnumerable<LandingPageBlock>> UpdateBlocksAsync(int landingPageId, IEnumerable<LandingPageBlock> blocks);

    /// <summary>
    /// Record a page visit for analytics.
    /// </summary>
    Task<LandingPageVisit> RecordVisitAsync(LandingPageVisit visit);

    /// <summary>
    /// Record a conversion (form submission).
    /// </summary>
    Task<bool> RecordConversionAsync(int landingPageId, string? visitorId, int? leadId);

    /// <summary>
    /// Get analytics for a landing page.
    /// </summary>
    Task<LandingPageAnalytics> GetAnalyticsAsync(int landingPageId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Compile page content to HTML.
    /// </summary>
    Task<string> CompileToHtmlAsync(int landingPageId);

    /// <summary>
    /// Check if a slug is available.
    /// </summary>
    Task<bool> IsSlugAvailableAsync(string slug, int? excludeId = null);

    /// <summary>
    /// Generate a unique slug from a name.
    /// </summary>
    Task<string> GenerateSlugAsync(string name);

    /// <summary>
    /// Create an A/B test variant.
    /// </summary>
    Task<LandingPage> CreateVariantAsync(int originalPageId, string variantName, int trafficPercentage, int userId);
}

/// <summary>
/// Landing page analytics DTO.
/// </summary>
public class LandingPageAnalytics
{
    /// <summary>
    /// Total page views.
    /// </summary>
    public int TotalPageViews { get; set; }

    /// <summary>
    /// Unique visitors.
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// Total conversions.
    /// </summary>
    public int Conversions { get; set; }

    /// <summary>
    /// Conversion rate percentage.
    /// </summary>
    public decimal ConversionRate { get; set; }

    /// <summary>
    /// Average time on page in seconds.
    /// </summary>
    public double AverageTimeOnPage { get; set; }

    /// <summary>
    /// Bounce rate percentage.
    /// </summary>
    public decimal BounceRate { get; set; }

    /// <summary>
    /// Views by date.
    /// </summary>
    public Dictionary<DateTime, int> ViewsByDate { get; set; } = new();

    /// <summary>
    /// Conversions by date.
    /// </summary>
    public Dictionary<DateTime, int> ConversionsByDate { get; set; } = new();

    /// <summary>
    /// Views by device type.
    /// </summary>
    public Dictionary<string, int> ViewsByDevice { get; set; } = new();

    /// <summary>
    /// Top referrers.
    /// </summary>
    public Dictionary<string, int> TopReferrers { get; set; } = new();

    /// <summary>
    /// Views by country.
    /// </summary>
    public Dictionary<string, int> ViewsByCountry { get; set; } = new();

    /// <summary>
    /// UTM source breakdown.
    /// </summary>
    public Dictionary<string, int> UtmSources { get; set; } = new();
}
