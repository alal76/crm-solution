/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

import { useState, useCallback, useMemo } from 'react';

export interface PaginationState {
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface PaginationConfig {
  defaultPageSize?: number;
  pageSizeOptions?: number[];
}

export interface UsePaginationResult<T> {
  // Pagination state
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  
  // Computed values
  paginatedData: T[];
  startIndex: number;
  endIndex: number;
  
  // Actions
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  setTotalCount: (count: number) => void;
  handlePageChange: (event: unknown, newPage: number) => void;
  handlePageSizeChange: (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
  
  // Options
  pageSizeOptions: number[];
  
  // Reset
  reset: () => void;
}

const DEFAULT_PAGE_SIZE_OPTIONS = [10, 25, 50, 100];

/**
 * Hook for managing pagination state - for client-side pagination
 * Use this when you have all data loaded and want to paginate client-side
 */
export function usePagination<T>(
  data: T[],
  config: PaginationConfig = {}
): UsePaginationResult<T> {
  const {
    defaultPageSize = 25,
    pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS
  } = config;

  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(defaultPageSize);

  const totalCount = data.length;
  const totalPages = Math.ceil(totalCount / pageSize);

  // Calculate paginated data
  const paginatedData = useMemo(() => {
    const start = page * pageSize;
    return data.slice(start, start + pageSize);
  }, [data, page, pageSize]);

  const startIndex = page * pageSize;
  const endIndex = Math.min(startIndex + pageSize, totalCount);

  const handlePageChange = useCallback((_: unknown, newPage: number) => {
    setPage(newPage);
  }, []);

  const handlePageSizeChange = useCallback((event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const newSize = parseInt(event.target.value, 10);
    setPageSize(newSize);
    setPage(0); // Reset to first page when changing page size
  }, []);

  const reset = useCallback(() => {
    setPage(0);
    setPageSize(defaultPageSize);
  }, [defaultPageSize]);

  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    paginatedData,
    startIndex,
    endIndex,
    setPage,
    setPageSize,
    setTotalCount: () => {}, // No-op for client-side pagination
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
    reset
  };
}

/**
 * Hook for managing server-side pagination state
 * Use this when data is fetched from server with pagination params
 */
export function useServerPagination(
  config: PaginationConfig = {}
): Omit<UsePaginationResult<never>, 'paginatedData'> & {
  getPaginationParams: () => { page: number; pageSize: number };
} {
  const {
    defaultPageSize = 25,
    pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS
  } = config;

  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(defaultPageSize);
  const [totalCount, setTotalCount] = useState(0);

  const totalPages = Math.ceil(totalCount / pageSize);
  const startIndex = page * pageSize;
  const endIndex = Math.min(startIndex + pageSize, totalCount);

  const handlePageChange = useCallback((_: unknown, newPage: number) => {
    setPage(newPage);
  }, []);

  const handlePageSizeChange = useCallback((event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const newSize = parseInt(event.target.value, 10);
    setPageSize(newSize);
    setPage(0);
  }, []);

  const reset = useCallback(() => {
    setPage(0);
    setPageSize(defaultPageSize);
    setTotalCount(0);
  }, [defaultPageSize]);

  const getPaginationParams = useCallback(() => ({
    page: page + 1, // API typically uses 1-based pages
    pageSize
  }), [page, pageSize]);

  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    startIndex,
    endIndex,
    setPage,
    setPageSize,
    setTotalCount,
    handlePageChange,
    handlePageSizeChange,
    pageSizeOptions,
    reset,
    getPaginationParams
  };
}

export default usePagination;
