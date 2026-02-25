/**
 * Centralized logging service that respects environment settings.
 * In production, debug logs are suppressed and errors can be sent to monitoring services.
 */

type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LogEntry {
  level: LogLevel;
  message: string;
  timestamp: string;
  data?: unknown;
}

class Logger {
  private readonly isDevelopment = process.env.NODE_ENV === 'development';
  private readonly isTest = process.env.NODE_ENV === 'test';

  /**
   * Debug logging - only shows in development
   */
  debug(message: string, data?: unknown): void {
    if (this.isDevelopment) {
      console.debug(`[DEBUG] ${message}`, data !== undefined ? data : '');
    }
  }

  /**
   * Info logging - shows in development, can be configured for production
   */
  info(message: string, data?: unknown): void {
    if (this.isDevelopment || this.shouldLogInProduction('info')) {
      console.info(`[INFO] ${message}`, data !== undefined ? data : '');
    }
  }

  /**
   * Warning logging - always shows
   */
  warn(message: string, data?: unknown): void {
    console.warn(`[WARN] ${message}`, data !== undefined ? data : '');
  }

  /**
   * Error logging - always shows and can send to monitoring service
   */
  error(message: string, error?: unknown): void {
    console.error(`[ERROR] ${message}`, error !== undefined ? error : '');
    
    // In production, send to error tracking service
    if (!this.isDevelopment && !this.isTest) {
      this.sendToErrorTracking({ level: 'error', message, timestamp: new Date().toISOString(), data: error });
    }
  }

  /**
   * Log API request - useful for debugging
   */
  apiRequest(method: string, url: string, data?: unknown): void {
    this.debug(`API ${method.toUpperCase()} ${url}`, data);
  }

  /**
   * Log API response - useful for debugging
   */
  apiResponse(method: string, url: string, status: number, data?: unknown): void {
    if (status >= 400) {
      this.warn(`API ${method.toUpperCase()} ${url} returned ${status}`, data);
    } else {
      this.debug(`API ${method.toUpperCase()} ${url} returned ${status}`, data);
    }
  }

  /**
   * Log user action - useful for analytics
   */
  userAction(action: string, details?: Record<string, unknown>): void {
    this.debug(`User Action: ${action}`, details);
  }

  /**
   * Log performance metric
   */
  performance(metric: string, durationMs: number, details?: Record<string, unknown>): void {
    this.debug(`Performance: ${metric} took ${durationMs}ms`, details);
  }

  private shouldLogInProduction(level: LogLevel): boolean {
    // Can be configured via environment variable or feature flag
    const productionLogLevel = process.env.REACT_APP_LOG_LEVEL || 'warn';
    const levels: LogLevel[] = ['debug', 'info', 'warn', 'error'];
    return levels.indexOf(level) >= levels.indexOf(productionLogLevel as LogLevel);
  }

  private sendToErrorTracking(entry: LogEntry): void {
    // Placeholder for error tracking service integration (e.g., Sentry, LogRocket)
    // In a real implementation, this would send to an external service
    // Example: Sentry.captureException(entry.data);
    
    // For now, just ensure errors are captured
    if (typeof window !== 'undefined' && 'Sentry' in window) {
      // Sentry integration would go here
      type WindowWithSentry = Window & typeof globalThis & {
        Sentry?: { captureMessage?: (msg: string, opts?: object) => void };
      };
      (window as WindowWithSentry).Sentry?.captureMessage?.(entry.message, { extra: entry });
    }
  }
}

export const logger = new Logger();
export default logger;
