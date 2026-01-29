/**
 * Hook for handling concurrency conflicts when saving data.
 * Provides state management and handlers for conflict resolution.
 */

import { useState, useCallback } from 'react';
import { ConflictData } from '../components/common/ConcurrencyConflictDialog';

interface UseConcurrencyControlOptions<T> {
  /** Function to reload the latest data from server */
  onReload: () => Promise<T>;
  /** Function to force save, ignoring concurrency check */
  onForceSave: () => Promise<void>;
  /** Optional callback after successful reload */
  onReloadSuccess?: (data: T) => void;
  /** Optional callback after successful force save */
  onForceSaveSuccess?: () => void;
  /** Optional callback on error */
  onError?: (error: Error) => void;
}

interface UseConcurrencyControlResult {
  /** Whether the conflict dialog is open */
  conflictDialogOpen: boolean;
  /** The conflict data from the server */
  conflictData: ConflictData | undefined;
  /** Handler for when a 409 Conflict error is received */
  handleConflictError: (error: any, entityType: string, entityId: number) => void;
  /** Handler to close the dialog */
  handleCloseDialog: () => void;
  /** Handler when user chooses to reload */
  handleReload: () => Promise<void>;
  /** Handler when user chooses to overwrite */
  handleOverwrite: () => Promise<void>;
  /** Loading state for reload operation */
  isReloading: boolean;
  /** Loading state for overwrite operation */
  isOverwriting: boolean;
}

/**
 * Hook to manage concurrency conflict resolution in forms.
 * 
 * @example
 * ```tsx
 * const {
 *   conflictDialogOpen,
 *   conflictData,
 *   handleConflictError,
 *   handleCloseDialog,
 *   handleReload,
 *   handleOverwrite,
 * } = useConcurrencyControl({
 *   onReload: () => fetchCustomer(id),
 *   onForceSave: () => saveCustomer(data, { forceOverwrite: true }),
 *   onReloadSuccess: (data) => setFormData(data),
 * });
 * 
 * // In your save handler:
 * try {
 *   await saveCustomer(data);
 * } catch (error) {
 *   if (error.response?.status === 409) {
 *     handleConflictError(error, 'Customer', customerId);
 *   }
 * }
 * ```
 */
export function useConcurrencyControl<T = any>(
  options: UseConcurrencyControlOptions<T>
): UseConcurrencyControlResult {
  const [conflictDialogOpen, setConflictDialogOpen] = useState(false);
  const [conflictData, setConflictData] = useState<ConflictData | undefined>();
  const [isReloading, setIsReloading] = useState(false);
  const [isOverwriting, setIsOverwriting] = useState(false);

  const handleConflictError = useCallback(
    (error: any, entityType: string, entityId: number) => {
      // Extract conflict info from the error response
      const errorData = error.response?.data;
      
      setConflictData({
        entityType,
        entityId,
        modifiedBy: errorData?.modifiedBy,
        modifiedAt: errorData?.modifiedAt,
        serverVersion: errorData?.serverVersion,
        clientVersion: errorData?.clientVersion,
      });
      
      setConflictDialogOpen(true);
    },
    []
  );

  const handleCloseDialog = useCallback(() => {
    setConflictDialogOpen(false);
  }, []);

  const handleReload = useCallback(async () => {
    setIsReloading(true);
    try {
      const data = await options.onReload();
      setConflictDialogOpen(false);
      setConflictData(undefined);
      options.onReloadSuccess?.(data);
    } catch (error) {
      options.onError?.(error as Error);
    } finally {
      setIsReloading(false);
    }
  }, [options]);

  const handleOverwrite = useCallback(async () => {
    setIsOverwriting(true);
    try {
      await options.onForceSave();
      setConflictDialogOpen(false);
      setConflictData(undefined);
      options.onForceSaveSuccess?.();
    } catch (error) {
      options.onError?.(error as Error);
    } finally {
      setIsOverwriting(false);
    }
  }, [options]);

  return {
    conflictDialogOpen,
    conflictData,
    handleConflictError,
    handleCloseDialog,
    handleReload,
    handleOverwrite,
    isReloading,
    isOverwriting,
  };
}

export default useConcurrencyControl;
