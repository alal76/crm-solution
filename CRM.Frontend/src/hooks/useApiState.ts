import { useState, useCallback, useRef } from 'react';

/**
 * Error details for API calls
 */
export interface ApiError {
  message: string;
  code?: string;
  details?: Record<string, string[]>;
  status?: number;
}

/**
 * State for API operations with loading, error, and success tracking
 */
export interface ApiState<T = any> {
  data: T | null;
  loading: boolean;
  error: ApiError | null;
  success: string | null;
}

/**
 * Options for API state hook
 */
export interface UseApiStateOptions {
  /** Auto-clear success message after this many milliseconds (default: 3000, 0 = never) */
  successTimeout?: number;
  /** Auto-clear error message after this many milliseconds (default: 0 = never) */
  errorTimeout?: number;
  /** Log errors to console (default: true) */
  logErrors?: boolean;
}

const defaultOptions: UseApiStateOptions = {
  successTimeout: 3000,
  errorTimeout: 0,
  logErrors: true,
};

/**
 * Hook for managing API state with loading, error, and success handling.
 * Provides consistent error handling across the application.
 * 
 * @example
 * const { loading, error, success, execute, clearError, clearSuccess } = useApiState();
 * 
 * const handleSave = async () => {
 *   const result = await execute(
 *     async () => {
 *       const response = await apiClient.post('/items', data);
 *       return response.data;
 *     },
 *     'Item saved successfully'
 *   );
 *   if (result) {
 *     // Success - close dialog
 *   }
 *   // If error, dialog stays open with error displayed
 * };
 */
export function useApiState<T = any>(options: UseApiStateOptions = {}) {
  const opts = { ...defaultOptions, ...options };
  
  const [state, setState] = useState<ApiState<T>>({
    data: null,
    loading: false,
    error: null,
    success: null,
  });
  
  const successTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const errorTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  
  /**
   * Clear any pending timeouts
   */
  const clearTimeouts = useCallback(() => {
    if (successTimeoutRef.current) {
      clearTimeout(successTimeoutRef.current);
      successTimeoutRef.current = null;
    }
    if (errorTimeoutRef.current) {
      clearTimeout(errorTimeoutRef.current);
      errorTimeoutRef.current = null;
    }
  }, []);
  
  /**
   * Parse error from various response formats
   */
  const parseError = useCallback((err: any): ApiError => {
    // Axios error with response
    if (err.response) {
      const data = err.response.data;
      return {
        message: data?.message || data?.title || data?.error || 'An error occurred',
        code: data?.code,
        details: data?.errors || data?.details,
        status: err.response.status,
      };
    }
    
    // Fetch error with JSON body
    if (err.json) {
      return {
        message: err.json.message || err.json.title || 'An error occurred',
        code: err.json.code,
        details: err.json.errors || err.json.details,
        status: err.status,
      };
    }
    
    // Plain error object
    if (err.message) {
      return {
        message: err.message,
        code: err.code,
        status: err.status,
      };
    }
    
    // String error
    if (typeof err === 'string') {
      return { message: err };
    }
    
    return { message: 'An unexpected error occurred' };
  }, []);
  
  /**
   * Set loading state
   */
  const setLoading = useCallback((loading: boolean) => {
    setState(prev => ({ ...prev, loading }));
  }, []);
  
  /**
   * Clear error state
   */
  const clearError = useCallback(() => {
    if (errorTimeoutRef.current) {
      clearTimeout(errorTimeoutRef.current);
      errorTimeoutRef.current = null;
    }
    setState(prev => ({ ...prev, error: null }));
  }, []);
  
  /**
   * Clear success state
   */
  const clearSuccess = useCallback(() => {
    if (successTimeoutRef.current) {
      clearTimeout(successTimeoutRef.current);
      successTimeoutRef.current = null;
    }
    setState(prev => ({ ...prev, success: null }));
  }, []);
  
  /**
   * Set error state
   */
  const setError = useCallback((error: string | ApiError | null) => {
    clearTimeouts();
    
    const errorObj = error 
      ? (typeof error === 'string' ? { message: error } : error)
      : null;
    
    setState(prev => ({ ...prev, error: errorObj, loading: false }));
    
    if (errorObj && opts.logErrors) {
      console.error('[API Error]', errorObj);
    }
    
    if (errorObj && opts.errorTimeout && opts.errorTimeout > 0) {
      errorTimeoutRef.current = setTimeout(() => {
        setState(prev => ({ ...prev, error: null }));
      }, opts.errorTimeout);
    }
  }, [clearTimeouts, opts.errorTimeout, opts.logErrors]);
  
  /**
   * Set success state
   */
  const setSuccess = useCallback((message: string | null) => {
    clearTimeouts();
    
    setState(prev => ({ ...prev, success: message, loading: false }));
    
    if (message && opts.successTimeout && opts.successTimeout > 0) {
      successTimeoutRef.current = setTimeout(() => {
        setState(prev => ({ ...prev, success: null }));
      }, opts.successTimeout);
    }
  }, [clearTimeouts, opts.successTimeout]);
  
  /**
   * Execute an async operation with automatic loading, error, and success handling.
   * Returns the result on success, or null on error.
   * 
   * @param operation - The async operation to execute
   * @param successMessage - Message to display on success (optional)
   * @returns The result of the operation, or null if an error occurred
   */
  const execute = useCallback(async <R = T>(
    operation: () => Promise<R>,
    successMessage?: string
  ): Promise<R | null> => {
    clearTimeouts();
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const result = await operation();
      
      setState(prev => ({
        ...prev,
        data: result as any,
        loading: false,
        error: null,
        success: successMessage || null,
      }));
      
      if (successMessage && opts.successTimeout && opts.successTimeout > 0) {
        successTimeoutRef.current = setTimeout(() => {
          setState(prev => ({ ...prev, success: null }));
        }, opts.successTimeout);
      }
      
      return result;
    } catch (err: any) {
      const errorObj = parseError(err);
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: errorObj,
      }));
      
      if (opts.logErrors) {
        console.error('[API Error]', err);
      }
      
      if (opts.errorTimeout && opts.errorTimeout > 0) {
        errorTimeoutRef.current = setTimeout(() => {
          setState(prev => ({ ...prev, error: null }));
        }, opts.errorTimeout);
      }
      
      return null;
    }
  }, [clearTimeouts, parseError, opts.successTimeout, opts.errorTimeout, opts.logErrors]);
  
  /**
   * Reset all state
   */
  const reset = useCallback(() => {
    clearTimeouts();
    setState({
      data: null,
      loading: false,
      error: null,
      success: null,
    });
  }, [clearTimeouts]);
  
  return {
    ...state,
    setLoading,
    setError,
    setSuccess,
    clearError,
    clearSuccess,
    execute,
    reset,
  };
}

/**
 * Hook for managing dialog state with error handling.
 * Keeps dialog open when errors occur.
 */
export function useDialogState() {
  const [open, setOpen] = useState(false);
  const [confirmClose, setConfirmClose] = useState(false);
  
  const handleOpen = useCallback(() => {
    setOpen(true);
    setConfirmClose(false);
  }, []);
  
  const handleClose = useCallback((hasUnsavedChanges?: boolean) => {
    if (hasUnsavedChanges) {
      setConfirmClose(true);
    } else {
      setOpen(false);
      setConfirmClose(false);
    }
  }, []);
  
  const confirmCloseDialog = useCallback(() => {
    setOpen(false);
    setConfirmClose(false);
  }, []);
  
  const cancelClose = useCallback(() => {
    setConfirmClose(false);
  }, []);
  
  return {
    open,
    confirmClose,
    handleOpen,
    handleClose,
    confirmCloseDialog,
    cancelClose,
    setOpen,
  };
}

export default useApiState;
