/**
 * CRM Solution - Global Error Handler
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * Captures and logs all JavaScript errors, promise rejections,
 * and console errors for debugging purposes.
 */

interface ErrorLogEntry {
  id: string;
  timestamp: string;
  type: 'error' | 'unhandledrejection' | 'console.error' | 'network' | 'component';
  message: string;
  stack?: string;
  source?: string;
  line?: number;
  column?: number;
  url: string;
  componentStack?: string;
  additionalInfo?: Record<string, unknown>;
}

interface DebugConfig {
  enabled: boolean;
  logToConsole: boolean;
  logToLocalStorage: boolean;
  maxStoredLogs: number;
  captureNetworkErrors: boolean;
  captureConsoleErrors: boolean;
  onError?: (entry: ErrorLogEntry) => void;
}

const defaultConfig: DebugConfig = {
  enabled: true,
  logToConsole: true,
  logToLocalStorage: true,
  maxStoredLogs: 200,
  captureNetworkErrors: true,
  captureConsoleErrors: true,
};

// Error logs storage
const errorLogs: ErrorLogEntry[] = [];
let config: DebugConfig = { ...defaultConfig };
let isInitialized = false;
const originalConsoleError = console.error;
const originalFetch = window.fetch;

// Generate unique ID for each error
const generateId = (): string => {
  return `err_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
};

// Create error log entry
const createLogEntry = (
  type: ErrorLogEntry['type'],
  message: string,
  extra?: Partial<ErrorLogEntry>
): ErrorLogEntry => {
  return {
    id: generateId(),
    timestamp: new Date().toISOString(),
    type,
    message,
    url: window.location.href,
    ...extra,
  };
};

// Log error entry
const logError = (entry: ErrorLogEntry): void => {
  if (!config.enabled) return;

  // Add to in-memory logs
  errorLogs.push(entry);

  // Trim if exceeded max
  while (errorLogs.length > config.maxStoredLogs) {
    errorLogs.shift();
  }

  // Log to console with formatting
  if (config.logToConsole) {
    const typeColors: Record<string, string> = {
      error: 'red',
      unhandledrejection: 'purple',
      'console.error': 'orange',
      network: 'blue',
      component: 'crimson',
    };

    console.group(
      `%c🔴 [${entry.type.toUpperCase()}] ${entry.timestamp}`,
      `color: ${typeColors[entry.type] || 'red'}; font-weight: bold;`
    );
    console.log('%cMessage:', 'font-weight: bold;', entry.message);
    if (entry.source) {
      console.log('%cSource:', 'color: gray;', `${entry.source}:${entry.line}:${entry.column}`);
    }
    if (entry.stack) {
      console.log('%cStack:', 'color: orange;', entry.stack);
    }
    if (entry.additionalInfo) {
      console.log('%cAdditional Info:', 'color: blue;', entry.additionalInfo);
    }
    console.groupEnd();
  }

  // Persist to localStorage
  if (config.logToLocalStorage) {
    try {
      const stored = JSON.parse(localStorage.getItem('crm_debug_logs') || '[]');
      stored.push(entry);
      while (stored.length > config.maxStoredLogs) {
        stored.shift();
      }
      localStorage.setItem('crm_debug_logs', JSON.stringify(stored));
    } catch (e) {
      // localStorage might be full or disabled
    }
  }

  // Call custom error handler
  if (config.onError) {
    config.onError(entry);
  }
};

// Global error handler
const handleGlobalError = (event: ErrorEvent): void => {
  const entry = createLogEntry('error', event.message, {
    stack: event.error?.stack,
    source: event.filename,
    line: event.lineno,
    column: event.colno,
  });
  logError(entry);
};

// Unhandled promise rejection handler
const handleUnhandledRejection = (event: PromiseRejectionEvent): void => {
  const reason = event.reason;
  const message = reason instanceof Error 
    ? reason.message 
    : String(reason);
  const stack = reason instanceof Error 
    ? reason.stack 
    : undefined;

  const entry = createLogEntry('unhandledrejection', message, { stack });
  logError(entry);
};

// Console error interceptor
const interceptConsoleError = (): void => {
  console.error = (...args: unknown[]) => {
    // Call original
    originalConsoleError.apply(console, args);

    if (!config.captureConsoleErrors) return;

    const message = args.map((arg) => {
      if (arg instanceof Error) {
        return `${arg.name}: ${arg.message}`;
      }
      return String(arg);
    }).join(' ');

    const entry = createLogEntry('console.error', message, {
      additionalInfo: { args: args.map((a) => (a instanceof Error ? a.stack : a)) },
    });
    
    // Only log if not already being logged (avoid recursion)
    if (!message.includes('[DEBUG]')) {
      logError(entry);
    }
  };
};

// Fetch interceptor for network errors
const interceptFetch = (): void => {
  window.fetch = async (...args: Parameters<typeof fetch>): Promise<Response> => {
    const [input, init] = args;
    const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url;

    try {
      const response = await originalFetch.apply(window, args);

      if (!response.ok && config.captureNetworkErrors) {
        const entry = createLogEntry('network', `HTTP ${response.status}: ${response.statusText}`, {
          additionalInfo: {
            url,
            method: init?.method || 'GET',
            status: response.status,
            statusText: response.statusText,
          },
        });
        logError(entry);
      }

      return response;
    } catch (error) {
      if (config.captureNetworkErrors) {
        const entry = createLogEntry('network', `Network request failed: ${error instanceof Error ? error.message : String(error)}`, {
          stack: error instanceof Error ? error.stack : undefined,
          additionalInfo: {
            url,
            method: init?.method || 'GET',
          },
        });
        logError(entry);
      }
      throw error;
    }
  };
};

// Initialize the global error handler
export const initializeErrorHandler = (customConfig?: Partial<DebugConfig>): void => {
  if (isInitialized) {
    console.warn('[DEBUG] Error handler already initialized');
    return;
  }

  config = { ...defaultConfig, ...customConfig };

  // Add global error listeners
  window.addEventListener('error', handleGlobalError);
  window.addEventListener('unhandledrejection', handleUnhandledRejection);

  // Intercept console.error
  if (config.captureConsoleErrors) {
    interceptConsoleError();
  }

  // Intercept fetch
  if (config.captureNetworkErrors) {
    interceptFetch();
  }

  isInitialized = true;

  console.log(
    '%c🔧 Debug mode enabled - All errors will be captured and logged',
    'color: green; font-weight: bold;'
  );
  console.log('%cConfig:', 'color: blue;', config);
};

// Get all error logs
export const getErrorLogs = (): ErrorLogEntry[] => [...errorLogs];

// Get logs from localStorage (persisted across sessions)
export const getPersistedLogs = (): ErrorLogEntry[] => {
  try {
    return JSON.parse(localStorage.getItem('crm_debug_logs') || '[]');
  } catch {
    return [];
  }
};

// Clear all logs
export const clearErrorLogs = (): void => {
  errorLogs.length = 0;
  localStorage.removeItem('crm_debug_logs');
  console.log('%c🧹 Error logs cleared', 'color: green;');
};

// Export logs as JSON file
export const exportLogsAsJson = (): void => {
  const logs = getErrorLogs();
  const blob = new Blob([JSON.stringify(logs, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `crm_error_logs_${new Date().toISOString().replace(/[:.]/g, '-')}.json`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
};

// Get summary of errors
export const getErrorSummary = (): {
  total: number;
  byType: Record<string, number>;
  lastError?: ErrorLogEntry;
} => {
  const byType: Record<string, number> = {};
  
  for (const log of errorLogs) {
    byType[log.type] = (byType[log.type] || 0) + 1;
  }

  return {
    total: errorLogs.length,
    byType,
    lastError: errorLogs[errorLogs.length - 1],
  };
};

// Log component error (for ErrorBoundary integration)
export const logComponentError = (
  error: Error,
  componentStack?: string,
  componentName?: string
): void => {
  const entry = createLogEntry('component', error.message, {
    stack: error.stack,
    componentStack,
    additionalInfo: { componentName },
  });
  logError(entry);
};

// Update configuration at runtime
export const updateConfig = (newConfig: Partial<DebugConfig>): void => {
  config = { ...config, ...newConfig };
  console.log('%c🔧 Debug config updated:', 'color: blue;', config);
};

// Check if debug mode is enabled
export const isDebugEnabled = (): boolean => config.enabled;

// Disable error handler
export const disableErrorHandler = (): void => {
  window.removeEventListener('error', handleGlobalError);
  window.removeEventListener('unhandledrejection', handleUnhandledRejection);
  console.error = originalConsoleError;
  window.fetch = originalFetch;
  config.enabled = false;
  console.log('%c🔴 Debug mode disabled', 'color: red;');
};

// Create a UI debug panel (for development)
export const createDebugPanel = (): void => {
  const existingPanel = document.getElementById('crm-debug-panel');
  if (existingPanel) {
    existingPanel.remove();
    return;
  }

  const panel = document.createElement('div');
  panel.id = 'crm-debug-panel';
  panel.style.cssText = `
    position: fixed;
    bottom: 10px;
    right: 10px;
    width: 400px;
    max-height: 300px;
    overflow: auto;
    background: rgba(0, 0, 0, 0.9);
    color: #fff;
    font-family: monospace;
    font-size: 12px;
    padding: 10px;
    border-radius: 8px;
    z-index: 99999;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
  `;

  const header = document.createElement('div');
  header.innerHTML = `
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;">
      <strong>🐛 Debug Panel</strong>
      <div>
        <button id="debug-clear" style="margin-right: 5px; cursor: pointer;">Clear</button>
        <button id="debug-export" style="margin-right: 5px; cursor: pointer;">Export</button>
        <button id="debug-close" style="cursor: pointer;">×</button>
      </div>
    </div>
  `;
  panel.appendChild(header);

  const content = document.createElement('div');
  content.id = 'debug-content';
  panel.appendChild(content);

  document.body.appendChild(panel);

  // Update content
  const updateContent = (): void => {
    const logs = getErrorLogs();
    content.innerHTML = logs.length === 0
      ? '<div style="color: #888;">No errors logged</div>'
      : logs.slice(-10).reverse().map((log) => `
          <div style="margin-bottom: 8px; padding: 5px; background: rgba(255,0,0,0.1); border-radius: 4px;">
            <div style="color: ${log.type === 'error' ? '#ff6b6b' : log.type === 'network' ? '#4dabf7' : '#ffd43b'};">
              [${log.type}] ${new Date(log.timestamp).toLocaleTimeString()}
            </div>
            <div style="word-break: break-all;">${log.message.substring(0, 100)}${log.message.length > 100 ? '...' : ''}</div>
          </div>
        `).join('');
  };

  updateContent();

  // Event listeners
  document.getElementById('debug-close')?.addEventListener('click', () => panel.remove());
  document.getElementById('debug-clear')?.addEventListener('click', () => {
    clearErrorLogs();
    updateContent();
  });
  document.getElementById('debug-export')?.addEventListener('click', exportLogsAsJson);

  // Auto-update every second
  setInterval(updateContent, 1000);
};

// Expose to window for debugging
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).CRMDebug = {
    getErrorLogs,
    getPersistedLogs,
    clearErrorLogs,
    exportLogsAsJson,
    getErrorSummary,
    updateConfig,
    createDebugPanel,
    isDebugEnabled,
    disableErrorHandler,
  };
}

/**
 * Parse API validation errors into user-friendly messages
 * Handles .NET ValidationProblemDetails format and other common error formats
 */
export interface ParsedApiError {
  message: string;
  fieldErrors: { field: string; messages: string[] }[];
  hasFieldErrors: boolean;
}

export const parseApiError = (error: any, defaultMessage = 'An error occurred'): ParsedApiError => {
  const result: ParsedApiError = {
    message: defaultMessage,
    fieldErrors: [],
    hasFieldErrors: false,
  };

  if (!error) return result;

  const responseData = error.response?.data || error.data || error;

  // Handle .NET ValidationProblemDetails format
  // { "errors": { "fieldName": ["error1", "error2"], "$.jsonPath": ["error"] } }
  if (responseData?.errors && typeof responseData.errors === 'object') {
    const errors = responseData.errors;
    
    for (const [field, messages] of Object.entries(errors)) {
      if (Array.isArray(messages) && messages.length > 0) {
        // Clean up field names: remove $ prefix, camelCase to readable
        let cleanField = field
          .replace(/^\$\.?/, '') // Remove leading $. or $
          .replace(/([A-Z])/g, ' $1') // Add space before capitals
          .replace(/^./, str => str.toUpperCase()) // Capitalize first letter
          .trim();
        
        // Handle special cases
        if (cleanField.toLowerCase() === 'dto') {
          cleanField = 'Request Data';
        }
        
        result.fieldErrors.push({
          field: cleanField,
          messages: messages.map(m => String(m)),
        });
      }
    }
    
    result.hasFieldErrors = result.fieldErrors.length > 0;
    
    // Build a comprehensive error message
    if (result.hasFieldErrors) {
      const errorParts = result.fieldErrors.map(fe => {
        const fieldName = fe.field || 'Unknown field';
        const msgs = fe.messages.map(m => {
          // Make common error messages more user-friendly
          return m
            .replace(/The JSON value could not be converted to [^.]+\./g, 'Invalid format.')
            .replace(/is required/gi, 'is required')
            .replace(/must be/gi, 'must be')
            .replace(/cannot be/gi, 'cannot be');
        }).join('; ');
        return `${fieldName}: ${msgs}`;
      });
      result.message = `Validation failed:\n• ${errorParts.join('\n• ')}`;
    }
    
    // Use title from ValidationProblemDetails if available
    if (responseData.title && !result.hasFieldErrors) {
      result.message = responseData.title;
    }
    
    // Include detail if available
    if (responseData.detail) {
      result.message = result.hasFieldErrors 
        ? result.message 
        : responseData.detail;
    }
    
    return result;
  }

  // Handle simple message format: { "message": "error text" }
  if (responseData?.message) {
    result.message = responseData.message;
    return result;
  }

  // Handle .NET ProblemDetails format: { "title": "...", "detail": "..." }
  if (responseData?.title) {
    result.message = responseData.detail || responseData.title;
    return result;
  }

  // Handle array of errors: ["error1", "error2"]
  if (Array.isArray(responseData)) {
    result.message = responseData.map(e => String(e)).join('; ');
    return result;
  }

  // Handle string error
  if (typeof responseData === 'string') {
    result.message = responseData;
    return result;
  }

  // Handle Axios error message
  if (error.message) {
    result.message = error.message;
    
    // Add HTTP status info if available
    if (error.response?.status) {
      const statusText = error.response.statusText || getHttpStatusText(error.response.status);
      result.message = `${statusText}: ${result.message}`;
    }
  }

  return result;
};

/**
 * Get user-friendly error message from an API error
 */
export const getApiErrorMessage = (error: any, defaultMessage = 'An error occurred'): string => {
  return parseApiError(error, defaultMessage).message;
};

/**
 * Get HTTP status text for common status codes
 */
const getHttpStatusText = (status: number): string => {
  const statusTexts: Record<number, string> = {
    400: 'Bad Request',
    401: 'Unauthorized',
    403: 'Forbidden',
    404: 'Not Found',
    409: 'Conflict',
    422: 'Validation Error',
    500: 'Server Error',
    502: 'Bad Gateway',
    503: 'Service Unavailable',
  };
  return statusTexts[status] || `Error ${status}`;
};

export default {
  initializeErrorHandler,
  getErrorLogs,
  getPersistedLogs,
  clearErrorLogs,
  exportLogsAsJson,
  getErrorSummary,
  logComponentError,
  updateConfig,
  isDebugEnabled,
  disableErrorHandler,
  createDebugPanel,
  parseApiError,
  getApiErrorMessage,
};
