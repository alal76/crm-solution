/**
 * CRM Solution - Error Boundary Component
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * Catches JavaScript errors anywhere in the child component tree,
 * logs errors, and displays a fallback UI.
 */

import React, { Component, ErrorInfo as ReactErrorInfo, ReactNode } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Alert,
  AlertTitle,
  Chip,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Error as ErrorIcon,
  Refresh as RefreshIcon,
  Home as HomeIcon,
  BugReport as BugReportIcon,
} from '@mui/icons-material';

interface ComponentErrorInfo {
  componentStack: string;
}

interface ErrorLogEntry {
  timestamp: string;
  error: string;
  componentStack?: string;
  url: string;
  userAgent: string;
}

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
  onError?: (error: Error, errorInfo: ComponentErrorInfo) => void;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ComponentErrorInfo | null;
  errorLogs: ErrorLogEntry[];
}

// Global error logs storage
const globalErrorLogs: ErrorLogEntry[] = [];

// Export function to get all logged errors
export const getErrorLogs = (): ErrorLogEntry[] => [...globalErrorLogs];

// Export function to clear error logs
export const clearErrorLogs = (): void => {
  globalErrorLogs.length = 0;
};

// Log error to console with formatting
const logErrorToConsole = (entry: ErrorLogEntry): void => {
  console.group(`%c🚨 Error Caught at ${entry.timestamp}`, 'color: red; font-weight: bold;');
  console.error('Error:', entry.error);
  if (entry.componentStack) {
    console.log('%cComponent Stack:', 'color: orange;', entry.componentStack);
  }
  console.log('%cURL:', 'color: blue;', entry.url);
  console.log('%cUser Agent:', 'color: gray;', entry.userAgent);
  console.groupEnd();
};

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
      errorLogs: [],
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo): void {
    const entry: ErrorLogEntry = {
      timestamp: new Date().toISOString(),
      error: `${error.name}: ${error.message}\n${error.stack || ''}`,
      componentStack: errorInfo.componentStack,
      url: window.location.href,
      userAgent: navigator.userAgent,
    };

    // Store in global logs
    globalErrorLogs.push(entry);

    // Log to console
    logErrorToConsole(entry);

    // Store in local storage for persistence
    try {
      const existingLogs = JSON.parse(localStorage.getItem('crm_error_logs') || '[]');
      existingLogs.push(entry);
      // Keep only last 100 errors
      const trimmedLogs = existingLogs.slice(-100);
      localStorage.setItem('crm_error_logs', JSON.stringify(trimmedLogs));
    } catch (e) {
      console.warn('Failed to persist error log:', e);
    }

    this.setState({ errorInfo: errorInfo as unknown as ComponentErrorInfo, errorLogs: [...globalErrorLogs] });

    // Call custom error handler if provided
    if (this.props.onError) {
      this.props.onError(error, errorInfo as unknown as ComponentErrorInfo);
    }
  }

  handleRefresh = (): void => {
    window.location.reload();
  };

  handleGoHome = (): void => {
    window.location.href = '/';
  };

  handleRetry = (): void => {
    this.setState({ hasError: false, error: null, errorInfo: null });
  };

  handleCopyError = (): void => {
    const { error, errorInfo } = this.state;
    const errorText = `
Error: ${error?.name}: ${error?.message}

Stack Trace:
${error?.stack || 'N/A'}

Component Stack:
${errorInfo?.componentStack || 'N/A'}

URL: ${window.location.href}
Timestamp: ${new Date().toISOString()}
    `.trim();

    navigator.clipboard.writeText(errorText).then(() => {
      alert('Error details copied to clipboard');
    });
  };

  render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      const { error, errorInfo } = this.state;

      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '60vh',
            p: 3,
          }}
        >
          <Paper
            elevation={3}
            sx={{
              p: 4,
              maxWidth: 800,
              width: '100%',
              borderTop: '4px solid',
              borderColor: 'error.main',
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <ErrorIcon color="error" sx={{ fontSize: 48, mr: 2 }} />
              <Box>
                <Typography variant="h5" color="error" gutterBottom>
                  Something went wrong
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  An error occurred while rendering this component
                </Typography>
              </Box>
            </Box>

            <Alert severity="error" sx={{ mb: 3 }}>
              <AlertTitle>Error Details</AlertTitle>
              <Typography variant="body2" component="pre" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-all' }}>
                {error?.message || 'Unknown error'}
              </Typography>
            </Alert>

            <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
              <Button
                variant="contained"
                startIcon={<RefreshIcon />}
                onClick={this.handleRetry}
              >
                Try Again
              </Button>
              <Button
                variant="outlined"
                startIcon={<RefreshIcon />}
                onClick={this.handleRefresh}
              >
                Refresh Page
              </Button>
              <Button
                variant="outlined"
                startIcon={<HomeIcon />}
                onClick={this.handleGoHome}
              >
                Go to Home
              </Button>
              <Button
                variant="outlined"
                color="secondary"
                startIcon={<BugReportIcon />}
                onClick={this.handleCopyError}
              >
                Copy Error Details
              </Button>
            </Box>

            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography>Technical Details</Typography>
                  <Chip label="Debug" size="small" color="warning" />
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                <Typography variant="subtitle2" gutterBottom>
                  Stack Trace:
                </Typography>
                <Paper
                  variant="outlined"
                  sx={{
                    p: 2,
                    bgcolor: 'grey.100',
                    maxHeight: 200,
                    overflow: 'auto',
                    mb: 2,
                  }}
                >
                  <Typography
                    variant="body2"
                    component="pre"
                    sx={{
                      fontFamily: 'monospace',
                      fontSize: '0.75rem',
                      whiteSpace: 'pre-wrap',
                      wordBreak: 'break-all',
                      m: 0,
                    }}
                  >
                    {error?.stack || 'No stack trace available'}
                  </Typography>
                </Paper>

                {errorInfo?.componentStack && (
                  <>
                    <Typography variant="subtitle2" gutterBottom>
                      Component Stack:
                    </Typography>
                    <Paper
                      variant="outlined"
                      sx={{
                        p: 2,
                        bgcolor: 'grey.100',
                        maxHeight: 200,
                        overflow: 'auto',
                      }}
                    >
                      <Typography
                        variant="body2"
                        component="pre"
                        sx={{
                          fontFamily: 'monospace',
                          fontSize: '0.75rem',
                          whiteSpace: 'pre-wrap',
                          m: 0,
                        }}
                      >
                        {errorInfo.componentStack}
                      </Typography>
                    </Paper>
                  </>
                )}
              </AccordionDetails>
            </Accordion>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
