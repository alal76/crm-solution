import React from 'react';
import { Alert, AlertProps } from '@mui/material';

export interface ErrorAlertProps {
  /** Error message to display */
  error: string | null | undefined;
  /** Callback when alert is dismissed */
  onClose?: () => void;
  /** Whether the alert is dismissible (default: true if onClose is provided) */
  dismissible?: boolean;
  /** Custom sx styles */
  sx?: AlertProps['sx'];
  /** Alert severity (default: error) */
  severity?: 'error' | 'warning' | 'info' | 'success';
}

/**
 * Reusable error/alert component with consistent styling
 * Only renders when error is truthy
 */
export const ErrorAlert: React.FC<ErrorAlertProps> = ({
  error,
  onClose,
  dismissible = !!onClose,
  sx = { mb: 2 },
  severity = 'error',
}) => {
  if (!error) return null;

  return (
    <Alert 
      severity={severity} 
      sx={sx} 
      onClose={dismissible ? onClose : undefined}
    >
      {error}
    </Alert>
  );
};

export interface SuccessAlertProps {
  /** Success message to display */
  message: string | null | undefined;
  /** Callback when alert is dismissed */
  onClose?: () => void;
  /** Whether the alert is dismissible (default: true if onClose is provided) */
  dismissible?: boolean;
  /** Custom sx styles */
  sx?: AlertProps['sx'];
}

/**
 * Reusable success alert component
 * Only renders when message is truthy
 */
export const SuccessAlert: React.FC<SuccessAlertProps> = ({
  message,
  onClose,
  dismissible = !!onClose,
  sx = { mb: 2 },
}) => {
  if (!message) return null;

  return (
    <Alert 
      severity="success" 
      sx={sx} 
      onClose={dismissible ? onClose : undefined}
    >
      {message}
    </Alert>
  );
};

export interface ValidationErrorAlertProps {
  /** Error message to display (can be multi-line with validation details) */
  error: string | null | undefined;
  /** Callback when alert is dismissed */
  onClose?: () => void;
  /** Whether the alert is dismissible (default: true if onClose is provided) */
  dismissible?: boolean;
  /** Custom sx styles */
  sx?: AlertProps['sx'];
}

/**
 * Specialized error alert for validation errors
 * Displays multi-line error messages with proper formatting
 * Use with getApiErrorMessage() from errorHandler.ts
 */
export const ValidationErrorAlert: React.FC<ValidationErrorAlertProps> = ({
  error,
  onClose,
  dismissible = !!onClose,
  sx = { mb: 2 },
}) => {
  if (!error) return null;

  return (
    <Alert 
      severity="error" 
      sx={{ 
        ...sx as object,
        whiteSpace: 'pre-line',
        '& .MuiAlert-message': {
          whiteSpace: 'pre-line',
        }
      }} 
      onClose={dismissible ? onClose : undefined}
    >
      {error}
    </Alert>
  );
};

export default ErrorAlert;
