/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * News & Social Media Feed Service Interface
 */

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for fetching news and social media feeds for customers/companies
/// </summary>
public interface INewsSocialService
{
    /// <summary>
    /// Get news and social feeds for a customer
    /// </summary>
    Task<NewsSocialFeedResponse> GetFeedsAsync(NewsSocialFeedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get news articles for a company name
    /// </summary>
    Task<List<NewsItemDto>> GetNewsAsync(string companyName, int maxItems = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get social media posts for given handles/URLs
    /// </summary>
    Task<List<SocialFeedDto>> GetSocialFeedsAsync(
        string? linkedInUrl, 
        string? twitterHandle, 
        string? facebookUrl,
        int maxItems = 10, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if news API is configured
    /// </summary>
    bool IsNewsApiConfigured();

    /// <summary>
    /// Check if social media APIs are configured
    /// </summary>
    bool IsSocialApiConfigured();

    /// <summary>
    /// Analyze sentiment of text content using LLM
    /// </summary>
    Task<string> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default);
}
