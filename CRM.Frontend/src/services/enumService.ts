/**
 * CRM Solution - Enum Management API Service
 * Phase 3: Frontend Implementation (SPEC-GEN-002)
 */

import apiClient from './apiClient';
import { EnumCategory, EnumValue, CreateEnumValueDto, UpdateEnumValueDto, EnumTransition, EnumValidationResult } from '../types/enums';

const BASE_PATH = '/enum-management';

export const enumService = {
  // Category methods
  getAllCategories: () => apiClient.get<EnumCategory[]>(`${BASE_PATH}/categories`),
  getCategoryByName: (categoryName: string) => apiClient.get<EnumCategory>(`${BASE_PATH}/categories/${categoryName}`),
  getCategoryByEntityType: (entityType: string, propertyName: string) =>
    apiClient.get<EnumCategory>(`${BASE_PATH}/categories/by-entity?entityType=${entityType}&propertyName=${propertyName}`),
  
  // Value methods
  getValuesByCategoryName: (categoryName: string, includeInactive = false) =>
    apiClient.get<EnumValue[]>(`${BASE_PATH}/categories/${categoryName}/values?includeInactive=${String(includeInactive)}`),
  getValuesByCategoryId: (categoryId: number, includeInactive = false) =>
    apiClient.get<EnumValue[]>(`${BASE_PATH}/categories/${categoryId}/values?includeInactive=${String(includeInactive)}`),
  getValueById: (valueId: number) => apiClient.get<EnumValue>(`${BASE_PATH}/values/${valueId}`),
  createValue: (categoryId: number, dto: CreateEnumValueDto) =>
    apiClient.post<EnumValue>(`${BASE_PATH}/categories/${categoryId}/values`, dto),
  updateValue: (valueId: number, dto: UpdateEnumValueDto) =>
    apiClient.put<EnumValue>(`${BASE_PATH}/values/${valueId}`, dto),
  deleteValue: (valueId: number) => apiClient.delete(`${BASE_PATH}/values/${valueId}`),
  reorderValues: (categoryId: number, newSortOrders: Record<number, number>) =>
    apiClient.post<EnumValue[]>(`${BASE_PATH}/categories/${categoryId}/values/reorder`, newSortOrders),
  
  // Transition methods
  getTransitionsByCategoryId: (categoryId: number) =>
    apiClient.get<EnumTransition[]>(`${BASE_PATH}/categories/${categoryId}/transitions`),
  validateTransition: (categoryId: number, fromValueId: number | undefined, toValueId: number, userRole?: string) =>
    apiClient.post<EnumValidationResult>(`${BASE_PATH}/categories/${categoryId}/transitions/validate`, { fromValueId, toValueId, userRole }),
};

export default enumService;
