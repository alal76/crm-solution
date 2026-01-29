/**
 * News and Social Media Feed Service
 * Provides API calls for fetching news and social media feeds for customers
 */

import apiClient from './apiClient';

// Types
export interface NewsItem {
  id: string;
  title: string;
  source: string;
  author?: string;
  url: string;
  imageUrl?: string;
  publishedAt: string;
  summary?: string;
  sentiment: 'positive' | 'neutral' | 'negative';
}

export interface SocialFeed {
  id: string;
  platform: 'linkedin' | 'twitter' | 'facebook';
  content: string;
  author: string;
  authorHandle?: string;
  authorImageUrl?: string;
  publishedAt: string;
  url?: string;
  engagementCount: number;
  likeCount: number;
  shareCount: number;
  commentCount: number;
}

export interface NewsSocialFeedResponse {
  newsItems: NewsItem[];
  socialFeeds: SocialFeed[];
  lastUpdated: string;
  error?: string;
  isFromCache: boolean;
}

export interface NewsSocialStatus {
  newsApiConfigured: boolean;
  socialApiConfigured: boolean;
  message: string;
}

/**
 * Get API configuration status
 */
export const getNewsSocialStatus = async (): Promise<NewsSocialStatus> => {
  const response = await apiClient.get<NewsSocialStatus>('/news-social/status');
  return response.data;
};

/**
 * Get news and social feeds for a customer
 */
export const getNewsSocialFeeds = async (
  customerId: number,
  maxNewsItems: number = 10,
  maxSocialItems: number = 10
): Promise<NewsSocialFeedResponse> => {
  const response = await apiClient.get<NewsSocialFeedResponse>(
    `/news-social/${customerId}`,
    {
      params: { maxNewsItems, maxSocialItems }
    }
  );
  return response.data;
};

/**
 * Force refresh feeds for a customer (bypass cache)
 */
export const refreshNewsSocialFeeds = async (
  customerId: number,
  maxNewsItems: number = 10,
  maxSocialItems: number = 10
): Promise<NewsSocialFeedResponse> => {
  const response = await apiClient.post<NewsSocialFeedResponse>(
    `/news-social/refresh/${customerId}`,
    null,
    {
      params: { maxNewsItems, maxSocialItems }
    }
  );
  return response.data;
};

/**
 * Get news for a specific company name
 */
export const getNewsForCompany = async (
  companyName: string,
  maxItems: number = 10
): Promise<NewsItem[]> => {
  const response = await apiClient.get<NewsItem[]>('/news-social/news', {
    params: { companyName, maxItems }
  });
  return response.data;
};

/**
 * Get social feeds for given handles/URLs
 */
export const getSocialFeeds = async (
  linkedInUrl?: string,
  twitterHandle?: string,
  facebookUrl?: string,
  maxItems: number = 10
): Promise<SocialFeed[]> => {
  const response = await apiClient.get<SocialFeed[]>('/news-social/social', {
    params: { linkedInUrl, twitterHandle, facebookUrl, maxItems }
  });
  return response.data;
};

/**
 * Analyze sentiment of text
 */
export const analyzeSentiment = async (text: string): Promise<string> => {
  const response = await apiClient.post<{ sentiment: string }>('/news-social/sentiment', { text });
  return response.data.sentiment;
};

export default {
  getNewsSocialStatus,
  getNewsSocialFeeds,
  refreshNewsSocialFeeds,
  getNewsForCompany,
  getSocialFeeds,
  analyzeSentiment
};
