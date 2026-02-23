// Hooks - Barrel Export
export { useApiState } from './useApiState';
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
