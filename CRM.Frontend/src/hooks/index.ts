// Hooks - Barrel Export
export { useApiState } from './useApiState';
export { useLoadingState } from './useLoadingState';
export { usePagination } from './usePagination';
export { useFieldConfig } from './useFieldConfig';

// SignalR hooks for real-time updates
export {
  useRecordSubscription,
  useEntityTypeSubscription,
  useEditingNotification,
  useSignalRConnection,
} from './useSignalR';

// UI Customization hooks
export {
  useFeatureFlag,
  useFeatureFlagVariant,
  useDashboardCustomization,
} from './useUICustomization';

// SLA real-time countdown hooks
export { useSLACountdown, useSLADashboardUpdates } from './useSLACountdown';
export type { SLACountdown, SLABreachEvent, SLAWarningEvent } from './useSLACountdown';

// Password requirements hook (fetches backend policy)
export { usePasswordRequirements } from './usePasswordRequirements';
export type { PasswordRequirements, PasswordValidationResult, UsePasswordRequirementsReturn } from './usePasswordRequirements';

// Navigation preferences hook (persists sidebar state to localStorage)
export { useNavigationPreferences } from './useNavigationPreferences';
export type { NavigationPreferences, UseNavigationPreferencesReturn } from './useNavigationPreferences';

// Recent items hooks (re-exports from RecentItemsContext for convenience)
export { useRecentItems, useTrackRecentItem } from './useRecentItems';
export type { RecentItem, RecentItemType } from './useRecentItems';

// Font size preference hook (TODO-UX-05) — writes to both crm_font_size_preference and crm-font-size
export { useFontSize, FONT_SIZE_STORAGE_KEY } from './useFontSize';
export type { FontSize, UseFontSizeReturn } from './useFontSize';
