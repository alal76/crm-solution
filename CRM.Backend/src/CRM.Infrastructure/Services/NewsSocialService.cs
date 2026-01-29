/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * News & Social Media Feed Service Implementation
 * Integrates with NewsAPI.org, Twitter/X API, LinkedIn API for real-time feeds
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Configuration for external news and social APIs
/// </summary>
public class NewsSocialOptions
{
    public NewsApiOptions NewsApi { get; set; } = new();
    public TwitterApiOptions Twitter { get; set; } = new();
    public LinkedInApiOptions LinkedIn { get; set; } = new();
    public bool EnableSentimentAnalysis { get; set; } = true;
    public int CacheMinutes { get; set; } = 15;
}

public class NewsApiOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://newsapi.org/v2";
    public bool Enabled { get; set; } = true;
}

public class TwitterApiOptions
{
    public string BearerToken { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.twitter.com/2";
    public bool Enabled { get; set; } = true;
}

public class LinkedInApiOptions
{
    public string AccessToken { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.linkedin.com/v2";
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// News and Social Media Feed Service
/// </summary>
public class NewsSocialService : INewsSocialService
{
    private readonly ILogger<NewsSocialService> _logger;
    private readonly NewsSocialOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache? _cache;
    private readonly ILLMService? _llmService;
    private readonly ICustomerService _customerService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NewsSocialService(
        ILogger<NewsSocialService> logger,
        IOptions<NewsSocialOptions> options,
        ICustomerService customerService,
        HttpClient? httpClient = null,
        IDistributedCache? cache = null,
        ILLMService? llmService = null)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _cache = cache;
        _llmService = llmService;
        _customerService = customerService;
    }

    private static bool IsValidApiKey(string? key) =>
        !string.IsNullOrEmpty(key) && 
        !key.StartsWith("${") && 
        key.Length > 10;

    public bool IsNewsApiConfigured() => IsValidApiKey(_options.NewsApi.ApiKey) && _options.NewsApi.Enabled;

    public bool IsSocialApiConfigured() =>
        (IsValidApiKey(_options.Twitter.BearerToken) && _options.Twitter.Enabled) ||
        (IsValidApiKey(_options.LinkedIn.AccessToken) && _options.LinkedIn.Enabled);

    public async Task<NewsSocialFeedResponse> GetFeedsAsync(NewsSocialFeedRequest request, CancellationToken cancellationToken = default)
    {
        var response = new NewsSocialFeedResponse();

        try
        {
            // Get company info from customer if not provided
            string? companyName = request.CompanyName;
            string? linkedInUrl = request.LinkedInUrl;
            string? twitterHandle = request.TwitterHandle;
            string? facebookUrl = request.FacebookUrl;

            if (request.CustomerId > 0 && string.IsNullOrEmpty(companyName))
            {
                var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
                if (customer != null)
                {
                    companyName = customer.Company;
                    linkedInUrl ??= customer.LinkedInUrl;
                    twitterHandle ??= customer.TwitterHandle;
                    facebookUrl ??= customer.FacebookUrl;
                }
            }

            // Check cache first (with exception handling for Redis connectivity issues)
            if (!request.RefreshCache && _cache != null)
            {
                try
                {
                    var cacheKey = $"news-social:{request.CustomerId}:{companyName}";
                    var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
                    if (!string.IsNullOrEmpty(cached))
                    {
                        var cachedResponse = JsonSerializer.Deserialize<NewsSocialFeedResponse>(cached, JsonOptions);
                        if (cachedResponse != null)
                        {
                            cachedResponse.IsFromCache = true;
                            return cachedResponse;
                        }
                    }
                }
                catch (Exception cacheEx)
                {
                    _logger.LogDebug(cacheEx, "Cache read failed, continuing without cache");
                }
            }

            // Fetch news and social feeds in parallel
            var newsTask = !string.IsNullOrEmpty(companyName)
                ? GetNewsAsync(companyName, request.MaxNewsItems, cancellationToken)
                : Task.FromResult(new List<NewsItemDto>());

            var socialTask = GetSocialFeedsAsync(
                linkedInUrl,
                twitterHandle,
                facebookUrl,
                request.MaxSocialItems,
                cancellationToken);

            await Task.WhenAll(newsTask, socialTask);

            response.NewsItems = await newsTask;
            response.SocialFeeds = await socialTask;
            response.LastUpdated = DateTime.UtcNow;

            // Cache the result (with exception handling)
            if (_cache != null && (response.NewsItems.Any() || response.SocialFeeds.Any()))
            {
                try
                {
                    var cacheKey = $"news-social:{request.CustomerId}:{companyName}";
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(response, JsonOptions),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes)
                        },
                        cancellationToken);
                }
                catch (Exception cacheEx)
                {
                    _logger.LogDebug(cacheEx, "Cache write failed, continuing without caching");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news/social feeds for customer {CustomerId}", request.CustomerId);
            response.Error = "Failed to fetch feeds. Please check API configuration.";
        }

        return response;
    }

    public async Task<List<NewsItemDto>> GetNewsAsync(string companyName, int maxItems = 10, CancellationToken cancellationToken = default)
    {
        var news = new List<NewsItemDto>();

        if (!IsNewsApiConfigured())
        {
            _logger.LogWarning("NewsAPI is not configured. Returning empty news list.");
            return news;
        }

        try
        {
            // Search for news about the company
            var encodedQuery = HttpUtility.UrlEncode(companyName);
            var url = $"{_options.NewsApi.BaseUrl}/everything?q={encodedQuery}&language=en&sortBy=publishedAt&pageSize={maxItems}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Api-Key", _options.NewsApi.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, JsonOptions);

                if (newsResponse?.Articles != null)
                {
                    var sentimentTasks = new List<Task<string>>();
                    var articles = newsResponse.Articles.Take(maxItems).ToList();

                    foreach (var article in articles)
                    {
                        var newsItem = new NewsItemDto
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = article.Title ?? "",
                            Source = article.Source?.Name ?? "Unknown",
                            Author = article.Author,
                            Url = article.Url ?? "",
                            ImageUrl = article.UrlToImage,
                            PublishedAt = article.PublishedAt ?? DateTime.UtcNow,
                            Summary = article.Description,
                            Sentiment = "neutral"
                        };
                        news.Add(newsItem);
                    }

                    // Analyze sentiment for news items if enabled
                    if (_options.EnableSentimentAnalysis && _llmService != null)
                    {
                        await AnalyzeSentimentForNewsAsync(news, cancellationToken);
                    }
                }
            }
            else
            {
                _logger.LogWarning("NewsAPI returned {StatusCode} for query: {Query}", response.StatusCode, companyName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news for company: {CompanyName}", companyName);
        }

        return news;
    }

    public async Task<List<SocialFeedDto>> GetSocialFeedsAsync(
        string? linkedInUrl,
        string? twitterHandle,
        string? facebookUrl,
        int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        var feeds = new List<SocialFeedDto>();

        // Fetch Twitter feeds
        if (!string.IsNullOrEmpty(twitterHandle))
        {
            var twitterFeeds = await GetTwitterFeedsAsync(twitterHandle, maxItems, cancellationToken);
            feeds.AddRange(twitterFeeds);
        }

        // Fetch LinkedIn feeds
        if (!string.IsNullOrEmpty(linkedInUrl))
        {
            var linkedInFeeds = await GetLinkedInFeedsAsync(linkedInUrl, maxItems, cancellationToken);
            feeds.AddRange(linkedInFeeds);
        }

        // Sort by date
        return feeds.OrderByDescending(f => f.PublishedAt).Take(maxItems).ToList();
    }

    private async Task<List<SocialFeedDto>> GetTwitterFeedsAsync(string handle, int maxItems, CancellationToken cancellationToken)
    {
        var feeds = new List<SocialFeedDto>();

        if (string.IsNullOrEmpty(_options.Twitter.BearerToken) || !_options.Twitter.Enabled)
        {
            _logger.LogDebug("Twitter API is not configured");
            return feeds;
        }

        try
        {
            // Clean the handle
            var cleanHandle = handle.TrimStart('@');

            // Get user ID first
            var userUrl = $"{_options.Twitter.BaseUrl}/users/by/username/{cleanHandle}";
            var userRequest = new HttpRequestMessage(HttpMethod.Get, userUrl);
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Twitter.BearerToken);

            var userResponse = await _httpClient.SendAsync(userRequest, cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twitter API user lookup failed for handle: {Handle}", cleanHandle);
                return feeds;
            }

            var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            var userData = JsonSerializer.Deserialize<TwitterUserResponse>(userContent, JsonOptions);

            if (userData?.Data == null)
            {
                return feeds;
            }

            // Get user's tweets
            var tweetsUrl = $"{_options.Twitter.BaseUrl}/users/{userData.Data.Id}/tweets?max_results={Math.Min(maxItems, 100)}&tweet.fields=created_at,public_metrics&expansions=author_id";
            var tweetsRequest = new HttpRequestMessage(HttpMethod.Get, tweetsUrl);
            tweetsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Twitter.BearerToken);

            var tweetsResponse = await _httpClient.SendAsync(tweetsRequest, cancellationToken);
            if (!tweetsResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twitter API tweets fetch failed for user: {UserId}", userData.Data.Id);
                return feeds;
            }

            var tweetsContent = await tweetsResponse.Content.ReadAsStringAsync(cancellationToken);
            var tweetsData = JsonSerializer.Deserialize<TwitterTweetsResponse>(tweetsContent, JsonOptions);

            if (tweetsData?.Data != null)
            {
                foreach (var tweet in tweetsData.Data)
                {
                    feeds.Add(new SocialFeedDto
                    {
                        Id = tweet.Id,
                        Platform = "twitter",
                        Content = tweet.Text,
                        Author = userData.Data.Name ?? cleanHandle,
                        AuthorHandle = $"@{cleanHandle}",
                        PublishedAt = tweet.CreatedAt ?? DateTime.UtcNow,
                        Url = $"https://twitter.com/{cleanHandle}/status/{tweet.Id}",
                        LikeCount = tweet.PublicMetrics?.LikeCount ?? 0,
                        ShareCount = tweet.PublicMetrics?.RetweetCount ?? 0,
                        CommentCount = tweet.PublicMetrics?.ReplyCount ?? 0,
                        EngagementCount = (tweet.PublicMetrics?.LikeCount ?? 0) +
                                          (tweet.PublicMetrics?.RetweetCount ?? 0) +
                                          (tweet.PublicMetrics?.ReplyCount ?? 0)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Twitter feeds for handle: {Handle}", handle);
        }

        return feeds;
    }

    private async Task<List<SocialFeedDto>> GetLinkedInFeedsAsync(string linkedInUrl, int maxItems, CancellationToken cancellationToken)
    {
        var feeds = new List<SocialFeedDto>();

        if (string.IsNullOrEmpty(_options.LinkedIn.AccessToken) || !_options.LinkedIn.Enabled)
        {
            _logger.LogDebug("LinkedIn API is not configured");
            return feeds;
        }

        try
        {
            // LinkedIn API requires organization ID or person URN
            // For company pages, we would need to use the Marketing API
            // This is a simplified implementation that would need OAuth2 flow

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.LinkedIn.BaseUrl}/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.LinkedIn.AccessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // LinkedIn's API is complex and requires specific permissions
                // This would need to be expanded based on actual use case
                _logger.LogInformation("LinkedIn API connected successfully");
            }
            else
            {
                _logger.LogWarning("LinkedIn API returned {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching LinkedIn feeds for URL: {Url}", linkedInUrl);
        }

        return feeds;
    }

    private async Task AnalyzeSentimentForNewsAsync(List<NewsItemDto> newsItems, CancellationToken cancellationToken)
    {
        if (_llmService == null || !newsItems.Any())
        {
            return;
        }

        try
        {
            // Batch sentiment analysis
            var titles = newsItems.Select(n => n.Title).ToList();
            var prompt = $@"Analyze the sentiment of each news headline below. Return only a JSON array with sentiment values (positive, neutral, or negative) in the same order as the headlines.

Headlines:
{string.Join("\n", titles.Select((t, i) => $"{i + 1}. {t}"))}

Return format: [""positive"", ""neutral"", ""negative"", ...]";

            var llmRequest = new LLMRequest
            {
                Prompt = prompt,
                MaxTokens = 500,
                Temperature = 0.1,
                JsonMode = true
            };

            var response = await _llmService.CompletionAsync(llmRequest, cancellationToken);

            if (response.Success && !string.IsNullOrEmpty(response.Content))
            {
                var sentiments = JsonSerializer.Deserialize<List<string>>(response.Content, JsonOptions);
                if (sentiments != null)
                {
                    for (int i = 0; i < Math.Min(sentiments.Count, newsItems.Count); i++)
                    {
                        newsItems[i].Sentiment = sentiments[i].ToLower();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for news items");
        }
    }

    public async Task<string> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default)
    {
        if (_llmService == null || string.IsNullOrEmpty(text))
        {
            return "neutral";
        }

        try
        {
            var llmRequest = new LLMRequest
            {
                Prompt = $"Analyze the sentiment of this text and respond with only one word: positive, neutral, or negative.\n\nText: {text}",
                MaxTokens = 10,
                Temperature = 0.1
            };

            var response = await _llmService.CompletionAsync(llmRequest, cancellationToken);

            if (response.Success && !string.IsNullOrEmpty(response.Content))
            {
                var sentiment = response.Content.Trim().ToLower();
                if (sentiment.Contains("positive")) return "positive";
                if (sentiment.Contains("negative")) return "negative";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for text");
        }

        return "neutral";
    }
}

#region API Response Models

internal class NewsApiResponse
{
    public string? Status { get; set; }
    public int TotalResults { get; set; }
    public List<NewsApiArticle>? Articles { get; set; }
}

internal class NewsApiArticle
{
    public NewsApiSource? Source { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? UrlToImage { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Content { get; set; }
}

internal class NewsApiSource
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

internal class TwitterUserResponse
{
    public TwitterUserData? Data { get; set; }
}

internal class TwitterUserData
{
    public string Id { get; set; } = "";
    public string? Name { get; set; }
    public string? Username { get; set; }
}

internal class TwitterTweetsResponse
{
    public List<TwitterTweet>? Data { get; set; }
}

internal class TwitterTweet
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    [JsonPropertyName("public_metrics")]
    public TwitterPublicMetrics? PublicMetrics { get; set; }
}

internal class TwitterPublicMetrics
{
    [JsonPropertyName("like_count")]
    public int LikeCount { get; set; }
    [JsonPropertyName("retweet_count")]
    public int RetweetCount { get; set; }
    [JsonPropertyName("reply_count")]
    public int ReplyCount { get; set; }
    [JsonPropertyName("quote_count")]
    public int QuoteCount { get; set; }
}

#endregion
