// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

import { useState, useCallback } from 'react';

/**
 * Lightweight loading + error state manager for async operations.
 *
 * Compared to the more feature-rich `useApiState` hook (which supports success
 * messages and auto-clear timeouts), `useLoadingState` is intentionally minimal:
 * it wraps any async function and tracks only the loading flag and the last
 * error string. Use it in components that only need a spinner and an inline
 * error message.
 *
 * @template T - The resolved value type of the wrapped async function.
 *
 * @example
 * const { loading, error, execute } = useLoadingState(
 *   () => leadService.getAll()
 * );
 *
 * // In JSX:
 * <Button onClick={execute} disabled={loading}>
 *   {loading ? <CircularProgress size={16} /> : 'Load'}
 * </Button>
 * {error && <Alert severity="error">{error}</Alert>}
 */
export function useLoadingState<T = void>(
  asyncFn: (...args: unknown[]) => Promise<T>
) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const execute = useCallback(
    async (...args: unknown[]): Promise<T | undefined> => {
      setLoading(true);
      setError(null);
      try {
        const result = await asyncFn(...args);
        return result;
      } catch (err) {
        const message =
          err instanceof Error ? err.message : 'An unexpected error occurred';
        setError(message);
        return undefined;
      } finally {
        setLoading(false);
      }
    },
    // asyncFn is intentionally excluded — callers pass a stable reference or
    // an inline arrow that references stable service methods.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  /**
   * Manually reset the error state (e.g. when the user dismisses an alert).
   */
  const clearError = useCallback(() => setError(null), []);

  return { loading, error, execute, setError, clearError };
}
