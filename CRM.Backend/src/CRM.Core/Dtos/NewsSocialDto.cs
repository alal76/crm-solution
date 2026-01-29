/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * News & Social Media Feed DTOs
 */

namespace CRM.Core.Dtos;

/// <summary>
/// News article DTO
/// </summary>
public class NewsItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Summary { get; set; }
    public string Sentiment { get; set; } = "neutral"; // positive, neutral, negative
}

/// <summary>
/// Social media post DTO
/// </summary>
public class SocialFeedDto
{
    public string Id { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // linkedin, twitter, facebook
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? AuthorHandle { get; set; }
    public string? AuthorImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Url { get; set; }
    public int EngagementCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public int CommentCount { get; set; }
}

/// <summary>
/// Combined news and social feed response
/// </summary>
public class NewsSocialFeedResponse
{
    public List<NewsItemDto> NewsItems { get; set; } = new();
    public List<SocialFeedDto> SocialFeeds { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
    public bool IsFromCache { get; set; }
}

/// <summary>
/// Request for fetching news/social feeds for a company
/// </summary>
public class NewsSocialFeedRequest
{
    public int CustomerId { get; set; }
    public string? CompanyName { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public bool RefreshCache { get; set; }
    public int MaxNewsItems { get; set; } = 10;
    public int MaxSocialItems { get; set; } = 10;
}
