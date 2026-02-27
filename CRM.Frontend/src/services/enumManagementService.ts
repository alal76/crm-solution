import apiClient from './apiClient';

// ─────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────

export interface LookupCategoryDto {
  id: number;
  name: string;
  description?: string | null;
  isActive: boolean;
  isSystemManaged: boolean;
  allowCustomValues: boolean;
  entityType?: string | null;
  propertyName?: string | null;
  validationSchema?: string | null;
  itemCount: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface LookupCategoryDetailDto extends LookupCategoryDto {
  items: LookupItemDto[];
}

export interface LookupItemDto {
  id: number;
  lookupCategoryId: number;
  key: string;
  value: string;
  meta?: string | null;
  sortOrder: number;
  isActive: boolean;
  isDefault: boolean;
  isSystemValue: boolean;
  color?: string | null;
  icon?: string | null;
  validationRules?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateLookupCategoryDto {
  name: string;
  description?: string;
  entityType?: string;
  propertyName?: string;
  isActive: boolean;
  allowCustomValues: boolean;
  validationSchema?: string;
}

export interface UpdateLookupCategoryDto {
  name: string;
  description?: string;
  entityType?: string;
  propertyName?: string;
  isActive: boolean;
  allowCustomValues: boolean;
  validationSchema?: string;
}

export interface CreateLookupItemDto {
  key: string;
  value: string;
  meta?: string;
  sortOrder: number;
  isActive: boolean;
  isDefault: boolean;
  color?: string;
  icon?: string;
  validationRules?: string;
}

export interface UpdateLookupItemDto {
  key: string;
  value: string;
  meta?: string;
  sortOrder: number;
  isActive: boolean;
  isDefault: boolean;
  color?: string;
  icon?: string;
  validationRules?: string;
}

// ─────────────────────────────────────────────────────────────────
// Categories
// ─────────────────────────────────────────────────────────────────

export const getCategories = (params?: { entityType?: string; includeInactive?: boolean }) =>
  apiClient.get<LookupCategoryDto[]>('/enum-management/categories', { params }).then(r => r.data);

export const getCategoryDetail = (id: number) =>
  apiClient.get<LookupCategoryDetailDto>(`/enum-management/categories/${id}`).then(r => r.data);

export const createCategory = (dto: CreateLookupCategoryDto) =>
  apiClient.post<LookupCategoryDto>('/enum-management/categories', dto).then(r => r.data);

export const updateCategory = (id: number, dto: UpdateLookupCategoryDto) =>
  apiClient.put<LookupCategoryDto>(`/enum-management/categories/${id}`, dto).then(r => r.data);

export const deleteCategory = (id: number) =>
  apiClient.delete(`/enum-management/categories/${id}`);

// ─────────────────────────────────────────────────────────────────
// Items
// ─────────────────────────────────────────────────────────────────

export const getItems = (categoryId: number, params?: { includeInactive?: boolean }) =>
  apiClient.get<LookupItemDto[]>(`/enum-management/categories/${categoryId}/items`, { params }).then(r => r.data);

export const createItem = (categoryId: number, dto: CreateLookupItemDto) =>
  apiClient.post<LookupItemDto>(`/enum-management/categories/${categoryId}/items`, dto).then(r => r.data);

export const updateItem = (id: number, dto: UpdateLookupItemDto) =>
  apiClient.put<LookupItemDto>(`/enum-management/items/${id}`, dto).then(r => r.data);

export const deleteItem = (id: number) =>
  apiClient.delete(`/enum-management/items/${id}`);

export const reorderItems = (categoryId: number, orderedIds: number[]) =>
  apiClient.post(`/enum-management/categories/${categoryId}/items/reorder`, { orderedIds });
