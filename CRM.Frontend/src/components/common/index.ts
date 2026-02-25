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

export { default as AddressModalComponent } from './AddressModalComponent';
export type { AddressModalComponentProps } from './AddressModalComponent';

export { default as AddressFormComponent } from './AddressFormComponent';
export type { AddressFormComponentProps } from './AddressFormComponent';

export { default as AddressListComponent } from './AddressListComponent';
export type { AddressListComponentProps } from './AddressListComponent';

// Logo Display for branding
export { default as LogoDisplay } from './LogoDisplay';

// Chat Timeline Item for displaying chat messages in activity timeline
export { default as ChatTimelineItem } from './ChatTimelineItem';
export type { ChatMessageActivity } from './ChatTimelineItem';

// Analytics Embed for embedding Superset/Power BI dashboards
export { default as AnalyticsEmbed } from './AnalyticsEmbed';

// Accessible DataGrid with keyboard navigation
export { DataGrid } from './DataGrid';
export type { DataGridProps, DataGridColumn, SortOrder, PaginationInfo } from './DataGrid';

// Accessible SearchBar with suggestions
export { SearchBar } from './SearchBar';
export type { SearchBarProps, SearchSuggestion } from './SearchBar';

// Inline editing cell for data grids
export { EditableCell } from './EditableCell';
export type { EditableCellProps, CellType, EditableCellValue, CellValidation } from './EditableCell';

// Bulk action toolbar for list views
export { BulkActionToolbar, createDeleteAction, createExportAction } from './BulkActionToolbar';
export type { BulkActionToolbarProps, BulkAction } from './BulkActionToolbar';

// Advanced filter builder for complex queries  
export { AdvancedFilterBuilder } from './AdvancedFilterBuilder';
export type { 
  AdvancedFilterBuilderProps, 
  FilterField, 
  FilterCondition, 
  FilterGroup, 
  SavedFilter,
  LogicalOperator 
} from './AdvancedFilterBuilder';

// Split view for side-by-side record comparison
export { SplitView } from './SplitView';
export type { SplitViewProps, SplitOrientation } from './SplitView';

// Customizable sidebar with drag-drop reordering
export { CustomizableSidebar } from './CustomizableSidebar';
export type { 
  CustomizableSidebarProps, 
  SidebarSection, 
  SidebarItem, 
  SidebarPreferences 
} from './CustomizableSidebar';

// Recent items component and context
export { RecentItems } from './RecentItems';
export type { RecentItemsProps } from './RecentItems';

// Recent items dropdown for AppBar quick access
export { RecentItemsDropdown } from './RecentItemsDropdown';
export type { RecentItemsDropdownProps } from './RecentItemsDropdown';
