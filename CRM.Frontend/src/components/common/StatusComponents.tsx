import React from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Collapse,
  IconButton,
  LinearProgress,
  Snackbar,
  Typography,
} from '@mui/material';
import {
  Close as CloseIcon,
  Error as ErrorIcon,
  CheckCircle as SuccessIcon,
  Info as InfoIcon,
  Warning as WarningIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { ApiError } from '../../hooks/useApiState';

/**
 * Props for DialogError component
 */
export interface DialogErrorProps {
  /** Error object or string to display */
  error: ApiError | string | null;
  /** Callback when error is dismissed */
  onClose?: () => void;
  /** Show retry button */
  onRetry?: () => void;
  /** Additional styles */
  sx?: object;
}

/**
 * Error display component for dialogs.
 * Shows error message within a dialog, keeping dialog open until user dismisses.
 */
export const DialogError: React.FC<DialogErrorProps> = ({
  error,
  onClose,
  onRetry,
  sx = {},
}) => {
  if (!error) return null;
  
  const message = typeof error === 'string' ? error : error.message;
  const details = typeof error === 'object' ? error.details : undefined;
  
  return (
    <Collapse in={!!error}>
      <Alert
        severity="error"
        sx={{ mb: 2, ...sx }}
        action={
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            {onRetry && (
              <IconButton
                size="small"
                color="inherit"
                onClick={onRetry}
                title="Retry"
              >
                <RefreshIcon fontSize="small" />
              </IconButton>
            )}
            {onClose && (
              <IconButton
                size="small"
                color="inherit"
                onClick={onClose}
              >
                <CloseIcon fontSize="small" />
              </IconButton>
            )}
          </Box>
        }
      >
        <Typography variant="body2" fontWeight={500}>
          {message}
        </Typography>
        {details && Object.keys(details).length > 0 && (
          <Box component="ul" sx={{ mt: 1, mb: 0, pl: 2 }}>
            {Object.entries(details).map(([field, errors]) => (
              <Box component="li" key={field}>
                <Typography variant="caption">
                  <strong>{field}:</strong> {Array.isArray(errors) ? errors.join(', ') : String(errors)}
                </Typography>
              </Box>
            ))}
          </Box>
        )}
      </Alert>
    </Collapse>
  );
};

/**
 * Props for DialogSuccess component
 */
export interface DialogSuccessProps {
  /** Success message to display */
  message: string | null;
  /** Callback when dismissed */
  onClose?: () => void;
  /** Additional styles */
  sx?: object;
}

/**
 * Success message display for dialogs
 */
export const DialogSuccess: React.FC<DialogSuccessProps> = ({
  message,
  onClose,
  sx = {},
}) => {
  if (!message) return null;
  
  return (
    <Collapse in={!!message}>
      <Alert
        severity="success"
        sx={{ mb: 2, ...sx }}
        onClose={onClose}
      >
        {message}
      </Alert>
    </Collapse>
  );
};

/**
 * Props for LoadingOverlay component
 */
export interface LoadingOverlayProps {
  /** Whether loading is active */
  loading: boolean;
  /** Loading message */
  message?: string;
  /** Use linear progress instead of circular */
  linear?: boolean;
  /** Opacity of overlay background */
  opacity?: number;
}

/**
 * Loading overlay for dialogs and cards
 */
export const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  loading,
  message,
  linear = false,
  opacity = 0.7,
}) => {
  if (!loading) return null;
  
  return (
    <Box
      sx={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: `rgba(255, 255, 255, ${opacity})`,
        zIndex: 1000,
      }}
    >
      {linear ? (
        <Box sx={{ width: '60%' }}>
          <LinearProgress />
          {message && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1, textAlign: 'center' }}>
              {message}
            </Typography>
          )}
        </Box>
      ) : (
        <>
          <CircularProgress />
          {message && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              {message}
            </Typography>
          )}
        </>
      )}
    </Box>
  );
};

/**
 * Props for ActionButton component
 */
export interface ActionButtonProps {
  /** Button label (can also use children) */
  label?: string;
  /** Button content (alternative to label) */
  children?: React.ReactNode;
  /** Loading state */
  loading?: boolean;
  /** Disabled state */
  disabled?: boolean;
  /** Click handler */
  onClick: () => void;
  /** Button variant */
  variant?: 'text' | 'contained' | 'outlined';
  /** Button color */
  color?: 'inherit' | 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning';
  /** Start icon */
  startIcon?: React.ReactNode;
  /** Full width */
  fullWidth?: boolean;
  /** Size */
  size?: 'small' | 'medium' | 'large';
  /** Additional styles */
  sx?: object;
}

/**
 * Button with integrated loading state
 */
export const ActionButton: React.FC<ActionButtonProps> = ({
  label,
  children,
  loading = false,
  disabled = false,
  onClick,
  variant = 'contained',
  color = 'primary',
  startIcon,
  fullWidth = false,
  size = 'medium',
  sx = {},
}) => {
  const buttonContent = children || label || 'Submit';
  
  return (
    <Button
      variant={variant}
      color={color}
      onClick={onClick}
      disabled={disabled || loading}
      startIcon={loading ? <CircularProgress size={18} color="inherit" /> : startIcon}
      fullWidth={fullWidth}
      size={size}
      sx={sx}
    >
      {loading ? 'Please wait...' : buttonContent}
    </Button>
  );
};

/**
 * Props for StatusSnackbar component
 */
export interface StatusSnackbarProps {
  /** Error message */
  error?: string | null;
  /** Success message */
  success?: string | null;
  /** Info message */
  info?: string | null;
  /** Warning message */
  warning?: string | null;
  /** Callback to clear error */
  onCloseError?: () => void;
  /** Callback to clear success */
  onCloseSuccess?: () => void;
  /** Callback to clear info */
  onCloseInfo?: () => void;
  /** Callback to clear warning */
  onCloseWarning?: () => void;
  /** Auto-hide duration in ms (default: 5000) */
  autoHideDuration?: number;
}

/**
 * Snackbar for showing status messages at the bottom of the screen
 */
export const StatusSnackbar: React.FC<StatusSnackbarProps> = ({
  error,
  success,
  info,
  warning,
  onCloseError,
  onCloseSuccess,
  onCloseInfo,
  onCloseWarning,
  autoHideDuration = 5000,
}) => {
  // Determine which message to show (priority: error > warning > success > info)
  const activeMessage = error || warning || success || info;
  const severity = error ? 'error' : warning ? 'warning' : success ? 'success' : 'info';
  const onClose = error ? onCloseError : warning ? onCloseWarning : success ? onCloseSuccess : onCloseInfo;
  
  return (
    <Snackbar
      open={!!activeMessage}
      autoHideDuration={autoHideDuration}
      onClose={() => onClose?.()}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
    >
      <Alert
        severity={severity}
        onClose={() => onClose?.()}
        sx={{ width: '100%' }}
      >
        {activeMessage}
      </Alert>
    </Snackbar>
  );
};

/**
 * Props for EmptyState component
 */
export interface EmptyStateProps {
  /** Title */
  title?: string;
  /** Description */
  description?: string;
  /** Icon */
  icon?: React.ReactNode;
  /** Action button label */
  actionLabel?: string;
  /** Action button click handler */
  onAction?: () => void;
}

/**
 * Empty state display for lists with no data
 */
export const EmptyState: React.FC<EmptyStateProps> = ({
  title = 'No data found',
  description = 'There are no items to display.',
  icon = <InfoIcon sx={{ fontSize: 48, color: 'text.disabled' }} />,
  actionLabel,
  onAction,
}) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: 8,
        px: 2,
      }}
    >
      {icon}
      <Typography variant="h6" color="text.secondary" sx={{ mt: 2 }}>
        {title}
      </Typography>
      <Typography variant="body2" color="text.disabled" sx={{ mt: 1, textAlign: 'center' }}>
        {description}
      </Typography>
      {actionLabel && onAction && (
        <Button
          variant="outlined"
          onClick={onAction}
          sx={{ mt: 3 }}
        >
          {actionLabel}
        </Button>
      )}
    </Box>
  );
};

/**
 * Props for InlineError component
 */
export interface InlineErrorProps {
  /** Error message */
  error: string | null;
  /** Retry callback */
  onRetry?: () => void;
}

/**
 * Inline error display with optional retry button
 */
export const InlineError: React.FC<InlineErrorProps> = ({ error, onRetry }) => {
  if (!error) return null;
  
  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 2,
        py: 4,
      }}
    >
      <ErrorIcon color="error" />
      <Typography color="error">{error}</Typography>
      {onRetry && (
        <Button
          variant="outlined"
          color="error"
          size="small"
          startIcon={<RefreshIcon />}
          onClick={onRetry}
        >
          Retry
        </Button>
      )}
    </Box>
  );
};

export default DialogError;
