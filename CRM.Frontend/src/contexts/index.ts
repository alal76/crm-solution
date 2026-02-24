/**
 * Contexts - Barrel Export
 */

// Authentication context
export { AuthProvider, useAuth, AuthContext } from './AuthContext';

// Theme context for light/dark/system mode
export { ThemeContextProvider, useThemeContext } from './ThemeContext';

// Font size adjustment context
export { FontSizeProvider, useFontSize, FontSizeSelector } from './FontSizeContext';
export type { FontSize } from './FontSizeContext';

// Recent items tracking context
export { RecentItemsProvider, useRecentItems, useTrackRecentItem } from './RecentItemsContext';
export type { RecentItem, RecentItemType } from './RecentItemsContext';
