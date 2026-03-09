/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */

import apiClient from './apiClient';

// ============================================================================
// Types
// ============================================================================

export enum ArticleType {
  HowTo = 0,
  FAQ = 1,
  Troubleshooting = 2,
  BestPractice = 3,
  Documentation = 4,
  Process = 5,
  Policy = 6,
  ReleaseNotes = 7,
  Video = 8,
  Template = 9,
}

export enum ArticleStatus {
  Draft = 0,
  InReview = 1,
  Published = 2,
  NeedsUpdate = 3,
  Archived = 4,
  Deprecated = 5,
}

export enum ArticleVisibility {
  Internal = 0,
  CustomerPortal = 1,
  Public = 2,
}

export interface KnowledgeBaseArticleDto {
  id: number;
  articleNumber: string;
  title: string;
  summary?: string;
  slug: string;
  content: string;
  contentFormat?: string;
  articleType: number;
  status: number;
  visibility: number;
  categoryId?: number;
  categoryName?: string;
  tags?: string;
  keywords?: string;
  authorUserId?: number;
  authorName?: string;
  viewCount: number;
  helpfulCount: number;
  notHelpfulCount: number;
  averageRating?: number;
  ratingCount: number;
  caseDeflectionCount: number;
  isFeatured: boolean;
  publishedAt?: string;
  expiresAt?: string;
  reviewDate?: string;
  version: number;
  languageCode?: string;
  productsJson?: string;
  relatedArticleIdsJson?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateKnowledgeBaseArticleDto {
  title: string;
  summary?: string;
  slug?: string;
  content: string;
  contentFormat?: string;
  articleType: ArticleType;
  status: ArticleStatus;
  visibility: ArticleVisibility;
  categoryId?: number | null;
  tags?: string;
  keywords?: string;
  isFeatured?: boolean;
  expiresAt?: string | null;
  reviewDate?: string | null;
  languageCode?: string;
  productsJson?: string;
}

export interface UpdateKnowledgeBaseArticleDto {
  title?: string;
  summary?: string;
  slug?: string;
  content?: string;
  contentFormat?: string;
  articleType?: ArticleType;
  status?: ArticleStatus;
  visibility?: ArticleVisibility;
  categoryId?: number | null;
  tags?: string;
  keywords?: string;
  isFeatured?: boolean;
  expiresAt?: string | null;
  reviewDate?: string | null;
  productsJson?: string;
}

export interface KnowledgeCategoryDto {
  id: number;
  name: string;
  description?: string;
  slug: string;
  icon?: string;
  displayOrder: number;
  isActive: boolean;
  parentId?: number;
  articleCount: number;
}

export interface CreateKnowledgeCategoryDto {
  name: string;
  description?: string;
  slug?: string;
  icon?: string;
  displayOrder?: number;
  isActive?: boolean;
  parentId?: number | null;
}

export interface UpdateKnowledgeCategoryDto {
  name?: string;
  description?: string;
  slug?: string;
  icon?: string;
  displayOrder?: number;
  isActive?: boolean;
  parentId?: number | null;
}

// KB-017: Tree DTO for KnowledgeCategoryManagementPage — built client-side from flat list
export interface KnowledgeCategoryTreeDto {
  id: number;
  name: string;
  slug: string;
  description?: string;
  icon?: string;
  parentId?: number;
  children: KnowledgeCategoryTreeDto[];
  articleCount: number;
  displayOrder: number;
  isActive: boolean;
}

export interface KnowledgeBaseFeedbackDto {
  isHelpful?: boolean;
  rating?: number;
  comment?: string;
  userId?: number;
}

export interface KnowledgeArticlePagedResult {
  items: KnowledgeBaseArticleDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface KnowledgeArticleFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number;
  status?: string;
}

// ============================================================================
// Service
// ============================================================================

/**
 * Knowledge Base service — article CRUD, categories, feedback, case deflection.
 * Routes: /knowledge/articles  and  /knowledge/categories
 */
export const knowledgeBaseService = {
  // --------------------------------------------------------------------------
  // Articles
  // --------------------------------------------------------------------------

  /** List / search articles (paginated). */
  getArticles: async (filters: KnowledgeArticleFilters = {}): Promise<KnowledgeArticlePagedResult> => {
    const params = new URLSearchParams();
    if (filters.page) params.set('page', String(filters.page));
    if (filters.pageSize) params.set('pageSize', String(filters.pageSize));
    if (filters.search) params.set('search', filters.search);
    if (filters.categoryId) params.set('categoryId', String(filters.categoryId));
    if (filters.status) params.set('status', filters.status);

    const query = params.toString() ? `?${params.toString()}` : '';
    const response = await apiClient.get<KnowledgeArticlePagedResult>(`/knowledge/articles${query}`);
    return response.data;
  },

  /** Get a single article by ID. */
  getArticleById: async (id: number): Promise<KnowledgeBaseArticleDto> => {
    const response = await apiClient.get<KnowledgeBaseArticleDto>(`/knowledge/articles/${id}`);
    return response.data;
  },

  /** Get a single article by URL slug. */
  getArticleBySlug: async (slug: string): Promise<KnowledgeBaseArticleDto> => {
    const response = await apiClient.get<KnowledgeBaseArticleDto>(`/knowledge/articles/slug/${slug}`);
    return response.data;
  },

  /** Create a new article. */
  createArticle: async (dto: CreateKnowledgeBaseArticleDto): Promise<KnowledgeBaseArticleDto> => {
    const response = await apiClient.post<KnowledgeBaseArticleDto>('/knowledge/articles', dto);
    return response.data;
  },

  /** Update an existing article. */
  updateArticle: async (id: number, dto: UpdateKnowledgeBaseArticleDto): Promise<KnowledgeBaseArticleDto> => {
    const response = await apiClient.put<KnowledgeBaseArticleDto>(`/knowledge/articles/${id}`, dto);
    return response.data;
  },

  /** Soft-delete an article. */
  deleteArticle: async (id: number): Promise<void> => {
    await apiClient.delete(`/knowledge/articles/${id}`);
  },

  /** Publish an article (Draft/InReview → Published). */
  publishArticle: async (id: number): Promise<KnowledgeBaseArticleDto> => {
    const response = await apiClient.patch<KnowledgeBaseArticleDto>(`/knowledge/articles/${id}/publish`);
    return response.data;
  },

  /** Archive an article (any status → Archived). */
  archiveArticle: async (id: number): Promise<KnowledgeBaseArticleDto> => {
    const response = await apiClient.patch<KnowledgeBaseArticleDto>(`/knowledge/articles/${id}/archive`);
    return response.data;
  },

  /** Submit user feedback on an article. */
  submitFeedback: async (id: number, feedback: KnowledgeBaseFeedbackDto): Promise<void> => {
    await apiClient.post(`/knowledge/articles/${id}/feedback`, feedback);
  },

  /** Get the most-viewed published articles. */
  getPopular: async (count = 10): Promise<KnowledgeBaseArticleDto[]> => {
    const response = await apiClient.get<KnowledgeBaseArticleDto[]>(
      `/knowledge/articles/popular?count=${count}`
    );
    return response.data;
  },

  /** Get the most recently published articles. */
  getRecent: async (count = 10): Promise<KnowledgeBaseArticleDto[]> => {
    const response = await apiClient.get<KnowledgeBaseArticleDto[]>(
      `/knowledge/articles/recent?count=${count}`
    );
    return response.data;
  },

  /** Get articles linked to a specific product. */
  getByProduct: async (productId: number): Promise<KnowledgeBaseArticleDto[]> => {
    const response = await apiClient.get<KnowledgeBaseArticleDto[]>(
      `/knowledge/articles/by-product/${productId}`
    );
    return response.data;
  },

  /** Record a case deflection event. */
  trackCaseDeflection: async (articleId: number, serviceRequestId?: number): Promise<void> => {
    const query = serviceRequestId !== undefined ? `?serviceRequestId=${serviceRequestId}` : '';
    await apiClient.post(`/knowledge/articles/${articleId}/case-deflection${query}`);
  },

  // --------------------------------------------------------------------------
  // Categories
  // --------------------------------------------------------------------------

  /** Get all active categories with article counts. */
  getCategories: async (): Promise<KnowledgeCategoryDto[]> => {
    const response = await apiClient.get<KnowledgeCategoryDto[]>('/knowledge/categories');
    return response.data;
  },

  /** Create a new category. */
  createCategory: async (dto: CreateKnowledgeCategoryDto): Promise<KnowledgeCategoryDto> => {
    const response = await apiClient.post<KnowledgeCategoryDto>('/knowledge/categories', dto);
    return response.data;
  },

  /** Update an existing category. */
  updateCategory: async (id: number, dto: UpdateKnowledgeCategoryDto): Promise<KnowledgeCategoryDto> => {
    const response = await apiClient.put<KnowledgeCategoryDto>(`/knowledge/categories/${id}`, dto);
    return response.data;
  },

  /** Soft-delete a category. */
  deleteCategory: async (id: number): Promise<void> => {
    await apiClient.delete(`/knowledge/categories/${id}`);
  },
};

export default knowledgeBaseService;
