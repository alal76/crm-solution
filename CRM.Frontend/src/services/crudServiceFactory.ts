// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

import apiClient from './apiClient';
import type { PaginatedResponse } from '../types/common';

// Re-export so consumers can import PaginatedResponse from this module too.
export type { PaginatedResponse };

/**
 * A fully-typed service object for standard REST CRUD operations.
 *
 * @template T         - The entity type returned by the API (e.g. Lead, Account).
 * @template CreateDto - Shape of the request body used for POST (defaults to Partial<T>).
 * @template UpdateDto - Shape of the request body used for PUT (defaults to Partial<T>).
 */
export interface CrudService<T, CreateDto = Partial<T>, UpdateDto = Partial<T>> {
  /**
   * GET  {basePath}?page=&pageSize=&...extraParams
   * Returns a paginated list of entities.
   */
  getAll(
    page?: number,
    pageSize?: number,
    params?: Record<string, unknown>
  ): Promise<PaginatedResponse<T>>;

  /** GET  {basePath}/{id} */
  getById(id: number): Promise<T>;

  /** POST {basePath} */
  create(dto: CreateDto): Promise<T>;

  /** PUT  {basePath}/{id} */
  update(id: number, dto: UpdateDto): Promise<T>;

  /** DELETE {basePath}/{id} — primary method name */
  // eslint-disable-next-line @typescript-eslint/method-signature-style
  delete(id: number): Promise<void>;

  /** DELETE {basePath}/{id} — alias kept for backward-compatibility */
  remove(id: number): Promise<void>;
}

/**
 * Generates a standard CRUD service for the given REST base path.
 *
 * Eliminates boilerplate across service files that only need the five standard
 * operations (list, get, create, update, delete).
 *
 * The `apiClient` instance already has `/api` baked into its base URL, so
 * `basePath` should NOT include `/api` (e.g. use `'/leads'`, not `'/api/leads'`).
 *
 * @example
 * // Minimal — all CRUD operations, default Partial<T> DTOs
 * const leadService = createCrudService<Lead>('/leads');
 *
 * @example
 * // With explicit DTO types
 * const policyService = createCrudService<
 *   EscalationPolicyDto,
 *   CreateEscalationPolicyDto,
 *   Partial<CreateEscalationPolicyDto>
 * >('/escalationpolicies');
 *
 * @example
 * // Extending with domain-specific methods via object spread
 * const slaService = {
 *   ...createCrudService<SLAPolicyDto, CreateSLAPolicyDto>('/slapolicies'),
 *   assignPolicy: async (policyId: number, srId: number) => { ... },
 * };
 */
export function createCrudService<
  T,
  CreateDto = Partial<T>,
  UpdateDto = Partial<T>,
>(basePath: string): CrudService<T, CreateDto, UpdateDto> {
  return {
    async getAll(page = 1, pageSize = 20, params = {}) {
      const response = await apiClient.get<PaginatedResponse<T>>(basePath, {
        params: { page, pageSize, ...params },
      });
      return response.data;
    },

    async getById(id: number) {
      const response = await apiClient.get<T>(`${basePath}/${id}`);
      return response.data;
    },

    async create(dto: CreateDto) {
      const response = await apiClient.post<T>(basePath, dto);
      return response.data;
    },

    async update(id: number, dto: UpdateDto) {
      const response = await apiClient.put<T>(`${basePath}/${id}`, dto);
      return response.data;
    },

    async remove(id: number) {
      await apiClient.delete(`${basePath}/${id}`);
    },

    // Alias so callers that use the conventional `.delete()` name still work.
    async delete(id: number) {
      await apiClient.delete(`${basePath}/${id}`);
    },
  };
}
