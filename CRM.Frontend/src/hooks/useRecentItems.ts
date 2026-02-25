/**
 * useRecentItems - Hook for tracking recently viewed records.
 * TODO-UX-15: Recent items quick access.
 *
 * This hook is a thin re-export of the RecentItemsContext hooks so they are
 * discoverable via the `hooks/` directory alongside the rest of the CRM hooks.
 *
 * Full implementation lives in `contexts/RecentItemsContext.tsx`.
 *
 * Usage:
 *   const { recentItems, addRecentItem, clearRecentItems } = useRecentItems();
 *   useTrackRecentItem({ id: account.id, type: 'account', title: account.name, path: `/accounts/${account.id}` });
 */

export { useRecentItems, useTrackRecentItem } from '../contexts/RecentItemsContext';
export type { RecentItem, RecentItemType } from '../contexts/RecentItemsContext';
