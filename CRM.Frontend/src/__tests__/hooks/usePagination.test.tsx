import { renderHook, act } from '@testing-library/react';
import { usePagination, useServerPagination } from '../../hooks/usePagination';

describe('usePagination', () => {
  it('paginates client-side data', () => {
    const data = Array.from({ length: 30 }, (_, i) => `Item ${i + 1}`);

    const { result } = renderHook(() => usePagination(data, { defaultPageSize: 10 }));

    expect(result.current.totalCount).toBe(30);
    expect(result.current.totalPages).toBe(3);
    expect(result.current.paginatedData).toHaveLength(10);

    act(() => {
      result.current.handlePageChange({}, 1);
    });

    expect(result.current.page).toBe(1);
    expect(result.current.paginatedData[0]).toBe('Item 11');
  });
});

describe('useServerPagination', () => {
  it('returns API pagination params', () => {
    const { result } = renderHook(() => useServerPagination({ defaultPageSize: 20 }));

    expect(result.current.getPaginationParams()).toEqual({ page: 1, pageSize: 20 });

    act(() => {
      result.current.handlePageChange({}, 2);
    });

    expect(result.current.getPaginationParams()).toEqual({ page: 3, pageSize: 20 });
  });
});
