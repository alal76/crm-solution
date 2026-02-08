// Common Components - Barrel Export
export { TabPanel, a11yProps } from './TabPanel';
export type { TabPanelProps } from './TabPanel';

export { ConfirmDialog, useConfirmDialog } from './ConfirmDialog';
export type { ConfirmDialogProps } from './ConfirmDialog';

export { LoadingSpinner } from './LoadingSpinner';
export type { LoadingSpinnerProps } from './LoadingSpinner';

export { ErrorAlert, SuccessAlert, ValidationErrorAlert } from './ErrorAlert';
export type { ErrorAlertProps, SuccessAlertProps, ValidationErrorAlertProps } from './ErrorAlert';

export { 
  DialogError,
  DialogSuccess,
  LoadingOverlay,
  ActionButton,
  StatusSnackbar,
  EmptyState,
  InlineError,
} from './StatusComponents';
export type {
  DialogErrorProps,
  DialogSuccessProps,
  LoadingOverlayProps,
  ActionButtonProps,
  StatusSnackbarProps,
  EmptyStateProps,
  InlineErrorProps,
} from './StatusComponents';

export { ConcurrencyConflictDialog } from './ConcurrencyConflictDialog';
export type { ConflictData } from './ConcurrencyConflictDialog';

export { UserEditingIndicator, UserEditingAvatars } from './UserEditingIndicator';

// Related Entities Panel for displaying linked records
export { RelatedEntitiesPanel } from './RelatedEntitiesPanel';
export type { RelatedEntitiesPanelProps, RelatedEntityType } from './RelatedEntitiesPanel';

// Enhanced Empty State with illustrations
export { EnhancedEmptyState } from './EnhancedEmptyState';
export type { EnhancedEmptyStateProps, EmptyStateVariant, EntityIllustration } from './EnhancedEmptyState';

// Dialog Header for consistent CRUD dialog headers
export { DialogHeader } from './DialogHeader';
export type { DialogHeaderProps, DialogMode, DialogEntityType } from './DialogHeader';

// Chat Timeline Item for displaying chat messages in activity timeline
export { default as ChatTimelineItem } from './ChatTimelineItem';
export type { ChatMessageActivity } from './ChatTimelineItem';

// Analytics Embed for embedding Superset/Power BI dashboards
export { default as AnalyticsEmbed } from './AnalyticsEmbed';
