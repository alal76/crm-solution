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

// Concurrency control hook
export { useConcurrencyControl } from './useConcurrencyControl';
